using System.Security.Cryptography;
using System.Text.Json;
using StS2Launcher.Core;
using StS2Launcher.Step16.Fixture;

namespace StS2Launcher.Core.Tests;

[TestClass]
public sealed class ManagedPreparationFoundationTests
{
    [TestMethod]
    public void OrderedManagedPreparationGatesReachFourOfFourPass()
    {
        var gates = new ManagedPreparationGateSequence();
        gates.Record(ManagedPreparationGate.FixtureRead, true, "fixture read");
        gates.Record(ManagedPreparationGate.FixtureRoundTrip, true, "round trip");
        gates.Record(ManagedPreparationGate.ControlledIlRewrite, true, "rewrite");
        gates.Record(ManagedPreparationGate.RealStS2MetadataInspection, true, "real metadata");

        var summary = gates.Snapshot();
        Assert.IsTrue(summary.Passed);
        Assert.AreEqual(4, summary.PassedGates);
        Assert.AreEqual("MANAGED PREPARATION PASS — 4/4", summary.Summary);
    }

    [TestMethod]
    public void ManagedPreparationStopsAtFirstFailingGate()
    {
        var gates = new ManagedPreparationGateSequence();
        gates.Record(ManagedPreparationGate.FixtureRead, true, "fixture read");
        gates.Record(ManagedPreparationGate.FixtureRoundTrip, false, "failed");

        Assert.ThrowsExactly<InvalidOperationException>(() =>
            gates.Record(ManagedPreparationGate.ControlledIlRewrite, true, "must not advance"));

        var summary = gates.Snapshot();
        Assert.IsFalse(summary.Passed);
        Assert.AreEqual(ManagedPreparationGate.FixtureRoundTrip, summary.FirstFailingGate);
    }

    [TestMethod]
    public void ManagedPreparationRejectsOutOfOrderGate()
    {
        var gates = new ManagedPreparationGateSequence();
        Assert.ThrowsExactly<InvalidOperationException>(() =>
            gates.Record(ManagedPreparationGate.ControlledIlRewrite, true, "out of order"));
    }

    [TestMethod]
    public void ProjectOwnedFixtureReadRoundTripAndRewritePass()
    {
        using var temp = new TemporaryDirectory();
        var foundation = new ManagedPreparationFoundation(temp.Path);
        var fixturePath = typeof(FixtureTarget).Assembly.Location;

        var read = foundation.RunFixtureRead(fixturePath);
        var roundTrip = foundation.RunFixtureRoundTrip(fixturePath);
        var rewrite = foundation.RunControlledIlRewrite(fixturePath);

        Assert.IsTrue(read.Passed, read.Detail);
        Assert.IsTrue(roundTrip.Passed, roundTrip.Detail);
        Assert.IsTrue(rewrite.Passed, rewrite.Detail);
        StringAssert.Contains(read.Detail, "RewriteMe IL constant: 7");
        StringAssert.Contains(rewrite.Detail, "RewriteMe 7 → 42");
        StringAssert.Contains(rewrite.Detail, "Real StS2 install modified: NO");
    }

    [TestMethod]
    public async Task RealAssemblyInspectionUsesReceiptBackedInstallReadOnly()
    {
        using var temp = new TemporaryDirectory();
        var managedPath = System.IO.Path.Combine(
            temp.Path,
            SteamOfflineInstallInspection.ManagedRootRelativePath,
            "Depot-2868842");
        var relative = "data_sts2_macos_arm64/sts2.dll";
        var target = System.IO.Path.Combine(managedPath, relative.Replace('/', System.IO.Path.DirectorySeparatorChar));
        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(target)!);
        File.Copy(typeof(FixtureTarget).Assembly.Location, target, overwrite: true);

