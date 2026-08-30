# StS2 Launcher iOS — Step 35.0.6 Deferred Cecil Open + In-Method Localization

Active candidate: **Step 35.0.6 / `0.0.129 (129)`**.

Physical 0.0.126 remains the authoritative runtime frontier: the same-run journal/map/manifest entered exact transformed `ExecuteVeryEarly()` and stopped after the planned `System.Collections.Concurrent` 8→9 host-resolution event with no `C_INVOKE_RETURNED`, localizing the hard kill to synchronous `MoveNext` work before the first incomplete await.

Physical 0.0.127 and 0.0.128 did **not** reach that game boundary. Both ended normally at Gate A with `AssemblyResolutionException` for `System.Runtime 9.0.0.0` while preparing the diagnostic clone. Analysis against the exact trusted game input (`sts2.dll` SHA-256 `e7ceb80669bfaf5c8fccabaa126ae2bb283aba514be5b5b55612579cfd285f18`) identified the 0.0.128 defect: the clone source was opened with Cecil `ReadingMode.Immediate` before the bounded writer resolver was configured.

0.0.129 changes only that diagnostic pre-write ordering. It mirrors the physically closed Step-32 pattern: **file-backed deferred module open → collect/audit external constant metadata → configure the exact in-memory `System.Runtime` + `Sentry` surrogates → write → reopen with rejecting resolution and reverify constant metadata**. No external game dependency bytes are opened by the writer resolver. The exact closed transformed source remains immutable and is re-hashed after clone emission and before Gate B.

The separate diagnostic clone still preserves assembly identity/MVID and carries output-only `INMETHOD_*` entry markers in `ExecuteVeryEarly.MoveNext`, the selected pre-first-await game methods, and relevant managed-IL type initializers. Gate C arms a launcher-owned `Action<string>` immediately before the one reflected diagnostic invocation.

After a physical run, collect `Step35-CurrentRun.txt`, `Step35-LastCheckpoint.txt`, the run-specific crash journal/static map named by the manifest, the normal managed report if present, and a matching `.ips` after any hard termination. The final durable `INMETHOD_*` line is the primary localization result.

**Authority:** a 0.0.129 A–D 4/4 result is diagnostic completion only. It cannot close exact Step 35 because Gate B/C execute an instrumented derivative rather than the exact closed transformed bytes. Step 35 remains OPEN until a separately defined authoritative transformed artifact passes its physical closure contract.
