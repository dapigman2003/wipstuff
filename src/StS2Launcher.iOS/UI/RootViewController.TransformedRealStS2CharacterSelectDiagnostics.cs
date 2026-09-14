using Foundation;
using StS2Launcher.Core;
using StS2Launcher.iOS.Platform;
using System.Diagnostics;

namespace StS2Launcher.iOS;

public sealed partial class RootViewController
{
    private bool TryPrepareStep59Auxiliary(UIButton? requestedButton, UILabel? requestedResult, UILabel? requestedDetail, string operationName, out UIButton button, out UILabel resultLabel, out UILabel detailLabel)
    {
        button = requestedButton!;
        resultLabel = requestedResult!;
        detailLabel = requestedDetail!;
        if (_statusLabel is null || button is null || resultLabel is null || detailLabel is null)
            return false;
        if (!CurrentReleasePresentation.BundleIdentityMatchesExpected)
        {
            SetStartupLadderRefusal(59, resultLabel, detailLabel, "RELEASE IDENTITY FAIL", "Built bundle identity does not match the source-pinned candidate.");
            return false;
        }
        if (!_step58Gates.Snapshot().Passed || !_transformedRealStS2VeryEarlyInitialization.ExactStep58ClosurePassed)
        {
            SetStartupLadderRefusal(59, resultLabel, detailLabel, "PREREQUISITE NOT MET", $"{operationName} requires Step 58.0 4/4 runtime-ownership authority in this same process.");
            return false;
        }
        if (GodotStep15NativeBridge.IsRenderingActive)
        {
            SetStartupLadderRefusal(59, resultLabel, detailLabel, "RENDERER MUST BE FROZEN", $"{operationName} requires rendering stopped at entry.");
            return false;
        }
        return true;
    }

