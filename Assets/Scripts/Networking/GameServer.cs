using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Facepunch.Steamworks;

public class GameServer : MonoBehaviour
{
    public float hz = 60;

    public static GameServer Instance = null;
    public LinkedList<ServerObject> serverObjects = new LinkedList<ServerObject>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogError("GameServer cannot have multiple instances!");
            Destroy(gameObject);
        }
    }

    void Start()
    {
        ClientManager.Instance.serverMessageEvents[NetworkMessageType.PushAllRigidbodiesUp] += OnMessagePushAllRigidbodiesUp;

        StartCoroutine(ServerUpdate());
    }

    void OnMessagePushAllRigidbodiesUp(string message, ulong steamID)
    {
        Debug.Log("Server received: " + message);

        Rigidbody[] rigidbodies = FindObjectsOfType<Rigidbody>();

        foreach (Rigidbody r in rigidbodies)
        {
            r.AddForce(new Vector3(0, Random.Range(2, 10), 0), ForceMode.Impulse);
            r.AddTorque(new Vector3(Random.Range(-1, 1), Random.Range(-1, 1), Random.Range(-1, 1)));
        }

        ClientManager.Instance.SendToAllClients("Client, I pushed " + rigidbodies.Length + " objects up like you said!", NetworkMessageType.PushAllRigidbodiesUp, Networking.SendType.Reliable);
    }

    IEnumerator ServerUpdate()
    {
        while (true)
        {
            foreach (ServerObject serverObject in serverObjects)
            {
                SendMessageServerObject(serverObject);
            }

            yield return new WaitForSeconds(1.0f / hz);
        }
    }

    public void SendMessageServerObject (ServerObject serverObject)
    {
        if (serverObject.transform.hasChanged)
        {
            serverObject.transform.hasChanged = false;
            string message = JsonUtility.ToJson(new MessageServerObject(serverObject));
            ClientManager.Instance.SendToAllClients(message, NetworkMessageType.ServerObject, Networking.SendType.Unreliable);
        }
    }

    public void SendMessageDestroyServerObject (ServerObject serverObject)
    {
        string message = JsonUtility.ToJson(new MessageDestroyServerObject(serverObject));
        ClientManager.Instance.SendToAllClients(message, NetworkMessageType.DestroyGameObject, Networking.SendType.Reliable);
    }

    void OnDestroy()
    {
        ClientManager.Instance.serverMessageEvents[NetworkMessageType.PushAllRigidbodiesUp] -= OnMessagePushAllRigidbodiesUp;
    }
}
