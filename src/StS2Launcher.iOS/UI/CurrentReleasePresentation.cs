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
        "STEP 35.0.16 — GODOT CALLBACK BOUNDARY + MANAGED COMMAND-LINE FORWARD PROBE";

    public const string MilestoneLine =
        "STEPS 01–26 CLOSED • STEP 27 CLOSED NEGATIVE • STEP 28 CLOSED POSITIVE 5/5 • STEP 29 CLOSED POSITIVE 4/4 • STEP 30 CLOSED POSITIVE 4/4 • STEP 31 CLOSED POSITIVE 4/4 • STEP 32 CLOSED POSITIVE 4/4 • STEP 33 CLOSED POSITIVE 4/4 • STEP 34 CLOSED POSITIVE 4/4 • STEP 35 OPEN";

    public const string Summary =
        "Physical 0.0.138 produced the decisive NATURAL/COMPAT split. NATURAL entered GodotSharp Dictionary creation and stopped after GS014 inside the native dictionary thunk. COMPAT passed CL_CRITICAL_001_POST, reached CL_CRITICAL_002_PRE, entered Godot.OS..cctor as GS033, and stopped before GS032 GetCmdlineArgs. Step 35.0.16 / 0.0.139 preserves both controls, expands OS-cctor closure probes, and adds a bounded FORWARD mode using the proven BCL Dictionary<string,string> rewrite plus one local new string[0] command-line provider substitution.";

    public const string InitialStatus =
        "Status: Steps 32–34 are CLOSED POSITIVE at 4/4. Step 35 remains OPEN. 0.0.138 physically showed that both the natural Dictionary path and the natural Godot.OS type initializer reach GodotSharp native callback plumbing before any permitted Godot bootstrap has initialized NativeFuncs._unmanagedCallbacks. This is strong boundary evidence, not yet proof of a single null-pointer root cause. Candidate 0.0.139 does not initialize Godot or load native code: OS-RECON deepens entry-only localization from Godot.OS..cctor; FORWARD bypasses only the already-localized command-line Godot dependency with a local empty string[] provider. Resolver authority, native-load refusal, Godot-startup prohibition, and exact Step-32 authority remain unchanged.";

    public const string ExpectedDisplayVersion = "0.0.139";
    public const string ExpectedBuildVersion = "139";
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
        "physical Step34 exact PrewarmJit closure -> exact ExecuteVeryEarly source token 0x06007D02 + async MoveNext token 0x0600BC71 -> 0.0.124 exact Invoke frontier -> 0.0.126 same-run durable correlation -> 0.0.129 ECMA bridge diagnosis -> 0.0.130/0.0.131 Save/Platform localization -> 0.0.132 CommandLineHelper-triggered initialization -> 0.0.133/0.0.135 instrumentation rejection diagnosis -> 0.0.136 CommandLine dictionary constructor frontier -> 0.0.138 NATURAL enters NativeFuncs.godotsharp_dictionary_new and stops after CustomUnsafe.AsPointer (GS014), while COMPAT passes managed dictionary assignment then enters Godot.OS..cctor (GS033) before GetCmdlineArgs (GS032) -> reconnaissance maps both paths to NativeFuncs._unmanagedCallbacks calli thunks -> 0.0.139 preserves NATURAL, retains dictionary-only OS-RECON with deeper OS-cctor closure markers, and adds FORWARD with exactly one local new string[0] command-line provider substitution -> strict runtime resolver, initializer-bearing rejection, native-load refusal, <=60s Task await, no Godot bootstrap, and exact Step32 source isolation remain unchanged";

    public static string DisplayVersion =>
        NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleShortVersionString")?.ToString() ?? "unknown";

    public static string DisplayBuild =>
        NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleVersion")?.ToString() ?? "unknown";

    public static bool BundleIdentityMatchesExpected =>
        string.Equals(DisplayVersion, ExpectedDisplayVersion, StringComparison.Ordinal) &&
        string.Equals(DisplayBuild, ExpectedBuildVersion, StringComparison.Ordinal);
}
