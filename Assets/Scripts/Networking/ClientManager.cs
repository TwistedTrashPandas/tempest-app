using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Facepunch.Steamworks;

namespace MastersOfTempest.Networking
{
    //  1. Initializes steam on startup
    //  2. Calls Update
    //  3. Handles all the networking messages
    //  4. Disposes and shuts down Steam on close

    public class ClientManager : MonoBehaviour
    {
        public static ClientManager Instance = null;

        // The app id should be 480 for testing purposes
        public uint appId = 480;
        public bool debugClientMessages = false;
        public bool debugServerMessages = false;

        // Dynamically let other classes subscribe to these events
        public Dictionary<NetworkMessageType, System.Action<string, ulong>> clientMessageEvents;
        public Dictionary<NetworkMessageType, System.Action<string, ulong>> serverMessageEvents;

        private Client client;
        private int serverMessagesOffset = 0;

        void Awake()
        {
            // Make sure that the plugins are found in both editor and build
            System.Environment.SetEnvironmentVariable("PATH", Application.dataPath + "/Plugins/", System.EnvironmentVariableTarget.Process);

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
                DialogBox.Show("Make sure that you are online and Steam is running.\nDo you want to exit the game?", true, true, Application.Quit, null);
            }

            // Create all the actions for incoming network messages
            clientMessageEvents = new Dictionary<NetworkMessageType, System.Action<string, ulong>>();
            serverMessageEvents = new Dictionary<NetworkMessageType, System.Action<string, ulong>>();

            System.Array types = System.Enum.GetValues(typeof(NetworkMessageType));
            serverMessagesOffset = types.Length;

            // Listen to all the network messages on different channels - each represents a message type
            foreach (NetworkMessageType type in types)
            {
                // Listen to messages for the client
                client.Networking.SetListenChannel((int)type, true);
                clientMessageEvents[type] = new System.Action<string, ulong>(DebugClientMessageEvent);

                // Listen to all messages for the server with an offset -> know which messages are for the server
                client.Networking.SetListenChannel(serverMessagesOffset + (int)type, true);
                serverMessageEvents[type] = new System.Action<string, ulong>(DebugServerMessageEvent);
            }

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

        void DebugClientMessageEvent(string message, ulong steamID)
        {
            if (debugClientMessages)
            {
                Debug.Log("Client received message from " + steamID + ":\n" + message);
            }
        }

        void DebugServerMessageEvent(string message, ulong steamID)
        {
            if (debugServerMessages)
            {
                Debug.Log("Server received message from " + steamID + ":\n" + message);
            }
        }

        bool OnIncomingConnection(ulong steamID)
        {
            Debug.Log("Incoming peer to peer connection from user " + steamID);
            return true;
        }

        void OnConnectionFailed(ulong steamID, Facepunch.Steamworks.Networking.SessionError sessionError)
        {
            Debug.Log("Connection failed with user " + steamID + " " + sessionError);
        }

        // This is where all the messages are received and delegated to the respective events
        void OnP2PData(ulong steamID, byte[] data, int dataLength, int channel)
        {
            string message = System.Text.Encoding.UTF8.GetString(data, 0, dataLength);

            if (channel < serverMessagesOffset)
            {
                // The message is for the client
                NetworkMessageType messageType = (NetworkMessageType)channel;
                clientMessageEvents[messageType].Invoke(message, steamID);
            }
            else
            {
                // The message is for the server (which is running on this client)
                NetworkMessageType messageType = (NetworkMessageType)(channel - serverMessagesOffset);
                serverMessageEvents[messageType].Invoke(message, steamID);
            }
        }

        private void SendToClient(ulong steamID, byte[] data, int channel, Facepunch.Steamworks.Networking.SendType sendType)
        {
            if (client != null)
            {
                // Send the message to the client on the channel of this message type
                if (!client.Networking.SendP2PPacket(steamID, data, data.Length, sendType, channel))
                {
                    Debug.Log("Could not send peer to peer packet to user " + steamID);
                }
                else if (debugClientMessages)
                {
                    Debug.Log("Sending message to " + steamID + ":\n" + System.Text.Encoding.UTF8.GetString(data));
                }
            }
        }

        public void SendToClient(ulong steamID, string message, NetworkMessageType networkMessageType, Facepunch.Steamworks.Networking.SendType sendType)
        {
            SendToClient(steamID, System.Text.Encoding.UTF8.GetBytes(message), (int)networkMessageType, sendType);
        }

        public void SendToAllClients(string message, NetworkMessageType networkMessageType, Facepunch.Steamworks.Networking.SendType sendType)
        {
            if (client != null)
            {
                byte[] data = System.Text.Encoding.UTF8.GetBytes(message);
                ulong[] lobbyMemberIDs = client.Lobby.GetMemberIDs();

                foreach (ulong steamID in lobbyMemberIDs)
                {
                    SendToClient(steamID, data, (int)networkMessageType, sendType);
                }
            }
        }

        public void SendToServer(string message, NetworkMessageType networkMessageType, Facepunch.Steamworks.Networking.SendType sendType)
        {
            // Messages for the server are sent on a different channel than messages for a client
            // This way the client knows if the incoming message is for him as a client or him as a server
            SendToClient(client.Lobby.Owner, System.Text.Encoding.UTF8.GetBytes(message), serverMessagesOffset + (int)networkMessageType, sendType);
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
}
