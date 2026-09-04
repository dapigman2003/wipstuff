# Step 35.0.27 — Exact-authority closure candidate + Gate-D finalization

Release: 0.0.150 (150)

## Motivation

Physical 0.0.149 completed diagnostic Gate C end-to-end: the diagnostic ExecuteVeryEarly MethodInfo.Invoke returned, the Task was RanToCompletion, the bounded await returned, post-await resolver/native confinement passed, and Gate C was recorded PASS. The device UI later displayed Gate-D terminal 4/4 final-check progress but the durable journal remained at D_START for an extended interval. Because those terminal progress events were not durable checkpoints, 0.0.149 is not promoted to a formal Gate-D PASS.

The compatibility question is therefore no longer served by another diagnostic callsite probe. The next authority question is whether the exact closed transformed Step-32 sts2 artifact itself can execute ExecuteVeryEarly under the now physically proven Godot bridge prerequisite.

## Gate-D finalization correction

Gate D now writes durable checkpoints from inside the core audit around OfflineReady return, selected-primary reproof, final isolation checks, result construction, terminal progress emission and Task return. The UI separately records Gate-D await resumption and result recording. The terminal 4/4 progress event is emitted only after the Gate-D result object has already been constructed.

Gate-D progress updates remain on the compact progress controls and no longer rebuild the large historical detail label for each receipt verifier event. After terminal final checks the heartbeat switches from “last verifier progress” to an explicit finalization wait and warns after ten seconds while the durable D_* checkpoints identify the exact boundary.

## Exact-authority mode

`Step35DiagnosticMode.GodotCoreExactClosure` is an explicit fifth mode and the only Step-35.0.27 mode intended as an exact-authority closure candidate.

The mode requires a fresh launcher process followed by successful Step-15 Gates A–C in the same process. It then:

1. Re-manufactures and re-verifies the closed Step-32 transformed artifact and all existing Gate-A static/reconnaissance evidence.
2. Gate B CLR-admits the exact closed Step-32 transformed sts2 bytes, not the Step-35 diagnostic sts2 clone.
3. The strict Step-35 load context has no GodotSharp diagnostic override, so it resolves the exact hash-pinned prepared GodotSharp bytes.
4. It reproduces the physically proven 1,800-byte / 225-pointer managed->native handoff, complete 37-pointer / 296-byte ManagedCallbacks creation, script lookup, GDMonoCache adoption, GD_OnCoreApiAssemblyLoaded return, and exact eight-request/eight-host-load/zero-private bootstrap resolver seal.
5. Gate C binds the exact transformed ExecuteVeryEarly token and MVID, proves the diagnostic checkpoint bridge type is absent, invokes the exact method once, and awaits its Task under the unchanged <=60-second boundary.
6. Gate D re-proves OfflineReady, receipt-backed source isolation, exact transformed hash, runtime-binding plan, loaded prepared dependency hashes, resolver/native confinement, unique sts2 residency and exact method token.

## Authority limits

The source-built Godot 4.5.1 callback producer is a project-owned same-version launcher prerequisite whose physical compatibility has already been demonstrated across managed->native and native->managed Godot APIs. It is not represented as the exact game's native executable callback producer.

The receipt-backed original game sts2.dll is never CLR-loaded. The exact Step-32 transformed artifact is the Step-35 authority. The game native executable is not loaded. ExecuteEssential, ExecuteDeferred, PrewarmJit, the game entry point, Harmony/MonoMod runtime patching, arbitrary resolver fallback and broader game startup remain forbidden.

A successful physical EXACT-CLOSURE A–D 4/4 is the Step-35 closure candidate under this explicitly defined launcher-host bridge prerequisite. Diagnostic-mode 4/4 results remain diagnostic evidence only.
