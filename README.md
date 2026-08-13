# StS2 Launcher iOS — Step 01.1

This is the corrected **ground-zero UIKit bootstrap test**.

## Why Step 01.1 exists

Step 01 installed successfully but immediately terminated on launch.

Reviewing the bootstrap found a concrete lifecycle bug: the scene configuration requested a
`UIWindowScene`, but `SceneDelegate` inherited from `UISceneDelegate`.

For a window scene, the delegate must conform to `UIWindowSceneDelegate`. In .NET iOS the
correct base class is `UIWindowSceneDelegate`, which also exposes the Objective-C exported
`Window` property UIKit expects.

Step 01.1 changes only that startup boundary (plus version/test identifiers).

## Scope

Contains only:

- .NET 9 iOS
- UIKit/Foundation
- AppDelegate
- correctly typed UIWindowSceneDelegate
- one UIKit view controller
- one test-log button
- unsigned IPA packaging

Still contains no Godot, SteamKit2, Mono.Cecil, Keychain integration, native game/runtime
libraries, game files, or runtime patching.

## Expected physical-device result

Install:

`StS2-Launcher-Step-01.1.ipa`

Then launch it.

Expected visible screen:

```text
StS2 Launcher

STEP 01.1 — UI BOOTSTRAP PASS

Version 0.0.2

Status: UI rendered successfully.

Lifecycle: Active

[ Write Test Log ]
```

The screen should be white.

Tap `Write Test Log`; the status should change to `PASS: test log written at ...`.

Then test background/foreground and terminate/reopen.

See `docs/STEP-01.1-TEST.md`.
