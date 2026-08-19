using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace StS2Launcher.Core;

/// <summary>
/// Step 14 boundary: inspect the already-managed StS2 depot without executing,
/// rewriting, loading, or contacting Steam. The exact Step 13 OfflineReady proof
/// is re-used as the trust precondition, then the receipt/file tree is classified
/// and likely dependency/dynamic-code indicators are scanned from managed binary
/// metadata strings.
///
/// This is deliberately heuristic. A marker in assembly metadata means "inspect
/// this later", not "this runtime path is definitely required".
/// </summary>
public sealed class SteamCompatibilityInventoryInspection
{
    public const uint TargetAppId = SteamManagedInstallAttempt.TargetAppId;

    private static readonly string[] AssetExtensions =
    [
        ".pck", ".godot", ".tscn", ".tres", ".res", ".scn", ".gd", ".gdshader",
        ".shader", ".glsl", ".png", ".jpg", ".jpeg", ".webp", ".svg", ".ico",
        ".ogg", ".wav", ".mp3", ".flac", ".bank", ".ttf", ".otf", ".json",
        ".csv", ".xml", ".txt", ".cfg", ".ini", ".translation", ".po", ".mo",
    ];

    private static readonly string[] GodotContentExtensions =
    [
        ".pck", ".godot", ".tscn", ".tres", ".res", ".scn", ".gd", ".gdshader",
    ];

    private static readonly string[] NativeExtensions =
    [
        ".dylib", ".so", ".bundle", ".a",
    ];

    private static readonly string[] GodotMarkers =
    [
        "GodotSharp", "Godot.NativeInterop", "Godot.Collections", "Godot.GD",
    ];

    private static readonly string[] FmodMarkers =
    [
        "FMOD", "fmodstudio", "fmodL", "fmod",
    ];

    private static readonly string[] SpineMarkers =
    [
        "Spine", "spine-csharp", "Spine.Unity",
    ];

    private static readonly string[] ReflectionMarkers =
    [
        "System.Reflection", "Activator.CreateInstance", "Assembly.Load", "MethodInfo",
        "PropertyInfo", "GetMethod", "GetProperty", "GetConstructor",
    ];

    private static readonly string[] DynamicCodeMarkers =
    [
        "System.Reflection.Emit", "Reflection.Emit", "DynamicMethod", "AssemblyBuilder",
        "MethodBuilder", "TypeBuilder", "ILGenerator", "Expression.Compile",
        "LambdaExpression.Compile", "System.Linq.Expressions",
    ];

    private static readonly string[] PlatformMarkers =
    [
        "Microsoft.Win32", "System.Management", "System.Windows", "AppKit", "kernel32",
        "user32", "advapi32", "steam_api64", "steam_api", "win-x64", "win-arm64",
        "osx-x64", "osx-arm64", "linux-x64", "linux-arm64",
    ];

    private readonly string _outputRootDirectory;
    private readonly SteamOfflineInstallInspection _offlineInspection;

    public SteamCompatibilityInventoryInspection(string outputRootDirectory)
    {
        if (string.IsNullOrWhiteSpace(outputRootDirectory))
            throw new ArgumentException("Output directory is required.", nameof(outputRootDirectory));

        _outputRootDirectory = Path.GetFullPath(outputRootDirectory);
        _offlineInspection = new SteamOfflineInstallInspection(_outputRootDirectory);
    }

