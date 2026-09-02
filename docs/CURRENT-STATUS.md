# Current status

## Active candidate — Step 35.0.17 / 0.0.140 (140)

Steps 01–26 are closed. Step 27 is **CLOSED NEGATIVE**. Step 28 is **CLOSED POSITIVE 5/5**. Steps 29–34 are **CLOSED POSITIVE 4/4**. **Step 35 remains OPEN.**

The authoritative exact-transformed Step-35 execution frontier remains physical **0.0.126**: exact `ExecuteVeryEarly()` entered `MethodInfo.Invoke`, but no `C_INVOKE_RETURNED` was durably recorded. All later Step-35 binaries are diagnostic derivatives unless a separately defined closure candidate restores explicit exact-byte execution authority.

## Physical localization through 0.0.136

- 0.0.129 isolated and corrected the synthetic `Action<string>` MemberRef issue; 0.0.130 established `Action<string>::Invoke(!0)` as the bridge contract.
- 0.0.130–0.0.132 advanced through SaveManager/UserDataPathProvider/Platform/NullPlatform and into work triggered by `CommandLineHelper.TryGetValue`.
- 0.0.133 and 0.0.135 proved live-stack CL/CLTV callsite sweeps could make the CommandLine cctor invalid before instruction zero; those probes remain retired.
- 0.0.136 used only stack-neutral critical markers. It entered `CommandLineHelper..cctor`, emitted `CL_CRITICAL_001_PRE`, and terminated before the matching POST. The exact-source map localized that interval to `Godot.Collections.Dictionary<string,string>` construction before `_args` assignment.

## 0.0.137 Codemagic result

0.0.137 was a pre-device failure: static validation passed, host tests were **208/209**, and the sole failure was the GodotSharp derivative verifier checking the sts2 bridge type. No IPA/device evidence was produced. 0.0.138 corrected that verifier boundary without changing the intended runtime experiment.

## Physical 0.0.138 NATURAL/COMPAT result

Two separate fresh-process runs were captured on 2026-09-01.

### NATURAL — `NaturalGodotDictionaryRecon`

Run ID `20260901T2040125125330Z-pid23845-5d0e9577b6624eada7d215301f748dda` advanced:

`CommandLineHelper..cctor` → `CL_CRITICAL_001_PRE` → Godot generic Dictionary cctor/ctor → non-generic Godot Dictionary ctor → `NativeFuncs::godotsharp_dictionary_new()` → `NativeFuncs::godotsharp_dictionary_new(godot_dictionary&)` → `CustomUnsafe::AsPointer(godot_dictionary&)` (`GS014`), then hard termination.

`CL_CRITICAL_001_POST` did not appear. This refines the 0.0.136 outer constructor interval into the GodotSharp native dictionary thunk.

### COMPAT — `ManagedDictionaryCompatibility`

Run ID `20260901T2041218794300Z-pid23901-efa52409cc044c019abd9968690909aa` emitted:

`CL_CRITICAL_001_PRE` → `CL_CRITICAL_001_POST` → `CL_CRITICAL_002_PRE` → `INMETHOD_GS033 — Godot.OS::.cctor()`, then hard termination.

`INMETHOD_GS032 — Godot.OS::GetCmdlineArgs()` and `CL_CRITICAL_002_POST` did not appear. Therefore the exact four-reference BCL Dictionary rewrite physically works and moves the frontier past the 0.0.136/NATURAL dictionary failure, while the next natural failure is **inside `Godot.OS` type initialization before GetCmdlineArgs body entry**.

## Native callback interpretation

The read-only 0.0.138 GodotSharp map shows:

- the dictionary native thunk reads `NativeFuncs._unmanagedCallbacks.godotsharp_dictionary_new` and invokes it via `calli`;
- `Godot.OS::.cctor()` begins with `StringName` creation and repeated `ClassDB_get_method_with_compatibility` method-bind initialization;
- StringName construction and method-bind lookup likewise use function pointers stored under `NativeFuncs._unmanagedCallbacks`;
- `NativeFuncs.Initialize(IntPtr,int)` sets its initialized flag and copies the supplied unmanaged callback struct into `_unmanagedCallbacks`;
- the `GS021` marker for `NativeFuncs.Initialize` was not observed in the physical tails.

