namespace MastersOfTempest.PlayerControls
{
    /// <summary>
    /// Base class for PlayerActions. Children should override Execute method.
    /// </summary>
    public abstract class PlayerAction
    {
        //todo: perhaps return result, or accept callback as a parameter
        public abstract void Execute(Gamemaster context);

        public static PlayerAction Empty { get; } = new EmptyAction();
    }
}
