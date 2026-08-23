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
        "STEP 27.0.13 — SYNTHETIC PREFLIGHT SCOPE HARDENING";

    public const string MilestoneLine =
        "STEPS 01–26 PHYSICALLY CLOSED • 0.0.96 COMPILED • 209/211 HOST TESTS PASS • PRODUCTION NORMALIZATION RETAINED";

    public const string Summary =
        "Codemagic proved 0.0.96 compiles and executes the host suite: 209/211 tests passed. The two failures were synthetic-boundary regressions because Gate A unconditionally applied the new exact real-0Harmony patch-engine normalization audit to randomized minimal Harmony-like fixtures that are intentionally only valid through Gates A–N. Step 27.0.13 scopes the compatibility rewrite to the canonical production target identity only: exact 0Harmony 2.4.2 still requires the full audited fingerprint and a byte-distinct normalized runtime image, while internal synthetic replay retains its original fixture bytes exactly. The public production constructor, normalized cctor, Gate T ordering, and PatchProcessor.Patch() boundary are unchanged. No StS2 member is reflected, patched, or invoked.";

    public const string InitialStatus =
        "Status: Steps 01–26 are physically closed. Physical 0.0.94 remains the latest device evidence: the original HarmonySharedState cctor still stops before T6 after netstandard resolution succeeds. Build 0.0.95 did not compile. Build 0.0.96 fixed that compile defect and Codemagic ran 211 host tests, with 209 passing and two synthetic A–N replay tests failing because the canonical HarmonySharedState normalizer was applied outside its intended exact-production target. Build 0.0.97 keeps the same 11-instruction AOT-normalized production cctor and CecilOpCodes binding, while restoring byte-identical synthetic replay for internal randomized fixtures. Gate B still loads the byte-distinct normalized image for exact production 0Harmony 2.4.2, and Gate T runs/validates it before any PatchProcessor.Patch(). Force-quit/relaunch before every Step-27 retry once Gate B has started.";

    public const string ExpectedDisplayVersion = "0.0.97";
    public const string ExpectedBuildVersion = "97";
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
