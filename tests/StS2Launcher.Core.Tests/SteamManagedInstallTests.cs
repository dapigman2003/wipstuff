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
    public void SuccessfulResultContractIncludesAtomicReplacementProof()
    {
        var properties = typeof(SteamManagedInstallResult).GetProperties().Select(p => p.Name).ToHashSet();
        Assert.IsTrue(properties.Contains(nameof(SteamManagedInstallResult.ExistingInstallPreservedUntilCommit)));
        Assert.IsTrue(properties.Contains(nameof(SteamManagedInstallResult.AtomicCommitCompleted)));
        Assert.IsTrue(properties.Contains(nameof(SteamManagedInstallResult.StagingAbsentAfterResult)));
        Assert.IsTrue(properties.Contains(nameof(SteamManagedInstallResult.BackupAbsentAfterResult)));
    }
}
