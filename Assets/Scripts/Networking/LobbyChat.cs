using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Facepunch.Steamworks;
using System;

namespace MastersOfTempest.Networking
{
    public class LobbyChat : MonoBehaviour
    {
        public UnityEngine.UI.InputField inputFieldChat;
        public UnityEngine.UI.Text textChat;

        void Start()
        {
            ClientManager.Instance.clientMessageEvents[NetworkMessageType.LobbyChat] += OnMessageLobbyChat;
        }

        void OnMessageLobbyChat(string message, ulong steamID)
        {
            textChat.text += "<color=grey>[" + Client.Instance.Friends.Get(steamID).Name + "]: </color>" + message + "\n";
        }

        public void SendChatMessage()
        {
            ClientManager.Instance.SendToAllClients(inputFieldChat.text, NetworkMessageType.LobbyChat, Facepunch.Steamworks.Networking.SendType.Reliable);

            inputFieldChat.text = "";
            inputFieldChat.ActivateInputField();
            inputFieldChat.Select();
            inputFieldChat.placeholder.gameObject.SetActive(false);
        }

        void OnDestroy()
        {
            ClientManager.Instance.clientMessageEvents[NetworkMessageType.LobbyChat] -= OnMessageLobbyChat;
        }
    }
}
