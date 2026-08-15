# Current status

- Steps 01–05: **complete and physically verified**.
- Foundation: `FOUNDATION PASS — 5/5`.
- Core: `12/12`.
- Real iOS Keychain regression: `7/7`.
- Steam CM connection: `3/3`.
- Step 06: **passed** — real credential authentication reached Steam Guard.
- Step 06.1: **passed** — mobile Steam Guard approval completed and authenticated identity returned.
- Step 06.2: **passed** — refresh token + identity stored in real iOS Keychain; password-free relaunch/resume and sign-out passed.
- Step 06.3.1: **passed** — persistent token semantics corrected; automatic and manual saved-session login remain reliable over time.
- Current source step: **Step 07**.
- App version: **0.0.28 / build 28**.
- New boundary: request an app ownership ticket for Slay the Spire 2, App ID **2868840**, after saved-session authentication and SteamID match.
- Success condition: exact callback AppID + `EResult.OK` + non-empty ownership ticket.
- Ownership ticket payload: never displayed/logged/persisted.
- PICS/depot/manifest/CDN/download: **not implemented**.
