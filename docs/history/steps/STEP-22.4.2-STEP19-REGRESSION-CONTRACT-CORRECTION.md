# Step 22.4.2 — Step 19 Regression Contract Correction

## Trigger

The Step 22.4.1 canonical foundation built and ran on the physical iPhone. The user reported that every test passed except Step 19 Gate A.

Inspection showed the current Step 19 Gate A still contained the original pre-Step-20 assertion requiring both:

- `RuntimeFeature.IsDynamicCodeSupported == false`
- `RuntimeFeature.IsDynamicCodeCompiled == false`

## Why the assertion became stale

Step 19.2 originally closed before Step 20 and physically proved expression execution in a runtime where both flags were false.

Step 20 later and intentionally enabled the Mono interpreter using `MtouchInterpreter=-all`. The later physically measured canonical runtime reports `IsDynamicCodeSupported=true` while `IsDynamicCodeCompiled=false`.

Therefore the Step 22.4.1 regression was testing an intermediate runtime characteristic that Step 20 intentionally changed, rather than the durable compatibility capability.

## Correction

Step 22.4.2 keeps the three expression execution probes authoritative and changes the iOS runtime contract to:

- successful `Compile()`, `Compile(false)`, and `Compile(true)` execution;
- `RuntimeFeature.IsDynamicCodeCompiled == false` on iOS;
- `RuntimeFeature.IsDynamicCodeSupported` is diagnostic.

A pure `ExpressionRuntimeCompatibilityPolicy` recognizes:

- the historical pre-Step-20 `false/false` mode;
- the canonical interpreter-enabled `true/false` mode;
- any iOS mode with `IsDynamicCodeCompiled=true` as incompatible.

Host unit tests cover all policy combinations, and canonical static validation rejects reintroduction of the obsolete hard requirement.

## Scope

No Steam, install, Godot, Cecil transform, framework-binding, or real-StS2 loading behavior is changed. `ExpressionInterpreterCompatibility.cs` is the one protected Step 22.2 Core behavior file intentionally revised; the remaining baseline Core behavior files stay byte-for-byte unchanged.
