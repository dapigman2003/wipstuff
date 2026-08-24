# StS2 Launcher iOS — Step 27 Controlled Launcher-Owned Harmony Patch + Unpatch

Steps 01–26 are physically closed. Step 27 remains focused on proving one launcher-owned Harmony patch/unpatch boundary on iOS before any StS2 member is reflected or modified.

## Active candidate

**Step 27.0.21 / `0.0.105 (105)` — raw HarmonySharedState method-body normalization**

Codemagic 0.0.104 compiled production and tests and executed all **212** host tests. **211 passed / 1 failed**. This time the failure was inside the real production normalizer: the exact hash-pinned official Harmony-Fat 2.4.2 net9.0 surrogate entered `CreateIosNormalizedHarmonyRuntimeImage`, the Deferred Cecil read succeeded, and `Mono.Cecil.ModuleDefinition.Write` failed while rebuilding unrelated enum-typed Constant metadata because the fail-closed resolver correctly refused `System.Reflection.BindingFlags`.

That exposed the remaining design flaw: even Deferred Cecil cannot round-trip the full assembly without resolving enum definitions during metadata serialization. 0.0.105 removes that dependency rather than adding a framework-enum whitelist.

Gate A now:

- keeps the exact original 0Harmony 2.4.2 patch-engine fingerprint and rejecting metadata resolver;
- uses Deferred Cecil **read-only** for identity, field/cctor shape, and existing metadata-token discovery;
- reuses the original cctor's already-existing parameterless `Dictionary<...>` constructor MemberRef tokens and exact HarmonySharedState FieldDef tokens;
- maps the existing cctor RVA to its PE file offset with `PEReader.PEHeaders`;
- requires a fat ECMA-335 header, no exception/extra sections, and sufficient existing body capacity;
- clones the prepared bytes and replaces **only that existing method-body slot** with a 12-byte fat header plus the exact 47-byte / 11-instruction direct-state IL;
- verifies no byte outside the original cctor slot changed;
- reopens the in-memory result with Deferred Cecil and requires the exact 11-instruction fingerprint.

No metadata table, heap/blob, AssemblyRef, MemberRef, TypeSpec, Constant, CustomAttribute, resource, signature, or unrelated method is rebuilt or moved. The source/live/prepared Harmony files remain immutable.

The official Harmony-Fat host regression remains content-addressed by the already-observed release archive and selected net9.0 DLL SHA-256 values. It now serves its intended purpose: exercising the production normalizer against a real merged Harmony 2.4.2 image before IPA publication.

The full 0.0.104 Codemagic 211/212 report and Cecil-writer stack are preserved in project history.

## iOS detour decision rule

The stop rule remains unchanged: reach T6 with the normalized cctor; if public `PatchProcessor.Patch()` works, continue Harmony. If T6 passes but T7/T8 fails, perform one representative patch/unpatch on a launcher-owned post-publish interpreted fixture. If that also fails, stop iterating Harmony internals and propose deterministic ahead-of-load transforms on derived runtime copies; that would be a master-plan-level architecture change.

## Build

Workflow: `ios-step-27`

Expected app version: `0.0.105 (105)`

Expected IPA: `artifacts/StS2-Launcher-Step-27.ipa`

Codemagic must pass the hash-pinned official Harmony 2.4.2 normalizer regression and the complete host suite before publish. Physical acceptance remains A–Z **26/26**, then OfflineReady PASS and Foundation 5/5.
