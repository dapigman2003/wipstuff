# Release checklist — Steps 43–47 / 0.0.176

Release identity: display/build `0.0.176 (176)`, IPA `StS2-Launcher-Steps-43-47.ipa`, workflow `ios-canonical`.

Require physical Step-42 4/4 provenance to be sealed, including exact InitPools token/22-method closure/zero-boundary map and one-shot return with zero context/native deltas.

Require the startup-ladder Core/UI/test surfaces: generic four-gate sequence with stable ordinals; five separately exposed rungs 43–47; same-process prior-rung authority checks; separate reports/checkpoints; Steps 43–46 frozen at entry; Step45 and Step47 one-shot lockout; Step46 complete async state-machine/MoveNext map durability before Step47; and Step47 refreeze-on-failure / leave-rendering-active-only-on-4/4 semantics.

Require no direct `GameStartup()` invocation, no `DoCloudSync`, no migration/archive mutation, no `InitializePlatform`, no external Steamworks/native Steam invocation, no `ExecuteDeferred`, no `LoadDeferredStartupAssets`, and no FMOD/Spine/native game-extension execution in the new ladder source. Step47 may invoke only exact audited `LaunchMainMenu(skipIntro=true)` after Step46 admissibility.

Require exact version/build 0.0.176/176 in csproj/plist/release presentation/current-release shell constants; Codemagic/host-test/IPA-verifier headings and filenames updated to Steps 43–47; active manifests regenerated; no proprietary StS2/native payload; final ZIP integrity clean; and canonical validator green again from a completely fresh extraction.
