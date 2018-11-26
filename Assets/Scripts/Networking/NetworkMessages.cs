using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MasterOfTempest.Networking
{
    // Add new types here (e.g. the name of the class that uses it)
    // Make sure to assign and never change the index
    public enum NetworkMessageType
    {
        Empty = 0,
        LobbyChat = 1,
        LobbyStartGame = 2,
        ServerObject = 3,
        DestroyGameObject = 4,
        NetworkBehaviourInitialized = 5,
        PushRigidbodyUp = 6
    };
}
