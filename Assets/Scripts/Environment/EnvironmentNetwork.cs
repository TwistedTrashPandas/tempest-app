using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MasterOfTempest.Networking;
using MastersOfTempest.Environment.Interacting;
using MastersOfTempest.Environment;

namespace MasterOfTempest
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
        EnvironmentManager environmentManager;
        
        [System.Serializable]
        private struct MessageAllEnvObjects
        {
            public List<MessageEnvObject> envObjects;

            public MessageAllEnvObjects(List<EnvObject> objects)
            {
                envObjects = new List<MessageEnvObject>();
                for (int i = 0; i < objects.Count; i++)
                {
                    envObjects.Add(new MessageEnvObject(objects[i].transform, 0));
                }
            }
        };

        [System.Serializable]
        public struct MessageEnvObject
        {
            public int instanceID;
            public Vector3 position;
            public Vector3 localScale;
            public Quaternion orientation;
            public EnvSpawner.EnvObjectType type;

            public MessageEnvObject(Transform transform, EnvSpawner.EnvObjectType t)
            {
                position = transform.position;
                localScale = transform.localScale;
                orientation = transform.rotation;
                instanceID = transform.GetInstanceID();
                type = t;
            }
        };

        // Used for initialization when this object is on a client
        protected override void StartClient()
        {
            environmentManager = GetComponent<EnvironmentManager>();
            if (environmentManager == null)
                throw new System.InvalidOperationException("EnvironmentNetwork cannot operate without EnvironmentManager on the same object.");
        }

        // Used for initialization when this object is on the server
        protected override void StartServer()
        {
            environmentManager = GetComponent<EnvironmentManager>();
            if (environmentManager == null)
                throw new System.InvalidOperationException("EnvironmentNetwork cannot operate without EnvironmentManager on the same object.");
            StartCoroutine(SendEnvObjects());
        }

        IEnumerator SendEnvObjects()
        {
            while (true)
            {
                string msg = JsonUtility.ToJson(new MessageAllEnvObjects(environmentManager.envSpawner.envObjects));
                SendToAllClients(msg, Facepunch.Steamworks.Networking.SendType.Reliable);
                yield return new WaitForSeconds(1.0f / GameServer.Instance.hz);
            }
        }

        // Called when the object is on a client and receives a message
        protected override void OnClientReceivedMessage(string message, ulong steamID)
        {
            if (environmentManager != null)
            {
                base.OnClientReceivedMessage(message, steamID);
                MessageAllEnvObjects tmp = JsonUtility.FromJson<MessageAllEnvObjects>(message);
                environmentManager.envSpawner.UpdateEnvObjects(tmp.envObjects);
            }
        }
    }
}
