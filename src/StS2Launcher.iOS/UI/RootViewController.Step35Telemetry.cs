using System.Text;

namespace StS2Launcher.iOS;

public sealed partial class RootViewController
{
    private bool TryInitializeStep35RunTelemetry(out string error)
    {
        error = string.Empty;
        lock (_step35CrashCheckpointSync)
        {
            _step35RunTelemetryReady = false;
            _step35RunId = null;
            _step35CrashCheckpointPath = null;
            _step35StaticMapPath = null;
            _step35GodotReconnaissancePath = null;
            _step35LastCheckpointPath = null;

            try
            {
                Directory.CreateDirectory(_deviceTestReportWriter.ReportsRoot);
                var initializedUtc = DateTimeOffset.UtcNow;
                var runId = $"{initializedUtc:yyyyMMddTHHmmssfffffffZ}-pid{Environment.ProcessId}-{Guid.NewGuid():N}";
                var crashFileName = $"Step35-CrashCheckpoint-{runId}.txt";
                var staticMapFileName = $"Step35-ExecuteVeryEarly-StaticMap-{runId}.txt";
                var godotReconFileName = $"Step35-GodotNativeReconnaissance-{runId}.txt";
                var crashPath = Path.Combine(_deviceTestReportWriter.ReportsRoot, crashFileName);
                var staticMapPath = Path.Combine(_deviceTestReportWriter.ReportsRoot, staticMapFileName);
                var godotReconPath = Path.Combine(_deviceTestReportWriter.ReportsRoot, godotReconFileName);
                var lastCheckpointPath = Path.Combine(_deviceTestReportWriter.ReportsRoot, Step35LastCheckpointFileName);
                var currentRunPath = Path.Combine(_deviceTestReportWriter.ReportsRoot, Step35CurrentRunFileName);

                WriteStep35TextFileDurably(
                    crashPath,
                    "StS2 Launcher — Step 35.0.30 Comprehensive GodotSharp / Native reconnaissance crash checkpoint\n" +
                    "Output-only diagnostic; never consumed as trusted runtime input.\n" +
                    $"Run ID: {runId}\n" +
                    $"Initialized UTC: {initializedUtc:O}\n" +
                    $"Process ID: {Environment.ProcessId}\n" +
                    $"App version: {CurrentReleasePresentation.DisplayVersion} ({CurrentReleasePresentation.DisplayBuild})\n" +
                    $"Expected source version: {CurrentReleasePresentation.ExpectedDisplayVersion} ({CurrentReleasePresentation.ExpectedBuildVersion})\n" +
                    "Candidate: STEP 35.0.30 — EXACT-AUTHORITY CLOSURE + GATE-D FINALIZATION\n" +
                    $"Diagnostic mode: {_transformedRealStS2VeryEarlyInitialization.DiagnosticMode}\n" +
                    "Execution policy: exact source transform/resolver/later-game-boundary prohibitions unchanged; Gate A performs read-only bundle-wide native/GodotSharp inspection and emits separately verified sts2 + GodotSharp diagnostic derivatives; GodotSharp probes are entry-only. NATURAL preserves the original Godot dictionary/native path; OS-RECON applies only the bounded four-reference BCL dictionary substitution and keeps Godot.OS natural; FORWARD adds exactly one local new string[0] provider substitution for Godot.OS.GetCmdlineArgs. Those three controls require a fresh process with no Godot state. CORE-HANDOFF is the sole explicit exception: it requires the already-proven Step-15 smoke engine, rejects dotnet-feature or Godot-managed-runtime state, and hands the exact source-built callback table to the private GodotSharp derivative. Native game loading remains forbidden.\n" +
                    $"Implementation: {CurrentReleasePresentation.Step35ImplementationMarker}\n\n");

                var initialLine = $"{initializedUtc:O} | run={runId} | pid={Environment.ProcessId} | managedThread={Environment.CurrentManagedThreadId} | RUN_TELEMETRY_READY — run-specific journal created and durably flushed before Gate A.";
                using (var stream = new FileStream(crashPath, FileMode.Append, FileAccess.Write, FileShare.Read, 4096, FileOptions.None))
                using (var writer = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), 1024, leaveOpen: true))
                {
                    writer.WriteLine(initialLine);
                    writer.Flush();
                    stream.Flush(flushToDisk: true);
                }

                WriteStep35TextFileDurably(
                    lastCheckpointPath,
                    "StS2 Launcher — Step 35 last durable checkpoint\n" +
                    "Output-only diagnostic; overwrite-on-each-checkpoint convenience file.\n" +
                    $"Run ID: {runId}\n" +
                    $"Journal: {crashFileName}\n" +
                    $"Static map: {staticMapFileName}\n" +
                    $"Godot/native reconnaissance: {godotReconFileName}\n" +
                    initialLine + "\n");

                WriteStep35TextFileDurably(
                    currentRunPath,
                    "StS2 Launcher — Step 35 current run manifest\n" +
                    "Output-only diagnostic; use this file to correlate artifacts from the same process run.\n" +
                    $"Run ID: {runId}\n" +
                    $"Initialized UTC: {initializedUtc:O}\n" +
                    $"Process ID: {Environment.ProcessId}\n" +
                    $"App version: {CurrentReleasePresentation.DisplayVersion} ({CurrentReleasePresentation.DisplayBuild})\n" +
                    $"Crash journal: {crashFileName}\n" +
                    $"Static map: {staticMapFileName}\n" +
                    $"Godot/native reconnaissance: {godotReconFileName}\n" +
                    $"Last checkpoint: {Step35LastCheckpointFileName}\n" +
                    $"Diagnostic mode: {_transformedRealStS2VeryEarlyInitialization.DiagnosticMode}\n" +
                    "Candidate: STEP 35.0.30 — EXACT-AUTHORITY CLOSURE + GATE-D FINALIZATION\n");

                _step35RunId = runId;
                _step35CrashCheckpointPath = crashPath;
                _step35StaticMapPath = staticMapPath;
                _step35GodotReconnaissancePath = godotReconPath;
                _step35LastCheckpointPath = lastCheckpointPath;
                _step35RunTelemetryReady = true;
                return true;
            }
            catch (Exception ex)
            {
                error = $"{ex.GetType().Name}: {ex.Message}";
                Console.Error.WriteLine($"Step-35 run-telemetry initialization failed: {error}");
                return false;
            }
        }
    }

    private bool WriteStep35GodotReconnaissance(out string error)
    {
        error = string.Empty;
        try
        {
            if (!_step35RunTelemetryReady || string.IsNullOrWhiteSpace(_step35RunId) || string.IsNullOrWhiteSpace(_step35GodotReconnaissancePath))
                throw new InvalidOperationException("Step-35 run telemetry is not initialized for Godot/native reconnaissance.");

            var body = _transformedRealStS2VeryEarlyInitialization.GetVerifiedGodotReconnaissanceReport();
            var markerMap = _transformedRealStS2VeryEarlyInitialization.GetVerifiedGodotSharpDiagnosticMarkerMap();
            WriteStep35TextFileDurably(
                _step35GodotReconnaissancePath,
                $"Run ID: {_step35RunId}\n" +
                $"Process ID: {Environment.ProcessId}\n" +
                $"App version: {CurrentReleasePresentation.DisplayVersion} ({CurrentReleasePresentation.DisplayBuild})\n" +
                $"Expected source version: {CurrentReleasePresentation.ExpectedDisplayVersion} ({CurrentReleasePresentation.ExpectedBuildVersion})\n" +
                $"Diagnostic mode: {_transformedRealStS2VeryEarlyInitialization.DiagnosticMode}\n" +
                "Candidate: STEP 35.0.30 — COMPREHENSIVE GODOTSHARP / NATIVE RECONNAISSANCE + EXACT-AUTHORITY CLOSURE\n" +
                "This file is output-only and is written before Gate B CLR admission.\n\n" +
                body +
                "\n\n[GODOTSHARP RUNTIME ENTRY-MARKER PLAN]\n" +
                markerMap + "\n");
            return true;
        }
        catch (Exception ex)
        {
            error = $"{ex.GetType().Name}: {ex.Message}";
            Console.Error.WriteLine($"Step-35 Godot/native reconnaissance write failed: {error}");
            return false;
        }
    }

    private bool WriteStep35StaticMap(out string error)
    {
        error = string.Empty;
        try
        {
            if (!_step35RunTelemetryReady || string.IsNullOrWhiteSpace(_step35RunId) || string.IsNullOrWhiteSpace(_step35StaticMapPath))
                throw new InvalidOperationException("Step-35 run telemetry is not initialized.");

            var body = _transformedRealStS2VeryEarlyInitialization.GetVerifiedVeryEarlyStaticInstructionMap();
            WriteStep35TextFileDurably(
                _step35StaticMapPath,
                $"Generated UTC: {DateTimeOffset.UtcNow:O}\n" +
                $"Run ID: {_step35RunId}\n" +
                $"Process ID: {Environment.ProcessId}\n" +
                $"App version: {CurrentReleasePresentation.DisplayVersion} ({CurrentReleasePresentation.DisplayBuild})\n" +
                $"Expected source version: {CurrentReleasePresentation.ExpectedDisplayVersion} ({CurrentReleasePresentation.ExpectedBuildVersion})\n" +
                "Candidate: STEP 35.0.30 — same-run exact-source static map + GodotSharp callback-boundary five-mode closure candidate\n" +
                "This file is generated from the already-verified exact transformed image before CLR admission and is never consumed as runtime input.\n\n" +
                body + "\n");
            return true;
        }
        catch (Exception ex)
        {
            error = $"{ex.GetType().Name}: {ex.Message}";
            Console.Error.WriteLine($"Step-35 static-map write failed: {error}");
            return false;
        }
    }

    private void WriteStep35CrashCheckpoint(string detail)
    {
        try
        {
            lock (_step35CrashCheckpointSync)
            {
                if (!_step35RunTelemetryReady ||
                    string.IsNullOrWhiteSpace(_step35RunId) ||
                    string.IsNullOrWhiteSpace(_step35CrashCheckpointPath) ||
                    string.IsNullOrWhiteSpace(_step35StaticMapPath) ||
                    string.IsNullOrWhiteSpace(_step35GodotReconnaissancePath) ||
                    string.IsNullOrWhiteSpace(_step35LastCheckpointPath))
                {
                    Console.Error.WriteLine("Step-35 crash-checkpoint append skipped because run telemetry is not initialized.");
                    return;
                }

                var singleLine = (detail ?? string.Empty).Replace('\r', ' ').Replace('\n', ' ');
                var line = $"{DateTimeOffset.UtcNow:O} | run={_step35RunId} | pid={Environment.ProcessId} | managedThread={Environment.CurrentManagedThreadId} | {singleLine}";

                using (var stream = new FileStream(_step35CrashCheckpointPath, FileMode.Append, FileAccess.Write, FileShare.Read, 4096, FileOptions.None))
                using (var writer = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), 1024, leaveOpen: true))
                {
                    writer.WriteLine(line);
                    writer.Flush();
                    stream.Flush(flushToDisk: true);
                }

                WriteStep35TextFileDurably(
                    _step35LastCheckpointPath,
                    "StS2 Launcher — Step 35 last durable checkpoint\n" +
                    "Output-only diagnostic; overwrite-on-each-checkpoint convenience file.\n" +
                    $"Run ID: {_step35RunId}\n" +
                    $"Journal: {Path.GetFileName(_step35CrashCheckpointPath)}\n" +
                    $"Static map: {Path.GetFileName(_step35StaticMapPath)}\n" +
                    $"Godot/native reconnaissance: {Path.GetFileName(_step35GodotReconnaissancePath)}\n" +
                    line + "\n");
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Step-35 crash-checkpoint append failed: {ex.GetType().Name}: {ex.Message}");
        }
    }

    private static void WriteStep35TextFileDurably(string path, string body)
    {
        using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read, 4096, FileOptions.None);
        using var writer = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), 1024, leaveOpen: true);
        writer.Write(body);
        writer.Flush();
        stream.Flush(flushToDisk: true);
    }
}
