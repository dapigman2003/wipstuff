# Release checklist — Step 38.2 / 0.0.165

Release identity: display/build `0.0.165 (165)`, IPA `StS2-Launcher-Step-38.ipa`, workflow `ios-canonical`.

- Static validator passes completely in the working tree and a fresh extraction of the final ZIP.
- Codemagic host tests compile/pass under the pinned .NET SDK.
- Native-link preflight passes before iOS publish.
- IPA verifier reports the Step 38.2 UI identity.
- Existing `ios-canonical` NuGet/.NET/Godot/iOS caches remain enabled and unchanged.
- Physical 0.0.159 Step-36.0.5 4/4, physical 0.0.161 Step-37.0.1 4/4, physical 0.0.163 startup-wrapper boundary, and physical 0.0.164 Step-38.1 direct `_EnterTree` NRE evidence are preserved in history.
- The selected derivative preserves the proven ModelDb bootstrap and exact two-instruction inert `NGame.GameStartupWrapper()`.
- Step 38.2 requires exactly one original `_EnterTree -> SentryService.Initialize()` call and exactly one original `GetWindow/FilesDropped/Connect` block with no external branch entering the block, then replaces only those instructions with NOPs.
- Serialized verification preserves `_EnterTree` instruction count, requires the exact NOP delta, zero remaining Sentry/GetWindow/FilesDropped/Connect references, and exactly one inert-wrapper call.
- Step38 Gate A uses deferred rejecting-resolver Cecil and still rejects `_Ready`, `GameStartup`, `InitializePlatform`, `LaunchMainMenu`, deferred startup, and OneTimeInitialization re-entry.
- Gate B instantiates only the already-proven private FMOD-neutral PackedScene and keeps it off-tree.
- Gate C directly invokes only the verified Step-38.2 `NGame._EnterTree()` body once; no `AddChild` or `_Ready` invocation exists in Step38 code.
- Gate D requires off-tree/state-2/native confinement before releasing the temporary node without `_ExitTree`.
- No GameStartup, InitializePlatform, LaunchMainMenu, ExecuteDeferred, Steam init, native GDExtension load, gameplay, retry, or state reset.
- No proprietary game/native payload is present in the source ZIP.
