# StS2 Launcher iOS — Step 13 offline launcher state

Experimental unofficial iOS launcher/compatibility-host foundation for users who legitimately own Slay the Spire 2 on Steam.

## Project state

**Steps 01–12 are complete and closed on a physical iPhone.** Step 12.4.1 / `0.0.39` is the current physically exercised content-management baseline, including the forced fresh-CDN regression.

This archive is **Step 13 / `0.0.40 (40)`**, the next single-capability candidate.

## Step 13 boundary

Step 13 adds one capability only: determine whether the previously created Step 12 managed install is **offline-ready** without consulting Steam or the saved Steam session.

The Step 13 inspector:

- reads only `Step12-ManagedInstall` from the project-owned Documents tree;
- accepts exactly one current-boundary `Depot-*` managed directory;
- reads the existing non-secret `.sts2launcher-install.json` with the Step 12.1 source-generated `System.Text.Json` context;
- validates App ID, depot identity, manifest ID, branch, safe unique paths, lengths and SHA-1 metadata;
- verifies the exact local file set;
- re-hashes every managed file and requires the recorded SHA-1/length to match;
- returns `OfflineReady`, `OnlineSetupRequired`, or `RepairRequired`;
- never receives a `SteamSessionStore`, `SteamClient`, HTTP client, WebSocket, PICS/CDN object, or other network dependency;
- explicitly reports that online manifest freshness is **unknown while offline**.

`OfflineReady` does **not** mean the game can execute on iOS yet. It also does not re-prove ownership or tell us whether Steam has published a newer manifest since the last online manager run. Those require the already-proven online path.

All Step 12 install/update/repair/cache controls remain available as regression tools.

## Build

Use Codemagic workflow:

```text
ios-step-13
```

Expected app version/header:

```text
0.0.40 (40)
STEP 13 — OFFLINE LAUNCHER STATE
```

Expected IPA artifact:

```text
artifacts/StS2-Launcher-Step-13.ipa
```

See `docs/STEP-13-TEST.md` for the physical-iPhone gate.

## Scope boundary

No game launch, multi-depot composition, compatibility inventory, Mono.Cecil work, Godot host/rendering, Steam Cloud, or Workshop support is added in Step 13.
