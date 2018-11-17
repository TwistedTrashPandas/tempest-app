using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NetworkMessageType
{
    MessageLobbyChat,
    MessageServerObject,
    MessageDestroyGameObject
};

[System.Serializable]
struct MessageServerObject
{
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
