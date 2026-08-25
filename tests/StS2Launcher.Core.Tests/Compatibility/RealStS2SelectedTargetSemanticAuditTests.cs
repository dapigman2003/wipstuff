using System.Security.Cryptography;
using System.Text.Json;
using Mono.Cecil;
using Mono.Cecil.Cil;
using StS2Launcher.Core;

namespace StS2Launcher.Core.Tests;

[TestClass]
public sealed class RealStS2SelectedTargetSemanticAuditTests
{
    [TestMethod]
    public void OrderedSelectedTargetSemanticAuditGatesReachFourOfFourPass()
    {
        var gates = new RealStS2SelectedTargetSemanticAuditGateSequence();
        gates.Record(RealStS2SelectedTargetSemanticAuditGate.SelectedEvidenceBindingAndOfflineReady, true, "source");
        gates.Record(RealStS2SelectedTargetSemanticAuditGate.ExactSemanticContextAudit, true, "context");
        gates.Record(RealStS2SelectedTargetSemanticAuditGate.DeterministicDisposition, true, "disposition");
        gates.Record(RealStS2SelectedTargetSemanticAuditGate.FinalIsolationAudit, true, "isolation");
        var summary = gates.Snapshot();
        Assert.IsTrue(summary.Passed);
        Assert.AreEqual(4, summary.Gates.Count);
        Assert.AreEqual("SELECTED TARGET SEMANTIC CONTEXT AUDIT PASS — 4/4", summary.Summary);
    }

    [TestMethod]
    public void SelectedTargetSemanticAuditStopsAfterFirstFailure()
    {
        var gates = new RealStS2SelectedTargetSemanticAuditGateSequence();
        gates.Record(RealStS2SelectedTargetSemanticAuditGate.SelectedEvidenceBindingAndOfflineReady, true, "source");
        gates.Record(RealStS2SelectedTargetSemanticAuditGate.ExactSemanticContextAudit, false, "failed");
        Assert.ThrowsExactly<InvalidOperationException>(() =>
            gates.Record(RealStS2SelectedTargetSemanticAuditGate.DeterministicDisposition, true, "must not advance"));
        Assert.AreEqual(RealStS2SelectedTargetSemanticAuditGate.ExactSemanticContextAudit, gates.Snapshot().FirstFailingGate);
    }

    [TestMethod]
    public async Task ExactModManagerPatchAllEvidenceIsAuditedThenDeferredWithoutMutationOrClrLoad()
    {
        using var temp = new TempTestDirectory("sts2-step30");
        var managedPath = Path.Combine(temp.Path, SteamOfflineInstallInspection.ManagedRootRelativePath, "Depot-2868842");
        var primaryRelative = "SlayTheSpire2.app/Contents/Resources/data_sts2_macos_arm64/sts2.dll";
        var primaryPath = Path.Combine(managedPath, primaryRelative.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(primaryPath)!);
        WriteSyntheticPrimaryAssembly(primaryPath);
        var evidence = BuildEvidence(primaryPath);

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
        var audit = new RealStS2SelectedTargetSemanticAudit(temp.Path, evidence);
        var gateA = await audit.RunSelectedEvidenceBindingAndOfflineReadyAsync();
        var gateB = audit.RunExactSemanticContextAudit();
        var gateC = audit.RunDeterministicDisposition();
        var gateD = await audit.RunFinalIsolationAuditAsync();
        var after = SHA256.HashData(await File.ReadAllBytesAsync(primaryPath));

        Assert.IsTrue(gateA.Passed, gateA.Detail);
        Assert.IsTrue(gateB.Passed, gateB.Detail);
        Assert.IsTrue(gateC.Passed, gateC.Detail);
        Assert.IsTrue(gateD.Passed, gateD.Detail);
        CollectionAssert.AreEqual(before, after);
        StringAssert.Contains(gateA.Detail, "Selected source method: System.Void MegaCrit.Sts2.Core.Modding.ModManager::TryLoadMod(MegaCrit.Sts2.Core.Modding.Mod)");
        StringAssert.Contains(gateA.Detail, "Cecil dependency resolution requests: 0");
        StringAssert.Contains(gateB.Detail, "Structurally scoped to ModManager.TryLoadMod(Mod): YES");
        StringAssert.Contains(gateB.Detail, "HarmonyLib.Harmony::PatchAll(System.Reflection.Assembly)");
        StringAssert.Contains(gateB.Detail, "Real StS2 CLR load/invocation: NO");
        StringAssert.Contains(gateC.Detail, "DEFER — MOD/HARMONY COMPATIBILITY PATH; NO BASE-GAME REWRITE AUTHORIZED");
        StringAssert.Contains(gateC.Detail, "Predeclared behavior change for this selected site: NONE");
        StringAssert.Contains(gateD.Detail, "Real-game rewrite authorized by Step 30: NO");
        StringAssert.Contains(gateD.Detail, "Cecil writes performed by Step 30: 0");
    }

