using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MastersOfTempest.PlayerControls
{
    public abstract class ApprenticeAction : PlayerAction
    {
        protected static ApprenticeInput apprenticeInput;

        protected ApprenticeInput GetApprenticeInput(Gamemaster context)
        {
            if(apprenticeInput == null)
            {
                apprenticeInput = context.GetCurrentPlayer().GetComponent<ApprenticeInput>();
            }

            return apprenticeInput;
        }
    }
}
