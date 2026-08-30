# Testing — Step 35.0.8 Save/Platform/Godot Native-Boundary Localization

Active candidate: Step 35.0.8 / `0.0.131 (131)`.

Physical baseline: Step 32 `0.0.120` CLOSED POSITIVE 4/4, Step 33 `0.0.121` CLOSED POSITIVE 4/4, Step 34 `0.0.122` CLOSED POSITIVE 4/4. Step 35 remains OPEN. Physical 0.0.126 remains the authoritative exact transformed-byte frontier. Physical 0.0.129 proved the deferred writer correction but failed on malformed `Action<string>::Invoke(string)` bridge metadata. Physical 0.0.130 proved the corrected `Action<string>::Invoke(!0)` bridge and localized the hard termination under `SaveManager.get_Instance`, after a nested second `TestMode.get_IsOn`, before either settings-init method.

Release identity: IPA `StS2-Launcher-Step-35.ipa`, TRX `step35.trx`, version `0.0.131 (131)`. The Codemagic workflow key remains `ios-canonical` so NuGet/Godot/iOS arm64 `obj`/AOT caches survive diagnostic revisions.

Running Step 34 and then Step 35 in the same process is invalid because Step 34 leaves `sts2` resident in a non-collectible private context. Always force-quit before Step 35. Once Gate B begins, the 0.0.131 process is spent and must be force-quit before another run.

## Authority rule for 0.0.131

0.0.131 is a **diagnostic derivative**, not an exact Step-35 compatibility candidate. Gate A must re-create and verify the exact closed Step-32 transformed artifact, then create a separate instrumented clone. Gate B/C may CLR-admit and execute only that clone. A 4/4 result is localization evidence but **must not be recorded as Step-35 closure**.

## Host/static expectations

Static validation must protect the closed Step-32/33/34 manifests and the active Step-35.0.8 manifest. Host tests protect ordered four-gate completion, first-failure stopping, exact target constants (`ExecuteVeryEarly` token `0x06007D02`, `<ExecuteVeryEarly>d__7::MoveNext` token `0x0600BC71`), initializer-free dependency admission, initializer-bearing refusal, crash-checkpoint coverage, static-map callsite/await tagging, diagnostic-only 4/4 wording, the serialized `Action<string>::Invoke(!0)` MemberRef shape, and serialized pre/post callsite-marker placement.

The iOS source/static contract additionally requires:

- one immutable Run ID/PID created before Gate A;
- unique run-specific journal/static map plus `Step35-CurrentRun.txt` and independently flushed `Step35-LastCheckpoint.txt`;
- exact transformed-source hash recheck immediately after diagnostic-clone emission and again before Gate-B admission;
- diagnostic-clone identity/MVID/signature/hash verification before CLR admission;
- no active text that describes 0.0.131 diagnostic 4/4 as exact Step-35 PASS/closure;
- no Godot startup/native bootstrap added to this candidate.

## Diagnostic-output contract

All Step-35 telemetry is output-only and never trusted as runtime input. The exact-source static map is written after Gate-A semantic verification and before Gate B. It is metadata-only and may not resolve dependencies.

For 0.0.131, decisive marker evidence extends the existing set with:

- `INMETHOD_021` — `SaveManager.ConstructDefault()`;
- `INMETHOD_022` — `UserDataPathProvider.GetAccountScopedBasePath(...)`;
- `INMETHOD_023` — `PlatformUtil.get_PrimaryPlatform()`;
- managed `INMETHOD_CCTOR` for `PlatformUtil..cctor` when triggered;
- `INMETHOD_024` — `NullPlatformUtilStrategy..ctor()`;
- `INMETHOD_025` — `GodotFileIo..ctor(string)`;
- `INMETHOD_026` — `GodotFileIo.CreateDirectory(string)`;
- `INMETHOD_180/181` — immediately before/after `Godot.DirAccess.DirExistsAbsolute(string)`;
- `INMETHOD_182/183` — immediately before/after `Godot.DirAccess.MakeDirRecursiveAbsolute(string)`.

The last durable marker localizes execution; it does not by itself establish a native root cause.

## Physical Gate A — VerifiedExecutionPreflight

