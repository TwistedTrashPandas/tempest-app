using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerObject : MonoBehaviour
{
    public string resourceName = "";

	void Start ()
    {
        if (gameObject.layer == LayerMask.NameToLayer("Server"))
        {
            // Check if the resource name is valid
            if (Resources.Load<GameObject>("ServerObjects/" + resourceName) == null)
            {
                Debug.LogError("Cannot find resource \"" + resourceName + "\" of gameobject " + name);
            }

            // Register to game server
            GameServer.Instance.serverObjects.AddLast(this);
        }
	}

    void OnDestroy()
    {
        if (gameObject.layer == LayerMask.NameToLayer("Server"))
        {
            // Send destroy message
            GameServer.Instance.serverObjects.Remove(this);
            GameServer.Instance.SendMessageDestroyServerObject(this);
        }
    }
}
