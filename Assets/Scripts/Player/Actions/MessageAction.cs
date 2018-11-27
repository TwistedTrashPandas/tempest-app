using UnityEngine;

namespace MastersOfTempest.PlayerControls
{
    public class MessageAction : PlayerAction
    {
        public override void Execute(Gamemaster context)
        {
            Debug.Log("This is a message");
        }
    }
}
