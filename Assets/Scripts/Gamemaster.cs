using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MastersOfTempest
{
    public class Gamemaster : MonoBehaviour
    {
        private Ship ship;
        private List<Player> players;

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
    }
}
