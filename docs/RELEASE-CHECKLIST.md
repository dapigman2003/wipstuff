# Release checklist — Steps 58–64 character-select admission/render continuation / 0.0.190

Release identity: display/build `0.0.190 (190)`, IPA `StS2-Launcher-Steps-58-64.ipa`, workflow `ios-canonical`.

Require physical authority through **Step 57 4/4/frozen** to be preserved, including the final 0.0.188 Step-56 and 0.0.189 Step-57 pass artifacts. The Physically Closed Path remains fresh-process-only and capped at Step 52.

Require the new block to preserve exact sequencing and authority:

- Step 58 maps exact closed generic `GetSubmenuType<NCharacterSelectScreen>()`; no invocation.
- Step 59 invokes only that exact factory once, while frozen, and proves exact retained off-tree screen identity plus actual lifecycle map.
- Step 60 maps exact zero-arg `void InitializeSingleplayer()`; no invocation.
- Step 61 invokes only that exact method once on the retained off-tree screen while frozen; zero escape/drift.
- Step 62 maps exact `void NSubmenuStack.Push(NSubmenu)` plus actual tree-entry lifecycle; no push.
- Step 63 invokes only exact Push once while frozen and proves exact visible/in-tree parent/identity authority.
- Step 64 audits actual in-tree frame/input callbacks, writes the map before rendering, authorizes exactly one 750 ms render residency with 5000 ms evidence ceiling, stops rendering before telemetry, and ends frozen.

Steps 59/61/63/64 must be one-shot with no in-process retry after arm. Original `OpenCharacterSelect`, character choice/confirm/embark/run-start, whole `GameStartup`/original `LaunchMainMenu`, cloud/migration/platform/Steam/deferred startup, native game extensions, trusted-install mutation, and Step 65+ remain unopened.

Require exact 0.0.190 source/plist/release/shell identity; current active manifests regenerated; protected manifests unchanged; no proprietary StS2/native payload in source archive; `history.zip` rebuilt from `docs/history`; final ZIP integrity clean; canonical validator green in the release tree and fresh extraction.
