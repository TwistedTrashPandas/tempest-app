using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MastersOfTempest.Environment.Interacting;
using MastersOfTempest.Environment.VisualEffects;
using MasterOfTempest.Networking;

namespace MastersOfTempest.Environment
{
    public class EnvironmentManager : MonoBehaviour
    {
        public VectorField vectorField;
        public VisualSpawner visualSpawner;
        public EnvSpawner envSpawner { get; private set; }
        
        void Awake()
        {
            visualSpawner = GetComponent<VisualSpawner>();
            if (visualSpawner == null)
                throw new System.InvalidOperationException("Spawner for visual effects is not specified");

            envSpawner = GetComponent<EnvSpawner>();
            if (envSpawner == null)
                throw new System.InvalidOperationException("Spawner for environment objects is not specified");
        }

        void Update()
        {

        }
    }
}