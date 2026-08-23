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
        "STEP 27.0.20 — HASH-PINNED REAL-HARMONY NORMALIZER EXECUTION";

    public const string MilestoneLine =
        "STEPS 01–26 PHYSICALLY CLOSED • 0.0.103 RAN 212 HOST TESTS • 211 PASS / 1 MERGED-METADATA ASSUMPTION FAIL • PRODUCTION NORMALIZER UNCHANGED";

    public const string Summary =
        "Codemagic 0.0.103 compiled production and tests and executed all 212 host tests. The sole failure occurred before normalization because the merged official Harmony-Fat net9.0 binary contains multiple System.Runtime AssemblyRef rows, making SingleOrDefault an invalid provenance check. Step 27.0.20 removes all target-framework inference from merged AssemblyRef topology, pins the exact official Harmony-Fat 2.4.2 release archive and selected net9.0 DLL by SHA-256, and then executes the production Deferred-Cecil normalizer directly. ControlledHarmonyPatchExecution.cs and the 11-instruction HarmonySharedState cctor rewrite remain byte-for-byte unchanged. No StS2 member is reflected, patched, or invoked.";

    public const string InitialStatus =
        "Status: Steps 01–26 are physically closed. Codemagic 0.0.103 ran 212 tests at 211/212; the only failure was a duplicate System.Runtime AssemblyRef assumption before the real normalizer call. Build 0.0.104 replaces merged-reference inference with exact release/DLL SHA-256 provenance and proceeds directly to the normalizer. If CI passes, the next meaningful evidence is on-device T6, then the single public PatchProcessor.Patch() boundary at T7/T8.";

    public const string ExpectedDisplayVersion = "0.0.104";
    public const string ExpectedBuildVersion = "104";
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
