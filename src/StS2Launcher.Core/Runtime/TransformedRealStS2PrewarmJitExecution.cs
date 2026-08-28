using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.Loader;
using System.Security.Cryptography;
using System.Text.Json;
using Mono.Cecil;

namespace StS2Launcher.Core;

/// <summary>
/// Step 34 boundary. Re-manufactures/reverifies the physically closed Step-32 transformed image,
/// re-establishes the Step-33 transformed-primary admission contract in a fresh execution-capable
/// private AssemblyLoadContext, then reflects and invokes exactly transformed
/// MegaCrit.Sts2.Core.Helpers.OneTimeInitialization::PrewarmJit() once. Resolver authority remains
/// limited to the persisted Step-21/22 host bindings and hash-pinned initializer-free prepared private
/// dependencies. Initializer-bearing private dependencies (including 0Harmony), unplanned managed
/// requests, native resolution, entry-point execution, Godot startup, and broader game initialization
/// remain forbidden and fail closed.
/// </summary>
public sealed class TransformedRealStS2PrewarmJitExecution : IDisposable
{
    public const string LoadContextName = "StS2Launcher-Step34-PrewarmJit";
    public const string TargetTypeFullName = "MegaCrit.Sts2.Core.Helpers.OneTimeInitialization";
    public const string TargetMethodName = "PrewarmJit";
    public const string TargetMethodFullName = "System.Void MegaCrit.Sts2.Core.Helpers.OneTimeInitialization::PrewarmJit()";
    public const uint ClosedStep32TransformedPrewarmJitToken = 0x0600AFEA;

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
    private Step34ExecutionLoadContext? _loadContext;
    private bool _disposed;

    public TransformedRealStS2PrewarmJitExecution(string launcherDataRoot, bool collectibleLoadContext = false)
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

