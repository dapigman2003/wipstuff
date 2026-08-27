using System.Security.Cryptography;
using System.Text.Json;
using Mono.Cecil;
using Mono.Cecil.Cil;
using StS2Launcher.Core;

namespace StS2Launcher.Core.Tests;

[TestClass]
public sealed class RealStS2PrepareMethodRewriteTests
{
    [TestMethod]
    public void OrderedRealRewriteGatesReachFourOfFourPass()
    {
        var gates = new RealStS2PrepareMethodRewriteGateSequence();
        gates.Record(RealStS2PrepareMethodRewriteGate.SourceAdmissionAndPrivateClone, true, "source");
        gates.Record(RealStS2PrepareMethodRewriteGate.DeterministicStackNeutralRewrite, true, "rewrite");
        gates.Record(RealStS2PrepareMethodRewriteGate.TransformedImageVerification, true, "verify");
        gates.Record(RealStS2PrepareMethodRewriteGate.FinalIsolationAudit, true, "isolation");
        var summary = gates.Snapshot();
        Assert.IsTrue(summary.Passed);
        Assert.AreEqual(4, summary.Gates.Count);
        Assert.AreEqual("REAL STS2 PREPAREMETHOD REWRITE PASS — 4/4", summary.Summary);
    }

    [TestMethod]
    public void RealRewriteStopsAfterFirstFailure()
    {
        var gates = new RealStS2PrepareMethodRewriteGateSequence();
        gates.Record(RealStS2PrepareMethodRewriteGate.SourceAdmissionAndPrivateClone, true, "source");
        gates.Record(RealStS2PrepareMethodRewriteGate.DeterministicStackNeutralRewrite, false, "failed");
        Assert.ThrowsExactly<InvalidOperationException>(() =>
            gates.Record(RealStS2PrepareMethodRewriteGate.TransformedImageVerification, true, "must not advance"));
        Assert.AreEqual(RealStS2PrepareMethodRewriteGate.DeterministicStackNeutralRewrite, gates.Snapshot().FirstFailingGate);
    }

