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
        Assert.AreEqual("STEP 35.0.9 DIAGNOSTIC LOCALIZATION COMPLETE — 4/4 — NOT STEP 35 CLOSURE", summary.Summary);
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

        var nullType = new TypeDefinition("MegaCrit.Sts2.Core.Platform.Null", "NullPlatformUtilStrategy", Mono.Cecil.TypeAttributes.Public | Mono.Cecil.TypeAttributes.Class, module.TypeSystem.Object);
        module.Types.Add(nullType);
        var nullCtor = new MethodDefinition(".ctor", Mono.Cecil.MethodAttributes.Public | Mono.Cecil.MethodAttributes.HideBySig | Mono.Cecil.MethodAttributes.SpecialName | Mono.Cecil.MethodAttributes.RTSpecialName, module.TypeSystem.Void);
        nullType.Methods.Add(nullCtor);
        var objectCtor = new MethodReference(".ctor", module.TypeSystem.Void, module.TypeSystem.Object) { HasThis = true };
        var ctorIl = nullCtor.Body.GetILProcessor();
        ctorIl.Append(ctorIl.Create(OpCodes.Ldarg_0));
        ctorIl.Append(ctorIl.Create(OpCodes.Call, objectCtor));
        ctorIl.Append(ctorIl.Create(OpCodes.Ret));

        var map = TransformedRealStS2VeryEarlyInitialization.BuildStaticInstructionMap(wrapper, moveNext, nullCtor);

        StringAssert.Contains(map, "[MOVENEXT IL]");
        StringAssert.Contains(map, "[NULL PLATFORM CTOR IL]");
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

    [TestMethod]
    public void DiagnosticActionStringInvokeMemberRefRoundTripsAsDeclaringTypeVarZero()
    {
        using var temp = new TempTestDirectory("sts2-step35-action-memberref");
        var assemblyPath = Path.Combine(temp.Path, "ActionMemberRefFixture.dll");
        using (var assembly = AssemblyDefinition.CreateAssembly(
                   new AssemblyNameDefinition("ActionMemberRefFixture", new Version(1, 0, 0, 0)),
                   "ActionMemberRefFixture",
                   ModuleKind.Dll))
        {
            var module = assembly.MainModule;
            var systemRuntime = new AssemblyNameReference("System.Runtime", new Version(9, 0, 0, 0));
            module.AssemblyReferences.Add(systemRuntime);
            var (_, invoke) = TransformedRealStS2VeryEarlyInitialization.CreateDiagnosticActionStringInvokeReference(module, systemRuntime);

            var type = new TypeDefinition("Fixture", "Probe", Mono.Cecil.TypeAttributes.Public | Mono.Cecil.TypeAttributes.Class, module.TypeSystem.Object);
            module.Types.Add(type);
            var method = new MethodDefinition("Run", Mono.Cecil.MethodAttributes.Public | Mono.Cecil.MethodAttributes.Static, module.TypeSystem.Void);
            type.Methods.Add(method);
            var il = method.Body.GetILProcessor();
            il.Append(il.Create(OpCodes.Ldnull));
            il.Append(il.Create(OpCodes.Ldstr, "marker"));
            il.Append(il.Create(OpCodes.Callvirt, invoke));
            il.Append(il.Create(OpCodes.Ret));
            assembly.Write(assemblyPath);
        }

        using var reopened = AssemblyDefinition.ReadAssembly(assemblyPath, new ReaderParameters { ReadSymbols = false, ReadingMode = ReadingMode.Deferred });
        var serializedMethod = reopened.MainModule.Types.Single(type => type.FullName == "Fixture.Probe").Methods.Single(method => method.Name == "Run");
        var call = serializedMethod.Body.Instructions.Single(instruction => instruction.OpCode == OpCodes.Callvirt);
        Assert.IsInstanceOfType(call.Operand, typeof(MethodReference));
        var serializedInvoke = (MethodReference)call.Operand;
        Assert.AreEqual("Invoke", serializedInvoke.Name);
        Assert.IsInstanceOfType(serializedInvoke.DeclaringType, typeof(GenericInstanceType));
        var declaringType = (GenericInstanceType)serializedInvoke.DeclaringType;
        Assert.AreEqual("System.Action`1", declaringType.ElementType.FullName);
        Assert.AreEqual(1, declaringType.GenericArguments.Count);
        Assert.AreEqual("System.String", declaringType.GenericArguments[0].FullName);
        Assert.AreEqual(1, serializedInvoke.Parameters.Count);
        Assert.IsInstanceOfType(serializedInvoke.Parameters[0].ParameterType, typeof(GenericParameter));
        var parameter = (GenericParameter)serializedInvoke.Parameters[0].ParameterType;
        Assert.AreEqual(GenericParameterType.Type, parameter.Type);
        Assert.AreEqual(0, parameter.Position);
    }

    [TestMethod]
    public void DiagnosticGodotCallsiteMarkersRoundTripImmediatelyBeforeAndAfterTargetCall()
    {
        using var temp = new TempTestDirectory("sts2-step35-godot-callsite");
        var assemblyPath = Path.Combine(temp.Path, "GodotCallsiteFixture.dll");
        const string beforeMarker = "INMETHOD_180 — before Godot.DirAccess.DirExistsAbsolute";
        const string afterMarker = "INMETHOD_181 — after Godot.DirAccess.DirExistsAbsolute";

        using (var assembly = AssemblyDefinition.CreateAssembly(
                   new AssemblyNameDefinition("GodotCallsiteFixture", new Version(1, 0, 0, 0)),
                   "GodotCallsiteFixture",
                   ModuleKind.Dll))
        {
            var module = assembly.MainModule;
            var godotSharp = new AssemblyNameReference("GodotSharp", new Version(4, 5, 1, 0));
            module.AssemblyReferences.Add(godotSharp);
            var dirAccess = new TypeReference("Godot", "DirAccess", module, godotSharp, false);
            var dirExists = new MethodReference("DirExistsAbsolute", module.TypeSystem.Boolean, dirAccess)
            {
                HasThis = false,
                CallingConvention = MethodCallingConvention.Default,
            };
            dirExists.Parameters.Add(new ParameterDefinition(module.TypeSystem.String));

            var bridge = new TypeDefinition("StS2Launcher.Step35Diagnostics", "ExecuteVeryEarlyCheckpointBridge", Mono.Cecil.TypeAttributes.Public | Mono.Cecil.TypeAttributes.Class, module.TypeSystem.Object);
            module.Types.Add(bridge);
            var emit = new MethodDefinition("Emit", Mono.Cecil.MethodAttributes.Public | Mono.Cecil.MethodAttributes.Static, module.TypeSystem.Void);
            emit.Parameters.Add(new ParameterDefinition(module.TypeSystem.String));
            emit.Body.GetILProcessor().Append(emit.Body.GetILProcessor().Create(OpCodes.Ret));
            bridge.Methods.Add(emit);

            var fixture = new TypeDefinition("Fixture", "GodotFileIo", Mono.Cecil.TypeAttributes.Public | Mono.Cecil.TypeAttributes.Class, module.TypeSystem.Object);
            module.Types.Add(fixture);
            var method = new MethodDefinition("CreateDirectory", Mono.Cecil.MethodAttributes.Public | Mono.Cecil.MethodAttributes.Static, module.TypeSystem.Void);
            method.Parameters.Add(new ParameterDefinition("path", Mono.Cecil.ParameterAttributes.None, module.TypeSystem.String));
            fixture.Methods.Add(method);
            var il = method.Body.GetILProcessor();
            il.Append(il.Create(OpCodes.Ldarg_0));
            il.Append(il.Create(OpCodes.Call, dirExists));
            il.Append(il.Create(OpCodes.Pop));
            il.Append(il.Create(OpCodes.Ret));

            TransformedRealStS2VeryEarlyInitialization.InsertCallsiteMarkers(method, emit, dirExists.FullName, beforeMarker, afterMarker);
            assembly.Write(assemblyPath);
        }

        using var reopened = AssemblyDefinition.ReadAssembly(assemblyPath, new ReaderParameters { ReadSymbols = false, ReadingMode = ReadingMode.Deferred });
        var serializedMethod = reopened.MainModule.Types.Single(type => type.FullName == "Fixture.GodotFileIo").Methods.Single(method => method.Name == "CreateDirectory");
        var targetCall = serializedMethod.Body.Instructions.Single(instruction =>
            instruction.OpCode == OpCodes.Call && instruction.Operand is MethodReference method && method.Name == "DirExistsAbsolute");
        Assert.AreEqual(OpCodes.Call, targetCall.Previous!.OpCode);
        Assert.AreEqual("Emit", ((MethodReference)targetCall.Previous.Operand).Name);
        Assert.AreEqual(OpCodes.Ldstr, targetCall.Previous.Previous!.OpCode);
        Assert.AreEqual(beforeMarker, targetCall.Previous.Previous.Operand);
        Assert.AreEqual(OpCodes.Ldstr, targetCall.Next!.OpCode);
        Assert.AreEqual(afterMarker, targetCall.Next.Operand);
        Assert.AreEqual(OpCodes.Call, targetCall.Next.Next!.OpCode);
        Assert.AreEqual("Emit", ((MethodReference)targetCall.Next.Next.Operand).Name);
    }

    [TestMethod]
    public void DiagnosticNullPlatformConstructorCallsiteSweepRoundTripsEveryNonBaseCallLikeInstruction()
    {
        using var temp = new TempTestDirectory("sts2-step35-nullplatform-sweep");
        var assemblyPath = Path.Combine(temp.Path, "NullPlatformSweepFixture.dll");
        IReadOnlyList<TransformedRealStS2VeryEarlyInitialization.DiagnosticCallsiteSweepEntry> plan;

        using (var assembly = AssemblyDefinition.CreateAssembly(
                   new AssemblyNameDefinition("NullPlatformSweepFixture", new Version(1, 0, 0, 0)),
                   "NullPlatformSweepFixture",
                   ModuleKind.Dll))
        {
            var module = assembly.MainModule;
            var bridge = new TypeDefinition("StS2Launcher.Step35Diagnostics", "ExecuteVeryEarlyCheckpointBridge", Mono.Cecil.TypeAttributes.Public | Mono.Cecil.TypeAttributes.Class, module.TypeSystem.Object);
            module.Types.Add(bridge);
            var emit = new MethodDefinition("Emit", Mono.Cecil.MethodAttributes.Public | Mono.Cecil.MethodAttributes.Static, module.TypeSystem.Void);
            emit.Parameters.Add(new ParameterDefinition(module.TypeSystem.String));
            emit.Body.GetILProcessor().Append(emit.Body.GetILProcessor().Create(OpCodes.Ret));
            bridge.Methods.Add(emit);

            var helper = new TypeDefinition("Fixture", "Helper", Mono.Cecil.TypeAttributes.Public | Mono.Cecil.TypeAttributes.Class, module.TypeSystem.Object);
            module.Types.Add(helper);
            var helperCtor = new MethodDefinition(".ctor", Mono.Cecil.MethodAttributes.Public | Mono.Cecil.MethodAttributes.HideBySig | Mono.Cecil.MethodAttributes.SpecialName | Mono.Cecil.MethodAttributes.RTSpecialName, module.TypeSystem.Void);
            helper.Methods.Add(helperCtor);
            var objectCtor = new MethodReference(".ctor", module.TypeSystem.Void, module.TypeSystem.Object) { HasThis = true };
            var helperCtorIl = helperCtor.Body.GetILProcessor();
            helperCtorIl.Append(helperCtorIl.Create(OpCodes.Ldarg_0));
            helperCtorIl.Append(helperCtorIl.Create(OpCodes.Call, objectCtor));
            helperCtorIl.Append(helperCtorIl.Create(OpCodes.Ret));
            var ping = new MethodDefinition("Ping", Mono.Cecil.MethodAttributes.Public | Mono.Cecil.MethodAttributes.Static, module.TypeSystem.Void);
            ping.Body.GetILProcessor().Append(ping.Body.GetILProcessor().Create(OpCodes.Ret));
            helper.Methods.Add(ping);

            var nullType = new TypeDefinition("MegaCrit.Sts2.Core.Platform.Null", "NullPlatformUtilStrategy", Mono.Cecil.TypeAttributes.Public | Mono.Cecil.TypeAttributes.Class, module.TypeSystem.Object);
            module.Types.Add(nullType);
            var ctor = new MethodDefinition(".ctor", Mono.Cecil.MethodAttributes.Public | Mono.Cecil.MethodAttributes.HideBySig | Mono.Cecil.MethodAttributes.SpecialName | Mono.Cecil.MethodAttributes.RTSpecialName, module.TypeSystem.Void);
            nullType.Methods.Add(ctor);
            var il = ctor.Body.GetILProcessor();
            il.Append(il.Create(OpCodes.Ldarg_0));
            il.Append(il.Create(OpCodes.Call, objectCtor));
            il.Append(il.Create(OpCodes.Newobj, helperCtor));
            il.Append(il.Create(OpCodes.Pop));
            il.Append(il.Create(OpCodes.Call, ping));
            il.Append(il.Create(OpCodes.Ret));

            plan = TransformedRealStS2VeryEarlyInitialization.InsertNullPlatformConstructorCallsiteMarkers(ctor, emit);
            Assert.AreEqual(2, plan.Count);
            Assert.AreEqual(2, plan[0].CallsiteOrdinal);
            Assert.AreEqual(Code.Newobj, plan[0].OpCodeCode);
            Assert.AreEqual(3, plan[1].CallsiteOrdinal);
            Assert.AreEqual(Code.Call, plan[1].OpCodeCode);
            assembly.Write(assemblyPath);
        }

        using var reopened = AssemblyDefinition.ReadAssembly(assemblyPath, new ReaderParameters { ReadSymbols = false, ReadingMode = ReadingMode.Deferred });
        var ctorAfter = reopened.MainModule.Types.Single(type => type.FullName == TransformedRealStS2VeryEarlyInitialization.NullPlatformTypeFullName)
            .Methods.Single(method => method.FullName == TransformedRealStS2VeryEarlyInitialization.NullPlatformConstructorFullName);
        foreach (var entry in plan)
        {
            Assert.IsTrue(ctorAfter.Body.Instructions.Any(instruction => instruction.OpCode.Code == Code.Ldstr && Equals(instruction.Operand, entry.BeforeMarker)));
            Assert.IsTrue(ctorAfter.Body.Instructions.Any(instruction => instruction.OpCode.Code == Code.Ldstr && Equals(instruction.Operand, entry.AfterMarker)));
        }
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
