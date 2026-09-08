# Reports

## Step 38.2 current evidence contract

Physical 0.0.161 is the authority closing Step 37.0.1. Physical 0.0.163 is the authority proving `_EnterTree -> GameStartupWrapper`. Physical 0.0.164 is the authority proving the inert-wrapper derivative passes Gate A/B and that direct off-tree `_EnterTree` physically fails with a managed NullReferenceException at 2/4 while `IsInsideTree=false` and initializer/rejected/native deltas remain zero.

Step 38.2 requires the same-process Step-37 closure before Gate A. Preserve `Step38-CrashCheckpoint-<RunId>.txt`, `Step38-NGameLifecycle-StaticMap-<RunId>.txt`, `Step38-LastCheckpoint.txt`, and `Step38-TransformedRealStS2NGameLifecycleEntry.txt`, along with prerequisite Step35/Step36/Step37 reports.

The Step38 static map must record exact selected-authority lifecycle tokens/IL/callsites, verify the selected `GameStartupWrapper` is exactly `call Task.get_CompletedTask; ret`, verify zero remaining `_EnterTree` SentryService.Initialize/GetWindow/FilesDropped/Connect references, and record the same-NGame call closure. Gate C failure telemetry includes nested exception diagnostics plus resolver/host/private load names. Successful Gate C/D requires `IsInsideTree=false`, OneTimeInitialization state `2`, and zero initializer-bearing, rejected-managed, or native-load activity.

## Step 38.1 current evidence contract

Physical 0.0.161 is the authority closing Step 37.0.1: sealed `game.tscn` passed, the FMOD-neutral derivative loaded as PackedScene, the real NGame hierarchy instantiated off-tree, resolver/host/private/initializer/rejected/native deltas remained zero, and the run reached normal `RUN_END`. Physical 0.0.163 is the authority proving the next exact lifecycle seam: deferred Cecil mapping found `_EnterTree -> GameStartupWrapper` and stopped before any lifecycle invocation.

Step 38.1 requires that same-process Step-37 closure before Gate A. Preserve `Step38-CrashCheckpoint-<RunId>.txt`, `Step38-NGameLifecycle-StaticMap-<RunId>.txt`, `Step38-LastCheckpoint.txt`, and `Step38-TransformedRealStS2NGameLifecycleEntry.txt`, along with prerequisite Step35/Step36/Step37 reports.

The Step38 static map records exact selected-authority lifecycle tokens/IL/callsites, verifies the selected `GameStartupWrapper` is exactly `call Task.get_CompletedTask; ret`, and records the same-NGame call closure reachable from unchanged `_EnterTree`. Gate C telemetry must capture full nested exception/context state if direct `_EnterTree` fails. Successful Gate C/D requires `IsInsideTree=false`, OneTimeInitialization state `2`, and zero initializer-bearing, rejected-managed, or native-load activity.
