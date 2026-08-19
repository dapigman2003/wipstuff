using System.Security.Cryptography;
using System.Text.Json;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace StS2Launcher.Core;

/// <summary>
/// Step 17 boundary. Turns Step 14's broad metadata/string indicators into
/// concrete, receipt-backed IL evidence for the macOS arm64 managed payload.
/// The analysis is intentionally read-only: no assembly resolution, no CLR load,
/// no game execution and no writes inside the managed installation.
/// </summary>
public sealed class CompatibilityCallSiteAnalysis
{
    private const int SampleLimit = 14;

    private readonly string _launcherDataRoot;
    private readonly SteamOfflineInstallInspection _offlineInspection;
    private ScopeSnapshot? _scope;
    private ScanSnapshot? _scan;

    public CompatibilityCallSiteAnalysis(string launcherDataRoot)
    {
        if (string.IsNullOrWhiteSpace(launcherDataRoot))
            throw new ArgumentException("Launcher data root is required.", nameof(launcherDataRoot));

        _launcherDataRoot = Path.GetFullPath(launcherDataRoot);
        _offlineInspection = new SteamOfflineInstallInspection(_launcherDataRoot);
    }

    public void Reset()
    {
        _scope = null;
        _scan = null;
    }

    public async Task<CompatibilityCallSiteGateResult> RunArm64ManagedScopeAsync(
        IProgress<CompatibilityCallSiteProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Reset();
            cancellationToken.ThrowIfCancellationRequested();
            progress?.Report(new CompatibilityCallSiteProgress(
                CompatibilityCallSiteGate.Arm64ManagedScope,
                0,
                0,
                null,
                "Re-proving OfflineReady before selecting the iOS-relevant macOS arm64 managed scope…"));

            IProgress<SteamOfflineInstallProgress>? offlineProgress = progress is null
                ? null
                : new CallbackProgress<SteamOfflineInstallProgress>(value =>
                    progress.Report(new CompatibilityCallSiteProgress(
                        CompatibilityCallSiteGate.Arm64ManagedScope,
                        value.CompletedFiles,
                        value.TotalFiles,
                        value.CurrentFile,
                        $"OfflineReady precondition — {value.Message} ({value.CompletedBytes:N0}/{value.TotalBytes:N0} bytes)")));

            var offline = await _offlineInspection.RunAsync(offlineProgress, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            if (offline.Outcome == SteamOfflineInstallOutcome.Cancelled)
                throw new OperationCanceledException("Step 17 OfflineReady precondition was cancelled.", cancellationToken);
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

            // Include architecture-neutral managed files if the depot ever adds any,
            // but deliberately exclude the x86_64 duplicate payload from iPhone/AOT analysis.
            var scopeFiles = arm64
                .Concat(shared)
                .OrderBy(file => file.RelativePath, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            var primaryMatches = arm64
                .Where(file => IsPrimaryArm64StS2Path(file.RelativePath))
                .ToArray();
            if (primaryMatches.Length != 1)
                throw new InvalidDataException($"Expected exactly one macOS arm64 sts2.dll, found {primaryMatches.Length}.");

            ulong scopeBytes = 0;
            foreach (var file in scopeFiles)
                checked { scopeBytes += (ulong)file.Length; }

            _scope = new ScopeSnapshot(
                managedRoot,
                receipt,
                offline,
                scopeFiles,
                primaryMatches[0].RelativePath.Replace('\\', '/'),
                allManaged.Length,
                arm64.Length,
                x86.Length,
                shared.Length,
                scopeBytes);

            return Pass(
                CompatibilityCallSiteGate.Arm64ManagedScope,
                "Receipt-backed iOS-relevant managed scope selected without opening game assemblies.\n" +
                $"OfflineReady precondition: YES ({offline.VerifiedFiles:N0}/{offline.PlannedFiles:N0} files)\n" +
                $"All .dll/.exe candidates in depot: {allManaged.Length:N0}\n" +
                $"macOS arm64 candidates selected: {arm64.Length:N0}\n" +
                $"Architecture-neutral managed candidates selected: {shared.Length:N0}\n" +
                $"macOS x86_64 duplicates deliberately excluded: {x86.Length:N0}\n" +
                $"Step 17 scan scope: {scopeFiles.Length:N0} files / {scopeBytes:N0} bytes\n" +
                $"Primary assembly: {_scope.PrimaryRelativePath}\n" +
                "Steam session consulted: NO\nNetwork attempted by Step 17: NO\nReal managed install modified: NO");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return Fail(CompatibilityCallSiteGate.Arm64ManagedScope, ex);
        }
    }

    public async Task<CompatibilityCallSiteGateResult> RunActualIlCallSiteScanAsync(
        IProgress<CompatibilityCallSiteProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var scope = RequireScope();
            _scan = null;
            var scan = new ScanAccumulator(scope.PrimaryRelativePath);

            for (var index = 0; index < scope.Files.Length; index++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var file = scope.Files[index];
                var relative = file.RelativePath.Replace('\\', '/');
                progress?.Report(new CompatibilityCallSiteProgress(
                    CompatibilityCallSiteGate.ActualIlCallSites,
                    index,
                    scope.Files.Length,
                    relative,
                    "Reading method bodies and recording concrete IL method-reference sites; no dependency resolution or execution…"));

                var path = ResolveChildPath(scope.ManagedRoot, relative);
                try
                {
                    using var module = ReadModule(path);
                    scan.ParsedModules++;
                    ScanModule(module, relative, scan);
                }
                catch (BadImageFormatException)
                {
                    scan.NonManagedCandidates++;
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException or ArgumentException or InvalidOperationException)
                {
                    scan.MetadataFailures.Add($"{relative}: {ex.GetType().Name}: {ex.Message}");
                }
            }

            if (scan.MetadataFailures.Count > 0)
                throw new InvalidDataException($"IL scan failed for {scan.MetadataFailures.Count} candidate(s); first: {scan.MetadataFailures[0]}");
            if (!scan.PrimarySeen)
                throw new InvalidDataException("The selected macOS arm64 sts2.dll was not parsed during the IL scan.");
            if (scan.ParsedModules <= 0 || scan.MethodsWithBodies <= 0)
                throw new InvalidDataException("No managed method bodies were parsed from the selected arm64 scope.");

            _scan = scan.Freeze();
            progress?.Report(new CompatibilityCallSiteProgress(
                CompatibilityCallSiteGate.ActualIlCallSites,
                scope.Files.Length,
                scope.Files.Length,
                scope.PrimaryRelativePath,
                "Concrete IL call-site scan complete."));

            return Pass(
                CompatibilityCallSiteGate.ActualIlCallSites,
                "Concrete IL call sites scanned across the selected arm64/shared managed scope.\n" +
                $"Managed modules parsed: {_scan.ParsedModules:N0}\n" +
                $"Non-managed .dll/.exe candidates skipped: {_scan.NonManagedCandidates:N0}\n" +
                $"Methods with IL bodies: {_scan.MethodsWithBodies:N0}\n" +
                $"IL instructions inspected: {_scan.Instructions:N0}\n" +
                $"Concrete method-reference sites: {_scan.MethodReferenceSites:N0}\n" +
                $"Indirect calli instructions: {_scan.CalliSites:N0}\n" +
                $"Dynamic/AOT-sensitive call sites: {_scan.DynamicCallSites:N0}\n" +
                $"Dynamic/AOT-sensitive sites inside primary sts2.dll: {_scan.PrimaryDynamicCallSites:N0}\n" +
                $"Categories: {FormatCounts(_scan.DynamicCategories)}\n" +
                "Sample actual call sites:\n" + FormatLineSamples(_scan.DynamicSamples) + "\n\n" +
                "Evidence policy: these are IL instruction operands, not raw string hits. They still prove only that code exists, not that every method is reachable during gameplay.\n" +
                "Assembly dependency resolution attempted: NO\nGame assembly loaded/executed: NO\nReal managed install modified: NO");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return Fail(CompatibilityCallSiteGate.ActualIlCallSites, ex);
        }
    }

