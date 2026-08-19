using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Security.Cryptography;
using System.Text.Json;
using Mono.Cecil;
using Mono.Cecil.Cil;
using StS2Launcher.Core;

namespace StS2Launcher.Core.Tests;

[TestClass]
public sealed class RealAssemblyRewriteWorkspaceTests
{
    [TestMethod]
    public void OrderedRealAssemblyRewriteGatesReachFourOfFourPass()
    {
        var gates = new RealAssemblyRewriteGateSequence();
        gates.Record(RealAssemblyRewriteGate.WorkspaceClone, true, "clone");
        gates.Record(RealAssemblyRewriteGate.PrimaryRoundTrip, true, "roundtrip");
        gates.Record(RealAssemblyRewriteGate.NeutralIlRewrite, true, "nop");
        gates.Record(RealAssemblyRewriteGate.IsolationAudit, true, "audit");

        var summary = gates.Snapshot();
        Assert.IsTrue(summary.Passed);
        Assert.AreEqual(4, summary.PassedGates);
        Assert.AreEqual("REAL ASSEMBLY REWRITE WORKSPACE PASS — 4/4", summary.Summary);
    }

    [TestMethod]
    public void RealAssemblyRewriteGatesStopAfterFirstFailure()
    {
        var gates = new RealAssemblyRewriteGateSequence();
        gates.Record(RealAssemblyRewriteGate.WorkspaceClone, true, "clone");
        gates.Record(RealAssemblyRewriteGate.PrimaryRoundTrip, false, "failed");
        Assert.ThrowsExactly<InvalidOperationException>(() =>
            gates.Record(RealAssemblyRewriteGate.NeutralIlRewrite, true, "must not advance"));
        Assert.AreEqual(RealAssemblyRewriteGate.PrimaryRoundTrip, gates.Snapshot().FirstFailingGate);
    }

    [TestMethod]
    public async Task RealArm64AssemblyCopyRoundTripNeutralRewriteAndIsolationPass()
    {
        using var temp = new TempTestDirectory("sts2-step18");
        var managedPath = Path.Combine(temp.Path, SteamOfflineInstallInspection.ManagedRootRelativePath, "Depot-2868842");
        var arm64Relative = "SlayTheSpire2.app/Contents/Resources/data_sts2_macos_arm64/sts2.dll";
        var godotSharpRelative = "SlayTheSpire2.app/Contents/Resources/data_sts2_macos_arm64/godot-runtime-payload.dll";
        var systemRuntimeRelative = "SlayTheSpire2.app/Contents/Resources/data_sts2_macos_arm64/runtime-contract-payload.dll";
        var x86Relative = "SlayTheSpire2.app/Contents/Resources/data_sts2_macos_x86_64/sts2.dll";
        var sharedRelative = "SlayTheSpire2.app/Contents/Resources/shared-helper.dll";

        var arm64Path = Path.Combine(managedPath, arm64Relative.Replace('/', Path.DirectorySeparatorChar));
        var godotSharpPath = Path.Combine(managedPath, godotSharpRelative.Replace('/', Path.DirectorySeparatorChar));
        var systemRuntimePath = Path.Combine(managedPath, systemRuntimeRelative.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(arm64Path)!);
        // Create dependencies under conventional filenames only long enough to build the synthetic
        // primary assembly with Cecil's default resolver, then rename them. Gate B must resolve
        // GodotSharp by ASSEMBLY IDENTITY and must also permit the unambiguous System.Runtime
        // 8.0.0.0 -> 9.0.0.0 workspace version-unification case observed on the physical iPhone.
        var setupGodotSharpPath = Path.Combine(Path.GetDirectoryName(arm64Path)!, "GodotSharp.dll");
        var setupSystemRuntimePath = Path.Combine(Path.GetDirectoryName(arm64Path)!, "System.Runtime.dll");
        WriteSyntheticEnumDependencyAssembly(setupGodotSharpPath);
        WriteSyntheticRuntimeContractAssembly(setupSystemRuntimePath);
        WriteSyntheticAssemblyWithExternalEnumDefaults(arm64Path, Path.GetDirectoryName(arm64Path)!);
        File.Move(setupGodotSharpPath, godotSharpPath);
        File.Move(setupSystemRuntimePath, systemRuntimePath);

        var x86Path = Path.Combine(managedPath, x86Relative.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(x86Path)!);
        File.Copy(arm64Path, x86Path, overwrite: true);

        var sharedPath = Path.Combine(managedPath, sharedRelative.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(sharedPath)!);
        WriteSyntheticAssembly(sharedPath, "shared-helper");

        var files = new List<SteamManagedInstallFile>();
        foreach (var relative in new[] { arm64Relative, godotSharpRelative, systemRuntimeRelative, x86Relative, sharedRelative })
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
            777UL,
            "public",
            DateTimeOffset.UtcNow,
            files);
        await using (var stream = File.Create(Path.Combine(managedPath, SteamManagedInstallReceipt.FileName)))
        {
            await JsonSerializer.SerializeAsync(stream, receipt, SteamManagedInstallJsonContext.Default.SteamManagedInstallReceipt);
        }

