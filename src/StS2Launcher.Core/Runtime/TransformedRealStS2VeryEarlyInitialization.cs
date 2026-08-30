using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.Loader;
using System.Security.Cryptography;
using System.Text.Json;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace StS2Launcher.Core;

/// <summary>
/// Step 35 boundary. Re-manufactures/reverifies the physically closed Step-32 transformed image,
/// emits a diagnostic-only clone that preserves identity/MVID while adding entry checkpoints to the
/// pre-first-await call chain, admits only that clone into a fresh execution-capable private
/// AssemblyLoadContext, then reflects and invokes the instrumented
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
    public const string DiagnosticBridgeTypeFullName = "StS2Launcher.Step35Diagnostics.ExecuteVeryEarlyCheckpointBridge";
    public const string DiagnosticBridgeCallbackFieldName = "Callback";
    private const string DiagnosticCloneFileName = "sts2.step35.0.8.instrumented.dll";
    private const string DiagnosticCecilWriteSystemRuntimeIdentity = "System.Runtime, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
    private const string DiagnosticCecilWriteSentryIdentity = "Sentry, Version=5.0.0.0, Culture=neutral, PublicKeyToken=fba2ec45388e2af0";
    private static readonly IReadOnlyDictionary<DiagnosticExternalConstantTypeKey, TypeCode> DiagnosticAuditedExternalConstantTypeRequirements =
        new Dictionary<DiagnosticExternalConstantTypeKey, TypeCode>
        {
            [new(DiagnosticCecilWriteSystemRuntimeIdentity, "System.Reflection.BindingFlags", false)] = TypeCode.Int32,
            [new(DiagnosticCecilWriteSentryIdentity, "Sentry.BreadcrumbLevel", false)] = TypeCode.Int32,
            [new(DiagnosticCecilWriteSentryIdentity, "Sentry.SentryLevel", false)] = TypeCode.Int16,
        };

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
            progress?.Report(new(gate, 0, 8, null,
                "Re-running the physically closed Step-32 A-D transform contract. No StS2 CLR admission or game invocation occurs in Gate A."));
            RequireRewritePass("Step 32 Gate A", await _rewrite.RunSourceAdmissionAndPrivateCloneAsync(cancellationToken: cancellationToken).ConfigureAwait(false));
            progress?.Report(new(gate, 1, 8, null, "Step-32 Gate A requalified."));
            RequireRewritePass("Step 32 Gate B", _rewrite.RunDeterministicStackNeutralRewrite());
            progress?.Report(new(gate, 2, 8, null, "Step-32 Gate B requalified; exact transformed bytes manufactured privately."));
            RequireRewritePass("Step 32 Gate C", _rewrite.RunTransformedImageVerification());
            progress?.Report(new(gate, 3, 8, null, "Step-32 Gate C requalified; transformed semantics independently reopened and verified."));
            RequireRewritePass("Step 32 Gate D", await _rewrite.RunFinalIsolationAuditAsync(cancellationToken: cancellationToken).ConfigureAwait(false));
            progress?.Report(new(gate, 4, 8, null, "Step-32 Gate D requalified; trusted source remains isolated."));

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
            string veryEarlyStaticInstructionMap;
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

                veryEarlyStaticInstructionMap = BuildStaticInstructionMap(transformedMethod, transformedMoveNext);
                transformedMethodToken = transformedMethod.MetadataToken.ToUInt32();
                transformedMoveNextToken = transformedMoveNext.MetadataToken.ToUInt32();
                if (sourceResolver.Requests.Count != 0 || transformedResolver.Requests.Count != 0)
                    throw new InvalidDataException("Step-35 source/transformed very-early metadata inspection unexpectedly resolved a dependency through Cecil.");
            }
            progress?.Report(new(gate, 5, 8, transformedPath,
                "Exact source/transformed ExecuteVeryEarly wrapper + async MoveNext semantics requalified; no direct ExecuteEssential/ExecuteDeferred/PrewarmJit or Harmony call crosses this boundary."));

            stage = "Step-35.0.8 diagnostic-clone instrumentation";
            var diagnosticRoot = Path.Combine(_launcherDataRoot, "Step35-ExecuteVeryEarlyDiagnostic");
            Directory.CreateDirectory(diagnosticRoot);
            var diagnosticPath = Path.Combine(diagnosticRoot, DiagnosticCloneFileName);
            var diagnostic = CreateInstrumentedDiagnosticClone(transformedPath, diagnosticPath);
            VerifyFileLength(transformedPath, TransformedRealStS2AssemblyAdmission.ClosedStep32TransformedBytes, "exact transformed primary after diagnostic-clone emission");
            var transformedSha256AfterDiagnosticEmission = ComputeSha256Hex(transformedPath);
            if (!transformedSha256AfterDiagnosticEmission.Equals(transformedSha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Step-35.0.8 diagnostic-clone emission changed the exact closed transformed source; refusing to continue.");
            progress?.Report(new(gate, 6, 8, diagnosticPath,
                $"Exact transformed image requalified, then a Step-35.0.8 diagnostic-only clone was emitted with {diagnostic.MarkerCount:N0} in-method entry markers. Cecil serialization used {diagnostic.WriteResolutionRequestCount:N0} bounded writer-only constant-metadata resolution request(s) across {diagnostic.ApprovedConstantScopeCount:N0} audited scope(s), then the clone reopened under rejecting resolution; the exact transformed source was immediately re-hashed unchanged."));

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

            progress?.Report(new(gate, 7, 8, _planPath, "Prepared runtime-binding plan and exact initializer-bearing boundary requalified; no prepared assembly has been CLR-loaded."));

            _preflight = new ExecutionPreflightSnapshot(
                transformedPath,
                transformedSha256,
                diagnostic.Path,
                diagnostic.Sha256,
                diagnostic.Length,
                diagnostic.MethodToken,
                diagnostic.MoveNextToken,
                diagnostic.MarkerCount,
                transformedMethodToken,
                transformedMoveNextToken,
                targetSemanticSha256,
                moveNextSemanticSha256,
                veryEarlyStaticInstructionMap,
                plan,
                planSha256,
                prepared,
                preparedPrimary,
                initializerBearing[0]);

            EnsureNoStS2Loaded("Gate A exit");
            progress?.Report(new(gate, 8, 8, _planPath, "Execution preflight complete; exact transformed source, instrumented diagnostic clone, and all prepared dependencies remain outside the CLR."));

            return Pass(gate,
                "EXACT CLOSED TRANSFORMED IMAGE, VERY-EARLY ASYNC STARTUP TARGET, AND EXECUTION RESOLVER PLAN REQUALIFIED; NO STS2 CLR LOAD OR GAME INVOCATION OCCURRED.\n" +
                "Physical Step-32 transform closure re-run: 4/4 PASS\n" +
                $"Source SHA-256: {sourceSha256}\n" +
                $"Transformed SHA-256: {transformedSha256}\n" +
                $"Transformed bytes: {TransformedRealStS2AssemblyAdmission.ClosedStep32TransformedBytes:N0}\n" +
                $"Step-35.0.8 diagnostic clone SHA-256: {diagnostic.Sha256}\n" +
                $"Step-35.0.8 diagnostic clone bytes: {diagnostic.Length:N0}\n" +
                $"Injected durable checkpoint markers: {diagnostic.MarkerCount:N0}\n" +
                $"Diagnostic constant-metadata fingerprint SHA-256: {diagnostic.ConstantMetadataSha256}\n" +
                $"Cecil writer-only constant-metadata resolution requests: {diagnostic.WriteResolutionRequestCount:N0}\n" +
                $"Cecil writer-only resolution identities: {diagnostic.WriteResolutionIdentities}\n" +
                $"Cecil writer-only synthetic constant types / approved scopes / approved requirements: {diagnostic.SyntheticConstantTypeCount:N0} / {diagnostic.ApprovedConstantScopeCount:N0} / {diagnostic.ApprovedConstantRequirementCount:N0}\n" +
                "External assembly bytes opened by the writer-only surrogate resolver: 0\n" +
                "Diagnostic reopen/verification dependency resolution requests: 0\n" +
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

    public string GetVerifiedVeryEarlyStaticInstructionMap()
    {
        ThrowIfDisposed();
        return RequirePreflight().VeryEarlyStaticInstructionMap;
    }

    [UnconditionalSuppressMessage(
        "Trimming",
        "IL2026",
        Justification = "Step 35.0.6 CLR-admits only the separately hash-pinned diagnostic clone after re-verifying the exact transformed source; member reflection/invocation remains deferred to Gate C.")]
    public TransformedRealStS2VeryEarlyInitializationGateResult RunExecutionCapableClrAdmission()
        => RunExecutionCapableClrAdmission(crashCheckpoint: null);

    [UnconditionalSuppressMessage(
        "Trimming",
        "IL2026",
        Justification = "Step 35.0.6 re-verifies the exact closed transformed source but CLR-admits only the separately hash-pinned diagnostic clone; the callback remains output-only crash telemetry.")]
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

            stage = "immediate exact-transformed and diagnostic-clone hash recheck";
            Checkpoint(crashCheckpoint, "B_HASH_START — rechecking the exact closed transformed primary and the Step-35.0.8 instrumented diagnostic clone before CLR admission.");
            VerifyFileLength(preflight.TransformedPath, TransformedRealStS2AssemblyAdmission.ClosedStep32TransformedBytes, "exact transformed primary");
            var exactImmediateSha256 = ComputeSha256Hex(preflight.TransformedPath);
            if (!exactImmediateSha256.Equals(TransformedRealStS2AssemblyAdmission.ClosedStep32TransformedSha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Step-35 exact transformed image changed between Gate A verification and Gate B CLR admission.");
            VerifyFileLength(preflight.DiagnosticPath, preflight.DiagnosticLength, "Step-35.0.8 instrumented diagnostic clone");
            var immediateSha256 = ComputeSha256Hex(preflight.DiagnosticPath);
            if (!immediateSha256.Equals(preflight.DiagnosticSha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Step-35.0.8 diagnostic clone changed between Gate A instrumentation and Gate B CLR admission.");
            Checkpoint(crashCheckpoint, $"B_HASH_PASS — exact transformed source still matched {exactImmediateSha256}; instrumented diagnostic clone matched {immediateSha256}.");

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

            stage = "instrumented diagnostic sts2.dll LoadFromStream";
            Checkpoint(crashCheckpoint, "B_LOADPRIMARY_START — entering Step-35.0.8 instrumented diagnostic-clone LoadPrimary/LoadFromStream path; exact closed transformed source remains untouched on disk.");
            var assembly = context.LoadPrimary(preflight.DiagnosticPath, immediateSha256);
            Checkpoint(crashCheckpoint, "B_LOADPRIMARY_PASS — instrumented diagnostic clone returned from LoadPrimary/LoadFromStream.");
            if (!ReferenceEquals(AssemblyLoadContext.GetLoadContext(assembly), context))
                throw new InvalidDataException("The Step-35.0.8 diagnostic sts2.dll clone did not load into the dedicated Step-35 AssemblyLoadContext.");
            Checkpoint(crashCheckpoint, "B_CONTEXT_OWNERSHIP_PASS — instrumented diagnostic clone belongs to the dedicated Step-35 AssemblyLoadContext.");

            Checkpoint(crashCheckpoint, "B_GETNAME_START — reading loaded diagnostic-clone assembly identity.");
            var actualIdentity = assembly.GetName().FullName ?? assembly.GetName().Name ?? string.Empty;
            if (!actualIdentity.Equals(TransformedRealStS2AssemblyAdmission.ClosedStep32AssemblyIdentity, StringComparison.Ordinal))
                throw new InvalidDataException($"Loaded diagnostic-clone identity mismatch. Expected '{TransformedRealStS2AssemblyAdmission.ClosedStep32AssemblyIdentity}', actual '{actualIdentity}'.");
            Checkpoint(crashCheckpoint, $"B_GETNAME_PASS — loaded diagnostic-clone identity matched: {actualIdentity}.");
            Checkpoint(crashCheckpoint, "B_MVID_START — reading loaded diagnostic-clone module MVID.");
            var actualMvid = assembly.ManifestModule.ModuleVersionId;
            if (actualMvid != TransformedRealStS2AssemblyAdmission.ClosedStep32Mvid)
                throw new InvalidDataException($"Loaded diagnostic-clone module MVID mismatch. Expected {TransformedRealStS2AssemblyAdmission.ClosedStep32Mvid}, actual {actualMvid}.");
            Checkpoint(crashCheckpoint, $"B_MVID_PASS — loaded diagnostic-clone MVID matched the exact transformed source: {actualMvid}.");
            if (context.ManagedResolverRequests.Count != 0 || context.PrivateLoads.Count != 0 ||
                context.InitializerBearingRequests.Count != 0 || context.RejectedManagedRequests.Count != 0 || context.NativeLoadAttempts.Count != 0)
            {
                throw new InvalidDataException(
                    "Step-35.0.8 diagnostic-clone admission no longer matches the physically closed Step-33 zero-resolution admission behavior. " +
                    context.FormatResolverState());
            }
            Checkpoint(crashCheckpoint, "B_ZERO_RESOLUTION_PASS — primary admission produced zero managed/private/initializer/rejected/native resolution activity.");

            var matches = FindLoadedStS2Assemblies();
            if (matches.Length != 1 || !ReferenceEquals(matches[0], assembly))
                throw new InvalidDataException($"Expected exactly one diagnostic sts2 assembly after Step-35 Gate B, found {matches.Length}.");
            Checkpoint(crashCheckpoint, "B_GLOBAL_RESIDENCY_PASS — exactly one sts2 assembly is resident and it is the instrumented diagnostic clone.");
            var contextAssemblies = context.Assemblies.ToArray();
            if (contextAssemblies.Length != 1 || !ReferenceEquals(contextAssemblies[0], assembly))
                throw new InvalidDataException($"Step-35 context contains {contextAssemblies.Length} private assemblies immediately after admission instead of exactly the instrumented diagnostic sts2 clone.");
            Checkpoint(crashCheckpoint, "B_PRIVATE_CONTEXT_ENUM_PASS — private context contains exactly the instrumented diagnostic clone after admission.");

            _admission = new PrimaryAdmissionSnapshot(assembly, actualIdentity, actualMvid, immediateSha256);
            Checkpoint(crashCheckpoint, "B_PASS_RETURN — Gate B completed successfully and is returning its PASS result.");

            return Pass(gate,
                "STEP-33 ZERO-RESOLUTION ADMISSION BEHAVIOR RE-ESTABLISHED FOR THE STEP-35.0.8 INSTRUMENTED DIAGNOSTIC CLONE; NO GAME MEMBER REFLECTION/INVOCATION YET.\n" +
                $"Loaded identity: {actualIdentity}\n" +
                $"Loaded MVID: {actualMvid}\n" +
                $"AssemblyLoadContext: {context.Name ?? LoadContextName}\n" +
                $"Instrumented diagnostic-clone SHA-256 immediately before LoadFromStream: {immediateSha256}\n" +
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
        Justification = "Step 35.0.6 deliberately reflects and invokes one instrumented diagnostic clone of the exact async initialization method after re-verifying the exact transformed source. The dynamic payload is preserved by the physical copy/no-link runtime policy.")]
    [UnconditionalSuppressMessage(
        "Trimming",
        "IL2026",
        Justification = "Step 35.0.6 reflects and invokes only the separately verified diagnostic clone of ExecuteVeryEarly; this derivative is localization evidence and is never exact Step-35 closure evidence.")]
    public async Task<TransformedRealStS2VeryEarlyInitializationGateResult> RunDiagnosticExecuteVeryEarlyInvocationAsync(
        Action<string>? crashCheckpoint,
        CancellationToken cancellationToken = default)
    {
        const TransformedRealStS2VeryEarlyInitializationGate gate = TransformedRealStS2VeryEarlyInitializationGate.DiagnosticExecuteVeryEarlyInvocation;
        var stage = "initialization";
        try
        {
            if (crashCheckpoint is null)
                throw new InvalidOperationException("Step-35.0.8 diagnostic Gate C requires a durable launcher-owned checkpoint callback; refusing to execute an instrumented clone without in-method telemetry.");
            Checkpoint(crashCheckpoint, "C_ENTRY — entered Gate C diagnostic-clone ExecuteVeryEarly binding/invocation/await boundary; exact transformed source remains outside the CLR.");
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

            stage = "instrumented diagnostic very-early type/member binding";
            Checkpoint(crashCheckpoint, "C_BIND_TYPE_START — calling Assembly.GetType for exact OneTimeInitialization target.");
            var targetType = admission.Assembly.GetType(TargetTypeFullName, throwOnError: true, ignoreCase: false)
                ?? throw new MissingMemberException(TargetTypeFullName);
            if (!ReferenceEquals(targetType.Assembly, admission.Assembly))
                throw new InvalidDataException("Step-35.0.8 target type did not bind from the admitted diagnostic sts2 clone.");
            Checkpoint(crashCheckpoint, "C_BIND_TYPE_PASS — OneTimeInitialization target type bound from the separately verified diagnostic sts2 clone.");

            Checkpoint(crashCheckpoint, "C_BIND_METHOD_START — calling Type.GetMethod for the diagnostic clone's static parameterless ExecuteVeryEarly.");
            var method = targetType.GetMethod(
                TargetMethodName,
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                binder: null,
                types: Type.EmptyTypes,
                modifiers: null)
                ?? throw new MissingMethodException(TargetTypeFullName, TargetMethodName);
            Checkpoint(crashCheckpoint, "C_BIND_METHOD_PASS — diagnostic-clone ExecuteVeryEarly MethodInfo binding returned.");

            if (!ReferenceEquals(method.DeclaringType, targetType) || !method.IsStatic || method.ReturnType != typeof(Task) || method.GetParameters().Length != 0)
                throw new InvalidDataException("Step-35.0.8 reflected diagnostic ExecuteVeryEarly identity/signature drifted from the exact static parameterless System.Threading.Tasks.Task target.");
            Checkpoint(crashCheckpoint, "C_SIGNATURE_PASS — reflected instrumented method retains the exact static parameterless Task-returning target contract.");
            if (method.MetadataToken != unchecked((int)preflight.DiagnosticMethodToken))
                throw new InvalidDataException($"Step-35.0.8 reflected instrumented ExecuteVeryEarly token drifted: 0x{method.MetadataToken:X8} != preflight diagnostic 0x{preflight.DiagnosticMethodToken:X8}.");
            Checkpoint(crashCheckpoint, $"C_TOKEN_PASS — reflected ExecuteVeryEarly token matched 0x{method.MetadataToken:X8}.");
            if (method.Module.ModuleVersionId != TransformedRealStS2AssemblyAdmission.ClosedStep32Mvid)
                throw new InvalidDataException("Step-35 reflected ExecuteVeryEarly module MVID drifted from the closed transformed image.");
            Checkpoint(crashCheckpoint, $"C_MVID_PASS — reflected diagnostic-clone ExecuteVeryEarly module MVID matched {method.Module.ModuleVersionId}.");

            stage = "Step-35.0.8 in-method checkpoint bridge arm";
            var bridgeType = admission.Assembly.GetType(DiagnosticBridgeTypeFullName, throwOnError: true, ignoreCase: false)
                ?? throw new MissingMemberException(DiagnosticBridgeTypeFullName);
            var bridgeField = bridgeType.GetField(DiagnosticBridgeCallbackFieldName, BindingFlags.Static | BindingFlags.Public)
                ?? throw new MissingFieldException(DiagnosticBridgeTypeFullName, DiagnosticBridgeCallbackFieldName);
            if (bridgeField.FieldType != typeof(Action<string>))
                throw new InvalidDataException($"Step-35.0.8 diagnostic bridge field type drifted: {bridgeField.FieldType.FullName}.");
            bridgeField.SetValue(null, crashCheckpoint);
            Checkpoint(crashCheckpoint, $"C_DIAGNOSTIC_BRIDGE_ARMED — instrumented diagnostic clone callback armed; markerCount={preflight.DiagnosticMarkerCount}. The next durable INMETHOD_* record is emitted from inside the executing sts2.dll method body.");

            stage = "single instrumented diagnostic ExecuteVeryEarly invocation";
            Task task;
            try
            {
                Checkpoint(crashCheckpoint, "C_INVOKE_START — entering the first and only MethodInfo.Invoke(null, null) for the Step-35.0.8 instrumented ExecuteVeryEarly diagnostic clone.");
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
                    "Step-35.0.8 instrumented ExecuteVeryEarly threw synchronously during the first controlled invocation. " +
                    DescribeException(target) + "\nResolver state at failure: " + context.FormatResolverState(), target);
            }

            stage = "await diagnostic-clone ExecuteVeryEarly Task completion";
            try
            {
                Checkpoint(crashCheckpoint, "C_WAIT_START — awaiting the diagnostic clone's returned ExecuteVeryEarly Task with the unchanged predeclared 60-second boundary.");
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
                    "Step-35.0.8 diagnostic-clone ExecuteVeryEarly Task faulted during the controlled await. " +
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
                "STEP-35.0.8 DIAGNOSTIC-CLONE EXECUTEVERYEARLY INVOCATION/AWAIT COMPLETED NORMALLY; THIS IS LOCALIZATION EVIDENCE, NOT EXACT STEP-35 CLOSURE.\n" +
                $"Target type: {TargetTypeFullName}\n" +
                $"Target method: {TargetMethodFullName}\n" +
                $"Reflected diagnostic-clone MethodDef token: 0x{method.MetadataToken:X8}\n" +
                $"Preflight diagnostic-clone async MoveNext token: 0x{preflight.DiagnosticMoveNextToken:X8}\n" +
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
            progress?.Report(new(gate, 0, 4, null, "Re-proving the receipt-backed install after instrumented diagnostic ExecuteVeryEarly initialization."));
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
            progress?.Report(new(gate, 1, 4, trustedPrimaryPath, "Trusted receipt-backed primary remains byte-identical."));

            stage = "transformed image / plan / dependency hash reproof";
            var transformedSha256 = ComputeSha256Hex(preflight.TransformedPath);
            if (!transformedSha256.Equals(TransformedRealStS2AssemblyAdmission.ClosedStep32TransformedSha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Verified exact transformed sts2.dll changed after ExecuteVeryEarly diagnostic execution.");
            var diagnosticSha256 = ComputeSha256Hex(preflight.DiagnosticPath);
            if (!diagnosticSha256.Equals(preflight.DiagnosticSha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Step-35.0.8 instrumented diagnostic clone changed during ExecuteVeryEarly execution.");
            progress?.Report(new(gate, 2, 4, preflight.DiagnosticPath, "Exact transformed source and instrumented diagnostic clone remain byte-identical to their Gate-A hashes."));
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
                throw new InvalidDataException("Step-35.0.8 diagnostic-clone CLR residency/context ownership drifted during final audit.");
            if (execution.MethodToken != unchecked((int)preflight.DiagnosticMethodToken))
                throw new InvalidDataException("Step-35.0.8 execution snapshot diagnostic ExecuteVeryEarly token drifted during final audit.");

            progress?.Report(new(gate, 4, 4, preflight.DiagnosticPath, "Final source/diagnostic-clone/plan/dependency/context isolation checks passed."));

            return Pass(gate,
                "STEP-35.0.8 DIAGNOSTIC-CLONE FINAL ISOLATION AUDIT PASSED; THIS DOES NOT CLOSE EXACT STEP 35.\n" +
                $"Post-execution OfflineReady: PASS ({offline.VerifiedFiles:N0}/{offline.PlannedFiles:N0} files)\n" +
                $"Receipt-backed original SHA-256 unchanged: {trustedSha256}\n" +
                $"Verified exact transformed SHA-256 unchanged: {transformedSha256}\n" +
                $"Instrumented diagnostic clone SHA-256 unchanged: {diagnosticSha256}\n" +
                $"Runtime-binding plan SHA-256 unchanged: {planSha256}\n" +
                $"Unique resident sts2 identity: {admission.AssemblyFullName}\n" +
                $"Resident sts2 AssemblyLoadContext: {context.Name ?? LoadContextName}\n" +
                "Resident sts2 load input: Step-35.0.8 instrumented diagnostic clone derived from the reverified exact Step-32 transformed image\n" +
                $"Initializer-free prepared private dependencies resident and re-hashed: {verifiedPrivate:N0}\n" +
                $"Managed resolver requests total: {context.ManagedResolverRequests.Count:N0}\n" +
                $"Exact planned host-framework loads total: {context.HostLoads.Count:N0}\n" +
                $"Prepared private dependency loads total: {context.PrivateLoads.Count:N0}\n" +
                "Initializer-bearing private dependency requests: 0\n" +
                "Unplanned managed resolution: NO\n" +
                "Native game resolution/loading: NO\n" +
                "Instrumented diagnostic ExecuteVeryEarly invocation count: 1\n" +
                "Receipt-backed/prepared original sts2.dll CLR-loaded: NO\n" +
                "Game entry point / ExecuteEssential / ExecuteDeferred intentionally invoked by launcher: NO\n" +
                "Harmony/MonoMod runtime patching intentionally invoked by launcher: NO\n" +
                "Godot/game startup intentionally requested by launcher: NO\n" +
                "After a 0.0.131 diagnostic 4/4 result, Step 35 remains OPEN. Use the localization evidence to design a separately defined compatibility candidate, then return to an explicitly authoritative transformed artifact for physical closure testing.");
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

    internal static string BuildStaticInstructionMap(MethodDefinition wrapper, MethodDefinition moveNext)
    {
        if (!wrapper.HasBody || !moveNext.HasBody)
            throw new InvalidDataException("Step-35 static instruction map requires managed IL for wrapper and MoveNext.");

        var lines = new List<string>
        {
            "StS2 Launcher — Step 35 ExecuteVeryEarly static IL/callsite map",
            "Diagnostic-only output derived from the exact transformed image before CLR admission; never consumed as trusted runtime input.",
            $"Wrapper: token=0x{wrapper.MetadataToken.ToUInt32():X8}; {wrapper.FullName}",
            $"Wrapper instructions={wrapper.Body.Instructions.Count}; handlers={wrapper.Body.ExceptionHandlers.Count}; locals={wrapper.Body.Variables.Count}",
            $"MoveNext: token=0x{moveNext.MetadataToken.ToUInt32():X8}; {moveNext.FullName}",
            $"MoveNext instructions={moveNext.Body.Instructions.Count}; handlers={moveNext.Body.ExceptionHandlers.Count}; locals={moveNext.Body.Variables.Count}",
            "Legend: CALLSITE marks call/callvirt/newobj; AWAIT-CANDIDATE marks Async*MethodBuilder await registration; scope is metadata scope only (no Cecil Resolve).",
            string.Empty,
            "[WRAPPER IL]",
        };
        AppendInstructionMap(lines, wrapper);
        lines.Add(string.Empty);
        lines.Add("[MOVENEXT IL]");
        AppendInstructionMap(lines, moveNext);
        return string.Join("\n", lines);
    }

    private static void AppendInstructionMap(List<string> lines, MethodDefinition method)
    {
        var callIndex = 0;
        foreach (var instruction in method.Body.Instructions)
        {
            var tags = new List<string>();
            if (instruction.OpCode.Code is Code.Call or Code.Callvirt or Code.Newobj)
            {
                callIndex++;
                tags.Add($"CALLSITE#{callIndex:D3}");
            }

            if (instruction.Operand is MethodReference methodReference &&
                (methodReference.Name.Contains("AwaitUnsafeOnCompleted", StringComparison.Ordinal) ||
                 methodReference.Name.Contains("AwaitOnCompleted", StringComparison.Ordinal)))
            {
                tags.Add("AWAIT-CANDIDATE");
            }

            var tagText = tags.Count == 0 ? string.Empty : " [" + string.Join(",", tags) + "]";
            lines.Add($"IL_{instruction.Offset:X4}: {instruction.OpCode.Name}{tagText}{DescribeInstructionOperand(instruction.Operand)}");
        }
    }

    private static string DescribeInstructionOperand(object? operand)
        => operand switch
        {
            null => string.Empty,
            MethodReference method => $" | method={method.FullName} | token=0x{method.MetadataToken.ToUInt32():X8} | scope={DescribeMetadataScope(method.DeclaringType.Scope)}",
            FieldReference field => $" | field={field.FullName} | token=0x{field.MetadataToken.ToUInt32():X8} | scope={DescribeMetadataScope(field.DeclaringType.Scope)}",
            TypeReference type => $" | type={type.FullName} | token=0x{type.MetadataToken.ToUInt32():X8} | scope={DescribeMetadataScope(type.Scope)}",
            Instruction target => $" | target=IL_{target.Offset:X4}",
            Instruction[] targets => " | targets=" + string.Join(",", targets.Select(target => $"IL_{target.Offset:X4}")),
            VariableDefinition variable => $" | local=V_{variable.Index}:{variable.VariableType.FullName}",
            ParameterDefinition parameter => $" | parameter={parameter.Index}:{parameter.ParameterType.FullName}",
            string text => $" | string={JsonSerializer.Serialize(text)}",
            _ => $" | operand={operand}",
        };

    private static string DescribeMetadataScope(IMetadataScope? scope)
        => scope switch
        {
            AssemblyNameReference assembly => assembly.FullName,
            ModuleDefinition module => module.Assembly?.Name.FullName ?? module.Name,
            ModuleReference module => module.Name,
            null => "<none>",
            _ => scope.ToString() ?? "<unknown>",
        };

    private static (string TypeName, string MethodFullName, string Marker)[] GetDiagnosticMarkerTargets() =>
    [
        (TargetTypeFullName + "/" + TargetStateMachineTypeName, "System.Void MegaCrit.Sts2.Core.Helpers.OneTimeInitialization/<ExecuteVeryEarly>d__7::MoveNext()", "INMETHOD_001 — ExecuteVeryEarly.MoveNext entered"),
        ("MegaCrit.Sts2.Core.TestSupport.TestMode", "System.Boolean MegaCrit.Sts2.Core.TestSupport.TestMode::get_IsOn()", "INMETHOD_010 — TestMode.get_IsOn entered"),
        ("MegaCrit.Sts2.Core.Saves.SaveManager", "MegaCrit.Sts2.Core.Saves.SaveManager MegaCrit.Sts2.Core.Saves.SaveManager::get_Instance()", "INMETHOD_020 — SaveManager.get_Instance entered"),
        ("MegaCrit.Sts2.Core.Saves.SaveManager", "MegaCrit.Sts2.Core.Saves.SaveManager MegaCrit.Sts2.Core.Saves.SaveManager::ConstructDefault()", "INMETHOD_021 — SaveManager.ConstructDefault entered"),
        ("MegaCrit.Sts2.Core.Saves.UserDataPathProvider", "System.String MegaCrit.Sts2.Core.Saves.UserDataPathProvider::GetAccountScopedBasePath(System.String,System.Nullable`1<MegaCrit.Sts2.Core.Platform.PlatformType>,System.Nullable`1<System.UInt64>)", "INMETHOD_022 — UserDataPathProvider.GetAccountScopedBasePath entered"),
        ("MegaCrit.Sts2.Core.Platform.PlatformUtil", "MegaCrit.Sts2.Core.Platform.PlatformType MegaCrit.Sts2.Core.Platform.PlatformUtil::get_PrimaryPlatform()", "INMETHOD_023 — PlatformUtil.get_PrimaryPlatform entered"),
        ("MegaCrit.Sts2.Core.Platform.Null.NullPlatformUtilStrategy", "System.Void MegaCrit.Sts2.Core.Platform.Null.NullPlatformUtilStrategy::.ctor()", "INMETHOD_024 — NullPlatformUtilStrategy..ctor entered"),
        ("MegaCrit.Sts2.Core.Saves.GodotFileIo", "System.Void MegaCrit.Sts2.Core.Saves.GodotFileIo::.ctor(System.String)", "INMETHOD_025 — GodotFileIo..ctor entered"),
        ("MegaCrit.Sts2.Core.Saves.GodotFileIo", "System.Void MegaCrit.Sts2.Core.Saves.GodotFileIo::CreateDirectory(System.String)", "INMETHOD_026 — GodotFileIo.CreateDirectory entered"),
        ("MegaCrit.Sts2.Core.Saves.SaveManager", "MegaCrit.Sts2.Core.Saves.ReadSaveResult`1<MegaCrit.Sts2.Core.Saves.SettingsSave> MegaCrit.Sts2.Core.Saves.SaveManager::InitSettingsDataForTest()", "INMETHOD_030 — SaveManager.InitSettingsDataForTest entered"),
        ("MegaCrit.Sts2.Core.Saves.SaveManager", "MegaCrit.Sts2.Core.Saves.ReadSaveResult`1<MegaCrit.Sts2.Core.Saves.SettingsSave> MegaCrit.Sts2.Core.Saves.SaveManager::InitSettingsData()", "INMETHOD_031 — SaveManager.InitSettingsData entered"),
        (TargetTypeFullName, "System.Void MegaCrit.Sts2.Core.Helpers.OneTimeInitialization::set_SettingsReadResult(MegaCrit.Sts2.Core.Saves.ReadSaveResult`1<MegaCrit.Sts2.Core.Saves.SettingsSave>)", "INMETHOD_040 — OneTimeInitialization.set_SettingsReadResult entered"),
        ("MegaCrit.Sts2.Core.Modding.ModManagerFileIo", "System.Void MegaCrit.Sts2.Core.Modding.ModManagerFileIo::.ctor()", "INMETHOD_050 — ModManagerFileIo..ctor entered"),
        ("MegaCrit.Sts2.Core.Saves.SaveManager", "MegaCrit.Sts2.Core.Saves.SettingsSave MegaCrit.Sts2.Core.Saves.SaveManager::get_SettingsSave()", "INMETHOD_060 — SaveManager.get_SettingsSave entered"),
        ("MegaCrit.Sts2.Core.Saves.SettingsSave", "MegaCrit.Sts2.Core.Modding.ModSettings MegaCrit.Sts2.Core.Saves.SettingsSave::get_ModSettings()", "INMETHOD_070 — SettingsSave.get_ModSettings entered"),
        ("MegaCrit.Sts2.Core.Debug.ReleaseInfoManager", "MegaCrit.Sts2.Core.Debug.ReleaseInfoManager MegaCrit.Sts2.Core.Debug.ReleaseInfoManager::get_Instance()", "INMETHOD_080 — ReleaseInfoManager.get_Instance entered"),
        ("MegaCrit.Sts2.Core.Debug.ReleaseInfoManager", "MegaCrit.Sts2.Core.Debug.SemanticVersion MegaCrit.Sts2.Core.Debug.ReleaseInfoManager::get_SemVer()", "INMETHOD_090 — ReleaseInfoManager.get_SemVer entered"),
        ("MegaCrit.Sts2.Core.Modding.ModManager", "System.Threading.Tasks.Task MegaCrit.Sts2.Core.Modding.ModManager::Initialize(MegaCrit.Sts2.Core.Modding.IModManagerFileIo,MegaCrit.Sts2.Core.Modding.ModSettings,MegaCrit.Sts2.Core.Debug.SemanticVersion)", "INMETHOD_100 — ModManager.Initialize entered"),
    ];

    private static (string TypeName, string MethodFullName, string CalleeFullName, string BeforeMarker, string AfterMarker)[] GetDiagnosticCallsiteMarkerTargets() =>
    [
        ("MegaCrit.Sts2.Core.Saves.GodotFileIo", "System.Void MegaCrit.Sts2.Core.Saves.GodotFileIo::CreateDirectory(System.String)", "System.Boolean Godot.DirAccess::DirExistsAbsolute(System.String)", "INMETHOD_180 — GodotFileIo.CreateDirectory before Godot.DirAccess.DirExistsAbsolute", "INMETHOD_181 — GodotFileIo.CreateDirectory after Godot.DirAccess.DirExistsAbsolute"),
        ("MegaCrit.Sts2.Core.Saves.GodotFileIo", "System.Void MegaCrit.Sts2.Core.Saves.GodotFileIo::CreateDirectory(System.String)", "Godot.Error Godot.DirAccess::MakeDirRecursiveAbsolute(System.String)", "INMETHOD_182 — GodotFileIo.CreateDirectory before Godot.DirAccess.MakeDirRecursiveAbsolute", "INMETHOD_183 — GodotFileIo.CreateDirectory after Godot.DirAccess.MakeDirRecursiveAbsolute"),
    ];

    private static DiagnosticCloneSnapshot CreateInstrumentedDiagnosticClone(string exactTransformedPath, string diagnosticPath)
    {
        if (File.Exists(diagnosticPath))
            File.Delete(diagnosticPath);

        string expectedConstantMetadataSha256;
        int writeResolutionRequestCount;
        int syntheticConstantTypeCount;
        int approvedConstantScopeCount;
        int approvedConstantRequirementCount;
        string writeResolutionIdentities;

        // Cecil serialization of the real sts2 image is known to query external enum metadata for
        // constant-bearing fields/properties/parameters. Step 32 physically proved a bounded resolver
        // that answers only those exact audited metadata queries with in-memory surrogate types and
        // never opens external assembly bytes. Reuse that writer-only resolver here; all reopen/runtime
        // phases return to rejecting/fail-closed resolution.
        using var resolver = new DiagnosticConstantMetadataWriteResolver();
        // Critical ordering: this must remain a deferred read. Immediate mode can force Cecil to
        // materialize constant-bearing metadata while the writer-only resolver is intentionally still
        // unconfigured, which physically caused the 0.0.128 Gate-A System.Runtime resolution failure.
        // This mirrors the physically closed Step-32 sequence: deferred open -> audit/configure -> write.
        using (var module = ModuleDefinition.ReadModule(exactTransformedPath, new ReaderParameters
               {
                   ReadSymbols = false,
                   ReadingMode = ReadingMode.Deferred,
                   AssemblyResolver = resolver,
               }))
        {
            if (resolver.Requests.Count != 0)
                throw new InvalidDataException("Step-35.0.8 diagnostic deferred-open unexpectedly resolved a dependency before the bounded writer resolver was configured.");

            var constantPlan = resolver.Configure(module);
            expectedConstantMetadataSha256 = RealStS2PrepareMethodRewrite.ComputeConstantMetadataFingerprint(module);
            syntheticConstantTypeCount = constantPlan.SyntheticTypeCount;
            approvedConstantScopeCount = constantPlan.ApprovedScopeCount;
            approvedConstantRequirementCount = constantPlan.ApprovedRequirementCount;

            if (EnumerateTypes(module.Types).Any(type => type.FullName == DiagnosticBridgeTypeFullName))
                throw new InvalidDataException("Step-35.0.8 diagnostic bridge type already exists in the exact transformed image.");

            var bridge = new TypeDefinition(
                "StS2Launcher.Step35Diagnostics",
                "ExecuteVeryEarlyCheckpointBridge",
                Mono.Cecil.TypeAttributes.Class | Mono.Cecil.TypeAttributes.Abstract | Mono.Cecil.TypeAttributes.Sealed |
                Mono.Cecil.TypeAttributes.Public | Mono.Cecil.TypeAttributes.BeforeFieldInit,
                module.TypeSystem.Object);
            module.Types.Add(bridge);

            var systemRuntime = module.AssemblyReferences
                .Where(reference => reference.Name == "System.Runtime")
                .OrderByDescending(reference => reference.Version)
                .FirstOrDefault()
                ?? throw new InvalidDataException("Step-35.0.8 diagnostic clone requires the existing System.Runtime metadata scope.");
            var (actionStringType, invoke) = CreateDiagnosticActionStringInvokeReference(module, systemRuntime);
            var callbackField = new FieldDefinition(
                DiagnosticBridgeCallbackFieldName,
                Mono.Cecil.FieldAttributes.Public | Mono.Cecil.FieldAttributes.Static,
                actionStringType);
            bridge.Fields.Add(callbackField);

            var emit = new MethodDefinition(
                "Emit",
                Mono.Cecil.MethodAttributes.Public | Mono.Cecil.MethodAttributes.Static | Mono.Cecil.MethodAttributes.HideBySig,
                module.TypeSystem.Void);
            emit.Parameters.Add(new ParameterDefinition("marker", Mono.Cecil.ParameterAttributes.None, module.TypeSystem.String));
            bridge.Methods.Add(emit);
            var emitIl = emit.Body.GetILProcessor();
            var haveCallback = Instruction.Create(OpCodes.Nop);
            emitIl.Append(Instruction.Create(OpCodes.Ldsfld, callbackField));
            emitIl.Append(Instruction.Create(OpCodes.Dup));
            emitIl.Append(Instruction.Create(OpCodes.Brtrue_S, haveCallback));
            emitIl.Append(Instruction.Create(OpCodes.Pop));
            emitIl.Append(Instruction.Create(OpCodes.Ret));
            emitIl.Append(haveCallback);
            emitIl.Append(Instruction.Create(OpCodes.Ldarg_0));
            emitIl.Append(Instruction.Create(OpCodes.Callvirt, invoke));
            emitIl.Append(Instruction.Create(OpCodes.Ret));

            var emitReference = module.ImportReference(emit);
            var markers = GetDiagnosticMarkerTargets();

            var markerCount = 0;
            foreach (var item in markers)
            {
                var type = EnumerateTypes(module.Types).SingleOrDefault(candidate => candidate.FullName == item.TypeName)
                    ?? throw new MissingMemberException($"Step-35.0.8 diagnostic marker target type missing: {item.TypeName}.");
                var methods = type.Methods.Where(method => method.FullName == item.MethodFullName && method.HasBody).ToArray();
                if (methods.Length != 1)
                    throw new MissingMethodException($"Step-35.0.8 expected exactly one managed-IL marker target {item.MethodFullName}, found {methods.Length}.");
                InsertEntryMarker(methods[0], emitReference, item.Marker);
                markerCount++;

                var cctor = type.Methods.SingleOrDefault(method => method.Name == ".cctor" && method.IsStatic && method.HasBody);
                if (cctor is not null)
                {
                    var cctorMarker = $"INMETHOD_CCTOR — {item.TypeName}..cctor entered";
                    if (!HasInjectedEntryMarker(cctor, cctorMarker))
                    {
                        InsertEntryMarker(cctor, emitReference, cctorMarker);
                        markerCount++;
                    }
                }
            }

            foreach (var item in GetDiagnosticCallsiteMarkerTargets())
            {
                var type = EnumerateTypes(module.Types).SingleOrDefault(candidate => candidate.FullName == item.TypeName)
                    ?? throw new MissingMemberException($"Step-35.0.8 diagnostic callsite target type missing: {item.TypeName}.");
                var method = type.Methods.SingleOrDefault(candidate => candidate.FullName == item.MethodFullName && candidate.HasBody)
                    ?? throw new MissingMethodException($"Step-35.0.8 diagnostic callsite target method missing: {item.MethodFullName}.");
                InsertCallsiteMarkers(method, emitReference, item.CalleeFullName, item.BeforeMarker, item.AfterMarker);
                markerCount += 2;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(diagnosticPath) ?? throw new InvalidOperationException("Diagnostic clone path has no parent."));
            module.Write(diagnosticPath, new WriterParameters { WriteSymbols = false });
            resolver.ValidateWriteRequests();
            writeResolutionRequestCount = resolver.Requests.Count;
            writeResolutionIdentities = string.Join(" | ", resolver.Requests
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal));
        }

        var length = new FileInfo(diagnosticPath).Length;
        var sha256 = ComputeSha256Hex(diagnosticPath);
        using var verifyResolver = new RejectingAssemblyResolver();
        using var verifyModule = ModuleDefinition.ReadModule(diagnosticPath, new ReaderParameters
        {
            ReadSymbols = false,
            ReadingMode = ReadingMode.Deferred,
            AssemblyResolver = verifyResolver,
        });
        if (verifyResolver.Requests.Count != 0)
            throw new InvalidDataException("Step-35.0.8 diagnostic clone verification unexpectedly resolved a dependency.");
        var verifiedConstantMetadataSha256 = RealStS2PrepareMethodRewrite.ComputeConstantMetadataFingerprint(verifyModule);
        if (!verifiedConstantMetadataSha256.Equals(expectedConstantMetadataSha256, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Step-35.0.8 diagnostic clone changed the exact transformed image's constant metadata semantics during Cecil serialization.");
        if (verifyModule.Assembly?.Name.FullName != TransformedRealStS2AssemblyAdmission.ClosedStep32AssemblyIdentity ||
            verifyModule.Mvid != TransformedRealStS2AssemblyAdmission.ClosedStep32Mvid)
            throw new InvalidDataException("Step-35.0.8 diagnostic clone changed assembly identity or MVID.");
        var target = RealStS2PrepareMethodRewrite.FindMethodByStableIdentity(verifyModule, TargetTypeFullName, TargetMethodFullName);
        RequireVeryEarlySignature(target, "diagnostic clone");
        var moveNext = FindVeryEarlyMoveNext(verifyModule);
        var bridgeType = EnumerateTypes(verifyModule.Types).SingleOrDefault(type => type.FullName == DiagnosticBridgeTypeFullName)
            ?? throw new MissingMemberException(DiagnosticBridgeTypeFullName);
        var bridgeFields = bridgeType.Fields.Where(field => field.Name == DiagnosticBridgeCallbackFieldName).ToArray();
        var bridgeEmitMethods = bridgeType.Methods.Where(method => method.Name == "Emit").ToArray();
        if (bridgeFields.Length != 1 || bridgeFields[0].FieldType.FullName != "System.Action`1<System.String>" ||
            bridgeEmitMethods.Length != 1 || !bridgeEmitMethods[0].IsStatic || !bridgeEmitMethods[0].HasBody || bridgeEmitMethods[0].ReturnType.FullName != "System.Void" ||
            bridgeEmitMethods[0].Parameters.Count != 1 || bridgeEmitMethods[0].Parameters[0].ParameterType.FullName != "System.String")
        {
            throw new InvalidDataException("Step-35.0.8 diagnostic bridge field/method signature drifted after serialization.");
        }

        // 0.0.129 physically proved that a synthetically encoded Action<string>::Invoke(string)
        // MemberRef is not runtime-equivalent to Action<T>::Invoke(T): iOS returned a managed
        // MissingMethodException before the first in-method marker. Require the serialized bridge
        // callvirt to preserve the declaring type's VAR(0) signature exactly: Action<string>::Invoke(!0).
        var bridgeInvokeInstructions = bridgeEmitMethods[0].Body.Instructions
            .Where(instruction => instruction.OpCode == OpCodes.Callvirt && instruction.Operand is MethodReference)
            .ToArray();
        if (bridgeInvokeInstructions.Length != 1 || bridgeInvokeInstructions[0].Operand is not MethodReference bridgeInvoke ||
            bridgeInvoke.Name != "Invoke" || !bridgeInvoke.HasThis || bridgeInvoke.ReturnType.FullName != "System.Void" ||
            bridgeInvoke.DeclaringType is not GenericInstanceType bridgeActionType ||
            bridgeActionType.ElementType.FullName != "System.Action`1" || bridgeActionType.GenericArguments.Count != 1 ||
            bridgeActionType.GenericArguments[0].FullName != "System.String" || bridgeInvoke.Parameters.Count != 1 ||
            bridgeInvoke.Parameters[0].ParameterType is not GenericParameter bridgeInvokeParameter ||
            bridgeInvokeParameter.Type != GenericParameterType.Type || bridgeInvokeParameter.Position != 0)
        {
            throw new InvalidDataException("Step-35.0.8 diagnostic bridge Invoke MemberRef is not encoded as Action<string>::Invoke(!0).");
        }

        var expectedMarkerCount = 0;
        var verifiedCctorTypes = new HashSet<string>(StringComparer.Ordinal);
        foreach (var item in GetDiagnosticMarkerTargets())
        {
            var type = EnumerateTypes(verifyModule.Types).SingleOrDefault(candidate => candidate.FullName == item.TypeName)
                ?? throw new MissingMemberException($"Step-35.0.8 diagnostic verification target type missing: {item.TypeName}.");
            var method = type.Methods.SingleOrDefault(candidate => candidate.FullName == item.MethodFullName && candidate.HasBody)
                ?? throw new MissingMethodException($"Step-35.0.8 diagnostic verification target method missing: {item.MethodFullName}.");
            if (!HasInjectedEntryMarkerAtStart(method, item.Marker))
                throw new InvalidDataException($"Step-35.0.8 marker is not the first stack-neutral checkpoint in {item.MethodFullName}: {item.Marker}.");
            expectedMarkerCount++;

            var cctor = type.Methods.SingleOrDefault(candidate => candidate.Name == ".cctor" && candidate.IsStatic && candidate.HasBody);
            if (cctor is not null && verifiedCctorTypes.Add(item.TypeName))
            {
                var cctorMarker = $"INMETHOD_CCTOR — {item.TypeName}..cctor entered";
                if (!HasInjectedEntryMarkerAtStart(cctor, cctorMarker))
                    throw new InvalidDataException($"Step-35.0.8 cctor marker is not the first stack-neutral checkpoint in {item.TypeName}..cctor.");
                expectedMarkerCount++;
            }
        }

        foreach (var item in GetDiagnosticCallsiteMarkerTargets())
        {
            var type = EnumerateTypes(verifyModule.Types).SingleOrDefault(candidate => candidate.FullName == item.TypeName)
                ?? throw new MissingMemberException($"Step-35.0.8 diagnostic callsite verification type missing: {item.TypeName}.");
            var method = type.Methods.SingleOrDefault(candidate => candidate.FullName == item.MethodFullName && candidate.HasBody)
                ?? throw new MissingMethodException($"Step-35.0.8 diagnostic callsite verification method missing: {item.MethodFullName}.");
            if (!HasInjectedCallsiteMarkers(method, item.CalleeFullName, item.BeforeMarker, item.AfterMarker))
                throw new InvalidDataException($"Step-35.0.8 callsite markers did not serialize immediately around {item.CalleeFullName} in {item.MethodFullName}.");
            expectedMarkerCount += 2;
        }

        var markerCountVerified = EnumerateTypes(verifyModule.Types)
            .SelectMany(type => type.Methods)
            .Where(method => method.HasBody)
            .SelectMany(method => method.Body.Instructions)
            .Count(instruction => instruction.OpCode.Code == Code.Ldstr && instruction.Operand is string text && text.StartsWith("INMETHOD_", StringComparison.Ordinal));
        if (markerCountVerified != expectedMarkerCount)
            throw new InvalidDataException($"Step-35.0.8 diagnostic clone marker count drifted after serialization: expected {expectedMarkerCount}, observed {markerCountVerified}.");

        return new DiagnosticCloneSnapshot(
            diagnosticPath,
            sha256,
            length,
            target.MetadataToken.ToUInt32(),
            moveNext.MetadataToken.ToUInt32(),
            markerCountVerified,
            verifiedConstantMetadataSha256,
            writeResolutionRequestCount,
            syntheticConstantTypeCount,
            approvedConstantScopeCount,
            approvedConstantRequirementCount,
            writeResolutionIdentities);
    }

    internal static (GenericInstanceType ActionStringType, MethodReference InvokeReference) CreateDiagnosticActionStringInvokeReference(
        ModuleDefinition module,
        IMetadataScope systemRuntime)
    {
        ArgumentNullException.ThrowIfNull(module);
        ArgumentNullException.ThrowIfNull(systemRuntime);

        // ECMA-335 MemberRef signatures on a constructed generic declaring type still use the
        // declaring type's VAR generic parameter in the member signature. The 0.0.129 bridge
        // incorrectly encoded Invoke(string), which the iOS runtime could not bind to
        // Action<T>.Invoke(T), producing a managed MissingMethodException before INMETHOD_001.
        // Model Action<T> explicitly, then construct Action<string> only at the declaring-type
        // level so Invoke remains encoded as Invoke(!0).
        var actionOpen = new TypeReference("System", "Action`1", module, systemRuntime, false);
        var actionTypeParameter = new GenericParameter("T", actionOpen);
        actionOpen.GenericParameters.Add(actionTypeParameter);
        var actionStringType = new GenericInstanceType(actionOpen);
        actionStringType.GenericArguments.Add(module.TypeSystem.String);
        var invoke = new MethodReference("Invoke", module.TypeSystem.Void, actionStringType)
        {
            HasThis = true,
            ExplicitThis = false,
            CallingConvention = MethodCallingConvention.Default,
        };
        invoke.Parameters.Add(new ParameterDefinition(actionTypeParameter));
        return (actionStringType, invoke);
    }

    internal static void InsertCallsiteMarkers(
        MethodDefinition method,
        MethodReference emitReference,
        string calleeFullName,
        string beforeMarker,
        string afterMarker)
    {
        if (!method.HasBody || method.Body.Instructions.Count == 0)
            throw new InvalidDataException($"Cannot instrument callsite in method without IL: {method.FullName}.");

        var matches = method.Body.Instructions
            .Where(instruction => instruction.OpCode.Code is Code.Call or Code.Callvirt &&
                                  instruction.Operand is MethodReference callee &&
                                  callee.FullName == calleeFullName)
            .ToArray();
        if (matches.Length != 1)
            throw new InvalidDataException($"Step-35.0.8 expected exactly one callsite for {calleeFullName} in {method.FullName}; found {matches.Length}.");

        var callsite = matches[0];
        var isBranchTarget = method.Body.Instructions.Any(instruction => instruction.Operand switch
        {
            Instruction target => ReferenceEquals(target, callsite),
            Instruction[] targets => targets.Any(target => ReferenceEquals(target, callsite)),
            _ => false,
        });
        if (isBranchTarget)
            throw new InvalidDataException($"Step-35.0.8 refuses to place a pre-call marker on branch-target callsite {calleeFullName} in {method.FullName}.");

        var il = method.Body.GetILProcessor();
        il.InsertBefore(callsite, Instruction.Create(OpCodes.Ldstr, beforeMarker));
        il.InsertBefore(callsite, Instruction.Create(OpCodes.Call, emitReference));
        var afterText = Instruction.Create(OpCodes.Ldstr, afterMarker);
        il.InsertAfter(callsite, afterText);
        il.InsertAfter(afterText, Instruction.Create(OpCodes.Call, emitReference));
    }

    private static bool HasInjectedCallsiteMarkers(
        MethodDefinition method,
        string calleeFullName,
        string beforeMarker,
        string afterMarker)
    {
        var matches = method.Body.Instructions
            .Where(instruction => instruction.OpCode.Code is Code.Call or Code.Callvirt &&
                                  instruction.Operand is MethodReference callee &&
                                  callee.FullName == calleeFullName)
            .ToArray();
        if (matches.Length != 1)
            return false;

        var callsite = matches[0];
        return callsite.Previous?.OpCode.Code == Code.Call &&
               callsite.Previous.Operand is MethodReference beforeEmit &&
               beforeEmit.DeclaringType.FullName == DiagnosticBridgeTypeFullName &&
               beforeEmit.Name == "Emit" &&
               callsite.Previous.Previous?.OpCode.Code == Code.Ldstr &&
               callsite.Previous.Previous.Operand is string beforeText &&
               beforeText == beforeMarker &&
               callsite.Next?.OpCode.Code == Code.Ldstr &&
               callsite.Next.Operand is string afterText &&
               afterText == afterMarker &&
               callsite.Next.Next?.OpCode.Code == Code.Call &&
               callsite.Next.Next.Operand is MethodReference afterEmit &&
               afterEmit.DeclaringType.FullName == DiagnosticBridgeTypeFullName &&
               afterEmit.Name == "Emit";
    }

    private static void InsertEntryMarker(MethodDefinition method, MethodReference emitReference, string marker)
    {
        if (!method.HasBody || method.Body.Instructions.Count == 0)
            throw new InvalidDataException($"Cannot instrument method without IL: {method.FullName}.");
        if (HasInjectedEntryMarker(method, marker))
            return;
        var first = method.Body.Instructions[0];
        var il = method.Body.GetILProcessor();
        il.InsertBefore(first, Instruction.Create(OpCodes.Ldstr, marker));
        il.InsertBefore(first, Instruction.Create(OpCodes.Call, emitReference));
    }

    private static bool HasInjectedEntryMarker(MethodDefinition method, string marker)
        => method.HasBody && method.Body.Instructions.Any(instruction => instruction.OpCode.Code == Code.Ldstr && Equals(instruction.Operand, marker));

    private static bool HasInjectedEntryMarkerAtStart(MethodDefinition method, string marker)
    {
        if (!method.HasBody || method.Body.Instructions.Count < 2)
            return false;
        var markerInstruction = method.Body.Instructions[0];
        var callInstruction = method.Body.Instructions[1];
        return markerInstruction.OpCode.Code == Code.Ldstr && Equals(markerInstruction.Operand, marker) &&
               callInstruction.OpCode.Code == Code.Call && callInstruction.Operand is MethodReference call &&
               call.Name == "Emit" && call.DeclaringType.FullName == DiagnosticBridgeTypeFullName;
    }

    private sealed record DiagnosticCloneSnapshot(
        string Path,
        string Sha256,
        long Length,
        uint MethodToken,
        uint MoveNextToken,
        int MarkerCount,
        string ConstantMetadataSha256,
        int WriteResolutionRequestCount,
        int SyntheticConstantTypeCount,
        int ApprovedConstantScopeCount,
        int ApprovedConstantRequirementCount,
        string WriteResolutionIdentities);

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
        string DiagnosticPath,
        string DiagnosticSha256,
        long DiagnosticLength,
        uint DiagnosticMethodToken,
        uint DiagnosticMoveNextToken,
        int DiagnosticMarkerCount,
        uint TransformedMethodToken,
        uint TransformedMoveNextToken,
        string TargetSemanticSha256,
        string MoveNextSemanticSha256,
        string VeryEarlyStaticInstructionMap,
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
            Justification = "Step 35.0.6 LoadPrimary accepts only the already hash-pinned selected execution image; Gate B supplies the separately verified diagnostic clone and the exact transformed source stays outside the CLR.")]
        internal Assembly LoadPrimary(string transformedPath, string expectedSha256)
        {
            Checkpoint("B_LOADPRIMARY_REHASH_START — LoadPrimary is re-hashing the selected Step-35 execution image immediately before LoadFromStream.");
            var actualSha256 = ComputeSha256Hex(transformedPath);
            if (!actualSha256.Equals(expectedSha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Step-35 selected execution image hash changed immediately before LoadFromStream.");
            Checkpoint($"B_LOADPRIMARY_REHASH_PASS — immediate selected execution-image SHA-256 matched {actualSha256}.");
            using var stream = new FileStream(transformedPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            Checkpoint("B_LOADFROMSTREAM_START — entering AssemblyLoadContext.LoadFromStream for the selected Step-35 execution image.");
            var assembly = LoadFromStream(stream);
            Checkpoint("B_LOADFROMSTREAM_PASS — AssemblyLoadContext.LoadFromStream returned the selected Step-35 execution image.");
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


    private static Dictionary<DiagnosticExternalConstantTypeKey, TypeCode> CollectDiagnosticExternalConstantTypeRequirements(ModuleDefinition module)
    {
        var requirements = new Dictionary<DiagnosticExternalConstantTypeKey, TypeCode>();

        void Add(TypeReference declaredType, object? constant, string provider)
        {
            if (constant is null)
                return;
            var leaf = GetDiagnosticConstantResolutionLeaf(declaredType);
            if (leaf is null || leaf.Scope is ModuleDefinition)
                return;
            if (leaf.Scope is not AssemblyNameReference assemblyReference)
                throw new InvalidDataException($"Step-35.0.8 constant provider '{provider}' has unsupported metadata scope '{leaf.Scope?.MetadataScopeType}'.");

            var typeCode = Type.GetTypeCode(constant.GetType());
            if (!IsSupportedDiagnosticConstantTypeCode(typeCode))
                throw new InvalidDataException($"Step-35.0.8 constant provider '{provider}' has unsupported constant storage type {constant.GetType().FullName}.");
            var key = new DiagnosticExternalConstantTypeKey(assemblyReference.FullName, leaf.FullName, leaf.IsNested);
            if (requirements.TryGetValue(key, out var prior) && prior != typeCode)
                throw new InvalidDataException($"Step-35.0.8 external constant type '{leaf.FullName}' has inconsistent storage types {prior} and {typeCode}.");
            requirements[key] = typeCode;
        }

        foreach (var type in EnumerateTypes(module.Types))
        {
            foreach (var field in type.Fields)
                if (field.HasConstant)
                    Add(field.FieldType, field.Constant, $"field {field.FullName}");
            foreach (var property in type.Properties)
                if (property.HasConstant)
                    Add(property.PropertyType, property.Constant, $"property {property.FullName}");
            foreach (var method in type.Methods)
            {
                if (method.MethodReturnType.HasConstant)
                    Add(method.MethodReturnType.ReturnType, method.MethodReturnType.Constant, $"return {method.FullName}");
                foreach (var parameter in method.Parameters)
                    if (parameter.HasConstant)
                        Add(parameter.ParameterType, parameter.Constant, $"parameter {method.FullName}::{parameter.Name}");
            }
        }

        return requirements;
    }

    private static TypeReference? GetDiagnosticConstantResolutionLeaf(TypeReference type)
    {
        while (true)
        {
            switch (type)
            {
                case GenericInstanceType genericInstance:
                    if (genericInstance.ElementType.FullName == "System.Nullable`1" && genericInstance.GenericArguments.Count == 1)
                    {
                        type = genericInstance.GenericArguments[0];
                        continue;
                    }
                    type = genericInstance.ElementType;
                    continue;
                case OptionalModifierType optionalModifier:
                    type = optionalModifier.ElementType;
                    continue;
                case RequiredModifierType requiredModifier:
                    type = requiredModifier.ElementType;
                    continue;
                case ByReferenceType byReference:
                    type = byReference.ElementType;
                    continue;
                case SentinelType sentinel:
                    type = sentinel.ElementType;
                    continue;
                case ArrayType:
                case GenericParameter:
                    return null;
            }

            if (type.MetadataType is MetadataType.Boolean or MetadataType.Char or MetadataType.SByte or MetadataType.Byte or
                MetadataType.Int16 or MetadataType.UInt16 or MetadataType.Int32 or MetadataType.UInt32 or MetadataType.Int64 or
                MetadataType.UInt64 or MetadataType.Single or MetadataType.Double or MetadataType.String or MetadataType.Object)
                return null;
            return type;
        }
    }

    private static bool IsSupportedDiagnosticConstantTypeCode(TypeCode typeCode)
        => typeCode is TypeCode.Boolean or TypeCode.Char or TypeCode.SByte or TypeCode.Byte or TypeCode.Int16 or TypeCode.UInt16 or
            TypeCode.Int32 or TypeCode.UInt32 or TypeCode.Int64 or TypeCode.UInt64 or TypeCode.Single or TypeCode.Double;

    private static TypeReference GetDiagnosticPrimitiveConstantType(ModuleDefinition sourceModule, TypeCode typeCode)
        => typeCode switch
        {
            TypeCode.Boolean => sourceModule.TypeSystem.Boolean,
            TypeCode.Char => sourceModule.TypeSystem.Char,
            TypeCode.SByte => sourceModule.TypeSystem.SByte,
            TypeCode.Byte => sourceModule.TypeSystem.Byte,
            TypeCode.Int16 => sourceModule.TypeSystem.Int16,
            TypeCode.UInt16 => sourceModule.TypeSystem.UInt16,
            TypeCode.Int32 => sourceModule.TypeSystem.Int32,
            TypeCode.UInt32 => sourceModule.TypeSystem.UInt32,
            TypeCode.Int64 => sourceModule.TypeSystem.Int64,
            TypeCode.UInt64 => sourceModule.TypeSystem.UInt64,
            TypeCode.Single => sourceModule.TypeSystem.Single,
            TypeCode.Double => sourceModule.TypeSystem.Double,
            _ => throw new InvalidDataException($"Unsupported Step-35.0.8 constant storage type {typeCode}."),
        };

    private sealed class DiagnosticConstantMetadataWriteResolver : IAssemblyResolver
    {
        private readonly List<string> _requests = [];
        private readonly Dictionary<string, AssemblyDefinition> _surrogates = new(StringComparer.Ordinal);
        private bool _configured;

        internal IReadOnlyList<string> Requests => _requests;

        internal DiagnosticConstantMetadataResolutionPlan Configure(ModuleDefinition sourceModule)
        {
            if (_configured)
                throw new InvalidOperationException("The Step-35.0.8 constant-metadata write resolver was already configured.");
            _configured = true;

            var requirements = CollectDiagnosticExternalConstantTypeRequirements(sourceModule);
            ValidateAuditedRequirementSet(requirements);

            var assemblyReferences = new Dictionary<string, AssemblyNameReference>(StringComparer.Ordinal);
            foreach (var identity in requirements.Keys.Select(key => key.AssemblyFullName).Distinct(StringComparer.Ordinal))
            {
                var matches = sourceModule.AssemblyReferences.Where(reference => reference.FullName.Equals(identity, StringComparison.Ordinal)).ToArray();
                if (matches.Length != 1)
                    throw new InvalidDataException($"Step-35.0.8 source must contain exactly one AssemblyRef for audited constant-metadata scope {identity}; found {matches.Length}.");
                assemblyReferences.Add(identity, matches[0]);
            }

            var syntheticTypeCount = 0;
            foreach (var scopeGroup in requirements.GroupBy(pair => pair.Key.AssemblyFullName, StringComparer.Ordinal).OrderBy(group => group.Key, StringComparer.Ordinal))
            {
                var sourceReference = assemblyReferences[scopeGroup.Key];
                var surrogateName = new AssemblyNameDefinition(sourceReference.Name, sourceReference.Version)
                {
                    Culture = sourceReference.Culture,
                    PublicKeyToken = sourceReference.PublicKeyToken is null ? [] : sourceReference.PublicKeyToken.ToArray(),
                };
                var safeName = sourceReference.Name.Replace('.', '-');
                var surrogate = AssemblyDefinition.CreateAssembly(surrogateName, $"Step35.{safeName}.ConstantMetadataSurrogate.dll", ModuleKind.Dll);
                _surrogates.Add(scopeGroup.Key, surrogate);

                foreach (var requirement in scopeGroup.OrderBy(pair => pair.Key.TypeFullName, StringComparer.Ordinal))
                {
                    if (requirement.Key.IsNested)
                        throw new InvalidDataException($"Step-35.0.8 does not permit nested external constant type synthesis: {requirement.Key.TypeFullName}.");
                    var separator = requirement.Key.TypeFullName.LastIndexOf('.');
                    var typeNamespace = separator < 0 ? string.Empty : requirement.Key.TypeFullName[..separator];
                    var typeName = separator < 0 ? requirement.Key.TypeFullName : requirement.Key.TypeFullName[(separator + 1)..];
                    var enumBase = new TypeReference("System", "Enum", surrogate.MainModule, surrogateName);
                    var syntheticEnum = new TypeDefinition(typeNamespace, typeName,
                        Mono.Cecil.TypeAttributes.Public | Mono.Cecil.TypeAttributes.Sealed, enumBase);
                    syntheticEnum.Fields.Add(new FieldDefinition("value__",
                        Mono.Cecil.FieldAttributes.Public | Mono.Cecil.FieldAttributes.SpecialName | Mono.Cecil.FieldAttributes.RTSpecialName,
                        GetDiagnosticPrimitiveConstantType(sourceModule, requirement.Value)));
                    surrogate.MainModule.Types.Add(syntheticEnum);
                    syntheticTypeCount++;
                }
            }

            return new DiagnosticConstantMetadataResolutionPlan(syntheticTypeCount, _surrogates.Count, requirements.Count);
        }

        private static void ValidateAuditedRequirementSet(IReadOnlyDictionary<DiagnosticExternalConstantTypeKey, TypeCode> actual)
        {
            var missing = DiagnosticAuditedExternalConstantTypeRequirements
                .Where(pair => !actual.TryGetValue(pair.Key, out var observed) || observed != pair.Value)
                .Select(pair => FormatRequirement(pair.Key, pair.Value)).OrderBy(value => value, StringComparer.Ordinal).ToArray();
            var unexpected = actual
                .Where(pair => !DiagnosticAuditedExternalConstantTypeRequirements.TryGetValue(pair.Key, out var expected) || expected != pair.Value)
                .Select(pair => FormatRequirement(pair.Key, pair.Value)).OrderBy(value => value, StringComparer.Ordinal).ToArray();
            if (missing.Length == 0 && unexpected.Length == 0)
                return;
            var detail = new List<string>();
            if (missing.Length != 0) detail.Add("missing/changed audited requirement(s): " + string.Join(" | ", missing));
            if (unexpected.Length != 0) detail.Add("unexpected requirement(s): " + string.Join(" | ", unexpected));
            throw new InvalidDataException("Step-35.0.8 external constant-metadata requirement set drifted from the physically proven Step-32 audit; " + string.Join("; ", detail));
        }

        private static string FormatRequirement(DiagnosticExternalConstantTypeKey key, TypeCode typeCode)
            => $"{key.AssemblyFullName} / {key.TypeFullName} / {typeCode} / nested={key.IsNested}";

        public AssemblyDefinition Resolve(AssemblyNameReference name) => ResolveCore(name);
        public AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters) => ResolveCore(name);

        private AssemblyDefinition ResolveCore(AssemblyNameReference name)
        {
            _requests.Add(name.FullName);
            if (!_configured || !_surrogates.TryGetValue(name.FullName, out var surrogate))
                throw new AssemblyResolutionException(name);
            return surrogate;
        }

        internal void ValidateWriteRequests()
        {
            if (_requests.Count == 0)
                throw new InvalidDataException("Step-35.0.8 expected Cecil serialization to use at least one bounded constant-metadata surrogate, but no write-time resolution request occurred.");
            var unexpected = _requests.Where(value => !_surrogates.ContainsKey(value)).Distinct(StringComparer.Ordinal).OrderBy(value => value, StringComparer.Ordinal).ToArray();
            if (unexpected.Length != 0)
                throw new InvalidDataException("Step-35.0.8 Cecil serialization attempted an unapproved assembly resolution: " + string.Join(" | ", unexpected));
        }

        public void Dispose()
        {
            foreach (var surrogate in _surrogates.Values)
                surrogate.Dispose();
            _surrogates.Clear();
        }
    }

    private sealed record DiagnosticConstantMetadataResolutionPlan(int SyntheticTypeCount, int ApprovedScopeCount, int ApprovedRequirementCount);
    private readonly record struct DiagnosticExternalConstantTypeKey(string AssemblyFullName, string TypeFullName, bool IsNested);

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
