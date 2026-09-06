# Current status

## Active candidate — Step 35.0.32 / Step 36.0.3 / 0.0.157 (157)

Physical **0.0.156** closed the Step-36 failure location. Gate B mounted the exact receipt-backed `SlayTheSpire2.app/Contents/Resources/Slay the Spire 2.pck`, `LoadResourcePack(..., replaceFiles=false, offset=0)` returned true, and `res://localization/eng` was present. Gate C then entered unchanged `ExecuteEssential` and failed in `ModelDb.Init` while `Activator.CreateInstance(BowlbugsNormal)` triggered `BowlbugsNormal..cctor`; that static initializer requested `ModelDb.Monster<BowlbugEgg>()` before `MONSTER.BOWLBUG_EGG` was in the ModelDb dictionary. The base exception was `KeyNotFoundException`. State had moved `1 -> 2`, sts2/GodotSharp private-context ownership remained intact, and there were zero initializer-bearing requests, zero rejected managed requests, and zero native-load attempts.

**0.0.157 / Step 36.0.3** addresses this as a general generated-model bootstrap-order compatibility problem. A private sts2 derivative is built from the exact Step-32 transformed source before CLR admission. The transform statically derives canonical generated model order and direct generic `ModelDb<T>` dependencies from model static constructors, computes the dependency closure for forward/backward bootstrap edges, pre-injects that closure in dependency order, then preserves canonical final dictionary order by remove/re-adding the same pre-injected instance at its original normal-loop position. It fails closed unless the physical `BowlbugsNormal -> BowlbugEgg` edge is actually present in the static graph.

Step 35.0.32 MODEL-BOOTSTRAP admits only that verified sts2 derivative while retaining exact prepared GodotSharp and the physically proven source-built Godot 4.5.1 bridge. Step 36 re-proves unchanged `ExecuteEssential`, mounts the same PCK, and invokes the full method once. A successful return closes not only ModelDb.Init but also the original trailing `ModelIdSerializationCache.Init -> ModelDb.InitIds -> MessageTypes.Initialize -> ActionTypes.Initialize` sequence in that same call. A later failure still emits the full exception chain/base/loader/state/resolver/context evidence.

The Codemagic workflow key remains `ios-canonical` and existing dependency/Godot/.NET/iOS intermediate cache paths remain enabled. `ExecuteDeferred`, launcher-driven `PrewarmJit`, game entry, native game loading, runtime Harmony/MonoMod, arbitrary resolver fallback, retry, and state reset remain forbidden.

## Physical sequence for 0.0.157

1. Fresh process: Step 15 Gates A-C.
2. Step 35.0.32 **MODEL-BOOTSTRAP** once; require 4/4.
3. Step 36.0.3 A-D once; if Gate C starts, do not retry in-process.
4. Preserve Step35 and Step36 run-correlated checkpoint/static-map/final reports.
