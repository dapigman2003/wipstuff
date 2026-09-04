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
        "STEP 35.0.27 — EXACT AUTHORITY CLOSURE + GATE D FINALIZATION";

    public const string MilestoneLine =
        "STEPS 01–26 CLOSED • STEP 27 CLOSED NEGATIVE • STEP 28 CLOSED POSITIVE 5/5 • STEP 29 CLOSED POSITIVE 4/4 • STEP 30 CLOSED POSITIVE 4/4 • STEP 31 CLOSED POSITIVE 4/4 • STEP 32 CLOSED POSITIVE 4/4 • STEP 33 CLOSED POSITIVE 4/4 • STEP 34 CLOSED POSITIVE 4/4 • STEP 35 OPEN";

    public const string Summary =
        "Physical 0.0.149 completed diagnostic Gate C end-to-end: exact target binding on the diagnostic derivative, MethodInfo.Invoke return, Task RanToCompletion, bounded await return, post-await resolver/native confinement PASS, and C_RESULT_RECORD_PASS. The UI then reached Gate-D terminal 4/4 final-check progress while the durable journal remained at D_START. Step 35.0.27 / 0.0.150 keeps the proven bridge intact, adds durable Gate-D finalization boundaries and avoids heavy detail-label churn, and adds an EXACT-CLOSURE mode that CLR-admits the exact closed Step-32 transformed sts2 plus exact prepared GodotSharp through the same physically proven source-built Godot 4.5.1 bridge.";

    public const string InitialStatus =
        "Status: Steps 32–34 are CLOSED POSITIVE at 4/4 and Step 35 remains OPEN pending an exact-authority device result. Physical 0.0.149 proves diagnostic Gate C completes normally and reaches Gate D. The current 0.0.150 candidate preserves the bridge/resolver contract, fixes/localizes Gate-D finalization, and exposes an explicit EXACT-CLOSURE button using the exact transformed sts2 and exact prepared GodotSharp CLR inputs. The pinned .NET 9.0.314 + iOS workload 9.0.314.3 warm cache is preserved.";

    public const string ExpectedDisplayVersion = "0.0.150";
    public const string ExpectedBuildVersion = "150";
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
        "physical Step34 exact PrewarmJit closure -> exact ExecuteVeryEarly source token 0x06007D02 + async MoveNext token 0x0600BC71 -> 0.0.124 exact Invoke frontier -> 0.0.126 same-run durable correlation -> 0.0.129 ECMA bridge diagnosis -> 0.0.130/0.0.131 Save/Platform localization -> 0.0.132 CommandLineHelper-triggered initialization -> 0.0.133/0.0.135 instrumentation rejection diagnosis -> 0.0.136 CommandLine dictionary constructor frontier -> 0.0.138 NATURAL dictionary-native callback boundary + COMPAT Godot.OS callback boundary -> 0.0.139 CI 209/210 stale-summary stop -> 0.0.140 NATURAL/OS-RECON/FORWARD proof reaches GS031/GS024 and GodotFileIo.CreateDirectory -> 0.0.141 CI 210/211 callback-telemetry assertion stop -> 0.0.143 CORE-HANDOFF accepts the 1,800-byte/225-pointer table through NativeFuncs.Initialize(IntPtr,int), initialized=true, then natural GetCmdlineArgs reaches OS.get_Singleton -> 0.0.144 EngineGetSingleton/GS035 native instance-binding frontier -> 0.0.145 godotApiCacheUpdated=false reverse cache absent while CSharpLanguage exists -> 0.0.146 physically proves 37-pointer ManagedCallbacks creation, game-script lookup, GDMonoCache cache adoption with reverseBindingReady=true, and GD_OnCoreApiAssemblyLoaded return; Gate C then stops before target binding only because the old pre-bootstrap resolver snapshot rejects the bootstrap's exact eight-request host-framework delta -> 0.0.147 validates that exact measured delta and seals a post-bootstrap resolver baseline but Codemagic stops at 212/213 on a stale negative-test message assertion -> 0.0.148 corrects only that host regression contract with bridge/Gate-C runtime unchanged -> 0.0.149 adds Gate-D forwarded file/byte progress, a live heartbeat UI, and verified warm .NET+iOS workload caching without altering bridge/Gate-C semantics -> physical 0.0.149 returns/awaits diagnostic ExecuteVeryEarly RanToCompletion with confinement PASS and records Gate C PASS, then UI reaches Gate-D terminal 4/4 while durable telemetry remains at D_START -> 0.0.150 adds durable Gate-D finalization boundaries plus exact transformed sts2 + exact prepared GodotSharp authority mode -> strict resolver, initializer-bearing rejection, native-game-load refusal, <=60s Task await, and exact Step32 source isolation remain unchanged";

    public static string DisplayVersion =>
        NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleShortVersionString")?.ToString() ?? "unknown";

    public static string DisplayBuild =>
        NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleVersion")?.ToString() ?? "unknown";

    public static bool BundleIdentityMatchesExpected =>
        string.Equals(DisplayVersion, ExpectedDisplayVersion, StringComparison.Ordinal) &&
        string.Equals(DisplayBuild, ExpectedBuildVersion, StringComparison.Ordinal);
}
