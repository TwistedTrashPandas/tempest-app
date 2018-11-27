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
        public static EnvironmentManager instance_c;
        public static EnvironmentManager instance_s;

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

        void Start()
        {
            if (GetComponent<ServerObject>().onServer)
            {
                if (instance_s == null)
                    instance_s = this;
                else
                    throw new System.InvalidOperationException("EnvironmentManager instace already exists on server!");
            }
            else
            {
                if (instance_c == null)
                    instance_c = this;
                else
                    throw new System.InvalidOperationException("EnvironmentManager instace already exists on server!");
            }
        }

        void Update()
        {

        }
    }
}