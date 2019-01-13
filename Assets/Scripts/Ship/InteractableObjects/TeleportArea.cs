using MastersOfTempest.PlayerControls;
using UnityEngine;

namespace MastersOfTempest.ShipBL
{
    public class TeleportArea : InteractablePart
    {
        public Transform returnTransform;

        public override Access GetAccess()
        {
            return Access.Apprentice;
        }

        public override PlayerAction GetAction()
        {
            return new TeleportAction(this);
        }

        public override float GetDistance()
        {
            return float.MaxValue;
        }
    }
}
