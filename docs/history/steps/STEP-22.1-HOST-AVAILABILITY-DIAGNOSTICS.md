# Step 22.1 — Host Framework Availability Diagnostics

Step 22 physically failed at Gate A (`RootedHostAvailability`) before any real StS2 CLR load.
The original Step 22 Gate A stopped at the first failed framework identity, which was not enough
to distinguish one missing/transitive root from a broader iOS host-availability problem.

Step 22.1 is diagnostic-only. It preserves the exact Step 22 22-seed root set and the Step 21/21.1
binding implementation. Gate A now performs two ordered passes:

1. Exact identity pass over all 44 measured framework identities. No diagnostic fallback loads are
   allowed during this pass, so one failed probe cannot alter the result of a later exact probe.
2. After every exact result is frozen, failed simple names receive a simple-name load probe. This
   distinguishes an assembly that is not host-loadable at all from one that is available but fails
   the requested version/token identity qualification.

Every Gate A run writes:

`Documents/StS2Launcher/Step22.1-HostFrameworkAvailabilityDiagnostics.txt`

The app already exposes Documents through iOS Files (`UIFileSharingEnabled` and
`LSSupportsOpeningDocumentsInPlace`). The report contains only runtime/framework assembly metadata,
not Steam credentials/tokens or Apple signing material.

Gate A still passes only when all 44 exact identity probes qualify. Gates B-D are unchanged in
purpose and are not considered tested when Gate A fails.
