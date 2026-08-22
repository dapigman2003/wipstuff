# Step 27.0.2 — Measured AccessTools Initializer + Framework Preservation

Physical Step 27.0.1 / `0.0.85 (85)` failed safely at Gate O before AccessTools initialization or patching. The metadata audit disproved the earlier BindingFlags-only assumption and exposed the real receipt-backed `HarmonyLib.AccessTools::.cctor`: 56 instructions, no locals/handlers, with BindingFlags initialization plus Mono/runtime classification via `Type.GetType`, reflected `RuntimeInformation.FrameworkDescription`, an add-handler dictionary, and `ReaderWriterLockSlim` construction.

The preceding physical Step 27.0 / `0.0.84 (84)` remains the furthest execution result at A–Q PASS / Gate R FAIL; `PatchProcessor.Patch()` has still never run on iPhone.

Step 27.0.2 / `0.0.86 (86)` makes two related corrections without changing the intended patch boundary:

- Gate O requires the exact physically measured AccessTools initializer structure: exact fields, 56-instruction/opcode fingerprint, exact string probes, exact call/newobj surface, exact BindingFlags values, and exact cache-field stores.
- Gate O also proves the host can resolve `System.Runtime.InteropServices.RuntimeInformation` by the exact string Harmony uses and can reflect/read `FrameworkDescription` before AccessTools executes.
- The iOS host adds a candidate-only `DynamicDependency` anchor for `RuntimeInformation` public properties, `PropertyInfo` public methods, open `Dictionary<,>` constructors, and `ReaderWriterLockSlim` constructors. This is bounded to the measured AccessTools initializer and does not execute Harmony.
- Gate R remains the sole AccessTools execution boundary via `RuntimeHelpers.RunClassConstructor`. After completion it verifies `all`, `allDeclared`, runtime-detection booleans, `allTypesCached == null`, an empty add-handler cache, an initialized/unheld cache lock, unchanged hashes/context, and zero native/unplanned requests.
- Prefix registration remains Gate S. The first actual `PatchProcessor.Patch()` remains Gate T.

No Harmony fork, downloaded framework implementation, StS2 reflection, game invocation, Godot startup, or native game library is admitted.
