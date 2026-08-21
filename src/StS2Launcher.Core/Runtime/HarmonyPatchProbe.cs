using System.Runtime.CompilerServices;
using System.Threading;

namespace StS2Launcher.Core;

/// <summary>
/// Launcher-owned Step 27 patch target and prefix. Both methods are deliberately tiny and never
/// touch StS2 state. The target has deterministic original behavior; the prefix deterministically
/// replaces the result and skips the original so the physical run can prove patch and unpatch
/// behavior without ambiguity.
/// </summary>
public static class HarmonyPatchProbe
{
    private static int _targetCalls;
    private static int _prefixCalls;

    public static int TargetCalls => Volatile.Read(ref _targetCalls);
    public static int PrefixCalls => Volatile.Read(ref _prefixCalls);

    public static void ResetCounters()
    {
        Volatile.Write(ref _targetCalls, 0);
        Volatile.Write(ref _prefixCalls, 0);
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static int Target(int value)
    {
        Interlocked.Increment(ref _targetCalls);
        return value + 1;
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static bool Prefix(int value, ref int __result)
    {
        Interlocked.Increment(ref _prefixCalls);
        __result = value + 1000;
        return false;
    }
}
