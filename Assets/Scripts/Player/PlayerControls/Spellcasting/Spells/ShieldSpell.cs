using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MastersOfTempest.PlayerControls.Spellcasting
{
    public class ShieldSpell : Spell
    {
        public override Rune[] SpellSequence
        {
            get
            {
                return new Rune[] { Rune.Fire, Rune.Water, Rune.Fire, Rune.Water };
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