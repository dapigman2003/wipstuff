# Current Status — Step 35.0.7 Generic Delegate MemberRef Fix + In-Method Localization

## Active candidate — Step 35.0.7 / 0.0.130 (130)

Steps 01–26 are closed; Step 27 is CLOSED NEGATIVE; Step 28 is CLOSED POSITIVE 5/5; Steps 29–34 are CLOSED POSITIVE 4/4. **Step 35 remains OPEN.**

Physical 0.0.126 remains the authoritative exact Step-35 runtime frontier. The exact source target remains `ExecuteVeryEarly` token `0x06007D02`, with async `<ExecuteVeryEarly>d__7::MoveNext` source token `0x0600BC71`. Its same-run artifacts proved Gate A/B PASS, exact transformed `ExecuteVeryEarly()` binding, entry into the single `MethodInfo.Invoke`, planned `GodotSharp`/`Steamworks.NET` and host-framework resolution, and a final durable `System.Collections.Concurrent 8.0.0.0 -> host 9.0.0.0` event with no `C_INVOKE_RETURNED`. The exact static map places the unresolved hard-kill region in initial synchronous `<ExecuteVeryEarly>d__7.MoveNext` work before the first incomplete await.

Physical 0.0.127 and 0.0.128 did not reach game execution. Both failed normally in Gate A on `System.Runtime 9.0.0.0` while creating the diagnostic clone. Analysis against the exact trusted `sts2.dll` localized 0.0.128 to Cecil `ReadingMode.Immediate` occurring before the bounded writer resolver was configured.

Physical 0.0.129 **fixed that writer problem**. Same-run Run ID `20260830T1420370609570Z-pid1112-2319942e61974a08a0ff786c712ec456` proved:

- Gate A PASS, including diagnostic-clone emission/reopen verification and same-run static-map write;
- Gate B PASS, including exact-source re-hash, diagnostic-clone hash `62583636d00f3169dda3b61686c39b0df875e4f2903d982b0f42492bdd5bd70b`, `LoadFromStream`, identity/MVID/context ownership and zero-resolution admission;
- Gate C bound the diagnostic clone's exact target contract, armed the bridge field as exact `System.Action<string>`, and entered the one reflected invocation;
- planned `GodotSharp` and `Steamworks.NET` private loads and planned host framework resolutions succeeded;
- instead of a native hard kill, managed control returned with `MissingMethodException: Method not found: void System.Action\`1.Invoke(string)`;
- no `INMETHOD_*` marker was emitted, so the failure occurred in the diagnostic bridge callback before the first selected game-method entry marker could be durably recorded;
- the run ended normally and Gate D did not run.

This is a **diagnostic instrumentation defect**, not new evidence that the exact game path itself changed. The 0.0.126 exact runtime frontier therefore remains authoritative.

## Root cause and 0.0.130 correction

The 0.0.129 bridge created a constructed declaring type `System.Action<string>` and then synthesized its `Invoke` MemberRef with a concrete `System.String` parameter. That is not the metadata signature of the generic declaring type's method. Under ECMA-335, the MemberRef on the constructed generic declaring type must retain the declaring type's generic variable: `Action<string>::Invoke(!0)`, where `!0` is supplied as `string` by the constructed declaring type. The physical iOS runtime rejected the incorrectly synthesized `Invoke(string)` reference with a managed `MissingMethodException`.

0.0.130 changes only this diagnostic bridge metadata encoding:

1. model open `System.Action<T>` with one Cecil type generic parameter;
2. construct the bridge field as `System.Action<string>`;
3. encode `Invoke` with parameter `!0` owned by the open declaring type, not concrete `string`;
4. after serialization, reopen under the rejecting resolver and require the bridge callvirt operand to be exactly a `System.Action<string>` declaring type with one `Invoke(!0)` type-generic parameter at position 0;
5. retain all 0.0.129 deferred-open writer, exact-source, marker, strict runtime resolver, timeout and later-boundary prohibitions unchanged.

Gate B/C remain diagnostic derivative execution only. `ExecuteEssential`, `ExecuteDeferred`, launcher-driven `PrewarmJit`, entry-point execution, Harmony/MonoMod patching, initializer-bearing `0Harmony`, arbitrary managed fallback, native game loading and Godot/game startup remain forbidden. Cancellation remains INCONCLUSIVE and Step 35 requires a fresh process.

A successful 0.0.130 A–D 4/4 result is **Step 35.0.7 diagnostic localization complete — NOT Step 35 closure**. The desired next evidence remains the last durable `INMETHOD_*` marker before any hard termination or managed failure.
