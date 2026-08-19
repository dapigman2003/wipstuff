using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace StS2Launcher.Core.Tests;

[TestClass]
public sealed class DeviceTestReportWriterTests
{
    [TestMethod]
    public async Task WriteLatestAsyncWritesDeterministicUtf8ReportAndOverwritesLatest()
    {
        using var temp = new TempTestDirectory("sts2-device-report");
        var writer = new DeviceTestReportWriter(temp.Path);

        var first = await writer.WriteLatestAsync(
            "Foundation-5of5.txt",
            "StS2 Launcher — Foundation 5/5",
            "PASS",
            "first detail",
            ["Version: test", "Architecture: arm64"]);

        Assert.AreEqual(Path.Combine(temp.Path, DeviceTestReportWriter.ReportsDirectoryName, "Foundation-5of5.txt"), first);
        Assert.IsTrue(File.Exists(first));
        var firstBytes = await File.ReadAllBytesAsync(first);
        Assert.IsFalse(
            firstBytes.Length >= 3 && firstBytes[0] == 0xEF && firstBytes[1] == 0xBB && firstBytes[2] == 0xBF,
            "Report should be UTF-8 without BOM.");
        var firstText = Encoding.UTF8.GetString(firstBytes);
        StringAssert.Contains(firstText, "RESULT\nPASS");
        StringAssert.Contains(firstText, "Version: test");
        StringAssert.Contains(firstText, "first detail");

        var second = await writer.WriteLatestAsync(
            "Foundation-5of5.txt",
            "StS2 Launcher — Foundation 5/5",
            "FAIL",
            "second detail");

        Assert.AreEqual(first, second);
        var secondText = await File.ReadAllTextAsync(second);
        StringAssert.Contains(secondText, "RESULT\nFAIL");
        StringAssert.Contains(secondText, "second detail");
        Assert.IsFalse(secondText.Contains("first detail", StringComparison.Ordinal));
        Assert.AreEqual(1, Directory.EnumerateFiles(writer.ReportsRoot, "Foundation-5of5.txt").Count());
        Assert.AreEqual(0, Directory.EnumerateFiles(writer.ReportsRoot, "*.tmp-*").Count());
    }

    [TestMethod]
    [DataRow("../escape.txt")]
    [DataRow("subdir/report.txt")]
    [DataRow("report.log")]
    [DataRow("")]
    public async Task WriteLatestAsyncRejectsUnsafeFileNames(string fileName)
    {
        using var temp = new TempTestDirectory("sts2-device-report-invalid");
        var writer = new DeviceTestReportWriter(temp.Path);

        await Assert.ThrowsExactlyAsync<ArgumentException>(() =>
            writer.WriteLatestAsync(fileName, "title", "PASS", "detail"));
    }
}