    public async Task<TransformedRealStS2PrewarmJitExecutionGateResult> RunVerifiedExecutionPreflightAsync(
        IProgress<TransformedRealStS2PrewarmJitExecutionProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        const TransformedRealStS2PrewarmJitExecutionGate gate = TransformedRealStS2PrewarmJitExecutionGate.VerifiedExecutionPreflight;
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

            var transformedPath = Path.Combine(
                _launcherDataRoot,
                RealStS2PrepareMethodRewrite.WorkRootName,
                RealStS2PrepareMethodRewrite.TransformedRootName,
                RealStS2PrepareMethodRewrite.PrimaryFileName);

            stage = "physical Step-32 transformed artifact and target identity";
            VerifyFileLength(transformedPath, TransformedRealStS2AssemblyAdmission.ClosedStep32TransformedBytes, "transformed primary");
            var transformedSha256 = ComputeSha256Hex(transformedPath);
            if (!transformedSha256.Equals(TransformedRealStS2AssemblyAdmission.ClosedStep32TransformedSha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException($"Step 34 requires the exact physically closed Step-32 transformed SHA-256 {TransformedRealStS2AssemblyAdmission.ClosedStep32TransformedSha256}; observed {transformedSha256}.");

            uint transformedMethodToken;
            using (var resolver = new RejectingAssemblyResolver())
            using (var module = ModuleDefinition.ReadModule(transformedPath, new ReaderParameters
                   {
                       ReadSymbols = false,
                       ReadingMode = ReadingMode.Deferred,
                       AssemblyResolver = resolver,
                   }))
            {
                if (module.Assembly?.Name.FullName != TransformedRealStS2AssemblyAdmission.ClosedStep32AssemblyIdentity ||
                    module.Mvid != TransformedRealStS2AssemblyAdmission.ClosedStep32Mvid)
                {
                    throw new InvalidDataException("Step-34 transformed image identity/MVID drifted from the closed Step-32/33 evidence.");
                }

                var method = RealStS2PrepareMethodRewrite.FindMethodByStableIdentity(module, TargetTypeFullName, TargetMethodFullName);
                transformedMethodToken = method.MetadataToken.ToUInt32();
                if (transformedMethodToken != ClosedStep32TransformedPrewarmJitToken)
                    throw new InvalidDataException($"Step-34 transformed PrewarmJit MethodDef token drifted: 0x{transformedMethodToken:X8} != 0x{ClosedStep32TransformedPrewarmJitToken:X8}.");
                if (!method.IsStatic || method.Parameters.Count != 0 || method.ReturnType.FullName != "System.Void")
                    throw new InvalidDataException("Step-34 transformed PrewarmJit signature drifted from static parameterless void.");

                var semanticSha256 = RealStS2PrepareMethodRewrite.ComputeMethodSemanticFingerprint(method);
                if (!semanticSha256.Equals(TransformedRealStS2AssemblyAdmission.ClosedStep32TransformedSemanticSha256, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException($"Step-34 transformed PrewarmJit semantic fingerprint drifted: {semanticSha256}.");
                var prepareMethodCount = method.Body.Instructions.Count(instruction =>
                    instruction.Operand is MethodReference reference &&
                    reference.DeclaringType.FullName == "System.Runtime.CompilerServices.RuntimeHelpers" &&
                    reference.Name == "PrepareMethod");
                if (prepareMethodCount != 0)
                    throw new InvalidDataException($"Step-34 transformed PrewarmJit unexpectedly contains {prepareMethodCount} PrepareMethod reference(s).");
                if (resolver.Requests.Count != 0)
                    throw new InvalidDataException("Step-34 transformed target metadata inspection unexpectedly resolved a dependency through Cecil.");
            }
            progress?.Report(new(gate, 5, 7, transformedPath, "Exact closed transformed image and PrewarmJit identity/semantics requalified."));

            stage = "Step-21/22 prepared execution-plan preflight";
            var preparedResult = await _preparedPreflight.RunPreparedLoadPreflightAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            if (!preparedResult.Passed)
                throw new InvalidDataException("Step-34 prepared runtime-plan preflight failed: " + preparedResult.Detail.Replace('\n', ' '));

            var planBytes = await File.ReadAllBytesAsync(_planPath, cancellationToken).ConfigureAwait(false);
            var planSha256 = Convert.ToHexString(SHA256.HashData(planBytes)).ToLowerInvariant();
            var plan = JsonSerializer.Deserialize(planBytes, RuntimeFrameworkBindingJsonContext.Default.RuntimeFrameworkBindingPlanDocument)
                ?? throw new InvalidDataException("Step 34 could not deserialize the persisted Step-21/22 runtime-binding plan.");
            ValidateExecutionPlan(plan);

            var prepared = plan.PreparedAssemblies.Select(item =>
            {
                var relative = NormalizeRelative(item.RelativePath);
                var path = ResolveChildPath(_preparedRoot, relative, "Step-34 prepared dependency path");
                VerifyFileLength(path, item.Length, "prepared dependency");
                var hash = ComputeSha1Hex(path);
                if (!hash.Equals(item.Sha1Hex, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException($"Step-34 prepared dependency SHA-1 drifted before execution: {relative}.");
                return new PreparedExecutionEntry(item, path, new AssemblyName(item.AssemblyFullName), ReadModuleInitializerCount(path));
            }).ToArray();

            var preparedPrimary = prepared.Single(item => item.Plan.IsPrimary);
            if (!preparedPrimary.Plan.AssemblyFullName.Equals(TransformedRealStS2AssemblyAdmission.ClosedStep32AssemblyIdentity, StringComparison.Ordinal))
                throw new InvalidDataException("Step-34 prepared-plan primary identity differs from the closed transformed-primary identity.");

            var initializerBearing = prepared.Where(item => !item.Plan.IsPrimary && item.ModuleInitializerCount > 0).ToArray();
            if (initializerBearing.Length != 1 ||
                !string.Equals(initializerBearing[0].AssemblyName.Name, ControlledManagedInitialization.TargetSimpleName, StringComparison.OrdinalIgnoreCase) ||
                (initializerBearing[0].AssemblyName.Version ?? ZeroVersion) != ControlledManagedInitialization.TargetVersion)
            {
                throw new InvalidDataException(
                    "Step 34 requires the previously proven sole initializer-bearing private dependency to remain exact 0Harmony 2.4.2.0; observed: " +
                    (initializerBearing.Length == 0 ? "<none>" : string.Join(" | ", initializerBearing.Select(item => item.Plan.AssemblyFullName))));
            }

            _preflight = new ExecutionPreflightSnapshot(
                transformedPath,
                transformedSha256,
                transformedMethodToken,
                plan,
                planSha256,
                prepared,
                preparedPrimary,
                initializerBearing[0]);

            EnsureNoStS2Loaded("Gate A exit");
            progress?.Report(new(gate, 7, 7, _planPath, "Execution preflight complete; transformed primary and all prepared dependencies remain outside the CLR."));

            return Pass(gate,
                "EXACT CLOSED TRANSFORMED IMAGE, PREWARMJIT TARGET, AND EXECUTION RESOLVER PLAN REQUALIFIED; NO STS2 CLR LOAD OR GAME INVOCATION OCCURRED.\n" +
                "Physical Step-32 transform closure re-run: 4/4 PASS\n" +
                $"Transformed SHA-256: {transformedSha256}\n" +
                $"Transformed bytes: {TransformedRealStS2AssemblyAdmission.ClosedStep32TransformedBytes:N0}\n" +
                $"Assembly identity: {TransformedRealStS2AssemblyAdmission.ClosedStep32AssemblyIdentity}\n" +
                $"Module MVID: {TransformedRealStS2AssemblyAdmission.ClosedStep32Mvid}\n" +
                $"Transformed PrewarmJit semantic fingerprint: {TransformedRealStS2AssemblyAdmission.ClosedStep32TransformedSemanticSha256}\n" +
                $"Transformed PrewarmJit metadata token: 0x{transformedMethodToken:X8}\n" +
                "Transformed PrepareMethod references: 0\n" +
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
        Justification = "Step 34 admits only the exact hash-pinned transformed primary image; member reflection/invocation is deferred to Gate C.")]
    public TransformedRealStS2PrewarmJitExecutionGateResult RunExecutionCapableClrAdmission()
    {
        const TransformedRealStS2PrewarmJitExecutionGate gate = TransformedRealStS2PrewarmJitExecutionGate.ExecutionCapableClrAdmission;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var preflight = RequirePreflight();
            EnsureNoStS2Loaded("Gate B entry");
            if (_loadContext is not null)
                throw new InvalidOperationException("Step 34 Gate B requires a fresh dedicated load context.");

            stage = "immediate transformed hash recheck";
            VerifyFileLength(preflight.TransformedPath, TransformedRealStS2AssemblyAdmission.ClosedStep32TransformedBytes, "transformed primary");
            var immediateSha256 = ComputeSha256Hex(preflight.TransformedPath);
            if (!immediateSha256.Equals(TransformedRealStS2AssemblyAdmission.ClosedStep32TransformedSha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Step-34 transformed image changed between Gate A verification and Gate B CLR admission.");

            stage = "execution-capable strict AssemblyLoadContext construction";
            var context = new Step34ExecutionLoadContext(
                LoadContextName,
                preflight.Plan,
                preflight.PreparedAssemblies,
                _collectibleLoadContext);
            _loadContext = context;

            stage = "exact transformed sts2.dll LoadFromStream";
            var assembly = context.LoadPrimary(preflight.TransformedPath, immediateSha256);
            if (!ReferenceEquals(AssemblyLoadContext.GetLoadContext(assembly), context))
                throw new InvalidDataException("The transformed sts2.dll did not load into the dedicated Step-34 AssemblyLoadContext.");

            var actualIdentity = assembly.GetName().FullName ?? assembly.GetName().Name ?? string.Empty;
            if (!actualIdentity.Equals(TransformedRealStS2AssemblyAdmission.ClosedStep32AssemblyIdentity, StringComparison.Ordinal))
                throw new InvalidDataException($"Loaded transformed identity mismatch. Expected '{TransformedRealStS2AssemblyAdmission.ClosedStep32AssemblyIdentity}', actual '{actualIdentity}'.");
            var actualMvid = assembly.ManifestModule.ModuleVersionId;
            if (actualMvid != TransformedRealStS2AssemblyAdmission.ClosedStep32Mvid)
                throw new InvalidDataException($"Loaded transformed module MVID mismatch. Expected {TransformedRealStS2AssemblyAdmission.ClosedStep32Mvid}, actual {actualMvid}.");

            if (context.ManagedResolverRequests.Count != 0 || context.PrivateLoads.Count != 0 ||
                context.InitializerBearingRequests.Count != 0 || context.RejectedManagedRequests.Count != 0 || context.NativeLoadAttempts.Count != 0)
            {
                throw new InvalidDataException(
                    "Step-34 transformed primary admission no longer matches the physically closed Step-33 zero-resolution admission behavior. " +
                    context.FormatResolverState());
            }

            var matches = FindLoadedStS2Assemblies();
            if (matches.Length != 1 || !ReferenceEquals(matches[0], assembly))
                throw new InvalidDataException($"Expected exactly one transformed sts2 assembly after Step-34 Gate B, found {matches.Length}.");
            var contextAssemblies = context.Assemblies.ToArray();
            if (contextAssemblies.Length != 1 || !ReferenceEquals(contextAssemblies[0], assembly))
                throw new InvalidDataException($"Step-34 context contains {contextAssemblies.Length} private assemblies immediately after admission instead of exactly transformed sts2.");

            _admission = new PrimaryAdmissionSnapshot(assembly, actualIdentity, actualMvid, immediateSha256);

            return Pass(gate,
                "PHYSICALLY CLOSED STEP-33 TRANSFORMED-PRIMARY ADMISSION BEHAVIOR RE-ESTABLISHED IN THE STEP-34 EXECUTION CONTEXT; NO GAME MEMBER REFLECTION/INVOCATION YET.\n" +
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
            if (_collectibleLoadContext)
                ReleaseLoadContext();
            _admission = null;
            return Fail(gate, stage, ex);
        }
    }

    [UnconditionalSuppressMessage(
        "Trimming",
        "IL2026",
        Justification = "Step 34 deliberately reflects and invokes one exact method on the exact transformed real-StS2 image. The dynamic payload is preserved by the physical copy/no-link runtime policy.")]
    public TransformedRealStS2PrewarmJitExecutionGateResult RunExactPrewarmJitInvocation()
    {
        const TransformedRealStS2PrewarmJitExecutionGate gate = TransformedRealStS2PrewarmJitExecutionGate.ExactPrewarmJitInvocation;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var preflight = RequirePreflight();
            var admission = RequireAdmission();
            var context = RequireLoadContext();

            if (context.ManagedResolverRequests.Count != 0 || context.PrivateLoads.Count != 0 ||
                context.InitializerBearingRequests.Count != 0 || context.RejectedManagedRequests.Count != 0 || context.NativeLoadAttempts.Count != 0)
            {
                throw new InvalidDataException("Step-34 resolver state changed before Gate C invocation. " + context.FormatResolverState());
            }

            var resolverRequestsBefore = context.ManagedResolverRequests.Count;
            var hostLoadsBefore = context.HostLoads.Count;
            var privateLoadsBefore = context.PrivateLoads.Count;
            var nativeAttemptsBefore = context.NativeLoadAttempts.Count;

            stage = "exact transformed type/member binding";
            var targetType = admission.Assembly.GetType(TargetTypeFullName, throwOnError: true, ignoreCase: false)
                ?? throw new MissingMemberException(TargetTypeFullName);
            if (!ReferenceEquals(targetType.Assembly, admission.Assembly))
                throw new InvalidDataException("Step-34 target type did not bind from the transformed sts2 assembly.");

            var method = targetType.GetMethod(
                TargetMethodName,
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                binder: null,
                types: Type.EmptyTypes,
                modifiers: null)
                ?? throw new MissingMethodException(TargetTypeFullName, TargetMethodName);

            if (!ReferenceEquals(method.DeclaringType, targetType) || !method.IsStatic || method.ReturnType != typeof(void) || method.GetParameters().Length != 0)
                throw new InvalidDataException("Step-34 reflected PrewarmJit identity/signature drifted from the exact static parameterless void target.");
            if (method.MetadataToken != unchecked((int)ClosedStep32TransformedPrewarmJitToken))
                throw new InvalidDataException($"Step-34 reflected PrewarmJit token drifted: 0x{method.MetadataToken:X8} != 0x{ClosedStep32TransformedPrewarmJitToken:X8}.");
            if (method.Module.ModuleVersionId != TransformedRealStS2AssemblyAdmission.ClosedStep32Mvid)
                throw new InvalidDataException("Step-34 reflected PrewarmJit module MVID drifted from the closed transformed image.");

            stage = "single exact transformed PrewarmJit invocation";
            try
            {
                method.Invoke(null, null);
            }
            catch (TargetInvocationException ex)
            {
                var target = ex.InnerException ?? ex;
                throw new InvalidOperationException(
                    "Step-34 transformed PrewarmJit threw during the first controlled invocation. " +
                    DescribeException(target) + "\nResolver state at failure: " + context.FormatResolverState(), target);
            }

            stage = "post-invocation resolver/native confinement";
            if (context.InitializerBearingRequests.Count != 0)
                throw new InvalidDataException("Step-34 PrewarmJit requested an initializer-bearing private dependency, which remains forbidden: " + string.Join(" | ", context.InitializerBearingRequests));
            if (context.RejectedManagedRequests.Count != 0)
                throw new InvalidDataException("Step-34 PrewarmJit triggered an unplanned managed resolver request: " + string.Join(" | ", context.RejectedManagedRequests));
            if (context.NativeLoadAttempts.Count != 0)
                throw new InvalidDataException("Step-34 PrewarmJit attempted native library resolution: " + string.Join(" | ", context.NativeLoadAttempts));

            var privateAssemblies = context.Assemblies.ToArray();
            foreach (var loaded in privateAssemblies.Where(item => !ReferenceEquals(item, admission.Assembly)))
            {
                var simple = loaded.GetName().Name ?? string.Empty;
                var prepared = preflight.PreparedAssemblies.SingleOrDefault(item =>
                    !item.Plan.IsPrimary && string.Equals(item.AssemblyName.Name, simple, StringComparison.OrdinalIgnoreCase));
                if (prepared is null)
                    throw new InvalidDataException("Step-34 private context contains an assembly outside the prepared plan: " + (loaded.GetName().FullName ?? simple));
                if (prepared.ModuleInitializerCount != 0)
                    throw new InvalidDataException("Step-34 private context admitted an initializer-bearing dependency: " + prepared.Plan.AssemblyFullName);
            }

            _execution = new ExecutionSnapshot(
                method.MetadataToken,
                context.ManagedResolverRequests.Skip(resolverRequestsBefore).ToArray(),
                context.HostLoads.Skip(hostLoadsBefore).ToArray(),
                context.PrivateLoads.Skip(privateLoadsBefore).ToArray(),
                context.NativeLoadAttempts.Skip(nativeAttemptsBefore).ToArray(),
                privateAssemblies.Select(item => item.GetName().FullName ?? item.GetName().Name ?? "<unknown>").ToArray());

            return Pass(gate,
                "FIRST CONTROLLED INVOCATION OF THE EXACT TRANSFORMED REAL-STS2 PREWARMJIT SITE RETURNED NORMALLY.\n" +
                $"Target type: {TargetTypeFullName}\n" +
                $"Target method: {TargetMethodFullName}\n" +
                $"Reflected transformed MethodDef token: 0x{method.MetadataToken:X8}\n" +
                $"Target module MVID: {method.Module.ModuleVersionId}\n" +
                "Invocation count: 1\n" +
                "Return contract: void; returned normally: YES\n" +
                $"Managed resolver requests caused by binding/invocation: {context.ManagedResolverRequests.Count - resolverRequestsBefore:N0}\n" +
                $"Exact planned host-framework loads caused by binding/invocation: {context.HostLoads.Count - hostLoadsBefore:N0}\n" +
                $"Initializer-free prepared private dependency loads caused by binding/invocation: {context.PrivateLoads.Count - privateLoadsBefore:N0}\n" +
                "Initializer-bearing private dependency requests: 0\n" +
                "Unplanned managed resolver requests: 0\n" +
                "Native resolution attempts: 0\n" +
                $"Private context assemblies after invocation: {privateAssemblies.Length:N0}\n" +
                "Receipt-backed/prepared original sts2.dll CLR-loaded: NO\n" +
                "Game entry point invoked: NO\n" +
                "Any game method other than exact PrewarmJit intentionally invoked: NO\n" +
                "Harmony/MonoMod runtime patch API invoked: NO\n" +
                "Godot/game startup requested: NO");
        }
        catch (Exception ex)
        {
            _execution = null;
            return Fail(gate, stage, ex);
        }
    }

    public async Task<TransformedRealStS2PrewarmJitExecutionGateResult> RunFinalIsolationAuditAsync(
        IProgress<TransformedRealStS2PrewarmJitExecutionProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        const TransformedRealStS2PrewarmJitExecutionGate gate = TransformedRealStS2PrewarmJitExecutionGate.FinalIsolationAudit;
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
            progress?.Report(new(gate, 0, 3, null, "Re-proving the receipt-backed install after exact transformed PrewarmJit execution."));
            var offline = await _offlineInspection.RunAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            if (offline.Outcome == SteamOfflineInstallOutcome.Cancelled)
                throw new OperationCanceledException("Step 34 final OfflineReady audit was cancelled.", cancellationToken);
            if (!offline.Success || string.IsNullOrWhiteSpace(offline.ManagedInstallRelativePath))
                throw new InvalidDataException(offline.Error ?? "Step-34 final OfflineReady re-verification failed.");

            var managedRoot = ResolveChildPath(_launcherDataRoot, offline.ManagedInstallRelativePath, "Step-34 managed install");
            var trustedPrimaryPath = ResolveChildPath(managedRoot, TransformedRealStS2AssemblyAdmission.ExactPrimaryRelativePath, "Step-34 trusted primary path");
            var trustedSha256 = ComputeSha256Hex(trustedPrimaryPath);
            if (!trustedSha256.Equals(TransformedRealStS2AssemblyAdmission.ClosedStep32SourceSha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException($"Receipt-backed original sts2.dll changed after Step-34 execution: {trustedSha256}.");
            progress?.Report(new(gate, 1, 3, trustedPrimaryPath, "Trusted receipt-backed primary remains byte-identical."));

            stage = "transformed image / plan / dependency hash reproof";
            var transformedSha256 = ComputeSha256Hex(preflight.TransformedPath);
            if (!transformedSha256.Equals(TransformedRealStS2AssemblyAdmission.ClosedStep32TransformedSha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Verified transformed sts2.dll changed after PrewarmJit execution.");
            var planSha256 = ComputeSha256Hex(_planPath);
            if (!planSha256.Equals(preflight.PlanSha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Prepared runtime-binding plan changed during Step-34 execution.");

            var verifiedPrivate = 0;
            foreach (var loaded in context.Assemblies.Where(item => !ReferenceEquals(item, admission.Assembly)))
            {
                var simple = loaded.GetName().Name ?? string.Empty;
                var prepared = preflight.PreparedAssemblies.SingleOrDefault(item =>
                    !item.Plan.IsPrimary && string.Equals(item.AssemblyName.Name, simple, StringComparison.OrdinalIgnoreCase))
                    ?? throw new InvalidDataException("Step-34 loaded private assembly is outside the prepared plan: " + (loaded.GetName().FullName ?? simple));
                if (prepared.ModuleInitializerCount != 0)
                    throw new InvalidDataException("Step-34 loaded initializer-bearing dependency during PrewarmJit execution: " + prepared.Plan.AssemblyFullName);
                VerifyFileLength(prepared.PreparedPath, prepared.Plan.Length, "loaded private dependency");
                var hash = ComputeSha1Hex(prepared.PreparedPath);
                if (!hash.Equals(prepared.Plan.Sha1Hex, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException("Step-34 loaded private dependency bytes changed: " + prepared.Plan.RelativePath);
                verifiedPrivate++;
            }

            if (context.InitializerBearingRequests.Count != 0 || context.RejectedManagedRequests.Count != 0 || context.NativeLoadAttempts.Count != 0)
                throw new InvalidDataException("Step-34 final resolver/native isolation counters are not clean. " + context.FormatResolverState());
            var matches = FindLoadedStS2Assemblies();
            if (matches.Length != 1 || !ReferenceEquals(matches[0], admission.Assembly) || !ReferenceEquals(AssemblyLoadContext.GetLoadContext(admission.Assembly), context))
                throw new InvalidDataException("Step-34 transformed-primary CLR residency/context ownership drifted during final audit.");
            if (execution.MethodToken != unchecked((int)ClosedStep32TransformedPrewarmJitToken))
                throw new InvalidDataException("Step-34 execution snapshot target token drifted during final audit.");

            progress?.Report(new(gate, 3, 3, preflight.TransformedPath, "Final source/transformed/plan/dependency/context isolation checks passed."));

            return Pass(gate,
                "STEP 34 FINAL CONTROLLED PREWARMJIT EXECUTION ISOLATION AUDIT PASSED.\n" +
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
                "Exact transformed PrewarmJit invocation count: 1\n" +
                "Receipt-backed/prepared original sts2.dll CLR-loaded: NO\n" +
                "Game entry point or broader startup invoked: NO\n" +
                "Harmony/MonoMod runtime patching invoked: NO\n" +
                "Godot/game startup: NO\n" +
                "Authorization after Step-34 PASS: a later separately gated boundary may advance into the next measured managed-initialization site; Step 34 authorizes no broader startup by itself.");
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

    private void ValidateExecutionPlan(RuntimeFrameworkBindingPlanDocument plan)
    {
        if (plan.SchemaVersion != RuntimeFrameworkBindingPlanDocument.CurrentSchemaVersion)
            throw new InvalidDataException($"Unsupported Step-21/22 runtime-binding plan schema {plan.SchemaVersion}.");
        if (!plan.RuntimeClosureReady || plan.Blockers.Length != 0 || plan.Edges.Any(edge => edge.BindingKind.StartsWith("Blocker:", StringComparison.Ordinal)))
            throw new InvalidDataException("Step 34 requires the physically established zero-blocker Step-21/22 runtime-binding plan.");
        var primary = plan.PreparedAssemblies.Where(item => item.IsPrimary).ToArray();
        if (primary.Length != 1)
            throw new InvalidDataException($"Step 34 expected exactly one prepared primary, found {primary.Length}.");
        if (!primary[0].AssemblyFullName.Equals(TransformedRealStS2AssemblyAdmission.ClosedStep32AssemblyIdentity, StringComparison.Ordinal) ||
            !plan.PrimaryAssemblyFullName.Equals(TransformedRealStS2AssemblyAdmission.ClosedStep32AssemblyIdentity, StringComparison.Ordinal))
            throw new InvalidDataException("Step-34 runtime-binding plan primary identity differs from the closed transformed assembly identity.");
        if (!primary[0].RelativePath.Equals(plan.PrimaryAssemblyRelativePath, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Step-34 runtime-binding plan primary path/entry disagree.");
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
            throw new InvalidDataException("Step-34 prepared dependency metadata inspection unexpectedly requested assembly resolution.");
        return count;
    }

    private static void RequireRewritePass(string label, RealStS2PrepareMethodRewriteGateResult result)
    {
        if (!result.Passed)
            throw new InvalidDataException($"{label} failed while Step 34 requalified the closed Step-32 transformation: {result.Detail.Replace('\n', ' ')}");
    }

    private void EnsureNoStS2Loaded(string stage)
    {
        var matches = FindLoadedStS2Assemblies();
        if (matches.Length == 0)
            return;
        var detail = string.Join(" | ", matches.Select(assembly =>
            $"{assembly.GetName().FullName} @ {AssemblyLoadContext.GetLoadContext(assembly)?.Name ?? "<unknown-context>"}"));
        throw new InvalidDataException($"Step 34 requires a fresh process at {stage}; sts2 is already CLR-resident: {detail}");
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
        => _preflight ?? throw new InvalidOperationException("Step 34 Gate A must pass before Gate B.");

    private PrimaryAdmissionSnapshot RequireAdmission()
        => _admission ?? throw new InvalidOperationException("Step 34 Gate B must pass before Gate C.");

    private ExecutionSnapshot RequireExecution()
        => _execution ?? throw new InvalidOperationException("Step 34 Gate C must pass before Gate D.");

    private Step34ExecutionLoadContext RequireLoadContext()
        => _loadContext ?? throw new InvalidOperationException("Step 34 dedicated AssemblyLoadContext is unavailable.");

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(TransformedRealStS2PrewarmJitExecution));
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
            throw new FileNotFoundException($"Step 34 {scope} is missing.", path);
        var actual = new FileInfo(path).Length;
        if (actual != expected)
            throw new InvalidDataException($"Step 34 {scope} length mismatch: {actual} != {expected}.");
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

    private static TransformedRealStS2PrewarmJitExecutionGateResult Pass(TransformedRealStS2PrewarmJitExecutionGate gate, string detail)
        => new(gate, true, detail);

    private static TransformedRealStS2PrewarmJitExecutionGateResult Fail(TransformedRealStS2PrewarmJitExecutionGate gate, string stage, Exception ex)
        => new(gate, false, $"Stage: {stage}\n{ex.GetType().Name}: {ex.Message}");

    private sealed record ExecutionPreflightSnapshot(
        string TransformedPath,
        string TransformedSha256,
        uint TransformedMethodToken,
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

    internal sealed class Step34ExecutionLoadContext : AssemblyLoadContext
    {
        private readonly IReadOnlyDictionary<string, PreparedExecutionEntry> _privateBySimpleName;
        private readonly RuntimeBindingHostFramework[] _hostBindings;

        internal Step34ExecutionLoadContext(
            string name,
            RuntimeFrameworkBindingPlanDocument plan,
            IReadOnlyList<PreparedExecutionEntry> preparedAssemblies,
            bool isCollectible)
            : base(name, isCollectible)
        {
            var privateBySimpleName = new Dictionary<string, PreparedExecutionEntry>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in preparedAssemblies.Where(item => !item.Plan.IsPrimary))
            {
                var simple = item.AssemblyName.Name ?? throw new InvalidDataException($"Prepared assembly identity has no simple name: {item.Plan.AssemblyFullName}");
                if (IsHostFrameworkContractName(simple))
                    throw new InvalidDataException($"Step-34 resolver received framework-shaped private assembly '{simple}'.");
                if (!privateBySimpleName.TryAdd(simple, item))
                    throw new InvalidDataException($"Step-34 resolver received duplicate prepared simple name '{simple}'.");
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
            Justification = "Step 34 loads only the exact hash-pinned transformed primary image and exact hash-pinned prepared private dependencies selected by the persisted runtime plan.")]
        internal Assembly LoadPrimary(string transformedPath, string expectedSha256)
        {
            var actualSha256 = ComputeSha256Hex(transformedPath);
            if (!actualSha256.Equals(expectedSha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Step-34 transformed primary hash changed immediately before LoadFromStream.");
            using var stream = new FileStream(transformedPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return LoadFromStream(stream);
        }

        [UnconditionalSuppressMessage(
            "Trimming",
            "IL2026",
            Justification = "Step 34 resolves only exact persisted host bindings and hash-pinned initializer-free prepared private assemblies.")]
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
                    var detail = $"{requestedFullName} => {privateAssembly.Plan.AssemblyFullName}; moduleInitializers={privateAssembly.ModuleInitializerCount}";
                    InitializerBearingRequests.Add(detail);
                    throw new FileLoadException(
                        "Step 34 refuses initializer-bearing private dependencies during the PrewarmJit boundary: " + detail);
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

                VerifyFileLength(privateAssembly.PreparedPath, privateAssembly.Plan.Length, "prepared private dependency");
                var hash = ComputeSha1Hex(privateAssembly.PreparedPath);
                if (!hash.Equals(privateAssembly.Plan.Sha1Hex, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException("Step-34 prepared private dependency SHA-1 changed immediately before load: " + privateAssembly.Plan.RelativePath);

                using var stream = new FileStream(privateAssembly.PreparedPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                var loaded = LoadFromStream(stream);
                var actualFullName = loaded.GetName().FullName ?? loaded.GetName().Name ?? string.Empty;
                if (!actualFullName.Equals(privateAssembly.Plan.AssemblyFullName, StringComparison.Ordinal))
                    throw new FileLoadException($"Step-34 private dependency loaded identity drifted. Planned '{privateAssembly.Plan.AssemblyFullName}', actual '{actualFullName}'.");
                PrivateLoads.Add($"{requestedFullName} => {actualFullName}");
                return loaded;
            }

            var hostMatches = _hostBindings
                .Where(binding => ExactRequestedIdentity(assemblyName, new AssemblyName(binding.RequestedFullName)))
                .ToArray();
            if (hostMatches.Length == 0)
                return Reject(requestedFullName, "request is neither an exact planned host-framework binding nor an identified prepared private dependency");

            var allowedActual = hostMatches
                .Select(binding => binding.ActualFullName)
                .Distinct(StringComparer.Ordinal)
                .ToHashSet(StringComparer.Ordinal);
            var hostAssembly = AssemblyLoadContext.Default.LoadFromAssemblyName(assemblyName);
            var hostFullName = hostAssembly.GetName().FullName ?? hostAssembly.GetName().Name ?? string.Empty;
            if (!allowedActual.Contains(hostFullName))
                throw new FileLoadException(
                    $"Step-34 host binding drift for '{requestedFullName}'. Planned actual identity: {string.Join(" | ", allowedActual)}; runtime actual: {hostFullName}.");
            HostLoads.Add($"{requestedFullName} => {hostFullName}");
            return hostAssembly;
        }

        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            NativeLoadAttempts.Add(unmanagedDllName);
            throw new DllNotFoundException(
                $"Step 34 controlled PrewarmJit boundary refuses native library resolution for '{unmanagedDllName}'.");
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
            throw new FileLoadException("Step-34 strict managed resolver rejected an unplanned request: " + detail);
        }
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
