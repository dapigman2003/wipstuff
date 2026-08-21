namespace StS2Launcher.Core;

public enum ControlledHarmonyConstructionGate
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
}
