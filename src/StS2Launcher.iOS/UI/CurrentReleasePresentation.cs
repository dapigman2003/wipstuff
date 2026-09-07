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
        "STEP 37.0 — CONTROLLED GAME.TSCN ADMISSION AFTER PHYSICAL STEP-36 CLOSURE";

    public const string MilestoneLine =
        "STEPS 01–26 CLOSED • STEP 27 CLOSED NEGATIVE • STEPS 28–34 CLOSED POSITIVE • STEP 35 CLOSED POSITIVE • STEP 36 CLOSED POSITIVE 4/4 • STEP 37 OPEN — CONTROLLED OFF-TREE GAME-SCENE ADMISSION";

    public const string Summary =
        "Physical 0.0.159 closed Step 36.0.5 at 4/4: the dependency-aware ModelDb authority completed unchanged ExecuteEssential state 1→2, including ModelIdSerializationCache.Init, ModelDb.InitIds, MessageTypes.Initialize, and ActionTypes.Initialize, with final OfflineReady 428/428 and zero initializer-bearing/rejected/native escape. Step 37.0 freezes that authority and admits only a sealed game.tscn copy. The exact PCK scene is pinned at 10,414 bytes / SHA-256 aaec1e04f689122fd812b83fee09ea6e30e2991cf5851dc599b01b7802320ad8 / MD5 dc9a89798eb1bb38e23563866e746ac5. Exactly three copied-scene compatibility edits neutralize FmodBankLoader/FmodListener2D before PackedScene load and off-tree instantiation.";

    public const string InitialStatus =
        "Status: Step 36.0.5 is physically closed 4/4 on 0.0.159. For 0.0.160 use a fresh process: Step 15 A-C → Step 35.0.32 MODEL-BOOTSTRAP 4/4 → Step 36.0.5 4/4 → Step 37.0 once. Step 37 extracts and seals game.tscn from the already verified PCK, prepares a private three-edit FMOD-neutral copy, loads it as PackedScene, and instantiates it off-tree only. SceneTree insertion, NGame.GameStartup, LaunchMainMenu, ExecuteDeferred, Steam initialization, FMOD/Spine/Sentry native GDExtensions, and gameplay remain forbidden.";

    public const string ExpectedDisplayVersion = "0.0.160";
    public const string ExpectedBuildVersion = "160";
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
        "physical Step34 exact PrewarmJit closure -> exact ExecuteVeryEarly source token 0x06007D02 + async MoveNext token 0x0600BC71 -> 0.0.124 exact Invoke frontier -> 0.0.126 same-run durable correlation -> 0.0.129 ECMA bridge diagnosis -> 0.0.130/0.0.131 Save/Platform localization -> 0.0.132 CommandLineHelper-triggered initialization -> 0.0.133/0.0.135 instrumentation rejection diagnosis -> 0.0.136 CommandLine dictionary constructor frontier -> 0.0.138 NATURAL dictionary-native callback boundary + COMPAT Godot.OS callback boundary -> 0.0.139 CI 209/210 stale-summary stop -> 0.0.140 NATURAL/OS-RECON/FORWARD proof reaches GS031/GS024 and GodotFileIo.CreateDirectory -> 0.0.141 CI 210/211 callback-telemetry assertion stop -> 0.0.143 CORE-HANDOFF accepts the 1,800-byte/225-pointer table through NativeFuncs.Initialize(IntPtr,int), initialized=true, then natural GetCmdlineArgs reaches OS.get_Singleton -> 0.0.144 EngineGetSingleton/GS035 native instance-binding frontier -> 0.0.145 godotApiCacheUpdated=false reverse cache absent while CSharpLanguage exists -> 0.0.146 physically proves 37-pointer ManagedCallbacks creation, game-script lookup, GDMonoCache cache adoption with reverseBindingReady=true, and GD_OnCoreApiAssemblyLoaded return; Gate C then stops before target binding only because the old pre-bootstrap resolver snapshot rejects the bootstrap's exact eight-request host-framework delta -> 0.0.147 validates that exact measured delta and seals a post-bootstrap resolver baseline but Codemagic stops at 212/213 on a stale negative-test message assertion -> 0.0.148 corrects only that host regression contract with bridge/Gate-C runtime unchanged -> 0.0.149 adds Gate-D forwarded file/byte progress, a live heartbeat UI, and verified warm .NET+iOS workload caching without altering bridge/Gate-C semantics -> physical 0.0.149 returns/awaits diagnostic ExecuteVeryEarly RanToCompletion with confinement PASS and records Gate C PASS, then UI reaches Gate-D terminal 4/4 while durable telemetry remains at D_START -> 0.0.150 adds durable Gate-D finalization boundaries plus exact transformed sts2 + exact prepared GodotSharp authority mode but Codemagic iOS compile stops on missing Core.Runtime import after 895/895 static + 214/214 host + native-link PASS -> 0.0.152 physically proves exact transformed sts2 + exact prepared GodotSharp ExecuteVeryEarly completion and Gate-D passed=true/exactAuthority=true through D_TASK_RETURN_START, localizing the remaining defect to UIKit await continuation -> physical 0.0.153 moves Gate D behind an outer worker and reaches D_WORKER_RETURN with passed=true/exactAuthority=true but still never resumes the captured UIKit await continuation -> 0.0.154 uses ConfigureAwait(false) for Gate-D completion plus explicit InvokeOnMainThread finalization; physical Step 36 then reaches exact ExecuteEssential and fails only at missing res://localization/eng -> physical 0.0.155 proves that exact PCK mount and localization probe, then reaches a nested TargetInvocationException inside unchanged ExecuteEssential -> physical 0.0.156 resolves that chain to ModelDb.Init / BowlbugsNormal..cctor / missing MONSTER.BOWLBUG_EGG with clean resolver/native context -> 0.0.157 Gate A fails safely on nonexistent ModelDb.Contains(Type) metadata assumption -> 0.0.158 reaches unchanged ExecuteEssential but fails on concrete-parameter Dictionary ContainsKey MemberRef binding -> 0.0.159 re-encodes ContainsKey/get_Item/Remove/Add with declaring-type VAR(0)/VAR(1) using the physically proven Step-35 ECMA pattern -> strict resolver, initializer-bearing rejection, native-game-load refusal, and exact Step32 source isolation remain unchanged";

    public const string Step36ImplementationMarker =
        "physical 0.0.156 exact PCK + localization PASS -> unchanged ExecuteEssential -> ModelDb.Init -> Activator.CreateInstance(BowlbugsNormal) -> BowlbugsNormal..cctor -> ModelDb.Monster<BowlbugEgg>() -> KeyNotFound MONSTER.BOWLBUG_EGG with state 1->2 and zero rejected/initializer-bearing/native loads -> derive canonical AbstractModelSubtypes order -> scan direct static-cctor generic ModelDb<T> dependencies -> compute backward-edge dependency closure -> preinject dependency order -> normal ModelDb iteration reuses and remove/re-adds same instances at canonical positions using VAR(0)/VAR(1) Dictionary MemberRefs -> exact prepared GodotSharp bridge retained -> full unchanged ExecuteEssential one-shot -> complete nested failure telemetry retained -> final selected-authority/OfflineReady/context audit; ExecuteDeferred/PrewarmJit/entry remain forbidden";


    public const string Step37ImplementationMarker =
        "physical 0.0.159 Step-36.0.5 4/4 closure -> exact receipt-backed PCK game.tscn 10,414-byte SHA-256/MD5 seal -> copied three-edit FMOD-neutral derivative only -> exact GodotSharp ResourceLoader.Load PackedScene cache-ignore -> off-tree Instantiate GenEditState.Disabled -> require NGame + expected managed hierarchy + inert FmodBankLoader/FmodListener2D Node types -> IsInsideTree=false -> immediate release -> zero initializer-bearing/rejected/native escape; AddChild/GameStartup/LaunchMainMenu/ExecuteDeferred/Steam/native GDExtensions remain forbidden";

    public static string DisplayVersion =>
        NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleShortVersionString")?.ToString() ?? "unknown";

    public static string DisplayBuild =>
        NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleVersion")?.ToString() ?? "unknown";

    public static bool BundleIdentityMatchesExpected =>
        string.Equals(DisplayVersion, ExpectedDisplayVersion, StringComparison.Ordinal) &&
        string.Equals(DisplayBuild, ExpectedBuildVersion, StringComparison.Ordinal);
}
