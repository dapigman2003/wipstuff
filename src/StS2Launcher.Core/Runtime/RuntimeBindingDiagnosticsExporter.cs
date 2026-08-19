using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace StS2Launcher.Core;

/// <summary>
/// Step 21.1 reporting-only helper. Reads the already persisted Step 21 binding plan as data and
/// writes a share-safe plain-text diagnostic report under Documents/StS2Launcher. It does not alter
/// the Step 21 binding algorithm, prepared set, trusted install, or plan file, and the launcher never
/// consumes the exported text report as trusted input.
/// </summary>
public sealed class RuntimeBindingDiagnosticsExporter
{
    public const string ReportFileName = "Step21.1-RuntimeBindingDiagnostics.txt";
    public const string ReportFormatVersion = "1";

    private readonly string _launcherDataRoot;

    public RuntimeBindingDiagnosticsExporter(string launcherDataRoot)
    {
        if (string.IsNullOrWhiteSpace(launcherDataRoot))
            throw new ArgumentException("Launcher data root is required.", nameof(launcherDataRoot));

        _launcherDataRoot = Path.GetFullPath(launcherDataRoot);
    }

    public string ReportPath => Path.Combine(_launcherDataRoot, ReportFileName);

    public async Task<RuntimeBindingDiagnosticsExportResult> ExportAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var planPath = Path.Combine(
            _launcherDataRoot,
            PreparedRuntimeFrameworkBinding.WorkRootName,
            PreparedRuntimeFrameworkBinding.PlanRootName,
            PreparedRuntimeFrameworkBinding.PlanFileName);
        if (!File.Exists(planPath))
        {
            throw new FileNotFoundException(
                "The persisted Step 21 runtime binding plan is not present. Run Step 21 Gates A–D first, then retry the diagnostic export.",
                planPath);
        }

        RuntimeFrameworkBindingPlanDocument plan;
        await using (var stream = File.OpenRead(planPath))
        {
            plan = await JsonSerializer.DeserializeAsync(
                stream,
                RuntimeFrameworkBindingJsonContext.Default.RuntimeFrameworkBindingPlanDocument,
                cancellationToken).ConfigureAwait(false)
                ?? throw new InvalidDataException("The persisted Step 21 runtime binding plan deserialized as null.");
        }

        ValidatePlan(plan);
        var planSha256 = await ComputeSha256HexAsync(planPath, cancellationToken).ConfigureAwait(false);
        var report = BuildReport(plan, planSha256);

