using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.Loader;
using System.Security.Cryptography;
using System.Text.Json;
using Mono.Cecil;

namespace StS2Launcher.Core;

/// <summary>
/// Step 33 boundary. Re-manufactures and independently verifies the physically closed Step-32
/// transformed real-StS2 image, then admits only those exact transformed bytes into a dedicated
/// AssemblyLoadContext. No game type/member reflection, method invocation, entry point, Godot startup,
/// or native game loading is authorized. The receipt-backed/original sts2.dll remains outside the CLR.
/// </summary>
public sealed class TransformedRealStS2AssemblyAdmission : IDisposable
{
    public const string LoadContextName = "StS2Launcher-Step33-TransformedGame";
    public const string ExpectedPrimarySimpleName = "sts2";
    public const string ExactPrimaryRelativePath = "SlayTheSpire2.app/Contents/Resources/data_sts2_macos_arm64/sts2.dll";
    public const string ClosedStep32SourceSha256 = "e7ceb80669bfaf5c8fccabaa126ae2bb283aba514be5b5b55612579cfd285f18";
    public const string ClosedStep32TransformedSha256 = "39c0a89ad0d5c6eb1553e23dd8537a7b7ab8278fad4115d186db5751570211ef";
    public const long ClosedStep32TransformedBytes = 9_304_576;
    public const string ClosedStep32AssemblyIdentity = "sts2, Version=0.1.0.0, Culture=neutral, PublicKeyToken=null";
    public static readonly Guid ClosedStep32Mvid = Guid.Parse("518e4758-52d7-47c2-b776-471a0e29e49d");
    public const string ClosedStep32TransformedSemanticSha256 = "47fadf2a46eda098f310b7d0ee54e37d1e952ac272fc966d16d557ed46a0b74a";

    private readonly string _launcherDataRoot;
    private readonly string _preparedWorkRoot;
    private readonly string _preparedRoot;
    private readonly string _planPath;
    private readonly RealStS2PrepareMethodRewrite _rewrite;
    private readonly FirstRealGameAssemblyLoad _preparedPreflight;
    private readonly SteamOfflineInstallInspection _offlineInspection;
    private readonly bool _collectibleLoadContext;

    private AdmissionPreflightSnapshot? _preflight;
    private PrimaryAdmissionSnapshot? _primaryAdmission;
    private ResolverAuditSnapshot? _resolverAudit;
    private Step33AdmissionLoadContext? _loadContext;
    private bool _disposed;

