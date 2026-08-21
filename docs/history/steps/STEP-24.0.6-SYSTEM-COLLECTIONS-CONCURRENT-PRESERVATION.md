# Step 24.0.6 — System.Collections.Concurrent Dynamic-IL Preservation

## Physical evidence that motivated this candidate

Step 24.0.5 / `0.0.78 (78)` reached a physical iPhone and advanced materially farther than every earlier Step 24 build:

- Gate A — InitializationPreflight: **PASS**;
- Gate B — ProvenLoadStateReplay: **PASS**;
- Gate C — DeferredModuleInitialization: **FAIL** at the explicit `RuntimeHelpers.RunModuleConstructor` completion barrier;
- Gate D did not run.

Gate A reproduced the exact measured build-77 metadata fingerprint: one initializer-bearing dependency, exact `0Harmony 2.4.2.0`, one `<Module>..cctor`, four automatic initializers, 111 same-assembly methods audited, seven raw conservative MonoMod logging findings, seven conditionally dormant findings, zero blocking initializer hazards, and inert logger state. Gate B then reproduced the physically proven Step 23 private load state with six initializer-free private assemblies, zero native attempts, zero rejected/unplanned requests, and `0Harmony` still absent.

Gate C successfully loaded the exact receipt-backed `0Harmony` assembly and began its module initializer. The explicit completion barrier then threw:

``System.MissingMethodException: Method not found: void System.Collections.Concurrent.ConcurrentBag`1..ctor()``

The exception arose while `MonoMod.Logs.DebugLog::.cctor` was being triggered by `MMDbgLog.LogVersion()` from `<Module>..cctor`.

This is not evidence that `ConcurrentBag<T>()` is absent from .NET 9. It is a real framework API. The launcher is published with `TrimMode=full`, while the real `0Harmony` assembly is intentionally not a build-time project/content reference and enters the CLR only from the receipt-backed prepared runtime on-device. The build-time trimmer therefore cannot statically observe every framework member reachable only from this post-publish IL.

## Candidate correction

Step 24.0.6 / `0.0.79 (79)` makes exactly one runtime-build policy change:

- add `System.Collections.Concurrent` as one **Step-24 candidate-only `TrimmerRootAssembly`**.

This preserves the framework assembly and its statically understood dependencies from trimming so the dynamically loaded MonoMod initialization closure can reach the measured concurrent-collection surface.

The candidate deliberately does **not**:

- disable or reduce global `TrimMode=full`;
- change the physically proven `MtouchInterpreter=-all` policy;
- enable broad `UseInterpreter=true`;
- interpret `System.Collections.Concurrent` specially;
- add any downloaded/private `System.Collections.Concurrent.dll` implementation;
- expand the Step 22 direct-root set itself (the exact 22 Step 22 roots remain unchanged);
- alter the Step 24 Gate A conditional hazard policy;
- alter Gate B private-context membership or resolver behavior;
- broaden native resolution;
- invoke Harmony patch APIs or StS2 game members.

The whole `System.Collections.Concurrent` assembly is rooted rather than preserving only one constructor because the same physically measured automatic-initialization IL also uses `ConcurrentDictionary<,>` construction and `TryAdd`. One framework-assembly preservation root tests the related trim-survival surface in one physical run while remaining much narrower than disabling trimming or broadening the interpreter policy.

## What the next physical run can prove

If build 79 reaches Gate C and the module constructor completes, the physical evidence will show that the build-78 failure was a trim-survival issue for the dynamically reached framework surface.

If Gate C instead progresses to a distinct AOT/generic-execution failure, that becomes a separate boundary. Do not preemptively change `MtouchInterpreter=-all`; the interpreter/AOT policy remains protected until physical evidence specifically requires a new candidate.

If Gate C reaches a different missing framework member, preserve the exact new evidence and decide whether it belongs to the same bounded framework assembly or exposes another separately measured host-surface requirement.

## Candidate identity

- step: **24.0.6**
- version: **0.0.79 (79)**
- workflow: **`ios-step-24`**
- IPA: **`artifacts/StS2-Launcher-Step-24.ipa`**
- device report: `Documents/StS2Launcher/Reports/Step24-ControlledManagedInitialization.txt`
