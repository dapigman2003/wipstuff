# Release checklist — Steps 53–57 single-player submenu continuation / 0.0.189

Release identity: display/build `0.0.189 (189)`, IPA `StS2-Launcher-Steps-53-57.ipa`, workflow `ios-canonical`.

Require stable physical baseline authority through Step 52 to remain unchanged, while retaining physical 0.0.187 evidence that Steps 53–56 also closed 4/4/frozen in sequence before the safe Step-57 Gate-A stop. The Physically Closed Path button must remain fresh-process-only, one-shot, and capped at Step 52 using the proven Step 15 A–C → 35 MODEL-BOOTSTRAP → 36 → 37 → skip 38 → 39–52 sequence.

Require Step 53 to bind/audit only exact zero-arg `NMainMenu.OpenSingleplayerSubmenu` returning `NSingleplayerSubmenu`, keep `SingleplayerButtonPressed` uninvoked, require immediate-frontier admissibility under retained runtime guards, and write its map before closure.

Require Step 54 to token-match the Step-53 runtime method, bind exact `NMainMenu.SubmenuStack : NMainMenuSubmenuStack` and exact stack field `_singleplayerSubmenu : NSingleplayerSubmenu`, and permit the lazily spawned field to be null before one-shot invocation. The invocation return must be reference-identical to the stack's post-open field; a pre-existing field value, if any, must retain identity. The binding map must be durable before invocation; rendering must remain frozen; success requires the real `NSingleplayerSubmenu` visible/in-tree and zero context/native drift. No in-process retry after arm.

Require Step 55 to audit the actual submenu subtree callbacks before rendering; persist the map first; allow exactly one `StartRendering`; request exactly 750 ms; use a 5000 ms post-stop evidence ceiling; synchronously call `StopRendering()` before telemetry; and require frozen retained submenu authority afterward.

Require Step 56 to map exact `void NSingleplayerSubmenu.OpenCharacterSelect(MegaCrit.Sts2.Core.Nodes.GodotExtensions.NButton)` without invocation, recheck the exact signature at Gate C before token/frontier use, record execution/deferred frontiers and unresolved/external-resolution failures, and keep any resource literals strictly diagnostic. Step 56 must not invoke or load character select.

Require Step 57 Gate A to bind exactly one selected-metadata `NMainMenuSubmenuStack._characterSelectScreenScene : Godot.PackedScene`, require the same exact field on the retained runtime stack, read only the already-loaded value's existing `ResourcePath`, require exact `res://scenes/screens/character_select_screen.tscn`, and require exactly one matching receipt-backed PCK directory entry under the sealed header/count authority. Gate B may then perform read-only extraction only. It must validate directory MD5, report SHA-256/size/flags/referenced resources and textual Spine/FMOD/GDExtension/dylib/dll occurrences, and never invoke `OpenCharacterSelect`, `ResourceLoader`, `PackedScene.Instantiate`, or rendering.

Keep original `GameStartup`, original `LaunchMainMenu`, cloud, migration mutation, platform initialization, external/native Steam, deferred startup, character-select loading/admission, native FMOD/Spine, trusted-install mutation, and Step 58+ unopened.

Keep the 0.0.183 AOT cache/sentinel telemetry and stable `ios-canonical` workflow unchanged. Require exact 0.0.189 source/plist/release/shell identity and retain physical 0.0.184/0.0.185/0.0.186/0.0.187/0.0.188 localization evidence; active manifests regenerated; protected manifests unchanged; no proprietary StS2/native payload; exact `history.zip`; final ZIP integrity clean; canonical validator green in release tree and fresh extraction.
