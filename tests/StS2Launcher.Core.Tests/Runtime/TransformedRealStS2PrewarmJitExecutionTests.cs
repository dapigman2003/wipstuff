using System.Reflection;
using System.Runtime.Loader;
using System.Security.Cryptography;
using Mono.Cecil;
using Mono.Cecil.Cil;
using StS2Launcher.Core;

namespace StS2Launcher.Core.Tests;

[TestClass]
public sealed class TransformedRealStS2PrewarmJitExecutionTests
{
    [TestMethod]
    public void OrderedExecutionGatesReachFourOfFourPass()
    {
        var gates = new TransformedRealStS2PrewarmJitExecutionGateSequence();
        gates.Record(new(TransformedRealStS2PrewarmJitExecutionGate.VerifiedExecutionPreflight, true, "preflight"));
        gates.Record(new(TransformedRealStS2PrewarmJitExecutionGate.ExecutionCapableClrAdmission, true, "admission"));
        gates.Record(new(TransformedRealStS2PrewarmJitExecutionGate.ExactPrewarmJitInvocation, true, "invoke"));
        gates.Record(new(TransformedRealStS2PrewarmJitExecutionGate.FinalIsolationAudit, true, "isolation"));

        var summary = gates.Snapshot();
        Assert.IsTrue(summary.Passed);
        Assert.AreEqual(4, summary.Gates.Count);
        Assert.AreEqual("TRANSFORMED REAL STS2 PREWARMJIT EXECUTION PASS — 4/4", summary.Summary);
    }

    [TestMethod]
    public void ExecutionStopsAfterFirstFailure()
    {
        var gates = new TransformedRealStS2PrewarmJitExecutionGateSequence();
        gates.Record(new(TransformedRealStS2PrewarmJitExecutionGate.VerifiedExecutionPreflight, true, "preflight"));
        gates.Record(new(TransformedRealStS2PrewarmJitExecutionGate.ExecutionCapableClrAdmission, true, "admission"));
        gates.Record(new(TransformedRealStS2PrewarmJitExecutionGate.ExactPrewarmJitInvocation, false, "failed"));

        Assert.ThrowsExactly<InvalidOperationException>(() =>
            gates.Record(new(TransformedRealStS2PrewarmJitExecutionGate.FinalIsolationAudit, true, "must not advance")));
        Assert.AreEqual(TransformedRealStS2PrewarmJitExecutionGate.ExactPrewarmJitInvocation, gates.Snapshot().FirstFailingGate);
    }

