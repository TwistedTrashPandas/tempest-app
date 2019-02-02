using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MastersOfTempest.Networking;
using static MastersOfTempest.EnvironmentNetwork;
using UnityEngine.SceneManagement;
using MastersOfTempest.ShipBL;

namespace MastersOfTempest.Environment.Interacting
{
    public class EnvSpawner : MonoBehaviour
    {
        // spawn parameters 
        public bool spawning = true;

        public float spawnRate;
        [Range(0f, 2f)]
        public float dampingFactorVel;
        [Range(0f, 100f)]
        public float dampingFactorForce;
        public float maximumObjVelocity;
        public float maxLargeObjVelocity;

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

        public int numRings = -1;
        public uint startObjects = 20;
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

        private const float spawnDistToShip = 70f;

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
            float avg = 0f;
            for (int i = 0; i < 100000f; i++)
            {
                avg += RandomGaussian.NextGaussian(0f, 1f);
            }
            Debug.Log(avg /= 100000f);
        }

        private void FixedUpdate()
        {
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
                        // DISABLED DAMPING FOR NOW
                        /*if (!Mathf.Approximately(dampingFactorForce, 0f))
                            DampForce(i);
                        if (!Mathf.Approximately(dampingFactorVel, 1f))
                            DampVelocity(i);*/
                        ClampVelocity(i);
                    }
                }
            }
        }

        private void RemoveFirstEnvObject()
        {
            if (envObjects.Count > maxNumObjects)
            {
                EnvObject toDestroy;
                toDestroy = envObjects[0];
                envObjects.RemoveAt(0);
                if (toDestroy is Damaging)
                {
                    toDestroy.EnableGravity();
                    toDestroy.GetComponent<Damaging>().health = -1f;
                    Destroy(toDestroy.gameObject, 10f);
                }
                else
                    Destroy(toDestroy.gameObject, 0f);
            }
        }

        public void AddEnvObject(EnvObject envObject)
        {
            RemoveFirstEnvObject();
            envObjects.Add(envObject);
        }

        private Vector3 GetRandomPointOnSphere(float minRadius, float maxRadius, bool position = false)
        {
            float alpha = Random.Range(0f, 2f * Mathf.PI);
            float beta = Mathf.Acos(Random.Range(-1f, 1f));
            float sinBeta = Mathf.Sin(beta);
            float radius = maxRadius - Mathf.Pow(Random.Range(0f, 1f), 1f / 3f) * (maxRadius - minRadius);
            if (position && numRings > 0)
            {
                float stepSize = maxRadius / numRings;
                Vector3 centerWS = vectorField.GetCenterWS();
                Vector3 shipPos = gamemaster.GetShip().transform.position;
                centerWS.y = 0f;
                shipPos.y = 0f;
                if (startObjects == maxNumObjects) 
                    radius = Mathf.Min(Mathf.Min(Mathf.Floor(Mathf.Pow(envObjects.Count, 2.0f) / numRings), numRings) * stepSize, Vector3.Distance(shipPos, centerWS)) + minRadius;
                else
                    radius = Random.Range(0, numRings) * Mathf.Min(stepSize, Vector3.Distance(shipPos, centerWS)) + minRadius;
            }
            sinBeta *= radius;
            return new Vector3(Mathf.Cos(alpha) * sinBeta, Mathf.Cos(beta) * radius, Mathf.Sin(alpha) * sinBeta);
        }

        public Vector3 GetShipPos()
        {
            return gamemaster.GetShip().transform.position;
        }

        public void StartSpawning()
        {
            StartCoroutine(SpawnObject());
        }

        private IEnumerator SpawnObject()
        {
            yield return new WaitForSeconds(spawnRate * 2);
            int firstSpawns = (int)startObjects;
            while (((spawnRate > 0f && startObjects != maxNumObjects) || (envObjects.Count < maxNumObjects)) && spawning)
            {
                RemoveFirstEnvObject();

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
                if (firstSpawns > 0)
                {
                    yield return new WaitForSeconds(0f);
                    firstSpawns--;
                }
                else
                    yield return new WaitForSeconds(spawnRate);
            }
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

        public void RemoveAllObjects()
        {
            spawnRate = -1f;
            maxNumObjects = 0;
            spawning = false;
            for (int i = envObjects.Count; i >= 0; i--)
            {
                RemoveFirstEnvObject();
            }
        }

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
            if (envObjects[idx].transform.localScale.x > 10f)
                envObjects[idx].ClampVelocity(maxLargeObjVelocity);
            else
                envObjects[idx].ClampVelocity(maximumObjVelocity);
        }

        // main functions for initializing an envobject of given type 
        private void InstantiateNewObject(bool onServer, Vector3 position, Quaternion orientation, EnvObjectType type = EnvObjectType.Damaging, int ID = 0)
        {
            Vector3 localScale = Vector3.one;
            Vector3 dims = vectorField.GetDimensions();
            Vector3 initialPos = Vector3.zero;
            float cellSize = vectorField.GetCellSize();
            float cellSizeH = vectorField.GetHorizontalCellSize();
            {
                int prefabNum = 0;
                switch (type)
                {
                    case EnvObjectType.Damaging:
                        float randomSize;
                        Vector3 randOffset;
                        Quaternion randOrientation = Quaternion.Euler(Random.Range(0f, 180f), Random.Range(0f, 180f), Random.Range(0f, 180f));
                        prefabNum = Mathf.FloorToInt(Random.Range(0f, damagingPrefabs.Length - Mathf.Epsilon));
                        var currGO = GameObject.Instantiate(damagingPrefabs[prefabNum], position, randOrientation);
                        var currEnvObject = currGO.GetComponent<EnvObject>();
                        envObjects.Add(currEnvObject);
                        Damaging dmg = currGO.GetComponent<Damaging>();
                        if (Random.Range(0, 3) == 0)
                        {
                            randomSize = Random.Range(0.5f, 1.5f);
                            currEnvObject.moveType = (MoveType.Direct); //((Random.Range(2, 4) >= 3) ? 3 : 2);
                            currEnvObject.speed *= 0.4f;
                            randOffset = GetRandomPointOnSphere(minRadiusS, maxRadiusS, numRings > 0);
                            dmg.damage = 0.1f * randomSize;
                        }
                        else
                        {
                            randomSize = Random.Range(15f, 25f);
                            currEnvObject.speed *= 0.01f;
                            dmg.health = randomSize;
                            currEnvObject.moveType = (MoveType)((Random.Range(0, 3) <= 1) ? 0 : 2); // MoveType.ForceDirect; // 
                            if ((int)currEnvObject.moveType <= 1)
                                currEnvObject.GetComponent<Rigidbody>().constraints |= (RigidbodyConstraints.FreezePositionY);
                            randOffset = GetRandomPointOnSphere(minRadiusS * 1.5f, maxRadiusS * 1.1f, numRings > 0);
                            dmg.damage = 0.35f * randomSize;
                            currEnvObject.SetMass(randomSize);
                        }

                        // hard coded so far larger rocks are slower but deal more damage
                        currEnvObject.GetComponent<Damaging>().envSpawner = this;

                        localScale = new Vector3(randomSize, randomSize, randomSize);
                        position += randOffset;

                        position.y = Mathf.Clamp(gamemaster.GetShip().transform.position.y + RandomGaussian.NextGaussian(0, 1f, -1f, 1f) * dims.y * cellSize * 0.25f, dims.y * cellSize * 0.05f, dims.y * cellSize * 0.85f);

                        if (Vector3.Distance(position, gamemaster.GetShip().transform.position) < spawnDistToShip)
                            position += currEnvObject.transform.forward * spawnDistToShip;

                        currEnvObject.transform.position = position;
                        currEnvObject.transform.localScale = localScale;
                        currEnvObject.GetComponent<Rigidbody>().angularVelocity = (new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)) * rockRotSpeed) / randomSize;

                        break;
                    case EnvObjectType.DangerZone:
                        initialPos = new Vector3(Random.Range(0, dims.x) * cellSizeH, 0f, Random.Range(0, dims.z) * cellSizeH) + new Vector3(0.5f, 0.5f, 0.5f);
                        initialPos.y = Mathf.Clamp(gamemaster.GetShip().transform.position.y + RandomGaussian.NextGaussian(0, 1f, -1f, 1f) * dims.y * cellSize * 0.3f, dims.y * cellSize * 0.05f, dims.y * cellSize * 0.85f);
                        prefabNum = Mathf.FloorToInt(Random.Range(0f, dangerzonesPrefabs.Length - Mathf.Epsilon));
                        envObjects.Add(GameObject.Instantiate(dangerzonesPrefabs[prefabNum], initialPos, orientation).GetComponent<EnvObject>());
                        Destroy(envObjects[envObjects.Count - 1].GetComponent<ParticleSystem>());
                        envObjects[envObjects.Count - 1].moveType = MoveType.Static;
                        break;
                    case EnvObjectType.VoiceChatZone:
                        initialPos = new Vector3(Random.Range(0, dims.x) * cellSizeH, 0f, Random.Range(0, dims.z) * cellSizeH) + new Vector3(0.5f, 0.5f, 0.5f);
                        initialPos.y = Mathf.Clamp(gamemaster.GetShip().transform.position.y + RandomGaussian.NextGaussian(0, 1f, -1f, 1f) * dims.y * cellSize * 0.3f, dims.y * cellSize * 0.05f, dims.y * cellSize * 0.85f);
                        prefabNum = Mathf.FloorToInt(Random.Range(0f, voiceChatZonesPrefabs.Length - Mathf.Epsilon));
                        envObjects.Add(GameObject.Instantiate(voiceChatZonesPrefabs[prefabNum], initialPos, orientation).GetComponent<EnvObject>());
                        envObjects[envObjects.Count - 1].moveType = MoveType.Static;
                        Destroy(envObjects[envObjects.Count - 1].GetComponent<ParticleSystem>());
                        break;
                }
                envObjects[envObjects.Count - 1].transform.parent = objectContainer.transform;
                envObjects[envObjects.Count - 1].GetComponent<EnvObject>().listIndex = envObjects.Count - 1;
                envObjects[envObjects.Count - 1].relativeTargetPos = GetRandomPointOnSphere(minRadiusT, maxRadiusT);
                // prefabnumber only important for client to choose correct prefab for initialization
            }
        }
    }
}