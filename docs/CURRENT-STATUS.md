# Current Status — Step 27 Controlled Launcher-Owned Harmony Patch + Unpatch

## Physically closed boundary

**Steps 01–26 are closed on a physical iPhone.** The latest fully closed runtime boundary is **Step 26.0 / 0.0.83 (83)**: A–N 14/14 PASS, OfflineReady PASS, Foundation 5/5 PASS.

## Step 27 physical evidence

### 0.0.84 (84) — execution frontier

Physical Step 27.0 reached **17/25**: Gates A–Q PASS, Gate R PrefixRegistration FAIL. `HarmonyMethod(MethodInfo)` implicitly triggered `HarmonyLib.AccessTools::.cctor`, which threw before any `PatchProcessor.Patch()` call. This remains the furthest clean Step-27 execution evidence.

### 0.0.85 (85) — first AccessTools metadata correction

Physical Step 27.0.1 reached **14/26** and failed safely at Gate O before AccessTools initialization. It exposed the actual runtime-detection/cache initializer: exact BindingFlags state, `Mono.Runtime`, string/reflection access to `RuntimeInformation.FrameworkDescription`, an add-handler dictionary, and `ReaderWriterLockSlim`.

### 0.0.86 (86) — instruction-count correction

Physical Step 27.0.2 again reached **14/26** at Gate O and established that the receipt-backed `AccessTools::.cctor` is **57 instructions**, not 56, with exactly one `ldc.i4.1`. No Gate R execution or patch occurred.

### 0.0.87 (87) — operand-attribution correction

Physical Step 27.0.3 again reached **14/26** at Gate O. The 57-instruction/opcode fingerprint matched, but the semantic operand check failed. Both `Type.GetType("System.Runtime.InteropServices.RuntimeInformation", bool)` calls use `throwOnError=false`; the single `ldc.i4.1` belongs to `ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion)`.

### 0.0.88 (88) — unstable N–Q region / fresh-process clarification

The user observed repeated abrupt app termination around the **N–Q** region without a surviving managed Step-27 report. One retry produced a normal **0/26 Gate-A failure** because `sts2` was already loaded in `StS2Launcher-Step27-HarmonyPatchExecution`. That rejection is expected: once Gate B has loaded the private context, the same app process is no longer a valid fresh-process Step-27 environment, whether or not Patch() was ever reached.

The 0.0.88 observation does **not** revoke the earlier 0.0.84 A–Q success; it shows that the current expanded Gate-O path needs better crash attribution before we infer a new runtime boundary.

### 0.0.89 (89) — Gate-S hard crash localized

Physical Step 27.0.5 crash telemetry finally localized the abrupt termination. The last synchronously flushed checkpoint was **Gate S / PrefixRegistration / S1**, immediately after entering exact `PatchProcessor.AddPrefix(MethodInfo)` reflection invocation and before S2 could record a return. Therefore Gate R had completed far enough for Gate S to begin, and Gate T (`PatchProcessor.Patch()`) was still not reached. The crash is inside the AddPrefix convenience path, whose measured body constructs `HarmonyMethod(MethodInfo)` and stores it into `PatchProcessor.prefix`.


### 0.0.90 (90) — Gate-T hard crash localized inside PatchProcessor.Patch()

Physical Step 27.0.6 advanced through the bounded annotation-free prefix descriptor path and reached **Gate T / PatchEngineExecution / T1**. The last synchronously flushed checkpoint records entry into the first exact `PatchProcessor.Patch()` reflection invocation while the launcher target was still uninvoked. No T2 checkpoint survived. Therefore the physical frontier moved from AddPrefix into the public patch engine itself; the exact internal failing operation remains unproven.

The raw breadcrumb is preserved at `docs/history/reports/STEP-27.0.6-PHYSICAL-GATE-T-CRASH-CHECKPOINT.txt`.


### 0.0.91 (91) — clean Gate-O regression, no Gate-T execution

Physical Step 27.0.7 replayed **Gates A–N PASS** and then failed normally at **Gate O / HarmonyPatchApiResolution (14/26)**. The failure was `System.IO.InvalidDataException: Targeted patch API reflection unexpectedly changed resolver/load counters.` No hard crash occurred. Because Gate O failed, Gate P and every later gate—including the new HarmonySharedState initialization and the former Patch() crash boundary—were not reached.

The only newly added runtime-reflection block after Gate O's resolver/load snapshot was the HarmonySharedState Type/.cctor/version-field inspection. The candidate therefore proved that this newly introduced runtime reflection has an observable loader/resolver effect on the physical iOS runtime. It did **not** prove that HarmonySharedState initialization or PatchProcessor.Patch() was fixed. The full raw report is preserved at `docs/history/reports/STEP-27.0.7-PHYSICAL-GATE-O-REPORT.txt`.

### 2026-08-22 supplied Gate-S checkpoint — provenance conflict, not a new runtime frontier

A supplied checkpoint had a fresh UTC timestamp but reported **Gate S / S1 — entering exact `PatchProcessor.AddPrefix(MethodInfo)` reflection invocation**. That text is the archived 0.0.89 Gate-S/S1 path, not executable 0.0.92 source. Because that legacy schema lacked app/source identity, it was treated as a provenance mismatch rather than new runtime evidence. Step 27.0.9 / 0.0.93 added fail-closed release identity and self-identifying crash telemetry without changing the patch path.

