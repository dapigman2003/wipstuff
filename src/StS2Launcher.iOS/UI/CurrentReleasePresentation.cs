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
        "STEP 34.0 — CONTROLLED TRANSFORMED REAL-STS2 PREWARMJIT EXECUTION";

    public const string MilestoneLine =
        "STEPS 01–26 CLOSED • STEP 27 CLOSED NEGATIVE • STEP 28 CLOSED POSITIVE 5/5 • STEP 29 CLOSED POSITIVE 4/4 • STEP 30 CLOSED POSITIVE 4/4 • STEP 31 CLOSED POSITIVE 4/4 • STEP 32 CLOSED POSITIVE 4/4 • STEP 33 CLOSED POSITIVE 4/4 • STEP 34 OPEN";

    public const string Summary =
        "Physical 0.0.120 CLOSED Step 32 positively at 4/4 and fixed the exact transformed real-StS2 artifact: SHA-256 39c0a89ad0d5c6eb1553e23dd8537a7b7ab8278fad4115d186db5751570211ef, MVID 518e4758-52d7-47c2-b776-471a0e29e49d, transformed PrewarmJit token 0x0600AFEA, semantic fingerprint 47fadf2a46eda098f310b7d0ee54e37d1e952ac272fc966d16d557ed46a0b74a, and zero PrepareMethod references. Physical 0.0.121 then CLOSED Step 33 positively at 4/4: only those exact transformed primary bytes entered StS2Launcher-Step33-TransformedGame, with zero resolver requests, zero private dependency/native admission and no game-member invocation. Step 34.0 is the first controlled execution boundary: re-establish the exact transformed-primary state in a strict execution-capable private AssemblyLoadContext, bind only OneTimeInitialization::PrewarmJit(), invoke it exactly once, and preserve fail-closed resolver/native isolation.";

    public const string InitialStatus =
        "Status: Steps 32 and 33 are CLOSED POSITIVE at 4/4. Step 34 is OPEN. Candidate 0.0.122 re-manufactures/reverifies the exact closed transformed image, requalifies the zero-blocker prepared runtime plan, re-establishes the Step-33 zero-resolution transformed-primary CLR admission state, and then reflects/invokes only exact transformed OneTimeInitialization::PrewarmJit() once. Exact persisted host-framework bindings and hash-pinned initializer-free prepared dependencies may resolve on demand. Initializer-bearing 0Harmony 2.4.2.0, unplanned managed requests, native loading, the game entry point, Harmony patching and Godot/game startup remain forbidden.";

    public const string ExpectedDisplayVersion = "0.0.122";
    public const string ExpectedBuildVersion = "122";
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
        "physical Step33 transformed-primary-only admission -> fresh exact transformed requalification -> strict execution-capable private ALC -> exact transformed PrewarmJit type/signature/token binding -> one MethodInfo.Invoke -> only exact host bindings + hash-pinned initializer-free prepared dependencies -> zero initializer-bearing/native/unplanned escape -> OfflineReady/source/transformed/plan isolation reproof";

    public static string DisplayVersion =>
        NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleShortVersionString")?.ToString() ?? "unknown";

    public static string DisplayBuild =>
        NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleVersion")?.ToString() ?? "unknown";

    public static bool BundleIdentityMatchesExpected =>
        string.Equals(DisplayVersion, ExpectedDisplayVersion, StringComparison.Ordinal) &&
        string.Equals(DisplayBuild, ExpectedBuildVersion, StringComparison.Ordinal);
}
