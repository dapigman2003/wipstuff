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
        "STEP 35.0.14 — MANAGED COMMAND-LINE DICTIONARY COMPATIBILITY PROBE";

    public const string MilestoneLine =
        "STEPS 01–26 CLOSED • STEP 27 CLOSED NEGATIVE • STEP 28 CLOSED POSITIVE 5/5 • STEP 29 CLOSED POSITIVE 4/4 • STEP 30 CLOSED POSITIVE 4/4 • STEP 31 CLOSED POSITIVE 4/4 • STEP 32 CLOSED POSITIVE 4/4 • STEP 33 CLOSED POSITIVE 4/4 • STEP 34 CLOSED POSITIVE 4/4 • STEP 35 OPEN";

    public const string Summary =
        "Physical 0.0.136 finally entered CommandLineHelper..cctor and emitted INMETHOD_CL_CRITICAL_001_PRE, then hard-terminated before the matching POST after the planned System.Collections.Concurrent 8→9 host binding. Step 35.0.14 / 0.0.137 preserves the exact-source map and stack-neutral markers but rewrites only CommandLineHelper._args plus its .ctor/set_Item/TryGetValue references from Godot.Collections.Dictionary<string,string> to the existing System.Collections.Generic.Dictionary<string,string> contract; the natural Godot.OS.GetCmdlineArgs call remains untouched.";

    public const string InitialStatus =
        "Status: Steps 32–34 are CLOSED POSITIVE at 4/4. Step 35 remains OPEN. Physical 0.0.136 moved the diagnostic frontier into CommandLineHelper..cctor and localized the hard termination to Godot.Collections.Dictionary<string,string> construction before _args assignment; the final resolver event remains contextual rather than causal. Candidate 0.0.137 is a diagnostic compatibility derivative that substitutes only that private command-line container with the existing BCL Dictionary<string,string> contract while leaving Godot.OS.GetCmdlineArgs natural and preserving all resolver/native/Godot-startup prohibitions.";

    public const string ExpectedDisplayVersion = "0.0.137";
    public const string ExpectedBuildVersion = "137";
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
        "physical Step34 exact PrewarmJit closure -> exact ExecuteVeryEarly source token 0x06007D02 + async MoveNext token 0x0600BC71 -> 0.0.124 exact Invoke frontier -> 0.0.126 same-run durable correlation confirms no C_INVOKE_RETURNED -> 0.0.127/0.0.128 Gate-A Cecil failures -> 0.0.129 deferred-open bounded writer fix -> managed MissingMethodException on synthetic Action<string>.Invoke(string) -> 0.0.130 ECMA-correct Action<string>::Invoke(!0) reaches SaveManager.get_Instance -> 0.0.131 reaches ConstructDefault/UserDataPathProvider/PlatformUtil cctor/NullPlatformUtilStrategy..ctor -> 0.0.132 localizes to CommandLineHelper.TryGetValue-triggered type initialization -> 0.0.133/0.0.135 expose the managed InvalidProgramException and then disprove MaxStack-only instrumentation rejection -> 0.0.136 stack-neutral-only markers enter CommandLineHelper..cctor and stop after CL_CRITICAL_001_PRE before _args assignment, physically localizing the hard kill to Godot.Collections.Dictionary<string,string> construction; final System.Collections.Concurrent 8→9 binding remains contextual -> Step35.0.14 rewrites only CommandLineHelper._args plus its .ctor/set_Item/TryGetValue MemberRefs to the existing System.Collections.Generic.Dictionary<string,string> contract, retains natural Godot.OS.GetCmdlineArgs, exact-source maps, unchanged cctor MaxStack, corrected NP ordinals, strict runtime resolver/Task await <=60s/native/Godot-startup prohibitions, and exact Step32 source isolation";

    public static string DisplayVersion =>
        NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleShortVersionString")?.ToString() ?? "unknown";

    public static string DisplayBuild =>
        NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleVersion")?.ToString() ?? "unknown";

    public static bool BundleIdentityMatchesExpected =>
        string.Equals(DisplayVersion, ExpectedDisplayVersion, StringComparison.Ordinal) &&
        string.Equals(DisplayBuild, ExpectedBuildVersion, StringComparison.Ordinal);
}
