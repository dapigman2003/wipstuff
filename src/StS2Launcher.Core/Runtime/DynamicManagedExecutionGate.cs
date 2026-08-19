namespace StS2Launcher.Core;

public enum DynamicManagedExecutionGate
{
    FixtureIntegrityAndOfflineReady = 1,
    DynamicFixtureExecution = 2,
    PrivateDependencyResolution = 3,
    IsolationAudit = 4,
}
