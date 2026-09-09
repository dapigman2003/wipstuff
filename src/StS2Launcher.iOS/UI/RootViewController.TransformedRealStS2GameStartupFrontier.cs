using Foundation;
using StS2Launcher.Core;
using StS2Launcher.iOS.Platform;
using System.Text;

namespace StS2Launcher.iOS;

public sealed partial class RootViewController
{
    private readonly TransformedRealStS2GameStartupFrontierGateSequence _step41Gates = new();
    private UIButton? _step41GameStartupFrontierButton;
    private UILabel? _step41ResultLabel;
    private UILabel? _step41DetailLabel;
    private readonly object _step41CheckpointSync = new();
    private string? _step41RunId;
    private string? _step41CrashCheckpointPath;
    private string? _step41LastCheckpointPath;
    private string? _step41StaticMapPath;
    private bool _step41TelemetryReady;

    private void AddTransformedRealStS2GameStartupFrontierControls(UIStackView content)
    {
        content.AddArrangedSubview(Separator());
        content.AddArrangedSubview(Label(
            "Step 41.0 — GameStartup async frontier map (non-invoking)",
            UIFont.BoldSystemFontOfSize(18),
            UIColor.Label));

        _step41GameStartupFrontierButton = SystemButton(
            "Run Step 41.0 A–D — Verify frozen Step 40 → Map GameStartup/MoveNext → Map boundary closure → Confinement",
            16);
        _step41GameStartupFrontierButton.TouchUpInside += async (_, _) => await RunTransformedRealStS2GameStartupFrontierAsync();
        content.AddArrangedSubview(_step41GameStartupFrontierButton);

        _step41ResultLabel = Label(
            "GAMESTARTUP ASYNC FRONTIER MAP: NOT RUN",
            UIFont.BoldSystemFontOfSize(15),
            UIColor.SecondaryLabel);
        content.AddArrangedSubview(_step41ResultLabel);

        _step41DetailLabel = Label(
            "Physical 0.0.171 closed Step 40 at 4/4: the real in-tree NGame survived a controlled real render pulse and refreeze with state/native confinement intact. Step 41.0 does not invoke GameStartup. Gate A requires same-process Step-40 4/4 with rendering frozen. Gate B locates exact NGame.GameStartup, its AsyncStateMachineAttribute, compiler-generated state-machine type, MoveNext token/body, fields, and direct IL using deferred rejecting-resolver Cecil. Gate C maps the transitive same-sts2 MoveNext closure and records path-qualified platform/Steam/main-menu/deferred/FMOD/Spine/Sentry/native-facing references without executing them. Gate D proves rendering stayed frozen, GameStartupWrapper stayed inert, NGame authority/state=2 stayed intact, and no initializer/rejected/native runtime escape occurred. This is the mandatory map-before-enable step; GameStartup/InitializePlatform/Steam/main-menu/deferred remain unopened.",
            UIFont.SystemFontOfSize(13),
            UIColor.SecondaryLabel);
        content.AddArrangedSubview(_step41DetailLabel);
    }

