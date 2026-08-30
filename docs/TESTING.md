# Testing — Step 35.0.9 Null-Platform Constructor Callsite Localization

Active candidate: Step 35.0.9 / `0.0.132 (132)`.

Physical baseline: Step 32 `0.0.120` CLOSED POSITIVE 4/4, Step 33 `0.0.121` CLOSED POSITIVE 4/4, Step 34 `0.0.122` CLOSED POSITIVE 4/4. Step 35 remains OPEN. Physical 0.0.126 remains the authoritative exact transformed-byte frontier. Physical 0.0.129 exposed the malformed diagnostic `Action<string>::Invoke(string)` MemberRef; 0.0.130 proved `Action<string>::Invoke(!0)` and localized below `SaveManager.get_Instance`; physical 0.0.131 reached `NullPlatformUtilStrategy..ctor` but never reached `GodotFileIo..ctor`.

Release identity: IPA `StS2-Launcher-Step-35.ipa`, TRX `step35.trx`, version `0.0.132 (132)`. The Codemagic workflow key remains `ios-canonical`.

Running Step 34 and then Step 35 in the same process is invalid because Step 34 leaves `sts2` resident in a non-collectible private context. Always force-quit before Step 35. Once Gate B begins, the 0.0.132 process is spent and must be force-quit before another run.

## Authority rule for 0.0.132

0.0.132 is a **diagnostic derivative**, not an exact Step-35 compatibility candidate. Gate A must re-create and verify the exact closed Step-32 transformed artifact, then create a separate instrumented clone. Gate B/C may CLR-admit and execute only that clone. A 4/4 result is localization evidence but **must not be recorded as Step-35 closure**.

## Host/static expectations

Static validation must protect the closed Step-32/33/34 manifests and the active Step-35.0.9 manifest. Host tests protect ordered four-gate completion, first-failure stopping, exact target constants (`ExecuteVeryEarly` source token `0x06007D02`, `<ExecuteVeryEarly>d__7::MoveNext` source token `0x0600BC71`), initializer-free dependency admission, initializer-bearing refusal, crash-checkpoint coverage, static-map callsite/await tagging, diagnostic-only 4/4 wording, serialized `Action<string>::Invoke(!0)`, selected Godot callsite markers, and the new NullPlatform constructor callsite sweep.

The iOS/static contract additionally requires:

- one immutable Run ID/PID created before Gate A;
- unique run-specific journal/static map plus `Step35-CurrentRun.txt` and independently flushed `Step35-LastCheckpoint.txt`;
- exact transformed-source hash recheck after diagnostic-clone emission and before Gate-B admission;
- diagnostic-clone identity/MVID/signature/hash verification before CLR admission;
- no active text that describes 0.0.132 diagnostic 4/4 as exact Step-35 PASS/closure;
- no Godot startup/native bootstrap or resolver broadening.

## Diagnostic-output contract

All Step-35 telemetry is output-only and never trusted as runtime input. The exact-source static map is written after Gate-A semantic verification and before Gate B. For 0.0.132 it must include `[NULL PLATFORM CTOR IL]` with `CALLSITE#xxx` ordinals for the exact managed constructor.

0.0.132 retains prior markers through `INMETHOD_024`, `INMETHOD_025/026`, `INMETHOD_180/181`, and `INMETHOD_182/183`. It additionally emits ordered `INMETHOD_NPxxx_PRE` / `INMETHOD_NPxxx_POST` pairs around every non-base `call`, `callvirt`, and `newobj` in `NullPlatformUtilStrategy..ctor()`.

The final durable marker localizes execution. Resolver events remain context/frontier evidence and are not root-cause attribution by themselves.

## Physical Gate A — VerifiedExecutionPreflight

Require a fresh process with no resident `sts2`. Re-run the physically closed Step-32 transform A–D. Require exact source SHA-256 `e7ceb80669bfaf5c8fccabaa126ae2bb283aba514be5b5b55612579cfd285f18`, transformed SHA-256 `39c0a89ad0d5c6eb1553e23dd8537a7b7ab8278fad4115d186db5751570211ef`, transformed length 9,304,576, identity/MVID, source target token `0x06007D02`, source MoveNext token `0x0600BC71`, and matching source/transformed semantic fingerprints.

