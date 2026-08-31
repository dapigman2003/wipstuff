# Step 35.0.12 — MaxStack-safe CommandLine / Godot boundary localization

Candidate: `0.0.135 (135)`.

## Why this exists

Physical 0.0.133 corrected the NullPlatform CALLSITE ordinal but the new CommandLineHelper cctor instrumentation was rejected with managed `InvalidProgramException` before any cctor marker executed. The launcher returned normally and wrote `RUN_END`, so this is a diagnostic-IL defect rather than a new exact-game frontier. The generic PRE/POST sweep can add one transient stack item while original call arguments or a return value remain live; 0.0.133 did not raise the method header `MaxStack`.

The authoritative game frontier therefore remains physical 0.0.132: `NullPlatformUtilStrategy..ctor` invokes `CommandLineHelper.TryGetValue`, which triggers type initialization and hard-terminates before returning.

## 0.0.135 diagnostic changes

The exact transformed source, runtime resolver plan, timeout, fresh-process rules, native prohibition, initializer-bearing dependency prohibition, Godot-startup prohibition, later-boundary prohibition, and diagnostic-clone-only execution model remain unchanged.

0.0.135 changes only output-only instrumentation and its validation:

1. every generic diagnostic callsite sweep reserves one additional `MaxStack` slot because PRE/POST markers may execute with original stack values live;
2. the existing targeted callsite-marker helper is hardened the same way for future deeper GodotFileIo reachability;
3. entry and critical stack-neutral markers require at least one stack slot but do not otherwise inflate MaxStack;
4. Gate A captures the exact-source `CommandLineHelper..cctor` MaxStack, requires diagnostic MaxStack to be exactly source+1 after the CL sweep, serializes the clone, reopens it, and requires the same header value after write;
5. four redundant stack-neutral critical markers are placed only where the original evaluation stack is empty:
   - before `_args` dictionary construction;
   - after the dictionary value is assigned to `_args`;
   - before `Godot.OS.GetCmdlineArgs()`;
   - after the returned `string[]` is stored into its local;
6. the normal `INMETHOD_CLxxx_PRE/POST` and `INMETHOD_CLTVxxx_PRE/POST` sweeps remain, as do corrected `INMETHOD_NPxxx_PRE/POST` ordinals;
7. the exact-source static map now prints the CommandLine cctor MaxStack alongside its IL/callsite map;
8. host regression coverage includes a real CLR execution test: a generated cctor with declared `MaxStack=3` calls a three-argument method, is instrumented, must serialize as `MaxStack=4`, then is loaded and executed. This is intended to reproduce the 0.0.133 failure mode if the MaxStack correction is removed.

## Expected physical interpretation

The redundant markers are designed to make one device build useful even if a later generic callsite marker has another problem.

- no CommandLine cctor entry marker: CLR/JIT/type-initializer admission failed before instruction zero;
- cctor entry but no critical dictionary PRE: failure immediately after cctor entry instrumentation;
- dictionary PRE without dictionary POST: failure during `Godot.Collections.Dictionary<string,string>` construction/assignment;
- dictionary POST then GetCmdlineArgs PRE without GetCmdlineArgs POST: physical localization to `Godot.OS.GetCmdlineArgs()`;
- GetCmdlineArgs POST: the Godot command-line call returned and subsequent CL markers localize later parser work;
- cctor completes and `INMETHOD_027` appears: type initialization returned and the actual `TryGetValue` body entered.

A diagnostic 4/4 remains localization evidence only and cannot close exact Step 35.
