# Step 33.0 / 0.0.121 — Test Plan

1. Run canonical static validation and require zero failures.
2. Run the complete host test suite, including `TransformedRealStS2AssemblyAdmissionTests` gate-order and admission-only resolver tests.
3. Publish the iOS app with the physically proven interpreter/copy/no-link policy and verify the final IPA is 0.0.121 (121).
4. Install on the physical iPhone, force-quit/relaunch, and do not start Godot or run any earlier real-game CLR-load boundary in that process.
5. Run **Step 33 A–D** once.
6. Preserve `Documents/StS2Launcher/Reports/Step33-TransformedRealStS2AssemblyAdmission.txt` whether PASS or FAIL.
7. A valid PASS is exactly **TRANSFORMED REAL STS2 CLR ADMISSION PASS — 4/4**. Any failure stops later gates and becomes the next evidence boundary.
