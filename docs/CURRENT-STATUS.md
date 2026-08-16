# Current status

**Steps 01–13 are complete and closed on a physical iPhone.**

**Current physically proven baseline:** Step 13.0.1 runtime / `0.0.40 (40)`. The Step 13 local-only gate passed: the legitimate Step 12 managed install became `OfflineReady` without a Steam/session/network dependency, local corruption became `RepairRequired`, repair returned it to a good state, and the prior regressions passed.

**Current source candidate:** Step 14 — read-only compatibility inventory.

- App version: `0.0.41 (41)`
- Codemagic workflow: `ios-step-14`
- Expected IPA: `artifacts/StS2-Launcher-Step-14.ipa`

Step 14 adds one capability only: re-prove `OfflineReady`, then inventory the receipt-backed installed depot for assets, Godot content, managed assemblies, native binaries, GodotSharp/FMOD/Spine indicators, reflection/dynamic-code indicators, and platform-specific pieces.

The Step 14 inspector has no Steam session/client/HTTP/CDN dependency, does not write the managed install, does not load game assemblies, and does not execute or launch game code.

**Step 14 is not complete until the physical-iPhone inventory gate in `docs/STEP-14-TEST.md` passes and the actual inventory findings are reviewed.**

No Step 15 Godot-host work or later compatibility rewriting has started.
