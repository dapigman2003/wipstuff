# Step 59.8 — compilation-efficient callsite trace / 0.0.202

## Evidence entering this candidate

Physical 0.0.201 used the compilation-efficient Step59D deck and a separate fresh-process real Step-59 transition. Step59D found no definite retained `NConfirmButton` Ready-product blocker. The real transition reproducibly failed in `NConfirmButton.OnEnable()` while the retained button remained ready/in-tree and its audited direct fields remained populated. Post-failure `_isEnabled` was true, the character-select logical stack was bound to the exact retained main-menu stack, and a one-player `StartRunLobby` existed. Formal physical authority remains Step 57.

## 0.0.202 design

Compilation, not phone execution, is the scarce resource. 0.0.202 therefore precompiles several independent Step-59 experiments into one IPA rather than spending a new build per hypothesis.

The already-selected private sts2 compatibility derivative gains a synthetic `ConfirmButtonCheckpointBridge` whose public static `Action<string>` callback is null by default. Stack-neutral `STEP59TRACE_*` markers are injected around selected method entries, original calls/newobj instructions and returns in the confirm-button/base-button/hotkey/input/controller chain. Explicit Step-59 experiments temporarily arm the callback to the durable checkpoint writer and always disarm it afterward. The real-handler path disarms before post-failure snapshots so diagnostic getters cannot contaminate the causal trace.

The same IPA contains Step59D, 59R controller/singleton rehearsal, 59U isolated `UpdateControllerButton()`, 59E isolated retained embark `Enable()`, and the real original Step59 transition. Mutation-bearing branches require fresh-process separation. The Step-59 peer matrix receives a local 4096-node ceiling while the historical/default Step39 traversal stays 256.

## Boundary

This candidate does not directly repair NConfirmButton fields, replay `_Ready()`, bypass `OnEnable()`, mutate the trusted StS2 install/TSCN/PCK, select a character, confirm/embark, start a run, or open Step 63. The existing PackedScene unique-name compatibility behavior remains the prerequisite compatibility layer.
