# Testing

Active candidate: `0.0.171 (171)`, IPA `StS2-Launcher-Step-40.ipa`, workflow `ios-canonical`.

Canonical validation must preserve physical Step-36/37/38/39 closures and the exact Step-39.1 compatibility image. Step 40 must require same-process Step-39 4/4 with the retained real NGame in-tree and rendering stopped; reverify the selected image and inert GameStartupWrapper; audit actual in-tree `_Process`, `_PhysicsProcess`, `_Draw`, `_Input`, `_ShortcutInput`, `_UnhandledInput`, `_UnhandledKeyInput`, and `_GuiInput` methods through in-module base chains with deferred/rejecting Cecil; reject surviving startup/platform/FM0D/Spine/Sentry/Steam forbidden edges or unresolved same-sts2 references; permit exactly one `StartRendering()` call; target a 100 ms pulse; call `StopRendering()` on the first continuation before any other Step-40 work; require observed elapsed <=500 ms and rendering inactive afterward; and re-prove singleton/parent/`_window`/state 2 plus zero initializer-bearing/rejected/native escape.

Host regressions cover Step40 gate ordering, stable ordinals, failure sequencing, summary text, and pinned pulse bounds. Static validation also asserts the one-Start/one-Stop UI shape, no Step40 native-host changes, no GameStartup enabling, and exact physical 0.0.170 provenance. Codemagic remains the first actual C# compiler/AOT/link/package authority for this new source.

Physical sequence: fresh process → Step 15 A-C → Step 35 MODEL-BOOTSTRAP 4/4 → Step 36.0.5 4/4 → Step 37.0.1 4/4 → **skip Step 38** → Step 39.0 4/4 → **same process** Step 40.0 A-D once. Once Gate C is armed, never retry Step40 in-process.

Expected Step40 device evidence on success: Gate B static map written before restart; `J_C_START_RENDERING_CALL`; `J_C_START_RENDERING_RETURNED` with active true; `J_C_STOP_RENDERING_CALL`; `J_C_STOP_RENDERING_RETURNED` with active false and elapsed <=500 ms; Gate C pass with zero initializer/rejected/native delta; Gate D pass; `RUN_STEP40_4OF4`; normal report return with rendering still inactive.
