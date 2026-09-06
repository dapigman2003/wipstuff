# StS2 Launcher — Step 35.0.32 / Step 36.0.4

Active candidate: **0.0.158 (158)** — corrected dependency-aware `ModelDb` bootstrap compatibility plus one full unchanged `ExecuteEssential` attempt.

Physical **0.0.156** proved the PCK/localization handoff and localized unchanged `ExecuteEssential()` to `ModelDb.Init()`: eager initialization of `BowlbugsNormal` requested `ModelDb.Monster<BowlbugEgg>()` before `MONSTER.BOWLBUG_EGG` reached its canonical insertion point. Physical **0.0.157** then failed safely at Step-35 Gate A, before CLR admission, because the new compatibility helper assumed a private `ModelDb.Contains(Type)` method that the real assembly does not expose; the result was `InvalidOperationException: NoMatch`.

**0.0.158 fixes that pre-CLR transform defect without weakening the model-order strategy.** Gate A still derives the same 1,624-model / 56-edge / 47-backward-edge / 41-preinject plan from the exact Step-32 transformed authority and still requires the physical `BowlbugsNormal -> BowlbugEgg` edge and audited indices. The order-preserving helper no longer searches for private `ModelDb.Contains` or compiler-emitted Dictionary callsites. It discovers the exact closed `Dictionary<ModelId,AbstractModel>` field and constructs `ContainsKey`, `Add`, `Remove`, and `get_Item` MemberRefs directly from that closed generic metadata. Required ModelDb method/field lookups now fail with explicit shape diagnostics.

The compatibility mode retains exact prepared GodotSharp and the physically proven source-built Godot 4.5.1 bridge. `ExecuteVeryEarly` remains unchanged. Step 36 mounts the same receipt-backed PCK and invokes the **full unchanged `ExecuteEssential` once**. If ModelDb succeeds, `ModelIdSerializationCache.Init`, `ModelDb.InitIds`, `MessageTypes.Initialize`, and `ActionTypes.Initialize` continue naturally in the original call.

`ExecuteDeferred`, launcher-driven `PrewarmJit`, game entry, native game loading, runtime Harmony/MonoMod patching, arbitrary resolver fallback, retries, and `_state` reset remain forbidden.

## Physical test sequence

1. Fresh process: Step 15 Gates A-C.
2. Run Step 35 **MODEL-BOOTSTRAP** once and require 4/4.
3. Run **Step 36.0.4 A-D** once. Do not retry Gate C in the same process.
4. Preserve Step35 and Step36 run-correlated checkpoint/static-map/final-report artifacts.

Codemagic workflow key remains `ios-canonical`; the existing NuGet, .NET, Godot Step-15, and iOS intermediate cache paths are unchanged for warm-cache reuse.
