# Step 19.2 physical-iPhone test

Build workflow:

```text
ios-step-19-2
```

Confirm:

```text
STEP 19.2 — EXPRESSION INTERPRETER COMPATIBILITY
Version 0.0.54
```

Run the ordered A–D expression compatibility button and stop at the first failure.

## Gate A expected proof

Gate A should report all three results as `42`:

```text
Compile() automatic-fallback probe result: 42
Compile(preferInterpretation: false) probe result: 42
Compile(preferInterpretation: true) probe result: 42
RuntimeFeature.IsDynamicCodeSupported: False
RuntimeFeature.IsDynamicCodeCompiled: False
iOS no-dynamic-code fallback precondition proven: True
```

If `Compile()` or `Compile(false)` fails while dynamic code is false, stop and report the full Gate A diagnostic. That would be a host `System.Linq.Expressions` runtime/configuration problem, not a reason to mutate copied desktop framework DLLs.

## Gate B expected proof

Gate B is read-only. It should classify the real call sites observed in the receipt-backed workspace and report:

```text
Assemblies selected for Cecil mutation: 0
Gate B compatibility disposition: HOST RUNTIME FALLBACK — NO GAME/APPLICATION IL REWRITE REQUIRED
```

The exact framework/consumer counts are evidence and are not hard-coded.

## Gates C–D expected proof

Gate C must report:

```text
Cecil assembly writes performed by Gate C: 0
Strong-name flags/public keys/tokens modified: NO
Consumer/game assemblies rewritten: NO
```

Gate D must prove every source, prepared, and live-install file remains receipt-identical and report zero managed Compile call sites rewritten.

A 4/4 pass does not claim StS2 starts. After 4/4, run:

```text
Verify Offline-Ready Install (Local Only)
Run Foundation 5/5 Regression
```

If those pass, Step 19 can be closed and the next subsystem can address runtime payload/framework binding or the next evidence-backed compatibility boundary.
