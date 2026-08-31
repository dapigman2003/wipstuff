# Release checklist — Step 35.0.13

Release identity:

- display/build: `0.0.136 (136)`
- IPA: `StS2-Launcher-Step-35.ipa`
- active workflow: `ios-canonical`

Before handing off/building:

- [ ] release identity is exactly `0.0.136 (136)` in csproj, Info.plist, shell release constants, UI source, testing docs and this checklist;
- [ ] `bash scripts/validate.sh` passes;
- [ ] `bash scripts/test.sh` passes in a host with `dotnet`;
- [ ] protected Step 29–34 manifests remain unchanged and valid;
- [ ] current Step-35 candidate manifest matches all active candidate files;
- [ ] exact Step-32 transformed source hash/semantic authority is unchanged;
- [ ] physical 0.0.133 is documented as managed `InvalidProgramException` instrumentation failure with normal `RUN_END`, not as a Godot compatibility result;
- [ ] same-run static map contains `[NULL PLATFORM CTOR IL]`, `[COMMAND LINE HELPER CCTOR IL]`, and `[COMMAND LINE HELPER TRYGETVALUE IL]`, with CommandLine cctor exact-source MaxStack;
- [ ] generic live-stack marker sweeps reserve one extra MaxStack slot;
- [ ] Gate-A post-write verification requires CommandLine cctor diagnostic MaxStack = exact-source MaxStack + 1;
- [ ] executable host regression CLR-loads and executes a tight-MaxStack rewritten cctor;
- [ ] four stack-neutral critical CommandLine markers bracket dictionary construction/assignment and `Godot.OS.GetCmdlineArgs` invocation/result storage;
- [ ] CL cctor plan is required to contain `Godot.OS.GetCmdlineArgs`;
- [ ] injected diagnostic bridge calls do not consume exact-source CALLSITE ordinals;
- [ ] CommandLine branch-target skip preserves exact-source ordinals and cannot silently skip required `Godot.OS.GetCmdlineArgs`;
- [ ] no Godot bootstrap/native game load/arbitrary resolver fallback/runtime Harmony patching was introduced;
- [ ] no proprietary `sts2.dll`, GodotSharp, Steamworks, Sentry, deps file, app bundle, signing secret or credential is included in the source archive.

For physical testing, force-quit before Step 35 and after any run where Gate B began. Preserve all matching run-correlated telemetry. Cancellation is INCONCLUSIVE. A 0.0.136 A–D 4/4 result is **diagnostic completion only** and cannot close exact Step 35. Do not broaden resolver/native/Harmony/Godot authority in this candidate.
