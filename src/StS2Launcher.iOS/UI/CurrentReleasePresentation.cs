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
        "STEP 35.0.5 — IN-METHOD PRE-FIRST-AWAIT LOCALIZATION";

    public const string MilestoneLine =
        "STEPS 01–26 CLOSED • STEP 27 CLOSED NEGATIVE • STEP 28 CLOSED POSITIVE 5/5 • STEP 29 CLOSED POSITIVE 4/4 • STEP 30 CLOSED POSITIVE 4/4 • STEP 31 CLOSED POSITIVE 4/4 • STEP 32 CLOSED POSITIVE 4/4 • STEP 33 CLOSED POSITIVE 4/4 • STEP 34 CLOSED POSITIVE 4/4 • STEP 35 OPEN";

    public const string Summary =
        "Physical 0.0.126 proved the same-run synchronous ExecuteVeryEarly Invoke frontier. Physical 0.0.127 then failed closed in Gate A before CLR admission: Cecil diagnostic-clone serialization requested System.Runtime 9.0.0.0 constant metadata, reproducing the writer-only resolution trap already solved in Step 32. Step 35.0.5 / 0.0.128 reuses that exact audited in-memory constant-metadata surrogate resolver for clone serialization only, then reopens/verifies the clone with rejecting resolution before any Gate-B CLR admission.";

    public const string InitialStatus =
        "Status: Steps 32–34 are CLOSED POSITIVE at 4/4. Step 35 remains OPEN. Physical 0.0.127 did not reach the game boundary: Gate A failed normally while Cecil wrote the diagnostic clone because the rejecting resolver refused System.Runtime 9.0.0.0 metadata resolution. Candidate 0.0.128 changes only that writer path, using the already-audited Step-32 synthetic constant-metadata surrogates; exact transformed-source hashing, diagnostic-only authority, strict runtime resolution, INMETHOD localization, later-boundary prohibitions and physical acceptance rules are unchanged.";

    public const string ExpectedDisplayVersion = "0.0.128";
    public const string ExpectedBuildVersion = "128";
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
    public const string Step34ImplementationMarker =
        "physical Step33 transformed-primary-only admission -> fresh exact transformed requalification -> strict execution-capable private ALC -> exact transformed PrewarmJit type/signature/token 0x0600AFEA binding -> one MethodInfo.Invoke -> only exact host bindings + hash-pinned initializer-free prepared dependencies -> zero initializer-bearing/native/unplanned escape -> OfflineReady/source/transformed/plan isolation reproof";
    public const string Step35ImplementationMarker =
        "physical Step34 exact PrewarmJit closure -> 0.0.124 exact Invoke frontier -> 0.0.125 static map -> 0.0.126 same-run durable correlation confirms final resolver marker System.Collections.Concurrent 8->9 and no C_INVOKE_RETURNED -> 0.0.127 Gate-A diagnostic-clone Cecil write failed closed on System.Runtime 9.0.0.0 metadata resolution before CLR admission -> Step32 physically proven audited System.Runtime+Sentry in-memory constant-metadata surrogate resolver reused for diagnostic-clone serialization only -> exact source ExecuteVeryEarly token 0x06007D02 + async MoveNext token 0x0600BC71 -> exact Step32 transformed source reverified and left untouched -> separate identity/MVID-preserving Step35.0.5 diagnostic clone -> post-write constant-metadata fingerprint + rejecting-resolver reopen verification -> output-only INMETHOD entry markers -> launcher Action<string> callback armed immediately before one MethodInfo.Invoke -> strict runtime resolver/Task await <=60s/later boundaries unchanged -> isolation reproof";

    public static string DisplayVersion =>
        NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleShortVersionString")?.ToString() ?? "unknown";

    public static string DisplayBuild =>
        NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleVersion")?.ToString() ?? "unknown";

    public static bool BundleIdentityMatchesExpected =>
        string.Equals(DisplayVersion, ExpectedDisplayVersion, StringComparison.Ordinal) &&
        string.Equals(DisplayBuild, ExpectedBuildVersion, StringComparison.Ordinal);
}
