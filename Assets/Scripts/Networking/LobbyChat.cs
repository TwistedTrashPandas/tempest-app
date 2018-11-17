using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Facepunch.Steamworks;

public class LobbyChat : MonoBehaviour
{
    public UnityEngine.UI.InputField inputFieldChat;
    public UnityEngine.UI.Text textChat;

	void Start ()
    {
        ClientManager.Instance.networkMessageReceiveEvents[NetworkMessageType.MessageLobbyChat] += OnMessageLobbyChat;
    }

    void OnMessageLobbyChat(string message, ulong steamID)
    {
        textChat.text += "<color=grey>[" + Client.Instance.Friends.Get(steamID).Name + "]: </color>" + message + "\n";
    }

    public void SendChatMessage ()
    {
        ClientManager.Instance.SendToAllClients(inputFieldChat.text, NetworkMessageType.MessageLobbyChat, Networking.SendType.Reliable);

        inputFieldChat.text = "";
        inputFieldChat.ActivateInputField();
        inputFieldChat.Select();
        inputFieldChat.placeholder.gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        ClientManager.Instance.networkMessageReceiveEvents[NetworkMessageType.MessageLobbyChat] -= OnMessageLobbyChat;
    }
}
