using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Text;
using System.Text.Json;

namespace StS2Launcher.Core;

public sealed class HostFrameworkClosureFoundation
{
    private readonly string _launcherDataRoot;
    private readonly PreparedRuntimeFrameworkBinding _binding;
    private bool _gateBQualified;

    public HostFrameworkClosureFoundation(string launcherDataRoot)
    {
        _launcherDataRoot = Path.GetFullPath(launcherDataRoot);
        _binding = new PreparedRuntimeFrameworkBinding(_launcherDataRoot);
    }

    public void Reset()
    {
        _binding.Reset();
        _gateBQualified = false;
    }

    public HostFrameworkClosureGateResult RunRootedHostAvailabilityProbe()
    {
        const string stage = "rooted host framework availability probe";
        var reportPath = Path.Combine(_launcherDataRoot, "Step22.2-HostBindingFrontierDiagnostics.txt");
        try
        {
            EnsureNoStS2AssemblyLoaded();
            Directory.CreateDirectory(_launcherDataRoot);

            var initiallyLoaded = AppDomain.CurrentDomain.GetAssemblies()
                .Select(assembly => assembly.GetName())
                .Where(name => !string.IsNullOrWhiteSpace(name.Name))
                .GroupBy(name => name.Name!, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    group => group.Key,
                    group => group.OrderByDescending(name => name.Version ?? new Version(0, 0, 0, 0)).First(),
                    StringComparer.OrdinalIgnoreCase);

            var observations = new List<HostProbeObservation>();
            foreach (var spec in HostFrameworkClosureRootSet.ExpectedHostClosure)
                observations.Add(ProbeHostFrameworkExact(spec, initiallyLoaded));

            // Do not let diagnostic fallback loads influence any exact-identity result. Only after
            // all 44 exact probes are finished do we ask whether each failed simple name is
            // nevertheless loadable, which distinguishes absence from identity mismatch.
            for (var i = 0; i < observations.Count; i++)
            {
                if (!observations[i].Passed)
                    observations[i] = AddSimpleNameDiagnostic(observations[i]);
            }

            EnsureNoStS2AssemblyLoaded();
            WriteHostAvailabilityReport(reportPath, observations, initiallyLoaded.Values);

            var passed = observations.Count(item => item.Passed);
            var failed = observations.Count - passed;
            var direct = observations.Where(item => item.DirectRoot).ToArray();
            var directPassed = direct.Count(item => item.Passed);
            var directFailed = direct.Length - directPassed;
            var diagnosticOnlyFailed = observations.Count(item => !item.Passed && !item.DirectRoot);
            if (directFailed != 0)
            {
                var first = direct.First(item => !item.Passed);
                throw new FileLoadException(
                    $"Required host-binding frontier incomplete: {directPassed}/{direct.Length} direct roots qualified, {directFailed} failed. " +
                    $"First required-root failure: {first.Spec.Name}: {first.FailureType}: {first.FailureMessage}. " +
                    $"Complete report written to Files-visible Documents as '{Path.GetFileName(reportPath)}'.");
            }

            var sample = string.Join("\n", direct.Take(14).Select(item => $"  {item.Spec.Name} -> {item.ActualVersion}"));
            return Pass(HostFrameworkClosureGate.RootedHostAvailability,
                $"Required host-binding frontier roots: {directPassed:N0}/{HostFrameworkClosureRootSet.DirectTrimmerRoots.Count:N0} qualified\n" +
                $"Complete 44-name diagnostic frontier: {passed:N0}/{HostFrameworkClosureRootSet.ExpectedHostClosure.Count:N0} loadable; {diagnosticOnlyFailed:N0} transitive-only diagnostic misses\n" +
                $"Complete Gate A report: Files → On My iPhone → StS2 Launcher → StS2Launcher → {Path.GetFileName(reportPath)}\n" +
                "Gate A rule: only the 22 measured direct host-binding roots are required. Downstream implementation assemblies referenced only by host-bound framework assemblies are diagnostic, not independent game bindings.\n" +
                "Gate B recomputes the real sts2.dll dependency plan and is authoritative for any residual binding blockers.\n" +
                "Required host-binding sample:\n" + sample + "\n" +
                "StS2 assembly loaded/executed: NO\nReal managed install modified: NO");
        }
        catch (Exception ex)
        {
            return Fail(HostFrameworkClosureGate.RootedHostAvailability, stage, ex);
        }
    }

