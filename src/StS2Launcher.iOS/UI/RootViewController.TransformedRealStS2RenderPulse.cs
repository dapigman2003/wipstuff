using Foundation;
using StS2Launcher.Core;
using StS2Launcher.iOS.Platform;
using System.Diagnostics;
using System.Text;

namespace StS2Launcher.iOS;

public sealed partial class RootViewController
{
    private readonly TransformedRealStS2RenderPulseGateSequence _step40Gates = new();
    private UIButton? _step40RenderPulseButton;
    private UILabel? _step40ResultLabel;
    private UILabel? _step40DetailLabel;
    private readonly object _step40CheckpointSync = new();
    private string? _step40RunId;
    private string? _step40CrashCheckpointPath;
    private string? _step40LastCheckpointPath;
    private string? _step40StaticMapPath;
    private bool _step40TelemetryReady;
    private bool _step40PulseUiStarted;

    private void AddTransformedRealStS2RenderPulseControls(UIStackView content)
    {
        content.AddArrangedSubview(Separator());
        content.AddArrangedSubview(Label(
            "Step 40.1 — Controlled real-game render pulse",
            UIFont.BoldSystemFontOfSize(18),
            UIColor.Label));

        _step40RenderPulseButton = SystemButton(
            $"Run Step 40.1 A–D — Audit frame callbacks → Render ~{TransformedRealStS2VeryEarlyInitialization.Step40RenderPulseTargetMilliseconds} ms → Refreeze → Confinement",
            16);
        _step40RenderPulseButton.TouchUpInside += async (_, _) => await RunTransformedRealStS2RenderPulseAsync();
        content.AddArrangedSubview(_step40RenderPulseButton);

        _step40ResultLabel = Label(
            "CONTROLLED REAL-GAME RENDER PULSE: NOT RUN",
            UIFont.BoldSystemFontOfSize(15),
            UIColor.SecondaryLabel);
        content.AddArrangedSubview(_step40ResultLabel);

        _step40DetailLabel = Label(
            "Physical 0.0.170 closed Step 39 at 4/4: the real NGame hierarchy is attached to SceneTree.Root with exact singleton/_window/parent authority, state 2, and rendering synchronously frozen. Step 40.1 is the only authorized same-process continuation. Gate A re-proves that frozen Step-39 authority and the inert GameStartupWrapper. Gate B maps the actual in-tree sts2 _Process/_PhysicsProcess/_Draw/input callback surface with deferred rejecting-resolver Cecil; Step-39's already-clean branch-insensitive _Notification closure remains prerequisite authority. Gate C starts the existing Godot CADisplayLink exactly once, requests a 100 ms stop delay, and the first managed continuation immediately calls StopRendering before telemetry; a cold long first frame may postpone that continuation, so post-stop evidence is accepted through 2000 ms while the actual observed overshoot is reported. Gate D requires rendering frozen again and the inserted NGame/state/native confinement unchanged. GameStartup, InitializePlatform, main menu, ExecuteDeferred, Steam, FMOD/Spine/native game GDExtensions, explicit _ExitTree, RemoveChild/Free, and leaving rendering active remain forbidden. Do not retry Step 40 in-process once Gate C is armed.",
            UIFont.SystemFontOfSize(13),
            UIColor.SecondaryLabel);
        content.AddArrangedSubview(_step40DetailLabel);
    }

