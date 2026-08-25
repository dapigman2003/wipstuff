# Testing — Step 29 Real StS2 Compatibility Target Audit

## Static validation

Run `bash scripts/validate.sh`.

The active candidate is Step 29.0 / `0.0.112 (112)`. Validation must preserve the physically closed Step-28 implementation/evidence while pinning the new read-only target-audit boundary:

- Step 28 physical closure report is present and records **5/5**, **1000 / 1041 / 1041**, and post-execution **OfflineReady 428/428**;
- Step-28 production mechanism remains hash-protected;
- Step 29 opens only the exact receipt-backed ARM64 `sts2.dll` with Cecil `ReadingMode.Deferred` and a rejecting resolver;
- Step 29 performs zero `ModuleDefinition.Write`, `Assembly.Load`, `AssemblyLoadContext` load, StS2 reflection/invocation, Harmony/MonoMod runtime patching, Godot/game startup or native game loading;
- exact candidate evidence includes source method token, IL offset/opcode, target scope/member and source method-body SHA-256;
- `Expression.Compile` is explicitly excluded from candidate selection because Step 19 already physically closed that compatibility question;
- version/build/workflow/IPA/TRX identity is `0.0.112 (112)` / `ios-step-29` / `StS2-Launcher-Step-29.ipa` / `step29.trx`.

## Host tests

Run `bash scripts/test.sh`.

The host suite retains all earlier regression fixtures, including Step 28, and adds synthetic Step-29 coverage proving:

- ordered 4/4 gate accounting and stop-on-first-failure;
- deterministic candidate priority;
- exact Harmony runtime-patch selection ahead of later platform surfaces;
- Step-19 `Expression.Compile` exclusion;
- receipt-backed source hash stability;
- zero CLR load / zero Cecil write semantics.

## Codemagic

Run workflow:

```text
ios-step-29
```

Authority sequence remains:

1. canonical static validation;
2. complete host tests;
3. iOS publish;
4. IPA verification;
5. physical iPhone Step 29 A–D.

Do not infer device success from CI alone.

## Physical Step 29

Use a fresh process and the existing good Step-12 managed install. Do not run a real-StS2 load boundary first.

Tap the Step-29 A–D control and preserve:

`Documents/StS2Launcher/Reports/Step29-RealStS2CompatibilityTargetAudit.txt`

Required close condition:

```text
REAL STS2 COMPATIBILITY TARGET AUDIT PASS — 4/4
```

If Gate C selects a candidate, preserve its exact category, source method, metadata token, IL offset/opcode, target scope/member and method-body SHA-256. If Gate C reports `NO DIRECT PRIMARY TARGET`, preserve that result; do not manually choose a different target in the same candidate.
