# Current status

## Active candidate — Step 35.0.32 / Step 36.0.4 / 0.0.158 (158)

Physical **0.0.156** closed PCK/localization and localized the first unchanged `ExecuteEssential` failure to `ModelDb.Init -> BowlbugsNormal..cctor -> ModelDb.Monster<BowlbugEgg>() -> KeyNotFoundException(MONSTER.BOWLBUG_EGG)` with state `1 -> 2`, intact sts2/GodotSharp private-context ownership, and zero initializer-bearing, rejected-managed, or native-load attempts.

Physical **0.0.157** never admitted the compatibility sts2 derivative. Step-35 Gate A stopped in the pre-CLR compatibility clone with `InvalidOperationException: NoMatch`. Review of the clone builder identifies the first brittle lookup as an assumed private `ModelDb.Contains(Type)` method; the real assembly does not expose that member. This is a transform-construction defect, not a retreat of the physically closed Step-35 bridge/runtime or Step-36 resource boundaries.

**0.0.158 / Step 36.0.4** retains the bootstrap-order compatibility strategy and the independently audited model graph (`1624` canonical types, `56` direct static-cctor ModelDb edges, `47` backward edges, `41` pre-injected dependency-closure types, BowlbugsNormal index `658`, BowlbugEgg index `846`) but removes reliance on that nonexistent helper and on compiler-emitted Dictionary callsite shapes. The transform discovers the closed `Dictionary<ModelId,AbstractModel>` field and constructs exact `ContainsKey`, `Add`, `Remove`, and `get_Item` MemberRefs from its generic arguments. ModelDb method and field lookup failures now produce explicit metadata-shape diagnostics rather than LINQ `NoMatch`.

Step 35 MODEL-BOOTSTRAP admits only the verified compatibility derivative, with exact prepared GodotSharp and the proven source-built Godot bridge. Step 36 re-proves unchanged `ExecuteEssential`, mounts `SlayTheSpire2.app/Contents/Resources/Slay the Spire 2.pck` via `LoadResourcePack`, proves `res://localization/eng`, and invokes the full method once. Success naturally covers `ModelDb.Init -> ModelIdSerializationCache.Init -> ModelDb.InitIds -> MessageTypes.Initialize -> ActionTypes.Initialize`; later failure retains full nested exception/state/resolver/context telemetry.

The workflow remains `ios-canonical` with the existing cache keys/paths. `ExecuteDeferred`, launcher-driven `PrewarmJit`, game entry, native game loading, runtime Harmony/MonoMod, arbitrary resolver fallback, retry, and state reset remain forbidden.

## Physical sequence for 0.0.158

1. Fresh process: Step 15 Gates A-C.
2. Step 35.0.32 **MODEL-BOOTSTRAP** once; require 4/4.
3. Step 36.0.4 A-D once; once Gate C begins, do not retry in-process.
4. Preserve the Step35 and Step36 run-correlated checkpoint/static-map/final reports.
