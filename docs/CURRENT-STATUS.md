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

### 0.0.94 (94) — original cctor survives host netstandard binding, still terminates before T6

Physical Step 27.0.10 self-identified **App version 0.0.94 (94)** and **Expected source version 0.0.94 (94)**. Its last durable T5 observer record shows the dedicated Step-27 load context completed the host binding `netstandard, Version=2.0.0.0` => `netstandard, Version=2.1.0.0` while the original `HarmonyLib.HarmonySharedState::.cctor` was running. The process then still terminated before T6; `PatchProcessor.Patch()` and the launcher target remained uninvoked.

This proves the observed netstandard request itself was satisfied and moves the actionable problem back inside the remaining original cctor work. The raw checkpoint is preserved at `docs/history/reports/STEP-27.0.10-PHYSICAL-GATE-T5-OBSERVER-CRASH-CHECKPOINT.txt`.

### 0.0.95 (95) — Codemagic compile stop; runtime fix not exercised

Codemagic never reached iOS publish or device packaging for Step 27.0.11. The host-test compilation failed with eleven `CS0104` errors in `ControlledHarmonyPatchExecution.cs`: bare `OpCodes` in the new Cecil initializer rewrite was ambiguous between `System.Reflection.Emit.OpCodes` and `Mono.Cecil.Cil.OpCodes`. This is a compile-only defect introduced by the normalizer source; it provides no negative runtime evidence about the HarmonySharedState AOT-normalization design. The exact compiler output is preserved at `docs/history/reports/STEP-27.0.11-CODEMAGIC-CS0104-HOST-COMPILE-FAILURE.txt`.

### 0.0.96 (96) — compile fixed; 209/211 host tests pass; synthetic preflight scope regression

Codemagic proved the Cecil `OpCodes` compile correction works: the project compiled, the host test runner executed all **211** tests, and **209 passed**. The two failures were `SyntheticStep26ReplayThroughEmptyProcessorStillPassesBeforePatchBoundary` and `GateCReportsThrowingModuleInitializerAndDoesNotAdvance`. Both failed at Gate A with the same message: the new HarmonySharedState normalization attempted the full exact 0Harmony 2.4.2 patch-engine audit against a randomized minimal synthetic Harmony-like fixture, where the internal patch-engine types intentionally do not exist.

This is a host-test scope defect, not physical runtime evidence and not a failure of the normalized production cctor. The public production constructor is already pinned to exact `0Harmony` 2.4.2; Step 27.0.13 therefore restores the synthetic A–N replay by applying the production normalizer only to that canonical target, while internal non-canonical test targets retain their exact original bytes. The full 0.0.96 host report is preserved at `docs/history/reports/STEP-27.0.12-CODEMAGIC-HOST-TEST-FAILURE.txt`.

## Active candidate — Step 27.0.13 / 0.0.97 (97)

- Workflow: **`ios-step-27`**
- IPA: **`artifacts/StS2-Launcher-Step-27.ipa`**
- Main device report: `Documents/StS2Launcher/Reports/Step27-ControlledHarmonyPatchExecution.txt`
- Crash checkpoint: `Documents/StS2Launcher/Reports/Step27-CrashCheckpoint.txt`
- Closed prerequisite: Step 26.0 + OfflineReady + Foundation 5/5
- StS2 reflection/patching/invocation: **forbidden**

### Gates A–S

A–N replay the physically closed Step-26 chain. For the public production boundary, Gate A still performs the fail-closed compatibility normalization only for exact `0Harmony, Version=2.4.2.0`: the original patch-engine metadata fingerprint must pass first, `HarmonySharedState::.cctor` is rewritten only in a deterministic in-memory runtime image, the image is reopened and audited as an exact **11-instruction** initializer, and the runtime SHA must be byte-distinct from the untouched prepared SHA. `CecilOpCodes = Mono.Cecil.Cil.OpCodes` remains mandatory for all eleven emitted instructions.