    public TransformedRealStS2AssemblyAdmission(string launcherDataRoot, bool collectibleLoadContext = false)
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
        _primaryAdmission = null;
        _resolverAudit = null;
    }

    public async Task<TransformedRealStS2AssemblyAdmissionGateResult> RunVerifiedTransformedImagePreflightAsync(
        IProgress<TransformedRealStS2AssemblyAdmissionProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        const TransformedRealStS2AssemblyAdmissionGate gate = TransformedRealStS2AssemblyAdmissionGate.VerifiedTransformedImagePreflight;
        var stage = "initialization";
        try
        {
            Reset();
            EnsureNoStS2Loaded("Gate A entry");
            cancellationToken.ThrowIfCancellationRequested();

            stage = "fresh Step-32 transformed-image manufacture and verification";
            progress?.Report(new(gate, 0, 6, null,
                "Re-running the physically closed Step-32 A-D contract to manufacture and independently verify a fresh launcher-private transformed sts2.dll. No CLR admission occurs in this sub-boundary."));

            var rewriteA = await _rewrite.RunSourceAdmissionAndPrivateCloneAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            RequireRewritePass("Step 32 Gate A", rewriteA);
            progress?.Report(new(gate, 1, 6, null, "Step-32 Gate A requalified."));

            var rewriteB = _rewrite.RunDeterministicStackNeutralRewrite();
            RequireRewritePass("Step 32 Gate B", rewriteB);
            progress?.Report(new(gate, 2, 6, null, "Step-32 Gate B requalified; transformed bytes materialized privately."));

            var rewriteC = _rewrite.RunTransformedImageVerification();
            RequireRewritePass("Step 32 Gate C", rewriteC);
            progress?.Report(new(gate, 3, 6, null, "Step-32 Gate C requalified; reopened semantic verification passed."));

            var rewriteD = await _rewrite.RunFinalIsolationAuditAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            RequireRewritePass("Step 32 Gate D", rewriteD);
            progress?.Report(new(gate, 4, 6, null, "Step-32 Gate D requalified; original install remains isolated."));

            var transformedPath = Path.Combine(
                _launcherDataRoot,
                RealStS2PrepareMethodRewrite.WorkRootName,
                RealStS2PrepareMethodRewrite.TransformedRootName,
                RealStS2PrepareMethodRewrite.PrimaryFileName);
            stage = "closed Step-32 transformed artifact identity";
            VerifyFileLength(transformedPath, ClosedStep32TransformedBytes, "transformed primary");
            var transformedSha256 = ComputeSha256Hex(transformedPath);
            if (!transformedSha256.Equals(ClosedStep32TransformedSha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException($"Step 33 requires the exact physically closed Step-32 transformed SHA-256 {ClosedStep32TransformedSha256}; observed {transformedSha256}.");

            using (var resolver = new RejectingAssemblyResolver())
            using (var module = ModuleDefinition.ReadModule(transformedPath, new ReaderParameters
                   {
                       ReadSymbols = false,
                       ReadingMode = ReadingMode.Deferred,
                       AssemblyResolver = resolver,
                   }))
            {
                if (module.Assembly?.Name.FullName != ClosedStep32AssemblyIdentity || module.Mvid != ClosedStep32Mvid)
                    throw new InvalidDataException("Step-33 transformed image assembly identity/MVID drifted from the physically closed Step-32 image.");
                var method = RealStS2PrepareMethodRewrite.FindMethodByStableIdentity(
                    module,
                    "MegaCrit.Sts2.Core.Helpers.OneTimeInitialization",
                    "System.Void MegaCrit.Sts2.Core.Helpers.OneTimeInitialization::PrewarmJit()");
                var semanticSha256 = RealStS2PrepareMethodRewrite.ComputeMethodSemanticFingerprint(method);
                if (!semanticSha256.Equals(ClosedStep32TransformedSemanticSha256, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException($"Step-33 transformed PrewarmJit semantic fingerprint drifted: {semanticSha256}.");
                var prepareMethodCount = method.Body.Instructions.Count(instruction =>
                    instruction.Operand is MethodReference reference &&
                    reference.DeclaringType.FullName == "System.Runtime.CompilerServices.RuntimeHelpers" &&
                    reference.Name == "PrepareMethod");
                if (prepareMethodCount != 0)
                    throw new InvalidDataException($"Step-33 transformed PrewarmJit unexpectedly contains {prepareMethodCount} PrepareMethod reference(s).");
                if (resolver.Requests.Count != 0)
                    throw new InvalidDataException("Step-33 transformed-image admission preflight unexpectedly resolved a dependency through Cecil.");
            }
            progress?.Report(new(gate, 5, 6, transformedPath, "Exact physically closed transformed hash/identity/semantic fingerprint requalified."));

            stage = "Step-21/22 prepared runtime-plan preflight";
            var preparedResult = await _preparedPreflight.RunPreparedLoadPreflightAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            if (!preparedResult.Passed)
                throw new InvalidDataException("Step-33 prepared runtime-plan preflight failed: " + preparedResult.Detail.Replace('\n', ' '));

            var planBytes = await File.ReadAllBytesAsync(_planPath, cancellationToken).ConfigureAwait(false);
            var planSha256 = Convert.ToHexString(SHA256.HashData(planBytes)).ToLowerInvariant();
            var plan = JsonSerializer.Deserialize(planBytes, RuntimeFrameworkBindingJsonContext.Default.RuntimeFrameworkBindingPlanDocument)
                ?? throw new InvalidDataException("Step 33 could not deserialize the persisted Step-21/22 runtime-binding plan.");
            ValidateAdmissionPlan(plan);

            var prepared = plan.PreparedAssemblies.Select(item =>
            {
                var relative = NormalizeRelative(item.RelativePath);
                var path = ResolveChildPath(_preparedRoot, relative, "Step-33 prepared dependency path");
                var identity = new AssemblyName(item.AssemblyFullName);
                var moduleInitializerCount = ReadModuleInitializerCount(path);
                return new PreparedAdmissionEntry(item, path, identity, moduleInitializerCount);
            }).ToArray();
            var preparedPrimary = prepared.Single(item => item.Plan.IsPrimary);
            if (!preparedPrimary.Plan.AssemblyFullName.Equals(ClosedStep32AssemblyIdentity, StringComparison.Ordinal))
                throw new InvalidDataException("Step-33 prepared-plan primary identity differs from the physically closed Step-32 transformed identity.");

            _preflight = new AdmissionPreflightSnapshot(
                transformedPath,
                transformedSha256,
                plan,
                planSha256,
                prepared,
                preparedPrimary);

            EnsureNoStS2Loaded("Gate A exit");
            progress?.Report(new(gate, 6, 6, _planPath, "Step-33 admission-only preflight complete; no game assembly has entered the CLR."));

            return Pass(gate,
                "EXACT STEP-32 TRANSFORMED REAL-STS2 IMAGE RE-MANUFACTURED, REVERIFIED, AND QUALIFIED FOR CLR ADMISSION; NO CLR LOAD OCCURRED.\n" +
                "Physical Step-32 closure re-run inside Gate A: 4/4 PASS\n" +
                $"Transformed SHA-256: {transformedSha256}\n" +
                $"Transformed bytes: {ClosedStep32TransformedBytes:N0}\n" +
                $"Assembly identity: {ClosedStep32AssemblyIdentity}\n" +
                $"Module MVID: {ClosedStep32Mvid}\n" +
                $"Transformed PrewarmJit semantic fingerprint: {ClosedStep32TransformedSemanticSha256}\n" +
                "Transformed PrepareMethod references: 0\n" +
                $"Prepared runtime-binding plan SHA-256: {planSha256}\n" +
                $"Prepared private/game assemblies requalified by Step-23 preflight: {prepared.Length:N0}\n" +
                $"Host framework binding entries available to the admission-only resolver: {plan.HostFrameworkBindings.Length:N0}\n" +
                "Original receipt-backed/prepared sts2.dll admitted to CLR: NO\n" +
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
        Justification = "Step 33 loads only the exact independently reverified Step-32 transformed image. It performs no game type/member reflection or invocation.")]
    public TransformedRealStS2AssemblyAdmissionGateResult RunTransformedPrimaryClrAdmission()
    {
        const TransformedRealStS2AssemblyAdmissionGate gate = TransformedRealStS2AssemblyAdmissionGate.TransformedPrimaryClrAdmission;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var preflight = RequirePreflight();
            EnsureNoStS2Loaded("Gate B entry");
            if (_loadContext is not null)
                throw new InvalidOperationException("Step 33 Gate B requires a fresh dedicated load context.");

            stage = "immediate transformed hash recheck";
            VerifyFileLength(preflight.TransformedPath, ClosedStep32TransformedBytes, "transformed primary");
            var immediateSha256 = ComputeSha256Hex(preflight.TransformedPath);
            if (!immediateSha256.Equals(ClosedStep32TransformedSha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Step-33 transformed image changed between Gate A verification and Gate B CLR admission.");

            stage = "dedicated transformed-game AssemblyLoadContext construction";
            var context = new Step33AdmissionLoadContext(
                LoadContextName,
                preflight.Plan,
                preflight.PreparedAssemblies,
                _collectibleLoadContext);
            _loadContext = context;

            stage = "exact transformed sts2.dll LoadFromStream";
            var assembly = context.LoadPrimary(preflight.TransformedPath, immediateSha256);
            if (!ReferenceEquals(AssemblyLoadContext.GetLoadContext(assembly), context))
                throw new InvalidDataException("The transformed sts2.dll did not load into the dedicated Step-33 AssemblyLoadContext.");

            var actualIdentity = assembly.GetName().FullName ?? assembly.GetName().Name ?? string.Empty;
            if (!actualIdentity.Equals(ClosedStep32AssemblyIdentity, StringComparison.Ordinal))
                throw new InvalidDataException($"Loaded transformed identity mismatch. Expected '{ClosedStep32AssemblyIdentity}', actual '{actualIdentity}'.");
            var actualMvid = assembly.ManifestModule.ModuleVersionId;
            if (actualMvid != ClosedStep32Mvid)
                throw new InvalidDataException($"Loaded transformed module MVID mismatch. Expected {ClosedStep32Mvid}, actual {actualMvid}.");

            var gameMatches = FindLoadedStS2Assemblies();
            if (gameMatches.Length != 1 || !ReferenceEquals(gameMatches[0], assembly))
                throw new InvalidDataException($"Expected exactly one sts2 assembly after Step-33 Gate B, found {gameMatches.Length}.");
            var contextAssemblies = context.Assemblies.ToArray();
            if (contextAssemblies.Length != 1 || !ReferenceEquals(contextAssemblies[0], assembly))
                throw new InvalidDataException($"Step-33 admission-only context contains {contextAssemblies.Length} private assemblies instead of exactly the transformed primary.");

            _primaryAdmission = new PrimaryAdmissionSnapshot(
                assembly,
                actualIdentity,
                actualMvid,
                immediateSha256,
                context.ManagedResolverRequests.Count,
                context.HostLoads.Count,
                context.PrivateDependencyRequests.Count,
                context.RejectedManagedRequests.Count,
                context.NativeLoadAttempts.Count);

            return Pass(gate,
                "FIRST CLR ADMISSION OF THE VERIFIED TRANSFORMED REAL-STS2 IMAGE SUCCEEDED. THE BOUNDARY STOPS AFTER IMAGE/CONTEXT IDENTITY VERIFICATION.\n" +
                $"Loaded identity: {actualIdentity}\n" +
                $"Loaded MVID: {actualMvid}\n" +
                $"AssemblyLoadContext: {context.Name ?? LoadContextName}\n" +
                $"Exact transformed SHA-256 immediately before LoadFromStream: {immediateSha256}\n" +
                $"Managed resolver requests during transformed primary admission: {context.ManagedResolverRequests.Count:N0}\n" +
                $"Host framework bindings serviced during admission: {context.HostLoads.Count:N0}\n" +
                $"Private dependency requests during admission: {context.PrivateDependencyRequests.Count:N0}\n" +
                $"Rejected managed requests during admission: {context.RejectedManagedRequests.Count:N0}\n" +
                $"Native load attempts during admission: {context.NativeLoadAttempts.Count:N0}\n" +
                "Original receipt-backed/prepared sts2.dll used as CLR load input: NO\n" +
                "Game entry point invoked: NO\n" +
                "Game type/member reflection performed: NO\n" +
                "Game method/delegate invoked: NO\n" +
                "Godot/game initialization requested: NO");
        }
        catch (Exception ex)
        {
            if (_collectibleLoadContext)
                ReleaseLoadContext();
            _primaryAdmission = null;
            return Fail(gate, stage, ex);
        }
    }

    public TransformedRealStS2AssemblyAdmissionGateResult RunAdmissionOnlyResolverAudit()
    {
        const TransformedRealStS2AssemblyAdmissionGate gate = TransformedRealStS2AssemblyAdmissionGate.AdmissionOnlyResolverAudit;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            RequirePreflight();
            var admission = RequirePrimaryAdmission();
            var context = RequireLoadContext();

            stage = "admission-only private-context membership";
            var privateAssemblies = context.Assemblies.ToArray();
            if (privateAssemblies.Length != 1 || !ReferenceEquals(privateAssemblies[0], admission.Assembly))
                throw new InvalidDataException("Step-33 private context expanded beyond the transformed primary during admission-only verification.");
            if (context.PrivateDependencyRequests.Count != 0)
                throw new InvalidDataException("Step-33 transformed primary admission attempted to resolve a private game dependency: " + string.Join(" | ", context.PrivateDependencyRequests));
            if (context.RejectedManagedRequests.Count != 0)
                throw new InvalidDataException("Step-33 strict resolver rejected an unplanned managed request: " + string.Join(" | ", context.RejectedManagedRequests));
            if (context.NativeLoadAttempts.Count != 0)
                throw new InvalidDataException("Step-33 transformed primary admission attempted native resolution: " + string.Join(" | ", context.NativeLoadAttempts));

            stage = "transformed-primary residency audit";
            var gameMatches = FindLoadedStS2Assemblies();
            if (gameMatches.Length != 1 || !ReferenceEquals(gameMatches[0], admission.Assembly))
                throw new InvalidDataException("The transformed primary is not the unique sts2 assembly resident after Step-33 Gate B.");

            _resolverAudit = new ResolverAuditSnapshot(
                context.ManagedResolverRequests.ToArray(),
                context.HostLoads.ToArray(),
                context.PrivateDependencyRequests.ToArray(),
                context.RejectedManagedRequests.ToArray(),
                context.NativeLoadAttempts.ToArray());

            return Pass(gate,
                "ADMISSION-ONLY RESOLVER/CONTEXT AUDIT PASSED. ONLY THE TRANSFORMED PRIMARY ENTERED THE PRIVATE CLR CONTEXT.\n" +
                $"Private context assemblies: {privateAssemblies.Length:N0} (transformed sts2 only)\n" +
                $"Managed resolver requests: {context.ManagedResolverRequests.Count:N0}\n" +
                $"Exact planned host-framework bindings serviced: {context.HostLoads.Count:N0}\n" +
                "Private game dependency loads: 0\n" +
                "Private game dependency requests: 0\n" +
                "Unplanned managed resolver requests: 0\n" +
                "Native resolution attempts: 0\n" +
                "Receipt-backed/prepared original sts2.dll CLR admission: NO\n" +
                "Game member reflection/invocation: NO");
        }
        catch (Exception ex)
        {
            _resolverAudit = null;
            return Fail(gate, stage, ex);
        }
    }

    public async Task<TransformedRealStS2AssemblyAdmissionGateResult> RunFinalIsolationAuditAsync(
        IProgress<TransformedRealStS2AssemblyAdmissionProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        const TransformedRealStS2AssemblyAdmissionGate gate = TransformedRealStS2AssemblyAdmissionGate.FinalIsolationAudit;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var preflight = RequirePreflight();
            var admission = RequirePrimaryAdmission();
            RequireResolverAudit();
            var context = RequireLoadContext();
            cancellationToken.ThrowIfCancellationRequested();

            stage = "post-admission OfflineReady";
            progress?.Report(new(gate, 0, 2, null, "Re-proving the receipt-backed install after transformed-image CLR admission."));
            var offline = await _offlineInspection.RunAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            if (offline.Outcome == SteamOfflineInstallOutcome.Cancelled)
                throw new OperationCanceledException("Step 33 final OfflineReady audit was cancelled.", cancellationToken);
            if (!offline.Success || string.IsNullOrWhiteSpace(offline.ManagedInstallRelativePath))
                throw new InvalidDataException(offline.Error ?? "Step-33 final OfflineReady re-verification failed.");

            var managedRoot = ResolveChildPath(_launcherDataRoot, offline.ManagedInstallRelativePath, "Step-33 managed install");
            var trustedPrimaryPath = ResolveChildPath(managedRoot, ExactPrimaryRelativePath, "Step-33 trusted primary path");
            var trustedSha256 = ComputeSha256Hex(trustedPrimaryPath);
            if (!trustedSha256.Equals(ClosedStep32SourceSha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException($"Receipt-backed original sts2.dll changed after transformed admission: {trustedSha256}.");

            stage = "post-admission transformed/plan/context reproof";
            var transformedSha256 = ComputeSha256Hex(preflight.TransformedPath);
            if (!transformedSha256.Equals(ClosedStep32TransformedSha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Verified transformed sts2.dll changed after CLR admission.");
            var planSha256 = ComputeSha256Hex(_planPath);
            if (!planSha256.Equals(preflight.PlanSha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Prepared runtime-binding plan changed during Step-33 admission.");
            if (context.PrivateDependencyRequests.Count != 0 || context.RejectedManagedRequests.Count != 0 || context.NativeLoadAttempts.Count != 0)
                throw new InvalidDataException("Step-33 resolver/native isolation counters changed after Gate C.");
            var gameMatches = FindLoadedStS2Assemblies();
            if (gameMatches.Length != 1 || !ReferenceEquals(gameMatches[0], admission.Assembly) || !ReferenceEquals(AssemblyLoadContext.GetLoadContext(admission.Assembly), context))
                throw new InvalidDataException("Step-33 transformed-primary CLR residency/context ownership drifted during the final audit.");
            progress?.Report(new(gate, 2, 2, preflight.TransformedPath, "Final source/transformed/plan/context isolation checks passed."));

            return Pass(gate,
                "STEP 33 FINAL TRANSFORMED-REAL-STS2 CLR-ADMISSION ISOLATION AUDIT PASSED.\n" +
                $"Post-admission OfflineReady: PASS ({offline.VerifiedFiles:N0}/{offline.PlannedFiles:N0} files)\n" +
                $"Receipt-backed original SHA-256 unchanged: {trustedSha256}\n" +
                $"Verified transformed SHA-256 unchanged: {transformedSha256}\n" +
                $"Runtime-binding plan SHA-256 unchanged: {planSha256}\n" +
                $"Unique resident sts2 assembly identity: {admission.AssemblyFullName}\n" +
                $"Resident sts2 AssemblyLoadContext: {context.Name ?? LoadContextName}\n" +
                "Resident sts2 load input: exact Step-32 transformed image\n" +
                "Receipt-backed/prepared original sts2.dll CLR-loaded by Step 33: NO\n" +
                "Private game dependencies CLR-loaded by Step 33: NO\n" +
                "Unplanned managed resolution: NO\n" +
                "Native game resolution/loading: NO\n" +
                "Game entry point/type/member invocation: NO\n" +
                "Godot/game startup: NO\n" +
                "Authorization after Step-33 PASS: a later separately gated boundary may prepare for and invoke the exact transformed compatibility site; Step 33 itself remains admission-only.");
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

    private void ValidateAdmissionPlan(RuntimeFrameworkBindingPlanDocument plan)
    {
        if (plan.SchemaVersion != RuntimeFrameworkBindingPlanDocument.CurrentSchemaVersion)
            throw new InvalidDataException($"Unsupported Step-21/22 runtime-binding plan schema {plan.SchemaVersion}.");
        if (!plan.RuntimeClosureReady || plan.Blockers.Length != 0 || plan.Edges.Any(edge => edge.BindingKind.StartsWith("Blocker:", StringComparison.Ordinal)))
            throw new InvalidDataException("Step 33 requires the physically established zero-blocker Step-21/22 runtime-binding plan.");
        var primary = plan.PreparedAssemblies.Where(item => item.IsPrimary).ToArray();
        if (primary.Length != 1)
            throw new InvalidDataException($"Step 33 expected exactly one prepared primary, found {primary.Length}.");
        if (!primary[0].AssemblyFullName.Equals(ClosedStep32AssemblyIdentity, StringComparison.Ordinal) ||
            !plan.PrimaryAssemblyFullName.Equals(ClosedStep32AssemblyIdentity, StringComparison.Ordinal))
            throw new InvalidDataException("Step-33 runtime-binding plan primary identity differs from the physically closed Step-32 assembly identity.");
        if (!primary[0].RelativePath.Equals(plan.PrimaryAssemblyRelativePath, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Step-33 runtime-binding plan primary path/entry disagree.");
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
            throw new InvalidDataException("Step-33 prepared dependency metadata inspection unexpectedly requested assembly resolution.");
        return count;
    }

    private static void RequireRewritePass(string label, RealStS2PrepareMethodRewriteGateResult result)
    {
        if (!result.Passed)
            throw new InvalidDataException($"{label} failed while Step 33 requalified the closed Step-32 transformation: {result.Detail.Replace('\n', ' ')}");
    }

    private void EnsureNoStS2Loaded(string stage)
    {
        var matches = FindLoadedStS2Assemblies();
        if (matches.Length == 0)
            return;
        var detail = string.Join(" | ", matches.Select(assembly =>
            $"{assembly.GetName().FullName} @ {AssemblyLoadContext.GetLoadContext(assembly)?.Name ?? "<unknown-context>"}"));
        throw new InvalidDataException($"Step 33 requires a fresh process at {stage}; sts2 is already CLR-resident: {detail}");
    }

    private static Assembly[] FindLoadedStS2Assemblies()
        => AppDomain.CurrentDomain.GetAssemblies()
            .Where(assembly => string.Equals(assembly.GetName().Name, ExpectedPrimarySimpleName, StringComparison.OrdinalIgnoreCase))
            .ToArray();

    private void ReleaseLoadContext()
    {
        var context = _loadContext;
        _loadContext = null;
        _primaryAdmission = null;
        _resolverAudit = null;
        if (context is not null && context.IsCollectible)
            context.Unload();
    }

    private AdmissionPreflightSnapshot RequirePreflight()
        => _preflight ?? throw new InvalidOperationException("Step 33 Gate A must pass before Gate B.");

    private PrimaryAdmissionSnapshot RequirePrimaryAdmission()
        => _primaryAdmission ?? throw new InvalidOperationException("Step 33 Gate B must pass before Gate C.");

    private ResolverAuditSnapshot RequireResolverAudit()
        => _resolverAudit ?? throw new InvalidOperationException("Step 33 Gate C must pass before Gate D.");

    private Step33AdmissionLoadContext RequireLoadContext()
        => _loadContext ?? throw new InvalidOperationException("Step 33 dedicated AssemblyLoadContext is unavailable.");

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(TransformedRealStS2AssemblyAdmission));
    }

    private static string ComputeSha256Hex(string path)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }

    private static void VerifyFileLength(string path, long expected, string scope)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"Step 33 {scope} is missing.", path);
        var actual = new FileInfo(path).Length;
        if (actual != expected)
            throw new InvalidDataException($"Step 33 {scope} length mismatch: {actual} != {expected}.");
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

    private static TransformedRealStS2AssemblyAdmissionGateResult Pass(TransformedRealStS2AssemblyAdmissionGate gate, string detail)
        => new(gate, true, detail);

    private static TransformedRealStS2AssemblyAdmissionGateResult Fail(TransformedRealStS2AssemblyAdmissionGate gate, string stage, Exception ex)
        => new(gate, false, $"Stage: {stage}\n{ex.GetType().Name}: {ex.Message}");

    private sealed record AdmissionPreflightSnapshot(
        string TransformedPath,
        string TransformedSha256,
        RuntimeFrameworkBindingPlanDocument Plan,
        string PlanSha256,
        PreparedAdmissionEntry[] PreparedAssemblies,
        PreparedAdmissionEntry PreparedPrimary);

    internal sealed record PreparedAdmissionEntry(
        RuntimeBindingPreparedAssembly Plan,
        string PreparedPath,
        AssemblyName AssemblyName,
        int ModuleInitializerCount);

    private sealed record PrimaryAdmissionSnapshot(
        Assembly Assembly,
        string AssemblyFullName,
        Guid Mvid,
        string ImmediateSha256,
        int ManagedResolverRequestsAtLoad,
        int HostLoadsAtLoad,
        int PrivateDependencyRequestsAtLoad,
        int RejectedManagedRequestsAtLoad,
        int NativeLoadAttemptsAtLoad);

    private sealed record ResolverAuditSnapshot(
        IReadOnlyList<string> ManagedResolverRequests,
        IReadOnlyList<string> HostLoads,
        IReadOnlyList<string> PrivateDependencyRequests,
        IReadOnlyList<string> RejectedManagedRequests,
        IReadOnlyList<string> NativeLoadAttempts);

    internal sealed class Step33AdmissionLoadContext : AssemblyLoadContext
    {
        private readonly IReadOnlyDictionary<string, PreparedAdmissionEntry> _privateBySimpleName;
        private readonly RuntimeBindingHostFramework[] _hostBindings;

        internal Step33AdmissionLoadContext(
            string name,
            RuntimeFrameworkBindingPlanDocument plan,
            IReadOnlyList<PreparedAdmissionEntry> preparedAssemblies,
            bool isCollectible)
            : base(name, isCollectible)
        {
            var privateBySimpleName = new Dictionary<string, PreparedAdmissionEntry>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in preparedAssemblies.Where(item => !item.Plan.IsPrimary))
            {
                var simple = item.AssemblyName.Name ?? throw new InvalidDataException($"Prepared assembly identity has no simple name: {item.Plan.AssemblyFullName}");
                if (IsHostFrameworkContractName(simple))
                    throw new InvalidDataException($"Step-33 resolver received framework-shaped private assembly '{simple}'.");
                if (!privateBySimpleName.TryAdd(simple, item))
                    throw new InvalidDataException($"Step-33 resolver received duplicate prepared simple name '{simple}'.");
            }
            _privateBySimpleName = privateBySimpleName;
            _hostBindings = plan.HostFrameworkBindings;
        }

        internal List<string> ManagedResolverRequests { get; } = [];
        internal List<string> HostLoads { get; } = [];
        internal List<string> PrivateDependencyRequests { get; } = [];
        internal List<string> RejectedManagedRequests { get; } = [];
        internal List<string> NativeLoadAttempts { get; } = [];

        [UnconditionalSuppressMessage(
            "Trimming",
            "IL2026",
            Justification = "Step 33 admits only the exact hash-pinned transformed primary image and performs no game member reflection/invocation.")]
        internal Assembly LoadPrimary(string transformedPath, string expectedSha256)
        {
            var actualSha256 = ComputeSha256Hex(transformedPath);
            if (!actualSha256.Equals(expectedSha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Step-33 transformed primary hash changed immediately before LoadFromStream.");
            using var stream = new FileStream(transformedPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return LoadFromStream(stream);
        }

        protected override Assembly? Load(AssemblyName assemblyName)
        {
            var requestedFullName = assemblyName.FullName ?? assemblyName.Name ?? "<unknown>";
            ManagedResolverRequests.Add(requestedFullName);
            if (assemblyName.Name is null)
                return Reject(requestedFullName, "assembly request has no simple name");

            if (_privateBySimpleName.TryGetValue(assemblyName.Name, out var privateAssembly))
            {
                var detail = $"{requestedFullName} => {privateAssembly.Plan.AssemblyFullName}; moduleInitializers={privateAssembly.ModuleInitializerCount}";
                PrivateDependencyRequests.Add(detail);
                throw new FileLoadException(
                    "Step 33 is transformed-primary admission only and refuses private dependency CLR admission: " + detail);
            }

            var hostMatches = _hostBindings
                .Where(binding => ExactRequestedIdentity(assemblyName, new AssemblyName(binding.RequestedFullName)))
                .ToArray();
            if (hostMatches.Length == 0)
                return Reject(requestedFullName, "request is neither an exact planned host-framework binding nor an identified private prepared dependency");

            var allowedActual = hostMatches
                .Select(binding => binding.ActualFullName)
                .Distinct(StringComparer.Ordinal)
                .ToHashSet(StringComparer.Ordinal);
            var hostAssembly = AssemblyLoadContext.Default.LoadFromAssemblyName(assemblyName);
            var actualFullName = hostAssembly.GetName().FullName ?? hostAssembly.GetName().Name ?? string.Empty;
            if (!allowedActual.Contains(actualFullName))
                throw new FileLoadException(
                    $"Step-33 host binding drift for '{requestedFullName}'. Planned actual identity: {string.Join(" | ", allowedActual)}; runtime actual: {actualFullName}.");
            HostLoads.Add($"{requestedFullName} => {actualFullName}");
            return hostAssembly;
        }

        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            NativeLoadAttempts.Add(unmanagedDllName);
            throw new DllNotFoundException(
                $"Step 33 admission-only boundary refuses native library resolution for '{unmanagedDllName}'.");
        }

        private Assembly? Reject(string requestedFullName, string reason)
        {
            var detail = $"{requestedFullName} — {reason}";
            RejectedManagedRequests.Add(detail);
            throw new FileLoadException("Step-33 strict managed resolver rejected an unplanned request: " + detail);
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
