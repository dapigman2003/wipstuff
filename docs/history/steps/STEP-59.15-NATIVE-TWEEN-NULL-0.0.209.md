# Step 59.15 — native TweenProperty null localization / 0.0.209

Physical 0.0.208 finally separated native rejection from managed wrapper loss.

Both **59W** (Wrapper profile, wrapper repair enabled) and **59X** (Full profile, wrapper + fluent repair enabled) reached the isolated retained-embark confirm-button tail. In each run the visual writes, old-tween `Kill()`, and `Node.CreateTween()` returned normally. The next `Tween.TweenProperty(embark, "position", _showPos, 0.35)` returned null.

The decisive bridge telemetry is the before/after observation delta. Before the failing call, the same private GodotSharp runtime had already observed valid nonzero native pointers becoming real `Godot.PropertyTweener` instances. At the failing call, `observations` incremented by one, `nativeNulls` incremented by one, `lastNativePtr` became `0x0`, while `managedNulls=0`, `wrongTypes=0`, and `repairs=0`. The Full run also had healthy fluent-return observations. Therefore the active failure is not loss or mistyping of a valid PropertyTweener wrapper: native Godot itself returns null for this specific property-tween operation.

0.0.209 keeps the 0.0.208 Baseline / Wrapper / Full GodotSharp profile transform unchanged and moves the next experiment entirely into Step-59 runtime probing. **59N** runs a row-isolated native rejection matrix using fresh Tweens for each row: Node vs SceneTree creation, optional BindNode, pre/post old-tween Kill, Tween validity/running/native identity, TweenInterval vs TweenProperty, and several target/property/value combinations. It also runs complete fluent tails for SceneTree-created position tweens. **59Y** and **59Z** are focused fresh-process confirmation probes for SceneTree-created full position tails without and with BindNode.

No new bootstrap-critical GodotSharp IL transform is introduced by 0.0.209. A missing optional matrix method or one failed row is recorded as evidence and does not abort remaining rows.
