using MastersOfTempest.PlayerControls;

namespace MastersOfTempest.ShipBL
{
    public class TeleportArea : InteractablePart
    {
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
            // TODO
            return float.MaxValue;
        }
    }
}
