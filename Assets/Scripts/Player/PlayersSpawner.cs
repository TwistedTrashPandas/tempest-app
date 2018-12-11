using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MastersOfTempest.Networking;

namespace MastersOfTempest.PlayerControls
{
    /// <summary>
    /// Spawns the player object for each player from the Lobby and then terminates self
    /// </summary>
    public class PlayersSpawner : MonoBehaviour
    {
        public Player PlayerPrefab;
        private void Awake()
        {
            if (PlayerPrefab == null)
            {
                throw new InvalidOperationException($"{nameof(PlayerPrefab)} is not specified!");
            }

            var players = NetworkManager.Instance.GetLobbyMemberIDs();
            foreach(var playerId in players)
            {
                //Set parent so that it goes to the proper scene
                var playerInstance = Instantiate(PlayerPrefab, this.transform);
                playerInstance.PlayerId = playerId;
                //Remove the parent so that we are not destroyed with the spawner
                playerInstance.transform.parent = null;
                Debug.Log($"Spawned for player# {playerId}");
            }
            //We don't need it anymore after its job is done
            Destroy(this.gameObject);
        }
    }
}
