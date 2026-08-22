# Step 27.0.4 — AccessTools Operand Attribution Correction

Physical Step 27.0.3 / `0.0.87 (87)` failed safely at Gate O, **14/26**, before `HarmonyLib.AccessTools` initialization, prefix registration, or patch execution.

The broad fingerprint is now stable: 57 instructions, the expected runtime-detection strings/calls/stores, and exactly one `ldc.i4.1`. The remaining failure showed that Step 27.0.3 attributed that `1` to the wrong operation.

The physical IL control flow proves:

- first `Type.GetType("System.Runtime.InteropServices.RuntimeInformation", bool)` uses `false`; its null path falls back to `!IsMonoRuntime`;
- second `Type.GetType("System.Runtime.InteropServices.RuntimeInformation", bool)` also uses `false`; its null path falls back to `false`;
- the sole `ldc.i4.1` supplies `LockRecursionPolicy.SupportsRecursion` to `ReaderWriterLockSlim(LockRecursionPolicy)`.

Step 27.0.4 / `0.0.88 (88)` therefore keeps the exact 57-instruction policy but changes the semantic checks to **false / false** for the two `Type.GetType` operands and **SupportsRecursion (1)** for the lock constructor. The bounded trimming-preservation anchor is unchanged.

Gate R remains the sole explicit AccessTools type-initialization boundary. Gate S remains prefix registration. Gate T remains the first intentional `PatchProcessor.Patch()` call. No StS2 member is reflected, patched, or invoked.

The top-of-app release presentation introduced in Step 27.0.3 is retained and advanced to Step 27.0.4 / bundle-derived 0.0.88.
