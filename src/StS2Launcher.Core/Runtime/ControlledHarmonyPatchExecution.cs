using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace StS2Launcher.Core;

/// <summary>
/// Step 27 boundary. Replays the physically proven Step 26 empty PatchProcessor state in a fresh
/// private AssemblyLoadContext, then admits exactly one launcher-owned prefix, executes the first
/// real Harmony PatchProcessor.Patch() boundary against a launcher-owned inert target, proves the
/// patched behavior, removes exactly that prefix, and proves original behavior is restored. StS2
/// member reflection/invocation, patching of any game method, Godot startup, and native game-library
/// loading remain forbidden.
/// </summary>
public sealed class ControlledHarmonyPatchExecution : IDisposable
{
    public const string TargetSimpleName = "0Harmony";
    public static readonly Version TargetVersion = new(2, 4, 2, 0);
    public const string LoadContextName = "StS2Launcher-Step27-HarmonyPatchExecution";
    public const string HarmonyTypeFullName = "HarmonyLib.Harmony";
    public const string HarmonyId = "com.community.sts2launcher.step25.probe";
    public const string PatchProcessorTypeFullName = "HarmonyLib.PatchProcessor";
    public const string AccessToolsTypeFullName = "HarmonyLib.AccessTools";

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
    private HarmonyPatchApiSnapshot? _patchApi;
    private LauncherPatchProbeSnapshot? _patchProbe;
    private BaselineProbeInvocationSnapshot? _baselineProbeInvocation;
    private AccessToolsTypeInitializationSnapshot? _accessToolsTypeInitialization;
    private PrefixRegistrationSnapshot? _prefixRegistration;
    private PatchExecutionSnapshot? _patchExecution;
    private ProbeInvocationSnapshot? _patchedProbeInvocation;
    private UnpatchSnapshot? _unpatch;
    private ProbeInvocationSnapshot? _restoredProbeInvocation;
    private object? _harmonyInstance;
    private object? _patchProcessorInstance;
    private object? _prefixDescriptor;
    private MethodInfo? _replacementMethod;
    private bool _provenInitializationAuditPassed;
    private bool _provenHarmonyTypeInitializationAuditPassed;
    private bool _provenPostConstructionAuditPassed;
    private bool _provenPostProcessorAuditPassed;
    private bool _postPatchAuditPassed;
    private bool _postUnpatchAuditPassed;
    private Step27LoadContext? _loadContext;
    private bool _disposed;

    public ControlledHarmonyPatchExecution(string launcherDataRoot, bool collectibleLoadContext = false)
        : this(
            launcherDataRoot,
            collectibleLoadContext,
            FirstRealGameAssemblyLoad.ExpectedPrimarySimpleName,
            TargetSimpleName,
            TargetVersion,
            [FirstRealGameAssemblyLoad.ExpectedPrimarySimpleName, "SlayTheSpire2", TargetSimpleName])
    {
    }

    internal ControlledHarmonyPatchExecution(
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
        ClearStep27ObjectState();
        ReleaseLoadContext();
        _step23Preflight.Reset();
        _preflight = null;
        _provenInitializationAuditPassed = false;
        _provenHarmonyTypeInitializationAuditPassed = false;
        _provenPostConstructionAuditPassed = false;
        _provenPostProcessorAuditPassed = false;
        _postPatchAuditPassed = false;
        _postUnpatchAuditPassed = false;
    }

