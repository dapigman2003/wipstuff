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
        "STEP 27.0.11 — IOS HARMONYSHAREDSTATE AOT NORMALIZATION";

    public const string MilestoneLine =
        "STEPS 01–26 PHYSICALLY CLOSED • 0.0.94 CONFIRMED T5 CCTOR FAILURE • IOS-SAFE HARMONYSHAREDSTATE NORMALIZATION ACTIVE";

    public const string Summary =
        "Physical 0.0.94 self-identified correctly and again terminated inside HarmonySharedState::.cctor after the dedicated Step-27 load context successfully resolved netstandard. Step 27.0.11 converts that diagnosis into a bounded compatibility fix: Gate A deterministically rewrites only HarmonySharedState::.cctor in an in-memory runtime image of the verified prepared 0Harmony 2.4.2 assembly, replacing dynamic shared-state assembly creation and Mono StackFrame FieldRefAccess initialization with direct launcher-private dictionaries, actualVersion=102, and a null methodAddressRef. The prepared/source/live files remain untouched, and PatchProcessor.Patch() is still forbidden until the normalized initializer returns and T6 validates. No StS2 member is reflected, patched, or invoked.";

    public const string InitialStatus =
        "Status: Steps 01–26 are physically closed. Physical 0.0.94 confirms the current hard stop remains inside HarmonyLib.HarmonySharedState::.cctor after host netstandard resolution succeeds. Build 0.0.95 removes the two runtime-only operations that are not needed in this single-version private Harmony context: dynamic HarmonySharedState singleton generation/loading and StackFrame FieldRefAccess construction. Gate A audits and reopens an exact 11-instruction normalized cctor, Gate B loads that byte-distinct image from memory while re-verifying the untouched prepared SHA, and Gate T runs/validates it before any PatchProcessor.Patch(). Force-quit/relaunch before every Step-27 retry once Gate B has started.";

    public const string ExpectedDisplayVersion = "0.0.95";
    public const string ExpectedBuildVersion = "95";
    public const string GateSImplementationMarker =
        "bounded HarmonyMethod() descriptor; PatchProcessor.AddPrefix(MethodInfo) runtime invocation forbidden";
    public const string GateTImplementationMarker =
        "Gate-A 11-instruction iOS-normalized HarmonySharedState cctor loaded from memory; T5b RunClassConstructor uses direct state only; PatchProcessor.Patch() remains after T6";

    public static string DisplayVersion =>
        NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleShortVersionString")?.ToString() ?? "unknown";

    public static string DisplayBuild =>
        NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleVersion")?.ToString() ?? "unknown";

    public static bool BundleIdentityMatchesExpected =>
        string.Equals(DisplayVersion, ExpectedDisplayVersion, StringComparison.Ordinal) &&
        string.Equals(DisplayBuild, ExpectedBuildVersion, StringComparison.Ordinal);
}
