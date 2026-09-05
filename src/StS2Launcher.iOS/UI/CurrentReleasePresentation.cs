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
        "STEP 35.0.31 / STEP 36.0.2 — EXACT EXECUTEESSENTIAL FAILURE-CHAIN CAPTURE";

    public const string MilestoneLine =
        "STEPS 01–26 CLOSED • STEP 27 CLOSED NEGATIVE • STEP 28 CLOSED POSITIVE 5/5 • STEP 29 CLOSED POSITIVE 4/4 • STEP 30 CLOSED POSITIVE 4/4 • STEP 31 CLOSED POSITIVE 4/4 • STEP 32 CLOSED POSITIVE 4/4 • STEP 33 CLOSED POSITIVE 4/4 • STEP 34 CLOSED POSITIVE 4/4 • STEP 35 EXACT CORE CLOSURE POSITIVE • STEP 36 OPEN — INTERNAL ESSENTIAL FAILURE LOCALIZATION";

    public const string Summary =
        "Physical 0.0.155 closes the Step-36 resource-pack handoff: the exact receipt-backed Slay the Spire 2 PCK mounted additively through exact GodotSharp, LoadResourcePack returned true, and Godot.DirAccess.Open proved res://localization/eng before Gate C. The unchanged exact transformed ExecuteEssential invocation then threw after entry, but the 0.0.155 failure formatter retained only one reflection layer and reported TargetInvocationException: Arg_TargetInvocationException. 0.0.156 changes observation only: it keeps the same exact single MethodInfo.Invoke and records the complete InnerException chain, ReflectionTypeLoadException loader exceptions, base exception, post-failure OneTimeInitialization state, resolver/load deltas, and sts2/GodotSharp private-context continuity.";

    public const string InitialStatus =
        "Status: exact Step-35 runtime/authority closure remains physically positive and the Step-36 game-PCK lifecycle is now physically positive. Step 36.0.2 is a diagnostic refinement at the same ExecuteEssential boundary; it does not split the method, retry it, reset state, invoke child initializers directly, or authorize ExecuteDeferred/PrewarmJit/game entry/native game loading/Harmony/MonoMod/arbitrary resolver fallback.";

    public const string ExpectedDisplayVersion = "0.0.156";
    public const string ExpectedBuildVersion = "156";
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
        "physical Step34 exact PrewarmJit closure -> exact ExecuteVeryEarly source token 0x06007D02 + async MoveNext token 0x0600BC71 -> 0.0.124 exact Invoke frontier -> 0.0.126 same-run durable correlation -> 0.0.129 ECMA bridge diagnosis -> 0.0.130/0.0.131 Save/Platform localization -> 0.0.132 CommandLineHelper-triggered initialization -> 0.0.133/0.0.135 instrumentation rejection diagnosis -> 0.0.136 CommandLine dictionary constructor frontier -> 0.0.138 NATURAL dictionary-native callback boundary + COMPAT Godot.OS callback boundary -> 0.0.139 CI 209/210 stale-summary stop -> 0.0.140 NATURAL/OS-RECON/FORWARD proof reaches GS031/GS024 and GodotFileIo.CreateDirectory -> 0.0.141 CI 210/211 callback-telemetry assertion stop -> 0.0.143 CORE-HANDOFF accepts the 1,800-byte/225-pointer table through NativeFuncs.Initialize(IntPtr,int), initialized=true, then natural GetCmdlineArgs reaches OS.get_Singleton -> 0.0.144 EngineGetSingleton/GS035 native instance-binding frontier -> 0.0.145 godotApiCacheUpdated=false reverse cache absent while CSharpLanguage exists -> 0.0.146 physically proves 37-pointer ManagedCallbacks creation, game-script lookup, GDMonoCache cache adoption with reverseBindingReady=true, and GD_OnCoreApiAssemblyLoaded return; Gate C then stops before target binding only because the old pre-bootstrap resolver snapshot rejects the bootstrap's exact eight-request host-framework delta -> 0.0.147 validates that exact measured delta and seals a post-bootstrap resolver baseline but Codemagic stops at 212/213 on a stale negative-test message assertion -> 0.0.148 corrects only that host regression contract with bridge/Gate-C runtime unchanged -> 0.0.149 adds Gate-D forwarded file/byte progress, a live heartbeat UI, and verified warm .NET+iOS workload caching without altering bridge/Gate-C semantics -> physical 0.0.149 returns/awaits diagnostic ExecuteVeryEarly RanToCompletion with confinement PASS and records Gate C PASS, then UI reaches Gate-D terminal 4/4 while durable telemetry remains at D_START -> 0.0.150 adds durable Gate-D finalization boundaries plus exact transformed sts2 + exact prepared GodotSharp authority mode but Codemagic iOS compile stops on missing Core.Runtime import after 895/895 static + 214/214 host + native-link PASS -> 0.0.152 physically proves exact transformed sts2 + exact prepared GodotSharp ExecuteVeryEarly completion and Gate-D passed=true/exactAuthority=true through D_TASK_RETURN_START, localizing the remaining defect to UIKit await continuation -> physical 0.0.153 moves Gate D behind an outer worker and reaches D_WORKER_RETURN with passed=true/exactAuthority=true but still never resumes the captured UIKit await continuation -> 0.0.154 uses ConfigureAwait(false) for Gate-D completion plus explicit InvokeOnMainThread finalization; physical Step 36 then reaches exact ExecuteEssential and fails only at missing res://localization/eng -> physical 0.0.155 proves that exact PCK mount and localization probe, then reaches a nested TargetInvocationException inside unchanged ExecuteEssential -> 0.0.156 preserves the one-call boundary and adds full failure-chain/state/context telemetry -> strict resolver, initializer-bearing rejection, native-game-load refusal, and exact Step32 source isolation remain unchanged";

    public const string Step36ImplementationMarker =
        "physical exact Step35 core closure -> physical 0.0.154 localization-path failure -> physical 0.0.155 exact receipt-backed PCK LoadResourcePack(replaceFiles=false, offset=0) + res://localization/eng proof -> source ExecuteEssential token 0x06007D03 semantic reproof -> one exact ExecuteEssential MethodInfo.Invoke -> on failure record complete InnerException chain + ReflectionTypeLoadException.LoaderExceptions + GetBaseException + post-failure state + resolver/load deltas + sts2/GodotSharp load-context continuity -> no retry/no state reset/no child probes -> on success require state 2 and final OfflineReady/hash/context reproof; ExecuteDeferred/PrewarmJit/entry remain forbidden";

    public static string DisplayVersion =>
        NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleShortVersionString")?.ToString() ?? "unknown";

    public static string DisplayBuild =>
        NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleVersion")?.ToString() ?? "unknown";

    public static bool BundleIdentityMatchesExpected =>
        string.Equals(DisplayVersion, ExpectedDisplayVersion, StringComparison.Ordinal) &&
        string.Equals(DisplayBuild, ExpectedBuildVersion, StringComparison.Ordinal);
}
