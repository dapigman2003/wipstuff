using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading;
using Mono.Cecil;
using Mono.Cecil.Cil;
using BinaryPrimitives = System.Buffers.Binary.BinaryPrimitives;
using PEReader = System.Reflection.PortableExecutable.PEReader;

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
    public const string HarmonySharedStateTypeFullName = "HarmonyLib.HarmonySharedState";
    public const int HarmonySharedStateInternalVersion = 102;
    public const string InterpretedPatchFixtureDirectoryName = "Step27InterpretedPatchFixture";
    public const string InterpretedPatchFixtureFileName = "StS2Launcher.Step27.InterpretedPatchFixture.dll";
    public const string InterpretedPatchFixtureAssemblySimpleName = "StS2Launcher.Step27.InterpretedPatchFixture";
    public const string InterpretedPatchFixtureTypeFullName = "StS2Launcher.Step27.InterpretedPatchFixture.InterpretedPatchProbe";

    // Exact runtime-generated assembly names reached by the tagged Harmony 2.4.2 shared-state /
    // MonoMod ILGenerator path. These are not prepared dependencies and are admitted only after
    // Gate T begins. Any other private-context addition remains fail-closed.
    private static readonly HashSet<string> AllowedPatchEngineGeneratedAssemblySimpleNames = new(StringComparer.Ordinal)
    {
        "HarmonySharedState",
        "MonoMod.Utils.Cil.ILGeneratorProxy",
    };

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
    private readonly string _interpretedPatchFixturePath;
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
    private HarmonySharedStateInitializationSnapshot? _harmonySharedStateInitialization;
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
            Path.Combine(AppContext.BaseDirectory, InterpretedPatchFixtureDirectoryName),
            collectibleLoadContext)
    {
    }

    public ControlledHarmonyPatchExecution(
        string launcherDataRoot,
        string interpretedPatchFixtureRoot,
        bool collectibleLoadContext = false)
        : this(
            launcherDataRoot,
            interpretedPatchFixtureRoot,
            collectibleLoadContext,
            FirstRealGameAssemblyLoad.ExpectedPrimarySimpleName,
            TargetSimpleName,
            TargetVersion,
            [FirstRealGameAssemblyLoad.ExpectedPrimarySimpleName, "SlayTheSpire2", TargetSimpleName, "HarmonySharedState", "MonoMod.Utils.Cil.ILGeneratorProxy", InterpretedPatchFixtureAssemblySimpleName])
    {
    }

    internal ControlledHarmonyPatchExecution(
        string launcherDataRoot,
        string interpretedPatchFixtureRoot,
        bool collectibleLoadContext,
        string expectedPrimarySimpleName,
        string targetSimpleName,
        Version targetVersion,
        IReadOnlyCollection<string> freshProcessAssemblyNames)
    {
        if (string.IsNullOrWhiteSpace(launcherDataRoot))
            throw new ArgumentException("Launcher data root is required.", nameof(launcherDataRoot));
        if (string.IsNullOrWhiteSpace(interpretedPatchFixtureRoot))
            throw new ArgumentException("Step 27 interpreted patch fixture root is required.", nameof(interpretedPatchFixtureRoot));
        if (string.IsNullOrWhiteSpace(expectedPrimarySimpleName))
            throw new ArgumentException("Expected primary simple name is required.", nameof(expectedPrimarySimpleName));
        if (string.IsNullOrWhiteSpace(targetSimpleName))
            throw new ArgumentException("Target simple name is required.", nameof(targetSimpleName));
        if (targetVersion is null)
            throw new ArgumentNullException(nameof(targetVersion));
        if (freshProcessAssemblyNames is null || freshProcessAssemblyNames.Count == 0)
            throw new ArgumentException("At least one fresh-process assembly identity is required.", nameof(freshProcessAssemblyNames));

        _launcherDataRoot = Path.GetFullPath(launcherDataRoot);
        _interpretedPatchFixturePath = Path.Combine(Path.GetFullPath(interpretedPatchFixtureRoot), InterpretedPatchFixtureFileName);
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

            stage = "HarmonySharedState iOS runtime-image normalization";
            var requiresIosHarmonyNormalization =
                _targetSimpleName.Equals(TargetSimpleName, StringComparison.OrdinalIgnoreCase) &&
                _targetVersion == TargetVersion;
            var harmonyRuntimeImage = requiresIosHarmonyNormalization
                ? CreateIosNormalizedHarmonyRuntimeImage(target.PreparedPath)
                : CreateSyntheticPassthroughRuntimeImage(target.PreparedPath);
            if (!harmonyRuntimeImage.SourcePreparedSha1.Equals(target.Plan.Sha1Hex, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Step 27 Gate A refuses the Harmony runtime-image selection because the source prepared SHA-1 no longer matches the persisted plan.");
            if (requiresIosHarmonyNormalization &&
                harmonyRuntimeImage.RuntimeImageSha1.Equals(harmonyRuntimeImage.SourcePreparedSha1, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException("Step 27 Gate A expected the bounded HarmonySharedState normalization to produce a byte-distinct runtime image.");
            }
            if (!requiresIosHarmonyNormalization &&
                !harmonyRuntimeImage.RuntimeImageSha1.Equals(harmonyRuntimeImage.SourcePreparedSha1, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException("Step 27 internal synthetic-target replay must remain byte-identical when iOS HarmonySharedState normalization is not applicable.");
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
                harmonyRuntimeImage,
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
                $"HarmonySharedState iOS runtime-image normalization: {(requiresIosHarmonyNormalization ? "PASS — canonical 0Harmony 2.4.2 target" : "NOT APPLICABLE — internal synthetic target replay")}\n" +
                $"Source prepared SHA-1 preserved: {harmonyRuntimeImage.SourcePreparedSha1}\n" +
                $"Runtime-image SHA-1: {harmonyRuntimeImage.RuntimeImageSha1}\n" +
                $"Runtime-image bytes: {harmonyRuntimeImage.RuntimeImageBytes.LongLength:N0}\n" +
                (requiresIosHarmonyNormalization
                    ? "Normalized HarmonySharedState::.cctor: direct launcher-private state/originals/originalsMono dictionaries + actualVersion=102 + null methodAddressRef\n" +
                      "Runtime dynamic HarmonySharedState singleton creation/ReflectionHelper.Load/StackFrame FieldRefAccess initialization: REMOVED FROM NORMALIZED CCTOR\n"
                    : "Internal synthetic target replay: original fixture bytes retained exactly; production normalization policy not bypassed.\n") +
                "Prepared/source/live file mutation: NO\n" +
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
                preflight.Target.Plan.AssemblyFullName,
                preflight.HarmonyRuntimeImage,
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
    public ControlledHarmonyPatchExecutionGateResult RunHarmonyPatchApiResolution(
        IProgress<ControlledHarmonyPatchExecutionProgress>? progress = null)
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
            ReportProgress(progress, ControlledHarmonyPatchExecutionGate.HarmonyPatchApiResolution, "O1 — reading exact PatchProcessor/HarmonyMethod Cecil metadata; no runtime member invocation.");
            var metadata = ReadHarmonyPatchMetadata(preflight.Target.PreparedPath);
            if (!metadata.Allowed)
                throw new InvalidDataException("Step 27 Gate O refuses patch admission because the exact patch metadata shape changed:\n" + metadata.Detail);
            ReportProgress(progress, ControlledHarmonyPatchExecutionGate.HarmonyPatchApiResolution, "O2 — reading exact AccessTools::.cctor Cecil fingerprint; no AccessTools runtime reflection or initialization.");
            var accessToolsMetadata = ReadAccessToolsMetadata(preflight.Target.PreparedPath);
            if (!accessToolsMetadata.Allowed)
                throw new InvalidDataException("Step 27 Gate O refuses AccessTools admission because its type initializer is not the exact physically measured runtime-detection/cache shape:\n" + accessToolsMetadata.Detail);
            ReportProgress(progress, ControlledHarmonyPatchExecutionGate.HarmonyPatchApiResolution, "O3 — auditing HarmonySharedState, replacement creation, and detour internals from exact receipt-backed 0Harmony; metadata only.");
            var patchEngineMetadata = ReadHarmonyPatchEngineMetadata(preflight.Target.PreparedPath);
            if (!patchEngineMetadata.Allowed)
                throw new InvalidDataException("Step 27 Gate O refuses patch-engine admission because the exact shared-state/replacement/detour metadata shape changed:\n" + patchEngineMetadata.Detail);

            stage = "AccessTools and patch-engine host-framework preservation preflight";
            ReportProgress(progress, ControlledHarmonyPatchExecutionGate.HarmonyPatchApiResolution, "O4 — resolving RuntimeInformation by the exact string used by Harmony; metadata admission only.");
            var runtimeInformationType = Type.GetType("System.Runtime.InteropServices.RuntimeInformation", throwOnError: false, ignoreCase: false)
                ?? throw new TypeLoadException("Step 27 AccessTools preservation preflight cannot resolve RuntimeInformation by the exact string used by Harmony.");
            if (runtimeInformationType != typeof(RuntimeInformation))
                throw new InvalidDataException("String-resolved RuntimeInformation does not bind to the host RuntimeInformation type.");
            ReportProgress(progress, ControlledHarmonyPatchExecutionGate.HarmonyPatchApiResolution, "O5 — resolving FrameworkDescription PropertyInfo without invoking its getter.");
            var frameworkDescriptionProperty = runtimeInformationType.GetProperty("FrameworkDescription", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                ?? throw new MissingMemberException("RuntimeInformation.FrameworkDescription was trimmed; Step 27 cannot safely initialize AccessTools.");
            if (frameworkDescriptionProperty.PropertyType != typeof(string) || frameworkDescriptionProperty.GetMethod is null)
                throw new InvalidDataException("RuntimeInformation.FrameworkDescription runtime shape changed.");
            ReportProgress(progress, ControlledHarmonyPatchExecutionGate.HarmonyPatchApiResolution, "O6 — resolving the exact physically proven AccessTools Dictionary<,>() and ReaderWriterLockSlim(LockRecursionPolicy) constructor metadata; constructors are not invoked.");
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
            ReportProgress(progress, ControlledHarmonyPatchExecutionGate.HarmonyPatchApiResolution, "O7 — resolving exact PatchProcessor AddPrefix/Patch/Unpatch runtime MethodInfo objects.");
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
            ReportProgress(progress, ControlledHarmonyPatchExecutionGate.HarmonyPatchApiResolution, "O8 — resolving exact HarmonyMethod runtime type/constructor/field; no HarmonyMethod is constructed.");
            var harmonyMethodType = initialization.TargetAssembly.GetType("HarmonyLib.HarmonyMethod", throwOnError: false, ignoreCase: false)
                ?? throw new TypeLoadException("Exact HarmonyLib.HarmonyMethod type is absent from loaded 0Harmony.");
            if (!ReferenceEquals(prefixField.FieldType, harmonyMethodType))
                throw new InvalidDataException("PatchProcessor.prefix runtime type no longer matches exact HarmonyLib.HarmonyMethod.");
            if (harmonyMethodType.TypeInitializer is not null)
                throw new InvalidDataException("Step 27 does not permit an implicit HarmonyMethod type initializer.");
            var harmonyMethodConstructors = harmonyMethodType.GetConstructors(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            var harmonyMethodDefaultConstructor = harmonyMethodConstructors.SingleOrDefault(candidate => candidate.GetParameters().Length == 0)
                ?? throw new MissingMethodException("HarmonyLib.HarmonyMethod", ".ctor()");
            var harmonyMethodConstructor = harmonyMethodConstructors.SingleOrDefault(candidate =>
            {
                var parameters = candidate.GetParameters();
                return parameters.Length == 1 && parameters[0].ParameterType == typeof(MethodInfo);
            }) ?? throw new MissingMethodException("HarmonyLib.HarmonyMethod", ".ctor(System.Reflection.MethodInfo)");
            var harmonyMethodMethodField = harmonyMethodType.GetField("method", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                ?? throw new MissingFieldException("HarmonyLib.HarmonyMethod", "method");
            var harmonyMethodPriorityField = harmonyMethodType.GetField("priority", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                ?? throw new MissingFieldException("HarmonyLib.HarmonyMethod", "priority");
            if (harmonyMethodMethodField.FieldType != typeof(MethodInfo) || harmonyMethodPriorityField.FieldType != typeof(int))
                throw new InvalidDataException("HarmonyMethod method/priority runtime field shape changed.");

            ReportProgress(progress, ControlledHarmonyPatchExecutionGate.HarmonyPatchApiResolution, "O9 — resolving exact AccessTools runtime Type/.cctor/fields without reading any static field.");
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

            ReportProgress(progress, ControlledHarmonyPatchExecutionGate.HarmonyPatchApiResolution, "O10 — exact physically proven patch API/AccessTools runtime reflection complete; verifying no private-context/native side effects before snapshot commit.");
            if (context.ManagedResolverRequests.Count != managedBefore || context.PrivateLoads.Count != privateBefore || context.HostLoads.Count != hostBefore)
                throw new InvalidDataException("Targeted patch API reflection unexpectedly changed resolver/load counters.");
            if (context.NativeLoadAttempts.Count != nativeBefore)
                throw new DllNotFoundException("Targeted patch API reflection attempted native resolution.");
            if (context.RejectedManagedRequests.Count != 0)
                throw new FileLoadException("Targeted patch API reflection triggered an unplanned managed request: " + string.Join(" | ", context.RejectedManagedRequests));
            var membershipAfter = context.Assemblies.Select(a => a.GetName().FullName ?? a.GetName().Name ?? string.Empty).OrderBy(v => v, StringComparer.Ordinal).ToArray();
            if (!membershipAfter.SequenceEqual(membershipBefore, StringComparer.Ordinal))
                throw new InvalidDataException("Targeted patch API reflection changed private-context membership.");

            ReportProgress(progress, ControlledHarmonyPatchExecutionGate.HarmonyPatchApiResolution, "O11 — runtime reflection/context checks passed; committing the bounded patch API + metadata-only patch-engine snapshot.");
            _patchApi = new HarmonyPatchApiSnapshot(
                addPrefix,
                patch,
                unpatch,
                prefixField,
                harmonyMethodType,
                harmonyMethodDefaultConstructor,
                harmonyMethodConstructor,
                harmonyMethodMethodField,
                harmonyMethodPriorityField,
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
                frameworkDescriptionProperty,
                accessToolsMetadata.TypeInitializerAudit,
                patchEngineMetadata.Detail,
                patchEngineMetadata.HarmonySharedStateTypeInitializerAudit,
                patchEngineMetadata.GetOrCreateSharedStateTypeAudit,
                patchEngineMetadata.MethodCreatorPrepareAudit,
                patchEngineMetadata.UpdateWrapperAudit,
                patchEngineMetadata.DetourMethodAudit,
                patchEngineMetadata.UpdatePatchInfoAudit,
                metadata.AddPrefixAudit,
                metadata.PatchAudit,
                metadata.UnpatchAudit,
                metadata.HarmonyMethodDefaultConstructorAudit,
                metadata.HarmonyMethodConstructorAudit);

            return Pass(
                ControlledHarmonyPatchExecutionGate.HarmonyPatchApiResolution,
                "TARGETED HARMONY PATCH API + PATCH-ENGINE METADATA RESOLUTION SUCCEEDED WITHOUT PATCH-DESCRIPTION CONSTRUCTION OR PATCHING.\n" +
                "Reference prefix API: PatchProcessor.AddPrefix(System.Reflection.MethodInfo) — exact IL audited, NOT invoked by Step 27.0.8\n" +
                "Patch execution method: PatchProcessor.Patch() -> System.Reflection.MethodInfo — remains the exact public acceptance boundary\n" +
                "Exact removal method: PatchProcessor.Unpatch(System.Reflection.MethodInfo)\n" +
                "Patch descriptor type: HarmonyLib.HarmonyMethod — no type initializer\n" +
                "Bounded iOS descriptor constructor: HarmonyMethod() — exact default priority=-1 shape\n" +
                "Reference constructor: HarmonyMethod(System.Reflection.MethodInfo) — exact ImportMethod path audited, NOT invoked by Step 27.0.8\n" +
                "Patch-engine closure: HarmonySharedState singleton -> MethodCreator replacement generation -> MonoMod detour -> UpdatePatchInfo — CECIL METADATA AUDITED ONLY in Gate O\n" +
                "Gate-O runtime reflection surface: restored to the physically passing 0.0.90 PatchProcessor/HarmonyMethod/AccessTools boundary\n" +
                "Bounded Reflection.Emit/MethodHandle runtime preservation preflight: DEFERRED TO GATE T so its loader effects are measured\n" +
                "HarmonySharedState runtime Type/.cctor/version reflection: DEFERRED TO GATE T so its loader effects are measured\n" +
                "AccessTools static-field values read: NO — Gate R owns AccessTools type initialization\n" +
                "HarmonyMethod object constructed: NO\n" +
                "PatchProcessor.Patch invoked: NO\n" +
                "Launcher patch probe invoked: NO\n" +
                "StS2 type/member reflected or invoked: NO\n" +
                "Patch-engine metadata summary:\n" + patchEngineMetadata.Detail + "\n" +
                "Audited HarmonySharedState::.cctor IL:\n" + patchEngineMetadata.HarmonySharedStateTypeInitializerAudit + "\n" +
                "Audited HarmonySharedState::GetOrCreateSharedStateType IL:\n" + patchEngineMetadata.GetOrCreateSharedStateTypeAudit + "\n" +
                "Audited MethodCreatorConfig::Prepare IL:\n" + patchEngineMetadata.MethodCreatorPrepareAudit + "\n" +
                "Audited PatchFunctions::UpdateWrapper IL:\n" + patchEngineMetadata.UpdateWrapperAudit + "\n" +
                "Audited PatchTools::DetourMethod IL:\n" + patchEngineMetadata.DetourMethodAudit + "\n" +
                "Audited HarmonySharedState::UpdatePatchInfo IL:\n" + patchEngineMetadata.UpdatePatchInfoAudit + "\n" +
                "Audited AddPrefix(MethodInfo) IL:\n" + metadata.AddPrefixAudit + "\n" +
                "Audited Patch() IL:\n" + metadata.PatchAudit + "\n" +
                "Audited Unpatch(MethodInfo) IL:\n" + metadata.UnpatchAudit + "\n" +
                "Audited HarmonyMethod() IL:\n" + metadata.HarmonyMethodDefaultConstructorAudit + "\n" +
                "Audited HarmonyMethod(MethodInfo) IL (reference path only):\n" + metadata.HarmonyMethodConstructorAudit + "\n" +
                "Audited AccessTools::.cctor IL:\n" + accessToolsMetadata.TypeInitializerAudit);
        }
        catch (Exception ex)
        {
            return Fail(ControlledHarmonyPatchExecutionGate.HarmonyPatchApiResolution, stage, ex);
        }
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Step 27.0.24 deliberately inspects and loads an exact-hash launcher-owned assembly that is copied into the app only after publish, so it is outside the build-time trimmer/AOT graph.")]
    [UnconditionalSuppressMessage("Trimming", "IL2075", Justification = "All reflected fixture members are exact-name/exact-signature checked from the same Cecil-audited post-publish image before use.")]
    public ControlledHarmonyPatchExecutionGateResult RunLauncherPatchProbeResolution()
    {
        var stage = "post-publish interpreted patch fixture admission";
        try
        {
            ThrowIfDisposed();
            _ = RequirePatchApi();
            var processorApi = RequireProcessorApi();
            var harmonyInstance = _harmonyInstance ?? throw new InvalidOperationException("Step 27 retained Harmony instance is missing before interpreted-target processor creation.");
            var context = RequireLoadContext();
            if (!File.Exists(_interpretedPatchFixturePath))
                throw new FileNotFoundException("Step 27.0.24 post-publish interpreted patch fixture is missing.", _interpretedPatchFixturePath);

            var fixtureBytes = File.ReadAllBytes(_interpretedPatchFixturePath);
            if (fixtureBytes.Length == 0)
                throw new InvalidDataException("Step 27.0.24 post-publish interpreted patch fixture is empty.");
            var fixtureSha256 = Convert.ToHexString(SHA256.HashData(fixtureBytes)).ToLowerInvariant();

            using var resolver = new Step27MetadataOnlyResolver(_interpretedPatchFixturePath);
            using var module = ModuleDefinition.ReadModule(new MemoryStream(fixtureBytes, writable: false), new ReaderParameters
            {
                InMemory = true,
                ReadSymbols = false,
                ReadingMode = ReadingMode.Deferred,
                AssemblyResolver = resolver,
                MetadataResolver = resolver,
            });
            if (module.Assembly?.Name is null)
                throw new BadImageFormatException("Step 27.0.24 interpreted patch fixture has no managed assembly manifest.");
            if (!module.Assembly.Name.Name.Equals(InterpretedPatchFixtureAssemblySimpleName, StringComparison.Ordinal) ||
                module.Assembly.Name.Version != new Version(1, 0, 0, 0))
            {
                throw new InvalidDataException("Step 27.0.24 interpreted patch fixture identity changed: " + module.Assembly.Name.FullName);
            }
            if (module.Attributes.HasFlag(Mono.Cecil.ModuleAttributes.ILOnly) == false)
                throw new InvalidDataException("Step 27.0.24 interpreted patch fixture must be IL-only.");
            if (module.Assembly.Name.HasPublicKey)
                throw new InvalidDataException("Step 27.0.24 interpreted patch fixture must remain launcher-owned and unsigned.");

            foreach (var reference in module.AssemblyReferences)
            {
                var simple = reference.Name;
                if (simple.Equals("mscorlib", StringComparison.Ordinal) ||
                    simple.Equals("netstandard", StringComparison.Ordinal) ||
                    simple.Equals("System", StringComparison.Ordinal) ||
                    simple.StartsWith("System.", StringComparison.Ordinal) ||
                    simple.Equals("Microsoft.CSharp", StringComparison.Ordinal) ||
                    simple.StartsWith("Microsoft.VisualBasic", StringComparison.Ordinal))
                {
                    continue;
                }
                throw new InvalidDataException("Step 27.0.24 interpreted patch fixture has an unexpected non-framework dependency: " + reference.FullName);
            }

            var moduleType = module.Types.SingleOrDefault(type => type.Name.Equals("<Module>", StringComparison.Ordinal));
            if (moduleType?.Methods.Any(method => method.Name.Equals(".cctor", StringComparison.Ordinal)) == true)
                throw new InvalidDataException("Step 27.0.24 interpreted patch fixture must not contain a module initializer.");

            var probeTypeDefinition = EnumerateTypes(module.Types).SingleOrDefault(type => type.FullName.Equals(InterpretedPatchFixtureTypeFullName, StringComparison.Ordinal))
                ?? throw new MissingMemberException(InterpretedPatchFixtureTypeFullName);
            if (!probeTypeDefinition.IsPublic || !probeTypeDefinition.IsAbstract || !probeTypeDefinition.IsSealed)
                throw new InvalidDataException("Step 27.0.24 interpreted patch fixture probe is no longer the expected public static type.");

            MethodDefinition ExactMethod(string name, string returnType, params string[] parameterTypes)
            {
                var candidates = probeTypeDefinition.Methods.Where(method =>
                    method.IsPublic && method.IsStatic && !method.HasGenericParameters &&
                    method.Name.Equals(name, StringComparison.Ordinal) &&
                    method.ReturnType.FullName.Equals(returnType, StringComparison.Ordinal) &&
                    method.Parameters.Select(parameter => parameter.ParameterType.FullName).SequenceEqual(parameterTypes, StringComparer.Ordinal)).ToArray();
                if (candidates.Length != 1)
                    throw new MissingMethodException(InterpretedPatchFixtureTypeFullName, name);
                if (!candidates[0].HasBody)
                    throw new InvalidDataException($"Step 27.0.24 interpreted patch fixture method {name} has no managed IL body.");
                return candidates[0];
            }

            _ = ExactMethod("ResetCounters", "System.Void");
            _ = ExactMethod("Target", "System.Int32", "System.Int32");
            _ = ExactMethod("InvokeTarget", "System.Int32", "System.Int32");
            var prefixDefinition = ExactMethod("Prefix", "System.Boolean", "System.Int32", "System.Int32&");
            if (!prefixDefinition.Parameters[0].Name.Equals("value", StringComparison.Ordinal) ||
                !prefixDefinition.Parameters[1].Name.Equals("__result", StringComparison.Ordinal))
                throw new InvalidDataException("Step 27.0.24 interpreted prefix parameter names changed from value + __result.");

            var exactInt32Fields = probeTypeDefinition.Fields.Where(field =>
                field.IsPublic && field.IsStatic && field.FieldType.FullName.Equals("System.Int32", StringComparison.Ordinal)).ToArray();
            if (exactInt32Fields.Count(field => field.Name.Equals("TargetCalls", StringComparison.Ordinal)) != 1 ||
                exactInt32Fields.Count(field => field.Name.Equals("PrefixCalls", StringComparison.Ordinal)) != 1)
                throw new InvalidDataException("Step 27.0.24 interpreted fixture counter fields changed.");

            var managedBefore = context.ManagedResolverRequests.Count;
            var privateBefore = context.PrivateLoads.Count;
            var hostBefore = context.HostLoads.Count;
            var nativeBefore = context.NativeLoadAttempts.Count;
            var rejectedBefore = context.RejectedManagedRequests.Count;
            var membershipBefore = SnapshotPrivateContextMembership(context);

            stage = "exact-hash post-publish interpreted fixture LoadFromStream";
            var fixtureAssembly = context.LoadVerifiedInterpretedFixture(
                fixtureBytes,
                module.Assembly.Name.FullName,
                "Step 27.0.24 post-publish interpreted patch fixture");
            if (!ReferenceEquals(AssemblyLoadContext.GetLoadContext(fixtureAssembly), context))
                throw new InvalidDataException("Step 27.0.24 interpreted patch fixture did not load into the Step 27 private context.");
            if (context.PrivateLoads.Count != privateBefore + 1)
                throw new InvalidDataException("Step 27.0.24 interpreted fixture load was not recorded as exactly one private load.");
            if (context.NativeLoadAttempts.Count != nativeBefore || context.RejectedManagedRequests.Count != rejectedBefore)
                throw new InvalidDataException("Step 27.0.24 interpreted fixture admission caused native or rejected managed resolution.");

            var membershipAfter = SnapshotPrivateContextMembership(context);
            var added = membershipAfter.Except(membershipBefore, StringComparer.Ordinal).ToArray();
            if (added.Length != 1 || !string.Equals(new AssemblyName(added[0]).Name, InterpretedPatchFixtureAssemblySimpleName, StringComparison.Ordinal))
                throw new InvalidDataException("Step 27.0.24 fixture admission changed private-context membership by anything other than the exact interpreted fixture.");

            stage = "interpreted fixture MethodInfo/FieldInfo resolution";
            var probeType = fixtureAssembly.GetType(InterpretedPatchFixtureTypeFullName, throwOnError: true, ignoreCase: false)
                ?? throw new MissingMemberException(InterpretedPatchFixtureTypeFullName);
            var target = probeType.GetMethod("Target", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, binder: null, [typeof(int)], modifiers: null)
                ?? throw new MissingMethodException(InterpretedPatchFixtureTypeFullName, "Target");
            var invokeTarget = probeType.GetMethod("InvokeTarget", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, binder: null, [typeof(int)], modifiers: null)
                ?? throw new MissingMethodException(InterpretedPatchFixtureTypeFullName, "InvokeTarget");
            var prefix = probeType.GetMethod("Prefix", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                ?? throw new MissingMethodException(InterpretedPatchFixtureTypeFullName, "Prefix");
            var resetCounters = probeType.GetMethod("ResetCounters", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, binder: null, Type.EmptyTypes, modifiers: null)
                ?? throw new MissingMethodException(InterpretedPatchFixtureTypeFullName, "ResetCounters");
            var targetCallsField = probeType.GetField("TargetCalls", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                ?? throw new MissingFieldException(InterpretedPatchFixtureTypeFullName, "TargetCalls");
            var prefixCallsField = probeType.GetField("PrefixCalls", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                ?? throw new MissingFieldException(InterpretedPatchFixtureTypeFullName, "PrefixCalls");

            if (!ReferenceEquals(target.DeclaringType?.Assembly, fixtureAssembly) ||
                !ReferenceEquals(invokeTarget.DeclaringType?.Assembly, fixtureAssembly) ||
                !ReferenceEquals(prefix.DeclaringType?.Assembly, fixtureAssembly))
                throw new InvalidDataException("Step 27.0.24 interpreted fixture member resolution escaped the exact loaded fixture.");
            if (target.ReturnType != typeof(int) || invokeTarget.ReturnType != typeof(int) || prefix.ReturnType != typeof(bool) ||
                targetCallsField.FieldType != typeof(int) || prefixCallsField.FieldType != typeof(int))
                throw new InvalidDataException("Step 27.0.24 interpreted fixture runtime member shapes changed.");
            var prefixParameters = prefix.GetParameters();
            if (prefixParameters.Length != 2 ||
                prefixParameters[0].ParameterType != typeof(int) || !string.Equals(prefixParameters[0].Name, "value", StringComparison.Ordinal) ||
                prefixParameters[1].ParameterType != typeof(int).MakeByRefType() || !string.Equals(prefixParameters[1].Name, "__result", StringComparison.Ordinal))
                throw new InvalidDataException("Step 27.0.24 interpreted fixture prefix runtime signature changed.");

            stage = "fresh interpreted-target PatchProcessor creation";
            object interpretedProcessor;
            try
            {
                interpretedProcessor = processorApi.CreateProcessorMethod.Invoke(harmonyInstance, [target])
                    ?? throw new InvalidDataException("Harmony.CreateProcessor(interpreted Target) returned null.");
            }
            catch (TargetInvocationException ex) when (ex.InnerException is not null)
            {
                throw new InvalidOperationException("Harmony.CreateProcessor(interpreted Target) threw.", ex.InnerException);
            }
            if (!ReferenceEquals(interpretedProcessor.GetType(), processorApi.PatchProcessorType) ||
                !ReferenceEquals(processorApi.InstanceField.GetValue(interpretedProcessor), harmonyInstance) ||
                !ReferenceEquals(processorApi.OriginalField.GetValue(interpretedProcessor), target))
                throw new InvalidDataException("Fresh interpreted-target PatchProcessor did not retain the exact Harmony instance and interpreted target MethodBase.");
            _patchProcessorInstance = interpretedProcessor;
            if (context.NativeLoadAttempts.Count != nativeBefore || context.RejectedManagedRequests.Count != rejectedBefore)
                throw new InvalidDataException("Interpreted-target PatchProcessor creation caused native or rejected managed resolution.");
            var postProcessorMembership = SnapshotPrivateContextMembership(context);
            if (!postProcessorMembership.SequenceEqual(membershipAfter, StringComparer.Ordinal))
                throw new InvalidDataException("Interpreted-target PatchProcessor creation changed private-context membership.");
            if (targetCallsField.GetValue(null) is not int initialTargetCalls || initialTargetCalls != 0 ||
                prefixCallsField.GetValue(null) is not int initialPrefixCalls || initialPrefixCalls != 0)
                throw new InvalidDataException("Interpreted patch fixture counters changed during admission/processor creation.");

            var targetSignature = $"{target.ReturnType.FullName} {target.DeclaringType!.FullName}::{target.Name}(System.Int32)";
            var invokeTargetSignature = $"{invokeTarget.ReturnType.FullName} {invokeTarget.DeclaringType!.FullName}::{invokeTarget.Name}(System.Int32)";
            var prefixSignature = $"{prefix.ReturnType.FullName} {prefix.DeclaringType!.FullName}::{prefix.Name}(System.Int32,System.Int32&)";
            _patchProbe = new LauncherPatchProbeSnapshot(
                fixtureAssembly,
                _interpretedPatchFixturePath,
                fixtureSha256,
                target,
                invokeTarget,
                prefix,
                resetCounters,
                targetCallsField,
                prefixCallsField,
                targetSignature,
                invokeTargetSignature,
                prefixSignature);

            return Pass(
                ControlledHarmonyPatchExecutionGate.LauncherPatchProbeResolution,
                "LAUNCHER-OWNED POST-PUBLISH INTERPRETED PATCH FIXTURE ADMITTED + RESOLVED WITHOUT INVOCATION.\n" +
                $"Fixture: {InterpretedPatchFixtureFileName}\n" +
                $"Fixture SHA-256: {fixtureSha256}\n" +
                $"Fixture bytes: {fixtureBytes.Length:N0}\n" +
                $"Assembly: {fixtureAssembly.GetName().FullName}\n" +
                "Load timing: copied into .app only AFTER dotnet publish; not an iOS project/content reference or AOT input\n" +
                $"Load context: {AssemblyLoadContext.GetLoadContext(fixtureAssembly)?.Name}\n" +
                $"Target: {targetSignature}\n" +
                $"In-fixture direct caller: {invokeTargetSignature}\n" +
                $"Prefix: {prefixSignature}\n" +
                "Fresh PatchProcessor target: exact interpreted Target MethodInfo via audited Harmony.CreateProcessor(MethodBase)\n" +
                "Prefix parameter names: value + __result — EXACT\n" +
                $"Resolver/load deltas during fixture admission: managed={context.ManagedResolverRequests.Count - managedBefore}; private={context.PrivateLoads.Count - privateBefore}; host={context.HostLoads.Count - hostBefore}; native=0\n" +
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

    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Step 27.0.24 invokes only exact members of the launcher-owned post-publish interpreted fixture admitted in Gate P.")]
    public ControlledHarmonyPatchExecutionGateResult RunBaselineProbeInvocation()
    {
        var stage = "post-publish interpreted fixture baseline invocation";
        try
        {
            ThrowIfDisposed();
            var probe = RequirePatchProbe();
            var context = RequireLoadContext();
            var managedBefore = context.ManagedResolverRequests.Count;
            var privateBefore = context.PrivateLoads.Count;
            var hostBefore = context.HostLoads.Count;
            var nativeBefore = context.NativeLoadAttempts.Count;
            var membershipBefore = SnapshotPrivateContextMembership(context);

            ResetPatchProbeCounters(probe);
            stage = "reflection baseline target invocation";
            var reflectedResult = InvokePatchProbeInt32(probe.Target, 41, "Step 27.0.24 baseline Target reflection invocation");
            stage = "interpreted in-fixture direct-call baseline invocation";
            var directResult = InvokePatchProbeInt32(probe.InvokeTarget, 41, "Step 27.0.24 baseline InvokeTarget invocation");

            var counters = ReadPatchProbeCounters(probe);
            if (directResult != 42 || reflectedResult != 42 || counters.TargetCalls != 2 || counters.PrefixCalls != 0)
                throw new InvalidDataException($"Interpreted fixture baseline behavior changed: direct={directResult}, reflection={reflectedResult}, targetCalls={counters.TargetCalls}, prefixCalls={counters.PrefixCalls}.");
            if (context.ManagedResolverRequests.Count != managedBefore || context.PrivateLoads.Count != privateBefore || context.HostLoads.Count != hostBefore || context.NativeLoadAttempts.Count != nativeBefore)
                throw new InvalidDataException("Interpreted fixture baseline invocation unexpectedly changed the Step 27 private resolver/load counters.");
            var membershipAfter = SnapshotPrivateContextMembership(context);
            if (!membershipAfter.SequenceEqual(membershipBefore, StringComparer.Ordinal))
                throw new InvalidDataException("Interpreted fixture baseline invocation changed private-context membership.");

            _baselineProbeInvocation = new BaselineProbeInvocationSnapshot(directResult, reflectedResult, counters.TargetCalls, counters.PrefixCalls);
            return Pass(
                ControlledHarmonyPatchExecutionGate.BaselineProbeInvocation,
                "POST-PUBLISH INTERPRETED FIXTURE BASELINE BEHAVIOR ESTABLISHED BEFORE PATCHING.\n" +
                "Input: 41\n" +
                $"Target reflection result: {reflectedResult}\n" +
                $"In-fixture direct-call result: {directResult}\n" +
                $"Target calls: {counters.TargetCalls}\n" +
                $"Prefix calls: {counters.PrefixCalls}\n" +
                "Expected original behavior value + 1: YES\n" +
                "Both invocation routes executed managed IL from the post-publish fixture: YES\n" +
                "PatchProcessor.Patch invoked: NO\n" +
                "StS2 type/member reflected or invoked: NO");
        }
        catch (Exception ex)
        {
            return Fail(ControlledHarmonyPatchExecutionGate.BaselineProbeInvocation, stage, ex);
        }
    }

    [UnconditionalSuppressMessage("Trimming", "IL2075", Justification = "AccessTools is post-publish Harmony code unavailable to the build-time trimmer; Gate O bounds its exact physical runtime-detection/cache initializer and verifies the string-reflected framework surface before this explicit completion barrier.")]
    public ControlledHarmonyPatchExecutionGateResult RunAccessToolsTypeInitialization(
        IProgress<ControlledHarmonyPatchExecutionProgress>? progress = null)
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

            stage = "host reflective RuntimeInformation.FrameworkDescription invocation";
            ReportProgress(progress, ControlledHarmonyPatchExecutionGate.AccessToolsTypeInitialization, "R1 — invoking the preserved RuntimeInformation.FrameworkDescription getter through PropertyInfo.GetValue before AccessTools::.cctor.");
            var frameworkDescription = patchApi.RuntimeFrameworkDescriptionProperty.GetValue(null) as string;
            if (string.IsNullOrWhiteSpace(frameworkDescription))
                throw new InvalidDataException("RuntimeInformation.FrameworkDescription returned an empty value through the exact reflected getter path required by AccessTools.");

            stage = "explicit HarmonyLib.AccessTools type-initialization completion barrier";
            ReportProgress(progress, ControlledHarmonyPatchExecutionGate.AccessToolsTypeInitialization, "R2 — entering RuntimeHelpers.RunClassConstructor(HarmonyLib.AccessTools.TypeHandle).");
            RuntimeHelpers.RunClassConstructor(patchApi.AccessToolsType.TypeHandle);
            ReportProgress(progress, ControlledHarmonyPatchExecutionGate.AccessToolsTypeInitialization, "R3 — AccessTools::.cctor returned; verifying exact static state.");

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
            if (frameworkDescription.StartsWith(".NET Framework", StringComparison.Ordinal))
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
            RequirePatchProbeCounters(2, 0, "AccessTools type initialization unexpectedly invoked the interpreted fixture target or prefix.");

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

    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Step 27 constructs only the exact metadata-verified parameterless HarmonyMethod descriptor and writes its exact public method field for a launcher prefix with no Harmony annotations.")]
    [UnconditionalSuppressMessage("Trimming", "IL2075", Justification = "Step 27 accesses only exact post-publish HarmonyMethod/PatchProcessor fields and constructors resolved by Gate O.")]
    public ControlledHarmonyPatchExecutionGateResult RunPrefixRegistration(
        IProgress<ControlledHarmonyPatchExecutionProgress>? progress = null)
    {
        var stage = "bounded iOS-safe prefix descriptor registration";
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

            // HarmonyMethod(MethodInfo) imports Harmony annotations through AccessTools. The launcher probe intentionally
            // carries no Harmony annotations, so the exact AddPrefix(MethodInfo) result is equivalent to a default
            // HarmonyMethod descriptor whose method field is the exact prefix. This bounded path avoids the iOS hard
            // crash physically localized inside AddPrefix's HarmonyMethod(MethodInfo) construction/import path.
            var harmonyAnnotations = probe.Prefix.GetCustomAttributesData()
                .Where(attribute =>
                    string.Equals(attribute.AttributeType.Namespace, "HarmonyLib", StringComparison.Ordinal) ||
                    string.Equals(attribute.AttributeType.Assembly.GetName().Name, TargetSimpleName, StringComparison.OrdinalIgnoreCase))
                .ToArray();
            if (harmonyAnnotations.Length != 0)
                throw new InvalidDataException("Step 27 bounded descriptor substitution requires the launcher prefix to carry zero Harmony annotations; observed: " + string.Join(", ", harmonyAnnotations.Select(attribute => attribute.AttributeType.FullName)));

            var managedBefore = context.ManagedResolverRequests.Count;
            var privateBefore = context.PrivateLoads.Count;
            var hostBefore = context.HostLoads.Count;
            var nativeBefore = context.NativeLoadAttempts.Count;
            var membershipBefore = context.Assemblies.Select(a => a.GetName().FullName ?? a.GetName().Name ?? string.Empty).OrderBy(v => v, StringComparer.Ordinal).ToArray();

            object descriptor;
            try
            {
                stage = "exact parameterless HarmonyMethod construction";
                ReportProgress(progress, ControlledHarmonyPatchExecutionGate.PrefixRegistration, "S1 — entering exact HarmonyMethod() reflection construction; AddPrefix(MethodInfo) and ImportMethod are NOT invoked.");
                descriptor = patchApi.HarmonyMethodDefaultConstructor.Invoke([])
                    ?? throw new InvalidDataException("Exact HarmonyMethod() constructor returned null.");
                ReportProgress(progress, ControlledHarmonyPatchExecutionGate.PrefixRegistration, "S2 — HarmonyMethod() returned; verifying exact default descriptor state before any field assignment.");
            }
            catch (TargetInvocationException ex) when (ex.InnerException is not null)
            {
                throw new InvalidOperationException("Exact HarmonyMethod() constructor threw.", ex.InnerException);
            }

            if (!ReferenceEquals(descriptor.GetType(), patchApi.HarmonyMethodType))
                throw new InvalidDataException("HarmonyMethod() returned an unexpected runtime type.");
            if (patchApi.HarmonyMethodMethodField.GetValue(descriptor) is not null)
                throw new InvalidDataException("HarmonyMethod() default descriptor unexpectedly has a non-null method field.");
            if (patchApi.HarmonyMethodPriorityField.GetValue(descriptor) is not int priority || priority != -1)
                throw new InvalidDataException($"HarmonyMethod() default priority changed; expected -1, observed {patchApi.HarmonyMethodPriorityField.GetValue(descriptor) ?? "<null>"}.");

            stage = "exact HarmonyMethod.method assignment";
            ReportProgress(progress, ControlledHarmonyPatchExecutionGate.PrefixRegistration, "S3 — assigning only HarmonyMethod.method = exact launcher Prefix MethodInfo; no annotation import.");
            patchApi.HarmonyMethodMethodField.SetValue(descriptor, probe.Prefix);
            if (!ReferenceEquals(patchApi.HarmonyMethodMethodField.GetValue(descriptor), probe.Prefix))
                throw new InvalidDataException("HarmonyMethod.method did not retain the exact launcher-owned prefix MethodInfo.");

            stage = "exact PatchProcessor.prefix assignment";
            ReportProgress(progress, ControlledHarmonyPatchExecutionGate.PrefixRegistration, "S4 — assigning only PatchProcessor.prefix = bounded HarmonyMethod descriptor; Patch() remains uninvoked.");
            patchApi.PrefixField.SetValue(processor, descriptor);
            if (!ReferenceEquals(patchApi.PrefixField.GetValue(processor), descriptor))
                throw new InvalidDataException("PatchProcessor.prefix did not retain the bounded HarmonyMethod descriptor.");
            ReportProgress(progress, ControlledHarmonyPatchExecutionGate.PrefixRegistration, "S5 — bounded descriptor registration complete; verifying isolation before first Patch() boundary.");

            if (context.NativeLoadAttempts.Count != nativeBefore)
                throw new DllNotFoundException("Prefix descriptor registration attempted native resolution: " + string.Join(" | ", context.NativeLoadAttempts.Skip(nativeBefore)));
            if (context.RejectedManagedRequests.Count != 0)
                throw new FileLoadException("Prefix descriptor registration triggered an unplanned managed request: " + string.Join(" | ", context.RejectedManagedRequests));
            var membershipAfter = context.Assemblies.Select(a => a.GetName().FullName ?? a.GetName().Name ?? string.Empty).OrderBy(v => v, StringComparer.Ordinal).ToArray();
            if (!membershipAfter.SequenceEqual(membershipBefore, StringComparer.Ordinal))
                throw new InvalidDataException("Prefix descriptor registration changed private-context membership.");
            var postSha1 = ComputeSha1Hex(preflight.Target.PreparedPath);
            if (!postSha1.Equals(targetSha1, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("0Harmony prepared bytes changed across prefix descriptor registration.");
            RequirePatchProbeCounters(2, 0, "Prefix descriptor registration unexpectedly invoked the launcher probe or prefix.");

            _prefixDescriptor = descriptor;
            _prefixRegistration = new PrefixRegistrationSnapshot(
                postSha1,
                context.ManagedResolverRequests.Count - managedBefore,
                context.PrivateLoads.Count - privateBefore,
                context.HostLoads.Count - hostBefore,
                context.NativeLoadAttempts.Count - nativeBefore);

            return Pass(
                ControlledHarmonyPatchExecutionGate.PrefixRegistration,
                "CONTROLLED BOUNDED HARMONY PREFIX DESCRIPTOR REGISTRATION SUCCEEDED WITHOUT PATCHING.\n" +
                "Reference API: PatchProcessor.AddPrefix(MethodInfo) — exact six-instruction IL remains metadata-audited but was NOT invoked because physical 0.0.89 localized a hard crash inside its HarmonyMethod(MethodInfo)/ImportMethod path.\n" +
                "Compatibility path: exact HarmonyMethod() -> verify priority=-1/method=null -> method=launcher Prefix -> PatchProcessor.prefix=descriptor\n" +
                "Launcher prefix Harmony annotations: 0 — required for equivalence\n" +
                $"Prefix: {probe.PrefixSignature}\n" +
                "HarmonyMethod.method retained exact prefix MethodInfo: YES\n" +
                "HarmonyLib.AccessTools type initializer completed explicitly in prior Gate R: YES\n" +
                $"Managed resolver requests during registration: {_prefixRegistration.ManagedResolverRequests:N0}\n" +
                $"Private loads during registration: {_prefixRegistration.PrivateLoads:N0}\n" +
                $"Host loads during registration: {_prefixRegistration.HostLoads:N0}\n" +
                $"Native load attempts during registration: {_prefixRegistration.NativeLoadAttempts:N0}\n" +
                "PatchProcessor.AddPrefix invoked: NO\n" +
                "HarmonyMethod(MethodInfo)/ImportMethod invoked: NO\n" +
                "PatchProcessor.Patch invoked: NO\n" +
                "Launcher target/prefix invoked: NO\n" +
                "StS2 type/member reflected or invoked: NO");
        }
        catch (Exception ex)
        {
            return Fail(ControlledHarmonyPatchExecutionGate.PrefixRegistration, stage, ex);
        }
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Step 27 resolves and initializes only the exact metadata-audited HarmonySharedState type, then invokes exactly PatchProcessor.Patch() from the verified post-publish API surface against a launcher-owned probe.")]
    [UnconditionalSuppressMessage("Trimming", "IL2075", Justification = "Step 27 resolves only the exact HarmonySharedState runtime members admitted by Gate O metadata, then reads only its exact version field and the replacement MethodInfo.")]
    public ControlledHarmonyPatchExecutionGateResult RunPatchEngineExecution(
        IProgress<ControlledHarmonyPatchExecutionProgress>? progress = null)
    {
        var stage = "measured patch-engine runtime resolution before exact PatchProcessor.Patch() invocation";
        try
        {
            ThrowIfDisposed();
            var preflight = RequirePreflight();
            var initialization = RequireInitialization();
            var patchApi = RequirePatchApi();
            var probe = RequirePatchProbe();
            _ = RequirePrefixRegistration();
            var context = RequireLoadContext();
            var processor = _patchProcessorInstance ?? throw new InvalidOperationException("Step 27 retained PatchProcessor instance is missing.");

            VerifyFileLength(preflight.Target.PreparedPath, preflight.Target.Plan.Length, "prepared patch-execution target");
            var targetSha1 = ComputeSha1Hex(preflight.Target.PreparedPath);
            if (!targetSha1.Equals(preflight.Target.Plan.Sha1Hex, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Step 27 0Harmony SHA-1 changed immediately before the Gate-T patch-engine runtime boundary.");
            RequirePatchProbeCounters(2, 0, "Launcher patch probe counters changed before the Gate-T patch-engine runtime boundary.");
            if (context.RejectedManagedRequests.Count != 0)
                throw new FileLoadException("Gate T started with prior rejected managed requests: " + string.Join(" | ", context.RejectedManagedRequests));

            // 0.0.91 proved that adding HarmonySharedState runtime reflection to Gate O changed
            // resolver/load counters. Keep Gate O on the physically passing 0.0.90 runtime surface;
            // measure every newly introduced runtime reflection operation here instead.
            var frameworkManagedBefore = context.ManagedResolverRequests.Count;
            var frameworkPrivateBefore = context.PrivateLoads.Count;
            var frameworkHostBefore = context.HostLoads.Count;
            var frameworkNativeBefore = context.NativeLoadAttempts.Count;
            var frameworkRejectedBefore = context.RejectedManagedRequests.Count;
            var frameworkMembershipBefore = SnapshotPrivateContextMembership(context);

            stage = "bounded patch-engine host-framework preservation preflight";
            ReportProgress(progress, ControlledHarmonyPatchExecutionGate.PatchEngineExecution,
                "T1 — entering bounded Reflection.Emit/RuntimeMethodHandle runtime preservation preflight; no HarmonySharedState runtime reflection, initializer, Patch(), or interpreted target invocation yet.");
            ValidatePatchEngineHostFrameworkPreservationSurface();
            var frameworkMembershipAfter = SnapshotPrivateContextMembership(context);
            if (!frameworkMembershipAfter.SequenceEqual(frameworkMembershipBefore, StringComparer.Ordinal))
                throw new InvalidDataException("Bounded patch-engine host-framework preflight unexpectedly changed private-context membership.");
            if (context.NativeLoadAttempts.Count != frameworkNativeBefore)
                throw new DllNotFoundException("Bounded patch-engine host-framework preflight attempted private-context native resolution.");
            if (context.RejectedManagedRequests.Count != frameworkRejectedBefore)
                throw new FileLoadException("Bounded patch-engine host-framework preflight triggered an unplanned managed request: " + string.Join(" | ", context.RejectedManagedRequests.Skip(frameworkRejectedBefore)));
            RequirePatchProbeCounters(2, 0, "Bounded patch-engine host-framework preflight unexpectedly invoked the interpreted fixture target or prefix.");
            var frameworkPostSha1 = ComputeSha1Hex(preflight.Target.PreparedPath);
            if (!frameworkPostSha1.Equals(targetSha1, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("0Harmony prepared bytes changed across the bounded patch-engine host-framework preflight.");
            var frameworkManagedDelta = context.ManagedResolverRequests.Count - frameworkManagedBefore;
            var frameworkPrivateDelta = context.PrivateLoads.Count - frameworkPrivateBefore;
            var frameworkHostDelta = context.HostLoads.Count - frameworkHostBefore;
            ReportProgress(progress, ControlledHarmonyPatchExecutionGate.PatchEngineExecution,
                $"T2 — bounded host-framework preflight returned with private membership unchanged; resolver/load deltas measured: managed={frameworkManagedDelta}, private={frameworkPrivateDelta}, host={frameworkHostDelta}, native=0.");

            var sharedResolutionManagedBefore = context.ManagedResolverRequests.Count;
            var sharedResolutionPrivateBefore = context.PrivateLoads.Count;
            var sharedResolutionHostBefore = context.HostLoads.Count;
            var sharedResolutionNativeBefore = context.NativeLoadAttempts.Count;
            var sharedResolutionRejectedBefore = context.RejectedManagedRequests.Count;
            var sharedResolutionMembershipBefore = SnapshotPrivateContextMembership(context);

            stage = "exact HarmonySharedState runtime reflection resolution";
            ReportProgress(progress, ControlledHarmonyPatchExecutionGate.PatchEngineExecution,
                "T3 — entering exact HarmonySharedState runtime Type/.cctor/internalVersion/actualVersion/state reflection from the already-loaded bounded iOS-normalized 0Harmony image. The initializer and Patch() remain uninvoked.");
            var harmonySharedStateType = initialization.TargetAssembly.GetType(HarmonySharedStateTypeFullName, throwOnError: false, ignoreCase: false)
                ?? throw new TypeLoadException("Exact HarmonyLib.HarmonySharedState type is absent from loaded 0Harmony.");
            if (!ReferenceEquals(harmonySharedStateType.Assembly, initialization.TargetAssembly))
                throw new InvalidDataException("HarmonySharedState runtime Type resolved from an unexpected assembly instance.");
            if (harmonySharedStateType.IsPublic || !harmonySharedStateType.IsAbstract || !harmonySharedStateType.IsSealed)
                throw new InvalidDataException("HarmonyLib.HarmonySharedState is no longer the expected internal static type.");
            var harmonySharedStateTypeInitializer = harmonySharedStateType.TypeInitializer
                ?? throw new MissingMethodException(HarmonySharedStateTypeFullName, ".cctor()");
            var harmonySharedStateInternalVersionField = harmonySharedStateType.GetField("internalVersion", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)
                ?? throw new MissingFieldException(HarmonySharedStateTypeFullName, "internalVersion");
            var harmonySharedStateActualVersionField = harmonySharedStateType.GetField("actualVersion", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)
                ?? throw new MissingFieldException(HarmonySharedStateTypeFullName, "actualVersion");
            var harmonySharedStateStateField = harmonySharedStateType.GetField("state", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)
                ?? throw new MissingFieldException(HarmonySharedStateTypeFullName, "state");
            var harmonySharedStateOriginalsField = harmonySharedStateType.GetField("originals", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)
                ?? throw new MissingFieldException(HarmonySharedStateTypeFullName, "originals");
            var harmonySharedStateOriginalsMonoField = harmonySharedStateType.GetField("originalsMono", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)
                ?? throw new MissingFieldException(HarmonySharedStateTypeFullName, "originalsMono");
            var harmonySharedStateMethodAddressRefField = harmonySharedStateType.GetField("methodAddressRef", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)
                ?? throw new MissingFieldException(HarmonySharedStateTypeFullName, "methodAddressRef");
            if (!harmonySharedStateInternalVersionField.IsLiteral || harmonySharedStateInternalVersionField.FieldType != typeof(int) ||
                harmonySharedStateInternalVersionField.GetRawConstantValue() is not int internalVersion || internalVersion != 102)
                throw new InvalidDataException("HarmonySharedState.internalVersion changed from the Gate-O-audited 2.4.2 value 102.");
            if (harmonySharedStateActualVersionField.FieldType != typeof(int) || !harmonySharedStateActualVersionField.IsInitOnly)
                throw new InvalidDataException("HarmonySharedState.actualVersion runtime field shape changed.");

            var sharedResolutionMembershipAfter = SnapshotPrivateContextMembership(context);
            if (!sharedResolutionMembershipAfter.SequenceEqual(sharedResolutionMembershipBefore, StringComparer.Ordinal))
                throw new InvalidDataException("HarmonySharedState runtime reflection unexpectedly changed private-context membership before its initializer.");
            if (context.NativeLoadAttempts.Count != sharedResolutionNativeBefore)
                throw new DllNotFoundException("HarmonySharedState runtime reflection attempted private-context native resolution.");
            if (context.RejectedManagedRequests.Count != sharedResolutionRejectedBefore)
                throw new FileLoadException("HarmonySharedState runtime reflection triggered an unplanned managed request: " + string.Join(" | ", context.RejectedManagedRequests.Skip(sharedResolutionRejectedBefore)));
            RequirePatchProbeCounters(2, 0, "HarmonySharedState runtime reflection unexpectedly invoked the interpreted fixture target or prefix.");
            var sharedResolutionPostSha1 = ComputeSha1Hex(preflight.Target.PreparedPath);
            if (!sharedResolutionPostSha1.Equals(targetSha1, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("0Harmony prepared bytes changed across HarmonySharedState runtime reflection.");
            var sharedResolutionManagedDelta = context.ManagedResolverRequests.Count - sharedResolutionManagedBefore;
            var sharedResolutionPrivateDelta = context.PrivateLoads.Count - sharedResolutionPrivateBefore;
            var sharedResolutionHostDelta = context.HostLoads.Count - sharedResolutionHostBefore;
            ReportProgress(progress, ControlledHarmonyPatchExecutionGate.PatchEngineExecution,
                $"T4 — normalized HarmonySharedState runtime reflection returned; internalVersion=102 and private membership unchanged. Measured deltas: managed={sharedResolutionManagedDelta}, private={sharedResolutionPrivateDelta}, host={sharedResolutionHostDelta}, native=0. Static field values were NOT read and the cctor was NOT run.");

            var sharedManagedBefore = context.ManagedResolverRequests.Count;
            var sharedPrivateBefore = context.PrivateLoads.Count;
            var sharedHostBefore = context.HostLoads.Count;
            var sharedNativeBefore = context.NativeLoadAttempts.Count;
            var sharedRejectedBefore = context.RejectedManagedRequests.Count;
            var sharedMembershipBefore = SnapshotPrivateContextMembership(context);

            stage = "bounded iOS-normalized HarmonySharedState type initialization";
            var generatedBeforeSharedInitialization = DescribeKnownPatchEngineGeneratedAssemblies();
            if (generatedBeforeSharedInitialization.Length != 0)
                throw new InvalidDataException("HarmonySharedState initialization began with an unexpected pre-existing generated patch-engine assembly: " + FormatNames(generatedBeforeSharedInitialization));

            var runtimeImageHash = Convert.ToHexString(SHA1.HashData(preflight.HarmonyRuntimeImage.RuntimeImageBytes)).ToLowerInvariant();
            if (!runtimeImageHash.Equals(preflight.HarmonyRuntimeImage.RuntimeImageSha1, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("The bounded iOS-normalized Harmony runtime image changed after Gate A.");
            ReportProgress(progress, ControlledHarmonyPatchExecutionGate.PatchEngineExecution,
                "T5a — bounded iOS-normalized HarmonySharedState cctor image reverified in memory; no generated HarmonySharedState/ILGeneratorProxy assembly exists before initialization. PatchProcessor.Patch() and interpreted target remain uninvoked.");
            ReportProgress(progress, ControlledHarmonyPatchExecutionGate.PatchEngineExecution,
                "T5b — entering RuntimeHelpers.RunClassConstructor(HarmonySharedState.TypeHandle) against the normalized direct-state initializer; dynamic shared-state assembly creation and StackFrame FieldRefAccess initialization are absent from this runtime cctor. PatchProcessor.Patch() and interpreted target remain uninvoked.");
            RuntimeHelpers.RunClassConstructor(harmonySharedStateType.TypeHandle);
            ReportProgress(progress, ControlledHarmonyPatchExecutionGate.PatchEngineExecution,
                "T6 — normalized HarmonySharedState::.cctor returned; validating direct state dictionaries, null methodAddressRef, actualVersion=102, zero generated shared-state assemblies, hashes, and isolation before Patch().");

            var actualVersionValue = harmonySharedStateActualVersionField.GetValue(null);
            if (actualVersionValue is not int actualVersion || actualVersion != HarmonySharedStateInternalVersion)
                throw new InvalidDataException($"HarmonySharedState.actualVersion changed from the normalized value {HarmonySharedStateInternalVersion} after explicit initialization; observed {actualVersionValue ?? "<null>"}.");
            if (harmonySharedStateStateField.GetValue(null) is null ||
                harmonySharedStateOriginalsField.GetValue(null) is null ||
                harmonySharedStateOriginalsMonoField.GetValue(null) is null)
                throw new InvalidDataException("Normalized HarmonySharedState initialization did not initialize all three direct state dictionaries.");
            if (harmonySharedStateMethodAddressRefField.GetValue(null) is not null)
                throw new InvalidDataException("Normalized HarmonySharedState.methodAddressRef must remain null on the bounded iOS runtime path.");
            RequirePatchProbeCounters(2, 0, "HarmonySharedState initialization unexpectedly invoked the interpreted fixture target or prefix.");
            if (context.NativeLoadAttempts.Count != sharedNativeBefore)
                throw new DllNotFoundException("HarmonySharedState initialization attempted private-context native resolution: " + string.Join(" | ", context.NativeLoadAttempts.Skip(sharedNativeBefore)));
            if (context.RejectedManagedRequests.Count != sharedRejectedBefore)
                throw new FileLoadException("HarmonySharedState initialization triggered an unplanned managed request: " + string.Join(" | ", context.RejectedManagedRequests.Skip(sharedRejectedBefore)));
            var sharedMembershipAfter = ValidateBoundedPatchEngineContextTransition(
                context,
                sharedMembershipBefore,
                "HarmonySharedState initialization");
            var sharedGeneratedAssemblies = DescribeKnownPatchEngineGeneratedAssemblies();
            if (sharedGeneratedAssemblies.Length != 0)
                throw new InvalidDataException("Normalized HarmonySharedState initialization unexpectedly generated a patch-engine assembly: " + FormatNames(sharedGeneratedAssemblies));
            var sharedPostSha1 = ComputeSha1Hex(preflight.Target.PreparedPath);
            if (!sharedPostSha1.Equals(targetSha1, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("0Harmony prepared bytes changed across explicit HarmonySharedState initialization.");

            _harmonySharedStateInitialization = new HarmonySharedStateInitializationSnapshot(
                sharedPostSha1,
                actualVersion,
                context.ManagedResolverRequests.Count - sharedManagedBefore,
                context.PrivateLoads.Count - sharedPrivateBefore,
                context.HostLoads.Count - sharedHostBefore,
                context.NativeLoadAttempts.Count - sharedNativeBefore,
                sharedMembershipAfter,
                sharedGeneratedAssemblies);

            stage = "post-publish System.Linq patch-engine member preservation preflight";
            ReportProgress(progress, ControlledHarmonyPatchExecutionGate.PatchEngineExecution,
                "T6a — entering exact host System.Linq callable-surface preflight for Harmony MethodCreator Select/Union/ToDictionary; PatchProcessor.Patch() and interpreted target remain uninvoked.");
            var linqFrameworkSurface = ValidatePatchEngineLinqFrameworkPreservationSurface();
            ReportProgress(progress, ControlledHarmonyPatchExecutionGate.PatchEngineExecution,
                "T6b — host System.Linq MethodCreator callable surface is present under the copy/no-link host policy: " + linqFrameworkSurface + ". Entering PatchProcessor.Patch() is now permitted.");

            var managedBefore = context.ManagedResolverRequests.Count;
            var privateBefore = context.PrivateLoads.Count;
            var hostBefore = context.HostLoads.Count;
            var nativeBefore = context.NativeLoadAttempts.Count;
            var rejectedBefore = context.RejectedManagedRequests.Count;
            var membershipBefore = SnapshotPrivateContextMembership(context);

            stage = "exact PatchProcessor.Patch() invocation after explicit shared-state initialization";
            object? rawReplacement;
            try
            {
                ReportProgress(progress, ControlledHarmonyPatchExecutionGate.PatchEngineExecution,
                    "T7 — entering the first exact PatchProcessor.Patch() reflection invocation after explicit HarmonySharedState initialization; interpreted target is still not invoked.");
                rawReplacement = patchApi.PatchMethod.Invoke(processor, null);
                ReportProgress(progress, ControlledHarmonyPatchExecutionGate.PatchEngineExecution,
                    "T8 — PatchProcessor.Patch() returned; validating replacement MethodInfo and bounded isolation state.");
            }
            catch (TargetInvocationException ex) when (ex.InnerException is not null)
            {
                throw new InvalidOperationException("Exact PatchProcessor.Patch() threw after explicit HarmonySharedState initialization.", ex.InnerException);
            }
            if (rawReplacement is not MethodInfo replacement)
                throw new InvalidDataException("PatchProcessor.Patch() did not return a System.Reflection.MethodInfo replacement.");
            if (replacement.ReturnType != typeof(int))
                throw new InvalidDataException("Harmony replacement return type does not match the post-publish interpreted Int32 target.");
            var replacementParameters = replacement.GetParameters();
            if (replacementParameters.Length != 1 || replacementParameters[0].ParameterType != typeof(int))
                throw new InvalidDataException("Harmony replacement parameter surface does not match the post-publish interpreted Int32 target.");

            if (context.NativeLoadAttempts.Count != nativeBefore)
                throw new DllNotFoundException("PatchProcessor.Patch attempted private-context native resolution: " + string.Join(" | ", context.NativeLoadAttempts.Skip(nativeBefore)));
            if (context.RejectedManagedRequests.Count != rejectedBefore)
                throw new FileLoadException("PatchProcessor.Patch triggered an unplanned managed request: " + string.Join(" | ", context.RejectedManagedRequests.Skip(rejectedBefore)));
            var membershipAfter = ValidateBoundedPatchEngineContextTransition(
                context,
                membershipBefore,
                "PatchProcessor.Patch");
            var generatedAssembliesAfterPatch = DescribeKnownPatchEngineGeneratedAssemblies();
            var postSha1 = ComputeSha1Hex(preflight.Target.PreparedPath);
            if (!postSha1.Equals(targetSha1, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("0Harmony prepared bytes changed across Patch().");
            RequirePatchProbeCounters(2, 0, "Patch installation unexpectedly invoked the interpreted fixture target or prefix.");

            _replacementMethod = replacement;
            _patchExecution = new PatchExecutionSnapshot(
                postSha1,
                replacement.Name,
                replacement.DeclaringType?.FullName ?? "<dynamic/no-declaring-type>",
                context.ManagedResolverRequests.Count - managedBefore,
                context.PrivateLoads.Count - privateBefore,
                context.HostLoads.Count - hostBefore,
                context.NativeLoadAttempts.Count - nativeBefore,
                membershipAfter,
                generatedAssembliesAfterPatch);

            ReportProgress(progress, ControlledHarmonyPatchExecutionGate.PatchEngineExecution,
                "T9 — measured host/runtime resolution, explicit HarmonySharedState initialization, and exact PatchProcessor.Patch() all completed with replacement/isolation validation; interpreted target remains uninvoked until Gate V.");

            return Pass(
                ControlledHarmonyPatchExecutionGate.PatchEngineExecution,
                "MEASURED PATCH-ENGINE RUNTIME RESOLUTION + IOS-NORMALIZED HARMONY SHARED-STATE INITIALIZATION + FIRST REAL HARMONY PATCH ENGINE EXECUTION COMPLETED AGAINST POST-PUBLISH INTERPRETED TARGET.\n" +
                $"Bounded host-framework preflight deltas: managed={frameworkManagedDelta:N0}; private={frameworkPrivateDelta:N0}; host={frameworkHostDelta:N0}; native=0\n" +
                $"HarmonySharedState runtime-reflection deltas: managed={sharedResolutionManagedDelta:N0}; private={sharedResolutionPrivateDelta:N0}; host={sharedResolutionHostDelta:N0}; native=0\n" +
                "HarmonySharedState source metadata: exact Gate-O-audited 2.4.2 shape; runtime cctor: Gate-A-audited 11-instruction iOS-normalized direct-state shape\n" +
                "RuntimeHelpers.RunClassConstructor(HarmonySharedState.TypeHandle): EXACTLY ONCE against normalized runtime image, before public Patch()\n" +
                $"HarmonySharedState.actualVersion: {_harmonySharedStateInitialization.ActualVersion:N0} (expected 102)\n" +
                $"Managed resolver requests during shared-state initialization: {_harmonySharedStateInitialization.ManagedResolverRequests:N0}\n" +
                $"Private loads during shared-state initialization: {_harmonySharedStateInitialization.PrivateLoads:N0}\n" +
                $"Host loads during shared-state initialization: {_harmonySharedStateInitialization.HostLoads:N0}\n" +
                $"Native load attempts during shared-state initialization: {_harmonySharedStateInitialization.NativeLoadAttempts:N0}\n" +
                $"Known generated assemblies after normalized shared-state initialization: {FormatNames(_harmonySharedStateInitialization.KnownGeneratedAssemblies)} (required: none)\n" +
                "API invoked: HarmonyLib.PatchProcessor::Patch() — EXACTLY ONCE\n" +
                $"Original target: {probe.TargetSignature}\n" +
                $"Registered prefix: {probe.PrefixSignature}\n" +
                $"Replacement MethodInfo: {_patchExecution.ReplacementDeclaringType}::{_patchExecution.ReplacementName}\n" +
                $"Managed resolver requests during Patch(): {_patchExecution.ManagedResolverRequests:N0}\n" +
                $"Private loads during Patch(): {_patchExecution.PrivateLoads:N0}\n" +
                $"Host loads during Patch(): {_patchExecution.HostLoads:N0}\n" +
                $"Native load attempts during Patch(): {_patchExecution.NativeLoadAttempts:N0}\n" +
                $"Known generated assemblies after Patch(): {FormatNames(_patchExecution.KnownGeneratedAssemblies)}\n" +
                "Interpreted target invoked after patch: NO — Gate V owns execution of patched behavior\n" +
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
            var expected = patchExecution.PrivateContextMembership;
            var actual = SnapshotPrivateContextMembership(context);
            if (!actual.SequenceEqual(expected, StringComparer.Ordinal))
                throw new InvalidDataException("Step 27 post-patch private context changed after the exact Gate-T patch-engine snapshot.");
            if (context.NativeLoadAttempts.Count != 0)
                throw new DllNotFoundException("Step 27 observed native-library resolution during patch installation: " + string.Join(" | ", context.NativeLoadAttempts));
            if (context.RejectedManagedRequests.Count != 0)
                throw new FileLoadException("Step 27 observed rejected/unplanned managed requests during patch installation: " + string.Join(" | ", context.RejectedManagedRequests));
            if (!ReferenceEquals(patchApi.PrefixField.GetValue(processor), descriptor) || !ReferenceEquals(patchApi.HarmonyMethodMethodField.GetValue(descriptor), probe.Prefix))
                throw new InvalidDataException("Step 27 registered prefix descriptor changed across Patch().");
            RequirePatchProbeCounters(2, 0, "Patch installation audit observed unexpected interpreted fixture target/prefix invocation.");
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

    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Step 27.0.24 invokes only exact MethodInfo objects from the launcher-owned post-publish interpreted fixture admitted in Gate P.")]
    public ControlledHarmonyPatchExecutionGateResult RunPatchedProbeInvocation()
    {
        var stage = "patched post-publish interpreted fixture invocation";
        try
        {
            ThrowIfDisposed();
            var probe = RequirePatchProbe();
            _ = RequirePatchExecution();
            if (!_postPatchAuditPassed)
                throw new InvalidOperationException("Step 27 Gate U must pass before invoking patched interpreted fixture behavior.");
            var context = RequireLoadContext();
            var membershipBefore = SnapshotPrivateContextMembership(context);
            var nativeBefore = context.NativeLoadAttempts.Count;

            RequirePatchProbeCounters(2, 0, "Step 27.0.24 patched fixture invocation did not begin from the established baseline counters.");

            stage = "patched Target reflection invocation";
            var reflectedResult = InvokePatchProbeInt32(probe.Target, 41, "Step 27.0.24 patched Target reflection invocation");
            var afterReflection = ReadPatchProbeCounters();

            stage = "patched in-fixture direct-call invocation";
            var directResult = InvokePatchProbeInt32(probe.InvokeTarget, 41, "Step 27.0.24 patched InvokeTarget invocation");
            var finalCounters = ReadPatchProbeCounters();

            if (reflectedResult != 1041 || afterReflection.TargetCalls != 2 || afterReflection.PrefixCalls != 1)
                throw new InvalidDataException($"Patched interpreted reflection route did not execute exact prefix/skip-original behavior: result={reflectedResult}, targetCalls={afterReflection.TargetCalls}, prefixCalls={afterReflection.PrefixCalls}.");
            if (directResult != 1041 || finalCounters.TargetCalls != 2 || finalCounters.PrefixCalls != 2)
                throw new InvalidDataException($"Patched interpreted in-fixture direct route did not execute exact prefix/skip-original behavior: result={directResult}, targetCalls={finalCounters.TargetCalls}, prefixCalls={finalCounters.PrefixCalls}.");
            if (context.NativeLoadAttempts.Count != nativeBefore || context.RejectedManagedRequests.Count != 0)
                throw new InvalidDataException("Patched interpreted fixture invocation caused native or rejected managed resolution.");
            var membershipAfter = SnapshotPrivateContextMembership(context);
            if (!membershipAfter.SequenceEqual(membershipBefore, StringComparer.Ordinal))
                throw new InvalidDataException("Patched interpreted fixture invocation changed private-context membership.");

            _patchedProbeInvocation = new ProbeInvocationSnapshot(
                reflectedResult,
                directResult,
                finalCounters.TargetCalls,
                finalCounters.PrefixCalls);
            return Pass(
                ControlledHarmonyPatchExecutionGate.PatchedProbeInvocation,
                "POST-PUBLISH INTERPRETED PATCHED METHOD EXECUTION SUCCEEDED THROUGH REFLECTION + IN-FIXTURE DIRECT CALL.\n" +
                "Input: 41\n" +
                $"Patched Target reflection result: {reflectedResult}\n" +
                $"Patched InvokeTarget result: {directResult}\n" +
                $"Target-body calls after both patched invocations: {finalCounters.TargetCalls} — unchanged from baseline 2\n" +
                $"Prefix calls after both patched invocations: {finalCounters.PrefixCalls}\n" +
                "Prefix set __result = value + 1000 and returned false: PHYSICALLY OBSERVED\n" +
                "Original interpreted Target body skipped on both routes: YES\n" +
                "StS2 type/member reflected, patched, or invoked: NO");
        }
        catch (Exception ex)
        {
            return Fail(ControlledHarmonyPatchExecutionGate.PatchedProbeInvocation, stage, ex);
        }
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Step 27.0.24 invokes exactly PatchProcessor.Unpatch(MethodInfo) with the exact post-publish interpreted prefix admitted in Gate P.")]
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
            var membershipBefore = SnapshotPrivateContextMembership(context);
            var countersBefore = ReadPatchProbeCounters();

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
            var countersAfter = ReadPatchProbeCounters();
            if (countersAfter != countersBefore)
                throw new InvalidDataException("Exact prefix unpatch unexpectedly invoked the interpreted fixture target or prefix.");
            if (context.NativeLoadAttempts.Count != nativeBefore)
                throw new DllNotFoundException("Exact prefix unpatch attempted native resolution: " + string.Join(" | ", context.NativeLoadAttempts.Skip(nativeBefore)));
            if (context.RejectedManagedRequests.Count != 0)
                throw new FileLoadException("Exact prefix unpatch triggered an unplanned managed request: " + string.Join(" | ", context.RejectedManagedRequests));
            var membershipAfter = SnapshotPrivateContextMembership(context);
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
                countersBefore.TargetCalls,
                countersBefore.PrefixCalls);

            return Pass(
                ControlledHarmonyPatchExecutionGate.ExactPrefixUnpatch,
                "EXACT POST-PUBLISH INTERPRETED HARMONY PREFIX REMOVAL COMPLETED.\n" +
                "API invoked: PatchProcessor.Unpatch(System.Reflection.MethodInfo) — exact prefix MethodInfo only\n" +
                $"Removed prefix: {probe.PrefixSignature}\n" +
                $"Managed resolver requests during unpatch: {_unpatch.ManagedResolverRequests:N0}\n" +
                $"Private loads during unpatch: {_unpatch.PrivateLoads:N0}\n" +
                $"Host loads during unpatch: {_unpatch.HostLoads:N0}\n" +
                $"Native load attempts during unpatch: {_unpatch.NativeLoadAttempts:N0}\n" +
                "Interpreted target/prefix invoked during unpatch: NO\n" +
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
            var patchExecution = RequirePatchExecution();
            var unpatch = RequireUnpatch();
            var context = RequireLoadContext();

            var targetSha1 = ComputeSha1Hex(preflight.Target.PreparedPath);
            if (!targetSha1.Equals(unpatch.PreparedSha1, StringComparison.OrdinalIgnoreCase) || !targetSha1.Equals(preflight.Target.Plan.Sha1Hex, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Step 27 0Harmony prepared bytes changed after exact prefix unpatch.");
            var expected = patchExecution.PrivateContextMembership;
            var actual = SnapshotPrivateContextMembership(context);
            if (!actual.SequenceEqual(expected, StringComparer.Ordinal))
                throw new InvalidDataException("Step 27 post-unpatch private context changed from the exact Gate-T patch-engine snapshot.");
            if (context.NativeLoadAttempts.Count != 0)
                throw new DllNotFoundException("Step 27 observed native-library resolution during patch/unpatch: " + string.Join(" | ", context.NativeLoadAttempts));
            if (context.RejectedManagedRequests.Count != 0)
                throw new FileLoadException("Step 27 observed rejected/unplanned managed requests during patch/unpatch: " + string.Join(" | ", context.RejectedManagedRequests));
            var counters = ReadPatchProbeCounters();
            if (counters.TargetCalls != unpatch.TargetCallsAtUnpatch || counters.PrefixCalls != unpatch.PrefixCallsAtUnpatch)
                throw new InvalidDataException("Step 27 post-unpatch audit observed unexpected interpreted fixture target/prefix invocation.");

            _postUnpatchAuditPassed = true;
            return Pass(
                ControlledHarmonyPatchExecutionGate.PostUnpatchAudit,
                "POST-UNPATCH ISOLATION AUDIT PASSED BEFORE RESTORED INTERPRETED TARGET INVOCATION.\n" +
                $"Private context: {actual.Length:N0}/{expected.Length:N0} expected assemblies\n" +
                "0Harmony prepared SHA-1 unchanged: YES\n" +
                "Native load attempts: 0\n" +
                "Rejected/unplanned managed requests: 0\n" +
                $"Target calls remain: {counters.TargetCalls}\n" +
                $"Prefix calls remain: {counters.PrefixCalls}\n" +
                "Restored interpreted behavior not yet invoked: YES\n" +
                "StS2 type/member reflected, patched, or invoked: NO");
        }
        catch (Exception ex)
        {
            return Fail(ControlledHarmonyPatchExecutionGate.PostUnpatchAudit, stage, ex);
        }
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Step 27.0.24 invokes only exact MethodInfo objects from the launcher-owned post-publish interpreted fixture admitted in Gate P.")]
    public ControlledHarmonyPatchExecutionGateResult RunRestoredProbeInvocation()
    {
        var stage = "restored post-publish interpreted fixture invocation";
        try
        {
            ThrowIfDisposed();
            var probe = RequirePatchProbe();
            _ = RequireUnpatch();
            if (!_postUnpatchAuditPassed)
                throw new InvalidOperationException("Step 27 Gate X must pass before verifying restored interpreted fixture behavior.");
            var context = RequireLoadContext();
            var membershipBefore = SnapshotPrivateContextMembership(context);
            var nativeBefore = context.NativeLoadAttempts.Count;

            var countersBefore = ReadPatchProbeCounters();
            if (countersBefore.TargetCalls != 2 || countersBefore.PrefixCalls != 2)
                throw new InvalidDataException($"Step 27 restored invocation expected patched-phase counters target=2/prefix=2, observed target={countersBefore.TargetCalls}/prefix={countersBefore.PrefixCalls}.");

            stage = "restored Target reflection invocation";
            var reflectedResult = InvokePatchProbeInt32(probe.Target, 41, "Step 27.0.24 restored Target reflection invocation");
            stage = "restored in-fixture direct-call invocation";
            var directResult = InvokePatchProbeInt32(probe.InvokeTarget, 41, "Step 27.0.24 restored InvokeTarget invocation");
            var counters = ReadPatchProbeCounters();

            if (reflectedResult != 42 || directResult != 42 || counters.TargetCalls != 4 || counters.PrefixCalls != 2)
                throw new InvalidDataException($"Exact unpatch did not restore interpreted baseline behavior on both routes: reflection={reflectedResult}, direct={directResult}, targetCalls={counters.TargetCalls}, prefixCalls={counters.PrefixCalls}.");
            if (context.NativeLoadAttempts.Count != nativeBefore || context.RejectedManagedRequests.Count != 0)
                throw new InvalidDataException("Restored interpreted fixture invocation caused native or rejected managed resolution.");
            var membershipAfter = SnapshotPrivateContextMembership(context);
            if (!membershipAfter.SequenceEqual(membershipBefore, StringComparer.Ordinal))
                throw new InvalidDataException("Restored interpreted fixture invocation changed private-context membership.");

            _restoredProbeInvocation = new ProbeInvocationSnapshot(
                reflectedResult,
                directResult,
                counters.TargetCalls,
                counters.PrefixCalls);
            return Pass(
                ControlledHarmonyPatchExecutionGate.RestoredProbeInvocation,
                "POST-PUBLISH INTERPRETED ORIGINAL BEHAVIOR RESTORED AFTER EXACT PREFIX UNPATCH.\n" +
                "Input: 41\n" +
                $"Restored Target reflection result: {reflectedResult}\n" +
                $"Restored InvokeTarget result: {directResult}\n" +
                $"Target-body calls: {counters.TargetCalls}\n" +
                $"Prefix calls: {counters.PrefixCalls} — unchanged across restored invocations\n" +
                "Original value + 1 behavior restored on both interpreted routes: YES\n" +
                "StS2 type/member reflected, patched, or invoked: NO");
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
            var expected = patchExecution.PrivateContextMembership;
            var actual = SnapshotPrivateContextMembership(context);
            if (!actual.SequenceEqual(expected, StringComparer.Ordinal))
                throw new InvalidDataException("Step 27 final private context changed from the exact Gate-T patch-engine snapshot.");
            if (context.NativeLoadAttempts.Count != 0)
                throw new DllNotFoundException("Step 27 observed native-library resolution: " + string.Join(" | ", context.NativeLoadAttempts));
            if (context.RejectedManagedRequests.Count != 0)
                throw new FileLoadException("Step 27 observed rejected/unplanned managed requests: " + string.Join(" | ", context.RejectedManagedRequests));
            if (!ReferenceEquals(processor.GetType(), processorApi.PatchProcessorType) ||
                !ReferenceEquals(processorApi.InstanceField.GetValue(processor), harmonyInstance) ||
                !ReferenceEquals(processorApi.OriginalField.GetValue(processor), probe.Target))
                throw new InvalidDataException("Step 27 retained interpreted-target PatchProcessor/Harmony/original identity changed.");
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
                "Patch lifecycle: post-publish interpreted target admission → bounded HarmonyMethod descriptor registration → Patch() → patched reflection/in-fixture-direct invocation → Unpatch(MethodInfo) → restored reflection/in-fixture-direct invocation\n" +
                $"Interpreted fixture SHA-256: {probe.FixtureSha256}\n" +
                "Patched result: 1041 on both routes\n" +
                "Restored result: 42 on both routes\n" +
                $"Final interpreted target calls: {restored.TargetCalls}\n" +
                $"Final interpreted prefix calls: {restored.PrefixCalls}\n" +
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
        _harmonySharedStateInitialization = null;
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
            throw new InvalidDataException("Step 27 requires a fresh process; a game/Harmony/shared-state assembly is already loaded: " + string.Join(" | ", matches));
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
        if (cctor.Body.Instructions.Count != 57)
            hazards.Add($"AccessTools type initializer instruction count changed: observed {cctor.Body.Instructions.Count}, expected 57.");

        var expectedOpcodeCounts = new Dictionary<Code, int>
        {
            [Code.Ldnull] = 6,
            [Code.Stsfld] = 8,
            [Code.Ldc_I4] = 1,
            [Code.Ldsfld] = 1,
            [Code.Ldc_I4_2] = 1,
            [Code.Or] = 1,
            [Code.Ldc_I4_0] = 5,
            [Code.Ldc_I4_1] = 1,
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

        var typedGetTypeCalls = cctor.Body.Instructions
            .Where(instruction => instruction.OpCode.Code == Code.Call &&
                instruction.Operand is MethodReference method &&
                method.FullName.Equals("System.Type System.Type::GetType(System.String,System.Boolean)", StringComparison.Ordinal))
            .ToArray();
        if (typedGetTypeCalls.Length != 2 ||
            typedGetTypeCalls[0].Previous is null || !TryGetLdcI4Value(typedGetTypeCalls[0].Previous, out var firstThrowOnError) || firstThrowOnError != 0 ||
            typedGetTypeCalls[1].Previous is null || !TryGetLdcI4Value(typedGetTypeCalls[1].Previous, out var secondThrowOnError) || secondThrowOnError != 0)
        {
            hazards.Add("AccessTools::.cctor RuntimeInformation Type.GetType throwOnError operands changed; expected false then false.");
        }

        var readerWriterLockConstructors = cctor.Body.Instructions
            .Where(instruction => instruction.OpCode.Code == Code.Newobj &&
                instruction.Operand is MethodReference method &&
                method.FullName.Equals("System.Void System.Threading.ReaderWriterLockSlim::.ctor(System.Threading.LockRecursionPolicy)", StringComparison.Ordinal))
            .ToArray();
        if (readerWriterLockConstructors.Length != 1 ||
            readerWriterLockConstructors[0].Previous is null ||
            !TryGetLdcI4Value(readerWriterLockConstructors[0].Previous, out var lockRecursionPolicy) ||
            lockRecursionPolicy != (int)System.Threading.LockRecursionPolicy.SupportsRecursion)
        {
            hazards.Add("AccessTools::.cctor ReaderWriterLockSlim recursion-policy operand changed; expected SupportsRecursion (1).");
        }

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
            $"Type initializer instructions: {cctor.Body.Instructions.Count:N0} (expected 57)\n" +
            $"AccessTools.all expected BindingFlags value: {expectedAll}\n" +
            $"AccessTools.allDeclared expected BindingFlags value: {expectedAllDeclared}\n" +
            "Measured runtime probes: Mono.Runtime + RuntimeInformation.FrameworkDescription (.NET Framework / .NET Core legacy classification)\n" +
            "Measured cache initialization: allTypesCached=null + Dictionary<Type,FastInvokeHandler> + ReaderWriterLockSlim(SupportsRecursion)\n" +
            $"Blocking AccessTools initializer hazards: {hazards.Count:N0}" +
            (hazards.Count == 0 ? "\nExact Step 27.0.2 physical AccessTools initializer fingerprint: MATCH" : "\n" + string.Join("\n", hazards));
        return new AccessToolsMetadataSnapshot(hazards.Count == 0, detail, FormatMethodAudit(cctor));
    }

    private static void ValidatePatchEngineHostFrameworkPreservationSurface()
    {
        if (typeof(DynamicMethod).GetConstructors(BindingFlags.Public | BindingFlags.Instance).Length == 0)
            throw new MissingMethodException(typeof(DynamicMethod).FullName, ".ctor(...)");
        if (!typeof(ILGenerator).GetMethods(BindingFlags.Public | BindingFlags.Instance).Any(method => method.Name.Equals(nameof(ILGenerator.Emit), StringComparison.Ordinal)))
            throw new MissingMethodException(typeof(ILGenerator).FullName, nameof(ILGenerator.Emit));
        _ = typeof(AssemblyName).GetConstructor([typeof(string)])
            ?? throw new MissingMethodException(typeof(AssemblyName).FullName, ".ctor(System.String)");
        if (!typeof(AssemblyBuilder).GetMethods(BindingFlags.Public | BindingFlags.Static).Any(method => method.Name.Equals(nameof(AssemblyBuilder.DefineDynamicAssembly), StringComparison.Ordinal)))
            throw new MissingMethodException(typeof(AssemblyBuilder).FullName, nameof(AssemblyBuilder.DefineDynamicAssembly));
        if (!typeof(AssemblyBuilder).GetMethods(BindingFlags.Public | BindingFlags.Instance).Any(method => method.Name.Equals(nameof(AssemblyBuilder.DefineDynamicModule), StringComparison.Ordinal)))
            throw new MissingMethodException(typeof(AssemblyBuilder).FullName, nameof(AssemblyBuilder.DefineDynamicModule));
        if (!typeof(ModuleBuilder).GetMethods(BindingFlags.Public | BindingFlags.Instance).Any(method => method.Name.Equals(nameof(ModuleBuilder.DefineType), StringComparison.Ordinal)))
            throw new MissingMethodException(typeof(ModuleBuilder).FullName, nameof(ModuleBuilder.DefineType));
        if (!typeof(TypeBuilder).GetMethods(BindingFlags.Public | BindingFlags.Instance).Any(method => method.Name.Equals(nameof(TypeBuilder.DefineMethod), StringComparison.Ordinal)))
            throw new MissingMethodException(typeof(TypeBuilder).FullName, nameof(TypeBuilder.DefineMethod));
        if (!typeof(MethodBuilder).GetMethods(BindingFlags.Public | BindingFlags.Instance).Any(method => method.Name.Equals(nameof(MethodBuilder.GetILGenerator), StringComparison.Ordinal)))
            throw new MissingMethodException(typeof(MethodBuilder).FullName, nameof(MethodBuilder.GetILGenerator));
        _ = typeof(MethodBase).GetProperty(nameof(MethodBase.MethodHandle), BindingFlags.Public | BindingFlags.Instance)
            ?? throw new MissingMemberException(typeof(MethodBase).FullName, nameof(MethodBase.MethodHandle));
        if (!typeof(RuntimeMethodHandle).GetMethods(BindingFlags.Public | BindingFlags.Instance).Any(method => method.Name.Equals(nameof(RuntimeMethodHandle.GetFunctionPointer), StringComparison.Ordinal)))
            throw new MissingMethodException(typeof(RuntimeMethodHandle).FullName, nameof(RuntimeMethodHandle.GetFunctionPointer));
    }

    private static string ValidatePatchEngineLinqFrameworkPreservationSurface()
    {
        // Physical 0.0.105 proved that assembly binding alone is insufficient for post-publish
        // payloads under TrimMode=full: 0Harmony resolved System.Linq but MethodCreator failed on
        // the trimmed two-sequence Enumerable.Union<T> member. Keep this runtime check independent
        // of Harmony and do not invoke any LINQ operator; it verifies the exact public signatures
        // that the Gate-O-audited MethodCreatorConfig.Prepare path calls immediately.
        var enumerableType = typeof(Enumerable);
        var methods = enumerableType.GetMethods(BindingFlags.Public | BindingFlags.Static);

        static bool IsIEnumerableOfGenericParameter(Type type, int genericParameterPosition)
            => type.IsGenericType &&
               type.GetGenericTypeDefinition() == typeof(IEnumerable<>) &&
               type.GetGenericArguments()[0].IsGenericParameter &&
               type.GetGenericArguments()[0].GenericParameterPosition == genericParameterPosition;

        static bool IsExactTwoSequenceUnion(MethodInfo method)
        {
            if (!method.Name.Equals(nameof(Enumerable.Union), StringComparison.Ordinal) ||
                !method.IsGenericMethodDefinition ||
                method.GetGenericArguments().Length != 1)
            {
                return false;
            }

            var parameters = method.GetParameters();
            return parameters.Length == 2 &&
                   IsIEnumerableOfGenericParameter(parameters[0].ParameterType, 0) &&
                   IsIEnumerableOfGenericParameter(parameters[1].ParameterType, 0);
        }

        static bool IsExactNonIndexedSelect(MethodInfo method)
        {
            if (!method.Name.Equals(nameof(Enumerable.Select), StringComparison.Ordinal) ||
                !method.IsGenericMethodDefinition ||
                method.GetGenericArguments().Length != 2)
            {
                return false;
            }

            var parameters = method.GetParameters();
            if (parameters.Length != 2 ||
                !IsIEnumerableOfGenericParameter(parameters[0].ParameterType, 0) ||
                !parameters[1].ParameterType.IsGenericType ||
                parameters[1].ParameterType.GetGenericTypeDefinition() != typeof(Func<,>))
            {
                return false;
            }

            var selectorArguments = parameters[1].ParameterType.GetGenericArguments();
            return selectorArguments.Length == 2 &&
                   selectorArguments[0].IsGenericParameter &&
                   selectorArguments[0].GenericParameterPosition == 0 &&
                   selectorArguments[1].IsGenericParameter &&
                   selectorArguments[1].GenericParameterPosition == 1;
        }

        static bool IsExactThreeSelectorToDictionary(MethodInfo method)
        {
            if (!method.Name.Equals(nameof(Enumerable.ToDictionary), StringComparison.Ordinal) ||
                !method.IsGenericMethodDefinition ||
                method.GetGenericArguments().Length != 3)
            {
                return false;
            }

            var parameters = method.GetParameters();
            if (parameters.Length != 3 ||
                !IsIEnumerableOfGenericParameter(parameters[0].ParameterType, 0))
            {
                return false;
            }

            var keySelector = parameters[1].ParameterType;
            var elementSelector = parameters[2].ParameterType;
            if (!keySelector.IsGenericType || keySelector.GetGenericTypeDefinition() != typeof(Func<,>) ||
                !elementSelector.IsGenericType || elementSelector.GetGenericTypeDefinition() != typeof(Func<,>))
            {
                return false;
            }

            var keyArguments = keySelector.GetGenericArguments();
            var elementArguments = elementSelector.GetGenericArguments();
            return keyArguments.Length == 2 &&
                   keyArguments[0].IsGenericParameter && keyArguments[0].GenericParameterPosition == 0 &&
                   keyArguments[1].IsGenericParameter && keyArguments[1].GenericParameterPosition == 1 &&
                   elementArguments.Length == 2 &&
                   elementArguments[0].IsGenericParameter && elementArguments[0].GenericParameterPosition == 0 &&
                   elementArguments[1].IsGenericParameter && elementArguments[1].GenericParameterPosition == 2;
        }

        var union = methods.SingleOrDefault(IsExactTwoSequenceUnion);
        if (union is null)
        {
            throw new MissingMethodException(
                "System.Linq.Enumerable",
                "Union<TSource>(IEnumerable<TSource>, IEnumerable<TSource>)");
        }

        var select = methods.SingleOrDefault(IsExactNonIndexedSelect);
        if (select is null)
        {
            throw new MissingMethodException(
                "System.Linq.Enumerable",
                "Select<TSource,TResult>(IEnumerable<TSource>, Func<TSource,TResult>)");
        }

        var toDictionary = methods.SingleOrDefault(IsExactThreeSelectorToDictionary);
        if (toDictionary is null)
        {
            throw new MissingMethodException(
                "System.Linq.Enumerable",
                "ToDictionary<TSource,TKey,TElement>(IEnumerable<TSource>, Func<TSource,TKey>, Func<TSource,TElement>)");
        }

        return $"{enumerableType.Assembly.GetName().Name}: Select/Union/ToDictionary";
    }

    private static string[] SnapshotPrivateContextMembership(Step27LoadContext context)
        => context.Assemblies
            .Select(assembly => assembly.GetName().FullName ?? assembly.GetName().Name ?? string.Empty)
            .OrderBy(value => value, StringComparer.Ordinal)
            .ToArray();

    private static string[] ValidateBoundedPatchEngineContextTransition(
        Step27LoadContext context,
        string[] membershipBefore,
        string operation)
    {
        var membershipAfter = SnapshotPrivateContextMembership(context);
        var removed = membershipBefore.Except(membershipAfter, StringComparer.Ordinal).ToArray();
        if (removed.Length != 0)
            throw new InvalidDataException($"{operation} removed private-context assemblies unexpectedly: {FormatNames(removed)}");

        var added = membershipAfter.Except(membershipBefore, StringComparer.Ordinal).ToArray();
        foreach (var identity in added)
        {
            string? simpleName;
            try
            {
                simpleName = new AssemblyName(identity).Name;
            }
            catch (Exception ex)
            {
                throw new InvalidDataException($"{operation} added an assembly with an unreadable identity: {identity}", ex);
            }

            if (simpleName is null || !AllowedPatchEngineGeneratedAssemblySimpleNames.Contains(simpleName))
                throw new InvalidDataException($"{operation} added an unplanned private-context assembly: {identity}");
        }

        foreach (var allowedName in AllowedPatchEngineGeneratedAssemblySimpleNames)
        {
            var count = context.Assemblies.Count(assembly => string.Equals(assembly.GetName().Name, allowedName, StringComparison.Ordinal));
            if (count > 1)
                throw new InvalidDataException($"{operation} produced duplicate '{allowedName}' assemblies in the private context: {count}.");
        }

        return membershipAfter;
    }

    private static string[] DescribeKnownPatchEngineGeneratedAssemblies()
        => AppDomain.CurrentDomain.GetAssemblies()
            .Where(assembly => AllowedPatchEngineGeneratedAssemblySimpleNames.Contains(assembly.GetName().Name ?? string.Empty))
            .Select(assembly =>
            {
                var identity = assembly.GetName().FullName ?? assembly.GetName().Name ?? string.Empty;
                var loadContext = AssemblyLoadContext.GetLoadContext(assembly);
                return $"{identity} @ {loadContext?.Name ?? "<unknown-context>"}";
            })
            .OrderBy(value => value, StringComparer.Ordinal)
            .ToArray();

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

    private static HarmonyRuntimeImageNormalizationSnapshot CreateSyntheticPassthroughRuntimeImage(string path)
    {
        var bytes = File.ReadAllBytes(path);
        if (bytes.Length == 0)
            throw new InvalidDataException("Step 27 internal synthetic-target replay received an empty prepared runtime image.");

        var sha1 = Convert.ToHexString(SHA1.HashData(bytes)).ToLowerInvariant();
        return new HarmonyRuntimeImageNormalizationSnapshot(
            sha1,
            sha1,
            bytes,
            "<internal synthetic target: production HarmonySharedState normalization not applicable>",
            "<byte-identical synthetic passthrough>");
    }

    private static HarmonyRuntimeImageNormalizationSnapshot CreateIosNormalizedHarmonyRuntimeImage(string path)
    {
        var sourceAudit = ReadHarmonyPatchEngineMetadata(path);
        if (!sourceAudit.Allowed)
        {
            throw new InvalidDataException(
                "Step 27 refuses to normalize HarmonySharedState because the source 0Harmony patch-engine fingerprint is no longer the exact audited 2.4.2 shape:\n" +
                sourceAudit.Detail);
        }

        var sourceBytes = File.ReadAllBytes(path);
        if (sourceBytes.Length == 0)
            throw new InvalidDataException("HarmonySharedState normalization source image is empty.");
        var sourceSha1 = Convert.ToHexString(SHA1.HashData(sourceBytes)).ToLowerInvariant();

        using var resolver = new Step27MetadataOnlyResolver(path);
        using var module = ModuleDefinition.ReadModule(new MemoryStream(sourceBytes, writable: false), new ReaderParameters
        {
            InMemory = true,
            ReadSymbols = false,
            // Keep the admission/token-discovery read deferred and resolution-free. The normalizer
            // no longer asks Cecil to write the assembly: Cecil 0.11.6 must resolve enum-typed
            // constants while rebuilding metadata (MetadataBuilder.GetConstantType), which makes a
            // whole-module round-trip incompatible with this fail-closed metadata-only boundary.
            ReadingMode = ReadingMode.Deferred,
            AssemblyResolver = resolver,
            MetadataResolver = resolver,
        });
        if (module.Assembly?.Name is null)
            throw new BadImageFormatException("Managed assembly manifest missing while normalizing HarmonySharedState: " + path);
        if (!string.Equals(module.Assembly.Name.Name, TargetSimpleName, StringComparison.OrdinalIgnoreCase) ||
            (module.Assembly.Name.Version ?? ZeroVersion) != TargetVersion)
        {
            throw new InvalidDataException(
                $"HarmonySharedState normalization is pinned to {TargetSimpleName}, Version={TargetVersion}; observed {module.Assembly.Name.FullName}.");
        }
        if ((module.Attributes & ModuleAttributes.StrongNameSigned) != 0)
        {
            throw new InvalidDataException(
                "HarmonySharedState raw-body normalization refuses a StrongNameSigned source image because in-place IL substitution would invalidate its strong-name signature.");
        }
        if ((module.Attributes & ModuleAttributes.ILOnly) == 0)
            throw new InvalidDataException("HarmonySharedState raw-body normalization requires an IL-only source module.");

        var sharedStateType = EnumerateTypes(module.Types).SingleOrDefault(type => type.FullName.Equals(HarmonySharedStateTypeFullName, StringComparison.Ordinal))
            ?? throw new TypeLoadException("Exact HarmonyLib.HarmonySharedState type is missing from the normalization source image.");
        var cctor = sharedStateType.Methods.SingleOrDefault(method => method.IsConstructor && method.IsStatic && method.HasBody)
            ?? throw new MissingMethodException(HarmonySharedStateTypeFullName, ".cctor()");

        FieldDefinition RequireField(string name, string fieldTypeFullName, bool requireInitOnly)
        {
            var matches = sharedStateType.Fields.Where(field => field.Name.Equals(name, StringComparison.Ordinal)).ToArray();
            if (matches.Length != 1)
                throw new MissingFieldException(HarmonySharedStateTypeFullName, name);
            var field = matches[0];
            if (!field.IsStatic || !field.FieldType.FullName.Equals(fieldTypeFullName, StringComparison.Ordinal) ||
                (requireInitOnly && !field.IsInitOnly))
            {
                throw new InvalidDataException($"HarmonySharedState normalization field shape changed for {name}: {field.FullName}.");
            }
            return field;
        }

        var stateField = RequireField("state", "System.Collections.Generic.Dictionary`2<System.Reflection.MethodBase,System.Byte[]>", requireInitOnly: true);
        var originalsField = RequireField("originals", "System.Collections.Generic.Dictionary`2<System.Reflection.MethodInfo,System.Reflection.MethodBase>", requireInitOnly: true);
        var originalsMonoField = RequireField("originalsMono", "System.Collections.Generic.Dictionary`2<System.Int64,System.Reflection.MethodBase[]>", requireInitOnly: true);
        var methodAddressRefField = sharedStateType.Fields.SingleOrDefault(field => field.Name.Equals("methodAddressRef", StringComparison.Ordinal))
            ?? throw new MissingFieldException(HarmonySharedStateTypeFullName, "methodAddressRef");
        if (!methodAddressRefField.IsStatic || !methodAddressRefField.IsInitOnly ||
            !methodAddressRefField.FieldType.FullName.Contains("HarmonyLib.AccessTools/FieldRef", StringComparison.Ordinal))
        {
            throw new InvalidDataException("HarmonySharedState.methodAddressRef no longer has the exact static readonly AccessTools.FieldRef shape required by the bounded iOS normalization.");
        }
        var actualVersionField = RequireField("actualVersion", "System.Int32", requireInitOnly: true);
        var internalVersionField = sharedStateType.Fields.SingleOrDefault(field => field.Name.Equals("internalVersion", StringComparison.Ordinal))
            ?? throw new MissingFieldException(HarmonySharedStateTypeFullName, "internalVersion");
        if (!internalVersionField.IsLiteral || !internalVersionField.HasConstant || Convert.ToInt32(internalVersionField.Constant) != HarmonySharedStateInternalVersion)
            throw new InvalidDataException($"HarmonySharedState.internalVersion changed from {HarmonySharedStateInternalVersion}.");

        if (cctor.IsPInvokeImpl || cctor.PInvokeInfo is not null)
            throw new InvalidDataException("HarmonySharedState::.cctor unexpectedly became a P/Invoke boundary; refusing normalization.");
        if (cctor.RVA <= 0)
            throw new InvalidDataException("HarmonySharedState::.cctor has no physical method-body RVA; refusing raw-body normalization.");
        if (cctor.Body.ExceptionHandlers.Count != 0)
            throw new InvalidDataException("HarmonySharedState::.cctor unexpectedly contains exception handlers; refusing bounded raw-body normalization.");

        static int RequireFieldToken(FieldDefinition field)
        {
            var token = field.MetadataToken.ToInt32();
            if ((token & unchecked((int)0xFF000000)) != 0x04000000)
                throw new InvalidDataException($"Expected a FieldDef token for {field.FullName}; observed 0x{token:X8}.");
            return token;
        }

        static int RequireExistingParameterlessConstructorToken(MethodDefinition initializer, TypeReference declaringType)
        {
            var matches = initializer.Body.Instructions
                .Where(instruction =>
                    instruction.OpCode.Code == Code.Newobj &&
                    instruction.Operand is MethodReference called &&
                    called.Name.Equals(".ctor", StringComparison.Ordinal) &&
                    called.Parameters.Count == 0 &&
                    called.DeclaringType.FullName.Equals(declaringType.FullName, StringComparison.Ordinal))
                .Select(instruction => ((MethodReference)instruction.Operand).MetadataToken.ToInt32())
                .Distinct()
                .ToArray();
            if (matches.Length != 1)
            {
                throw new InvalidDataException(
                    $"HarmonySharedState::.cctor must contain exactly one existing parameterless constructor token for {declaringType.FullName}; observed {matches.Length}.");
            }

            var token = matches[0];
            if ((token & unchecked((int)0xFF000000)) != 0x0A000000)
                throw new InvalidDataException($"Expected an existing MemberRef constructor token for {declaringType.FullName}; observed 0x{token:X8}.");
            return token;
        }

        var stateCtorToken = RequireExistingParameterlessConstructorToken(cctor, stateField.FieldType);
        var originalsCtorToken = RequireExistingParameterlessConstructorToken(cctor, originalsField.FieldType);
        var originalsMonoCtorToken = RequireExistingParameterlessConstructorToken(cctor, originalsMonoField.FieldType);
        var stateFieldToken = RequireFieldToken(stateField);
        var originalsFieldToken = RequireFieldToken(originalsField);
        var originalsMonoFieldToken = RequireFieldToken(originalsMonoField);
        var methodAddressRefFieldToken = RequireFieldToken(methodAddressRefField);
        var actualVersionFieldToken = RequireFieldToken(actualVersionField);

        var replacementIl = new byte[47];
        var ilOffset = 0;

        void EmitTokenInstruction(byte opcode, int token)
        {
            replacementIl[ilOffset++] = opcode;
            BinaryPrimitives.WriteInt32LittleEndian(replacementIl.AsSpan(ilOffset, 4), token);
            ilOffset += 4;
        }

        EmitTokenInstruction(0x73, stateCtorToken);
        EmitTokenInstruction(0x80, stateFieldToken);
        EmitTokenInstruction(0x73, originalsCtorToken);
        EmitTokenInstruction(0x80, originalsFieldToken);
        EmitTokenInstruction(0x73, originalsMonoCtorToken);
        EmitTokenInstruction(0x80, originalsMonoFieldToken);
        replacementIl[ilOffset++] = 0x14;
        EmitTokenInstruction(0x80, methodAddressRefFieldToken);
        replacementIl[ilOffset++] = 0x20;
        BinaryPrimitives.WriteInt32LittleEndian(replacementIl.AsSpan(ilOffset, 4), HarmonySharedStateInternalVersion);
        ilOffset += 4;
        EmitTokenInstruction(0x80, actualVersionFieldToken);
        replacementIl[ilOffset++] = 0x2A;
        if (ilOffset != replacementIl.Length)
            throw new InvalidOperationException($"Internal Step 27 replacement IL length drift: expected {replacementIl.Length}, wrote {ilOffset}.");

        var runtimeBytes = (byte[])sourceBytes.Clone();
        int methodFileOffset;
        int originalBodyStorageLength;
        using (var peStream = new MemoryStream(sourceBytes, writable: false))
        using (var peReader = new PEReader(peStream))
        {
            if (!peReader.HasMetadata)
                throw new BadImageFormatException("HarmonySharedState normalization source is not a managed PE image.");

            var sectionIndex = peReader.PEHeaders.GetContainingSectionIndex(cctor.RVA);
            if (sectionIndex < 0)
                throw new BadImageFormatException($"HarmonySharedState::.cctor RVA 0x{cctor.RVA:X8} is not contained by any PE section.");
            var section = peReader.PEHeaders.SectionHeaders[sectionIndex];
            methodFileOffset = checked(section.PointerToRawData + (cctor.RVA - section.VirtualAddress));
            if (methodFileOffset < 0 || methodFileOffset > sourceBytes.Length - 12)
                throw new BadImageFormatException($"HarmonySharedState::.cctor file offset 0x{methodFileOffset:X8} is outside the source image.");

            var firstHeaderByte = sourceBytes[methodFileOffset];
            if ((firstHeaderByte & 0x03) != 0x03)
                throw new InvalidDataException($"HarmonySharedState::.cctor must use a fat ECMA-335 method header for bounded in-place normalization; observed format 0x{(firstHeaderByte & 0x03):X2}.");

            var flagsAndSize = BinaryPrimitives.ReadUInt16LittleEndian(sourceBytes.AsSpan(methodFileOffset, 2));
            var headerDwords = (flagsAndSize >> 12) & 0x0F;
            var headerSize = checked(headerDwords * 4);
            if (headerDwords < 3 || headerSize > 60)
                throw new BadImageFormatException($"HarmonySharedState::.cctor fat-header size is invalid: {headerSize} bytes.");
            if (methodFileOffset > sourceBytes.Length - headerSize)
                throw new BadImageFormatException("HarmonySharedState::.cctor fat header extends beyond the source image.");

            var originalCodeSize = BinaryPrimitives.ReadInt32LittleEndian(sourceBytes.AsSpan(methodFileOffset + 4, 4));
            if (originalCodeSize <= 0)
                throw new InvalidDataException($"HarmonySharedState::.cctor original CodeSize is invalid: {originalCodeSize}.");
            originalBodyStorageLength = checked(headerSize + originalCodeSize);
            if (methodFileOffset > sourceBytes.Length - originalBodyStorageLength)
                throw new BadImageFormatException("HarmonySharedState::.cctor declared body extends beyond the source image.");

            var cctorEndRva = checked(cctor.RVA + originalBodyStorageLength);
            var overlappingMethod = EnumerateTypes(module.Types)
                .SelectMany(type => type.Methods)
                .FirstOrDefault(method =>
                    !ReferenceEquals(method, cctor) &&
                    method.RVA > 0 &&
                    method.RVA >= cctor.RVA &&
                    method.RVA < cctorEndRva);
            if (overlappingMethod is not null)
            {
                throw new InvalidDataException(
                    $"HarmonySharedState::.cctor method-body slot overlaps another managed method RVA ({overlappingMethod.FullName} at 0x{overlappingMethod.RVA:X8}); refusing in-place normalization.");
            }

            const ushort CorIlMethodMoreSects = 0x0008;
            if ((flagsAndSize & CorIlMethodMoreSects) != 0)
                throw new InvalidDataException("HarmonySharedState::.cctor unexpectedly carries extra method sections; refusing bounded raw-body normalization.");

            const int replacementHeaderSize = 12;
            if (replacementHeaderSize + replacementIl.Length > originalBodyStorageLength)
            {
                throw new InvalidDataException(
                    $"HarmonySharedState::.cctor original method-body slot is too small for the bounded replacement: available={originalBodyStorageLength}, required={replacementHeaderSize + replacementIl.Length}.");
            }

            Array.Clear(runtimeBytes, methodFileOffset, originalBodyStorageLength);
            const ushort replacementFatFlagsAndSize = 0x3003;
            BinaryPrimitives.WriteUInt16LittleEndian(runtimeBytes.AsSpan(methodFileOffset, 2), replacementFatFlagsAndSize);
            BinaryPrimitives.WriteUInt16LittleEndian(runtimeBytes.AsSpan(methodFileOffset + 2, 2), 1);
            BinaryPrimitives.WriteInt32LittleEndian(runtimeBytes.AsSpan(methodFileOffset + 4, 4), replacementIl.Length);
            BinaryPrimitives.WriteInt32LittleEndian(runtimeBytes.AsSpan(methodFileOffset + 8, 4), 0);
            replacementIl.AsSpan().CopyTo(runtimeBytes.AsSpan(methodFileOffset + replacementHeaderSize, replacementIl.Length));
        }

        if (sourceBytes.AsSpan().SequenceEqual(runtimeBytes))
            throw new InvalidDataException("HarmonySharedState raw-body normalization produced a byte-identical runtime image.");

        for (var index = 0; index < sourceBytes.Length; index++)
        {
            if (index >= methodFileOffset && index < methodFileOffset + originalBodyStorageLength)
                continue;
            if (sourceBytes[index] != runtimeBytes[index])
            {
                throw new InvalidDataException(
                    $"HarmonySharedState raw-body normalization changed bytes outside the admitted .cctor method-body slot at file offset 0x{index:X8}.");
            }
        }

        string normalizedAudit;
        using (var normalizedStream = new MemoryStream(runtimeBytes, writable: false))
        using (var normalizedResolver = new Step27MetadataOnlyResolver(path + "::<normalized-runtime-image>"))
        using (var normalizedModule = ModuleDefinition.ReadModule(normalizedStream, new ReaderParameters
        {
            InMemory = true,
            ReadSymbols = false,
            ReadingMode = ReadingMode.Deferred,
            AssemblyResolver = normalizedResolver,
            MetadataResolver = normalizedResolver,
        }))
        {
            if (normalizedModule.Assembly?.Name.FullName != module.Assembly.Name.FullName)
                throw new InvalidDataException("HarmonySharedState normalization changed the 0Harmony managed assembly identity.");
            var normalizedType = EnumerateTypes(normalizedModule.Types).Single(type => type.FullName.Equals(HarmonySharedStateTypeFullName, StringComparison.Ordinal));
            var normalizedCctor = normalizedType.Methods.Single(method => method.IsConstructor && method.IsStatic && method.HasBody);
            var instructions = normalizedCctor.Body.Instructions;
            if (instructions.Count != 11 ||
                instructions.Count(instruction => instruction.OpCode.Code == Code.Newobj) != 3 ||
                instructions.Count(instruction => instruction.OpCode.Code == Code.Stsfld) != 5 ||
                instructions.Count(instruction => instruction.OpCode.Code == Code.Ldnull) != 1 ||
                instructions.Count(instruction => instruction.OpCode.Code == Code.Ldc_I4) != 1 ||
                instructions.Count(instruction => instruction.OpCode.Code == Code.Ret) != 1 ||
                instructions.Any(instruction => instruction.Operand is MethodReference called &&
                    (called.FullName.Contains("GetOrCreateSharedStateType", StringComparison.Ordinal) ||
                     called.FullName.Contains("FieldRefAccess", StringComparison.Ordinal) ||
                     called.FullName.Contains("ReflectionHelper::Load", StringComparison.Ordinal))))
            {
                throw new InvalidDataException("Normalized HarmonySharedState::.cctor failed its exact 11-instruction iOS-safe fingerprint audit.");
            }
            normalizedAudit = FormatMethodAudit(normalizedCctor);
        }

        var runtimeSha1 = Convert.ToHexString(SHA1.HashData(runtimeBytes)).ToLowerInvariant();
        return new HarmonyRuntimeImageNormalizationSnapshot(
            sourceSha1,
            runtimeSha1,
            runtimeBytes,
            sourceAudit.HarmonySharedStateTypeInitializerAudit,
            normalizedAudit);
    }

    private static HarmonyPatchEngineMetadataSnapshot ReadHarmonyPatchEngineMetadata(string path)
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
            throw new BadImageFormatException("Managed assembly manifest missing while auditing the Harmony patch engine: " + path);

        var allTypes = EnumerateTypes(module.Types).ToArray();
        var sharedStateType = allTypes.SingleOrDefault(type => type.FullName.Equals(HarmonySharedStateTypeFullName, StringComparison.Ordinal));
        var patchFunctionsType = allTypes.SingleOrDefault(type => type.FullName.Equals("HarmonyLib.PatchFunctions", StringComparison.Ordinal));
        var methodCreatorConfigType = allTypes.SingleOrDefault(type => type.FullName.Equals("HarmonyLib.MethodCreatorConfig", StringComparison.Ordinal));
        var methodPatcherToolsType = allTypes.SingleOrDefault(type => type.FullName.Equals("HarmonyLib.MethodPatcherTools", StringComparison.Ordinal));
        var patchToolsType = allTypes.SingleOrDefault(type => type.FullName.Equals("HarmonyLib.PatchTools", StringComparison.Ordinal));
        if (sharedStateType is null || patchFunctionsType is null || methodCreatorConfigType is null || methodPatcherToolsType is null || patchToolsType is null)
        {
            return new HarmonyPatchEngineMetadataSnapshot(
                false,
                "One or more exact Harmony patch-engine internal types are missing.",
                "<shared-state missing>", "<get-or-create missing>", "<prepare missing>", "<update-wrapper missing>", "<detour missing>", "<update-patch-info missing>");
        }
        if (sharedStateType.IsPublic || !sharedStateType.IsAbstract || !sharedStateType.IsSealed)
        {
            return new HarmonyPatchEngineMetadataSnapshot(
                false,
                "HarmonySharedState is no longer the expected internal static type.",
                "<invalid shared-state type>", "<blocked>", "<blocked>", "<blocked>", "<blocked>", "<blocked>");
        }

        var sharedStateCctors = sharedStateType.Methods.Where(method => method.IsConstructor && method.IsStatic).ToArray();
        var sharedStateCctor = sharedStateCctors.SingleOrDefault();
        var getOrCreateSharedStateType = sharedStateType.Methods.SingleOrDefault(method =>
            method.IsStatic && method.Name.Equals("GetOrCreateSharedStateType", StringComparison.Ordinal) && method.Parameters.Count == 0);
        var getPatchInfo = sharedStateType.Methods.SingleOrDefault(method =>
            method.IsStatic && method.Name.Equals("GetPatchInfo", StringComparison.Ordinal) && method.Parameters.Count == 1 &&
            method.Parameters[0].ParameterType.FullName.Equals("System.Reflection.MethodBase", StringComparison.Ordinal));
        var updatePatchInfo = sharedStateType.Methods.SingleOrDefault(method =>
            method.IsStatic && method.Name.Equals("UpdatePatchInfo", StringComparison.Ordinal) && method.Parameters.Count == 3);
        var internalVersionField = sharedStateType.Fields.SingleOrDefault(field => field.Name.Equals("internalVersion", StringComparison.Ordinal));
        var actualVersionField = sharedStateType.Fields.SingleOrDefault(field => field.Name.Equals("actualVersion", StringComparison.Ordinal));

        var updateWrapper = patchFunctionsType.Methods.SingleOrDefault(method =>
            method.IsStatic && method.Name.Equals("UpdateWrapper", StringComparison.Ordinal) && method.Parameters.Count == 2);
        var prepare = methodCreatorConfigType.Methods.SingleOrDefault(method =>
            !method.IsStatic && method.Name.Equals("Prepare", StringComparison.Ordinal) && method.Parameters.Count == 0);
        var createDynamicMethod = methodPatcherToolsType.Methods.SingleOrDefault(method =>
            method.IsStatic && method.Name.Equals("CreateDynamicMethod", StringComparison.Ordinal) && method.Parameters.Count == 3);
        var detourMethod = patchToolsType.Methods.SingleOrDefault(method =>
            method.IsStatic && method.Name.Equals("DetourMethod", StringComparison.Ordinal) && method.Parameters.Count == 2);

        var missing = new List<string>();
        if (sharedStateCctor is null || !sharedStateCctor.HasBody) missing.Add("HarmonySharedState::.cctor");
        if (getOrCreateSharedStateType is null || !getOrCreateSharedStateType.HasBody) missing.Add("HarmonySharedState::GetOrCreateSharedStateType");
        if (getPatchInfo is null || !getPatchInfo.HasBody) missing.Add("HarmonySharedState::GetPatchInfo");
        if (updatePatchInfo is null || !updatePatchInfo.HasBody) missing.Add("HarmonySharedState::UpdatePatchInfo");
        if (updateWrapper is null || !updateWrapper.HasBody) missing.Add("PatchFunctions::UpdateWrapper");
        if (prepare is null || !prepare.HasBody) missing.Add("MethodCreatorConfig::Prepare");
        if (createDynamicMethod is null || !createDynamicMethod.HasBody) missing.Add("MethodPatcherTools::CreateDynamicMethod");
        if (detourMethod is null || !detourMethod.HasBody) missing.Add("PatchTools::DetourMethod");
        if (internalVersionField is null || !internalVersionField.HasConstant || Convert.ToInt32(internalVersionField.Constant) != 102) missing.Add("HarmonySharedState::internalVersion == 102");
        if (actualVersionField is null || !actualVersionField.IsStatic || !actualVersionField.FieldType.FullName.Equals("System.Int32", StringComparison.Ordinal)) missing.Add("HarmonySharedState::actualVersion static Int32");
        if (missing.Count != 0)
        {
            return new HarmonyPatchEngineMetadataSnapshot(
                false,
                "Required exact Harmony patch-engine members are missing or changed: " + string.Join(" | ", missing),
                sharedStateCctor?.HasBody == true ? FormatMethodAudit(sharedStateCctor) : "<missing>",
                getOrCreateSharedStateType?.HasBody == true ? FormatMethodAudit(getOrCreateSharedStateType) : "<missing>",
                prepare?.HasBody == true ? FormatMethodAudit(prepare) : "<missing>",
                updateWrapper?.HasBody == true ? FormatMethodAudit(updateWrapper) : "<missing>",
                detourMethod?.HasBody == true ? FormatMethodAudit(detourMethod) : "<missing>",
                updatePatchInfo?.HasBody == true ? FormatMethodAudit(updatePatchInfo) : "<missing>");
        }

        static string[] CalledMembers(MethodDefinition method) => method.Body.Instructions
            .Where(instruction => instruction.Operand is MethodReference)
            .Select(instruction => ((MethodReference)instruction.Operand).FullName)
            .ToArray();
        static bool Calls(string[] calls, string fragment) => calls.Any(value => value.Contains(fragment, StringComparison.Ordinal));

        var cctorCalls = CalledMembers(sharedStateCctor!);
        var getOrCreateCalls = CalledMembers(getOrCreateSharedStateType!);
        var prepareCalls = CalledMembers(prepare!);
        var updateWrapperCalls = CalledMembers(updateWrapper!);
        var detourCalls = CalledMembers(detourMethod!);
        var updatePatchInfoCalls = CalledMembers(updatePatchInfo!);

        var hazards = new List<string>();
        if (!Calls(cctorCalls, "HarmonyLib.HarmonySharedState::GetOrCreateSharedStateType"))
            hazards.Add("HarmonySharedState::.cctor no longer enters GetOrCreateSharedStateType.");
        if (!Calls(cctorCalls, "HarmonyLib.AccessTools::get_IsMonoRuntime") ||
            !Calls(cctorCalls, "HarmonyLib.AccessTools::Field") ||
            !Calls(cctorCalls, "HarmonyLib.AccessTools::FieldRefAccess"))
            hazards.Add("HarmonySharedState::.cctor no longer exposes the measured Mono StackFrame/FieldRefAccess branch.");
        if (!Calls(getOrCreateCalls, "System.Type::GetType") ||
            !Calls(getOrCreateCalls, "Mono.Cecil.ModuleDefinition::CreateModule") ||
            !Calls(getOrCreateCalls, "MonoMod.Utils.ReflectionHelper::Load"))
            hazards.Add("GetOrCreateSharedStateType no longer exposes Type.GetType -> Cecil CreateModule -> ReflectionHelper.Load dynamic-singleton flow.");
        if (!Calls(prepareCalls, "HarmonyLib.HarmonySharedState::GetPatchInfo") ||
            !Calls(prepareCalls, "HarmonyLib.MethodPatcherTools::CreateDynamicMethod") ||
            !Calls(prepareCalls, "MonoMod.Utils.DynamicMethodDefinition::GetILGenerator"))
            hazards.Add("MethodCreatorConfig.Prepare no longer exposes shared-state lookup -> DynamicMethodDefinition creation -> ILGenerator flow.");
        if (!Calls(updateWrapperCalls, "HarmonyLib.MethodCreator::CreateReplacement") ||
            !Calls(updateWrapperCalls, "HarmonyLib.PatchTools::DetourMethod"))
            hazards.Add("PatchFunctions.UpdateWrapper no longer exposes CreateReplacement -> DetourMethod flow.");
        if (!Calls(detourCalls, "MonoMod.Core.DetourFactory::get_Current") || !Calls(detourCalls, "CreateDetour"))
            hazards.Add("PatchTools.DetourMethod no longer exposes DetourFactory.Current.CreateDetour.");
        if (!Calls(updatePatchInfoCalls, "System.RuntimeMethodHandle::GetFunctionPointer"))
            hazards.Add("HarmonySharedState.UpdatePatchInfo no longer exposes the Mono replacement MethodHandle.GetFunctionPointer path.");

        foreach (var method in new[] { sharedStateCctor!, getOrCreateSharedStateType!, getPatchInfo!, updatePatchInfo!, updateWrapper!, prepare!, createDynamicMethod!, detourMethod! })
        {
            if (method.IsPInvokeImpl || method.PInvokeInfo is not null)
                hazards.Add("P/Invoke in exact patch-engine audit member: " + method.FullName);
        }

        var detail =
            "HarmonySharedState internalVersion: 102 — MATCH\n" +
            "HarmonySharedState explicit type-initialization boundary: PRESENT\n" +
            "Shared-state singleton path: Type.GetType -> Cecil ModuleDefinition.CreateModule -> ReflectionHelper.Load — PRESENT\n" +
            "Mono StackFrame methodAddress FieldRefAccess dynamic-code branch: PRESENT\n" +
            "MethodCreatorConfig.Prepare: GetPatchInfo -> CreateDynamicMethod -> GetILGenerator — PRESENT\n" +
            "PatchFunctions.UpdateWrapper: CreateReplacement -> PatchTools.DetourMethod — PRESENT\n" +
            "PatchTools.DetourMethod: DetourFactory.Current.CreateDetour — PRESENT\n" +
            "HarmonySharedState.UpdatePatchInfo: MethodHandle.GetFunctionPointer Mono path — PRESENT\n" +
            $"Blocking patch-engine metadata hazards: {hazards.Count:N0}" +
            (hazards.Count == 0 ? string.Empty : "\n" + string.Join("\n", hazards));

        return new HarmonyPatchEngineMetadataSnapshot(
            hazards.Count == 0,
            detail,
            FormatMethodAudit(sharedStateCctor!),
            FormatMethodAudit(getOrCreateSharedStateType!),
            FormatMethodAudit(prepare!),
            FormatMethodAudit(updateWrapper!),
            FormatMethodAudit(detourMethod!),
            FormatMethodAudit(updatePatchInfo!));
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
            return new HarmonyPatchMetadataSnapshot(false, "PatchProcessor or HarmonyMethod type missing from exact 0Harmony.", "<missing>", "<missing>", "<missing>", "<missing>", "<missing>");
        if (!processorType.IsPublic || processorType.IsAbstract || processorType.IsInterface ||
            !harmonyMethodType.IsPublic || harmonyMethodType.IsAbstract || harmonyMethodType.IsInterface)
            return new HarmonyPatchMetadataSnapshot(false, "PatchProcessor/HarmonyMethod runtime type shape changed.", "<invalid type>", "<invalid type>", "<invalid type>", "<invalid type>", "<invalid type>");

        var harmonyMethodTypeInitializers = harmonyMethodType.Methods.Where(method => method.IsConstructor && method.IsStatic).ToArray();
        if (harmonyMethodTypeInitializers.Length != 0)
            return new HarmonyPatchMetadataSnapshot(false, $"Step 27 requires HarmonyMethod to have no type initializer; observed {harmonyMethodTypeInitializers.Length}.", "<blocked>", "<blocked>", "<blocked>", "<blocked>", "<blocked>");

        var prefixField = processorType.Fields.SingleOrDefault(field => !field.IsStatic && field.Name.Equals("prefix", StringComparison.Ordinal));
        var harmonyMethodMethodField = harmonyMethodType.Fields.SingleOrDefault(field => !field.IsStatic && field.IsPublic && field.Name.Equals("method", StringComparison.Ordinal));
        if (prefixField is null || !prefixField.FieldType.FullName.Equals("HarmonyLib.HarmonyMethod", StringComparison.Ordinal) ||
            harmonyMethodMethodField is null || !harmonyMethodMethodField.FieldType.FullName.Equals("System.Reflection.MethodInfo", StringComparison.Ordinal))
            return new HarmonyPatchMetadataSnapshot(false, "PatchProcessor.prefix or HarmonyMethod.method field shape changed.", "<blocked>", "<blocked>", "<blocked>", "<blocked>", "<blocked>");

        var harmonyMethodConstructors = harmonyMethodType.Methods.Where(method => method.IsConstructor && !method.IsStatic && method.IsPublic).ToArray();
        var harmonyMethodDefaultConstructor = harmonyMethodConstructors.SingleOrDefault(method => method.Parameters.Count == 0);
        var harmonyMethodConstructor = harmonyMethodConstructors.SingleOrDefault(method =>
            method.Parameters.Count == 1 && method.Parameters[0].ParameterType.FullName.Equals("System.Reflection.MethodInfo", StringComparison.Ordinal));
        if (harmonyMethodDefaultConstructor is null || !harmonyMethodDefaultConstructor.HasBody)
            return new HarmonyPatchMetadataSnapshot(false, "Exact public HarmonyMethod() constructor is missing or bodyless.", "<blocked>", "<blocked>", "<blocked>", harmonyMethodDefaultConstructor?.FullName ?? "<missing>", harmonyMethodConstructor?.FullName ?? "<missing>");
        if (harmonyMethodConstructor is null || !harmonyMethodConstructor.HasBody)
            return new HarmonyPatchMetadataSnapshot(false, "Exact public HarmonyMethod(MethodInfo) constructor is missing or bodyless.", "<blocked>", "<blocked>", "<blocked>", FormatMethodAudit(harmonyMethodDefaultConstructor), harmonyMethodConstructor?.FullName ?? "<missing>");

        var addPrefixCandidates = processorType.Methods.Where(method => method.IsPublic && !method.IsStatic && method.Name.Equals("AddPrefix", StringComparison.Ordinal)).ToArray();
        var addPrefix = addPrefixCandidates.SingleOrDefault(method =>
            method.Parameters.Count == 1 &&
            method.Parameters[0].ParameterType.FullName.Equals("System.Reflection.MethodInfo", StringComparison.Ordinal) &&
            method.ReturnType.FullName.Equals(PatchProcessorTypeFullName, StringComparison.Ordinal));
        if (addPrefix is null || !addPrefix.HasBody)
            return new HarmonyPatchMetadataSnapshot(false, "Exact PatchProcessor.AddPrefix(MethodInfo) is missing or bodyless.", addPrefix?.FullName ?? "<missing>", "<blocked>", "<blocked>", FormatMethodAudit(harmonyMethodDefaultConstructor), FormatMethodAudit(harmonyMethodConstructor));

        var patchCandidates = processorType.Methods.Where(method => method.IsPublic && !method.IsStatic && method.Name.Equals("Patch", StringComparison.Ordinal)).ToArray();
        var patch = patchCandidates.SingleOrDefault(method => method.Parameters.Count == 0 && method.ReturnType.FullName.Equals("System.Reflection.MethodInfo", StringComparison.Ordinal));
        if (patch is null || !patch.HasBody)
            return new HarmonyPatchMetadataSnapshot(false, "Exact parameterless PatchProcessor.Patch() -> MethodInfo is missing or bodyless.", FormatMethodAudit(addPrefix), patch?.FullName ?? "<missing>", "<blocked>", FormatMethodAudit(harmonyMethodDefaultConstructor), FormatMethodAudit(harmonyMethodConstructor));

        var unpatchCandidates = processorType.Methods.Where(method => method.IsPublic && !method.IsStatic && method.Name.Equals("Unpatch", StringComparison.Ordinal)).ToArray();
        var unpatch = unpatchCandidates.SingleOrDefault(method =>
            method.Parameters.Count == 1 &&
            method.Parameters[0].ParameterType.FullName.Equals("System.Reflection.MethodInfo", StringComparison.Ordinal) &&
            method.ReturnType.FullName.Equals(PatchProcessorTypeFullName, StringComparison.Ordinal));
        if (unpatch is null || !unpatch.HasBody)
            return new HarmonyPatchMetadataSnapshot(false, "Exact PatchProcessor.Unpatch(MethodInfo) is missing or bodyless.", FormatMethodAudit(addPrefix), FormatMethodAudit(patch), unpatch?.FullName ?? "<missing>", FormatMethodAudit(harmonyMethodDefaultConstructor), FormatMethodAudit(harmonyMethodConstructor));

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

        var defaultCtorIl = harmonyMethodDefaultConstructor.Body.Instructions;
        var defaultCtorShape = defaultCtorIl.Count == 6 &&
            defaultCtorIl[0].OpCode.Code == Code.Ldarg_0 &&
            defaultCtorIl[1].OpCode.Code == Code.Ldc_I4_M1 &&
            defaultCtorIl[2].OpCode.Code == Code.Stfld && defaultCtorIl[2].Operand is FieldReference priorityStore && priorityStore.Name.Equals("priority", StringComparison.Ordinal) && priorityStore.FieldType.FullName.Equals("System.Int32", StringComparison.Ordinal) &&
            defaultCtorIl[3].OpCode.Code == Code.Ldarg_0 &&
            defaultCtorIl[4].OpCode.Code == Code.Call && defaultCtorIl[4].Operand is MethodReference objectCtorCall && objectCtorCall.DeclaringType.FullName.Equals("System.Object", StringComparison.Ordinal) && objectCtorCall.Name.Equals(".ctor", StringComparison.Ordinal) &&
            defaultCtorIl[5].OpCode.Code == Code.Ret;

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
        if (!defaultCtorShape) hazards.Add("HarmonyMethod() no longer matches exact priority=-1 -> object::.ctor -> ret shape required by the bounded iOS descriptor path.");
        if (!addPrefixShape) hazards.Add("AddPrefix(MethodInfo) no longer matches new HarmonyMethod(fixMethod) -> prefix -> return this.");
        if (!patchShape) hazards.Add("Patch() no longer exposes the measured GetPatchInfo/AddPrefixes/UpdateWrapper/UpdatePatchInfo flow.");
        if (!unpatchShape) hazards.Add("Unpatch(MethodInfo) no longer exposes the measured GetPatchInfo/RemovePatch/UpdateWrapper/UpdatePatchInfo flow.");
        if (!harmonyMethodCtorShape) hazards.Add("HarmonyMethod(MethodInfo) no longer calls its measured ImportMethod path.");
        foreach (var method in new[] { addPrefix, patch, unpatch, harmonyMethodDefaultConstructor, harmonyMethodConstructor })
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
            "HarmonyMethod iOS-safe descriptor constructor: exact public .ctor() with priority=-1 only\n" +
            "HarmonyMethod(MethodInfo) remains metadata-audited reference behavior and calls ImportMethod\n" +
            "HarmonyMethod retained field: method:System.Reflection.MethodInfo\n" +
            $"Blocking patch metadata hazards: {hazards.Count:N0}" +
            (hazards.Count == 0 ? string.Empty : "\n" + string.Join("\n", hazards));
        return new HarmonyPatchMetadataSnapshot(
            hazards.Count == 0,
            detail,
            FormatMethodAudit(addPrefix),
            FormatMethodAudit(patch),
            FormatMethodAudit(unpatch),
            FormatMethodAudit(harmonyMethodDefaultConstructor), FormatMethodAudit(harmonyMethodConstructor));
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

    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Step 27.0.24 accesses only exact reflected members of the Cecil-audited launcher-owned post-publish fixture.")]
    private static (int TargetCalls, int PrefixCalls) ReadPatchProbeCounters(LauncherPatchProbeSnapshot probe)
    {
        var targetValue = probe.TargetCallsField.GetValue(null);
        var prefixValue = probe.PrefixCallsField.GetValue(null);
        if (targetValue is not int targetCalls || prefixValue is not int prefixCalls)
            throw new InvalidDataException("Step 27.0.24 interpreted patch fixture counter fields did not return Int32 values.");
        return (targetCalls, prefixCalls);
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Step 27.0.24 invokes only exact reflected members of the Cecil-audited launcher-owned post-publish fixture.")]
    private static void ResetPatchProbeCounters(LauncherPatchProbeSnapshot probe)
    {
        try
        {
            _ = probe.ResetCounters.Invoke(null, null);
        }
        catch (TargetInvocationException ex) when (ex.InnerException is not null)
        {
            throw new InvalidOperationException("Step 27.0.24 interpreted patch fixture ResetCounters() threw.", ex.InnerException);
        }
        var counters = ReadPatchProbeCounters(probe);
        if (counters.TargetCalls != 0 || counters.PrefixCalls != 0)
            throw new InvalidDataException($"Step 27.0.24 interpreted patch fixture counters did not reset: target={counters.TargetCalls}, prefix={counters.PrefixCalls}.");
    }

    private (int TargetCalls, int PrefixCalls) ReadPatchProbeCounters()
        => ReadPatchProbeCounters(RequirePatchProbe());

    private void RequirePatchProbeCounters(int expectedTargetCalls, int expectedPrefixCalls, string operation)
    {
        var counters = ReadPatchProbeCounters();
        if (counters.TargetCalls != expectedTargetCalls || counters.PrefixCalls != expectedPrefixCalls)
        {
            throw new InvalidDataException(
                $"{operation}: expected target={expectedTargetCalls}, prefix={expectedPrefixCalls}; observed target={counters.TargetCalls}, prefix={counters.PrefixCalls}.");
        }
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Step 27.0.24 invokes only exact reflected Int32 methods of the Cecil-audited launcher-owned post-publish fixture.")]
    private static int InvokePatchProbeInt32(MethodInfo method, int value, string operation)
    {
        try
        {
            var raw = method.Invoke(null, [value]);
            return raw is int result
                ? result
                : throw new InvalidDataException($"{operation} did not return System.Int32.");
        }
        catch (TargetInvocationException ex) when (ex.InnerException is not null)
        {
            throw new InvalidOperationException($"{operation} threw.", ex.InnerException);
        }
    }

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

    private static void ReportProgress(
        IProgress<ControlledHarmonyPatchExecutionProgress>? progress,
        ControlledHarmonyPatchExecutionGate gate,
        string detail)
        => progress?.Report(new ControlledHarmonyPatchExecutionProgress(gate, 0, 0, null, detail));

    private static ControlledHarmonyPatchExecutionGateResult Pass(ControlledHarmonyPatchExecutionGate gate, string detail)
        => new(gate, true, detail);

    private static ControlledHarmonyPatchExecutionGateResult Fail(ControlledHarmonyPatchExecutionGate gate, string stage, Exception ex)
        => new(gate, false, $"Stage: {stage}\n{ex}");

    private sealed class Step27MetadataOnlyResolver(string auditedPath) : IAssemblyResolver, IMetadataResolver
    {
        public AssemblyDefinition Resolve(AssemblyNameReference name)
            => throw new InvalidOperationException(
                $"Step 27 Gate A metadata-only audit attempted forbidden external assembly resolution while reading '{auditedPath}': {name.FullName}");

        public AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
            => Resolve(name);

        TypeDefinition IMetadataResolver.Resolve(TypeReference type)
            => throw new InvalidOperationException(
                $"Step 27 Gate A metadata-only audit attempted forbidden type resolution while reading '{auditedPath}': {type.FullName}");

        FieldDefinition IMetadataResolver.Resolve(FieldReference field)
            => throw new InvalidOperationException(
                $"Step 27 Gate A metadata-only audit attempted forbidden field resolution while reading '{auditedPath}': {field.FullName}");

        MethodDefinition IMetadataResolver.Resolve(MethodReference method)
            => throw new InvalidOperationException(
                $"Step 27 Gate A metadata-only audit attempted forbidden method resolution while reading '{auditedPath}': {method.FullName}");

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

    private sealed record HarmonyPatchEngineMetadataSnapshot(
        bool Allowed,
        string Detail,
        string HarmonySharedStateTypeInitializerAudit,
        string GetOrCreateSharedStateTypeAudit,
        string MethodCreatorPrepareAudit,
        string UpdateWrapperAudit,
        string DetourMethodAudit,
        string UpdatePatchInfoAudit);

    private sealed record HarmonyPatchMetadataSnapshot(
        bool Allowed,
        string Detail,
        string AddPrefixAudit,
        string PatchAudit,
        string UnpatchAudit,
        string HarmonyMethodDefaultConstructorAudit,
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
        HarmonyRuntimeImageNormalizationSnapshot HarmonyRuntimeImage,
        SteamOfflineInstallResult Offline);

    private sealed record HarmonyRuntimeImageNormalizationSnapshot(
        string SourcePreparedSha1,
        string RuntimeImageSha1,
        byte[] RuntimeImageBytes,
        string OriginalTypeInitializerAudit,
        string NormalizedTypeInitializerAudit);

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
        ConstructorInfo HarmonyMethodDefaultConstructor,
        ConstructorInfo HarmonyMethodConstructor,
        FieldInfo HarmonyMethodMethodField,
        FieldInfo HarmonyMethodPriorityField,
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
        PropertyInfo RuntimeFrameworkDescriptionProperty,
        string AccessToolsTypeInitializerAudit,
        string PatchEngineMetadataDetail,
        string HarmonySharedStateTypeInitializerAudit,
        string HarmonySharedStateGetOrCreateAudit,
        string MethodCreatorPrepareAudit,
        string UpdateWrapperAudit,
        string DetourMethodAudit,
        string UpdatePatchInfoAudit,
        string AddPrefixAudit,
        string PatchAudit,
        string UnpatchAudit,
        string HarmonyMethodDefaultConstructorAudit,
        string HarmonyMethodConstructorAudit);

    private sealed record LauncherPatchProbeSnapshot(
        Assembly FixtureAssembly,
        string FixturePath,
        string FixtureSha256,
        MethodInfo Target,
        MethodInfo InvokeTarget,
        MethodInfo Prefix,
        MethodInfo ResetCounters,
        FieldInfo TargetCallsField,
        FieldInfo PrefixCallsField,
        string TargetSignature,
        string InvokeTargetSignature,
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

    private sealed record HarmonySharedStateInitializationSnapshot(
        string PreparedSha1,
        int ActualVersion,
        int ManagedResolverRequests,
        int PrivateLoads,
        int HostLoads,
        int NativeLoadAttempts,
        string[] PrivateContextMembership,
        string[] KnownGeneratedAssemblies);

    private sealed record PatchExecutionSnapshot(
        string PreparedSha1,
        string ReplacementName,
        string ReplacementDeclaringType,
        int ManagedResolverRequests,
        int PrivateLoads,
        int HostLoads,
        int NativeLoadAttempts,
        string[] PrivateContextMembership,
        string[] KnownGeneratedAssemblies);

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
        private readonly string _normalizedHarmonyAssemblyFullName;
        private readonly HarmonyRuntimeImageNormalizationSnapshot _harmonyRuntimeImage;

        public Step27LoadContext(
            string name,
            RuntimeFrameworkBindingPlanDocument plan,
            IReadOnlyList<PreparedAssemblySnapshot> preparedAssemblies,
            string normalizedHarmonyAssemblyFullName,
            HarmonyRuntimeImageNormalizationSnapshot harmonyRuntimeImage,
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
            _normalizedHarmonyAssemblyFullName = normalizedHarmonyAssemblyFullName;
            _harmonyRuntimeImage = harmonyRuntimeImage;
        }

        public string? AllowedInitializerAssemblyFullName { get; set; }
        public Action<string>? DiagnosticObserver { get; set; }
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
            Stream stream;
            if (prepared.Plan.AssemblyFullName.Equals(_normalizedHarmonyAssemblyFullName, StringComparison.Ordinal))
            {
                if (!_harmonyRuntimeImage.SourcePreparedSha1.Equals(hash, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException("Step 27 normalized Harmony runtime image source SHA-1 drifted immediately before load.");
                var runtimeHash = Convert.ToHexString(SHA1.HashData(_harmonyRuntimeImage.RuntimeImageBytes)).ToLowerInvariant();
                if (!runtimeHash.Equals(_harmonyRuntimeImage.RuntimeImageSha1, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException("Step 27 normalized Harmony runtime image bytes changed after Gate A.");
                stream = new MemoryStream(_harmonyRuntimeImage.RuntimeImageBytes, writable: false);
                DiagnosticObserver?.Invoke("loading bounded iOS-normalized Harmony runtime image: source=" + hash + "; runtime=" + runtimeHash);
            }
            else
            {
                stream = new FileStream(prepared.PreparedPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            using (stream)
            {
                var loaded = LoadFromStream(stream);
                var privateLoadDetail = $"{explicitReason}: {prepared.Plan.AssemblyFullName} => {loaded.GetName().FullName}";
                PrivateLoads.Add(privateLoadDetail);
                DiagnosticObserver?.Invoke("private load completed: " + privateLoadDetail);
                return loaded;
            }
        }

        [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Step 27.0.24 loads only the exact-hash launcher-owned post-publish interpreted patch fixture admitted by Gate P.")]
        public Assembly LoadVerifiedInterpretedFixture(byte[] bytes, string expectedFullName, string explicitReason)
        {
            if (bytes is null || bytes.Length == 0)
                throw new InvalidDataException("Step 27 interpreted patch fixture bytes are empty.");
            using var stream = new MemoryStream(bytes, writable: false);
            var loaded = LoadFromStream(stream);
            var actualFullName = loaded.GetName().FullName ?? loaded.FullName ?? string.Empty;
            if (!actualFullName.Equals(expectedFullName, StringComparison.Ordinal))
                throw new InvalidDataException($"Step 27 interpreted patch fixture identity changed during load: {actualFullName} != {expectedFullName}");
            var privateLoadDetail = $"{explicitReason}: {expectedFullName} => {actualFullName}";
            PrivateLoads.Add(privateLoadDetail);
            DiagnosticObserver?.Invoke("private interpreted fixture load completed: " + privateLoadDetail);
            return loaded;
        }

        public Assembly ResolvePlanned(AssemblyName assemblyName)
            => Load(assemblyName) ?? throw new FileLoadException("Step 24 planned resolver returned null for " + (assemblyName.FullName ?? assemblyName.Name));

        [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Step 24 resolves only exact receipt-verified prepared assemblies and exact persisted host bindings.")]
        protected override Assembly? Load(AssemblyName assemblyName)
        {
            var requestedFullName = assemblyName.FullName ?? assemblyName.Name ?? "<unknown>";
            ManagedResolverRequests.Add(requestedFullName);
            DiagnosticObserver?.Invoke("managed resolver request: " + requestedFullName);

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

            var hostLoadDetail = $"{requestedFullName} => {actualFullName}";
            HostLoads.Add(hostLoadDetail);
            DiagnosticObserver?.Invoke("host load completed: " + hostLoadDetail);
            return hostAssembly;
        }

        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            NativeLoadAttempts.Add(unmanagedDllName);
            DiagnosticObserver?.Invoke("native resolver request (will be rejected): " + unmanagedDllName);
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
