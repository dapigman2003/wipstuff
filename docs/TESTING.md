# Testing

Active candidate: `0.0.169 (169)`, IPA `StS2-Launcher-Step-39.ipa`, workflow `ios-canonical`.

Canonical validation must preserve the physical Step-36/37/38 closures and the exact ModelDb + inert-startup-wrapper + Step-38.2 compatibility transform. Step 39 must pin the four targeted PCK preflight resources; require a fresh process in which Step 38 was not invoked; require `NGame.Instance == null` before insertion; resolve the live `Engine.GetMainLoop()` / `SceneTree.Root`; enumerate the actual hierarchy; audit selected-sts2 `_EnterTree`, `_Ready`, and `_Notification` methods including in-module managed base chains; reject direct OneTimeInitialization/GameStartup/platform/main-menu/deferred/FMOD/Spine/Sentry/Steam references; perform exactly one `SceneTree.Root.AddChild(NGame)`; verify `IsInsideTree`, singleton, parent, `_window`, state 2, and zero initializer/rejected/native escape; call `StopRendering()` immediately after Gate C returns; never restart rendering or RemoveChild/Free/explicitly invoke `_ExitTree`; and keep proprietary game/native payloads out of the archive.

Host tests cover Step39 gate ordering, stable ordinals, failure sequencing, summary text, and pinned preflight authorities. Codemagic remains the first actual C# compiler for the new Step-39 iOS/core source in this environment.

Physical sequence: fresh process → Step 15 A-C → Step 35 MODEL-BOOTSTRAP 4/4 → Step 36.0.5 4/4 → Step 37.0.1 4/4 → **skip Step 38** → Step 39.0 A-D once. Relaunch after any Gate-C-started attempt.

## 0.0.169 Gate-B lifecycle compatibility

Physical 0.0.167 passed Gate A and failed closed in Gate B before AddChild on exact Steam cloud-capability and Sentry lifecycle-closure references. 0.0.169 keeps the same A-D SceneTree experiment but changes the selected private compatibility derivative: the two exact `SteamRemoteStorage` cloud probes in `SaveManager.ConstructDefault()` become false, the game-owned SentryService/nested helper surface becomes inert, and compiler-generated void helpers that directly call external `Sentry.*` methods are inerted. Serialization must reverify all of those properties before device use.

Device sequence remains fresh process: Step 15 A-C → Step 35.0.32 4/4 → Step 36.0.5 4/4 → Step 37.0.1 4/4 → skip Step 38 → Step 39.1 once. If Gate C starts, never retry in-process.

## 0.0.167 compile correction

Physical Codemagic 0.0.166 passed 1051/1051 static checks, 233/233 host tests, and the Step-15 native-link preflight, then failed before publish/device execution with CS0103 because the Step-39 UI partial omitted `using StS2Launcher.iOS.Platform;`. 0.0.167 changes only that import and release provenance.
