namespace MastersOfTempest.Networking
{
    public enum NetworkMessageType
    {
        PingPong,
        LobbyChat,
        LobbyStartGame,
        ServerObject,
        ServerObjectList,
        DestroyServerObject,
        NetworkBehaviour,
        NetworkBehaviourInitialized,
        ClientReadyForInitialization
    };
}
