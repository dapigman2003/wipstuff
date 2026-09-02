using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace StS2Launcher.Core;

/// <summary>
/// Step-35 diagnostic-only, read-only reconnaissance over the already verified managed install.
/// It never loads a game/native binary, never calls a native entry point, and never mutates the install.
/// The output is intended to make expensive physical runs more informative by correlating GodotSharp
/// managed call paths with the native Mach-O inventory that would normally back Godot bootstrap calls.
/// </summary>
internal static class Step35GodotReconnaissance
{
    private static readonly string[] NativePathHints =
    [
        "/Contents/MacOS/", "/Contents/Frameworks/", "/Contents/PlugIns/", "/Contents/Resources/",
    ];

    private static readonly string[] NativeExtensions =
    [
        ".dylib", ".so", ".bundle", ".framework",
    ];

    private static readonly string[] InterestingNativeKeywords =
    [
        "godot", "godotsharp", "mono", "coreclr", "hostfxr", "hostpolicy", "gdextension",
        "nativefunc", "nativecall", "variant", "dictionary", "cmdline", "commandline", "script",
        "managed", "dotnet", "interop", "callback", "fmod", "steam_api",
    ];

    private static readonly string[] RequiredGodotTypes =
    [
        "Godot.Collections.Dictionary`2",
        "Godot.OS",
        "Godot.NativeCalls",
        "Godot.NativeInterop.NativeFuncs",
        "Godot.NativeInterop.InteropUtils",
        "Godot.NativeInterop.Marshaling",
    ];

    internal static string BuildReport(string managedInstallRoot, string godotSharpPath)
    {
        managedInstallRoot = Path.GetFullPath(managedInstallRoot);
        godotSharpPath = Path.GetFullPath(godotSharpPath);
        var lines = new List<string>
        {
            "StS2 Launcher — Step 35 comprehensive Godot / native reconnaissance",
            "Diagnostic-only output; read-only inspection. No binary in the managed install is loaded, executed, rewritten, or used as trusted runtime input by this report.",
            $"Generated UTC: {DateTimeOffset.UtcNow:O}",
            $"Managed install root: {managedInstallRoot}",
            $"GodotSharp path: {godotSharpPath}",
            $"GodotSharp bytes: {new FileInfo(godotSharpPath).Length:N0}",
            $"GodotSharp SHA-1: {ComputeHash(godotSharpPath, bytes => SHA1.HashData(bytes))}",
            $"GodotSharp SHA-256: {ComputeHash(godotSharpPath, bytes => SHA256.HashData(bytes))}",
            string.Empty,
        };

        AppendGodotSharpManagedMap(lines, godotSharpPath);
        lines.Add(string.Empty);
        AppendMachOInventory(lines, managedInstallRoot);
        return string.Join("\n", lines);
    }

