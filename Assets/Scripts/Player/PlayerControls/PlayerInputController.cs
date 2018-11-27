using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MastersOfTempest.PlayerControls
{

    public abstract class PlayerInputController : MonoBehaviour
    {
        public event EventHandler ActionMade;

        public abstract void Interrupt();

        public abstract void Suppress();

        public abstract void Resume();

        protected void TriggerActionEvent(ActionMadeEventArgs args)
        {
            ActionMade?.Invoke(this, args);
        }
    }
}
