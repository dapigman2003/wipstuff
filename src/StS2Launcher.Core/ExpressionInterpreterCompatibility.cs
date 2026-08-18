using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text.Json;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace StS2Launcher.Core;

/// <summary>
/// Step 19 boundary. Proves the library-level System.Linq.Expressions interpreter on the
/// physical iOS/AOT host, clones a fresh receipt-backed ARM64/shared managed workspace,
/// discovers real direct expression-tree Compile calls, and rewrites only structurally-safe
/// parameterless/literal-false calls to request preferInterpretation=true. All game-file
/// writes remain launcher-private and the trusted Step 12 install stays read-only.
/// </summary>
public sealed class ExpressionInterpreterCompatibility
{
    public const string WorkRootName = "Step19-ExpressionInterpreterCompatibility";
    public const string SourceRootName = "source";
    public const string PreparedRootName = "prepared";

    private const int SampleLimit = 16;

    private readonly string _launcherDataRoot;
    private readonly string _workRoot;
    private readonly SteamOfflineInstallInspection _offlineInspection;
    private readonly SortedSet<string> _workspaceResolvedAssemblies = new(StringComparer.Ordinal);
    private WorkspaceSnapshot? _workspace;
    private DiscoverySnapshot? _discovery;
    private RewriteSnapshot? _rewrite;

