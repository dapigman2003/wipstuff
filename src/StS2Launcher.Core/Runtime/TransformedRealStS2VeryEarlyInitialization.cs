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
    private const string DiagnosticCloneFileName = "sts2.step35.0.15.instrumented.dll";
    private const string GodotSharpDiagnosticCloneFileName = "GodotSharp.step35.0.15.instrumented.dll";
    internal const string GodotSharpDiagnosticBridgeTypeFullName = "StS2Launcher.Step35Diagnostics.GodotSharpCheckpointBridge";
    internal const string GodotSharpDiagnosticBridgeCallbackFieldName = "Callback";
    internal const string NullPlatformTypeFullName = "MegaCrit.Sts2.Core.Platform.Null.NullPlatformUtilStrategy";
    internal const string NullPlatformConstructorFullName = "System.Void MegaCrit.Sts2.Core.Platform.Null.NullPlatformUtilStrategy::.ctor()";
    internal const string CommandLineHelperTypeFullName = "MegaCrit.Sts2.Core.Helpers.CommandLineHelper";
    internal const string CommandLineHelperTryGetValueFullName = "System.Boolean MegaCrit.Sts2.Core.Helpers.CommandLineHelper::TryGetValue(System.String,System.String&)";
    internal const string ManagedDictionaryOpenFullName = "System.Collections.Generic.Dictionary`2";
    internal const string ManagedStringDictionaryFullName = "System.Collections.Generic.Dictionary`2<System.String,System.String>";
    internal const string CommandLineCctorDictionaryBeforeMarker = "INMETHOD_CL_CRITICAL_001_PRE — CommandLineHelper..cctor before _args dictionary construction";
    internal const string CommandLineCctorDictionaryAfterMarker = "INMETHOD_CL_CRITICAL_001_POST — CommandLineHelper..cctor after _args dictionary assignment";
    internal const string CommandLineCctorGetCmdlineArgsBeforeMarker = "INMETHOD_CL_CRITICAL_002_PRE — CommandLineHelper..cctor before Godot.OS.GetCmdlineArgs";
    internal const string CommandLineCctorGetCmdlineArgsAfterMarker = "INMETHOD_CL_CRITICAL_002_POST — CommandLineHelper..cctor after Godot.OS.GetCmdlineArgs result stored";
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

    public Step35DiagnosticMode DiagnosticMode { get; set; } = Step35DiagnosticMode.ManagedDictionaryCompatibility;

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

                var transformedNullPlatformType = EnumerateTypes(transformedModule.Types).SingleOrDefault(type => type.FullName == NullPlatformTypeFullName)
                    ?? throw new MissingMemberException($"Step-35.0.15 static-map target type missing: {NullPlatformTypeFullName}.");
                var transformedNullPlatformConstructor = transformedNullPlatformType.Methods.SingleOrDefault(method => method.FullName == NullPlatformConstructorFullName && method.HasBody)
                    ?? throw new MissingMethodException($"Step-35.0.15 static-map target constructor missing: {NullPlatformConstructorFullName}.");
                var transformedCommandLineHelperType = EnumerateTypes(transformedModule.Types).SingleOrDefault(type => type.FullName == CommandLineHelperTypeFullName)
                    ?? throw new MissingMemberException($"Step-35.0.15 static-map target type missing: {CommandLineHelperTypeFullName}.");
                var transformedCommandLineHelperCctor = transformedCommandLineHelperType.Methods.SingleOrDefault(method => method.Name == ".cctor" && method.IsStatic && method.HasBody)
                    ?? throw new MissingMethodException($"Step-35.0.15 static-map target cctor missing: {CommandLineHelperTypeFullName}..cctor.");
                var transformedCommandLineHelperTryGetValue = transformedCommandLineHelperType.Methods.SingleOrDefault(method => method.FullName == CommandLineHelperTryGetValueFullName && method.HasBody)
                    ?? throw new MissingMethodException($"Step-35.0.15 static-map target method missing: {CommandLineHelperTryGetValueFullName}.");
                veryEarlyStaticInstructionMap = BuildStaticInstructionMap(
                    transformedMethod,
                    transformedMoveNext,
                    transformedNullPlatformConstructor,
                    transformedCommandLineHelperCctor,
                    transformedCommandLineHelperTryGetValue);
                transformedMethodToken = transformedMethod.MetadataToken.ToUInt32();
                transformedMoveNextToken = transformedMoveNext.MetadataToken.ToUInt32();
                if (sourceResolver.Requests.Count != 0 || transformedResolver.Requests.Count != 0)
                    throw new InvalidDataException("Step-35 source/transformed very-early metadata inspection unexpectedly resolved a dependency through Cecil.");
            }
            progress?.Report(new(gate, 5, 8, transformedPath,
                "Exact source/transformed ExecuteVeryEarly wrapper + async MoveNext semantics requalified; no direct ExecuteEssential/ExecuteDeferred/PrewarmJit or Harmony call crosses this boundary."));

            stage = "Step-35.0.15 diagnostic-clone instrumentation";
            var diagnosticRoot = Path.Combine(_launcherDataRoot, "Step35-ExecuteVeryEarlyDiagnostic");
            Directory.CreateDirectory(diagnosticRoot);
            var diagnosticPath = Path.Combine(diagnosticRoot, DiagnosticCloneFileName);
            var diagnosticMode = DiagnosticMode;
            var diagnostic = CreateInstrumentedDiagnosticClone(transformedPath, diagnosticPath, diagnosticMode);
            VerifyFileLength(transformedPath, TransformedRealStS2AssemblyAdmission.ClosedStep32TransformedBytes, "exact transformed primary after diagnostic-clone emission");
            var transformedSha256AfterDiagnosticEmission = ComputeSha256Hex(transformedPath);
            if (!transformedSha256AfterDiagnosticEmission.Equals(transformedSha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Step-35.0.15 diagnostic-clone emission changed the exact closed transformed source; refusing to continue.");
            progress?.Report(new(gate, 6, 8, diagnosticPath,
                $"Exact transformed image requalified, then a Step-35.0.15 diagnostic-only clone was emitted for mode {diagnosticMode} with {diagnostic.MarkerCount:N0} durable in-method markers, critical stack-neutral CommandLine boundaries, {diagnostic.CommandLineManagedDictionarySubstitutionCount:N0} managed Dictionary<string,string> compatibility substitution(s), and unchanged serialized cctor MaxStack. Cecil serialization used {diagnostic.WriteResolutionRequestCount:N0} bounded writer-only constant-metadata resolution request(s) across {diagnostic.ApprovedConstantScopeCount:N0} audited scope(s), then the clone reopened under rejecting resolution; the exact transformed source was immediately re-hashed unchanged."));

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

            stage = "GodotSharp diagnostic clone + installed-bundle native reconnaissance";
            var preparedGodotSharp = prepared.SingleOrDefault(item => !item.Plan.IsPrimary && string.Equals(item.AssemblyName.Name, "GodotSharp", StringComparison.OrdinalIgnoreCase))
                ?? throw new InvalidDataException("Step-35.0.15 comprehensive reconnaissance requires the exact prepared GodotSharp private dependency.");
            if (preparedGodotSharp.ModuleInitializerCount != 0)
                throw new InvalidDataException("Step-35.0.15 refuses to create a runtime diagnostic derivative from initializer-bearing GodotSharp metadata.");
            var godotSharpDiagnosticPath = Path.Combine(diagnosticRoot, GodotSharpDiagnosticCloneFileName);
            var godotSharpDiagnostic = CreateInstrumentedGodotSharpDiagnosticClone(preparedGodotSharp.PreparedPath, godotSharpDiagnosticPath);
            if (!godotSharpDiagnostic.AssemblyIdentity.Equals(preparedGodotSharp.Plan.AssemblyFullName, StringComparison.Ordinal))
                throw new InvalidDataException($"Step-35.0.15 GodotSharp diagnostic identity drifted from prepared plan: {godotSharpDiagnostic.AssemblyIdentity} != {preparedGodotSharp.Plan.AssemblyFullName}.");
            VerifyFileLength(preparedGodotSharp.PreparedPath, preparedGodotSharp.Plan.Length, "prepared GodotSharp after diagnostic-clone emission");
            var godotSourceSha1AfterDiagnostic = ComputeSha1Hex(preparedGodotSharp.PreparedPath);
            if (!godotSourceSha1AfterDiagnostic.Equals(preparedGodotSharp.Plan.Sha1Hex, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Step-35.0.15 GodotSharp diagnostic-clone emission changed the exact prepared source; refusing to continue.");

            var offlineForRecon = await _offlineInspection.RunAsync(progress: null, cancellationToken).ConfigureAwait(false);
            if (!offlineForRecon.Success || !offlineForRecon.ExactManagedTreeVerified || string.IsNullOrWhiteSpace(offlineForRecon.ManagedInstallRelativePath))
                throw new InvalidDataException("Step-35.0.15 native reconnaissance requires the exact Step-13 OfflineReady managed tree.");
            var managedInstallRoot = ResolveChildPath(_launcherDataRoot, NormalizeRelative(offlineForRecon.ManagedInstallRelativePath), "Step-35 managed-install reconnaissance root");
            var godotReconnaissanceReport = Step35GodotReconnaissance.BuildReport(managedInstallRoot, preparedGodotSharp.PreparedPath);

            progress?.Report(new(gate, 7, 8, godotSharpDiagnosticPath,
                $"Prepared runtime-binding plan requalified; emitted a separately hash-pinned GodotSharp diagnostic clone with {godotSharpDiagnostic.MarkerCount:N0} entry-only markers, and completed read-only Mach-O/native + GodotSharp IL reconnaissance over the exact OfflineReady tree. No prepared/native image was executed."));

            _preflight = new ExecutionPreflightSnapshot(
                transformedPath,
                transformedSha256,
                diagnostic.Path,
                diagnostic.Sha256,
                diagnostic.Length,
                diagnostic.MethodToken,
                diagnostic.MoveNextToken,
                diagnostic.MarkerCount,
                diagnosticMode,
                godotSharpDiagnostic,
                godotReconnaissanceReport,
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
                $"Step-35.0.15 diagnostic clone SHA-256: {diagnostic.Sha256}\n" +
                $"Step-35.0.15 diagnostic clone bytes: {diagnostic.Length:N0}\n" +
                $"Injected durable sts2 checkpoint markers: {diagnostic.MarkerCount:N0}\n" +
                $"Diagnostic mode: {diagnosticMode}\n" +
                $"GodotSharp diagnostic clone SHA-256: {godotSharpDiagnostic.Sha256}\n" +
                $"GodotSharp diagnostic clone bytes: {godotSharpDiagnostic.Length:N0}\n" +
                $"GodotSharp entry-only checkpoint markers: {godotSharpDiagnostic.MarkerCount:N0}\n" +
                $"GodotSharp MVID preserved: {godotSharpDiagnostic.Mvid}\n" +
                $"GodotSharp writer constant-requirement fingerprint SHA-256: {godotSharpDiagnostic.ConstantRequirementFingerprintSha256}\n" +
                $"GodotSharp writer-only resolution requests / scopes / requirements: {godotSharpDiagnostic.WriteResolutionRequestCount:N0} / {godotSharpDiagnostic.ApprovedConstantScopeCount:N0} / {godotSharpDiagnostic.ApprovedConstantRequirementCount:N0}\n" +
                "Read-only installed-bundle Mach-O/native reconnaissance: COMPLETE\n" +
                $"CommandLineHelper cctor MaxStack exact-source/diagnostic: {diagnostic.CommandLineCctorOriginalMaxStack} / {diagnostic.CommandLineCctorDiagnosticMaxStack}\n" +
                "CommandLineHelper critical stack-neutral markers: 4\n" +
                $"CommandLineHelper managed dictionary compatibility substitutions: {diagnostic.CommandLineManagedDictionarySubstitutionCount:N0}\n" +
                "CommandLineHelper Godot.OS.GetCmdlineArgs compatibility substitution: NO (natural call intentionally retained)\n" +
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

    public string GetVerifiedGodotReconnaissanceReport()
    {
        ThrowIfDisposed();
        return RequirePreflight().GodotReconnaissanceReport;
    }

    public string GetVerifiedGodotSharpDiagnosticMarkerMap()
    {
        ThrowIfDisposed();
        return RequirePreflight().GodotSharpDiagnostic.MarkerMap;
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
            Checkpoint(crashCheckpoint, "B_HASH_START — rechecking the exact closed transformed primary and the Step-35.0.15 instrumented diagnostic clone before CLR admission.");
            VerifyFileLength(preflight.TransformedPath, TransformedRealStS2AssemblyAdmission.ClosedStep32TransformedBytes, "exact transformed primary");
            var exactImmediateSha256 = ComputeSha256Hex(preflight.TransformedPath);
            if (!exactImmediateSha256.Equals(TransformedRealStS2AssemblyAdmission.ClosedStep32TransformedSha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Step-35 exact transformed image changed between Gate A verification and Gate B CLR admission.");
            VerifyFileLength(preflight.DiagnosticPath, preflight.DiagnosticLength, "Step-35.0.15 instrumented diagnostic clone");
            var immediateSha256 = ComputeSha256Hex(preflight.DiagnosticPath);
            if (!immediateSha256.Equals(preflight.DiagnosticSha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Step-35.0.15 diagnostic clone changed between Gate A instrumentation and Gate B CLR admission.");
            VerifyFileLength(preflight.GodotSharpDiagnostic.Path, preflight.GodotSharpDiagnostic.Length, "Step-35.0.15 GodotSharp diagnostic clone");
            var immediateGodotSha256 = ComputeSha256Hex(preflight.GodotSharpDiagnostic.Path);
            if (!immediateGodotSha256.Equals(preflight.GodotSharpDiagnostic.Sha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Step-35.0.15 GodotSharp diagnostic clone changed between Gate A instrumentation and Gate B admission preparation.");
            Checkpoint(crashCheckpoint, $"B_HASH_PASS — exact transformed source still matched {exactImmediateSha256}; sts2 diagnostic clone matched {immediateSha256}; GodotSharp diagnostic derivative matched {immediateGodotSha256}.");

            stage = "execution-capable strict AssemblyLoadContext construction";
            Checkpoint(crashCheckpoint, "B_ALC_CONSTRUCT_START — constructing strict Step-35 execution AssemblyLoadContext.");
            var godotOverride = new PrivateDiagnosticOverride(
                "GodotSharp",
                preflight.GodotSharpDiagnostic.Path,
                preflight.GodotSharpDiagnostic.Sha256,
                preflight.GodotSharpDiagnostic.Length,
                preflight.GodotSharpDiagnostic.AssemblyIdentity,
                preflight.GodotSharpDiagnostic.Mvid,
                GodotSharpDiagnosticBridgeTypeFullName,
                GodotSharpDiagnosticBridgeCallbackFieldName,
                preflight.GodotSharpDiagnostic.MarkerCount);
            var context = new Step35ExecutionLoadContext(
                LoadContextName,
                preflight.Plan,
                preflight.PreparedAssemblies,
                _collectibleLoadContext,
                crashCheckpoint,
                [godotOverride]);
            _loadContext = context;
            Checkpoint(crashCheckpoint, "B_ALC_CONSTRUCT_PASS — strict Step-35 execution AssemblyLoadContext constructed.");

            stage = "instrumented diagnostic sts2.dll LoadFromStream";
            Checkpoint(crashCheckpoint, "B_LOADPRIMARY_START — entering Step-35.0.15 instrumented diagnostic-clone LoadPrimary/LoadFromStream path; exact closed transformed source remains untouched on disk.");
            var assembly = context.LoadPrimary(preflight.DiagnosticPath, immediateSha256);
            Checkpoint(crashCheckpoint, "B_LOADPRIMARY_PASS — instrumented diagnostic clone returned from LoadPrimary/LoadFromStream.");
            if (!ReferenceEquals(AssemblyLoadContext.GetLoadContext(assembly), context))
                throw new InvalidDataException("The Step-35.0.15 diagnostic sts2.dll clone did not load into the dedicated Step-35 AssemblyLoadContext.");
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
                    "Step-35.0.15 diagnostic-clone admission no longer matches the physically closed Step-33 zero-resolution admission behavior. " +
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
                "STEP-33 ZERO-RESOLUTION ADMISSION BEHAVIOR RE-ESTABLISHED FOR THE STEP-35.0.15 INSTRUMENTED DIAGNOSTIC CLONE; NO GAME MEMBER REFLECTION/INVOCATION YET.\n" +
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
                throw new InvalidOperationException("Step-35.0.15 diagnostic Gate C requires a durable launcher-owned checkpoint callback; refusing to execute an instrumented clone without in-method telemetry.");
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
                throw new InvalidDataException("Step-35.0.15 target type did not bind from the admitted diagnostic sts2 clone.");
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
                throw new InvalidDataException("Step-35.0.15 reflected diagnostic ExecuteVeryEarly identity/signature drifted from the exact static parameterless System.Threading.Tasks.Task target.");
            Checkpoint(crashCheckpoint, "C_SIGNATURE_PASS — reflected instrumented method retains the exact static parameterless Task-returning target contract.");
            if (method.MetadataToken != unchecked((int)preflight.DiagnosticMethodToken))
                throw new InvalidDataException($"Step-35.0.15 reflected instrumented ExecuteVeryEarly token drifted: 0x{method.MetadataToken:X8} != preflight diagnostic 0x{preflight.DiagnosticMethodToken:X8}.");
            Checkpoint(crashCheckpoint, $"C_TOKEN_PASS — reflected ExecuteVeryEarly token matched 0x{method.MetadataToken:X8}.");
            if (method.Module.ModuleVersionId != TransformedRealStS2AssemblyAdmission.ClosedStep32Mvid)
                throw new InvalidDataException("Step-35 reflected ExecuteVeryEarly module MVID drifted from the closed transformed image.");
            Checkpoint(crashCheckpoint, $"C_MVID_PASS — reflected diagnostic-clone ExecuteVeryEarly module MVID matched {method.Module.ModuleVersionId}.");

            stage = "Step-35.0.15 in-method checkpoint bridge arm";
            var bridgeType = admission.Assembly.GetType(DiagnosticBridgeTypeFullName, throwOnError: true, ignoreCase: false)
                ?? throw new MissingMemberException(DiagnosticBridgeTypeFullName);
            var bridgeField = bridgeType.GetField(DiagnosticBridgeCallbackFieldName, BindingFlags.Static | BindingFlags.Public)
                ?? throw new MissingFieldException(DiagnosticBridgeTypeFullName, DiagnosticBridgeCallbackFieldName);
            if (bridgeField.FieldType != typeof(Action<string>))
                throw new InvalidDataException($"Step-35.0.15 diagnostic bridge field type drifted: {bridgeField.FieldType.FullName}.");
            bridgeField.SetValue(null, crashCheckpoint);
            Checkpoint(crashCheckpoint, $"C_DIAGNOSTIC_BRIDGE_ARMED — instrumented diagnostic clone callback armed; markerCount={preflight.DiagnosticMarkerCount}. The next durable INMETHOD_* record is emitted from inside the executing sts2.dll method body.");

            stage = "single instrumented diagnostic ExecuteVeryEarly invocation";
            Task task;
            try
            {
                Checkpoint(crashCheckpoint, "C_INVOKE_START — entering the first and only MethodInfo.Invoke(null, null) for the Step-35.0.15 instrumented ExecuteVeryEarly diagnostic clone.");
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
                    "Step-35.0.15 instrumented ExecuteVeryEarly threw synchronously during the first controlled invocation. " +
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
                    "Step-35.0.15 diagnostic-clone ExecuteVeryEarly Task faulted during the controlled await. " +
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
                "STEP-35.0.15 DIAGNOSTIC-CLONE EXECUTEVERYEARLY INVOCATION/AWAIT COMPLETED NORMALLY; THIS IS LOCALIZATION EVIDENCE, NOT EXACT STEP-35 CLOSURE.\n" +
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
                throw new InvalidDataException("Step-35.0.15 instrumented diagnostic clone changed during ExecuteVeryEarly execution.");
            var godotDiagnosticSha256 = ComputeSha256Hex(preflight.GodotSharpDiagnostic.Path);
            if (!godotDiagnosticSha256.Equals(preflight.GodotSharpDiagnostic.Sha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Step-35.0.15 GodotSharp diagnostic clone changed during ExecuteVeryEarly execution.");
            progress?.Report(new(gate, 2, 4, preflight.DiagnosticPath, "Exact transformed source plus sts2/GodotSharp diagnostic derivatives remain byte-identical to their Gate-A hashes."));
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
                throw new InvalidDataException("Step-35.0.15 diagnostic-clone CLR residency/context ownership drifted during final audit.");
            if (execution.MethodToken != unchecked((int)preflight.DiagnosticMethodToken))
                throw new InvalidDataException("Step-35.0.15 execution snapshot diagnostic ExecuteVeryEarly token drifted during final audit.");

            progress?.Report(new(gate, 4, 4, preflight.DiagnosticPath, "Final source/diagnostic-clone/plan/dependency/context isolation checks passed."));

            return Pass(gate,
                "STEP-35.0.15 DIAGNOSTIC-CLONE FINAL ISOLATION AUDIT PASSED; THIS DOES NOT CLOSE EXACT STEP 35.\n" +
                $"Post-execution OfflineReady: PASS ({offline.VerifiedFiles:N0}/{offline.PlannedFiles:N0} files)\n" +
                $"Receipt-backed original SHA-256 unchanged: {trustedSha256}\n" +
                $"Verified exact transformed SHA-256 unchanged: {transformedSha256}\n" +
                $"Instrumented sts2 diagnostic clone SHA-256 unchanged: {diagnosticSha256}\n" +
                $"Instrumented GodotSharp diagnostic clone SHA-256 unchanged: {godotDiagnosticSha256}\n" +
                $"Diagnostic mode: {preflight.DiagnosticMode}\n" +
                $"Runtime-binding plan SHA-256 unchanged: {planSha256}\n" +
                $"Unique resident sts2 identity: {admission.AssemblyFullName}\n" +
                $"Resident sts2 AssemblyLoadContext: {context.Name ?? LoadContextName}\n" +
                "Resident sts2 load input: Step-35.0.15 instrumented diagnostic clone derived from the reverified exact Step-32 transformed image\n" +
                $"Initializer-free prepared private dependencies resident and re-hashed: {verifiedPrivate:N0}\n" +
                $"Managed resolver requests total: {context.ManagedResolverRequests.Count:N0}\n" +
                $"Exact planned host-framework loads total: {context.HostLoads.Count:N0}\n" +
                $"Prepared/diagnostic private dependency loads total: {context.PrivateLoads.Count:N0}\n" +
                $"GodotSharp entry-only marker plan size: {preflight.GodotSharpDiagnostic.MarkerCount:N0}\n" +
                "Initializer-bearing private dependency requests: 0\n" +
                "Unplanned managed resolution: NO\n" +
                "Native game resolution/loading: NO\n" +
                "Instrumented diagnostic ExecuteVeryEarly invocation count: 1\n" +
                "Receipt-backed/prepared original sts2.dll CLR-loaded: NO\n" +
                "Game entry point / ExecuteEssential / ExecuteDeferred intentionally invoked by launcher: NO\n" +
                "Harmony/MonoMod runtime patching intentionally invoked by launcher: NO\n" +
                "Godot/game startup intentionally requested by launcher: NO\n" +
                "After a 0.0.138 diagnostic 4/4 result, Step 35 remains OPEN. Use the localization evidence to design a separately defined compatibility candidate, then return to an explicitly authoritative transformed artifact for physical closure testing.");
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

    internal static string BuildStaticInstructionMap(
        MethodDefinition wrapper,
        MethodDefinition moveNext,
        MethodDefinition? nullPlatformConstructor = null,
        MethodDefinition? commandLineHelperCctor = null,
        MethodDefinition? commandLineHelperTryGetValue = null)
    {
        if (!wrapper.HasBody || !moveNext.HasBody ||
            (nullPlatformConstructor is not null && !nullPlatformConstructor.HasBody) ||
            (commandLineHelperCctor is not null && !commandLineHelperCctor.HasBody) ||
            (commandLineHelperTryGetValue is not null && !commandLineHelperTryGetValue.HasBody))
        {
            throw new InvalidDataException("Step-35 static instruction map requires managed IL for wrapper, MoveNext, and every requested narrow-path method.");
        }

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
        if (nullPlatformConstructor is not null)
        {
            lines.Add(string.Empty);
            lines.Add("[NULL PLATFORM CTOR IL]");
            lines.Add($"NullPlatform constructor: token=0x{nullPlatformConstructor.MetadataToken.ToUInt32():X8}; {nullPlatformConstructor.FullName}");
            lines.Add("Step 35.0.15 dynamic constructor markers use the exact-source CALLSITE ordinals below; the direct base-constructor call is intentionally not wrapped.");
            AppendInstructionMap(lines, nullPlatformConstructor);
        }
        if (commandLineHelperCctor is not null)
        {
            lines.Add(string.Empty);
            lines.Add("[COMMAND LINE HELPER CCTOR IL]");
            lines.Add($"CommandLineHelper cctor: token=0x{commandLineHelperCctor.MetadataToken.ToUInt32():X8}; {commandLineHelperCctor.FullName}");
            lines.Add($"CommandLineHelper cctor exact-source MaxStack={commandLineHelperCctor.Body.MaxStackSize}; instructions={commandLineHelperCctor.Body.Instructions.Count}; locals={commandLineHelperCctor.Body.Variables.Count}; handlers={commandLineHelperCctor.Body.ExceptionHandlers.Count}");
            lines.Add("Step 35.0.15 retains this exact-source CALLSITE map for correlation. Runtime mode is chosen separately: NATURAL preserves this Godot dictionary contract for deep GodotSharp entry-marker localization; COMPAT rewrites only the field/.ctor/set_Item/TryGetValue contract to System.Collections.Generic.Dictionary<string,string>. Both modes retain four stack-neutral critical markers and leave Godot.OS.GetCmdlineArgs natural.");
            AppendInstructionMap(lines, commandLineHelperCctor);
        }
        if (commandLineHelperTryGetValue is not null)
        {
            lines.Add(string.Empty);
            lines.Add("[COMMAND LINE HELPER TRYGETVALUE IL]");
            lines.Add($"CommandLineHelper TryGetValue: token=0x{commandLineHelperTryGetValue.MetadataToken.ToUInt32():X8}; {commandLineHelperTryGetValue.FullName}");
            lines.Add("Step 35.0.15 retains this exact-source CALLSITE map for correlation and emits no CLTV sweep markers. NATURAL preserves the Godot dictionary TryGetValue MemberRef; COMPAT rewrites only that reference to the BCL Dictionary<string,string> equivalent. INMETHOD_027 proves method entry and outer NP002_POST proves return.");
            AppendInstructionMap(lines, commandLineHelperTryGetValue);
        }
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
        (NullPlatformTypeFullName, NullPlatformConstructorFullName, "INMETHOD_024 — NullPlatformUtilStrategy..ctor entered"),
        (CommandLineHelperTypeFullName, CommandLineHelperTryGetValueFullName, "INMETHOD_027 — CommandLineHelper.TryGetValue entered"),
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

    private static DiagnosticCloneSnapshot CreateInstrumentedDiagnosticClone(string exactTransformedPath, string diagnosticPath, Step35DiagnosticMode diagnosticMode)
    {
        if (File.Exists(diagnosticPath))
            File.Delete(diagnosticPath);

        string expectedConstantMetadataSha256;
        int writeResolutionRequestCount;
        int syntheticConstantTypeCount;
        int approvedConstantScopeCount;
        int approvedConstantRequirementCount;
        string writeResolutionIdentities;
        IReadOnlyList<DiagnosticCallsiteSweepEntry> nullPlatformCallsitePlan = Array.Empty<DiagnosticCallsiteSweepEntry>();
        IReadOnlyList<DiagnosticCallsiteSweepEntry> commandLineCctorCallsitePlan = Array.Empty<DiagnosticCallsiteSweepEntry>();
        IReadOnlyList<DiagnosticCallsiteSweepEntry> commandLineTryGetValueCallsitePlan = Array.Empty<DiagnosticCallsiteSweepEntry>();
        int commandLineCctorOriginalMaxStack = 0;
        int commandLineCctorDiagnosticMaxStack = 0;
        int commandLineManagedDictionarySubstitutionCount = 0;

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
                throw new InvalidDataException("Step-35.0.15 diagnostic deferred-open unexpectedly resolved a dependency before the bounded writer resolver was configured.");

            var constantPlan = resolver.Configure(module);
            expectedConstantMetadataSha256 = RealStS2PrepareMethodRewrite.ComputeConstantMetadataFingerprint(module);
            syntheticConstantTypeCount = constantPlan.SyntheticTypeCount;
            approvedConstantScopeCount = constantPlan.ApprovedScopeCount;
            approvedConstantRequirementCount = constantPlan.ApprovedRequirementCount;

            if (EnumerateTypes(module.Types).Any(type => type.FullName == DiagnosticBridgeTypeFullName))
                throw new InvalidDataException("Step-35.0.15 diagnostic bridge type already exists in the exact transformed image.");

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
                ?? throw new InvalidDataException("Step-35.0.15 diagnostic clone requires the existing System.Runtime metadata scope.");
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
                    ?? throw new MissingMemberException($"Step-35.0.15 diagnostic marker target type missing: {item.TypeName}.");
                var methods = type.Methods.Where(method => method.FullName == item.MethodFullName && method.HasBody).ToArray();
                if (methods.Length != 1)
                    throw new MissingMethodException($"Step-35.0.15 expected exactly one managed-IL marker target {item.MethodFullName}, found {methods.Length}.");
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
                    ?? throw new MissingMemberException($"Step-35.0.15 diagnostic callsite target type missing: {item.TypeName}.");
                var method = type.Methods.SingleOrDefault(candidate => candidate.FullName == item.MethodFullName && candidate.HasBody)
                    ?? throw new MissingMethodException($"Step-35.0.15 diagnostic callsite target method missing: {item.MethodFullName}.");
                InsertCallsiteMarkers(method, emitReference, item.CalleeFullName, item.BeforeMarker, item.AfterMarker);
                markerCount += 2;
            }

            var nullPlatformType = EnumerateTypes(module.Types).SingleOrDefault(candidate => candidate.FullName == NullPlatformTypeFullName)
                ?? throw new MissingMemberException($"Step-35.0.15 NullPlatform callsite-sweep type missing: {NullPlatformTypeFullName}.");
            var nullPlatformConstructor = nullPlatformType.Methods.SingleOrDefault(candidate => candidate.FullName == NullPlatformConstructorFullName && candidate.HasBody)
                ?? throw new MissingMethodException($"Step-35.0.15 NullPlatform callsite-sweep constructor missing: {NullPlatformConstructorFullName}.");
            nullPlatformCallsitePlan = InsertNullPlatformConstructorCallsiteMarkers(nullPlatformConstructor, emitReference);
            markerCount += checked(nullPlatformCallsitePlan.Count * 2);

            var commandLineType = EnumerateTypes(module.Types).SingleOrDefault(candidate => candidate.FullName == CommandLineHelperTypeFullName)
                ?? throw new MissingMemberException($"Step-35.0.15 CommandLineHelper sweep type missing: {CommandLineHelperTypeFullName}.");
            var commandLineCctor = commandLineType.Methods.SingleOrDefault(candidate => candidate.Name == ".cctor" && candidate.IsStatic && candidate.HasBody)
                ?? throw new MissingMethodException($"Step-35.0.15 CommandLineHelper cctor missing: {CommandLineHelperTypeFullName}..cctor.");
            var commandLineTryGetValue = commandLineType.Methods.SingleOrDefault(candidate => candidate.FullName == CommandLineHelperTryGetValueFullName && candidate.HasBody)
                ?? throw new MissingMethodException($"Step-35.0.15 CommandLineHelper method missing: {CommandLineHelperTryGetValueFullName}.");

            // Physical 0.0.133 and 0.0.135 proved that live-stack CL/CLTV callbacks can invalidate
            // CommandLineHelper..cctor before instruction zero, so they stay retired. Physical 0.0.136
            // then entered the stack-neutral cctor and hard-terminated after CL_CRITICAL_001_PRE but before
            // the matching POST, localizing the physical interval to Godot.Collections.Dictionary<string,string>
            // construction before _args assignment. Step 35.0.15 keeps the exact-source map and markers but
            // rewrites only that private container contract to System.Collections.Generic.Dictionary<string,string>.
            // Godot.OS.GetCmdlineArgs remains natural so the next physical boundary is not silently bypassed.
            commandLineCctorOriginalMaxStack = commandLineCctor.Body.MaxStackSize;
            InsertCommandLineHelperCriticalBoundaryMarkers(commandLineCctor, emitReference);
            markerCount += 4;
            if (diagnosticMode == Step35DiagnosticMode.ManagedDictionaryCompatibility)
            {
                var commandLineManagedDictionaryRewrite = ApplyCommandLineHelperManagedDictionaryCompatibilityRewrite(module, commandLineCctor, commandLineTryGetValue);
                if (commandLineManagedDictionaryRewrite.SubstitutionCount != 4)
                    throw new InvalidDataException($"Step-35.0.15 expected exactly four CommandLine managed-dictionary compatibility substitutions; observed {commandLineManagedDictionaryRewrite.SubstitutionCount}.");
                commandLineManagedDictionarySubstitutionCount = commandLineManagedDictionaryRewrite.SubstitutionCount;
            }
            else
            {
                commandLineManagedDictionarySubstitutionCount = 0;
            }
            commandLineCctorDiagnosticMaxStack = commandLineCctor.Body.MaxStackSize;
            if (commandLineCctorDiagnosticMaxStack != commandLineCctorOriginalMaxStack)
                throw new InvalidDataException($"Step-35.0.15 stack-neutral CommandLineHelper cctor unexpectedly changed MaxStack before serialization: original={commandLineCctorOriginalMaxStack}, diagnostic={commandLineCctorDiagnosticMaxStack}.");

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
            throw new InvalidDataException("Step-35.0.15 diagnostic clone verification unexpectedly resolved a dependency.");
        var verifiedConstantMetadataSha256 = RealStS2PrepareMethodRewrite.ComputeConstantMetadataFingerprint(verifyModule);
        if (!verifiedConstantMetadataSha256.Equals(expectedConstantMetadataSha256, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Step-35.0.15 diagnostic clone changed the exact transformed image's constant metadata semantics during Cecil serialization.");
        if (verifyModule.Assembly?.Name.FullName != TransformedRealStS2AssemblyAdmission.ClosedStep32AssemblyIdentity ||
            verifyModule.Mvid != TransformedRealStS2AssemblyAdmission.ClosedStep32Mvid)
            throw new InvalidDataException("Step-35.0.15 diagnostic clone changed assembly identity or MVID.");
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
            throw new InvalidDataException("Step-35.0.15 diagnostic bridge field/method signature drifted after serialization.");
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
            throw new InvalidDataException("Step-35.0.15 diagnostic bridge Invoke MemberRef is not encoded as Action<string>::Invoke(!0).");
        }

        var expectedMarkerCount = 0;
        var verifiedCctorTypes = new HashSet<string>(StringComparer.Ordinal);
        foreach (var item in GetDiagnosticMarkerTargets())
        {
            var type = EnumerateTypes(verifyModule.Types).SingleOrDefault(candidate => candidate.FullName == item.TypeName)
                ?? throw new MissingMemberException($"Step-35.0.15 diagnostic verification target type missing: {item.TypeName}.");
            var method = type.Methods.SingleOrDefault(candidate => candidate.FullName == item.MethodFullName && candidate.HasBody)
                ?? throw new MissingMethodException($"Step-35.0.15 diagnostic verification target method missing: {item.MethodFullName}.");
            if (!HasInjectedEntryMarkerAtStart(method, item.Marker))
                throw new InvalidDataException($"Step-35.0.15 marker is not the first stack-neutral checkpoint in {item.MethodFullName}: {item.Marker}.");
            expectedMarkerCount++;

            var cctor = type.Methods.SingleOrDefault(candidate => candidate.Name == ".cctor" && candidate.IsStatic && candidate.HasBody);
            if (cctor is not null && verifiedCctorTypes.Add(item.TypeName))
            {
                var cctorMarker = $"INMETHOD_CCTOR — {item.TypeName}..cctor entered";
                if (!HasInjectedEntryMarkerAtStart(cctor, cctorMarker))
                    throw new InvalidDataException($"Step-35.0.15 cctor marker is not the first stack-neutral checkpoint in {item.TypeName}..cctor.");
                expectedMarkerCount++;
            }
        }

        foreach (var item in GetDiagnosticCallsiteMarkerTargets())
        {
            var type = EnumerateTypes(verifyModule.Types).SingleOrDefault(candidate => candidate.FullName == item.TypeName)
                ?? throw new MissingMemberException($"Step-35.0.15 diagnostic callsite verification type missing: {item.TypeName}.");
            var method = type.Methods.SingleOrDefault(candidate => candidate.FullName == item.MethodFullName && candidate.HasBody)
                ?? throw new MissingMethodException($"Step-35.0.15 diagnostic callsite verification method missing: {item.MethodFullName}.");
            if (!HasInjectedCallsiteMarkers(method, item.CalleeFullName, item.BeforeMarker, item.AfterMarker))
                throw new InvalidDataException($"Step-35.0.15 callsite markers did not serialize immediately around {item.CalleeFullName} in {item.MethodFullName}.");
            expectedMarkerCount += 2;
        }

        var verifiedNullPlatformType = EnumerateTypes(verifyModule.Types).SingleOrDefault(candidate => candidate.FullName == NullPlatformTypeFullName)
            ?? throw new MissingMemberException($"Step-35.0.15 serialized NullPlatform sweep type missing: {NullPlatformTypeFullName}.");
        var verifiedNullPlatformConstructor = verifiedNullPlatformType.Methods.SingleOrDefault(candidate => candidate.FullName == NullPlatformConstructorFullName && candidate.HasBody)
            ?? throw new MissingMethodException($"Step-35.0.15 serialized NullPlatform sweep constructor missing: {NullPlatformConstructorFullName}.");
        foreach (var entry in nullPlatformCallsitePlan)
        {
            if (!HasInjectedDiagnosticCallsiteMarkers(verifiedNullPlatformConstructor, entry))
                throw new InvalidDataException($"Step-35.0.15 serialized NullPlatform CALLSITE#{entry.CallsiteOrdinal:D3} marker pair drifted around {entry.CalleeFullName}.");
            expectedMarkerCount += 2;
        }

        var verifiedCommandLineType = EnumerateTypes(verifyModule.Types).SingleOrDefault(candidate => candidate.FullName == CommandLineHelperTypeFullName)
            ?? throw new MissingMemberException($"Step-35.0.15 serialized CommandLineHelper sweep type missing: {CommandLineHelperTypeFullName}.");
        var verifiedCommandLineCctor = verifiedCommandLineType.Methods.SingleOrDefault(candidate => candidate.Name == ".cctor" && candidate.IsStatic && candidate.HasBody)
            ?? throw new MissingMethodException($"Step-35.0.15 serialized CommandLineHelper cctor missing: {CommandLineHelperTypeFullName}..cctor.");
        if (verifiedCommandLineCctor.Body.MaxStackSize != commandLineCctorDiagnosticMaxStack ||
            verifiedCommandLineCctor.Body.MaxStackSize != commandLineCctorOriginalMaxStack)
        {
            throw new InvalidDataException($"Step-35.0.15 serialized stack-neutral CommandLineHelper cctor MaxStack drifted: original={commandLineCctorOriginalMaxStack}, observed={verifiedCommandLineCctor.Body.MaxStackSize}.");
        }
        if (!HasCommandLineHelperCriticalBoundaryMarkers(verifiedCommandLineCctor))
            throw new InvalidDataException("Step-35.0.15 serialized CommandLineHelper cctor critical stack-neutral markers drifted.");
        expectedMarkerCount += 4;

        var verifiedCommandLineTryGetValue = verifiedCommandLineType.Methods.SingleOrDefault(candidate => candidate.FullName == CommandLineHelperTryGetValueFullName && candidate.HasBody)
            ?? throw new MissingMethodException($"Step-35.0.15 serialized CommandLineHelper TryGetValue missing: {CommandLineHelperTryGetValueFullName}.");
        if (diagnosticMode == Step35DiagnosticMode.ManagedDictionaryCompatibility)
            VerifyCommandLineHelperManagedDictionaryCompatibilityRewrite(verifiedCommandLineType, verifiedCommandLineCctor, verifiedCommandLineTryGetValue);
        else
            VerifyCommandLineHelperNaturalGodotDictionaryPreserved(verifiedCommandLineType, verifiedCommandLineCctor, verifiedCommandLineTryGetValue);

        var markerCountVerified = EnumerateTypes(verifyModule.Types)
            .SelectMany(type => type.Methods)
            .Where(method => method.HasBody)
            .SelectMany(method => method.Body.Instructions)
            .Count(instruction => instruction.OpCode.Code == Code.Ldstr && instruction.Operand is string text && text.StartsWith("INMETHOD_", StringComparison.Ordinal));
        if (markerCountVerified != expectedMarkerCount)
            throw new InvalidDataException($"Step-35.0.15 diagnostic clone marker count drifted after serialization: expected {expectedMarkerCount}, observed {markerCountVerified}.");

        return new DiagnosticCloneSnapshot(
            diagnosticPath,
            sha256,
            length,
            target.MetadataToken.ToUInt32(),
            moveNext.MetadataToken.ToUInt32(),
            markerCountVerified,
            commandLineCctorOriginalMaxStack,
            commandLineCctorDiagnosticMaxStack,
            commandLineManagedDictionarySubstitutionCount,
            verifiedConstantMetadataSha256,
            writeResolutionRequestCount,
            syntheticConstantTypeCount,
            approvedConstantScopeCount,
            approvedConstantRequirementCount,
            writeResolutionIdentities);
    }

    internal static GodotSharpDiagnosticCloneSnapshot CreateInstrumentedGodotSharpDiagnosticClone(string exactPreparedPath, string diagnosticPath)
    {
        if (File.Exists(diagnosticPath))
            File.Delete(diagnosticPath);

        string sourceIdentity;
        Guid sourceMvid;
        string constantRequirementFingerprint;
        int writeResolutionRequestCount;
        string writeResolutionIdentities;
        int syntheticConstantTypeCount;
        int approvedConstantScopeCount;
        int approvedConstantRequirementCount;
        IReadOnlyList<GodotSharpDiagnosticMarker> markerPlan;

        using var resolver = new SelfAuditingConstantMetadataWriteResolver();
        using (var module = ModuleDefinition.ReadModule(exactPreparedPath, new ReaderParameters
               {
                   ReadSymbols = false,
                   ReadingMode = ReadingMode.Deferred,
                   AssemblyResolver = resolver,
               }))
        {
            if (resolver.Requests.Count != 0)
                throw new InvalidDataException("Step-35.0.15 GodotSharp diagnostic deferred-open unexpectedly resolved a dependency before writer configuration.");
            sourceIdentity = module.Assembly?.Name.FullName ?? throw new InvalidDataException("Step-35.0.15 GodotSharp diagnostic source has no assembly identity.");
            sourceMvid = module.Mvid;
            var constantPlan = resolver.Configure(module);
            constantRequirementFingerprint = constantPlan.RequirementFingerprintSha256;
            syntheticConstantTypeCount = constantPlan.SyntheticTypeCount;
            approvedConstantScopeCount = constantPlan.ApprovedScopeCount;
            approvedConstantRequirementCount = constantPlan.ApprovedRequirementCount;

            if (EnumerateTypes(module.Types).Any(type => type.FullName == GodotSharpDiagnosticBridgeTypeFullName))
                throw new InvalidDataException("Step-35.0.15 GodotSharp diagnostic bridge type already exists in the exact prepared image.");

            var bridge = new TypeDefinition(
                "StS2Launcher.Step35Diagnostics",
                "GodotSharpCheckpointBridge",
                Mono.Cecil.TypeAttributes.Class | Mono.Cecil.TypeAttributes.Abstract | Mono.Cecil.TypeAttributes.Sealed |
                Mono.Cecil.TypeAttributes.Public | Mono.Cecil.TypeAttributes.BeforeFieldInit,
                module.TypeSystem.Object);
            module.Types.Add(bridge);

            var systemRuntime = module.AssemblyReferences
                .Where(reference => reference.Name == "System.Runtime")
                .OrderByDescending(reference => reference.Version)
                .FirstOrDefault()
                ?? throw new InvalidDataException("Step-35.0.15 GodotSharp diagnostic clone requires the existing System.Runtime metadata scope.");
            var (actionStringType, invoke) = CreateDiagnosticActionStringInvokeReference(module, systemRuntime);
            var callbackField = new FieldDefinition(
                GodotSharpDiagnosticBridgeCallbackFieldName,
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
            markerPlan = BuildGodotSharpDiagnosticMarkerPlan(module);
            if (markerPlan.Count < 6)
                throw new InvalidDataException($"Step-35.0.15 GodotSharp diagnostic marker plan is unexpectedly sparse: {markerPlan.Count} method(s).");
            foreach (var item in markerPlan)
            {
                var method = EnumerateTypes(module.Types)
                    .SelectMany(type => type.Methods)
                    .SingleOrDefault(method => method.FullName == item.MethodFullName && method.HasBody)
                    ?? throw new MissingMethodException($"Step-35.0.15 GodotSharp diagnostic marker target disappeared: {item.MethodFullName}.");
                InsertEntryMarker(method, emitReference, item.Marker);
            }

            Directory.CreateDirectory(Path.GetDirectoryName(diagnosticPath) ?? throw new InvalidOperationException("GodotSharp diagnostic clone path has no parent."));
            module.Write(diagnosticPath, new WriterParameters { WriteSymbols = false });
            resolver.ValidateWriteRequests();
            writeResolutionRequestCount = resolver.Requests.Count;
            writeResolutionIdentities = string.Join(" | ", resolver.Requests.Distinct(StringComparer.Ordinal).OrderBy(value => value, StringComparer.Ordinal));
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
            throw new InvalidDataException("Step-35.0.15 GodotSharp diagnostic verification unexpectedly resolved a dependency.");
        if ((verifyModule.Assembly?.Name.FullName ?? string.Empty) != sourceIdentity || verifyModule.Mvid != sourceMvid)
            throw new InvalidDataException("Step-35.0.15 GodotSharp diagnostic clone changed assembly identity or MVID.");

        var bridgeType = EnumerateTypes(verifyModule.Types).SingleOrDefault(type => type.FullName == GodotSharpDiagnosticBridgeTypeFullName)
            ?? throw new MissingMemberException(GodotSharpDiagnosticBridgeTypeFullName);
        var callback = bridgeType.Fields.SingleOrDefault(field => field.Name == GodotSharpDiagnosticBridgeCallbackFieldName)
            ?? throw new MissingFieldException(GodotSharpDiagnosticBridgeTypeFullName, GodotSharpDiagnosticBridgeCallbackFieldName);
        var bridgeEmit = bridgeType.Methods.SingleOrDefault(method => method.Name == "Emit" && method.HasBody)
            ?? throw new MissingMethodException(GodotSharpDiagnosticBridgeTypeFullName, "Emit");
        if (callback.FieldType.FullName != "System.Action`1<System.String>" || bridgeEmit.Parameters.Count != 1 || bridgeEmit.Parameters[0].ParameterType.FullName != "System.String")
            throw new InvalidDataException("Step-35.0.15 GodotSharp diagnostic bridge signature drifted after serialization.");
        var invokeSites = bridgeEmit.Body.Instructions.Where(instruction => instruction.OpCode.Code == Code.Callvirt && instruction.Operand is MethodReference).ToArray();
        if (invokeSites.Length != 1 || invokeSites[0].Operand is not MethodReference bridgeInvoke ||
            bridgeInvoke.Name != "Invoke" || bridgeInvoke.Parameters.Count != 1 ||
            bridgeInvoke.Parameters[0].ParameterType is not GenericParameter bridgeInvokeParameter ||
            bridgeInvokeParameter.Type != GenericParameterType.Type || bridgeInvokeParameter.Position != 0)
        {
            throw new InvalidDataException("Step-35.0.15 GodotSharp diagnostic bridge Invoke MemberRef is not encoded as Action<string>::Invoke(!0).");
        }

        foreach (var item in markerPlan)
        {
            var method = EnumerateTypes(verifyModule.Types)
                .SelectMany(type => type.Methods)
                .SingleOrDefault(method => method.FullName == item.MethodFullName && method.HasBody)
                ?? throw new MissingMethodException($"Step-35.0.15 serialized GodotSharp marker target missing: {item.MethodFullName}.");
            if (!HasInjectedEntryMarkerAtStart(method, item.Marker, GodotSharpDiagnosticBridgeTypeFullName))
                throw new InvalidDataException($"Step-35.0.15 serialized GodotSharp marker is not first in {item.MethodFullName}: {item.Marker}.");
        }
        var markerCount = EnumerateTypes(verifyModule.Types).SelectMany(type => type.Methods).Where(method => method.HasBody)
            .SelectMany(method => method.Body.Instructions)
            .Count(instruction => instruction.OpCode.Code == Code.Ldstr && instruction.Operand is string text && text.StartsWith("INMETHOD_GS", StringComparison.Ordinal));
        if (markerCount != markerPlan.Count)
            throw new InvalidDataException($"Step-35.0.15 GodotSharp diagnostic marker count drifted: expected {markerPlan.Count}, observed {markerCount}.");

        var markerMap = string.Join("\n", markerPlan.Select(item => $"{item.Marker} | {item.MethodFullName}"));
        return new GodotSharpDiagnosticCloneSnapshot(
            diagnosticPath,
            sha256,
            length,
            sourceIdentity,
            sourceMvid,
            markerCount,
            markerMap,
            constantRequirementFingerprint,
            writeResolutionRequestCount,
            syntheticConstantTypeCount,
            approvedConstantScopeCount,
            approvedConstantRequirementCount,
            writeResolutionIdentities);
    }

    private static IReadOnlyList<GodotSharpDiagnosticMarker> BuildGodotSharpDiagnosticMarkerPlan(ModuleDefinition module)
    {
        var allTypes = EnumerateTypes(module.Types).ToArray();
        var allMethods = allTypes.SelectMany(type => type.Methods).Where(method => method.HasBody).ToArray();
        var selected = new Dictionary<string, MethodDefinition>(StringComparer.Ordinal);
        void Add(MethodDefinition method) => selected.TryAdd(method.FullName, method);
        void AddByName(string typeName, params string[] methodNames)
        {
            var type = allTypes.SingleOrDefault(candidate => candidate.FullName == typeName);
            if (type is null) return;
            foreach (var method in type.Methods.Where(method => method.HasBody && methodNames.Contains(method.Name, StringComparer.Ordinal))) Add(method);
        }

        AddByName("Godot.Collections.Dictionary`2", ".cctor", ".ctor", "TryGetValue", "set_Item", "get_Item", "Dispose", "GetEnumerator");
        AddByName("Godot.Collections.GodotDictionary", ".cctor", ".ctor", "TryGetValue", "set_Item", "get_Item", "Dispose");
        AddByName("Godot.OS", ".cctor", "GetCmdlineArgs", "get_Singleton");
        AddByName("Godot.GodotObject", "GetPtr");
        AddByName("Godot.NativeCalls", "godot_icall_0_108");
        AddByName("Godot.NativeInterop.NativeFuncs", "Initialize");

        if (!selected.Values.Any(method => method.DeclaringType.FullName == "Godot.Collections.Dictionary`2" && method.Name == ".ctor"))
            throw new MissingMethodException("Step-35.0.15 GodotSharp diagnostic requires at least one Godot.Collections.Dictionary`2 constructor.");
        if (!selected.Values.Any(method => method.DeclaringType.FullName == "Godot.OS" && method.Name == "GetCmdlineArgs"))
            throw new MissingMethodException("Step-35.0.15 GodotSharp diagnostic requires Godot.OS.GetCmdlineArgs.");
        if (!selected.Values.Any(method => method.DeclaringType.FullName == "Godot.NativeCalls" && method.Name == "godot_icall_0_108"))
            throw new MissingMethodException("Step-35.0.15 GodotSharp diagnostic requires Godot.NativeCalls.godot_icall_0_108.");

        var localByFullName = allMethods.GroupBy(method => method.FullName, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);
        var queue = new Queue<(MethodDefinition Method, int Depth)>();
        foreach (var seed in selected.Values.Where(method =>
                     (method.DeclaringType.FullName == "Godot.Collections.Dictionary`2" && method.Name == ".ctor") ||
                     (method.DeclaringType.FullName == "Godot.OS" && method.Name == "GetCmdlineArgs") ||
                     (method.DeclaringType.FullName == "Godot.NativeCalls" && method.Name == "godot_icall_0_108")))
            queue.Enqueue((seed, 0));
        while (queue.Count != 0 && selected.Count < 96)
        {
            var (method, depth) = queue.Dequeue();
            if (depth >= 3) continue;
            foreach (var instruction in method.Body.Instructions)
            {
                if (instruction.Operand is not MethodReference called || !localByFullName.TryGetValue(called.FullName, out var local)) continue;
                if (selected.TryAdd(local.FullName, local)) queue.Enqueue((local, depth + 1));
            }
        }

        var ordered = selected.Values
            .OrderBy(method => method.DeclaringType.FullName, StringComparer.Ordinal)
            .ThenBy(method => method.MetadataToken.ToUInt32())
            .Take(96)
            .ToArray();
        return ordered.Select((method, index) => new GodotSharpDiagnosticMarker(
            $"INMETHOD_GS{index + 1:D3} — GodotSharp {method.FullName}", method.FullName)).ToArray();
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

    internal sealed record DiagnosticCallsiteSweepEntry(
        int CallsiteOrdinal,
        Code OpCodeCode,
        string CalleeFullName,
        string BeforeMarker,
        string AfterMarker);

    internal sealed record CommandLineManagedDictionaryRewriteSnapshot(int SubstitutionCount, string ManagedDictionaryTypeFullName);

    internal static CommandLineManagedDictionaryRewriteSnapshot ApplyCommandLineHelperManagedDictionaryCompatibilityRewrite(
        ModuleDefinition module,
        MethodDefinition cctor,
        MethodDefinition tryGetValue)
    {
        ArgumentNullException.ThrowIfNull(module);
        ArgumentNullException.ThrowIfNull(cctor);
        ArgumentNullException.ThrowIfNull(tryGetValue);
        if (cctor.DeclaringType.FullName != CommandLineHelperTypeFullName || cctor.Name != ".cctor" || !cctor.IsStatic || !cctor.HasBody)
            throw new InvalidDataException($"Step-35.0.15 managed CommandLine dictionary rewrite refuses unexpected cctor: {cctor.FullName}.");
        if (tryGetValue.FullName != CommandLineHelperTryGetValueFullName || !tryGetValue.HasBody || !ReferenceEquals(cctor.DeclaringType, tryGetValue.DeclaringType))
            throw new InvalidDataException($"Step-35.0.15 managed CommandLine dictionary rewrite refuses unexpected TryGetValue method: {tryGetValue.FullName}.");

        var systemCollectionsScopes = module.AssemblyReferences
            .Where(reference => reference.Name == "System.Collections")
            .OrderByDescending(reference => reference.Version)
            .ToArray();
        if (systemCollectionsScopes.Length != 1)
            throw new InvalidDataException($"Step-35.0.15 requires exactly one existing System.Collections AssemblyRef in sts2; found {systemCollectionsScopes.Length}.");
        var systemCollections = systemCollectionsScopes[0];

        // Construct the MemberRefs exactly like ECMA-335 expects for a constructed generic declaring
        // type: the declaring type is Dictionary<string,string>, while member signatures retain the
        // declaring type's VAR(0)/VAR(1) parameters. This deliberately mirrors the physically corrected
        // Action<string>::Invoke(!0) bridge encoding rather than synthesizing concrete string parameters.
        var dictionaryOpen = new TypeReference("System.Collections.Generic", "Dictionary`2", module, systemCollections, false);
        var keyParameter = new GenericParameter("TKey", dictionaryOpen);
        var valueParameter = new GenericParameter("TValue", dictionaryOpen);
        dictionaryOpen.GenericParameters.Add(keyParameter);
        dictionaryOpen.GenericParameters.Add(valueParameter);
        var dictionaryString = new GenericInstanceType(dictionaryOpen);
        dictionaryString.GenericArguments.Add(module.TypeSystem.String);
        dictionaryString.GenericArguments.Add(module.TypeSystem.String);

        var argsFields = cctor.DeclaringType.Fields.Where(field => field.Name == "_args" && field.IsStatic).ToArray();
        if (argsFields.Length != 1)
            throw new InvalidDataException($"Step-35.0.15 expected exactly one static CommandLineHelper._args field; found {argsFields.Length}.");
        var argsField = argsFields[0];
        if (!argsField.FieldType.FullName.StartsWith("Godot.Collections.Dictionary`2<System.String,System.String>", StringComparison.Ordinal))
            throw new InvalidDataException($"Step-35.0.15 expected CommandLineHelper._args to use the exact Godot string dictionary before compatibility rewriting; observed {argsField.FieldType.FullName}.");
        argsField.FieldType = dictionaryString;

        var godotDictionaryCtorSites = cctor.Body.Instructions.Where(instruction =>
            instruction.OpCode.Code == Code.Newobj &&
            instruction.Operand is MethodReference callee &&
            callee.Name == ".ctor" &&
            callee.DeclaringType.FullName.StartsWith("Godot.Collections.Dictionary`2<System.String,System.String>", StringComparison.Ordinal)).ToArray();
        if (godotDictionaryCtorSites.Length != 1)
            throw new InvalidDataException($"Step-35.0.15 expected exactly one Godot string-dictionary constructor in CommandLineHelper..cctor; found {godotDictionaryCtorSites.Length}.");
        godotDictionaryCtorSites[0].Operand = new MethodReference(".ctor", module.TypeSystem.Void, dictionaryString)
        {
            HasThis = true,
            ExplicitThis = false,
            CallingConvention = MethodCallingConvention.Default,
        };

        var godotDictionarySetterSites = cctor.Body.Instructions.Where(instruction =>
            instruction.OpCode.Code == Code.Callvirt &&
            instruction.Operand is MethodReference callee &&
            callee.Name == "set_Item" &&
            callee.DeclaringType.FullName.StartsWith("Godot.Collections.Dictionary`2<System.String,System.String>", StringComparison.Ordinal)).ToArray();
        if (godotDictionarySetterSites.Length != 1)
            throw new InvalidDataException($"Step-35.0.15 expected exactly one Godot string-dictionary set_Item in CommandLineHelper..cctor; found {godotDictionarySetterSites.Length}.");
        var managedSetter = new MethodReference("set_Item", module.TypeSystem.Void, dictionaryString)
        {
            HasThis = true,
            ExplicitThis = false,
            CallingConvention = MethodCallingConvention.Default,
        };
        managedSetter.Parameters.Add(new ParameterDefinition(keyParameter));
        managedSetter.Parameters.Add(new ParameterDefinition(valueParameter));
        godotDictionarySetterSites[0].Operand = managedSetter;

        var godotDictionaryTryGetValueSites = tryGetValue.Body.Instructions.Where(instruction =>
            instruction.OpCode.Code == Code.Callvirt &&
            instruction.Operand is MethodReference callee &&
            callee.Name == "TryGetValue" &&
            callee.DeclaringType.FullName.StartsWith("Godot.Collections.Dictionary`2<System.String,System.String>", StringComparison.Ordinal)).ToArray();
        if (godotDictionaryTryGetValueSites.Length != 1)
            throw new InvalidDataException($"Step-35.0.15 expected exactly one Godot string-dictionary TryGetValue in CommandLineHelper.TryGetValue; found {godotDictionaryTryGetValueSites.Length}.");
        var managedTryGetValue = new MethodReference("TryGetValue", module.TypeSystem.Boolean, dictionaryString)
        {
            HasThis = true,
            ExplicitThis = false,
            CallingConvention = MethodCallingConvention.Default,
        };
        managedTryGetValue.Parameters.Add(new ParameterDefinition(keyParameter));
        managedTryGetValue.Parameters.Add(new ParameterDefinition(new ByReferenceType(valueParameter)));
        godotDictionaryTryGetValueSites[0].Operand = managedTryGetValue;

        var naturalGetCmdlineArgsCount = cctor.Body.Instructions.Count(instruction =>
            instruction.OpCode.Code == Code.Call &&
            instruction.Operand is MethodReference callee &&
            callee.DeclaringType.FullName == "Godot.OS" &&
            callee.Name == "GetCmdlineArgs");
        if (naturalGetCmdlineArgsCount != 1)
            throw new InvalidDataException($"Step-35.0.15 requires the natural Godot.OS.GetCmdlineArgs call to remain exactly once after the dictionary-only compatibility rewrite; observed {naturalGetCmdlineArgsCount}.");

        return new CommandLineManagedDictionaryRewriteSnapshot(4, dictionaryString.FullName);
    }

    internal static void VerifyCommandLineHelperManagedDictionaryCompatibilityRewrite(
        TypeDefinition commandLineType,
        MethodDefinition cctor,
        MethodDefinition tryGetValue)
    {
        var argsField = commandLineType.Fields.SingleOrDefault(field => field.Name == "_args" && field.IsStatic)
            ?? throw new InvalidDataException("Step-35.0.15 serialized CommandLineHelper._args field is missing.");
        if (argsField.FieldType.FullName != ManagedStringDictionaryFullName)
            throw new InvalidDataException($"Step-35.0.15 serialized CommandLineHelper._args field is not the managed string dictionary: {argsField.FieldType.FullName}.");
        if (argsField.FieldType.Scope is not AssemblyNameReference argsScope || argsScope.Name != "System.Collections")
            throw new InvalidDataException($"Step-35.0.15 serialized managed CommandLineHelper._args field scope drifted from the existing System.Collections contract: {argsField.FieldType.Scope}.");

        static MethodReference RequireOne(MethodDefinition method, string name)
        {
            var matches = method.Body.Instructions
                .Where(instruction => instruction.OpCode.Code is Code.Call or Code.Callvirt or Code.Newobj)
                .Select(instruction => instruction.Operand)
                .OfType<MethodReference>()
                .Where(reference => reference.DeclaringType.FullName == ManagedStringDictionaryFullName && reference.Name == name)
                .ToArray();
            if (matches.Length != 1)
                throw new InvalidDataException($"Step-35.0.15 serialized managed CommandLine rewrite expected exactly one {ManagedStringDictionaryFullName}::{name} in {method.FullName}; found {matches.Length}.");
            return matches[0];
        }

        var ctor = RequireOne(cctor, ".ctor");
        if (ctor.Parameters.Count != 0 || !ctor.HasThis)
            throw new InvalidDataException("Step-35.0.15 serialized managed Dictionary<string,string> constructor MemberRef drifted.");

        var setter = RequireOne(cctor, "set_Item");
        if (setter.Parameters.Count != 2 ||
            setter.Parameters[0].ParameterType is not GenericParameter setterKey || setterKey.Type != GenericParameterType.Type || setterKey.Position != 0 ||
            setter.Parameters[1].ParameterType is not GenericParameter setterValue || setterValue.Type != GenericParameterType.Type || setterValue.Position != 1)
        {
            throw new InvalidDataException("Step-35.0.15 serialized managed Dictionary<string,string>.set_Item MemberRef is not encoded as set_Item(!0,!1).");
        }

        var tryGetValueReference = RequireOne(tryGetValue, "TryGetValue");
        if (tryGetValueReference.Parameters.Count != 2 || tryGetValueReference.ReturnType.FullName != "System.Boolean" ||
            tryGetValueReference.Parameters[0].ParameterType is not GenericParameter tryKey || tryKey.Type != GenericParameterType.Type || tryKey.Position != 0 ||
            tryGetValueReference.Parameters[1].ParameterType is not ByReferenceType byRefValue ||
            byRefValue.ElementType is not GenericParameter tryValue || tryValue.Type != GenericParameterType.Type || tryValue.Position != 1)
        {
            throw new InvalidDataException("Step-35.0.15 serialized managed Dictionary<string,string>.TryGetValue MemberRef is not encoded as TryGetValue(!0,!1&).");
        }

        var residualGodotDictionaryReferences = cctor.Body.Instructions.Concat(tryGetValue.Body.Instructions)
            .Where(instruction => instruction.Operand is MethodReference reference &&
                                  reference.DeclaringType.FullName.StartsWith("Godot.Collections.Dictionary`2<System.String,System.String>", StringComparison.Ordinal))
            .ToArray();
        if (residualGodotDictionaryReferences.Length != 0)
            throw new InvalidDataException($"Step-35.0.15 serialized CommandLine methods retained {residualGodotDictionaryReferences.Length} Godot string-dictionary call reference(s).");

        var naturalGetCmdlineArgsCount = cctor.Body.Instructions.Count(instruction =>
            instruction.OpCode.Code == Code.Call &&
            instruction.Operand is MethodReference callee &&
            callee.DeclaringType.FullName == "Godot.OS" &&
            callee.Name == "GetCmdlineArgs");
        if (naturalGetCmdlineArgsCount != 1)
            throw new InvalidDataException($"Step-35.0.15 serialized dictionary-only compatibility clone changed the natural Godot.OS.GetCmdlineArgs call count: {naturalGetCmdlineArgsCount}.");
    }

    internal static void VerifyCommandLineHelperNaturalGodotDictionaryPreserved(
        TypeDefinition commandLineType,
        MethodDefinition cctor,
        MethodDefinition tryGetValue)
    {
        var argsField = commandLineType.Fields.SingleOrDefault(field => field.Name == "_args" && field.IsStatic)
            ?? throw new InvalidDataException("Step-35.0.15 natural-recon CommandLineHelper._args field is missing.");
        if (!argsField.FieldType.FullName.StartsWith("Godot.Collections.Dictionary`2<System.String,System.String>", StringComparison.Ordinal))
            throw new InvalidDataException($"Step-35.0.15 natural-recon CommandLineHelper._args no longer uses the exact Godot string dictionary: {argsField.FieldType.FullName}.");

        var ctorCount = cctor.Body.Instructions.Count(instruction =>
            instruction.OpCode.Code == Code.Newobj && instruction.Operand is MethodReference callee &&
            callee.Name == ".ctor" && callee.DeclaringType.FullName.StartsWith("Godot.Collections.Dictionary`2<System.String,System.String>", StringComparison.Ordinal));
        var setterCount = cctor.Body.Instructions.Count(instruction =>
            instruction.OpCode.Code == Code.Callvirt && instruction.Operand is MethodReference callee &&
            callee.Name == "set_Item" && callee.DeclaringType.FullName.StartsWith("Godot.Collections.Dictionary`2<System.String,System.String>", StringComparison.Ordinal));
        var tryCount = tryGetValue.Body.Instructions.Count(instruction =>
            instruction.OpCode.Code == Code.Callvirt && instruction.Operand is MethodReference callee &&
            callee.Name == "TryGetValue" && callee.DeclaringType.FullName.StartsWith("Godot.Collections.Dictionary`2<System.String,System.String>", StringComparison.Ordinal));
        if (ctorCount != 1 || setterCount != 1 || tryCount != 1)
            throw new InvalidDataException($"Step-35.0.15 natural-recon Godot string-dictionary references drifted: ctor={ctorCount}, set_Item={setterCount}, TryGetValue={tryCount}.");

        var naturalGetCmdlineArgsCount = cctor.Body.Instructions.Count(instruction =>
            instruction.OpCode.Code == Code.Call && instruction.Operand is MethodReference callee &&
            callee.DeclaringType.FullName == "Godot.OS" && callee.Name == "GetCmdlineArgs");
        if (naturalGetCmdlineArgsCount != 1)
            throw new InvalidDataException($"Step-35.0.15 natural-recon clone changed the Godot.OS.GetCmdlineArgs call count: {naturalGetCmdlineArgsCount}.");
    }

    internal static void InsertCommandLineHelperCriticalBoundaryMarkers(MethodDefinition method, MethodReference emitReference)
    {
        if (method.DeclaringType.FullName != CommandLineHelperTypeFullName || method.Name != ".cctor" || !method.IsStatic || !method.HasBody)
            throw new InvalidDataException($"Step-35.0.15 critical CommandLineHelper cctor markers refuse unexpected method: {method.FullName}.");

        var original = method.Body.Instructions.ToArray();
        var dictionaryCtor = original.SingleOrDefault(instruction =>
            instruction.OpCode.Code == Code.Newobj &&
            instruction.Operand is MethodReference callee &&
            callee.Name == ".ctor" &&
            callee.DeclaringType.FullName.StartsWith("Godot.Collections.Dictionary`2<System.String,System.String>", StringComparison.Ordinal))
            ?? throw new InvalidDataException("Step-35.0.15 could not locate the exact CommandLineHelper _args dictionary constructor.");
        var dictionaryStore = dictionaryCtor.Next;
        if (dictionaryStore is null || dictionaryStore.OpCode.Code != Code.Stsfld ||
            dictionaryStore.Operand is not FieldReference dictionaryField || dictionaryField.Name != "_args")
        {
            throw new InvalidDataException("Step-35.0.15 requires the CommandLineHelper _args stsfld immediately after its dictionary constructor.");
        }

        var getCmdlineArgs = original.SingleOrDefault(instruction =>
            instruction.OpCode.Code == Code.Call &&
            instruction.Operand is MethodReference callee &&
            callee.DeclaringType.FullName == "Godot.OS" &&
            callee.Name == "GetCmdlineArgs")
            ?? throw new InvalidDataException("Step-35.0.15 could not locate Godot.OS.GetCmdlineArgs in CommandLineHelper..cctor.");
        var cmdlineStore = getCmdlineArgs.Next;
        if (cmdlineStore is null || cmdlineStore.OpCode.Code is not (Code.Stloc or Code.Stloc_0 or Code.Stloc_1 or Code.Stloc_2 or Code.Stloc_3 or Code.Stloc_S))
            throw new InvalidDataException("Step-35.0.15 requires Godot.OS.GetCmdlineArgs to store its result immediately before the stack-neutral POST marker.");

        foreach (var critical in new[] { dictionaryCtor, dictionaryStore, getCmdlineArgs, cmdlineStore })
        {
            if (IsInstructionBranchTarget(method, critical))
                throw new InvalidDataException($"Step-35.0.15 refuses a critical CommandLineHelper marker at branch target IL_{critical.Offset:X4}.");
        }

        EnsureMinimumDiagnosticMaxStack(method, 1);
        var il = method.Body.GetILProcessor();
        InsertMarkerBefore(il, dictionaryCtor, emitReference, CommandLineCctorDictionaryBeforeMarker);
        InsertMarkerAfter(il, dictionaryStore, emitReference, CommandLineCctorDictionaryAfterMarker);
        InsertMarkerBefore(il, getCmdlineArgs, emitReference, CommandLineCctorGetCmdlineArgsBeforeMarker);
        InsertMarkerAfter(il, cmdlineStore, emitReference, CommandLineCctorGetCmdlineArgsAfterMarker);
    }

    internal static bool HasCommandLineHelperCriticalBoundaryMarkers(MethodDefinition method)
    {
        if (!method.HasBody)
            return false;
        var markers = method.Body.Instructions
            .Where(instruction => instruction.OpCode.Code == Code.Ldstr && instruction.Operand is string)
            .Select(instruction => (string)instruction.Operand!)
            .ToHashSet(StringComparer.Ordinal);
        return markers.Contains(CommandLineCctorDictionaryBeforeMarker) &&
               markers.Contains(CommandLineCctorDictionaryAfterMarker) &&
               markers.Contains(CommandLineCctorGetCmdlineArgsBeforeMarker) &&
               markers.Contains(CommandLineCctorGetCmdlineArgsAfterMarker);
    }

    private static void EnsureMinimumDiagnosticMaxStack(MethodDefinition method, int minimum)
    {
        if (method.Body.MaxStackSize < minimum)
            method.Body.MaxStackSize = minimum;
    }

    private static void ReserveLiveStackDiagnosticMarkerSlot(MethodDefinition method)
        => method.Body.MaxStackSize = checked(method.Body.MaxStackSize + 1);

    private static bool IsInstructionBranchTarget(MethodDefinition method, Instruction target)
        => method.Body.Instructions.Any(candidate => candidate.Operand switch
        {
            Instruction single => ReferenceEquals(single, target),
            Instruction[] many => many.Any(item => ReferenceEquals(item, target)),
            _ => false,
        });

    private static void InsertMarkerBefore(ILProcessor il, Instruction target, MethodReference emitReference, string marker)
    {
        il.InsertBefore(target, Instruction.Create(OpCodes.Ldstr, marker));
        il.InsertBefore(target, Instruction.Create(OpCodes.Call, emitReference));
    }

    private static void InsertMarkerAfter(ILProcessor il, Instruction target, MethodReference emitReference, string marker)
    {
        var text = Instruction.Create(OpCodes.Ldstr, marker);
        il.InsertAfter(target, text);
        il.InsertAfter(text, Instruction.Create(OpCodes.Call, emitReference));
    }

    internal static IReadOnlyList<DiagnosticCallsiteSweepEntry> InsertNullPlatformConstructorCallsiteMarkers(
        MethodDefinition method,
        MethodReference emitReference)
    {
        if (method.FullName != NullPlatformConstructorFullName)
            throw new InvalidDataException($"Step-35.0.15 NullPlatform callsite sweep refuses unexpected method: {method.FullName}.");
        return InsertDiagnosticCallsiteSweepMarkers(
            method,
            emitReference,
            "INMETHOD_NP",
            "NullPlatformUtilStrategy..ctor",
            skipDirectBaseConstructor: true,
            failOnBranchTarget: true);
    }

    internal static IReadOnlyList<DiagnosticCallsiteSweepEntry> InsertCommandLineHelperCctorCallsiteMarkers(
        MethodDefinition method,
        MethodReference emitReference)
    {
        if (method.DeclaringType.FullName != CommandLineHelperTypeFullName || method.Name != ".cctor" || !method.IsStatic)
            throw new InvalidDataException($"Step-35.0.15 CommandLineHelper cctor sweep refuses unexpected method: {method.FullName}.");
        return InsertDiagnosticCallsiteSweepMarkers(
            method,
            emitReference,
            "INMETHOD_CL",
            "CommandLineHelper..cctor",
            skipDirectBaseConstructor: false,
            failOnBranchTarget: false);
    }

    internal static IReadOnlyList<DiagnosticCallsiteSweepEntry> InsertCommandLineHelperTryGetValueCallsiteMarkers(
        MethodDefinition method,
        MethodReference emitReference)
    {
        if (method.FullName != CommandLineHelperTryGetValueFullName)
            throw new InvalidDataException($"Step-35.0.15 CommandLineHelper TryGetValue sweep refuses unexpected method: {method.FullName}.");
        return InsertDiagnosticCallsiteSweepMarkers(
            method,
            emitReference,
            "INMETHOD_CLTV",
            "CommandLineHelper.TryGetValue",
            skipDirectBaseConstructor: false,
            failOnBranchTarget: false);
    }

    internal static IReadOnlyList<DiagnosticCallsiteSweepEntry> InsertDiagnosticCallsiteSweepMarkers(
        MethodDefinition method,
        MethodReference emitReference,
        string markerPrefix,
        string displayMethodName,
        bool skipDirectBaseConstructor,
        bool failOnBranchTarget)
    {
        if (!method.HasBody || method.Body.Instructions.Count == 0)
            throw new InvalidDataException($"Cannot instrument diagnostic callsite sweep without IL: {method.FullName}.");
        if (string.IsNullOrWhiteSpace(markerPrefix) || string.IsNullOrWhiteSpace(displayMethodName))
            throw new ArgumentException("Diagnostic callsite sweep marker prefix/display name are required.");

        // Snapshot after entry markers have been inserted, because that is the real production ordering.
        // Injected bridge calls must be ignored BEFORE ordinal accounting so dynamic marker ordinals stay
        // aligned with the exact-source static map. Physical 0.0.132 exposed the old +1 skew here.
        var originalInstructions = method.Body.Instructions.ToArray();
        var result = new List<DiagnosticCallsiteSweepEntry>();
        var callsiteOrdinal = 0;
        foreach (var instruction in originalInstructions)
        {
            if (instruction.OpCode.Code is not (Code.Call or Code.Callvirt or Code.Newobj) ||
                instruction.Operand is not MethodReference callee)
            {
                continue;
            }

            if (callee.DeclaringType.FullName == DiagnosticBridgeTypeFullName && callee.Name == "Emit")
                continue;

            callsiteOrdinal++;

            // Do not hold a constructor's uninitialized `this` across a diagnostic callback.
            // The entry marker already proves entry; exact-source ordinal accounting still includes
            // this base call, but it is intentionally left unwrapped.
            if (skipDirectBaseConstructor && method.IsConstructor && instruction.OpCode.Code == Code.Call && callee.Name == ".ctor" &&
                method.DeclaringType.BaseType is not null &&
                callee.DeclaringType.FullName == method.DeclaringType.BaseType.FullName)
            {
                continue;
            }

            var isBranchTarget = IsInstructionBranchTarget(method, instruction);
            if (isBranchTarget)
            {
                if (failOnBranchTarget)
                    throw new InvalidDataException($"Step-35.0.15 refuses to sweep branch-target CALLSITE#{callsiteOrdinal:D3} ({callee.FullName}) in {method.FullName}.");
                continue;
            }

            var beforeMarker = $"{markerPrefix}{callsiteOrdinal:D3}_PRE — {displayMethodName} CALLSITE#{callsiteOrdinal:D3} before {instruction.OpCode.Name} {callee.FullName}";
            var afterMarker = $"{markerPrefix}{callsiteOrdinal:D3}_POST — {displayMethodName} CALLSITE#{callsiteOrdinal:D3} after {instruction.OpCode.Name} {callee.FullName}";
            var il = method.Body.GetILProcessor();
            il.InsertBefore(instruction, Instruction.Create(OpCodes.Ldstr, beforeMarker));
            il.InsertBefore(instruction, Instruction.Create(OpCodes.Call, emitReference));
            var afterText = Instruction.Create(OpCodes.Ldstr, afterMarker);
            il.InsertAfter(instruction, afterText);
            il.InsertAfter(afterText, Instruction.Create(OpCodes.Call, emitReference));
            result.Add(new DiagnosticCallsiteSweepEntry(callsiteOrdinal, instruction.OpCode.Code, callee.FullName, beforeMarker, afterMarker));
        }

        if (result.Count == 0)
            throw new InvalidDataException($"Step-35.0.15 diagnostic callsite sweep found no eligible managed call/newobj target in {method.FullName}.");

        // PRE/POST markers may execute while original call arguments or return values are still
        // on the evaluation stack. Reserve one additional slot in the method header. Physical
        // 0.0.133 proved that Cecil round-trip verification alone does not catch an undersized
        // MaxStack header: the CLR rejected CommandLineHelper..cctor before instruction zero.
        ReserveLiveStackDiagnosticMarkerSlot(method);
        return result;
    }

    private static bool HasInjectedDiagnosticCallsiteMarkers(
        MethodDefinition method,
        DiagnosticCallsiteSweepEntry entry)
        => method.Body.Instructions.Any(callsite =>
            callsite.OpCode.Code == entry.OpCodeCode &&
            callsite.Operand is MethodReference callee &&
            callee.FullName == entry.CalleeFullName &&
            callsite.Previous?.OpCode.Code == Code.Call &&
            callsite.Previous.Operand is MethodReference beforeEmit &&
            beforeEmit.DeclaringType.FullName == DiagnosticBridgeTypeFullName &&
            beforeEmit.Name == "Emit" &&
            callsite.Previous.Previous?.OpCode.Code == Code.Ldstr &&
            Equals(callsite.Previous.Previous.Operand, entry.BeforeMarker) &&
            callsite.Next?.OpCode.Code == Code.Ldstr &&
            Equals(callsite.Next.Operand, entry.AfterMarker) &&
            callsite.Next.Next?.OpCode.Code == Code.Call &&
            callsite.Next.Next.Operand is MethodReference afterEmit &&
            afterEmit.DeclaringType.FullName == DiagnosticBridgeTypeFullName &&
            afterEmit.Name == "Emit");

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
            throw new InvalidDataException($"Step-35.0.15 expected exactly one callsite for {calleeFullName} in {method.FullName}; found {matches.Length}.");

        var callsite = matches[0];
        var isBranchTarget = IsInstructionBranchTarget(method, callsite);
        if (isBranchTarget)
            throw new InvalidDataException($"Step-35.0.15 refuses to place a pre-call marker on branch-target callsite {calleeFullName} in {method.FullName}.");

        ReserveLiveStackDiagnosticMarkerSlot(method);
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
        EnsureMinimumDiagnosticMaxStack(method, 1);
        var first = method.Body.Instructions[0];
        var il = method.Body.GetILProcessor();
        il.InsertBefore(first, Instruction.Create(OpCodes.Ldstr, marker));
        il.InsertBefore(first, Instruction.Create(OpCodes.Call, emitReference));
    }

    private static bool HasInjectedEntryMarker(MethodDefinition method, string marker)
        => method.HasBody && method.Body.Instructions.Any(instruction => instruction.OpCode.Code == Code.Ldstr && Equals(instruction.Operand, marker));

    private static bool HasInjectedEntryMarkerAtStart(
        MethodDefinition method,
        string marker,
        string expectedBridgeTypeFullName = DiagnosticBridgeTypeFullName)
    {
        if (!method.HasBody || method.Body.Instructions.Count < 2)
            return false;
        var markerInstruction = method.Body.Instructions[0];
        var callInstruction = method.Body.Instructions[1];
        return markerInstruction.OpCode.Code == Code.Ldstr && Equals(markerInstruction.Operand, marker) &&
               callInstruction.OpCode.Code == Code.Call && callInstruction.Operand is MethodReference call &&
               call.Name == "Emit" && call.DeclaringType.FullName == expectedBridgeTypeFullName;
    }

    internal sealed record GodotSharpDiagnosticCloneSnapshot(
        string Path,
        string Sha256,
        long Length,
        string AssemblyIdentity,
        Guid Mvid,
        int MarkerCount,
        string MarkerMap,
        string ConstantRequirementFingerprintSha256,
        int WriteResolutionRequestCount,
        int SyntheticConstantTypeCount,
        int ApprovedConstantScopeCount,
        int ApprovedConstantRequirementCount,
        string WriteResolutionIdentities);

    private sealed record GodotSharpDiagnosticMarker(string Marker, string MethodFullName);

    internal sealed record PrivateDiagnosticOverride(
        string SimpleName,
        string Path,
        string Sha256,
        long Length,
        string AssemblyIdentity,
        Guid Mvid,
        string BridgeTypeFullName,
        string BridgeCallbackFieldName,
        int MarkerCount);

    private sealed record DiagnosticCloneSnapshot(
        string Path,
        string Sha256,
        long Length,
        uint MethodToken,
        uint MoveNextToken,
        int MarkerCount,
        int CommandLineCctorOriginalMaxStack,
        int CommandLineCctorDiagnosticMaxStack,
        int CommandLineManagedDictionarySubstitutionCount,
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
        Step35DiagnosticMode DiagnosticMode,
        GodotSharpDiagnosticCloneSnapshot GodotSharpDiagnostic,
        string GodotReconnaissanceReport,
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
        private readonly IReadOnlyDictionary<string, PrivateDiagnosticOverride> _diagnosticOverridesBySimpleName;
        private readonly RuntimeBindingHostFramework[] _hostBindings;
        private readonly Action<string>? _crashCheckpoint;

        internal Step35ExecutionLoadContext(
            string name,
            RuntimeFrameworkBindingPlanDocument plan,
            IReadOnlyList<PreparedExecutionEntry> preparedAssemblies,
            bool isCollectible,
            Action<string>? crashCheckpoint = null,
            IReadOnlyList<PrivateDiagnosticOverride>? diagnosticOverrides = null)
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
            var overrides = new Dictionary<string, PrivateDiagnosticOverride>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in diagnosticOverrides ?? Array.Empty<PrivateDiagnosticOverride>())
            {
                if (!privateBySimpleName.ContainsKey(item.SimpleName))
                    throw new InvalidDataException($"Step-35 diagnostic private override has no prepared-plan source: {item.SimpleName}.");
                if (!overrides.TryAdd(item.SimpleName, item))
                    throw new InvalidDataException($"Step-35 duplicate diagnostic private override: {item.SimpleName}.");
            }
            _diagnosticOverridesBySimpleName = overrides;
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

                var selectedPath = privateAssembly.PreparedPath;
                PrivateDiagnosticOverride? diagnosticOverride = null;
                if (_diagnosticOverridesBySimpleName.TryGetValue(assemblyName.Name, out var selectedOverride))
                {
                    diagnosticOverride = selectedOverride;
                    VerifyFileLength(selectedOverride.Path, selectedOverride.Length, $"Step-35 diagnostic private override {selectedOverride.SimpleName}");
                    var diagnosticSha256 = ComputeSha256Hex(selectedOverride.Path);
                    if (!diagnosticSha256.Equals(selectedOverride.Sha256, StringComparison.OrdinalIgnoreCase))
                        throw new InvalidDataException($"Step-35 diagnostic private override SHA-256 changed immediately before load: {selectedOverride.SimpleName}.");
                    selectedPath = selectedOverride.Path;
                    Checkpoint($"RESOLVE_PRIVATE_DIAGNOSTIC_SELECTED — {requestedFullName}; derivativeSha256={diagnosticSha256}; markerCount={selectedOverride.MarkerCount}");
                }

                using var stream = new FileStream(selectedPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                Checkpoint($"RESOLVE_PRIVATE_LOADFROMSTREAM_START — {requestedFullName}");
                var loaded = LoadFromStream(stream);
                Checkpoint($"RESOLVE_PRIVATE_LOADFROMSTREAM_PASS — {requestedFullName}");
                var actualFullName = loaded.GetName().FullName ?? loaded.GetName().Name ?? string.Empty;
                if (!actualFullName.Equals(privateAssembly.Plan.AssemblyFullName, StringComparison.Ordinal))
                    throw new FileLoadException($"Step-35 private dependency loaded identity drifted. Planned '{privateAssembly.Plan.AssemblyFullName}', actual '{actualFullName}'.");

                if (diagnosticOverride is not null)
                {
                    if (!actualFullName.Equals(diagnosticOverride.AssemblyIdentity, StringComparison.Ordinal) || loaded.ManifestModule.ModuleVersionId != diagnosticOverride.Mvid)
                        throw new FileLoadException($"Step-35 diagnostic private override identity/MVID drifted for {diagnosticOverride.SimpleName}.");
                    var bridgeType = loaded.GetType(diagnosticOverride.BridgeTypeFullName, throwOnError: true, ignoreCase: false)
                        ?? throw new MissingMemberException(diagnosticOverride.BridgeTypeFullName);
                    var bridgeField = bridgeType.GetField(diagnosticOverride.BridgeCallbackFieldName, BindingFlags.Static | BindingFlags.Public)
                        ?? throw new MissingFieldException(diagnosticOverride.BridgeTypeFullName, diagnosticOverride.BridgeCallbackFieldName);
                    if (bridgeField.FieldType != typeof(Action<string>))
                        throw new InvalidDataException($"Step-35 diagnostic private bridge field type drifted for {diagnosticOverride.SimpleName}: {bridgeField.FieldType.FullName}.");
                    bridgeField.SetValue(null, _crashCheckpoint);
                    Checkpoint($"GODOT_DIAGNOSTIC_BRIDGE_ARMED — {diagnosticOverride.SimpleName} entry-only callback armed before resolver returned the assembly; markerCount={diagnosticOverride.MarkerCount}.");
                }

                PrivateLoads.Add($"{requestedFullName} => {actualFullName}" + (diagnosticOverride is null ? string.Empty : " [diagnostic derivative]"));
                Checkpoint($"RESOLVE_PRIVATE_PASS — {requestedFullName} => {actualFullName}" + (diagnosticOverride is null ? string.Empty : " [diagnostic derivative]"));
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
                throw new InvalidDataException($"Step-35.0.15 constant provider '{provider}' has unsupported metadata scope '{leaf.Scope?.MetadataScopeType}'.");

            var typeCode = Type.GetTypeCode(constant.GetType());
            if (!IsSupportedDiagnosticConstantTypeCode(typeCode))
                throw new InvalidDataException($"Step-35.0.15 constant provider '{provider}' has unsupported constant storage type {constant.GetType().FullName}.");
            var key = new DiagnosticExternalConstantTypeKey(assemblyReference.FullName, leaf.FullName, leaf.IsNested);
            if (requirements.TryGetValue(key, out var prior) && prior != typeCode)
                throw new InvalidDataException($"Step-35.0.15 external constant type '{leaf.FullName}' has inconsistent storage types {prior} and {typeCode}.");
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
            _ => throw new InvalidDataException($"Unsupported Step-35.0.15 constant storage type {typeCode}."),
        };

    private sealed class SelfAuditingConstantMetadataWriteResolver : IAssemblyResolver
    {
        private readonly List<string> _requests = [];
        private readonly Dictionary<string, AssemblyDefinition> _surrogates = new(StringComparer.Ordinal);
        private bool _configured;

        internal IReadOnlyList<string> Requests => _requests;

        internal SelfAuditingConstantMetadataResolutionPlan Configure(ModuleDefinition sourceModule)
        {
            if (_configured)
                throw new InvalidOperationException("The Step-35.0.15 self-auditing GodotSharp constant-metadata resolver was already configured.");
            _configured = true;
            var requirements = CollectDiagnosticExternalConstantTypeRequirements(sourceModule);
            var fingerprintText = string.Join("\n", requirements
                .OrderBy(pair => pair.Key.AssemblyFullName, StringComparer.Ordinal)
                .ThenBy(pair => pair.Key.TypeFullName, StringComparer.Ordinal)
                .Select(pair => $"{pair.Key.AssemblyFullName}|{pair.Key.TypeFullName}|{pair.Value}|nested={pair.Key.IsNested}"));
            var fingerprint = Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(fingerprintText))).ToLowerInvariant();

            var assemblyReferences = new Dictionary<string, AssemblyNameReference>(StringComparer.Ordinal);
            foreach (var identity in requirements.Keys.Select(key => key.AssemblyFullName).Distinct(StringComparer.Ordinal))
            {
                var matches = sourceModule.AssemblyReferences.Where(reference => reference.FullName.Equals(identity, StringComparison.Ordinal)).ToArray();
                if (matches.Length != 1)
                    throw new InvalidDataException($"Step-35.0.15 GodotSharp source must contain exactly one AssemblyRef for self-audited constant scope {identity}; found {matches.Length}.");
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
                var surrogate = AssemblyDefinition.CreateAssembly(surrogateName, $"Step35.GodotSharp.{safeName}.ConstantMetadataSurrogate.dll", ModuleKind.Dll);
                _surrogates.Add(scopeGroup.Key, surrogate);
                foreach (var requirement in scopeGroup.OrderBy(pair => pair.Key.TypeFullName, StringComparer.Ordinal))
                {
                    if (requirement.Key.IsNested)
                        throw new InvalidDataException($"Step-35.0.15 GodotSharp diagnostic does not permit nested external constant type synthesis: {requirement.Key.TypeFullName}.");
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
            return new SelfAuditingConstantMetadataResolutionPlan(syntheticTypeCount, _surrogates.Count, requirements.Count, fingerprint);
        }

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
            var unexpected = _requests.Where(value => !_surrogates.ContainsKey(value)).Distinct(StringComparer.Ordinal).OrderBy(value => value, StringComparer.Ordinal).ToArray();
            if (unexpected.Length != 0)
                throw new InvalidDataException("Step-35.0.15 GodotSharp Cecil serialization attempted an unapproved assembly resolution: " + string.Join(" | ", unexpected));
        }

        public void Dispose()
        {
            foreach (var surrogate in _surrogates.Values) surrogate.Dispose();
            _surrogates.Clear();
        }
    }

    private sealed record SelfAuditingConstantMetadataResolutionPlan(
        int SyntheticTypeCount,
        int ApprovedScopeCount,
        int ApprovedRequirementCount,
        string RequirementFingerprintSha256);

    private sealed class DiagnosticConstantMetadataWriteResolver : IAssemblyResolver
    {
        private readonly List<string> _requests = [];
        private readonly Dictionary<string, AssemblyDefinition> _surrogates = new(StringComparer.Ordinal);
        private bool _configured;

        internal IReadOnlyList<string> Requests => _requests;

        internal DiagnosticConstantMetadataResolutionPlan Configure(ModuleDefinition sourceModule)
        {
            if (_configured)
                throw new InvalidOperationException("The Step-35.0.15 constant-metadata write resolver was already configured.");
            _configured = true;

            var requirements = CollectDiagnosticExternalConstantTypeRequirements(sourceModule);
            ValidateAuditedRequirementSet(requirements);

            var assemblyReferences = new Dictionary<string, AssemblyNameReference>(StringComparer.Ordinal);
            foreach (var identity in requirements.Keys.Select(key => key.AssemblyFullName).Distinct(StringComparer.Ordinal))
            {
                var matches = sourceModule.AssemblyReferences.Where(reference => reference.FullName.Equals(identity, StringComparison.Ordinal)).ToArray();
                if (matches.Length != 1)
                    throw new InvalidDataException($"Step-35.0.15 source must contain exactly one AssemblyRef for audited constant-metadata scope {identity}; found {matches.Length}.");
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
                        throw new InvalidDataException($"Step-35.0.15 does not permit nested external constant type synthesis: {requirement.Key.TypeFullName}.");
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
            throw new InvalidDataException("Step-35.0.15 external constant-metadata requirement set drifted from the physically proven Step-32 audit; " + string.Join("; ", detail));
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
                throw new InvalidDataException("Step-35.0.15 expected Cecil serialization to use at least one bounded constant-metadata surrogate, but no write-time resolution request occurred.");
            var unexpected = _requests.Where(value => !_surrogates.ContainsKey(value)).Distinct(StringComparer.Ordinal).OrderBy(value => value, StringComparer.Ordinal).ToArray();
            if (unexpected.Length != 0)
                throw new InvalidDataException("Step-35.0.15 Cecil serialization attempted an unapproved assembly resolution: " + string.Join(" | ", unexpected));
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
