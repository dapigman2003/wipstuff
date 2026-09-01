namespace StS2Launcher.Core;

/// <summary>
/// All modes are diagnostic derivatives and can never close exact Step 35.
/// NaturalGodotDictionaryRecon preserves the original Godot dictionary/native path as the 0.0.138 control.
/// ManagedDictionaryCompatibility keeps the proven four-reference BCL dictionary substitution while leaving
/// Godot.OS.GetCmdlineArgs natural; 0.0.138 physically reached Godot.OS..cctor in this mode.
/// ManagedCommandLineCompatibility adds one bounded substitution for that already-localized command-line
/// provider and returns an empty managed string array, allowing the next non-Godot startup frontier to be measured
/// without manufacturing Godot native callback state.
/// </summary>
public enum Step35DiagnosticMode
{
    NaturalGodotDictionaryRecon = 0,
    ManagedDictionaryCompatibility = 1,
    ManagedCommandLineCompatibility = 2,
}
