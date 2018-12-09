namespace MastersOfTempest.Networking
{
    public enum NetworkMessageType
    {
        PingPong,
        LobbyChat,
        LobbyStartGame,
        Initialization,
        ServerObject,
        ServerObjectList,
        DestroyServerObject,
        NetworkBehaviour,
        NetworkBehaviourInitialized
    };
}
