using System.Text;

namespace StS2Launcher.Core;

/// <summary>
/// Writes deterministic, shareable device-test reports under the launcher's Documents tree.
/// Reports are output-only diagnostics and are never consumed as trusted runtime input.
/// </summary>
public sealed class DeviceTestReportWriter
{
    public const string ReportsDirectoryName = "Reports";

    private readonly string _reportsRoot;

    public DeviceTestReportWriter(string launcherDataRoot)
    {
        if (string.IsNullOrWhiteSpace(launcherDataRoot))
            throw new ArgumentException("Launcher data root must be non-empty.", nameof(launcherDataRoot));

        _reportsRoot = Path.Combine(Path.GetFullPath(launcherDataRoot), ReportsDirectoryName);
    }

    public string ReportsRoot => _reportsRoot;

    public async Task<string> WriteLatestAsync(
        string fileName,
        string title,
        string result,
        string detail,
        IEnumerable<string>? metadata = null,
        CancellationToken cancellationToken = default)
    {
        var safeFileName = ValidateFileName(fileName);
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Report title must be non-empty.", nameof(title));
        if (string.IsNullOrWhiteSpace(result))
            throw new ArgumentException("Report result must be non-empty.", nameof(result));

        Directory.CreateDirectory(_reportsRoot);
        var destination = Path.Combine(_reportsRoot, safeFileName);
        var temporary = destination + ".tmp-" + Guid.NewGuid().ToString("N");

        var builder = new StringBuilder(capacity: Math.Max(512, detail?.Length ?? 0));
        builder.AppendLine(title.Trim());
        builder.AppendLine("Report format: 1");
        builder.AppendLine("Trust note: output only; this text file is never consumed as trusted runtime input.");
        builder.AppendLine("Secret note: this report intentionally excludes Steam passwords, refresh tokens, Steam Guard material, and Apple signing secrets.");
        builder.AppendLine($"Generated UTC: {DateTimeOffset.UtcNow:O}");
        builder.AppendLine();
        builder.AppendLine("RESULT");
        builder.AppendLine(result.Trim());

        if (metadata is not null)
        {
            var entries = metadata
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value.Trim())
                .ToArray();
            if (entries.Length > 0)
            {
                builder.AppendLine();
                builder.AppendLine("METADATA");
                foreach (var entry in entries)
                    builder.AppendLine(entry);
            }
        }

        builder.AppendLine();
        builder.AppendLine("DETAIL");
        builder.AppendLine(string.IsNullOrWhiteSpace(detail) ? "(none)" : detail.TrimEnd());
        builder.AppendLine();
        builder.AppendLine("END OF REPORT");

        try
        {
            await File.WriteAllTextAsync(temporary, builder.ToString(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), cancellationToken)
                .ConfigureAwait(false);
            File.Move(temporary, destination, overwrite: true);
            return destination;
        }
        finally
        {
            try
            {
                if (File.Exists(temporary))
                    File.Delete(temporary);
            }
            catch
            {
                // Never hide the original report-write result with temporary-file cleanup noise.
            }
        }
    }

    private static string ValidateFileName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Report file name must be non-empty.", nameof(value));

        var trimmed = value.Trim();
        if (!trimmed.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Device-test reports must use a .txt extension.", nameof(value));
        if (!string.Equals(trimmed, Path.GetFileName(trimmed), StringComparison.Ordinal))
            throw new ArgumentException("Report file name must not contain a directory path.", nameof(value));
        if (trimmed.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            throw new ArgumentException("Report file name contains invalid characters.", nameof(value));

        return trimmed;
    }
}
