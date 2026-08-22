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
        "STEP 27.0.6 — BOUNDED iOS HARMONY PREFIX-DESCRIPTOR REGISTRATION";

    public const string MilestoneLine =
        "STEPS 01–26 PHYSICALLY CLOSED • 0.0.89 HARD CRASH LOCALIZED INSIDE ADDPREFIX • PATCH() STILL NOT REACHED";

    public const string Summary =
        "Physical 0.0.89 crash telemetry localized the abrupt iOS termination to Gate S after S1: the process entered exact PatchProcessor.AddPrefix(MethodInfo) reflection invocation and disappeared before S2. Harmony's audited AddPrefix body is only new HarmonyMethod(fixMethod) -> PatchProcessor.prefix -> return this; HarmonyMethod(MethodInfo) then enters ImportMethod to inspect Harmony annotations. Step 27.0.6 keeps the same 26-gate launcher-only patch objective but avoids that crashing convenience wrapper for the deliberately annotation-free launcher prefix. Gate S instead constructs the exact metadata-verified HarmonyMethod() default descriptor, verifies priority=-1/method=null, assigns only the exact launcher Prefix MethodInfo, then assigns only PatchProcessor.prefix. PatchProcessor.Patch() remains Gate T and is unchanged. No StS2 member is reflected, patched, or invoked.";

    public const string InitialStatus =
        "Status: Steps 01–26 are physically closed. Build 0.0.84 remains the furthest clean Step-27 execution evidence at A–Q. Build 0.0.89 added durable crash telemetry and physically localized the hard crash to Gate S1 inside AddPrefix(MethodInfo), after the explicit AccessTools boundary. Build 0.0.90 is the bounded descriptor-registration candidate. Run Step 27 only after a force-quit/relaunch. Once Gate B starts, force-quit before every retry regardless of where the run stops. If the app terminates without a managed report, preserve Documents/StS2Launcher/Reports/Step27-CrashCheckpoint.txt before another attempt.";

    public static string DisplayVersion =>
        NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleShortVersionString")?.ToString() ?? "unknown";
}
