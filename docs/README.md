# StS2 Launcher — Step 40.0

Active candidate: **0.0.171 (171)** — first controlled render-loop resumption against the physically admitted real StS2 hierarchy.

Physical **0.0.170 / Step 39.2** is closed positive 4/4. The real NGame hierarchy is physically proven in the live SceneTree with exact singleton/window/parent authority, OneTimeInitialization state 2, and rendering frozen with zero initializer-bearing/rejected/native escape.

Step 40 preserves that exact compatibility image and keeps `GameStartupWrapper` inert. Gate A requires the same-process frozen Step-39 authority. Gate B maps actual in-tree `_Process`, `_PhysicsProcess`, `_Draw`, and Godot input callbacks with deferred/rejecting Cecil before any restart. Gate C authorizes exactly one `StartRendering()` pulse targeted at 100 ms and immediately refreezes on the first continuation; observed elapsed must be <=500 ms. Gate D requires the same in-tree authority and zero forbidden escape with rendering stopped again.

Still closed: GameStartup/platform/main-menu/deferred startup, Steam/native Steam, native game GDExtensions, explicit `_ExitTree`, RemoveChild/Free, gameplay startup, and leaving rendering active.

Authoritative status: `CURRENT-STATUS.md`. Historical design/evidence remains under `history/`.
