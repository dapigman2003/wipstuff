# StS2 Launcher iOS — Step 20 Dynamic Managed Execution Foundation

**Version:** `0.0.55 (55)`  
**Codemagic workflow:** `ios-step-20`

Steps **01–19 are physically complete and closed** on the iPhone, including the Step 19.2 expression-interpreter/framework-boundary result plus OfflineReady and Foundation 5/5 closure regressions.

Step 20 proves the next prerequisite for a launcher that obtains the game after the IPA is installed: the Release iOS host must be able to load and execute managed IL that was **not present as an AOT input when the IPA was built**.

The build keeps all build-time launcher assemblies AOT-targeted with:

```xml
<MtouchInterpreter>-all</MtouchInterpreter>
```

Microsoft documents this configuration as AOT-compiling all build-time assemblies while retaining the Mono interpreter for runtime/dynamic managed code. The three Step 20 fixture DLLs are deliberately built separately and copied into the `.app` **only after `dotnet publish` completes**. They are never project references, content/resource items, or AOT/link inputs.

## Ordered gates

A. **FixtureIntegrityAndOfflineReady** — re-prove OfflineReady; validate the bundled SHA-256 manifest; require all three project-owned fixtures to be pure IL with a tightly bounded assembly-reference graph; Cecil-probe their exact identities; copy them into launcher-private Step 20 storage and re-hash them.

B. **DynamicFixtureExecution** — create a fresh dedicated `AssemblyLoadContext`, load the standalone fixture from exact verified bytes, reflect its public `Run()` probe, and require deterministic result `42`. The fixture exercises loops, generics, and exception-finally IL. No private dependency is allowed at this gate.

C. **PrivateDependencyResolution** — create another fresh load context, load a root fixture from verified bytes, and satisfy its one project-owned dependency only from the SHA-256-verified private fixture directory. The requested name/version/culture/public-key-token must exactly match the Cecil-probed identity and the dependency is re-hashed immediately before load. Result must again be `42`. Unknown non-framework fallback is rejected.

D. **IsolationAudit** — re-hash all private fixtures and the manifest, re-prove the complete OfflineReady managed-install tree, verify the managed-install identity did not change, and explicitly assert that no assembly named `sts2` entered the CLR during Step 20.

Target device result:

```text
DYNAMIC MANAGED EXECUTION FOUNDATION PASS — 4/4
```

After 4/4, run **Verify Offline-Ready Install (Local Only)** and **Foundation 5/5 Regression** before closing Step 20.

Step 20 intentionally does **not** load or execute any StS2 assembly, bind GodotSharp/game framework references, run Harmony/MonoMod, integrate FMOD/Spine, or add Cloud/Workshop. If Step 20 closes, Step 21 can address the prepared runtime/framework binding boundary with the dynamic managed-execution mechanism physically proven first.
