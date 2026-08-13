# StS2 Launcher iOS — Step 04

Steps 01.1–03 proved:

- UIKit scene/window startup
- native UI rendering
- sandbox file writing
- separate `StS2Launcher.Core` assembly
- Core-driven launcher state transitions

Step 04 introduces exactly one new platform subsystem:

```text
StS2Launcher.Core.ICredentialStore
               ↑
      iOS Keychain adapter
```

## Important

This build stores **no Steam credentials**.

It uses only two fixed dummy strings:

```text
STEP04-ALPHA
STEP04-BETA
```

under a Step-04-specific Keychain service name.

## What the test proves

1. missing record can be queried cleanly;
2. dummy value can be written;
3. dummy value can be read;
4. same logical key can be overwritten;
5. new value is returned instead of old value;
6. value survives process termination/relaunch;
7. dummy value can be deleted.

## Build

Codemagic workflow:

```text
ios-step-04
```

Expected artifact:

```text
artifacts/StS2-Launcher-Step-04.ipa
```

See `docs/STEP-04-TEST.md` before testing.
