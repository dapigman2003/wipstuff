using Foundation;

namespace StS2Launcher.iOS;

/// <summary>
/// Single source for the release identity shown at the top of the launcher UI.
/// The display version is read from the built bundle so it cannot drift from Info.plist;
/// the candidate step/summary are hash-pinned and statically validated for every release.
/// </summary>
internal static class CurrentReleasePresentation
{
    public const string StepTitle =
        "STEP 27.0.10 — HARMONYSHAREDSTATE CCTOR IN-FLIGHT OBSERVABILITY";

    public const string MilestoneLine =
        "STEPS 01–26 PHYSICALLY CLOSED • 0.0.93 CROSSED T1–T4 • HARD STOP IS INSIDE HARMONYSHAREDSTATE::.CCTOR";

    public const string Summary =
        "Physical 0.0.93 self-identified correctly and crossed Gate T1–T4, proving the bounded host preservation preflight and exact HarmonySharedState runtime reflection return on device. Its last durable checkpoint was T5 immediately before RuntimeHelpers.RunClassConstructor(HarmonySharedState.TypeHandle), with PatchProcessor.Patch() and the launcher target still uninvoked. Step 27.0.10 keeps that exact cctor and patch path intact but arms bounded, output-only observers during the cctor for dedicated-ALC resolver activity and process assembly-load events, especially the generated HarmonySharedState and MonoMod.Utils.Cil.ILGeneratorProxy assemblies. No StS2 member is reflected, patched, or invoked.";

    public const string InitialStatus =
        "Status: Steps 01–26 are physically closed. Physical 0.0.93 establishes the current Step-27 frontier inside HarmonyLib.HarmonySharedState::.cctor after T1–T4 returned. Build 0.0.94 does not pre-run or bypass any HarmonySharedState internal operation: it adds durable in-flight resolver/assembly-load breadcrumbs around the unchanged RunClassConstructor call so the next abrupt termination can be attributed before singleton load, between singleton load and ILGeneratorProxy generation, or after both. Force-quit/relaunch before every Step-27 retry once Gate B has started.";

    public const string ExpectedDisplayVersion = "0.0.94";
    public const string ExpectedBuildVersion = "94";
    public const string GateSImplementationMarker =
        "bounded HarmonyMethod() descriptor; PatchProcessor.AddPrefix(MethodInfo) runtime invocation forbidden";
    public const string GateTImplementationMarker =
        "T1-T4 physically crossed; unchanged HarmonySharedState RunClassConstructor with bounded cctor resolver/AssemblyLoad observers; PatchProcessor.Patch() remains after T6";

    public static string DisplayVersion =>
        NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleShortVersionString")?.ToString() ?? "unknown";

    public static string DisplayBuild =>
        NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleVersion")?.ToString() ?? "unknown";

    public static bool BundleIdentityMatchesExpected =>
        string.Equals(DisplayVersion, ExpectedDisplayVersion, StringComparison.Ordinal) &&
        string.Equals(DisplayBuild, ExpectedBuildVersion, StringComparison.Ordinal);
}
