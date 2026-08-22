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
        "STEP 27.0.4 — CONTROLLED LAUNCHER-OWNED HARMONY PATCH + UNPATCH";

    public const string MilestoneLine =
        "STEPS 01–26 PHYSICALLY CLOSED • STEP 27 ACCESSTOOLS OPERAND ATTRIBUTION CORRECTION • STS2 REMAINS UNTOUCHED";

    public const string Summary =
        "Physical 0.0.87 stopped safely at Gate O before AccessTools initialization or patching. The 57-instruction fingerprint matched, but the phone disproved the prior attribution of the single ldc.i4.1: both RuntimeInformation Type.GetType(string, bool) probes use throwOnError=false, while ldc.i4.1 supplies LockRecursionPolicy.SupportsRecursion to ReaderWriterLockSlim. Step 27.0.4 pins those exact semantics, preserves the already-bounded reflected framework surface, then keeps Gate R as explicit AccessTools initialization, Gate S as prefix registration, and Gate T as the first PatchProcessor.Patch() boundary. No StS2 member is reflected, patched, or invoked.";

    public const string InitialStatus =
        "Status: Steps 01–26 are physically closed. Step 27.0 reached A–Q before implicit AccessTools initialization failed; Step 27.0.1–27.0.3 then stopped safely at Gate O while measuring and tightening that initializer. Step 27.0.4 is the active operand-attribution correction. Run Step 27 only in a fresh process. If Gate T or later runs, force-quit before retrying. Long reports are written to Files under Documents/StS2Launcher/Reports.";

    public static string DisplayVersion =>
        NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleShortVersionString")?.ToString() ?? "unknown";
}
