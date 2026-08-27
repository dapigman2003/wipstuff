using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace StS2Launcher.Core;

/// <summary>
/// Step 32.0 boundary. Physical Step 31 closed the exact OneTimeInitialization::PrewarmJit()
/// PrepareMethod family positively for rewrite design. Step 32 performs the first real-StS2 semantic
/// transformation, but only on a launcher-private copy and without CLR admission. Each of the six
/// PrepareMethod(handle) calls becomes one Pop; each of the four PrepareMethod(handle, instantiation[])
/// calls becomes two Pops. The replacement consumes exactly the stack values consumed by the original
/// void call while retaining the reflection/method-handle discovery that precedes it. The receipt-backed
/// Step-12 install remains immutable; no Harmony/MonoMod runtime patching, Godot/game startup, native
/// loading, or real-StS2 CLR load/invocation is part of this boundary.
/// </summary>
public sealed class RealStS2PrepareMethodRewrite
{
    public const string WorkRootName = "Step32-RealStS2PrepareMethodRewrite";
    public const string SourceRootName = "source";
    public const string TransformedRootName = "transformed";
    public const string PrimaryFileName = "sts2.dll";
    private const string CecilWriteSystemRuntimeIdentity = "System.Runtime, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
    private const string CecilWriteSentryIdentity = "Sentry, Version=5.0.0.0, Culture=neutral, PublicKeyToken=fba2ec45388e2af0";
    private static readonly IReadOnlyDictionary<ExternalConstantTypeKey, TypeCode> AuditedExternalConstantTypeRequirements =
        new Dictionary<ExternalConstantTypeKey, TypeCode>
        {
            [new(CecilWriteSystemRuntimeIdentity, "System.Reflection.BindingFlags", false)] = TypeCode.Int32,
            [new(CecilWriteSentryIdentity, "Sentry.BreadcrumbLevel", false)] = TypeCode.Int32,
            [new(CecilWriteSentryIdentity, "Sentry.SentryLevel", false)] = TypeCode.Int16,
        };

    internal static readonly RewriteEvidence PhysicalStep31Evidence = new(
        SourceSha1: "e424ace9399a82edea4dd7e0fa5761635dfd6c5d",
        SourceSha256: "e7ceb80669bfaf5c8fccabaa126ae2bb283aba514be5b5b55612579cfd285f18",
        SourceBytes: 9_363_456,
        AssemblyIdentity: "sts2, Version=0.1.0.0, Culture=neutral, PublicKeyToken=null",
        Mvid: Guid.Parse("518e4758-52d7-47c2-b776-471a0e29e49d"),
        SourceType: "MegaCrit.Sts2.Core.Helpers.OneTimeInitialization",
        SourceMethod: "System.Void MegaCrit.Sts2.Core.Helpers.OneTimeInitialization::PrewarmJit()",
        MethodToken: 0x06007D05,
        MethodBodySha256: "7f25b7bd955c407fc69306cf26af2162223353f5606560458066aed085e72ab9",
        SourceInstructionCount: 117,
        SourceExceptionHandlerCount: 2,
        Sites:
        [
            new(0x003D, 1, "System.Void System.Runtime.CompilerServices.RuntimeHelpers::PrepareMethod(System.RuntimeMethodHandle)"),
            new(0x0052, 1, "System.Void System.Runtime.CompilerServices.RuntimeHelpers::PrepareMethod(System.RuntimeMethodHandle)"),
            new(0x007A, 2, "System.Void System.Runtime.CompilerServices.RuntimeHelpers::PrepareMethod(System.RuntimeMethodHandle,System.RuntimeTypeHandle[])"),
            new(0x00A2, 2, "System.Void System.Runtime.CompilerServices.RuntimeHelpers::PrepareMethod(System.RuntimeMethodHandle,System.RuntimeTypeHandle[])"),
            new(0x00CA, 2, "System.Void System.Runtime.CompilerServices.RuntimeHelpers::PrepareMethod(System.RuntimeMethodHandle,System.RuntimeTypeHandle[])"),
            new(0x00F2, 2, "System.Void System.Runtime.CompilerServices.RuntimeHelpers::PrepareMethod(System.RuntimeMethodHandle,System.RuntimeTypeHandle[])"),
            new(0x0136, 1, "System.Void System.Runtime.CompilerServices.RuntimeHelpers::PrepareMethod(System.RuntimeMethodHandle)"),
            new(0x014C, 1, "System.Void System.Runtime.CompilerServices.RuntimeHelpers::PrepareMethod(System.RuntimeMethodHandle)"),
            new(0x0162, 1, "System.Void System.Runtime.CompilerServices.RuntimeHelpers::PrepareMethod(System.RuntimeMethodHandle)"),
            new(0x0178, 1, "System.Void System.Runtime.CompilerServices.RuntimeHelpers::PrepareMethod(System.RuntimeMethodHandle)"),
        ]);

    private readonly string _launcherDataRoot;
    private readonly string _workRoot;
    private readonly SteamOfflineInstallInspection _offlineInspection;
    private readonly RewriteEvidence _expected;
    private SourceSnapshot? _source;
    private TransformationSnapshot? _transformation;
    private VerificationSnapshot? _verification;

    public RealStS2PrepareMethodRewrite(string launcherDataRoot)
        : this(launcherDataRoot, PhysicalStep31Evidence)
    {
    }

    internal RealStS2PrepareMethodRewrite(string launcherDataRoot, RewriteEvidence expected)
    {
        if (string.IsNullOrWhiteSpace(launcherDataRoot))
            throw new ArgumentException("Launcher data root is required.", nameof(launcherDataRoot));
        _launcherDataRoot = Path.GetFullPath(launcherDataRoot);
        _workRoot = Path.Combine(_launcherDataRoot, WorkRootName);
        _offlineInspection = new SteamOfflineInstallInspection(_launcherDataRoot);
        _expected = expected ?? throw new ArgumentNullException(nameof(expected));
    }

    public void Reset()
    {
        _source = null;
        _transformation = null;
        _verification = null;
    }

