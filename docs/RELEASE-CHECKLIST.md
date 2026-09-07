# Release checklist — Step 37.0 / 0.0.160

Release identity: display/build `0.0.160 (160)`, IPA `StS2-Launcher-Step-37.ipa`, workflow `ios-canonical`.

- Static validator passes completely.
- Codemagic host tests compile/pass under the pinned .NET SDK.
- Native-link preflight passes before iOS publish.
- IPA verifier reports the Step 37.0 UI identity.
- Existing `ios-canonical` NuGet/.NET/Godot/iOS caches remain enabled and unchanged.
- Physical 0.0.159 Step-36.0.5 4/4 evidence is preserved in history.
- Step37 pins exact game.tscn bytes/SHA-256/PCK-MD5/layout before any derivative work.
- Only a private copy is modified; Step-12 managed install/PCK are read-only.
- Only three FMOD-neutral scene edits are authorized.
- Gate C loads PackedScene only; Gate D instantiates off-tree and requires `IsInsideTree=false`.
- No AddChild, GameStartup, LaunchMainMenu, ExecuteDeferred, Steam init, native GDExtension load, gameplay, retry, or state reset.
- No proprietary game/native payload is present in the source ZIP.
