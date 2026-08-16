using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StS2Launcher.Core;

namespace StS2Launcher.Core.Tests;

[TestClass]
public sealed class SteamManagedInstallTests
{
    [TestMethod]
    public void Step12TargetAppIdRemainsSlayTheSpire2()
    {
        Assert.AreEqual(2868840u, SteamManagedInstallAttempt.TargetAppId);
        Assert.AreEqual(SteamResumableDepotDownloadAttempt.TargetAppId, SteamManagedInstallAttempt.TargetAppId);
    }

    [TestMethod]
    public void StateClassifierDistinguishesInstallUpdateRepairAndCurrent()
    {
        var receipt = new SteamManagedInstallReceipt(
            SteamManagedInstallReceipt.CurrentSchemaVersion,
            2868840,
            123,
            456,
            "public",
            DateTimeOffset.UnixEpoch,
            Array.Empty<SteamManagedInstallFile>());

        Assert.AreEqual(
            SteamManagedInstallState.NotInstalled,
            SteamManagedInstallAttempt.DetermineStateFromReceipt(false, null, 123, 456, false));
        Assert.AreEqual(
            SteamManagedInstallState.RepairNeeded,
            SteamManagedInstallAttempt.DetermineStateFromReceipt(true, null, 123, 456, false));
        Assert.AreEqual(
            SteamManagedInstallState.UpdateAvailable,
            SteamManagedInstallAttempt.DetermineStateFromReceipt(true, receipt, 123, 999, true));
        Assert.AreEqual(
            SteamManagedInstallState.RepairNeeded,
            SteamManagedInstallAttempt.DetermineStateFromReceipt(true, receipt, 123, 456, false));
        Assert.AreEqual(
            SteamManagedInstallState.UpToDate,
            SteamManagedInstallAttempt.DetermineStateFromReceipt(true, receipt, 123, 456, true));
    }


    [TestMethod]
    public void StateClassifierTreatsMalformedOrForeignReceiptAsRepairNeeded()
    {
        static SteamManagedInstallReceipt Receipt(
            uint appId = 2868840u,
            IReadOnlyList<SteamManagedInstallFile>? files = null) =>
            new(
                SteamManagedInstallReceipt.CurrentSchemaVersion,
                appId,
                123u,
                456UL,
                "public",
                DateTimeOffset.UnixEpoch,
                files ?? new[]
                {
                    new SteamManagedInstallFile("safe.bin", 1, new string('A', 40)),
                });

        Assert.AreEqual(
            SteamManagedInstallState.RepairNeeded,
            SteamManagedInstallAttempt.DetermineStateFromReceipt(true, Receipt(appId: 999u), 123u, 456UL, true));

        Assert.AreEqual(
            SteamManagedInstallState.RepairNeeded,
            SteamManagedInstallAttempt.DetermineStateFromReceipt(
                true,
                Receipt(files: new[] { new SteamManagedInstallFile("../escape.bin", 1, new string('A', 40)) }),
                123u,
                456UL,
                true));

        Assert.AreEqual(
            SteamManagedInstallState.RepairNeeded,
            SteamManagedInstallAttempt.DetermineStateFromReceipt(
                true,
                Receipt(files: new[] { new SteamManagedInstallFile("safe.bin", -1, new string('A', 40)) }),
                123u,
                456UL,
                true));

        Assert.AreEqual(
            SteamManagedInstallState.RepairNeeded,
            SteamManagedInstallAttempt.DetermineStateFromReceipt(
                true,
                Receipt(files: new[] { new SteamManagedInstallFile("safe.bin", 1, "not-a-sha1") }),
                123u,
                456UL,
                true));

        Assert.AreEqual(
            SteamManagedInstallState.RepairNeeded,
            SteamManagedInstallAttempt.DetermineStateFromReceipt(
                true,
                Receipt(files: new[]
                {
                    new SteamManagedInstallFile("dup.bin", 1, new string('A', 40)),
                    new SteamManagedInstallFile("DUP.bin", 1, new string('B', 40)),
                }),
                123u,
                456UL,
                true));
    }

    [TestMethod]
    public void ReceiptContainsOnlyNonSecretIntegrityMetadata()
    {
        var names = typeof(SteamManagedInstallReceipt).GetProperties().Select(p => p.Name).ToArray();
        Assert.IsFalse(names.Any(name => name.Contains("Token", StringComparison.OrdinalIgnoreCase)));
        Assert.IsFalse(names.Any(name => name.Contains("Key", StringComparison.OrdinalIgnoreCase)));
        Assert.IsFalse(names.Any(name => name.Contains("Password", StringComparison.OrdinalIgnoreCase)));
        Assert.IsFalse(names.Any(name => name.Contains("Guard", StringComparison.OrdinalIgnoreCase)));

        var fileNames = typeof(SteamManagedInstallFile).GetProperties().Select(p => p.Name).ToArray();
        CollectionAssert.AreEquivalent(new[] { "RelativePath", "Length", "Sha1Hex" }, fileNames);
    }

