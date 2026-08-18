# Current project status

**Steps 01–17 are complete and closed on a physical iPhone.**

Step 15 / runtime `0.0.43 (43)` physically proved the independent source-built Godot 4.5.1-stable iOS host through native availability, engine/render-loop control, Metal rendering, and physical touch/lifecycle forwarding. A small initial-orientation/panel-layout quirk remains recorded as non-blocking.

Step 16.1 / runtime `0.0.45 (45)` physically passed all four Managed Preparation gates. Mono.Cecil 0.11.6 successfully read/write/reopened a project-owned fixture, performed a controlled IL rewrite, and inspected the real receipt-backed StS2 managed metadata read-only.

Step 17 / runtime `0.0.46 (46)` physically passed all four Compatibility Call-Site Analysis gates: receipt-backed ARM64/shared scope selection, concrete IL call-site scanning, native/platform interop classification, and a primary ARM64 `sts2.dll` dependency-pressure map. Step 17 remained read-only.

**Current source candidate:** Step 18 — Real Assembly Rewrite Workspace.

- App version: `0.0.47 (47)`
- Codemagic workflow: `ios-step-18`
- Mono.Cecil runtime pin: `0.11.6`
- Godot 4.5.1 Step 15 host: retained as a regression-protected foundation
- Test model: ordered gates A–D; stop at first failure

Step 18 gates:

A. re-prove OfflineReady and clone the receipt-backed macOS arm64 + architecture-neutral managed scope into launcher-private Step 18 storage;
B. Cecil-write/reopen the real copied primary ARM64 `sts2.dll` and compare its logical metadata fingerprint;
C. insert one semantics-neutral IL NOP into a deterministic method of the copied primary assembly and verify it after reopen;
D. re-hash every workspace source and corresponding original managed-install file, proving the live Step 12 install remained unchanged.

**Step 18 does not apply a behaviorally significant compatibility fix and does not resolve, load, or execute StS2 assemblies.**
