using System.Collections;

namespace MastersOfTempest
{
    /// <summary>
    /// Pass this as an argument to coroutines that you want to cancel from the outside
    /// </summary>
    public class CoroutineCancellationToken
    {
        public static CoroutineCancellationToken Empty { get; } = new CoroutineCancellationToken();



        public bool CancellationRequested { get; set; } = false;
    }

    public static class CoroutineCancellationTokenExtensions
    {
        public static IEnumerator TimedCancel(this CoroutineCancellationToken token, float time)
        {
            yield return new UnityEngine.WaitForSeconds(time);
            token.CancellationRequested = true;
        }
    }
}
