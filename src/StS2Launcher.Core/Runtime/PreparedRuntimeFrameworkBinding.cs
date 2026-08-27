using System.Reflection;
using System.Runtime.Loader;
using System.Security.Cryptography;
using System.Text.Json;
using Mono.Cecil;

namespace StS2Launcher.Core;

/// <summary>
/// Step 21 boundary. Builds an execution-oriented managed dependency plan for the real receipt-backed
/// ARM64 StS2 payload without loading any game assembly into the CLR. Desktop runtime/framework
/// implementation assemblies are treated as diagnostic inputs only when the iOS host can satisfy the
/// corresponding framework contract. Reachable private/game assemblies are copied byte-for-byte into
/// a launcher-private prepared set. Missing, ambiguous, version-incompatible, and non-IL-only edges are
/// preserved as explicit blockers instead of being hidden by broad resolver fallback.
/// </summary>
public sealed class PreparedRuntimeFrameworkBinding
{
    public const string WorkRootName = "Step21-PreparedRuntimeBinding";
    public const string SourceRootName = "source";
    public const string PreparedRootName = "prepared";
    public const string PlanRootName = "plan";
    public const string PlanFileName = "runtime-binding-plan.json";

    private readonly string _launcherDataRoot;
    private readonly string _workRoot;
    private readonly SteamOfflineInstallInspection _offlineInspection;
    private WorkspaceSnapshot? _workspace;
    private BindingSnapshot? _binding;
    private PreparedSnapshot? _prepared;

    public PreparedRuntimeFrameworkBinding(string launcherDataRoot)
    {
        if (string.IsNullOrWhiteSpace(launcherDataRoot))
            throw new ArgumentException("Launcher data root is required.", nameof(launcherDataRoot));

        _launcherDataRoot = Path.GetFullPath(launcherDataRoot);
        _workRoot = Path.Combine(_launcherDataRoot, WorkRootName);
        _offlineInspection = new SteamOfflineInstallInspection(_launcherDataRoot);
    }

    public void Reset()
    {
        _workspace = null;
        _binding = null;
        _prepared = null;
    }

