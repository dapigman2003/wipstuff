# Step 34.0.1 — iOS SystemButton compile correction

Step 34 remains **OPEN** at `0.0.122 (122)`. This is a build-only correction; the controlled transformed `PrewarmJit()` execution design is unchanged.

## Evidence

The first Step-34 Codemagic attempt passed canonical static validation **735/735** and the host regression suite **194/194**, with the stable `ios-canonical` cache restored (including a 2.8G iOS arm64 `obj` tree and 1,092 AOT output files). iOS publish then stopped during C# compilation on CS1503 in the new Step-34 UI partial. No IPA was produced and no physical Step-34 evidence exists from that attempt.

## Cause and correction

`RootViewController.SystemButton` accepts `(string title, nfloat fontSize)` and internally creates `UIButton.FromType(UIButtonType.System)`. The Step-34 partial accidentally passed `UIButtonType.System` as the second argument. The correction passes font size `17`, matching the established launcher controls.

No Core/runtime resolver, Step-32 transform, Step-33 admission, Step-34 gate, caching, trimming, Godot, dependency, signing, or release-identity behavior changes. Static validation now rejects `UIButtonType.System` in the Step-34 partial so the same API-shape mistake cannot recur there.
