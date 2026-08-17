# Third-Party Components — Step 15

## Runtime

### SteamKit2 3.4.0

Used for the proven Steam CM/authentication/content foundation. The iOS build keeps WebSocket-only CM transport and the dedicated `SocketsHttpHandler` policy for `HttpClientPurpose.CMWebSocket`.

### protobuf-net / protobuf-net.Core

Transitive SteamKit dependencies. The iOS app roots these assemblies together with SteamKit2 because the Steam message serializer uses reflection and full trimming previously removed required accessors.

### Godot Engine 4.5.1-stable

Step 15 source-builds the pinned upstream Godot Engine `4.5.1-stable` tag on the Codemagic macOS runner and statically links the resulting arm64 iOS archive. Godot Engine is MIT-licensed. The repository does not vendor the upstream Godot source or a prebuilt Godot binary; the generated archive is a build artifact.

The Step 15 build makes one host-integration patch to the disposable upstream build tree: it renames the standalone iOS `main()` function to avoid colliding with the existing .NET/UIKit launcher entry point. `apple_embedded_main` is retained. A repository-owned custom Godot module supplies the narrow Step 15 C bridge.

## Build-only

### Mono.Cecil 0.11.6

Used only by `tools/StS2Launcher.SteamKitIosPatcher` against an isolated local NuGet copy of SteamKit2. The patcher verifies SteamKit2 3.4.0 and replaces at most one unsupported `Process.StartTime` call with `DateTime.UtcNow` if that call is still present. It refuses ambiguous matches and is not packaged into the IPA.

### Microsoft.NET.Test.Sdk / MSTest

Used only by `tests/StS2Launcher.Core.Tests` for host unit tests. Test framework/adapter assemblies are build-time dependencies and are explicitly excluded from the IPA verification policy.

### SCons 4.8.1

Installed into a build-cache-local Python virtual environment by Codemagic and used only to compile the pinned Godot iOS static archive. It is not packaged into the IPA.

## Repository policy

This repository contains no Slay the Spire 2 game files, Steam credentials, Apple signing secrets, or proprietary FMOD/Spine assets. Step 15 adds only project-owned smoke-project files and bridge source; it does not add later-stage StS2 compatibility rewrites or game runtime payloads.
