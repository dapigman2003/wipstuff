using System.Reflection;
using System.Runtime.Loader;
using Mono.Cecil;

namespace StS2Launcher.Core;

/// <summary>
/// Step 36.0 boundary. This phase is intentionally available only after the exact Step-35 core closure has
/// completed in the same process. It preserves the exact closed Step-32 transformed sts2 assembly and exact
/// prepared GodotSharp bridge established by Step 35, statically re-proves ExecuteEssential against the exact
/// source/transformed pair, invokes only ExecuteEssential once, and then re-proves isolation. ExecuteDeferred,
/// PrewarmJit, the game entry point, Harmony/MonoMod runtime patching, arbitrary resolver fallback, and native
/// game loading remain forbidden.
/// </summary>
public sealed partial class TransformedRealStS2VeryEarlyInitialization
{
    public const string EssentialTargetMethodName = "ExecuteEssential";
    public const string EssentialTargetMethodFullName = "System.Void MegaCrit.Sts2.Core.Helpers.OneTimeInitialization::ExecuteEssential()";
    public const uint SourceEssentialTargetMethodToken = 0x06007D03;
    public const int ExpectedStateAfterVeryEarly = 1;
    public const int ExpectedStateAfterEssential = 2;

    private bool _exactStep35CoreClosurePassed;
    private Step36BaselineSnapshot? _step36Baseline;
    private EssentialPreflightSnapshot? _essentialPreflight;
    private EssentialBindingSnapshot? _essentialBinding;
    private EssentialExecutionSnapshot? _essentialExecution;

    public bool ExactStep35CoreClosurePassed => _exactStep35CoreClosurePassed;

    public string GetVerifiedEssentialStaticInstructionMap()
        => _essentialPreflight?.StaticInstructionMap
           ?? throw new InvalidOperationException("Step 36.0 Gate A has not produced a verified ExecuteEssential static map.");

    private void ResetStep36State()
    {
        _exactStep35CoreClosurePassed = false;
        _step36Baseline = null;
        _essentialPreflight = null;
        _essentialBinding = null;
        _essentialExecution = null;
    }

    private void MarkExactStep35CoreClosurePassed(Step35ExecutionLoadContext context)
    {
        _exactStep35CoreClosurePassed = true;
        _step36Baseline = CaptureStep36Baseline(context);
        _essentialPreflight = null;
        _essentialBinding = null;
        _essentialExecution = null;
    }

