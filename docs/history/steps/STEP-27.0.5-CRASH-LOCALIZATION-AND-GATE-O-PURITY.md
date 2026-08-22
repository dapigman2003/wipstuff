# Step 27.0.5 — Crash Localization + Gate-O Purity

## Trigger

Physical `0.0.88 (88)` did not provide a stable managed failure at the newly corrected Gate O. The user observed repeated abrupt app termination around the N–Q region. A subsequent same-process retry failed Gate A because `sts2` was already loaded in the dedicated Step-27 context. The raw Gate-A report is retained at `../reports/STEP-27.0.4-PHYSICAL-FRESH-PROCESS-GUARD-REPORT.txt`.

The Gate-A result is not a new regression. Once any Step-27 attempt reaches Gate B, the process contains the private game/Harmony load context and no longer satisfies the experiment's fresh-process precondition. The prior documentation incorrectly emphasized force-quit only after Gate T; 0.0.89 corrects that wording to require force-quit before every retry once Gate B has started.

The abrupt N–Q termination has no surviving managed exception, so the exact crash boundary is not yet established. The 0.0.84 physical A–Q result therefore remains the strongest clean execution evidence through Q.

## Candidate correction

Step 27.0.5 / `0.0.89 (89)` does not change the 26-gate launcher-only patch objective.

1. Add `Documents/StS2Launcher/Reports/Step27-CrashCheckpoint.txt`, an output-only diagnostic synchronously overwritten and flushed to disk at:
   - run start;
   - every gate START;
   - every gate PASS/FAIL;
   - ordinary progress callbacks;
   - sensitive Gate-O substages O1–O9;
   - Gate-R reflected getter / class-constructor substages R1–R3;
   - prefix-registration substages S1–S2;
   - first-patch substages T1–T2.
2. Make progress callbacks synchronous for crash-checkpoint persistence while still marshaling UI-label updates to the main thread.
3. Restore Gate O to admission/resolution semantics: it resolves the exact string-based `RuntimeInformation` type, `FrameworkDescription` PropertyInfo, and measured constructor metadata but **does not call `PropertyInfo.GetValue`**.
4. Gate R now owns the first reflected `FrameworkDescription` getter invocation immediately before `RuntimeHelpers.RunClassConstructor(HarmonyLib.AccessTools.TypeHandle)`.
5. No new trimmer root, Harmony fork, interpreter broadening, StS2 reflection, patching, game startup, or native loading is introduced.

## Interpretation rule for next physical run

If the process terminates without the normal Step-27 report, preserve `Step27-CrashCheckpoint.txt` before running Step 27 again. Its last `Phase`, `Gate`, and `Detail` become the authoritative crash-localization evidence for that attempt.