    private static HostProbeObservation ProbeHostFrameworkExact(
        HostFrameworkClosureSpec spec,
        IReadOnlyDictionary<string, AssemblyName> initiallyLoaded)
    {
        var requested = new AssemblyName
        {
            Name = spec.Name,
            Version = spec.MinimumVersion,
            CultureName = string.Empty,
        };
        requested.SetPublicKeyToken(Convert.FromHexString(spec.PublicKeyToken));
        var alreadyLoaded = initiallyLoaded.TryGetValue(spec.Name, out var initialIdentity);

        try
        {
            var assembly = AssemblyLoadContext.Default.LoadFromAssemblyName(requested);
            var actual = assembly.GetName();
            var actualName = actual.Name ?? string.Empty;
            var actualVersion = actual.Version ?? new Version(0, 0, 0, 0);
            var actualToken = TokenHex(actual.GetPublicKeyToken());
            string? qualificationFailure = null;
            if (!actualName.Equals(spec.Name, StringComparison.OrdinalIgnoreCase))
                qualificationFailure = $"Host returned '{actualName}' for requested framework root '{spec.Name}'.";
            else if (actualVersion.CompareTo(spec.MinimumVersion) < 0)
                qualificationFailure = $"Host framework is too old: required >= {spec.MinimumVersion}, actual {actualVersion}.";
            else if (!actualToken.Equals(spec.PublicKeyToken, StringComparison.OrdinalIgnoreCase))
                qualificationFailure = $"Public-key-token mismatch: expected {spec.PublicKeyToken}, actual {actualToken}.";

            return new HostProbeObservation(
                spec,
                qualificationFailure is null,
                HostFrameworkClosureRootSet.DirectTrimmerRoots.Contains(spec.Name, StringComparer.OrdinalIgnoreCase),
                alreadyLoaded,
                initialIdentity?.FullName,
                actual.FullName,
                actualVersion,
                actualToken,
                qualificationFailure is null ? null : nameof(FileLoadException),
                qualificationFailure,
                null,
                null);
        }
        catch (Exception ex)
        {
            return new HostProbeObservation(
                spec,
                false,
                HostFrameworkClosureRootSet.DirectTrimmerRoots.Contains(spec.Name, StringComparer.OrdinalIgnoreCase),
                alreadyLoaded,
                initialIdentity?.FullName,
                null,
                null,
                null,
                ex.GetType().Name,
                ex.Message,
                null,
                null);
        }
    }

    private static HostProbeObservation AddSimpleNameDiagnostic(HostProbeObservation observation)
    {
        try
        {
            var simple = AssemblyLoadContext.Default.LoadFromAssemblyName(new AssemblyName(observation.Spec.Name));
            return observation with { SimpleNameProbeIdentity = simple.GetName().FullName, SimpleNameProbeFailure = null };
        }
        catch (Exception ex)
        {
            return observation with { SimpleNameProbeIdentity = null, SimpleNameProbeFailure = $"{ex.GetType().Name}: {ex.Message}" };
        }
    }

