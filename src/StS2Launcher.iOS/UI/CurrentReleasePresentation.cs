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
        "STEP 35.0.13 — STACK-NEUTRAL COMMAND-LINE/GODOT BOUNDARY LOCALIZATION";

    public const string MilestoneLine =
        "STEPS 01–26 CLOSED • STEP 27 CLOSED NEGATIVE • STEP 28 CLOSED POSITIVE 5/5 • STEP 29 CLOSED POSITIVE 4/4 • STEP 30 CLOSED POSITIVE 4/4 • STEP 31 CLOSED POSITIVE 4/4 • STEP 32 CLOSED POSITIVE 4/4 • STEP 33 CLOSED POSITIVE 4/4 • STEP 34 CLOSED POSITIVE 4/4 • STEP 35 OPEN";

    public const string Summary =
        "Physical 0.0.135 reproduced the pre-instruction-zero CommandLineHelper InvalidProgramException even with serialized MaxStack raised and verified, disproving the MaxStack-only theory. Step 35.0.13 / 0.0.136 removes all live-stack CL/CLTV runtime callbacks and retains only empty-stack cctor entry/critical markers around the Godot dictionary constructor and Godot.OS.GetCmdlineArgs while preserving every prior resolver/native/Godot-startup prohibition.";

    public const string InitialStatus =
        "Status: Steps 32–34 are CLOSED POSITIVE at 4/4. Step 35 remains OPEN. The authoritative physical game frontier remains the 0.0.132 interval inside CommandLineHelper type initialization triggered by NullPlatformUtilStrategy..ctor; 0.0.133 and 0.0.135 are CLOSED DIAGNOSTIC NEGATIVE because live-stack CommandLine instrumentation was CLR-invalid before instruction zero. Candidate 0.0.136 is diagnostic-only: it removes those live-stack callbacks, preserves exact-source maps, verifies unchanged cctor MaxStack, and uses only empty-stack critical boundaries to distinguish dictionary construction from Godot.OS.GetCmdlineArgs without bootstrapping Godot or broadening runtime authority.";

    public const string ExpectedDisplayVersion = "0.0.136";
    public const string ExpectedBuildVersion = "136";
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
        "physical Step34 exact PrewarmJit closure -> exact ExecuteVeryEarly source token 0x06007D02 + async MoveNext token 0x0600BC71 -> 0.0.124 exact Invoke frontier -> 0.0.126 same-run durable correlation confirms no C_INVOKE_RETURNED -> 0.0.127/0.0.128 Gate-A Cecil failures -> 0.0.129 deferred-open bounded writer fix -> managed MissingMethodException on synthetic Action<string>.Invoke(string) -> 0.0.130 ECMA-correct Action<string>::Invoke(!0) reaches SaveManager.get_Instance -> 0.0.131 reaches ConstructDefault/UserDataPathProvider/PlatformUtil cctor/NullPlatformUtilStrategy..ctor -> 0.0.132 emits NP003_PRE before CommandLineHelper.TryGetValue with no POST while exact map says CALLSITE#002 -> 0.0.133 corrects NP002 but its CommandLine cctor sweep faults with managed InvalidProgramException before any cctor marker and reaches normal RUN_END -> 0.0.135 reproduces the same pre-zero InvalidProgramException despite verified MaxStack headroom, disproving MaxStack-only causation -> Step35.0.13 removes all live-stack CL/CLTV runtime callbacks, preserves exact-source CL/CLTV maps, verifies unchanged cctor MaxStack, and retains four stack-neutral critical markers around dictionary assignment and Godot.OS.GetCmdlineArgs result storage -> exact Step32 transformed source remains untouched -> strict runtime resolver/Task await <=60s/Godot-startup prohibition unchanged -> isolation reproof";

    public static string DisplayVersion =>
        NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleShortVersionString")?.ToString() ?? "unknown";

    public static string DisplayBuild =>
        NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleVersion")?.ToString() ?? "unknown";

    public static bool BundleIdentityMatchesExpected =>
        string.Equals(DisplayVersion, ExpectedDisplayVersion, StringComparison.Ordinal) &&
        string.Equals(DisplayBuild, ExpectedBuildVersion, StringComparison.Ordinal);
}
