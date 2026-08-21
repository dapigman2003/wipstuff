# Release Checklist — Step 25

## Source/package

- canonical live iOS project is `src/StS2Launcher.iOS/StS2Launcher.iOS.csproj`;
- no live legacy `StS2Launcher.Step05.iOS` path;
- `history.zip` is optional/inert and not needed by validation or build;
- no game payload, Steam reusable secrets, Apple signing secrets, or proprietary native game binaries in source/archive;
- Step 22.4.2 protected behavior remains intact;
- physically closed Step 23.4.3 load-only implementation remains protected;
- physically closed Step 24.0.6 controlled-initialization implementation remains protected;
- `System.Collections.Concurrent` remains the one physically proven Step-24 dynamic-IL preservation root, separately classified from the exact Step-22 22-root set;
- Step 25 adds only the targeted Harmony API/object-construction subsystem, host tests, isolated UI/reporting, release wiring, and current documentation/tooling updates;
- no Harmony patch/processor API, StS2 member reflection/invocation, Godot/game startup, or native game load is introduced.

## Static/host build

- `bash scripts/validate.sh` passes;
- `bash scripts/test.sh` passes;
- Godot build/preflight passes on Codemagic/macOS;
- iOS publish succeeds with `TrimMode=full`, `MtouchInterpreter=-all`, `UseInterpreter!=true`, `PublishAot!=true`;
- IPA verification passes;
- expected version is `0.0.80 (80)`;
- workflow is `ios-step-25`;
- expected IPA is `artifacts/StS2-Launcher-Step-25.ipa`;
- host TRX is `artifacts/test-results/step25.trx`.

## Device

- header identifies `STEP 25 — CONTROLLED HARMONY API RESOLUTION + INSTANCE CONSTRUCTION`;
- start from a fresh process;
- Gate A = exact closed Step-24 preconditions plus exact metadata-only Harmony `.cctor`/constructor/API audit, `HARMONY_DEBUG` absent, measured `DEBUG=false` branch, no blocking execution edge;
- Gate B = exact accepted Step-23 initializer-free context replay, `0Harmony` absent;
- Gate C = exact closed Step-24 target load + `RuntimeHelpers.RunModuleConstructor`, zero native/unplanned requests;
- Gate D = exact closed Step-24 post-initialization audit;
- Gate E = exact runtime `HarmonyLib.Harmony` + measured `.cctor` + `.ctor(string)` + `Id` + `DEBUG` resolution only; no `DEBUG` read, no type initialization, no construction;
- Gate F = exact measured Harmony type initializer completed with `RuntimeHelpers.RunClassConstructor`, `Harmony.DEBUG=false`, unchanged context/hash, zero native/unplanned requests;
- Gate G = exact post-type-initialization hash/context/resolver/DEBUG audit;
- Gate H = exact constructor invocation with fixed probe ID, exact object/type/context/ID/DEBUG verification, unchanged context membership/hash, zero native/unplanned requests;
- Gate I = exact post-construction plan/file/OfflineReady/context/object audit;
- Step 25 = **9/9**;
- `Reports/Step25-ControlledHarmonyConstruction.txt` exists;
- OfflineReady = PASS;
- Foundation 5/5 = PASS.

## Stop conditions

- If Gate A finds any Step-23/24 regression, target/API identity drift, changed/missing Harmony type initializer, constructor-shape drift, non-empty `HARMONY_DEBUG`, missing DEBUG guard, or blocking constructor execution edge, stop before Step-25 CLR loading and send the report.
- If Gate B fails to reproduce the exact Step-23 inert context, stop; do not admit `0Harmony`.
- If Gate C fails to reproduce the physically closed Step-24 module-initialization state, stop; do not resolve Harmony.
- If Gate D finds Step-24 byte/plan/OfflineReady/context drift, stop before targeted reflection.
- If Gate E resolves anything other than the exact measured Harmony API/type-initializer surface, stop before Gate F. Gate E must not read `DEBUG`, run the type initializer, or construct an object.
- If Gate F throws, produces `Harmony.DEBUG=true`, changes context membership/hash, causes native resolution, or makes an unplanned managed request, stop before construction.
- If Gate G detects post-type-initialization hash/context/resolver/DEBUG drift, stop before construction.
- If Gate H throws, changes context membership/hash, causes native resolution, or makes an unplanned managed request, stop; do not attempt patching or game reflection.
- If Gate I detects byte/plan/OfflineReady/context/object drift, treat Step 25 as unclosed.
- After Gate B, force-quit before rerunning any fresh-process runtime regression.
