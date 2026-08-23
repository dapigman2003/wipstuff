using Foundation;

namespace StS2Launcher.iOS;

/// <summary>
/// Single source for the release identity shown at the top of the launcher UI.
/// The display version is read from the built bundle so it cannot drift from Info.plist;
/// the candidate step/summary are hash-pinned and statically validated for every release.
/// </summary>
internal static class CurrentReleasePresentation
{
    public const string StepTitle =
        "STEP 27.0.16 — REAL HARMONY FAT RELEASE FIXTURE HARDENING";

    public const string MilestoneLine =
        "STEPS 01–26 PHYSICALLY CLOSED • 0.0.99 FIXTURE ACQUISITION STOP • PRODUCTION NORMALIZER UNCHANGED • OFFICIAL HARMONY-FAT 2.4.2 CI FIXTURE";

    public const string Summary =
        "Physical 0.0.97 exposed the Mono.Cecil ReadingMode.Immediate regression and 0.0.98 corrected production normalization to Deferred reads. Build 0.0.98 then exposed a test-only Cecil namespace collision, fixed in 0.0.99. Codemagic 0.0.99 compiled both production and tests but stopped in the test-project MSBuild fixture-copy target because Lib.Harmony's NuGet package did not expose the assumed lib/netstandard2.0/0Harmony.dll implementation path. Step 27.0.16 removes that package-layout assumption: the canonical host-test script downloads the exact tagged Harmony-Fat.2.4.2.0 release archive, extracts only netstandard2.0/0Harmony.dll into a quarantined artifact folder, and passes its absolute path through STS2_STEP27_REAL_HARMONY_FIXTURE. Production Deferred-Cecil normalization, the exact 11-instruction HarmonySharedState cctor image, and Gates S/T are unchanged. No StS2 member is reflected, patched, or invoked.";

    public const string InitialStatus =
        "Status: Steps 01–26 are physically closed. Physical 0.0.94 localized the original HarmonySharedState cctor crash before T6; physical 0.0.97 exposed eager Cecil metadata reading before the normalized image could be tested. Builds 0.0.98 and 0.0.99 then hardened the real-Harmony CI regression, with 0.0.99 proving source/test compilation but failing before test execution on a brittle NuGet package-path assumption. Build 0.0.100 moves acquisition to the official Harmony-Fat 2.4.2 tagged release asset while leaving production runtime code unchanged. If normalization reaches T6 on-device, the next evidence boundary is the single public PatchProcessor.Patch() call at T7; if that runtime-detour boundary fails, the documented next experiment is one interpreted post-publish launcher-owned probe before any architecture pivot.";

    public const string ExpectedDisplayVersion = "0.0.100";
    public const string ExpectedBuildVersion = "100";
    public const string GateSImplementationMarker =
        "bounded HarmonyMethod() descriptor; PatchProcessor.AddPrefix(MethodInfo) runtime invocation forbidden";
    public const string GateTImplementationMarker =
        "Gate-A 11-instruction iOS-normalized HarmonySharedState cctor loaded from memory; deferred Cecil source/audit; T5b RunClassConstructor uses direct state only; PatchProcessor.Patch() remains after T6";

    public static string DisplayVersion =>
        NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleShortVersionString")?.ToString() ?? "unknown";

    public static string DisplayBuild =>
        NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleVersion")?.ToString() ?? "unknown";

    public static bool BundleIdentityMatchesExpected =>
        string.Equals(DisplayVersion, ExpectedDisplayVersion, StringComparison.Ordinal) &&
        string.Equals(DisplayBuild, ExpectedBuildVersion, StringComparison.Ordinal);
}
