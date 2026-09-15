# Step 59.13 — 0.0.207 viability hardening

0.0.206 did not reach iOS compilation. Codemagic compiled the complete `StS2Launcher.Core` project, then the host-test project failed on one unsupported MSTest API: `Assert.Greater`. Static validation had passed 1437 checks, exposing a validation blind spot.

0.0.207 intentionally does **not** rewrite the runtime again. The three critical Core runtime files are hash-frozen to the exact 0.0.206 bytes that Codemagic compiled:

- `TransformedRealStS2VeryEarlyInitialization.cs` — `f32eef56c8120b30a453237cfbcb63c89eb39e0a3dae6946a5a780f07002e342`
- `TransformedRealStS2CharacterSelectContinuation.cs` — `2e9665de425a0ce2e0478ea755a05e3b22a4909f9b9c5cee41d2b19cf4ce734c`
- `TransformedRealStS2StartupLadder.cs` — `7968570ec64d710ba80d9f8a3ba98d8fd037269651e9348a08d51475cb8d7057`

The host assertion is replaced by the already-established MSTest-v4 form `Assert.IsTrue(fluentRepairCalls > 0, fluentName)`. Validation now scans every test source for `Assert.*` calls and rejects APIs outside the repository's known supported surface.

The compilation-efficient device deck is unchanged: 59T control, 59W wrapper-only repair, 59X full repair, 59E full Enable, real Step 59, then 60–62 if the transition succeeds; 59D/R/H/U remain available controls.

Architecture note: the launcher as a whole is not being reclassified as technical debt. The concentrated debt is that the Step-35 GodotSharp derivative builder hosts both bootstrap-critical compatibility and experimental Step-59 rewrite logic. That should be split only after the current PropertyTweener behavior is physically characterized; refactoring it before obtaining that evidence would unnecessarily discard a Core implementation already proven to compile.
