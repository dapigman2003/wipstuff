# Current status

**Steps 01–12 are complete and closed on a physical iPhone.**

**Current physically exercised baseline:** Step 12.4.1 / `0.0.39`, including the download-cache clear and forced fresh-CDN update regression.

**Current source candidate:** Step 13 — offline launcher state.

- App version: `0.0.40 (40)`
- Codemagic workflow: `ios-step-13`
- Expected IPA: `artifacts/StS2-Launcher-Step-13.ipa`

Step 13 adds exactly one capability: inspect the already-managed Step 12 depot using local storage only and classify it as:

- `OnlineSetupRequired` — no managed install exists;
- `OfflineReady` — the source-generated receipt is valid for App ID 2868840 and the exact local tree matches all recorded lengths/SHA-1s;
- `RepairRequired` — the local receipt/layout/tree is missing, malformed, foreign, incomplete, extra, length-mismatched, or hash-mismatched.

The Step 13 inspector does not accept or consult a Steam session and contains no Steam client/HTTP/WebSocket/PICS/CDN path. `OfflineReady` therefore remains available when Steam is unreachable. Online manifest freshness is intentionally unknown until the existing Step 12 online manager is run again.

**Step 13 is not complete until the physical-iPhone offline gate in `docs/STEP-13-TEST.md` passes.**

No Step 14 compatibility-inventory work has started.
