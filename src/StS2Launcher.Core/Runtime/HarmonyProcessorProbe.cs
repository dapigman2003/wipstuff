namespace StS2Launcher.Core;

/// <summary>
/// Launcher-owned inert target for Step 26 Harmony PatchProcessor creation. The method is never
/// patched or invoked by the Step 26 boundary; its MethodInfo is used only to prove that Harmony can
/// retain an ordinary host MethodBase inside an empty PatchProcessor without touching StS2 members.
/// </summary>
public static class HarmonyProcessorProbe
{
    public static int Target(int value) => value + 1;
}
