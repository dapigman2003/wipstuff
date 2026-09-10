# Testing — Steps 43–52 guarded direct main-menu ladder / 0.0.182

Active candidate: `0.0.182 (182)`, IPA `StS2-Launcher-Steps-43-52.ipa`, workflow `ios-canonical`.

Static/container validation proves source wiring, exact hashes, fail-stop sequencing, one-shot guards, report surfaces, release identity, provenance, and payload/security policy. Codemagic remains compile/AOT/link/package authority. Physical iPhone reports remain runtime authority.

For physical testing use a fresh process and establish the normal prerequisite authority through Step 48, then focus on `49 → 50 → 51 → 52`, continuing immediately only after each rung reports 4/4. Stop on the first failure. Never retry an armed one-shot boundary in-process.

## 0.0.182 Steps 49–52 focused correction

Step 49 Gate B must record exact immediate-child RootSceneContainer authority and the exact trivial setter audit. Gate C may repair a null NGame property only to that exact retained child. The checkpoint should then progress through `M49_C_ADDCHILD_RETURNED`, `M49_C_INSIDE_TREE_PASS`, `M49_C_PARENT_PASS`, and `M49_C_CHILD_COUNT_OBSERVED` before final confinement. A failure before/after any of these markers is intentionally distinguishable.

RootSceneContainer identity verification must not invoke the historical whole-NGame `EnumerateStep39NodeGraph` path. The exact authority is a single immediate NGame child named `RootSceneContainer`, exact managed type `NSceneContainer`, exact parent NGame, in-tree, with nullable-property consistency.

Steps 50 and 52 audit the dynamically expanded menu tree using the dedicated `EnumerateStartupLadderMenuNodeGraph` helper capped at 4,096 nodes. The immutable historical Step-39 helper remains capped at 256 and is not modified. A 4,096-node overflow is a diagnostic failure, not permission to continue.

Step 50 still runs only the short audited render pulse and refreezes. Step 51 still maps exact single-player handler frontiers without invocation/rendering. Step 52 still requests 1500 ms sustained residency and synchronously refreezes before evidence evaluation. Preserve the rung-specific checkpoint/static-map/final-report files for the first failure or final successful rung.

Do not infer gameplay, Steam, cloud, FMOD/Spine-native, or original-GameStartup viability from a static/container/Codemagic pass. Those remain separate future runtime boundaries.
