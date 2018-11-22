using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerObject : MonoBehaviour
{
    public string resourceName = "";

    public bool onServer = true;
    public int serverID = 0;
    public float lastUpdate = 0;

    void Start ()
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
            GameServer.Instance.serverObjects.AddLast(this);
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
}

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
struct MessageDestroyServerObject
{
    public int instanceID;

    public MessageDestroyServerObject(ServerObject serverObject)
    {
        instanceID = serverObject.transform.GetInstanceID();
    }
}
