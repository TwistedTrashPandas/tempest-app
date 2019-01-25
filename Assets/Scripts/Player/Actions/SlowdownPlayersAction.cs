using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MastersOfTempest.PlayerControls
{
    public class SlowdownPlayersAction : PlayerAction
    {
        public float MultiplierValue { get; private set; }
        public SlowdownPlayersAction(float multiplier)
        {
            MultiplierValue = multiplier;
        }


        public override void Execute(Gamemaster context)
        {
            var players = context.GetPlayers();
            players.ForEach(player => player.CharacterPositionManipulator.ChangeSpeed(MultiplierValue));
        }
    }
}
