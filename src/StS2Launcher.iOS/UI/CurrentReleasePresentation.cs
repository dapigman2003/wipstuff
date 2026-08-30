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
        "STEP 35.0.9 — NULL-PLATFORM CONSTRUCTOR CALLSITE LOCALIZATION";

    public const string MilestoneLine =
        "STEPS 01–26 CLOSED • STEP 27 CLOSED NEGATIVE • STEP 28 CLOSED POSITIVE 5/5 • STEP 29 CLOSED POSITIVE 4/4 • STEP 30 CLOSED POSITIVE 4/4 • STEP 31 CLOSED POSITIVE 4/4 • STEP 32 CLOSED POSITIVE 4/4 • STEP 33 CLOSED POSITIVE 4/4 • STEP 34 CLOSED POSITIVE 4/4 • STEP 35 OPEN";

    public const string Summary =
        "Physical 0.0.131 reached SaveManager.ConstructDefault, UserDataPathProvider.GetAccountScopedBasePath, PlatformUtil..cctor and NullPlatformUtilStrategy..ctor, then hard-terminated after the System.Collections.Concurrent 8→9 host binding and before GodotFileIo..ctor. That falsifies the prior first-DirAccess boundary hypothesis at this level. Step 35.0.9 / 0.0.132 keeps all authority frozen and instruments every non-base call/callvirt/newobj in NullPlatformUtilStrategy..ctor with ordered pre/post markers; the same-run static map now includes that constructor's exact IL and CALLSITE ordinals.";

    public const string InitialStatus =
        "Status: Steps 32–34 are CLOSED POSITIVE at 4/4. Step 35 remains OPEN. Physical 0.0.131 narrows the hard kill to code executing inside NullPlatformUtilStrategy..ctor, after its entry marker and before GodotFileIo..ctor. Candidate 0.0.132 adds only a verified ordered pre/post sweep around that constructor's existing managed call/newobj instructions and extends the output-only static map; it does not initialize Godot, broaden resolution, or alter the exact closed transformed source.";

    public const string ExpectedDisplayVersion = "0.0.132";
    public const string ExpectedBuildVersion = "132";
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
        "physical Step34 exact PrewarmJit closure -> exact ExecuteVeryEarly source token 0x06007D02 + async MoveNext token 0x0600BC71 -> 0.0.124 exact Invoke frontier -> 0.0.126 same-run durable correlation confirms no C_INVOKE_RETURNED -> 0.0.127/0.0.128 Gate-A Cecil failures -> 0.0.129 deferred-open bounded writer fix -> managed MissingMethodException on synthetic Action<string>.Invoke(string) -> 0.0.130 ECMA-correct Action<string>::Invoke(!0) physically reaches SaveManager.get_Instance -> 0.0.131 physically reaches ConstructDefault/UserDataPathProvider/PlatformUtil cctor/NullPlatformUtilStrategy..ctor but no GodotFileIo..ctor -> Step35.0.9 diagnostic clone preserves all prior markers and adds verified ordered pre/post markers around every non-base call/callvirt/newobj in NullPlatformUtilStrategy..ctor plus exact constructor IL/CALLSITE ordinals in the same-run static map -> exact Step32 transformed source remains untouched -> strict runtime resolver/Task await <=60s/Godot-startup prohibition unchanged -> isolation reproof";

    public static string DisplayVersion =>
        NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleShortVersionString")?.ToString() ?? "unknown";

    public static string DisplayBuild =>
        NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleVersion")?.ToString() ?? "unknown";

    public static bool BundleIdentityMatchesExpected =>
        string.Equals(DisplayVersion, ExpectedDisplayVersion, StringComparison.Ordinal) &&
        string.Equals(DisplayBuild, ExpectedBuildVersion, StringComparison.Ordinal);
}
