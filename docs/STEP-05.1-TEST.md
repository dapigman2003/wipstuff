# Step 05.1 — SteamKit iOS Link-Fix Test

## Phase A — Codemagic compile/package test

Expected outcome:

```text
Step 05.1 repository validation passed.
Publishing Step 05.1 with TrimMode=full...
...
Created .../artifacts/StS2-Launcher-Step-05.1.ipa
Step 05.1 IPA verification passed.
```

The previous error must be absent:

```text
ld: framework 'DiskArbitration' not found
```

### If the build still fails

Do not change anything else.

Send:

1. the first fatal error;
2. `artifacts/logs/step05-1-native-preflight.log`;
3. `artifacts/logs/step05-1-failure-scan.log` if it exists.

The failure scan is designed to tell us which package/intermediate file still contains the native `DiskArbitration` reference.

## Phase B — physical iPhone test

Only if the IPA builds.

Expected first screen:

```text
STEP 05.1 — STEAM LINK FIX
Version 0.0.7

NO LOGIN • NO PASSWORD • NO STEAM GUARD • NO TOKEN

STEAMKIT ASSEMBLY: PASS — ...
STEAM CONNECTION: NOT RUN
CORE LINK: PASS
```

The app must stay open.

## Phase C — network probe

Tap:

```text
Run Steam Connection Probe
```

Expected temporary result:

```text
STEAM CONNECTION: CONNECTING…
```

Target result within the 20-second probe:

```text
STEAM CONNECTION PASS — 3/3
```

That means:

- SteamClient constructed;
- ConnectedCallback received;
- DisconnectedCallback received.

No account authentication is attempted.

## Regression checks

Run:

```text
Run Core Self-Test
```

Expected:

```text
CORE SELF-TEST PASS — 12/12
```

Keychain regression should remain healthy, and Core state controls should remain responsive.

## Report back

```text
STEP 05.1 RESULT

Codemagic build: PASS / FAIL
IPA produced: YES / NO

If build failed:
First fatal error:
...
Native-preflight relevant lines:
...
Failure-scan relevant lines:
...

If IPA built:
Install: PASS / FAIL
First launch: PASS / FAIL
Stayed open: YES / NO
SteamKit assembly load: PASS / FAIL
Displayed SteamKit version:
Steam connection probe: 3/3 PASS / FAIL
ConnectedCallback received: YES / NO / UNKNOWN
DisconnectedCallback received: YES / NO / UNKNOWN
Core self-test: 12/12 PASS / FAIL
Keychain regression: PASS / FAIL
Core state regression: PASS / FAIL

Exact Steam result/detail:
...
```

## Advancement rule

Authentication remains blocked until Step 05.1 both:

1. produces an IPA without the DiskArbitration native-link failure; and
2. reaches `STEAM CONNECTION PASS — 3/3` on the physical iPhone.
