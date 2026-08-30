# Historical Step Documentation Index

This directory is the readable project history. Historical records stay here even though active source/tooling uses canonical names.

## Retrospective early foundation

- `STEP-01-FOUNDATION-RETROSPECTIVE.md`
- `STEP-02-FOUNDATION-RETROSPECTIVE.md`
- `STEP-03-FOUNDATION-RETROSPECTIVE.md`
- `STEP-04-FOUNDATION-RETROSPECTIVE.md`
- `STEP-05-FINAL-TEST.md`

The Step 01–04 files were written retrospectively during Step 22.4 from the physically closed Foundation evidence because the earliest individual step documents were not retained in the later source tree.

## Later records

Step-specific design/test/fix records for Steps 06 through 22.x are retained in `steps/`. They describe what was known at the time and should be treated as history, not current build instructions.

The current architecture and plan always live one level up in `docs/`.

## Selected raw reports

`reports/` retains diagnostic outputs when they materially explain a later architectural decision.

- `reports/STEP-24.0.2-PHYSICAL-GATE-A-REPORT.txt` — physical build 75 stopped at Gate A before any Step 24 CLR load because Cecil attempted to resolve `GodotSharp`; this is the direct evidence for Step 24.0.3.
- `reports/STEP-24.0.3-PHYSICAL-GATE-A-REPORT.txt` — physical build 76 repeated the build-75 Gate A `GodotSharp` resolver failure at 0/4, proving the explicit `MethodReference.Resolve()` removal did not eliminate every broad/eager Cecil metadata-resolution path.
- `reports/STEP-24.0.4-PHYSICAL-GATE-A-REPORT.txt` — physical build 77 eliminated the resolver failure and exposed the actual target closure: exactly seven conservative MonoMod logging dispatch findings and four automatic initializers; Gate B never ran.
- `reports/STEP-24.0.5-PHYSICAL-GATE-C-REPORT.txt` — physical build 78 passed Gates A/B and reached the real module initializer; Gate C exposed the trimmed `ConcurrentBag<T>` constructor that motivated the final preservation root.

## Canonical-foundation build fixes

- `STEP-22.4.1-MSTEST-V4-CODEMAGIC-FIX.md`

- `STEP-22.4.2-STEP19-REGRESSION-CONTRACT-CORRECTION.md`

## First real managed-game load

- `STEP-23-FIRST-REAL-CLR-LOAD.md`
- `STEP-23-TEST.md`

- [Step 23.1 — Host-Test Isolation Fix](steps/STEP-23.1-HOST-TEST-ISOLATION-FIX.md)
- [Step 23.2 — Deterministic Host-Test Identity Isolation](steps/STEP-23.2-DETERMINISTIC-HOST-TEST-IDENTITY-ISOLATION.md)
- [Step 23.3 — Synthetic Fixture Binding-Plan Coverage Fix](steps/STEP-23.3-SYNTHETIC-FIXTURE-PLAN-COVERAGE-FIX.md)

- [Step 23.4 — Deferred Dependency Module-Initializer Boundary](steps/STEP-23.4-DEFERRED-DEPENDENCY-MODULE-INITIALIZER-BOUNDARY.md)

- `steps/STEP-23.4.1-CECIL-IL-AUDIT-COMPILE-FIX.md` — compile-only missing Cecil.Cil namespace correction.

- `steps/STEP-23.4.2-SYNTHETIC-CORELIB-FIXTURE-NORMALIZATION.md` — host-test fixture correction removing artificial legacy mscorlib metadata.

- `steps/STEP-23.4.3-CECIL-CORELIB-SCOPE-CONSTRUCTION-FIX.md` — constructs the synthetic module-initializer core-library scope correctly before Cecil creates primitive void metadata.

