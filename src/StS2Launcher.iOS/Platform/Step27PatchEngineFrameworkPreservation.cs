using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;

namespace StS2Launcher.iOS.Platform;

/// <summary>
/// Step 27.0.7 candidate-only trimming preservation for the framework surface reached after the
/// physically proven Gate-T/T1 entry into HarmonyLib.PatchProcessor.Patch(). The receipt-backed
/// 0Harmony assembly is loaded only after publish, so the build-time trimmer cannot see its calls
/// into HarmonySharedState, MonoMod DynamicMethodDefinition, Reflection.Emit, or MethodHandle APIs.
/// Keep this anchor limited to the exact tagged Harmony 2.4.2 patch-engine closure audited by Gate O.
/// </summary>
internal static class Step27PatchEngineFrameworkPreservation
{
    private const DynamicallyAccessedMemberTypes PublicCallableSurface =
        DynamicallyAccessedMemberTypes.PublicMethods |
        DynamicallyAccessedMemberTypes.PublicProperties;

    // HarmonySharedState's Mono stack-frame compatibility branch uses AccessTools.FieldRefAccess,
    // whose MonoMod DynamicMethodDefinition generator probes private DynamicMethod implementation
    // fields and then generates an executable delegate through Reflection.Emit/Cecil.
    [DynamicDependency(
        DynamicallyAccessedMemberTypes.PublicConstructors |
        DynamicallyAccessedMemberTypes.PublicMethods |
        DynamicallyAccessedMemberTypes.NonPublicFields,
        typeof(DynamicMethod))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicMethods, typeof(ILGenerator))]

    // MethodCreator/MonoMod return MethodInfo objects and later HarmonySharedState reads MethodHandle
    // and GetFunctionPointer on Mono. Preserve the callable/property surface because those calls live
    // only in the post-publish 0Harmony image and are invisible to the app trimmer.
    [DynamicDependency(PublicCallableSurface, typeof(MethodBase))]
    [DynamicDependency(PublicCallableSurface, typeof(MethodInfo))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicMethods, typeof(RuntimeMethodHandle))]
    [DynamicDependency(PublicCallableSurface, typeof(FieldInfo))]

    // Harmony/MonoMod contain a MethodBuilder fallback/utility path on Mono/non-Windows. This is a
    // bounded type list from the exact upstream patch-engine source, not a Reflection.Emit assembly root.
    [DynamicDependency(
        DynamicallyAccessedMemberTypes.PublicConstructors |
        PublicCallableSurface,
        typeof(AssemblyName))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicMethods, typeof(AssemblyBuilder))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicMethods, typeof(ModuleBuilder))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicMethods, typeof(TypeBuilder))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicMethods, typeof(MethodBuilder))]
    internal static void Activate()
    {
        // Intentionally empty. The Step-27 UI startup roots only these DynamicDependency attributes.
        // No Reflection.Emit member, Harmony member, target method, or generated code is executed here.
    }
}
