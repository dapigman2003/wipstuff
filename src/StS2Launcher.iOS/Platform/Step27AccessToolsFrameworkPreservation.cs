using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

namespace StS2Launcher.iOS.Platform;

/// <summary>
/// Step 27.0.4 candidate-only trimming preservation for the exact framework surface reached by
/// the physically measured HarmonyLib.AccessTools::.cctor. The real 0Harmony assembly is loaded
/// only after publish, and AccessTools locates RuntimeInformation by string before reflecting its
/// FrameworkDescription property, so the build-time trimmer cannot infer that dependency from the
/// Harmony IL. Keep this anchor bounded to the measured AccessTools runtime-detection/cache shape.
/// </summary>
internal static class Step27AccessToolsFrameworkPreservation
{
    // AccessTools uses Type.GetType("System.Runtime.InteropServices.RuntimeInformation", false)
    // followed by Type.GetProperty("FrameworkDescription") and PropertyInfo.GetValue(...).
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicProperties, typeof(RuntimeInformation))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicMethods, typeof(PropertyInfo))]
    // The measured initializer then creates its add-handler cache and lock directly from post-publish IL.
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(Dictionary<,>))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(ReaderWriterLockSlim))]
    internal static void Activate()
    {
        // Intentionally empty. Step 27 UI startup calls this method solely to root the
        // DynamicDependency attributes. No Harmony or preserved framework member is executed here.
    }
}
