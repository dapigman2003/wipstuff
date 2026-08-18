using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mono.Cecil;
using Mono.Cecil.Cil;
using StS2Launcher.Core;

namespace StS2Launcher.Core.Tests;

[TestClass]
public sealed class ExpressionInterpreterCompatibilityTests
{
    [TestMethod]
    public void OrderedExpressionInterpreterGatesReachFourOfFourPass()
    {
        var gates = new ExpressionInterpreterCompatibilityGateSequence();
        gates.Record(ExpressionInterpreterCompatibilityGate.InterpreterCapabilityAndWorkspaceClone, true, "probe/clone");
        gates.Record(ExpressionInterpreterCompatibilityGate.RealCompileTargetDiscovery, true, "targets");
        gates.Record(ExpressionInterpreterCompatibilityGate.PreferInterpretationRewrite, true, "rewrite");
        gates.Record(ExpressionInterpreterCompatibilityGate.IsolationAudit, true, "audit");

        var summary = gates.Snapshot();
        Assert.IsTrue(summary.Passed);
        Assert.AreEqual(4, summary.PassedGates);
        Assert.AreEqual("EXPRESSION INTERPRETER COMPATIBILITY PASS — 4/4", summary.Summary);
    }

    [TestMethod]
    public void ExpressionInterpreterGatesStopAfterFirstFailure()
    {
        var gates = new ExpressionInterpreterCompatibilityGateSequence();
        gates.Record(ExpressionInterpreterCompatibilityGate.InterpreterCapabilityAndWorkspaceClone, true, "probe/clone");
        gates.Record(ExpressionInterpreterCompatibilityGate.RealCompileTargetDiscovery, false, "no target");
        Assert.ThrowsExactly<InvalidOperationException>(() =>
            gates.Record(ExpressionInterpreterCompatibilityGate.PreferInterpretationRewrite, true, "must not advance"));
        Assert.AreEqual(ExpressionInterpreterCompatibilityGate.RealCompileTargetDiscovery, gates.Snapshot().FirstFailingGate);
    }

    [TestMethod]
    public async Task RealWorkspaceExpressionCompileCallsAreForcedToInterpretationAndInstallStaysUntouched()
    {
        using var temp = new TemporaryDirectory();
        var managedPath = Path.Combine(temp.Path, SteamOfflineInstallInspection.ManagedRootRelativePath, "Depot-2868842");
        var arm64Relative = "SlayTheSpire2.app/Contents/Resources/data_sts2_macos_arm64/sts2.dll";
        var x86Relative = "SlayTheSpire2.app/Contents/Resources/data_sts2_macos_x86_64/sts2.dll";
        var sharedRelative = "SlayTheSpire2.app/Contents/Resources/shared-helper.dll";

        var arm64Path = Path.Combine(managedPath, arm64Relative.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(arm64Path)!);
        WriteSyntheticExpressionAssembly(arm64Path, "sts2", includeTargets: true);

        var x86Path = Path.Combine(managedPath, x86Relative.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(x86Path)!);
        File.Copy(arm64Path, x86Path, overwrite: true);

        var sharedPath = Path.Combine(managedPath, sharedRelative.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(sharedPath)!);
        WriteSyntheticExpressionAssembly(sharedPath, "shared-helper", includeTargets: false);

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
            991UL,
            "public",
            DateTimeOffset.UtcNow,
            files);
        await using (var stream = File.Create(Path.Combine(managedPath, SteamManagedInstallReceipt.FileName)))
        {
            await JsonSerializer.SerializeAsync(stream, receipt, SteamManagedInstallJsonContext.Default.SteamManagedInstallReceipt);
        }

