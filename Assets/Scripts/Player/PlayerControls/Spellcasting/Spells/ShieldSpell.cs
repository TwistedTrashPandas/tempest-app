using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MastersOfTempest.PlayerControls.Spellcasting
{
    public class ShieldSpell : Spell
    {
        public override Charge[] SpellSequence
        {
            get
            {
                return new Charge[] { Charge.Fire, Charge.Water, Charge.Fire, Charge.Water };
            }
        }

        public override String Name
        {
            get
            {
                return "Shield";
            }
        }

        public override PlayerAction GetPlayerAction()
        {
            return new ShieldShip();
        }
    }
}
