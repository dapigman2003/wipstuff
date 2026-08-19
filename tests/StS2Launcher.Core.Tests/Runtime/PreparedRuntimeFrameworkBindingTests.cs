using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mono.Cecil;
using StS2Launcher.Core;

namespace StS2Launcher.Core.Tests;

[TestClass]
public sealed class PreparedRuntimeFrameworkBindingTests
{
    [TestMethod]
    public void OrderedRuntimeFrameworkBindingGatesReachFourOfFourPass()
    {
        var gates = new RuntimeFrameworkBindingGateSequence();
        gates.Record(RuntimeFrameworkBindingGate.RuntimePayloadClassification, true, "scope");
        gates.Record(RuntimeFrameworkBindingGate.HostFrameworkBindingPlan, true, "plan");
        gates.Record(RuntimeFrameworkBindingGate.PreparedRuntimeAssemblySet, true, "prepared");
        gates.Record(RuntimeFrameworkBindingGate.ClosureAudit, true, "audit");

        var summary = gates.Snapshot();
        Assert.IsTrue(summary.Passed);
        Assert.AreEqual(4, summary.PassedGates);
        Assert.AreEqual("PREPARED RUNTIME / FRAMEWORK BINDING PASS — 4/4", summary.Summary);
    }

    [TestMethod]
    public void RuntimeFrameworkBindingGatesStopAfterFirstFailure()
    {
        var gates = new RuntimeFrameworkBindingGateSequence();
        gates.Record(RuntimeFrameworkBindingGate.RuntimePayloadClassification, true, "scope");
        gates.Record(RuntimeFrameworkBindingGate.HostFrameworkBindingPlan, false, "plan failure");
        Assert.ThrowsExactly<InvalidOperationException>(() =>
            gates.Record(RuntimeFrameworkBindingGate.PreparedRuntimeAssemblySet, true, "must not advance"));
        Assert.AreEqual(RuntimeFrameworkBindingGate.HostFrameworkBindingPlan, gates.Snapshot().FirstFailingGate);
    }

    [TestMethod]
    public async Task RealStyleGraphPrefersHostSystemRuntimeAndPreparesOnlyPrivateIlAssemblies()
    {
        using var temp = new TempTestDirectory("sts2-step21-tests");
        var install = await CreateSyntheticInstallAsync(temp.Path, includeMissingPrivateReference: false, includeSystemNamedPrivateFallback: false);
        var primaryBefore = SHA256.HashData(await File.ReadAllBytesAsync(install.PrimaryPath));

        var binding = new PreparedRuntimeFrameworkBinding(temp.Path);
        var gateA = await binding.RunRuntimePayloadClassificationAsync();
        var gateB = binding.RunHostFrameworkBindingPlan();
        var gateC = await binding.RunPreparedRuntimeAssemblySetAsync();
        var gateD = await binding.RunClosureAuditAsync();
        var primaryAfter = SHA256.HashData(await File.ReadAllBytesAsync(install.PrimaryPath));

        Assert.IsTrue(gateA.Passed, gateA.Detail);
        Assert.IsTrue(gateB.Passed, gateB.Detail);
        Assert.IsTrue(gateC.Passed, gateC.Detail);
        Assert.IsTrue(gateD.Passed, gateD.Detail);
        CollectionAssert.AreEqual(primaryBefore, primaryAfter);

        StringAssert.Contains(gateA.Detail, "macOS x86_64 duplicates excluded: 1");
        StringAssert.Contains(gateB.Detail, "Runtime closure ready for first real CLR load: YES");
        StringAssert.Contains(gateB.Detail, "System.Linq.Expressions, Version=");
        StringAssert.Contains(gateC.Detail, "Cecil assembly writes performed by Step 21 Gate C: 0");
        StringAssert.Contains(gateD.Detail, "Original Step 12 managed install unchanged: YES");
        StringAssert.Contains(gateD.Detail, "StS2 assembly loaded/executed: NO");

        var preparedRoot = Path.Combine(temp.Path, PreparedRuntimeFrameworkBinding.WorkRootName, PreparedRuntimeFrameworkBinding.PreparedRootName);
        var preparedDlls = Directory.EnumerateFiles(preparedRoot, "*.dll", SearchOption.AllDirectories)
            .Select(Path.GetFileName)
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        CollectionAssert.AreEquivalent(new[] { "sts2.dll", "Game.Dependency.dll" }, preparedDlls!);
        Assert.IsFalse(Directory.EnumerateFiles(preparedRoot, "System.Linq.Expressions.dll", SearchOption.AllDirectories).Any());

        var planPath = Path.Combine(temp.Path, PreparedRuntimeFrameworkBinding.WorkRootName, PreparedRuntimeFrameworkBinding.PlanRootName, PreparedRuntimeFrameworkBinding.PlanFileName);
        await using var stream = File.OpenRead(planPath);
        var plan = await JsonSerializer.DeserializeAsync(stream, RuntimeFrameworkBindingJsonContext.Default.RuntimeFrameworkBindingPlanDocument);
        Assert.IsNotNull(plan);
        Assert.IsTrue(plan.RuntimeClosureReady);
        Assert.AreEqual(0, plan.Blockers.Length);
        Assert.IsTrue(plan.HostFrameworkBindings.Any(item => item.RequestedFullName.StartsWith("System.Linq.Expressions, Version=", StringComparison.Ordinal)));
        Assert.IsTrue(plan.PreparedAssemblies.Any(item => item.AssemblyFullName.StartsWith("Game.Dependency,", StringComparison.Ordinal)));
    }

