﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MastersOfTempest.PlayerControls.Spellcasting
{
    public enum Rune
    {
        Wind,
        Fire,
        Water,
        Ice
    }

    public static class RuneExtensions
    {
        public static string FriendlyName(this Rune rune)
        {
            switch(rune)
            {
                case Rune.Wind: return "Air";
                case Rune.Fire: return "Fire";
                case Rune.Water: return "Water";
                case Rune.Ice: return "Ice";
                default: throw new InvalidOperationException($"Unexpected {nameof(Rune)} value of {rune}");
            }
        }
    }
}