# Step 15.0.4 — Normal Godot static-archive link semantics

## Codemagic evidence

Step 15.0.3 progressed through the pinned Godot 4.5.1 iOS build and all project-owned archive preflights, then failed at the final .NET/iOS native link with:

```text
duplicate symbol '__pcre2_ckd_smul' in:
  libgodot-step15.a(...pcre2_chkdint_16...)
  libgodot-step15.a(...pcre2_chkdint_32...)
ld: 1 duplicate symbols
```

## Cause

The app project marked the *combined* Godot archive `ForceLoad=true`. That turns normal static-library selection into whole-archive loading. Godot's combined archive contains width-specific PCRE2 object members; forcing all members into one executable pulls both 16-bit and 32-bit private checked-integer helpers, whose private helper symbol is intentionally not width-suffixed.

This is different from an archive being malformed. A static archive may contain object members with overlapping private definitions as long as normal link resolution does not select incompatible members together.

## Fix

Only the Step 15 Godot NativeReference policy changes:

```xml
<ForceLoad>false</ForceLoad>
<SmartLink>false</SmartLink>
<LinkerFlags>-ObjC -lz</LinkerFlags>
```

`-ObjC` is retained for Objective-C class/category members. The project-owned C/C++ bridge exports and `apple_embedded_main` references provide ordinary linker roots for the engine dependency graph.

No Godot source behavior, app runtime version, Gate A-D contract, Steam foundation, managed install, or Step 14 inventory behavior changes.
