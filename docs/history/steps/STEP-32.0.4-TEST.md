# Step 32.0.4 — Test Procedure

Version: `0.0.119 (119)`

1. Run Codemagic workflow `step32-fast`.
2. Require canonical static validation PASS and complete host suite **231/231 PASS**. The Step-32 end-to-end regression must pass with both exact padded five-byte detail strings.
3. If fast fails, stop and preserve artifacts. Do not run device CI.
4. If fast passes, run `ios-step-32` on the exact same commit.
5. Require static validation, pinned iOS workload/publish, and IPA verification PASS.
6. Only then install the IPA from a fresh process and run physical Step 32 A–D.

Physical acceptance remains unchanged: **4/4 PASS**, with Gate B 6/6 + 4/4, ten exact five-byte windows, no Cecil serialization/resolution, Gate C 10/0 `PrepareMethod` references with exact padded semantics and byte-diff confinement, and Gate D OfflineReady/trusted-install/no-CLR-load isolation.

Preserve `Documents/StS2Launcher/Reports/Step32-RealStS2PrepareMethodRewrite.txt`.
