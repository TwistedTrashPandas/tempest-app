using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JoinLobbyDialog : MonoBehaviour
{
    public ulong lobbyIDToJoin;
    public Text text;

    public void ShowDialog (ulong lobbyID, string steamUserName)
    {
        gameObject.SetActive(true);
        lobbyIDToJoin = lobbyID;
        text.text = "Do you want to accept the invitation to the lobby of " + steamUserName + "?";
    }

    public void AcceptLobbyInvitation ()
    {
        Facepunch.Steamworks.Client.Instance.Lobby.Leave();
        Facepunch.Steamworks.Client.Instance.Lobby.Join(lobbyIDToJoin);
        gameObject.SetActive(false);
        lobbyIDToJoin = 0;
        text.text = "?";
    }

    public void DeclineLobbyInvitation ()
    {
        gameObject.SetActive(false);
        lobbyIDToJoin = 0;
        text.text = "?";
    }
}
