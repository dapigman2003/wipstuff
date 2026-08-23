# Step 27.0.20 — hash-pinned real-Harmony normalizer execution

Candidate: `0.0.104 (104)`

Codemagic 0.0.103 compiled production and tests and executed all 212 host tests at 211/212. The sole failure was `OfficialHarmony242Net9FatNormalizerUsesDeferredMetadataAndPreservesSourceBytes`, which threw `System.InvalidOperationException: Sequence contains more than one matching element` at the test-only `System.Runtime` `SingleOrDefault` lookup. The failure happened before `CreateIosNormalizedHarmonyRuntimeImage` was invoked.

The repeated CI issue was architectural in the regression itself: a dependency-merged assembly is not a stable place to infer target framework or provenance from uniqueness/absence/version assumptions about AssemblyRef rows. 0.0.104 removes that category of inference.

Codemagic 0.0.103 supplied stable content identities for the exact official inputs:

- `Harmony-Fat.2.4.2.0.zip` SHA-256: `a5fc5f9d9640b927d786a0527faa18bf7aa776788235140c59e9b73de87a7774`
- selected member: `net9.0/0Harmony.dll`
- extracted DLL SHA-256: `a849b726e1f9248d71aabbed8114deaf79beb7acc25e8344ff92a27ad8ac87ab`

0.0.104 pins both hashes in the canonical host script, independently re-hashes the DLL in C#, verifies only exact 0Harmony 2.4.2 identity plus the EditorBrowsable metadata surface, and then immediately invokes the unchanged production Deferred-Cecil normalizer. Production on-device admission remains governed by the exact prepared StS2 Harmony fingerprint.

`ControlledHarmonyPatchExecution.cs` and the master plan are unchanged.