- [Step 23.4.3 — Physical Closure](steps/STEP-23.4.3-PHYSICAL-CLOSURE.md) — all A–D gates, OfflineReady, and Foundation 5/5 passed on physical iPhone; Step 23 closed.
- [Step 24 — Controlled 0Harmony Module Initialization Boundary](steps/STEP-24-CONTROLLED-MANAGED-INITIALIZATION.md) — design lineage for the automatic-initialization boundary after physical Step 23 closure.
- `steps/STEP-24.0.1-OFFLINEREADY-API-COMPILE-FIX.md` — build-73 compile-only correction to the established OfflineReady inspection API; build 74 subsequently reached host tests.
- `steps/STEP-24.0.2-PINVOKE-AUDIT-FIX.md` — build-74 host tests exposed a same-assembly P/Invoke audit blind spot; build 75 corrected it and reached physical Gate A.
- `steps/STEP-24.0.3-CECIL-LOCAL-METADATA-RESOLUTION-FIX.md` — build 75 motivated removal of explicit Cecil method resolution; physical build 76 later showed that correction was incomplete by repeating the same Gate A `GodotSharp` resolver failure.
- `steps/STEP-24.0.4-DEFERRED-TWO-PASS-METADATA-AUDIT-FIX.md` — physical build 76 repeated the opaque `GodotSharp` resolver failure; build 77 narrowed Gate A to deferred shallow whole-plan classification plus target-only closure audit with explicit rejecting Cecil resolvers and stronger diagnostics.
- `steps/STEP-24.0.5-CONDITIONAL-MONOMOD-LOGGING-DISPATCH.md` — physical build 77 exposed seven exact MonoMod logger dispatch findings; build 78 preserved the raw audit, conditionally classified exactly that fingerprint, and physically passed Gates A/B before reaching Gate C.
- `steps/STEP-24.0.6-SYSTEM-COLLECTIONS-CONCURRENT-PRESERVATION.md` — physical build 78 entered the real 0Harmony module initializer and failed on a trimmed `ConcurrentBag<T>` constructor; 0.0.79 added one `System.Collections.Concurrent` trimmer root without changing the interpreter or execution boundary.
- [Step 24.0.6 — Physical Closure](steps/STEP-24.0.6-PHYSICAL-CLOSURE.md) — user-confirmed physical 4/4 plus OfflineReady PASS and Foundation 5/5; Step 24 closed and the concurrent-collections preservation root became protected platform policy.
- [Step 25 — Controlled Harmony API Resolution + Type Initialization + Instance Construction](steps/STEP-25-CONTROLLED-HARMONY-CONSTRUCTION.md) — active nine-gate candidate that replays closed Step 24, resolves only the exact Harmony API/type-initializer surface, explicitly completes the measured Harmony type initializer, constructs one inert Harmony object, and still forbids patching/game/Godot/native progression.
- `steps/STEP-25.0.1-HOST-LOCAL-ASSEMBLY-CLASSIFICATION-FIX.md` — Step 25.0 / 0.0.80 compiled and ran 180 host tests at 177/180; 0.0.81 minimally fixes synthetic local-assembly classification plus one stale test-only fingerprint label.
- `steps/STEP-25.0.2-HARMONY-CONSTRUCTOR-FRAMEWORK-PRESERVATION.md` — physical 0.0.81 advanced 7/9 through Harmony type initialization; Gate H exposed trimmed `Environment.Version`, so 0.0.82 preserves the bounded framework type surface referenced by the exact measured constructor IL.

- `steps/STEP-25.0.2-PHYSICAL-CLOSURE.md` — physical 9/9 Step 25 closure with OfflineReady + Foundation preserved.
- `steps/STEP-26-CONTROLLED-HARMONY-PROCESSOR-CREATION.md` — active empty PatchProcessor creation design.
- `steps/STEP-26.0-PHYSICAL-CLOSURE.md` — physical 14/14 Step 26 closure with OfflineReady PASS and Foundation 5/5; inert PatchProcessor creation became the accepted baseline.
- `steps/STEP-27-CONTROLLED-LAUNCHER-HARMONY-PATCH.md` — active first real Harmony patch/unpatch design, restricted to a deterministic launcher-owned target/prefix pair; StS2 reflection remains deferred to Step 28.
- `reports/STEP-27.0-PHYSICAL-GATE-R-REPORT.txt` — physical 0.0.84 reached 17/25: A–Q PASS, Gate R failed before `Patch()` because `HarmonyMethod(MethodInfo)` implicitly triggered `HarmonyLib.AccessTools::.cctor`.
- `steps/STEP-27.0.1-ACCESSTOOLS-TYPE-INITIALIZATION-BOUNDARY.md` — 0.0.85 makes the physically discovered AccessTools type initializer an explicit metadata-audited gate before prefix registration; first patch execution shifts to Gate T.
- `reports/STEP-27.0.1-PHYSICAL-GATE-O-REPORT.txt` — physical 0.0.85 failed safely 14/26 at Gate O; no AccessTools execution occurred, and the report exposed the broader runtime-detection/cache surface that 0.0.86 then attempted to fingerprint (later corrected by the 0.0.86 physical run to 57 instructions).
- `steps/STEP-27.0.2-ACCESSTOOLS-MEASURED-INITIALIZER-PRESERVATION.md` — 0.0.86 pins that exact physical AccessTools fingerprint, preserves only its bounded string/reflection/cache framework surface, and keeps explicit AccessTools initialization at Gate R before prefix registration and patching.
- `reports/STEP-27.0.2-PHYSICAL-GATE-O-REPORT.txt` — physical 0.0.86 failed safely 14/26 at Gate O; no AccessTools execution or patch occurred, and the phone corrected the exact initializer fingerprint to 57 instructions with one required `ldc.i4.1`.
- `steps/STEP-27.0.3-ACCESSTOOLS-PHYSICAL-FINGERPRINT-CORRECTION.md` — 0.0.87 confirmed the 57-instruction AccessTools fingerprint but failed safely at Gate O because the single `ldc.i4.1` was attributed to the wrong operation; it also introduced/enforced the synchronized top-of-app current release presentation.
- `reports/STEP-27.0.3-PHYSICAL-GATE-O-REPORT.txt` — physical 0.0.87 failed safely 14/26 at Gate O; both RuntimeInformation Type.GetType probes are false, and the required `ldc.i4.1` belongs to ReaderWriterLockSlim SupportsRecursion.
- `steps/STEP-27.0.4-ACCESSTOOLS-OPERAND-ATTRIBUTION-CORRECTION.md` — 0.0.88 pins false/false Type.GetType operands plus SupportsRecursion (1) for the lock constructor without moving the AccessTools or patch execution gates.
- `reports/STEP-27.0.4-PHYSICAL-FRESH-PROCESS-GUARD-REPORT.txt` — physical 0.0.88 same-process retry was rejected safely at Gate A because the Step-27 `sts2`/Harmony context was already resident; user separately reported repeated abrupt termination around N–Q without a surviving managed report.
- `steps/STEP-27.0.5-CRASH-LOCALIZATION-AND-GATE-O-PURITY.md` — 0.0.89 adds durable per-gate/substage crash checkpoints, corrects the force-quit rule to apply after Gate B starts, and moves reflected FrameworkDescription execution from Gate O into Gate R without changing the 26-gate patch objective.

