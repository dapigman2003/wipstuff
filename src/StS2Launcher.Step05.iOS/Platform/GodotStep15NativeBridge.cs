using System.Runtime.InteropServices;

namespace StS2Launcher.Step05.iOS.Platform;

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

    public static int Start(IntPtr parentController, IntPtr containerView, string projectPath) =>
        StartNative(parentController, containerView, projectPath);

    public static bool StopRendering() => StopRenderingNative() != 0;
    public static bool StartRendering() => StartRenderingNative() != 0;

    private static string PtrToString(IntPtr pointer) =>
        pointer == IntPtr.Zero ? string.Empty : Marshal.PtrToStringUTF8(pointer) ?? string.Empty;
}
