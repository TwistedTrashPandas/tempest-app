using System;
using UnityEngine;

namespace MastersOfTempest.PlayerControls
{

    public abstract class PlayerInputController : MonoBehaviour
    {
        public Camera FirstPersonCamera;
        protected CameraDirectionController CameraDirectionController;

        public event EventHandler ActionMade;

        protected virtual void Start()
        {
            if (FirstPersonCamera == null)
            {
                throw new InvalidOperationException($"{nameof(FirstPersonCamera)} is not specified!");
            }
            CameraDirectionController = gameObject.AddComponent<CameraDirectionController>();
            CameraDirectionController.FirstPersonCamera = FirstPersonCamera;
        }

        public abstract void Interrupt();

        public abstract void Suppress();

        public abstract void Resume();

        public virtual void Bootstrap(Player player)
        {
            FirstPersonCamera = player.FirstPersonCamera;
        }
    
        protected void TriggerActionEvent(ActionMadeEventArgs args)
        {
            ActionMade?.Invoke(this, args);
        }
    }
}
