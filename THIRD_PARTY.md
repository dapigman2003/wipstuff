# Third-Party Components — Step 17

## Runtime

### SteamKit2 3.4.0

Used for the proven Steam CM/authentication/content foundation. The iOS build keeps WebSocket-only CM transport and the dedicated `SocketsHttpHandler` policy for `HttpClientPurpose.CMWebSocket`.

### protobuf-net / protobuf-net.Core

Transitive SteamKit dependencies. The iOS app roots these assemblies together with SteamKit2 because the Steam message serializer uses reflection and full trimming previously removed required accessors.

### Godot Engine 4.5.1-stable

The pinned Godot Engine `4.5.1-stable` source is built on the Codemagic macOS runner and statically linked as the physically proven Step 15 arm64 iOS host. Godot Engine is MIT-licensed. The repository does not vendor the upstream Godot source or a prebuilt engine binary; the generated archive is a build artifact/cache artifact.

### Mono.Cecil 0.11.6

Steps 16–17 intentionally use Mono.Cecil at runtime for managed metadata/IL **file** inspection. Step 16 also proves controlled rewriting only on a project-owned fixture; Step 17 uses Cecil read-only to scan concrete call sites in the receipt-backed macOS arm64 managed payload. The same pinned version remains used by the build-only SteamKit compatibility patcher. No real StS2 assembly is rewritten yet.

## Build-only

### StS2Launcher.SteamKitIosPatcher / Mono.Cecil 0.11.6

The build-only patcher operates on an isolated local NuGet copy of SteamKit2. It verifies SteamKit2 3.4.0 and replaces at most one unsupported `Process.StartTime` call with `DateTime.UtcNow` if that call is still present. It refuses ambiguous matches and the patcher itself is not packaged into the IPA.

### Microsoft.NET.Test.Sdk / MSTest

Used only by `tests/StS2Launcher.Core.Tests` for host unit tests. Test framework/adapter assemblies are build-time dependencies and are excluded from the IPA.

### SCons 4.8.1

Installed into a temporary Python virtual environment by Codemagic and used only to compile the pinned Godot iOS static archive. It is not packaged into the IPA.

## Repository policy

This repository contains no Slay the Spire 2 game files, Steam credentials, Apple signing secrets, or proprietary FMOD/Spine assets. The Step 16 regression fixture is a tiny project-owned assembly built from source during CI and bundled only as inert test data. Step 17 adds no third-party or game payload.
