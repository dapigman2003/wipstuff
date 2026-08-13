using Mono.Cecil;
using Mono.Cecil.Cil;

const string ExpectedAssemblyName = "SteamKit2";
const string ExpectedVersionPrefix = "3.4.0";
const string SteamClientTypeName = "SteamKit2.SteamClient";
const string UnsupportedGetter = "System.DateTime System.Diagnostics.Process::get_StartTime()";

if (args.Length != 1)
{
    Console.Error.WriteLine("Usage: StS2Launcher.SteamKitIosPatcher <SteamKit2.dll>");
    return 2;
}

var inputPath = Path.GetFullPath(args[0]);
if (!File.Exists(inputPath))
{
    Console.Error.WriteLine($"ERROR: SteamKit2 assembly not found: {inputPath}");
    return 3;
}

var tempPath = inputPath + ".sts2-ios.tmp";
if (File.Exists(tempPath))
    File.Delete(tempPath);

try
{
    using var assembly = AssemblyDefinition.ReadAssembly(
        inputPath,
        new ReaderParameters
        {
            InMemory = true,
            ReadSymbols = false
        });

    if (!string.Equals(assembly.Name.Name, ExpectedAssemblyName, StringComparison.Ordinal))
    {
        Console.Error.WriteLine($"ERROR: expected {ExpectedAssemblyName}, got {assembly.Name.Name}.");
        return 4;
    }

    var version = assembly.Name.Version?.ToString() ?? "unknown";
    if (!version.StartsWith(ExpectedVersionPrefix, StringComparison.Ordinal))
    {
        Console.Error.WriteLine(
            $"ERROR: Step 05.12 comparison patcher expects SteamKit2 {ExpectedVersionPrefix}; got {version}.");
        return 5;
    }

    var module = assembly.MainModule;
    var steamClient = module.Types.SingleOrDefault(t => t.FullName == SteamClientTypeName);
    if (steamClient is null)
    {
        Console.Error.WriteLine($"ERROR: {SteamClientTypeName} was not found.");
        return 6;
    }

    var matches = steamClient.Methods
        .Where(m => m.HasBody)
        .SelectMany(m => m.Body.Instructions.Select(i => (Method: m, Instruction: i)))
        .Where(x => x.Instruction.Operand is MethodReference mr &&
                    string.Equals(mr.FullName, UnsupportedGetter, StringComparison.Ordinal))
        .ToArray();

    if (matches.Length > 1)
    {
        Console.Error.WriteLine(
            $"ERROR: expected zero or one Process.StartTime call in {SteamClientTypeName}; " +
            $"found {matches.Length}. Refusing a broad or ambiguous patch.");
        return 7;
    }

    if (matches.Length == 0)
    {
        // SteamKit 3.4.0 may have removed or changed the constructor-time use.
        // This is acceptable for the controlled upgrade experiment: verify the
        // unsupported call is absent and leave the publisher-signed assembly intact.
        Console.WriteLine("STEP05.12 STEAMKIT IOS PATCH: PASS");
        Console.WriteLine($"Assembly: SteamKit2 {version}");
        Console.WriteLine("Patched method: (none)");
        Console.WriteLine("Replacement count: 0");
        Console.WriteLine("Process.StartTime status: already absent");
        Console.WriteLine("Replacement value: not required");
        Console.WriteLine("Strong-name publisher signature removed from local build copy: NO");
        return 0;
    }

    var match = matches[0];
    var called = (MethodReference)match.Instruction.Operand!;
    var il = match.Method.Body.GetILProcessor();

    // Original stack at this point contains the Process instance used by
    // Process.StartTime. iOS explicitly does not support that property.
    // Consume that Process value, then push DateTime.UtcNow instead. The
    // surrounding SteamKit using/finally remains intact.
    match.Instruction.OpCode = OpCodes.Pop;
    match.Instruction.Operand = null;

    var utcNowGetter = new MethodReference(
        "get_UtcNow",
        called.ReturnType,
        called.ReturnType)
    {
        HasThis = false
    };

    il.InsertAfter(match.Instruction, Instruction.Create(OpCodes.Call, utcNowGetter));

    // The assembly is publisher strong-name signed. We cannot re-sign a third-party
    // assembly with its publisher key. The app compiles against this patched local
    // copy, so strip the invalidated signature rather than emitting a bad signature.
    assembly.Name.HasPublicKey = false;
    assembly.Name.PublicKey = Array.Empty<byte>();
    module.Attributes &= ~ModuleAttributes.StrongNameSigned;

    assembly.Write(tempPath, new WriterParameters { WriteSymbols = false });

    using (var verify = AssemblyDefinition.ReadAssembly(tempPath))
    {
        var remaining = verify.MainModule.Types
            .Where(t => t.FullName == SteamClientTypeName)
            .SelectMany(t => t.Methods)
            .Where(m => m.HasBody)
            .SelectMany(m => m.Body.Instructions)
            .Count(i => i.Operand is MethodReference mr &&
                        string.Equals(mr.FullName, UnsupportedGetter, StringComparison.Ordinal));

        if (remaining != 0)
        {
            Console.Error.WriteLine(
                $"ERROR: verification found {remaining} surviving Process.StartTime call(s).");
            return 8;
        }
    }

    File.Move(tempPath, inputPath, overwrite: true);

    Console.WriteLine("STEP05.12 STEAMKIT IOS PATCH: PASS");
    Console.WriteLine($"Assembly: SteamKit2 {version}");
    Console.WriteLine($"Patched method: {match.Method.FullName}");
    Console.WriteLine("Replacement count: 1");
    Console.WriteLine("Process.StartTime status: patched");
    Console.WriteLine("Unsupported call removed: System.Diagnostics.Process.StartTime");
    Console.WriteLine("Replacement value: System.DateTime.UtcNow");
    Console.WriteLine("Strong-name publisher signature removed from local build copy: YES");
    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine("ERROR: SteamKit iOS compatibility patch failed.");
    Console.Error.WriteLine(ex);
    return 9;
}
finally
{
    if (File.Exists(tempPath))
        File.Delete(tempPath);
}
