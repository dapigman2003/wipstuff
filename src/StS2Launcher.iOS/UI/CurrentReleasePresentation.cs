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
        "STEP 27.0.24 — SINGLE POST-PUBLISH INTERPRETED HARMONY DECISION EXPERIMENT";

    public const string MilestoneLine =
        "STEPS 01–26 PHYSICALLY CLOSED • 0.0.107 REMOVED TRIM AMBIGUITY • PATCH() NOW THROWS NOTIMPLEMENTED • ONE INTERPRETED FIXTURE TRIAL";

    public const string Summary =
        "Physical 0.0.107 proved the copy/no-link host policy removed the prior LINQ and DebuggableAttribute trimming blockers, while the normalized HarmonySharedState boundary remained physically viable. The one public PatchProcessor.Patch() call then threw System.NotImplementedException inside PatchFunctions.UpdateWrapper, but that stack does not distinguish replacement generation from detour installation. Step 27.0.24 therefore performs the single stop-rule experiment against a launcher-owned DLL copied into the app only after dotnet publish: both Target and Prefix are post-publish interpreted IL, a fresh PatchProcessor is created for that exact target via public Harmony.CreateProcessor(MethodBase), Patch() is invoked once, and—only if patching succeeds—the exact prefix is unpatched once and original behavior must return. If this representative interpreted fixture cannot patch, Step 27 stops iterating Harmony internals and Step 28 pivots to ahead-of-load managed IL transformation. No StS2 member is reflected, patched, or invoked.";

    public const string InitialStatus =
        "Status: Steps 01–26 are physically closed. Physical 0.0.107 eliminated the known publish-time trimming ambiguity and reached the real PatchProcessor.Patch() boundary, which now throws NotImplementedException from PatchFunctions.UpdateWrapper. Build 0.0.108 does not patch Harmony or force a MonoMod backend: it moves the exact launcher-owned target/prefix into a DLL copied after publish and creates a fresh processor for that interpreted target. This is the single representative interpreted patch/unpatch decision experiment required by the Step-27 stop rule.";

    public const string ExpectedDisplayVersion = "0.0.108";
    public const string ExpectedBuildVersion = "108";
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