        var bytes = await File.ReadAllBytesAsync(target);
        var sha1 = Convert.ToHexString(SHA1.HashData(bytes)).ToLowerInvariant();
        var receipt = new SteamManagedInstallReceipt(
            SteamManagedInstallReceipt.CurrentSchemaVersion,
            2868840,
            2868842,
            123456789UL,
            "public",
            DateTimeOffset.UtcNow,
            [new SteamManagedInstallFile(relative, bytes.LongLength, sha1)]);
        await using (var stream = File.Create(System.IO.Path.Combine(managedPath, SteamManagedInstallReceipt.FileName)))
        {
            await JsonSerializer.SerializeAsync(
                stream,
                receipt,
                SteamManagedInstallJsonContext.Default.SteamManagedInstallReceipt);
        }

        var before = SHA256.HashData(await File.ReadAllBytesAsync(target));
        var foundation = new ManagedPreparationFoundation(temp.Path);
        var result = await foundation.RunRealStS2MetadataInspectionAsync();
        var after = SHA256.HashData(await File.ReadAllBytesAsync(target));

        Assert.IsTrue(result.Passed, result.Detail);
        CollectionAssert.AreEqual(before, after);
        StringAssert.Contains(result.Detail, "OfflineReady precondition: YES");
        StringAssert.Contains(result.Detail, "Managed modules parsed by Cecil: 1");
        StringAssert.Contains(result.Detail, "Post-inspection candidate SHA-1s reverified: 1/1");
        StringAssert.Contains(result.Detail, "Assembly dependency resolution attempted: NO");
        StringAssert.Contains(result.Detail, "sts2.dll receipt SHA-1 preserved after inspection: YES");
        StringAssert.Contains(result.Detail, "Network attempted: NO");
        StringAssert.Contains(result.Detail, "Real managed install modified: NO");
        StringAssert.Contains(result.Detail, "Game assembly loaded/executed: NO");
    }

    [TestMethod]
    public async Task RealAssemblyInspectionSelectsMacOsArm64Sts2WhenDepotContainsBothArchitectures()
    {
        using var temp = new TemporaryDirectory();
        var managedPath = System.IO.Path.Combine(temp.Path, SteamOfflineInstallInspection.ManagedRootRelativePath, "Depot-2868842");
        var paths = new[]
        {
            "SlayTheSpire2.app/Contents/Resources/data_sts2_macos_arm64/sts2.dll",
            "SlayTheSpire2.app/Contents/Resources/data_sts2_macos_x86_64/sts2.dll",
        };
        var files = new List<SteamManagedInstallFile>();
        foreach (var relative in paths)
        {
            var target = System.IO.Path.Combine(managedPath, relative.Replace('/', System.IO.Path.DirectorySeparatorChar));
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(target)!);
            File.Copy(typeof(FixtureTarget).Assembly.Location, target, overwrite: true);
            var bytes = await File.ReadAllBytesAsync(target);
            files.Add(new SteamManagedInstallFile(relative, bytes.LongLength, Convert.ToHexString(SHA1.HashData(bytes)).ToLowerInvariant()));
        }

        var receipt = new SteamManagedInstallReceipt(
            SteamManagedInstallReceipt.CurrentSchemaVersion, 2868840, 2868842, 123456789UL, "public", DateTimeOffset.UtcNow, files);
        await using (var stream = File.Create(System.IO.Path.Combine(managedPath, SteamManagedInstallReceipt.FileName)))
        {
            await JsonSerializer.SerializeAsync(stream, receipt, SteamManagedInstallJsonContext.Default.SteamManagedInstallReceipt);
        }

        var foundation = new ManagedPreparationFoundation(temp.Path);
        var result = await foundation.RunRealStS2MetadataInspectionAsync();

        Assert.IsTrue(result.Passed, result.Detail);
        StringAssert.Contains(result.Detail, "sts2.dll candidates discovered: 2");
        StringAssert.Contains(result.Detail, "Selected primary StS2 assembly: SlayTheSpire2.app/Contents/Resources/data_sts2_macos_arm64/sts2.dll");
        StringAssert.Contains(result.Detail, "Post-inspection candidate SHA-1s reverified: 2/2");
    }

    private sealed class TemporaryDirectory : IDisposable
    {
        public TemporaryDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "sts2-step16-" + Guid.NewGuid().ToString("N"));
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
