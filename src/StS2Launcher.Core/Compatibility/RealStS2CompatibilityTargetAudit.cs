using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace StS2Launcher.Core;

/// <summary>
/// Step 29.0 boundary. Step 28 physically proved that deterministic Cecil semantic
/// transformation and transformed-only interpreted execution work together. Step 29.0
/// deliberately does not guess a real-game patch from broad historical categories: it
/// re-audits the current receipt-backed macOS arm64 sts2.dll, fingerprints exact concrete
/// IL call sites that are relevant to the post-Step-28 iOS compatibility frontier, and
/// deterministically selects at most one audit candidate for a later transformation step.
///
/// This boundary is read-only. It never writes the managed install, never resolves Cecil
/// dependencies, never CLR-loads sts2.dll, and never invokes a real StS2 member.
/// </summary>
public sealed class RealStS2CompatibilityTargetAudit
{
    private readonly string _launcherDataRoot;
    private readonly SteamOfflineInstallInspection _offlineInspection;
    private SourceSnapshot? _source;
    private AuditSnapshot? _audit;
    private CandidateSelectionSnapshot? _selection;

    public RealStS2CompatibilityTargetAudit(string launcherDataRoot)
    {
        if (string.IsNullOrWhiteSpace(launcherDataRoot))
            throw new ArgumentException("Launcher data root is required.", nameof(launcherDataRoot));

        _launcherDataRoot = Path.GetFullPath(launcherDataRoot);
        _offlineInspection = new SteamOfflineInstallInspection(_launcherDataRoot);
    }

    public void Reset()
    {
        _source = null;
        _audit = null;
        _selection = null;
    }

