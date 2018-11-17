using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Facepunch.Steamworks;

//  1. Initializes steam on startup
//  2. Calls Update
//  3. Handles all the networking messages
//  4. Disposes and shuts down Steam on close

public class ClientManager : MonoBehaviour
{
    public static ClientManager Instance = null;

    // The app id should be 480 for testing purposes
    public uint appId = 480;
    public bool debugIncomingNetworkMessages = false;
    public bool debugOutgoingNetworkMessages = false;

    // Dynamically let other classes subscribe to these events
    public Dictionary<NetworkMessageType, System.Action<string, ulong>> networkMessageReceiveEvents;

    private Client client;

    void Awake ()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.LogError("There already is an instance of the ClientManager!");
        }

        // Configurate facepunch steamworks sdk
        Config.ForUnity(Application.platform.ToString());

        try
        {
            // Create a steam_appid.txt with the app id in it, required by the SDK
            System.IO.File.WriteAllText(Application.dataPath + "/../steam_appid.txt", appId.ToString());
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Couldn't write steam_appid.txt: " + e.Message);
        }

        // Create the client
        client = new Client(appId);

        if (client.IsValid)
        {
            Debug.Log("Steam Initialized: " + client.Username + " / " + client.SteamId);
        }
        else
        {
            client = null;
            Debug.LogWarning("Couldn't initialize Steam. Make sure that Steam is running.");
        }

        // Create all the actions for incoming network messages
        networkMessageReceiveEvents = new Dictionary<NetworkMessageType, System.Action<string, ulong>>();

        // Listen to all the network messages on different channels - each represents a message type
        foreach (NetworkMessageType type in System.Enum.GetValues(typeof(NetworkMessageType)))
        {
            client.Networking.SetListenChannel((int)type, true);
            networkMessageReceiveEvents[type] = new System.Action<string, ulong>(DefaultNetworkMessageReceiveAction);
        }
    }

    void Start()
    {
        client.Networking.OnIncomingConnection += OnIncomingConnection;
        client.Networking.OnConnectionFailed += OnConnectionFailed;
        client.Networking.OnP2PData += OnP2PData;
    }

    void Update()
    {
        if (client != null)
        {
            UnityEngine.Profiling.Profiler.BeginSample("Steam Update");
            client.Update();
            UnityEngine.Profiling.Profiler.EndSample();
        }
    }

    void DefaultNetworkMessageReceiveAction(string message, ulong steamID)
    {
        if (debugIncomingNetworkMessages)
        {
            Debug.Log("Incoming network message from " + steamID + ":\n" + message);
        }
    }

    bool OnIncomingConnection(ulong steamID)
    {
        Debug.Log("Incoming peer to peer connection from user " + steamID);
        return true;
    }

    void OnConnectionFailed(ulong steamID, Networking.SessionError sessionError)
    {
        Debug.Log("Connection failed with user " + steamID + " " + sessionError);
    }

    // This is where all the messages are received and delegated to the respective events
    void OnP2PData(ulong steamID, byte[] data, int dataLength, int channel)
    {
        NetworkMessageType messageType = (NetworkMessageType)channel;
        string message = System.Text.Encoding.UTF8.GetString(data, 0, dataLength);

        networkMessageReceiveEvents[messageType].Invoke(message, steamID);
    }

    private void SendToClient (ulong steamID, byte[] data, NetworkMessageType networkMessageType, Networking.SendType sendType)
    {
        if (client != null)
        {
            // Send the message to the client on the channel of this message type
            if (!client.Networking.SendP2PPacket(steamID, data, data.Length, sendType, (int)networkMessageType))
            {
                Debug.Log("Could not send peer to peer packet to user " + steamID);
            }
        }
    }

    public void SendToClient(ulong steamID, string message, NetworkMessageType networkMessageType, Networking.SendType sendType)
    {
        if (debugOutgoingNetworkMessages)
        {
            Debug.Log("Sending message to " + steamID + ":\n" + message);
        }

        SendToClient(steamID, System.Text.Encoding.UTF8.GetBytes(message), networkMessageType, sendType);
    }

    public void SendToAllClients(string message, NetworkMessageType networkMessageType, Networking.SendType sendType)
    {
        if (client != null)
        {
            byte[] data = System.Text.Encoding.UTF8.GetBytes(message);
            ulong[] lobbyMemberIDs = client.Lobby.GetMemberIDs();

            foreach (ulong steamID in lobbyMemberIDs)
            {
                if (debugOutgoingNetworkMessages)
                {
                    Debug.Log("Sending message to " + steamID + ":\n" + message);
                }

                SendToClient(steamID, data, networkMessageType, sendType);
            }
        }
    }

    void OnDestroy()
    {
        if (client != null)
        {
            client.Networking.OnIncomingConnection -= OnIncomingConnection;
            client.Networking.OnConnectionFailed -= OnConnectionFailed;
            client.Networking.OnP2PData -= OnP2PData;
            client.Dispose();
            client = null;
        }
    }
}
