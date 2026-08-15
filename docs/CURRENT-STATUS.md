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
- Step 07: **passed on physical iPhone** — App ID **2868840** returned an exact-AppID, `EResult.OK`, non-empty ownership ticket and no content request followed.
- Current source step: **Step 08**.
- App version: **0.0.29 / build 29**.
- New boundary: PICS app metadata discovery for App ID **2868840** after re-proving Step 07 ownership.
- Success condition: target app info returned without missing-token state, at least one numeric depot found, and at least one visible branch manifest ID found.
- Displayed discovery metadata: depot ID, optional `oslist` / `osarch` / `language`, branch name, manifest ID, PICS change number.
- PICS access-token value: **never displayed/logged/persisted**.
- Depot keys / manifest bodies / CDN / chunks / file download: **not implemented**.
