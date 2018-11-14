using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Facepunch.Steamworks;

public class GameServer : MonoBehaviour
{
    public float hz = 30;

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
        Client.Instance.Networking.OnP2PData += OnP2PData;

        // Listen to all the network messages on different channels that identify this message type
        foreach (int channel in System.Enum.GetValues(typeof(NetworkMessageType)))
        {
            Client.Instance.Networking.SetListenChannel(channel, true);
        }

        StartCoroutine(ServerUpdate());
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

    void OnP2PData(ulong steamID, byte[] data, int dataLength, int channel)
    {
        NetworkMessageType messageType = (NetworkMessageType)channel;

        if (messageType == NetworkMessageType.MessageServerObject)
        {
            // ...
        }
    }

    public void SendMessageServerObject (ServerObject serverObject)
    {
        if (serverObject.transform.hasChanged)
        {
            SendToAllClients(new MessageServerObject(serverObject), Networking.SendType.Reliable, NetworkMessageType.MessageServerObject);
            serverObject.transform.hasChanged = false;
        }
    }

    public void SendMessageDestroyServerObject (ServerObject serverObject)
    {
        SendToAllClients(new MessageDestroyServerObject(serverObject), Networking.SendType.Reliable, NetworkMessageType.MessageDestroyGameObject);
    }

    public void SendToAllClients<T> (T serializableMessage, Networking.SendType sendType, NetworkMessageType networkMessageType)
    {
        if (Client.Instance != null)
        {
            string message = JsonUtility.ToJson(serializableMessage);
            byte[] data = System.Text.Encoding.UTF8.GetBytes(message);

            ulong[] memberIDs = Client.Instance.Lobby.GetMemberIDs();

            foreach (ulong id in memberIDs)
            {
                // Send the message to the client on the channel of this message type
                if (!Client.Instance.Networking.SendP2PPacket(id, data, data.Length, sendType, (int)networkMessageType))
                {
                    Debug.Log("Could not send peer to peer packet to user " + id);
                }
            }
        }
    }

    void OnDestroy()
    {
        if (Client.Instance != null)
        {
            Client.Instance.Networking.OnP2PData -= OnP2PData;
        }
    }
}
