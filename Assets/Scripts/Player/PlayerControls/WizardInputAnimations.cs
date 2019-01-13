﻿using System;
using System.Collections;
using System.Collections.Generic;
using MastersOfTempest.ShipBL;
using UnityEngine;

namespace MastersOfTempest.PlayerControls
{
    public class WizardInputAnimations : MonoBehaviour
    {
        public WizardInput WizardInput;
        public Camera FirstPersonCamera;
        private const string ArmsPrefabPath = "WizardArms";
        private WizardArmsController armsController;


        private void Start()
        {
            if (WizardInput == null)
            {
                throw new InvalidOperationException($"{nameof(WizardInput)} is not specified!");
            }
            var arms = Resources.Load<WizardArmsController>(ArmsPrefabPath);
            armsController = Instantiate(arms, FirstPersonCamera.transform);
            WizardInput.StartedCharging += OnChargeStarted;
            WizardInput.ChargingCancelled += OnChargeCancelled;
            WizardInput.ChargingCompleted += OnChargeCompleted;
            WizardInput.DischargeHit += OnDischargeHit;
            WizardInput.DischargeMiss += OnDischargeMiss;
        }

        private CoroutineCancellationToken chargeCancellationToken;
        private void OnChargeStarted(object sender, EventArgs args)
        {
            armsController.HoldSpell();
            var ps = WizardInput.GetCurrentInteractable() as PowerSource;
            if(ps != null)
            {
                chargeCancellationToken = new CoroutineCancellationToken();
                ps.Particles.StartChannel(armsController.SuckPoint, chargeCancellationToken);
            }
            Debug.Log("Animation for charge starting showed");
        }

        private void OnChargeCancelled(object sender, EventArgs args)
        {

            armsController.PulseRightHand();
            armsController.ReleaseSpell();
            if(chargeCancellationToken != null)
            {
                chargeCancellationToken.CancellationRequested = true;
                chargeCancellationToken = null;
            }
            Debug.Log("Animation for charge cancelled showed");
        }

        private void OnChargeCompleted(object sender, EventArgs args)
        {

            //TODO: some particles effect prob
            armsController.PulseRightHand();
            if(chargeCancellationToken != null)
            {
                chargeCancellationToken.CancellationRequested = true;
                chargeCancellationToken = null;
            }
            Debug.Log("Animation for charge completion showed");
        }

        private void OnDischargeHit(object sender, EventArgs args)
        {
            armsController.PulseRightHand();
            armsController.ReleaseSpell();
            Debug.Log("Animation for discharge hit showed");
        }

        private void OnDischargeMiss(object sender, EventArgs args)
        {

            armsController.PulseRightHand();
            armsController.ReleaseSpell();
            Debug.Log("Animation for discharge miss showed");
        }
    }
}
