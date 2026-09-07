# Step 38.0.1 — deferred Cecil metadata-read correction

Physical app **0.0.162 (162)** reached Step 38 only after the same-process Step-37.0.1 4/4 authority was present, then failed safely in Gate A before any lifecycle invocation. The failure was `Mono.Cecil.AssemblyResolutionException` for `GodotSharp, Version=4.5.1.0` while opening the exact selected compatibility image for the NGame lifecycle map.

The defect was in the Step-38 audit reader, not in the physically proven runtime authority: Gate A used Cecil `ReadingMode.Immediate` with the default resolver. Earlier physically closed metadata audits, including Step 36, use deferred reads with a rejecting resolver so inspection does not require external assembly resolution.

0.0.163 changes only the Step-38 Gate-A reader:

- `ReadingMode.Deferred`;
- explicit `RejectingAssemblyResolver`;
- explicit `MetadataResolver` bound to that rejecting resolver;
- `ReadSymbols=false`, `InMemory=true` retained;
- Gate A additionally requires **zero external assembly-resolution requests** before accepting the lifecycle map.

The Step-38 execution policy is unchanged. `_EnterTree` is still invoked at most once and only after the static call-closure audit passes. SceneTree insertion, `_Ready`, `_ExitTree`, `GameStartup`, `InitializePlatform`, `LaunchMainMenu`, `ExecuteDeferred`, Steam initialization, native GDExtensions, and gameplay remain forbidden.

This correction does not add GodotSharp or any other game/native payload to the source archive and does not relax the managed runtime resolver.
