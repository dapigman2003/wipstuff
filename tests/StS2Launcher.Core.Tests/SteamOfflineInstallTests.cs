using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StS2Launcher.Core;

namespace StS2Launcher.Core.Tests;

[TestClass]
public sealed class SteamOfflineInstallTests
{
    [TestMethod]
    public async Task OfflineInspectorProvesReadyFromLocalReceiptAndHashesOnly()
    {
        var root = NewRoot();
        try
        {
            var managed = CreateManagedDepot(root);
            var files = new Dictionary<string, byte[]>
            {
                ["game/data.bin"] = [1, 2, 3, 4, 5],
                ["readme.txt"] = [9, 8, 7],
            };
            await WriteManagedTreeAsync(managed, files);

            var inspector = new SteamOfflineInstallInspection(root);
            var result = await inspector.RunAsync();

            Assert.IsTrue(result.Success);
            Assert.AreEqual(SteamOfflineInstallOutcome.OfflineReady, result.Outcome);
            Assert.AreEqual(SteamOfflineInstallState.OfflineReady, result.State);
            Assert.AreEqual(2868840u, result.TargetAppId);
            Assert.IsNotNull(result.DepotId);
            Assert.IsNotNull(result.InstalledManifestId);
            Assert.AreEqual(2868842u, result.DepotId.Value);
            Assert.AreEqual(8653035385353091849UL, result.InstalledManifestId.Value);
            Assert.AreEqual("public", result.Branch);
            Assert.IsTrue(result.ManagedDirectoryFound);
            Assert.IsTrue(result.ReceiptFound);
            Assert.IsTrue(result.ReceiptStructurallyValid);
            Assert.AreEqual(files.Count, result.PlannedFiles);
            Assert.AreEqual(files.Count, result.VerifiedFiles);
            Assert.AreEqual(files.Values.Sum(bytes => (ulong)bytes.Length), result.PlannedBytes);
            Assert.AreEqual(result.PlannedBytes, result.VerifiedBytes);
            Assert.IsTrue(result.ExactManagedTreeVerified);
            Assert.IsFalse(result.SteamSessionConsulted);
            Assert.IsFalse(result.NetworkAccessAttempted);
            Assert.IsFalse(result.OnlineManifestFreshnessKnown);
            Assert.IsNull(result.Error);
        }
        finally
        {
            DeleteRoot(root);
        }
    }

    [TestMethod]
    public async Task OfflineInspectorRequiresOnlineSetupWhenManagedInstallIsAbsent()
    {
        var root = NewRoot();
        try
        {
            var inspector = new SteamOfflineInstallInspection(root);
            var result = await inspector.RunAsync();

            Assert.AreEqual(SteamOfflineInstallOutcome.NoManagedInstall, result.Outcome);
            Assert.AreEqual(SteamOfflineInstallState.OnlineSetupRequired, result.State);
            Assert.IsFalse(result.Success);
            Assert.IsFalse(result.ManagedDirectoryFound);
            Assert.IsFalse(result.SteamSessionConsulted);
            Assert.IsFalse(result.NetworkAccessAttempted);
        }
        finally
        {
            DeleteRoot(root);
        }
    }

    [TestMethod]
    public async Task OfflineInspectorRejectsCorruptOrUnexpectedManagedContent()
    {
        var root = NewRoot();
        try
        {
            var managed = CreateManagedDepot(root);
            var files = new Dictionary<string, byte[]>
            {
                ["game/data.bin"] = [1, 2, 3, 4, 5],
            };
            await WriteManagedTreeAsync(managed, files);

            await File.WriteAllBytesAsync(Path.Combine(managed, "game", "data.bin"), [5, 4, 3, 2, 1]);
            var inspector = new SteamOfflineInstallInspection(root);
            var corrupt = await inspector.RunAsync();

            Assert.AreEqual(SteamOfflineInstallOutcome.VerificationFailed, corrupt.Outcome);
            Assert.AreEqual(SteamOfflineInstallState.RepairRequired, corrupt.State);
            Assert.IsFalse(corrupt.ExactManagedTreeVerified);
            StringAssert.Contains(corrupt.Error ?? string.Empty, "SHA-1 mismatch");

            await WriteManagedTreeAsync(managed, files);
            await File.WriteAllTextAsync(Path.Combine(managed, "unexpected.bin"), "unexpected");
            var extra = await inspector.RunAsync();

            Assert.AreEqual(SteamOfflineInstallOutcome.VerificationFailed, extra.Outcome);
            Assert.AreEqual(SteamOfflineInstallState.RepairRequired, extra.State);
            StringAssert.Contains(extra.Error ?? string.Empty, "unexpected");
        }
        finally
        {
            DeleteRoot(root);
        }
    }

