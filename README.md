# StS2 Launcher iOS — Step 19.2 Expression Interpreter Compatibility

**Version:** `0.0.54 (54)`  
**Codemagic workflow:** `ios-step-19-2`

Steps 01–18 are physically complete and closed. Step 19 investigates one AOT-sensitive class: direct `System.Linq.Expressions` `Compile` usage.

## Physical evidence that led to 19.2

- Step 19 / `0.0.52`: Gate A proved `Compile(preferInterpretation: true)` executes on the physical iPhone while `RuntimeFeature.IsDynamicCodeSupported` and `RuntimeFeature.IsDynamicCodeCompiled` are both false. Gate B observed 8 structurally-safe parameterless direct `Compile()` sites and 2 unsafe sites, but the first policy rejected the safe sites because their containing assemblies carried strong-name identity.
- Step 19.1 / `0.0.53`: strong-name handling was widened for prepared copies. Gate C then failed while attempting to write copied `System.Linq.Expressions.dll` with Mono.Cecil: `NotSupportedException: Writing mixed-mode assemblies is not supported`.

The 19.1 failure exposed a target-ownership error: direct calls *inside the copied desktop framework implementation* are not evidence that StS2/application IL itself needs rewriting. The copied macOS `System.Linq.Expressions.dll` is also a non-IL-only/ReadyToRun-or-mixed-mode image from Cecil's perspective, so forcing it through Cecil's writer would cross the wrong compatibility boundary.

Modern `System.Linq.Expressions` already routes `LambdaExpression.Compile()` through the interpreter when dynamic code is unavailable. `Compile(false)` falls through the same `Compile()` path when IL compilation is unavailable. Step 19.2 therefore proves those behaviors directly in the actual iOS host instead of modifying copied desktop framework images.

## Step 19.2 gates

A. **InterpreterCapabilityAndWorkspaceClone** — execute and verify all three host paths: `Compile()`, `Compile(preferInterpretation: false)`, and `Compile(preferInterpretation: true)`; record dynamic-code feature flags and the host `System.Linq.Expressions` identity; re-prove OfflineReady; clone and SHA-1 verify the ARM64/shared managed workspace.

B. **RealCompileTargetDiscovery** — read-only scan real direct expression `Compile` sites. Classify caller ownership (`System.*` framework versus non-framework consumer), strong-name identity, IL-only versus non-IL-only/ReadyToRun/mixed-mode shape, primary `sts2.dll` pressure, and old branch/EH insertion hazards. **Select zero assemblies for mutation.**

C. **HostFallbackPreparedCopy** — Step 19.2 performs **zero Cecil assembly writes**. Copy the complete prepared tree byte-for-byte and immediately prove every prepared SHA-1 equals the verified source/receipt.

D. **IsolationAudit** — independently re-hash source, prepared, and live-install trees. Every prepared file must remain receipt-identical; zero rewrite records are an invariant.

A 4/4 pass closes this direct expression-Compile compatibility class as:

```text
HOST RUNTIME FALLBACK — NO GAME/APPLICATION IL REWRITE REQUIRED
```

This does not claim StS2 executes yet. A later runtime-payload subsystem still has to prove that game/framework references are bound to the iOS host runtime rather than copied desktop framework implementation images.
