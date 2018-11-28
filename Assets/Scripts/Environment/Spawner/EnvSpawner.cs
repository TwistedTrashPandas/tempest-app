using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MastersOfTempest.Networking;
using static MastersOfTempest.EnvironmentNetwork;

namespace MastersOfTempest.Environment.Interacting
{
    public class EnvSpawner : MonoBehaviour
    {
        public enum EnvObjectType
        {
            Damaging,
            DangerZone,
            Supporting
        };

        // spawn parameters 
        public float spawnRate;
        public float damping_factor;

        public VectorField vectorField;
        // all envObjects treated the same
        public List<EnvObject> envObjects { get; private set; }
        // prefab lists
        public GameObject[] damagingPrefabs;
        public GameObject[] supportingPrefabs;
        public GameObject[] dangerzonesPrefabs;

        // TODO use currServerTime for synchronization 
        private float currServerTime;
        private float currFixedTime;
        private float hz;
        private Gamemaster gamemaster;
        private GameObject objectContainer;
        // last obj positions (extrapolate or interpolate ?)
        private List<Transform> envObjTransforms;

        public void Initialize(Gamemaster gm, VectorField vf)
        {
            envObjects = new List<EnvObject>();
            objectContainer = new GameObject("EnvObjectContainer");
            // test initialization for networking
            if (GetComponent<ServerObject>().onServer)
            {
                vectorField = vf;
                for (int i = 0; i < 100; i++)
                    InstantiateNewObject(true, new Vector3(), Vector3.one, Quaternion.identity, EnvObjectType.Damaging, 0, 0);
            }
            else
            {
                envObjTransforms = new List<Transform>();
            }
            hz = 1.0f / GameServer.Instance.hz;
            currServerTime = 0f;
            gamemaster = gm;
        }


        private void FixedUpdate()
        {
            // TODO: multiple implementations for behaviour
            // update all objects' velocity or add force by looking up value in vector grid if the spawner is on the server
            if (GetComponent<ServerObject>().onServer)
            {
                for (int i = 0; i < envObjects.Count; i++)
                {
                    envObjects[i].AddForce(vectorField.GetVectorAtPos(envObjects[i].transform.position), new Vector3());
                    //envObjects[i].SetVelocity(vectorField.GetVectorAtPos(envObjects[i].transform.position));
                    //envObjects[i].DampVelocity(damping_factor);
                }
            }
            else
            {
                currFixedTime += Time.fixedDeltaTime;
                if (currFixedTime >= hz)
                    currFixedTime -= hz;
                for (int i = 0; i < envObjects.Count; i++)
                {
                    envObjects[i].gameObject.transform.position = Vector3.Lerp(envObjects[i].gameObject.transform.position, envObjTransforms[i].position, currFixedTime);
                    envObjects[i].gameObject.transform.rotation = Quaternion.Lerp(envObjects[i].gameObject.transform.rotation, envObjTransforms[i].rotation, currFixedTime);
                }
            }
        }

        // main functions for initializing an envobject of given type 
        private void InstantiateNewObject(bool onServer, Vector3 position, Vector3 localScale, Quaternion orientation, EnvObjectType type = EnvObjectType.Damaging, int ID = 0, int prefabNum = 0)
        {
            switch (type)
            {
                case EnvObjectType.Damaging:
                    envObjects.Add(GameObject.Instantiate(damagingPrefabs[prefabNum], position, orientation).GetComponent<EnvObject>());
                    break;
                case EnvObjectType.DangerZone:
                    envObjects.Add(GameObject.Instantiate(dangerzonesPrefabs[prefabNum], position, orientation).GetComponent<EnvObject>());
                    break;
                case EnvObjectType.Supporting:
                    envObjects.Add(GameObject.Instantiate(supportingPrefabs[prefabNum], position, orientation).GetComponent<EnvObject>());
                    break;
            }
            envObjects[envObjects.Count - 1].transform.parent = objectContainer.transform;
            envObjects[envObjects.Count - 1].transform.localScale = localScale;
            if (!onServer)
            {
                //  set layer, instanceID (for deleting/updating objects) only for clients
                envObjects[envObjects.Count - 1].gameObject.layer = 9;
                envObjects[envObjects.Count - 1].instanceID = ID;
                //  disable unnecessary components
                Destroy(envObjects[envObjects.Count - 1].GetComponent<Rigidbody>());
                Destroy(envObjects[envObjects.Count - 1].GetComponent<Collider>());
                envObjTransforms.Add(envObjects[envObjects.Count - 1].transform);
            }
            else
            {
                // prefabnumber only important for client to choose correct prefab for initialization
                envObjects[envObjects.Count - 1].prefabNum = prefabNum;
            }
        }

        private void UpdateTransform(MessageEnvObject obj, int idx)
        {
            /*envObjects[idx].transform.position = obj.position;
            envObjects[idx].transform.localScale = obj.localScale;
            envObjects[idx].transform.rotation = obj.orientation;*/
            envObjTransforms[idx].position = obj.position;
            envObjTransforms[idx].rotation = obj.orientation;
        }

        public void UpdateEnvObjects(List<MessageEnvObject> objects, float serverTime)
        {
            print(Time.fixedTime - serverTime);
            currServerTime = serverTime;
            for (int i = 0; i < objects.Count; i++)
            {
                //  less objects on client than on server currently -> create new object
                if (envObjects.Count <= i)
                {
                    InstantiateNewObject(false, objects[i].position, objects[i].localScale, objects[i].orientation, objects[i].type, objects[i].instanceID, objects[i].prefabNum);
                }
                else
                {
                    // object already present -> update transform
                    if (envObjects[i].instanceID == objects[i].instanceID)
                    {
                        // TODO: interpolate for positions (smoother)
                        UpdateTransform(objects[i], i);
                    }
                    // object got destroyed on server -> destroy on all clients
                    else
                    {
                        GameObject toDestroy = envObjects[i].gameObject;
                        envObjTransforms.RemoveAt(i);
                        envObjects.RemoveAt(i);
                        print("removed on client");
                        Destroy(toDestroy);
                        i--;
                    }
                }
            }
        }
    }
}