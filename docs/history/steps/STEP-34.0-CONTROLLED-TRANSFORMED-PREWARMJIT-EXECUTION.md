# Step 34.0 — Controlled Transformed Real-StS2 PrewarmJit Execution

## Status

OPEN candidate: **0.0.122 (122)**.

Physical prerequisites:

- Step 32 CLOSED POSITIVE at 4/4 on 0.0.120.
- Step 33 CLOSED POSITIVE at 4/4 on 0.0.121.
- Exact transformed sts2.dll SHA-256: `39c0a89ad0d5c6eb1553e23dd8537a7b7ab8278fad4115d186db5751570211ef`.
- Exact transformed MVID: `518e4758-52d7-47c2-b776-471a0e29e49d`.
- Exact transformed `PrewarmJit()` semantic fingerprint: `47fadf2a46eda098f310b7d0ee54e37d1e952ac272fc966d16d557ed46a0b74a`.
- Exact transformed `PrewarmJit()` MethodDef token in the closed serialized image: `0x0600AFEA`.
- Step 33 proved transformed-primary CLR admission itself caused zero managed resolver requests, zero private dependency loads and zero native load attempts.

## Purpose

Step 34 is the first authorized execution of a real transformed StS2 game method. It does **not** authorize game startup. The only intentional game-member execution is:

`System.Void MegaCrit.Sts2.Core.Helpers.OneTimeInitialization::PrewarmJit()`

The Step-32 transformation is not changed. Step 34 re-manufactures and independently verifies the same exact transformed image, then executes only the already-audited compatibility site.

## Gate A — VerifiedExecutionPreflight

- Require a fresh process with no resident `sts2` assembly.
- Re-run Step 32 A–D to manufacture/reverify the exact closed transformed image.
- Require exact transformed hash, size, identity, MVID, semantic fingerprint, token `0x0600AFEA`, and zero `RuntimeHelpers.PrepareMethod` references.
- Re-run the Step-23 prepared runtime-plan preflight.
- Re-hash every prepared assembly and inspect module initializer metadata without dependency resolution.
- Require the previously established sole initializer-bearing private dependency to remain exact `0Harmony` 2.4.2.0.
- No StS2 CLR load, game reflection/invocation or native loading is authorized.

## Gate B — ExecutionCapableClrAdmission

- Create a dedicated `StS2Launcher-Step34-PrewarmJit` AssemblyLoadContext.
- Load only the exact transformed primary bytes via `LoadFromStream`.
- Re-prove the Step-33 physical admission behavior: zero resolver requests, zero private loads, zero initializer-bearing requests, zero rejected requests, and zero native attempts during primary admission.
- Stop before game type/member reflection.

## Gate C — ExactPrewarmJitInvocation

- Reflect only exact type `MegaCrit.Sts2.Core.Helpers.OneTimeInitialization` from the transformed resident assembly.
- Bind only static parameterless void `PrewarmJit()`.
- Require transformed MethodDef token `0x0600AFEA` and the closed MVID.
- Invoke that method **exactly once**.
- The execution resolver may service only:
  - exact persisted Step-21/22 host-framework bindings from `AssemblyLoadContext.Default`;
  - exact hash-pinned prepared private dependencies whose module initializer count is zero.
- Any initializer-bearing dependency request, including `0Harmony`, fails closed.
- Any unplanned managed request or native request fails closed.
- Preserve the exact inner exception and resolver state if invocation fails; do not guess or broaden authority in the same candidate.

## Gate D — FinalIsolationAudit

- Re-prove OfflineReady.
- Re-hash the receipt-backed source, transformed image, runtime-binding plan, and every loaded private prepared dependency.
- Require transformed `sts2` to remain the unique resident StS2 primary in the dedicated Step-34 context.
- Require every resident private dependency to be an initializer-free exact prepared-plan member.
- Require zero initializer-bearing, unplanned managed or native requests.
- Require exactly one successful target invocation and no entry-point/Godot/Harmony broader startup.

## Explicitly forbidden

Step 34 does not authorize:

- CLR admission of the receipt-backed/prepared original `sts2.dll`;
- intentional invocation of any game method other than exact transformed `PrewarmJit()`;
- the game entry point or broad initialization sequence;
- initializer-bearing `0Harmony` admission;
- Harmony/MonoMod API invocation or runtime patching;
- native game libraries;
- Godot/game startup;
- arbitrary resolver fallback.

## Physical close condition

`Step34-TransformedRealStS2PrewarmJitExecution.txt` must report **4/4 PASS** on exact 0.0.122 after canonical Codemagic validation, host tests, iOS publish and IPA verification.

A 4/4 pass proves only that the exact transformed `PrewarmJit()` compatibility site returns normally under the strict prepared resolver on physical iOS. The next managed-initialization boundary remains separately designed and authorized.
