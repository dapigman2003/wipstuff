# Release Checklist — Canonical Foundation

## Source structure

- Live iOS project is exactly `src/StS2Launcher.iOS/StS2Launcher.iOS.csproj`.
- No live `src/StS2Launcher.Step05.iOS` directory exists.
- Active scripts do not reference `history.zip` or legacy Step05 project paths.
- Historical step docs remain readable under `docs/history/steps/`.
- `history.zip`, if present, is reference-only and not required by validation/build.

## Runtime policy

- `TrimMode=full`.
- `MtouchInterpreter=-all`.
- `UseInterpreter=true` rejected.
- NativeAOT/`PublishAot=true` rejected.
- SteamKit/protobuf trimmer roots retained.
- exact 22 measured Step 22 direct framework roots retained.
- DiskArbitration removal retained.
- Godot 4.5.1 native bridge/link policy retained.

## Security/content

- no StS2 game payload in source/IPA;
- no Steam credentials/tokens/Guard secrets in source;
- no Apple signing credentials/private keys in source;
- no proprietary FMOD/Spine payloads;
- dynamic test fixtures remain project-owned post-publish test data, not app/AOT project references.

## Build acceptance

- static validation PASS;
- host unit tests PASS;
- Godot build/native preflight PASS;
- iOS publish PASS;
- runtime-policy telemetry reports `MtouchInterpreter=-all` and no broad interpreter/NativeAOT;
- final IPA verification PASS;
- expected version is 0.0.62 (62).

## Physical acceptance

- header `STEP 22.4 — CANONICAL FOUNDATION`;
- Step 22 A–D 4/4;
- blockers 0;
- runtime closure ready YES;
- OfflineReady PASS;
- Foundation 5/5 PASS;
- text reports present in Files.
