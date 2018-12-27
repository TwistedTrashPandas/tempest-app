using System;
using MastersOfTempest.PlayerControls;
using UnityEngine;

namespace MastersOfTempest.ShipBL
{
    public abstract class InteractablePart : MonoBehaviour
    {
        /// <summary>
        /// Get action to be executed when interacted with
        /// </summary>
        /// <returns>Player action to be executed.</returns>
        public abstract PlayerAction GetAction();
        
        /// <summary>
        /// Get minimum distance the player has to be within the object in order
        /// to interact
        /// </summary>
        /// <returns>Minimum distance for interaction to happen</returns>
        public abstract float GetDistance();

        public abstract Access GetAccess();

        [Flags]
        public enum Access
        {
            None = 0,
            Apprentice = 1,
            Wizard = 2,
            Players = 3,
            Spectator = 4
        }
    }
}
