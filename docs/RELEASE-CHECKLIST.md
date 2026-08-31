# Release checklist — Step 35.0.10

Release identity:

- display/build: `0.0.133 (133)`
- IPA: `StS2-Launcher-Step-35.ipa`
- active workflow: `ios-canonical`

Before handing off/building:

- [ ] release identity is exactly `0.0.133 (133)` in csproj, Info.plist, shell release constants, UI source, testing docs and this checklist;
- [ ] `bash scripts/validate.sh` passes;
- [ ] `bash scripts/test.sh` passes in a host with `dotnet`;
- [ ] protected Step 29–34 manifests remain unchanged and valid;
- [ ] current Step-35 candidate manifest matches all active candidate files;
- [ ] exact Step-32 transformed source hash/semantic authority is unchanged;
- [ ] same-run static map contains `[NULL PLATFORM CTOR IL]`, `[COMMAND LINE HELPER CCTOR IL]`, and `[COMMAND LINE HELPER TRYGETVALUE IL]`;
- [ ] CL cctor plan is required to contain `Godot.OS.GetCmdlineArgs`;
- [ ] injected diagnostic bridge calls do not consume exact-source CALLSITE ordinals;
- [ ] regression reproduces production entry-marker-before-sweep ordering from physical 0.0.132;
- [ ] CommandLine branch-target skip preserves exact-source ordinals and cannot silently skip required `Godot.OS.GetCmdlineArgs`;
- [ ] no Godot bootstrap/native game load/arbitrary resolver fallback/runtime Harmony patching was introduced;
- [ ] no proprietary `sts2.dll`, GodotSharp, Steamworks, Sentry, deps file, app bundle, signing secret or credential is included in the source archive.

For physical testing, force-quit before Step 35 and after any run where Gate B began. Preserve all matching run-correlated telemetry. Cancellation is INCONCLUSIVE. A 0.0.133 A–D 4/4 result is **diagnostic completion only** and cannot close exact Step 35. Do not broaden resolver/native/Harmony/Godot authority in this candidate.
