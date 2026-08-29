using System.Reflection;
using System.Runtime.Loader;
using System.Security.Cryptography;
using Mono.Cecil;
using Mono.Cecil.Cil;
using StS2Launcher.Core;

namespace StS2Launcher.Core.Tests;

[TestClass]
public sealed class TransformedRealStS2VeryEarlyInitializationTests
{
    [TestMethod]
    public void OrderedDiagnosticLocalizationGatesReachFourOfFourWithoutClaimingClosure()
    {
        var gates = new TransformedRealStS2VeryEarlyInitializationGateSequence();
        gates.Record(new(TransformedRealStS2VeryEarlyInitializationGate.VerifiedExecutionPreflight, true, "preflight"));
        gates.Record(new(TransformedRealStS2VeryEarlyInitializationGate.ExecutionCapableClrAdmission, true, "admission"));
        gates.Record(new(TransformedRealStS2VeryEarlyInitializationGate.DiagnosticExecuteVeryEarlyInvocation, true, "invoke"));
        gates.Record(new(TransformedRealStS2VeryEarlyInitializationGate.FinalIsolationAudit, true, "isolation"));

        var summary = gates.Snapshot();
        Assert.IsTrue(summary.Passed);
        Assert.AreEqual(4, summary.Gates.Count);
        Assert.AreEqual("STEP 35.0.4 DIAGNOSTIC LOCALIZATION COMPLETE — 4/4 — NOT STEP 35 CLOSURE", summary.Summary);
    }

    [TestMethod]
    public void VeryEarlyInitializationStopsAfterFirstFailure()
    {
        var gates = new TransformedRealStS2VeryEarlyInitializationGateSequence();
        gates.Record(new(TransformedRealStS2VeryEarlyInitializationGate.VerifiedExecutionPreflight, true, "preflight"));
        gates.Record(new(TransformedRealStS2VeryEarlyInitializationGate.ExecutionCapableClrAdmission, true, "admission"));
        gates.Record(new(TransformedRealStS2VeryEarlyInitializationGate.DiagnosticExecuteVeryEarlyInvocation, false, "failed"));

        Assert.ThrowsExactly<InvalidOperationException>(() =>
            gates.Record(new(TransformedRealStS2VeryEarlyInitializationGate.FinalIsolationAudit, true, "must not advance")));
        Assert.AreEqual(TransformedRealStS2VeryEarlyInitializationGate.DiagnosticExecuteVeryEarlyInvocation, gates.Snapshot().FirstFailingGate);
    }

    [TestMethod]
    public void VeryEarlyContextLoadsInitializerFreePrivateDependencyAndRejectsInitializerBearingDependency()
    {
        using var temp = new TempTestDirectory("sts2-step35-execution-context");
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
            new TransformedRealStS2VeryEarlyInitialization.PreparedExecutionEntry(primaryPlan, primaryPath, primaryIdentity, 0),
            new TransformedRealStS2VeryEarlyInitialization.PreparedExecutionEntry(dependencyPlan, dependencyPath, dependencyIdentity, 0),
            new TransformedRealStS2VeryEarlyInitialization.PreparedExecutionEntry(harmonyPlan, harmonyPath, harmonyIdentity, 1),
        };

        var crashCheckpoints = new List<string>();
        var context = new TransformedRealStS2VeryEarlyInitialization.Step35ExecutionLoadContext(
            "Step35-Test",
            plan,
            entries,
            isCollectible: true,
            crashCheckpoint: crashCheckpoints.Add);
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
            Assert.IsTrue(crashCheckpoints.Any(item => item.StartsWith("B_LOADFROMSTREAM_START", StringComparison.Ordinal)));
            Assert.IsTrue(crashCheckpoints.Any(item => item.StartsWith("B_LOADFROMSTREAM_PASS", StringComparison.Ordinal)));
            Assert.IsTrue(crashCheckpoints.Any(item => item.StartsWith("RESOLVE_PRIVATE_PASS", StringComparison.Ordinal)));
            Assert.IsTrue(crashCheckpoints.Any(item => item.StartsWith("RESOLVE_INITIALIZER_BEARING_REJECT", StringComparison.Ordinal)));
        }
        finally
        {
            context.Unload();
        }
    }

    [TestMethod]
    public void StaticInstructionMapCapturesCallsitesAndAwaitCandidatesWithoutResolution()
    {
        using var assembly = AssemblyDefinition.CreateAssembly(
            new AssemblyNameDefinition("StaticMapFixture", new Version(1, 0, 0, 0)),
            "StaticMapFixture",
            ModuleKind.Dll);
        var module = assembly.MainModule;
        var type = new TypeDefinition("Fixture", "StateMachine", Mono.Cecil.TypeAttributes.Public | Mono.Cecil.TypeAttributes.Class, module.TypeSystem.Object);
        module.Types.Add(type);

        var wrapper = new MethodDefinition("ExecuteVeryEarly", Mono.Cecil.MethodAttributes.Public | Mono.Cecil.MethodAttributes.Static, module.TypeSystem.Void);
        wrapper.Body.GetILProcessor().Append(wrapper.Body.GetILProcessor().Create(OpCodes.Ret));
        type.Methods.Add(wrapper);

        var moveNext = new MethodDefinition("MoveNext", Mono.Cecil.MethodAttributes.Public, module.TypeSystem.Void);
        type.Methods.Add(moveNext);
        var systemRuntime = new AssemblyNameReference("System.Runtime", new Version(9, 0, 0, 0));
        module.AssemblyReferences.Add(systemRuntime);
        var builderType = new TypeReference("System.Runtime.CompilerServices", "AsyncTaskMethodBuilder", module, systemRuntime);
        var awaitCall = new MethodReference("AwaitUnsafeOnCompleted", module.TypeSystem.Void, builderType);
        var il = moveNext.Body.GetILProcessor();
        il.Append(il.Create(OpCodes.Call, awaitCall));
        il.Append(il.Create(OpCodes.Ret));

        var map = TransformedRealStS2VeryEarlyInitialization.BuildStaticInstructionMap(wrapper, moveNext);

        StringAssert.Contains(map, "[MOVENEXT IL]");
        StringAssert.Contains(map, "CALLSITE#001");
        StringAssert.Contains(map, "AWAIT-CANDIDATE");
        StringAssert.Contains(map, "System.Runtime, Version=9.0.0.0");
        StringAssert.Contains(map, "AwaitUnsafeOnCompleted");
    }

    [TestMethod]
    public void Step35PinsTheExactVeryEarlyManagedInitializationTarget()
    {
        Assert.AreEqual("MegaCrit.Sts2.Core.Helpers.OneTimeInitialization", TransformedRealStS2VeryEarlyInitialization.TargetTypeFullName);
        Assert.AreEqual("ExecuteVeryEarly", TransformedRealStS2VeryEarlyInitialization.TargetMethodName);
        Assert.AreEqual("System.Threading.Tasks.Task MegaCrit.Sts2.Core.Helpers.OneTimeInitialization::ExecuteVeryEarly()", TransformedRealStS2VeryEarlyInitialization.TargetMethodFullName);
        Assert.AreEqual(0x06007D02u, TransformedRealStS2VeryEarlyInitialization.SourceTargetMethodToken);
        Assert.AreEqual("<ExecuteVeryEarly>d__7", TransformedRealStS2VeryEarlyInitialization.TargetStateMachineTypeName);
        Assert.AreEqual(0x0600BC71u, TransformedRealStS2VeryEarlyInitialization.SourceStateMachineMoveNextToken);
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
