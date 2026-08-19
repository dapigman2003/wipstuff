using Foundation;
using StS2Launcher.Core;
using StS2Launcher.iOS.Platform;
using UIKit;

namespace StS2Launcher.iOS;

public sealed partial class RootViewController
{
    private async Task RunGodotFoundationGatesABCAsync()
    {
        if (_godotFoundationResultLabel is null ||
            _godotFoundationDetailLabel is null ||
            _godotFoundationStartButton is null ||
            _godotFoundationGateDButton is null ||
            _godotHostContainer is null ||
            _statusLabel is null)
        {
            return;
        }

        if (_godotSessionStarted || _godotProcessRequiresRestart)
        {
            _statusLabel.Text = "A Step 15 Godot start has already touched process-global engine state. Finish Gate D if available, or force-quit/relaunch before another attempt.";
            _statusLabel.TextColor = UIColor.SystemOrange;
            return;
        }

        BeginSteamOperation(allowCancel: false);
        _godotFoundationGates.Reset();
        _godotHostContainer.Hidden = false;
        _godotFoundationResultLabel.Text = "GODOT FOUNDATION: GATE A RUNNING…";
        _godotFoundationResultLabel.TextColor = UIColor.Label;
        _godotFoundationDetailLabel.Text = "Gate A: resolving the statically linked Godot 4.5.1 native bridge. Later gates will not run if this gate fails.";
        _statusLabel.Text = "STEP 15 GATE A — native Godot availability/linkage.";
        _statusLabel.TextColor = UIColor.Label;

        try
        {
            string engineVersion;
            try
            {
                engineVersion = GodotStep15NativeBridge.EngineVersion;
            }
            catch (Exception ex)
            {
                RecordGodotGate(GodotFoundationGate.NativeAvailability, false,
                    $"Native bridge resolution failed: {ex.GetType().Name}: {ex.Message}");
                return;
            }

            if (!string.Equals(engineVersion, "4.5.1-stable", StringComparison.Ordinal))
            {
                RecordGodotGate(GodotFoundationGate.NativeAvailability, false,
                    $"Expected Godot 4.5.1-stable, native bridge reported '{engineVersion}'.");
                return;
            }

            RecordGodotGate(GodotFoundationGate.NativeAvailability, true,
                $"Native static bridge resolved and reported Godot {engineVersion}.");

            _godotFoundationResultLabel.Text = "GODOT FOUNDATION: GATE B RUNNING…";
            _godotFoundationDetailLabel.Text = FormatGodotFoundationDetail(
                "Gate B: initializing Godot with the project-owned smoke project, proving the render loop can stop and restart, then leaving it running for Gate C.");
            _statusLabel.Text = "STEP 15 GATE B — engine initialize + render-loop stop/restart.";

            var smokeProjectPath = Path.Combine(NSBundle.MainBundle.BundlePath, "Step15GodotSmokeProject");
            if (!File.Exists(Path.Combine(smokeProjectPath, "project.godot")))
            {
                RecordGodotGate(GodotFoundationGate.EngineInitializeRenderLoop, false,
                    $"Bundled smoke project missing: {smokeProjectPath}");
                return;
            }

            // Ensure the arranged subview has non-zero UIKit bounds before the
            // native bridge creates Godot's Metal-backed view.
            View?.LayoutIfNeeded();
            _godotHostContainer.LayoutIfNeeded();
            if (_godotHostContainer.Bounds.Width < 1 || _godotHostContainer.Bounds.Height < 1)
            {
                RecordGodotGate(GodotFoundationGate.EngineInitializeRenderLoop, false,
                    $"Godot host container has invalid bounds: {_godotHostContainer.Bounds}.");
                return;
            }

            var startResult = GodotStep15NativeBridge.Start(Handle, _godotHostContainer.Handle, smokeProjectPath);
            _godotProcessRequiresRestart = GodotStep15NativeBridge.RequiresProcessRestart;
            if (startResult != 0 || !GodotStep15NativeBridge.IsEngineStarted)
            {
                RecordGodotGate(GodotFoundationGate.EngineInitializeRenderLoop, false,
                    $"Native Godot start failed ({startResult}): {GodotStep15NativeBridge.LastError}" +
                    (_godotProcessRequiresRestart ? " Force-quit/relaunch before another launcher operation." : string.Empty));
                return;
            }

            _godotSessionStarted = true;
            var stopped = GodotStep15NativeBridge.StopRendering();
            var restarted = GodotStep15NativeBridge.StartRendering();
            if (!stopped || !restarted || !GodotStep15NativeBridge.IsRenderingActive)
            {
                RecordGodotGate(GodotFoundationGate.EngineInitializeRenderLoop, false,
                    $"Engine started but render-loop control failed. stop={stopped}, restart={restarted}, active={GodotStep15NativeBridge.IsRenderingActive}.");
                return;
            }

            RecordGodotGate(GodotFoundationGate.EngineInitializeRenderLoop, true,
                "Godot initialized from the bundled project; CADisplayLink render loop stopped and restarted successfully.");

            _godotFoundationResultLabel.Text = "GODOT FOUNDATION: GATE C RUNNING…";
            _godotFoundationDetailLabel.Text = FormatGodotFoundationDetail(
                "Gate C: waiting for Godot setup2/start, a Metal-backed rendering layer, and the project-owned scene's fresh render marker.");
            _statusLabel.Text = "STEP 15 GATE C — Metal smoke-scene render.";

            var gateCReady = await WaitForGodotConditionAsync(
                () => GodotStep15NativeBridge.IsSetupFinished &&
                      GodotStep15NativeBridge.IsMetalLayerReady &&
                      GodotStep15NativeBridge.IsRenderingActive &&
                      GodotStep15NativeBridge.RenderMarkerReady,
                TimeSpan.FromSeconds(30));

            if (!gateCReady)
            {
                RecordGodotGate(GodotFoundationGate.MetalRender, false,
                    $"Timed out waiting for Metal smoke scene. setup={GodotStep15NativeBridge.IsSetupFinished}, metal={GodotStep15NativeBridge.IsMetalLayerReady}, active={GodotStep15NativeBridge.IsRenderingActive}, marker={GodotStep15NativeBridge.RenderMarkerReady}, nativeError='{GodotStep15NativeBridge.LastError}'.");
                return;
            }

            RecordGodotGate(GodotFoundationGate.MetalRender, true,
                "Godot setup completed with a Metal rendering layer and the project-owned scene produced its render-ready marker.");

            _godotFoundationResultLabel.Text = "GODOT FOUNDATION IN PROGRESS — 3/4";
            _godotFoundationResultLabel.TextColor = UIColor.Label;
            _godotFoundationDetailLabel.Text = FormatGodotFoundationDetail(
                "Gates A–C PASS. Gate D is manual: tap inside the visible Godot panel until it turns green, send the app to the background once, return, then tap Verify Gate D.");
            _statusLabel.Text = "STEP 15 GATES A–C PASS. Complete the touch + background/foreground Gate D now. Do not run unrelated launcher tests until you relaunch after Step 15.";
            _statusLabel.TextColor = UIColor.Label;
        }
        catch (Exception ex)
        {
            try
            {
                _godotProcessRequiresRestart |= GodotStep15NativeBridge.RequiresProcessRestart;
            }
            catch
            {
                // If native bridge telemetry itself is unavailable, preserve the original exception.
            }
            var nextGate = (GodotFoundationGate)Math.Min(_godotFoundationGates.Results.Count + 1, 4);
            if (_godotFoundationGates.Snapshot().FirstFailingGate is null && _godotFoundationGates.Results.Count < 4)
            {
                try
                {
                    _godotFoundationGates.Record(nextGate, false, $"Unhandled {ex.GetType().Name}: {ex.Message}");
                }
                catch
                {
                    // Preserve the original exception in the UI if gate accounting itself cannot advance.
                }
            }
            _godotFoundationResultLabel.Text = _godotFoundationGates.Snapshot().Summary;
            _godotFoundationResultLabel.TextColor = UIColor.SystemRed;
            _godotFoundationDetailLabel.Text = FormatGodotFoundationDetail($"Unhandled Step 15 exception: {ex.GetType().Name}: {ex.Message}");
            _statusLabel.Text = "STEP 15 FAIL: stop at the first failing Godot Foundation gate and report this screen.";
            _statusLabel.TextColor = UIColor.SystemRed;
        }
        finally
        {
            await WriteDeviceTestReportFromLabelsAsync(
                "Step15-GodotFoundation.txt",
                "StS2 Launcher — Step 15 Godot Foundation",
                _godotFoundationResultLabel,
                _godotFoundationDetailLabel,
                CancellationToken.None);
            EndSteamOperation();
        }
    }

