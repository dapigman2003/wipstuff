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
        "STEP 35.0.14 — COMPREHENSIVE GODOTSHARP / NATIVE RECON + DUAL MODE";

    public const string MilestoneLine =
        "STEPS 01–26 CLOSED • STEP 27 CLOSED NEGATIVE • STEP 28 CLOSED POSITIVE 5/5 • STEP 29 CLOSED POSITIVE 4/4 • STEP 30 CLOSED POSITIVE 4/4 • STEP 31 CLOSED POSITIVE 4/4 • STEP 32 CLOSED POSITIVE 4/4 • STEP 33 CLOSED POSITIVE 4/4 • STEP 34 CLOSED POSITIVE 4/4 • STEP 35 OPEN";

    public const string Summary =
        "Physical 0.0.136 entered CommandLineHelper..cctor and hard-terminated inside Godot.Collections.Dictionary<string,string> construction after CL_CRITICAL_001_PRE. This rebuilt Step 35.0.14 / 0.0.137 adds read-only bundle-wide Mach-O/native reconnaissance, a separately verified entry-only GodotSharp diagnostic derivative, and two fresh-process modes in one app: NATURAL preserves the original Godot dictionary for inner localization; COMPAT applies only the four-reference BCL Dictionary<string,string> substitution to advance toward the still-natural Godot.OS.GetCmdlineArgs/native-callback boundary.";

    public const string InitialStatus =
        "Status: Steps 32–34 are CLOSED POSITIVE at 4/4. Step 35 remains OPEN. Physical 0.0.136 localized the hard termination to Godot.Collections.Dictionary<string,string> construction before _args assignment; the final resolver event remains contextual rather than causal. Candidate 0.0.137 now carries both a natural deep-recon mode and the bounded managed-dictionary compatibility mode, plus a same-run GodotSharp IL/native-callback map and read-only Mach-O dependency/rpath/symbol/string inventory. Resolver authority, native-load refusal, Godot-startup prohibition, and exact Step-32 authority are unchanged.";

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
        "physical Step34 exact PrewarmJit closure -> exact ExecuteVeryEarly source token 0x06007D02 + async MoveNext token 0x0600BC71 -> 0.0.124 exact Invoke frontier -> 0.0.126 same-run durable correlation -> 0.0.129 ECMA bridge diagnosis -> 0.0.130/0.0.131 Save/Platform localization -> 0.0.132 CommandLineHelper-triggered initialization -> 0.0.133/0.0.135 instrumentation rejection diagnosis -> 0.0.136 stack-neutral markers enter CommandLineHelper..cctor and stop after CL_CRITICAL_001_PRE, physically localizing the hard kill to Godot.Collections.Dictionary<string,string> construction -> rebuilt 0.0.137 Gate A performs read-only exact-tree Mach-O dependency/rpath/symbol/string inventory plus GodotSharp IL/calli/PInvoke/native-callback mapping, emits a separately verified entry-only GodotSharp diagnostic derivative, and offers NATURAL (original Godot dictionary preserved) and COMPAT (only _args/.ctor/set_Item/TryGetValue rewritten to BCL Dictionary<string,string>) fresh-process modes -> Godot.OS.GetCmdlineArgs remains natural in both -> strict runtime resolver, initializer-bearing rejection, native-load refusal, <=60s Task await, no Godot bootstrap, and exact Step32 source isolation remain unchanged";

    public static string DisplayVersion =>
        NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleShortVersionString")?.ToString() ?? "unknown";

    public static string DisplayBuild =>
        NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleVersion")?.ToString() ?? "unknown";

    public static bool BundleIdentityMatchesExpected =>
        string.Equals(DisplayVersion, ExpectedDisplayVersion, StringComparison.Ordinal) &&
        string.Equals(DisplayBuild, ExpectedBuildVersion, StringComparison.Ordinal);
}
