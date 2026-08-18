# Step 18 — Real Assembly Rewrite Workspace

## Boundary

Step 18 is the first boundary that allows Mono.Cecil to **write a real StS2 managed assembly**, but only after that assembly has been copied out of the receipt-backed Step 12 installation into launcher-private scratch storage.

It does **not** apply a real compatibility fix yet.

## Ordered gates

### Gate A — WorkspaceClone

1. Re-prove Step 13 `OfflineReady`.
2. Read the trusted Step 12 install receipt.
3. Select the same iOS-relevant managed scope proven by Step 17: `data_sts2_macos_arm64` plus any architecture-neutral managed candidates, excluding `data_sts2_macos_x86_64` duplicates.
4. Recreate `Documents/StS2Launcher/Step18-RealAssemblyRewrite/source` from scratch.
5. Copy each selected file and SHA-1 verify the copy against the trusted receipt.

### Gate B — PrimaryRoundTrip

Open the launcher-private copy of the primary ARM64 `sts2.dll` with Cecil, write it to the `roundtrip` output tree, reopen it, and compare a logical metadata fingerprint (assembly/version/runtime/type/method/reference counts).

Cecil writer-required dependency resolution is allowed only through a strict resolver rooted in the SHA-1-verified Step 18 workspace. CLR loading and fallback to runtime/system/live-install/network paths remain forbidden.

### Gate C — NeutralIlRewrite

Select one deterministic real method body in the copied ARM64 `sts2.dll` and insert exactly one `nop` before its original first instruction. Write to the separate `rewritten` output tree and reopen it.

Required proof:

- first instruction is the inserted NOP;
- original first opcode immediately follows it;
- instruction count increased by exactly one;
- source copy still matches receipt SHA-1;
- rewritten output differs byte-for-byte from the source.

The NOP is deliberately semantics-neutral. This proves the real rewrite machinery without claiming any iOS compatibility issue has been fixed.

### Gate D — IsolationAudit

Re-hash every selected source copy and every corresponding original Step 12 managed-install file against the receipt. Reopen both generated outputs and re-prove the NOP transformation.

The gate passes only if the original managed installation remains receipt-identical.

## Non-goals

Step 18 does not:

- rewrite the live managed install;
- fix Harmony/Reflection.Emit/PInvoke/GodotSharp/Steamworks behavior;
- resolve dependencies from outside the SHA-1-verified Step 18 workspace;
- load StS2 assemblies into the CLR;
- execute StS2;
- integrate FMOD/Spine;
- add Cloud or Workshop.

## Step 18.1 physical-device correction

The initial Step 18 device run passed Gate A but Gate B failed while Cecil emitted metadata for the real `sts2.dll`:

```text
AssemblyResolutionException: Failed to resolve assembly: GodotSharp, Version=4.5.10...
```

This did not mean the game assembly was missing or corrupt. Cecil can require referenced type metadata while writing an otherwise unchanged module (for example enum-typed constants/default parameters). Step 18.1 therefore supplies an `IAssemblyResolver` that is intentionally restricted to the already receipt-verified `Step18-RealAssemblyRewrite/source` tree. The resolver never searches the live install, the launcher runtime, trusted platform assemblies/GAC, network locations, or arbitrary filesystem paths.
