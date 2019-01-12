using System;
using MastersOfTempest.PlayerControls.QTE;
using MastersOfTempest.ShipBL;
using TMPro;
using UnityEngine;

namespace MastersOfTempest.PlayerControls
{
    /// <summary>
    /// Input controller for the apprentice (navigator) player.
    /// Allows interaction with objects that have proper tags
    /// by raycasting and pressing of the interaction key.
    /// </summary>
    public class ApprenticeInput : PlayerInputController
    {
        private const string InteractionMessagePrefabName = "UIPrefabs/Apprentice/InteractionMessage";

        private PlayerAction currentAction;
        private CoroutineCancellationToken currentCancellationToken;
        private bool isActive = true;
        private Camera firstPersonCamera;
        private InteractionsController interactionsController;
        private ApprenticeInputAnimations animations;

        protected void Update()
        {
            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                animations.Throw(firstPersonCamera);
            }
            else if (Input.GetKeyDown(KeyCode.Space))
            {
                animations.Meditate();
            }
        }

        public override void Interrupt()
        {
            if (currentCancellationToken != null)
            {
                currentCancellationToken.CancellationRequested = true;
                currentCancellationToken = null;
                currentAction = null;
            }
        }

        public override void Resume()
        {
            isActive = true;
        }

        public override void Suppress()
        {
            Interrupt();
            isActive = false;
        }

        public override void Bootstrap()
        {
            //Set up camera that is used to determine interactable objects
            firstPersonCamera = CameraDirectionController.FirstPersonCamera;

            //Create a message UI element to show hints to player
            interactionsController = gameObject.AddComponent<InteractionsController>();
            interactionsController.Setup(CameraDirectionController.FirstPersonCamera, float.MaxValue, InteractionCheck);
            interactionsController.NewInteractable += OnNewInteractable;
            interactionsController.PlayerInteracted += OnPlayerInteracted;
            interactionsController.LostSight += OnLostSight;

            // Spawn and attach hands
            Instantiate(Resources.Load<GameObject>("ApprenticeHands"), firstPersonCamera.transform, false);

            // Make sure that the hands are visible
            firstPersonCamera.nearClipPlane = 0.01f;

            animations = GetComponentInChildren<ApprenticeInputAnimations>();

            var highlighter = gameObject.AddComponent<InteractionsHighlighter>();
            highlighter.InteractionsController = interactionsController;
        }

        private bool InteractionCheck ()
        {
            if (interactionsController.CurrentlyLookedAt is TeleportArea)
            {
                return Input.GetKeyDown(KeyCode.E);
            }
            else if (interactionsController.CurrentlyLookedAt is RepairArea)
            {
                return Input.GetKeyDown(KeyCode.Mouse0);
            }

            return false;
        }

        private void OnNewInteractable(object sender, EventArgs args)
        {
            Debug.Log(nameof(OnNewInteractable));
        }

        private void OnPlayerInteracted(object sender, EventArgs args)
        {
            Debug.Log(nameof(OnPlayerInteracted));
            TriggerActionEvent(new ActionMadeEventArgs(((InteractionEventArgs)args).InteractableObject.GetAction()));
        }

        private void OnLostSight(object sender, EventArgs e)
        {
            Debug.Log(nameof(OnLostSight));
        }

        public void Teleport (TeleportArea target)
        {
            Debug.Log("TODO: Teleport to " + target.name);
        }

        public void Repair (RepairArea target)
        {
            // Find the ship network behaviour on the client
            // Send message to repair parts from the ShipPartManager
            Ship[] ships = FindObjectsOfType<Ship>();

            foreach (Ship ship in ships)
            {
                if (ship.gameObject.scene.Equals(Networking.GameClient.Instance.gameObject.scene))
                {
                    ship.RepairShipPartAreaOnServer(target.shipPartArea, 0.2f);
                    break;
                }
            }

            animations.Repair();
        }
    }
}