    public CompatibilityCallSiteGateResult RunNativePlatformInteropClassification()
    {
        try
        {
            var scan = RequireScan();
            return Pass(
                CompatibilityCallSiteGate.NativePlatformInterop,
                "Native/platform-sensitive surfaces classified from the concrete arm64 IL scan.\n" +
                $"P/Invoke definitions: {scan.PInvokeDefinitions:N0}\n" +
                $"P/Invoke call sites observed inside defining modules: {scan.PInvokeCallSites:N0}\n" +
                $"Platform-sensitive managed API call sites: {scan.PlatformApiCallSites:N0}\n" +
                $"Primary sts2.dll platform-sensitive call sites: {scan.PrimaryPlatformApiCallSites:N0}\n" +
                $"Native module names ({scan.NativeModules.Count:N0}): {FormatCounts(scan.NativeModules)}\n" +
                $"Platform API categories: {FormatCounts(scan.PlatformCategories)}\n" +
                "P/Invoke call-site sample:\n" + FormatLineSamples(scan.PInvokeSamples) + "\n\n" +
                "Platform-sensitive call-site sample:\n" + FormatLineSamples(scan.PlatformSamples) + "\n\n" +
                "This is a triage map: a P/Invoke declaration or platform-sensitive call is not automatically a blocker until its required runtime path is established.\n" +
                "Desktop native libraries executed: NO\nAssembly dependency resolution attempted: NO\nReal managed install modified: NO");
        }
        catch (Exception ex)
        {
            return Fail(CompatibilityCallSiteGate.NativePlatformInterop, ex);
        }
    }

