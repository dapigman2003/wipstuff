using System.Reflection;
using System.Runtime.Loader;
using System.Text.Json;
using Mono.Cecil;

namespace StS2Launcher.Core;

/// <summary>
/// Step 36.0.2 boundary. This phase is intentionally available only after the exact Step-35 core closure has
/// completed in the same process. It preserves the exact closed Step-32 transformed sts2 assembly and exact
/// prepared GodotSharp bridge established by Step 35, statically re-proves ExecuteEssential against the exact
/// source/transformed pair, mounts the exact receipt-backed game PCK into the live Godot resource filesystem, invokes only ExecuteEssential once, and then re-proves isolation. ExecuteDeferred,
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
    public const string GameResourcePackRelativePath = "SlayTheSpire2.app/Contents/Resources/Slay the Spire 2.pck";
    public const string RequiredLocalizationProbePath = "res://localization/eng";

    private bool _exactStep35CoreClosurePassed;
    private Step36BaselineSnapshot? _step36Baseline;
    private EssentialPreflightSnapshot? _essentialPreflight;
    private EssentialBindingSnapshot? _essentialBinding;
    private EssentialResourcePackHandoffSnapshot? _essentialResourcePackHandoff;
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
        _essentialResourcePackHandoff = null;
        _essentialExecution = null;
    }

    private void MarkExactStep35CoreClosurePassed(Step35ExecutionLoadContext context, string managedInstallRoot)
    {
        _exactStep35CoreClosurePassed = true;
        _step36Baseline = CaptureStep36Baseline(context, managedInstallRoot);
        _essentialPreflight = null;
        _essentialBinding = null;
        _essentialResourcePackHandoff = null;
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
            _essentialResourcePackHandoff = null;
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
            _essentialResourcePackHandoff = null;
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

            stage = "receipt-backed game resource-pack handoff";
            var resourcePack = ResolveReceiptBackedGameResourcePack(checkpoint);
            var resourceHandoff = MountExactGameResourcePackAndProbe(resourcePack, context, checkpoint);
            RequireStep36BaselineUnchanged(context, "Gate B post-resource-pack handoff");

            _essentialBinding = new EssentialBindingSnapshot(method, stateField, stateBefore);
            _essentialResourcePackHandoff = resourceHandoff;
            _essentialExecution = null;
            Checkpoint(checkpoint, $"E_B_PASS — exact ExecuteEssential MethodInfo bound; token=0x{method.MetadataToken:X8}; stateBefore={stateBefore}; exact game PCK mounted; localizationProbe={resourceHandoff.LocalizationProbePath}; resolver baseline unchanged.");
            return EssentialPass(gate,
                $"Exact transformed sts2 authority continuity: PASS\n" +
                $"ExecuteEssential token: 0x{method.MetadataToken:X8}\n" +
                $"MVID: {method.Module.ModuleVersionId}\n" +
                $"OneTimeInitialization state before ExecuteEssential: {stateBefore}\n" +
                $"Receipt-backed game resource pack: {resourceHandoff.PackRelativePath} ({resourceHandoff.PackLength:N0} bytes; receipt SHA-1 {resourceHandoff.ReceiptSha1})\n" +
                $"Godot resource-pack mount returned: PASS (replaceFiles=false)\n" +
                $"Localization directory probe: {resourceHandoff.LocalizationProbePath} => PRESENT\n" +
                "Diagnostic sts2 bridge present: NO\n" +
                "Resolver/native state changed during binding/resource handoff: NO");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"E_B_FAIL — stage={stage}; {ex.GetType().FullName}: {ex.Message}");
            _essentialBinding = null;
            _essentialResourcePackHandoff = null;
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
            var resourceHandoff = RequireEssentialResourcePackHandoff();
            var context = RequireLoadContext();
            RequireStep36BaselineUnchanged(context, "Gate C pre-invoke");
            var stateBefore = ReadOneTimeInitializationState(binding.StateField);
            if (stateBefore != ExpectedStateAfterVeryEarly)
                throw new InvalidDataException($"Step 36.0 pre-invoke state drifted: expected {ExpectedStateAfterVeryEarly}, observed {stateBefore}.");

            stage = "single exact ExecuteEssential invocation";
            Checkpoint(checkpoint, $"E_C_INVOKE_START — invoking exact transformed ExecuteEssential once on managedThread={Environment.CurrentManagedThreadId}; stateBefore={stateBefore}; gamePackMounted={resourceHandoff.PackRelativePath}; localizationProbe={resourceHandoff.LocalizationProbePath}. This synchronous boundary has no launcher retry in the same process.");
            var resolverCountBefore = context.ManagedResolverRequests.Count;
            var hostLoadCountBefore = context.HostLoads.Count;
            var privateLoadCountBefore = context.PrivateLoads.Count;
            var initializerBearingCountBefore = context.InitializerBearingRequests.Count;
            var rejectedManagedCountBefore = context.RejectedManagedRequests.Count;
            var nativeLoadCountBefore = context.NativeLoadAttempts.Count;
            try
            {
                binding.Method.Invoke(null, null);
            }
            catch (Exception ex)
            {
                var stateAfterFailure = TryReadOneTimeInitializationState(binding.StateField);
                var diagnostic = BuildEssentialInvocationFailureDiagnostic(
                    ex,
                    stateBefore,
                    stateAfterFailure,
                    binding.Method,
                    context,
                    resolverCountBefore,
                    hostLoadCountBefore,
                    privateLoadCountBefore,
                    initializerBearingCountBefore,
                    rejectedManagedCountBefore,
                    nativeLoadCountBefore);
                foreach (var line in diagnostic.CheckpointLines)
                    Checkpoint(checkpoint, line);
                throw new InvalidOperationException(diagnostic.ReportText, ex);
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
                $"Receipt-backed game resource pack mounted before invocation: {resourceHandoff.PackRelativePath}\n" +
                $"Localization probe before invocation: {resourceHandoff.LocalizationProbePath} => PRESENT\n" +
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
            Checkpoint(checkpoint, $"E_C_FAIL — stage={stage}; top={ex.GetType().FullName}; base={ex.GetBaseException().GetType().FullName}; message={SanitizeCheckpoint(ex.GetBaseException().Message)}");
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
            var resourceHandoff = RequireEssentialResourcePackHandoff();
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
            if (!Path.GetFullPath(managedRoot).Equals(Path.GetFullPath(resourceHandoff.ManagedInstallRoot), StringComparison.Ordinal))
                throw new InvalidDataException("Step 36.0 managed-install root drifted from the exact Step-35 closure baseline used for the game resource-pack handoff.");
            VerifyFileLength(resourceHandoff.PackAbsolutePath, resourceHandoff.PackLength, "Step-36 mounted game resource pack");
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
                $"Receipt-backed game resource pack remained present: {resourceHandoff.PackRelativePath} ({resourceHandoff.PackLength:N0} bytes)\n" +
                $"Localization resource probe used before ExecuteEssential: {resourceHandoff.LocalizationProbePath}\n" +
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

    private GameResourcePackSnapshot ResolveReceiptBackedGameResourcePack(Action<string>? checkpoint)
    {
        var baseline = _step36Baseline ?? throw new InvalidOperationException("Step 36.0 baseline is absent.");
        var managedRoot = Path.GetFullPath(baseline.ManagedInstallRoot);
        var receiptPath = ResolveChildPath(managedRoot, SteamManagedInstallReceipt.FileName, "Step-36 managed-install receipt");
        Checkpoint(checkpoint, $"E_B_PACK_RECEIPT_START — reading the already-OfflineReady Step-12 receipt to locate the exact game PCK; managedRoot={managedRoot}.");
        if (!File.Exists(receiptPath))
            throw new FileNotFoundException("Step 36.0 exact game resource-pack handoff requires the Step-12 managed-install receipt.", receiptPath);

        SteamManagedInstallReceipt? receipt;
        try
        {
            receipt = JsonSerializer.Deserialize(
                File.ReadAllBytes(receiptPath),
                SteamManagedInstallJsonContext.Default.SteamManagedInstallReceipt);
        }
        catch (JsonException ex)
        {
            throw new InvalidDataException("Step 36.0 could not deserialize the already-verified managed-install receipt while locating the game resource pack.", ex);
        }

        if (receipt is null || receipt.SchemaVersion != SteamManagedInstallReceipt.CurrentSchemaVersion || receipt.AppId != SteamManagedInstallAttempt.TargetAppId)
            throw new InvalidDataException("Step 36.0 managed-install receipt identity drifted after exact Step-35 closure.");
        var expectedDirectoryName = $"Depot-{receipt.DepotId}";
        if (!string.Equals(new DirectoryInfo(managedRoot).Name, expectedDirectoryName, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException($"Step 36.0 managed-install depot directory drifted: expected {expectedDirectoryName}, observed {new DirectoryInfo(managedRoot).Name}.");

        var packEntries = receipt.Files
            .Where(file => string.Equals(NormalizeRelative(file.RelativePath), NormalizeRelative(GameResourcePackRelativePath), StringComparison.OrdinalIgnoreCase))
            .ToArray();
        if (packEntries.Length != 1)
            throw new InvalidDataException($"Step 36.0 requires exactly one receipt entry for {GameResourcePackRelativePath}; found {packEntries.Length}.");
        var packEntry = packEntries[0];
        if (packEntry.Length <= 0 || packEntry.Sha1Hex.Length != 40 || !packEntry.Sha1Hex.All(Uri.IsHexDigit))
            throw new InvalidDataException("Step 36.0 game PCK receipt entry has an invalid length or SHA-1 shape.");
        var packPath = ResolveChildPath(managedRoot, packEntry.RelativePath, "Step-36 game resource pack");
        VerifyFileLength(packPath, packEntry.Length, "Step-36 receipt-backed game resource pack");
        Checkpoint(checkpoint, $"E_B_PACK_RECEIPT_PASS — exact receipt-backed game PCK located; relative={NormalizeRelative(packEntry.RelativePath)}; bytes={packEntry.Length}; receiptSha1={packEntry.Sha1Hex}; no second full-file hash is performed because exact Step-35 Gate D just re-proved OfflineReady 428/428.");
        return new GameResourcePackSnapshot(managedRoot, NormalizeRelative(packEntry.RelativePath), packPath, packEntry.Length, packEntry.Sha1Hex.ToLowerInvariant());
    }

    private EssentialResourcePackHandoffSnapshot MountExactGameResourcePackAndProbe(
        GameResourcePackSnapshot pack,
        Step35ExecutionLoadContext context,
        Action<string>? checkpoint)
    {
        if (_essentialResourcePackHandoff is not null)
            throw new InvalidOperationException("Step 36.0 game resource pack has already been mounted in this process; no same-process retry is permitted.");
        var handoff = _callbackHandoff ?? throw new InvalidOperationException("Step 36.0 game resource-pack handoff requires the exact prepared GodotSharp bridge from Step 35.");
        var godotAssembly = handoff.GodotSharpAssembly;
        if (!ReferenceEquals(AssemblyLoadContext.GetLoadContext(godotAssembly), context))
            throw new InvalidDataException("Step 36.0 exact GodotSharp assembly left the dedicated Step-35 context before resource-pack handoff.");

        Checkpoint(checkpoint, $"E_B_PACK_BIND_START — binding exact GodotSharp Godot.ProjectSettings.LoadResourcePack for the receipt-backed PCK; replaceFiles=false; offset=0.");
        var projectSettings = godotAssembly.GetType("Godot.ProjectSettings", throwOnError: true, ignoreCase: false)
            ?? throw new MissingMemberException("Godot.ProjectSettings");
        var loadResourcePack = projectSettings.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            .SingleOrDefault(candidate =>
            {
                if (!string.Equals(candidate.Name, "LoadResourcePack", StringComparison.Ordinal) || candidate.ReturnType != typeof(bool))
                    return false;
                var parameters = candidate.GetParameters();
                return parameters.Length == 3 &&
                       parameters[0].ParameterType == typeof(string) &&
                       parameters[1].ParameterType == typeof(bool) &&
                       (parameters[2].ParameterType == typeof(int) || parameters[2].ParameterType == typeof(long));
            })
            ?? throw new MissingMethodException("Godot.ProjectSettings", "LoadResourcePack(string,bool,int/long)");
        Checkpoint(checkpoint, $"E_B_PACK_BIND_PASS — exact GodotSharp LoadResourcePack bound; token=0x{loadResourcePack.MetadataToken:X8}; offsetType={loadResourcePack.GetParameters()[2].ParameterType.FullName}.");

        object offsetArgument = loadResourcePack.GetParameters()[2].ParameterType == typeof(long) ? (object)0L : 0;
        Checkpoint(checkpoint, $"E_B_PACK_LOAD_START — mounting receipt-backed game PCK into the live source-built Godot resource filesystem; path={pack.AbsolutePath}; replaceFiles=false; offset=0.");
        bool loadReturned;
        try
        {
            loadReturned = loadResourcePack.Invoke(null, new object?[] { pack.AbsolutePath, false, offsetArgument }) is true;
        }
        catch (TargetInvocationException ex) when (ex.InnerException is not null)
        {
            throw new InvalidOperationException($"Exact GodotSharp ProjectSettings.LoadResourcePack threw {ex.InnerException.GetType().FullName}: {ex.InnerException.Message}", ex.InnerException);
        }
        Checkpoint(checkpoint, $"E_B_PACK_LOAD_RETURNED — returned={loadReturned}; replaceFiles=false; path={pack.RelativePath}.");
        if (!loadReturned)
            throw new InvalidDataException("Godot.ProjectSettings.LoadResourcePack returned false for the exact receipt-backed Slay the Spire 2 PCK.");

        Checkpoint(checkpoint, $"E_B_LOCALIZATION_DIR_PROBE_START — probing the exact prior failure path {RequiredLocalizationProbePath} through Godot.DirAccess.Open after pack mount.");
        var dirAccess = godotAssembly.GetType("Godot.DirAccess", throwOnError: true, ignoreCase: false)
            ?? throw new MissingMemberException("Godot.DirAccess");
        var open = dirAccess.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            .SingleOrDefault(candidate =>
            {
                if (!string.Equals(candidate.Name, "Open", StringComparison.Ordinal))
                    return false;
                var parameters = candidate.GetParameters();
                return parameters.Length == 1 && parameters[0].ParameterType == typeof(string);
            })
            ?? throw new MissingMethodException("Godot.DirAccess", "Open(string)");
        object? opened = null;
        try
        {
            opened = open.Invoke(null, new object?[] { RequiredLocalizationProbePath });
            Checkpoint(checkpoint, $"E_B_LOCALIZATION_DIR_PROBE_RETURNED — exists={opened is not null}; path={RequiredLocalizationProbePath}.");
            if (opened is null)
                throw new InvalidDataException($"The receipt-backed game PCK mounted successfully, but Godot still cannot open {RequiredLocalizationProbePath}; refusing ExecuteEssential rather than bypassing localization.");
        }
        finally
        {
            if (opened is IDisposable disposable)
                disposable.Dispose();
        }

        Checkpoint(checkpoint, $"E_B_GAME_RESOURCE_PACK_PASS — exact receipt-backed game PCK is mounted additively in source-built Godot and the prior localization directory boundary is now visible; pack={pack.RelativePath}; localization={RequiredLocalizationProbePath}.");
        return new EssentialResourcePackHandoffSnapshot(
            pack.ManagedInstallRoot,
            pack.RelativePath,
            pack.AbsolutePath,
            pack.Length,
            pack.ReceiptSha1,
            RequiredLocalizationProbePath);
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

    private static Step36BaselineSnapshot CaptureStep36Baseline(Step35ExecutionLoadContext context, string managedInstallRoot)
        => new(
            context.ManagedResolverRequests.ToArray(),
            context.HostLoads.ToArray(),
            context.PrivateLoads.ToArray(),
            Path.GetFullPath(managedInstallRoot));

    private EssentialPreflightSnapshot RequireEssentialPreflight()
        => _essentialPreflight ?? throw new InvalidOperationException("Step 36.0 Gate A must pass before Gate B.");

    private EssentialBindingSnapshot RequireEssentialBinding()
        => _essentialBinding ?? throw new InvalidOperationException("Step 36.0 Gate B must pass before Gate C.");

    private EssentialResourcePackHandoffSnapshot RequireEssentialResourcePackHandoff()
        => _essentialResourcePackHandoff ?? throw new InvalidOperationException("Step 36.0 Gate B must mount and verify the exact game resource pack before Gate C.");

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


    internal static string FormatExceptionDiagnostic(Exception exception)
    {
        var lines = new List<string>();
        var current = exception;
        var depth = 0;
        while (current is not null)
        {
            lines.Add($"Exception depth {depth}: {current.GetType().FullName}");
            lines.Add($"  Message: {current.Message}");
            lines.Add($"  HResult: 0x{current.HResult:X8}");
            lines.Add($"  Source: {current.Source ?? "<null>"}");
            lines.Add($"  TargetSite: {FormatTargetSite(current.TargetSite)}");
            lines.Add("  StackTrace:");
            lines.Add(IndentMultiline(current.StackTrace ?? "<null>", "    "));
            if (current is ReflectionTypeLoadException typeLoad && typeLoad.LoaderExceptions is { Length: > 0 })
            {
                lines.Add($"  LoaderExceptions: {typeLoad.LoaderExceptions.Length}");
                for (var i = 0; i < typeLoad.LoaderExceptions.Length; i++)
                {
                    var loader = typeLoad.LoaderExceptions[i];
                    if (loader is null)
                    {
                        lines.Add($"    [{i}] <null>");
                        continue;
                    }
                    lines.Add($"    [{i}] {loader.GetType().FullName}: {loader.Message}");
                    lines.Add($"        HResult: 0x{loader.HResult:X8}");
                    lines.Add($"        Source: {loader.Source ?? "<null>"}");
                    lines.Add($"        TargetSite: {FormatTargetSite(loader.TargetSite)}");
                    lines.Add("        StackTrace:");
                    lines.Add(IndentMultiline(loader.StackTrace ?? "<null>", "          "));
                }
            }
            current = current.InnerException;
            depth++;
        }

        var baseException = exception.GetBaseException();
        lines.Add($"Base exception: {baseException.GetType().FullName}: {baseException.Message}");
        lines.Add($"Base HResult: 0x{baseException.HResult:X8}");
        lines.Add($"Base Source: {baseException.Source ?? "<null>"}");
        lines.Add($"Base TargetSite: {FormatTargetSite(baseException.TargetSite)}");
        lines.Add("Base StackTrace:");
        lines.Add(IndentMultiline(baseException.StackTrace ?? "<null>", "  "));
        return string.Join("\n", lines);
    }

    private static EssentialInvocationFailureDiagnostic BuildEssentialInvocationFailureDiagnostic(
        Exception exception,
        int stateBefore,
        int? stateAfterFailure,
        MethodInfo method,
        Step35ExecutionLoadContext context,
        int resolverCountBefore,
        int hostLoadCountBefore,
        int privateLoadCountBefore,
        int initializerBearingCountBefore,
        int rejectedManagedCountBefore,
        int nativeLoadCountBefore)
    {
        var resolverDelta = context.ManagedResolverRequests.Skip(resolverCountBefore).ToArray();
        var hostLoadDelta = context.HostLoads.Skip(hostLoadCountBefore).ToArray();
        var privateLoadDelta = context.PrivateLoads.Skip(privateLoadCountBefore).ToArray();
        var initializerDelta = context.InitializerBearingRequests.Skip(initializerBearingCountBefore).ToArray();
        var rejectedDelta = context.RejectedManagedRequests.Skip(rejectedManagedCountBefore).ToArray();
        var nativeDelta = context.NativeLoadAttempts.Skip(nativeLoadCountBefore).ToArray();
        var sts2Assembly = method.DeclaringType?.Assembly ?? method.Module.Assembly;
        var sts2ContextSame = ReferenceEquals(AssemblyLoadContext.GetLoadContext(sts2Assembly), context);
        var godotSharpAssemblies = context.Assemblies.Where(a => string.Equals(a.GetName().Name, "GodotSharp", StringComparison.Ordinal)).ToArray();
        var godotSharpContextSame = godotSharpAssemblies.Length == 1 && ReferenceEquals(AssemblyLoadContext.GetLoadContext(godotSharpAssemblies[0]), context);
        var exceptionText = FormatExceptionDiagnostic(exception);
        var stateText = stateAfterFailure?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "<unreadable>";

        var report =
            "Exact transformed ExecuteEssential threw. Full diagnostic follows.\n" +
            $"State before invocation: {stateBefore}\n" +
            $"State after failed invocation: {stateText}\n" +
            $"sts2 still in exact Step-35 private load context: {sts2ContextSame}\n" +
            $"GodotSharp assemblies in private context: {godotSharpAssemblies.Length}\n" +
            $"GodotSharp still in exact Step-35 private load context: {godotSharpContextSame}\n" +
            $"Managed resolver delta: {FormatDelta(resolverDelta)}\n" +
            $"Host-load delta: {FormatDelta(hostLoadDelta)}\n" +
            $"Private-load delta: {FormatDelta(privateLoadDelta)}\n" +
            $"Initializer-bearing request delta: {FormatDelta(initializerDelta)}\n" +
            $"Rejected managed request delta: {FormatDelta(rejectedDelta)}\n" +
            $"Native-load attempt delta: {FormatDelta(nativeDelta)}\n" +
            exceptionText;

        var checkpointLines = new List<string>
        {
            $"E_C_EXCEPTION_CAPTURED — top={exception.GetType().FullName}; base={exception.GetBaseException().GetType().FullName}; stateBefore={stateBefore}; stateAfterFailure={stateText}.",
            $"E_C_POST_FAILURE_CONTEXT — sts2ContextSame={sts2ContextSame}; godotSharpCount={godotSharpAssemblies.Length}; godotSharpContextSame={godotSharpContextSame}; resolverDelta={resolverDelta.Length}; hostLoadDelta={hostLoadDelta.Length}; privateLoadDelta={privateLoadDelta.Length}; initializerDelta={initializerDelta.Length}; rejectedDelta={rejectedDelta.Length}; nativeDelta={nativeDelta.Length}.",
        };
        var chain = exception;
        var depth = 0;
        while (chain is not null)
        {
            checkpointLines.Add($"E_C_EXCEPTION_DEPTH — depth={depth}; type={chain.GetType().FullName}; hresult=0x{chain.HResult:X8}; source={SanitizeCheckpoint(chain.Source)}; target={SanitizeCheckpoint(FormatTargetSite(chain.TargetSite))}; message={SanitizeCheckpoint(chain.Message)}; stack={SanitizeCheckpoint(chain.StackTrace)}");
            if (chain is ReflectionTypeLoadException typeLoad && typeLoad.LoaderExceptions is { Length: > 0 })
            {
                for (var i = 0; i < typeLoad.LoaderExceptions.Length; i++)
                {
                    var loader = typeLoad.LoaderExceptions[i];
                    checkpointLines.Add(loader is null
                        ? $"E_C_LOADER_EXCEPTION — depth={depth}; index={i}; <null>"
                        : $"E_C_LOADER_EXCEPTION — depth={depth}; index={i}; type={loader.GetType().FullName}; hresult=0x{loader.HResult:X8}; source={SanitizeCheckpoint(loader.Source)}; target={SanitizeCheckpoint(FormatTargetSite(loader.TargetSite))}; message={SanitizeCheckpoint(loader.Message)}; stack={SanitizeCheckpoint(loader.StackTrace)}");
                }
            }
            chain = chain.InnerException;
            depth++;
        }
        var baseException = exception.GetBaseException();
        checkpointLines.Add($"E_C_BASE_EXCEPTION — type={baseException.GetType().FullName}; hresult=0x{baseException.HResult:X8}; source={SanitizeCheckpoint(baseException.Source)}; target={SanitizeCheckpoint(FormatTargetSite(baseException.TargetSite))}; message={SanitizeCheckpoint(baseException.Message)}; stack={SanitizeCheckpoint(baseException.StackTrace)}");
        return new EssentialInvocationFailureDiagnostic(report, checkpointLines.ToArray());
    }

    private static int? TryReadOneTimeInitializationState(FieldInfo stateField)
    {
        try
        {
            return ReadOneTimeInitializationState(stateField);
        }
        catch
        {
            return null;
        }
    }

    private static string FormatTargetSite(MethodBase? targetSite)
        => targetSite is null
            ? "<null>"
            : $"{targetSite.DeclaringType?.FullName ?? "<global>"}::{targetSite}";

    private static string IndentMultiline(string value, string indent)
        => string.Join("\n", value.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n').Split('\n').Select(line => indent + line));

    private static string SanitizeCheckpoint(string? value)
        => (value ?? "<null>")
            .Replace("\r", "\\r", StringComparison.Ordinal)
            .Replace("\n", "\\n", StringComparison.Ordinal)
            .Replace("|", "\\|", StringComparison.Ordinal);

    private static string FormatDelta(IReadOnlyList<string> values)
        => values.Count == 0 ? "0" : $"{values.Count}: " + string.Join(" | ", values);

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
        string[] PrivateLoads,
        string ManagedInstallRoot);

    private sealed record GameResourcePackSnapshot(
        string ManagedInstallRoot,
        string RelativePath,
        string AbsolutePath,
        long Length,
        string ReceiptSha1);

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

    private sealed record EssentialResourcePackHandoffSnapshot(
        string ManagedInstallRoot,
        string PackRelativePath,
        string PackAbsolutePath,
        long PackLength,
        string ReceiptSha1,
        string LocalizationProbePath);

    private sealed record EssentialInvocationFailureDiagnostic(
        string ReportText,
        string[] CheckpointLines);

    private sealed record EssentialExecutionSnapshot(
        int MethodToken,
        int StateBefore,
        int StateAfter,
        string[] ManagedResolverRequests,
        string[] HostLoads,
        string[] PrivateLoads,
        string[] PrivateContextAssemblies);
}
