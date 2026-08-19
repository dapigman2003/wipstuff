# Current Architecture — Step 22.3

## Safety boundary

The trusted Step 12 managed install is authoritative and remains immutable during compatibility preparation. Compatibility/runtime work uses launcher-private workspaces. The source/IPA never bundles Slay the Spire 2 assemblies or proprietary FMOD/Spine payloads.

Reusable Steam authentication material remains in iOS Keychain/runtime only. Shareable reports intentionally exclude passwords, refresh tokens, Steam Guard material, and Apple signing credentials.

## Physically proven layers

1. UIKit, lifecycle, Core/Keychain, Steam CM transport and full-trim/AOT foundation.
2. Steam authentication/session/ownership/PICS/download/resume.
3. Atomic managed install/update/repair and OfflineReady verification.
4. Compatibility inventory, Godot 4.5.1 native iOS host, Cecil metadata/write proof, read-only call-site analysis.
5. Expression fallback/framework boundary.
6. Post-publish dynamic managed IL execution through the Mono interpreter while build-time launcher assemblies remain AOT-targeted (`MtouchInterpreter=-all`).
7. Real `sts2.dll` runtime/framework dependency planning.
8. Host framework binding frontier with zero blockers and runtime closure ready for first controlled real CLR load.

Step 22.3 changes no item above. `tools/validation/protected-step22.2-core.sha256` protects all 97 pre-existing Step 22.2 Core source files byte-for-byte.

## Current source organization

`StS2Launcher.Core` uses one namespace but subsystem folders for navigation. The SDK recursively compiles them, so the restructuring does not change type identities.

The iOS `RootViewController` remains one sealed type but is split into partial files for UI construction, Steam session/content/install operations, local verification, Godot, compatibility gates, runtime gates, foundation regression, and report writing.

## Runtime policy

- `net9.0-ios`, `ios-arm64`
- full trimming
- build-time launcher assemblies remain AOT-targeted
- Mono interpreter retained with `MtouchInterpreter=-all`
- NativeAOT rejected
- SteamKit/protobuf reflection roots preserved
- the physically proven 22-name Step 22 host binding root frontier preserved exactly
- DiskArbitration remains removed only from the generated iOS linker framework set
- pinned source-built Godot 4.5.1-static host retained

## Build/test architecture

Current entry points are deliberately small:

- `scripts/validate.sh`
- `scripts/test.sh`
- `scripts/build-ios.sh`
- `scripts/verify-ipa.sh`
- `scripts/codemagic.sh`
- `scripts/build-godot-step15.sh`
- `scripts/preflight-godot-link-step15.sh`

All earlier step-specific scripts remain under `history/scripts/steps` as non-authoritative historical evidence.
