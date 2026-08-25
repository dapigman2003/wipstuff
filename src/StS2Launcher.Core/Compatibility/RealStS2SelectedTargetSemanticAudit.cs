using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace StS2Launcher.Core;

/// <summary>
/// Step 30.0 boundary. Physical Step 29 selected one exact Harmony.PatchAll call in
/// MegaCrit.Sts2.Core.Modding.ModManager::TryLoadMod, but explicitly authorized audit only.
/// Step 30 binds that physical evidence back to the same receipt-backed ARM64 sts2.dll and
/// inspects the selected method's exact IL/control-flow/exception context before any semantic
/// rewrite is allowed. Because the selected site is structurally inside the mod-loading path,
/// this step deterministically decides whether it belongs on the base-game compatibility
/// frontier. The boundary is read-only: no Cecil write, no CLR load/invocation of sts2.dll,
/// no Harmony/MonoMod runtime patching, no Godot/game startup, and no native game loading.
/// </summary>
public sealed class RealStS2SelectedTargetSemanticAudit
{
    internal static readonly SelectedTargetEvidence PhysicalStep29Evidence = new(
        SourceSha1: "e424ace9399a82edea4dd7e0fa5761635dfd6c5d",
        SourceSha256: "e7ceb80669bfaf5c8fccabaa126ae2bb283aba514be5b5b55612579cfd285f18",
        SourceBytes: 9_363_456,
        AssemblyIdentity: "sts2, Version=0.1.0.0, Culture=neutral, PublicKeyToken=null",
        Mvid: Guid.Parse("518e4758-52d7-47c2-b776-471a0e29e49d"),
        SourceType: "MegaCrit.Sts2.Core.Modding.ModManager",
        SourceMethod: "System.Void MegaCrit.Sts2.Core.Modding.ModManager::TryLoadMod(MegaCrit.Sts2.Core.Modding.Mod)",
        MethodToken: 0x06007927,
        IlOffset: 0x0D9D,
        OpCode: "Callvirt",
        TargetScope: "0Harmony",
        TargetMember: "System.Void HarmonyLib.Harmony::PatchAll(System.Reflection.Assembly)",
        MethodBodySha256: "50c8c4394082f3c73df414fad8675540cfc00a99ccc4f350b616cec574cdbcbd");

    private readonly string _launcherDataRoot;
    private readonly SteamOfflineInstallInspection _offlineInspection;
    private readonly SelectedTargetEvidence _expected;
    private SourceSnapshot? _source;
    private SemanticContextSnapshot? _context;
    private DispositionSnapshot? _disposition;

    public RealStS2SelectedTargetSemanticAudit(string launcherDataRoot)
        : this(launcherDataRoot, PhysicalStep29Evidence)
    {
    }

    internal RealStS2SelectedTargetSemanticAudit(string launcherDataRoot, SelectedTargetEvidence expected)
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

