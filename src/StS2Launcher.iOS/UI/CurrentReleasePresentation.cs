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
        "STEP 27.0.21 — RAW HARMONYSHAREDSTATE METHOD-BODY NORMALIZATION";

    public const string MilestoneLine =
        "STEPS 01–26 PHYSICALLY CLOSED • 0.0.104 RAN 212 HOST TESTS • 211 PASS / 1 REAL CECIL-WRITER FAILURE • WHOLE-MODULE REWRITE REMOVED";

    public const string Summary =
        "Codemagic 0.0.104 compiled production and tests and executed all 212 host tests. The hash-pinned official Harmony-Fat 2.4.2 net9.0 surrogate finally entered the production normalizer, where the Deferred Cecil read succeeded but Mono.Cecil.ModuleDefinition.Write failed while rebuilding enum-typed Constant metadata and attempted forbidden resolution of System.Reflection.BindingFlags. Step 27.0.21 removes Cecil's whole-module writer instead of whitelisting framework enums: Gate A keeps Deferred Cecil read-only for exact admission and existing-token discovery, clones the prepared bytes, and replaces only the existing HarmonySharedState::.cctor PE method-body slot with the same exact 11-instruction direct-state body using metadata tokens already present in the source image. No byte outside that original method-body slot may change. No StS2 member is reflected, patched, or invoked.";

    public const string InitialStatus =
        "Status: Steps 01–26 are physically closed. Codemagic 0.0.104 ran 212 tests at 211/212; the only failure was the first genuine production-normalizer failure, inside Cecil's metadata writer while resolving BindingFlags for an unrelated enum constant. Build 0.0.105 removes whole-module serialization and performs a bounded in-place PE method-body substitution. If CI passes, the next meaningful evidence is on-device T6, then the single public PatchProcessor.Patch() boundary at T7/T8.";

    public const string ExpectedDisplayVersion = "0.0.105";
    public const string ExpectedBuildVersion = "105";
    public const string GateSImplementationMarker =
        "bounded HarmonyMethod() descriptor; PatchProcessor.AddPrefix(MethodInfo) runtime invocation forbidden";
    public const string GateTImplementationMarker =
        "Gate-A raw PE method-body normalized HarmonySharedState cctor; Deferred Cecil read/audit only; no Cecil whole-module write; T5b RunClassConstructor uses direct state only; PatchProcessor.Patch() remains after T6";

    public static string DisplayVersion =>
        NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleShortVersionString")?.ToString() ?? "unknown";

    public static string DisplayBuild =>
        NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleVersion")?.ToString() ?? "unknown";

    public static bool BundleIdentityMatchesExpected =>
        string.Equals(DisplayVersion, ExpectedDisplayVersion, StringComparison.Ordinal) &&
        string.Equals(DisplayBuild, ExpectedBuildVersion, StringComparison.Ordinal);
}
