# StS2 Launcher — Steps 43–50 direct main-menu startup ladder

Active candidate: **0.0.179 (179)** — eight independently gated startup rungs in one IPA; stop on the first failure.

Physical **0.0.175 / Step 42** is closed positive **4/4**: exact `NGame.InitPools()` mapped to a 22-method zero-boundary closure, executed once on the real in-tree `NGame`, and returned with state 2, frozen rendering, and zero resolver/host/private/initializer/rejected/native deltas.

Physical **0.0.176** reached Step 45 Gate B and stopped safely before mutation on an incorrect singleton backing-field assumption. Physical **0.0.177** corrected exact `_mockInstance`/`_instance` authority and reached the real local-save call boundary; it stopped only on one planned host-framework materialization. Physical **0.0.178** then closed **Step 45 4/4**: Step 46 Gate A accepted same-process local-save authority with state 2, rendering frozen, and zero post-Step-45 resolver/host/private/initializer/rejected/native drift. Step 46 Gate B also located exact `NGame.LaunchMainMenu(bool)` token `0x06001BDF` and async `MoveNext` token `0x06001C3D` / 336 IL instructions. The old Step-46 Gate C failed safely without invocation because its graph recursively treated dormant delegate/function-pointer targets as executable and therefore pulled deferred startup, multiplayer, Steam, and Spine paths into one artificial closure.

0.0.179 keeps physically exercised Steps 43–45 unchanged and replaces the blocked live-launch design with an execution-aware, direct-resource ladder:

**Step 43 Null-platform authority → Step 44 read-only legacy-data guard → Step 45 local SaveManager initialization → Step 46.1 non-invoking immediate/deferred `LaunchMainMenu` frontier map → Step 47 exact PCK main-menu resource + private Spine-neutral background preparation → Step 48 one-shot off-tree exact `NMainMenu` instantiation/lifecycle audit → Step 49 one-shot frozen `RootSceneContainer.AddChild(NMainMenu)` → Step 50 audited bounded main-menu render pulse.**

Every rung has its own four-gate result and run-correlated evidence. Later rungs remain locked until the prior rung is 4/4 in the same process. Steps 43–49 keep rendering frozen; **Step 50 alone may start rendering, and it synchronously stops rendering again before evaluating success**.

Original whole `GameStartup`, original `LaunchMainMenu`, `DoCloudSync`, migration mutation, `InitializePlatform`, `LoadDeferredStartupAssetsAsync`, `ExecuteDeferred`, external Steamworks/native Steam, and FMOD/Spine native game extensions remain unopened.

Authoritative status, exact boundaries, report names, and physical sequence: `docs/CURRENT-STATUS.md`.

0.0.174 never reached device runtime: Codemagic Core compilation failed on invalid Step-42 identifier names. 0.0.175 corrected only those compile-surface identifiers and later physically closed Step 42.
