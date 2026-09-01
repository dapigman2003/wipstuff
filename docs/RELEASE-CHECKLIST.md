# Release checklist — Step 35.0.15 comprehensive 0.0.138

Release identity: display/build `0.0.138 (138)`, IPA `StS2-Launcher-Step-35.ipa`, workflow `ios-canonical`.

Before handoff/build:

- [ ] release identity is exactly `0.0.138 (138)` in csproj, Info.plist, shell release constants, UI source, testing docs and this checklist;
- [ ] `bash scripts/validate.sh` passes;
- [ ] `bash scripts/test.sh` passes on a host with `dotnet` with **209/209 or greater** if additional tests are added;
- [ ] the 0.0.137 verifier-only failure is documented as pre-device and not mistaken for runtime evidence;
- [ ] GodotSharp post-write entry-marker verification passes `GodotSharpDiagnosticBridgeTypeFullName` rather than the sts2 bridge constant;
- [ ] protected Step 29–34 manifests remain unchanged and valid;
- [ ] exact Step-32 transformed source authority is unchanged;
- [ ] physical 0.0.136 is documented as cctor entry + `CL_CRITICAL_001_PRE` with no POST, localizing to Godot string-dictionary construction before assignment;
- [ ] runtime CL/CLTV live-stack sweeps remain retired;
- [ ] four stack-neutral CommandLine critical markers remain;
- [ ] `Step35DiagnosticMode` exposes NATURAL and COMPAT in the same app, with fresh-process semantics;
- [ ] NATURAL preserves the exact Godot string dictionary contract;
- [ ] COMPAT applies exactly four managed dictionary substitutions, reuses existing `System.Collections`, preserves generic VAR MemberRefs, and leaves `Godot.OS.GetCmdlineArgs()` natural;
- [ ] a separate GodotSharp derivative preserves assembly identity/MVID and contains bounded entry-only probes, never live-stack sweeps;
- [ ] Gate A writes `Step35-GodotNativeReconnaissance-<RunId>.txt` before Gate B;
- [ ] reconnaissance is read-only and inventories GodotSharp IL/PInvoke/calli/callback fields plus native Mach-O dependencies/rpaths/bounded symbols/strings;
- [ ] exact prepared GodotSharp is hash-reverified before the separately hash-pinned derivative is selected;
- [ ] no Godot bootstrap, native game load, arbitrary resolver fallback, later startup phase, entry point, or runtime Harmony/MonoMod patching was introduced;
- [ ] no proprietary game DLLs, native app bundle, signing secret, or credential is included in the source archive.

For physical testing, run NATURAL and COMPAT in separate fresh processes. A 0.0.138 A–D 4/4 result from either mode is diagnostic completion only and cannot close exact Step 35.
