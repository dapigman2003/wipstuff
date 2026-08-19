namespace StS2Launcher.Core;

public sealed record HostFrameworkClosureSpec(string Name, Version MinimumVersion, string PublicKeyToken);

public static class HostFrameworkClosureRootSet
{
    // These 22 direct roots are the actual host-binding frontier derived from the physical Step 21.1
    // report: every framework-shaped assembly Step 21 had put in the private prepared set, plus every
    // framework assembly directly blocked from a non-framework consumer (sts2/GodotSharp/Sentry/0Harmony).
    // Step 22.1 proved all 22 direct roots load on iPhone. Downstream implementation assemblies referenced
    // only by these host-bound framework assemblies are diagnostic observations, not independent game bindings.
    public static IReadOnlyList<string> DirectTrimmerRoots { get; } = new string[]
    {
        "netstandard",
        "System.Data.Common",
        "System.Diagnostics.Contracts",
        "System.Diagnostics.StackTrace",
        "System.Diagnostics.TraceSource",
        "System.Diagnostics.Tracing",
        "System.IO.FileSystem.DriveInfo",
        "System.IO.MemoryMappedFiles",
        "System.Net.Ping",
        "System.Net.Quic",
        "System.Numerics.Vectors",
        "System.Reflection.Metadata",
        "System.Runtime.CompilerServices.Unsafe",
        "System.Runtime.Loader",
        "System.Runtime.Serialization.Json",
        "System.Runtime.Serialization.Primitives",
        "System.Runtime.Serialization.Xml",
        "System.Threading.Tasks.Parallel",
        "System.Threading.ThreadPool",
        "System.Xml.XDocument",
        "System.Xml.XmlSerializer",
        "System.Xml.XPath",
    };

    // Complete 44-name desktop/workspace framework frontier observed by Step 21.1. Step 22.2 still probes
    // every identity for diagnostics, but Gate A requires only DirectTrimmerRoots. The remaining names are
    // implementation/transitive observations; Gate B's recomputed real sts2.dll plan is authoritative about
    // whether any of them is still an actual unresolved binding after the 22 host roots are active.
    public static IReadOnlyList<HostFrameworkClosureSpec> ExpectedHostClosure { get; } = new HostFrameworkClosureSpec[]
    {
        new("netstandard", new Version(2,1,0,0), "cc7b13ffcd2ddd51"),
        new("System.ComponentModel.EventBasedAsync", new Version(9,0,0,0), "b03f5f7f11d50a3a"),
        new("System.Data.Common", new Version(9,0,0,0), "b03f5f7f11d50a3a"),
        new("System.Diagnostics.Contracts", new Version(9,0,0,0), "b03f5f7f11d50a3a"),
        new("System.Diagnostics.FileVersionInfo", new Version(9,0,0,0), "b03f5f7f11d50a3a"),
        new("System.Diagnostics.StackTrace", new Version(9,0,0,0), "b03f5f7f11d50a3a"),
        new("System.Diagnostics.TextWriterTraceListener", new Version(9,0,0,0), "b03f5f7f11d50a3a"),
        new("System.Diagnostics.TraceSource", new Version(9,0,0,0), "b03f5f7f11d50a3a"),
        new("System.Diagnostics.Tracing", new Version(9,0,0,0), "b03f5f7f11d50a3a"),
        new("System.Drawing.Primitives", new Version(9,0,0,0), "b03f5f7f11d50a3a"),
        new("System.IO.Compression.Brotli", new Version(9,0,0,0), "b77a5c561934e089"),
        new("System.IO.Compression.ZipFile", new Version(9,0,0,0), "b77a5c561934e089"),
        new("System.IO.FileSystem.DriveInfo", new Version(9,0,0,0), "b03f5f7f11d50a3a"),
        new("System.IO.FileSystem.Watcher", new Version(9,0,0,0), "b03f5f7f11d50a3a"),
        new("System.IO.MemoryMappedFiles", new Version(9,0,0,0), "b03f5f7f11d50a3a"),
        new("System.IO.Pipes", new Version(9,0,0,0), "b03f5f7f11d50a3a"),
        new("System.Linq.Parallel", new Version(9,0,0,0), "b03f5f7f11d50a3a"),
        new("System.Linq.Queryable", new Version(9,0,0,0), "b03f5f7f11d50a3a"),
        new("System.Net.HttpListener", new Version(9,0,0,0), "cc7b13ffcd2ddd51"),
        new("System.Net.Mail", new Version(9,0,0,0), "cc7b13ffcd2ddd51"),
        new("System.Net.Ping", new Version(9,0,0,0), "b03f5f7f11d50a3a"),
        new("System.Net.Quic", new Version(9,0,0,0), "b03f5f7f11d50a3a"),
        new("System.Net.WebClient", new Version(9,0,0,0), "cc7b13ffcd2ddd51"),
        new("System.Net.WebProxy", new Version(9,0,0,0), "cc7b13ffcd2ddd51"),
        new("System.Numerics.Vectors", new Version(9,0,0,0), "b03f5f7f11d50a3a"),
        new("System.Private.DataContractSerialization", new Version(9,0,0,0), "b03f5f7f11d50a3a"),
        new("System.Private.Xml.Linq", new Version(9,0,0,0), "cc7b13ffcd2ddd51"),
        new("System.Reflection.DispatchProxy", new Version(9,0,0,0), "b03f5f7f11d50a3a"),
        new("System.Reflection.Metadata", new Version(9,0,0,0), "b03f5f7f11d50a3a"),
        new("System.Resources.Writer", new Version(9,0,0,0), "b03f5f7f11d50a3a"),
        new("System.Runtime.CompilerServices.Unsafe", new Version(9,0,0,0), "b03f5f7f11d50a3a"),
        new("System.Runtime.CompilerServices.VisualC", new Version(9,0,0,0), "b03f5f7f11d50a3a"),
        new("System.Runtime.Loader", new Version(9,0,0,0), "b03f5f7f11d50a3a"),
        new("System.Runtime.Serialization.Json", new Version(9,0,0,0), "b03f5f7f11d50a3a"),
        new("System.Runtime.Serialization.Primitives", new Version(9,0,0,0), "b03f5f7f11d50a3a"),
        new("System.Runtime.Serialization.Xml", new Version(9,0,0,0), "b03f5f7f11d50a3a"),
        new("System.Security.Claims", new Version(9,0,0,0), "b03f5f7f11d50a3a"),
        new("System.Threading.Tasks.Parallel", new Version(9,0,0,0), "b03f5f7f11d50a3a"),
        new("System.Threading.ThreadPool", new Version(9,0,0,0), "b03f5f7f11d50a3a"),
        new("System.Transactions.Local", new Version(9,0,0,0), "cc7b13ffcd2ddd51"),
        new("System.Xml.XDocument", new Version(9,0,0,0), "b03f5f7f11d50a3a"),
        new("System.Xml.XmlSerializer", new Version(9,0,0,0), "b03f5f7f11d50a3a"),
        new("System.Xml.XPath", new Version(9,0,0,0), "b03f5f7f11d50a3a"),
        new("System.Xml.XPath.XDocument", new Version(9,0,0,0), "b03f5f7f11d50a3a"),
    };
}