This strongly supports the architectural diagnosis that managed GodotSharp wrappers are being touched before the current Step-35 no-bootstrap policy has established their native callback table. It is not absolute proof of a null callback address because 0.0.138 did not log the pointer values themselves.

The final System.Collections / System.Collections.Concurrent resolver events remain context only. PRE/POST/GS ordering is the causal localization evidence.

## 0.0.139 Codemagic result

0.0.139 did **not** produce an IPA or physical evidence. Static validation passed at 837 checks. Codemagic executed **210** host tests: **209 passed, 1 failed**. The only failure was `OrderedDiagnosticLocalizationGatesReachFourOfFourWithoutClaimingClosure`, whose assertion still expected `STEP 35.0.15 DIAGNOSTIC LOCALIZATION COMPLETE — 4/4 — NOT STEP 35 CLOSURE` while the production summary correctly emitted Step 35.0.16. This was a test/provenance mismatch, not evidence against the OS-RECON/FORWARD runtime experiment.

## Step 35.0.17 / 0.0.140

0.0.140 changes no Step-35 runtime compatibility behavior from 0.0.139. It corrects the stale gate-summary assertion, advances all active release/diagnostic identity to Step 35.0.17 / 0.0.140, and statically requires production/test summary identity to agree before host testing.

0.0.140 does **not** bootstrap Godot and does **not** broaden native/runtime resolver authority. Gate A still re-manufactures/reverifies the exact closed Step-32 transformed source, writes same-run output-only maps, emits separately verified diagnostic derivatives, and immediately re-hashes the authoritative sources unchanged.

Three fresh-process modes are exposed:

1. **NATURAL — `NaturalGodotDictionaryRecon`**: preserve the original Godot Dictionary and natural Godot.OS path. This is primarily a regression/control mode now.
2. **OS-RECON — `ManagedDictionaryCompatibility`**: retain exactly four substitutions (`CommandLineHelper._args`, Dictionary `.ctor`, `set_Item`, `TryGetValue`) to `System.Collections.Generic.Dictionary<string,string>`, leave `Godot.OS.GetCmdlineArgs()` natural, and deepen the GodotSharp entry-marker closure rooted at `Godot.OS::.cctor()` and `Godot.OS/MethodName::.cctor()` through StringName/ClassDB/NativeFuncs local callees.
3. **FORWARD — `ManagedCommandLineCompatibility`**: apply those same four Dictionary substitutions plus exactly one call-site substitution in `CommandLineHelper..cctor`: natural `Godot.OS.GetCmdlineArgs()` is replaced by a local bridge method returning a new zero-length managed `string[]`. Post-write verification requires zero residual natural GetCmdlineArgs calls, exactly one local provider call, and provider IL equivalent to `ldc.i4.0; newarr System.String; ret`.

The FORWARD empty-array behavior is a diagnostic compatibility choice, not a final command-line policy.

Exact source pins remain unchanged. The owner-supplied source `sts2.dll` was previously verified at SHA-256 `e7ceb80669bfaf5c8fccabaa126ae2bb283aba514be5b5b55612579cfd285f18`, matching the closed Step-32 authority; observed `GodotSharp.dll` SHA-256 is `0e4897ecdfb31456a97c7d8028dfb8d7dbdc632e2f73fc9b438d7b266a139289` and remains observed input evidence rather than a promoted global pin.

The target remains `MegaCrit.Sts2.Core.Helpers.OneTimeInitialization::ExecuteVeryEarly()`, source token `0x06007D02`, async MoveNext source token `0x0600BC71`. A 0.0.140 diagnostic 4/4 from any mode is **NOT Step-35 closure**.
