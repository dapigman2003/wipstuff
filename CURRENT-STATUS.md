# Current status

**Steps 01–11: complete on physical iPhone.**

**Current source boundary: Step 12.3 — independently verified Step 11 cache reuse + stronger deterministic update test.**

App version: `0.0.37 (37)`.
Codemagic workflow: `ios-step-12-3`.

Step 12 remains open pending its complete physical-iPhone gates.

## Why Step 12.3 exists

Step 12 (`0.0.33`) processed all `428` files / `2323747842` bytes but failed before commit when reflection-based receipt JSON hit `ConstructorContainsNullParameterNames` under full trimming. Step 12.1 (`0.0.34`) moved that contract to compile-time `System.Text.Json` metadata.

The next device run exposed an iOS `TimeoutException: The request timed out.` while the reused Step 11 downloader was materializing `Slay the Spire 2.pck`. Step 12.2 (`0.0.35`) added bounded per-CDN failover for that direct timeout shape without changing the proven Step 05 HTTP-handler policy. Its first Codemagic compile then failed with `CS0160` because two authenticated retry blocks caught `HttpRequestException` before derived `SteamKitWebRequestException`; Step 12.2.1 (`0.0.36`) corrected only that ordering.

While testing the deterministic update-state helper, another design weakness became visible: the helper intentionally made the Step 12 install receipt stale, and the manager then used that stale receipt as the only trust anchor for an already-complete Step 11 source cache. The cache was discarded and reacquired even though it could be independently proven against Steam. Cancelling during acquisition also returned misleading `Planned files/bytes: 0` telemetry.

## Step 12.3 change

Step 11 already downloads the real current Steam manifest before it notices that the manifest-specific final cache exists. Step 12.3 uses that fact: the existing final cache is now checked as an exact manifest tree and every regular file is re-hashed against the Steam manifest SHA-1. Only a cache that fails path/size/SHA-1 verification is deleted and reacquired.

The Step 12 install receipt is therefore no longer allowed to vouch for—or invalidate—the Step 11 source cache. Step 12 receives explicit `ExistingFinalVerifiedAgainstManifest` telemetry, forwards verification progress to the UI, preserves planned file/byte counts when source acquisition is cancelled/times out, and reports whether any new source bytes were downloaded during the manager run.

The deterministic update helper is also stronger. It changes only the local receipt: it stales the manifest ID and changes the SHA-1 identity of the smallest non-empty receipt file while leaving the real managed file untouched. The next run must classify `UpdateAvailable`, use the verified current source, replace at least one file rather than only rewriting a receipt, verify the complete staged tree, atomically commit, and finish `UpToDate` on Steam's actual current public manifest.

Step 12.1 AOT-safe receipt JSON and Step 12.2/12.2.1 CDN timeout failover/catch ordering remain regression-protected.

Later boundaries remain excluded: multi-depot composition, compatibility inventory, Godot/runtime execution, Cloud, and Workshop.
