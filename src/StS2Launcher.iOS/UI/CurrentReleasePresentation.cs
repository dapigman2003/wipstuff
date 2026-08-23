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
        "STEP 27.0.14 — DEFERRED CECIL NORMALIZATION + REAL HARMONY CI GATE";

    public const string MilestoneLine =
        "STEPS 01–26 PHYSICALLY CLOSED • 0.0.97 GATE A ROOT-CAUSED • DEFERRED CECIL WRITE • REAL HARMONY 2.4.2 CI FIXTURE";

    public const string Summary =
        "Physical 0.0.97 failed before Gate B because the new HarmonySharedState runtime-image normalizer reopened exact 0Harmony with Mono.Cecil ReadingMode.Immediate. Immediate mode eagerly decodes custom-attribute constructor arguments and forced the deliberately rejecting metadata resolver to resolve System.ComponentModel.EditorBrowsableState. Step 27.0.14 uses ReadingMode.Deferred for both the source rewrite and normalized-image audit; Cecil's writer completes a deferred module with custom-attribute resolution disabled. Codemagic now also restores exact merged Lib.Harmony 2.4.2 netstandard2.0 as a quarantined test fixture and runs the real production normalizer against it, requiring byte-immutable source plus the exact byte-distinct 11-instruction runtime image. Gate S/T patch behavior is otherwise unchanged. No StS2 member is reflected, patched, or invoked.";

    public const string InitialStatus =
        "Status: Steps 01–26 are physically closed. Physical 0.0.94 localized the original HarmonySharedState cctor crash before T6. Builds 0.0.95 and 0.0.96 exposed compile/test-harness regressions before device execution. Physical 0.0.97 then exposed a deterministic Gate-A Cecil regression: the normalizer alone used ReadingMode.Immediate and eagerly resolved an unrelated EditorBrowsableState custom-attribute argument. Build 0.0.98 restores the project's established metadata-only Deferred-read discipline end-to-end and adds a Codemagic regression test against the real upstream Harmony 2.4.2 binary so this class of failure must be caught before IPA publication. If the normalized run reaches T6, the next evidence boundary is the single public PatchProcessor.Patch() call at T7; if that runtime-detour boundary fails, the documented next experiment is an interpreted post-publish launcher-owned probe before any architecture pivot.";

    public const string ExpectedDisplayVersion = "0.0.98";
    public const string ExpectedBuildVersion = "98";
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
