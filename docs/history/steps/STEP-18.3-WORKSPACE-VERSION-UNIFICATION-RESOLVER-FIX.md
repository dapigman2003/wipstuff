# Step 18.3 — Workspace Version-Unification Resolver Fix

Step 18.2 physically advanced Gate B past the original `GodotSharp` filename/metadata-resolver failure. The next real-device failure was:

```text
System.Runtime, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
```

while the SHA-1-verified Step 18 workspace contained:

```text
System.Runtime, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
```

This proves the Step 18.2 catalog is now being used, but its exact-version identity rule is stricter than Cecil's normal same-directory assembly resolution behavior. Cecil's normal resolver opens a same-name assembly found in its search directory without first requiring the file's assembly version to equal the requested `AssemblyRef` version.

## Step 18.3 correction

- Preserve exact metadata identity matching as the first choice.
- If no exact match exists, consider only workspace candidates with the same assembly name, culture, and public-key token while ignoring version.
- Permit the version-only fallback only when the verified workspace presents exactly one candidate assembly identity.
- Continue rejecting multiple version-distinct identities rather than guessing.
- Continue rejecting byte-distinct duplicates for the selected identity.
- Recheck the selected file's receipt SHA-1 immediately before Cecil opens it.
- Re-open and verify the selected file still has the exact catalog identity that was chosen.
- Keep both Cecil assembly and metadata resolution bound to the same workspace resolver.
- Keep runtime/system/live-install/network fallback forbidden.

The output assembly's original `AssemblyRef` version is not rewritten by this resolver policy. The fallback supplies metadata to Cecil's writer only; it is not a runtime binding redirect or a behavioral compatibility patch.

## Regression

The Step 18 host regression still proves filename-independent `GodotSharp` resolution, and now additionally creates a primary assembly that requests a synthetic `System.Runtime` 8.0.0.0 enum dependency while the verified workspace contains only `System.Runtime` 9.0.0.0 under an unrelated filename. Gate B must round-trip successfully and report the workspace version-unification trace.

Runtime version: `0.0.50 (50)`
Workflow: `ios-step-18-3`