### 0.0.93 (93) — provenance confirmed; hard crash is inside HarmonySharedState::.cctor

The next physical checkpoint self-identifies **App version 0.0.93 (93)**, **Expected source version 0.0.93 (93)**, the Step 27.0.9 candidate, and the bounded Gate-S implementation marker. Its last durable record is **Gate T / PatchEngineExecution / T5 — entering `RuntimeHelpers.RunClassConstructor(HarmonySharedState.TypeHandle)`**, with `PatchProcessor.Patch()` and the launcher target still uninvoked.

That advances the physical frontier past T1–T4: the bounded host Reflection.Emit/RuntimeMethodHandle preservation preflight returned, and the exact HarmonySharedState runtime Type/.cctor/version-field reflection also returned. The abrupt termination is therefore inside `HarmonyLib.HarmonySharedState::.cctor` before T6. The raw breadcrumb is preserved at `docs/history/reports/STEP-27.0.9-PHYSICAL-GATE-T5-CRASH-CHECKPOINT.txt`.

## Active candidate — Step 27.0.10 / 0.0.94 (94)

- Workflow: **`ios-step-27`**
- IPA: **`artifacts/StS2-Launcher-Step-27.ipa`**
- Main device report: `Documents/StS2Launcher/Reports/Step27-ControlledHarmonyPatchExecution.txt`
- Crash checkpoint: `Documents/StS2Launcher/Reports/Step27-CrashCheckpoint.txt`
- Closed prerequisite: Step 26.0 + OfflineReady + Foundation 5/5
- StS2 reflection/patching/invocation: **forbidden**

### Gates A–S

A–N replay the physically closed Step 26 chain. Gate O retains the physically passing 0.0.90 runtime-reflection surface plus the broader receipt-backed HarmonySharedState/replacement/detour Cecil audit. P–Q resolve and baseline only the launcher-owned probe. R explicitly initializes the measured AccessTools surface. S retains the bounded annotation-free `HarmonyMethod()` descriptor path and never invokes `AddPrefix(MethodInfo)`.

### Gate T — cctor in-flight observability without pre-execution

T1–T4 are behavior-identical to 0.0.93 and are now physically crossed.

- **T1/T2** — bounded Reflection.Emit/RuntimeMethodHandle host-preservation preflight and exact isolation accounting.
- **T3/T4** — exact HarmonySharedState Type/.cctor/internalVersion/actualVersion reflection, with the initializer still unrun and exact loader deltas measured.
- **T5a** — require no pre-existing process-visible generated `HarmonySharedState` or `MonoMod.Utils.Cil.ILGeneratorProxy` assembly, then arm bounded output-only observers.
- While the cctor is active, the dedicated Step-27 `AssemblyLoadContext` reports managed resolver/private/host/native activity through the existing synchronous crash-checkpoint channel. A process `AssemblyLoad` observer reports only dynamic assemblies or the two exact generated names `HarmonySharedState` and `MonoMod.Utils.Cil.ILGeneratorProxy`.
- **T5b** — enter the same exact `RuntimeHelpers.RunClassConstructor(HarmonySharedState.TypeHandle)` call that hard-stopped 0.0.93. No HarmonySharedState internal operation is manually invoked or primed beforehand.
- **T6** — emitted only if the cctor returns; observers are removed before the existing version/generated-assembly/hash/isolation checks.
- **T7/T8/T9** — unchanged: exactly one public `PatchProcessor.Patch()` call, then replacement/isolation validation. The launcher target remains uninvoked until Gate V.

The next hard-stop checkpoint should therefore distinguish at least three useful milestones without altering the cctor's pre-state: no observed relevant load, generated `HarmonySharedState` loaded, or `MonoMod.Utils.Cil.ILGeneratorProxy` loaded. These are causal breadcrumbs, not source-line diagnoses.

`TrimMode=full`, `MtouchInterpreter=-all`, existing DynamicDependency preservation, trusted/prepared-byte immutability, and the broad `UseInterpreter=true`/NativeAOT prohibitions remain unchanged.

### Gates U–Z

U audits before patched execution. V proves patched launcher behavior. W removes exactly the prefix by `MethodInfo`. X audits. Y proves restored behavior. Z performs final hashes/OfflineReady/context/native isolation. No StS2 member is touched.

## Fresh-process rule

**Force-quit/relaunch before every Step-27 retry once any previous attempt reached Gate B.** Gate A intentionally rejects a process where `sts2` or Harmony remains loaded. If Gate T or later ran, also assume launcher/shared patch-engine state may remain process-resident.

If the app hard-crashes, copy `Step27-CrashCheckpoint.txt` before starting another run. Candidate 0.0.94 checkpoints include installed app version/build, expected source version/build, active candidate identity, the exact Gate-S implementation marker, and the Gate-T cctor-observer implementation marker.

## Acceptance

Codemagic + host tests + publish + IPA verification PASS; install `0.0.94 (94)`; from a fresh process run A–Z to **26/26 PASS**; then OfflineReady PASS and Foundation 5/5 PASS. If Step 27 closes, Step 28 is the first targeted StS2 member-reflection boundary.
