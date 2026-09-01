namespace StS2Launcher.Core;

/// <summary>
/// Both modes are diagnostic derivatives and can never close exact Step 35.
/// NaturalGodotDictionaryRecon preserves the original CommandLineHelper Godot dictionary so the
/// instrumented GodotSharp clone can localize the physically proven constructor failure.
/// ManagedDictionaryCompatibility retains the bounded dictionary substitution introduced in the 0.0.137 pre-device candidate and carried into 0.0.138 so the same
/// app build can advance to the next Godot boundary after a fresh-process relaunch.
/// </summary>
public enum Step35DiagnosticMode
{
    NaturalGodotDictionaryRecon = 0,
    ManagedDictionaryCompatibility = 1,
}
