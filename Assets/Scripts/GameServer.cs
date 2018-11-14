using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Facepunch.Steamworks;

public class GameServer : MonoBehaviour
{
    public float hz = 30;

    public Transform[] sharedCubes;

    void Start()
    {
        Client.Instance.Networking.OnP2PData += OnP2PData;

        // Channel 1 is for messages from the client to the server
        Client.Instance.Networking.SetListenChannel(1, true);

        StartCoroutine(ServerUpdate());
    }

    IEnumerator ServerUpdate()
    {
        while (true)
        {
            foreach (Transform cube in sharedCubes)
            {
                SendCubeTransform(cube);
            }

            yield return new WaitForSeconds(1.0f / hz);
        }
    }

    void OnP2PData(ulong steamID, byte[] data, int dataLength, int channel)
    {
        if (channel == 1)
        {
            // Client sent a message to me
        }
    }

    public void SendCubeTransform (Transform cube)
    {
        string message = cube.GetInstanceID() + "\n"
            + cube.transform.localPosition.x + "," + cube.transform.localPosition.y + "," + cube.transform.localPosition.z + "\n"
            + cube.transform.localRotation.x + "," + cube.transform.localRotation.y + "," + cube.transform.localRotation.z + "," + cube.transform.localRotation.w + "\n"
            + cube.transform.localScale.x + "," + cube.transform.localScale.y + "," + cube.transform.localScale.z;

        byte[] data = System.Text.Encoding.UTF8.GetBytes(message);
        ulong[] memberIDs = Client.Instance.Lobby.GetMemberIDs();

        foreach (ulong id in memberIDs)
        {
            // Channel 0 is for messages from the server to the client
            if (!Client.Instance.Networking.SendP2PPacket(id, data, data.Length, Networking.SendType.Reliable, 0))
            {
                Debug.Log("Could not send peer to peer packet to user " + id);
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
