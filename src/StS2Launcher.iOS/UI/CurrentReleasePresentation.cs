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
        "STEP 27.0.23 — DYNAMIC PAYLOAD NO-TRIM HOST POLICY";

    public const string MilestoneLine =
        "STEPS 01–26 PHYSICALLY CLOSED • 0.0.106 PASSED LINQ CLOSURE • DYNAMICMETHODDEFINITION REACHED • DEBUGGABLEATTRIBUTE TRIM IDENTIFIED";

    public const string Summary =
        "Physical 0.0.106 proved the System.Linq root fixed the prior Union<T> blocker and advanced the first public PatchProcessor.Patch() call into Harmony MethodPatcherTools.CreateDynamicMethod. MonoMod.Utils.DynamicMethodDefinition then failed type initialization because full trimming had removed System.Diagnostics.DebuggableAttribute from the host framework surface. This is a second independent post-publish BCL trimming failure before PatchTools.DetourMethod. Step 27.0.23 therefore changes the host architecture from full trimming to the macios copy/no-link policy (MtouchLink=None + TrimMode=copy) so the dynamically loaded StS2/Harmony/mod world is not constrained by ILLink's publish-time visibility. Harmony patch execution itself is otherwise unchanged and remains on trial; no StS2 member is reflected, patched, or invoked.";

    public const string InitialStatus =
        "Status: Steps 01–26 are physically closed. Physical 0.0.106 reached PatchProcessor.Patch() after the normalized HarmonySharedState cctor and the exact LINQ closure preflight, then failed in MonoMod.Utils.DynamicMethodDefinition type initialization because DebuggableAttribute had been trimmed from the host framework. Build 0.0.107 disables managed trimming with MtouchLink=None + TrimMode=copy while retaining MtouchInterpreter=-all and the exact same Harmony patch boundary. The next device run should distinguish another true runtime/dynamic-code limitation from the now-removed linker ambiguity.";

    public const string ExpectedDisplayVersion = "0.0.107";
    public const string ExpectedBuildVersion = "107";
    public const string GateSImplementationMarker =
        "bounded HarmonyMethod() descriptor; PatchProcessor.AddPrefix(MethodInfo) runtime invocation forbidden";
    public const string GateTImplementationMarker =
        "Gate-A raw PE method-body normalized HarmonySharedState cctor; host MtouchLink=None + TrimMode=copy dynamic-payload policy; T6a/T6b LINQ closure retained; PatchProcessor.Patch() unchanged after T6";

    public static string DisplayVersion =>
        NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleShortVersionString")?.ToString() ?? "unknown";

    public static string DisplayBuild =>
        NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleVersion")?.ToString() ?? "unknown";

    public static bool BundleIdentityMatchesExpected =>
        string.Equals(DisplayVersion, ExpectedDisplayVersion, StringComparison.Ordinal) &&
        string.Equals(DisplayBuild, ExpectedBuildVersion, StringComparison.Ordinal);
}
