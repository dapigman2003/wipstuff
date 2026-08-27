# Step 27.0.22 — post-publish System.Linq framework preservation

Candidate: `0.0.106 (106)`

## Physical evidence from 0.0.105

Physical 0.0.105 materially advanced the Step-27 boundary. Gate A created the raw-PE normalized `0Harmony` runtime image, the normalized `HarmonySharedState::.cctor` executed and returned, and Gate T entered the first exact public `PatchProcessor.Patch()` call. The call then failed as a managed exception before Harmony could finish constructing its replacement method:

`System.MissingMethodException: Method not found: System.Linq.Enumerable.Union<T>(IEnumerable<T>, IEnumerable<T>)`

The stack is `HarmonyLib.MethodCreator..ctor -> PatchFunctions.UpdateWrapper -> PatchProcessor.Patch`. Gate-O metadata already proves the same MethodCreator path also calls `Enumerable.Select` and `Enumerable.ToDictionary`. This is not evidence that MonoMod's detour backend failed: the call never reached `PatchTools.DetourMethod`.

## Root cause

The real `0Harmony.dll` is a receipt-backed post-publish payload. ILLink cannot analyze its calls while publishing the iOS host. `TrimMode=full` therefore retained the `System.Linq` assembly identity but removed an otherwise-normal public BCL member that no build-time launcher code had proven reachable. This is the same class of issue previously observed for post-publish `System.Collections.Concurrent`, but now at method granularity inside a host-bound framework assembly.

Step 22 proved *binding availability*. 0.0.105 proves that dynamically loaded payloads also require a distinct *callable member preservation* contract.

## Candidate change

0.0.106 adds one evidence-backed `TrimmerRootAssembly` root: `System.Linq`. Rooting the complete framework assembly is intentional. The dynamic payload calls several LINQ operators and Microsoft recommends assembly roots when code is dynamically used and cannot be statically described reliably. Chasing only `Union<T>` would leave the same trimming failure class open for the immediately adjacent `Select`/`ToDictionary` calls.

Gate T also gains a non-Harmony T6a/T6b preflight after normalized shared-state validation and before public `PatchProcessor.Patch()`. It requires the host `System.Linq.Enumerable` public surface to contain the exact non-indexed `Select<TSource,TResult>`, two-sequence `Union<TSource>`, and three-selector `ToDictionary<TSource,TKey,TElement>` shapes audited in the Harmony MethodCreator closure. The preflight invokes none of those operators and does not touch the launcher target.

## Longer-term mod-loader implication

This candidate is deliberately scoped to the exact Harmony patch-engine closure currently under test. It is **not** a claim that `TrimMode=full` is a sufficient final policy for arbitrary third-party mods loaded after publish. Microsoft documents dynamic plugin assembly loading as fundamentally difficult for trim analysis because the trimmer cannot see the code that will arrive later. Before the project enables an open-ended StS2 mod DLL surface, the master plan must revisit the host preservation strategy separately (for example, a broader rooted framework contract or a less aggressive iOS trim mode) rather than relying on an endless sequence of per-mod missing-member fixes.

That future policy question does not block the present Step-27 experiment: the current target is one exact, audited Harmony 2.4.2 patch path, and 0.0.105 gives a concrete missing framework assembly surface to preserve and re-test.

## Decision on Step 28

Do **not** pivot yet. The 0.0.105 failure occurred before replacement generation completed and before `PatchTools.DetourMethod -> DetourFactory.Current.CreateDetour`. The current master-plan stop rule therefore has not been met. A Step-28 ahead-of-load architecture remains the fallback if, after dynamic framework closure is proven, Harmony reaches its actual patch/detour boundary and fails for an iOS execution reason.

`MASTER-PLAN.md` remains byte-for-byte unchanged for 0.0.106.