    [TestMethod]
    public async Task OfflineInspectorRejectsForeignReceiptWithoutContactingSteam()
    {
        var root = NewRoot();
        try
        {
            var managed = CreateManagedDepot(root);
            Directory.CreateDirectory(managed);
            await File.WriteAllBytesAsync(Path.Combine(managed, "file.bin"), [1]);
            var receipt = new SteamManagedInstallReceipt(
                SteamManagedInstallReceipt.CurrentSchemaVersion,
                999u,
                2868842u,
                123UL,
                "public",
                DateTimeOffset.UnixEpoch,
                new[]
                {
                    new SteamManagedInstallFile("file.bin", 1, Sha1Hex([1])),
                });
            await WriteReceiptAsync(managed, receipt);

            var result = await new SteamOfflineInstallInspection(root).RunAsync();

            Assert.AreEqual(SteamOfflineInstallOutcome.ReceiptMissingOrInvalid, result.Outcome);
            Assert.AreEqual(SteamOfflineInstallState.RepairRequired, result.State);
            Assert.IsFalse(result.ReceiptStructurallyValid);
            Assert.IsFalse(result.SteamSessionConsulted);
            Assert.IsFalse(result.NetworkAccessAttempted);
        }
        finally
        {
            DeleteRoot(root);
        }
    }

    [TestMethod]
    public void OfflineResultContractExplicitlySeparatesLocalReadinessFromOnlineFreshness()
    {
        var properties = typeof(SteamOfflineInstallResult).GetProperties().Select(p => p.Name).ToHashSet();
        Assert.IsTrue(properties.Contains(nameof(SteamOfflineInstallResult.ExactManagedTreeVerified)));
        Assert.IsTrue(properties.Contains(nameof(SteamOfflineInstallResult.SteamSessionConsulted)));
        Assert.IsTrue(properties.Contains(nameof(SteamOfflineInstallResult.NetworkAccessAttempted)));
        Assert.IsTrue(properties.Contains(nameof(SteamOfflineInstallResult.OnlineManifestFreshnessKnown)));
        Assert.AreEqual(2868840u, SteamOfflineInstallInspection.TargetAppId);
    }

    private static string NewRoot()
    {
        var root = Path.Combine(Path.GetTempPath(), "sts2-offline-inspection-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        return root;
    }

    private static string CreateManagedDepot(string root)
    {
        var managed = Path.Combine(root, SteamOfflineInstallInspection.ManagedRootRelativePath, "Depot-2868842");
        Directory.CreateDirectory(managed);
        return managed;
    }

    private static async Task WriteManagedTreeAsync(string managed, IReadOnlyDictionary<string, byte[]> files)
    {
        if (Directory.Exists(managed))
        {
            foreach (var file in Directory.EnumerateFiles(managed, "*", SearchOption.AllDirectories))
                File.Delete(file);
        }
        Directory.CreateDirectory(managed);

        var receiptFiles = new List<SteamManagedInstallFile>();
        foreach (var pair in files)
        {
            var path = Path.Combine(managed, pair.Key.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            await File.WriteAllBytesAsync(path, pair.Value);
            receiptFiles.Add(new SteamManagedInstallFile(pair.Key, pair.Value.LongLength, Sha1Hex(pair.Value)));
        }

        var receipt = new SteamManagedInstallReceipt(
            SteamManagedInstallReceipt.CurrentSchemaVersion,
            2868840u,
            2868842u,
            8653035385353091849UL,
            "public",
            DateTimeOffset.UnixEpoch,
            receiptFiles);
        await WriteReceiptAsync(managed, receipt);
    }

    private static async Task WriteReceiptAsync(string managed, SteamManagedInstallReceipt receipt)
    {
        var path = Path.Combine(managed, SteamManagedInstallReceipt.FileName);
        await using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
        await JsonSerializer.SerializeAsync(
            stream,
            receipt,
            SteamManagedInstallJsonContext.Default.SteamManagedInstallReceipt);
    }

    private static string Sha1Hex(byte[] bytes) =>
        Convert.ToHexString(SHA1.HashData(bytes));

    private static void DeleteRoot(string root)
    {
        if (Directory.Exists(root))
            Directory.Delete(root, recursive: true);
    }
}
