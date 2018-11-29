using System;

namespace MastersOfTempest.PlayerControls
{
    public class ActionMadeEventArgs : EventArgs
    {
        public PlayerAction Action { get; private set; }
        public ActionMadeEventArgs(PlayerAction action)
        {
            Action = action;
        }
    }
}