    private static RealStS2SelectedTargetSemanticAudit.SelectedTargetEvidence BuildEvidence(string path)
    {
        var raw = File.ReadAllBytes(path);
        using var module = ModuleDefinition.ReadModule(path, new ReaderParameters { ReadSymbols = false, ReadingMode = ReadingMode.Deferred });
        var method = module.Types.SelectMany(EnumerateTypes).SelectMany(type => type.Methods)
            .Single(value => value.DeclaringType.FullName == "MegaCrit.Sts2.Core.Modding.ModManager" && value.Name == "TryLoadMod");
        var selected = method.Body.Instructions.Single(instruction =>
            instruction.Operand is MethodReference target &&
            target.FullName == "System.Void HarmonyLib.Harmony::PatchAll(System.Reflection.Assembly)");
        var target = (MethodReference)selected.Operand;
        return new RealStS2SelectedTargetSemanticAudit.SelectedTargetEvidence(
            Convert.ToHexString(SHA1.HashData(raw)).ToLowerInvariant(),
            Convert.ToHexString(SHA256.HashData(raw)).ToLowerInvariant(),
            raw.LongLength,
            module.Assembly.Name.FullName,
            module.Mvid,
            method.DeclaringType.FullName,
            method.FullName,
            method.MetadataToken.ToUInt32(),
            selected.Offset,
            selected.OpCode.Code.ToString(),
            ((AssemblyNameReference)target.DeclaringType.Scope).Name,
            target.FullName,
            RealStS2SelectedTargetSemanticAudit.ComputeMethodBodyFingerprint(method));
    }

    private static IEnumerable<TypeDefinition> EnumerateTypes(TypeDefinition root)
    {
        yield return root;
        foreach (var nested in root.NestedTypes.SelectMany(EnumerateTypes))
            yield return nested;
    }

    private static void WriteSyntheticPrimaryAssembly(string path)
    {
        using var assembly = AssemblyDefinition.CreateAssembly(
            new AssemblyNameDefinition("sts2", new Version(0, 1, 0, 0)),
            "sts2",
            ModuleKind.Dll);
        var module = assembly.MainModule;
        var modType = new TypeDefinition("MegaCrit.Sts2.Core.Modding", "Mod", TypeAttributes.Public | TypeAttributes.Class, module.TypeSystem.Object);
        var managerType = new TypeDefinition("MegaCrit.Sts2.Core.Modding", "ModManager", TypeAttributes.Public | TypeAttributes.Class, module.TypeSystem.Object);
        module.Types.Add(modType);
        module.Types.Add(managerType);

        var harmonyAssembly = new AssemblyNameReference("0Harmony", new Version(2, 4, 2, 0));
        var runtimeAssembly = new AssemblyNameReference("System.Runtime", new Version(9, 0, 0, 0));
        module.AssemblyReferences.Add(harmonyAssembly);
        module.AssemblyReferences.Add(runtimeAssembly);
        var harmonyType = new TypeReference("HarmonyLib", "Harmony", module, harmonyAssembly);
        var assemblyType = new TypeReference("System.Reflection", "Assembly", module, runtimeAssembly);
        var patchAll = new MethodReference("PatchAll", module.TypeSystem.Void, harmonyType) { HasThis = true };
        patchAll.Parameters.Add(new ParameterDefinition(assemblyType));

        var method = new MethodDefinition("TryLoadMod", MethodAttributes.Public, module.TypeSystem.Void);
        method.Parameters.Add(new ParameterDefinition("mod", ParameterAttributes.None, modType));
        managerType.Methods.Add(method);
        var il = method.Body.GetILProcessor();
        il.Append(il.Create(OpCodes.Nop));
        il.Append(il.Create(OpCodes.Ldstr, "synthetic mod path"));
        il.Append(il.Create(OpCodes.Pop));
        il.Append(il.Create(OpCodes.Ldnull));
        il.Append(il.Create(OpCodes.Ldnull));
        il.Append(il.Create(OpCodes.Callvirt, patchAll));
        il.Append(il.Create(OpCodes.Ret));
        assembly.Write(path);
    }
}
