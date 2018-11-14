using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Facepunch.Steamworks;

public class GameClient : MonoBehaviour
{
    // Use the gameobject instance id from the server to keep track of the objects
    public Dictionary<int, GameObject> objectsFromServer = new Dictionary<int, GameObject>();

    void Start()
    {
        Client.Instance.Networking.OnP2PData += OnP2PData;

        // Listen to all the network messages on different channels that identify this message type
        foreach (int channel in System.Enum.GetValues(typeof(NetworkMessageType)))
        {
            Client.Instance.Networking.SetListenChannel(channel, true);
        }
    }

    void OnP2PData(ulong steamID, byte[] data, int dataLength, int channel)
    {
        NetworkMessageType messageType = (NetworkMessageType)channel;
        string message = System.Text.Encoding.UTF8.GetString(data, 0, dataLength);

        if (messageType == NetworkMessageType.MessageServerObject)
        {
            MessageServerObject messageServerObject = JsonUtility.FromJson<MessageServerObject>(message);

            // Create a new object if it doesn't exist yet
            if (!objectsFromServer.ContainsKey(messageServerObject.instanceID))
            {
                // Make sure that the parent exists already if it has one
                if (!messageServerObject.hasParent || objectsFromServer.ContainsKey(messageServerObject.parentInstanceID))
                {
                    GameObject instance = Instantiate(Resources.Load<GameObject>(messageServerObject.resourceName));
                    objectsFromServer[messageServerObject.instanceID] = instance;
                    instance.layer = LayerMask.NameToLayer("Client");
                    DestroyImmediate(instance.GetComponent<ServerObject>());
                }
            }

            Transform tmp = objectsFromServer[messageServerObject.instanceID].transform;

            // Update values
            tmp.name = messageServerObject.name + " (" + messageServerObject.instanceID + ")";
            tmp.localPosition = messageServerObject.localPosition;
            tmp.localRotation = messageServerObject.localRotation;
            tmp.localScale = messageServerObject.localScale;

            if (messageServerObject.hasParent && objectsFromServer.ContainsKey(messageServerObject.parentInstanceID))
            {
                tmp.SetParent(objectsFromServer[messageServerObject.parentInstanceID].transform, false);
            }
        }
        else if (messageType == NetworkMessageType.MessageDestroyGameObject)
        {
            MessageDestroyServerObject destroyTransformMessage = JsonUtility.FromJson<MessageDestroyServerObject>(message);

            if (objectsFromServer.ContainsKey(destroyTransformMessage.instanceID))
            {
                Destroy(objectsFromServer[destroyTransformMessage.instanceID].gameObject);
                objectsFromServer.Remove(destroyTransformMessage.instanceID);
            }
        }
    }

    public void SendMessage()
    {
        /*
        if (!Client.Instance.Networking.SendP2PPacket(id, data, data.Length, Networking.SendType.Reliable, NetworkMessage.?))
        {
            Debug.Log("Could not send peer to peer packet to user " + id);
        }
        */
    }

    void OnDestroy()
    {
        if (Client.Instance != null)
        {
            Client.Instance.Networking.OnP2PData -= OnP2PData;
        }
    }
}