        Directory.CreateDirectory(_launcherDataRoot);
        var reportPath = ReportPath;
        var tempPath = reportPath + ".tmp";
        try
        {
            await File.WriteAllTextAsync(tempPath, report, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), cancellationToken)
                .ConfigureAwait(false);
            File.Move(tempPath, reportPath, overwrite: true);
        }
        finally
        {
            try { if (File.Exists(tempPath)) File.Delete(tempPath); } catch { }
        }

        var reportSha256 = await ComputeSha256HexAsync(reportPath, cancellationToken).ConfigureAwait(false);
        var uniqueRequested = plan.Blockers
            .Select(blocker => blocker.RequestedFullName)
            .Distinct(StringComparer.Ordinal)
            .Count();

        return new RuntimeBindingDiagnosticsExportResult(
            reportPath,
            planSha256,
            reportSha256,
            plan.Blockers.Length,
            uniqueRequested,
            plan.RuntimeClosureReady);
    }

    internal static string BuildReport(RuntimeFrameworkBindingPlanDocument plan, string planSha256)
    {
        ValidatePlan(plan);
        if (string.IsNullOrWhiteSpace(planSha256))
            throw new ArgumentException("Plan SHA-256 is required.", nameof(planSha256));

        var sb = new StringBuilder(32 * 1024);
        sb.AppendLine("StS2 Launcher — Step 21.1 Runtime Binding Diagnostics");
        sb.AppendLine($"Report format: {ReportFormatVersion}");
        sb.AppendLine("Purpose: shareable diagnostic export of the persisted Step 21 binding plan");
        sb.AppendLine("Trust note: this text file is output only; the launcher never consumes it as trusted runtime input.");
        sb.AppendLine("Secret note: this report schema contains assembly/binding metadata only; it does not include Steam credentials, refresh tokens, Steam Guard material, or Apple signing secrets.");
        sb.AppendLine();

        sb.AppendLine("SUMMARY");
        sb.AppendLine($"App ID: {plan.AppId}");
        sb.AppendLine($"Depot ID: {plan.DepotId}");
        sb.AppendLine($"Manifest ID: {plan.ManifestId}");
        sb.AppendLine($"Branch: {plan.Branch}");
        sb.AppendLine($"Managed install relative path: {plan.ManagedInstallRelativePath}");
        sb.AppendLine($"Primary assembly relative path: {plan.PrimaryAssemblyRelativePath}");
        sb.AppendLine($"Primary assembly identity: {plan.PrimaryAssemblyFullName}");
        sb.AppendLine($"Persisted plan SHA-256: {planSha256.ToLowerInvariant()}");
        sb.AppendLine($"Prepared assemblies: {plan.PreparedAssemblies.Length}");
        sb.AppendLine($"Host framework bindings: {plan.HostFrameworkBindings.Length}");
        sb.AppendLine($"Dependency graph edges: {plan.Edges.Length}");
        sb.AppendLine($"Explicit binding blockers: {plan.Blockers.Length}");
        sb.AppendLine($"Unique requested identities with blockers: {plan.Blockers.Select(x => x.RequestedFullName).Distinct(StringComparer.Ordinal).Count()}");
        sb.AppendLine($"Runtime closure ready for first real CLR load: {(plan.RuntimeClosureReady ? "YES" : "NO")}");
        sb.AppendLine();

        sb.AppendLine("BLOCKERS BY KIND");
        var byKind = plan.Blockers
            .GroupBy(blocker => blocker.Kind, StringComparer.Ordinal)
            .Select(group => new { Kind = group.Key, Count = group.Count() })
            .OrderByDescending(item => item.Count)
            .ThenBy(item => item.Kind, StringComparer.Ordinal)
            .ToArray();
        if (byKind.Length == 0)
        {
            sb.AppendLine("(none)");
        }
        else
        {
            foreach (var item in byKind)
                sb.AppendLine($"{item.Count,4}  {item.Kind}");
        }
        sb.AppendLine();

        sb.AppendLine("UNIQUE BLOCKED REQUESTS");
        var requestedGroups = plan.Blockers
            .GroupBy(blocker => blocker.RequestedFullName, StringComparer.Ordinal)
            .Select(group => new
            {
                Requested = group.Key,
                Count = group.Count(),
                Kinds = group.Select(x => x.Kind).Distinct(StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal).ToArray(),
                Sources = group.Select(x => x.SourceAssemblyFullName).Distinct(StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal).ToArray(),
            })
            .OrderByDescending(item => item.Count)
            .ThenBy(item => item.Requested, StringComparer.Ordinal)
            .ToArray();
        if (requestedGroups.Length == 0)
        {
            sb.AppendLine("(none)");
        }
        else
        {
            for (var index = 0; index < requestedGroups.Length; index++)
            {
                var item = requestedGroups[index];
                sb.AppendLine($"{index + 1:D3}. {item.Requested}");
                sb.AppendLine($"     blocker occurrences: {item.Count}");
                sb.AppendLine($"     kinds: {string.Join(", ", item.Kinds)}");
                sb.AppendLine($"     unique source assemblies: {item.Sources.Length}");
                foreach (var source in item.Sources)
                    sb.AppendLine($"       <- {source}");
            }
        }
        sb.AppendLine();

        sb.AppendLine($"BLOCKERS — COMPLETE ({plan.Blockers.Length})");
        if (plan.Blockers.Length == 0)
        {
            sb.AppendLine("(none)");
        }
        else
        {
            for (var index = 0; index < plan.Blockers.Length; index++)
            {
                var blocker = plan.Blockers[index];
                sb.AppendLine($"#{index + 1:D3}");
                sb.AppendLine($"Kind: {blocker.Kind}");
                sb.AppendLine($"Source: {blocker.SourceAssemblyFullName}");
                sb.AppendLine($"Requested: {blocker.RequestedFullName}");
                sb.AppendLine($"Detail: {blocker.Detail}");
                sb.AppendLine();
            }
        }

        sb.AppendLine("EDGE COUNTS BY BINDING KIND");
        var edgeCounts = plan.Edges
            .GroupBy(edge => edge.BindingKind, StringComparer.Ordinal)
            .Select(group => new { Kind = group.Key, Count = group.Count() })
            .OrderByDescending(item => item.Count)
            .ThenBy(item => item.Kind, StringComparer.Ordinal)
            .ToArray();
        if (edgeCounts.Length == 0)
        {
            sb.AppendLine("(none)");
        }
        else
        {
            foreach (var item in edgeCounts)
                sb.AppendLine($"{item.Count,4}  {item.Kind}");
        }
        sb.AppendLine();

        sb.AppendLine($"HOST FRAMEWORK BINDINGS ({plan.HostFrameworkBindings.Length})");
        if (plan.HostFrameworkBindings.Length == 0)
        {
            sb.AppendLine("(none)");
        }
        else
        {
            foreach (var binding in plan.HostFrameworkBindings
                         .OrderBy(item => item.RequestedFullName, StringComparer.Ordinal)
                         .ThenBy(item => item.ActualFullName, StringComparer.Ordinal))
            {
                sb.AppendLine($"Requested: {binding.RequestedFullName}");
                sb.AppendLine($"Host:      {binding.ActualFullName}");
                sb.AppendLine($"References observed: {binding.ReferenceCount}");
                sb.AppendLine();
            }
        }

        sb.AppendLine($"PREPARED IL-ONLY ASSEMBLIES ({plan.PreparedAssemblies.Length})");
        if (plan.PreparedAssemblies.Length == 0)
        {
            sb.AppendLine("(none)");
        }
        else
        {
            foreach (var assembly in plan.PreparedAssemblies
                         .OrderBy(item => item.RelativePath, StringComparer.OrdinalIgnoreCase))
            {
                sb.AppendLine($"{(assembly.IsPrimary ? "PRIMARY" : "PRIVATE")}  {assembly.AssemblyFullName}");
                sb.AppendLine($"  relative path: {assembly.RelativePath}");
                sb.AppendLine($"  receipt SHA-1: {assembly.Sha1Hex}");
                sb.AppendLine($"  length: {assembly.Length}");
            }
        }
        sb.AppendLine();

        sb.AppendLine("END OF STEP 21.1 RUNTIME BINDING DIAGNOSTICS");
        return sb.ToString();
    }

    private static void ValidatePlan(RuntimeFrameworkBindingPlanDocument plan)
    {
        if (plan.SchemaVersion != RuntimeFrameworkBindingPlanDocument.CurrentSchemaVersion)
            throw new InvalidDataException($"Unsupported Step 21 binding-plan schema {plan.SchemaVersion}; expected {RuntimeFrameworkBindingPlanDocument.CurrentSchemaVersion}.");
        if (plan.AppId == 0 || plan.DepotId == 0 || plan.ManifestId == 0)
            throw new InvalidDataException("Persisted Step 21 binding plan has an invalid Steam identity.");
        if (string.IsNullOrWhiteSpace(plan.PrimaryAssemblyFullName) || string.IsNullOrWhiteSpace(plan.PrimaryAssemblyRelativePath))
            throw new InvalidDataException("Persisted Step 21 binding plan is missing the primary assembly identity/path.");
        if (plan.PreparedAssemblies is null || plan.HostFrameworkBindings is null || plan.Blockers is null || plan.Edges is null)
            throw new InvalidDataException("Persisted Step 21 binding plan contains null collections.");
        if (plan.RuntimeClosureReady != (plan.Blockers.Length == 0))
            throw new InvalidDataException("Persisted Step 21 binding plan has inconsistent RuntimeClosureReady/blocker state.");
    }

    private static async Task<string> ComputeSha256HexAsync(string path, CancellationToken cancellationToken)
    {
        await using var stream = File.OpenRead(path);
        using var sha256 = SHA256.Create();
        var hash = await sha256.ComputeHashAsync(stream, cancellationToken).ConfigureAwait(false);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}

public sealed record RuntimeBindingDiagnosticsExportResult(
    string ReportPath,
    string PlanSha256,
    string ReportSha256,
    int BlockerCount,
    int UniqueBlockedRequestedIdentityCount,
    bool RuntimeClosureReady);
