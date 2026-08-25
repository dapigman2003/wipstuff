using System.Security.Cryptography;
using System.Text.Json;
using Mono.Cecil;
using Mono.Cecil.Cil;
using StS2Launcher.Core;

namespace StS2Launcher.Core.Tests;

[TestClass]
public sealed class RealStS2CompatibilityTargetAuditTests
{
    [TestMethod]
    public void OrderedRealStS2TargetAuditGatesReachFourOfFourPass()
    {
        var gates = new RealStS2CompatibilityTargetAuditGateSequence();
        gates.Record(RealStS2CompatibilityTargetAuditGate.SourceAdmissionAndOfflineReady, true, "source");
        gates.Record(RealStS2CompatibilityTargetAuditGate.ExactRiskCallSiteAudit, true, "audit");
        gates.Record(RealStS2CompatibilityTargetAuditGate.DeterministicCandidateSelection, true, "selection");
        gates.Record(RealStS2CompatibilityTargetAuditGate.FinalIsolationAudit, true, "isolation");

        var summary = gates.Snapshot();
        Assert.IsTrue(summary.Passed);
        Assert.AreEqual(4, summary.Gates.Count);
        Assert.AreEqual("REAL STS2 COMPATIBILITY TARGET AUDIT PASS — 4/4", summary.Summary);
    }

    [TestMethod]
    public void RealStS2TargetAuditStopsAfterFirstFailure()
    {
        var gates = new RealStS2CompatibilityTargetAuditGateSequence();
        gates.Record(RealStS2CompatibilityTargetAuditGate.SourceAdmissionAndOfflineReady, true, "source");
        gates.Record(RealStS2CompatibilityTargetAuditGate.ExactRiskCallSiteAudit, false, "failed");
        Assert.ThrowsExactly<InvalidOperationException>(() =>
            gates.Record(RealStS2CompatibilityTargetAuditGate.DeterministicCandidateSelection, true, "must not advance"));
        Assert.AreEqual(RealStS2CompatibilityTargetAuditGate.ExactRiskCallSiteAudit, gates.Snapshot().FirstFailingGate);
    }

    [TestMethod]
    public void SelectionPriorityKeepsRetiredRuntimeDetoursAheadOfLaterIntegrationSurfaces()
    {
        Assert.IsTrue(RealStS2CompatibilityTargetAudit.PriorityForCategory("HarmonyRuntimePatch") <
                      RealStS2CompatibilityTargetAudit.PriorityForCategory("MonoModRuntimeDetour"));
        Assert.IsTrue(RealStS2CompatibilityTargetAudit.PriorityForCategory("MonoModRuntimeDetour") <
                      RealStS2CompatibilityTargetAudit.PriorityForCategory("ReflectionEmit"));
        Assert.IsTrue(RealStS2CompatibilityTargetAudit.PriorityForCategory("DynamicAssemblyLoad") <
                      RealStS2CompatibilityTargetAudit.PriorityForCategory("NativeLibrary"));
        Assert.IsTrue(RealStS2CompatibilityTargetAudit.PriorityForCategory("NativeLibrary") <
                      RealStS2CompatibilityTargetAudit.PriorityForCategory("IndirectCalli"));
    }

