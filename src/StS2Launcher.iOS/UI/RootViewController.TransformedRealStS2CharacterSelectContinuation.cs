using Foundation;
using StS2Launcher.Core;
using StS2Launcher.iOS.Platform;
using System.Diagnostics;

namespace StS2Launcher.iOS;

public sealed partial class RootViewController
{
    private readonly TransformedRealStS2StartupLadderGateSequence _step58Gates = new(58, "CHARACTER SELECT RUNTIME OWNERSHIP AUDIT");
    private readonly TransformedRealStS2StartupLadderGateSequence _step59Gates = new(59, TransformedRealStS2VeryEarlyInitialization.Step59GateName);
    private readonly TransformedRealStS2StartupLadderGateSequence _step60Gates = new(60, "ACTIVE CHARACTER SELECT SURFACE AUDIT");
    private readonly TransformedRealStS2StartupLadderGateSequence _step61Gates = new(61, "CHARACTER SELECT SHORT RENDER RESIDENCY");
    private readonly TransformedRealStS2StartupLadderGateSequence _step62Gates = new(62, "CHARACTER SELECT SUSTAINED RENDER RESIDENCY");

    private UIButton? _step58Button; private UIButton? _step59Button; private UIButton? _step60Button; private UIButton? _step61Button; private UIButton? _step62Button;
    private UIButton? _step59DiagnosticDeckButton;
    private UILabel? _step58ResultLabel; private UILabel? _step59ResultLabel; private UILabel? _step60ResultLabel; private UILabel? _step61ResultLabel; private UILabel? _step62ResultLabel;
    private UILabel? _step58DetailLabel; private UILabel? _step59DetailLabel; private UILabel? _step60DetailLabel; private UILabel? _step61DetailLabel; private UILabel? _step62DetailLabel;
    private UILabel? _step59DiagnosticDeckResultLabel; private UILabel? _step59DiagnosticDeckDetailLabel;
    private bool _step59TransitionUiStarted; private bool _step59DiagnosticDeckUiStarted; private bool _step61PulseUiStarted; private bool _step62PulseUiStarted;

