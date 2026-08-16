# Third-Party Components — Step 06.3.1

## Runtime

### SteamKit2 3.4.0

Used for the proven Steam CM connection foundation and the Step 06/06.1 authentication session, including same-session mobile confirmation polling. The iOS build uses WebSocket-only transport and supplies a dedicated `SocketsHttpHandler` for `HttpClientPurpose.CMWebSocket`.

### protobuf-net / protobuf-net.Core

Transitive SteamKit dependencies. The iOS app roots these assemblies together with SteamKit2 because the Steam message serializer discovers generated protobuf metadata through reflection, and full trimming previously removed required accessors.

## Build-only

### Mono.Cecil 0.11.6

Used only by `tools/StS2Launcher.SteamKitIosPatcher` against an isolated local NuGet copy of SteamKit2. The patcher verifies SteamKit2 3.4.0 and replaces at most one unsupported `Process.StartTime` call with `DateTime.UtcNow` if that call is still present. It refuses ambiguous matches and is not packaged into the IPA.

### Microsoft.NET.Test.Sdk / MSTest

Used only by `tests/StS2Launcher.Core.Tests` for host unit tests. Test framework/adapter assemblies are build-time dependencies and are explicitly excluded from the IPA verification policy.

## Repository policy

This repository contains no Slay the Spire 2 game files, Steam credentials, Apple signing secrets, proprietary FMOD/Spine assets, Godot runtime, or later-stage game compatibility components.
