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
        public ParticlesSucker Particles;

        private void Awake()
        {
            if (Particles == null)
            {
                throw new InvalidOperationException($"{nameof(Particles)} is not specified!");
            }
            if(Charge == Charge.None)
            {
                throw new InvalidOperationException($"{nameof(Charge)} should not be {nameof(Charge.None)}");
            }
        }

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
