using System;
using UnityEngine;

namespace MastersOfTempest.PlayerControls.QTE
{
    public class QTENewKeyEventArgs : EventArgs
    {
        public KeyCode AwaitedKey { get; private set; }
        public float TimeToReact { get; private set; }

        public QTENewKeyEventArgs(KeyCode neededKey, float timeToReact)
        {
            AwaitedKey = neededKey;
            TimeToReact = timeToReact;
        }
    }
}
