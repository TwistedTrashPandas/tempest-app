﻿using System;
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

        private KeyCode interactionKey = KeyCode.F;

        private PlayerAction currentAction;
        private CoroutineCancellationToken currentCancellationToken;
        private bool isActive = true;
        private Camera firstPersonCamera;
        private InteractionsController interactionsController;

        protected void Start()
        {
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
            interactionsController.Setup(CameraDirectionController.FirstPersonCamera, float.MaxValue, () => Input.GetKeyDown(interactionKey));
            interactionsController.NewInteractable += OnNewInteractable;
            interactionsController.PlayerInteracted += OnPlayerInteracted;
            interactionsController.LostSight += OnLostSight;

            var highlighter = gameObject.AddComponent<InteractionsHighlighter>();
            highlighter.InteractionsController = interactionsController;
        }

        private void OnNewInteractable(object sender, EventArgs e)
        {
            Debug.Log(nameof(OnNewInteractable));
        }

        private void OnPlayerInteracted(object sender, EventArgs e)
        {
            Debug.Log(nameof(OnPlayerInteracted));
        }

        private void OnLostSight(object sender, EventArgs e)
        {
            Debug.Log(nameof(OnLostSight));
        }
    }
}
