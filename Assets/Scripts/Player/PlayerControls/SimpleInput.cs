using System;
using UnityEngine;
using MastersOfTempest.PlayerControls.QTE;

namespace MastersOfTempest.PlayerControls
{
    /// <summary>
    /// Listens to user's keyboard input and moves the ship if player pressed 3 same keys in a row
    /// </summary>
    public class SimpleInput : PlayerInputController
    {
        public QTEDriver QTEDriver;

        private Array keyCodes;
        private KeyCode lastPressed;
        private int counter;
        private const int triggerAmount = 3;

        private PlayerAction currentAction;
        private CoroutineCancellationToken currentCancellationToken;
        private bool isActive = true;

        private void Awake()
        {
            keyCodes = Enum.GetValues(typeof(KeyCode));
            SanityCheck();
        }

        private void OnEnable()
        {
            QTEDriver.Success += OnSuccess;
            QTEDriver.Fail += OnFail;
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
                foreach (KeyCode vKey in keyCodes)
                {
                    if (Input.GetKeyDown(vKey))
                    {
                        if (lastPressed == vKey)
                        {
                            ++counter;
                        }
                        else
                        {
                            counter = 1;
                            lastPressed = vKey;
                        }
                    }
                }
                if (counter == triggerAmount)
                {
                    KeyPressedEnoughTimes(lastPressed);
                    counter = 0;
                }
            }
        }

        private void KeyPressedEnoughTimes(KeyCode key)
        {
            const float exampleDuration = .5f;
            switch (key)
            {
                case KeyCode.A: StartQTE(new ApplyForceOnShip(Vector3.left * 500f, exampleDuration)); break;
                case KeyCode.D: StartQTE(new ApplyForceOnShip(Vector3.right * 500f, exampleDuration));  break;
                default: Debug.Log("Press A or D to get something happening"); break;
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
        }
    }
}
