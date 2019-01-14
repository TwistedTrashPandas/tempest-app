using System;
using MastersOfTempest.Networking;
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

        private TeleportArea teleportArea = null;
        private bool teleported = false;

        private GUIStyle style;
        private string text = "";

        protected void Start()
        {
            style = new GUIStyle();
            style.fontSize = 50;
            style.richText = true;
            style.alignment = TextAnchor.MiddleCenter;
            style.normal.textColor = Color.white;
            style.normal.background = Texture2D.whiteTexture;
        }

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
            else if (teleported)
            {
                if (!(interactionsController.CurrentlyLookedAt is TeleportArea))
                {
                    text = "<b>E</b>\nTeleport Back";
                }

                if (Input.GetKeyDown(KeyCode.E))
                {
                    Teleport(teleportArea);
                    text = "";
                }
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
            interactionsController.Setup(CameraDirectionController.FirstPersonCamera, float.MaxValue, InteractionCheck, PlayerRole.Apprentice);
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
            InteractablePart interactablePart = ((InteractionEventArgs)args).InteractableObject;

            if (interactablePart is TeleportArea)
            {
                ((InteractionEventArgs)args).InteractableObject.GetComponent<MeshRenderer>().enabled = true;
                text = "<b>E</b>\nTeleport";
            }
            else if (interactablePart is RepairArea)
            {
                text = "<b>Left Mouse</b>\nRepair";
            }
        }

        private void OnPlayerInteracted(object sender, EventArgs args)
        {
            TriggerActionEvent(new ActionMadeEventArgs(((InteractionEventArgs)args).InteractableObject.GetAction()));
        }

        private void OnLostSight(object sender, EventArgs args)
        {
            if (((InteractionEventArgs)args).InteractableObject is TeleportArea)
            {
                ((InteractionEventArgs)args).InteractableObject.GetComponent<MeshRenderer>().enabled = false;
            }

            text = "";
        }

        public void Teleport (TeleportArea target)
        {
            bool goBack = teleported && !(interactionsController.CurrentlyLookedAt is TeleportArea);

            target.gameObject.GetComponent<TeleportActionNetworked>().TeleportOnServer(GetComponent<ServerObject>().serverID, goBack);

            if (goBack)
            {
                target = null;
                teleported = false;
            }
            else
            {
                teleported = true;
                teleportArea = target;
            }
        }

        public void Repair (RepairArea target)
        {
            // Send message to repair parts from the ShipPartManager
            FindObjectOfType<Ship>().RepairShipPartAreaOnServer(target.shipPartArea, 0.2f);
            animations.Repair();
        }

        private void OnGUI()
        {
            if (text.Length > 0)
            {
                GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f);

                GUIContent content = new GUIContent(text);
                Vector2 size = style.CalcSize(content);

                GUI.Label(new Rect((Screen.width / 2) - (size.x / 2), Screen.height - (size.y + 50) , size.x + 10, size.y + 10), content, style);
            }
        }
    }
}
