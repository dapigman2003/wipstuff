# Release Checklist — Step 27.0.20

## Source / policy

- Steps 01–26 remain closed/protected.
- Preserve all prior physical Step-27 evidence, including 0.0.93/0.0.94 T5 cctor localization and the 0.0.97 Deferred-vs-Immediate Cecil failure.
- Both production normalizer reads must use `ReadingMode.Deferred`; `ReadingMode.Immediate` must remain absent from `ControlledHarmonyPatchExecution.cs`.
- The Step-27 metadata-only resolver remains fail-closed; do not whitelist `EditorBrowsableState` or other framework types.
- Production normalization remains restricted to exact `0Harmony` / `2.4.2.0` plus the exact original patch-engine metadata fingerprint. Internal randomized synthetic targets retain byte-identical passthrough.
- Source/live/prepared Harmony files remain immutable. Only the separately retained in-memory runtime image may contain the 11-instruction normalized `HarmonySharedState::.cctor`.
- The normalized cctor may only initialize `state`, `originals`, `originalsMono`, null `methodAddressRef`, set `actualVersion=102`, and return; it must not call `GetOrCreateSharedStateType`, `ReflectionHelper.Load`, or `FieldRefAccess`.
- T5a/T5b/T6/T7 ordering and the single public `PatchProcessor.Patch()` acceptance call remain unchanged.
- The official host structural surrogate comes only from `https://github.com/pardeike/Harmony/releases/download/v2.4.2.0/Harmony-Fat.2.4.2.0.zip`.
- Fixture provenance is content-addressed: archive SHA-256 must equal `a5fc5f9d9640b927d786a0527faa18bf7aa776788235140c59e9b73de87a7774`; extracted `net9.0/0Harmony.dll` SHA-256 must equal `a849b726e1f9248d71aabbed8114deaf79beb7acc25e8344ff92a27ad8ac87ab`.
- The script must still require exactly one root-or-wrapped `net9.0/0Harmony.dll` member and print all `0Harmony.dll` members on selection drift.
- The C# regression must independently re-hash the selected DLL, verify `0Harmony` 2.4.2.0 and the `EditorBrowsableAttribute` surface without reading constructor arguments, invoke `CreateIosNormalizedHarmonyRuntimeImage`, preserve source bytes, produce a byte-distinct image, and verify the exact 11-instruction cctor audit.
- The C# regression must not infer target framework or provenance from uniqueness/absence/version assumptions about `System.Runtime` or `netstandard` AssemblyRef rows.
- `ControlledHarmonyPatchExecution.cs` must remain byte-for-byte unchanged from 0.0.103.
- Crash checkpoints, `TrimMode=full`, `MtouchInterpreter=-all`, existing preservation roots, `UseInterpreter=true` prohibition, NativeAOT prohibition, and StS2/Game/Godot/native boundaries remain unchanged.
- The master document is unchanged.

## Build identity

- version: `0.0.104 (104)`
- workflow: `ios-step-27`
- IPA: `artifacts/StS2-Launcher-Step-27.ipa`
- TRX: `artifacts/test-results/step27.trx`
- top launcher banner: **Step 27.0.20**, bundle-derived **Version 0.0.104**, hash-pinned real-Harmony normalizer-execution status.

## Device-run discipline

- Force-quit/relaunch before the run. Once Gate B starts, force-quit before any retry.
- Preserve `Step27-CrashCheckpoint.txt` before another attempt after abrupt termination.
- T5a = normalized image reverified; T5b = normalized cctor entered; T6 = normalized cctor returned/validated; T7 = public `PatchProcessor.Patch()` entered.
- If T6 passes but T7/T8 fails, do not add another speculative Harmony-internal workaround: use the documented post-publish interpreted launcher-owned experiment, then pivot to ahead-of-load Cecil if that representative detour also fails.

## Authority

Require static validation, the full Codemagic host suite including the hash-pinned real-Harmony normalizer test, iOS publish, and IPA verification PASS before installation. Physical A–Z expected **26/26**, then OfflineReady PASS and Foundation 5/5.
