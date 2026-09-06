# Release checklist — Step 35.0.32 / Step 36.0.3 / 0.0.157

Release identity: display/build `0.0.157 (157)`, IPA `StS2-Launcher-Step-36.ipa`, workflow `ios-canonical`.

- `ios-canonical` workflow key unchanged for cache reuse.
- Static validator passes with candidate manifests current and protected historical manifests unchanged.
- Host tests and native-link preflight pass.
- iOS Release publish/link and IPA verification pass.
- Bundle identity is exactly `0.0.157 (157)`.
- Device run uses fresh process: Step 15 A-C -> Step 35 MODEL-BOOTSTRAP 4/4 -> Step 36.0.3 once.
- Preserve run-correlated Step35 + Step36 artifacts before interpreting a result.
- Do not retry Step 36 Gate C in-process after it starts.
- Do not authorize ExecuteDeferred, PrewarmJit, game entry, native game loading, runtime Harmony/MonoMod, arbitrary resolver fallback, or state reset from this release.
