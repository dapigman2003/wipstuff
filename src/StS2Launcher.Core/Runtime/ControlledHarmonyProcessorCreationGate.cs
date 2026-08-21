namespace StS2Launcher.Core;

public enum ControlledHarmonyProcessorCreationGate
{
    InitializationPreflight = 1,
    ProvenLoadStateReplay = 2,
    DeferredModuleInitialization = 3,
    ProvenInitializationAudit = 4,
    HarmonyApiResolution = 5,
    HarmonyTypeInitialization = 6,
    HarmonyTypeInitializationAudit = 7,
    HarmonyInstanceConstruction = 8,
    PostConstructionAudit = 9,
    HarmonyProcessorApiResolution = 10,
    PatchProcessorTypeInitialization = 11,
    LauncherProbeResolution = 12,
    HarmonyProcessorCreation = 13,
    PostProcessorAudit = 14,
}
