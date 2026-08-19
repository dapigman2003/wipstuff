using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Security.Cryptography;
using Mono.Cecil;

namespace StS2Launcher.Core;

/// <summary>
/// Step 20 boundary. Proves that the Release iOS host can execute managed IL that was not
/// AOT-compiled into the IPA, using the Mono interpreter and project-owned fixture assemblies.
/// The test deliberately stops short of loading any StS2 assembly. It also proves one controlled
/// private dependency-resolution hop from launcher-private storage and re-verifies the trusted
/// Step 12 managed install after dynamic execution.
/// </summary>
public sealed class DynamicManagedExecutionFoundation
{
    public const string WorkRootName = "Step20-DynamicManagedExecution";
    public const string FixtureRootName = "fixtures";
    public const string BundleFixtureDirectoryName = "Step20DynamicFixtures";
    public const string ManifestFileName = "step20-fixtures.sha256";
    public const string DynamicFixtureFileName = "StS2Launcher.Step20.DynamicFixture.dll";
    public const string DependencyFixtureFileName = "StS2Launcher.Step20.DependencyFixture.dll";
    public const string RootFixtureFileName = "StS2Launcher.Step20.RootFixture.dll";

    private static readonly string[] RequiredFixtureFiles =
    [
        DynamicFixtureFileName,
        DependencyFixtureFileName,
        RootFixtureFileName,
    ];

    private static readonly string[] RequiredFixtureAssemblyNames =
    [
        "StS2Launcher.Step20.DynamicFixture",
        "StS2Launcher.Step20.DependencyFixture",
        "StS2Launcher.Step20.RootFixture",
    ];

    private readonly string _launcherDataRoot;
    private readonly string _bundleFixtureRoot;
    private readonly string _workRoot;
    private readonly SteamOfflineInstallInspection _offlineInspection;
    private FixtureWorkspaceSnapshot? _workspace;
    private ExecutionSnapshot? _dynamicExecution;
    private DependencyExecutionSnapshot? _dependencyExecution;

    public DynamicManagedExecutionFoundation(string launcherDataRoot, string bundleFixtureRoot)
    {
        if (string.IsNullOrWhiteSpace(launcherDataRoot))
            throw new ArgumentException("Launcher data root is required.", nameof(launcherDataRoot));
        if (string.IsNullOrWhiteSpace(bundleFixtureRoot))
            throw new ArgumentException("Step 20 bundle fixture root is required.", nameof(bundleFixtureRoot));

        _launcherDataRoot = Path.GetFullPath(launcherDataRoot);
        _bundleFixtureRoot = Path.GetFullPath(bundleFixtureRoot);
        _workRoot = Path.Combine(_launcherDataRoot, WorkRootName);
        _offlineInspection = new SteamOfflineInstallInspection(_launcherDataRoot);
    }

    public void Reset()
    {
        _workspace = null;
        _dynamicExecution = null;
        _dependencyExecution = null;
    }

