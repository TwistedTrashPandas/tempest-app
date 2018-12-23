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
        public Rune Rune;
        public float ChargeTime = 2f;

        public override Access GetAccess()
        {
            return Access.Wizard;
        }

        public override PlayerAction GetAction()
        {
            return new DrawEnergyAction(Rune, ChargeTime);
        }

        public override float GetDistance()
        {
            //TODO: tweak this value
            return float.MaxValue;
        }
    }
}