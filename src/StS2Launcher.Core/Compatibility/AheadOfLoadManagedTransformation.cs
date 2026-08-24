using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.Loader;
using System.Security.Cryptography;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace StS2Launcher.Core;

/// <summary>
/// Step 28.0 boundary. After Step 27 physically ruled out Harmony/MonoMod runtime replacement
/// for a representative post-publish interpreted target, this subsystem proves the replacement
/// architecture: deterministic Cecil transformation of a verified launcher-private image before
/// CLR admission, followed by interpreted execution of only the transformed bytes.
///
/// The fixture is project-owned and copied into the app only after dotnet publish. The original
/// bundled fixture and its private source clone are never loaded into the CLR by this subsystem.
/// </summary>
public sealed class AheadOfLoadManagedTransformation
{
    public const string WorkRootName = "Step28-AheadOfLoadTransformation";
    public const string SourceRootName = "source";
    public const string TransformedRootName = "transformed";
    public const string BundleFixtureDirectoryName = "Step28AheadOfLoadFixture";
    public const string ManifestFileName = "step28-ahead-of-load-fixture.sha256";
    public const string FixtureFileName = "StS2Launcher.Step28.AheadOfLoadFixture.dll";
    public const string FixtureAssemblySimpleName = "StS2Launcher.Step28.AheadOfLoadFixture";
    public const string FixtureTypeFullName = "StS2Launcher.Step28.AheadOfLoadFixture.AheadOfLoadRewriteProbe";
    public const int BaselineAdjustment = 1;
    public const int TransformedAdjustment = 1000;
    public const int ProbeInput = 41;
    public const int TransformedExpectedResult = 1041;

    private readonly string _launcherDataRoot;
    private readonly string _bundleFixtureRoot;
    private readonly string _workRoot;
    private readonly SteamOfflineInstallInspection _offlineInspection;
    private FixtureSnapshot? _fixture;
    private TransformationSnapshot? _transformation;
    private ExecutionSnapshot? _execution;

    public AheadOfLoadManagedTransformation(string launcherDataRoot, string bundleFixtureRoot)
    {
        if (string.IsNullOrWhiteSpace(launcherDataRoot))
            throw new ArgumentException("Launcher data root is required.", nameof(launcherDataRoot));
        if (string.IsNullOrWhiteSpace(bundleFixtureRoot))
            throw new ArgumentException("Step 28 bundle fixture root is required.", nameof(bundleFixtureRoot));

        _launcherDataRoot = Path.GetFullPath(launcherDataRoot);
        _bundleFixtureRoot = Path.GetFullPath(bundleFixtureRoot);
        _workRoot = Path.Combine(_launcherDataRoot, WorkRootName);
        _offlineInspection = new SteamOfflineInstallInspection(_launcherDataRoot);
    }

    public void Reset()
    {
        _fixture = null;
        _transformation = null;
        _execution = null;
    }

