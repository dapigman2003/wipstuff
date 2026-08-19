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

`artifacts/StS2-Launcher-Step-22.4.ipa`

## IPA verification

```sh
bash scripts/verify-ipa.sh artifacts/StS2-Launcher-Step-22.4.ipa
```

Output:

`artifacts/reports/ipa-verification.txt`

## Codemagic

Workflow:

`ios-step-22-4`

Authoritative entry point:

```sh
bash scripts/codemagic.sh
```

The pipeline runs static validation, host tests, iOS workload setup, Godot build/preflight, iOS publish, and final IPA verification.

## Physical acceptance for Step 22.4

Install version `0.0.62` and require:

1. Step 22 A–D = 4/4;
2. explicit binding blockers = 0;
3. runtime closure ready for first real CLR load = YES;
4. OfflineReady = PASS;
5. Foundation 5/5 = PASS.

Confirm the corresponding reports exist in Files under `On My iPhone → StS2 Launcher → StS2Launcher`.

Do not begin the real `sts2.dll` load subsystem until this acceptance passes.
