# Step 59.14 — PropertyTweener profile isolation — 0.0.208

Physical 0.0.207 did **not** reach Step 59. It proved the revised GodotSharp derivative could be emitted, serialized, independently reopened, hash-verified, selected by the strict private resolver, and loaded. Step-35 Gate A passed and Gate B admitted the model-bootstrap sts2 derivative. During the managed callback handoff, PackedScene compatibility armed successfully, then bootstrap reflection over the experimental PropertyTweener bridge raised `FileNotFoundException` before the PropertyTweener arm checkpoint. The closed path stopped fail-closed and invoked no later stage.

This crossed the project's viability threshold for refactoring the concentrated Step-35/GodotSharp experiment debt. The bootstrap architecture itself is retained, as are the physically proven PackedScene correction and Step52→58 ownership architecture. The change is that Step-59 experimental rewrites no longer share one mandatory GodotSharp derivative.

0.0.208 generates three independent derivatives from the same verified prepared GodotSharp source:

- **Baseline**: entry diagnostics + physically proven `PackedScene.Instantiate` unique-name compatibility only. It contains no PropertyTweener experimental fields or helper methods.
- **Wrapper**: Baseline + the dedicated `TweenProperty` native helper observation/repair. It records the native pointer and original managed object before repair. `SetEase`, `SetTrans`, and `FromCurrent` remain natural.
- **Full**: Wrapper + return-boundary observation/repair for the three fluent PropertyTweener methods. The transform does not assume their internal native-call/cast shape.

The selected profile is sealed before Step-35 Gate A and is process-scoped. The iOS UI exposes three fresh-process closed-path buttons. Step 59T requires Baseline, 59W requires Wrapper, and 59X/59E/real Step 59 require Full. Wrong-profile probes fail before mutation.

Bootstrap-critical loading no longer reflects the Godot-typed experimental helper methods. Baseline reflects no PropertyTweener experiment members. Wrapper/Full bootstrap only inspect primitive `bool`/`int` control/telemetry fields; Godot-typed helper signatures are a static Cecil serialization contract. This directly removes the 0.0.207 reflection surface.

Exception telemetry is also strengthened: Step-35 managed-load failures preserve nested exception type/message, `FileName`, `FusionLog`, and stack information so another loader failure is actionable instead of appearing as an empty `FileNotFoundException`.

The intended compilation-efficient physical sequence is:

1. fresh Baseline closed path → Step 58 → 59T;
2. fresh Wrapper closed path → Step 58 → 59W;
3. fresh Full closed path → Step 58 → 59X;
4. if 59X passes, fresh Full → 58 → 59E;
5. if 59E passes, fresh Full → 58 → real 59;
6. if real 59 closes, continue 60 → 61 → 62 in that same process.

This candidate does not claim a PropertyTweener runtime fix until physical evidence reaches the actual Step-59 probes.
