# StS2 Launcher iOS — Step 16 Managed Preparation Foundation

Experimental unofficial iOS launcher/compatibility-host project for users who legitimately own Slay the Spire 2 on Steam.

## Project state

**Steps 01–15 are complete and closed on a physical iPhone.** Step 15 physically proved the independent source-built Godot 4.5.1-stable iOS host: native bridge availability, embedded initialization/render-loop control, Metal smoke-scene rendering, physical touch, and UIKit lifecycle forwarding all passed. A small initial-orientation/panel-layout quirk is recorded as non-blocking and is not changed in this release.

This archive is **Step 16 / runtime `0.0.44 (44)`**, using the accelerated model of one tightly related subsystem with several ordered gates.

## Step 16 subsystem — Managed Preparation Foundation

Step 16 adds Mono.Cecil `0.11.6` to the launcher runtime and proves the file-based managed-assembly preparation mechanism before touching any real game assembly.

Ordered gates:

- **Gate A — Fixture read:** open a project-owned fixture DLL with Cecil as raw metadata/IL; do not load or execute it.
- **Gate B — Fixture write/reopen:** write only a launcher-private fixture copy and reopen it successfully.
- **Gate C — Controlled IL rewrite:** change only the fixture `RewriteMe()` body from constant `7` to `42`, write, reopen, and verify the exact transformation.
- **Gate D — Real StS2 metadata inspection:** re-prove the Step 13 `OfflineReady` managed tree, then parse the receipt-backed installed managed assemblies one at a time with Cecil, including real `sts2.dll`, without writing/loading/executing them.

The only Step 16 writes are under launcher-private `Step16-ManagedPreparation` scratch storage. The real Step 12 managed installation remains read-only.

## Build

Use Codemagic workflow:

```text
ios-step-16
```

Expected app:

```text
0.0.44 (44)
STEP 16 — MANAGED PREPARATION FOUNDATION
```

Expected IPA:

```text
artifacts/StS2-Launcher-Step-16.ipa
```

The cached/pinned Godot 4.5.1 Step 15 static host is still built/restored and linked so that proven native foundation remains regression-protected.

See `docs/STEP-16-TEST.md` for physical-iPhone testing.

## Scope boundary

Step 16 does **not** rewrite real StS2 assemblies, execute `sts2.dll`, integrate FMOD/Spine, launch the game, add Cloud, or add Workshop support.
