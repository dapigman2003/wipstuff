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
/// Step 26 boundary. Replays the physically proven Step 25 Harmony construction state in a fresh
/// private AssemblyLoadContext, then resolves the exact Harmony.CreateProcessor(MethodBase) /
/// PatchProcessor constructor surface, explicitly completes only the measured PatchProcessor type
/// initializer, resolves one launcher-owned inert probe MethodInfo, and constructs one empty
/// PatchProcessor. Patch(), patch registration, StS2 member reflection/invocation, Godot startup, and
/// native game libraries remain forbidden.
/// </summary>
public sealed class ControlledHarmonyProcessorCreation : IDisposable
{
    public const string TargetSimpleName = "0Harmony";
    public static readonly Version TargetVersion = new(2, 4, 2, 0);
    public const string LoadContextName = "StS2Launcher-Step26-HarmonyProcessorCreation";
    public const string HarmonyTypeFullName = "HarmonyLib.Harmony";
    public const string HarmonyId = "com.community.sts2launcher.step25.probe";
    public const string PatchProcessorTypeFullName = "HarmonyLib.PatchProcessor";

    // Physical Step 24.0.4 / 0.0.77 measured these seven conservative dispatch findings in the
    // exact receipt-backed 0Harmony 2.4.2 automatic-initialization closure. They are not a general
    // allowlist. Step 24.0.5 may conditionally classify exactly this fingerprint as dormant only
    // when the process is in an explicitly inert MonoMod logging state. Any changed/additional
    // finding remains fail-closed before Gate B.
    internal static readonly string[] ObservedMonoModLoggingDispatchHazards =
    [
        "Same-assembly method without managed IL body reachable: System.Boolean MonoMod.Logs.DebugFormatter::TryFormatInto(T&,System.Object,System.Span`1<System.Char>,System.Int32&) -> System.Boolean MonoMod.Logs.IDebugFormattable::TryFormatInto(System.Span`1<System.Char>,System.Int32&)",
        "Same-assembly method without managed IL body reachable: System.Void MonoMod.Logs.DebugLog/LogMessage::ReportTo(MonoMod.Logs.DebugLog/OnLogMessage) -> System.Void MonoMod.Logs.DebugLog/OnLogMessage::Invoke(System.String,System.DateTime,MonoMod.Logs.LogLevel,System.String)",
        "Same-assembly method without managed IL body reachable: System.Void MonoMod.Logs.DebugLog/LogMessage::ReportTo(MonoMod.Logs.DebugLog/OnLogMessageDetailed) -> System.Void MonoMod.Logs.DebugLog/OnLogMessageDetailed::Invoke(System.String,System.DateTime,MonoMod.Logs.LogLevel,System.String,System.ReadOnlyMemory`1<MonoMod.Logs.MessageHole>)",
        "Same-assembly method without managed IL body reachable: System.Void MonoMod.Logs.DebugLog::TryInitializeLogToFile(System.String,System.String[],MonoMod.Logs.LogLevelFilter) -> System.Void MonoMod.Logs.DebugLog/OnLogMessage::.ctor(System.Object,System.IntPtr)",
        "Same-assembly method without managed IL body reachable: System.Void MonoMod.Logs.DebugLog::TryInitializeMemoryLog(MonoMod.Logs.LogLevelFilter) -> System.Void MonoMod.Logs.DebugLog/OnLogMessage::.ctor(System.Object,System.IntPtr)",
        "indirect function/delegate target reachable: System.Void MonoMod.Logs.DebugLog::TryInitializeLogToFile(System.String,System.String[],MonoMod.Logs.LogLevelFilter) at IL_007C",
        "indirect function/delegate target reachable: System.Void MonoMod.Logs.DebugLog::TryInitializeMemoryLog(MonoMod.Logs.LogLevelFilter) at IL_002F",
    ];

    private static readonly string[] MonoModLoggingAppContextKeys =
    [
        "MonoMod.LogRecordHoles",
        "MonoMod.LogReplayQueueLength",
        "MonoMod.LogSpam",
        "MonoMod.LogToFile",
        "MonoMod.LogToFileFilter",
        "MonoMod.LogInMemory",
    ];

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
    private HarmonyApiSnapshot? _harmonyApi;
    private HarmonyTypeInitializationSnapshot? _harmonyTypeInitialization;
    private HarmonyProcessorCreationSnapshot? _harmonyConstruction;
    private HarmonyProcessorApiSnapshot? _processorApi;
    private PatchProcessorTypeInitializationSnapshot? _processorTypeInitialization;
    private LauncherProbeSnapshot? _launcherProbe;
    private ProcessorCreationSnapshot? _processorCreation;
    private object? _harmonyInstance;
    private object? _patchProcessorInstance;
    private bool _provenInitializationAuditPassed;
    private bool _provenHarmonyTypeInitializationAuditPassed;
    private bool _provenPostConstructionAuditPassed;
    private Step26LoadContext? _loadContext;
    private bool _disposed;

    public ControlledHarmonyProcessorCreation(string launcherDataRoot, bool collectibleLoadContext = false)
        : this(
            launcherDataRoot,
            collectibleLoadContext,
            FirstRealGameAssemblyLoad.ExpectedPrimarySimpleName,
            TargetSimpleName,
            TargetVersion,
            [FirstRealGameAssemblyLoad.ExpectedPrimarySimpleName, "SlayTheSpire2", TargetSimpleName])
    {
    }

    internal ControlledHarmonyProcessorCreation(
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
        ClearStep26ObjectState();
        ReleaseLoadContext();
        _step23Preflight.Reset();
        _preflight = null;
        _provenInitializationAuditPassed = false;
        _provenHarmonyTypeInitializationAuditPassed = false;
        _provenPostConstructionAuditPassed = false;
    }

