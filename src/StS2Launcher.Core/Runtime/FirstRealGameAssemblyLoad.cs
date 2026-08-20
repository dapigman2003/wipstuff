using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Security.Cryptography;
using System.Text.Json;
using Mono.Cecil;

namespace StS2Launcher.Core;

/// <summary>
/// Step 23 boundary. Performs the first real CLR load of the receipt-backed prepared StS2 managed
/// payload after Step 22 has proven zero binding blockers. The boundary is intentionally load-only:
/// no game entry point, game type/member reflection, method invocation, Godot initialization, or
/// native game library resolution is permitted. Gate A requires the primary game assembly itself to
/// be module-initializer-free. Initializer-bearing private dependencies are classified and deferred,
/// because loading those dependencies would cross an automatic-execution boundary. Step 23 loads only
/// the maximal initializer-free private closure; the deferred initializer boundary belongs to Step 24.
/// </summary>
public sealed class FirstRealGameAssemblyLoad : IDisposable
{
    public const string ExpectedPrimarySimpleName = "sts2";
    public const string LoadContextName = "StS2Launcher-Step23-Game";

    private readonly string _launcherDataRoot;
    private readonly string _step21WorkRoot;
    private readonly string _preparedRoot;
    private readonly string _planPath;
    private readonly SteamOfflineInstallInspection _offlineInspection;
    private readonly bool _collectibleLoadContext;
    private readonly string _expectedPrimarySimpleName;
    private readonly HashSet<string> _freshProcessAssemblyNames;

    private PreflightSnapshot? _preflight;
    private PrimaryLoadSnapshot? _primaryLoad;
    private DependencyResolutionSnapshot? _dependencyResolution;
    private Step23GameLoadContext? _loadContext;
    private bool _disposed;

    public FirstRealGameAssemblyLoad(string launcherDataRoot, bool collectibleLoadContext = false)
        : this(
            launcherDataRoot,
            collectibleLoadContext,
            ExpectedPrimarySimpleName,
            [ExpectedPrimarySimpleName, "SlayTheSpire2"])
    {
    }

    internal FirstRealGameAssemblyLoad(
        string launcherDataRoot,
        bool collectibleLoadContext,
        string expectedPrimarySimpleName,
        IReadOnlyCollection<string> freshProcessAssemblyNames)
    {
        if (string.IsNullOrWhiteSpace(launcherDataRoot))
            throw new ArgumentException("Launcher data root is required.", nameof(launcherDataRoot));
        if (string.IsNullOrWhiteSpace(expectedPrimarySimpleName))
            throw new ArgumentException("Expected primary simple name is required.", nameof(expectedPrimarySimpleName));
        if (freshProcessAssemblyNames is null || freshProcessAssemblyNames.Count == 0)
            throw new ArgumentException("At least one fresh-process assembly name is required.", nameof(freshProcessAssemblyNames));

        _launcherDataRoot = Path.GetFullPath(launcherDataRoot);
        _step21WorkRoot = Path.Combine(_launcherDataRoot, PreparedRuntimeFrameworkBinding.WorkRootName);
        _preparedRoot = Path.Combine(_step21WorkRoot, PreparedRuntimeFrameworkBinding.PreparedRootName);
        _planPath = Path.Combine(
            _step21WorkRoot,
            PreparedRuntimeFrameworkBinding.PlanRootName,
            PreparedRuntimeFrameworkBinding.PlanFileName);
        _offlineInspection = new SteamOfflineInstallInspection(_launcherDataRoot);
        _collectibleLoadContext = collectibleLoadContext;
        _expectedPrimarySimpleName = expectedPrimarySimpleName.Trim();
        _freshProcessAssemblyNames = freshProcessAssemblyNames
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        _freshProcessAssemblyNames.Add(_expectedPrimarySimpleName);
    }

    public void Reset()
    {
        ThrowIfDisposed();
        ReleaseLoadContext();
        _preflight = null;
        _primaryLoad = null;
        _dependencyResolution = null;
    }