- `steps/STEP-27.0.6-BOUNDED-IOS-PREFIX-DESCRIPTOR-REGISTRATION.md` — 0.0.90 bounded descriptor path after physical 0.0.89 localized a hard crash inside AddPrefix.
- `reports/STEP-27.0.5-PHYSICAL-GATE-S-CRASH-CHECKPOINT.txt` — raw durable 0.0.89 Gate-S/S1 crash checkpoint.
- `reports/STEP-27.0.6-PHYSICAL-GATE-T-CRASH-CHECKPOINT.txt` — raw durable 0.0.90 Gate-T/T1 checkpoint proving the bounded descriptor path reached the first exact public `PatchProcessor.Patch()` invocation before abrupt termination and before launcher-target invocation.
- `steps/STEP-27.0.7-HARMONY-SHARED-STATE-INITIALIZATION-AND-PATCH-ENGINE-PRESERVATION.md` — 0.0.91 decomposes the newly measured patch-engine frontier with explicit HarmonySharedState T1/T2, bounded dynamic-code preservation, then the unchanged single public Patch() acceptance call at T3/T4 with T5 validation.
- `steps/STEP-27.0.8-GATE-O-PURITY-RESTORATION-AND-T-RUNTIME-RESOLUTION.md` — 0.0.92 records physical 0.0.91 Gate-O resolver/load-counter regression, restores Gate-O runtime purity, and moves bounded host/HarmonySharedState runtime resolution into measured T1–T4 before shared-state initialization T5/T6 and public Patch T7/T8/T9.
- `steps/STEP-27.0.9-CRASH-CHECKPOINT-RELEASE-PROVENANCE-HARDENING.md` — 0.0.93 records the fresh-timestamp/legacy-S1 provenance conflict, leaves the 0.0.92 patch path unchanged, fail-closes on bundle/source identity mismatch, and adds release/candidate/Gate-S identity to every crash checkpoint.
- `reports/STEP-27.0.9-PHYSICAL-GATE-T5-CRASH-CHECKPOINT.txt` — self-identifying physical 0.0.93 Gate-T/T5 breadcrumb proving T1–T4 crossed and localizing abrupt termination inside the exact HarmonySharedState class-constructor call before Patch()/launcher-target invocation.
- `steps/STEP-27.0.10-HARMONYSHAREDSTATE-CCTOR-IN-FLIGHT-OBSERVABILITY.md` — 0.0.94 preserves the unchanged T5 cctor boundary and adds bounded synchronous resolver/AssemblyLoad milestones so a hard stop can be localized without pre-running HarmonySharedState internals.
- `steps/STEP-27.0.11-IOS-HARMONYSHAREDSTATE-AOT-NORMALIZATION.md` — 0.0.95 replaces only the failing HarmonySharedState runtime cctor in a verified in-memory 0Harmony image with direct local state initialization, preserving the trusted prepared file and keeping Patch() behind T6.
- `reports/STEP-27.0.11-CODEMAGIC-CS0104-HOST-COMPILE-FAILURE.txt` — Codemagic host compilation for 0.0.95 stopped on eleven ambiguous bare `OpCodes` references before iOS publish/runtime.
- `steps/STEP-27.0.12-CECIL-OPCODES-COMPILE-HARDENING.md` — 0.0.96 keeps the 0.0.95 runtime design unchanged and explicitly aliases Cecil opcodes for the eleven normalized cctor instructions.
- `steps/STEP-27.0.13-SYNTHETIC-PREFLIGHT-SCOPE-HARDENING.md` — 0.0.97 keeps the production normalization unchanged while restoring byte-identical randomized synthetic A–N replay after the 0.0.96 209/211 host-test result.
- `reports/STEP-27.0.12-CODEMAGIC-HOST-TEST-FAILURE.txt` — full 0.0.96 Codemagic host report proving compilation succeeded and exactly two synthetic Gate-A tests failed before publish.
- `reports/STEP-27.0.10-PHYSICAL-GATE-T5-OBSERVER-CRASH-CHECKPOINT.txt` — physical 0.0.94 confirms the cctor survives netstandard host binding but still terminates before T6/Patch().
- `reports/STEP-27.0.13-PHYSICAL-GATE-A-REPORT.txt` — physical 0.0.97 fails 0/26 in the new runtime-image normalizer because Cecil Immediate mode eagerly decodes an `EditorBrowsableState` custom-attribute argument through the deliberately rejecting metadata resolver; no Gate-B load or patch execution occurs.
- `steps/STEP-27.0.14-DEFERRED-CECIL-NORMALIZATION-AND-REAL-HARMONY-CI-GATE.md` — 0.0.98 restores Deferred Cecil reads, adds an exact upstream Harmony 2.4.2 normalizer regression to Codemagic, and records the one-experiment detour stop rule before any architecture pivot.
- `reports/STEP-27.0.14-CODEMAGIC-TEST-COMPILE-FAILURE.txt` — 0.0.98 Codemagic proves the production core compiles but the new real-Harmony test stops on ambiguous `ICustomAttributeProvider` before the fixture can execute.
- `steps/STEP-27.0.15-REAL-HARMONY-TEST-NAMESPACE-COMPILE-HARDENING.md` — 0.0.99 aliases the Cecil custom-attribute-provider interface in the quarantined real-Harmony regression and leaves production Deferred normalization unchanged.
- `reports/STEP-27.0.15-CODEMAGIC-REAL-HARMONY-FIXTURE-ACQUISITION-FAILURE.txt` — 0.0.99 Codemagic proves production and test source compilation, then stops before test execution because the MSBuild target assumed a nonexistent NuGet `lib/netstandard2.0/0Harmony.dll` implementation path.
- `steps/STEP-27.0.16-REAL-HARMONY-FAT-RELEASE-FIXTURE-HARDENING.md` — 0.0.100 removes NuGet/MSBuild package-layout coupling and acquires the exact official Harmony-Fat 2.4.2 release fixture in the canonical host-test script.
- `reports/STEP-27.0.16-CODEMAGIC-HARMONY-FAT-ARCHIVE-MEMBER-FAILURE.txt` — 0.0.100 Codemagic proves the official release download succeeded but the exact-root archive-member assumption found zero netstandard2.0 candidates before any build/test.
- `steps/STEP-27.0.17-HARMONY-FAT-ARCHIVE-MEMBER-DISCOVERY-HARDENING.md` — 0.0.101 keeps production runtime code unchanged and selects exactly one wrapped `/netstandard2.0/0Harmony.dll` archive member by strict suffix, with diagnostic member listing on drift.
- `reports/STEP-27.0.17-CODEMAGIC-HARMONY-FAT-NETSTANDARD-ABSENCE.txt` — 0.0.101 Codemagic proves the official fat release contains no netstandard2.0 implementation and stops before build/test after printing the complete 0Harmony framework set.
- `steps/STEP-27.0.18-OFFICIAL-NET9-FAT-NORMALIZER-SURROGATE.md` — 0.0.102 keeps production runtime code unchanged and uses the official merged net9.0 Harmony 2.4.2 implementation as a clearly labeled host-only structural surrogate for the Deferred-Cecil normalizer regression.

