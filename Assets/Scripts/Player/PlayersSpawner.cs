using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MastersOfTempest.Networking;
using Facepunch.Steamworks;
using UnityEngine.SceneManagement;

namespace MastersOfTempest.PlayerControls
{
    /// <summary>
    /// Spawns the player object for each player from the Lobby and then terminates self
    /// </summary>
    public class PlayersSpawner : MonoBehaviour
    {
        public Player PlayerPrefab;
        public GameObject apprenticePrefab;
        public GameObject wizardPrefab;
        private void Start()
        {
            if (PlayerPrefab == null)
            {
                throw new InvalidOperationException($"{nameof(PlayerPrefab)} is not specified!");
            }

            var players = NetworkManager.Instance.GetLobbyMemberIDs();

            //Set server scene as active for the player spawning
            Scene previouslyActiveScene = SceneManager.GetActiveScene();
            SceneManager.SetActiveScene(gameObject.scene);

            foreach (var playerId in players)
            {
                var role = (PlayerRole)int.Parse(Client.Instance.Lobby.GetMemberData(playerId, PlayerRoleExtensions.LobbyDataKey));

                var playerInstance = Instantiate(PlayerPrefab);
                playerInstance.PlayerId = playerId;
                var spawnPoint = role.GetSpawnPoint();
                if (spawnPoint != null)
                {
                    playerInstance.transform.position = spawnPoint.transform.position;
                    StartCoroutine(SetParent(playerInstance.transform, spawnPoint.transform));
                    //playerInstance.transform.SetParent(spawnPoint.transform, true);
                }

                switch (role)
                {
                    case PlayerRole.Apprentice:
                        GameObject.Instantiate(apprenticePrefab, playerInstance.transform.position, Quaternion.Euler(-90, 0, 0), playerInstance.transform);
                        break;
                    case PlayerRole.Wizard:
                        GameObject.Instantiate(wizardPrefab, playerInstance.transform.position, Quaternion.Euler(-90, 0, 0), playerInstance.transform);
                        break;
                    case PlayerRole.Spectator:
                        break;
                }
                Debug.Log($"Spawned for player# {playerId}");
            }
            //Set client scene back as active as it's the default behaviour
            SceneManager.SetActiveScene(previouslyActiveScene);

            //We don't need it anymore after its job is done
            Destroy(this.gameObject, 10f);
        }

        private IEnumerator SetParent(Transform child, Transform parent)
        {
            yield return new WaitForFixedUpdate();
            child.SetParent(parent.transform, true);
        }
    }
}