        var installBefore = SHA256.HashData(await File.ReadAllBytesAsync(arm64Path));
        var compatibility = new ExpressionInterpreterCompatibility(temp.Path);
        var gateA = await compatibility.RunInterpreterCapabilityAndWorkspaceCloneAsync();
        var gateB = compatibility.RunRealCompileTargetDiscovery();
        var gateC = compatibility.RunPreferInterpretationRewrite();
        var gateD = await compatibility.RunIsolationAuditAsync();
        var installAfter = SHA256.HashData(await File.ReadAllBytesAsync(arm64Path));

        Assert.IsTrue(gateA.Passed, gateA.Detail);
        Assert.IsTrue(gateB.Passed, gateB.Detail);
        Assert.IsTrue(gateC.Passed, gateC.Detail);
        Assert.IsTrue(gateD.Passed, gateD.Detail);
        CollectionAssert.AreEqual(installBefore, installAfter);

        StringAssert.Contains(gateA.Detail, "Compile(preferInterpretation: true) probe result: 42");
        StringAssert.Contains(gateA.Detail, "macOS x86_64 duplicates excluded: 1");
        StringAssert.Contains(gateB.Detail, "Structurally-safe parameterless Compile() sites: 2");
        StringAssert.Contains(gateB.Detail, "Literal Compile(false) sites: 3");
        StringAssert.Contains(gateB.Detail, "Already-interpreted Compile(true) sites: 1");
        StringAssert.Contains(gateB.Detail, "Dynamic/non-literal Compile(bool) sites left untouched: 1");
        StringAssert.Contains(gateB.Detail, "Parameterless sites skipped for branch/EH/prefix safety: 2");
        StringAssert.Contains(gateB.Detail, "Writable supported sites selected: 5");
        StringAssert.Contains(gateC.Detail, "Total real call sites rewritten: 5");
        StringAssert.Contains(gateD.Detail, "Total Compile sites forced to interpreter preference: 5");
        StringAssert.Contains(gateD.Detail, "Original Step 12 install unchanged: YES");

        var sourceCopy = Path.Combine(
            temp.Path,
            ExpressionInterpreterCompatibility.WorkRootName,
            ExpressionInterpreterCompatibility.SourceRootName,
            arm64Relative.Replace('/', Path.DirectorySeparatorChar));
        var preparedCopy = Path.Combine(
            temp.Path,
            ExpressionInterpreterCompatibility.WorkRootName,
            ExpressionInterpreterCompatibility.PreparedRootName,
            arm64Relative.Replace('/', Path.DirectorySeparatorChar));
        CollectionAssert.AreEqual(await File.ReadAllBytesAsync(arm64Path), await File.ReadAllBytesAsync(sourceCopy));
        Assert.IsFalse((await File.ReadAllBytesAsync(sourceCopy)).SequenceEqual(await File.ReadAllBytesAsync(preparedCopy)));

        using var prepared = ModuleDefinition.ReadModule(preparedCopy, new ReaderParameters
        {
            ReadingMode = ReadingMode.Deferred,
            AssemblyResolver = RejectingTestResolver.Instance,
            MetadataResolver = RejectingTestResolver.Instance,
        });
        var calls = prepared.Types
            .SelectMany(type => type.Methods)
            .Where(method => method.HasBody)
            .SelectMany(method => method.Body.Instructions)
            .Where(instruction => instruction.OpCode.Code is Code.Call or Code.Callvirt)
            .Where(instruction => instruction.Operand is MethodReference method && method.Name == "Compile")
            .ToArray();
        Assert.AreEqual(9, calls.Length);
        Assert.AreEqual(2, calls.Count(instruction => ((MethodReference)instruction.Operand).Parameters.Count == 0));
        Assert.AreEqual(6, calls.Count(IsImmediatelyPrecededByTrueLiteral));

