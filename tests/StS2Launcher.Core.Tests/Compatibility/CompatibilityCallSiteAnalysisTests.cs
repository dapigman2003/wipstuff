using System.Security.Cryptography;
using System.Text.Json;
using Mono.Cecil;
using Mono.Cecil.Cil;
using StS2Launcher.Core;

namespace StS2Launcher.Core.Tests;

[TestClass]
public sealed class CompatibilityCallSiteAnalysisTests
{
    [TestMethod]
    public void OrderedCompatibilityCallSiteGatesReachFourOfFourPass()
    {
        var gates = new CompatibilityCallSiteGateSequence();
        gates.Record(CompatibilityCallSiteGate.Arm64ManagedScope, true, "scope");
        gates.Record(CompatibilityCallSiteGate.ActualIlCallSites, true, "calls");
        gates.Record(CompatibilityCallSiteGate.NativePlatformInterop, true, "interop");
        gates.Record(CompatibilityCallSiteGate.PrimaryDependencyPressureMap, true, "map");

        var summary = gates.Snapshot();
        Assert.IsTrue(summary.Passed);
        Assert.AreEqual(4, summary.PassedGates);
        Assert.AreEqual("COMPATIBILITY CALL-SITE ANALYSIS PASS — 4/4", summary.Summary);
    }

    [TestMethod]
    public void CompatibilityCallSiteGatesStopAfterFirstFailure()
    {
        var gates = new CompatibilityCallSiteGateSequence();
        gates.Record(CompatibilityCallSiteGate.Arm64ManagedScope, true, "scope");
        gates.Record(CompatibilityCallSiteGate.ActualIlCallSites, false, "failed");
        Assert.ThrowsExactly<InvalidOperationException>(() =>
            gates.Record(CompatibilityCallSiteGate.NativePlatformInterop, true, "must not advance"));
        Assert.AreEqual(CompatibilityCallSiteGate.ActualIlCallSites, gates.Snapshot().FirstFailingGate);
    }

    [TestMethod]
    public async Task Arm64AnalysisUsesActualIlCallsAndExcludesX8664Duplicate()
    {
        using var temp = new TempTestDirectory("sts2-step17");
        var managedPath = Path.Combine(temp.Path, SteamOfflineInstallInspection.ManagedRootRelativePath, "Depot-2868842");
        var arm64Relative = "SlayTheSpire2.app/Contents/Resources/data_sts2_macos_arm64/sts2.dll";
        var x86Relative = "SlayTheSpire2.app/Contents/Resources/data_sts2_macos_x86_64/sts2.dll";
        var sharedRelative = "SlayTheSpire2.app/Contents/Resources/shared-helper.dll";

        var arm64Path = Path.Combine(managedPath, arm64Relative.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(arm64Path)!);
        WriteSyntheticRiskAssembly(arm64Path, "sts2");

        var x86Path = Path.Combine(managedPath, x86Relative.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(x86Path)!);
        File.Copy(arm64Path, x86Path, overwrite: true);

        var sharedPath = Path.Combine(managedPath, sharedRelative.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(sharedPath)!);
        WriteSimpleAssembly(sharedPath, "shared-helper");

        var files = new List<SteamManagedInstallFile>();
        foreach (var relative in new[] { arm64Relative, x86Relative, sharedRelative })
        {
            var path = Path.Combine(managedPath, relative.Replace('/', Path.DirectorySeparatorChar));
            var bytes = await File.ReadAllBytesAsync(path);
            files.Add(new SteamManagedInstallFile(
                relative,
                bytes.LongLength,
                Convert.ToHexString(SHA1.HashData(bytes)).ToLowerInvariant()));
        }

        var receipt = new SteamManagedInstallReceipt(
            SteamManagedInstallReceipt.CurrentSchemaVersion,
            2868840,
            2868842,
            999UL,
            "public",
            DateTimeOffset.UtcNow,
            files);
        await using (var stream = File.Create(Path.Combine(managedPath, SteamManagedInstallReceipt.FileName)))
        {
            await JsonSerializer.SerializeAsync(stream, receipt, SteamManagedInstallJsonContext.Default.SteamManagedInstallReceipt);
        }

        var before = SHA256.HashData(await File.ReadAllBytesAsync(arm64Path));
        var analysis = new CompatibilityCallSiteAnalysis(temp.Path);
        var gateA = await analysis.RunArm64ManagedScopeAsync();
        var gateB = await analysis.RunActualIlCallSiteScanAsync();
        var gateC = analysis.RunNativePlatformInteropClassification();
        var gateD = await analysis.RunPrimaryDependencyPressureMapAsync();
        var after = SHA256.HashData(await File.ReadAllBytesAsync(arm64Path));

        Assert.IsTrue(gateA.Passed, gateA.Detail);
        Assert.IsTrue(gateB.Passed, gateB.Detail);
        Assert.IsTrue(gateC.Passed, gateC.Detail);
        Assert.IsTrue(gateD.Passed, gateD.Detail);
        CollectionAssert.AreEqual(before, after);

        StringAssert.Contains(gateA.Detail, "macOS arm64 candidates selected: 1");
        StringAssert.Contains(gateA.Detail, "Architecture-neutral managed candidates selected: 1");
        StringAssert.Contains(gateA.Detail, "macOS x86_64 duplicates deliberately excluded: 1");
        StringAssert.Contains(gateB.Detail, "ExpressionCompile=1");
        StringAssert.Contains(gateB.Detail, "Evidence policy: these are IL instruction operands, not raw string hits.");
        StringAssert.Contains(gateC.Detail, "P/Invoke definitions: 1");
        StringAssert.Contains(gateC.Detail, "libfixture.dylib");
        StringAssert.Contains(gateD.Detail, "Primary dynamic/AOT-sensitive sites: 1 (ExpressionCompile=1)");
        StringAssert.Contains(gateD.Detail, "Godot/GodotSharp=1");
        StringAssert.Contains(gateD.Detail, "Steamworks=1");
        StringAssert.Contains(gateD.Detail, "All Step 17 scan candidates receipt SHA-1 preserved: YES");
        StringAssert.Contains(gateD.Detail, "Assembly dependency resolution attempted: NO");
        StringAssert.Contains(gateD.Detail, "Game assembly loaded/executed: NO");
    }

