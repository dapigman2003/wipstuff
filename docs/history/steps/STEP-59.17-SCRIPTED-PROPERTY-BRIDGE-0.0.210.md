# Step 59.17 — Scripted-object generic property bridge diagnostics and progression fallback — 0.0.210

Physical 0.0.209 / 59N localized the active failure beyond Tween creation. Fresh Node, SceneTree, and bound SceneTree Tweens are valid/running and accept `TweenInterval`, while `TweenProperty` returns native null for `position`, `scale`, and `modulate` when the target is the game-scripted `NConfirmButton`. The same `TweenProperty` mechanism succeeds for the retained native `Outline` `TextureRect` child.

0.0.210 therefore leaves the physically viable Baseline / Wrapper / Full GodotSharp derivative builder and profile enum byte-for-byte unchanged. No new bootstrap-critical IL rewrite is introduced. The new work is confined to Step-59 runtime/UI branches.

## 59P — generic property resolution matrix

Fresh FULL-profile process only. Compare three retained objects: the scripted embark `NConfirmButton`, the scripted character-select `_actDropdown`, and the native embark `Outline` child. For `position`, `scale`, and `modulate`, record direct managed property access alongside Godot `Get(StringName)`, `GetIndexed(NodePath)`, and same-value `Set` / `SetIndexed` probes. Record native pointer and script identity. One property failure must not prevent the remaining rows from being captured.

## 59I — managed/native identity matrix

Fresh FULL-profile process only. For the same three objects, compare the retained managed instance with private-GodotSharp `InteropUtils.UnmanagedGetManaged(NativePtr)`: exact runtime type, managed identity hash, native pointer, instance ID, script identity, and `ReferenceEquals`. Record the exact script-instance / instance-binding native callback signatures without invoking those callbacks directly. This distinguishes a generic property callback problem from a duplicate/stale native-to-managed object association.

## 59Q — direct-managed embark compatibility + original transition

Fresh FULL-profile process only. This is a bounded progression experiment, not a claim that the generic bridge is repaired. Prepare the retained embark button using direct managed operations that avoid the failing native generic-property tween path. The required bypass state is `_isEnabled=true` plus direct `Position=_showPos`; base NButton hotkey/controller updates, expected visual state, old-tween kill/clear, and focus refresh are attempted as best-effort fidelity stages and any failures are preserved without preventing the transition attempt. Then invoke the existing original `OpenCharacterSelect(NButton)` Gate C exactly once.

The key mechanism is that the original `NClickableControl.Enable()` immediately returns when `_isEnabled` is already true, so the original game-owned transition can continue without re-entering the failing `NConfirmButton.OnEnable()` TweenProperty tail. If Gate C and frozen confinement Gate D pass, the same shared Step-59 gate accumulator must close 4/4 so the already-compiled Steps 60–62 can run in that same process.

## Safety / compilation-efficiency contract

- Profile selection still occurs before Step 35.
- No 0.0.202-style shared sts2 instrumentation is reintroduced.
- PackedScene unique-name compatibility remains unchanged.
- The 0.0.208/0.0.209 GodotSharp derivative builder and `PropertyTweenerExperimentProfile` enum are hash-pinned unchanged.
- 59P, 59I, and 59Q are one-shot fresh-process branches.
- 59Q never fabricates a native PropertyTweener and does not pretend to fix generic property access globally.
- Existing 59N/Y/Z and older 59D/R/H/U/T/W/X/E/real-59 controls remain compiled for fallback evidence.
- Step 63 remains unopened.
