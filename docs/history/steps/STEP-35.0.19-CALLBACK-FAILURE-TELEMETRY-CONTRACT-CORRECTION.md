# Step 35.0.19 — callback failure-telemetry contract correction

Candidate: 0.0.142 (142)

## Evidence requiring the correction

Codemagic for 0.0.141 passed 853 static checks and executed 211 host tests. 210 passed. The sole failure was `GodotCoreCallbackHandoffRejectsMissingTableBeforeAnyPreflightOrClrWork`: the test expected zero checkpoints for invalid callback-table metadata, while production emitted one `CB_INITIALIZE_MANAGED_FAIL` checkpoint.

## Diagnosis

`RunGodotCoreCallbackHandoffInitialization` rejects a null/empty/misaligned callback table before `RequirePreflight()`, `RequireAdmission()`, `RequireLoadContext()`, `CB_INIT_ENTRY`, GodotSharp loading, or `NativeFuncs.Initialize`. The method-level catch then deliberately writes one durable managed-failure checkpoint and rethrows. That telemetry is consistent with Step 35's durable crash-localization contract and does not constitute CLR admission or callback handoff.

## Correction

0.0.142 makes no runtime-semantic change to CORE-HANDOFF. The negative host regression now requires:

- `ArgumentException` for null/invalid callback metadata;
- exactly one checkpoint;
- that checkpoint contains `CB_INITIALIZE_MANAGED_FAIL`, `stage=initialization`, and `System.ArgumentException`;
- the checkpoint does not contain `CB_INIT_ENTRY`.

Static validation now pins this exact regression contract. Release identity advances to Step 35.0.19 / 0.0.142 (142), and the 0.0.141 Codemagic failure is preserved as immutable provenance.

## Unchanged authority

The three no-Godot controls and the Step15-live CORE-HANDOFF runtime experiment are otherwise unchanged. CORE-HANDOFF still obtains the source-built Godot 4.5.1 runtime-interoperability table, initializes only the verified private GodotSharp derivative once, and remains diagnostic-only. A 4/4 result cannot close exact Step 35.
