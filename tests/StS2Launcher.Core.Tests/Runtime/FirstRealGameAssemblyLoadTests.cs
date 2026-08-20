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
public sealed class FirstRealGameAssemblyLoadTests
{
    [TestMethod]
    public void OrderedFirstRealLoadGatesReachFourOfFourPass()
    {
        var gates = new FirstRealGameAssemblyLoadGateSequence();
        gates.Record(new FirstRealGameAssemblyLoadGateResult(FirstRealGameAssemblyLoadGate.PreparedLoadPreflight, true, "preflight"));
        gates.Record(new FirstRealGameAssemblyLoadGateResult(FirstRealGameAssemblyLoadGate.PrimaryAssemblyLoad, true, "load"));
        gates.Record(new FirstRealGameAssemblyLoadGateResult(FirstRealGameAssemblyLoadGate.PlannedDependencyResolution, true, "bindings"));
        gates.Record(new FirstRealGameAssemblyLoadGateResult(FirstRealGameAssemblyLoadGate.LoadIsolationAudit, true, "audit"));

        var summary = gates.Snapshot();
        Assert.IsTrue(summary.Passed);
        Assert.AreEqual(4, summary.PassedGates);
        Assert.AreEqual("FIRST REAL STS2 CLR LOAD BOUNDARY PASS — 4/4", summary.Summary);
    }

    [TestMethod]
    public void FirstRealLoadGatesStopAfterFirstFailure()
    {
        var gates = new FirstRealGameAssemblyLoadGateSequence();
        gates.Record(new FirstRealGameAssemblyLoadGateResult(FirstRealGameAssemblyLoadGate.PreparedLoadPreflight, true, "preflight"));
        gates.Record(new FirstRealGameAssemblyLoadGateResult(FirstRealGameAssemblyLoadGate.PrimaryAssemblyLoad, false, "load failed"));
        Assert.ThrowsExactly<InvalidOperationException>(() =>
            gates.Record(new FirstRealGameAssemblyLoadGateResult(FirstRealGameAssemblyLoadGate.PlannedDependencyResolution, true, "must not advance")));
        Assert.AreEqual(FirstRealGameAssemblyLoadGate.PrimaryAssemblyLoad, gates.Snapshot().FirstFailingGate);
    }

    [TestMethod]
    public async Task SyntheticZeroBlockerPreparedRuntimeLoadsAndResolvesWithoutInvokingGameCode()
    {
        var primarySimpleName = CreateSyntheticPrimarySimpleName();
        using var temp = new TempTestDirectory("sts2-step23-tests");
        await CreateSyntheticPreparedRuntimeAsync(temp.Path, primarySimpleName, primaryModuleInitializer: false, dependencyModuleInitializer: false, tamperPreparedAfterPlan: false);

        var results = await RunSyntheticLoadAndDisposeAsync(temp.Path, primarySimpleName);

        Assert.IsTrue(results.GateA.Passed, results.GateA.Detail);
        Assert.IsTrue(results.GateB.Passed, results.GateB.Detail);
        Assert.IsTrue(results.GateC.Passed, results.GateC.Detail);
        Assert.IsTrue(results.GateD.Passed, results.GateD.Detail);
        StringAssert.Contains(results.GateA.Detail, "Primary module initializers: 0");
        StringAssert.Contains(results.GateB.Detail, "FIRST REAL STS2 CLR LOAD SUCCEEDED");
        StringAssert.Contains(results.GateB.Detail, "Game entry point invoked: NO");
        StringAssert.Contains(results.GateC.Detail, "Host framework requirements resolved from default context: 1");
        StringAssert.Contains(results.GateC.Detail, "Private prepared requirements resolved from Step 23 context: 1");
        StringAssert.Contains(results.GateD.Detail, "Native load attempts: 0");
        StringAssert.Contains(results.GateD.Detail, "Trusted Step 12 managed install unchanged: YES");
    }

