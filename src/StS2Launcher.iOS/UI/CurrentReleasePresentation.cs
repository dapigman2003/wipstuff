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
        "STEP 28.0.1 — AHEAD-OF-LOAD MANAGED TRANSFORMATION COMPILE FIX";

    public const string MilestoneLine =
        "STEPS 01–26 PHYSICALLY CLOSED • STEP 27 CLOSED NEGATIVE ON 0.0.108 • 0.0.109 COMPILE STOP CORRECTED • STEP 28 MECHANISM UNCHANGED";

    public const string Summary =
        "Physical 0.0.108 completed the Step-27 stop-rule experiment: a launcher-owned post-publish interpreted target still reached the exact public PatchProcessor.Patch() boundary and failed with System.NotImplementedException from PatchFunctions.UpdateWrapper, so runtime Harmony/MonoMod replacement remains retired. Step 28.0 keeps the replacement pipeline unchanged: admit a separately built post-publish source fixture as Cecil metadata only, clone it to launcher-private storage, rewrite Adjustment() from 1 to 1000 before CLR admission, reopen and hash-verify both images, then load only the transformed bytes and require Adjustment()==1000, Target(41)==1041, and the in-fixture direct-call InvokeTarget(41)==1041. Codemagic 0.0.109 passed static validation and built the external fixtures but StS2Launcher.Core compilation stopped because this production boundary referenced a missing CallbackProgress<T> adapter. Step 28.0.1 adds only that established callback-backed IProgress<T> adapter and a static regression guard. No Harmony patch API, real StS2 member reflection/invocation, Godot/game startup, native game loading, trusted-install mutation, gate semantics, or resolver policy changes.";

    public const string InitialStatus =
        "Status: Steps 01–26 are physically closed and Step 27 is closed negative by physical 0.0.108. Codemagic 0.0.109 was compile-only evidence: static validation passed, all external fixtures built, then Core failed CS0246 on missing CallbackProgress<T> before host tests. Build 0.0.110 is the Step 28.0.1 compile-fix candidate; Codemagic compile/full host tests/iOS publish/IPA verification are the next authority before the unchanged physical A–E run.";

    public const string ExpectedDisplayVersion = "0.0.110";
    public const string ExpectedBuildVersion = "110";
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
