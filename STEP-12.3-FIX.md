# Step 12.3 — verified cache reuse + stronger update capability gate

Physical update-state testing showed two related weaknesses in the Step 12 test path:

1. `Prepare Update-State Test` deliberately changed the Step 12 receipt manifest ID, and `AcquireVerifiedSourceAsync` then refused to trust the already-complete Step 11 cache because the stale Step 12 receipt no longer matched it. This caused a needless full source reacquisition.
2. Cancelling during that source phase could report `Planned files: 0 / Planned bytes: 0` even after Step 11 had already downloaded the current Steam manifest and knew the real plan.

Step 12.3 keeps the same install/update/repair boundary but changes the trust relationship. Step 11 now directly verifies an existing manifest-specific final cache against the freshly downloaded current Steam manifest: exact tree shape, file length, and Steam SHA-1. It returns `ExistingFinalVerifiedAgainstManifest=true` only after that proof. Step 12 trusts that proof regardless of whether its install receipt is stale. A cache that fails verification is still discarded and reacquired through the existing resumable downloader.

Step 12 also forwards Step 11 source progress, carries planned file/byte counts into cancelled/timed-out manager results, and reports both whether the cache was reverified and how many new source bytes were downloaded.

The deterministic update helper now changes only the project-owned receipt in two ways: it stales the manifest ID and gives the smallest non-empty file entry a synthetic different SHA-1. Actual game files are not changed. That forces the normal Update path to source at least one file from the independently verified current cache, while all compatible unchanged files can still be locally reused. A successful gate therefore proves more than a receipt rewrite: `UpdateAvailable -> Update`, source acquisition/trust, changed-file replacement, full staging verification, current receipt generation, atomic commit, and final `UpToDate` classification.

No multi-depot logic, compatibility inspection, Godot/runtime, Cloud, or Workshop is introduced.