    private async Task RunTransformedRealStS2GameStartupFrontierAsync()
    {
        if (_step41GameStartupFrontierButton is null || _step41ResultLabel is null || _step41DetailLabel is null || _statusLabel is null)
            return;

        if (!CurrentReleasePresentation.BundleIdentityMatchesExpected)
        {
            _step41ResultLabel.Text = "GAMESTARTUP ASYNC FRONTIER MAP: RELEASE IDENTITY FAIL";
            _step41ResultLabel.TextColor = UIColor.SystemRed;
            _statusLabel.Text = "STEP 41 REFUSED — built bundle identity does not match the source-pinned candidate.";
            _statusLabel.TextColor = UIColor.SystemRed;
            return;
        }

        if (!_step40Gates.Snapshot().Passed || !_transformedRealStS2VeryEarlyInitialization.ExactStep40ClosurePassed)
        {
            _step41ResultLabel.Text = "GAMESTARTUP ASYNC FRONTIER MAP: STEP 40 PREREQUISITE NOT MET";
            _step41ResultLabel.TextColor = UIColor.SystemOrange;
            _step41DetailLabel.Text = "Step 41.0 requires Step 40.1 4/4 in this same process, with the real NGame retained in-tree and rendering frozen again. Fresh launch sequence: Step 15 A-C → Step 35.0.32 4/4 → Step 36.0.5 4/4 → Step 37.0.1 4/4 → skip Step 38 → Step 39.0 4/4 → Step 40.1 4/4 → Step 41.0.";
            _statusLabel.Text = "STEP 41.0 REFUSED — complete Step 40.1 4/4 in this process first.";
            _statusLabel.TextColor = UIColor.SystemOrange;
            return;
        }

        if (GodotStep15NativeBridge.IsRenderingActive)
        {
            _step41ResultLabel.Text = "GAMESTARTUP ASYNC FRONTIER MAP: RENDERER NOT FROZEN";
            _step41ResultLabel.TextColor = UIColor.SystemRed;
            _step41DetailLabel.Text = "Step 41.0 is metadata-only and requires rendering to remain stopped after Step 40. Do not map startup from an active renderer.";
            _statusLabel.Text = "STEP 41.0 REFUSED — rendering is active.";
            _statusLabel.TextColor = UIColor.SystemRed;
            return;
        }

        if (!TryInitializeStep41Telemetry(out var telemetryError))
        {
            _step41ResultLabel.Text = "GAMESTARTUP ASYNC FRONTIER MAP: TELEMETRY FAIL / NOT RUN";
            _step41ResultLabel.TextColor = UIColor.SystemRed;
            _step41DetailLabel.Text = telemetryError;
            _statusLabel.Text = "STEP 41 REFUSED — durable run telemetry could not be established.";
            _statusLabel.TextColor = UIColor.SystemRed;
            return;
        }

        BeginSteamOperation(allowCancel: false);
        _step41Gates.Reset();
        _step41ResultLabel.Text = "GAMESTARTUP ASYNC FRONTIER MAP: GATE A RUNNING…";
        _step41ResultLabel.TextColor = UIColor.Label;
        _statusLabel.Text = $"STEP 41.0 RUN {_step41RunId} — Step 40 is closed in-process and rendering is frozen. No GameStartup invocation is authorized.";
        _statusLabel.TextColor = UIColor.Label;

        try
        {
            WriteStep41Checkpoint("RUN_START — Step 41.0 non-invoking GameStartup async frontier map started from same-process Step-40 4/4 frozen authority. GameStartup is not invoked and rendering is not restarted.");

            var gateA = _transformedRealStS2VeryEarlyInitialization.RunStep41ClosedStep40FrozenAuthority(
                !GodotStep15NativeBridge.IsRenderingActive,
                WriteStep41Checkpoint);
            if (!RecordStep41Gate(gateA))
                return;

            _step41ResultLabel.Text = "GAMESTARTUP ASYNC FRONTIER MAP: GATE B RUNNING…";
            _statusLabel.Text = "STEP 41.0 GATE B — map exact GameStartup + AsyncStateMachine + MoveNext metadata/IL with rendering frozen; do not invoke it.";
            var gateB = _transformedRealStS2VeryEarlyInitialization.RunStep41GameStartupAsyncStateMachineMap(WriteStep41Checkpoint);
            if (!RecordStep41Gate(gateB))
                return;

            _step41ResultLabel.Text = "GAMESTARTUP ASYNC FRONTIER MAP: GATE C RUNNING…";
            _statusLabel.Text = "STEP 41.0 GATE C — traverse the same-sts2 MoveNext closure and classify startup/platform/Steam/native boundary paths without execution.";
            var gateC = _transformedRealStS2VeryEarlyInitialization.RunStep41TransitiveStartupBoundaryMap(WriteStep41Checkpoint);
            if (!RecordStep41Gate(gateC))
                return;
            if (!WriteStep41StaticMap(out var staticMapError))
                throw new IOException("Step 41.0 verified GameStartup frontier static map could not be durably written: " + staticMapError);
            WriteStep41Checkpoint("K_C_STATIC_MAP_WRITE_RETURNED — verified Step-41 GameStartup/MoveNext IL + transitive boundary map durably written; GameStartup still not invoked; rendering still stopped.");

            _step41ResultLabel.Text = "GAMESTARTUP ASYNC FRONTIER MAP: GATE D RUNNING…";
            _statusLabel.Text = "STEP 41.0 GATE D — prove the mapping was runtime-inert: renderer frozen, GameStartupWrapper inert, NGame authority/state preserved, zero native/initializer/rejected escape.";
            var gateD = _transformedRealStS2VeryEarlyInitialization.RunStep41FrozenNoInvocationConfinement(
                !GodotStep15NativeBridge.IsRenderingActive,
                WriteStep41Checkpoint);
            if (!RecordStep41Gate(gateD))
                return;

            var snapshot = _step41Gates.Snapshot();
            _step41ResultLabel.Text = snapshot.Summary;
            _step41ResultLabel.TextColor = UIColor.Label;
            _step41DetailLabel.Text =
                "All four Step 41.0 gates passed. Same-process Step-40 frozen authority was reverified; exact NGame.GameStartup and its compiler async state-machine MoveNext IL were mapped from the selected compatibility image with zero external Cecil resolution; the transitive same-sts2 startup closure and path-qualified platform/Steam/main-menu/deferred/FMOD/Spine/Sentry/native-facing references were recorded; and final frozen NGame/state/native confinement held. GameStartup itself was never invoked. The static map is the authority for designing the next separately bounded startup transform/invocation step.";
            _statusLabel.Text = "STEP 41.0 COMPLETE — 4/4. GameStartup async frontier is mapped without invocation. Preserve Step41 artifacts; use the static map to choose the next exact startup boundary.";
            _statusLabel.TextColor = UIColor.Label;
            WriteStep41Checkpoint("RUN_STEP41_4OF4 — exact GameStartup async state-machine and transitive startup boundary map completed under frozen no-invocation confinement; GameStartup/platform/Steam/main-menu/deferred/native execution remains unopened.");
        }
        catch (Exception ex)
        {
            WriteStep41Checkpoint($"RUN_MANAGED_EXCEPTION — {ex.GetType().FullName}: {SanitizeStep41Checkpoint(ex.Message)}");
            _step41ResultLabel.Text = "GAMESTARTUP ASYNC FRONTIER MAP: EXCEPTION";
            _step41ResultLabel.TextColor = UIColor.SystemRed;
            _step41DetailLabel.Text = $"Unhandled Step 41.0 exception: {ex.GetType().Name}: {ex.Message}";
            _statusLabel.Text = "STEP 41 EXCEPTION — preserve Step41 reports. No GameStartup invocation was authorized by this step.";
            _statusLabel.TextColor = UIColor.SystemRed;
        }
        finally
        {
            WriteStep41Checkpoint($"RUN_FINALLY_ENTER — Step-41 managed control reached finally; managedThread={Environment.CurrentManagedThreadId}; isMain={NSThread.IsMain}; renderingActive={GodotStep15NativeBridge.IsRenderingActive}.");
            await WriteDeviceTestReportFromLabelsAsync(
                "Step41-TransformedRealStS2GameStartupFrontier.txt",
                "StS2 Launcher — Step 41.0 GameStartup Async Frontier Map",
                _step41ResultLabel,
                _step41DetailLabel,
                CancellationToken.None).ConfigureAwait(false);
            WriteStep41Checkpoint("RUN_NORMAL_REPORT_RETURNED — Step41 report writer returned.");
            if (NSThread.IsMain)
                EndSteamOperation();
            else
                InvokeOnMainThread(EndSteamOperation);
            WriteStep41Checkpoint("RUN_END — Step-41 UI operation ended normally; GameStartup was never invoked and Step 41 never restarted rendering.");
        }
    }

