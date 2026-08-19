using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StS2Launcher.Core;

namespace StS2Launcher.Core.Tests;

[TestClass]
public sealed class RuntimeBindingDiagnosticsExporterTests
{
    [TestMethod]
    public async Task ExporterGroupsAndListsEveryPersistedBlockerInShareSafeText()
    {
        using var temp = new TemporaryDirectory();
        var plan = CreatePlan(
            [
                new RuntimeBindingBlocker("HostFrameworkUnavailable", "Game.A, Version=1.0.0.0", "System.Widget, Version=8.0.0.0", "Host did not provide this contract."),
                new RuntimeBindingBlocker("HostFrameworkUnavailable", "Game.B, Version=1.0.0.0", "System.Widget, Version=8.0.0.0", "Host did not provide this contract from another source."),
                new RuntimeBindingBlocker("MissingWorkspaceAssembly", "sts2, Version=1.0.0.0", "Game.Missing, Version=2.0.0.0", "No verified private assembly exists."),
            ],
            runtimeClosureReady: false);
        var planPath = await WritePlanAsync(temp.Path, plan);

        var exporter = new RuntimeBindingDiagnosticsExporter(temp.Path);
        var result = await exporter.ExportAsync();

        Assert.AreEqual(3, result.BlockerCount);
        Assert.AreEqual(2, result.UniqueBlockedRequestedIdentityCount);
        Assert.IsFalse(result.RuntimeClosureReady);
        Assert.IsTrue(File.Exists(result.ReportPath));
        Assert.AreEqual(Path.Combine(temp.Path, RuntimeBindingDiagnosticsExporter.ReportFileName), result.ReportPath);
        Assert.AreEqual(Convert.ToHexString(SHA256.HashData(await File.ReadAllBytesAsync(planPath))).ToLowerInvariant(), result.PlanSha256);

        var report = await File.ReadAllTextAsync(result.ReportPath);
        StringAssert.Contains(report, "Step 21.1 Runtime Binding Diagnostics");
        StringAssert.Contains(report, "Explicit binding blockers: 3");
        StringAssert.Contains(report, "Unique requested identities with blockers: 2");
        StringAssert.Contains(report, "   2  HostFrameworkUnavailable");
        StringAssert.Contains(report, "   1  MissingWorkspaceAssembly");
        StringAssert.Contains(report, "#001");
        StringAssert.Contains(report, "#002");
        StringAssert.Contains(report, "#003");
        StringAssert.Contains(report, "System.Widget, Version=8.0.0.0");
        StringAssert.Contains(report, "Game.Missing, Version=2.0.0.0");
        StringAssert.Contains(report, "Runtime closure ready for first real CLR load: NO");
        Assert.IsFalse(report.Contains(temp.Path, StringComparison.Ordinal), "Shareable report must not leak absolute app/sandbox paths.");
        Assert.IsFalse(report.Contains("ActualLocation", StringComparison.Ordinal), "Shareable report should not dump host absolute locations.");
    }

    [TestMethod]
    public async Task ExporterCanReadExistingStep21PlanWithoutRerunningGates()
    {
        using var temp = new TemporaryDirectory();
        var plan = CreatePlan(
            [new RuntimeBindingBlocker("NonIlOnlyWorkspaceAssembly", "sts2, Version=1.0.0.0", "Desktop.Framework, Version=9.0.0.0", "Only a ReadyToRun/mixed-mode desktop image is available.")],
            runtimeClosureReady: false);
        await WritePlanAsync(temp.Path, plan);

        var exporter = new RuntimeBindingDiagnosticsExporter(temp.Path);
        var result = await exporter.ExportAsync();

        Assert.AreEqual(1, result.BlockerCount);
        var report = await File.ReadAllTextAsync(result.ReportPath);
        StringAssert.Contains(report, "NonIlOnlyWorkspaceAssembly");
        StringAssert.Contains(report, "Desktop.Framework, Version=9.0.0.0");
        StringAssert.Contains(report, "Persisted plan SHA-256:");
    }

    [TestMethod]
    public async Task ExporterRejectsMissingPersistedPlanInsteadOfCreatingMisleadingReport()
    {
        using var temp = new TemporaryDirectory();
        var exporter = new RuntimeBindingDiagnosticsExporter(temp.Path);

        var ex = await Assert.ThrowsExactlyAsync<FileNotFoundException>(() => exporter.ExportAsync());
        StringAssert.Contains(ex.Message, "Run Step 21 Gates A–D first");
        Assert.IsFalse(File.Exists(exporter.ReportPath));
    }

    [TestMethod]
    public async Task ExporterRejectsInconsistentRuntimeClosureFlag()
    {
        using var temp = new TemporaryDirectory();
        var invalid = CreatePlan(
            [new RuntimeBindingBlocker("MissingWorkspaceAssembly", "sts2, Version=1.0.0.0", "Missing, Version=1.0.0.0", "missing")],
            runtimeClosureReady: true);
        await WritePlanAsync(temp.Path, invalid);
        var exporter = new RuntimeBindingDiagnosticsExporter(temp.Path);

        var ex = await Assert.ThrowsExactlyAsync<InvalidDataException>(() => exporter.ExportAsync());
        StringAssert.Contains(ex.Message, "inconsistent RuntimeClosureReady/blocker state");
    }

    private static RuntimeFrameworkBindingPlanDocument CreatePlan(
        RuntimeBindingBlocker[] blockers,
        bool runtimeClosureReady)
        => new(
            RuntimeFrameworkBindingPlanDocument.CurrentSchemaVersion,
            2868840,
            2868842,
            123456789UL,
            "public",
            "ManagedInstall/Depot-2868842",
            "SlayTheSpire2.app/Contents/Resources/data_sts2_macos_arm64/sts2.dll",
            "sts2, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",
            [
                new RuntimeBindingPreparedAssembly(
                    "SlayTheSpire2.app/Contents/Resources/data_sts2_macos_arm64/sts2.dll",
                    "sts2, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",
                    "00112233445566778899aabbccddeeff00112233",
                    100,
                    true),
            ],
            [
                new RuntimeBindingHostFramework(
                    "System.Runtime, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
                    "System.Runtime, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
                    "/private/var/containers/Bundle/Application/SHOULD-NOT-BE-EXPORTED/System.Runtime.dll",
                    5),
            ],
            blockers,
            blockers.Select(blocker => new RuntimeBindingEdge(
                blocker.SourceAssemblyFullName,
                blocker.RequestedFullName,
                "Blocker:" + blocker.Kind,
                blocker.Detail)).ToArray(),
            runtimeClosureReady);

    private static async Task<string> WritePlanAsync(string launcherDataRoot, RuntimeFrameworkBindingPlanDocument plan)
    {
        var planRoot = Path.Combine(
            launcherDataRoot,
            PreparedRuntimeFrameworkBinding.WorkRootName,
            PreparedRuntimeFrameworkBinding.PlanRootName);
        Directory.CreateDirectory(planRoot);
        var planPath = Path.Combine(planRoot, PreparedRuntimeFrameworkBinding.PlanFileName);
        await using var stream = File.Create(planPath);
        await JsonSerializer.SerializeAsync(
            stream,
            plan,
            RuntimeFrameworkBindingJsonContext.Default.RuntimeFrameworkBindingPlanDocument);
        return planPath;
    }

    private sealed class TemporaryDirectory : IDisposable
    {
        public TemporaryDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "sts2-step21-1-export-tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            try { Directory.Delete(Path, recursive: true); } catch { }
        }
    }
}
