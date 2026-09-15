# 0.0.209 native Tween rejection testing

- Bundle identity must be **0.0.209 (209)**.
- Every mutating Step-59 branch requires a fresh process.
- Primary run: fresh **FULL** closed path → Step 58 → **59N**. Preserve checkpoint, report, and static map.
- 59N must continue after individual row failures and record Tween managed/native identity, `IsValid`, `IsRunning`, old-tween relation, and PropertyTweener bridge state for each relevant row.
- 59N rows cover Node/SceneTree/SceneTree+BindNode creation, pre/post old-tween Kill context, TweenInterval, embark position current/showPos, embark scale/modulate current, outline modulate current, and two SceneTree full position tails.
- If the SceneTree full-tail row passes, confirm in a fresh FULL process with **59Y**. If the bound SceneTree row passes, confirm with **59Z**.
- 59Y/59Z run with wrapper/fluent repair disabled; a pass therefore validates the alternative creation path rather than a PropertyTweener repair.
- 59T/W/X remain available as controls and 59D/R/H/U remain optional controls.
- Preserve all files from a surprising failure; no matrix-row assumption is allowed to block Step 35.

## Retained 0.0.201 regression procedure

The **0.0.201 NConfirmButton OnEnable-preflight expectations** remain part of regression coverage. Step 59D must still emit `RUN_START_DIAGNOSTIC_DECK` and `M59D_B_STATIC_MAP_WRITE_RETURNED` without arming the handler. A real-handler failure path must still preserve `M59_C_CONFIRM_BUTTON_POSTFAIL` and `M59_C_POSTFAIL_STATIC_MAP_REFRESH_RETURNED`. `M59_B_CONFIRM_BUTTON_PREFLIGHT` remains required.
