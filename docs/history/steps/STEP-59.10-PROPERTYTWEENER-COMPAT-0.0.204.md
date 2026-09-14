# Step 59.10 — PropertyTweener managed-wrapper compatibility (0.0.204)

## Physical basis from 0.0.203

The compilation-efficient Step-59 probes localized the retained embark-button failure without modifying shared sts2 methods:

- **59U**: exact inherited `NButton.UpdateControllerButton()` returned normally.
- **59T**: `outline.Modulate`, `buttonImage.Modulate`, existing `Tween.Kill()`, and `Node.CreateTween()` all returned normally. `Tween.TweenProperty(...)` then returned `null`.
- **59E**: direct retained `NClickableControl.Enable()` outside `OpenCharacterSelect` reproduced the same `NullReferenceException` with target `NConfirmButton.OnEnable()`.

The original `NConfirmButton.OnEnable()` immediately chains `SetEase()` after `TweenProperty()`, so a null `TweenProperty()` return explains the observed NRE. The failure no longer requires the submenu push or `OpenCharacterSelect` context.

A bounded excerpt of the still-active Step-35 GodotSharp telemetry around the 59E failure shows the object-return path entering native method-bind ptrcall, `InteropUtils.UnmanagedGetManaged(IntPtr)`, script-instance lookup, instance-binding lookup, and wrapper construction/tracking.

## Static GodotSharp localization

Inspection of the exact supplied GodotSharp image established:

- `Godot.Tween.TweenProperty` calls a signature-specific `Godot.NativeCalls` object-return helper and casts the result to `Godot.PropertyTweener`.
- That NativeCalls helper has exactly **one callsite in the whole image**: `Tween.TweenProperty`.
- The helper obtains the returned native reference pointer and then calls `Godot.NativeInterop.InteropUtils.UnmanagedGetManaged(IntPtr)`.
- `Godot.PropertyTweener` has an exact internal `.ctor(IntPtr)` wrapper constructor.
- `PropertyTweener.SetEase`, `SetTrans`, and `FromCurrent` use shared NativeCalls helpers, so 0.0.204 deliberately does **not** patch those shared helpers.

## 0.0.204 correction

0.0.204 specializes only the dedicated NativeCalls helper selected by `Tween.TweenProperty`:

1. Preserve the native pointer passed to `UnmanagedGetManaged(IntPtr)`.
2. Let normal `UnmanagedGetManaged` behavior run first.
3. If it returns a managed object, return it unchanged.
4. If it returns null and the native pointer is zero, preserve null unchanged.
5. Only if it returns null **and** the native pointer is nonzero, construct the exact internal `Godot.PropertyTweener(IntPtr)` wrapper.
6. Record a fallback count and last native pointer so physical runs can prove whether the correction was actually exercised.

This does not globally change `InteropUtils.UnmanagedGetManaged`, does not patch shared fluent helpers, and does not modify sts2 methods.

The physically proven PackedScene instanced-root `unique_name_in_owner` compatibility remains present and is separately guarded by static validation.

## Compilation-efficient physical plan

Use the same 0.0.204 IPA for progressive fresh-process experiments:

1. Closed path through Step 52 -> Step 58 -> **59T**. Expected progressive result: TweenProperty returns a non-null `PropertyTweener`; fallback telemetry states whether the narrow compatibility was exercised; then SetEase/SetTrans/FromCurrent are tested naturally.
2. Fresh process -> Step 58 -> **59E**. This tests the complete retained embark `Enable()` path.
3. Fresh process -> Step 58 -> **real Step 59**. If it succeeds, continue to Steps 60 -> 61 -> 62 without recompiling.
4. 59R/59H/59U/59D remain available as controls if any unexpected divergence occurs.

Step 35 telemetry remains available for now because it can still corroborate managed/native wrapper behavior. Performance cleanup is deferred until the compatibility path is stable unless it begins to interfere with correctness.

Step 63 remains unopened.
