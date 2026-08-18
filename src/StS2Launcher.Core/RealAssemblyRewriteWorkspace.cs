using System.Security.Cryptography;
using System.Text.Json;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace StS2Launcher.Core;

/// <summary>
/// Step 18 boundary. Creates a launcher-private compatibility workspace from the
/// receipt-backed macOS-arm64 managed payload, proves Cecil can write/reopen a
/// real copied StS2 assembly, and applies one semantics-neutral NOP insertion to
/// a copy only. The Step 12 managed install remains read-only throughout.
/// </summary>
public sealed class RealAssemblyRewriteWorkspace
{
    public const string WorkRootName = "Step18-RealAssemblyRewrite";
    public const string SourceRootName = "source";
    public const string RoundTripRootName = "roundtrip";
    public const string RewrittenRootName = "rewritten";

    private readonly string _launcherDataRoot;
    private readonly string _workRoot;
    private readonly SteamOfflineInstallInspection _offlineInspection;
    private WorkspaceSnapshot? _workspace;
    private RewriteSnapshot? _rewrite;
    private readonly SortedSet<string> _workspaceResolvedAssemblies = new(StringComparer.Ordinal);

    public RealAssemblyRewriteWorkspace(string launcherDataRoot)
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
        _rewrite = null;
        _workspaceResolvedAssemblies.Clear();
    }

    public async Task<RealAssemblyRewriteGateResult> RunWorkspaceCloneAsync(
        IProgress<RealAssemblyRewriteProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Reset();
            cancellationToken.ThrowIfCancellationRequested();
            progress?.Report(new RealAssemblyRewriteProgress(
                RealAssemblyRewriteGate.WorkspaceClone,
                0,
                0,
                null,
                "Re-proving OfflineReady before creating the launcher-private Step 18 compatibility workspace…"));

            IProgress<SteamOfflineInstallProgress>? offlineProgress = progress is null
                ? null
                : new CallbackProgress<SteamOfflineInstallProgress>(value =>
                    progress.Report(new RealAssemblyRewriteProgress(
                        RealAssemblyRewriteGate.WorkspaceClone,
                        value.CompletedFiles,
                        value.TotalFiles,
                        value.CurrentFile,
                        $"OfflineReady precondition — {value.Message} ({value.CompletedBytes:N0}/{value.TotalBytes:N0} bytes)")));

            var offline = await _offlineInspection.RunAsync(offlineProgress, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            if (offline.Outcome == SteamOfflineInstallOutcome.Cancelled)
                throw new OperationCanceledException("Step 18 OfflineReady precondition was cancelled.", cancellationToken);
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
                progress?.Report(new RealAssemblyRewriteProgress(
                    RealAssemblyRewriteGate.WorkspaceClone,
                    index,
                    scopeFiles.Length,
                    relative,
                    $"Copying receipt-backed ARM64/shared managed payload into launcher-private scratch storage ({copiedBytes:N0}/{scopeBytes:N0} bytes copied)…"));

                var sourcePath = ResolveChildPath(managedRoot, relative);
                var destinationPath = ResolveChildPath(sourceRoot, relative);
                Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
                await CopyFileAsync(sourcePath, destinationPath, cancellationToken).ConfigureAwait(false);

                var info = new FileInfo(destinationPath);
                if (info.Length != file.Length)
                    throw new InvalidDataException($"Workspace copy length mismatch for {relative}: {info.Length} != {file.Length}.");
                var hash = await ComputeSha1HexAsync(destinationPath, cancellationToken).ConfigureAwait(false);
                if (!hash.Equals(file.Sha1Hex, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException($"Workspace copy SHA-1 mismatch for {relative}.");
                checked { copiedBytes += (ulong)file.Length; }
            }

            if (copiedBytes != scopeBytes)
                throw new InvalidDataException("Step 18 workspace byte accounting did not cover the complete selected managed scope.");

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
                scopeBytes);

            progress?.Report(new RealAssemblyRewriteProgress(
                RealAssemblyRewriteGate.WorkspaceClone,
                scopeFiles.Length,
                scopeFiles.Length,
                _workspace.PrimaryRelativePath,
                "Launcher-private compatibility workspace clone complete; every source copy matches the trusted receipt SHA-1."));

            return Pass(
                RealAssemblyRewriteGate.WorkspaceClone,
                "Receipt-backed ARM64 compatibility workspace created without modifying the managed install.\n" +
                $"OfflineReady precondition: YES ({offline.VerifiedFiles:N0}/{offline.PlannedFiles:N0} files)\n" +
                $"All depot .dll/.exe filename candidates: {allManaged.Length:N0}\n" +
                $"macOS arm64 candidates copied: {arm64.Length:N0}\n" +
                $"Architecture-neutral candidates copied: {shared.Length:N0}\n" +
                $"macOS x86_64 duplicates excluded from rewrite workspace: {x86.Length:N0}\n" +
                $"Workspace source copies: {scopeFiles.Length:N0} files / {scopeBytes:N0} bytes\n" +
                $"Primary assembly: {_workspace.PrimaryRelativePath}\n" +
                $"Workspace root: {WorkRootName}/{SourceRootName}\n" +
                "Every workspace source copy receipt SHA-1 verified: YES\n" +
                "Steam session consulted: NO\nNetwork attempted by Step 18: NO\nReal managed install modified: NO");
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
            return Fail(RealAssemblyRewriteGate.WorkspaceClone, ex);
        }
    }

    public RealAssemblyRewriteGateResult RunPrimaryRoundTrip()
    {
        try
        {
            var workspace = RequireWorkspace();
            var sourcePath = ResolveChildPath(workspace.SourceRoot, workspace.PrimaryRelativePath);
            var roundTripRoot = Path.Combine(_workRoot, RoundTripRootName);
            var outputPath = ResolveChildPath(roundTripRoot, workspace.PrimaryRelativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
            DeleteIfExists(outputPath);

            var sourceHashBefore = ComputeSha1Hex(sourcePath);
            var expectedHash = GetReceiptFile(workspace, workspace.PrimaryRelativePath).Sha1Hex;
            if (!sourceHashBefore.Equals(expectedHash, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Primary workspace source no longer matches its receipt SHA-1 before Cecil round-trip.");

            ModuleFingerprint before;
            using (var resolver = CreateWorkspaceResolver(workspace))
            using (var module = ReadModuleImmediate(sourcePath, resolver))
            {
                before = Fingerprint(module);
                module.Write(outputPath, new WriterParameters { WriteSymbols = false });
                RecordWorkspaceResolutions(resolver);
            }

            if (!File.Exists(outputPath) || new FileInfo(outputPath).Length <= 0)
                throw new InvalidDataException("Cecil returned without creating a non-empty real-assembly round-trip output.");

            ModuleFingerprint after;
            using (var reopened = ReadModuleImmediate(outputPath))
                after = Fingerprint(reopened);

            if (before != after)
                throw new InvalidDataException($"Real-assembly round-trip metadata fingerprint changed. Before={before}; After={after}.");

            var sourceHashAfter = ComputeSha1Hex(sourcePath);
            if (!sourceHashAfter.Equals(expectedHash, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Primary workspace source changed during Cecil round-trip.");

            return Pass(
                RealAssemblyRewriteGate.PrimaryRoundTrip,
                "Mono.Cecil wrote and reopened a REAL StS2 assembly copy using only the verified Step 18 workspace for any writer-required dependency resolution.\n" +
                $"Source: {WorkRootName}/{SourceRootName}/{workspace.PrimaryRelativePath}\n" +
                $"Output: {WorkRootName}/{RoundTripRootName}/{workspace.PrimaryRelativePath}\n" +
                $"Assembly: {before.AssemblyName} {before.AssemblyVersion}\n" +
                $"Runtime: {before.RuntimeVersion}\n" +
                $"Types/methods: {before.TypeCount:N0}/{before.MethodCount:N0}\n" +
                $"Assembly/module references: {before.AssemblyReferenceCount:N0}/{before.ModuleReferenceCount:N0}\n" +
                "Logical metadata fingerprint preserved after write/reopen: YES\n" +
                "Workspace source receipt SHA-1 preserved: YES\n" +
                $"Workspace-only dependency resolutions observed: {_workspaceResolvedAssemblies.Count:N0}" +
                (_workspaceResolvedAssemblies.Count == 0 ? "\n" : $" ({string.Join(", ", _workspaceResolvedAssemblies.Take(8))}{(_workspaceResolvedAssemblies.Count > 8 ? ", …" : string.Empty)})\n") +
                "Dependency resolver scope: SHA-1-verified Step 18 workspace ONLY\nResolved dependency file SHA-1 rechecked immediately before Cecil open: YES\n" +
                "Fallback to runtime/system/live-install/network resolver paths: NO\n" +
                "Assembly.Load attempted: NO\nReal managed install modified: NO");
        }
        catch (Exception ex)
        {
            return Fail(RealAssemblyRewriteGate.PrimaryRoundTrip, ex);
        }
    }

    public RealAssemblyRewriteGateResult RunNeutralIlRewrite()
    {
        try
        {
            var workspace = RequireWorkspace();
            var sourcePath = ResolveChildPath(workspace.SourceRoot, workspace.PrimaryRelativePath);
            var rewriteRoot = Path.Combine(_workRoot, RewrittenRootName);
            var outputPath = ResolveChildPath(rewriteRoot, workspace.PrimaryRelativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
            DeleteIfExists(outputPath);

            var expectedHash = GetReceiptFile(workspace, workspace.PrimaryRelativePath).Sha1Hex;
            var sourceHashBefore = ComputeSha1Hex(sourcePath);
            if (!sourceHashBefore.Equals(expectedHash, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Primary workspace source no longer matches its receipt SHA-1 before neutral rewrite.");

            string methodFullName;
            Code originalFirstCode;
            int originalInstructionCount;
            ModuleFingerprint sourceFingerprint;

            using (var resolver = CreateWorkspaceResolver(workspace))
            using (var module = ReadModuleImmediate(sourcePath, resolver))
            {
                sourceFingerprint = Fingerprint(module);
                var method = SelectNeutralRewriteMethod(module);
                methodFullName = method.FullName;
                originalInstructionCount = method.Body.Instructions.Count;
                if (originalInstructionCount <= 0)
                    throw new InvalidDataException("Selected neutral-rewrite method unexpectedly has no IL instructions.");

                var first = method.Body.Instructions[0];
                originalFirstCode = first.OpCode.Code;
                method.Body.GetILProcessor().InsertBefore(first, Instruction.Create(OpCodes.Nop));
                module.Write(outputPath, new WriterParameters { WriteSymbols = false });
                RecordWorkspaceResolutions(resolver);
            }

            if (!File.Exists(outputPath) || new FileInfo(outputPath).Length <= 0)
                throw new InvalidDataException("Cecil returned without creating a non-empty neutral-rewrite output.");

            using (var reopened = ReadModuleImmediate(outputPath))
            {
                var reopenedFingerprint = Fingerprint(reopened);
                if (reopenedFingerprint != sourceFingerprint)
                    throw new InvalidDataException("Neutral rewrite unexpectedly changed the primary assembly logical metadata fingerprint.");

                var method = FindMethodByFullName(reopened, methodFullName)
                    ?? throw new InvalidDataException($"Rewritten target method was not found after reopen: {methodFullName}");
                if (!method.HasBody || method.Body.Instructions.Count != originalInstructionCount + 1)
                    throw new InvalidDataException("Rewritten method did not preserve the expected instruction count + one NOP.");
                if (method.Body.Instructions[0].OpCode.Code != Code.Nop)
                    throw new InvalidDataException("Rewritten method does not begin with the Step 18 semantics-neutral NOP marker.");
                if (method.Body.Instructions[1].OpCode.Code != originalFirstCode)
                    throw new InvalidDataException("The original first IL opcode was not preserved immediately after the inserted NOP.");
            }

            var sourceHashAfter = ComputeSha1Hex(sourcePath);
            if (!sourceHashAfter.Equals(expectedHash, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Primary workspace source changed during neutral rewrite.");

            var rewrittenHash = ComputeSha1Hex(outputPath);
            if (rewrittenHash.Equals(sourceHashBefore, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Neutral rewrite output is byte-identical to the source; expected the inserted NOP to produce a distinct copy.");

            _rewrite = new RewriteSnapshot(methodFullName, originalFirstCode, originalInstructionCount, rewrittenHash);

            return Pass(
                RealAssemblyRewriteGate.NeutralIlRewrite,
                "A semantics-neutral IL rewrite was applied to the REAL primary StS2 assembly COPY only.\n" +
                $"Target method: {methodFullName}\n" +
                "Transformation: insert one IL NOP at method entry\n" +
                $"Original first opcode preserved after NOP: {originalFirstCode}\n" +
                $"Instruction count: {originalInstructionCount:N0} → {originalInstructionCount + 1:N0}\n" +
                $"Output: {WorkRootName}/{RewrittenRootName}/{workspace.PrimaryRelativePath}\n" +
                "Rewritten output differs from source bytes: YES\n" +
                "Workspace source receipt SHA-1 preserved: YES\n" +
                $"Workspace-only dependency resolutions observed across Gates B/C: {_workspaceResolvedAssemblies.Count:N0}\n" +
                "Dependency resolver scope: SHA-1-verified Step 18 workspace ONLY\nResolved dependency file SHA-1 rechecked immediately before Cecil open: YES\n" +
                "Behaviorally significant game fix attempted: NO\nGame assembly loaded/executed: NO\nReal managed install modified: NO");
        }
        catch (Exception ex)
        {
            _rewrite = null;
            return Fail(RealAssemblyRewriteGate.NeutralIlRewrite, ex);
        }
    }

    public async Task<RealAssemblyRewriteGateResult> RunIsolationAuditAsync(
        IProgress<RealAssemblyRewriteProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var workspace = RequireWorkspace();
            var rewrite = _rewrite ?? throw new InvalidOperationException("Gate C must pass before the Step 18 isolation audit.");

            var expectedRelativePaths = new HashSet<string>(
                workspace.Files.Select(file => NormalizeRelative(file.RelativePath)),
                StringComparer.OrdinalIgnoreCase);
            var actualRelativePaths = Directory.EnumerateFiles(workspace.SourceRoot, "*", SearchOption.AllDirectories)
                .Select(path => Path.GetRelativePath(workspace.SourceRoot, path).Replace(Path.DirectorySeparatorChar, '/'))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            if (!expectedRelativePaths.SetEquals(actualRelativePaths))
                throw new InvalidDataException("Step 18 source workspace no longer contains exactly the selected receipt-backed managed file set.");

            ulong sourceVerifiedBytes = 0;
            ulong installVerifiedBytes = 0;
            for (var index = 0; index < workspace.Files.Length; index++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var file = workspace.Files[index];
                var relative = NormalizeRelative(file.RelativePath);
                progress?.Report(new RealAssemblyRewriteProgress(
                    RealAssemblyRewriteGate.IsolationAudit,
                    index,
                    workspace.Files.Length,
                    relative,
                    "Re-hashing both the Step 18 source workspace and original Step 12 managed install to prove rewrite isolation…"));

                var sourcePath = ResolveChildPath(workspace.SourceRoot, relative);
                var sourceHash = await ComputeSha1HexAsync(sourcePath, cancellationToken).ConfigureAwait(false);
                if (!sourceHash.Equals(file.Sha1Hex, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException($"Step 18 workspace source no longer matches receipt SHA-1: {relative}");
                checked { sourceVerifiedBytes += (ulong)file.Length; }

                var installPath = ResolveChildPath(workspace.ManagedRoot, relative);
                var installHash = await ComputeSha1HexAsync(installPath, cancellationToken).ConfigureAwait(false);
                if (!installHash.Equals(file.Sha1Hex, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException($"Original managed install no longer matches receipt SHA-1 after Step 18 rewrite testing: {relative}");
                checked { installVerifiedBytes += (ulong)file.Length; }
            }

            if (sourceVerifiedBytes != workspace.ScopeBytes || installVerifiedBytes != workspace.ScopeBytes)
                throw new InvalidDataException("Step 18 isolation-audit byte accounting did not cover the complete selected managed scope.");

            var roundTripPath = ResolveChildPath(Path.Combine(_workRoot, RoundTripRootName), workspace.PrimaryRelativePath);
            var rewrittenPath = ResolveChildPath(Path.Combine(_workRoot, RewrittenRootName), workspace.PrimaryRelativePath);
            if (!File.Exists(roundTripPath) || !File.Exists(rewrittenPath))
                throw new InvalidDataException("Step 18 output directories are missing the expected primary round-trip/rewrite copies.");

            using (var roundTripModule = ReadModuleImmediate(roundTripPath))
            {
                if (roundTripModule.Assembly?.Name?.Name is null)
                    throw new InvalidDataException("Round-trip output no longer opens as an assembly.");
            }

            using (var rewrittenModule = ReadModuleImmediate(rewrittenPath))
            {
                var method = FindMethodByFullName(rewrittenModule, rewrite.MethodFullName)
                    ?? throw new InvalidDataException("Neutral-rewrite target method is missing during final isolation audit.");
                if (!method.HasBody || method.Body.Instructions.Count != rewrite.OriginalInstructionCount + 1 ||
                    method.Body.Instructions[0].OpCode.Code != Code.Nop ||
                    method.Body.Instructions[1].OpCode.Code != rewrite.OriginalFirstCode)
                {
                    throw new InvalidDataException("Neutral-rewrite proof did not survive final reopen/isolation audit.");
                }
            }

            var finalRewriteHash = await ComputeSha1HexAsync(rewrittenPath, cancellationToken).ConfigureAwait(false);
            if (!finalRewriteHash.Equals(rewrite.RewrittenSha1, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Neutral rewrite output changed between Gate C and Gate D.");

            progress?.Report(new RealAssemblyRewriteProgress(
                RealAssemblyRewriteGate.IsolationAudit,
                workspace.Files.Length,
                workspace.Files.Length,
                workspace.PrimaryRelativePath,
                "Isolation audit complete: source workspace and real install remain receipt-identical; only launcher-private output copies differ."));

            return Pass(
                RealAssemblyRewriteGate.IsolationAudit,
                "Step 18 compatibility workspace isolation and reproducibility audit passed.\n" +
                $"Workspace source set exact: {actualRelativePaths.Count:N0}/{workspace.Files.Length:N0}\n" +
                $"Workspace source receipt SHA-1s reverified: {workspace.Files.Length:N0}/{workspace.Files.Length:N0} ({sourceVerifiedBytes:N0} bytes)\n" +
                $"Original managed-install receipt SHA-1s reverified: {workspace.Files.Length:N0}/{workspace.Files.Length:N0} ({installVerifiedBytes:N0} bytes)\n" +
                "Primary Cecil round-trip output reopens: YES\n" +
                $"Neutral NOP rewrite still present after reopen: YES ({rewrite.MethodFullName})\n" +
                "Only launcher-private Step18-RealAssemblyRewrite outputs were written: YES\n" +
                "Original Step 12 install unchanged: YES\n" +
                $"Workspace-only dependency assemblies resolved by Cecil: {_workspaceResolvedAssemblies.Count:N0}\n" +
                "Every dependency resolution was constrained to Step18-RealAssemblyRewrite/source: YES\nResolved dependency file SHA-1 rechecked immediately before Cecil open: YES\n" +
                "Fallback to runtime/system/live-install/network resolver paths: NO\n" +
                "Steam session consulted: NO\nNetwork attempted: NO\nGame assembly loaded/executed: NO\n" +
                "Next-step policy: behavioral compatibility rewrites may now be developed against workspace copies, one evidence-backed incompatibility class at a time.");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return Fail(RealAssemblyRewriteGate.IsolationAudit, ex);
        }
    }

    private WorkspaceSnapshot RequireWorkspace()
        => _workspace ?? throw new InvalidOperationException("Gate A must pass before later Step 18 gates run.");

    private static SteamManagedInstallFile GetReceiptFile(WorkspaceSnapshot workspace, string relative)
        => workspace.Files.Single(file => NormalizeRelative(file.RelativePath).Equals(relative, StringComparison.OrdinalIgnoreCase));

    private static MethodDefinition SelectNeutralRewriteMethod(ModuleDefinition module)
    {
        var candidates = EnumerateTypes(module.Types)
            .SelectMany(type => type.Methods)
            .Where(method => method.HasBody &&
                             !method.IsAbstract &&
                             !method.IsPInvokeImpl &&
                             method.Body.Instructions.Count > 0 &&
                             method.Body.ExceptionHandlers.Count == 0)
            .OrderBy(method => method.IsConstructor)
            .ThenBy(method => method.FullName, StringComparer.Ordinal)
            .ToArray();

        if (candidates.Length == 0)
            throw new InvalidDataException("No deterministic method body without exception handlers was available for the Step 18 NOP rewrite.");
        return candidates[0];
    }

    private static MethodDefinition? FindMethodByFullName(ModuleDefinition module, string fullName)
        => EnumerateTypes(module.Types)
            .SelectMany(type => type.Methods)
            .FirstOrDefault(method => method.FullName.Equals(fullName, StringComparison.Ordinal));

    private static IEnumerable<TypeDefinition> EnumerateTypes(IEnumerable<TypeDefinition> roots)
    {
        foreach (var type in roots)
        {
            yield return type;
            foreach (var nested in EnumerateTypes(type.NestedTypes))
                yield return nested;
        }
    }

    private static ModuleFingerprint Fingerprint(ModuleDefinition module)
    {
        var types = EnumerateTypes(module.Types).ToArray();
        var methods = types.Sum(type => type.Methods.Count);
        return new ModuleFingerprint(
            module.Assembly?.Name?.Name ?? "<netmodule>",
            module.Assembly?.Name?.Version?.ToString() ?? "unknown",
            module.Name,
            module.RuntimeVersion,
            types.Length,
            methods,
            module.AssemblyReferences.Count,
            module.ModuleReferences.Count);
    }

    private static ModuleDefinition ReadModuleImmediate(string path)
        => ModuleDefinition.ReadModule(path, new ReaderParameters
        {
            ReadSymbols = false,
            ReadingMode = ReadingMode.Immediate,
        });

    private static ModuleDefinition ReadModuleImmediate(string path, IAssemblyResolver resolver)
        => ModuleDefinition.ReadModule(path, new ReaderParameters
        {
            ReadSymbols = false,
            ReadingMode = ReadingMode.Immediate,
            AssemblyResolver = resolver,
        });

    private static WorkspaceOnlyAssemblyResolver CreateWorkspaceResolver(WorkspaceSnapshot workspace)
    {
        var primaryDirectory = Path.GetDirectoryName(ResolveChildPath(workspace.SourceRoot, workspace.PrimaryRelativePath))
            ?? throw new InvalidDataException("The Step 18 primary assembly has no workspace directory.");

        var directories = workspace.Files
            .Select(file => Path.GetDirectoryName(ResolveChildPath(workspace.SourceRoot, NormalizeRelative(file.RelativePath))))
            .Where(directory => !string.IsNullOrWhiteSpace(directory))
            .Select(directory => Path.GetFullPath(directory!))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(directory => directory.Equals(primaryDirectory, StringComparison.Ordinal) ? 0 : 1)
            .ThenBy(directory => directory, StringComparer.Ordinal)
            .ToArray();

        var trustedFiles = workspace.Files.ToDictionary(
            file => Path.GetFullPath(ResolveChildPath(workspace.SourceRoot, NormalizeRelative(file.RelativePath))),
            file => file.Sha1Hex,
            StringComparer.Ordinal);

        return new WorkspaceOnlyAssemblyResolver(workspace.SourceRoot, directories, trustedFiles);
    }

    private void RecordWorkspaceResolutions(WorkspaceOnlyAssemblyResolver resolver)
    {
        foreach (var name in resolver.ResolvedAssemblyNames)
            _workspaceResolvedAssemblies.Add(name);
    }

    private sealed class WorkspaceOnlyAssemblyResolver : IAssemblyResolver
    {
        private readonly string _rootPrefix;
        private readonly string[] _directories;
        private readonly Dictionary<string, AssemblyDefinition> _cache = new(StringComparer.Ordinal);
        private readonly Dictionary<string, string> _trustedFileSha1;
        private readonly SortedSet<string> _resolvedAssemblyNames = new(StringComparer.Ordinal);
        private bool _disposed;

        public WorkspaceOnlyAssemblyResolver(
            string root,
            IEnumerable<string> directories,
            IReadOnlyDictionary<string, string> trustedFileSha1)
        {
            var fullRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar);
            _rootPrefix = fullRoot + Path.DirectorySeparatorChar;
            _directories = directories
                .Select(Path.GetFullPath)
                .Where(path => path.Equals(fullRoot, StringComparison.Ordinal) || path.StartsWith(_rootPrefix, StringComparison.Ordinal))
                .Distinct(StringComparer.Ordinal)
                .ToArray();
            _trustedFileSha1 = trustedFileSha1.ToDictionary(
                pair => Path.GetFullPath(pair.Key),
                pair => pair.Value,
                StringComparer.Ordinal);
            if (_directories.Length == 0 || _trustedFileSha1.Count == 0)
                throw new InvalidDataException("The Step 18 workspace-only resolver has no trusted receipt-backed search scope.");
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

            foreach (var directory in _directories)
            {
                foreach (var extension in new[] { ".dll", ".exe" })
                {
                    var candidate = Path.GetFullPath(Path.Combine(directory, name.Name + extension));
                    if (!candidate.StartsWith(_rootPrefix, StringComparison.Ordinal) ||
                        !_trustedFileSha1.TryGetValue(candidate, out var expectedSha1) ||
                        !File.Exists(candidate))
                    {
                        continue;
                    }

                    var actualSha1 = RealAssemblyRewriteWorkspace.ComputeSha1Hex(candidate);
                    if (!actualSha1.Equals(expectedSha1, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidDataException(
                            $"Step 18 workspace dependency changed after Gate A SHA-1 verification: {Path.GetFileName(candidate)}");
                    }

                    var reader = new ReaderParameters
                    {
                        ReadSymbols = false,
                        ReadingMode = ReadingMode.Immediate,
                        InMemory = true,
                        AssemblyResolver = this,
                    };
                    var module = ModuleDefinition.ReadModule(candidate, reader);
                    var assembly = module.Assembly;
                    if (assembly is null || !assembly.Name.Name.Equals(name.Name, StringComparison.Ordinal))
                    {
                        module.Dispose();
                        continue;
                    }

                    _cache[name.FullName] = assembly;
                    _resolvedAssemblyNames.Add(assembly.Name.Name);
                    return assembly;
                }
            }

            throw new AssemblyResolutionException(name);
        }

        public void Dispose()
        {
            if (_disposed)
                return;
            _disposed = true;
            foreach (var assembly in _cache.Values.Distinct())
                assembly.Dispose();
            _cache.Clear();
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
                throw new InvalidOperationException("Step 18 work root escaped launcher-private storage.");
            if (Directory.Exists(work))
                Directory.Delete(work, recursive: true);
        }
        catch when (!throwOnFailure)
        {
            // Best effort after cancellation/failure. A later Gate A always replaces the workspace from scratch.
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
            throw new InvalidDataException($"Path escaped the expected root: {relativePath}");
        return child;
    }

    private static string NormalizeRelative(string path)
        => path.Replace('\\', '/').TrimStart('/');

    private static bool IsManagedAssemblyFileName(string path)
    {
        var extension = Path.GetExtension(path);
        return extension.Equals(".dll", StringComparison.OrdinalIgnoreCase) ||
               extension.Equals(".exe", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsMacOsArm64ManagedPath(string path)
        => ("/" + NormalizeRelative(path))
            .Contains("/data_sts2_macos_arm64/", StringComparison.OrdinalIgnoreCase);

    private static bool IsMacOsX8664ManagedPath(string path)
        => ("/" + NormalizeRelative(path))
            .Contains("/data_sts2_macos_x86_64/", StringComparison.OrdinalIgnoreCase);

    private static bool IsPrimaryArm64StS2Path(string path)
        => ("/" + NormalizeRelative(path))
            .EndsWith("/data_sts2_macos_arm64/sts2.dll", StringComparison.OrdinalIgnoreCase);

    private static string ComputeSha1Hex(string path)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 256 * 1024, FileOptions.SequentialScan);
        using var sha1 = SHA1.Create();
        return Convert.ToHexString(sha1.ComputeHash(stream)).ToLowerInvariant();
    }

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

    private static RealAssemblyRewriteGateResult Pass(RealAssemblyRewriteGate gate, string detail)
        => new(gate, true, detail);

    private static RealAssemblyRewriteGateResult Fail(RealAssemblyRewriteGate gate, Exception ex)
        => new(gate, false, $"{ex.GetType().Name}: {ex.Message}");

    private sealed class CallbackProgress<T> : IProgress<T>
    {
        private readonly Action<T> _callback;
        public CallbackProgress(Action<T> callback) => _callback = callback ?? throw new ArgumentNullException(nameof(callback));
        public void Report(T value) => _callback(value);
    }

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
        ulong ScopeBytes);

    private sealed record RewriteSnapshot(
        string MethodFullName,
        Code OriginalFirstCode,
        int OriginalInstructionCount,
        string RewrittenSha1);

    private sealed record ModuleFingerprint(
        string AssemblyName,
        string AssemblyVersion,
        string ModuleName,
        string RuntimeVersion,
        int TypeCount,
        int MethodCount,
        int AssemblyReferenceCount,
        int ModuleReferenceCount);
}
