using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine.Networking;

public static class UnityWebRequestAwaiterExtensions
{
    public static TaskAwaiter GetAwaiter(this UnityWebRequestAsyncOperation op)
    {
        var tcs = new TaskCompletionSource<object>();
        op.completed += _ => tcs.SetResult(null);
        return ((Task)tcs.Task).GetAwaiter();
    }
}
