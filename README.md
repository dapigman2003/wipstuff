# StS2Launcher — Steps 58–62

Active candidate: **0.0.206 (206)**. Physical history remains closed through **Step 57**, while the active architecture uses **Step 52 as the prerequisite baseline for Step 58**. Legacy Steps 53–57 remain optional regression/history diagnostics rather than mandatory gameplay choreography.

Use **Run Physically Closed Path — Step 15 A–C → 35–37 → SKIP 38 → 39–52** from a fresh process. It ends frozen. Then run **Step 58 directly**.

Physical **0.0.203** isolated the current runtime defect: 59U returned normally; 59T proved outline/image modulation, tween kill, and `CreateTween()` succeed, then `TweenProperty()` returns null; 59E reproduced the same `NConfirmButton.OnEnable()` failure outside `OpenCharacterSelect`. Physical **0.0.204** proved the first null-only wrapper fallback never activated. Physical **0.0.205** never reached Step 59 because Step-35 preflight rejected a speculative assumption that `PropertyTweener.SetEase()` had a particular post-native `isinst` IL shape. That rejection is treated as a candidate-construction failure, not a game regression.

**0.0.206 removes that internal-shape assumption entirely and maximizes information/optionality per compilation.** The dedicated `TweenProperty` helper still records the native pointer plus managed-wrapper null/wrong-type identity and can repair a nonzero-pointer unusable wrapper. `SetEase`, `SetTrans`, and `FromCurrent` are now observed/repaired only at their typed `PropertyTweener` return boundaries, independent of their internal generated native-call/cast shape.

The same IPA exposes three fresh-process tail experiments: **59T CONTROL** (wrapper off, fluent off), **59W WRAPPER-ONLY** (wrapper on, fluent off), and **59X FULL REPAIR** (wrapper on, fluent on). If 59X passes, use a fresh process for **59E**; if 59E passes, use a fresh process for real **Step 59**. If real Step 59 succeeds, continue **60 → 61 → 62** without recompiling. 59D/R/H/U remain available controls.

Step 35's heavy GodotSharp telemetry remains enabled because it still provides useful managed/native corroboration. It may make diagnostic runs laggy. Character choice, embark/run start, continuous interactive ownership, and **Step 63+ remain unopened**.

The source archive intentionally contains no proprietary StS2 game payload.
