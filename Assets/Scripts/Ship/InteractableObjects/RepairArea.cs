using MastersOfTempest.PlayerControls;

namespace MastersOfTempest.ShipBL
{
    public class RepairArea : InteractablePart
    {
        public override Access GetAccess()
        {
            return Access.Apprentice;
        }

        public override PlayerAction GetAction()
        {
            return new RepairAction(this);
        }

        public override float GetDistance()
        {
            // TODO
            return float.MaxValue;
        }
    }
}
