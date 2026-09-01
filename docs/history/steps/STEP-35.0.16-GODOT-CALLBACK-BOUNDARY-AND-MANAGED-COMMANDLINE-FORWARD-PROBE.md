# Step 35.0.16 — Godot callback boundary + managed command-line forward probe

Candidate: `0.0.139 (139)`.

## Why this candidate exists

Physical 0.0.138 produced two fresh-process diagnostic runs. NATURAL refined the previously known Godot Dictionary constructor failure into `NativeFuncs::godotsharp_dictionary_new(...)`, ending after `CustomUnsafe::AsPointer`. COMPAT physically proved that the four-reference BCL Dictionary substitution works, then entered `Godot.OS::.cctor()` and terminated before `Godot.OS.GetCmdlineArgs()` itself entered.

The accompanying static GodotSharp map shows both paths converge on `NativeFuncs._unmanagedCallbacks` function-pointer thunks. Step 35 intentionally forbids Godot bootstrap and native loading, so 0.0.139 does not attempt to manufacture callback state.

## Three fresh-process modes

All modes remain diagnostic derivatives and require a force-quit/relaunch between runs.

- `NaturalGodotDictionaryRecon`: preserves the natural Godot Dictionary and `Godot.OS` path. It is retained as a control; rerunning it is optional unless regression confirmation is needed.
- `ManagedDictionaryCompatibility` (UI: OS-RECON): applies exactly the already-proven four substitutions affecting `CommandLineHelper._args`, Dictionary constructor, `set_Item`, and `TryGetValue`; `Godot.OS.GetCmdlineArgs()` remains natural. The GodotSharp entry-marker graph now explicitly roots local closure discovery at `Godot.OS::.cctor()` and `Godot.OS/MethodName::.cctor()` and includes StringName/ClassDB/NativeFuncs callback-boundary methods.
- `ManagedCommandLineCompatibility` (UI: FORWARD): applies the same four Dictionary substitutions and replaces exactly one `Godot.OS.GetCmdlineArgs()` call site in `CommandLineHelper..cctor` with `StS2Launcher.Step35Diagnostics.ExecuteVeryEarlyCheckpointBridge.GetManagedCommandLineArgsCompatibility()`. That local method is verified after Cecil serialization to be exactly `ldc.i4.0; newarr System.String; ret`.

## Why empty args are bounded

The FORWARD provider is intentionally not a general Godot emulation layer. It substitutes only the already-localized command-line dependency and returns no command-line options. This is sufficient to measure the next startup boundary without claiming that empty arguments are the final launcher behavior.

## Verification requirements

Gate A must fail closed unless:

- the exact closed Step-32 transformed source is recreated/reverified and remains hash-identical after derivative creation;
- NATURAL retains the original Godot Dictionary and exactly one natural `Godot.OS.GetCmdlineArgs()` call;
- OS-RECON performs exactly four managed Dictionary substitutions and retains exactly one natural `Godot.OS.GetCmdlineArgs()` call;
- FORWARD performs exactly four managed Dictionary substitutions plus exactly one managed command-line provider substitution, retains zero natural `Godot.OS.GetCmdlineArgs()` calls in `CommandLineHelper..cctor`, and contains exactly one call to the verified local provider;
- the separate GodotSharp derivative preserves identity/MVID and every serialized GS marker calls the GodotSharp diagnostic bridge;
- CommandLine critical markers remain stack-neutral and serialized cctor MaxStack remains unchanged;
- no native image is loaded/executed by reconnaissance; runtime native resolution remains rejected;
- no Godot bootstrap, later OneTimeInitialization phase, entry point, Harmony/MonoMod patching or arbitrary resolver fallback is introduced.

A 4/4 result in any mode is diagnostic evidence only. Exact Step 35 remains open until an explicitly authoritative closure candidate executes the intended exact transformed artifact under a separately defined compatibility contract.