    public async Task<DynamicManagedExecutionGateResult> RunFixtureIntegrityAndOfflineReadyAsync(
        IProgress<DynamicManagedExecutionProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Reset();
            cancellationToken.ThrowIfCancellationRequested();

            var alreadyLoaded = GetLoadedStep20FixtureAssemblies();

            progress?.Report(new DynamicManagedExecutionProgress(
                DynamicManagedExecutionGate.FixtureIntegrityAndOfflineReady,
                0,
                RequiredFixtureFiles.Length,
                null,
                "Re-proving OfflineReady and validating the project-owned dynamic-execution fixture payload before any runtime assembly load…"));

            IProgress<SteamOfflineInstallProgress>? offlineProgress = progress is null
                ? null
                : new CallbackProgress<SteamOfflineInstallProgress>(value =>
                    progress.Report(new DynamicManagedExecutionProgress(
                        DynamicManagedExecutionGate.FixtureIntegrityAndOfflineReady,
                        value.CompletedFiles,
                        value.TotalFiles,
                        value.CurrentFile,
                        $"OfflineReady precondition — {value.Message} ({value.CompletedBytes:N0}/{value.TotalBytes:N0} bytes)")));

            var offline = await _offlineInspection.RunAsync(offlineProgress, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            if (offline.Outcome == SteamOfflineInstallOutcome.Cancelled)
                throw new OperationCanceledException("Step 20 OfflineReady precondition was cancelled.", cancellationToken);
            if (!offline.Success || string.IsNullOrWhiteSpace(offline.ManagedInstallRelativePath))
                throw new InvalidDataException(offline.Error ?? $"Managed install is not OfflineReady ({offline.State}/{offline.Outcome}).");

            if (!Directory.Exists(_bundleFixtureRoot))
                throw new DirectoryNotFoundException($"Step 20 bundled fixture directory is missing: {_bundleFixtureRoot}");
            var manifestPath = Path.Combine(_bundleFixtureRoot, ManifestFileName);
            var manifest = ParseFixtureManifest(await File.ReadAllLinesAsync(manifestPath, cancellationToken).ConfigureAwait(false));

            PrepareFreshWorkRoot();
            var privateFixtureRoot = Path.Combine(_workRoot, FixtureRootName);
            Directory.CreateDirectory(privateFixtureRoot);

            var files = new List<FixtureFileSnapshot>();
            for (var index = 0; index < RequiredFixtureFiles.Length; index++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var fileName = RequiredFixtureFiles[index];
                progress?.Report(new DynamicManagedExecutionProgress(
                    DynamicManagedExecutionGate.FixtureIntegrityAndOfflineReady,
                    index,
                    RequiredFixtureFiles.Length,
                    fileName,
                    "Verifying bundled fixture SHA-256, probing managed identity with Cecil, then copying into launcher-private Step 20 storage…"));

                if (!manifest.TryGetValue(fileName, out var expectedSha256))
                    throw new InvalidDataException($"Step 20 fixture manifest has no entry for {fileName}.");

                var bundledPath = Path.Combine(_bundleFixtureRoot, fileName);
                if (!File.Exists(bundledPath))
                    throw new FileNotFoundException($"Step 20 bundled fixture is missing: {fileName}", bundledPath);
                var bundledHash = await ComputeSha256HexAsync(bundledPath, cancellationToken).ConfigureAwait(false);
                if (!bundledHash.Equals(expectedSha256, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException($"Step 20 bundled fixture SHA-256 mismatch for {fileName}.");

                var identity = ReadManagedIdentity(bundledPath);
                var expectedSimpleName = Path.GetFileNameWithoutExtension(fileName);
                if (!identity.Name.Equals(expectedSimpleName, StringComparison.Ordinal))
                    throw new InvalidDataException($"Step 20 fixture identity mismatch: {fileName} contains assembly '{identity.Name}'.");
                if (!identity.IsIlOnly)
                    throw new InvalidDataException($"Step 20 fixture must be pure IL for the interpreter proof, but {fileName} is not IL-only.");
                ValidateFixtureReferenceBoundary(fileName, identity.AssemblyReferences);

                var privatePath = Path.Combine(privateFixtureRoot, fileName);
                await CopyFileAsync(bundledPath, privatePath, cancellationToken).ConfigureAwait(false);
                var privateHash = await ComputeSha256HexAsync(privatePath, cancellationToken).ConfigureAwait(false);
                if (!privateHash.Equals(expectedSha256, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException($"Step 20 private fixture copy SHA-256 mismatch for {fileName}.");

                files.Add(new FixtureFileSnapshot(fileName, privatePath, expectedSha256, identity.FullName, identity.AssemblyReferences));
            }

            var privateManifestPath = Path.Combine(privateFixtureRoot, ManifestFileName);
            File.Copy(manifestPath, privateManifestPath, overwrite: false);
            var manifestHash = await ComputeSha256HexAsync(manifestPath, cancellationToken).ConfigureAwait(false);
            var privateManifestHash = await ComputeSha256HexAsync(privateManifestPath, cancellationToken).ConfigureAwait(false);
            if (!manifestHash.Equals(privateManifestHash, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Step 20 fixture manifest copy differs from the bundled source manifest.");

            _workspace = new FixtureWorkspaceSnapshot(
                ResolveChildPath(_launcherDataRoot, offline.ManagedInstallRelativePath),
                offline,
                privateFixtureRoot,
                files.ToDictionary(file => file.FileName, StringComparer.Ordinal),
                manifestHash,
                RuntimeFeature.IsDynamicCodeSupported,
                RuntimeFeature.IsDynamicCodeCompiled);

            progress?.Report(new DynamicManagedExecutionProgress(
                DynamicManagedExecutionGate.FixtureIntegrityAndOfflineReady,
                RequiredFixtureFiles.Length,
                RequiredFixtureFiles.Length,
                privateFixtureRoot,
                "Project-owned Step 20 fixture payload is hash-verified and ready for a fresh runtime load."));

            return Pass(
                DynamicManagedExecutionGate.FixtureIntegrityAndOfflineReady,
                "Fresh project-owned dynamic-execution fixture payload established without loading any fixture or game assembly.\n" +
                $"OfflineReady precondition: YES ({offline.VerifiedFiles:N0}/{offline.PlannedFiles:N0} files)\n" +
                $"RuntimeFeature.IsDynamicCodeSupported: {_workspace.DynamicCodeSupported}\n" +
                $"RuntimeFeature.IsDynamicCodeCompiled: {_workspace.DynamicCodeCompiled}\n" +
                $"Bundled fixture files SHA-256 verified: {files.Count}/{RequiredFixtureFiles.Length}\n" +
                $"Launcher-private fixture copies SHA-256 verified: {files.Count}/{RequiredFixtureFiles.Length}\n" +
                $"Dynamic fixture identity: {files.Single(file => file.FileName == DynamicFixtureFileName).AssemblyFullName}\n" +
                $"Dependency fixture identity: {files.Single(file => file.FileName == DependencyFixtureFileName).AssemblyFullName}\n" +
                $"Root fixture identity: {files.Single(file => file.FileName == RootFixtureFileName).AssemblyFullName}\n" +
                $"Workspace root: {WorkRootName}/{FixtureRootName}\n" +
                $"Step 20 fixture assemblies already loaded before Gate B: {(alreadyLoaded.Length == 0 ? "NO" : "YES — " + string.Join(", ", alreadyLoaded))}\n" +
                "Gate B freshness policy: a new dedicated AssemblyLoadContext is always used, so a prior diagnostic run cannot satisfy the new load implicitly.\n" +
                "StS2 assembly loaded/executed: NO\nSteam session consulted: NO\nNetwork attempted by Step 20: NO\nReal managed install modified: NO");
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
            return Fail(DynamicManagedExecutionGate.FixtureIntegrityAndOfflineReady, "fixture integrity / OfflineReady", ex);
        }
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Step 20 intentionally reflects over a project-owned external fixture after its exact SHA-256 and assembly identity are verified.")]
    public DynamicManagedExecutionGateResult RunDynamicFixtureExecution()
    {
        const string stage = "project-owned external IL load + execution";
        try
        {
            var workspace = RequireWorkspace();
            EnsureNoStS2AssemblyLoaded();
            var fixture = RequireFixture(workspace, DynamicFixtureFileName);
            var context = new Step20FixtureLoadContext("Step20-GateB-DynamicFixture", new Dictionary<string, FixtureFileSnapshot>(StringComparer.Ordinal));
            var assembly = LoadAssemblyFromVerifiedBytes(context, fixture);
            if (!ReferenceEquals(AssemblyLoadContext.GetLoadContext(assembly), context))
                throw new InvalidDataException("Gate B fixture did not load into the dedicated Step 20 AssemblyLoadContext.");

            var result = InvokeInt32Probe(
                assembly,
                "StS2Launcher.Step20.DynamicFixture.DynamicFixtureProbe",
                "Run");
            if (result != 42)
                throw new InvalidDataException($"Step 20 dynamic fixture returned {result}; expected 42.");
            if (context.PrivateDependencyLoadCount != 0)
                throw new InvalidDataException("Gate B unexpectedly loaded a private fixture dependency.");

            _dynamicExecution = new ExecutionSnapshot(
                assembly.GetName().FullName ?? assembly.FullName ?? "<unknown>",
                context.Name ?? "<unnamed>",
                result,
                context.RequestedAssemblyNames.ToArray());

            EnsureNoStS2AssemblyLoaded();
            return Pass(
                DynamicManagedExecutionGate.DynamicFixtureExecution,
                "A project-owned managed assembly that was copied into the IPA as data—not referenced/AOT-compiled into the launcher—was loaded from verified bytes and executed successfully.\n" +
                $"Loaded assembly: {_dynamicExecution.AssemblyFullName}\n" +
                $"AssemblyLoadContext: {_dynamicExecution.LoadContextName}\n" +
                $"Dynamic fixture result: {_dynamicExecution.Result} (expected 42)\n" +
                $"Private dependency loads: {context.PrivateDependencyLoadCount}\n" +
                $"Framework/dependency requests observed: {FormatSamples(_dynamicExecution.RequestedAssemblyNames)}\n" +
                $"RuntimeFeature.IsDynamicCodeSupported remains: {RuntimeFeature.IsDynamicCodeSupported}\n" +
                $"RuntimeFeature.IsDynamicCodeCompiled remains: {RuntimeFeature.IsDynamicCodeCompiled}\n" +
                "Execution mechanism proven: runtime-loaded IL can execute in this Release iOS host without JIT code generation.\n" +
                "StS2 assembly loaded/executed: NO\nReal managed install modified: NO");
        }
        catch (Exception ex)
        {
            _dynamicExecution = null;
            return Fail(DynamicManagedExecutionGate.DynamicFixtureExecution, stage, UnwrapInvocation(ex));
        }
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Step 20 intentionally reflects over project-owned external fixtures after exact SHA-256 and identity verification.")]
    public DynamicManagedExecutionGateResult RunPrivateDependencyResolution()
    {
        const string stage = "private verified dependency resolution + transitive IL execution";
        try
        {
            var workspace = RequireWorkspace();
            _ = _dynamicExecution ?? throw new InvalidOperationException("Gate B must pass before Gate C.");
            EnsureNoStS2AssemblyLoaded();

            var root = RequireFixture(workspace, RootFixtureFileName);
            var dependency = RequireFixture(workspace, DependencyFixtureFileName);
            var privateDependencies = new Dictionary<string, FixtureFileSnapshot>(StringComparer.Ordinal)
            {
                ["StS2Launcher.Step20.DependencyFixture"] = dependency,
            };
            var context = new Step20FixtureLoadContext("Step20-GateC-PrivateDependency", privateDependencies);
            var rootAssembly = LoadAssemblyFromVerifiedBytes(context, root);
            var result = InvokeInt32Probe(
                rootAssembly,
                "StS2Launcher.Step20.RootFixture.RootFixtureProbe",
                "Run");
            if (result != 42)
                throw new InvalidDataException($"Step 20 dependent fixture returned {result}; expected 42.");
            if (context.PrivateDependencyLoadCount != 1)
                throw new InvalidDataException($"Expected exactly one private dependency load, observed {context.PrivateDependencyLoadCount}.");
            if (!context.PrivateDependencyLoadedNames.Contains("StS2Launcher.Step20.DependencyFixture", StringComparer.Ordinal))
                throw new InvalidDataException("The expected Step 20 dependency fixture was not resolved from launcher-private verified storage.");

            var loadedDependency = context.LoadedPrivateAssemblies
                .SingleOrDefault(assembly => string.Equals(assembly.GetName().Name, "StS2Launcher.Step20.DependencyFixture", StringComparison.Ordinal));
            if (loadedDependency is null || !ReferenceEquals(AssemblyLoadContext.GetLoadContext(loadedDependency), context))
                throw new InvalidDataException("The Step 20 dependency fixture was not loaded into Gate C's dedicated AssemblyLoadContext.");

            _dependencyExecution = new DependencyExecutionSnapshot(
                rootAssembly.GetName().FullName ?? rootAssembly.FullName ?? "<unknown>",
                loadedDependency.GetName().FullName ?? loadedDependency.FullName ?? "<unknown>",
                context.Name ?? "<unnamed>",
                result,
                context.PrivateDependencyLoadCount,
                context.RequestedAssemblyNames.ToArray());

            EnsureNoStS2AssemblyLoaded();
            return Pass(
                DynamicManagedExecutionGate.PrivateDependencyResolution,
                "A second runtime-loaded fixture executed through one explicit transitive managed dependency supplied only from SHA-256-verified launcher-private Step 20 storage.\n" +
                $"Root assembly: {_dependencyExecution.RootAssemblyFullName}\n" +
                $"Resolved dependency: {_dependencyExecution.DependencyAssemblyFullName}\n" +
                $"AssemblyLoadContext: {_dependencyExecution.LoadContextName}\n" +
                $"Dependent fixture result: {_dependencyExecution.Result} (expected 42)\n" +
                $"Verified private dependency loads: {_dependencyExecution.PrivateDependencyLoadCount}\n" +
                $"Assembly requests observed: {FormatSamples(_dependencyExecution.RequestedAssemblyNames)}\n" +
                "Dependency fallback to live StS2 install: NO\nDependency fallback to network: NO\n" +
                "StS2 assembly loaded/executed: NO\nReal managed install modified: NO");
        }
        catch (Exception ex)
        {
            _dependencyExecution = null;
            return Fail(DynamicManagedExecutionGate.PrivateDependencyResolution, stage, UnwrapInvocation(ex));
        }
    }

    public async Task<DynamicManagedExecutionGateResult> RunIsolationAuditAsync(
        IProgress<DynamicManagedExecutionProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var workspace = RequireWorkspace();
            _ = _dynamicExecution ?? throw new InvalidOperationException("Gate B evidence is missing.");
            _ = _dependencyExecution ?? throw new InvalidOperationException("Gate C evidence is missing.");

            progress?.Report(new DynamicManagedExecutionProgress(
                DynamicManagedExecutionGate.IsolationAudit,
                0,
                RequiredFixtureFiles.Length,
                null,
                "Re-hashing every private Step 20 fixture after execution and re-proving the complete OfflineReady managed install…"));

            var verifiedFixtures = 0;
            foreach (var fileName in RequiredFixtureFiles)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var fixture = RequireFixture(workspace, fileName);
                var hash = await ComputeSha256HexAsync(fixture.PrivatePath, cancellationToken).ConfigureAwait(false);
                if (!hash.Equals(fixture.Sha256Hex, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException($"Step 20 private fixture changed after execution: {fileName}");
                verifiedFixtures++;
            }

            var privateManifestPath = Path.Combine(workspace.PrivateFixtureRoot, ManifestFileName);
            var privateManifestHash = await ComputeSha256HexAsync(privateManifestPath, cancellationToken).ConfigureAwait(false);
            if (!privateManifestHash.Equals(workspace.ManifestSha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Step 20 private fixture manifest changed after execution.");

            IProgress<SteamOfflineInstallProgress>? offlineProgress = progress is null
                ? null
                : new CallbackProgress<SteamOfflineInstallProgress>(value =>
                    progress.Report(new DynamicManagedExecutionProgress(
                        DynamicManagedExecutionGate.IsolationAudit,
                        value.CompletedFiles,
                        value.TotalFiles,
                        value.CurrentFile,
                        $"Post-execution OfflineReady audit — {value.Message} ({value.CompletedBytes:N0}/{value.TotalBytes:N0} bytes)")));
            var offline = await _offlineInspection.RunAsync(offlineProgress, cancellationToken).ConfigureAwait(false);
            if (!offline.Success || !offline.ExactManagedTreeVerified || !offline.ReceiptStructurallyValid)
                throw new InvalidDataException(offline.Error ?? "Post-Step-20 OfflineReady audit failed.");
            if (offline.DepotId != workspace.Offline.DepotId ||
                offline.InstalledManifestId != workspace.Offline.InstalledManifestId ||
                !string.Equals(offline.ManagedInstallRelativePath, workspace.Offline.ManagedInstallRelativePath, StringComparison.Ordinal))
            {
                throw new InvalidDataException("The managed-install identity changed during Step 20.");
            }

            EnsureNoStS2AssemblyLoaded();
            var fixtureLoaded = GetLoadedStep20FixtureAssemblies();
            foreach (var required in RequiredFixtureAssemblyNames)
            {
                if (!fixtureLoaded.Contains(required, StringComparer.Ordinal))
                    throw new InvalidDataException($"Expected executed Step 20 fixture assembly is missing from the process audit: {required}");
            }

            var privateFiles = Directory.EnumerateFiles(workspace.PrivateFixtureRoot, "*", SearchOption.AllDirectories)
                .Select(Path.GetFileName)
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToArray();
            var expectedFiles = RequiredFixtureFiles.Append(ManifestFileName).OrderBy(name => name, StringComparer.Ordinal).ToArray();
            if (!privateFiles.SequenceEqual(expectedFiles, StringComparer.Ordinal))
                throw new InvalidDataException("Unexpected file(s) exist in the Step 20 launcher-private fixture workspace.");

            progress?.Report(new DynamicManagedExecutionProgress(
                DynamicManagedExecutionGate.IsolationAudit,
                verifiedFixtures,
                RequiredFixtureFiles.Length,
                workspace.PrivateFixtureRoot,
                "Step 20 dynamic-execution isolation audit complete."));

            return Pass(
                DynamicManagedExecutionGate.IsolationAudit,
                "Dynamic managed execution remained confined to project-owned Step 20 fixtures and the trusted game installation stayed receipt-identical.\n" +
                $"Private fixture SHA-256s reverified after execution: {verifiedFixtures}/{RequiredFixtureFiles.Length}\n" +
                "Private fixture manifest SHA-256 preserved: YES\n" +
                $"Post-execution OfflineReady exact-tree verification: YES ({offline.VerifiedFiles:N0}/{offline.PlannedFiles:N0} files)\n" +
                "Managed install identity unchanged: YES\n" +
                $"Executed Step 20 fixture assemblies observed: {string.Join(", ", fixtureLoaded)}\n" +
                "StS2 assembly loaded/executed: NO\n" +
                "Writes to receipt-backed managed install: NO\nNetwork/Steam dependency for Step 20 execution: NO\n" +
                "Step 20 conclusion: runtime-loaded project-owned IL + one private managed dependency execute successfully in the Release iOS host; real StS2 loading remains deferred to a later gate.");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return Fail(DynamicManagedExecutionGate.IsolationAudit, "post-execution isolation audit", ex);
        }
    }

    private FixtureWorkspaceSnapshot RequireWorkspace()
        => _workspace ?? throw new InvalidOperationException("Gate A must pass before later Step 20 gates.");

    private static FixtureFileSnapshot RequireFixture(FixtureWorkspaceSnapshot workspace, string fileName)
        => workspace.Files.TryGetValue(fileName, out var fixture)
            ? fixture
            : throw new InvalidDataException($"Step 20 fixture snapshot is missing {fileName}.");

    private void PrepareFreshWorkRoot()
    {
        BestEffortDeleteWorkRoot();
        Directory.CreateDirectory(_workRoot);
    }

    private void BestEffortDeleteWorkRoot()
    {
        try
        {
            if (Directory.Exists(_workRoot))
                Directory.Delete(_workRoot, recursive: true);
        }
        catch
        {
            // A later explicit file/hash check will surface any stale workspace problem.
        }
    }

    private static Dictionary<string, string> ParseFixtureManifest(IEnumerable<string> lines)
    {
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var raw in lines)
        {
            var line = raw.Trim();
            if (line.Length == 0 || line.StartsWith('#'))
                continue;
            var separator = line.IndexOf("  ", StringComparison.Ordinal);
            if (separator <= 0)
                throw new InvalidDataException($"Malformed Step 20 fixture manifest line: {line}");
            var hash = line[..separator].Trim().ToLowerInvariant();
            var fileName = line[(separator + 2)..].Trim();
            if (hash.Length != 64 || !hash.All(Uri.IsHexDigit))
                throw new InvalidDataException($"Invalid Step 20 fixture SHA-256 in manifest: {fileName}");
            if (fileName.Contains('/') || fileName.Contains('\\') || !RequiredFixtureFiles.Contains(fileName, StringComparer.Ordinal))
                throw new InvalidDataException($"Unexpected Step 20 fixture manifest file: {fileName}");
            if (!result.TryAdd(fileName, hash))
                throw new InvalidDataException($"Duplicate Step 20 fixture manifest entry: {fileName}");
        }
        if (result.Count != RequiredFixtureFiles.Length)
            throw new InvalidDataException($"Step 20 fixture manifest expected {RequiredFixtureFiles.Length} entries, found {result.Count}.");
        return result;
    }

    private static AssemblyIdentitySnapshot ReadManagedIdentity(string path)
    {
        using var module = ModuleDefinition.ReadModule(path, new ReaderParameters
        {
            ReadSymbols = false,
            ReadingMode = ReadingMode.Deferred,
            InMemory = true,
        });
        if (module.Assembly?.Name is null)
            throw new BadImageFormatException($"Step 20 fixture has no assembly manifest: {path}");
        if (module.Types.SelectMany(EnumerateTypes).SelectMany(type => type.Methods).Any(method => method.IsPInvokeImpl))
            throw new InvalidDataException($"Step 20 fixture unexpectedly contains P/Invoke metadata: {path}");
        return new AssemblyIdentitySnapshot(
            module.Assembly.Name.Name,
            module.Assembly.Name.FullName,
            (module.Attributes & ModuleAttributes.ILOnly) != 0,
            module.AssemblyReferences.Select(reference => reference.FullName).OrderBy(value => value, StringComparer.Ordinal).ToArray());
    }

    private static IEnumerable<TypeDefinition> EnumerateTypes(TypeDefinition root)
    {
        yield return root;
        foreach (var nested in root.NestedTypes)
        {
            foreach (var value in EnumerateTypes(nested))
                yield return value;
        }
    }

    private static void ValidateFixtureReferenceBoundary(string fileName, IReadOnlyList<string> references)
    {
        var dependencyName = "StS2Launcher.Step20.DependencyFixture";
        foreach (var fullName in references)
        {
            var simpleName = new AssemblyName(fullName).Name ?? string.Empty;
            if (IsFrameworkContractName(simpleName))
                continue;
            if (fileName.Equals(RootFixtureFileName, StringComparison.Ordinal) && simpleName.Equals(dependencyName, StringComparison.Ordinal))
                continue;
            throw new InvalidDataException($"Step 20 fixture {fileName} has an unexpected non-framework assembly reference: {fullName}");
        }

        var hasPrivateDependency = references.Any(reference =>
            string.Equals(new AssemblyName(reference).Name, dependencyName, StringComparison.Ordinal));
        if (fileName.Equals(RootFixtureFileName, StringComparison.Ordinal) != hasPrivateDependency)
        {
            throw new InvalidDataException(
                fileName.Equals(RootFixtureFileName, StringComparison.Ordinal)
                    ? "Step 20 root fixture no longer references the exact private dependency fixture."
                    : $"Step 20 fixture {fileName} unexpectedly references the private dependency fixture.");
        }
    }

    private static bool IsFrameworkContractName(string simpleName)
        => simpleName.Equals("mscorlib", StringComparison.Ordinal) ||
           simpleName.Equals("netstandard", StringComparison.Ordinal) ||
           simpleName.Equals("System", StringComparison.Ordinal) ||
           simpleName.StartsWith("System.", StringComparison.Ordinal) ||
           simpleName.Equals("Microsoft.CSharp", StringComparison.Ordinal) ||
           simpleName.StartsWith("Microsoft.VisualBasic", StringComparison.Ordinal);

    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Step 20 intentionally loads exact-hash project-owned external IL whose complete fixture metadata is preserved outside the trimmer input set.")]
    private static Assembly LoadAssemblyFromVerifiedBytes(Step20FixtureLoadContext context, FixtureFileSnapshot fixture)
    {
        var hash = ComputeSha256Hex(fixture.PrivatePath);
        if (!hash.Equals(fixture.Sha256Hex, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException($"Step 20 fixture SHA-256 changed immediately before runtime load: {fixture.FileName}");
        using var stream = new MemoryStream(File.ReadAllBytes(fixture.PrivatePath), writable: false);
        var assembly = context.LoadFromStream(stream);
        var loadedName = assembly.GetName().FullName ?? assembly.FullName ?? string.Empty;
        if (!loadedName.Equals(fixture.AssemblyFullName, StringComparison.Ordinal))
            throw new InvalidDataException($"Runtime-loaded Step 20 fixture identity differs from its Cecil-probed identity: {loadedName} != {fixture.AssemblyFullName}");
        return assembly;
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "The target type belongs to an exact-hash project-owned external fixture whose metadata is intentionally inspected and invoked by name.")]
    [UnconditionalSuppressMessage("Trimming", "IL2075", Justification = "The target type belongs to an exact-hash project-owned external fixture whose public static probe method is intentionally invoked by name.")]
    private static int InvokeInt32Probe(Assembly assembly, string typeName, string methodName)
    {
        var type = assembly.GetType(typeName, throwOnError: true, ignoreCase: false)
            ?? throw new MissingMemberException(typeName);
        var method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static, binder: null, Type.EmptyTypes, modifiers: null)
            ?? throw new MissingMethodException(typeName, methodName);
        if (method.ReturnType != typeof(int))
            throw new InvalidDataException($"Step 20 probe method has unexpected return type: {method.ReturnType.FullName}");
        var value = method.Invoke(null, null);
        return value is int result
            ? result
            : throw new InvalidDataException("Step 20 probe invocation did not return Int32.");
    }

    private static Exception UnwrapInvocation(Exception ex)
        => ex is TargetInvocationException { InnerException: not null } invocation ? invocation.InnerException : ex;

    private static string[] GetLoadedStep20FixtureAssemblies()
        => AppDomain.CurrentDomain.GetAssemblies()
            .Select(assembly => assembly.GetName().Name)
            .Where(name => name is not null && RequiredFixtureAssemblyNames.Contains(name, StringComparer.Ordinal))
            .Cast<string>()
            .Distinct(StringComparer.Ordinal)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

    private static void EnsureNoStS2AssemblyLoaded()
    {
        var matches = AppDomain.CurrentDomain.GetAssemblies()
            .Select(assembly => assembly.GetName())
            .Where(name => string.Equals(name.Name, "sts2", StringComparison.OrdinalIgnoreCase))
            .Select(name => name.FullName ?? name.Name ?? "sts2")
            .ToArray();
        if (matches.Length != 0)
            throw new InvalidDataException("Step 20 detected a loaded StS2 assembly even though this subsystem is fixture-only: " + string.Join(", ", matches));
    }

    private static string ResolveChildPath(string root, string relativePath)
    {
        if (!SteamSingleFileTargetSelector.IsSafeRelativePath(relativePath))
            throw new InvalidDataException($"Unsafe relative path: {relativePath}");
        var rootFull = Path.GetFullPath(root);
        var child = Path.GetFullPath(Path.Combine(rootFull, relativePath.Replace('/', Path.DirectorySeparatorChar)));
        var prefix = rootFull.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        if (!child.StartsWith(prefix, StringComparison.Ordinal))
            throw new InvalidDataException($"Path escaped root: {relativePath}");
        return child;
    }

    private static async Task CopyFileAsync(string source, string destination, CancellationToken cancellationToken)
    {
        await using var input = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.Read, 128 * 1024, FileOptions.Asynchronous | FileOptions.SequentialScan);
        await using var output = new FileStream(destination, FileMode.CreateNew, FileAccess.Write, FileShare.None, 128 * 1024, FileOptions.Asynchronous | FileOptions.SequentialScan);
        await input.CopyToAsync(output, 128 * 1024, cancellationToken).ConfigureAwait(false);
        await output.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    private static async Task<string> ComputeSha256HexAsync(string path, CancellationToken cancellationToken)
    {
        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 256 * 1024, FileOptions.Asynchronous | FileOptions.SequentialScan);
        using var sha = SHA256.Create();
        var hash = await sha.ComputeHashAsync(stream, cancellationToken).ConfigureAwait(false);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static string ComputeSha256Hex(string path)
    {
        using var stream = File.OpenRead(path);
        using var sha = SHA256.Create();
        return Convert.ToHexString(sha.ComputeHash(stream)).ToLowerInvariant();
    }

    private static string FormatSamples(IReadOnlyList<string> values)
        => values.Count == 0 ? "none" : string.Join(", ", values.Take(10));

    private static DynamicManagedExecutionGateResult Pass(DynamicManagedExecutionGate gate, string detail)
        => new(gate, true, detail);

    private static DynamicManagedExecutionGateResult Fail(DynamicManagedExecutionGate gate, string stage, Exception ex)
        => new(gate, false, $"Stage: {stage}\n{ex.GetType().Name}: {ex.Message}");

    private sealed class CallbackProgress<T> : IProgress<T>
    {
        private readonly Action<T> _callback;
        public CallbackProgress(Action<T> callback) => _callback = callback ?? throw new ArgumentNullException(nameof(callback));
        public void Report(T value) => _callback(value);
    }

    private sealed class Step20FixtureLoadContext : AssemblyLoadContext
    {
        private readonly IReadOnlyDictionary<string, FixtureFileSnapshot> _privateDependencies;
        private readonly List<string> _requestedAssemblyNames = [];
        private readonly List<string> _privateDependencyLoadedNames = [];
        private readonly List<Assembly> _loadedPrivateAssemblies = [];

        public Step20FixtureLoadContext(string name, IReadOnlyDictionary<string, FixtureFileSnapshot> privateDependencies)
            : base(name, isCollectible: false)
        {
            _privateDependencies = privateDependencies;
        }

        public IReadOnlyList<string> RequestedAssemblyNames => _requestedAssemblyNames;
        public IReadOnlyList<string> PrivateDependencyLoadedNames => _privateDependencyLoadedNames;
        public IReadOnlyList<Assembly> LoadedPrivateAssemblies => _loadedPrivateAssemblies;
        public int PrivateDependencyLoadCount => _privateDependencyLoadedNames.Count;

        [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Gate C loads only an exact-identity, exact-hash project-owned private dependency; its code and metadata are intentionally outside the build-time trimmer graph.")]
        protected override Assembly? Load(AssemblyName assemblyName)
        {
            var requested = assemblyName.FullName ?? assemblyName.Name ?? "<unknown>";
            if (!_requestedAssemblyNames.Contains(requested, StringComparer.Ordinal))
                _requestedAssemblyNames.Add(requested);

            if (assemblyName.Name is null)
                throw new FileLoadException("Step 20 private resolver received an assembly request without a simple name.");

            if (!_privateDependencies.TryGetValue(assemblyName.Name, out var fixture))
            {
                if (IsFrameworkContractName(assemblyName.Name))
                    return null; // Delegate only known platform/framework contracts to the host runtime.
                throw new FileLoadException(
                    $"Step 20 private resolver refuses fallback for non-framework assembly '{requested}'. Only exact verified fixture dependencies may load privately.");
            }

            var expected = new AssemblyName(fixture.AssemblyFullName);
            if (!AssemblyIdentityMatches(assemblyName, expected))
            {
                throw new FileLoadException(
                    $"Step 20 private resolver rejected identity mismatch. Requested '{requested}', verified private fixture is '{fixture.AssemblyFullName}'.");
            }

            var hash = ComputeSha256Hex(fixture.PrivatePath);
            if (!hash.Equals(fixture.Sha256Hex, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException($"Step 20 private dependency SHA-256 changed immediately before load: {fixture.FileName}");

            using var stream = new MemoryStream(File.ReadAllBytes(fixture.PrivatePath), writable: false);
            var assembly = LoadFromStream(stream);
            _privateDependencyLoadedNames.Add(assemblyName.Name);
            _loadedPrivateAssemblies.Add(assembly);
            return assembly;
        }

        private static bool AssemblyIdentityMatches(AssemblyName requested, AssemblyName candidate)
        {
            if (!string.Equals(requested.Name, candidate.Name, StringComparison.Ordinal) || requested.Version != candidate.Version)
                return false;
            var requestedCulture = string.IsNullOrWhiteSpace(requested.CultureName) ? string.Empty : requested.CultureName;
            var candidateCulture = string.IsNullOrWhiteSpace(candidate.CultureName) ? string.Empty : candidate.CultureName;
            if (!string.Equals(requestedCulture, candidateCulture, StringComparison.OrdinalIgnoreCase))
                return false;
            return GetTokenHex(requested).Equals(GetTokenHex(candidate), StringComparison.OrdinalIgnoreCase);
        }

        private static string GetTokenHex(AssemblyName name)
            => Convert.ToHexString(name.GetPublicKeyToken() ?? []).ToLowerInvariant();
    }

    private sealed record AssemblyIdentitySnapshot(string Name, string FullName, bool IsIlOnly, IReadOnlyList<string> AssemblyReferences);
    private sealed record FixtureFileSnapshot(
        string FileName,
        string PrivatePath,
        string Sha256Hex,
        string AssemblyFullName,
        IReadOnlyList<string> AssemblyReferences);
    private sealed record FixtureWorkspaceSnapshot(
        string ManagedRoot,
        SteamOfflineInstallResult Offline,
        string PrivateFixtureRoot,
        IReadOnlyDictionary<string, FixtureFileSnapshot> Files,
        string ManifestSha256,
        bool DynamicCodeSupported,
        bool DynamicCodeCompiled);
    private sealed record ExecutionSnapshot(
        string AssemblyFullName,
        string LoadContextName,
        int Result,
        IReadOnlyList<string> RequestedAssemblyNames);
    private sealed record DependencyExecutionSnapshot(
        string RootAssemblyFullName,
        string DependencyAssemblyFullName,
        string LoadContextName,
        int Result,
        int PrivateDependencyLoadCount,
        IReadOnlyList<string> RequestedAssemblyNames);
}