    public ExpressionInterpreterCompatibility(string launcherDataRoot)
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
        _discovery = null;
        _rewrite = null;
        _workspaceResolvedAssemblies.Clear();
    }

    public async Task<ExpressionInterpreterCompatibilityGateResult> RunInterpreterCapabilityAndWorkspaceCloneAsync(
        IProgress<ExpressionInterpreterCompatibilityProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Reset();
            cancellationToken.ThrowIfCancellationRequested();

            progress?.Report(new ExpressionInterpreterCompatibilityProgress(
                ExpressionInterpreterCompatibilityGate.InterpreterCapabilityAndWorkspaceClone,
                0,
                0,
                null,
                "Proving System.Linq.Expressions Compile(preferInterpretation: true) in the actual iOS/AOT launcher process before touching any game copy…"));

            var interpreterProbe = RunInterpreterProbe();

            IProgress<SteamOfflineInstallProgress>? offlineProgress = progress is null
                ? null
                : new CallbackProgress<SteamOfflineInstallProgress>(value =>
                    progress.Report(new ExpressionInterpreterCompatibilityProgress(
                        ExpressionInterpreterCompatibilityGate.InterpreterCapabilityAndWorkspaceClone,
                        value.CompletedFiles,
                        value.TotalFiles,
                        value.CurrentFile,
                        $"OfflineReady precondition — {value.Message} ({value.CompletedBytes:N0}/{value.TotalBytes:N0} bytes)")));

            var offline = await _offlineInspection.RunAsync(offlineProgress, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            if (offline.Outcome == SteamOfflineInstallOutcome.Cancelled)
                throw new OperationCanceledException("Step 19 OfflineReady precondition was cancelled.", cancellationToken);
            if (!offline.Success || string.IsNullOrWhiteSpace(offline.ManagedInstallRelativePath))
                throw new InvalidDataException(offline.Error ?? $"Managed install is not OfflineReady ({offline.State}/{offline.Outcome}).");

            var managedRoot = ResolveChildPath(_launcherDataRoot, offline.ManagedInstallRelativePath);
            var receipt = await ReadReceiptAsync(managedRoot, cancellationToken).ConfigureAwait(false);
            ValidateReceiptSnapshot(receipt, offline);

            var allManaged = receipt.Files
                .Where(file => IsManagedAssemblyFileName(file.RelativePath))
                .OrderBy(file => file.RelativePath, StringComparer.OrdinalIgnoreCase)
                .ToArray();
            if (allManaged.Length == 0)
                throw new InvalidDataException("The verified install contains no receipt-backed managed-module filename candidates.");

            var arm64 = allManaged.Where(file => IsMacOsArm64ManagedPath(file.RelativePath)).ToArray();
            var x86 = allManaged.Where(file => IsMacOsX8664ManagedPath(file.RelativePath)).ToArray();
            var shared = allManaged
                .Where(file => !IsMacOsArm64ManagedPath(file.RelativePath) && !IsMacOsX8664ManagedPath(file.RelativePath))
                .ToArray();
            if (arm64.Length == 0)
                throw new InvalidDataException("No receipt-backed data_sts2_macos_arm64 managed modules were found.");

            var scopeFiles = arm64
                .Concat(shared)
                .OrderBy(file => file.RelativePath, StringComparer.OrdinalIgnoreCase)
                .ToArray();
            var primaryMatches = arm64.Where(file => IsPrimaryArm64StS2Path(file.RelativePath)).ToArray();
            if (primaryMatches.Length != 1)
                throw new InvalidDataException($"Expected exactly one macOS arm64 sts2.dll, found {primaryMatches.Length}.");

            ulong scopeBytes = 0;
            foreach (var file in scopeFiles)
                checked { scopeBytes += (ulong)file.Length; }

            PrepareFreshWorkRoot();
            var sourceRoot = Path.Combine(_workRoot, SourceRootName);
            Directory.CreateDirectory(sourceRoot);

            ulong copiedBytes = 0;
            for (var index = 0; index < scopeFiles.Length; index++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var file = scopeFiles[index];
                var relative = NormalizeRelative(file.RelativePath);
                progress?.Report(new ExpressionInterpreterCompatibilityProgress(
                    ExpressionInterpreterCompatibilityGate.InterpreterCapabilityAndWorkspaceClone,
                    index,
                    scopeFiles.Length,
                    relative,
                    $"Copying receipt-backed ARM64/shared managed payload into fresh Step 19 source storage ({copiedBytes:N0}/{scopeBytes:N0} bytes copied)…"));

                var sourcePath = ResolveChildPath(managedRoot, relative);
                var destinationPath = ResolveChildPath(sourceRoot, relative);
                Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
                await CopyFileAsync(sourcePath, destinationPath, cancellationToken).ConfigureAwait(false);

                var info = new FileInfo(destinationPath);
                if (info.Length != file.Length)
                    throw new InvalidDataException($"Step 19 workspace copy length mismatch for {relative}: {info.Length} != {file.Length}.");
                var hash = await ComputeSha1HexAsync(destinationPath, cancellationToken).ConfigureAwait(false);
                if (!hash.Equals(file.Sha1Hex, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException($"Step 19 workspace copy SHA-1 mismatch for {relative}.");
                checked { copiedBytes += (ulong)file.Length; }
            }

            if (copiedBytes != scopeBytes)
                throw new InvalidDataException("Step 19 workspace byte accounting did not cover the complete selected managed scope.");

            _workspace = new WorkspaceSnapshot(
                managedRoot,
                sourceRoot,
                receipt,
                offline,
                scopeFiles,
                NormalizeRelative(primaryMatches[0].RelativePath),
                allManaged.Length,
                arm64.Length,
                x86.Length,
                shared.Length,
                scopeBytes,
                interpreterProbe);

            progress?.Report(new ExpressionInterpreterCompatibilityProgress(
                ExpressionInterpreterCompatibilityGate.InterpreterCapabilityAndWorkspaceClone,
                scopeFiles.Length,
                scopeFiles.Length,
                _workspace.PrimaryRelativePath,
                "Expression interpreter proof and fresh receipt-backed Step 19 workspace clone complete."));

            return Pass(
                ExpressionInterpreterCompatibilityGate.InterpreterCapabilityAndWorkspaceClone,
                "Physical-host expression interpreter capability and fresh compatibility workspace established.\n" +
                $"Compile(preferInterpretation: true) probe result: {interpreterProbe.Result} (expected 42)\n" +
                $"RuntimeFeature.IsDynamicCodeSupported: {interpreterProbe.DynamicCodeSupported}\n" +
                $"RuntimeFeature.IsDynamicCodeCompiled: {interpreterProbe.DynamicCodeCompiled}\n" +
                $"OfflineReady precondition: YES ({offline.VerifiedFiles:N0}/{offline.PlannedFiles:N0} files)\n" +
                $"macOS arm64 candidates copied: {arm64.Length:N0}\n" +
                $"Architecture-neutral candidates copied: {shared.Length:N0}\n" +
                $"macOS x86_64 duplicates excluded: {x86.Length:N0}\n" +
                $"Step 19 source workspace: {scopeFiles.Length:N0} files / {scopeBytes:N0} bytes\n" +
                $"Primary assembly: {_workspace.PrimaryRelativePath}\n" +
                $"Workspace root: {WorkRootName}/{SourceRootName}\n" +
                "Every workspace source copy receipt SHA-1 verified: YES\n" +
                "Game assembly loaded/executed: NO\nSteam session consulted: NO\nNetwork attempted by Step 19: NO\nReal managed install modified: NO");
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
            return Fail(ExpressionInterpreterCompatibilityGate.InterpreterCapabilityAndWorkspaceClone, "interpreter capability / workspace clone", ex);
        }
    }

    public ExpressionInterpreterCompatibilityGateResult RunRealCompileTargetDiscovery()
    {
        var stage = "initialization";
        try
        {
            stage = "workspace target scan setup";
            var workspace = RequireWorkspace();
            var assemblies = new List<TargetAssemblySnapshot>();
            var samples = new List<string>();
            var parsedModules = 0;
            var nonManagedCandidates = 0;
            long parameterlessSafe = 0;
            long literalFalse = 0;
            long existingTrue = 0;
            long dynamicBoolean = 0;
            long unsafeParameterless = 0;
            long strongNamedSupported = 0;
            long writableSupported = 0;
            long primarySupported = 0;

            stage = "real direct Compile call-site scan";
            using var resolver = CreateWorkspaceResolver(workspace);
            foreach (var file in workspace.Files)
            {
                var relative = NormalizeRelative(file.RelativePath);
                var sourcePath = ResolveChildPath(workspace.SourceRoot, relative);
                try
                {
                    using var module = ReadModuleWithWorkspaceResolver(sourcePath, resolver, ReadingMode.Deferred);
                    EnsureWorkspaceResolverBound(module, resolver);
                    if (module.Assembly?.Name is null)
                        continue;
                    parsedModules++;

                    var stats = ScanCompileSites(module);
                    var strongNamed = IsStrongNamed(module);
                    var supported = checked(stats.ParameterlessSafe + stats.LiteralFalse);
                    var isPrimary = relative.Equals(workspace.PrimaryRelativePath, StringComparison.OrdinalIgnoreCase);
                    if (isPrimary)
                        primarySupported = supported;

                    parameterlessSafe += stats.ParameterlessSafe;
                    literalFalse += stats.LiteralFalse;
                    existingTrue += stats.LiteralTrue;
                    dynamicBoolean += stats.DynamicBoolean;
                    unsafeParameterless += stats.ParameterlessUnsafe;
                    if (strongNamed)
                        strongNamedSupported += supported;
                    else
                        writableSupported += supported;

                    foreach (var sample in stats.Samples)
                        AddSample(samples, $"{relative}: {sample}");

                    assemblies.Add(new TargetAssemblySnapshot(
                        relative,
                        module.Assembly.Name.FullName,
                        strongNamed,
                        stats));
                }
                catch (BadImageFormatException)
                {
                    nonManagedCandidates++;
                }
            }
            RecordWorkspaceResolutions(resolver);

            var writableTargets = assemblies
                .Where(value => !value.StrongNamed && value.Stats.SupportedRewrites > 0)
                .OrderBy(value => value.RelativePath.Equals(workspace.PrimaryRelativePath, StringComparison.OrdinalIgnoreCase) ? 0 : 1)
                .ThenBy(value => value.RelativePath, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (writableSupported <= 0 || writableTargets.Length == 0)
            {
                throw new InvalidDataException(
                    "No structurally-safe unsigned direct System.Linq.Expressions Compile target is available for the Step 19 rewrite. " +
                    $"Observed parameterless-safe={parameterlessSafe}, literal-false={literalFalse}, parameterless-unsafe={unsafeParameterless}, strong-named-supported={strongNamedSupported}. " +
                    "Do not broaden the rewrite speculatively; choose the next evidence-backed incompatibility class from Step 17 instead.");
            }

            _discovery = new DiscoverySnapshot(
                parsedModules,
                nonManagedCandidates,
                assemblies.ToArray(),
                writableTargets,
                parameterlessSafe,
                literalFalse,
                existingTrue,
                dynamicBoolean,
                unsafeParameterless,
                strongNamedSupported,
                writableSupported,
                primarySupported,
                samples.ToArray());

            return Pass(
                ExpressionInterpreterCompatibilityGate.RealCompileTargetDiscovery,
                "Real direct expression-tree Compile call sites classified from the receipt-verified Step 19 source workspace.\n" +
                $"Managed modules parsed: {parsedModules:N0}\n" +
                $"Non-managed .dll/.exe candidates skipped: {nonManagedCandidates:N0}\n" +
                $"Structurally-safe parameterless Compile() sites: {parameterlessSafe:N0}\n" +
                $"Literal Compile(false) sites: {literalFalse:N0}\n" +
                $"Already-interpreted Compile(true) sites: {existingTrue:N0}\n" +
                $"Dynamic/non-literal Compile(bool) sites left untouched: {dynamicBoolean:N0}\n" +
                $"Parameterless sites skipped for branch/EH/prefix safety: {unsafeParameterless:N0}\n" +
                $"Supported sites in strong-named assemblies deliberately skipped: {strongNamedSupported:N0}\n" +
                $"Writable supported sites selected: {writableSupported:N0} across {writableTargets.Length:N0} assembly/assemblies\n" +
                $"Supported sites inside primary sts2.dll: {primarySupported:N0}\n" +
                "Sample real sites:\n" + FormatLineSamples(samples) + "\n\n" +
                "Rewrite policy: only direct LambdaExpression/Expression<TDelegate>.Compile() or literal Compile(false) calls are eligible; arbitrary Compile(bool), strong-named assemblies, control-flow-sensitive insertion points, Harmony/MonoMod, Reflection.Emit, Assembly.Load and native interop are not rewritten by Step 19.\n" +
                "Assembly dependency resolver: SHA-1-verified Step 19 source workspace ONLY\nGame assembly loaded/executed: NO\nReal managed install modified: NO");
        }
        catch (Exception ex)
        {
            _discovery = null;
            return Fail(ExpressionInterpreterCompatibilityGate.RealCompileTargetDiscovery, stage, ex);
        }
    }

    public ExpressionInterpreterCompatibilityGateResult RunPreferInterpretationRewrite()
    {
        var stage = "initialization";
        try
        {
            stage = "rewrite snapshot setup";
            var workspace = RequireWorkspace();
            var discovery = _discovery ?? throw new InvalidOperationException("Gate B must pass before the Step 19 compatibility rewrite.");
            var preparedRoot = Path.Combine(_workRoot, PreparedRootName);
            if (Directory.Exists(preparedRoot))
                Directory.Delete(preparedRoot, recursive: true);
            Directory.CreateDirectory(preparedRoot);

            stage = "full prepared-tree byte copy";
            foreach (var file in workspace.Files)
            {
                var relative = NormalizeRelative(file.RelativePath);
                var sourcePath = ResolveChildPath(workspace.SourceRoot, relative);
                var destinationPath = ResolveChildPath(preparedRoot, relative);
                Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
                File.Copy(sourcePath, destinationPath, overwrite: false);
            }

            var rewrittenAssemblies = new List<PreparedAssemblySnapshot>();
            long totalRewrittenSites = 0;
            long totalParameterless = 0;
            long totalLiteralFalse = 0;

            foreach (var target in discovery.WritableTargets)
            {
                stage = $"source SHA-1 recheck: {target.RelativePath}";
                var receiptFile = GetReceiptFile(workspace, target.RelativePath);
                var sourcePath = ResolveChildPath(workspace.SourceRoot, target.RelativePath);
                var sourceHashBefore = ComputeSha1Hex(sourcePath);
                if (!sourceHashBefore.Equals(receiptFile.Sha1Hex, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException($"Step 19 source changed before rewrite: {target.RelativePath}");

                stage = $"Cecil rewrite: {target.RelativePath}";
                CompileSiteStats before;
                ModuleFingerprint beforeFingerprint;
                int rewrittenParameterless;
                int rewrittenLiteralFalse;
                using (var resolver = CreateWorkspaceResolver(workspace))
                using (var module = ReadModuleWithWorkspaceResolver(sourcePath, resolver, ReadingMode.Immediate))
                {
                    EnsureWorkspaceResolverBound(module, resolver);
                    if (IsStrongNamed(module))
                        throw new InvalidDataException($"Step 19 refuses to rewrite strong-named assembly: {target.RelativePath}");

                    before = ScanCompileSites(module);
                    beforeFingerprint = Fingerprint(module);
                    if (!string.Equals(beforeFingerprint.AssemblyFullName, target.AssemblyFullName, StringComparison.Ordinal))
                        throw new InvalidDataException($"Step 19 target assembly identity changed between Gate B and Gate C for {target.RelativePath}.");
                    if (before.SupportedRewrites != target.Stats.SupportedRewrites)
                    {
                        throw new InvalidDataException(
                            $"Step 19 target count changed between Gate B and Gate C for {target.RelativePath}: " +
                            $"{before.SupportedRewrites} != {target.Stats.SupportedRewrites}.");
                    }

                    (rewrittenParameterless, rewrittenLiteralFalse) = ApplyPreferInterpretationRewrite(module);
                    if (rewrittenParameterless != before.ParameterlessSafe || rewrittenLiteralFalse != before.LiteralFalse)
                    {
                        throw new InvalidDataException(
                            $"Step 19 rewrite count mismatch for {target.RelativePath}: " +
                            $"parameterless {rewrittenParameterless}/{before.ParameterlessSafe}, literal-false {rewrittenLiteralFalse}/{before.LiteralFalse}.");
                    }

                    var outputPath = ResolveChildPath(preparedRoot, target.RelativePath);
                    var temporaryPath = outputPath + ".step19tmp";
                    DeleteIfExists(temporaryPath);
                    module.Write(temporaryPath, new WriterParameters { WriteSymbols = false });
                    DeleteIfExists(outputPath);
                    File.Move(temporaryPath, outputPath);
                    RecordWorkspaceResolutions(resolver);
                }

                stage = $"explicit-resolver prepared-output reopen: {target.RelativePath}";
                var preparedPath = ResolveChildPath(preparedRoot, target.RelativePath);
                CompileSiteStats after;
                ModuleFingerprint afterFingerprint;
                using (var resolver = CreateWorkspaceResolver(workspace))
                using (var reopened = ReadModuleWithWorkspaceResolver(preparedPath, resolver, ReadingMode.Deferred))
                {
                    EnsureWorkspaceResolverBound(reopened, resolver);
                    after = ScanCompileSites(reopened);
                    afterFingerprint = Fingerprint(reopened);
                    RecordWorkspaceResolutions(resolver);
                }

                var rewrittenTotal = checked(rewrittenParameterless + rewrittenLiteralFalse);
                if (!beforeFingerprint.MetadataEquivalentTo(afterFingerprint))
                    throw new InvalidDataException($"Step 19 prepared output changed the structural metadata fingerprint for {target.RelativePath}. Before={beforeFingerprint}; After={afterFingerprint}.");
                if (afterFingerprint.InstructionCount != checked(beforeFingerprint.InstructionCount + rewrittenParameterless))
                    throw new InvalidDataException($"Step 19 prepared output instruction-count delta is not exactly the inserted parameterless Compile argument count for {target.RelativePath}.");
                if (after.TotalDirectCompileSites != before.TotalDirectCompileSites)
                    throw new InvalidDataException($"Step 19 changed the number of direct expression Compile call sites in {target.RelativePath}.");
                if (after.ParameterlessSafe != 0 || after.LiteralFalse != 0)
                    throw new InvalidDataException($"Prepared output still contains supported non-interpreted Compile sites in {target.RelativePath}.");
                if (after.ParameterlessUnsafe != before.ParameterlessUnsafe || after.DynamicBoolean != before.DynamicBoolean)
                    throw new InvalidDataException($"Step 19 changed an explicitly out-of-scope Compile-site class in {target.RelativePath}.");
                if (after.LiteralTrue != checked(before.LiteralTrue + rewrittenTotal))
                    throw new InvalidDataException($"Prepared output does not contain the expected Compile(true) count in {target.RelativePath}.");

                var preparedHash = ComputeSha1Hex(preparedPath);
                if (preparedHash.Equals(sourceHashBefore, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException($"Step 19 prepared output bytes did not change for {target.RelativePath}.");
                var sourceHashAfter = ComputeSha1Hex(sourcePath);
                if (!sourceHashAfter.Equals(receiptFile.Sha1Hex, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException($"Step 19 source copy changed while writing prepared output: {target.RelativePath}");

                rewrittenAssemblies.Add(new PreparedAssemblySnapshot(
                    target.RelativePath,
                    sourceHashBefore,
                    preparedHash,
                    rewrittenParameterless,
                    rewrittenLiteralFalse,
                    before,
                    after,
                    beforeFingerprint,
                    afterFingerprint));
                totalRewrittenSites = checked(totalRewrittenSites + rewrittenTotal);
                totalParameterless = checked(totalParameterless + rewrittenParameterless);
                totalLiteralFalse = checked(totalLiteralFalse + rewrittenLiteralFalse);
            }

            if (totalRewrittenSites <= 0 || rewrittenAssemblies.Count == 0)
                throw new InvalidDataException("Step 19 Gate C produced no real compatibility rewrite.");
            if (totalRewrittenSites != discovery.WritableSupported)
                throw new InvalidDataException($"Step 19 did not rewrite the complete selected supported set: {totalRewrittenSites} != {discovery.WritableSupported}.");

            _rewrite = new RewriteSnapshot(
                preparedRoot,
                rewrittenAssemblies.ToArray(),
                totalRewrittenSites,
                totalParameterless,
                totalLiteralFalse);

            return Pass(
                ExpressionInterpreterCompatibilityGate.PreferInterpretationRewrite,
                "Real AOT compatibility rewrite applied to launcher-private prepared assembly copies.\n" +
                $"Rewritten assemblies: {rewrittenAssemblies.Count:N0}\n" +
                $"Direct parameterless Compile() sites converted to Compile(true): {totalParameterless:N0}\n" +
                $"Literal Compile(false) sites converted to Compile(true): {totalLiteralFalse:N0}\n" +
                $"Total real call sites rewritten: {totalRewrittenSites:N0}\n" +
                "Every rewritten assembly reopened with explicit workspace assembly + metadata resolvers: YES\n" +
                "Every rewritten assembly preserves structural metadata; instruction-count delta equals only inserted bool arguments: YES\n" +
                "Every rewritten assembly has zero remaining structurally-safe parameterless/literal-false target sites: YES\n" +
                "Dynamic Compile(bool), unsafe branch/EH insertion sites and strong-named assemblies preserved: YES\n" +
                "All unchanged Step 19 prepared files remain byte-for-byte source copies: pending Gate D full audit\n" +
                $"Workspace-only dependency resolutions observed: {_workspaceResolvedAssemblies.Count:N0}\n" +
                "Source workspace receipt SHA-1 preserved for every rewritten source: YES\n" +
                "Actual Step 12 install modified: NO\nGame assembly loaded/executed: NO");
        }
        catch (Exception ex)
        {
            _rewrite = null;
            return Fail(ExpressionInterpreterCompatibilityGate.PreferInterpretationRewrite, stage, ex);
        }
    }

    public async Task<ExpressionInterpreterCompatibilityGateResult> RunIsolationAuditAsync(
        IProgress<ExpressionInterpreterCompatibilityProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var stage = "initialization";
        try
        {
            stage = "audit snapshot setup";
            var workspace = RequireWorkspace();
            var discovery = _discovery ?? throw new InvalidOperationException("Gate B must pass before the Step 19 isolation audit.");
            var rewrite = _rewrite ?? throw new InvalidOperationException("Gate C must pass before the Step 19 isolation audit.");

            var expectedPaths = workspace.Files
                .Select(file => NormalizeRelative(file.RelativePath))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            var sourcePaths = EnumerateRelativeFiles(workspace.SourceRoot);
            var preparedPaths = EnumerateRelativeFiles(rewrite.PreparedRoot);
            if (!expectedPaths.SetEquals(sourcePaths))
                throw new InvalidDataException("Step 19 source workspace no longer contains exactly the selected receipt-backed file set.");
            if (!expectedPaths.SetEquals(preparedPaths))
                throw new InvalidDataException("Step 19 prepared workspace does not contain exactly the selected source file set.");

            var rewrittenByPath = rewrite.Assemblies.ToDictionary(value => value.RelativePath, StringComparer.OrdinalIgnoreCase);
            ulong sourceVerifiedBytes = 0;
            ulong installVerifiedBytes = 0;
            var unchangedPreparedFiles = 0;
            var rewrittenPreparedFiles = 0;

            for (var index = 0; index < workspace.Files.Length; index++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var file = workspace.Files[index];
                var relative = NormalizeRelative(file.RelativePath);
                progress?.Report(new ExpressionInterpreterCompatibilityProgress(
                    ExpressionInterpreterCompatibilityGate.IsolationAudit,
                    index,
                    workspace.Files.Length,
                    relative,
                    "Re-hashing Step 19 source, prepared output, and original managed install; rewritten outputs must be the only byte differences…"));

                var sourcePath = ResolveChildPath(workspace.SourceRoot, relative);
                var sourceHash = await ComputeSha1HexAsync(sourcePath, cancellationToken).ConfigureAwait(false);
                if (!sourceHash.Equals(file.Sha1Hex, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException($"Step 19 source no longer matches receipt SHA-1: {relative}");
                checked { sourceVerifiedBytes += (ulong)file.Length; }

                var installPath = ResolveChildPath(workspace.ManagedRoot, relative);
                var installHash = await ComputeSha1HexAsync(installPath, cancellationToken).ConfigureAwait(false);
                if (!installHash.Equals(file.Sha1Hex, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException($"Original managed install no longer matches receipt SHA-1 after Step 19: {relative}");
                checked { installVerifiedBytes += (ulong)file.Length; }

                var preparedPath = ResolveChildPath(rewrite.PreparedRoot, relative);
                var preparedHash = await ComputeSha1HexAsync(preparedPath, cancellationToken).ConfigureAwait(false);
                if (rewrittenByPath.TryGetValue(relative, out var rewritten))
                {
                    if (!preparedHash.Equals(rewritten.PreparedSha1, StringComparison.OrdinalIgnoreCase))
                        throw new InvalidDataException($"Step 19 rewritten prepared output changed after Gate C: {relative}");
                    if (preparedHash.Equals(file.Sha1Hex, StringComparison.OrdinalIgnoreCase))
                        throw new InvalidDataException($"Step 19 rewritten prepared output unexpectedly matches its source bytes: {relative}");
                    rewrittenPreparedFiles++;
                }
                else
                {
                    if (!preparedHash.Equals(file.Sha1Hex, StringComparison.OrdinalIgnoreCase))
                        throw new InvalidDataException($"Step 19 changed a non-target prepared file: {relative}");
                    unchangedPreparedFiles++;
                }
            }

            if (sourceVerifiedBytes != workspace.ScopeBytes || installVerifiedBytes != workspace.ScopeBytes)
                throw new InvalidDataException("Step 19 source/install audit byte accounting did not cover the complete selected scope.");
            if (rewrittenPreparedFiles != rewrite.Assemblies.Length || unchangedPreparedFiles + rewrittenPreparedFiles != workspace.Files.Length)
                throw new InvalidDataException("Step 19 prepared-tree target/non-target accounting is inconsistent.");

            stage = "rewritten prepared assembly structural audit";
            foreach (var rewritten in rewrite.Assemblies)
            {
                var path = ResolveChildPath(rewrite.PreparedRoot, rewritten.RelativePath);
                using var resolver = CreateWorkspaceResolver(workspace);
                using var module = ReadModuleWithWorkspaceResolver(path, resolver, ReadingMode.Deferred);
                EnsureWorkspaceResolverBound(module, resolver);
                var stats = ScanCompileSites(module);
                var fingerprint = Fingerprint(module);
                if (fingerprint != rewritten.AfterFingerprint)
                    throw new InvalidDataException($"Final Step 19 structural fingerprint changed after Gate C: {rewritten.RelativePath}.");
                if (stats.TotalDirectCompileSites != rewritten.After.TotalDirectCompileSites)
                    throw new InvalidDataException($"Final Step 19 direct Compile-site count changed after Gate C: {rewritten.RelativePath}.");
                if (stats.ParameterlessSafe != 0 || stats.LiteralFalse != 0)
                    throw new InvalidDataException($"Final audit found an unconverted supported Compile site in {rewritten.RelativePath}.");
                if (stats.LiteralTrue != rewritten.After.LiteralTrue || stats.DynamicBoolean != rewritten.After.DynamicBoolean || stats.ParameterlessUnsafe != rewritten.After.ParameterlessUnsafe)
                    throw new InvalidDataException($"Final Step 19 structural counts changed after Gate C: {rewritten.RelativePath}.");
                RecordWorkspaceResolutions(resolver);
            }

            if (rewrite.TotalRewrittenSites != discovery.WritableSupported)
                throw new InvalidDataException("Final Step 19 rewrite count no longer matches the Gate B selected target set.");

            progress?.Report(new ExpressionInterpreterCompatibilityProgress(
                ExpressionInterpreterCompatibilityGate.IsolationAudit,
                workspace.Files.Length,
                workspace.Files.Length,
                workspace.PrimaryRelativePath,
                "Step 19 isolation audit complete: source/install remain receipt-identical and only selected prepared assemblies differ."));

            return Pass(
                ExpressionInterpreterCompatibilityGate.IsolationAudit,
                "Step 19 prepared-payload isolation and structural audit passed.\n" +
                $"Source workspace receipt SHA-1s reverified: {workspace.Files.Length:N0}/{workspace.Files.Length:N0} ({sourceVerifiedBytes:N0} bytes)\n" +
                $"Original managed-install receipt SHA-1s reverified: {workspace.Files.Length:N0}/{workspace.Files.Length:N0} ({installVerifiedBytes:N0} bytes)\n" +
                $"Prepared files unchanged byte-for-byte: {unchangedPreparedFiles:N0}\n" +
                $"Prepared assemblies intentionally rewritten: {rewrittenPreparedFiles:N0}\n" +
                $"Total Compile sites forced to interpreter preference: {rewrite.TotalRewrittenSites:N0}\n" +
                "Every rewritten prepared assembly reopens with the explicit verified-workspace resolver: YES\n" +
                "No selected non-interpreted direct Compile target remains in rewritten outputs: YES\n" +
                "Original Step 12 install unchanged: YES\n" +
                $"Workspace-only dependency assemblies resolved by Cecil: {_workspaceResolvedAssemblies.Count:N0}\n" +
                $"Only launcher-private {WorkRootName} source/prepared files were written: YES\n" +
                "Fallback to runtime/system/live-install/network resolver paths: NO\n" +
                "Game assembly loaded/executed: NO\nSteam session consulted: NO\nNetwork attempted: NO\n" +
                "Step 19 result is a prepared managed compatibility payload only; Harmony/MonoMod, Reflection.Emit, dynamic Assembly.Load, native interop and game startup remain later evidence-driven subsystems.");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return Fail(ExpressionInterpreterCompatibilityGate.IsolationAudit, stage, ex);
        }
    }

    private static InterpreterProbeSnapshot RunInterpreterProbe()
    {
        var captured = 17;
        Expression<Func<int, int>> expression = value => value + captured;
        var interpreted = expression.Compile(preferInterpretation: true);
        var result = interpreted(25);
        if (result != 42)
            throw new InvalidDataException($"Expression interpreter probe returned {result}; expected 42.");

        return new InterpreterProbeSnapshot(
            result,
            RuntimeFeature.IsDynamicCodeSupported,
            RuntimeFeature.IsDynamicCodeCompiled);
    }

    private WorkspaceSnapshot RequireWorkspace()
        => _workspace ?? throw new InvalidOperationException("Gate A must pass before later Step 19 gates run.");

    private static SteamManagedInstallFile GetReceiptFile(WorkspaceSnapshot workspace, string relative)
        => workspace.Files.Single(file => NormalizeRelative(file.RelativePath).Equals(relative, StringComparison.OrdinalIgnoreCase));

    private static CompileSiteStats ScanCompileSites(ModuleDefinition module)
    {
        var stats = new MutableCompileSiteStats();
        foreach (var type in EnumerateTypes(module.Types))
        {
            foreach (var method in type.Methods)
            {
                if (!method.HasBody)
                    continue;

                var protectedEntryPoints = CollectStructuralEntryPoints(method);
                var instructions = method.Body.Instructions;
                for (var index = 0; index < instructions.Count; index++)
                {
                    var instruction = instructions[index];
                    if (instruction.OpCode.Code is not (Code.Call or Code.Callvirt) || instruction.Operand is not MethodReference target)
                        continue;
                    if (!IsExpressionCompileMethod(target))
                        continue;

                    stats.TotalDirectCompileSites++;
                    var source = $"{type.FullName}::{method.Name} @ IL_{instruction.Offset:x4}";
                    if (target.Parameters.Count == 0)
                    {
                        var hazard = GetParameterlessInsertionHazard(method, instruction, index, protectedEntryPoints);
                        if (hazard is not null)
                        {
                            stats.ParameterlessUnsafe++;
                            AddSample(stats.Samples, $"UNSAFE Compile() [{hazard}]: {source}");
                        }
                        else
                        {
                            stats.ParameterlessSafe++;
                            AddSample(stats.Samples, $"Compile() -> eligible Compile(true): {source}");
                        }
                        continue;
                    }

                    if (target.Parameters.Count == 1 && IsBoolean(target.Parameters[0].ParameterType))
                    {
                        if (index > 0 && TryGetInt32Constant(instructions[index - 1], out var value))
                        {
                            if (value == 0)
                            {
                                stats.LiteralFalse++;
                                AddSample(stats.Samples, $"Compile(false) -> eligible Compile(true): {source}");
                            }
                            else if (value == 1)
                            {
                                stats.LiteralTrue++;
                                AddSample(stats.Samples, $"Compile(true) already interpreter-preferred: {source}");
                            }
                            else
                            {
                                stats.DynamicBoolean++;
                                AddSample(stats.Samples, $"Compile(bool non-0/1 literal) left untouched: {source}");
                            }
                        }
                        else
                        {
                            stats.DynamicBoolean++;
                            AddSample(stats.Samples, $"Compile(dynamic bool) left untouched: {source}");
                        }
                    }
                }
            }
        }

        return stats.Freeze();
    }

    private static (int Parameterless, int LiteralFalse) ApplyPreferInterpretationRewrite(ModuleDefinition module)
    {
        var rewrittenParameterless = 0;
        var rewrittenLiteralFalse = 0;

        foreach (var type in EnumerateTypes(module.Types))
        {
            foreach (var method in type.Methods)
            {
                if (!method.HasBody)
                    continue;

                var protectedEntryPoints = CollectStructuralEntryPoints(method);
                var originalInstructions = method.Body.Instructions.ToArray();
                var il = method.Body.GetILProcessor();
                for (var index = 0; index < originalInstructions.Length; index++)
                {
                    var instruction = originalInstructions[index];
                    if (instruction.OpCode.Code is not (Code.Call or Code.Callvirt) || instruction.Operand is not MethodReference target)
                        continue;
                    if (!IsExpressionCompileMethod(target))
                        continue;

                    if (target.Parameters.Count == 0)
                    {
                        if (GetParameterlessInsertionHazard(method, instruction, index, protectedEntryPoints) is not null)
                            continue;

                        var overload = CreatePreferInterpretationOverload(module, target);
                        il.InsertBefore(instruction, il.Create(OpCodes.Ldc_I4_1));
                        instruction.Operand = overload;
                        rewrittenParameterless++;
                        continue;
                    }

                    if (target.Parameters.Count == 1 && IsBoolean(target.Parameters[0].ParameterType))
                    {
                        var previous = instruction.Previous;
                        if (previous is not null && TryGetInt32Constant(previous, out var value) && value == 0)
                        {
                            SetInt32ConstantToOnePreservingEncoding(previous);
                            rewrittenLiteralFalse++;
                        }
                    }
                }
            }
        }

        return (rewrittenParameterless, rewrittenLiteralFalse);
    }

    private static MethodReference CreatePreferInterpretationOverload(ModuleDefinition module, MethodReference original)
    {
        var overload = new MethodReference(original.Name, original.ReturnType, original.DeclaringType)
        {
            HasThis = original.HasThis,
            ExplicitThis = original.ExplicitThis,
            CallingConvention = original.CallingConvention,
        };
        overload.Parameters.Add(new ParameterDefinition(module.TypeSystem.Boolean));
        return overload;
    }

    private static bool IsExpressionCompileMethod(MethodReference target)
    {
        if (!target.Name.Equals("Compile", StringComparison.Ordinal) || !target.HasThis)
            return false;

        var type = target.DeclaringType.FullName;
        if (!type.Equals("System.Linq.Expressions.LambdaExpression", StringComparison.Ordinal) &&
            !type.StartsWith("System.Linq.Expressions.Expression`1", StringComparison.Ordinal))
        {
            return false;
        }

        var scope = GetScopeName(target.DeclaringType.Scope);
        return scope.Equals("System.Linq.Expressions", StringComparison.OrdinalIgnoreCase) ||
               scope.Equals("System.Core", StringComparison.OrdinalIgnoreCase) ||
               string.IsNullOrWhiteSpace(scope);
    }

    private static bool IsBoolean(TypeReference type)
        => type.MetadataType == MetadataType.Boolean || type.FullName.Equals("System.Boolean", StringComparison.Ordinal);

    private static string GetScopeName(IMetadataScope? scope)
        => scope switch
        {
            AssemblyNameReference assembly => assembly.Name ?? string.Empty,
            ModuleDefinition module => module.Assembly?.Name?.Name ?? module.Name,
            ModuleReference moduleReference => moduleReference.Name ?? string.Empty,
            _ => scope?.Name ?? string.Empty,
        };

    private static string? GetParameterlessInsertionHazard(
        MethodDefinition method,
        Instruction instruction,
        int instructionIndex,
        HashSet<Instruction> protectedEntryPoints)
    {
        var instructions = method.Body.Instructions;
        if (instructionIndex > 0 && IsPrefixInstruction(instructions[instructionIndex - 1]))
            return "immediate IL prefix";
        if (protectedEntryPoints.Contains(instruction))
            return "branch/exception-handler entry point";
        if (HasCrossingShortBranch(method, instructionIndex))
            return "crossing short branch would change displacement";
        return null;
    }

    private static bool HasCrossingShortBranch(MethodDefinition method, int insertionIndex)
    {
        var instructions = method.Body.Instructions;
        var indexByInstruction = new Dictionary<Instruction, int>(instructions.Count);
        for (var index = 0; index < instructions.Count; index++)
            indexByInstruction[instructions[index]] = index;

        for (var sourceIndex = 0; sourceIndex < instructions.Count; sourceIndex++)
        {
            var branch = instructions[sourceIndex];
            if (branch.OpCode.OperandType != OperandType.ShortInlineBrTarget || branch.Operand is not Instruction target)
                continue;
            if (!indexByInstruction.TryGetValue(target, out var targetIndex))
                return true;

            if ((sourceIndex < insertionIndex && targetIndex >= insertionIndex) ||
                (sourceIndex >= insertionIndex && targetIndex < insertionIndex))
            {
                return true;
            }
        }
        return false;
    }

    private static void SetInt32ConstantToOnePreservingEncoding(Instruction instruction)
    {
        switch (instruction.OpCode.Code)
        {
            case Code.Ldc_I4_0:
                instruction.OpCode = OpCodes.Ldc_I4_1;
                instruction.Operand = null;
                return;
            case Code.Ldc_I4_S:
                instruction.Operand = (sbyte)1;
                return;
            case Code.Ldc_I4:
                instruction.Operand = 1;
                return;
            default:
                throw new InvalidDataException($"Expected an integer-zero literal encoding, got {instruction.OpCode.Code}.");
        }
    }

    private static bool IsPrefixInstruction(Instruction instruction)
        => instruction.OpCode.Code is Code.Constrained or Code.No or Code.Readonly or Code.Tail or Code.Unaligned or Code.Volatile;

    private static HashSet<Instruction> CollectStructuralEntryPoints(MethodDefinition method)
    {
        var targets = new HashSet<Instruction>();
        foreach (var instruction in method.Body.Instructions)
        {
            if (instruction.Operand is Instruction target)
                targets.Add(target);
            else if (instruction.Operand is Instruction[] multiple)
            {
                foreach (var value in multiple)
                    targets.Add(value);
            }
        }

        foreach (var handler in method.Body.ExceptionHandlers)
        {
            AddIfNotNull(targets, handler.TryStart);
            AddIfNotNull(targets, handler.TryEnd);
            AddIfNotNull(targets, handler.HandlerStart);
            AddIfNotNull(targets, handler.HandlerEnd);
            AddIfNotNull(targets, handler.FilterStart);
        }
        return targets;
    }

    private static void AddIfNotNull(HashSet<Instruction> values, Instruction? value)
    {
        if (value is not null)
            values.Add(value);
    }

    private static bool TryGetInt32Constant(Instruction instruction, out int value)
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
            case Code.Ldc_I4_S:
                value = instruction.Operand is sbyte shortValue ? shortValue : Convert.ToInt32(instruction.Operand);
                return true;
            case Code.Ldc_I4:
                value = instruction.Operand is int intValue ? intValue : Convert.ToInt32(instruction.Operand);
                return true;
            default:
                value = 0;
                return false;
        }
    }

    private static bool IsStrongNamed(ModuleDefinition module)
    {
        if ((module.Attributes & ModuleAttributes.StrongNameSigned) != 0)
            return true;
        return module.Assembly?.Name?.PublicKey is { Length: > 0 };
    }

    private static ModuleFingerprint Fingerprint(ModuleDefinition module)
    {
        var types = EnumerateTypes(module.Types).ToArray();
        var methods = types.SelectMany(type => type.Methods).ToArray();
        var bodies = methods.Where(method => method.HasBody).ToArray();
        return new ModuleFingerprint(
            module.Assembly?.Name?.FullName ?? "<netmodule>",
            module.Name,
            module.RuntimeVersion,
            types.Length,
            methods.Length,
            types.Sum(type => type.Fields.Count),
            types.Sum(type => type.Properties.Count),
            types.Sum(type => type.Events.Count),
            module.AssemblyReferences.Count,
            module.ModuleReferences.Count,
            bodies.Length,
            bodies.Sum(method => method.Body.ExceptionHandlers.Count),
            bodies.Sum(method => method.Body.Instructions.Count));
    }

    private static IEnumerable<TypeDefinition> EnumerateTypes(IEnumerable<TypeDefinition> roots)
    {
        foreach (var type in roots)
        {
            yield return type;
            foreach (var nested in EnumerateTypes(type.NestedTypes))
                yield return nested;
        }
    }

    private static HashSet<string> EnumerateRelativeFiles(string root)
        => Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
            .Select(path => Path.GetRelativePath(root, path).Replace(Path.DirectorySeparatorChar, '/'))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

    private static void AddSample(List<string> samples, string value)
    {
        if (samples.Count >= SampleLimit)
            return;
        if (!samples.Contains(value, StringComparer.Ordinal))
            samples.Add(value);
    }

    private static string FormatLineSamples(IReadOnlyList<string> values)
        => values.Count == 0 ? "• none" : string.Join("\n", values.Select(value => $"• {value}"));

    private static void EnsureWorkspaceResolverBound(ModuleDefinition module, WorkspaceOnlyAssemblyResolver resolver)
    {
        if (!ReferenceEquals(module.AssemblyResolver, resolver))
            throw new InvalidDataException("Cecil module is not using the Step 19 verified-workspace assembly resolver.");
        if (module.MetadataResolver is not MetadataResolver metadataResolver ||
            !ReferenceEquals(metadataResolver.AssemblyResolver, resolver))
        {
            throw new InvalidDataException("Cecil module is not using the Step 19 verified-workspace metadata resolver.");
        }
    }

    private static ModuleDefinition ReadModuleWithWorkspaceResolver(
        string path,
        IAssemblyResolver resolver,
        ReadingMode readingMode)
    {
        var metadataResolver = new MetadataResolver(resolver);
        return ModuleDefinition.ReadModule(path, new ReaderParameters
        {
            ReadSymbols = false,
            ReadingMode = readingMode,
            InMemory = true,
            AssemblyResolver = resolver,
            MetadataResolver = metadataResolver,
        });
    }

    private static WorkspaceOnlyAssemblyResolver CreateWorkspaceResolver(WorkspaceSnapshot workspace)
    {
        var primaryDirectory = Path.GetDirectoryName(ResolveChildPath(workspace.SourceRoot, workspace.PrimaryRelativePath))
            ?? throw new InvalidDataException("The Step 19 primary assembly has no workspace directory.");
        var trustedFiles = workspace.Files.ToDictionary(
            file => Path.GetFullPath(ResolveChildPath(workspace.SourceRoot, NormalizeRelative(file.RelativePath))),
            file => file.Sha1Hex,
            StringComparer.OrdinalIgnoreCase);
        return new WorkspaceOnlyAssemblyResolver(workspace.SourceRoot, primaryDirectory, trustedFiles);
    }

    private void RecordWorkspaceResolutions(WorkspaceOnlyAssemblyResolver resolver)
    {
        foreach (var name in resolver.ResolvedAssemblyNames)
            _workspaceResolvedAssemblies.Add(name);
    }

    private sealed class WorkspaceOnlyAssemblyResolver : IAssemblyResolver
    {
        private readonly string _rootPrefix;
        private readonly string _primaryDirectory;
        private readonly Dictionary<string, AssemblyDefinition> _cache = new(StringComparer.Ordinal);
        private readonly Dictionary<string, string> _trustedFileSha1;
        private readonly SortedSet<string> _resolvedAssemblyNames = new(StringComparer.Ordinal);
        private WorkspaceAssemblyCandidate[]? _catalog;
        private bool _disposed;

        public WorkspaceOnlyAssemblyResolver(
            string root,
            string primaryDirectory,
            IReadOnlyDictionary<string, string> trustedFileSha1)
        {
            var fullRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar);
            _rootPrefix = fullRoot + Path.DirectorySeparatorChar;
            _primaryDirectory = Path.GetFullPath(primaryDirectory);
            if (!_primaryDirectory.StartsWith(_rootPrefix, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("The Step 19 primary assembly directory escaped the workspace resolver root.");

            _trustedFileSha1 = trustedFileSha1.ToDictionary(
                pair => Path.GetFullPath(pair.Key),
                pair => pair.Value,
                StringComparer.OrdinalIgnoreCase);
            if (_trustedFileSha1.Count == 0)
                throw new InvalidDataException("The Step 19 workspace-only resolver has no trusted receipt-backed search scope.");
        }

        public IReadOnlyCollection<string> ResolvedAssemblyNames => _resolvedAssemblyNames;

        public AssemblyDefinition Resolve(AssemblyNameReference name)
            => Resolve(name, new ReaderParameters());

        public AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(WorkspaceOnlyAssemblyResolver));
            ArgumentNullException.ThrowIfNull(name);
            ArgumentNullException.ThrowIfNull(parameters);

            if (_cache.TryGetValue(name.FullName, out var cached))
                return cached;

            var catalog = GetOrBuildCatalog();
            var simpleMatches = catalog
                .Where(candidate => candidate.Name.Equals(name.Name, StringComparison.Ordinal))
                .ToArray();
            var exactMatches = simpleMatches
                .Where(candidate => AssemblyIdentityMatches(name, candidate))
                .OrderBy(candidate => IsPrimaryDirectory(candidate.Path) ? 0 : 1)
                .ThenBy(candidate => candidate.Path, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            var matches = exactMatches;
            var versionRelaxed = false;
            if (matches.Length == 0)
            {
                var versionCompatible = simpleMatches
                    .Where(candidate => AssemblyIdentityMatchesIgnoringVersion(name, candidate))
                    .OrderBy(candidate => IsPrimaryDirectory(candidate.Path) ? 0 : 1)
                    .ThenBy(candidate => candidate.Path, StringComparer.OrdinalIgnoreCase)
                    .ToArray();
                var distinctCompatibleIdentities = versionCompatible
                    .Select(candidate => candidate.FullName)
                    .Distinct(StringComparer.Ordinal)
                    .ToArray();
                if (distinctCompatibleIdentities.Length == 1)
                {
                    matches = versionCompatible;
                    versionRelaxed = matches.Length > 0;
                }
                else if (distinctCompatibleIdentities.Length > 1)
                {
                    throw new InvalidDataException(
                        $"Step 19 workspace identity resolver found multiple version-distinct identity candidates for '{name.FullName}': " +
                        string.Join(" | ", versionCompatible.Select(candidate => $"{candidate.FullName} @ {Path.GetFileName(candidate.Path)}")));
                }
            }

            if (matches.Length == 0)
            {
                var available = simpleMatches.Length == 0
                    ? "none with that simple name"
                    : string.Join(" | ", simpleMatches.Select(candidate => $"{candidate.FullName} @ {Path.GetFileName(candidate.Path)}"));
                throw new InvalidDataException(
                    $"Step 19 workspace identity resolver could not match requested assembly '{name.FullName}'. Workspace identity candidates: {available}.");
            }

            var distinctHashes = matches
                .Select(candidate => candidate.ExpectedSha1)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
            if (distinctHashes.Length > 1)
            {
                throw new InvalidDataException(
                    $"Step 19 workspace identity resolver found multiple byte-distinct files for '{name.FullName}': " +
                    string.Join(" | ", matches.Select(candidate => candidate.Path)));
            }

            var selected = matches[0];
            VerifyTrustedFileImmediatelyBeforeOpen(selected.Path, selected.ExpectedSha1);

            var metadataResolver = new MetadataResolver(this);
            var reader = new ReaderParameters
            {
                ReadSymbols = false,
                ReadingMode = ReadingMode.Deferred,
                InMemory = true,
                AssemblyResolver = this,
                MetadataResolver = metadataResolver,
            };
            var module = ModuleDefinition.ReadModule(selected.Path, reader);
            var assembly = module.Assembly;
            var openedCandidate = assembly is null
                ? null
                : ToCandidate(selected.Path, selected.ExpectedSha1, assembly.Name);
            if (openedCandidate is null || openedCandidate != selected)
            {
                module.Dispose();
                throw new InvalidDataException(
                    $"Step 19 workspace dependency identity changed between catalog and Cecil open for '{name.FullName}'.");
            }

            _cache[name.FullName] = assembly;
            _resolvedAssemblyNames.Add(versionRelaxed
                ? $"{name.FullName} -> {assembly.Name.FullName} [workspace version-unified]"
                : assembly.Name.FullName);
            return assembly;
        }

        private WorkspaceAssemblyCandidate[] GetOrBuildCatalog()
        {
            if (_catalog is not null)
                return _catalog;

            var candidates = new List<WorkspaceAssemblyCandidate>();
            foreach (var pair in _trustedFileSha1.OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase))
            {
                var path = pair.Key;
                var extension = Path.GetExtension(path);
                if (!extension.Equals(".dll", StringComparison.OrdinalIgnoreCase) &&
                    !extension.Equals(".exe", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!IsWithinRoot(path) || !File.Exists(path))
                    continue;

                VerifyTrustedFileImmediatelyBeforeOpen(path, pair.Value);
                try
                {
                    using var probe = ModuleDefinition.ReadModule(path, new ReaderParameters
                    {
                        ReadSymbols = false,
                        ReadingMode = ReadingMode.Deferred,
                        InMemory = true,
                        AssemblyResolver = RejectingCatalogProbeResolver.Instance,
                        MetadataResolver = RejectingCatalogProbeResolver.Instance,
                    });
                    if (probe.Assembly?.Name is { } identity)
                        candidates.Add(ToCandidate(path, pair.Value, identity));
                }
                catch (BadImageFormatException)
                {
                    // Receipt-backed .dll/.exe filename candidates can include native PEs.
                }
            }

            if (candidates.Count == 0)
                throw new InvalidDataException("The Step 19 workspace identity catalog contains no managed assemblies.");

            _catalog = candidates.ToArray();
            return _catalog;
        }

        private void VerifyTrustedFileImmediatelyBeforeOpen(string path, string expectedSha1)
        {
            var fullPath = Path.GetFullPath(path);
            if (!IsWithinRoot(fullPath) || !_trustedFileSha1.TryGetValue(fullPath, out var trustedSha1))
                throw new InvalidDataException($"Step 19 attempted to resolve outside the trusted workspace: {path}");
            if (!trustedSha1.Equals(expectedSha1, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException($"Step 19 workspace catalog trust metadata changed for {Path.GetFileName(path)}.");

            var actualSha1 = ComputeSha1Hex(fullPath);
            if (!actualSha1.Equals(expectedSha1, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException(
                    $"Step 19 workspace dependency changed after Gate A SHA-1 verification: {Path.GetFileName(path)}");
            }
        }

        private bool IsWithinRoot(string path)
            => Path.GetFullPath(path).StartsWith(_rootPrefix, StringComparison.OrdinalIgnoreCase);

        private bool IsPrimaryDirectory(string path)
            => string.Equals(Path.GetDirectoryName(Path.GetFullPath(path)), _primaryDirectory, StringComparison.OrdinalIgnoreCase);

        private static WorkspaceAssemblyCandidate ToCandidate(string path, string sha1, AssemblyNameReference identity)
            => new(
                path,
                sha1,
                identity.Name,
                identity.Version,
                identity.Culture ?? string.Empty,
                identity.PublicKeyToken is { Length: > 0 } token ? Convert.ToHexString(token).ToLowerInvariant() : string.Empty,
                identity.FullName);

        private static bool AssemblyIdentityMatches(AssemblyNameReference requested, WorkspaceAssemblyCandidate candidate)
            => AssemblyIdentityMatchesIgnoringVersion(requested, candidate) &&
               (requested.Version is null || candidate.Version == requested.Version);

        private static bool AssemblyIdentityMatchesIgnoringVersion(AssemblyNameReference requested, WorkspaceAssemblyCandidate candidate)
        {
            if (!candidate.Name.Equals(requested.Name, StringComparison.Ordinal))
                return false;
            if (!string.Equals(candidate.Culture, requested.Culture ?? string.Empty, StringComparison.OrdinalIgnoreCase))
                return false;
            var requestedToken = requested.PublicKeyToken is { Length: > 0 } token
                ? Convert.ToHexString(token).ToLowerInvariant()
                : string.Empty;
            return candidate.PublicKeyToken.Equals(requestedToken, StringComparison.OrdinalIgnoreCase);
        }

        public void Dispose()
        {
            if (_disposed)
                return;
            _disposed = true;
            foreach (var assembly in _cache.Values.Distinct())
                assembly.Dispose();
            _cache.Clear();
            _catalog = null;
        }

        private sealed record WorkspaceAssemblyCandidate(
            string Path,
            string ExpectedSha1,
            string Name,
            Version Version,
            string Culture,
            string PublicKeyToken,
            string FullName);

        private sealed class RejectingCatalogProbeResolver : IAssemblyResolver, IMetadataResolver
        {
            public static RejectingCatalogProbeResolver Instance { get; } = new();
            private RejectingCatalogProbeResolver() { }
            public AssemblyDefinition Resolve(AssemblyNameReference name)
                => throw new InvalidOperationException($"Step 19 identity catalog unexpectedly attempted dependency resolution while probing {name.FullName}.");
            public AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
                => Resolve(name);
            TypeDefinition IMetadataResolver.Resolve(TypeReference type)
                => throw new InvalidOperationException($"Step 19 identity catalog unexpectedly attempted type resolution while probing {type.FullName}.");
            FieldDefinition IMetadataResolver.Resolve(FieldReference field)
                => throw new InvalidOperationException($"Step 19 identity catalog unexpectedly attempted field resolution while probing {field.FullName}.");
            MethodDefinition IMetadataResolver.Resolve(MethodReference method)
                => throw new InvalidOperationException($"Step 19 identity catalog unexpectedly attempted method resolution while probing {method.FullName}.");
            public void Dispose() { }
        }
    }

    private async Task<SteamManagedInstallReceipt> ReadReceiptAsync(string managedRoot, CancellationToken cancellationToken)
    {
        var receiptPath = Path.Combine(managedRoot, SteamManagedInstallReceipt.FileName);
        await using var stream = new FileStream(
            receiptPath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            16 * 1024,
            FileOptions.Asynchronous | FileOptions.SequentialScan);
        return await JsonSerializer.DeserializeAsync(
                   stream,
                   SteamManagedInstallJsonContext.Default.SteamManagedInstallReceipt,
                   cancellationToken)
               .ConfigureAwait(false)
               ?? throw new InvalidDataException("The verified Step 12 receipt unexpectedly deserialized to null.");
    }

    private static void ValidateReceiptSnapshot(SteamManagedInstallReceipt receipt, SteamOfflineInstallResult offline)
    {
        if (!offline.ReceiptStructurallyValid || !offline.ExactManagedTreeVerified)
            throw new InvalidDataException("OfflineReady did not include structurally valid receipt + exact-tree proof.");
        if (receipt.SchemaVersion != SteamManagedInstallReceipt.CurrentSchemaVersion ||
            receipt.AppId != SteamOfflineInstallInspection.TargetAppId ||
            offline.DepotId is null || receipt.DepotId != offline.DepotId.Value ||
            offline.InstalledManifestId is null || receipt.ManifestId != offline.InstalledManifestId.Value ||
            !string.Equals(receipt.Branch, offline.Branch, StringComparison.Ordinal) ||
            receipt.Files is null || receipt.Files.Count == 0 || receipt.Files.Count != offline.PlannedFiles)
        {
            throw new InvalidDataException("The Step 12 receipt changed or became inconsistent after OfflineReady was proven.");
        }

        var unique = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var file in receipt.Files)
        {
            if (file is null ||
                !SteamSingleFileTargetSelector.IsSafeRelativePath(file.RelativePath) ||
                file.Length < 0 ||
                file.Sha1Hex.Length != 40 ||
                !file.Sha1Hex.All(Uri.IsHexDigit) ||
                !unique.Add(NormalizeRelative(file.RelativePath)))
            {
                throw new InvalidDataException("The Step 12 receipt contains an invalid or duplicate file entry.");
            }
        }
    }

    private void PrepareFreshWorkRoot()
    {
        BestEffortDeleteWorkRoot(throwOnFailure: true);
        Directory.CreateDirectory(_workRoot);
    }

    private void BestEffortDeleteWorkRoot(bool throwOnFailure = false)
    {
        try
        {
            var root = Path.GetFullPath(_launcherDataRoot).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            var work = Path.GetFullPath(_workRoot);
            if (!work.StartsWith(root, StringComparison.Ordinal) || work.Equals(_launcherDataRoot, StringComparison.Ordinal))
                throw new InvalidOperationException("Step 19 work root escaped launcher-private storage.");
            if (Directory.Exists(work))
                Directory.Delete(work, recursive: true);
        }
        catch when (!throwOnFailure)
        {
            // A later Gate A recreates the Step 19 workspace from scratch.
        }
    }

    private static void DeleteIfExists(string path)
    {
        if (File.Exists(path))
            File.Delete(path);
    }

    private static async Task CopyFileAsync(string source, string destination, CancellationToken cancellationToken)
    {
        await using var input = new FileStream(
            source,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            256 * 1024,
            FileOptions.Asynchronous | FileOptions.SequentialScan);
        await using var output = new FileStream(
            destination,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            256 * 1024,
            FileOptions.Asynchronous | FileOptions.SequentialScan);
        await input.CopyToAsync(output, 256 * 1024, cancellationToken).ConfigureAwait(false);
        await output.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    private static string ResolveChildPath(string root, string relativePath)
    {
        if (!SteamSingleFileTargetSelector.IsSafeRelativePath(relativePath))
            throw new InvalidDataException($"Unsafe relative path: {relativePath}");
        var rootFull = Path.GetFullPath(root);
        var child = Path.GetFullPath(Path.Combine(rootFull, relativePath.Replace('/', Path.DirectorySeparatorChar)));
        var prefix = rootFull.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        if (!child.StartsWith(prefix, StringComparison.Ordinal))
            throw new InvalidDataException($"Path escaped the managed root: {relativePath}");
        return child;
    }

    private static string NormalizeRelative(string value)
        => value.Replace('\\', '/').TrimStart('/');

    private static async Task<string> ComputeSha1HexAsync(string path, CancellationToken cancellationToken)
    {
        await using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            256 * 1024,
            FileOptions.Asynchronous | FileOptions.SequentialScan);
        using var sha1 = SHA1.Create();
        var hash = await sha1.ComputeHashAsync(stream, cancellationToken).ConfigureAwait(false);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static string ComputeSha1Hex(string path)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 256 * 1024, FileOptions.SequentialScan);
        using var sha1 = SHA1.Create();
        return Convert.ToHexString(sha1.ComputeHash(stream)).ToLowerInvariant();
    }

    private static bool IsManagedAssemblyFileName(string path)
    {
        var extension = Path.GetExtension(path);
        return extension.Equals(".dll", StringComparison.OrdinalIgnoreCase) ||
               extension.Equals(".exe", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsMacOsArm64ManagedPath(string path)
        => ("/" + NormalizeRelative(path)).Contains("/data_sts2_macos_arm64/", StringComparison.OrdinalIgnoreCase);

    private static bool IsMacOsX8664ManagedPath(string path)
        => ("/" + NormalizeRelative(path)).Contains("/data_sts2_macos_x86_64/", StringComparison.OrdinalIgnoreCase);

    private static bool IsPrimaryArm64StS2Path(string path)
        => ("/" + NormalizeRelative(path)).EndsWith("/data_sts2_macos_arm64/sts2.dll", StringComparison.OrdinalIgnoreCase);

    private static ExpressionInterpreterCompatibilityGateResult Pass(ExpressionInterpreterCompatibilityGate gate, string detail)
        => new(gate, true, detail);

    private static ExpressionInterpreterCompatibilityGateResult Fail(
        ExpressionInterpreterCompatibilityGate gate,
        string stage,
        Exception ex)
        => new(gate, false, $"Stage: {stage}\n{ex.GetType().Name}: {ex.Message}");

    private sealed class CallbackProgress<T> : IProgress<T>
    {
        private readonly Action<T> _callback;
        public CallbackProgress(Action<T> callback) => _callback = callback ?? throw new ArgumentNullException(nameof(callback));
        public void Report(T value) => _callback(value);
    }

    private sealed class MutableCompileSiteStats
    {
        public long TotalDirectCompileSites { get; set; }
        public long ParameterlessSafe { get; set; }
        public long ParameterlessUnsafe { get; set; }
        public long LiteralFalse { get; set; }
        public long LiteralTrue { get; set; }
        public long DynamicBoolean { get; set; }
        public List<string> Samples { get; } = [];

        public CompileSiteStats Freeze() => new(
            TotalDirectCompileSites,
            ParameterlessSafe,
            ParameterlessUnsafe,
            LiteralFalse,
            LiteralTrue,
            DynamicBoolean,
            Samples.ToArray());
    }

    private sealed record CompileSiteStats(
        long TotalDirectCompileSites,
        long ParameterlessSafe,
        long ParameterlessUnsafe,
        long LiteralFalse,
        long LiteralTrue,
        long DynamicBoolean,
        IReadOnlyList<string> Samples)
    {
        public long SupportedRewrites => checked(ParameterlessSafe + LiteralFalse);
    }

    private sealed record InterpreterProbeSnapshot(
        int Result,
        bool DynamicCodeSupported,
        bool DynamicCodeCompiled);

    private sealed record WorkspaceSnapshot(
        string ManagedRoot,
        string SourceRoot,
        SteamManagedInstallReceipt Receipt,
        SteamOfflineInstallResult Offline,
        SteamManagedInstallFile[] Files,
        string PrimaryRelativePath,
        int AllManagedCandidates,
        int Arm64Candidates,
        int X8664Candidates,
        int SharedCandidates,
        ulong ScopeBytes,
        InterpreterProbeSnapshot InterpreterProbe);

    private sealed record TargetAssemblySnapshot(
        string RelativePath,
        string AssemblyFullName,
        bool StrongNamed,
        CompileSiteStats Stats);

    private sealed record DiscoverySnapshot(
        int ParsedModules,
        int NonManagedCandidates,
        TargetAssemblySnapshot[] Assemblies,
        TargetAssemblySnapshot[] WritableTargets,
        long ParameterlessSafe,
        long LiteralFalse,
        long ExistingTrue,
        long DynamicBoolean,
        long UnsafeParameterless,
        long StrongNamedSupported,
        long WritableSupported,
        long PrimarySupported,
        IReadOnlyList<string> Samples);

    private sealed record PreparedAssemblySnapshot(
        string RelativePath,
        string SourceSha1,
        string PreparedSha1,
        int RewrittenParameterless,
        int RewrittenLiteralFalse,
        CompileSiteStats Before,
        CompileSiteStats After,
        ModuleFingerprint BeforeFingerprint,
        ModuleFingerprint AfterFingerprint);

    private sealed record ModuleFingerprint(
        string AssemblyFullName,
        string ModuleName,
        string RuntimeVersion,
        int TypeCount,
        int MethodCount,
        int FieldCount,
        int PropertyCount,
        int EventCount,
        int AssemblyReferenceCount,
        int ModuleReferenceCount,
        int MethodBodyCount,
        int ExceptionHandlerCount,
        int InstructionCount)
    {
        public bool MetadataEquivalentTo(ModuleFingerprint other)
            => AssemblyFullName == other.AssemblyFullName &&
               ModuleName == other.ModuleName &&
               RuntimeVersion == other.RuntimeVersion &&
               TypeCount == other.TypeCount &&
               MethodCount == other.MethodCount &&
               FieldCount == other.FieldCount &&
               PropertyCount == other.PropertyCount &&
               EventCount == other.EventCount &&
               AssemblyReferenceCount == other.AssemblyReferenceCount &&
               ModuleReferenceCount == other.ModuleReferenceCount &&
               MethodBodyCount == other.MethodBodyCount &&
               ExceptionHandlerCount == other.ExceptionHandlerCount;
    }

    private sealed record RewriteSnapshot(
        string PreparedRoot,
        PreparedAssemblySnapshot[] Assemblies,
        long TotalRewrittenSites,
        long RewrittenParameterless,
        long RewrittenLiteralFalse);
}