    public TransformedRealStS2EssentialInitializationGateResult RunEssentialStaticPreflight(
        Action<string>? checkpoint = null)
    {
        const TransformedRealStS2EssentialInitializationGate gate = TransformedRealStS2EssentialInitializationGate.ExactStep35ClosureAndStaticPreflight;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            RequireExactStep35CoreClosure("Step 36 Gate A entry");
            var preflight = RequirePreflight();
            var admission = RequireAdmission();
            var context = RequireLoadContext();
            RequireStep36BaselineUnchanged(context, "Gate A entry");
            if (!IsExactAuthorityMode)
                throw new InvalidOperationException("Step 36.0 requires the exact-authority Step-35 mode; diagnostic derivatives are not accepted.");

            Checkpoint(checkpoint, "E_A_ENTRY — exact Step-35 core closure baseline present; beginning read-only ExecuteEssential source/transformed audit.");
            stage = "source/transformed ExecuteEssential semantic audit";
            var sourcePath = Path.Combine(
                _launcherDataRoot,
                RealStS2PrepareMethodRewrite.WorkRootName,
                RealStS2PrepareMethodRewrite.SourceRootName,
                RealStS2PrepareMethodRewrite.PrimaryFileName);
            VerifyFileLength(sourcePath, ClosedSourceBytes, "Step-36 exact source primary");
            var sourceSha256 = ComputeSha256Hex(sourcePath);
            if (!sourceSha256.Equals(TransformedRealStS2AssemblyAdmission.ClosedStep32SourceSha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Step 36.0 exact source SHA-256 drifted from the physically closed Step-32 source.");
            VerifyFileLength(preflight.TransformedPath, TransformedRealStS2AssemblyAdmission.ClosedStep32TransformedBytes, "Step-36 exact transformed primary");
            var transformedSha256 = ComputeSha256Hex(preflight.TransformedPath);
            if (!transformedSha256.Equals(TransformedRealStS2AssemblyAdmission.ClosedStep32TransformedSha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Step 36.0 exact transformed SHA-256 drifted from the physically closed Step-32 transform.");
            if (!admission.ImmediateSha256.Equals(transformedSha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Step 36.0 CLR-resident sts2 authority is not the exact closed transformed image.");

            uint transformedToken;
            string semanticSha256;
            string staticMap;
            using (var sourceResolver = new RejectingAssemblyResolver())
            using (var transformedResolver = new RejectingAssemblyResolver())
            using (var sourceModule = ModuleDefinition.ReadModule(sourcePath, new ReaderParameters
                   {
                       ReadSymbols = false,
                       ReadingMode = ReadingMode.Deferred,
                       AssemblyResolver = sourceResolver,
                   }))
            using (var transformedModule = ModuleDefinition.ReadModule(preflight.TransformedPath, new ReaderParameters
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
                    throw new InvalidDataException("Step 36.0 source/transformed identity or MVID drifted from the closed Step-32 authority.");
                }

                var sourceMethod = FindMethodByToken(sourceModule, SourceEssentialTargetMethodToken);
                if (sourceMethod.DeclaringType.FullName != TargetTypeFullName || sourceMethod.FullName != EssentialTargetMethodFullName)
                    throw new InvalidDataException($"Step 36.0 source token 0x{SourceEssentialTargetMethodToken:X8} no longer identifies {EssentialTargetMethodFullName}.");
                RequireEssentialSignature(sourceMethod, "source");
                var transformedMethod = RealStS2PrepareMethodRewrite.FindMethodByStableIdentity(
                    transformedModule,
                    TargetTypeFullName,
                    EssentialTargetMethodFullName);
                RequireEssentialSignature(transformedMethod, "transformed");

                semanticSha256 = RealStS2PrepareMethodRewrite.ComputeMethodSemanticFingerprint(sourceMethod);
                var transformedSemantic = RealStS2PrepareMethodRewrite.ComputeMethodSemanticFingerprint(transformedMethod);
                if (!semanticSha256.Equals(transformedSemantic, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException("Step 36.0 ExecuteEssential semantics drifted across the closed Step-32 serialization.");

                var forbiddenSourceCalls = CountForbiddenEssentialBoundaryCalls(sourceMethod);
                var forbiddenTransformedCalls = CountForbiddenEssentialBoundaryCalls(transformedMethod);
                if (forbiddenSourceCalls != 0 || forbiddenTransformedCalls != 0)
                    throw new InvalidDataException($"Step 36.0 ExecuteEssential crosses a forbidden later OneTimeInitialization boundary; source={forbiddenSourceCalls}; transformed={forbiddenTransformedCalls}.");
                if (CountHarmonyMethodReferences(sourceMethod) != 0 || CountHarmonyMethodReferences(transformedMethod) != 0)
                    throw new InvalidDataException("Step 36.0 ExecuteEssential directly references Harmony, which remains outside this boundary.");

                transformedToken = transformedMethod.MetadataToken.ToUInt32();
                staticMap = BuildEssentialStaticInstructionMap(sourceMethod, transformedMethod, semanticSha256);
                if (sourceResolver.Requests.Count != 0 || transformedResolver.Requests.Count != 0)
                    throw new InvalidDataException("Step 36.0 static ExecuteEssential audit unexpectedly resolved a dependency through Cecil.");
            }

            _essentialPreflight = new EssentialPreflightSnapshot(
                sourcePath,
                sourceSha256,
                preflight.TransformedPath,
                transformedSha256,
                transformedToken,
                semanticSha256,
                staticMap,
                ComputeSha256Hex(_planPath));
            _essentialBinding = null;
            _essentialExecution = null;
            Checkpoint(checkpoint, $"E_A_PASS — exact source/transformed ExecuteEssential semantics matched; sourceToken=0x{SourceEssentialTargetMethodToken:X8}; transformedToken=0x{transformedToken:X8}; semanticSha256={semanticSha256}; no ExecuteDeferred/PrewarmJit/Harmony crossover.");
            return EssentialPass(gate,
                $"Exact Step-35 core closure prerequisite: PASS\n" +
                $"Source ExecuteEssential token: 0x{SourceEssentialTargetMethodToken:X8}\n" +
                $"Transformed ExecuteEssential token: 0x{transformedToken:X8}\n" +
                $"Signature: {EssentialTargetMethodFullName}\n" +
                $"Semantic fingerprint source/transformed: {semanticSha256}\n" +
                "Direct ExecuteDeferred/PrewarmJit/ExecuteVeryEarly calls: 0\n" +
                "Direct Harmony references: 0\n" +
                "Cecil dependency resolution requests: 0");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"E_A_FAIL — stage={stage}; {ex.GetType().FullName}: {ex.Message}");
            _essentialPreflight = null;
            _essentialBinding = null;
            _essentialExecution = null;
            return EssentialFail(gate, stage, ex);
        }
    }

    public TransformedRealStS2EssentialInitializationGateResult RunEssentialAuthorityBinding(
        Action<string>? checkpoint = null)
    {
        const TransformedRealStS2EssentialInitializationGate gate = TransformedRealStS2EssentialInitializationGate.ExactAuthorityContinuityAndBinding;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            RequireExactStep35CoreClosure("Step 36 Gate B entry");
            var preflight = RequireEssentialPreflight();
            var admission = RequireAdmission();
            var context = RequireLoadContext();
            RequireStep36BaselineUnchanged(context, "Gate B entry");
            Checkpoint(checkpoint, "E_B_ENTRY — exact Step-35 authority/context continuity accepted; binding exact ExecuteEssential without invoking it.");

            stage = "exact ExecuteEssential reflection binding";
            var targetType = admission.Assembly.GetType(TargetTypeFullName, throwOnError: true, ignoreCase: false)
                ?? throw new MissingMemberException(TargetTypeFullName);
            var method = targetType.GetMethod(
                EssentialTargetMethodName,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static,
                binder: null,
                types: Type.EmptyTypes,
                modifiers: null)
                ?? throw new MissingMethodException(TargetTypeFullName, EssentialTargetMethodName);
            if (!method.IsStatic || method.GetParameters().Length != 0 || method.ReturnType != typeof(void))
                throw new InvalidDataException("Step 36.0 reflected ExecuteEssential no longer has the exact static parameterless void contract.");
            if (method.MetadataToken != unchecked((int)preflight.TransformedMethodToken))
                throw new InvalidDataException($"Step 36.0 reflected ExecuteEssential token drifted: 0x{method.MetadataToken:X8} != 0x{preflight.TransformedMethodToken:X8}.");
            if (method.Module.ModuleVersionId != TransformedRealStS2AssemblyAdmission.ClosedStep32Mvid)
                throw new InvalidDataException("Step 36.0 reflected ExecuteEssential module MVID drifted from the closed transformed image.");
            if (admission.Assembly.GetType(DiagnosticBridgeTypeFullName, throwOnError: false, ignoreCase: false) is not null)
                throw new InvalidDataException("Step 36.0 exact sts2 authority unexpectedly contains the Step-35 diagnostic checkpoint bridge.");

            var stateField = targetType.GetField("_state", BindingFlags.Static | BindingFlags.NonPublic)
                ?? throw new MissingFieldException(TargetTypeFullName, "_state");
            var stateBefore = ReadOneTimeInitializationState(stateField);
            if (stateBefore != ExpectedStateAfterVeryEarly)
                throw new InvalidDataException($"Step 36.0 requires OneTimeInitialization state {ExpectedStateAfterVeryEarly} after exact ExecuteVeryEarly; observed {stateBefore}.");
            RequireStep36BaselineUnchanged(context, "Gate B post-binding");

            _essentialBinding = new EssentialBindingSnapshot(method, stateField, stateBefore);
            _essentialExecution = null;
            Checkpoint(checkpoint, $"E_B_PASS — exact ExecuteEssential MethodInfo bound; token=0x{method.MetadataToken:X8}; stateBefore={stateBefore}; resolver baseline unchanged.");
            return EssentialPass(gate,
                $"Exact transformed sts2 authority continuity: PASS\n" +
                $"ExecuteEssential token: 0x{method.MetadataToken:X8}\n" +
                $"MVID: {method.Module.ModuleVersionId}\n" +
                $"OneTimeInitialization state before ExecuteEssential: {stateBefore}\n" +
                "Diagnostic sts2 bridge present: NO\n" +
                "Resolver/native state changed during binding: NO");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"E_B_FAIL — stage={stage}; {ex.GetType().FullName}: {ex.Message}");
            _essentialBinding = null;
            _essentialExecution = null;
            return EssentialFail(gate, stage, ex);
        }
    }

    public TransformedRealStS2EssentialInitializationGateResult RunExactExecuteEssentialInvocation(
        Action<string>? checkpoint = null)
    {
        const TransformedRealStS2EssentialInitializationGate gate = TransformedRealStS2EssentialInitializationGate.ExecuteEssentialInvocation;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            RequireExactStep35CoreClosure("Step 36 Gate C entry");
            var binding = RequireEssentialBinding();
            var context = RequireLoadContext();
            RequireStep36BaselineUnchanged(context, "Gate C pre-invoke");
            var stateBefore = ReadOneTimeInitializationState(binding.StateField);
            if (stateBefore != ExpectedStateAfterVeryEarly)
                throw new InvalidDataException($"Step 36.0 pre-invoke state drifted: expected {ExpectedStateAfterVeryEarly}, observed {stateBefore}.");

            stage = "single exact ExecuteEssential invocation";
            Checkpoint(checkpoint, $"E_C_INVOKE_START — invoking exact transformed ExecuteEssential once on managedThread={Environment.CurrentManagedThreadId}; stateBefore={stateBefore}. This synchronous boundary has no launcher retry in the same process.");
            try
            {
                binding.Method.Invoke(null, null);
            }
            catch (TargetInvocationException ex) when (ex.InnerException is not null)
            {
                throw new InvalidOperationException(
                    $"Exact transformed ExecuteEssential threw {ex.InnerException.GetType().FullName}: {ex.InnerException.Message}",
                    ex.InnerException);
            }
            Checkpoint(checkpoint, "E_C_INVOKE_RETURNED — exact transformed ExecuteEssential returned to the launcher.");

            var stateAfter = ReadOneTimeInitializationState(binding.StateField);
            if (stateAfter != ExpectedStateAfterEssential)
                throw new InvalidDataException($"Step 36.0 ExecuteEssential returned but OneTimeInitialization state was {stateAfter}; expected {ExpectedStateAfterEssential}.");
            if (context.InitializerBearingRequests.Count != 0)
                throw new InvalidDataException("Step 36.0 ExecuteEssential requested an initializer-bearing private dependency: " + string.Join(" | ", context.InitializerBearingRequests));
            if (context.RejectedManagedRequests.Count != 0)
                throw new InvalidDataException("Step 36.0 ExecuteEssential triggered an unplanned managed resolver request: " + string.Join(" | ", context.RejectedManagedRequests));
            if (context.NativeLoadAttempts.Count != 0)
                throw new InvalidDataException("Step 36.0 ExecuteEssential attempted native library resolution: " + string.Join(" | ", context.NativeLoadAttempts));

            _essentialExecution = new EssentialExecutionSnapshot(
                binding.Method.MetadataToken,
                stateBefore,
                stateAfter,
                context.ManagedResolverRequests.ToArray(),
                context.HostLoads.ToArray(),
                context.PrivateLoads.ToArray(),
                context.Assemblies.Select(a => a.GetName().FullName ?? a.GetName().Name ?? "<unknown>").OrderBy(x => x, StringComparer.Ordinal).ToArray());
            Checkpoint(checkpoint, $"E_C_PASS — exact ExecuteEssential returned; stateAfter={stateAfter}; {context.FormatResolverState()}.");
            return EssentialPass(gate,
                "Exact transformed ExecuteEssential invocation: PASS\n" +
                $"State transition: {stateBefore} -> {stateAfter}\n" +
                $"Managed resolver requests total: {context.ManagedResolverRequests.Count}\n" +
                $"Host framework loads total: {context.HostLoads.Count}\n" +
                $"Private dependency loads total: {context.PrivateLoads.Count}\n" +
                "Initializer-bearing requests: 0\n" +
                "Rejected managed requests: 0\n" +
                "Native library resolution attempts: 0\n" +
                "ExecuteDeferred / PrewarmJit / entry point intentionally invoked by launcher: NO");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"E_C_FAIL — stage={stage}; {ex.GetType().FullName}: {ex.Message}");
            _essentialExecution = null;
            return EssentialFail(gate, stage, ex);
        }
    }

    public async Task<TransformedRealStS2EssentialInitializationGateResult> RunEssentialFinalIsolationAuditAsync(
        IProgress<TransformedRealStS2EssentialInitializationProgress>? progress = null,
        CancellationToken cancellationToken = default,
        Action<string>? checkpoint = null)
    {
        const TransformedRealStS2EssentialInitializationGate gate = TransformedRealStS2EssentialInitializationGate.FinalIsolationAudit;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            RequireExactStep35CoreClosure("Step 36 Gate D entry");
            var essentialPreflight = RequireEssentialPreflight();
            var execution = RequireEssentialExecution();
            var admission = RequireAdmission();
            var context = RequireLoadContext();
            cancellationToken.ThrowIfCancellationRequested();
            Checkpoint(checkpoint, "E_D_AUDIT_ENTRY — final Step-36 isolation audit entered.");

            stage = "post-ExecuteEssential OfflineReady reproof";
            progress?.Report(new(gate, 0, 4, null, "Re-proving receipt-backed OfflineReady after exact ExecuteEssential."));
            IProgress<SteamOfflineInstallProgress>? offlineProgress = progress is null
                ? null
                : new CallbackProgress<SteamOfflineInstallProgress>(value =>
                    progress.Report(new(
                        gate,
                        value.CompletedFiles,
                        value.TotalFiles,
                        value.CurrentFile,
                        $"Step 36 Gate D OfflineReady {value.Phase} — {value.Message}",
                        value.CompletedBytes,
                        value.TotalBytes,
                        value.Phase.ToString())));
            var offline = await _offlineInspection.RunAsync(offlineProgress, cancellationToken).ConfigureAwait(false);
            Checkpoint(checkpoint, $"E_D_OFFLINE_READY_RETURNED — outcome={offline.Outcome}; success={offline.Success}; verifiedFiles={offline.VerifiedFiles}; plannedFiles={offline.PlannedFiles}.");
            if (offline.Outcome == SteamOfflineInstallOutcome.Cancelled)
                throw new OperationCanceledException("Step 36.0 final OfflineReady audit was cancelled.", cancellationToken);
            if (!offline.Success || string.IsNullOrWhiteSpace(offline.ManagedInstallRelativePath))
                throw new InvalidDataException(offline.Error ?? "Step 36.0 final OfflineReady re-verification failed.");

            var managedRoot = ResolveChildPath(_launcherDataRoot, offline.ManagedInstallRelativePath, "Step-36 managed install");
            var trustedPrimaryPath = ResolveChildPath(managedRoot, TransformedRealStS2AssemblyAdmission.ExactPrimaryRelativePath, "Step-36 trusted primary path");
            var trustedSha256 = ComputeSha256Hex(trustedPrimaryPath);
            if (!trustedSha256.Equals(TransformedRealStS2AssemblyAdmission.ClosedStep32SourceSha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Step 36.0 receipt-backed original sts2.dll changed after ExecuteEssential.");
            progress?.Report(new(gate, 1, 4, trustedPrimaryPath, "Receipt-backed original remains byte-identical."));

            stage = "exact transformed / plan / dependency reproof";
            var transformedSha256 = ComputeSha256Hex(essentialPreflight.TransformedPath);
            if (!transformedSha256.Equals(TransformedRealStS2AssemblyAdmission.ClosedStep32TransformedSha256, StringComparison.OrdinalIgnoreCase) ||
                !admission.ImmediateSha256.Equals(transformedSha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Step 36.0 exact transformed authority hash drifted after ExecuteEssential.");
            var planSha256 = ComputeSha256Hex(_planPath);
            if (!planSha256.Equals(essentialPreflight.PlanSha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Step 36.0 prepared runtime-binding plan changed after ExecuteEssential.");
            progress?.Report(new(gate, 2, 4, essentialPreflight.TransformedPath, "Exact transformed authority and runtime-binding plan remain byte-identical."));

            var step35Preflight = RequirePreflight();
            var verifiedPrivate = 0;
            foreach (var loaded in context.Assemblies.Where(item => !ReferenceEquals(item, admission.Assembly)))
            {
                var simple = loaded.GetName().Name ?? string.Empty;
                var prepared = step35Preflight.PreparedAssemblies.SingleOrDefault(item =>
                    !item.Plan.IsPrimary && string.Equals(item.AssemblyName.Name, simple, StringComparison.OrdinalIgnoreCase))
                    ?? throw new InvalidDataException("Step 36.0 loaded private assembly is outside the prepared plan: " + (loaded.GetName().FullName ?? simple));
                if (prepared.ModuleInitializerCount != 0)
                    throw new InvalidDataException("Step 36.0 loaded initializer-bearing private dependency: " + prepared.Plan.AssemblyFullName);
                VerifyFileLength(prepared.PreparedPath, prepared.Plan.Length, "Step-36 loaded private dependency");
                var hash = ComputeSha1Hex(prepared.PreparedPath);
                if (!hash.Equals(prepared.Plan.Sha1Hex, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException("Step 36.0 loaded private dependency bytes changed: " + prepared.Plan.RelativePath);
                verifiedPrivate++;
            }
            if (context.InitializerBearingRequests.Count != 0 || context.RejectedManagedRequests.Count != 0 || context.NativeLoadAttempts.Count != 0)
                throw new InvalidDataException("Step 36.0 final resolver/native isolation counters are not clean. " + context.FormatResolverState());
            var matches = FindLoadedStS2Assemblies();
            if (matches.Length != 1 || !ReferenceEquals(matches[0], admission.Assembly) || !ReferenceEquals(AssemblyLoadContext.GetLoadContext(admission.Assembly), context))
                throw new InvalidDataException("Step 36.0 exact resident sts2 identity/context ownership drifted during final audit.");
            if (execution.MethodToken != unchecked((int)essentialPreflight.TransformedMethodToken))
                throw new InvalidDataException("Step 36.0 ExecuteEssential execution token drifted during final audit.");
            var finalState = ReadOneTimeInitializationState(RequireEssentialBinding().StateField);
            if (finalState != ExpectedStateAfterEssential)
                throw new InvalidDataException($"Step 36.0 final OneTimeInitialization state drifted: expected {ExpectedStateAfterEssential}, observed {finalState}.");
            progress?.Report(new(gate, 3, 4, essentialPreflight.TransformedPath, "Resolver/context/state isolation checks passed."));

            Checkpoint(checkpoint, $"E_D_FINAL_CHECKS_PASS — exact authority/plan/dependency/resolver/context/state checks passed; state={finalState}; verifiedPrivate={verifiedPrivate}; {context.FormatResolverState()}.");
            var result = EssentialPass(gate,
                "STEP 36.0 FINAL ISOLATION AUDIT PASSED.\n" +
                $"OfflineReady: PASS ({offline.VerifiedFiles:N0}/{offline.PlannedFiles:N0} files)\n" +
                $"Receipt-backed original SHA-256 unchanged: {trustedSha256}\n" +
                $"Exact transformed SHA-256 unchanged: {transformedSha256}\n" +
                $"Runtime-binding plan SHA-256 unchanged: {planSha256}\n" +
                $"OneTimeInitialization final state: {finalState}\n" +
                $"Initializer-free private dependencies resident/re-hashed: {verifiedPrivate}\n" +
                "Initializer-bearing requests: 0\n" +
                "Rejected managed requests: 0\n" +
                "Native game resolution/loading: NO\n" +
                "ExecuteDeferred / PrewarmJit / game entry point intentionally invoked by launcher: NO");
            progress?.Report(new(gate, 4, 4, essentialPreflight.TransformedPath, "Step 36.0 final isolation audit complete."));
            Checkpoint(checkpoint, "E_D_TASK_RETURN_START — returning completed Step-36 Gate-D result.");
            return result;
        }
        catch (OperationCanceledException)
        {
            Checkpoint(checkpoint, $"E_D_CANCELLED_INCONCLUSIVE — stage={stage}.");
            throw;
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"E_D_FAIL — stage={stage}; {ex.GetType().FullName}: {ex.Message}");
            return EssentialFail(gate, stage, ex);
        }
    }

    private void RequireExactStep35CoreClosure(string boundary)
    {
        if (!_exactStep35CoreClosurePassed || _step36Baseline is null)
            throw new InvalidOperationException($"{boundary} requires a successful exact Step-35 core closure in this same process. Run Step 15 A-C, then Step 35 EXACT-CLOSURE 4/4 first.");
        if (!IsExactAuthorityMode)
            throw new InvalidOperationException($"{boundary} requires Step35DiagnosticMode.GodotCoreExactClosure.");
        if (_callbackHandoff is null || !_managedPluginReverseBridgePrepared)
            throw new InvalidOperationException($"{boundary} requires the physically proven Godot managed/native bridge to remain installed.");
    }

    private void RequireStep36BaselineUnchanged(Step35ExecutionLoadContext context, string boundary)
    {
        var expected = _step36Baseline ?? throw new InvalidOperationException("Step 36.0 baseline is absent.");
        if (!SequenceEqual(expected.ManagedResolverRequests, context.ManagedResolverRequests) ||
            !SequenceEqual(expected.HostLoads, context.HostLoads) ||
            !SequenceEqual(expected.PrivateLoads, context.PrivateLoads) ||
            context.InitializerBearingRequests.Count != 0 ||
            context.RejectedManagedRequests.Count != 0 ||
            context.NativeLoadAttempts.Count != 0)
        {
            throw new InvalidDataException($"Step 36.0 resolver state changed before its authorized invocation at {boundary}. {context.FormatResolverState()}");
        }
    }

    private static bool SequenceEqual(IReadOnlyList<string> expected, IReadOnlyList<string> actual)
        => expected.Count == actual.Count && expected.SequenceEqual(actual, StringComparer.Ordinal);

    private static Step36BaselineSnapshot CaptureStep36Baseline(Step35ExecutionLoadContext context)
        => new(
            context.ManagedResolverRequests.ToArray(),
            context.HostLoads.ToArray(),
            context.PrivateLoads.ToArray());

    private EssentialPreflightSnapshot RequireEssentialPreflight()
        => _essentialPreflight ?? throw new InvalidOperationException("Step 36.0 Gate A must pass before Gate B.");

    private EssentialBindingSnapshot RequireEssentialBinding()
        => _essentialBinding ?? throw new InvalidOperationException("Step 36.0 Gate B must pass before Gate C.");

    private EssentialExecutionSnapshot RequireEssentialExecution()
        => _essentialExecution ?? throw new InvalidOperationException("Step 36.0 Gate C must pass before Gate D.");

    private static void RequireEssentialSignature(MethodDefinition method, string scope)
    {
        if (!method.IsStatic || method.Parameters.Count != 0 || method.ReturnType.FullName != "System.Void" || !method.HasBody)
            throw new InvalidDataException($"Step 36.0 {scope} ExecuteEssential no longer has the exact static parameterless managed-IL void contract.");
    }

    private static int CountForbiddenEssentialBoundaryCalls(MethodDefinition method)
    {
        var forbidden = new HashSet<string>(StringComparer.Ordinal) { "ExecuteVeryEarly", "ExecuteDeferred", "PrewarmJit" };
        return method.Body.Instructions.Count(instruction =>
            instruction.Operand is MethodReference reference &&
            reference.DeclaringType.FullName == TargetTypeFullName &&
            forbidden.Contains(reference.Name));
    }

    private static string BuildEssentialStaticInstructionMap(MethodDefinition sourceMethod, MethodDefinition transformedMethod, string semanticSha256)
    {
        var directCalls = transformedMethod.Body.Instructions
            .Where(i => i.Operand is MethodReference)
            .Select(i => (MethodReference)i.Operand)
            .Select(reference => reference.FullName)
            .ToArray();
        var lines = new List<string>
        {
            "StS2 Launcher — Step 36.0 ExecuteEssential static IL/callsite map",
            "Read-only evidence from exact source/transformed images; never consumed as trusted runtime input.",
            $"Source method: token=0x{sourceMethod.MetadataToken.ToUInt32():X8}; {sourceMethod.FullName}",
            $"Transformed method: token=0x{transformedMethod.MetadataToken.ToUInt32():X8}; {transformedMethod.FullName}",
            $"Semantic fingerprint source/transformed: {semanticSha256}",
            $"Transformed instructions={transformedMethod.Body.Instructions.Count}; handlers={transformedMethod.Body.ExceptionHandlers.Count}; locals={transformedMethod.Body.Variables.Count}",
            $"Direct method-reference operands={directCalls.Length}",
            "Direct calls/references:",
        };
        foreach (var call in directCalls)
            lines.Add("  - " + call);
        lines.Add(string.Empty);
        lines.Add("[TRANSFORMED EXECUTEESSENTIAL IL]");
        AppendInstructionMap(lines, transformedMethod);
        return string.Join("\n", lines) + "\n";
    }

    private static int ReadOneTimeInitializationState(FieldInfo stateField)
    {
        var value = stateField.GetValue(null) ?? throw new InvalidDataException("OneTimeInitialization._state returned null.");
        return Convert.ToInt32(value, System.Globalization.CultureInfo.InvariantCulture);
    }

    private static TransformedRealStS2EssentialInitializationGateResult EssentialPass(
        TransformedRealStS2EssentialInitializationGate gate,
        string detail)
        => new(gate, true, detail);

    private static TransformedRealStS2EssentialInitializationGateResult EssentialFail(
        TransformedRealStS2EssentialInitializationGate gate,
        string stage,
        Exception ex)
        => new(gate, false, $"Stage: {stage}\n{ex.GetType().Name}: {ex.Message}");

    private sealed record Step36BaselineSnapshot(
        string[] ManagedResolverRequests,
        string[] HostLoads,
        string[] PrivateLoads);

    private sealed record EssentialPreflightSnapshot(
        string SourcePath,
        string SourceSha256,
        string TransformedPath,
        string TransformedSha256,
        uint TransformedMethodToken,
        string SemanticSha256,
        string StaticInstructionMap,
        string PlanSha256);

    private sealed record EssentialBindingSnapshot(
        MethodInfo Method,
        FieldInfo StateField,
        int StateBefore);

    private sealed record EssentialExecutionSnapshot(
        int MethodToken,
        int StateBefore,
        int StateAfter,
        string[] ManagedResolverRequests,
        string[] HostLoads,
        string[] PrivateLoads,
        string[] PrivateContextAssemblies);
}