    private async Task VerifyGodotFoundationGateDAsync()
    {
        if (_godotFoundationResultLabel is null ||
            _godotFoundationDetailLabel is null ||
            _godotFoundationGateDButton is null ||
            _statusLabel is null)
        {
            return;
        }

        var snapshot = _godotFoundationGates.Snapshot();
        if (!_godotSessionStarted || snapshot.FirstFailingGate is not null || snapshot.Results.Count != 3)
        {
            _statusLabel.Text = "Gate D is only available after Gates A–C pass in this process.";
            _statusLabel.TextColor = UIColor.SystemOrange;
            await WriteDeviceTestReportFromLabelsAsync(
                "Step15-GodotFoundation.txt",
                "StS2 Launcher — Step 15 Godot Foundation",
                _godotFoundationResultLabel,
                _godotFoundationDetailLabel,
                CancellationToken.None);
            return;
        }

        try
        {
            var touch = GodotStep15NativeBridge.TouchMarkerReady;
            var background = GodotStep15NativeBridge.BackgroundCount;
            var foreground = GodotStep15NativeBridge.ForegroundCount;
            var focusOut = GodotStep15NativeBridge.FocusOutCount;
            var focusIn = GodotStep15NativeBridge.FocusInCount;

            if (!touch || background < 1 || foreground < 1 || focusOut < 1 || focusIn < 1)
            {
                _godotFoundationResultLabel.Text = "GODOT FOUNDATION IN PROGRESS — 3/4";
                _godotFoundationResultLabel.TextColor = UIColor.SystemOrange;
                _godotFoundationDetailLabel.Text = FormatGodotFoundationDetail(
                    $"Gate D not complete yet. touch={YesNo(touch)}, background={background}, foreground={foreground}, focusOut={focusOut}, focusIn={focusIn}. Tap the Godot panel, background the app once, return, then verify again.");
                _statusLabel.Text = "STEP 15 GATE D PENDING — missing touch or lifecycle evidence; no failure recorded yet.";
                _statusLabel.TextColor = UIColor.SystemOrange;
                await WriteDeviceTestReportFromLabelsAsync(
                    "Step15-GodotFoundation.txt",
                    "StS2 Launcher — Step 15 Godot Foundation",
                    _godotFoundationResultLabel,
                    _godotFoundationDetailLabel,
                    CancellationToken.None);
                return;
            }

            _godotFoundationGates.Record(
                GodotFoundationGate.TouchLifecycle,
                true,
                $"Godot touch marker observed; lifecycle forwarding observed (background={background}, foreground={foreground}, focusOut={focusOut}, focusIn={focusIn}).");

            snapshot = _godotFoundationGates.Snapshot();
            _godotFoundationResultLabel.Text = snapshot.Summary;
            _godotFoundationResultLabel.TextColor = UIColor.Label;
            _godotFoundationDetailLabel.Text = FormatGodotFoundationDetail(
                "All four Step 15 ordered gates passed. Force-quit/relaunch before running the existing Foundation 5/5 regression; Step 15 does not attempt to execute any StS2 game content.");
            _statusLabel.Text = "PASS: STEP 15 GODOT FOUNDATION — 4/4. Native availability, engine/render-loop control, Metal project render, touch, and lifecycle are proven on this iPhone.";
            _statusLabel.TextColor = UIColor.Label;
            _godotFoundationGateDButton.Enabled = false;
        }
        catch (Exception ex)
        {
            _godotFoundationGates.Record(
                GodotFoundationGate.TouchLifecycle,
                false,
                $"Gate D native telemetry failed: {ex.GetType().Name}: {ex.Message}");
            _godotFoundationResultLabel.Text = _godotFoundationGates.Snapshot().Summary;
            _godotFoundationResultLabel.TextColor = UIColor.SystemRed;
            _godotFoundationDetailLabel.Text = FormatGodotFoundationDetail("Gate D failed while reading Godot touch/lifecycle telemetry.");
            _statusLabel.Text = "STEP 15 FAIL at Gate D. Stop and report this result; later work is not proven.";
            _statusLabel.TextColor = UIColor.SystemRed;
            _godotFoundationGateDButton.Enabled = false;
        }

        await WriteDeviceTestReportFromLabelsAsync(
            "Step15-GodotFoundation.txt",
            "StS2 Launcher — Step 15 Godot Foundation",
            _godotFoundationResultLabel,
            _godotFoundationDetailLabel,
            CancellationToken.None);
    }