    [TestMethod]
    public async Task MissingPrivateDependencyIsExplicitPlanBlockerButDoesNotCorruptPreparationAudit()
    {
        using var temp = new TempTestDirectory("sts2-step21-tests");
        await CreateSyntheticInstallAsync(temp.Path, includeMissingPrivateReference: true, includeSystemNamedPrivateFallback: false);

        var binding = new PreparedRuntimeFrameworkBinding(temp.Path);
        var gateA = await binding.RunRuntimePayloadClassificationAsync();
        var gateB = binding.RunHostFrameworkBindingPlan();
        var gateC = await binding.RunPreparedRuntimeAssemblySetAsync();
        var gateD = await binding.RunClosureAuditAsync();

        Assert.IsTrue(gateA.Passed, gateA.Detail);
        Assert.IsTrue(gateB.Passed, gateB.Detail);
        Assert.IsTrue(gateC.Passed, gateC.Detail);
        Assert.IsTrue(gateD.Passed, gateD.Detail);
        StringAssert.Contains(gateB.Detail, "Explicit binding blockers: 1");
        StringAssert.Contains(gateB.Detail, "Runtime closure ready for first real CLR load: NO");
        StringAssert.Contains(gateB.Detail, "MissingWorkspaceAssembly");
        StringAssert.Contains(gateD.Detail, "Explicit blockers preserved in plan: 1");
        StringAssert.Contains(gateD.Detail, "Runtime closure ready for first real CLR load: NO");
    }

    [TestMethod]
    public async Task SystemNamedPackageFallsBackToVerifiedWorkspaceWhenHostCannotProvideIt()
    {
        using var temp = new TempTestDirectory("sts2-step21-tests");
        await CreateSyntheticInstallAsync(temp.Path, includeMissingPrivateReference: false, includeSystemNamedPrivateFallback: true);

        var binding = new PreparedRuntimeFrameworkBinding(temp.Path);
        var gateA = await binding.RunRuntimePayloadClassificationAsync();
        var gateB = binding.RunHostFrameworkBindingPlan();
        var gateC = await binding.RunPreparedRuntimeAssemblySetAsync();
        var gateD = await binding.RunClosureAuditAsync();

        Assert.IsTrue(gateA.Passed, gateA.Detail);
        Assert.IsTrue(gateB.Passed, gateB.Detail);
        Assert.IsTrue(gateC.Passed, gateC.Detail);
        Assert.IsTrue(gateD.Passed, gateD.Detail);
        StringAssert.Contains(gateB.Detail, "Runtime closure ready for first real CLR load: YES");

        var preparedRoot = Path.Combine(temp.Path, PreparedRuntimeFrameworkBinding.WorkRootName, PreparedRuntimeFrameworkBinding.PreparedRootName);
        Assert.IsTrue(Directory.EnumerateFiles(preparedRoot, "System.StS2SyntheticPortable.dll", SearchOption.AllDirectories).Any());
    }

