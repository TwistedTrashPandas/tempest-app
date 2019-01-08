using MastersOfTempest.ShipBL;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MastersOfTempest.PlayerControls
{
    public class RepairAction : ApprenticeAction
    {
        private RepairArea target;

        public RepairAction (RepairArea target)
        {
            this.target = target;
        }

        public override void Execute(Gamemaster context)
        {
            GetApprenticeInput(context).Repair(target);
        }
    }
}
