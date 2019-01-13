using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MastersOfTempest.Environment.Interacting;
using MastersOfTempest.Environment.VisualEffects;
using MastersOfTempest.Networking;

namespace MastersOfTempest.Environment
{
    public class EnvironmentManager : NetworkBehaviour
    {
        public VectorField vectorField;
        public VisualSpawner visualSpawner;
        public EnvSpawner envSpawner { get; private set; }
        private Gamemaster gamemaster;

        private struct DamageRockMessage
        {
            public int rockServerID;
            public float damage;
        }

        protected override void StartServer()
        {
            base.StartServer();
            Initialize(true);
        }

        protected override void StartClient()
        {
            Initialize(false);
        }

        private void Initialize(bool onServer)
        {
            // initalize important scripts for environment + register for gamemaster
            vectorField = GetComponent<VectorField>();
            if (vectorField == null)
                throw new System.InvalidOperationException("VectorField is not specified");
            
            if (onServer)
            {
                gamemaster = GameObject.Find("Gamemaster").GetComponent<Gamemaster>();// FindObjectOfType<Gamemaster>();
                if (gamemaster == null)
                {
                    throw new System.InvalidOperationException("EnvironmentManager cannot operate without Gamemaster in the same scene!");
                }

                envSpawner = GetComponent<EnvSpawner>();
                if (envSpawner == null)
                    throw new System.InvalidOperationException("Spawner for environment objects is not specified");
                envSpawner.Initialize(gamemaster, vectorField, GetComponent<ServerObject>().onServer);
                gamemaster.Register(this);
            }
            else
            {
                visualSpawner = GetComponent<VisualSpawner>();
                if (visualSpawner == null)
                    throw new System.InvalidOperationException("Spawner for visual effects is not specified");
                visualSpawner.Initialize(vectorField);
            }
        }

        protected override void OnServerReceivedMessageRaw(byte[] data, ulong steamID)
        {
            DamageRockMessage message = ByteSerializer.FromBytes<DamageRockMessage>(data);
            ServerObject rock = GameServer.Instance.GetServerObject(message.rockServerID);
            rock.GetComponent<Damaging>().RemoveHealth(message.damage);
            print(message.damage);
        }

        public void DamageRockOnServer (int rockServerID, float damage)
        {
            if (serverObject.onServer)
            {
                Debug.LogError(nameof(DamageRockOnServer) + " should not be called on the server!");
            }
            else
            {
                DamageRockMessage message = new DamageRockMessage();
                message.rockServerID = rockServerID;
                message.damage = damage;
                SendToServer(ByteSerializer.GetBytes(message), Facepunch.Steamworks.Networking.SendType.Reliable);
            }
        }
    }
}