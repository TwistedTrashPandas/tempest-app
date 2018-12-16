﻿using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace MastersOfTempest.Networking
{
    [RequireComponent(typeof(ServerObject))]
    public abstract class NetworkBehaviour : MonoBehaviour
    {
        protected bool initialized = false;
        protected ServerObject serverObject;

        private int typeId;
        private HashSet<ulong> clientsReadyForInitialization = new HashSet<ulong>();

        protected virtual void Start()
        {
            initialized = false;
            serverObject = GetComponent<ServerObject>();
            typeId = NetworkManager.Instance.GetTypeIdOfNetworkBehaviour(GetType());

            // NetworkBehaviour messages are managed by the GameClient, GameServer and their ServerObjects
            serverObject.AddNetworkBehaviourEvents(typeId, OnReceivedMessageRaw, OnNetworkBehaviourInitialized);

            if (!serverObject.onServer)
            {
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

        IEnumerator SendNetworkBehaviourInitializedMessage()
        {
            while (!GameClient.Instance.IsInitialized())
            {
                // Wait until all objects from the server spawned before sending the initialized message
                // The NetworkBehaviour could otherwise try to access objects that did not spawn yet
                yield return new WaitForEndOfFrame();
            }

            // The NetworkBehaviour on the server has to be sure that this object spawned and listens to messages from the server
            MessageNetworkBehaviourInitialized message = new MessageNetworkBehaviourInitialized(serverObject.serverID, typeId);
            NetworkManager.Instance.SendToServer(ByteSerializer.GetBytes(message), NetworkMessageType.NetworkBehaviourInitialized, Facepunch.Steamworks.Networking.SendType.Reliable);
        }

        private void OnNetworkBehaviourInitialized(ulong steamID)
        {
            if (!initialized)
            {
                if (serverObject.onServer)
                {
                    // Only initialize if all clients are ready
                    bool allClientsReady = true;
                    clientsReadyForInitialization.Add(steamID);

                    ulong[] lobbyMemberIDs = NetworkManager.Instance.GetLobbyMemberIDs();

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
                        MessageNetworkBehaviourInitialized message = new MessageNetworkBehaviourInitialized(serverObject.serverID, typeId);
                        NetworkManager.Instance.SendToAllClients(ByteSerializer.GetBytes(message), NetworkMessageType.NetworkBehaviourInitialized, Facepunch.Steamworks.Networking.SendType.Reliable);
                        StartServer();
                    }
                }
                else
                {
                    initialized = true;
                    StartClient();
                }
            }
        }

        private void OnReceivedMessageRaw(byte[] data, ulong steamID)
        {
            if (serverObject.onServer)
            {
                OnServerReceivedMessageRaw(data, steamID);
            }
            else
            {
                OnClientReceivedMessageRaw(data, steamID);
            }
        }

        protected void SendToServer(byte[] data, Facepunch.Steamworks.Networking.SendType sendType)
        {
            MessageNetworkBehaviour message = new MessageNetworkBehaviour(serverObject.serverID, typeId, data);
            NetworkManager.Instance.SendToServer(message.ToBytes(), NetworkMessageType.NetworkBehaviour, sendType);
        }

        protected void SendToServer(string message, Facepunch.Steamworks.Networking.SendType sendType = Facepunch.Steamworks.Networking.SendType.Reliable)
        {
            SendToServer(System.Text.Encoding.UTF8.GetBytes(message), sendType);
        }

        protected void SendToClient(ulong steamID, byte[] data, Facepunch.Steamworks.Networking.SendType sendType)
        {
            MessageNetworkBehaviour message = new MessageNetworkBehaviour(serverObject.serverID, typeId, data);
            NetworkManager.Instance.SendToClient(steamID, message.ToBytes(), NetworkMessageType.NetworkBehaviour, sendType);
        }

        protected void SendToClient(ulong steamID, string message, Facepunch.Steamworks.Networking.SendType sendType = Facepunch.Steamworks.Networking.SendType.Reliable)
        {
            SendToClient(steamID, System.Text.Encoding.UTF8.GetBytes(message), sendType);
        }

        protected void SendToAllClients(byte[] data, Facepunch.Steamworks.Networking.SendType sendType)
        {
            MessageNetworkBehaviour message = new MessageNetworkBehaviour(serverObject.serverID, typeId, data);
            NetworkManager.Instance.SendToAllClients(message.ToBytes(), NetworkMessageType.NetworkBehaviour, sendType);
        }

        protected void SendToAllClients(string message, Facepunch.Steamworks.Networking.SendType sendType = Facepunch.Steamworks.Networking.SendType.Reliable)
        {
            SendToAllClients(System.Text.Encoding.UTF8.GetBytes(message), sendType);
        }

        protected void OnDestroy()
        {
            serverObject.RemoveNetworkBehaviourEvents(typeId);

            if (serverObject.onServer)
            {
                OnDestroyServer();
            }
            else
            {
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

        protected virtual void OnServerReceivedMessageRaw(byte[] data, ulong steamID)
        {
            // Can be overwritten by the subclass
            OnServerReceivedMessage(System.Text.Encoding.UTF8.GetString(data), steamID);
        }

        protected virtual void OnClientReceivedMessageRaw(byte[] data, ulong steamID)
        {
            // Can be overwritten by the subclass
            OnClientReceivedMessage(System.Text.Encoding.UTF8.GetString(data), steamID);
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
}
