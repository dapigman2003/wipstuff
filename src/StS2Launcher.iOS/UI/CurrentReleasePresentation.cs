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
        "STEP 29.0 — REAL STS2 COMPATIBILITY TARGET AUDIT";

    public const string MilestoneLine =
        "STEPS 01–26 CLOSED • STEP 27 CLOSED NEGATIVE • STEP 28 CLOSED POSITIVE 5/5 ON 0.0.111 • STEP 29 EXACT REAL-STS2 TARGET AUDIT";

    public const string Summary =
        "Physical 0.0.111 closed Step 28 positively: all A–E gates passed, only the verified transformed fixture entered the private CLR context, Adjustment()/Target(41)/InvokeTarget(41) returned 1000 / 1041 / 1041, source/transformed hashes remained stable, and post-execution OfflineReady passed 428/428. The ahead-of-load transform-before-load architecture is therefore physically established. Step 29.0 now returns to the exact receipt-backed macOS arm64 sts2.dll to regenerate the missing concrete target evidence before the first real semantic transformation. It is deliberately read-only: deferred Cecil metadata/IL inspection with a rejecting resolver, deterministic at-most-one audit-candidate selection, no Cecil write, no sts2 CLR load/invocation, no Harmony/MonoMod runtime patching, no Godot/game startup, and no native game loading.";

    public const string InitialStatus =
        "Status: Step 28 is CLOSED POSITIVE by physical 0.0.111 A–E 5/5, including 1000 / 1041 / 1041 transformed execution and OfflineReady 428/428 after execution. Build 0.0.112 is Step 29.0: a fresh-process, read-only exact-IL audit of receipt-backed ARM64 sts2.dll that selects at most one fingerprinted compatibility candidate for the next transformation iteration. Run Codemagic first; after compile/host/IPA verification, run Step 29 A–D on device and preserve Step29-RealStS2CompatibilityTargetAudit.txt.";

    public const string ExpectedDisplayVersion = "0.0.112";
    public const string ExpectedBuildVersion = "112";
    public const string Step28ImplementationMarker =
        "verified post-publish source -> private clone -> Cecil constant rewrite before CLR load -> reopen/hash verify -> transformed-only private AssemblyLoadContext execution";
    public const string Step29ImplementationMarker =
        "receipt-backed arm64 sts2.dll -> deferred rejecting-resolver Cecil audit -> exact token/IL/target/body fingerprint -> at-most-one audit candidate -> zero writes/zero CLR load -> OfflineReady reproof";

    // Historical Step-27 crash-report provenance markers remain available as regression/evidence tooling.
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
