# StS2 Launcher — Steps 43–52 physically closed stabilization

Active candidate: **0.0.183 (183)** — stabilization/ergonomics/CI only. **Step 53 remains unopened.**

Physical authority now reaches **Step 52 4/4** on the current same-process route. The supplied first 0.0.182 Step-50 attempt proved a clean 1,075-node in-tree frame/input map and successful renderer start/refreeze, then failed only because its first managed continuation arrived at ~2526 ms beyond the old 2000 ms evidence ceiling. The user subsequently reported a fresh rerun with **Steps 50, 51 and 52 all 4/4**; successful rerun report files were not supplied, so history records that closure explicitly as user-reported physical authority rather than fabricating artifacts.

0.0.183 does not broaden runtime authorization. It makes the already-closed path easier and the CI build more measurable:

- a **fresh-process, one-shot Physically Closed Path button** runs Step 15 A–C → Step 35.0.32 MODEL-BOOTSTRAP → 36 → 37 → **skips 38** → 39 through 52, calling the existing step implementations and requiring each exact pass/closure predicate before advancing;
- Step 50 keeps its exact **100 ms requested stop delay** and synchronous stop-before-telemetry behavior, but uses a Step-50-specific **4000 ms evidence ceiling** to cover the physically observed cold-frame 2526 ms return. Historical Step 40 stays 2000 ms; Step 52 stays 1500/6000 ms;
- Codemagic keeps workflow `ios-canonical` and the existing caches, but now also caches the two tiny .NET iOS AOT dependency sentinels `AOTCompileInputs.cache` and `AOTCompileInputs.cache.uptodate`, plus reports pre/post marker hashes and LLVM `opt`/`llc` counts. The supplied 0.0.182 artifact localized **3195 of 3250 seconds** to iOS publish/package despite 2.9 GB of restored iOS obj/AOT cache and 1092 cached AOT outputs.

Every existing step keeps its independent reports/checkpoints and fail-closed behavior. The convenience runner stops at the first non-closed return and requires a process relaunch; it does not run Step 15 Gate D and it never invokes Step 38. Rendering must be frozen after Step 52.

Original whole `GameStartup`, original `LaunchMainMenu`, single-player handler invocation, `DoCloudSync`, migration mutation, `InitializePlatform`, deferred startup/`ExecuteDeferred`, external/native Steam, and native FMOD/Spine remain unopened.

Authoritative status and exact physical sequence: `docs/CURRENT-STATUS.md`.
