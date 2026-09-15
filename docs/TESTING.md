# 0.0.208 profile-isolated PropertyTweener testing

- Bundle identity must be **0.0.208 (208)**.
- Every closed-path/profile run and every mutating Step-59 branch requires a fresh process.
- **BASELINE run:** press `Run Closed Path — BASELINE (PackedScene only) → Step 52`, then Step 58, then **59T**. Baseline contains no PropertyTweener experiment.
- **WRAPPER run:** fresh process, press `Run Closed Path — WRAPPER profile → Step 52`, then Step 58, then **59W**. The bridge records the original native pointer and managed wrapper/null/wrong-type state before wrapper repair. Fluent returns remain natural.
- **FULL run:** fresh process, press `Run Closed Path — FULL profile → Step 52`, then Step 58, then **59X**. If it passes, fresh FULL processes may run **59E** and then real **59**.
- If real Step 59 passes, continue **60 → 61 → 62** without recompiling.
- 59D/R/H/U remain optional controls under any profile.
- Wrong-profile Step-59 buttons must fail closed before mutation and instruct a relaunch with the matching profile.
- Preserve crash checkpoint, last checkpoint, report, and static map for each experimental branch.

## Retained 0.0.201 regression procedure

The **0.0.201 NConfirmButton OnEnable-preflight expectations** remain part of regression coverage. Step 59D must still emit `RUN_START_DIAGNOSTIC_DECK` and `M59D_B_STATIC_MAP_WRITE_RETURNED` without arming the handler. A real-handler failure path must still preserve `M59_C_CONFIRM_BUTTON_POSTFAIL` and `M59_C_POSTFAIL_STATIC_MAP_REFRESH_RETURNED`. `M59_B_CONFIRM_BUTTON_PREFLIGHT` remains required.
