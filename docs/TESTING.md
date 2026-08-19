# Testing — Canonical Foundation

## Current principle

Text files are the primary handoff format for long diagnostics. Screenshots are useful for short UI state only.

## Static validation

```sh
bash scripts/validate.sh
```

Output:

`artifacts/reports/static-validation.txt`

The validator checks only authoritative current source/docs/tooling. It must **not** depend on `history.zip` or any legacy `StS2Launcher.Step05.iOS` path.

## Host unit tests

```sh
bash scripts/test.sh
```

Output:

`artifacts/reports/host-unit-tests.txt`

Detailed test results remain under `artifacts/test-results/`.

## Godot native preflight

On macOS/Xcode:

```sh
bash scripts/build-godot.sh
bash scripts/preflight-godot-link.sh
```

Shareable preflight output:

`artifacts/reports/godot-native-preflight.txt`

## iOS build

On macOS/Xcode with the pinned .NET SDK/workload available:

```sh
bash scripts/build-ios.sh
```

Expected IPA:

`artifacts/StS2-Launcher-Step-22.4.2.ipa`

## IPA verification

```sh
bash scripts/verify-ipa.sh artifacts/StS2-Launcher-Step-22.4.2.ipa
```

Output:

`artifacts/reports/ipa-verification.txt`

## Codemagic

Workflow:

`ios-step-22-4-2`

Authoritative entry point:

```sh
bash scripts/codemagic.sh
```

The pipeline runs static validation, host tests, iOS workload setup, Godot build/preflight, iOS publish, and final IPA verification.

## Physical acceptance for Step 22.4.2

Install version `0.0.64` and require:

1. Step 19 A–D = 4/4;
   - `Compile()` = 42;
   - `Compile(false)` = 42;
   - `Compile(true)` = 42;
   - on iOS, `RuntimeFeature.IsDynamicCodeCompiled=false`;
   - `RuntimeFeature.IsDynamicCodeSupported` is diagnostic and may be true in the canonical Step-20+ interpreter-enabled runtime;
2. Step 22 A–D = 4/4;
3. explicit binding blockers = 0;
4. runtime closure ready for first real CLR load = YES;
5. OfflineReady = PASS;
6. Foundation 5/5 = PASS.

Confirm the corresponding reports exist in Files under `On My iPhone → StS2 Launcher → StS2Launcher`.

If Step 19 fails, send `Reports/Step19-ExpressionInterpreter.txt` rather than relying on a screenshot.

Do not begin the real `sts2.dll` load subsystem until this acceptance passes.
