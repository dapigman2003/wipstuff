# Release checklist — Step 35.0.32 / Step 36.0.4 / 0.0.158

Release identity: display/build `0.0.158 (158)`, IPA `StS2-Launcher-Step-36.ipa`, workflow `ios-canonical`.

- Static validator passes completely.
- Host test suite passes under the pinned .NET SDK in Codemagic.
- Native-link preflight passes before iOS publish.
- IPA verifier passes and reports the Step 35.0.32 / Step 36.0.4 UI identity.
- Bundle identity is exactly `0.0.158 (158)`.
- Existing `ios-canonical` NuGet/.NET/Godot/iOS caches remain enabled and unchanged.
- Device run uses a fresh process: Step 15 A-C -> Step 35 MODEL-BOOTSTRAP 4/4 -> Step 36.0.4 once.
- No retry after Step-36 Gate C begins; no `_state` reset.
- No ExecuteDeferred, launcher PrewarmJit, game entry, native game image, runtime Harmony/MonoMod, or arbitrary resolver fallback.
- Preserve all run-correlated Step35/Step36 telemetry.