- `reports/STEP-27.0.18-CODEMAGIC-NET9-SURROGATE-REFERENCE-ASSERTION-FAILURE.txt` — 0.0.102 Codemagic acquired the official net9 surrogate and ran 212 tests at 211/212; the only failure was the invalid assumption that a net9 implementation cannot reference netstandard.
- `steps/STEP-27.0.19-NET9-SURROGATE-REFERENCE-GRAPH-ASSERTION-FIX.md` — 0.0.103 removes that negative inference, positively pins the selected net9 archive member and System.Runtime 9.0 profile, and leaves production normalization unchanged.

- `reports/STEP-27.0.19-CODEMAGIC-DUPLICATE-SYSTEM-RUNTIME-ASSEMBLYREF-FAILURE.txt` — 0.0.103 Codemagic compiled production/tests and ran 212 tests at 211/212; the only failure was test-only `SingleOrDefault` over duplicate `System.Runtime` AssemblyRef rows before the normalizer call.
- `steps/STEP-27.0.20-HASH-PINNED-REAL-HARMONY-NORMALIZER-EXECUTION.md` — 0.0.104 removes AssemblyRef-based surrogate inference, pins the exact official release archive/DLL hashes observed by Codemagic, and proceeds directly to the unchanged production normalizer.
- `reports/STEP-27.0.20-CODEMAGIC-CECIL-WRITER-ENUM-CONSTANT-FAILURE.txt` — 0.0.104 Codemagic ran all 212 host tests at 211/212; the official Harmony surrogate reached the production normalizer and exposed Cecil whole-module serialization resolving `System.Reflection.BindingFlags` while rebuilding enum Constant metadata.
- `steps/STEP-27.0.21-RAW-METHOD-BODY-NORMALIZATION.md` — 0.0.105 removes Cecil whole-module writing from Gate A and replaces only the already-audited `HarmonySharedState::.cctor` PE method-body slot in the private in-memory runtime image using existing metadata tokens.
- `reports/STEP-27.0.21-PHYSICAL-T7-SYSTEM-LINQ-TRIM-FAILURE.txt` — physical 0.0.105 proof that raw-body HarmonySharedState normalization succeeded and public PatchProcessor.Patch() then failed in MethodCreator on trimmed `Enumerable.Union<T>` before the detour boundary.
- `steps/STEP-27.0.22-POST-PUBLISH-SYSTEM-LINQ-PRESERVATION.md` — 0.0.106 roots complete `System.Linq` for post-publish dynamic payloads and adds T6a/T6b exact LINQ callable-surface preflight; master plan remains unchanged because the native detour boundary is still untested.

