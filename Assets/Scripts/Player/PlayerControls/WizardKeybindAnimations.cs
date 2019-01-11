using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MastersOfTempest.PlayerControls
{
    public class WizardKeybindAnimations : MonoBehaviour
    {
        public WizardArmsController armsController;

        void Start()
        {
            if (armsController == null)
            {
                throw new InvalidOperationException($"{nameof(armsController)} is not specified!");
            }
        }

        void Update()
        {
            if (Input.GetKeyDown("q"))
            {
                armsController.TakeOutBook();
            }
            if (Input.GetKeyDown("w"))
            {
                armsController.HideBook();

            }
            if (Input.GetKeyDown("e"))
            {
                armsController.HoldSpell();
            }
            if (Input.GetKeyDown("t"))
            {
                armsController.PulseRightHand();
            }
            if (Input.GetKeyDown("r"))
            {
                armsController.ReleaseSpell();
            }
        }
    }
}
