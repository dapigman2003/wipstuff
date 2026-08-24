namespace StS2Launcher.Core;

public enum AheadOfLoadManagedTransformationGate
{
    FixtureAdmissionAndOfflineReady = 1,
    DeterministicRewrite = 2,
    TransformedImageVerification = 3,
    TransformedExecution = 4,
    FinalIsolationAudit = 5,
}
