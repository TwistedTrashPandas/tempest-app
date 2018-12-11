using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MastersOfTempest.Networking
{
    public class GameServer : MonoBehaviour
    {
        public static GameServer Instance = null;

        [Header("Server Parameters")]
        public float hz = 16;
        [Tooltip("Will not send objects if they didn't change their transform. Enabling can cause teleportation for objects that start moving after being static.")]
        [SerializeField]
        private bool onlySendChanges = true;

        private Dictionary<int, ServerObject> serverObjects = new Dictionary<int, ServerObject>();
        private HashSet<ulong> clientsReadyForInitialization = new HashSet<ulong>();
        private bool allClientsInitialized = false;

        // Make it possible to let other scripts subscribe to these events
        private System.Action serverInitializedEvents;

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
            // Pause everything until all clients are initialized
            Time.timeScale = 0;

            // Instantiate should create objects in the server scene by default
            UnityEngine.SceneManagement.SceneManager.SetActiveScene(gameObject.scene);

            NetworkManager.Instance.serverMessageEvents[NetworkMessageType.NetworkBehaviour] += OnMessageNetworkBehaviour;
            NetworkManager.Instance.serverMessageEvents[NetworkMessageType.NetworkBehaviourInitialized] += OnMessageNetworkBehaviourInitialized;
            NetworkManager.Instance.serverMessageEvents[NetworkMessageType.Initialization] += OnMessageInitialization;
            NetworkManager.Instance.serverMessageEvents[NetworkMessageType.PingPong] += OnMessagePingPong;
        }

        /// <summary>
        /// Sends a list of serverObjects to all clients with the server tick rate (hz).
        /// </summary>
        /// <returns></returns>
        IEnumerator ServerUpdate()
        {
            Time.timeScale = 1;

            while (true)
            {
                yield return new WaitForSecondsRealtime(1.0f / hz);

                SendAllServerObjects(onlySendChanges, Facepunch.Steamworks.Networking.SendType.Unreliable);
            }
        }

        private int GetHierarchyDepthOfTransform (Transform transform, int depth)
        {
            if (transform.parent != null)
            {
                return GetHierarchyDepthOfTransform(transform.parent, 1 + depth);
            }

            return depth;
        }

        private void SendAllServerObjects (bool onlySendChangedTransforms, Facepunch.Steamworks.Networking.SendType sendType)
        {
            // Save all server object messages that need to be sended into one pool
            // Sort the pool by the depth of the transform in the hierarchy
            // This makes sure that parents are instantiated before their children
            List<KeyValuePair<int, ServerObject>> serverObjectsToSend = new List<KeyValuePair<int, ServerObject>>();

            foreach (ServerObject serverObject in serverObjects.Values)
            {
                if (!onlySendChangedTransforms || serverObject.transform.hasChanged)
                {
                    serverObject.transform.hasChanged = false;
                    KeyValuePair<int, ServerObject> serverObjectToAdd = new KeyValuePair<int, ServerObject>(GetHierarchyDepthOfTransform(serverObject.transform, 0), serverObject);
                    serverObjectsToSend.Add(serverObjectToAdd);
                }
            }

            // Sort by the depth of the transform
            serverObjectsToSend.Sort
            (
                delegate (KeyValuePair<int, ServerObject> a, KeyValuePair<int, ServerObject> b)
                {
                    return a.Key - b.Key;
                }
            );

            // Create and send server object list messages until the pool is empty
            while (serverObjectsToSend.Count > 0)
            {
                // Make sure that the message is small enough to fit into the UDP packet (1200 bytes)
                MessageServerObjectList messageServerObjectList = new MessageServerObjectList();
                messageServerObjectList.messages = new MessageServerObject[14];
                messageServerObjectList.count = 0;

                for (int i = 0; i < messageServerObjectList.messages.Length; i++)
                {
                    if (serverObjectsToSend.Count > 0)
                    {
                        messageServerObjectList.messages[i] = new MessageServerObject(serverObjectsToSend[0].Value);
                        messageServerObjectList.count++;
                        serverObjectsToSend.RemoveAt(0);
                    }
                    else
                    {
                        break;
                    }
                }

                // Send the message to all clients
                byte[] data = ByteSerializer.GetBytes(messageServerObjectList);
                NetworkManager.Instance.SendToAllClients(data, NetworkMessageType.ServerObjectList, sendType);
            }
        }

        public void RegisterAndSendMessageServerObject (ServerObject serverObject)
        {
            if (allClientsInitialized)
            {
                // Make sure that objects are spawned on the server (with UDP it could happen that they don't spawn)
                byte[] data = ByteSerializer.GetBytes(new MessageServerObject(serverObject));
                NetworkManager.Instance.SendToAllClients(data, NetworkMessageType.ServerObject, Facepunch.Steamworks.Networking.SendType.Reliable);
            }

            serverObjects.Add(serverObject.serverID, serverObject);
        }

        public void RemoveServerObject (ServerObject serverObject)
        {
            serverObjects.Remove(serverObject.serverID);
        }

        public void SendMessageDestroyServerObject(ServerObject serverObject)
        {
            byte[] data = System.BitConverter.GetBytes(serverObject.serverID);
            NetworkManager.Instance.SendToAllClients(data, NetworkMessageType.DestroyServerObject, Facepunch.Steamworks.Networking.SendType.Reliable);
        }

        void OnMessageInitialization(byte[] data, ulong steamID)
        {
            // Only start the server loop if all the clients have loaded the scene and sent the message
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
                allClientsInitialized = true;

                // Make sure that all the objects on the server are spawned for all clients
                SendAllServerObjects(false, Facepunch.Steamworks.Networking.SendType.Reliable);
                NetworkManager.Instance.serverMessageEvents[NetworkMessageType.Initialization] -= OnMessageInitialization;

                // Answer to all the clients that the initialization finished
                // This works because the messages are reliable and in order (meaning all the objects on the client must have spawned when this message arrives)
                NetworkManager.Instance.SendToAllClients(data, NetworkMessageType.Initialization, Facepunch.Steamworks.Networking.SendType.Reliable);

                // Start the server loop and invoke all subscribed actions
                StartCoroutine(ServerUpdate());
                serverInitializedEvents?.Invoke();
            }
        }

        void OnMessagePingPong(byte[] data, ulong steamID)
        {
            NetworkManager.Instance.SendToClient(steamID, data, NetworkMessageType.PingPong, Facepunch.Steamworks.Networking.SendType.Unreliable);
        }

        void OnMessageNetworkBehaviour(byte[] data, ulong steamID)
        {
            MessageNetworkBehaviour message = ByteSerializer.FromBytes<MessageNetworkBehaviour>(data);
            byte[] messageData = new byte[message.dataLength];
            System.Array.Copy(message.data, messageData, message.dataLength);

            serverObjects[message.serverID].HandleNetworkBehaviourMessage(message.typeID, message.data, steamID);
        }

        void OnMessageNetworkBehaviourInitialized(byte[] data, ulong steamID)
        {
            MessageNetworkBehaviourInitialized message = ByteSerializer.FromBytes<MessageNetworkBehaviourInitialized>(data);
            serverObjects[message.serverID].HandleNetworkBehaviourInitializedMessage(message.typeID, steamID);
        }

        /// <summary>
        /// Add an action that is called when the server is initialized.
        /// Make sure that you unsubscribe and that the object with this script is on the server.
        /// </summary>
        /// <param name="clientInitializedAction">The action to be called</param>
        public void SubscribeToServerInitializedAction(System.Action serverInitializedAction)
        {
            serverInitializedEvents += serverInitializedAction;
        }

        public void UnsubscribeFromServerInitializedAction(System.Action serverInitializedAction)
        {
            serverInitializedEvents -= serverInitializedAction;
        }

        void OnDestroy()
        {
            NetworkManager.Instance.serverMessageEvents[NetworkMessageType.NetworkBehaviour] -= OnMessageNetworkBehaviour;
            NetworkManager.Instance.serverMessageEvents[NetworkMessageType.NetworkBehaviourInitialized] -= OnMessageNetworkBehaviourInitialized;
            NetworkManager.Instance.serverMessageEvents[NetworkMessageType.PingPong] -= OnMessagePingPong;
        }
    }
}
