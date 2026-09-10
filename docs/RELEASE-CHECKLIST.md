# Release checklist — Steps 43–52 physically closed stabilization / 0.0.183

Release identity: display/build `0.0.183 (183)`, IPA `StS2-Launcher-Steps-43-52.ipa`, workflow `ios-canonical`.

Require the current physical status to state that Step 49 is closed and Steps 50–52 are user-reported 4/4 on a fresh rerun; do not fabricate missing successful rerun report files. Seal the supplied first Step-50 checkpoint/static-map/final-report/last-checkpoint as the durable timing-only failure evidence.

Require the Physically Closed Path control to be fresh-process-only and one-shot. It must call existing implementations in this exact order: Step 15 A–C; Step 35.0.32 MODEL-BOOTSTRAP; 36; 37; explicit skip of 38; 39; 40.1; 41; 42; 43 through 52. It must require each exact pass/closure predicate before advancing, stop immediately on failure, preserve normal per-step reports, write `PhysicallyClosedPath-ToStep52.txt`, and require final rendering frozen. Step 15 Gate D must not be auto-run.

Require Step 50 target to remain 100 ms and stop-before-telemetry ordering unchanged. Require a Step-50-specific evidence ceiling of exactly 4000 ms. Require historical Step 40 maximum to remain exactly 2000 ms and Step 52 to remain exactly 1500/6000 ms.

Require Codemagic workflow ID `ios-canonical` and `mac_mini_m2` to remain stable. Keep existing NuGet/Godot/.NET/obj caches and add only canonical `bin/Release/net9.0-ios/ios-arm64/AOTCompileInputs.cache` and `AOTCompileInputs.cache.uptodate`, not the whole bin tree. Require pre/post marker hash/size/mtime telemetry and post-publish binlog counts for AOT-up-to-date messages, LLVM opt, LLVM llc, and missing-sentinel diagnostics.

Require no Step 53+ implementation or UI control. Original whole `GameStartup`, original `LaunchMainMenu`, single-player handler invocation, `DoCloudSync`, migration/archive mutation, `InitializePlatform`, deferred startup/`ExecuteDeferred`, external/native Steam, and native FMOD/Spine remain unopened.

Require exact version/build `0.0.183/183` in csproj/plist/release presentation/current-release shell constants; active candidate manifests regenerated; protected manifests unchanged; no proprietary StS2/native payload; exact `history.zip`; final ZIP integrity clean; canonical validator green from both the release tree and a completely fresh extraction.
