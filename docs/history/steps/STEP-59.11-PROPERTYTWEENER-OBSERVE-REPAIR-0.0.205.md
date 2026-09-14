# Step 59.11 — PropertyTweener full-chain observe/repair (0.0.205)

Physical 0.0.204 reran 59T and reproduced the same boundary: visual modulation, old tween kill, and `Node.CreateTween()` returned normally, while `Tween.TweenProperty()` returned null. The 0.0.204 bridge reported `fallbackCount=0` and `lastNativePtr=0x0`. Because `lastNativePtr` was only assigned inside the fallback, this does **not** prove the native return was null; it proves only that the null-only fallback did not execute.

The generated binding shape is `NativeCalls object return -> isinst Godot.PropertyTweener`. Therefore a non-null managed object of the wrong wrapper type can bypass the 0.0.204 null check and then become null at `isinst`.

0.0.205 maximizes information/optionality per compilation:
- **59T CONTROL** explicitly disables repair and records every dedicated TweenProperty native pointer, managed-null count, wrong-type count, last managed object/runtime type, and fluent return telemetry.
- **59X REPAIR** enables the same bridge. If native pointer is nonzero and the managed result is null or not a `PropertyTweener`, it constructs the exact internal `PropertyTweener(IntPtr)` wrapper.
- `PropertyTweener.SetEase`, `SetTrans`, and `FromCurrent` receive method-local switchable post-native-call compatibility. When repair is enabled and their generated return is unusable, they return the already-valid receiver; shared `Godot.NativeCalls` helpers are not modified.
- **59E** and **real Step 59** explicitly enable repair in their own fresh processes.
- 59D/R/H/U remain available, and a successful real Step 59 can proceed directly through 60–62 in the same IPA.

PackedScene instanced-root unique-name compatibility remains separately protected. No direct game-field repair, `_Ready()` replay, character choice, embark/run start, or Step 63 is opened.
