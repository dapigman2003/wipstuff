# Documentation status

Current candidate: **Step 35.0.30 / Step 36.0 — 0.0.153 (153)**.

Physical 0.0.152 proves the exact Step-35 core authority path through a constructed Gate-D PASS result. The remaining 0.0.152 defect is the UIKit continuation after `D_TASK_RETURN_START`; 0.0.153 fixes that return path without changing core Step-35 behavior.

Step 36.0 is the first post-Step-35 initialization boundary. It requires a clean same-process Step-35 EXACT-CLOSURE 4/4 and invokes only exact transformed `ExecuteEssential()` once. `ExecuteDeferred`, `PrewarmJit`, game entry, Harmony/MonoMod runtime patching, arbitrary resolver fallback, and native game loading remain later boundaries.
