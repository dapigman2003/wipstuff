# Release checklist — Step 35.0.14

Release identity:

- display/build: `0.0.137 (137)`
- IPA: `StS2-Launcher-Step-35.ipa`
- active workflow: `ios-canonical`

Before handoff/build:

- [ ] release identity is exactly `0.0.137 (137)` in csproj, Info.plist, shell release constants, UI source, testing docs and this checklist;
- [ ] `bash scripts/validate.sh` passes;
- [ ] `bash scripts/test.sh` passes on a host with `dotnet`;
- [ ] protected Step 29–34 manifests remain unchanged and valid;
- [ ] exact Step-32 transformed source authority is unchanged;
- [ ] physical 0.0.136 is documented as cctor entry + `CL_CRITICAL_001_PRE` with no POST, localizing to Godot string-dictionary construction before assignment;
- [ ] runtime CL/CLTV live-stack sweeps remain retired;
- [ ] four stack-neutral CommandLine critical markers remain;
- [ ] exactly four managed dictionary substitutions are applied and post-write verified;
- [ ] rewrite reuses the existing `System.Collections` AssemblyRef and generic VAR MemberRefs;
- [ ] natural `Godot.OS.GetCmdlineArgs()` remains exactly once;
- [ ] no Godot bootstrap, native game load, arbitrary resolver fallback, or runtime Harmony/MonoMod patching was introduced;
- [ ] no proprietary game DLLs, app bundle, signing secret, or credential is included in the source archive.

For physical testing, force-quit before Step 35 and after any run where Gate B began. A 0.0.137 A–D 4/4 result is diagnostic completion only and cannot close exact Step 35.
