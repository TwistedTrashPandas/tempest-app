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
            whereItWasHit = where;
        }

        public override void Execute(Gamemaster context)
        {
            var players = context.GetPlayers();
            foreach (var player in players)
            {
                var camera = player.GetPlayerCameraController();
                /*
                    The ship is approx. 9 units long.
                    We want the intesity of the shaking to be max at 0 and drop by distance squared.
                 */
                const float maxSqrDist = 100f;
                var distanceCoeff = Mathf.Clamp01((player.transform.position - whereItWasHit).sqrMagnitude / maxSqrDist);

                float intensity = damageAmount * (1f - distanceCoeff);
                camera.ShakeCamera(intensity);
            }
        }
    }
}
