# Step 28.0.2 — Physical Closure

## Accepted build

- step: **28.0.2**
- version: **0.0.111 (111)**
- workflow: **`ios-step-28`**
- physical target: arm64 iPhone
- device report: `docs/history/reports/STEP-28.0.2-PHYSICAL-CLOSURE.txt`

## Physical result

The supplied device report closes Step 28 positively at **A–E / 5/5 PASS**.

The decisive runtime proof is Gate D:

- `Adjustment() == 1000`;
- `Target(41) == 1041` through reflection;
- `InvokeTarget(41) == 1041` through the fixture's own direct managed IL call;
- exactly one Step-28 fixture identity entered the CLR, and it was the verified transformed image;
- the original bundled/private-source bytes never entered the CLR.

Gate E then re-proved the complete isolation contract:

- bundle/source/transformed SHA-256 values remained stable;
- post-execution `OfflineReady` passed **428/428**;
- the trusted Step-12 managed install remained unchanged;
- no unexpected private managed dependency fallback or native activity occurred;
- no Harmony/MonoMod runtime patching occurred.

## Architecture consequence

Step 28 establishes the combined production mechanism that Step 27 could not provide through runtime detours:

```text
verified immutable receipt-backed source
  -> launcher-private copy
  -> deterministic Mono.Cecil semantic transformation before CLR admission
  -> reopen + metadata/hash verification
  -> CLR-load only the verified transformed image
  -> execute under the proven post-publish interpreter host
```

This is a physically closed mechanism boundary, not merely a host-test result. Runtime Harmony/MonoMod replacement remains closed negative and is not reopened.

## Boundary that remains intentionally closed

Step 28 did not transform, reflect over, or invoke any real StS2 member. It did not start the game/Godot path, load native game libraries, or establish mod compatibility.

The next candidate may therefore select one narrowly audited real StS2 compatibility point, but the exact receipt-backed ARM64 type/member/signature/IL must be re-established before a semantic rewrite is authored.
