using MastersOfTempest.ShipBL;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MastersOfTempest.PlayerControls
{
    public class TeleportAction : ApprenticeAction
    {
        private TeleportArea target;

        public TeleportAction (TeleportArea target)
        {
            this.target = target;
        }

        public override void Execute(Gamemaster context)
        {
            GetApprenticeInput(context).Teleport(target);
        }
    }
}
