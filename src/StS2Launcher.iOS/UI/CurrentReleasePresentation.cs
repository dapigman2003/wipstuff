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
        "STEP 27.0.18 — OFFICIAL NET9 HARMONY-FAT NORMALIZER SURROGATE";

    public const string MilestoneLine =
        "STEPS 01–26 PHYSICALLY CLOSED • 0.0.101 PROVED NO NETSTANDARD FAT BINARY • PRODUCTION NORMALIZER UNCHANGED • OFFICIAL NET9 STRUCTURAL CI SURROGATE";

    public const string Summary =
        "Physical 0.0.97 exposed the Mono.Cecil ReadingMode.Immediate regression and 0.0.98 corrected production normalization to Deferred reads. Builds 0.0.98–0.0.101 hardened the real-Harmony CI gate. Codemagic 0.0.101 downloaded the official Harmony-Fat.2.4.2.0 archive and printed every implementation member, proving that this release contains netcoreapp3.x, net5.0–net10.0, and .NET Framework binaries but no netstandard2.0 implementation. Step 27.0.18 therefore uses the official merged net9.0/0Harmony.dll only as a host structural surrogate for the unchanged production normalizer; on-device exact 0Harmony 2.4.2 metadata remains the production authority. Production Deferred-Cecil normalization, the exact 11-instruction HarmonySharedState cctor image, and Gates S/T are unchanged. No StS2 member is reflected, patched, or invoked.";

    public const string InitialStatus =
        "Status: Steps 01–26 are physically closed. Physical 0.0.94 localized the original HarmonySharedState cctor crash before T6; physical 0.0.97 exposed eager Cecil metadata reading before the normalized image could be tested. Codemagic 0.0.101 proved the official Harmony-Fat 2.4.2 release has no netstandard2.0 implementation, so build 0.0.102 uses its exact merged net9.0 implementation as a clearly labeled host-only structural surrogate while leaving production runtime code unchanged. If normalization reaches T6 on-device, the next evidence boundary is the single public PatchProcessor.Patch() call at T7; if that runtime-detour boundary fails, the documented next experiment is one interpreted post-publish launcher-owned probe before any architecture pivot.";

    public const string ExpectedDisplayVersion = "0.0.102";
    public const string ExpectedBuildVersion = "102";
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
