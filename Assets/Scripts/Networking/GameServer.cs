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

        public void SendMessageServerObject(ServerObject serverObject)
        {
            if (serverObject.transform.hasChanged)
            {
                serverObject.transform.hasChanged = false;
                string message = JsonUtility.ToJson(new MessageServerObject(serverObject));
                ClientManager.Instance.SendToAllClients(message, NetworkMessageType.ServerObject, Facepunch.Steamworks.Networking.SendType.Unreliable);
            }
        }

        public void SendMessageDestroyServerObject(ServerObject serverObject)
        {
            string message = JsonUtility.ToJson(new MessageDestroyServerObject(serverObject));
            ClientManager.Instance.SendToAllClients(message, NetworkMessageType.DestroyGameObject, Facepunch.Steamworks.Networking.SendType.Reliable);
        }
    }
}
