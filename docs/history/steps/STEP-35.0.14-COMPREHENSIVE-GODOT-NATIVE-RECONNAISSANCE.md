# Step 35.0.14 — Comprehensive GodotSharp/native reconnaissance + dual diagnostic mode

Candidate: rebuilt `0.0.137 (137)`.

## Evidence entering this step

Physical 0.0.136 removed all live-stack CommandLine runtime sweeps and finally executed `CommandLineHelper..cctor`. The durable sequence reached `INMETHOD_CL_CRITICAL_001_PRE` immediately before exact-source `newobj Godot.Collections.Dictionary<string,string>::.ctor()` and never reached the matching POST. This localizes the measured hard-termination interval to Godot string-dictionary construction before `_args` assignment. The last `System.Collections.Concurrent 8 -> 9` host binding remains contextual rather than causal.

The first 0.0.137 source draft proposed only a four-reference managed dictionary compatibility substitution. Before physical testing, that draft was superseded by this comprehensive candidate because physical iOS builds are expensive and a one-boundary-per-build strategy was producing too little information.

## Candidate design

The exact closed Step-32 transformed image remains immutable authority and is recreated/reverified before any derivative is emitted. Step 35.0.14 adds three diagnostic layers without broadening execution authority.

### 1. Read-only exact-depot reconnaissance

Before Gate B, inspect the exact OfflineReady depot without loading or executing any native image. The report records:

- exact prepared GodotSharp identity/hash and assembly references;
- selected critical/local GodotSharp IL rooted in `Godot.Collections.Dictionary`2`, `Godot.OS.GetCmdlineArgs`, `Godot.GodotObject.GetPtr`, `Godot.NativeCalls.godot_icall_0_108`, and `Godot.NativeInterop.NativeFuncs.Initialize`;
- P/Invoke declarations, `calli` sites, and `NativeFuncs` / `UnmanagedCallbacks` field-use sites;
- recognized Mach-O images, selected arm64 slice metadata, linked dylibs, rpaths, and bounded interesting symbol/string matches for Godot/.NET/native-bootstrap terms.

The reconnaissance report is output-only and is written durably before CLR admission.

### 2. Entry-only GodotSharp diagnostic derivative

Create `GodotSharp.step35.0.14.instrumented.dll` from the exact prepared GodotSharp image using Cecil Deferred open and a self-auditing writer-only constant-metadata resolver. Preserve assembly identity and MVID. Reopen under rejecting resolution and verify the bridge as ECMA-correct `Action<string>::Invoke(!0)`.

Instrument only method **entries**, never live evaluation stacks. The bounded marker plan includes the dictionary constructor, `OS.GetCmdlineArgs`, `NativeCalls.godot_icall_0_108`, and local callees to bounded depth. This is specifically intended to avoid the 0.0.133/0.0.135 live-stack instrumentation failure family.

At runtime the strict private resolver first re-hashes the exact prepared GodotSharp source, then separately verifies the derivative hash and loads the derivative under the already planned GodotSharp identity. The launcher arms the diagnostic callback before returning the assembly to the requesting game code. Native resolution remains rejected.

### 3. Two modes in one IPA

Each mode requires a fresh process, but both are shipped in the same 0.0.137 app so no rebuild is required between them.

**NATURAL — `NaturalGodotDictionaryRecon`** preserves the original `CommandLineHelper` Godot dictionary. The expected value is inner `INMETHOD_GS...` evidence between `CL_CRITICAL_001_PRE` and the hard termination.

**COMPAT — `ManagedDictionaryCompatibility`** applies exactly four substitutions inside `CommandLineHelper`: `_args` type, dictionary constructor, `set_Item`, and `TryGetValue` become the existing BCL `Dictionary<string,string>` contract using the existing `System.Collections` AssemblyRef and generic VAR MemberRefs. `Godot.OS.GetCmdlineArgs()` remains natural and unmodified.

## Interpretation

NATURAL localizes the original 0.0.136 GodotSharp constructor path. COMPAT tests whether bypassing only that collection boundary advances to `Godot.OS.GetCmdlineArgs()` or another natural path. The same-run reconnaissance file maps all GodotSharp entry markers to exact method signatures and static native-boundary context.

A diagnostic 4/4 in either mode cannot close exact Step 35. No Godot bootstrap, native game loading, arbitrary resolver fallback, later startup phase, game entry point, initializer-bearing `0Harmony`, or runtime Harmony/MonoMod patching is authorized.
