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
        "STEP 35.0.1 — VERY-EARLY B→C HARD-TERMINATION CRASH LOCALIZATION";

    public const string MilestoneLine =
        "STEPS 01–26 CLOSED • STEP 27 CLOSED NEGATIVE • STEP 28 CLOSED POSITIVE 5/5 • STEP 29 CLOSED POSITIVE 4/4 • STEP 30 CLOSED POSITIVE 4/4 • STEP 31 CLOSED POSITIVE 4/4 • STEP 32 CLOSED POSITIVE 4/4 • STEP 33 CLOSED POSITIVE 4/4 • STEP 34 CLOSED POSITIVE 4/4 • STEP 35 OPEN";

    public const string Summary =
        "Physical 0.0.122 CLOSED Step 34 positively at 4/4. Physical Step 35.0 / 0.0.123 then hard-terminated around the visible Gate-B region; the matching iOS .ips reports EXC_BAD_ACCESS/SIGKILL with faulting main-thread PC=0x0, while no managed Step-35 report survived. Because the display can remain on Gate B while synchronous Gate-C reflection/invocation has already begun, Step 35.0.1 / 0.0.124 preserves the exact 0.0.123 execution policy and adds synchronously flushed, provenance/thread-aware checkpoints across Gate B, the B→C transition, ExecuteVeryEarly binding/invocation/Task await, and resolver callbacks. No broader startup authority is added.";

    public const string InitialStatus =
        "Status: Steps 32–34 are CLOSED POSITIVE at 4/4. Step 35 remains OPEN after physical 0.0.123 hard-terminated with main-thread PC=0x0 around the B→C frontier. Candidate 0.0.124 is diagnostic-only: exact source/transformed target, strict resolver, one ExecuteVeryEarly invocation, and <=60s Task await remain unchanged; durable Step35-CrashCheckpoint.txt telemetry is added. Cancellation is INCONCLUSIVE, not a compatibility FAIL. ExecuteEssential, ExecuteDeferred, the receipt-backed original, initializer-bearing 0Harmony 2.4.2.0, unplanned managed/native loading, the game entry point, Harmony patching and Godot/game startup remain forbidden.";

    public const string ExpectedDisplayVersion = "0.0.124";
    public const string ExpectedBuildVersion = "124";
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
        "physical Step34 exact PrewarmJit closure -> physical 0.0.123 main-thread PC=0x0 hard termination near visible B/C frontier -> unchanged exact source ExecuteVeryEarly token 0x06007D02 + async MoveNext token 0x0600BC71 -> unchanged semantic audit/strict transformed-primary ALC/one MethodInfo.Invoke/Task await <=60s/exact resolver -> synchronous provenance+thread crash checkpoints around Gate B, B-to-C transition, binding, invoke, await and resolver callbacks -> isolation reproof";

    public static string DisplayVersion =>
        NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleShortVersionString")?.ToString() ?? "unknown";

    public static string DisplayBuild =>
        NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleVersion")?.ToString() ?? "unknown";

    public static bool BundleIdentityMatchesExpected =>
        string.Equals(DisplayVersion, ExpectedDisplayVersion, StringComparison.Ordinal) &&
        string.Equals(DisplayBuild, ExpectedBuildVersion, StringComparison.Ordinal);
}
