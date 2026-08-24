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
        "STEP 28.0 — AHEAD-OF-LOAD MANAGED TRANSFORMATION ARCHITECTURE PIVOT";

    public const string MilestoneLine =
        "STEPS 01–26 PHYSICALLY CLOSED • STEP 27 CLOSED NEGATIVE ON 0.0.108 • RUNTIME HARMONY DETOURS RETIRED • CECIL BEFORE LOAD";

    public const string Summary =
        "Physical 0.0.108 completed the Step-27 stop-rule experiment: the target was a launcher-owned DLL copied only after dotnet publish, Gate Q proved both reflection and an in-fixture direct managed IL call executed that post-publish interpreted target, and the fresh PatchProcessor still threw System.NotImplementedException from PatchFunctions.UpdateWrapper when Patch() was invoked. That removes the remaining AOT-target ambiguity and closes runtime Harmony/MonoMod replacement as the active architecture. Step 28.0 therefore proves the replacement pipeline without touching real StS2 behavior: admit a separately built post-publish source fixture as Cecil metadata only, clone it to launcher-private storage, rewrite Adjustment() from 1 to 1000 before CLR admission, reopen and hash-verify both images, then load only the transformed bytes and require Target(41) plus the in-fixture direct-call InvokeTarget(41) to both return 1041. No Harmony patch API, real StS2 member reflection/invocation, Godot/game startup, native game loading, or trusted-install mutation is part of this candidate.";

    public const string InitialStatus =
        "Status: Steps 01–26 are physically closed. Step 27 is now closed negative by physical 0.0.108: a genuine post-publish interpreted target still failed at the exact public PatchProcessor.Patch() boundary with NotImplementedException from PatchFunctions.UpdateWrapper. Build 0.0.109 is the first Step-28 architecture-pivot candidate and uses deterministic ahead-of-load Cecil transformation only; the original source fixture never enters the CLR.";

    public const string ExpectedDisplayVersion = "0.0.109";
    public const string ExpectedBuildVersion = "109";
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
