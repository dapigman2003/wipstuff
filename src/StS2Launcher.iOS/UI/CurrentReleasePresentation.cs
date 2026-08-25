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
        "STEP 31.0 — PREPAREMETHOD SEMANTIC CONTEXT AUDIT";

    public const string MilestoneLine =
        "STEPS 01–26 CLOSED • STEP 27 CLOSED NEGATIVE • STEP 28 CLOSED POSITIVE 5/5 • STEP 29 CLOSED POSITIVE 4/4 • STEP 30 CLOSED POSITIVE 4/4 • STEP 31 PREPAREMETHOD SEMANTIC AUDIT";

    public const string Summary =
        "Physical 0.0.113 closed Step 30 positively at 4/4 and formally deferred the selected ModManager.TryLoadMod(Mod) -> Harmony.PatchAll(Assembly) site from the base-game frontier. Step 31.0 follows the first recorded non-mod Step-29 family: MegaCrit.Sts2.Core.Helpers.OneTimeInitialization::PrewarmJit(), token 0x06007D05, body SHA-256 7f25b7bd955c407fc69306cf26af2162223353f5606560458066aed085e72ab9, with ten exact RuntimeHelpers.PrepareMethod calls. Gate A binds that exact physical evidence to the receipt-backed ARM64 sts2.dll; Gate B records per-site IL/control-flow/exception context; Gate C may mark the family eligible for a separately predeclared rewrite design but authorizes no rewrite; Gate D re-proves source hashes, OfflineReady, and no-CLR-load isolation. The boundary remains read-only with deferred Cecil inspection and a rejecting resolver.";

    public const string InitialStatus =
        "Status: Step 30 is CLOSED POSITIVE by physical 0.0.113 A–D 4/4. Build 0.0.114 is Step 31.0: inspect the exact fingerprinted PrewarmJit/PrepareMethod family before any real-game semantic write. Run Codemagic first; after compile/host/IPA verification, force-quit/relaunch, run Step 31 A–D on device, and preserve Step31-PrepareMethodSemanticContextAudit.txt.";

    public const string ExpectedDisplayVersion = "0.0.114";
    public const string ExpectedBuildVersion = "114";
    public const string Step28ImplementationMarker =
        "verified post-publish source -> private clone -> Cecil constant rewrite before CLR load -> reopen/hash verify -> transformed-only private AssemblyLoadContext execution";
    public const string Step29ImplementationMarker =
        "receipt-backed arm64 sts2.dll -> deferred rejecting-resolver Cecil audit -> exact token/IL/target/body fingerprint -> at-most-one audit candidate -> zero writes/zero CLR load -> OfflineReady reproof";
    public const string Step30ImplementationMarker =
        "physical Step29 exact source+token+IL+target+body fingerprint -> deferred rejecting-resolver semantic context audit -> mod-path disposition -> zero writes/zero CLR load -> OfflineReady reproof";
    public const string Step31ImplementationMarker =
        "physical Step29 PrewarmJit token+body fingerprint+10 PrepareMethod offsets -> deferred rejecting-resolver per-site semantic context audit -> rewrite-design eligibility only -> zero writes/zero CLR load -> OfflineReady reproof";

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
