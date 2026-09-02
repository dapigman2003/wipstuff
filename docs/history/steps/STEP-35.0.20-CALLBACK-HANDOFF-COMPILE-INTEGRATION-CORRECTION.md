# Step 35.0.20 — callback-handoff compile-integration correction

Candidate: 0.0.143 (143)

## Evidence requiring the correction

Codemagic for 0.0.142 passed 855/855 static checks, all 211 host tests, and the Step-15 standalone native-link preflight. The iOS C# compile then failed with CS0103 at every Step-35 reference to `GodotStep15NativeBridge`.

## Diagnosis

`GodotStep15NativeBridge` is declared in `StS2Launcher.iOS.Platform`. The established `RootViewController.Godot.cs` partial imports that namespace. The new `RootViewController.TransformedRealStS2VeryEarlyInitialization.cs` partial did not. Partial-class files do not share `using` directives, so the Step-35 file could not resolve the bridge type even though the bridge source was present and its native symbols passed standalone link preflight.

## Correction

0.0.143 adds exactly one source-level integration fix to the Step-35 UI partial:

`using StS2Launcher.iOS.Platform;`

The release identity advances to Step 35.0.20 / 0.0.143 (143). Static validation now requires both the bridge source and the explicit Platform namespace import in the Step-35 partial, preserving this exact failure as a regression guard.

## Unchanged runtime authority

CORE-HANDOFF runtime behavior is unchanged from 0.0.142. NATURAL / OS-RECON / FORWARD remain the fresh-process controls. CORE-HANDOFF still requires the already-live project-owned Step-15 Godot engine, rejects competing Godot-managed-runtime state, obtains the exact source-built Godot 4.5.1 runtime-interoperability callback table, initializes only the verified private GodotSharp derivative through `NativeFuncs.Initialize(IntPtr,int)` once, and then measures the natural ExecuteVeryEarly path. No game native executable is loaded and no callback pointer is fabricated. A diagnostic 4/4 cannot close exact Step 35.
