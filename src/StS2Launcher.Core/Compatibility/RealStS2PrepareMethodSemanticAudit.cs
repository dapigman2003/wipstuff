using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace StS2Launcher.Core;

/// <summary>
/// Step 31.0 boundary. Physical Step 30 deferred the Step-29 Harmony/PatchAll selection from the
/// base-game frontier and pointed the next evidence iteration at the highest-priority non-mod family:
/// RuntimeHelpers.PrepareMethod calls in MegaCrit.Sts2.Core.Helpers.OneTimeInitialization::PrewarmJit().
/// Step 31 binds the exact receipt-backed method token/body fingerprint and all ten physically audited
/// PrepareMethod sites, then records their exact IL/control-flow/exception context. This boundary is
/// read-only: no Cecil write, no CLR load/invocation of sts2.dll, no Harmony/MonoMod runtime patching,
/// no Godot/game startup, and no native game loading. A PASS may make the family eligible for a later
/// explicitly predeclared rewrite design; it does not itself authorize a semantic change.
/// </summary>
public sealed class RealStS2PrepareMethodSemanticAudit
{
    internal static readonly PrepareMethodEvidence PhysicalStep29PrepareMethodEvidence = new(
        SourceSha1: "e424ace9399a82edea4dd7e0fa5761635dfd6c5d",
        SourceSha256: "e7ceb80669bfaf5c8fccabaa126ae2bb283aba514be5b5b55612579cfd285f18",
        SourceBytes: 9_363_456,
        AssemblyIdentity: "sts2, Version=0.1.0.0, Culture=neutral, PublicKeyToken=null",
        Mvid: Guid.Parse("518e4758-52d7-47c2-b776-471a0e29e49d"),
        SourceType: "MegaCrit.Sts2.Core.Helpers.OneTimeInitialization",
        SourceMethod: "System.Void MegaCrit.Sts2.Core.Helpers.OneTimeInitialization::PrewarmJit()",
        MethodToken: 0x06007D05,
        MethodBodySha256: "7f25b7bd955c407fc69306cf26af2162223353f5606560458066aed085e72ab9",
        Sites:
        [
            new(0x003D, "Call", "System.Runtime", "System.Void System.Runtime.CompilerServices.RuntimeHelpers::PrepareMethod(System.RuntimeMethodHandle)"),
            new(0x0052, "Call", "System.Runtime", "System.Void System.Runtime.CompilerServices.RuntimeHelpers::PrepareMethod(System.RuntimeMethodHandle)"),
            new(0x007A, "Call", "System.Runtime", "System.Void System.Runtime.CompilerServices.RuntimeHelpers::PrepareMethod(System.RuntimeMethodHandle,System.RuntimeTypeHandle[])"),
            new(0x00A2, "Call", "System.Runtime", "System.Void System.Runtime.CompilerServices.RuntimeHelpers::PrepareMethod(System.RuntimeMethodHandle,System.RuntimeTypeHandle[])"),
            new(0x00CA, "Call", "System.Runtime", "System.Void System.Runtime.CompilerServices.RuntimeHelpers::PrepareMethod(System.RuntimeMethodHandle,System.RuntimeTypeHandle[])"),
            new(0x00F2, "Call", "System.Runtime", "System.Void System.Runtime.CompilerServices.RuntimeHelpers::PrepareMethod(System.RuntimeMethodHandle,System.RuntimeTypeHandle[])"),
            new(0x0136, "Call", "System.Runtime", "System.Void System.Runtime.CompilerServices.RuntimeHelpers::PrepareMethod(System.RuntimeMethodHandle)"),
            new(0x014C, "Call", "System.Runtime", "System.Void System.Runtime.CompilerServices.RuntimeHelpers::PrepareMethod(System.RuntimeMethodHandle)"),
            new(0x0162, "Call", "System.Runtime", "System.Void System.Runtime.CompilerServices.RuntimeHelpers::PrepareMethod(System.RuntimeMethodHandle)"),
            new(0x0178, "Call", "System.Runtime", "System.Void System.Runtime.CompilerServices.RuntimeHelpers::PrepareMethod(System.RuntimeMethodHandle)"),
        ]);

    private readonly string _launcherDataRoot;
    private readonly SteamOfflineInstallInspection _offlineInspection;
    private readonly PrepareMethodEvidence _expected;
    private SourceSnapshot? _source;
    private SemanticContextSnapshot? _context;
    private DispositionSnapshot? _disposition;

    public RealStS2PrepareMethodSemanticAudit(string launcherDataRoot)
        : this(launcherDataRoot, PhysicalStep29PrepareMethodEvidence)
    {
    }

