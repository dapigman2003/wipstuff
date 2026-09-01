> **Superseded pre-physical draft.** This narrow 0.0.137 design was replaced before device testing by `STEP-35.0.14-COMPREHENSIVE-GODOT-NATIVE-RECONNAISSANCE.md`. It is retained only as history.

# Step 35.0.14 — Managed CommandLine dictionary compatibility probe

Candidate: `0.0.137 (137)`.

## Evidence entering this step

Physical 0.0.136 removed all live-stack CommandLine runtime sweeps and finally executed `CommandLineHelper..cctor`. The durable sequence reached `INMETHOD_CL_CRITICAL_001_PRE` immediately before the exact-source `newobj Godot.Collections.Dictionary<string,string>::.ctor()` and never reached the matching POST. This localizes the measured interval to Godot string-dictionary construction before `_args` assignment. The last `System.Collections.Concurrent 8 -> 9` host binding remains contextual rather than causal.

## Candidate change

The exact closed Step-32 transformed image remains the authority and is recreated/reverified before any derivative is emitted. The Step-35.0.14 diagnostic clone applies exactly four compatibility substitutions inside `MegaCrit.Sts2.Core.Helpers.CommandLineHelper`:

1. `_args` field type: `Godot.Collections.Dictionary<string,string>` -> `System.Collections.Generic.Dictionary<string,string>`.
2. cctor `newobj`: Godot dictionary constructor -> BCL dictionary constructor.
3. cctor `set_Item`: Godot dictionary -> BCL dictionary, encoded as `set_Item(!0,!1)` on a constructed `Dictionary<string,string>` declaring type.
4. `TryGetValue`: Godot dictionary -> BCL dictionary, encoded as `TryGetValue(!0,!1&)`.

The rewrite reuses the exact existing `System.Collections` AssemblyRef already present in sts2. It does not add a new runtime resolver authority or private dependency. `Godot.OS.GetCmdlineArgs()` remains exactly once and natural.

## Verification

Gate A must fail closed unless all four source substitutions are found exactly once. After Cecil serialization, the diagnostic clone is reopened under rejecting resolution and must prove:

- `_args` has the BCL string-dictionary type scoped to existing `System.Collections`;
- the BCL `.ctor`, `set_Item(!0,!1)`, and `TryGetValue(!0,!1&)` MemberRefs have the expected constructed-generic/VAR signatures;
- no Godot string-dictionary call reference remains in the affected cctor/TryGetValue methods;
- `Godot.OS.GetCmdlineArgs()` remains exactly once;
- CommandLine cctor MaxStack is unchanged;
- the four stack-neutral critical markers and all corrected prior markers remain serialized;
- exact transformed source SHA-256/MVID/semantics remain unchanged.

## Physical interpretation

If `CL_CRITICAL_001_POST` now appears, the managed dictionary substitution passed the 0.0.136 hard-termination interval. If `CL_CRITICAL_002_PRE` appears without its POST, the next natural physical frontier is `Godot.OS.GetCmdlineArgs()`. If `CL_CRITICAL_002_POST` and `INMETHOD_027` appear, CommandLine type initialization completed and the run may advance to the later NullPlatform/GodotFileIo path.

A diagnostic 4/4 cannot close exact Step 35. Godot bootstrap, native game loading, arbitrary resolver fallback, initializer-bearing 0Harmony, later OneTimeInitialization phases, game entry-point execution, and Harmony/MonoMod runtime patching remain forbidden.
