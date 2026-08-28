using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.Loader;
using System.Security.Cryptography;
using System.Text.Json;
using Mono.Cecil;

namespace StS2Launcher.Core;

/// <summary>
/// Step 35 boundary. Re-manufactures/reverifies the physically closed Step-32 transformed image,
/// re-establishes the Step-33 transformed-primary admission contract in a fresh execution-capable
/// private AssemblyLoadContext, then reflects and invokes exactly transformed
/// MegaCrit.Sts2.Core.Helpers.OneTimeInitialization::ExecuteVeryEarly() once and awaits its exact Task to completion. Resolver authority remains
/// limited to the persisted Step-21/22 host bindings and hash-pinned initializer-free prepared private
/// dependencies. Initializer-bearing private dependencies (including 0Harmony), unplanned managed
/// requests, native resolution, entry-point execution, Godot startup, and broader game initialization
/// remain forbidden and fail closed.
/// </summary>
public sealed class TransformedRealStS2VeryEarlyInitialization : IDisposable
{
    public const string LoadContextName = "StS2Launcher-Step35-VeryEarly";
    public const string TargetTypeFullName = "MegaCrit.Sts2.Core.Helpers.OneTimeInitialization";
    public const string TargetMethodName = "ExecuteVeryEarly";
    public const string TargetMethodFullName = "System.Threading.Tasks.Task MegaCrit.Sts2.Core.Helpers.OneTimeInitialization::ExecuteVeryEarly()";
    public const uint SourceTargetMethodToken = 0x06007D02;
    public const long ClosedSourceBytes = 9_363_456;
    public const string TargetStateMachineTypeName = "<ExecuteVeryEarly>d__7";
    public const uint SourceStateMachineMoveNextToken = 0x0600BC71;

    private readonly string _launcherDataRoot;
    private readonly string _preparedWorkRoot;
    private readonly string _preparedRoot;
    private readonly string _planPath;
    private readonly RealStS2PrepareMethodRewrite _rewrite;
    private readonly FirstRealGameAssemblyLoad _preparedPreflight;
    private readonly SteamOfflineInstallInspection _offlineInspection;
    private readonly bool _collectibleLoadContext;

    private ExecutionPreflightSnapshot? _preflight;
    private PrimaryAdmissionSnapshot? _admission;
    private ExecutionSnapshot? _execution;
    private Step35ExecutionLoadContext? _loadContext;
    private bool _disposed;

    public TransformedRealStS2VeryEarlyInitialization(string launcherDataRoot, bool collectibleLoadContext = false)
    {
        if (string.IsNullOrWhiteSpace(launcherDataRoot))
            throw new ArgumentException("Launcher data root is required.", nameof(launcherDataRoot));

        _launcherDataRoot = Path.GetFullPath(launcherDataRoot);
        _preparedWorkRoot = Path.Combine(_launcherDataRoot, PreparedRuntimeFrameworkBinding.WorkRootName);
        _preparedRoot = Path.Combine(_preparedWorkRoot, PreparedRuntimeFrameworkBinding.PreparedRootName);
        _planPath = Path.Combine(
            _preparedWorkRoot,
            PreparedRuntimeFrameworkBinding.PlanRootName,
            PreparedRuntimeFrameworkBinding.PlanFileName);
        _rewrite = new RealStS2PrepareMethodRewrite(_launcherDataRoot);
        _preparedPreflight = new FirstRealGameAssemblyLoad(_launcherDataRoot, collectibleLoadContext: true);
        _offlineInspection = new SteamOfflineInstallInspection(_launcherDataRoot);
        _collectibleLoadContext = collectibleLoadContext;
    }

    public void Reset()
    {
        ThrowIfDisposed();
        ReleaseLoadContext();
        _rewrite.Reset();
        _preparedPreflight.Reset();
        _preflight = null;
        _admission = null;
        _execution = null;
    }

