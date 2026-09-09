# StS2 Launcher — Step 40.0

Active candidate: **0.0.171 (171)** — controlled real-game render pulse after physical Step 39 closure.

Physical **0.0.170 / Step 39.2** closed real SceneTree admission **4/4** on iPhone. The exact 69-node hierarchy passed its lifecycle audit, `SceneTree.Root.AddChild(NGame)` returned, `NGame.Instance`/`_window`/parent/state authority held, rendering stopped synchronously, and frozen Gate-D confinement passed with zero initializer-bearing/rejected/native escape.

**Step 40.0 does not enable GameStartup.** It keeps the physically proven Step-39.1 compatibility image and inert `GameStartupWrapper`, statically audits the retained in-tree hierarchy for frame/input callbacks, then permits one short `StartRendering()` pulse targeted at 100 ms. The first continuation immediately calls `StopRendering()` and requires the observed pulse to remain <=500 ms before final frozen confinement is accepted. There is no in-process retry once the pulse is armed.

Authoritative status and exact device sequence: `docs/CURRENT-STATUS.md`. The complete 0.0.170 Step-39 report, checkpoint, last checkpoint, and static map are preserved under `docs/history/reports/`.
