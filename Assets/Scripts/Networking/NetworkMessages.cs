namespace MastersOfTempest.Networking
{
    // Add new types here (e.g. the name of the class that uses it)
    // Make sure to assign and never change the index
    public enum NetworkMessageType
    {
        Empty = 0,
        LobbyChat = 1,
        LobbyStartGame = 2,
        ServerObject = 3,
        DestroyServerObject = 4,
        NetworkBehaviourInitialized = 5,
        PingPong = 6,
        ForceManipulator = 7,
        NoMessages = 8,
        EnvObjects = 9,
        ServerObjectList = 10,
        InitializeServer = 11,
    };
}