    [TestMethod]
    public void ExecutionContextLoadsInitializerFreePrivateDependencyAndRejectsInitializerBearingDependency()
    {
        using var temp = new TempTestDirectory("sts2-step34-execution-context");
        var primaryPath = Path.Combine(temp.Path, "sts2.dll");
        var dependencyPath = Path.Combine(temp.Path, "GameDependency.dll");
        var harmonyPath = Path.Combine(temp.Path, "0Harmony.dll");
        WriteAssembly(primaryPath, "sts2");
        WriteAssembly(dependencyPath, "GameDependency");
        WriteAssembly(harmonyPath, "0Harmony", new Version(2, 4, 2, 0));

        var primaryIdentity = AssemblyName.GetAssemblyName(primaryPath);
        var dependencyIdentity = AssemblyName.GetAssemblyName(dependencyPath);
        var harmonyIdentity = AssemblyName.GetAssemblyName(harmonyPath);
        var primaryBytes = File.ReadAllBytes(primaryPath);
        var dependencyBytes = File.ReadAllBytes(dependencyPath);
        var harmonyBytes = File.ReadAllBytes(harmonyPath);
        var primarySha256 = Convert.ToHexString(SHA256.HashData(primaryBytes)).ToLowerInvariant();

        var primaryPlan = CreatePlanEntry("sts2.dll", primaryIdentity, primaryBytes, isPrimary: true);
        var dependencyPlan = CreatePlanEntry("GameDependency.dll", dependencyIdentity, dependencyBytes, isPrimary: false);
        var harmonyPlan = CreatePlanEntry("0Harmony.dll", harmonyIdentity, harmonyBytes, isPrimary: false);
        var plan = new RuntimeFrameworkBindingPlanDocument(
            RuntimeFrameworkBindingPlanDocument.CurrentSchemaVersion,
            SteamOfflineInstallInspection.TargetAppId,
            2868842,
            1,
            "public",
            "Managed",
            primaryPlan.RelativePath,
            primaryPlan.AssemblyFullName,
            [primaryPlan, dependencyPlan, harmonyPlan],
            [],
            [],
            [],
            true);
        var entries = new[]
        {
            new TransformedRealStS2PrewarmJitExecution.PreparedExecutionEntry(primaryPlan, primaryPath, primaryIdentity, 0),
            new TransformedRealStS2PrewarmJitExecution.PreparedExecutionEntry(dependencyPlan, dependencyPath, dependencyIdentity, 0),
            new TransformedRealStS2PrewarmJitExecution.PreparedExecutionEntry(harmonyPlan, harmonyPath, harmonyIdentity, 1),
        };

        var context = new TransformedRealStS2PrewarmJitExecution.Step34ExecutionLoadContext(
            "Step34-Test",
            plan,
            entries,
            isCollectible: true);
        try
        {
            var loadedPrimary = context.LoadPrimary(primaryPath, primarySha256);
            Assert.AreSame(context, AssemblyLoadContext.GetLoadContext(loadedPrimary));
            Assert.AreEqual("sts2", loadedPrimary.GetName().Name);
            Assert.AreEqual(0, context.ManagedResolverRequests.Count);

            var loadedDependency = context.LoadFromAssemblyName(new AssemblyName(dependencyPlan.AssemblyFullName));
            Assert.AreSame(context, AssemblyLoadContext.GetLoadContext(loadedDependency));
            Assert.AreEqual("GameDependency", loadedDependency.GetName().Name);
            Assert.AreEqual(1, context.PrivateLoads.Count);
            Assert.AreEqual(0, context.RejectedManagedRequests.Count);

            Assert.ThrowsExactly<FileLoadException>(() =>
                context.LoadFromAssemblyName(new AssemblyName(harmonyPlan.AssemblyFullName)));
            Assert.AreEqual(1, context.InitializerBearingRequests.Count);
            Assert.AreEqual(0, context.NativeLoadAttempts.Count);
            Assert.AreEqual(2, context.Assemblies.Count());
        }
        finally
        {
            context.Unload();
        }
    }

    [TestMethod]
    public void Step34PinsThePhysicallyClosedTransformedTarget()
    {
        Assert.AreEqual("MegaCrit.Sts2.Core.Helpers.OneTimeInitialization", TransformedRealStS2PrewarmJitExecution.TargetTypeFullName);
        Assert.AreEqual("PrewarmJit", TransformedRealStS2PrewarmJitExecution.TargetMethodName);
        Assert.AreEqual(0x0600AFEAu, TransformedRealStS2PrewarmJitExecution.ClosedStep32TransformedPrewarmJitToken);
        Assert.AreEqual("39c0a89ad0d5c6eb1553e23dd8537a7b7ab8278fad4115d186db5751570211ef", TransformedRealStS2AssemblyAdmission.ClosedStep32TransformedSha256);
    }

    private static RuntimeBindingPreparedAssembly CreatePlanEntry(string relative, AssemblyName identity, byte[] bytes, bool isPrimary)
        => new(
            relative,
            identity.FullName!,
            Convert.ToHexString(SHA1.HashData(bytes)).ToLowerInvariant(),
            bytes.LongLength,
            isPrimary);

    private static void WriteAssembly(string path, string name, Version? version = null)
    {
        using var assembly = AssemblyDefinition.CreateAssembly(
            new AssemblyNameDefinition(name, version ?? new Version(1, 0, 0, 0)),
            name,
            ModuleKind.Dll);
        var module = assembly.MainModule;
        var type = new TypeDefinition("Fixture", "Marker", Mono.Cecil.TypeAttributes.Public | Mono.Cecil.TypeAttributes.Class, module.TypeSystem.Object);
        module.Types.Add(type);
        var method = new MethodDefinition("Ping", Mono.Cecil.MethodAttributes.Public | Mono.Cecil.MethodAttributes.Static, module.TypeSystem.Void);
        method.Body.GetILProcessor().Append(method.Body.GetILProcessor().Create(OpCodes.Ret));
        type.Methods.Add(method);
        assembly.Write(path);
    }
}
