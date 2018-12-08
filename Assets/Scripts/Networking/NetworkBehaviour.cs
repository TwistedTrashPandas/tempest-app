﻿using UnityEngine;

namespace MastersOfTempest.Networking
{
    [RequireComponent(typeof(ServerObject))]
    public class NetworkBehaviour : MonoBehaviour
    {
        protected bool initialized = false;
        protected ServerObject serverObject;

        public NetworkMessageType networkMessageType = NetworkMessageType.Empty;

        [System.Serializable]
        private struct NetworkBehaviourMessage
        {
            public int serverID;
            public string message;

            public NetworkBehaviourMessage(int serverID, string message)
            {
                this.serverID = serverID;
                this.message = message;
            }
        };

        protected virtual void Start()
        {
            initialized = false;
            serverObject = GetComponent<ServerObject>();

            if (serverObject.onServer)
            {
                ClientManager.Instance.serverMessageEvents[networkMessageType] += OnServerMessage;
                // Wait for the initialize message
                ClientManager.Instance.serverMessageEvents[NetworkMessageType.NetworkBehaviourInitialized] += OnServerNetworkBehaviourInitialized;
            }
            else
            {
                //We should remove all "physics" components on the client side: 
                //only Server should determine all the movement;
                RemovePhysics();

                ClientManager.Instance.clientMessageEvents[networkMessageType] += OnClientMessage;

                // Wait for the initialize message
                ClientManager.Instance.clientMessageEvents[NetworkMessageType.NetworkBehaviourInitialized] += OnClientNetworkBehaviourInitialized;

                // Begin the initialization, tell the server that this object is ready
                byte[] data = System.BitConverter.GetBytes(serverObject.serverID);
                ClientManager.Instance.SendToServer(data, NetworkMessageType.NetworkBehaviourInitialized, Facepunch.Steamworks.Networking.SendType.Reliable);
            }

            if (networkMessageType == NetworkMessageType.Empty)
            {
                Debug.LogError("NetworkMessageType of " + gameObject.name + " should not be Empty!\nDid you forget to add a new type in NetworkMessages.cs?");
            }
        }

        protected virtual void StartServer()
        {
            // To be overwritten by the superclass
        }

        protected virtual void StartClient()
        {
            // To be overwritten by the superclass
        }

        protected virtual void Update()
        {
            if (initialized)
            {
                if (serverObject.onServer)
                {
                    UpdateServer();
                }
                else
                {
                    UpdateClient();
                }
            }
        }

        protected virtual void UpdateServer()
        {
            // To be overwritten by the superclass
        }

        protected virtual void UpdateClient()
        {
            // To be overwritten by the superclass
        }

        private void OnServerNetworkBehaviourInitialized(byte[] data, ulong steamID)
        {
            int initializedServerObjectID = System.BitConverter.ToInt32(data, 0);

            if (!initialized && (serverObject.serverID == initializedServerObjectID))
            {
                initialized = true;
                ClientManager.Instance.SendToClient(steamID, data, NetworkMessageType.NetworkBehaviourInitialized, Facepunch.Steamworks.Networking.SendType.Reliable);
                ClientManager.Instance.serverMessageEvents[NetworkMessageType.NetworkBehaviourInitialized] -= OnServerNetworkBehaviourInitialized;
                StartServer();
            }
        }

        private void OnClientNetworkBehaviourInitialized(byte[] data, ulong steamID)
        {
            int initializedServerObjectID = System.BitConverter.ToInt32(data, 0);

            if (!initialized && (serverObject.serverID == initializedServerObjectID))
            {
                initialized = true;
                ClientManager.Instance.clientMessageEvents[NetworkMessageType.NetworkBehaviourInitialized] -= OnClientNetworkBehaviourInitialized;
                StartClient();
            }
        }

        private void OnServerMessage(byte[] data, ulong steamID)
        {
            string message = System.Text.Encoding.UTF8.GetString(data);
            NetworkBehaviourMessage networkBehaviourMessage = JsonUtility.FromJson<NetworkBehaviourMessage>(message);

            // Call the function only if the message is for this instance
            if (serverObject.serverID == networkBehaviourMessage.serverID)
            {
                OnServerReceivedMessage(networkBehaviourMessage.message, steamID);
            }
        }

        private void OnClientMessage(byte[] data, ulong steamID)
        {
            string message = System.Text.Encoding.UTF8.GetString(data);
            NetworkBehaviourMessage networkBehaviourMessage = JsonUtility.FromJson<NetworkBehaviourMessage>(message);

            // Call the function only if the message is for this instance
            if (serverObject.serverID == networkBehaviourMessage.serverID)
            {
                OnClientReceivedMessage(networkBehaviourMessage.message, steamID);
            }
        }

        protected virtual void OnServerReceivedMessage(string message, ulong steamID)
        {
            // To be overwritten by the superclass
        }

        protected virtual void OnClientReceivedMessage(string message, ulong steamID)
        {
            // To be overwritten by the superclass
        }

        protected void SendToServer(string message, Facepunch.Steamworks.Networking.SendType sendType = Facepunch.Steamworks.Networking.SendType.Reliable)
        {
            NetworkBehaviourMessage networkBehaviourMessage = new NetworkBehaviourMessage(serverObject.serverID, message);
            byte[] data = System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(networkBehaviourMessage));
            ClientManager.Instance.SendToServer(data, networkMessageType, sendType);
        }

        protected void SendToClient(ulong steamID, string message, Facepunch.Steamworks.Networking.SendType sendType = Facepunch.Steamworks.Networking.SendType.Reliable)
        {
            NetworkBehaviourMessage networkBehaviourMessage = new NetworkBehaviourMessage(serverObject.serverID, message);
            byte[] data = System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(networkBehaviourMessage));
            ClientManager.Instance.SendToClient(steamID, data, networkMessageType, sendType);
        }

        protected void SendToAllClients(string message, Facepunch.Steamworks.Networking.SendType sendType = Facepunch.Steamworks.Networking.SendType.Reliable)
        {
            NetworkBehaviourMessage networkBehaviourMessage = new NetworkBehaviourMessage(serverObject.serverID, message);
            byte[] data = System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(networkBehaviourMessage));
            ClientManager.Instance.SendToAllClients(data, networkMessageType, sendType);
        }

        protected void OnDestroy()
        {
            if (serverObject.onServer)
            {
                ClientManager.Instance.serverMessageEvents[networkMessageType] -= OnServerMessage;
                OnDestroyServer();
            }
            else
            {
                ClientManager.Instance.clientMessageEvents[networkMessageType] -= OnClientMessage;
                OnDestroyClient();
            }
        }

        protected virtual void OnDestroyServer()
        {
            // To be overwritten by the superclass
        }

        protected virtual void OnDestroyClient()
        {
            // To be overwritten by the superclass
        }

        private void RemovePhysics()
        {
            var rigidBodyComponent = GetComponent<Rigidbody>();
            if(rigidBodyComponent != null)
            {
                Destroy(rigidBodyComponent);
            }
        }
    }
}
