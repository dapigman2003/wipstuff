# Step 59.12 — shape-independent PropertyTweener typed-return repair (0.0.206)

Physical 0.0.205 never reached Step 59. The physically closed convenience path stopped in Step-35 Gate A because candidate 0.0.205 required `PropertyTweener.SetEase()` to contain a specific post-native `isinst Godot.PropertyTweener` sequence. The real GodotSharp 4.5.1 image does not satisfy that speculative internal-shape assumption, so the fail-closed transform correctly rejected the candidate before CLR admission or game progression.

0.0.206 preserves the physically proven Step52→58 ownership architecture and the existing PackedScene instanced-root unique-name compatibility. It removes all fluent-method dependence on native-call/cast layout. `SetEase`, `SetTrans`, and `FromCurrent` are selected only by exact instance method identity + `Godot.PropertyTweener` return type. Every original `ret` instruction object is converted into the start of a typed epilogue that stores the return value, observes it, optionally returns the already-valid receiver when the typed value is null, and then returns. Existing branches to an original `ret` therefore cannot skip the epilogue.

`Tween.TweenProperty()` retains its dedicated single-caller native-helper observation because physical 0.0.204 proved the public method returns null and that helper specialization already survives Step-35 generation. It records the native pointer, managed-null/wrong-type identity, and can construct exact `PropertyTweener(IntPtr)` only when the native pointer is nonzero and the normal managed wrapper is unusable.

The same 0.0.206 IPA exposes three independent fresh-process tail modes to maximize information/optionality per compilation:

- **59T CONTROL** — wrapper repair OFF, fluent repair OFF.
- **59W WRAPPER-ONLY** — wrapper repair ON, fluent repair OFF.
- **59X FULL REPAIR** — wrapper repair ON, fluent typed-return repair ON.

If 59X passes, 59E and the real Step 59 use full repair automatically; a successful real Step 59 can continue through Steps 60–62 without recompilation. 59D/R/H/U remain available controls. Step 63 remains unopened.
