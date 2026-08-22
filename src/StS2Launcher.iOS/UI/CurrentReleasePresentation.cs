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
        "STEP 27.0.9 — CRASH-CHECKPOINT RELEASE PROVENANCE HARDENING";

    public const string MilestoneLine =
        "STEPS 01–26 PHYSICALLY CLOSED • SUPPLIED S1 TEXT MATCHES 0.0.89, NOT 0.0.92 • PATCH PATH UNCHANGED";

    public const string Summary =
        "The supplied 2026-08-22 Gate-S/S1 checkpoint says PatchProcessor.AddPrefix(MethodInfo) was entered. That exact executable S1 path was removed after physical 0.0.89 and does not exist in candidate 0.0.92, whose Gate S uses the bounded HarmonyMethod() descriptor path and never invokes AddPrefix(MethodInfo). Step 27.0.9 keeps the 0.0.92 Gate O/S/T runtime behavior unchanged and hardens crash-checkpoint provenance with installed bundle version/build, source candidate identity, and an explicit Gate-S implementation marker before continuing the same measured PatchProcessor.Patch() frontier. No StS2 member is reflected, patched, or invoked.";

    public const string InitialStatus =
        "Status: Steps 01–26 are physically closed. Physical 0.0.90 reached PatchProcessor.Patch() and hard-terminated there; physical 0.0.91 failed normally at Gate O before Gate T. The newly supplied checkpoint has a fresh timestamp but contains the archived 0.0.89 AddPrefix S1 text, so it cannot be attributed to executable 0.0.92 source. Build 0.0.93 does not change the patch algorithm: it adds fail-closed bundle/source identity checking and self-identifying crash telemetry, then retries the unchanged bounded Gate-S descriptor and decomposed Gate-T runtime boundary. Force-quit/relaunch before every Step-27 retry once Gate B has started.";

    public const string ExpectedDisplayVersion = "0.0.93";
    public const string ExpectedBuildVersion = "93";
    public const string GateSImplementationMarker =
        "bounded HarmonyMethod() descriptor; PatchProcessor.AddPrefix(MethodInfo) runtime invocation forbidden";

    public static string DisplayVersion =>
        NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleShortVersionString")?.ToString() ?? "unknown";

    public static string DisplayBuild =>
        NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleVersion")?.ToString() ?? "unknown";

    public static bool BundleIdentityMatchesExpected =>
        string.Equals(DisplayVersion, ExpectedDisplayVersion, StringComparison.Ordinal) &&
        string.Equals(DisplayBuild, ExpectedBuildVersion, StringComparison.Ordinal);
}
