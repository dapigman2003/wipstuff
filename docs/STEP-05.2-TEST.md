# Step 05.2 — iOS Framework Filter Test

## Build target

A successful build must produce:

```text
artifacts/StS2-Launcher-Step-05.2.ipa
```

The build log should contain both framework-filter lines. `DiskArbitration` should be present before filtering and absent afterward.

If native linking still fails, the important artifacts are:

```text
artifacts/logs/step05-2-framework-filter.log
artifacts/logs/step05-2-generated-linker-frameworks.txt
artifacts/logs/step05-2-native-symbols.log
artifacts/logs/step05-2-failure-scan.log
artifacts/logs/step05-2-publish.log
artifacts/logs/step05-2-dotnet-ios.binlog
```

## Physical iPhone target

Expected first screen:

```text
STEP 05.2 — IOS FRAMEWORK FILTER
Version 0.0.8

NO LOGIN • NO PASSWORD • NO STEAM GUARD • NO TOKEN

STEAMKIT ASSEMBLY: PASS — ...
STEAM CONNECTION: NOT RUN
CORE LINK: PASS
```

The target Steam result is:

```text
STEAM CONNECTION PASS — 3/3
```

Regression target:

```text
CORE SELF-TEST PASS — 12/12
```

Keychain and Core state controls must remain healthy.

## Report format

```text
STEP 05.2 RESULT

Codemagic build: PASS / FAIL
IPA produced: YES / NO

If build failed:
First fatal error:
Framework filter BEFORE:
Framework filter AFTER:
Any undefined DA*/IO* symbol:
Artifacts ZIP uploaded: YES / NO

If IPA built:
Install: PASS / FAIL
First launch: PASS / FAIL
Stayed open: YES / NO
SteamKit assembly load: PASS / FAIL
Displayed SteamKit version:
Steam connection probe: 3/3 PASS / FAIL
ConnectedCallback: YES / NO / UNKNOWN
DisconnectedCallback: YES / NO / UNKNOWN
Core self-test: 12/12 PASS / FAIL
Keychain regression: PASS / FAIL
Core state regression: PASS / FAIL
Exact Steam result/detail:
```

Authentication remains blocked until the IPA builds and the physical iPhone reaches `STEAM CONNECTION PASS — 3/3`.