    public async Task<TransformedRealStS2VeryEarlyInitializationGateResult> RunVerifiedExecutionPreflightAsync(
        IProgress<TransformedRealStS2VeryEarlyInitializationProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        const TransformedRealStS2VeryEarlyInitializationGate gate = TransformedRealStS2VeryEarlyInitializationGate.VerifiedExecutionPreflight;
        var stage = "initialization";
        try
        {
            Reset();
            EnsureNoStS2Loaded("Gate A entry");
            cancellationToken.ThrowIfCancellationRequested();

            stage = "closed Step-32 transformation requalification";
            progress?.Report(new(gate, 0, 7, null,
                "Re-running the physically closed Step-32 A-D transform contract. No StS2 CLR admission or game invocation occurs in Gate A."));
            RequireRewritePass("Step 32 Gate A", await _rewrite.RunSourceAdmissionAndPrivateCloneAsync(cancellationToken: cancellationToken).ConfigureAwait(false));
            progress?.Report(new(gate, 1, 7, null, "Step-32 Gate A requalified."));
            RequireRewritePass("Step 32 Gate B", _rewrite.RunDeterministicStackNeutralRewrite());
            progress?.Report(new(gate, 2, 7, null, "Step-32 Gate B requalified; exact transformed bytes manufactured privately."));
            RequireRewritePass("Step 32 Gate C", _rewrite.RunTransformedImageVerification());
            progress?.Report(new(gate, 3, 7, null, "Step-32 Gate C requalified; transformed semantics independently reopened and verified."));
            RequireRewritePass("Step 32 Gate D", await _rewrite.RunFinalIsolationAuditAsync(cancellationToken: cancellationToken).ConfigureAwait(false));
            progress?.Report(new(gate, 4, 7, null, "Step-32 Gate D requalified; trusted source remains isolated."));

            stage = "physical Step-32 transformed artifact and very-early startup target identity";
            var sourcePath = Path.Combine(
                _launcherDataRoot,
                RealStS2PrepareMethodRewrite.WorkRootName,
                RealStS2PrepareMethodRewrite.SourceRootName,
                RealStS2PrepareMethodRewrite.PrimaryFileName);
            var transformedPath = Path.Combine(
                _launcherDataRoot,
                RealStS2PrepareMethodRewrite.WorkRootName,
                RealStS2PrepareMethodRewrite.TransformedRootName,
                RealStS2PrepareMethodRewrite.PrimaryFileName);

            VerifyFileLength(sourcePath, ClosedSourceBytes, "private source primary");
            var sourceSha256 = ComputeSha256Hex(sourcePath);
            if (!sourceSha256.Equals(TransformedRealStS2AssemblyAdmission.ClosedStep32SourceSha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException($"Step 35 requires the exact physically closed source SHA-256 {TransformedRealStS2AssemblyAdmission.ClosedStep32SourceSha256}; observed {sourceSha256}.");
            VerifyFileLength(transformedPath, TransformedRealStS2AssemblyAdmission.ClosedStep32TransformedBytes, "transformed primary");
            var transformedSha256 = ComputeSha256Hex(transformedPath);
            if (!transformedSha256.Equals(TransformedRealStS2AssemblyAdmission.ClosedStep32TransformedSha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException($"Step 35 requires the exact physically closed Step-32 transformed SHA-256 {TransformedRealStS2AssemblyAdmission.ClosedStep32TransformedSha256}; observed {transformedSha256}.");

            uint transformedMethodToken;
            uint transformedMoveNextToken;
            string targetSemanticSha256;
            string moveNextSemanticSha256;
            using (var sourceResolver = new RejectingAssemblyResolver())
            using (var transformedResolver = new RejectingAssemblyResolver())
            using (var sourceModule = ModuleDefinition.ReadModule(sourcePath, new ReaderParameters
                   {
                       ReadSymbols = false,
                       ReadingMode = ReadingMode.Deferred,
                       AssemblyResolver = sourceResolver,
                   }))
            using (var transformedModule = ModuleDefinition.ReadModule(transformedPath, new ReaderParameters
                   {
                       ReadSymbols = false,
                       ReadingMode = ReadingMode.Deferred,
                       AssemblyResolver = transformedResolver,
                   }))
            {
                if (sourceModule.Assembly?.Name.FullName != TransformedRealStS2AssemblyAdmission.ClosedStep32AssemblyIdentity ||
                    sourceModule.Mvid != TransformedRealStS2AssemblyAdmission.ClosedStep32Mvid ||
                    transformedModule.Assembly?.Name.FullName != TransformedRealStS2AssemblyAdmission.ClosedStep32AssemblyIdentity ||
                    transformedModule.Mvid != TransformedRealStS2AssemblyAdmission.ClosedStep32Mvid)
                {
                    throw new InvalidDataException("Step-35 source/transformed image identity or MVID drifted from the closed Step-32/34 evidence.");
                }

                var sourceMethod = FindMethodByToken(sourceModule, SourceTargetMethodToken);
                if (sourceMethod.DeclaringType.FullName != TargetTypeFullName || sourceMethod.FullName != TargetMethodFullName)
                    throw new InvalidDataException($"Step-35 source token 0x{SourceTargetMethodToken:X8} no longer identifies exact {TargetMethodFullName}.");
                RequireVeryEarlySignature(sourceMethod, "source");
                var transformedMethod = RealStS2PrepareMethodRewrite.FindMethodByStableIdentity(transformedModule, TargetTypeFullName, TargetMethodFullName);
                RequireVeryEarlySignature(transformedMethod, "transformed");

                targetSemanticSha256 = RealStS2PrepareMethodRewrite.ComputeMethodSemanticFingerprint(sourceMethod);
                var transformedTargetSemantic = RealStS2PrepareMethodRewrite.ComputeMethodSemanticFingerprint(transformedMethod);
                if (!targetSemanticSha256.Equals(transformedTargetSemantic, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException("Step-35 ExecuteVeryEarly wrapper semantics drifted across the Step-32 serialization even though this method is outside the authorized rewrite family.");

                var sourceMoveNext = FindVeryEarlyMoveNext(sourceModule);
                if (sourceMoveNext.MetadataToken.ToUInt32() != SourceStateMachineMoveNextToken)
                    throw new InvalidDataException($"Step-35 source ExecuteVeryEarly state-machine MoveNext token drifted: 0x{sourceMoveNext.MetadataToken.ToUInt32():X8}.");
                var transformedMoveNext = FindVeryEarlyMoveNext(transformedModule);
                moveNextSemanticSha256 = RealStS2PrepareMethodRewrite.ComputeMethodSemanticFingerprint(sourceMoveNext);
                var transformedMoveNextSemantic = RealStS2PrepareMethodRewrite.ComputeMethodSemanticFingerprint(transformedMoveNext);
                if (!moveNextSemanticSha256.Equals(transformedMoveNextSemantic, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException("Step-35 ExecuteVeryEarly async state-machine semantics drifted across the Step-32 serialization.");

                var sourceLaterCalls = CountLaterOneTimeInitializationCalls(sourceMoveNext);
                var transformedLaterCalls = CountLaterOneTimeInitializationCalls(transformedMoveNext);
                if (sourceLaterCalls != 0 || transformedLaterCalls != 0)
                    throw new InvalidDataException($"Step-35 ExecuteVeryEarly unexpectedly directly calls a later OneTimeInitialization boundary; source={sourceLaterCalls}, transformed={transformedLaterCalls}.");
                if (CountHarmonyMethodReferences(sourceMoveNext) != 0 || CountHarmonyMethodReferences(transformedMoveNext) != 0)
                    throw new InvalidDataException("Step-35 ExecuteVeryEarly unexpectedly contains a direct Harmony method reference.");

                transformedMethodToken = transformedMethod.MetadataToken.ToUInt32();
                transformedMoveNextToken = transformedMoveNext.MetadataToken.ToUInt32();
                if (sourceResolver.Requests.Count != 0 || transformedResolver.Requests.Count != 0)
                    throw new InvalidDataException("Step-35 source/transformed very-early metadata inspection unexpectedly resolved a dependency through Cecil.");
            }
            progress?.Report(new(gate, 5, 7, transformedPath,
                "Exact source/transformed ExecuteVeryEarly wrapper + async MoveNext semantics requalified; no direct ExecuteEssential/ExecuteDeferred/PrewarmJit or Harmony call crosses this boundary."));

            stage = "Step-21/22 prepared execution-plan preflight";
            var preparedResult = await _preparedPreflight.RunPreparedLoadPreflightAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            if (!preparedResult.Passed)
                throw new InvalidDataException("Step-35 prepared runtime-plan preflight failed: " + preparedResult.Detail.Replace('\n', ' '));

            var planBytes = await File.ReadAllBytesAsync(_planPath, cancellationToken).ConfigureAwait(false);
            var planSha256 = Convert.ToHexString(SHA256.HashData(planBytes)).ToLowerInvariant();
            var plan = JsonSerializer.Deserialize(planBytes, RuntimeFrameworkBindingJsonContext.Default.RuntimeFrameworkBindingPlanDocument)
                ?? throw new InvalidDataException("Step 35 could not deserialize the persisted Step-21/22 runtime-binding plan.");
            ValidateExecutionPlan(plan);

            var prepared = plan.PreparedAssemblies.Select(item =>
            {
                var relative = NormalizeRelative(item.RelativePath);
                var path = ResolveChildPath(_preparedRoot, relative, "Step-35 prepared dependency path");
                VerifyFileLength(path, item.Length, "prepared dependency");
                var hash = ComputeSha1Hex(path);
                if (!hash.Equals(item.Sha1Hex, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException($"Step-35 prepared dependency SHA-1 drifted before execution: {relative}.");
                return new PreparedExecutionEntry(item, path, new AssemblyName(item.AssemblyFullName), ReadModuleInitializerCount(path));
            }).ToArray();

            var preparedPrimary = prepared.Single(item => item.Plan.IsPrimary);
            if (!preparedPrimary.Plan.AssemblyFullName.Equals(TransformedRealStS2AssemblyAdmission.ClosedStep32AssemblyIdentity, StringComparison.Ordinal))
                throw new InvalidDataException("Step-35 prepared-plan primary identity differs from the closed transformed-primary identity.");

            var initializerBearing = prepared.Where(item => !item.Plan.IsPrimary && item.ModuleInitializerCount > 0).ToArray();
            if (initializerBearing.Length != 1 ||
                !string.Equals(initializerBearing[0].AssemblyName.Name, ControlledManagedInitialization.TargetSimpleName, StringComparison.OrdinalIgnoreCase) ||
                (initializerBearing[0].AssemblyName.Version ?? ZeroVersion) != ControlledManagedInitialization.TargetVersion)
            {
                throw new InvalidDataException(
                    "Step 35 requires the previously proven sole initializer-bearing private dependency to remain exact 0Harmony 2.4.2.0; observed: " +
                    (initializerBearing.Length == 0 ? "<none>" : string.Join(" | ", initializerBearing.Select(item => item.Plan.AssemblyFullName))));
            }

            progress?.Report(new(gate, 6, 7, _planPath, "Prepared runtime-binding plan and exact initializer-bearing boundary requalified; no prepared assembly has been CLR-loaded."));

            _preflight = new ExecutionPreflightSnapshot(
                transformedPath,
                transformedSha256,
                transformedMethodToken,
                transformedMoveNextToken,
                targetSemanticSha256,
                moveNextSemanticSha256,
                plan,
                planSha256,
                prepared,
                preparedPrimary,
                initializerBearing[0]);

            EnsureNoStS2Loaded("Gate A exit");
            progress?.Report(new(gate, 7, 7, _planPath, "Execution preflight complete; transformed primary and all prepared dependencies remain outside the CLR."));

            return Pass(gate,
                "EXACT CLOSED TRANSFORMED IMAGE, VERY-EARLY ASYNC STARTUP TARGET, AND EXECUTION RESOLVER PLAN REQUALIFIED; NO STS2 CLR LOAD OR GAME INVOCATION OCCURRED.\n" +
                "Physical Step-32 transform closure re-run: 4/4 PASS\n" +
                $"Source SHA-256: {sourceSha256}\n" +
                $"Transformed SHA-256: {transformedSha256}\n" +
                $"Transformed bytes: {TransformedRealStS2AssemblyAdmission.ClosedStep32TransformedBytes:N0}\n" +
                $"Assembly identity: {TransformedRealStS2AssemblyAdmission.ClosedStep32AssemblyIdentity}\n" +
                $"Module MVID: {TransformedRealStS2AssemblyAdmission.ClosedStep32Mvid}\n" +
                $"Source ExecuteVeryEarly MethodDef token: 0x{SourceTargetMethodToken:X8}\n" +
                $"Transformed ExecuteVeryEarly MethodDef token: 0x{transformedMethodToken:X8}\n" +
                $"ExecuteVeryEarly semantic fingerprint source/transformed: {targetSemanticSha256} / {targetSemanticSha256}\n" +
                $"Source ExecuteVeryEarly MoveNext token: 0x{SourceStateMachineMoveNextToken:X8}\n" +
                $"Transformed ExecuteVeryEarly MoveNext token: 0x{transformedMoveNextToken:X8}\n" +
                $"ExecuteVeryEarly MoveNext semantic fingerprint source/transformed: {moveNextSemanticSha256} / {moveNextSemanticSha256}\n" +
                "Direct later OneTimeInitialization calls from ExecuteVeryEarly MoveNext: 0\n" +
                "Direct Harmony method references from ExecuteVeryEarly MoveNext: 0\n" +
                $"Prepared runtime-binding plan SHA-256: {planSha256}\n" +
                $"Prepared assemblies requalified: {prepared.Length:N0}\n" +
                $"Initializer-free private dependencies eligible on demand: {prepared.Count(item => !item.Plan.IsPrimary && item.ModuleInitializerCount == 0):N0}\n" +
                $"Initializer-bearing dependencies kept forbidden: {initializerBearing[0].Plan.AssemblyFullName}\n" +
                $"Host framework binding entries available: {plan.HostFrameworkBindings.Length:N0}\n" +
                "Original receipt-backed/prepared sts2.dll CLR-loaded: NO\n" +
                "Game type/member reflection or invocation: NO\n" +
                "Native game loading: NO");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _preflight = null;
            return Fail(gate, stage, ex);
        }
    }

    [UnconditionalSuppressMessage(
        "Trimming",
        "IL2026",
        Justification = "Step 35 admits only the exact hash-pinned transformed primary image; member reflection/invocation is deferred to Gate C.")]
    public TransformedRealStS2VeryEarlyInitializationGateResult RunExecutionCapableClrAdmission()
        => RunExecutionCapableClrAdmission(crashCheckpoint: null);

    [UnconditionalSuppressMessage(
        "Trimming",
        "IL2026",
        Justification = "Step 35.0.1 preserves the exact Step-35 transformed-primary LoadFromStream admission path; the added callback is output-only crash telemetry.")]
    public TransformedRealStS2VeryEarlyInitializationGateResult RunExecutionCapableClrAdmission(Action<string>? crashCheckpoint)
    {
        const TransformedRealStS2VeryEarlyInitializationGate gate = TransformedRealStS2VeryEarlyInitializationGate.ExecutionCapableClrAdmission;
        var stage = "initialization";
        try
        {
            Checkpoint(crashCheckpoint, "B_ENTRY — entered Gate B execution-capable CLR admission.");
            ThrowIfDisposed();
            var preflight = RequirePreflight();
            EnsureNoStS2Loaded("Gate B entry");
            Checkpoint(crashCheckpoint, "B_FRESH_PROCESS_PASS — no sts2 assembly is CLR-resident at Gate B entry.");
            if (_loadContext is not null)
                throw new InvalidOperationException("Step 35 Gate B requires a fresh dedicated load context.");

            stage = "immediate transformed hash recheck";
            Checkpoint(crashCheckpoint, "B_HASH_START — rechecking exact transformed primary length/SHA-256 before CLR admission.");
            VerifyFileLength(preflight.TransformedPath, TransformedRealStS2AssemblyAdmission.ClosedStep32TransformedBytes, "transformed primary");
            var immediateSha256 = ComputeSha256Hex(preflight.TransformedPath);
            if (!immediateSha256.Equals(TransformedRealStS2AssemblyAdmission.ClosedStep32TransformedSha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Step-35 transformed image changed between Gate A verification and Gate B CLR admission.");
            Checkpoint(crashCheckpoint, $"B_HASH_PASS — transformed primary recheck matched {immediateSha256}.");

            stage = "execution-capable strict AssemblyLoadContext construction";
            Checkpoint(crashCheckpoint, "B_ALC_CONSTRUCT_START — constructing strict Step-35 execution AssemblyLoadContext.");
            var context = new Step35ExecutionLoadContext(
                LoadContextName,
                preflight.Plan,
                preflight.PreparedAssemblies,
                _collectibleLoadContext,
                crashCheckpoint);
            _loadContext = context;
            Checkpoint(crashCheckpoint, "B_ALC_CONSTRUCT_PASS — strict Step-35 execution AssemblyLoadContext constructed.");

            stage = "exact transformed sts2.dll LoadFromStream";
            Checkpoint(crashCheckpoint, "B_LOADPRIMARY_START — entering exact transformed primary LoadPrimary/LoadFromStream path.");
            var assembly = context.LoadPrimary(preflight.TransformedPath, immediateSha256);
            Checkpoint(crashCheckpoint, "B_LOADPRIMARY_PASS — exact transformed primary returned from LoadPrimary/LoadFromStream.");
            if (!ReferenceEquals(AssemblyLoadContext.GetLoadContext(assembly), context))
                throw new InvalidDataException("The transformed sts2.dll did not load into the dedicated Step-35 AssemblyLoadContext.");
            Checkpoint(crashCheckpoint, "B_CONTEXT_OWNERSHIP_PASS — transformed primary belongs to the dedicated Step-35 AssemblyLoadContext.");

            Checkpoint(crashCheckpoint, "B_GETNAME_START — reading loaded transformed assembly identity.");
            var actualIdentity = assembly.GetName().FullName ?? assembly.GetName().Name ?? string.Empty;
            if (!actualIdentity.Equals(TransformedRealStS2AssemblyAdmission.ClosedStep32AssemblyIdentity, StringComparison.Ordinal))
                throw new InvalidDataException($"Loaded transformed identity mismatch. Expected '{TransformedRealStS2AssemblyAdmission.ClosedStep32AssemblyIdentity}', actual '{actualIdentity}'.");
            Checkpoint(crashCheckpoint, $"B_GETNAME_PASS — loaded transformed identity matched: {actualIdentity}.");
            Checkpoint(crashCheckpoint, "B_MVID_START — reading loaded transformed module MVID.");
            var actualMvid = assembly.ManifestModule.ModuleVersionId;
            if (actualMvid != TransformedRealStS2AssemblyAdmission.ClosedStep32Mvid)
                throw new InvalidDataException($"Loaded transformed module MVID mismatch. Expected {TransformedRealStS2AssemblyAdmission.ClosedStep32Mvid}, actual {actualMvid}.");
            Checkpoint(crashCheckpoint, $"B_MVID_PASS — loaded transformed MVID matched: {actualMvid}.");

            if (context.ManagedResolverRequests.Count != 0 || context.PrivateLoads.Count != 0 ||
                context.InitializerBearingRequests.Count != 0 || context.RejectedManagedRequests.Count != 0 || context.NativeLoadAttempts.Count != 0)
            {
                throw new InvalidDataException(
                    "Step-35 transformed primary admission no longer matches the physically closed Step-33 zero-resolution admission behavior. " +
                    context.FormatResolverState());
            }
            Checkpoint(crashCheckpoint, "B_ZERO_RESOLUTION_PASS — primary admission produced zero managed/private/initializer/rejected/native resolution activity.");

            var matches = FindLoadedStS2Assemblies();
            if (matches.Length != 1 || !ReferenceEquals(matches[0], assembly))
                throw new InvalidDataException($"Expected exactly one transformed sts2 assembly after Step-35 Gate B, found {matches.Length}.");
            Checkpoint(crashCheckpoint, "B_GLOBAL_RESIDENCY_PASS — exactly one sts2 assembly is resident and it is the transformed primary.");
            var contextAssemblies = context.Assemblies.ToArray();
            if (contextAssemblies.Length != 1 || !ReferenceEquals(contextAssemblies[0], assembly))
                throw new InvalidDataException($"Step-35 context contains {contextAssemblies.Length} private assemblies immediately after admission instead of exactly transformed sts2.");
            Checkpoint(crashCheckpoint, "B_PRIVATE_CONTEXT_ENUM_PASS — private context contains exactly the transformed primary after admission.");

            _admission = new PrimaryAdmissionSnapshot(assembly, actualIdentity, actualMvid, immediateSha256);
            Checkpoint(crashCheckpoint, "B_PASS_RETURN — Gate B completed successfully and is returning its PASS result.");

            return Pass(gate,
                "PHYSICALLY CLOSED STEP-33 TRANSFORMED-PRIMARY ADMISSION BEHAVIOR RE-ESTABLISHED IN THE STEP-35 VERY-EARLY EXECUTION CONTEXT; NO GAME MEMBER REFLECTION/INVOCATION YET.\n" +
                $"Loaded identity: {actualIdentity}\n" +
                $"Loaded MVID: {actualMvid}\n" +
                $"AssemblyLoadContext: {context.Name ?? LoadContextName}\n" +
                $"Exact transformed SHA-256 immediately before LoadFromStream: {immediateSha256}\n" +
                "Managed resolver requests during primary admission: 0\n" +
                "Private dependency loads during primary admission: 0\n" +
                "Initializer-bearing dependency requests during primary admission: 0\n" +
                "Rejected managed requests during primary admission: 0\n" +
                "Native load attempts during primary admission: 0\n" +
                "Original receipt-backed/prepared sts2.dll used as CLR input: NO\n" +
                "Game type/member reflection performed: NO\n" +
                "Game method invoked: NO\n" +
                "Godot/game initialization requested: NO");
        }
        catch (Exception ex)
        {
            Checkpoint(crashCheckpoint, $"B_MANAGED_FAIL — stage={stage}; {ex.GetType().FullName}: {ex.Message}");
            if (_collectibleLoadContext)
                ReleaseLoadContext();
            _admission = null;
            return Fail(gate, stage, ex);
        }
    }

    [UnconditionalSuppressMessage(
        "Trimming",
        "IL2026",
        Justification = "Step 35 deliberately reflects and invokes one exact async initialization method on the exact transformed real-StS2 image. The dynamic payload is preserved by the physical copy/no-link runtime policy.")]
    public Task<TransformedRealStS2VeryEarlyInitializationGateResult> RunExactExecuteVeryEarlyInvocationAsync(
        CancellationToken cancellationToken = default)
        => RunExactExecuteVeryEarlyInvocationAsync(crashCheckpoint: null, cancellationToken: cancellationToken);

    [UnconditionalSuppressMessage(
        "Trimming",
        "IL2026",
        Justification = "Step 35.0.1 preserves the exact Step-35 reflected ExecuteVeryEarly invocation; the added callback is output-only crash telemetry.")]
    public async Task<TransformedRealStS2VeryEarlyInitializationGateResult> RunExactExecuteVeryEarlyInvocationAsync(
        Action<string>? crashCheckpoint,
        CancellationToken cancellationToken = default)
    {
        const TransformedRealStS2VeryEarlyInitializationGate gate = TransformedRealStS2VeryEarlyInitializationGate.ExactExecuteVeryEarlyInvocation;
        var stage = "initialization";
        try
        {
            Checkpoint(crashCheckpoint, "C_ENTRY — entered Gate C exact ExecuteVeryEarly binding/invocation/await boundary.");
            ThrowIfDisposed();
            var preflight = RequirePreflight();
            var admission = RequireAdmission();
            var context = RequireLoadContext();
            cancellationToken.ThrowIfCancellationRequested();

            if (context.ManagedResolverRequests.Count != 0 || context.PrivateLoads.Count != 0 ||
                context.InitializerBearingRequests.Count != 0 || context.RejectedManagedRequests.Count != 0 || context.NativeLoadAttempts.Count != 0)
            {
                throw new InvalidDataException("Step-35 resolver state changed before Gate C invocation. " + context.FormatResolverState());
            }
            Checkpoint(crashCheckpoint, "C_RESOLVER_PRECHECK_PASS — resolver/native state is still zero immediately before target binding.");

            var resolverRequestsBefore = context.ManagedResolverRequests.Count;
            var hostLoadsBefore = context.HostLoads.Count;
            var privateLoadsBefore = context.PrivateLoads.Count;
            var nativeAttemptsBefore = context.NativeLoadAttempts.Count;

            stage = "exact transformed very-early type/member binding";
            Checkpoint(crashCheckpoint, "C_BIND_TYPE_START — calling Assembly.GetType for exact OneTimeInitialization target.");
            var targetType = admission.Assembly.GetType(TargetTypeFullName, throwOnError: true, ignoreCase: false)
                ?? throw new MissingMemberException(TargetTypeFullName);
            if (!ReferenceEquals(targetType.Assembly, admission.Assembly))
                throw new InvalidDataException("Step-35 target type did not bind from the transformed sts2 assembly.");
            Checkpoint(crashCheckpoint, "C_BIND_TYPE_PASS — exact OneTimeInitialization target type bound from transformed sts2.");

            Checkpoint(crashCheckpoint, "C_BIND_METHOD_START — calling Type.GetMethod for exact static parameterless ExecuteVeryEarly.");
            var method = targetType.GetMethod(
                TargetMethodName,
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                binder: null,
                types: Type.EmptyTypes,
                modifiers: null)
                ?? throw new MissingMethodException(TargetTypeFullName, TargetMethodName);
            Checkpoint(crashCheckpoint, "C_BIND_METHOD_PASS — ExecuteVeryEarly MethodInfo binding returned.");

            if (!ReferenceEquals(method.DeclaringType, targetType) || !method.IsStatic || method.ReturnType != typeof(Task) || method.GetParameters().Length != 0)
                throw new InvalidDataException("Step-35 reflected ExecuteVeryEarly identity/signature drifted from the exact static parameterless System.Threading.Tasks.Task target.");
            Checkpoint(crashCheckpoint, "C_SIGNATURE_PASS — reflected method is exact static parameterless Task-returning target.");
            if (method.MetadataToken != unchecked((int)preflight.TransformedMethodToken))
                throw new InvalidDataException($"Step-35 reflected ExecuteVeryEarly token drifted: 0x{method.MetadataToken:X8} != preflight 0x{preflight.TransformedMethodToken:X8}.");
            Checkpoint(crashCheckpoint, $"C_TOKEN_PASS — reflected ExecuteVeryEarly token matched 0x{method.MetadataToken:X8}.");
            if (method.Module.ModuleVersionId != TransformedRealStS2AssemblyAdmission.ClosedStep32Mvid)
                throw new InvalidDataException("Step-35 reflected ExecuteVeryEarly module MVID drifted from the closed transformed image.");
            Checkpoint(crashCheckpoint, $"C_MVID_PASS — reflected ExecuteVeryEarly module MVID matched {method.Module.ModuleVersionId}.");

            stage = "single exact transformed ExecuteVeryEarly invocation";
            Task task;
            try
            {
                Checkpoint(crashCheckpoint, "C_INVOKE_START — entering the first and only MethodInfo.Invoke(null, null) for transformed ExecuteVeryEarly.");
                var result = method.Invoke(null, null);
                Checkpoint(crashCheckpoint, "C_INVOKE_RETURNED — MethodInfo.Invoke returned to the launcher.");
                task = result as Task
                    ?? throw new InvalidDataException("Step-35 ExecuteVeryEarly returned null or a non-Task object despite its exact Task return contract.");
                Checkpoint(crashCheckpoint, $"C_TASK_CONFIRMED — invocation returned a non-null Task; initial status={task.Status}.");
            }
            catch (TargetInvocationException ex)
            {
                var target = ex.InnerException ?? ex;
                throw new InvalidOperationException(
                    "Step-35 transformed ExecuteVeryEarly threw synchronously during the first controlled invocation. " +
                    DescribeException(target) + "\nResolver state at failure: " + context.FormatResolverState(), target);
            }

            stage = "await exact ExecuteVeryEarly Task completion";
            try
            {
                Checkpoint(crashCheckpoint, "C_WAIT_START — awaiting the exact returned ExecuteVeryEarly Task with the predeclared 60-second boundary.");
                await task.WaitAsync(TimeSpan.FromSeconds(60), cancellationToken).ConfigureAwait(false);
                Checkpoint(crashCheckpoint, $"C_WAIT_COMPLETED — ExecuteVeryEarly Task await returned; status={task.Status}.");
            }
            catch (TimeoutException ex)
            {
                throw new TimeoutException(
                    "Step-35 ExecuteVeryEarly did not complete within the predeclared 60-second boundary. Resolver state at timeout: " + context.FormatResolverState(), ex);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    "Step-35 transformed ExecuteVeryEarly Task faulted during the first controlled await. " +
                    DescribeException(ex) + "\nResolver state at failure: " + context.FormatResolverState(), ex);
            }

            stage = "post-invocation resolver/native confinement";
            if (!task.IsCompletedSuccessfully)
                throw new InvalidDataException($"Step-35 ExecuteVeryEarly Task returned from await without IsCompletedSuccessfully=true; status={task.Status}.");
            if (context.InitializerBearingRequests.Count != 0)
                throw new InvalidDataException("Step-35 ExecuteVeryEarly requested an initializer-bearing private dependency, which remains forbidden: " + string.Join(" | ", context.InitializerBearingRequests));
            if (context.RejectedManagedRequests.Count != 0)
                throw new InvalidDataException("Step-35 ExecuteVeryEarly triggered an unplanned managed resolver request: " + string.Join(" | ", context.RejectedManagedRequests));
            if (context.NativeLoadAttempts.Count != 0)
                throw new InvalidDataException("Step-35 ExecuteVeryEarly attempted native library resolution: " + string.Join(" | ", context.NativeLoadAttempts));
            Checkpoint(crashCheckpoint, $"C_POST_RESOLVER_PASS — post-await confinement passed; {context.FormatResolverState()}.");

            var privateAssemblies = context.Assemblies.ToArray();
            foreach (var loaded in privateAssemblies.Where(item => !ReferenceEquals(item, admission.Assembly)))
            {
                var simple = loaded.GetName().Name ?? string.Empty;
                var prepared = preflight.PreparedAssemblies.SingleOrDefault(item =>
                    !item.Plan.IsPrimary && string.Equals(item.AssemblyName.Name, simple, StringComparison.OrdinalIgnoreCase));
                if (prepared is null)
                    throw new InvalidDataException("Step-35 private context contains an assembly outside the prepared plan: " + (loaded.GetName().FullName ?? simple));
                if (prepared.ModuleInitializerCount != 0)
                    throw new InvalidDataException("Step-35 private context admitted an initializer-bearing dependency: " + prepared.Plan.AssemblyFullName);
            }

            Checkpoint(crashCheckpoint, $"C_PRIVATE_CONTEXT_ENUM_PASS — private context enumeration completed with {privateAssemblies.Length} resident assembly/assemblies.");
            _execution = new ExecutionSnapshot(
                method.MetadataToken,
                context.ManagedResolverRequests.Skip(resolverRequestsBefore).ToArray(),
                context.HostLoads.Skip(hostLoadsBefore).ToArray(),
                context.PrivateLoads.Skip(privateLoadsBefore).ToArray(),
                context.NativeLoadAttempts.Skip(nativeAttemptsBefore).ToArray(),
                privateAssemblies.Select(item => item.GetName().FullName ?? item.GetName().Name ?? "<unknown>").ToArray());
            Checkpoint(crashCheckpoint, "C_PASS_RETURN — Gate C completed successfully and is returning its PASS result.");

            return Pass(gate,
                "FIRST CONTROLLED INVOCATION/AWAIT OF THE EXACT TRANSFORMED REAL-STS2 EXECUTEVERYEARLY INITIALIZATION SITE COMPLETED NORMALLY.\n" +
                $"Target type: {TargetTypeFullName}\n" +
                $"Target method: {TargetMethodFullName}\n" +
                $"Reflected transformed MethodDef token: 0x{method.MetadataToken:X8}\n" +
                $"Preflight transformed async MoveNext token: 0x{preflight.TransformedMoveNextToken:X8}\n" +
                $"Target module MVID: {method.Module.ModuleVersionId}\n" +
                "Launcher invocation count: 1\n" +
                "Return contract: exact System.Threading.Tasks.Task; awaited to completion: YES\n" +
                $"Task final status: {task.Status}\n" +
                $"Managed resolver requests caused by binding/invocation/await: {context.ManagedResolverRequests.Count - resolverRequestsBefore:N0}\n" +
                $"Exact planned host-framework loads caused by binding/invocation/await: {context.HostLoads.Count - hostLoadsBefore:N0}\n" +
                $"Initializer-free prepared private dependency loads caused by binding/invocation/await: {context.PrivateLoads.Count - privateLoadsBefore:N0}\n" +
                "Initializer-bearing private dependency requests: 0\n" +
                "Unplanned managed resolver requests: 0\n" +
                "Native resolution attempts: 0\n" +
                $"Private context assemblies after completion: {privateAssemblies.Length:N0}\n" +
                "Receipt-backed/prepared original sts2.dll CLR-loaded: NO\n" +
                "Game entry point invoked by launcher: NO\n" +
                "Later OneTimeInitialization entry methods intentionally invoked by launcher: NO\n" +
                "Harmony/MonoMod runtime patch API intentionally invoked by launcher: NO\n" +
                "Godot/game startup intentionally requested by launcher: NO");
        }
        catch (OperationCanceledException)
        {
            Checkpoint(crashCheckpoint, $"C_CANCELLED_INCONCLUSIVE — stage={stage}; invocation may already have occurred; fresh process required before retry.");
            throw;
        }
        catch (Exception ex)
        {
            Checkpoint(crashCheckpoint, $"C_MANAGED_FAIL — stage={stage}; {ex.GetType().FullName}: {ex.Message}");
            _execution = null;
            return Fail(gate, stage, ex);
        }
    }

    public async Task<TransformedRealStS2VeryEarlyInitializationGateResult> RunFinalIsolationAuditAsync(
        IProgress<TransformedRealStS2VeryEarlyInitializationProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        const TransformedRealStS2VeryEarlyInitializationGate gate = TransformedRealStS2VeryEarlyInitializationGate.FinalIsolationAudit;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var preflight = RequirePreflight();
            var admission = RequireAdmission();
            var execution = RequireExecution();
            var context = RequireLoadContext();
            cancellationToken.ThrowIfCancellationRequested();

            stage = "post-execution OfflineReady and source reproof";
            progress?.Report(new(gate, 0, 3, null, "Re-proving the receipt-backed install after exact transformed ExecuteVeryEarly initialization."));
            var offline = await _offlineInspection.RunAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            if (offline.Outcome == SteamOfflineInstallOutcome.Cancelled)
                throw new OperationCanceledException("Step 35 final OfflineReady audit was cancelled.", cancellationToken);
            if (!offline.Success || string.IsNullOrWhiteSpace(offline.ManagedInstallRelativePath))
                throw new InvalidDataException(offline.Error ?? "Step-35 final OfflineReady re-verification failed.");

            var managedRoot = ResolveChildPath(_launcherDataRoot, offline.ManagedInstallRelativePath, "Step-35 managed install");
            var trustedPrimaryPath = ResolveChildPath(managedRoot, TransformedRealStS2AssemblyAdmission.ExactPrimaryRelativePath, "Step-35 trusted primary path");
            var trustedSha256 = ComputeSha256Hex(trustedPrimaryPath);
            if (!trustedSha256.Equals(TransformedRealStS2AssemblyAdmission.ClosedStep32SourceSha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException($"Receipt-backed original sts2.dll changed after Step-35 execution: {trustedSha256}.");
            progress?.Report(new(gate, 1, 3, trustedPrimaryPath, "Trusted receipt-backed primary remains byte-identical."));

            stage = "transformed image / plan / dependency hash reproof";
            var transformedSha256 = ComputeSha256Hex(preflight.TransformedPath);
            if (!transformedSha256.Equals(TransformedRealStS2AssemblyAdmission.ClosedStep32TransformedSha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Verified transformed sts2.dll changed after ExecuteVeryEarly initialization.");
            var planSha256 = ComputeSha256Hex(_planPath);
            if (!planSha256.Equals(preflight.PlanSha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Prepared runtime-binding plan changed during Step-35 execution.");

            var verifiedPrivate = 0;
            foreach (var loaded in context.Assemblies.Where(item => !ReferenceEquals(item, admission.Assembly)))
            {
                var simple = loaded.GetName().Name ?? string.Empty;
                var prepared = preflight.PreparedAssemblies.SingleOrDefault(item =>
                    !item.Plan.IsPrimary && string.Equals(item.AssemblyName.Name, simple, StringComparison.OrdinalIgnoreCase))
                    ?? throw new InvalidDataException("Step-35 loaded private assembly is outside the prepared plan: " + (loaded.GetName().FullName ?? simple));
                if (prepared.ModuleInitializerCount != 0)
                    throw new InvalidDataException("Step-35 loaded initializer-bearing dependency during ExecuteVeryEarly initialization: " + prepared.Plan.AssemblyFullName);
                VerifyFileLength(prepared.PreparedPath, prepared.Plan.Length, "loaded private dependency");
                var hash = ComputeSha1Hex(prepared.PreparedPath);
                if (!hash.Equals(prepared.Plan.Sha1Hex, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException("Step-35 loaded private dependency bytes changed: " + prepared.Plan.RelativePath);
                verifiedPrivate++;
            }

            if (context.InitializerBearingRequests.Count != 0 || context.RejectedManagedRequests.Count != 0 || context.NativeLoadAttempts.Count != 0)
                throw new InvalidDataException("Step-35 final resolver/native isolation counters are not clean. " + context.FormatResolverState());
            var matches = FindLoadedStS2Assemblies();
            if (matches.Length != 1 || !ReferenceEquals(matches[0], admission.Assembly) || !ReferenceEquals(AssemblyLoadContext.GetLoadContext(admission.Assembly), context))
                throw new InvalidDataException("Step-35 transformed-primary CLR residency/context ownership drifted during final audit.");
            if (execution.MethodToken != unchecked((int)preflight.TransformedMethodToken))
                throw new InvalidDataException("Step-35 execution snapshot ExecuteVeryEarly token drifted during final audit.");

            progress?.Report(new(gate, 3, 3, preflight.TransformedPath, "Final source/transformed/plan/dependency/context isolation checks passed."));

            return Pass(gate,
                "STEP 35 FINAL CONTROLLED VERY-EARLY INITIALIZATION ISOLATION AUDIT PASSED.\n" +
                $"Post-execution OfflineReady: PASS ({offline.VerifiedFiles:N0}/{offline.PlannedFiles:N0} files)\n" +
                $"Receipt-backed original SHA-256 unchanged: {trustedSha256}\n" +
                $"Verified transformed SHA-256 unchanged: {transformedSha256}\n" +
                $"Runtime-binding plan SHA-256 unchanged: {planSha256}\n" +
                $"Unique resident sts2 identity: {admission.AssemblyFullName}\n" +
                $"Resident sts2 AssemblyLoadContext: {context.Name ?? LoadContextName}\n" +
                "Resident sts2 load input: exact physically closed Step-32 transformed image\n" +
                $"Initializer-free prepared private dependencies resident and re-hashed: {verifiedPrivate:N0}\n" +
                $"Managed resolver requests total: {context.ManagedResolverRequests.Count:N0}\n" +
                $"Exact planned host-framework loads total: {context.HostLoads.Count:N0}\n" +
                $"Prepared private dependency loads total: {context.PrivateLoads.Count:N0}\n" +
                "Initializer-bearing private dependency requests: 0\n" +
                "Unplanned managed resolution: NO\n" +
                "Native game resolution/loading: NO\n" +
                "Exact transformed ExecuteVeryEarly invocation count: 1\n" +
                "Receipt-backed/prepared original sts2.dll CLR-loaded: NO\n" +
                "Game entry point / ExecuteEssential / ExecuteDeferred intentionally invoked by launcher: NO\n" +
                "Harmony/MonoMod runtime patching intentionally invoked by launcher: NO\n" +
                "Godot/game startup intentionally requested by launcher: NO\n" +
                "Authorization after Step-35 PASS: a later separately gated boundary may advance into the next measured managed-initialization site; Step 35 authorizes no broader startup by itself.");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return Fail(gate, stage, ex);
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;
        ReleaseLoadContext();
        _preparedPreflight.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    private static void Checkpoint(Action<string>? crashCheckpoint, string detail)
    {
        if (crashCheckpoint is null)
            return;
        try
        {
            crashCheckpoint(detail);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Step-35 crash-checkpoint callback failed: {ex.GetType().Name}: {ex.Message}");
        }
    }

    private static string DescribeException(Exception ex)
    {
        var parts = new List<string>();
        var current = ex;
        for (var depth = 0; current is not null && depth < 6; depth++, current = current.InnerException!)
        {
            var stack = string.IsNullOrWhiteSpace(current.StackTrace) ? string.Empty : " | stack=" + current.StackTrace.Replace('\n', ' ').Replace('\r', ' ');
            parts.Add($"[{depth}] {current.GetType().FullName}: {current.Message}{stack}");
        }
        return string.Join(" || ", parts);
    }

    private static void RequireVeryEarlySignature(MethodDefinition method, string scope)
    {
        if (!method.IsStatic || method.Parameters.Count != 0 || method.ReturnType.FullName != "System.Threading.Tasks.Task")
            throw new InvalidDataException($"Step-35 {scope} ExecuteVeryEarly signature drifted from static parameterless System.Threading.Tasks.Task.");
        if (!method.HasBody)
            throw new InvalidDataException($"Step-35 {scope} ExecuteVeryEarly no longer has managed IL.");
    }

    private static MethodDefinition FindVeryEarlyMoveNext(ModuleDefinition module)
    {
        var stateMachines = EnumerateTypes(module.Types)
            .Where(type => type.Name.Equals(TargetStateMachineTypeName, StringComparison.Ordinal) &&
                           type.DeclaringType?.FullName == TargetTypeFullName)
            .ToArray();
        if (stateMachines.Length != 1)
            throw new InvalidDataException($"Step-35 expected exactly one nested {TargetStateMachineTypeName} state-machine type, found {stateMachines.Length}.");
        var methods = stateMachines[0].Methods
            .Where(method => method.Name.Equals("MoveNext", StringComparison.Ordinal) &&
                             !method.IsStatic && method.Parameters.Count == 0 && method.ReturnType.FullName == "System.Void")
            .ToArray();
        if (methods.Length != 1 || !methods[0].HasBody)
            throw new InvalidDataException($"Step-35 expected exactly one managed-IL MoveNext on {TargetStateMachineTypeName}, found {methods.Length}.");
        return methods[0];
    }

    private static MethodDefinition FindMethodByToken(ModuleDefinition module, uint token)
        => EnumerateTypes(module.Types).SelectMany(type => type.Methods)
            .SingleOrDefault(method => method.MetadataToken.ToUInt32() == token)
            ?? throw new MissingMethodException($"Step-35 source method token 0x{token:X8} is absent.");

    private static int CountLaterOneTimeInitializationCalls(MethodDefinition moveNext)
    {
        var later = new HashSet<string>(StringComparer.Ordinal) { "ExecuteEssential", "ExecuteDeferred", "PrewarmJit" };
        return moveNext.Body.Instructions.Count(instruction =>
            instruction.Operand is MethodReference reference &&
            reference.DeclaringType.FullName == TargetTypeFullName &&
            later.Contains(reference.Name));
    }

    private static int CountHarmonyMethodReferences(MethodDefinition method)
        => method.Body.Instructions.Count(instruction =>
            instruction.Operand is MethodReference reference &&
            ((reference.DeclaringType.Scope is AssemblyNameReference assembly && assembly.Name == "0Harmony") ||
             (reference.DeclaringType.Namespace ?? string.Empty).StartsWith("HarmonyLib", StringComparison.Ordinal)));

    private static IEnumerable<TypeDefinition> EnumerateTypes(IEnumerable<TypeDefinition> roots)
    {
        foreach (var type in roots)
        {
            yield return type;
            foreach (var nested in EnumerateTypes(type.NestedTypes))
                yield return nested;
        }
    }

    private void ValidateExecutionPlan(RuntimeFrameworkBindingPlanDocument plan)
    {
        if (plan.SchemaVersion != RuntimeFrameworkBindingPlanDocument.CurrentSchemaVersion)
            throw new InvalidDataException($"Unsupported Step-21/22 runtime-binding plan schema {plan.SchemaVersion}.");
        if (!plan.RuntimeClosureReady || plan.Blockers.Length != 0 || plan.Edges.Any(edge => edge.BindingKind.StartsWith("Blocker:", StringComparison.Ordinal)))
            throw new InvalidDataException("Step 35 requires the physically established zero-blocker Step-21/22 runtime-binding plan.");
        var primary = plan.PreparedAssemblies.Where(item => item.IsPrimary).ToArray();
        if (primary.Length != 1)
            throw new InvalidDataException($"Step 35 expected exactly one prepared primary, found {primary.Length}.");
        if (!primary[0].AssemblyFullName.Equals(TransformedRealStS2AssemblyAdmission.ClosedStep32AssemblyIdentity, StringComparison.Ordinal) ||
            !plan.PrimaryAssemblyFullName.Equals(TransformedRealStS2AssemblyAdmission.ClosedStep32AssemblyIdentity, StringComparison.Ordinal))
            throw new InvalidDataException("Step-35 runtime-binding plan primary identity differs from the closed transformed assembly identity.");
        if (!primary[0].RelativePath.Equals(plan.PrimaryAssemblyRelativePath, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Step-35 runtime-binding plan primary path/entry disagree.");
    }

    private static int ReadModuleInitializerCount(string path)
    {
        using var resolver = new RejectingAssemblyResolver();
        using var module = ModuleDefinition.ReadModule(path, new ReaderParameters
        {
            ReadSymbols = false,
            ReadingMode = ReadingMode.Deferred,
            AssemblyResolver = resolver,
        });
        var count = module.Types
            .Where(type => type.Name.Equals("<Module>", StringComparison.Ordinal))
            .SelectMany(type => type.Methods)
            .Count(method => method.Name.Equals(".cctor", StringComparison.Ordinal) && method.IsStatic && method.HasBody);
        if (resolver.Requests.Count != 0)
            throw new InvalidDataException("Step-35 prepared dependency metadata inspection unexpectedly requested assembly resolution.");
        return count;
    }

    private static void RequireRewritePass(string label, RealStS2PrepareMethodRewriteGateResult result)
    {
        if (!result.Passed)
            throw new InvalidDataException($"{label} failed while Step 35 requalified the closed Step-32 transformation: {result.Detail.Replace('\n', ' ')}");
    }

    private void EnsureNoStS2Loaded(string stage)
    {
        var matches = FindLoadedStS2Assemblies();
        if (matches.Length == 0)
            return;
        var detail = string.Join(" | ", matches.Select(assembly =>
            $"{assembly.GetName().FullName} @ {AssemblyLoadContext.GetLoadContext(assembly)?.Name ?? "<unknown-context>"}"));
        throw new InvalidDataException($"Step 35 requires a fresh process at {stage}; sts2 is already CLR-resident: {detail}");
    }

    private static Assembly[] FindLoadedStS2Assemblies()
        => AppDomain.CurrentDomain.GetAssemblies()
            .Where(assembly => string.Equals(assembly.GetName().Name, TransformedRealStS2AssemblyAdmission.ExpectedPrimarySimpleName, StringComparison.OrdinalIgnoreCase))
            .ToArray();

    private void ReleaseLoadContext()
    {
        var context = _loadContext;
        _loadContext = null;
        _admission = null;
        _execution = null;
        if (context is not null && context.IsCollectible)
            context.Unload();
    }

    private ExecutionPreflightSnapshot RequirePreflight()
        => _preflight ?? throw new InvalidOperationException("Step 35 Gate A must pass before Gate B.");

    private PrimaryAdmissionSnapshot RequireAdmission()
        => _admission ?? throw new InvalidOperationException("Step 35 Gate B must pass before Gate C.");

    private ExecutionSnapshot RequireExecution()
        => _execution ?? throw new InvalidOperationException("Step 35 Gate C must pass before Gate D.");

    private Step35ExecutionLoadContext RequireLoadContext()
        => _loadContext ?? throw new InvalidOperationException("Step 35 dedicated AssemblyLoadContext is unavailable.");

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(TransformedRealStS2VeryEarlyInitialization));
    }

    private static string ComputeSha256Hex(string path)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }

    private static string ComputeSha1Hex(string path)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Convert.ToHexString(SHA1.HashData(stream)).ToLowerInvariant();
    }

    private static void VerifyFileLength(string path, long expected, string scope)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"Step 35 {scope} is missing.", path);
        var actual = new FileInfo(path).Length;
        if (actual != expected)
            throw new InvalidDataException($"Step 35 {scope} length mismatch: {actual} != {expected}.");
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

    private static TransformedRealStS2VeryEarlyInitializationGateResult Pass(TransformedRealStS2VeryEarlyInitializationGate gate, string detail)
        => new(gate, true, detail);

    private static TransformedRealStS2VeryEarlyInitializationGateResult Fail(TransformedRealStS2VeryEarlyInitializationGate gate, string stage, Exception ex)
        => new(gate, false, $"Stage: {stage}\n{ex.GetType().Name}: {ex.Message}");

    private sealed record ExecutionPreflightSnapshot(
        string TransformedPath,
        string TransformedSha256,
        uint TransformedMethodToken,
        uint TransformedMoveNextToken,
        string TargetSemanticSha256,
        string MoveNextSemanticSha256,
        RuntimeFrameworkBindingPlanDocument Plan,
        string PlanSha256,
        PreparedExecutionEntry[] PreparedAssemblies,
        PreparedExecutionEntry PreparedPrimary,
        PreparedExecutionEntry ForbiddenInitializerDependency);

    internal sealed record PreparedExecutionEntry(
        RuntimeBindingPreparedAssembly Plan,
        string PreparedPath,
        AssemblyName AssemblyName,
        int ModuleInitializerCount);

    private sealed record PrimaryAdmissionSnapshot(
        Assembly Assembly,
        string AssemblyFullName,
        Guid Mvid,
        string ImmediateSha256);

    private sealed record ExecutionSnapshot(
        int MethodToken,
        IReadOnlyList<string> ManagedResolverRequests,
        IReadOnlyList<string> HostLoads,
        IReadOnlyList<string> PrivateLoads,
        IReadOnlyList<string> NativeLoadAttempts,
        IReadOnlyList<string> PrivateContextAssemblies);

    internal sealed class Step35ExecutionLoadContext : AssemblyLoadContext
    {
        private readonly IReadOnlyDictionary<string, PreparedExecutionEntry> _privateBySimpleName;
        private readonly RuntimeBindingHostFramework[] _hostBindings;
        private readonly Action<string>? _crashCheckpoint;

        internal Step35ExecutionLoadContext(
            string name,
            RuntimeFrameworkBindingPlanDocument plan,
            IReadOnlyList<PreparedExecutionEntry> preparedAssemblies,
            bool isCollectible,
            Action<string>? crashCheckpoint = null)
            : base(name, isCollectible)
        {
            _crashCheckpoint = crashCheckpoint;
            var privateBySimpleName = new Dictionary<string, PreparedExecutionEntry>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in preparedAssemblies.Where(item => !item.Plan.IsPrimary))
            {
                var simple = item.AssemblyName.Name ?? throw new InvalidDataException($"Prepared assembly identity has no simple name: {item.Plan.AssemblyFullName}");
                if (IsHostFrameworkContractName(simple))
                    throw new InvalidDataException($"Step-35 resolver received framework-shaped private assembly '{simple}'.");
                if (!privateBySimpleName.TryAdd(simple, item))
                    throw new InvalidDataException($"Step-35 resolver received duplicate prepared simple name '{simple}'.");
            }
            _privateBySimpleName = privateBySimpleName;
            _hostBindings = plan.HostFrameworkBindings;
        }

        internal List<string> ManagedResolverRequests { get; } = [];
        internal List<string> HostLoads { get; } = [];
        internal List<string> PrivateLoads { get; } = [];
        internal List<string> InitializerBearingRequests { get; } = [];
        internal List<string> RejectedManagedRequests { get; } = [];
        internal List<string> NativeLoadAttempts { get; } = [];

        [UnconditionalSuppressMessage(
            "Trimming",
            "IL2026",
            Justification = "Step 35 loads only the exact hash-pinned transformed primary image and exact hash-pinned prepared private dependencies selected by the persisted runtime plan.")]
        internal Assembly LoadPrimary(string transformedPath, string expectedSha256)
        {
            Checkpoint("B_LOADPRIMARY_REHASH_START — LoadPrimary is re-hashing transformed bytes immediately before LoadFromStream.");
            var actualSha256 = ComputeSha256Hex(transformedPath);
            if (!actualSha256.Equals(expectedSha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Step-35 transformed primary hash changed immediately before LoadFromStream.");
            Checkpoint($"B_LOADPRIMARY_REHASH_PASS — immediate LoadPrimary SHA-256 matched {actualSha256}.");
            using var stream = new FileStream(transformedPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            Checkpoint("B_LOADFROMSTREAM_START — entering AssemblyLoadContext.LoadFromStream for exact transformed primary.");
            var assembly = LoadFromStream(stream);
            Checkpoint("B_LOADFROMSTREAM_PASS — AssemblyLoadContext.LoadFromStream returned transformed primary.");
            return assembly;
        }

        [UnconditionalSuppressMessage(
            "Trimming",
            "IL2026",
            Justification = "Step 35 resolves only exact persisted host bindings and hash-pinned initializer-free prepared private assemblies.")]
        protected override Assembly? Load(AssemblyName assemblyName)
        {
            var requestedFullName = assemblyName.FullName ?? assemblyName.Name ?? "<unknown>";
            Checkpoint($"RESOLVE_MANAGED_START — {requestedFullName}");
            ManagedResolverRequests.Add(requestedFullName);
            if (assemblyName.Name is null)
                return Reject(requestedFullName, "assembly request has no simple name");

            if (_privateBySimpleName.TryGetValue(assemblyName.Name, out var privateAssembly))
            {
                if (privateAssembly.ModuleInitializerCount > 0)
                {
                    var detail = $"{requestedFullName} => {privateAssembly.Plan.AssemblyFullName}; moduleInitializers={privateAssembly.ModuleInitializerCount}";
                    InitializerBearingRequests.Add(detail);
                    Checkpoint($"RESOLVE_INITIALIZER_BEARING_REJECT — {detail}");
                    throw new FileLoadException(
                        "Step 35 refuses initializer-bearing private dependencies during the ExecuteVeryEarly boundary: " + detail);
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

                Checkpoint($"RESOLVE_PRIVATE_HASH_START — {requestedFullName} => {privateAssembly.Plan.RelativePath}");
                VerifyFileLength(privateAssembly.PreparedPath, privateAssembly.Plan.Length, "prepared private dependency");
                var hash = ComputeSha1Hex(privateAssembly.PreparedPath);
                if (!hash.Equals(privateAssembly.Plan.Sha1Hex, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException("Step-35 prepared private dependency SHA-1 changed immediately before load: " + privateAssembly.Plan.RelativePath);
                Checkpoint($"RESOLVE_PRIVATE_HASH_PASS — {requestedFullName}; sha1={hash}");

                using var stream = new FileStream(privateAssembly.PreparedPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                Checkpoint($"RESOLVE_PRIVATE_LOADFROMSTREAM_START — {requestedFullName}");
                var loaded = LoadFromStream(stream);
                Checkpoint($"RESOLVE_PRIVATE_LOADFROMSTREAM_PASS — {requestedFullName}");
                var actualFullName = loaded.GetName().FullName ?? loaded.GetName().Name ?? string.Empty;
                if (!actualFullName.Equals(privateAssembly.Plan.AssemblyFullName, StringComparison.Ordinal))
                    throw new FileLoadException($"Step-35 private dependency loaded identity drifted. Planned '{privateAssembly.Plan.AssemblyFullName}', actual '{actualFullName}'.");
                PrivateLoads.Add($"{requestedFullName} => {actualFullName}");
                Checkpoint($"RESOLVE_PRIVATE_PASS — {requestedFullName} => {actualFullName}");
                return loaded;
            }

            var hostMatches = _hostBindings
                .Where(binding => ExactRequestedIdentity(assemblyName, new AssemblyName(binding.RequestedFullName)))
                .ToArray();
            if (hostMatches.Length == 0)
                return Reject(requestedFullName, "request is neither an exact planned host-framework binding nor an identified prepared private dependency");

            Checkpoint($"RESOLVE_HOST_START — {requestedFullName}");
            var allowedActual = hostMatches
                .Select(binding => binding.ActualFullName)
                .Distinct(StringComparer.Ordinal)
                .ToHashSet(StringComparer.Ordinal);
            var hostAssembly = AssemblyLoadContext.Default.LoadFromAssemblyName(assemblyName);
            var hostFullName = hostAssembly.GetName().FullName ?? hostAssembly.GetName().Name ?? string.Empty;
            if (!allowedActual.Contains(hostFullName))
                throw new FileLoadException(
                    $"Step-35 host binding drift for '{requestedFullName}'. Planned actual identity: {string.Join(" | ", allowedActual)}; runtime actual: {hostFullName}.");
            HostLoads.Add($"{requestedFullName} => {hostFullName}");
            Checkpoint($"RESOLVE_HOST_PASS — {requestedFullName} => {hostFullName}");
            return hostAssembly;
        }

        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            Checkpoint($"RESOLVE_NATIVE_REJECT — {unmanagedDllName}");
            NativeLoadAttempts.Add(unmanagedDllName);
            throw new DllNotFoundException(
                $"Step 35 controlled ExecuteVeryEarly boundary refuses native library resolution for '{unmanagedDllName}'.");
        }

        internal string FormatResolverState()
            => $"managed={ManagedResolverRequests.Count}; host={HostLoads.Count}; private={PrivateLoads.Count}; initializerBearing={InitializerBearingRequests.Count}; rejected={RejectedManagedRequests.Count}; native={NativeLoadAttempts.Count}" +
               $"; managedRequests=[{FormatItems(ManagedResolverRequests)}]; privateLoads=[{FormatItems(PrivateLoads)}]; initializerRequests=[{FormatItems(InitializerBearingRequests)}]; rejected=[{FormatItems(RejectedManagedRequests)}]; native=[{FormatItems(NativeLoadAttempts)}]";

        private static string FormatItems(IEnumerable<string> items)
        {
            var array = items.ToArray();
            return array.Length == 0 ? "<none>" : string.Join(" | ", array);
        }

        private Assembly? Reject(string requestedFullName, string reason)
        {
            var detail = $"{requestedFullName} — {reason}";
            RejectedManagedRequests.Add(detail);
            Checkpoint($"RESOLVE_MANAGED_REJECT — {detail}");
            throw new FileLoadException("Step-35 strict managed resolver rejected an unplanned request: " + detail);
        }

        private void Checkpoint(string detail)
            => TransformedRealStS2VeryEarlyInitialization.Checkpoint(_crashCheckpoint, detail);
    }

    private sealed class RejectingAssemblyResolver : IAssemblyResolver
    {
        internal List<string> Requests { get; } = [];

        public AssemblyDefinition Resolve(AssemblyNameReference name)
        {
            Requests.Add(name.FullName);
            throw new AssemblyResolutionException(name);
        }

        public AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters) => Resolve(name);
        public void Dispose() { }
    }
}
