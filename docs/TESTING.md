# 0.0.207 PropertyTweener three-mode / compilation-efficient testing

0.0.207 carries the Codemagic-compiled 0.0.206 Core runtime byte-for-byte; the only functional source correction is in the host-test/build-validation surface.

- Bundle identity must be **0.0.207 (207)**.
- Every mutating Step-59 branch requires a fresh process.
- Recommended high-information sequence: **59T CONTROL → 59W WRAPPER-ONLY → 59X FULL REPAIR → 59E → real 59 → 60 → 61 → 62**, stopping only when a branch fails or supplies the needed distinction.
- 59D/R/H/U remain optional controls in the same IPA.
- Never replay legacy Steps 53–57 merely to unlock Step 58.
- Preserve crash checkpoint, last checkpoint, report, and static map for each experimental branch.

## Retained 0.0.201 regression procedure

The **0.0.201 NConfirmButton OnEnable-preflight expectations** remain part of regression coverage. Step 59D must still be able to emit `RUN_START_DIAGNOSTIC_DECK` and `M59D_B_STATIC_MAP_WRITE_RETURNED` without arming the handler. A real-handler failure path must still preserve `M59_C_CONFIRM_BUTTON_POSTFAIL` and `M59_C_POSTFAIL_STATIC_MAP_REFRESH_RETURNED` evidence.
- Retain `M59_B_CONFIRM_BUTTON_PREFLIGHT` in the Step-59 preflight evidence.
