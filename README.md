# StS2 Launcher — Step 35.0.32 / Step 36.0.3

Active candidate: **0.0.157 (157)** — dependency-aware `ModelDb` bootstrap compatibility plus one full unchanged `ExecuteEssential` attempt.

Physical **0.0.156** closed the previous ambiguity. The exact receipt-backed PCK mounted successfully, `res://localization/eng` was visible, and unchanged `ExecuteEssential()` reached `ModelDb.Init()`. `Activator.CreateInstance(BowlbugsNormal)` triggered `BowlbugsNormal..cctor`, which called `ModelDb.Monster<BowlbugEgg>()` before `MONSTER.BOWLBUG_EGG` had reached its generated insertion point. The base failure was `KeyNotFoundException: MONSTER.BOWLBUG_EGG`; resolver/native counters stayed clean and both `sts2` and `GodotSharp` remained in the intended private context.

**0.0.157 fixes the ordering class, not one monster.** Gate A derives a verified compatibility copy of the exact Step-32 transformed `sts2` image. It statically extracts the generated canonical model order, scans canonical model static constructors for direct generic `ModelDb<T>` dependencies, finds every dependency edge that points forward in the generated order, computes the dependency closure, and pre-injects that closure in dependency order. During normal `ModelDb.Init` iteration, pre-injected models are removed/re-added with the **same instance** at their original generated-list position. This preserves final ModelDb insertion order and avoids duplicate constructors while satisfying eager static-initializer dependencies.

The compatibility mode uses the already-proven **exact prepared GodotSharp** and source-built Godot 4.5.1 bridge. `ExecuteVeryEarly` remains unchanged. Step 36 then mounts the same receipt-backed PCK and invokes the **full unchanged `ExecuteEssential` once**. If ModelDb succeeds, `ModelIdSerializationCache.Init`, `ModelDb.InitIds`, `MessageTypes.Initialize`, and `ActionTypes.Initialize` are allowed to run naturally in the original call. Full nested-exception/state/resolver/context telemetry remains active if any later initializer fails.

`ExecuteDeferred`, launcher-driven `PrewarmJit`, game entry, native game loading, runtime Harmony/MonoMod patching, arbitrary resolver fallback, retries, and `_state` reset remain forbidden.

## Physical test sequence

1. Fresh process: run Step 15 Gates A-C.
2. Without force-quitting/backgrounding, run Step 35 **MODEL-BOOTSTRAP** once and require 4/4.
3. Run **Step 36.0.3 A-D** once. Do not retry Gate C in the same process.
4. Preserve the Step35 and Step36 run-correlated checkpoint/static-map/final-report artifacts.

Codemagic workflow key remains `ios-canonical` so existing workflow-scoped NuGet, .NET, Godot Step-15, and iOS intermediate caches can be reused.
