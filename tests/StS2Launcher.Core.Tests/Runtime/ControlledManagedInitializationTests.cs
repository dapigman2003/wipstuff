using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mono.Cecil;
using Mono.Cecil.Cil;
using StS2Launcher.Core;

namespace StS2Launcher.Core.Tests;

[TestClass]
[DoNotParallelize]
public sealed class ControlledManagedInitializationTests
{
    [TestMethod]
    public void OrderedInitializationGatesReachFourOfFourPass()
    {
        var gates = new ControlledManagedInitializationGateSequence();
        gates.Record(new ControlledManagedInitializationGateResult(ControlledManagedInitializationGate.InitializationPreflight, true, "preflight"));
        gates.Record(new ControlledManagedInitializationGateResult(ControlledManagedInitializationGate.ProvenLoadStateReplay, true, "replay"));
        gates.Record(new ControlledManagedInitializationGateResult(ControlledManagedInitializationGate.DeferredModuleInitialization, true, "initialize"));
        gates.Record(new ControlledManagedInitializationGateResult(ControlledManagedInitializationGate.PostInitializationAudit, true, "audit"));

        var summary = gates.Snapshot();
        Assert.IsTrue(summary.Passed);
        Assert.AreEqual(4, summary.PassedGates);
        Assert.AreEqual("CONTROLLED MANAGED INITIALIZATION BOUNDARY PASS — 4/4", summary.Summary);
    }

    [TestMethod]
    public void InitializationGatesStopAfterFirstFailure()
    {
        var gates = new ControlledManagedInitializationGateSequence();
        gates.Record(new ControlledManagedInitializationGateResult(ControlledManagedInitializationGate.InitializationPreflight, true, "preflight"));
        gates.Record(new ControlledManagedInitializationGateResult(ControlledManagedInitializationGate.ProvenLoadStateReplay, false, "replay failed"));
        Assert.ThrowsExactly<InvalidOperationException>(() =>
            gates.Record(new ControlledManagedInitializationGateResult(ControlledManagedInitializationGate.DeferredModuleInitialization, true, "must not advance")));
        Assert.AreEqual(ControlledManagedInitializationGate.ProvenLoadStateReplay, gates.Snapshot().FirstFailingGate);
    }

    [TestMethod]
    public async Task SyntheticDeferredModuleInitializerCompletesAndAuditPasses()
    {
        var names = CreateNames();
        using var temp = new TempTestDirectory("sts2-step24-tests");
        await CreateSyntheticPreparedRuntimeAsync(temp.Path, names, InitializerKind.Return);

        using var boundary = CreateSyntheticBoundary(temp.Path, names);
        var gateA = await boundary.RunInitializationPreflightAsync();
        Assert.IsTrue(gateA.Passed, gateA.Detail);
        StringAssert.Contains(gateA.Detail, "Initializer hazards: 0");
        StringAssert.Contains(gateA.Detail, "IL_0000: Ret");

        var gateB = boundary.RunProvenLoadStateReplay();
        Assert.IsTrue(gateB.Passed, gateB.Detail);
        StringAssert.Contains(gateB.Detail, "0Harmony loaded: NO");

        var gateC = boundary.RunDeferredModuleInitialization();
        Assert.IsTrue(gateC.Passed, gateC.Detail);
        StringAssert.Contains(gateC.Detail, "RuntimeHelpers.RunModuleConstructor completion barrier: PASS");
        StringAssert.Contains(gateC.Detail, "Native load attempts during target load/initializer: 0");

        var gateD = await boundary.RunPostInitializationAuditAsync();
        Assert.IsTrue(gateD.Passed, gateD.Detail);
        StringAssert.Contains(gateD.Detail, "Native load attempts: 0");
        StringAssert.Contains(gateD.Detail, "Explicit Harmony patching/API invocation: NO");
    }

    [TestMethod]
    public async Task GateARejectsReachablePInvokeBeforeAnyStep24ClrLoad()
    {
        var names = CreateNames();
        using var temp = new TempTestDirectory("sts2-step24-tests");
        await CreateSyntheticPreparedRuntimeAsync(temp.Path, names, InitializerKind.PInvoke);

        using var boundary = CreateSyntheticBoundary(temp.Path, names);
        var gateA = await boundary.RunInitializationPreflightAsync();

        Assert.IsFalse(gateA.Passed);
        StringAssert.Contains(gateA.Detail, "P/Invoke reachable");
        AssertNotLoaded(names.PrimarySimpleName);
        AssertNotLoaded(names.TargetSimpleName);
    }

