# Step 17 — Compatibility Call-Site Analysis — Design

## Boundary

Step 17 is a **read-only compatibility-evidence subsystem**. It converts Step 14's broad metadata/string indicators into concrete IL/native/dependency evidence for the receipt-backed macOS arm64 managed payload.

It does not rewrite, resolve, load, or execute StS2 assemblies.

## Ordered gates

### Gate A — ARM64 managed scope

- re-prove the Step 13 `OfflineReady` exact local tree;
- read the trusted Step 12 receipt;
- select every receipt-backed `data_sts2_macos_arm64` `.dll/.exe` candidate;
- include any architecture-neutral managed candidates;
- deliberately exclude `data_sts2_macos_x86_64` duplicates from compatibility prioritization;
- require exactly one primary `data_sts2_macos_arm64/sts2.dll`.

### Gate B — actual IL call sites

Open each selected file as one Cecil `ModuleDefinition` in deferred mode and inspect actual method bodies. Record only concrete call-like IL instructions (`call`, `callvirt`, `newobj`, function-pointer loads, `jmp`, and `calli`).

Dynamic/AOT-sensitive categories include:

- `System.Reflection.Emit`;
- `Expression.Compile`;
- Harmony runtime patch APIs;
- MonoMod runtime-detour/dynamic-method APIs;
- dynamic assembly loading;
- `RuntimeHelpers.PrepareMethod`;
- indirect `calli`.

This is stronger evidence than Step 14's string markers, but it still does not prove runtime reachability.

### Gate C — native/platform interop

Classify:

- P/Invoke definitions;
- observed calls to P/Invoke methods within their defining modules;
- native module names;
- direct calls to selected platform-sensitive APIs such as `System.Diagnostics.Process`, `Microsoft.Win32.Registry`, Windows principal APIs, `NativeLibrary`, and native function-pointer conversion APIs.

The output is triage evidence, not an automatic blocker verdict.

### Gate D — primary dependency pressure map

For the unique arm64 `sts2.dll`:

- count direct external method-reference targets by assembly/scope;
- count direct calls into Godot/GodotSharp, Steamworks, FMOD, Spine, Harmony and MonoMod;
- report actual dynamic/platform-sensitive call counts inside `sts2.dll`;
- report a bounded sample of concrete source → target calls;
- re-hash every Step 17 scan candidate against the trusted Step 12 receipt after analysis.

## Safety invariants

Step 17 must not call Cecil `Resolve()`, `Assembly.Load`, `AssemblyLoadContext`, reflection invocation, game code, Steam, HTTP, CDN, or any filesystem write/move/delete API from the Step 17 analysis class.

No real StS2 file is changed.

## Why this step exists

Step 14 found 98 managed files with broad dynamic/JIT indicators and 273 platform-specific indicators. Those counts were intentionally conservative. Step 17 narrows that evidence to concrete IL operands and native interop declarations so the next compatibility step can target an actual required incompatibility instead of rewriting based on string presence alone.