    private void AddCharacterSelectContinuationControls(UIStackView content)
    {
        content.AddArrangedSubview(Separator());
        content.AddArrangedSubview(Label("Steps 58–62 — real character-select ownership + visible Godot render trial", UIFont.BoldSystemFontOfSize(18), UIColor.Label));
        content.AddArrangedSubview(Label(
            "Requires the physically proven Step 57 extension in the same fresh process. Physical 0.0.200 proved the PackedScene unique-name compatibility correction reaches the original character-select transition, which now fails inside retained embark NConfirmButton.OnEnable(). This 0.0.201 build is intentionally compilation-efficient: Step 59D gathers a deep no-mutation embark/button peer/field/IL/frontier/scene diagnostic deck and stops before the handler, while the normal Step 59 button remains available as a separate fresh-process real-transition experiment from the same compiled IPA. Any real-handler failure also captures a post-failure button matrix before returning. Steps 60–62 remain locked unless the real handler succeeds. Step 63/continuous interactive ownership remains unopened.",
            UIFont.SystemFontOfSize(13), UIColor.SecondaryLabel));

        (_step58Button, _step58ResultLabel, _step58DetailLabel) = AddStartupLadderStepControls(content,
            "Step 58.0 — adopt/audit actual game-owned character-select runtime state",
            "Run Step 58.0 A–D — AUDIT OWNERSHIP", "CHARACTER SELECT RUNTIME OWNERSHIP AUDIT: LOCKED",
            "Requires Step 57.0 4/4. Rebinds exact OpenCharacterSelect(NButton), proves its button parameter is unused, records the actual _characterSelectSubmenu state whether absent/off-tree/in-tree/visible, and re-audits the whole real handler frontier without invoking it. No artificial null/off-tree prerequisite remains.");
        _step58Button.TouchUpInside += async (_, _) => await RunStep58StartupLadderAsync();

        (_step59DiagnosticDeckButton, _step59DiagnosticDeckResultLabel, _step59DiagnosticDeckDetailLabel) = AddStartupLadderStepControls(content,
            "Step 59D — compilation-efficient deep diagnostic deck (NO HANDLER)",
            "Run Step 59D — DEEP DIAGNOSTICS ONLY", "STEP 59D DEEP DIAGNOSTIC DECK: LOCKED",
            "Recommended first 0.0.201 phone run after Step 58. Executes only Step-59 Gates A+B, writes the full ready/unique-name/embark field matrix plus whole-SceneTree NConfirmButton peer comparison, observational accessors, null-field live-node candidates, exact IL field/call order, and transitive execution frontiers. It intentionally stops before NSubmenuStack.Push/OpenCharacterSelect; no one-shot handler arm, field write, _Ready replay, OnEnable call, render restart, character input, or embark action.");
        _step59DiagnosticDeckButton.TouchUpInside += async (_, _) => await RunStep59DiagnosticDeckAsync();

        (_step59Button, _step59ResultLabel, _step59DetailLabel) = AddStartupLadderStepControls(content,
            "Step 59.0 — forensic prerequisites + one real OpenCharacterSelect transition",
            "Run Step 59.0 A–D — FORENSIC HANDLER", "FORENSIC REAL OPENCHARACTERSELECT TRANSITION: LOCKED",
            "Requires Step 58.0 4/4. This is the separate real-transition experiment from the same 0.0.201 IPA. Prefer Step 59D first and preserve its report. Gate B reruns the full deep diagnostic deck; unhealthy state blocks before any Push/OpenCharacterSelect arm. If the original handler is armed and still throws, 0.0.201 automatically captures a post-failure embark full-field/peer/null-candidate matrix and refreshes the durable static map before the UI returns. Rendering stays frozen.");
        _step59Button.TouchUpInside += async (_, _) => await RunStep59StartupLadderAsync();

        (_step60Button, _step60ResultLabel, _step60DetailLabel) = AddStartupLadderStepControls(content,
            "Step 60.0 — audit the actual active character-select subtree + frame/input surface",
            "Run Step 60.0 A–D — AUDIT ACTIVE SCREEN", "ACTIVE CHARACTER SELECT SURFACE AUDIT: LOCKED",
            "Requires Step 59.0 4/4. Audits the actual visible/in-tree subtree and immediate Godot frame/input callbacks under retained guards, then inventories exact TSCN signal-connection records. No rendering restart or intentional interaction occurs.");
        _step60Button.TouchUpInside += async (_, _) => await RunStep60StartupLadderAsync();

        (_step61Button, _step61ResultLabel, _step61DetailLabel) = AddStartupLadderStepControls(content,
            "Step 61.0 — short visible character-select render + synchronous refreeze",
            "Run Step 61.0 A–D — RENDER 2s", "CHARACTER SELECT SHORT RENDER RESIDENCY: LOCKED",
            $"Requires Step 60.0 4/4. Shows the real active character-select UI for approximately {TransformedRealStS2VeryEarlyInitialization.Step61CharacterSelectRenderTargetMilliseconds / 1000.0:F0} seconds, then synchronously stops rendering and proves the same visible/in-tree authority. Observation only; do not intentionally tap the game UI.");
        _step61Button.TouchUpInside += async (_, _) => await RunStep61StartupLadderAsync();

        (_step62Button, _step62ResultLabel, _step62DetailLabel) = AddStartupLadderStepControls(content,
            "Step 62.0 — sustained visible character-select render + synchronous refreeze",
            "Run Step 62.0 A–D — RENDER 10s", "CHARACTER SELECT SUSTAINED RENDER RESIDENCY: LOCKED",
            $"Requires Step 61.0 4/4. Leaves the real Godot character-select UI visibly rendering for approximately {TransformedRealStS2VeryEarlyInitialization.Step62CharacterSelectSustainedRenderTargetMilliseconds / 1000.0:F0} seconds, then refreezes and verifies confinement. Observation only; deliberate interaction and Step 63 remain unopened.");
        _step62Button.TouchUpInside += async (_, _) => await RunStep62StartupLadderAsync();
    }

    private (UIButton Button, UILabel Result, UILabel Detail) GetCharacterSelectContinuationControls(int step)
        => step switch
        {
            58 => (_step58Button!, _step58ResultLabel!, _step58DetailLabel!),
            59 => (_step59Button!, _step59ResultLabel!, _step59DetailLabel!),
            60 => (_step60Button!, _step60ResultLabel!, _step60DetailLabel!),
            61 => (_step61Button!, _step61ResultLabel!, _step61DetailLabel!),
            62 => (_step62Button!, _step62ResultLabel!, _step62DetailLabel!),
            _ => throw new ArgumentOutOfRangeException(nameof(step)),
        };

