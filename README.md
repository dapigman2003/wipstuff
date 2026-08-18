# StS2 Launcher iOS — Step 17 Compatibility Call-Site Analysis

Experimental unofficial iOS launcher/compatibility-host project for users who legitimately own Slay the Spire 2 on Steam.

## Project state

**Steps 01–16 are complete and closed on a physical iPhone.** Step 15 physically proved the independent Godot 4.5.1 Metal/touch/lifecycle host. Step 16.1 physically proved Mono.Cecil read/write/reopen, a controlled project-owned IL rewrite, and real StS2 managed-metadata inspection under iOS AOT/full trimming.

This archive is **Step 17 / runtime `0.0.46 (46)`**, continuing the accelerated model of one related subsystem with several ordered gates.

## Step 17 subsystem — Compatibility Call-Site Analysis

Step 14 deliberately over-counted broad string/metadata compatibility signals. Step 17 uses the Step 16 Cecil foundation to inspect **actual IL instruction operands** in the receipt-backed macOS arm64 managed payload.

Ordered gates:

- **Gate A — ARM64 scope:** re-prove OfflineReady, select macOS arm64 + architecture-neutral managed files, exclude x86_64 duplicates, and require the unique arm64 `sts2.dll`.
- **Gate B — actual IL calls:** scan concrete call-like instructions and classify Reflection.Emit, Expression.Compile, Harmony/MonoMod runtime patching, dynamic assembly load, PrepareMethod and indirect `calli` evidence.
- **Gate C — native/platform interop:** report P/Invoke definitions/call sites, native module names, and selected platform-sensitive managed APIs.
- **Gate D — primary dependency map:** report direct `sts2.dll` calls into Godot, Steamworks, FMOD, Spine, Harmony/MonoMod and other external assemblies, then re-hash every scanned file against the install receipt.

Step 17 never calls Cecil dependency resolution, never loads or executes game assemblies, and never writes inside the managed install.

## Build

Use Codemagic workflow:

```text
ios-step-17
```

Expected app:

```text
0.0.46 (46)
STEP 17 — COMPATIBILITY CALL-SITE ANALYSIS
```

Expected IPA:

```text
artifacts/StS2-Launcher-Step-17.ipa
```

The Step 15 Godot host and Step 16 project-owned fixture remain packaged only as regression boundaries.

See `docs/STEP-17-TEST.md` for physical-iPhone testing.

## Scope boundary

Step 17 does **not** rewrite real StS2 assemblies, execute `sts2.dll`, integrate FMOD/Spine runtimes, launch the game, add Cloud, or add Workshop support.
