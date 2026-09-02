namespace StS2Launcher.Core;

/// <summary>
/// All modes are diagnostic derivatives and can never close exact Step 35.
/// NaturalGodotDictionaryRecon preserves the original Godot dictionary/native path as the physical control.
/// ManagedDictionaryCompatibility keeps the proven four-reference BCL dictionary substitution while leaving
/// Godot.OS.GetCmdlineArgs natural.
/// ManagedCommandLineCompatibility adds one bounded substitution for that already-localized command-line
/// provider and returns an empty managed string array.
/// GodotCoreCallbackHandoff preserves the natural sts2/GodotSharp callsites but is intentionally run only after
/// the already-proven embedded Step-15 smoke engine is live; it feeds the exact source-built Godot runtime
/// interop callback table into the separately verified GodotSharp diagnostic derivative before ExecuteVeryEarly.
/// </summary>
public enum Step35DiagnosticMode
{
    NaturalGodotDictionaryRecon = 0,
    ManagedDictionaryCompatibility = 1,
    ManagedCommandLineCompatibility = 2,
    GodotCoreCallbackHandoff = 3,
}
