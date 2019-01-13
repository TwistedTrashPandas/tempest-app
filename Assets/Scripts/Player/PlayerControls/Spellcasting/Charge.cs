using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MastersOfTempest.PlayerControls.Spellcasting
{
    public enum Charge
    {
        None = 0,
        Wind = 1,
        Fire = 2,
        Water = 3,
        Earth = 4
    }

    public static class ChargeExtensions
    {
        public static string FriendlyName(this Charge Charge)
        {
            switch(Charge)
            {
                case Charge.Wind: return "Air";
                case Charge.Fire: return "Fire";
                case Charge.Water: return "Water";
                case Charge.Earth: return "Earth";
                case Charge.None: return "No charge";
                default: throw new InvalidOperationException($"Unexpected {nameof(Charge)} value of {Charge}");
            }
        }

        public static Color CorrespondingColor(this Charge Charge)
        {
            switch(Charge)
            {
                case Charge.Wind: return new Color(0.3843138f, 0.6509804f, 0.5529412f);
                case Charge.Fire: return new Color(0.5803922f, 0f, 0f);
                case Charge.Water: return new Color(0.1176471f, 0.2627451f, 0.5490196f);
                case Charge.Earth: return new Color(0.5607843f, 0.4156863f, 0.08235294f);
                case Charge.None: return Color.white;
                default: throw new InvalidOperationException($"Unexpected {nameof(Charge)} value of {Charge}");
            }
        }
    }
}
