# Current status

- Steps 01–05: **complete and physically verified**.
- Final foundation result: `FOUNDATION PASS — 5/5`.
- Core: `12/12`.
- Real iOS Keychain regression: `7/7`.
- Steam CM connection: `3/3`.
- Step 06: **passed** — real credential auth reached Steam Guard.
- Step 06.1: **passed** — mobile Steam Guard approval completed and authenticated identity returned.
- Step 06.2: **passed** — persistent refresh token + identity stored in real iOS Keychain; force-close/relaunch password-free resume passed; identity matched; sign-out/clear passed.
- Step 06.3: **partially proven** — automatic restore passed, but repeated real-device retries later produced transient/age-dependent `AccessDenied`.
- Current source step: **Step 06.3.1**.
- App version: **0.0.27 / build 27**.
- Fix boundary: align token logon with persistent-session semantics (`ShouldRememberPassword=true`), use a fresh per-attempt `LoginID`, avoid explicit successful-session `LogOff`, and expose non-secret refresh-token expiry timing.
- `AccessDenied` is preserved as non-destructive until a definitive expired/revoked/invalid credential result is observed.
- Password/Steam Guard secret persistence: forbidden.
- Ownership: intentionally deferred to Step 07.
