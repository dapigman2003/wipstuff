namespace StS2Launcher.Core;

public enum ControlledManagedInitializationGate
{
    InitializationPreflight = 1,
    ProvenLoadStateReplay = 2,
    DeferredModuleInitialization = 3,
    PostInitializationAudit = 4,
}
