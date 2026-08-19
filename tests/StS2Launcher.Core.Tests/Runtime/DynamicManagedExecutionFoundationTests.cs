using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mono.Cecil;
using StS2Launcher.Core;

namespace StS2Launcher.Core.Tests;

[TestClass]
public sealed class DynamicManagedExecutionFoundationTests
{
    [TestMethod]
    public void OrderedDynamicManagedExecutionGatesReachFourOfFourPass()
    {
        var gates = new DynamicManagedExecutionGateSequence();
        gates.Record(DynamicManagedExecutionGate.FixtureIntegrityAndOfflineReady, true, "fixture/offline");
        gates.Record(DynamicManagedExecutionGate.DynamicFixtureExecution, true, "dynamic");
        gates.Record(DynamicManagedExecutionGate.PrivateDependencyResolution, true, "dependency");
        gates.Record(DynamicManagedExecutionGate.IsolationAudit, true, "audit");

        var summary = gates.Snapshot();
        Assert.IsTrue(summary.Passed);
        Assert.AreEqual(4, summary.PassedGates);
        Assert.AreEqual("DYNAMIC MANAGED EXECUTION FOUNDATION PASS — 4/4", summary.Summary);
    }

    [TestMethod]
    public void DynamicManagedExecutionGatesStopAfterFirstFailure()
    {
        var gates = new DynamicManagedExecutionGateSequence();
        gates.Record(DynamicManagedExecutionGate.FixtureIntegrityAndOfflineReady, true, "fixture/offline");
        gates.Record(DynamicManagedExecutionGate.DynamicFixtureExecution, false, "load failed");
        Assert.ThrowsExactly<InvalidOperationException>(() =>
            gates.Record(DynamicManagedExecutionGate.PrivateDependencyResolution, true, "must not advance"));
        Assert.AreEqual(DynamicManagedExecutionGate.DynamicFixtureExecution, gates.Snapshot().FirstFailingGate);
    }

    [TestMethod]
    public async Task ProjectOwnedExternalIlAndPrivateDependencyExecuteWithoutTouchingManagedInstall()
    {
        var bundleRoot = RequireHostFixtureRoot();
        using var temp = new TempTestDirectory("sts2-step20-tests");
        var managedPath = await CreateOfflineReadyInstallAsync(temp.Path, 20001UL);
        var managedFile = Path.Combine(managedPath, "payload.bin");
        var before = SHA256.HashData(await File.ReadAllBytesAsync(managedFile));

        var foundation = new DynamicManagedExecutionFoundation(temp.Path, bundleRoot);
        var gateA = await foundation.RunFixtureIntegrityAndOfflineReadyAsync();
        var gateB = foundation.RunDynamicFixtureExecution();
        var gateC = foundation.RunPrivateDependencyResolution();
        var gateD = await foundation.RunIsolationAuditAsync();
        var after = SHA256.HashData(await File.ReadAllBytesAsync(managedFile));

        Assert.IsTrue(gateA.Passed, gateA.Detail);
        Assert.IsTrue(gateB.Passed, gateB.Detail);
        Assert.IsTrue(gateC.Passed, gateC.Detail);
        Assert.IsTrue(gateD.Passed, gateD.Detail);
        CollectionAssert.AreEqual(before, after);

        StringAssert.Contains(gateA.Detail, "Bundled fixture files SHA-256 verified: 3/3");
        StringAssert.Contains(gateB.Detail, "Dynamic fixture result: 42 (expected 42)");
        StringAssert.Contains(gateB.Detail, "Execution mechanism proven: runtime-loaded IL can execute");
        StringAssert.Contains(gateC.Detail, "Dependent fixture result: 42 (expected 42)");
        StringAssert.Contains(gateC.Detail, "Verified private dependency loads: 1");
        StringAssert.Contains(gateD.Detail, "Post-execution OfflineReady exact-tree verification: YES");
        StringAssert.Contains(gateD.Detail, "StS2 assembly loaded/executed: NO");
    }

