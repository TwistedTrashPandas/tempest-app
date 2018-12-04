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

        public enum MoveType
        {
            Force,
            Velocity,
            Direct
        };

        // spawn parameters 
        public float spawnRate;
        [Range(0f, 2f)]
        public float damping_factor_vel;
        [Range(0f, 100f)]
        public float damping_factor_force;
        public Vector3 maximumObjVelocity;

        // for initializing a random target position around the ship
        public float minRadius;
        public float maxRadius;

        public uint maxNumObjects;
        public VectorField vectorField;
        // all envObjects treated the same
        public List<EnvObject> envObjects { get; private set; }
        // prefab lists
        public GameObject[] damagingPrefabs;
        public GameObject[] supportingPrefabs;
        public GameObject[] dangerzonesPrefabs;

        public MoveType moveType;

        // TODO use currServerTime for synchronization 
        private float currServerTime;
        private float currFixedTime;
        private float hz;
        private bool onServer;
        private Gamemaster gamemaster;
        private GameObject objectContainer;
        // last obj positions (extrapolate or interpolate ?)
        private List<Transform> envObjTransforms;

        public void Initialize(Gamemaster gm, VectorField vf, bool server)
        {
            envObjects = new List<EnvObject>();
            objectContainer = new GameObject("EnvObjectContainer");
            onServer = server;
            // test initialization for networking
            if (onServer)
            {
                vectorField = vf;
                hz = 1.0f / GameServer.Instance.hz;
                StartSpawning();
            }
            else
            {
                envObjTransforms = new List<Transform>();
                hz = 1f / 64f;
            }
            currServerTime = 0f;
            gamemaster = gm;
        }

        private void FixedUpdate()
        {
            // TODO: multiple implementations for behaviour
            // update all objects' velocity or add force by looking up value in vector grid if the spawner is on the server
            if (onServer)
            {
                switch (moveType)
                {
                    case MoveType.Direct:
                        MoveAllDirectly();
                        break;
                    case MoveType.Force:
                        AddForceToAll();
                        break;
                    case MoveType.Velocity:
                        SetVelForAll();
                        break;
                    default:
                        throw new System.InvalidOperationException("MoveType of Environment Spawner has to be set");
                }
                if (!Mathf.Approximately(damping_factor_force, 0f))
                    DampForce();
                if (!Mathf.Approximately(damping_factor_vel, 1f))
                    DampVelocity();
            }
            /*else
            {
                // "interpolate" objects on client
                currFixedTime += Time.fixedDeltaTime;
                if (currFixedTime >= hz)
                    currFixedTime -= hz;
                for (int i = 0; i < envObjects.Count; i++)
                {
                    envObjects[i].MoveRigidbodyTo(Vector3.Lerp(envObjects[i].gameObject.transform.position, envObjTransforms[i].position, currFixedTime));
                    envObjects[i].RotateRigidbodyTo(Quaternion.Lerp(envObjects[i].gameObject.transform.rotation, envObjTransforms[i].rotation, currFixedTime));
                }
            }*/
        }

        public void StartSpawning()
        {
            StartCoroutine(SpawnObject());
        }

        private IEnumerator SpawnObject()
        {
            if (envObjects.Count > maxNumObjects)
                envObjects.RemoveAt(0);
            InstantiateNewObject(true, new Vector3(), Vector3.one, Quaternion.identity, EnvObjectType.Damaging, 0, 0);
            yield return new WaitForSeconds(spawnRate);
            StartCoroutine(SpawnObject());
        }

        private void MoveAllDirectly()
        {
            Vector3 targetPos = gamemaster.GetShip().transform.position;
            for (int i = 0; i < envObjects.Count; i++)
            {
                envObjects[i].MoveDirectly(targetPos);
            }
        }

        private void AddForceToAll()
        {
            for (int i = 0; i < envObjects.Count; i++)
            {
                envObjects[i].AddForce(vectorField.GetVectorAtPos(envObjects[i].transform.position), new Vector3());
            }
        }

        private void SetVelForAll()
        {
            for (int i = 0; i < envObjects.Count; i++)
            {
                envObjects[i].SetVelocity(vectorField.GetVectorAtPos(envObjects[i].transform.position));
            }
        }

        private void DampVelocity()
        {
            for (int i = 0; i < envObjects.Count; i++)
            {
                envObjects[i].DampVelocity(damping_factor_vel);
            }
        }

        private void DampForce()
        {
            for (int i = 0; i < envObjects.Count; i++)
            {
                envObjects[i].DampForce(damping_factor_force);
            }
        }

        private void ClampVelocity()
        {

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
            //envObjects[envObjects.Count - 1].transform.parent = objectContainer.transform;
            envObjects[envObjects.Count - 1].transform.localScale = localScale;
            float alpha = Random.Range(0, 2 * Mathf.PI);
            float beta = Mathf.Acos(Random.Range(-1f, 1f));
            float sinBeta = Mathf.Sin(beta);
            float radius = Random.Range(minRadius, maxRadius);
            sinBeta *= radius;
            envObjects[envObjects.Count - 1].relativeTargetPos = new Vector3(Mathf.Cos(alpha) * sinBeta, Mathf.Cos(beta) * radius, Mathf.Sin(alpha) * sinBeta);
            if (!onServer)
            {
                //  set layer, instanceID (for deleting/updating objects) only for clients
                envObjects[envObjects.Count - 1].gameObject.layer = 9;
                envObjects[envObjects.Count - 1].instanceID = ID;
                //  disable unnecessary components
                envObjects[envObjects.Count - 1].GetComponent<Rigidbody>().isKinematic = true;
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