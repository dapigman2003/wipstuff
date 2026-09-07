# Testing

Active candidate: `0.0.161 (161)`, IPA `StS2-Launcher-Step-37.ipa`, TRX `step37.trx`, workflow `ios-canonical`.

Canonical validation must prove the physical Step-36.0.5 closure is preserved, the Step-37 exact PCK scene seal is pinned, only the three authorized FMOD-neutral scene edits exist, trusted Step-12 content is never mutated, and the scene boundary cannot call `AddChild`, `GameStartup`, `LaunchMainMenu`, `ExecuteDeferred`, Steam initialization, or native GDExtension code.

Codemagic remains the first actual C# compile in this environment. The stable workflow/cache lineage is intentionally unchanged.

Physical sequence: fresh process -> Step 15 A-C -> Step 35 MODEL-BOOTSTRAP 4/4 -> Step 36.0.5 4/4 -> Step 37.0.1 A-D once. Use a fresh process after any Gate C/D failure.