    private async Task RunTransformedRealStS2RenderPulseAsync()
    {
        if (_step40RenderPulseButton is null || _step40ResultLabel is null || _step40DetailLabel is null || _statusLabel is null)
            return;

        if (!CurrentReleasePresentation.BundleIdentityMatchesExpected)
        {
            _step40ResultLabel.Text = "CONTROLLED REAL-GAME RENDER PULSE: RELEASE IDENTITY FAIL";
            _step40ResultLabel.TextColor = UIColor.SystemRed;
            _statusLabel.Text = "STEP 40 REFUSED — built bundle identity does not match the source-pinned candidate.";
            _statusLabel.TextColor = UIColor.SystemRed;
            return;
        }

        if (!_step39Gates.Snapshot().Passed || !_transformedRealStS2VeryEarlyInitialization.ExactStep39ClosurePassed)
        {
            _step40ResultLabel.Text = "CONTROLLED REAL-GAME RENDER PULSE: STEP 39 PREREQUISITE NOT MET";
            _step40ResultLabel.TextColor = UIColor.SystemOrange;
            _step40DetailLabel.Text = "Step 40.1 requires Step 39.0 4/4 in this same process, because Step 39 intentionally retains the real NGame in-tree with rendering frozen. Fresh launch sequence: Step 15 A-C → Step 35.0.32 MODEL-BOOTSTRAP 4/4 → Step 36.0.5 4/4 → Step 37.0.1 4/4 → skip Step 38 → Step 39.0 4/4 → Step 40.1 once.";
            _statusLabel.Text = "STEP 40.1 REFUSED — complete Step 39.0 4/4 in this process first.";
            _statusLabel.TextColor = UIColor.SystemOrange;
            return;
        }

        if (_step40PulseUiStarted)
        {
            _step40ResultLabel.Text = "CONTROLLED REAL-GAME RENDER PULSE: FRESH PROCESS REQUIRED";
            _step40ResultLabel.TextColor = UIColor.SystemOrange;
            _step40DetailLabel.Text = "A Step-40 render pulse has already been armed in this process. Preserve reports and relaunch; do not retry StartRendering after the first Step-40 pulse attempt.";
            _statusLabel.Text = "STEP 40.1 REFUSED — render pulse already attempted in this process.";
            _statusLabel.TextColor = UIColor.SystemOrange;
            return;
        }

        if (GodotStep15NativeBridge.IsRenderingActive)
        {
            _step40ResultLabel.Text = "CONTROLLED REAL-GAME RENDER PULSE: RENDERER NOT FROZEN";
            _step40ResultLabel.TextColor = UIColor.SystemRed;
            _step40DetailLabel.Text = "Step 40.1 requires the renderer to still be stopped by Step 39. Do not run a pulse from an already-active renderer; preserve reports and relaunch.";
            _statusLabel.Text = "STEP 40.1 REFUSED — rendering is already active.";
            _statusLabel.TextColor = UIColor.SystemRed;
            return;
        }

        if (!TryInitializeStep40Telemetry(out var telemetryError))
        {
            _step40ResultLabel.Text = "CONTROLLED REAL-GAME RENDER PULSE: TELEMETRY FAIL / NOT RUN";
            _step40ResultLabel.TextColor = UIColor.SystemRed;
            _step40DetailLabel.Text = telemetryError;
            _statusLabel.Text = "STEP 40 REFUSED — durable run telemetry could not be established.";
            _statusLabel.TextColor = UIColor.SystemRed;
            return;
        }

        BeginSteamOperation(allowCancel: false);
        _step40Gates.Reset();
        _step40ResultLabel.Text = "CONTROLLED REAL-GAME RENDER PULSE: GATE A RUNNING…";
        _step40ResultLabel.TextColor = UIColor.Label;
        _statusLabel.Text = $"STEP 40.1 RUN {_step40RunId} — same-process Step 39 is closed 4/4 and renderer is frozen. Gate A re-verifies retained authority before any restart.";
        _statusLabel.TextColor = UIColor.Label;

        try
        {
            WriteStep40Checkpoint("RUN_START — Step 40.1 controlled real-game render pulse started from same-process Step-39 4/4 frozen authority. No render restart has occurred yet.");

            var gateA = _transformedRealStS2VeryEarlyInitialization.RunStep40ClosedStep39FrozenAuthority(
                !GodotStep15NativeBridge.IsRenderingActive,
                WriteStep40Checkpoint);
            if (!RecordStep40Gate(gateA))
                return;

            _step40ResultLabel.Text = "CONTROLLED REAL-GAME RENDER PULSE: GATE B RUNNING…";
            _statusLabel.Text = "STEP 40.1 GATE B — renderer remains frozen while the actual in-tree hierarchy's frame/input managed callbacks are Cecil-mapped fail-closed.";
            WriteStep40Checkpoint($"J_B_UI_SELECTED — Gate B selected on UI thread; managedThread={Environment.CurrentManagedThreadId}; isMain={NSThread.IsMain}; renderingActive={GodotStep15NativeBridge.IsRenderingActive}.");
            var gateB = _transformedRealStS2VeryEarlyInitialization.RunStep40FrameDrivenManagedSurfaceAudit(WriteStep40Checkpoint);
            if (!RecordStep40Gate(gateB))
                return;
            if (!WriteStep40StaticMap(out var staticMapError))
                throw new IOException("Step 40.1 verified frame-driven static map could not be durably written: " + staticMapError);
            WriteStep40Checkpoint("J_B_STATIC_MAP_WRITE_RETURNED — verified Step-40 in-tree frame/input static map durably written before StartRendering.");

            _step40ResultLabel.Text = "CONTROLLED REAL-GAME RENDER PULSE: GATE C RUNNING…";
            _statusLabel.Text = $"STEP 40.1 GATE C — first/only controlled render pulse. StartRendering once, request stop after {TransformedRealStS2VeryEarlyInitialization.Step40RenderPulseTargetMilliseconds} ms, then synchronously refreeze at the first managed main-thread opportunity before evaluating the result.";
            if (!NSThread.IsMain)
                throw new InvalidOperationException("Step 40.1 Gate C must arm StartRendering on the UIKit main thread.");

            _transformedRealStS2VeryEarlyInitialization.BeginStep40BoundedRenderPulse(WriteStep40Checkpoint);
            _step40PulseUiStarted = true;
            WriteStep40Checkpoint($"J_C_START_RENDERING_CALL — invoking StartRendering exactly once; managedThread={Environment.CurrentManagedThreadId}; isMain={NSThread.IsMain}; renderingActiveBefore={GodotStep15NativeBridge.IsRenderingActive}.");
            var stopwatch = Stopwatch.StartNew();
            var startReturned = GodotStep15NativeBridge.StartRendering();
            var activeAfterStart = GodotStep15NativeBridge.IsRenderingActive;
            WriteStep40Checkpoint($"J_C_START_RENDERING_RETURNED — returned={startReturned}; renderingActiveAfterStart={activeAfterStart}; nativeError='{SanitizeStep40Checkpoint(GodotStep15NativeBridge.LastError)}'.");

            if (startReturned && activeAfterStart)
                await Task.Delay(TransformedRealStS2VeryEarlyInitialization.Step40RenderPulseTargetMilliseconds);

            // The delay continuation and Godot CADisplayLink both run on UIKit's main thread. A long first
            // Godot frame can therefore postpone this continuation. The first managed continuation performs
            // no checkpoint/file I/O before synchronously asking the main thread to stop rendering.
            var elapsedAtFirstContinuationMilliseconds = stopwatch.Elapsed.TotalMilliseconds;
            var stopObservation = await StopStep40RenderingOnMainThreadAsync();
            stopwatch.Stop();
            WriteStep40Checkpoint($"J_C_STOP_RENDERING_RETURNED — first managed continuation called StopRendering before telemetry; managedThread={Environment.CurrentManagedThreadId}; isMain={NSThread.IsMain}; elapsedAtFirstContinuationMs={elapsedAtFirstContinuationMilliseconds:F1}; returned={stopObservation.StopReturned}; renderingActiveAfterStop={stopObservation.ActiveAfterStop}; observedElapsedMs={stopwatch.Elapsed.TotalMilliseconds:F1}; overshootMs={stopwatch.Elapsed.TotalMilliseconds - TransformedRealStS2VeryEarlyInitialization.Step40RenderPulseTargetMilliseconds:F1}; nativeError='{SanitizeStep40Checkpoint(GodotStep15NativeBridge.LastError)}'. Rendering is never restarted by Step 40 after this point.");

            var gateC = _transformedRealStS2VeryEarlyInitialization.RunStep40BoundedRenderPulseEvidence(
                startReturned,
                activeAfterStart,
                stopObservation.StopReturned,
                stopObservation.ActiveAfterStop,
                stopwatch.Elapsed.TotalMilliseconds,
                WriteStep40Checkpoint);
            if (!RecordStep40Gate(gateC))
            {
                _statusLabel.Text = !stopObservation.ActiveAfterStop
                    ? "STEP 40.1 FAIL at Gate C — renderer is frozen again. Preserve Step40 reports and relaunch; do not retry the pulse in-process."
                    : "STEP 40.1 FAIL at Gate C — renderer could not be confirmed frozen. Preserve reports and relaunch immediately; do not continue this process.";
                _statusLabel.TextColor = UIColor.SystemRed;
                return;
            }

            _step40ResultLabel.Text = "CONTROLLED REAL-GAME RENDER PULSE: GATE D RUNNING…";
            _statusLabel.Text = "STEP 40.1 GATE D — rendering must remain frozen; verify retained NGame singleton/parent/_window/state authority and zero initializer/rejected/native escape after the pulse.";
            var gateD = _transformedRealStS2VeryEarlyInitialization.RunStep40FrozenPostPulseConfinement(
                !GodotStep15NativeBridge.IsRenderingActive,
                WriteStep40Checkpoint);
            if (!RecordStep40Gate(gateD))
                return;

            var snapshot = _step40Gates.Snapshot();
            _step40ResultLabel.Text = snapshot.Summary;
            _step40ResultLabel.TextColor = UIColor.Label;
            _step40DetailLabel.Text =
                "All four Step 40.1 gates passed. Same-process Step-39 frozen authority was reverified; the actual in-tree sts2 frame/input callback surface was Cecil-mapped with no forbidden/unresolved escape; the existing Godot CADisplayLink was restarted exactly once for the bounded pulse and synchronously stopped again; and final NGame singleton/parent/_window/state plus initializer/rejected/native confinement held with rendering frozen. GameStartup/platform/main-menu/ExecuteDeferred/Steam/native GDExtensions/gameplay remain future separately authorized boundaries.";
            _statusLabel.Text = "STEP 40.1 COMPLETE — 4/4. First controlled real-game render pulse closed with the hierarchy retained in-tree and rendering frozen. Preserve Step40 artifacts and relaunch before any later boundary.";
            _statusLabel.TextColor = UIColor.Label;
            WriteStep40Checkpoint("RUN_STEP40_4OF4 — controlled real-game render pulse completed; renderer refrozen; inserted NGame retained in-tree; GameStartup/platform/main-menu/deferred/Steam/native boundaries remain unopened.");
        }
        catch (Exception ex)
        {
            WriteStep40Checkpoint($"RUN_MANAGED_EXCEPTION — {ex.GetType().FullName}: {SanitizeStep40Checkpoint(ex.Message)}");
            _step40ResultLabel.Text = "CONTROLLED REAL-GAME RENDER PULSE: EXCEPTION";
            _step40ResultLabel.TextColor = UIColor.SystemRed;
            _step40DetailLabel.Text = $"Unhandled Step 40.1 exception: {ex.GetType().Name}: {ex.Message}";
            _statusLabel.Text = GodotStep15NativeBridge.IsRenderingActive
                ? "STEP 40 EXCEPTION — rendering is still active or unknown. Preserve reports and relaunch immediately; do not continue this process."
                : "STEP 40 EXCEPTION — rendering is frozen. Preserve Step40 reports and relaunch; do not retry the pulse in-process.";
            _statusLabel.TextColor = UIColor.SystemRed;
        }
        finally
        {
            WriteStep40Checkpoint($"RUN_FINALLY_ENTER — Step-40 managed control reached finally; managedThread={Environment.CurrentManagedThreadId}; isMain={NSThread.IsMain}; renderingActive={GodotStep15NativeBridge.IsRenderingActive}; pulseUiStarted={_step40PulseUiStarted}.");
            await WriteDeviceTestReportFromLabelsAsync(
                "Step40-TransformedRealStS2RenderPulse.txt",
                "StS2 Launcher — Step 40.1 Controlled Real-Game Render Pulse",
                _step40ResultLabel,
                _step40DetailLabel,
                CancellationToken.None).ConfigureAwait(false);
            WriteStep40Checkpoint("RUN_NORMAL_REPORT_RETURNED — Step40 report writer returned.");
            if (NSThread.IsMain)
                EndSteamOperation();
            else
                InvokeOnMainThread(EndSteamOperation);
            WriteStep40Checkpoint("RUN_END — Step-40 UI operation ended normally after explicit teardown; Step 40 never restarts rendering after its single StopRendering call.");
        }
    }

