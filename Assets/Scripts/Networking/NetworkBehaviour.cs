using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace MastersOfTempest.Networking
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ServerObject))]
    public class NetworkBehaviour : MonoBehaviour
    {
        protected bool initialized = false;
        protected ServerObject serverObject;

        private HashSet<ulong> clientsReadyForInitialization = new HashSet<ulong>();

        protected virtual void Start()
        {
            initialized = false;
            serverObject = GetComponent<ServerObject>();

            if (serverObject.onServer)
            {
                // NetworkBehaviour messages are managed by the GameClient and GameServer, add the handlers for the mesaages
                // The serverID is the same as the instance ID of the transform but might not have been set on the server object yet
                GameServer.Instance.AddNetworkBehaviourEvents(transform.GetInstanceID(), OnServerReceivedMessageRaw, OnServerNetworkBehaviourInitialized);
            }
            else
            {
                GameClient.Instance.AddNetworkBehaviourEvents(serverObject.serverID, OnClientReceivedMessageRaw, OnClientNetworkBehaviourInitialized);

                // Begin the initialization, tell the server that this object is ready
                StartCoroutine(SendNetworkBehaviourInitializedMessage());
            }
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

        IEnumerator SendNetworkBehaviourInitializedMessage ()
        {
            while (!GameClient.Instance.IsInitialized())
            {
                // Wait until all objects from the server spawned before sending the initialized message
                // The NetworkBehaviour could otherwise try to access objects that did not spawn yet
                yield return new WaitForEndOfFrame();
            }

            // The NetworkBehaviour on the server has to be sure that this object spawned and listens to messages from the server
            byte[] data = System.BitConverter.GetBytes(serverObject.serverID);
            ClientManager.Instance.SendToServer(data, NetworkMessageType.NetworkBehaviourInitialized, Facepunch.Steamworks.Networking.SendType.Reliable);
        }

        private void OnServerNetworkBehaviourInitialized(ulong steamID)
        {
            if (!initialized)
            {
                // Only initialize if all clients are ready
                bool allClientsReady = true;
                clientsReadyForInitialization.Add(steamID);

                ulong[] lobbyMemberIDs = ClientManager.Instance.GetLobbyMemberIDs();

                foreach (ulong id in lobbyMemberIDs)
                {
                    if (!clientsReadyForInitialization.Contains(id))
                    {
                        allClientsReady = false;
                        break;
                    }
                }

                if (allClientsReady)
                {
                    // All clients are ready to be initialized, send a message to initialize all of them at the same time
                    initialized = true;
                    byte[] data = System.BitConverter.GetBytes(serverObject.serverID);
                    ClientManager.Instance.SendToAllClients(data, NetworkMessageType.NetworkBehaviourInitialized, Facepunch.Steamworks.Networking.SendType.Reliable);
                    StartServer();
                }
            }
        }

        private void OnClientNetworkBehaviourInitialized(ulong steamID)
        {
            if (!initialized)
            {
                initialized = true;
                StartClient();
            }
        }

        // Can be overridden by the subclass in order to access the bytes directly
        protected virtual void OnServerReceivedMessageRaw(byte[] data, ulong steamID)
        {
            string message = System.Text.Encoding.UTF8.GetString(data);
            OnServerReceivedMessage(message, steamID);
        }

        // Can be overridden by the subclass in order to access the bytes directly
        protected virtual void OnClientReceivedMessageRaw(byte[] data, ulong steamID)
        {
            string message = System.Text.Encoding.UTF8.GetString(data);
            OnClientReceivedMessage(message, steamID);
        }

        protected void SendToServer(byte[] data, Facepunch.Steamworks.Networking.SendType sendType)
        {
            NetworkBehaviourMessage networkBehaviourMessage = new NetworkBehaviourMessage(serverObject.serverID, data);
            ClientManager.Instance.SendToServer(ByteSerializer.GetBytes(networkBehaviourMessage), NetworkMessageType.NetworkBehaviour, sendType);
        }

        protected void SendToServer(string message, Facepunch.Steamworks.Networking.SendType sendType = Facepunch.Steamworks.Networking.SendType.Reliable)
        {
            SendToServer(System.Text.Encoding.UTF8.GetBytes(message), sendType);
        }

        protected void SendToClient(ulong steamID, byte[] data, Facepunch.Steamworks.Networking.SendType sendType)
        {
            NetworkBehaviourMessage networkBehaviourMessage = new NetworkBehaviourMessage(serverObject.serverID, data);
            ClientManager.Instance.SendToClient(steamID, ByteSerializer.GetBytes(networkBehaviourMessage), NetworkMessageType.NetworkBehaviour, sendType);
        }

        protected void SendToClient(ulong steamID, string message, Facepunch.Steamworks.Networking.SendType sendType = Facepunch.Steamworks.Networking.SendType.Reliable)
        {
            SendToClient(steamID, System.Text.Encoding.UTF8.GetBytes(message), sendType);
        }

        protected void SendToAllClients(byte[] data, Facepunch.Steamworks.Networking.SendType sendType)
        {
            NetworkBehaviourMessage networkBehaviourMessage = new NetworkBehaviourMessage(serverObject.serverID, data);
            ClientManager.Instance.SendToAllClients(ByteSerializer.GetBytes(networkBehaviourMessage), NetworkMessageType.NetworkBehaviour, sendType);
        }

        protected void SendToAllClients(string message, Facepunch.Steamworks.Networking.SendType sendType = Facepunch.Steamworks.Networking.SendType.Reliable)
        {
            SendToAllClients(System.Text.Encoding.UTF8.GetBytes(message), sendType);
        }

        protected void OnDestroy()
        {
            if (serverObject.onServer)
            {
                GameServer.Instance.RemoveNetworkBehaviourEvents(serverObject.serverID);
                OnDestroyServer();
            }
            else
            {
                GameClient.Instance.RemoveNetworkBehaviourEvents(serverObject.serverID);
                OnDestroyClient();
            }
        }

        protected virtual void StartServer()
        {
            // To be overwritten by the subclass
        }

        protected virtual void StartClient()
        {
            // To be overwritten by the subclass
        }

        protected virtual void UpdateServer()
        {
            // To be overwritten by the subclass
        }

        protected virtual void UpdateClient()
        {
            // To be overwritten by the subclass
        }

        protected virtual void OnServerReceivedMessage(string message, ulong steamID)
        {
            // To be overwritten by the subclass
        }

        protected virtual void OnClientReceivedMessage(string message, ulong steamID)
        {
            // To be overwritten by the subclass
        }

        protected virtual void OnDestroyServer()
        {
            // To be overwritten by the subclass
        }

        protected virtual void OnDestroyClient()
        {
            // To be overwritten by the subclass
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct NetworkBehaviourMessage
    {
        public int serverID;                                        // 4 bytes
        public int dataLength;                                      // 4 bytes
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1192)]
        public byte[] data;                                         // 1192 bytes
                                                                    // 1200 bytes

        public NetworkBehaviourMessage(int serverID, byte[] data)
        {
            this.serverID = serverID;

            this.data = new byte[1192];
            System.Array.Copy(data, this.data, data.Length);
            this.dataLength = data.Length;
        }
    };
}
