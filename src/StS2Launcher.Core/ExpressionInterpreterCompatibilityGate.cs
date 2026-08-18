namespace StS2Launcher.Core;

public enum ExpressionInterpreterCompatibilityGate
{
    InterpreterCapabilityAndWorkspaceClone = 1,
    RealCompileTargetDiscovery = 2,
    PreferInterpretationRewrite = 3,
    IsolationAudit = 4,
}
