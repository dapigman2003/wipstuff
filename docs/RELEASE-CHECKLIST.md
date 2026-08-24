# Release Checklist — Step 27.0.23

## Source / policy

- Steps 01–26 remain closed/protected.
- Preserve all prior physical Step-27 evidence, including 0.0.105 `Enumerable.Union<T>` and 0.0.106 `DebuggableAttribute` trimming failures.
- `MtouchLink=None` and `TrimMode=copy` are the active dynamic-payload host policy; `TrimMode=full` is forbidden for this candidate.
- `MtouchInterpreter=-all` remains required; broad `UseInterpreter=true` and NativeAOT remain prohibited.
- Earlier measured TrimmerRootAssembly/DynamicDependency entries may remain as historical/protection descriptors but must not be claimed as the complete post-publish preservation mechanism.
- Raw Harmony normalization remains restricted to exact `0Harmony` 2.4.2 and changes only the admitted `HarmonySharedState::.cctor` slot in the in-memory image.
- Both production Cecil reads remain Deferred/read-only; whole-module Cecil serialization remains forbidden.
- T5a/T5b/T6 and T6a/T6b ordering remains unchanged; the single public `PatchProcessor.Patch()` acceptance call remains unchanged.
- Source/live/prepared game/Harmony files remain immutable.
- No StS2 member reflection, patching, or invocation; no Godot/game startup; no native game-library loading.
- The master document is revised only to replace the previously protected full-trim policy with the copy/no-link dynamic-payload policy.

## Build identity

- version: `0.0.107 (107)`
- workflow: `ios-step-27`
- IPA: `artifacts/StS2-Launcher-Step-27.ipa`
- TRX: `artifacts/test-results/step27.trx`
- top launcher banner: **Step 27.0.23**, bundle-derived **Version 0.0.107**, physical 0.0.106 `DebuggableAttribute` trimming diagnosis and copy/no-link policy.

## Device-run discipline

- Force-quit/relaunch before the run. Once Gate B starts, force-quit before any retry.
- Preserve `Step27-CrashCheckpoint.txt` before another attempt after abrupt termination.
- T6 = normalized shared state; T6a/T6b = exact LINQ closure; T7 = public `PatchProcessor.Patch()` entered.
- Another missing BCL member under copy/no-link would indicate a build-policy/tooling problem. A replacement-generation/dynamic-code or `PatchTools.DetourMethod -> DetourFactory.Current.CreateDetour` failure after trimming ambiguity is gone is the evidence relevant to the Step-28 pivot decision.

## Authority

Require static validation, the full Codemagic host suite including the hash-pinned real-Harmony normalizer test, iOS publish, and IPA verification PASS before installation. Physical A–Z expected **26/26**, then OfflineReady PASS and Foundation 5/5.
