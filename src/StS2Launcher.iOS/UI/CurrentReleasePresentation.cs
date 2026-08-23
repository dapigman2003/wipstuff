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
        "STEP 27.0.15 — REAL HARMONY TEST COMPILE HARDENING";

    public const string MilestoneLine =
        "STEPS 01–26 PHYSICALLY CLOSED • 0.0.98 TEST COMPILE STOP • CECIL TYPE ALIAS FIXED • REAL HARMONY 2.4.2 CI FIXTURE RETAINED";

    public const string Summary =
        "Physical 0.0.97 exposed the Mono.Cecil ReadingMode.Immediate regression, and 0.0.98 corrected production normalization to Deferred reads. Codemagic 0.0.98 then compiled StS2Launcher.Core but stopped compiling the newly added real-Harmony host regression because bare ICustomAttributeProvider was ambiguous between System.Reflection and Mono.Cecil. Step 27.0.15 makes that test type explicit with a CecilCustomAttributeProvider alias. The production Deferred-Cecil normalizer, exact merged Lib.Harmony 2.4.2 quarantined fixture, byte-immutable source requirement, byte-distinct 11-instruction runtime image, and Gate S/T behavior are unchanged. No StS2 member is reflected, patched, or invoked.";

    public const string InitialStatus =
        "Status: Steps 01–26 are physically closed. Physical 0.0.94 localized the original HarmonySharedState cctor crash before T6. Physical 0.0.97 exposed the eager-Cecil normalizer regression. Build 0.0.98 restored Deferred metadata handling but its new real-Harmony regression did not compile because ICustomAttributeProvider was ambiguous between System.Reflection and Mono.Cecil; production code itself compiled. Build 0.0.99 fixes only that test namespace collision and preserves the real Harmony 2.4.2 CI gate. If the normalized run reaches T6, the next evidence boundary is the single public PatchProcessor.Patch() call at T7; if that runtime-detour boundary fails, the documented next experiment is an interpreted post-publish launcher-owned probe before any architecture pivot.";

    public const string ExpectedDisplayVersion = "0.0.99";
    public const string ExpectedBuildVersion = "99";
    public const string GateSImplementationMarker =
        "bounded HarmonyMethod() descriptor; PatchProcessor.AddPrefix(MethodInfo) runtime invocation forbidden";
    public const string GateTImplementationMarker =
        "Gate-A 11-instruction iOS-normalized HarmonySharedState cctor loaded from memory; deferred Cecil source/audit; T5b RunClassConstructor uses direct state only; PatchProcessor.Patch() remains after T6";

    public static string DisplayVersion =>
        NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleShortVersionString")?.ToString() ?? "unknown";

    public static string DisplayBuild =>
        NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleVersion")?.ToString() ?? "unknown";

    public static bool BundleIdentityMatchesExpected =>
        string.Equals(DisplayVersion, ExpectedDisplayVersion, StringComparison.Ordinal) &&
        string.Equals(DisplayBuild, ExpectedBuildVersion, StringComparison.Ordinal);
}
