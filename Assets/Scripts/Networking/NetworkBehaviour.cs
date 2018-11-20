using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Facepunch.Steamworks;

public class NetworkBehaviour : MonoBehaviour
{
    private ServerObject serverObject;
    public NetworkMessageType networkMessageType = NetworkMessageType.Empty;

    [System.Serializable]
    private struct NetworkBehaviourMessage
    {
        public int serverID;
        public string message;

        public NetworkBehaviourMessage (int serverID, string message)
        {
            this.serverID = serverID;
            this.message = message;
        }
    };

    protected virtual void Start ()
    {
        serverObject = GetComponent<ServerObject>();

        if (serverObject.onServer)
        {
            ClientManager.Instance.serverMessageEvents[networkMessageType] += OnServerMessage;
            StartServer();
        }
        else
        {
            ClientManager.Instance.clientMessageEvents[networkMessageType] += OnClientMessage;
            StartClient();
        }

        if (networkMessageType == NetworkMessageType.Empty)
        {
            Debug.LogError("NetworkMessageType of " + gameObject.name + " should not be Empty!");
        }
    }

    protected virtual void StartServer()
    {
        // To be overwritten by the superclass
    }

    protected virtual void StartClient()
    {
        // To be overwritten by the superclass
    }

    protected virtual void Update()
    {
        if (serverObject.onServer)
        {
            UpdateServer();
        }
        else
        {
            UpdateClient();
        }
    }

    protected virtual void UpdateServer()
    {
        // To be overwritten by the superclass
    }

    protected virtual void UpdateClient()
    {
        // To be overwritten by the superclass
    }

    private void OnServerMessage (string message, ulong steamID)
    {
        NetworkBehaviourMessage networkBehaviourMessage = JsonUtility.FromJson<NetworkBehaviourMessage>(message);

        // Call the function only if the message is for this instance
        if (serverObject.serverID == networkBehaviourMessage.serverID)
        {
            OnServerReceivedMessage(networkBehaviourMessage.message, steamID);
        }
    }

    private void OnClientMessage(string message, ulong steamID)
    {
        NetworkBehaviourMessage networkBehaviourMessage = JsonUtility.FromJson<NetworkBehaviourMessage>(message);

        // Call the function only if the message is for this instance
        if (serverObject.serverID == networkBehaviourMessage.serverID)
        {
            OnClientReceivedMessage(networkBehaviourMessage.message, steamID);
        }
    }

    protected virtual void OnServerReceivedMessage(string message, ulong steamID)
    {
        // To be overwritten by the superclass
    }

    protected virtual void OnClientReceivedMessage(string message, ulong steamID)
    {
        // To be overwritten by the superclass
    }

    protected void SendToServer(string message, Networking.SendType sendType = Networking.SendType.Reliable)
    {
        NetworkBehaviourMessage networkBehaviourMessage = new NetworkBehaviourMessage(serverObject.serverID, message);
        ClientManager.Instance.SendToServer(JsonUtility.ToJson(networkBehaviourMessage), networkMessageType, sendType);
    }

    protected void SendToClient (ulong steamID, string message, Networking.SendType sendType = Networking.SendType.Reliable)
    {
        NetworkBehaviourMessage networkBehaviourMessage = new NetworkBehaviourMessage(serverObject.serverID, message);
        ClientManager.Instance.SendToClient(steamID, JsonUtility.ToJson(networkBehaviourMessage), networkMessageType, sendType);
    }

    protected void SendToAllClients(string message, Networking.SendType sendType = Networking.SendType.Reliable)
    {
        NetworkBehaviourMessage networkBehaviourMessage = new NetworkBehaviourMessage(serverObject.serverID, message);
        ClientManager.Instance.SendToAllClients(JsonUtility.ToJson(networkBehaviourMessage), networkMessageType, sendType);
    }

    protected void OnDestroy()
    {
        if (serverObject.onServer)
        {
            ClientManager.Instance.serverMessageEvents[networkMessageType] -= OnServerMessage;
            OnDestroyServer();
        }
        else
        {
            ClientManager.Instance.clientMessageEvents[networkMessageType] -= OnClientMessage;
            OnDestroyClient();
        }
    }

    protected virtual void OnDestroyServer ()
    {
        // To be overwritten by the superclass
    }

    protected virtual void OnDestroyClient()
    {
        // To be overwritten by the superclass
    }
}