    private void RecordGodotGate(GodotFoundationGate gate, bool passed, string detail)
    {
        _godotFoundationGates.Record(gate, passed, detail);
        if (_godotFoundationResultLabel is not null)
        {
            _godotFoundationResultLabel.Text = _godotFoundationGates.Snapshot().Summary;
            _godotFoundationResultLabel.TextColor = passed ? UIColor.Label : UIColor.SystemRed;
        }
        if (_godotFoundationDetailLabel is not null)
            _godotFoundationDetailLabel.Text = FormatGodotFoundationDetail(detail);
        if (!passed && _statusLabel is not null)
        {
            var letter = (char)('A' + (int)gate - 1);
            _statusLabel.Text = $"STEP 15 FAIL at Gate {letter} ({gate}). Stop here; later Godot gates were not run.";
            _statusLabel.TextColor = UIColor.SystemRed;
        }
    }

    private string FormatGodotFoundationDetail(string tail)
    {
        var lines = new List<string>();
        foreach (var gate in _godotFoundationGates.Results)
        {
            var letter = (char)('A' + (int)gate.Gate - 1);
            lines.Add($"Gate {letter} — {gate.Gate}: {(gate.Passed ? "PASS" : "FAIL")} — {gate.Detail}");
        }

        if (_godotSessionStarted || _godotProcessRequiresRestart)
        {
            lines.Add($"Process relaunch required before another Godot/unrelated launcher operation: {YesNo(_godotProcessRequiresRestart)}");
        }

        if (_godotSessionStarted)
        {
            lines.Add($"Native engine started: {YesNo(GodotStep15NativeBridge.IsEngineStarted)}");
            lines.Add($"Setup finished: {YesNo(GodotStep15NativeBridge.IsSetupFinished)}");
            lines.Add($"Metal layer ready: {YesNo(GodotStep15NativeBridge.IsMetalLayerReady)}");
            lines.Add($"Render loop active: {YesNo(GodotStep15NativeBridge.IsRenderingActive)}");
            lines.Add($"Render marker: {YesNo(GodotStep15NativeBridge.RenderMarkerReady)}");
            lines.Add($"Touch marker: {YesNo(GodotStep15NativeBridge.TouchMarkerReady)}");
            lines.Add($"Lifecycle counts: focusOut={GodotStep15NativeBridge.FocusOutCount}, background={GodotStep15NativeBridge.BackgroundCount}, foreground={GodotStep15NativeBridge.ForegroundCount}, focusIn={GodotStep15NativeBridge.FocusInCount}");
        }

        lines.Add("Step 15 project: launcher-owned smoke scene only; managed StS2 install is not loaded, rewritten, or executed.");
        lines.Add("Audio/game runtime/Cecil/FMOD/Spine/Steamworks integration: NOT TESTED BY STEP 15");
        lines.Add(tail);
        return string.Join("\n", lines);
    }

    private async Task<bool> WaitForGodotConditionAsync(Func<bool> condition, TimeSpan timeout)
    {
        var deadline = DateTimeOffset.UtcNow + timeout;
        while (DateTimeOffset.UtcNow < deadline)
        {
            if (EvaluateGodotConditionOnMainThread(condition))
                return true;
            await Task.Delay(100);
        }
        return EvaluateGodotConditionOnMainThread(condition);
    }

    private bool EvaluateGodotConditionOnMainThread(Func<bool> condition)
    {
        if (NSThread.IsMain)
            return condition();

        var result = false;
        InvokeOnMainThread(() => result = condition());
        return result;
    }
}
