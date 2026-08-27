# Step 27 — Controlled Launcher-Owned Harmony Patch + Unpatch

## Starting evidence

Physical Step 26.0 / `0.0.83 (83)` closed the empty `PatchProcessor` boundary **14/14**, followed by OfflineReady PASS and Foundation 5/5.

That means the next unresolved Harmony boundary is actual method replacement. Step 27 keeps the target entirely launcher-owned so Harmony/AOT/runtime patch-engine behavior can be characterized before any real StS2 member is reflected or patched.

## Objective

Prove one complete, reversible Harmony prefix lifecycle against a deterministic launcher-owned static method:

- original launcher method behavior is measured first;
- exact patch metadata is admitted fail-closed;
- one exact prefix is registered;
- exactly one `PatchProcessor.Patch()` crosses the patch-engine boundary;
- patched behavior is observed through reflection and direct calls;
- exactly that prefix is removed;
- original behavior is observed again;
- hashes, OfflineReady, private-context membership, managed resolution, and native refusal remain intact throughout.

## Launcher-owned probe

`HarmonyPatchProbe.Target(int value)`:

- increments a target-body counter;
- returns `value + 1`;
- is marked `NoInlining | NoOptimization`.

`HarmonyPatchProbe.Prefix(int value, ref int __result)`:

- increments a prefix counter;
- writes `__result = value + 1000`;
- returns `false` so Harmony must skip the original body;
- is marked `NoInlining | NoOptimization`.

The fixed test input is `41`:

- baseline/restored result = `42`;
- patched result = `1041`.

## Gates

### A–N — exact Step 26 replay

Reproduce the complete physically closed Step 26 chain, ending with the exact empty `PatchProcessor` in the same private context used by the new patch gates.

### O — HarmonyPatchApiResolution

Before constructing a patch descriptor, Cecil-audit and runtime-resolve only:

- `PatchProcessor.AddPrefix(MethodInfo)`;
- `PatchProcessor.Patch() -> MethodInfo`;
- `PatchProcessor.Unpatch(MethodInfo)`;
- `HarmonyMethod(MethodInfo)`;
- `PatchProcessor.prefix`;
- `HarmonyMethod.method`.

Require the measured Harmony 2.4.2 structural call flow and reject P/Invoke or metadata drift. No patch method is invoked.

### P — LauncherPatchProbeResolution

Resolve only the exact launcher-owned target/prefix pair in the host/default load context. Require exact types and parameter names `value` and `__result`. No invocation.

### Q — BaselineProbeInvocation

Reset counters. Call `Target(41)` directly and by `MethodInfo.Invoke`. Require both results = 42, target calls = 2, prefix calls = 0.

### R — PrefixRegistration

Invoke only `AddPrefix(MethodInfo)` and verify the resulting exact `HarmonyMethod` retains the exact launcher prefix. Require unchanged 0Harmony bytes/context/native/resolver state and unchanged probe counters. `Patch()` is still not invoked.

### S — PatchEngineExecution

Invoke exactly one `PatchProcessor.Patch()`. Require a replacement `MethodInfo`, unchanged `0Harmony` bytes, unchanged private-context membership, zero native attempts, zero rejected managed requests, and unchanged probe counters. Do not invoke the patched target yet.

### T — PostPatchAudit

Re-hash plan/prepared/live bytes, re-prove OfflineReady, and require exact Step-26 private-context/native/resolver state before any patched invocation.

### U — PatchedProbeInvocation

Invoke target first through reflection, then directly. Require result 1041 on both routes. Prefix count must increase twice; original target-body count must not increase.

### V — ExactPrefixUnpatch

Invoke exactly `PatchProcessor.Unpatch(prefix MethodInfo)` and require same processor identity, unchanged bytes/context, zero native/rejected requests, and unchanged counters.

### W — PostUnpatchAudit

Audit target hash/context/native/resolver state before restored invocation.

### X — RestoredProbeInvocation

Invoke target through reflection and direct routes. Require result 42 on both, original target-body count advances twice, and prefix count does not advance.

### Y — FinalIsolationAudit

