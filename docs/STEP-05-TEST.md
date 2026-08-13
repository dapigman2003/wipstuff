# Step 05 — Physical iPhone Steam Network Test

## Purpose

Prove three things independently of authentication:

1. SteamKit2 can be restored/published into the iOS application.
2. SteamKit2 can be loaded/executed under the iOS runtime/AOT environment.
3. SteamKit2 can connect to Steam and deliver callbacks on the physical iPhone.

No Steam credentials are entered or stored.

## First launch

Expected:

```text
STEP 05 — STEAM NETWORK PROBE
Version 0.0.6
NO LOGIN • NO PASSWORD • NO STEAM GUARD • NO TOKEN
STEAMKIT ASSEMBLY: PASS — ...
STEAM CONNECTION: NOT RUN
CORE LINK: PASS
```

The exact assembly version may contain four numeric components.

The app must remain open.

If it crashes before showing the screen, report that as the result — this would indicate a SteamKit/dependency/AOT load problem.

## Network test

Make sure the iPhone has normal Internet access.

Tap:

```text
Run Steam Connection Probe
```

During the test, expected temporary state:

```text
STEAM CONNECTION: CONNECTING…
```

Within the 20-second probe window, the desired final result is:

```text
STEAM CONNECTION PASS — 3/3
```

The detail should say that:

- the SteamKit client was constructed;
- `ConnectedCallback` was received;
- disconnect was requested;
- `DisconnectedCallback` was received;
- no authentication was attempted.

## What counts as a failure

Report the complete visible detail if you get:

```text
STEAM CONNECTION FAIL — 1/3
```

or:

```text
STEAM CONNECTION FAIL — 2/3
```

or:

```text
STEAM CONNECTION: EXCEPTION
```

Also report whether the app stayed responsive.

Do not retry repeatedly before reporting the first failure; the first result is the useful diagnostic.

## Regression checks

Tap:

```text
Run Core Self-Test
```

Expected:

```text
CORE SELF-TEST PASS — 12/12
```

The Step-04 dummy Keychain value should normally remain absent after the successful delete from Step 04:

```text
KEYCHAIN REGRESSION: PASS — Step-04 dummy value absent
```

Cycle/reset Core state once to confirm the previous architecture remains responsive.

## Report back

```text
STEP 05 RESULT

Install: PASS / FAIL
First launch: PASS / FAIL
Stayed open: YES / NO
SteamKit assembly load: PASS / FAIL
Displayed SteamKit version:
Steam connection probe: 3/3 PASS / FAIL
ConnectedCallback received: YES / NO / UNKNOWN
DisconnectedCallback received: YES / NO / UNKNOWN
Elapsed time shown:
Core self-test: 12/12 PASS / FAIL
Keychain regression: PASS / FAIL
Core state regression: PASS / FAIL

Exact Steam result/detail:
...

Anything else:
...
```

## Advancement rule

Do not add Steam authentication until `STEAM CONNECTION PASS — 3/3` is proven on the physical iPhone.
