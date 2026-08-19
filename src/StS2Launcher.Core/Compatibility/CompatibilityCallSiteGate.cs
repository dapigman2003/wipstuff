namespace StS2Launcher.Core;

public enum CompatibilityCallSiteGate
{
    Arm64ManagedScope = 1,
    ActualIlCallSites = 2,
    NativePlatformInterop = 3,
    PrimaryDependencyPressureMap = 4,
}
