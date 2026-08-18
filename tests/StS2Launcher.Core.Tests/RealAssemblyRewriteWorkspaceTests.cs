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
        using var temp = new TemporaryDirectory();
        var managedPath = Path.Combine(temp.Path, SteamOfflineInstallInspection.ManagedRootRelativePath, "Depot-2868842");
        var arm64Relative = "SlayTheSpire2.app/Contents/Resources/data_sts2_macos_arm64/sts2.dll";
        var x86Relative = "SlayTheSpire2.app/Contents/Resources/data_sts2_macos_x86_64/sts2.dll";
        var sharedRelative = "SlayTheSpire2.app/Contents/Resources/shared-helper.dll";

        var arm64Path = Path.Combine(managedPath, arm64Relative.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(arm64Path)!);
        WriteSyntheticAssembly(arm64Path, "sts2");

        var x86Path = Path.Combine(managedPath, x86Relative.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(x86Path)!);
        File.Copy(arm64Path, x86Path, overwrite: true);

        var sharedPath = Path.Combine(managedPath, sharedRelative.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(sharedPath)!);
        WriteSyntheticAssembly(sharedPath, "shared-helper");

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

        StringAssert.Contains(gateA.Detail, "macOS arm64 candidates copied: 1");
        StringAssert.Contains(gateA.Detail, "macOS x86_64 duplicates excluded from rewrite workspace: 1");
        StringAssert.Contains(gateB.Detail, "REAL StS2 assembly copy");
        StringAssert.Contains(gateB.Detail, "Logical metadata fingerprint preserved after write/reopen: YES");
        StringAssert.Contains(gateC.Detail, "insert one IL NOP at method entry");
        StringAssert.Contains(gateC.Detail, "Behaviorally significant game fix attempted: NO");
        StringAssert.Contains(gateD.Detail, "Original Step 12 install unchanged: YES");
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

        using var rewritten = ModuleDefinition.ReadModule(rewrittenCopy, new ReaderParameters { ReadingMode = ReadingMode.Immediate });
        var target = rewritten.Types.SelectMany(type => type.Methods).Single(method => method.Name == "Target");
        Assert.AreEqual(Code.Nop, target.Body.Instructions[0].OpCode.Code);
        Assert.AreEqual(Code.Ldc_I4_7, target.Body.Instructions[1].OpCode.Code);
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

    private sealed class TemporaryDirectory : IDisposable
    {
        public TemporaryDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "sts2-step18-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(Path))
                    Directory.Delete(Path, recursive: true);
            }
            catch
            {
                // Best-effort test cleanup only.
            }
        }
    }
}
