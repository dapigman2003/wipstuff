using System.Buffers.Binary;
using System.Globalization;
using System.Reflection.PortableExecutable;
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

            var sourceImage = File.ReadAllBytes(source.PrivateSourcePath);
            var transformedImage = sourceImage.ToArray();
            int oneArgumentReplacements;
            int twoArgumentReplacements;
            int sourcePopCount;
            int sourceNopCount;
            string expectedTransformedSemanticSha256;
            string expectedConstantMetadataSha256;
            int expectedTransformedInstructionCount;
            MethodBodyFileLocation methodLocation;
            List<RawPatchWindow> patchWindows = [];

            using (var resolver = new RejectingAssemblyResolver())
            using (var module = ReadModuleDeferred(source.PrivateSourcePath, resolver))
            {
                var method = ValidateModuleAndMethod(module, requireExactOffsets: true);
                expectedConstantMetadataSha256 = ComputeConstantMetadataFingerprint(module);
                sourcePopCount = method.Body.Instructions.Count(instruction => instruction.OpCode.Code == Code.Pop);
                sourceNopCount = method.Body.Instructions.Count(instruction => instruction.OpCode.Code == Code.Nop);
                methodLocation = LocateMethodBodyCode(sourceImage, method.RVA);
                if (methodLocation.CodeSize != method.Body.CodeSize)
                    throw new InvalidDataException($"Step-32 PE/Cecil method code-size mismatch: PE={methodLocation.CodeSize}, Cecil={method.Body.CodeSize}.");

                oneArgumentReplacements = 0;
                twoArgumentReplacements = 0;
                foreach (var site in _expected.Sites.OrderBy(site => site.IlOffset))
                {
                    var instruction = method.Body.Instructions.SingleOrDefault(value => value.Offset == site.IlOffset)
                        ?? throw new InvalidDataException($"Step-32 expected PrepareMethod instruction IL_{site.IlOffset:X4} is missing.");
                    if (!TryGetPrepareMethod(instruction, out var target) || instruction.OpCode.Code != Code.Call ||
                        target.Parameters.Count != site.ArgumentCount || target.FullName != site.TargetMember)
                        throw new InvalidDataException($"Step-32 expected direct PrepareMethod site IL_{site.IlOffset:X4} drifted before rewrite.");
                    if (FindIncomingBranchSources(method, instruction).Count != 0)
                        throw new InvalidDataException($"Step-32 refuses to rewrite branch-targeted PrepareMethod site IL_{site.IlOffset:X4}.");

                    var fileOffset = checked(methodLocation.CodeFileOffset + site.IlOffset);
                    if (site.IlOffset < 0 || site.IlOffset + 5 > methodLocation.CodeSize || fileOffset < 0 || fileOffset + 5 > sourceImage.LongLength)
                        throw new InvalidDataException($"Step-32 patch window IL_{site.IlOffset:X4} falls outside the exact PrewarmJit method body.");
                    var fileIndex = checked((int)fileOffset);
                    if (sourceImage[fileIndex] != 0x28)
                        throw new InvalidDataException($"Step-32 raw IL site IL_{site.IlOffset:X4} is not the expected 5-byte call opcode (0x28).");
                    var rawToken = BinaryPrimitives.ReadUInt32LittleEndian(sourceImage.AsSpan(fileIndex + 1, 4));
                    var expectedToken = target.MetadataToken.ToUInt32();
                    if (expectedToken == 0 || rawToken != expectedToken)
                        throw new InvalidDataException($"Step-32 raw IL token mismatch at IL_{site.IlOffset:X4}: PE=0x{rawToken:X8}, Cecil=0x{expectedToken:X8}.");

                    var replacement = site.ArgumentCount switch
                    {
                        1 => new byte[] { 0x26, 0x00, 0x00, 0x00, 0x00 },
                        2 => new byte[] { 0x26, 0x26, 0x00, 0x00, 0x00 },
                        _ => throw new InvalidDataException($"Unexpected PrepareMethod argument count {site.ArgumentCount} at IL_{site.IlOffset:X4}."),
                    };
                    patchWindows.Add(new RawPatchWindow(site.IlOffset, site.ArgumentCount, expectedToken, fileOffset, sourceImage.AsSpan(fileIndex, 5).ToArray(), replacement));
                    if (site.ArgumentCount == 1) oneArgumentReplacements++; else twoArgumentReplacements++;
                }

                if (oneArgumentReplacements != 6 || twoArgumentReplacements != 4 || patchWindows.Count != 10)
                    throw new InvalidDataException($"Step-32 rewrite count mismatch: one-arg={oneArgumentReplacements}, two-arg={twoArgumentReplacements}, windows={patchWindows.Count}.");

                // Build the exact expected post-patch semantic shape in memory only. This never serializes the module.
                // Each original 5-byte call window remains exactly 5 bytes on disk, so padding Nops intentionally
                // preserve all later IL offsets, branch displacements, EH boundaries, metadata tables, and PE layout.
                var il = method.Body.GetILProcessor();
                foreach (var site in _expected.Sites.OrderByDescending(site => site.IlOffset))
                {
                    var instruction = method.Body.Instructions.Single(value => value.Offset == site.IlOffset);
                    instruction.OpCode = OpCodes.Pop;
                    instruction.Operand = null;
                    var cursor = instruction;
                    if (site.ArgumentCount == 2)
                    {
                        var secondPop = il.Create(OpCodes.Pop);
                        il.InsertAfter(cursor, secondPop);
                        cursor = secondPop;
                    }
                    var nopCount = site.ArgumentCount == 1 ? 4 : 3;
                    for (var i = 0; i < nopCount; i++)
                    {
                        var nop = il.Create(OpCodes.Nop);
                        il.InsertAfter(cursor, nop);
                        cursor = nop;
                    }
                }
                if (CountPrepareMethodReferences(method) != 0)
                    throw new InvalidDataException("Step-32 transformed in-memory PrewarmJit still contains RuntimeHelpers.PrepareMethod references.");
                expectedTransformedInstructionCount = method.Body.Instructions.Count;
                if (expectedTransformedInstructionCount != _expected.SourceInstructionCount + 40)
                    throw new InvalidDataException($"Step-32 exact-length transformed instruction count mismatch: {expectedTransformedInstructionCount}.");
                if (method.Body.Instructions.Count(value => value.OpCode.Code == Code.Pop) != sourcePopCount + 14 ||
                    method.Body.Instructions.Count(value => value.OpCode.Code == Code.Nop) != sourceNopCount + 36)
                    throw new InvalidDataException("Step-32 exact-length Pop/Nop replacement shape did not match the predeclared 50-byte patch contract.");
                expectedTransformedSemanticSha256 = ComputeMethodSemanticFingerprint(method);
                if (resolver.Requests.Count != 0)
                    throw new InvalidDataException($"Cecil dependency resolution occurred while planning the Step-32 raw IL patch: {string.Join(", ", resolver.Requests)}");
            }

            foreach (var window in patchWindows)
                window.Replacement.AsSpan().CopyTo(transformedImage.AsSpan(checked((int)window.FileOffset), 5));
            var changedByteCount = ValidateOnlyApprovedPatchWindowsChanged(sourceImage, transformedImage, patchWindows);
            File.WriteAllBytes(transformedPath, transformedImage);

            var transformedSha256 = ComputeSha256Hex(transformedPath);
            var transformedBytes = new FileInfo(transformedPath).Length;
            if (transformedBytes != source.SourceBytes)
                throw new InvalidDataException($"Step-32 exact-length transformation changed file length: source={source.SourceBytes}, transformed={transformedBytes}.");
            if (!ComputeSha256Hex(source.PrimaryPath).Equals(source.SourceSha256, StringComparison.OrdinalIgnoreCase) ||
                !ComputeSha256Hex(source.PrivateSourcePath).Equals(source.SourceSha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Step-32 source/trusted image changed while writing the transformed copy.");
            if (transformedSha256.Equals(source.SourceSha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Step-32 transformed output is byte-identical to the source image.");
            EnsureStS2NotLoaded("Gate B exit");

            _transformation = new TransformationSnapshot(
                transformedPath, transformedSha256, transformedBytes, oneArgumentReplacements, twoArgumentReplacements,
                sourcePopCount, sourceNopCount, expectedTransformedInstructionCount, expectedTransformedSemanticSha256,
                expectedConstantMetadataSha256, methodLocation, patchWindows, changedByteCount);

            return Pass(gate,
                "FIRST REAL-STS2 EXACT-LENGTH IL TRANSFORMATION WRITTEN TO A LAUNCHER-PRIVATE IMAGE.\n" +
                "Predeclared behavior change: suppress RuntimeHelpers.PrepareMethod eager-compilation requests inside the exact fingerprinted PrewarmJit() method only.\n" +
                "Stack contract — PrepareMethod(handle) 5-byte call -> Pop + Nop + Nop + Nop + Nop.\n" +
                "Stack contract — PrepareMethod(handle, instantiation[]) 5-byte call -> Pop + Pop + Nop + Nop + Nop.\n" +
                $"One-argument sites rewritten: {oneArgumentReplacements}/6\n" +
                $"Two-argument sites rewritten: {twoArgumentReplacements}/4\n" +
                "Patch windows: 10 x exactly 5 bytes\n" +
                $"Changed bytes inside approved windows: {changedByteCount:N0}\n" +
                $"PrewarmJit RVA/code file offset/code size: 0x{methodLocation.MethodRva:X8} / 0x{methodLocation.CodeFileOffset:X} / {methodLocation.CodeSize:N0}\n" +
                "All bytes outside the ten approved call windows unchanged: YES\n" +
                "All later IL offsets / PE layout / metadata tables preserved by equal-length replacement: YES\n" +
                "Cecil serialization performed: NO\n" +
                "Cecil dependency resolution requests during rewrite planning: 0\n" +
                $"Expected transformed PrewarmJit semantic fingerprint SHA-256: {expectedTransformedSemanticSha256}\n" +
                $"Constant metadata semantic fingerprint preserved for reopen verification: {expectedConstantMetadataSha256}\n" +
                $"Source SHA-256 preserved: {source.SourceSha256}\n" +
                $"Transformed SHA-256: {transformedSha256}\n" +
                $"Transformed bytes: {transformedBytes:N0} (exactly source length)\n" +
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
            int sourceNopCount;
            int transformedNopCount;
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
                sourceNopCount = sourceMethod.Body.Instructions.Count(value => value.OpCode.Code == Code.Nop);
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
                transformedNopCount = transformedMethod.Body.Instructions.Count(value => value.OpCode.Code == Code.Nop);
                ValidateReplacementInstructionShape(transformedMethod);
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
            // Gate B planned the exact padded semantic shape in memory but never serialized it. Gate C now
            // reopens the raw-patched image and requires the same offset-independent semantic fingerprint.
            if (!transformedSemanticSha256.Equals(transformation.ExpectedTransformedSemanticSha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Step-32 reopened transformed PrewarmJit does not match the exact in-memory predeclared semantic rewrite.");
            if (!sourceConstantMetadataSha256.Equals(transformation.ExpectedConstantMetadataSha256, StringComparison.OrdinalIgnoreCase) ||
                !transformedConstantMetadataSha256.Equals(sourceConstantMetadataSha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Step-32 source/transformed constant metadata semantics changed even though the raw patch is confined to method IL bytes.");
            if (transformedBodySha256.Equals(sourceBodySha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Step-32 reopened transformed PrewarmJit body fingerprint unexpectedly matches the source body.");
            if (sourceInstructionCount != _expected.SourceInstructionCount || transformedInstructionCount != transformation.ExpectedTransformedInstructionCount)
                throw new InvalidDataException("Step-32 source/transformed instruction count drifted.");
            if (sourceHandlerCount != _expected.SourceExceptionHandlerCount || transformedHandlerCount != sourceHandlerCount)
                throw new InvalidDataException("Step-32 exception-handler topology count changed.");
            var expectedPopDelta = transformation.OneArgumentReplacements + (transformation.TwoArgumentReplacements * 2);
            const int expectedNopDelta = 36;
            if (transformedPopCount != sourcePopCount + expectedPopDelta || transformedNopCount != sourceNopCount + expectedNopDelta)
                throw new InvalidDataException("Step-32 reopened transformed image did not preserve the exact Pop/Nop replacement count.");
            var sourceImage = File.ReadAllBytes(source.PrivateSourcePath);
            var transformedImage = File.ReadAllBytes(transformation.TransformedPath);
            var changedByteCount = ValidateOnlyApprovedPatchWindowsChanged(sourceImage, transformedImage, transformation.PatchWindows);
            if (changedByteCount != transformation.ChangedByteCount || transformedImage.LongLength != sourceImage.LongLength)
                throw new InvalidDataException("Step-32 raw patch byte-diff evidence changed between Gate B and Gate C.");
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
                $"Pop/Nop delta: +{expectedPopDelta} / +{expectedNopDelta}\n" +
                $"Changed bytes confined to ten approved 5-byte windows: YES ({changedByteCount:N0} differing bytes)\n" +
                "PE layout and all bytes outside the selected method-call windows unchanged: YES\n" +
                "Assembly identity/MVID preserved: YES\n" +
                "Reopened transformed semantics match exact pre-write padded plan: YES\n" +
                "Cecil serialization performed by Step 32: NO\n" +
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
                "Cecil read/verification dependency resolution requests: 0; Cecil serialization: NONE\n" +
                $"Exact-length raw patch windows retained: {transformation.PatchWindows.Count}/10; changed bytes: {transformation.ChangedByteCount:N0}\n" +
                "External framework/game assembly bytes opened for transformation: 0\n" +
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

    private void ValidateReplacementInstructionShape(MethodDefinition method)
    {
        foreach (var site in _expected.Sites)
        {
            var expectedCodes = site.ArgumentCount == 1
                ? new[] { Code.Pop, Code.Nop, Code.Nop, Code.Nop, Code.Nop }
                : new[] { Code.Pop, Code.Pop, Code.Nop, Code.Nop, Code.Nop };
            for (var i = 0; i < expectedCodes.Length; i++)
            {
                var instruction = method.Body.Instructions.SingleOrDefault(value => value.Offset == site.IlOffset + i)
                    ?? throw new InvalidDataException($"Step-32 transformed replacement instruction IL_{site.IlOffset + i:X4} is missing.");
                if (instruction.OpCode.Code != expectedCodes[i] || instruction.Operand is not null)
                    throw new InvalidDataException($"Step-32 transformed replacement shape drifted at IL_{site.IlOffset + i:X4}: expected {expectedCodes[i]}, observed {instruction.OpCode.Code}.");
            }
        }
    }

    private static int ValidateOnlyApprovedPatchWindowsChanged(byte[] source, byte[] transformed, IReadOnlyList<RawPatchWindow> windows)
    {
        if (source.Length != transformed.Length)
            throw new InvalidDataException($"Step-32 exact-length patch changed image size: source={source.Length}, transformed={transformed.Length}.");
        var approved = new HashSet<int>();
        foreach (var window in windows)
        {
            var start = checked((int)window.FileOffset);
            if (start < 0 || start + 5 > source.Length)
                throw new InvalidDataException($"Step-32 approved patch window IL_{window.IlOffset:X4} is outside the image.");
            if (!source.AsSpan(start, 5).SequenceEqual(window.Original))
                throw new InvalidDataException($"Step-32 source bytes changed inside approved patch window IL_{window.IlOffset:X4}.");
            if (!transformed.AsSpan(start, 5).SequenceEqual(window.Replacement))
                throw new InvalidDataException($"Step-32 transformed bytes do not match the exact replacement at IL_{window.IlOffset:X4}.");
            for (var i = 0; i < 5; i++)
                if (!approved.Add(start + i))
                    throw new InvalidDataException($"Step-32 approved patch windows overlap at file offset 0x{start + i:X}.");
        }
        if (windows.Count != 10 || approved.Count != 50)
            throw new InvalidDataException($"Step-32 patch-window cardinality drifted: windows={windows.Count}, approved-bytes={approved.Count}.");

        var changed = 0;
        for (var i = 0; i < source.Length; i++)
        {
            if (source[i] == transformed[i]) continue;
            changed++;
            if (!approved.Contains(i))
                throw new InvalidDataException($"Step-32 transformed an unapproved byte at file offset 0x{i:X}.");
        }
        if (changed == 0)
            throw new InvalidDataException("Step-32 exact-length transformation changed zero bytes.");
        return changed;
    }

    private static MethodBodyFileLocation LocateMethodBodyCode(byte[] image, int methodRva)
    {
        if (methodRva <= 0)
            throw new InvalidDataException("Step-32 selected method has an invalid RVA.");

        using var peStream = new MemoryStream(image, writable: false);
        using var peReader = new PEReader(peStream);
        if (!peReader.HasMetadata)
            throw new InvalidDataException("Step-32 source image has no CLR metadata.");

        int sectionIndex;
        try
        {
            sectionIndex = peReader.PEHeaders.GetContainingSectionIndex(methodRva);
        }
        catch (Exception ex) when (ex is ArgumentOutOfRangeException or BadImageFormatException)
        {
            throw new InvalidDataException($"Step-32 could not map selected method RVA 0x{methodRva:X8} to a PE section.", ex);
        }
        if (sectionIndex < 0 || sectionIndex >= peReader.PEHeaders.SectionHeaders.Length)
            throw new InvalidDataException($"Step-32 selected method RVA 0x{methodRva:X8} does not map to a PE section.");

        var section = peReader.PEHeaders.SectionHeaders[sectionIndex];
        var delta = checked(methodRva - section.VirtualAddress);
        if (delta < 0 || delta >= section.SizeOfRawData)
            throw new InvalidDataException("Step-32 selected method RVA maps outside PE raw section data.");
        var methodHeaderOffset = checked((long)section.PointerToRawData + delta);
        if (methodHeaderOffset < 0 || methodHeaderOffset >= image.LongLength)
            throw new InvalidDataException("Step-32 selected method header maps outside the PE image.");

        var headerIndex = checked((int)methodHeaderOffset);
        var first = image[headerIndex];
        int headerSize;
        int codeSize;
        if ((first & 0x3) == 0x2)
        {
            headerSize = 1;
            codeSize = first >> 2;
        }
        else if ((first & 0x3) == 0x3)
        {
            if (headerIndex + 12 > image.Length)
                throw new InvalidDataException("Step-32 selected method has a truncated fat IL header.");
            var flagsAndSize = BinaryPrimitives.ReadUInt16LittleEndian(image.AsSpan(headerIndex, 2));
            headerSize = ((flagsAndSize >> 12) & 0x0F) * 4;
            if (headerSize < 12 || headerIndex + headerSize > image.Length)
                throw new InvalidDataException($"Step-32 selected method has invalid fat IL header size {headerSize}.");
            codeSize = checked((int)BinaryPrimitives.ReadUInt32LittleEndian(image.AsSpan(headerIndex + 4, 4)));
        }
        else
        {
            throw new InvalidDataException($"Step-32 selected method uses unsupported IL header format byte 0x{first:X2}.");
        }

        var codeOffset = checked(methodHeaderOffset + headerSize);
        if (codeSize <= 0 || codeOffset < 0 || codeOffset + codeSize > image.LongLength)
            throw new InvalidDataException("Step-32 selected method IL code range is outside the PE image.");
        return new MethodBodyFileLocation(methodRva, methodHeaderOffset, codeOffset, codeSize, headerSize);
    }

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

    private sealed class RejectingAssemblyResolver : IAssemblyResolver
    {
        private readonly List<string> _requests = [];
        public IReadOnlyList<string> Requests => _requests;
        public AssemblyDefinition Resolve(AssemblyNameReference name) { _requests.Add(name.FullName); throw new AssemblyResolutionException(name); }
        public AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters) { _requests.Add(name.FullName); throw new AssemblyResolutionException(name); }
        public void Dispose() { }
    }

    private sealed record RawPatchWindow(int IlOffset, int ArgumentCount, uint TargetToken, long FileOffset, byte[] Original, byte[] Replacement);
    private readonly record struct MethodBodyFileLocation(int MethodRva, long HeaderFileOffset, long CodeFileOffset, int CodeSize, int HeaderSize);

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
        int SourceNopCount,
        int ExpectedTransformedInstructionCount,
        string ExpectedTransformedSemanticSha256,
        string ExpectedConstantMetadataSha256,
        MethodBodyFileLocation MethodLocation,
        IReadOnlyList<RawPatchWindow> PatchWindows,
        int ChangedByteCount);

    private sealed record VerificationSnapshot(
        string TransformedBodySha256,
        string TransformedSemanticSha256,
        int SourcePrepareMethodCount,
        int TransformedPrepareMethodCount);
}
