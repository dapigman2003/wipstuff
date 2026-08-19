# Step 19.1 — strong-name identity prepared-copy fix

## Physical trigger

Step 19 / `0.0.52` reached the physical iPhone and produced:

```text
Gate A — InterpreterCapabilityAndWorkspaceClone: PASS
Compile(preferInterpretation: true) probe result: 42
RuntimeFeature.IsDynamicCodeSupported: False
RuntimeFeature.IsDynamicCodeCompiled: False

Gate B — RealCompileTargetDiscovery: FAIL
parameterless-safe=8
literal-false=0
parameterless-unsafe=2
strong-named-supported=8
```

The expression-interpreter target was therefore validated, not disproved: eight structurally-safe real direct `Compile()` calls exist, but the original policy excluded every one because their containing assemblies carried strong-name metadata.

## Correction

Step 19.1 keeps those real sites eligible under a narrowly defined prepared-copy rule:

- receipt-backed install and Step 19 `source` copies remain unchanged;
- assembly name/version/culture/public key/public-key token/full name remain unchanged;
- if a modified source module sets `StrongNameSigned`, the prepared copy clears that stale flag before Cecil writes the changed bytes;
- no private signing key is used;
- public key/token are not stripped;
- dependent `AssemblyRef` identities are not rewritten;
- signed-without-public-key metadata is rejected as malformed;
- Gate C and Gate D independently verify before/after identity and signature-disposition state.

This deliberately avoids two broader alternatives: stripping strong-name identity from a graph of game assemblies, or inventing/re-signing with a launcher-controlled key. Both would create unnecessary identity churn and a much larger compatibility surface.

## What remains unproven

Step 19.1 still does not CLR-load or execute the prepared game payload. Passing these gates proves only that the intended IL transformation and strong-name identity disposition are structurally consistent and isolated. Later execution gates remain authoritative for actual runtime binding/loading behavior.
