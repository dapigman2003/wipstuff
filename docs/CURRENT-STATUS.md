# Current status

## Active candidate — Step 35.0.32 / Step 36.0.5 / 0.0.159 (159)

Physical **0.0.158** closed Step 35 MODEL-BOOTSTRAP and reached unchanged `ExecuteEssential`. Gate A/B passed, the receipt-backed PCK mounted, and `res://localization/eng` remained visible. Gate C then failed in `ModelDb.Init()` with `MissingMethodException: bool Dictionary<ModelId,AbstractModel>.ContainsKey(ModelId)`. Context stayed confined: no initializer-bearing request, rejected managed request, or native-load attempt occurred.

The dependency graph itself was generated successfully: `1624` canonical model types, `56` direct static-cctor ModelDb dependencies, `47` backward dependencies, `41` preinject targets, and the physical `BowlbugsNormal[658] -> BowlbugEgg[846]` edge. The failure is therefore the serialized generic MemberRef encoding introduced by 0.0.158.

**0.0.159 / Step 36.0.5** preserves that graph and same-instance canonical-order strategy, but reconstructs `Dictionary<TKey,TValue>` explicitly and emits `ContainsKey`, `get_Item`, `Remove`, and `Add` with declaring-type `VAR(0)`/`VAR(1)` signatures. This mirrors the Step-35 Action/Dictionary generic MemberRef pattern that already worked physically on iOS. The serialized derivative is reopened and the VAR positions are verified fail-closed before CLR admission.

Step 36 still invokes the full unchanged `ExecuteEssential` once. Success naturally continues through `ModelIdSerializationCache.Init -> ModelDb.InitIds -> MessageTypes.Initialize -> ActionTypes.Initialize`. Later failure retains full nested exception/state/resolver/context telemetry.

The `ios-canonical` workflow and existing cache paths are unchanged. ExecuteDeferred, launcher-driven PrewarmJit, game entry, native game loading, runtime Harmony/MonoMod, arbitrary resolver fallback, retry, and state reset remain forbidden.

## Physical sequence for 0.0.159

1. Fresh process: Step 15 Gates A-C.
2. Step 35.0.32 MODEL-BOOTSTRAP once; require 4/4.
3. Step 36.0.5 A-D once; once Gate C begins, do not retry in-process.
4. Preserve Step35/Step36 run-correlated reports.
