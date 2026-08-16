# Step 12.2.1 — catch-order compile hotfix

The first Step 12.2 Codemagic attempt passed static validation but failed during host compilation with `CS0160` at the two authenticated CDN retry catch chains in `SteamResumableDepotDownloadAttempt`.

`SteamKitWebRequestException` derives from `HttpRequestException`. Step 12.2 accidentally placed the broader `HttpRequestException` catch before the SteamKit-specific catch, so the latter was unreachable.

Step 12.2.1 reorders only those two catch chains so `SteamKitWebRequestException` comes first. The direct `TimeoutException` failover added by Step 12.2 remains unchanged. No install-manager, receipt, Steam session, CDN handler, verification, resume, or atomic-commit policy is broadened.

The Step 12 validator now also checks the authenticated retry catch ordering so this exact `CS0160` regression cannot pass static validation again.