- `steps/STEP-27.0.23-DYNAMIC-PAYLOAD-NO-TRIM-HOST-POLICY.md` — replaces full trimming with copy/no-link after physical 0.0.106 exposed the second post-publish BCL trim failure.
- `reports/STEP-27.0.22-PHYSICAL-DYNAMICMETHODDEFINITION-DEBUGGABLEATTRIBUTE-TRIM-FAILURE.txt` — full physical 0.0.106 Gate-T report.

- `reports/STEP-27.0.23-PHYSICAL-NOTIMPLEMENTED-PATCHENGINE.txt` — physical 0.0.107 removed the known trimming ambiguity and reached the first real PatchProcessor.Patch(), which threw NotImplementedException from PatchFunctions.UpdateWrapper.
- `steps/STEP-27.0.24-POST-PUBLISH-INTERPRETED-PATCH-EXPERIMENT.md` — the single stop-rule trial against a launcher-owned target/prefix DLL copied only after iOS publish; failure to patch ends Harmony-internal iteration and triggers the Step-28 ahead-of-load pivot.
- `reports/STEP-27.0.24-PHYSICAL-INTERPRETED-PATCH-FAILURE.txt` — physical 0.0.108 final Step-27 discriminator: 19/26, Gate T PatchEngineExecution fails with `NotImplementedException` from `PatchFunctions.UpdateWrapper` against a genuine post-publish interpreted target.
- `steps/STEP-27.0.24-PHYSICAL-NEGATIVE-CLOSURE.md` — closes Step 27 negatively and records the architecture decision to retire runtime Harmony/MonoMod replacement.
- `steps/STEP-28-AHEAD-OF-LOAD-MANAGED-TRANSFORMATION.md` — Step 28.0 / 0.0.109 design: deterministic Cecil transformation before CLR admission, transformed-only private-context execution, no real StS2 behavior yet.
- `reports/STEP-28.0-CODEMAGIC-CORE-COMPILE-FAILURE.txt` — 0.0.109 Codemagic passed 845/845 static validation and built all external fixtures, then Core compilation stopped before MSTest on missing `CallbackProgress<>`; no iOS/device evidence exists.
- `steps/STEP-28.0.1-CALLBACK-PROGRESS-COMPILE-FIX.md` — 0.0.110 adds only the established callback-backed `IProgress<T>` adapter plus static regression guards; the Step-28 A–E experiment and architecture remain unchanged.
- `reports/STEP-28.0.1-CODEMAGIC-HOST-TEST-FAILURE.txt` — raw 0.0.110 Codemagic host report: compile succeeded, 216/217 tests passed, and Gate A failed on eager Cecil `System.Runtime` resolution before rewrite/load.
- `steps/STEP-28.0.2-DEFERRED-CECIL-METADATA-READ-FIX.md` — 0.0.111 switches only Step-28 fixture metadata reads from Immediate to Deferred while retaining the rejecting resolver; experiment semantics remain unchanged.

