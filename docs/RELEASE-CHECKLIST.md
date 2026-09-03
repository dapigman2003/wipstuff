# Release checklist — Step 35.0.26 / 0.0.149

Release identity: display/build `0.0.149 (149)`, IPA `StS2-Launcher-Step-35.ipa`, workflow `ios-canonical`.

Physical 0.0.146 proved the bridge. 0.0.147 added the exact post-bootstrap resolver seal, but Codemagic stopped at 212/213 host tests solely because one negative test required a stale `preflight` substring. 0.0.148 corrected the host-test contract and has reached Gate D on-device. 0.0.149 preserves the bridge/Gate-C contract, adds live Gate-D forwarded file/byte progress plus heartbeat liveness, and caches/verifies the pinned Codemagic .NET+iOS workload toolchain.

Before handoff/build:

- [ ] release identity is exactly `0.0.149 (149)` in csproj, Info.plist, shell release constants, UI source, testing docs and this checklist;
- [ ] `bash scripts/validate.sh` passes;
- [ ] `bash scripts/test.sh` passes on a host with `dotnet`;
- [ ] pinned Godot 4.5.1 iOS source builds with `module_mono_enabled=yes` and standalone native-link preflight passes;
- [ ] protected Step 29–34 manifests remain unchanged and valid;
- [ ] exact Step-32 authority and tokens `0x06007D02` / `0x0600BC71` remain unchanged;
- [ ] physical 0.0.145 reverse-binding preflight is documented with `csharpLanguage=True`, `godotApiCacheUpdated=False`, `createManagedBindingCallback=False`, `reverseBindingReady=False`, `godotDotNetInitialized=False`, and normal pre-Gate-C stop;
- [ ] `Step35DiagnosticMode` still exposes NATURAL, OS-RECON, FORWARD, and CORE-HANDOFF;
- [ ] Step-15 smoke `project.godot` contains no `dotnet` feature;
- [ ] native Step-15 bridge roots the callback-table exports plus `sts2_step15_get_managed_callbacks_size`, `sts2_step15_install_external_managed_callbacks`, `sts2_step15_is_external_managed_bridge_installed`, `sts2_step15_signal_external_core_api_loaded`, and `sts2_step15_did_external_core_api_signal_return`;
- [ ] managed bootstrap verifies `GodotPlugins.Game.Main.InitializeFromGameProject` with unmanaged entry point `godotsharp_game_main_init` but does not directly invoke that `UnmanagedCallersOnly` method;
- [ ] private GodotSharp `ManagedCallbacks` is verified as exactly 37 unmanaged function-pointer fields and `ManagedCallbacks.Create(IntPtr)` must return all non-null pointers;
- [ ] `ScriptManagerBridge.LookupScriptsInAssembly` is called only on the already admitted diagnostic sts2 assembly;
- [ ] native cache adoption accepts only the complete exact-size callback struct, is single-shot, requires no Godot-owned initialized .NET runtime, and uses `GDMonoCache::update_godot_api_cache` rather than assigning individual callbacks;
- [ ] `GD_OnCoreApiAssemblyLoaded` is its own durable boundary after cache adoption;
- [ ] launcher never writes/fakes `GDMono::runtime_initialized` or `initialized` ownership state;
- [ ] runtime native **game** resolution remains rejected; no game executable/library, later OneTimeInitialization phase, entry point, arbitrary resolver fallback, or Harmony/MonoMod runtime patching is introduced;
- [ ] Gate D forwards the protected `SteamOfflineInstallProgress` checkpoints into Step-35 progress without modifying the physically protected Step-13 verifier;
- [ ] Step-35 UI includes a dedicated Gate-D progress bar/latest-file/throughput surface plus a one-second elapsed/last-progress heartbeat;
- [ ] Codemagic caches `$HOME/.dotnet` and validates the exact `9.0.314.3` iOS workload marker before skipping workload installation;
- [ ] no proprietary game DLL/native app bundle/signing secret/credential is included in the source archive.

Physical testing: fresh process → Step 15 Gates A–C → without force-quitting, Step 35 CORE-HANDOFF once. Preserve the run-correlated reports. A 0.0.149 A–D 4/4 result is diagnostic completion only and cannot close exact Step 35.
