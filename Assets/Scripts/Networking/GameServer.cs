using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Facepunch.Steamworks;

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
            StartCoroutine(ServerUpdate());

            ClientManager.Instance.serverMessageEvents[NetworkMessageType.PingPong] += OnMessagePingPong;
        }

        /// <summary>
        /// Sends a list of serverObjects to all clients with the server tick rate (hz).
        /// </summary>
        /// <returns></returns>
        IEnumerator ServerUpdate()
        {
            while (true)
            {
                yield return new WaitForSeconds(1.0f / hz);

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
                    MessageServerObjectList messageServerObjectList = new MessageServerObjectList();
                    messageServerObjectList.messages = new List<string>();

                    // Make sure that the message is small enough to fit into the UDP packet (1200 bytes)
                    while (messagesToSend.Count > 0)
                    {
                        // Add a message from the pool in order to check the size of the message
                        string tmp = JsonUtility.ToJson(messagesToSend.Last.Value);
                        messageServerObjectList.messages.Add(tmp);

                        // Get the size of the message
                        string json = JsonUtility.ToJson(messageServerObjectList);
                        byte[] data = System.Text.Encoding.UTF8.GetBytes(json);

                        if (data.Length > 1200)
                        {
                            // The message is too big, remove the last message, send it and create a new one
                            messageServerObjectList.messages.Remove(tmp);
                            break;
                        }
                        else
                        {
                            // Remove the message from the pool and add the next message
                            messagesToSend.RemoveLast();
                        }
                    }

                    // Send the message to all clients
                    string message = JsonUtility.ToJson(messageServerObjectList);
                    ClientManager.Instance.SendToAllClients(message, NetworkMessageType.ServerObjectList, Facepunch.Steamworks.Networking.SendType.Unreliable);
                }
            }
        }

        public void RegisterAndSendMessageServerObject (ServerObject serverObject)
        {
            // Make sure that objects are spawned on the server (with UDP it could happen that they don't spawn)
            string message = JsonUtility.ToJson(new MessageServerObject(serverObject));
            ClientManager.Instance.SendToAllClients(message, NetworkMessageType.ServerObject, Facepunch.Steamworks.Networking.SendType.Reliable);
            serverObjects.AddLast(serverObject);
        }

        public void SendMessageDestroyServerObject(ServerObject serverObject)
        {
            string message = JsonUtility.ToJson(new MessageDestroyServerObject(serverObject));
            ClientManager.Instance.SendToAllClients(message, NetworkMessageType.DestroyServerObject, Facepunch.Steamworks.Networking.SendType.Reliable);
        }

        void OnMessagePingPong(string message, ulong steamID)
        {
            ClientManager.Instance.SendToClient(steamID, message, NetworkMessageType.PingPong, Facepunch.Steamworks.Networking.SendType.Unreliable);
        }

        void OnDestroy()
        {
            ClientManager.Instance.serverMessageEvents[NetworkMessageType.PingPong] -= OnMessagePingPong;
        }
    }
}