    private async Task RunStep58StartupLadderAsync()
    {
        const int step = 58;
        if (!TryPrepareStartupLadderStep(step, _step57Gates.Snapshot().Passed && _transformedRealStS2VeryEarlyInitialization.ExactStep57ClosurePassed,
                "Step 58.0 requires Step 57.0 4/4 retained-PackedScene/PCK authority in this same process.", out var button, out var resultLabel, out var detailLabel)) return;
        if (!TryInitializeStartupLadderTelemetry(step, "Character-select runtime ownership audit", "Step58-CharacterSelectOwnership-StaticMap", out var error)) { SetStartupLadderRefusal(step, resultLabel, detailLabel, "TELEMETRY FAIL / NOT RUN", error); return; }
        BeginSteamOperation(allowCancel: false); _step58Gates.Reset();
        try
        {
            WriteStartupLadderCheckpoint(step, "RUN_START — Step 58.0 actual game-owned character-select state + real OpenCharacterSelect frontier audit started; no mutation/rendering.");
            if (!RecordStartupLadderGate(_step58Gates, _transformedRealStS2VeryEarlyInitialization.RunStep58ClosedStep57Authority(!GodotStep15NativeBridge.IsRenderingActive, d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!RecordStartupLadderGate(_step58Gates, _transformedRealStS2VeryEarlyInitialization.RunStep58RuntimeOwnershipBinding(d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!RecordStartupLadderGate(_step58Gates, _transformedRealStS2VeryEarlyInitialization.RunStep58OpenCharacterSelectFrontierAudit(d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!WriteStartupLadderStaticMap(step, out var mapError)) throw new IOException("Step 58 ownership map write failed: " + mapError);
            _transformedRealStS2VeryEarlyInitialization.MarkStep58StaticMapDurablyWritten(); WriteStartupLadderCheckpoint(step, "M58_C_STATIC_MAP_WRITE_RETURNED — runtime ownership + real-handler frontier evidence durably written; no mutation occurred.");
            if (!RecordStartupLadderGate(_step58Gates, _transformedRealStS2VeryEarlyInitialization.RunStep58FrozenOwnershipConfinement(!GodotStep15NativeBridge.IsRenderingActive, d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            CompleteStartupLadderStep(step, _step58Gates, resultLabel, detailLabel, "RUNTIME OWNERSHIP AUDIT CLOSED 4/4. Step 59 may let the real game handler finish the transition only if needed."); button.Enabled = false;
            WriteStartupLadderCheckpoint(step, "RUN_STEP58_4OF4 — actual game-owned character-select state accepted/audited; Step 59 real-handler transition unlocked.");
        }
        catch (Exception ex) { HandleStartupLadderException(step, resultLabel, detailLabel, ex, mutationArmed: false); }
        finally { await FinishStartupLadderStepAsync(step, "Step58-TransformedRealStS2CharacterSelectOwnership.txt", "StS2 Launcher — Step 58.0 Character Select Runtime Ownership Audit", resultLabel, detailLabel, "Step 58 never invokes OpenCharacterSelect or restarts rendering."); }
    }

    private bool TryPrepareStep59DiagnosticDeck(out UIButton button, out UILabel resultLabel, out UILabel detailLabel)
    {
        button = _step59DiagnosticDeckButton!;
        resultLabel = _step59DiagnosticDeckResultLabel!;
        detailLabel = _step59DiagnosticDeckDetailLabel!;
        if (_statusLabel is null)
            return false;
        if (!CurrentReleasePresentation.BundleIdentityMatchesExpected)
        {
            SetStartupLadderRefusal(59, resultLabel, detailLabel, "RELEASE IDENTITY FAIL", "Built bundle identity does not match the source-pinned candidate.");
            return false;
        }
        if (!_step58Gates.Snapshot().Passed || !_transformedRealStS2VeryEarlyInitialization.ExactStep58ClosurePassed)
        {
            SetStartupLadderRefusal(59, resultLabel, detailLabel, "PREREQUISITE NOT MET", "Step 59D requires Step 58.0 4/4 runtime-ownership authority in this same fresh process.");
            return false;
        }
        if (GodotStep15NativeBridge.IsRenderingActive)
        {
            SetStartupLadderRefusal(59, resultLabel, detailLabel, "RENDERER MUST BE FROZEN", "Step 59D is observational and requires rendering stopped at entry.");
            return false;
        }
        return true;
    }

    private async Task RunStep59DiagnosticDeckAsync()
    {
        const int step = 59;
        if (_step59TransitionUiStarted || _transformedRealStS2VeryEarlyInitialization.Step59TransitionStarted)
        {
            SetStartupLadderRefusal(step, _step59DiagnosticDeckResultLabel!, _step59DiagnosticDeckDetailLabel!, "HANDLER ALREADY ARMED", "Step 59D must not run after the real Step-59 handler has armed in this process. Preserve reports and relaunch.");
            return;
        }
        if (_step59DiagnosticDeckUiStarted)
        {
            SetStartupLadderRefusal(step, _step59DiagnosticDeckResultLabel!, _step59DiagnosticDeckDetailLabel!, "DIAGNOSTIC ALREADY RUN", "The deep diagnostic deck already ran in this process. Preserve its report; use a fresh process for the separate real-transition experiment.");
            return;
        }
        if (!TryPrepareStep59DiagnosticDeck(out var button, out var resultLabel, out var detailLabel))
            return;
        if (!TryInitializeStartupLadderTelemetry(step, "Compilation-efficient Step-59 deep diagnostic deck", "Step59-DeepDiagnosticDeck-StaticMap", out var error))
        {
            SetStartupLadderRefusal(step, resultLabel, detailLabel, "TELEMETRY FAIL / NOT RUN", error);
            return;
        }
        BeginSteamOperation(allowCancel: false);
        _step59Gates.Reset();
        _step59DiagnosticDeckUiStarted = true;
        button.Enabled = false;
        try
        {
            WriteStartupLadderCheckpoint(step, "RUN_START_DIAGNOSTIC_DECK — Step 59D compilation-efficient observational deck started. Gates A+B only; no Push/OpenCharacterSelect arm is permitted in this operation.");
            if (!RecordStartupLadderGate(_step59Gates, _transformedRealStS2VeryEarlyInitialization.RunStep59ClosedStep58Authority(!GodotStep15NativeBridge.IsRenderingActive, d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!RecordStartupLadderGate(_step59Gates, _transformedRealStS2VeryEarlyInitialization.RunStep59RealHandlerBinding(d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!WriteStartupLadderStaticMap(step, out var mapError)) throw new IOException("Step 59D deep diagnostic map write failed: " + mapError);
            _transformedRealStS2VeryEarlyInitialization.MarkStep59StaticMapDurablyWritten();
            WriteStartupLadderCheckpoint(step, "M59D_B_STATIC_MAP_WRITE_RETURNED — complete compilation-efficient diagnostic deck durably written; handler arm=NO; mutation=NO; renderingStopped=True.");
            resultLabel.Text = "STEP 59D: DIAGNOSTIC DECK COMPLETE / NO HANDLER ARMED";
            resultLabel.TextColor = UIColor.SystemGreen;
            detailLabel.Text = _transformedRealStS2VeryEarlyInitialization.Step59ReadyBindingPreflightPassed
                ? "Deep preflight found no definite blocker. Preserve this report; the same compiled IPA can next run the separate real Step 59 transition in a fresh process if desired."
                : "Deep preflight found a definite blocker and stopped without arming the handler. Preserve the static map/report; no real Step-59 transition is needed until the evidence is reviewed.";
            if (_statusLabel is not null)
            {
                _statusLabel.Text = "STEP 59D COMPLETE — deep diagnostic evidence captured with no handler arm. Preserve reports; use a fresh process for any separate Step-59 transition run.";
                _statusLabel.TextColor = UIColor.SystemGreen;
            }
            WriteStartupLadderCheckpoint(step, $"RUN_STEP59D_COMPLETE — handlerAuthorized={_transformedRealStS2VeryEarlyInitialization.Step59ReadyBindingPreflightPassed}; handlerArmed=False; directRepair=False; renderingStopped=True.");
        }
        catch (Exception ex)
        {
            HandleStartupLadderException(step, resultLabel, detailLabel, ex, mutationArmed: false);
        }
        finally
        {
            await FinishStartupLadderStepAsync(step, "Step59D-CompilationEfficientDeepDiagnosticDeck.txt", "StS2 Launcher — Step 59D Compilation-Efficient Deep Diagnostic Deck", resultLabel, detailLabel, "Step 59D never arms Push/OpenCharacterSelect, writes fields, replays _Ready/OnEnable, restarts rendering, or opens Step 63.");
        }
    }

    private async Task RunStep59StartupLadderAsync()
    {
        const int step = 59;
        if (_step59DiagnosticDeckUiStarted) { var labels = GetStartupLadderLabels(step); SetStartupLadderRefusal(step, labels.Result, labels.Detail, "FRESH PROCESS REQUIRED", "Step 59D already ran in this process. Preserve its observational evidence and relaunch before the separate real-handler Step 59 experiment."); return; }
        if (_step59TransitionUiStarted || _transformedRealStS2VeryEarlyInitialization.Step59TransitionStarted) { var labels = GetStartupLadderLabels(step); SetStartupLadderRefusal(step, labels.Result, labels.Detail, "ONE-SHOT ALREADY ARMED", "Step 59 real handler was already armed in this process. Preserve reports and relaunch; never retry it in-process."); return; }
        if (!TryPrepareStartupLadderStep(step, _step58Gates.Snapshot().Passed && _transformedRealStS2VeryEarlyInitialization.ExactStep58ClosurePassed,
                "Step 59.0 requires Step 58.0 4/4 runtime-ownership authority in this same process.", out var button, out var resultLabel, out var detailLabel)) return;
        if (!TryInitializeStartupLadderTelemetry(step, "Forensic real OpenCharacterSelect frozen transition", "Step59-RealOpenCharacterSelect-StaticMap", out var error)) { SetStartupLadderRefusal(step, resultLabel, detailLabel, "TELEMETRY FAIL / NOT RUN", error); return; }
        BeginSteamOperation(allowCancel: false); _step59Gates.Reset();
        try
        {
            WriteStartupLadderCheckpoint(step, "RUN_START — Step 59.0 forensic prerequisite snapshot + original OpenCharacterSelect transition started. No speculative character-select repair is performed; any previously audited submenu Push occurs only if the retained single-player logical stack is actually null.");
            if (!RecordStartupLadderGate(_step59Gates, _transformedRealStS2VeryEarlyInitialization.RunStep59ClosedStep58Authority(!GodotStep15NativeBridge.IsRenderingActive, d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!RecordStartupLadderGate(_step59Gates, _transformedRealStS2VeryEarlyInitialization.RunStep59RealHandlerBinding(d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!WriteStartupLadderStaticMap(step, out var preflightMapError)) throw new IOException("Step 59 forensic preflight map write failed: " + preflightMapError);
            _transformedRealStS2VeryEarlyInitialization.MarkStep59StaticMapDurablyWritten(); WriteStartupLadderCheckpoint(step, "M59_B_STATIC_MAP_WRITE_RETURNED — ready-binding/scene/TSCN forensic evidence durably written before Gate C; no Push/OpenCharacterSelect mutation has armed.");
            if (_transformedRealStS2VeryEarlyInitialization.Step59HandlerInvocationRequired && _transformedRealStS2VeryEarlyInitialization.Step59ReadyBindingPreflightPassed) { _step59TransitionUiStarted = true; button.Enabled = false; }
            var transitionResult = _transformedRealStS2VeryEarlyInitialization.RunStep59RealHandlerTransition(d => WriteStartupLadderCheckpoint(step, d));
            var transitionPassed = RecordStartupLadderGate(_step59Gates, transitionResult, resultLabel, detailLabel);
            if (!WriteStartupLadderStaticMap(step, out var mapError)) throw new IOException("Step 59 transition/post-failure map refresh failed: " + mapError);
            WriteStartupLadderCheckpoint(step, transitionPassed
                ? "M59_C_STATIC_MAP_REFRESH_RETURNED — successful game-owned transition evidence appended to the already-durable forensic map; rendering remains stopped."
                : "M59_C_POSTFAIL_STATIC_MAP_REFRESH_RETURNED — failed original-handler evidence plus post-failure confirm-button runtime deck durably refreshed before UI return; rendering remains stopped.");
            if (!transitionPassed) return;
            if (!RecordStartupLadderGate(_step59Gates, _transformedRealStS2VeryEarlyInitialization.RunStep59FrozenPostTransitionConfinement(!GodotStep15NativeBridge.IsRenderingActive, d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            CompleteStartupLadderStep(step, _step59Gates, resultLabel, detailLabel, "GAME-OWNED SUBMENU + CHARACTER-SELECT TRANSITION CLOSED 4/4. The real screen is visible/in-tree and logically bound; Step 60 active-surface audit is unlocked."); button.Enabled = false;
            WriteStartupLadderCheckpoint(step, "RUN_STEP59_4OF4 — game-owned submenu-stack binding and character-select transition/adoption closed; Step 60 actual active-screen audit unlocked.");
        }
        catch (Exception ex) { HandleStartupLadderException(step, resultLabel, detailLabel, ex, mutationArmed: _step59TransitionUiStarted || _transformedRealStS2VeryEarlyInitialization.Step59TransitionStarted); }
        finally { await FinishStartupLadderStepAsync(step, "Step59-TransformedRealStS2OpenCharacterSelectFrozen.txt", "StS2 Launcher — Step 59.0 Forensic Real OpenCharacterSelect Frozen Transition", resultLabel, detailLabel, "Step 59 leaves rendering frozen. If the real handler was armed, never retry Step 59 in-process."); }
    }

    private async Task RunStep60StartupLadderAsync()
    {
        const int step = 60;
        if (!TryPrepareStartupLadderStep(step, _step59Gates.Snapshot().Passed && _transformedRealStS2VeryEarlyInitialization.ExactStep59ClosurePassed,
                "Step 60.0 requires Step 59.0 4/4 visible/in-tree game-owned character-select authority.", out var button, out var resultLabel, out var detailLabel)) return;
        if (!TryInitializeStartupLadderTelemetry(step, "Active character-select surface audit", "Step60-CharacterSelectActiveSurface-StaticMap", out var error)) { SetStartupLadderRefusal(step, resultLabel, detailLabel, "TELEMETRY FAIL / NOT RUN", error); return; }
        BeginSteamOperation(allowCancel: false); _step60Gates.Reset();
        try
        {
            WriteStartupLadderCheckpoint(step, "RUN_START — Step 60.0 actual active character-select subtree frame/input + exact TSCN connection inventory audit started; rendering remains frozen.");
            if (!RecordStartupLadderGate(_step60Gates, _transformedRealStS2VeryEarlyInitialization.RunStep60ClosedStep59Authority(!GodotStep15NativeBridge.IsRenderingActive, d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!RecordStartupLadderGate(_step60Gates, _transformedRealStS2VeryEarlyInitialization.RunStep60ActiveFrameInputAudit(d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!RecordStartupLadderGate(_step60Gates, _transformedRealStS2VeryEarlyInitialization.RunStep60SceneConnectionInventory(d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!WriteStartupLadderStaticMap(step, out var mapError)) throw new IOException("Step 60 active-surface map write failed: " + mapError);
            _transformedRealStS2VeryEarlyInitialization.MarkStep60StaticMapDurablyWritten(); WriteStartupLadderCheckpoint(step, "M60_C_STATIC_MAP_WRITE_RETURNED — active screen/frame/input + TSCN connection evidence durably written.");
            if (!RecordStartupLadderGate(_step60Gates, _transformedRealStS2VeryEarlyInitialization.RunStep60FrozenSurfaceConfinement(!GodotStep15NativeBridge.IsRenderingActive, d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            CompleteStartupLadderStep(step, _step60Gates, resultLabel, detailLabel, "ACTIVE CHARACTER-SELECT SURFACE AUDIT CLOSED 4/4. Step 61 short visible render/refreeze is unlocked."); button.Enabled = false;
            WriteStartupLadderCheckpoint(step, "RUN_STEP60_4OF4 — actual active character-select surface audited; Step 61 visible render trial unlocked.");
        }
        catch (Exception ex) { HandleStartupLadderException(step, resultLabel, detailLabel, ex, mutationArmed: false); }
        finally { await FinishStartupLadderStepAsync(step, "Step60-TransformedRealStS2CharacterSelectActiveSurface.txt", "StS2 Launcher — Step 60.0 Active Character Select Surface Audit", resultLabel, detailLabel, "Step 60 never restarts rendering or intentionally interacts with the game UI."); }
    }

    private async Task RunStep61StartupLadderAsync()
    {
        const int step = 61;
        if (_step61PulseUiStarted || _transformedRealStS2VeryEarlyInitialization.Step61PulseStarted) { var labels = GetStartupLadderLabels(step); SetStartupLadderRefusal(step, labels.Result, labels.Detail, "ONE-SHOT ALREADY ARMED", "Step 61 render residency was already armed in this process. Preserve reports and relaunch."); return; }
        if (!TryPrepareStartupLadderStep(step, _step60Gates.Snapshot().Passed && _transformedRealStS2VeryEarlyInitialization.ExactStep60ClosurePassed,
                "Step 61.0 requires Step 60.0 4/4 active-surface authority.", out var button, out var resultLabel, out var detailLabel)) return;
        if (!TryInitializeStartupLadderTelemetry(step, "Short visible character-select render residency", "Step61-CharacterSelectShortRender-StaticMap", out var error)) { SetStartupLadderRefusal(step, resultLabel, detailLabel, "TELEMETRY FAIL / NOT RUN", error); return; }
        BeginSteamOperation(allowCancel: false); _step61Gates.Reset();
        try
        {
            WriteStartupLadderCheckpoint(step, "RUN_START — Step 61.0 short visible character-select render/refreeze trial started; observation only, no intentional interaction.");
            if (!RecordStartupLadderGate(_step61Gates, _transformedRealStS2VeryEarlyInitialization.RunStep61ClosedStep60Authority(!GodotStep15NativeBridge.IsRenderingActive, d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!RecordStartupLadderGate(_step61Gates, _transformedRealStS2VeryEarlyInitialization.RunStep61RenderPreflight(d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!WriteStartupLadderStaticMap(step, out var mapError)) throw new IOException("Step 61 render preflight map write failed: " + mapError);
            _transformedRealStS2VeryEarlyInitialization.MarkStep61StaticMapDurablyWritten(); WriteStartupLadderCheckpoint(step, "M61_B_STATIC_MAP_WRITE_RETURNED — short-render preflight durable before StartRendering.");
            if (!NSThread.IsMain) throw new InvalidOperationException("Step 61.0 render residency must be armed on UIKit main thread.");
            _transformedRealStS2VeryEarlyInitialization.BeginStep61BoundedRenderPulse(d => WriteStartupLadderCheckpoint(step, d)); _step61PulseUiStarted = true; button.Enabled = false;
            var stopwatch = Stopwatch.StartNew(); var startReturned = GodotStep15NativeBridge.StartRendering(); var activeAfterStart = GodotStep15NativeBridge.IsRenderingActive;
            WriteStartupLadderCheckpoint(step, $"M61_C_START_RENDERING_RETURNED — returned={startReturned}; activeAfterStart={activeAfterStart}; nativeError='{SanitizeStartupLadderCheckpoint(GodotStep15NativeBridge.LastError)}'.");
            if (startReturned && activeAfterStart) await Task.Delay(TransformedRealStS2VeryEarlyInitialization.Step61CharacterSelectRenderTargetMilliseconds);
            var stopReturned = GodotStep15NativeBridge.StopRendering(); var activeAfterStop = GodotStep15NativeBridge.IsRenderingActive; stopwatch.Stop();
            if (!RecordStartupLadderGate(_step61Gates, _transformedRealStS2VeryEarlyInitialization.RunStep61RenderPulseEvidence(startReturned, activeAfterStart, stopReturned, activeAfterStop, stopwatch.Elapsed.TotalMilliseconds, d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!RecordStartupLadderGate(_step61Gates, _transformedRealStS2VeryEarlyInitialization.RunStep61FrozenPostResidencyConfinement(!GodotStep15NativeBridge.IsRenderingActive, d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            CompleteStartupLadderStep(step, _step61Gates, resultLabel, detailLabel, "SHORT VISIBLE CHARACTER-SELECT RENDER CLOSED 4/4. Step 62 sustained visible render/refreeze is unlocked.");
            WriteStartupLadderCheckpoint(step, "RUN_STEP61_4OF4 — short visible character-select render completed/refroze; Step 62 sustained trial unlocked.");
        }
        catch (Exception ex) { if (GodotStep15NativeBridge.IsRenderingActive) GodotStep15NativeBridge.StopRendering(); HandleStartupLadderException(step, resultLabel, detailLabel, ex, mutationArmed: _step61PulseUiStarted || _transformedRealStS2VeryEarlyInitialization.Step61PulseStarted); }
        finally { await FinishStartupLadderStepAsync(step, "Step61-TransformedRealStS2CharacterSelectShortRender.txt", "StS2 Launcher — Step 61.0 Character Select Short Render Residency", resultLabel, detailLabel, "Step 61 always attempts to leave rendering frozen after its one-shot residency."); }
    }

    private async Task RunStep62StartupLadderAsync()
    {
        const int step = 62;
        if (_step62PulseUiStarted || _transformedRealStS2VeryEarlyInitialization.Step62PulseStarted) { var labels = GetStartupLadderLabels(step); SetStartupLadderRefusal(step, labels.Result, labels.Detail, "ONE-SHOT ALREADY ARMED", "Step 62 sustained render residency was already armed in this process. Preserve reports and relaunch."); return; }
        if (!TryPrepareStartupLadderStep(step, _step61Gates.Snapshot().Passed && _transformedRealStS2VeryEarlyInitialization.ExactStep61ClosurePassed,
                "Step 62.0 requires Step 61.0 4/4 short visible render/refreeze authority.", out var button, out var resultLabel, out var detailLabel)) return;
        if (!TryInitializeStartupLadderTelemetry(step, "Sustained visible character-select render residency", "Step62-CharacterSelectSustainedRender-StaticMap", out var error)) { SetStartupLadderRefusal(step, resultLabel, detailLabel, "TELEMETRY FAIL / NOT RUN", error); return; }
        BeginSteamOperation(allowCancel: false); _step62Gates.Reset();
        try
        {
            WriteStartupLadderCheckpoint(step, "RUN_START — Step 62.0 sustained visible character-select render/refreeze trial started; observation only, do not intentionally interact.");
            if (!RecordStartupLadderGate(_step62Gates, _transformedRealStS2VeryEarlyInitialization.RunStep62ClosedStep61Authority(!GodotStep15NativeBridge.IsRenderingActive, d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!RecordStartupLadderGate(_step62Gates, _transformedRealStS2VeryEarlyInitialization.RunStep62SustainedRenderPreflight(d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!WriteStartupLadderStaticMap(step, out var mapError)) throw new IOException("Step 62 sustained-render preflight map write failed: " + mapError);
            _transformedRealStS2VeryEarlyInitialization.MarkStep62StaticMapDurablyWritten(); WriteStartupLadderCheckpoint(step, "M62_B_STATIC_MAP_WRITE_RETURNED — sustained-render preflight durable before StartRendering.");
            if (!NSThread.IsMain) throw new InvalidOperationException("Step 62.0 sustained residency must be armed on UIKit main thread.");
            _transformedRealStS2VeryEarlyInitialization.BeginStep62SustainedRenderPulse(d => WriteStartupLadderCheckpoint(step, d)); _step62PulseUiStarted = true; button.Enabled = false;
            var stopwatch = Stopwatch.StartNew(); var startReturned = GodotStep15NativeBridge.StartRendering(); var activeAfterStart = GodotStep15NativeBridge.IsRenderingActive;
            WriteStartupLadderCheckpoint(step, $"M62_C_START_RENDERING_RETURNED — returned={startReturned}; activeAfterStart={activeAfterStart}; nativeError='{SanitizeStartupLadderCheckpoint(GodotStep15NativeBridge.LastError)}'.");
            if (startReturned && activeAfterStart) await Task.Delay(TransformedRealStS2VeryEarlyInitialization.Step62CharacterSelectSustainedRenderTargetMilliseconds);
            var stopReturned = GodotStep15NativeBridge.StopRendering(); var activeAfterStop = GodotStep15NativeBridge.IsRenderingActive; stopwatch.Stop();
            if (!RecordStartupLadderGate(_step62Gates, _transformedRealStS2VeryEarlyInitialization.RunStep62SustainedRenderEvidence(startReturned, activeAfterStart, stopReturned, activeAfterStop, stopwatch.Elapsed.TotalMilliseconds, d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!RecordStartupLadderGate(_step62Gates, _transformedRealStS2VeryEarlyInitialization.RunStep62FrozenPostResidencyConfinement(!GodotStep15NativeBridge.IsRenderingActive, d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            CompleteStartupLadderStep(step, _step62Gates, resultLabel, detailLabel, "SUSTAINED VISIBLE CHARACTER-SELECT RENDER CLOSED 4/4. Candidate stops here; Step 63/continuous interactive ownership remains unopened.");
            WriteStartupLadderCheckpoint(step, "RUN_STEP62_4OF4 — sustained real character-select render completed/refroze; Step 63 remains unopened.");
        }
        catch (Exception ex) { if (GodotStep15NativeBridge.IsRenderingActive) GodotStep15NativeBridge.StopRendering(); HandleStartupLadderException(step, resultLabel, detailLabel, ex, mutationArmed: _step62PulseUiStarted || _transformedRealStS2VeryEarlyInitialization.Step62PulseStarted); }
        finally { await FinishStartupLadderStepAsync(step, "Step62-TransformedRealStS2CharacterSelectSustainedRender.txt", "StS2 Launcher — Step 62.0 Character Select Sustained Render Residency", resultLabel, detailLabel, "Step 62 always attempts to leave rendering frozen. Intentional interaction and Step 63 are not opened by this candidate."); }
    }
}
