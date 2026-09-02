# Release checklist — Step 35.0.20 / 0.0.143

Release identity: display/build `0.0.143 (143)`, IPA `StS2-Launcher-Step-35.ipa`, workflow `ios-canonical`.

0.0.143 is a pre-device compile-integration correction over 0.0.142: runtime behavior is unchanged; the Step-35 iOS partial now explicitly imports `StS2Launcher.iOS.Platform` so `GodotStep15NativeBridge` resolves during the iOS compile.

Before handoff/build:

- [ ] release identity is exactly `0.0.143 (143)` in csproj, Info.plist, shell release constants, UI source, testing docs and this checklist;
- [ ] `bash scripts/validate.sh` passes;
- [ ] `bash scripts/test.sh` passes on a host with `dotnet`; expected count is 211 or greater after the new callback-handoff regression;
- [ ] pinned Godot 4.5.1 iOS source builds with `module_mono_enabled=yes` and standalone native-link preflight passes;
- [ ] protected Step 29–34 manifests remain unchanged and valid;
- [ ] exact Step-32 authority and tokens `0x06007D02` / `0x0600BC71` remain unchanged;
- [ ] physical 0.0.140 NATURAL/OS-RECON/FORWARD evidence is documented, including GS031 NATURAL, OS cctor→StringName→GS024, and FORWARD `CL_CRITICAL_002_POST` / `NP002_POST` / GodotFileIo→DirAccess→StringName→GS024;
- [ ] `Step35DiagnosticMode` exposes NATURAL, OS-RECON, FORWARD, and CORE-HANDOFF;
- [ ] NATURAL preserves natural Godot Dictionary/GetCmdlineArgs; OS-RECON retains exactly four managed Dictionary substitutions; FORWARD adds exactly one verified `new string[0]` provider substitution;
- [ ] Step-15 smoke `project.godot` contains no `dotnet` feature;
- [ ] native Step-15 bridge roots `sts2_step15_is_runtime_interop_ready`, `sts2_step15_has_dotnet_feature`, `sts2_step15_is_dotnet_runtime_initialized`, and `sts2_step15_get_runtime_interop_funcs` are ReferenceNativeSymbol/link-preflight roots;
- [ ] callback export requires live Engine/ProjectSettings/CSharpLanguage/GDMono native state and returns only the exact `godotsharp::get_runtime_interop_funcs` pointer/size after null/alignment/null-entry checks;
- [ ] CORE-HANDOFF UI refuses if the Step-15 engine is not setup-complete, if the smoke project advertises `dotnet`, or if Godot's own .NET runtime is initialized;
- [ ] managed handoff binds exact private GodotSharp `NativeFuncs.Initialize(IntPtr,int)`, requires `initialized=false`, invokes once, requires `initialized=true`, and freezes resolver/native counters before Gate C;
- [ ] runtime native **game** resolution remains rejected; no game executable/library, later OneTimeInitialization phase, entry point, arbitrary resolver fallback, or Harmony/MonoMod runtime patching is introduced;
- [ ] no proprietary game DLL/native app bundle/signing secret/credential is included in the source archive.

Physical testing for the new mode: fresh process → Step 15 Gates A–C → without force-quitting, Step 35 CORE-HANDOFF once. The old three controls need not be rerun unless regression confirmation is desired. A 0.0.143 A–D 4/4 result is diagnostic completion only and cannot close exact Step 35.