    public async Task<ControlledHarmonyPatchExecutionGateResult> RunInitializationPreflightAsync(
        IProgress<ControlledHarmonyPatchExecutionProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            EnsureFreshProcess();
            cancellationToken.ThrowIfCancellationRequested();

            stage = "accepted Step 23 preflight replay";
            progress?.Report(new ControlledHarmonyPatchExecutionProgress(
                ControlledHarmonyPatchExecutionGate.InitializationPreflight,
                0,
                0,
                null,
                "Re-running the physically proven Step 23 Gate A preflight before any Step 24 CLR load…"));

            _step23Preflight.Reset();
            var step23Result = await _step23Preflight.RunPreparedLoadPreflightAsync(
                progress is null
                    ? null
                    : new CallbackProgress<FirstRealGameAssemblyLoadProgress>(value =>
                        progress.Report(new ControlledHarmonyPatchExecutionProgress(
                            ControlledHarmonyPatchExecutionGate.InitializationPreflight,
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
                    "Step 27 Gate A requires HARMONY_DEBUG to be absent/empty so the exact Harmony constructor debug branch remains dormant. " +
                    "Observed value length: " + harmonyDebugEnvironment.Length.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }
            if (!harmonyConstructorMetadata.Allowed)
            {
                throw new InvalidDataException(
                    "Step 27 Gate A refuses Harmony construction because the exact constructor metadata policy did not pass:\n" +
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

            progress?.Report(new ControlledHarmonyPatchExecutionProgress(
                ControlledHarmonyPatchExecutionGate.InitializationPreflight,
                prepared.Count,
                prepared.Count,
                target.Plan.RelativePath,
                "The accepted Step 23 preflight still passes and the sole deferred initializer is exactly 0Harmony 2.4.2.0 with zero effective blocking hazards under the measured conditional policy."));

            return Pass(
                ControlledHarmonyPatchExecutionGate.InitializationPreflight,
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
                "No real game/Harmony assembly was loaded by Step 27 Gate A: YES");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return Fail(ControlledHarmonyPatchExecutionGate.InitializationPreflight, stage, ex);
        }
    }

    public ControlledHarmonyPatchExecutionGateResult RunProvenLoadStateReplay()
    {
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var preflight = RequirePreflight();
            EnsureFreshProcess();

            stage = "dedicated load context creation";
            var context = new Step27LoadContext(
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
                ControlledHarmonyPatchExecutionGate.ProvenLoadStateReplay,
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
            return Fail(ControlledHarmonyPatchExecutionGate.ProvenLoadStateReplay, stage, ex);
        }
    }

    public ControlledHarmonyPatchExecutionGateResult RunDeferredModuleInitialization()
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
                ControlledHarmonyPatchExecutionGate.DeferredModuleInitialization,
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
            return Fail(ControlledHarmonyPatchExecutionGate.DeferredModuleInitialization, stage, ex);
        }
    }

    public async Task<ControlledHarmonyPatchExecutionGateResult> RunProvenInitializationAuditAsync(
        IProgress<ControlledHarmonyPatchExecutionProgress>? progress = null,
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
                progress?.Report(new ControlledHarmonyPatchExecutionProgress(
                    ControlledHarmonyPatchExecutionGate.ProvenInitializationAudit,
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
                ControlledHarmonyPatchExecutionGate.ProvenInitializationAudit,
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
            return Fail(ControlledHarmonyPatchExecutionGate.ProvenInitializationAudit, stage, ex);
        }
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Step 27 intentionally resolves one exact receipt-verified post-publish Harmony type and API surface after metadata preflight.")]
    [UnconditionalSuppressMessage("Trimming", "IL2075", Justification = "The exact post-publish Harmony type is unavailable to the build-time trimmer; Gate A/E enforce the required constructor/member shape at runtime.")]
    public ControlledHarmonyPatchExecutionGateResult RunHarmonyApiResolution()
    {
        var stage = "Harmony API resolution";
        try
        {
            ThrowIfDisposed();
            var preflight = RequirePreflight();
            var initialization = RequireInitialization();
            var context = RequireLoadContext();
            if (!_provenInitializationAuditPassed)
                throw new InvalidOperationException("Step 27 Gate D must pass before resolving the Harmony API surface.");

            stage = "target assembly ownership";
            var targetAssembly = initialization.TargetAssembly;
            if (!ReferenceEquals(AssemblyLoadContext.GetLoadContext(targetAssembly), context))
                throw new InvalidDataException("Step 27 Harmony target is not owned by the dedicated Step 27 load context.");
            var actualIdentity = targetAssembly.GetName().FullName ?? targetAssembly.GetName().Name ?? string.Empty;
            if (!actualIdentity.Equals(preflight.Target.Plan.AssemblyFullName, StringComparison.Ordinal))
                throw new InvalidDataException("Step 27 Harmony target identity drifted before API resolution.");

            stage = "exact Harmony type resolution";
            var harmonyType = targetAssembly.GetType(HarmonyTypeFullName, throwOnError: true, ignoreCase: false)
                ?? throw new TypeLoadException("Step 27 could not resolve the exact HarmonyLib.Harmony type.");
            if (harmonyType.Assembly != targetAssembly || !harmonyType.IsClass || harmonyType.IsAbstract || !harmonyType.IsPublic)
                throw new InvalidDataException("Step 27 requires HarmonyLib.Harmony to be a public non-abstract class owned by exact 0Harmony.");
            var typeInitializer = harmonyType.TypeInitializer
                ?? throw new MissingMethodException("Step 27 requires the exact HarmonyLib.Harmony type initializer measured by Gate A; it is missing at runtime.");

            stage = "exact Harmony constructor resolution";
            var constructors = harmonyType.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            var publicConstructors = constructors.Where(ctor => ctor.IsPublic).ToArray();
            var constructor = publicConstructors.SingleOrDefault(ctor =>
            {
                var parameters = ctor.GetParameters();
                return parameters.Length == 1 && parameters[0].ParameterType == typeof(string);
            });
            if (constructor is null || publicConstructors.Length != 1)
                throw new MissingMethodException("Step 27 requires exactly one public HarmonyLib.Harmony instance constructor and it must be .ctor(System.String).");

            stage = "exact Harmony observation members";
            var idProperty = harmonyType.GetProperty("Id", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            if (idProperty is null || idProperty.PropertyType != typeof(string) || idProperty.GetMethod is null || !idProperty.GetMethod.IsPublic)
                throw new MissingMemberException("Step 27 requires the public instance Harmony.Id string getter for post-construction verification.");
            var debugField = harmonyType.GetField("DEBUG", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
            if (debugField is null || debugField.FieldType != typeof(bool))
                throw new MissingFieldException("Step 27 requires the public static Harmony.DEBUG boolean field for the inert-constructor precondition.");

            stage = "pre-type-initialization environment precondition";
            var harmonyDebugEnvironment = Environment.GetEnvironmentVariable("HARMONY_DEBUG");
            if (!string.IsNullOrEmpty(harmonyDebugEnvironment))
                throw new InvalidDataException("Step 27 refuses Harmony type initialization while HARMONY_DEBUG is non-empty.");

            _harmonyApi = new HarmonyApiSnapshot(harmonyType, typeInitializer, constructor, idProperty, debugField, actualIdentity);

            return Pass(
                ControlledHarmonyPatchExecutionGate.HarmonyApiResolution,
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
                "Harmony type initializer executed by Step 27: NO\n" +
                "Harmony object constructed: NO\n" +
                "Harmony patch API invoked: NO\n" +
                "Game type/member reflected or invoked: NO");
        }
        catch (Exception ex)
        {
            return Fail(ControlledHarmonyPatchExecutionGate.HarmonyApiResolution, stage, ex);
        }
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Step 27 explicitly executes only the exact Harmony type initializer whose IL shape was measured by Gate A.")]
    [UnconditionalSuppressMessage("Trimming", "IL2075", Justification = "Step 27 reads only exact reflection members resolved by Gate E after the explicit Harmony type-initialization barrier.")]
    public ControlledHarmonyPatchExecutionGateResult RunHarmonyTypeInitialization()
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
                throw new InvalidDataException("Step 27 target SHA-1 changed immediately before Harmony type initialization.");
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("HARMONY_DEBUG")))
                throw new InvalidDataException("Step 27 refuses Harmony type initialization because HARMONY_DEBUG became non-empty after Gate E.");

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
                throw new InvalidDataException("Step 27 Harmony.DEBUG is true after the explicit Harmony type initializer.");
            if (context.NativeLoadAttempts.Count != nativeAttemptsBefore)
                throw new DllNotFoundException("Harmony type initialization attempted native-library resolution: " + string.Join(" | ", context.NativeLoadAttempts.Skip(nativeAttemptsBefore)));
            if (context.RejectedManagedRequests.Count != 0)
                throw new FileLoadException("Harmony type initialization triggered an unplanned managed request: " + string.Join(" | ", context.RejectedManagedRequests));
            var contextMembershipAfter = context.Assemblies
                .Select(assembly => assembly.GetName().FullName ?? assembly.GetName().Name ?? string.Empty)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            if (!contextMembershipAfter.SequenceEqual(contextMembershipBefore, StringComparer.Ordinal))
                throw new InvalidDataException("Step 27 Harmony type initialization changed private-context assembly membership.");

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
                ControlledHarmonyPatchExecutionGate.HarmonyTypeInitialization,
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
            return Fail(ControlledHarmonyPatchExecutionGate.HarmonyTypeInitialization, stage, ex);
        }
    }

    [UnconditionalSuppressMessage("Trimming", "IL2075", Justification = "Step 27 Gate G reads only exact Harmony reflection members resolved by Gate E after Gate F completed the type initializer.")]
    public ControlledHarmonyPatchExecutionGateResult RunHarmonyTypeInitializationAudit()
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
                throw new InvalidDataException("Step 27 0Harmony hash drifted after the Harmony type initializer.");
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("HARMONY_DEBUG")))
                throw new InvalidDataException("Step 27 HARMONY_DEBUG became non-empty after Harmony type initialization.");
            if (api.DebugField.GetValue(null) is not bool debugValue || debugValue)
                throw new InvalidDataException("Step 27 Harmony.DEBUG is true during the post-type-initialization audit.");
            if (_harmonyInstance is not null)
                throw new InvalidOperationException("Step 27 Harmony object exists before the instance-construction gate.");

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
                throw new InvalidDataException("Step 27 Harmony type initialization changed the physically proven Step 24 context membership.");
            if (context.NativeLoadAttempts.Count != 0)
                throw new DllNotFoundException("Step 27 observed native-library resolution after Harmony type initialization: " + string.Join(" | ", context.NativeLoadAttempts));
            if (context.RejectedManagedRequests.Count != 0)
                throw new FileLoadException("Step 27 observed rejected/unplanned managed requests after Harmony type initialization: " + string.Join(" | ", context.RejectedManagedRequests));

            _provenHarmonyTypeInitializationAuditPassed = true;
            return Pass(
                ControlledHarmonyPatchExecutionGate.HarmonyTypeInitializationAudit,
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
            return Fail(ControlledHarmonyPatchExecutionGate.HarmonyTypeInitializationAudit, stage, ex);
        }
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Step 27 invokes one exact constructor from receipt-verified post-publish 0Harmony after metadata, API-shape, and explicit type-initialization preflight.")]
    [UnconditionalSuppressMessage("Trimming", "IL2075", Justification = "Step 27 retains only exact reflection objects resolved and verified by Gate E after the explicit type-initialization boundary.")]
    public ControlledHarmonyPatchExecutionGateResult RunHarmonyInstanceConstruction()
    {
        var stage = "Harmony instance construction";
        try
        {
            ThrowIfDisposed();
            var preflight = RequirePreflight();
            var api = RequireHarmonyApi();
            var context = RequireLoadContext();
            if (!_provenHarmonyTypeInitializationAuditPassed)
                throw new InvalidOperationException("Step 27 Gate G must pass before the Harmony instance constructor runs.");

            stage = "immediate pre-construction integrity recheck";
            VerifyFileLength(preflight.Target.PreparedPath, preflight.Target.Plan.Length, "prepared Harmony construction target");
            var targetSha1 = ComputeSha1Hex(preflight.Target.PreparedPath);
            if (!targetSha1.Equals(preflight.Target.Plan.Sha1Hex, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Step 27 target SHA-1 changed immediately before Harmony construction.");
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("HARMONY_DEBUG")))
                throw new InvalidDataException("Step 27 refuses construction because HARMONY_DEBUG became non-empty after Gate G.");
            if (api.DebugField.GetValue(null) is not bool debugBefore || debugBefore)
                throw new InvalidDataException("Step 27 refuses construction because Harmony.DEBUG became true after Gate G.");

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
                throw new InvalidDataException("Step 27 constructor returned an unexpected runtime type.");
            if (!ReferenceEquals(AssemblyLoadContext.GetLoadContext(instance.GetType().Assembly), context))
                throw new InvalidDataException("Step 27 Harmony instance type escaped the dedicated load context.");
            var id = api.IdProperty.GetValue(instance) as string;
            if (!string.Equals(id, HarmonyId, StringComparison.Ordinal))
                throw new InvalidDataException($"Step 27 Harmony.Id mismatch. Expected '{HarmonyId}', observed '{id ?? "<null>"}'.");
            if (api.DebugField.GetValue(null) is not bool debugAfter || debugAfter)
                throw new InvalidDataException("Step 27 Harmony.DEBUG was true after inert constructor execution.");

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
                throw new InvalidDataException("Step 27 Harmony construction changed private-context assembly membership.");

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
                ControlledHarmonyPatchExecutionGate.HarmonyInstanceConstruction,
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
            return Fail(ControlledHarmonyPatchExecutionGate.HarmonyInstanceConstruction, stage, ex);
        }
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Step 27 final audit reads only exact reflection members already resolved and verified by Gate E.")]
    [UnconditionalSuppressMessage("Trimming", "IL2075", Justification = "Step 27 final audit retains only exact reflection objects already resolved and verified by Gate E.")]
    public async Task<ControlledHarmonyPatchExecutionGateResult> RunPostConstructionAuditAsync(
        IProgress<ControlledHarmonyPatchExecutionProgress>? progress = null,
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
            var instance = _harmonyInstance ?? throw new InvalidOperationException("Step 27 Harmony instance is missing after Gate H.");

            stage = "runtime plan rehash";
            var planSha256 = await ComputeSha256HexAsync(_planPath, cancellationToken).ConfigureAwait(false);
            if (!planSha256.Equals(preflight.PlanSha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Step 27 runtime-binding plan changed during Harmony construction.");

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
                    throw new InvalidDataException("Step 27 prepared/live byte identity changed during Harmony construction: " + item.Plan.RelativePath);
                verified++;
                progress?.Report(new ControlledHarmonyPatchExecutionProgress(
                    ControlledHarmonyPatchExecutionGate.PostConstructionAudit,
                    verified,
                    preflight.PreparedAssemblies.Length,
                    item.Plan.RelativePath,
                    "Re-hashing prepared/live bytes after the Harmony constructor boundary…"));
            }

            stage = "OfflineReady postcondition";
            var offline = await _offlineInspection.RunAsync(null, cancellationToken).ConfigureAwait(false);
            if (!offline.Success || !offline.ExactManagedTreeVerified || offline.InstalledManifestId != preflight.Plan.ManifestId)
                throw new InvalidDataException(offline.Error ?? "OfflineReady exact-tree verification failed after Step 27 Harmony construction.");

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
                throw new InvalidDataException("Step 27 post-construction private context differs from the physically proven Step 24 context membership.");
            if (context.NativeLoadAttempts.Count != 0)
                throw new DllNotFoundException("Step 27 observed native-library resolution: " + string.Join(" | ", context.NativeLoadAttempts));
            if (context.RejectedManagedRequests.Count != 0)
                throw new FileLoadException("Step 27 observed rejected/unplanned managed requests: " + string.Join(" | ", context.RejectedManagedRequests));

            stage = "Harmony object identity audit";
            if (!ReferenceEquals(instance.GetType(), api.HarmonyType))
                throw new InvalidDataException("Step 27 retained Harmony object changed runtime type.");
            var id = api.IdProperty.GetValue(instance) as string;
            if (!string.Equals(id, HarmonyId, StringComparison.Ordinal) || !string.Equals(id, construction.Id, StringComparison.Ordinal))
                throw new InvalidDataException("Step 27 retained Harmony object ID changed after construction.");
            if (api.DebugField.GetValue(null) is not bool debugValue || debugValue)
                throw new InvalidDataException("Step 27 Harmony.DEBUG is true during the final audit.");
            var targetSha1 = await ComputeSha1HexAsync(preflight.Target.PreparedPath, cancellationToken).ConfigureAwait(false);
            if (!targetSha1.Equals(initialization.PreparedSha1, StringComparison.OrdinalIgnoreCase) ||
                !targetSha1.Equals(typeInitialization.PreparedSha1, StringComparison.OrdinalIgnoreCase) ||
                !targetSha1.Equals(construction.PreparedSha1, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Step 27 0Harmony prepared hash changed across module initialization/type initialization/construction.");

            _provenPostConstructionAuditPassed = true;
            return Pass(
                ControlledHarmonyPatchExecutionGate.PostConstructionAudit,
                "Step 27 post-construction isolation audit passed.\n" +
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
                "Native game library loaded by Step 27: NO\n" +
                "Process note: the Step 27 private managed context remains resident until process exit; force-quit before rerunning earlier fresh-process CLR-load regressions.");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return Fail(ControlledHarmonyPatchExecutionGate.PostConstructionAudit, stage, ex);
        }
    }


    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Step 27 resolves only the exact receipt-verified post-publish Harmony.CreateProcessor/PatchProcessor surface after Cecil metadata preflight.")]
    [UnconditionalSuppressMessage("Trimming", "IL2075", Justification = "The post-publish Harmony types are unavailable to the build-time trimmer; exact runtime reflection is bounded by metadata and identity checks.")]
    public ControlledHarmonyPatchExecutionGateResult RunHarmonyProcessorApiResolution()
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
                throw new InvalidOperationException("Step 27 Gate I must pass before resolving the processor API surface.");

            stage = "processor Cecil metadata preflight";
            var metadata = ReadHarmonyProcessorMetadata(preflight.Target.PreparedPath);
            if (!metadata.Allowed)
                throw new InvalidDataException("Step 27 Gate J refuses processor admission because the exact processor metadata shape changed:\n" + metadata.Detail);

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
                throw new InvalidDataException("HarmonyLib.PatchProcessor escaped the Step 27 private load context.");

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
                throw new InvalidDataException($"Step 27 requires exactly one public PatchProcessor constructor; observed {constructors.Length}.");

            var createProcessorCandidates = harmonyApi.HarmonyType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(method => method.Name.Equals("CreateProcessor", StringComparison.Ordinal))
                .ToArray();
            var createProcessor = createProcessorCandidates.SingleOrDefault(method =>
            {
                var parameters = method.GetParameters();
                return parameters.Length == 1 && parameters[0].ParameterType == typeof(MethodBase) && ReferenceEquals(method.ReturnType, processorType);
            }) ?? throw new MissingMethodException(HarmonyTypeFullName, "CreateProcessor(System.Reflection.MethodBase)");
            if (createProcessorCandidates.Length != 1)
                throw new InvalidDataException($"Step 27 requires exactly one public Harmony.CreateProcessor overload; observed {createProcessorCandidates.Length}.");

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
                ControlledHarmonyPatchExecutionGate.HarmonyProcessorApiResolution,
                "TARGETED HARMONY PROCESSOR API RESOLUTION SUCCEEDED WITHOUT PATCHPROCESSOR TYPE INITIALIZATION OR CONSTRUCTION.\n" +
                $"Harmony factory: {HarmonyTypeFullName}::CreateProcessor(System.Reflection.MethodBase)\n" +
                $"Processor type: {processorType.FullName}\n" +
                "Processor type initializer: PRESENT — exact measured locker initialization shape\n" +
                "Processor constructor: .ctor(HarmonyLib.Harmony,System.Reflection.MethodBase)\n" +
                "Retained fields: instance + original — exact measured types\n" +
                "PatchProcessor type initializer executed by Step 27 Gate J: NO\n" +
                "PatchProcessor object constructed: NO\n" +
                "Patch()/Harmony.Patch invoked: NO\n" +
                "StS2 type/member reflected or invoked: NO\n" +
                "Audited Harmony.CreateProcessor IL:\n" + metadata.CreateProcessorAudit + "\n" +
                "Audited PatchProcessor::.cctor IL:\n" + metadata.PatchProcessorTypeInitializerAudit + "\n" +
                "Audited PatchProcessor::.ctor IL:\n" + metadata.PatchProcessorConstructorAudit);
        }
        catch (Exception ex)
        {
            return Fail(ControlledHarmonyPatchExecutionGate.HarmonyProcessorApiResolution, stage, ex);
        }
    }

    [UnconditionalSuppressMessage("Trimming", "IL2075", Justification = "Step 27 explicitly initializes only the exact PatchProcessor TypeHandle resolved and metadata-verified in Gate J.")]
    public ControlledHarmonyPatchExecutionGateResult RunPatchProcessorTypeInitialization()
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
                throw new InvalidDataException("Step 27 0Harmony SHA-1 changed immediately before PatchProcessor type initialization.");

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
                ControlledHarmonyPatchExecutionGate.PatchProcessorTypeInitialization,
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
            return Fail(ControlledHarmonyPatchExecutionGate.PatchProcessorTypeInitialization, stage, ex);
        }
    }

    [DynamicDependency(nameof(HarmonyProcessorProbe.Target), typeof(HarmonyProcessorProbe))]
    public ControlledHarmonyPatchExecutionGateResult RunLauncherProbeResolution()
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
                throw new InvalidDataException("Step 27 launcher probe return type changed.");
            var parameters = method.GetParameters();
            if (parameters.Length != 1 || parameters[0].ParameterType != typeof(int))
                throw new InvalidDataException("Step 27 launcher probe signature changed.");
            if (method.DeclaringType != typeof(HarmonyProcessorProbe) || method.IsGenericMethod || !method.IsStatic)
                throw new InvalidDataException("Step 27 launcher probe MethodInfo shape is unexpected.");
            var probeContext = AssemblyLoadContext.GetLoadContext(method.DeclaringType.Assembly);
            if (!ReferenceEquals(probeContext, AssemblyLoadContext.Default))
                throw new InvalidDataException("Step 27 launcher-owned probe is not in the default host load context.");
            if (context.ManagedResolverRequests.Count != managedBefore || context.PrivateLoads.Count != privateBefore || context.HostLoads.Count != hostBefore || context.NativeLoadAttempts.Count != nativeBefore)
                throw new InvalidDataException("Resolving the launcher-owned probe unexpectedly affected the private Harmony context.");

            var signature = $"{method.ReturnType.FullName} {method.DeclaringType!.FullName}::{method.Name}({string.Join(",", parameters.Select(p => p.ParameterType.FullName))})";
            _launcherProbe = new LauncherProbeSnapshot(method, signature);
            return Pass(
                ControlledHarmonyPatchExecutionGate.LauncherProbeResolution,
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
            return Fail(ControlledHarmonyPatchExecutionGate.LauncherProbeResolution, stage, ex);
        }
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Step 27 invokes exactly Harmony.CreateProcessor(MethodBase) from the verified post-publish Harmony API surface; no Patch method is invoked.")]
    [UnconditionalSuppressMessage("Trimming", "IL2075", Justification = "Step 27 retains exact post-publish reflection objects verified by Gates J-L.")]
    public ControlledHarmonyPatchExecutionGateResult RunHarmonyProcessorCreation()
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
            var harmonyInstance = _harmonyInstance ?? throw new InvalidOperationException("Step 27 retained Harmony instance is missing after the Step 25 replay.");

            VerifyFileLength(preflight.Target.PreparedPath, preflight.Target.Plan.Length, "prepared processor-creation target");
            var targetSha1 = ComputeSha1Hex(preflight.Target.PreparedPath);
            if (!targetSha1.Equals(preflight.Target.Plan.Sha1Hex, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Step 27 0Harmony SHA-1 changed immediately before CreateProcessor.");
            if (harmonyApi.DebugField.GetValue(null) is not bool debug || debug)
                throw new InvalidDataException("Step 27 refuses processor creation because Harmony.DEBUG is true.");

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
                throw new InvalidDataException("Step 27 CreateProcessor returned an unexpected runtime type.");
            if (!ReferenceEquals(AssemblyLoadContext.GetLoadContext(processor.GetType().Assembly), context))
                throw new InvalidDataException("Step 27 PatchProcessor type escaped the dedicated private load context.");
            if (!ReferenceEquals(processorApi.InstanceField.GetValue(processor), harmonyInstance))
                throw new InvalidDataException("Step 27 PatchProcessor did not retain the exact proven Harmony instance.");
            if (!ReferenceEquals(processorApi.OriginalField.GetValue(processor), probe.Method))
                throw new InvalidDataException("Step 27 PatchProcessor did not retain the exact launcher-owned probe MethodBase.");

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
                ControlledHarmonyPatchExecutionGate.HarmonyProcessorCreation,
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
            return Fail(ControlledHarmonyPatchExecutionGate.HarmonyProcessorCreation, stage, ex);
        }
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Step 27 final audit reads only exact reflection fields/members already resolved and verified by earlier gates.")]
    [UnconditionalSuppressMessage("Trimming", "IL2075", Justification = "Step 27 final audit retains only exact post-publish reflection objects bounded by Gates J-M.")]
    public async Task<ControlledHarmonyPatchExecutionGateResult> RunPostProcessorAuditAsync(
        IProgress<ControlledHarmonyPatchExecutionProgress>? progress = null,
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
            var harmonyInstance = _harmonyInstance ?? throw new InvalidOperationException("Step 27 retained Harmony instance is missing.");
            var processor = _patchProcessorInstance ?? throw new InvalidOperationException("Step 27 retained PatchProcessor instance is missing.");

            stage = "runtime plan rehash";
            var planSha256 = await ComputeSha256HexAsync(_planPath, cancellationToken).ConfigureAwait(false);
            if (!planSha256.Equals(preflight.PlanSha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Step 27 runtime-binding plan changed during processor creation.");

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
                    throw new InvalidDataException("Step 27 prepared/live byte identity changed during processor creation: " + item.Plan.RelativePath);
                verified++;
                progress?.Report(new ControlledHarmonyPatchExecutionProgress(
                    ControlledHarmonyPatchExecutionGate.PostProcessorAudit,
                    verified,
                    preflight.PreparedAssemblies.Length,
                    item.Plan.RelativePath,
                    "Re-hashing prepared/live bytes after the empty PatchProcessor boundary…"));
            }

            stage = "OfflineReady postcondition";
            var offline = await _offlineInspection.RunAsync(null, cancellationToken).ConfigureAwait(false);
            if (!offline.Success || !offline.ExactManagedTreeVerified || offline.InstalledManifestId != preflight.Plan.ManifestId)
                throw new InvalidDataException(offline.Error ?? "OfflineReady exact-tree verification failed after Step 27 processor creation.");

            stage = "private context membership audit";
            var expected = preflight.PreparedAssemblies
                .Where(item => item.ModuleInitializerCount == 0 || item.Plan.AssemblyFullName.Equals(preflight.Target.Plan.AssemblyFullName, StringComparison.Ordinal))
                .Select(item => item.Plan.AssemblyFullName)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            var actual = context.Assemblies.Select(a => a.GetName().FullName ?? a.GetName().Name ?? string.Empty).OrderBy(v => v, StringComparer.Ordinal).ToArray();
            if (!actual.SequenceEqual(expected, StringComparer.Ordinal))
                throw new InvalidDataException("Step 27 post-processor private context differs from the physically proven Step 25 context membership.");
            if (context.NativeLoadAttempts.Count != 0)
                throw new DllNotFoundException("Step 27 observed native-library resolution: " + string.Join(" | ", context.NativeLoadAttempts));
            if (context.RejectedManagedRequests.Count != 0)
                throw new FileLoadException("Step 27 observed rejected/unplanned managed requests: " + string.Join(" | ", context.RejectedManagedRequests));

            stage = "retained processor identity audit";
            if (!ReferenceEquals(processor.GetType(), processorApi.PatchProcessorType) || !ReferenceEquals(processorApi.InstanceField.GetValue(processor), harmonyInstance) || !ReferenceEquals(processorApi.OriginalField.GetValue(processor), probe.Method))
                throw new InvalidDataException("Step 27 retained PatchProcessor state changed after creation.");
            if (harmonyApi.DebugField.GetValue(null) is not bool debug || debug)
                throw new InvalidDataException("Step 27 Harmony.DEBUG is true during final processor audit.");
            var targetSha1 = await ComputeSha1HexAsync(preflight.Target.PreparedPath, cancellationToken).ConfigureAwait(false);
            if (!targetSha1.Equals(processorTypeInitialization.PreparedSha1, StringComparison.OrdinalIgnoreCase) || !targetSha1.Equals(creation.PreparedSha1, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Step 27 0Harmony prepared hash changed across PatchProcessor type initialization/creation.");

            _provenPostProcessorAuditPassed = true;
            return Pass(
                ControlledHarmonyPatchExecutionGate.PostProcessorAudit,
                "Step 27 post-processor isolation audit passed.\n" +
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
                "Native game library loaded by Step 27: NO\n" +
                "Process note: the Step 27 private managed context remains resident until process exit; force-quit before rerunning earlier fresh-process CLR-load regressions.");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return Fail(ControlledHarmonyPatchExecutionGate.PostProcessorAudit, stage, ex);
        }
    }


    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Step 27 resolves only exact receipt-verified post-publish Harmony patch APIs after Cecil metadata preflight.")]
    [UnconditionalSuppressMessage("Trimming", "IL2075", Justification = "The post-publish Harmony patch types are unavailable to the build-time trimmer; exact runtime reflection is bounded by metadata and identity checks.")]
    public ControlledHarmonyPatchExecutionGateResult RunHarmonyPatchApiResolution()
    {
        var stage = "Harmony patch API resolution";
        try
        {
            ThrowIfDisposed();
            var preflight = RequirePreflight();
            var initialization = RequireInitialization();
            var processorApi = RequireProcessorApi();
            var context = RequireLoadContext();
            if (!_provenPostProcessorAuditPassed)
                throw new InvalidOperationException("Step 27 Gate N must pass before resolving any patch API surface.");

            stage = "patch Cecil metadata preflight";
            var metadata = ReadHarmonyPatchMetadata(preflight.Target.PreparedPath);
            if (!metadata.Allowed)
                throw new InvalidDataException("Step 27 Gate O refuses patch admission because the exact patch metadata shape changed:\n" + metadata.Detail);
            var accessToolsMetadata = ReadAccessToolsMetadata(preflight.Target.PreparedPath);
            if (!accessToolsMetadata.Allowed)
                throw new InvalidDataException("Step 27 Gate O refuses AccessTools admission because its type initializer is not the exact physically measured runtime-detection/cache shape:\n" + accessToolsMetadata.Detail);

            stage = "AccessTools host-framework preservation preflight";
            var runtimeInformationType = Type.GetType("System.Runtime.InteropServices.RuntimeInformation", throwOnError: false, ignoreCase: false)
                ?? throw new TypeLoadException("Step 27 AccessTools preservation preflight cannot resolve RuntimeInformation by the exact string used by Harmony.");
            if (runtimeInformationType != typeof(RuntimeInformation))
                throw new InvalidDataException("String-resolved RuntimeInformation does not bind to the host RuntimeInformation type.");
            var frameworkDescriptionProperty = runtimeInformationType.GetProperty("FrameworkDescription", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                ?? throw new MissingMemberException("RuntimeInformation.FrameworkDescription was trimmed; Step 27 cannot safely initialize AccessTools.");
            if (frameworkDescriptionProperty.PropertyType != typeof(string) || frameworkDescriptionProperty.GetMethod is null)
                throw new InvalidDataException("RuntimeInformation.FrameworkDescription runtime shape changed.");
            var frameworkDescription = frameworkDescriptionProperty.GetValue(null) as string;
            if (string.IsNullOrWhiteSpace(frameworkDescription))
                throw new InvalidDataException("RuntimeInformation.FrameworkDescription returned an empty value during AccessTools preservation preflight.");
            _ = typeof(Dictionary<,>).GetConstructor(Type.EmptyTypes)
                ?? throw new MissingMethodException("System.Collections.Generic.Dictionary<,>", ".ctor()");
            _ = typeof(ReaderWriterLockSlim).GetConstructor([typeof(LockRecursionPolicy)])
                ?? throw new MissingMethodException(typeof(ReaderWriterLockSlim).FullName, ".ctor(System.Threading.LockRecursionPolicy)");

            var managedBefore = context.ManagedResolverRequests.Count;
            var privateBefore = context.PrivateLoads.Count;
            var hostBefore = context.HostLoads.Count;
            var nativeBefore = context.NativeLoadAttempts.Count;
            var membershipBefore = context.Assemblies.Select(a => a.GetName().FullName ?? a.GetName().Name ?? string.Empty).OrderBy(v => v, StringComparer.Ordinal).ToArray();

            stage = "exact patch API reflection";
            var processorType = processorApi.PatchProcessorType;
            var addPrefixCandidates = processorType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(method => method.Name.Equals("AddPrefix", StringComparison.Ordinal)).ToArray();
            var addPrefix = addPrefixCandidates.SingleOrDefault(method =>
            {
                var parameters = method.GetParameters();
                return parameters.Length == 1 && parameters[0].ParameterType == typeof(MethodInfo) && ReferenceEquals(method.ReturnType, processorType);
            }) ?? throw new MissingMethodException(PatchProcessorTypeFullName, "AddPrefix(System.Reflection.MethodInfo)");

            var patchCandidates = processorType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(method => method.Name.Equals("Patch", StringComparison.Ordinal)).ToArray();
            var patch = patchCandidates.SingleOrDefault(method => method.GetParameters().Length == 0 && method.ReturnType == typeof(MethodInfo))
                ?? throw new MissingMethodException(PatchProcessorTypeFullName, "Patch()");

            var unpatchCandidates = processorType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(method => method.Name.Equals("Unpatch", StringComparison.Ordinal)).ToArray();
            var unpatch = unpatchCandidates.SingleOrDefault(method =>
            {
                var parameters = method.GetParameters();
                return parameters.Length == 1 && parameters[0].ParameterType == typeof(MethodInfo) && ReferenceEquals(method.ReturnType, processorType);
            }) ?? throw new MissingMethodException(PatchProcessorTypeFullName, "Unpatch(System.Reflection.MethodInfo)");

            var prefixField = processorType.GetField("prefix", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                ?? throw new MissingFieldException(PatchProcessorTypeFullName, "prefix");
            var harmonyMethodType = initialization.TargetAssembly.GetType("HarmonyLib.HarmonyMethod", throwOnError: false, ignoreCase: false)
                ?? throw new TypeLoadException("Exact HarmonyLib.HarmonyMethod type is absent from loaded 0Harmony.");
            if (!ReferenceEquals(prefixField.FieldType, harmonyMethodType))
                throw new InvalidDataException("PatchProcessor.prefix runtime type no longer matches exact HarmonyLib.HarmonyMethod.");
            if (harmonyMethodType.TypeInitializer is not null)
                throw new InvalidDataException("Step 27 does not permit an implicit HarmonyMethod type initializer.");
            var harmonyMethodConstructors = harmonyMethodType.GetConstructors(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            var harmonyMethodConstructor = harmonyMethodConstructors.SingleOrDefault(candidate =>
            {
                var parameters = candidate.GetParameters();
                return parameters.Length == 1 && parameters[0].ParameterType == typeof(MethodInfo);
            }) ?? throw new MissingMethodException("HarmonyLib.HarmonyMethod", ".ctor(System.Reflection.MethodInfo)");
            var harmonyMethodMethodField = harmonyMethodType.GetField("method", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                ?? throw new MissingFieldException("HarmonyLib.HarmonyMethod", "method");
            if (harmonyMethodMethodField.FieldType != typeof(MethodInfo))
                throw new InvalidDataException("HarmonyMethod.method runtime field type changed.");

            var accessToolsType = initialization.TargetAssembly.GetType(AccessToolsTypeFullName, throwOnError: false, ignoreCase: false)
                ?? throw new TypeLoadException("Exact HarmonyLib.AccessTools type is absent from loaded 0Harmony.");
            if (!(accessToolsType.IsAbstract && accessToolsType.IsSealed))
                throw new InvalidDataException("HarmonyLib.AccessTools is no longer an exact static type.");
            var accessToolsTypeInitializer = accessToolsType.TypeInitializer
                ?? throw new MissingMethodException(AccessToolsTypeFullName, ".cctor()");
            var accessToolsAllField = accessToolsType.GetField("all", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                ?? throw new MissingFieldException(AccessToolsTypeFullName, "all");
            var accessToolsAllDeclaredField = accessToolsType.GetField("allDeclared", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                ?? throw new MissingFieldException(AccessToolsTypeFullName, "allDeclared");
            if (accessToolsAllField.FieldType != typeof(BindingFlags) || accessToolsAllDeclaredField.FieldType != typeof(BindingFlags))
                throw new InvalidDataException("HarmonyLib.AccessTools BindingFlags field types changed.");
            var accessToolsAllTypesCachedField = accessToolsType.GetField("allTypesCached", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)
                ?? throw new MissingFieldException(AccessToolsTypeFullName, "allTypesCached");
            var accessToolsIsMonoRuntimeField = accessToolsType.GetField("<IsMonoRuntime>k__BackingField", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)
                ?? throw new MissingFieldException(AccessToolsTypeFullName, "<IsMonoRuntime>k__BackingField");
            var accessToolsIsNetFrameworkRuntimeField = accessToolsType.GetField("<IsNetFrameworkRuntime>k__BackingField", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)
                ?? throw new MissingFieldException(AccessToolsTypeFullName, "<IsNetFrameworkRuntime>k__BackingField");
            var accessToolsIsNetCoreRuntimeField = accessToolsType.GetField("<IsNetCoreRuntime>k__BackingField", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)
                ?? throw new MissingFieldException(AccessToolsTypeFullName, "<IsNetCoreRuntime>k__BackingField");
            var accessToolsAddHandlerCacheField = accessToolsType.GetField("addHandlerCache", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)
                ?? throw new MissingFieldException(AccessToolsTypeFullName, "addHandlerCache");
            var accessToolsAddHandlerCacheLockField = accessToolsType.GetField("addHandlerCacheLock", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)
                ?? throw new MissingFieldException(AccessToolsTypeFullName, "addHandlerCacheLock");
            if (accessToolsAllTypesCachedField.FieldType != typeof(Type[]) ||
                accessToolsIsMonoRuntimeField.FieldType != typeof(bool) ||
                accessToolsIsNetFrameworkRuntimeField.FieldType != typeof(bool) ||
                accessToolsIsNetCoreRuntimeField.FieldType != typeof(bool) ||
                accessToolsAddHandlerCacheLockField.FieldType != typeof(ReaderWriterLockSlim))
                throw new InvalidDataException("HarmonyLib.AccessTools measured runtime-detection/cache field types changed.");
            var cacheType = accessToolsAddHandlerCacheField.FieldType;
            if (!cacheType.IsGenericType || cacheType.GetGenericTypeDefinition() != typeof(Dictionary<,>) ||
                cacheType.GetGenericArguments()[0] != typeof(Type) ||
                !cacheType.GetGenericArguments()[1].FullName!.Equals("HarmonyLib.FastInvokeHandler", StringComparison.Ordinal))
                throw new InvalidDataException("HarmonyLib.AccessTools addHandlerCache field type changed.");
            // Do not read any AccessTools static field here: Gate R owns the type-initialization boundary.

            if (context.ManagedResolverRequests.Count != managedBefore || context.PrivateLoads.Count != privateBefore || context.HostLoads.Count != hostBefore)
                throw new InvalidDataException("Targeted patch API reflection unexpectedly changed resolver/load counters.");
            if (context.NativeLoadAttempts.Count != nativeBefore)
                throw new DllNotFoundException("Targeted patch API reflection attempted native resolution.");
            if (context.RejectedManagedRequests.Count != 0)
                throw new FileLoadException("Targeted patch API reflection triggered an unplanned managed request: " + string.Join(" | ", context.RejectedManagedRequests));
            var membershipAfter = context.Assemblies.Select(a => a.GetName().FullName ?? a.GetName().Name ?? string.Empty).OrderBy(v => v, StringComparer.Ordinal).ToArray();
            if (!membershipAfter.SequenceEqual(membershipBefore, StringComparer.Ordinal))
                throw new InvalidDataException("Targeted patch API reflection changed private-context membership.");

            _patchApi = new HarmonyPatchApiSnapshot(
                addPrefix,
                patch,
                unpatch,
                prefixField,
                harmonyMethodType,
                harmonyMethodConstructor,
                harmonyMethodMethodField,
                accessToolsType,
                accessToolsTypeInitializer,
                accessToolsAllField,
                accessToolsAllDeclaredField,
                accessToolsAllTypesCachedField,
                accessToolsIsMonoRuntimeField,
                accessToolsIsNetFrameworkRuntimeField,
                accessToolsIsNetCoreRuntimeField,
                accessToolsAddHandlerCacheField,
                accessToolsAddHandlerCacheLockField,
                frameworkDescription,
                accessToolsMetadata.TypeInitializerAudit,
                metadata.AddPrefixAudit,
                metadata.PatchAudit,
                metadata.UnpatchAudit,
                metadata.HarmonyMethodConstructorAudit);

            return Pass(
                ControlledHarmonyPatchExecutionGate.HarmonyPatchApiResolution,
                "TARGETED HARMONY PATCH API RESOLUTION SUCCEEDED WITHOUT PATCH-DESCRIPTION CONSTRUCTION OR PATCHING.\n" +
                "Patch description admission: PatchProcessor.AddPrefix(System.Reflection.MethodInfo)\n" +
                "Patch execution method: PatchProcessor.Patch() -> System.Reflection.MethodInfo\n" +
                "Exact removal method: PatchProcessor.Unpatch(System.Reflection.MethodInfo)\n" +
                "Patch descriptor type: HarmonyLib.HarmonyMethod — no type initializer\n" +
                "Patch descriptor constructor: HarmonyMethod(System.Reflection.MethodInfo)\n" +
                "Patch descriptor retained method field: method : System.Reflection.MethodInfo\n" +
                "AccessTools type initializer: PRESENT — exact Step 27.0.1 physical runtime-detection/cache fingerprint\n" +
                $"Host RuntimeInformation.FrameworkDescription preservation preflight: {frameworkDescription}\n" +
                "AccessTools static-field values read: NO — Gate R owns the type-initialization boundary\n" +
                "HarmonyMethod object constructed: NO\n" +
                "PatchProcessor.Patch invoked: NO\n" +
                "Launcher patch probe invoked: NO\n" +
                "StS2 type/member reflected or invoked: NO\n" +
                "Audited AddPrefix(MethodInfo) IL:\n" + metadata.AddPrefixAudit + "\n" +
                "Audited Patch() IL:\n" + metadata.PatchAudit + "\n" +
                "Audited Unpatch(MethodInfo) IL:\n" + metadata.UnpatchAudit + "\n" +
                "Audited HarmonyMethod(MethodInfo) IL:\n" + metadata.HarmonyMethodConstructorAudit + "\n" +
                "Audited AccessTools::.cctor IL:\n" + accessToolsMetadata.TypeInitializerAudit);
        }
        catch (Exception ex)
        {
            return Fail(ControlledHarmonyPatchExecutionGate.HarmonyPatchApiResolution, stage, ex);
        }
    }

    [DynamicDependency(nameof(HarmonyPatchProbe.Target), typeof(HarmonyPatchProbe))]
    [DynamicDependency(nameof(HarmonyPatchProbe.Prefix), typeof(HarmonyPatchProbe))]
    public ControlledHarmonyPatchExecutionGateResult RunLauncherPatchProbeResolution()
    {
        var stage = "launcher patch probe MethodInfo resolution";
        try
        {
            ThrowIfDisposed();
            _ = RequirePatchApi();
            var context = RequireLoadContext();
            var managedBefore = context.ManagedResolverRequests.Count;
            var privateBefore = context.PrivateLoads.Count;
            var hostBefore = context.HostLoads.Count;
            var nativeBefore = context.NativeLoadAttempts.Count;

            var target = typeof(HarmonyPatchProbe).GetMethod(nameof(HarmonyPatchProbe.Target), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                ?? throw new MissingMethodException(typeof(HarmonyPatchProbe).FullName, nameof(HarmonyPatchProbe.Target));
            var prefix = typeof(HarmonyPatchProbe).GetMethod(nameof(HarmonyPatchProbe.Prefix), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                ?? throw new MissingMethodException(typeof(HarmonyPatchProbe).FullName, nameof(HarmonyPatchProbe.Prefix));

            if (target.ReturnType != typeof(int) || target.IsGenericMethod || !target.IsStatic)
                throw new InvalidDataException("Step 27 launcher patch target shape changed.");
            var targetParameters = target.GetParameters();
            if (targetParameters.Length != 1 || targetParameters[0].ParameterType != typeof(int) || !string.Equals(targetParameters[0].Name, "value", StringComparison.Ordinal))
                throw new InvalidDataException("Step 27 launcher patch target signature/parameter metadata changed.");

            if (prefix.ReturnType != typeof(bool) || prefix.IsGenericMethod || !prefix.IsStatic)
                throw new InvalidDataException("Step 27 launcher prefix shape changed.");
            var prefixParameters = prefix.GetParameters();
            if (prefixParameters.Length != 2 ||
                prefixParameters[0].ParameterType != typeof(int) || !string.Equals(prefixParameters[0].Name, "value", StringComparison.Ordinal) ||
                prefixParameters[1].ParameterType != typeof(int).MakeByRefType() || !string.Equals(prefixParameters[1].Name, "__result", StringComparison.Ordinal))
                throw new InvalidDataException("Step 27 launcher prefix signature/parameter metadata changed.");

            if (!ReferenceEquals(AssemblyLoadContext.GetLoadContext(target.DeclaringType!.Assembly), AssemblyLoadContext.Default) ||
                !ReferenceEquals(AssemblyLoadContext.GetLoadContext(prefix.DeclaringType!.Assembly), AssemblyLoadContext.Default))
                throw new InvalidDataException("Step 27 launcher patch probe is not in the default host load context.");
            if (context.ManagedResolverRequests.Count != managedBefore || context.PrivateLoads.Count != privateBefore || context.HostLoads.Count != hostBefore || context.NativeLoadAttempts.Count != nativeBefore)
                throw new InvalidDataException("Resolving the launcher patch probe unexpectedly affected the private Harmony context.");

            var targetSignature = $"{target.ReturnType.FullName} {target.DeclaringType.FullName}::{target.Name}({string.Join(",", targetParameters.Select(p => p.ParameterType.FullName))})";
            var prefixSignature = $"{prefix.ReturnType.FullName} {prefix.DeclaringType.FullName}::{prefix.Name}({string.Join(",", prefixParameters.Select(p => p.ParameterType.FullName))})";
            _patchProbe = new LauncherPatchProbeSnapshot(target, prefix, targetSignature, prefixSignature);

            return Pass(
                ControlledHarmonyPatchExecutionGate.LauncherPatchProbeResolution,
                "LAUNCHER-OWNED PATCH TARGET + PREFIX RESOLVED WITHOUT INVOCATION.\n" +
                $"Target: {targetSignature}\n" +
                $"Prefix: {prefixSignature}\n" +
                "Prefix parameter names: value + __result — EXACT\n" +
                "Declaring assembly load context: DEFAULT HOST\n" +
                "Target invoked: NO\n" +
                "Prefix invoked: NO\n" +
                "Harmony patch API invoked: NO\n" +
                "StS2 assembly/type/member reflection: NO");
        }
        catch (Exception ex)
        {
            return Fail(ControlledHarmonyPatchExecutionGate.LauncherPatchProbeResolution, stage, ex);
        }
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Step 27 invokes only the exact launcher-owned probe MethodInfo already rooted and signature-verified in Gate P.")]
    public ControlledHarmonyPatchExecutionGateResult RunBaselineProbeInvocation()
    {
        var stage = "launcher baseline probe invocation";
        try
        {
            ThrowIfDisposed();
            var probe = RequirePatchProbe();
            var context = RequireLoadContext();
            var managedBefore = context.ManagedResolverRequests.Count;
            var privateBefore = context.PrivateLoads.Count;
            var hostBefore = context.HostLoads.Count;
            var nativeBefore = context.NativeLoadAttempts.Count;

            HarmonyPatchProbe.ResetCounters();
            stage = "direct baseline target invocation";
            var directResult = HarmonyPatchProbe.Target(41);
            stage = "reflection baseline target invocation";
            var reflectedRaw = probe.Target.Invoke(null, [41]);
            if (reflectedRaw is not int reflectedResult)
                throw new InvalidDataException("Step 27 baseline reflection invocation did not return System.Int32.");

            var targetCalls = HarmonyPatchProbe.TargetCalls;
            var prefixCalls = HarmonyPatchProbe.PrefixCalls;
            if (directResult != 42 || reflectedResult != 42 || targetCalls != 2 || prefixCalls != 0)
                throw new InvalidDataException($"Launcher baseline behavior changed: direct={directResult}, reflection={reflectedResult}, targetCalls={targetCalls}, prefixCalls={prefixCalls}.");
            if (context.ManagedResolverRequests.Count != managedBefore || context.PrivateLoads.Count != privateBefore || context.HostLoads.Count != hostBefore || context.NativeLoadAttempts.Count != nativeBefore)
                throw new InvalidDataException("Launcher baseline invocation unexpectedly affected the private Harmony context.");

            _baselineProbeInvocation = new BaselineProbeInvocationSnapshot(directResult, reflectedResult, targetCalls, prefixCalls);
            return Pass(
                ControlledHarmonyPatchExecutionGate.BaselineProbeInvocation,
                "LAUNCHER-OWNED PROBE BASELINE BEHAVIOR ESTABLISHED BEFORE PATCHING.\n" +
                "Input: 41\n" +
                $"Direct result: {directResult}\n" +
                $"Reflection result: {reflectedResult}\n" +
                $"Target calls: {targetCalls}\n" +
                $"Prefix calls: {prefixCalls}\n" +
                "Expected original behavior value + 1: YES\n" +
                "PatchProcessor.Patch invoked: NO\n" +
                "StS2 type/member reflected or invoked: NO");
        }
        catch (TargetInvocationException ex) when (ex.InnerException is not null)
        {
            return Fail(ControlledHarmonyPatchExecutionGate.BaselineProbeInvocation, stage, ex.InnerException);
        }
        catch (Exception ex)
        {
            return Fail(ControlledHarmonyPatchExecutionGate.BaselineProbeInvocation, stage, ex);
        }
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Step 27 explicitly initializes only the exact physically measured HarmonyLib.AccessTools runtime-detection/cache initializer before HarmonyMethod construction.")]
    [UnconditionalSuppressMessage("Trimming", "IL2075", Justification = "AccessTools is post-publish Harmony code unavailable to the build-time trimmer; Gate O bounds its exact physical runtime-detection/cache initializer and verifies the string-reflected framework surface before this explicit completion barrier.")]
    public ControlledHarmonyPatchExecutionGateResult RunAccessToolsTypeInitialization()
    {
        var stage = "explicit HarmonyLib.AccessTools type-initialization completion barrier";
        try
        {
            ThrowIfDisposed();
            var preflight = RequirePreflight();
            var patchApi = RequirePatchApi();
            _ = RequireBaselineProbeInvocation();
            var context = RequireLoadContext();

            VerifyFileLength(preflight.Target.PreparedPath, preflight.Target.Plan.Length, "prepared AccessTools initialization target");
            var targetSha1 = ComputeSha1Hex(preflight.Target.PreparedPath);
            if (!targetSha1.Equals(preflight.Target.Plan.Sha1Hex, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Step 27 0Harmony SHA-1 changed immediately before AccessTools type initialization.");

            var managedBefore = context.ManagedResolverRequests.Count;
            var privateBefore = context.PrivateLoads.Count;
            var hostBefore = context.HostLoads.Count;
            var nativeBefore = context.NativeLoadAttempts.Count;
            var membershipBefore = context.Assemblies.Select(a => a.GetName().FullName ?? a.GetName().Name ?? string.Empty).OrderBy(v => v, StringComparer.Ordinal).ToArray();

            RuntimeHelpers.RunClassConstructor(patchApi.AccessToolsType.TypeHandle);

            stage = "AccessTools post-initialization BindingFlags verification";
            var allRaw = patchApi.AccessToolsAllField.GetValue(null)
                ?? throw new InvalidDataException("AccessTools.all remained null after explicit type initialization.");
            var allDeclaredRaw = patchApi.AccessToolsAllDeclaredField.GetValue(null)
                ?? throw new InvalidDataException("AccessTools.allDeclared remained null after explicit type initialization.");
            if (allRaw is not BindingFlags all || allDeclaredRaw is not BindingFlags allDeclared)
                throw new InvalidDataException("AccessTools BindingFlags fields returned unexpected runtime values/types.");
            var expectedAll = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static |
                BindingFlags.GetField | BindingFlags.SetField | BindingFlags.GetProperty | BindingFlags.SetProperty;
            var expectedAllDeclared = expectedAll | BindingFlags.DeclaredOnly;
            if (all != expectedAll || allDeclared != expectedAllDeclared)
                throw new InvalidDataException($"AccessTools BindingFlags initialization changed: all={all} ({(int)all}), allDeclared={allDeclared} ({(int)allDeclared}), expected={expectedAll} ({(int)expectedAll}) / {expectedAllDeclared} ({(int)expectedAllDeclared}).");

            stage = "AccessTools post-initialization runtime-detection/cache verification";
            if (patchApi.AccessToolsAllTypesCachedField.GetValue(null) is not null)
                throw new InvalidDataException("AccessTools.allTypesCached no longer matches the measured null post-initialization state.");
            if (patchApi.AccessToolsIsMonoRuntimeField.GetValue(null) is not bool isMonoRuntime ||
                patchApi.AccessToolsIsNetFrameworkRuntimeField.GetValue(null) is not bool isNetFrameworkRuntime ||
                patchApi.AccessToolsIsNetCoreRuntimeField.GetValue(null) is not bool isNetCoreRuntime)
                throw new InvalidDataException("AccessTools runtime-detection backing fields returned unexpected runtime values/types.");
            var addHandlerCache = patchApi.AccessToolsAddHandlerCacheField.GetValue(null)
                ?? throw new InvalidDataException("AccessTools.addHandlerCache remained null after explicit type initialization.");
            if (addHandlerCache is not ICollection cacheCollection || cacheCollection.Count != 0)
                throw new InvalidDataException("AccessTools.addHandlerCache is not the expected empty ICollection immediately after type initialization.");
            var addHandlerCacheLock = patchApi.AccessToolsAddHandlerCacheLockField.GetValue(null) as ReaderWriterLockSlim
                ?? throw new InvalidDataException("AccessTools.addHandlerCacheLock remained null or changed type after explicit type initialization.");
            if (addHandlerCacheLock.IsReadLockHeld || addHandlerCacheLock.IsUpgradeableReadLockHeld || addHandlerCacheLock.IsWriteLockHeld)
                throw new InvalidDataException("AccessTools.addHandlerCacheLock unexpectedly holds a lock immediately after type initialization.");
            var frameworkDescription = RuntimeInformation.FrameworkDescription;
            if (string.IsNullOrWhiteSpace(frameworkDescription) || frameworkDescription.StartsWith(".NET Framework", StringComparison.Ordinal))
                throw new InvalidDataException("AccessTools runtime environment is not the expected non-.NET-Framework host: " + frameworkDescription);

            if (context.NativeLoadAttempts.Count != nativeBefore)
                throw new DllNotFoundException("AccessTools type initialization attempted native resolution: " + string.Join(" | ", context.NativeLoadAttempts.Skip(nativeBefore)));
            if (context.RejectedManagedRequests.Count != 0)
                throw new FileLoadException("AccessTools type initialization triggered an unplanned managed request: " + string.Join(" | ", context.RejectedManagedRequests));
            var membershipAfter = context.Assemblies.Select(a => a.GetName().FullName ?? a.GetName().Name ?? string.Empty).OrderBy(v => v, StringComparer.Ordinal).ToArray();
            if (!membershipAfter.SequenceEqual(membershipBefore, StringComparer.Ordinal))
                throw new InvalidDataException("AccessTools type initialization changed private-context membership.");
            var postSha1 = ComputeSha1Hex(preflight.Target.PreparedPath);
            if (!postSha1.Equals(targetSha1, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("0Harmony prepared bytes changed across AccessTools type initialization.");
            if (HarmonyPatchProbe.TargetCalls != 2 || HarmonyPatchProbe.PrefixCalls != 0)
                throw new InvalidDataException("AccessTools type initialization unexpectedly invoked the launcher target or prefix.");

            _accessToolsTypeInitialization = new AccessToolsTypeInitializationSnapshot(
                postSha1, all, allDeclared, isMonoRuntime, isNetFrameworkRuntime, isNetCoreRuntime, frameworkDescription,
                context.ManagedResolverRequests.Count - managedBefore,
                context.PrivateLoads.Count - privateBefore,
                context.HostLoads.Count - hostBefore,
                context.NativeLoadAttempts.Count - nativeBefore);

            return Pass(
                ControlledHarmonyPatchExecutionGate.AccessToolsTypeInitialization,
                "CONTROLLED HARMONY ACCESSTOOLS TYPE INITIALIZATION SUCCEEDED.\n" +
                "Completion barrier: RuntimeHelpers.RunClassConstructor(HarmonyLib.AccessTools.TypeHandle) = PASS\n" +
                "Measured initializer: exact Step 27.0.1 physical runtime-detection/cache shape\n" +
                $"AccessTools.all: {all} ({(int)all})\n" +
                $"AccessTools.allDeclared: {allDeclared} ({(int)allDeclared})\n" +
                $"AccessTools.IsMonoRuntime: {isMonoRuntime}\n" +
                $"AccessTools.IsNetFrameworkRuntime: {isNetFrameworkRuntime}\n" +
                $"AccessTools.IsNetCoreRuntime: {isNetCoreRuntime}\n" +
                $"RuntimeInformation.FrameworkDescription: {frameworkDescription}\n" +
                "AccessTools.allTypesCached: null — MATCH\n" +
                "AccessTools.addHandlerCache: empty — MATCH\n" +
                "AccessTools.addHandlerCacheLock: initialized/unheld — MATCH\n" +
                $"Managed resolver requests during type initialization: {_accessToolsTypeInitialization.ManagedResolverRequests:N0}\n" +
                $"Private loads during type initialization: {_accessToolsTypeInitialization.PrivateLoads:N0}\n" +
                $"Host loads during type initialization: {_accessToolsTypeInitialization.HostLoads:N0}\n" +
                $"Native load attempts during type initialization: {_accessToolsTypeInitialization.NativeLoadAttempts:N0}\n" +
                "Private-context membership changed: NO\n" +
                "HarmonyMethod object constructed: NO\n" +
                "PatchProcessor.Patch invoked: NO\n" +
                "Launcher target/prefix invoked: NO\n" +
                "StS2 type/member reflected or invoked: NO");
        }
        catch (Exception ex)
        {
            return Fail(ControlledHarmonyPatchExecutionGate.AccessToolsTypeInitialization, stage, ex);
        }
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Step 27 invokes exactly PatchProcessor.AddPrefix(MethodInfo) from the metadata-verified post-publish API surface.")]
    [UnconditionalSuppressMessage("Trimming", "IL2075", Justification = "Step 27 inspects only exact post-publish HarmonyMethod fields/types resolved by Gate O.")]
    public ControlledHarmonyPatchExecutionGateResult RunPrefixRegistration()
    {
        var stage = "exact AddPrefix(MethodInfo) invocation";
        try
        {
            ThrowIfDisposed();
            var preflight = RequirePreflight();
            var patchApi = RequirePatchApi();
            var probe = RequirePatchProbe();
            _ = RequireBaselineProbeInvocation();
            _ = RequireAccessToolsTypeInitialization();
            var context = RequireLoadContext();
            var processor = _patchProcessorInstance ?? throw new InvalidOperationException("Step 27 retained PatchProcessor instance is missing.");

            VerifyFileLength(preflight.Target.PreparedPath, preflight.Target.Plan.Length, "prepared prefix-registration target");
            var targetSha1 = ComputeSha1Hex(preflight.Target.PreparedPath);
            if (!targetSha1.Equals(preflight.Target.Plan.Sha1Hex, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Step 27 0Harmony SHA-1 changed immediately before prefix registration.");

            var managedBefore = context.ManagedResolverRequests.Count;
            var privateBefore = context.PrivateLoads.Count;
            var hostBefore = context.HostLoads.Count;
            var nativeBefore = context.NativeLoadAttempts.Count;
            var membershipBefore = context.Assemblies.Select(a => a.GetName().FullName ?? a.GetName().Name ?? string.Empty).OrderBy(v => v, StringComparer.Ordinal).ToArray();

            object? returned;
            try
            {
                returned = patchApi.AddPrefixMethod.Invoke(processor, [probe.Prefix]);
            }
            catch (TargetInvocationException ex) when (ex.InnerException is not null)
            {
                throw new InvalidOperationException("Exact PatchProcessor.AddPrefix(MethodInfo) threw.", ex.InnerException);
            }
            if (!ReferenceEquals(returned, processor))
                throw new InvalidDataException("PatchProcessor.AddPrefix(MethodInfo) did not return the same processor instance.");

            stage = "registered prefix descriptor verification";
            var descriptor = patchApi.PrefixField.GetValue(processor)
                ?? throw new InvalidDataException("PatchProcessor.prefix remained null after exact AddPrefix(MethodInfo).");
            if (!ReferenceEquals(descriptor.GetType(), patchApi.HarmonyMethodType))
                throw new InvalidDataException("PatchProcessor.prefix is not exact HarmonyLib.HarmonyMethod.");
            if (!ReferenceEquals(patchApi.HarmonyMethodMethodField.GetValue(descriptor), probe.Prefix))
                throw new InvalidDataException("HarmonyMethod.method did not retain the exact launcher-owned prefix MethodInfo.");

            if (context.NativeLoadAttempts.Count != nativeBefore)
                throw new DllNotFoundException("Prefix registration attempted native resolution: " + string.Join(" | ", context.NativeLoadAttempts.Skip(nativeBefore)));
            if (context.RejectedManagedRequests.Count != 0)
                throw new FileLoadException("Prefix registration triggered an unplanned managed request: " + string.Join(" | ", context.RejectedManagedRequests));
            var membershipAfter = context.Assemblies.Select(a => a.GetName().FullName ?? a.GetName().Name ?? string.Empty).OrderBy(v => v, StringComparer.Ordinal).ToArray();
            if (!membershipAfter.SequenceEqual(membershipBefore, StringComparer.Ordinal))
                throw new InvalidDataException("Prefix registration changed private-context membership.");
            var postSha1 = ComputeSha1Hex(preflight.Target.PreparedPath);
            if (!postSha1.Equals(targetSha1, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("0Harmony prepared bytes changed across prefix registration.");
            if (HarmonyPatchProbe.TargetCalls != 2 || HarmonyPatchProbe.PrefixCalls != 0)
                throw new InvalidDataException("Prefix registration unexpectedly invoked the launcher probe or prefix.");

            _prefixDescriptor = descriptor;
            _prefixRegistration = new PrefixRegistrationSnapshot(
                postSha1,
                context.ManagedResolverRequests.Count - managedBefore,
                context.PrivateLoads.Count - privateBefore,
                context.HostLoads.Count - hostBefore,
                context.NativeLoadAttempts.Count - nativeBefore);

            return Pass(
                ControlledHarmonyPatchExecutionGate.PrefixRegistration,
                "CONTROLLED HARMONY PREFIX DESCRIPTION REGISTRATION SUCCEEDED WITHOUT PATCHING.\n" +
                "API invoked: PatchProcessor.AddPrefix(System.Reflection.MethodInfo)\n" +
                $"Prefix: {probe.PrefixSignature}\n" +
                "HarmonyMethod constructed: YES — by exact AddPrefix(MethodInfo) only\n" +
                "HarmonyMethod.method retained exact prefix MethodInfo: YES\n" +
                "HarmonyLib.AccessTools type initializer completed explicitly in prior Gate R: YES\n" +
                $"Managed resolver requests during registration: {_prefixRegistration.ManagedResolverRequests:N0}\n" +
                $"Private loads during registration: {_prefixRegistration.PrivateLoads:N0}\n" +
                $"Host loads during registration: {_prefixRegistration.HostLoads:N0}\n" +
                $"Native load attempts during registration: {_prefixRegistration.NativeLoadAttempts:N0}\n" +
                "PatchProcessor.Patch invoked: NO\n" +
                "Launcher target/prefix invoked: NO\n" +
                "StS2 type/member reflected or invoked: NO");
        }
        catch (Exception ex)
        {
            return Fail(ControlledHarmonyPatchExecutionGate.PrefixRegistration, stage, ex);
        }
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Step 27 invokes exactly PatchProcessor.Patch() from the metadata-verified post-publish API surface against a launcher-owned probe.")]
    [UnconditionalSuppressMessage("Trimming", "IL2075", Justification = "Step 27 retains only the exact replacement MethodInfo returned by the verified Patch() call.")]
    public ControlledHarmonyPatchExecutionGateResult RunPatchEngineExecution()
    {
        var stage = "exact PatchProcessor.Patch() invocation";
        try
        {
            ThrowIfDisposed();
            var preflight = RequirePreflight();
            var patchApi = RequirePatchApi();
            var probe = RequirePatchProbe();
            _ = RequirePrefixRegistration();
            var context = RequireLoadContext();
            var processor = _patchProcessorInstance ?? throw new InvalidOperationException("Step 27 retained PatchProcessor instance is missing.");

            VerifyFileLength(preflight.Target.PreparedPath, preflight.Target.Plan.Length, "prepared patch-execution target");
            var targetSha1 = ComputeSha1Hex(preflight.Target.PreparedPath);
            if (!targetSha1.Equals(preflight.Target.Plan.Sha1Hex, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Step 27 0Harmony SHA-1 changed immediately before Patch().");

            var managedBefore = context.ManagedResolverRequests.Count;
            var privateBefore = context.PrivateLoads.Count;
            var hostBefore = context.HostLoads.Count;
            var nativeBefore = context.NativeLoadAttempts.Count;
            var membershipBefore = context.Assemblies.Select(a => a.GetName().FullName ?? a.GetName().Name ?? string.Empty).OrderBy(v => v, StringComparer.Ordinal).ToArray();

            object? rawReplacement;
            try
            {
                rawReplacement = patchApi.PatchMethod.Invoke(processor, null);
            }
            catch (TargetInvocationException ex) when (ex.InnerException is not null)
            {
                throw new InvalidOperationException("Exact PatchProcessor.Patch() threw.", ex.InnerException);
            }
            if (rawReplacement is not MethodInfo replacement)
                throw new InvalidDataException("PatchProcessor.Patch() did not return a System.Reflection.MethodInfo replacement.");
            if (replacement.ReturnType != typeof(int))
                throw new InvalidDataException("Harmony replacement return type does not match the launcher-owned Int32 target.");
            var replacementParameters = replacement.GetParameters();
            if (replacementParameters.Length != 1 || replacementParameters[0].ParameterType != typeof(int))
                throw new InvalidDataException("Harmony replacement parameter surface does not match the launcher-owned Int32 target.");

            if (context.NativeLoadAttempts.Count != nativeBefore)
                throw new DllNotFoundException("PatchProcessor.Patch attempted native resolution: " + string.Join(" | ", context.NativeLoadAttempts.Skip(nativeBefore)));
            if (context.RejectedManagedRequests.Count != 0)
                throw new FileLoadException("PatchProcessor.Patch triggered an unplanned managed request: " + string.Join(" | ", context.RejectedManagedRequests));
            var membershipAfter = context.Assemblies.Select(a => a.GetName().FullName ?? a.GetName().Name ?? string.Empty).OrderBy(v => v, StringComparer.Ordinal).ToArray();
            if (!membershipAfter.SequenceEqual(membershipBefore, StringComparer.Ordinal))
                throw new InvalidDataException("PatchProcessor.Patch changed private-context membership.");
            var postSha1 = ComputeSha1Hex(preflight.Target.PreparedPath);
            if (!postSha1.Equals(targetSha1, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("0Harmony prepared bytes changed across Patch().");
            if (HarmonyPatchProbe.TargetCalls != 2 || HarmonyPatchProbe.PrefixCalls != 0)
                throw new InvalidDataException("Patch installation unexpectedly invoked the launcher target or prefix.");

            _replacementMethod = replacement;
            _patchExecution = new PatchExecutionSnapshot(
                postSha1,
                replacement.Name,
                replacement.DeclaringType?.FullName ?? "<dynamic/no-declaring-type>",
                context.ManagedResolverRequests.Count - managedBefore,
                context.PrivateLoads.Count - privateBefore,
                context.HostLoads.Count - hostBefore,
                context.NativeLoadAttempts.Count - nativeBefore);

            return Pass(
                ControlledHarmonyPatchExecutionGate.PatchEngineExecution,
                "FIRST REAL HARMONY PATCH ENGINE EXECUTION COMPLETED AGAINST LAUNCHER-OWNED TARGET.\n" +
                "API invoked: HarmonyLib.PatchProcessor::Patch() — EXACTLY ONCE\n" +
                $"Original target: {probe.TargetSignature}\n" +
                $"Registered prefix: {probe.PrefixSignature}\n" +
                $"Replacement MethodInfo: {_patchExecution.ReplacementDeclaringType}::{_patchExecution.ReplacementName}\n" +
                $"Managed resolver requests during Patch(): {_patchExecution.ManagedResolverRequests:N0}\n" +
                $"Private loads during Patch(): {_patchExecution.PrivateLoads:N0}\n" +
                $"Host loads during Patch(): {_patchExecution.HostLoads:N0}\n" +
                $"Native load attempts during Patch(): {_patchExecution.NativeLoadAttempts:N0}\n" +
                "Launcher target invoked after patch: NO — Gate V owns execution of patched behavior\n" +
                "StS2 type/member reflected, patched, or invoked: NO");
        }
        catch (Exception ex)
        {
            return Fail(ControlledHarmonyPatchExecutionGate.PatchEngineExecution, stage, ex);
        }
    }

    [UnconditionalSuppressMessage("Trimming", "IL2075", Justification = "Step 27 post-patch audit reads only exact reflection objects resolved by earlier gates.")]
    public async Task<ControlledHarmonyPatchExecutionGateResult> RunPostPatchAuditAsync(
        IProgress<ControlledHarmonyPatchExecutionProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var stage = "post-patch audit";
        try
        {
            ThrowIfDisposed();
            var preflight = RequirePreflight();
            var patchApi = RequirePatchApi();
            var probe = RequirePatchProbe();
            var registration = RequirePrefixRegistration();
            var patchExecution = RequirePatchExecution();
            var context = RequireLoadContext();
            var processor = _patchProcessorInstance ?? throw new InvalidOperationException("Step 27 retained PatchProcessor instance is missing.");
            var descriptor = _prefixDescriptor ?? throw new InvalidOperationException("Step 27 retained HarmonyMethod prefix descriptor is missing.");
            _ = _replacementMethod ?? throw new InvalidOperationException("Step 27 replacement MethodInfo is missing after Patch().");

            stage = "runtime plan rehash";
            var planSha256 = await ComputeSha256HexAsync(_planPath, cancellationToken).ConfigureAwait(false);
            if (!planSha256.Equals(preflight.PlanSha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Step 27 runtime-binding plan changed during Patch().");

            stage = "prepared/live byte audit";
            var verified = 0;
            foreach (var item in preflight.PreparedAssemblies)
            {
                cancellationToken.ThrowIfCancellationRequested();
                VerifyFileLength(item.PreparedPath, item.Plan.Length, "prepared post-patch");
                VerifyFileLength(item.LivePath, item.Plan.Length, "live post-patch");
                var preparedSha1 = await ComputeSha1HexAsync(item.PreparedPath, cancellationToken).ConfigureAwait(false);
                var liveSha1 = await ComputeSha1HexAsync(item.LivePath, cancellationToken).ConfigureAwait(false);
                if (!preparedSha1.Equals(item.Plan.Sha1Hex, StringComparison.OrdinalIgnoreCase) || !liveSha1.Equals(item.Plan.Sha1Hex, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException("Step 27 prepared/live byte identity changed during Patch(): " + item.Plan.RelativePath);
                verified++;
                progress?.Report(new ControlledHarmonyPatchExecutionProgress(
                    ControlledHarmonyPatchExecutionGate.PostPatchAudit,
                    verified,
                    preflight.PreparedAssemblies.Length,
                    item.Plan.RelativePath,
                    "Re-hashing prepared/live bytes after Patch() but before patched target invocation…"));
            }

            stage = "OfflineReady post-patch pre-invocation check";
            var offline = await _offlineInspection.RunAsync(null, cancellationToken).ConfigureAwait(false);
            if (!offline.Success || !offline.ExactManagedTreeVerified || offline.InstalledManifestId != preflight.Plan.ManifestId)
                throw new InvalidDataException(offline.Error ?? "OfflineReady exact-tree verification failed after Patch().");

            stage = "post-patch state audit";
            var expected = preflight.PreparedAssemblies
                .Where(item => item.ModuleInitializerCount == 0 || item.Plan.AssemblyFullName.Equals(preflight.Target.Plan.AssemblyFullName, StringComparison.Ordinal))
                .Select(item => item.Plan.AssemblyFullName).OrderBy(value => value, StringComparer.Ordinal).ToArray();
            var actual = context.Assemblies.Select(a => a.GetName().FullName ?? a.GetName().Name ?? string.Empty).OrderBy(v => v, StringComparer.Ordinal).ToArray();
            if (!actual.SequenceEqual(expected, StringComparer.Ordinal))
                throw new InvalidDataException("Step 27 post-patch private context differs from the physically proven Step 26 context membership.");
            if (context.NativeLoadAttempts.Count != 0)
                throw new DllNotFoundException("Step 27 observed native-library resolution during patch installation: " + string.Join(" | ", context.NativeLoadAttempts));
            if (context.RejectedManagedRequests.Count != 0)
                throw new FileLoadException("Step 27 observed rejected/unplanned managed requests during patch installation: " + string.Join(" | ", context.RejectedManagedRequests));
            if (!ReferenceEquals(patchApi.PrefixField.GetValue(processor), descriptor) || !ReferenceEquals(patchApi.HarmonyMethodMethodField.GetValue(descriptor), probe.Prefix))
                throw new InvalidDataException("Step 27 registered prefix descriptor changed across Patch().");
            if (HarmonyPatchProbe.TargetCalls != 2 || HarmonyPatchProbe.PrefixCalls != 0)
                throw new InvalidDataException("Patch installation audit observed unexpected launcher target/prefix invocation.");
            var targetSha1 = await ComputeSha1HexAsync(preflight.Target.PreparedPath, cancellationToken).ConfigureAwait(false);
            if (!targetSha1.Equals(registration.PreparedSha1, StringComparison.OrdinalIgnoreCase) || !targetSha1.Equals(patchExecution.PreparedSha1, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Step 27 0Harmony prepared hash changed across prefix registration/Patch().");

            _postPatchAuditPassed = true;
            return Pass(
                ControlledHarmonyPatchExecutionGate.PostPatchAudit,
                "POST-PATCH ISOLATION AUDIT PASSED BEFORE ANY PATCHED TARGET INVOCATION.\n" +
                $"Prepared/live assemblies re-hashed: {verified:N0}/{preflight.PreparedAssemblies.Length:N0}\n" +
                $"Runtime plan SHA-256 unchanged: {planSha256}\n" +
                $"Private context: {actual.Length:N0}/{expected.Length:N0} expected assemblies\n" +
                "OfflineReady exact-tree verification: YES\n" +
                "Registered HarmonyMethod still retains exact launcher prefix: YES\n" +
                "Launcher target/prefix invocation count unchanged since baseline: YES\n" +
                "Native load attempts: 0\n" +
                "Rejected/unplanned managed requests: 0\n" +
                "StS2 type/member reflected, patched, or invoked: NO");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return Fail(ControlledHarmonyPatchExecutionGate.PostPatchAudit, stage, ex);
        }
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Step 27 invokes only the exact launcher-owned MethodInfo rooted and verified in Gate P.")]
    public ControlledHarmonyPatchExecutionGateResult RunPatchedProbeInvocation()
    {
        var stage = "patched launcher probe invocation";
        try
        {
            ThrowIfDisposed();
            var probe = RequirePatchProbe();
            _ = RequirePatchExecution();
            if (!_postPatchAuditPassed)
                throw new InvalidOperationException("Step 27 Gate U must pass before invoking patched launcher behavior.");
            var context = RequireLoadContext();
            var membershipBefore = context.Assemblies.Select(a => a.GetName().FullName ?? a.GetName().Name ?? string.Empty).OrderBy(v => v, StringComparer.Ordinal).ToArray();
            var nativeBefore = context.NativeLoadAttempts.Count;

            if (HarmonyPatchProbe.TargetCalls != 2 || HarmonyPatchProbe.PrefixCalls != 0)
                throw new InvalidDataException("Step 27 patched invocation did not begin from the established baseline counters.");

            stage = "patched reflection invocation";
            var reflectedRaw = probe.Target.Invoke(null, [41]);
            if (reflectedRaw is not int reflectedResult)
                throw new InvalidDataException("Patched reflection invocation did not return System.Int32.");
            var afterReflectionTargetCalls = HarmonyPatchProbe.TargetCalls;
            var afterReflectionPrefixCalls = HarmonyPatchProbe.PrefixCalls;

            stage = "patched direct invocation";
            var directResult = HarmonyPatchProbe.Target(41);
            var finalTargetCalls = HarmonyPatchProbe.TargetCalls;
            var finalPrefixCalls = HarmonyPatchProbe.PrefixCalls;

            if (reflectedResult != 1041 || afterReflectionTargetCalls != 2 || afterReflectionPrefixCalls != 1)
                throw new InvalidDataException($"Patched reflection route did not execute the exact prefix/skip-original behavior: result={reflectedResult}, targetCalls={afterReflectionTargetCalls}, prefixCalls={afterReflectionPrefixCalls}.");
            if (directResult != 1041 || finalTargetCalls != 2 || finalPrefixCalls != 2)
                throw new InvalidDataException($"Patched direct route did not execute the exact prefix/skip-original behavior: result={directResult}, targetCalls={finalTargetCalls}, prefixCalls={finalPrefixCalls}.");
            if (context.NativeLoadAttempts.Count != nativeBefore || context.RejectedManagedRequests.Count != 0)
                throw new InvalidDataException("Patched launcher invocation caused native or rejected managed resolution.");
            var membershipAfter = context.Assemblies.Select(a => a.GetName().FullName ?? a.GetName().Name ?? string.Empty).OrderBy(v => v, StringComparer.Ordinal).ToArray();
            if (!membershipAfter.SequenceEqual(membershipBefore, StringComparer.Ordinal))
                throw new InvalidDataException("Patched launcher invocation changed private-context membership.");

            _patchedProbeInvocation = new ProbeInvocationSnapshot(reflectedResult, directResult, finalTargetCalls, finalPrefixCalls);
            return Pass(
                ControlledHarmonyPatchExecutionGate.PatchedProbeInvocation,
                "LAUNCHER-OWNED PATCHED METHOD EXECUTION SUCCEEDED THROUGH REFLECTION AND DIRECT CALL ROUTES.\n" +
                "Input: 41\n" +
                $"Patched reflection result: {reflectedResult}\n" +
                $"Patched direct result: {directResult}\n" +
                $"Target-body calls after both patched invocations: {finalTargetCalls} — unchanged from baseline 2\n" +
                $"Prefix calls after both patched invocations: {finalPrefixCalls}\n" +
                "Prefix set __result = value + 1000 and returned false: PHYSICALLY OBSERVED\n" +
                "Original target body skipped by both patched routes: YES\n" +
                "StS2 type/member reflected, patched, or invoked: NO");
        }
        catch (TargetInvocationException ex) when (ex.InnerException is not null)
        {
            return Fail(ControlledHarmonyPatchExecutionGate.PatchedProbeInvocation, stage, ex.InnerException);
        }
        catch (Exception ex)
        {
            return Fail(ControlledHarmonyPatchExecutionGate.PatchedProbeInvocation, stage, ex);
        }
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Step 27 invokes exactly PatchProcessor.Unpatch(MethodInfo) with the launcher-owned prefix already verified in Gate P.")]
    public ControlledHarmonyPatchExecutionGateResult RunExactPrefixUnpatch()
    {
        var stage = "exact PatchProcessor.Unpatch(MethodInfo) invocation";
        try
        {
            ThrowIfDisposed();
            var preflight = RequirePreflight();
            var patchApi = RequirePatchApi();
            var probe = RequirePatchProbe();
            _ = RequirePatchedProbeInvocation();
            var context = RequireLoadContext();
            var processor = _patchProcessorInstance ?? throw new InvalidOperationException("Step 27 retained PatchProcessor instance is missing.");

            VerifyFileLength(preflight.Target.PreparedPath, preflight.Target.Plan.Length, "prepared unpatch target");
            var targetSha1 = ComputeSha1Hex(preflight.Target.PreparedPath);
            if (!targetSha1.Equals(preflight.Target.Plan.Sha1Hex, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Step 27 0Harmony SHA-1 changed immediately before exact prefix unpatch.");

            var managedBefore = context.ManagedResolverRequests.Count;
            var privateBefore = context.PrivateLoads.Count;
            var hostBefore = context.HostLoads.Count;
            var nativeBefore = context.NativeLoadAttempts.Count;
            var membershipBefore = context.Assemblies.Select(a => a.GetName().FullName ?? a.GetName().Name ?? string.Empty).OrderBy(v => v, StringComparer.Ordinal).ToArray();
            var targetCallsBefore = HarmonyPatchProbe.TargetCalls;
            var prefixCallsBefore = HarmonyPatchProbe.PrefixCalls;

            object? returned;
            try
            {
                returned = patchApi.UnpatchMethod.Invoke(processor, [probe.Prefix]);
            }
            catch (TargetInvocationException ex) when (ex.InnerException is not null)
            {
                throw new InvalidOperationException("Exact PatchProcessor.Unpatch(MethodInfo) threw.", ex.InnerException);
            }
            if (!ReferenceEquals(returned, processor))
                throw new InvalidDataException("PatchProcessor.Unpatch(MethodInfo) did not return the same processor instance.");
            if (HarmonyPatchProbe.TargetCalls != targetCallsBefore || HarmonyPatchProbe.PrefixCalls != prefixCallsBefore)
                throw new InvalidDataException("Exact prefix unpatch unexpectedly invoked the launcher target or prefix.");
            if (context.NativeLoadAttempts.Count != nativeBefore)
                throw new DllNotFoundException("Exact prefix unpatch attempted native resolution: " + string.Join(" | ", context.NativeLoadAttempts.Skip(nativeBefore)));
            if (context.RejectedManagedRequests.Count != 0)
                throw new FileLoadException("Exact prefix unpatch triggered an unplanned managed request: " + string.Join(" | ", context.RejectedManagedRequests));
            var membershipAfter = context.Assemblies.Select(a => a.GetName().FullName ?? a.GetName().Name ?? string.Empty).OrderBy(v => v, StringComparer.Ordinal).ToArray();
            if (!membershipAfter.SequenceEqual(membershipBefore, StringComparer.Ordinal))
                throw new InvalidDataException("Exact prefix unpatch changed private-context membership.");
            var postSha1 = ComputeSha1Hex(preflight.Target.PreparedPath);
            if (!postSha1.Equals(targetSha1, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("0Harmony prepared bytes changed across exact prefix unpatch.");

            _unpatch = new UnpatchSnapshot(
                postSha1,
                context.ManagedResolverRequests.Count - managedBefore,
                context.PrivateLoads.Count - privateBefore,
                context.HostLoads.Count - hostBefore,
                context.NativeLoadAttempts.Count - nativeBefore,
                targetCallsBefore,
                prefixCallsBefore);

            return Pass(
                ControlledHarmonyPatchExecutionGate.ExactPrefixUnpatch,
                "EXACT LAUNCHER-OWNED HARMONY PREFIX REMOVAL COMPLETED.\n" +
                "API invoked: PatchProcessor.Unpatch(System.Reflection.MethodInfo) — exact prefix MethodInfo only\n" +
                $"Removed prefix: {probe.PrefixSignature}\n" +
                $"Managed resolver requests during unpatch: {_unpatch.ManagedResolverRequests:N0}\n" +
                $"Private loads during unpatch: {_unpatch.PrivateLoads:N0}\n" +
                $"Host loads during unpatch: {_unpatch.HostLoads:N0}\n" +
                $"Native load attempts during unpatch: {_unpatch.NativeLoadAttempts:N0}\n" +
                "Launcher target/prefix invoked during unpatch: NO\n" +
                "Restored behavior not yet invoked — Gate Y owns that proof\n" +
                "StS2 type/member reflected, patched, or invoked: NO");
        }
        catch (Exception ex)
        {
            return Fail(ControlledHarmonyPatchExecutionGate.ExactPrefixUnpatch, stage, ex);
        }
    }

    public ControlledHarmonyPatchExecutionGateResult RunPostUnpatchAudit()
    {
        var stage = "post-unpatch audit";
        try
        {
            ThrowIfDisposed();
            var preflight = RequirePreflight();
            var unpatch = RequireUnpatch();
            var context = RequireLoadContext();

            var targetSha1 = ComputeSha1Hex(preflight.Target.PreparedPath);
            if (!targetSha1.Equals(unpatch.PreparedSha1, StringComparison.OrdinalIgnoreCase) || !targetSha1.Equals(preflight.Target.Plan.Sha1Hex, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Step 27 0Harmony prepared bytes changed after exact prefix unpatch.");
            var expected = preflight.PreparedAssemblies
                .Where(item => item.ModuleInitializerCount == 0 || item.Plan.AssemblyFullName.Equals(preflight.Target.Plan.AssemblyFullName, StringComparison.Ordinal))
                .Select(item => item.Plan.AssemblyFullName).OrderBy(value => value, StringComparer.Ordinal).ToArray();
            var actual = context.Assemblies.Select(a => a.GetName().FullName ?? a.GetName().Name ?? string.Empty).OrderBy(v => v, StringComparer.Ordinal).ToArray();
            if (!actual.SequenceEqual(expected, StringComparer.Ordinal))
                throw new InvalidDataException("Step 27 post-unpatch private context differs from the physically proven Step 26 context membership.");
            if (context.NativeLoadAttempts.Count != 0)
                throw new DllNotFoundException("Step 27 observed native-library resolution during patch/unpatch: " + string.Join(" | ", context.NativeLoadAttempts));
            if (context.RejectedManagedRequests.Count != 0)
                throw new FileLoadException("Step 27 observed rejected/unplanned managed requests during patch/unpatch: " + string.Join(" | ", context.RejectedManagedRequests));
            if (HarmonyPatchProbe.TargetCalls != unpatch.TargetCallsAtUnpatch || HarmonyPatchProbe.PrefixCalls != unpatch.PrefixCallsAtUnpatch)
                throw new InvalidDataException("Step 27 post-unpatch audit observed unexpected launcher target/prefix invocation.");

            _postUnpatchAuditPassed = true;
            return Pass(
                ControlledHarmonyPatchExecutionGate.PostUnpatchAudit,
                "POST-UNPATCH ISOLATION AUDIT PASSED BEFORE RESTORED TARGET INVOCATION.\n" +
                $"Private context: {actual.Length:N0}/{expected.Length:N0} expected assemblies\n" +
                "0Harmony prepared SHA-1 unchanged: YES\n" +
                "Native load attempts: 0\n" +
                "Rejected/unplanned managed requests: 0\n" +
                $"Target calls remain: {HarmonyPatchProbe.TargetCalls}\n" +
                $"Prefix calls remain: {HarmonyPatchProbe.PrefixCalls}\n" +
                "Restored launcher behavior not yet invoked: YES\n" +
                "StS2 type/member reflected, patched, or invoked: NO");
        }
        catch (Exception ex)
        {
            return Fail(ControlledHarmonyPatchExecutionGate.PostUnpatchAudit, stage, ex);
        }
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Step 27 invokes only the exact launcher-owned MethodInfo rooted and verified in Gate P.")]
    public ControlledHarmonyPatchExecutionGateResult RunRestoredProbeInvocation()
    {
        var stage = "restored launcher probe invocation";
        try
        {
            ThrowIfDisposed();
            var probe = RequirePatchProbe();
            _ = RequireUnpatch();
            if (!_postUnpatchAuditPassed)
                throw new InvalidOperationException("Step 27 Gate X must pass before verifying restored launcher behavior.");
            var context = RequireLoadContext();
            var membershipBefore = context.Assemblies.Select(a => a.GetName().FullName ?? a.GetName().Name ?? string.Empty).OrderBy(v => v, StringComparer.Ordinal).ToArray();
            var nativeBefore = context.NativeLoadAttempts.Count;

            var targetCallsBefore = HarmonyPatchProbe.TargetCalls;
            var prefixCallsBefore = HarmonyPatchProbe.PrefixCalls;
            if (targetCallsBefore != 2 || prefixCallsBefore != 2)
                throw new InvalidDataException($"Step 27 restored invocation expected patched-phase counters target=2/prefix=2, observed target={targetCallsBefore}/prefix={prefixCallsBefore}.");

            stage = "restored reflection invocation";
            var reflectedRaw = probe.Target.Invoke(null, [41]);
            if (reflectedRaw is not int reflectedResult)
                throw new InvalidDataException("Restored reflection invocation did not return System.Int32.");
            stage = "restored direct invocation";
            var directResult = HarmonyPatchProbe.Target(41);
            var targetCalls = HarmonyPatchProbe.TargetCalls;
            var prefixCalls = HarmonyPatchProbe.PrefixCalls;

            if (reflectedResult != 42 || directResult != 42 || targetCalls != 4 || prefixCalls != 2)
                throw new InvalidDataException($"Exact unpatch did not restore baseline behavior on both invocation routes: reflection={reflectedResult}, direct={directResult}, targetCalls={targetCalls}, prefixCalls={prefixCalls}.");
            if (context.NativeLoadAttempts.Count != nativeBefore || context.RejectedManagedRequests.Count != 0)
                throw new InvalidDataException("Restored launcher invocation caused native or rejected managed resolution.");
            var membershipAfter = context.Assemblies.Select(a => a.GetName().FullName ?? a.GetName().Name ?? string.Empty).OrderBy(v => v, StringComparer.Ordinal).ToArray();
            if (!membershipAfter.SequenceEqual(membershipBefore, StringComparer.Ordinal))
                throw new InvalidDataException("Restored launcher invocation changed private-context membership.");

            _restoredProbeInvocation = new ProbeInvocationSnapshot(reflectedResult, directResult, targetCalls, prefixCalls);
            return Pass(
                ControlledHarmonyPatchExecutionGate.RestoredProbeInvocation,
                "LAUNCHER-OWNED ORIGINAL BEHAVIOR RESTORED AFTER EXACT PREFIX UNPATCH.\n" +
                "Input: 41\n" +
                $"Restored reflection result: {reflectedResult}\n" +
                $"Restored direct result: {directResult}\n" +
                $"Target-body calls: {targetCalls}\n" +
                $"Prefix calls: {prefixCalls} — unchanged across restored invocations\n" +
                "Original value + 1 behavior restored on both invocation routes: YES\n" +
                "StS2 type/member reflected, patched, or invoked: NO");
        }
        catch (TargetInvocationException ex) when (ex.InnerException is not null)
        {
            return Fail(ControlledHarmonyPatchExecutionGate.RestoredProbeInvocation, stage, ex.InnerException);
        }
        catch (Exception ex)
        {
            return Fail(ControlledHarmonyPatchExecutionGate.RestoredProbeInvocation, stage, ex);
        }
    }

    [UnconditionalSuppressMessage("Trimming", "IL2075", Justification = "Step 27 final audit reads only exact reflection objects already resolved by prior gates.")]
    public async Task<ControlledHarmonyPatchExecutionGateResult> RunFinalIsolationAuditAsync(
        IProgress<ControlledHarmonyPatchExecutionProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var stage = "final Step 27 isolation audit";
        try
        {
            ThrowIfDisposed();
            var preflight = RequirePreflight();
            var harmonyApi = RequireHarmonyApi();
            var processorApi = RequireProcessorApi();
            var patchApi = RequirePatchApi();
            var probe = RequirePatchProbe();
            var registration = RequirePrefixRegistration();
            var patchExecution = RequirePatchExecution();
            var unpatch = RequireUnpatch();
            var restored = RequireRestoredProbeInvocation();
            var context = RequireLoadContext();
            var harmonyInstance = _harmonyInstance ?? throw new InvalidOperationException("Step 27 retained Harmony instance is missing.");
            var processor = _patchProcessorInstance ?? throw new InvalidOperationException("Step 27 retained PatchProcessor instance is missing.");
            var descriptor = _prefixDescriptor ?? throw new InvalidOperationException("Step 27 retained prefix descriptor is missing.");

            stage = "runtime plan final rehash";
            var planSha256 = await ComputeSha256HexAsync(_planPath, cancellationToken).ConfigureAwait(false);
            if (!planSha256.Equals(preflight.PlanSha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Step 27 runtime-binding plan changed during patch/unpatch execution.");

            stage = "prepared/live final byte audit";
            var verified = 0;
            foreach (var item in preflight.PreparedAssemblies)
            {
                cancellationToken.ThrowIfCancellationRequested();
                VerifyFileLength(item.PreparedPath, item.Plan.Length, "prepared final Step 27");
                VerifyFileLength(item.LivePath, item.Plan.Length, "live final Step 27");
                var preparedSha1 = await ComputeSha1HexAsync(item.PreparedPath, cancellationToken).ConfigureAwait(false);
                var liveSha1 = await ComputeSha1HexAsync(item.LivePath, cancellationToken).ConfigureAwait(false);
                if (!preparedSha1.Equals(item.Plan.Sha1Hex, StringComparison.OrdinalIgnoreCase) || !liveSha1.Equals(item.Plan.Sha1Hex, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException("Step 27 prepared/live byte identity changed: " + item.Plan.RelativePath);
                verified++;
                progress?.Report(new ControlledHarmonyPatchExecutionProgress(
                    ControlledHarmonyPatchExecutionGate.FinalIsolationAudit,
                    verified,
                    preflight.PreparedAssemblies.Length,
                    item.Plan.RelativePath,
                    "Final re-hash after patch, patched invocation, exact unpatch, and restored invocation…"));
            }

            stage = "OfflineReady final postcondition";
            var offline = await _offlineInspection.RunAsync(null, cancellationToken).ConfigureAwait(false);
            if (!offline.Success || !offline.ExactManagedTreeVerified || offline.InstalledManifestId != preflight.Plan.ManifestId)
                throw new InvalidDataException(offline.Error ?? "OfflineReady exact-tree verification failed after Step 27 patch/unpatch cycle.");

            stage = "final runtime state audit";
            var expected = preflight.PreparedAssemblies
                .Where(item => item.ModuleInitializerCount == 0 || item.Plan.AssemblyFullName.Equals(preflight.Target.Plan.AssemblyFullName, StringComparison.Ordinal))
                .Select(item => item.Plan.AssemblyFullName).OrderBy(value => value, StringComparer.Ordinal).ToArray();
            var actual = context.Assemblies.Select(a => a.GetName().FullName ?? a.GetName().Name ?? string.Empty).OrderBy(v => v, StringComparer.Ordinal).ToArray();
            if (!actual.SequenceEqual(expected, StringComparer.Ordinal))
                throw new InvalidDataException("Step 27 final private context differs from the physically proven Step 26 context membership.");
            if (context.NativeLoadAttempts.Count != 0)
                throw new DllNotFoundException("Step 27 observed native-library resolution: " + string.Join(" | ", context.NativeLoadAttempts));
            if (context.RejectedManagedRequests.Count != 0)
                throw new FileLoadException("Step 27 observed rejected/unplanned managed requests: " + string.Join(" | ", context.RejectedManagedRequests));
            if (!ReferenceEquals(processor.GetType(), processorApi.PatchProcessorType) || !ReferenceEquals(processorApi.InstanceField.GetValue(processor), harmonyInstance))
                throw new InvalidDataException("Step 27 retained PatchProcessor/Harmony identity changed.");
            if (!ReferenceEquals(patchApi.PrefixField.GetValue(processor), descriptor) || !ReferenceEquals(patchApi.HarmonyMethodMethodField.GetValue(descriptor), probe.Prefix))
                throw new InvalidDataException("Step 27 retained prefix descriptor identity changed.");
            if (harmonyApi.DebugField.GetValue(null) is not bool debug || debug)
                throw new InvalidDataException("Step 27 Harmony.DEBUG is true during final audit.");
            var targetSha1 = await ComputeSha1HexAsync(preflight.Target.PreparedPath, cancellationToken).ConfigureAwait(false);
            if (!targetSha1.Equals(registration.PreparedSha1, StringComparison.OrdinalIgnoreCase) ||
                !targetSha1.Equals(patchExecution.PreparedSha1, StringComparison.OrdinalIgnoreCase) ||
                !targetSha1.Equals(unpatch.PreparedSha1, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Step 27 0Harmony prepared hash changed across patch lifecycle.");
            if (restored.ReflectionResult != 42 || restored.DirectResult != 42 || restored.TargetCalls != 4 || restored.PrefixCalls != 2)
                throw new InvalidDataException("Step 27 restored-behavior snapshot changed before final audit.");

            return Pass(
                ControlledHarmonyPatchExecutionGate.FinalIsolationAudit,
                "STEP 27 FINAL PATCH/UNPATCH ISOLATION AUDIT PASSED.\n" +
                $"Prepared/live assemblies re-hashed: {verified:N0}/{preflight.PreparedAssemblies.Length:N0}\n" +
                $"Runtime plan SHA-256 unchanged: {planSha256}\n" +
                $"Private context: {actual.Length:N0}/{expected.Length:N0} expected assemblies\n" +
                "OfflineReady exact-tree verification: YES\n" +
                "Harmony.DEBUG: false\n" +
                "Patch lifecycle: AddPrefix(MethodInfo) → Patch() → patched reflection/direct invocation → Unpatch(MethodInfo) → restored reflection/direct invocation\n" +
                "Patched result: 1041 on both routes\n" +
                "Restored result: 42 on both routes\n" +
                $"Final launcher target calls: {restored.TargetCalls}\n" +
                $"Final launcher prefix calls: {restored.PrefixCalls}\n" +
                "Native load attempts: 0\n" +
                "Rejected/unplanned managed requests: 0\n" +
                "Trusted Step 12 managed install unchanged: YES\n" +
                "Prepared Step 21/22 bytes unchanged: YES\n" +
                "StS2 entry point/type/member reflection, patching, or invocation: NO\n" +
                "Godot/game initialization: NO\n" +
                "Native game library loaded by Step 27: NO\n" +
                "Process note: the Step 27 private managed context and Harmony runtime patch state remain process-resident until exit; force-quit before rerunning earlier fresh-process regressions.");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return Fail(ControlledHarmonyPatchExecutionGate.FinalIsolationAudit, stage, ex);
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;
        ClearStep27ObjectState();
        ReleaseLoadContext();
        _step23Preflight.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    private void ClearStep27ObjectState()
    {
        _replacementMethod = null;
        _prefixDescriptor = null;
        _restoredProbeInvocation = null;
        _unpatch = null;
        _patchedProbeInvocation = null;
        _patchExecution = null;
        _prefixRegistration = null;
        _baselineProbeInvocation = null;
        _patchProbe = null;
        _patchApi = null;
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
        HarmonyPatchProbe.ResetCounters();
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
            throw new InvalidDataException("Step 27 requires a fresh process; a game/Harmony assembly is already loaded: " + string.Join(" | ", matches));
    }



    private static AccessToolsMetadataSnapshot ReadAccessToolsMetadata(string path)
    {
        using var resolver = new Step27MetadataOnlyResolver(path);
        using var module = ModuleDefinition.ReadModule(path, new ReaderParameters
        {
            InMemory = true,
            ReadSymbols = false,
            ReadingMode = ReadingMode.Deferred,
            AssemblyResolver = resolver,
            MetadataResolver = resolver,
        });
        if (module.Assembly?.Name is null)
            throw new BadImageFormatException("Managed assembly manifest missing while auditing Harmony AccessTools: " + path);

        var accessTools = EnumerateTypes(module.Types).SingleOrDefault(type => type.FullName.Equals(AccessToolsTypeFullName, StringComparison.Ordinal));
        if (accessTools is null)
            return new AccessToolsMetadataSnapshot(false, "HarmonyLib.AccessTools is missing from exact 0Harmony.", "<missing>");
        if (!accessTools.IsPublic || !accessTools.IsAbstract || !accessTools.IsSealed || accessTools.IsInterface)
            return new AccessToolsMetadataSnapshot(false, "HarmonyLib.AccessTools is no longer the exact public static-class shape.", "<invalid type>");

        var hazards = new List<string>();
        FieldDefinition? ExactField(string name, string typeName, bool requirePublic = false, bool requireInitOnly = false)
        {
            var matches = accessTools.Fields.Where(field => field.Name.Equals(name, StringComparison.Ordinal)).ToArray();
            if (matches.Length != 1)
            {
                hazards.Add($"AccessTools field {name} count changed: {matches.Length}.");
                return null;
            }
            var field = matches[0];
            if (!field.IsStatic || !field.FieldType.FullName.Equals(typeName, StringComparison.Ordinal) ||
                (requirePublic && !field.IsPublic) || (requireInitOnly && !field.IsInitOnly))
            {
                hazards.Add($"AccessTools field {name} shape changed: {field.FullName}.");
                return null;
            }
            return field;
        }

        var allTypesCached = ExactField("allTypesCached", "System.Type[]");
        var allField = ExactField("all", "System.Reflection.BindingFlags", requirePublic: true, requireInitOnly: true);
        var allDeclaredField = ExactField("allDeclared", "System.Reflection.BindingFlags", requirePublic: true, requireInitOnly: true);
        var isMonoRuntime = ExactField("<IsMonoRuntime>k__BackingField", "System.Boolean");
        var isNetFrameworkRuntime = ExactField("<IsNetFrameworkRuntime>k__BackingField", "System.Boolean");
        var isNetCoreRuntime = ExactField("<IsNetCoreRuntime>k__BackingField", "System.Boolean");
        var addHandlerCache = ExactField("addHandlerCache", "System.Collections.Generic.Dictionary`2<System.Type,HarmonyLib.FastInvokeHandler>");
        var addHandlerCacheLock = ExactField("addHandlerCacheLock", "System.Threading.ReaderWriterLockSlim");

        var typeInitializers = accessTools.Methods.Where(method => method.IsConstructor && method.IsStatic).ToArray();
        if (typeInitializers.Length != 1 || !typeInitializers[0].HasBody)
            return new AccessToolsMetadataSnapshot(false, $"Expected exactly one managed AccessTools type initializer; observed {typeInitializers.Length}.", typeInitializers.Length == 0 ? "<missing>" : string.Join("\n", typeInitializers.Select(m => m.FullName)));
        var cctor = typeInitializers[0];
        if (cctor.IsPInvokeImpl || cctor.PInvokeInfo is not null || cctor.Body.ExceptionHandlers.Count != 0 || cctor.Body.Variables.Count != 0)
            hazards.Add("AccessTools type initializer contains P/Invoke, handlers, or locals outside the physically measured runtime-detection/cache shape.");
        if (cctor.Body.Instructions.Count != 56)
            hazards.Add($"AccessTools type initializer instruction count changed: observed {cctor.Body.Instructions.Count}, expected 56.");

        var expectedOpcodeCounts = new Dictionary<Code, int>
        {
            [Code.Ldnull] = 6,
            [Code.Stsfld] = 8,
            [Code.Ldc_I4] = 1,
            [Code.Ldsfld] = 1,
            [Code.Ldc_I4_2] = 1,
            [Code.Or] = 1,
            [Code.Ldc_I4_0] = 5,
            [Code.Ldstr] = 7,
            [Code.Call] = 6,
            [Code.Callvirt] = 6,
            [Code.Ceq] = 3,
            [Code.Dup] = 2,
            [Code.Brtrue_S] = 2,
            [Code.Pop] = 2,
            [Code.Br_S] = 2,
            [Code.Newobj] = 2,
            [Code.Ret] = 1,
        };
        var observedOpcodeCounts = cctor.Body.Instructions
            .GroupBy(instruction => instruction.OpCode.Code)
            .ToDictionary(group => group.Key, group => group.Count());
        foreach (var expected in expectedOpcodeCounts)
        {
            observedOpcodeCounts.TryGetValue(expected.Key, out var observed);
            if (observed != expected.Value)
                hazards.Add($"AccessTools::.cctor opcode count changed for {expected.Key}: observed {observed}, expected {expected.Value}.");
        }
        foreach (var unexpected in observedOpcodeCounts.Keys.Except(expectedOpcodeCounts.Keys).OrderBy(code => code.ToString(), StringComparer.Ordinal))
            hazards.Add($"AccessTools::.cctor contains unexpected opcode {unexpected} ({observedOpcodeCounts[unexpected]} occurrence(s)).");

        var expectedStrings = new[]
        {
            "Mono.Runtime",
            "System.Runtime.InteropServices.RuntimeInformation",
            "FrameworkDescription",
            ".NET Framework",
            "System.Runtime.InteropServices.RuntimeInformation",
            "FrameworkDescription",
            ".NET Core",
        };
        var observedStrings = cctor.Body.Instructions.Where(instruction => instruction.OpCode.Code == Code.Ldstr).Select(instruction => instruction.Operand as string ?? string.Empty).ToArray();
        if (!observedStrings.SequenceEqual(expectedStrings, StringComparer.Ordinal))
            hazards.Add("AccessTools::.cctor string-literal sequence changed: " + FormatNames(observedStrings));

        var expectedMethodOperands = new[]
        {
            "System.Type System.Type::GetType(System.String)",
            "System.Type System.Type::GetType(System.String,System.Boolean)",
            "System.Type System.Type::GetType(System.String,System.Boolean)",
            "System.Boolean HarmonyLib.AccessTools::get_IsMonoRuntime()",
            "System.Reflection.PropertyInfo System.Type::GetProperty(System.String)",
            "System.Reflection.PropertyInfo System.Type::GetProperty(System.String)",
            "System.Object System.Reflection.PropertyInfo::GetValue(System.Object,System.Object[])",
            "System.Object System.Reflection.PropertyInfo::GetValue(System.Object,System.Object[])",
            "System.String System.Object::ToString()",
            "System.String System.Object::ToString()",
            "System.Boolean System.String::StartsWith(System.String)",
            "System.Boolean System.String::StartsWith(System.String)",
            "System.Void System.Collections.Generic.Dictionary`2<System.Type,HarmonyLib.FastInvokeHandler>::.ctor()",
            "System.Void System.Threading.ReaderWriterLockSlim::.ctor(System.Threading.LockRecursionPolicy)",
        }.OrderBy(value => value, StringComparer.Ordinal).ToArray();
        var observedMethodOperands = cctor.Body.Instructions
            .Where(instruction => instruction.OpCode.Code is Code.Call or Code.Callvirt or Code.Newobj)
            .Select(instruction => instruction.Operand is MethodReference method ? method.FullName : "<non-method>")
            .OrderBy(value => value, StringComparer.Ordinal)
            .ToArray();
        if (!observedMethodOperands.SequenceEqual(expectedMethodOperands, StringComparer.Ordinal))
            hazards.Add("AccessTools::.cctor call/newobj surface changed: " + FormatNames(observedMethodOperands));

        var expectedStores = new[]
        {
            "System.Type[] HarmonyLib.AccessTools::allTypesCached",
            "System.Reflection.BindingFlags HarmonyLib.AccessTools::all",
            "System.Reflection.BindingFlags HarmonyLib.AccessTools::allDeclared",
            "System.Boolean HarmonyLib.AccessTools::<IsMonoRuntime>k__BackingField",
            "System.Boolean HarmonyLib.AccessTools::<IsNetFrameworkRuntime>k__BackingField",
            "System.Boolean HarmonyLib.AccessTools::<IsNetCoreRuntime>k__BackingField",
            "System.Collections.Generic.Dictionary`2<System.Type,HarmonyLib.FastInvokeHandler> HarmonyLib.AccessTools::addHandlerCache",
            "System.Threading.ReaderWriterLockSlim HarmonyLib.AccessTools::addHandlerCacheLock",
        }.OrderBy(value => value, StringComparer.Ordinal).ToArray();
        var observedStores = cctor.Body.Instructions
            .Where(instruction => instruction.OpCode.Code == Code.Stsfld)
            .Select(instruction => instruction.Operand is FieldReference field ? field.FullName : "<non-field>")
            .OrderBy(value => value, StringComparer.Ordinal)
            .ToArray();
        if (!observedStores.SequenceEqual(expectedStores, StringComparer.Ordinal))
            hazards.Add("AccessTools::.cctor static-field store surface changed: " + FormatNames(observedStores));

        var expectedAll = (int)(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static |
            BindingFlags.GetField | BindingFlags.SetField | BindingFlags.GetProperty | BindingFlags.SetProperty);
        var expectedAllDeclared = expectedAll | (int)BindingFlags.DeclaredOnly;
        if (allField is not null)
        {
            var store = cctor.Body.Instructions.SingleOrDefault(instruction => instruction.OpCode.Code == Code.Stsfld && ReferenceEquals(instruction.Operand, allField));
            if (store is null || store.Previous is null || !TryGetLdcI4Value(store.Previous, out var value) || value != expectedAll)
                hazards.Add($"AccessTools.all initializer value/shape changed; expected direct constant {expectedAll}.");
        }
        if (allDeclaredField is not null)
        {
            var store = cctor.Body.Instructions.SingleOrDefault(instruction => instruction.OpCode.Code == Code.Stsfld && ReferenceEquals(instruction.Operand, allDeclaredField));
            if (store is null || store.Previous?.OpCode.Code != Code.Or || store.Previous.Previous is null || !TryGetLdcI4Value(store.Previous.Previous, out var declaredOnly) || declaredOnly != (int)BindingFlags.DeclaredOnly ||
                store.Previous.Previous.Previous?.OpCode.Code != Code.Ldsfld || store.Previous.Previous.Previous.Operand is not FieldReference source || !source.Name.Equals("all", StringComparison.Ordinal))
                hazards.Add($"AccessTools.allDeclared initializer shape changed; expected all | DeclaredOnly ({expectedAllDeclared}).");
        }
        if (allTypesCached is not null)
        {
            var first = cctor.Body.Instructions.FirstOrDefault();
            if (first?.OpCode.Code != Code.Ldnull || first.Next?.OpCode.Code != Code.Stsfld || first.Next.Operand is not FieldReference field || !field.Name.Equals(allTypesCached.Name, StringComparison.Ordinal))
                hazards.Add("AccessTools.allTypesCached is no longer initialized to null at the beginning of the type initializer.");
        }
        if (cctor.Body.Instructions.LastOrDefault()?.OpCode.Code != Code.Ret)
            hazards.Add("AccessTools type initializer no longer ends in ret.");

        var detail =
            "Type: HarmonyLib.AccessTools — exact public static class\n" +
            $"Type initializer count: {typeInitializers.Length:N0}\n" +
            $"Type initializer instructions: {cctor.Body.Instructions.Count:N0} (expected 56)\n" +
            $"AccessTools.all expected BindingFlags value: {expectedAll}\n" +
            $"AccessTools.allDeclared expected BindingFlags value: {expectedAllDeclared}\n" +
            "Measured runtime probes: Mono.Runtime + RuntimeInformation.FrameworkDescription (.NET Framework / .NET Core legacy classification)\n" +
            "Measured cache initialization: allTypesCached=null + Dictionary<Type,FastInvokeHandler> + ReaderWriterLockSlim\n" +
            $"Blocking AccessTools initializer hazards: {hazards.Count:N0}" +
            (hazards.Count == 0 ? "\nExact Step 27.0.1 physical AccessTools initializer fingerprint: MATCH" : "\n" + string.Join("\n", hazards));
        return new AccessToolsMetadataSnapshot(hazards.Count == 0, detail, FormatMethodAudit(cctor));
    }

    private static bool TryGetLdcI4Value(Instruction instruction, out int value)
    {
        switch (instruction.OpCode.Code)
        {
            case Code.Ldc_I4_M1: value = -1; return true;
            case Code.Ldc_I4_0: value = 0; return true;
            case Code.Ldc_I4_1: value = 1; return true;
            case Code.Ldc_I4_2: value = 2; return true;
            case Code.Ldc_I4_3: value = 3; return true;
            case Code.Ldc_I4_4: value = 4; return true;
            case Code.Ldc_I4_5: value = 5; return true;
            case Code.Ldc_I4_6: value = 6; return true;
            case Code.Ldc_I4_7: value = 7; return true;
            case Code.Ldc_I4_8: value = 8; return true;
            case Code.Ldc_I4_S: value = instruction.Operand is sbyte signed ? signed : Convert.ToInt32(instruction.Operand); return true;
            case Code.Ldc_I4: value = Convert.ToInt32(instruction.Operand); return true;
            default: value = 0; return false;
        }
    }

    private static HarmonyPatchMetadataSnapshot ReadHarmonyPatchMetadata(string path)
    {
        using var resolver = new Step27MetadataOnlyResolver(path);
        using var module = ModuleDefinition.ReadModule(path, new ReaderParameters
        {
            InMemory = true,
            ReadSymbols = false,
            ReadingMode = ReadingMode.Deferred,
            AssemblyResolver = resolver,
            MetadataResolver = resolver,
        });
        if (module.Assembly?.Name is null)
            throw new BadImageFormatException("Managed assembly manifest missing while auditing Harmony patch APIs: " + path);

        var processorType = EnumerateTypes(module.Types).SingleOrDefault(type => type.FullName.Equals(PatchProcessorTypeFullName, StringComparison.Ordinal));
        var harmonyMethodType = EnumerateTypes(module.Types).SingleOrDefault(type => type.FullName.Equals("HarmonyLib.HarmonyMethod", StringComparison.Ordinal));
        if (processorType is null || harmonyMethodType is null)
            return new HarmonyPatchMetadataSnapshot(false, "PatchProcessor or HarmonyMethod type missing from exact 0Harmony.", "<missing>", "<missing>", "<missing>", "<missing>");
        if (!processorType.IsPublic || processorType.IsAbstract || processorType.IsInterface ||
            !harmonyMethodType.IsPublic || harmonyMethodType.IsAbstract || harmonyMethodType.IsInterface)
            return new HarmonyPatchMetadataSnapshot(false, "PatchProcessor/HarmonyMethod runtime type shape changed.", "<invalid type>", "<invalid type>", "<invalid type>", "<invalid type>");

        var harmonyMethodTypeInitializers = harmonyMethodType.Methods.Where(method => method.IsConstructor && method.IsStatic).ToArray();
        if (harmonyMethodTypeInitializers.Length != 0)
            return new HarmonyPatchMetadataSnapshot(false, $"Step 27 requires HarmonyMethod to have no type initializer; observed {harmonyMethodTypeInitializers.Length}.", "<blocked>", "<blocked>", "<blocked>", "<blocked>");

        var prefixField = processorType.Fields.SingleOrDefault(field => !field.IsStatic && field.Name.Equals("prefix", StringComparison.Ordinal));
        var harmonyMethodMethodField = harmonyMethodType.Fields.SingleOrDefault(field => !field.IsStatic && field.IsPublic && field.Name.Equals("method", StringComparison.Ordinal));
        if (prefixField is null || !prefixField.FieldType.FullName.Equals("HarmonyLib.HarmonyMethod", StringComparison.Ordinal) ||
            harmonyMethodMethodField is null || !harmonyMethodMethodField.FieldType.FullName.Equals("System.Reflection.MethodInfo", StringComparison.Ordinal))
            return new HarmonyPatchMetadataSnapshot(false, "PatchProcessor.prefix or HarmonyMethod.method field shape changed.", "<blocked>", "<blocked>", "<blocked>", "<blocked>");

        var harmonyMethodConstructors = harmonyMethodType.Methods.Where(method => method.IsConstructor && !method.IsStatic && method.IsPublic).ToArray();
        var harmonyMethodConstructor = harmonyMethodConstructors.SingleOrDefault(method =>
            method.Parameters.Count == 1 && method.Parameters[0].ParameterType.FullName.Equals("System.Reflection.MethodInfo", StringComparison.Ordinal));
        if (harmonyMethodConstructor is null || !harmonyMethodConstructor.HasBody)
            return new HarmonyPatchMetadataSnapshot(false, "Exact public HarmonyMethod(MethodInfo) constructor is missing or bodyless.", "<blocked>", "<blocked>", "<blocked>", harmonyMethodConstructor?.FullName ?? "<missing>");

        var addPrefixCandidates = processorType.Methods.Where(method => method.IsPublic && !method.IsStatic && method.Name.Equals("AddPrefix", StringComparison.Ordinal)).ToArray();
        var addPrefix = addPrefixCandidates.SingleOrDefault(method =>
            method.Parameters.Count == 1 &&
            method.Parameters[0].ParameterType.FullName.Equals("System.Reflection.MethodInfo", StringComparison.Ordinal) &&
            method.ReturnType.FullName.Equals(PatchProcessorTypeFullName, StringComparison.Ordinal));
        if (addPrefix is null || !addPrefix.HasBody)
            return new HarmonyPatchMetadataSnapshot(false, "Exact PatchProcessor.AddPrefix(MethodInfo) is missing or bodyless.", addPrefix?.FullName ?? "<missing>", "<blocked>", "<blocked>", FormatMethodAudit(harmonyMethodConstructor));

        var patchCandidates = processorType.Methods.Where(method => method.IsPublic && !method.IsStatic && method.Name.Equals("Patch", StringComparison.Ordinal)).ToArray();
        var patch = patchCandidates.SingleOrDefault(method => method.Parameters.Count == 0 && method.ReturnType.FullName.Equals("System.Reflection.MethodInfo", StringComparison.Ordinal));
        if (patch is null || !patch.HasBody)
            return new HarmonyPatchMetadataSnapshot(false, "Exact parameterless PatchProcessor.Patch() -> MethodInfo is missing or bodyless.", FormatMethodAudit(addPrefix), patch?.FullName ?? "<missing>", "<blocked>", FormatMethodAudit(harmonyMethodConstructor));

        var unpatchCandidates = processorType.Methods.Where(method => method.IsPublic && !method.IsStatic && method.Name.Equals("Unpatch", StringComparison.Ordinal)).ToArray();
        var unpatch = unpatchCandidates.SingleOrDefault(method =>
            method.Parameters.Count == 1 &&
            method.Parameters[0].ParameterType.FullName.Equals("System.Reflection.MethodInfo", StringComparison.Ordinal) &&
            method.ReturnType.FullName.Equals(PatchProcessorTypeFullName, StringComparison.Ordinal));
        if (unpatch is null || !unpatch.HasBody)
            return new HarmonyPatchMetadataSnapshot(false, "Exact PatchProcessor.Unpatch(MethodInfo) is missing or bodyless.", FormatMethodAudit(addPrefix), FormatMethodAudit(patch), unpatch?.FullName ?? "<missing>", FormatMethodAudit(harmonyMethodConstructor));

        var addPrefixIl = addPrefix.Body.Instructions;
        var addPrefixShape = addPrefixIl.Count == 6 &&
            addPrefixIl[0].OpCode.Code == Code.Ldarg_0 &&
            addPrefixIl[1].OpCode.Code == Code.Ldarg_1 &&
            addPrefixIl[2].OpCode.Code == Code.Newobj &&
            addPrefixIl[2].Operand is MethodReference addPrefixCtor &&
            addPrefixCtor.DeclaringType.FullName.Equals("HarmonyLib.HarmonyMethod", StringComparison.Ordinal) &&
            addPrefixCtor.Name.Equals(".ctor", StringComparison.Ordinal) &&
            addPrefixCtor.Parameters.Count == 1 && addPrefixCtor.Parameters[0].ParameterType.FullName.Equals("System.Reflection.MethodInfo", StringComparison.Ordinal) &&
            addPrefixIl[3].OpCode.Code == Code.Stfld && addPrefixIl[3].Operand is FieldReference prefixStore && prefixStore.Name.Equals("prefix", StringComparison.Ordinal) &&
            addPrefixIl[4].OpCode.Code == Code.Ldarg_0 && addPrefixIl[5].OpCode.Code == Code.Ret;

        static string[] CalledMembers(MethodDefinition method) => method.Body.Instructions
            .Where(instruction => instruction.Operand is MethodReference)
            .Select(instruction => ((MethodReference)instruction.Operand).FullName)
            .ToArray();

        var patchCalls = CalledMembers(patch);
        var unpatchCalls = CalledMembers(unpatch);
        var harmonyMethodCtorCalls = CalledMembers(harmonyMethodConstructor);
        var patchShape = patchCalls.Any(value => value.Contains("HarmonyLib.HarmonySharedState::GetPatchInfo", StringComparison.Ordinal)) &&
            patchCalls.Any(value => value.Contains("HarmonyLib.PatchInfo::AddPrefixes", StringComparison.Ordinal)) &&
            patchCalls.Any(value => value.Contains("HarmonyLib.PatchFunctions::UpdateWrapper", StringComparison.Ordinal)) &&
            patchCalls.Any(value => value.Contains("HarmonyLib.HarmonySharedState::UpdatePatchInfo", StringComparison.Ordinal));
        var unpatchShape = unpatchCalls.Any(value => value.Contains("HarmonyLib.HarmonySharedState::GetPatchInfo", StringComparison.Ordinal)) &&
            unpatchCalls.Any(value => value.Contains("HarmonyLib.PatchInfo::RemovePatch", StringComparison.Ordinal)) &&
            unpatchCalls.Any(value => value.Contains("HarmonyLib.PatchFunctions::UpdateWrapper", StringComparison.Ordinal)) &&
            unpatchCalls.Any(value => value.Contains("HarmonyLib.HarmonySharedState::UpdatePatchInfo", StringComparison.Ordinal));
        var harmonyMethodCtorShape = harmonyMethodCtorCalls.Any(value => value.Contains("HarmonyLib.HarmonyMethod::ImportMethod", StringComparison.Ordinal));

        var hazards = new List<string>();
        if (!addPrefixShape) hazards.Add("AddPrefix(MethodInfo) no longer matches new HarmonyMethod(fixMethod) -> prefix -> return this.");
        if (!patchShape) hazards.Add("Patch() no longer exposes the measured GetPatchInfo/AddPrefixes/UpdateWrapper/UpdatePatchInfo flow.");
        if (!unpatchShape) hazards.Add("Unpatch(MethodInfo) no longer exposes the measured GetPatchInfo/RemovePatch/UpdateWrapper/UpdatePatchInfo flow.");
        if (!harmonyMethodCtorShape) hazards.Add("HarmonyMethod(MethodInfo) no longer calls its measured ImportMethod path.");
        foreach (var method in new[] { addPrefix, patch, unpatch, harmonyMethodConstructor })
        {
            if (method.IsPInvokeImpl || method.PInvokeInfo is not null)
                hazards.Add("P/Invoke patch API body: " + method.FullName);
        }

        var detail =
            $"AddPrefix overloads: {addPrefixCandidates.Length:N0}\n" +
            "Exact prefix-registration API: AddPrefix(System.Reflection.MethodInfo) -> HarmonyLib.PatchProcessor\n" +
            $"Patch overloads: {patchCandidates.Length:N0}\n" +
            "Exact patch API: Patch() -> System.Reflection.MethodInfo\n" +
            $"Unpatch overloads: {unpatchCandidates.Length:N0}\n" +
            "Exact removal API: Unpatch(System.Reflection.MethodInfo) -> HarmonyLib.PatchProcessor\n" +
            "HarmonyMethod type initializer count: 0\n" +
            "HarmonyMethod retained field: method:System.Reflection.MethodInfo\n" +
            $"Blocking patch metadata hazards: {hazards.Count:N0}" +
            (hazards.Count == 0 ? string.Empty : "\n" + string.Join("\n", hazards));
        return new HarmonyPatchMetadataSnapshot(
            hazards.Count == 0,
            detail,
            FormatMethodAudit(addPrefix),
            FormatMethodAudit(patch),
            FormatMethodAudit(unpatch),
            FormatMethodAudit(harmonyMethodConstructor));
    }

    private static HarmonyProcessorMetadataSnapshot ReadHarmonyProcessorMetadata(string path)
    {
        using var resolver = new Step27MetadataOnlyResolver(path);
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
        using var resolver = new Step27MetadataOnlyResolver(path);
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
                $"Step 27 requires exactly one managed HarmonyLib.Harmony type initializer; observed {typeInitializers.Length}.",
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
                "Step 27 requires exactly one public HarmonyLib.Harmony constructor, .ctor(System.String), with managed IL.",
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
        using var resolver = new Step27MetadataOnlyResolver(path);
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
        => _initialization ?? throw new InvalidOperationException("Step 27 Gate C must pass before later Step 27 gates run.");

    private HarmonyApiSnapshot RequireHarmonyApi()
        => _harmonyApi ?? throw new InvalidOperationException("Step 27 Gate E must pass before Harmony type initialization runs.");

    private HarmonyTypeInitializationSnapshot RequireHarmonyTypeInitialization()
        => _harmonyTypeInitialization ?? throw new InvalidOperationException("Step 27 Gate F must pass before later Harmony gates run.");

    private HarmonyProcessorCreationSnapshot RequireHarmonyProcessorCreation()
        => _harmonyConstruction ?? throw new InvalidOperationException("Step 27 Gate H must pass before the Step 25 replay audit runs.");

    private HarmonyProcessorApiSnapshot RequireProcessorApi()
        => _processorApi ?? throw new InvalidOperationException("Step 27 Gate J must pass before PatchProcessor type initialization.");

    private PatchProcessorTypeInitializationSnapshot RequireProcessorTypeInitialization()
        => _processorTypeInitialization ?? throw new InvalidOperationException("Step 27 Gate K must pass before launcher probe resolution or processor construction.");

    private LauncherProbeSnapshot RequireLauncherProbe()
        => _launcherProbe ?? throw new InvalidOperationException("Step 27 Gate L must pass before PatchProcessor construction.");

    private ProcessorCreationSnapshot RequireProcessorCreation()
        => _processorCreation ?? throw new InvalidOperationException("Step 27 Gate M must pass before the final processor audit.");

    private HarmonyPatchApiSnapshot RequirePatchApi()
        => _patchApi ?? throw new InvalidOperationException("Step 27 Gate O must pass before launcher patch-probe resolution or any patch operation.");

    private LauncherPatchProbeSnapshot RequirePatchProbe()
        => _patchProbe ?? throw new InvalidOperationException("Step 27 Gate P must pass before launcher patch-probe invocation or registration.");

    private BaselineProbeInvocationSnapshot RequireBaselineProbeInvocation()
        => _baselineProbeInvocation ?? throw new InvalidOperationException("Step 27 Gate Q must pass before prefix registration.");

    private AccessToolsTypeInitializationSnapshot RequireAccessToolsTypeInitialization()
        => _accessToolsTypeInitialization ?? throw new InvalidOperationException("Step 27 Gate R must pass before HarmonyMethod construction/prefix registration.");

    private PrefixRegistrationSnapshot RequirePrefixRegistration()
        => _prefixRegistration ?? throw new InvalidOperationException("Step 27 Gate S must pass before PatchProcessor.Patch().");

    private PatchExecutionSnapshot RequirePatchExecution()
        => _patchExecution ?? throw new InvalidOperationException("Step 27 Gate T must pass before post-patch audit or patched invocation.");

    private ProbeInvocationSnapshot RequirePatchedProbeInvocation()
        => _patchedProbeInvocation ?? throw new InvalidOperationException("Step 27 Gate V must pass before exact prefix unpatch.");

    private UnpatchSnapshot RequireUnpatch()
        => _unpatch ?? throw new InvalidOperationException("Step 27 Gate W must pass before post-unpatch or restored-behavior gates.");

    private ProbeInvocationSnapshot RequireRestoredProbeInvocation()
        => _restoredProbeInvocation ?? throw new InvalidOperationException("Step 27 Gate Y must pass before the final isolation audit.");

    private Step27LoadContext RequireLoadContext()
        => _loadContext ?? throw new InvalidOperationException("Step 27 dedicated load context is unavailable.");

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(ControlledHarmonyPatchExecution));
    }

    private static ControlledHarmonyPatchExecutionGateResult Pass(ControlledHarmonyPatchExecutionGate gate, string detail)
        => new(gate, true, detail);

    private static ControlledHarmonyPatchExecutionGateResult Fail(ControlledHarmonyPatchExecutionGate gate, string stage, Exception ex)
        => new(gate, false, $"Stage: {stage}\n{ex}");

    private sealed class Step27MetadataOnlyResolver(string auditedPath) : IAssemblyResolver, IMetadataResolver
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

    private sealed record HarmonyPatchMetadataSnapshot(
        bool Allowed,
        string Detail,
        string AddPrefixAudit,
        string PatchAudit,
        string UnpatchAudit,
        string HarmonyMethodConstructorAudit);

    private sealed record AccessToolsMetadataSnapshot(
        bool Allowed,
        string Detail,
        string TypeInitializerAudit);

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

    private sealed record HarmonyPatchApiSnapshot(
        MethodInfo AddPrefixMethod,
        MethodInfo PatchMethod,
        MethodInfo UnpatchMethod,
        FieldInfo PrefixField,
        Type HarmonyMethodType,
        ConstructorInfo HarmonyMethodConstructor,
        FieldInfo HarmonyMethodMethodField,
        Type AccessToolsType,
        ConstructorInfo AccessToolsTypeInitializer,
        FieldInfo AccessToolsAllField,
        FieldInfo AccessToolsAllDeclaredField,
        FieldInfo AccessToolsAllTypesCachedField,
        FieldInfo AccessToolsIsMonoRuntimeField,
        FieldInfo AccessToolsIsNetFrameworkRuntimeField,
        FieldInfo AccessToolsIsNetCoreRuntimeField,
        FieldInfo AccessToolsAddHandlerCacheField,
        FieldInfo AccessToolsAddHandlerCacheLockField,
        string RuntimeFrameworkDescription,
        string AccessToolsTypeInitializerAudit,
        string AddPrefixAudit,
        string PatchAudit,
        string UnpatchAudit,
        string HarmonyMethodConstructorAudit);

    private sealed record LauncherPatchProbeSnapshot(
        MethodInfo Target,
        MethodInfo Prefix,
        string TargetSignature,
        string PrefixSignature);

    private sealed record BaselineProbeInvocationSnapshot(
        int DirectResult,
        int ReflectionResult,
        int TargetCalls,
        int PrefixCalls);

    private sealed record AccessToolsTypeInitializationSnapshot(
        string PreparedSha1,
        BindingFlags All,
        BindingFlags AllDeclared,
        bool IsMonoRuntime,
        bool IsNetFrameworkRuntime,
        bool IsNetCoreRuntime,
        string RuntimeFrameworkDescription,
        int ManagedResolverRequests,
        int PrivateLoads,
        int HostLoads,
        int NativeLoadAttempts);

    private sealed record PrefixRegistrationSnapshot(
        string PreparedSha1,
        int ManagedResolverRequests,
        int PrivateLoads,
        int HostLoads,
        int NativeLoadAttempts);

    private sealed record PatchExecutionSnapshot(
        string PreparedSha1,
        string ReplacementName,
        string ReplacementDeclaringType,
        int ManagedResolverRequests,
        int PrivateLoads,
        int HostLoads,
        int NativeLoadAttempts);

    private sealed record ProbeInvocationSnapshot(
        int ReflectionResult,
        int DirectResult,
        int TargetCalls,
        int PrefixCalls);

    private sealed record UnpatchSnapshot(
        string PreparedSha1,
        int ManagedResolverRequests,
        int PrivateLoads,
        int HostLoads,
        int NativeLoadAttempts,
        int TargetCallsAtUnpatch,
        int PrefixCallsAtUnpatch);

    private sealed class Step27LoadContext : AssemblyLoadContext
    {
        private readonly IReadOnlyDictionary<string, PreparedAssemblySnapshot> _privateBySimpleName;
        private readonly RuntimeBindingHostFramework[] _hostBindings;

        public Step27LoadContext(
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
