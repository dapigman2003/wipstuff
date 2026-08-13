using ProtoBuf.Meta;

namespace StS2Launcher.Core;

/// <summary>
/// Step 05.11 experiment: keep protobuf-net 3.2.56 on its runtime/reflection
/// serializer path instead of allowing runtime serializer compilation.
///
/// iOS device execution is AOT-only and cannot support Reflection.Emit. This
/// setting is applied before SteamKit constructs/sends its initial ClientHello.
/// It does not change the Steam protocol or add authentication.
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
