# Step 05.3 — SteamClient iOS Constructor Compatibility Test

## Build target

```text
artifacts/StS2-Launcher-Step-05.3.ipa
```

Required build telemetry:

```text
STEP05.3 STEAMKIT IOS PATCH: PASS
Replacement count: 1
Unsupported call removed: System.Diagnostics.Process.StartTime
Replacement value: System.DateTime.UtcNow
```

The Step 05.2 framework telemetry must also still show `DiskArbitration` before
filtering and absent afterward.

## Physical iPhone target

Expected marker:

```text
STEP 05.3 — IOS STEAMCLIENT COMPAT
Version 0.0.9
```

Target result:

```text
STEAM CONNECTION PASS — 3/3
```

Regression target:

```text
CORE SELF-TEST PASS — 12/12
```

## Report format

```text
STEP 05.3 RESULT

Codemagic build: PASS / FAIL
IPA produced: YES / NO
Install: PASS / FAIL
First launch: PASS / FAIL
Stayed open: YES / NO
SteamKit assembly: PASS / FAIL
Displayed SteamKit version:
Steam connection probe: 3/3 PASS / FAIL
ConnectedCallback: YES / NO / UNKNOWN
DisconnectedCallback: YES / NO / UNKNOWN
Core self-test: 12/12 PASS / FAIL
Exact Steam result/detail:

If build failed:
First fatal error:
STEP05.3 SteamKit patch line(s):
Artifacts ZIP uploaded: YES / NO
```

Authentication remains blocked until the physical iPhone reaches
`STEAM CONNECTION PASS — 3/3`.