    [TestMethod]
    public void ReceiptJsonUsesSourceGeneratedMetadataAndRoundTrips()
    {
        var receipt = new SteamManagedInstallReceipt(
            SteamManagedInstallReceipt.CurrentSchemaVersion,
            2868840u,
            2868842u,
            8653035385353091849UL,
            "public",
            DateTimeOffset.UnixEpoch,
            new[]
            {
                new SteamManagedInstallFile("bin/example.dat", 1234, "00112233445566778899AABBCCDDEEFF00112233"),
            });

        var json = JsonSerializer.Serialize(
            receipt,
            SteamManagedInstallJsonContext.Default.SteamManagedInstallReceipt);
        var restored = JsonSerializer.Deserialize(
            json,
            SteamManagedInstallJsonContext.Default.SteamManagedInstallReceipt);

        Assert.IsNotNull(restored);
        Assert.AreEqual(receipt.SchemaVersion, restored.SchemaVersion);
        Assert.AreEqual(receipt.AppId, restored.AppId);
        Assert.AreEqual(receipt.DepotId, restored.DepotId);
        Assert.AreEqual(receipt.ManifestId, restored.ManifestId);
        Assert.AreEqual(receipt.Branch, restored.Branch);
        Assert.AreEqual(receipt.CreatedUtc, restored.CreatedUtc);
        Assert.AreEqual(1, restored.Files.Count);
        Assert.AreEqual(receipt.Files[0], restored.Files[0]);
        StringAssert.Contains(json, "\n");
    }


    [TestMethod]
    public void SyntheticUpdateReceiptForcesUpdateAndOneSourceReplacementIdentity()
    {
        var files = new[]
        {
            new SteamManagedInstallFile("a.bin", 10, new string('A', 40)),
            new SteamManagedInstallFile("b.bin", 20, new string('B', 40)),
        };
        var receipt = new SteamManagedInstallReceipt(
            SteamManagedInstallReceipt.CurrentSchemaVersion,
            2868840u,
            2868842u,
            123UL,
            "public",
            DateTimeOffset.UnixEpoch,
            files);

        var synthetic = SteamManagedInstallAttempt.CreateSyntheticUpdateReceipt(receipt);

        Assert.AreNotEqual(receipt.ManifestId, synthetic.ManifestId);
        Assert.AreEqual(receipt.Files.Count, synthetic.Files.Count);
        Assert.AreEqual(1, receipt.Files.Zip(synthetic.Files).Count(pair => pair.First != pair.Second));
        Assert.IsTrue(receipt.Files.Zip(synthetic.Files).Any(pair =>
            pair.First.RelativePath == pair.Second.RelativePath &&
            pair.First.Length == pair.Second.Length &&
            !string.Equals(pair.First.Sha1Hex, pair.Second.Sha1Hex, StringComparison.OrdinalIgnoreCase)));
        Assert.AreEqual(
            SteamManagedInstallState.UpdateAvailable,
            SteamManagedInstallAttempt.DetermineStateFromReceipt(true, synthetic, synthetic.DepotId, receipt.ManifestId, false));
    }

    [TestMethod]
    public void SuccessfulResultContractIncludesAtomicReplacementProof()
    {
        var properties = typeof(SteamManagedInstallResult).GetProperties().Select(p => p.Name).ToHashSet();
        Assert.IsTrue(properties.Contains(nameof(SteamManagedInstallResult.ExistingInstallPreservedUntilCommit)));
        Assert.IsTrue(properties.Contains(nameof(SteamManagedInstallResult.SourceCacheReverifiedAgainstCurrentManifest)));
        Assert.IsTrue(properties.Contains(nameof(SteamManagedInstallResult.SourceNewlyDownloadedBytes)));
        Assert.IsTrue(properties.Contains(nameof(SteamManagedInstallResult.AtomicCommitCompleted)));
        Assert.IsTrue(properties.Contains(nameof(SteamManagedInstallResult.StagingAbsentAfterResult)));
        Assert.IsTrue(properties.Contains(nameof(SteamManagedInstallResult.BackupAbsentAfterResult)));
    }
    [TestMethod]
    public void DownloadCacheMaintenanceDeletesOnlyStep11CacheAndIsIdempotent()
    {
        var root = Path.Combine(Path.GetTempPath(), "sts2-cache-maintenance-" + Guid.NewGuid().ToString("N"));
        try
        {
            var cache = Path.Combine(root, SteamDownloadCacheMaintenance.CacheRelativePath);
            var managed = Path.Combine(root, "Step12-ManagedInstall", "Depot-2868842");
            Directory.CreateDirectory(Path.Combine(cache, "complete", "2868842", "123"));
            Directory.CreateDirectory(Path.Combine(cache, ".resume", "2868842-123"));
            Directory.CreateDirectory(managed);
            File.WriteAllText(Path.Combine(cache, "complete", "2868842", "123", "cached.bin"), "cache");
            File.WriteAllText(Path.Combine(managed, "managed.bin"), "managed");

            var maintenance = new SteamDownloadCacheMaintenance(root);
            Assert.IsTrue(maintenance.Exists());

            var first = maintenance.Clear();
            Assert.IsTrue(first.CacheExisted);
            Assert.IsTrue(first.CacheAbsentAfterClear);
            Assert.IsFalse(maintenance.Exists());
            Assert.IsTrue(File.Exists(Path.Combine(managed, "managed.bin")));

            var second = maintenance.Clear();
            Assert.IsFalse(second.CacheExisted);
            Assert.IsTrue(second.CacheAbsentAfterClear);
            Assert.IsTrue(File.Exists(Path.Combine(managed, "managed.bin")));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

}
