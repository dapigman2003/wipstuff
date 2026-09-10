using Foundation;
using StS2Launcher.Core;
using StS2Launcher.iOS.Platform;
using System.Diagnostics;

namespace StS2Launcher.iOS;

public sealed partial class RootViewController
{
    private async Task RunStep51StartupLadderAsync()
    {
        const int step = 51;
        if (!TryPrepareStartupLadderStep(step, _step50Gates.Snapshot().Passed && _transformedRealStS2VeryEarlyInitialization.ExactStep50ClosurePassed,
                "Step 51.0 requires Step 50.0 4/4 short render-pulse authority with rendering synchronously refrozen in this same process.", out var button, out var resultLabel, out var detailLabel))
            return;
        if (!TryInitializeStartupLadderTelemetry(step, "Non-invoking single-player frontier map", "Step51-SingleplayerFrontier-StaticMap", out var error))
        {
            SetStartupLadderRefusal(step, resultLabel, detailLabel, "TELEMETRY FAIL / NOT RUN", error);
            return;
        }
        BeginSteamOperation(allowCancel: false);
        _step51Gates.Reset();
        try
        {
            WriteStartupLadderCheckpoint(step, "RUN_START — Step 51.0 non-invoking exact single-player button frontier map started from refrozen Step-50 authority. No menu button handler is authorized for invocation.");
            if (!RecordStartupLadderGate(_step51Gates, _transformedRealStS2VeryEarlyInitialization.RunStep51ClosedStep50Authority(!GodotStep15NativeBridge.IsRenderingActive, d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!RecordStartupLadderGate(_step51Gates, _transformedRealStS2VeryEarlyInitialization.RunStep51SingleplayerHandlerBinding(d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!RecordStartupLadderGate(_step51Gates, _transformedRealStS2VeryEarlyInitialization.RunStep51SingleplayerFrontierMap(d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!WriteStartupLadderStaticMap(step, out var mapError)) throw new IOException("Step 51 single-player frontier map write failed: " + mapError);
            _transformedRealStS2VeryEarlyInitialization.MarkStep51StaticMapDurablyWritten();
            WriteStartupLadderCheckpoint(step, "M51_C_STATIC_MAP_WRITE_RETURNED — exact single-player handler IL + execution-qualified frontier map durably written; handler invocation remains NO and rendering remains stopped.");
            if (!RecordStartupLadderGate(_step51Gates, _transformedRealStS2VeryEarlyInitialization.RunStep51FrozenNoInvocationConfinement(!GodotStep15NativeBridge.IsRenderingActive, d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            CompleteStartupLadderStep(step, _step51Gates, resultLabel, detailLabel,
                "SINGLE-PLAYER FRONTIER MAP CLOSED 4/4. Exact handlers and their execution/deferred frontier are durable evidence only; no handler was invoked. Step 52 sustained render residency is unlocked.");
            WriteStartupLadderCheckpoint(step, "RUN_STEP51_4OF4 — non-invoking single-player frontier map closed; Step 52 may run one longer bounded render residency pulse.");
        }
        catch (Exception ex)
        {
            HandleStartupLadderException(step, resultLabel, detailLabel, ex, mutationArmed: false);
        }
        finally
        {
            await FinishStartupLadderStepAsync(step, "Step51-TransformedRealStS2SingleplayerFrontier.txt", "StS2 Launcher — Step 51.0 Singleplayer Frontier Map", resultLabel, detailLabel,
                "Step 51 never invokes a menu handler and never restarts rendering.");
        }
    }

    private async Task RunStep52StartupLadderAsync()
    {
        const int step = 52;
        if (_step52PulseUiStarted)
        {
            var labels = GetStartupLadderLabels(step);
            SetStartupLadderRefusal(step, labels.Result, labels.Detail, "ONE-SHOT ALREADY ARMED", "Step 52 sustained render pulse was already armed in this process. Preserve reports and relaunch; never retry StartRendering.");
            return;
        }
        if (!TryPrepareStartupLadderStep(step, _step51Gates.Snapshot().Passed && _transformedRealStS2VeryEarlyInitialization.ExactStep51ClosurePassed,
                "Step 52.0 requires Step 51.0 4/4 durable non-invoking single-player frontier authority in this same process.", out var button, out var resultLabel, out var detailLabel))
            return;
        if (!TryInitializeStartupLadderTelemetry(step, "Sustained direct-main-menu render residency", "Step52-MainMenuSustainedFrameInput-StaticMap", out var error))
        {
            SetStartupLadderRefusal(step, resultLabel, detailLabel, "TELEMETRY FAIL / NOT RUN", error);
            return;
        }
        BeginSteamOperation(allowCancel: false);
        _step52Gates.Reset();
        try
        {
            WriteStartupLadderCheckpoint(step, "RUN_START — Step 52.0 sustained direct-main-menu render residency started from frozen Step-51 authority. A fresh frame/input map must be durable before rendering may start.");
            if (!RecordStartupLadderGate(_step52Gates, _transformedRealStS2VeryEarlyInitialization.RunStep52ClosedStep51Authority(!GodotStep15NativeBridge.IsRenderingActive, d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!RecordStartupLadderGate(_step52Gates, _transformedRealStS2VeryEarlyInitialization.RunStep52SustainedFrameInputAudit(d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!WriteStartupLadderStaticMap(step, out var mapError)) throw new IOException("Step 52 sustained frame/input map write failed before StartRendering: " + mapError);
            _transformedRealStS2VeryEarlyInitialization.MarkStep52StaticMapDurablyWritten();
            WriteStartupLadderCheckpoint(step, "M52_B_STATIC_MAP_WRITE_RETURNED — sustained-render frame/input map durably written before StartRendering.");
            if (!NSThread.IsMain)
                throw new InvalidOperationException("Step 52.0 sustained render pulse must be armed on UIKit main thread.");
            _transformedRealStS2VeryEarlyInitialization.BeginStep52SustainedRenderPulse(d => WriteStartupLadderCheckpoint(step, d));
            _step52PulseUiStarted = true;
            button.Enabled = false;
            WriteStartupLadderCheckpoint(step, $"M52_C_START_RENDERING_CALL — invoking StartRendering exactly once for sustained residency; managedThread={Environment.CurrentManagedThreadId}; isMain={NSThread.IsMain}; activeBefore={GodotStep15NativeBridge.IsRenderingActive}.");
            var stopwatch = Stopwatch.StartNew();
            var startReturned = GodotStep15NativeBridge.StartRendering();
            var activeAfterStart = GodotStep15NativeBridge.IsRenderingActive;
            WriteStartupLadderCheckpoint(step, $"M52_C_START_RENDERING_RETURNED — returned={startReturned}; activeAfterStart={activeAfterStart}; nativeError='{SanitizeStartupLadderCheckpoint(GodotStep15NativeBridge.LastError)}'.");
            if (startReturned && activeAfterStart)
                await Task.Delay(TransformedRealStS2VeryEarlyInitialization.Step52SustainedRenderTargetMilliseconds);

            // As in Step 40/50, stop first on the first managed continuation. No post-delay telemetry or
            // file I/O is allowed to happen while the embedded renderer is still active.
            var elapsedBeforeStop = stopwatch.Elapsed.TotalMilliseconds;
            var stopReturned = GodotStep15NativeBridge.StopRendering();
            var activeAfterStop = GodotStep15NativeBridge.IsRenderingActive;
            stopwatch.Stop();
            var observedElapsed = stopwatch.Elapsed.TotalMilliseconds;
            WriteStartupLadderCheckpoint(step, $"M52_C_STOP_RENDERING_RETURNED — elapsedBeforeStopMs={elapsedBeforeStop:F1}; stopReturned={stopReturned}; activeAfterStop={activeAfterStop}; observedElapsedMs={observedElapsed:F1}; nativeError='{SanitizeStartupLadderCheckpoint(GodotStep15NativeBridge.LastError)}'.");
            if (!RecordStartupLadderGate(_step52Gates, _transformedRealStS2VeryEarlyInitialization.RunStep52SustainedRenderPulseEvidence(startReturned, activeAfterStart, stopReturned, activeAfterStop, observedElapsed, d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!RecordStartupLadderGate(_step52Gates, _transformedRealStS2VeryEarlyInitialization.RunStep52FrozenPostResidencyConfinement(!GodotStep15NativeBridge.IsRenderingActive, d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            CompleteStartupLadderStep(step, _step52Gates, resultLabel, detailLabel,
                "SUSTAINED REAL MAIN-MENU RENDER RESIDENCY CLOSED 4/4. Real NMainMenu stayed in-tree for the longer render window and was synchronously refrozen with confinement intact.");
            WriteStartupLadderCheckpoint(step, "RUN_STEP52_4OF4 — sustained real-main-menu rendering closed and refroze; preserve visual observations and reports. Single-player handler remains uninvoked.");
        }
        catch (Exception ex)
        {
            if (GodotStep15NativeBridge.IsRenderingActive)
            {
                var stopped = GodotStep15NativeBridge.StopRendering();
                WriteStartupLadderCheckpoint(step, $"M52_EXCEPTION_REFREEZE — StopRendering returned={stopped}; activeAfterStop={GodotStep15NativeBridge.IsRenderingActive}; nativeError='{SanitizeStartupLadderCheckpoint(GodotStep15NativeBridge.LastError)}'.");
            }
            HandleStartupLadderException(step, resultLabel, detailLabel, ex, mutationArmed: _step52PulseUiStarted);
        }
        finally
        {
            if (GodotStep15NativeBridge.IsRenderingActive)
                GodotStep15NativeBridge.StopRendering();
            await FinishStartupLadderStepAsync(step, "Step52-TransformedRealStS2MainMenuSustainedRender.txt", "StS2 Launcher — Step 52.0 Main Menu Sustained Render Residency", resultLabel, detailLabel,
                "Step 52 always leaves rendering frozen. If the sustained pulse was armed, never retry Step 52 in-process.");
        }
    }
}
