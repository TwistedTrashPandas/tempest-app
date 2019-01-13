using System;
namespace MastersOfTempest.ShipBL
{
    public class ShipPartHitEventArgs : EventArgs
    {
        public float damageAmount;

        public ShipPartHitEventArgs(float damage)
        {
            damageAmount = damage;
        }
    }
}
