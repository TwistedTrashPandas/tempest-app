using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MastersOfTempest.Networking;
using MastersOfTempest.Environment.Interacting;
using MastersOfTempest.Environment;

namespace MastersOfTempest
{
    // Use this class for custom networking messages between the client and the server
    // Requires a ServerObject to be attached to this gameObject
    // The transform is automatically synchronized already by the ServerObject

    // NOTES:
    // - Rename this class and the message struct
    // - Make sure that the public member networkMessageType is set to the type of your message (e.g. the name of this class)
    // - Only use SendToServer(...) as a client and let the server answer with SendToClient(...) or SendToAllClients(...)
    // - If you have to use the MonoBehaviour Start(), Update() and OnDestroy() methods you have to call base.Start() / base.Update() / base.OnDestroy() inside
    // - If you want to synchronize a lot of objects it is more performant to only have one NetworkBehaviour that handles e.g. a list of objects that you want to synchronize
    public class EnvironmentNetwork : NetworkBehaviour
    {        
        [System.Serializable]
        private struct MessageAllEnvObjects
        {
            public List<MessageEnvObject> envObjects;
            public float lastUpdate;

            public MessageAllEnvObjects(List<EnvObject> objects)
            {
                envObjects = new List<MessageEnvObject>();
                for (int i = 0; i < objects.Count; i++)
                {
                    envObjects.Add(new MessageEnvObject(objects[i].transform, objects[i].type, objects[i].prefabNum));
                }
                lastUpdate = Time.fixedTime;
            }
        };

        [System.Serializable]
        public struct MessageEnvObject
        {
            public int instanceID;
            public int prefabNum;
            public Vector3 position;
            public Vector3 localScale;
            public Quaternion orientation;
            public EnvSpawner.EnvObjectType type;

            public MessageEnvObject(Transform transform, EnvSpawner.EnvObjectType t, int pNum)
            {
                position = transform.position;
                localScale = transform.localScale;
                orientation = transform.rotation;
                instanceID = transform.GetInstanceID();
                type = t;
                prefabNum = pNum;
            }
        };

        private EnvironmentManager envManager;

        // Used for initialization when this object is on a client
        protected override void StartClient()
        {
            envManager = GetComponent<EnvironmentManager>();
            if (envManager == null)
                throw new System.InvalidOperationException("EnvironmentNetwork cannot operate without Environment Manager on the same object");
        }

        // Used for initialization when this object is on the server
        protected override void StartServer()
        {
            envManager = GetComponent<EnvironmentManager>();
            if (envManager == null)
                throw new System.InvalidOperationException("EnvironmentNetwork cannot operate without Environment Manager on the same object");
            StartCoroutine(SendEnvObjects());
        }

        IEnumerator SendEnvObjects()
        {
            while (true)
            {
                string msg = JsonUtility.ToJson(new MessageAllEnvObjects(envManager.envSpawner.envObjects));
                SendToAllClients(msg, Facepunch.Steamworks.Networking.SendType.Reliable);
                yield return new WaitForSeconds(1.0f / GameServer.Instance.hz);
            }
        }

        // Called when the object is on a client and receives a message
        protected override void OnClientReceivedMessage(string message, ulong steamID)
        {
            if (envManager != null)
            {
                base.OnClientReceivedMessage(message, steamID);
                MessageAllEnvObjects tmp = JsonUtility.FromJson<MessageAllEnvObjects>(message);
                envManager.envSpawner.UpdateEnvObjects(tmp.envObjects, tmp.lastUpdate);
            }
        }
    }
}
