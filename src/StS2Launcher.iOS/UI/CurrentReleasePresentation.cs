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
        "STEP 27.0.8 — GATE-O PURITY RESTORATION + PATCH-ENGINE RUNTIME RESOLUTION";

    public const string MilestoneLine =
        "STEPS 01–26 PHYSICALLY CLOSED • 0.0.91 FAILED CLEANLY AT GATE O • NEW RUNTIME REFLECTION MOVED TO GATE T";

    public const string Summary =
        "Physical 0.0.91 replayed A–N but failed cleanly at Gate O because the newly added HarmonySharedState runtime reflection changed resolver/load counters. That means 0.0.91 never reached the former Gate-T crash boundary. Step 27.0.8 restores Gate O to the physically passing 0.0.90 runtime-reflection surface while retaining the broader HarmonySharedState -> MethodCreator -> MonoMod detour chain as Cecil metadata audit only. Gate T now measures the new host Reflection.Emit/MethodHandle preflight at T1/T2, the exact HarmonySharedState runtime Type/.cctor/version reflection at T3/T4, explicit HarmonySharedState initialization at T5/T6, and exactly one public PatchProcessor.Patch() at T7/T8 with T9 validation. No StS2 member is reflected, patched, or invoked.";

    public const string InitialStatus =
        "Status: Steps 01–26 are physically closed. Build 0.0.90 reached Gate T and hard-terminated inside PatchProcessor.Patch(). Build 0.0.91 did not crash, but it failed normally at Gate O / 14 of 26 because the expanded HarmonySharedState runtime reflection changed resolver/load counters before Gate T. Build 0.0.92 restores Gate-O runtime purity and moves every newly introduced runtime-reflection/preflight operation into measured Gate-T substages before retrying the exact public Patch() boundary. Run Step 27 only after a force-quit/relaunch. Once Gate B starts, force-quit before every retry regardless of where the run stops. If the app terminates without a managed report, preserve Documents/StS2Launcher/Reports/Step27-CrashCheckpoint.txt before another attempt.";

    public static string DisplayVersion =>
        NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleShortVersionString")?.ToString() ?? "unknown";
}
