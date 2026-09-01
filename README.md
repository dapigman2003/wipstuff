# StS2 Launcher — Step 35

Active candidate: **Step 35.0.14 / 0.0.137 (137)** — comprehensive GodotSharp/native reconnaissance with dual diagnostic modes.

Steps 01–26 are closed. Step 27 is CLOSED NEGATIVE. Step 28 is CLOSED POSITIVE 5/5. Steps 29–34 are CLOSED POSITIVE 4/4. **Step 35 remains OPEN.** Exact-byte authority is unchanged: the natural target remains the closed Step-32 transformed `MegaCrit.Sts2.Core.Helpers.OneTimeInitialization::ExecuteVeryEarly()` image. Diagnostic derivatives can localize compatibility failures but cannot close exact Step 35.

Physical 0.0.136 finally entered `CommandLineHelper..cctor` with stack-neutral probes and emitted `INMETHOD_CL_CRITICAL_001_PRE` immediately before `Godot.Collections.Dictionary<string,string>` construction. No matching POST was durable. That localizes the measured hard-termination interval to the Godot dictionary constructor before `_args` assignment; the final `System.Collections.Concurrent 8 -> 9` resolver record remains contextual rather than causal.

This rebuilt 0.0.137 is designed to extract substantially more information per IPA. Gate A performs read-only reconnaissance over the exact OfflineReady depot, including GodotSharp critical IL, P/Invoke/calli/native-callback metadata and Mach-O dependency/rpath/symbol/string inventory. It also emits a separately hash-pinned **entry-only** GodotSharp diagnostic derivative. The app exposes two Step-35 modes that must be run in separate fresh processes: **NATURAL** preserves the original Godot dictionary so the instrumented GodotSharp path can localize the 0.0.136 failure from inside GodotSharp; **COMPAT** applies only the bounded four-reference BCL `Dictionary<string,string>` substitution so the same IPA can advance toward the still-natural `Godot.OS.GetCmdlineArgs()` boundary.

Resolver authority, initializer-bearing rejection, native-load refusal, no-Godot-startup rule, telemetry provenance, and exact Step-32 source isolation remain unchanged. The new reconnaissance report is output-only and never authorizes or performs native execution.

A diagnostic 4/4 result from either mode is **not Step-35 closure**. The source archive contains no proprietary game-managed or native payload.

Start with `docs/CURRENT-STATUS.md`, `docs/REGRESSION-CONTRACTS.md`, and `docs/TESTING.md`. Historical step/evidence records are under `docs/history/` and mirrored in `history.zip`.
