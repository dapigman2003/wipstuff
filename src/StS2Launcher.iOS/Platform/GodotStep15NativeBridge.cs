using System.Runtime.InteropServices;

namespace StS2Launcher.iOS.Platform;

internal static class GodotStep15NativeBridge
{
    private const string InternalLibrary = "__Internal";

    [DllImport(InternalLibrary, EntryPoint = "sts2_step15_get_engine_version", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr GetEngineVersionNative();

    [DllImport(InternalLibrary, EntryPoint = "sts2_step15_last_error", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr GetLastErrorNative();

    [DllImport(InternalLibrary, EntryPoint = "sts2_step15_is_engine_started", CallingConvention = CallingConvention.Cdecl)]
    private static extern int IsEngineStartedNative();

    [DllImport(InternalLibrary, EntryPoint = "sts2_step15_start", CallingConvention = CallingConvention.Cdecl)]
    private static extern int StartNative(
        IntPtr parentController,
        IntPtr containerView,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string projectPathUtf8);

    [DllImport(InternalLibrary, EntryPoint = "sts2_step15_requires_process_restart", CallingConvention = CallingConvention.Cdecl)]
    private static extern int RequiresProcessRestartNative();

    [DllImport(InternalLibrary, EntryPoint = "sts2_step15_is_setup_finished", CallingConvention = CallingConvention.Cdecl)]
    private static extern int IsSetupFinishedNative();

    [DllImport(InternalLibrary, EntryPoint = "sts2_step15_is_rendering_active", CallingConvention = CallingConvention.Cdecl)]
    private static extern int IsRenderingActiveNative();

    [DllImport(InternalLibrary, EntryPoint = "sts2_step15_is_metal_layer_ready", CallingConvention = CallingConvention.Cdecl)]
    private static extern int IsMetalLayerReadyNative();

    [DllImport(InternalLibrary, EntryPoint = "sts2_step15_stop_rendering", CallingConvention = CallingConvention.Cdecl)]
    private static extern int StopRenderingNative();

    [DllImport(InternalLibrary, EntryPoint = "sts2_step15_start_rendering", CallingConvention = CallingConvention.Cdecl)]
    private static extern int StartRenderingNative();

    [DllImport(InternalLibrary, EntryPoint = "sts2_step15_render_marker_ready", CallingConvention = CallingConvention.Cdecl)]
    private static extern int RenderMarkerReadyNative();

    [DllImport(InternalLibrary, EntryPoint = "sts2_step15_touch_marker_ready", CallingConvention = CallingConvention.Cdecl)]
    private static extern int TouchMarkerReadyNative();

    [DllImport(InternalLibrary, EntryPoint = "sts2_step15_background_count", CallingConvention = CallingConvention.Cdecl)]
    private static extern int BackgroundCountNative();

    [DllImport(InternalLibrary, EntryPoint = "sts2_step15_foreground_count", CallingConvention = CallingConvention.Cdecl)]
    private static extern int ForegroundCountNative();

    [DllImport(InternalLibrary, EntryPoint = "sts2_step15_focus_out_count", CallingConvention = CallingConvention.Cdecl)]
    private static extern int FocusOutCountNative();

    [DllImport(InternalLibrary, EntryPoint = "sts2_step15_focus_in_count", CallingConvention = CallingConvention.Cdecl)]
    private static extern int FocusInCountNative();

    [DllImport(InternalLibrary, EntryPoint = "sts2_step15_is_runtime_interop_ready", CallingConvention = CallingConvention.Cdecl)]
    private static extern int IsRuntimeInteropReadyNative();

    [DllImport(InternalLibrary, EntryPoint = "sts2_step15_has_dotnet_feature", CallingConvention = CallingConvention.Cdecl)]
    private static extern int HasDotNetFeatureNative();

    [DllImport(InternalLibrary, EntryPoint = "sts2_step15_is_dotnet_runtime_initialized", CallingConvention = CallingConvention.Cdecl)]
    private static extern int IsDotNetRuntimeInitializedNative();

    [DllImport(InternalLibrary, EntryPoint = "sts2_step15_has_csharp_language_singleton", CallingConvention = CallingConvention.Cdecl)]
    private static extern int HasCSharpLanguageSingletonNative();

    [DllImport(InternalLibrary, EntryPoint = "sts2_step15_is_godot_api_cache_updated", CallingConvention = CallingConvention.Cdecl)]
    private static extern int IsGodotApiCacheUpdatedNative();

    [DllImport(InternalLibrary, EntryPoint = "sts2_step15_has_managed_create_binding_callback", CallingConvention = CallingConvention.Cdecl)]
    private static extern int HasManagedCreateBindingCallbackNative();

    [DllImport(InternalLibrary, EntryPoint = "sts2_step15_is_reverse_binding_ready", CallingConvention = CallingConvention.Cdecl)]
    private static extern int IsReverseBindingReadyNative();

    [DllImport(InternalLibrary, EntryPoint = "sts2_step15_get_managed_callbacks_size", CallingConvention = CallingConvention.Cdecl)]
    private static extern int GetManagedCallbacksSizeNative();

    [DllImport(InternalLibrary, EntryPoint = "sts2_step15_is_external_managed_bridge_installed", CallingConvention = CallingConvention.Cdecl)]
    private static extern int IsExternalManagedBridgeInstalledNative();

    [DllImport(InternalLibrary, EntryPoint = "sts2_step15_install_external_managed_callbacks", CallingConvention = CallingConvention.Cdecl)]
    private static extern int InstallExternalManagedCallbacksNative(
        [In, MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U1, SizeParamIndex = 1)] byte[] callbacks,
        int sizeBytes);

    [DllImport(InternalLibrary, EntryPoint = "sts2_step15_signal_external_core_api_loaded", CallingConvention = CallingConvention.Cdecl)]
    private static extern int SignalExternalCoreApiLoadedNative();

    [DllImport(InternalLibrary, EntryPoint = "sts2_step15_did_external_core_api_signal_return", CallingConvention = CallingConvention.Cdecl)]
    private static extern int DidExternalCoreApiSignalReturnNative();

    [DllImport(InternalLibrary, EntryPoint = "sts2_step15_get_runtime_interop_funcs", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr GetRuntimeInteropFunctionsNative(out int sizeBytes);

    public static string EngineVersion => PtrToString(GetEngineVersionNative());
    public static string LastError => PtrToString(GetLastErrorNative());
    public static bool IsEngineStarted => IsEngineStartedNative() != 0;
    public static bool RequiresProcessRestart => RequiresProcessRestartNative() != 0;
    public static bool IsSetupFinished => IsSetupFinishedNative() != 0;
    public static bool IsRenderingActive => IsRenderingActiveNative() != 0;
    public static bool IsMetalLayerReady => IsMetalLayerReadyNative() != 0;
    public static bool RenderMarkerReady => RenderMarkerReadyNative() != 0;
    public static bool TouchMarkerReady => TouchMarkerReadyNative() != 0;
    public static int BackgroundCount => BackgroundCountNative();
    public static int ForegroundCount => ForegroundCountNative();
    public static int FocusOutCount => FocusOutCountNative();
    public static int FocusInCount => FocusInCountNative();
    public static bool IsRuntimeInteropReady => IsRuntimeInteropReadyNative() != 0;
    public static bool HasDotNetFeature => HasDotNetFeatureNative() != 0;
    public static bool IsDotNetRuntimeInitialized => IsDotNetRuntimeInitializedNative() != 0;

    public static bool HasCSharpLanguageSingleton => HasCSharpLanguageSingletonNative() != 0;
    public static bool IsGodotApiCacheUpdated => IsGodotApiCacheUpdatedNative() != 0;
    public static bool HasManagedCreateBindingCallback => HasManagedCreateBindingCallbackNative() != 0;
    public static bool IsReverseBindingReady => IsReverseBindingReadyNative() != 0;
    public static int ManagedCallbacksSizeBytes => GetManagedCallbacksSizeNative();
    public static bool IsExternalManagedBridgeInstalled => IsExternalManagedBridgeInstalledNative() != 0;
    public static bool DidExternalCoreApiSignalReturn => DidExternalCoreApiSignalReturnNative() != 0;

    public static bool InstallExternalManagedCallbacks(byte[] callbacks) =>
        callbacks is { Length: > 0 } && InstallExternalManagedCallbacksNative(callbacks, callbacks.Length) != 0;

    public static bool SignalExternalCoreApiLoaded() => SignalExternalCoreApiLoadedNative() != 0;

    public static IntPtr GetRuntimeInteropFunctions(out int sizeBytes) =>
        GetRuntimeInteropFunctionsNative(out sizeBytes);

    public static int Start(IntPtr parentController, IntPtr containerView, string projectPath) =>
        StartNative(parentController, containerView, projectPath);

    public static bool StopRendering() => StopRenderingNative() != 0;
    public static bool StartRendering() => StartRenderingNative() != 0;

    private static string PtrToString(IntPtr pointer) =>
        pointer == IntPtr.Zero ? string.Empty : Marshal.PtrToStringUTF8(pointer) ?? string.Empty;
}
