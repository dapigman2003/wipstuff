namespace StS2Launcher.Core;

/// <summary>
/// Defines the current regression contract for System.Linq.Expressions on the launcher runtime.
/// Step 19 originally ran before the Step 20 Mono interpreter was enabled, so the historical
/// physical device reported IsDynamicCodeSupported=false. The canonical Step 20+ iOS runtime uses
/// MtouchInterpreter=-all, where dynamic code may be supported by the interpreter while native JIT
/// compilation remains unavailable. Current regressions therefore require successful expression
/// execution plus IsDynamicCodeCompiled=false on iOS; IsDynamicCodeSupported is diagnostic.
/// </summary>
public static class ExpressionRuntimeCompatibilityPolicy
{
    public const string HistoricalNoDynamicCodeMode = "ios-historical-no-dynamic-code-fallback";
    public const string InterpreterEnabledMode = "ios-interpreter-enabled-dynamic-code";
    public const string NonIosHostMode = "non-ios-host-test";
    public const string UnexpectedDynamicCompilationMode = "ios-unexpected-dynamic-code-compilation";

    public static ExpressionRuntimeCompatibilityAssessment Evaluate(
        bool isIos,
        bool dynamicCodeSupported,
        bool dynamicCodeCompiled)
    {
        if (!isIos)
        {
            return new ExpressionRuntimeCompatibilityAssessment(
                true,
                NonIosHostMode,
                "Non-iOS host-test runtime: iOS non-JIT policy is not applied; expression execution results remain authoritative for this host test.");
        }

        if (dynamicCodeCompiled)
        {
            return new ExpressionRuntimeCompatibilityAssessment(
                false,
                UnexpectedDynamicCompilationMode,
                $"Current iOS regression requires RuntimeFeature.IsDynamicCodeCompiled == false because the canonical launcher uses AOT plus the Mono interpreter rather than JIT-compiled dynamic native code. Observed IsDynamicCodeSupported={dynamicCodeSupported}, IsDynamicCodeCompiled={dynamicCodeCompiled}.");
        }

        if (dynamicCodeSupported)
        {
            return new ExpressionRuntimeCompatibilityAssessment(
                true,
                InterpreterEnabledMode,
                "Post-Step-20 canonical iOS runtime accepted: dynamic code is supported by the interpreter, while RuntimeFeature.IsDynamicCodeCompiled remains false (no dynamic native-code compilation/JIT).");
        }

        return new ExpressionRuntimeCompatibilityAssessment(
            true,
            HistoricalNoDynamicCodeMode,
            "Historical Step 19 iOS runtime accepted: dynamic code is unsupported and not dynamically compiled; the expression APIs still execute successfully through the host framework fallback path.");
    }
}

public sealed record ExpressionRuntimeCompatibilityAssessment(
    bool Compatible,
    string Mode,
    string Detail);
