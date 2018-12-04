﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MastersOfTempest.PlayerControls.Spellcasting
{
    public class SteerLeftSpell : Spell
    {
        public override Rune[] SpellSequence
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override PlayerAction GetPlayerAction()
        {
            return new SteerShip(SteerShip.SteeringDirection.Left);
        }
    }
}