    [TestMethod]
    public async Task GateARejectsFunctionPointerIndirectionBeforeAnyStep24ClrLoad()
    {
        var names = CreateNames();
        using var temp = new TempTestDirectory("sts2-step24-tests");
        await CreateSyntheticPreparedRuntimeAsync(temp.Path, names, InitializerKind.FunctionPointer);

        using var boundary = CreateSyntheticBoundary(temp.Path, names);
        var gateA = await boundary.RunInitializationPreflightAsync();

        Assert.IsFalse(gateA.Passed);
        StringAssert.Contains(gateA.Detail, "indirect function/delegate target reachable");
        AssertNotLoaded(names.PrimarySimpleName);
        AssertNotLoaded(names.TargetSimpleName);
    }

    [TestMethod]
    public async Task GateARejectsImplicitTypeInitializerPInvokeBeforeAnyStep24ClrLoad()
    {
        var names = CreateNames();
        using var temp = new TempTestDirectory("sts2-step24-tests");
        await CreateSyntheticPreparedRuntimeAsync(temp.Path, names, InitializerKind.TypeInitializerPInvoke);

        using var boundary = CreateSyntheticBoundary(temp.Path, names);
        var gateA = await boundary.RunInitializationPreflightAsync();

        Assert.IsFalse(gateA.Passed);
        StringAssert.Contains(gateA.Detail, "P/Invoke reachable");
        AssertNotLoaded(names.PrimarySimpleName);
        AssertNotLoaded(names.TargetSimpleName);
    }

