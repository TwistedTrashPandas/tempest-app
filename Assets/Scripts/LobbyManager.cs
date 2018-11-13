using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Facepunch.Steamworks;

public class LobbyManager : MonoBehaviour
{
    public GameObject friendPrefab;

    public Transform lobbyLayout;
    public Transform friendsLayout;

	// Use this for initialization
	void Start ()
    {
        if (Client.Instance != null)
        {
            Client.Instance.Lobby.OnLobbyCreated += OnLobbyCreated;
            Client.Instance.Lobby.OnLobbyJoined += OnLobbyJoined;

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
    }

    void OnLobbyJoined (bool success)
    {
        if (success && Client.Instance.Lobby.IsValid)
        {
            Debug.Log("Joined lobby \"" + Client.Instance.Lobby.Name + "\"");
        }
    }

    // THIS IS NOT A NICE WAY TO HANDLE THE LOBBY!
    IEnumerator RefreshLobby ()
    {
        yield return new WaitForSeconds(0.25f);

        while (true)
        {
            DestroyAllFriends(lobbyLayout);
            DestroyAllFriends(friendsLayout);

            // Load all friends of this user
            Client.Instance.Friends.Refresh();
            IEnumerable<SteamFriend> friends = Client.Instance.Friends.All;

            foreach (SteamFriend friend in friends)
            {
                if (friend.IsOnline)
                {
                    Friend f = Instantiate(friendPrefab, friendsLayout, false).GetComponent<Friend>();
                    f.id = friend.Id;

                    Client.Instance.Friends.GetAvatar(Friends.AvatarSize.Large, friend.Id, f.OnImage);
                }
            }

            // Display current users that are in this lobby
            ulong[] memberIDs = Client.Instance.Lobby.GetMemberIDs();

            foreach (ulong id in memberIDs)
            {
                Friend f = Instantiate(friendPrefab, lobbyLayout, false).GetComponent<Friend>();
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
