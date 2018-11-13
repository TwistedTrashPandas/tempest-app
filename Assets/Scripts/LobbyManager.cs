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
	
	// Update is called once per frame
	void Update ()
    {

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
        Debug.Log("Accepting invitation to the lobby " + lobbyID + " from user " + otherUserID);
        Client.Instance.Lobby.Leave();
        Client.Instance.Lobby.Join(lobbyID);
    }

    // THIS IS NOT A NICE WAY TO HANDLE THE LOBBY!
    IEnumerator RefreshLobby ()
    {
        yield return new WaitForSeconds(0.25f);

        while (true)
        {
            DestroyAllFriends(layoutLobby);
            DestroyAllFriends(layoutFriends);

            // Load all friends of this user
            Client.Instance.Friends.Refresh();
            IEnumerable<SteamFriend> friends = Client.Instance.Friends.All;

            foreach (SteamFriend friend in friends)
            {
                if (friend.IsOnline)
                {
                    Friend f = Instantiate(friendPrefab, layoutFriends, false).GetComponent<Friend>();
                    f.id = friend.Id;

                    Client.Instance.Friends.GetAvatar(Friends.AvatarSize.Large, friend.Id, f.OnImage);
                }
            }

            // Display current users that are in this lobby
            textLobby.text = Client.Instance.Lobby.Name;
            ulong[] memberIDs = Client.Instance.Lobby.GetMemberIDs();

            foreach (ulong id in memberIDs)
            {
                Friend f = Instantiate(friendPrefab, layoutLobby, false).GetComponent<Friend>();
                f.id = id;
                f.buttonInvite.gameObject.SetActive(false);

                Client.Instance.Friends.GetAvatar(Friends.AvatarSize.Large, id, f.OnImage);
            }

            yield return new WaitForSeconds(3);
        }
    }

    void DestroyAllFriends (Transform parent)
    {
        Friend[] tmp = parent.GetComponentsInChildren<Friend>();

        foreach (Friend f in tmp)
        {
            Destroy(f.gameObject);
        }
    }
}
