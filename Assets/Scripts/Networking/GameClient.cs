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
        ClientManager.Instance.clientMessageEvents[NetworkMessageType.ServerObject] += OnMessageServerObject;
        ClientManager.Instance.clientMessageEvents[NetworkMessageType.DestroyGameObject] += OnMessageDestroyGameObject;
        ClientManager.Instance.clientMessageEvents[NetworkMessageType.PushAllRigidbodiesUp] += OnMessagePushAllRigidbodiesUp;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ClientManager.Instance.SendToServer("Server, push all objects up!", NetworkMessageType.PushAllRigidbodiesUp, Networking.SendType.Reliable);
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
                GameObject obj = Instantiate(Resources.Load<GameObject>("ServerObjects/" + messageServerObject.resourceName));
                objectsFromServer[messageServerObject.instanceID] = obj;

                // Overwrite the layer so that the server camera does not see this object as well
                obj.layer = LayerMask.NameToLayer("Client");
                obj.GetComponent<ServerObject>().serverID = messageServerObject.instanceID;
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

    void OnMessagePushAllRigidbodiesUp(string message, ulong steamID)
    {
        Debug.Log("Client received: " + message);
    }

    void OnDestroy()
    {
        ClientManager.Instance.clientMessageEvents[NetworkMessageType.ServerObject] -= OnMessageServerObject;
        ClientManager.Instance.clientMessageEvents[NetworkMessageType.DestroyGameObject] -= OnMessageDestroyGameObject;
        ClientManager.Instance.clientMessageEvents[NetworkMessageType.PushAllRigidbodiesUp] -= OnMessagePushAllRigidbodiesUp;
    }
}