    public async Task<RealStS2CompatibilityTargetAuditGateResult> RunSourceAdmissionAndOfflineReadyAsync(
        IProgress<RealStS2CompatibilityTargetAuditProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        const RealStS2CompatibilityTargetAuditGate gate = RealStS2CompatibilityTargetAuditGate.SourceAdmissionAndOfflineReady;
        try
        {
            Reset();
            cancellationToken.ThrowIfCancellationRequested();
            progress?.Report(new RealStS2CompatibilityTargetAuditProgress(
                gate,
                0,
                0,
                null,
                "Re-proving OfflineReady before admitting the exact receipt-backed macOS arm64 sts2.dll as Cecil metadata only…"));

            IProgress<SteamOfflineInstallProgress>? offlineProgress = progress is null
                ? null
                : new CallbackProgress<SteamOfflineInstallProgress>(value =>
                    progress.Report(new RealStS2CompatibilityTargetAuditProgress(
                        gate,
                        value.CompletedFiles,
                        value.TotalFiles,
                        value.CurrentFile,
                        $"OfflineReady precondition — {value.Message} ({value.CompletedBytes:N0}/{value.TotalBytes:N0} bytes)")));

            var offline = await _offlineInspection.RunAsync(offlineProgress, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            if (offline.Outcome == SteamOfflineInstallOutcome.Cancelled)
                throw new OperationCanceledException("Step 29 OfflineReady precondition was cancelled.", cancellationToken);
            if (!offline.Success || string.IsNullOrWhiteSpace(offline.ManagedInstallRelativePath))
                throw new InvalidDataException(offline.Error ?? $"Managed install is not OfflineReady ({offline.State}/{offline.Outcome}).");

            var managedRoot = ResolveChildPath(_launcherDataRoot, offline.ManagedInstallRelativePath);
            var receiptPath = Path.Combine(managedRoot, SteamManagedInstallReceipt.FileName);
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

            var primaryMatches = receipt.Files
                .Where(file => IsPrimaryArm64StS2Path(file.RelativePath))
                .ToArray();
            if (primaryMatches.Length != 1)
                throw new InvalidDataException($"Expected exactly one receipt-backed macOS arm64 sts2.dll, found {primaryMatches.Length}.");

            var primary = primaryMatches[0];
            var normalizedRelative = primary.RelativePath.Replace('\\', '/');
            var primaryPath = ResolveChildPath(managedRoot, normalizedRelative);
            if (!File.Exists(primaryPath))
                throw new FileNotFoundException("The receipt-backed primary sts2.dll is missing.", primaryPath);

            var actualLength = new FileInfo(primaryPath).Length;
            if (actualLength != primary.Length)
                throw new InvalidDataException($"Primary sts2.dll length drift: receipt={primary.Length:N0}, actual={actualLength:N0}.");

            var sha1 = await ComputeHashHexAsync(primaryPath, SHA1.Create(), cancellationToken).ConfigureAwait(false);
            if (!sha1.Equals(primary.Sha1Hex, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Primary sts2.dll no longer matches its trusted Step 12 receipt SHA-1.");
            var sha256 = await ComputeHashHexAsync(primaryPath, SHA256.Create(), cancellationToken).ConfigureAwait(false);

            var loadedBefore = FindLoadedAssemblyIdentities("sts2");
            if (loadedBefore.Count != 0)
                throw new InvalidDataException("A sts2 assembly identity is already resident in the CLR. Force-quit before Step 29 so metadata-only admission can be attributed to a fresh process.");

            using var resolver = new RejectingAssemblyResolver();
            using var module = ReadModuleDeferred(primaryPath, resolver);
            if (module.Assembly?.Name is null)
                throw new InvalidDataException("Primary sts2.dll does not contain a managed assembly manifest.");
            if (!string.Equals(module.Assembly.Name.Name, "sts2", StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException($"Expected primary assembly simple name 'sts2', observed '{module.Assembly.Name.Name}'.");
            if (resolver.Requests.Count != 0)
                throw new InvalidDataException("Metadata-only source admission unexpectedly requested Cecil dependency resolution: " + string.Join(" | ", resolver.Requests));

            var loadedAfter = FindLoadedAssemblyIdentities("sts2");
            if (loadedAfter.Count != 0)
                throw new InvalidDataException("Cecil metadata admission unexpectedly caused sts2.dll to enter the CLR.");

            _source = new SourceSnapshot(
                offline,
                managedRoot,
                normalizedRelative,
                primaryPath,
                primary.Length,
                sha1,
                sha256,
                module.Assembly.Name.FullName,
                module.Mvid,
                module.RuntimeVersion,
                ResolverRequests: 0);

            return Pass(
                gate,
                "RECEIPT-BACKED REAL STS2 SOURCE ADMITTED AS METADATA ONLY; NO TRANSFORMATION OR CLR LOAD OCCURRED.\n" +
                $"OfflineReady precondition: PASS ({offline.VerifiedFiles:N0}/{offline.PlannedFiles:N0} files)\n" +
                $"Primary: {normalizedRelative}\n" +
                $"Assembly identity: {_source.AssemblyIdentity}\n" +
                $"Module MVID: {_source.Mvid:D}\n" +
                $"Runtime metadata version: {_source.RuntimeVersion}\n" +
                $"Receipt SHA-1: {_source.Sha1}\n" +
                $"Source SHA-256: {_source.Sha256}\n" +
                $"Source bytes: {_source.Bytes:N0}\n" +
                "Cecil reading mode: Deferred\n" +
                "Cecil dependency resolution requests: 0\n" +
                "sts2 CLR-loaded before/after Gate A: NO / NO\n" +
                "Trusted Step 12 managed install modified: NO\n" +
                "Real StS2 member invoked: NO");
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

    public RealStS2CompatibilityTargetAuditGateResult RunExactRiskCallSiteAudit()
    {
        const RealStS2CompatibilityTargetAuditGate gate = RealStS2CompatibilityTargetAuditGate.ExactRiskCallSiteAudit;
        try
        {
            var source = RequireSource();
            var resolver = new RejectingAssemblyResolver();
            using (resolver)
            using (var module = ReadModuleDeferred(source.PrimaryPath, resolver))
            {
                var candidates = new List<RiskCallSite>();
                var allCategoryCounts = new Dictionary<string, int>(StringComparer.Ordinal);
                var subsystemCounts = new Dictionary<string, int>(StringComparer.Ordinal);
                var expressionCompileSites = 0;
                var methodsWithBodies = 0;
                var instructionsInspected = 0;
                var methodReferenceSites = 0;

                foreach (var type in EnumerateTypes(module.Types))
                {
                    foreach (var method in type.Methods)
                    {
                        if (!method.HasBody)
                            continue;

                        methodsWithBodies++;
                        var bodyFingerprint = ComputeMethodBodyFingerprint(method);
                        foreach (var instruction in method.Body.Instructions)
                        {
                            instructionsInspected++;
                            if (instruction.OpCode.Code == Code.Calli)
                            {
                                AddCandidate(candidates, allCategoryCounts, new RiskCallSite(
                                    "IndirectCalli",
                                    PriorityForCategory("IndirectCalli"),
                                    type.FullName,
                                    method.FullName,
                                    method.MetadataToken.ToUInt32(),
                                    instruction.Offset,
                                    instruction.OpCode.Code.ToString(),
                                    FormatOperand(instruction.Operand),
                                    "<indirect>",
                                    bodyFingerprint));
                                continue;
                            }

                            if (!IsMethodReferenceInstruction(instruction.OpCode.Code) || instruction.Operand is not MethodReference target)
                                continue;

                            methodReferenceSites++;
                            var subsystem = ClassifySubsystem(target);
                            if (subsystem is not null)
                                Increment(subsystemCounts, subsystem);

                            var category = ClassifyCompatibilityRisk(target);
                            if (category is null)
                                continue;
                            if (category == "ExpressionCompile")
                            {
                                expressionCompileSites++;
                                Increment(allCategoryCounts, "ExpressionCompile(Step19Closed)");
                                continue;
                            }

                            AddCandidate(candidates, allCategoryCounts, new RiskCallSite(
                                category,
                                PriorityForCategory(category),
                                type.FullName,
                                method.FullName,
                                method.MetadataToken.ToUInt32(),
                                instruction.Offset,
                                instruction.OpCode.Code.ToString(),
                                target.FullName,
                                GetTargetScopeName(target),
                                bodyFingerprint));
                        }
                    }
                }

                if (resolver.Requests.Count != 0)
                    throw new InvalidDataException("Exact IL audit unexpectedly requested Cecil dependency resolution: " + string.Join(" | ", resolver.Requests));
                if (methodsWithBodies == 0 || instructionsInspected == 0)
                    throw new InvalidDataException("The primary sts2.dll exposed no managed IL bodies for Step 29 auditing.");

                var ordered = candidates
                    .OrderBy(candidate => candidate.Priority)
                    .ThenBy(candidate => candidate.SourceMethod, StringComparer.Ordinal)
                    .ThenBy(candidate => candidate.IlOffset)
                    .ThenBy(candidate => candidate.Target, StringComparer.Ordinal)
                    .ToArray();

                _audit = new AuditSnapshot(
                    methodsWithBodies,
                    instructionsInspected,
                    methodReferenceSites,
                    expressionCompileSites,
                    ordered,
                    new Dictionary<string, int>(allCategoryCounts, StringComparer.Ordinal),
                    new Dictionary<string, int>(subsystemCounts, StringComparer.Ordinal),
                    ResolverRequests: 0);

                return Pass(
                    gate,
                    "EXACT REAL-STS2 IL COMPATIBILITY SURFACE AUDITED WITHOUT RESOLUTION, REWRITE, OR EXECUTION.\n" +
                    $"Primary methods with IL bodies: {methodsWithBodies:N0}\n" +
                    $"IL instructions inspected: {instructionsInspected:N0}\n" +
                    $"Concrete method-reference sites: {methodReferenceSites:N0}\n" +
                    $"Step-29 transformation-candidate sites: {ordered.Length:N0}\n" +
                    $"Candidate categories: {FormatCounts(allCategoryCounts)}\n" +
                    $"Expression.Compile sites excluded from Step-29 candidacy by physically closed Step 19 policy: {expressionCompileSites:N0}\n" +
                    $"Primary subsystem-call counts: {FormatCounts(subsystemCounts)}\n" +
                    "Highest-priority exact candidate samples:\n" + FormatCandidateSamples(ordered, 16) + "\n\n" +
                    "Selection policy: Harmony runtime-patch calls first, then MonoMod runtime detours, Reflection.Emit/PrepareMethod/dynamic assembly loads, platform/native API surfaces, then indirect calli.\n" +
                    "Evidence policy: a concrete IL site proves code exists; it does not by itself prove runtime reachability or authorize a semantic rewrite.\n" +
                    "Cecil dependency resolution requests: 0\n" +
                    "Real StS2 assembly CLR-loaded/executed: NO\n" +
                    "Trusted Step 12 managed install modified: NO");
            }
        }
        catch (Exception ex)
        {
            return Fail(gate, ex);
        }
    }

    public RealStS2CompatibilityTargetAuditGateResult RunDeterministicCandidateSelection()
    {
        const RealStS2CompatibilityTargetAuditGate gate = RealStS2CompatibilityTargetAuditGate.DeterministicCandidateSelection;
        try
        {
            var audit = RequireAudit();
            if (audit.Candidates.Length == 0)
            {
                _selection = new CandidateSelectionSnapshot(null, "NO DIRECT PRIMARY TARGET");
                return Pass(
                    gate,
                    "DETERMINISTIC SELECTION COMPLETED: NO DIRECT PRIMARY sts2.dll TRANSFORMATION CANDIDATE EXISTS UNDER THE STEP-29 POLICY.\n" +
                    "Selection status: NO DIRECT PRIMARY TARGET\n" +
                    "No rewrite is synthesized. A later candidate must broaden evidence deliberately (for example to a dependency-owned site or a separately gated integration frontier) rather than inventing a target.\n" +
                    "Real StS2 bytes changed: NO\n" +
                    "Real StS2 member invoked: NO");
            }

            var selected = audit.Candidates[0];
            _selection = new CandidateSelectionSnapshot(selected, "AUDIT CANDIDATE SELECTED");
            return Pass(
                gate,
                "ONE EXACT REAL-STS2 AUDIT CANDIDATE SELECTED DETERMINISTICALLY; THIS BUILD DOES NOT TRANSFORM IT.\n" +
                "Selection status: AUDIT CANDIDATE SELECTED\n" +
                $"Priority: {selected.Priority}\n" +
                $"Category: {selected.Category}\n" +
                $"Source type: {selected.SourceType}\n" +
                $"Source method: {selected.SourceMethod}\n" +
                $"Source method token: 0x{selected.MethodToken:X8}\n" +
                $"IL offset/opcode: IL_{selected.IlOffset:X4} / {selected.OpCode}\n" +
                $"Target scope: {selected.TargetScope}\n" +
                $"Target member: {selected.Target}\n" +
                $"Source method-body fingerprint SHA-256: {selected.MethodBodySha256}\n" +
                "Authorization: AUDIT ONLY — the next candidate must inspect the selected method's exact surrounding semantics and predeclare the intended behavior change before any Cecil write.\n" +
                "Real StS2 bytes changed: NO\n" +
                "Real StS2 member invoked: NO");
        }
        catch (Exception ex)
        {
            return Fail(gate, ex);
        }
    }

    public async Task<RealStS2CompatibilityTargetAuditGateResult> RunFinalIsolationAuditAsync(
        IProgress<RealStS2CompatibilityTargetAuditProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        const RealStS2CompatibilityTargetAuditGate gate = RealStS2CompatibilityTargetAuditGate.FinalIsolationAudit;
        try
        {
            var source = RequireSource();
            var audit = RequireAudit();
            var selection = RequireSelection();

            progress?.Report(new RealStS2CompatibilityTargetAuditProgress(
                gate,
                0,
                0,
                source.PrimaryRelativePath,
                "Re-hashing primary sts2.dll and re-proving OfflineReady after the read-only audit…"));

            var currentLength = new FileInfo(source.PrimaryPath).Length;
            var currentSha1 = await ComputeHashHexAsync(source.PrimaryPath, SHA1.Create(), cancellationToken).ConfigureAwait(false);
            var currentSha256 = await ComputeHashHexAsync(source.PrimaryPath, SHA256.Create(), cancellationToken).ConfigureAwait(false);
            if (currentLength != source.Bytes ||
                !currentSha1.Equals(source.Sha1, StringComparison.OrdinalIgnoreCase) ||
                !currentSha256.Equals(source.Sha256, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException("Primary sts2.dll changed across the Step-29 read-only audit.");
            }

            IProgress<SteamOfflineInstallProgress>? offlineProgress = progress is null
                ? null
                : new CallbackProgress<SteamOfflineInstallProgress>(value =>
                    progress.Report(new RealStS2CompatibilityTargetAuditProgress(
                        gate,
                        value.CompletedFiles,
                        value.TotalFiles,
                        value.CurrentFile,
                        $"Post-audit OfflineReady — {value.Message} ({value.CompletedBytes:N0}/{value.TotalBytes:N0} bytes)")));
            var offline = await _offlineInspection.RunAsync(offlineProgress, cancellationToken).ConfigureAwait(false);
            if (!offline.Success || !offline.ExactManagedTreeVerified)
                throw new InvalidDataException(offline.Error ?? "Post-audit OfflineReady verification failed.");

            var loaded = FindLoadedAssemblyIdentities("sts2");
            if (loaded.Count != 0)
                throw new InvalidDataException("Step 29 unexpectedly caused a sts2 assembly identity to become CLR-resident.");

            return Pass(
                gate,
                "STEP 29.0 FINAL READ-ONLY ISOLATION AUDIT PASSED.\n" +
                $"Primary receipt SHA-1 unchanged: {currentSha1}\n" +
                $"Primary SHA-256 unchanged: {currentSha256}\n" +
                $"Primary bytes unchanged: {currentLength:N0}\n" +
                $"Post-audit OfflineReady: PASS ({offline.VerifiedFiles:N0}/{offline.PlannedFiles:N0} files)\n" +
                "Trusted Step 12 managed install unchanged: YES\n" +
                "sts2 assembly/type/member CLR load or invocation by Step 29: NO\n" +
                $"Cecil dependency resolution requests across audit: {source.ResolverRequests + audit.ResolverRequests}\n" +
                $"Selection status: {selection.Status}\n" +
                (selection.Selected is null
                    ? "Selected exact candidate: none\n"
                    : $"Selected exact candidate: {selection.Selected.Category} — {selection.Selected.SourceMethod} @ IL_{selection.Selected.IlOffset:X4} -> {selection.Selected.Target}\n") +
                "Cecil writes performed by Step 29: 0\n" +
                "Harmony/MonoMod runtime patching invoked by Step 29: NO\n" +
                "Godot/game startup or native game loading attempted by Step 29: NO");
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

    internal static string? ClassifyCompatibilityRisk(MethodReference target)
    {
        var type = target.DeclaringType.FullName;
        var scope = GetTargetScopeName(target);
        var name = target.Name;

        if (type.StartsWith("System.Linq.Expressions.", StringComparison.Ordinal) && name.StartsWith("Compile", StringComparison.Ordinal))
            return "ExpressionCompile";
        if ((type.StartsWith("HarmonyLib.", StringComparison.Ordinal) || scope.Equals("0Harmony", StringComparison.OrdinalIgnoreCase)) &&
            name is "Patch" or "PatchAll" or "PatchCategory" or "PatchAllUncategorized" or "CreateProcessor")
            return "HarmonyRuntimePatch";
        if (type.StartsWith("MonoMod.RuntimeDetour.", StringComparison.Ordinal) ||
            type.StartsWith("MonoMod.Cil.", StringComparison.Ordinal) ||
            type.Contains("DynamicMethodDefinition", StringComparison.Ordinal))
            return "MonoModRuntimeDetour";
        if (type.Contains("System.Reflection.Emit", StringComparison.Ordinal))
            return "ReflectionEmit";
        if (type.Equals("System.Runtime.CompilerServices.RuntimeHelpers", StringComparison.Ordinal) && name.Equals("PrepareMethod", StringComparison.Ordinal))
            return "PrepareMethod";
        if (type.Equals("System.Reflection.Assembly", StringComparison.Ordinal) && name.StartsWith("Load", StringComparison.Ordinal))
            return "DynamicAssemblyLoad";
        if (type.StartsWith("System.Runtime.Loader.AssemblyLoadContext", StringComparison.Ordinal) && name.Contains("Load", StringComparison.Ordinal))
            return "DynamicAssemblyLoad";
        if (type.StartsWith("System.Diagnostics.Process", StringComparison.Ordinal))
            return "System.Diagnostics.Process";
        if (type.StartsWith("Microsoft.Win32.Registry", StringComparison.Ordinal))
            return "Microsoft.Win32.Registry";
        if (type.StartsWith("System.Security.Principal.Windows", StringComparison.Ordinal))
            return "WindowsPrincipal";
        if (type.Equals("System.Runtime.InteropServices.NativeLibrary", StringComparison.Ordinal) && name.Equals("SetDllImportResolver", StringComparison.Ordinal))
            return "DllImportResolver";
        if (type.Equals("System.Runtime.InteropServices.NativeLibrary", StringComparison.Ordinal))
            return "NativeLibrary";
        if (type.Equals("System.Runtime.InteropServices.Marshal", StringComparison.Ordinal) &&
            name is "GetDelegateForFunctionPointer" or "GetFunctionPointerForDelegate")
            return "NativeFunctionPointer";

        return null;
    }

    internal static int PriorityForCategory(string category)
        => category switch
        {
            "HarmonyRuntimePatch" => 10,
            "MonoModRuntimeDetour" => 20,
            "ReflectionEmit" => 30,
            "PrepareMethod" => 40,
            "DynamicAssemblyLoad" => 50,
            "System.Diagnostics.Process" => 60,
            "Microsoft.Win32.Registry" => 70,
            "WindowsPrincipal" => 80,
            "DllImportResolver" => 90,
            "NativeLibrary" => 100,
            "NativeFunctionPointer" => 110,
            "IndirectCalli" => 120,
            _ => int.MaxValue,
        };

    private SourceSnapshot RequireSource()
        => _source ?? throw new InvalidOperationException("Step 29 Gate A must pass before later gates run.");

    private AuditSnapshot RequireAudit()
        => _audit ?? throw new InvalidOperationException("Step 29 Gate B must pass before later gates run.");

    private CandidateSelectionSnapshot RequireSelection()
        => _selection ?? throw new InvalidOperationException("Step 29 Gate C must pass before the final isolation audit.");

    private static ModuleDefinition ReadModuleDeferred(string path, IAssemblyResolver resolver)
        => ModuleDefinition.ReadModule(path, new ReaderParameters
        {
            ReadSymbols = false,
            ReadingMode = ReadingMode.Deferred,
            AssemblyResolver = resolver,
            MetadataResolver = new MetadataResolver(resolver),
        });

    private static void AddCandidate(
        List<RiskCallSite> candidates,
        Dictionary<string, int> categoryCounts,
        RiskCallSite candidate)
    {
        candidates.Add(candidate);
        Increment(categoryCounts, candidate.Category);
    }

    private static bool IsMethodReferenceInstruction(Code code)
        => code is Code.Call or Code.Callvirt or Code.Newobj or Code.Ldftn or Code.Ldvirtftn or Code.Jmp;

    private static string? ClassifySubsystem(MethodReference target)
    {
        var type = target.DeclaringType.FullName;
        var scope = GetTargetScopeName(target);
        if (scope.Equals("GodotSharp", StringComparison.OrdinalIgnoreCase) || type.StartsWith("Godot.", StringComparison.Ordinal))
            return "Godot/GodotSharp";
        if (scope.Contains("Steamworks", StringComparison.OrdinalIgnoreCase) || type.StartsWith("Steamworks.", StringComparison.Ordinal))
            return "Steamworks";
        if (scope.Contains("FMOD", StringComparison.OrdinalIgnoreCase) || type.StartsWith("FMOD.", StringComparison.Ordinal))
            return "FMOD";
        if (scope.Contains("Spine", StringComparison.OrdinalIgnoreCase) || type.StartsWith("Spine.", StringComparison.Ordinal))
            return "Spine";
        if (scope.Equals("0Harmony", StringComparison.OrdinalIgnoreCase) || type.StartsWith("HarmonyLib.", StringComparison.Ordinal))
            return "Harmony";
        if (scope.StartsWith("MonoMod", StringComparison.OrdinalIgnoreCase) || type.StartsWith("MonoMod.", StringComparison.Ordinal))
            return "MonoMod";
        return null;
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

    private static string ComputeMethodBodyFingerprint(MethodDefinition method)
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

    private static void Increment(Dictionary<string, int> values, string key)
    {
        if (values.TryGetValue(key, out var current))
            values[key] = checked(current + 1);
        else
            values[key] = 1;
    }

    private static string FormatCounts(IReadOnlyDictionary<string, int> values)
        => values.Count == 0
            ? "none"
            : string.Join(", ", values
                .OrderByDescending(pair => pair.Value)
                .ThenBy(pair => pair.Key, StringComparer.Ordinal)
                .Select(pair => $"{pair.Key}={pair.Value:N0}"));

    private static string FormatCandidateSamples(IReadOnlyList<RiskCallSite> candidates, int max)
        => candidates.Count == 0
            ? "• none"
            : string.Join("\n", candidates.Take(max).Select(candidate =>
                $"• P{candidate.Priority} {candidate.Category}: {candidate.SourceMethod} [0x{candidate.MethodToken:X8}] IL_{candidate.IlOffset:X4} {candidate.OpCode} -> [{candidate.TargetScope}] {candidate.Target}; body-sha256={candidate.MethodBodySha256}"));

    private static IReadOnlyList<string> FindLoadedAssemblyIdentities(string simpleName)
        => AppDomain.CurrentDomain.GetAssemblies()
            .Select(assembly => assembly.GetName())
            .Where(name => string.Equals(name.Name, simpleName, StringComparison.OrdinalIgnoreCase))
            .Select(name => name.FullName ?? name.Name ?? "<unknown>")
            .OrderBy(value => value, StringComparer.Ordinal)
            .ToArray();

    private static bool IsPrimaryArm64StS2Path(string path)
        => ("/" + path.Replace('\\', '/').TrimStart('/'))
            .EndsWith("/data_sts2_macos_arm64/sts2.dll", StringComparison.OrdinalIgnoreCase);

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
                !unique.Add(file.RelativePath.Replace('\\', '/')))
            {
                throw new InvalidDataException("The Step 12 receipt contains an invalid or duplicate file entry.");
            }
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
            await using var stream = new FileStream(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                256 * 1024,
                FileOptions.Asynchronous | FileOptions.SequentialScan);
            var hash = await algorithm.ComputeHashAsync(stream, cancellationToken).ConfigureAwait(false);
            return Convert.ToHexString(hash).ToLowerInvariant();
        }
    }

    private static RealStS2CompatibilityTargetAuditGateResult Pass(RealStS2CompatibilityTargetAuditGate gate, string detail)
        => new(gate, true, detail);

    private static RealStS2CompatibilityTargetAuditGateResult Fail(RealStS2CompatibilityTargetAuditGate gate, Exception ex)
        => new(gate, false, $"Stage failed with {ex.GetType().Name}: {ex.Message}\n{ex}");

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

        public AssemblyDefinition Resolve(AssemblyNameReference name)
        {
            _requests.Add(name.FullName);
            throw new AssemblyResolutionException(name);
        }

        public AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
        {
            _requests.Add(name.FullName);
            throw new AssemblyResolutionException(name);
        }

        public void Dispose() { }
    }

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
        string RuntimeVersion,
        int ResolverRequests);

    private sealed record AuditSnapshot(
        int MethodsWithBodies,
        int InstructionsInspected,
        int MethodReferenceSites,
        int ExpressionCompileSites,
        RiskCallSite[] Candidates,
        IReadOnlyDictionary<string, int> CategoryCounts,
        IReadOnlyDictionary<string, int> SubsystemCounts,
        int ResolverRequests);

    private sealed record CandidateSelectionSnapshot(
        RiskCallSite? Selected,
        string Status);

    private sealed record RiskCallSite(
        string Category,
        int Priority,
        string SourceType,
        string SourceMethod,
        uint MethodToken,
        int IlOffset,
        string OpCode,
        string Target,
        string TargetScope,
        string MethodBodySha256);
}
