# Step 18.2 — Workspace Assembly-Identity Resolver Fix

The physical iPhone repeated Gate B's `AssemblyResolutionException` for `GodotSharp` after Step 18.1. This confirmed that the first workspace-only resolver implementation was incomplete.

## Root problem

Step 18.1 still constructed candidate dependency paths as `<AssemblyName>.dll` / `<AssemblyName>.exe`. That silently assumed a managed assembly's metadata name must equal its depot filename. A real compatibility workspace must not rely on that convention.

Cecil can also resolve types during metadata emission (for example enum-typed default values), so Step 18.2 explicitly binds both `ReaderParameters.AssemblyResolver` and `ReaderParameters.MetadataResolver` to the same workspace-only policy.

## Step 18.2 correction

- Build a catalog by reading the **assembly identity stored in each SHA-1-verified workspace file**.
- Skip receipt-backed `.dll/.exe` files that are not managed PE files.
- Match requested references by assembly metadata identity (name/version/culture/public-key token), not filename.
- Prefer the primary ARM64 directory when byte-identical duplicates share an identity.
- Reject byte-distinct ambiguous duplicates.
- Recheck SHA-1 immediately before every Cecil metadata probe/open.
- Explicitly use `MetadataResolver(workspaceResolver)` for the primary module and resolved dependencies.
- Keep all fallback to runtime/system/live-install/network paths forbidden.

The host regression now deliberately stores the synthetic `GodotSharp` assembly under a filename that is **not** `GodotSharp.dll`; Gate B must still resolve it by metadata identity.

Runtime version: `0.0.49 (49)`
Workflow: `ios-step-18-2`
