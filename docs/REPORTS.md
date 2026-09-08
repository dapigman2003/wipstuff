# Reports

## Step 39.0 current evidence contract

Physical 0.0.165 is the authority closing Step 38.2 at 4/4. Step 39 uses that as prior evidence but does **not** rerun Step 38 in the same process. The required fresh chain is Step 15 → Step 35 → Step 36 → Step 37 → Step 39.

Preserve `Step39-CrashCheckpoint-<RunId>.txt`, `Step39-SceneTreeAdmission-StaticMap-<RunId>.txt`, `Step39-LastCheckpoint.txt`, and `Step39-TransformedRealStS2SceneTreeAdmission.txt`, along with prerequisite Step35/Step36/Step37 reports.

The Step39 static map is produced after Gate B and records the exact selected compatibility hash, live SceneTree-root type, actual off-tree node graph, actual selected-sts2 managed node types, immediate `_EnterTree/_Ready/_Notification` callbacks including in-module base chains, and direct call references. It also records the exact four-resource preflight hashes and audited GDScript header facts. Any direct startup/native/platform edge is a Gate-B failure.

Gate C telemetry records AddChild start/return or nested exception plus resolver/host/private/initializer/rejected/native deltas. Immediately after Gate C returns the UI records `StopRendering()` and the observed render-active state. Successful Gate C requires in-tree `NGame`, exact singleton/parent, non-null `_window`, state 2, and zero initializer/rejected/native escape. Successful Gate D additionally requires rendering frozen and retains the node in-tree without `_ExitTree`/RemoveChild/Free.

## Closed Step 38.2 evidence

Preserve the physical 0.0.165 Step38 checkpoint, static map, last checkpoint, and final report under `docs/history/reports/STEP-38.2-PHYSICAL-COMPLETE-0.0.165-*`. They prove the compatibility `_EnterTree` returned once off-tree at state 2 with zero resolver/initializer/rejected/native deltas and normal teardown.
