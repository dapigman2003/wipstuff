using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Security.Cryptography;
using System.Text.Json;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace StS2Launcher.Core;

/// <summary>
/// Step 24 boundary. Replays the physically proven Step 23 initializer-free load state in a fresh
/// private AssemblyLoadContext, then admits exactly the known deferred 0Harmony dependency and
/// explicitly ensures its module constructor has completed. No Harmony API, game entry point,
/// game type/member, Godot startup, or native game library is intentionally invoked.
/// </summary>
public sealed class ControlledManagedInitialization : IDisposable
{
    public const string TargetSimpleName = "0Harmony";
    public static readonly Version TargetVersion = new(2, 4, 2, 0);
    public const string LoadContextName = "StS2Launcher-Step24-Initialization";

    private readonly string _launcherDataRoot;
    private readonly string _step21WorkRoot;
    private readonly string _preparedRoot;
    private readonly string _planPath;
    private readonly SteamOfflineInstallInspection _offlineInspection;
    private readonly FirstRealGameAssemblyLoad _step23Preflight;
    private readonly bool _collectibleLoadContext;
    private readonly string _expectedPrimarySimpleName;
    private readonly string _targetSimpleName;
    private readonly Version _targetVersion;
    private readonly HashSet<string> _freshProcessAssemblyNames;

    private InitializationPreflightSnapshot? _preflight;
    private ProvenLoadReplaySnapshot? _replay;
    private DeferredInitializationSnapshot? _initialization;
    private Step24LoadContext? _loadContext;
    private bool _disposed;

    public ControlledManagedInitialization(string launcherDataRoot, bool collectibleLoadContext = false)
        : this(
            launcherDataRoot,
            collectibleLoadContext,
            FirstRealGameAssemblyLoad.ExpectedPrimarySimpleName,
            TargetSimpleName,
            TargetVersion,
            [FirstRealGameAssemblyLoad.ExpectedPrimarySimpleName, "SlayTheSpire2", TargetSimpleName])
    {
    }

    internal ControlledManagedInitialization(
        string launcherDataRoot,
        bool collectibleLoadContext,
        string expectedPrimarySimpleName,
        string targetSimpleName,
        Version targetVersion,
        IReadOnlyCollection<string> freshProcessAssemblyNames)
    {
        if (string.IsNullOrWhiteSpace(launcherDataRoot))
            throw new ArgumentException("Launcher data root is required.", nameof(launcherDataRoot));
        if (string.IsNullOrWhiteSpace(expectedPrimarySimpleName))
            throw new ArgumentException("Expected primary simple name is required.", nameof(expectedPrimarySimpleName));
        if (string.IsNullOrWhiteSpace(targetSimpleName))
            throw new ArgumentException("Target simple name is required.", nameof(targetSimpleName));
        if (targetVersion is null)
            throw new ArgumentNullException(nameof(targetVersion));
        if (freshProcessAssemblyNames is null || freshProcessAssemblyNames.Count == 0)
            throw new ArgumentException("At least one fresh-process assembly identity is required.", nameof(freshProcessAssemblyNames));

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
        _targetSimpleName = targetSimpleName.Trim();
        _targetVersion = targetVersion;
        _freshProcessAssemblyNames = freshProcessAssemblyNames
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        _freshProcessAssemblyNames.Add(_expectedPrimarySimpleName);
        _freshProcessAssemblyNames.Add(_targetSimpleName);
        _step23Preflight = new FirstRealGameAssemblyLoad(
            _launcherDataRoot,
            collectibleLoadContext: true,
            expectedPrimarySimpleName: _expectedPrimarySimpleName,
            freshProcessAssemblyNames: _freshProcessAssemblyNames.Where(name => !name.Equals(_targetSimpleName, StringComparison.OrdinalIgnoreCase)).ToArray());
    }

    public void Reset()
    {
        ThrowIfDisposed();
        ReleaseLoadContext();
        _step23Preflight.Reset();
        _preflight = null;
        _replay = null;
        _initialization = null;
    }

