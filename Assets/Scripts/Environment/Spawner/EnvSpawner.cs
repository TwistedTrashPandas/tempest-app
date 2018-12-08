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
            Supporting,
            Null
        };

        // spawn parameters 
        public float spawnRate;
        [Range(0f, 2f)]
        public float dampingFactorVel;
        [Range(0f, 100f)]
        public float dampingFactorForce;
        public float maximumObjVelocity;

        public float spawnProbD;
        public float spawnProbS;
        public float spawnProbZ;

        // for initializing a random target position around the ship
        public float minRadiusT;
        public float maxRadiusT;

        // for initializing a random spawn position around the center of the tornado
        public float minRadiusS;
        public float maxRadiusS;

        public uint maxNumObjects;
        public VectorField vectorField;
        // all envObjects treated the same
        public List<EnvObject> envObjects { get; private set; }
        // prefab lists
        public GameObject[] damagingPrefabs;
        public GameObject[] supportingPrefabs;
        public GameObject[] dangerzonesPrefabs;

        public MoveType moveType;
        public GameObject objectContainer;

        // TODO use currServerTime for synchronization 

        private float spawnProbSum;
        private float currServerTime;
        private float currFixedTime;
        private float hz;
        private bool onServer;
        private Gamemaster gamemaster;
        // last obj positions (extrapolate or interpolate ?)
        private List<Transform> envObjTransforms;

        public void Initialize(Gamemaster gm, VectorField vf, bool server)
        {
            envObjects = new List<EnvObject>();
            onServer = server;
            // test initialization for networking
            if (onServer)
            {
                vectorField = vf;
                objectContainer = GameObject.Instantiate(objectContainer);
                hz = 1.0f / GameServer.Instance.hz;
                StartSpawning();
                objectContainer.layer = 9;
            }
            else
            {
                envObjTransforms = new List<Transform>();
                hz = 1f / 64f;
            }
            currServerTime = 0f;
            objectContainer.layer = 10;
            gamemaster = gm;
            spawnProbSum = spawnProbD + spawnProbS + spawnProbZ;
        }

        private void FixedUpdate()
        {
            // TODO: multiple implementations for behaviour
            // update all objects' velocity or add force by looking up value in vector grid if the spawner is on the server
            if (onServer)
            {
                Vector3 targetPos = gamemaster.GetShip().transform.position;
                for (int i = envObjects.Count - 1; i >= 0; i--)
                {
                    if (envObjects[i] == null)
                    {
                        envObjects.RemoveAt(i);
                        continue;
                    }
                    else
                    {
                        envObjects[i].MoveNext(targetPos, vectorField.GetVectorAtPos(envObjects[i].transform.position));
                        if (!Mathf.Approximately(dampingFactorForce, 0f))
                            DampForce(i);
                        if (!Mathf.Approximately(dampingFactorVel, 1f))
                            DampVelocity(i);
                        ClampVelocity(i);
                    }
                }
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

        private Vector3 GetRandomPointOnSphere(float minRadius, float maxRadius)
        {
            float alpha = Random.Range(0, 2 * Mathf.PI);
            float beta = Mathf.Acos(Random.Range(-1f, 1f));
            float sinBeta = Mathf.Sin(beta);
            float radius = Random.Range(minRadius, maxRadius);
            sinBeta *= radius;
            return new Vector3(Mathf.Cos(alpha) * sinBeta, Mathf.Cos(beta) * radius, Mathf.Sin(alpha) * sinBeta);
        }

        public void StartSpawning()
        {
            StartCoroutine(SpawnObject());
        }
        private IEnumerator SpawnObject()
        {
            if (envObjects.Count > maxNumObjects)
                envObjects.RemoveAt(0);
            Vector3 centerPos = vectorField.GetCenter();
            centerPos.y = 0f;
            EnvObjectType type;
            float randomType = Random.Range(0f, spawnProbSum);

            // randomly select type of envObject
            if (randomType < spawnProbD)
                type = EnvObjectType.Damaging;
            else
            {
                if (randomType < spawnProbS + spawnProbD)
                    type = EnvObjectType.Supporting;
                else
                    type = EnvObjectType.DangerZone;
            }

            InstantiateNewObject(true, centerPos, Vector3.one, Quaternion.identity, type, 0);
            yield return new WaitForSeconds(spawnRate);
            StartCoroutine(SpawnObject());
        }

        /*
        private void MoveAllDirectly(int idx, Vector3 targetPos)
        {
            envObjects[idx].MoveDirectly(targetPos);
        }

        private void AddForceToAll(int idx)
        {
            envObjects[idx].AddForce(vectorField.GetVectorAtPos(envObjects[idx].transform.position), new Vector3());
        }

        private void SetVelForAll(int idx)
        {
            envObjects[idx].SetVelocity(vectorField.GetVectorAtPos(envObjects[idx].transform.position));
        }*/

        private void DampVelocity(int idx)
        {
            envObjects[idx].DampVelocity(dampingFactorVel);
        }

        private void DampForce(int idx)
        {
            envObjects[idx].DampForce(dampingFactorForce);
        }

        private void ClampVelocity(int idx)
        {
            envObjects[idx].ClampVelocity(maximumObjVelocity);
        }

        // main functions for initializing an envobject of given type 
        private void InstantiateNewObject(bool onServer, Vector3 position, Vector3 localScale, Quaternion orientation, EnvObjectType type = EnvObjectType.Damaging, int ID = 0)
        {
            Vector3 randOffset = GetRandomPointOnSphere(minRadiusS, maxRadiusS);
            position += randOffset;
            {
                int prefabNum = 0;
                switch (type)
                {
                    case EnvObjectType.Damaging:
                        prefabNum = Mathf.FloorToInt(Random.Range(0f, damagingPrefabs.Length-Mathf.Epsilon));
                        envObjects.Add(GameObject.Instantiate(damagingPrefabs[prefabNum], position, orientation).GetComponent<EnvObject>());
                        break;
                    case EnvObjectType.DangerZone:
                        prefabNum =  Mathf.FloorToInt( Random.Range(0f, dangerzonesPrefabs.Length - Mathf.Epsilon));
                        envObjects.Add(GameObject.Instantiate(dangerzonesPrefabs[prefabNum], position, orientation).GetComponent<EnvObject>());
                        break;
                    case EnvObjectType.Supporting:
                        prefabNum = Mathf.FloorToInt(Random.Range(0f, supportingPrefabs.Length - Mathf.Epsilon));
                        envObjects.Add(GameObject.Instantiate(supportingPrefabs[prefabNum], position, orientation).GetComponent<EnvObject>());
                        break;
                }
                envObjects[envObjects.Count - 1].transform.parent = objectContainer.transform;
                envObjects[envObjects.Count - 1].relativeTargetPos = GetRandomPointOnSphere(minRadiusT, maxRadiusT);
                // prefabnumber only important for client to choose correct prefab for initialization
                envObjects[envObjects.Count - 1].moveType = moveType;
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
                    InstantiateNewObject(false, objects[i].position, objects[i].localScale, objects[i].orientation, objects[i].type, objects[i].instanceID);
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