# Step 36.0 — Controlled exact ExecuteEssential

Release: 0.0.153 (153)

## Authority

Physical 0.0.152 establishes positive exact Step-35 core closure: exact transformed sts2 + exact prepared GodotSharp, the complete source-built Godot 4.5.1 bidirectional bridge, exact ExecuteVeryEarly RanToCompletion, and final isolation result constructed as PASS. 0.0.153 fixes only the UIKit Gate-D return plumbing before exposing this new boundary.

The next game initialization method is pinned from the exact source assembly:

- type: `MegaCrit.Sts2.Core.Helpers.OneTimeInitialization`
- method: `ExecuteEssential`
- source token: `0x06007D03`
- signature: static, parameterless, `System.Void`
- required pre-state after ExecuteVeryEarly: `1`
- required post-state after successful ExecuteEssential: `2`

The shipped XML documentation describes ExecuteEssential as initialization needed before the main menu can display, including the UI atlas/core systems.

## Ordered gates

**A — Exact Step-35 closure + static preflight.** Require same-process exact Step-35 core closure and unchanged resolver baseline. Re-open exact source and exact transformed images with rejecting Cecil resolvers. Require source token `0x06007D03`, exact static/parameterless/void managed-IL signature, source/transformed semantic fingerprint equality, zero direct ExecuteVeryEarly/ExecuteDeferred/PrewarmJit crossover, zero direct Harmony references, and zero Cecil dependency resolution. Emit a run-correlated Step36 ExecuteEssential static IL/callsite map.

**B — Exact authority continuity + binding.** Require the exact closed transformed sts2 assembly and exact Godot bridge/context from Step 35 to remain resident and unchanged. Bind transformed ExecuteEssential by exact type/signature, require its transformed token/MVID from Gate A, require no Step-35 diagnostic bridge in the authority image, and require `OneTimeInitialization._state == 1` before invocation.

**C — Single exact ExecuteEssential invocation.** Invoke exact transformed ExecuteEssential once on the main thread. Require normal return, state transition `1 -> 2`, and zero initializer-bearing, rejected-managed, or native resolver escape. No retry is permitted in the same process once invocation begins.

**D — Final isolation audit.** Re-prove OfflineReady, original/transformed hashes, runtime-binding plan, every resident initializer-free private dependency, exact sts2 context ownership, clean resolver/native counters, exact ExecuteEssential token, and final state `2`.

## Explicitly forbidden

Step 36.0 does not invoke `ExecuteDeferred`, `PrewarmJit`, the game entry point, Harmony/MonoMod APIs, or the game native executable. It does not permit arbitrary managed/native resolver fallback and does not fabricate Godot runtime ownership.

## Expected device sequence

Fresh app -> Step 15 Gates A-C -> Step 35 EXACT-CLOSURE 4/4 -> without force-quitting/backgrounding -> Step 36.0 A-D once.
