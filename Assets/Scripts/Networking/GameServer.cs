using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Facepunch.Steamworks;

namespace MastersOfTempest.Networking
{
    public class GameServer : MonoBehaviour
    {
        public float hz = 64;

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
            StartCoroutine(ServerUpdate());

            ClientManager.Instance.serverMessageEvents[NetworkMessageType.PingPong] += OnMessagePingPong;
        }

        IEnumerator ServerUpdate()
        {
            while (true)
            {
                foreach (ServerObject serverObject in serverObjects)
                {
                    SendMessageServerObject(serverObject, Facepunch.Steamworks.Networking.SendType.Unreliable);
                }

                yield return new WaitForSeconds(1.0f / hz);
            }
        }

        public void RegisterServerObject (ServerObject serverObject)
        {
            // Make sure that objects are spawned on the server (with UDP it could happen that they don't spawn)
            SendMessageServerObject(serverObject, Facepunch.Steamworks.Networking.SendType.Reliable);
            serverObjects.AddLast(serverObject);
        }

        public void SendMessageServerObject(ServerObject serverObject, Facepunch.Steamworks.Networking.SendType sendType)
        {
            if (serverObject.transform.hasChanged)
            {
                serverObject.transform.hasChanged = false;
                string message = JsonUtility.ToJson(new MessageServerObject(serverObject));
                ClientManager.Instance.SendToAllClients(message, NetworkMessageType.ServerObject, sendType);
            }
        }

        public void SendMessageDestroyServerObject(ServerObject serverObject)
        {
            string message = JsonUtility.ToJson(new MessageDestroyServerObject(serverObject));
            ClientManager.Instance.SendToAllClients(message, NetworkMessageType.DestroyGameObject, Facepunch.Steamworks.Networking.SendType.Reliable);
        }

        void OnMessagePingPong(string message, ulong steamID)
        {
            ClientManager.Instance.SendToClient(steamID, message, NetworkMessageType.PingPong, Facepunch.Steamworks.Networking.SendType.Unreliable);
        }

        void OnDestroy()
        {
            ClientManager.Instance.serverMessageEvents[NetworkMessageType.PingPong] -= OnMessagePingPong;
        }
    }
}