    public async Task<ControlledHarmonyProcessorCreationGateResult> RunInitializationPreflightAsync(
        IProgress<ControlledHarmonyProcessorCreationProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            EnsureFreshProcess();
            cancellationToken.ThrowIfCancellationRequested();

            stage = "accepted Step 23 preflight replay";
            progress?.Report(new ControlledHarmonyProcessorCreationProgress(
                ControlledHarmonyProcessorCreationGate.InitializationPreflight,
                0,
                0,
                null,
                "Re-running the physically proven Step 23 Gate A preflight before any Step 24 CLR load…"));

            _step23Preflight.Reset();
            var step23Result = await _step23Preflight.RunPreparedLoadPreflightAsync(
                progress is null
                    ? null
                    : new CallbackProgress<FirstRealGameAssemblyLoadProgress>(value =>
                        progress.Report(new ControlledHarmonyProcessorCreationProgress(
                            ControlledHarmonyProcessorCreationGate.InitializationPreflight,
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

            stage = "conditional automatic-initialization policy";
            var monoModEnvironmentOverrides = Environment.GetEnvironmentVariables().Keys
                .Cast<object>()
                .Select(key => Convert.ToString(key, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty)
                .Where(key => key.StartsWith("MONOMOD_", StringComparison.OrdinalIgnoreCase))
                .OrderBy(key => key, StringComparer.OrdinalIgnoreCase)
                .ToArray();
            var monoModAppContextOverrides = MonoModLoggingAppContextKeys
                .Where(key => AppContext.GetData(key) is not null || AppContext.TryGetSwitch(key, out _))
                .OrderBy(key => key, StringComparer.Ordinal)
                .ToArray();
            var hazardPolicy = EvaluateInitializerHazardPolicy(
                target.AssemblyName.Name ?? string.Empty,
                target.AssemblyName.Version ?? ZeroVersion,
                target.InitializerHazards,
                target.AutomaticInitializerAudits,
                System.Diagnostics.Debugger.IsAttached,
                monoModEnvironmentOverrides,
                monoModAppContextOverrides);
            if (!hazardPolicy.Allowed)
            {
                throw new InvalidDataException(
                    "Step 24 Gate A refuses automatic initialization because the bounded Cecil call-graph audit found a prohibited, unresolved, or non-dormant execution edge:\n" +
                    string.Join("\n", target.InitializerHazards) + "\n" +
                    "Conditional policy:\n" + hazardPolicy.Detail + "\n" +
                    "Audited automatic-initialization IL:\n" + string.Join("\n", target.AutomaticInitializerAudits));
            }

            stage = "Harmony constructor metadata preflight";
            var harmonyConstructorMetadata = ReadHarmonyConstructorMetadata(target.PreparedPath);
            var harmonyDebugEnvironment = Environment.GetEnvironmentVariable("HARMONY_DEBUG");
            if (!string.IsNullOrEmpty(harmonyDebugEnvironment))
            {
                throw new InvalidDataException(
                    "Step 26 Gate A requires HARMONY_DEBUG to be absent/empty so the exact Harmony constructor debug branch remains dormant. " +
                    "Observed value length: " + harmonyDebugEnvironment.Length.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }
            if (!harmonyConstructorMetadata.Allowed)
            {
                throw new InvalidDataException(
                    "Step 26 Gate A refuses Harmony construction because the exact constructor metadata policy did not pass:\n" +
                    harmonyConstructorMetadata.Detail + "\n" +
                    "Audited Harmony constructor IL:\n" + harmonyConstructorMetadata.ConstructorAudit);
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
                harmonyConstructorMetadata,
                offline);

            progress?.Report(new ControlledHarmonyProcessorCreationProgress(
                ControlledHarmonyProcessorCreationGate.InitializationPreflight,
                prepared.Count,
                prepared.Count,
                target.Plan.RelativePath,
                "The accepted Step 23 preflight still passes and the sole deferred initializer is exactly 0Harmony 2.4.2.0 with zero effective blocking hazards under the measured conditional policy."));

            return Pass(
                ControlledHarmonyProcessorCreationGate.InitializationPreflight,
                "Step 24 initialization preflight passed before any Step 24 CLR load.\n" +
                "Accepted Step 23 Gate A replay: PASS\n" +
                $"Runtime plan SHA-256: {planSha256}\n" +
                $"Prepared assemblies: {prepared.Count:N0}\n" +
                $"Initializer-bearing dependencies: {initializerBearing.Length:N0}\n" +
                $"Initialization target: {target.Plan.AssemblyFullName}\n" +
                $"Target module initializers: {target.ModuleInitializerCount:N0}\n" +
                $"Automatic initializer methods in audited closure: {target.AutomaticInitializerCount:N0}\n" +
                $"Initializer reachable same-assembly methods audited: {target.InitializerReachableMethods:N0}\n" +
                $"Raw conservative audit findings: {target.InitializerHazards.Count:N0}\n" +
                "Raw conservative audit detail:\n" + (target.InitializerHazards.Count == 0 ? "<none>" : string.Join("\n", target.InitializerHazards)) + "\n" +
                $"Conditionally dormant MonoMod logging findings: {hazardPolicy.ConditionalHazardCount:N0}\n" +
                $"Initializer hazards: {hazardPolicy.BlockingHazardCount:N0}\n" +
                "Conditional automatic-initialization policy: PASS\n" +
                hazardPolicy.Detail + "\n" +
                $"Target prepared SHA-1: {target.Plan.Sha1Hex}\n" +
                "Audited automatic-initialization IL:\n" + string.Join("\n", target.AutomaticInitializerAudits) + "\n" +
                "Harmony constructor metadata policy: PASS\n" +
                harmonyConstructorMetadata.Detail + "\n" +
                "HARMONY_DEBUG environment activation: ABSENT\n" +
                "Audited Harmony constructor IL:\n" + harmonyConstructorMetadata.ConstructorAudit + "\n" +
                "No real game/Harmony assembly was loaded by Step 26 Gate A: YES");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return Fail(ControlledHarmonyProcessorCreationGate.InitializationPreflight, stage, ex);
        }
    }

    public ControlledHarmonyProcessorCreationGateResult RunProvenLoadStateReplay()
    {
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var preflight = RequirePreflight();
            EnsureFreshProcess();

            stage = "dedicated load context creation";
            var context = new Step26LoadContext(
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
                ControlledHarmonyProcessorCreationGate.ProvenLoadStateReplay,
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
            return Fail(ControlledHarmonyProcessorCreationGate.ProvenLoadStateReplay, stage, ex);
        }
    }

    public ControlledHarmonyProcessorCreationGateResult RunDeferredModuleInitialization()
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
                targetAssembly,
                actualIdentity,
                postSha1,
                context.ManagedResolverRequests.Count - managedRequestsBefore,
                context.PrivateLoads.Count - privateLoadsBefore,
                context.HostLoads.Count - hostLoadsBefore,
                context.NativeLoadAttempts.Count - nativeAttemptsBefore);

            return Pass(
                ControlledHarmonyProcessorCreationGate.DeferredModuleInitialization,
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
            return Fail(ControlledHarmonyProcessorCreationGate.DeferredModuleInitialization, stage, ex);
        }
    }

    public async Task<ControlledHarmonyProcessorCreationGateResult> RunProvenInitializationAuditAsync(
        IProgress<ControlledHarmonyProcessorCreationProgress>? progress = null,
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
                progress?.Report(new ControlledHarmonyProcessorCreationProgress(
                    ControlledHarmonyProcessorCreationGate.ProvenInitializationAudit,
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

            _provenInitializationAuditPassed = true;
            return Pass(
                ControlledHarmonyProcessorCreationGate.ProvenInitializationAudit,
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
            return Fail(ControlledHarmonyProcessorCreationGate.ProvenInitializationAudit, stage, ex);
        }
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Step 26 intentionally resolves one exact receipt-verified post-publish Harmony type and API surface after metadata preflight.")]
    [UnconditionalSuppressMessage("Trimming", "IL2075", Justification = "The exact post-publish Harmony type is unavailable to the build-time trimmer; Gate A/E enforce the required constructor/member shape at runtime.")]
    public ControlledHarmonyProcessorCreationGateResult RunHarmonyApiResolution()
    {
        var stage = "Harmony API resolution";
        try
        {
            ThrowIfDisposed();
            var preflight = RequirePreflight();
            var initialization = RequireInitialization();
            var context = RequireLoadContext();
            if (!_provenInitializationAuditPassed)
                throw new InvalidOperationException("Step 26 Gate D must pass before resolving the Harmony API surface.");

            stage = "target assembly ownership";
            var targetAssembly = initialization.TargetAssembly;
            if (!ReferenceEquals(AssemblyLoadContext.GetLoadContext(targetAssembly), context))
                throw new InvalidDataException("Step 26 Harmony target is not owned by the dedicated Step 26 load context.");
            var actualIdentity = targetAssembly.GetName().FullName ?? targetAssembly.GetName().Name ?? string.Empty;
            if (!actualIdentity.Equals(preflight.Target.Plan.AssemblyFullName, StringComparison.Ordinal))
                throw new InvalidDataException("Step 26 Harmony target identity drifted before API resolution.");

            stage = "exact Harmony type resolution";
            var harmonyType = targetAssembly.GetType(HarmonyTypeFullName, throwOnError: true, ignoreCase: false)
                ?? throw new TypeLoadException("Step 26 could not resolve the exact HarmonyLib.Harmony type.");
            if (harmonyType.Assembly != targetAssembly || !harmonyType.IsClass || harmonyType.IsAbstract || !harmonyType.IsPublic)
                throw new InvalidDataException("Step 26 requires HarmonyLib.Harmony to be a public non-abstract class owned by exact 0Harmony.");
            var typeInitializer = harmonyType.TypeInitializer
                ?? throw new MissingMethodException("Step 26 requires the exact HarmonyLib.Harmony type initializer measured by Gate A; it is missing at runtime.");

            stage = "exact Harmony constructor resolution";
            var constructors = harmonyType.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            var publicConstructors = constructors.Where(ctor => ctor.IsPublic).ToArray();
            var constructor = publicConstructors.SingleOrDefault(ctor =>
            {
                var parameters = ctor.GetParameters();
                return parameters.Length == 1 && parameters[0].ParameterType == typeof(string);
            });
            if (constructor is null || publicConstructors.Length != 1)
                throw new MissingMethodException("Step 26 requires exactly one public HarmonyLib.Harmony instance constructor and it must be .ctor(System.String).");

            stage = "exact Harmony observation members";
            var idProperty = harmonyType.GetProperty("Id", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            if (idProperty is null || idProperty.PropertyType != typeof(string) || idProperty.GetMethod is null || !idProperty.GetMethod.IsPublic)
                throw new MissingMemberException("Step 26 requires the public instance Harmony.Id string getter for post-construction verification.");
            var debugField = harmonyType.GetField("DEBUG", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
            if (debugField is null || debugField.FieldType != typeof(bool))
                throw new MissingFieldException("Step 26 requires the public static Harmony.DEBUG boolean field for the inert-constructor precondition.");

            stage = "pre-type-initialization environment precondition";
            var harmonyDebugEnvironment = Environment.GetEnvironmentVariable("HARMONY_DEBUG");
            if (!string.IsNullOrEmpty(harmonyDebugEnvironment))
                throw new InvalidDataException("Step 26 refuses Harmony type initialization while HARMONY_DEBUG is non-empty.");

            _harmonyApi = new HarmonyApiSnapshot(harmonyType, typeInitializer, constructor, idProperty, debugField, actualIdentity);

            return Pass(
                ControlledHarmonyProcessorCreationGate.HarmonyApiResolution,
                "TARGETED HARMONY API RESOLUTION SUCCEEDED WITHOUT TYPE INITIALIZATION OR CONSTRUCTION.\n" +
                $"Assembly: {actualIdentity}\n" +
                $"Type: {harmonyType.FullName}\n" +
                "Type initializer: PRESENT — exact Gate-A-measured static-cache initializer\n" +
                "Public instance constructors: 1\n" +
                "Exact constructor: HarmonyLib.Harmony::.ctor(System.String)\n" +
                "Observation property: Harmony.Id : System.String\n" +
                "Inert-state field metadata: Harmony.DEBUG : System.Boolean\n" +
                "HARMONY_DEBUG environment activation: ABSENT\n" +
                "Harmony.DEBUG value read: NO — Gate F owns the type-initialization boundary\n" +
                "Targeted reflection only: YES\n" +
                "Harmony type initializer executed by Step 26: NO\n" +
                "Harmony object constructed: NO\n" +
                "Harmony patch API invoked: NO\n" +
                "Game type/member reflected or invoked: NO");
        }
        catch (Exception ex)
        {
            return Fail(ControlledHarmonyProcessorCreationGate.HarmonyApiResolution, stage, ex);
        }
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Step 26 explicitly executes only the exact Harmony type initializer whose IL shape was measured by Gate A.")]
    [UnconditionalSuppressMessage("Trimming", "IL2075", Justification = "Step 26 reads only exact reflection members resolved by Gate E after the explicit Harmony type-initialization barrier.")]
    public ControlledHarmonyProcessorCreationGateResult RunHarmonyTypeInitialization()
    {
        var stage = "Harmony type initialization";
        try
        {
            ThrowIfDisposed();
            var preflight = RequirePreflight();
            var api = RequireHarmonyApi();
            var context = RequireLoadContext();

            stage = "immediate pre-type-initialization integrity recheck";
            VerifyFileLength(preflight.Target.PreparedPath, preflight.Target.Plan.Length, "prepared Harmony type-initialization target");
            var targetSha1 = ComputeSha1Hex(preflight.Target.PreparedPath);
            if (!targetSha1.Equals(preflight.Target.Plan.Sha1Hex, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Step 26 target SHA-1 changed immediately before Harmony type initialization.");
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("HARMONY_DEBUG")))
                throw new InvalidDataException("Step 26 refuses Harmony type initialization because HARMONY_DEBUG became non-empty after Gate E.");

            var managedRequestsBefore = context.ManagedResolverRequests.Count;
            var privateLoadsBefore = context.PrivateLoads.Count;
            var hostLoadsBefore = context.HostLoads.Count;
            var nativeAttemptsBefore = context.NativeLoadAttempts.Count;
            var contextMembershipBefore = context.Assemblies
                .Select(assembly => assembly.GetName().FullName ?? assembly.GetName().Name ?? string.Empty)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();

            stage = "Harmony type-constructor completion barrier";
            RuntimeHelpers.RunClassConstructor(api.HarmonyType.TypeHandle);

            stage = "post-type-initialization state verification";
            if (api.DebugField.GetValue(null) is not bool debugAfter || debugAfter)
                throw new InvalidDataException("Step 26 Harmony.DEBUG is true after the explicit Harmony type initializer.");
            if (context.NativeLoadAttempts.Count != nativeAttemptsBefore)
                throw new DllNotFoundException("Harmony type initialization attempted native-library resolution: " + string.Join(" | ", context.NativeLoadAttempts.Skip(nativeAttemptsBefore)));
            if (context.RejectedManagedRequests.Count != 0)
                throw new FileLoadException("Harmony type initialization triggered an unplanned managed request: " + string.Join(" | ", context.RejectedManagedRequests));
            var contextMembershipAfter = context.Assemblies
                .Select(assembly => assembly.GetName().FullName ?? assembly.GetName().Name ?? string.Empty)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            if (!contextMembershipAfter.SequenceEqual(contextMembershipBefore, StringComparer.Ordinal))
                throw new InvalidDataException("Step 26 Harmony type initialization changed private-context assembly membership.");

            var postSha1 = ComputeSha1Hex(preflight.Target.PreparedPath);
            if (!postSha1.Equals(targetSha1, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("0Harmony prepared bytes changed across Harmony type initialization.");

            _harmonyTypeInitialization = new HarmonyTypeInitializationSnapshot(
                postSha1,
                context.ManagedResolverRequests.Count - managedRequestsBefore,
                context.PrivateLoads.Count - privateLoadsBefore,
                context.HostLoads.Count - hostLoadsBefore,
                context.NativeLoadAttempts.Count - nativeAttemptsBefore);

            return Pass(
                ControlledHarmonyProcessorCreationGate.HarmonyTypeInitialization,
                "CONTROLLED HARMONY TYPE INITIALIZATION SUCCEEDED.\n" +
                "Completion barrier: RuntimeHelpers.RunClassConstructor(HarmonyLib.Harmony.TypeHandle) = PASS\n" +
                "Measured type initializer: ConditionalWeakTable<...> → AssemblyCachedCategories\n" +
                "Harmony.DEBUG after type initialization: false\n" +
                $"Managed resolver requests during type initialization: {_harmonyTypeInitialization.ManagedResolverRequestsDuringTypeInitialization:N0}\n" +
                $"Private loads during type initialization: {_harmonyTypeInitialization.PrivateLoadsDuringTypeInitialization:N0}\n" +
                $"Host loads during type initialization: {_harmonyTypeInitialization.HostLoadsDuringTypeInitialization:N0}\n" +
                $"Native load attempts during type initialization: {_harmonyTypeInitialization.NativeLoadAttemptsDuringTypeInitialization:N0}\n" +
                "Private-context membership changed: NO\n" +
                "Harmony object constructed: NO\n" +
                "Harmony patch/processor API invoked: NO\n" +
                "Game type/member reflected or invoked: NO");
        }
        catch (Exception ex)
        {
            return Fail(ControlledHarmonyProcessorCreationGate.HarmonyTypeInitialization, stage, ex);
        }
    }

    [UnconditionalSuppressMessage("Trimming", "IL2075", Justification = "Step 26 Gate G reads only exact Harmony reflection members resolved by Gate E after Gate F completed the type initializer.")]
    public ControlledHarmonyProcessorCreationGateResult RunHarmonyTypeInitializationAudit()
    {
        var stage = "Harmony type-initialization audit";
        try
        {
            ThrowIfDisposed();
            var preflight = RequirePreflight();
            var api = RequireHarmonyApi();
            var typeInitialization = RequireHarmonyTypeInitialization();
            var context = RequireLoadContext();

            stage = "type-initialization byte/context audit";
            var targetSha1 = ComputeSha1Hex(preflight.Target.PreparedPath);
            if (!targetSha1.Equals(typeInitialization.PreparedSha1, StringComparison.OrdinalIgnoreCase) ||
                !targetSha1.Equals(preflight.Target.Plan.Sha1Hex, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Step 26 0Harmony hash drifted after the Harmony type initializer.");
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("HARMONY_DEBUG")))
                throw new InvalidDataException("Step 26 HARMONY_DEBUG became non-empty after Harmony type initialization.");
            if (api.DebugField.GetValue(null) is not bool debugValue || debugValue)
                throw new InvalidDataException("Step 26 Harmony.DEBUG is true during the post-type-initialization audit.");
            if (_harmonyInstance is not null)
                throw new InvalidOperationException("Step 26 Harmony object exists before the instance-construction gate.");

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
                throw new InvalidDataException("Step 26 Harmony type initialization changed the physically proven Step 24 context membership.");
            if (context.NativeLoadAttempts.Count != 0)
                throw new DllNotFoundException("Step 26 observed native-library resolution after Harmony type initialization: " + string.Join(" | ", context.NativeLoadAttempts));
            if (context.RejectedManagedRequests.Count != 0)
                throw new FileLoadException("Step 26 observed rejected/unplanned managed requests after Harmony type initialization: " + string.Join(" | ", context.RejectedManagedRequests));

            _provenHarmonyTypeInitializationAuditPassed = true;
            return Pass(
                ControlledHarmonyProcessorCreationGate.HarmonyTypeInitializationAudit,
                "HARMONY TYPE-INITIALIZATION AUDIT PASSED.\n" +
                $"0Harmony prepared SHA-1 unchanged: {targetSha1}\n" +
                $"Private context: {actual.Length:N0}/{expected.Length:N0} expected assemblies\n" +
                "Harmony.DEBUG: false\n" +
                "HARMONY_DEBUG environment activation: ABSENT\n" +
                "Harmony object constructed: NO\n" +
                $"Native load attempts: {context.NativeLoadAttempts.Count:N0}\n" +
                $"Rejected/unplanned managed requests: {context.RejectedManagedRequests.Count:N0}\n" +
                "Harmony patch/processor API invoked: NO\n" +
                "Game type/member reflected or invoked: NO");
        }
        catch (Exception ex)
        {
            return Fail(ControlledHarmonyProcessorCreationGate.HarmonyTypeInitializationAudit, stage, ex);
        }
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Step 26 invokes one exact constructor from receipt-verified post-publish 0Harmony after metadata, API-shape, and explicit type-initialization preflight.")]
    [UnconditionalSuppressMessage("Trimming", "IL2075", Justification = "Step 26 retains only exact reflection objects resolved and verified by Gate E after the explicit type-initialization boundary.")]
    public ControlledHarmonyProcessorCreationGateResult RunHarmonyInstanceConstruction()
    {
        var stage = "Harmony instance construction";
        try
        {
            ThrowIfDisposed();
            var preflight = RequirePreflight();
            var api = RequireHarmonyApi();
            var context = RequireLoadContext();
            if (!_provenHarmonyTypeInitializationAuditPassed)
                throw new InvalidOperationException("Step 26 Gate G must pass before the Harmony instance constructor runs.");

            stage = "immediate pre-construction integrity recheck";
            VerifyFileLength(preflight.Target.PreparedPath, preflight.Target.Plan.Length, "prepared Harmony construction target");
            var targetSha1 = ComputeSha1Hex(preflight.Target.PreparedPath);
            if (!targetSha1.Equals(preflight.Target.Plan.Sha1Hex, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Step 26 target SHA-1 changed immediately before Harmony construction.");
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("HARMONY_DEBUG")))
                throw new InvalidDataException("Step 26 refuses construction because HARMONY_DEBUG became non-empty after Gate G.");
            if (api.DebugField.GetValue(null) is not bool debugBefore || debugBefore)
                throw new InvalidDataException("Step 26 refuses construction because Harmony.DEBUG became true after Gate G.");

            var managedRequestsBefore = context.ManagedResolverRequests.Count;
            var privateLoadsBefore = context.PrivateLoads.Count;
            var hostLoadsBefore = context.HostLoads.Count;
            var nativeAttemptsBefore = context.NativeLoadAttempts.Count;
            var contextMembershipBefore = context.Assemblies
                .Select(assembly => assembly.GetName().FullName ?? assembly.GetName().Name ?? string.Empty)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();

            stage = "exact Harmony(string) constructor invocation";
            object instance;
            try
            {
                instance = api.Constructor.Invoke([HarmonyId]);
            }
            catch (TargetInvocationException ex) when (ex.InnerException is not null)
            {
                throw new InvalidOperationException(
                    "Exact Harmony(string) constructor threw: " + ex.InnerException.GetType().FullName + ": " + ex.InnerException.Message,
                    ex.InnerException);
            }

            stage = "constructed instance verification";
            if (!ReferenceEquals(instance.GetType(), api.HarmonyType))
                throw new InvalidDataException("Step 26 constructor returned an unexpected runtime type.");
            if (!ReferenceEquals(AssemblyLoadContext.GetLoadContext(instance.GetType().Assembly), context))
                throw new InvalidDataException("Step 26 Harmony instance type escaped the dedicated load context.");
            var id = api.IdProperty.GetValue(instance) as string;
            if (!string.Equals(id, HarmonyId, StringComparison.Ordinal))
                throw new InvalidDataException($"Step 26 Harmony.Id mismatch. Expected '{HarmonyId}', observed '{id ?? "<null>"}'.");
            if (api.DebugField.GetValue(null) is not bool debugAfter || debugAfter)
                throw new InvalidDataException("Step 26 Harmony.DEBUG was true after inert constructor execution.");

            stage = "constructor isolation checks";
            if (context.NativeLoadAttempts.Count != nativeAttemptsBefore)
                throw new DllNotFoundException("Harmony construction attempted native-library resolution: " + string.Join(" | ", context.NativeLoadAttempts.Skip(nativeAttemptsBefore)));
            if (context.RejectedManagedRequests.Count != 0)
                throw new FileLoadException("Harmony construction triggered an unplanned managed request: " + string.Join(" | ", context.RejectedManagedRequests));
            var contextMembershipAfter = context.Assemblies
                .Select(assembly => assembly.GetName().FullName ?? assembly.GetName().Name ?? string.Empty)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            if (!contextMembershipAfter.SequenceEqual(contextMembershipBefore, StringComparer.Ordinal))
                throw new InvalidDataException("Step 26 Harmony construction changed private-context assembly membership.");

            var postSha1 = ComputeSha1Hex(preflight.Target.PreparedPath);
            if (!postSha1.Equals(targetSha1, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("0Harmony prepared bytes changed across Harmony object construction.");

            _harmonyInstance = instance;
            _harmonyConstruction = new HarmonyProcessorCreationSnapshot(
                id,
                postSha1,
                context.ManagedResolverRequests.Count - managedRequestsBefore,
                context.PrivateLoads.Count - privateLoadsBefore,
                context.HostLoads.Count - hostLoadsBefore,
                context.NativeLoadAttempts.Count - nativeAttemptsBefore);

            return Pass(
                ControlledHarmonyProcessorCreationGate.HarmonyInstanceConstruction,
                "CONTROLLED HARMONY INSTANCE CONSTRUCTION SUCCEEDED.\n" +
                $"Constructor: {HarmonyTypeFullName}::.ctor(System.String)\n" +
                $"Probe ID: {id}\n" +
                "Harmony type initializer was completed in prior Gate F: YES\n" +
                "Harmony.DEBUG after construction: false\n" +
                $"Managed resolver requests during construction: {_harmonyConstruction.ManagedResolverRequestsDuringConstruction:N0}\n" +
                $"Private loads during construction: {_harmonyConstruction.PrivateLoadsDuringConstruction:N0}\n" +
                $"Host loads during construction: {_harmonyConstruction.HostLoadsDuringConstruction:N0}\n" +
                $"Native load attempts during construction: {_harmonyConstruction.NativeLoadAttemptsDuringConstruction:N0}\n" +
                "Private-context membership changed: NO\n" +
                "Harmony Patch/PatchAll/CreateProcessor invoked: NO\n" +
                "Game type/member reflected or invoked: NO\n" +
                "Godot/game startup requested: NO");
        }
        catch (Exception ex)
        {
            return Fail(ControlledHarmonyProcessorCreationGate.HarmonyInstanceConstruction, stage, ex);
        }
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Step 26 final audit reads only exact reflection members already resolved and verified by Gate E.")]
    [UnconditionalSuppressMessage("Trimming", "IL2075", Justification = "Step 26 final audit retains only exact reflection objects already resolved and verified by Gate E.")]
    public async Task<ControlledHarmonyProcessorCreationGateResult> RunPostConstructionAuditAsync(
        IProgress<ControlledHarmonyProcessorCreationProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var stage = "post-construction audit";
        try
        {
            ThrowIfDisposed();
            var preflight = RequirePreflight();
            var initialization = RequireInitialization();
            var typeInitialization = RequireHarmonyTypeInitialization();
            var api = RequireHarmonyApi();
            var construction = RequireHarmonyProcessorCreation();
            var context = RequireLoadContext();
            var instance = _harmonyInstance ?? throw new InvalidOperationException("Step 26 Harmony instance is missing after Gate H.");

            stage = "runtime plan rehash";
            var planSha256 = await ComputeSha256HexAsync(_planPath, cancellationToken).ConfigureAwait(false);
            if (!planSha256.Equals(preflight.PlanSha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Step 26 runtime-binding plan changed during Harmony construction.");

            stage = "prepared/live byte audit";
            var verified = 0;
            foreach (var item in preflight.PreparedAssemblies)
            {
                cancellationToken.ThrowIfCancellationRequested();
                VerifyFileLength(item.PreparedPath, item.Plan.Length, "prepared post-construction");
                VerifyFileLength(item.LivePath, item.Plan.Length, "live post-construction");
                var preparedSha1 = await ComputeSha1HexAsync(item.PreparedPath, cancellationToken).ConfigureAwait(false);
                var liveSha1 = await ComputeSha1HexAsync(item.LivePath, cancellationToken).ConfigureAwait(false);
                if (!preparedSha1.Equals(item.Plan.Sha1Hex, StringComparison.OrdinalIgnoreCase) ||
                    !liveSha1.Equals(item.Plan.Sha1Hex, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException("Step 26 prepared/live byte identity changed during Harmony construction: " + item.Plan.RelativePath);
                verified++;
                progress?.Report(new ControlledHarmonyProcessorCreationProgress(
                    ControlledHarmonyProcessorCreationGate.PostConstructionAudit,
                    verified,
                    preflight.PreparedAssemblies.Length,
                    item.Plan.RelativePath,
                    "Re-hashing prepared/live bytes after the Harmony constructor boundary…"));
            }

            stage = "OfflineReady postcondition";
            var offline = await _offlineInspection.RunAsync(null, cancellationToken).ConfigureAwait(false);
            if (!offline.Success || !offline.ExactManagedTreeVerified || offline.InstalledManifestId != preflight.Plan.ManifestId)
                throw new InvalidDataException(offline.Error ?? "OfflineReady exact-tree verification failed after Step 26 Harmony construction.");

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
                throw new InvalidDataException("Step 26 post-construction private context differs from the physically proven Step 24 context membership.");
            if (context.NativeLoadAttempts.Count != 0)
                throw new DllNotFoundException("Step 26 observed native-library resolution: " + string.Join(" | ", context.NativeLoadAttempts));
            if (context.RejectedManagedRequests.Count != 0)
                throw new FileLoadException("Step 26 observed rejected/unplanned managed requests: " + string.Join(" | ", context.RejectedManagedRequests));

            stage = "Harmony object identity audit";
            if (!ReferenceEquals(instance.GetType(), api.HarmonyType))
                throw new InvalidDataException("Step 26 retained Harmony object changed runtime type.");
            var id = api.IdProperty.GetValue(instance) as string;
            if (!string.Equals(id, HarmonyId, StringComparison.Ordinal) || !string.Equals(id, construction.Id, StringComparison.Ordinal))
                throw new InvalidDataException("Step 26 retained Harmony object ID changed after construction.");
            if (api.DebugField.GetValue(null) is not bool debugValue || debugValue)
                throw new InvalidDataException("Step 26 Harmony.DEBUG is true during the final audit.");
            var targetSha1 = await ComputeSha1HexAsync(preflight.Target.PreparedPath, cancellationToken).ConfigureAwait(false);
            if (!targetSha1.Equals(initialization.PreparedSha1, StringComparison.OrdinalIgnoreCase) ||
                !targetSha1.Equals(typeInitialization.PreparedSha1, StringComparison.OrdinalIgnoreCase) ||
                !targetSha1.Equals(construction.PreparedSha1, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Step 26 0Harmony prepared hash changed across module initialization/type initialization/construction.");

            _provenPostConstructionAuditPassed = true;
            return Pass(
                ControlledHarmonyProcessorCreationGate.PostConstructionAudit,
                "Step 26 post-construction isolation audit passed.\n" +
                $"Prepared/live assemblies re-hashed: {verified:N0}/{preflight.PreparedAssemblies.Length:N0}\n" +
                $"Runtime plan SHA-256 unchanged: {planSha256}\n" +
                $"Private context: {actual.Length:N0}/{expected.Length:N0} expected assemblies\n" +
                $"Harmony instance type: {api.HarmonyType.FullName}\n" +
                $"Harmony.Id: {id}\n" +
                "Harmony.DEBUG: false\n" +
                $"Native load attempts: {context.NativeLoadAttempts.Count:N0}\n" +
                $"Rejected/unplanned managed requests: {context.RejectedManagedRequests.Count:N0}\n" +
                "Post-construction OfflineReady exact-tree verification: YES\n" +
                "Trusted Step 12 managed install unchanged: YES\n" +
                "Prepared Step 21/22 bytes unchanged: YES\n" +
                "Harmony type initialization: YES — exact measured static-cache initializer only\n" +
                "Harmony object construction: YES — exact string constructor only\n" +
                "Harmony patching/processor API invocation: NO\n" +
                "Game entry point/type/member reflection or invocation: NO\n" +
                "Godot/game initialization: NO\n" +
                "Native game library loaded by Step 26: NO\n" +
                "Process note: the Step 26 private managed context remains resident until process exit; force-quit before rerunning earlier fresh-process CLR-load regressions.");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return Fail(ControlledHarmonyProcessorCreationGate.PostConstructionAudit, stage, ex);
        }
    }


    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Step 26 resolves only the exact receipt-verified post-publish Harmony.CreateProcessor/PatchProcessor surface after Cecil metadata preflight.")]
    [UnconditionalSuppressMessage("Trimming", "IL2075", Justification = "The post-publish Harmony types are unavailable to the build-time trimmer; exact runtime reflection is bounded by metadata and identity checks.")]
    public ControlledHarmonyProcessorCreationGateResult RunHarmonyProcessorApiResolution()
    {
        var stage = "Harmony processor API resolution";
        try
        {
            ThrowIfDisposed();
            var preflight = RequirePreflight();
            var harmonyApi = RequireHarmonyApi();
            var initialization = RequireInitialization();
            var context = RequireLoadContext();
            if (!_provenPostConstructionAuditPassed)
                throw new InvalidOperationException("Step 26 Gate I must pass before resolving the processor API surface.");

            stage = "processor Cecil metadata preflight";
            var metadata = ReadHarmonyProcessorMetadata(preflight.Target.PreparedPath);
            if (!metadata.Allowed)
                throw new InvalidDataException("Step 26 Gate J refuses processor admission because the exact processor metadata shape changed:\n" + metadata.Detail);

            var managedBefore = context.ManagedResolverRequests.Count;
            var privateBefore = context.PrivateLoads.Count;
            var hostBefore = context.HostLoads.Count;
            var nativeBefore = context.NativeLoadAttempts.Count;
            var membershipBefore = context.Assemblies.Select(a => a.GetName().FullName ?? a.GetName().Name ?? string.Empty).OrderBy(v => v, StringComparer.Ordinal).ToArray();

            stage = "exact PatchProcessor type resolution";
            var processorType = initialization.TargetAssembly.GetType(PatchProcessorTypeFullName, throwOnError: false, ignoreCase: false)
                ?? throw new TypeLoadException("Exact HarmonyLib.PatchProcessor type is absent from loaded 0Harmony.");
            if (!processorType.IsClass || !processorType.IsPublic || processorType.IsAbstract)
                throw new InvalidDataException("HarmonyLib.PatchProcessor runtime type shape is unexpected.");
            if (!ReferenceEquals(AssemblyLoadContext.GetLoadContext(processorType.Assembly), context))
                throw new InvalidDataException("HarmonyLib.PatchProcessor escaped the Step 26 private load context.");

            var typeInitializer = processorType.TypeInitializer
                ?? throw new MissingMethodException(PatchProcessorTypeFullName, ".cctor");
            var constructors = processorType.GetConstructors(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            var constructor = constructors.SingleOrDefault(candidate =>
            {
                var parameters = candidate.GetParameters();
                return parameters.Length == 2 &&
                       ReferenceEquals(parameters[0].ParameterType, harmonyApi.HarmonyType) &&
                       parameters[1].ParameterType == typeof(MethodBase);
            }) ?? throw new MissingMethodException(PatchProcessorTypeFullName, ".ctor(HarmonyLib.Harmony,System.Reflection.MethodBase)");
            if (constructors.Length != 1)
                throw new InvalidDataException($"Step 26 requires exactly one public PatchProcessor constructor; observed {constructors.Length}.");

            var createProcessorCandidates = harmonyApi.HarmonyType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(method => method.Name.Equals("CreateProcessor", StringComparison.Ordinal))
                .ToArray();
            var createProcessor = createProcessorCandidates.SingleOrDefault(method =>
            {
                var parameters = method.GetParameters();
                return parameters.Length == 1 && parameters[0].ParameterType == typeof(MethodBase) && ReferenceEquals(method.ReturnType, processorType);
            }) ?? throw new MissingMethodException(HarmonyTypeFullName, "CreateProcessor(System.Reflection.MethodBase)");
            if (createProcessorCandidates.Length != 1)
                throw new InvalidDataException($"Step 26 requires exactly one public Harmony.CreateProcessor overload; observed {createProcessorCandidates.Length}.");

            var instanceField = processorType.GetField("instance", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                ?? throw new MissingFieldException(PatchProcessorTypeFullName, "instance");
            var originalField = processorType.GetField("original", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                ?? throw new MissingFieldException(PatchProcessorTypeFullName, "original");
            if (!ReferenceEquals(instanceField.FieldType, harmonyApi.HarmonyType) || originalField.FieldType != typeof(MethodBase))
                throw new InvalidDataException("PatchProcessor retained-field runtime types do not match the measured 2.4.2 shape.");

            if (context.ManagedResolverRequests.Count != managedBefore || context.PrivateLoads.Count != privateBefore || context.HostLoads.Count != hostBefore)
                throw new InvalidDataException("Targeted processor API reflection unexpectedly changed resolver/load counters.");
            if (context.NativeLoadAttempts.Count != nativeBefore)
                throw new DllNotFoundException("Targeted processor API reflection attempted native resolution.");
            if (context.RejectedManagedRequests.Count != 0)
                throw new FileLoadException("Targeted processor API reflection triggered an unplanned managed request: " + string.Join(" | ", context.RejectedManagedRequests));
            var membershipAfter = context.Assemblies.Select(a => a.GetName().FullName ?? a.GetName().Name ?? string.Empty).OrderBy(v => v, StringComparer.Ordinal).ToArray();
            if (!membershipAfter.SequenceEqual(membershipBefore, StringComparer.Ordinal))
                throw new InvalidDataException("Targeted processor API reflection changed private-context membership.");

            _processorApi = new HarmonyProcessorApiSnapshot(
                processorType,
                typeInitializer,
                constructor,
                createProcessor,
                instanceField,
                originalField,
                metadata.CreateProcessorAudit,
                metadata.PatchProcessorConstructorAudit,
                metadata.PatchProcessorTypeInitializerAudit);

            return Pass(
                ControlledHarmonyProcessorCreationGate.HarmonyProcessorApiResolution,
                "TARGETED HARMONY PROCESSOR API RESOLUTION SUCCEEDED WITHOUT PATCHPROCESSOR TYPE INITIALIZATION OR CONSTRUCTION.\n" +
                $"Harmony factory: {HarmonyTypeFullName}::CreateProcessor(System.Reflection.MethodBase)\n" +
                $"Processor type: {processorType.FullName}\n" +
                "Processor type initializer: PRESENT — exact measured locker initialization shape\n" +
                "Processor constructor: .ctor(HarmonyLib.Harmony,System.Reflection.MethodBase)\n" +
                "Retained fields: instance + original — exact measured types\n" +
                "PatchProcessor type initializer executed by Step 26 Gate J: NO\n" +
                "PatchProcessor object constructed: NO\n" +
                "Patch()/Harmony.Patch invoked: NO\n" +
                "StS2 type/member reflected or invoked: NO\n" +
                "Audited Harmony.CreateProcessor IL:\n" + metadata.CreateProcessorAudit + "\n" +
                "Audited PatchProcessor::.cctor IL:\n" + metadata.PatchProcessorTypeInitializerAudit + "\n" +
                "Audited PatchProcessor::.ctor IL:\n" + metadata.PatchProcessorConstructorAudit);
        }
        catch (Exception ex)
        {
            return Fail(ControlledHarmonyProcessorCreationGate.HarmonyProcessorApiResolution, stage, ex);
        }
    }

    [UnconditionalSuppressMessage("Trimming", "IL2075", Justification = "Step 26 explicitly initializes only the exact PatchProcessor TypeHandle resolved and metadata-verified in Gate J.")]
    public ControlledHarmonyProcessorCreationGateResult RunPatchProcessorTypeInitialization()
    {
        var stage = "PatchProcessor type initialization";
        try
        {
            ThrowIfDisposed();
            var preflight = RequirePreflight();
            var api = RequireProcessorApi();
            var context = RequireLoadContext();

            VerifyFileLength(preflight.Target.PreparedPath, preflight.Target.Plan.Length, "prepared PatchProcessor type-initialization target");
            var targetSha1 = ComputeSha1Hex(preflight.Target.PreparedPath);
            if (!targetSha1.Equals(preflight.Target.Plan.Sha1Hex, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Step 26 0Harmony SHA-1 changed immediately before PatchProcessor type initialization.");

            var managedBefore = context.ManagedResolverRequests.Count;
            var privateBefore = context.PrivateLoads.Count;
            var hostBefore = context.HostLoads.Count;
            var nativeBefore = context.NativeLoadAttempts.Count;
            var membershipBefore = context.Assemblies.Select(a => a.GetName().FullName ?? a.GetName().Name ?? string.Empty).OrderBy(v => v, StringComparer.Ordinal).ToArray();

            stage = "RuntimeHelpers.RunClassConstructor(PatchProcessor.TypeHandle)";
            try
            {
                RuntimeHelpers.RunClassConstructor(api.PatchProcessorType.TypeHandle);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Exact PatchProcessor type initializer did not complete.", ex);
            }

            if (context.NativeLoadAttempts.Count != nativeBefore)
                throw new DllNotFoundException("PatchProcessor type initialization attempted native resolution: " + string.Join(" | ", context.NativeLoadAttempts.Skip(nativeBefore)));
            if (context.RejectedManagedRequests.Count != 0)
                throw new FileLoadException("PatchProcessor type initialization triggered an unplanned managed request: " + string.Join(" | ", context.RejectedManagedRequests));
            var membershipAfter = context.Assemblies.Select(a => a.GetName().FullName ?? a.GetName().Name ?? string.Empty).OrderBy(v => v, StringComparer.Ordinal).ToArray();
            if (!membershipAfter.SequenceEqual(membershipBefore, StringComparer.Ordinal))
                throw new InvalidDataException("PatchProcessor type initialization changed private-context membership.");
            var postSha1 = ComputeSha1Hex(preflight.Target.PreparedPath);
            if (!postSha1.Equals(targetSha1, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("0Harmony prepared bytes changed across PatchProcessor type initialization.");

            _processorTypeInitialization = new PatchProcessorTypeInitializationSnapshot(
                postSha1,
                context.ManagedResolverRequests.Count - managedBefore,
                context.PrivateLoads.Count - privateBefore,
                context.HostLoads.Count - hostBefore,
                context.NativeLoadAttempts.Count - nativeBefore);

            return Pass(
                ControlledHarmonyProcessorCreationGate.PatchProcessorTypeInitialization,
                "CONTROLLED PATCHPROCESSOR TYPE INITIALIZATION SUCCEEDED.\n" +
                "Completion barrier: RuntimeHelpers.RunClassConstructor(HarmonyLib.PatchProcessor.TypeHandle) = PASS\n" +
                "Measured initializer: new object() → PatchProcessor.locker\n" +
                $"Managed resolver requests during type initialization: {_processorTypeInitialization.ManagedResolverRequestsDuringTypeInitialization:N0}\n" +
                $"Private loads during type initialization: {_processorTypeInitialization.PrivateLoadsDuringTypeInitialization:N0}\n" +
                $"Host loads during type initialization: {_processorTypeInitialization.HostLoadsDuringTypeInitialization:N0}\n" +
                $"Native load attempts during type initialization: {_processorTypeInitialization.NativeLoadAttemptsDuringTypeInitialization:N0}\n" +
                "Private-context membership changed: NO\n" +
                "PatchProcessor object constructed: NO\n" +
                "Patch()/Harmony.Patch invoked: NO\n" +
                "StS2 type/member reflected or invoked: NO");
        }
        catch (Exception ex)
        {
            return Fail(ControlledHarmonyProcessorCreationGate.PatchProcessorTypeInitialization, stage, ex);
        }
    }

    [DynamicDependency(nameof(HarmonyProcessorProbe.Target), typeof(HarmonyProcessorProbe))]
    public ControlledHarmonyProcessorCreationGateResult RunLauncherProbeResolution()
    {
        var stage = "launcher probe MethodInfo resolution";
        try
        {
            ThrowIfDisposed();
            _ = RequireProcessorTypeInitialization();
            var context = RequireLoadContext();
            var managedBefore = context.ManagedResolverRequests.Count;
            var privateBefore = context.PrivateLoads.Count;
            var hostBefore = context.HostLoads.Count;
            var nativeBefore = context.NativeLoadAttempts.Count;

            var method = typeof(HarmonyProcessorProbe).GetMethod(
                nameof(HarmonyProcessorProbe.Target),
                BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                ?? throw new MissingMethodException(typeof(HarmonyProcessorProbe).FullName, nameof(HarmonyProcessorProbe.Target));
            if (method.ReturnType != typeof(int))
                throw new InvalidDataException("Step 26 launcher probe return type changed.");
            var parameters = method.GetParameters();
            if (parameters.Length != 1 || parameters[0].ParameterType != typeof(int))
                throw new InvalidDataException("Step 26 launcher probe signature changed.");
            if (method.DeclaringType != typeof(HarmonyProcessorProbe) || method.IsGenericMethod || !method.IsStatic)
                throw new InvalidDataException("Step 26 launcher probe MethodInfo shape is unexpected.");
            var probeContext = AssemblyLoadContext.GetLoadContext(method.DeclaringType.Assembly);
            if (!ReferenceEquals(probeContext, AssemblyLoadContext.Default))
                throw new InvalidDataException("Step 26 launcher-owned probe is not in the default host load context.");
            if (context.ManagedResolverRequests.Count != managedBefore || context.PrivateLoads.Count != privateBefore || context.HostLoads.Count != hostBefore || context.NativeLoadAttempts.Count != nativeBefore)
                throw new InvalidDataException("Resolving the launcher-owned probe unexpectedly affected the private Harmony context.");

            var signature = $"{method.ReturnType.FullName} {method.DeclaringType!.FullName}::{method.Name}({string.Join(",", parameters.Select(p => p.ParameterType.FullName))})";
            _launcherProbe = new LauncherProbeSnapshot(method, signature);
            return Pass(
                ControlledHarmonyProcessorCreationGate.LauncherProbeResolution,
                "LAUNCHER-OWNED PATCHPROCESSOR TARGET METHOD RESOLVED.\n" +
                $"Method: {signature}\n" +
                "Declaring assembly load context: DEFAULT HOST\n" +
                "Method invoked: NO\n" +
                "StS2 assembly/type/member reflection: NO\n" +
                "Harmony processor API invoked: NO\n" +
                "Patch()/Harmony.Patch invoked: NO");
        }
        catch (Exception ex)
        {
            return Fail(ControlledHarmonyProcessorCreationGate.LauncherProbeResolution, stage, ex);
        }
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Step 26 invokes exactly Harmony.CreateProcessor(MethodBase) from the verified post-publish Harmony API surface; no Patch method is invoked.")]
    [UnconditionalSuppressMessage("Trimming", "IL2075", Justification = "Step 26 retains exact post-publish reflection objects verified by Gates J-L.")]
    public ControlledHarmonyProcessorCreationGateResult RunHarmonyProcessorCreation()
    {
        var stage = "Harmony PatchProcessor creation";
        try
        {
            ThrowIfDisposed();
            var preflight = RequirePreflight();
            var harmonyApi = RequireHarmonyApi();
            var processorApi = RequireProcessorApi();
            _ = RequireProcessorTypeInitialization();
            var probe = RequireLauncherProbe();
            var context = RequireLoadContext();
            var harmonyInstance = _harmonyInstance ?? throw new InvalidOperationException("Step 26 retained Harmony instance is missing after the Step 25 replay.");

            VerifyFileLength(preflight.Target.PreparedPath, preflight.Target.Plan.Length, "prepared processor-creation target");
            var targetSha1 = ComputeSha1Hex(preflight.Target.PreparedPath);
            if (!targetSha1.Equals(preflight.Target.Plan.Sha1Hex, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Step 26 0Harmony SHA-1 changed immediately before CreateProcessor.");
            if (harmonyApi.DebugField.GetValue(null) is not bool debug || debug)
                throw new InvalidDataException("Step 26 refuses processor creation because Harmony.DEBUG is true.");

            var managedBefore = context.ManagedResolverRequests.Count;
            var privateBefore = context.PrivateLoads.Count;
            var hostBefore = context.HostLoads.Count;
            var nativeBefore = context.NativeLoadAttempts.Count;
            var membershipBefore = context.Assemblies.Select(a => a.GetName().FullName ?? a.GetName().Name ?? string.Empty).OrderBy(v => v, StringComparer.Ordinal).ToArray();

            stage = "exact Harmony.CreateProcessor(MethodBase) invocation";
            object processor;
            try
            {
                processor = processorApi.CreateProcessorMethod.Invoke(harmonyInstance, [probe.Method])
                    ?? throw new InvalidOperationException("Harmony.CreateProcessor returned null.");
            }
            catch (TargetInvocationException ex) when (ex.InnerException is not null)
            {
                throw new InvalidOperationException("Exact Harmony.CreateProcessor(MethodBase) invocation threw: " + ex.InnerException.GetType().FullName + ": " + ex.InnerException.Message, ex.InnerException);
            }

            stage = "empty PatchProcessor verification";
            if (!ReferenceEquals(processor.GetType(), processorApi.PatchProcessorType))
                throw new InvalidDataException("Step 26 CreateProcessor returned an unexpected runtime type.");
            if (!ReferenceEquals(AssemblyLoadContext.GetLoadContext(processor.GetType().Assembly), context))
                throw new InvalidDataException("Step 26 PatchProcessor type escaped the dedicated private load context.");
            if (!ReferenceEquals(processorApi.InstanceField.GetValue(processor), harmonyInstance))
                throw new InvalidDataException("Step 26 PatchProcessor did not retain the exact proven Harmony instance.");
            if (!ReferenceEquals(processorApi.OriginalField.GetValue(processor), probe.Method))
                throw new InvalidDataException("Step 26 PatchProcessor did not retain the exact launcher-owned probe MethodBase.");

            if (context.NativeLoadAttempts.Count != nativeBefore)
                throw new DllNotFoundException("CreateProcessor attempted native resolution: " + string.Join(" | ", context.NativeLoadAttempts.Skip(nativeBefore)));
            if (context.RejectedManagedRequests.Count != 0)
                throw new FileLoadException("CreateProcessor triggered an unplanned managed request: " + string.Join(" | ", context.RejectedManagedRequests));
            var membershipAfter = context.Assemblies.Select(a => a.GetName().FullName ?? a.GetName().Name ?? string.Empty).OrderBy(v => v, StringComparer.Ordinal).ToArray();
            if (!membershipAfter.SequenceEqual(membershipBefore, StringComparer.Ordinal))
                throw new InvalidDataException("CreateProcessor changed private-context membership.");
            var postSha1 = ComputeSha1Hex(preflight.Target.PreparedPath);
            if (!postSha1.Equals(targetSha1, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("0Harmony prepared bytes changed across CreateProcessor.");

            _patchProcessorInstance = processor;
            _processorCreation = new ProcessorCreationSnapshot(
                postSha1,
                context.ManagedResolverRequests.Count - managedBefore,
                context.PrivateLoads.Count - privateBefore,
                context.HostLoads.Count - hostBefore,
                context.NativeLoadAttempts.Count - nativeBefore);

            return Pass(
                ControlledHarmonyProcessorCreationGate.HarmonyProcessorCreation,
                "CONTROLLED EMPTY HARMONY PATCHPROCESSOR CREATION SUCCEEDED.\n" +
                "Factory invoked: HarmonyLib.Harmony::CreateProcessor(System.Reflection.MethodBase)\n" +
                $"Target: {probe.Signature}\n" +
                "Returned type: HarmonyLib.PatchProcessor\n" +
                "Retained Harmony instance: EXACT\n" +
                "Retained original MethodBase: EXACT launcher-owned probe\n" +
                $"Managed resolver requests during creation: {_processorCreation.ManagedResolverRequestsDuringCreation:N0}\n" +
                $"Private loads during creation: {_processorCreation.PrivateLoadsDuringCreation:N0}\n" +
                $"Host loads during creation: {_processorCreation.HostLoadsDuringCreation:N0}\n" +
                $"Native load attempts during creation: {_processorCreation.NativeLoadAttemptsDuringCreation:N0}\n" +
                "Private-context membership changed: NO\n" +
                "PatchProcessor.Patch invoked: NO\n" +
                "Harmony.Patch/PatchAll invoked: NO\n" +
                "StS2 type/member reflected or invoked: NO\n" +
                "Launcher probe method invoked: NO");
        }
        catch (Exception ex)
        {
            return Fail(ControlledHarmonyProcessorCreationGate.HarmonyProcessorCreation, stage, ex);
        }
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Step 26 final audit reads only exact reflection fields/members already resolved and verified by earlier gates.")]
    [UnconditionalSuppressMessage("Trimming", "IL2075", Justification = "Step 26 final audit retains only exact post-publish reflection objects bounded by Gates J-M.")]
    public async Task<ControlledHarmonyProcessorCreationGateResult> RunPostProcessorAuditAsync(
        IProgress<ControlledHarmonyProcessorCreationProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var stage = "post-processor audit";
        try
        {
            ThrowIfDisposed();
            var preflight = RequirePreflight();
            var harmonyApi = RequireHarmonyApi();
            var processorApi = RequireProcessorApi();
            var processorTypeInitialization = RequireProcessorTypeInitialization();
            var probe = RequireLauncherProbe();
            var creation = RequireProcessorCreation();
            var context = RequireLoadContext();
            var harmonyInstance = _harmonyInstance ?? throw new InvalidOperationException("Step 26 retained Harmony instance is missing.");
            var processor = _patchProcessorInstance ?? throw new InvalidOperationException("Step 26 retained PatchProcessor instance is missing.");

            stage = "runtime plan rehash";
            var planSha256 = await ComputeSha256HexAsync(_planPath, cancellationToken).ConfigureAwait(false);
            if (!planSha256.Equals(preflight.PlanSha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Step 26 runtime-binding plan changed during processor creation.");

            stage = "prepared/live byte audit";
            var verified = 0;
            foreach (var item in preflight.PreparedAssemblies)
            {
                cancellationToken.ThrowIfCancellationRequested();
                VerifyFileLength(item.PreparedPath, item.Plan.Length, "prepared post-processor");
                VerifyFileLength(item.LivePath, item.Plan.Length, "live post-processor");
                var preparedSha1 = await ComputeSha1HexAsync(item.PreparedPath, cancellationToken).ConfigureAwait(false);
                var liveSha1 = await ComputeSha1HexAsync(item.LivePath, cancellationToken).ConfigureAwait(false);
                if (!preparedSha1.Equals(item.Plan.Sha1Hex, StringComparison.OrdinalIgnoreCase) || !liveSha1.Equals(item.Plan.Sha1Hex, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException("Step 26 prepared/live byte identity changed during processor creation: " + item.Plan.RelativePath);
                verified++;
                progress?.Report(new ControlledHarmonyProcessorCreationProgress(
                    ControlledHarmonyProcessorCreationGate.PostProcessorAudit,
                    verified,
                    preflight.PreparedAssemblies.Length,
                    item.Plan.RelativePath,
                    "Re-hashing prepared/live bytes after the empty PatchProcessor boundary…"));
            }

            stage = "OfflineReady postcondition";
            var offline = await _offlineInspection.RunAsync(null, cancellationToken).ConfigureAwait(false);
            if (!offline.Success || !offline.ExactManagedTreeVerified || offline.InstalledManifestId != preflight.Plan.ManifestId)
                throw new InvalidDataException(offline.Error ?? "OfflineReady exact-tree verification failed after Step 26 processor creation.");

            stage = "private context membership audit";
            var expected = preflight.PreparedAssemblies
                .Where(item => item.ModuleInitializerCount == 0 || item.Plan.AssemblyFullName.Equals(preflight.Target.Plan.AssemblyFullName, StringComparison.Ordinal))
                .Select(item => item.Plan.AssemblyFullName)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            var actual = context.Assemblies.Select(a => a.GetName().FullName ?? a.GetName().Name ?? string.Empty).OrderBy(v => v, StringComparer.Ordinal).ToArray();
            if (!actual.SequenceEqual(expected, StringComparer.Ordinal))
                throw new InvalidDataException("Step 26 post-processor private context differs from the physically proven Step 25 context membership.");
            if (context.NativeLoadAttempts.Count != 0)
                throw new DllNotFoundException("Step 26 observed native-library resolution: " + string.Join(" | ", context.NativeLoadAttempts));
            if (context.RejectedManagedRequests.Count != 0)
                throw new FileLoadException("Step 26 observed rejected/unplanned managed requests: " + string.Join(" | ", context.RejectedManagedRequests));

            stage = "retained processor identity audit";
            if (!ReferenceEquals(processor.GetType(), processorApi.PatchProcessorType) || !ReferenceEquals(processorApi.InstanceField.GetValue(processor), harmonyInstance) || !ReferenceEquals(processorApi.OriginalField.GetValue(processor), probe.Method))
                throw new InvalidDataException("Step 26 retained PatchProcessor state changed after creation.");
            if (harmonyApi.DebugField.GetValue(null) is not bool debug || debug)
                throw new InvalidDataException("Step 26 Harmony.DEBUG is true during final processor audit.");
            var targetSha1 = await ComputeSha1HexAsync(preflight.Target.PreparedPath, cancellationToken).ConfigureAwait(false);
            if (!targetSha1.Equals(processorTypeInitialization.PreparedSha1, StringComparison.OrdinalIgnoreCase) || !targetSha1.Equals(creation.PreparedSha1, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Step 26 0Harmony prepared hash changed across PatchProcessor type initialization/creation.");

            return Pass(
                ControlledHarmonyProcessorCreationGate.PostProcessorAudit,
                "Step 26 post-processor isolation audit passed.\n" +
                $"Prepared/live assemblies re-hashed: {verified:N0}/{preflight.PreparedAssemblies.Length:N0}\n" +
                $"Runtime plan SHA-256 unchanged: {planSha256}\n" +
                $"Private context: {actual.Length:N0}/{expected.Length:N0} expected assemblies\n" +
                $"Harmony.Id: {HarmonyId}\n" +
                $"PatchProcessor target: {probe.Signature}\n" +
                "PatchProcessor retained Harmony/original fields: EXACT\n" +
                "Harmony.DEBUG: false\n" +
                $"Native load attempts: {context.NativeLoadAttempts.Count:N0}\n" +
                $"Rejected/unplanned managed requests: {context.RejectedManagedRequests.Count:N0}\n" +
                "Post-processor OfflineReady exact-tree verification: YES\n" +
                "Trusted Step 12 managed install unchanged: YES\n" +
                "Prepared Step 21/22 bytes unchanged: YES\n" +
                "PatchProcessor type initialization: YES — exact measured locker initializer only\n" +
                "PatchProcessor object construction: YES — exact CreateProcessor(MethodBase) only\n" +
                "PatchProcessor.Patch/Harmony.Patch/PatchAll: NOT INVOKED\n" +
                "Launcher probe method: NOT INVOKED\n" +
                "StS2 entry point/type/member reflection or invocation: NO\n" +
                "Godot/game initialization: NO\n" +
                "Native game library loaded by Step 26: NO\n" +
                "Process note: the Step 26 private managed context remains resident until process exit; force-quit before rerunning earlier fresh-process CLR-load regressions.");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return Fail(ControlledHarmonyProcessorCreationGate.PostProcessorAudit, stage, ex);
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;
        ClearStep26ObjectState();
        ReleaseLoadContext();
        _step23Preflight.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    private void ClearStep26ObjectState()
    {
        _patchProcessorInstance = null;
        _processorCreation = null;
        _launcherProbe = null;
        _processorTypeInitialization = null;
        _processorApi = null;
        _harmonyInstance = null;
        _harmonyApi = null;
        _harmonyTypeInitialization = null;
        _harmonyConstruction = null;
        _initialization = null;
        _replay = null;
    }

    private void ReleaseLoadContext()
    {
        var context = _loadContext;
        _loadContext = null;
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
            throw new InvalidDataException("Step 26 requires a fresh process; a game/Harmony assembly is already loaded: " + string.Join(" | ", matches));
    }


    private static HarmonyProcessorMetadataSnapshot ReadHarmonyProcessorMetadata(string path)
    {
        using var resolver = new Step26MetadataOnlyResolver(path);
        using var module = ModuleDefinition.ReadModule(path, new ReaderParameters
        {
            InMemory = true,
            ReadSymbols = false,
            ReadingMode = ReadingMode.Deferred,
            AssemblyResolver = resolver,
            MetadataResolver = resolver,
        });
        if (module.Assembly?.Name is null)
            throw new BadImageFormatException("Managed assembly manifest missing while auditing Harmony processor API: " + path);

        var harmonyType = EnumerateTypes(module.Types).SingleOrDefault(type => type.FullName.Equals(HarmonyTypeFullName, StringComparison.Ordinal));
        var processorType = EnumerateTypes(module.Types).SingleOrDefault(type => type.FullName.Equals(PatchProcessorTypeFullName, StringComparison.Ordinal));
        if (harmonyType is null || processorType is null)
            return new HarmonyProcessorMetadataSnapshot(false, "Harmony or PatchProcessor type missing from exact 0Harmony.", "<missing>", "<missing>", "<missing>");
        if (!processorType.IsPublic || processorType.IsAbstract || processorType.IsInterface || processorType.BaseType?.FullName != "System.Object")
            return new HarmonyProcessorMetadataSnapshot(false, "HarmonyLib.PatchProcessor is not the expected public non-abstract class.", "<invalid type>", "<invalid type>", "<invalid type>");

        var createCandidates = harmonyType.Methods.Where(method => method.IsPublic && !method.IsStatic && method.Name.Equals("CreateProcessor", StringComparison.Ordinal)).ToArray();
        var createProcessor = createCandidates.SingleOrDefault(method =>
            method.Parameters.Count == 1 &&
            method.Parameters[0].ParameterType.FullName.Equals("System.Reflection.MethodBase", StringComparison.Ordinal) &&
            method.ReturnType.FullName.Equals(PatchProcessorTypeFullName, StringComparison.Ordinal));
        if (createProcessor is null || createCandidates.Length != 1 || !createProcessor.HasBody)
            return new HarmonyProcessorMetadataSnapshot(false, $"Expected exactly one managed public Harmony.CreateProcessor(MethodBase); observed {createCandidates.Length}.", createProcessor?.FullName ?? "<missing>", "<not audited>", "<not audited>");

        var constructors = processorType.Methods.Where(method => method.IsConstructor && !method.IsStatic && method.IsPublic).ToArray();
        var constructor = constructors.SingleOrDefault(method =>
            method.Parameters.Count == 2 &&
            method.Parameters[0].ParameterType.FullName.Equals(HarmonyTypeFullName, StringComparison.Ordinal) &&
            method.Parameters[1].ParameterType.FullName.Equals("System.Reflection.MethodBase", StringComparison.Ordinal));
        if (constructor is null || constructors.Length != 1 || !constructor.HasBody)
            return new HarmonyProcessorMetadataSnapshot(false, $"Expected exactly one managed public PatchProcessor(Harmony,MethodBase) constructor; observed {constructors.Length}.", FormatMethodAudit(createProcessor), constructor?.FullName ?? "<missing>", "<not audited>");

        var instanceField = processorType.Fields.SingleOrDefault(field => !field.IsStatic && field.Name.Equals("instance", StringComparison.Ordinal));
        var originalField = processorType.Fields.SingleOrDefault(field => !field.IsStatic && field.Name.Equals("original", StringComparison.Ordinal));
        if (instanceField is null || originalField is null ||
            !instanceField.FieldType.FullName.Equals(HarmonyTypeFullName, StringComparison.Ordinal) ||
            !originalField.FieldType.FullName.Equals("System.Reflection.MethodBase", StringComparison.Ordinal))
            return new HarmonyProcessorMetadataSnapshot(false, "PatchProcessor retained instance/original fields no longer match the measured 2.4.2 types.", FormatMethodAudit(createProcessor), FormatMethodAudit(constructor), "<not audited>");

        var typeInitializers = processorType.Methods.Where(method => method.IsConstructor && method.IsStatic).ToArray();
        if (typeInitializers.Length != 1 || !typeInitializers[0].HasBody)
            return new HarmonyProcessorMetadataSnapshot(false, $"Expected exactly one managed PatchProcessor type initializer; observed {typeInitializers.Length}.", FormatMethodAudit(createProcessor), FormatMethodAudit(constructor), typeInitializers.Length == 0 ? "<missing>" : string.Join("\n", typeInitializers.Select(m => m.FullName)));
        var typeInitializer = typeInitializers[0];

        var createIl = createProcessor.Body.Instructions;
        var createShape = createIl.Count == 4 &&
            createIl[0].OpCode.Code == Code.Ldarg_0 &&
            createIl[1].OpCode.Code == Code.Ldarg_1 &&
            createIl[2].OpCode.Code == Code.Newobj &&
            createIl[2].Operand is MethodReference createCtor &&
            createCtor.DeclaringType.FullName.Equals(PatchProcessorTypeFullName, StringComparison.Ordinal) &&
            createCtor.Name.Equals(".ctor", StringComparison.Ordinal) &&
            createCtor.Parameters.Count == 2 &&
            createCtor.Parameters[0].ParameterType.FullName.Equals(HarmonyTypeFullName, StringComparison.Ordinal) &&
            createCtor.Parameters[1].ParameterType.FullName.Equals("System.Reflection.MethodBase", StringComparison.Ordinal) &&
            createIl[3].OpCode.Code == Code.Ret;

        var ctorIl = constructor.Body.Instructions;
        var ctorShape = ctorIl.Count == 9 &&
            ctorIl[0].OpCode.Code == Code.Ldarg_0 &&
            ctorIl[1].OpCode.Code == Code.Call &&
            ctorIl[1].Operand is MethodReference objectCtor &&
            objectCtor.DeclaringType.FullName.Equals("System.Object", StringComparison.Ordinal) && objectCtor.Name.Equals(".ctor", StringComparison.Ordinal) &&
            ctorIl[2].OpCode.Code == Code.Ldarg_0 && ctorIl[3].OpCode.Code == Code.Ldarg_1 &&
            ctorIl[4].OpCode.Code == Code.Stfld && ctorIl[4].Operand is FieldReference instanceStore && instanceStore.Name.Equals("instance", StringComparison.Ordinal) &&
            ctorIl[5].OpCode.Code == Code.Ldarg_0 && ctorIl[6].OpCode.Code == Code.Ldarg_2 &&
            ctorIl[7].OpCode.Code == Code.Stfld && ctorIl[7].Operand is FieldReference originalStore && originalStore.Name.Equals("original", StringComparison.Ordinal) &&
            ctorIl[8].OpCode.Code == Code.Ret;

        var cctorIl = typeInitializer.Body.Instructions;
        var cctorShape = cctorIl.Count == 3 &&
            cctorIl[0].OpCode.Code == Code.Newobj &&
            cctorIl[0].Operand is MethodReference lockerCtor && lockerCtor.DeclaringType.FullName.Equals("System.Object", StringComparison.Ordinal) && lockerCtor.Name.Equals(".ctor", StringComparison.Ordinal) &&
            cctorIl[1].OpCode.Code == Code.Stsfld &&
            cctorIl[1].Operand is FieldReference lockerField && lockerField.DeclaringType.FullName.Equals(PatchProcessorTypeFullName, StringComparison.Ordinal) && lockerField.Name.Equals("locker", StringComparison.Ordinal) && lockerField.FieldType.FullName.Equals("System.Object", StringComparison.Ordinal) &&
            cctorIl[2].OpCode.Code == Code.Ret;

        var hazards = new List<string>();
        if (!createShape) hazards.Add("Harmony.CreateProcessor(MethodBase) no longer matches ldarg.0 → ldarg.1 → new PatchProcessor(this, original) → ret.");
        if (!ctorShape) hazards.Add("PatchProcessor(Harmony,MethodBase) no longer matches the measured field-storage-only constructor.");
        if (!cctorShape) hazards.Add("PatchProcessor::.cctor no longer matches new object() → locker → ret.");

        var createAudit = FormatMethodAudit(createProcessor);
        var ctorAudit = FormatMethodAudit(constructor);
        var cctorAudit = FormatMethodAudit(typeInitializer);
        var detail =
            $"Harmony.CreateProcessor overloads: {createCandidates.Length:N0}\n" +
            "Exact factory: CreateProcessor(System.Reflection.MethodBase) -> HarmonyLib.PatchProcessor\n" +
            $"PatchProcessor public constructors: {constructors.Length:N0}\n" +
            "Exact constructor: .ctor(HarmonyLib.Harmony,System.Reflection.MethodBase)\n" +
            "Retained fields: instance:HarmonyLib.Harmony; original:System.Reflection.MethodBase\n" +
            "PatchProcessor type initializer: new System.Object() → locker\n" +
            $"Blocking processor metadata hazards: {hazards.Count:N0}" +
            (hazards.Count == 0 ? string.Empty : "\n" + string.Join("\n", hazards));
        return new HarmonyProcessorMetadataSnapshot(hazards.Count == 0, detail, createAudit, ctorAudit, cctorAudit);
    }

    private static HarmonyConstructorMetadataSnapshot ReadHarmonyConstructorMetadata(string path)
    {
        using var resolver = new Step26MetadataOnlyResolver(path);
        using var module = ModuleDefinition.ReadModule(path, new ReaderParameters
        {
            InMemory = true,
            ReadSymbols = false,
            ReadingMode = ReadingMode.Deferred,
            AssemblyResolver = resolver,
            MetadataResolver = resolver,
        });
        if (module.Assembly?.Name is null)
            throw new BadImageFormatException("Managed assembly manifest missing while auditing Harmony constructor: " + path);

        var harmonyType = EnumerateTypes(module.Types)
            .SingleOrDefault(type => type.FullName.Equals(HarmonyTypeFullName, StringComparison.Ordinal));
        if (harmonyType is null)
            return new HarmonyConstructorMetadataSnapshot(false, "HarmonyLib.Harmony type is absent from exact 0Harmony.", "<type missing>", "<type missing>", 0, 0);
        if (!harmonyType.IsPublic || harmonyType.IsInterface || harmonyType.IsAbstract || harmonyType.BaseType?.FullName != "System.Object")
            return new HarmonyConstructorMetadataSnapshot(false, "HarmonyLib.Harmony is not a public non-abstract class.", "<invalid type shape>", "<invalid type shape>", 0, 0);

        var typeInitializers = harmonyType.Methods
            .Where(method => method.IsConstructor && method.IsStatic)
            .ToArray();
        if (typeInitializers.Length != 1 || !typeInitializers[0].HasBody)
        {
            return new HarmonyConstructorMetadataSnapshot(
                false,
                $"Step 26 requires exactly one managed HarmonyLib.Harmony type initializer; observed {typeInitializers.Length}.",
                "<constructor not audited>",
                typeInitializers.Length == 0 ? "<type initializer missing>" : string.Join("\n", typeInitializers.Select(method => method.HasBody ? FormatMethodAudit(method) : method.FullName)),
                0,
                0);
        }

        var typeInitializer = typeInitializers[0];
        var typeInitializerAudit = FormatMethodAudit(typeInitializer);
        var typeInitInstructions = typeInitializer.Body.Instructions;
        var typeInitializerShapeMatches =
            typeInitInstructions.Count == 3 &&
            typeInitInstructions[0].OpCode.Code == Code.Newobj &&
            typeInitInstructions[0].Operand is MethodReference typeInitCtor &&
            typeInitCtor.Name.Equals(".ctor", StringComparison.Ordinal) &&
            typeInitCtor.DeclaringType.FullName.StartsWith("System.Runtime.CompilerServices.ConditionalWeakTable`2<", StringComparison.Ordinal) &&
            typeInitInstructions[1].OpCode.Code == Code.Stsfld &&
            typeInitInstructions[1].Operand is FieldReference typeInitField &&
            typeInitField.DeclaringType.FullName.Equals(HarmonyTypeFullName, StringComparison.Ordinal) &&
            typeInitField.Name.Equals("AssemblyCachedCategories", StringComparison.Ordinal) &&
            typeInitField.FieldType.FullName.StartsWith("System.Runtime.CompilerServices.ConditionalWeakTable`2<", StringComparison.Ordinal) &&
            typeInitInstructions[2].OpCode.Code == Code.Ret;
        if (!typeInitializerShapeMatches)
        {
            return new HarmonyConstructorMetadataSnapshot(
                false,
                "HarmonyLib.Harmony::.cctor no longer matches the measured inert 2.4.2 static-cache initialization shape.",
                "<constructor not audited>",
                typeInitializerAudit,
                0,
                0);
        }

        var publicConstructors = harmonyType.Methods
            .Where(method => method.IsConstructor && !method.IsStatic && method.IsPublic)
            .ToArray();
        var constructor = publicConstructors.SingleOrDefault(method =>
            method.Parameters.Count == 1 &&
            method.Parameters[0].ParameterType.FullName.Equals("System.String", StringComparison.Ordinal));
        if (constructor is null || publicConstructors.Length != 1 || !constructor.HasBody)
        {
            return new HarmonyConstructorMetadataSnapshot(
                false,
                "Step 26 requires exactly one public HarmonyLib.Harmony constructor, .ctor(System.String), with managed IL.",
                string.Join("\n", publicConstructors.Select(method => method.HasBody ? FormatMethodAudit(method) : method.FullName)),
                typeInitializerAudit,
                publicConstructors.Length,
                0);
        }

        var idProperty = harmonyType.Properties.SingleOrDefault(property =>
            property.Name.Equals("Id", StringComparison.Ordinal) &&
            property.PropertyType.FullName.Equals("System.String", StringComparison.Ordinal));
        if (idProperty?.GetMethod is null || !idProperty.GetMethod.IsPublic)
            return new HarmonyConstructorMetadataSnapshot(false, "HarmonyLib.Harmony.Id public string getter is missing.", FormatMethodAudit(constructor), typeInitializerAudit, publicConstructors.Length, 0);

        var debugField = harmonyType.Fields.SingleOrDefault(field =>
            field.Name.Equals("DEBUG", StringComparison.Ordinal) &&
            field.FieldType.FullName.Equals("System.Boolean", StringComparison.Ordinal));
        if (debugField is null || !debugField.IsPublic || !debugField.IsStatic)
            return new HarmonyConstructorMetadataSnapshot(false, "HarmonyLib.Harmony.DEBUG public static bool field is missing.", FormatMethodAudit(constructor), typeInitializerAudit, publicConstructors.Length, 0);

        var instructions = constructor.Body.Instructions;
        var debugFieldLoadIndex = -1;
        var debugBlockEndIndex = -1;
        for (var i = 0; i < instructions.Count - 1; i++)
        {
            if (instructions[i].OpCode.Code != Code.Ldsfld || instructions[i].Operand is not FieldReference field ||
                !field.DeclaringType.FullName.Equals(HarmonyTypeFullName, StringComparison.Ordinal) ||
                !field.Name.Equals("DEBUG", StringComparison.Ordinal))
                continue;
            var branch = instructions[i + 1];
            if (branch.OpCode.Code is not (Code.Brfalse or Code.Brfalse_S) || branch.Operand is not Instruction branchTarget)
                continue;
            debugFieldLoadIndex = i;
            debugBlockEndIndex = instructions.IndexOf(branchTarget);
            break;
        }

        var hasHarmonyDebugLiteral = instructions.Any(instruction =>
            instruction.OpCode.Code == Code.Ldstr &&
            instruction.Operand is string value &&
            value.Equals("HARMONY_DEBUG", StringComparison.Ordinal));
        var hasEnvironmentProbe = instructions.Any(instruction =>
            instruction.Operand is MethodReference method &&
            method.DeclaringType.FullName.Equals("System.Environment", StringComparison.Ordinal) &&
            method.Name.Equals("GetEnvironmentVariable", StringComparison.Ordinal));
        var writesId = instructions.Any(instruction =>
            (instruction.OpCode.Code == Code.Stfld && instruction.Operand is FieldReference field &&
             field.DeclaringType.FullName.Equals(HarmonyTypeFullName, StringComparison.Ordinal) &&
             field.Name.Contains("Id", StringComparison.Ordinal)) ||
            (instruction.Operand is MethodReference method &&
             method.DeclaringType.FullName.Equals(HarmonyTypeFullName, StringComparison.Ordinal) &&
             method.Name.Equals("set_Id", StringComparison.Ordinal)));

        var hazards = new List<string>();
        if (!hasHarmonyDebugLiteral || !hasEnvironmentProbe)
            hazards.Add("Harmony constructor no longer contains the expected HARMONY_DEBUG environment probe.");
        if (debugFieldLoadIndex < 0 || debugBlockEndIndex <= debugFieldLoadIndex + 1)
            hazards.Add("Harmony constructor no longer has the expected DEBUG=false branch guard around debug-only work.");
        if (!writesId)
            hazards.Add("Harmony constructor no longer writes the Id property/backing field.");

        var conditionalDebugCalls = 0;
        for (var i = 0; i < instructions.Count; i++)
        {
            var instruction = instructions[i];
            if (instruction.OpCode.Code is Code.Calli or Code.Ldftn or Code.Ldvirtftn or Code.Jmp)
                hazards.Add($"Unbounded indirect execution opcode in Harmony constructor at {instruction.Offset:X4}: {instruction.OpCode.Code}");

            if (instruction.Operand is not MethodReference called ||
                instruction.OpCode.Code is not (Code.Call or Code.Callvirt or Code.Newobj))
                continue;

            var inDebugOnlyBlock = debugFieldLoadIndex >= 0 && i > debugFieldLoadIndex + 1 && i < debugBlockEndIndex;
            var scopeName = GetMethodScopeName(called);
            var sameAssembly = scopeName.Equals(module.Assembly.Name.Name, StringComparison.OrdinalIgnoreCase);
            var allowedLocalSetter = sameAssembly &&
                called.DeclaringType.FullName.Equals(HarmonyTypeFullName, StringComparison.Ordinal) &&
                called.Name.Equals("set_Id", StringComparison.Ordinal);

            if (inDebugOnlyBlock)
            {
                conditionalDebugCalls++;
                continue;
            }
            if (allowedLocalSetter || IsHostFrameworkContractName(scopeName))
                continue;
            if (sameAssembly)
            {
                hazards.Add("Same-assembly Harmony call reachable with DEBUG=false outside the measured debug branch: " + called.FullName);
                continue;
            }
            hazards.Add("Unexpected non-framework constructor execution edge: " + called.FullName + " [scope=" + scopeName + "]");
        }

        if (conditionalDebugCalls == 0)
            hazards.Add("Harmony constructor DEBUG branch contains no conditional calls; expected 2.4.2 debug-only structure was not observed.");

        var audit = FormatMethodAudit(constructor);
        var detail =
            $"Harmony type: {harmonyType.FullName}\n" +
            $"Public instance constructors: {publicConstructors.Length:N0}\n" +
            "Exact constructor: .ctor(System.String)\n" +
            "Harmony type initializer: EXACT measured static-cache shape\n" +
            "Harmony type initializer operation: ConditionalWeakTable<...> construction → AssemblyCachedCategories\n" +
            "HARMONY_DEBUG environment probe: PRESENT\n" +
            "DEBUG=false branch guard: PRESENT\n" +
            $"Conditionally dormant calls inside DEBUG branch: {conditionalDebugCalls:N0}\n" +
            "Id write: PRESENT\n" +
            $"Blocking constructor/type-initializer metadata hazards: {hazards.Count:N0}" +
            (hazards.Count == 0 ? string.Empty : "\n" + string.Join("\n", hazards));

        return new HarmonyConstructorMetadataSnapshot(hazards.Count == 0, detail, audit, typeInitializerAudit, publicConstructors.Length, conditionalDebugCalls);
    }

    private static PreparedMetadataSnapshot ReadPreparedMetadata(string path, bool includeInitializerCallGraph, string targetSimpleName)
    {
        using var resolver = new Step26MetadataOnlyResolver(path);
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


    internal static InitializerHazardPolicyDecision EvaluateInitializerHazardPolicy(
        string assemblySimpleName,
        Version assemblyVersion,
        IReadOnlyCollection<string> hazards,
        IReadOnlyCollection<string> automaticInitializerAudits,
        bool debuggerAttached,
        IReadOnlyCollection<string> monoModEnvironmentOverrideNames,
        IReadOnlyCollection<string> monoModAppContextOverrideNames)
    {
        var orderedHazards = hazards.OrderBy(value => value, StringComparer.Ordinal).ToArray();
        if (orderedHazards.Length == 0)
        {
            return new InitializerHazardPolicyDecision(
                true,
                0,
                0,
                "No conservative initializer findings require conditional classification.");
        }

        // Synthetic targets and any future/deviating real target retain the original hard fail-closed
        // behavior. The conditional path exists only for the physically measured merged 0Harmony
        // 2.4.2 logger fingerprint from Step 24.0.4.
        if (!assemblySimpleName.Equals(TargetSimpleName, StringComparison.OrdinalIgnoreCase) || assemblyVersion != TargetVersion)
        {
            return new InitializerHazardPolicyDecision(
                false,
                orderedHazards.Length,
                0,
                "Conditional dispatch classification is unavailable for any target other than exact 0Harmony 2.4.2.0.");
        }

        var expected = ObservedMonoModLoggingDispatchHazards.OrderBy(value => value, StringComparer.Ordinal).ToArray();
        if (!orderedHazards.SequenceEqual(expected, StringComparer.Ordinal))
        {
            var missing = expected.Except(orderedHazards, StringComparer.Ordinal).ToArray();
            var additional = orderedHazards.Except(expected, StringComparer.Ordinal).ToArray();
            return new InitializerHazardPolicyDecision(
                false,
                orderedHazards.Length,
                0,
                "The conservative hazard fingerprint differs from the physically measured Step 24.0.4 set. " +
                $"Missing={FormatNames(missing)}; additional={FormatNames(additional)}.");
        }

        var auditNames = automaticInitializerAudits.Select(GetAuditedMethodName).ToArray();
        var requiredAuditNames = new[]
        {
            "System.Void <Module>::.cctor()",
            "System.Void MonoMod.Switches::.cctor()",
            "System.Void MonoMod.Logs.DebugLog::.cctor()",
            "System.Void MonoMod.Logs.DebugLog/LevelSubscriptions::.cctor()",
        };
        if (auditNames.Length != requiredAuditNames.Length ||
            !requiredAuditNames.All(required => auditNames.Contains(required, StringComparer.Ordinal)))
        {
            return new InitializerHazardPolicyDecision(
                false,
                orderedHazards.Length,
                0,
                "The automatic type-initializer set differs from the physically measured four-method MonoMod logging shape: " +
                FormatNames(auditNames));
        }

        var moduleAudit = automaticInitializerAudits.Single(value => value.StartsWith("method=System.Void <Module>::.cctor();", StringComparison.Ordinal));
        var switchesAudit = automaticInitializerAudits.Single(value => value.StartsWith("method=System.Void MonoMod.Switches::.cctor();", StringComparison.Ordinal));
        var debugLogAudit = automaticInitializerAudits.Single(value => value.StartsWith("method=System.Void MonoMod.Logs.DebugLog::.cctor();", StringComparison.Ordinal));
        var levelSubscriptionsAudit = automaticInitializerAudits.Single(value => value.StartsWith("method=System.Void MonoMod.Logs.DebugLog/LevelSubscriptions::.cctor();", StringComparison.Ordinal));
        if (!moduleAudit.Contains("instructions=2", StringComparison.Ordinal) ||
            !moduleAudit.Contains("MMDbgLog::LogVersion()", StringComparison.Ordinal) ||
            !moduleAudit.Contains("IL_0005: Ret", StringComparison.Ordinal) ||
            !switchesAudit.Contains("instructions=48", StringComparison.Ordinal) ||
            !switchesAudit.Contains("System.Environment::GetEnvironmentVariables()", StringComparison.Ordinal) ||
            !switchesAudit.Contains("MonoMod.Switches::BestEffortParseEnvVar", StringComparison.Ordinal) ||
            !debugLogAudit.Contains("instructions=15", StringComparison.Ordinal) ||
            !debugLogAudit.Contains("MonoMod.Logs.DebugLog::.ctor()", StringComparison.Ordinal) ||
            !debugLogAudit.Contains("MonoMod.Logs.DebugLog::Instance", StringComparison.Ordinal) ||
            !debugLogAudit.Contains("MonoMod.Logs.DebugLog::simpleRegDict", StringComparison.Ordinal) ||
            !levelSubscriptionsAudit.Contains("instructions=3", StringComparison.Ordinal) ||
            !levelSubscriptionsAudit.Contains("MonoMod.Logs.DebugLog/LevelSubscriptions::.ctor()", StringComparison.Ordinal) ||
            !levelSubscriptionsAudit.Contains("MonoMod.Logs.DebugLog/LevelSubscriptions::None", StringComparison.Ordinal))
        {
            return new InitializerHazardPolicyDecision(
                false,
                orderedHazards.Length,
                0,
                "One or more automatic type initializers no longer match the physically measured Step 24.0.4 structural shape.");
        }

        var blockers = new List<string>();
        if (debuggerAttached)
            blockers.Add("managed debugger is attached");
        if (monoModEnvironmentOverrideNames.Count != 0)
            blockers.Add("MONOMOD_* environment override name(s): " + FormatNames(monoModEnvironmentOverrideNames));
        if (monoModAppContextOverrideNames.Count != 0)
            blockers.Add("MonoMod logging AppContext override name(s): " + FormatNames(monoModAppContextOverrideNames));
        if (blockers.Count != 0)
        {
            return new InitializerHazardPolicyDecision(
                false,
                orderedHazards.Length,
                0,
                "The physically measured logger dispatches are only classified dormant in the default inert logger state. " +
                string.Join("; ", blockers) + ". Values are intentionally not reported.");
        }

        return new InitializerHazardPolicyDecision(
            true,
            0,
            orderedHazards.Length,
            "Exact Step 24.0.4 MonoMod logger dispatch fingerprint: MATCH\n" +
            "Debugger attached: NO\n" +
            "MONOMOD_* environment override names present: NO\n" +
            "MonoMod logging AppContext override names present: NO\n" +
            "Policy note: these seven conservative dispatch findings are conditionally dormant for this exact measured initialization shape; P/Invoke/calli/native/reflection/dynamic/unresolved or any changed/additional edge remains blocking.");
    }

    private static string GetAuditedMethodName(string audit)
    {
        const string Prefix = "method=";
        var start = audit.StartsWith(Prefix, StringComparison.Ordinal) ? Prefix.Length : 0;
        var end = audit.IndexOf(';', start);
        return end >= 0 ? audit[start..end] : audit[start..];
    }

    private static string FormatNames(IEnumerable<string> names)
    {
        var ordered = names.Where(value => !string.IsNullOrWhiteSpace(value)).OrderBy(value => value, StringComparer.Ordinal).ToArray();
        return ordered.Length == 0 ? "<none>" : string.Join(" | ", ordered);
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
        => _initialization ?? throw new InvalidOperationException("Step 26 Gate C must pass before later Step 26 gates run.");

    private HarmonyApiSnapshot RequireHarmonyApi()
        => _harmonyApi ?? throw new InvalidOperationException("Step 26 Gate E must pass before Harmony type initialization runs.");

    private HarmonyTypeInitializationSnapshot RequireHarmonyTypeInitialization()
        => _harmonyTypeInitialization ?? throw new InvalidOperationException("Step 26 Gate F must pass before later Harmony gates run.");

    private HarmonyProcessorCreationSnapshot RequireHarmonyProcessorCreation()
        => _harmonyConstruction ?? throw new InvalidOperationException("Step 26 Gate H must pass before the Step 25 replay audit runs.");

    private HarmonyProcessorApiSnapshot RequireProcessorApi()
        => _processorApi ?? throw new InvalidOperationException("Step 26 Gate J must pass before PatchProcessor type initialization.");

    private PatchProcessorTypeInitializationSnapshot RequireProcessorTypeInitialization()
        => _processorTypeInitialization ?? throw new InvalidOperationException("Step 26 Gate K must pass before launcher probe resolution or processor construction.");

    private LauncherProbeSnapshot RequireLauncherProbe()
        => _launcherProbe ?? throw new InvalidOperationException("Step 26 Gate L must pass before PatchProcessor construction.");

    private ProcessorCreationSnapshot RequireProcessorCreation()
        => _processorCreation ?? throw new InvalidOperationException("Step 26 Gate M must pass before the final processor audit.");

    private Step26LoadContext RequireLoadContext()
        => _loadContext ?? throw new InvalidOperationException("Step 26 dedicated load context is unavailable.");

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(ControlledHarmonyProcessorCreation));
    }

    private static ControlledHarmonyProcessorCreationGateResult Pass(ControlledHarmonyProcessorCreationGate gate, string detail)
        => new(gate, true, detail);

    private static ControlledHarmonyProcessorCreationGateResult Fail(ControlledHarmonyProcessorCreationGate gate, string stage, Exception ex)
        => new(gate, false, $"Stage: {stage}\n{ex}");

    private sealed class Step26MetadataOnlyResolver(string auditedPath) : IAssemblyResolver, IMetadataResolver
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

    internal sealed record InitializerHazardPolicyDecision(
        bool Allowed,
        int BlockingHazardCount,
        int ConditionalHazardCount,
        string Detail);

    private sealed record HarmonyConstructorMetadataSnapshot(
        bool Allowed,
        string Detail,
        string ConstructorAudit,
        string TypeInitializerAudit,
        int PublicConstructorCount,
        int ConditionalDebugCallCount);

    private sealed record HarmonyProcessorMetadataSnapshot(
        bool Allowed,
        string Detail,
        string CreateProcessorAudit,
        string PatchProcessorConstructorAudit,
        string PatchProcessorTypeInitializerAudit);

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
        HarmonyConstructorMetadataSnapshot HarmonyConstructorMetadata,
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
        Assembly TargetAssembly,
        string AssemblyFullName,
        string PreparedSha1,
        int ManagedResolverRequestsDuringInitialization,
        int PrivateLoadsDuringInitialization,
        int HostLoadsDuringInitialization,
        int NativeLoadAttemptsDuringInitialization);

    private sealed record HarmonyApiSnapshot(
        Type HarmonyType,
        ConstructorInfo TypeInitializer,
        ConstructorInfo Constructor,
        PropertyInfo IdProperty,
        FieldInfo DebugField,
        string AssemblyFullName);

    private sealed record HarmonyTypeInitializationSnapshot(
        string PreparedSha1,
        int ManagedResolverRequestsDuringTypeInitialization,
        int PrivateLoadsDuringTypeInitialization,
        int HostLoadsDuringTypeInitialization,
        int NativeLoadAttemptsDuringTypeInitialization);

    private sealed record HarmonyProcessorCreationSnapshot(
        string Id,
        string PreparedSha1,
        int ManagedResolverRequestsDuringConstruction,
        int PrivateLoadsDuringConstruction,
        int HostLoadsDuringConstruction,
        int NativeLoadAttemptsDuringConstruction);

    private sealed record HarmonyProcessorApiSnapshot(
        Type PatchProcessorType,
        ConstructorInfo TypeInitializer,
        ConstructorInfo Constructor,
        MethodInfo CreateProcessorMethod,
        FieldInfo InstanceField,
        FieldInfo OriginalField,
        string CreateProcessorAudit,
        string PatchProcessorConstructorAudit,
        string PatchProcessorTypeInitializerAudit);

    private sealed record PatchProcessorTypeInitializationSnapshot(
        string PreparedSha1,
        int ManagedResolverRequestsDuringTypeInitialization,
        int PrivateLoadsDuringTypeInitialization,
        int HostLoadsDuringTypeInitialization,
        int NativeLoadAttemptsDuringTypeInitialization);

    private sealed record LauncherProbeSnapshot(MethodInfo Method, string Signature);

    private sealed record ProcessorCreationSnapshot(
        string PreparedSha1,
        int ManagedResolverRequestsDuringCreation,
        int PrivateLoadsDuringCreation,
        int HostLoadsDuringCreation,
        int NativeLoadAttemptsDuringCreation);

    private sealed class Step26LoadContext : AssemblyLoadContext
    {
        private readonly IReadOnlyDictionary<string, PreparedAssemblySnapshot> _privateBySimpleName;
        private readonly RuntimeBindingHostFramework[] _hostBindings;

        public Step26LoadContext(
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