Require a fresh process with no resident `sts2`. Re-run the physically closed Step-32 transform A–D. Require exact source SHA-256 `e7ceb80669bfaf5c8fccabaa126ae2bb283aba514be5b5b55612579cfd285f18`, transformed SHA-256 `39c0a89ad0d5c6eb1553e23dd8537a7b7ab8278fad4115d186db5751570211ef`, transformed length 9,304,576, identity/MVID, source target token `0x06007D02`, source MoveNext token `0x0600BC71`, source/transformed wrapper + MoveNext semantic equality, zero direct later-boundary calls, zero direct Harmony references, and zero Cecil dependency resolution for those audits.

Diagnostic-clone creation must use **Cecil `ReadingMode.Deferred` only**. The initial open must produce zero resolver requests. Only then may the audited writer-only constant-metadata resolver be configured. `ReadingMode.Immediate` is forbidden because physical 0.0.128 proved it can resolve `System.Runtime` before configuration.

The bridge must be produced through the same helper exercised by the synthetic round-trip host test and serialize as `Action<string>::Invoke(!0)`, never `Invoke(string)`. This is the specific regression exposed by physical 0.0.129.

Create `sts2.step35.0.8.instrumented.dll`. The clone may add only the diagnostic bridge, selected entry markers, and the two selected Godot callsite pre/post marker pairs. It must preserve assembly identity/MVID and target signature. Production verification must reopen under a rejecting resolver and require:

- exactly one serialized bridge `callvirt Invoke` with declaring type `Action<string>` and sole parameter `VAR(0)` / Cecil `GenericParameterType.Type`, position 0;
- every selected entry marker at the start of its target method;
- exactly one `Godot.DirAccess::DirExistsAbsolute(System.String)` call in `GodotFileIo.CreateDirectory` with the `180/181` pair immediately around it;
- exactly one `Godot.DirAccess::MakeDirRecursiveAbsolute(System.String)` call in that method with the `182/183` pair immediately around it;
- no marked Godot callsite is a branch target before instrumentation;
- total serialized `INMETHOD_*` marker count equals the computed expected count;
- exact transformed source re-hashes unchanged after clone emission.

Requalify the persisted zero-blocker runtime plan and exact sole initializer-bearing `0Harmony 2.4.2.0` dependency. Write the same-Run-ID static map before any CLR admission.

## Physical Gate B — ExecutionCapableClrAdmission

Immediately re-hash both exact transformed source and diagnostic clone. The exact transformed source remains outside the CLR. `LoadFromStream` only the diagnostic clone into `StS2Launcher-Step35-VeryEarly`; require preserved identity/MVID/context ownership, unique resident `sts2`, and zero managed/private/initializer/rejected/native resolution during primary admission.

## Physical Gate C — DiagnosticExecuteVeryEarlyInvocation

Reflect only the admitted diagnostic clone's static parameterless Task-returning `ExecuteVeryEarly()`. Require the Gate-A-discovered diagnostic token and preserved closed MVID. Bind the exact `Action<string>` bridge field and write `C_DIAGNOSTIC_BRIDGE_ARMED` before invocation.

Invoke exactly once. If control returns, require a non-null `Task` and await at most 60 seconds. Runtime resolution remains limited to exact persisted host-framework bindings and exact hash-pinned initializer-free prepared private dependencies. Any initializer-bearing, unplanned managed, or native request fails closed.

Interpret hard termination as follows:

- `180` without `181`: physical boundary is inside/at `DirExistsAbsolute`;
- `181` then `182` without `183`: physical boundary is inside/at `MakeDirRecursiveAbsolute`;
- both post markers present: continue from the next marker; do not attribute failure to those calls;
- if `GodotFileIo.CreateDirectory` entry itself is absent, the failure is earlier in the Save/Platform path.

Operator cancellation is **INCONCLUSIVE**, not PASS or compatibility FAIL.

## Physical Gate D — FinalIsolationAudit

If Gate C returns, re-prove OfflineReady, source SHA-256, exact transformed-source SHA-256, diagnostic-clone SHA-256, runtime-plan SHA-256, every resident private dependency hash, unique diagnostic-clone residency/context ownership, zero initializer-bearing/unplanned/native escape, and exactly one diagnostic `ExecuteVeryEarly` invocation.

The launcher must not intentionally invoke `ExecuteEssential`, `ExecuteDeferred`, `PrewarmJit`, the entry point, Harmony APIs, Godot startup or game startup. A Gate-D 4/4 result means **Step 35.0.8 diagnostic localization completed 4/4; Step 35 remains OPEN**.