    private bool RecordStep41Gate(TransformedRealStS2GameStartupFrontierGateResult result)
    {
        _step41Gates.Record(result);
        if (_step41ResultLabel is not null)
        {
            _step41ResultLabel.Text = _step41Gates.Snapshot().Summary;
            _step41ResultLabel.TextColor = result.Passed ? UIColor.Label : UIColor.SystemRed;
        }
        if (_step41DetailLabel is not null)
            _step41DetailLabel.Text = result.Detail;
        if (!result.Passed && _statusLabel is not null)
        {
            var letter = (char)('A' + (int)result.Gate - 1);
            _statusLabel.Text = $"STEP 41.0 FAIL at Gate {letter} ({result.Gate}). Stop here; later gates were not run. Preserve Step41 artifacts; no GameStartup invocation was authorized.";
            _statusLabel.TextColor = UIColor.SystemRed;
        }
        return result.Passed;
    }

    private bool TryInitializeStep41Telemetry(out string error)
    {
        error = string.Empty;
        lock (_step41CheckpointSync)
        {
            _step41TelemetryReady = false;
            _step41RunId = null;
            _step41CrashCheckpointPath = null;
            _step41LastCheckpointPath = null;
            _step41StaticMapPath = null;
            try
            {
                Directory.CreateDirectory(_deviceTestReportWriter.ReportsRoot);
                var now = DateTimeOffset.UtcNow;
                var runId = $"{now:yyyyMMddTHHmmssfffffffZ}-pid{Environment.ProcessId}-{Guid.NewGuid():N}";
                var crashName = $"Step41-CrashCheckpoint-{runId}.txt";
                var staticName = $"Step41-GameStartupFrontier-StaticMap-{runId}.txt";
                var crashPath = Path.Combine(_deviceTestReportWriter.ReportsRoot, crashName);
                var lastPath = Path.Combine(_deviceTestReportWriter.ReportsRoot, "Step41-LastCheckpoint.txt");
                var staticPath = Path.Combine(_deviceTestReportWriter.ReportsRoot, staticName);
                WriteStep35TextFileDurably(
                    crashPath,
                    "StS2 Launcher — Step 41.0 GameStartup async frontier checkpoint\n" +
                    "Output-only diagnostic; never consumed as trusted runtime input.\n" +
                    $"Run ID: {runId}\n" +
                    $"Initialized UTC: {now:O}\n" +
                    $"Process ID: {Environment.ProcessId}\n" +
                    $"App version: {CurrentReleasePresentation.DisplayVersion} ({CurrentReleasePresentation.DisplayBuild})\n" +
                    "Candidate: STEP 41.0 — CLOSED STEP-40 FROZEN AUTHORITY + EXACT GAMESTARTUP ASYNC STATEMACHINE MAP + TRANSITIVE STARTUP BOUNDARY MAP + NO-INVOCATION CONFINEMENT\n" +
                    "Prerequisite: same-process Step-40 4/4 with real NGame retained in-tree and renderer stopped. GameStartup/InitializePlatform/main-menu/ExecuteDeferred/Steam/native game GDExtension execution remains forbidden.\n\n");
                _step41RunId = runId;
                _step41CrashCheckpointPath = crashPath;
                _step41LastCheckpointPath = lastPath;
                _step41StaticMapPath = staticPath;
                _step41TelemetryReady = true;
                WriteStep41Checkpoint("RUN_TELEMETRY_READY — Step41 run journal created and durably flushed before Gate A.");
                return true;
            }
            catch (Exception ex)
            {
                error = $"{ex.GetType().Name}: {ex.Message}";
                return false;
            }
        }
    }

