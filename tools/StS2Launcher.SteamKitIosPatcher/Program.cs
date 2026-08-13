using Mono.Cecil;
using Mono.Cecil.Cil;

const string ExpectedAssemblyName = "SteamKit2";
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

    if (!string.Equals(
            assembly.Name.Name,
            ExpectedAssemblyName,
            StringComparison.Ordinal))
    {
        Console.Error.WriteLine(
            $"ERROR: expected {ExpectedAssemblyName}, got {assembly.Name.Name}.");
        return 4;
    }

    var version = assembly.Name.Version?.ToString() ?? "unknown";
    if (!version.StartsWith("3.3.1", StringComparison.Ordinal))
    {
        Console.Error.WriteLine(
            $"ERROR: Step 05.5 patch is pinned to SteamKit2 3.3.1; got {version}.");
        return 5;
    }

    var module = assembly.MainModule;
    var steamClient = module.Types.SingleOrDefault(t => t.FullName == SteamClientTypeName);
    if (steamClient is null)
    {
        Console.Error.WriteLine($"ERROR: {SteamClientTypeName} was not found.");
        return 6;
    }

    var replacements = 0;
    string? patchedMethod = null;

    foreach (var method in steamClient.Methods.Where(m => m.HasBody))
    {
        var il = method.Body.GetILProcessor();
        var instructions = method.Body.Instructions.ToArray();

        foreach (var instruction in instructions)
        {
            if (instruction.Operand is not MethodReference called ||
                !string.Equals(called.FullName, UnsupportedGetter, StringComparison.Ordinal))
            {
                continue;
            }

            // Original stack at this point contains the Process instance used by
            // Process.StartTime. iOS explicitly does not support that property.
            // Consume that Process value, then push DateTime.UtcNow instead.
            // The surrounding SteamKit using/finally remains intact and still
            // disposes the Process instance exactly as upstream intended.
            instruction.OpCode = OpCodes.Pop;
            instruction.Operand = null;

            var utcNowGetter = new MethodReference(
                "get_UtcNow",
                called.ReturnType,
                called.ReturnType)
            {
                HasThis = false
            };

            il.InsertAfter(
                instruction,
                Instruction.Create(OpCodes.Call, utcNowGetter));

            replacements++;
            patchedMethod = method.FullName;
        }
    }

    if (replacements != 1)
    {
        Console.Error.WriteLine(
            $"ERROR: expected exactly one Process.StartTime call in {SteamClientTypeName}; " +
            $"found {replacements}. Refusing to write a broad or ambiguous patch.");
        return 7;
    }

    // SteamKit2 3.3.1 is strong-name signed. We cannot legitimately re-sign a
    // third-party assembly with its publisher's private key. The application is
    // compiled against this patched build after the patch is applied, so remove
    // the signing identity rather than emitting an invalid strong-name signature.
    assembly.Name.HasPublicKey = false;
    assembly.Name.PublicKey = Array.Empty<byte>();
    module.Attributes &= ~ModuleAttributes.StrongNameSigned;

    assembly.Write(tempPath, new WriterParameters { WriteSymbols = false });

    // Verify the emitted assembly before replacing the local NuGet copy.
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

    Console.WriteLine("STEP05.5 STEAMKIT IOS PATCH: PASS");
    Console.WriteLine($"Assembly: SteamKit2 {version}");
    Console.WriteLine($"Patched method: {patchedMethod}");
    Console.WriteLine("Replacement count: 1");
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
