# Step 19.1 — Expression Interpreter Compatibility

## Why this remains the first behavioral rewrite

Step 18 physically proved that real StS2 managed assemblies can be transformed safely in a closed launcher-private workspace. Step 19 therefore changes one real compatibility behavior without combining unrelated AOT problems.

The selected boundary is direct `System.Linq.Expressions` compilation. Modern .NET exposes `LambdaExpression.Compile(bool preferInterpretation)` and `Expression<TDelegate>.Compile(bool preferInterpretation)`. Passing `true` requests interpreted execution when available. Gate A proves that path functionally on the physical iPhone before any game assembly is modified.

The original Step 19 / `0.0.52` physical run produced decisive evidence:

```text
Gate A: PASS
Compile(preferInterpretation: true) probe result: 42
RuntimeFeature.IsDynamicCodeSupported: False
RuntimeFeature.IsDynamicCodeCompiled: False

Gate B: FAIL under the original unsigned-only policy
Structurally-safe parameterless Compile() sites: 8
Literal Compile(false) sites: 0
Parameterless sites skipped for structural safety: 2
Supported sites carrying strong-name identity: 8
```

That is not evidence to abandon the expression-interpreter target. It proves the real ARM64 payload contains eight structurally-safe direct calls matching the intended rewrite, but the first policy excluded every one because their containing assembly carried strong-name identity/signature metadata.

Step 19.1 corrects that policy while keeping the live/source trust boundary unchanged.

## Trust and write model

Step 19.1 creates a fresh workspace:

```text
Documents/StS2Launcher/Step19-ExpressionInterpreterCompatibility/source
Documents/StS2Launcher/Step19-ExpressionInterpreterCompatibility/prepared
```

`source` is cloned from the Step 12 receipt-backed install using the physically proven ARM64/shared selection rule. Every source copy is SHA-1 verified against the receipt. The live install and the `source` copies remain byte-identical to the receipt throughout all gates.

All Cecil dependency resolution remains restricted to the SHA-1-verified Step 19 `source` tree using the Step 18-proven assembly-identity catalog, exact-first matching, unambiguous same-name/culture/token version unification, immediate SHA-1 recheck before dependency open, and explicit assembly + metadata resolver binding. Generated `prepared` assemblies are never added as resolver inputs.

## Strong-name identity/signature-disposition policy

The original Step 19 treated every strong-name-bearing assembly as non-writable. That was unnecessarily broad for the modern .NET runtime targeted by this launcher.

Step 19.1 distinguishes **assembly identity** from the now-stale **signature claim** after a prepared copy is modified.

For any selected assembly:

1. Capture its `StrongNameSigned` flag, public key bytes, public-key token, and complete assembly full name before rewriting.
2. If `StrongNameSigned` is set but no public key exists, reject the assembly as malformed rather than guessing.
3. Preserve assembly name, version, culture, full public key, public-key token, and resulting assembly full name exactly.
4. If the source has `StrongNameSigned`, clear only `ModuleAttributes.StrongNameSigned` in the **modified prepared copy** before writing. The original signature cannot honestly describe modified bytes.
5. Do not supply, invent, or embed any private strong-name signing key.
6. Do not strip the public key/token and do not rewrite dependent `AssemblyRef` identities.
7. Reopen the output and prove the public key/token/full identity are unchanged and the modified copy no longer claims `StrongNameSigned`.
8. Gate D independently reopens both the receipt-backed source and prepared outputs and re-proves the recorded before/after strong-name state.

This policy is intentionally limited to launcher-private prepared copies. It does not alter the receipt-backed install or source clone, and it does not claim runtime execution compatibility by itself; later load/execution gates remain authoritative.

## Ordered gates

### Gate A — InterpreterCapabilityAndWorkspaceClone

1. In the actual launcher process, construct a captured expression, not a constant-only expression.
2. Execute it through `Compile(preferInterpretation: true)` and require result `42`.
3. Record `RuntimeFeature.IsDynamicCodeSupported` and `RuntimeFeature.IsDynamicCodeCompiled` as diagnostics only.
4. Re-prove OfflineReady.
5. Read the trusted Step 12 receipt.
6. Select `data_sts2_macos_arm64` plus architecture-neutral managed filename candidates and exclude `data_sts2_macos_x86_64` duplicates.
7. Recreate the Step 19 `source` tree and copy/SHA-1-verify the complete selected scope.

No StS2 assembly is CLR-loaded or executed.

### Gate B — RealCompileTargetDiscovery

Open every managed candidate through the explicit workspace resolver and scan actual IL `call` / `callvirt` operands.

Eligible method families:

```text
System.Linq.Expressions.LambdaExpression::Compile(...)
System.Linq.Expressions.Expression<TDelegate>::Compile(...)
```

Classify each direct call as:

- structurally-safe parameterless `Compile()`;
- literal `Compile(false)`;
- already-interpreter-preferred literal `Compile(true)`;
- dynamic/non-literal `Compile(bool)`;
- parameterless but structurally unsafe for insertion.

For each containing assembly also capture strong-name identity/signature state.

