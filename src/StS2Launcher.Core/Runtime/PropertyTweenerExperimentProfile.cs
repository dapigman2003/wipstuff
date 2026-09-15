namespace StS2Launcher.Core;

/// <summary>
/// Selects the Step-59 GodotSharp PropertyTweener experiment surface before Step 35 creates/loads
/// the private GodotSharp derivative. The profile is process-scoped and must be chosen before Gate A.
/// Baseline preserves only the physically proven PackedScene compatibility. Wrapper adds only the
/// TweenProperty native-pointer/managed-wrapper observation+repair. Full adds the typed-return
/// fluent observation+repair for SetEase/SetTrans/FromCurrent.
/// </summary>
public enum PropertyTweenerExperimentProfile
{
    Baseline = 0,
    Wrapper = 1,
    Full = 2,
}
