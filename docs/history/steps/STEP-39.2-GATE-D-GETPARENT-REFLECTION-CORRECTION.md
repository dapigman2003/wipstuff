# Step 39.2 — Gate-D GetParent reflection correction

Physical 0.0.169 / Step 39.1 passed Gates A/B/C and established the first successful real `SceneTree.Root.AddChild(NGame)` on iPhone. The inserted hierarchy was inside the live tree with exact singleton/window/parent/state authority and zero rejected/initializer/native escape; rendering was synchronously stopped immediately after Gate C.

Gate D then failed before completing its frozen-confinement assertions with `System.InvalidOperationException: MoreThanOneMatch`. The launcher reflected all zero-argument methods named `GetParent` and used LINQ `Single(...)`; Godot exposes more than one such method shape.

0.0.170 changes only Gate-D bookkeeping. It enumerates zero-argument `GetParent` candidates and requires exactly one candidate that is not a generic method definition, contains no open generic parameters, and has a return type assignable from the actual live `SceneTree.Root` type. If no such method exists, the launcher fails explicitly and records the candidate method list.

No Step-39 compatibility-image transform, Gate-A/B policy, Gate-C insertion semantics, rendering policy, startup/native boundary, or game payload is changed.
