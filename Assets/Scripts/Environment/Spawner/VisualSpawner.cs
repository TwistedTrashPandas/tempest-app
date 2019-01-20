using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MastersOfTempest.Networking;

namespace MastersOfTempest.Environment.VisualEffects
{
    public class VisualSpawner : MonoBehaviour
    {
        public bool water;
        public bool tornPS;
        public bool tornTex;
        public bool cpuVF;

        public GameObject waterPrefab;
        private HeightField heightField;

        public GameObject tornadoPrefab;
        private TornadoPS tornadoPS;

        public GameObject tornadoTexPrefab;
        private SetTornadoTexture tornadoTex;

        public GameObject cpuParticleVectorFieldPrefab;
        private GameObject cpuParticleVectorField;

        public void Initialize(VectorField vectorField)
        {
            // only run tornado on client
            if (!GetComponent<ServerObject>().onServer)
            {
                if (water)
                {
                    if (waterPrefab == null)
                        throw new System.InvalidOperationException("Water prefab can't be null");
                    GameObject temp = GameObject.Instantiate(waterPrefab);
                    heightField = temp.GetComponent<HeightField>();
                    if (heightField == null)
                        throw new System.InvalidOperationException("HeightField.cs has to be attached to the water prefab");
                    // heightField.vectorField = vectorField;
                    heightField.gameObject.layer = 4;
                    heightField.mainCam = Camera.main;
                    heightField.Initialize(vectorField.GetCenterWS());
                }
                if (cpuVF)
                {
                    if (cpuParticleVectorFieldPrefab == null)
                        throw new System.InvalidOperationException("Water prefab can't be null");
                    GameObject temp = GameObject.Instantiate(cpuParticleVectorFieldPrefab,vectorField.GetCenterWS(),Quaternion.identity);
                    cpuParticleVectorField = temp;
                }

                if (tornPS)
                {
                    if (tornadoPrefab == null)
                        throw new System.InvalidOperationException("Tornado prefab can't be null");
                    GameObject temp = GameObject.Instantiate(tornadoPrefab);
                    tornadoPS = temp.GetComponent<TornadoPS>();
                    if (tornadoPS == null)
                        throw new System.InvalidOperationException("TornadoPS.cs has to be attached to the tornado prefab");
                    tornadoPS.vectorField = vectorField;
                    tornadoPS.gameObject.layer = 0;
                }

                if (tornTex)
                {
                    if (tornadoTexPrefab == null)
                        throw new System.InvalidOperationException("TornadoTex prefab can't be null");
                    GameObject temp = GameObject.Instantiate(tornadoTexPrefab);
                    tornadoTex = temp.GetComponent<SetTornadoTexture>();
                    if (tornadoTex == null)
                        throw new System.InvalidOperationException("SetTornadoTexture.cs has to be attached to the tornadoTex prefab");
                    tornadoTex.vectorField = vectorField;
                    tornadoTex.camPos = Camera.main.transform;
                    tornadoTex.gameObject.layer = 0;
                }
            }
        }
    }
}