    [TestMethod]
    public async Task GateARejectsPrimaryModuleInitializerBeforeAnyRealClrLoad()
    {
        var primarySimpleName = CreateSyntheticPrimarySimpleName();
        using var temp = new TempTestDirectory("sts2-step23-tests");
        await CreateSyntheticPreparedRuntimeAsync(temp.Path, primarySimpleName, primaryModuleInitializer: true, dependencyModuleInitializer: false, tamperPreparedAfterPlan: false);

        using var foundation = CreateSyntheticFoundation(temp.Path, primarySimpleName);
        var gateA = await foundation.RunPreparedLoadPreflightAsync();

        Assert.IsFalse(gateA.Passed);
        StringAssert.Contains(gateA.Detail, "<Module>..cctor module initializer");
        AssertSyntheticPrimaryNotLoaded(primarySimpleName);
    }

    [TestMethod]
    public async Task DependencyModuleInitializerIsDeferredWhilePrimaryAndSafeClosureLoad()
    {
        var primarySimpleName = CreateSyntheticPrimarySimpleName();
        using var temp = new TempTestDirectory("sts2-step23-tests");
        await CreateSyntheticPreparedRuntimeAsync(
            temp.Path,
            primarySimpleName,
            primaryModuleInitializer: false,
            dependencyModuleInitializer: true,
            tamperPreparedAfterPlan: false);

        var results = await RunSyntheticLoadAndDisposeAsync(temp.Path, primarySimpleName);

        Assert.IsTrue(results.GateA.Passed, results.GateA.Detail);
        Assert.IsTrue(results.GateB.Passed, results.GateB.Detail);
        Assert.IsTrue(results.GateC.Passed, results.GateC.Detail);
        Assert.IsTrue(results.GateD.Passed, results.GateD.Detail);
        StringAssert.Contains(results.GateA.Detail, "Deferred initializer-bearing private assemblies: 1");
        StringAssert.Contains(results.GateA.Detail, "IL_0000: Ret");
        StringAssert.Contains(results.GateC.Detail, "Deferred initializer-bearing private requirements: 1");
        StringAssert.Contains(results.GateD.Detail, "Initializer-bearing prepared dependencies loaded: 0/1");
    }

    [TestMethod]
    public async Task GateARejectsPersistedPlanThatDoesNotCoverPreparedAssemblyReferences()
    {
        var primarySimpleName = CreateSyntheticPrimarySimpleName();
        using var temp = new TempTestDirectory("sts2-step23-tests");
        var synthetic = await CreateSyntheticPreparedRuntimeAsync(temp.Path, primarySimpleName, primaryModuleInitializer: false, dependencyModuleInitializer: false, tamperPreparedAfterPlan: false);

        RuntimeFrameworkBindingPlanDocument plan;
        await using (var input = File.OpenRead(synthetic.PlanPath))
        {
            plan = await JsonSerializer.DeserializeAsync(
                input,
                RuntimeFrameworkBindingJsonContext.Default.RuntimeFrameworkBindingPlanDocument)
                ?? throw new AssertFailedException("Synthetic Step 23 plan could not be read.");
        }
        var reducedEdges = plan.Edges
            .Where(edge => !edge.SourceAssemblyFullName.Equals(plan.PrimaryAssemblyFullName, StringComparison.Ordinal) ||
                           !edge.BindingKind.Equals("WorkspaceExact", StringComparison.Ordinal))
            .ToArray();
        await using (var output = File.Create(synthetic.PlanPath))
        {
            await JsonSerializer.SerializeAsync(
                output,
                plan with { Edges = reducedEdges },
                RuntimeFrameworkBindingJsonContext.Default.RuntimeFrameworkBindingPlanDocument);
        }

        using var foundation = CreateSyntheticFoundation(temp.Path, primarySimpleName);
        var gateA = await foundation.RunPreparedLoadPreflightAsync();

        Assert.IsFalse(gateA.Passed);
        StringAssert.Contains(gateA.Detail, "does not exactly cover the Cecil AssemblyRef metadata");
        AssertSyntheticPrimaryNotLoaded(primarySimpleName);
    }

