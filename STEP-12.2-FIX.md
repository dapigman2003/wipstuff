# Step 12.2 — iOS CDN `TimeoutException` failover hotfix

## Device evidence

The Step 12.1 physical-iPhone run reported:

```text
INSTALL MANAGER FAIL — verified source unavailable
State before: NotInstalled
Action taken: Install
Atomic commit completed: NO
Error: Could not materialize resumable staged file
'SlayTheSpire2.app/Contents/Resources/Slay the Spire 2.pck':
TimeoutException: The request timed out.
```

This is a different boundary from the Step 12.0 receipt-serialization failure. The manager had not reached receipt creation or atomic commit. It was reacquiring/materializing the manifest-specific Step 11 source tree.

## Localization

SteamKit 3.4.0's CDN client uses bounded cancellation tokens for request headers and response-body reads. The project intentionally leaves `HttpClientPurpose.CDN` on the platform-default iOS handler; only `CMWebSocket` uses the proven dedicated `SocketsHttpHandler` compatibility path.

The Step 11 resumable downloader already performs bounded CDN-server failover for:

- `SteamKitWebRequestException`;
- `TaskCanceledException` when the overall operation itself was not cancelled;
- `HttpRequestException`;
- `IOException`.

On iOS, the platform `NSUrlSessionHandler` can surface a cancelled/timeout response-stream read as a direct `TimeoutException` with the message `The request timed out.`. Step 11 did not catch that shape, so it escaped `DownloadChunkFromAnyServerAsync` and was converted into a whole-file materialization failure.

The same gap also existed in manifest downloading and in the authenticated retry after a CDN endpoint first returned HTTP 403 and a CDN auth token was obtained.

## Narrow fix

Step 12.2 adds:

```csharp
catch (TimeoutException) when (!cancellationToken.IsCancellationRequested)
{
    // Try another bounded CDN server.
}
```

to both Step 11 manifest and chunk failover paths, for both the initial endpoint request and the authenticated retry.

No HTTP handler is changed. No SteamKit version is changed. No timeout constants are changed. No source verification, SHA-1 rules, resume validation, receipt schema, staging logic, or atomic replacement logic is weakened.

## Expected retry behavior

The failed Step 12.1 run used the resumable Step 11 source path. Any checksum-valid partial data that Step 11 preserved remains eligible for the existing resume rules. A Step 12.2 retry should therefore reuse valid resume data and fetch only missing/invalid chunks before Step 12 verifies the complete source tree.

## Version / gate

Build workflow `ios-step-12-2`, install `0.0.35 (35)`, then rerun `docs/STEP-12-TEST.md`. Step 12 is not closed until all original physical-device gates pass.