    public async Task<CompatibilityCallSiteGateResult> RunPrimaryDependencyPressureMapAsync(
        IProgress<CompatibilityCallSiteProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var scope = RequireScope();
            var scan = RequireScan();
            if (!scan.PrimarySeen)
                throw new InvalidDataException("Primary sts2.dll evidence is unavailable.");

            var verified = 0;
            ulong verifiedBytes = 0;
            var primaryHashPreserved = false;
            for (var index = 0; index < scope.Files.Length; index++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var file = scope.Files[index];
                var relative = file.RelativePath.Replace('\\', '/');
                progress?.Report(new CompatibilityCallSiteProgress(
                    CompatibilityCallSiteGate.PrimaryDependencyPressureMap,
                    index,
                    scope.Files.Length,
                    relative,
                    "Re-hashing every Step 17 scan candidate after Cecil inspection to prove the analysis remained read-only…"));
                var path = ResolveChildPath(scope.ManagedRoot, relative);
                var hash = await ComputeSha1HexAsync(path, cancellationToken).ConfigureAwait(false);
                if (!hash.Equals(file.Sha1Hex, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException($"Step 17 scan candidate no longer matches its trusted receipt SHA-1: {relative}");
                verified++;
                checked { verifiedBytes += (ulong)file.Length; }
                if (relative.Equals(scope.PrimaryRelativePath, StringComparison.OrdinalIgnoreCase))
                    primaryHashPreserved = true;
            }

            if (verified != scope.Files.Length || verifiedBytes != scope.ScopeBytes)
                throw new InvalidDataException("Post-analysis SHA-1 verification did not cover the complete Step 17 arm64/shared scope.");
            if (!primaryHashPreserved)
                throw new InvalidDataException("Primary sts2.dll was not included in post-analysis SHA-1 verification.");

            progress?.Report(new CompatibilityCallSiteProgress(
                CompatibilityCallSiteGate.PrimaryDependencyPressureMap,
                scope.Files.Length,
                scope.Files.Length,
                scope.PrimaryRelativePath,
                "Primary dependency pressure map complete; all scanned files still match the install receipt."));

            return Pass(
                CompatibilityCallSiteGate.PrimaryDependencyPressureMap,
                "Primary sts2.dll direct-dependency pressure map built from actual IL method references.\n" +
                $"Primary assembly: {scope.PrimaryRelativePath}\n" +
                $"Primary methods with bodies: {scan.PrimaryMethodsWithBodies:N0}\n" +
                $"Primary concrete method-reference sites: {scan.PrimaryMethodReferenceSites:N0}\n" +
                $"Primary dynamic/AOT-sensitive sites: {scan.PrimaryDynamicCallSites:N0} ({FormatCounts(scan.PrimaryDynamicCategories)})\n" +
                $"Primary platform-sensitive sites: {scan.PrimaryPlatformApiCallSites:N0} ({FormatCounts(scan.PrimaryPlatformCategories)})\n" +
                $"Primary subsystem calls: {FormatCounts(scan.PrimarySubsystemCalls)}\n" +
                $"Top external target assemblies/scopes: {FormatCounts(scan.PrimaryExternalTargets, 16)}\n" +
                $"Primary assembly references ({scan.PrimaryAssemblyReferences.Count:N0}): {FormatInlineSample(scan.PrimaryAssemblyReferences, 18)}\n" +
                "Primary dynamic/AOT-sensitive call sample:\n" + FormatLineSamples(scan.PrimaryDynamicSamples) + "\n\n" +
                "Primary platform-sensitive call sample:\n" + FormatLineSamples(scan.PrimaryPlatformSamples) + "\n\n" +
                "Primary subsystem-call sample:\n" + FormatLineSamples(scan.PrimarySubsystemSamples) + "\n\n" +
                $"Post-analysis receipt SHA-1s reverified: {verified:N0}/{scope.Files.Length:N0} ({verifiedBytes:N0} bytes)\n" +
                "Primary sts2.dll receipt SHA-1 preserved: YES\n" +
                "All Step 17 scan candidates receipt SHA-1 preserved: YES\n" +
                "Assembly dependency resolution attempted: NO\nSteam session consulted: NO\nNetwork attempted: NO\nReal managed install modified: NO\nGame assembly loaded/executed: NO\n" +
                "Next-step policy: use these concrete counts/samples to choose a narrow compatibility rewrite target; Step 17 performs no rewrite itself.");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return Fail(CompatibilityCallSiteGate.PrimaryDependencyPressureMap, ex);
        }
    }

    private static void ScanModule(ModuleDefinition module, string relativePath, ScanAccumulator scan)
    {
        var assemblyName = module.Assembly?.Name?.Name ?? module.Name;
        var isPrimary = relativePath.Equals(scan.PrimaryRelativePath, StringComparison.OrdinalIgnoreCase);
        if (isPrimary)
        {
            if (scan.PrimarySeen)
                throw new InvalidDataException("Primary sts2.dll was encountered more than once during Step 17 scanning.");
            if (module.Assembly?.Name is null)
                throw new InvalidDataException("Primary sts2.dll does not contain a managed assembly manifest.");
            scan.PrimarySeen = true;
            foreach (var reference in module.AssemblyReferences)
                scan.PrimaryAssemblyReferences.Add($"{reference.Name} {reference.Version}");
        }

        var pinvokeDefinitions = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var type in EnumerateTypes(module.Types))
        {
            foreach (var method in type.Methods)
            {
                if (method.IsPInvokeImpl && method.PInvokeInfo is not null)
                {
                    scan.PInvokeDefinitions++;
                    var nativeModule = method.PInvokeInfo.Module?.Name ?? "<unknown-native-module>";
                    Increment(scan.NativeModules, nativeModule);
                    pinvokeDefinitions[method.FullName] = nativeModule;
                    AddSample(scan.PInvokeDefinitionSamples, $"{assemblyName}: {method.FullName} -> {nativeModule}!{method.PInvokeInfo.EntryPoint}");
                }
            }
        }

        foreach (var type in EnumerateTypes(module.Types))
        {
            foreach (var method in type.Methods)
            {
                if (!method.HasBody)
                    continue;

                scan.MethodsWithBodies++;
                if (isPrimary)
                    scan.PrimaryMethodsWithBodies++;

                var source = $"{assemblyName}:{type.FullName}::{method.Name}";
                foreach (var instruction in method.Body.Instructions)
                {
                    scan.Instructions++;
                    if (instruction.OpCode.Code == Code.Calli)
                    {
                        scan.CalliSites++;
                        scan.DynamicCallSites++;
                        Increment(scan.DynamicCategories, "IndirectCalli");
                        AddSample(scan.DynamicSamples, $"IndirectCalli: {source}");
                        if (isPrimary)
                        {
                            scan.PrimaryDynamicCallSites++;
                            Increment(scan.PrimaryDynamicCategories, "IndirectCalli");
                            AddSample(scan.PrimaryDynamicSamples, $"IndirectCalli: {source}");
                        }
                        continue;
                    }

                    if (!IsMethodReferenceInstruction(instruction.OpCode.Code) || instruction.Operand is not MethodReference target)
                        continue;

                    scan.MethodReferenceSites++;
                    if (isPrimary)
                        scan.PrimaryMethodReferenceSites++;

                    var targetText = target.FullName;
                    var dynamicCategory = ClassifyDynamicRisk(target);
                    if (dynamicCategory is not null)
                    {
                        scan.DynamicCallSites++;
                        Increment(scan.DynamicCategories, dynamicCategory);
                        AddSample(scan.DynamicSamples, $"{dynamicCategory}: {source} -> {targetText}");
                        if (isPrimary)
                        {
                            scan.PrimaryDynamicCallSites++;
                            Increment(scan.PrimaryDynamicCategories, dynamicCategory);
                            AddSample(scan.PrimaryDynamicSamples, $"{dynamicCategory}: {source} -> {targetText}");
                        }
                    }

                    if (pinvokeDefinitions.TryGetValue(target.FullName, out var nativeModule))
                    {
                        scan.PInvokeCallSites++;
                        AddSample(scan.PInvokeSamples, $"{source} -> {target.FullName} -> {nativeModule}");
                    }

                    var platformCategory = ClassifyPlatformRisk(target);
                    if (platformCategory is not null)
                    {
                        scan.PlatformApiCallSites++;
                        Increment(scan.PlatformCategories, platformCategory);
                        AddSample(scan.PlatformSamples, $"{platformCategory}: {source} -> {targetText}");
                        if (isPrimary)
                        {
                            scan.PrimaryPlatformApiCallSites++;
                            Increment(scan.PrimaryPlatformCategories, platformCategory);
                            AddSample(scan.PrimaryPlatformSamples, $"{platformCategory}: {source} -> {targetText}");
                        }
                    }

                    if (isPrimary)
                    {
                        var targetScope = GetTargetScopeName(target);
                        if (!string.IsNullOrWhiteSpace(targetScope) &&
                            !targetScope.Equals(assemblyName, StringComparison.OrdinalIgnoreCase))
                        {
                            Increment(scan.PrimaryExternalTargets, targetScope);
                        }

                        var subsystem = ClassifySubsystem(target);
                        if (subsystem is not null)
                        {
                            Increment(scan.PrimarySubsystemCalls, subsystem);
                            AddSample(scan.PrimarySubsystemSamples, $"{subsystem}: {source} -> {targetText}");
                        }
                    }
                }
            }
        }
    }

