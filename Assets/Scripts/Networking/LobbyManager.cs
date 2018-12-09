using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Facepunch.Steamworks;

namespace MastersOfTempest.Networking
{
    public class LobbyManager : MonoBehaviour
    {
        public GameObject friendPrefab;

        public Transform layoutLobby;
        public Transform layoutFriends;

        public UnityEngine.UI.Text textLobby;
        public UnityEngine.UI.Text textFriends;
        public UnityEngine.UI.Button readyButton;

        [Header("Change this to your development scene(s)")]
        public string serverSceneName = "Server";
        public string clientSceneName = "Client";

        private ulong lobbyIDToJoin;
        private bool ready = false;

        void Start()
        {
            if (Client.Instance != null)
            {
                Client.Instance.Lobby.OnLobbyCreated += OnLobbyCreated;
                Client.Instance.Lobby.OnLobbyJoined += OnLobbyJoined;
                Client.Instance.Lobby.OnUserInvitedToLobby += OnUserInvitedToLobby;

                ClientManager.Instance.clientMessageEvents[NetworkMessageType.LobbyStartGame] += OnMessageLobbyStartGame;

                // Create a lobby that the player is in when the game starts
                CreateDefaultLobby();

                StartCoroutine(RefreshLobby());
            }
            else
            {
                Debug.LogError("Client instance is null!");
            }

            Client.Instance.Lobby.SetMemberData("Ready", ready.ToString());
            StartCoroutine(CheckForEveryoneReady());
        }

        IEnumerator CheckForEveryoneReady()
        {
            // Only the lobby owner checks if everyone is ready and then sends a message to everyone to start the game
            bool gameStarted = false;

            while (!gameStarted)
            {
                // Always check this because the lobby owner could have changed
                if (Client.Instance.SteamId == Client.Instance.Lobby.Owner)
                {
                    ulong[] memberSteamIDs = Client.Instance.Lobby.GetMemberIDs();

                    if (memberSteamIDs.Length > 0)
                    {
                        bool everyoneReady = true;

                        foreach (ulong steamID in memberSteamIDs)
                        {
                            bool memberReady = false;
                            bool.TryParse(Client.Instance.Lobby.GetMemberData(steamID, "Ready"), out memberReady);

                            if (!memberReady)
                            {
                                everyoneReady = false;
                                break;
                            }
                        }

                        if (everyoneReady)
                        {
                            // Send the game start message only once
                            byte[] data = System.Text.Encoding.UTF8.GetBytes("LobbyStartGame");
                            ClientManager.Instance.SendToAllClients(data, NetworkMessageType.LobbyStartGame, Facepunch.Steamworks.Networking.SendType.Reliable);
                            gameStarted = true;
                        }
                    }
                }

                yield return new WaitForSeconds(0.5f);
            }
        }

        public void Ready()
        {
            ready = !ready;
            Client.Instance.Lobby.SetMemberData("Ready", ready.ToString());
            readyButton.GetComponent<UnityEngine.UI.Image>().color = ready ? new UnityEngine.Color(0, 0.25f, 0) : new UnityEngine.Color(0.25f, 0, 0);
            readyButton.GetComponentInChildren<UnityEngine.UI.Text>().text = ready ? "Ready" : "Not Ready";
        }

        void CreateDefaultLobby()
        {
            Client.Instance.Lobby.Create(Lobby.Type.FriendsOnly, 4);
            Client.Instance.Lobby.Name = Client.Instance.Username + "'s Lobby";
        }

        void OnMessageLobbyStartGame(byte[] data, ulong steamID)
        {
            // Load client scene
            UnityEngine.SceneManagement.SceneManager.LoadScene(clientSceneName);

            // Also load server scene if you are the owner of the lobby
            if (Client.Instance.Lobby.Owner == Client.Instance.SteamId)
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(serverSceneName, UnityEngine.SceneManagement.LoadSceneMode.Additive);
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
                Debug.LogError("Failed to create lobby. Trying to recreate the default lobby...");
                CreateDefaultLobby();
            }
        }

        void OnLobbyJoined(bool success)
        {
            if (success && Client.Instance.Lobby.IsValid)
            {
                Debug.Log("Joined lobby \"" + Client.Instance.Lobby.Name + "\"");
            }
            else
            {
                Debug.LogError("Failed to join lobby. Recreating default lobby...");
                CreateDefaultLobby();
            }
        }

        void OnUserInvitedToLobby(ulong lobbyID, ulong otherUserID)
        {
            Debug.Log("Got invitation to the lobby " + lobbyID + " from user " + otherUserID);
            lobbyIDToJoin = lobbyID;
            string message = "Player " + Client.Instance.Friends.Get(otherUserID).Name + " invited you to a lobby.\nDo you want to join?";
            DialogBox.Show(message, true, true, AcceptLobbyInvitation, null);
        }

        void AcceptLobbyInvitation()
        {
            Client.Instance.Lobby.Leave();
            Client.Instance.Lobby.Join(lobbyIDToJoin);
        }

        IEnumerator RefreshLobby()
        {
            yield return new WaitForSeconds(0.25f);

            while (true)
            {
                RefreshFriendAvatars();
                RefreshLobbyAvatars();

                yield return new WaitForSeconds(0.5f);
            }
        }

        void RefreshFriendAvatars()
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
                    // A new lobby member joined, activate ready outline
                    SteamFriend friend = Client.Instance.Friends.Get(steamID);
                    InstantiateFriendAvatar(friend, layoutLobby, false).imageReadyOutline.gameObject.SetActive(true);
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

        FriendAvatar InstantiateFriendAvatar(SteamFriend friend, Transform parent, bool inviteable)
        {
            FriendAvatar tmp = Instantiate(friendPrefab, parent, false).GetComponent<FriendAvatar>();
            tmp.gameObject.name = friend.Name;
            tmp.steamID = friend.Id;
            tmp.buttonInvite.gameObject.SetActive(inviteable);

            Client.Instance.Friends.GetAvatar(Friends.AvatarSize.Large, friend.Id, tmp.OnImage);

            return tmp;
        }

        void OnDestroy()
        {
            if (Client.Instance != null)
            {
                Client.Instance.Lobby.OnLobbyCreated -= OnLobbyCreated;
                Client.Instance.Lobby.OnLobbyJoined -= OnLobbyJoined;
                Client.Instance.Lobby.OnUserInvitedToLobby -= OnUserInvitedToLobby;

                ClientManager.Instance.clientMessageEvents[NetworkMessageType.LobbyStartGame] -= OnMessageLobbyStartGame;
            }
        }
    }
}
