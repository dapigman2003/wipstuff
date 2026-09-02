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
        "STEP 35.0.19 — GODOT CORE CALLBACK-HANDOFF PROBE";

    public const string MilestoneLine =
        "STEPS 01–26 CLOSED • STEP 27 CLOSED NEGATIVE • STEP 28 CLOSED POSITIVE 5/5 • STEP 29 CLOSED POSITIVE 4/4 • STEP 30 CLOSED POSITIVE 4/4 • STEP 31 CLOSED POSITIVE 4/4 • STEP 32 CLOSED POSITIVE 4/4 • STEP 33 CLOSED POSITIVE 4/4 • STEP 34 CLOSED POSITIVE 4/4 • STEP 35 OPEN";

    public const string Summary =
        "Physical 0.0.140 completed all three controls. NATURAL reached GS031; OS-RECON reached Godot.OS..cctor -> StringName -> GS024; FORWARD cleared CommandLineHelper and reproduced the same callback boundary at GodotFileIo.CreateDirectory -> Godot.DirAccess.DirExistsAbsolute -> StringName -> GS024. 0.0.141 added the separately gated Step15-live CORE-HANDOFF but Codemagic stopped at 210/211 host tests because its invalid-table regression expected zero checkpoints while production intentionally emitted one CB_INITIALIZE_MANAGED_FAIL before any preflight/CLR work. Step 35.0.19 / 0.0.142 preserves the 0.0.141 runtime experiment unchanged and corrects only that failure-telemetry test contract.";

    public const string InitialStatus =
        "Status: Steps 32–34 are CLOSED POSITIVE at 4/4. Step 35 remains OPEN. 0.0.142 is a pre-device host-test contract correction over 0.0.141; CORE-HANDOFF runtime behavior is unchanged. Invalid callback metadata must fail before preflight/CLR work, produce exactly one durable CB_INITIALIZE_MANAGED_FAIL record, and never reach CB_INIT_ENTRY. Candidate 0.0.142 does not fabricate callbacks and never loads the game native executable. NATURAL/OS-RECON/FORWARD retain their fresh-process/no-Godot contracts. CORE-HANDOFF remains the sole explicit exception: after Step 15 proves its project-owned Godot engine live, it obtains the exact callback table from that same source-built Godot 4.5.1 engine, copies it once through GodotSharp NativeFuncs.Initialize(IntPtr,int), then invokes the verified natural diagnostic derivative. A diagnostic 4/4 still cannot close exact Step 35.";

    public const string ExpectedDisplayVersion = "0.0.142";
    public const string ExpectedBuildVersion = "142";
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
        "physical Step34 exact PrewarmJit closure -> exact ExecuteVeryEarly source token 0x06007D02 + async MoveNext token 0x0600BC71 -> 0.0.124 exact Invoke frontier -> 0.0.126 same-run durable correlation -> 0.0.129 ECMA bridge diagnosis -> 0.0.130/0.0.131 Save/Platform localization -> 0.0.132 CommandLineHelper-triggered initialization -> 0.0.133/0.0.135 instrumentation rejection diagnosis -> 0.0.136 CommandLine dictionary constructor frontier -> 0.0.138 NATURAL dictionary-native callback boundary + COMPAT Godot.OS callback boundary -> 0.0.139 CI-only stale-summary failure -> 0.0.140 physical three-mode proof: NATURAL reaches GS031 GetUnsafeAddress, OS-RECON reaches OS cctor -> StringName -> GS024, FORWARD clears CL_CRITICAL_002_POST/INMETHOD_027/NP002_POST and reaches GodotFileIo.CreateDirectory -> DirAccess -> StringName -> GS024 -> uploaded main executable exact SHA-256 7fadae8d46f0074ba745bc3beebe31a13df5fafed2f2ac69cd68b3c5dd8508e6 exposes standard Godot 4.5.1 runtime interop producer -> 0.0.141 enables the source-built native mono/C# module, preserves three prior controls, and adds a fail-closed Step15-live CORE-HANDOFF that obtains the exact godotsharp::get_runtime_interop_funcs table, rejects dotnet-feature/competing-runtime state, invokes private GodotSharp NativeFuncs.Initialize(IntPtr,int) once, then measures the natural ExecuteVeryEarly path -> 0.0.141 CI 210/211 exposes only the negative-test zero-checkpoint mismatch -> 0.0.142 preserves runtime behavior and corrects the failure-telemetry regression -> strict resolver, initializer-bearing rejection, native-game-load refusal, <=60s Task await, and exact Step32 source isolation remain unchanged";

    public static string DisplayVersion =>
        NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleShortVersionString")?.ToString() ?? "unknown";

    public static string DisplayBuild =>
        NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleVersion")?.ToString() ?? "unknown";

    public static bool BundleIdentityMatchesExpected =>
        string.Equals(DisplayVersion, ExpectedDisplayVersion, StringComparison.Ordinal) &&
        string.Equals(DisplayBuild, ExpectedBuildVersion, StringComparison.Ordinal);
}