    private static void AppendGodotSharpManagedMap(List<string> lines, string godotSharpPath)
    {
        lines.Add("[GODOTSHARP MANAGED / NATIVE-BOUNDARY MAP]");
        using var resolver = new RejectingResolver();
        using var module = ModuleDefinition.ReadModule(godotSharpPath, new ReaderParameters
        {
            ReadSymbols = false,
            ReadingMode = ReadingMode.Deferred,
            AssemblyResolver = resolver,
        });
        if (resolver.Requests.Count != 0)
            throw new InvalidDataException("Step-35 GodotSharp reconnaissance unexpectedly resolved an external assembly during metadata-only open.");

        lines.Add($"Assembly identity: {module.Assembly?.Name.FullName ?? "<module-only>"}");
        lines.Add($"Module MVID: {module.Mvid}");
        lines.Add("AssemblyRefs:");
        foreach (var reference in module.AssemblyReferences.OrderBy(reference => reference.FullName, StringComparer.Ordinal))
            lines.Add("  - " + reference.FullName);
        if (module.ModuleReferences.Count != 0)
        {
            lines.Add("ModuleRefs / PInvoke libraries declared by metadata:");
            foreach (var reference in module.ModuleReferences.OrderBy(reference => reference.Name, StringComparer.Ordinal))
                lines.Add("  - " + reference.Name);
        }

        var allTypes = EnumerateTypes(module.Types).ToArray();
        foreach (var required in RequiredGodotTypes)
            lines.Add($"Required type {required}: {(allTypes.Any(type => type.FullName == required) ? "PRESENT" : "MISSING")}");

        var selected = new List<MethodDefinition>();
        void AddMethods(string typeName, params string[] methodNames)
        {
            var type = allTypes.SingleOrDefault(candidate => candidate.FullName == typeName);
            if (type is null)
                return;
            foreach (var method in type.Methods.Where(method => method.HasBody && methodNames.Contains(method.Name, StringComparer.Ordinal)))
                if (!selected.Contains(method)) selected.Add(method);
        }

        AddMethods("Godot.Collections.Dictionary`2", ".cctor", ".ctor", "TryGetValue", "set_Item", "get_Item", "Dispose", "GetEnumerator");
        AddMethods("Godot.Collections.GodotDictionary", ".cctor", ".ctor", "Dispose", "TryGetValue", "set_Item", "get_Item");
        AddMethods("Godot.OS", ".cctor", "GetCmdlineArgs", "get_Singleton");
        AddMethods("Godot.OS/MethodName", ".cctor");
        AddMethods("Godot.OSInstance", ".ctor");
        AddMethods("Godot.StringName", ".cctor", "op_Implicit");
        AddMethods("Godot.GodotObject", ".cctor", "GetPtr", "ClassDB_get_method_with_compatibility", "ConstructAndInitialize");
        AddMethods("Godot.NativeCalls", "godot_icall_0_108");
        AddMethods("Godot.NativeInterop.InteropUtils", "EngineGetSingleton", "UnmanagedGetManaged");
        AddMethods("Godot.NativeInterop.Marshaling", "ConvertStringToNative");
        AddMethods("Godot.NativeInterop.GodotBoolExtensions", "ToBool");
        AddMethods("Godot.NativeInterop.NativeFuncs", ".cctor", "Initialize",
            "godotsharp_string_name_new_from_string",
            "godotsharp_method_bind_get_method_with_compatibility",
            "godotsharp_engine_get_singleton",
            "godotsharp_internal_unmanaged_get_script_instance_managed",
            "godotsharp_internal_unmanaged_get_instance_binding_managed",
            "godotsharp_internal_unmanaged_instance_binding_create_managed");

        // Physical 0.0.143 CORE-HANDOFF reached Godot.OS.GetCmdlineArgs -> OS.get_Singleton
        // after the callback table was initialized. Expand the metadata-only local closure around
        // singleton acquisition/wrapping while retaining the prior Dictionary/OS paths.
        var byFullName = allTypes.SelectMany(type => type.Methods).Where(method => method.HasBody)
            .GroupBy(method => method.FullName, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);
        var queue = new Queue<(MethodDefinition Method, int Depth)>();
        foreach (var seed in selected.Where(method =>
                     (method.DeclaringType.FullName == "Godot.Collections.Dictionary`2" && method.Name == ".ctor") ||
                     (method.DeclaringType.FullName == "Godot.OS" && method.Name is "GetCmdlineArgs" or ".cctor" or "get_Singleton") ||
                     (method.DeclaringType.FullName == "Godot.OS/MethodName" && method.Name == ".cctor") ||
                     (method.DeclaringType.FullName == "Godot.NativeInterop.InteropUtils" && method.Name is "EngineGetSingleton" or "UnmanagedGetManaged")))
            queue.Enqueue((seed, 0));
        var visited = new HashSet<string>(selected.Select(method => method.FullName), StringComparer.Ordinal);
        while (queue.Count != 0 && visited.Count < 128)
        {
            var (method, depth) = queue.Dequeue();
            if (depth >= 3) continue;
            foreach (var instruction in method.Body.Instructions)
            {
                if (instruction.Operand is not MethodReference called || !byFullName.TryGetValue(called.FullName, out var local))
                    continue;
                if (visited.Add(local.FullName))
                {
                    selected.Add(local);
                    queue.Enqueue((local, depth + 1));
                }
            }
        }

        lines.Add($"Selected critical/local managed methods: {selected.Count}");
        foreach (var method in selected.OrderBy(method => method.DeclaringType.FullName, StringComparer.Ordinal).ThenBy(method => method.MetadataToken.ToUInt32()))
            AppendMethodMap(lines, method);

        var allMethods = allTypes.SelectMany(type => type.Methods).ToArray();
        var pinvokes = allMethods.Where(method => method.IsPInvokeImpl || method.PInvokeInfo is not null).ToArray();
        lines.Add(string.Empty);
        lines.Add($"GodotSharp P/Invoke methods: {pinvokes.Length}");
        foreach (var method in pinvokes.Take(200))
            lines.Add($"  PINVOKE token=0x{method.MetadataToken.ToUInt32():X8} | {method.FullName} | module={method.PInvokeInfo?.Module?.Name ?? "<none>"} | entry={method.PInvokeInfo?.EntryPoint ?? "<none>"}");
        if (pinvokes.Length > 200) lines.Add($"  ... {pinvokes.Length - 200} additional P/Invoke method(s) omitted");

        var calli = allMethods.Where(method => method.HasBody)
            .SelectMany(method => method.Body.Instructions.Where(instruction => instruction.OpCode.Code == Code.Calli)
                .Select(instruction => (method, instruction)))
            .ToArray();
        lines.Add($"GodotSharp calli sites: {calli.Length}");
        foreach (var item in calli.Take(200))
            lines.Add($"  CALLI method=0x{item.method.MetadataToken.ToUInt32():X8} {item.method.FullName} | IL_{item.instruction.Offset:X4} | signature={item.instruction.Operand}");
        if (calli.Length > 200) lines.Add($"  ... {calli.Length - 200} additional calli site(s) omitted");

        var callbackFieldUses = allMethods.Where(method => method.HasBody)
            .SelectMany(method => method.Body.Instructions.Where(instruction => instruction.Operand is FieldReference field &&
                    (field.DeclaringType.FullName.Contains("NativeFuncs", StringComparison.Ordinal) ||
                     field.DeclaringType.FullName.Contains("UnmanagedCallbacks", StringComparison.Ordinal)))
                .Select(instruction => (method, instruction, field: (FieldReference)instruction.Operand!)))
            .ToArray();
        lines.Add($"NativeFuncs/UnmanagedCallbacks field-use sites: {callbackFieldUses.Length}");
        foreach (var item in callbackFieldUses.Take(240))
            lines.Add($"  CALLBACK-FIELD method=0x{item.method.MetadataToken.ToUInt32():X8} {item.method.FullName} | IL_{item.instruction.Offset:X4} {item.instruction.OpCode.Code} | {item.field.FullName}");
        if (callbackFieldUses.Length > 240) lines.Add($"  ... {callbackFieldUses.Length - 240} additional callback field-use site(s) omitted");

        if (resolver.Requests.Count != 0)
            throw new InvalidDataException("Step-35 GodotSharp reconnaissance unexpectedly resolved an external assembly during IL walk.");
    }