    private async Task RunStep59ControllerRehearsalAsync()
    {
        const int step = 59;
        if (_step59TransitionUiStarted || _step59MutationProbeUiStarted || _transformedRealStS2VeryEarlyInitialization.Step59TransitionStarted || _transformedRealStS2VeryEarlyInitialization.Step59DiagnosticMutationProbeStarted)
        {
            SetStartupLadderRefusal(step, _step59ControllerRehearsalResultLabel!, _step59ControllerRehearsalDetailLabel!, "FRESH PROCESS REQUIRED", "Step 59R must run before any Step-59 mutation/real-handler experiment. Preserve existing reports and relaunch.");
            return;
        }
        if (_step59ControllerRehearsalUiStarted)
        {
            SetStartupLadderRefusal(step, _step59ControllerRehearsalResultLabel!, _step59ControllerRehearsalDetailLabel!, "REHEARSAL ALREADY RUN", "Step 59R already ran in this process. Preserve its report.");
            return;
        }
        if (!TryPrepareStep59Auxiliary(_step59ControllerRehearsalButton, _step59ControllerRehearsalResultLabel, _step59ControllerRehearsalDetailLabel, "Step 59R", out var button, out var resultLabel, out var detailLabel))
            return;
        if (!TryInitializeStartupLadderTelemetry(step, "Step-59 controller/hotkey singleton rehearsal", "Step59R-ControllerSingletonRehearsal-StaticMap", out var error))
        {
            SetStartupLadderRefusal(step, resultLabel, detailLabel, "TELEMETRY FAIL / NOT RUN", error);
            return;
        }

        BeginSteamOperation(allowCancel: false);
        _step59Gates.Reset();
        _step59ControllerRehearsalUiStarted = true;
        button.Enabled = false;
        try
        {
            WriteStartupLadderCheckpoint(step, "RUN_START_59R — controller/hotkey singleton rehearsal started. Gates A+B plus observational getter/icon lookup rehearsal only; no Enable/OnEnable/RegisterHotkeys/Push/OpenCharacterSelect.");
            if (!RecordStartupLadderGate(_step59Gates, _transformedRealStS2VeryEarlyInitialization.RunStep59ClosedStep58Authority(!GodotStep15NativeBridge.IsRenderingActive, d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!RecordStartupLadderGate(_step59Gates, _transformedRealStS2VeryEarlyInitialization.RunStep59RealHandlerBinding(d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            var diagnostics = _transformedRealStS2VeryEarlyInitialization.RunStep59ControllerSingletonRehearsal(d => WriteStartupLadderCheckpoint(step, d));
            if (!WriteStartupLadderStaticMap(step, out var mapError)) throw new IOException("Step 59R controller/singleton map write failed: " + mapError);
            _transformedRealStS2VeryEarlyInitialization.MarkStep59StaticMapDurablyWritten();
            WriteStartupLadderCheckpoint(step, "M59R_STATIC_MAP_WRITE_RETURNED — controller/hotkey rehearsal durably written; handlerArmed=False; mutationProbe=False.");
            resultLabel.Text = "STEP 59R: CONTROLLER/SINGLETON REHEARSAL COMPLETE";
            resultLabel.TextColor = UIColor.SystemGreen;
            detailLabel.Text = "Getter/icon-lookup rehearsal completed without arming the real handler. Preserve the report. Because phone runs are cheap, use a fresh process for 59U, 59E, or the real Step 59 experiment.";
            if (_statusLabel is not null)
            {
                _statusLabel.Text = "STEP 59R COMPLETE — controller/hotkey singleton evidence captured. Fresh process recommended for the next Step-59 branch.";
                _statusLabel.TextColor = UIColor.SystemGreen;
            }
            WriteStartupLadderCheckpoint(step, $"RUN_STEP59R_COMPLETE — diagnosticsLength={diagnostics.Length}; realHandlerArmed=False; mutationProbe=False.");
        }
        catch (Exception ex)
        {
            HandleStartupLadderException(step, resultLabel, detailLabel, ex, mutationArmed: false);
        }
        finally
        {
            await FinishStartupLadderStepAsync(step, "Step59R-ControllerSingletonRehearsal.txt", "StS2 Launcher — Step 59R Controller/Hotkey Singleton Rehearsal", resultLabel, detailLabel, "Step 59R invokes only observational getters/icon lookups; no real handler or button Enable is armed.");
        }
    }

    private async Task RunStep59RegisterHotkeysProbeAsync()
    {
        const int step = 59;
        if (_step59DiagnosticDeckUiStarted || _step59ControllerRehearsalUiStarted || _step59TransitionUiStarted || _step59MutationProbeUiStarted ||
            _transformedRealStS2VeryEarlyInitialization.Step59TransitionStarted || _transformedRealStS2VeryEarlyInitialization.Step59DiagnosticMutationProbeStarted)
        {
            SetStartupLadderRefusal(step, _step59RegisterHotkeysProbeResultLabel!, _step59RegisterHotkeysProbeDetailLabel!, "FRESH PROCESS REQUIRED", "Step 59H is a one-shot hotkey-binding diagnostic. Relaunch and reproduce through Step 58 without running another Step-59 branch first.");
            return;
        }
        if (!TryPrepareStep59Auxiliary(_step59RegisterHotkeysProbeButton, _step59RegisterHotkeysProbeResultLabel, _step59RegisterHotkeysProbeDetailLabel, "Step 59H", out var button, out var resultLabel, out var detailLabel)) return;
        if (!TryInitializeStartupLadderTelemetry(step, "Step-59 isolated RegisterHotkeys probe", "Step59H-RegisterHotkeys-StaticMap", out var error))
        {
            SetStartupLadderRefusal(step, resultLabel, detailLabel, "TELEMETRY FAIL / NOT RUN", error); return;
        }
        BeginSteamOperation(allowCancel: false); _step59Gates.Reset();
        try
        {
            WriteStartupLadderCheckpoint(step, "RUN_START_59H — isolated retained-embark RegisterHotkeys probe started. No UpdateControllerButton/Enable/OnEnable/Push/OpenCharacterSelect.");
            if (!RecordStartupLadderGate(_step59Gates, _transformedRealStS2VeryEarlyInitialization.RunStep59ClosedStep58Authority(!GodotStep15NativeBridge.IsRenderingActive, d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!RecordStartupLadderGate(_step59Gates, _transformedRealStS2VeryEarlyInitialization.RunStep59RealHandlerBinding(d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!WriteStartupLadderStaticMap(step, out var preflightMapError)) throw new IOException("Step 59H preflight map write failed: " + preflightMapError);
            _transformedRealStS2VeryEarlyInitialization.MarkStep59StaticMapDurablyWritten(); _step59MutationProbeUiStarted = true; button.Enabled = false;
            var probe = _transformedRealStS2VeryEarlyInitialization.RunStep59RegisterHotkeysProbe(d => WriteStartupLadderCheckpoint(step, d));
            if (!WriteStartupLadderStaticMap(step, out var mapError)) throw new IOException("Step 59H post-probe map refresh failed: " + mapError);
            WriteStartupLadderCheckpoint(step, $"M59H_STATIC_MAP_REFRESH_RETURNED — passed={probe.Passed}; freshProcessRequired=True.");
            resultLabel.Text = probe.Passed ? "STEP 59H: REGISTERHOTKEYS RETURNED" : "STEP 59H: REGISTERHOTKEYS THREW — EVIDENCE CAPTURED";
            resultLabel.TextColor = probe.Passed ? UIColor.SystemGreen : UIColor.SystemOrange;
            detailLabel.Text = probe.Passed ? "RegisterHotkeys returned. Preserve the report and relaunch before another Step-59 experiment." : "RegisterHotkeys reproduced a failure. Preserve the report; the hotkey-binding path is localized. Relaunch afterward.";
        }
        catch (Exception ex) { HandleStartupLadderException(step, resultLabel, detailLabel, ex, mutationArmed: _step59MutationProbeUiStarted || _transformedRealStS2VeryEarlyInitialization.Step59DiagnosticMutationProbeStarted); }
        finally { await FinishStartupLadderStepAsync(step, "Step59H-IsolatedRegisterHotkeys.txt", "StS2 Launcher — Step 59H Isolated RegisterHotkeys Probe", resultLabel, detailLabel, "Step 59H may add hotkey bindings and requires a fresh process afterward. It never calls OpenCharacterSelect."); }
    }

    private async Task RunStep59UpdateControllerProbeAsync()
    {
        const int step = 59;
        if (_step59DiagnosticDeckUiStarted || _step59ControllerRehearsalUiStarted || _step59TransitionUiStarted || _step59MutationProbeUiStarted ||
            _transformedRealStS2VeryEarlyInitialization.Step59TransitionStarted || _transformedRealStS2VeryEarlyInitialization.Step59DiagnosticMutationProbeStarted)
        {
            SetStartupLadderRefusal(step, _step59UpdateControllerProbeResultLabel!, _step59UpdateControllerProbeDetailLabel!, "FRESH PROCESS REQUIRED", "Step 59U is a one-shot mutating diagnostic. Relaunch and reproduce through Step 58 without running another Step-59 branch first.");
            return;
        }
        if (!TryPrepareStep59Auxiliary(_step59UpdateControllerProbeButton, _step59UpdateControllerProbeResultLabel, _step59UpdateControllerProbeDetailLabel, "Step 59U", out var button, out var resultLabel, out var detailLabel))
            return;
        if (!TryInitializeStartupLadderTelemetry(step, "Step-59 isolated UpdateControllerButton probe", "Step59U-UpdateControllerButton-StaticMap", out var error))
        {
            SetStartupLadderRefusal(step, resultLabel, detailLabel, "TELEMETRY FAIL / NOT RUN", error);
            return;
        }

        BeginSteamOperation(allowCancel: false);
        _step59Gates.Reset();
        try
        {
            WriteStartupLadderCheckpoint(step, "RUN_START_59U — isolated retained-embark UpdateControllerButton probe started. No Enable/OnEnable/RegisterHotkeys/Push/OpenCharacterSelect.");
            if (!RecordStartupLadderGate(_step59Gates, _transformedRealStS2VeryEarlyInitialization.RunStep59ClosedStep58Authority(!GodotStep15NativeBridge.IsRenderingActive, d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!RecordStartupLadderGate(_step59Gates, _transformedRealStS2VeryEarlyInitialization.RunStep59RealHandlerBinding(d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!WriteStartupLadderStaticMap(step, out var preflightMapError)) throw new IOException("Step 59U preflight map write failed: " + preflightMapError);
            _transformedRealStS2VeryEarlyInitialization.MarkStep59StaticMapDurablyWritten();
            _step59MutationProbeUiStarted = true;
            button.Enabled = false;
            var probe = _transformedRealStS2VeryEarlyInitialization.RunStep59UpdateControllerButtonProbe(d => WriteStartupLadderCheckpoint(step, d));
            if (!WriteStartupLadderStaticMap(step, out var mapError)) throw new IOException("Step 59U post-probe map refresh failed: " + mapError);
            WriteStartupLadderCheckpoint(step, $"M59U_STATIC_MAP_REFRESH_RETURNED — passed={probe.Passed}; freshProcessRequired=True.");
            resultLabel.Text = probe.Passed ? "STEP 59U: UPDATECONTROLLERBUTTON RETURNED" : "STEP 59U: UPDATECONTROLLERBUTTON THREW — EVIDENCE CAPTURED";
            resultLabel.TextColor = probe.Passed ? UIColor.SystemGreen : UIColor.SystemOrange;
            detailLabel.Text = probe.Passed
                ? "The isolated UpdateControllerButton path returned. Preserve the report and relaunch before any other Step-59 experiment."
                : "The isolated controller-update path threw. This is useful localization evidence; preserve the report and relaunch.";
            WriteStartupLadderCheckpoint(step, $"RUN_STEP59U_COMPLETE — passed={probe.Passed}; diagnosticsLength={probe.Diagnostics.Length}; realHandlerArmed=False; freshProcessRequired=True.");
        }
        catch (Exception ex)
        {
            HandleStartupLadderException(step, resultLabel, detailLabel, ex, mutationArmed: _step59MutationProbeUiStarted || _transformedRealStS2VeryEarlyInitialization.Step59DiagnosticMutationProbeStarted);
        }
        finally
        {
            await FinishStartupLadderStepAsync(step, "Step59U-IsolatedUpdateControllerButton.txt", "StS2 Launcher — Step 59U Isolated UpdateControllerButton Probe", resultLabel, detailLabel, "Step 59U is diagnostic only and requires a fresh process afterward. It never calls OpenCharacterSelect.");
        }
    }

    private Task RunStep59ConfirmTailProbeAsync() => RunStep59ConfirmTailProbeAsync(repairEnabled: false);
    private Task RunStep59ConfirmTailRepairProbeAsync() => RunStep59ConfirmTailProbeAsync(repairEnabled: true);

    private async Task RunStep59ConfirmTailProbeAsync(bool repairEnabled)
    {
        const int step = 59;
        var code = repairEnabled ? "59X" : "59T";
        var requestedButton = repairEnabled ? _step59ConfirmTailRepairProbeButton : _step59ConfirmTailProbeButton;
        var requestedResult = repairEnabled ? _step59ConfirmTailRepairProbeResultLabel : _step59ConfirmTailProbeResultLabel;
        var requestedDetail = repairEnabled ? _step59ConfirmTailRepairProbeDetailLabel : _step59ConfirmTailProbeDetailLabel;
        if (_step59DiagnosticDeckUiStarted || _step59ControllerRehearsalUiStarted || _step59TransitionUiStarted || _step59MutationProbeUiStarted ||
            _transformedRealStS2VeryEarlyInitialization.Step59TransitionStarted || _transformedRealStS2VeryEarlyInitialization.Step59DiagnosticMutationProbeStarted)
        {
            SetStartupLadderRefusal(step, requestedResult!, requestedDetail!, "FRESH PROCESS REQUIRED", $"Step {code} is a one-shot confirm-button tail experiment. Relaunch through Step 58 without another Step-59 branch first.");
            return;
        }
        if (!TryPrepareStep59Auxiliary(requestedButton, requestedResult, requestedDetail, $"Step {code}", out var button, out var resultLabel, out var detailLabel)) return;
        if (!TryInitializeStartupLadderTelemetry(step, repairEnabled ? "Step-59X PropertyTweener repair tail probe" : "Step-59T PropertyTweener control tail probe", repairEnabled ? "Step59X-PropertyTweenerRepair-StaticMap" : "Step59T-PropertyTweenerControl-StaticMap", out var error))
        {
            SetStartupLadderRefusal(step, resultLabel, detailLabel, "TELEMETRY FAIL / NOT RUN", error); return;
        }
        BeginSteamOperation(allowCancel: false); _step59Gates.Reset();
        try
        {
            WriteStartupLadderCheckpoint(step, $"RUN_START_{code} — isolated NConfirmButton post-base tail; repairEnabled={repairEnabled}. No NButton.OnEnable/RegisterHotkeys/UpdateControllerButton/Push/OpenCharacterSelect.");
            if (!RecordStartupLadderGate(_step59Gates, _transformedRealStS2VeryEarlyInitialization.RunStep59ClosedStep58Authority(!GodotStep15NativeBridge.IsRenderingActive, d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!RecordStartupLadderGate(_step59Gates, _transformedRealStS2VeryEarlyInitialization.RunStep59RealHandlerBinding(d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!WriteStartupLadderStaticMap(step, out var preflightMapError)) throw new IOException($"Step {code} preflight map write failed: " + preflightMapError);
            _transformedRealStS2VeryEarlyInitialization.MarkStep59StaticMapDurablyWritten(); _step59MutationProbeUiStarted = true; button.Enabled = false;
            var probe = repairEnabled
                ? _transformedRealStS2VeryEarlyInitialization.RunStep59ConfirmButtonTailRepairProbe(d => WriteStartupLadderCheckpoint(step, d))
                : _transformedRealStS2VeryEarlyInitialization.RunStep59ConfirmButtonTailProbe(d => WriteStartupLadderCheckpoint(step, d));
            if (!WriteStartupLadderStaticMap(step, out var mapError)) throw new IOException($"Step {code} post-probe map refresh failed: " + mapError);
            WriteStartupLadderCheckpoint(step, $"M{code}_STATIC_MAP_REFRESH_RETURNED — repairEnabled={repairEnabled}; passed={probe.Passed}; freshProcessRequired=True.");
            resultLabel.Text = probe.Passed ? $"STEP {code}: FULL TAIL RETURNED" : $"STEP {code}: TAIL THREW — STAGE CAPTURED";
            resultLabel.TextColor = probe.Passed ? UIColor.SystemGreen : UIColor.SystemOrange;
            detailLabel.Text = probe.Passed ? "Every tail stage returned. Preserve the report and relaunch for the next branch." : "A tail stage failed; bridge telemetry distinguishes native-null, managed-null, wrong-type, and repair activity. Preserve the report and relaunch.";
        }
        catch (Exception ex) { HandleStartupLadderException(step, resultLabel, detailLabel, ex, mutationArmed: _step59MutationProbeUiStarted || _transformedRealStS2VeryEarlyInitialization.Step59DiagnosticMutationProbeStarted); }
        finally { await FinishStartupLadderStepAsync(step, repairEnabled ? "Step59X-PropertyTweenerRepairTail.txt" : "Step59T-PropertyTweenerControlTail.txt", repairEnabled ? "StS2 Launcher — Step 59X PropertyTweener Repair Tail" : "StS2 Launcher — Step 59T PropertyTweener Control Tail", resultLabel, detailLabel, $"Step {code} mutates only retained confirm-button visual/tween state and requires a fresh process afterward."); }
    }

    private async Task RunStep59IsolatedEnableProbeAsync()
    {
        const int step = 59;
        if (_step59DiagnosticDeckUiStarted || _step59ControllerRehearsalUiStarted || _step59TransitionUiStarted || _step59MutationProbeUiStarted ||
            _transformedRealStS2VeryEarlyInitialization.Step59TransitionStarted || _transformedRealStS2VeryEarlyInitialization.Step59DiagnosticMutationProbeStarted)
        {
            SetStartupLadderRefusal(step, _step59IsolatedEnableProbeResultLabel!, _step59IsolatedEnableProbeDetailLabel!, "FRESH PROCESS REQUIRED", "Step 59E is a one-shot button-lifecycle diagnostic. Relaunch and reproduce through Step 58 without running another Step-59 branch first.");
            return;
        }
        if (!TryPrepareStep59Auxiliary(_step59IsolatedEnableProbeButton, _step59IsolatedEnableProbeResultLabel, _step59IsolatedEnableProbeDetailLabel, "Step 59E", out var button, out var resultLabel, out var detailLabel))
            return;
        if (!TryInitializeStartupLadderTelemetry(step, "Step-59 isolated retained embark Enable probe", "Step59E-IsolatedEmbarkEnable-StaticMap", out var error))
        {
            SetStartupLadderRefusal(step, resultLabel, detailLabel, "TELEMETRY FAIL / NOT RUN", error);
            return;
        }

        BeginSteamOperation(allowCancel: false);
        _step59Gates.Reset();
        try
        {
            WriteStartupLadderCheckpoint(step, "RUN_START_59E — isolated retained embark Enable probe started outside OpenCharacterSelect. No runtime-wide IL instrumentation is used; compare this probe with 59H/59U/59T to isolate the failing subpath.");
            if (!RecordStartupLadderGate(_step59Gates, _transformedRealStS2VeryEarlyInitialization.RunStep59ClosedStep58Authority(!GodotStep15NativeBridge.IsRenderingActive, d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!RecordStartupLadderGate(_step59Gates, _transformedRealStS2VeryEarlyInitialization.RunStep59RealHandlerBinding(d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!WriteStartupLadderStaticMap(step, out var preflightMapError)) throw new IOException("Step 59E preflight map write failed: " + preflightMapError);
            _transformedRealStS2VeryEarlyInitialization.MarkStep59StaticMapDurablyWritten();
            _step59MutationProbeUiStarted = true;
            button.Enabled = false;
            var probe = _transformedRealStS2VeryEarlyInitialization.RunStep59IsolatedEmbarkEnableProbe(d => WriteStartupLadderCheckpoint(step, d));
            if (!WriteStartupLadderStaticMap(step, out var mapError)) throw new IOException("Step 59E post-probe map refresh failed: " + mapError);
            WriteStartupLadderCheckpoint(step, $"M59E_STATIC_MAP_REFRESH_RETURNED — passed={probe.Passed}; freshProcessRequired=True; realHandlerArmed=False.");
            resultLabel.Text = probe.Passed ? "STEP 59E: ISOLATED EMBARK ENABLE RETURNED" : "STEP 59E: ISOLATED EMBARK ENABLE THREW — EVIDENCE CAPTURED";
            resultLabel.TextColor = probe.Passed ? UIColor.SystemGreen : UIColor.SystemOrange;
            detailLabel.Text = probe.Passed
                ? "The isolated Enable path returned outside OpenCharacterSelect. Preserve the trace/report and relaunch before another Step-59 experiment."
                : "The isolated Enable path reproduced a failure outside OpenCharacterSelect. Preserve the report; this sharply localizes the button path. Relaunch afterward.";
            WriteStartupLadderCheckpoint(step, $"RUN_STEP59E_COMPLETE — passed={probe.Passed}; diagnosticsLength={probe.Diagnostics.Length}; realHandlerArmed=False; freshProcessRequired=True.");
        }
        catch (Exception ex)
        {
            HandleStartupLadderException(step, resultLabel, detailLabel, ex, mutationArmed: _step59MutationProbeUiStarted || _transformedRealStS2VeryEarlyInitialization.Step59DiagnosticMutationProbeStarted);
        }
        finally
        {
            await FinishStartupLadderStepAsync(step, "Step59E-IsolatedEmbarkEnable.txt", "StS2 Launcher — Step 59E Isolated Retained Embark Enable Probe", resultLabel, detailLabel, "Step 59E mutates only the retained button lifecycle/hotkey/tween state and requires a fresh process afterward. It never calls OpenCharacterSelect.");
        }
    }

}
