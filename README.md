# StS2 Launcher iOS — Step 18 Real Assembly Rewrite Workspace

Experimental unofficial iOS launcher/compatibility-host project for users who legitimately own Slay the Spire 2 on Steam.

## Project state

**Steps 01–17 are complete and closed on a physical iPhone.** Step 15 physically proved the independent Godot 4.5.1 Metal/touch/lifecycle host. Step 16.1 physically proved Mono.Cecil read/write/reopen and real StS2 read-only metadata inspection. Step 17 physically passed the receipt-backed ARM64 actual-IL/native/dependency analysis subsystem.

This archive is **Step 18.1.1 source hotfix / runtime Step 18.1 `0.0.48 (48)`**, continuing the accelerated model of one related subsystem with several ordered gates. Step 18.1.1 fixes only the host-test Mono.Cecil API compile error discovered by Codemagic; runtime behavior is unchanged.

## Step 18 subsystem — Real Assembly Rewrite Workspace

Step 18 proves that the real StS2 ARM64 managed payload can be prepared and rewritten safely **outside the live installation** before any behaviorally significant compatibility transformation is attempted.

Ordered gates:

- **Gate A — workspace clone:** re-prove OfflineReady, select the Step 17 macOS arm64 + architecture-neutral managed scope, exclude x86_64 duplicates, copy it to launcher-private `Step18-RealAssemblyRewrite/source`, and SHA-1 verify every copy against the Step 12 receipt.
- **Gate B — real primary round-trip:** Cecil-write/reopen the copied ARM64 `sts2.dll`, allowing only strict workspace-local dependency resolution when Cecil metadata emission requires it, and verify its logical metadata fingerprint remains stable.
- **Gate C — semantics-neutral rewrite:** insert one IL `nop` at the entry of a deterministic method in the copied primary `sts2.dll`, write/reopen it, and verify the original first opcode remains immediately after the NOP.
- **Gate D — isolation audit:** re-hash every source workspace file and every corresponding original managed-install file against the receipt, then re-prove the generated round-trip/NOP outputs.

Step 18 writes only under launcher-private Step 18 scratch storage. It never writes the Step 12 managed install, loads StS2 into the CLR, or executes the game. Cecil writer-required dependency resolution is permitted only inside the SHA-1-verified Step 18 source workspace; there is no fallback to runtime/system/live-install/network locations.

## Build

Use Codemagic workflow:

```text
ios-step-18-1
```

Expected app:

```text
0.0.48 (48)
STEP 18.1 — REAL ASSEMBLY REWRITE WORKSPACE
```

Expected IPA:

```text
artifacts/StS2-Launcher-Step-18.ipa
```

See `docs/STEP-18-TEST.md` for physical-iPhone testing.

## Scope boundary

Step 18 does **not** apply a behaviorally significant StS2 compatibility fix, execute `sts2.dll`, integrate FMOD/Spine runtimes, launch the game, add Cloud, or add Workshop support.
