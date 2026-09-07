# StS2 Launcher — Step 35.0.32 / Step 36.0.5

Active candidate: **0.0.159 (159)** — ECMA-correct dependency-aware `ModelDb` bootstrap plus one full unchanged `ExecuteEssential` attempt.

Physical **0.0.158** successfully closed Step 35 MODEL-BOOTSTRAP and reached the unchanged Step-36 `ExecuteEssential()` boundary. The compatibility graph itself was correct and durably reported: 1,624 canonical models, 56 direct static-cctor ModelDb edges, 47 backward edges, 41 pre-injected dependency targets, including `BowlbugsNormal[658] -> BowlbugEgg[846]`. Execution then failed in `ModelDb.Init()` with `MissingMethodException: Dictionary<ModelId,AbstractModel>.ContainsKey(ModelId)`.

The defect was the emitted ECMA-335 MemberRef signature, not the dependency strategy. **0.0.159** mirrors the already-physical Step-35 generic MemberRef correction: the declaring type is constructed as `Dictionary<ModelId,AbstractModel>`, while member signatures remain encoded with declaring-type `VAR(0)`/`VAR(1)`. `ContainsKey`, `get_Item`, `Remove`, and `Add` are all reopened and verified to retain those generic-variable signatures before the derivative is admitted.

The 41-model bootstrap, same-instance remove/re-add canonical ordering, exact prepared GodotSharp bridge, PCK mount, localization proof, resolver confinement, and one-shot unchanged `ExecuteEssential` policy are otherwise unchanged. If ModelDb succeeds, the same device attempt continues naturally through `ModelIdSerializationCache.Init`, `ModelDb.InitIds`, `MessageTypes.Initialize`, and `ActionTypes.Initialize`.

`ExecuteDeferred`, launcher-driven `PrewarmJit`, game entry, native game loading, runtime Harmony/MonoMod, arbitrary resolver fallback, retry, and `_state` reset remain forbidden.

## Physical test sequence

1. Fresh process: Step 15 Gates A-C.
2. Run Step 35 **MODEL-BOOTSTRAP** once and require 4/4.
3. Run **Step 36.0.5 A-D** once. Do not retry Gate C in the same process.
4. Preserve Step35 and Step36 run-correlated checkpoint/static-map/final-report artifacts.

Codemagic workflow key remains `ios-canonical`; existing NuGet, .NET, Godot Step-15, and iOS intermediate cache paths are unchanged for warm-cache reuse.
