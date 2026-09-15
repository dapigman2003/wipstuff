using Foundation;
using StS2Launcher.Core;
using StS2Launcher.iOS.Platform;

namespace StS2Launcher.iOS;

public sealed partial class RootViewController
{
    private async Task RunStep59GenericPropertyMatrixAsync()
    {
        const int step = 59;
        if (_step59DiagnosticDeckUiStarted || _step59ControllerRehearsalUiStarted || _step59TransitionUiStarted || _step59MutationProbeUiStarted ||
            _transformedRealStS2VeryEarlyInitialization.Step59TransitionStarted || _transformedRealStS2VeryEarlyInitialization.Step59DiagnosticMutationProbeStarted)
        {
            SetStartupLadderRefusal(step, _step59GenericPropertyMatrixResultLabel!, _step59GenericPropertyMatrixDetailLabel!, "FRESH PROCESS REQUIRED", "Step 59P performs same-value generic property Set/SetIndexed probes. Relaunch and reproduce through Step 58 under the FULL profile without running another Step-59 branch first.");
            return;
        }
        if (_transformedRealStS2VeryEarlyInitialization.SelectedPropertyTweenerProfile != PropertyTweenerExperimentProfile.Full)
        {
            SetStartupLadderRefusal(step, _step59GenericPropertyMatrixResultLabel!, _step59GenericPropertyMatrixDetailLabel!, "FULL PROFILE REQUIRED", $"Step 59P requires profile=Full, but this process uses {_transformedRealStS2VeryEarlyInitialization.SelectedPropertyTweenerProfile}. Relaunch and choose the FULL closed-path button before Step 35.");
            return;
        }
        if (!TryPrepareStep59Auxiliary(_step59GenericPropertyMatrixButton, _step59GenericPropertyMatrixResultLabel, _step59GenericPropertyMatrixDetailLabel, "Step 59P", out var button, out var resultLabel, out var detailLabel)) return;
        if (!TryInitializeStartupLadderTelemetry(step, "Step-59 scripted-object generic property-resolution matrix", "Step59P-GenericPropertyResolution-StaticMap", out var error))
        {
            SetStartupLadderRefusal(step, resultLabel, detailLabel, "TELEMETRY FAIL / NOT RUN", error); return;
        }
        BeginSteamOperation(allowCancel: false); _step59Gates.Reset();
        try
        {
            WriteStartupLadderCheckpoint(step, "RUN_START_59P — scripted-object generic property-resolution matrix started. FULL profile; PropertyTweener repair disabled. Direct managed access is compared with Get/GetIndexed and same-value Set/SetIndexed across scripted and native targets.");
            if (!RecordStartupLadderGate(_step59Gates, _transformedRealStS2VeryEarlyInitialization.RunStep59ClosedStep58Authority(!GodotStep15NativeBridge.IsRenderingActive, d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!RecordStartupLadderGate(_step59Gates, _transformedRealStS2VeryEarlyInitialization.RunStep59RealHandlerBinding(d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!WriteStartupLadderStaticMap(step, out var preflightMapError)) throw new IOException("Step 59P preflight map write failed: " + preflightMapError);
            _transformedRealStS2VeryEarlyInitialization.MarkStep59StaticMapDurablyWritten(); _step59MutationProbeUiStarted = true; button.Enabled = false;
            var diagnostics = _transformedRealStS2VeryEarlyInitialization.RunStep59GenericPropertyResolutionMatrix(d => WriteStartupLadderCheckpoint(step, d));
            if (!WriteStartupLadderStaticMap(step, out var mapError)) throw new IOException("Step 59P post-matrix map refresh failed: " + mapError);
            WriteStartupLadderCheckpoint(step, $"M59P_STATIC_MAP_REFRESH_RETURNED — diagnosticsLength={diagnostics.Length}; freshProcessRequired=True; realHandlerArmed=False.");
            resultLabel.Text = "STEP 59P: PROPERTY BRIDGE MATRIX COMPLETE";
            resultLabel.TextColor = UIColor.SystemGreen;
            detailLabel.Text = "The scripted/native property-access comparison completed. Preserve the report and relaunch before 59I or 59Q.";
        }
        catch (Exception ex) { HandleStartupLadderException(step, resultLabel, detailLabel, ex, mutationArmed: _step59MutationProbeUiStarted || _transformedRealStS2VeryEarlyInitialization.Step59DiagnosticMutationProbeStarted); }
        finally { await FinishStartupLadderStepAsync(step, "Step59P-GenericPropertyResolutionMatrix.txt", "StS2 Launcher — Step 59P Generic Property Resolution Matrix", resultLabel, detailLabel, "Step 59P performs same-value generic property write probes and requires a fresh process afterward. It never calls OpenCharacterSelect."); }
    }

    private async Task RunStep59ManagedIdentityMatrixAsync()
    {
        const int step = 59;
        if (_step59DiagnosticDeckUiStarted || _step59ControllerRehearsalUiStarted || _step59TransitionUiStarted || _step59MutationProbeUiStarted ||
            _transformedRealStS2VeryEarlyInitialization.Step59TransitionStarted || _transformedRealStS2VeryEarlyInitialization.Step59DiagnosticMutationProbeStarted)
        {
            SetStartupLadderRefusal(step, _step59ManagedIdentityMatrixResultLabel!, _step59ManagedIdentityMatrixDetailLabel!, "FRESH PROCESS REQUIRED", "Step 59I is a one-shot managed/native identity matrix. Relaunch and reproduce through Step 58 under the FULL profile without running another Step-59 branch first.");
            return;
        }
        if (_transformedRealStS2VeryEarlyInitialization.SelectedPropertyTweenerProfile != PropertyTweenerExperimentProfile.Full)
        {
            SetStartupLadderRefusal(step, _step59ManagedIdentityMatrixResultLabel!, _step59ManagedIdentityMatrixDetailLabel!, "FULL PROFILE REQUIRED", $"Step 59I requires profile=Full, but this process uses {_transformedRealStS2VeryEarlyInitialization.SelectedPropertyTweenerProfile}. Relaunch and choose the FULL closed-path button before Step 35.");
            return;
        }
        if (!TryPrepareStep59Auxiliary(_step59ManagedIdentityMatrixButton, _step59ManagedIdentityMatrixResultLabel, _step59ManagedIdentityMatrixDetailLabel, "Step 59I", out var button, out var resultLabel, out var detailLabel)) return;
        if (!TryInitializeStartupLadderTelemetry(step, "Step-59 managed/native object identity matrix", "Step59I-ManagedNativeIdentity-StaticMap", out var error))
        {
            SetStartupLadderRefusal(step, resultLabel, detailLabel, "TELEMETRY FAIL / NOT RUN", error); return;
        }
        BeginSteamOperation(allowCancel: false); _step59Gates.Reset();
        try
        {
            WriteStartupLadderCheckpoint(step, "RUN_START_59I — managed/native object identity matrix started. Compares retained objects with private GodotSharp UnmanagedGetManaged(nativePtr), instance IDs, script identity, and native callback signatures.");
            if (!RecordStartupLadderGate(_step59Gates, _transformedRealStS2VeryEarlyInitialization.RunStep59ClosedStep58Authority(!GodotStep15NativeBridge.IsRenderingActive, d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!RecordStartupLadderGate(_step59Gates, _transformedRealStS2VeryEarlyInitialization.RunStep59RealHandlerBinding(d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!WriteStartupLadderStaticMap(step, out var preflightMapError)) throw new IOException("Step 59I preflight map write failed: " + preflightMapError);
            _transformedRealStS2VeryEarlyInitialization.MarkStep59StaticMapDurablyWritten(); _step59MutationProbeUiStarted = true; button.Enabled = false;
            var diagnostics = _transformedRealStS2VeryEarlyInitialization.RunStep59ManagedNativeIdentityMatrix(d => WriteStartupLadderCheckpoint(step, d));
            if (!WriteStartupLadderStaticMap(step, out var mapError)) throw new IOException("Step 59I post-matrix map refresh failed: " + mapError);
            WriteStartupLadderCheckpoint(step, $"M59I_STATIC_MAP_REFRESH_RETURNED — diagnosticsLength={diagnostics.Length}; freshProcessRequired=True; realHandlerArmed=False.");
            resultLabel.Text = "STEP 59I: OBJECT IDENTITY MATRIX COMPLETE";
            resultLabel.TextColor = UIColor.SystemGreen;
            detailLabel.Text = "The retained/native managed-object identity comparison completed. Preserve the report and relaunch before 59P or 59Q.";
        }
        catch (Exception ex) { HandleStartupLadderException(step, resultLabel, detailLabel, ex, mutationArmed: _step59MutationProbeUiStarted || _transformedRealStS2VeryEarlyInitialization.Step59DiagnosticMutationProbeStarted); }
        finally { await FinishStartupLadderStepAsync(step, "Step59I-ManagedNativeIdentityMatrix.txt", "StS2 Launcher — Step 59I Managed / Native Identity Matrix", resultLabel, detailLabel, "Step 59I is identity-only but is treated as one-shot for clean evidence. It never calls OpenCharacterSelect."); }
    }

    private async Task RunStep59ManagedPropertyBypassTransitionAsync()
    {
        const int step = 59;
        if (_step59DiagnosticDeckUiStarted || _step59ControllerRehearsalUiStarted || _step59TransitionUiStarted || _step59MutationProbeUiStarted ||
            _transformedRealStS2VeryEarlyInitialization.Step59TransitionStarted || _transformedRealStS2VeryEarlyInitialization.Step59DiagnosticMutationProbeStarted)
        {
            SetStartupLadderRefusal(step, _step59ManagedPropertyBypassTransitionResultLabel!, _step59ManagedPropertyBypassTransitionDetailLabel!, "FRESH PROCESS REQUIRED", "Step 59Q preconditions the retained embark button and then arms the original transition. Relaunch and reproduce through Step 58 under the FULL profile with no prior Step-59 branch.");
            return;
        }
        if (_transformedRealStS2VeryEarlyInitialization.SelectedPropertyTweenerProfile != PropertyTweenerExperimentProfile.Full)
        {
            SetStartupLadderRefusal(step, _step59ManagedPropertyBypassTransitionResultLabel!, _step59ManagedPropertyBypassTransitionDetailLabel!, "FULL PROFILE REQUIRED", $"Step 59Q requires profile=Full, but this process uses {_transformedRealStS2VeryEarlyInitialization.SelectedPropertyTweenerProfile}. Relaunch and choose the FULL closed-path button before Step 35.");
            return;
        }
        if (!TryPrepareStep59Auxiliary(_step59ManagedPropertyBypassTransitionButton, _step59ManagedPropertyBypassTransitionResultLabel, _step59ManagedPropertyBypassTransitionDetailLabel, "Step 59Q", out var button, out var resultLabel, out var detailLabel)) return;
        if (!TryInitializeStartupLadderTelemetry(step, "Step-59 direct-managed embark compatibility plus original transition", "Step59Q-ManagedPropertyBypassTransition-StaticMap", out var error))
        {
            SetStartupLadderRefusal(step, resultLabel, detailLabel, "TELEMETRY FAIL / NOT RUN", error); return;
        }
        BeginSteamOperation(allowCancel: false); _step59Gates.Reset();
        try
        {
            WriteStartupLadderCheckpoint(step, "RUN_START_59Q — direct-managed embark compatibility preparation plus original OpenCharacterSelect transition started. No TweenProperty call is used during preparation; the original handler remains authoritative for submenu transition/state.");
            if (!RecordStartupLadderGate(_step59Gates, _transformedRealStS2VeryEarlyInitialization.RunStep59ClosedStep58Authority(!GodotStep15NativeBridge.IsRenderingActive, d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!RecordStartupLadderGate(_step59Gates, _transformedRealStS2VeryEarlyInitialization.RunStep59RealHandlerBinding(d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!WriteStartupLadderStaticMap(step, out var preflightMapError)) throw new IOException("Step 59Q preflight map write failed: " + preflightMapError);
            _transformedRealStS2VeryEarlyInitialization.MarkStep59StaticMapDurablyWritten();
            _step59MutationProbeUiStarted = true; button.Enabled = false;
            var prep = _transformedRealStS2VeryEarlyInitialization.RunStep59ManagedPropertyBypassPreparation(d => WriteStartupLadderCheckpoint(step, d));
            if (!WriteStartupLadderStaticMap(step, out var prepMapError)) throw new IOException("Step 59Q preparation map refresh failed: " + prepMapError);
            WriteStartupLadderCheckpoint(step, $"M59Q_PREP_STATIC_MAP_REFRESH_RETURNED — prepared={prep.Prepared}; diagnosticsLength={prep.Diagnostics.Length}; renderingStopped=True.");
            if (!prep.Prepared)
            {
                resultLabel.Text = "STEP 59Q: BYPASS PREPARATION FAILED";
                resultLabel.TextColor = UIColor.SystemOrange;
                detailLabel.Text = "The direct-managed compatibility preparation failed before the original handler was armed. Preserve the report and relaunch.";
                return;
            }

            _step59TransitionUiStarted = true;
            var transitionResult = _transformedRealStS2VeryEarlyInitialization.RunStep59RealHandlerTransition(d => WriteStartupLadderCheckpoint(step, d));
            var transitionPassed = RecordStartupLadderGate(_step59Gates, transitionResult, resultLabel, detailLabel);
            if (!WriteStartupLadderStaticMap(step, out var transitionMapError)) throw new IOException("Step 59Q transition/post-failure map refresh failed: " + transitionMapError);
            WriteStartupLadderCheckpoint(step, transitionPassed
                ? "M59Q_C_STATIC_MAP_REFRESH_RETURNED — original OpenCharacterSelect returned after direct-managed embark preconditioning; rendering remains stopped."
                : "M59Q_C_POSTFAIL_STATIC_MAP_REFRESH_RETURNED — original OpenCharacterSelect still failed after direct-managed embark preconditioning; evidence durably refreshed.");
            if (!transitionPassed) return;
            if (!RecordStartupLadderGate(_step59Gates, _transformedRealStS2VeryEarlyInitialization.RunStep59FrozenPostTransitionConfinement(!GodotStep15NativeBridge.IsRenderingActive, d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            CompleteStartupLadderStep(step, _step59Gates, resultLabel, detailLabel, "MANAGED-PROPERTY BYPASS + ORIGINAL CHARACTER-SELECT TRANSITION CLOSED 4/4. Step 60 is unlocked in this same process.");
            WriteStartupLadderCheckpoint(step, "RUN_STEP59Q_4OF4 — direct-managed embark compatibility allowed the original game-owned transition to close; Step 60 active-screen audit unlocked.");
        }
        catch (Exception ex) { HandleStartupLadderException(step, resultLabel, detailLabel, ex, mutationArmed: _step59MutationProbeUiStarted || _step59TransitionUiStarted || _transformedRealStS2VeryEarlyInitialization.Step59DiagnosticMutationProbeStarted || _transformedRealStS2VeryEarlyInitialization.Step59TransitionStarted); }
        finally { await FinishStartupLadderStepAsync(step, "Step59Q-ManagedPropertyBypassTransition.txt", "StS2 Launcher — Step 59Q Direct-Managed Embark Compatibility + Original Transition", resultLabel, detailLabel, "Step 59Q mutates retained button state and may arm the original transition. Never retry it in-process. If it closes 4/4, continue directly to Step 60."); }
    }
}
