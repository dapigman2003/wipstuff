# Step 33.0 — Verified Transformed Real-StS2 CLR Admission

## Authorization

Physical Step 32.0.5 / 0.0.120 is CLOSED POSITIVE at 4/4. The exact launcher-private transformed `sts2.dll` is 9,304,576 bytes, SHA-256 `39c0a89ad0d5c6eb1553e23dd8537a7b7ab8278fad4115d186db5751570211ef`, assembly identity `sts2, Version=0.1.0.0, Culture=neutral, PublicKeyToken=null`, MVID `518e4758-52d7-47c2-b776-471a0e29e49d`, and transformed PrewarmJit semantic fingerprint `47fadf2a46eda098f310b7d0ee54e37d1e952ac272fc966d16d557ed46a0b74a`.

Step 33 is authorized only to test CLR admission of that independently reverified transformed image. Execution remains a later boundary.

## Gate model

- **Gate A — VerifiedTransformedImagePreflight.** Start from a fresh process. Re-run Step 32 A–D to manufacture a fresh transformed image, then require the exact physically closed transformed hash/length/identity/MVID/semantic fingerprint and zero PrepareMethod references. Re-run the existing Step-23 prepared-runtime preflight to requalify the persisted Step-21/22 zero-blocker plan and prepared set. No StS2 CLR admission occurs.
- **Gate B — TransformedPrimaryClrAdmission.** Re-hash the transformed image immediately before `LoadFromStream`, load those exact bytes into a dedicated private `AssemblyLoadContext`, and verify loaded assembly identity, MVID, context ownership, and unique `sts2` residency. The original receipt-backed/prepared primary is not a load input. No game member is reflected or invoked.
- **Gate C — AdmissionOnlyResolverAudit.** Require the Step-33 private context to contain only the transformed primary. Private prepared dependency requests are refused and fail closed. Unplanned managed resolution and native resolution are refused. Exact planned host-framework bindings may be returned from `AssemblyLoadContext.Default` only if the CLR itself requests them during primary admission.
- **Gate D — FinalIsolationAudit.** Re-prove OfflineReady, receipt-backed original SHA-256, transformed SHA-256, runtime-plan SHA-256, unique transformed-context residency, zero private dependency/native expansion, and zero game invocation/startup.

## Forbidden

Step 33 does not invoke `PrewarmJit`, reflect game types/members, run an entry point, load private game dependencies, start Godot/game logic, load native game libraries, mutate the trusted install, use Harmony/MonoMod runtime patching, or enable arbitrary resolver fallback.

## Physical close condition

One exact 0.0.121 candidate must pass Codemagic static validation, the full host suite, iOS publish/IPA verification, then a fresh-process physical Step 33 A–D run at **4/4**. The authoritative device report is `Step33-TransformedRealStS2AssemblyAdmission.txt`.

A 4/4 PASS authorizes design of a later separately gated controlled transformed-site execution boundary; it does not itself prove `PrewarmJit()` or broader initialization executes successfully.
