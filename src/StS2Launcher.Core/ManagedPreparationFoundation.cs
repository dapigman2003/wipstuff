using System.Security.Cryptography;
using System.Text.Json;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace StS2Launcher.Core;

/// <summary>
/// Step 16 boundary. Mono.Cecil is used only as a metadata/IL file transformer:
/// project-owned fixture files may be written under the launcher's Step 16 work
/// directory, while the real receipt-backed StS2 managed install is opened
/// read-only and never loaded into the CLR.
/// </summary>
public sealed class ManagedPreparationFoundation
{
    public const string FixtureAssemblyName = "StS2Launcher.Step16.Fixture";
    public const string FixtureTypeName = "StS2Launcher.Step16.Fixture.FixtureTarget";
    public const string FixtureMethodName = "RewriteMe";
    public const string FixtureIdentityFieldName = "IdentityMarker";
    public const string FixtureIdentityMarker = "STEP16_CECIL_FIXTURE_V1";
    public const int FixtureOriginalValue = 7;
    public const int FixtureRewrittenValue = 42;

    private readonly string _launcherDataRoot;
    private readonly string _workRoot;
    private readonly SteamOfflineInstallInspection _offlineInspection;

    public ManagedPreparationFoundation(string launcherDataRoot)
    {
        if (string.IsNullOrWhiteSpace(launcherDataRoot))
            throw new ArgumentException("Launcher data root is required.", nameof(launcherDataRoot));

        _launcherDataRoot = Path.GetFullPath(launcherDataRoot);
        _workRoot = Path.Combine(_launcherDataRoot, "Step16-ManagedPreparation");
        _offlineInspection = new SteamOfflineInstallInspection(_launcherDataRoot);
    }

    public ManagedPreparationGateResult RunFixtureRead(string fixturePath)
    {
        try
        {
            ValidateFixtureSourcePath(fixturePath);
            using var assembly = ReadAssembly(fixturePath, ReadingMode.Immediate);
            var value = ValidateFixtureAssembly(assembly);
            var cecilVersion = typeof(AssemblyDefinition).Assembly.GetName().Version?.ToString() ?? "unknown";
            return Pass(
                ManagedPreparationGate.FixtureRead,
                $"Mono.Cecil {cecilVersion} opened the bundled project-owned fixture without loading it.\n" +
                $"Assembly: {assembly.Name.Name} {assembly.Name.Version}\n" +
                $"Module runtime: {assembly.MainModule.RuntimeVersion}\n" +
                $"Fixture identity: {FixtureIdentityMarker}\n" +
                $"{FixtureMethodName} IL constant: {value}");
        }
        catch (Exception ex)
        {
            return Fail(ManagedPreparationGate.FixtureRead, ex);
        }
    }