    private bool RecordStep40Gate(TransformedRealStS2RenderPulseGateResult result)
    {
        _step40Gates.Record(result);
        if (_step40ResultLabel is not null)
        {
            _step40ResultLabel.Text = _step40Gates.Snapshot().Summary;
            _step40ResultLabel.TextColor = result.Passed ? UIColor.Label : UIColor.SystemRed;
        }
        if (_step40DetailLabel is not null)
            _step40DetailLabel.Text = result.Detail;
        if (!result.Passed && _statusLabel is not null)
        {
            var letter = (char)('A' + (int)result.Gate - 1);
            _statusLabel.Text = $"STEP 40.1 FAIL at Gate {letter} ({result.Gate}). Stop here; later gates were not run. Preserve Step40 artifacts and relaunch before another render-pulse attempt.";
            _statusLabel.TextColor = UIColor.SystemRed;
        }
        return result.Passed;
    }

    private async Task<(bool StopReturned, bool ActiveAfterStop)> StopStep40RenderingOnMainThreadAsync()
    {
        if (NSThread.IsMain)
            return StopStep40RenderingNow();

        var completion = new TaskCompletionSource<(bool StopReturned, bool ActiveAfterStop)>(TaskCreationOptions.RunContinuationsAsynchronously);
        InvokeOnMainThread(() =>
        {
            try
            {
                completion.TrySetResult(StopStep40RenderingNow());
            }
            catch (Exception ex)
            {
                completion.TrySetException(ex);
            }
        });
        return await completion.Task.ConfigureAwait(false);
    }

