# Release Checklist — Step 34.0 Controlled Transformed Real-StS2 PrewarmJit Execution

## Candidate identity

- step/candidate: **Step 34.0**
- version: `0.0.122 (122)`
- workflow: `ios-canonical`
- expected IPA: `artifacts/StS2-Launcher-Step-34.ipa`
- expected device report: `Documents/StS2Launcher/Reports/Step34-TransformedRealStS2PrewarmJitExecution.txt`

## Required before device testing

- [ ] release identity is exactly `0.0.122 (122)`;
- [ ] canonical static validation passes;
- [ ] complete active host suite passes, including Step-34 gate ordering / strict execution-resolver tests;
- [ ] iOS publish succeeds with `MtouchInterpreter=-all`, `MtouchLink=None`, `TrimMode=copy`;
- [ ] IPA verification succeeds;
- [ ] `ios-canonical` remains the stable Codemagic workflow key and configured NuGet/Godot/iOS-arm64 `obj` caches remain enabled;
- [ ] source archive contains no proprietary `sts2.dll` or game payload;
- [ ] physical Step-32 0.0.120 and Step-33 0.0.121 4/4 closure reports are preserved in history;
- [ ] exact closed transformed SHA-256 `39c0a89ad0d5c6eb1553e23dd8537a7b7ab8278fad4115d186db5751570211ef`, MVID, semantic fingerprint, and transformed token `0x0600AFEA` remain pinned;
- [ ] no Step-34 code authorizes the receipt-backed original, game entry point, `0Harmony`, Harmony/MonoMod patching, native loading, or Godot/game startup.

## Physical run

Use a fresh app process. Do not run Step 23/24/33 or other real-game CLR-load buttons first and do not start Godot.

Gate A must re-run the exact closed Step-32 transformation, re-prove the transformed target, requalify the zero-blocker runtime plan, re-hash all prepared assemblies, and confirm exact `0Harmony 2.4.2.0` is still the sole initializer-bearing private dependency. No CLR admission yet.

Gate B must `LoadFromStream` only the exact transformed primary into `StS2Launcher-Step34-PrewarmJit` and re-prove the Step-33 zero-resolution primary-admission behavior.

Gate C must bind only exact transformed `OneTimeInitialization::PrewarmJit()` and invoke it exactly once. Only exact planned host bindings and hash-pinned initializer-free private dependencies may resolve. Initializer-bearing/unplanned/native requests fail closed.

Gate D must re-prove OfflineReady, original/transformed/plan/dependency hashes, unique transformed-context residency, clean resolver/native isolation, and one exact PrewarmJit invocation.

## Failure discipline

Do not broaden authority to make the run advance. A transformed identity/hash/semantic mismatch, changed initializer classification, target binding failure, managed/native resolver rejection, dependency drift, or exception from exact PrewarmJit is new evidence and must fail closed.

A Step-34 4/4 PASS authorizes only design of a later separately gated progressive managed-initialization boundary. It does not authorize broad game startup or Godot integration by itself.
