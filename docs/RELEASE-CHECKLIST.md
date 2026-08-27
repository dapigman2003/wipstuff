# Release Checklist — Step 32.0.4 Audited Constant-Metadata Resolver

## Candidate identity

- step/candidate: **Step 32.0.4**
- version: `0.0.119 (119)`
- workflow: `ios-step-32`
- expected IPA: `artifacts/StS2-Launcher-Step-32.ipa`
- latest physical Step-32 evidence: 0.0.117 Gate A PASS / Gate B Sentry fail-closed before mutation
- static correction evidence: `docs/history/reports/STEP-32-STATIC-STS2-CONSTANT-METADATA-AUDIT.txt`
- lean baseline: user-confirmed Codemagic 0.0.118

## Required before device testing

- [ ] release identity is exactly `0.0.119 (119)`;
- [ ] canonical static validation passes;
- [ ] complete active host suite passes;
- [ ] host tests cover the exact three audited external constant type/storage requirements;
- [ ] host tests prove an unaudited external constant requirement fails closed before transformation output;
- [ ] iOS publish/package succeeds with `MtouchInterpreter=-all`, `MtouchLink=None`, `TrimMode=copy`;
- [ ] IPA verification passes;
- [ ] no proprietary `sts2.dll`, app bundle, credentials, or signing secrets are present in the source archive;
- [ ] the static audit remains present under `docs/history/reports` and hash-pinned.

## Physical acceptance

- [ ] install the verified IPA;
- [ ] force-quit before the Step-32 run;
- [ ] legitimate Step-12 install is OfflineReady;
- [ ] Gate A re-proves exact receipt/source identity and 10/10 PrepareMethod sites;
- [ ] Gate B reports exactly 3/3 audited constant requirements across 2/2 exact scopes and writes the transformed private image;
- [ ] Gate B opens zero external dependency bytes and accepts no unplanned resolution identity;
- [ ] Gate C proves 10 → 0 PrepareMethod references and unchanged constant-metadata semantic fingerprint;
- [ ] Gate D re-proves trusted-install isolation, OfflineReady, and zero real-StS2 CLR admission/invocation;
- [ ] final result is `REAL STS2 PREPAREMETHOD REWRITE PASS — 4/4`.

## Failure discipline

Do not broaden authority to make the run advance. A request for GodotSharp, System.Collections, another Sentry identity/type, another storage type, or another external assembly is new evidence and must fail closed. Do not enable Cecil default resolution, search paths, trimming/linking, runtime Harmony patching, real-game CLR admission, Godot/game startup, or native loading as part of Step 32.0.4.
