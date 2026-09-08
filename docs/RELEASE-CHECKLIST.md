# Release checklist — Step 39.1 / 0.0.169

Release identity: display/build `0.0.169 (169)`, IPA `StS2-Launcher-Step-39.ipa`, workflow `ios-canonical`.

- Static validator passes completely in the working tree and a fresh extraction of the final ZIP.
- Codemagic host tests compile/pass under the pinned .NET SDK; native-link preflight passes before iOS publish.
- IPA verifier reports the Step 39.1 UI identity.
- Existing `ios-canonical` NuGet/.NET/Godot/iOS caches remain enabled and unchanged.
- Physical Step-36.0.5, Step-37.0.1, and Step-38.2 4/4 evidence is preserved in history.
- No proprietary game resource/native bytes from the Step-39 preflight are stored in source; only hashes/derived audit notes are retained.
- Gate A re-verifies the selected compatibility derivative and exact four-resource PCK authority.
- The selected private derivative must force exactly the two physical 0.0.167 Steam cloud capability probes false and verify the game-owned Sentry wrapper/helper inert boundary after serialization.
- Gate B requires fresh NGame / `NGame.Instance == null`, resolves live SceneTree root, traverses the actual hierarchy, and audits immediate selected-sts2 lifecycle callbacks including in-module base types; surviving external Sentry/Steam/native/startup edges still fail closed.
- Gate C performs exactly one real `SceneTree.Root.AddChild(NGame)` and requires `IsInsideTree=true`, singleton exactness, parent exactness, `_window != null`, state 2, and zero initializer/rejected/native escape.
- The UI calls `GodotStep15NativeBridge.StopRendering()` immediately after Gate C returns, before recording/advancing; Step 39 never calls `StartRendering()`.
- Gate D requires rendering stopped and retains the inserted NGame in-tree; no RemoveChild, Free, or explicit `_ExitTree`.
- No GameStartup, InitializePlatform, LaunchMainMenu, ExecuteDeferred, Steam init, native GDExtension load, gameplay, retry, or state reset.
- Fresh-process test chain explicitly skips Step 38 before Step 39.
- No proprietary game/native payload is present in the source ZIP.

## 0.0.167 compile correction

Physical Codemagic 0.0.166 passed 1051/1051 static checks, 233/233 host tests, and the Step-15 native-link preflight, then failed before publish/device execution with CS0103 because the Step-39 UI partial omitted `using StS2Launcher.iOS.Platform;`. 0.0.167 changes only that import and release provenance.
