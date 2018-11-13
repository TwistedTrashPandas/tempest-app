using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Facepunch.Steamworks;

public class PeerToPeerManager : MonoBehaviour
{
    public UnityEngine.UI.InputField inputFieldChat;
    public UnityEngine.UI.Text textChat;

	void Start ()
    {
        Client.Instance.Networking.OnIncomingConnection = OnIncomingConnection;
        Client.Instance.Networking.OnConnectionFailed += OnConnectionFailed;
        Client.Instance.Networking.OnP2PData = OnRecievedP2PData;

        Client.Instance.Networking.SetListenChannel(0, true);
    }

    bool OnIncomingConnection (ulong steamID)
    {
        Debug.Log("Incoming peer to peer connection from user " + steamID);
        return true;
    }

    void OnConnectionFailed (ulong steamID, Networking.SessionError sessionError)
    {
        Debug.Log("Connection failed with user " + steamID + " " + sessionError);
    }

    void OnRecievedP2PData(ulong steamID, byte[] data, int dataLength, int channel)
    {
        string message = System.Text.Encoding.UTF8.GetString(data, 0, dataLength);
        textChat.text += "<color=grey>[" + Client.Instance.Friends.Get(steamID).Name + "]: </color>" + message + "\n";
    }

    public void SendChatMessage ()
    {
        byte[] data = System.Text.Encoding.UTF8.GetBytes(inputFieldChat.text);
        ulong[] memberIDs = Client.Instance.Lobby.GetMemberIDs();

        foreach (ulong id in memberIDs)
        {
            Debug.Log("Sending " + inputFieldChat.text + " to " + id);
            if (!Client.Instance.Networking.SendP2PPacket(id, data, data.Length))
            {
                Debug.Log("Could not send peer to peer packet to user " + id);
            }
        }

        inputFieldChat.text = "";
        inputFieldChat.ActivateInputField();
        inputFieldChat.Select();
        inputFieldChat.placeholder.gameObject.SetActive(false);
    }
}
