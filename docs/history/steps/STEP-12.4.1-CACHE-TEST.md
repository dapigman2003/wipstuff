# Step 12.4.1 — download-cache maintenance / forced fresh-download regression

Step 12 is already complete. Step 12.4.1 adds only test/maintenance controls so the fresh CDN acquisition path can be deliberately exercised again without deleting the stable Step 12 managed install or the saved Steam session.

## Safety boundary

The cache-clear helper is restricted to:

```text
StS2Launcher/Step11-ResumableDepot
```

It does **not** intentionally modify:

- `Step12-ManagedInstall` game files;
- `.sts2launcher-install.json` for a clear-only operation;
- the iOS Keychain / saved Steam refresh token;
- ownership/authentication state.

## Clear-only control

Tap **Clear Download Cache Only (Keep Managed Install)**.

Expected result:

```text
DOWNLOAD CACHE: CLEARED
Cache absent now: YES
Managed Step 12 install: PRESERVED
Saved Steam session: PRESERVED
```

A subsequent normal manager run may still be a no-op when the managed installation is already `UpToDate`; clearing the source cache alone does not create a need to update.

## Forced fresh-download regression

1. Start from a valid `UpToDate` managed install.
2. Tap **Prepare Fresh Download Test (Force Update + Clear Cache)**.
3. Require `Download cache absent now: YES`.
4. Tap **Inspect + Install / Update / Repair** once.
5. Let the operation finish. Do not press the preparation button again during the run.

Expected manager behavior:

```text
State before: UpdateAvailable
Action taken: Update
Source cache reverified against current Steam manifest: NO
Source bytes downloaded this manager run: > 0
Replaced files: >= 1
Atomic commit completed: YES
State after: UpToDate
```

Because both the completed and `.resume` Step 11 cache trees were cleared, this run should perform a genuine current-depot CDN acquisition rather than reusing a prior Step 11 source. Exact downloaded bytes may differ if Steam publishes a new manifest or transfer accounting changes, so the important proof is non-zero fresh source acquisition plus a successful fully verified Update/commit.

If the operation is deliberately cancelled after transfer progress becomes non-zero, the Step 11 resumable path may preserve new `.resume` state. Tapping the manager again should then resume/revalidate that newly created state according to the already-proven Step 11 rules.

## Scope

This test control does not begin Step 13 and does not add offline launcher-state behavior, multi-depot composition, compatibility inspection, Godot/runtime execution, Cloud, or Workshop.
