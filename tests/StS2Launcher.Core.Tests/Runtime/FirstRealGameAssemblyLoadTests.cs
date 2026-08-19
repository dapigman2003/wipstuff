using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
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
        ForceCollectibleContexts();
        using var temp = new TempTestDirectory("sts2-step23-tests");
        await CreateSyntheticPreparedRuntimeAsync(temp.Path, includeModuleInitializer: false, tamperPreparedAfterPlan: false);

        var results = await RunSyntheticLoadAndDisposeAsync(temp.Path);
        ForceCollectibleContexts();

        Assert.IsTrue(results.GateA.Passed, results.GateA.Detail);
        Assert.IsTrue(results.GateB.Passed, results.GateB.Detail);
        Assert.IsTrue(results.GateC.Passed, results.GateC.Detail);
        Assert.IsTrue(results.GateD.Passed, results.GateD.Detail);
        StringAssert.Contains(results.GateA.Detail, "Module initializers found: 0");
        StringAssert.Contains(results.GateB.Detail, "FIRST REAL STS2 CLR LOAD SUCCEEDED");
        StringAssert.Contains(results.GateB.Detail, "Game entry point invoked: NO");
        StringAssert.Contains(results.GateC.Detail, "Host framework requirements resolved from default context: 1");
        StringAssert.Contains(results.GateC.Detail, "Private prepared requirements resolved from Step 23 context: 1");
        StringAssert.Contains(results.GateD.Detail, "Native load attempts: 0");
        StringAssert.Contains(results.GateD.Detail, "Trusted Step 12 managed install unchanged: YES");
    }

    [TestMethod]
    public async Task GateARejectsModuleInitializerBeforeAnyRealClrLoad()
    {
        ForceCollectibleContexts();
        using var temp = new TempTestDirectory("sts2-step23-tests");
        await CreateSyntheticPreparedRuntimeAsync(temp.Path, includeModuleInitializer: true, tamperPreparedAfterPlan: false);

        using var foundation = new FirstRealGameAssemblyLoad(temp.Path, collectibleLoadContext: true);
        var gateA = await foundation.RunPreparedLoadPreflightAsync();

        Assert.IsFalse(gateA.Passed);
        StringAssert.Contains(gateA.Detail, "<Module>..cctor module initializer");
        Assert.IsFalse(AppDomain.CurrentDomain.GetAssemblies().Any(assembly =>
            string.Equals(assembly.GetName().Name, FirstRealGameAssemblyLoad.ExpectedPrimarySimpleName, StringComparison.OrdinalIgnoreCase)));
    }

    [TestMethod]
    public async Task GateARejectsPersistedPlanThatDoesNotCoverPreparedAssemblyReferences()
    {
        ForceCollectibleContexts();
        using var temp = new TempTestDirectory("sts2-step23-tests");
        var synthetic = await CreateSyntheticPreparedRuntimeAsync(temp.Path, includeModuleInitializer: false, tamperPreparedAfterPlan: false);

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
                           !new AssemblyName(edge.RequestedFullName).Name!.Equals("Game.Dependency", StringComparison.OrdinalIgnoreCase))
            .ToArray();
        await using (var output = File.Create(synthetic.PlanPath))
        {
            await JsonSerializer.SerializeAsync(
                output,
                plan with { Edges = reducedEdges },
                RuntimeFrameworkBindingJsonContext.Default.RuntimeFrameworkBindingPlanDocument);
        }

        using var foundation = new FirstRealGameAssemblyLoad(temp.Path, collectibleLoadContext: true);
        var gateA = await foundation.RunPreparedLoadPreflightAsync();

        Assert.IsFalse(gateA.Passed);
        StringAssert.Contains(gateA.Detail, "does not exactly cover the Cecil AssemblyRef metadata");
        Assert.IsFalse(AppDomain.CurrentDomain.GetAssemblies().Any(assembly =>
            string.Equals(assembly.GetName().Name, FirstRealGameAssemblyLoad.ExpectedPrimarySimpleName, StringComparison.OrdinalIgnoreCase)));
    }

    [TestMethod]
    public async Task GateARejectsPreparedByteDriftBeforeAnyRealClrLoad()
    {
        ForceCollectibleContexts();
        using var temp = new TempTestDirectory("sts2-step23-tests");
        var synthetic = await CreateSyntheticPreparedRuntimeAsync(temp.Path, includeModuleInitializer: false, tamperPreparedAfterPlan: false);
        await File.AppendAllTextAsync(synthetic.PreparedPrimaryPath, "tamper");

        using var foundation = new FirstRealGameAssemblyLoad(temp.Path, collectibleLoadContext: true);
        var gateA = await foundation.RunPreparedLoadPreflightAsync();

        Assert.IsFalse(gateA.Passed);
        StringAssert.Contains(gateA.Detail, "file length mismatch");
        Assert.IsFalse(AppDomain.CurrentDomain.GetAssemblies().Any(assembly =>
            string.Equals(assembly.GetName().Name, FirstRealGameAssemblyLoad.ExpectedPrimarySimpleName, StringComparison.OrdinalIgnoreCase)));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static async Task<GateResults> RunSyntheticLoadAndDisposeAsync(string launcherRoot)
    {
        // Async state-machine locals can remain strongly referenced by the completed Task long
        // enough to delay collectible AssemblyLoadContext unloading on some CI runtimes. Keep
        // the foundation in an explicitly nullable field and clear it in finally so subsequent
        // Step 23 tests never depend on GC/JIT lifetime heuristics from this helper.
        FirstRealGameAssemblyLoad? foundation = null;
        try
        {
            foundation = new FirstRealGameAssemblyLoad(launcherRoot, collectibleLoadContext: true);
            var gateA = await foundation.RunPreparedLoadPreflightAsync();
            var gateB = foundation.RunPrimaryAssemblyLoad();
            var gateC = foundation.RunPlannedDependencyResolution();
            var gateD = await foundation.RunLoadIsolationAuditAsync();
            return new GateResults(gateA, gateB, gateC, gateD);
        }
        finally
        {
            foundation?.Dispose();
            foundation = null;
        }
    }

    private static async Task<SyntheticPreparedRuntime> CreateSyntheticPreparedRuntimeAsync(
        string launcherRoot,
        bool includeModuleInitializer,
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

        var dependencyRelative = $"{arm64RelativeRoot}/Game.Dependency.dll";
        var primaryRelative = $"{arm64RelativeRoot}/sts2.dll";
        var liveDependency = Path.Combine(managedRoot, dependencyRelative.Replace('/', Path.DirectorySeparatorChar));
        var livePrimary = Path.Combine(managedRoot, primaryRelative.Replace('/', Path.DirectorySeparatorChar));

        WriteAssembly(
            liveDependency,
            "Game.Dependency",
            new Version(1, 0, 0, 0),
            [systemLinqReference],
            includeModuleInitializer: false);
        WriteAssembly(
            livePrimary,
            "sts2",
            new Version(0, 1, 0, 0),
            [systemLinqReference, new AssemblyReferenceSpec("Game.Dependency", new Version(1, 0, 0, 0), string.Empty)],
            includeModuleInitializer);

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
            [new RuntimeBindingHostFramework(systemLinqFullName, systemLinqFullName, string.Empty, 2)],
            [],
            [
                new RuntimeBindingEdge(primaryFullName, systemLinqFullName, "HostFramework", systemLinqFullName),
                new RuntimeBindingEdge(primaryFullName, dependencyFullName, "WorkspaceExact", dependencyFullName),
                new RuntimeBindingEdge(dependencyFullName, systemLinqFullName, "HostFramework", systemLinqFullName),
            ],
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
        assembly.MainModule.AssemblyReferences.Clear();
        foreach (var reference in references)
        {
            var item = new AssemblyNameReference(reference.Name, reference.Version);
            if (!string.IsNullOrEmpty(reference.PublicKeyTokenHex))
                item.PublicKeyToken = Convert.FromHexString(reference.PublicKeyTokenHex);
            assembly.MainModule.AssemblyReferences.Add(item);
        }

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

        assembly.Write(path);
    }

    private static void ForceCollectibleContexts()
    {
        // Collectible ALC unloading is GC-driven. Wait for the observable Step 23 synthetic
        // assembly to disappear instead of assuming a fixed small number of collections is enough.
        // This is host-test isolation only; the physical iOS Step 23 context remains non-collectible.
        for (var i = 0; i < 16; i++)
        {
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
            GC.WaitForPendingFinalizers();
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);

            if (!AppDomain.CurrentDomain.GetAssemblies().Any(assembly =>
                    string.Equals(assembly.GetName().Name, FirstRealGameAssemblyLoad.ExpectedPrimarySimpleName, StringComparison.OrdinalIgnoreCase) &&
                    AssemblyLoadContext.GetLoadContext(assembly)?.IsCollectible == true))
            {
                return;
            }

            Thread.Sleep(10);
        }

        Assert.Fail("A collectible synthetic Step 23 sts2 assembly remained loaded after forced cleanup; host tests must not depend on test ordering or collectible ALC GC timing.");
    }

    private sealed record AssemblyReferenceSpec(string Name, Version Version, string PublicKeyTokenHex);
    private sealed record SyntheticPreparedRuntime(string PreparedPrimaryPath, string PlanPath);
    private sealed record GateResults(
        FirstRealGameAssemblyLoadGateResult GateA,
        FirstRealGameAssemblyLoadGateResult GateB,
        FirstRealGameAssemblyLoadGateResult GateC,
        FirstRealGameAssemblyLoadGateResult GateD);
}
