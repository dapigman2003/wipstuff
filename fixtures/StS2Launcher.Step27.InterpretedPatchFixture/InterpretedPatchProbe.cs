namespace StS2Launcher.Step27.InterpretedPatchFixture;

public static class InterpretedPatchProbe
{
    public static int TargetCalls;
    public static int PrefixCalls;

    public static void ResetCounters()
    {
        TargetCalls = 0;
        PrefixCalls = 0;
    }

    public static int Target(int value)
    {
        TargetCalls++;
        return value + 1;
    }

    public static int InvokeTarget(int value) => Target(value);

    public static bool Prefix(int value, ref int __result)
    {
        PrefixCalls++;
        __result = value + 1000;
        return false;
    }
}
