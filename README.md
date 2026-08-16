# StS2 Launcher iOS — Step 12.1

Experimental unofficial iOS launcher/compatibility-host foundation for users who legitimately own Slay the Spire 2 on Steam.

## Current boundary

**Step 12.1 — AOT receipt-serialization hotfix for the existing one-depot install / update / repair manager.**

Steps 01–11 remain regressions. Step 12.1 keeps the same Step 12 manager boundary and deliberately reuses the proven Step 11 resumable downloader as its Steam acquisition engine instead of creating a second CDN implementation.

The first physical-iPhone Step 12 (`0.0.33`) run processed all 428 planned files / 2,323,747,842 bytes but failed before commit with `NotSupportedException: ConstructorContainsNullParameterNames, StS2Launcher.Core.SteamManagedInstallReceipt`. Step 12.1 changes only receipt JSON serialization/deserialization to compile-time `System.Text.Json` source-generated metadata so it is safe under the existing full-trimming policy. See `docs/STEP-12.1-FIX.md`.

The manager:

1. restores/authenticates the saved Steam session through the existing boundaries;
2. re-proves ownership and discovers the current public manifest;
3. selects the same single direct public depot policy used by Steps 09–11;
4. classifies the stable managed install as `NotInstalled`, `UpToDate`, `UpdateAvailable`, or `RepairNeeded`;
5. acquires a fully verified manifest-specific Step 11 source only when work is required;
6. builds a non-secret local receipt containing only AppID/depot/manifest/branch and file path/length/SHA-1;
7. stages a complete replacement tree, reusing already-valid installed files when hashes match;
8. verifies the complete staging tree against the receipt;
9. preserves the previous managed install until commit;
10. replaces the stable install through a rollback-safe directory-rename commit and restores the prior install if replacement fails.

Stable managed files live beneath:

```text
Documents/StS2Launcher/Step12-ManagedInstall/Depot-<depot>/...
```

The receipt is:

```text
.sts2launcher-install.json
```

It contains no Steam password, refresh token, Guard data, ownership ticket, PICS token, depot key, manifest request code, CDN token, or downloaded payload bytes.

## Physical-device proof helpers

After proving the first Install run, Step 12/12.1 includes two local-only diagnostics:

- **Prepare Repair Test** flips one byte in one managed file. The next manager run must classify `RepairNeeded` and finish `REPAIR PASS`.
- **Prepare Update-State Test** changes only the project-owned local receipt manifest ID. The next manager run must classify `UpdateAvailable`, rediscover Steam's real current manifest, and finish `UPDATE PASS`.

These helpers exist only to deterministically exercise the manager branches. They do not fabricate Steam content or credentials.

## Explicitly not included

Step 12.1 does **not** add multi-depot app composition, compatibility inventory, Mono.Cecil rewriting, Godot/runtime execution, Steam Cloud, or Workshop.

See `docs/STEP-12-TEST.md` for the physical-device completion gate.
