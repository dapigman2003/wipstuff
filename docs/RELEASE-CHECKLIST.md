# Release checklist — Steps 53–57 single-player submenu continuation / 0.0.185

Release identity: display/build `0.0.185 (185)`, IPA `StS2-Launcher-Steps-53-57.ipa`, workflow `ios-canonical`.

Require physical authority through Step 52 to remain unchanged. The Physically Closed Path button must remain fresh-process-only, one-shot, and capped at Step 52 using the proven Step 15 A–C → 35 MODEL-BOOTSTRAP → 36 → 37 → skip 38 → 39–52 sequence.

Require Step 53 to bind/audit only exact zero-arg `NMainMenu.OpenSingleplayerSubmenu` returning `NSingleplayerSubmenu`, keep `SingleplayerButtonPressed` uninvoked, require immediate-frontier admissibility under retained runtime guards, and write its map before closure.

Require Step 54 to token-match the Step-53 runtime method and exact `NMainMenu._singleplayerSubmenu` type before one-shot invocation; the invocation return must be reference-identical to that retained submenu. The binding map must be durable before invocation; rendering must remain frozen; success requires the same real `NSingleplayerSubmenu` visible/in-tree and zero context/native drift. No in-process retry after arm.

Require Step 55 to audit the actual submenu subtree callbacks before rendering; persist the map first; allow exactly one `StartRendering`; request exactly 750 ms; use a 5000 ms post-stop evidence ceiling; synchronously call `StopRendering()` before telemetry; and require frozen retained submenu authority afterward.

Require Step 56 to map exact zero-arg void `NSingleplayerSubmenu.OpenCharacterSelect` without invocation, record execution/deferred frontiers, unresolved/external-resolution failures, and resource-string/character-select-TSCN candidates.

Require Step 57 to perform read-only receipt-backed PCK extraction only. It must validate directory MD5, report SHA-256/size/flags/referenced resources and textual Spine/FMOD/GDExtension/dylib/dll occurrences, and never invoke `OpenCharacterSelect`, `ResourceLoader`, `PackedScene`, or rendering.

Keep original `GameStartup`, original `LaunchMainMenu`, cloud, migration mutation, platform initialization, external/native Steam, deferred startup, character-select loading/admission, native FMOD/Spine, trusted-install mutation, and Step 58+ unopened.

Keep the 0.0.183 AOT cache/sentinel telemetry and stable `ios-canonical` workflow unchanged. Require exact 0.0.185/184 source/plist/release/shell identity; active manifests regenerated; protected manifests unchanged; no proprietary StS2/native payload; exact `history.zip`; final ZIP integrity clean; canonical validator green in release tree and fresh extraction.
