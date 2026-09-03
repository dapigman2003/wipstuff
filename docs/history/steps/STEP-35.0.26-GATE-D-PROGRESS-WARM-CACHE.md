# Step 35.0.26 — Gate D live integrity progress + warm Codemagic toolchain cache

Release: 0.0.149 (149)

## Motivation

0.0.148 has reached Gate D on-device, but the final receipt-backed OfflineReady reproof can appear stationary for a long interval because Step 35 did not forward the nested `SteamOfflineInstallProgress` stream. A single large receipt file (notably the game PCK) could therefore spend a long time inside one SHA-1 operation without changing the file counter.

Codemagic telemetry also showed that the existing NuGet, source-built Godot and iOS arm64 `obj` caches were restored, while the pinned .NET SDK/iOS workload lived outside the cache and was reinstalled by the canonical script.

## Runtime/UI change

The proven 0.0.146 managed-plugin bridge and 0.0.147 post-bootstrap resolver/Gate-C contract are unchanged. Gate D now forwards the protected nested OfflineReady file/byte checkpoints into the Step-35 progress surface without changing the physically protected Step-13 verifier. Because that verifier reports after each file rather than during a single SHA-1 operation, the UI adds a one-second liveness heartbeat showing elapsed time and time since the last verifier checkpoint while a large file is still hashing.

The iOS Step-35 surface adds a dedicated Gate-D `UIProgressView` and progress label showing receipt-hash percentage, file counts, byte totals, latest verifier file and observed throughput, plus the heartbeat timing. The indicator reserves its final quarter for the post-hash source/diagnostic/plan/dependency/context checks so progress remains monotonic after the receipt hash completes.

## Codemagic cache change

The stable `ios-canonical` workflow retains the existing NuGet, Godot Step-15 and iOS arm64 `obj` caches and additionally caches `$HOME/.dotnet`. The canonical script still verifies exact SDK `9.0.314`. It skips `dotnet workload install ios --version 9.0.314.3` only when a cache marker exactly matches the workload set and `dotnet workload list` confirms that `ios` is registered; otherwise it reinstalls and refreshes the marker.

This is build-time acceleration only and does not alter runtime/game inputs.

## Closure

A 0.0.149 diagnostic 4/4 remains diagnostic-derivative evidence only. Exact Step 35 remains OPEN.
