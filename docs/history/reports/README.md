# Historical Reports

Selected build/device diagnostic outputs are retained here when they materially explain an architectural decision. Step records under `../steps/` remain the primary readable history.
- `STEP-25.0.1-PHYSICAL-GATE-H-REPORT.txt` — physical Step 25.0.1 / 0.0.81 report: Gates A–G PASS, Gate H FAIL at exact Harmony(string) invocation with missing `System.Environment.get_Version()`.
- `STEP-27.0.4-PHYSICAL-FRESH-PROCESS-GUARD-REPORT.txt` — physical 0.0.88 same-process retry rejected at Gate A because `sts2` remained resident in the dedicated Step-27 context; separate user observation reports abrupt process termination around N–Q with no managed report.

- `STEP-27.0.5-PHYSICAL-GATE-S-CRASH-CHECKPOINT.txt` — raw 0.0.89 synchronously flushed breadcrumb localizing the hard crash to Gate S/S1 inside `PatchProcessor.AddPrefix(MethodInfo)` before `Patch()`.
- `STEP-28.0-CODEMAGIC-CORE-COMPILE-FAILURE.txt` — raw 0.0.109 Codemagic host/build output: static validation and external fixtures succeeded, then Core compilation stopped on CS0246 for missing `CallbackProgress<>` before MSTest/iOS publish.
- `STEP-28.0.2-PHYSICAL-CLOSURE.txt` — raw physical 0.0.111 Step-28 report: A–E 5/5, transformed execution 1000 / 1041 / 1041, transformed-only identity admission, OfflineReady 428/428 after execution.
- `STEP-29.0-PHYSICAL-CLOSURE.txt` — raw physical 0.0.112 Step-29 report: A–D 4/4, exact selected `ModManager.TryLoadMod(Mod) -> Harmony.PatchAll(Assembly)` fingerprint, zero source mutation/CLR load, OfflineReady 428/428.
- `STEP-32.0-CODEMAGIC-HOST-TEST-FAILURE.txt` — raw 0.0.115 Codemagic report: 996/996 static validation, successful compile, 230/231 host tests; Gate C rejected an invalid pre-serialization offset-sensitive body-fingerprint prediction after the private rewrite was written.
- `STEP-32.0-CODEMAGIC-STATIC-VALIDATION.txt` — raw 0.0.115 canonical static-validation report: 996/996 PASS before the host suite exposed the Gate-C serialization-verifier defect.
- `STEP-32.0.2-PHYSICAL-SENTRY-CONSTANT-METADATA-FAILURE.txt` — raw physical 0.0.117 report: Gate A re-proved the exact receipt-backed source; Gate B failed closed before mutation when pre-write constant-metadata inventory found exact external scope `Sentry 5.0.0.0`.
- `STEP-32-STATIC-STS2-CONSTANT-METADATA-AUDIT.txt` — static-only audit of the exact Step-32 `sts2.dll`; proves 3,059 Constant rows, identifies the two Sentry default-parameter constants and seven System.Runtime `BindingFlags` constants, and records null-only external scopes without broadening resolver authority.
- `STEP-32.0.4-CODEMAGIC-HOST-FIXTURE-FAILURE.txt` — first 0.0.119 Codemagic attempt: 669/669 static validation, compile success, 183/186 host tests; all three failures occurred in synthetic System.Runtime fixture construction before production Step-32 behavior was exercised.
- `STEP-32.0.4-PHYSICAL-GATE-C-TRANSFORMED-METHOD-IDENTITY-FAILURE.txt` — raw physical 0.0.119 report: Step 32 reached 2/4; Gate A and the first real-StS2 private Gate-B semantic write passed, then Gate C failed at the old source-token-based transformed method identity/body check before semantic/constant-metadata reopen verification.
- `STEP-32.0.5-PHYSICAL-CLOSURE-4OF4.txt` — authoritative physical 0.0.120 Step-32 4/4 closure report; exact transformed SHA-256 and semantic verification, no trusted-source mutation or CLR admission.

- `STEP-33.0-CODEMAGIC-COLD-CACHE-HOST-COMPILE-FAILURE.txt` — first cache-tuned 0.0.121 Codemagic attempt: all configured caches were cold; host-test compilation stopped on removed MSTest `Assert.ThrowsException` before iOS publish/AOT, so the new AOT cache was not exercised or seeded.
- `STEP-33.0-PHYSICAL-CLOSURE-4OF4.txt` — authoritative physical 0.0.121 Step-33 report: A–D 4/4, exact transformed-primary CLR admission, zero managed/private/native admission-time expansion, original primary excluded, no game-member invocation.
- `STEP-34.0-CODEMAGIC-IOS-UI-COMPILE-FAILURE.txt` — first 0.0.122 CI attempt: 735/735 static and 194/194 host PASS, stable iOS cache restored, then iOS C# compilation stopped on a Step-34 `SystemButton` argument-type mistake before AOT/publish completion; no IPA/device evidence.

- `STEP-34.0-PHYSICAL-CLOSURE-4OF4.txt` — physical 0.0.122 Step-34 closure: exact transformed PrewarmJit invoked once and returned normally; 8 managed requests = 6 exact host + 2 initializer-free private loads; zero initializer-bearing/unplanned/native escape; 4/4 PASS.
