﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MastersOfTempest.PlayerControls.Spellcasting
{
    public abstract class Spell
    {
        public bool newSpellCast { get; set; }
        public abstract PlayerAction GetPlayerAction();
        public abstract Charge[] SpellSequence { get; }
        public abstract string Name { get; }
        public abstract Color SpellColor { get; }
    }
}
