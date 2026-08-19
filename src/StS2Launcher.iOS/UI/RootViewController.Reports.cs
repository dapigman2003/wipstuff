using Foundation;
using System.Runtime.CompilerServices;
using UIKit;

namespace StS2Launcher.iOS;

public sealed partial class RootViewController
{
    private async Task WriteDeviceTestReportFromLabelsAsync(
        string fileName,
        string title,
        UILabel? resultLabel,
        UILabel? detailLabel,
        CancellationToken cancellationToken = default)
    {
        var snapshot = SnapshotReportLabels(resultLabel, detailLabel);
        await WriteDeviceTestReportAsync(
            fileName,
            title,
            snapshot.Result,
            snapshot.Detail,
            cancellationToken).ConfigureAwait(false);
    }

    private async Task WriteDeviceTestReportAsync(
        string fileName,
        string title,
        string result,
        string detail,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var version = NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleShortVersionString")?.ToString() ?? "unknown";
            var build = NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleVersion")?.ToString() ?? "unknown";
            await _deviceTestReportWriter.WriteLatestAsync(
                fileName,
                title,
                string.IsNullOrWhiteSpace(result) ? "NO RESULT TEXT" : result,
                detail ?? string.Empty,
                [
                    $"App version: {version} ({build})",
                    $"Runtime: {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}",
                    $"OS architecture: {System.Runtime.InteropServices.RuntimeInformation.OSArchitecture}",
                    $"Process architecture: {System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture}",
                    $"RuntimeFeature.IsDynamicCodeSupported: {RuntimeFeature.IsDynamicCodeSupported}",
                    $"RuntimeFeature.IsDynamicCodeCompiled: {RuntimeFeature.IsDynamicCodeCompiled}",
                    "Reports root: Documents/StS2Launcher/Reports",
                ],
                cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Device test report write failed for {fileName}: {ex.GetType().Name}: {ex.Message}");
            InvokeOnMainThread(() =>
            {
                if (_statusLabel is null)
                    return;
                var existing = _statusLabel.Text ?? string.Empty;
                if (!existing.Contains("REPORT WRITE FAILED", StringComparison.Ordinal))
                    _statusLabel.Text = existing + $"\nREPORT WRITE FAILED: {fileName} — {ex.GetType().Name}: {ex.Message}";
                _statusLabel.TextColor = UIColor.SystemOrange;
            });
        }
    }

    private (string Result, string Detail) SnapshotReportLabels(UILabel? resultLabel, UILabel? detailLabel)
    {
        var result = string.Empty;
        var detail = string.Empty;

        void Capture()
        {
            result = resultLabel?.Text ?? "(result label unavailable)";
            detail = detailLabel?.Text ?? "(detail label unavailable)";
        }

        if (NSThread.IsMain)
            Capture();
        else
            InvokeOnMainThread(Capture);

        return (result, detail);
    }
}