    [TestMethod]
    public async Task ReceiptBackedPrimaryAuditSelectsExactHarmonyRuntimePatchWithoutMutationOrClrLoad()
    {
        using var temp = new TempTestDirectory("sts2-step29");
        var managedPath = Path.Combine(temp.Path, SteamOfflineInstallInspection.ManagedRootRelativePath, "Depot-2868842");
        var primaryRelative = "SlayTheSpire2.app/Contents/Resources/data_sts2_macos_arm64/sts2.dll";
        var primaryPath = Path.Combine(managedPath, primaryRelative.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(primaryPath)!);
        WriteSyntheticPrimaryAssembly(primaryPath);

        var bytes = await File.ReadAllBytesAsync(primaryPath);
        var receipt = new SteamManagedInstallReceipt(
            SteamManagedInstallReceipt.CurrentSchemaVersion,
            2868840,
            2868842,
            123456UL,
            "public",
            DateTimeOffset.UtcNow,
            [new SteamManagedInstallFile(
                primaryRelative,
                bytes.LongLength,
                Convert.ToHexString(SHA1.HashData(bytes)).ToLowerInvariant())]);
        await using (var stream = File.Create(Path.Combine(managedPath, SteamManagedInstallReceipt.FileName)))
        {
            await JsonSerializer.SerializeAsync(stream, receipt, SteamManagedInstallJsonContext.Default.SteamManagedInstallReceipt);
        }

        var before = SHA256.HashData(await File.ReadAllBytesAsync(primaryPath));
        var audit = new RealStS2CompatibilityTargetAudit(temp.Path);
        var gateA = await audit.RunSourceAdmissionAndOfflineReadyAsync();
        var gateB = audit.RunExactRiskCallSiteAudit();
        var gateC = audit.RunDeterministicCandidateSelection();
        var gateD = await audit.RunFinalIsolationAuditAsync();
        var after = SHA256.HashData(await File.ReadAllBytesAsync(primaryPath));

        Assert.IsTrue(gateA.Passed, gateA.Detail);
        Assert.IsTrue(gateB.Passed, gateB.Detail);
        Assert.IsTrue(gateC.Passed, gateC.Detail);
        Assert.IsTrue(gateD.Passed, gateD.Detail);
        CollectionAssert.AreEqual(before, after);

        StringAssert.Contains(gateA.Detail, "Cecil dependency resolution requests: 0");
        StringAssert.Contains(gateA.Detail, "sts2 CLR-loaded before/after Gate A: NO / NO");
        StringAssert.Contains(gateB.Detail, "HarmonyRuntimePatch=1");
        StringAssert.Contains(gateB.Detail, "Expression.Compile sites excluded from Step-29 candidacy by physically closed Step 19 policy: 1");
        StringAssert.Contains(gateC.Detail, "Category: HarmonyRuntimePatch");
        StringAssert.Contains(gateC.Detail, "HarmonyLib.Harmony::PatchAll");
        StringAssert.Contains(gateC.Detail, "Authorization: AUDIT ONLY");
        StringAssert.Contains(gateD.Detail, "Trusted Step 12 managed install unchanged: YES");
        StringAssert.Contains(gateD.Detail, "Cecil writes performed by Step 29: 0");
        StringAssert.Contains(gateD.Detail, "sts2 assembly/type/member CLR load or invocation by Step 29: NO");
    }

    private static void WriteSyntheticPrimaryAssembly(string path)
    {
        using var assembly = AssemblyDefinition.CreateAssembly(
            new AssemblyNameDefinition("sts2", new Version(1, 0, 0, 0)),
            "sts2",
            ModuleKind.Dll);
        var module = assembly.MainModule;
        var type = new TypeDefinition("Synthetic", "Bootstrap", TypeAttributes.Public | TypeAttributes.Class, module.TypeSystem.Object);
        module.Types.Add(type);

        var harmonyAssembly = new AssemblyNameReference("0Harmony", new Version(2, 4, 2, 0));
        var expressionAssembly = new AssemblyNameReference("System.Linq.Expressions", new Version(9, 0, 0, 0));
        var diagnosticsAssembly = new AssemblyNameReference("System.Diagnostics.Process", new Version(9, 0, 0, 0));
        module.AssemblyReferences.Add(harmonyAssembly);
        module.AssemblyReferences.Add(expressionAssembly);
        module.AssemblyReferences.Add(diagnosticsAssembly);

        var harmonyType = new TypeReference("HarmonyLib", "Harmony", module, harmonyAssembly);
        var patchAll = new MethodReference("PatchAll", module.TypeSystem.Void, harmonyType) { HasThis = true };
        var lambdaType = new TypeReference("System.Linq.Expressions", "LambdaExpression", module, expressionAssembly);
        var compile = new MethodReference("Compile", module.TypeSystem.Object, lambdaType) { HasThis = true };
        var processType = new TypeReference("System.Diagnostics", "Process", module, diagnosticsAssembly);
        var processStart = new MethodReference("Start", module.TypeSystem.Object, processType) { HasThis = false };
        processStart.Parameters.Add(new ParameterDefinition(module.TypeSystem.String));

        var method = new MethodDefinition("Run", MethodAttributes.Public | MethodAttributes.Static, module.TypeSystem.Void);
        type.Methods.Add(method);
        var il = method.Body.GetILProcessor();
        il.Append(il.Create(OpCodes.Ldnull));
        il.Append(il.Create(OpCodes.Callvirt, patchAll));
        il.Append(il.Create(OpCodes.Ldnull));
        il.Append(il.Create(OpCodes.Callvirt, compile));
        il.Append(il.Create(OpCodes.Pop));
        il.Append(il.Create(OpCodes.Ldstr, "ignored"));
        il.Append(il.Create(OpCodes.Call, processStart));
        il.Append(il.Create(OpCodes.Pop));
        il.Append(il.Create(OpCodes.Ret));

        assembly.Write(path);
    }
}
