# Testing strategy — Step 11

Codemagic performs source-policy validation, host unit tests, the iOS AOT/native build and IPA structure verification before device testing.

Host tests retain the older foundation/auth/session/ownership/discovery/content contracts and add Step 11 pure-policy coverage for:

- target App ID remains `2868840`;
- Step 10 progress enum values remain stable;
- Step 11 adds a separate `Resuming` progress phase;
- the local Adler-32 implementation matches a standard known vector;
- streaming Adler-32 matches the in-memory implementation;
- Step 11 result telemetry cannot expose raw downloaded bytes or Steam token/key/request-code values.

Physical-device proof remains mandatory because host tests cannot establish iOS lifecycle behavior, abrupt process termination, Keychain behavior, Steam CM/CDN connectivity, filesystem persistence, or atomic directory behavior on the actual device.

See `STEP-11-TEST.md` for the required force-quit/relaunch/resume sequence.


## Step 12
See `STEP-12-TEST.md` for install / update / repair manager verification.
