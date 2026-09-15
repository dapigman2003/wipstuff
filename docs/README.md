# StS2Launcher — Steps 58–62

Active candidate: **0.0.207 (207)**. Physical history remains closed through **Step 57**, while the active architecture uses **Step 52 as the prerequisite baseline for Step 58**. Legacy Steps 53–57 remain optional regression/history diagnostics rather than mandatory gameplay choreography.

Use **Run Physically Closed Path — Step 15 A–C → 35–37 → SKIP 38 → 39–52** from a fresh process. It ends frozen. Then run **Step 58 directly**.

Physical **0.0.203** isolated the runtime defect: 59U returned normally; 59T proved outline/image modulation, tween kill, and `CreateTween()` succeed, then `TweenProperty()` returns null; 59E reproduced the same `NConfirmButton.OnEnable()` failure outside `OpenCharacterSelect`. Physical **0.0.204** proved the first null-only wrapper fallback never activated. Physical **0.0.205** was rejected safely in Step-35 preflight because of a speculative fluent-method IL-shape assumption. **0.0.206 corrected that assumption and Codemagic successfully compiled the entire StS2Launcher.Core project**, but the pipeline stopped while compiling the host tests because one new assertion used the unsupported MSTest `Assert.Greater` API. No 0.0.206 game/runtime result exists.

**0.0.207 is a viability-hardening release, not another runtime rewrite.** The 0.0.206 Core runtime implementation is hash-frozen byte-for-byte. Only the host-test/build/release surface is corrected: the unsupported assertion is replaced with the already-used `Assert.IsTrue` form, and static validation now rejects assertion APIs outside the repository's known MSTest-v4 surface. This intentionally minimizes compile/runtime risk while preserving maximum experimental optionality.

The same IPA retains **59T CONTROL** (wrapper off, fluent off), **59W WRAPPER-ONLY** (wrapper on, fluent off), and **59X FULL REPAIR** (wrapper on, fluent on). If 59X passes, use a fresh process for **59E**; if 59E passes, use a fresh process for real **Step 59**. If real Step 59 succeeds, continue **60 → 61 → 62** without recompiling. 59D/R/H/U remain available controls.

Step 35's heavy GodotSharp telemetry remains enabled because it still provides useful managed/native corroboration. It may make diagnostic runs laggy. Character choice, embark/run start, continuous interactive ownership, and **Step 63+ remain unopened**.

The source archive intentionally contains no proprietary StS2 game payload.
