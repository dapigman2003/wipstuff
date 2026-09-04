using Foundation;
using StS2Launcher.Core;

namespace StS2Launcher.iOS;

public sealed partial class RootViewController
{
    private UIProgressView? _step35GateDProgressView;
    private UILabel? _step35GateDProgressLabel;
    private DateTimeOffset? _step35GateDProgressStartedAt;
    private DateTimeOffset? _step35GateDLastProgressAt;
    private string _step35GateDLastProgressText = "Gate D integrity audit progress will appear here.";
    private NSTimer? _step35GateDHeartbeatTimer;
    private bool _step35GateDFinalChecksReported;

    private void AddStep35GateDProgressControls(UIStackView content)
    {
        _step35GateDProgressLabel = Label(
            "Gate D integrity audit progress will appear here.",
            UIFont.SystemFontOfSize(12),
            UIColor.SecondaryLabel);
        _step35GateDProgressLabel.Hidden = true;
        content.AddArrangedSubview(_step35GateDProgressLabel);

        _step35GateDProgressView = new UIProgressView(UIProgressViewStyle.Default)
        {
            Progress = 0f,
            Hidden = true,
            AccessibilityLabel = "Step 35 Gate D integrity audit progress",
        };
        content.AddArrangedSubview(_step35GateDProgressView);
    }

    private void ResetStep35GateDProgress(bool visible)
    {
        if (_step35GateDProgressView is null || _step35GateDProgressLabel is null)
            return;

        StopStep35GateDHeartbeat();
        _step35GateDProgressStartedAt = visible ? DateTimeOffset.UtcNow : null;
        _step35GateDLastProgressAt = visible ? DateTimeOffset.UtcNow : null;
        _step35GateDFinalChecksReported = false;
        _step35GateDProgressView.Progress = 0f;
        _step35GateDProgressView.Hidden = !visible;
        _step35GateDProgressLabel.Hidden = !visible;
        _step35GateDLastProgressText = visible
            ? "Gate D integrity audit starting — receipt-backed files will be SHA-1 verified locally. Large files can hold the file/byte counters steady while their SHA-1 is being computed."
            : "Gate D integrity audit progress will appear here.";
        _step35GateDProgressLabel.Text = _step35GateDLastProgressText;
        if (visible)
            StartStep35GateDHeartbeat();
    }

    private void UpdateStep35GateDProgress(TransformedRealStS2VeryEarlyInitializationProgress value)
    {
        if (value.Gate != TransformedRealStS2VeryEarlyInitializationGate.FinalIsolationAudit ||
            _step35GateDProgressView is null ||
            _step35GateDProgressLabel is null)
            return;

        if (_step35GateDProgressView.Hidden)
            ResetStep35GateDProgress(visible: true);

        double overallFraction;
        string summary;
        var terminalPostHashProgress = value.TotalBytes == 0 && value.TotalItems > 0 && value.ProcessedItems >= value.TotalItems;
        if (value.TotalBytes > 0)
        {
            var hashFraction = Math.Clamp((double)value.ProcessedBytes / value.TotalBytes, 0d, 1d);
            overallFraction = hashFraction * 0.75d;
            var percent = hashFraction * 100d;
            summary = $"Gate D receipt hash — {percent:0.0}% • {value.ProcessedItems:N0}/{value.TotalItems:N0} files • {FormatStep35GateDBytes(value.ProcessedBytes)} / {FormatStep35GateDBytes(value.TotalBytes)}";
        }
        else if (value.TotalItems > 0 && value.ProcessedItems > 0)
        {
            var postHashFraction = Math.Clamp((double)value.ProcessedItems / value.TotalItems, 0d, 1d);
            overallFraction = 0.75d + (postHashFraction * 0.25d);
            summary = terminalPostHashProgress
                ? $"Gate D verification COMPLETE — {value.ProcessedItems:N0}/{value.TotalItems:N0}. Finalizing the Gate-D result…"
                : $"Gate D post-hash isolation checks — {value.ProcessedItems:N0}/{value.TotalItems:N0}";
        }
        else
        {
            overallFraction = 0d;
            summary = "Gate D integrity audit — preparing receipt-backed verification";
        }

        _step35GateDProgressView.SetProgress((float)Math.Clamp(overallFraction, 0d, 1d), animated: true);
        _step35GateDLastProgressAt = DateTimeOffset.UtcNow;
        if (terminalPostHashProgress)
            _step35GateDFinalChecksReported = true;

        var rateText = string.Empty;
        if (value.ProcessedBytes > 0 && _step35GateDProgressStartedAt is { } started)
        {
            var elapsed = DateTimeOffset.UtcNow - started;
            if (elapsed.TotalSeconds >= 1d)
            {
                var bytesPerSecond = value.ProcessedBytes / elapsed.TotalSeconds;
                if (bytesPerSecond > 0d)
                    rateText = $" • {FormatStep35GateDBytes((ulong)bytesPerSecond)}/s";
            }
        }

        var current = string.IsNullOrWhiteSpace(value.CurrentPath)
            ? string.Empty
            : $"\nLatest verifier file: {value.CurrentPath}";
        _step35GateDLastProgressText = summary + rateText + current;
        _step35GateDProgressLabel.Text = BuildStep35GateDHeartbeatText();
    }

