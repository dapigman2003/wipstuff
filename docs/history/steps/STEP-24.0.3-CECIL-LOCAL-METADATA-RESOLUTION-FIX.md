# Step 24.0.3 — Cecil Local-Metadata Resolution Fix

## Trigger

Step 24.0.2 / `0.0.75 (75)` reached a physical iPhone and failed safely at **Gate A — InitializationPreflight**, stage `prepared target classification`:

`AssemblyResolutionException: Failed to resolve assembly: 'GodotSharp, Version=4.5.1.0, Culture=neutral, PublicKeyToken=null'`

The Step 24 report recorded **0/4** and no Gate B/C/D execution. Therefore no Step 24 real Harmony load or module initialization occurred; physical Step 23.4.3 remains the latest closed runtime boundary.

## Root cause

Gate A is intended to be metadata-only and self-contained. Its bounded automatic-initialization traversal classified external calls by AssemblyRef scope, but for nominally same-assembly calls it also used Cecil `MethodReference.Resolve()` to recover the target MethodDef.

That API can walk external type/base/member metadata as part of method resolution. On the real prepared `0Harmony` graph, this caused Cecil to request `GodotSharp 4.5.1.0` during classification. The failure therefore did not establish that Harmony's initializer actually executes Godot code; it established that the audit implementation itself depended on external Cecil resolution.

Adding `GodotSharp` to Cecil's resolver would be the wrong correction because it would broaden the metadata environment and could hide the distinction between a genuine reachable external execution edge and a resolver implementation detail.

## Correction

Step 24.0.3 removes external Cecil assembly resolution from same-assembly initializer traversal.

For a call whose scope names the audited assembly, Gate A now resolves only from metadata already loaded in that module:

1. accept an operand that is already the exact local `MethodDefinition`;
2. accept an element method that is already a local `MethodDefinition`;
3. recover a local MethodDef directly from the current module token when possible;
4. otherwise match the declaring local type and method metadata deterministically;
5. if the local reference cannot be matched unambiguously, record `Unresolved same-assembly call (local metadata only)` and fail closed.

No external assembly is resolved merely to complete the same-assembly call graph.

External calls are still classified by their declared scope. `GodotSharp` remains a non-framework external edge and is **not allowed** by this correction. If the real initializer actually reaches a GodotSharp method, Gate A should now stop with an explicit `Prohibited API reachable ... [GodotSharp]` hazard instead of an `AssemblyResolutionException`.

Hazard failures also include the audited automatic-initialization IL so the next candidate can be designed directly from physical evidence.

## Protected behavior

No change to the physically proven Step 23.4.3 implementation. No change to Step 24 gate ordering, exact `0Harmony 2.4.2.0` target, strict private resolver, native-load refusal, `RuntimeHelpers.RunModuleConstructor` completion barrier, trusted/prepared bytes, or the prohibition on Harmony patch APIs, game invocation, Godot startup, and native game-library loading.

The Master Plan is unchanged because this is a candidate-level metadata-audit correction, not a durable architecture or roadmap change.

## Candidate

- Step: **24.0.3**
- version: **0.0.76 (76)**
- workflow: **`ios-step-24`**
- expected IPA: **`artifacts/StS2-Launcher-Step-24.ipa`**

## Authority

Codemagic must first compile the corrected audit, pass the full host suite, and verify the IPA. The next physical run then determines whether the real Harmony initializer is wholly inside the measured managed/framework boundary or contains a genuine external edge that needs its own gated step.
