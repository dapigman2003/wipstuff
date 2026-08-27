# Step 27.0.19 — net9 surrogate reference-graph assertion fix

Candidate: `0.0.103 (103)`

Codemagic 0.0.102 proved the official Harmony-Fat fixture pipeline now works end-to-end through test execution: the release archive downloaded, `net9.0/0Harmony.dll` was selected, production and tests compiled, and all 212 host tests ran. 211 passed.

The single failure was confined to the new surrogate regression. It assumed that a net9 implementation could not retain a `netstandard` AssemblyRef. The official net9 Harmony binary disproved that assumption. The assertion failed before `CreateIosNormalizedHarmonyRuntimeImage` was invoked, so it is not evidence against production Deferred-Cecil normalization.

0.0.103 removes the invalid negative reference-graph inference. `scripts/test.sh` exports the exact selected release-archive member, and the test requires that member to be the root `net9.0/0Harmony.dll` or a wrapped equivalent. The existing positive `System.Runtime, Version=9.0.0.0` check remains. Production `ControlledHarmonyPatchExecution.cs`, the exact 11-instruction normalized `HarmonySharedState` cctor, T5/T6/T7 ordering, and all on-device admission rules are unchanged.

The purpose is to let the real upstream surrogate finally enter the production normalizer during Codemagic instead of failing on an unrelated packaging/reference assumption.