    public async Task<AheadOfLoadManagedTransformationGateResult> RunFixtureAdmissionAndOfflineReadyAsync(
        IProgress<AheadOfLoadManagedTransformationProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Reset();
            cancellationToken.ThrowIfCancellationRequested();

            var alreadyLoaded = GetLoadedFixtureAssemblies();
            if (alreadyLoaded.Length != 0)
            {
                throw new InvalidOperationException(
                    "Step 28 requires a fresh process before Gate A. The Step-28 fixture identity is already CLR-loaded: " +
                    string.Join(" | ", alreadyLoaded));
            }

            progress?.Report(new AheadOfLoadManagedTransformationProgress(
                AheadOfLoadManagedTransformationGate.FixtureAdmissionAndOfflineReady,
                0,
                1,
                null,
                "Re-proving OfflineReady, then validating the post-publish Step-28 source fixture as Cecil metadata only before any CLR admission…"));

            IProgress<SteamOfflineInstallProgress>? offlineProgress = progress is null
                ? null
                : new CallbackProgress<SteamOfflineInstallProgress>(value =>
                    progress.Report(new AheadOfLoadManagedTransformationProgress(
                        AheadOfLoadManagedTransformationGate.FixtureAdmissionAndOfflineReady,
                        value.CompletedFiles,
                        value.TotalFiles,
                        value.CurrentFile,
                        $"OfflineReady precondition — {value.Message} ({value.CompletedBytes:N0}/{value.TotalBytes:N0} bytes)")));

            var offline = await _offlineInspection.RunAsync(offlineProgress, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            if (offline.Outcome == SteamOfflineInstallOutcome.Cancelled)
                throw new OperationCanceledException("Step 28 OfflineReady precondition was cancelled.", cancellationToken);
            if (!offline.Success)
                throw new InvalidDataException(offline.Error ?? $"Managed install is not OfflineReady ({offline.State}/{offline.Outcome}).");

            if (!Directory.Exists(_bundleFixtureRoot))
                throw new DirectoryNotFoundException($"Step 28 bundled fixture directory is missing: {_bundleFixtureRoot}");

            var bundlePath = Path.Combine(_bundleFixtureRoot, FixtureFileName);
            var manifestPath = Path.Combine(_bundleFixtureRoot, ManifestFileName);
            if (!File.Exists(bundlePath) || new FileInfo(bundlePath).Length == 0)
                throw new FileNotFoundException("Step 28 bundled fixture is missing or empty.", bundlePath);
            if (!File.Exists(manifestPath))
                throw new FileNotFoundException("Step 28 fixture manifest is missing.", manifestPath);

            var manifestSha256 = ParseSingleSha256Manifest(manifestPath, FixtureFileName);
            var bundleSha256 = ComputeSha256Hex(bundlePath);
            if (!bundleSha256.Equals(manifestSha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Step 28 bundled fixture SHA-256 does not match its post-publish manifest.");

            using (var module = ReadFixtureModule(bundlePath))
                ValidateSourceFixtureShape(module);

            PrepareFreshWorkRoot();
            var sourceRoot = Path.Combine(_workRoot, SourceRootName);
            Directory.CreateDirectory(sourceRoot);
            var sourcePath = Path.Combine(sourceRoot, FixtureFileName);
            await CopyFileAsync(bundlePath, sourcePath, cancellationToken).ConfigureAwait(false);
            var sourceSha256 = ComputeSha256Hex(sourcePath);
            if (!sourceSha256.Equals(bundleSha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Step 28 launcher-private source clone differs from the verified bundled fixture.");

            _fixture = new FixtureSnapshot(
                offline,
                bundlePath,
                bundleSha256,
                sourcePath,
                sourceSha256,
                new FileInfo(sourcePath).Length);

            return Pass(
                AheadOfLoadManagedTransformationGate.FixtureAdmissionAndOfflineReady,
                "POST-PUBLISH SOURCE FIXTURE ADMITTED AS METADATA ONLY; ORIGINAL IMAGE HAS NOT ENTERED THE CLR.\n" +
                $"OfflineReady precondition: PASS ({offline.VerifiedFiles:N0}/{offline.PlannedFiles:N0} files)\n" +
                $"Fixture: {FixtureFileName}\n" +
                $"Fixture SHA-256: {bundleSha256}\n" +
                $"Fixture bytes: {_fixture.Bytes:N0}\n" +
                $"Source metadata: Adjustment() => {BaselineAdjustment}; Target(value) calls Adjustment(); InvokeTarget(value) directly calls Target(value)\n" +
                $"Private source clone: {WorkRootName}/{SourceRootName}/{FixtureFileName}\n" +
                "Fixture assembly already loaded before Gate A: NO\n" +
                "Bundle fixture CLR-loaded by Gate A: NO\n" +
                "Private source clone CLR-loaded by Gate A: NO\n" +
                "Real StS2 assembly/type/member reflected or invoked: NO\n" +
                "Trusted Step 12 managed install modified: NO");
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
            return Fail(AheadOfLoadManagedTransformationGate.FixtureAdmissionAndOfflineReady, ex);
        }
    }

    public AheadOfLoadManagedTransformationGateResult RunDeterministicRewrite()
    {
        try
        {
            var fixture = RequireFixture();
            EnsureFixtureStillNotLoaded("Gate B entry");
            var sourceShaBefore = ComputeSha256Hex(fixture.SourcePath);
            if (!sourceShaBefore.Equals(fixture.SourceSha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Step 28 private source clone changed before transformation.");

            var transformedRoot = Path.Combine(_workRoot, TransformedRootName);
            Directory.CreateDirectory(transformedRoot);
            var transformedPath = Path.Combine(transformedRoot, FixtureFileName);

            using (var module = ReadFixtureModule(fixture.SourcePath))
            {
                ValidateSourceFixtureShape(module);
                var probeType = FindProbeType(module);
                var adjustment = FindMethod(probeType, "Adjustment", 0);
                var instructions = adjustment.Body.Instructions;
                instructions[0].OpCode = Mono.Cecil.Cil.OpCodes.Ldc_I4;
                instructions[0].Operand = TransformedAdjustment;
                module.Write(transformedPath);
            }

            if (!File.Exists(transformedPath) || new FileInfo(transformedPath).Length == 0)
                throw new InvalidDataException("Step 28 transformed output was not created.");

            var sourceShaAfter = ComputeSha256Hex(fixture.SourcePath);
            var transformedSha256 = ComputeSha256Hex(transformedPath);
            if (!sourceShaAfter.Equals(fixture.SourceSha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Step 28 source clone changed while writing the transformed copy.");
            if (transformedSha256.Equals(fixture.SourceSha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Step 28 transformed output is byte-identical to the source image; the intended semantic rewrite was not materialized.");

            _transformation = new TransformationSnapshot(
                transformedPath,
                transformedSha256,
                new FileInfo(transformedPath).Length);

            return Pass(
                AheadOfLoadManagedTransformationGate.DeterministicRewrite,
                "DETERMINISTIC AHEAD-OF-LOAD CECIL TRANSFORMATION WRITTEN TO A NEW PRIVATE IMAGE.\n" +
                $"Transformation: {FixtureTypeFullName}::Adjustment() constant {BaselineAdjustment} -> {TransformedAdjustment}\n" +
                $"Source SHA-256 preserved: {fixture.SourceSha256}\n" +
                $"Transformed SHA-256: {transformedSha256}\n" +
                $"Transformed bytes: {_transformation.Bytes:N0}\n" +
                $"Output: {WorkRootName}/{TransformedRootName}/{FixtureFileName}\n" +
                "Source/bundle mutation: NO\n" +
                "Assembly.Load/LoadFromStream during transformation: NO\n" +
                "Harmony/MonoMod runtime patch API invoked: NO\n" +
                "Real StS2 member touched: NO");
        }
        catch (Exception ex)
        {
            return Fail(AheadOfLoadManagedTransformationGate.DeterministicRewrite, ex);
        }
    }

    public AheadOfLoadManagedTransformationGateResult RunTransformedImageVerification()
    {
        try
        {
            var fixture = RequireFixture();
            var transformed = RequireTransformation();
            EnsureFixtureStillNotLoaded("Gate C entry");

            if (!ComputeSha256Hex(fixture.BundlePath).Equals(fixture.BundleSha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Step 28 bundled source fixture changed after transformation.");
            if (!ComputeSha256Hex(fixture.SourcePath).Equals(fixture.SourceSha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Step 28 private source clone changed after transformation.");
            if (!ComputeSha256Hex(transformed.Path).Equals(transformed.Sha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Step 28 transformed image changed before verification.");

            using (var source = ReadFixtureModule(fixture.SourcePath))
                ValidateSourceFixtureShape(source);
            using (var transformedModule = ReadFixtureModule(transformed.Path))
                ValidateTransformedFixtureShape(transformedModule);

            return Pass(
                AheadOfLoadManagedTransformationGate.TransformedImageVerification,
                "TRANSFORMED IMAGE REOPENED AND VERIFIED BEFORE CLR ADMISSION.\n" +
                $"Source Adjustment() remains: {BaselineAdjustment}\n" +
                $"Transformed Adjustment() is: {TransformedAdjustment}\n" +
                "Target(value) still directly calls Adjustment(): YES\n" +
                "InvokeTarget(value) still directly calls Target(value): YES\n" +
                "Unexpected P/Invoke methods in source/transformed fixture: 0\n" +
                "Bundle/source SHA-256 unchanged: YES\n" +
                "Transformed SHA-256 unchanged: YES\n" +
                "Original source image CLR-loaded: NO\n" +
                "Harmony runtime replacement/detour used: NO");
        }
        catch (Exception ex)
        {
            return Fail(AheadOfLoadManagedTransformationGate.TransformedImageVerification, ex);
        }
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Step 28 intentionally executes a hash-verified post-publish project-owned fixture through the Mono interpreter; its metadata is outside the build-time trimmer graph.")]
    public AheadOfLoadManagedTransformationGateResult RunTransformedExecution()
    {
        try
        {
            var fixture = RequireFixture();
            var transformed = RequireTransformation();
            EnsureFixtureStillNotLoaded("Gate D entry");
            if (!ComputeSha256Hex(transformed.Path).Equals(transformed.Sha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Step 28 transformed image SHA-256 changed immediately before CLR load.");

            var context = new Step28TransformedLoadContext("StS2Launcher-Step28-AheadOfLoadTransformation");
            Assembly assembly;
            using (var stream = new MemoryStream(File.ReadAllBytes(transformed.Path), writable: false))
                assembly = context.LoadFromStream(stream);

            if (!string.Equals(assembly.GetName().Name, FixtureAssemblySimpleName, StringComparison.Ordinal))
                throw new InvalidDataException($"Step 28 loaded unexpected assembly identity: {assembly.FullName}");
            if (!ReferenceEquals(AssemblyLoadContext.GetLoadContext(assembly), context))
                throw new InvalidDataException("Step 28 transformed fixture did not load into the dedicated private AssemblyLoadContext.");

            var type = assembly.GetType(FixtureTypeFullName, throwOnError: true, ignoreCase: false)
                ?? throw new MissingMemberException(FixtureTypeFullName);
            var target = GetExactPublicStaticIntMethod(type, "Target");
            var invokeTarget = GetExactPublicStaticIntMethod(type, "InvokeTarget");
            var adjustment = type.GetMethod("Adjustment", BindingFlags.Public | BindingFlags.Static, binder: null, types: Type.EmptyTypes, modifiers: null)
                ?? throw new MissingMethodException(FixtureTypeFullName, "Adjustment");
            if (adjustment.ReturnType != typeof(int))
                throw new InvalidDataException("Step 28 transformed Adjustment() return type drifted.");

            var adjustmentResult = InvokeInt32(adjustment, null);
            var targetResult = InvokeInt32(target, ProbeInput);
            var directCallResult = InvokeInt32(invokeTarget, ProbeInput);
            if (adjustmentResult != TransformedAdjustment || targetResult != TransformedExpectedResult || directCallResult != TransformedExpectedResult)
            {
                throw new InvalidDataException(
                    $"Step 28 transformed execution mismatch: Adjustment={adjustmentResult}, Target={targetResult}, InvokeTarget={directCallResult}; expected {TransformedAdjustment}/{TransformedExpectedResult}/{TransformedExpectedResult}.");
            }

            var loadedMatches = GetLoadedFixtureAssemblyObjects();
            if (loadedMatches.Length != 1 || !ReferenceEquals(loadedMatches[0], assembly))
                throw new InvalidDataException($"Expected exactly one CLR-loaded Step-28 fixture assembly after Gate D, found {loadedMatches.Length}.");

            _execution = new ExecutionSnapshot(
                context,
                assembly,
                context.RequestedAssemblyNames.ToArray(),
                adjustmentResult,
                targetResult,
                directCallResult);

            return Pass(
                AheadOfLoadManagedTransformationGate.TransformedExecution,
                "ONLY THE VERIFIED TRANSFORMED IMAGE ENTERED THE CLR, AND BOTH EXECUTION ROUTES OBSERVED THE REWRITE.\n" +
                $"Load context: {context.Name}\n" +
                $"Loaded identity: {assembly.FullName}\n" +
                $"Adjustment() result: {adjustmentResult}\n" +
                $"Target({ProbeInput}) reflection result: {targetResult}\n" +
                $"InvokeTarget({ProbeInput}) in-fixture direct-call result: {directCallResult}\n" +
                $"Expected transformed behavior: value + {TransformedAdjustment} = {TransformedExpectedResult}\n" +
                "Both routes executed post-publish transformed managed IL: YES\n" +
                "CLR-loaded Step-28 fixture identities: exactly 1\n" +
                "Original bundled/private-source bytes CLR-loaded: NO\n" +
                $"Managed dependency requests observed by private context: {context.RequestedAssemblyNames.Count}\n" +
                "Unexpected private dependency fallback: NO\n" +
                "Harmony/MonoMod loaded or invoked by Step 28: NO");
        }
        catch (Exception ex)
        {
            return Fail(AheadOfLoadManagedTransformationGate.TransformedExecution, ex);
        }
    }

    public async Task<AheadOfLoadManagedTransformationGateResult> RunFinalIsolationAuditAsync(
        IProgress<AheadOfLoadManagedTransformationProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var fixture = RequireFixture();
            var transformed = RequireTransformation();
            var execution = RequireExecution();
            cancellationToken.ThrowIfCancellationRequested();

            progress?.Report(new AheadOfLoadManagedTransformationProgress(
                AheadOfLoadManagedTransformationGate.FinalIsolationAudit,
                0,
                1,
                null,
                "Re-hashing bundle/source/transformed images and re-proving OfflineReady after transformed execution…"));

            if (!ComputeSha256Hex(fixture.BundlePath).Equals(fixture.BundleSha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Step 28 bundled fixture changed after execution.");
            if (!ComputeSha256Hex(fixture.SourcePath).Equals(fixture.SourceSha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Step 28 private source clone changed after execution.");
            if (!ComputeSha256Hex(transformed.Path).Equals(transformed.Sha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Step 28 transformed image changed after execution.");

            var loadedMatches = GetLoadedFixtureAssemblyObjects();
            if (loadedMatches.Length != 1 || !ReferenceEquals(loadedMatches[0], execution.Assembly))
                throw new InvalidDataException("Step 28 CLR fixture membership changed after transformed execution.");
            if (!ReferenceEquals(AssemblyLoadContext.GetLoadContext(execution.Assembly), execution.Context))
                throw new InvalidDataException("Step 28 transformed fixture escaped its dedicated private AssemblyLoadContext.");

            var offline = await _offlineInspection.RunAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            if (offline.Outcome == SteamOfflineInstallOutcome.Cancelled)
                throw new OperationCanceledException("Step 28 final OfflineReady audit was cancelled.", cancellationToken);
            if (!offline.Success)
                throw new InvalidDataException(offline.Error ?? $"Final OfflineReady audit failed ({offline.State}/{offline.Outcome}).");

            return Pass(
                AheadOfLoadManagedTransformationGate.FinalIsolationAudit,
                "STEP 28.0 FINAL ISOLATION AUDIT PASSED.\n" +
                $"Bundle fixture SHA-256 unchanged: {fixture.BundleSha256}\n" +
                $"Private source SHA-256 unchanged: {fixture.SourceSha256}\n" +
                $"Transformed image SHA-256 unchanged: {transformed.Sha256}\n" +
                $"Post-execution OfflineReady: PASS ({offline.VerifiedFiles:N0}/{offline.PlannedFiles:N0} files)\n" +
                "Trusted Step 12 managed install unchanged: YES\n" +
                "Exactly one Step-28 fixture identity CLR-loaded: YES — transformed image only\n" +
                "Dedicated private load context retained: YES\n" +
                $"Managed framework requests delegated to host: {execution.RequestedAssemblyNames.Count}\n" +
                "Unexpected private dependency resolution: NO\n" +
                "Real StS2 assembly/type/member reflection or invocation by Step 28: NO\n" +
                "Harmony/MonoMod runtime patching by Step 28: NO\n" +
                "Architecture result: deterministic ahead-of-load semantic transformation + post-publish interpreted execution is physically testable without runtime detours.");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return Fail(AheadOfLoadManagedTransformationGate.FinalIsolationAudit, ex);
        }
    }

    private static ModuleDefinition ReadFixtureModule(string path)
        => ModuleDefinition.ReadModule(path, new ReaderParameters
        {
            ReadingMode = ReadingMode.Immediate,
            AssemblyResolver = RejectingAssemblyResolver.Instance,
        });

    private static void ValidateSourceFixtureShape(ModuleDefinition module)
    {
        ValidateCommonFixtureShape(module);
        var probe = FindProbeType(module);
        var adjustment = FindMethod(probe, "Adjustment", 0);
        if (!HasExactIntConstantReturn(adjustment, BaselineAdjustment))
            throw new InvalidDataException($"Step 28 source Adjustment() must be exact constant {BaselineAdjustment} -> ret IL.");
    }

    private static void ValidateTransformedFixtureShape(ModuleDefinition module)
    {
        ValidateCommonFixtureShape(module);
        var probe = FindProbeType(module);
        var adjustment = FindMethod(probe, "Adjustment", 0);
        if (!HasExactIntConstantReturn(adjustment, TransformedAdjustment))
            throw new InvalidDataException($"Step 28 transformed Adjustment() must be exact constant {TransformedAdjustment} -> ret IL.");
    }

    private static void ValidateCommonFixtureShape(ModuleDefinition module)
    {
        if (module.Assembly is null || !string.Equals(module.Assembly.Name.Name, FixtureAssemblySimpleName, StringComparison.Ordinal))
            throw new InvalidDataException($"Unexpected Step 28 fixture assembly identity: {module.Assembly?.Name.FullName ?? "<module-without-assembly>"}");
        if (module.EntryPoint is not null)
            throw new InvalidDataException("Step 28 fixture must remain a class library without an entry point.");
        if (EnumerateTypes(module.Types).SelectMany(type => type.Methods).Any(method => method.IsPInvokeImpl || method.PInvokeInfo is not null))
            throw new InvalidDataException("Step 28 fixture unexpectedly contains P/Invoke metadata.");

        var probe = FindProbeType(module);
        var adjustment = FindMethod(probe, "Adjustment", 0);
        var target = FindMethod(probe, "Target", 1);
        var invokeTarget = FindMethod(probe, "InvokeTarget", 1);
        if (adjustment.ReturnType.MetadataType != MetadataType.Int32 || target.ReturnType.MetadataType != MetadataType.Int32 || invokeTarget.ReturnType.MetadataType != MetadataType.Int32)
            throw new InvalidDataException("Step 28 fixture return-type surface drifted.");
        if (target.Parameters[0].ParameterType.MetadataType != MetadataType.Int32 || invokeTarget.Parameters[0].ParameterType.MetadataType != MetadataType.Int32)
            throw new InvalidDataException("Step 28 fixture parameter-type surface drifted.");
        if (!HasDirectCall(target, adjustment))
            throw new InvalidDataException("Step 28 Target(value) must retain a direct managed IL call to Adjustment().");
        if (!HasDirectCall(invokeTarget, target))
            throw new InvalidDataException("Step 28 InvokeTarget(value) must retain a direct managed IL call to Target().");
    }

    private static TypeDefinition FindProbeType(ModuleDefinition module)
        => EnumerateTypes(module.Types).SingleOrDefault(type => type.FullName.Equals(FixtureTypeFullName, StringComparison.Ordinal))
           ?? throw new MissingMemberException(FixtureTypeFullName);

    private static MethodDefinition FindMethod(TypeDefinition type, string name, int parameterCount)
        => type.Methods.SingleOrDefault(method =>
               method.Name.Equals(name, StringComparison.Ordinal) &&
               method.IsPublic && method.IsStatic &&
               method.Parameters.Count == parameterCount &&
               method.HasBody)
           ?? throw new MissingMethodException(type.FullName, name);

    private static bool HasDirectCall(MethodDefinition caller, MethodDefinition target)
        => caller.Body.Instructions.Any(instruction =>
            instruction.OpCode.Code is Code.Call or Code.Callvirt &&
            instruction.Operand is MethodReference method &&
            method.FullName.Equals(target.FullName, StringComparison.Ordinal));

    private static bool HasExactIntConstantReturn(MethodDefinition method, int value)
    {
        var instructions = method.Body.Instructions;
        if (instructions.Count != 2 || instructions[1].OpCode.Code != Code.Ret)
            return false;
        return TryReadInt32Constant(instructions[0], out var actual) && actual == value;
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

    private static MethodInfo GetExactPublicStaticIntMethod(Type type, string name)
    {
        var method = type.GetMethod(name, BindingFlags.Public | BindingFlags.Static, binder: null, types: [typeof(int)], modifiers: null)
            ?? throw new MissingMethodException(type.FullName, name);
        if (method.ReturnType != typeof(int))
            throw new InvalidDataException($"Step 28 method {name} return type drifted.");
        return method;
    }

    private static int InvokeInt32(MethodInfo method, int? argument)
    {
        object? result = argument.HasValue ? method.Invoke(null, [argument.Value]) : method.Invoke(null, null);
        return result is int value
            ? value
            : throw new InvalidDataException($"Step 28 method {method.Name} did not return Int32.");
    }

    private void EnsureFixtureStillNotLoaded(string stage)
    {
        var loaded = GetLoadedFixtureAssemblies();
        if (loaded.Length != 0)
            throw new InvalidOperationException($"Step 28 original/transformed fixture identity entered the CLR before the intended Gate-D load ({stage}): {string.Join(" | ", loaded)}");
    }

    private static string[] GetLoadedFixtureAssemblies()
        => GetLoadedFixtureAssemblyObjects()
            .Select(assembly => $"{assembly.FullName} @ {AssemblyLoadContext.GetLoadContext(assembly)?.Name ?? "<unknown-context>"}")
            .OrderBy(value => value, StringComparer.Ordinal)
            .ToArray();

    private static Assembly[] GetLoadedFixtureAssemblyObjects()
        => AppDomain.CurrentDomain.GetAssemblies()
            .Where(assembly => string.Equals(assembly.GetName().Name, FixtureAssemblySimpleName, StringComparison.Ordinal))
            .ToArray();

    private FixtureSnapshot RequireFixture()
        => _fixture ?? throw new InvalidOperationException("Step 28 Gate A must pass before this gate runs.");

    private TransformationSnapshot RequireTransformation()
        => _transformation ?? throw new InvalidOperationException("Step 28 Gate B must pass before this gate runs.");

    private ExecutionSnapshot RequireExecution()
        => _execution ?? throw new InvalidOperationException("Step 28 Gate D must pass before this gate runs.");

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
            // A later exact-path create/write will surface a durable error if stale scratch remains.
        }
    }

    private static async Task CopyFileAsync(string source, string destination, CancellationToken cancellationToken)
    {
        await using var input = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.Read, 128 * 1024, useAsync: true);
        await using var output = new FileStream(destination, FileMode.CreateNew, FileAccess.Write, FileShare.None, 128 * 1024, useAsync: true);
        await input.CopyToAsync(output, 128 * 1024, cancellationToken).ConfigureAwait(false);
        await output.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    private static string ParseSingleSha256Manifest(string path, string expectedFileName)
    {
        var lines = File.ReadAllLines(path).Where(line => !string.IsNullOrWhiteSpace(line)).ToArray();
        if (lines.Length != 1)
            throw new InvalidDataException($"Step 28 fixture manifest must contain exactly one non-empty row, found {lines.Length}.");
        var match = System.Text.RegularExpressions.Regex.Match(lines[0], "^([0-9a-fA-F]{64})\\s+\\*?(.+)$");
        if (!match.Success || !string.Equals(match.Groups[2].Value.Trim(), expectedFileName, StringComparison.Ordinal))
            throw new InvalidDataException("Step 28 fixture manifest row is malformed or names the wrong file.");
        return match.Groups[1].Value.ToLowerInvariant();
    }

    private static string ComputeSha256Hex(string path)
        => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();

    private static AheadOfLoadManagedTransformationGateResult Pass(AheadOfLoadManagedTransformationGate gate, string detail)
        => new(gate, true, detail);

    private static AheadOfLoadManagedTransformationGateResult Fail(AheadOfLoadManagedTransformationGate gate, Exception ex)
        => new(gate, false, $"Stage failed with {ex.GetType().Name}: {ex.Message}\n{ex}");

    private sealed class RejectingAssemblyResolver : IAssemblyResolver
    {
        public static RejectingAssemblyResolver Instance { get; } = new();
        public AssemblyDefinition Resolve(AssemblyNameReference name)
            => throw new AssemblyResolutionException(name);
        public AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
            => throw new AssemblyResolutionException(name);
        public void Dispose() { }
    }

    private sealed class Step28TransformedLoadContext : AssemblyLoadContext
    {
        private readonly List<string> _requestedAssemblyNames = [];

        public Step28TransformedLoadContext(string name) : base(name, isCollectible: false) { }

        public IReadOnlyList<string> RequestedAssemblyNames => _requestedAssemblyNames;

        protected override Assembly? Load(AssemblyName assemblyName)
        {
            var requested = assemblyName.FullName ?? assemblyName.Name ?? "<unknown>";
            if (!_requestedAssemblyNames.Contains(requested, StringComparer.Ordinal))
                _requestedAssemblyNames.Add(requested);

            if (assemblyName.Name is not null && IsFrameworkContractName(assemblyName.Name))
                return null;

            throw new FileLoadException(
                $"Step 28 private load context refuses non-framework dependency fallback for '{requested}'. The fixture is required to be self-contained apart from host framework contracts.");
        }

        private static bool IsFrameworkContractName(string simpleName)
            => simpleName.Equals("mscorlib", StringComparison.OrdinalIgnoreCase) ||
               simpleName.Equals("netstandard", StringComparison.OrdinalIgnoreCase) ||
               simpleName.Equals("System.Private.CoreLib", StringComparison.OrdinalIgnoreCase) ||
               simpleName.Equals("System.Runtime", StringComparison.OrdinalIgnoreCase) ||
               simpleName.Equals("System.Console", StringComparison.OrdinalIgnoreCase) ||
               simpleName.StartsWith("System.", StringComparison.OrdinalIgnoreCase) ||
               simpleName.StartsWith("Microsoft.", StringComparison.OrdinalIgnoreCase);
    }

    private sealed record FixtureSnapshot(
        SteamOfflineInstallResult Offline,
        string BundlePath,
        string BundleSha256,
        string SourcePath,
        string SourceSha256,
        long Bytes);

    private sealed record TransformationSnapshot(string Path, string Sha256, long Bytes);

    private sealed record ExecutionSnapshot(
        Step28TransformedLoadContext Context,
        Assembly Assembly,
        IReadOnlyList<string> RequestedAssemblyNames,
        int AdjustmentResult,
        int TargetResult,
        int DirectCallResult);
}
