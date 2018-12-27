using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MastersOfTempest.PlayerControls.Spellcasting
{
    public class SteerLeftSpell : Spell
    {
        public override Charge[] SpellSequence
        {
            get
            {
                return new Charge[] {Charge.Earth, Charge.Fire, Charge.Water, Charge.Wind};
            }
        }

        public override String Name
        {
            get
            {
                return "Right halfwind";
            }
        }

        public override PlayerAction GetPlayerAction()
        {
            return new SteerShip(SteerShip.SteeringDirection.Left);
        }
    }
}