    private static (bool StopReturned, bool ActiveAfterStop) StopStep40RenderingNow()
    {
        var stopReturned = GodotStep15NativeBridge.StopRendering();
        return (stopReturned, GodotStep15NativeBridge.IsRenderingActive);
    }

    private bool TryInitializeStep40Telemetry(out string error)
    {
        error = string.Empty;
        lock (_step40CheckpointSync)
        {
            _step40TelemetryReady = false;
            _step40RunId = null;
            _step40CrashCheckpointPath = null;
            _step40LastCheckpointPath = null;
            _step40StaticMapPath = null;
            try
            {
                Directory.CreateDirectory(_deviceTestReportWriter.ReportsRoot);
                var now = DateTimeOffset.UtcNow;
                var runId = $"{now:yyyyMMddTHHmmssfffffffZ}-pid{Environment.ProcessId}-{Guid.NewGuid():N}";
                var crashName = $"Step40-CrashCheckpoint-{runId}.txt";
                var staticName = $"Step40-RenderPulse-StaticMap-{runId}.txt";
                var crashPath = Path.Combine(_deviceTestReportWriter.ReportsRoot, crashName);
                var lastPath = Path.Combine(_deviceTestReportWriter.ReportsRoot, "Step40-LastCheckpoint.txt");
                var staticPath = Path.Combine(_deviceTestReportWriter.ReportsRoot, staticName);
                WriteStep35TextFileDurably(
                    crashPath,
                    "StS2 Launcher — Step 40.1 controlled real-game render-pulse checkpoint\n" +
                    "Output-only diagnostic; never consumed as trusted runtime input.\n" +
                    $"Run ID: {runId}\n" +
                    $"Initialized UTC: {now:O}\n" +
                    $"Process ID: {Environment.ProcessId}\n" +
                    $"App version: {CurrentReleasePresentation.DisplayVersion} ({CurrentReleasePresentation.DisplayBuild})\n" +
                    $"Candidate: STEP 40.1 — CLOSED STEP-39 AUTHORITY + FRAME/INPUT AUDIT + ONE ~{TransformedRealStS2VeryEarlyInitialization.Step40RenderPulseTargetMilliseconds}MS RENDER PULSE + SYNCHRONOUS REFREEZE + FROZEN CONFINEMENT\n" +
                    "Prerequisite: same-process Step-39.0 4/4 with real NGame retained in-tree and renderer stopped. GameStartup/InitializePlatform/main-menu/ExecuteDeferred/Steam/native game GDExtensions/explicit _ExitTree/RemoveChild/Free remain forbidden.\n\n");
                _step40RunId = runId;
                _step40CrashCheckpointPath = crashPath;
                _step40LastCheckpointPath = lastPath;
                _step40StaticMapPath = staticPath;
                _step40TelemetryReady = true;
                WriteStep40Checkpoint("RUN_TELEMETRY_READY — Step40 run journal created and durably flushed before Gate A.");
                return true;
            }
            catch (Exception ex)
            {
                error = $"{ex.GetType().Name}: {ex.Message}";
                return false;
            }
        }
    }

