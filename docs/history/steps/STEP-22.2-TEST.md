# Step 22.2 Physical Test

1. Build `ios-step-22-2`.
2. Confirm:
   - `STEP 22.2 — HOST BINDING FRONTIER CORRECTION`
   - `Version 0.0.60`
3. Run:
   - `Run Step 22.2 A–D — Qualify 22 Roots → Recompute Closure → Prepare Host-Bound Set → Audit`
4. Stop at the first failing gate.

Gate A should now pass when the physically proven 22 direct roots qualify, even if some transitive-only diagnostic
probes remain unavailable. The full diagnostic file is:

`Files → On My iPhone → StS2 Launcher → StS2Launcher → Step22.2-HostBindingFrontierDiagnostics.txt`

If Gate B passes but Gate C fails, use `Export Current Runtime Binding Diagnostics to Files` and send the refreshed
`Step21.1-RuntimeBindingDiagnostics.txt`.

Success target:

`HOST FRAMEWORK CLOSURE FOUNDATION PASS — 4/4`

with:

- required host-binding roots: 22/22;
- explicit binding blockers: 0;
- runtime closure ready for first real CLR load: YES;
- no private prepared framework implementations;
- no StS2 CLR load/execution.

After 4/4, run OfflineReady and Foundation 5/5 for formal closure.
