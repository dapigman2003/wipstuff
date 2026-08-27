# Testing — Step 32.0.3 Retired Harmony Active-Surface Trim

Active candidate: Step 32.0.3 / `0.0.118 (118)`. This is a maintenance-only candidate; the Step-32 rewrite and known Sentry blocker are intentionally unchanged.

## Authority sequence

1. `scripts/validate.sh` — canonical static policy/provenance validation.
2. `scripts/test.sh` — active host regression suite, without the retired Step-25–27 Harmony suite or Harmony-Fat network acquisition.
3. `scripts/build-ios.sh` — publish/package the iOS candidate without the Step-27 interpreted fixture or retired Harmony UI/AOT surface.
4. `scripts/verify-ipa.sh` — verify release identity, native closure, active fixture rules, and IPA structure.
5. Compare Codemagic wall-clock/logs against the pre-trim 0.0.117 pipeline.

Release identity remains workflow `ios-step-32`, IPA `StS2-Launcher-Step-32.ipa`, TRX `step32.trx`, now version `0.0.118 (118)`.

### Maintenance-trim regression focus

Static/host/package guards must prove:

- the physically closed Step-25/26/27 runtime-Harmony implementation, dedicated tests, UI, preservation anchors, and Step-27 interpreted fixture are absent from the active source/project/package graph;
- active scripts contain no Harmony-Fat download, Step-27 fixture build, Step-27 environment variable, or Step-27 IPA payload rule;
- Step 24 controlled `0Harmony` initialization remains active and protected because it is a separately proven dependency-initialization capability;
- Step 28 ahead-of-load transformation remains active and its fixture is still built outside the iOS project graph and copied only after publish;
- `MtouchInterpreter=-all`, `MtouchLink=None`, and `TrimMode=copy` remain unchanged;
- current Step-29/30/31 physical evidence and Step-32 rewrite implementation remain protected;
- `RealStS2PrepareMethodRewrite.cs` is unchanged from 0.0.117.

### Known physical Step-32 state

Physical 0.0.117 passed Gate A and failed Gate B before mutation on unexpected external constant-metadata scope `Sentry, Version=5.0.0.0, Culture=neutral, PublicKeyToken=fba2ec45388e2af0`. This maintenance candidate does not correct that boundary.

A physical 0.0.118 run is therefore optional and is **not** expected to close Step 32. The next feature candidate should be designed only after static inspection of the exact Sentry constant metadata in the user's receipt-backed game files.

Codemagic `build-summary.txt` records per-stage elapsed seconds for SDK setup, static validation, host tests/fixture builds, iOS workload setup, iOS publish/package preparation, IPA verification, and the total canonical pipeline so the maintenance trim can be measured rather than inferred.
