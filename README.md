# StS2 Launcher — Step 39.2

Active candidate: **0.0.170 (170)** — Gate-D frozen-confinement reflection correction after the first successful real Godot `SceneTree` insertion.

Physical **0.0.169 / Step 39.1** passed Gates A and B, produced the full 69-node lifecycle map with zero forbidden/unresolved/native-facing audit escapes, and then physically passed Gate C: the first real `SceneTree.Root.AddChild(NGame)` returned with `IsInsideTree=True`, exact `NGame.Instance`, non-null `_window`, exact root parent, OneTimeInitialization state 2, and zero rejected/initializer/native deltas. Rendering was synchronously stopped immediately afterward.

Gate D then failed only in launcher reflection bookkeeping: `Enumerable.Single(...)` saw more than one zero-argument `GetParent` method shape and threw `MoreThanOneMatch`. **0.0.170 changes only that Gate-D selector** to choose the non-generic, closed, SceneTree-root-compatible `GetParent()` overload. The Step-39.1 compatibility image, Gate A/B audit policy, Gate C AddChild semantics, inert GameStartupWrapper, Steam/Sentry/FM0D/Spine/native boundaries, and immediate render freeze are unchanged.

Authoritative status: `docs/CURRENT-STATUS.md`. The exact 0.0.169 physical report, checkpoint journal, last checkpoint, and full static map are preserved under `docs/history/reports/`.
