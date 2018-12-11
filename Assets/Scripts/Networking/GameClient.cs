using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Facepunch.Steamworks;

namespace MastersOfTempest.Networking
{
    public class GameClient : MonoBehaviour
    {
        public static GameClient Instance = null;

        public float pingsPerSec = 1;

        // Stores all the server object prefabs based on their resource id
        private Dictionary<int, GameObject> serverObjectPrefabs = new Dictionary<int, GameObject>();

        // Use the gameobject instance id from the server to keep track of the objects
        private Dictionary<int, ServerObject> objectsFromServer = new Dictionary<int, ServerObject>();

        // Make it possible to let other scripts subscribe to these events
        private System.Action clientInitializedEvents;

        [SerializeField]
        private bool initialized = false;
        [SerializeField]
        private float ping = 0;
        private float lastPingTime = 0;

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

            // Load all the prefabs for server objects
            ServerObject[] serverObjectResources = Resources.LoadAll<ServerObject>("ServerObjects/");

            foreach (ServerObject s in serverObjectResources)
            {
                if (s.resourceID >= 0 && !serverObjectPrefabs.ContainsKey(s.resourceID))
                {
                    serverObjectPrefabs[s.resourceID] = s.gameObject;
                }
                else
                {
                    Debug.LogError("Server object " + s.name + " does not have a valid resource id!");
                }
            }
        }

        void Start()
        {
            NetworkManager.Instance.clientMessageEvents[NetworkMessageType.ServerObject] += OnMessageServerObject;
            NetworkManager.Instance.clientMessageEvents[NetworkMessageType.ServerObjectList] += OnMessageServerObjectList;
            NetworkManager.Instance.clientMessageEvents[NetworkMessageType.DestroyServerObject] += OnMessageDestroyServerObject;
            NetworkManager.Instance.clientMessageEvents[NetworkMessageType.PingPong] += OnMessagePingPong;
            NetworkManager.Instance.clientMessageEvents[NetworkMessageType.NetworkBehaviour] += OnMessageNetworkBehaviour;
            NetworkManager.Instance.clientMessageEvents[NetworkMessageType.NetworkBehaviourInitialized] += OnMessageNetworkBehaviourInitialized;
            NetworkManager.Instance.clientMessageEvents[NetworkMessageType.Initialization] += OnMessageInitialization;

            StartCoroutine(SendInitializationMessage());
        }

        void Update()
        {
            if (Time.time - lastPingTime > (1.0f / pingsPerSec))
            {
                byte[] data = System.BitConverter.GetBytes(Time.time);
                NetworkManager.Instance.SendToServer(data, NetworkMessageType.PingPong, Facepunch.Steamworks.Networking.SendType.Unreliable);
                lastPingTime = Time.time;
            }
        }

        IEnumerator SendInitializationMessage ()
        {
            while (!initialized)
            {
                // Send a message to initialize the server
                byte[] data = System.Text.Encoding.UTF8.GetBytes("Initialization");
                NetworkManager.Instance.SendToServer(data, NetworkMessageType.Initialization, Facepunch.Steamworks.Networking.SendType.Reliable);

                yield return new WaitForSecondsRealtime(0.5f);
            }
        }

        void OnMessageInitialization(byte[] data, ulong steamID)
        {
            if (!initialized)
            {
                initialized = true;
                clientInitializedEvents?.Invoke();
                NetworkManager.Instance.clientMessageEvents[NetworkMessageType.Initialization] -= OnMessageInitialization;
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
                    ServerObject tmp = Instantiate(serverObjectPrefabs[messageServerObject.resourceID]).GetComponent<ServerObject>();
                    objectsFromServer[messageServerObject.instanceID] = tmp;

                    // Set attributes, also update transform after spawn
                    tmp.onServer = false;
                    tmp.serverID = messageServerObject.instanceID;
                    tmp.transform.localPosition = messageServerObject.localPosition;
                    tmp.transform.localRotation = messageServerObject.localRotation;
                    tmp.transform.localScale = messageServerObject.localScale;
                }
            }

            ServerObject serverObject = objectsFromServer[messageServerObject.instanceID];

            if (serverObject.lastUpdate <= messageServerObject.time)
            {
                // Update values only if the UDP packet values are newer
                serverObject.name = "[" + messageServerObject.instanceID + "]\t" + messageServerObject.name;
                serverObject.lastUpdate = messageServerObject.time;

                // Update the transform
                serverObject.UpdateTransformFromMessageServerObject(messageServerObject);

                // Update parent if possible
                if (messageServerObject.hasParent)
                {
                    if (objectsFromServer.ContainsKey(messageServerObject.parentInstanceID))
                    {
                        serverObject.transform.SetParent(objectsFromServer[messageServerObject.parentInstanceID].transform, false);
                    }
                }
                else
                {
                    serverObject.transform.SetParent(null);
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
            MessageNetworkBehaviour message = ByteSerializer.FromBytes<MessageNetworkBehaviour>(data);
            byte[] messageData = new byte[message.dataLength];
            System.Array.Copy(message.data, messageData, message.dataLength);

            objectsFromServer[message.serverID].HandleNetworkBehaviourMessage(message.typeID, message.data, steamID);
        }

        void OnMessageNetworkBehaviourInitialized(byte[] data, ulong steamID)
        {
            MessageNetworkBehaviourInitialized message = ByteSerializer.FromBytes<MessageNetworkBehaviourInitialized>(data);
            objectsFromServer[message.serverID].HandleNetworkBehaviourInitializedMessage(message.typeID, steamID);
        }

        /// <summary>
        /// Add an action that is called when the client is initialized.
        /// Make sure that you unsubscribe and that the object with this script is on the client.
        /// </summary>
        /// <param name="clientInitializedAction">The action to be called</param>
        public void SubscribeToClientInitializedAction(System.Action clientInitializedAction)
        {
            clientInitializedEvents += clientInitializedAction;
        }

        public void UnsubscribeFromClientInitializedAction(System.Action clientInitializedAction)
        {
            clientInitializedEvents -= clientInitializedAction;
        }

        public bool IsInitialized ()
        {
            return initialized;
        }

        public float GetPing ()
        {
            return ping;
        }

        void OnDestroy()
        {
            NetworkManager.Instance.clientMessageEvents[NetworkMessageType.ServerObject] -= OnMessageServerObject;
            NetworkManager.Instance.clientMessageEvents[NetworkMessageType.ServerObjectList] -= OnMessageServerObjectList;
            NetworkManager.Instance.clientMessageEvents[NetworkMessageType.DestroyServerObject] -= OnMessageDestroyServerObject;
            NetworkManager.Instance.clientMessageEvents[NetworkMessageType.PingPong] -= OnMessagePingPong;
            NetworkManager.Instance.clientMessageEvents[NetworkMessageType.NetworkBehaviour] -= OnMessageNetworkBehaviour;
            NetworkManager.Instance.clientMessageEvents[NetworkMessageType.NetworkBehaviourInitialized] -= OnMessageNetworkBehaviourInitialized;
        }
    }
}
