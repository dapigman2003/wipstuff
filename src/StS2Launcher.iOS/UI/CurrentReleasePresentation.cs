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
        "STEP 27.0.22 — POST-PUBLISH SYSTEM.LINQ FRAMEWORK PRESERVATION";

    public const string MilestoneLine =
        "STEPS 01–26 PHYSICALLY CLOSED • 0.0.105 PROVED RAW HARMONYSHAREDSTATE NORMALIZATION • T7 REACHED • SYSTEM.LINQ MEMBER TRIM IDENTIFIED";

    public const string Summary =
        "Physical 0.0.105 proved the raw PE HarmonySharedState normalization on-device and advanced Gate T into the first public PatchProcessor.Patch() call. Patch() then threw a managed MissingMethodException before replacement generation completed because the post-publish 0Harmony MethodCreator called System.Linq.Enumerable.Union<T>, which full trimming had removed from the host framework surface. Step 27.0.22 treats this as a dynamic-payload linker contract, not a Harmony detour failure: System.Linq is now an explicit TrimmerRootAssembly, and Gate T performs an exact Select/Union/ToDictionary callable-surface preflight after T6 and before T7. The master plan and runtime Harmony normalization remain unchanged; no StS2 member is reflected, patched, or invoked.";

    public const string InitialStatus =
        "Status: Steps 01–26 are physically closed. Physical 0.0.105 reached Gate T7 after the normalized HarmonySharedState cctor returned successfully, then PatchProcessor.Patch() failed in Harmony MethodCreator with MissingMethodException for Enumerable.Union<T>. Build 0.0.106 roots the complete System.Linq framework assembly for post-publish dynamic payload use and adds a pre-Patch LINQ member-closure check. If that closure passes, the next evidence will finally distinguish replacement-generation/dynamic-code behavior from the later MonoMod detour boundary.";

    public const string ExpectedDisplayVersion = "0.0.106";
    public const string ExpectedBuildVersion = "106";
    public const string GateSImplementationMarker =
        "bounded HarmonyMethod() descriptor; PatchProcessor.AddPrefix(MethodInfo) runtime invocation forbidden";
    public const string GateTImplementationMarker =
        "Gate-A raw PE method-body normalized HarmonySharedState cctor; System.Linq rooted for post-publish payload; T6a/T6b Select/Union/ToDictionary closure preflight; PatchProcessor.Patch() remains after T6";

    public static string DisplayVersion =>
        NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleShortVersionString")?.ToString() ?? "unknown";

    public static string DisplayBuild =>
        NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleVersion")?.ToString() ?? "unknown";

    public static bool BundleIdentityMatchesExpected =>
        string.Equals(DisplayVersion, ExpectedDisplayVersion, StringComparison.Ordinal) &&
        string.Equals(DisplayBuild, ExpectedBuildVersion, StringComparison.Ordinal);
}
