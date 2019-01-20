using System;
using UnityEngine;

namespace MastersOfTempest.PlayerControls
{
    public class SteerShip : PlayerAction
    {
        const float SteeringForceValue = 1000f;
        public enum SteeringDirection
        {
            Left,
            Right,
            Forward,
            Backward,
            Up,
            Down
        }
        private SteeringDirection direction;
        public SteerShip(SteeringDirection direction)
        {
            this.direction = direction;
        }

        public override void Execute(Gamemaster context)
        {
            Vector3 forceDirection;
            var ship = context.GetShip();
            Quaternion rotBefore = ship.transform.rotation;
            ship.transform.rotation = Quaternion.Euler(0f, rotBefore.eulerAngles.y, 0f);
            switch (direction)
            {
                case SteeringDirection.Left: forceDirection = -ship.transform.right; break;
                case SteeringDirection.Right: forceDirection = ship.transform.right; break;
                case SteeringDirection.Forward: forceDirection = ship.transform.forward; break;
                case SteeringDirection.Backward: forceDirection = -ship.transform.forward; break;
                case SteeringDirection.Up: forceDirection = ship.transform.up; break;
                case SteeringDirection.Down: forceDirection = -ship.transform.up; break;
                default: throw new InvalidOperationException($"Unknown value {nameof(SteeringDirection)} of {direction}");
            }

            //TODO: duration for the force, or add as an impulse
            if ((ship.GetCurrenStatus().Condition & ShipBL.ShipCondition.Freezing) == ShipBL.ShipCondition.Freezing)
                forceDirection *= ship.GetFreezingSlowDown();

            ship.transform.rotation = rotBefore;

            ship.GetShipForceManipulator().AddForce(forceDirection * SteeringForceValue, .5f);
        }
    }
}
