# Step 12.4 — post-Step-12 stabilization

Step 12 was completed on a physical iPhone in Step 12.3. Step 12.4 adds no Step 13 capability. It is a cleanup and defensive bug-hardening release intended to become the new baseline only after Codemagic and a short physical-device regression pass.

## Changes

1. **Malformed receipt hardening**
   - A local receipt must now have a non-zero app/depot/manifest identity, a non-empty branch, safe case-unique relative paths, non-negative lengths, and 40-digit hexadecimal SHA-1 values.
   - A structurally invalid receipt is treated as unreadable/`RepairNeeded` instead of being allowed to trigger incidental null/overflow/path exceptions.
   - State classification also rejects a receipt for the wrong App ID, wrong depot, or wrong branch.

2. **Interrupted receipt-write cleanup**
   - Receipt writes still use the Step 12.1 source-generated JSON contract.
   - A failed or cancelled write now removes `.sts2launcher-install.json.tmp` best-effort so the temp file cannot poison the otherwise-valid managed tree on the next inspection.

3. **Result-finalization robustness**
   - Staging/backup cleanup or rollback I/O failures are appended to the manager result instead of escaping from result construction and masking the original failure/cancellation.
   - The existing `StagingAbsentAfterResult` / `BackupAbsentAfterResult` telemetry remains authoritative.

4. **Step 11 resume-scan accounting fix**
   - Reused-chunk/byte counters are now committed only after the complete existing `.part` file checksum scan succeeds.
   - If an I/O error occurs halfway through that scan and the partial file is discarded, already-scanned chunks are no longer left counted and then counted again when redownloaded.

5. **Unreadable final-cache recovery**
   - If a manifest-specific Step 11 final cache becomes unreadable while its files are being re-hashed, the cache is treated as untrusted so Step 12 can discard/reacquire it rather than aborting the manager as an unrelated exception.

6. **Legacy CDN timeout consistency**
   - The already-proven iOS `TimeoutException` failover behavior from Step 12.2/Step 11 is mirrored into the still-exposed Step 09 single-file and Step 10 full-depot regression paths.
   - `SteamKitWebRequestException` remains before its `HttpRequestException` base catch in authenticated retry chains.
   - The proven HTTP-handler policy is unchanged: `SocketsHttpHandler` is still dedicated to `CMWebSocket`; CDN traffic stays on the platform-default client.

7. **Cleanup**
   - Stale Step 06/08 startup labels were removed.
   - Host-test/Codemagic artifact names now identify Step 12.4.
   - Foundation verification now disables the other Steam-operation controls while it runs, avoiding accidental concurrent Steam probes.

## Deliberately unchanged

- SteamKit2 `3.4.0`.
- `TrimMode=full` and the three trimmer roots.
- DiskArbitration linker filter.
- build-only Process.StartTime SteamKit patch.
- authentication/session/ownership/discovery boundaries.
- Step 11 resumable acquisition design.
- Step 12.1 source-generated receipt JSON.
- Step 12.3 independently verified cache reuse and deterministic update test.
- install/update/repair staging, complete SHA-1 verification, and rollback-safe directory replacement.

No offline launcher state, multi-depot composition, compatibility inventory, Godot/runtime, Cloud, Workshop, or other Step 13+ work is included.

## Regression gate before adopting 12.4 as the baseline

Run Codemagic workflow `ios-step-12-4`, install `0.0.38 (38)`, then on the physical iPhone:

1. Run **Foundation 5/5 Regression** and require `FOUNDATION PASS — 5/5`.
2. Run **Inspect + Install / Update / Repair** on the existing current install and require `UpToDate -> None -> UpToDate`.
3. Run **Prepare Repair Test**, then the manager, and require `REPAIR PASS` ending `UpToDate`.
4. Run **Prepare Update Test**, then the manager, and require `UPDATE PASS`, at least one replaced file, atomic commit, and final `UpToDate`.

The already-completed Step 12 capability remains closed while this stabilization build is being verified. Do not start Step 13 as part of this pass.
