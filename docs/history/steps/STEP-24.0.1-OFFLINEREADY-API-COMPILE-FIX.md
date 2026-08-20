# Step 24.0.1 — OfflineReady API Compile Fix

## Trigger

The Step 24.0 / `0.0.73 (73)` Codemagic run passed canonical static validation **281/281** and began Core compilation, but failed before host tests with CS1061 at two Step 24 OfflineReady checks. `ControlledManagedInitialization` called `SteamOfflineInstallInspection.InspectAsync` and read an `OfflineReady` member, but the established Step 13 contract exposes `RunAsync` and reports readiness through `SteamOfflineInstallResult.Success` plus `ExactManagedTreeVerified`.

No IPA was produced and no Step 24 physical-device evidence exists for build 73.

## Correction

At both the pre-initialization and post-initialization OfflineReady checks, use the same established contract already used by the physically proven Step 23 loader:

- call `SteamOfflineInstallInspection.RunAsync`;
- require `Success`;
- require `ExactManagedTreeVerified`;
- retain the existing depot/manifest and managed-path identity checks appropriate to each gate.

The active scripts' report headings are also corrected from stale Step 23 wording to Step 24 wording so CI artifacts identify the candidate accurately.

## Protected behavior

No Step 23 production code changes. No Step 24 gate ordering, initializer target, Cecil audit, private resolver, module-constructor barrier, native refusal, game/Harmony invocation policy, trusted-install policy, or prepared-runtime policy changes.

## Candidate

- Step: **24.0.1**
- version: **0.0.74 (74)**
- workflow: **`ios-step-24`**
- expected IPA: **`artifacts/StS2-Launcher-Step-24.ipa`**

## Authority

Codemagic remains compile/host-test/build authority. Physical iPhone remains runtime authority.
