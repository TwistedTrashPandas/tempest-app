using System;
using System.Collections;
using System.Collections.Generic;
using MastersOfTempest.PlayerControls.QTE;
using MastersOfTempest.PlayerControls.Spellcasting;
using MastersOfTempest.ShipBL;
using UnityEngine;

namespace MastersOfTempest.PlayerControls
{
    public class WizardInput : PlayerInputController
    {
        public event EventHandler StartedCharging;
        public event EventHandler ChargingCancelled;
        public event EventHandler ChargingCompleted;
        public event EventHandler DischargeHit;
        public event EventHandler DischargeMiss;

        private PlayerAction currentAction;
        private CoroutineCancellationToken currentCancellationToken;
        private bool isActive = true;

        private InteractionsController interactionsController;

        private enum WizardState
        {
            Idle = 0,
            Charging = 1,
            Charged = 2
        }
        private WizardState currentState;
        private Rune currentChargeType;

        private const int MouseToCharge = 0;
        private const float DischargeDistance = float.MaxValue;
        private float chargingTime;
        private float timeToCharge = 2f;
        /*
            Wizard input.
            1. Wizard input is controlled by actions and interactions controller
            2. We have states: 
                i. Idle
                ii. Drawing energy
                iii. Energy drawn

            When player is idle, we do nothing. When we receive a command to start drawing energy, this component sends an event and starts timer. If we at this state receive "object lost sight" or player stopped pressing the button, we interrupt the energy drawing, send interrupt event and go to idle

            When player completed the energy drawing, we go to state (iii). Here on mouse release we will try to send the energy to the object we're looking at, sending corresponding event on hit and miss.

            Also we need a spells controller that will check the player's input and send commands to the ship
         */

        public void StartCharging(Rune chargeType, float time)
        {
            if (Mathf.Approximately(time, 0f))
            {
                currentState = WizardState.Charged;
                currentChargeType = chargeType;
                ChargingCompleted?.Invoke(this, new ChargingEventArgs(currentChargeType));
            }
            else
            {
                currentState = WizardState.Charging;
                currentChargeType = chargeType;
                chargingTime = 0f;
                timeToCharge = time;
                StartedCharging?.Invoke(this, new ChargingEventArgs(currentChargeType));
            }
        }

        private void Discharge()
        {
            RaycastHit hit;
            var ray = interactionsController.FirstPersonCamera.ViewportPointToRay(new Vector3(.5f, .5f, 0f));
            if (Physics.Raycast(ray, out hit, DischargeDistance, interactionsController.FirstPersonCamera.cullingMask))
            {
                var recepticle = hit.transform.GetComponent<PowerRecepticle>();
                if (recepticle != null)
                {
                    recepticle.Charge(currentChargeType);
                    currentState = WizardState.Idle;
                    DischargeHit?.Invoke(this, new ChargingEventArgs(currentChargeType));
                }
                else
                {
                    currentState = WizardState.Idle;
                    DischargeMiss?.Invoke(this, new ChargingEventArgs(currentChargeType));
                }
            }
            else
            {
                currentState = WizardState.Idle;
                DischargeMiss?.Invoke(this, new ChargingEventArgs(currentChargeType));
            }
        }

        private void Update()
        {
            switch (currentState)
            {
                case WizardState.Idle: break;
                case WizardState.Charging: ChargingUpdate(); break;
                case WizardState.Charged: ChargedUpdate(); break;
                default: throw new InvalidOperationException($"Unexpected {nameof(WizardState)} value of {currentState}");
            }
        }

        private void ChargingUpdate()
        {
            if (Input.GetMouseButton(MouseToCharge))
            {
                chargingTime += Time.deltaTime;
                if (chargingTime > timeToCharge)
                {
                    currentState = WizardState.Charged;
                    ChargingCompleted?.Invoke(this, new ChargingEventArgs(currentChargeType));
                }
            }
            else
            {
                currentState = WizardState.Idle;
                ChargingCancelled?.Invoke(this, new ChargingEventArgs(currentChargeType));
            }
        }

        private void ChargedUpdate()
        {
            if (Input.GetMouseButtonUp(MouseToCharge))
            {
                Discharge();
            }
        }

        private void OnUserLostSight(object sender, EventArgs args)
        {
            if(currentState == WizardState.Charging)
            {
                currentState = WizardState.Idle;
                ChargingCancelled?.Invoke(this, new ChargingEventArgs(currentChargeType));                
            }
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

        public override void Bootstrap()
        {
            interactionsController = gameObject.AddComponent<InteractionsController>();
            interactionsController.Setup(CameraDirectionController.FirstPersonCamera, float.MaxValue, UserInteracts);
            interactionsController.PlayerInteracted += OnUserInteraction;
            interactionsController.LostSight += OnUserLostSight;

            var highlighter = gameObject.AddComponent<InteractionsHighlighter>();
            highlighter.InteractionsController = interactionsController;

            var animations = gameObject.AddComponent<WizardInputAnimations>();
            animations.WizardInput = this;
        }

        private bool UserInteracts()
        {
            return Input.GetMouseButtonDown(MouseToCharge);
        }

        private void OnUserInteraction(object sender, EventArgs args)
        {
            TriggerActionEvent(new ActionMadeEventArgs(((InteractionEventArgs) args).InteractableObject.GetAction()));
        }
    }
}