    private void WriteStep41Checkpoint(string detail)
    {
        if (!_step41TelemetryReady || string.IsNullOrWhiteSpace(_step41RunId) ||
            string.IsNullOrWhiteSpace(_step41CrashCheckpointPath) || string.IsNullOrWhiteSpace(_step41LastCheckpointPath))
            return;
        try
        {
            lock (_step41CheckpointSync)
            {
                var single = SanitizeStep41Checkpoint(detail);
                var line = $"{DateTimeOffset.UtcNow:O} | run={_step41RunId} | pid={Environment.ProcessId} | managedThread={Environment.CurrentManagedThreadId} | {single}";
                using (var stream = new FileStream(_step41CrashCheckpointPath, FileMode.Append, FileAccess.Write, FileShare.Read, 4096, FileOptions.None))
                using (var writer = new StreamWriter(stream, new UTF8Encoding(false), 1024, leaveOpen: true))
                {
                    writer.WriteLine(line);
                    writer.Flush();
                    stream.Flush(flushToDisk: true);
                }
                WriteStep35TextFileDurably(
                    _step41LastCheckpointPath,
                    "StS2 Launcher — Step 41 last durable checkpoint\n" +
                    "Output-only diagnostic; overwrite-on-each-checkpoint convenience file.\n" +
                    $"Run ID: {_step41RunId}\n" +
                    $"Journal: {Path.GetFileName(_step41CrashCheckpointPath)}\n" +
                    $"Static map: {Path.GetFileName(_step41StaticMapPath)}\n" +
                    line + "\n");
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Step-41 checkpoint append failed: {ex.GetType().Name}: {ex.Message}");
        }
    }

    private bool WriteStep41StaticMap(out string error)
    {
        error = string.Empty;
        try
        {
            if (!_step41TelemetryReady || string.IsNullOrWhiteSpace(_step41StaticMapPath))
                throw new InvalidOperationException("Step41 telemetry/static-map path is not initialized.");
            WriteStep35TextFileDurably(
                _step41StaticMapPath,
                _transformedRealStS2VeryEarlyInitialization.GetVerifiedStep41StaticMap());
            return true;
        }
        catch (Exception ex)
        {
            error = $"{ex.GetType().Name}: {ex.Message}";
            return false;
        }
    }

    private static string SanitizeStep41Checkpoint(string? detail)
        => (detail ?? string.Empty).Replace('\r', ' ').Replace('\n', ' ');
}
