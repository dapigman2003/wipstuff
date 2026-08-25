using System.Security.Cryptography;
using System.Text.Json;
using Mono.Cecil;
using Mono.Cecil.Cil;
using StS2Launcher.Core;

namespace StS2Launcher.Core.Tests;

[TestClass]
public sealed class RealStS2PrepareMethodSemanticAuditTests
{
    [TestMethod]
    public void OrderedPrepareMethodSemanticAuditGatesReachFourOfFourPass()
    {
        var gates = new RealStS2PrepareMethodSemanticAuditGateSequence();
        gates.Record(RealStS2PrepareMethodSemanticAuditGate.EvidenceBindingAndOfflineReady, true, "source");
        gates.Record(RealStS2PrepareMethodSemanticAuditGate.ExactPrepareMethodSemanticContextAudit, true, "context");
        gates.Record(RealStS2PrepareMethodSemanticAuditGate.DeterministicDisposition, true, "disposition");
        gates.Record(RealStS2PrepareMethodSemanticAuditGate.FinalIsolationAudit, true, "isolation");
        var summary = gates.Snapshot();
        Assert.IsTrue(summary.Passed);
        Assert.AreEqual(4, summary.Gates.Count);
        Assert.AreEqual("PREPAREMETHOD SEMANTIC CONTEXT AUDIT PASS — 4/4", summary.Summary);
    }

    [TestMethod]
    public void PrepareMethodSemanticAuditStopsAfterFirstFailure()
    {
        var gates = new RealStS2PrepareMethodSemanticAuditGateSequence();
        gates.Record(RealStS2PrepareMethodSemanticAuditGate.EvidenceBindingAndOfflineReady, true, "source");
        gates.Record(RealStS2PrepareMethodSemanticAuditGate.ExactPrepareMethodSemanticContextAudit, false, "failed");
        Assert.ThrowsExactly<InvalidOperationException>(() =>
            gates.Record(RealStS2PrepareMethodSemanticAuditGate.DeterministicDisposition, true, "must not advance"));
        Assert.AreEqual(RealStS2PrepareMethodSemanticAuditGate.ExactPrepareMethodSemanticContextAudit, gates.Snapshot().FirstFailingGate);
    }

    [TestMethod]
    public async Task ExactPrewarmJitPrepareMethodFamilyIsAuditedWithoutMutationOrClrLoad()
    {
        using var temp = new TempTestDirectory("sts2-step31");
        var managedPath = Path.Combine(temp.Path, SteamOfflineInstallInspection.ManagedRootRelativePath, "Depot-2868842");
        const string primaryRelative = "SlayTheSpire2.app/Contents/Resources/data_sts2_macos_arm64/sts2.dll";
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
        var audit = new RealStS2PrepareMethodSemanticAudit(temp.Path, evidence);
        var gateA = await audit.RunEvidenceBindingAndOfflineReadyAsync();
        var gateB = audit.RunExactPrepareMethodSemanticContextAudit();
        var gateC = audit.RunDeterministicDisposition();
        var gateD = await audit.RunFinalIsolationAuditAsync();
        var after = SHA256.HashData(await File.ReadAllBytesAsync(primaryPath));

        Assert.IsTrue(gateA.Passed, gateA.Detail);
        Assert.IsTrue(gateB.Passed, gateB.Detail);
        Assert.IsTrue(gateC.Passed, gateC.Detail);
        Assert.IsTrue(gateD.Passed, gateD.Detail);
        CollectionAssert.AreEqual(before, after);
        StringAssert.Contains(gateA.Detail, "Expected PrepareMethod sites rebound: 10/10");
        StringAssert.Contains(gateA.Detail, "Cecil dependency resolution requests: 0");
        StringAssert.Contains(gateB.Detail, "PrepareMethod sites: 10");
        StringAssert.Contains(gateB.Detail, "All PrepareMethod sites are direct Call: YES");
        StringAssert.Contains(gateB.Detail, "Real StS2 CLR load/invocation: NO");
        StringAssert.Contains(gateC.Detail, "ELIGIBLE FOR EXPLICIT REWRITE DESIGN; NO WRITE AUTHORIZED");
        StringAssert.Contains(gateC.Detail, "Predeclared behavior change for Step 31: NONE");
        StringAssert.Contains(gateD.Detail, "Real-game rewrite authorized by Step 31: NO");
        StringAssert.Contains(gateD.Detail, "Cecil writes performed by Step 31: 0");
    }

