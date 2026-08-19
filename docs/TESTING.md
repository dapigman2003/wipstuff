# Testing — Current Step 22.3 Baseline

## Active local/Codemagic commands

Static validation:

```sh
bash scripts/validate.sh
```

Host unit tests (requires .NET SDK):

```sh
bash scripts/test.sh
```

iOS build (macOS/Xcode):

```sh
bash scripts/build-ios.sh
```

Final IPA audit (macOS):

```sh
bash scripts/verify-ipa.sh artifacts/StS2-Launcher-Step-22.3.ipa
```

Codemagic authoritative pipeline:

```sh
bash scripts/codemagic.sh
```

All current test/validation entry points produce plain-text results under `artifacts/reports/`.

## Physical acceptance order

Install Step 22.3 and confirm version 0.0.61.

1. Run Step 22 A–D regression. Require 4/4, explicit blockers 0, Runtime closure ready=YES.
2. Run Verify Offline-Ready Install. Require PASS.
3. Run Foundation 5/5 Regression. Require PASS.
4. Open Files → On My iPhone → StS2 Launcher → StS2Launcher → Reports and verify at least:
   - `Step22-HostBindingFrontier.txt`
   - `Step13-OfflineReady.txt`
   - `Foundation-5of5.txt`
5. Share the text files directly when diagnosing any failure.

Stop at the first failing compatibility/regression boundary. Do not begin Step 23 from a regressed foundation.
