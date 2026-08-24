namespace StS2Launcher.Step28.AheadOfLoadFixture;

/// <summary>
/// Project-owned Step 28 fixture. The source image returns value+1. Step 28 rewrites only
/// Adjustment() in a launcher-private copy before the assembly is ever admitted to the CLR.
/// InvokeTarget() retains a direct managed IL call to Target() so the physical test proves that
/// an ordinary in-assembly call observes the transformed image, not just reflection dispatch.
/// </summary>
public static class AheadOfLoadRewriteProbe
{
    public static int Adjustment() => 1;

    public static int Target(int value) => value + Adjustment();

    public static int InvokeTarget(int value) => Target(value);
}
