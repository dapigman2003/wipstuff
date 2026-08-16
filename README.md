# StS2 Launcher iOS — Step 12.3

Experimental unofficial iOS launcher/compatibility-host foundation for users who legitimately own Slay the Spire 2 on Steam.

## Current boundary

**Step 12.3 — independently verified Step 11 cache reuse + stronger deterministic update test.**

Steps 01–11 remain physical-iPhone regressions. Step 12 is still the one-depot install/update/repair manager boundary; Step 12.3 fixes the cache-trust and test-telemetry weakness exposed while exercising its synthetic update path.

Physical-device/debug history:

- Step 12 / `0.0.33`: all 428 files / 2,323,747,842 bytes reached complete source/staging verification, then receipt JSON failed before commit with `ConstructorContainsNullParameterNames` under full trimming.
- Step 12.1 / `0.0.34`: receipt JSON moved to compile-time `System.Text.Json` metadata.
- The next device run reached source reacquisition and failed while materializing `Slay the Spire 2.pck` with `TimeoutException: The request timed out.`.
- Step 12.2 / `0.0.35`: direct iOS `TimeoutException` is treated as a bounded per-CDN endpoint failure and fails over just like the existing transient transport failures.
- The first Step 12.2 Codemagic run failed host compilation with `CS0160` because `HttpRequestException` appeared before derived `SteamKitWebRequestException` in two authenticated retry catch chains.
- Step 12.2.1 / `0.0.36`: corrected that catch ordering.
- Physical update-state testing then revealed that intentionally staling the Step 12 receipt caused an already-complete Step 11 cache to be discarded and reacquired. Cancelling during that phase also left the Step 12 result showing `Planned bytes: 0`, even though the current manifest had already been discovered.
- Step 12.3 / `0.0.37`: an existing manifest-specific Step 11 final cache is directly reverified against the freshly downloaded current Steam manifest (exact paths, sizes, and Steam SHA-1s). A stale Step 12 receipt is no longer used as the cache trust anchor. Only a cache that fails that independent manifest check is discarded and reacquired. Source progress is forwarded into the Step 12 UI, and planned file/byte telemetry is retained on cancel/timeout.

The synthetic update test is stronger in Step 12.3. It changes only the project-owned receipt: the manifest ID becomes stale and one smallest non-empty file identity is given a synthetic different SHA-1. Actual installed game bytes are untouched. The next manager run must therefore exercise the real `UpdateAvailable -> Update -> UpToDate` path, copy at least one file from the verified current source, verify the complete staging tree, write the real current receipt, and atomically replace the managed install.

## Manager guarantees

The Step 12 manager:

1. restores/authenticates the saved Steam session through the existing boundaries;
2. re-proves ownership and discovers the real current public manifest;
3. selects the same single direct public depot policy used by Steps 09–11;
4. classifies the stable managed install as `NotInstalled`, `UpToDate`, `UpdateAvailable`, or `RepairNeeded`;
5. obtains a current-manifest Step 11 source, reusing an existing final cache only after direct Steam-manifest verification;
6. builds a non-secret local receipt containing only AppID/depot/manifest/branch and file path/length/SHA-1;
7. stages a complete replacement tree, reusing installed files only when their recorded identity and actual SHA-1 match the current source;
8. verifies the staging tree against the current receipt;
9. preserves the previous managed install until commit;
10. performs a rollback-safe directory-rename commit and restores the prior install if replacement fails.

Stable managed files live beneath:

```text
Documents/StS2Launcher/Step12-ManagedInstall/Depot-<depot>/...
```

The receipt is `.sts2launcher-install.json`. It contains no Steam password, refresh token, Guard data, ownership ticket, PICS token, depot key, manifest request code, CDN token, or downloaded payload bytes.

## Physical-device proof helpers

After a successful Install and no-op/UpToDate run:

- **Prepare Repair Test** flips one byte in one managed file. The next manager run must classify `RepairNeeded` and finish `REPAIR PASS`.
- **Prepare Update Test** changes only the receipt (stale manifest + one synthetic changed-file SHA-1). The next manager run must classify `UpdateAvailable`, reverify the existing current-manifest Step 11 cache against Steam, show `Source bytes downloaded this manager run: 0` when that cache is valid, replace at least one file from the verified source, and finish `UPDATE PASS` with an atomic commit.

These helpers deterministically exercise manager branches; they do not fabricate Steam content or credentials.

## Explicitly not included

Step 12.3 does **not** add multi-depot app composition, compatibility inventory, Mono.Cecil rewriting, Godot/runtime execution, Steam Cloud, or Workshop.

See `docs/STEP-12-TEST.md` for the physical-device completion gate.
