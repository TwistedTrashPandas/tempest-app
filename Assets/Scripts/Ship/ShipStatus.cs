using System;
using System.Collections;
using System.Collections.Generic;
using MastersOfTempest.PlayerControls;
using UnityEngine;

namespace MastersOfTempest.ShipBL
{
    public class ShipStatus
    {
        public event EventHandler ActionRequest;

        const float freezeSlowdown = .4f;
        public float Health { get; set; }
        public float Shield { get; set; }
        public ShipCondition Condition { get; private set; } = ShipCondition.None;

        public void AddCondition(ShipCondition condition)
        {
            if(condition.HasFlag(ShipCondition.Freezing))
            {
                if(!Condition.HasFlag(ShipCondition.Freezing))
                {
                    ActionRequest?.Invoke(this, new ActionMadeEventArgs(new SlowdownPlayersAction(freezeSlowdown)));
                }
            }
            if(condition.HasFlag(ShipCondition.Fragile))
            {
                //TODO: action for fragile on players
                // throw new NotImplementedException();
            }
            Condition |= condition;
        }

        public void RemoveCondition(ShipCondition condition)
        {
            if(condition.HasFlag(ShipCondition.Freezing) && Condition.HasFlag(ShipCondition.Freezing))
            {
                ActionRequest?.Invoke(this, new ActionMadeEventArgs(new SlowdownPlayersAction(1f / freezeSlowdown)));
            }
            if(condition.HasFlag(ShipCondition.Fragile))
            {
                //TODO: action for unfragile on players
                // throw new NotImplementedException();
            }
            Condition &= ~condition;
        }

        public void ResetCondition()
        {
            if(Condition.HasFlag(ShipCondition.Freezing))
            {
                ActionRequest?.Invoke(this, new ActionMadeEventArgs(new SlowdownPlayersAction(1f / freezeSlowdown)));
            }
            Condition = ShipCondition.None;
        }
    }
}