Final plan/prepared/live rehash, OfflineReady exact-tree proof, exact context membership, zero native/rejected requests, exact retained object identities, `Harmony.DEBUG=false`, and exact restored-behavior snapshot.

## Explicitly still out of scope

- `Harmony.Patch`, `PatchAll`, categories, patch-class discovery;
- postfix/transpiler/finalizer/inner patch registration;
- any StS2 type/member reflection, patching, or invocation;
- the StS2 entry point;
- Godot/game startup;
- native game-library loading;
- trusted/prepared game-byte mutation.

## Candidate identity

- step: **27.0**
- version: **0.0.84 (84)**
- workflow: **`ios-step-27`**
- IPA: **`artifacts/StS2-Launcher-Step-27.ipa`**
- device report: `Documents/StS2Launcher/Reports/Step27-ControlledHarmonyPatchExecution.txt`

## Physical result — 0.0.84 (84)

The first physical Step 27 run reached **17/25**. Gates A–Q passed. Gate R failed during exact `PatchProcessor.AddPrefix(MethodInfo)` before `Patch()` was called.

The stack established a previously implicit execution boundary:

`AddPrefix(MethodInfo)` → `HarmonyMethod(MethodInfo)` → `HarmonyMethod.ImportMethod` → `HarmonyMethodExtensions.CopyTo` → `HarmonyMethod.HarmonyFields()` → automatic `HarmonyLib.AccessTools::.cctor` → `NullReferenceException`.

No launcher patch was installed and no StS2 member was reflected or invoked. The raw report is preserved at `docs/history/reports/STEP-27.0-PHYSICAL-GATE-R-REPORT.txt`.

The next candidate does not weaken prefix or patch policy. It first metadata-audits the exact `AccessTools` static initializer and gives that automatic initialization its own explicit gate before retrying prefix registration.

## Physical metadata refinement — 0.0.85 (85)

Step 27.0.1 failed at Gate O before AccessTools execution and exposed the real AccessTools runtime-detection/cache surface. The 0.0.86 candidate initially interpreted that evidence as a 56-instruction fingerprint; physical build 86 later corrected the count to 57. This does not supersede build 84's A–Q execution evidence; it records how the preflight model was refined before the explicit AccessTools gate.


## Physical fingerprint refinement — 0.0.86 (86)

Step 27.0.2 failed safely at Gate O before AccessTools execution or patching. Its stricter opcode audit corrected the prior 56-instruction interpretation: the receipt-backed initializer is 57 instructions and includes one required `ldc.i4.1`. Step 27.0.3 / 0.0.87 then confirmed the 57-instruction fingerprint but disproved the initial operand attribution: both `RuntimeInformation` `Type.GetType(string,bool)` calls use `false`, while the sole `ldc.i4.1` supplies `LockRecursionPolicy.SupportsRecursion` to `ReaderWriterLockSlim`. Step 27.0.4 / 0.0.88 pins those exact semantics. Gate R remains the explicit AccessTools initializer and Gate T remains the first patch call.

## Step 27.0.5 crash-localization note

Physical 0.0.88 produced intermittent abrupt termination around N–Q without a managed report, plus one expected Gate-A stale-process rejection after a prior Step-27 context had already been loaded. Step 27.0.5 / 0.0.89 keeps A–Z unchanged but adds a synchronously flushed crash checkpoint at every gate transition and sensitive O/R/S/T substages. Gate O no longer invokes the reflected `RuntimeInformation.FrameworkDescription` getter; Gate R owns that first reflected execution immediately before AccessTools type initialization. The first patch remains Gate T.

## Step 27.0.6 / 0.0.90

Physical 0.0.89 crash checkpoint localized hard termination to Gate S/S1 inside `AddPrefix(MethodInfo)`. The next candidate keeps AddPrefix as exact Cecil-audited reference behavior but does not invoke it for the annotation-free launcher prefix. Gate S instead builds the equivalent descriptor via exact `HarmonyMethod()`, verifies default state, assigns only `method`, and assigns only `PatchProcessor.prefix`; Gate T remains the first actual patch call.

## Step 27.0.7 / 0.0.91

