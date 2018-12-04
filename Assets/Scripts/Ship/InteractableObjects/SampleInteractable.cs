using MastersOfTempest.PlayerControls;

namespace MastersOfTempest.ShipBL
{
    public class SampleInteractable : InteractablePart
    {
        public override PlayerAction GetApprenticeAction()
        {
            return new MessageAction($"Sample interactable {transform.name} was interacted with!");
        }

        public override string GetResultDescription()
        {
            return "pay respect";
        }
    }
}
