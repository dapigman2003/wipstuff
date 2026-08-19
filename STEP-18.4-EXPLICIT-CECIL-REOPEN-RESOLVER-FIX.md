# Step 18.4 — Explicit Cecil Reopen Resolver Fix

## Physical evidence that triggered this correction

The Step 18.3.1 / runtime `0.0.50 (50)` build reached the physical iPhone. Gate A passed with the receipt-backed 185-file macOS-arm64 workspace intact. Gate B then failed with:

```text
AssemblyResolutionException: Failed to resolve assembly:
GodotSharp, Version=4.5.1.0, Culture=neutral, PublicKeyToken=null
```

This followed the prior Step 18.2 run, where the custom workspace identity resolver had already advanced past `GodotSharp` and exposed `System.Runtime 8.0.0.0` versus workspace `9.0.0.0`. Step 18.3 added a narrow exact-first, unambiguous version-only workspace rule for that writer boundary.

## Root cause found in the A–D implementation

The Step 18.3 Gate B source path was correctly opened with both Cecil resolver layers bound to `WorkspaceOnlyAssemblyResolver`, and the writer ran on that bound module. After the write, however, Gate B reopened the generated output through a no-resolver `ReadModuleImmediate(outputPath)` helper. Cecil was therefore free to lazily use its implicit default assembly resolver during verification metadata materialization.

The same no-resolver generated-output reopen existed in:

- Gate C rewritten-output verification;
- Gate D round-trip output audit reopen;
- Gate D rewritten output audit/NOP reopen.

That made the subsystem trust policy asymmetric: production writes were workspace-confined, while output verification could escape it.

## Step 18.4 correction

Step 18.4 removes the no-resolver real-game read helper. Every real StS2 source or generated output is now opened through one helper that explicitly supplies:

```text
AssemblyResolver = verified Step 18 workspace identity resolver
MetadataResolver = MetadataResolver(the same workspace resolver)
```

Source rewrite reads retain `ReadingMode.Immediate`. Generated-output verification reads use `ReadingMode.Deferred`, and resolved dependency modules are also opened deferred. This reduces unrelated metadata materialization while keeping any genuinely required resolution inside the same receipt/SHA-1-verified source workspace.

Generated outputs are **not** added to the dependency trust catalog. The only dependency source remains the Gate A `source` tree verified against the Step 12 receipt.

## Diagnostic hardening

Gate B, C and D now include an exact failure stage, such as:

```text
Stage: primary Cecil writer
Stage: round-trip output Cecil reopen
Stage: rewritten output NOP proof
Stage: isolation audit rewritten Cecil reopen/NOP proof
```

This makes a future failure actionable without inferring whether it happened during write or verification.

## Version / workflow

```text
Step 18.4
0.0.51 (51)
ios-step-18-4
```

The trust boundary remains unchanged: no live-install writes, no runtime/system/GAC/TPA/network fallback, no `Assembly.Load`, and no StS2 execution.