Physical 0.0.90 advanced through the bounded Gate-S descriptor path and hard-terminated after the Gate-T/T1 crash checkpoint entered the first exact public `PatchProcessor.Patch()` invocation. No T2 checkpoint or launcher-target invocation survived. This physically moves the frontier inside the patch engine but does not identify the failing internal operation.

The 0.0.91 candidate keeps public `PatchProcessor.Patch()` as the acceptance boundary. Gate O adds receipt-backed audits for the exact HarmonySharedState -> MethodCreator -> MonoMod detour -> UpdatePatchInfo chain and bounded host Reflection.Emit/MethodHandle preservation. Gate T first makes HarmonySharedState initialization explicit at T1/T2, then invokes public Patch exactly once at T3/T4 and validates the replacement at T5. A hard stop can therefore distinguish shared-state initialization from the remaining replacement/detour path while still grouping related consecutive boundaries in one device run.

The detailed candidate record is `STEP-27.0.7-HARMONY-SHARED-STATE-INITIALIZATION-AND-PATCH-ENGINE-PRESERVATION.md`. The master document is intentionally unchanged.

## Physical 0.0.91 result + Step 27.0.8 / 0.0.92

Physical 0.0.91 did **not** reach Gate T. Gates A–N replayed successfully, then Gate O failed normally at 14/26 with `Targeted patch API reflection unexpectedly changed resolver/load counters.` No crash occurred and no later gate ran. The new Gate-O runtime work relative to the physically passing 0.0.90 surface was the HarmonySharedState Type/.cctor/version-field reflection, so the result proves that this reflection has an observable loader/resolver effect on the physical iOS runtime. It does not prove anything about HarmonySharedState initialization or PatchProcessor.Patch() beyond the earlier 0.0.90 evidence. The full report is preserved at `docs/history/reports/STEP-27.0.7-PHYSICAL-GATE-O-REPORT.txt`.

Step 27.0.8 / 0.0.92 corrects the regression without weakening Gate O. Gate O keeps the new receipt-backed HarmonySharedState/replacement/detour **Cecil metadata audit** but restores runtime reflection to the 0.0.90 PatchProcessor/HarmonyMethod/AccessTools surface. Gate T now owns and measures the new runtime surfaces: T1/T2 bounded Reflection.Emit/RuntimeMethodHandle host preflight, T3/T4 exact HarmonySharedState runtime Type/.cctor/version reflection with exact resolver/load deltas, T5/T6 explicit shared-state initialization/version/generated-assembly validation, T7/T8 the single public PatchProcessor.Patch() call, and T9 replacement/isolation validation. The public patch call remains the acceptance boundary and the launcher target remains uninvoked until Gate V.

The detailed candidate record is `STEP-27.0.8-GATE-O-PURITY-RESTORATION-AND-T-RUNTIME-RESOLUTION.md`. The master document is intentionally unchanged.

## Supplied 2026-08-22 legacy-S1 checkpoint + Step 27.0.9 / 0.0.93

A newly supplied crash checkpoint has a fresh UTC timestamp but reports `S1 — entering exact PatchProcessor.AddPrefix(MethodInfo) reflection invocation.` That executable text belongs to physical 0.0.89 and is absent from candidate 0.0.92, whose Gate S enters the bounded parameterless `HarmonyMethod()` descriptor path and explicitly does not invoke AddPrefix or ImportMethod. Because the old checkpoint format did not carry bundle/candidate provenance, this observation cannot be attributed to the uploaded 0.0.92 source and does not change the physical runtime frontier.

Step 27.0.9 / 0.0.93 therefore makes no patch-engine behavior change. It adds a fail-closed bundle/source release-identity check before any Step-27 gate and writes installed version/build, expected source version/build, active candidate identity, and the bounded Gate-S implementation marker into every synchronously flushed crash checkpoint. Gate O, the bounded Gate-S descriptor, and Gate T T1–T9 remain runtime-identical to 0.0.92.

The detailed candidate record is `STEP-27.0.9-CRASH-CHECKPOINT-RELEASE-PROVENANCE-HARDENING.md`. The master document is intentionally unchanged.