    [TestMethod]
    public async Task GateARejectsPreparedByteDriftBeforeAnyRealClrLoad()
    {
        var primarySimpleName = CreateSyntheticPrimarySimpleName();
        using var temp = new TempTestDirectory("sts2-step23-tests");
        var synthetic = await CreateSyntheticPreparedRuntimeAsync(temp.Path, primarySimpleName, primaryModuleInitializer: false, dependencyModuleInitializer: false, tamperPreparedAfterPlan: false);
        await File.AppendAllTextAsync(synthetic.PreparedPrimaryPath, "tamper");

        using var foundation = CreateSyntheticFoundation(temp.Path, primarySimpleName);
        var gateA = await foundation.RunPreparedLoadPreflightAsync();

        Assert.IsFalse(gateA.Passed);
        StringAssert.Contains(gateA.Detail, "file length mismatch");
        AssertSyntheticPrimaryNotLoaded(primarySimpleName);
    }

    private static async Task<GateResults> RunSyntheticLoadAndDisposeAsync(string launcherRoot, string primarySimpleName)
    {
        using var foundation = CreateSyntheticFoundation(launcherRoot, primarySimpleName);
        var gateA = await foundation.RunPreparedLoadPreflightAsync();
        var gateB = foundation.RunPrimaryAssemblyLoad();
        var gateC = foundation.RunPlannedDependencyResolution();
        var gateD = await foundation.RunLoadIsolationAuditAsync();
        return new GateResults(gateA, gateB, gateC, gateD);
    }

    private static FirstRealGameAssemblyLoad CreateSyntheticFoundation(string launcherRoot, string primarySimpleName)
        => new(
            launcherRoot,
            collectibleLoadContext: true,
            expectedPrimarySimpleName: primarySimpleName,
            freshProcessAssemblyNames: [primarySimpleName]);

    private static string CreateSyntheticPrimarySimpleName()
        => "StS2Launcher.Step23.SyntheticPrimary." + Guid.NewGuid().ToString("N");

    private static void AssertSyntheticPrimaryNotLoaded(string primarySimpleName)
        => Assert.IsFalse(
            AppDomain.CurrentDomain.GetAssemblies().Any(assembly =>
                string.Equals(assembly.GetName().Name, primarySimpleName, StringComparison.OrdinalIgnoreCase)),
            $"Synthetic primary '{primarySimpleName}' was loaded before the intended Step 23 CLR-load boundary.");

