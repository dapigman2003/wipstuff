# Step 32.0.3 — Exact-Length IL Patch + Fast Codemagic Preflight

Version: `0.0.118 (118)`

## Trigger

Physical 0.0.117 again passed Gate A with the exact receipt-backed source, OfflineReady 428/428, all ten Step-31 `PrepareMethod` sites, zero Cecil read-time resolution, no CLR admission, and an unchanged trusted install. Gate B then correctly failed closed **before writing** because whole-module Cecil serialization required another unrelated external Constant-table scope: `Sentry, Version=5.0.0.0`.

This proves that extending the writer resolver one dependency at a time would turn a ten-instruction rewrite into broad unrelated metadata-resolution work. The 6+4 semantic rewrite itself remains untested, not disproven.

Raw evidence: `docs/history/reports/STEP-32.0.2-PHYSICAL-UNEXPECTED-CONSTANT-SCOPE-FAILURE.txt`.

## Step-32 writer decision

Step 32.0.3 stops using `ModuleDefinition.Write` for this candidate. Mono.Cecil remains the **binding and verification authority** for the exact receipt-backed assembly, method token/body fingerprint, ten call offsets/signatures, branch topology, and reopened transformed semantics. The actual write is an exact-length PE/IL patch on the launcher-private clone only.

Every selected direct `call` is exactly five bytes (`0x28` + 4-byte metadata token). Gate B must verify both the opcode and token bytes at each physical file offset derived from the selected method RVA before changing anything.

Replacement bytes are exactly five bytes too:

- 6 × `PrepareMethod(handle)` → `Pop, Nop, Nop, Nop, Nop` (`26 00 00 00 00`);
- 4 × `PrepareMethod(handle, instantiation[])` → `Pop, Pop, Nop, Nop, Nop` (`26 26 00 00 00`).

The padding Nops intentionally keep every later IL byte offset, branch displacement, exception-handler boundary, metadata table, RVA, section layout, and file length unchanged.

## Fail-closed write invariants

Gate B must:

1. bind the exact Step-31 physical source/method/sites with deferred Cecil and the rejecting resolver;
2. map the verified method RVA to its PE section and method-body code start without external resolution;
3. prove each site is the expected 5-byte direct `call` and that its raw metadata token equals Cecil's bound target token;
4. require all ten 5-byte windows to be disjoint and inside the selected method code range;
5. apply replacements only to a byte-for-byte launcher-private copy;
6. compare source/transformed images byte-for-byte and reject any difference outside the fifty approved bytes;
7. require transformed length to equal source length exactly;
8. perform **no Cecil serialization and no dependency resolution**.

Gate C reopens the result under the rejecting resolver and verifies 10→0 PrepareMethod references, the exact padded Pop/Nop shape at the original offsets, the predeclared semantic fingerprint, constant metadata equality, unchanged assembly identity/MVID/EH topology, and the same approved byte-diff proof.

## Codemagic minute policy

The free-tier workflow is split to avoid repeating expensive work unnecessarily:

- `step32-fast`: pinned SDK + canonical static validation + the **complete host regression suite** only. No iOS workload, Godot publish, or IPA.
- `ios-step-32`: run only after `step32-fast` passes on the **exact same commit**. It repeats static validation as a cheap integrity guard, then installs the pinned iOS workload, publishes and verifies the IPA. It intentionally does not rerun the full host suite.

Both workflows emit phase timings and cache-size telemetry. NuGet restores now use the cached `$HOME/.nuget/packages`; the exact Harmony regression archive and a pristine pinned SDK snapshot are cached. iOS workload install uses `--skip-manifest-update`. The device workflow remains on the free `mac_mini_m2`.

The authority requirement is unchanged: a physical IPA is valid only when the fast workflow PASS and device workflow PASS refer to the same commit.

## Scope unchanged

No real StS2 CLR load/invocation, Harmony/MonoMod runtime patching, Godot/game startup, native loading, or trusted-install mutation is authorized. A Step-32 A–D 4/4 PASS still authorizes only the next separately gated transformed-real-StS2 CLR admission/execution experiment.
