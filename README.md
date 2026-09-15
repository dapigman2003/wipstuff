# StS2Launcher — Steps 58–62

Active candidate: **0.0.208 (208)**. The current architecture uses **Step 52 as the prerequisite baseline for Step 58**; legacy Steps 53–57 remain optional regression/history diagnostics.

**0.0.208 is a profile-isolation refactor.** Physical 0.0.207 proved that the PropertyTweener GodotSharp derivative could be emitted, serialized, hash-verified, and loaded, but Step 35 then failed during bootstrap reflection over Godot-typed experimental bridge members. That was another candidate-construction failure before the intended Step-59 phone experiment, so the Step-59 experiment is no longer allowed to share one bootstrap-critical derivative.

One IPA now contains three independently generated GodotSharp profiles from the same verified prepared source:

- **BASELINE** — physically proven PackedScene unique-name compatibility only; no PropertyTweener experimental fields/helpers. Use this profile for **59T**.
- **WRAPPER** — Baseline + dedicated `TweenProperty` native-pointer/original-managed-wrapper observation and wrapper repair. Fluent returns remain natural. Use this profile for **59W**.
- **FULL** — Wrapper + typed-return `SetEase` / `SetTrans` / `FromCurrent` observation and fallback. Use this profile for **59X**, **59E**, and real **Step 59**.

Choose the matching **closed-path profile button before Step 35**. Each button runs Step 15 A–C → Step 35–37 → skips 38 → Step 39–52, ending frozen. Then run Step 58. A defect in Wrapper or Full no longer prevents a fresh-process Baseline run from reaching the previously proven path. Bootstrap reflects only primitive experimental telemetry/control fields; Godot-typed helper signatures are verified statically with Cecil and are not reflected during Step 35 load. Loader failures now record full exception/stack/FileName/FusionLog chains.

Recommended use of this compilation: fresh BASELINE → Step 58 → **59T**; fresh WRAPPER → Step 58 → **59W**; fresh FULL → Step 58 → **59X**. If 59X passes, use fresh FULL processes for **59E** and then real **Step 59**. If real Step 59 succeeds, continue **60 → 61 → 62** in that same process. 59D/R/H/U remain optional controls.

The source archive intentionally contains no proprietary StS2 game payload.