    public async Task<RuntimeFrameworkBindingGateResult> RunRuntimePayloadClassificationAsync(
        IProgress<RuntimeFrameworkBindingProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Reset();
            EnsureNoStS2AssemblyLoaded();
            cancellationToken.ThrowIfCancellationRequested();

            progress?.Report(new RuntimeFrameworkBindingProgress(
                RuntimeFrameworkBindingGate.RuntimePayloadClassification,
                0,
                0,
                null,
                "Re-proving OfflineReady before creating the receipt-backed Step 21 ARM64/shared managed workspace…"));

            IProgress<SteamOfflineInstallProgress>? offlineProgress = progress is null
                ? null
                : new CallbackProgress<SteamOfflineInstallProgress>(value =>
                    progress.Report(new RuntimeFrameworkBindingProgress(
                        RuntimeFrameworkBindingGate.RuntimePayloadClassification,
                        value.CompletedFiles,
                        value.TotalFiles,
                        value.CurrentFile,
                        $"OfflineReady precondition — {value.Message} ({value.CompletedBytes:N0}/{value.TotalBytes:N0} bytes)")));

            var offline = await _offlineInspection.RunAsync(offlineProgress, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            if (offline.Outcome == SteamOfflineInstallOutcome.Cancelled)
                throw new OperationCanceledException("Step 21 OfflineReady precondition was cancelled.", cancellationToken);
            if (!offline.Success || string.IsNullOrWhiteSpace(offline.ManagedInstallRelativePath))
                throw new InvalidDataException(offline.Error ?? $"Managed install is not OfflineReady ({offline.State}/{offline.Outcome}).");

            var managedRoot = ResolveChildPath(_launcherDataRoot, offline.ManagedInstallRelativePath);
            var receipt = await ReadReceiptAsync(managedRoot, cancellationToken).ConfigureAwait(false);
            ValidateReceiptSnapshot(receipt, offline);

            var allManagedNames = receipt.Files
                .Where(file => IsManagedAssemblyFileName(file.RelativePath))
                .OrderBy(file => file.RelativePath, StringComparer.OrdinalIgnoreCase)
                .ToArray();
            var arm64 = allManagedNames.Where(file => IsMacOsArm64ManagedPath(file.RelativePath)).ToArray();
            var x86 = allManagedNames.Where(file => IsMacOsX8664ManagedPath(file.RelativePath)).ToArray();
            var shared = allManagedNames
                .Where(file => !IsMacOsArm64ManagedPath(file.RelativePath) && !IsMacOsX8664ManagedPath(file.RelativePath))
                .ToArray();
            if (arm64.Length == 0)
                throw new InvalidDataException("No receipt-backed data_sts2_macos_arm64 managed filename candidates were found.");

            var selected = arm64
                .Concat(shared)
                .OrderBy(file => file.RelativePath, StringComparer.OrdinalIgnoreCase)
                .ToArray();
            var primaryMatches = arm64.Where(file => IsPrimaryArm64StS2Path(file.RelativePath)).ToArray();
            if (primaryMatches.Length != 1)
                throw new InvalidDataException($"Expected exactly one macOS ARM64 sts2.dll receipt entry, found {primaryMatches.Length}.");

            PrepareFreshWorkRoot();
            var sourceRoot = Path.Combine(_workRoot, SourceRootName);
            Directory.CreateDirectory(sourceRoot);

            var catalog = new List<WorkspaceAssemblyCandidate>();
            var nonManagedCandidates = 0;
            var ilOnlyCount = 0;
            var nonIlOnlyCount = 0;
            ulong selectedBytes = 0;

            for (var index = 0; index < selected.Length; index++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var receiptFile = selected[index];
                var relative = NormalizeRelative(receiptFile.RelativePath);
                progress?.Report(new RuntimeFrameworkBindingProgress(
                    RuntimeFrameworkBindingGate.RuntimePayloadClassification,
                    index,
                    selected.Length,
                    relative,
                    "Copying and SHA-1 verifying receipt-backed ARM64/shared managed filename candidates, then probing managed metadata without dependency resolution…"));

                var livePath = ResolveChildPath(managedRoot, relative);
                var sourcePath = ResolveChildPath(sourceRoot, relative);
                Directory.CreateDirectory(Path.GetDirectoryName(sourcePath)!);
                await CopyFileAsync(livePath, sourcePath, cancellationToken).ConfigureAwait(false);

                var info = new FileInfo(sourcePath);
                if (info.Length != receiptFile.Length)
                    throw new InvalidDataException($"Step 21 source-copy length mismatch for {relative}: {info.Length} != {receiptFile.Length}.");
                var sourceSha1 = await ComputeSha1HexAsync(sourcePath, cancellationToken).ConfigureAwait(false);
                if (!sourceSha1.Equals(receiptFile.Sha1Hex, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException($"Step 21 source-copy SHA-1 mismatch for {relative}.");
                checked { selectedBytes += (ulong)receiptFile.Length; }

                if (!TryReadManagedCandidate(sourcePath, relative, receiptFile, out var candidate))
                {
                    nonManagedCandidates++;
                    continue;
                }

                catalog.Add(candidate!);
                if (candidate!.IsIlOnly) ilOnlyCount++; else nonIlOnlyCount++;
            }

            var primaryRelative = NormalizeRelative(primaryMatches[0].RelativePath);
            var primaryCandidates = catalog.Where(candidate =>
                candidate.RelativePath.Equals(primaryRelative, StringComparison.OrdinalIgnoreCase)).ToArray();
            if (primaryCandidates.Length != 1)
                throw new InvalidDataException("The receipt-backed primary ARM64 sts2.dll did not probe as exactly one managed assembly.");

            var primary = primaryCandidates[0];
            if (!primary.IsIlOnly)
                throw new InvalidDataException("Primary ARM64 sts2.dll is not IL-only; Step 21 cannot prepare it for the interpreter-backed managed runtime path.");

            _workspace = new WorkspaceSnapshot(
                managedRoot,
                NormalizeRelative(offline.ManagedInstallRelativePath),
                sourceRoot,
                receipt,
                offline,
                selected,
                catalog.OrderBy(candidate => candidate.RelativePath, StringComparer.OrdinalIgnoreCase).ToArray(),
                primary,
                allManagedNames.Length,
                arm64.Length,
                x86.Length,
                shared.Length,
                nonManagedCandidates,
                selectedBytes);

            EnsureNoStS2AssemblyLoaded();
            progress?.Report(new RuntimeFrameworkBindingProgress(
                RuntimeFrameworkBindingGate.RuntimePayloadClassification,
                selected.Length,
                selected.Length,
                primaryRelative,
                "Receipt-backed real managed scope classified. No game assembly has entered the CLR."));

            return Pass(
                RuntimeFrameworkBindingGate.RuntimePayloadClassification,
                "Real ARM64/shared managed runtime-input workspace established and classified without CLR-loading StS2.\n" +
                $"OfflineReady precondition: YES ({offline.VerifiedFiles:N0}/{offline.PlannedFiles:N0} files)\n" +
                $"All depot .dll/.exe filename candidates: {allManagedNames.Length:N0}\n" +
                $"macOS ARM64 candidates copied: {arm64.Length:N0}\n" +
                $"Architecture-neutral candidates copied: {shared.Length:N0}\n" +
                $"macOS x86_64 duplicates excluded: {x86.Length:N0}\n" +
                $"Selected source workspace: {selected.Length:N0} files / {selectedBytes:N0} bytes\n" +
                $"Managed assemblies successfully cataloged: {catalog.Count:N0}\n" +
                $"IL-only managed assemblies: {ilOnlyCount:N0}\n" +
                $"Non-IL-only/ReadyToRun-or-mixed-mode managed assemblies: {nonIlOnlyCount:N0}\n" +
                $"Non-managed .dll/.exe filename candidates: {nonManagedCandidates:N0}\n" +
                $"Primary assembly: {primary.RelativePath}\n" +
                $"Primary identity: {primary.FullName}\n" +
                "Every Step 21 source copy receipt SHA-1 verified: YES\n" +
                "Assembly dependency resolution attempted by Cecil: NO\n" +
                "StS2 assembly loaded/executed: NO\n" +
                "Steam session consulted: NO\nNetwork attempted by Step 21: NO\nReal managed install modified: NO");
        }
        catch (OperationCanceledException)
        {
            BestEffortDeleteWorkRoot();
            Reset();
            throw;
        }
        catch (Exception ex)
        {
            BestEffortDeleteWorkRoot();
            Reset();
            return Fail(RuntimeFrameworkBindingGate.RuntimePayloadClassification, "runtime payload classification", ex);
        }
    }

    public RuntimeFrameworkBindingGateResult RunHostFrameworkBindingPlan()
    {
        var stage = "initialization";
        try
        {
            stage = "workspace precondition";
            var workspace = RequireWorkspace();
            EnsureNoStS2AssemblyLoaded();

            var edges = new List<RuntimeBindingEdge>();
            var blockers = new List<RuntimeBindingBlocker>();
            var hostObservations = new Dictionary<string, HostBindingAccumulator>(StringComparer.Ordinal);
            var reachable = new Dictionary<string, WorkspaceAssemblyCandidate>(StringComparer.OrdinalIgnoreCase)
            {
                [workspace.Primary.RelativePath] = workspace.Primary,
            };
            var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var queue = new Queue<WorkspaceAssemblyCandidate>();
            queue.Enqueue(workspace.Primary);

            while (queue.Count > 0)
            {
                var source = queue.Dequeue();
                if (!visited.Add(source.RelativePath))
                    continue;

                stage = $"dependency graph scan: {source.RelativePath}";
                foreach (var reference in source.References.OrderBy(reference => reference.FullName, StringComparer.Ordinal))
                {
                    if (TryBindHostFramework(reference, out var hostBinding, out var hostFailure))
                    {
                        var hostKey = reference.FullName + " => " + hostBinding!.ActualFullName;
                        if (!hostObservations.TryGetValue(hostKey, out var accumulator))
                        {
                            accumulator = new HostBindingAccumulator(reference.FullName, hostBinding.ActualFullName, hostBinding.ActualLocation);
                            hostObservations.Add(hostKey, accumulator);
                        }
                        accumulator.ReferenceCount++;
                        edges.Add(new RuntimeBindingEdge(source.FullName, reference.FullName, "HostFramework", hostBinding.ActualFullName));
                        continue;
                    }

                    stage = $"workspace identity resolution: {reference.FullName}";
                    var workspaceResolution = ResolveWorkspaceReference(workspace, reference);
                    if (workspaceResolution.Candidate is not null)
                    {
                        var target = workspaceResolution.Candidate;
                        if (!target.IsIlOnly)
                        {
                            blockers.Add(new RuntimeBindingBlocker(
                                "NonIlOnlyWorkspaceAssembly",
                                source.FullName,
                                reference.FullName,
                                $"Resolved only to non-IL-only desktop image '{target.FullName}' at {target.RelativePath}. This image is not eligible for the interpreter-backed prepared runtime set."));
                            edges.Add(new RuntimeBindingEdge(source.FullName, reference.FullName, "Blocker:NonIlOnlyWorkspaceAssembly", target.RelativePath));
                            continue;
                        }

                        reachable[target.RelativePath] = target;
                        if (!visited.Contains(target.RelativePath))
                            queue.Enqueue(target);
                        var kind = workspaceResolution.VersionUnified ? "WorkspaceVersionUnified" : "WorkspaceExact";
                        edges.Add(new RuntimeBindingEdge(source.FullName, reference.FullName, kind, target.FullName));
                        continue;
                    }

                    var blockerKind = IsHostFrameworkProbeCandidate(reference.Name)
                        ? "HostFrameworkUnavailable"
                        : workspaceResolution.BlockerKind;
                    var detail = IsHostFrameworkProbeCandidate(reference.Name)
                        ? $"The iOS host did not bind this framework-shaped reference ({hostFailure ?? "no host binding"}), and the verified Step 21 workspace did not provide a usable private IL fallback. {workspaceResolution.Detail}"
                        : workspaceResolution.Detail;
                    blockers.Add(new RuntimeBindingBlocker(blockerKind, source.FullName, reference.FullName, detail));
                    edges.Add(new RuntimeBindingEdge(source.FullName, reference.FullName, $"Blocker:{blockerKind}", detail));
                }
            }

            var hostBoundSimpleNames = hostObservations.Values
                .Select(observation => GetSimpleName(observation.ActualFullName))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            foreach (var candidate in reachable.Values.ToArray())
            {
                if (candidate.RelativePath.Equals(workspace.Primary.RelativePath, StringComparison.OrdinalIgnoreCase))
                    continue;
                if (!hostBoundSimpleNames.Contains(candidate.Name))
                    continue;

                blockers.Add(new RuntimeBindingBlocker(
                    "HostPrivateSimpleNameConflict",
                    workspace.Primary.FullName,
                    candidate.FullName,
                    $"Assembly simple name '{candidate.Name}' is both host-bound and reachable as a private workspace assembly. Step 21 refuses a split type-identity plan."));
                reachable.Remove(candidate.RelativePath);
            }

            var hostBindings = hostObservations.Values
                .OrderBy(item => item.RequestedFullName, StringComparer.Ordinal)
                .ThenBy(item => item.ActualFullName, StringComparer.Ordinal)
                .Select(item => new RuntimeBindingHostFramework(
                    item.RequestedFullName,
                    item.ActualFullName,
                    item.ActualLocation,
                    item.ReferenceCount))
                .ToArray();

            var preparedAssemblies = reachable.Values
                .OrderBy(candidate => candidate.RelativePath, StringComparer.OrdinalIgnoreCase)
                .Select(candidate => new RuntimeBindingPreparedAssembly(
                    candidate.RelativePath,
                    candidate.FullName,
                    candidate.ExpectedSha1,
                    candidate.Length,
                    candidate.RelativePath.Equals(workspace.Primary.RelativePath, StringComparison.OrdinalIgnoreCase)))
                .ToArray();

            var blockerArray = blockers
                .OrderBy(blocker => blocker.SourceAssemblyFullName, StringComparer.Ordinal)
                .ThenBy(blocker => blocker.RequestedFullName, StringComparer.Ordinal)
                .ThenBy(blocker => blocker.Kind, StringComparer.Ordinal)
                .ToArray();
            var edgeArray = edges
                .OrderBy(edge => edge.SourceAssemblyFullName, StringComparer.Ordinal)
                .ThenBy(edge => edge.RequestedFullName, StringComparer.Ordinal)
                .ThenBy(edge => edge.BindingKind, StringComparer.Ordinal)
                .ToArray();

            var plan = new RuntimeFrameworkBindingPlanDocument(
                RuntimeFrameworkBindingPlanDocument.CurrentSchemaVersion,
                workspace.Receipt.AppId,
                workspace.Receipt.DepotId,
                workspace.Receipt.ManifestId,
                workspace.Receipt.Branch,
                workspace.ManagedInstallRelativePath,
                workspace.Primary.RelativePath,
                workspace.Primary.FullName,
                preparedAssemblies,
                hostBindings,
                blockerArray,
                edgeArray,
                blockerArray.Length == 0);

            _binding = new BindingSnapshot(plan);
            EnsureNoStS2AssemblyLoaded();

            var hostSamples = hostBindings.Length == 0
                ? "  (none)"
                : string.Join("\n", hostBindings.Take(12).Select(binding => $"  {binding.RequestedFullName} -> {binding.ActualFullName}"));
            var blockerSamples = blockerArray.Length == 0
                ? "  (none)"
                : string.Join("\n", blockerArray.Take(14).Select(blocker => $"  [{blocker.Kind}] {blocker.RequestedFullName} <- {blocker.SourceAssemblyFullName}"));

            return Pass(
                RuntimeFrameworkBindingGate.HostFrameworkBindingPlan,
                "Real sts2.dll dependency graph classified into host-framework, verified-private, and explicit-blocker edges without CLR-loading any game assembly.\n" +
                $"Reachable IL-only private/game assemblies selected for prepared set: {preparedAssemblies.Length:N0}\n" +
                $"Host framework binding identities observed: {hostBindings.Length:N0}\n" +
                $"Dependency graph edges classified: {edgeArray.Length:N0}\n" +
                $"Explicit binding blockers: {blockerArray.Length:N0}\n" +
                $"Runtime closure ready for first real CLR load: {(plan.RuntimeClosureReady ? "YES" : "NO")}\n" +
                "Copied desktop System.* implementations preferred over a working iOS-host binding: NO\n" +
                "Ambiguous/missing/private/non-IL-only dependencies silently guessed: NO\n" +
                "Host framework mapping sample:\n" + hostSamples + "\n" +
                "Binding blocker sample:\n" + blockerSamples + "\n" +
                "Step 21 interpretation rule: blockers are first-class plan output, not a Gate B failure. A 4/4 Step 21 proves an authoritative plan; Runtime closure ready=YES is a separate readiness signal.\n" +
                "StS2 assembly loaded/executed: NO\nReal managed install modified: NO");
        }
        catch (Exception ex)
        {
            return Fail(RuntimeFrameworkBindingGate.HostFrameworkBindingPlan, stage, ex);
        }
    }

    public async Task<RuntimeFrameworkBindingGateResult> RunPreparedRuntimeAssemblySetAsync(
        IProgress<RuntimeFrameworkBindingProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var stage = "initialization";
        try
        {
            stage = "workspace/binding precondition";
            var workspace = RequireWorkspace();
            var binding = RequireBinding();
            EnsureNoStS2AssemblyLoaded();

            var preparedRoot = Path.Combine(_workRoot, PreparedRootName);
            var planRoot = Path.Combine(_workRoot, PlanRootName);
            BestEffortDeleteDirectory(preparedRoot);
            BestEffortDeleteDirectory(planRoot);
            Directory.CreateDirectory(preparedRoot);
            Directory.CreateDirectory(planRoot);

            var preparedFiles = new List<PreparedFileSnapshot>();
            for (var index = 0; index < binding.Plan.PreparedAssemblies.Length; index++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var planned = binding.Plan.PreparedAssemblies[index];
                progress?.Report(new RuntimeFrameworkBindingProgress(
                    RuntimeFrameworkBindingGate.PreparedRuntimeAssemblySet,
                    index,
                    binding.Plan.PreparedAssemblies.Length,
                    planned.RelativePath,
                    "Building execution-oriented prepared set by byte-copy only; receipt-backed source SHA-1 is rechecked immediately before copy…"));

                var sourceCandidate = workspace.Catalog.SingleOrDefault(candidate =>
                    candidate.RelativePath.Equals(planned.RelativePath, StringComparison.OrdinalIgnoreCase))
                    ?? throw new InvalidDataException($"Planned prepared assembly disappeared from Step 21 source catalog: {planned.RelativePath}");
                if (!sourceCandidate.IsIlOnly)
                    throw new InvalidDataException($"Step 21 plan attempted to prepare non-IL-only assembly: {planned.RelativePath}");

                var sourcePath = ResolveChildPath(workspace.SourceRoot, planned.RelativePath);
                var sourceHash = await ComputeSha1HexAsync(sourcePath, cancellationToken).ConfigureAwait(false);
                if (!sourceHash.Equals(planned.Sha1Hex, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException($"Step 21 source SHA-1 changed immediately before prepared copy: {planned.RelativePath}");

                var destinationPath = ResolveChildPath(preparedRoot, planned.RelativePath);
                Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
                await CopyFileAsync(sourcePath, destinationPath, cancellationToken).ConfigureAwait(false);
                var destinationHash = await ComputeSha1HexAsync(destinationPath, cancellationToken).ConfigureAwait(false);
                if (!destinationHash.Equals(planned.Sha1Hex, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException($"Step 21 prepared-copy SHA-1 mismatch: {planned.RelativePath}");
                if (new FileInfo(destinationPath).Length != planned.Length)
                    throw new InvalidDataException($"Step 21 prepared-copy length mismatch: {planned.RelativePath}");

                var identity = ReadManagedIdentity(destinationPath);
                if (!identity.FullName.Equals(planned.AssemblyFullName, StringComparison.Ordinal))
                    throw new InvalidDataException($"Step 21 prepared assembly identity changed: {planned.RelativePath}");
                if (!identity.IsIlOnly)
                    throw new InvalidDataException($"Step 21 prepared assembly unexpectedly became non-IL-only: {planned.RelativePath}");

                preparedFiles.Add(new PreparedFileSnapshot(planned.RelativePath, destinationPath, destinationHash, identity.FullName));
            }

            stage = "binding plan serialization";
            var planPath = Path.Combine(planRoot, PlanFileName);
            await using (var stream = File.Create(planPath))
            {
                await JsonSerializer.SerializeAsync(
                    stream,
                    binding.Plan,
                    RuntimeFrameworkBindingJsonContext.Default.RuntimeFrameworkBindingPlanDocument,
                    cancellationToken).ConfigureAwait(false);
            }
            var planSha256 = await ComputeSha256HexAsync(planPath, cancellationToken).ConfigureAwait(false);

            _prepared = new PreparedSnapshot(preparedRoot, planPath, planSha256, preparedFiles.ToArray());
            EnsureNoStS2AssemblyLoaded();

            progress?.Report(new RuntimeFrameworkBindingProgress(
                RuntimeFrameworkBindingGate.PreparedRuntimeAssemblySet,
                binding.Plan.PreparedAssemblies.Length,
                binding.Plan.PreparedAssemblies.Length,
                planPath,
                "Prepared runtime set and deterministic binding plan written without Cecil assembly writes or live-install mutation."));

            return Pass(
                RuntimeFrameworkBindingGate.PreparedRuntimeAssemblySet,
                "Execution-oriented Step 21 prepared managed set created by verified byte-copy only.\n" +
                $"Prepared IL-only assemblies: {preparedFiles.Count:N0}\n" +
                $"Prepared primary sts2.dll included: {(binding.Plan.PreparedAssemblies.Count(item => item.IsPrimary) == 1 ? "YES" : "NO")}\n" +
                $"Host framework bindings kept outside private prepared set: {binding.Plan.HostFrameworkBindings.Length:N0}\n" +
                $"Explicit blockers carried into plan: {binding.Plan.Blockers.Length:N0}\n" +
                $"Runtime closure ready for first real CLR load: {(binding.Plan.RuntimeClosureReady ? "YES" : "NO")}\n" +
                $"Plan: {WorkRootName}/{PlanRootName}/{PlanFileName}\n" +
                $"Plan SHA-256: {planSha256}\n" +
                "Cecil assembly writes performed by Step 21 Gate C: 0\n" +
                "Strong-name/public-key metadata modified: NO\n" +
                "Desktop framework/ReadyToRun images copied into execution set merely because they exist in depot: NO\n" +
                "Prepared assembly bytes remain receipt-identical: YES\n" +
                "StS2 assembly loaded/executed: NO\nReal managed install modified: NO");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return Fail(RuntimeFrameworkBindingGate.PreparedRuntimeAssemblySet, stage, ex);
        }
    }

    public async Task<RuntimeFrameworkBindingGateResult> RunClosureAuditAsync(
        IProgress<RuntimeFrameworkBindingProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var stage = "initialization";
        try
        {
            stage = "workspace/binding/prepared precondition";
            var workspace = RequireWorkspace();
            var binding = RequireBinding();
            var prepared = RequirePrepared();
            EnsureNoStS2AssemblyLoaded();

            var verifiedSource = 0;
            ulong verifiedSourceBytes = 0;
            for (var index = 0; index < workspace.SelectedFiles.Length; index++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var receiptFile = workspace.SelectedFiles[index];
                var relative = NormalizeRelative(receiptFile.RelativePath);
                progress?.Report(new RuntimeFrameworkBindingProgress(
                    RuntimeFrameworkBindingGate.ClosureAudit,
                    index,
                    workspace.SelectedFiles.Length,
                    relative,
                    "Re-hashing complete Step 21 source workspace and corresponding live receipt-backed install before accepting the runtime binding plan…"));

                var sourcePath = ResolveChildPath(workspace.SourceRoot, relative);
                var livePath = ResolveChildPath(workspace.ManagedRoot, relative);
                var sourceHash = await ComputeSha1HexAsync(sourcePath, cancellationToken).ConfigureAwait(false);
                var liveHash = await ComputeSha1HexAsync(livePath, cancellationToken).ConfigureAwait(false);
                if (!sourceHash.Equals(receiptFile.Sha1Hex, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException($"Step 21 source workspace drift detected: {relative}");
                if (!liveHash.Equals(receiptFile.Sha1Hex, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException($"Trusted live install drift detected during Step 21 audit: {relative}");
                verifiedSource++;
                checked { verifiedSourceBytes += (ulong)receiptFile.Length; }
            }

            stage = "prepared file-set audit";
            var expectedPrepared = binding.Plan.PreparedAssemblies
                .Select(item => NormalizeRelative(item.RelativePath))
                .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
                .ToArray();
            var actualPrepared = Directory.EnumerateFiles(prepared.PreparedRoot, "*", SearchOption.AllDirectories)
                .Select(path => NormalizeRelative(Path.GetRelativePath(prepared.PreparedRoot, path)))
                .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
                .ToArray();
            if (!expectedPrepared.SequenceEqual(actualPrepared, StringComparer.OrdinalIgnoreCase))
                throw new InvalidDataException("Step 21 prepared directory contains a file-set different from the binding plan.");

            foreach (var planned in binding.Plan.PreparedAssemblies)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var path = ResolveChildPath(prepared.PreparedRoot, planned.RelativePath);
                var hash = await ComputeSha1HexAsync(path, cancellationToken).ConfigureAwait(false);
                if (!hash.Equals(planned.Sha1Hex, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException($"Step 21 prepared file no longer matches receipt/source SHA-1: {planned.RelativePath}");
                var identity = ReadManagedIdentity(path);
                if (!identity.IsIlOnly || !identity.FullName.Equals(planned.AssemblyFullName, StringComparison.Ordinal))
                    throw new InvalidDataException($"Step 21 prepared identity/IL-only audit failed: {planned.RelativePath}");
            }

            stage = "plan file audit";
            var planHash = await ComputeSha256HexAsync(prepared.PlanPath, cancellationToken).ConfigureAwait(false);
            if (!planHash.Equals(prepared.PlanSha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Step 21 runtime binding plan changed after Gate C.");
            await using var planStream = File.OpenRead(prepared.PlanPath);
            var persistedPlan = await JsonSerializer.DeserializeAsync(
                planStream,
                RuntimeFrameworkBindingJsonContext.Default.RuntimeFrameworkBindingPlanDocument,
                cancellationToken).ConfigureAwait(false)
                ?? throw new InvalidDataException("Step 21 runtime binding plan could not be deserialized.");
            ValidatePersistedPlan(persistedPlan, binding.Plan);

            stage = "post-plan OfflineReady verification";
            var offline = await _offlineInspection.RunAsync(null, cancellationToken).ConfigureAwait(false);
            if (!offline.Success || !offline.ExactManagedTreeVerified || offline.InstalledManifestId != workspace.Receipt.ManifestId)
                throw new InvalidDataException(offline.Error ?? "OfflineReady exact-tree verification failed after Step 21 preparation.");

            var hostBoundNames = binding.Plan.HostFrameworkBindings
                .Select(bindingItem => GetSimpleName(bindingItem.ActualFullName))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            foreach (var preparedAssembly in binding.Plan.PreparedAssemblies)
            {
                var simpleName = GetSimpleName(preparedAssembly.AssemblyFullName);
                if (hostBoundNames.Contains(simpleName))
                    throw new InvalidDataException($"Prepared set duplicates host-bound assembly simple name '{simpleName}'.");
            }

            EnsureNoStS2AssemblyLoaded();
            return Pass(
                RuntimeFrameworkBindingGate.ClosureAudit,
                "Step 21 source/prepared/live-install/binding-plan isolation audit passed.\n" +
                $"Source workspace receipt SHA-1s reverified: {verifiedSource:N0}/{workspace.SelectedFiles.Length:N0} ({verifiedSourceBytes:N0} bytes)\n" +
                $"Prepared files exactly match plan: {actualPrepared.Length:N0}/{expectedPrepared.Length:N0}\n" +
                $"Prepared files receipt-identical and managed-identity stable: {binding.Plan.PreparedAssemblies.Length:N0}/{binding.Plan.PreparedAssemblies.Length:N0}\n" +
                $"Host-bound framework simple names duplicated privately: 0\n" +
                $"Binding plan SHA-256 preserved: {prepared.PlanSha256}\n" +
                $"Explicit blockers preserved in plan: {binding.Plan.Blockers.Length:N0}\n" +
                $"Runtime closure ready for first real CLR load: {(binding.Plan.RuntimeClosureReady ? "YES" : "NO")}\n" +
                "Post-preparation OfflineReady exact-tree verification: YES\n" +
                "Original Step 12 managed install unchanged: YES\n" +
                "StS2 assembly loaded/executed: NO\n" +
                "STEP 21 meaning: 4/4 proves a trustworthy execution-set/binding plan. If Runtime closure ready=NO, Step 22 must address the recorded blockers before any real game CLR load.");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return Fail(RuntimeFrameworkBindingGate.ClosureAudit, stage, ex);
        }
    }

    private static bool TryReadManagedCandidate(
        string sourcePath,
        string relativePath,
        SteamManagedInstallFile receiptFile,
        out WorkspaceAssemblyCandidate? candidate)
    {
        try
        {
            using var resolver = RejectingAssemblyResolver.Instance;
            using var module = ModuleDefinition.ReadModule(sourcePath, new ReaderParameters
            {
                ReadingMode = ReadingMode.Deferred,
                InMemory = false,
                ReadSymbols = false,
                AssemblyResolver = resolver,
                MetadataResolver = new MetadataResolver(resolver),
            });
            if (module.Assembly?.Name is null)
            {
                candidate = null;
                return false;
            }

            var references = module.AssemblyReferences
                .Select(reference => new AssemblyReferenceSnapshot(
                    reference.Name,
                    reference.Version ?? new Version(0, 0, 0, 0),
                    NormalizeCulture(reference.Culture),
                    TokenHex(reference.PublicKeyToken),
                    reference.FullName))
                .OrderBy(reference => reference.FullName, StringComparer.Ordinal)
                .ToArray();

            candidate = new WorkspaceAssemblyCandidate(
                NormalizeRelative(relativePath),
                sourcePath,
                receiptFile.Sha1Hex.ToLowerInvariant(),
                receiptFile.Length,
                module.Assembly.Name.Name,
                module.Assembly.Name.Version ?? new Version(0, 0, 0, 0),
                NormalizeCulture(module.Assembly.Name.Culture),
                TokenHex(module.Assembly.Name.PublicKeyToken),
                module.Assembly.Name.FullName,
                (module.Attributes & ModuleAttributes.ILOnly) != 0,
                (module.Attributes & ModuleAttributes.StrongNameSigned) != 0,
                references,
                module.ModuleReferences.Select(reference => reference.Name).OrderBy(name => name, StringComparer.Ordinal).ToArray());
            return true;
        }
        catch (BadImageFormatException)
        {
            candidate = null;
            return false;
        }
    }

    private static ManagedIdentitySnapshot ReadManagedIdentity(string path)
    {
        using var resolver = RejectingAssemblyResolver.Instance;
        using var module = ModuleDefinition.ReadModule(path, new ReaderParameters
        {
            ReadingMode = ReadingMode.Deferred,
            InMemory = false,
            ReadSymbols = false,
            AssemblyResolver = resolver,
            MetadataResolver = new MetadataResolver(resolver),
        });
        if (module.Assembly?.Name is null)
            throw new BadImageFormatException($"Managed assembly manifest missing: {path}");
        return new ManagedIdentitySnapshot(module.Assembly.Name.FullName, (module.Attributes & ModuleAttributes.ILOnly) != 0);
    }

    private static bool TryBindHostFramework(
        AssemblyReferenceSnapshot reference,
        out HostBindingResult? binding,
        out string? failure)
    {
        binding = null;
        failure = null;
        if (!IsHostFrameworkProbeCandidate(reference.Name))
            return false;

        try
        {
            var requested = new System.Reflection.AssemblyName
            {
                Name = reference.Name,
                Version = reference.Version,
                CultureName = reference.Culture == "neutral" ? string.Empty : reference.Culture,
            };
            if (!string.IsNullOrEmpty(reference.PublicKeyToken))
                requested.SetPublicKeyToken(Convert.FromHexString(reference.PublicKeyToken));

            var assembly = AssemblyLoadContext.Default.LoadFromAssemblyName(requested);
            var actual = assembly.GetName();
            if (!string.Equals(actual.Name, reference.Name, StringComparison.OrdinalIgnoreCase))
            {
                failure = $"host returned unexpected simple name '{actual.Name}'";
                return false;
            }
            if (!string.Equals(NormalizeCulture(actual.CultureName), reference.Culture, StringComparison.OrdinalIgnoreCase))
            {
                failure = $"host culture mismatch: requested {reference.Culture}, actual {NormalizeCulture(actual.CultureName)}";
                return false;
            }
            var actualToken = TokenHex(actual.GetPublicKeyToken());
            if (!string.Equals(actualToken, reference.PublicKeyToken, StringComparison.OrdinalIgnoreCase))
            {
                failure = $"host public-key-token mismatch: requested {reference.PublicKeyToken}, actual {actualToken}";
                return false;
            }
            var actualVersion = actual.Version ?? new Version(0, 0, 0, 0);
            if (actualVersion.CompareTo(reference.Version) < 0)
            {
                failure = $"host version too low: requested {reference.Version}, actual {actualVersion}";
                return false;
            }

            binding = new HostBindingResult(actual.FullName ?? actual.Name ?? reference.Name, SafeAssemblyLocation(assembly));
            return true;
        }
        catch (Exception ex) when (ex is FileNotFoundException or FileLoadException or BadImageFormatException or NotSupportedException)
        {
            failure = $"{ex.GetType().Name}: {ex.Message}";
            return false;
        }
    }

    private static WorkspaceResolution ResolveWorkspaceReference(WorkspaceSnapshot workspace, AssemblyReferenceSnapshot requested)
    {
        var simpleMatches = workspace.Catalog
            .Where(candidate => candidate.Name.Equals(requested.Name, StringComparison.OrdinalIgnoreCase))
            .ToArray();
        if (simpleMatches.Length == 0)
            return WorkspaceResolution.Blocked("MissingWorkspaceAssembly", "No managed assembly with this simple name exists in the verified ARM64/shared Step 21 workspace.");

        var identityMatches = simpleMatches
            .Where(candidate => IdentityIgnoringVersionMatches(requested, candidate))
            .ToArray();
        if (identityMatches.Length == 0)
        {
            var available = string.Join(" | ", simpleMatches.Take(8).Select(candidate => candidate.FullName));
            return WorkspaceResolution.Blocked("WorkspaceIdentityMismatch", $"Same-name workspace assemblies exist, but culture/public-key-token identity differs. Candidates: {available}");
        }

        var exactMatches = identityMatches.Where(candidate => candidate.Version == requested.Version).ToArray();
        if (exactMatches.Length > 0)
            return SelectUnambiguousWorkspaceCandidate(exactMatches, versionUnified: false);

        var versions = identityMatches.Select(candidate => candidate.Version).Distinct().OrderBy(version => version).ToArray();
        if (versions.Length != 1)
        {
            var available = string.Join(", ", versions.Select(version => version.ToString()));
            return WorkspaceResolution.Blocked("WorkspaceVersionAmbiguity", $"Multiple version-distinct workspace candidates match name/culture/token: {available}");
        }

        var actualVersion = versions[0];
        if (actualVersion.CompareTo(requested.Version) < 0)
            return WorkspaceResolution.Blocked("WorkspaceVersionTooLow", $"Only matching workspace version {actualVersion} is lower than requested {requested.Version}.");

        return SelectUnambiguousWorkspaceCandidate(identityMatches, versionUnified: true);
    }

    private static WorkspaceResolution SelectUnambiguousWorkspaceCandidate(
        WorkspaceAssemblyCandidate[] matches,
        bool versionUnified)
    {
        var distinctHashes = matches.Select(candidate => candidate.ExpectedSha1).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        if (distinctHashes.Length != 1)
        {
            var available = string.Join(" | ", matches.Take(8).Select(candidate => $"{candidate.FullName} @ {candidate.RelativePath}"));
            return WorkspaceResolution.Blocked("WorkspaceByteAmbiguity", $"Multiple byte-distinct workspace files satisfy the requested identity: {available}");
        }

        var selected = matches
            .OrderBy(candidate => IsMacOsArm64ManagedPath(candidate.RelativePath) ? 0 : 1)
            .ThenBy(candidate => candidate.RelativePath, StringComparer.OrdinalIgnoreCase)
            .First();
        return new WorkspaceResolution(selected, versionUnified, string.Empty, string.Empty);
    }

    private static bool IdentityIgnoringVersionMatches(AssemblyReferenceSnapshot requested, WorkspaceAssemblyCandidate candidate)
        => candidate.Name.Equals(requested.Name, StringComparison.OrdinalIgnoreCase) &&
           candidate.Culture.Equals(requested.Culture, StringComparison.OrdinalIgnoreCase) &&
           candidate.PublicKeyToken.Equals(requested.PublicKeyToken, StringComparison.OrdinalIgnoreCase);

    private static bool IsHostFrameworkProbeCandidate(string name)
        => name.Equals("System", StringComparison.OrdinalIgnoreCase) ||
           name.StartsWith("System.", StringComparison.OrdinalIgnoreCase) ||
           name.Equals("mscorlib", StringComparison.OrdinalIgnoreCase) ||
           name.Equals("netstandard", StringComparison.OrdinalIgnoreCase) ||
           name.Equals("Microsoft.CSharp", StringComparison.OrdinalIgnoreCase) ||
           name.Equals("Microsoft.VisualBasic", StringComparison.OrdinalIgnoreCase) ||
           name.Equals("Microsoft.VisualBasic.Core", StringComparison.OrdinalIgnoreCase) ||
           name.StartsWith("Microsoft.Win32.", StringComparison.OrdinalIgnoreCase);

    private static void ValidatePersistedPlan(
        RuntimeFrameworkBindingPlanDocument persisted,
        RuntimeFrameworkBindingPlanDocument expected)
    {
        if (persisted.SchemaVersion != RuntimeFrameworkBindingPlanDocument.CurrentSchemaVersion ||
            persisted.AppId != expected.AppId ||
            persisted.DepotId != expected.DepotId ||
            persisted.ManifestId != expected.ManifestId ||
            !persisted.Branch.Equals(expected.Branch, StringComparison.Ordinal) ||
            !persisted.PrimaryAssemblyRelativePath.Equals(expected.PrimaryAssemblyRelativePath, StringComparison.OrdinalIgnoreCase) ||
            !persisted.PrimaryAssemblyFullName.Equals(expected.PrimaryAssemblyFullName, StringComparison.Ordinal) ||
            persisted.PreparedAssemblies.Length != expected.PreparedAssemblies.Length ||
            persisted.HostFrameworkBindings.Length != expected.HostFrameworkBindings.Length ||
            persisted.Blockers.Length != expected.Blockers.Length ||
            persisted.Edges.Length != expected.Edges.Length ||
            persisted.RuntimeClosureReady != expected.RuntimeClosureReady)
        {
            throw new InvalidDataException("Persisted Step 21 runtime binding plan does not match the in-memory Gate B plan summary.");
        }
    }

    private WorkspaceSnapshot RequireWorkspace()
        => _workspace ?? throw new InvalidOperationException("Gate A must pass before later Step 21 gates.");

    private BindingSnapshot RequireBinding()
        => _binding ?? throw new InvalidOperationException("Gate B must pass before later Step 21 gates.");

    private PreparedSnapshot RequirePrepared()
        => _prepared ?? throw new InvalidOperationException("Gate C must pass before Gate D.");

    private static void EnsureNoStS2AssemblyLoaded()
    {
        var matches = AppDomain.CurrentDomain.GetAssemblies()
            .Select(assembly => assembly.GetName().Name ?? string.Empty)
            .Where(name => name.Equals("sts2", StringComparison.OrdinalIgnoreCase) ||
                           name.Equals("SlayTheSpire2", StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (matches.Length > 0)
            throw new InvalidDataException("Step 21 detected a loaded real-game assembly even though CLR-loading StS2 remains out of scope: " + string.Join(", ", matches));
    }

    private async Task<SteamManagedInstallReceipt> ReadReceiptAsync(string managedRoot, CancellationToken cancellationToken)
    {
        var receiptPath = Path.Combine(managedRoot, SteamManagedInstallReceipt.FileName);
        if (!File.Exists(receiptPath))
            throw new FileNotFoundException("Managed-install receipt is missing.", receiptPath);
        await using var stream = File.OpenRead(receiptPath);
        return await JsonSerializer.DeserializeAsync(
                   stream,
                   SteamManagedInstallJsonContext.Default.SteamManagedInstallReceipt,
                   cancellationToken).ConfigureAwait(false)
               ?? throw new InvalidDataException("Managed-install receipt could not be deserialized.");
    }

    private static void ValidateReceiptSnapshot(SteamManagedInstallReceipt receipt, SteamOfflineInstallResult offline)
    {
        if (!offline.ReceiptStructurallyValid || !offline.ExactManagedTreeVerified)
            throw new InvalidDataException("Step 21 requires an exact OfflineReady managed-install receipt/tree before compatibility preparation.");
        if (receipt.SchemaVersion != SteamManagedInstallReceipt.CurrentSchemaVersion ||
            receipt.AppId != 2868840 ||
            offline.DepotId is null || receipt.DepotId != offline.DepotId.Value ||
            offline.InstalledManifestId is null || receipt.ManifestId != offline.InstalledManifestId.Value ||
            !receipt.Branch.Equals(offline.Branch, StringComparison.Ordinal))
            throw new InvalidDataException("Managed-install receipt identity changed between OfflineReady inspection and Step 21 setup.");
        if (receipt.Files.Count != offline.PlannedFiles)
            throw new InvalidDataException("Managed-install receipt file count differs from OfflineReady verification.");
        foreach (var file in receipt.Files)
        {
            if (!SteamSingleFileTargetSelector.IsSafeRelativePath(file.RelativePath) ||
                file.Length < 0 ||
                file.Sha1Hex.Length != 40 ||
                !file.Sha1Hex.All(Uri.IsHexDigit))
                throw new InvalidDataException($"Malformed managed-install receipt file entry: {file.RelativePath}");
        }
    }

    private void PrepareFreshWorkRoot()
    {
        BestEffortDeleteWorkRoot();
        Directory.CreateDirectory(_workRoot);
    }

    private void BestEffortDeleteWorkRoot() => BestEffortDeleteDirectory(_workRoot);

    private static void BestEffortDeleteDirectory(string path)
    {
        try
        {
            if (Directory.Exists(path))
                Directory.Delete(path, recursive: true);
        }
        catch
        {
            // A failed cleanup must not broaden the write boundary; a later create/copy will fail explicitly.
        }
    }

    private static async Task CopyFileAsync(string source, string destination, CancellationToken cancellationToken)
    {
        await using var input = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.Read, 128 * 1024, useAsync: true);
        await using var output = new FileStream(destination, FileMode.CreateNew, FileAccess.Write, FileShare.None, 128 * 1024, useAsync: true);
        await input.CopyToAsync(output, 128 * 1024, cancellationToken).ConfigureAwait(false);
        await output.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    private static async Task<string> ComputeSha1HexAsync(string path, CancellationToken cancellationToken)
    {
        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 128 * 1024, useAsync: true);
        using var sha1 = SHA1.Create();
        var hash = await sha1.ComputeHashAsync(stream, cancellationToken).ConfigureAwait(false);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static async Task<string> ComputeSha256HexAsync(string path, CancellationToken cancellationToken)
    {
        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 128 * 1024, useAsync: true);
        using var sha256 = SHA256.Create();
        var hash = await sha256.ComputeHashAsync(stream, cancellationToken).ConfigureAwait(false);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static string ResolveChildPath(string root, string relativePath)
    {
        var normalized = NormalizeRelative(relativePath);
        if (!SteamSingleFileTargetSelector.IsSafeRelativePath(normalized))
            throw new InvalidDataException($"Unsafe Step 21 relative path: {relativePath}");
        var fullRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var full = Path.GetFullPath(Path.Combine(root, normalized.Replace('/', Path.DirectorySeparatorChar)));
        if (!full.StartsWith(fullRoot, StringComparison.Ordinal))
            throw new InvalidDataException($"Step 21 path escaped its declared root: {relativePath}");
        return full;
    }

    private static string NormalizeRelative(string path)
        => path.Replace('\\', '/').TrimStart('/');

    private static bool IsManagedAssemblyFileName(string relativePath)
        => relativePath.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) ||
           relativePath.EndsWith(".exe", StringComparison.OrdinalIgnoreCase);

    private static bool IsMacOsArm64ManagedPath(string relativePath)
        => ("/" + NormalizeRelative(relativePath))
            .Contains("/data_sts2_macos_arm64/", StringComparison.OrdinalIgnoreCase);

    private static bool IsMacOsX8664ManagedPath(string relativePath)
        => ("/" + NormalizeRelative(relativePath))
            .Contains("/data_sts2_macos_x86_64/", StringComparison.OrdinalIgnoreCase);

    private static bool IsPrimaryArm64StS2Path(string relativePath)
        => ("/" + NormalizeRelative(relativePath))
            .EndsWith("/data_sts2_macos_arm64/sts2.dll", StringComparison.OrdinalIgnoreCase);

    private static string NormalizeCulture(string? culture)
        => string.IsNullOrWhiteSpace(culture) ? "neutral" : culture;

    private static string TokenHex(byte[]? token)
        => token is null || token.Length == 0 ? string.Empty : Convert.ToHexString(token).ToLowerInvariant();

    private static string SafeAssemblyLocation(Assembly assembly)
    {
        try
        {
            return string.IsNullOrWhiteSpace(assembly.Location) ? "<iOS host/AOT bundle>" : assembly.Location;
        }
        catch
        {
            return "<iOS host/AOT bundle>";
        }
    }

    private static string GetSimpleName(string fullName)
    {
        var comma = fullName.IndexOf(',');
        return comma < 0 ? fullName.Trim() : fullName[..comma].Trim();
    }

    private static RuntimeFrameworkBindingGateResult Pass(RuntimeFrameworkBindingGate gate, string detail)
        => new(gate, true, detail);

    private static RuntimeFrameworkBindingGateResult Fail(RuntimeFrameworkBindingGate gate, string stage, Exception ex)
        => new(gate, false, $"Stage: {stage}\n{ex.GetType().Name}: {ex.Message}");

    private sealed record WorkspaceSnapshot(
        string ManagedRoot,
        string ManagedInstallRelativePath,
        string SourceRoot,
        SteamManagedInstallReceipt Receipt,
        SteamOfflineInstallResult Offline,
        SteamManagedInstallFile[] SelectedFiles,
        WorkspaceAssemblyCandidate[] Catalog,
        WorkspaceAssemblyCandidate Primary,
        int AllManagedFilenameCandidates,
        int Arm64Candidates,
        int X86Candidates,
        int SharedCandidates,
        int NonManagedFilenameCandidates,
        ulong SelectedBytes);

    private sealed record WorkspaceAssemblyCandidate(
        string RelativePath,
        string SourcePath,
        string ExpectedSha1,
        long Length,
        string Name,
        Version Version,
        string Culture,
        string PublicKeyToken,
        string FullName,
        bool IsIlOnly,
        bool StrongNameSigned,
        AssemblyReferenceSnapshot[] References,
        string[] ModuleReferences);

    private sealed record AssemblyReferenceSnapshot(
        string Name,
        Version Version,
        string Culture,
        string PublicKeyToken,
        string FullName);

    private sealed record ManagedIdentitySnapshot(string FullName, bool IsIlOnly);
    private sealed record HostBindingResult(string ActualFullName, string ActualLocation);
    private sealed record BindingSnapshot(RuntimeFrameworkBindingPlanDocument Plan);
    private sealed record PreparedSnapshot(string PreparedRoot, string PlanPath, string PlanSha256, PreparedFileSnapshot[] Files);
    private sealed record PreparedFileSnapshot(string RelativePath, string Path, string Sha1Hex, string AssemblyFullName);

    private sealed class HostBindingAccumulator(string requestedFullName, string actualFullName, string actualLocation)
    {
        public string RequestedFullName { get; } = requestedFullName;
        public string ActualFullName { get; } = actualFullName;
        public string ActualLocation { get; } = actualLocation;
        public int ReferenceCount { get; set; }
    }

    private sealed record WorkspaceResolution(
        WorkspaceAssemblyCandidate? Candidate,
        bool VersionUnified,
        string BlockerKind,
        string Detail)
    {
        public static WorkspaceResolution Blocked(string kind, string detail)
            => new(null, false, kind, detail);
    }

    private sealed class RejectingAssemblyResolver : IAssemblyResolver
    {
        public static readonly RejectingAssemblyResolver Instance = new();
        public AssemblyDefinition Resolve(AssemblyNameReference name) => throw new AssemblyResolutionException(name);
        public AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters) => throw new AssemblyResolutionException(name);
        public void Dispose() { }
    }

    private sealed class CallbackProgress<T>(Action<T> callback) : IProgress<T>
    {
        public void Report(T value) => callback(value);
    }
}
