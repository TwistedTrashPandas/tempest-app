using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Add new types here (e.g. the name of the class that uses it)
// Make sure to assign and never change the index
public enum NetworkMessageType
{
    Empty = 0,
    LobbyChat = 1,
    LobbyStartGame = 2,
    ServerObject = 3,
    DestroyGameObject = 4,
    NetworkBehaviourInitialized = 5,
    PushRigidbodyUp = 6
};

[System.Serializable]
struct MessageServerObject
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

    public MessageServerObject (ServerObject serverObject)
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
struct MessageDestroyServerObject
{
    public int instanceID;

    public MessageDestroyServerObject(ServerObject serverObject)
    {
        instanceID = serverObject.transform.GetInstanceID();
    }
}
