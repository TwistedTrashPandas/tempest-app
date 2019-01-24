using System;
using System.Collections;
using System.Collections.Generic;
using MastersOfTempest.ShipBL;
using UnityEngine;
using MastersOfTempest.PlayerControls.Spellcasting;

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
            WizardInput.ShowHideBook += OnShowHideBook;
            WizardInput.NextPage += OnNextPage;
            WizardInput.PreviousPage += OnPreviousPage;
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

        private void OnShowHideBook(object sender, EventArgs args)
        {
            armsController.ToggleBook();
        }

        private void OnNextPage(object sender, EventArgs args)
        {
            armsController.NextPage();
        }

        private void OnPreviousPage(object sender, EventArgs args)
        {
            armsController.PrevPage();
        }

        private void OnChargeCancelled(object sender, EventArgs args)
        {

            armsController.PulseRightHand();
            armsController.ReleaseSpell();
            var charge = ((ChargingEventArgs)args).Charge;
            for (int i = 0; i < armsController.DissipatePS.Length; i++)
            {
                var main = armsController.DissipatePS[i].main;
                main.startColor = charge.CorrespondingColor();
                armsController.DissipatePS[i].Play();
            }
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
            var charge = ((ChargingEventArgs)args).Charge;
            for (int i = 0; i < armsController.HoldPS.Length; i++)
            {
                var main = armsController.HoldPS[i].main;
                main.startColor = charge.CorrespondingColor();
                armsController.HoldPS[i].Play();
            }
            if (chargeCancellationToken != null)
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
            for (int i = 0; i < armsController.HoldPS.Length; i++)
            {
                armsController.HoldPS[i].Stop();
            }
            var charge = ((ChargingEventArgs)args).Charge;
            var main = armsController.FeedPS.ParticlesColor = charge.CorrespondingColor();
            var token = new CoroutineCancellationToken();
            armsController.FeedPS.GetComponent<ParticleSystem>().startLifetime = 0.25f;
            StartCoroutine(token.TimedCancel(.25f));
            print(WizardInput.GetCurrentInteractable().transform);
            armsController.FeedPS.StartChannel(WizardInput.GetCurrentInteractable().transform, token);
            Debug.Log("Animation for discharge hit showed");
        }

        private void OnDischargeMiss(object sender, EventArgs args)
        {
            for (int i = 0; i < armsController.HoldPS.Length; i++)
            {
                armsController.HoldPS[i].Stop();
            }
            var charge = ((ChargingEventArgs)args).Charge;
            for (int i = 0; i < armsController.DissipatePS.Length; i++)
            {
                var main = armsController.DissipatePS[i].main;
                main.startColor = charge.CorrespondingColor();
                armsController.DissipatePS[i].Play();
            }
            armsController.PulseRightHand();
            armsController.ReleaseSpell();
            Debug.Log("Animation for discharge miss showed");
        }
    }
}
