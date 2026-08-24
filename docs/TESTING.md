# Testing — Step 27 Controlled Launcher-Owned Harmony Patch + Unpatch

## Static validation

Run `bash scripts/validate.sh`.

For Step 27.0.24 / `0.0.108 (108)`, validation retains the proven copy/no-link host policy, raw-body HarmonySharedState normalizer, exact public patch API surface, and single `PatchProcessor.Patch()` call. It additionally proves the final stop-rule fixture is genuinely outside the iOS build-time AOT graph.

Required interpreted-fixture invariants:

- `fixtures/StS2Launcher.Step27.InterpretedPatchFixture` exists as a standalone net9.0 project;
- neither the iOS project nor the host-test project contains a ProjectReference/Content/BundleResource reference to it;
- `scripts/build-ios.sh` builds the fixture before publish if needed but copies it into `Step27InterpretedPatchFixture/` only **after** `dotnet publish` returns;
- `scripts/verify-ipa.sh` requires exactly one byte-identical fixture DLL in that data-only directory plus a valid SHA-256 manifest;
- the fixture is IL-only, unsigned, framework-only, has no module initializer, and exposes only the deterministic Target/InvokeTarget/Prefix/reset/counter surface required by the experiment;
- `InvokeTarget` retains a direct IL `call` to `Target`;
- Gate P loads the exact fixture bytes into the private context and creates a fresh processor through public `Harmony.CreateProcessor(MethodBase)` for the exact interpreted Target;
- production Step 27 no longer references the old build-time `HarmonyPatchProbe` path;
- Gate T still contains exactly one `PatchProcessor.Patch()` reflection invocation; Gate W contains exactly one exact `Unpatch(MethodInfo)` invocation if patching succeeds;
- no `MONOMOD_DMD_TYPE`, forced Cecil/Emit backend, or other MonoMod backend override is introduced;
- no StS2 member is reflected, patched, or invoked.

The raw-body normalizer remains bounded to the canonical Harmony 2.4.2 target and changes only the admitted `HarmonySharedState::.cctor` slot in the in-memory image. The real upstream Harmony-Fat host surrogate remains content-addressed by the existing archive/DLL SHA-256 pins.

Physical 0.0.107 must be archived as evidence that copy/no-link removed the two known trim blockers and that the new failure is `System.NotImplementedException` surfaced from `PatchFunctions.UpdateWrapper` during the real public Patch() call.

## Host tests

Run `bash scripts/test.sh`.

In addition to the existing full suite and official Harmony normalizer regression, the script builds the Step-27 interpreted fixture separately, copies it into a host artifact directory, exports only its path through `STS2_STEP27_INTERPRETED_PATCH_FIXTURE`, and then runs the tests. The test project must not reference the fixture project.

The fixture regression uses Cecil Deferred/read-only metadata to verify the exact type/method/field surface and direct `InvokeTarget -> Target` IL call, then loads the DLL from bytes in a collectible host ALC to prove baseline Target/InvokeTarget behavior and Prefix result semantics without creating an iOS project dependency.

## Codemagic / physical run

Codemagic must pass static validation, the complete host suite, iOS publish, and IPA verification. Then install `0.0.108 (108)` from a fresh process.

The decisive outcomes are:

- **PASS:** Patch() returns for the interpreted target; both patched routes return 1041 with the original body skipped; exact unpatch restores 42 on both routes. Harmony remains viable for the representative post-publish managed model.
- **FAIL:** the interpreted fixture cannot patch. Preserve the full Step-27 report/crash checkpoint and stop Harmony-internal iteration. The next major step is Step 28 ahead-of-load managed IL transformation with a master-plan architecture revision.
