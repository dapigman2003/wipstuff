# Documentation status

Current candidate: **Step 35.0.31 / Step 36.0 — 0.0.154 (154)**.

Physical 0.0.153 proves the exact Step-35 Gate-D PASS result also returns through the outer worker at `D_WORKER_RETURN`. The remaining defect is the captured UIKit continuation itself; 0.0.154 bypasses it with `ConfigureAwait(false)` and explicit main-thread finalization without changing core Step-35 behavior.

Step 36.0 is the first post-Step-35 initialization boundary. It requires the same-process durable exact Step-35 core closure and invokes only exact transformed `ExecuteEssential()` once; it no longer depends on the historical stalled UI gate snapshot. `ExecuteDeferred`, `PrewarmJit`, game entry, Harmony/MonoMod runtime patching, arbitrary resolver fallback, and native game loading remain later boundaries.
