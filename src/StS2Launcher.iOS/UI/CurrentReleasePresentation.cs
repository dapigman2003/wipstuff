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
        "STEP 32.0.3 — EXACT-LENGTH PRIVATE IL PATCH";

    public const string MilestoneLine =
        "STEPS 01–26 CLOSED • STEP 27 CLOSED NEGATIVE • STEP 28 CLOSED POSITIVE 5/5 • STEP 29 CLOSED POSITIVE 4/4 • STEP 30 CLOSED POSITIVE 4/4 • STEP 31 CLOSED POSITIVE 4/4 • STEP 32 FIRST REAL STS2 REWRITE";

    public const string Summary =
        "Physical 0.0.114 closed Step 31 positively at 4/4 and confirmed the exact OneTimeInitialization::PrewarmJit() token 0x06007D05/body fingerprint and ten RuntimeHelpers.PrepareMethod sites. Physical 0.0.116 and 0.0.117 then proved whole-module Cecil serialization pulls unrelated external Constant-table metadata (System.Runtime, then Sentry) into a ten-call rewrite. Step 32.0.3 keeps the exact 6+4 stack-neutral behavior change but removes Cecil serialization: Gate B binds the exact sites with deferred/rejecting Cecil, verifies each raw 5-byte call opcode+token, and replaces only those ten 5-byte windows on the launcher-private copy with equal-length Pop/Nop sequences. Gate C reopens with Cecil and proves 10->0 PrepareMethod references, exact padded semantics, unchanged metadata, and no byte changes outside the approved windows. No real-StS2 CLR load occurs in this build.";

    public const string InitialStatus =
        "Status: Step 31 remains CLOSED POSITIVE 4/4. Physical 0.0.117 reached Step-32 Gate A PASS then failed closed before writing because the bounded Cecil writer discovered another unrelated external constant scope: Sentry 5.0.0.0. Build 0.0.118 is Step 32.0.3: no ModuleDefinition.Write, no dependency resolver expansion, exact-length raw IL patch only. To conserve Codemagic free M2 minutes, run step32-fast first; only if it passes on the exact commit run ios-step-32, and install the IPA only after that device workflow also passes. Preserve both CI summaries plus Step32-RealStS2PrepareMethodRewrite.txt.";

    public const string ExpectedDisplayVersion = "0.0.118";
    public const string ExpectedBuildVersion = "118";
    public const string Step28ImplementationMarker =
        "verified post-publish source -> private clone -> Cecil constant rewrite before CLR load -> reopen/hash verify -> transformed-only private AssemblyLoadContext execution";
    public const string Step29ImplementationMarker =
        "receipt-backed arm64 sts2.dll -> deferred rejecting-resolver Cecil audit -> exact token/IL/target/body fingerprint -> at-most-one audit candidate -> zero writes/zero CLR load -> OfflineReady reproof";
    public const string Step30ImplementationMarker =
        "physical Step29 exact source+token+IL+target+body fingerprint -> deferred rejecting-resolver semantic context audit -> mod-path disposition -> zero writes/zero CLR load -> OfflineReady reproof";
    public const string Step31ImplementationMarker =
        "physical Step29 PrewarmJit token+body fingerprint+10 PrepareMethod offsets -> deferred rejecting-resolver per-site semantic context audit -> rewrite-design eligibility only -> zero writes/zero CLR load -> OfflineReady reproof";
    public const string Step32ImplementationMarker =
        "physical Step31 exact PrewarmJit evidence -> private sts2.dll clone -> deferred rejecting-resolver Cecil bind -> verify ten raw 5-byte call opcode+token windows -> equal-length Pop/Nop byte patch only -> no Cecil serialization -> reopen semantic + byte-diff + constant-metadata verification -> zero CLR load -> OfflineReady reproof";

    // Historical Step-27 crash-report provenance markers remain available as regression/evidence tooling.
    public const string GateSImplementationMarker =
        "bounded HarmonyMethod() descriptor; PatchProcessor.AddPrefix(MethodInfo) runtime invocation forbidden";
    public const string GateTImplementationMarker =
        "Gate-A raw PE method-body normalized HarmonySharedState cctor; post-publish interpreted Target+Prefix fixture; fresh processor via Harmony.CreateProcessor(MethodBase); exactly one Patch() and conditional exact Unpatch() decision boundary";

    public static string DisplayVersion =>
        NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleShortVersionString")?.ToString() ?? "unknown";

    public static string DisplayBuild =>
        NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleVersion")?.ToString() ?? "unknown";

    public static bool BundleIdentityMatchesExpected =>
        string.Equals(DisplayVersion, ExpectedDisplayVersion, StringComparison.Ordinal) &&
        string.Equals(DisplayBuild, ExpectedBuildVersion, StringComparison.Ordinal);
}
