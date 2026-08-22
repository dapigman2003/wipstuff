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
        "STEP 27.0.5 — CRASH-LOCALIZED LAUNCHER-OWNED HARMONY PATCH + UNPATCH";

    public const string MilestoneLine =
        "STEPS 01–26 PHYSICALLY CLOSED • STEP 27 N–T CRASH LOCALIZATION + CLEANER GATE O • STS2 REMAINS UNTOUCHED";

    public const string Summary =
        "Physical 0.0.88 produced unstable process termination around the N–Q region, plus one expected Gate-A fresh-process rejection when the prior Step-27 load context was still resident. Step 27.0.5 keeps the same 26-gate launcher-only patch objective but makes crash attribution durable: Step27-CrashCheckpoint.txt is synchronously flushed at every gate transition and sensitive O/R/S/T substage. Gate O now performs admission/resolution only and no longer invokes RuntimeInformation.FrameworkDescription through PropertyInfo.GetValue; Gate R owns that first reflected getter invocation immediately before the explicit AccessTools initializer. No StS2 member is reflected, patched, or invoked.";

    public const string InitialStatus =
        "Status: Steps 01–26 are physically closed. Build 0.0.84 remains the furthest clean Step-27 execution evidence at A–Q; 0.0.85–0.0.87 refined the AccessTools metadata fingerprint. Physical 0.0.88 showed intermittent hard termination around N–Q, so 0.0.89 is the crash-localization candidate. Run Step 27 only after a force-quit/relaunch. Once Gate B starts, force-quit before every retry regardless of where the run stops. If the app terminates without a managed report, preserve Documents/StS2Launcher/Reports/Step27-CrashCheckpoint.txt before another attempt.";

    public static string DisplayVersion =>
        NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleShortVersionString")?.ToString() ?? "unknown";
}
