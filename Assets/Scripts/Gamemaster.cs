﻿using System;
using System.Collections.Generic;
using MastersOfTempest.Networking;
using MastersOfTempest.ShipBL;
using MastersOfTempest.Environment;
using UnityEngine;

namespace MastersOfTempest
{
    /// <summary>
    /// Provides context for all game objects that want to interact with each other.
    /// Behaves the same on Client and Server.
    /// </summary>
    public class Gamemaster : NetworkBehaviour
    {
        private Ship ship;
        private List<Player> players;
        private Player currentPlayer;
        private EnvironmentManager envManager;
        private VoiceChat voiceChat;

        private void Awake()
        {
            players = new List<Player>();
        }

        public void Register(Ship shipToRegister)
        {
            if (ship != null)
            {
                throw new InvalidOperationException("Game master already has a Ship object registered!");
            }
            ship = shipToRegister;
        }

        public void Register(Player player)
        {
            if (players.Contains(player))
            {
                throw new InvalidOperationException($"Player object {nameof(player)} has already been registered!");
            }
            players.Add(player);
        }

        public Player GetCurrentPlayer()
        {
            return currentPlayer;
        }

        public List<Player> GetPlayers()
        {
            return players;
        }

        public void SetCurrentPlayer(Player player)
        {
            currentPlayer = player;
        }

        public void Register(EnvironmentManager envMng)
        {
            if (envManager != null)
            {
                throw new InvalidOperationException("Game master already has an EnvironmentManager object registered!");
            }
            envManager = envMng;
        }

        public void Register(VoiceChat vC)
        {
            if (voiceChat != null)
            {
                throw new InvalidOperationException($"Gamemaster object {(this.gameObject.name)} already has a voice chat object registered!");
            }
            voiceChat = vC;
        }

        public Ship GetShip()
        {
            return ship;
        }

        public VoiceChat GetVoiceChat()
        {
            return voiceChat;
        }

        public EnvironmentManager GetEnvironmentManager()
        {
            return envManager;
        }
    }
}
