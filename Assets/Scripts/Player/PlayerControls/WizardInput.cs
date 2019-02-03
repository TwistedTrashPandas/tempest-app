using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        public event EventHandler ShowHideBook;
        public event EventHandler NextPage;
        public event EventHandler PreviousPage;

        private PlayerAction currentAction;
        private bool isActive = true;

        private InteractionsController interactionsController;

        private enum WizardState
        {
            Idle = 0,
            Charging = 1,
            Charged = 2
        }
        private WizardState currentState;
        private Charge currentChargeType;

        private const int MouseToCharge = 0;
        private const float DischargeDistance = float.MaxValue;

        private const KeyCode TakeOutBook = KeyCode.F;
        private const KeyCode NextPageBook = KeyCode.E;
        private const KeyCode PreviousPageBook = KeyCode.Q;
        private Dictionary<KeyCode, int> KeysToIndexMapping = new Dictionary<KeyCode, int>() { { KeyCode.Alpha1, 1 }, { KeyCode.Alpha2, 2 }, { KeyCode.Alpha3, 3 }, { KeyCode.Alpha4, 4 } };
        private List<PowerRecepticle> powerRecepticles;
        private bool bookOpen;
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
        protected void Start()
        {
            const int ExpectedPowerRecepticlesCount = 4;
            powerRecepticles = FindObjectsOfType<PowerRecepticle>().Where(pr => pr.gameObject.scene == this.gameObject.scene).ToList();
            if (powerRecepticles.Count != ExpectedPowerRecepticlesCount)
            {
                throw new InvalidOperationException($"Unexpected amount of power recepticles! Expected {ExpectedPowerRecepticlesCount}, found: {powerRecepticles.Count}");
            }
        }


        public void StartCharging(Charge chargeType, float time)
        {
            if (Mathf.Approximately(time, 0f))
            {
                currentState = WizardState.Charged;
                currentChargeType = chargeType;
                StartedCharging?.Invoke(this, new ChargingEventArgs(currentChargeType));
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
                    recepticle.RequestCharge(currentChargeType);
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
            CheckBookInput();
            switch (currentState)
            {
                case WizardState.Idle: break;
                case WizardState.Charging: ChargingUpdate(); break;
                case WizardState.Charged: ChargedUpdate(); break;
                default: throw new InvalidOperationException($"Unexpected {nameof(WizardState)} value of {currentState}");
            }
        }

        private void CheckBookInput()
        {
            if (Input.GetKeyDown(TakeOutBook))
            {
                bookOpen ^= true;
                ShowHideBook?.Invoke(this, EventArgs.Empty);
            }
            else if (bookOpen)
            {
                if (Input.GetKeyDown(NextPageBook))
                {
                    NextPage?.Invoke(this, EventArgs.Empty);
                }
                else if (Input.GetKeyDown(PreviousPageBook))
                {
                    PreviousPage?.Invoke(this, EventArgs.Empty);
                }
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
            else if (KeysToIndexMapping.Keys.Any(key => Input.GetKeyDown(key)))
            {
                var recepticle = powerRecepticles.First(pr => pr.Index == KeysToIndexMapping[KeysToIndexMapping.Keys.First(key => Input.GetKeyDown(key))]);
                recepticle.RequestCharge(currentChargeType);
                currentState = WizardState.Idle;
                DischargeHit?.Invoke(this, new ChargingEventArgs(currentChargeType));
            }
        }

        private void OnUserLostSight(object sender, EventArgs args)
        {
            if (currentState == WizardState.Charging)
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
            interactionsController.Setup(float.MaxValue, UserInteracts, PlayerRole.Wizard);
            interactionsController.PlayerInteracted += OnUserInteraction;
            interactionsController.LostSight += OnUserLostSight;

            var highlighter = gameObject.AddComponent<InteractionsHighlighter>();
            highlighter.InteractionsController = interactionsController;

            var animations = gameObject.AddComponent<WizardInputAnimations>();
            animations.WizardInput = this;
            animations.FirstPersonCamera = Camera.main;
        }

        private bool UserInteracts()
        {
            return Input.GetMouseButtonDown(MouseToCharge);
        }

        private void OnUserInteraction(object sender, EventArgs args)
        {
            TriggerActionEvent(new ActionMadeEventArgs(((InteractionEventArgs)args).InteractableObject.GetAction()));
        }

        public InteractablePart GetCurrentInteractable()
        {
            return interactionsController.CurrentlyLookedAt;
        }
    }
}
