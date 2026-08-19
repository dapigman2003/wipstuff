using System.Reflection;
using System.Runtime.Loader;
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
        try
        {
            EnsureNoStS2AssemblyLoaded();
            var observations = new List<string>();
            foreach (var spec in HostFrameworkClosureRootSet.ExpectedHostClosure)
            {
                var requested = new AssemblyName
                {
                    Name = spec.Name,
                    Version = spec.MinimumVersion,
                    CultureName = string.Empty,
                };
                requested.SetPublicKeyToken(Convert.FromHexString(spec.PublicKeyToken));

                var assembly = AssemblyLoadContext.Default.LoadFromAssemblyName(requested);
                var actual = assembly.GetName();
                var actualName = actual.Name ?? string.Empty;
                var actualVersion = actual.Version ?? new Version(0, 0, 0, 0);
                var actualToken = TokenHex(actual.GetPublicKeyToken());
                if (!actualName.Equals(spec.Name, StringComparison.OrdinalIgnoreCase))
                    throw new FileLoadException($"Host returned '{actualName}' for requested framework root '{spec.Name}'.");
                if (actualVersion.CompareTo(spec.MinimumVersion) < 0)
                    throw new FileLoadException($"Host framework '{spec.Name}' is too old: required >= {spec.MinimumVersion}, actual {actualVersion}.");
                if (!actualToken.Equals(spec.PublicKeyToken, StringComparison.OrdinalIgnoreCase))
                    throw new FileLoadException($"Host framework '{spec.Name}' public-key-token mismatch: expected {spec.PublicKeyToken}, actual {actualToken}.");
                observations.Add($"{spec.Name} -> {actualVersion}");
            }
            EnsureNoStS2AssemblyLoaded();
            var sample = string.Join("\n", observations.Take(14).Select(item => "  " + item));
            return Pass(HostFrameworkClosureGate.RootedHostAvailability,
                $"Step 21.1 framework frontier host-loadable: {observations.Count:N0}/{HostFrameworkClosureRootSet.ExpectedHostClosure.Count:N0}\n" +
                $"Direct TrimmerRootAssembly seeds compiled into Step 22: {HostFrameworkClosureRootSet.DirectTrimmerRoots.Count:N0}\n" +
                "Transitive framework closure is supplied by the iOS/.NET host, not copied macOS System.* images.\n" +
                "Host binding sample:\n" + sample + "\n" +
                "StS2 assembly loaded/executed: NO\nReal managed install modified: NO");
        }
        catch (Exception ex)
        {
            return Fail(HostFrameworkClosureGate.RootedHostAvailability, stage, ex);
        }
    }

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
