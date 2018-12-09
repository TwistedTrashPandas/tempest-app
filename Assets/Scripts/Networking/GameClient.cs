﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Facepunch.Steamworks;

namespace MastersOfTempest.Networking
{
    public class GameClient : MonoBehaviour
    {
        public static GameClient Instance = null;

        // Use the gameobject instance id from the server to keep track of the objects
        public Dictionary<int, ServerObject> objectsFromServer = new Dictionary<int, ServerObject>();

        public float pingsPerSec = 1;
        public float ping = 0;

        private float lastPingTime = 0;

        // Handles all the incoming network behaviour messages from the server network behaviours
        private Dictionary<int, System.Action<byte[], ulong>> clientNetworkBehaviourEvents = new Dictionary<int, System.Action<byte[], ulong>>();
        private Dictionary<int, System.Action<ulong>> clientNetworkBehaviourInitializedEvents = new Dictionary<int, System.Action<ulong>>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Debug.LogError(nameof(GameClient) + " cannot have multiple instances!");
                Destroy(gameObject);
            }
        }

        void Start()
        {
            ClientManager.Instance.clientMessageEvents[NetworkMessageType.ServerObject] += OnMessageServerObject;
            ClientManager.Instance.clientMessageEvents[NetworkMessageType.ServerObjectList] += OnMessageServerObjectList;
            ClientManager.Instance.clientMessageEvents[NetworkMessageType.DestroyServerObject] += OnMessageDestroyServerObject;
            ClientManager.Instance.clientMessageEvents[NetworkMessageType.PingPong] += OnMessagePingPong;
            ClientManager.Instance.clientMessageEvents[NetworkMessageType.NetworkBehaviour] += OnMessageNetworkBehaviour;
            ClientManager.Instance.clientMessageEvents[NetworkMessageType.NetworkBehaviourInitialized] += OnMessageNetworkBehaviourInitialized;

            // Send a message to initialize the server
            byte[] data = System.Text.Encoding.UTF8.GetBytes("ClientReadyForInitialization");
            ClientManager.Instance.SendToServer(data, NetworkMessageType.ClientReadyForInitialization, Facepunch.Steamworks.Networking.SendType.Reliable);

            // Wait a bit before sending the message
            lastPingTime = Time.time + pingsPerSec;
        }

        void Update()
        {
            if (Time.time - lastPingTime > (1.0f / pingsPerSec))
            {
                byte[] data = System.BitConverter.GetBytes(Time.time);
                ClientManager.Instance.SendToServer(data, NetworkMessageType.PingPong, Facepunch.Steamworks.Networking.SendType.Unreliable);
                lastPingTime = Time.time;
            }
        }

        void OnMessageServerObject(byte[] data, ulong steamID)
        {
            MessageServerObject messageServerObject = ByteSerializer.FromBytes<MessageServerObject>(data);
            UpdateServerObject(messageServerObject);
        }

        void UpdateServerObject(MessageServerObject messageServerObject)
        {
            // Create a new object if it doesn't exist yet
            if (!objectsFromServer.ContainsKey(messageServerObject.instanceID))
            {
                // Make sure that the parent exists already if it has one
                if (!messageServerObject.hasParent || objectsFromServer.ContainsKey(messageServerObject.parentInstanceID))
                {
                    GameObject resource = Resources.Load<GameObject>("ServerObjects/" + messageServerObject.resourceName);
                    ServerObject tmp = Instantiate(resource).GetComponent<ServerObject>();
                    objectsFromServer[messageServerObject.instanceID] = tmp;

                    // Overwrite the layer so that the server camera does not see this object as well
                    tmp.onServer = false;
                    tmp.serverID = messageServerObject.instanceID;
                    tmp.gameObject.layer = LayerMask.NameToLayer("Client");

                    // Set the transform after spawn
                    tmp.transform.localPosition = messageServerObject.localPosition;
                    tmp.transform.localRotation = messageServerObject.localRotation;
                    tmp.transform.localScale = messageServerObject.localScale;
                }
            }

            ServerObject serverObject = objectsFromServer[messageServerObject.instanceID];

            if (serverObject.lastUpdate <= messageServerObject.time)
            {
                // Update values only if the UDP packet values are newer
                serverObject.name = messageServerObject.name + "\t\t(" + messageServerObject.instanceID + ")";
                serverObject.lastUpdate = messageServerObject.time;

                // Update the transform
                serverObject.UpdateTransformFromMessageServerObject(messageServerObject);

                // Update parent if possible
                if (messageServerObject.hasParent && objectsFromServer.ContainsKey(messageServerObject.parentInstanceID))
                {
                    serverObject.transform.SetParent(objectsFromServer[messageServerObject.parentInstanceID].transform, false);
                }
            }
        }

        void OnMessageServerObjectList (byte[] data, ulong steamID)
        {
            MessageServerObjectList messageServerObjectList = ByteSerializer.FromBytes<MessageServerObjectList>(data);

            for (int i = 0; i < messageServerObjectList.count; i++)
            {
                UpdateServerObject(messageServerObjectList.messages[i]);
            }
        }

        void OnMessageDestroyServerObject(byte[] data, ulong steamID)
        {
            int serverIDToDestroy = System.BitConverter.ToInt32(data, 0);

            if (objectsFromServer.ContainsKey(serverIDToDestroy))
            {
                Destroy(objectsFromServer[serverIDToDestroy].gameObject);
                objectsFromServer.Remove(serverIDToDestroy);
            }
        }

        void OnMessagePingPong(byte[] data, ulong steamID)
        {
            ping = Time.time - System.BitConverter.ToSingle(data, 0);
        }

        void OnMessageNetworkBehaviour(byte[] data, ulong steamID)
        {
            NetworkBehaviourMessage message = ByteSerializer.FromBytes<NetworkBehaviourMessage>(data);
            byte[] messageData = new byte[message.dataLength];
            System.Array.Copy(message.data, messageData, message.dataLength);

            if (clientNetworkBehaviourEvents.ContainsKey(message.serverID))
            {
                clientNetworkBehaviourEvents[message.serverID].Invoke(messageData, steamID);
            }
        }

        void OnMessageNetworkBehaviourInitialized(byte[] data, ulong steamID)
        {
            int serverID = System.BitConverter.ToInt32(data, 0);
            clientNetworkBehaviourInitializedEvents[serverID].Invoke(steamID);
        }

        public void AddNetworkBehaviourEvents(int serverID, System.Action<byte[], ulong> behaviourAction, System.Action<ulong> initializedAction)
        {
            clientNetworkBehaviourEvents[serverID] = behaviourAction;
            clientNetworkBehaviourInitializedEvents[serverID] = initializedAction;
        }

        public void RemoveNetworkBehaviourEvents(int serverID)
        {
            clientNetworkBehaviourEvents.Remove(serverID);
            clientNetworkBehaviourInitializedEvents.Remove(serverID);
        }

        void OnDestroy()
        {
            ClientManager.Instance.clientMessageEvents[NetworkMessageType.ServerObject] -= OnMessageServerObject;
            ClientManager.Instance.clientMessageEvents[NetworkMessageType.ServerObjectList] -= OnMessageServerObjectList;
            ClientManager.Instance.clientMessageEvents[NetworkMessageType.DestroyServerObject] -= OnMessageDestroyServerObject;
            ClientManager.Instance.clientMessageEvents[NetworkMessageType.PingPong] -= OnMessagePingPong;
            ClientManager.Instance.clientMessageEvents[NetworkMessageType.NetworkBehaviour] -= OnMessageNetworkBehaviour;
            ClientManager.Instance.clientMessageEvents[NetworkMessageType.NetworkBehaviourInitialized] -= OnMessageNetworkBehaviourInitialized;
        }
    }
}
