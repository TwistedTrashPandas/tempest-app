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
        ClientManager.Instance.networkMessageReceiveEvents[NetworkMessageType.MessageServerObject] += OnMessageServerObject;
        ClientManager.Instance.networkMessageReceiveEvents[NetworkMessageType.MessageDestroyGameObject] += OnMessageDestroyGameObject;
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
                GameObject instance = Instantiate(Resources.Load<GameObject>("ServerObjects/" + messageServerObject.resourceName));
                objectsFromServer[messageServerObject.instanceID] = instance;
                instance.layer = LayerMask.NameToLayer("Client");
                DestroyImmediate(instance.GetComponent<ServerObject>());
            }
        }

        Transform tmp = objectsFromServer[messageServerObject.instanceID].transform;

        // Update values
        tmp.name = messageServerObject.name + "\t\t(" + messageServerObject.instanceID + ")";
        tmp.localPosition = messageServerObject.localPosition;
        tmp.localRotation = messageServerObject.localRotation;
        tmp.localScale = messageServerObject.localScale;

        if (messageServerObject.hasParent && objectsFromServer.ContainsKey(messageServerObject.parentInstanceID))
        {
            tmp.SetParent(objectsFromServer[messageServerObject.parentInstanceID].transform, false);
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

    void OnDestroy()
    {
        ClientManager.Instance.networkMessageReceiveEvents[NetworkMessageType.MessageServerObject] -= OnMessageServerObject;
        ClientManager.Instance.networkMessageReceiveEvents[NetworkMessageType.MessageDestroyGameObject] -= OnMessageDestroyGameObject;
    }
}
