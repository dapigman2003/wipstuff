# Step 39.0.1 — iOS namespace compile correction

Candidate: 0.0.167 (167).

Physical Codemagic evidence for 0.0.166 proved 1051/1051 static checks, 233/233 host tests, and the Step-15 native-link preflight before the iOS C# compile failed with CS0103 at all four Step-39 references to `GodotStep15NativeBridge`.

`GodotStep15NativeBridge` is declared in `StS2Launcher.iOS.Platform`. C# partial-class source files do not share `using` directives, and the new `RootViewController.TransformedRealStS2SceneTreeAdmission.cs` partial omitted the namespace import even though established RootViewController partials already use it.

0.0.167 changes only compile integration/release provenance:

- add `using StS2Launcher.iOS.Platform;` to the Step-39 UI partial;
- add a static regression contract requiring that import whenever the Step-39 UI references `GodotStep15NativeBridge`;
- advance display/build identity to 0.0.167 (167);
- preserve the exact Step-39 SceneTree/runtime semantics and the `ios-canonical` workflow/cache paths.

No game/native compatibility policy changes. Step 39 remains the first real `SceneTree.Root.AddChild(NGame)` boundary with the verified inert GameStartupWrapper and immediate render freeze after Gate C returns.
