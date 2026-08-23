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
        "STEP 27.0.19 — NET9 SURROGATE REFERENCE-GRAPH ASSERTION FIX";

    public const string MilestoneLine =
        "STEPS 01–26 PHYSICALLY CLOSED • 0.0.102 RAN 212 HOST TESTS • 211 PASS / 1 TEST-ASSUMPTION FAIL • PRODUCTION NORMALIZER UNCHANGED";

    public const string Summary =
        "Codemagic 0.0.102 finally acquired the official Harmony-Fat 2.4.2 net9.0 structural surrogate, compiled production and tests, and executed all 212 host tests. 211 passed. The sole failure was a test-only assumption that a net9 implementation must have no netstandard AssemblyRef; the official net9 binary legitimately retains that compatibility reference. Step 27.0.19 removes that negative inference, positively proves net9 selection from the exact release archive member plus System.Runtime 9.0 metadata, and leaves the production Deferred-Cecil normalizer and 11-instruction HarmonySharedState cctor rewrite byte-for-byte unchanged. No StS2 member is reflected, patched, or invoked.";

    public const string InitialStatus =
        "Status: Steps 01–26 are physically closed. Codemagic 0.0.102 reached the real Harmony regression and ran 212 host tests at 211/212; only the surrogate test's false no-netstandard-reference assertion failed before normalization was invoked. Build 0.0.103 corrects that host assertion without changing production runtime code. If CI passes, the next meaningful evidence is on-device T6, then the single public PatchProcessor.Patch() boundary at T7/T8.";

    public const string ExpectedDisplayVersion = "0.0.103";
    public const string ExpectedBuildVersion = "103";
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
