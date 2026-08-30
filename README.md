# StS2 Launcher iOS — Step 35.0.7 Generic Delegate MemberRef Fix + In-Method Localization

Active candidate: **Step 35.0.7 / `0.0.130 (130)`**.

Physical 0.0.129 proved the deferred Cecil writer correction: Gate A emitted and verified the diagnostic clone, Gate B admitted it with zero primary-resolution activity, and Gate C armed the bridge and entered the one diagnostic `ExecuteVeryEarly()` invocation. The run then failed normally with `MissingMethodException: Method not found: void System.Action`1.Invoke(string)` before any `INMETHOD_*` marker.

The defect is in the diagnostic bridge's synthetic generic MemberRef, not in the exact game compatibility boundary. 0.0.129 encoded the call as `Action<string>::Invoke(string)`. Step 35.0.7 models open `Action<T>` explicitly and serializes the MemberRef as `Action<string>::Invoke(!0)`, then reopens the clone under rejecting resolution and verifies that exact generic-variable signature before Gate B.

All 0.0.129 protections remain: file-backed deferred Cecil open; bounded in-memory `System.Runtime` + `Sentry` constant-metadata writer surrogates only; exact transformed source unchanged and re-hashed; separate identity/MVID-preserving diagnostic clone; strict runtime resolver; initializer-bearing/native/unplanned rejection; one reflected diagnostic invocation; 60-second Task bound; no broader startup/Harmony/Godot execution.

The separate diagnostic clone carries output-only `INMETHOD_*` entry markers in `ExecuteVeryEarly.MoveNext`, the selected pre-first-await game methods, and relevant managed-IL type initializers. Gate C arms a launcher-owned `Action<string>` immediately before the one reflected diagnostic invocation.

**Authority:** a 0.0.130 A–D 4/4 result is diagnostic completion only. It cannot close exact Step 35 because Gate B/C execute an instrumented derivative rather than the exact closed transformed bytes. Step 35 remains OPEN until a separately defined authoritative transformed artifact passes its physical closure contract.