    public async Task<RealStS2SelectedTargetSemanticAuditGateResult> RunSelectedEvidenceBindingAndOfflineReadyAsync(
        IProgress<RealStS2SelectedTargetSemanticAuditProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        const RealStS2SelectedTargetSemanticAuditGate gate = RealStS2SelectedTargetSemanticAuditGate.SelectedEvidenceBindingAndOfflineReady;
        try
        {
            Reset();
            cancellationToken.ThrowIfCancellationRequested();
            progress?.Report(new RealStS2SelectedTargetSemanticAuditProgress(
                gate, 0, 0, null,
                "Re-proving OfflineReady and binding the physical Step-29 selected token/offset/body fingerprint to the exact receipt-backed ARM64 sts2.dll…"));

            IProgress<SteamOfflineInstallProgress>? offlineProgress = progress is null
                ? null
                : new CallbackProgress<SteamOfflineInstallProgress>(value =>
                    progress.Report(new RealStS2SelectedTargetSemanticAuditProgress(
                        gate,
                        value.CompletedFiles,
                        value.TotalFiles,
                        value.CurrentFile,
                        $"OfflineReady precondition — {value.Message} ({value.CompletedBytes:N0}/{value.TotalBytes:N0} bytes)")));

            var offline = await _offlineInspection.RunAsync(offlineProgress, cancellationToken).ConfigureAwait(false);
            if (offline.Outcome == SteamOfflineInstallOutcome.Cancelled)
                throw new OperationCanceledException("Step 30 OfflineReady precondition was cancelled.", cancellationToken);
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
                throw new InvalidDataException("Primary sts2.dll does not match the physically closed Step-29 source identity.");

            var loadedBefore = FindLoadedAssemblyIdentities("sts2");
            if (loadedBefore.Count != 0)
                throw new InvalidDataException("A sts2 assembly identity is already resident in the CLR. Force-quit before Step 30.");

            using var resolver = new RejectingAssemblyResolver();
            using var module = ReadModuleDeferred(path, resolver);
            if (module.Assembly?.Name is null)
                throw new InvalidDataException("Primary sts2.dll does not contain a managed assembly manifest.");
            if (!module.Assembly.Name.FullName.Equals(_expected.AssemblyIdentity, StringComparison.Ordinal) || module.Mvid != _expected.Mvid)
                throw new InvalidDataException("Primary sts2.dll assembly identity/MVID does not match the physical Step-29 evidence.");

            var method = FindMethodByToken(module, _expected.MethodToken);
            ValidateSelectedMethodAndInstruction(method, _expected);
            var bodyHash = ComputeMethodBodyFingerprint(method);
            if (!bodyHash.Equals(_expected.MethodBodySha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException($"Selected method body fingerprint drift: expected {_expected.MethodBodySha256}, observed {bodyHash}.");
            if (resolver.Requests.Count != 0)
                throw new InvalidDataException("Step-30 evidence binding unexpectedly requested Cecil dependency resolution: " + string.Join(" | ", resolver.Requests));
            if (FindLoadedAssemblyIdentities("sts2").Count != 0)
                throw new InvalidDataException("Step-30 Cecil evidence binding unexpectedly caused sts2.dll to enter the CLR.");

            _source = new SourceSnapshot(offline, managedRoot, relative, path, bytes, sha1, sha256, module.Assembly.Name.FullName, module.Mvid, bodyHash);
            return Pass(gate,
                "PHYSICAL STEP-29 SELECTED EVIDENCE REBOUND TO THE EXACT RECEIPT-BACKED SOURCE WITHOUT CLR ADMISSION.\n" +
                $"OfflineReady precondition: PASS ({offline.VerifiedFiles:N0}/{offline.PlannedFiles:N0} files)\n" +
                $"Primary: {relative}\n" +
                $"Assembly identity: {_source.AssemblyIdentity}\n" +
                $"Module MVID: {_source.Mvid:D}\n" +
                $"Receipt SHA-1: {_source.Sha1}\n" +
                $"Source SHA-256: {_source.Sha256}\n" +
                $"Source bytes: {_source.Bytes:N0}\n" +
                $"Selected source method: {_expected.SourceMethod}\n" +
                $"Selected method token: 0x{_expected.MethodToken:X8}\n" +
                $"Selected IL site: IL_{_expected.IlOffset:X4} / {_expected.OpCode} -> [{_expected.TargetScope}] {_expected.TargetMember}\n" +
                $"Selected method-body fingerprint SHA-256: {bodyHash}\n" +
                "Cecil reading mode: Deferred\n" +
                "Cecil dependency resolution requests: 0\n" +
                "sts2 CLR-loaded before/after Gate A: NO / NO\n" +
                "Trusted Step 12 managed install modified: NO");
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex) { return Fail(gate, ex); }
    }

    public RealStS2SelectedTargetSemanticAuditGateResult RunExactSemanticContextAudit()
    {
        const RealStS2SelectedTargetSemanticAuditGate gate = RealStS2SelectedTargetSemanticAuditGate.ExactSemanticContextAudit;
        try
        {
            var source = RequireSource();
            using var resolver = new RejectingAssemblyResolver();
            using var module = ReadModuleDeferred(source.PrimaryPath, resolver);
            var method = FindMethodByToken(module, _expected.MethodToken);
            ValidateSelectedMethodAndInstruction(method, _expected);
            var bodyHash = ComputeMethodBodyFingerprint(method);
            if (!bodyHash.Equals(_expected.MethodBodySha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Selected method body changed between Step-30 gates.");

            var instructions = method.Body.Instructions;
            var selectedIndex = FindInstructionIndex(instructions, _expected.IlOffset);
            if (selectedIndex < 0)
                throw new InvalidDataException("Selected IL offset is no longer present in the exact method body.");
            var selected = instructions[selectedIndex];
            if (selected.Operand is not MethodReference target)
                throw new InvalidDataException("Selected IL instruction no longer carries a method-reference operand.");

            const int radius = 14;
            var first = Math.Max(0, selectedIndex - radius);
            var last = Math.Min(instructions.Count - 1, selectedIndex + radius);
            var window = instructions.Skip(first).Take(last - first + 1).Select(FormatInstruction).ToArray();
            var incoming = EnumerateBranchSources(method, selected).Select(FormatInstruction).ToArray();
            var handlers = method.Body.ExceptionHandlers.Where(handler => CoversInstruction(handler, selected)).Select(FormatExceptionHandler).ToArray();
            var stringLiterals = instructions.Skip(first).Take(last - first + 1)
                .Where(instruction => instruction.OpCode.Code == Code.Ldstr && instruction.Operand is string)
                .Select(instruction => (string)instruction.Operand)
                .ToArray();

            var harmonySites = CountMethodReferences(method, reference =>
                GetTargetScopeName(reference).Equals("0Harmony", StringComparison.OrdinalIgnoreCase) ||
                reference.DeclaringType.FullName.StartsWith("HarmonyLib.", StringComparison.Ordinal));
            var dynamicLoadSites = CountMethodReferences(method, reference =>
                reference.DeclaringType.FullName.StartsWith("System.Runtime.Loader.AssemblyLoadContext", StringComparison.Ordinal) ||
                (reference.DeclaringType.FullName == "System.Reflection.Assembly" && reference.Name.StartsWith("Load", StringComparison.Ordinal)));

            var isModScoped = method.DeclaringType.FullName == _expected.SourceType &&
                              method.Name == "TryLoadMod" &&
                              method.Parameters.Count == 1 &&
                              method.Parameters[0].ParameterType.FullName == "MegaCrit.Sts2.Core.Modding.Mod";
            var callShape = target.HasThis && target.Parameters.Count == 1 && target.Parameters[0].ParameterType.FullName == "System.Reflection.Assembly";
            if (!callShape)
                throw new InvalidDataException("Selected Harmony.PatchAll call shape drifted from instance + Assembly argument.");
            if (resolver.Requests.Count != 0)
                throw new InvalidDataException("Step-30 semantic context audit unexpectedly requested Cecil dependency resolution: " + string.Join(" | ", resolver.Requests));

            _context = new SemanticContextSnapshot(
                isModScoped,
                method.IsStatic,
                method.Body.MaxStackSize,
                method.Body.InitLocals,
                method.Body.Variables.Count,
                method.Body.ExceptionHandlers.Count,
                harmonySites,
                dynamicLoadSites,
                incoming.Length,
                handlers.Length,
                window,
                incoming,
                handlers,
                stringLiterals);

            return Pass(gate,
                "EXACT SELECTED METHOD SEMANTICS INSPECTED WITHOUT RESOLUTION, REWRITE, OR EXECUTION.\n" +
                $"Declaring type: {method.DeclaringType.FullName}\n" +
                $"Method: {method.FullName}\n" +
                $"Method token/body fingerprint: 0x{method.MetadataToken.ToUInt32():X8} / {bodyHash}\n" +
                $"Method static: {(method.IsStatic ? "YES" : "NO")}\n" +
                $"Method body: instructions={instructions.Count:N0}; max-stack={method.Body.MaxStackSize}; init-locals={(method.Body.InitLocals ? "YES" : "NO")}; locals={method.Body.Variables.Count}; exception-handlers={method.Body.ExceptionHandlers.Count}\n" +
                $"Selected call: IL_{selected.Offset:X4} {selected.OpCode.Code} -> [{GetTargetScopeName(target)}] {target.FullName}\n" +
                "Selected call stack contract: instance Harmony + one System.Reflection.Assembly argument -> void\n" +
                $"Structurally scoped to ModManager.TryLoadMod(Mod): {(isModScoped ? "YES" : "NO")}\n" +
                $"Harmony method-reference sites in selected method: {harmonySites:N0}\n" +
                $"Dynamic assembly-load sites in selected method: {dynamicLoadSites:N0}\n" +
                $"Branches targeting selected instruction: {incoming.Length:N0}\n" +
                $"Exception handlers covering selected instruction: {handlers.Length:N0}\n" +
                "Exact IL context (14 instructions before/after selected site, bounded by method edges):\n" + string.Join("\n", window) + "\n" +
                "Branch sources targeting selected instruction:\n" + (incoming.Length == 0 ? "• none" : string.Join("\n", incoming.Select(value => "• " + value))) + "\n" +
                "Exception regions covering selected instruction:\n" + (handlers.Length == 0 ? "• none" : string.Join("\n", handlers.Select(value => "• " + value))) + "\n" +
                "String literals in exact IL context:\n" + (stringLiterals.Length == 0 ? "• none" : string.Join("\n", stringLiterals.Select(value => "• " + value))) + "\n" +
                "Cecil dependency resolution requests: 0\n" +
                "Real StS2 CLR load/invocation: NO\n" +
                "Cecil writes: 0");
        }
        catch (Exception ex) { return Fail(gate, ex); }
    }

    public RealStS2SelectedTargetSemanticAuditGateResult RunDeterministicDisposition()
    {
        const RealStS2SelectedTargetSemanticAuditGate gate = RealStS2SelectedTargetSemanticAuditGate.DeterministicDisposition;
        try
        {
            var context = RequireContext();
            if (!context.IsModScoped)
                throw new InvalidDataException("The physically selected Harmony.PatchAll site is no longer structurally confined to ModManager.TryLoadMod(Mod); do not infer the planned disposition.");

            const string status = "DEFER — MOD/HARMONY COMPATIBILITY PATH; NO BASE-GAME REWRITE AUTHORIZED";
            _disposition = new DispositionSnapshot(status, RewriteAuthorized: false);
            return Pass(gate,
                "SELECTED STEP-29 HARMONY SITE IS DEFERRED FROM THE BASE-GAME TRANSFORMATION FRONTIER.\n" +
                $"Disposition: {status}\n" +
                "Evidence basis: the exact selected instruction remains inside MegaCrit.Sts2.Core.Modding.ModManager::TryLoadMod(MegaCrit.Sts2.Core.Modding.Mod), and its target is HarmonyLib.Harmony::PatchAll(System.Reflection.Assembly).\n" +
                "Project policy: Workshop/Harmony-mod compatibility is a later separate design problem and must not block getting the base game running; Step 27 already closed runtime Harmony method replacement negatively on this iOS host.\n" +
                "Predeclared behavior change for this selected site: NONE. This candidate explicitly does not authorize replacing, deleting, bypassing, or otherwise rewriting the PatchAll call.\n" +
                "Runtime reachability claim: NONE — structural IL location does not prove this method executes during base-game startup.\n" +
                "Next frontier after physical closure: inspect the highest-priority non-mod compatibility site from the Step-29 evidence (the PrepareMethod/OneTimeInitialization::PrewarmJit family is the first recorded non-mod family) under its own exact semantic audit before any Cecil write.\n" +
                "Real StS2 bytes changed: NO\n" +
                "Real StS2 member invoked: NO");
        }
        catch (Exception ex) { return Fail(gate, ex); }
    }

    public async Task<RealStS2SelectedTargetSemanticAuditGateResult> RunFinalIsolationAuditAsync(
        IProgress<RealStS2SelectedTargetSemanticAuditProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        const RealStS2SelectedTargetSemanticAuditGate gate = RealStS2SelectedTargetSemanticAuditGate.FinalIsolationAudit;
        try
        {
            var source = RequireSource();
            var context = RequireContext();
            var disposition = RequireDisposition();
            progress?.Report(new RealStS2SelectedTargetSemanticAuditProgress(
                gate, 0, 0, source.PrimaryRelativePath,
                "Re-hashing primary sts2.dll and re-proving OfflineReady after the selected-method semantic audit…"));

            var bytes = new FileInfo(source.PrimaryPath).Length;
            var sha1 = await ComputeHashHexAsync(source.PrimaryPath, SHA1.Create(), cancellationToken).ConfigureAwait(false);
            var sha256 = await ComputeHashHexAsync(source.PrimaryPath, SHA256.Create(), cancellationToken).ConfigureAwait(false);
            if (bytes != source.Bytes || !sha1.Equals(source.Sha1, StringComparison.OrdinalIgnoreCase) || !sha256.Equals(source.Sha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Primary sts2.dll changed across the Step-30 read-only audit.");

            IProgress<SteamOfflineInstallProgress>? offlineProgress = progress is null
                ? null
                : new CallbackProgress<SteamOfflineInstallProgress>(value =>
                    progress.Report(new RealStS2SelectedTargetSemanticAuditProgress(
                        gate,
                        value.CompletedFiles,
                        value.TotalFiles,
                        value.CurrentFile,
                        $"Post-audit OfflineReady — {value.Message} ({value.CompletedBytes:N0}/{value.TotalBytes:N0} bytes)")));
            var offline = await _offlineInspection.RunAsync(offlineProgress, cancellationToken).ConfigureAwait(false);
            if (!offline.Success || !offline.ExactManagedTreeVerified)
                throw new InvalidDataException(offline.Error ?? "Post-audit OfflineReady verification failed.");
            if (FindLoadedAssemblyIdentities("sts2").Count != 0)
                throw new InvalidDataException("Step 30 unexpectedly caused a sts2 assembly identity to become CLR-resident.");

            return Pass(gate,
                "STEP 30.0 FINAL READ-ONLY ISOLATION AUDIT PASSED.\n" +
                $"Primary receipt SHA-1 unchanged: {sha1}\n" +
                $"Primary SHA-256 unchanged: {sha256}\n" +
                $"Primary bytes unchanged: {bytes:N0}\n" +
                $"Post-audit OfflineReady: PASS ({offline.VerifiedFiles:N0}/{offline.PlannedFiles:N0} files)\n" +
                "Trusted Step 12 managed install unchanged: YES\n" +
                "sts2 assembly/type/member CLR load or invocation by Step 30: NO\n" +
                "Cecil dependency resolution requests across audit: 0\n" +
                $"Selected method remained mod-scoped: {(context.IsModScoped ? "YES" : "NO")}\n" +
                $"Disposition: {disposition.Status}\n" +
                $"Real-game rewrite authorized by Step 30: {(disposition.RewriteAuthorized ? "YES" : "NO")}\n" +
                "Cecil writes performed by Step 30: 0\n" +
                "Harmony/MonoMod runtime patching invoked by Step 30: NO\n" +
                "Godot/game startup or native game loading attempted by Step 30: NO");
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex) { return Fail(gate, ex); }
    }

    private SourceSnapshot RequireSource() => _source ?? throw new InvalidOperationException("Step 30 Gate A must pass before later gates run.");
    private SemanticContextSnapshot RequireContext() => _context ?? throw new InvalidOperationException("Step 30 Gate B must pass before later gates run.");
    private DispositionSnapshot RequireDisposition() => _disposition ?? throw new InvalidOperationException("Step 30 Gate C must pass before the final isolation audit.");

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

    private static void ValidateSelectedMethodAndInstruction(MethodDefinition method, SelectedTargetEvidence expected)
    {
        if (!method.HasBody)
            throw new InvalidDataException("Physically selected Step-29 method no longer has managed IL.");
        if (method.DeclaringType.FullName != expected.SourceType || method.FullName != expected.SourceMethod)
            throw new InvalidDataException($"Selected method token identity drift: observed {method.FullName}.");
        var selected = method.Body.Instructions.Where(instruction => instruction.Offset == expected.IlOffset).ToArray();
        if (selected.Length != 1)
            throw new InvalidDataException($"Expected exactly one instruction at IL_{expected.IlOffset:X4}, found {selected.Length}.");
        if (!selected[0].OpCode.Code.ToString().Equals(expected.OpCode, StringComparison.Ordinal) || selected[0].Operand is not MethodReference target)
            throw new InvalidDataException("Selected IL opcode/operand shape drifted from the Step-29 evidence.");
        if (!GetTargetScopeName(target).Equals(expected.TargetScope, StringComparison.Ordinal) || target.FullName != expected.TargetMember)
            throw new InvalidDataException($"Selected IL target drift: observed [{GetTargetScopeName(target)}] {target.FullName}.");
    }

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

    private static RealStS2SelectedTargetSemanticAuditGateResult Pass(RealStS2SelectedTargetSemanticAuditGate gate, string detail) => new(gate, true, detail);
    private static RealStS2SelectedTargetSemanticAuditGateResult Fail(RealStS2SelectedTargetSemanticAuditGate gate, Exception ex) => new(gate, false, $"Stage failed with {ex.GetType().Name}: {ex.Message}\n{ex}");

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

    internal sealed record SelectedTargetEvidence(
        string SourceSha1,
        string SourceSha256,
        long SourceBytes,
        string AssemblyIdentity,
        Guid Mvid,
        string SourceType,
        string SourceMethod,
        uint MethodToken,
        int IlOffset,
        string OpCode,
        string TargetScope,
        string TargetMember,
        string MethodBodySha256);

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

    private sealed record SemanticContextSnapshot(
        bool IsModScoped,
        bool IsStatic,
        int MaxStack,
        bool InitLocals,
        int LocalCount,
        int ExceptionHandlerCount,
        int HarmonyReferenceCount,
        int DynamicLoadReferenceCount,
        int BranchesToSelected,
        int CoveringExceptionHandlers,
        string[] IlWindow,
        string[] IncomingBranches,
        string[] CoveringHandlers,
        string[] StringLiterals);

    private sealed record DispositionSnapshot(string Status, bool RewriteAuthorized);
}
