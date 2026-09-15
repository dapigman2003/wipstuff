# 0.0.208 profile-isolation release checklist

- Bundle identity: **0.0.208 (208)**.
- Step52→58 current ownership path remains active; legacy 53–57 remain optional.
- Three closed-path profile buttons are present and select **Baseline / Wrapper / Full before Step 35**.
- Baseline derivative contains PackedScene compatibility and **no PropertyTweener experimental bridge members**.
- Wrapper derivative contains TweenProperty pointer/original-wrapper observation+repair and leaves fluent methods natural.
- Full derivative adds typed-return SetEase/SetTrans/FromCurrent observation+repair.
- Step-35 bootstrap never reflects Godot-typed experimental helper methods; only primitive fields are inspected for experimental profiles.
- 59T requires Baseline; 59W requires Wrapper; 59X/59E/real59 require Full. Wrong profile fails before mutation.
- Loader exception telemetry includes full chain, FileName/FusionLog, and stack.
- PackedScene unique-name compatibility remains present in every profile.
- Rejected 0.0.202 runtime-wide trace bridge remains absent.
- 0.0.205 post-native fluent-shape assumption remains absent.
- Same IPA retains 59D/R/H/U, 59T/W/X/E, real 59, and 60–62.
- No proprietary `sts2.dll`, `GodotSharp.dll`, PCK, app bundle, or native game payload is packaged.
