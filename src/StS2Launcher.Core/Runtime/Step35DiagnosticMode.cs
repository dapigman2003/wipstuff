namespace StS2Launcher.Core;

/// <summary>
/// The first four modes are diagnostic derivatives. GodotCoreExactClosure is the explicit exact-authority closure candidate.
/// NaturalGodotDictionaryRecon preserves the original Godot dictionary/native path as the physical control.
/// ManagedDictionaryCompatibility keeps the proven four-reference BCL dictionary substitution while leaving
/// Godot.OS.GetCmdlineArgs natural.
/// ManagedCommandLineCompatibility adds one bounded substitution for that already-localized command-line
/// provider and returns an empty managed string array.
/// GodotCoreCallbackHandoff preserves the natural sts2/GodotSharp callsites but is intentionally run only after
/// the already-proven embedded Step-15 smoke engine is live; it feeds the exact source-built Godot runtime
/// interop callback table into the separately verified GodotSharp diagnostic derivative before ExecuteVeryEarly.
/// GodotCoreExactClosure is the exact-authority closure candidate: it admits the exact closed Step-32 transformed
/// sts2 bytes and the exact prepared GodotSharp bytes, reproduces the same physically proven bridge bootstrap,
/// then invokes the exact transformed ExecuteVeryEarly once. It must run in a fresh process after Step 15 and does
/// not use either diagnostic derivative as CLR input.
/// </summary>
public enum Step35DiagnosticMode
{
    NaturalGodotDictionaryRecon = 0,
    ManagedDictionaryCompatibility = 1,
    ManagedCommandLineCompatibility = 2,
    GodotCoreCallbackHandoff = 3,
    GodotCoreExactClosure = 4,
}
