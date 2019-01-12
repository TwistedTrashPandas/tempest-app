using System.Collections;
using System.Collections.Generic;
using MastersOfTempest.PlayerControls;
using UnityEngine;

namespace MastersOfTempest
{
    public class ShakeCameraAction : PlayerAction
    {
        private float damageAmount;
        private Vector3 whereItWasHit;

        public ShakeCameraAction(float damage, Vector3 where)
        {
            damageAmount = damage;
            where = whereItWasHit;
        }

        public override void Execute(Gamemaster context)
        {
            var camera = context.GetCurrentPlayer();
            Debug.Log("CAMERA SHAKING!");
            //TODO: get camera and shake!
        }
    }
}
