using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace StS2Launcher.iOS.Platform;

/// <summary>
/// Step 25.0.2 candidate-only trimming preservation for the exact framework surface referenced by
/// the physically measured HarmonyLib.Harmony::.ctor(string) IL. The real 0Harmony assembly is
/// loaded only after publish, so the build-time trimmer cannot discover these dependencies from
/// the Harmony IL itself. Keep this list bounded to framework types actually present in that exact
/// constructor and do not use it as a general reflection/dynamic-code preservation mechanism.
/// </summary>
internal static class Step25HarmonyConstructorFrameworkPreservation
{
    private const DynamicallyAccessedMemberTypes PublicCallableSurface =
        DynamicallyAccessedMemberTypes.PublicMethods |
        DynamicallyAccessedMemberTypes.PublicProperties;

    // Environment.GetEnvironmentVariable, Environment.Version, Environment.OSVersion.
    [DynamicDependency(PublicCallableSurface, typeof(Environment))]
    // Environment.OSVersion.Platform and Object.ToString() on the resulting enum/value path.
    [DynamicDependency(PublicCallableSurface, typeof(OperatingSystem))]
    // Type.GetTypeFromHandle, Type.Assembly.
    [DynamicDependency(PublicCallableSurface, typeof(Type))]
    // Assembly.GetName, Assembly.Location.
    [DynamicDependency(PublicCallableSurface, typeof(Assembly))]
    // AssemblyName.Version.
    [DynamicDependency(PublicCallableSurface, typeof(AssemblyName))]
    // MemberInfo.DeclaringType.
    [DynamicDependency(PublicCallableSurface, typeof(MemberInfo))]
    // DateTime.Now and formatting reached by the measured debug-only branch.
    [DynamicDependency(PublicCallableSurface, typeof(DateTime))]
    // Formatting of Environment.Version through the interpolated-string handler.
    [DynamicDependency(PublicCallableSurface, typeof(Version))]
    // Exact measured constructor uses .ctor, AppendLiteral, AppendFormatted<T>, and ToStringAndClear.
    [DynamicDependency(
        DynamicallyAccessedMemberTypes.PublicConstructors |
        DynamicallyAccessedMemberTypes.PublicMethods,
        typeof(DefaultInterpolatedStringHandler))]
    internal static void Activate()
    {
        // Intentionally empty. AppDelegate.FinishedLaunching calls this method so the method carrying
        // these DynamicDependency attributes is rooted. The attributes preserve metadata/IL only;
        // they do not execute any of the preserved framework members or any Harmony code.
    }
}