    private static bool IsMethodReferenceInstruction(Code code)
        => code is Code.Call or Code.Callvirt or Code.Newobj or Code.Ldftn or Code.Ldvirtftn or Code.Jmp;

    private static string? ClassifyDynamicRisk(MethodReference target)
    {
        var type = target.DeclaringType.FullName;
        var scope = GetTargetScopeName(target);
        var name = target.Name;

        if (type.Contains("System.Reflection.Emit", StringComparison.Ordinal))
            return "ReflectionEmit";
        if (type.StartsWith("System.Linq.Expressions.", StringComparison.Ordinal) && name.StartsWith("Compile", StringComparison.Ordinal))
            return "ExpressionCompile";
        if ((type.StartsWith("HarmonyLib.", StringComparison.Ordinal) || scope.Equals("0Harmony", StringComparison.OrdinalIgnoreCase)) &&
            name is "Patch" or "PatchAll" or "PatchCategory" or "PatchAllUncategorized" or "CreateProcessor")
            return "HarmonyRuntimePatch";
        if (type.StartsWith("MonoMod.RuntimeDetour.", StringComparison.Ordinal) ||
            type.StartsWith("MonoMod.Cil.", StringComparison.Ordinal) ||
            type.Contains("DynamicMethodDefinition", StringComparison.Ordinal))
            return "MonoModRuntimeDetour";
        if (type.Equals("System.Reflection.Assembly", StringComparison.Ordinal) && name.StartsWith("Load", StringComparison.Ordinal))
            return "DynamicAssemblyLoad";
        if (type.StartsWith("System.Runtime.Loader.AssemblyLoadContext", StringComparison.Ordinal) && name.Contains("Load", StringComparison.Ordinal))
            return "DynamicAssemblyLoad";
        if (type.Equals("System.Runtime.CompilerServices.RuntimeHelpers", StringComparison.Ordinal) && name.Equals("PrepareMethod", StringComparison.Ordinal))
            return "PrepareMethod";

        return null;
    }