        var installBefore = SHA256.HashData(await File.ReadAllBytesAsync(arm64Path));
        var workspace = new RealAssemblyRewriteWorkspace(temp.Path);
        var gateA = await workspace.RunWorkspaceCloneAsync();
        var gateB = workspace.RunPrimaryRoundTrip();
        var gateC = workspace.RunNeutralIlRewrite();
        var gateD = await workspace.RunIsolationAuditAsync();
        var installAfter = SHA256.HashData(await File.ReadAllBytesAsync(arm64Path));

        Assert.IsTrue(gateA.Passed, gateA.Detail);
        Assert.IsTrue(gateB.Passed, gateB.Detail);
        Assert.IsTrue(gateC.Passed, gateC.Detail);
        Assert.IsTrue(gateD.Passed, gateD.Detail);
        CollectionAssert.AreEqual(installBefore, installAfter);

        StringAssert.Contains(gateA.Detail, "macOS arm64 candidates copied: 3");
        StringAssert.Contains(gateA.Detail, "macOS x86_64 duplicates excluded from rewrite workspace: 1");
        StringAssert.Contains(gateB.Detail, "REAL StS2 assembly copy");
        StringAssert.Contains(gateB.Detail, "Logical metadata fingerprint preserved after write/reopen: YES");
        StringAssert.Contains(gateB.Detail, "Generated-output reopen resolver explicitly bound to workspace identity catalog: YES");
        StringAssert.Contains(gateB.Detail, "Generated-output verification uses deferred Cecil reading: YES");
        StringAssert.Contains(gateB.Detail, "Workspace-only dependency resolutions observed:");
        StringAssert.Contains(gateB.Detail, "GodotSharp, Version=4.5.10.0");
        StringAssert.Contains(gateB.Detail, "System.Runtime, Version=8.0.0.0");
        StringAssert.Contains(gateB.Detail, "System.Runtime, Version=9.0.0.0");
        StringAssert.Contains(gateB.Detail, "[workspace version-unified]");
        Assert.IsFalse(File.Exists(Path.Combine(Path.GetDirectoryName(arm64Path)!, "GodotSharp.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(Path.GetDirectoryName(arm64Path)!, "System.Runtime.dll")));
        StringAssert.Contains(gateB.Detail, "Fallback to runtime/system/live-install/network resolver paths: NO");
        StringAssert.Contains(gateC.Detail, "insert one IL NOP at method entry");
        StringAssert.Contains(gateC.Detail, "Generated-output reopen resolver explicitly bound to workspace identity catalog: YES");
        StringAssert.Contains(gateC.Detail, "Behaviorally significant game fix attempted: NO");
        StringAssert.Contains(gateD.Detail, "Original Step 12 install unchanged: YES");
        StringAssert.Contains(gateD.Detail, "Primary Cecil round-trip output reopens with explicit workspace resolver: YES");
        StringAssert.Contains(gateD.Detail, "Generated-output audit reopens use deferred Cecil reading: YES");
        StringAssert.Contains(gateD.Detail, "Every dependency resolution was constrained to Step18-RealAssemblyRewrite/source: YES");
        StringAssert.Contains(gateD.Detail, "Game assembly loaded/executed: NO");

        var sourceCopy = Path.Combine(
            temp.Path,
            RealAssemblyRewriteWorkspace.WorkRootName,
            RealAssemblyRewriteWorkspace.SourceRootName,
            arm64Relative.Replace('/', Path.DirectorySeparatorChar));
        var rewrittenCopy = Path.Combine(
            temp.Path,
            RealAssemblyRewriteWorkspace.WorkRootName,
            RealAssemblyRewriteWorkspace.RewrittenRootName,
            arm64Relative.Replace('/', Path.DirectorySeparatorChar));
        CollectionAssert.AreEqual(await File.ReadAllBytesAsync(arm64Path), await File.ReadAllBytesAsync(sourceCopy));
        var sourceBytes = await File.ReadAllBytesAsync(sourceCopy);
        var rewrittenBytes = await File.ReadAllBytesAsync(rewrittenCopy);
        Assert.IsFalse(sourceBytes.SequenceEqual(rewrittenBytes));

        using var rewritten = ModuleDefinition.ReadModule(rewrittenCopy, new ReaderParameters
        {
            ReadingMode = ReadingMode.Deferred,
            AssemblyResolver = RejectingTestResolver.Instance,
            MetadataResolver = RejectingTestResolver.Instance,
        });
        var target = rewritten.Types.SelectMany(type => type.Methods).Single(method => method.Name == "Target");
        Assert.AreEqual(Code.Nop, target.Body.Instructions[0].OpCode.Code);
        Assert.AreEqual(Code.Ldc_I4_7, target.Body.Instructions[1].OpCode.Code);
    }

    private static void WriteSyntheticEnumDependencyAssembly(string path)
    {
        using var assembly = AssemblyDefinition.CreateAssembly(
            new AssemblyNameDefinition("GodotSharp", new Version(4, 5, 10, 0)),
            "GodotSharp",
            ModuleKind.Dll);
        var module = assembly.MainModule;
        var enumType = new TypeDefinition(
            "Synthetic.Dependency",
            "ExternalMode",
            TypeAttributes.Public | TypeAttributes.Sealed,
            module.ImportReference(typeof(Enum)));
        module.Types.Add(enumType);
        enumType.Fields.Add(new FieldDefinition(
            "value__",
            FieldAttributes.Public | FieldAttributes.SpecialName | FieldAttributes.RTSpecialName,
            module.TypeSystem.Int32));
        enumType.Fields.Add(new FieldDefinition(
            "One",
            FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.Literal | FieldAttributes.HasDefault,
            enumType)
        {
            Constant = 1,
        });
        assembly.Write(path);
    }

    private static void WriteSyntheticRuntimeContractAssembly(string path)
    {
        // Mono.Cecil treats assemblies named System.Runtime as core libraries. For a newly-created
        // module there is no metadata image/reader yet, so asking that CoreTypeSystem for Int32
        // can dereference a null reader. Construct the fixture under a temporary non-core identity
        // so Cecil uses its normal CommonTypeSystem, then switch only the final assembly identity
        // to the exact System.Runtime 9.0.0.0 identity needed by the version-unification regression.
        using var assembly = AssemblyDefinition.CreateAssembly(
            new AssemblyNameDefinition("Synthetic.SystemRuntimeFixture", new Version(9, 0, 0, 0)),
            "System.Runtime",
            ModuleKind.Dll);
        var module = assembly.MainModule;
        var enumBase = new TypeReference(
            "System",
            "Enum",
            module,
            module.TypeSystem.CoreLibrary);
        var enumType = new TypeDefinition(
            "Synthetic.Runtime",
            "RuntimeMode",
            TypeAttributes.Public | TypeAttributes.Sealed,
            enumBase);
        module.Types.Add(enumType);
        enumType.Fields.Add(new FieldDefinition(
            "value__",
            FieldAttributes.Public | FieldAttributes.SpecialName | FieldAttributes.RTSpecialName,
            module.TypeSystem.Int32));
        enumType.Fields.Add(new FieldDefinition(
            "One",
            FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.Literal | FieldAttributes.HasDefault,
            enumType)
        {
            Constant = 1,
        });

        assembly.Name.Name = "System.Runtime";
        assembly.Name.Version = new Version(9, 0, 0, 0);
        assembly.Write(path);
    }

    private static void WriteSyntheticAssemblyWithExternalEnumDefaults(string path, string dependencyDirectory)
    {
        using var setupResolver = new DefaultAssemblyResolver();
        setupResolver.AddSearchDirectory(dependencyDirectory);
        using var assembly = AssemblyDefinition.CreateAssembly(
            new AssemblyNameDefinition("sts2", new Version(1, 2, 3, 4)),
            "sts2",
            new ModuleParameters
            {
                Kind = ModuleKind.Dll,
                AssemblyResolver = setupResolver,
            });
        var module = assembly.MainModule;
        var godotReference = new AssemblyNameReference("GodotSharp", new Version(4, 5, 10, 0));
        var runtimeReference = new AssemblyNameReference("System.Runtime", new Version(8, 0, 0, 0));
        module.AssemblyReferences.Add(godotReference);
        module.AssemblyReferences.Add(runtimeReference);
        // Mono.Cecil 0.11.6 exposes the value-type flag as the fifth positional
        // TypeReference constructor argument. Keep this positional so the host
        // regression test compiles against the exact pinned Cecil API.
        var godotEnum = new TypeReference(
            "Synthetic.Dependency",
            "ExternalMode",
            module,
            godotReference,
            true);
        var runtimeEnum = new TypeReference(
            "Synthetic.Runtime",
            "RuntimeMode",
            module,
            runtimeReference,
            true);

        var type = new TypeDefinition("Synthetic", "GameType", TypeAttributes.Public | TypeAttributes.Class, module.TypeSystem.Object);
        module.Types.Add(type);
        var target = new MethodDefinition("Target", MethodAttributes.Public | MethodAttributes.Static, module.TypeSystem.Int32);
        target.Parameters.Add(new ParameterDefinition(
            "mode",
            ParameterAttributes.Optional | ParameterAttributes.HasDefault,
            godotEnum)
        {
            Constant = 1,
        });
        target.Parameters.Add(new ParameterDefinition(
            "runtimeMode",
            ParameterAttributes.Optional | ParameterAttributes.HasDefault,
            runtimeEnum)
        {
            Constant = 1,
        });
        type.Methods.Add(target);
        var il = target.Body.GetILProcessor();
        il.Append(il.Create(OpCodes.Ldc_I4_7));
        il.Append(il.Create(OpCodes.Ret));
        assembly.Write(path);
    }

    private static void WriteSyntheticAssembly(string path, string assemblyName)
    {
        using var assembly = AssemblyDefinition.CreateAssembly(
            new AssemblyNameDefinition(assemblyName, new Version(1, 2, 3, 4)),
            assemblyName,
            ModuleKind.Dll);
        var type = new TypeDefinition("Synthetic", "GameType", TypeAttributes.Public | TypeAttributes.Class, assembly.MainModule.TypeSystem.Object);
        assembly.MainModule.Types.Add(type);
        var target = new MethodDefinition("Target", MethodAttributes.Public | MethodAttributes.Static, assembly.MainModule.TypeSystem.Int32);
        type.Methods.Add(target);
        var il = target.Body.GetILProcessor();
        il.Append(il.Create(OpCodes.Ldc_I4_7));
        il.Append(il.Create(OpCodes.Ret));
        assembly.Write(path);
    }


    private sealed class RejectingTestResolver : IAssemblyResolver, IMetadataResolver
    {
        public static RejectingTestResolver Instance { get; } = new();
        private RejectingTestResolver() { }
        public AssemblyDefinition Resolve(AssemblyNameReference name)
            => throw new AssertFailedException($"Verification-only test unexpectedly resolved assembly {name.FullName}.");
        public AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
            => Resolve(name);
        TypeDefinition IMetadataResolver.Resolve(TypeReference type)
            => throw new AssertFailedException($"Verification-only test unexpectedly resolved type {type.FullName}.");
        FieldDefinition IMetadataResolver.Resolve(FieldReference field)
            => throw new AssertFailedException($"Verification-only test unexpectedly resolved field {field.FullName}.");
        MethodDefinition IMetadataResolver.Resolve(MethodReference method)
            => throw new AssertFailedException($"Verification-only test unexpectedly resolved method {method.FullName}.");
        public void Dispose() { }
    }
}