## Step 28 physical closure / Step 29 frontier

- `reports/STEP-28.0.2-PHYSICAL-CLOSURE.txt` — raw physical 0.0.111 report: Step 28 A–E 5/5 PASS, Gate D 1000 / 1041 / 1041, transformed-only CLR admission, post-execution OfflineReady 428/428.
- `steps/STEP-28.0.2-PHYSICAL-CLOSURE.md` — formal positive closure of deterministic transform-before-load + post-publish interpreted execution.
- `steps/STEP-29-REAL-STS2-COMPATIBILITY-TARGET-AUDIT.md` — active read-only exact ARM64 `sts2.dll` target re-audit/selection boundary.
- `steps/STEP-29-TEST.md` — physical Step-29 A–D acceptance procedure.

## Step 29 physical closure / Step 30 frontier

- `reports/STEP-29.0-PHYSICAL-CLOSURE.txt` — raw physical 0.0.112 report: Step 29 A–D 4/4 PASS; exact receipt-backed source preserved; selected `ModManager.TryLoadMod(Mod)` token `0x06007927` @ `IL_0D9D` -> `Harmony.PatchAll(Assembly)`; zero writes/CLR load; OfflineReady 428/428.
- `steps/STEP-29.0-PHYSICAL-CLOSURE.md` — formal positive closure of exact real-StS2 read-only target selection.
- `steps/STEP-30-SELECTED-HARMONY-TARGET-SEMANTIC-CONTEXT-AUDIT.md` — design for the read-only selected Harmony target semantic-context/product-scope audit.
- `steps/STEP-30-TEST.md` — physical Step-30 A–D acceptance procedure.
- `steps/STEP-30.0-PHYSICAL-CLOSURE.md` — physical 0.0.113 4/4 closure; selected Harmony/mod site deferred.
- `reports/STEP-30.0-PHYSICAL-CLOSURE.txt` — raw physical 0.0.113 Step-30 report.
- `steps/STEP-31-PREPAREMETHOD-SEMANTIC-CONTEXT-AUDIT.md` — active read-only PrewarmJit/PrepareMethod semantic-context audit.
- `steps/STEP-31-TEST.md` — physical Step-31 A–D acceptance procedure.

## Step 31 physical closure / Step 32 frontier