Diagnostic Cecil open/write ordering remains: **Deferred open -> verify zero resolver requests -> configure the exact audited writer-only constant metadata surrogates -> write -> rejecting-resolver reopen**. Never restore Immediate read before writer configuration.

Create `sts2.step35.0.9.instrumented.dll`. Preserve assembly identity/MVID and the exact target signature. Production verification must require:

- exactly one serialized bridge `callvirt Invoke` on `Action<string>` with sole parameter `VAR(0)` / `GenericParameterType.Type`, position 0;
- all prior selected entry and Godot callsite markers still serialize correctly;
- exact `NullPlatformUtilStrategy..ctor()` exists as managed IL;
- the constructor sweep enumerates original `call`/`callvirt`/`newobj` instructions, skips only the direct base `.ctor` and diagnostic bridge calls, and finds at least one selected callsite;
- no swept callsite is a branch target;
- every planned `INMETHOD_NPxxx_PRE/POST` pair reopens immediately around the same opcode and callee;
- the same-run static map contains `[NULL PLATFORM CTOR IL]` and constructor `CALLSITE#` ordinals;
- total serialized `INMETHOD_*` marker count equals the computed expected count;
- exact transformed source re-hashes unchanged after clone emission.

Requalify the persisted zero-blocker runtime plan and exact sole initializer-bearing `0Harmony 2.4.2.0` dependency. Write the same-Run-ID static map before any CLR admission.

## Physical Gate B — ExecutionCapableClrAdmission

Immediately re-hash both exact transformed source and diagnostic clone. The exact transformed source remains outside the CLR. `LoadFromStream` only the diagnostic clone into `StS2Launcher-Step35-VeryEarly`; require preserved identity/MVID/context ownership, unique resident `sts2`, and zero managed/private/initializer/rejected/native resolution during primary admission.

## Physical Gate C — DiagnosticExecuteVeryEarlyInvocation

Reflect only the admitted diagnostic clone's static parameterless Task-returning `ExecuteVeryEarly()`. Require the Gate-A-discovered diagnostic token and preserved closed MVID. Bind the exact `Action<string>` bridge field and durably write `C_DIAGNOSTIC_BRIDGE_ARMED` before invocation.

Invoke exactly once. If control returns, require a non-null `Task` and await at most 60 seconds. Runtime resolution remains limited to exact persisted host-framework bindings and exact hash-pinned initializer-free prepared private dependencies. Any initializer-bearing, unplanned managed, or native request fails closed.

Interpret hard termination using the constructor callsite sweep:

- `INMETHOD_NPxxx_PRE` without matching `POST`: boundary is at/in that exact outgoing call/newobj shown by the same-run constructor static map;
- matching `POST` means that call returned normally;
- `INMETHOD_024` with no subsequent NP pre marker: failure is before the first swept non-base call, so the next experiment must instrument non-call IL rather than broaden startup authority;
- `INMETHOD_025` means the constructor advanced into `GodotFileIo`; preserved downstream markers then govern interpretation.

Operator cancellation is **INCONCLUSIVE**, not PASS or compatibility FAIL.

## Physical Gate D — FinalIsolationAudit

If Gate C returns, re-prove OfflineReady, source SHA-256, exact transformed-source SHA-256, diagnostic-clone SHA-256, runtime-plan SHA-256, every resident private dependency hash, unique diagnostic-clone residency/context ownership, zero initializer-bearing/unplanned/native escape, and exactly one diagnostic `ExecuteVeryEarly` invocation.

The launcher must not intentionally invoke `ExecuteEssential`, `ExecuteDeferred`, `PrewarmJit`, the entry point, Harmony APIs, Godot startup or game startup. A Gate-D 4/4 result means **Step 35.0.9 diagnostic localization completed 4/4; Step 35 remains OPEN**.
