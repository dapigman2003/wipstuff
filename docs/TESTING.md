# 0.0.210 scripted-object property bridge testing

- Bundle identity must be **0.0.210 (210)**.
- Every mutating Step-59 branch requires a fresh process.
- **Run 1:** fresh FULL closed path → Step 58 → **59P**. Preserve checkpoint, report, and static map.
- 59P must compare scripted `NConfirmButton`, scripted `NActDropdown`, and native `Outline` across direct managed access plus Godot `Get`, `GetIndexed`, same-value `Set`, and `SetIndexed` for `position`, `scale`, and `modulate`.
- **Run 2:** fresh FULL closed path → Step 58 → **59I**. Preserve all files. It must record retained/native pointer identity, `InteropUtils.UnmanagedGetManaged` mapping, `ReferenceEquals`, instance IDs, script identity, and native callback signatures.
- **Run 3:** fresh FULL closed path → Step 58 → **59Q**. 59Q must first durably record required versus optional direct-managed preparation stages, then invoke the original game-owned transition exactly once.
- If 59Q closes Step 59 4/4, stay in the same process and run **60 → 61 → 62**.
- Physical 0.0.209 / 59N remains a required regression fact: valid fresh Tweens + successful interval rows; scripted NConfirmButton property rows fail native-null; native Outline modulate rows pass.
- 59N/Y/Z and 59T/W/X remain available as controls/history; they are not the preferred first runs for 0.0.210.
- No 59P/59I/59Q assumption may become a Step-35 bootstrap blocker.

## Retained 0.0.201 regression procedure

The **0.0.201 NConfirmButton OnEnable-preflight expectations** remain part of regression coverage. Step 59D must still emit `RUN_START_DIAGNOSTIC_DECK` and `M59D_B_STATIC_MAP_WRITE_RETURNED` without arming the handler. A real-handler failure path must still preserve `M59_C_CONFIRM_BUTTON_POSTFAIL` and `M59_C_POSTFAIL_STATIC_MAP_REFRESH_RETURNED`. `M59_B_CONFIRM_BUTTON_PREFLIGHT` remains required.