    private static async Task<SyntheticInstall> CreateSyntheticInstallAsync(
        string launcherRoot,
        bool includeMissingPrivateReference,
        bool includeSystemNamedPrivateFallback)
    {
        var managedRoot = Path.Combine(launcherRoot, SteamOfflineInstallInspection.ManagedRootRelativePath, "Depot-2868842");
        var arm64Root = Path.Combine(managedRoot, "SlayTheSpire2.app", "Contents", "Resources", "data_sts2_macos_arm64");
        var x86Root = Path.Combine(managedRoot, "SlayTheSpire2.app", "Contents", "Resources", "data_sts2_macos_x86_64");
        Directory.CreateDirectory(arm64Root);
        Directory.CreateDirectory(x86Root);

        var primaryPath = Path.Combine(arm64Root, "sts2.dll");
        var dependencyPath = Path.Combine(arm64Root, "Game.Dependency.dll");
        WriteAssembly(dependencyPath, "Game.Dependency", new Version(1, 0, 0, 0), []);

        var expressionsIdentity = typeof(System.Linq.Expressions.Expression).Assembly.GetName();
        var expressionsActualVersion = expressionsIdentity.Version ?? new Version(9, 0, 0, 0);
        var expressionsRequestedVersion = new Version(Math.Max(0, expressionsActualVersion.Major - 1), 0, 0, 0);
        var expressionsToken = Convert.ToHexString(expressionsIdentity.GetPublicKeyToken() ?? []).ToLowerInvariant();
        var references = new List<AssemblyReferenceSpec>
        {
            new("System.Linq.Expressions", expressionsRequestedVersion, expressionsToken),
            new("Game.Dependency", new Version(1, 0, 0, 0), string.Empty),
        };
        if (includeMissingPrivateReference)
            references.Add(new AssemblyReferenceSpec("Missing.Private.Dependency", new Version(1, 0, 0, 0), string.Empty));
        if (includeSystemNamedPrivateFallback)
        {
            var portablePath = Path.Combine(arm64Root, "System.StS2SyntheticPortable.dll");
            WriteAssembly(portablePath, "System.StS2SyntheticPortable", new Version(1, 0, 0, 0), []);
            references.Add(new AssemblyReferenceSpec("System.StS2SyntheticPortable", new Version(1, 0, 0, 0), string.Empty));
        }
        WriteAssembly(primaryPath, "sts2", new Version(1, 0, 0, 0), references);
        File.Copy(primaryPath, Path.Combine(x86Root, "sts2.dll"));

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

        var receipt = new SteamManagedInstallReceipt(
            SteamManagedInstallReceipt.CurrentSchemaVersion,
            2868840,
            2868842,
            21001UL,
            "public",
            DateTimeOffset.UtcNow,
            receiptFiles);
        await using (var stream = File.Create(Path.Combine(managedRoot, SteamManagedInstallReceipt.FileName)))
        {
            await JsonSerializer.SerializeAsync(stream, receipt, SteamManagedInstallJsonContext.Default.SteamManagedInstallReceipt);
        }
        return new SyntheticInstall(managedRoot, primaryPath);
    }

    private static void WriteAssembly(
        string path,
        string name,
        Version version,
        IReadOnlyList<AssemblyReferenceSpec> references)
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
        assembly.Write(path);
    }

    private sealed record AssemblyReferenceSpec(string Name, Version Version, string PublicKeyTokenHex);
    private sealed record SyntheticInstall(string ManagedRoot, string PrimaryPath);
}