- `reports/STEP-31.0-PHYSICAL-CLOSURE.txt` — raw physical 0.0.114 report: Step 31 A–D 4/4 PASS; exact PrewarmJit token/body + ten PrepareMethod sites rebound; rewrite-design eligibility confirmed; zero writes/CLR load; OfflineReady 428/428.
- `steps/STEP-31.0-PHYSICAL-CLOSURE.md` — formal positive closure of the PrepareMethod semantic-context audit.
- `steps/STEP-32-FIRST-REAL-STS2-PREPAREMETHOD-REWRITE.md` — active first real-game launcher-private transformation design with exact stack-neutral rewrite semantics.
- `steps/STEP-32-TEST.md` — physical Step-32 A–D acceptance procedure.
- `reports/STEP-32.0-CODEMAGIC-HOST-TEST-FAILURE.txt` — raw 0.0.115 Codemagic host report: static validation 996/996, compile success, 230/231 host tests; sole Step-32 Gate-C failure was the invalid pre-write offset-sensitive body-fingerprint serialization invariant.
- `steps/STEP-32.0.1-SERIALIZED-FINGERPRINT-VERIFICATION-FIX.md` — 0.0.116 keeps the exact 6+4 rewrite and corrects only the pre-write→reopen fingerprint model: offset-independent semantic hash is predictive; physical body hash is post-write evidence.
- `steps/STEP-32.0.1-TEST.md` — physical acceptance procedure for 0.0.116 after the serialized-fingerprint verification correction.
- `reports/STEP-32.0-CODEMAGIC-STATIC-VALIDATION.txt` — raw 0.0.115 static report: 996/996 PASS.
- `reports/STEP-32.0.1-PHYSICAL-CECIL-WRITE-RESOLUTION-FAILURE.txt` — raw physical 0.0.116 report: Gate A PASS; Gate B failed inside Cecil Constant-table serialization on exact `System.Runtime 9.0.0.0` resolution before any CLR admission.
- `steps/STEP-32.0.2-BOUNDED-CONSTANT-METADATA-WRITE-RESOLVER.md` — 0.0.117 keeps the 6+4 rewrite unchanged and introduces an exact-identity, in-memory write-only constant-metadata surrogate instead of broad resolver fallback.
- `steps/STEP-32.0.2-TEST.md` — Codemagic + physical A–D acceptance procedure for the bounded write resolver correction.
- `reports/STEP-32.0.2-PHYSICAL-SENTRY-CONSTANT-METADATA-FAILURE.txt` — raw physical 0.0.117 report: Gate A PASS; Gate B failed closed before mutation because the verified real module also contains external constant metadata scoped to exact `Sentry 5.0.0.0`.
- `reports/STEP-32-STATIC-STS2-CONSTANT-METADATA-AUDIT.txt` — static-only audit of the exact receipt-backed Step-32 DLL; resolves the Sentry constant providers/types/storage and distinguishes non-null resolver requirements from null-only external Constant rows.
- `steps/STEP-32.0.3-RETIRED-HARMONY-ACTIVE-SURFACE-TRIM.md` — 0.0.118 maintenance-only trim that moves closed Step-25/26/27 runtime-Harmony executable surface out of the active build while leaving the Step-32 rewrite/resolver behavior unchanged.
- `steps/STEP-32.0.3-TEST.md` — CI/build acceptance procedure for the maintenance trim; no new Step-32 physical closure is claimed.
- `steps/STEP-32.0.4-AUDITED-MULTI-SCOPE-CONSTANT-METADATA-WRITE-RESOLVER.md` — 0.0.119 evidence-bounded resolver correction: exact audited System.Runtime + Sentry enum/storage requirements only, per-exact-assembly in-memory surrogates, all other requirements/resolution identities fail closed; 6+4 rewrite unchanged.
- `steps/STEP-32.0.4-TEST.md` — Codemagic + physical A–D acceptance procedure for 0.0.119.
- `reports/STEP-32.0.4-CODEMAGIC-HOST-FIXTURE-FAILURE.txt` — first 0.0.119 CI attempt: static 669/669 and compile passed; 183/186 host tests because the synthetic exact-System.Runtime fixture used an image-backed `TypeSystem.Int32` lookup before the intended Step-32 test paths ran; corrected fixture keeps production code/version unchanged.
- `reports/STEP-32.0.4-PHYSICAL-GATE-C-TRANSFORMED-METHOD-IDENTITY-FAILURE.txt` — physical 0.0.119 advanced Step 32 to 2/4: Gate A and Gate B passed; Gate C failed before semantic reopen checks because the verifier reused source MethodDef token 0x06007D05 as a post-Cecil-write locator.
- `steps/STEP-32.0.5-STABLE-TRANSFORMED-METHOD-VERIFICATION.md` — 0.0.120 correction: retain exact source-token admission, but bind the reopened transformed PrewarmJit by exact declaring type + full signature and treat transformed token preservation as diagnostic only.
- `steps/STEP-32.0.5-TEST.md` — CI/IPA/physical acceptance procedure for the 0.0.120 Gate-C verification correction.
- `reports/STEP-32.0.5-PHYSICAL-CLOSURE-4OF4.txt` — authoritative physical 0.0.120 Step-32 closure: A–D 4/4, exact transformed hash/semantic verification, trusted source unchanged, zero real-game CLR admission.
- `steps/STEP-33.0-VERIFIED-TRANSFORMED-CLR-ADMISSION.md` — Step 33.0 / 0.0.121 admission-only design: requalify exact Step-32 transformed image, LoadFromStream only transformed primary, reject private/native expansion, re-prove isolation.
- `steps/STEP-33.0-TEST.md` — Codemagic/IPA/physical acceptance procedure for the Step-33 transformed-primary admission boundary.

- `reports/STEP-33.0-CODEMAGIC-COLD-CACHE-HOST-COMPILE-FAILURE.txt` — cold-cache 0.0.121 host compile failure before iOS publish/AOT; corrected by moving the new Step-33 test to MSTest v4 `Assert.ThrowsExactly`.

- `reports/STEP-33.0-PHYSICAL-CLOSURE-4OF4.txt` — physical 0.0.121 Step-33 CLOSED POSITIVE 4/4; exact transformed primary admitted with zero resolver/private/native expansion and no game-member invocation.
- `steps/STEP-34.0-CONTROLLED-TRANSFORMED-PREWARMJIT-EXECUTION.md` — active 0.0.122 design for the first exact transformed real-StS2 `PrewarmJit()` invocation.
- `steps/STEP-34.0-TEST.md` — Step-34 host/static/physical acceptance contract.
- `reports/STEP-34.0-CODEMAGIC-IOS-UI-COMPILE-FAILURE.txt` — first Step-34 0.0.122 CI attempt: static 735/735 + host 194/194 PASS, persistent iOS cache restored, then UI CS1503 before publish completion; no IPA/device evidence.
- `steps/STEP-34.0.1-IOS-SYSTEMBUTTON-COMPILE-FIX.md` — build-only correction: Step-34 UI passes numeric font size 17 to the established `SystemButton` helper; runtime semantics/version unchanged.

