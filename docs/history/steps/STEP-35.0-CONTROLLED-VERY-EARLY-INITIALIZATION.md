# Step 35.0 — Controlled Transformed Real-StS2 Very-Early Initialization

Candidate: `0.0.123 (123)`.

## Evidence boundary

Physical `0.0.122` closed Step 34 at 4/4: exact transformed `OneTimeInitialization::PrewarmJit()` was invoked once and returned normally. Binding/invocation produced 8 managed resolver requests: 6 exact planned host-framework loads plus 2 hash-pinned initializer-free private dependency loads, with zero initializer-bearing, unplanned, or native requests. The receipt-backed original remained outside the CLR and no entry point/Godot startup occurred. Preserve `../reports/STEP-34.0-PHYSICAL-CLOSURE-4OF4.txt`.

Static read-only inspection of the exact receipt-backed Step-32 `sts2.dll` (source SHA-256 `e7ceb80669bfaf5c8fccabaa126ae2bb283aba514be5b5b55612579cfd285f18`, MVID `518e4758-52d7-47c2-b776-471a0e29e49d`) identified the next narrow natural managed-initialization boundary:

- `MegaCrit.Sts2.Core.Helpers.OneTimeInitialization::ExecuteVeryEarly()` — source MethodDef `0x06007D02`, static, parameterless, returns `System.Threading.Tasks.Task`.
- async state machine `<ExecuteVeryEarly>d__7::MoveNext` — source MethodDef `0x0600BC71`.
- the direct `ExecuteVeryEarly` call occurs once in `<GameStartup>d__117::MoveNext`; a direct `ExecuteEssential()` call appears later in that same state machine.
- `ExecuteDeferred()` is directly called from `<LoadDeferredStartupAssetsAsync>d__132::MoveNext`; `ExecuteDeferred()` directly calls `PrewarmJit()`.

This supports executing `ExecuteVeryEarly` alone before authorizing `ExecuteEssential` or `ExecuteDeferred`. It does not assume that the method will succeed on iOS; any exact failure is new evidence.

## Gates

**A — VerifiedExecutionPreflight.** Re-run the closed Step-32 transform A–D. Require exact source/transformed bytes, identity/MVID, and the exact source `ExecuteVeryEarly` token/signature. Independently reopen source and transformed images with rejecting Cecil resolvers. Require semantic fingerprint equality for both the wrapper and `<ExecuteVeryEarly>d__7::MoveNext`, source MoveNext token `0x0600BC71`, zero direct calls from MoveNext to `ExecuteEssential`, `ExecuteDeferred`, or `PrewarmJit`, zero direct Harmony method references, and zero Cecil resolution. Requalify the persisted prepared runtime plan and exact sole initializer-bearing `0Harmony 2.4.2.0` dependency. No StS2 CLR admission.

**B — ExecutionCapableClrAdmission.** Re-hash and LoadFromStream only the exact Step-32 transformed primary into `StS2Launcher-Step35-VeryEarly`. Require exact identity/MVID/context ownership and the physical Step-33 zero-resolution primary-admission behavior.

**C — ExactExecuteVeryEarlyInvocation.** Reflect exact transformed static parameterless Task-returning `ExecuteVeryEarly()`, require Gate-A-discovered transformed token + exact MVID, invoke once, require exact Task result, and await it for at most 60 seconds. Resolver authority is limited to exact persisted host-framework bindings and exact hash-pinned initializer-free prepared private dependencies. Initializer-bearing, unplanned managed, and all native resolution fail closed. Preserve full exception chain and resolver state on failure.

**D — FinalIsolationAudit.** Re-prove OfflineReady, trusted source/transformed/runtime-plan hashes, resident private dependency hashes, unique transformed-primary context residency, zero initializer-bearing/unplanned/native escape, and exactly one launcher invocation.

## Explicitly unauthorized

Receipt-backed/prepared original `sts2.dll` CLR admission; intentional launcher invocation of `ExecuteEssential`, `ExecuteDeferred`, `PrewarmJit`, or the game entry point; `0Harmony` initialization; Harmony/MonoMod patching; Godot/game startup; native game loading; arbitrary resolver fallback; broad startup sequencing.

A 4/4 PASS authorizes only a separately designed next managed-initialization boundary.