    private static void WriteSyntheticRiskAssembly(string path, string assemblyName)
    {
        using var assembly = AssemblyDefinition.CreateAssembly(
            new AssemblyNameDefinition(assemblyName, new Version(1, 0, 0, 0)),
            assemblyName,
            ModuleKind.Dll);
        var module = assembly.MainModule;
        var type = new TypeDefinition("Synthetic", "GameEntry", TypeAttributes.Public | TypeAttributes.Class, module.TypeSystem.Object);
        module.Types.Add(type);

        var pinvokeModule = new ModuleReference("libfixture.dylib");
        module.ModuleReferences.Add(pinvokeModule);
        var nativeProbe = new MethodDefinition(
            "NativeProbe",
            MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.PInvokeImpl,
            module.TypeSystem.Int32)
        {
            PInvokeInfo = new PInvokeInfo(PInvokeAttributes.CallConvCdecl, "fixture_probe", pinvokeModule),
        };
        type.Methods.Add(nativeProbe);

        var expressionsAssembly = new AssemblyNameReference("System.Linq.Expressions", new Version(9, 0, 0, 0));
        var godotAssembly = new AssemblyNameReference("GodotSharp", new Version(4, 5, 1, 0));
        var steamAssembly = new AssemblyNameReference("Steamworks.NET", new Version(20, 0, 0, 0));
        module.AssemblyReferences.Add(expressionsAssembly);
        module.AssemblyReferences.Add(godotAssembly);
        module.AssemblyReferences.Add(steamAssembly);

        var lambdaType = new TypeReference("System.Linq.Expressions", "LambdaExpression", module, expressionsAssembly);
        var compile = new MethodReference("Compile", module.TypeSystem.Object, lambdaType) { HasThis = true };
        var godotType = new TypeReference("Godot", "GD", module, godotAssembly);
        var godotPrint = new MethodReference("Print", module.TypeSystem.Void, godotType) { HasThis = false };
        var steamType = new TypeReference("Steamworks", "SteamAPI", module, steamAssembly);
        var steamInit = new MethodReference("Init", module.TypeSystem.Void, steamType) { HasThis = false };

        var run = new MethodDefinition("Run", MethodAttributes.Public | MethodAttributes.Static, module.TypeSystem.Void);
        type.Methods.Add(run);
        var il = run.Body.GetILProcessor();
        il.Append(il.Create(OpCodes.Ldnull));
        il.Append(il.Create(OpCodes.Callvirt, compile));
        il.Append(il.Create(OpCodes.Pop));
        il.Append(il.Create(OpCodes.Call, nativeProbe));
        il.Append(il.Create(OpCodes.Pop));
        il.Append(il.Create(OpCodes.Call, godotPrint));
        il.Append(il.Create(OpCodes.Call, steamInit));
        il.Append(il.Create(OpCodes.Ret));

        assembly.Write(path);
    }

    private static void WriteSimpleAssembly(string path, string assemblyName)
    {
        using var assembly = AssemblyDefinition.CreateAssembly(
            new AssemblyNameDefinition(assemblyName, new Version(1, 0, 0, 0)),
            assemblyName,
            ModuleKind.Dll);
        var type = new TypeDefinition("Synthetic", "Helper", TypeAttributes.Public | TypeAttributes.Class, assembly.MainModule.TypeSystem.Object);
        assembly.MainModule.Types.Add(type);
        var method = new MethodDefinition("Noop", MethodAttributes.Public | MethodAttributes.Static, assembly.MainModule.TypeSystem.Void);
        type.Methods.Add(method);
        var il = method.Body.GetILProcessor();
        il.Append(il.Create(OpCodes.Ret));
        assembly.Write(path);
    }
}
