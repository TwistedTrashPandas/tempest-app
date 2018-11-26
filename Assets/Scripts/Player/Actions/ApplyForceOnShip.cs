using UnityEngine;

namespace MastersOfTempest.PlayerControls
{
    /// <summary>
    /// Applies force on ship for the specified duration in seconds
    /// </summary>
    public class ApplyForceOnShip : PlayerAction
    {
        private Vector3 force;
        private float duration;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:MastersOfTempest.PlayerControls.ApplyForceOnShip"/> class.
        /// </summary>
        /// <param name="force">Force to be applied</param>
        /// <param name="duration">Duration in seconds</param>
        public ApplyForceOnShip(Vector3 force, float duration)
        {
            this.force = force;
            this.duration = duration;
        }

        public override void Execute(Gamemaster context)
        {
            context.GetShip().GetShipForceManipulator().AddForce(force, duration);
        }
    }
}