    private static async Task<SyntheticPreparedRuntime> CreateSyntheticPreparedRuntimeAsync(
        string launcherRoot,
        string primarySimpleName,
        bool primaryModuleInitializer,
        bool dependencyModuleInitializer,
        bool tamperPreparedAfterPlan)
    {
        var managedRelative = $"{SteamOfflineInstallInspection.ManagedRootRelativePath}/Depot-2868842";
        var managedRoot = Path.Combine(launcherRoot, managedRelative.Replace('/', Path.DirectorySeparatorChar));
        var arm64RelativeRoot = "SlayTheSpire2.app/Contents/Resources/data_sts2_macos_arm64";
        var liveRoot = Path.Combine(managedRoot, arm64RelativeRoot.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(liveRoot);

        var systemLinq = typeof(Enumerable).Assembly.GetName();
        var systemLinqFullName = systemLinq.FullName ?? throw new AssertFailedException("System.Linq has no FullName.");
        var systemLinqReference = new AssemblyReferenceSpec(
            systemLinq.Name ?? "System.Linq",
            systemLinq.Version ?? new Version(9, 0, 0, 0),
            Convert.ToHexString(systemLinq.GetPublicKeyToken() ?? []).ToLowerInvariant());

        var dependencySimpleName = "StS2Launcher.Step23.SyntheticDependency." + primarySimpleName.Split('.').Last();
        var dependencyRelative = $"{arm64RelativeRoot}/{dependencySimpleName}.dll";
        var primaryRelative = $"{arm64RelativeRoot}/sts2.dll";
        var liveDependency = Path.Combine(managedRoot, dependencyRelative.Replace('/', Path.DirectorySeparatorChar));
        var livePrimary = Path.Combine(managedRoot, primaryRelative.Replace('/', Path.DirectorySeparatorChar));

        WriteAssembly(
            liveDependency,
            dependencySimpleName,
            new Version(1, 0, 0, 0),
            [systemLinqReference],
            includeModuleInitializer: dependencyModuleInitializer);
        WriteAssembly(
            livePrimary,
            primarySimpleName,
            new Version(0, 1, 0, 0),
            [systemLinqReference, new AssemblyReferenceSpec(dependencySimpleName, new Version(1, 0, 0, 0), string.Empty)],
            primaryModuleInitializer);

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

        const ulong manifestId = 23001UL;
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
        var preparedDependency = Path.Combine(preparedRoot, dependencyRelative.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(preparedPrimary)!);
        File.Copy(livePrimary, preparedPrimary);
        File.Copy(liveDependency, preparedDependency);

        var primaryInfo = BuildPreparedPlanItem(preparedPrimary, primaryRelative, isPrimary: true);
        var dependencyInfo = BuildPreparedPlanItem(preparedDependency, dependencyRelative, isPrimary: false);
        var primaryFullName = primaryInfo.AssemblyFullName;
        var dependencyFullName = dependencyInfo.AssemblyFullName;

        var syntheticPlan = BuildSyntheticBindingPlan(
            preparedPrimary,
            primaryFullName,
            preparedDependency,
            dependencyFullName,
            dependencySimpleName,
            systemLinqFullName);

        var plan = new RuntimeFrameworkBindingPlanDocument(
            RuntimeFrameworkBindingPlanDocument.CurrentSchemaVersion,
            SteamOfflineInstallInspection.TargetAppId,
            2868842,
            manifestId,
            "public",
            managedRelative,
            primaryRelative,
            primaryFullName,
            [primaryInfo, dependencyInfo],
            syntheticPlan.HostFrameworkBindings,
            [],
            syntheticPlan.Edges,
            true);

        var planPath = Path.Combine(planRoot, PreparedRuntimeFrameworkBinding.PlanFileName);
        await using (var planStream = File.Create(planPath))
        {
            await JsonSerializer.SerializeAsync(planStream, plan, RuntimeFrameworkBindingJsonContext.Default.RuntimeFrameworkBindingPlanDocument);
        }

        if (tamperPreparedAfterPlan)
            await File.AppendAllTextAsync(preparedPrimary, "tamper");

        return new SyntheticPreparedRuntime(preparedPrimary, planPath);
    }


    private static SyntheticBindingPlan BuildSyntheticBindingPlan(
        string preparedPrimary,
        string primaryFullName,
        string preparedDependency,
        string dependencyFullName,
        string dependencySimpleName,
        string systemLinqFullName)
    {
        var edges = new List<RuntimeBindingEdge>();
        AddSyntheticEdges(preparedPrimary, primaryFullName, dependencyFullName, dependencySimpleName, systemLinqFullName, edges);
        AddSyntheticEdges(preparedDependency, dependencyFullName, dependencyFullName, dependencySimpleName, systemLinqFullName, edges);

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

        return new SyntheticBindingPlan(
            hostBindings,
            edges.OrderBy(edge => edge.SourceAssemblyFullName, StringComparer.Ordinal)
                .ThenBy(edge => edge.RequestedFullName, StringComparer.Ordinal)
                .ToArray());
    }

    private static void AddSyntheticEdges(
        string assemblyPath,
        string sourceFullName,
        string dependencyFullName,
        string dependencySimpleName,
        string systemLinqFullName,
        ICollection<RuntimeBindingEdge> edges)
    {
        using var module = ModuleDefinition.ReadModule(assemblyPath, new ReaderParameters
        {
            InMemory = true,
            ReadSymbols = false,
            ReadingMode = ReadingMode.Deferred,
        });

        foreach (var reference in module.AssemblyReferences)
        {
            if (reference.Name.Equals(dependencySimpleName, StringComparison.OrdinalIgnoreCase))
            {
                edges.Add(new RuntimeBindingEdge(sourceFullName, reference.FullName, "WorkspaceExact", dependencyFullName));
                continue;
            }

            if (reference.Name.Equals("System.Linq", StringComparison.OrdinalIgnoreCase))
            {
                edges.Add(new RuntimeBindingEdge(sourceFullName, reference.FullName, "HostFramework", systemLinqFullName));
                continue;
            }

            throw new AssertFailedException(
                $"Synthetic Step 23 fixture emitted an unexpected AssemblyRef '{reference.FullName}'. " +
                "Update the fixture binding-plan builder rather than weakening Gate A metadata coverage.");
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
        bool includeModuleInitializer)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        using var assembly = AssemblyDefinition.CreateAssembly(
            new AssemblyNameDefinition(name, version),
            name,
            ModuleKind.Dll);
        if (includeModuleInitializer)
        {
            var moduleType = assembly.MainModule.Types.Single(type => type.Name == "<Module>");
            var initializer = new MethodDefinition(
                ".cctor",
                Mono.Cecil.MethodAttributes.Private | Mono.Cecil.MethodAttributes.Static | Mono.Cecil.MethodAttributes.SpecialName | Mono.Cecil.MethodAttributes.RTSpecialName,
                assembly.MainModule.TypeSystem.Void);
            initializer.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
            moduleType.Methods.Add(initializer);
        }

        // Cecil's TypeSystem.Void may temporarily materialize a legacy mscorlib AssemblyRef
        // while constructing synthetic metadata. Normalize the fixture's AssemblyRef table only
        // after the initializer exists so the written .NET 9 test assembly contains exactly the
        // references this fixture intentionally declares. The production Step 23 resolver remains
        // strict and never aliases mscorlib to System.Private.CoreLib.
        assembly.MainModule.AssemblyReferences.Clear();
        foreach (var reference in references)
        {
            var item = new AssemblyNameReference(reference.Name, reference.Version);
            if (!string.IsNullOrEmpty(reference.PublicKeyTokenHex))
                item.PublicKeyToken = Convert.FromHexString(reference.PublicKeyTokenHex);
            assembly.MainModule.AssemblyReferences.Add(item);
        }

        assembly.Write(path);

        using var written = ModuleDefinition.ReadModule(path, new ReaderParameters
        {
            InMemory = true,
            ReadSymbols = false,
            ReadingMode = ReadingMode.Deferred,
        });
        if (written.AssemblyReferences.Any(reference => reference.Name.Equals("mscorlib", StringComparison.OrdinalIgnoreCase)))
        {
            throw new AssertFailedException(
                "Synthetic Step 23 fixture unexpectedly retained a legacy mscorlib AssemblyRef. " +
                "Fix the fixture generator rather than adding a production core-library alias.");
        }
    }

    private sealed record AssemblyReferenceSpec(string Name, Version Version, string PublicKeyTokenHex);
    private sealed record SyntheticBindingPlan(RuntimeBindingHostFramework[] HostFrameworkBindings, RuntimeBindingEdge[] Edges);
    private sealed record SyntheticPreparedRuntime(string PreparedPrimaryPath, string PlanPath);
    private sealed record GateResults(
        FirstRealGameAssemblyLoadGateResult GateA,
        FirstRealGameAssemblyLoadGateResult GateB,
        FirstRealGameAssemblyLoadGateResult GateC,
        FirstRealGameAssemblyLoadGateResult GateD);
}
