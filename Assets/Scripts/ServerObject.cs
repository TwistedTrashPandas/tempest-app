using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerObject : MonoBehaviour
{
    public string resourceName = "";

	void Start ()
    {
        // Check if the resource name is valid
        if (Resources.Load<GameObject>(resourceName) == null)
        {
            Debug.LogError("Cannot find resource \"" + resourceName + "\" of gameobject " + name);
        }

        // Register to game server
        GameServer.Instance.serverObjects.AddLast(this);

        // Make sure this is not rendered for the client
        gameObject.layer = LayerMask.NameToLayer("Server");
	}

    void OnDestroy()
    {
        if (GameServer.Instance != null)
        {
            // Send destroy message
            GameServer.Instance.serverObjects.Remove(this);
            GameServer.Instance.SendMessageDestroyServerObject(this);
        }
    }
}