Gate B requires at least one structurally-safe real target. A target is excluded only when its strong-name metadata is malformed (`StrongNameSigned` with no public key), not merely because it carries a public key or signed flag.

The previous physical `0.0.52` run observed eight safe parameterless real targets. If the depot is unchanged, Step 19.1 is expected to select those same sites rather than rejecting them solely for strong-name state; the gate still computes all counts from the actual receipt-backed payload and does not hard-code eight as a pass condition.

### Gate C — PreferInterpretationRewrite

For every selected target assembly:

1. Recheck the source copy against the receipt immediately before transformation.
2. Open it with `ReadingMode.Immediate` and the explicit source-workspace assembly + metadata resolver.
3. Re-scan and require Gate B target counts, assembly identity, and strong-name state to be unchanged.
4. Apply only these transformations:

```text
instance.Compile()
    ->
instance.Compile(true)
```

and

```text
instance.Compile(false)   // immediate literal only
    ->
instance.Compile(true)
```

5. If the source sets `StrongNameSigned`, clear only that flag in the prepared in-memory module. Preserve public key/token/full assembly identity and use no private key.
6. Write to a temporary file under `prepared`, then atomically move it into the prepared tree.
7. Reopen the generated assembly with `ReadingMode.Deferred` and a fresh explicit source-workspace resolver.
8. Verify:
   - structural metadata fingerprint is unchanged except for the intentional signed-flag disposition;
   - full assembly identity/public key/public-key token are unchanged;
   - modified prepared output does not claim `StrongNameSigned`;
   - total direct Compile-site count is unchanged;
   - no selected safe parameterless/literal-false site remains;
   - dynamic and structurally unsafe classes are unchanged;
   - `Compile(true)` count increased by exactly the rewritten-site count;
   - instruction count increased only by the parameterless calls that required a new bool push;
   - source SHA-1 remains receipt-identical;
   - prepared bytes differ from source;
   - number of cleared `StrongNameSigned` flags exactly matches the signed target-assembly count discovered at Gate B.

#### Parameterless IL insertion safety

Adding `ldc.i4.1` changes code size by one byte. Step 19 therefore refuses parameterless insertion when:

- the call immediately follows an IL prefix;
- the call is a branch or exception-handler boundary/entry point;
- any short branch crosses the insertion position.

The short-branch rule is intentionally conservative: a short branch has an 8-bit displacement, so a one-byte insertion can invalidate a previously legal boundary case.

#### Literal-false size preservation

Literal-false rewrites do not insert IL. They preserve the original constant instruction width:

```text
ldc.i4.0   -> ldc.i4.1
ldc.i4.s 0 -> ldc.i4.s 1
ldc.i4 0   -> ldc.i4 1
```

This avoids unnecessary branch-displacement changes.

### Gate D — IsolationAudit

1. Require the Step 19 `source` and `prepared` trees to contain exactly the receipt-selected file set.
2. Re-hash every source copy against the receipt.
3. Re-hash every corresponding original live-install file against the receipt.
4. Require every non-target prepared file to remain receipt-identical.
5. Require every target prepared assembly to match its Gate C output hash and differ from its source hash.
6. Reopen each rewritten **source** assembly with a fresh explicit resolver and re-prove its Gate C pre-rewrite fingerprint and strong-name state.
7. Reopen each rewritten **prepared** assembly with a fresh explicit resolver and re-prove its Gate C post-rewrite fingerprint, strong-name state, and Compile-site counts.
8. Require the final total rewrite count to equal the Gate B selected set exactly.

A 4/4 pass proves an isolated, reproducible behaviorally meaningful prepared payload. It still does not claim StS2 execution.

## Deliberate non-goals

Step 19.1 does not:

- rewrite dynamic/non-literal `Compile(bool)` decisions;
- rewrite malformed strong-name identities;
- strip public keys/tokens or re-key/re-sign game assemblies;
- use a private strong-name signing key;
- modify control-flow-sensitive parameterless sites merely to increase coverage;
- implement general Reflection.Emit substitution;
- implement Harmony/MonoMod detours;
- load prepared StS2 assemblies with `Assembly.Load`;
- execute StS2;
- integrate game FMOD/Spine binaries;
- add Cloud or Workshop.

## Host regression coverage

The Step 19.1 host fixtures exercise:

- both `LambdaExpression` and generic `Expression<TDelegate>` parameterless calls;
- three literal-false encodings (`ldc.i4.0`, `ldc.i4.s`, `ldc.i4`);
- an already-true call;
- a dynamic bool call that must remain untouched;
- a parameterless call that is a branch target and must remain untouched;
- a parameterless call crossed by a short branch and must remain untouched;
- x86_64 duplicate exclusion;
- a target assembly carrying public-key identity + `StrongNameSigned`;
- preservation of that target's full name/public key/public-key token while clearing only the prepared copy's stale `StrongNameSigned` bit;
- an unchanged consumer `AssemblyRef` that still matches the preserved target full identity;
- source/live-install SHA-1 preservation;
- generated-output structural validation;
- a no-valid-target case that must stop at Gate B without creating prepared output.
