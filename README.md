# StS2 Launcher iOS — Step 01

This is the **ground-zero device bootstrap test**.

## Scope

Step 01 contains only:

- .NET 9 iOS
- UIKit/Foundation
- an AppDelegate
- a SceneDelegate
- one native UIKit view controller
- a tiny file-write test
- unsigned IPA packaging for Sideloadly

It intentionally contains **no**:

- Godot
- SteamKit2
- Mono.Cecil
- Keychain code
- native static libraries
- game files
- custom AOT/interpreter settings
- runtime patching
- engine overlays

## Expected physical-device result

After installing `StS2-Launcher-Step-01.ipa` and opening it, the app must show a **white screen** with:

- `StS2 Launcher`
- `STEP 01 — UI BOOTSTRAP PASS`
- `Version 0.0.1`
- `Status: UI rendered successfully.`
- a `Write Test Log` button

Tap **Write Test Log**.

The status must change to a message beginning with:

`PASS: test log written`

Then:

1. send the app to the background;
2. return to it;
3. terminate it from the app switcher;
4. reopen it.

The launcher screen must return each time.

See `docs/STEP-01-TEST.md` for the exact pass/fail contract.

## Build in Codemagic

Use workflow:

`ios-step-01`

Expected artifact:

`artifacts/StS2-Launcher-Step-01.ipa`

The IPA is intentionally unsigned so it can be locally re-signed/sideloaded.

## Local macOS build

```bash
bash scripts/codemagic-build.sh
```

or, if the correct .NET/iOS workload is already installed:

```bash
bash scripts/build-step01.sh
bash scripts/verify-step01-ipa.sh artifacts/StS2-Launcher-Step-01.ipa
```
