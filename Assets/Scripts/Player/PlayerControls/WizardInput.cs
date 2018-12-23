using System;
using System.Collections;
using System.Collections.Generic;
using MastersOfTempest.PlayerControls.QTE;
using MastersOfTempest.PlayerControls.Spellcasting;
using UnityEngine;

namespace MastersOfTempest.PlayerControls
{
    public class WizardInput : PlayerInputController
    {
        private QTEDriver QTEDriver;
        private SpellcastingController spellcastingController;
        private KeyCode lastPressed;
        private int counter;
        private const int triggerAmount = 3;

        private PlayerAction currentAction;
        private CoroutineCancellationToken currentCancellationToken;
        private bool isActive = true;

        private InteractionsController interactionsController;

        protected override void Awake()
        {
            base.Awake();
        }

        protected void Start()
        {
            SanityCheck();
            QTEDriver.Success += OnSuccess;
            QTEDriver.Fail += OnFail;
            spellcastingController.SpellCasted += OnSpellCasted;
        }

        public override void Interrupt()
        {
            throw new NotImplementedException();
        }

        public override void Resume()
        {
            isActive = true;
        }

        public override void Suppress()
        {
            isActive = false;
        }

        private void Update()
        {
            if (isActive)
            {
                if(Input.GetKeyDown(KeyCode.F))
                {
                    spellcastingController.Active = !spellcastingController.Active;
                    CameraDirectionController.Active = !CameraDirectionController.Active;
                }
            }
        }


        private void StartQTE(PlayerAction action)
        {
            this.Suppress();
            currentAction = action;
            currentCancellationToken = new CoroutineCancellationToken();
            QTEDriver.StartQuickTimeEvent(currentCancellationToken);
        }

        private void OnSuccess(object sender, EventArgs e)
        {
            TriggerActionEvent(new ActionMadeEventArgs(currentAction));
        }

        void OnFail(object sender, EventArgs e)
        {
            currentCancellationToken.CancellationRequested = true;
            currentCancellationToken = null;
            this.Resume();
        }

        private void SanityCheck()
        {
            if (QTEDriver == null)
            {
                throw new InvalidOperationException($"{nameof(QTEDriver)} is not specified!");
            }
            if (spellcastingController == null)
            {
                throw new InvalidOperationException($"{nameof(spellcastingController)} is not specified!");
            }
        }

        public override void Bootstrap()
        {
            QTEDriver = gameObject.AddComponent<QTEDriver>();
            var qteRenderer = gameObject.AddComponent<QTESimpleUIRenderer>();
            qteRenderer.Driver = QTEDriver;
            spellcastingController = gameObject.AddComponent<SpellcastingController>();
            gameObject.AddComponent<SpellReferenceInfo>();

            interactionsController = gameObject.AddComponent<InteractionsController>();
            interactionsController.Setup(CameraDirectionController.FirstPersonCamera, float.MaxValue, () => Input.GetKeyDown(KeyCode.F));
            var highlighter = gameObject.AddComponent<InteractionsHighlighter>();
            highlighter.InteractionsController = interactionsController;
        }

        private void OnSpellCasted(object sender, EventArgs args)
        {
            var spellArgs = (SpellCastedEventArgs) args;
            Debug.Log($"Spell {spellArgs.Spell.Name} casted!");
          
            spellcastingController.Active = false;
            CameraDirectionController.Active = true;
            var spellAction = spellArgs.Spell.GetPlayerAction();
            TriggerActionEvent(new ActionMadeEventArgs(spellAction));
            StartQTE(spellAction);
        }
    }
}
