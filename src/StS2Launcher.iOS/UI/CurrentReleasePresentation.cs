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
        "STEP 33.0 — VERIFIED TRANSFORMED REAL-STS2 CLR ADMISSION";

    public const string MilestoneLine =
        "STEPS 01–26 CLOSED • STEP 27 CLOSED NEGATIVE • STEP 28 CLOSED POSITIVE 5/5 • STEP 29 CLOSED POSITIVE 4/4 • STEP 30 CLOSED POSITIVE 4/4 • STEP 31 CLOSED POSITIVE 4/4 • STEP 32 CLOSED POSITIVE 4/4 • STEP 33 OPEN";

    public const string Summary =
        "Physical 0.0.120 CLOSED Step 32 positively at 4/4. The exact receipt-backed sts2.dll was cloned privately; only the ten audited RuntimeHelpers.PrepareMethod calls in OneTimeInitialization::PrewarmJit() were replaced stack-neutrally (6 × Pop, 4 × Pop+Pop); the result serialized through the exact audited System.Runtime/Sentry metadata boundary; reopen verification proved zero PrepareMethod references, unchanged constant metadata, the exact transformed semantic fingerprint, and final source isolation. The closed transformed image is SHA-256 39c0a89ad0d5c6eb1553e23dd8537a7b7ab8278fad4115d186db5751570211ef. Step 33.0 now re-manufactures/reverifies that exact image and tests only its CLR admission into a dedicated private AssemblyLoadContext. No game member invocation, private dependency admission, native loading, or Godot/game startup is authorized.";

    public const string InitialStatus =
        "Status: Step 32 CLOSED POSITIVE — physical 0.0.120 passed 4/4. Step 33 is OPEN. Candidate 0.0.121 re-runs the closed Step-32 transform contract, requires the exact closed transformed hash/identity/semantic fingerprint, requalifies the existing zero-blocker runtime plan, and then LoadFromStream-admits only the transformed primary into a dedicated private CLR context. The receipt-backed/prepared original sts2.dll must never be the CLR load input; private dependency requests and native requests fail closed. Execution remains a later boundary.";

    public const string ExpectedDisplayVersion = "0.0.121";
    public const string ExpectedBuildVersion = "121";
    public const string Step28ImplementationMarker =
        "verified post-publish source -> private clone -> Cecil constant rewrite before CLR load -> reopen/hash verify -> transformed-only private AssemblyLoadContext execution";
    public const string Step29ImplementationMarker =
        "receipt-backed arm64 sts2.dll -> deferred rejecting-resolver Cecil audit -> exact token/IL/target/body fingerprint -> at-most-one audit candidate -> zero writes/zero CLR load -> OfflineReady reproof";
    public const string Step30ImplementationMarker =
        "physical Step29 exact source+token+IL+target+body fingerprint -> deferred rejecting-resolver semantic context audit -> mod-path disposition -> zero writes/zero CLR load -> OfflineReady reproof";
    public const string Step31ImplementationMarker =
        "physical Step29 PrewarmJit token+body fingerprint+10 PrepareMethod offsets -> deferred rejecting-resolver per-site semantic context audit -> rewrite-design eligibility only -> zero writes/zero CLR load -> OfflineReady reproof";
    public const string Step32ImplementationMarker =
        "physical Step31 exact source token/body/10-site evidence -> private sts2.dll clone -> 6 one-arg PrepareMethod calls to Pop + 4 two-arg calls to Pop+Pop -> exact audited System.Runtime+Sentry in-memory constant-metadata surrogates for Cecil write only -> transformed reopen by stable exact type+signature -> semantic + constant-metadata verification -> zero CLR load -> OfflineReady reproof";
    public const string Step33ImplementationMarker =
        "physical Step32 exact transformed hash+semantic fingerprint -> fresh Step32 requalification -> zero-blocker prepared-plan requalification -> exact transformed bytes LoadFromStream into dedicated private ALC -> transformed-primary-only context audit -> original/source isolation reproof -> zero game-member invocation/native load";

    public static string DisplayVersion =>
        NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleShortVersionString")?.ToString() ?? "unknown";

    public static string DisplayBuild =>
        NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleVersion")?.ToString() ?? "unknown";

    public static bool BundleIdentityMatchesExpected =>
        string.Equals(DisplayVersion, ExpectedDisplayVersion, StringComparison.Ordinal) &&
        string.Equals(DisplayBuild, ExpectedBuildVersion, StringComparison.Ordinal);
}