    private void CompleteStep35GateDProgress(bool passed, string authority)
    {
        if (_step35GateDProgressView is null || _step35GateDProgressLabel is null)
            return;
        StopStep35GateDHeartbeat();
        _step35GateDProgressView.SetProgress(1f, animated: false);
        _step35GateDFinalChecksReported = true;
        var elapsed = _step35GateDProgressStartedAt is { } started
            ? DateTimeOffset.UtcNow - started
            : TimeSpan.Zero;
        _step35GateDLastProgressText = $"Gate D {(passed ? "COMPLETE — PASS" : "COMPLETE — FAIL")} • {authority}\nElapsed: {FormatStep35GateDTime(elapsed)}";
        _step35GateDProgressLabel.Text = _step35GateDLastProgressText;
    }

    private void StartStep35GateDHeartbeat()
    {
        _step35GateDHeartbeatTimer = NSTimer.CreateRepeatingScheduledTimer(1.0, _ =>
        {
            if (_step35GateDProgressLabel is not null && !_step35GateDProgressLabel.Hidden)
                _step35GateDProgressLabel.Text = BuildStep35GateDHeartbeatText();
        });
    }

    private void StopStep35GateDHeartbeat()
    {
        _step35GateDHeartbeatTimer?.Invalidate();
        _step35GateDHeartbeatTimer?.Dispose();
        _step35GateDHeartbeatTimer = null;
    }

    private string BuildStep35GateDHeartbeatText()
    {
        if (_step35GateDProgressStartedAt is not { } started)
            return _step35GateDLastProgressText;
        var now = DateTimeOffset.UtcNow;
        var elapsed = now - started;
        var sinceProgress = _step35GateDLastProgressAt is { } last ? now - last : elapsed;
        if (_step35GateDFinalChecksReported)
        {
            var warning = sinceProgress.TotalSeconds >= 10d
                ? " • finalization is taking longer than expected; durable D_* checkpoints will identify the boundary"
                : string.Empty;
            return _step35GateDLastProgressText +
                   $"\nElapsed: {FormatStep35GateDTime(elapsed)} • finalization wait: {FormatStep35GateDTime(sinceProgress)}{warning}";
        }
        return _step35GateDLastProgressText +
               $"\nElapsed: {FormatStep35GateDTime(elapsed)} • last verifier progress: {FormatStep35GateDTime(sinceProgress)} ago";
    }

    private static string FormatStep35GateDTime(TimeSpan value)
        => value.TotalHours >= 1d
            ? $"{(int)value.TotalHours}:{value.Minutes:00}:{value.Seconds:00}"
            : $"{value.Minutes:00}:{value.Seconds:00}";

    private static string FormatStep35GateDBytes(ulong bytes)
    {
        const double kib = 1024d;
        const double mib = 1024d * 1024d;
        const double gib = 1024d * 1024d * 1024d;
        return bytes >= (ulong)gib ? $"{bytes / gib:0.00} GiB" :
               bytes >= (ulong)mib ? $"{bytes / mib:0.0} MiB" :
               bytes >= (ulong)kib ? $"{bytes / kib:0.0} KiB" :
               $"{bytes:N0} B";
    }
}
