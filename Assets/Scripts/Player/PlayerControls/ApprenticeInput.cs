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
        private QTEDriver QTEDriver;

        private const string InteractableTagName = "Interactable";

        private KeyCode interactionKey = KeyCode.F;

        private PlayerAction currentAction;
        private CoroutineCancellationToken currentCancellationToken;
        private bool isActive = true;
        private Transform currentlyLookedAt;
        private InteractablePart currentInteractable;
        private TMP_Text interactionMessage;
        private Camera firstPersonCamera;

        protected void Start()
        {
            SanityCheck();

            QTEDriver.Success += OnQTESuccess;
            QTEDriver.Fail += OnQTEFail;
        }

        private void Update()
        {
            if(isActive)
            {
                RaycastHit hit;
                var ray = firstPersonCamera.ViewportPointToRay(new Vector3(.5f, .5f, 0f));
                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.transform.CompareTag(InteractableTagName))
                    {
                        if(currentlyLookedAt != hit.transform)
                        {
                            currentlyLookedAt = hit.transform;
                            currentInteractable = currentInteractable.GetComponent<InteractablePart>();
                            interactionMessage.text = $"Press {interactionKey} to {currentInteractable.GetResultDescription()}";
                        }
                        if (Input.GetKeyDown(interactionKey))
                        {
                            currentAction = currentInteractable.GetApprenticeAction();
                            //Start QTE; supress this.
                            StartQTE();
                        }
                    }
                }
                else
                {
                    currentlyLookedAt = null;
                    interactionMessage.text = "";
                }
            }
        }

        private void StartQTE()
        {
            Suppress();
            currentCancellationToken = new CoroutineCancellationToken();
            QTEDriver.StartQuickTimeEvent(currentCancellationToken);
        }

        private void OnQTESuccess(object sender, EventArgs args)
        {
            TriggerActionEvent(new ActionMadeEventArgs(currentAction));
        }

        private void OnQTEFail(object sender, EventArgs args)
        {
            Interrupt();
            Resume();
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

        private void SanityCheck()
        {
            if (QTEDriver == null)
            {
                throw new InvalidOperationException($"{nameof(QTEDriver)} is not specified!");
            }
            if (interactionMessage == null)
            {
                throw new InvalidOperationException($"{nameof(interactionMessage)} is not specified!");
            }

        }

        public override void Bootstrap()
        {
            //Setup the QTE driver
            QTEDriver = gameObject.AddComponent<QTEDriver>();
            //Add renderer for the QTE events
            var qteRenderer = gameObject.AddComponent<QTESimpleUIRenderer>();
            qteRenderer.Driver = QTEDriver;
            //Set up camera that is used to determine interactable objects
            firstPersonCamera = CameraDirectionController.FirstPersonCamera;
            //Create a message UI element to show hints to player
            interactionMessage = UIManager.GetInstance().SpawnUIElement<TMP_Text>(InteractionMessagePrefabName);
        }
    }
}
