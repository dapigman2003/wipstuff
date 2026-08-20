# Release Checklist — Step 24

## Source/package

- canonical live iOS project is `src/StS2Launcher.iOS/StS2Launcher.iOS.csproj`;
- no live legacy `StS2Launcher.Step05.iOS` path;
- `history.zip` is optional/inert and not needed by validation or build;
- no game payload, Steam reusable secrets, Apple signing secrets, or proprietary native game binaries in source/archive;
- Step 22.4.2 protected behavior remains intact;
- the physically closed Step 23.4.3 load-only implementation remains protected;
- Step 24 is additive: controlled initialization subsystem, host tests, isolated UI/reporting, release wiring, and current documentation/tooling updates;
- Step 24.0.1 corrects only the OfflineReady inspection API used by the Step 24 subsystem; no gate/resolver/initialization policy broadening is allowed.

## Static/host build

- `bash scripts/validate.sh` passes;
- `bash scripts/test.sh` passes;
- Godot build/preflight passes on Codemagic/macOS;
- iOS publish succeeds with `MtouchInterpreter=-all`, `UseInterpreter!=true`, `PublishAot!=true`;
- IPA verification passes;
- expected version is `0.0.74 (74)`;
- workflow is `ios-step-24`;
- expected IPA is `artifacts/StS2-Launcher-Step-24.ipa`.

## Device

- header `STEP 24 — CONTROLLED 0HARMONY MODULE INITIALIZATION BOUNDARY`;
- start from a fresh process;
- Gate A = exact sole initializer target `0Harmony 2.4.2.0`, one `<Module>..cctor`, bounded automatic-initialization closure (including implicit type constructors) fully measured, hazards 0;
- Gate B = exact accepted Step 23 initializer-free context replay, `0Harmony` still absent;
- Gate C = exact target load plus `RuntimeHelpers.RunModuleConstructor` completion barrier, zero native/unplanned requests;
- Gate D = exact post-initialization context/byte/OfflineReady audit;
- Step 24 = 4/4;
- `Reports/Step24-ControlledManagedInitialization.txt` exists;
- OfflineReady = PASS;
- Foundation 5/5 = PASS.

## Stop conditions

- If Gate A finds a second initializer-bearing dependency, identity/version drift, a reachable P/Invoke/`calli`/function-pointer or delegate indirection/native-loader/explicit runtime-constructor/reflection-dynamic edge, an unmeasured implicit type initializer, or any accepted Step 23 preflight regression, stop before Step 24 CLR loading and send the report.
- If Gate B fails to reproduce the exact Step 23 inert context, stop; do not admit `0Harmony`.
- If Gate C throws, requests native code, makes an unplanned managed request, or admits another initializer-bearing assembly, stop; do not broaden resolver/native policy or call a Harmony API.
- If Gate D detects byte/plan/OfflineReady/context drift, treat Step 24 as unclosed.
- After Gate B, force-quit before rerunning any fresh-process runtime regression.