    [TestMethod]
    public async Task ExactPrewarmJitPrepareMethodFamilyIsRewrittenOnPrivateCopyOnly()
    {
        using var temp = new TempTestDirectory("sts2-step32");
        var managedPath = Path.Combine(temp.Path, SteamOfflineInstallInspection.ManagedRootRelativePath, "Depot-2868842");
        const string primaryRelative = "SlayTheSpire2.app/Contents/Resources/data_sts2_macos_arm64/sts2.dll";
        var primaryPath = Path.Combine(managedPath, primaryRelative.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(primaryPath)!);
        WriteSyntheticPrimaryAssembly(primaryPath, branchToFirstPrepareMethod: false);
        var evidence = BuildEvidence(primaryPath);
        await WriteReceiptAsync(managedPath, primaryRelative, primaryPath);

        var before = await File.ReadAllBytesAsync(primaryPath);
        var rewrite = new RealStS2PrepareMethodRewrite(temp.Path, evidence);
        var gateA = await rewrite.RunSourceAdmissionAndPrivateCloneAsync();
        var gateB = rewrite.RunDeterministicStackNeutralRewrite();
        var gateC = rewrite.RunTransformedImageVerification();
        var gateD = await rewrite.RunFinalIsolationAuditAsync();
        var after = await File.ReadAllBytesAsync(primaryPath);

        Assert.IsTrue(gateA.Passed, gateA.Detail);
        Assert.IsTrue(gateB.Passed, gateB.Detail);
        Assert.IsTrue(gateC.Passed, gateC.Detail);
        Assert.IsTrue(gateD.Passed, gateD.Detail);
        CollectionAssert.AreEqual(before, after);
        StringAssert.Contains(gateA.Detail, "PrepareMethod sites rebound: 10/10");
        StringAssert.Contains(gateB.Detail, "One-argument sites rewritten: 6/6");
        StringAssert.Contains(gateB.Detail, "Two-argument sites rewritten: 4/4");
        StringAssert.Contains(gateB.Detail, "PrepareMethod(handle, instantiation[]) -> Pop + Pop");
        StringAssert.Contains(gateB.Detail, "Synthetic constant-metadata resolver types: 3");
        StringAssert.Contains(gateB.Detail, "Audited external constant type/storage requirements approved: 3/3 across 2/2 exact assembly scopes");
        StringAssert.Contains(gateB.Detail, "System.Runtime, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
        StringAssert.Contains(gateB.Detail, "Sentry, Version=5.0.0.0, Culture=neutral, PublicKeyToken=fba2ec45388e2af0");
        StringAssert.Contains(gateC.Detail, "PrepareMethod references source/transformed: 10 / 0");
        StringAssert.Contains(gateD.Detail, "Trusted Step 12 managed install unchanged: YES");
        StringAssert.Contains(gateD.Detail, "Real StS2 assembly/type/member CLR load or invocation by Step 32: NO");

        var transformedPath = rewrite.TransformedPathForTests;
        Assert.IsNotNull(transformedPath);
        Assert.IsTrue(File.Exists(transformedPath));
        using var sourceModule = ModuleDefinition.ReadModule(primaryPath, new ReaderParameters { ReadSymbols = false, ReadingMode = ReadingMode.Deferred });
        using var transformedModule = ModuleDefinition.ReadModule(transformedPath!, new ReaderParameters { ReadSymbols = false, ReadingMode = ReadingMode.Deferred });
        var sourceMethod = FindPrewarmJit(sourceModule);
        var transformedMethod = FindPrewarmJit(transformedModule);
        Assert.AreEqual(10, CountPrepareMethod(sourceMethod));
        Assert.AreEqual(0, CountPrepareMethod(transformedMethod));
        Assert.AreEqual(sourceMethod.Body.Instructions.Count + 4, transformedMethod.Body.Instructions.Count);
        Assert.AreEqual(
            sourceMethod.Body.Instructions.Count(value => value.OpCode.Code == Code.Pop) + 14,
            transformedMethod.Body.Instructions.Count(value => value.OpCode.Code == Code.Pop));
        Assert.AreNotEqual(
            RealStS2PrepareMethodRewrite.ComputeMethodSemanticFingerprint(sourceMethod),
            RealStS2PrepareMethodRewrite.ComputeMethodSemanticFingerprint(transformedMethod));
    }

    [TestMethod]
    public async Task BranchTargetedPrepareMethodSiteIsRejectedBeforeAnyRewrite()
    {
        using var temp = new TempTestDirectory("sts2-step32-branch");
        var managedPath = Path.Combine(temp.Path, SteamOfflineInstallInspection.ManagedRootRelativePath, "Depot-2868842");
        const string primaryRelative = "SlayTheSpire2.app/Contents/Resources/data_sts2_macos_arm64/sts2.dll";
        var primaryPath = Path.Combine(managedPath, primaryRelative.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(primaryPath)!);
        WriteSyntheticPrimaryAssembly(primaryPath, branchToFirstPrepareMethod: true);
        var evidence = BuildEvidence(primaryPath);
        await WriteReceiptAsync(managedPath, primaryRelative, primaryPath);

        var rewrite = new RealStS2PrepareMethodRewrite(temp.Path, evidence);
        var gateA = await rewrite.RunSourceAdmissionAndPrivateCloneAsync();
        Assert.IsFalse(gateA.Passed);
        StringAssert.Contains(gateA.Detail, "became a branch target");
        Assert.IsFalse(Directory.Exists(Path.Combine(temp.Path, RealStS2PrepareMethodRewrite.WorkRootName)));
    }

    [TestMethod]
    public async Task UnauditedExternalConstantRequirementFailsClosedBeforeRewrite()
    {
        using var temp = new TempTestDirectory("sts2-step32-unaudited-constant");
        var managedPath = Path.Combine(temp.Path, SteamOfflineInstallInspection.ManagedRootRelativePath, "Depot-2868842");
        const string primaryRelative = "SlayTheSpire2.app/Contents/Resources/data_sts2_macos_arm64/sts2.dll";
        var primaryPath = Path.Combine(managedPath, primaryRelative.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(primaryPath)!);
        WriteSyntheticPrimaryAssembly(primaryPath, branchToFirstPrepareMethod: false, includeUnauditedExternalConstant: true);
        var evidence = BuildEvidence(primaryPath);
        await WriteReceiptAsync(managedPath, primaryRelative, primaryPath);

        var before = await File.ReadAllBytesAsync(primaryPath);
        var rewrite = new RealStS2PrepareMethodRewrite(temp.Path, evidence);
        var gateA = await rewrite.RunSourceAdmissionAndPrivateCloneAsync();
        var gateB = rewrite.RunDeterministicStackNeutralRewrite();
        var after = await File.ReadAllBytesAsync(primaryPath);

        Assert.IsTrue(gateA.Passed, gateA.Detail);
        Assert.IsFalse(gateB.Passed);
        StringAssert.Contains(gateB.Detail, "external constant-metadata requirement set drifted from the static audit");
        StringAssert.Contains(gateB.Detail, "Unexpected.Dependency, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
        CollectionAssert.AreEqual(before, after);
        Assert.IsFalse(File.Exists(Path.Combine(
            temp.Path,
            RealStS2PrepareMethodRewrite.WorkRootName,
            RealStS2PrepareMethodRewrite.TransformedRootName,
            RealStS2PrepareMethodRewrite.PrimaryFileName)));
    }

    private static async Task WriteReceiptAsync(string managedPath, string primaryRelative, string primaryPath)
    {
        var bytes = await File.ReadAllBytesAsync(primaryPath);
        var receipt = new SteamManagedInstallReceipt(
            SteamManagedInstallReceipt.CurrentSchemaVersion,
            2868840,
            2868842,
            123456UL,
            "public",
            DateTimeOffset.UtcNow,
            [new SteamManagedInstallFile(primaryRelative, bytes.LongLength, Convert.ToHexString(SHA1.HashData(bytes)).ToLowerInvariant())]);
        await using var stream = File.Create(Path.Combine(managedPath, SteamManagedInstallReceipt.FileName));
        await JsonSerializer.SerializeAsync(stream, receipt, SteamManagedInstallJsonContext.Default.SteamManagedInstallReceipt);
    }

    private static RealStS2PrepareMethodRewrite.RewriteEvidence BuildEvidence(string path)
    {
        var raw = File.ReadAllBytes(path);
        using var module = ModuleDefinition.ReadModule(path, new ReaderParameters { ReadSymbols = false, ReadingMode = ReadingMode.Deferred });
        var method = FindPrewarmJit(module);
        var sites = method.Body.Instructions
            .Where(instruction => instruction.Operand is MethodReference reference &&
                                  reference.DeclaringType.FullName == "System.Runtime.CompilerServices.RuntimeHelpers" &&
                                  reference.Name == "PrepareMethod")
            .Select(instruction =>
            {
                var target = (MethodReference)instruction.Operand;
                return new RealStS2PrepareMethodRewrite.RewriteCallSiteEvidence(instruction.Offset, target.Parameters.Count, target.FullName);
            }).ToArray();
        return new RealStS2PrepareMethodRewrite.RewriteEvidence(
            Convert.ToHexString(SHA1.HashData(raw)).ToLowerInvariant(),
            Convert.ToHexString(SHA256.HashData(raw)).ToLowerInvariant(),
            raw.LongLength,
            module.Assembly.Name.FullName,
            module.Mvid,
            method.DeclaringType.FullName,
            method.FullName,
            method.MetadataToken.ToUInt32(),
            RealStS2PrepareMethodSemanticAudit.ComputeMethodBodyFingerprint(method),
            method.Body.Instructions.Count,
            method.Body.ExceptionHandlers.Count,
            sites);
    }

    private static MethodDefinition FindPrewarmJit(ModuleDefinition module)
        => module.Types.SelectMany(EnumerateTypes).SelectMany(type => type.Methods)
            .Single(value => value.DeclaringType.FullName == "MegaCrit.Sts2.Core.Helpers.OneTimeInitialization" && value.Name == "PrewarmJit");

    private static int CountPrepareMethod(MethodDefinition method)
        => method.Body.Instructions.Count(instruction => instruction.Operand is MethodReference reference &&
            reference.DeclaringType.FullName == "System.Runtime.CompilerServices.RuntimeHelpers" && reference.Name == "PrepareMethod");

    private static IEnumerable<TypeDefinition> EnumerateTypes(TypeDefinition root)
    {
        yield return root;
        foreach (var nested in root.NestedTypes.SelectMany(EnumerateTypes)) yield return nested;
    }

    private static void WriteSyntheticPrimaryAssembly(
        string path,
        bool branchToFirstPrepareMethod,
        bool includeUnauditedExternalConstant = false)
    {
        using var syntheticRuntime = CreateSyntheticSystemRuntime();
        using var syntheticSentry = CreateSyntheticSentry();
        using var syntheticUnexpected = includeUnauditedExternalConstant ? CreateSyntheticUnexpectedDependency() : null;
        var resolverAssemblies = syntheticUnexpected is null
            ? new[] { syntheticRuntime, syntheticSentry }
            : new[] { syntheticRuntime, syntheticSentry, syntheticUnexpected };
        using var sourceWriteResolver = new MultiAssemblyResolver(resolverAssemblies);
        using var assembly = AssemblyDefinition.CreateAssembly(
            new AssemblyNameDefinition("sts2", new Version(0, 1, 0, 0)),
            "sts2",
            new ModuleParameters { Kind = ModuleKind.Dll, AssemblyResolver = sourceWriteResolver });
        var module = assembly.MainModule;

        var runtimeAssembly = CloneReference(syntheticRuntime.Name);
        var sentryAssembly = CloneReference(syntheticSentry.Name);
        module.AssemblyReferences.Add(runtimeAssembly);
        module.AssemblyReferences.Add(sentryAssembly);

        var type = new TypeDefinition(
            "MegaCrit.Sts2.Core.Helpers",
            "OneTimeInitialization",
            TypeAttributes.Public | TypeAttributes.Abstract | TypeAttributes.Sealed,
            module.TypeSystem.Object);
        module.Types.Add(type);
        var runtimeHelpers = new TypeReference("System.Runtime.CompilerServices", "RuntimeHelpers", module, runtimeAssembly);
        var runtimeMethodHandle = new TypeReference("System", "RuntimeMethodHandle", module, runtimeAssembly, true);
        var runtimeTypeHandle = new TypeReference("System", "RuntimeTypeHandle", module, runtimeAssembly, true);
        var runtimeTypeHandleArray = new ArrayType(runtimeTypeHandle);
        var prepareOne = new MethodReference("PrepareMethod", module.TypeSystem.Void, runtimeHelpers) { HasThis = false };
        prepareOne.Parameters.Add(new ParameterDefinition(runtimeMethodHandle));
        var prepareTwo = new MethodReference("PrepareMethod", module.TypeSystem.Void, runtimeHelpers) { HasThis = false };
        prepareTwo.Parameters.Add(new ParameterDefinition(runtimeMethodHandle));
        prepareTwo.Parameters.Add(new ParameterDefinition(runtimeTypeHandleArray));

        var constantHolder = new TypeDefinition(
            "MegaCrit.Sts2.Core.Helpers",
            "SyntheticConstantHolder",
            TypeAttributes.NotPublic | TypeAttributes.Abstract | TypeAttributes.Sealed,
            module.TypeSystem.Object);
        var bindingFlags = new TypeReference("System.Reflection", "BindingFlags", module, runtimeAssembly, true);
        constantHolder.Fields.Add(new FieldDefinition(
            "InstanceMemberBindingFlags",
            FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.Literal | FieldAttributes.HasDefault,
            bindingFlags)
        {
            Constant = 52,
        });
        module.Types.Add(constantHolder);

        var sentryService = new TypeDefinition(
            "MegaCrit.Sts2.Core.Debug",
            "SentryService",
            TypeAttributes.NotPublic | TypeAttributes.Abstract | TypeAttributes.Sealed,
            module.TypeSystem.Object);
        module.Types.Add(sentryService);
        var breadcrumbLevel = new TypeReference("Sentry", "BreadcrumbLevel", module, sentryAssembly, true);
        var sentryLevel = new TypeReference("Sentry", "SentryLevel", module, sentryAssembly, true);

        var addBreadcrumb = new MethodDefinition(
            "AddBreadcrumb",
            MethodAttributes.Public | MethodAttributes.Static,
            module.TypeSystem.Void);
        addBreadcrumb.Parameters.Add(new ParameterDefinition("category", ParameterAttributes.None, module.TypeSystem.String));
        addBreadcrumb.Parameters.Add(new ParameterDefinition("message", ParameterAttributes.None, module.TypeSystem.String));
        addBreadcrumb.Parameters.Add(new ParameterDefinition(
            "level",
            ParameterAttributes.Optional | ParameterAttributes.HasDefault,
            breadcrumbLevel)
        {
            Constant = 0,
        });
        addBreadcrumb.Body.GetILProcessor().Append(addBreadcrumb.Body.GetILProcessor().Create(OpCodes.Ret));
        sentryService.Methods.Add(addBreadcrumb);

        var captureMessage = new MethodDefinition(
            "CaptureMessage",
            MethodAttributes.Public | MethodAttributes.Static,
            module.TypeSystem.Void);
        captureMessage.Parameters.Add(new ParameterDefinition("message", ParameterAttributes.None, module.TypeSystem.String));
        captureMessage.Parameters.Add(new ParameterDefinition(
            "level",
            ParameterAttributes.Optional | ParameterAttributes.HasDefault,
            sentryLevel)
        {
            Constant = (short)1,
        });
        captureMessage.Body.GetILProcessor().Append(captureMessage.Body.GetILProcessor().Create(OpCodes.Ret));
        sentryService.Methods.Add(captureMessage);

        if (syntheticUnexpected is not null)
        {
            var unexpectedAssembly = CloneReference(syntheticUnexpected.Name);
            module.AssemblyReferences.Add(unexpectedAssembly);
            var unexpectedEnum = new TypeReference("Unexpected", "AuditEscape", module, unexpectedAssembly, true);
            constantHolder.Fields.Add(new FieldDefinition(
                "UnauditedExternalDefault",
                FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.Literal | FieldAttributes.HasDefault,
                unexpectedEnum)
            {
                Constant = 7,
            });
        }

        var method = new MethodDefinition("PrewarmJit", MethodAttributes.Public | MethodAttributes.Static, module.TypeSystem.Void);
        type.Methods.Add(method);
        method.Body.InitLocals = true;
        var handle = new VariableDefinition(runtimeMethodHandle);
        method.Body.Variables.Add(handle);
        var il = method.Body.GetILProcessor();
        Instruction? firstPrepare = null;
        Instruction? optionalBranch = null;
        if (branchToFirstPrepareMethod)
        {
            optionalBranch = il.Create(OpCodes.Br_S, il.Create(OpCodes.Nop));
            il.Append(optionalBranch);
        }
        for (var i = 0; i < 10; i++)
        {
            il.Append(il.Create(OpCodes.Ldloc, handle));
            Instruction call;
            if (i is >= 2 and <= 5)
            {
                il.Append(il.Create(OpCodes.Ldnull));
                call = il.Create(OpCodes.Call, prepareTwo);
            }
            else
            {
                call = il.Create(OpCodes.Call, prepareOne);
            }
            il.Append(call);
            firstPrepare ??= call;
        }
        il.Append(il.Create(OpCodes.Ret));
        if (optionalBranch is not null)
            optionalBranch.Operand = firstPrepare!;
        assembly.Write(path);
    }

    private static AssemblyDefinition CreateSyntheticSystemRuntime()
    {
        var name = new AssemblyNameDefinition("System.Runtime", new Version(9, 0, 0, 0))
        {
            PublicKeyToken = Convert.FromHexString("b03f5f7f11d50a3a"),
        };
        var assembly = AssemblyDefinition.CreateAssembly(name, "System.Runtime.dll", ModuleKind.Dll);
        // An in-memory assembly named System.Runtime is treated by Cecil as a core-library module.
        // Asking its TypeSystem for Int32 attempts an image-backed core-type lookup, but this
        // synthetic fixture has no PE image. Import the CLR primitive explicitly instead; this
        // changes only the host fixture and still gives Cecil an Int32 metadata type for value__.
        AddSyntheticEnum(assembly.MainModule, "System.Reflection", "BindingFlags", assembly.MainModule.ImportReference(typeof(int)));
        return assembly;
    }

    private static AssemblyDefinition CreateSyntheticSentry()
    {
        var name = new AssemblyNameDefinition("Sentry", new Version(5, 0, 0, 0))
        {
            PublicKeyToken = Convert.FromHexString("fba2ec45388e2af0"),
        };
        var assembly = AssemblyDefinition.CreateAssembly(name, "Sentry.dll", ModuleKind.Dll);
        AddSyntheticEnum(assembly.MainModule, "Sentry", "BreadcrumbLevel", assembly.MainModule.ImportReference(typeof(int)));
        AddSyntheticEnum(assembly.MainModule, "Sentry", "SentryLevel", assembly.MainModule.ImportReference(typeof(short)));
        return assembly;
    }

    private static AssemblyDefinition CreateSyntheticUnexpectedDependency()
    {
        var assembly = AssemblyDefinition.CreateAssembly(
            new AssemblyNameDefinition("Unexpected.Dependency", new Version(1, 0, 0, 0)),
            "Unexpected.Dependency.dll",
            ModuleKind.Dll);
        AddSyntheticEnum(assembly.MainModule, "Unexpected", "AuditEscape", assembly.MainModule.ImportReference(typeof(int)));
        return assembly;
    }

    private static void AddSyntheticEnum(ModuleDefinition module, string typeNamespace, string typeName, TypeReference storageType)
    {
        var enumType = new TypeDefinition(
            typeNamespace,
            typeName,
            TypeAttributes.Public | TypeAttributes.Sealed,
            module.ImportReference(typeof(Enum)));
        enumType.Fields.Add(new FieldDefinition(
            "value__",
            FieldAttributes.Public | FieldAttributes.SpecialName | FieldAttributes.RTSpecialName,
            storageType));
        module.Types.Add(enumType);
    }

    private static AssemblyNameReference CloneReference(AssemblyNameDefinition name)
        => new(name.Name, name.Version)
        {
            Culture = name.Culture,
            PublicKeyToken = name.PublicKeyToken is null ? [] : name.PublicKeyToken.ToArray(),
        };

    private sealed class MultiAssemblyResolver(IEnumerable<AssemblyDefinition> assemblies) : IAssemblyResolver
    {
        private readonly Dictionary<string, AssemblyDefinition> _assemblies =
            assemblies.ToDictionary(assembly => assembly.Name.FullName, StringComparer.Ordinal);

        public AssemblyDefinition Resolve(AssemblyNameReference name)
            => _assemblies.TryGetValue(name.FullName, out var assembly)
                ? assembly
                : throw new AssemblyResolutionException(name);

        public AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
            => Resolve(name);

        public void Dispose() { }
    }

}
