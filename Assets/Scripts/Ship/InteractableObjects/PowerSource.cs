using System;
using System.Collections;
using System.Collections.Generic;
using MastersOfTempest.PlayerControls;
using MastersOfTempest.PlayerControls.Spellcasting;
using UnityEngine;

namespace MastersOfTempest.ShipBL
{
    public class PowerSource : InteractablePart
    {
        public Charge Charge;
        private const float ChargeTime = 1f;

        public override Access GetAccess()
        {
            return Access.Wizard;
        }

        public override PlayerAction GetAction()
        {
            return new DrawEnergyAction(Charge, ChargeTime);
        }

        public override float GetDistance()
        {
            //TODO: tweak this value
            return float.MaxValue;
        }
    }
}
