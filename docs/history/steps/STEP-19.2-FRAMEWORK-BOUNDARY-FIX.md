# Step 19.2 — host-fallback / framework-boundary correction

## Physical failure addressed

Step 19.1 reached Gate C and failed at:

```text
Stage: Cecil rewrite: .../data_sts2_macos_arm64/System.Linq.Expressions.dll
NotSupportedException: Writing mixed-mode assemblies is not supported
```

The target path is the decisive evidence. Gate B had selected the copied macOS `System.Linq.Expressions.dll` framework implementation itself. Calls inside that framework assembly are not proof that StS2/application consumers require the same rewrite.

Mono.Cecil intentionally refuses to write a module when `ModuleAttributes.ILOnly` is clear. ReadyToRun/crossgen framework images are a known example of inputs that reach this boundary. Step 19.2 does **not** bypass this check, force the `ILOnly` flag, strip ReadyToRun/native sections, or attempt to rebuild the desktop framework image.

## Why no expression IL rewrite is now preferred

Modern `System.Linq.Expressions` was changed so `LambdaExpression.Compile()` respects `RuntimeFeature.IsDynamicCodeSupported`; if IL compilation is unavailable it falls back to the expression interpreter. The bool overload returns the interpreter directly when requested and otherwise falls through to `Compile()`, so `Compile(false)` also uses the fallback when IL compilation is unavailable.

Step 19 already physically proved `Compile(true)` works while both dynamic-code feature flags are false. Step 19.2 strengthens that proof by testing `Compile()`, `Compile(false)`, and `Compile(true)` independently on the physical iPhone.

If all three succeed with dynamic code unsupported/compiled false, rewriting direct Compile call sites is redundant at this compatibility layer.

## New policy

- Gate B keeps scanning all real direct Compile sites for evidence and ownership classification.
- `System.*` framework implementation and non-IL-only/ReadyToRun/mixed-mode images are diagnostic-only.
- Non-framework consumers are also diagnostic-only for Step 19.2; no call-site mutation is selected because the host runtime fallback has been physically proven.
- Gate C performs zero Cecil writes and creates a receipt-identical prepared tree.
- Gate D requires the complete source/prepared/live trees to remain receipt-identical.

The later execution architecture must bind managed framework references to the iOS host/runtime framework set. Step 19.2 intentionally does not claim that binding is already proven.
