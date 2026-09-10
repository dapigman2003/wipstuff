using Foundation;
using StS2Launcher.Core;
using StS2Launcher.iOS.Platform;
using System.Diagnostics;

namespace StS2Launcher.iOS;

public sealed partial class RootViewController
{
    private async Task RunStep48StartupLadderAsync()
    {
        const int step = 48;
        if (_step48InstantiationUiStarted)
        {
            var labels = GetStartupLadderLabels(step);
            SetStartupLadderRefusal(step, labels.Result, labels.Detail, "ONE-SHOT ALREADY ARMED", "Step 48 off-tree instantiation was already armed in this process. Preserve reports and relaunch; never retry.");
            return;
        }
        if (!TryPrepareStartupLadderStep(step, _step47Gates.Snapshot().Passed && _transformedRealStS2VeryEarlyInitialization.ExactStep47ClosurePassed,
                "Step 48.0 requires Step 47.0 4/4 exact main-menu PackedScene authority in this same process.", out var button, out var resultLabel, out var detailLabel))
            return;
        if (!TryInitializeStartupLadderTelemetry(step, "Off-tree NMainMenu instantiation", "Step48-MainMenuOffTree-StaticMap", out var error))
        {
            SetStartupLadderRefusal(step, resultLabel, detailLabel, "TELEMETRY FAIL / NOT RUN", error);
            return;
        }
        BeginSteamOperation(allowCancel: false);
        _step48Gates.Reset();
        try
        {
            WriteStartupLadderCheckpoint(step, "RUN_START — Step 48.0 one-shot off-tree real NMainMenu instantiation + actual lifecycle audit started; rendering remains frozen and no SceneTree admission is authorized yet.");
            if (!RecordStartupLadderGate(_step48Gates, _transformedRealStS2VeryEarlyInitialization.RunStep48ClosedStep47Authority(!GodotStep15NativeBridge.IsRenderingActive, d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!RecordStartupLadderGate(_step48Gates, _transformedRealStS2VeryEarlyInitialization.RunStep48ExactInstantiationBinding(d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!WriteStartupLadderStaticMap(step, out var preliminaryMapError)) throw new IOException("Step 48 preliminary guard-shape static map write failed before off-tree instantiation: " + preliminaryMapError);
            WriteStartupLadderCheckpoint(step, "M48_B_STATIC_MAP_WRITE_RETURNED — preliminary exact instantiation/guard-shape map durably written before the one-shot off-tree instance and runtime rehearsal. A successful Gate C will overwrite this same report with the actual hierarchy/lifecycle appendix.");
            _step48InstantiationUiStarted = true;
            button.Enabled = false;
            WriteStartupLadderCheckpoint(step, "M48_C_UI_ARMED — first/only off-tree NMainMenu PackedScene.Instantiate authorized; no in-process retry after this checkpoint.");
            if (!RecordStartupLadderGate(_step48Gates, _transformedRealStS2VeryEarlyInitialization.RunStep48OffTreeInstantiationAndLifecycleAudit(d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!WriteStartupLadderStaticMap(step, out var mapError)) throw new IOException("Step 48 actual off-tree hierarchy/lifecycle static map write failed: " + mapError);
            _transformedRealStS2VeryEarlyInitialization.MarkStep48StaticMapDurablyWritten();
            WriteStartupLadderCheckpoint(step, "M48_C_STATIC_MAP_WRITE_RETURNED — actual off-tree NMainMenu hierarchy + execution-qualified lifecycle map durably written before any Step-49 SceneTree admission.");
            if (!RecordStartupLadderGate(_step48Gates, _transformedRealStS2VeryEarlyInitialization.RunStep48FrozenOffTreeConfinement(!GodotStep15NativeBridge.IsRenderingActive, d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            CompleteStartupLadderStep(step, _step48Gates, resultLabel, detailLabel,
                "Real NMainMenu exists off-tree with an exact actual-node lifecycle map and frozen zero-native confinement. Step 49 frozen RootSceneContainer admission is unlocked.");
            WriteStartupLadderCheckpoint(step, "RUN_STEP48_4OF4 — real NMainMenu retained off-tree with durable lifecycle authority; Step 49 may AddChild it once while rendering remains frozen.");
        }
        catch (Exception ex)
        {
            HandleStartupLadderException(step, resultLabel, detailLabel, ex, mutationArmed: _step48InstantiationUiStarted);
        }
        finally
        {
            await FinishStartupLadderStepAsync(step, "Step48-TransformedRealStS2MainMenuOffTreeInstantiation.txt", "StS2 Launcher — Step 48.0 Main Menu Off-Tree Instantiation", resultLabel, detailLabel,
                "If off-tree instantiation was armed, never retry Step 48 in-process.");
        }
    }

    private async Task RunStep49StartupLadderAsync()
    {
        const int step = 49;
        if (_step49AdmissionUiStarted)
        {
            var labels = GetStartupLadderLabels(step);
            SetStartupLadderRefusal(step, labels.Result, labels.Detail, "ONE-SHOT ALREADY ARMED", "Step 49 AddChild boundary was already armed in this process. Preserve reports and relaunch; never retry.");
            return;
        }
        if (!TryPrepareStartupLadderStep(step, _step48Gates.Snapshot().Passed && _transformedRealStS2VeryEarlyInitialization.ExactStep48ClosurePassed,
                "Step 49.0 requires Step 48.0 4/4 retained off-tree NMainMenu authority in this same process.", out var button, out var resultLabel, out var detailLabel))
            return;
        if (!TryInitializeStartupLadderTelemetry(step, "Frozen NMainMenu SceneTree admission", "Step49-MainMenuAdmission-StaticMap", out var error))
        {
            SetStartupLadderRefusal(step, resultLabel, detailLabel, "TELEMETRY FAIL / NOT RUN", error);
            return;
        }
        BeginSteamOperation(allowCancel: false);
        _step49Gates.Reset();
        try
        {
            WriteStartupLadderCheckpoint(step, "RUN_START — Step 49.0 one-shot frozen NMainMenu admission started. Rendering remains stopped; only exact RootSceneContainer.AddChild of the already-audited off-tree menu is authorized.");
            if (!RecordStartupLadderGate(_step49Gates, _transformedRealStS2VeryEarlyInitialization.RunStep49ClosedStep48Authority(!GodotStep15NativeBridge.IsRenderingActive, d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!RecordStartupLadderGate(_step49Gates, _transformedRealStS2VeryEarlyInitialization.RunStep49RootSceneContainerBinding(d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!WriteStartupLadderStaticMap(step, out var mapError)) throw new IOException("Step 49 exact admission map write failed before AddChild: " + mapError);
            _transformedRealStS2VeryEarlyInitialization.MarkStep49StaticMapDurablyWritten();
            WriteStartupLadderCheckpoint(step, "M49_B_STATIC_MAP_WRITE_RETURNED — exact RootSceneContainer/AddChild authority map durably written before one-shot frozen admission.");
            _step49AdmissionUiStarted = true;
            button.Enabled = false;
            WriteStartupLadderCheckpoint(step, "M49_C_UI_ARMED — first/only frozen RootSceneContainer.AddChild(NMainMenu) authorized; no in-process retry after this checkpoint.");
            if (!RecordStartupLadderGate(_step49Gates, _transformedRealStS2VeryEarlyInitialization.RunStep49FrozenMainMenuAdmission(d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!RecordStartupLadderGate(_step49Gates, _transformedRealStS2VeryEarlyInitialization.RunStep49FrozenAdmissionConfinement(!GodotStep15NativeBridge.IsRenderingActive, d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            CompleteStartupLadderStep(step, _step49Gates, resultLabel, detailLabel,
                "Real NMainMenu is retained inside exact NGame.RootSceneContainer while rendering remains frozen. Step 50 frame/input audit + controlled render pulse is unlocked.");
            WriteStartupLadderCheckpoint(step, "RUN_STEP49_4OF4 — real NMainMenu frozen SceneTree admission closed; Step 50 may audit the actual in-tree frame/input surface and pulse rendering once.");
        }
        catch (Exception ex)
        {
            HandleStartupLadderException(step, resultLabel, detailLabel, ex, mutationArmed: _step49AdmissionUiStarted);
        }
        finally
        {
            await FinishStartupLadderStepAsync(step, "Step49-TransformedRealStS2MainMenuFrozenAdmission.txt", "StS2 Launcher — Step 49.0 Main Menu Frozen SceneTree Admission", resultLabel, detailLabel,
                "If AddChild was armed, never retry Step 49 in-process. Rendering must remain frozen.");
        }
    }

    private async Task RunStep50StartupLadderAsync()
    {
        const int step = 50;
        if (_step50PulseUiStarted)
        {
            var labels = GetStartupLadderLabels(step);
            SetStartupLadderRefusal(step, labels.Result, labels.Detail, "ONE-SHOT ALREADY ARMED", "Step 50 render pulse was already armed in this process. Preserve reports and relaunch; never retry StartRendering.");
            return;
        }
        if (!TryPrepareStartupLadderStep(step, _step49Gates.Snapshot().Passed && _transformedRealStS2VeryEarlyInitialization.ExactStep49ClosurePassed,
                "Step 50.0 requires Step 49.0 4/4 frozen in-tree NMainMenu authority in this same process.", out var button, out var resultLabel, out var detailLabel))
            return;
        if (!TryInitializeStartupLadderTelemetry(step, "Controlled direct-main-menu render pulse", "Step50-MainMenuFrameInput-StaticMap", out var error))
        {
            SetStartupLadderRefusal(step, resultLabel, detailLabel, "TELEMETRY FAIL / NOT RUN", error);
            return;
        }
        BeginSteamOperation(allowCancel: false);
        _step50Gates.Reset();
        try
        {
            WriteStartupLadderCheckpoint(step, "RUN_START — Step 50.0 actual in-tree main-menu frame/input audit + first/only controlled render pulse started from frozen Step-49 authority.");
            if (!RecordStartupLadderGate(_step50Gates, _transformedRealStS2VeryEarlyInitialization.RunStep50ClosedStep49Authority(!GodotStep15NativeBridge.IsRenderingActive, d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!RecordStartupLadderGate(_step50Gates, _transformedRealStS2VeryEarlyInitialization.RunStep50MenuFrameInputAudit(d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!WriteStartupLadderStaticMap(step, out var mapError)) throw new IOException("Step 50 actual in-tree frame/input map write failed before StartRendering: " + mapError);
            _transformedRealStS2VeryEarlyInitialization.MarkStep50StaticMapDurablyWritten();
            WriteStartupLadderCheckpoint(step, "M50_B_STATIC_MAP_WRITE_RETURNED — actual in-tree main-menu frame/input map durably written before StartRendering.");
            if (!NSThread.IsMain)
                throw new InvalidOperationException("Step 50.0 render pulse must be armed on UIKit main thread.");
            _transformedRealStS2VeryEarlyInitialization.BeginStep50BoundedRenderPulse(d => WriteStartupLadderCheckpoint(step, d));
            _step50PulseUiStarted = true;
            button.Enabled = false;
            WriteStartupLadderCheckpoint(step, $"M50_C_START_RENDERING_CALL — invoking StartRendering exactly once; managedThread={Environment.CurrentManagedThreadId}; isMain={NSThread.IsMain}; activeBefore={GodotStep15NativeBridge.IsRenderingActive}.");
            var stopwatch = Stopwatch.StartNew();
            var startReturned = GodotStep15NativeBridge.StartRendering();
            var activeAfterStart = GodotStep15NativeBridge.IsRenderingActive;
            WriteStartupLadderCheckpoint(step, $"M50_C_START_RENDERING_RETURNED — returned={startReturned}; activeAfterStart={activeAfterStart}; nativeError='{SanitizeStartupLadderCheckpoint(GodotStep15NativeBridge.LastError)}'.");
            if (startReturned && activeAfterStart)
                await Task.Delay(TransformedRealStS2VeryEarlyInitialization.Step40RenderPulseTargetMilliseconds);

            // Stop first, before any telemetry/file I/O on this continuation. A long Godot frame may postpone
            // this main-thread continuation; the measured overshoot is evidence, not an abandonment timeout.
            var elapsedBeforeStop = stopwatch.Elapsed.TotalMilliseconds;
            var stopReturned = GodotStep15NativeBridge.StopRendering();
            var activeAfterStop = GodotStep15NativeBridge.IsRenderingActive;
            stopwatch.Stop();
            var observedElapsed = stopwatch.Elapsed.TotalMilliseconds;
            WriteStartupLadderCheckpoint(step, $"M50_C_STOP_RENDERING_RETURNED — elapsedBeforeStopMs={elapsedBeforeStop:F1}; stopReturned={stopReturned}; activeAfterStop={activeAfterStop}; observedElapsedMs={observedElapsed:F1}; nativeError='{SanitizeStartupLadderCheckpoint(GodotStep15NativeBridge.LastError)}'.");
            if (!RecordStartupLadderGate(_step50Gates, _transformedRealStS2VeryEarlyInitialization.RunStep50BoundedRenderPulseEvidence(startReturned, activeAfterStart, stopReturned, activeAfterStop, observedElapsed, d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!RecordStartupLadderGate(_step50Gates, _transformedRealStS2VeryEarlyInitialization.RunStep50FrozenPostPulseConfinement(!GodotStep15NativeBridge.IsRenderingActive, d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            CompleteStartupLadderStep(step, _step50Gates, resultLabel, detailLabel,
                "DIRECT REAL MAIN-MENU RENDER PULSE CLOSED 4/4. Real NMainMenu stayed in-tree under exact RootSceneContainer; rendering was synchronously refrozen; original LaunchMainMenu remained uninvoked.");
            WriteStartupLadderCheckpoint(step, "RUN_STEP50_4OF4 — direct real main-menu render pulse closed and refroze; preserve reports and visual observations before relaunch.");
        }
        catch (Exception ex)
        {
            if (GodotStep15NativeBridge.IsRenderingActive)
            {
                var stopped = GodotStep15NativeBridge.StopRendering();
                WriteStartupLadderCheckpoint(step, $"M50_EXCEPTION_REFREEZE — StopRendering returned={stopped}; activeAfterStop={GodotStep15NativeBridge.IsRenderingActive}; nativeError='{SanitizeStartupLadderCheckpoint(GodotStep15NativeBridge.LastError)}'.");
            }
            HandleStartupLadderException(step, resultLabel, detailLabel, ex, mutationArmed: _step50PulseUiStarted);
        }
        finally
        {
            if (GodotStep15NativeBridge.IsRenderingActive)
                GodotStep15NativeBridge.StopRendering();
            await FinishStartupLadderStepAsync(step, "Step50-TransformedRealStS2MainMenuRenderPulse.txt", "StS2 Launcher — Step 50.0 Main Menu Controlled Render Pulse", resultLabel, detailLabel,
                "Step 50 always leaves rendering frozen. If the pulse was armed, never retry Step 50 in-process.");
        }
    }

}
