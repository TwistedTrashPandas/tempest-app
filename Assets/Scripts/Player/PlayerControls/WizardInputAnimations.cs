using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MastersOfTempest.PlayerControls
{
    public class WizardInputAnimations : MonoBehaviour
    {
        public WizardInput WizardInput;
        //public Animator anim; 

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

            //anim = GetComponent<Animator>();
            //anim.Play("Armature | Idle");
            
        }

        private void OnChargeStarted(object sender, EventArgs args)
        {
            
            //anim.CrossFade("Armature|GrabMagicRight", 0.5f);
            //anim.CrossFade("Armature|RightHandGrab", 0.5f);
            //anim.CrossFade("Armature|RightHandHold", 0.1f);
            Debug.Log("Animation for charge starting showed");
        }

        private void OnChargeCancelled(object sender, EventArgs args)
        {
            
            //anim.CrossFade("Armature|Idle", 0.5f);
            Debug.Log("Animation for charge cancelled showed");   
        }

        private void OnChargeCompleted(object sender, EventArgs args)
        {
            
            //anim.CrossFade("Armature|Idle", 0.5f);
            Debug.Log("Animation for charge completion showed");
        }

        private void OnDischargeHit(object sender, EventArgs args)
        {
            //anim.CrossFade("Armature|RightHandGrab", 0.5f);
            Debug.Log("Animation for discharge hit showed");
        }

        private void OnDischargeMiss(object sender, EventArgs args)
        {

            //anim.CrossFade("Armature|RightHandGrab", 0.5f); //Placeholder Animation atm
            Debug.Log("Animation for discharge miss showed");   
        }
    }
}
