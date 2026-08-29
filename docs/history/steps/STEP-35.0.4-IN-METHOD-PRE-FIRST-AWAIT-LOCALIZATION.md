# Step 35.0.4 — In-Method Pre-First-Await Localization

Candidate: **0.0.127 (127)**

## Basis
Physical 0.0.126 produced trustworthy same-run evidence and ended after the final planned framework resolution while `MethodInfo.Invoke` still had not returned. The exact static map limits the synchronous first-entry path to `TestMode`, save/settings initialization, mod file/settings/version preparation, `ModManager.Initialize`, and the first await machinery.

## Change
After re-running and verifying the closed Step-32 transformed source, Gate A writes a separate diagnostic clone. The source is never overwritten. The clone preserves assembly identity and MVID, adds `StS2Launcher.Step35Diagnostics.ExecuteVeryEarlyCheckpointBridge`, and injects output-only entry markers into `ExecuteVeryEarly.MoveNext`, the selected top-level pre-first-await callees, and any relevant static constructors present on those declaring types.

Gate B admits only the diagnostic clone under the existing strict resolver. Gate C binds the same static parameterless Task-returning `ExecuteVeryEarly`, arms the bridge `Action<string>` callback to the durable journal writer, then performs the one reflected Invoke/await. The last durable `INMETHOD_*` marker localizes the method/type-initializer frontier.

## Non-authority
This diagnostic clone does not close Step 35 and does not authorize `ExecuteEssential`, `ExecuteDeferred`, game entry, Harmony/MonoMod patching, native game loading, or Godot/game startup.

## Evidence semantics and acceptance

The exact closed Step-32 transformed source must be re-hashed immediately after clone emission and again before Gate-B CLR admission. Gate B/C may load/execute only the diagnostic clone. The active gate enum/report/UI must identify Gate C as diagnostic invocation rather than exact transformed execution.

Possible physical outcomes:

- hard termination after `C_DIAGNOSTIC_BRIDGE_ARMED` with an `INMETHOD_*` tail: localize to the final marked game method/type initializer;
- hard termination after bridge arm but before any `INMETHOD_*`: localize to bridge/first instrumented entry;
- managed diagnostic FAIL: preserve the managed failure and resolver state, but do not treat it as exact Step-35 closure evidence;
- diagnostic A–D 4/4: useful proof that the instrumented derivative survives, but **Step 35 remains OPEN**.

No 0.0.127 result may be promoted to exact Step-35 closure because the executed image is not byte-identical to the closed transformed SHA-256.

## WIP hardening review before candidate packaging

The inherited partial 0.0.127 implementation was reviewed against the 0.0.126 source and same-run physical reports before packaging. The review made three source-level corrections:

1. Active Gate-C/report/UI wording was changed so a diagnostic-clone 4/4 result cannot be presented as exact Step-35 closure.
2. The exact closed transformed source is re-hashed immediately after diagnostic-clone emission, in addition to the existing Gate-B recheck, so clone generation cannot silently mutate the authority artifact.
3. Diagnostic-clone post-serialization marker verification now counts each declaring-type `.cctor` once even when several selected methods share that type (notably `SaveManager`), and Gate C refuses to invoke the instrumented clone unless a durable launcher-owned checkpoint callback is present.

These corrections change diagnostic provenance/validation only. They do not broaden runtime authority beyond the Step-35.0.4 derivative experiment described above.
