# Current status

**Steps 01–12 are complete and closed on a physical iPhone.**

**Current physically exercised baseline: Step 12.4 — post-Step-12 stabilization / cleanup (`0.0.38`).**

The Step 12.4 short regression was reported working correctly. Its install/update/repair manager used the already-valid Step 11 source cache, so a brand-new CDN acquisition was not forced during that specific stabilization pass. Fresh acquisition and interrupted/resumable acquisition had already been physically proven in Steps 09–11.

**Current source candidate: Step 12.4.1 — download-cache test control.**

App version: `0.0.39 (39)`.
Codemagic workflow: `ios-step-12-4-1`.

Step 12.4.1 adds no launcher capability and does not begin Step 13. It adds only project-owned maintenance/test controls that can delete `Step11-ResumableDepot` without touching the Step 12 managed install or saved Steam session, and can pair that clear with the already-proven synthetic UpdateAvailable state so the next manager run is forced to reacquire the current public depot from Steam.

The proven Step 12 behavior remains unchanged: one selected direct public depot is classified as `NotInstalled`, `UpToDate`, `UpdateAvailable`, or `RepairNeeded`; a current-manifest Step 11 source is independently verified; install/update/repair build a complete staging tree, SHA-1 verify it, write the AOT-safe non-secret receipt, and replace the managed directory only after verification.

**Step 13 has not started.**
