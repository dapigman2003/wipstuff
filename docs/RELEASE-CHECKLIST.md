# Release checklist — Step 40.0 / 0.0.171

Release identity: display/build `0.0.171 (171)`, IPA `StS2-Launcher-Step-40.ipa`, workflow `ios-canonical`.

Before publishing:
- canonical static validator passes from the release tree and a clean extracted archive;
- host tests compile/pass under the pinned Codemagic toolchain;
- native-link preflight remains unchanged and passes;
- IPA verifier reports Step 40.0 UI identity and no proprietary StS2 payload;
- Step39 compatibility and SceneTree code remains hash-pinned, with physical 0.0.170 4/4 reports sealed in history;
- Step40 has exactly one launcher `StartRendering()` call and one mandatory `StopRendering()` call, with no restart after stop;
- Step40 Gate B uses deferred/rejecting Cecil and fail-closes before render restart on forbidden/unresolved frame/input closure edges;
- target pulse is 100 ms and maximum accepted elapsed is 500 ms;
- GameStartup/platform/main-menu/ExecuteDeferred/Steam/native GDExtensions/explicit `_ExitTree`/RemoveChild/Free remain unauthorized;
- source archive contains no proprietary game payload, Steam secrets, Apple signing secrets, or proprietary game-native binaries;
- `history.zip` is resealed from `docs/history/` after the final source edits;
- final ZIP hash is recorded after clean-extraction validation.
