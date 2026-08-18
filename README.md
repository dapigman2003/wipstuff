# StS2 Launcher iOS — Step 18 Real Assembly Rewrite Workspace

Experimental unofficial iOS launcher/compatibility-host project for users who legitimately own Slay the Spire 2 on Steam.

## Project state

**Steps 01–17 are complete and closed on a physical iPhone.** Step 15 physically proved the independent Godot 4.5.1 Metal/touch/lifecycle host. Step 16.1 physically proved Mono.Cecil read/write/reopen and real StS2 read-only metadata inspection. Step 17 physically passed the receipt-backed ARM64 actual-IL/native/dependency analysis subsystem.

This archive is **Step 18.4 / `0.0.51 (51)`**. Step 18.3 physically advanced the writer past the `System.Runtime 8.0.0.0` versus workspace `9.0.0.0` version boundary, but Gate B then surfaced `AssemblyResolutionException` for `GodotSharp` again. Source review found a separate resolver escape: the real source write used the strict workspace resolver, while Gate B's generated-output reopen (and the equivalent Gate C/Gate D audit reopens) called Cecil without resolver parameters. Step 18.4 removes every such generated-output escape, binds both Cecil resolver layers for source **and output** reads, uses deferred reads for verification/dependency metadata, and reports the exact failing stage.

## Step 18 subsystem — Real Assembly Rewrite Workspace

Step 18 proves that the real StS2 ARM64 managed payload can be prepared and rewritten safely **outside the live installation** before any behaviorally significant compatibility transformation is attempted.

Ordered gates:

- **Gate A — workspace clone:** re-prove OfflineReady, select the Step 17 macOS arm64 + architecture-neutral managed scope, exclude x86_64 duplicates, copy it to launcher-private `Step18-RealAssemblyRewrite/source`, and SHA-1 verify every copy against the Step 12 receipt.
- **Gate B — real primary round-trip:** Cecil-write the copied ARM64 `sts2.dll`, then reopen the generated output with a fresh explicitly bound workspace resolver (never Cecil's implicit default resolver), and verify its logical metadata fingerprint remains stable.
- **Gate C — semantics-neutral rewrite:** insert one IL `nop` at the entry of a deterministic method in the copied primary `sts2.dll`, write it, reopen the generated output through the explicit workspace resolver, and verify the original first opcode remains immediately after the NOP.
- **Gate D — isolation audit:** re-hash every source workspace file and every corresponding original managed-install file against the receipt, then re-prove both generated outputs using explicit-resolver deferred reopens.

Step 18 writes only under launcher-private Step 18 scratch storage. It never writes the Step 12 managed install, loads StS2 into the CLR, or executes the game. Cecil writer-required dependency resolution is permitted only inside the SHA-1-verified Step 18 source workspace; there is no fallback to runtime/system/live-install/network locations.

## Build

Use Codemagic workflow:

```text
ios-step-18-4
```

Expected app:

```text
0.0.51 (51)
STEP 18.4 — REAL ASSEMBLY REWRITE WORKSPACE
```

Expected IPA:

```text
artifacts/StS2-Launcher-Step-18.ipa
```

See `docs/STEP-18-TEST.md` for physical-iPhone testing.

## Scope boundary

Step 18 does **not** apply a behaviorally significant StS2 compatibility fix, execute `sts2.dll`, integrate FMOD/Spine runtimes, launch the game, add Cloud, or add Workshop support.
