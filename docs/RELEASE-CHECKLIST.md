# Release checklist — Step 38.0.1 / 0.0.163

Release identity: display/build `0.0.163 (163)`, IPA `StS2-Launcher-Step-38.ipa`, workflow `ios-canonical`.

- Static validator passes completely in the working tree and a fresh extraction of the final ZIP.
- Codemagic host tests compile/pass under the pinned .NET SDK.
- Native-link preflight passes before iOS publish.
- IPA verifier reports the Step 38.0.1 UI identity.
- Existing `ios-canonical` NuGet/.NET/Godot/iOS caches remain enabled and unchanged.
- Physical 0.0.159 Step-36.0.5 4/4 evidence and physical 0.0.161 Step-37.0.1 4/4 evidence are preserved in history.
- Step38 Gate A uses read-only Cecil against the exact selected compatibility image and rejects `_EnterTree` reachability into later startup/deferred boundaries.
- Gate B instantiates only the already-proven private FMOD-neutral PackedScene and keeps it off-tree.
- Gate C directly invokes only exact `NGame._EnterTree()` once; no `AddChild` or `_Ready` invocation exists in Step38 code.
- Gate D requires off-tree/state-2/native confinement before releasing the temporary node without `_ExitTree`.
- No GameStartup, InitializePlatform, LaunchMainMenu, ExecuteDeferred, Steam init, native GDExtension load, gameplay, retry, or state reset.
- No proprietary game/native payload is present in the source ZIP.