    public async Task<RealStS2PrepareMethodRewriteGateResult> RunSourceAdmissionAndPrivateCloneAsync(
        IProgress<RealStS2PrepareMethodRewriteProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        const RealStS2PrepareMethodRewriteGate gate = RealStS2PrepareMethodRewriteGate.SourceAdmissionAndPrivateClone;
        try
        {
            Reset();
            cancellationToken.ThrowIfCancellationRequested();
            progress?.Report(new(gate, 0, 0, null, "Re-proving OfflineReady and cloning the exact receipt-backed ARM64 sts2.dll into launcher-private Step-32 storage…"));

            IProgress<SteamOfflineInstallProgress>? offlineProgress = progress is null
                ? null
                : new CallbackProgress<SteamOfflineInstallProgress>(value =>
                    progress.Report(new RealStS2PrepareMethodRewriteProgress(
                        gate, value.CompletedFiles, value.TotalFiles, value.CurrentFile,
                        $"OfflineReady precondition — {value.Message} ({value.CompletedBytes:N0}/{value.TotalBytes:N0} bytes)")));

            var offline = await _offlineInspection.RunAsync(offlineProgress, cancellationToken).ConfigureAwait(false);
            if (offline.Outcome == SteamOfflineInstallOutcome.Cancelled)
                throw new OperationCanceledException("Step 32 OfflineReady precondition was cancelled.", cancellationToken);
            if (!offline.Success || string.IsNullOrWhiteSpace(offline.ManagedInstallRelativePath))
                throw new InvalidDataException(offline.Error ?? $"Managed install is not OfflineReady ({offline.State}/{offline.Outcome}).");

            var managedRoot = ResolveChildPath(_launcherDataRoot, offline.ManagedInstallRelativePath);
            var receipt = await ReadReceiptAsync(managedRoot, cancellationToken).ConfigureAwait(false);
            ValidateReceiptSnapshot(receipt, offline);
            var matches = receipt.Files.Where(file => IsPrimaryArm64StS2Path(file.RelativePath)).ToArray();
            if (matches.Length != 1)
                throw new InvalidDataException($"Expected exactly one receipt-backed macOS arm64 sts2.dll, found {matches.Length}.");
            var primary = matches[0];
            var primaryRelative = primary.RelativePath.Replace('\\', '/');
            var primaryPath = ResolveChildPath(managedRoot, primaryRelative);
            if (!File.Exists(primaryPath))
                throw new FileNotFoundException("The receipt-backed primary sts2.dll is missing.", primaryPath);

            var sourceBytes = new FileInfo(primaryPath).Length;
            var sourceSha1 = await ComputeHashHexAsync(primaryPath, SHA1.Create(), cancellationToken).ConfigureAwait(false);
            var sourceSha256 = await ComputeHashHexAsync(primaryPath, SHA256.Create(), cancellationToken).ConfigureAwait(false);
            if (sourceBytes != primary.Length || !sourceSha1.Equals(primary.Sha1Hex, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Primary sts2.dll no longer matches its trusted Step-12 receipt length/SHA-1.");
            ValidatePhysicalSourceIdentity(sourceBytes, sourceSha1, sourceSha256);
            EnsureStS2NotLoaded("Gate A entry");

            using (var resolver = new RejectingAssemblyResolver())
            using (var module = ReadModuleDeferred(primaryPath, resolver))
            {
                ValidateModuleAndMethod(module, requireExactOffsets: true);
                if (resolver.Requests.Count != 0)
                    throw new InvalidDataException($"Cecil dependency resolution occurred during Step-32 source admission: {string.Join(", ", resolver.Requests)}");
            }

            PrepareFreshWorkRoot();
            var privateSourceRoot = Path.Combine(_workRoot, SourceRootName);
            Directory.CreateDirectory(privateSourceRoot);
            var privateSourcePath = Path.Combine(privateSourceRoot, PrimaryFileName);
            await CopyFileAsync(primaryPath, privateSourcePath, cancellationToken).ConfigureAwait(false);
            var privateSourceSha256 = await ComputeHashHexAsync(privateSourcePath, SHA256.Create(), cancellationToken).ConfigureAwait(false);
            if (!privateSourceSha256.Equals(sourceSha256, StringComparison.OrdinalIgnoreCase) || new FileInfo(privateSourcePath).Length != sourceBytes)
                throw new InvalidDataException("Launcher-private Step-32 source clone does not exactly match the receipt-backed source image.");
            if (!ComputeSha256Hex(primaryPath).Equals(sourceSha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Receipt-backed sts2.dll changed while creating the Step-32 private clone.");
            EnsureStS2NotLoaded("Gate A exit");

            _source = new SourceSnapshot(offline, managedRoot, primaryRelative, primaryPath, privateSourcePath, sourceBytes, sourceSha1, sourceSha256);
            return Pass(gate,
                "EXACT RECEIPT-BACKED REAL STS2 SOURCE VERIFIED AND CLONED TO LAUNCHER-PRIVATE STORAGE; NO CLR ADMISSION OCCURRED.\n" +
                $"OfflineReady precondition: PASS ({offline.VerifiedFiles:N0}/{offline.PlannedFiles:N0} files)\n" +
                $"Primary: {primaryRelative}\n" +
                $"Assembly identity: {_expected.AssemblyIdentity}\n" +
                $"Module MVID: {_expected.Mvid}\n" +
                $"Receipt SHA-1: {sourceSha1}\n" +
                $"Source SHA-256: {sourceSha256}\n" +
                $"Source bytes: {sourceBytes:N0}\n" +
                $"Bound method: {_expected.SourceMethod} [0x{_expected.MethodToken:X8}]\n" +
                $"Bound method-body fingerprint SHA-256: {_expected.MethodBodySha256}\n" +
                $"PrepareMethod sites rebound: {_expected.Sites.Count}/{_expected.Sites.Count}\n" +
                $"Private source clone: {WorkRootName}/{SourceRootName}/{PrimaryFileName}\n" +
                "Branch targets entering selected PrepareMethod calls: 0\n" +
                "Cecil reading mode: Deferred\n" +
                "Cecil dependency resolution requests: 0\n" +
                "sts2 CLR-loaded before/after Gate A: NO / NO\n" +
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
            return Fail(gate, ex);
        }
    }

    public RealStS2PrepareMethodRewriteGateResult RunDeterministicStackNeutralRewrite()
    {
        const RealStS2PrepareMethodRewriteGate gate = RealStS2PrepareMethodRewriteGate.DeterministicStackNeutralRewrite;
        try
        {
            var source = RequireSource();
            EnsureStS2NotLoaded("Gate B entry");
            if (!ComputeSha256Hex(source.PrimaryPath).Equals(source.SourceSha256, StringComparison.OrdinalIgnoreCase) ||
                !ComputeSha256Hex(source.PrivateSourcePath).Equals(source.SourceSha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Step-32 source identity changed before transformation.");

            var transformedRoot = Path.Combine(_workRoot, TransformedRootName);
            if (Directory.Exists(transformedRoot)) Directory.Delete(transformedRoot, recursive: true);
            Directory.CreateDirectory(transformedRoot);
            var transformedPath = Path.Combine(transformedRoot, PrimaryFileName);

            int oneArgumentReplacements;
            int twoArgumentReplacements;
            int sourcePopCount;
            string expectedTransformedSemanticSha256;
            string expectedConstantMetadataSha256;
            int expectedTransformedInstructionCount;
            int writeResolutionRequestCount;
            int syntheticConstantTypeCount;
            int approvedConstantScopeCount;
            int approvedConstantRequirementCount;
            string writeResolutionIdentities;
            using (var resolver = new ConstantMetadataWriteResolver())
            using (var module = ReadModuleDeferred(source.PrivateSourcePath, resolver))
            {
                var method = ValidateModuleAndMethod(module, requireExactOffsets: true);
                var constantPlan = resolver.Configure(module);
                expectedConstantMetadataSha256 = ComputeConstantMetadataFingerprint(module);
                syntheticConstantTypeCount = constantPlan.SyntheticTypeCount;
                approvedConstantScopeCount = constantPlan.ApprovedScopeCount;
                approvedConstantRequirementCount = constantPlan.ApprovedRequirementCount;
                sourcePopCount = method.Body.Instructions.Count(instruction => instruction.OpCode.Code == Code.Pop);
                var il = method.Body.GetILProcessor();
                oneArgumentReplacements = 0;
                twoArgumentReplacements = 0;

                foreach (var site in _expected.Sites.OrderByDescending(site => site.IlOffset))
                {
                    var instruction = method.Body.Instructions.SingleOrDefault(value => value.Offset == site.IlOffset)
                        ?? throw new InvalidDataException($"Step-32 expected PrepareMethod instruction IL_{site.IlOffset:X4} is missing.");
                    if (!TryGetPrepareMethod(instruction, out var target) || target.Parameters.Count != site.ArgumentCount || target.FullName != site.TargetMember)
                        throw new InvalidDataException($"Step-32 expected PrepareMethod site IL_{site.IlOffset:X4} drifted before rewrite.");
                    if (FindIncomingBranchSources(method, instruction).Count != 0)
                        throw new InvalidDataException($"Step-32 refuses to rewrite branch-targeted PrepareMethod site IL_{site.IlOffset:X4}.");

                    if (site.ArgumentCount == 1)
                    {
                        instruction.OpCode = OpCodes.Pop;
                        instruction.Operand = null;
                        oneArgumentReplacements++;
                    }
                    else if (site.ArgumentCount == 2)
                    {
                        // Original stack immediately before call: [..., RuntimeMethodHandle, RuntimeTypeHandle[]].
                        // Inserted Pop consumes the array; rewritten call-as-Pop consumes the method handle.
                        il.InsertBefore(instruction, il.Create(OpCodes.Pop));
                        instruction.OpCode = OpCodes.Pop;
                        instruction.Operand = null;
                        twoArgumentReplacements++;
                    }
                    else
                    {
                        throw new InvalidDataException($"Unexpected PrepareMethod argument count {site.ArgumentCount} at IL_{site.IlOffset:X4}.");
                    }
                }

                if (oneArgumentReplacements != 6 || twoArgumentReplacements != 4)
                    throw new InvalidDataException($"Step-32 rewrite count mismatch: one-arg={oneArgumentReplacements}, two-arg={twoArgumentReplacements}.");
                if (CountPrepareMethodReferences(method) != 0)
                    throw new InvalidDataException("Step-32 transformed in-memory PrewarmJit still contains RuntimeHelpers.PrepareMethod references.");
                expectedTransformedInstructionCount = method.Body.Instructions.Count;
                if (expectedTransformedInstructionCount != _expected.SourceInstructionCount + twoArgumentReplacements)
                    throw new InvalidDataException($"Step-32 transformed instruction count mismatch: {expectedTransformedInstructionCount}.");
                if (method.Body.Instructions.Count(value => value.OpCode.Code == Code.Pop) != sourcePopCount + oneArgumentReplacements + (twoArgumentReplacements * 2))
                    throw new InvalidDataException("Step-32 stack-neutral Pop replacement count did not match the predeclared rewrite contract.");

                expectedTransformedSemanticSha256 = ComputeMethodSemanticFingerprint(method);
                if (resolver.Requests.Count != 0)
                    throw new InvalidDataException($"Cecil dependency resolution occurred before Step-32 serialization: {string.Join(", ", resolver.Requests)}");
                module.Write(transformedPath, new WriterParameters { WriteSymbols = false });
                resolver.ValidateWriteRequests();
                writeResolutionRequestCount = resolver.Requests.Count;
                writeResolutionIdentities = string.Join(" | ", resolver.Requests.Distinct(StringComparer.Ordinal).OrderBy(value => value, StringComparer.Ordinal));
            }

            var transformedSha256 = ComputeSha256Hex(transformedPath);
            var transformedBytes = new FileInfo(transformedPath).Length;
            if (!ComputeSha256Hex(source.PrimaryPath).Equals(source.SourceSha256, StringComparison.OrdinalIgnoreCase) ||
                !ComputeSha256Hex(source.PrivateSourcePath).Equals(source.SourceSha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Step-32 source/trusted image changed while writing the transformed copy.");
            if (transformedSha256.Equals(source.SourceSha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Step-32 transformed output is byte-identical to the source image.");
            EnsureStS2NotLoaded("Gate B exit");

            _transformation = new TransformationSnapshot(
                transformedPath, transformedSha256, transformedBytes, oneArgumentReplacements, twoArgumentReplacements,
                sourcePopCount, expectedTransformedInstructionCount, expectedTransformedSemanticSha256,
                expectedConstantMetadataSha256, writeResolutionRequestCount, syntheticConstantTypeCount);

            return Pass(gate,
                "FIRST REAL-STS2 SEMANTIC CECIL TRANSFORMATION WRITTEN TO A LAUNCHER-PRIVATE IMAGE.\n" +
                "Predeclared behavior change: suppress RuntimeHelpers.PrepareMethod eager-compilation requests inside the exact fingerprinted PrewarmJit() method only.\n" +
                "Stack contract — PrepareMethod(handle) -> Pop: consumes exactly 1 argument, returns void-equivalent empty stack.\n" +
                "Stack contract — PrepareMethod(handle, instantiation[]) -> Pop + Pop: consumes exactly 2 arguments, returns void-equivalent empty stack.\n" +
                $"One-argument sites rewritten: {oneArgumentReplacements}/6\n" +
                $"Two-argument sites rewritten: {twoArgumentReplacements}/4\n" +
                "Reflection/GetMethod/get_MethodHandle/array-construction instructions preserved: YES\n" +
                "Selected-call incoming branch targets: 0\n" +
                $"Expected transformed PrewarmJit semantic fingerprint SHA-256: {expectedTransformedSemanticSha256}\n" +
                $"Constant metadata semantic fingerprint preserved for reopen verification: {expectedConstantMetadataSha256}\n" +
                $"Synthetic constant-metadata resolver types: {syntheticConstantTypeCount:N0}\n" +
                $"Audited external constant type/storage requirements approved: {approvedConstantRequirementCount:N0}/3 across {approvedConstantScopeCount:N0}/2 exact assembly scopes\n" +
                $"Approved exact constant-metadata scopes: {CecilWriteSystemRuntimeIdentity} | {CecilWriteSentryIdentity}\n" +
                $"Cecil write-time resolution requests: {writeResolutionRequestCount:N0} — approved exact audited scope(s) only: {writeResolutionIdentities}\n" +
                "External framework/game assembly bytes opened by the write resolver: 0\n" +
                $"Source SHA-256 preserved: {source.SourceSha256}\n" +
                $"Transformed SHA-256: {transformedSha256}\n" +
                $"Transformed bytes: {transformedBytes:N0}\n" +
                $"Output: {WorkRootName}/{TransformedRootName}/{PrimaryFileName}\n" +
                "Trusted/source bytes mutated: NO\n" +
                "Assembly.Load/LoadFromStream during transformation: NO\n" +
                "Harmony/MonoMod runtime patch API invoked: NO");
        }
        catch (Exception ex)
        {
            return Fail(gate, ex);
        }
    }

    public RealStS2PrepareMethodRewriteGateResult RunTransformedImageVerification()
    {
        const RealStS2PrepareMethodRewriteGate gate = RealStS2PrepareMethodRewriteGate.TransformedImageVerification;
        try
        {
            var source = RequireSource();
            var transformation = RequireTransformation();
            EnsureStS2NotLoaded("Gate C entry");
            if (!ComputeSha256Hex(source.PrimaryPath).Equals(source.SourceSha256, StringComparison.OrdinalIgnoreCase) ||
                !ComputeSha256Hex(source.PrivateSourcePath).Equals(source.SourceSha256, StringComparison.OrdinalIgnoreCase) ||
                !ComputeSha256Hex(transformation.TransformedPath).Equals(transformation.TransformedSha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Step-32 source/transformed hashes changed before image verification.");

            int sourcePrepareCount;
            int transformedPrepareCount;
            int sourceInstructionCount;
            int transformedInstructionCount;
            int sourceHandlerCount;
            int transformedHandlerCount;
            int sourcePopCount;
            int transformedPopCount;
            string sourceBodySha256;
            string transformedBodySha256;
            string transformedSemanticSha256;
            string sourceConstantMetadataSha256;
            string transformedConstantMetadataSha256;

            using (var sourceResolver = new RejectingAssemblyResolver())
            using (var sourceModule = ReadModuleDeferred(source.PrivateSourcePath, sourceResolver))
            {
                var sourceMethod = ValidateModuleAndMethod(sourceModule, requireExactOffsets: true);
                sourcePrepareCount = CountPrepareMethodReferences(sourceMethod);
                sourceInstructionCount = sourceMethod.Body.Instructions.Count;
                sourceHandlerCount = sourceMethod.Body.ExceptionHandlers.Count;
                sourcePopCount = sourceMethod.Body.Instructions.Count(value => value.OpCode.Code == Code.Pop);
                sourceBodySha256 = RealStS2PrepareMethodSemanticAudit.ComputeMethodBodyFingerprint(sourceMethod);
                sourceConstantMetadataSha256 = ComputeConstantMetadataFingerprint(sourceModule);
                if (sourceResolver.Requests.Count != 0)
                    throw new InvalidDataException("Cecil dependency resolution occurred while re-verifying the Step-32 private source image.");
            }

            using (var transformedResolver = new RejectingAssemblyResolver())
            using (var transformedModule = ReadModuleDeferred(transformation.TransformedPath, transformedResolver))
            {
                ValidateAssemblyIdentity(transformedModule);
                var transformedMethod = FindMethodByToken(transformedModule, _expected.MethodToken);
                if (transformedMethod.FullName != _expected.SourceMethod || !transformedMethod.HasBody)
                    throw new InvalidDataException("Step-32 transformed PrewarmJit method identity/body drifted.");
                transformedPrepareCount = CountPrepareMethodReferences(transformedMethod);
                transformedInstructionCount = transformedMethod.Body.Instructions.Count;
                transformedHandlerCount = transformedMethod.Body.ExceptionHandlers.Count;
                transformedPopCount = transformedMethod.Body.Instructions.Count(value => value.OpCode.Code == Code.Pop);
                transformedBodySha256 = RealStS2PrepareMethodSemanticAudit.ComputeMethodBodyFingerprint(transformedMethod);
                transformedSemanticSha256 = ComputeMethodSemanticFingerprint(transformedMethod);
                transformedConstantMetadataSha256 = ComputeConstantMetadataFingerprint(transformedModule);
                if (transformedResolver.Requests.Count != 0)
                    throw new InvalidDataException("Cecil dependency resolution occurred while verifying the Step-32 transformed image.");
            }

            if (sourcePrepareCount != 10 || transformedPrepareCount != 0)
                throw new InvalidDataException($"PrepareMethod reference verification mismatch: source={sourcePrepareCount}, transformed={transformedPrepareCount}.");
            if (!sourceBodySha256.Equals(_expected.MethodBodySha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Step-32 source PrewarmJit body fingerprint changed.");
            // Instruction offsets are finalized by Cecil during serialization. The physical IL-body fingerprint
            // is therefore post-write evidence, not something that can be predicted from the pre-write in-memory
            // instruction offsets. The offset-independent semantic fingerprint is the exact pre-write -> reopen
            // invariant: it binds opcode/operand order, branch targets by instruction ordinal, and EH boundaries.
            if (!transformedSemanticSha256.Equals(transformation.ExpectedTransformedSemanticSha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Step-32 reopened transformed PrewarmJit does not match the exact in-memory predeclared semantic rewrite.");
            if (!sourceConstantMetadataSha256.Equals(transformation.ExpectedConstantMetadataSha256, StringComparison.OrdinalIgnoreCase) ||
                !transformedConstantMetadataSha256.Equals(sourceConstantMetadataSha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Step-32 source/transformed constant metadata semantics changed during Cecil serialization.");
            if (transformedBodySha256.Equals(sourceBodySha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Step-32 reopened transformed PrewarmJit body fingerprint unexpectedly matches the source body.");
            if (sourceInstructionCount != _expected.SourceInstructionCount || transformedInstructionCount != transformation.ExpectedTransformedInstructionCount)
                throw new InvalidDataException("Step-32 source/transformed instruction count drifted.");
            if (sourceHandlerCount != _expected.SourceExceptionHandlerCount || transformedHandlerCount != sourceHandlerCount)
                throw new InvalidDataException("Step-32 exception-handler topology count changed.");
            var expectedPopDelta = transformation.OneArgumentReplacements + (transformation.TwoArgumentReplacements * 2);
            if (transformedPopCount != sourcePopCount + expectedPopDelta)
                throw new InvalidDataException("Step-32 reopened transformed image did not preserve the exact Pop replacement count.");
            EnsureStS2NotLoaded("Gate C exit");

            _verification = new VerificationSnapshot(transformedBodySha256, transformedSemanticSha256, sourcePrepareCount, transformedPrepareCount);
            return Pass(gate,
                "TRANSFORMED REAL-STS2 IMAGE REOPENED AND EXACT REWRITE VERIFIED BEFORE CLR ADMISSION.\n" +
                $"Source PrewarmJit body fingerprint unchanged: {sourceBodySha256}\n" +
                $"Transformed PrewarmJit body fingerprint: {transformedBodySha256}\n" +
                $"Transformed semantic fingerprint: {transformedSemanticSha256}\n" +
                $"Constant metadata semantic fingerprint source/transformed: {sourceConstantMetadataSha256} / {transformedConstantMetadataSha256}\n" +
                $"PrepareMethod references source/transformed: {sourcePrepareCount} / {transformedPrepareCount}\n" +
                $"Instruction count source/transformed: {sourceInstructionCount} / {transformedInstructionCount}\n" +
                $"Exception-handler count source/transformed: {sourceHandlerCount} / {transformedHandlerCount}\n" +
                $"Pop delta: +{expectedPopDelta} (6 one-arg + 4 two-arg stack-neutral replacements)\n" +
                "Assembly identity/MVID preserved: YES\n" +
                "Reopened transformed semantics match exact pre-write plan: YES\n" +
                "Original receipt-backed/private-source image CLR-loaded: NO\n" +
                "Harmony/MonoMod runtime replacement/detour used: NO");
        }
        catch (Exception ex)
        {
            return Fail(gate, ex);
        }
    }

    public async Task<RealStS2PrepareMethodRewriteGateResult> RunFinalIsolationAuditAsync(
        IProgress<RealStS2PrepareMethodRewriteProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        const RealStS2PrepareMethodRewriteGate gate = RealStS2PrepareMethodRewriteGate.FinalIsolationAudit;
        try
        {
            var source = RequireSource();
            var transformation = RequireTransformation();
            var verification = RequireVerification();
            cancellationToken.ThrowIfCancellationRequested();
            EnsureStS2NotLoaded("Gate D entry");
            progress?.Report(new(gate, 0, 1, null, "Re-hashing trusted/source/transformed images and re-proving OfflineReady after the first real-StS2 private rewrite…"));

            if (!ComputeSha256Hex(source.PrimaryPath).Equals(source.SourceSha256, StringComparison.OrdinalIgnoreCase) ||
                !ComputeSha256Hex(source.PrivateSourcePath).Equals(source.SourceSha256, StringComparison.OrdinalIgnoreCase) ||
                !ComputeSha256Hex(transformation.TransformedPath).Equals(transformation.TransformedSha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Step-32 final image hashes do not match the verified snapshots.");
            if (!verification.TransformedSemanticSha256.Equals(transformation.ExpectedTransformedSemanticSha256, StringComparison.OrdinalIgnoreCase) ||
                verification.TransformedBodySha256.Equals(_expected.MethodBodySha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Step-32 transformed-method verification snapshot drifted.");

            IProgress<SteamOfflineInstallProgress>? offlineProgress = progress is null
                ? null
                : new CallbackProgress<SteamOfflineInstallProgress>(value =>
                    progress.Report(new RealStS2PrepareMethodRewriteProgress(
                        gate, value.CompletedFiles, value.TotalFiles, value.CurrentFile,
                        $"Post-rewrite OfflineReady — {value.Message} ({value.CompletedBytes:N0}/{value.TotalBytes:N0} bytes)")));
            var offline = await _offlineInspection.RunAsync(offlineProgress, cancellationToken).ConfigureAwait(false);
            if (offline.Outcome == SteamOfflineInstallOutcome.Cancelled)
                throw new OperationCanceledException("Step 32 final OfflineReady audit was cancelled.", cancellationToken);
            if (!offline.Success || offline.VerifiedFiles != source.Offline.VerifiedFiles || offline.PlannedFiles != source.Offline.PlannedFiles)
                throw new InvalidDataException(offline.Error ?? "Step-32 post-rewrite OfflineReady re-verification failed or changed file accounting.");
            EnsureStS2NotLoaded("Gate D exit");

            return Pass(gate,
                "STEP 32.0 FINAL REAL-STS2 PRIVATE-REWRITE ISOLATION AUDIT PASSED.\n" +
                $"Primary receipt SHA-1 unchanged: {source.SourceSha1}\n" +
                $"Primary SHA-256 unchanged: {source.SourceSha256}\n" +
                $"Primary bytes unchanged: {source.SourceBytes:N0}\n" +
                $"Private source SHA-256 unchanged: {source.SourceSha256}\n" +
                $"Transformed image SHA-256 unchanged: {transformation.TransformedSha256}\n" +
                $"Post-rewrite OfflineReady: PASS ({offline.VerifiedFiles:N0}/{offline.PlannedFiles:N0} files)\n" +
                "Trusted Step 12 managed install unchanged: YES\n" +
                "Exactly one semantic family changed: PrewarmJit RuntimeHelpers.PrepareMethod calls only\n" +
                "Real StS2 assembly/type/member CLR load or invocation by Step 32: NO\n" +
                $"Cecil read/verification dependency resolution requests: 0; write-time synthetic constant-metadata resolutions: {transformation.WriteResolutionRequestCount:N0} ({CecilWriteSystemRuntimeIdentity} only)\n" +
                "External framework/game assembly bytes opened by Cecil write resolver: 0\n" +
                "Harmony/MonoMod runtime patching invoked by Step 32: NO\n" +
                "Godot/game startup or native game loading attempted by Step 32: NO\n" +
                "Authorization after Step-32 PASS: transformed-image mechanism may advance to a separately gated real-StS2 CLR admission/execution boundary; this build itself does not load or execute the transformed game image.");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return Fail(gate, ex);
        }
    }

    internal string? TransformedPathForTests => _transformation?.TransformedPath;

    private SourceSnapshot RequireSource() => _source ?? throw new InvalidOperationException("Step 32 Gate A must pass before later gates run.");
    private TransformationSnapshot RequireTransformation() => _transformation ?? throw new InvalidOperationException("Step 32 Gate B must pass before later gates run.");
    private VerificationSnapshot RequireVerification() => _verification ?? throw new InvalidOperationException("Step 32 Gate C must pass before the final isolation audit.");

    private MethodDefinition ValidateModuleAndMethod(ModuleDefinition module, bool requireExactOffsets)
    {
        ValidateAssemblyIdentity(module);
        var method = FindMethodByToken(module, _expected.MethodToken);
        if (method.DeclaringType.FullName != _expected.SourceType || method.FullName != _expected.SourceMethod || !method.HasBody)
            throw new InvalidDataException("Step-32 PrewarmJit method identity/body does not match the physical Step-31 evidence.");
        var bodySha256 = RealStS2PrepareMethodSemanticAudit.ComputeMethodBodyFingerprint(method);
        if (!bodySha256.Equals(_expected.MethodBodySha256, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException($"Step-32 PrewarmJit body fingerprint drift: {bodySha256}.");
        if (method.Body.Instructions.Count != _expected.SourceInstructionCount || method.Body.ExceptionHandlers.Count != _expected.SourceExceptionHandlerCount)
            throw new InvalidDataException("Step-32 PrewarmJit instruction/exception-handler shape drifted from physical Step-31 evidence.");
        var sites = method.Body.Instructions.Where(instruction => TryGetPrepareMethod(instruction, out _)).ToArray();
        if (sites.Length != _expected.Sites.Count)
            throw new InvalidDataException($"Expected {_expected.Sites.Count} PrepareMethod calls, found {sites.Length}.");
        if (requireExactOffsets)
        {
            foreach (var expectedSite in _expected.Sites)
            {
                var instruction = sites.SingleOrDefault(site => site.Offset == expectedSite.IlOffset)
                    ?? throw new InvalidDataException($"Expected PrepareMethod site IL_{expectedSite.IlOffset:X4} is missing.");
                if (!TryGetPrepareMethod(instruction, out var target) || instruction.OpCode.Code != Code.Call ||
                    target.Parameters.Count != expectedSite.ArgumentCount || target.FullName != expectedSite.TargetMember)
                    throw new InvalidDataException($"PrepareMethod site IL_{expectedSite.IlOffset:X4} no longer matches the physical Step-31 signature.");
                if (FindIncomingBranchSources(method, instruction).Count != 0)
                    throw new InvalidDataException($"PrepareMethod site IL_{expectedSite.IlOffset:X4} became a branch target; stack-neutral rewrite is refused.");
            }
        }
        return method;
    }

    private void ValidateAssemblyIdentity(ModuleDefinition module)
    {
        if (module.Assembly?.Name is null || !module.Assembly.Name.FullName.Equals(_expected.AssemblyIdentity, StringComparison.Ordinal) || module.Mvid != _expected.Mvid)
            throw new InvalidDataException("Step-32 assembly identity/MVID does not match physical Step-31 evidence.");
    }

    private void ValidatePhysicalSourceIdentity(long bytes, string sha1, string sha256)
    {
        if (bytes != _expected.SourceBytes ||
            !sha1.Equals(_expected.SourceSha1, StringComparison.OrdinalIgnoreCase) ||
            !sha256.Equals(_expected.SourceSha256, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Primary sts2.dll does not match the physically closed Step-31 source identity.");
    }

    private static bool TryGetPrepareMethod(Instruction instruction, out MethodReference target)
    {
        if (instruction.Operand is MethodReference reference &&
            reference.DeclaringType.FullName == "System.Runtime.CompilerServices.RuntimeHelpers" &&
            reference.Name == "PrepareMethod")
        {
            target = reference;
            return true;
        }
        target = null!;
        return false;
    }

    private static int CountPrepareMethodReferences(MethodDefinition method)
        => method.Body.Instructions.Count(instruction => TryGetPrepareMethod(instruction, out _));

    private static IReadOnlyList<Instruction> FindIncomingBranchSources(MethodDefinition method, Instruction target)
    {
        var result = new List<Instruction>();
        foreach (var instruction in method.Body.Instructions)
        {
            if (instruction.Operand is Instruction single && ReferenceEquals(single, target)) result.Add(instruction);
            else if (instruction.Operand is Instruction[] many && many.Any(value => ReferenceEquals(value, target))) result.Add(instruction);
        }
        return result;
    }

    internal static string ComputeMethodSemanticFingerprint(MethodDefinition method)
    {
        var instructions = method.Body.Instructions;
        var index = instructions.Select((instruction, ordinal) => (instruction, ordinal)).ToDictionary(pair => pair.instruction, pair => pair.ordinal);
        var canonical = new StringBuilder();
        canonical.Append(method.FullName).Append('\n');
        for (var i = 0; i < instructions.Count; i++)
        {
            var instruction = instructions[i];
            canonical.Append(i.ToString(CultureInfo.InvariantCulture)).Append('|')
                .Append(instruction.OpCode.Code).Append('|')
                .Append(FormatSemanticOperand(instruction.Operand, index)).Append('\n');
        }
        foreach (var handler in method.Body.ExceptionHandlers)
        {
            canonical.Append("EH|").Append(handler.HandlerType).Append('|')
                .Append(IndexOf(handler.TryStart, index)).Append('|').Append(IndexOf(handler.TryEnd, index)).Append('|')
                .Append(IndexOf(handler.HandlerStart, index)).Append('|').Append(IndexOf(handler.HandlerEnd, index)).Append('|')
                .Append(IndexOf(handler.FilterStart, index)).Append('|').Append(handler.CatchType?.FullName ?? string.Empty).Append('\n');
        }
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical.ToString()))).ToLowerInvariant();
    }

    private static string FormatSemanticOperand(object? operand, IReadOnlyDictionary<Instruction, int> index)
        => operand switch
        {
            null => string.Empty,
            MethodReference method => $"method:{GetScopeName(method.DeclaringType.Scope)}:{method.FullName}",
            FieldReference field => $"field:{GetScopeName(field.DeclaringType.Scope)}:{field.FullName}",
            TypeReference type => $"type:{GetScopeName(type.Scope)}:{type.FullName}",
            Instruction instruction => $"I{index[instruction]}",
            Instruction[] many => string.Join(",", many.Select(value => $"I{index[value]}")),
            VariableDefinition variable => $"V_{variable.Index}",
            ParameterDefinition parameter => $"P_{parameter.Index}:{parameter.ParameterType.FullName}",
            string text => $"string:{text}",
            _ => Convert.ToString(operand, CultureInfo.InvariantCulture) ?? string.Empty,
        };

    private static string GetScopeName(IMetadataScope? scope)
        => scope switch
        {
            AssemblyNameReference assembly => assembly.Name ?? string.Empty,
            ModuleDefinition module => module.Assembly?.Name?.Name ?? module.Name,
            ModuleReference moduleReference => moduleReference.Name ?? string.Empty,
            _ => scope?.Name ?? string.Empty,
        };

    private static int IndexOf(Instruction? instruction, IReadOnlyDictionary<Instruction, int> index)
        => instruction is null ? -1 : index[instruction];

    private static MethodDefinition FindMethodByToken(ModuleDefinition module, uint token)
        => EnumerateTypes(module.Types).SelectMany(type => type.Methods)
            .SingleOrDefault(method => method.MetadataToken.ToUInt32() == token)
            ?? throw new MissingMethodException($"No method with metadata token 0x{token:X8} exists in sts2.dll.");

    private static IEnumerable<TypeDefinition> EnumerateTypes(IEnumerable<TypeDefinition> roots)
    {
        foreach (var type in roots)
        {
            yield return type;
            foreach (var nested in EnumerateTypes(type.NestedTypes)) yield return nested;
        }
    }

    private static Dictionary<ExternalConstantTypeKey, TypeCode> CollectExternalConstantTypeRequirements(ModuleDefinition module)
    {
        var requirements = new Dictionary<ExternalConstantTypeKey, TypeCode>();

        void Add(TypeReference declaredType, object? constant, string provider)
        {
            if (constant is null)
                return;
            var leaf = GetConstantResolutionLeaf(declaredType);
            if (leaf is null)
                return;
            if (leaf.Scope is ModuleDefinition)
                return;
            if (leaf.Scope is not AssemblyNameReference assemblyReference)
                throw new InvalidDataException($"Step-32 constant provider '{provider}' has unsupported metadata scope '{leaf.Scope?.MetadataScopeType}'.");

            var typeCode = Type.GetTypeCode(constant.GetType());
            if (!IsSupportedConstantTypeCode(typeCode))
                throw new InvalidDataException($"Step-32 constant provider '{provider}' has unsupported constant storage type {constant.GetType().FullName}.");
            var key = new ExternalConstantTypeKey(assemblyReference.FullName, leaf.FullName, leaf.IsNested);
            if (requirements.TryGetValue(key, out var prior) && prior != typeCode)
                throw new InvalidDataException($"Step-32 external constant type '{leaf.FullName}' has inconsistent storage types {prior} and {typeCode}.");
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

    private static TypeReference? GetConstantResolutionLeaf(TypeReference type)
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

    private static bool IsSupportedConstantTypeCode(TypeCode typeCode)
        => typeCode is TypeCode.Boolean or TypeCode.Char or TypeCode.SByte or TypeCode.Byte or TypeCode.Int16 or TypeCode.UInt16 or
            TypeCode.Int32 or TypeCode.UInt32 or TypeCode.Int64 or TypeCode.UInt64 or TypeCode.Single or TypeCode.Double;

    private static TypeReference GetPrimitiveConstantType(ModuleDefinition sourceModule, TypeCode typeCode)
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
            _ => throw new InvalidDataException($"Unsupported Step-32 constant storage type {typeCode}."),
        };

    internal static string ComputeConstantMetadataFingerprint(ModuleDefinition module)
    {
        var lines = new List<string>();
        static string Scope(TypeReference type) => type.Scope switch
        {
            AssemblyNameReference assembly => assembly.FullName,
            ModuleDefinition moduleDefinition => moduleDefinition.Assembly?.Name.FullName ?? moduleDefinition.Name,
            ModuleReference moduleReference => moduleReference.Name,
            _ => type.Scope?.ToString() ?? "<none>",
        };
        static string ConstantValue(object? value)
        {
            if (value is null) return "<null>";
            return value switch
            {
                float single => single.ToString("R", CultureInfo.InvariantCulture),
                double dbl => dbl.ToString("R", CultureInfo.InvariantCulture),
                IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture) ?? string.Empty,
                _ => value.ToString() ?? string.Empty,
            };
        }
        static void AddLine(List<string> target, string provider, TypeReference declaredType, object? value)
            => target.Add($"{provider}|{declaredType.FullName}|{Scope(declaredType)}|{value?.GetType().FullName ?? "<null>"}|{ConstantValue(value)}");

        foreach (var type in EnumerateTypes(module.Types))
        {
            foreach (var field in type.Fields)
                if (field.HasConstant)
                    AddLine(lines, $"F|{type.FullName}|{field.Name}", field.FieldType, field.Constant);
            foreach (var property in type.Properties)
                if (property.HasConstant)
                    AddLine(lines, $"P|{type.FullName}|{property.Name}", property.PropertyType, property.Constant);
            foreach (var method in type.Methods)
            {
                if (method.MethodReturnType.HasConstant)
                    AddLine(lines, $"R|{method.FullName}", method.MethodReturnType.ReturnType, method.MethodReturnType.Constant);
                for (var i = 0; i < method.Parameters.Count; i++)
                {
                    var parameter = method.Parameters[i];
                    if (parameter.HasConstant)
                        AddLine(lines, $"A|{method.FullName}|{i}|{parameter.Name}", parameter.ParameterType, parameter.Constant);
                }
            }
        }

        lines.Sort(StringComparer.Ordinal);
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join("\n", lines)))).ToLowerInvariant();
    }

    private static ModuleDefinition ReadModuleDeferred(string path, IAssemblyResolver resolver)
        => ModuleDefinition.ReadModule(path, new ReaderParameters
        {
            ReadSymbols = false,
            ReadingMode = ReadingMode.Deferred,
            AssemblyResolver = resolver,
        });

    private static bool IsPrimaryArm64StS2Path(string path)
        => ("/" + path.Replace('\\', '/').TrimStart('/')).EndsWith("/data_sts2_macos_arm64/sts2.dll", StringComparison.OrdinalIgnoreCase);

    private static void ValidateReceiptSnapshot(SteamManagedInstallReceipt receipt, SteamOfflineInstallResult offline)
    {
        if (!offline.ReceiptStructurallyValid || !offline.ExactManagedTreeVerified)
            throw new InvalidDataException("OfflineReady did not include structurally valid receipt + exact-tree proof.");
        if (receipt.SchemaVersion != SteamManagedInstallReceipt.CurrentSchemaVersion || receipt.AppId != SteamOfflineInstallInspection.TargetAppId ||
            offline.DepotId is null || receipt.DepotId != offline.DepotId.Value || offline.InstalledManifestId is null || receipt.ManifestId != offline.InstalledManifestId.Value ||
            !string.Equals(receipt.Branch, offline.Branch, StringComparison.Ordinal) || receipt.Files is null || receipt.Files.Count == 0 || receipt.Files.Count != offline.PlannedFiles)
            throw new InvalidDataException("The Step-12 receipt changed or became inconsistent after OfflineReady was proven.");
        var unique = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var file in receipt.Files)
        {
            if (file is null || !SteamSingleFileTargetSelector.IsSafeRelativePath(file.RelativePath) || file.Length < 0 || file.Sha1Hex.Length != 40 ||
                !file.Sha1Hex.All(Uri.IsHexDigit) || !unique.Add(file.RelativePath.Replace('\\', '/')))
                throw new InvalidDataException("The Step-12 receipt contains an invalid or duplicate file entry.");
        }
    }

    private static async Task<SteamManagedInstallReceipt> ReadReceiptAsync(string managedRoot, CancellationToken cancellationToken)
    {
        var receiptPath = Path.Combine(managedRoot, SteamManagedInstallReceipt.FileName);
        await using var stream = new FileStream(receiptPath, FileMode.Open, FileAccess.Read, FileShare.Read, 16 * 1024, FileOptions.Asynchronous | FileOptions.SequentialScan);
        var receipt = await JsonSerializer.DeserializeAsync(stream, SteamManagedInstallJsonContext.Default.SteamManagedInstallReceipt, cancellationToken).ConfigureAwait(false);
        return receipt ?? throw new InvalidDataException("The verified Step-12 receipt unexpectedly deserialized to null.");
    }

    private void PrepareFreshWorkRoot()
    {
        BestEffortDeleteWorkRoot();
        Directory.CreateDirectory(_workRoot);
    }

    private void BestEffortDeleteWorkRoot()
    {
        try { if (Directory.Exists(_workRoot)) Directory.Delete(_workRoot, recursive: true); }
        catch { }
    }

    private static async Task CopyFileAsync(string source, string destination, CancellationToken cancellationToken)
    {
        await using var input = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.Read, 256 * 1024, FileOptions.Asynchronous | FileOptions.SequentialScan);
        await using var output = new FileStream(destination, FileMode.CreateNew, FileAccess.Write, FileShare.None, 256 * 1024, FileOptions.Asynchronous | FileOptions.SequentialScan);
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

    private static async Task<string> ComputeHashHexAsync(string path, HashAlgorithm algorithm, CancellationToken cancellationToken)
    {
        using (algorithm)
        {
            await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 256 * 1024, FileOptions.Asynchronous | FileOptions.SequentialScan);
            return Convert.ToHexString(await algorithm.ComputeHashAsync(stream, cancellationToken).ConfigureAwait(false)).ToLowerInvariant();
        }
    }

    private static string ComputeSha256Hex(string path)
        => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();

    private static IReadOnlyList<string> FindLoadedAssemblyIdentities(string simpleName)
        => AppDomain.CurrentDomain.GetAssemblies().Select(assembly => assembly.GetName())
            .Where(name => string.Equals(name.Name, simpleName, StringComparison.OrdinalIgnoreCase))
            .Select(name => name.FullName ?? name.Name ?? "<unknown>").OrderBy(value => value, StringComparer.Ordinal).ToArray();

    private static void EnsureStS2NotLoaded(string stage)
    {
        var loaded = FindLoadedAssemblyIdentities("sts2");
        if (loaded.Count != 0)
            throw new InvalidDataException($"{stage}: a sts2 assembly identity is already resident in the CLR. Force-quit before Step 32. Loaded: {string.Join(" | ", loaded)}");
    }

    private static RealStS2PrepareMethodRewriteGateResult Pass(RealStS2PrepareMethodRewriteGate gate, string detail) => new(gate, true, detail);
    private static RealStS2PrepareMethodRewriteGateResult Fail(RealStS2PrepareMethodRewriteGate gate, Exception ex) => new(gate, false, $"Stage failed with {ex.GetType().Name}: {ex.Message}\n{ex}");

    private sealed class CallbackProgress<T> : IProgress<T>
    {
        private readonly Action<T> _callback;
        public CallbackProgress(Action<T> callback) => _callback = callback ?? throw new ArgumentNullException(nameof(callback));
        public void Report(T value) => _callback(value);
    }

    private sealed class ConstantMetadataWriteResolver : IAssemblyResolver
    {
        private readonly List<string> _requests = [];
        private readonly Dictionary<string, AssemblyDefinition> _surrogates = new(StringComparer.Ordinal);
        private bool _configured;

        public IReadOnlyList<string> Requests => _requests;

        public ConstantMetadataResolutionPlan Configure(ModuleDefinition sourceModule)
        {
            if (_configured)
                throw new InvalidOperationException("The Step-32 constant-metadata write resolver was already configured.");
            _configured = true;

            var requirements = CollectExternalConstantTypeRequirements(sourceModule);
            ValidateAuditedRequirementSet(requirements);

            var assemblyReferences = new Dictionary<string, AssemblyNameReference>(StringComparer.Ordinal);
            foreach (var identity in requirements.Keys.Select(key => key.AssemblyFullName).Distinct(StringComparer.Ordinal))
            {
                var matches = sourceModule.AssemblyReferences
                    .Where(reference => reference.FullName.Equals(identity, StringComparison.Ordinal))
                    .ToArray();
                if (matches.Length != 1)
                    throw new InvalidDataException($"Step-32 source must contain exactly one AssemblyRef for audited constant-metadata scope {identity}; found {matches.Length}.");
                assemblyReferences.Add(identity, matches[0]);
            }

            var syntheticTypeCount = 0;
            foreach (var scopeGroup in requirements
                         .GroupBy(pair => pair.Key.AssemblyFullName, StringComparer.Ordinal)
                         .OrderBy(group => group.Key, StringComparer.Ordinal))
            {
                var sourceReference = assemblyReferences[scopeGroup.Key];
                var surrogateName = new AssemblyNameDefinition(sourceReference.Name, sourceReference.Version)
                {
                    Culture = sourceReference.Culture,
                    PublicKeyToken = sourceReference.PublicKeyToken is null ? [] : sourceReference.PublicKeyToken.ToArray(),
                };
                var safeName = sourceReference.Name.Replace('.', '-');
                var surrogate = AssemblyDefinition.CreateAssembly(
                    surrogateName,
                    $"Step32.{safeName}.ConstantMetadataSurrogate.dll",
                    ModuleKind.Dll);
                _surrogates.Add(scopeGroup.Key, surrogate);

                foreach (var requirement in scopeGroup.OrderBy(pair => pair.Key.TypeFullName, StringComparer.Ordinal))
                {
                    if (requirement.Key.IsNested)
                        throw new InvalidDataException($"Step-32 does not permit nested external constant type synthesis: {requirement.Key.TypeFullName}.");
                    var separator = requirement.Key.TypeFullName.LastIndexOf('.');
                    var typeNamespace = separator < 0 ? string.Empty : requirement.Key.TypeFullName[..separator];
                    var typeName = separator < 0 ? requirement.Key.TypeFullName : requirement.Key.TypeFullName[(separator + 1)..];

                    // The surrogate is never written or loaded. Cecil only needs a TypeDefinition whose
                    // BaseType identifies System.Enum and whose value__ field exposes the audited primitive
                    // storage type. Keeping the synthetic System.Enum scope inside the same surrogate avoids
                    // introducing any secondary resolver path while satisfying that bounded metadata query.
                    var enumBase = new TypeReference("System", "Enum", surrogate.MainModule, surrogateName);
                    var syntheticEnum = new TypeDefinition(
                        typeNamespace,
                        typeName,
                        TypeAttributes.Public | TypeAttributes.Sealed,
                        enumBase);
                    syntheticEnum.Fields.Add(new FieldDefinition(
                        "value__",
                        FieldAttributes.Public | FieldAttributes.SpecialName | FieldAttributes.RTSpecialName,
                        GetPrimitiveConstantType(sourceModule, requirement.Value)));
                    surrogate.MainModule.Types.Add(syntheticEnum);
                    syntheticTypeCount++;
                }
            }

            return new ConstantMetadataResolutionPlan(
                syntheticTypeCount,
                _surrogates.Count,
                requirements.Count);
        }

        private static void ValidateAuditedRequirementSet(IReadOnlyDictionary<ExternalConstantTypeKey, TypeCode> actual)
        {
            var missing = AuditedExternalConstantTypeRequirements
                .Where(pair => !actual.TryGetValue(pair.Key, out var observed) || observed != pair.Value)
                .Select(pair => FormatRequirement(pair.Key, pair.Value))
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            var unexpected = actual
                .Where(pair => !AuditedExternalConstantTypeRequirements.TryGetValue(pair.Key, out var expected) || expected != pair.Value)
                .Select(pair => FormatRequirement(pair.Key, pair.Value))
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();

            if (missing.Length == 0 && unexpected.Length == 0)
                return;

            var detail = new List<string>();
            if (missing.Length != 0)
                detail.Add("missing/changed audited requirement(s): " + string.Join(" | ", missing));
            if (unexpected.Length != 0)
                detail.Add("unexpected requirement(s): " + string.Join(" | ", unexpected));
            throw new InvalidDataException(
                "Step-32 external constant-metadata requirement set drifted from the static audit of the exact receipt-backed sts2.dll; " +
                string.Join("; ", detail));
        }

        private static string FormatRequirement(ExternalConstantTypeKey key, TypeCode typeCode)
            => $"{key.AssemblyFullName} / {key.TypeFullName} / {typeCode} / nested={key.IsNested}";

        public AssemblyDefinition Resolve(AssemblyNameReference name)
            => ResolveCore(name);

        public AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
            => ResolveCore(name);

        private AssemblyDefinition ResolveCore(AssemblyNameReference name)
        {
            _requests.Add(name.FullName);
            if (!_configured || !_surrogates.TryGetValue(name.FullName, out var surrogate))
                throw new AssemblyResolutionException(name);
            return surrogate;
        }

        public void ValidateWriteRequests()
        {
            if (_requests.Count == 0)
                throw new InvalidDataException("Step-32 expected Cecil serialization to use at least one bounded constant-metadata surrogate, but no write-time resolution request occurred.");
            var unexpected = _requests
                .Where(value => !_surrogates.ContainsKey(value))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            if (unexpected.Length != 0)
                throw new InvalidDataException("Step-32 Cecil serialization attempted an unapproved assembly resolution: " + string.Join(" | ", unexpected));
        }

        public void Dispose()
        {
            foreach (var surrogate in _surrogates.Values)
                surrogate.Dispose();
            _surrogates.Clear();
        }
    }

    private sealed class RejectingAssemblyResolver : IAssemblyResolver
    {
        private readonly List<string> _requests = [];
        public IReadOnlyList<string> Requests => _requests;
        public AssemblyDefinition Resolve(AssemblyNameReference name) { _requests.Add(name.FullName); throw new AssemblyResolutionException(name); }
        public AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters) { _requests.Add(name.FullName); throw new AssemblyResolutionException(name); }
        public void Dispose() { }
    }

    private sealed record ConstantMetadataResolutionPlan(
        int SyntheticTypeCount,
        int ApprovedScopeCount,
        int ApprovedRequirementCount);
    private readonly record struct ExternalConstantTypeKey(string AssemblyFullName, string TypeFullName, bool IsNested);

    internal sealed record RewriteCallSiteEvidence(int IlOffset, int ArgumentCount, string TargetMember);
    internal sealed record RewriteEvidence(
        string SourceSha1,
        string SourceSha256,
        long SourceBytes,
        string AssemblyIdentity,
        Guid Mvid,
        string SourceType,
        string SourceMethod,
        uint MethodToken,
        string MethodBodySha256,
        int SourceInstructionCount,
        int SourceExceptionHandlerCount,
        IReadOnlyList<RewriteCallSiteEvidence> Sites);

    private sealed record SourceSnapshot(
        SteamOfflineInstallResult Offline,
        string ManagedRoot,
        string PrimaryRelativePath,
        string PrimaryPath,
        string PrivateSourcePath,
        long SourceBytes,
        string SourceSha1,
        string SourceSha256);

    private sealed record TransformationSnapshot(
        string TransformedPath,
        string TransformedSha256,
        long TransformedBytes,
        int OneArgumentReplacements,
        int TwoArgumentReplacements,
        int SourcePopCount,
        int ExpectedTransformedInstructionCount,
        string ExpectedTransformedSemanticSha256,
        string ExpectedConstantMetadataSha256,
        int WriteResolutionRequestCount,
        int SyntheticConstantTypeCount);

    private sealed record VerificationSnapshot(
        string TransformedBodySha256,
        string TransformedSemanticSha256,
        int SourcePrepareMethodCount,
        int TransformedPrepareMethodCount);
}
