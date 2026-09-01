# Release checklist — Step 35.0.16 / 0.0.139

Release identity: display/build `0.0.139 (139)`, IPA `StS2-Launcher-Step-35.ipa`, workflow `ios-canonical`.

Before handoff/build:

- [ ] release identity is exactly `0.0.139 (139)` in csproj, Info.plist, shell release constants, UI source, testing docs and this checklist;
- [ ] `bash scripts/validate.sh` passes;
- [ ] `bash scripts/test.sh` passes on a host with `dotnet`; expected count is 210 or greater after the new regressions;
- [ ] protected Step 29–34 manifests remain unchanged and valid;
- [ ] exact Step-32 transformed source authority and tokens `0x06007D02` / `0x0600BC71` remain unchanged;
- [ ] physical 0.0.138 NATURAL/COMPAT evidence is documented: NATURAL last inner marker GS014; COMPAT has `CL_CRITICAL_001_POST`, `CL_CRITICAL_002_PRE`, GS033 OS cctor, no GS032/GetCmdlineArgs;
- [ ] runtime live-stack CL/CLTV sweeps remain retired and four stack-neutral CommandLine critical markers remain;
- [ ] `Step35DiagnosticMode` exposes NATURAL, OS-RECON (`ManagedDictionaryCompatibility`), and FORWARD (`ManagedCommandLineCompatibility`);
- [ ] NATURAL preserves the exact Godot string Dictionary and natural GetCmdlineArgs;
- [ ] OS-RECON applies exactly four managed Dictionary substitutions and leaves natural GetCmdlineArgs exactly once;
- [ ] FORWARD applies the same four Dictionary substitutions plus exactly one managed command-line provider substitution;
- [ ] FORWARD post-write verification requires zero natural `Godot.OS.GetCmdlineArgs` calls in CommandLineHelper, exactly one local provider call, and provider body exactly `new string[0]`;
- [ ] GodotSharp derivative preserves identity/MVID, uses only derivative-specific entry markers, and expands the OS-cctor/StringName/ClassDB/NativeFuncs closure without live-stack instrumentation;
- [ ] Gate A writes same-run static/reconnaissance outputs before Gate B and re-hashes exact sts2/GodotSharp sources unchanged;
- [ ] runtime native resolution remains rejected; no Godot bootstrap, native game load, arbitrary resolver fallback, later OneTimeInitialization phase, entry point, or Harmony/MonoMod runtime patching is introduced;
- [ ] no proprietary game DLLs/native app bundle/signing secret/credential is included in the source archive.

Physical testing: run OS-RECON first, force-quit/relaunch, then FORWARD. NATURAL is optional regression confirmation. A 0.0.139 A–D 4/4 result from any mode is diagnostic completion only and cannot close exact Step 35.
