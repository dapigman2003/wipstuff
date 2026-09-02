# StS2 Launcher — Step 35

Active candidate: **Step 35.0.17 / 0.0.140 (140)** — Godot callback-boundary reconnaissance plus a bounded managed command-line forward probe.

Steps 01–26 are closed. Step 27 is CLOSED NEGATIVE. Step 28 is CLOSED POSITIVE 5/5. Steps 29–34 are CLOSED POSITIVE 4/4. **Step 35 remains OPEN.** The exact transformed authority is unchanged: the natural target remains the closed Step-32 `MegaCrit.Sts2.Core.Helpers.OneTimeInitialization::ExecuteVeryEarly()` image. Diagnostic derivatives can measure compatibility frontiers but cannot close exact Step 35.

Physical **0.0.138** produced the key NATURAL/COMPAT split. NATURAL entered the GodotSharp Dictionary path through `NativeFuncs::godotsharp_dictionary_new(...)` and its last durable inner marker was `INMETHOD_GS014` in `CustomUnsafe::AsPointer`; `CL_CRITICAL_001_POST` never appeared. COMPAT applied the exact four-reference BCL `Dictionary<string,string>` substitution, emitted `CL_CRITICAL_001_POST`, then `CL_CRITICAL_002_PRE`, entered `Godot.OS::.cctor()` as `INMETHOD_GS033`, and terminated before `INMETHOD_GS032` (`Godot.OS.GetCmdlineArgs`) or `CL_CRITICAL_002_POST`.

The 0.0.138 reconnaissance maps both regions to `NativeFuncs._unmanagedCallbacks` function-pointer thunks. `NativeFuncs.Initialize` is the managed method that copies the callback table; its entry marker was not observed in either physical sequence. This is strong evidence that the current no-Godot-bootstrap Step-35 path is reaching GodotSharp native callback plumbing before that plumbing is initialized, but 0.0.138 did not log callback pointer values and therefore does not prove a single null-pointer root cause.

0.0.139 did **not** reach IPA packaging: Codemagic ran 210 host tests, passed 209, and failed only `OrderedDiagnosticLocalizationGatesReachFourOfFourWithoutClaimingClosure` because the test still expected the obsolete Step 35.0.15 summary while production correctly emitted Step 35.0.16. Step 35.0.17 / 0.0.140 corrects that release/test-consistency defect only; the 0.0.139 runtime experiment is unchanged.

0.0.140 preserves the execution policy and offers three fresh-process diagnostic modes in one IPA:

- **NATURAL** — original Godot Dictionary and natural Godot.OS path; retained as a control.
- **OS-RECON** (`ManagedDictionaryCompatibility`) — the physically proven four-reference BCL Dictionary substitution, with natural `Godot.OS.GetCmdlineArgs()` retained. GodotSharp entry-only closure instrumentation is expanded from `Godot.OS::.cctor()` / `OS.MethodName::.cctor()` through StringName/ClassDB/NativeFuncs local callees.
- **FORWARD** (`ManagedCommandLineCompatibility`) — the same four Dictionary substitutions plus **exactly one** replacement of `Godot.OS.GetCmdlineArgs()` in `CommandLineHelper..cctor` by a launcher-injected provider verified after serialization to be exactly `new string[0]`.

FORWARD is intentionally diagnostic; an empty argument array is not declared to be final product semantics. No native image is loaded by reconnaissance, runtime native resolution still fails closed, and no Godot bootstrap, later startup phase, arbitrary resolver fallback, or Harmony/MonoMod runtime patching is authorized.

Start with `docs/CURRENT-STATUS.md`, `docs/REGRESSION-CONTRACTS.md`, and `docs/TESTING.md`. Historical step/evidence records are under `docs/history/` and mirrored in `history.zip`. The source archive contains no proprietary game-managed/native payload.