    internal RealStS2PrepareMethodSemanticAudit(string launcherDataRoot, PrepareMethodEvidence expected)
    {
        if (string.IsNullOrWhiteSpace(launcherDataRoot))
            throw new ArgumentException("Launcher data root is required.", nameof(launcherDataRoot));
        _launcherDataRoot = Path.GetFullPath(launcherDataRoot);
        _offlineInspection = new SteamOfflineInstallInspection(_launcherDataRoot);
        _expected = expected ?? throw new ArgumentNullException(nameof(expected));
    }

    public void Reset()
    {
        _source = null;
        _context = null;
        _disposition = null;
    }

    public async Task<RealStS2PrepareMethodSemanticAuditGateResult> RunEvidenceBindingAndOfflineReadyAsync(
        IProgress<RealStS2PrepareMethodSemanticAuditProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        const RealStS2PrepareMethodSemanticAuditGate gate = RealStS2PrepareMethodSemanticAuditGate.EvidenceBindingAndOfflineReady;
        try
        {
            Reset();
            cancellationToken.ThrowIfCancellationRequested();
            progress?.Report(new RealStS2PrepareMethodSemanticAuditProgress(
                gate, 0, 0, null,
                "Re-proving OfflineReady and binding the Step-29 PrewarmJit token/body fingerprint plus all ten PrepareMethod sites to the exact receipt-backed ARM64 sts2.dll…"));

            IProgress<SteamOfflineInstallProgress>? offlineProgress = progress is null
                ? null
                : new CallbackProgress<SteamOfflineInstallProgress>(value =>
                    progress.Report(new RealStS2PrepareMethodSemanticAuditProgress(
                        gate,
                        value.CompletedFiles,
                        value.TotalFiles,
                        value.CurrentFile,
                        $"OfflineReady precondition — {value.Message} ({value.CompletedBytes:N0}/{value.TotalBytes:N0} bytes)")));

            var offline = await _offlineInspection.RunAsync(offlineProgress, cancellationToken).ConfigureAwait(false);
            if (offline.Outcome == SteamOfflineInstallOutcome.Cancelled)
                throw new OperationCanceledException("Step 31 OfflineReady precondition was cancelled.", cancellationToken);
            if (!offline.Success || string.IsNullOrWhiteSpace(offline.ManagedInstallRelativePath))
                throw new InvalidDataException(offline.Error ?? $"Managed install is not OfflineReady ({offline.State}/{offline.Outcome}).");

            var managedRoot = ResolveChildPath(_launcherDataRoot, offline.ManagedInstallRelativePath);
            var receiptPath = Path.Combine(managedRoot, SteamManagedInstallReceipt.FileName);
            SteamManagedInstallReceipt? receipt;
            await using (var stream = new FileStream(receiptPath, FileMode.Open, FileAccess.Read, FileShare.Read, 16 * 1024, FileOptions.Asynchronous | FileOptions.SequentialScan))
            {
                receipt = await JsonSerializer.DeserializeAsync(
                    stream,
                    SteamManagedInstallJsonContext.Default.SteamManagedInstallReceipt,
                    cancellationToken).ConfigureAwait(false);
            }
            if (receipt is null)
                throw new InvalidDataException("The verified Step 12 receipt unexpectedly deserialized to null.");
            ValidateReceiptSnapshot(receipt, offline);

            var matches = receipt.Files.Where(file => IsPrimaryArm64StS2Path(file.RelativePath)).ToArray();
            if (matches.Length != 1)
                throw new InvalidDataException($"Expected exactly one receipt-backed macOS arm64 sts2.dll, found {matches.Length}.");
            var primary = matches[0];
            var relative = primary.RelativePath.Replace('\\', '/');
            var path = ResolveChildPath(managedRoot, relative);
            if (!File.Exists(path))
                throw new FileNotFoundException("The receipt-backed primary sts2.dll is missing.", path);

            var bytes = new FileInfo(path).Length;
            var sha1 = await ComputeHashHexAsync(path, SHA1.Create(), cancellationToken).ConfigureAwait(false);
            var sha256 = await ComputeHashHexAsync(path, SHA256.Create(), cancellationToken).ConfigureAwait(false);
            if (bytes != primary.Length || !sha1.Equals(primary.Sha1Hex, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Primary sts2.dll no longer matches its trusted Step 12 receipt length/SHA-1.");
            if (bytes != _expected.SourceBytes ||
                !sha1.Equals(_expected.SourceSha1, StringComparison.OrdinalIgnoreCase) ||
                !sha256.Equals(_expected.SourceSha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Primary sts2.dll does not match the physically closed Step-29/30 source identity.");

            if (FindLoadedAssemblyIdentities("sts2").Count != 0)
                throw new InvalidDataException("A sts2 assembly identity is already resident in the CLR. Force-quit before Step 31.");

            using var resolver = new RejectingAssemblyResolver();
            using var module = ReadModuleDeferred(path, resolver);
            if (module.Assembly?.Name is null)
                throw new InvalidDataException("Primary sts2.dll does not contain a managed assembly manifest.");
            if (!module.Assembly.Name.FullName.Equals(_expected.AssemblyIdentity, StringComparison.Ordinal) || module.Mvid != _expected.Mvid)
                throw new InvalidDataException("Primary sts2.dll assembly identity/MVID does not match the physical evidence.");

            var method = FindMethodByToken(module, _expected.MethodToken);
            ValidatePrepareMethodEvidence(method, _expected);
            var bodyHash = ComputeMethodBodyFingerprint(method);
            if (!bodyHash.Equals(_expected.MethodBodySha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException($"PrewarmJit method body fingerprint drift: expected {_expected.MethodBodySha256}, observed {bodyHash}.");
            if (resolver.Requests.Count != 0)
                throw new InvalidDataException("Step-31 evidence binding unexpectedly requested Cecil dependency resolution: " + string.Join(" | ", resolver.Requests));
            if (FindLoadedAssemblyIdentities("sts2").Count != 0)
                throw new InvalidDataException("Step-31 Cecil evidence binding unexpectedly caused sts2.dll to enter the CLR.");

            _source = new SourceSnapshot(offline, managedRoot, relative, path, bytes, sha1, sha256, module.Assembly.Name.FullName, module.Mvid, bodyHash);
            return Pass(gate,
                "PHYSICAL STEP-29 PREPAREMETHOD FAMILY EVIDENCE REBOUND TO THE EXACT RECEIPT-BACKED SOURCE WITHOUT CLR ADMISSION.\n" +
                $"OfflineReady precondition: PASS ({offline.VerifiedFiles:N0}/{offline.PlannedFiles:N0} files)\n" +
                $"Primary: {relative}\n" +
                $"Assembly identity: {_source.AssemblyIdentity}\n" +
                $"Module MVID: {_source.Mvid:D}\n" +
                $"Receipt SHA-1: {_source.Sha1}\n" +
                $"Source SHA-256: {_source.Sha256}\n" +
                $"Source bytes: {_source.Bytes:N0}\n" +
                $"Selected source method: {_expected.SourceMethod}\n" +
                $"Selected method token: 0x{_expected.MethodToken:X8}\n" +
                $"Selected method-body fingerprint SHA-256: {bodyHash}\n" +
                $"Expected PrepareMethod sites rebound: {_expected.Sites.Count:N0}/{_expected.Sites.Count:N0}\n" +
                "Expected offsets: " + string.Join(", ", _expected.Sites.Select(site => $"IL_{site.IlOffset:X4}")) + "\n" +
                "Cecil reading mode: Deferred\n" +
                "Cecil dependency resolution requests: 0\n" +
                "sts2 CLR-loaded before/after Gate A: NO / NO\n" +
                "Trusted Step 12 managed install modified: NO");
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex) { return Fail(gate, ex); }
    }

    public RealStS2PrepareMethodSemanticAuditGateResult RunExactPrepareMethodSemanticContextAudit()
    {
        const RealStS2PrepareMethodSemanticAuditGate gate = RealStS2PrepareMethodSemanticAuditGate.ExactPrepareMethodSemanticContextAudit;
        try
        {
            var source = RequireSource();
            using var resolver = new RejectingAssemblyResolver();
            using var module = ReadModuleDeferred(source.PrimaryPath, resolver);
            var method = FindMethodByToken(module, _expected.MethodToken);
            ValidatePrepareMethodEvidence(method, _expected);
            var bodyHash = ComputeMethodBodyFingerprint(method);
            if (!bodyHash.Equals(_expected.MethodBodySha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("PrewarmJit method body changed between Step-31 gates.");

            var instructions = method.Body.Instructions;
            var siteContexts = new List<PrepareMethodSiteContext>();
            const int beforeRadius = 10;
            const int afterRadius = 4;
            foreach (var expectedSite in _expected.Sites)
            {
                var index = FindInstructionIndex(instructions, expectedSite.IlOffset);
                if (index < 0)
                    throw new InvalidDataException($"Expected PrepareMethod site IL_{expectedSite.IlOffset:X4} is no longer present.");
                var instruction = instructions[index];
                var target = instruction.Operand as MethodReference
                    ?? throw new InvalidDataException($"PrepareMethod site IL_{expectedSite.IlOffset:X4} no longer carries a method-reference operand.");
                var first = Math.Max(0, index - beforeRadius);
                var last = Math.Min(instructions.Count - 1, index + afterRadius);
                var window = instructions.Skip(first).Take(last - first + 1).Select(FormatInstruction).ToArray();
                var incoming = EnumerateBranchSources(method, instruction).Select(FormatInstruction).ToArray();
                var handlers = method.Body.ExceptionHandlers.Where(handler => CoversInstruction(handler, instruction)).Select(FormatExceptionHandler).ToArray();
                siteContexts.Add(new PrepareMethodSiteContext(
                    expectedSite.IlOffset,
                    instruction.OpCode.Code.ToString(),
                    target.FullName,
                    target.Parameters.Count,
                    incoming,
                    handlers,
                    window));
            }

            var actualPrepareSites = EnumeratePrepareMethodSites(method).ToArray();
            var otherRuntimeHelperSites = CountMethodReferences(method, reference =>
                reference.DeclaringType.FullName == "System.Runtime.CompilerServices.RuntimeHelpers" && reference.Name != "PrepareMethod");
            var harmonyOrModReferences = CountMethodReferences(method, reference =>
                GetTargetScopeName(reference).Equals("0Harmony", StringComparison.OrdinalIgnoreCase) ||
                reference.DeclaringType.FullName.StartsWith("HarmonyLib.", StringComparison.Ordinal) ||
                reference.DeclaringType.FullName.StartsWith("MegaCrit.Sts2.Core.Modding.", StringComparison.Ordinal));
            var strings = instructions
                .Where(instruction => instruction.OpCode.Code == Code.Ldstr && instruction.Operand is string)
                .Select(instruction => (string)instruction.Operand)
                .Distinct(StringComparer.Ordinal)
                .ToArray();
            var isExpectedMethod = method.DeclaringType.FullName == _expected.SourceType &&
                                   method.FullName == _expected.SourceMethod &&
                                   method.IsStatic &&
                                   method.Parameters.Count == 0;
            var allDirectCalls = actualPrepareSites.All(instruction => instruction.OpCode.Code == Code.Call);
            var allVoidTargets = actualPrepareSites.All(instruction => instruction.Operand is MethodReference reference && reference.ReturnType.FullName == "System.Void");
            if (resolver.Requests.Count != 0)
                throw new InvalidDataException("Step-31 semantic context audit unexpectedly requested Cecil dependency resolution: " + string.Join(" | ", resolver.Requests));
            if (FindLoadedAssemblyIdentities("sts2").Count != 0)
                throw new InvalidDataException("Step-31 semantic context audit unexpectedly caused sts2.dll to enter the CLR.");

            _context = new SemanticContextSnapshot(
                isExpectedMethod,
                actualPrepareSites.Length,
                allDirectCalls,
                allVoidTargets,
                method.Body.Instructions.Count,
                method.Body.MaxStackSize,
                method.Body.InitLocals,
                method.Body.Variables.Count,
                method.Body.ExceptionHandlers.Count,
                otherRuntimeHelperSites,
                harmonyOrModReferences,
                siteContexts.ToArray(),
                strings);

            var siteDetail = string.Join("\n", siteContexts.Select(context =>
                $"PrepareMethod site IL_{context.IlOffset:X4}: {context.OpCode} -> {context.TargetMember}; args={context.ParameterCount}; branches-targeting={context.IncomingBranches.Length}; covering-handlers={context.CoveringHandlers.Length}\n" +
                "  Context (10 before / 4 after, bounded):\n  " + string.Join("\n  ", context.IlWindow) + "\n" +
                "  Branch sources: " + (context.IncomingBranches.Length == 0 ? "none" : string.Join(" | ", context.IncomingBranches)) + "\n" +
                "  Covering exception regions: " + (context.CoveringHandlers.Length == 0 ? "none" : string.Join(" | ", context.CoveringHandlers))));

            return Pass(gate,
                "EXACT PREWARMJIT/PREPAREMETHOD SEMANTICS INSPECTED WITHOUT RESOLUTION, REWRITE, OR EXECUTION.\n" +
                $"Declaring type: {method.DeclaringType.FullName}\n" +
                $"Method: {method.FullName}\n" +
                $"Method token/body fingerprint: 0x{method.MetadataToken.ToUInt32():X8} / {bodyHash}\n" +
                $"Method static / parameters: {(method.IsStatic ? "YES" : "NO")} / {method.Parameters.Count}\n" +
                $"Method body: instructions={instructions.Count:N0}; max-stack={method.Body.MaxStackSize}; init-locals={(method.Body.InitLocals ? "YES" : "NO")}; locals={method.Body.Variables.Count}; exception-handlers={method.Body.ExceptionHandlers.Count}\n" +
                $"PrepareMethod sites: {actualPrepareSites.Length:N0}\n" +
                $"All PrepareMethod sites are direct Call: {(allDirectCalls ? "YES" : "NO")}\n" +
                $"All PrepareMethod targets return void: {(allVoidTargets ? "YES" : "NO")}\n" +
                $"Other RuntimeHelpers method-reference sites: {otherRuntimeHelperSites:N0}\n" +
                $"Harmony/mod method-reference sites in PrewarmJit: {harmonyOrModReferences:N0}\n" +
                "Exact per-site IL/control-flow/exception context:\n" + siteDetail + "\n" +
                "String literals in PrewarmJit:\n" + (strings.Length == 0 ? "• none" : string.Join("\n", strings.Select(value => "• " + value))) + "\n" +
                "Cecil dependency resolution requests: 0\n" +
                "Real StS2 CLR load/invocation: NO\n" +
                "Cecil writes: 0");
        }
        catch (Exception ex) { return Fail(gate, ex); }
    }

    public RealStS2PrepareMethodSemanticAuditGateResult RunDeterministicDisposition()
    {
        const RealStS2PrepareMethodSemanticAuditGate gate = RealStS2PrepareMethodSemanticAuditGate.DeterministicDisposition;
        try
        {
            var context = RequireContext();
            if (!context.IsExpectedMethod || context.PrepareMethodSiteCount != _expected.Sites.Count || !context.AllDirectCalls || !context.AllVoidTargets)
                throw new InvalidDataException("The exact physically audited PrewarmJit/PrepareMethod family no longer has the expected structural shape; do not advance it toward rewrite design.");

            const string status = "BASE-GAME COMPATIBILITY FAMILY CONFIRMED — ELIGIBLE FOR EXPLICIT REWRITE DESIGN; NO WRITE AUTHORIZED";
            _disposition = new DispositionSnapshot(status, RewriteAuthorized: false, RewriteDesignEligible: true);
            return Pass(gate,
                "THE PREWARMJIT/PREPAREMETHOD FAMILY IS RETAINED ON THE BASE-GAME COMPATIBILITY FRONTIER, BUT THIS BUILD AUTHORIZES NO SEMANTIC CHANGE.\n" +
                $"Disposition: {status}\n" +
                "Evidence basis: the exact physical Step-29 method remains MegaCrit.Sts2.Core.Helpers.OneTimeInitialization::PrewarmJit(), token 0x06007D05, with the same body fingerprint and exactly ten direct RuntimeHelpers.PrepareMethod calls at the audited offsets.\n" +
                "Product-scope basis: unlike the Step-30 Harmony selection, this family is not structurally confined to MegaCrit.Sts2.Core.Modding and is therefore retained for base-game compatibility investigation.\n" +
                "Predeclared behavior change for Step 31: NONE. This candidate does not delete, replace, bypass, NOP, or otherwise rewrite any PrepareMethod call.\n" +
                "Runtime reachability claim: NONE — structural presence inside OneTimeInitialization::PrewarmJit() does not prove when or whether the method runs during iOS base-game startup.\n" +
                "Next frontier after physical closure: design one narrowly bounded ahead-of-load transformation for this exact fingerprinted method/sites, predeclare its stack/control-flow semantics, transform only a launcher-private copy, and verify the transformed image before any CLR admission.\n" +
                "Real StS2 bytes changed: NO\n" +
                "Real StS2 member invoked: NO");
        }
        catch (Exception ex) { return Fail(gate, ex); }
    }

    public async Task<RealStS2PrepareMethodSemanticAuditGateResult> RunFinalIsolationAuditAsync(
        IProgress<RealStS2PrepareMethodSemanticAuditProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        const RealStS2PrepareMethodSemanticAuditGate gate = RealStS2PrepareMethodSemanticAuditGate.FinalIsolationAudit;
        try
        {
            var source = RequireSource();
            var context = RequireContext();
            var disposition = RequireDisposition();
            progress?.Report(new RealStS2PrepareMethodSemanticAuditProgress(
                gate, 0, 0, source.PrimaryRelativePath,
                "Re-hashing primary sts2.dll and re-proving OfflineReady after the PrepareMethod semantic-context audit…"));

            var bytes = new FileInfo(source.PrimaryPath).Length;
            var sha1 = await ComputeHashHexAsync(source.PrimaryPath, SHA1.Create(), cancellationToken).ConfigureAwait(false);
            var sha256 = await ComputeHashHexAsync(source.PrimaryPath, SHA256.Create(), cancellationToken).ConfigureAwait(false);
            if (bytes != source.Bytes || !sha1.Equals(source.Sha1, StringComparison.OrdinalIgnoreCase) || !sha256.Equals(source.Sha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Primary sts2.dll changed across the Step-31 read-only audit.");

            IProgress<SteamOfflineInstallProgress>? offlineProgress = progress is null
                ? null
                : new CallbackProgress<SteamOfflineInstallProgress>(value =>
                    progress.Report(new RealStS2PrepareMethodSemanticAuditProgress(
                        gate,
                        value.CompletedFiles,
                        value.TotalFiles,
                        value.CurrentFile,
                        $"Post-audit OfflineReady — {value.Message} ({value.CompletedBytes:N0}/{value.TotalBytes:N0} bytes)")));
            var offline = await _offlineInspection.RunAsync(offlineProgress, cancellationToken).ConfigureAwait(false);
            if (!offline.Success || !offline.ExactManagedTreeVerified)
                throw new InvalidDataException(offline.Error ?? "Post-audit OfflineReady verification failed.");
            if (FindLoadedAssemblyIdentities("sts2").Count != 0)
                throw new InvalidDataException("Step 31 unexpectedly caused a sts2 assembly identity to become CLR-resident.");

            return Pass(gate,
                "STEP 31.0 FINAL READ-ONLY ISOLATION AUDIT PASSED.\n" +
                $"Primary receipt SHA-1 unchanged: {sha1}\n" +
                $"Primary SHA-256 unchanged: {sha256}\n" +
                $"Primary bytes unchanged: {bytes:N0}\n" +
                $"Post-audit OfflineReady: PASS ({offline.VerifiedFiles:N0}/{offline.PlannedFiles:N0} files)\n" +
                "Trusted Step 12 managed install unchanged: YES\n" +
                "sts2 assembly/type/member CLR load or invocation by Step 31: NO\n" +
                "Cecil dependency resolution requests across audit: 0\n" +
                $"PrepareMethod sites remained exact: {context.PrepareMethodSiteCount:N0}/{_expected.Sites.Count:N0}\n" +
                $"Disposition: {disposition.Status}\n" +
                $"Rewrite-design eligibility recorded: {(disposition.RewriteDesignEligible ? "YES" : "NO")}\n" +
                $"Real-game rewrite authorized by Step 31: {(disposition.RewriteAuthorized ? "YES" : "NO")}\n" +
                "Cecil writes performed by Step 31: 0\n" +
                "Harmony/MonoMod runtime patching invoked by Step 31: NO\n" +
                "Godot/game startup or native game loading attempted by Step 31: NO");
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex) { return Fail(gate, ex); }
    }

    private SourceSnapshot RequireSource() => _source ?? throw new InvalidOperationException("Step 31 Gate A must pass before later gates run.");
    private SemanticContextSnapshot RequireContext() => _context ?? throw new InvalidOperationException("Step 31 Gate B must pass before later gates run.");
    private DispositionSnapshot RequireDisposition() => _disposition ?? throw new InvalidOperationException("Step 31 Gate C must pass before the final isolation audit.");

    private static ModuleDefinition ReadModuleDeferred(string path, IAssemblyResolver resolver)
        => ModuleDefinition.ReadModule(path, new ReaderParameters
        {
            ReadSymbols = false,
            ReadingMode = ReadingMode.Deferred,
            AssemblyResolver = resolver,
            MetadataResolver = new MetadataResolver(resolver),
        });

    private static MethodDefinition FindMethodByToken(ModuleDefinition module, uint token)
    {
        var matches = EnumerateTypes(module.Types)
            .SelectMany(type => type.Methods)
            .Where(method => method.MetadataToken.ToUInt32() == token)
            .ToArray();
        if (matches.Length != 1)
            throw new InvalidDataException($"Expected exactly one method token 0x{token:X8}, found {matches.Length}.");
        return matches[0];
    }

    private static void ValidatePrepareMethodEvidence(MethodDefinition method, PrepareMethodEvidence expected)
    {
        if (!method.HasBody)
            throw new InvalidDataException("Physically audited PrewarmJit method no longer has managed IL.");
        if (method.DeclaringType.FullName != expected.SourceType || method.FullName != expected.SourceMethod)
            throw new InvalidDataException($"PrewarmJit method token identity drift: observed {method.FullName}.");
        var actualSites = EnumeratePrepareMethodSites(method).ToArray();
        if (actualSites.Length != expected.Sites.Count)
            throw new InvalidDataException($"Expected exactly {expected.Sites.Count} PrepareMethod sites, observed {actualSites.Length}.");
        foreach (var site in expected.Sites)
        {
            var matches = method.Body.Instructions.Where(instruction => instruction.Offset == site.IlOffset).ToArray();
            if (matches.Length != 1)
                throw new InvalidDataException($"Expected exactly one instruction at IL_{site.IlOffset:X4}, found {matches.Length}.");
            var instruction = matches[0];
            if (!instruction.OpCode.Code.ToString().Equals(site.OpCode, StringComparison.Ordinal) || instruction.Operand is not MethodReference target)
                throw new InvalidDataException($"PrepareMethod site IL_{site.IlOffset:X4} opcode/operand shape drifted from the Step-29 evidence.");
            if (!GetTargetScopeName(target).Equals(site.TargetScope, StringComparison.Ordinal) || target.FullName != site.TargetMember)
                throw new InvalidDataException($"PrepareMethod site IL_{site.IlOffset:X4} target drift: observed [{GetTargetScopeName(target)}] {target.FullName}.");
        }
    }

    private static IEnumerable<Instruction> EnumeratePrepareMethodSites(MethodDefinition method)
        => method.Body.Instructions.Where(instruction =>
            instruction.Operand is MethodReference reference &&
            reference.DeclaringType.FullName == "System.Runtime.CompilerServices.RuntimeHelpers" &&
            reference.Name == "PrepareMethod");

    private static int FindInstructionIndex(Mono.Collections.Generic.Collection<Instruction> instructions, int offset)
    {
        for (var index = 0; index < instructions.Count; index++)
            if (instructions[index].Offset == offset)
                return index;
        return -1;
    }

    private static IEnumerable<Instruction> EnumerateBranchSources(MethodDefinition method, Instruction target)
    {
        foreach (var instruction in method.Body.Instructions)
        {
            if (ReferenceEquals(instruction.Operand, target))
                yield return instruction;
            else if (instruction.Operand is Instruction[] targets && targets.Any(item => ReferenceEquals(item, target)))
                yield return instruction;
        }
    }

    private static bool CoversInstruction(ExceptionHandler handler, Instruction selected)
    {
        var offset = selected.Offset;
        return InRange(offset, handler.TryStart, handler.TryEnd) ||
               InRange(offset, handler.HandlerStart, handler.HandlerEnd) ||
               (handler.FilterStart is not null && InRange(offset, handler.FilterStart, handler.HandlerStart));
    }

    private static bool InRange(int offset, Instruction? start, Instruction? end)
        => start is not null && offset >= start.Offset && (end is null || offset < end.Offset);

    private static string FormatExceptionHandler(ExceptionHandler handler)
        => $"{handler.HandlerType}; try={FormatRange(handler.TryStart, handler.TryEnd)}; handler={FormatRange(handler.HandlerStart, handler.HandlerEnd)}; filter={(handler.FilterStart is null ? "none" : $"IL_{handler.FilterStart.Offset:X4}")}; catch={handler.CatchType?.FullName ?? "none"}";

    private static string FormatRange(Instruction? start, Instruction? end)
        => start is null ? "none" : $"IL_{start.Offset:X4}..{(end is null ? "<end>" : $"IL_{end.Offset:X4}")}";

    private static int CountMethodReferences(MethodDefinition method, Func<MethodReference, bool> predicate)
        => method.Body.Instructions.Count(instruction =>
            instruction.Operand is MethodReference reference && predicate(reference));

    private static string FormatInstruction(Instruction instruction)
        => $"IL_{instruction.Offset:X4} {instruction.OpCode.Code} {FormatOperand(instruction.Operand)}".TrimEnd();

    internal static string ComputeMethodBodyFingerprint(MethodDefinition method)
    {
        var canonical = new StringBuilder();
        canonical.Append(method.FullName).Append('\n');
        foreach (var instruction in method.Body.Instructions)
        {
            canonical.Append(instruction.Offset.ToString("X4"))
                .Append('|').Append(instruction.OpCode.Code)
                .Append('|').Append(FormatOperand(instruction.Operand))
                .Append('\n');
        }
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical.ToString()))).ToLowerInvariant();
    }

    private static string FormatOperand(object? operand)
        => operand switch
        {
            null => string.Empty,
            MethodReference method => $"method:{GetTargetScopeName(method)}:{method.FullName}",
            FieldReference field => $"field:{field.DeclaringType.Scope?.Name}:{field.FullName}",
            TypeReference type => $"type:{type.Scope?.Name}:{type.FullName}",
            Instruction instruction => $"IL_{instruction.Offset:X4}",
            Instruction[] instructions => string.Join(",", instructions.Select(value => $"IL_{value.Offset:X4}")),
            VariableDefinition variable => $"V_{variable.Index}",
            ParameterDefinition parameter => $"P_{parameter.Index}:{parameter.ParameterType.FullName}",
            string text => $"string:{text}",
            _ => Convert.ToString(operand, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
        };

    private static string GetTargetScopeName(MethodReference target)
    {
        IMetadataScope? scope = target.DeclaringType.Scope;
        return scope switch
        {
            AssemblyNameReference assembly => assembly.Name ?? string.Empty,
            ModuleDefinition module => module.Assembly?.Name?.Name ?? module.Name,
            ModuleReference moduleReference => moduleReference.Name ?? string.Empty,
            _ => scope?.Name ?? string.Empty,
        };
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

    private static IReadOnlyList<string> FindLoadedAssemblyIdentities(string simpleName)
        => AppDomain.CurrentDomain.GetAssemblies()
            .Select(assembly => assembly.GetName())
            .Where(name => string.Equals(name.Name, simpleName, StringComparison.OrdinalIgnoreCase))
            .Select(name => name.FullName ?? name.Name ?? "<unknown>")
            .OrderBy(value => value, StringComparer.Ordinal)
            .ToArray();

    private static bool IsPrimaryArm64StS2Path(string path)
        => ("/" + path.Replace('\\', '/').TrimStart('/')).EndsWith("/data_sts2_macos_arm64/sts2.dll", StringComparison.OrdinalIgnoreCase);

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
            throw new InvalidDataException("The Step 12 receipt changed or became inconsistent after OfflineReady was proven.");
        var unique = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var file in receipt.Files)
        {
            if (file is null || !SteamSingleFileTargetSelector.IsSafeRelativePath(file.RelativePath) || file.Length < 0 ||
                file.Sha1Hex.Length != 40 || !file.Sha1Hex.All(Uri.IsHexDigit) || !unique.Add(file.RelativePath.Replace('\\', '/')))
                throw new InvalidDataException("The Step 12 receipt contains an invalid or duplicate file entry.");
        }
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
            var hash = await algorithm.ComputeHashAsync(stream, cancellationToken).ConfigureAwait(false);
            return Convert.ToHexString(hash).ToLowerInvariant();
        }
    }

    private static RealStS2PrepareMethodSemanticAuditGateResult Pass(RealStS2PrepareMethodSemanticAuditGate gate, string detail) => new(gate, true, detail);
    private static RealStS2PrepareMethodSemanticAuditGateResult Fail(RealStS2PrepareMethodSemanticAuditGate gate, Exception ex) => new(gate, false, $"Stage failed with {ex.GetType().Name}: {ex.Message}\n{ex}");

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

    internal sealed record PrepareMethodCallSiteEvidence(int IlOffset, string OpCode, string TargetScope, string TargetMember);

    internal sealed record PrepareMethodEvidence(
        string SourceSha1,
        string SourceSha256,
        long SourceBytes,
        string AssemblyIdentity,
        Guid Mvid,
        string SourceType,
        string SourceMethod,
        uint MethodToken,
        string MethodBodySha256,
        IReadOnlyList<PrepareMethodCallSiteEvidence> Sites);

    private sealed record SourceSnapshot(
        SteamOfflineInstallResult Offline,
        string ManagedRoot,
        string PrimaryRelativePath,
        string PrimaryPath,
        long Bytes,
        string Sha1,
        string Sha256,
        string AssemblyIdentity,
        Guid Mvid,
        string MethodBodySha256);

    private sealed record PrepareMethodSiteContext(
        int IlOffset,
        string OpCode,
        string TargetMember,
        int ParameterCount,
        string[] IncomingBranches,
        string[] CoveringHandlers,
        string[] IlWindow);

    private sealed record SemanticContextSnapshot(
        bool IsExpectedMethod,
        int PrepareMethodSiteCount,
        bool AllDirectCalls,
        bool AllVoidTargets,
        int InstructionCount,
        int MaxStack,
        bool InitLocals,
        int LocalCount,
        int ExceptionHandlerCount,
        int OtherRuntimeHelperReferenceCount,
        int HarmonyOrModReferenceCount,
        PrepareMethodSiteContext[] SiteContexts,
        string[] StringLiterals);

    private sealed record DispositionSnapshot(string Status, bool RewriteAuthorized, bool RewriteDesignEligible);
}
