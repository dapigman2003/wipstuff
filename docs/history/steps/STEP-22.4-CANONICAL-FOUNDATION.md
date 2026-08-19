# Step 22.4 — Canonical Foundation

## Objective

Create the clean long-term source/documentation/build foundation before the first controlled real StS2 CLR load, without introducing new game compatibility behavior.

## Structural changes

- live project renamed from `StS2Launcher.Step05.iOS` to `StS2Launcher.iOS`;
- live namespace renamed from `StS2Launcher.Step05.iOS` to `StS2Launcher.iOS`;
- active Godot wrappers renamed to `build-godot.sh` and `preflight-godot-link.sh`;
- historical step docs moved to `docs/history/steps/`;
- `docs/MASTER-PLAN.md` added as the durable project/resumption plan;
- `history.zip` contains optional legacy scripts and a pre-rename Step05 iOS project snapshot;
- current validation/build is forbidden from depending on `history.zip`.

## Behavior boundary

No real StS2 CLR load, game initialization, new Cecil rewrite, new runtime fallback, new native framework, or new Steam capability is introduced.

## Acceptance

Codemagic must compile/test/build/verify successfully. Physical acceptance remains Step 22 A–D 4/4 + blockers 0 + runtime closure YES, followed by OfflineReady and Foundation 5/5.
