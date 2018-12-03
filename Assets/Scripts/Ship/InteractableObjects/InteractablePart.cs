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
        public abstract PlayerAction GetApprenticeAction();

        /// <summary>
        /// Get the message to display to the player. Should be phrased to
        /// fit the template "Press {key} to {message}"
        /// </summary>
        /// <returns>message to display to the player</returns>
        public abstract string GetResultDescription();
    }
}
