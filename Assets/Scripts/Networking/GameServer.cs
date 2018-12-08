using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MastersOfTempest.Networking
{
    public class GameServer : MonoBehaviour
    {
        [Header("Server Parameters")]
        public float hz = 16;
        [Tooltip("Will not send objects if they didn't change their transform. Enabling can cause teleportation for objects that start moving after being static.")]
        public bool onlySendChangedTransfroms = true;

        public static GameServer Instance = null;
        public LinkedList<ServerObject> serverObjects = new LinkedList<ServerObject>();

        private int numClientsReadyForStartServer = 0;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Debug.LogError("GameServer cannot have multiple instances!");
                Destroy(gameObject);
            }
        }

        void Start()
        {
            ClientManager.Instance.serverMessageEvents[NetworkMessageType.StartServer] += OnMessageStartServer;
            ClientManager.Instance.serverMessageEvents[NetworkMessageType.PingPong] += OnMessagePingPong;
        }

        /// <summary>
        /// Sends a list of serverObjects to all clients with the server tick rate (hz).
        /// </summary>
        /// <returns></returns>
        IEnumerator ServerUpdate()
        {
            // Send all objects once to make sure that they have spawned
            SendAllServerObjects(Facepunch.Steamworks.Networking.SendType.Reliable);

            while (true)
            {
                yield return new WaitForSeconds(1.0f / hz);

                SendAllServerObjects(Facepunch.Steamworks.Networking.SendType.Unreliable);
            }
        }

        private void SendAllServerObjects (Facepunch.Steamworks.Networking.SendType sendType)
        {
            // Save all server object messages that need to be sended into one pool
            LinkedList<MessageServerObject> messagesToSend = new LinkedList<MessageServerObject>();

            foreach (ServerObject serverObject in serverObjects)
            {
                if (!onlySendChangedTransfroms || serverObject.transform.hasChanged)
                {
                    serverObject.transform.hasChanged = false;
                    messagesToSend.AddLast(new MessageServerObject(serverObject));
                }
            }

            // Create and send server object list messages until the pool is empty
            while (messagesToSend.Count > 0)
            {
                // Make sure that the message is small enough to fit into the UDP packet (1200 bytes)
                MessageServerObjectList messageServerObjectList = new MessageServerObjectList();
                messageServerObjectList.messages = new MessageServerObject[10];
                messageServerObjectList.count = 0;

                for (int i = 0; i < messageServerObjectList.messages.Length; i++)
                {
                    if (messagesToSend.Count > 0)
                    {
                        messageServerObjectList.messages[i] = messagesToSend.Last.Value;
                        messageServerObjectList.count++;
                        messagesToSend.RemoveLast();
                    }
                    else
                    {
                        break;
                    }
                }

                // Send the message to all clients
                byte[] data = ByteSerializer.GetBytes(messageServerObjectList);
                ClientManager.Instance.SendToAllClients(data, NetworkMessageType.ServerObjectList, sendType);
            }
        }

        public void RegisterAndSendMessageServerObject (ServerObject serverObject)
        {
            // Make sure that objects are spawned on the server (with UDP it could happen that they don't spawn)
            byte[] data = ByteSerializer.GetBytes(new MessageServerObject(serverObject));
            ClientManager.Instance.SendToAllClients(data, NetworkMessageType.ServerObject, Facepunch.Steamworks.Networking.SendType.Reliable);
            serverObjects.AddLast(serverObject);
        }

        public void SendMessageDestroyServerObject(ServerObject serverObject)
        {
            byte[] data = System.BitConverter.GetBytes(serverObject.serverID);
            ClientManager.Instance.SendToAllClients(data, NetworkMessageType.DestroyServerObject, Facepunch.Steamworks.Networking.SendType.Reliable);
        }

        void OnMessageStartServer(byte[] data, ulong steamID)
        {
            // Only start the server if all the clients have loaded the scene and sent a message
            numClientsReadyForStartServer++;

            if (numClientsReadyForStartServer == ClientManager.Instance.GetClientCount())
            {
                StartCoroutine(ServerUpdate());
                ClientManager.Instance.serverMessageEvents[NetworkMessageType.StartServer] -= OnMessageStartServer;
            }
        }

        void OnMessagePingPong(byte[] data, ulong steamID)
        {
            ClientManager.Instance.SendToClient(steamID, data, NetworkMessageType.PingPong, Facepunch.Steamworks.Networking.SendType.Unreliable);
        }

        void OnDestroy()
        {
            ClientManager.Instance.serverMessageEvents[NetworkMessageType.PingPong] -= OnMessagePingPong;
        }
    }
}