    private static string? ClassifyPlatformRisk(MethodReference target)
    {
        var type = target.DeclaringType.FullName;
        var name = target.Name;
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

    private ScopeSnapshot RequireScope()
        => _scope ?? throw new InvalidOperationException("Gate A must pass before later Step 17 gates run.");

    private ScanSnapshot RequireScan()
        => _scan ?? throw new InvalidOperationException("Gate B must pass before later Step 17 classification gates run.");

    private static ModuleDefinition ReadModule(string path)
        => ModuleDefinition.ReadModule(path, new ReaderParameters
        {
            ReadSymbols = false,
            ReadingMode = ReadingMode.Deferred,
        });

    private static IEnumerable<TypeDefinition> EnumerateTypes(IEnumerable<TypeDefinition> roots)
    {
        foreach (var type in roots)
        {
            yield return type;
            foreach (var nested in EnumerateTypes(type.NestedTypes))
                yield return nested;
        }
    }

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

    private static void Increment(Dictionary<string, long> values, string key)
    {
        if (values.TryGetValue(key, out var current))
            values[key] = checked(current + 1);
        else
            values[key] = 1;
    }

    private static void AddSample(List<string> samples, string value)
    {
        if (samples.Count >= SampleLimit)
            return;
        if (!samples.Contains(value, StringComparer.Ordinal))
            samples.Add(value);
    }

    private static string FormatCounts(IReadOnlyDictionary<string, long> values, int max = 12)
    {
        if (values.Count == 0)
            return "none";
        return string.Join(", ", values
            .OrderByDescending(pair => pair.Value)
            .ThenBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase)
            .Take(max)
            .Select(pair => $"{pair.Key}={pair.Value:N0}"));
    }