    private static RealStS2PrepareMethodSemanticAudit.PrepareMethodEvidence BuildEvidence(string path)
    {
        var raw = File.ReadAllBytes(path);
        using var module = ModuleDefinition.ReadModule(path, new ReaderParameters { ReadSymbols = false, ReadingMode = ReadingMode.Deferred });
        var method = module.Types.SelectMany(EnumerateTypes).SelectMany(type => type.Methods)
            .Single(value => value.DeclaringType.FullName == "MegaCrit.Sts2.Core.Helpers.OneTimeInitialization" && value.Name == "PrewarmJit");
        var sites = method.Body.Instructions
            .Where(instruction => instruction.Operand is MethodReference reference &&
                                  reference.DeclaringType.FullName == "System.Runtime.CompilerServices.RuntimeHelpers" &&
                                  reference.Name == "PrepareMethod")
            .Select(instruction =>
            {
                var target = (MethodReference)instruction.Operand;
                var scope = ((AssemblyNameReference)target.DeclaringType.Scope).Name;
                return new RealStS2PrepareMethodSemanticAudit.PrepareMethodCallSiteEvidence(
                    instruction.Offset,
                    instruction.OpCode.Code.ToString(),
                    scope,
                    target.FullName);
            })
            .ToArray();
        return new RealStS2PrepareMethodSemanticAudit.PrepareMethodEvidence(
            Convert.ToHexString(SHA1.HashData(raw)).ToLowerInvariant(),
            Convert.ToHexString(SHA256.HashData(raw)).ToLowerInvariant(),
            raw.LongLength,
            module.Assembly.Name.FullName,
            module.Mvid,
            method.DeclaringType.FullName,
            method.FullName,
            method.MetadataToken.ToUInt32(),
            RealStS2PrepareMethodSemanticAudit.ComputeMethodBodyFingerprint(method),
            sites);
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
        var type = new TypeDefinition(
            "MegaCrit.Sts2.Core.Helpers",
            "OneTimeInitialization",
            TypeAttributes.Public | TypeAttributes.Abstract | TypeAttributes.Sealed,
            module.TypeSystem.Object);
        module.Types.Add(type);

        var runtimeAssembly = new AssemblyNameReference("System.Runtime", new Version(9, 0, 0, 0));
        module.AssemblyReferences.Add(runtimeAssembly);
        var runtimeHelpers = new TypeReference("System.Runtime.CompilerServices", "RuntimeHelpers", module, runtimeAssembly);
        var runtimeMethodHandle = new TypeReference("System", "RuntimeMethodHandle", module, runtimeAssembly, true);
        var runtimeTypeHandle = new TypeReference("System", "RuntimeTypeHandle", module, runtimeAssembly, true);
        var runtimeTypeHandleArray = new ArrayType(runtimeTypeHandle);
        var prepareOne = new MethodReference("PrepareMethod", module.TypeSystem.Void, runtimeHelpers) { HasThis = false };
        prepareOne.Parameters.Add(new ParameterDefinition(runtimeMethodHandle));
        var prepareTwo = new MethodReference("PrepareMethod", module.TypeSystem.Void, runtimeHelpers) { HasThis = false };
        prepareTwo.Parameters.Add(new ParameterDefinition(runtimeMethodHandle));
        prepareTwo.Parameters.Add(new ParameterDefinition(runtimeTypeHandleArray));

        var method = new MethodDefinition("PrewarmJit", MethodAttributes.Public | MethodAttributes.Static, module.TypeSystem.Void);
        type.Methods.Add(method);
        method.Body.InitLocals = true;
        var handle = new VariableDefinition(runtimeMethodHandle);
        method.Body.Variables.Add(handle);
        var il = method.Body.GetILProcessor();
        il.Append(il.Create(OpCodes.Ldstr, "synthetic prewarm"));
        il.Append(il.Create(OpCodes.Pop));
        for (var i = 0; i < 10; i++)
        {
            il.Append(il.Create(OpCodes.Ldloc, handle));
            if (i is >= 2 and <= 5)
            {
                il.Append(il.Create(OpCodes.Ldnull));
                il.Append(il.Create(OpCodes.Call, prepareTwo));
            }
            else
            {
                il.Append(il.Create(OpCodes.Call, prepareOne));
            }
        }
        il.Append(il.Create(OpCodes.Ret));
        assembly.Write(path);
    }
}