    [TestMethod]
    public void GateAMetadataAuditDoesNotResolveExternalBaseForNominallyLocalMemberRef()
    {
        using var temp = new TempTestDirectory("sts2-step24-tests");
        var path = Path.Combine(temp.Path, "resolver-free-memberref.dll");
        WriteExternalBaseMemberRefFixture(path);

        var reader = typeof(ControlledManagedInitialization).GetMethod(
            "ReadPreparedMetadata",
            BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new AssertFailedException("Step 24 metadata reader was not found.");

        object? snapshot;
        try
        {
            snapshot = reader.Invoke(null, [path, true, "SyntheticHarmony"]);
        }
        catch (TargetInvocationException ex)
        {
            Assert.Fail("Gate A metadata reader attempted external resolution instead of failing closed from local metadata: " + ex.InnerException);
            return;
        }

        Assert.IsNotNull(snapshot);
        var hazards = snapshot.GetType().GetProperty("InitializerHazards")?.GetValue(snapshot) as IReadOnlyList<string>;
        Assert.IsNotNull(hazards);
        Assert.IsTrue(
            hazards.Any(value => value.Contains("Unresolved same-assembly call (local metadata only)", StringComparison.Ordinal)),
            "The nominally local inherited MemberRef should become an unresolved-local hazard without consulting GodotSharp metadata.");
    }

    [TestMethod]
    public async Task GateCReportsThrowingModuleInitializerAndDoesNotAdvance()
    {
        var names = CreateNames();
        using var temp = new TempTestDirectory("sts2-step24-tests");
        await CreateSyntheticPreparedRuntimeAsync(temp.Path, names, InitializerKind.ThrowNull);

        using var boundary = CreateSyntheticBoundary(temp.Path, names);
        var gateA = await boundary.RunInitializationPreflightAsync();
        Assert.IsTrue(gateA.Passed, gateA.Detail);
        var gateB = boundary.RunProvenLoadStateReplay();
        Assert.IsTrue(gateB.Passed, gateB.Detail);

        var gateC = boundary.RunDeferredModuleInitialization();
        Assert.IsFalse(gateC.Passed);
        StringAssert.Contains(gateC.Detail, "Stage:");
    }

    private static ControlledManagedInitialization CreateSyntheticBoundary(string launcherRoot, SyntheticNames names)
        => new(
            launcherRoot,
            collectibleLoadContext: true,
            expectedPrimarySimpleName: names.PrimarySimpleName,
            targetSimpleName: names.TargetSimpleName,
            targetVersion: ControlledManagedInitialization.TargetVersion,
            freshProcessAssemblyNames: [names.PrimarySimpleName, names.TargetSimpleName]);

    private static SyntheticNames CreateNames()
    {
        var suffix = Guid.NewGuid().ToString("N");
        return new SyntheticNames(
            "StS2Launcher.Step24.SyntheticPrimary." + suffix,
            "StS2Launcher.Step24.SyntheticHarmony." + suffix);
    }

    private static void AssertNotLoaded(string simpleName)
        => Assert.IsFalse(
            AppDomain.CurrentDomain.GetAssemblies().Any(assembly =>
                string.Equals(assembly.GetName().Name, simpleName, StringComparison.OrdinalIgnoreCase)),
            $"Synthetic Step 24 assembly '{simpleName}' was loaded before the intended gate.");

    private static async Task CreateSyntheticPreparedRuntimeAsync(
        string launcherRoot,
        SyntheticNames names,
        InitializerKind initializerKind)
    {
        var managedRelative = $"{SteamOfflineInstallInspection.ManagedRootRelativePath}/Depot-2868842";
        var managedRoot = Path.Combine(launcherRoot, managedRelative.Replace('/', Path.DirectorySeparatorChar));
        var arm64RelativeRoot = "SlayTheSpire2.app/Contents/Resources/data_sts2_macos_arm64";
        var liveRoot = Path.Combine(managedRoot, arm64RelativeRoot.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(liveRoot);

        var systemLinq = typeof(Enumerable).Assembly.GetName();
        var systemLinqFullName = systemLinq.FullName ?? throw new AssertFailedException("System.Linq has no FullName.");
        var systemLinqReference = ToReferenceSpec(systemLinq, "System.Linq");

        var systemRuntime = Assembly.Load(new AssemblyName("System.Runtime")).GetName();
        var systemRuntimeFullName = systemRuntime.FullName ?? throw new AssertFailedException("System.Runtime has no FullName.");
        var systemRuntimeReference = ToReferenceSpec(systemRuntime, "System.Runtime");

        var targetRelative = $"{arm64RelativeRoot}/{names.TargetSimpleName}.dll";
        var primaryRelative = $"{arm64RelativeRoot}/sts2.dll";
        var liveTarget = Path.Combine(managedRoot, targetRelative.Replace('/', Path.DirectorySeparatorChar));
        var livePrimary = Path.Combine(managedRoot, primaryRelative.Replace('/', Path.DirectorySeparatorChar));

        WriteAssembly(
            liveTarget,
            names.TargetSimpleName,
            ControlledManagedInitialization.TargetVersion,
            [systemLinqReference, systemRuntimeReference],
            initializerKind);
        WriteAssembly(
            livePrimary,
            names.PrimarySimpleName,
            new Version(0, 1, 0, 0),
            [systemLinqReference, new(names.TargetSimpleName, ControlledManagedInitialization.TargetVersion, string.Empty)],
            InitializerKind.None);

        var receiptFiles = new List<SteamManagedInstallFile>();
        foreach (var path in Directory.EnumerateFiles(managedRoot, "*", SearchOption.AllDirectories).OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
        {
            var relative = Path.GetRelativePath(managedRoot, path).Replace(Path.DirectorySeparatorChar, '/');
            var bytes = await File.ReadAllBytesAsync(path);
            receiptFiles.Add(new SteamManagedInstallFile(
                relative,
                bytes.LongLength,
                Convert.ToHexString(SHA1.HashData(bytes)).ToLowerInvariant()));
        }

        const ulong manifestId = 24001UL;
        var receipt = new SteamManagedInstallReceipt(
            SteamManagedInstallReceipt.CurrentSchemaVersion,
            SteamOfflineInstallInspection.TargetAppId,
            2868842,
            manifestId,
            "public",
            DateTimeOffset.UtcNow,
            receiptFiles);
        await using (var receiptStream = File.Create(Path.Combine(managedRoot, SteamManagedInstallReceipt.FileName)))
        {
            await JsonSerializer.SerializeAsync(receiptStream, receipt, SteamManagedInstallJsonContext.Default.SteamManagedInstallReceipt);
        }

        var step21Root = Path.Combine(launcherRoot, PreparedRuntimeFrameworkBinding.WorkRootName);
        var preparedRoot = Path.Combine(step21Root, PreparedRuntimeFrameworkBinding.PreparedRootName);
        var planRoot = Path.Combine(step21Root, PreparedRuntimeFrameworkBinding.PlanRootName);
        Directory.CreateDirectory(planRoot);
        var preparedPrimary = Path.Combine(preparedRoot, primaryRelative.Replace('/', Path.DirectorySeparatorChar));
        var preparedTarget = Path.Combine(preparedRoot, targetRelative.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(preparedPrimary)!);
        File.Copy(livePrimary, preparedPrimary);
        File.Copy(liveTarget, preparedTarget);

        var primaryInfo = BuildPreparedPlanItem(preparedPrimary, primaryRelative, isPrimary: true);
        var targetInfo = BuildPreparedPlanItem(preparedTarget, targetRelative, isPrimary: false);

        var edges = new List<RuntimeBindingEdge>();
        AddSyntheticEdges(
            preparedPrimary,
            primaryInfo.AssemblyFullName,
            targetInfo.AssemblyFullName,
            names.TargetSimpleName,
            systemLinqFullName,
            systemRuntimeFullName,
            edges);
        AddSyntheticEdges(
            preparedTarget,
            targetInfo.AssemblyFullName,
            targetInfo.AssemblyFullName,
            names.TargetSimpleName,
            systemLinqFullName,
            systemRuntimeFullName,
            edges);

        var hostBindings = edges
            .Where(edge => edge.BindingKind.Equals("HostFramework", StringComparison.Ordinal))
            .GroupBy(edge => (edge.RequestedFullName, edge.Target))
            .OrderBy(group => group.Key.RequestedFullName, StringComparer.Ordinal)
            .Select(group => new RuntimeBindingHostFramework(
                group.Key.RequestedFullName,
                group.Key.Target,
                string.Empty,
                group.Count()))
            .ToArray();

        var plan = new RuntimeFrameworkBindingPlanDocument(
            RuntimeFrameworkBindingPlanDocument.CurrentSchemaVersion,
            SteamOfflineInstallInspection.TargetAppId,
            2868842,
            manifestId,
            "public",
            managedRelative,
            primaryRelative,
            primaryInfo.AssemblyFullName,
            [primaryInfo, targetInfo],
            hostBindings,
            [],
            edges.OrderBy(edge => edge.SourceAssemblyFullName, StringComparer.Ordinal)
                .ThenBy(edge => edge.RequestedFullName, StringComparer.Ordinal)
                .ToArray(),
            true);

        await using var planStream = File.Create(Path.Combine(planRoot, PreparedRuntimeFrameworkBinding.PlanFileName));
        await JsonSerializer.SerializeAsync(planStream, plan, RuntimeFrameworkBindingJsonContext.Default.RuntimeFrameworkBindingPlanDocument);
    }

    private static void AddSyntheticEdges(
        string assemblyPath,
        string sourceFullName,
        string targetFullName,
        string targetSimpleName,
        string systemLinqFullName,
        string systemRuntimeFullName,
        ICollection<RuntimeBindingEdge> edges)
    {
        using var module = ModuleDefinition.ReadModule(assemblyPath, new ReaderParameters
        {
            InMemory = true,
            ReadSymbols = false,
            ReadingMode = ReadingMode.Immediate,
        });

        foreach (var reference in module.AssemblyReferences)
        {
            if (reference.Name.Equals(targetSimpleName, StringComparison.OrdinalIgnoreCase))
            {
                edges.Add(new RuntimeBindingEdge(sourceFullName, reference.FullName, "WorkspaceExact", targetFullName));
                continue;
            }
            if (reference.Name.Equals("System.Linq", StringComparison.OrdinalIgnoreCase))
            {
                edges.Add(new RuntimeBindingEdge(sourceFullName, reference.FullName, "HostFramework", systemLinqFullName));
                continue;
            }
            if (reference.Name.Equals("System.Runtime", StringComparison.OrdinalIgnoreCase))
            {
                edges.Add(new RuntimeBindingEdge(sourceFullName, reference.FullName, "HostFramework", systemRuntimeFullName));
                continue;
            }
            throw new AssertFailedException("Synthetic Step 24 fixture emitted an unexpected AssemblyRef: " + reference.FullName);
        }
    }

    private static RuntimeBindingPreparedAssembly BuildPreparedPlanItem(string path, string relativePath, bool isPrimary)
    {
        using var module = ModuleDefinition.ReadModule(path, new ReaderParameters { InMemory = true, ReadSymbols = false });
        var fullName = module.Assembly?.Name.FullName ?? throw new AssertFailedException("Synthetic assembly has no identity.");
        var bytes = File.ReadAllBytes(path);
        return new RuntimeBindingPreparedAssembly(
            relativePath,
            fullName,
            Convert.ToHexString(SHA1.HashData(bytes)).ToLowerInvariant(),
            bytes.LongLength,
            isPrimary);
    }

    private static void WriteAssembly(
        string path,
        string name,
        Version version,
        IReadOnlyList<AssemblyReferenceSpec> references,
        InitializerKind initializerKind)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        using var assembly = AssemblyDefinition.CreateAssembly(
            new AssemblyNameDefinition(name, version),
            name,
            ModuleKind.Dll);

        var declaredReferences = new Dictionary<string, AssemblyNameReference>(StringComparer.OrdinalIgnoreCase);
        foreach (var reference in references)
        {
            var item = CreateAssemblyNameReference(reference);
            declaredReferences.Add(reference.Name, item);
            assembly.MainModule.AssemblyReferences.Add(item);
        }

        if (initializerKind != InitializerKind.None)
        {
            if (!declaredReferences.TryGetValue("System.Runtime", out var systemRuntimeReference))
                throw new AssertFailedException("Synthetic Step 24 initializer requires predeclared System.Runtime.");

            var voidType = assembly.MainModule.TypeSystem.Void;
            Assert.AreSame(systemRuntimeReference, voidType.Scope);
            var moduleType = assembly.MainModule.Types.Single(type => type.Name == "<Module>");
            var initializer = new MethodDefinition(
                ".cctor",
                Mono.Cecil.MethodAttributes.Private |
                Mono.Cecil.MethodAttributes.Static |
                Mono.Cecil.MethodAttributes.SpecialName |
                Mono.Cecil.MethodAttributes.RTSpecialName,
                voidType);
            moduleType.Methods.Add(initializer);

            switch (initializerKind)
            {
                case InitializerKind.Return:
                    initializer.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
                    break;
                case InitializerKind.ThrowNull:
                    initializer.Body.Instructions.Add(Instruction.Create(OpCodes.Ldnull));
                    initializer.Body.Instructions.Add(Instruction.Create(OpCodes.Throw));
                    break;
                case InitializerKind.PInvoke:
                    var pinvokeModule = new ModuleReference("libstep24fixture.dylib");
                    assembly.MainModule.ModuleReferences.Add(pinvokeModule);
                    var nativeProbe = CreatePInvokeProbe(voidType, pinvokeModule);
                    moduleType.Methods.Add(nativeProbe);
                    initializer.Body.Instructions.Add(Instruction.Create(OpCodes.Call, nativeProbe));
                    initializer.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
                    break;
                case InitializerKind.FunctionPointer:
                    var helper = new MethodDefinition(
                        "IndirectTarget",
                        Mono.Cecil.MethodAttributes.Private | Mono.Cecil.MethodAttributes.Static,
                        voidType);
                    helper.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
                    moduleType.Methods.Add(helper);
                    initializer.Body.Instructions.Add(Instruction.Create(OpCodes.Ldftn, helper));
                    initializer.Body.Instructions.Add(Instruction.Create(OpCodes.Pop));
                    initializer.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
                    break;
                case InitializerKind.TypeInitializerPInvoke:
                    var typeInitModule = new ModuleReference("libstep24fixture-typeinit.dylib");
                    assembly.MainModule.ModuleReferences.Add(typeInitModule);
                    var autoInitType = new TypeDefinition(
                        "StS2Launcher.Step24.Tests",
                        "AutoInitType",
                        Mono.Cecil.TypeAttributes.NotPublic | Mono.Cecil.TypeAttributes.Abstract | Mono.Cecil.TypeAttributes.Sealed,
                        assembly.MainModule.TypeSystem.Object);
                    assembly.MainModule.Types.Add(autoInitType);
                    var typeInitializer = new MethodDefinition(
                        ".cctor",
                        Mono.Cecil.MethodAttributes.Private |
                        Mono.Cecil.MethodAttributes.Static |
                        Mono.Cecil.MethodAttributes.SpecialName |
                        Mono.Cecil.MethodAttributes.RTSpecialName,
                        voidType);
                    autoInitType.Methods.Add(typeInitializer);
                    var typeNativeProbe = CreatePInvokeProbe(voidType, typeInitModule);
                    autoInitType.Methods.Add(typeNativeProbe);
                    typeInitializer.Body.Instructions.Add(Instruction.Create(OpCodes.Call, typeNativeProbe));
                    typeInitializer.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
                    var touch = new MethodDefinition(
                        "Touch",
                        Mono.Cecil.MethodAttributes.Public | Mono.Cecil.MethodAttributes.Static,
                        voidType);
                    touch.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
                    autoInitType.Methods.Add(touch);
                    initializer.Body.Instructions.Add(Instruction.Create(OpCodes.Call, touch));
                    initializer.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(initializerKind));
            }
        }

        assembly.Write(path);

        using var written = ModuleDefinition.ReadModule(path, new ReaderParameters
        {
            InMemory = true,
            ReadSymbols = false,
            ReadingMode = ReadingMode.Immediate,
        });
        Assert.IsFalse(written.AssemblyReferences.Any(reference => reference.Name.Equals("mscorlib", StringComparison.OrdinalIgnoreCase)));
        if (initializerKind != InitializerKind.None)
        {
            var initializer = written.Types.Single(type => type.Name == "<Module>").Methods.Single(method => method.Name == ".cctor");
            Assert.AreEqual(MetadataType.Void, initializer.ReturnType.MetadataType);
            Assert.AreEqual("System.Runtime", initializer.ReturnType.Scope?.Name);
        }
    }

    private static void WriteExternalBaseMemberRefFixture(string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        using var assembly = AssemblyDefinition.CreateAssembly(
            new AssemblyNameDefinition("SyntheticHarmony", ControlledManagedInitialization.TargetVersion),
            "SyntheticHarmony",
            ModuleKind.Dll);

        var runtimeName = Assembly.Load(new AssemblyName("System.Runtime")).GetName();
        var runtimeReference = CreateAssemblyNameReference(ToReferenceSpec(runtimeName, "System.Runtime"));
        assembly.MainModule.AssemblyReferences.Add(runtimeReference);
        var voidType = assembly.MainModule.TypeSystem.Void;
        Assert.AreSame(runtimeReference, voidType.Scope);

        var godotReference = new AssemblyNameReference("GodotSharp", new Version(4, 5, 1, 0));
        assembly.MainModule.AssemblyReferences.Add(godotReference);
        var unavailableBase = new TypeReference(
            "Godot",
            "GodotObject",
            assembly.MainModule,
            godotReference,
            false);
        var derived = new TypeDefinition(
            "StS2Launcher.Step24.Tests",
            "DerivedFromUnavailableGodot",
            Mono.Cecil.TypeAttributes.NotPublic,
            unavailableBase);
        assembly.MainModule.Types.Add(derived);

        var moduleType = assembly.MainModule.Types.Single(type => type.Name == "<Module>");
        var initializer = new MethodDefinition(
            ".cctor",
            Mono.Cecil.MethodAttributes.Private |
            Mono.Cecil.MethodAttributes.Static |
            Mono.Cecil.MethodAttributes.SpecialName |
            Mono.Cecil.MethodAttributes.RTSpecialName,
            voidType);
        moduleType.Methods.Add(initializer);

        // Deliberately declare the MemberRef on the local derived type without defining the method
        // there. A general Cecil Resolve() would be allowed to walk the unavailable Godot base type;
        // Step 24 must instead stop from local metadata with an unresolved-local hazard.
        var inheritedReference = new MethodReference("InheritedTouch", voidType, derived)
        {
            HasThis = false,
        };
        initializer.Body.Instructions.Add(Instruction.Create(OpCodes.Call, inheritedReference));
        initializer.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
        assembly.Write(path);
    }

    private static MethodDefinition CreatePInvokeProbe(TypeReference voidType, ModuleReference nativeModule)
        => new(
            "NativeProbe",
            Mono.Cecil.MethodAttributes.Private |
            Mono.Cecil.MethodAttributes.Static |
            Mono.Cecil.MethodAttributes.PInvokeImpl,
            voidType)
        {
            PInvokeInfo = new PInvokeInfo(Mono.Cecil.PInvokeAttributes.CallConvCdecl, "step24_fixture_probe", nativeModule),
        };

    private static AssemblyNameReference CreateAssemblyNameReference(AssemblyReferenceSpec reference)
    {
        var item = new AssemblyNameReference(reference.Name, reference.Version);
        if (!string.IsNullOrEmpty(reference.PublicKeyTokenHex))
            item.PublicKeyToken = Convert.FromHexString(reference.PublicKeyTokenHex);
        return item;
    }

    private static AssemblyReferenceSpec ToReferenceSpec(AssemblyName name, string fallbackName)
        => new(
            name.Name ?? fallbackName,
            name.Version ?? new Version(9, 0, 0, 0),
            Convert.ToHexString(name.GetPublicKeyToken() ?? []).ToLowerInvariant());

    private enum InitializerKind
    {
        None,
        Return,
        ThrowNull,
        PInvoke,
        FunctionPointer,
        TypeInitializerPInvoke,
    }

    private sealed record SyntheticNames(string PrimarySimpleName, string TargetSimpleName);
    private sealed record AssemblyReferenceSpec(string Name, Version Version, string PublicKeyTokenHex);
}
