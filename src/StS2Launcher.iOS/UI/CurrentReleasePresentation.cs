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
        "STEP 27.0.12 — CECIL OPCODES COMPILE HARDENING";

    public const string MilestoneLine =
        "STEPS 01–26 PHYSICALLY CLOSED • 0.0.95 STOPPED AT HOST COMPILATION • IOS-SAFE HARMONYSHAREDSTATE NORMALIZATION RETAINED";

    public const string Summary =
        "Codemagic proved 0.0.95 never reached runtime: host compilation stopped with CS0104 because the new Cecil cctor normalizer used bare OpCodes while both System.Reflection.Emit and Mono.Cecil.Cil were imported. Step 27.0.12 is a compile-only hardening of the same bounded compatibility fix: the eleven generated initializer instructions now use the explicit CecilOpCodes alias. Gate A still rewrites only HarmonySharedState::.cctor in an in-memory runtime image of the verified prepared 0Harmony 2.4.2 assembly; prepared/source/live files remain untouched; and PatchProcessor.Patch() remains forbidden until the normalized initializer returns and T6 validates. No StS2 member is reflected, patched, or invoked.";

    public const string InitialStatus =
        "Status: Steps 01–26 are physically closed. Physical 0.0.94 remains the latest device evidence: the original HarmonySharedState cctor still stops before T6 after netstandard resolution succeeds. Build 0.0.95 did not run on-device because host compilation failed on an ambiguous OpCodes symbol in the new normalizer. Build 0.0.96 retains the exact 11-instruction AOT-normalized cctor behavior and explicitly binds those generated instructions to Mono.Cecil.Cil.OpCodes via CecilOpCodes. Gate B still loads only the byte-distinct normalized image after re-verifying the untouched prepared SHA, and Gate T runs/validates it before any PatchProcessor.Patch(). Force-quit/relaunch before every Step-27 retry once Gate B has started.";

    public const string ExpectedDisplayVersion = "0.0.96";
    public const string ExpectedBuildVersion = "96";
    public const string GateSImplementationMarker =
        "bounded HarmonyMethod() descriptor; PatchProcessor.AddPrefix(MethodInfo) runtime invocation forbidden";
    public const string GateTImplementationMarker =
        "Gate-A 11-instruction iOS-normalized HarmonySharedState cctor loaded from memory; T5b RunClassConstructor uses direct state only; PatchProcessor.Patch() remains after T6";

    public static string DisplayVersion =>
        NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleShortVersionString")?.ToString() ?? "unknown";

    public static string DisplayBuild =>
        NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleVersion")?.ToString() ?? "unknown";

    public static bool BundleIdentityMatchesExpected =>
        string.Equals(DisplayVersion, ExpectedDisplayVersion, StringComparison.Ordinal) &&
        string.Equals(DisplayBuild, ExpectedBuildVersion, StringComparison.Ordinal);
}