- `reports/STEP-34.0-PHYSICAL-CLOSURE-4OF4.txt` — physical 0.0.122 closure: exact transformed PrewarmJit invoked once and returned normally; Step 34 CLOSED POSITIVE 4/4.
- `steps/STEP-35.0-CONTROLLED-VERY-EARLY-INITIALIZATION.md` — active 0.0.123 design for exact transformed `OneTimeInitialization::ExecuteVeryEarly()` invocation + bounded Task await.
- `steps/STEP-35.0-TEST.md` — Step-35 host/static/device acceptance contract.

## Step 35 physical hard-termination / Step 35.0.1 diagnostic frontier

- `reports/STEP-35.0-PHYSICAL-HARD-TERMINATION-SUMMARY.txt` — sanitized physical 0.0.123 evidence: app terminated around visible B→C region; matching iOS crash report shows EXC_BAD_ACCESS/SIGKILL, main-thread PC=0x0, and no managed Step-35 report.
- `steps/STEP-35.0.1-B-C-HARD-TERMINATION-CRASH-LOCALIZATION.md` — 0.0.124 diagnostic-only revision: freezes Step-35 compatibility authority and adds durable provenance/thread-aware checkpoints around Gate B, the B→C handoff, ExecuteVeryEarly binding/invocation/await, and resolver callbacks; cancellation is INCONCLUSIVE.

## Step 35.0.1 physical invoke-crash localization / Step 35.0.2 static IL-callsite diagnostic

- `reports/STEP-35.0.1-PHYSICAL-EXECUTEVERYEARLY-INVOKE-CRASH-LOCALIZATION.txt` — sanitized physical 0.0.124 evidence: Gate B passed fully; Gate C entered exact ExecuteVeryEarly `MethodInfo.Invoke`, serviced planned GodotSharp/Steamworks.NET/framework resolutions, then hard-terminated before `C_INVOKE_RETURNED`; matching `.ips` repeats main-thread PC=0x0 and the same runtime-heavy stack shape as 0.0.123.
- `steps/STEP-35.0.2-EXECUTEVERYEARLY-INVOKE-CRASH-STATIC-ILCALLSITE-LOCALIZATION.md` — 0.0.125 diagnostic-only revision: execution/resolver authority unchanged; emit a resolution-free static IL/callsite map of the exact verified transformed ExecuteVeryEarly wrapper + async MoveNext before CLR admission, while retaining durable runtime checkpoints.


## Step 35.0.2 physical repeat / Step 35.0.3 run-correlated telemetry

- `reports/STEP-35.0.2-PHYSICAL-REPEATED-HARD-TERMINATION-AND-TELEMETRY-CORRELATION.txt` — sanitized physical 0.0.125 evidence: same main-thread PC=0x0 / CODESIGNING Invalid Page family, but the available static map predates the matching crash-report process and no fixed-name checkpoint from that process was available.
- `steps/STEP-35.0.3-RUN-CORRELATED-DURABLE-TELEMETRY.md` — 0.0.126 diagnostic-only correction: immutable Run ID/PID, unique same-run journal/map, current-run manifest, independently flushed last checkpoint, fail-visible telemetry prerequisite, execution/resolver authority otherwise unchanged.

- `reports/STEP-35.0.3-PHYSICAL-SAME-RUN-CORRELATION-AND-INVOKE-FRONTIER.txt` — physical 0.0.126 proves same-run telemetry and repeats the no-C_INVOKE_RETURNED synchronous frontier.
- `steps/STEP-35.0.4-IN-METHOD-PRE-FIRST-AWAIT-LOCALIZATION.md` — 0.0.127 diagnostic-clone design with durable in-method entry markers.
- `reports/STEP-35.0.4-PHYSICAL-GATE-A-CECIL-WRITE-RESOLUTION-FAILURE.txt` — physical 0.0.127 ended normally at Gate A: diagnostic-clone Cecil serialization failed closed on exact System.Runtime 9.0.0.0 metadata resolution before CLR admission or game invocation.
- `steps/STEP-35.0.5-BOUNDED-CECIL-WRITER-RESOLUTION-FIX.md` — 0.0.128 reuses the physically proven Step-32 audited in-memory constant-metadata surrogate resolver for diagnostic-clone serialization only, then requires rejecting-resolver reopen plus constant-metadata fingerprint preservation.
- `reports/STEP-35.0.5-PHYSICAL-GATE-A-DEFERRED-OPEN-FAILURE.txt` — physical 0.0.128 repeated the normal Gate-A System.Runtime resolution failure before CLR admission; post-run analysis localized it to Immediate Cecil reading before bounded resolver configuration.
- `steps/STEP-35.0.6-DEFERRED-CECIL-OPEN-BEFORE-WRITER-CONFIGURATION.md` — 0.0.129 correction: mirror physically closed Step 32 with deferred open -> audit/configure -> bounded write -> rejecting reopen, without changing game-execution authority.
