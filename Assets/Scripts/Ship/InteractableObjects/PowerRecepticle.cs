using System;
using System.Collections;
using System.Collections.Generic;
using MastersOfTempest.PlayerControls;
using MastersOfTempest.PlayerControls.Spellcasting;
using UnityEngine;

namespace MastersOfTempest.ShipBL
{
    public class PowerRecepticle : InteractablePart
    {
        private Rune CurrentCharge;

        private bool IsCharged;

        public override Access GetAccess()
        {
            return Access.Wizard;
        }

        public override PlayerAction GetAction()
        {
            if(IsCharged)
            {
                //TODO: animations for charge cancellations
                IsCharged = false;
                return new DrawEnergyAction(CurrentCharge, 0f);
            }
            else 
            {
                return PlayerAction.Empty;
            }
        }

        public override float GetDistance()
        {
            //TODO: tweak this value
            return float.MaxValue;
        }

        public void Charge(Rune chargeType)
        {
            //TODO: animation for being charged/not charged
            CurrentCharge = chargeType;
            IsCharged = true;
        }
    }
}
