namespace MastersOfTempest
{
    /// <summary>
    /// Pass this as an argument to coroutines that you want to cancel from the outside
    /// </summary>
    public class CoroutineCancellationToken
    {
        public bool CancellationRequested { get; set; } = false;
    }
}
