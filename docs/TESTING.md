# Testing — Step 35.0 Controlled Transformed Real-StS2 Very-Early Initialization

Active candidate: Step 35.0 / `0.0.123 (123)`.

## Canonical authority chain

1. `python3 tools/validate_current.py`
2. complete host suite through `scripts/test.sh`
3. iOS publish/package through `scripts/build-ios.sh`
4. `scripts/verify-ipa.sh`
5. physical iPhone Step-35 A–D run from a **fresh process**

Release identity: IPA `StS2-Launcher-Step-35.ipa`, TRX `step35.trx`, version `0.0.123 (123)`. The Codemagic workflow key remains `ios-canonical` so NuGet/Godot/iOS arm64 `obj`/AOT caches survive numbered-step changes.

## Host/static expectations

Static validation must protect the physically closed Step-32/33/34 manifests and the active Step-35 candidate manifest. Step-35 host tests must protect ordered 4-gate completion, first-failure stopping, exact source target constants (`ExecuteVeryEarly` token `0x06007D02`, `<ExecuteVeryEarly>d__7::MoveNext` token `0x0600BC71`), initializer-free prepared dependency admission, and initializer-bearing dependency refusal. Active tests must use MSTest v4 `Assert.ThrowsExactly` APIs rather than removed `Assert.ThrowsException` APIs.

## Physical Gate A — VerifiedExecutionPreflight

Require a fresh process with no resident `sts2`. Re-run the physically closed Step-32 transform A–D. Require exact source SHA-256 `e7ceb80669bfaf5c8fccabaa126ae2bb283aba514be5b5b55612579cfd285f18`, exact transformed SHA-256 `39c0a89ad0d5c6eb1553e23dd8537a7b7ab8278fad4115d186db5751570211ef`, transformed length 9,304,576, identity/MVID, source token `0x06007D02`, and exact static parameterless Task signature. Independently reopen source/transformed images with a rejecting Cecil resolver; require identical semantic fingerprints for the wrapper and `<ExecuteVeryEarly>d__7::MoveNext`, source MoveNext token `0x0600BC71`, zero direct calls from MoveNext to `ExecuteEssential`/`ExecuteDeferred`/`PrewarmJit`, zero direct Harmony method references, and zero Cecil dependency resolution. Requalify the persisted zero-blocker prepared plan and exact sole initializer-bearing `0Harmony 2.4.2.0` dependency.

## Physical Gate B — ExecutionCapableClrAdmission

Immediately re-hash and `LoadFromStream` only the exact transformed primary into `StS2Launcher-Step35-VeryEarly`. Require exact identity/MVID/context ownership, exactly one resident `sts2`, and zero managed resolver requests/private loads/initializer-bearing requests/rejected requests/native attempts during primary admission.

## Physical Gate C — ExactExecuteVeryEarlyInvocation

Reflect only `MegaCrit.Sts2.Core.Helpers.OneTimeInitialization::ExecuteVeryEarly()` from the transformed assembly. Require static, parameterless, exact `System.Threading.Tasks.Task` return, Gate-A-discovered transformed MethodDef token, and exact MVID. Invoke exactly once and require a non-null `Task`; await it for at most **60 seconds**. The strict resolver may service only exact persisted host-framework bindings and exact hash-pinned initializer-free prepared private dependencies. Any initializer-bearing dependency request, unplanned managed request, native request, synchronous target exception, Task fault, cancellation, or timeout is a Gate-C failure and becomes the next evidence boundary.

## Physical Gate D — FinalIsolationAudit

Re-prove OfflineReady 428/428, receipt-backed source SHA-256, transformed SHA-256, runtime-plan SHA-256, every resident private dependency hash, unique transformed-primary residency/context ownership, zero initializer-bearing/unplanned/native escape, and exactly one Step-35 `ExecuteVeryEarly` invocation. The launcher must not intentionally invoke `ExecuteEssential`, `ExecuteDeferred`, `PrewarmJit`, the entry point, Harmony APIs, or Godot/game startup.

Preserve `Documents/StS2Launcher/Reports/Step35-TransformedRealStS2VeryEarlyInitialization.txt` whether PASS or FAIL. Accept only ordered A–D **4/4 PASS**. Do not rerun Step 35 in the same process after Gate B.