    public ManagedPreparationGateResult RunFixtureRoundTrip(string fixturePath)
    {
        try
        {
            ValidateFixtureSourcePath(fixturePath);
            PrepareWorkRoot();
            var output = Path.Combine(_workRoot, "fixture-roundtrip.dll");
            DeleteIfExists(output);

            var sourceHashBefore = ComputeSha256Hex(fixturePath);
            using (var assembly = ReadAssembly(fixturePath, ReadingMode.Immediate))
            {
                ValidateFixtureAssembly(assembly);
                assembly.Write(output, new WriterParameters { WriteSymbols = false });
            }

            if (!File.Exists(output) || new FileInfo(output).Length <= 0)
                throw new InvalidDataException("Cecil write returned without a non-empty round-trip assembly.");

            using (var reopened = ReadAssembly(output, ReadingMode.Immediate))
            {
                var value = ValidateFixtureAssembly(reopened);
                if (value != FixtureOriginalValue)
                    throw new InvalidDataException($"Round-trip fixture changed {FixtureMethodName} from {FixtureOriginalValue} to {value}.");
            }

            var sourceHashAfter = ComputeSha256Hex(fixturePath);
            if (!sourceHashBefore.Equals(sourceHashAfter, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("The bundled source fixture changed during round-trip testing.");

            return Pass(
                ManagedPreparationGate.FixtureRoundTrip,
                "Cecil wrote the project-owned fixture to launcher-private Step 16 scratch storage and reopened it successfully.\n" +
                $"Output: Step16-ManagedPreparation/{Path.GetFileName(output)}\n" +
                $"Output bytes: {new FileInfo(output).Length:N0}\n" +
                $"Reopened {FixtureMethodName}: {FixtureOriginalValue}\n" +
                "Bundled source fixture unchanged: YES");
        }
        catch (Exception ex)
        {
            return Fail(ManagedPreparationGate.FixtureRoundTrip, ex);
        }
    }

    public ManagedPreparationGateResult RunControlledIlRewrite(string fixturePath)
    {
        try
        {
            ValidateFixtureSourcePath(fixturePath);
            PrepareWorkRoot();
            var output = Path.Combine(_workRoot, "fixture-rewritten.dll");
            DeleteIfExists(output);
            var sourceHashBefore = ComputeSha256Hex(fixturePath);

            using (var assembly = ReadAssembly(fixturePath, ReadingMode.Immediate))
            {
                var original = ValidateFixtureAssembly(assembly);
                if (original != FixtureOriginalValue)
                    throw new InvalidDataException($"Expected fixture constant {FixtureOriginalValue}, found {original}.");

                var method = FindFixtureMethod(assembly);
                method.Body.ExceptionHandlers.Clear();
                method.Body.Variables.Clear();
                method.Body.Instructions.Clear();
                method.Body.InitLocals = false;
                method.Body.MaxStackSize = 1;
                var il = method.Body.GetILProcessor();
                il.Append(il.Create(OpCodes.Ldc_I4, FixtureRewrittenValue));
                il.Append(il.Create(OpCodes.Ret));

                assembly.Write(output, new WriterParameters { WriteSymbols = false });
            }

            using (var reopened = ReadAssembly(output, ReadingMode.Immediate))
            {
                var rewritten = ReadFixtureReturnConstant(reopened);
                if (rewritten != FixtureRewrittenValue)
                    throw new InvalidDataException($"Reopened IL rewrite returned metadata constant {rewritten}, expected {FixtureRewrittenValue}.");
                ValidateFixtureIdentity(reopened);
            }

            var sourceHashAfter = ComputeSha256Hex(fixturePath);
            if (!sourceHashBefore.Equals(sourceHashAfter, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("The bundled source fixture changed during IL rewrite testing.");

            return Pass(
                ManagedPreparationGate.ControlledIlRewrite,
                $"Controlled Cecil IL rewrite verified after write/reopen: {FixtureMethodName} {FixtureOriginalValue} → {FixtureRewrittenValue}.\n" +
                "Transformation target: project-owned fixture only\n" +
                $"Output: Step16-ManagedPreparation/{Path.GetFileName(output)}\n" +
                $"Fixture identity preserved: {FixtureIdentityMarker}\n" +
                "Bundled source fixture unchanged: YES\n" +
                "Real StS2 install modified: NO");
        }
        catch (Exception ex)
        {
            return Fail(ManagedPreparationGate.ControlledIlRewrite, ex);
        }
    }

    public async Task<ManagedPreparationGateResult> RunRealStS2MetadataInspectionAsync(
        IProgress<ManagedPreparationProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            progress?.Report(new ManagedPreparationProgress(
                ManagedPreparationGate.RealStS2MetadataInspection,
                0,
                0,
                null,
                "Re-proving the Step 13 OfflineReady tree before real-assembly metadata inspection…"));

            IProgress<SteamOfflineInstallProgress>? offlineProgress = progress is null
                ? null
                : new CallbackProgress<SteamOfflineInstallProgress>(value =>
                    progress.Report(new ManagedPreparationProgress(
                        ManagedPreparationGate.RealStS2MetadataInspection,
                        value.CompletedFiles,
                        value.TotalFiles,
                        value.CurrentFile,
                        $"OfflineReady precondition — {value.Message} " +
                        $"({value.CompletedBytes:N0}/{value.TotalBytes:N0} bytes)")));

            var offline = await _offlineInspection.RunAsync(offlineProgress, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            if (offline.Outcome == SteamOfflineInstallOutcome.Cancelled)
                throw new OperationCanceledException("Step 16 OfflineReady precondition was cancelled.", cancellationToken);
            if (!offline.Success || string.IsNullOrWhiteSpace(offline.ManagedInstallRelativePath))
            {
                throw new InvalidDataException(
                    offline.Error ?? $"Managed install is not OfflineReady ({offline.State}/{offline.Outcome}).");
            }

            var managedPath = ResolveChildPath(_launcherDataRoot, offline.ManagedInstallRelativePath);
            var receiptPath = Path.Combine(managedPath, SteamManagedInstallReceipt.FileName);
            SteamManagedInstallReceipt? receipt;
            await using (var stream = new FileStream(
                             receiptPath,
                             FileMode.Open,
                             FileAccess.Read,
                             FileShare.Read,
                             16 * 1024,
                             FileOptions.Asynchronous | FileOptions.SequentialScan))
            {
                receipt = await JsonSerializer.DeserializeAsync(
                        stream,
                        SteamManagedInstallJsonContext.Default.SteamManagedInstallReceipt,
                        cancellationToken)
                    .ConfigureAwait(false);
            }

            if (receipt is null)
                throw new InvalidDataException("The verified Step 12 receipt unexpectedly deserialized to null.");

            ValidateReceiptSnapshot(receipt, offline);

            var candidates = receipt.Files
                .Where(file => IsManagedAssemblyFileName(file.RelativePath))
                .OrderBy(file => file.RelativePath, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (candidates.Length == 0)
                throw new InvalidDataException("The verified install receipt contains no managed-module filename candidates.");

            // The macOS depot legitimately contains architecture-specific copies
            // of sts2.dll (arm64 and x86_64). Gate D is an iPhone/AOT preparation
            // boundary, so prefer the unique macOS arm64 game assembly while still
            // reading/re-hashing every receipt-backed managed candidate read-only.
            var sts2Candidates = candidates
                .Select(file => file.RelativePath.Replace('\\', '/'))
                .Where(relative => Path.GetFileName(relative).Equals("sts2.dll", StringComparison.OrdinalIgnoreCase))
                .ToArray();
            var primaryStS2RelativePath = SelectPrimaryStS2AssemblyRelativePath(sts2Candidates);

            ulong candidateBytes = 0;
            foreach (var candidate in candidates)
                checked { candidateBytes += (ulong)candidate.Length; }

            var parsedModules = 0;
            var nonManagedCandidates = 0;
            var metadataFailures = new List<string>();
            var postInspectionSha1Verified = 0;
            ulong postInspectionBytesVerified = 0;
            var totalTypes = 0L;
            var totalMethods = 0L;
            var totalPInvokeMethods = 0L;
            var totalReflectionEmitTypeRefs = 0L;
            var mainFound = false;
            var mainSha1Preserved = false;
            string? mainRelativePath = null;
            string? mainAssemblyName = null;
            Version? mainAssemblyVersion = null;
            string? mainRuntime = null;
            TargetArchitecture mainArchitecture = TargetArchitecture.I386;
            ModuleKind mainKind = ModuleKind.Dll;
            var mainTypes = 0;
            var mainMethods = 0;
            var mainPInvokeMethods = 0;
            var mainReflectionEmitTypeRefs = 0;
            var mainReferences = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
            var mainPInvokeModules = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);

            for (var index = 0; index < candidates.Length; index++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var file = candidates[index];
                var relative = file.RelativePath.Replace('\\', '/');
                progress?.Report(new ManagedPreparationProgress(
                    ManagedPreparationGate.RealStS2MetadataInspection,
                    index,
                    candidates.Length,
                    relative,
                    "Reading real installed managed metadata with Mono.Cecil; no assembly is resolved, loaded, executed, or written…"));

                var path = ResolveChildPath(managedPath, relative);
                var isMain = relative.Equals(primaryStS2RelativePath, StringComparison.OrdinalIgnoreCase);
                try
                {
                    // Read exactly this receipt-backed file as one CLI module. Using
                    // ModuleDefinition rather than AssemblyDefinition.Modules avoids
                    // implicitly following multi-module File-table sidecars. We also
                    // never call Resolve(), so referenced assemblies are not opened.
                    using var module = ReadModule(path, ReadingMode.Deferred);
                    parsedModules++;

                    long moduleTypes = 0;
                    long moduleMethods = 0;
                    long modulePInvokes = 0;
                    long emitRefs = 0;
                    foreach (var type in EnumerateTypes(module.Types))
                    {
                        moduleTypes++;
                        foreach (var method in type.Methods)
                        {
                            moduleMethods++;
                            if (method.IsPInvokeImpl && method.PInvokeInfo?.Module?.Name is { Length: > 0 } pinvokeModule)
                            {
                                modulePInvokes++;
                                if (isMain)
                                    mainPInvokeModules.Add(pinvokeModule);
                            }
                        }
                    }

                    foreach (var typeReference in module.GetTypeReferences())
                    {
                        var fullName = typeReference.FullName;
                        if (fullName.StartsWith("System.Reflection.Emit.", StringComparison.Ordinal) ||
                            fullName.Equals("System.Reflection.Emit", StringComparison.Ordinal) ||
                            fullName.Contains("System.Reflection.Emit", StringComparison.Ordinal))
                        {
                            emitRefs++;
                        }
                    }

                    checked
                    {
                        totalTypes += moduleTypes;
                        totalMethods += moduleMethods;
                        totalPInvokeMethods += modulePInvokes;
                        totalReflectionEmitTypeRefs += emitRefs;
                    }

                    if (isMain)
                    {
                        if (mainFound)
                            throw new InvalidDataException("The selected primary sts2.dll was encountered more than once.");
                        if (module.Assembly?.Name is null)
                            throw new InvalidDataException("Receipt-backed sts2.dll is a managed module but does not contain an assembly manifest.");

                        mainFound = true;
                        mainRelativePath = relative;
                        mainAssemblyName = module.Assembly.Name.Name;
                        mainAssemblyVersion = module.Assembly.Name.Version;
                        mainRuntime = module.RuntimeVersion;
                        mainArchitecture = module.Architecture;
                        mainKind = module.Kind;
                        mainTypes = checked((int)moduleTypes);
                        mainMethods = checked((int)moduleMethods);
                        mainPInvokeMethods = checked((int)modulePInvokes);
                        mainReflectionEmitTypeRefs = checked((int)emitRefs);
                        foreach (var reference in module.AssemblyReferences)
                            mainReferences.Add($"{reference.Name} {reference.Version}");
                    }
                }
                catch (BadImageFormatException)
                {
                    nonManagedCandidates++;
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException or ArgumentException or InvalidOperationException)
                {
                    metadataFailures.Add($"{relative}: {ex.GetType().Name}: {ex.Message}");
                }

                // OfflineReady proved the entire tree immediately before this scan.
                // Re-hash every .dll/.exe candidate after Cecil touches it so Gate D
                // can concretely prove that its metadata pass did not change any of
                // the files it opened, not merely the primary sts2.dll.
                var sha1After = await ComputeSha1HexAsync(path, cancellationToken).ConfigureAwait(false);
                if (!sha1After.Equals(file.Sha1Hex, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException($"Managed-module candidate no longer matches its Step 12 receipt after Cecil inspection: {relative}");

                postInspectionSha1Verified++;
                checked { postInspectionBytesVerified += (ulong)file.Length; }
                if (isMain)
                    mainSha1Preserved = true;
            }

            if (!mainFound || string.IsNullOrWhiteSpace(mainRelativePath) || string.IsNullOrWhiteSpace(mainAssemblyName))
                throw new InvalidDataException("Mono.Cecil did not locate/parse the receipt-backed real sts2.dll.");
            if (parsedModules <= 0)
                throw new InvalidDataException("Mono.Cecil did not parse any real installed managed module.");
            if (metadataFailures.Count > 0)
                throw new InvalidDataException($"Cecil metadata inspection failed for {metadataFailures.Count} candidate(s); first: {metadataFailures[0]}");
            if (postInspectionSha1Verified != candidates.Length || postInspectionBytesVerified != candidateBytes)
                throw new InvalidDataException("Post-inspection candidate hashing ended without re-verifying the complete .dll/.exe candidate set.");
            if (!mainSha1Preserved)
                throw new InvalidDataException("sts2.dll was parsed but its post-inspection receipt SHA-1 was not verified.");

            progress?.Report(new ManagedPreparationProgress(
                ManagedPreparationGate.RealStS2MetadataInspection,
                candidates.Length,
                candidates.Length,
                mainRelativePath,
                "Real managed metadata inspection complete; every .dll/.exe candidate still matches its trusted receipt SHA-1."));

            var referenceSample = mainReferences.Take(18).ToArray();
            var pinvokeSample = mainPInvokeModules.Take(12).ToArray();
            return Pass(
                ManagedPreparationGate.RealStS2MetadataInspection,
                "Real receipt-backed StS2 managed metadata parsed read-only with Mono.Cecil.\n" +
                $"OfflineReady precondition: YES ({offline.VerifiedFiles:N0}/{offline.PlannedFiles:N0} files)\n" +
                $"Managed-module filename candidates: {candidates.Length:N0} / {candidateBytes:N0} bytes\n" +
                $"Managed modules parsed by Cecil: {parsedModules:N0}\n" +
                $"Non-managed .dll/.exe candidates skipped: {nonManagedCandidates:N0}\n" +
                $"sts2.dll candidates discovered: {sts2Candidates.Length:N0}\n" +
                $"Selected primary StS2 assembly: {mainRelativePath}\n" +
                $"Post-inspection candidate SHA-1s reverified: {postInspectionSha1Verified:N0}/{candidates.Length:N0}\n" +
                $"Total parsed types / methods: {totalTypes:N0} / {totalMethods:N0}\n" +
                $"Total P/Invoke methods: {totalPInvokeMethods:N0}\n" +
                $"Total System.Reflection.Emit type refs: {totalReflectionEmitTypeRefs:N0}\n\n" +
                $"Main assembly: {mainRelativePath}\n" +
                $"Identity: {mainAssemblyName} {mainAssemblyVersion}\n" +
                $"Runtime / architecture / kind: {mainRuntime} / {mainArchitecture} / {mainKind}\n" +
                $"Main types / methods: {mainTypes:N0} / {mainMethods:N0}\n" +
                $"Main P/Invoke methods: {mainPInvokeMethods:N0}\n" +
                $"Main System.Reflection.Emit type refs: {mainReflectionEmitTypeRefs:N0}\n" +
                $"Main assembly references ({mainReferences.Count:N0}): {FormatSample(referenceSample)}\n" +
                $"Main P/Invoke modules ({mainPInvokeModules.Count:N0}): {FormatSample(pinvokeSample)}\n\n" +
                "sts2.dll receipt SHA-1 preserved after inspection: YES\n" +
                "All .dll/.exe candidate receipt SHA-1s preserved after inspection: YES\n" +
                "Assembly dependency resolution attempted: NO\n" +
                "Steam session consulted: NO\nNetwork attempted: NO\nReal managed install modified: NO\nGame assembly loaded/executed: NO");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return Fail(ManagedPreparationGate.RealStS2MetadataInspection, ex);
        }
    }


    private static string SelectPrimaryStS2AssemblyRelativePath(IReadOnlyList<string> sts2Candidates)
    {
        if (sts2Candidates.Count == 0)
            throw new InvalidDataException("The verified install receipt contains no sts2.dll candidate.");

        var arm64 = sts2Candidates
            .Where(IsMacOsArm64StS2Path)
            .ToArray();
        if (arm64.Length == 1)
            return arm64[0];
        if (arm64.Length > 1)
            throw new InvalidDataException($"More than one macOS arm64 receipt-backed sts2.dll was discovered: {FormatSample(arm64)}");

        if (sts2Candidates.Count == 1)
            return sts2Candidates[0];

        throw new InvalidDataException(
            $"Multiple receipt-backed sts2.dll candidates were discovered but no unique macOS arm64 candidate could be selected: {FormatSample(sts2Candidates)}");
    }

    private static bool IsMacOsArm64StS2Path(string relativePath)
    {
        var normalized = "/" + relativePath.Replace('\\', '/').TrimStart('/');
        return normalized.EndsWith("/data_sts2_macos_arm64/sts2.dll", StringComparison.OrdinalIgnoreCase);
    }

    private static AssemblyDefinition ReadAssembly(string path, ReadingMode mode)
    {
        return AssemblyDefinition.ReadAssembly(path, new ReaderParameters
        {
            ReadSymbols = false,
            ReadingMode = mode,
        });
    }

    private static ModuleDefinition ReadModule(string path, ReadingMode mode)
    {
        return ModuleDefinition.ReadModule(path, new ReaderParameters
        {
            ReadSymbols = false,
            ReadingMode = mode,
        });
    }

    private static void ValidateReceiptSnapshot(
        SteamManagedInstallReceipt receipt,
        SteamOfflineInstallResult offline)
    {
        if (!offline.ReceiptStructurallyValid || !offline.ExactManagedTreeVerified)
            throw new InvalidDataException("OfflineReady result did not include a structurally valid receipt and exact-tree proof.");
        if (receipt.SchemaVersion != SteamManagedInstallReceipt.CurrentSchemaVersion ||
            receipt.AppId != SteamOfflineInstallInspection.TargetAppId ||
            receipt.DepotId == 0 ||
            offline.DepotId is null || receipt.DepotId != offline.DepotId.Value ||
            receipt.ManifestId == 0 ||
            offline.InstalledManifestId is null || receipt.ManifestId != offline.InstalledManifestId.Value ||
            string.IsNullOrWhiteSpace(receipt.Branch) ||
            !string.Equals(receipt.Branch, offline.Branch, StringComparison.Ordinal) ||
            receipt.Files is null || receipt.Files.Count == 0 ||
            receipt.Files.Count != offline.PlannedFiles)
        {
            throw new InvalidDataException("The Step 12 receipt changed or became inconsistent after the OfflineReady precondition was proven.");
        }

        var unique = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        ulong plannedBytes = 0;
        foreach (var file in receipt.Files)
        {
            if (file is null ||
                !SteamSingleFileTargetSelector.IsSafeRelativePath(file.RelativePath) ||
                string.Equals(file.RelativePath, SteamManagedInstallReceipt.FileName, StringComparison.OrdinalIgnoreCase) ||
                !unique.Add(file.RelativePath) ||
                file.Length < 0 ||
                !IsSha1Hex(file.Sha1Hex))
            {
                throw new InvalidDataException("The Step 12 receipt file contract changed or became unsafe after OfflineReady verification.");
            }
            checked { plannedBytes += (ulong)file.Length; }
        }

        if (plannedBytes != offline.PlannedBytes)
            throw new InvalidDataException("The Step 12 receipt byte contract changed after OfflineReady verification.");
    }

    private static bool IsSha1Hex(string? value)
    {
        if (value is null || value.Length != 40)
            return false;
        foreach (var c in value)
        {
            var isHex = (c >= '0' && c <= '9') ||
                        (c >= 'a' && c <= 'f') ||
                        (c >= 'A' && c <= 'F');
            if (!isHex)
                return false;
        }
        return true;
    }

    private static int ValidateFixtureAssembly(AssemblyDefinition assembly)
    {
        if (!assembly.Name.Name.Equals(FixtureAssemblyName, StringComparison.Ordinal))
            throw new InvalidDataException($"Unexpected fixture assembly identity: {assembly.Name.Name}.");
        ValidateFixtureIdentity(assembly);
        var value = ReadFixtureReturnConstant(assembly);
        if (value != FixtureOriginalValue)
            throw new InvalidDataException($"Fixture {FixtureMethodName} returned IL constant {value}; expected {FixtureOriginalValue}.");
        return value;
    }

    private static void ValidateFixtureIdentity(AssemblyDefinition assembly)
    {
        var type = assembly.MainModule.GetType(FixtureTypeName)
                   ?? throw new InvalidDataException($"Fixture type missing: {FixtureTypeName}.");
        var field = type.Fields.SingleOrDefault(candidate => candidate.Name == FixtureIdentityFieldName)
                    ?? throw new InvalidDataException($"Fixture identity field missing: {FixtureIdentityFieldName}.");
        if (!field.HasConstant || !string.Equals(field.Constant as string, FixtureIdentityMarker, StringComparison.Ordinal))
            throw new InvalidDataException("Fixture identity constant did not match the Step 16 contract.");
    }

    private static MethodDefinition FindFixtureMethod(AssemblyDefinition assembly)
    {
        var type = assembly.MainModule.GetType(FixtureTypeName)
                   ?? throw new InvalidDataException($"Fixture type missing: {FixtureTypeName}.");
        var method = type.Methods.SingleOrDefault(candidate =>
            candidate.Name == FixtureMethodName &&
            candidate.IsStatic &&
            candidate.Parameters.Count == 0 &&
            candidate.ReturnType.MetadataType == MetadataType.Int32);
        if (method is null || !method.HasBody)
            throw new InvalidDataException($"Fixture method missing or body unavailable: {FixtureMethodName}.");
        return method;
    }

    private static int ReadFixtureReturnConstant(AssemblyDefinition assembly)
    {
        var method = FindFixtureMethod(assembly);
        var meaningful = method.Body.Instructions
            .Where(instruction => instruction.OpCode.Code != Code.Nop)
            .ToArray();
        if (meaningful.Length != 2 || meaningful[1].OpCode.Code != Code.Ret)
            throw new InvalidDataException($"Fixture {FixtureMethodName} body is not the expected constant-return shape.");
        if (!TryReadInt32Constant(meaningful[0], out var value))
            throw new InvalidDataException($"Fixture {FixtureMethodName} does not begin with an Int32 constant.");
        return value;
    }

    private static bool TryReadInt32Constant(Instruction instruction, out int value)
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
            case Code.Ldc_I4_S: value = Convert.ToSByte(instruction.Operand); return true;
            case Code.Ldc_I4: value = Convert.ToInt32(instruction.Operand); return true;
            default: value = 0; return false;
        }
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

    private void PrepareWorkRoot()
    {
        Directory.CreateDirectory(_launcherDataRoot);
        Directory.CreateDirectory(_workRoot);
    }

    private static void DeleteIfExists(string path)
    {
        if (File.Exists(path))
            File.Delete(path);
    }

    private static void ValidateFixtureSourcePath(string fixturePath)
    {
        if (string.IsNullOrWhiteSpace(fixturePath))
            throw new ArgumentException("Fixture path is required.", nameof(fixturePath));
        if (!File.Exists(fixturePath))
            throw new FileNotFoundException("Step 16 bundled fixture assembly is missing.", fixturePath);
    }

    private static string ComputeSha256Hex(string path)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Convert.ToHexString(SHA256.HashData(stream));
    }

    private static async Task<string> ComputeSha1HexAsync(string path, CancellationToken cancellationToken)
    {
        await using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            128 * 1024,
            FileOptions.Asynchronous | FileOptions.SequentialScan);
        using var sha1 = SHA1.Create();
        var hash = await sha1.ComputeHashAsync(stream, cancellationToken).ConfigureAwait(false);
        return Convert.ToHexString(hash).ToLowerInvariant();
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

    private static bool IsManagedAssemblyFileName(string path)
    {
        var extension = Path.GetExtension(path);
        return extension.Equals(".dll", StringComparison.OrdinalIgnoreCase) ||
               extension.Equals(".exe", StringComparison.OrdinalIgnoreCase);
    }

    private static string FormatSample(IReadOnlyList<string> values)
        => values.Count == 0 ? "none" : string.Join(", ", values);

    private sealed class CallbackProgress<T> : IProgress<T>
    {
        private readonly Action<T> _callback;

        public CallbackProgress(Action<T> callback)
        {
            _callback = callback ?? throw new ArgumentNullException(nameof(callback));
        }

        public void Report(T value) => _callback(value);
    }

    private static ManagedPreparationGateResult Pass(ManagedPreparationGate gate, string detail)
        => new(gate, true, detail);

    private static ManagedPreparationGateResult Fail(ManagedPreparationGate gate, Exception ex)
        => new(gate, false, $"{ex.GetType().Name}: {ex.Message}");
}
