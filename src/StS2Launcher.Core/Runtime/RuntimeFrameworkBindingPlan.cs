using System.Text.Json.Serialization;

namespace StS2Launcher.Core;

public sealed record RuntimeBindingPreparedAssembly(
    string RelativePath,
    string AssemblyFullName,
    string Sha1Hex,
    long Length,
    bool IsPrimary);

public sealed record RuntimeBindingHostFramework(
    string RequestedFullName,
    string ActualFullName,
    string ActualLocation,
    int ReferenceCount);

public sealed record RuntimeBindingBlocker(
    string Kind,
    string SourceAssemblyFullName,
    string RequestedFullName,
    string Detail);

public sealed record RuntimeBindingEdge(
    string SourceAssemblyFullName,
    string RequestedFullName,
    string BindingKind,
    string Target);

public sealed record RuntimeFrameworkBindingPlanDocument(
    int SchemaVersion,
    uint AppId,
    uint DepotId,
    ulong ManifestId,
    string Branch,
    string ManagedInstallRelativePath,
    string PrimaryAssemblyRelativePath,
    string PrimaryAssemblyFullName,
    RuntimeBindingPreparedAssembly[] PreparedAssemblies,
    RuntimeBindingHostFramework[] HostFrameworkBindings,
    RuntimeBindingBlocker[] Blockers,
    RuntimeBindingEdge[] Edges,
    bool RuntimeClosureReady)
{
    public const int CurrentSchemaVersion = 1;
}

[JsonSourceGenerationOptions(
    GenerationMode = JsonSourceGenerationMode.Metadata,
    WriteIndented = true)]
[JsonSerializable(typeof(RuntimeFrameworkBindingPlanDocument))]
[JsonSerializable(typeof(RuntimeBindingPreparedAssembly))]
[JsonSerializable(typeof(RuntimeBindingHostFramework))]
[JsonSerializable(typeof(RuntimeBindingBlocker))]
[JsonSerializable(typeof(RuntimeBindingEdge))]
public sealed partial class RuntimeFrameworkBindingJsonContext : JsonSerializerContext
{
}
