using System;
using UnityEngine;

namespace MastersOfTempest.PlayerControls
{
    public class SteerShip : PlayerAction
    {
        const float SteeringForceValue = 5000f;
        public enum SteeringDirection
        {
            Left,
            Right,
            Forward,
            Backward
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
            switch (direction)
            {
                case SteeringDirection.Left: forceDirection = -ship.transform.right; break;
                case SteeringDirection.Right: forceDirection = ship.transform.right; break;
                case SteeringDirection.Forward: forceDirection = ship.transform.forward; break;
                case SteeringDirection.Backward: forceDirection = -ship.transform.forward; break;
                default: throw new InvalidOperationException($"Unknown value {nameof(SteeringDirection)} of {direction}");
            }
            //TODO: duration for the force, or add as an impulse
            if ((ship.GetCurrenStatus().Condition & ShipBL.ShipCondition.Freezing) == ShipBL.ShipCondition.Freezing)
                forceDirection *= ship.GetFreezingSlowDown();
            ship.GetShipForceManipulator().AddForce(forceDirection * SteeringForceValue, .5f);
        }
    }
}
