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
        "STEP 35.0.4 — IN-METHOD PRE-FIRST-AWAIT LOCALIZATION";

    public const string MilestoneLine =
        "STEPS 01–26 CLOSED • STEP 27 CLOSED NEGATIVE • STEP 28 CLOSED POSITIVE 5/5 • STEP 29 CLOSED POSITIVE 4/4 • STEP 30 CLOSED POSITIVE 4/4 • STEP 31 CLOSED POSITIVE 4/4 • STEP 32 CLOSED POSITIVE 4/4 • STEP 33 CLOSED POSITIVE 4/4 • STEP 34 CLOSED POSITIVE 4/4 • STEP 35 OPEN";

    public const string Summary =
        "Physical 0.0.126 proved the run-correlation fix: journal, static map, current-run manifest and last-checkpoint all shared one Run ID/PID, and the durable frontier again stopped after planned System.Collections.Concurrent 8→9 resolution with no C_INVOKE_RETURNED. Step 35.0.4 / 0.0.127 now keeps the exact closed transformed source and resolver authority frozen while emitting a separate identity/MVID-preserving diagnostic clone with durable INMETHOD_* entry markers across ExecuteVeryEarly.MoveNext, the top-level pre-first-await game methods, and relevant type initializers.";

    public const string InitialStatus =
        "Status: Steps 32–34 are CLOSED POSITIVE at 4/4. Step 35 remains OPEN. Physical 0.0.126 reproduced the same synchronous Invoke hard-kill frontier with fully correlated telemetry. Candidate 0.0.127 is diagnostic-only: Gate A re-verifies the exact transformed source, creates and verifies an instrumented clone, Gate B admits only that clone, and Gate C arms a launcher callback immediately before the one ExecuteVeryEarly Invoke so the final durable INMETHOD_* entry identifies the failing pre-first-await method/type-initializer frontier. ExecuteEssential, ExecuteDeferred, game entry, Harmony, native loading and Godot/game startup remain forbidden.";

    public const string ExpectedDisplayVersion = "0.0.127";
    public const string ExpectedBuildVersion = "127";
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
        "physical Step34 exact PrewarmJit closure -> 0.0.124 exact Invoke frontier -> 0.0.125 static map -> 0.0.126 same-run durable correlation confirms final resolver marker System.Collections.Concurrent 8->9 and no C_INVOKE_RETURNED -> exact source ExecuteVeryEarly token 0x06007D02 + async MoveNext token 0x0600BC71 -> exact Step32 transformed source reverified and left untouched -> separate identity/MVID-preserving Step35.0.4 diagnostic clone -> output-only INMETHOD entry markers in ExecuteVeryEarly.MoveNext + top-level pre-first-await callees/type initializers -> launcher Action<string> callback armed immediately before one MethodInfo.Invoke -> strict prepared resolver/Task await <=60s/later boundaries unchanged -> isolation reproof";

    public static string DisplayVersion =>
        NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleShortVersionString")?.ToString() ?? "unknown";

    public static string DisplayBuild =>
        NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleVersion")?.ToString() ?? "unknown";

    public static bool BundleIdentityMatchesExpected =>
        string.Equals(DisplayVersion, ExpectedDisplayVersion, StringComparison.Ordinal) &&
        string.Equals(DisplayBuild, ExpectedBuildVersion, StringComparison.Ordinal);
}
