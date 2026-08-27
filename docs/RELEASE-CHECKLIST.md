# Release Checklist — Step 32.0.5 Stable Transformed Method Verification

## Candidate identity

- step/candidate: **Step 32.0.5**
- version: `0.0.120 (120)`
- workflow: `ios-step-32`
- expected IPA: `artifacts/StS2-Launcher-Step-32.ipa`
- expected device report: `Documents/StS2Launcher/Reports/Step32-RealStS2PrepareMethodRewrite.txt`

## Required before device testing

- [ ] release identity is exactly `0.0.120 (120)`;
- [ ] canonical static validation passes;
- [ ] complete active host suite passes;
- [ ] iOS publish succeeds with `MtouchInterpreter=-all`, `MtouchLink=None`, `TrimMode=copy`;
- [ ] IPA verification succeeds;
- [ ] Step-32 source archive contains no proprietary `sts2.dll` or game payload;
- [ ] physical 0.0.119 2/4 report is preserved in history;
- [ ] 0.0.120 production diff is limited to Gate-C stable transformed-method verification/diagnostics plus release/docs/tests.

## Physical run

Use a fresh app process. Gate A must re-prove the exact receipt-backed source, OfflineReady 428/428, source MethodDef token `0x06007D05`, 10/10 PrepareMethod sites, zero Cecil read-time resolution, zero real-StS2 CLR admission, and no trusted-install mutation.

Gate B must retain the exact 6/6 + 4/4 rewrite, three audited external constant requirements, exact System.Runtime/Sentry scopes only, zero external dependency-byte reads, and launcher-private transformed output.

Gate C must locate exactly one transformed `OneTimeInitialization::PrewarmJit()` by exact declaring type + full signature, then verify zero PrepareMethod references, the exact planned transformed semantic fingerprint, unchanged Constant-table semantic fingerprint, expected instruction/EH/Pop shape, assembly identity/MVID, and zero reopen resolution/CLR admission. The transformed token and old-source-token occupant are diagnostics only.

Gate D must re-prove trusted/source/transformed hashes, OfflineReady, and no real-StS2 CLR admission.

## Failure discipline

Do not broaden authority to make the run advance. A new resolver request, stable-identity ambiguity, semantic-fingerprint mismatch, Constant-table change, instruction/EH/Pop drift, or isolation failure is new evidence and must fail closed. Do not enable Cecil default resolution/search paths, trimming/linking, runtime Harmony patching, real-game CLR admission, Godot/game startup, or native loading as part of Step 32.0.5.

Step 33 remains unauthorized until physical A–D close **4/4**.
