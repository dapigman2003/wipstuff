# Third-Party Components — Step 32.0.3

## Runtime

### SteamKit2 3.4.0

Used for the proven Steam CM/authentication/content foundation. The iOS build keeps WebSocket-only CM transport and the dedicated `SocketsHttpHandler` policy for `HttpClientPurpose.CMWebSocket`.

### protobuf-net / protobuf-net.Core

Transitive SteamKit dependencies. The iOS app roots these assemblies together with SteamKit2 because the Steam message serializer uses reflection and full trimming previously removed required accessors.

### Godot Engine 4.5.1-stable

The pinned Godot Engine `4.5.1-stable` source is built on the Codemagic macOS runner and statically linked as the physically proven Step 15 arm64 iOS host. Godot Engine is MIT-licensed. The repository does not vendor the upstream Godot source or a prebuilt engine binary; the generated archive is a build artifact/cache artifact.

### Mono.Cecil 0.11.6

Mono.Cecil is the pinned metadata/IL engine used by the compatibility pipeline. Earlier steps use it for controlled fixture inspection/transformation and read-only real-game audits; Step 32 uses it to rewrite only a launcher-private clone of the exact receipt-backed real `sts2.dll`, followed by independent reopen/hash/semantic verification. The trusted Step-12 installation is never rewritten. The same pinned version is also used by the build-only SteamKit compatibility patcher.

## Build-only

### StS2Launcher.SteamKitIosPatcher / Mono.Cecil 0.11.6

The build-only patcher operates on an isolated local NuGet copy of SteamKit2. It verifies SteamKit2 3.4.0 and replaces at most one unsupported `Process.StartTime` call with `DateTime.UtcNow` if that call is still present. It refuses ambiguous matches and the patcher itself is not packaged into the IPA.


### Harmony 2.4.2 historical regression material

The runtime-Harmony Step 25–27 experiment is closed negative and no Harmony release fixture is downloaded, linked, or bundled by the active 0.0.118 build/test pipeline. The complete pre-trim 0.0.117 source candidate, including the old hash-pinned Harmony-Fat host-regression acquisition logic and interpreted fixture, is preserved inertly in the historical archive. Harmony is MIT-licensed.

### Microsoft.NET.Test.Sdk / MSTest

Used only by `tests/StS2Launcher.Core.Tests` for host unit tests. Test framework/adapter assemblies are build-time dependencies and are excluded from the IPA.

### SCons 4.8.1

Installed into a temporary Python virtual environment by Codemagic and used only to compile the pinned Godot iOS static archive. It is not packaged into the IPA.

## Repository policy

This repository contains no Slay the Spire 2 game files, Steam credentials, Apple signing secrets, or proprietary FMOD/Spine assets. The project-owned Step 16/20/28 fixtures are built from source during CI and enter the IPA only as controlled regression/data payloads required by still-active runtime foundations. The IPA contains no real StS2 game payload; real game bytes remain user-owned in the receipt-backed Documents installation.
