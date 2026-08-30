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
        "STEP 35.0.8 — SAVE/PLATFORM/GODOT NATIVE-BOUNDARY LOCALIZATION";

    public const string MilestoneLine =
        "STEPS 01–26 CLOSED • STEP 27 CLOSED NEGATIVE • STEP 28 CLOSED POSITIVE 5/5 • STEP 29 CLOSED POSITIVE 4/4 • STEP 30 CLOSED POSITIVE 4/4 • STEP 31 CLOSED POSITIVE 4/4 • STEP 32 CLOSED POSITIVE 4/4 • STEP 33 CLOSED POSITIVE 4/4 • STEP 34 CLOSED POSITIVE 4/4 • STEP 35 OPEN";

    public const string Summary =
        "Physical 0.0.130 proved the generic-delegate bridge correction and finally emitted durable game-body markers: ExecuteVeryEarly.MoveNext entered, TestMode.get_IsOn entered, SaveManager..cctor entered, and SaveManager.get_Instance entered; a second TestMode.get_IsOn marker then appeared before System.Text.Json and System.Collections.Concurrent 8→9 host resolution, after which the process hard-terminated. Step 35.0.8 / 0.0.131 keeps the exact source/resolver authority frozen and adds only narrow entry/callsite checkpoints through SaveManager.ConstructDefault, UserDataPathProvider, PlatformUtil, NullPlatformUtilStrategy and GodotFileIo, including before/after Godot.DirAccess calls.";

    public const string InitialStatus =
        "Status: Steps 32–34 are CLOSED POSITIVE at 4/4. Step 35 remains OPEN. Physical 0.0.130 localized the hard kill inside work reached from SaveManager.get_Instance, before either settings-init method. Exact sts2.dll analysis identifies the normal branch through ConstructDefault → UserDataPathProvider.GetAccountScopedBasePath → PlatformUtil static initialization → NullPlatformUtilStrategy → GodotFileIo.CreateDirectory. Candidate 0.0.131 adds output-only checkpoints at that path and immediately around DirAccess.DirExistsAbsolute/MakeDirRecursiveAbsolute; it does not initialize Godot or broaden any resolver/startup authority.";

    public const string ExpectedDisplayVersion = "0.0.131";
    public const string ExpectedBuildVersion = "131";
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
        "physical Step34 exact PrewarmJit closure -> exact ExecuteVeryEarly source token 0x06007D02 + async MoveNext token 0x0600BC71 -> 0.0.124 exact Invoke frontier -> 0.0.126 same-run durable correlation confirms no C_INVOKE_RETURNED -> 0.0.127/0.0.128 Gate-A Cecil failures -> 0.0.129 deferred-open bounded writer fix -> managed MissingMethodException on synthetic Action<string>.Invoke(string) -> 0.0.130 ECMA-correct Action<string>::Invoke(!0) physically emits INMETHOD_001/010/SaveManager cctor/020 and localizes termination under SaveManager.ConstructDefault before settings initialization -> exact sts2.dll path analysis identifies UserDataPathProvider -> PlatformUtil cctor -> NullPlatformUtilStrategy -> GodotFileIo.CreateDirectory -> Godot.DirAccess.DirExistsAbsolute/MakeDirRecursiveAbsolute -> Step35.0.8 diagnostic clone adds verified entry markers plus pre/post DirExistsAbsolute and MakeDirRecursiveAbsolute callsite markers -> exact Step32 transformed source remains untouched -> strict runtime resolver/Task await <=60s/Godot-startup prohibition unchanged -> isolation reproof";

    public static string DisplayVersion =>
        NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleShortVersionString")?.ToString() ?? "unknown";

    public static string DisplayBuild =>
        NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleVersion")?.ToString() ?? "unknown";

    public static bool BundleIdentityMatchesExpected =>
        string.Equals(DisplayVersion, ExpectedDisplayVersion, StringComparison.Ordinal) &&
        string.Equals(DisplayBuild, ExpectedBuildVersion, StringComparison.Ordinal);
}
