# Current status

**Steps 01–11: complete on physical iPhone.**

**Current source boundary: Step 12.2 — iOS CDN timeout/failover compatibility hotfix for the existing Step 12 install/update/repair manager.**

App version: `0.0.35 (35)`.
Codemagic workflow: `ios-step-12-2`.

The Step 12 (`0.0.33`) physical-iPhone install attempt successfully processed all `428` planned files / `2323747842` bytes, then failed before atomic commit because reflection-based `System.Text.Json` receipt serialization could not inspect the trimmed positional record constructor (`ConstructorContainsNullParameterNames`).

Step 12.1 (`0.0.34`) replaced that receipt path with compile-time `System.Text.Json` source-generated metadata. The next physical-iPhone manager run progressed past that prior failure and into Step 11 source acquisition, but failed while materializing:

```text
SlayTheSpire2.app/Contents/Resources/Slay the Spire 2.pck
```

with:

```text
TimeoutException: The request timed out.
```

The failure occurred inside the existing resumable CDN path. The helper already treated `TaskCanceledException`, `HttpRequestException`, `IOException`, and `SteamKitWebRequestException` as bounded per-server failures, but did not recognize the direct `TimeoutException` shape produced by the iOS platform HTTP stack during a bounded SteamKit request/body timeout. That exception escaped the per-server failover helper and aborted the whole source-acquisition attempt.

Step 12.2 keeps the Step 05 HTTP-handler policy unchanged: `SocketsHttpHandler` remains scoped only to `CMWebSocket`; CDN/WebAPI continue using the platform-default client. The narrow change is in the Step 11 resumable downloader: direct `TimeoutException` is now treated as a recoverable endpoint timeout and fails over to another bounded CDN server for both manifest and chunk downloads, including the retry-after-CDN-auth path. Existing resume data remains preserved.

Step 12.1's source-generated receipt fix remains in place. The Step 12 install/update/repair semantics and physical-device Gates A–D are otherwise unchanged. Step 12 is still open until those gates pass.

Later boundaries remain excluded: multi-depot composition, compatibility inventory, Godot/runtime execution, Cloud, Workshop.
