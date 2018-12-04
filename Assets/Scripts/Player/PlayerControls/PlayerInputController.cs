using System;
using UnityEngine;

namespace MastersOfTempest.PlayerControls
{

    public abstract class PlayerInputController : MonoBehaviour
    {
        protected CameraDirectionController CameraDirectionController;

        public event EventHandler ActionMade;

        private void Awake() 
        {
            CameraDirectionController = gameObject.AddComponent<CameraDirectionController>();
        }

        public abstract void Interrupt();

        public abstract void Suppress();

        public abstract void Resume();

        public abstract void Bootstrap();
        
        protected void TriggerActionEvent(ActionMadeEventArgs args)
        {
            ActionMade?.Invoke(this, args);
        }
    }
}
