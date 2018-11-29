using UnityEngine;

namespace MastersOfTempest.PlayerControls
{
    public class MessageAction : PlayerAction
    {
        private readonly string msg;
        public MessageAction(string message)
        {
            msg = message;
        }

        public override void Execute(Gamemaster context)
        {
            Debug.Log(msg);
        }
    }
}
