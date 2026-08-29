# Step 35.0.5 — Bounded Cecil Writer Resolution Fix

Candidate: **0.0.128 (128)**

## Basis
Physical 0.0.127 did not reach the Step-35 game boundary. Gate A failed normally while serializing the Step-35.0.4 diagnostic clone because the unconditional rejecting Cecil resolver refused `System.Runtime, Version=9.0.0.0` during constant-metadata emission.

This is not a new runtime compatibility failure. The project already encountered the same Cecil writer behavior during Step 32 and physically established an exact-identity, in-memory constant-metadata surrogate design that opens no external assembly bytes and rejects every unapproved metadata scope.

## Change
Step 35.0.5 keeps the Step-35.0.4 diagnostic-clone experiment intact but changes the clone writer path only:

1. open the exact closed Step-32 transformed image with the Step-32 audited constant-metadata writer resolver;
2. require zero dependency-resolution requests while opening/inspecting the module;
3. configure the resolver from the exact audited constant requirements (`System.Runtime` + `Sentry` enum/storage metadata only);
4. inject the same bridge and `INMETHOD_*` entry markers;
5. serialize the diagnostic clone, allowing only those exact in-memory writer-surrogate resolutions;
6. require the writer resolver to report only approved requests;
7. reopen the clone under the rejecting resolver;
8. require the post-write constant-metadata fingerprint to equal the pre-write exact transformed fingerprint;
9. continue existing identity/MVID/token/marker/hash verification and re-hash the exact transformed authority artifact unchanged.

The diagnostic clone filename advances to `sts2.step35.0.5.instrumented.dll` so any failed/partial 0.0.127 writer artifact cannot be mistaken for the new candidate.

## Authority
No runtime authority is broadened. Gate B/C still execute only a separately identified diagnostic derivative. Step 35 remains OPEN even if the derivative reaches 4/4. Exact transformed source bytes remain immutable and outside the CLR in this candidate. `ExecuteEssential`, `ExecuteDeferred`, launcher-driven `PrewarmJit`, game entry, Harmony/MonoMod patching, initializer-bearing `0Harmony`, unplanned managed/native resolution, native game loading, and Godot/game startup remain forbidden.

## Expected physical discriminator
If Gate A now passes, the run returns to the intended Step-35.0.4 localization experiment. The useful outcome is the final durable `INMETHOD_*` marker (or bridge-arm-without-marker) immediately before any hard termination. A managed Gate-A failure remains diagnostic and must not be interpreted as a game compatibility result.
