using System;
using MastersOfTempest.ShipBL;

public class InteractionEventArgs : EventArgs
{
    public InteractablePart InteractableObject { get; private set; }

    public InteractionEventArgs(InteractablePart obj)
    {
        InteractableObject = obj;
    }
}
