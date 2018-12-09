using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MastersOfTempest.Networking;

namespace MastersOfTempest.Environment.VisualEffects
{
    public class VisualSpawner : MonoBehaviour
    {
        public GameObject tornadoPrefab;
        private TornadoPS tornadoPS;

        public GameObject tornadoTexPrefab;
        private SetTornadoTexture tornadoTex;

        public void Initialize(VectorField vectorField)
        {
            if (!GetComponent<ServerObject>().onServer)
            {
                if (tornadoPrefab == null)
                    throw new System.InvalidOperationException("Tornado prefab can't be null");
                GameObject temp = GameObject.Instantiate(tornadoPrefab);
                tornadoPS = temp.GetComponent<TornadoPS>();
                if (tornadoPS == null)
                    throw new System.InvalidOperationException("TornadoPS.cs has to be attached to the tornado prefab");
                tornadoPS.vectorField = vectorField;

                if (tornadoTexPrefab == null)
                    throw new System.InvalidOperationException("TornadoTex prefab can't be null");
                temp = GameObject.Instantiate(tornadoTexPrefab);
                tornadoTex = temp.GetComponent<SetTornadoTexture>();
                if (tornadoPS == null)
                    throw new System.InvalidOperationException("SetTornadoTexture.cs has to be attached to the tornadoTex prefab");
                tornadoTex.vectorField = vectorField;
                tornadoTex.camPos = Camera.main.transform;
                // only run tornado on client
                tornadoPS.gameObject.layer = 9;
                tornadoTex.gameObject.layer = 9;
            }
        }
    }
}