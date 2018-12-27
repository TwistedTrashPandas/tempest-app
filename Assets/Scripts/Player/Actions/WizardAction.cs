using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MastersOfTempest.PlayerControls
{
    public abstract class WizardAction : PlayerAction
    {
        protected static WizardInput wizardInput;

        protected WizardInput GetWizardInput(Gamemaster context)
        {
            if(wizardInput == null)
            {
                wizardInput = context.GetCurrentPlayer().GetComponent<WizardInput>();
            }
            return wizardInput;
        }
    }
}
