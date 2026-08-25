using Foundation;

namespace StS2Launcher.iOS;

/// <summary>
/// Single source for the release identity shown at the top of the launcher UI.
/// The display version is read from the built bundle so it cannot drift from Info.plist;
/// the candidate step/summary are statically validated for every release.
/// </summary>
internal static class CurrentReleasePresentation
{
    public const string StepTitle =
        "STEP 28.0.2 — DEFERRED CECIL METADATA ADMISSION FIX";

    public const string MilestoneLine =
        "STEPS 01–26 PHYSICALLY CLOSED • STEP 27 CLOSED NEGATIVE ON 0.0.108 • 0.0.110 COMPILES; HOST 216/217 • STEP 28 GATE-A CECIL READ FIX";

    public const string Summary =
        "Physical 0.0.108 closed runtime Harmony/MonoMod replacement negatively, so Step 28 remains deterministic ahead-of-load Cecil transformation followed by transformed-only interpreted execution. Codemagic 0.0.110 proved the 0.0.109 CallbackProgress<T> compile defect is fixed: Core/tests compiled and 216/217 host tests passed. The sole failure was the Step-28 end-to-end host regression at Gate A, where Mono.Cecil ReadingMode.Immediate eagerly decoded unrelated custom-attribute arguments and asked the deliberately rejecting metadata resolver for System.Runtime, Version=9.0.0.0 before any rewrite or CLR load. Step 28.0.2 changes only fixture metadata reads to ReadingMode.Deferred, matching the established metadata-only audit pattern while retaining the rejecting resolver. No Harmony patch API, real StS2 member reflection/invocation, Godot/game startup, native game loading, trusted-install mutation, gate semantics, or runtime resolver policy changes.";

    public const string InitialStatus =
        "Status: Steps 01–26 are physically closed and Step 27 is closed negative by physical 0.0.108. Codemagic 0.0.110 passed static validation, compiled the project, and executed all 217 host tests; 216 passed and the only failure was Gate-A Cecil eager metadata resolution of System.Runtime before transformation. Build 0.0.111 is the Step 28.0.2 deferred-metadata-read correction; Codemagic full host tests/iOS publish/IPA verification are the next authority before the unchanged physical A–E run.";

    public const string ExpectedDisplayVersion = "0.0.111";
    public const string ExpectedBuildVersion = "111";
    public const string Step28ImplementationMarker =
        "verified post-publish source -> private clone -> Cecil constant rewrite before CLR load -> reopen/hash verify -> transformed-only private AssemblyLoadContext execution";

    // Historical Step-27 crash-report provenance markers are retained because the closed Step-27
    // UI/report path remains available as evidence/regression tooling, but it is not the active architecture.
    public const string GateSImplementationMarker =
        "bounded HarmonyMethod() descriptor; PatchProcessor.AddPrefix(MethodInfo) runtime invocation forbidden";
    public const string GateTImplementationMarker =
        "Gate-A raw PE method-body normalized HarmonySharedState cctor; post-publish interpreted Target+Prefix fixture; fresh processor via Harmony.CreateProcessor(MethodBase); exactly one Patch() and conditional exact Unpatch() decision boundary";

    public static string DisplayVersion =>
        NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleShortVersionString")?.ToString() ?? "unknown";

    public static string DisplayBuild =>
        NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleVersion")?.ToString() ?? "unknown";

    public static bool BundleIdentityMatchesExpected =>
        string.Equals(DisplayVersion, ExpectedDisplayVersion, StringComparison.Ordinal) &&
        string.Equals(DisplayBuild, ExpectedBuildVersion, StringComparison.Ordinal);
}
