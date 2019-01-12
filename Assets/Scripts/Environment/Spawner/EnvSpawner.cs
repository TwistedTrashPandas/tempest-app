using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MastersOfTempest.Networking;
using static MastersOfTempest.EnvironmentNetwork;
using UnityEngine.SceneManagement;

namespace MastersOfTempest.Environment.Interacting
{
    public class EnvSpawner : MonoBehaviour
    {
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

        public float rockRotSpeed;

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
        public GameObject[] voiceChatZonesPrefabs;
        public GameObject[] dangerzonesPrefabs;

        public MoveType moveType;
        public GameObject objectContainerPrefab;

        // TODO use currServerTime for synchronization 

        private float spawnProbSum;
        private bool onServer;
        private Gamemaster gamemaster;
        private GameObject objectContainer;

        public void Initialize(Gamemaster gm, VectorField vf, bool server)
        {
            envObjects = new List<EnvObject>();
            onServer = server;
            // test initialization for networking
            if (onServer)
            {
                vectorField = vf;
                // Switch active scene so that instantiate creates the object as part of the client scene       
                Scene previouslyActiveScene = SceneManager.GetActiveScene();
                SceneManager.SetActiveScene(GameServer.Instance.gameObject.scene);
                objectContainer = GameObject.Instantiate(objectContainerPrefab);
                // Switch back to the previously active scene  
                SceneManager.SetActiveScene(previouslyActiveScene);
                StartSpawning();
            }
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
        }

        private Vector3 GetRandomPointOnSphere(float minRadius, float maxRadius)
        {
            float alpha = Random.Range(0f, 2f * Mathf.PI);
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
            yield return new WaitForSeconds(spawnRate);
            if (envObjects.Count > maxNumObjects)
            {
                GameObject toDestroy;
                toDestroy = envObjects[0].gameObject;
                envObjects.RemoveAt(0);
                Destroy(toDestroy);
            }
            Vector3 centerPos = vectorField.GetCenterWS();
            centerPos.y = 0f;
            EnvObjectType type;
            float randomType = Random.Range(0f, spawnProbSum);

            // randomly select type of envObject
            if (randomType < spawnProbD)
                type = EnvObjectType.Damaging;
            else
            {
                if (randomType < spawnProbS + spawnProbD)
                    type = EnvObjectType.VoiceChatZone;
                else
                    type = EnvObjectType.DangerZone;
            }

            InstantiateNewObject(true, centerPos, Quaternion.identity, type, 0);
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
        private void InstantiateNewObject(bool onServer, Vector3 position, Quaternion orientation, EnvObjectType type = EnvObjectType.Damaging, int ID = 0)
        {
            Vector3 randOffset = GetRandomPointOnSphere(minRadiusS, maxRadiusS);
            Vector3 localScale = Vector3.one;
            Vector3 dims = vectorField.GetDimensions();
            Vector3 initialPos = Vector3.zero;
            float cellSize = vectorField.GetCellSize();
            float cellSizeH = vectorField.GetHorizontalCellSize();
            position += randOffset;
            {
                int prefabNum = 0;
                switch (type)
                {
                    case EnvObjectType.Damaging:
                        float randomSize = Random.Range(1f, 8.0f);
                        localScale = new Vector3(randomSize, randomSize, randomSize);
                        prefabNum = Mathf.FloorToInt(Random.Range(0f, damagingPrefabs.Length - Mathf.Epsilon));
                        position.y = Random.Range(0f, dims.y * cellSize);
                        envObjects.Add(GameObject.Instantiate(damagingPrefabs[prefabNum], position, orientation).GetComponent<EnvObject>());
                        // hard coded so far larger rocks are slower but deal more damage
                        envObjects[envObjects.Count - 1].GetComponent<Damaging>().damage = 0.1f * randomSize;
                        envObjects[envObjects.Count - 1].GetComponent<Rigidbody>().angularVelocity = new Vector3(Random.Range(-1f,1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)) * rockRotSpeed;
                        envObjects[envObjects.Count - 1].speed *= 1f / randomSize;
                        if (Random.Range(0, 10) == 0)
                        {
                            envObjects[envObjects.Count - 1].moveType = MoveType.Direct; // (MoveType) Random.Range(0,3);
                            envObjects[envObjects.Count - 1].speed *= 0.4f;
                        }
                        else
                            envObjects[envObjects.Count - 1].moveType = MoveType.ForceDirect; // (MoveType) Random.Range(0,3);
                        break;
                    case EnvObjectType.DangerZone:
                        initialPos = new Vector3(Random.Range(0, dims.x) * cellSizeH, Random.Range(dims.y * 0.1f, 0.8f * dims.y)* cellSize, Random.Range(0, dims.z)* cellSizeH) + new Vector3(0.5f,0.5f,0.5f);
                        prefabNum = Mathf.FloorToInt(Random.Range(0f, dangerzonesPrefabs.Length - Mathf.Epsilon));
                        envObjects.Add(GameObject.Instantiate(dangerzonesPrefabs[prefabNum], initialPos, orientation).GetComponent<EnvObject>());
                        Destroy(envObjects[envObjects.Count - 1].GetComponent<ParticleSystem>());
                        //envObjects[envObjects.Count - 1].moveType = MoveType.Static;
                        break;
                    case EnvObjectType.VoiceChatZone:
                        initialPos = new Vector3(Random.Range(0, dims.x)* cellSizeH, Random.Range(dims.y * 0.1f, 0.8f * dims.y)* cellSize, Random.Range(0, dims.z)* cellSizeH)  + new Vector3(0.5f, 0.5f, 0.5f);
                        prefabNum = Mathf.FloorToInt(Random.Range(0f, voiceChatZonesPrefabs.Length - Mathf.Epsilon));
                        envObjects.Add(GameObject.Instantiate(voiceChatZonesPrefabs[prefabNum], initialPos, orientation).GetComponent<EnvObject>());
                       // envObjects[envObjects.Count - 1].moveType = MoveType.Static;                    
                        Destroy(envObjects[envObjects.Count - 1].GetComponent<ParticleSystem>());                          
                        break;
                }
                envObjects[envObjects.Count - 1].transform.parent = objectContainer.transform;
                envObjects[envObjects.Count - 1].GetComponent<EnvObject>().listIndex = envObjects.Count - 1;
                envObjects[envObjects.Count - 1].transform.localScale = localScale;
                envObjects[envObjects.Count - 1].relativeTargetPos = GetRandomPointOnSphere(minRadiusT, maxRadiusT);
                // prefabnumber only important for client to choose correct prefab for initialization
            }
        }
    }
}