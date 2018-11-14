using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Facepunch.Steamworks;

public class GameClient : MonoBehaviour
{
    // Use the gameobject instance id from the server to keep track of the objects
    public Dictionary<int, Transform> sharedTransforms = new Dictionary<int, Transform>();

    void Start()
    {
        Client.Instance.Networking.OnP2PData += OnP2PData;

        // Channel 0 is for messages from the server to the client
        Client.Instance.Networking.SetListenChannel(0, true);
    }

    void OnP2PData(ulong steamID, byte[] data, int dataLength, int channel)
    {
        if (channel == 0)
        {
            // Server sent a message to me
            string message = System.Text.Encoding.UTF8.GetString(data, 0, dataLength);

            string[] idPositionRotationScale = message.Replace("(", "").Replace(")","").Split('\n');
            string[] pos = idPositionRotationScale[1].Split(',');
            string[] rot = idPositionRotationScale[2].Split(',');
            string[] scale = idPositionRotationScale[3].Split(',');

            int id = int.Parse(idPositionRotationScale[0]);

            // Create a new object if it doesn't exist yet
            if (!sharedTransforms.ContainsKey(id))
            {
                GameObject cubePrefab = Resources.Load<GameObject>("Cube");
                GameObject instance = Instantiate(cubePrefab);
                instance.layer = LayerMask.NameToLayer("Client");
                sharedTransforms[id] = instance.transform;
            }

            Transform tmp = sharedTransforms[id];

            tmp.localPosition = new Vector3(float.Parse(pos[0]), float.Parse(pos[1]), float.Parse(pos[2]));
            tmp.localRotation = new Quaternion(float.Parse(rot[0]), float.Parse(rot[1]), float.Parse(rot[2]), float.Parse(rot[3]));
            tmp.localScale = new Vector3(float.Parse(scale[0]), float.Parse(scale[1]), float.Parse(scale[2]));
        }
    }

    public void SendMessage()
    {
        // Channel 1 is for messages from the client to the server

        /*
        if (!Client.Instance.Networking.SendP2PPacket(id, data, data.Length, Networking.SendType.Reliable, 1))
        {
            Debug.Log("Could not send peer to peer packet to user " + id);
        }
        */
    }

    void OnDestroy()
    {
        if (Client.Instance != null)
        {
            Client.Instance.Networking.OnP2PData -= OnP2PData;
        }
    }
}
