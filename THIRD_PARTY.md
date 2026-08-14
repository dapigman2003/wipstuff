# Third-Party Components — Step 05.14

## SteamKit2

- Project: SteamRE/SteamKit
- Component: SteamKit2
- Version pinned in this step: 3.4.0
- Source: https://github.com/SteamRE/SteamKit
- Purpose: Steam network protocol client
- License: see the upstream SteamKit repository/license for authoritative terms

Step 05.14 retains SteamKit2 3.4.0 after the Step 05.12 physical-device
comparison showed that the upgrade alone did not remove the iOS AOT failure.
This step adds only lifecycle-stage diagnostics around the existing unauthenticated
SteamKit connection probe. The build-only iOS compatibility patcher accepts the
3.4.0 assembly and
handles the constructor timestamp boundary conservatively: if exactly one
`System.Diagnostics.Process.StartTime` call remains in `SteamClient`, it is
replaced with `DateTime.UtcNow`; if the call is already absent, the assembly is
left untouched; more than one match is a hard failure. The application still
uses SteamKit only for an unauthenticated connection/disconnection probe.

## Mono.Cecil — build tool only

- Project: Mono.Cecil
- Version pinned in the build-only patcher: 0.11.6
- Source: https://github.com/jbevain/cecil
- Purpose: deterministic pre-AOT edit of the local SteamKit2 build copy when required
- Packaging: not referenced by Core/iOS and must not be present in the IPA
- License: see the upstream Mono.Cecil repository/license for authoritative terms

This does not introduce the future StS2 runtime assembly-patching subsystem.
Mono.Cecil is used only by `tools/StS2Launcher.SteamKitIosPatcher` during CI.

## Existing platform/runtime components

The app also relies on the .NET iOS runtime/SDK and Apple system frameworks
supplied by the build/runtime environment.
