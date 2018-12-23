using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MastersOfTempest.PlayerControls
{
    public class WizardInputAnimations : MonoBehaviour
    {
        public WizardInput WizardInput;

        private void Start()
        {
            if (WizardInput == null)
            {
                throw new InvalidOperationException($"{nameof(WizardInput)} is not specified!");
            }
            WizardInput.StartedCharging += OnChargeStarted;
            WizardInput.ChargingCancelled += OnChargeCancelled;
            WizardInput.ChargingCompleted += OnChargeCompleted;
            WizardInput.DischargeHit += OnDischargeHit;
            WizardInput.DischargeMiss += OnDischargeMiss;
        }

        private void OnChargeStarted(object sender, EventArgs args)
        {
            //TODO: add animation
            Debug.Log("Animation for charge starting showed");
        }

        private void OnChargeCancelled(object sender, EventArgs args)
        {
            //TODO: add animation
            Debug.Log("Animation for charge cancelled showed");   
        }

        private void OnChargeCompleted(object sender, EventArgs args)
        {
            //TODO: add animation
            Debug.Log("Animation for charge completion showed");
        }

        private void OnDischargeHit(object sender, EventArgs args)
        {
            //TODO: add animation
            Debug.Log("Animation for discharge hit showed");
        }

        private void OnDischargeMiss(object sender, EventArgs args)
        {
            //TODO: add animation
            Debug.Log("Animation for discharge miss showed");   
        }
    }
}
