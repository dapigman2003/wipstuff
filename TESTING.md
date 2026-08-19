# Testing strategy — through Step 17 compatibility call-site analysis

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


## Step 12 / 12.1 / 12.2 / 12.2.1 / 12.3 / 12.4 / 12.4.1
See `STEP-12-TEST.md` for install / update / repair manager verification.

Step 12.1 failure localization and AOT receipt hotfix notes: `docs/STEP-12.1-FIX.md`.

Step 12.2 iOS CDN timeout/failover localization and hotfix notes: `docs/STEP-12.2-FIX.md`.

Step 12.3 verified cache reuse and stronger update-gate notes: `docs/STEP-12.3-FIX.md`.

Step 12.4 post-completion cleanup/hardening and regression gate: `docs/STEP-12.4-STABILIZATION.md`.

Step 12.4.1 download-cache clear and forced fresh-CDN regression: `docs/STEP-12.4.1-CACHE-TEST.md`.


Step 13 offline launcher-state gate: `docs/STEP-13-TEST.md`.


## Steps 14–17

Step 14 read-only compatibility inventory: `docs/STEP-14-TEST.md`.

Step 15 Godot Foundation ordered native/render/touch/lifecycle gates: `docs/STEP-15-TEST.md`.

Step 16 Managed Preparation ordered Cecil fixture/read-write-rewrite + real metadata gates: `docs/STEP-16-TEST.md`.

Step 17 concrete ARM64 IL/native/dependency analysis: `docs/STEP-17-TEST.md`.
