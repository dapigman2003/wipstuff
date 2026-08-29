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
        "STEP 35.0.2 — EXECUTEVERYEARLY INVOKE-CRASH STATIC ILCALLSITE LOCALIZATION";

    public const string MilestoneLine =
        "STEPS 01–26 CLOSED • STEP 27 CLOSED NEGATIVE • STEP 28 CLOSED POSITIVE 5/5 • STEP 29 CLOSED POSITIVE 4/4 • STEP 30 CLOSED POSITIVE 4/4 • STEP 31 CLOSED POSITIVE 4/4 • STEP 32 CLOSED POSITIVE 4/4 • STEP 33 CLOSED POSITIVE 4/4 • STEP 34 CLOSED POSITIVE 4/4 • STEP 35 OPEN";

    public const string Summary =
        "Physical 0.0.124 localized the Step-35 hard termination beyond ambiguity: Gate B passed completely; Gate C bound exact transformed ExecuteVeryEarly, entered the first/only MethodInfo.Invoke, loaded GodotSharp and Steamworks.NET plus exact host frameworks, then died before C_INVOKE_RETURNED with the same main-thread PC=0x0 native signature and essentially the same runtime stack shape as 0.0.123. Step 35.0.2 / 0.0.125 preserves the exact execution/resolver authority and existing durable checkpoints, and adds a pre-CLR output-only static IL/callsite map of the verified transformed ExecuteVeryEarly wrapper + async MoveNext so the invoke-time crash can be correlated to exact callsites before designing a smaller execution discriminator.";

    public const string InitialStatus =
        "Status: Steps 32–34 are CLOSED POSITIVE at 4/4. Step 35 remains OPEN after physical 0.0.124 proved Gate B PASS and localized the hard kill inside the synchronous execution initiated by exact ExecuteVeryEarly MethodInfo.Invoke, after planned GodotSharp/Steamworks.NET/framework resolutions but before Invoke returned. Candidate 0.0.125 is diagnostic-only: execution authority remains unchanged; Step35-ExecuteVeryEarly-StaticMap.txt is generated before CLR admission alongside the existing Step35-CrashCheckpoint.txt. Cancellation remains INCONCLUSIVE. ExecuteEssential, ExecuteDeferred, the receipt-backed original, initializer-bearing 0Harmony 2.4.2.0, unplanned managed/native loading, the game entry point, Harmony patching and Godot/game startup remain forbidden.";

    public const string ExpectedDisplayVersion = "0.0.125";
    public const string ExpectedBuildVersion = "125";
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
        "physical Step34 exact PrewarmJit closure -> 0.0.123/0.0.124 repeated main-thread PC=0x0 hard termination -> 0.0.124 proves Gate B PASS and exact ExecuteVeryEarly MethodInfo.Invoke entered, GodotSharp+Steamworks.NET+framework resolutions serviced, no C_INVOKE_RETURNED -> unchanged exact source ExecuteVeryEarly token 0x06007D02 + async MoveNext token 0x0600BC71 -> verified transformed wrapper/MoveNext static IL+callsite map emitted before CLR admission -> unchanged strict transformed-primary ALC/one MethodInfo.Invoke/Task await <=60s/exact resolver -> durable invoke/resolver checkpoints -> isolation reproof";

    public static string DisplayVersion =>
        NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleShortVersionString")?.ToString() ?? "unknown";

    public static string DisplayBuild =>
        NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleVersion")?.ToString() ?? "unknown";

    public static bool BundleIdentityMatchesExpected =>
        string.Equals(DisplayVersion, ExpectedDisplayVersion, StringComparison.Ordinal) &&
        string.Equals(DisplayBuild, ExpectedBuildVersion, StringComparison.Ordinal);
}