    private static string FormatLineSamples(IReadOnlyList<string> values)
        => values.Count == 0 ? "• none" : string.Join("\n", values.Select(value => $"• {value}"));

    private static string FormatInlineSample(IEnumerable<string> values, int max)
    {
        var sample = values.Take(max).ToArray();
        return sample.Length == 0 ? "none" : string.Join(", ", sample);
    }

    private static bool IsManagedAssemblyFileName(string path)
    {
        var extension = Path.GetExtension(path);
        return extension.Equals(".dll", StringComparison.OrdinalIgnoreCase) ||
               extension.Equals(".exe", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsMacOsArm64ManagedPath(string path)
        => ("/" + path.Replace('\\', '/').TrimStart('/'))
            .Contains("/data_sts2_macos_arm64/", StringComparison.OrdinalIgnoreCase);

    private static bool IsMacOsX8664ManagedPath(string path)
        => ("/" + path.Replace('\\', '/').TrimStart('/'))
            .Contains("/data_sts2_macos_x86_64/", StringComparison.OrdinalIgnoreCase);

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

    private static CompatibilityCallSiteGateResult Pass(CompatibilityCallSiteGate gate, string detail)
        => new(gate, true, detail);

    private static CompatibilityCallSiteGateResult Fail(CompatibilityCallSiteGate gate, Exception ex)
        => new(gate, false, $"{ex.GetType().Name}: {ex.Message}");

    private sealed class CallbackProgress<T> : IProgress<T>
    {
        private readonly Action<T> _callback;
        public CallbackProgress(Action<T> callback) => _callback = callback ?? throw new ArgumentNullException(nameof(callback));
        public void Report(T value) => _callback(value);
    }

    private sealed record ScopeSnapshot(
        string ManagedRoot,
        SteamManagedInstallReceipt Receipt,
        SteamOfflineInstallResult Offline,
        SteamManagedInstallFile[] Files,
        string PrimaryRelativePath,
        int AllManagedCandidates,
        int Arm64Candidates,
        int X8664Candidates,
        int SharedCandidates,
        ulong ScopeBytes);

    private sealed class ScanAccumulator
    {
        public ScanAccumulator(string primaryRelativePath) => PrimaryRelativePath = primaryRelativePath;
        public string PrimaryRelativePath { get; }
        public int ParsedModules { get; set; }
        public int NonManagedCandidates { get; set; }
        public long MethodsWithBodies { get; set; }
        public long Instructions { get; set; }
        public long MethodReferenceSites { get; set; }
        public long CalliSites { get; set; }
        public long DynamicCallSites { get; set; }
        public long PrimaryDynamicCallSites { get; set; }
        public long PInvokeDefinitions { get; set; }
        public long PInvokeCallSites { get; set; }
        public long PlatformApiCallSites { get; set; }
        public long PrimaryPlatformApiCallSites { get; set; }
        public bool PrimarySeen { get; set; }
        public long PrimaryMethodsWithBodies { get; set; }
        public long PrimaryMethodReferenceSites { get; set; }
        public Dictionary<string, long> DynamicCategories { get; } = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, long> NativeModules { get; } = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, long> PlatformCategories { get; } = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, long> PrimaryDynamicCategories { get; } = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, long> PrimaryPlatformCategories { get; } = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, long> PrimaryExternalTargets { get; } = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, long> PrimarySubsystemCalls { get; } = new(StringComparer.OrdinalIgnoreCase);
        public SortedSet<string> PrimaryAssemblyReferences { get; } = new(StringComparer.OrdinalIgnoreCase);
        public List<string> DynamicSamples { get; } = [];
        public List<string> PInvokeDefinitionSamples { get; } = [];
        public List<string> PInvokeSamples { get; } = [];
        public List<string> PlatformSamples { get; } = [];
        public List<string> PrimaryDynamicSamples { get; } = [];
        public List<string> PrimaryPlatformSamples { get; } = [];
        public List<string> PrimarySubsystemSamples { get; } = [];
        public List<string> MetadataFailures { get; } = [];

        public ScanSnapshot Freeze() => new(
            ParsedModules,
            NonManagedCandidates,
            MethodsWithBodies,
            Instructions,
            MethodReferenceSites,
            CalliSites,
            DynamicCallSites,
            PrimaryDynamicCallSites,
            PInvokeDefinitions,
            PInvokeCallSites,
            PlatformApiCallSites,
            PrimaryPlatformApiCallSites,
            PrimarySeen,
            PrimaryMethodsWithBodies,
            PrimaryMethodReferenceSites,
            new Dictionary<string, long>(DynamicCategories, StringComparer.OrdinalIgnoreCase),
            new Dictionary<string, long>(NativeModules, StringComparer.OrdinalIgnoreCase),
            new Dictionary<string, long>(PlatformCategories, StringComparer.OrdinalIgnoreCase),
            new Dictionary<string, long>(PrimaryDynamicCategories, StringComparer.OrdinalIgnoreCase),
            new Dictionary<string, long>(PrimaryPlatformCategories, StringComparer.OrdinalIgnoreCase),
            new Dictionary<string, long>(PrimaryExternalTargets, StringComparer.OrdinalIgnoreCase),
            new Dictionary<string, long>(PrimarySubsystemCalls, StringComparer.OrdinalIgnoreCase),
            PrimaryAssemblyReferences.ToArray(),
            DynamicSamples.ToArray(),
            PInvokeSamples.Count > 0 ? PInvokeSamples.ToArray() : PInvokeDefinitionSamples.ToArray(),
            PlatformSamples.ToArray(),
            PrimaryDynamicSamples.ToArray(),
            PrimaryPlatformSamples.ToArray(),
            PrimarySubsystemSamples.ToArray());
    }

    private sealed record ScanSnapshot(
        int ParsedModules,
        int NonManagedCandidates,
        long MethodsWithBodies,
        long Instructions,
        long MethodReferenceSites,
        long CalliSites,
        long DynamicCallSites,
        long PrimaryDynamicCallSites,
        long PInvokeDefinitions,
        long PInvokeCallSites,
        long PlatformApiCallSites,
        long PrimaryPlatformApiCallSites,
        bool PrimarySeen,
        long PrimaryMethodsWithBodies,
        long PrimaryMethodReferenceSites,
        IReadOnlyDictionary<string, long> DynamicCategories,
        IReadOnlyDictionary<string, long> NativeModules,
        IReadOnlyDictionary<string, long> PlatformCategories,
        IReadOnlyDictionary<string, long> PrimaryDynamicCategories,
        IReadOnlyDictionary<string, long> PrimaryPlatformCategories,
        IReadOnlyDictionary<string, long> PrimaryExternalTargets,
        IReadOnlyDictionary<string, long> PrimarySubsystemCalls,
        IReadOnlyList<string> PrimaryAssemblyReferences,
        IReadOnlyList<string> DynamicSamples,
        IReadOnlyList<string> PInvokeSamples,
        IReadOnlyList<string> PlatformSamples,
        IReadOnlyList<string> PrimaryDynamicSamples,
        IReadOnlyList<string> PrimaryPlatformSamples,
        IReadOnlyList<string> PrimarySubsystemSamples);
}
