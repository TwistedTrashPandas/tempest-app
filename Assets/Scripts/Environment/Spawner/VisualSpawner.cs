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

        public void Initialize(VectorField vectorField)
        {
            if (tornadoPrefab == null)
                throw new System.InvalidOperationException("Tornado prefab can't be null");
            GameObject temp = GameObject.Instantiate(tornadoPrefab);
            tornadoPS = temp.GetComponent<TornadoPS>();
            if (tornadoPS == null)
                throw new System.InvalidOperationException("TornadoPS.cs has to be attached to the tornado prefab");
            tornadoPS.vectorField = vectorField;

            // only run tornado on client
            if (!GetComponent<ServerObject>().onServer)
            {
                tornadoPS.gameObject.SetActive(true);
                tornadoPS.gameObject.layer = 9;
            }
            else
            {
                Destroy(tornadoPS.gameObject);
            }
        }
    }
}