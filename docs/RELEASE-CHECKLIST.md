# Release Checklist — Step 27.0.22

## Source / policy

- Steps 01–26 remain closed/protected.
- Preserve all prior physical Step-27 evidence, including the 0.0.105 on-device T7 `Enumerable.Union<T>` trimming failure.
- Both production Cecil reads must use `ReadingMode.Deferred`; `ReadingMode.Immediate` must remain absent from `ControlledHarmonyPatchExecution.cs`.
- `Mono.Cecil.ModuleDefinition.Write`, `module.Write`, and any whole-module Cecil serialization must remain absent from the production normalizer.
- The Step-27 metadata-only resolver remains fail-closed; do not whitelist `BindingFlags`, `EditorBrowsableState`, or other framework types.
- Production normalization remains restricted to exact `0Harmony` / `2.4.2.0` plus the exact original patch-engine metadata fingerprint. Internal randomized synthetic targets retain byte-identical passthrough.
- Source/live/prepared Harmony files remain immutable.
- Runtime normalization must clone the prepared source bytes and change only the original `HarmonySharedState::.cctor` method-body slot. Require IL-only input and reject `StrongNameSigned` source images before any byte substitution; reject any other managed-method RVA inside the admitted cctor storage.
- The raw patch must require a fat method header, no exception/extra sections, exact existing constructor/field tokens, enough slot capacity, and no byte changes outside the admitted cctor span.
- The replacement body remains exactly three `newobj`, five `stsfld`, one `ldnull`, one `ldc.i4 102`, and one `ret`; it must not call `GetOrCreateSharedStateType`, `ReflectionHelper.Load`, or `FieldRefAccess`.
- The post-patch Deferred Cecil audit must still report exactly 11 instructions.
- T5a/T5b/T6 ordering remains unchanged. Add T6a/T6b only between normalized shared-state validation and T7; the single public `PatchProcessor.Patch()` acceptance call remains unchanged.
- The official host structural surrogate comes only from `https://github.com/pardeike/Harmony/releases/download/v2.4.2.0/Harmony-Fat.2.4.2.0.zip`.
- Fixture provenance remains content-addressed: archive SHA-256 `a5fc5f9d9640b927d786a0527faa18bf7aa776788235140c59e9b73de87a7774`; extracted DLL SHA-256 `a849b726e1f9248d71aabbed8114deaf79beb7acc25e8344ff92a27ad8ac87ab`.
- The C# regression must independently re-hash the selected DLL, verify `0Harmony` 2.4.2.0 and `EditorBrowsableAttribute` surface, invoke the production normalizer, preserve source bytes, preserve exact PE length, produce a byte-distinct image, and verify the 11-instruction cctor.
- Crash checkpoints, `TrimMode=full`, `MtouchInterpreter=-all`, all earlier preservation roots, `UseInterpreter=true` prohibition, NativeAOT prohibition, and StS2/Game/Godot/native boundaries remain unchanged. Add exactly one candidate dynamic-payload whole-assembly root: `System.Linq`.
- The master document is unchanged.

## Build identity

- version: `0.0.106 (106)`
- workflow: `ios-step-27`
- IPA: `artifacts/StS2-Launcher-Step-27.ipa`
- TRX: `artifacts/test-results/step27.trx`
- top launcher banner: **Step 27.0.22**, bundle-derived **Version 0.0.106**, physical T7 System.Linq trimming diagnosis and preservation status.

## Device-run discipline

- Force-quit/relaunch before the run. Once Gate B starts, force-quit before any retry.
- Preserve `Step27-CrashCheckpoint.txt` before another attempt after abrupt termination.
- T5a = normalized image reverified; T5b = raw-body-normalized cctor entered; T6 = cctor returned/validated; T6a/T6b = exact linked `System.Linq` MethodCreator callable surface verified; T7 = public `PatchProcessor.Patch()` entered.
- A managed MissingMethodException caused by trimming does not satisfy the detour stop rule. Pivot only after the framework callable surface is proven and a representative patch reaches/fails the actual replacement/detour execution boundary for an iOS-specific reason.

## Authority

Require static validation, the full Codemagic host suite including the hash-pinned real-Harmony normalizer test, iOS publish, and IPA verification PASS before installation. Physical A–Z expected **26/26**, then OfflineReady PASS and Foundation 5/5.
