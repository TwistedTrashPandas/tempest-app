using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MastersOfTempest.PlayerControls.Spellcasting
{
    public static class SpellList
    {
        public static List<Spell> Spells {get;} = new List<Spell>
        {
            new AccelerateSpell(),
            //new ShieldSpell(),
            new SlowdownSpell(),
            new SteerLeftSpell(),
            new SteerRightSpell(),
            new SteerUpSpell(),
            new SteerDownSpell()
            // new SuperVisionSpell()
        };
    }
}
