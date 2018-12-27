using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MastersOfTempest.PlayerControls.Spellcasting
{
    public class AccelerateSpell : Spell
    {
        public override Charge[] SpellSequence
        {
            get
            {
                return new Charge[] {Charge.Fire, Charge.Fire, Charge.Fire, Charge.Fire};
            }
        }

        public override String Name
        {
            get
            {
                return "Tailwind";
            }
        }

        public override PlayerAction GetPlayerAction()
        {
            return new SteerShip(SteerShip.SteeringDirection.Forward);
        }
    }
}