    private static void AppendMethodMap(List<string> lines, MethodDefinition method)
    {
        lines.Add(string.Empty);
        lines.Add($"METHOD token=0x{method.MetadataToken.ToUInt32():X8}; maxstack={method.Body.MaxStackSize}; locals={method.Body.Variables.Count}; handlers={method.Body.ExceptionHandlers.Count}; {method.FullName}");
        foreach (var instruction in method.Body.Instructions)
        {
            var extra = instruction.Operand switch
            {
                MethodReference called => $" | method={called.FullName} | scope={called.DeclaringType.Scope}",
                FieldReference field => $" | field={field.FullName} | scope={field.DeclaringType.Scope}",
                TypeReference type => $" | type={type.FullName} | scope={type.Scope}",
                Instruction target => $" | target=IL_{target.Offset:X4}",
                Instruction[] targets => " | targets=" + string.Join(",", targets.Select(target => $"IL_{target.Offset:X4}")),
                string text => $" | string=\"{Escape(text)}\"",
                null => string.Empty,
                _ => $" | operand={instruction.Operand}",
            };
            lines.Add($"  IL_{instruction.Offset:X4}: {instruction.OpCode.Code}{extra}");
        }
    }

    private static void AppendMachOInventory(List<string> lines, string managedInstallRoot)
    {
        lines.Add("[MACH-O / NATIVE INVENTORY]");
        var candidates = Directory.EnumerateFiles(managedInstallRoot, "*", SearchOption.AllDirectories)
            .Where(IsNativeCandidatePath)
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var parsed = new List<MachOInfo>();
        foreach (var path in candidates)
        {
            try
            {
                var info = TryReadMachO(path, managedInstallRoot);
                parsed.Add(info ?? new MachOInfo(
                    Path.GetRelativePath(managedInstallRoot, path).Replace('\\', '/'),
                    new FileInfo(path).Length,
                    TryComputeSha256(path),
                    "NOT-MACHO", 0, 0, 0, null, [], [], [], [], null));
            }
            catch (Exception ex)
            {
                parsed.Add(new MachOInfo(Path.GetRelativePath(managedInstallRoot, path).Replace('\\', '/'), new FileInfo(path).Length,
                    TryComputeSha256(path), "PARSE-ERROR", 0, 0, 0, null, [], [], [], [], $"{ex.GetType().Name}: {ex.Message}"));
            }
        }

        lines.Add($"Native/Mach-O candidates inspected: {candidates.Length}");
        lines.Add($"Mach-O images recognized: {parsed.Count(info => info.Kind.StartsWith("MH_", StringComparison.Ordinal) || info.Kind.StartsWith("FAT_", StringComparison.Ordinal))}");
        foreach (var info in parsed)
        {
            lines.Add(string.Empty);
            lines.Add($"NATIVE {info.RelativePath}");
            lines.Add($"  bytes={info.Length:N0}; sha256={info.Sha256}; kind={info.Kind}; cpu=0x{info.CpuType:X8}; subtype=0x{info.CpuSubtype:X8}; filetype=0x{info.FileType:X8}");
            if (!string.IsNullOrWhiteSpace(info.Uuid)) lines.Add("  uuid=" + info.Uuid);
            if (!string.IsNullOrWhiteSpace(info.Error)) lines.Add("  error=" + info.Error);
            foreach (var dependency in info.Dependencies) lines.Add("  dylib=" + dependency);
            foreach (var rpath in info.Rpaths) lines.Add("  rpath=" + rpath);
            foreach (var symbol in info.InterestingSymbols) lines.Add("  symbol=" + symbol);
            foreach (var text in info.InterestingStrings) lines.Add("  string=" + text);
        }
    }

