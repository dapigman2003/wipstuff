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
        "STEP 32.0.3 — RETIRED HARMONY ACTIVE-SURFACE TRIM";

    public const string MilestoneLine =
        "STEPS 01–26 CLOSED • STEP 27 CLOSED NEGATIVE • STEP 28 CLOSED POSITIVE 5/5 • STEP 29 CLOSED POSITIVE 4/4 • STEP 30 CLOSED POSITIVE 4/4 • STEP 31 CLOSED POSITIVE 4/4 • STEP 32 FIRST REAL STS2 REWRITE";

    public const string Summary =
        "Physical 0.0.114 closed Step 31 positively at 4/4 and confirmed MegaCrit.Sts2.Core.Helpers.OneTimeInitialization::PrewarmJit(), token 0x06007D05, body SHA-256 7f25b7bd955c407fc69306cf26af2162223353f5606560458066aed085e72ab9, with ten exact RuntimeHelpers.PrepareMethod sites as eligible for explicit rewrite design. Step 32.0 is the first real-StS2 semantic transformation: Gate A re-proves OfflineReady and clones the exact receipt-backed sts2.dll into launcher-private storage; Gate B suppresses only those ten PrepareMethod calls with stack-neutral Pop replacements; Gate C reopens source/transformed images and verifies the exact planned semantic fingerprint with zero PrepareMethod references; Gate D re-proves source hashes, OfflineReady, and no-CLR-load isolation. The trusted Step-12 install remains immutable and the transformed image is not CLR-loaded in this build.";

    public const string InitialStatus =
        "Status: Physical 0.0.117 re-proved Step-32 Gate A, then Gate B failed closed before mutation because the verified real sts2.dll also contains external constant metadata scoped to exact Sentry 5.0.0.0. Build 0.0.118 is maintenance-only: the Step-32 6+4 rewrite and bounded writer behavior are intentionally unchanged while the retired Step-25/26/27 runtime-Harmony implementation, tests, UI, CI download, and interpreted fixture move out of the active build surface into inert history. Use Codemagic to verify the lean active surface and compare build time; do not interpret this maintenance build as a Sentry correction.";

    public const string ExpectedDisplayVersion = "0.0.118";
    public const string ExpectedBuildVersion = "118";
    public const string Step28ImplementationMarker =
        "verified post-publish source -> private clone -> Cecil constant rewrite before CLR load -> reopen/hash verify -> transformed-only private AssemblyLoadContext execution";
    public const string Step29ImplementationMarker =
        "receipt-backed arm64 sts2.dll -> deferred rejecting-resolver Cecil audit -> exact token/IL/target/body fingerprint -> at-most-one audit candidate -> zero writes/zero CLR load -> OfflineReady reproof";
    public const string Step30ImplementationMarker =
        "physical Step29 exact source+token+IL+target+body fingerprint -> deferred rejecting-resolver semantic context audit -> mod-path disposition -> zero writes/zero CLR load -> OfflineReady reproof";
    public const string Step31ImplementationMarker =
        "physical Step29 PrewarmJit token+body fingerprint+10 PrepareMethod offsets -> deferred rejecting-resolver per-site semantic context audit -> rewrite-design eligibility only -> zero writes/zero CLR load -> OfflineReady reproof";
    public const string Step32ImplementationMarker =
        "physical Step31 exact PrewarmJit evidence -> private sts2.dll clone -> 6 one-arg PrepareMethod calls to Pop + 4 two-arg calls to Pop+Pop -> bounded in-memory System.Runtime constant-metadata surrogate for Cecil write only -> reopen semantic + constant-metadata verification -> zero CLR load -> OfflineReady reproof";


    public static string DisplayVersion =>
        NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleShortVersionString")?.ToString() ?? "unknown";

    public static string DisplayBuild =>
        NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleVersion")?.ToString() ?? "unknown";

    public static bool BundleIdentityMatchesExpected =>
        string.Equals(DisplayVersion, ExpectedDisplayVersion, StringComparison.Ordinal) &&
        string.Equals(DisplayBuild, ExpectedBuildVersion, StringComparison.Ordinal);
}
