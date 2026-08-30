# Step 35.0.9 — Null-Platform Constructor Callsite Localization

Version: `0.0.132 (132)`

## Evidence basis

Physical 0.0.131 reached `NullPlatformUtilStrategy..ctor()` and then hard-terminated before `GodotFileIo..ctor(string)` emitted its entry marker. The final durable record was the planned `System.Collections.Concurrent 8.0.0.0 -> host 9.0.0.0` binding. No selected `Godot.DirAccess` marker was reached.

Therefore the next bounded experiment must localize **inside the NullPlatform constructor** rather than bootstrap Godot, broaden resolution, or assume the last resolver event is causal.

## Change

Keep exact transformed source/resolver/startup authority frozen. Preserve all existing entry and selected Godot callsite markers. Additionally enumerate the exact managed constructor's original `call`, `callvirt`, and `newobj` instructions and add ordered pre/post pairs around every non-base call-like instruction.

Markers use the constructor's original callsite ordinal:

`INMETHOD_NPxxx_PRE — NullPlatformUtilStrategy..ctor CALLSITE#xxx before ...`

`INMETHOD_NPxxx_POST — NullPlatformUtilStrategy..ctor CALLSITE#xxx after ...`

The direct base `.ctor` call is intentionally excluded so the callback is never executed while uninitialized `this` is on the evaluation stack. Any selected callsite that is itself a branch target fails instrumentation rather than silently changing control-flow coverage.

After serialization, the diagnostic clone reopens under rejecting resolution and each planned marker pair must remain immediately adjacent to the same opcode and callee. The exact transformed source is re-hashed unchanged.

The run-specific static map is extended with `[NULL PLATFORM CTOR IL]` so physical marker ordinals map directly to exact source-derived IL without a later proprietary-DLL analysis step.

## Regression protection

- retain synthetic Cecil serialize/reopen test for `Action<string>::Invoke(!0)`;
- retain selected Godot pre/post callsite-marker round-trip test;
- add synthetic serialize/reopen test for the NullPlatform constructor sweep, including skipped base constructor and instrumented `newobj`/`call` instructions;
- require total serialized `INMETHOD_*` count to equal the computed plan.

## Physical interpretation

- final `INMETHOD_NPxxx_PRE` without corresponding `POST`: boundary at/in that exact outgoing call/newobj;
- matching `POST` means the call returned and localization continues;
- no `NP` pre marker after `INMETHOD_024`: boundary is before the first swept non-base call and the next experiment should instrument non-call IL, not broaden startup authority;
- `INMETHOD_025` means execution advanced into `GodotFileIo`, at which point preserved downstream markers resume authority.

This step does not authorize Godot bootstrap/startup, native game loading, later initialization phases, Harmony/MonoMod runtime patching, or resolver broadening. A 4/4 result remains diagnostic-only and cannot close exact Step 35.
