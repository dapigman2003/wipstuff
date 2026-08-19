namespace StS2Launcher.Core;

public enum FirstRealGameAssemblyLoadGate
{
    PreparedLoadPreflight = 1,
    PrimaryAssemblyLoad = 2,
    PlannedDependencyResolution = 3,
    LoadIsolationAudit = 4,
}
