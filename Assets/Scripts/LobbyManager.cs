using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Facepunch.Steamworks;

public class LobbyManager : MonoBehaviour
{
    public GameObject friendPrefab;

    public Transform layoutLobby;
    public Transform layoutFriends;

    public UnityEngine.UI.Text textLobby;
    public UnityEngine.UI.Text textFriends;

    public JoinLobbyDialog joinLobbyDialog;

	// Use this for initialization
	void Start ()
    {
        if (Client.Instance != null)
        {
            Client.Instance.Lobby.OnLobbyCreated += OnLobbyCreated;
            Client.Instance.Lobby.OnLobbyJoined += OnLobbyJoined;
            Client.Instance.Lobby.OnUserInvitedToLobby += OnUserInvitedToLobby;

            // Create a lobby
            Client.Instance.Lobby.Create(Lobby.Type.FriendsOnly, 2);
            Client.Instance.Lobby.Name = Client.Instance.Username + "'s Lobby";

            StartCoroutine(RefreshLobby());
        }
        else
        {
            Debug.LogError("Client instance is null!");
        }
    }

    void OnLobbyCreated(bool success)
    {
        if (success && Client.Instance.Lobby.IsValid)
        {
            Debug.Log("Created lobby \"" + Client.Instance.Lobby.Name + "\"");
        }
        else
        {
            Debug.LogError("Failed to create lobby");
        }
    }

    void OnLobbyJoined (bool success)
    {
        if (success && Client.Instance.Lobby.IsValid)
        {
            Debug.Log("Joined lobby \"" + Client.Instance.Lobby.Name + "\"");
        }
        else
        {
            Debug.LogError("Failed to join lobby");
        }
    }

    void OnUserInvitedToLobby (ulong lobbyID, ulong otherUserID)
    {
        Debug.Log("Got invitation to the lobby " + lobbyID + " from user " + otherUserID);
        joinLobbyDialog.ShowDialog(lobbyID, Client.Instance.Friends.Get(otherUserID).Name);
    }

    IEnumerator RefreshLobby ()
    {
        yield return new WaitForSeconds(0.25f);

        while (true)
        {
            RefreshFriendAvatars();
            RefreshLobbyAvatars();

            yield return new WaitForSeconds(0.5f);
        }
    }

    void InstantiateFriendAvatar (SteamFriend friend, Transform parent, bool inviteable)
    {
        FriendAvatar tmp = Instantiate(friendPrefab, parent, false).GetComponent<FriendAvatar>();
        tmp.gameObject.name = friend.Name;
        tmp.steamID = friend.Id;
        tmp.buttonInvite.gameObject.SetActive(inviteable);

        Client.Instance.Friends.GetAvatar(Friends.AvatarSize.Large, friend.Id, tmp.OnImage);
    }

    void RefreshFriendAvatars ()
    {
        FriendAvatar[] friendAvatars = layoutFriends.GetComponentsInChildren<FriendAvatar>();

        Dictionary<ulong, bool> friendsToStay = new Dictionary<ulong, bool>();

        // Mark all the friends for removal later
        foreach (FriendAvatar f in friendAvatars)
        {
            friendsToStay[f.steamID] = false;
        }

        // Refresh all friends of this user
        Client.Instance.Friends.Refresh();
        IEnumerable<SteamFriend> friends = Client.Instance.Friends.All;

        foreach (SteamFriend friend in friends)
        {
            if (friend.IsOnline)
            {
                if (!friendsToStay.ContainsKey(friend.Id))
                {
                    // A new friend is now online
                    InstantiateFriendAvatar(friend, layoutFriends, true);
                }

                // This friend should not be removed later
                friendsToStay[friend.Id] = true;
            }
        }

        // Remove all friends that are no longer online
        foreach (FriendAvatar f in friendAvatars)
        {
            if (!friendsToStay[f.steamID])
            {
                Destroy(f.gameObject);
            }
        }
    }

    void RefreshLobbyAvatars()
    {
        FriendAvatar[] lobbyAvatars = layoutLobby.GetComponentsInChildren<FriendAvatar>();

        Dictionary<ulong, bool> lobbyMembersToStay = new Dictionary<ulong, bool>();

        // Mark all the friends for removal later
        foreach (FriendAvatar f in lobbyAvatars)
        {
            lobbyMembersToStay[f.steamID] = false;
        }

        // Display current users that are in this lobby
        textLobby.text = Client.Instance.Lobby.Name;
        ulong[] memberSteamIDs = Client.Instance.Lobby.GetMemberIDs();

        foreach (ulong steamID in memberSteamIDs)
        {
            if (!lobbyMembersToStay.ContainsKey(steamID))
            {
                // A new lobby member joined
                SteamFriend friend = Client.Instance.Friends.Get(steamID);
                InstantiateFriendAvatar(friend, layoutLobby, false);
            }

            // This lobby member should not be removed later
            lobbyMembersToStay[steamID] = true;
        }

        // Remove all lobby members that are no longer in the lobby
        foreach (FriendAvatar f in lobbyAvatars)
        {
            if (!lobbyMembersToStay[f.steamID])
            {
                Destroy(f.gameObject);
            }
        }
    }
}