    public async Task<SteamCompatibilityInventoryResult> RunAsync(
        IProgress<SteamCompatibilityInventoryProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        var outcome = SteamCompatibilityInventoryOutcome.Failed;
        uint? depotId = null;
        ulong? manifestId = null;
        string? branch = null;
        var offlineReady = false;
        var totalFiles = 0;
        ulong totalBytes = 0;
        var assetFiles = 0;
        ulong assetBytes = 0;
        var godotContentFiles = 0;
        var managedAssemblyFiles = 0;
        ulong managedAssemblyBytes = 0;
        var managedAssembliesScanned = 0;
        var nativeBinaryFiles = 0;
        ulong nativeBinaryBytes = 0;
        var godotSharpIndicatorFiles = 0;
        var fmodIndicatorFiles = 0;
        var spineIndicatorFiles = 0;
        var reflectionIndicatorFiles = 0;
        var dynamicCodeIndicatorFiles = 0;
        var platformSpecificFiles = 0;
        var otherFiles = 0;
        string? error = null;

        var managedAssemblyEvidence = new List<string>();
        var nativeBinaryEvidence = new List<string>();
        var godotSharpEvidence = new List<string>();
        var fmodEvidence = new List<string>();
        var spineEvidence = new List<string>();
        var reflectionEvidence = new List<string>();
        var dynamicCodeEvidence = new List<string>();
        var platformSpecificEvidence = new List<string>();
        var potentialIosBlockers = new List<string>();
        var dependencyNotes = new List<string>();

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            progress?.Report(new SteamCompatibilityInventoryProgress(
                SteamCompatibilityInventoryPhase.Locating,
                0,
                0,
                0,
                0,
                null,
                "Locating the Step 12 managed install; Step 14 will not modify it…"));

            progress?.Report(new SteamCompatibilityInventoryProgress(
                SteamCompatibilityInventoryPhase.VerifyingOfflineInstall,
                0,
                0,
                0,
                0,
                null,
                "Re-proving the Step 13 OfflineReady precondition locally before inventory…"));

            var offline = await _offlineInspection.RunAsync(
                    progress: null,
                    cancellationToken)
                .ConfigureAwait(false);

            if (!offline.Success)
            {
                outcome = SteamCompatibilityInventoryOutcome.LocalInstallNotReady;
                error = offline.Error ?? $"Step 13 local state is {offline.State}/{offline.Outcome}; repair or complete online setup before inventory.";
                return BuildResult();
            }

            offlineReady = true;
            depotId = offline.DepotId;
            manifestId = offline.InstalledManifestId;
            branch = offline.Branch;

            if (string.IsNullOrWhiteSpace(offline.ManagedInstallRelativePath))
            {
                outcome = SteamCompatibilityInventoryOutcome.LocalInstallNotReady;
                error = "Step 13 returned OfflineReady without a managed-install path.";
                return BuildResult();
            }

            var managedPath = ResolveChildPath(_outputRootDirectory, offline.ManagedInstallRelativePath);
            var receiptPath = Path.Combine(managedPath, SteamManagedInstallReceipt.FileName);

            progress?.Report(new SteamCompatibilityInventoryProgress(
                SteamCompatibilityInventoryPhase.ReadingReceipt,
                0,
                offline.PlannedFiles,
                0,
                offline.PlannedBytes,
                SteamManagedInstallReceipt.FileName,
                "Reading the already-verified non-secret install receipt…"));

            SteamManagedInstallReceipt? receipt;
            await using (var stream = new FileStream(
                             receiptPath,
                             FileMode.Open,
                             FileAccess.Read,
                             FileShare.Read,
                             bufferSize: 16 * 1024,
                             options: FileOptions.Asynchronous | FileOptions.SequentialScan))
            {
                receipt = await JsonSerializer.DeserializeAsync(
                        stream,
                        SteamManagedInstallJsonContext.Default.SteamManagedInstallReceipt,
                        cancellationToken)
                    .ConfigureAwait(false);
            }

            if (receipt is null)
            {
                outcome = SteamCompatibilityInventoryOutcome.LocalInstallNotReady;
                error = "The Step 12 receipt unexpectedly deserialized to null after the OfflineReady proof.";
                return BuildResult();
            }

            totalFiles = receipt.Files.Count;
            foreach (var file in receipt.Files)
                checked { totalBytes += (ulong)file.Length; }

            var orderedFiles = receipt.Files
                .OrderBy(file => file.RelativePath, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            var processedFiles = 0;
            ulong processedBytes = 0;

            foreach (var file in orderedFiles)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var relative = file.RelativePath.Replace('\\', '/');
                var path = ResolveChildPath(managedPath, relative);
                var extension = Path.GetExtension(relative).ToLowerInvariant();
                var lowerRelative = relative.ToLowerInvariant();

                var isAsset = AssetExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase);
                var isGodotContent = GodotContentExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase);
                var isPlatformSpecific = IsPlatformSpecificPath(lowerRelative);
                var pathGodot = ContainsAny(relative, GodotMarkers, out var godotPathMarkers);
                var pathFmod = ContainsAny(relative, FmodMarkers, out var fmodPathMarkers);
                var pathSpine = ContainsAny(relative, SpineMarkers, out var spinePathMarkers);

