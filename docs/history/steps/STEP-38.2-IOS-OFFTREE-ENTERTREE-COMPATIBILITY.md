# Step 38.2 — iOS/off-tree `NGame._EnterTree` compatibility

Physical 0.0.164 / Step 38.1 passed Gate A and Gate B, then physically entered `NGame._EnterTree()` once on the intentionally off-tree real `NGame` hierarchy. Gate C captured `System.NullReferenceException` with `IsInsideTree=false`; initializer-bearing/rejected/native deltas were all zero. The exact selected lifecycle map proves `GameStartupWrapper` was already the two-instruction inert `Task.CompletedTask` body and later startup reachability was zero.

The same static map shows two platform/environment-sensitive external edges in `_EnterTree` before the inert startup wrapper:

- `MegaCrit.Sts2.Core.Debug.SentryService.Initialize()`;
- `Godot.Node.GetWindow()` followed by `Godot.Window.SignalName.FilesDropped` and `Godot.GodotObject.Connect(...)`.

The Step-38 experiment deliberately calls `_EnterTree` while `IsInsideTree=false`; a real containing `Window` is therefore unavailable by construction. Sentry is also intentionally disabled for the unofficial iOS compatibility host.

0.0.165 extends the already selected pre-load compatibility derivative with exactly two stack-neutral edits in `_EnterTree`:

1. the single `SentryService.Initialize()` call becomes `nop`;
2. the exact bounded `ldarg.0 -> GetWindow -> FilesDropped delegate/cache -> Connect -> pop` block becomes `nop` instructions.

Before writing, the transform fails closed unless there is exactly one matching Sentry call, exactly one matching `GetWindow`, exactly one subsequent 3-parameter `GodotObject.Connect`, exactly one `FilesDropped` field reference inside the bounded block, and no branch from outside into that block. After serialization it reopens the derivative and requires:

- assembly identity/MVID and prior ModelDb bootstrap invariants preserved;
- exact inert `GameStartupWrapper` preserved;
- `_EnterTree` instruction count unchanged;
- NOP count increased by exactly `1 + boundedWindowBlockInstructionCount`;
- zero remaining `_EnterTree` Sentry initialize calls;
- zero remaining `_EnterTree` GetWindow calls;
- zero remaining `_EnterTree` FilesDropped field references;
- zero remaining `_EnterTree` matching GodotObject.Connect calls;
- exactly one preserved direct call to inert `GameStartupWrapper`;
- zero Cecil resolution requests on verification.

Step 38.2 still performs no `SceneTree.AddChild`, `_Ready`, synthetic `_ExitTree`, `GameStartup`, `InitializePlatform`, `LaunchMainMenu`, `ExecuteDeferred`, Steam initialization, or native GDExtension load.