    private static void WriteHostAvailabilityReport(
        string reportPath,
        IReadOnlyList<HostProbeObservation> observations,
        IEnumerable<AssemblyName> initiallyLoaded)
    {
        var sb = new StringBuilder();
        sb.AppendLine("StS2 Launcher — Step 22.2 Host Binding Frontier Diagnostics");
        sb.AppendLine("Report format: 1");
        sb.AppendLine("Purpose: physical-iPhone Gate A proof of the 22 required host-binding roots plus diagnostics for the wider 44-name desktop/workspace frontier.");
        sb.AppendLine("Trust note: output only; this text file is never consumed as trusted runtime input.");
        sb.AppendLine("Secret note: contains framework assembly metadata only; no Steam credentials/tokens or Apple signing secrets.");
        sb.AppendLine();
        sb.AppendLine("RUNTIME");
        sb.AppendLine($"Generated UTC: {DateTimeOffset.UtcNow:O}");
        sb.AppendLine($"Framework: {RuntimeInformation.FrameworkDescription}");
        sb.AppendLine($"OS architecture: {RuntimeInformation.OSArchitecture}");
        sb.AppendLine($"Process architecture: {RuntimeInformation.ProcessArchitecture}");
        sb.AppendLine($"RuntimeFeature.IsDynamicCodeSupported: {RuntimeFeature.IsDynamicCodeSupported}");
        sb.AppendLine($"RuntimeFeature.IsDynamicCodeCompiled: {RuntimeFeature.IsDynamicCodeCompiled}");
        sb.AppendLine();
        sb.AppendLine("SUMMARY");
        sb.AppendLine($"Expected framework identities: {observations.Count}");
        sb.AppendLine($"Direct TrimmerRootAssembly seeds: {HostFrameworkClosureRootSet.DirectTrimmerRoots.Count}");
        sb.AppendLine($"Qualified: {observations.Count(item => item.Passed)}");
        sb.AppendLine($"Failed: {observations.Count(item => !item.Passed)}");
        sb.AppendLine($"Direct roots failed: {observations.Count(item => !item.Passed && item.DirectRoot)}");
        sb.AppendLine($"Transitive-only expectations failed: {observations.Count(item => !item.Passed && !item.DirectRoot)}");
        sb.AppendLine();

        sb.AppendLine("FAILED PROBES (direct-root failures are blocking; transitive-only failures are diagnostic)");
        var failures = observations.Where(item => !item.Passed).ToArray();
        if (failures.Length == 0)
            sb.AppendLine("  none");
        else
        {
            foreach (var item in failures)
            {
                sb.AppendLine($"- {item.Spec.Name}");
                sb.AppendLine($"  requested: {item.Spec.Name}, Version>={item.Spec.MinimumVersion}, Culture=neutral, PublicKeyToken={item.Spec.PublicKeyToken}");
                sb.AppendLine($"  direct trimmer root: {(item.DirectRoot ? "YES" : "NO")}");
                sb.AppendLine($"  already loaded before Gate A: {(item.AlreadyLoaded ? "YES" : "NO")}");
                if (!string.IsNullOrWhiteSpace(item.InitialIdentity)) sb.AppendLine($"  initial identity: {item.InitialIdentity}");
                sb.AppendLine($"  exact-identity probe failure: {item.FailureType}: {item.FailureMessage}");
                if (!string.IsNullOrWhiteSpace(item.SimpleNameProbeIdentity))
                    sb.AppendLine($"  simple-name fallback probe: LOADABLE as {item.SimpleNameProbeIdentity}");
                else
                    sb.AppendLine($"  simple-name fallback probe: FAILED — {item.SimpleNameProbeFailure}");
            }
        }

        sb.AppendLine();
        sb.AppendLine("COMPLETE PROBE RESULTS");
        for (var i = 0; i < observations.Count; i++)
        {
            var item = observations[i];
            sb.AppendLine($"{i + 1:000}. {(item.Passed ? "PASS" : "FAIL")} {item.Spec.Name}");
            sb.AppendLine($"     requested minimum version: {item.Spec.MinimumVersion}");
            sb.AppendLine($"     requested token: {item.Spec.PublicKeyToken}");
            sb.AppendLine($"     direct trimmer root: {(item.DirectRoot ? "YES" : "NO")}");
            sb.AppendLine($"     loaded before probe: {(item.AlreadyLoaded ? "YES" : "NO")}");
            if (!string.IsNullOrWhiteSpace(item.ActualIdentity)) sb.AppendLine($"     actual: {item.ActualIdentity}");
            if (!item.Passed) sb.AppendLine($"     failure: {item.FailureType}: {item.FailureMessage}");
        }

        sb.AppendLine();
        sb.AppendLine("INITIAL DEFAULT-CONTEXT ASSEMBLIES (framework-shaped only)");
        foreach (var name in initiallyLoaded
                     .Where(name => !string.IsNullOrWhiteSpace(name.Name) && IsHostFrameworkShape(name.Name!))
                     .OrderBy(name => name.Name, StringComparer.OrdinalIgnoreCase))
            sb.AppendLine("  " + name.FullName);

        sb.AppendLine();
        sb.AppendLine("END OF STEP 22.2 HOST BINDING FRONTIER DIAGNOSTICS");
        File.WriteAllText(reportPath, sb.ToString(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    }

    private sealed record HostProbeObservation(
        HostFrameworkClosureSpec Spec,
        bool Passed,
        bool DirectRoot,
        bool AlreadyLoaded,
        string? InitialIdentity,
        string? ActualIdentity,
        Version? ActualVersion,
        string? ActualToken,
        string? FailureType,
        string? FailureMessage,
        string? SimpleNameProbeIdentity,
        string? SimpleNameProbeFailure);

    public async Task<HostFrameworkClosureGateResult> RunBindingClosureRecomputeAsync(
        IProgress<RuntimeFrameworkBindingProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var stage = "Step 21 runtime classification recompute";
        try
        {
            EnsureNoStS2AssemblyLoaded();
            _binding.Reset();
            var classification = await _binding.RunRuntimePayloadClassificationAsync(progress, cancellationToken).ConfigureAwait(false);
            if (!classification.Passed)
                throw new InvalidDataException("Nested Step 21 Gate A failed while Step 22 recomputed the real workspace: " + classification.Detail);

            stage = "Step 21 host/private binding-plan recompute";
            var planResult = _binding.RunHostFrameworkBindingPlan();
            if (!planResult.Passed)
                throw new InvalidDataException("Nested Step 21 Gate B failed while Step 22 recomputed binding closure: " + planResult.Detail);
            _gateBQualified = true;
            EnsureNoStS2AssemblyLoaded();
            return Pass(HostFrameworkClosureGate.BindingClosureRecompute,
                "Recomputed the physically proven Step 21 dependency plan under the rooted Step 22 iOS host. " +
                "Gate B is intentionally diagnostic: residual blockers do not fail here because Gate C persists the full plan before closure qualification, allowing Files export if anything remains.\n" +
                "StS2 assembly loaded/executed: NO\nReal managed install modified: NO\n\nNested Step 21 Gate B evidence:\n" + planResult.Detail);
        }
        catch (Exception ex)
        {
            _gateBQualified = false;
            return Fail(HostFrameworkClosureGate.BindingClosureRecompute, stage, ex);
        }
    }

    public async Task<HostFrameworkClosureGateResult> RunHostOnlyFrameworkPreparedSetAsync(
        IProgress<RuntimeFrameworkBindingProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var stage = "prepared-set precondition";
        try
        {
            if (!_gateBQualified)
                throw new InvalidOperationException("Step 22 Gate B must successfully recompute the binding plan before Gate C.");
            EnsureNoStS2AssemblyLoaded();
            stage = "nested Step 21 prepared-set construction";
            var result = await _binding.RunPreparedRuntimeAssemblySetAsync(progress, cancellationToken).ConfigureAwait(false);
            if (!result.Passed)
                throw new InvalidDataException("Nested Step 21 Gate C failed: " + result.Detail);

            stage = "persisted closure-plan qualification";
            var plan = await ReadPersistedPlanAsync(cancellationToken).ConfigureAwait(false);
            ValidateClosedPlan(plan);
            var privateFramework = plan.PreparedAssemblies
                .Where(item => IsHostFrameworkShape(GetSimpleName(item.AssemblyFullName)))
                .ToArray();
            if (privateFramework.Length != 0)
                throw new InvalidDataException("Step 22 prepared set still contains framework-shaped private assemblies: " + string.Join(" | ", privateFramework.Select(item => item.AssemblyFullName)));

            EnsureNoStS2AssemblyLoaded();
            return Pass(HostFrameworkClosureGate.HostOnlyFrameworkPreparedSet,
                $"Prepared private/game assemblies: {plan.PreparedAssemblies.Length:N0}\n" +
                $"Host framework bindings: {plan.HostFrameworkBindings.Length:N0}\n" +
                "Prepared System.*/netstandard framework assemblies: 0\n" +
                "Cecil assembly writes: 0\nPrepared files remain receipt-byte-identical: YES\n" +
                "Runtime closure ready for first real CLR load: YES\n" +
                "StS2 assembly loaded/executed: NO\nReal managed install modified: NO");
        }
        catch (Exception ex)
        {
            return Fail(HostFrameworkClosureGate.HostOnlyFrameworkPreparedSet, stage, ex);
        }
    }

    public async Task<HostFrameworkClosureGateResult> RunIsolationAuditAsync(
        IProgress<RuntimeFrameworkBindingProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var stage = "nested Step 21 closure audit";
        try
        {
            EnsureNoStS2AssemblyLoaded();
            var result = await _binding.RunClosureAuditAsync(progress, cancellationToken).ConfigureAwait(false);
            if (!result.Passed)
                throw new InvalidDataException("Nested Step 21 Gate D failed: " + result.Detail);

            stage = "final persisted-plan requalification";
            var plan = await ReadPersistedPlanAsync(cancellationToken).ConfigureAwait(false);
            ValidateClosedPlan(plan);
            if (plan.PreparedAssemblies.Any(item => IsHostFrameworkShape(GetSimpleName(item.AssemblyFullName))))
                throw new InvalidDataException("Final Step 22 audit detected a framework-shaped assembly in the private prepared set.");
            EnsureNoStS2AssemblyLoaded();
            return Pass(HostFrameworkClosureGate.IsolationAudit,
                "Nested Step 21 source/prepared/live/plan audit: PASS\n" +
                "Explicit binding blockers after independent audit: 0\n" +
                "Private prepared framework implementations: 0\n" +
                "Runtime closure ready for first real CLR load: YES\n" +
                "Trusted live install receipt/SHA-1 boundary preserved: YES\n" +
                "StS2 assembly loaded/executed: NO\n" +
                "Step 22 conclusion: the first real CLR-load boundary is dependency-closure eligible; actual StS2 load remains a later step.");
        }
        catch (Exception ex)
        {
            return Fail(HostFrameworkClosureGate.IsolationAudit, stage, ex);
        }
    }

    private async Task<RuntimeFrameworkBindingPlanDocument> ReadPersistedPlanAsync(CancellationToken cancellationToken)
    {
        var path = Path.Combine(_launcherDataRoot, PreparedRuntimeFrameworkBinding.WorkRootName, PreparedRuntimeFrameworkBinding.PlanRootName, PreparedRuntimeFrameworkBinding.PlanFileName);
        await using var stream = File.OpenRead(path);
        return await JsonSerializer.DeserializeAsync(stream, RuntimeFrameworkBindingJsonContext.Default.RuntimeFrameworkBindingPlanDocument, cancellationToken).ConfigureAwait(false)
               ?? throw new InvalidDataException("Persisted runtime binding plan deserialized to null.");
    }

    private static void ValidateClosedPlan(RuntimeFrameworkBindingPlanDocument plan)
    {
        if (!plan.RuntimeClosureReady || plan.Blockers.Length != 0)
            throw new InvalidDataException($"Runtime closure is not complete: ready={plan.RuntimeClosureReady}, blockers={plan.Blockers.Length}.");
        if (!plan.PreparedAssemblies.Any(item => item.IsPrimary && GetSimpleName(item.AssemblyFullName).Equals("sts2", StringComparison.OrdinalIgnoreCase)))
            throw new InvalidDataException("Closed plan does not contain the primary sts2 assembly in its prepared private set.");
    }

    private static bool IsHostFrameworkShape(string name)
        => name.Equals("System", StringComparison.OrdinalIgnoreCase) ||
           name.StartsWith("System.", StringComparison.OrdinalIgnoreCase) ||
           name.Equals("mscorlib", StringComparison.OrdinalIgnoreCase) ||
           name.Equals("netstandard", StringComparison.OrdinalIgnoreCase) ||
           name.Equals("Microsoft.CSharp", StringComparison.OrdinalIgnoreCase) ||
           name.Equals("Microsoft.VisualBasic", StringComparison.OrdinalIgnoreCase) ||
           name.Equals("Microsoft.VisualBasic.Core", StringComparison.OrdinalIgnoreCase) ||
           name.StartsWith("Microsoft.Win32.", StringComparison.OrdinalIgnoreCase);

    private static string GetSimpleName(string fullName)
    {
        var comma = fullName.IndexOf(',');
        return comma < 0 ? fullName.Trim() : fullName[..comma].Trim();
    }

    private static string TokenHex(byte[]? token)
        => token is null || token.Length == 0 ? string.Empty : Convert.ToHexString(token).ToLowerInvariant();

    private static void EnsureNoStS2AssemblyLoaded()
    {
        var matches = AppDomain.CurrentDomain.GetAssemblies()
            .Select(assembly => assembly.GetName().Name ?? string.Empty)
            .Where(name => name.Equals("sts2", StringComparison.OrdinalIgnoreCase) || name.Equals("SlayTheSpire2", StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (matches.Length != 0)
            throw new InvalidDataException("Step 22 detected a loaded real-game assembly even though StS2 CLR loading remains out of scope: " + string.Join(", ", matches));
    }

    private static HostFrameworkClosureGateResult Pass(HostFrameworkClosureGate gate, string detail) => new(gate, true, detail);
    private static HostFrameworkClosureGateResult Fail(HostFrameworkClosureGate gate, string stage, Exception ex)
        => new(gate, false, $"Stage: {stage}\n{ex.GetType().Name}: {ex.Message}\nStS2 assembly loaded/executed: NO intended\nReal managed install modified: NO intended");
}
