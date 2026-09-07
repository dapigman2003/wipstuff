# Reports

## Step 37.0 current evidence contract

Physical 0.0.159 is the authority closing Step 36.0.5: full unchanged `ExecuteEssential()` returned, state advanced 1->2, all four Step-36 gates passed, OfflineReady 428/428 reproof passed, and initializer-bearing/rejected/native deltas stayed zero.

Step 37.0 requires that same-process closure before Gate A. Preserve `Step37-CrashCheckpoint-<RunId>.txt`, `Step37-GameScene-StaticMap-<RunId>.txt`, `Step37-LastCheckpoint.txt`, and `Step37-TransformedRealStS2GameSceneAdmission.txt`, along with the prerequisite Step35/Step36 reports.

The Step37 static map records the sealed receipt-backed game.tscn authority and the three permitted FMOD-neutral compatibility edits. Gate C/D telemetry must record resolver/host/private-load deltas and fail on initializer-bearing, rejected-managed, or native-load escape.