    [TestMethod]
    public async Task GateARejectsUnexpectedNonFrameworkFixtureReferenceEvenWithValidHashManifest()
    {
        var sourceBundle = RequireHostFixtureRoot();
        using var temp = new TempTestDirectory("sts2-step20-tests");
        await CreateOfflineReadyInstallAsync(temp.Path, 20003UL);
        var modifiedBundle = Path.Combine(temp.Path, "unexpected-reference-fixtures");
        CopyFixtureBundle(sourceBundle, modifiedBundle);

        var rootPath = Path.Combine(modifiedBundle, DynamicManagedExecutionFoundation.RootFixtureFileName);
        var rewrittenPath = rootPath + ".rewritten";
        using (var module = ModuleDefinition.ReadModule(rootPath, new ReaderParameters { InMemory = true, ReadSymbols = false }))
        {
            module.AssemblyReferences.Add(new AssemblyNameReference("Unexpected.Private.Dependency", new Version(1, 0, 0, 0)));
            module.Write(rewrittenPath, new WriterParameters { WriteSymbols = false });
        }
        File.Move(rewrittenPath, rootPath, overwrite: true);
        await RewriteFixtureManifestAsync(modifiedBundle);

        var foundation = new DynamicManagedExecutionFoundation(temp.Path, modifiedBundle);
        var gateA = await foundation.RunFixtureIntegrityAndOfflineReadyAsync();

        Assert.IsFalse(gateA.Passed);
        StringAssert.Contains(gateA.Detail, "unexpected non-framework assembly reference");
    }

    [TestMethod]
    public async Task GateARejectsTamperedBundledFixtureBeforeRuntimeLoad()
    {
        var sourceBundle = RequireHostFixtureRoot();
        using var temp = new TempTestDirectory("sts2-step20-tests");
        await CreateOfflineReadyInstallAsync(temp.Path, 20002UL);
        var tamperedBundle = Path.Combine(temp.Path, "tampered-fixtures");
        CopyFixtureBundle(sourceBundle, tamperedBundle);
        await File.AppendAllTextAsync(Path.Combine(tamperedBundle, DynamicManagedExecutionFoundation.DynamicFixtureFileName), "tamper");

        var foundation = new DynamicManagedExecutionFoundation(temp.Path, tamperedBundle);
        var gateA = await foundation.RunFixtureIntegrityAndOfflineReadyAsync();

        Assert.IsFalse(gateA.Passed);
        StringAssert.Contains(gateA.Detail, "SHA-256 mismatch");
    }

    private static void CopyFixtureBundle(string sourceBundle, string destinationBundle)
    {
        Directory.CreateDirectory(destinationBundle);
        foreach (var file in Directory.EnumerateFiles(sourceBundle))
            File.Copy(file, Path.Combine(destinationBundle, Path.GetFileName(file)));
    }

    private static async Task RewriteFixtureManifestAsync(string bundleRoot)
    {
        var names = new[]
        {
            DynamicManagedExecutionFoundation.DynamicFixtureFileName,
            DynamicManagedExecutionFoundation.DependencyFixtureFileName,
            DynamicManagedExecutionFoundation.RootFixtureFileName,
        };
        var lines = new List<string>();
        foreach (var name in names)
        {
            var hash = Convert.ToHexString(SHA256.HashData(await File.ReadAllBytesAsync(Path.Combine(bundleRoot, name)))).ToLowerInvariant();
            lines.Add($"{hash}  {name}");
        }
        await File.WriteAllLinesAsync(Path.Combine(bundleRoot, DynamicManagedExecutionFoundation.ManifestFileName), lines);
    }

    private static string RequireHostFixtureRoot()
    {
        var root = Environment.GetEnvironmentVariable("STS2_STEP20_FIXTURE_ROOT");
        if (string.IsNullOrWhiteSpace(root) || !Directory.Exists(root))
            throw new AssertInconclusiveException("STS2_STEP20_FIXTURE_ROOT was not supplied by scripts/run-unit-tests-step20.sh.");
        return root;
    }

    private static async Task<string> CreateOfflineReadyInstallAsync(string launcherRoot, ulong manifestId)
    {
        var managedPath = Path.Combine(launcherRoot, SteamOfflineInstallInspection.ManagedRootRelativePath, "Depot-2868842");
        Directory.CreateDirectory(managedPath);
        var payloadPath = Path.Combine(managedPath, "payload.bin");
        var payload = new byte[] { 1, 3, 3, 7, 20 };
        await File.WriteAllBytesAsync(payloadPath, payload);

        var receipt = new SteamManagedInstallReceipt(
            SteamManagedInstallReceipt.CurrentSchemaVersion,
            2868840,
            2868842,
            manifestId,
            "public",
            DateTimeOffset.UtcNow,
            [new SteamManagedInstallFile("payload.bin", payload.LongLength, Convert.ToHexString(SHA1.HashData(payload)).ToLowerInvariant())]);
        await using var stream = File.Create(Path.Combine(managedPath, SteamManagedInstallReceipt.FileName));
        await JsonSerializer.SerializeAsync(stream, receipt, SteamManagedInstallJsonContext.Default.SteamManagedInstallReceipt);
        return managedPath;
    }
}
