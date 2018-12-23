﻿using System;
using System.Collections;
using System.Collections.Generic;
using MastersOfTempest.PlayerControls.Spellcasting;
using MastersOfTempest.ShipBL;
using UnityEngine;

namespace MastersOfTempest.PlayerControls
{
    public class DrawEnergyAction : WizardAction
    {
        private Rune chargeType;
        private float chargeDuration;

        public DrawEnergyAction(Rune chargeType, float chargeDuration)
        {
            this.chargeType = chargeType;
            this.chargeDuration = chargeDuration;
        }

        public override void Execute(Gamemaster context)
        {
            GetWizardInput(context).StartCharging(chargeType, chargeDuration);
        }
    }
}