    public async Task<ControlledManagedInitializationGateResult> RunInitializationPreflightAsync(
        IProgress<ControlledManagedInitializationProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            EnsureFreshProcess();
            cancellationToken.ThrowIfCancellationRequested();

            stage = "accepted Step 23 preflight replay";
            progress?.Report(new ControlledManagedInitializationProgress(
                ControlledManagedInitializationGate.InitializationPreflight,
                0,
                0,
                null,
                "Re-running the physically proven Step 23 Gate A preflight before any Step 24 CLR load…"));

            _step23Preflight.Reset();
            var step23Result = await _step23Preflight.RunPreparedLoadPreflightAsync(
                progress is null
                    ? null
                    : new CallbackProgress<FirstRealGameAssemblyLoadProgress>(value =>
                        progress.Report(new ControlledManagedInitializationProgress(
                            ControlledManagedInitializationGate.InitializationPreflight,
                            value.ProcessedItems,
                            value.TotalItems,
                            value.CurrentPath,
                            "Step 23 prerequisite: " + value.Detail))),
                cancellationToken).ConfigureAwait(false);
            if (!step23Result.Passed)
                throw new InvalidDataException("The accepted Step 23 preflight no longer passes. " + step23Result.Detail);

            stage = "persisted plan reload";
            if (!File.Exists(_planPath))
                throw new FileNotFoundException("Step 24 runtime-binding plan is missing.", _planPath);

            RuntimeFrameworkBindingPlanDocument plan;
            await using (var stream = new FileStream(
                _planPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                64 * 1024,
                FileOptions.Asynchronous | FileOptions.SequentialScan))
            {
                plan = await JsonSerializer.DeserializeAsync(
                    stream,
                    RuntimeFrameworkBindingJsonContext.Default.RuntimeFrameworkBindingPlanDocument,
                    cancellationToken).ConfigureAwait(false)
                    ?? throw new InvalidDataException("Step 24 could not deserialize the persisted runtime-binding plan.");
            }

            if (!plan.RuntimeClosureReady || plan.Blockers.Length != 0)
                throw new InvalidDataException("Step 24 requires the physically proven zero-blocker Step 21/22 runtime plan.");

            var offline = await _offlineInspection.RunAsync(null, cancellationToken).ConfigureAwait(false);
            if (!offline.Success || !offline.ExactManagedTreeVerified || string.IsNullOrWhiteSpace(offline.ManagedInstallRelativePath))
                throw new InvalidDataException(offline.Error ?? "Step 24 requires an exact OfflineReady managed install before any CLR load.");
            if (offline.DepotId != plan.DepotId || offline.InstalledManifestId != plan.ManifestId)
                throw new InvalidDataException("Step 24 runtime-binding plan depot/manifest no longer matches the OfflineReady install.");

            var managedRoot = ResolveChildPath(
                _launcherDataRoot,
                NormalizeRelative(offline.ManagedInstallRelativePath ?? plan.ManagedInstallRelativePath),
                "managed install root");

            // First classify only initializer presence across the exact prepared plan. Keep this pass
            // deliberately shallow: no method-body traversal and no dependency metadata resolution.
            // The detailed automatic-execution closure is audited only after the sole target identity
            // has been established from the complete prepared set.
            stage = "prepared initializer classification";
            var prepared = new List<PreparedAssemblySnapshot>(plan.PreparedAssemblies.Length);
            foreach (var item in plan.PreparedAssemblies)
            {
                cancellationToken.ThrowIfCancellationRequested();
                stage = $"prepared initializer classification: {item.RelativePath}";
                var preparedPath = ResolveChildPath(_preparedRoot, item.RelativePath, "prepared assembly path");
                var livePath = ResolveChildPath(managedRoot, item.RelativePath, "live assembly path");
                var metadata = ReadPreparedMetadata(preparedPath, includeInitializerCallGraph: false, _targetSimpleName);
                prepared.Add(new PreparedAssemblySnapshot(
                    item,
                    preparedPath,
                    livePath,
                    new AssemblyName(item.AssemblyFullName),
                    metadata.ModuleInitializerCount,
                    metadata.AutomaticInitializerCount,
                    metadata.AutomaticInitializerAudits,
                    metadata.InitializerReachableMethods,
                    metadata.InitializerHazards));
            }

            var primary = prepared.Single(item => item.Plan.IsPrimary);
            if (primary.ModuleInitializerCount != 0)
                throw new InvalidDataException("Step 24 cannot proceed because the primary sts2.dll is no longer initializer-free.");

            var initializerBearing = prepared
                .Where(item => !item.Plan.IsPrimary && item.ModuleInitializerCount > 0)
                .OrderBy(item => item.Plan.AssemblyFullName, StringComparer.Ordinal)
                .ToArray();
            if (initializerBearing.Length != 1)
            {
                throw new InvalidDataException(
                    $"Step 24.0 is intentionally scoped to exactly one deferred initializer-bearing dependency, but found {initializerBearing.Length}: " +
                    string.Join(" | ", initializerBearing.Select(item => item.Plan.AssemblyFullName)));
            }

            var target = initializerBearing[0];
            if (!string.Equals(target.AssemblyName.Name, _targetSimpleName, StringComparison.OrdinalIgnoreCase) ||
                (target.AssemblyName.Version ?? ZeroVersion) != _targetVersion)
            {
                throw new InvalidDataException(
                    $"Step 24.0 expected exactly {_targetSimpleName}, Version={_targetVersion}, but found {target.Plan.AssemblyFullName}.");
            }
            if (target.ModuleInitializerCount != 1)
                throw new InvalidDataException($"Step 24.0 expected exactly one {_targetSimpleName} <Module>..cctor, found {target.ModuleInitializerCount}.");

            // Now audit only the exact deferred target. This prevents unrelated prepared assemblies
            // from forcing method-body materialization merely to classify whether they own a module
            // initializer, and gives any remaining metadata-only failure an exact target/stage.
            stage = $"target automatic-initialization closure audit: {target.Plan.RelativePath}";
            var targetMetadata = ReadPreparedMetadata(target.PreparedPath, includeInitializerCallGraph: true, _targetSimpleName);
            if (targetMetadata.ModuleInitializerCount != target.ModuleInitializerCount)
                throw new InvalidDataException("Step 24 target module-initializer count changed between shallow classification and detailed audit.");

            target = target with
            {
                AutomaticInitializerCount = targetMetadata.AutomaticInitializerCount,
                AutomaticInitializerAudits = targetMetadata.AutomaticInitializerAudits,
                InitializerReachableMethods = targetMetadata.InitializerReachableMethods,
                InitializerHazards = targetMetadata.InitializerHazards,
            };
            var targetIndex = prepared.FindIndex(item => item.Plan.RelativePath.Equals(target.Plan.RelativePath, StringComparison.Ordinal));
            if (targetIndex < 0)
                throw new InvalidDataException("Step 24 could not relocate the selected initializer target in the prepared classification set.");
            prepared[targetIndex] = target;

            if (target.InitializerHazards.Count != 0)
            {
                throw new InvalidDataException(
                    "Step 24 Gate A refuses automatic initialization because the bounded Cecil call-graph audit found a prohibited or unresolved execution edge:\n" +
                    string.Join("\n", target.InitializerHazards) + "\n" +
                    "Audited automatic-initialization IL:\n" + string.Join("\n", target.AutomaticInitializerAudits));
            }

            stage = "plan digest";
            var planSha256 = await ComputeSha256HexAsync(_planPath, cancellationToken).ConfigureAwait(false);
            _preflight = new InitializationPreflightSnapshot(
                plan,
                planSha256,
                managedRoot,
                prepared.ToArray(),
                primary,
                target,
                offline);

            progress?.Report(new ControlledManagedInitializationProgress(
                ControlledManagedInitializationGate.InitializationPreflight,
                prepared.Count,
                prepared.Count,
                target.Plan.RelativePath,
                "The accepted Step 23 preflight still passes and the sole deferred initializer is exactly 0Harmony 2.4.2.0 with no prohibited edge in the bounded automatic-initialization closure."));

            return Pass(
                ControlledManagedInitializationGate.InitializationPreflight,
                "Step 24 initialization preflight passed before any Step 24 CLR load.\n" +
                "Accepted Step 23 Gate A replay: PASS\n" +
                $"Runtime plan SHA-256: {planSha256}\n" +
                $"Prepared assemblies: {prepared.Count:N0}\n" +
                $"Initializer-bearing dependencies: {initializerBearing.Length:N0}\n" +
                $"Initialization target: {target.Plan.AssemblyFullName}\n" +
                $"Target module initializers: {target.ModuleInitializerCount:N0}\n" +
                $"Automatic initializer methods in audited closure: {target.AutomaticInitializerCount:N0}\n" +
                $"Initializer reachable same-assembly methods audited: {target.InitializerReachableMethods:N0}\n" +
                $"Initializer hazards: {target.InitializerHazards.Count:N0}\n" +
                "Audited automatic-initialization IL:\n" + string.Join("\n", target.AutomaticInitializerAudits) + "\n" +
                "No real game/Harmony assembly was loaded by Step 24 Gate A: YES");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return Fail(ControlledManagedInitializationGate.InitializationPreflight, stage, ex);
        }
    }

    public ControlledManagedInitializationGateResult RunProvenLoadStateReplay()
    {
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var preflight = RequirePreflight();
            EnsureFreshProcess();

            stage = "dedicated load context creation";
            var context = new Step24LoadContext(
                LoadContextName,
                preflight.Plan,
                preflight.PreparedAssemblies,
                _collectibleLoadContext);
            _loadContext = context;

            stage = "primary load replay";
            var primaryAssembly = context.LoadPrepared(preflight.Primary, allowInitializer: false, explicitReason: "Step 23 primary replay");
            var primaryIdentity = primaryAssembly.GetName().FullName ?? primaryAssembly.GetName().Name ?? string.Empty;
            if (!primaryIdentity.Equals(preflight.Primary.Plan.AssemblyFullName, StringComparison.Ordinal))
                throw new InvalidDataException("Step 24 primary replay identity drifted from the persisted plan.");
            if (!ReferenceEquals(AssemblyLoadContext.GetLoadContext(primaryAssembly), context))
                throw new InvalidDataException("Step 24 primary replay did not remain in the dedicated context.");

            stage = "host + initializer-free private closure replay";
            var requirements = BuildBindingRequirements(preflight.Plan);
            var deferredRequirements = 0;
            foreach (var requirement in requirements)
            {
                if (requirement.Kind == PlannedBindingKind.HostFramework)
                {
                    context.ResolvePlanned(new AssemblyName(requirement.RequestedFullName));
                    continue;
                }

                var target = FindPreparedByTarget(preflight.PreparedAssemblies, requirement.ExpectedTargetFullName);
                if (target.ModuleInitializerCount > 0)
                {
                    deferredRequirements++;
                    continue;
                }
                context.ResolvePlanned(new AssemblyName(requirement.RequestedFullName));
            }

            stage = "accepted Step 23 context audit";
            var expected = preflight.PreparedAssemblies
                .Where(item => item.ModuleInitializerCount == 0)
                .Select(item => item.Plan.AssemblyFullName)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            var actual = context.Assemblies
                .Select(assembly => assembly.GetName().FullName ?? assembly.GetName().Name ?? string.Empty)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            if (!actual.SequenceEqual(expected, StringComparer.Ordinal))
            {
                throw new InvalidDataException(
                    "Step 24 Gate B did not reproduce the physically proven Step 23 initializer-free private context exactly. " +
                    $"Expected [{string.Join(" | ", expected)}], actual [{string.Join(" | ", actual)}].");
            }
            if (actual.Contains(preflight.Target.Plan.AssemblyFullName, StringComparer.Ordinal))
                throw new InvalidDataException("0Harmony entered the CLR before the explicit Step 24 initialization gate.");
            if (context.NativeLoadAttempts.Count != 0 || context.RejectedManagedRequests.Count != 0)
                throw new InvalidDataException("Step 24 Gate B replay encountered an unexpected managed/native resolver event.");

            _replay = new ProvenLoadReplaySnapshot(
                primaryIdentity,
                expected.Length,
                deferredRequirements,
                context.ManagedResolverRequests.Count,
                context.HostLoads.Count,
                context.PrivateLoads.Count,
                context.NativeLoadAttempts.Count);

            return Pass(
                ControlledManagedInitializationGate.ProvenLoadStateReplay,
                "Step 24 reproduced the physically proven Step 23 load-only state in the same private context that will be used for initialization.\n" +
                $"Primary: {primaryIdentity}\n" +
                $"Initializer-free private context: {actual.Length:N0}/{expected.Length:N0}\n" +
                $"Deferred private requirements retained: {deferredRequirements:N0}\n" +
                $"Managed resolver requests: {context.ManagedResolverRequests.Count:N0}\n" +
                $"Host loads: {context.HostLoads.Count:N0}\n" +
                $"Private loads: {context.PrivateLoads.Count:N0}\n" +
                $"Native load attempts: {context.NativeLoadAttempts.Count:N0}\n" +
                "0Harmony loaded: NO\n" +
                "Game entry point/member invoked: NO");
        }
        catch (Exception ex)
        {
            return Fail(ControlledManagedInitializationGate.ProvenLoadStateReplay, stage, ex);
        }
    }

    public ControlledManagedInitializationGateResult RunDeferredModuleInitialization()
    {
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var preflight = RequirePreflight();
            _ = RequireReplay();
            var context = RequireLoadContext();

            stage = "target hash recheck";
            VerifyFileLength(preflight.Target.PreparedPath, preflight.Target.Plan.Length, "prepared initialization target");
            var targetSha1 = ComputeSha1Hex(preflight.Target.PreparedPath);
            if (!targetSha1.Equals(preflight.Target.Plan.Sha1Hex, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Step 24 target SHA-1 changed immediately before automatic initialization.");

            var managedRequestsBefore = context.ManagedResolverRequests.Count;
            var privateLoadsBefore = context.PrivateLoads.Count;
            var hostLoadsBefore = context.HostLoads.Count;
            var nativeAttemptsBefore = context.NativeLoadAttempts.Count;

            stage = "0Harmony LoadFromStream";
            context.AllowedInitializerAssemblyFullName = preflight.Target.Plan.AssemblyFullName;
            var targetAssembly = context.LoadPrepared(preflight.Target, allowInitializer: true, explicitReason: "Step 24 deferred initialization target");
            var actualIdentity = targetAssembly.GetName().FullName ?? targetAssembly.GetName().Name ?? string.Empty;
            if (!actualIdentity.Equals(preflight.Target.Plan.AssemblyFullName, StringComparison.Ordinal))
                throw new InvalidDataException("Step 24 loaded an unexpected deferred target identity.");
            if (!ReferenceEquals(AssemblyLoadContext.GetLoadContext(targetAssembly), context))
                throw new InvalidDataException("Step 24 deferred target escaped the dedicated load context.");

            stage = "module constructor completion barrier";
            RuntimeHelpers.RunModuleConstructor(targetAssembly.ManifestModule.ModuleHandle);

            stage = "initializer isolation checks";
            if (context.NativeLoadAttempts.Count != nativeAttemptsBefore)
                throw new DllNotFoundException("The 0Harmony module initializer attempted native-library resolution: " + string.Join(" | ", context.NativeLoadAttempts.Skip(nativeAttemptsBefore)));
            if (context.RejectedManagedRequests.Count != 0)
                throw new FileLoadException("The 0Harmony module initializer triggered an unplanned managed request: " + string.Join(" | ", context.RejectedManagedRequests));

            var unexpectedInitializerLoads = context.Assemblies
                .Select(assembly => assembly.GetName().FullName ?? assembly.GetName().Name ?? string.Empty)
                .Where(fullName => preflight.PreparedAssemblies.Any(item =>
                    item.ModuleInitializerCount > 0 &&
                    !item.Plan.AssemblyFullName.Equals(preflight.Target.Plan.AssemblyFullName, StringComparison.Ordinal) &&
                    item.Plan.AssemblyFullName.Equals(fullName, StringComparison.Ordinal)))
                .ToArray();
            if (unexpectedInitializerLoads.Length != 0)
                throw new InvalidDataException("An untargeted initializer-bearing dependency entered the CLR: " + string.Join(" | ", unexpectedInitializerLoads));

            var postSha1 = ComputeSha1Hex(preflight.Target.PreparedPath);
            if (!postSha1.Equals(targetSha1, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("0Harmony prepared bytes changed across module initialization.");

            _initialization = new DeferredInitializationSnapshot(
                actualIdentity,
                postSha1,
                context.ManagedResolverRequests.Count - managedRequestsBefore,
                context.PrivateLoads.Count - privateLoadsBefore,
                context.HostLoads.Count - hostLoadsBefore,
                context.NativeLoadAttempts.Count - nativeAttemptsBefore);

            return Pass(
                ControlledManagedInitializationGate.DeferredModuleInitialization,
                "CONTROLLED 0HARMONY MODULE INITIALIZATION SUCCEEDED.\n" +
                $"Loaded identity: {actualIdentity}\n" +
                "Load context: " + LoadContextName + "\n" +
                "RuntimeHelpers.RunModuleConstructor completion barrier: PASS\n" +
                $"Managed resolver requests during target load/initializer: {_initialization.ManagedResolverRequestsDuringInitialization:N0}\n" +
                $"Private loads during target load/initializer: {_initialization.PrivateLoadsDuringInitialization:N0}\n" +
                $"Host loads during target load/initializer: {_initialization.HostLoadsDuringInitialization:N0}\n" +
                $"Native load attempts during target load/initializer: {_initialization.NativeLoadAttemptsDuringInitialization:N0}\n" +
                "Explicit Harmony API invoked: NO\n" +
                "Game type/member reflected or invoked: NO\n" +
                "Godot/game startup requested: NO");
        }
        catch (Exception ex)
        {
            return Fail(ControlledManagedInitializationGate.DeferredModuleInitialization, stage, ex);
        }
    }

    public async Task<ControlledManagedInitializationGateResult> RunPostInitializationAuditAsync(
        IProgress<ControlledManagedInitializationProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var preflight = RequirePreflight();
            var replay = RequireReplay();
            var initialization = RequireInitialization();
            var context = RequireLoadContext();

            stage = "runtime plan rehash";
            var planSha256 = await ComputeSha256HexAsync(_planPath, cancellationToken).ConfigureAwait(false);
            if (!planSha256.Equals(preflight.PlanSha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Step 24 runtime-binding plan changed during controlled initialization.");

            stage = "prepared/live byte audit";
            var verified = 0;
            foreach (var item in preflight.PreparedAssemblies)
            {
                cancellationToken.ThrowIfCancellationRequested();
                VerifyFileLength(item.PreparedPath, item.Plan.Length, "prepared post-initialization");
                VerifyFileLength(item.LivePath, item.Plan.Length, "live post-initialization");
                var preparedSha1 = await ComputeSha1HexAsync(item.PreparedPath, cancellationToken).ConfigureAwait(false);
                var liveSha1 = await ComputeSha1HexAsync(item.LivePath, cancellationToken).ConfigureAwait(false);
                if (!preparedSha1.Equals(item.Plan.Sha1Hex, StringComparison.OrdinalIgnoreCase) ||
                    !liveSha1.Equals(item.Plan.Sha1Hex, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidDataException("Step 24 prepared/live byte identity changed during controlled initialization: " + item.Plan.RelativePath);
                }
                verified++;
                progress?.Report(new ControlledManagedInitializationProgress(
                    ControlledManagedInitializationGate.PostInitializationAudit,
                    verified,
                    preflight.PreparedAssemblies.Length,
                    item.Plan.RelativePath,
                    "Re-hashing prepared/live bytes after the 0Harmony module constructor boundary…"));
            }

            stage = "OfflineReady postcondition";
            var offline = await _offlineInspection.RunAsync(null, cancellationToken).ConfigureAwait(false);
            if (!offline.Success || !offline.ExactManagedTreeVerified || offline.InstalledManifestId != preflight.Plan.ManifestId)
                throw new InvalidDataException(offline.Error ?? "OfflineReady exact-tree verification failed after Step 24 controlled initialization.");

            stage = "private context membership audit";
            var expected = preflight.PreparedAssemblies
                .Where(item => item.ModuleInitializerCount == 0 || item.Plan.AssemblyFullName.Equals(preflight.Target.Plan.AssemblyFullName, StringComparison.Ordinal))
                .Select(item => item.Plan.AssemblyFullName)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            var actual = context.Assemblies
                .Select(assembly => assembly.GetName().FullName ?? assembly.GetName().Name ?? string.Empty)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            if (!actual.SequenceEqual(expected, StringComparer.Ordinal))
            {
                throw new InvalidDataException(
                    "Step 24 post-initialization private context differs from the expected Step 23 closure + exactly 0Harmony. " +
                    $"Expected [{string.Join(" | ", expected)}], actual [{string.Join(" | ", actual)}].");
            }
            if (context.NativeLoadAttempts.Count != 0)
                throw new DllNotFoundException("Step 24 observed native-library resolution: " + string.Join(" | ", context.NativeLoadAttempts));
            if (context.RejectedManagedRequests.Count != 0)
                throw new FileLoadException("Step 24 observed rejected/unplanned managed requests: " + string.Join(" | ", context.RejectedManagedRequests));

            return Pass(
                ControlledManagedInitializationGate.PostInitializationAudit,
                "Step 24 post-initialization isolation audit passed.\n" +
                $"Loaded initialization target: {initialization.AssemblyFullName}\n" +
                $"Prepared/live assemblies re-hashed: {verified:N0}/{preflight.PreparedAssemblies.Length:N0}\n" +
                $"Runtime plan SHA-256 unchanged: {planSha256}\n" +
                $"Private context: {actual.Length:N0}/{expected.Length:N0} expected assemblies\n" +
                $"Step 23 initializer-free baseline assemblies retained: {replay.ExpectedPrivateAssemblies:N0}\n" +
                $"Native load attempts: {context.NativeLoadAttempts.Count:N0}\n" +
                $"Rejected/unplanned managed requests: {context.RejectedManagedRequests.Count:N0}\n" +
                "Post-initialization OfflineReady exact-tree verification: YES\n" +
                "Trusted Step 12 managed install unchanged: YES\n" +
                "Prepared Step 21/22 bytes unchanged: YES\n" +
                "Explicit Harmony patching/API invocation: NO\n" +
                "Game entry point/type/member invocation: NO\n" +
                "Godot/game initialization: NO\n" +
                "Native game library loaded by Step 24: NO\n" +
                "Process note: the Step 24 private managed context remains resident until process exit; force-quit before rerunning earlier fresh-process CLR-load regressions.");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return Fail(ControlledManagedInitializationGate.PostInitializationAudit, stage, ex);
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;
        ReleaseLoadContext();
        _step23Preflight.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    private void ReleaseLoadContext()
    {
        var context = _loadContext;
        _loadContext = null;
        _replay = null;
        _initialization = null;
        if (context is not null && context.IsCollectible)
            context.Unload();
    }

    private void EnsureFreshProcess()
    {
        var matches = AppDomain.CurrentDomain.GetAssemblies()
            .Where(assembly => _freshProcessAssemblyNames.Contains(assembly.GetName().Name ?? string.Empty))
            .Select(assembly => $"{assembly.GetName().FullName} @ {AssemblyLoadContext.GetLoadContext(assembly)?.Name ?? "<unknown-context>"}")
            .OrderBy(value => value, StringComparer.Ordinal)
            .ToArray();
        if (matches.Length != 0)
            throw new InvalidDataException("Step 24 requires a fresh process; a game/Harmony assembly is already loaded: " + string.Join(" | ", matches));
    }

    private static PreparedMetadataSnapshot ReadPreparedMetadata(string path, bool includeInitializerCallGraph, string targetSimpleName)
    {
        using var resolver = new Step24MetadataOnlyResolver(path);
        using var module = ModuleDefinition.ReadModule(path, new ReaderParameters
        {
            InMemory = true,
            ReadSymbols = false,
            ReadingMode = ReadingMode.Deferred,
            AssemblyResolver = resolver,
            MetadataResolver = resolver,
        });
        if (module.Assembly?.Name is null)
            throw new BadImageFormatException("Managed assembly manifest missing: " + path);

        var allTypes = EnumerateTypes(module.Types).ToArray();
        var typesByFullName = allTypes.ToDictionary(type => type.FullName, StringComparer.Ordinal);
        var moduleInitializers = module.Types
            .Where(type => type.Name.Equals("<Module>", StringComparison.Ordinal))
            .SelectMany(type => type.Methods)
            .Where(method => method.Name.Equals(".cctor", StringComparison.Ordinal) && method.IsStatic && method.HasBody)
            .ToArray();
        if (!includeInitializerCallGraph || moduleInitializers.Length == 0)
        {
            var directAudits = moduleInitializers.Select(FormatMethodAudit).ToArray();
            return new PreparedMetadataSnapshot(moduleInitializers.Length, moduleInitializers.Length, directAudits, 0, []);
        }

        var methodsByToken = allTypes
            .SelectMany(type => type.Methods)
            .Where(method => method.HasBody)
            .ToDictionary(method => method.MetadataToken.ToInt32());
        var queue = new Queue<MethodDefinition>(moduleInitializers);
        var visited = new HashSet<int>();
        var automaticInitializers = moduleInitializers
            .ToDictionary(method => method.MetadataToken.ToInt32());
        var hazards = new SortedSet<string>(StringComparer.Ordinal);

        while (queue.Count > 0)
        {
            var method = queue.Dequeue();
            var token = method.MetadataToken.ToInt32();
            if (!visited.Add(token))
                continue;
            if (visited.Count > 512)
            {
                hazards.Add("Initializer call graph exceeded the Step 24.0 bound of 512 same-assembly methods.");
                break;
            }

            if (method.IsPInvokeImpl || method.PInvokeInfo is not null)
                hazards.Add($"P/Invoke reachable: {method.FullName}");

            foreach (var instruction in method.Body.Instructions)
            {
                if (instruction.OpCode.Code == Code.Calli)
                    hazards.Add($"calli reachable: {method.FullName} at IL_{instruction.Offset:X4}");
                if (instruction.OpCode.Code is Code.Ldftn or Code.Ldvirtftn)
                    hazards.Add($"indirect function/delegate target reachable: {method.FullName} at IL_{instruction.Offset:X4}");

                if (instruction.Operand is FieldReference field)
                {
                    QueueAutomaticTypeInitializer(field.DeclaringType, typesByFullName, automaticInitializers, visited, queue);
                }

                if (instruction.Operand is not MethodReference called)
                    continue;

                // A call/newobj against a type with its own .cctor can trigger that type initializer
                // before the called member runs. Audit that implicit automatic-execution edge too.
                QueueAutomaticTypeInitializer(called.DeclaringType, typesByFullName, automaticInitializers, visited, queue);

                var scopeName = GetMethodScopeName(called);
                if (IsProhibitedDynamicOrNativeApi(called, scopeName, targetSimpleName))
                    hazards.Add($"Prohibited API reachable: {method.FullName} -> {called.FullName} [{scopeName}]");

                MethodDefinition? resolved = null;
                if (scopeName.Equals(module.Assembly.Name.Name, StringComparison.OrdinalIgnoreCase))
                {
                    // Do not call MethodReference.Resolve() here. Cecil's resolver may walk external
                    // type/base/member metadata while resolving an otherwise local MemberRef. On the
                    // physical Step 24.0.2 target that caused Gate A to abort while trying to resolve
                    // GodotSharp even though Gate A is supposed to be a self-contained metadata audit.
                    // Resolve only from definitions already present in this module. If a same-assembly
                    // reference cannot be matched unambiguously from local metadata, fail closed below.
                    resolved = ResolveSameAssemblyMethodFromLocalMetadata(module, called, typesByFullName);
                    if (resolved is null)
                        hazards.Add($"Unresolved same-assembly call (local metadata only): {method.FullName} -> {called.FullName}");
                }

                if (resolved is not null)
                {
                    // P/Invoke/extern stubs intentionally have no managed MethodBody. They therefore
                    // cannot be discovered by the body-bearing traversal set below. Inspect the
                    // resolved same-assembly target before the HasBody filter so a direct call to a
                    // native stub (including one reached through an implicit type initializer) is
                    // rejected during metadata-only Gate A rather than being silently skipped.
                    if (resolved.IsPInvokeImpl || resolved.PInvokeInfo is not null)
                    {
                        hazards.Add($"P/Invoke reachable: {resolved.FullName}");
                    }
                    else if (!resolved.HasBody)
                    {
                        // Step 24.0 does not guess what an extern/runtime/abstract same-assembly
                        // execution edge would do. Any bodyless reachable target is outside the
                        // bounded managed-IL closure and therefore fails closed.
                        hazards.Add($"Same-assembly method without managed IL body reachable: {method.FullName} -> {resolved.FullName}");
                    }
                    else if (methodsByToken.ContainsKey(resolved.MetadataToken.ToInt32()))
                    {
                        queue.Enqueue(resolved);
                    }
                }
            }
        }

        var audits = automaticInitializers.Values
            .OrderBy(method => method.MetadataToken.ToInt32())
            .Select(FormatMethodAudit)
            .ToArray();
        return new PreparedMetadataSnapshot(moduleInitializers.Length, automaticInitializers.Count, audits, visited.Count, hazards.ToArray());
    }


    private static MethodDefinition? ResolveSameAssemblyMethodFromLocalMetadata(
        ModuleDefinition module,
        MethodReference called,
        IReadOnlyDictionary<string, TypeDefinition> typesByFullName)
    {
        // MethodDef operands are already the exact local definition and require no resolver.
        if (called is MethodDefinition direct && ReferenceEquals(direct.Module, module))
            return direct;

        var elementMethod = called.GetElementMethod();
        if (elementMethod is MethodDefinition elementDefinition && ReferenceEquals(elementDefinition.Module, module))
            return elementDefinition;

        // Do not call ModuleDefinition.LookupToken for a MethodReference here. MethodDef operands
        // were already handled above; MemberRef/MethodSpec operands are matched deterministically
        // against definitions already materialized from this module. This keeps the audit independent
        // from Cecil token-resolution machinery and therefore from external assembly metadata.
        var declaringTypeName = GetElementTypeWithoutResolution(called.DeclaringType).FullName;
        if (!typesByFullName.TryGetValue(declaringTypeName, out var declaringType))
            return null;

        var reference = elementMethod;
        var candidates = declaringType.Methods
            .Where(candidate =>
                candidate.Name.Equals(reference.Name, StringComparison.Ordinal) &&
                candidate.Parameters.Count == reference.Parameters.Count &&
                candidate.GenericParameters.Count == reference.GenericParameters.Count)
            .ToArray();
        if (candidates.Length == 1)
            return candidates[0];

        // When overloads exist, use the metadata signature text only. This is deliberately
        // resolver-free. If generic substitution prevents an unambiguous match, return null and
        // let Gate A fail closed rather than resolving external assemblies or guessing.
        var exact = candidates
            .Where(candidate => MetadataSignatureEquals(candidate, reference))
            .ToArray();
        return exact.Length == 1 ? exact[0] : null;
    }

    private static TypeReference GetElementTypeWithoutResolution(TypeReference type)
    {
        while (type is TypeSpecification specification)
            type = specification.ElementType;
        return type;
    }

    private static bool MetadataSignatureEquals(MethodReference definition, MethodReference reference)
    {
        if (!definition.ReturnType.FullName.Equals(reference.ReturnType.FullName, StringComparison.Ordinal))
            return false;
        for (var i = 0; i < definition.Parameters.Count; i++)
        {
            if (!definition.Parameters[i].ParameterType.FullName.Equals(reference.Parameters[i].ParameterType.FullName, StringComparison.Ordinal))
                return false;
        }
        return true;
    }

    private static void QueueAutomaticTypeInitializer(
        TypeReference declaringType,
        IReadOnlyDictionary<string, TypeDefinition> typesByFullName,
        IDictionary<int, MethodDefinition> automaticInitializers,
        ISet<int> visited,
        Queue<MethodDefinition> queue)
    {
        var elementName = GetElementTypeWithoutResolution(declaringType).FullName;
        if (!typesByFullName.TryGetValue(elementName, out var type))
            return;

        var initializer = type.Methods.FirstOrDefault(method =>
            method.Name.Equals(".cctor", StringComparison.Ordinal) && method.IsStatic && method.HasBody);
        if (initializer is null)
            return;

        var token = initializer.MetadataToken.ToInt32();
        automaticInitializers[token] = initializer;
        if (!visited.Contains(token))
            queue.Enqueue(initializer);
    }

    private static bool IsProhibitedDynamicOrNativeApi(MethodReference method, string scopeName, string targetSimpleName)
    {
        var type = method.DeclaringType.FullName;
        var name = method.Name;

        if (type.Equals("System.Runtime.InteropServices.NativeLibrary", StringComparison.Ordinal) &&
            (name.Equals("Load", StringComparison.Ordinal) || name.Equals("TryLoad", StringComparison.Ordinal) || name.Equals("GetExport", StringComparison.Ordinal) || name.Equals("TryGetExport", StringComparison.Ordinal)))
            return true;
        if (type.StartsWith("System.Reflection.Emit.", StringComparison.Ordinal))
            return true;
        if (type.Equals("System.Activator", StringComparison.Ordinal))
            return true;
        if (type.Equals("System.Delegate", StringComparison.Ordinal) && name.Equals("DynamicInvoke", StringComparison.Ordinal))
            return true;
        if ((type.Equals("System.Reflection.MethodBase", StringComparison.Ordinal) ||
             type.Equals("System.Reflection.MethodInfo", StringComparison.Ordinal) ||
             type.Equals("System.Reflection.ConstructorInfo", StringComparison.Ordinal)) &&
            name.Equals("Invoke", StringComparison.Ordinal))
            return true;
        if (type.Equals("System.Reflection.Assembly", StringComparison.Ordinal) && name.StartsWith("Load", StringComparison.Ordinal))
            return true;
        if (type.Equals("System.Runtime.Loader.AssemblyLoadContext", StringComparison.Ordinal) && name.StartsWith("Load", StringComparison.Ordinal))
            return true;
        if (type.Equals("System.Runtime.InteropServices.Marshal", StringComparison.Ordinal) &&
            (name.Contains("FunctionPointer", StringComparison.Ordinal) || name.Contains("Delegate", StringComparison.Ordinal)))
            return true;
        if (type.Equals("System.Runtime.CompilerServices.RuntimeHelpers", StringComparison.Ordinal) &&
            (name.Equals("RunClassConstructor", StringComparison.Ordinal) || name.Equals("RunModuleConstructor", StringComparison.Ordinal)))
            return true;

        // A non-framework external call is an unmeasured execution edge for Step 24.0.
        if (!scopeName.Equals(targetSimpleName, StringComparison.OrdinalIgnoreCase) && !IsHostFrameworkContractName(scopeName))
            return true;

        return false;
    }

    private static string GetMethodScopeName(MethodReference method)
    {
        var scope = method.DeclaringType.Scope;
        return scope switch
        {
            AssemblyNameReference assembly => assembly.Name,
            ModuleDefinition module => module.Assembly?.Name?.Name ?? module.Name,
            ModuleReference moduleReference => moduleReference.Name,
            _ => scope?.Name ?? string.Empty,
        };
    }

    private static string FormatMethodAudit(MethodDefinition method)
    {
        const int maxInstructions = 160;
        var rendered = method.Body.Instructions
            .Take(maxInstructions)
            .Select(instruction => $"IL_{instruction.Offset:X4}: {instruction.OpCode.Code} {FormatInstructionOperand(instruction.Operand)}".TrimEnd())
            .ToArray();
        var suffix = method.Body.Instructions.Count > maxInstructions
            ? $" | ... {method.Body.Instructions.Count - maxInstructions} more instruction(s)"
            : string.Empty;
        return $"method={method.FullName}; token=0x{method.MetadataToken.ToInt32():X8}; instructions={method.Body.Instructions.Count}; handlers={method.Body.ExceptionHandlers.Count}; locals={method.Body.Variables.Count}; IL=[{string.Join(" | ", rendered)}]{suffix}";
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

    private static PlannedBindingRequirement[] BuildBindingRequirements(RuntimeFrameworkBindingPlanDocument plan)
    {
        var requirements = new List<PlannedBindingRequirement>();
        foreach (var group in plan.Edges.GroupBy(edge => edge.RequestedFullName, StringComparer.Ordinal))
        {
            var normalized = group.Select(edge =>
            {
                if (edge.BindingKind.Equals("HostFramework", StringComparison.Ordinal))
                    return new PlannedBindingRequirement(edge.RequestedFullName, PlannedBindingKind.HostFramework, edge.Target);
                if (edge.BindingKind.Equals("WorkspaceExact", StringComparison.Ordinal) || edge.BindingKind.Equals("WorkspaceVersionUnified", StringComparison.Ordinal))
                    return new PlannedBindingRequirement(edge.RequestedFullName, PlannedBindingKind.PrivatePrepared, edge.Target);
                throw new InvalidDataException($"Step 24 cannot execute plan edge kind '{edge.BindingKind}' for {edge.RequestedFullName}.");
            }).Distinct().ToArray();

            if (normalized.Length != 1)
                throw new InvalidDataException("Step 24 plan has inconsistent targets for " + group.Key + ".");
            requirements.Add(normalized[0]);
        }
        return requirements.OrderBy(item => item.RequestedFullName, StringComparer.Ordinal).ToArray();
    }

    private static PreparedAssemblySnapshot FindPreparedByTarget(IEnumerable<PreparedAssemblySnapshot> prepared, string fullName)
        => prepared.Single(item => item.Plan.AssemblyFullName.Equals(fullName, StringComparison.Ordinal));

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
            throw new FileNotFoundException($"Step 24 {scope} file is missing.", path);
        var actual = new FileInfo(path).Length;
        if (actual != expected)
            throw new InvalidDataException($"Step 24 {scope} file length mismatch for {path}: {actual} != {expected}.");
    }

    private static async Task<string> ComputeSha1HexAsync(string path, CancellationToken cancellationToken)
    {
        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 64 * 1024, FileOptions.Asynchronous | FileOptions.SequentialScan);
        using var sha1 = SHA1.Create();
        return Convert.ToHexString(await sha1.ComputeHashAsync(stream, cancellationToken).ConfigureAwait(false)).ToLowerInvariant();
    }

    private static string ComputeSha1Hex(string path)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Convert.ToHexString(SHA1.HashData(stream)).ToLowerInvariant();
    }

    private static async Task<string> ComputeSha256HexAsync(string path, CancellationToken cancellationToken)
    {
        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 64 * 1024, FileOptions.Asynchronous | FileOptions.SequentialScan);
        using var sha256 = SHA256.Create();
        return Convert.ToHexString(await sha256.ComputeHashAsync(stream, cancellationToken).ConfigureAwait(false)).ToLowerInvariant();
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

    private InitializationPreflightSnapshot RequirePreflight()
        => _preflight ?? throw new InvalidOperationException("Step 24 Gate A must pass before Gate B.");

    private ProvenLoadReplaySnapshot RequireReplay()
        => _replay ?? throw new InvalidOperationException("Step 24 Gate B must pass before Gate C.");

    private DeferredInitializationSnapshot RequireInitialization()
        => _initialization ?? throw new InvalidOperationException("Step 24 Gate C must pass before Gate D.");

    private Step24LoadContext RequireLoadContext()
        => _loadContext ?? throw new InvalidOperationException("Step 24 dedicated load context is unavailable.");

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(ControlledManagedInitialization));
    }

    private static ControlledManagedInitializationGateResult Pass(ControlledManagedInitializationGate gate, string detail)
        => new(gate, true, detail);

    private static ControlledManagedInitializationGateResult Fail(ControlledManagedInitializationGate gate, string stage, Exception ex)
        => new(gate, false, $"Stage: {stage}\n{ex}");

    private sealed class Step24MetadataOnlyResolver(string auditedPath) : IAssemblyResolver, IMetadataResolver
    {
        public AssemblyDefinition Resolve(AssemblyNameReference name)
            => throw new InvalidOperationException(
                $"Step 24 Gate A metadata-only audit attempted forbidden external assembly resolution while reading '{auditedPath}': {name.FullName}");

        public AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
            => Resolve(name);

        TypeDefinition IMetadataResolver.Resolve(TypeReference type)
            => throw new InvalidOperationException(
                $"Step 24 Gate A metadata-only audit attempted forbidden type resolution while reading '{auditedPath}': {type.FullName}");

        FieldDefinition IMetadataResolver.Resolve(FieldReference field)
            => throw new InvalidOperationException(
                $"Step 24 Gate A metadata-only audit attempted forbidden field resolution while reading '{auditedPath}': {field.FullName}");

        MethodDefinition IMetadataResolver.Resolve(MethodReference method)
            => throw new InvalidOperationException(
                $"Step 24 Gate A metadata-only audit attempted forbidden method resolution while reading '{auditedPath}': {method.FullName}");

        public void Dispose() { }
    }

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

    private sealed record PlannedBindingRequirement(string RequestedFullName, PlannedBindingKind Kind, string ExpectedTargetFullName);

    private sealed record PreparedMetadataSnapshot(
        int ModuleInitializerCount,
        int AutomaticInitializerCount,
        IReadOnlyList<string> AutomaticInitializerAudits,
        int InitializerReachableMethods,
        IReadOnlyList<string> InitializerHazards);

    private sealed record PreparedAssemblySnapshot(
        RuntimeBindingPreparedAssembly Plan,
        string PreparedPath,
        string LivePath,
        AssemblyName AssemblyName,
        int ModuleInitializerCount,
        int AutomaticInitializerCount,
        IReadOnlyList<string> AutomaticInitializerAudits,
        int InitializerReachableMethods,
        IReadOnlyList<string> InitializerHazards);

    private sealed record InitializationPreflightSnapshot(
        RuntimeFrameworkBindingPlanDocument Plan,
        string PlanSha256,
        string ManagedRoot,
        PreparedAssemblySnapshot[] PreparedAssemblies,
        PreparedAssemblySnapshot Primary,
        PreparedAssemblySnapshot Target,
        SteamOfflineInstallResult Offline);

    private sealed record ProvenLoadReplaySnapshot(
        string PrimaryAssemblyFullName,
        int ExpectedPrivateAssemblies,
        int DeferredPrivateRequirements,
        int ManagedResolverRequests,
        int HostLoads,
        int PrivateLoads,
        int NativeLoadAttempts);

    private sealed record DeferredInitializationSnapshot(
        string AssemblyFullName,
        string PreparedSha1,
        int ManagedResolverRequestsDuringInitialization,
        int PrivateLoadsDuringInitialization,
        int HostLoadsDuringInitialization,
        int NativeLoadAttemptsDuringInitialization);

    private sealed class Step24LoadContext : AssemblyLoadContext
    {
        private readonly IReadOnlyDictionary<string, PreparedAssemblySnapshot> _privateBySimpleName;
        private readonly RuntimeBindingHostFramework[] _hostBindings;

        public Step24LoadContext(
            string name,
            RuntimeFrameworkBindingPlanDocument plan,
            IReadOnlyList<PreparedAssemblySnapshot> preparedAssemblies,
            bool isCollectible)
            : base(name, isCollectible)
        {
            var privateBySimpleName = new Dictionary<string, PreparedAssemblySnapshot>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in preparedAssemblies)
            {
                var simple = item.AssemblyName.Name ?? throw new InvalidDataException("Prepared assembly identity has no simple name: " + item.Plan.AssemblyFullName);
                if (!privateBySimpleName.TryAdd(simple, item))
                    throw new InvalidDataException("Step 24 resolver received duplicate prepared simple name '" + simple + "'.");
            }
            _privateBySimpleName = privateBySimpleName;
            _hostBindings = plan.HostFrameworkBindings;
        }

        public string? AllowedInitializerAssemblyFullName { get; set; }
        public List<string> ManagedResolverRequests { get; } = [];
        public List<string> PrivateLoads { get; } = [];
        public List<string> HostLoads { get; } = [];
        public List<string> RejectedManagedRequests { get; } = [];
        public List<string> NativeLoadAttempts { get; } = [];

        [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Step 24 loads only receipt-verified prepared IL selected by the already-proven Step 21/22 plan.")]
        public Assembly LoadPrepared(PreparedAssemblySnapshot prepared, bool allowInitializer, string explicitReason)
        {
            if (prepared.ModuleInitializerCount > 0 &&
                (!allowInitializer || !prepared.Plan.AssemblyFullName.Equals(AllowedInitializerAssemblyFullName, StringComparison.Ordinal)))
            {
                throw new FileLoadException("Step 24 refuses an initializer-bearing prepared assembly outside the explicit Gate C target: " + prepared.Plan.AssemblyFullName);
            }

            var hash = ComputeSha1Hex(prepared.PreparedPath);
            if (!hash.Equals(prepared.Plan.Sha1Hex, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Step 24 prepared SHA-1 changed immediately before load: " + prepared.Plan.RelativePath);
            using var stream = new FileStream(prepared.PreparedPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var loaded = LoadFromStream(stream);
            PrivateLoads.Add($"{explicitReason}: {prepared.Plan.AssemblyFullName} => {loaded.GetName().FullName}");
            return loaded;
        }

        public Assembly ResolvePlanned(AssemblyName assemblyName)
            => Load(assemblyName) ?? throw new FileLoadException("Step 24 planned resolver returned null for " + (assemblyName.FullName ?? assemblyName.Name));

        [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Step 24 resolves only exact receipt-verified prepared assemblies and exact persisted host bindings.")]
        protected override Assembly? Load(AssemblyName assemblyName)
        {
            var requestedFullName = assemblyName.FullName ?? assemblyName.Name ?? "<unknown>";
            ManagedResolverRequests.Add(requestedFullName);

            if (assemblyName.Name is null)
                return Reject(requestedFullName, "assembly request has no simple name");

            if (_privateBySimpleName.TryGetValue(assemblyName.Name, out var privateAssembly))
            {
                if (privateAssembly.ModuleInitializerCount > 0 &&
                    !privateAssembly.Plan.AssemblyFullName.Equals(AllowedInitializerAssemblyFullName, StringComparison.Ordinal))
                {
                    return Reject(requestedFullName, "initializer-bearing private dependency is not the explicit Step 24 Gate C target");
                }
                if (!SameIdentityIgnoringVersion(assemblyName, privateAssembly.AssemblyName) ||
                    (privateAssembly.AssemblyName.Version ?? ZeroVersion).CompareTo(assemblyName.Version ?? ZeroVersion) < 0)
                {
                    return Reject(requestedFullName, "verified private candidate identity/version is incompatible: " + privateAssembly.Plan.AssemblyFullName);
                }

                var alreadyLoaded = Assemblies.FirstOrDefault(existing =>
                    string.Equals(existing.GetName().Name, privateAssembly.AssemblyName.Name, StringComparison.OrdinalIgnoreCase));
                if (alreadyLoaded is not null)
                    return alreadyLoaded;

                return LoadPrepared(privateAssembly, privateAssembly.ModuleInitializerCount == 0 ||
                    privateAssembly.Plan.AssemblyFullName.Equals(AllowedInitializerAssemblyFullName, StringComparison.Ordinal), "resolver");
            }

            var hostMatches = _hostBindings
                .Where(binding => ExactRequestedIdentity(assemblyName, new AssemblyName(binding.RequestedFullName)))
                .ToArray();
            if (hostMatches.Length == 0)
                return Reject(requestedFullName, "request is neither a prepared private assembly nor an exact planned host-framework binding");

            var allowedActual = hostMatches.Select(binding => binding.ActualFullName).Distinct(StringComparer.Ordinal).ToHashSet(StringComparer.Ordinal);
            var hostAssembly = AssemblyLoadContext.Default.LoadFromAssemblyName(assemblyName);
            var actualFullName = hostAssembly.GetName().FullName ?? hostAssembly.GetName().Name ?? string.Empty;
            if (!allowedActual.Contains(actualFullName))
                throw new FileLoadException($"Step 24 host binding drift for '{requestedFullName}'. Planned: {string.Join(" | ", allowedActual)}; actual: {actualFullName}.");

            HostLoads.Add($"{requestedFullName} => {actualFullName}");
            return hostAssembly;
        }

        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            NativeLoadAttempts.Add(unmanagedDllName);
            throw new DllNotFoundException($"Step 24 controlled-initialization boundary refuses native library resolution for '{unmanagedDllName}'.");
        }

        private Assembly? Reject(string requestedFullName, string reason)
        {
            var detail = requestedFullName + " — " + reason;
            RejectedManagedRequests.Add(detail);
            throw new FileLoadException("Step 24 strict managed resolver rejected an unplanned request: " + detail);
        }
    }
}
