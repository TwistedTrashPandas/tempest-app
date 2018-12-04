using System;
using MastersOfTempest.PlayerControls.QTE;
using MastersOfTempest.ShipBL;
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
        private QTEDriver QTEDriver;
        private UnityEngine.UI.Text InteractionMessage;

        private const string InteractableTagName = "Interactable";

        private KeyCode interactionKey = KeyCode.F;

        private PlayerAction currentAction;
        private CoroutineCancellationToken currentCancellationToken;
        private bool isActive = true;
        private Transform currentlyLookedAt;
        private InteractablePart currentInteractable;


        protected override void Start()
        {
            base.Start();
            SanityCheck();

            QTEDriver.Success += OnQTESuccess;
            QTEDriver.Fail += OnQTEFail;
        }

        private void Update()
        {
            if(isActive)
            {
                RaycastHit hit;
                var ray = FirstPersonCamera.ViewportPointToRay(new Vector3(.5f, .5f, 0f));
                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.transform.CompareTag(InteractableTagName))
                    {
                        if(currentlyLookedAt != hit.transform)
                        {
                            currentlyLookedAt = hit.transform;
                            currentInteractable = currentInteractable.GetComponent<InteractablePart>();
                            InteractionMessage.text = $"Press {interactionKey} to {currentInteractable.GetResultDescription()}";
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
                    InteractionMessage.text = "";
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
            if (InteractionMessage == null)
            {
                throw new InvalidOperationException($"{nameof(InteractionMessage)} is not specified!");
            }

        }

        public override void Bootstrap(Player player)
        {
            base.Bootstrap(player);
            InteractionMessage = player.Text;

            QTEDriver = gameObject.AddComponent<QTEDriver>();

            var qteRenderer = gameObject.AddComponent<QTESimpleUIRenderer>();
            qteRenderer.Driver = QTEDriver;
            qteRenderer.InfoForUser = player.Text;
        }
    }
}
