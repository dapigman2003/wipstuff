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
        "STEP 27.0.7 — HARMONY SHARED-STATE INITIALIZATION + PATCH-ENGINE PRESERVATION";

    public const string MilestoneLine =
        "STEPS 01–26 PHYSICALLY CLOSED • 0.0.90 REACHED PATCH() T1 • CRASH LOCALIZED INSIDE PATCH ENGINE";

    public const string Summary =
        "Physical 0.0.90 crash telemetry proves the bounded HarmonyMethod descriptor path reached Gate T and then hard-terminated after T1 while inside the first exact PatchProcessor.Patch() invocation, before the launcher target was invoked. Exact Harmony 2.4.2 source and Gate-O receipt-backed metadata show Patch() first enters HarmonySharedState.GetPatchInfo; HarmonySharedState::.cctor creates a dynamic singleton and on Mono can generate a FieldRef delegate before replacement generation and the MonoMod detour path. Step 27.0.7 keeps A–S and the public Patch() acceptance boundary intact, adds narrowly scoped framework preservation for the audited dynamic-code surface, and makes HarmonySharedState initialization an explicit T1/T2 boundary before the single Patch() call at T3/T4. T5 validates the returned replacement and isolation. No StS2 member is reflected, patched, or invoked.";

    public const string InitialStatus =
        "Status: Steps 01–26 are physically closed. Build 0.0.89 localized the earlier hard crash to AddPrefix(MethodInfo). Build 0.0.90 bypassed only that annotation-import convenience path and physically advanced through Gate S into Gate T/T1, where the process hard-terminated inside PatchProcessor.Patch() before T2 and before any launcher target invocation. Build 0.0.91 is the bounded shared-state/patch-engine preservation candidate. Run Step 27 only after a force-quit/relaunch. Once Gate B starts, force-quit before every retry regardless of where the run stops. If the app terminates without a managed report, preserve Documents/StS2Launcher/Reports/Step27-CrashCheckpoint.txt before another attempt.";

    public static string DisplayVersion =>
        NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleShortVersionString")?.ToString() ?? "unknown";
}
