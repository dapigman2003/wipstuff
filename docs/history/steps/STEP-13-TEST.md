# Step 13 — physical-iPhone offline launcher-state gate

## Boundary

Prove that a legitimately created Step 12 managed install can be recognized as a trustworthy **local offline state** without using the saved Steam session or making a Steam/network request.

Step 13 does not launch Slay the Spire 2 and does not claim that the locally recorded manifest is still Steam's newest manifest.

## Prerequisite

Start from the already-proven Step 12.4.1 managed install in `UpToDate` state. Do not delete the managed install.

## Gate A — build/install regression

1. Build Codemagic workflow `ios-step-13`.
2. Install the IPA on the physical iPhone.
3. Confirm the header is `STEP 13 — OFFLINE LAUNCHER STATE` and version is `0.0.40`.
4. With networking available, the existing Step 12 manager should still be able to report the stable install as `UpToDate` if you choose to run it.

## Gate B — real offline-ready proof

1. Enable **Airplane Mode** and make sure Wi-Fi is also off.
2. Force-quit the launcher from the app switcher.
3. Relaunch it while still offline.
4. The retained Step 06.3.1 automatic saved-session regression may time out/fail transiently because networking is unavailable. It must not delete the saved session merely because the network is offline. Wait for that automatic operation to finish so the shared test controls are enabled.
5. Tap **Verify Offline-Ready Install (Local Only)**.

Required result:

```text
OFFLINE READY PASS
State: OfflineReady
Receipt structurally valid: YES
Exact managed tree verified: YES
Steam session consulted: NO
Network access attempted by Step 13 check: NO
Online manifest freshness known: NO
```

The verified file count must equal the planned file count, and verified bytes must equal planned bytes.

## Gate C — local corruption must not be called offline-ready

This proves the negative classifier without deleting the full install.

1. While still offline, use the existing **Prepare Repair Test (Corrupt One Managed File)** control.
2. Run **Verify Offline-Ready Install (Local Only)** again.
3. It must return `RepairRequired` / local-file verification failure, **not** `OfflineReady`.
4. Re-enable networking.
5. Run **Inspect + Install / Update / Repair** and restore the managed install to `UpToDate` / `REPAIR PASS`.
6. Optionally run the Step 13 local check once more; it should return `OfflineReady` again.

## Gate D — regressions

After networking is restored, run **Foundation 5/5 Regression** and require the existing foundation pass.

The completed Step 12 install/update/repair behavior remains regression-protected; Step 13 does not change its download/commit algorithms.

## Completion rule

Step 13 is complete only after the physical device proves both:

- a valid managed install becomes `OfflineReady` with networking unavailable; and
- a deliberately corrupted managed file becomes `RepairRequired` locally and is recoverable through the existing Step 12 online repair path.

Do not move into compatibility inventory / Step 14 until this gate passes.
