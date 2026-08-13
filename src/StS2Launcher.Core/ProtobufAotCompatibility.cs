using ProtoBuf.Meta;

namespace StS2Launcher.Core;

/// <summary>
/// Retained Step 05.11 regression setting. The physical-iPhone Step 05.11 run
/// showed RuntimeTypeModel.Default.AutoCompile was already false before this
/// assignment, so Step 05.13 does not treat this as a fix. It remains only to
/// keep the proven Step 05.12 SteamKit2 3.4.0 surface unchanged while the Reflection.Emit stage is localized.
/// </summary>
public static class ProtobufAotCompatibility
{
    private static readonly object Gate = new();
    private static bool _configured;
    private static string? _summary;

    public static string Configure()
    {
        lock (Gate)
        {
            if (_configured)
                return _summary ?? "protobuf-net AOT mode already configured";

            var model = RuntimeTypeModel.Default;
            var before = model.AutoCompile;
            model.AutoCompile = false;
            var after = model.AutoCompile;
            var version = typeof(RuntimeTypeModel).Assembly.GetName().Version?.ToString() ?? "unknown";

            _summary =
                $"protobuf-net {version}; RuntimeTypeModel.Default.AutoCompile: {before} -> {after}";
            _configured = true;
            return _summary;
        }
    }
}