                BinaryScanResult? binaryScan = null;
                var managedCandidate = extension is ".dll" or ".exe";
                if (managedCandidate)
                {
                    progress?.Report(new SteamCompatibilityInventoryProgress(
                        SteamCompatibilityInventoryPhase.ScanningManagedAssemblies,
                        processedFiles,
                        totalFiles,
                        processedBytes,
                        totalBytes,
                        relative,
                        "Scanning managed-binary metadata strings for compatibility indicators; assembly code is not loaded or executed…"));

                    binaryScan = await ScanInterestingBinaryMarkersAsync(path, cancellationToken).ConfigureAwait(false);
                }

                var isManagedAssembly = binaryScan?.HasManagedMetadataSignature == true;
                var nativeKind = isManagedAssembly
                    ? null
                    : await DetectNativeBinaryKindAsync(path, extension, lowerRelative, cancellationToken).ConfigureAwait(false);
                var isNativeBinary = nativeKind is not null;

                if (isAsset)
                {
                    assetFiles++;
                    checked { assetBytes += (ulong)file.Length; }
                }

                if (isGodotContent)
                    godotContentFiles++;

                if (isManagedAssembly)
                {
                    managedAssemblyFiles++;
                    managedAssembliesScanned++;
                    checked { managedAssemblyBytes += (ulong)file.Length; }
                    managedAssemblyEvidence.Add(relative);
                }

                if (isNativeBinary)
                {
                    nativeBinaryFiles++;
                    checked { nativeBinaryBytes += (ulong)file.Length; }
                    nativeBinaryEvidence.Add($"{relative} [{nativeKind}]");
                }

                var godotMarkers = MergeMarkers(godotPathMarkers, binaryScan?.GodotMarkers);
                if (pathGodot || godotMarkers.Count > 0)
                {
                    godotSharpIndicatorFiles++;
                    godotSharpEvidence.Add(FormatEvidence(relative, godotMarkers, "Godot/GodotSharp path marker"));
                }

                var fmodMarkers = MergeMarkers(fmodPathMarkers, binaryScan?.FmodMarkers);
                if (pathFmod || fmodMarkers.Count > 0)
                {
                    fmodIndicatorFiles++;
                    fmodEvidence.Add(FormatEvidence(relative, fmodMarkers, "FMOD path marker"));
                }

                var spineMarkers = MergeMarkers(spinePathMarkers, binaryScan?.SpineMarkers);
                if (pathSpine || spineMarkers.Count > 0)
                {
                    spineIndicatorFiles++;
                    spineEvidence.Add(FormatEvidence(relative, spineMarkers, "Spine path marker"));
                }

                if (binaryScan?.ReflectionMarkers.Count > 0)
                {
                    reflectionIndicatorFiles++;
                    reflectionEvidence.Add(FormatEvidence(relative, binaryScan.ReflectionMarkers, "reflection metadata marker"));
                }

                if (binaryScan?.DynamicCodeMarkers.Count > 0)
                {
                    dynamicCodeIndicatorFiles++;
                    dynamicCodeEvidence.Add(FormatEvidence(relative, binaryScan.DynamicCodeMarkers, "dynamic-code metadata marker"));
                }

                var platformMarkers = MergeMarkers(
                    isPlatformSpecific ? new[] { "platform-specific path" } : Array.Empty<string>(),
                    binaryScan?.PlatformMarkers);
                if (platformMarkers.Count > 0)
                {
                    platformSpecificFiles++;
                    platformSpecificEvidence.Add(FormatEvidence(relative, platformMarkers, "platform-specific path"));
                }

