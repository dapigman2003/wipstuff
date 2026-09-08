# Release checklist — Step 38.1 / 0.0.164

Release identity: display/build `0.0.164 (164)`, IPA `StS2-Launcher-Step-38.ipa`, workflow `ios-canonical`.

- Static validator passes completely in the working tree and a fresh extraction of the final ZIP.
- Codemagic host tests compile/pass under the pinned .NET SDK.
- Native-link preflight passes before iOS publish.
- IPA verifier reports the Step 38.1 UI identity.
- Existing `ios-canonical` NuGet/.NET/Godot/iOS caches remain enabled and unchanged.
- Physical 0.0.159 Step-36.0.5 4/4, physical 0.0.161 Step-37.0.1 4/4, and physical 0.0.163 Step-38 Gate-A startup-wrapper boundary evidence are preserved in history.
- The pre-load selected derivative preserves the proven ModelDb bootstrap and additionally rewrites only `NGame.GameStartupWrapper()` to `call Task.get_CompletedTask; ret`, reusing an existing exact sts2 MemberRef.
- Serialized verification requires the wrapper to have exactly two instructions, no locals/handlers, the same method token, and unchanged `_EnterTree` to retain exactly one direct wrapper call.
- Step38 Gate A uses deferred rejecting-resolver Cecil, verifies the inert wrapper, permits that wrapper only, and rejects `_Ready`, `GameStartup`, `InitializePlatform`, `LaunchMainMenu`, deferred startup, and OneTimeInitialization re-entry.
- Gate B instantiates only the already-proven private FMOD-neutral PackedScene and keeps it off-tree.
- Gate C directly invokes only exact unchanged `NGame._EnterTree()` once; no `AddChild` or `_Ready` invocation exists in Step38 code.
- Gate D requires off-tree/state-2/native confinement before releasing the temporary node without `_ExitTree`.
- No GameStartup, InitializePlatform, LaunchMainMenu, ExecuteDeferred, Steam init, native GDExtension load, gameplay, retry, or state reset.
- No proprietary game/native payload is present in the source ZIP.
