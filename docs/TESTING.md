# Testing — Steps 43–52 guarded direct main-menu ladder / 0.0.180

Active candidate: `0.0.180 (180)`, IPA `StS2-Launcher-Steps-43-52.ipa`, workflow `ios-canonical`.

Static/container validation proves source wiring, exact hashes, fail-stop sequencing, one-shot guards, report surfaces, release identity, provenance, and payload/security policy. Codemagic remains compile/AOT/link/package authority. Physical iPhone reports remain runtime authority.

For physical testing use a fresh process, establish the normal Step-42 authority, then run `43 → 44 → 45 → 46 → 47 → 48 → 49 → 50 → 51 → 52`, continuing immediately only after each rung reports 4/4. Stop on the first failure. Never retry an armed one-shot boundary in-process.

Step 48.1 is expected to rehearse and prove the concrete lifecycle guards while the real `NMainMenu` remains off-tree. A non-empty Godot command line, missing/replaced SaveManager singleton, non-Null platform strategy, context/native/tree drift during `SetRichPresence`, or any side effect from `CheckCommandLineArgs()` is a fail-closed result before Step 49.

Steps 43–49 and 51 must keep rendering stopped. Step 50 runs only the short audited pulse and refreezes. Step 52 runs only after Step 51 4/4, requests 1500 ms sustained residency, and synchronously refreezes before evidence evaluation. Preserve the rung-specific checkpoint/static-map/final-report files for the first failure or the final successful rung.

Do not infer gameplay, Steam, cloud, FMOD/Spine-native, or original-GameStartup viability from a static/container/Codemagic pass. Those remain separate future runtime boundaries.