                if (!isAsset && !isManagedAssembly && !isNativeBinary &&
                    godotMarkers.Count == 0 && fmodMarkers.Count == 0 && spineMarkers.Count == 0)
                {
                    otherFiles++;
                }

                processedFiles++;
                checked { processedBytes += (ulong)file.Length; }
                progress?.Report(new SteamCompatibilityInventoryProgress(
                    SteamCompatibilityInventoryPhase.ClassifyingFiles,
                    processedFiles,
                    totalFiles,
                    processedBytes,
                    totalBytes,
                    relative,
                    "Classified installed content read-only."));
            }

            if (nativeBinaryFiles > 0)
            {
                potentialIosBlockers.Add(
                    $"Desktop/native binaries detected ({nativeBinaryFiles} file(s)). These cannot be assumed executable or link-compatible on iOS and must be replaced, excluded, or rebuilt for iOS where required.");
            }

            if (dynamicCodeIndicatorFiles > 0)
            {
                potentialIosBlockers.Add(
                    $"Dynamic-code/JIT indicators detected in {dynamicCodeIndicatorFiles} managed assembly file(s). Reflection.Emit/DynamicMethod/builder/Expression.Compile usage requires targeted no-JIT validation or offline compatibility rewriting before iOS execution.");
            }

            if (platformSpecificFiles > 0)
            {
                potentialIosBlockers.Add(
                    $"Platform-specific path/API indicators detected in {platformSpecificFiles} file(s). macOS/Windows/Linux-specific pieces must be separated from portable managed/game data before iOS runtime integration.");
            }

            if (godotSharpIndicatorFiles > 0)
            {
                dependencyNotes.Add(
                    $"Godot/GodotSharp managed integration indicators detected in {godotSharpIndicatorFiles} file(s); later Godot host/managed-bridge work must account for the exact versions and native/managed boundary.");
            }

            if (fmodIndicatorFiles > 0)
            {
                dependencyNotes.Add(
                    $"FMOD indicators detected in {fmodIndicatorFiles} file(s); later iOS runtime work must verify native availability/licensing and must not add proprietary FMOD assets to this repository.");
            }

            if (spineIndicatorFiles > 0)
            {
                dependencyNotes.Add(
                    $"Spine indicators detected in {spineIndicatorFiles} file(s); later runtime work must verify the exact runtime/native/licensing requirements and must not add proprietary Spine assets to this repository.");
            }

            if (reflectionIndicatorFiles > 0)
            {
                dependencyNotes.Add(
                    $"General reflection indicators detected in {reflectionIndicatorFiles} managed assembly file(s). This is not automatically an iOS blocker, but trimming/AOT-sensitive reflection paths need later targeted inspection.");
            }

            outcome = SteamCompatibilityInventoryOutcome.Complete;
            progress?.Report(new SteamCompatibilityInventoryProgress(
                SteamCompatibilityInventoryPhase.Complete,
                totalFiles,
                totalFiles,
                totalBytes,
                totalBytes,
                null,
                "Compatibility inventory complete. No game file was modified and no game code was executed."));
            return BuildResult();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            outcome = SteamCompatibilityInventoryOutcome.Cancelled;
            error = "Step 14 compatibility inventory was cancelled. The managed install was not modified.";
            return BuildResult();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidDataException or JsonException or NotSupportedException or OverflowException)
        {
            outcome = SteamCompatibilityInventoryOutcome.Failed;
            error = $"Compatibility inventory failed safely: {ex.GetType().Name}: {ex.Message}";
            return BuildResult();
        }

        SteamCompatibilityInventoryResult BuildResult() => new(
            outcome,
            TargetAppId,
            depotId,
            manifestId,
            branch,
            offlineReady,
            totalFiles,
            totalBytes,
            assetFiles,
            assetBytes,
            godotContentFiles,
            managedAssemblyFiles,
            managedAssemblyBytes,
            managedAssembliesScanned,
            nativeBinaryFiles,
            nativeBinaryBytes,
            godotSharpIndicatorFiles,
            fmodIndicatorFiles,
            spineIndicatorFiles,
            reflectionIndicatorFiles,
            dynamicCodeIndicatorFiles,
            platformSpecificFiles,
            otherFiles,
            managedAssemblyEvidence.OrderBy(value => value, StringComparer.OrdinalIgnoreCase).ToArray(),
            nativeBinaryEvidence.OrderBy(value => value, StringComparer.OrdinalIgnoreCase).ToArray(),
            godotSharpEvidence.OrderBy(value => value, StringComparer.OrdinalIgnoreCase).ToArray(),
            fmodEvidence.OrderBy(value => value, StringComparer.OrdinalIgnoreCase).ToArray(),
            spineEvidence.OrderBy(value => value, StringComparer.OrdinalIgnoreCase).ToArray(),
            reflectionEvidence.OrderBy(value => value, StringComparer.OrdinalIgnoreCase).ToArray(),
            dynamicCodeEvidence.OrderBy(value => value, StringComparer.OrdinalIgnoreCase).ToArray(),
            platformSpecificEvidence.OrderBy(value => value, StringComparer.OrdinalIgnoreCase).ToArray(),
            potentialIosBlockers.ToArray(),
            dependencyNotes.ToArray(),
            SteamSessionConsulted: false,
            NetworkAccessAttempted: false,
            ManagedInstallModified: false,
            GameLaunchAttempted: false,
            sw.Elapsed,
            error);
    }

    private static bool IsPlatformSpecificPath(string lowerRelativePath)
    {
        return lowerRelativePath.Contains("/contents/macos/", StringComparison.Ordinal) ||
               lowerRelativePath.Contains("/macos/", StringComparison.Ordinal) ||
               lowerRelativePath.Contains("/osx", StringComparison.Ordinal) ||
               lowerRelativePath.Contains("/win32", StringComparison.Ordinal) ||
               lowerRelativePath.Contains("/win64", StringComparison.Ordinal) ||
               lowerRelativePath.Contains("/windows", StringComparison.Ordinal) ||
               lowerRelativePath.Contains("/linux", StringComparison.Ordinal) ||
               lowerRelativePath.Contains("x86_64", StringComparison.Ordinal) ||
               lowerRelativePath.Contains("x86-64", StringComparison.Ordinal) ||
               lowerRelativePath.Contains("amd64", StringComparison.Ordinal) ||
               lowerRelativePath.EndsWith(".dylib", StringComparison.Ordinal) ||
               lowerRelativePath.EndsWith(".so", StringComparison.Ordinal);
    }

    private static bool ContainsAny(string value, IReadOnlyList<string> markers, out IReadOnlyList<string> matches)
    {
        var found = markers
            .Where(marker => value.Contains(marker, StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        matches = found;
        return found.Length > 0;
    }

    private static IReadOnlyList<string> MergeMarkers(
        IReadOnlyList<string>? first,
        IReadOnlyList<string>? second)
    {
        return (first ?? Array.Empty<string>())
            .Concat(second ?? Array.Empty<string>())
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static string FormatEvidence(
        string relativePath,
        IReadOnlyList<string> markers,
        string fallback)
    {
        var detail = markers.Count == 0 ? fallback : string.Join(", ", markers.Take(8));
        return $"{relativePath} — {detail}";
    }

    private static async Task<BinaryScanResult> ScanInterestingBinaryMarkersAsync(
        string path,
        CancellationToken cancellationToken)
    {
        var markerGroups = new[]
        {
            GodotMarkers,
            FmodMarkers,
            SpineMarkers,
            ReflectionMarkers,
            DynamicCodeMarkers,
            PlatformMarkers,
        };
        var maxMarkerLength = markerGroups
            .SelectMany(group => group)
            .Append("BSJB")
            .Max(marker => marker.Length);

        var godot = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var fmod = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var spine = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var reflection = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var dynamicCode = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var platform = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var hasManagedSignature = false;
        var tail = string.Empty;
        var buffer = new byte[64 * 1024];

        await using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: buffer.Length,
            options: FileOptions.Asynchronous | FileOptions.SequentialScan);

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var read = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken).ConfigureAwait(false);
            if (read <= 0)
                break;

            var text = tail + Encoding.Latin1.GetString(buffer, 0, read);
            if (!hasManagedSignature && text.Contains("BSJB", StringComparison.Ordinal))
                hasManagedSignature = true;

            AddMatches(text, GodotMarkers, godot);
            AddMatches(text, FmodMarkers, fmod);
            AddMatches(text, SpineMarkers, spine);
            AddMatches(text, ReflectionMarkers, reflection);
            AddMatches(text, DynamicCodeMarkers, dynamicCode);
            AddMatches(text, PlatformMarkers, platform);

            var keep = Math.Min(maxMarkerLength - 1, text.Length);
            tail = keep > 0 ? text[^keep..] : string.Empty;
        }

        return new BinaryScanResult(
            hasManagedSignature,
            godot.OrderBy(value => value, StringComparer.OrdinalIgnoreCase).ToArray(),
            fmod.OrderBy(value => value, StringComparer.OrdinalIgnoreCase).ToArray(),
            spine.OrderBy(value => value, StringComparer.OrdinalIgnoreCase).ToArray(),
            reflection.OrderBy(value => value, StringComparer.OrdinalIgnoreCase).ToArray(),
            dynamicCode.OrderBy(value => value, StringComparer.OrdinalIgnoreCase).ToArray(),
            platform.OrderBy(value => value, StringComparer.OrdinalIgnoreCase).ToArray());
    }

    private static void AddMatches(
        string text,
        IReadOnlyList<string> markers,
        ISet<string> destination)
    {
        foreach (var marker in markers)
        {
            if (text.Contains(marker, StringComparison.OrdinalIgnoreCase))
                destination.Add(marker);
        }
    }

    private static async Task<string?> DetectNativeBinaryKindAsync(
        string path,
        string extension,
        string lowerRelative,
        CancellationToken cancellationToken)
    {
        var knownNativeByPath = NativeExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase) ||
                                lowerRelative.Contains("/contents/macos/", StringComparison.Ordinal);

        var header = new byte[8];
        await using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: header.Length,
            options: FileOptions.Asynchronous | FileOptions.SequentialScan);
        var read = await stream.ReadAsync(header.AsMemory(0, header.Length), cancellationToken).ConfigureAwait(false);

        if (read >= 4)
        {
            if (header[0] == 0x7F && header[1] == (byte)'E' && header[2] == (byte)'L' && header[3] == (byte)'F')
                return "ELF native binary";

            if (header[0] == (byte)'M' && header[1] == (byte)'Z')
                return "PE/COFF native binary";

            var magic = $"{header[0]:X2}{header[1]:X2}{header[2]:X2}{header[3]:X2}";
            if (magic is "CFFAEDFE" or "FEEDFACF" or "CEFAEDFE" or "FEEDFACE")
                return "Mach-O native binary";
            if (magic is "CAFEBABE" or "BEBAFECA" or "CAFEBABF" or "BFBAFECA")
                return "Mach-O universal/fat binary";
        }

        return knownNativeByPath ? "native-library/path candidate" : null;
    }

    private static string ResolveChildPath(string root, string relativePath)
    {
        if (!SteamSingleFileTargetSelector.IsSafeRelativePath(relativePath))
            throw new InvalidDataException($"Unsafe relative path: {relativePath}");

        var rootFull = Path.GetFullPath(root);
        var child = Path.GetFullPath(Path.Combine(
            rootFull,
            relativePath.Replace('/', Path.DirectorySeparatorChar)));
        var prefix = rootFull.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        if (!child.StartsWith(prefix, StringComparison.Ordinal))
            throw new InvalidDataException($"Path escaped the managed root: {relativePath}");
        return child;
    }

    private sealed record BinaryScanResult(
        bool HasManagedMetadataSignature,
        IReadOnlyList<string> GodotMarkers,
        IReadOnlyList<string> FmodMarkers,
        IReadOnlyList<string> SpineMarkers,
        IReadOnlyList<string> ReflectionMarkers,
        IReadOnlyList<string> DynamicCodeMarkers,
        IReadOnlyList<string> PlatformMarkers);
}
