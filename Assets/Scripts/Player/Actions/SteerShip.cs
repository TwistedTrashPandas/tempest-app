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
            Down,
            HardLeft,
            HardRight
        }

        private SteeringDirection direction;
        private bool newSpellCast;

        public SteerShip(SteeringDirection direction, bool newSpellCast)
        {
            this.direction = direction;
            this.newSpellCast = newSpellCast;
        }

        public override void Execute(Gamemaster context)
        {
            Vector3 forceDirection;

            var ship = context.GetShip();

            Quaternion rotBefore = ship.transform.rotation;

            if (this.newSpellCast)
                ship.StoreRotation();
            
            ship.transform.rotation = ship.GetLastRotation();
            ship.transform.rotation = Quaternion.Euler(0f, ship.transform.rotation.eulerAngles.y, 0f);
            //ship.transform.rotation = originalRotation; // Quaternion.Euler(0f, rotBefore.eulerAngles.y, 0f);

            switch (direction)
            {
                case SteeringDirection.Left: forceDirection = Vector3.Normalize(-ship.transform.right + ship.transform.forward); break;
                case SteeringDirection.Right: forceDirection = Vector3.Normalize(ship.transform.right + ship.transform.forward); break;
                case SteeringDirection.HardLeft: forceDirection = -ship.transform.right; break;
                case SteeringDirection.HardRight: forceDirection = ship.transform.right; break;
                case SteeringDirection.Forward: forceDirection = ship.transform.forward; break;
                case SteeringDirection.Backward: forceDirection = -ship.transform.forward; break;
                case SteeringDirection.Up: forceDirection = ship.transform.up * 4f; break;
                case SteeringDirection.Down: forceDirection = -ship.transform.up * 4f; break;
                default: throw new InvalidOperationException($"Unknown value {nameof(SteeringDirection)} of {direction}");
            }

            //TODO: duration for the force, or add as an impulse
            if ((ship.GetCurrenStatus().Condition & ShipBL.ShipCondition.Freezing) == ShipBL.ShipCondition.Freezing)
                forceDirection *= ship.GetFreezingSlowDown();

            ship.transform.rotation = rotBefore;

            ship.GetShipForceManipulator().AddForce(forceDirection.normalized * SteeringForceValue, .5f);
        }
    }
}
