using System;
using System.Collections.Generic;
using MastersOfTempest.Networking;
using MastersOfTempest.ShipBL;

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

        public Ship GetShip()
        {
            return ship;
        }
    }
}
