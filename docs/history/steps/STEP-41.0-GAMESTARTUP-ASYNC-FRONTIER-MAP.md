# Step 41.0 — GameStartup async frontier map

Physical Step 40 is closed positive: a second 0.0.171 device run passed all four gates, completing the real render pulse at 103.3 ms and returning to frozen NGame/state/native confinement.

Step 41 is intentionally non-invoking. It preserves the exact selected Step-39.1 compatibility image, keeps `GameStartupWrapper` inert, and never restarts rendering. Gate A re-verifies same-process Step-40 4/4 frozen authority. Gate B maps exact `NGame.GameStartup`, requires its `AsyncStateMachineAttribute`, records the compiler-generated state-machine fields, and emits full GameStartup + `MoveNext` IL/token evidence. Gate C traverses the transitive same-sts2 `MoveNext` closure with deferred/rejecting Cecil and records path-qualified startup/platform/Steam/main-menu/deferred/FMOD/Spine/Sentry/native-facing references. Gate D proves the mapping caused no runtime drift or native escape.

The static map, not assumptions about async source order, is the authority for designing the next partial-startup boundary. No GameStartup execution, `InitializePlatform`, Steam initialization, main-menu launch, `ExecuteDeferred`, native game-extension loading, cleanup, or render restart is authorized by Step 41.
