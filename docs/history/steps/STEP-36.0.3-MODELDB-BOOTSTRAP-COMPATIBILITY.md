# Step 36.0.3 — ModelDb bootstrap compatibility

Physical 0.0.156 proved that the resource-pack/localization handoff is no longer the blocker. The first internal failure is deterministic:

`ExecuteEssential -> ModelDb.Init -> Activator.CreateInstance(BowlbugsNormal) -> BowlbugsNormal..cctor -> ModelDb.Monster<BowlbugEgg>() -> KeyNotFound(MONSTER.BOWLBUG_EGG)`.

The same trace showed no rejected managed request, no initializer-bearing request, no native-load attempt, and preserved private-context ownership. The compatibility hypothesis is therefore eager static-initializer execution relative to the generated canonical model insertion order.

0.0.157 derives a private compatibility image from the exact Step-32 transformed `sts2` bytes before CLR admission. It extracts the generated `AbstractModelSubtypes.All` order, identifies direct generic `ModelDb<T>` dependencies from canonical model static constructors, computes every backward/bootstrap dependency closure, and fails closed unless the physically observed `BowlbugsNormal -> BowlbugEgg` edge is present with the expected ordering. Dependency targets are pre-injected in topological order. The rewritten normal loop calls a private helper that either performs normal `Inject(type)` or, for an already pre-injected type, retrieves the existing model, removes its dictionary entry, and re-adds the same object at the original generated-list position. This preserves final insertion order and object identity while avoiding a second constructor call.

The transform is reopened under rejecting Cecil resolution and verifies identity/MVID, constant-metadata fingerprint, pre-inject count, one normal-loop helper site, and the helper's remove/re-add same-instance contract. Exact Step-32 source bytes remain unchanged.

Step 35 MODEL-BOOTSTRAP uses this sts2 derivative with exact prepared GodotSharp and the already-proven source-built Godot bridge. Step 36 then calls unchanged `ExecuteEssential()` exactly once. Success means the original post-ModelDb initializers also returned. Failure retains the complete nested exception/base/loader/state/resolver/context diagnostic from 0.0.156.

Still forbidden: `ExecuteDeferred`, launcher `PrewarmJit`, entry point, native game load, runtime Harmony/MonoMod patching, arbitrary resolver fallback, same-process retry, and `_state` reset.
