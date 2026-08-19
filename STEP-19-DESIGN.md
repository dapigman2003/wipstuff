# Step 19.2 — Expression Interpreter Compatibility design

Step 19.2 is an evidence-driven AOT compatibility decision around `System.Linq.Expressions` `Compile` behavior.

## Why the design changed

Step 19.1 physically failed while Cecil attempted to write the copied macOS `System.Linq.Expressions.dll`:

```text
NotSupportedException: Writing mixed-mode assemblies is not supported
```

The failure revealed two separate facts:

1. the observed direct `Compile()` sites were inside a desktop framework implementation assembly, not automatically game/application consumer sites; and
2. that framework image is not a safe Cecil write target because its image is non-IL-only/ReadyToRun-or-mixed-mode.

Step 19.2 does not flip `ILOnly`, strip native/ReadyToRun data, or reconstruct a desktop framework assembly.

The host-runtime side is more important: modern `LambdaExpression.Compile()` checks whether IL compilation is available and falls back to the expression interpreter when dynamic code is unsupported. `Compile(false)` ultimately reaches that same path when IL compilation is unavailable. Therefore the strongest Step 19 proof is physical host execution, not a speculative game/framework rewrite.

## Gate A — InterpreterCapabilityAndWorkspaceClone

On the physical iOS process, independently create equivalent captured expression trees and execute delegates produced by:

- `Compile()`;
- `Compile(preferInterpretation: false)`;
- `Compile(preferInterpretation: true)`.

Each must return `42`.

Record:

- `RuntimeFeature.IsDynamicCodeSupported`;
- `RuntimeFeature.IsDynamicCodeCompiled`;
- host `System.Linq.Expressions` full assembly identity.

On iOS specifically, Step 19.2 requires both dynamic-code flags to be false. That makes success of `Compile()` and `Compile(false)` direct evidence that the host runtime supplies a no-dynamic-code fallback rather than a hidden JIT path.

Then re-prove OfflineReady and create a fresh receipt-backed ARM64/shared source workspace exactly as in the protected Step 18 boundary.

## Gate B — RealCompileTargetDiscovery

Read-only scan actual IL call/callvirt sites for:

- `LambdaExpression.Compile()`;
- `LambdaExpression.Compile(bool)`;
- `Expression<TDelegate>.Compile()`;
- `Expression<TDelegate>.Compile(bool)`.

Classify, but do not mutate:

- parameterless safe/unsafe under the old insertion design;
- literal `false`, literal `true`, dynamic/nonliteral bool;
- `System.*` framework implementation versus non-framework consumer;
- IL-only versus non-IL-only/ReadyToRun/mixed-mode;
- strong-name identity;
- primary `sts2.dll` call-site count.

Gate B always selects zero Cecil mutation targets. The copied desktop framework payload is diagnostic input only; its presence is not iOS framework-execution proof.

## Gate C — zero-write prepared payload

Recreate the complete `prepared` tree by byte copy only.

For every file:

```text
receipt SHA-1 == Step 19 source SHA-1 == Step 19 prepared SHA-1
```

No `ModuleDefinition.Write` occurs in the Step 19.2 production compatibility implementation. Strong-name bits, public keys/tokens, metadata, IL, native/ReadyToRun sections, and all other bytes remain unchanged.

## Gate D — IsolationAudit

Independently verify:

- exact expected source file set;
- exact expected prepared file set;
- every source SHA-1 against receipt;
- every live-install SHA-1 against receipt;
- every prepared SHA-1 against receipt/source;
- zero mutation records;
- zero Cecil assembly writes.

A 4/4 pass establishes:

```text
HOST RUNTIME FALLBACK — NO GAME/APPLICATION IL REWRITE REQUIRED
```

It does not establish game execution or framework reference binding. Those remain later subsystems.