        var expressionUser = prepared.GetType("Synthetic.ExpressionUser")!;
        AssertConstantEncoding(expressionUser, "LiteralFalse", Code.Ldc_I4_1, null);
        AssertConstantEncoding(expressionUser, "LiteralFalseShort", Code.Ldc_I4_S, (sbyte)1);
        AssertConstantEncoding(expressionUser, "LiteralFalseLong", Code.Ldc_I4, 1);
        var unsafeMethod = expressionUser.Methods.Single(method => method.Name == "UnsafeBranchTargetParameterless");
        Assert.AreEqual(0, ((MethodReference)unsafeMethod.Body.Instructions.Single(instruction => instruction.OpCode.Code is Code.Call or Code.Callvirt).Operand).Parameters.Count);
        var crossingShortBranchMethod = expressionUser.Methods.Single(method => method.Name == "CrossingShortBranchParameterless");
        Assert.AreEqual(0, ((MethodReference)crossingShortBranchMethod.Body.Instructions.Single(instruction => instruction.OpCode.Code is Code.Call or Code.Callvirt).Operand).Parameters.Count);
    }

    [TestMethod]
    public async Task NoSupportedExpressionCompileTargetFailsAtGateBWithoutPreparedOutput()
    {
        using var temp = new TemporaryDirectory();
        var managedPath = Path.Combine(temp.Path, SteamOfflineInstallInspection.ManagedRootRelativePath, "Depot-2868842");
        var arm64Relative = "SlayTheSpire2.app/Contents/Resources/data_sts2_macos_arm64/sts2.dll";
        var arm64Path = Path.Combine(managedPath, arm64Relative.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(arm64Path)!);
        WriteSyntheticExpressionAssembly(arm64Path, "sts2", includeTargets: false);

        var bytes = await File.ReadAllBytesAsync(arm64Path);
        var receipt = new SteamManagedInstallReceipt(
            SteamManagedInstallReceipt.CurrentSchemaVersion,
            2868840,
            2868842,
            992UL,
            "public",
            DateTimeOffset.UtcNow,
            [new SteamManagedInstallFile(arm64Relative, bytes.LongLength, Convert.ToHexString(SHA1.HashData(bytes)).ToLowerInvariant())]);
        await using (var stream = File.Create(Path.Combine(managedPath, SteamManagedInstallReceipt.FileName)))
        {
            await JsonSerializer.SerializeAsync(stream, receipt, SteamManagedInstallJsonContext.Default.SteamManagedInstallReceipt);
        }

        var compatibility = new ExpressionInterpreterCompatibility(temp.Path);
        var gateA = await compatibility.RunInterpreterCapabilityAndWorkspaceCloneAsync();
        var gateB = compatibility.RunRealCompileTargetDiscovery();

        Assert.IsTrue(gateA.Passed, gateA.Detail);
        Assert.IsFalse(gateB.Passed);
        StringAssert.Contains(gateB.Detail, "No structurally-safe unsigned direct System.Linq.Expressions Compile target");
        Assert.IsFalse(Directory.Exists(Path.Combine(temp.Path, ExpressionInterpreterCompatibility.WorkRootName, ExpressionInterpreterCompatibility.PreparedRootName)));
    }

    private static bool IsImmediatelyPrecededByTrueLiteral(Instruction instruction)
        => instruction.Previous is { } previous && TryGetConstant(previous, out var value) && value == 1;

    private static bool TryGetConstant(Instruction instruction, out int value)
    {
        switch (instruction.OpCode.Code)
        {
            case Code.Ldc_I4_0: value = 0; return true;
            case Code.Ldc_I4_1: value = 1; return true;
            case Code.Ldc_I4_S: value = Convert.ToInt32(instruction.Operand); return true;
            case Code.Ldc_I4: value = Convert.ToInt32(instruction.Operand); return true;
            default: value = 0; return false;
        }
    }

    private static void AssertConstantEncoding(TypeDefinition type, string methodName, Code expectedCode, object? expectedOperand)
    {
        var method = type.Methods.Single(value => value.Name == methodName);
        var call = method.Body.Instructions.Single(instruction => instruction.OpCode.Code is Code.Call or Code.Callvirt);
        var constant = call.Previous ?? throw new AssertFailedException($"{methodName} Compile call has no argument instruction.");
        Assert.AreEqual(expectedCode, constant.OpCode.Code, $"Unexpected constant opcode in {methodName}.");
        if (expectedOperand is null)
            Assert.IsNull(constant.Operand, $"Unexpected constant operand in {methodName}.");
        else
            Assert.AreEqual(expectedOperand, constant.Operand, $"Unexpected constant operand in {methodName}.");
    }

    private static void WriteSyntheticExpressionAssembly(string path, string assemblyName, bool includeTargets)
    {
        using var assembly = AssemblyDefinition.CreateAssembly(
            new AssemblyNameDefinition(assemblyName, new Version(1, 0, 0, 0)),
            assemblyName,
            ModuleKind.Dll);
        var module = assembly.MainModule;
        var type = new TypeDefinition("Synthetic", "ExpressionUser", TypeAttributes.Public | TypeAttributes.Class, module.TypeSystem.Object);
        module.Types.Add(type);

        if (!includeTargets)
        {
            var noop = new MethodDefinition("Noop", MethodAttributes.Public | MethodAttributes.Static, module.TypeSystem.Void);
            type.Methods.Add(noop);
            noop.Body.GetILProcessor().Append(Instruction.Create(OpCodes.Ret));
            assembly.Write(path);
            return;
        }

        var expressionsAssembly = new AssemblyNameReference("System.Linq.Expressions", new Version(9, 0, 0, 0));
        module.AssemblyReferences.Add(expressionsAssembly);
        var lambdaType = new TypeReference("System.Linq.Expressions", "LambdaExpression", module, expressionsAssembly);
        var compileNoArgs = new MethodReference("Compile", module.TypeSystem.Object, lambdaType) { HasThis = true };
        var compileBool = new MethodReference("Compile", module.TypeSystem.Object, lambdaType) { HasThis = true };
        compileBool.Parameters.Add(new ParameterDefinition(module.TypeSystem.Boolean));

        var genericExpressionType = new TypeReference("System.Linq.Expressions", "Expression`1", module, expressionsAssembly);
        genericExpressionType.GenericParameters.Add(new GenericParameter("TDelegate", genericExpressionType));
        var expressionOfObject = new GenericInstanceType(genericExpressionType);
        expressionOfObject.GenericArguments.Add(module.TypeSystem.Object);
        var genericCompileNoArgs = new MethodReference("Compile", module.TypeSystem.Object, expressionOfObject) { HasThis = true };

        AddCompileMethod(type, "Parameterless", lambdaType, compileNoArgs, argumentKind: 0);
        AddCompileMethod(type, "GenericParameterless", expressionOfObject, genericCompileNoArgs, argumentKind: 0);
        AddCompileMethod(type, "LiteralFalse", lambdaType, compileBool, argumentKind: 1);
        AddCompileMethod(type, "LiteralFalseShort", lambdaType, compileBool, argumentKind: 4);
        AddCompileMethod(type, "LiteralFalseLong", lambdaType, compileBool, argumentKind: 5);
        AddCompileMethod(type, "LiteralTrue", lambdaType, compileBool, argumentKind: 2);
        AddCompileMethod(type, "DynamicBool", lambdaType, compileBool, argumentKind: 3);
        AddUnsafeBranchTargetCompileMethod(type, lambdaType, compileNoArgs);
        AddCrossingShortBranchCompileMethod(type, lambdaType, compileNoArgs);
        assembly.Write(path);
    }

    private static void AddCompileMethod(
        TypeDefinition type,
        string name,
        TypeReference lambdaType,
        MethodReference compile,
        int argumentKind)
    {
        var module = type.Module;
        var method = new MethodDefinition(name, MethodAttributes.Public | MethodAttributes.Static, module.TypeSystem.Void);
        method.Parameters.Add(new ParameterDefinition("lambda", ParameterAttributes.None, lambdaType));
        if (argumentKind == 3)
            method.Parameters.Add(new ParameterDefinition("prefer", ParameterAttributes.None, module.TypeSystem.Boolean));
        type.Methods.Add(method);

        var il = method.Body.GetILProcessor();
        il.Append(il.Create(OpCodes.Ldarg_0));
        if (argumentKind == 1)
            il.Append(il.Create(OpCodes.Ldc_I4_0));
        else if (argumentKind == 2)
            il.Append(il.Create(OpCodes.Ldc_I4_1));
        else if (argumentKind == 3)
            il.Append(il.Create(OpCodes.Ldarg_1));
        else if (argumentKind == 4)
            il.Append(il.Create(OpCodes.Ldc_I4_S, (sbyte)0));
        else if (argumentKind == 5)
            il.Append(il.Create(OpCodes.Ldc_I4, 0));
        il.Append(il.Create(OpCodes.Callvirt, compile));
        il.Append(il.Create(OpCodes.Pop));
        il.Append(il.Create(OpCodes.Ret));
    }


    private static void AddUnsafeBranchTargetCompileMethod(
        TypeDefinition type,
        TypeReference lambdaType,
        MethodReference compile)
    {
        var module = type.Module;
        var method = new MethodDefinition("UnsafeBranchTargetParameterless", MethodAttributes.Public | MethodAttributes.Static, module.TypeSystem.Void);
        method.Parameters.Add(new ParameterDefinition("lambda", ParameterAttributes.None, lambdaType));
        type.Methods.Add(method);

        var il = method.Body.GetILProcessor();
        var call = il.Create(OpCodes.Callvirt, compile);
        il.Append(il.Create(OpCodes.Ldarg_0));
        il.Append(il.Create(OpCodes.Br_S, call));
        il.Append(il.Create(OpCodes.Nop));
        il.Append(call);
        il.Append(il.Create(OpCodes.Pop));
        il.Append(il.Create(OpCodes.Ret));
    }


    private static void AddCrossingShortBranchCompileMethod(
        TypeDefinition type,
        TypeReference lambdaType,
        MethodReference compile)
    {
        var module = type.Module;
        var method = new MethodDefinition("CrossingShortBranchParameterless", MethodAttributes.Public | MethodAttributes.Static, module.TypeSystem.Void);
        method.Parameters.Add(new ParameterDefinition("lambda", ParameterAttributes.None, lambdaType));
        type.Methods.Add(method);

        var il = method.Body.GetILProcessor();
        var done = il.Create(OpCodes.Ret);
        il.Append(il.Create(OpCodes.Ldc_I4_0));
        il.Append(il.Create(OpCodes.Brtrue_S, done));
        il.Append(il.Create(OpCodes.Ldarg_0));
        il.Append(il.Create(OpCodes.Callvirt, compile));
        il.Append(il.Create(OpCodes.Pop));
        il.Append(done);
    }

    private sealed class RejectingTestResolver : IAssemblyResolver, IMetadataResolver
    {
        public static RejectingTestResolver Instance { get; } = new();
        private RejectingTestResolver() { }
        public AssemblyDefinition Resolve(AssemblyNameReference name)
            => throw new InvalidOperationException($"Unexpected test dependency resolution: {name.FullName}");
        public AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters) => Resolve(name);
        TypeDefinition IMetadataResolver.Resolve(TypeReference type)
            => throw new InvalidOperationException($"Unexpected test type resolution: {type.FullName}");
        FieldDefinition IMetadataResolver.Resolve(FieldReference field)
            => throw new InvalidOperationException($"Unexpected test field resolution: {field.FullName}");
        MethodDefinition IMetadataResolver.Resolve(MethodReference method)
            => throw new InvalidOperationException($"Unexpected test method resolution: {method.FullName}");
        public void Dispose() { }
    }

    private sealed class TemporaryDirectory : IDisposable
    {
        public TemporaryDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "sts2-step19-" + Guid.NewGuid().ToString("N"));
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