    private void WriteStep40Checkpoint(string detail)
    {
        if (!_step40TelemetryReady || string.IsNullOrWhiteSpace(_step40RunId) ||
            string.IsNullOrWhiteSpace(_step40CrashCheckpointPath) || string.IsNullOrWhiteSpace(_step40LastCheckpointPath))
            return;
        try
        {
            lock (_step40CheckpointSync)
            {
                var single = SanitizeStep40Checkpoint(detail);
                var line = $"{DateTimeOffset.UtcNow:O} | run={_step40RunId} | pid={Environment.ProcessId} | managedThread={Environment.CurrentManagedThreadId} | {single}";
                using (var stream = new FileStream(_step40CrashCheckpointPath, FileMode.Append, FileAccess.Write, FileShare.Read, 4096, FileOptions.None))
                using (var writer = new StreamWriter(stream, new UTF8Encoding(false), 1024, leaveOpen: true))
                {
                    writer.WriteLine(line);
                    writer.Flush();
                    stream.Flush(flushToDisk: true);
                }
                WriteStep35TextFileDurably(
                    _step40LastCheckpointPath,
                    "StS2 Launcher — Step 40 last durable checkpoint\n" +
                    "Output-only diagnostic; overwrite-on-each-checkpoint convenience file.\n" +
                    $"Run ID: {_step40RunId}\n" +
                    $"Journal: {Path.GetFileName(_step40CrashCheckpointPath)}\n" +
                    $"Static map: {Path.GetFileName(_step40StaticMapPath)}\n" +
                    line + "\n");
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Step-40 checkpoint append failed: {ex.GetType().Name}: {ex.Message}");
        }
    }

    private bool WriteStep40StaticMap(out string error)
    {
        error = string.Empty;
        try
        {
            if (!_step40TelemetryReady || string.IsNullOrWhiteSpace(_step40StaticMapPath))
                throw new InvalidOperationException("Step40 telemetry/static-map path is not initialized.");
            WriteStep35TextFileDurably(
                _step40StaticMapPath,
                _transformedRealStS2VeryEarlyInitialization.GetVerifiedStep40StaticMap());
            return true;
        }
        catch (Exception ex)
        {
            error = $"{ex.GetType().Name}: {ex.Message}";
            return false;
        }
    }

    private static string SanitizeStep40Checkpoint(string? detail)
        => (detail ?? string.Empty).Replace('\r', ' ').Replace('\n', ' ');
}
