using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MasterOfTempest.Networking;
using static MasterOfTempest.EnvironmentNetwork;

namespace MastersOfTempest.Environment.Interacting
{
    public class EnvSpawner : MonoBehaviour
    {
        public enum EnvObjectType
        {
            Damaging,
            DangerZone,
            Helping
        };

        public float damping_factor;
        public GameObject[] prefabs;
        public VectorField vectorField;
        public List<EnvObject> envObjects { get; private set; }


        private void Start()
        {
            envObjects = new List<EnvObject>();
            if (GetComponent<ServerObject>().onServer)
            {
                InstantiateNewObject(true);
                //InstantiateNewObject(true);
                //InstantiateNewObject(true);
                //InstantiateNewObject(true);
            }
        }

        void FixedUpdate()
        {
            // update all objects' velocity by looking up value in velocity grid if the spawner is on the server
            if (GetComponent<ServerObject>().onServer)
            {
                for (int i = 0; i < envObjects.Count; i++)
                {
                    envObjects[i].AddForce(vectorField.GetVectorAtPos(envObjects[i].transform.position), new Vector3());
                    //envObjects[i].SetVelocity(vectorField.GetVectorAtPos(envObjects[i].transform.position));
                    // envObjects[i].DampVelocity(damping_factor);
                }
                if (envObjects.Count == 4)
                {
                    GameObject toDestroy = envObjects[2].gameObject;
                    envObjects.RemoveAt(2);
                    Destroy(toDestroy);
                }
                else if (envObjects.Count == 3)
                {
                    InstantiateNewObject(true);
                    InstantiateNewObject(true);
                    InstantiateNewObject(true);
                    InstantiateNewObject(true);
                }
            }
        }

        private void InstantiateNewObject(bool onServer, EnvObjectType type = EnvObjectType.Damaging, int ID = 0)
        {
            envObjects.Add(GameObject.Instantiate(prefabs[0]).GetComponent<EnvObject>());
            if (!onServer)
            {
                envObjects[envObjects.Count - 1].gameObject.layer = 9;
                envObjects[envObjects.Count - 1].instanceID = ID;
                Destroy(envObjects[envObjects.Count - 1].GetComponent<Rigidbody>());
                Destroy(envObjects[envObjects.Count - 1].GetComponent<Collider>());
            }
        }

        private void UpdateTransform(MessageEnvObject obj, int idx)
        {
            envObjects[idx].transform.position = obj.position;
            envObjects[idx].transform.localScale = obj.localScale;
            envObjects[idx].transform.rotation = obj.orientation;
        }

        public void UpdateEnvObjects(List<MessageEnvObject> objects)
        {
            for (int i = 0; i < objects.Count; i++)
            {
                if (envObjects.Count <= i)
                {
                    InstantiateNewObject(false, objects[i].type, objects[i].instanceID);
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