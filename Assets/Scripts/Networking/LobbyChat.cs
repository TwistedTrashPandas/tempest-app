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

        void OnMessageLobbyChat(byte[] data, ulong steamID)
        {
            string message = System.Text.Encoding.UTF8.GetString(data);
            textChat.text += "<color=grey>[" + Client.Instance.Friends.Get(steamID).Name + "]: </color>" + message + "\n";
        }

        public void SendChatMessage()
        {
            byte[] data = System.Text.Encoding.UTF8.GetBytes(inputFieldChat.text);
            ClientManager.Instance.SendToAllClients(data, NetworkMessageType.LobbyChat, Facepunch.Steamworks.Networking.SendType.Reliable);

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
