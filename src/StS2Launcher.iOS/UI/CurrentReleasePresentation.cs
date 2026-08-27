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
        "STEP 32.0.5 — STABLE TRANSFORMED METHOD VERIFICATION";

    public const string MilestoneLine =
        "STEPS 01–26 CLOSED • STEP 27 CLOSED NEGATIVE • STEP 28 CLOSED POSITIVE 5/5 • STEP 29 CLOSED POSITIVE 4/4 • STEP 30 CLOSED POSITIVE 4/4 • STEP 31 CLOSED POSITIVE 4/4 • STEP 32 PHYSICAL FRONTIER 2/4";

    public const string Summary =
        "Physical 0.0.119 advanced Step 32 to 2/4: Gate A re-proved the exact receipt-backed sts2.dll and ten PrepareMethod sites, and Gate B successfully wrote the first launcher-private real-StS2 semantic transformation with the exact audited System.Runtime/Sentry metadata resolver. Gate C then stopped before semantic verification because it reused the physical Step-31 source MethodDef token as a post-Cecil-write locator. Build 0.0.120 keeps source token 0x06007D05 as an exact Gate-A/Gate-B admission locator, but Gate C reopens the serialized image by the exact declaring type plus full method signature and then proves the already-predeclared offset-independent semantic fingerprint, zero PrepareMethod references, unchanged constant metadata, instruction/EH shape, and Pop delta. The transformed token is reported diagnostically instead of being treated as semantic identity.";

    public const string InitialStatus =
        "Status: Step 32 remains OPEN at physical 2/4. 0.0.119 proved the bounded 6+4 rewrite can serialize the exact real sts2.dll with 9 write-time resolver requests confined to the three audited requirements across exact System.Runtime 9.0.0.0 and Sentry 5.0.0.0; no external dependency bytes were opened and the trusted install was not mutated. Gate C failed at the old token-based transformed-method locator before the reopened semantic fingerprint or constant-metadata checks ran. 0.0.120 changes only that post-write verification locator and adds token-drift diagnostics; rewrite semantics, resolver authority, no-CLR-load boundary, and Gate-D isolation requirements are unchanged.";

    public const string ExpectedDisplayVersion = "0.0.120";
    public const string ExpectedBuildVersion = "120";
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



    public static string DisplayVersion =>
        NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleShortVersionString")?.ToString() ?? "unknown";

    public static string DisplayBuild =>
        NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleVersion")?.ToString() ?? "unknown";

    public static bool BundleIdentityMatchesExpected =>
        string.Equals(DisplayVersion, ExpectedDisplayVersion, StringComparison.Ordinal) &&
        string.Equals(DisplayBuild, ExpectedBuildVersion, StringComparison.Ordinal);
}