    public async Task<FirstRealGameAssemblyLoadGateResult> RunPreparedLoadPreflightAsync(
        IProgress<FirstRealGameAssemblyLoadProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            EnsureNoRealGameAssemblyLoaded();
            cancellationToken.ThrowIfCancellationRequested();

            stage = "OfflineReady precondition";
            progress?.Report(new FirstRealGameAssemblyLoadProgress(
                FirstRealGameAssemblyLoadGate.PreparedLoadPreflight,
                0,
                0,
                null,
                "Re-proving the trusted Step 12 install and validating the persisted zero-blocker Step 21/22 prepared runtime before the first real CLR load…"));

            IProgress<SteamOfflineInstallProgress>? offlineProgress = progress is null
                ? null
                : new CallbackProgress<SteamOfflineInstallProgress>(value =>
                    progress.Report(new FirstRealGameAssemblyLoadProgress(
                        FirstRealGameAssemblyLoadGate.PreparedLoadPreflight,
                        value.CompletedFiles,
                        value.TotalFiles,
                        value.CurrentFile,
                        $"OfflineReady precondition — {value.Message}")));

            var offline = await _offlineInspection.RunAsync(offlineProgress, cancellationToken).ConfigureAwait(false);
            if (!offline.Success || !offline.ExactManagedTreeVerified || string.IsNullOrWhiteSpace(offline.ManagedInstallRelativePath))
                throw new InvalidDataException(offline.Error ?? "Step 23 requires an exact OfflineReady managed install.");

            stage = "persisted runtime-binding plan";
            if (!File.Exists(_planPath))
                throw new FileNotFoundException("Step 23 requires the persisted Step 21/22 runtime binding plan. Rerun Step 22 A–D in this installation first.", _planPath);
            if (!Directory.Exists(_preparedRoot))
                throw new DirectoryNotFoundException("Step 23 requires the persisted Step 21/22 prepared runtime directory. Rerun Step 22 A–D first.");

            var planSha256 = await ComputeSha256HexAsync(_planPath, cancellationToken).ConfigureAwait(false);
            await using var planStream = File.OpenRead(_planPath);
            var plan = await JsonSerializer.DeserializeAsync(
                planStream,
                RuntimeFrameworkBindingJsonContext.Default.RuntimeFrameworkBindingPlanDocument,
                cancellationToken).ConfigureAwait(false)
                ?? throw new InvalidDataException("Step 23 could not deserialize the persisted runtime-binding plan.");

            ValidatePlanForFirstLoad(plan, offline);

            var expectedPreparedPaths = plan.PreparedAssemblies
                .Select(item => NormalizeRelative(item.RelativePath))
                .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
                .ToArray();
            var actualPreparedPaths = Directory.EnumerateFiles(_preparedRoot, "*", SearchOption.AllDirectories)
                .Select(path => NormalizeRelative(Path.GetRelativePath(_preparedRoot, path)))
                .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
                .ToArray();
            if (!expectedPreparedPaths.SequenceEqual(actualPreparedPaths, StringComparer.OrdinalIgnoreCase))
                throw new InvalidDataException("Step 23 prepared file set differs from the persisted zero-blocker binding plan.");

            var managedRoot = ResolveChildPath(_launcherDataRoot, plan.ManagedInstallRelativePath, "Step 23 managed-install path");
            var prepared = new List<PreparedAssemblySnapshot>(plan.PreparedAssemblies.Length);
            var moduleInitializerCount = 0;
            var pinvokeMethodCount = 0;
            var moduleReferenceCount = 0;

            for (var index = 0; index < plan.PreparedAssemblies.Length; index++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var item = plan.PreparedAssemblies[index];
                var relative = NormalizeRelative(item.RelativePath);
                stage = $"prepared assembly preflight: {relative}";
                progress?.Report(new FirstRealGameAssemblyLoadProgress(
                    FirstRealGameAssemblyLoadGate.PreparedLoadPreflight,
                    index,
                    plan.PreparedAssemblies.Length,
                    relative,
                    "Re-hashing prepared/live bytes and inspecting IL-only identity, module initializer, P/Invoke and ModuleRef metadata with Cecil…"));

                if (!SteamSingleFileTargetSelector.IsSafeRelativePath(relative))
                    throw new InvalidDataException($"Unsafe prepared assembly relative path in Step 23 plan: {relative}");

                var preparedPath = ResolveChildPath(_preparedRoot, relative, "Step 23 prepared path");
                var livePath = ResolveChildPath(managedRoot, relative, "Step 23 live managed path");
                VerifyFileLength(preparedPath, item.Length, "prepared");
                VerifyFileLength(livePath, item.Length, "live");

                var preparedSha1 = await ComputeSha1HexAsync(preparedPath, cancellationToken).ConfigureAwait(false);
                var liveSha1 = await ComputeSha1HexAsync(livePath, cancellationToken).ConfigureAwait(false);
                if (!preparedSha1.Equals(item.Sha1Hex, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException($"Step 23 prepared SHA-1 mismatch: {relative}");
                if (!liveSha1.Equals(item.Sha1Hex, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException($"Step 23 trusted live-install SHA-1 mismatch: {relative}");

                var metadata = ReadPreparedMetadata(preparedPath);
                if (!metadata.IsIlOnly)
                    throw new InvalidDataException($"Step 23 prepared assembly is not IL-only: {relative}");
                if (!metadata.FullName.Equals(item.AssemblyFullName, StringComparison.Ordinal))
                    throw new InvalidDataException($"Step 23 prepared assembly identity drift: {relative}\nPlan: {item.AssemblyFullName}\nFile: {metadata.FullName}");

                moduleInitializerCount += metadata.ModuleInitializerCount;
                pinvokeMethodCount += metadata.PInvokeMethodCount;
                moduleReferenceCount += metadata.ModuleReferenceCount;
                prepared.Add(new PreparedAssemblySnapshot(
                    item,
                    preparedPath,
                    livePath,
                    new AssemblyName(item.AssemblyFullName),
                    metadata.AssemblyReferences,
                    metadata.ModuleInitializerCount,
                    metadata.ModuleInitializerAudits,
                    metadata.PInvokeMethodCount,
                    metadata.ModuleReferenceCount));
            }

            stage = "binding-plan metadata coverage";
            foreach (var item in prepared)
            {
                var metadataReferences = item.AssemblyReferences
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(value => value, StringComparer.Ordinal)
                    .ToArray();
                var plannedReferences = plan.Edges
                    .Where(edge => edge.SourceAssemblyFullName.Equals(item.Plan.AssemblyFullName, StringComparison.Ordinal))
                    .Select(edge => edge.RequestedFullName)
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(value => value, StringComparer.Ordinal)
                    .ToArray();
                if (!metadataReferences.SequenceEqual(plannedReferences, StringComparer.Ordinal))
                {
                    throw new InvalidDataException(
                        $"Step 23 persisted binding plan does not exactly cover the Cecil AssemblyRef metadata for '{item.Plan.AssemblyFullName}'. " +
                        $"Metadata [{string.Join(" | ", metadataReferences)}], plan [{string.Join(" | ", plannedReferences)}].");
                }
            }

            var primary = prepared.Single(item => item.Plan.IsPrimary);
            if (primary.ModuleInitializerCount != 0)
            {
                throw new InvalidDataException(
                    "Step 23 refuses the first CLR load because the primary game assembly itself contains a <Module>..cctor module initializer. " +
                    "Loading the primary would therefore cross an automatic-execution boundary before Step 24. Primary: " +
                    $"{primary.Plan.AssemblyFullName} ({primary.ModuleInitializerCount})");
            }

            var deferredInitializerAssemblies = prepared
                .Where(item => !item.Plan.IsPrimary && item.ModuleInitializerCount > 0)
                .OrderBy(item => item.Plan.AssemblyFullName, StringComparer.Ordinal)
                .ToArray();

            var privateNames = prepared
                .Select(item => item.AssemblyName.Name ?? string.Empty)
                .Where(name => name.Length > 0)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            var alreadyLoadedPrivate = AppDomain.CurrentDomain.GetAssemblies()
                .Where(assembly => privateNames.Contains(assembly.GetName().Name ?? string.Empty))
                .Select(assembly => assembly.GetName().FullName ?? assembly.GetName().Name ?? "<unknown>")
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            if (alreadyLoadedPrivate.Length != 0)
                throw new InvalidDataException("Step 23 requires a fresh process with no prepared private/game assembly already loaded: " + string.Join(" | ", alreadyLoadedPrivate));

            if (OperatingSystem.IsIOS() && RuntimeFeature.IsDynamicCodeCompiled)
                throw new InvalidDataException("Step 23 canonical iOS runtime unexpectedly reports RuntimeFeature.IsDynamicCodeCompiled=true; the proven AOT/interpreter contract has changed.");

            _preflight = new PreflightSnapshot(
                plan,
                planSha256,
                managedRoot,
                prepared.ToArray(),
                primary,
                deferredInitializerAssemblies,
                offline,
                moduleInitializerCount,
                pinvokeMethodCount,
                moduleReferenceCount,
                RuntimeFeature.IsDynamicCodeSupported,
                RuntimeFeature.IsDynamicCodeCompiled);

            progress?.Report(new FirstRealGameAssemblyLoadProgress(
                FirstRealGameAssemblyLoadGate.PreparedLoadPreflight,
                plan.PreparedAssemblies.Length,
                plan.PreparedAssemblies.Length,
                primary.Plan.RelativePath,
                "Zero-blocker prepared runtime is receipt-identical and IL-only. The primary is initializer-free; initializer-bearing dependencies are deferred from Step 23."));

            return Pass(
                FirstRealGameAssemblyLoadGate.PreparedLoadPreflight,
                "Step 23 first-load prerequisites are physically requalified before any real game assembly enters the CLR.\n" +
                $"OfflineReady exact-tree verification: YES ({offline.VerifiedFiles:N0}/{offline.PlannedFiles:N0} files)\n" +
                $"Runtime binding plan SHA-256: {planSha256}\n" +
                $"Runtime closure ready: {(plan.RuntimeClosureReady ? "YES" : "NO")}\n" +
                $"Explicit binding blockers: {plan.Blockers.Length:N0}\n" +
                $"Prepared private/game assemblies: {prepared.Count:N0}\n" +
                $"Prepared primary: {primary.Plan.AssemblyFullName}\n" +
                $"Module initializers found across prepared set: {moduleInitializerCount:N0}\n" +
                $"Primary module initializers: {primary.ModuleInitializerCount:N0}\n" +
                $"Deferred initializer-bearing private assemblies: {deferredInitializerAssemblies.Length:N0}\n" +
                (deferredInitializerAssemblies.Length == 0
                    ? "Deferred initializer audit: none\n"
                    : "Deferred initializer audit:\n" + string.Join("\n", deferredInitializerAssemblies.Select(item =>
                        $"  - {item.Plan.AssemblyFullName}: {string.Join(" || ", item.ModuleInitializerAudits)}")) + "\n") +
                $"P/Invoke methods present (diagnostic only; not invoked): {pinvokeMethodCount:N0}\n" +
                $"ModuleRef entries present (diagnostic only; not resolved): {moduleReferenceCount:N0}\n" +
                $"RuntimeFeature.IsDynamicCodeSupported: {RuntimeFeature.IsDynamicCodeSupported}\n" +
                $"RuntimeFeature.IsDynamicCodeCompiled: {RuntimeFeature.IsDynamicCodeCompiled}\n" +
                "Prepared/live bytes receipt-identical: YES\n" +
                "Persisted plan exactly covers prepared AssemblyRef metadata: YES\n" +
                "Prepared private/game assemblies already loaded before Gate B: 0\n" +
                "Initializer-bearing private dependencies loaded by Gate A: 0\n" +
                "Real StS2 CLR load performed by Gate A: NO\n" +
                "Real managed install modified: NO");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _preflight = null;
            return Fail(FirstRealGameAssemblyLoadGate.PreparedLoadPreflight, stage, ex);
        }
    }

    [UnconditionalSuppressMessage(
        "Trimming",
        "IL2026",
        Justification = "Step 23 intentionally loads the exact receipt-verified prepared StS2 IL image after Step 22 has proven its runtime dependency closure. No game member is reflected or invoked in this gate.")]
    public FirstRealGameAssemblyLoadGateResult RunPrimaryAssemblyLoad()
    {
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var preflight = RequirePreflight();
            EnsureNoRealGameAssemblyLoaded();
            if (_loadContext is not null)
                throw new InvalidOperationException("Step 23 Gate B requires a fresh dedicated load context.");

            stage = "immediate primary SHA-1 recheck";
            VerifyFileLength(preflight.Primary.PreparedPath, preflight.Primary.Plan.Length, "primary prepared");
            var immediateSha1 = ComputeSha1Hex(preflight.Primary.PreparedPath);
            if (!immediateSha1.Equals(preflight.Primary.Plan.Sha1Hex, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Step 23 primary prepared sts2.dll changed between Gate A and Gate B.");

            stage = "dedicated private AssemblyLoadContext construction";
            var context = new Step23GameLoadContext(
                LoadContextName,
                preflight.Plan,
                preflight.PreparedAssemblies,
                _collectibleLoadContext);
            // Store the context before crossing the load boundary so the failure path can release
            // collectible host-test contexts even when LoadFromStream itself throws.
            _loadContext = context;

            stage = "first real sts2.dll CLR load";
            var assembly = context.LoadPrimary(preflight.Primary);
            if (!ReferenceEquals(AssemblyLoadContext.GetLoadContext(assembly), context))
                throw new InvalidDataException("The real primary sts2.dll did not load into the dedicated Step 23 AssemblyLoadContext.");

            var actualIdentity = assembly.GetName().FullName ?? assembly.GetName().Name ?? string.Empty;
            if (!actualIdentity.Equals(preflight.Plan.PrimaryAssemblyFullName, StringComparison.Ordinal))
                throw new InvalidDataException($"Loaded primary identity mismatch. Expected '{preflight.Plan.PrimaryAssemblyFullName}', actual '{actualIdentity}'.");

            var loadedGameMatches = AppDomain.CurrentDomain.GetAssemblies()
                .Where(IsRealGameAssembly)
                .ToArray();
            if (loadedGameMatches.Length != 1 || !ReferenceEquals(loadedGameMatches[0], assembly))
                throw new InvalidDataException($"Expected exactly one real game assembly after Gate B, found {loadedGameMatches.Length}.");

            _primaryLoad = new PrimaryLoadSnapshot(
                actualIdentity,
                context.Name ?? LoadContextName,
                immediateSha1,
                context.ManagedResolverRequests.Count,
                context.PrivateLoads.Count,
                context.HostLoads.Count,
                context.NativeLoadAttempts.Count);

            return Pass(
                FirstRealGameAssemblyLoadGate.PrimaryAssemblyLoad,
                "FIRST REAL STS2 CLR LOAD SUCCEEDED. The boundary stops immediately after identity/context verification.\n" +
                $"Loaded identity: {actualIdentity}\n" +
                $"AssemblyLoadContext: {_primaryLoad.LoadContextName}\n" +
                $"Primary SHA-1 reverified immediately before load: {immediateSha1}\n" +
                $"Managed resolver requests observed during primary load: {context.ManagedResolverRequests.Count:N0}\n" +
                $"Private dependency loads observed during primary load: {context.PrivateLoads.Count:N0}\n" +
                $"Host framework loads observed during primary load: {context.HostLoads.Count:N0}\n" +
                $"Native load attempts observed: {context.NativeLoadAttempts.Count:N0}\n" +
                "Game entry point invoked: NO\n" +
                "Game type/member reflection performed: NO\n" +
                "Game method/delegate invoked: NO\n" +
                "Godot/game initialization requested: NO\n" +
                "Real managed install modified: NO");
        }
        catch (Exception ex)
        {
            ReleaseLoadContext();
            _primaryLoad = null;
            return Fail(FirstRealGameAssemblyLoadGate.PrimaryAssemblyLoad, stage, ex);
        }
    }

    public FirstRealGameAssemblyLoadGateResult RunPlannedDependencyResolution()
    {
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var preflight = RequirePreflight();
            RequirePrimaryLoad();
            var context = RequireLoadContext();

            stage = "binding-plan requirement normalization";
            var requirements = BuildBindingRequirements(preflight.Plan);
            var preparedByFullName = preflight.PreparedAssemblies
                .ToDictionary(item => item.Plan.AssemblyFullName, StringComparer.Ordinal);
            var hostResolved = 0;
            var privateResolved = 0;
            var deferredPrivateRequirements = 0;

            for (var index = 0; index < requirements.Length; index++)
            {
                var requirement = requirements[index];
                stage = $"runtime bind {index + 1}/{requirements.Length}: {requirement.RequestedFullName}";

                if (requirement.Kind == PlannedBindingKind.PrivatePrepared &&
                    preparedByFullName.TryGetValue(requirement.ExpectedTargetFullName, out var privateTarget) &&
                    privateTarget.ModuleInitializerCount > 0)
                {
                    deferredPrivateRequirements++;
                    continue;
                }

                var requested = new AssemblyName(requirement.RequestedFullName);
                var assembly = context.LoadFromAssemblyName(requested);
                var actualFullName = assembly.GetName().FullName ?? assembly.GetName().Name ?? string.Empty;
                var actualContext = AssemblyLoadContext.GetLoadContext(assembly);

                if (requirement.Kind == PlannedBindingKind.HostFramework)
                {
                    if (!ReferenceEquals(actualContext, AssemblyLoadContext.Default))
                        throw new InvalidDataException($"Host framework request resolved outside the default context: {requirement.RequestedFullName}");
                    if (!actualFullName.Equals(requirement.ExpectedTargetFullName, StringComparison.Ordinal))
                        throw new InvalidDataException($"Host framework target mismatch for '{requirement.RequestedFullName}'. Expected '{requirement.ExpectedTargetFullName}', actual '{actualFullName}'.");
                    hostResolved++;
                }
                else
                {
                    if (!ReferenceEquals(actualContext, context))
                        throw new InvalidDataException($"Private prepared request resolved outside the dedicated Step 23 context: {requirement.RequestedFullName}");
                    if (!actualFullName.Equals(requirement.ExpectedTargetFullName, StringComparison.Ordinal))
                        throw new InvalidDataException($"Private prepared target mismatch for '{requirement.RequestedFullName}'. Expected '{requirement.ExpectedTargetFullName}', actual '{actualFullName}'.");
                    privateResolved++;
                }
            }

            stage = "initializer-free private prepared-set load audit";
            var expectedPrivate = preflight.PreparedAssemblies
                .Where(item => item.ModuleInitializerCount == 0)
                .Select(item => item.Plan.AssemblyFullName)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            var actualPrivate = context.Assemblies
                .Select(assembly => assembly.GetName().FullName ?? assembly.GetName().Name ?? string.Empty)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            if (!expectedPrivate.SequenceEqual(actualPrivate, StringComparer.Ordinal))
            {
                throw new InvalidDataException(
                    "Step 23 runtime private assembly set differs from the maximal initializer-free prepared closure. " +
                    $"Expected [{string.Join(" | ", expectedPrivate)}], actual [{string.Join(" | ", actualPrivate)}].");
            }

            var deferredLoaded = AppDomain.CurrentDomain.GetAssemblies()
                .Select(assembly => assembly.GetName().FullName ?? assembly.GetName().Name ?? string.Empty)
                .Where(fullName => preflight.DeferredInitializerAssemblies.Any(item => item.Plan.AssemblyFullName.Equals(fullName, StringComparison.Ordinal)))
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            if (deferredLoaded.Length != 0)
                throw new InvalidDataException("Initializer-bearing private dependencies entered the CLR during Step 23: " + string.Join(" | ", deferredLoaded));

            if (context.DeferredInitializerRequests.Count != 0)
                throw new InvalidDataException("The CLR attempted to resolve an initializer-bearing private dependency during Step 23: " + string.Join(" | ", context.DeferredInitializerRequests));

            if (context.NativeLoadAttempts.Count != 0)
                throw new InvalidDataException("A native library resolution attempt occurred during Step 23 load-only dependency binding: " + string.Join(" | ", context.NativeLoadAttempts));
            if (context.RejectedManagedRequests.Count != 0)
                throw new InvalidDataException("The strict Step 23 resolver rejected one or more managed requests: " + string.Join(" | ", context.RejectedManagedRequests));

            _dependencyResolution = new DependencyResolutionSnapshot(
                requirements.Length,
                hostResolved,
                privateResolved,
                deferredPrivateRequirements,
                preflight.DeferredInitializerAssemblies.Length,
                actualPrivate.Length,
                context.ManagedResolverRequests.ToArray(),
                context.HostLoads.ToArray(),
                context.PrivateLoads.ToArray());

            return Pass(
                FirstRealGameAssemblyLoadGate.PlannedDependencyResolution,
                "Every host binding and initializer-free private dependency in the zero-blocker Step 21/22 plan resolved through the strict Step 23 runtime context without invoking game code. Initializer-bearing private dependencies remain deliberately deferred to Step 24.\n" +
                $"Unique planned binding requirements: {requirements.Length:N0}\n" +
                $"Host framework requirements resolved from default context: {hostResolved:N0}\n" +
                $"Private prepared requirements resolved from Step 23 context: {privateResolved:N0}\n" +
                $"Deferred initializer-bearing private requirements: {deferredPrivateRequirements:N0}\n" +
                $"Deferred initializer-bearing private assemblies: {preflight.DeferredInitializerAssemblies.Length:N0}\n" +
                $"Private assemblies resident in Step 23 context (primary + initializer-free closure): {actualPrivate.Length:N0}/{expectedPrivate.Length:N0}\n" +
                $"Strict resolver rejected managed requests: {context.RejectedManagedRequests.Count:N0}\n" +
                $"Native resolution attempts: {context.NativeLoadAttempts.Count:N0}\n" +
                $"Initializer-bearing dependency resolver requests: {context.DeferredInitializerRequests.Count:N0}\n" +
                "Unplanned non-framework fallback permitted: NO\n" +
                "Downloaded desktop framework implementation fallback permitted: NO\n" +
                "Game type/member reflection performed: NO\n" +
                "Game method/delegate invoked: NO");
        }
        catch (Exception ex)
        {
            _dependencyResolution = null;
            return Fail(FirstRealGameAssemblyLoadGate.PlannedDependencyResolution, stage, ex);
        }
    }

    public async Task<FirstRealGameAssemblyLoadGateResult> RunLoadIsolationAuditAsync(
        IProgress<FirstRealGameAssemblyLoadProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var preflight = RequirePreflight();
            var primaryLoad = RequirePrimaryLoad();
            var dependencyResolution = RequireDependencyResolution();
            var context = RequireLoadContext();

            stage = "binding-plan immutability";
            var currentPlanSha256 = await ComputeSha256HexAsync(_planPath, cancellationToken).ConfigureAwait(false);
            if (!currentPlanSha256.Equals(preflight.PlanSha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Persisted Step 21/22 binding plan changed during Step 23.");

            var verifiedPrepared = 0;
            for (var index = 0; index < preflight.PreparedAssemblies.Length; index++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var item = preflight.PreparedAssemblies[index];
                var relative = NormalizeRelative(item.Plan.RelativePath);
                stage = $"post-load byte audit: {relative}";
                progress?.Report(new FirstRealGameAssemblyLoadProgress(
                    FirstRealGameAssemblyLoadGate.LoadIsolationAudit,
                    index,
                    preflight.PreparedAssemblies.Length,
                    relative,
                    "Re-hashing prepared and trusted live bytes after the real CLR load…"));

                VerifyFileLength(item.PreparedPath, item.Plan.Length, "prepared");
                VerifyFileLength(item.LivePath, item.Plan.Length, "live");
                var preparedSha1 = await ComputeSha1HexAsync(item.PreparedPath, cancellationToken).ConfigureAwait(false);
                var liveSha1 = await ComputeSha1HexAsync(item.LivePath, cancellationToken).ConfigureAwait(false);
                if (!preparedSha1.Equals(item.Plan.Sha1Hex, StringComparison.OrdinalIgnoreCase) ||
                    !liveSha1.Equals(item.Plan.Sha1Hex, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidDataException($"Step 23 post-load byte isolation audit failed: {relative}");
                }
                verifiedPrepared++;
            }

            stage = "post-load OfflineReady verification";
            var offline = await _offlineInspection.RunAsync(null, cancellationToken).ConfigureAwait(false);
            if (!offline.Success || !offline.ExactManagedTreeVerified || offline.InstalledManifestId != preflight.Plan.ManifestId)
                throw new InvalidDataException(offline.Error ?? "OfflineReady exact-tree verification failed after the first real CLR load.");

            stage = "load-context ownership audit";
            var loadedGame = AppDomain.CurrentDomain.GetAssemblies()
                .Where(IsRealGameAssembly)
                .ToArray();
            if (loadedGame.Length != 1 || !ReferenceEquals(AssemblyLoadContext.GetLoadContext(loadedGame[0]), context))
                throw new InvalidDataException("The real sts2 assembly escaped the dedicated Step 23 load context or was loaded more than once.");

            var expectedPrivate = preflight.PreparedAssemblies
                .Where(item => item.ModuleInitializerCount == 0)
                .Select(item => item.Plan.AssemblyFullName)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            var actualPrivate = context.Assemblies
                .Select(assembly => assembly.GetName().FullName ?? assembly.GetName().Name ?? string.Empty)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            if (!expectedPrivate.SequenceEqual(actualPrivate, StringComparer.Ordinal))
                throw new InvalidDataException("Step 23 private load-context assembly membership changed after Gate C.");

            var deferredLoaded = AppDomain.CurrentDomain.GetAssemblies()
                .Select(assembly => assembly.GetName().FullName ?? assembly.GetName().Name ?? string.Empty)
                .Where(fullName => preflight.DeferredInitializerAssemblies.Any(item => item.Plan.AssemblyFullName.Equals(fullName, StringComparison.Ordinal)))
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            if (deferredLoaded.Length != 0)
                throw new InvalidDataException("Initializer-bearing private dependencies entered the CLR before Step 24: " + string.Join(" | ", deferredLoaded));

            if (context.NativeLoadAttempts.Count != 0)
                throw new InvalidDataException("Native library resolution occurred during Step 23 despite the load-only/native-refusal policy.");
            if (context.RejectedManagedRequests.Count != 0)
                throw new InvalidDataException("Step 23 strict managed resolver recorded rejected requests during a supposedly closed plan.");
            if (context.DeferredInitializerRequests.Count != 0)
                throw new InvalidDataException("Step 23 resolver was asked to load an initializer-bearing private dependency before Step 24.");

            progress?.Report(new FirstRealGameAssemblyLoadProgress(
                FirstRealGameAssemblyLoadGate.LoadIsolationAudit,
                preflight.PreparedAssemblies.Length,
                preflight.PreparedAssemblies.Length,
                preflight.Primary.Plan.RelativePath,
                "First real CLR load is isolated, plan-consistent, native-load-free and byte-preserving."));

            return Pass(
                FirstRealGameAssemblyLoadGate.LoadIsolationAudit,
                "Step 23 load-only isolation audit passed after the real sts2.dll and the maximal initializer-free planned managed closure entered the CLR. Initializer-bearing private dependencies remain outside the CLR for Step 24.\n" +
                $"Loaded primary identity: {primaryLoad.AssemblyFullName}\n" +
                $"Prepared/live assemblies re-hashed after load: {verifiedPrepared:N0}/{preflight.PreparedAssemblies.Length:N0}\n" +
                $"Runtime plan SHA-256 unchanged: {currentPlanSha256}\n" +
                $"Planned binding requirements considered: {dependencyResolution.TotalRequirements:N0}\n" +
                $"Deferred initializer-bearing private requirements: {dependencyResolution.DeferredPrivateRequirements:N0}\n" +
                $"Deferred initializer-bearing private assemblies: {dependencyResolution.DeferredInitializerAssemblies:N0}\n" +
                $"Private assemblies in dedicated context (initializer-free closure): {actualPrivate.Length:N0}/{expectedPrivate.Length:N0}\n" +
                $"Native load attempts: {context.NativeLoadAttempts.Count:N0}\n" +
                $"Rejected/unplanned managed requests: {context.RejectedManagedRequests.Count:N0}\n" +
                $"Initializer-bearing prepared dependencies loaded: 0/{preflight.DeferredInitializerAssemblies.Length:N0}\n" +
                "Post-load OfflineReady exact-tree verification: YES\n" +
                "Trusted Step 12 managed install unchanged: YES\n" +
                "Prepared Step 21/22 bytes unchanged: YES\n" +
                "Game entry point invoked: NO\n" +
                "Game type/member reflection performed: NO\n" +
                "Game method/delegate invoked: NO\n" +
                "Godot/game initialization requested: NO\n" +
                "Native game library loaded by Step 23: NO\n" +
                "Process note: the real sts2 managed assembly remains resident in the dedicated Step 23 context until process exit; force-quit before rerunning pre-load regressions that require no game assembly in the CLR.");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return Fail(FirstRealGameAssemblyLoadGate.LoadIsolationAudit, stage, ex);
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;
        ReleaseLoadContext();
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    private void ReleaseLoadContext()
    {
        var context = _loadContext;
        _loadContext = null;
        _primaryLoad = null;
        _dependencyResolution = null;
        if (context is not null && context.IsCollectible)
            context.Unload();
    }

    private void ValidatePlanForFirstLoad(RuntimeFrameworkBindingPlanDocument plan, SteamOfflineInstallResult offline)
    {
        if (plan.SchemaVersion != RuntimeFrameworkBindingPlanDocument.CurrentSchemaVersion)
            throw new InvalidDataException($"Unsupported Step 21/22 runtime-binding plan schema: {plan.SchemaVersion}");
        if (plan.AppId != SteamOfflineInstallInspection.TargetAppId)
            throw new InvalidDataException($"Runtime-binding plan targets unexpected App ID {plan.AppId}.");
        if (offline.DepotId != plan.DepotId || offline.InstalledManifestId != plan.ManifestId)
            throw new InvalidDataException("Runtime-binding plan depot/manifest does not match the currently OfflineReady install.");
        if (!string.Equals(offline.Branch, plan.Branch, StringComparison.Ordinal) ||
            !string.Equals(NormalizeRelative(offline.ManagedInstallRelativePath ?? string.Empty), NormalizeRelative(plan.ManagedInstallRelativePath), StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidDataException("Runtime-binding plan branch/managed-install path does not match the current OfflineReady install.");
        }
        if (!plan.RuntimeClosureReady || plan.Blockers.Length != 0)
            throw new InvalidDataException($"Step 23 refuses to load while runtime closure is not ready or blockers remain (ready={plan.RuntimeClosureReady}, blockers={plan.Blockers.Length}).");
        if (plan.Edges.Any(edge => edge.BindingKind.StartsWith("Blocker:", StringComparison.Ordinal)))
            throw new InvalidDataException("Step 23 plan contains blocker edges even though RuntimeClosureReady is true.");
        if (plan.PreparedAssemblies.Length == 0)
            throw new InvalidDataException("Step 23 runtime-binding plan contains no prepared assemblies.");

        var primary = plan.PreparedAssemblies.Where(item => item.IsPrimary).ToArray();
        if (primary.Length != 1)
            throw new InvalidDataException($"Step 23 expected exactly one prepared primary assembly, found {primary.Length}.");
        if (!primary[0].RelativePath.Equals(plan.PrimaryAssemblyRelativePath, StringComparison.OrdinalIgnoreCase) ||
            !primary[0].AssemblyFullName.Equals(plan.PrimaryAssemblyFullName, StringComparison.Ordinal))
        {
            throw new InvalidDataException("Step 23 primary prepared entry does not match the plan's primary identity/path.");
        }
        var primaryIdentity = new AssemblyName(plan.PrimaryAssemblyFullName);
        if (!string.Equals(primaryIdentity.Name, _expectedPrimarySimpleName, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException($"Step 23 expected primary simple name '{_expectedPrimarySimpleName}', found '{primaryIdentity.Name}'.");

        var preparedSimpleNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var prepared in plan.PreparedAssemblies)
        {
            var identity = new AssemblyName(prepared.AssemblyFullName);
            var simple = identity.Name ?? throw new InvalidDataException($"Prepared assembly identity has no simple name: {prepared.AssemblyFullName}");
            if (!preparedSimpleNames.Add(simple))
                throw new InvalidDataException($"Step 23 plan contains duplicate prepared simple name '{simple}'.");
            if (IsHostFrameworkContractName(simple))
                throw new InvalidDataException($"Step 23 zero-blocker prepared set still contains framework-shaped private assembly '{simple}'. Step 22 host-only closure must be rerun.");
        }
    }

    private static PlannedBindingRequirement[] BuildBindingRequirements(RuntimeFrameworkBindingPlanDocument plan)
    {
        var requirements = new List<PlannedBindingRequirement>();
        foreach (var group in plan.Edges.GroupBy(edge => edge.RequestedFullName, StringComparer.Ordinal))
        {
            var normalized = group.Select(edge =>
            {
                if (edge.BindingKind.Equals("HostFramework", StringComparison.Ordinal))
                    return new PlannedBindingRequirement(edge.RequestedFullName, PlannedBindingKind.HostFramework, edge.Target);
                if (edge.BindingKind.Equals("WorkspaceExact", StringComparison.Ordinal) ||
                    edge.BindingKind.Equals("WorkspaceVersionUnified", StringComparison.Ordinal))
                {
                    return new PlannedBindingRequirement(edge.RequestedFullName, PlannedBindingKind.PrivatePrepared, edge.Target);
                }
                throw new InvalidDataException($"Step 23 cannot execute plan edge kind '{edge.BindingKind}' for {edge.RequestedFullName}.");
            }).Distinct().ToArray();

            if (normalized.Length != 1)
            {
                throw new InvalidDataException(
                    $"Step 23 plan has inconsistent runtime targets for requested identity '{group.Key}': " +
                    string.Join(" | ", normalized.Select(item => $"{item.Kind}:{item.ExpectedTargetFullName}")));
            }
            requirements.Add(normalized[0]);
        }

        return requirements
            .OrderBy(item => item.RequestedFullName, StringComparer.Ordinal)
            .ToArray();
    }

    private static PreparedMetadataSnapshot ReadPreparedMetadata(string path)
    {
        using var module = ModuleDefinition.ReadModule(path, new ReaderParameters
        {
            InMemory = true,
            ReadSymbols = false,
            ReadingMode = ReadingMode.Deferred,
        });
        if (module.Assembly?.Name is null)
            throw new BadImageFormatException($"Managed assembly manifest missing: {path}");

        var moduleInitializers = module.Types
            .Where(type => type.Name.Equals("<Module>", StringComparison.Ordinal))
            .SelectMany(type => type.Methods)
            .Where(method => method.Name.Equals(".cctor", StringComparison.Ordinal) && method.IsStatic && method.HasBody)
            .ToArray();
        var moduleInitializerAudits = moduleInitializers
            .Select(FormatModuleInitializerAudit)
            .ToArray();
        var moduleInitializerCount = moduleInitializers.Length;
        var pinvokeCount = EnumerateTypes(module.Types)
            .SelectMany(type => type.Methods)
            .Count(method => method.IsPInvokeImpl || method.PInvokeInfo is not null);
        var references = module.AssemblyReferences
            .Select(reference => reference.FullName)
            .OrderBy(value => value, StringComparer.Ordinal)
            .ToArray();

        return new PreparedMetadataSnapshot(
            module.Assembly.Name.FullName,
            (module.Attributes & ModuleAttributes.ILOnly) != 0,
            references,
            moduleInitializerCount,
            moduleInitializerAudits,
            pinvokeCount,
            module.ModuleReferences.Count);
    }

    private static string FormatModuleInitializerAudit(MethodDefinition method)
    {
        const int maxInstructions = 96;
        var instructions = method.Body.Instructions;
        var rendered = instructions
            .Take(maxInstructions)
            .Select(instruction => $"IL_{instruction.Offset:X4}: {instruction.OpCode.Code} {FormatInstructionOperand(instruction.Operand)}".TrimEnd())
            .ToArray();
        var suffix = instructions.Count > maxInstructions ? $" | ... {instructions.Count - maxInstructions} more instruction(s)" : string.Empty;
        return $"token=0x{method.MetadataToken.ToInt32():X8}; instructions={instructions.Count}; handlers={method.Body.ExceptionHandlers.Count}; locals={method.Body.Variables.Count}; IL=[{string.Join(" | ", rendered)}]{suffix}";
    }

    private static string FormatInstructionOperand(object? operand)
        => operand switch
        {
            null => string.Empty,
            MethodReference method => method.FullName,
            FieldReference field => field.FullName,
            TypeReference type => type.FullName,
            string value => $"\"{value.Replace("\r", "\\r", StringComparison.Ordinal).Replace("\n", "\\n", StringComparison.Ordinal)}\"",
            Instruction target => $"IL_{target.Offset:X4}",
            Instruction[] targets => string.Join(",", targets.Select(target => $"IL_{target.Offset:X4}")),
            _ => operand.ToString() ?? string.Empty,
        };

    private static IEnumerable<TypeDefinition> EnumerateTypes(IEnumerable<TypeDefinition> roots)
    {
        foreach (var type in roots)
        {
            yield return type;
            foreach (var nested in EnumerateTypes(type.NestedTypes))
                yield return nested;
        }
    }

    private void EnsureNoRealGameAssemblyLoaded()
    {
        var matches = AppDomain.CurrentDomain.GetAssemblies()
            .Where(IsRealGameAssembly)
            .Select(assembly =>
            {
                var context = AssemblyLoadContext.GetLoadContext(assembly);
                return $"{assembly.GetName().FullName} @ {context?.Name ?? "<unknown-context>"}";
            })
            .OrderBy(value => value, StringComparer.Ordinal)
            .ToArray();
        if (matches.Length != 0)
            throw new InvalidDataException("Step 23 requires a fresh process before Gate A; a real game assembly is already loaded: " + string.Join(" | ", matches));
    }

    private bool IsRealGameAssembly(Assembly assembly)
    {
        var name = assembly.GetName().Name ?? string.Empty;
        return _freshProcessAssemblyNames.Contains(name);
    }

    private static bool IsHostFrameworkContractName(string name)
        => name.Equals("System", StringComparison.OrdinalIgnoreCase) ||
           name.StartsWith("System.", StringComparison.OrdinalIgnoreCase) ||
           name.Equals("mscorlib", StringComparison.OrdinalIgnoreCase) ||
           name.Equals("netstandard", StringComparison.OrdinalIgnoreCase) ||
           name.Equals("Microsoft.CSharp", StringComparison.OrdinalIgnoreCase) ||
           name.Equals("Microsoft.VisualBasic", StringComparison.OrdinalIgnoreCase) ||
           name.Equals("Microsoft.VisualBasic.Core", StringComparison.OrdinalIgnoreCase) ||
           name.StartsWith("Microsoft.Win32.", StringComparison.OrdinalIgnoreCase);

    private static bool SameIdentityIgnoringVersion(AssemblyName left, AssemblyName right)
    {
        if (!string.Equals(left.Name, right.Name, StringComparison.OrdinalIgnoreCase))
            return false;
        if (!string.Equals(NormalizeCulture(left.CultureName), NormalizeCulture(right.CultureName), StringComparison.OrdinalIgnoreCase))
            return false;
        return TokenHex(left).Equals(TokenHex(right), StringComparison.OrdinalIgnoreCase);
    }

    private static bool ExactRequestedIdentity(AssemblyName left, AssemblyName right)
        => SameIdentityIgnoringVersion(left, right) && (left.Version ?? ZeroVersion) == (right.Version ?? ZeroVersion);

    private static string NormalizeCulture(string? culture)
        => string.IsNullOrWhiteSpace(culture) ? "neutral" : culture;

    private static string TokenHex(AssemblyName name)
        => Convert.ToHexString(name.GetPublicKeyToken() ?? []).ToLowerInvariant();

    private static readonly Version ZeroVersion = new(0, 0, 0, 0);

    private static void VerifyFileLength(string path, long expected, string scope)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"Step 23 {scope} file is missing.", path);
        var actual = new FileInfo(path).Length;
        if (actual != expected)
            throw new InvalidDataException($"Step 23 {scope} file length mismatch for {path}: {actual} != {expected}.");
    }

    private static async Task<string> ComputeSha1HexAsync(string path, CancellationToken cancellationToken)
    {
        await using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 64 * 1024,
            options: FileOptions.Asynchronous | FileOptions.SequentialScan);
        using var sha1 = SHA1.Create();
        var hash = await sha1.ComputeHashAsync(stream, cancellationToken).ConfigureAwait(false);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static string ComputeSha1Hex(string path)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Convert.ToHexString(SHA1.HashData(stream)).ToLowerInvariant();
    }

    private static async Task<string> ComputeSha256HexAsync(string path, CancellationToken cancellationToken)
    {
        await using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 64 * 1024,
            options: FileOptions.Asynchronous | FileOptions.SequentialScan);
        using var sha256 = SHA256.Create();
        var hash = await sha256.ComputeHashAsync(stream, cancellationToken).ConfigureAwait(false);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static string ResolveChildPath(string root, string relativePath, string scope)
    {
        var normalized = NormalizeRelative(relativePath);
        if (!SteamSingleFileTargetSelector.IsSafeRelativePath(normalized))
            throw new InvalidDataException($"Unsafe {scope}: {relativePath}");
        var fullRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var full = Path.GetFullPath(Path.Combine(root, normalized.Replace('/', Path.DirectorySeparatorChar)));
        if (!full.StartsWith(fullRoot, StringComparison.Ordinal))
            throw new InvalidDataException($"{scope} escaped its declared root: {relativePath}");
        return full;
    }

    private static string NormalizeRelative(string path)
        => path.Replace('\\', '/').TrimStart('/');

    private PreflightSnapshot RequirePreflight()
        => _preflight ?? throw new InvalidOperationException("Step 23 Gate A must pass before Gate B.");

    private PrimaryLoadSnapshot RequirePrimaryLoad()
        => _primaryLoad ?? throw new InvalidOperationException("Step 23 Gate B must pass before Gate C.");

    private DependencyResolutionSnapshot RequireDependencyResolution()
        => _dependencyResolution ?? throw new InvalidOperationException("Step 23 Gate C must pass before Gate D.");

    private Step23GameLoadContext RequireLoadContext()
        => _loadContext ?? throw new InvalidOperationException("Step 23 dedicated load context is not available.");

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(FirstRealGameAssemblyLoad));
    }

    private static FirstRealGameAssemblyLoadGateResult Pass(FirstRealGameAssemblyLoadGate gate, string detail)
        => new(gate, true, detail);

    private static FirstRealGameAssemblyLoadGateResult Fail(FirstRealGameAssemblyLoadGate gate, string stage, Exception ex)
        => new(gate, false, $"Stage: {stage}\n{ex.GetType().Name}: {ex.Message}");

    private sealed class CallbackProgress<T> : IProgress<T>
    {
        private readonly Action<T> _callback;
        public CallbackProgress(Action<T> callback) => _callback = callback ?? throw new ArgumentNullException(nameof(callback));
        public void Report(T value) => _callback(value);
    }

    private enum PlannedBindingKind
    {
        HostFramework,
        PrivatePrepared,
    }

    private sealed record PlannedBindingRequirement(
        string RequestedFullName,
        PlannedBindingKind Kind,
        string ExpectedTargetFullName);

    private sealed record PreparedMetadataSnapshot(
        string FullName,
        bool IsIlOnly,
        IReadOnlyList<string> AssemblyReferences,
        int ModuleInitializerCount,
        IReadOnlyList<string> ModuleInitializerAudits,
        int PInvokeMethodCount,
        int ModuleReferenceCount);

    private sealed record PreparedAssemblySnapshot(
        RuntimeBindingPreparedAssembly Plan,
        string PreparedPath,
        string LivePath,
        AssemblyName AssemblyName,
        IReadOnlyList<string> AssemblyReferences,
        int ModuleInitializerCount,
        IReadOnlyList<string> ModuleInitializerAudits,
        int PInvokeMethodCount,
        int ModuleReferenceCount);

    private sealed record PreflightSnapshot(
        RuntimeFrameworkBindingPlanDocument Plan,
        string PlanSha256,
        string ManagedRoot,
        PreparedAssemblySnapshot[] PreparedAssemblies,
        PreparedAssemblySnapshot Primary,
        PreparedAssemblySnapshot[] DeferredInitializerAssemblies,
        SteamOfflineInstallResult Offline,
        int ModuleInitializerCount,
        int PInvokeMethodCount,
        int ModuleReferenceCount,
        bool DynamicCodeSupported,
        bool DynamicCodeCompiled);

    private sealed record PrimaryLoadSnapshot(
        string AssemblyFullName,
        string LoadContextName,
        string ImmediateSha1,
        int ManagedResolverRequestsAtLoad,
        int PrivateLoadsAtLoad,
        int HostLoadsAtLoad,
        int NativeLoadAttemptsAtLoad);

    private sealed record DependencyResolutionSnapshot(
        int TotalRequirements,
        int HostRequirements,
        int PrivateRequirements,
        int DeferredPrivateRequirements,
        int DeferredInitializerAssemblies,
        int LoadedPrivateAssemblies,
        IReadOnlyList<string> ManagedResolverRequests,
        IReadOnlyList<string> HostLoads,
        IReadOnlyList<string> PrivateLoads);

    private sealed class Step23GameLoadContext : AssemblyLoadContext
    {
        private readonly IReadOnlyDictionary<string, PreparedAssemblySnapshot> _privateBySimpleName;
        private readonly RuntimeBindingHostFramework[] _hostBindings;

        public Step23GameLoadContext(
            string name,
            RuntimeFrameworkBindingPlanDocument plan,
            IReadOnlyList<PreparedAssemblySnapshot> preparedAssemblies,
            bool isCollectible)
            : base(name, isCollectible)
        {
            var privateBySimpleName = new Dictionary<string, PreparedAssemblySnapshot>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in preparedAssemblies)
            {
                var simple = item.AssemblyName.Name ?? throw new InvalidDataException($"Prepared assembly identity has no simple name: {item.Plan.AssemblyFullName}");
                if (!privateBySimpleName.TryAdd(simple, item))
                    throw new InvalidDataException($"Step 23 resolver received duplicate prepared simple name '{simple}'.");
            }
            _privateBySimpleName = privateBySimpleName;
            _hostBindings = plan.HostFrameworkBindings;
        }

        public List<string> ManagedResolverRequests { get; } = [];
        public List<string> PrivateLoads { get; } = [];
        public List<string> HostLoads { get; } = [];
        public List<string> RejectedManagedRequests { get; } = [];
        public List<string> DeferredInitializerRequests { get; } = [];
        public List<string> NativeLoadAttempts { get; } = [];

        [UnconditionalSuppressMessage(
            "Trimming",
            "IL2026",
            Justification = "Step 23 loads only receipt-verified IL-only prepared assemblies selected by the audited zero-blocker Step 21/22 binding plan.")]
        public Assembly LoadPrimary(PreparedAssemblySnapshot primary)
        {
            var hash = ComputeSha1Hex(primary.PreparedPath);
            if (!hash.Equals(primary.Plan.Sha1Hex, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Step 23 primary SHA-1 changed immediately before LoadFromStream.");

            using var stream = new FileStream(primary.PreparedPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return LoadFromStream(stream);
        }

        [UnconditionalSuppressMessage(
            "Trimming",
            "IL2026",
            Justification = "Step 23 resolves only receipt-verified IL-only prepared assemblies from the audited zero-blocker binding plan; no loaded game member is reflected or invoked.")]
        protected override Assembly? Load(AssemblyName assemblyName)
        {
            var requestedFullName = assemblyName.FullName ?? assemblyName.Name ?? "<unknown>";
            ManagedResolverRequests.Add(requestedFullName);

            if (assemblyName.Name is null)
                return Reject(requestedFullName, "assembly request has no simple name");

            if (_privateBySimpleName.TryGetValue(assemblyName.Name, out var privateAssembly))
            {
                if (privateAssembly.ModuleInitializerCount > 0)
                {
                    var detail = $"{requestedFullName} => {privateAssembly.Plan.AssemblyFullName}";
                    DeferredInitializerRequests.Add(detail);
                    throw new FileLoadException(
                        "Step 23 refuses to load an initializer-bearing private dependency before the Step 24 initialization boundary: " + detail);
                }

                if (!SameIdentityIgnoringVersion(assemblyName, privateAssembly.AssemblyName) ||
                    (privateAssembly.AssemblyName.Version ?? ZeroVersion).CompareTo(assemblyName.Version ?? ZeroVersion) < 0)
                {
                    return Reject(
                        requestedFullName,
                        $"verified private candidate identity/version is incompatible: {privateAssembly.Plan.AssemblyFullName}");
                }

                var hash = ComputeSha1Hex(privateAssembly.PreparedPath);
                if (!hash.Equals(privateAssembly.Plan.Sha1Hex, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException($"Step 23 private dependency SHA-1 changed immediately before load: {privateAssembly.Plan.RelativePath}");

                using var stream = new FileStream(privateAssembly.PreparedPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                var loaded = LoadFromStream(stream);
                PrivateLoads.Add($"{requestedFullName} => {loaded.GetName().FullName}");
                return loaded;
            }

            var hostMatches = _hostBindings
                .Where(binding => ExactRequestedIdentity(assemblyName, new AssemblyName(binding.RequestedFullName)))
                .ToArray();
            if (hostMatches.Length == 0)
                return Reject(requestedFullName, "request is neither a prepared private assembly nor an exact planned host-framework binding");

            var allowedActual = hostMatches
                .Select(binding => binding.ActualFullName)
                .Distinct(StringComparer.Ordinal)
                .ToHashSet(StringComparer.Ordinal);
            var hostAssembly = AssemblyLoadContext.Default.LoadFromAssemblyName(assemblyName);
            var actualFullName = hostAssembly.GetName().FullName ?? hostAssembly.GetName().Name ?? string.Empty;
            if (!allowedActual.Contains(actualFullName))
            {
                throw new FileLoadException(
                    $"Step 23 host binding drift for '{requestedFullName}'. Planned actual identity: {string.Join(" | ", allowedActual)}; runtime actual: {actualFullName}.");
            }

            HostLoads.Add($"{requestedFullName} => {actualFullName}");
            return hostAssembly;
        }

        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            NativeLoadAttempts.Add(unmanagedDllName);
            throw new DllNotFoundException(
                $"Step 23 load-only boundary refuses native library resolution for '{unmanagedDllName}'. Native game integration is a later subsystem.");
        }

        private Assembly? Reject(string requestedFullName, string reason)
        {
            var detail = $"{requestedFullName} — {reason}";
            RejectedManagedRequests.Add(detail);
            throw new FileLoadException("Step 23 strict managed resolver rejected an unplanned request: " + detail);
        }
    }
}
