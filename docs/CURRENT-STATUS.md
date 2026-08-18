# Current project status

**Steps 01–16 are complete and closed on a physical iPhone.**

Step 15 / runtime `0.0.43 (43)` physically proved the independent source-built Godot 4.5.1-stable iOS host through all four ordered gates: native bridge availability, engine/render-loop control, Metal smoke rendering, and physical touch/lifecycle forwarding. A small initial-orientation/panel-layout quirk remains recorded as non-blocking.

Step 16.1 / runtime `0.0.45 (45)` physically passed all four Managed Preparation gates. Mono.Cecil 0.11.6 successfully read a project-owned fixture on-device, wrote/reopened a private copy, verified a controlled IL rewrite `7 → 42`, and parsed the real receipt-backed StS2 managed metadata read-only. The macOS depot's arm64/x86_64 duplicate `sts2.dll` layout is now explicitly handled.

**Current source candidate:** Step 17 — Compatibility Call-Site Analysis.

- App version: `0.0.46 (46)`
- Codemagic workflow: `ios-step-17`
- Mono.Cecil runtime pin: `0.11.6`
- Godot 4.5.1 Step 15 host: retained as a regression-protected foundation
- Test model: ordered gates A–D; stop at first failure

Step 17 gates:

A. re-prove OfflineReady and select the receipt-backed macOS arm64 + architecture-neutral managed scope while excluding x86_64 duplicates;
B. scan concrete IL method-reference instructions for dynamic/AOT-sensitive call sites;
C. classify P/Invoke/native-module and platform-sensitive managed API evidence;
D. build a direct dependency-pressure map for the primary arm64 `sts2.dll` and re-hash every scanned candidate.

**No real StS2 assembly is rewritten, resolved, loaded, or executed in Step 17.**
