using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MastersOfTempest.Environment.Interacting;
using MastersOfTempest.Environment.VisualEffects;
using MastersOfTempest.Networking;

namespace MastersOfTempest.Environment
{
    public class EnvironmentManager : MonoBehaviour
    {
        public VectorField vectorField;
        public VisualSpawner visualSpawner;
        public EnvSpawner envSpawner { get; private set; }
        private Gamemaster gamemaster;

        void Start()
        {
            vectorField = GetComponent<VectorField>();
            if (vectorField == null)
                throw new System.InvalidOperationException("VectorField is not specified");

            visualSpawner = GetComponent<VisualSpawner>();
            if (visualSpawner == null)
                throw new System.InvalidOperationException("Spawner for visual effects is not specified");
            visualSpawner.Initialize(vectorField);

            gamemaster = FindObjectOfType<Gamemaster>();
            if (gamemaster == null)
            {
                throw new System.InvalidOperationException("EnvironmentManager cannot operate without Gamemaster in the same scene!");
            }

            envSpawner = GetComponent<EnvSpawner>();
            if (envSpawner == null)
                throw new System.InvalidOperationException("Spawner for environment objects is not specified");
            envSpawner.Initialize(gamemaster);
            gamemaster.Register(this);
        }
    }
}