    private static bool IsNativeCandidatePath(string path)
    {
        var normalized = path.Replace('\\', '/');
        var extension = Path.GetExtension(normalized);
        if (NativeExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase)) return true;
        if (NativePathHints.Any(hint => normalized.Contains(hint, StringComparison.OrdinalIgnoreCase)))
        {
            if (normalized.Contains(".framework/", StringComparison.OrdinalIgnoreCase)) return true;
            if (extension.Length == 0) return true;
        }
        return false;
    }

    private static MachOInfo? TryReadMachO(string path, string root)
    {
        var sha256 = TryComputeSha256(path);
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        if (stream.Length < 4) return null;
        Span<byte> first = stackalloc byte[4];
        stream.ReadExactly(first);
        var little = BinaryPrimitives.ReadUInt32LittleEndian(first);
        var big = BinaryPrimitives.ReadUInt32BigEndian(first);
        stream.Position = 0;

        // Thin MH_MAGIC_64 / MH_CIGAM_64. The shipped macOS arm64 depot is expected to be little-endian.
        if (little == 0xFEEDFACF)
            return ReadThin64(stream, path, root, 0, stream.Length, "MH_MAGIC_64", sha256);

        // FAT_MAGIC/FAT_MAGIC_64, big-endian. Prefer the arm64 slice; otherwise inspect the first slice.
        if (big is 0xCAFEBABE or 0xCAFEBABF)
        {
            Span<byte> header = stackalloc byte[8];
            stream.ReadExactly(header);
            var count = BinaryPrimitives.ReadUInt32BigEndian(header[4..8]);
            var is64 = big == 0xCAFEBABF;
            var archSize = is64 ? 32 : 20;
            if (count > 128) throw new InvalidDataException($"fat architecture count {count} is implausible");
            var slices = new List<(uint Cpu, uint Subtype, long Offset, long Size)>();
            for (var i = 0; i < count; i++)
            {
                var bytes = new byte[archSize];
                stream.ReadExactly(bytes);
                var cpu = BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(0, 4));
                var subtype = BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(4, 4));
                long offset = is64 ? checked((long)BinaryPrimitives.ReadUInt64BigEndian(bytes.AsSpan(8, 8))) : BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(8, 4));
                long size = is64 ? checked((long)BinaryPrimitives.ReadUInt64BigEndian(bytes.AsSpan(16, 8))) : BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(12, 4));
                slices.Add((cpu, subtype, offset, size));
            }
            var selected = slices.FirstOrDefault(slice => slice.Cpu == 0x0100000C);
            if (selected.Size == 0) selected = slices.FirstOrDefault();
            if (selected.Size == 0) return null;
            var info = ReadThin64(stream, path, root, selected.Offset, selected.Size, is64 ? "FAT_MAGIC_64" : "FAT_MAGIC", sha256);
            return info with { Kind = info.Kind + $" sliceCount={slices.Count}" };
        }
        return null;
    }

    private static MachOInfo ReadThin64(Stream stream, string path, string root, long sliceOffset, long sliceSize, string kind, string sha256)
    {
        if (sliceOffset < 0 || sliceSize < 32 || sliceOffset + sliceSize > stream.Length)
            throw new InvalidDataException("Mach-O slice range is invalid");
        stream.Position = sliceOffset;
        var header = new byte[32];
        stream.ReadExactly(header);
        if (BinaryPrimitives.ReadUInt32LittleEndian(header.AsSpan(0, 4)) != 0xFEEDFACF)
            throw new InvalidDataException("selected slice is not little-endian MH_MAGIC_64");
        var cpu = BinaryPrimitives.ReadUInt32LittleEndian(header.AsSpan(4, 4));
        var subtype = BinaryPrimitives.ReadUInt32LittleEndian(header.AsSpan(8, 4));
        var filetype = BinaryPrimitives.ReadUInt32LittleEndian(header.AsSpan(12, 4));
        var ncmds = BinaryPrimitives.ReadUInt32LittleEndian(header.AsSpan(16, 4));
        var sizeofcmds = BinaryPrimitives.ReadUInt32LittleEndian(header.AsSpan(20, 4));
        if (ncmds > 65536 || sizeofcmds > sliceSize - 32)
            throw new InvalidDataException("Mach-O load-command bounds are invalid");

        var dependencies = new List<string>();
        var rpaths = new List<string>();
        var interestingSymbols = new List<string>();
        string? uuid = null;
        var symoff = 0u; var nsyms = 0u; var stroff = 0u; var strsize = 0u;
        var commandPos = sliceOffset + 32;
        for (var i = 0u; i < ncmds; i++)
        {
            stream.Position = commandPos;
            Span<byte> prefix = stackalloc byte[8];
            stream.ReadExactly(prefix);
            var cmd = BinaryPrimitives.ReadUInt32LittleEndian(prefix[..4]);
            var cmdsize = BinaryPrimitives.ReadUInt32LittleEndian(prefix[4..8]);
            if (cmdsize < 8 || commandPos + cmdsize > sliceOffset + sliceSize)
                throw new InvalidDataException($"load command {i} has invalid size {cmdsize}");
            var bytes = new byte[checked((int)cmdsize)];
            stream.Position = commandPos;
            stream.ReadExactly(bytes);
            var span = bytes.AsSpan();
            if (cmd is 0x0000000C or 0x0000000D or 0x80000018 or 0x8000001F or 0x80000023)
            {
                if (cmdsize >= 24)
                {
                    var nameOffset = BinaryPrimitives.ReadUInt32LittleEndian(span.Slice(8, 4));
                    var name = ReadCString(span, nameOffset);
                    if (!string.IsNullOrWhiteSpace(name)) dependencies.Add($"0x{cmd:X8}:{name}");
                }
            }
            else if (cmd == 0x8000001C && cmdsize >= 12)
            {
                var pathOffset = BinaryPrimitives.ReadUInt32LittleEndian(span.Slice(8, 4));
                var value = ReadCString(span, pathOffset);
                if (!string.IsNullOrWhiteSpace(value)) rpaths.Add(value);
            }
            else if (cmd == 0x0000001B && cmdsize >= 24)
            {
                uuid = FormatMachOUuid(span.Slice(8, 16));
            }
            else if (cmd == 0x00000002 && cmdsize >= 24)
            {
                symoff = BinaryPrimitives.ReadUInt32LittleEndian(span.Slice(8, 4));
                nsyms = BinaryPrimitives.ReadUInt32LittleEndian(span.Slice(12, 4));
                stroff = BinaryPrimitives.ReadUInt32LittleEndian(span.Slice(16, 4));
                strsize = BinaryPrimitives.ReadUInt32LittleEndian(span.Slice(20, 4));
            }
            commandPos += cmdsize;
        }

        if (nsyms != 0 && strsize != 0 && nsyms < 5_000_000 &&
            symoff + (ulong)nsyms * 16 <= (ulong)sliceSize && stroff + (ulong)strsize <= (ulong)sliceSize)
        {
            var strings = new byte[checked((int)strsize)];
            stream.Position = sliceOffset + stroff;
            stream.ReadExactly(strings);
            var entriesToRead = Math.Min(nsyms, 500_000u);
            var entry = new byte[16];
            for (var i = 0u; i < entriesToRead && interestingSymbols.Count < 240; i++)
            {
                stream.Position = sliceOffset + symoff + i * 16L;
                stream.ReadExactly(entry);
                var strx = BinaryPrimitives.ReadUInt32LittleEndian(entry.AsSpan(0, 4));
                if (strx >= strings.Length) continue;
                var name = ReadCString(strings, strx);
                if (IsInteresting(name)) interestingSymbols.Add(name);
            }
        }

        var interestingStrings = ExtractInterestingAsciiStrings(stream, sliceOffset, sliceSize, 240);
        return new MachOInfo(
            Path.GetRelativePath(root, path).Replace('\\', '/'),
            new FileInfo(path).Length,
            sha256,
            kind,
            cpu,
            subtype,
            filetype,
            uuid,
            dependencies.Distinct(StringComparer.Ordinal).Take(240).ToArray(),
            rpaths.Distinct(StringComparer.Ordinal).Take(120).ToArray(),
            interestingSymbols.Distinct(StringComparer.Ordinal).Take(240).ToArray(),
            interestingStrings,
            null);
    }

    private static string[] ExtractInterestingAsciiStrings(Stream stream, long offset, long size, int cap)
    {
        stream.Position = offset;
        var remaining = size;
        var buffer = new byte[64 * 1024];
        var current = new List<byte>(128);
        var results = new List<string>();
        void Flush()
        {
            if (current.Count >= 5 && current.Count <= 512)
            {
                var text = Encoding.ASCII.GetString(CollectionsMarshal.AsSpan(current));
                if (IsInteresting(text) && !results.Contains(text, StringComparer.Ordinal)) results.Add(text);
            }
            current.Clear();
        }
        while (remaining > 0 && results.Count < cap)
        {
            var read = stream.Read(buffer, 0, (int)Math.Min(buffer.Length, remaining));
            if (read <= 0) break;
            remaining -= read;
            for (var i = 0; i < read; i++)
            {
                var b = buffer[i];
                if (b is >= 0x20 and <= 0x7E)
                {
                    if (current.Count < 512) current.Add(b);
                }
                else Flush();
                if (results.Count >= cap) break;
            }
        }
        Flush();
        return results.Take(cap).ToArray();
    }

    private static bool IsInteresting(string text)
        => !string.IsNullOrWhiteSpace(text) && InterestingNativeKeywords.Any(keyword => text.Contains(keyword, StringComparison.OrdinalIgnoreCase));

    private static string ReadCString(ReadOnlySpan<byte> bytes, uint offset)
    {
        if (offset >= bytes.Length) return string.Empty;
        var slice = bytes[(int)offset..];
        var zero = slice.IndexOf((byte)0);
        if (zero >= 0) slice = slice[..zero];
        return Encoding.UTF8.GetString(slice);
    }

    private static string FormatMachOUuid(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length != 16) return string.Empty;
        var hex = Convert.ToHexString(bytes).ToLowerInvariant();
        return $"{hex[..8]}-{hex[8..12]}-{hex[12..16]}-{hex[16..20]}-{hex[20..]}";
    }

    private static string TryComputeSha256(string path)
    {
        try
        {
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
        }
        catch (Exception ex)
        {
            return $"<hash-error:{ex.GetType().Name}>";
        }
    }

    private static string ComputeHash(string path, Func<byte[], byte[]> hash)
        => Convert.ToHexString(hash(File.ReadAllBytes(path))).ToLowerInvariant();

    private static IEnumerable<TypeDefinition> EnumerateTypes(IEnumerable<TypeDefinition> roots)
    {
        foreach (var type in roots)
        {
            yield return type;
            foreach (var nested in EnumerateTypes(type.NestedTypes)) yield return nested;
        }
    }

    private static string Escape(string text)
        => text.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\r", "\\r", StringComparison.Ordinal).Replace("\n", "\\n", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal);

    private sealed class RejectingResolver : IAssemblyResolver
    {
        internal List<string> Requests { get; } = [];
        public AssemblyDefinition Resolve(AssemblyNameReference name) { Requests.Add(name.FullName); throw new AssemblyResolutionException(name); }
        public AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters) => Resolve(name);
        public void Dispose() { }
    }

    private sealed record MachOInfo(
        string RelativePath,
        long Length,
        string Sha256,
        string Kind,
        uint CpuType,
        uint CpuSubtype,
        uint FileType,
        string? Uuid,
        string[] Dependencies,
        string[] Rpaths,
        string[] InterestingSymbols,
        string[] InterestingStrings,
        string? Error);
}