Step 27.0.13 changes only the internal host-test replay scope. The randomized minimal synthetic Harmony-like targets used to prove Gates A–N are non-canonical by design and do not contain Harmony's full patch-engine internals. They now retain a byte-identical in-memory image and are explicitly barred from pretending to be normalized. This does **not** create a public bypass: the public constructor remains pinned to `TargetSimpleName = "0Harmony"` and `TargetVersion = 2.4.2.0`, and that canonical path still requires the exact patch-engine audit plus byte-distinct normalization.

Gate B's dedicated private load context re-verifies the on-disk prepared SHA immediately before load. Exact production Harmony uses the retained normalized bytes; internal synthetic replay uses the exact original fixture bytes. Gate O remains on the physically passing runtime-reflection surface plus metadata-only audit of the original HarmonySharedState/replacement/detour chain. R initializes the measured AccessTools surface. S retains the bounded annotation-free `HarmonyMethod()` descriptor path and never invokes `AddPrefix(MethodInfo)`.

### Gate T — normalized HarmonySharedState compatibility boundary

T1–T4 retain the physically crossed preservation/runtime-reflection sequence.

- **T1/T2** — bounded Reflection.Emit/RuntimeMethodHandle host-preservation preflight and exact isolation accounting.
- **T3/T4** — exact runtime reflection of the already-loaded normalized `HarmonySharedState` Type/.cctor/version/state fields without reading static values or running the cctor.
- **T5a** — re-hash the retained normalized runtime image and require zero pre-existing known generated `HarmonySharedState`/`MonoMod.Utils.Cil.ILGeneratorProxy` assemblies.
- **T5b** — execute exactly one `RuntimeHelpers.RunClassConstructor(HarmonySharedState.TypeHandle)` against the Gate-A-audited direct-state initializer. The normalized cctor contains no `GetOrCreateSharedStateType`, `ReflectionHelper.Load`, or `FieldRefAccess` call.
- **T6** — require `state`, `originals`, and `originalsMono` non-null; `methodAddressRef == null`; `actualVersion == 102`; zero generated shared-state/proxy assemblies; unchanged prepared bytes; unchanged launcher-probe counters; and bounded private-context isolation.
- **T7/T8/T9** — only after T6, retain exactly one public `PatchProcessor.Patch()` call and replacement/isolation validation. The launcher target remains uninvoked until Gate V.

The runtime compatibility design is therefore unchanged from 0.0.96 for the real app. This release only repairs the test harness so Codemagic can actually certify that design before device installation.

`TrimMode=full`, `MtouchInterpreter=-all`, existing DynamicDependency preservation, trusted/prepared-byte immutability, and the broad `UseInterpreter=true`/NativeAOT prohibitions remain unchanged.

### Gates U–Z

U audits before patched execution. V proves patched launcher behavior. W removes exactly the prefix by `MethodInfo`. X audits. Y proves restored behavior. Z performs final hashes/OfflineReady/context/native isolation. No StS2 member is touched.

## Fresh-process rule

**Force-quit/relaunch before every Step-27 retry once any previous attempt reached Gate B.** Gate A intentionally rejects a process where `sts2` or Harmony remains loaded. If Gate T or later ran, also assume launcher/shared patch-engine state may remain process-resident.

If the app hard-crashes, copy `Step27-CrashCheckpoint.txt` before starting another run. Candidate 0.0.97 checkpoints include installed app version/build, expected source version/build, active candidate identity, the exact Gate-S implementation marker, and the Gate-T normalized-cctor implementation marker.

## Acceptance

Codemagic + host tests + publish + IPA verification PASS; install `0.0.97 (97)`; from a fresh process run A–Z to **26/26 PASS**; then OfflineReady PASS and Foundation 5/5 PASS. If Step 27 closes, Step 28 is the first targeted StS2 member-reflection boundary.
