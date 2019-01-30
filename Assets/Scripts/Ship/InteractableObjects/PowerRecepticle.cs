using System;
using System.Collections;
using System.Collections.Generic;
using MastersOfTempest.Glow;
using MastersOfTempest.PlayerControls;
using MastersOfTempest.PlayerControls.Spellcasting;
using UnityEngine;

namespace MastersOfTempest.ShipBL
{
    [RequireComponent(typeof(PowerRecepticleController))]
    public class PowerRecepticle : InteractablePart
    {
        //TODO: when charge changed we need to stop the energy drawing of a wizard if any
        public Charge CurrentCharge { get; private set; }
        public MeshRenderer mat;
        /// <summary>
        /// The index used by the wizard input for "auto-aim"
        /// </summary>
        public int Index;
        private PowerRecepticleController controller;
        private GlowObject glowObj;

        private void Awake()
        {
            controller = GetComponent<PowerRecepticleController>();
            if (controller == null)
            {
                throw new InvalidOperationException($"{nameof(controller)} is not specified!");
            }
            if (mat == null)
            {
                throw new InvalidOperationException($"{nameof(mat)} is not specified!");
            }
            glowObj = GetComponent<GlowObject>();
            if (glowObj == null)
            {
                throw new InvalidOperationException($"{nameof(glowObj)} is not specified!");
            }
        }

        public override Access GetAccess()
        {
            return Access.Wizard;
        }

        /*
            Problem:
            PowerRecepticle should execute all BL on the server. InteractablePart works only on the ClientSide.
            We need another component that will be NetworkBehaviour and manage the charge state, etc.
            ClientSide part should know that it exists and tell it to charge/discharge


         */

        public override PlayerAction GetAction()
        {
            if (CurrentCharge == MastersOfTempest.PlayerControls.Spellcasting.Charge.None)
            {
                return PlayerAction.Empty;
            }
            else
            {
                //TODO: animations for charge cancellations

                controller.SetCharge(MastersOfTempest.PlayerControls.Spellcasting.Charge.None);
                return new DrawEnergyAction(CurrentCharge, 0f);
            }
        }

        public override float GetDistance()
        {
            //TODO: tweak this value
            return float.MaxValue;
        }

        /// <summary>
        /// Set the charge of this recepticle. Should be used only
        /// by the PowerRecepticleController.
        /// </summary>
        /// <param name="chargeType">Desured charge type</param>
        [Obsolete("This method should be called by " + nameof(PowerRecepticleController) + " only")]
        public void Charge(Charge chargeType)
        {
            //TODO: animation for being charged/not charged
            CurrentCharge = chargeType;
            glowObj.GlowColor = CurrentCharge.CorrespondingColor();
            glowObj.TurnGlowOn();
            mat.material.color = CurrentCharge.CorrespondingColor();;
            Debug.Log($"Received charge {CurrentCharge}");
        }

        /// <summary>
        /// Request controller to set the charge
        /// </summary>
        /// <param name="chargeType">Desired charge type</param>
        public void RequestCharge(Charge chargeType)
        {
            Debug.Log($"Requested charge {chargeType}");
            controller.SetCharge(chargeType);
        }

        public void Destabilise()
        {
            //TODO: play animation for the stabilisation
            Debug.Log($"Destabilising from {CurrentCharge}!!!");
        }
    }
}
