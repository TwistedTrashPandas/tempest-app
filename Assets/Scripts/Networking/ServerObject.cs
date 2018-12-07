using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MastersOfTempest.Networking
{
    public class ServerObject : MonoBehaviour
    {
        public string resourceName = "";

        public bool onServer = true;
        public int serverID = 0;
        public float lastUpdate = 0;

        [Header("Client Parameters")]
        public bool interpolateOnClient = true;

        // Interpolation variables
        private MessageServerObject currentMessage;
        private MessageServerObject lastMessage;
        private float timeSinceLastMessage = 0;
        private uint messageCount = 0;

        void Start()
        {
            if (onServer)
            {
                // Check if the resource name is valid
                if (Resources.Load<GameObject>("ServerObjects/" + resourceName) == null)
                {
                    Debug.LogError("Cannot find resource \"" + resourceName + "\" of gameobject " + name);
                }

                // Set server ID
                serverID = transform.GetInstanceID();

                // Register to game server
                GameServer.Instance.RegisterAndSendMessageServerObject(this);
            }
        }

        void Update()
        {
            if (!onServer && interpolateOnClient)
            {
                // Make sure that both messages exist
                if (messageCount > 1)
                {
                    // Interpolate between the transform from the last and the current message based on the time that passed since the last message
                    // This introduces a bit of latency but does not require any prediction
                    float dt = currentMessage.time - lastMessage.time;

                    if (dt > 0)
                    {
                        float interpolationFactor = timeSinceLastMessage / dt;
                        transform.localPosition = Vector3.Lerp(lastMessage.localPosition, currentMessage.localPosition, interpolationFactor);
                        transform.localRotation = Quaternion.Lerp(lastMessage.localRotation, currentMessage.localRotation, interpolationFactor);
                        transform.localScale = Vector3.Lerp(lastMessage.localScale, currentMessage.localScale, interpolationFactor);
                        timeSinceLastMessage += Time.deltaTime;
                    }
                }
            }
        }

        void OnDestroy()
        {
            if (onServer)
            {
                // Send destroy message
                GameServer.Instance.serverObjects.Remove(this);
                GameServer.Instance.SendMessageDestroyServerObject(this);
            }
        }

        public void UpdateTransformFromMessageServerObject (MessageServerObject messageServerObject)
        {
            if (!onServer)
            {
                if (interpolateOnClient)
                {
                    // Save data for interpolation
                    lastMessage = currentMessage;
                    currentMessage = messageServerObject;
                    timeSinceLastMessage = 0;
                    messageCount++;
                }
                else
                {
                    // Directly update the transform
                    transform.localPosition = messageServerObject.localPosition;
                    transform.localRotation = messageServerObject.localRotation;
                    transform.localScale = messageServerObject.localScale;
                }
            }
        }
    }

    [System.Serializable]
    public struct MessageServerObject
    {
        public float time;
        public string name;
        public string resourceName;
        public bool hasParent;
        public int parentInstanceID;
        public int instanceID;
        public Vector3 localPosition;
        public Quaternion localRotation;
        public Vector3 localScale;

        public MessageServerObject(ServerObject serverObject)
        {
            if (serverObject.transform.parent != null)
            {
                parentInstanceID = serverObject.transform.parent.GetInstanceID();
                hasParent = true;
            }
            else
            {
                parentInstanceID = 0;
                hasParent = false;
            }

            time = serverObject.lastUpdate = Time.time;
            name = serverObject.name;
            resourceName = serverObject.resourceName;
            instanceID = serverObject.transform.GetInstanceID();
            localPosition = serverObject.transform.localPosition;
            localRotation = serverObject.transform.localRotation;
            localScale = serverObject.transform.localScale;
        }
    }

    [System.Serializable]
    struct MessageServerObjectList
    {
        public List<string> messages;
    }

    [System.Serializable]
    public struct MessageDestroyServerObject
    {
        public int instanceID;

        public MessageDestroyServerObject(ServerObject serverObject)
        {
            instanceID = serverObject.transform.GetInstanceID();
        }
    }
}
