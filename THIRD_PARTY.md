# Third-Party Components — Step 05.5

## SteamKit2

- Project: SteamRE/SteamKit
- Component: SteamKit2
- Version pinned in this step: 3.3.1
- Source: https://github.com/SteamRE/SteamKit
- Purpose: Steam network protocol client
- License: see the upstream SteamKit repository/license for authoritative terms

Step 05.5 applies one build-time iOS compatibility edit to the local restored
SteamKit2 assembly: the `SteamClient` constructor's unsupported iOS
`System.Diagnostics.Process.StartTime` read is replaced with `DateTime.UtcNow`.
The application still uses SteamKit only for an unauthenticated
connection/disconnection probe in this step.

## Mono.Cecil — build tool only

- Project: Mono.Cecil
- Version pinned in the build-only patcher: 0.11.6
- Source: https://github.com/jbevain/cecil
- Purpose: deterministic pre-AOT edit of the local SteamKit2 build copy
- Packaging: not referenced by Core/iOS and must not be present in the IPA
- License: see the upstream Mono.Cecil repository/license for authoritative terms

This does not introduce the future StS2 runtime assembly-patching subsystem.
Mono.Cecil is used only by `tools/StS2Launcher.SteamKitIosPatcher` during CI.

## Existing platform/runtime components

The app also relies on the .NET iOS runtime/SDK and Apple system frameworks
supplied by the build/runtime environment.
