# Reports

## Step 38.0 current evidence contract

Physical 0.0.161 is the authority closing Step 37.0.1: sealed `game.tscn` passed, the FMOD-neutral derivative loaded as PackedScene, the real NGame hierarchy instantiated off-tree, resolver/host/private/initializer/rejected/native deltas remained zero, and the run reached normal `RUN_END`.

Step 38.0 requires that same-process Step-37 closure before Gate A. Preserve `Step38-CrashCheckpoint-<RunId>.txt`, `Step38-NGameLifecycle-StaticMap-<RunId>.txt`, `Step38-LastCheckpoint.txt`, and `Step38-TransformedRealStS2NGameLifecycleEntry.txt`, along with prerequisite Step35/Step36/Step37 reports.

The Step38 static map records exact selected-authority lifecycle tokens/IL/callsites and the same-NGame call closure reachable from `_EnterTree`. Gate C telemetry must capture full nested exception/context state if direct `_EnterTree` fails. Successful Gate C/D requires `IsInsideTree=false`, OneTimeInitialization state `2`, and zero initializer-bearing, rejected-managed, or native-load activity.
