# Step 22.2 — Host Binding Frontier Correction

## Physical evidence from Step 22.1

Step 22.1 Gate A probed the 44-name framework frontier measured by Step 21.1:

- 26/44 probes qualified;
- all 22 direct `TrimmerRootAssembly` roots qualified;
- 18 probes failed;
- every failure was transitive-only;
- each failed both exact-identity and later simple-name load with `FileNotFoundException`.

The 18 misses are implementation dependencies referenced only from framework assemblies such as
`netstandard`, `System.Runtime.Serialization.Json/Xml`, and `System.Xml.XDocument`. They are not direct
references from the game/private binding frontier once those source framework assemblies bind to the iOS host.

## Step 22.2 correction

The original Gate A conflated two different things:

1. framework identities that the private/game graph must bind directly to the iOS host; and
2. implementation dependencies inside framework assemblies that become the host runtime's responsibility once
   the framework edge is host-bound.

Step 22.2 keeps the same measured 22 direct roots. Gate A now requires **22/22 direct roots**. It still probes
all 44 identities and writes the full report, but transitive-only misses are diagnostic rather than gate failures.

Gate B then recomputes the real `sts2.dll` dependency plan. That recomputed plan—not the desktop framework
implementation graph—is authoritative about whether any real binding blockers remain.

## Gates

### A — RootedHostAvailability

Required:

- 22/22 direct roots qualify by name/culture/token/version;
- no real StS2 assembly is CLR-loaded;
- full 44-name diagnostic report is written.

Report:

`Documents/StS2Launcher/Step22.2-HostBindingFrontierDiagnostics.txt`

### B — BindingClosureRecompute

Re-run the physically proven Step 21 planner against the now-qualified host roots. Residual blockers are reported
without being guessed or hidden.

### C — HostOnlyFrameworkPreparedSet

Require:

- explicit binding blockers = 0;
- runtime closure ready = YES;
- no private prepared `System.*` / `netstandard` assembly;
- no Cecil writes;
- no StS2 CLR load.

### D — IsolationAudit

Re-run source/prepared/live/plan integrity checks and require the same closed plan.

## Safety

The Step 21/21.1 binding engine remains byte-for-byte protected. The 22 root names are unchanged from the
physically tested Step 22.1 build. Step 22.2 changes the wrapper Gate A acceptance rule and diagnostics only;
it does not add more framework roots or relax Gate C/D closure requirements.
