using MastersOfTempest.PlayerControls;

namespace MastersOfTempest.ShipBL
{
    public class SampleInteractable : InteractablePart
    {
        public override Access GetAccess()
        {
            return Access.Players;
        }

        public override PlayerAction GetAction()
        {
            return new MessageAction($"Sample interactable {transform.name} was interacted with!");
        }

        public override float GetDistance()
        {
            return float.MaxValue;
        }
    }
}