## Physical 0.0.93 T5 checkpoint + Step 27.0.10 / 0.0.94

The self-identifying 0.0.93 checkpoint proves that T1–T4 returned and that the abrupt termination occurs after entry into `RuntimeHelpers.RunClassConstructor(HarmonySharedState.TypeHandle)` but before T6. `PatchProcessor.Patch()` and the launcher target remained uninvoked.

Step 27.0.10 / 0.0.94 leaves that cctor call and the later public Patch() path intact. T5a requires no pre-existing generated shared-state/proxy assembly and arms output-only observation of the dedicated Step-27 ALC plus relevant process `AssemblyLoad` events; T5b enters the unchanged cctor. The observer is removed before T6 validation if the cctor returns. This is crash localization, not a workaround or acceptance-path substitution.

## Physical 0.0.94 T5 observer checkpoint + Step 27.0.11 / 0.0.95

Physical 0.0.94 again terminated inside `HarmonySharedState::.cctor`; its last checkpoint records successful dedicated-ALC host resolution of `netstandard 2.0.0.0` to host `netstandard 2.1.0.0`. `PatchProcessor.Patch()` and the launcher target remained uninvoked. This rules out the observed netstandard resolution as the immediate failure and keeps the frontier inside the cctor after that host-load completion.

Step 27.0.11 / 0.0.95 converts the diagnosis into an explicit iOS compatibility rewrite. Gate A keeps the receipt-backed/prepared 0Harmony file immutable, creates a byte-distinct in-memory runtime image, and replaces only `HarmonySharedState::.cctor` with direct local dictionary initialization, `actualVersion=102`, and null `methodAddressRef`. The rewritten cctor is reopened and pinned to an exact eleven-instruction fingerprint. The private load context loads that runtime image only for exact 0Harmony 2.4.2 after re-verifying the original prepared SHA. T5b then executes the normalized cctor and T6 requires direct state initialized with no generated `HarmonySharedState`/`ILGeneratorProxy` assembly before the existing single public Patch() boundary can run.

## CI hardening 0.0.96–0.0.104 + Step 27.0.21 / 0.0.105

Candidates 0.0.96–0.0.103 progressively removed compile/test-fixture assumptions that prevented the 0.0.95 HarmonySharedState normalization from reaching the real upstream-binary host regression. Those failures were preserved individually in `docs/history/reports/` and did not advance the physical device frontier.

Codemagic 0.0.104 is the first host run that compiled the full suite, executed all 212 tests, and passed the exact hash-pinned official Harmony-Fat 2.4.2 net9.0 surrogate into the production `CreateIosNormalizedHarmonyRuntimeImage` method. The Deferred Cecil read succeeded. The sole 211/212 failure occurred later in `Mono.Cecil.ModuleDefinition.Write`: Cecil rebuilt unrelated Constant metadata and attempted to resolve the enum type `System.Reflection.BindingFlags` through the deliberately rejecting metadata resolver.

Step 27.0.21 / 0.0.105 removes the whole-module Cecil writer rather than adding enum-resolution exceptions. Cecil remains Deferred/read-only for the exact source fingerprint and existing-token discovery. The compatibility image is an exact byte clone of the prepared Harmony DLL with only the already-audited `HarmonySharedState::.cctor` method-body storage replaced in place: a 12-byte fat header plus 47 bytes encoding the same eleven direct-state IL operations. The source must be IL-only and unsigned; the cctor must have a physical fat body with no exception/extra sections or overlapping managed-method RVA; all constructor and field operands must reuse existing metadata tokens; and a byte audit rejects any modification outside that original cctor storage span.

Gate T and the detour stop rule remain unchanged. A host PASS now proves the normalizer can transform a real merged Harmony 2.4.2 image without any metadata serialization. The next physical success criterion remains T6, followed by the single public `PatchProcessor.Patch()` acceptance boundary at T7/T8. If T6 passes but a bounded interpreted launcher-owned patch/unpatch experiment still cannot survive T7/T8, Step 27 stops iterating Harmony internals and the project proposes ahead-of-load transformation as an explicit master-plan change.
