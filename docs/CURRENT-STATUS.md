# Current status

**Steps 01–12 are complete and closed on a physical iPhone.**

**Current source candidate: Step 12.4 — post-Step-12 stabilization / cleanup.**

App version: `0.0.38 (38)`.
Codemagic workflow: `ios-step-12-4`.

Step 12.3 (`0.0.37`) is the last physically proven baseline. Step 12.4 adds no new launcher capability and does not begin Step 13. It hardens malformed receipt handling, interrupted receipt writes, cleanup/rollback result finalization, Step 11 partial-resume accounting, unreadable final-cache recovery, and the older Step 09/10 CDN timeout paths. It also removes stale diagnostic labels and cleans build/test artifact naming.

The proven Step 12 behavior remains: one selected direct public depot is classified as `NotInstalled`, `UpToDate`, `UpdateAvailable`, or `RepairNeeded`; a current-manifest Step 11 source is independently verified; install/update/repair build a complete staging tree, SHA-1 verify it, write the AOT-safe non-secret receipt, and replace the managed directory only after verification.

Before Step 12.4 becomes the new baseline, run its Codemagic build and the short physical-device regression in `STEP-12.4-STABILIZATION.md`.

**Step 13 has not started.**
