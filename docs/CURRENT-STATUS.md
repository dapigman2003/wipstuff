# Current project status

**Current correction:** Step 18.3 / `0.0.50 (50)` keeps the Step 18.2 SHA-1-verified assembly-identity catalog, but corrects its overly strict exact-version rule after the physical iPhone advanced past `GodotSharp` and failed on `System.Runtime, Version=8.0.0.0` while the verified workspace contained `System.Runtime, Version=9.0.0.0` with the same culture/public-key token.

**Steps 01–17 are complete and closed on a physical iPhone.**

Step 15 / runtime `0.0.43 (43)` physically proved the independent source-built Godot 4.5.1-stable iOS host through native availability, engine/render-loop control, Metal rendering, and physical touch/lifecycle forwarding. A small initial-orientation/panel-layout quirk remains recorded as non-blocking.

Step 16.1 / runtime `0.0.45 (45)` physically passed all four Managed Preparation gates. Mono.Cecil 0.11.6 successfully read/write/reopened a project-owned fixture, performed a controlled IL rewrite, and inspected the real receipt-backed StS2 managed metadata read-only.

Step 17 / runtime `0.0.46 (46)` physically passed all four Compatibility Call-Site Analysis gates: receipt-backed ARM64/shared scope selection, concrete IL call-site scanning, native/platform interop classification, and a primary ARM64 `sts2.dll` dependency-pressure map. Step 17 remained read-only.

**Current source candidate:** Step 18.3 — Real Assembly Rewrite Workspace version-unification resolver correction.

- App version: `0.0.50 (50)`
- Codemagic workflow: `ios-step-18-3`
- Mono.Cecil runtime pin: `0.11.6`
- Godot 4.5.1 Step 15 host: retained as a regression-protected foundation
- Test model: ordered gates A–D; stop at first failure

Step 18 gates:

A. re-prove OfflineReady and clone the receipt-backed macOS arm64 + architecture-neutral managed scope into launcher-private Step 18 storage;
B. Cecil-write/reopen the real copied primary ARM64 `sts2.dll` and compare its logical metadata fingerprint;
C. insert one semantics-neutral IL NOP into a deterministic method of the copied primary assembly and verify it after reopen;
D. re-hash every workspace source and corresponding original managed-install file, proving the live Step 12 install remained unchanged.

**Step 18.3 does not apply a behaviorally significant compatibility fix and does not load or execute StS2 assemblies. Cecil writer-required resolution is restricted to the SHA-1-verified launcher-private Step 18 workspace; runtime/system/live-install/network fallback is forbidden.**
