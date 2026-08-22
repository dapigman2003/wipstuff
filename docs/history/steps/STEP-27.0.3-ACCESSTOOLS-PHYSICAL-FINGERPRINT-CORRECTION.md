# Step 27.0.3 — AccessTools Physical Fingerprint Correction

Physical Step 27.0.2 / `0.0.86 (86)` failed safely at Gate O, **14/26**, before `HarmonyLib.AccessTools` initialization, prefix registration, or any patch execution.

The failure refined rather than broadened the boundary. The 0.0.86 metadata policy expected 56 instructions, but the receipt-backed `AccessTools::.cctor` contains **57**. The only newly exposed opcode is one `ldc.i4.1`.

That instruction is structurally meaningful: `AccessTools` performs two `Type.GetType("System.Runtime.InteropServices.RuntimeInformation", bool)` calls. The first passes `true` (`throwOnError=true`); the second passes `false`. Step 27.0.3 therefore does not merely loosen the instruction count. Gate O now requires:

- exactly 57 initializer instructions;
- exactly one `ldc.i4.1` in the measured opcode surface;
- the same exact fields, strings, call/newobj surface, stores, BindingFlags values, zero locals/handlers/PInvoke, and bounded framework-preservation preflight already required by 0.0.86;
- the first `Type.GetType(string,bool)` call to be immediately supplied `true` and the second immediately supplied `false`.

Gate R remains the sole explicit `AccessTools` type-initialization boundary. Gate S remains prefix registration. Gate T remains the first intentional `PatchProcessor.Patch()` call. No StS2 member is reflected, patched, or invoked.

The raw physical report is preserved at `docs/history/reports/STEP-27.0.2-PHYSICAL-GATE-O-REPORT.txt`.

This candidate also fixes a release-presentation regression discovered by the physical tester: the top launcher banner had remained hard-coded to Step 26 / 0.0.83. The banner now has one current-candidate presentation source, reads the displayed version from the built bundle, and is covered by static validation so future candidates cannot silently retain stale step/version/summary text.

## Physical result

Physical `0.0.87 (87)` again stopped safely at Gate O, **14/26**, before AccessTools execution or patching. The 57-instruction/opcode fingerprint matched, but the candidate's semantic attribution of the single `ldc.i4.1` was wrong. Both `RuntimeInformation` `Type.GetType(string,bool)` probes use `false`; the single `ldc.i4.1` instead supplies `LockRecursionPolicy.SupportsRecursion` to `ReaderWriterLockSlim`. The raw report is preserved at `docs/history/reports/STEP-27.0.3-PHYSICAL-GATE-O-REPORT.txt`. Step 27.0.4 corrects only that attribution while leaving Gates R–Z unchanged.
