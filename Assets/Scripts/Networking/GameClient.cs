using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Facepunch.Steamworks;

namespace MastersOfTempest.Networking
{
    public class GameClient : MonoBehaviour
    {
        // Use the gameobject instance id from the server to keep track of the objects
        public Dictionary<int, ServerObject> objectsFromServer = new Dictionary<int, ServerObject>();

        public float pingsPerSec = 1;
        public float ping = 0;

        private float lastPingTime = 0;

        void Start()
        {
            ClientManager.Instance.clientMessageEvents[NetworkMessageType.ServerObject] += OnMessageServerObject;
            ClientManager.Instance.clientMessageEvents[NetworkMessageType.DestroyGameObject] += OnMessageDestroyGameObject;
            ClientManager.Instance.clientMessageEvents[NetworkMessageType.PingPong] += OnMessagePingPong;

            // Wait a bit before sending the message
            lastPingTime = Time.time + pingsPerSec;
        }

        void Update()
        {
            if (Time.time - lastPingTime > (1.0f / pingsPerSec))
            {
                ClientManager.Instance.SendToServer(Time.time.ToString(), NetworkMessageType.PingPong, Facepunch.Steamworks.Networking.SendType.Unreliable);
                lastPingTime = Time.time;
            }
        }

        void OnMessageServerObject(string message, ulong steamID)
        {
            MessageServerObject messageServerObject = JsonUtility.FromJson<MessageServerObject>(message);

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
                    tmp.lastUpdate = messageServerObject.time;
                    tmp.serverID = messageServerObject.instanceID;
                    tmp.gameObject.layer = LayerMask.NameToLayer("Client");
                }
            }

            ServerObject serverObject = objectsFromServer[messageServerObject.instanceID];

            if (serverObject.lastUpdate <= messageServerObject.time)
            {
                // Update values only if the UDP packet values are newer
                serverObject.name = messageServerObject.name + "\t\t(" + messageServerObject.instanceID + ")";
                serverObject.lastUpdate = messageServerObject.time;
                serverObject.transform.localPosition = messageServerObject.localPosition;
                serverObject.transform.localRotation = messageServerObject.localRotation;
                serverObject.transform.localScale = messageServerObject.localScale;

                if (messageServerObject.hasParent && objectsFromServer.ContainsKey(messageServerObject.parentInstanceID))
                {
                    serverObject.transform.SetParent(objectsFromServer[messageServerObject.parentInstanceID].transform, false);
                }
            }
        }

        void OnMessageDestroyGameObject(string message, ulong steamID)
        {
            MessageDestroyServerObject destroyTransformMessage = JsonUtility.FromJson<MessageDestroyServerObject>(message);

            if (objectsFromServer.ContainsKey(destroyTransformMessage.instanceID))
            {
                Destroy(objectsFromServer[destroyTransformMessage.instanceID].gameObject);
                objectsFromServer.Remove(destroyTransformMessage.instanceID);
            }
        }

        void OnMessagePingPong(string message, ulong steamID)
        {
            ping = Time.time - float.Parse(message);
        }

        void OnDestroy()
        {
            ClientManager.Instance.clientMessageEvents[NetworkMessageType.ServerObject] -= OnMessageServerObject;
            ClientManager.Instance.clientMessageEvents[NetworkMessageType.DestroyGameObject] -= OnMessageDestroyGameObject;
            ClientManager.Instance.clientMessageEvents[NetworkMessageType.PingPong] -= OnMessagePingPong;
        }
    }
}
