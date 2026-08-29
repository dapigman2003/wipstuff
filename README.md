# StS2 Launcher iOS — Step 35.0.4 In-Method Pre-First-Await Localization

Active candidate: **Step 35.0.4 / `0.0.127 (127)`**.

Physical 0.0.126 proved the run-correlated telemetry fix: the journal, static map, current-run manifest, and last-checkpoint all carried one Run ID/PID. The run again entered exact transformed `ExecuteVeryEarly` and ended after the planned `System.Collections.Concurrent` 8→9 host-resolution marker with no `C_INVOKE_RETURNED`.

0.0.127 preserves and re-verifies the exact closed Step-32 transformed source and the Step-35 resolver/later-boundary policy, but creates a **separate diagnostic clone**. The clone preserves assembly identity/MVID and receives output-only `INMETHOD_*` entry markers in `ExecuteVeryEarly.MoveNext`, the top-level pre-first-await game methods, and relevant managed-IL type initializers. Gate C arms a launcher-owned `Action<string>` callback immediately before the one reflected diagnostic invocation. The exact transformed source is immediately re-hashed after clone emission, is never overwritten, and is never CLR-loaded by this candidate.

After a physical hard termination, collect `Step35-CurrentRun.txt`, `Step35-LastCheckpoint.txt`, the run-specific crash journal/static map named by the manifest, and the matching `.ips`. The final durable `INMETHOD_*` line is the primary localization result.

**Authority:** a 0.0.127 A–D 4/4 result is diagnostic completion only. It cannot close exact Step 35 because Gate B/C execute an instrumented derivative rather than the exact closed transformed bytes. Step 35 remains OPEN until a separately defined authoritative transformed artifact passes its physical closure contract.
