# Step 19 — Expression Interpreter Compatibility

## Why this is the first behavioral rewrite

Step 18 physically proved that real StS2 managed assemblies can be transformed safely in a closed launcher-private workspace. Step 19 should therefore change one real compatibility behavior, but it should not immediately combine multiple unrelated AOT problems.

The selected boundary is direct `System.Linq.Expressions` compilation. .NET exposes `LambdaExpression.Compile(bool preferInterpretation)` and `Expression<TDelegate>.Compile(bool preferInterpretation)`. Passing `true` requests the interpreted form when available, avoiding the default preference for generated executable code. That makes direct parameterless/literal-false calls a narrow candidate for iOS/no-JIT preparation while preserving the expression's intended behavior.

Step 19 does not assume such calls exist in the game. Real receipt-backed IL evidence is a gate prerequisite.

## Trust and write model

Step 19 creates a **fresh** workspace rather than consuming Step 18 outputs:

```text
Documents/StS2Launcher/Step19-ExpressionInterpreterCompatibility/source
Documents/StS2Launcher/Step19-ExpressionInterpreterCompatibility/prepared
```

`source` is cloned from the Step 12 receipt-backed install using the physically proven ARM64/shared selection rule. Every source copy is SHA-1 verified against the receipt. The live install remains read-only.

All Cecil dependency resolution is restricted to the SHA-1-verified Step 19 `source` tree using the Step 18-proven assembly-identity catalog, exact-first matching, unambiguous same-name/culture/token version unification, immediate SHA-1 recheck before open, and explicit assembly + metadata resolver binding. Generated `prepared` assemblies are never added as resolver inputs.

## Ordered gates

### Gate A — InterpreterCapabilityAndWorkspaceClone

1. In the actual launcher process, construct a captured expression (not a constant-only expression).
2. Execute it through `Compile(preferInterpretation: true)` and require the expected result `42`.
3. Record `RuntimeFeature.IsDynamicCodeSupported` and `RuntimeFeature.IsDynamicCodeCompiled` as diagnostics, but do not use either flag as a substitute for the functional interpreter proof.
4. Re-prove OfflineReady.
5. Read the trusted Step 12 receipt.
6. Select `data_sts2_macos_arm64` plus architecture-neutral managed filename candidates and exclude `data_sts2_macos_x86_64` duplicates.
7. Recreate the Step 19 `source` tree and copy/SHA-1-verify the complete selected scope.

No game assembly is loaded or executed.

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
- parameterless but structurally unsafe for insertion;
- supported but inside a strong-named assembly.

The gate requires at least one safe unsigned real target. If none exists, Step 19 fails deliberately and the next compatibility class should be chosen from real Step 17 evidence rather than broadening the matcher.

### Gate C — PreferInterpretationRewrite

For every selected unsigned target assembly:

1. Recheck the source copy against the receipt immediately before transformation.
2. Open it with `ReadingMode.Immediate` and the explicit source-workspace assembly + metadata resolver.
3. Re-scan and require the Gate B target count and assembly identity to be unchanged.
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

5. Write to a temporary file under `prepared`, then atomically move it into the prepared tree.
6. Reopen the generated assembly with `ReadingMode.Deferred` and a fresh explicit source-workspace resolver.
7. Verify:
   - structural metadata fingerprint is unchanged;
   - total direct Compile-site count is unchanged;
   - no selected safe parameterless/literal-false site remains;
   - already-out-of-scope dynamic/unsafe classes are unchanged;
   - `Compile(true)` count increased by exactly the rewritten-site count;
   - instruction count increased only by the number of parameterless calls that required a new bool push;
   - source SHA-1 remains receipt-identical;
   - prepared bytes differ from source.

#### Parameterless IL insertion safety

Adding `ldc.i4.1` changes code size by one byte. Step 19 therefore refuses parameterless insertion when:

- the call immediately follows an IL prefix;
- the call is a branch or exception-handler boundary/entry point;
- any short branch crosses the insertion position.

The short-branch rule is intentionally conservative. Cecil recalculates offsets when writing, but a short branch still has an 8-bit displacement. Skipping crossing short branches avoids creating a previously-valid method whose short displacement could overflow after insertion.

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
6. Reopen each rewritten prepared assembly with a fresh explicit source-workspace resolver.
7. Re-prove the Gate C structural fingerprint and Compile-site counts.

A 4/4 pass proves a reproducible, isolated, behaviorally meaningful prepared payload—not game execution.

## Deliberate non-goals

Step 19 does not:

- rewrite dynamic/non-literal `Compile(bool)` decisions;
- rewrite strong-named assemblies without a signing strategy;
- modify control-flow-sensitive parameterless call sites merely to increase coverage;
- implement general Reflection.Emit substitution;
- implement Harmony/MonoMod detours;
- load prepared StS2 assemblies with `Assembly.Load`;
- execute StS2;
- integrate game FMOD/Spine binaries;
- add Cloud or Workshop.

## Host regression coverage

The Step 19 host fixture exercises:

- both `LambdaExpression` and generic `Expression<TDelegate>` parameterless calls;
- three literal-false encodings (`ldc.i4.0`, `ldc.i4.s`, `ldc.i4`);
- an already-true call;
- a dynamic bool call that must remain untouched;
- a parameterless call that is a branch target and must remain untouched;
- a parameterless call crossed by a short branch and must remain untouched;
- x86_64 duplicate exclusion;
- source/live-install SHA-1 preservation;
- generated-output structural validation.
