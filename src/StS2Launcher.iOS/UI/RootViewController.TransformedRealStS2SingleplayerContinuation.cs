using Foundation;
using StS2Launcher.Core;
using StS2Launcher.iOS.Platform;
using System.Diagnostics;

namespace StS2Launcher.iOS;

public sealed partial class RootViewController
{
    private readonly TransformedRealStS2StartupLadderGateSequence _step53Gates = new(53, "SINGLEPLAYER OPEN FRONTIER");
    private readonly TransformedRealStS2StartupLadderGateSequence _step54Gates = new(54, "SINGLEPLAYER SUBMENU FROZEN OPEN");
    private readonly TransformedRealStS2StartupLadderGateSequence _step55Gates = new(55, "SINGLEPLAYER SUBMENU RENDER RESIDENCY");
    private readonly TransformedRealStS2StartupLadderGateSequence _step56Gates = new(56, "CHARACTER SELECT FRONTIER MAP");
    private readonly TransformedRealStS2StartupLadderGateSequence _step57Gates = new(57, "CHARACTER SELECT RESOURCE PREFLIGHT");

    private UIButton? _step53Button;
    private UIButton? _step54Button;
    private UIButton? _step55Button;
    private UIButton? _step56Button;
    private UIButton? _step57Button;
    private UILabel? _step53ResultLabel;
    private UILabel? _step54ResultLabel;
    private UILabel? _step55ResultLabel;
    private UILabel? _step56ResultLabel;
    private UILabel? _step57ResultLabel;
    private UILabel? _step53DetailLabel;
    private UILabel? _step54DetailLabel;
    private UILabel? _step55DetailLabel;
    private UILabel? _step56DetailLabel;
    private UILabel? _step57DetailLabel;
    private bool _step54OpenUiStarted;
    private bool _step55PulseUiStarted;

    private void AddSingleplayerContinuationControls(UIStackView content)
    {
        content.AddArrangedSubview(Separator());
        content.AddArrangedSubview(Label(
            "Steps 53–57 — single-player submenu continuation (stop on first failure)",
            UIFont.BoldSystemFontOfSize(18),
            UIColor.Label));
        content.AddArrangedSubview(Label(
            "Physical authority through Step 52 remains unchanged. Step 53 isolates only exact OpenSingleplayerSubmenu and must prove that direct frontier admissible before Step 54 may invoke it once while frozen. Step 55 audits/renders the resulting real submenu and refreezes. Steps 56–57 map the exact OpenCharacterSelect transition and inspect its discovered PCK resource bytes without invoking or loading character select. No Step 58+ behavior is present in this candidate.",
            UIFont.SystemFontOfSize(13),
            UIColor.SecondaryLabel));

        (_step53Button, _step53ResultLabel, _step53DetailLabel) = AddStartupLadderStepControls(
            content,
            "Step 53.0 — isolate exact OpenSingleplayerSubmenu execution frontier (NO invocation)",
            "Run Step 53.0 A–D — MAP ONLY",
            "SINGLEPLAYER OPEN FRONTIER: LOCKED",
            "Requires Step 52.0 4/4/refrozen. Binds zero-arg void NMainMenu.OpenSingleplayerSubmenu, audits only that method under retained runtime guards, and requires every immediate boundary admissible. SingleplayerButtonPressed remains evidence-only and uninvoked.");
        _step53Button.TouchUpInside += async (_, _) => await RunStep53StartupLadderAsync();

        (_step54Button, _step54ResultLabel, _step54DetailLabel) = AddStartupLadderStepControls(
            content,
            "Step 54.0 — one-shot frozen OpenSingleplayerSubmenu + visible real submenu authority",
            "Run Step 54.0 A–D — OPEN ONCE",
            "SINGLEPLAYER SUBMENU FROZEN OPEN: LOCKED",
            "Requires Step 53.0 4/4 admissible direct-open authority. Token-matches the runtime method, binds exact NMainMenu._singleplayerSubmenu, writes that map, then invokes only OpenSingleplayerSubmenu once while rendering stays stopped. Success requires the exact retained NSingleplayerSubmenu to become visible in-tree with zero context/native drift.");
        _step54Button.TouchUpInside += async (_, _) => await RunStep54StartupLadderAsync();

        (_step55Button, _step55ResultLabel, _step55DetailLabel) = AddStartupLadderStepControls(
            content,
            "Step 55.0 — actual single-player submenu callback audit + bounded render/refreeze",
            "Run Step 55.0 A–D — SUBMENU RENDER",
            "SINGLEPLAYER SUBMENU RENDER RESIDENCY: LOCKED",
            $"Requires Step 54.0 4/4. Audits the actual visible NSingleplayerSubmenu subtree frame/input callbacks under retained guards, writes the map before rendering, then runs one {TransformedRealStS2VeryEarlyInitialization.Step55SingleplayerRenderTargetMilliseconds} ms render residency with synchronous StopRendering before telemetry. Final state must be frozen.");
        _step55Button.TouchUpInside += async (_, _) => await RunStep55StartupLadderAsync();

        (_step56Button, _step56ResultLabel, _step56DetailLabel) = AddStartupLadderStepControls(
            content,
            "Step 56.0 — exact NSingleplayerSubmenu.OpenCharacterSelect frontier + resource-literal discovery",
            "Run Step 56.0 A–D — MAP ONLY",
            "CHARACTER SELECT FRONTIER MAP: LOCKED",
            "Requires Step 55.0 4/4/refrozen. Binds exact zero-arg void OpenCharacterSelect, records IL, maps its execution/deferred frontier without invocation, and records res:// string literals / character-select .tscn candidates from the immediate closure. Classified boundaries are evidence only.");
        _step56Button.TouchUpInside += async (_, _) => await RunStep56StartupLadderAsync();

        (_step57Button, _step57ResultLabel, _step57DetailLabel) = AddStartupLadderStepControls(
            content,
            "Step 57.0 — exact character-select PCK resource preflight (NO ResourceLoader)",
            "Run Step 57.0 A–D — READ-ONLY PREFLIGHT",
            "CHARACTER SELECT RESOURCE PREFLIGHT: LOCKED",
            "Requires Step 56.0 4/4 and exactly one character-select .tscn candidate. Reads only that entry from the receipt-backed PCK, validates directory MD5, records SHA-256 and referenced res:// paths, and scans textual Spine/FMOD/GDExtension/native tokens. It never calls OpenCharacterSelect, ResourceLoader, PackedScene.Instantiate, or rendering.");
        _step57Button.TouchUpInside += async (_, _) => await RunStep57StartupLadderAsync();
    }

    private (UIButton Button, UILabel Result, UILabel Detail) GetSingleplayerContinuationControls(int step)
        => step switch
        {
            53 => (_step53Button!, _step53ResultLabel!, _step53DetailLabel!),
            54 => (_step54Button!, _step54ResultLabel!, _step54DetailLabel!),
            55 => (_step55Button!, _step55ResultLabel!, _step55DetailLabel!),
            56 => (_step56Button!, _step56ResultLabel!, _step56DetailLabel!),
            57 => (_step57Button!, _step57ResultLabel!, _step57DetailLabel!),
            _ => throw new ArgumentOutOfRangeException(nameof(step)),
        };

    private async Task RunStep53StartupLadderAsync()
    {
        const int step = 53;
        if (!TryPrepareStartupLadderStep(step, _step52Gates.Snapshot().Passed && _transformedRealStS2VeryEarlyInitialization.ExactStep52ClosurePassed,
                "Step 53.0 requires Step 52.0 4/4 sustained main-menu render/refreeze authority in this same process.", out var button, out var resultLabel, out var detailLabel))
            return;
        if (!TryInitializeStartupLadderTelemetry(step, "Single-player submenu-open frontier", "Step53-SingleplayerOpen-StaticMap", out var error))
        {
            SetStartupLadderRefusal(step, resultLabel, detailLabel, "TELEMETRY FAIL / NOT RUN", error);
            return;
        }
        BeginSteamOperation(allowCancel: false);
        _step53Gates.Reset();
        try
        {
            WriteStartupLadderCheckpoint(step, "RUN_START — Step 53.0 exact OpenSingleplayerSubmenu frontier map started from physically closed/refrozen Step-52 authority. No single-player handler is invoked.");
            if (!RecordStartupLadderGate(_step53Gates, _transformedRealStS2VeryEarlyInitialization.RunStep53ClosedStep52Authority(!GodotStep15NativeBridge.IsRenderingActive, d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!RecordStartupLadderGate(_step53Gates, _transformedRealStS2VeryEarlyInitialization.RunStep53SingleplayerOpenBinding(d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!RecordStartupLadderGate(_step53Gates, _transformedRealStS2VeryEarlyInitialization.RunStep53SingleplayerOpenFrontierAudit(d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!WriteStartupLadderStaticMap(step, out var mapError)) throw new IOException("Step 53 single-player open frontier map write failed: " + mapError);
            _transformedRealStS2VeryEarlyInitialization.MarkStep53StaticMapDurablyWritten();
            WriteStartupLadderCheckpoint(step, "M53_C_STATIC_MAP_WRITE_RETURNED — exact admissible OpenSingleplayerSubmenu frontier durably written; handler invocation remains NO and rendering remains stopped.");
            if (!RecordStartupLadderGate(_step53Gates, _transformedRealStS2VeryEarlyInitialization.RunStep53FrozenNoInvocationConfinement(!GodotStep15NativeBridge.IsRenderingActive, d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            CompleteStartupLadderStep(step, _step53Gates, resultLabel, detailLabel,
                "EXACT SINGLE-PLAYER SUBMENU OPEN FRONTIER CLOSED 4/4. Only OpenSingleplayerSubmenu is admissible/authorized for the next one-shot rung; SingleplayerButtonPressed remains uninvoked.");
            button.Enabled = false;
            WriteStartupLadderCheckpoint(step, "RUN_STEP53_4OF4 — admissible exact submenu-open frontier closed; Step 54 may invoke only OpenSingleplayerSubmenu once while frozen.");
        }
        catch (Exception ex)
        {
            HandleStartupLadderException(step, resultLabel, detailLabel, ex, mutationArmed: false);
        }
        finally
        {
            await FinishStartupLadderStepAsync(step, "Step53-TransformedRealStS2SingleplayerOpenFrontier.txt", "StS2 Launcher — Step 53.0 Singleplayer Open Frontier", resultLabel, detailLabel,
                "Step 53 never invokes a menu handler or restarts rendering.");
        }
    }

    private async Task RunStep54StartupLadderAsync()
    {
        const int step = 54;
        if (_step54OpenUiStarted)
        {
            var labels = GetStartupLadderLabels(step);
            SetStartupLadderRefusal(step, labels.Result, labels.Detail, "ONE-SHOT ALREADY ARMED", "Step 54 exact OpenSingleplayerSubmenu invocation was already armed in this process. Preserve reports and relaunch; never retry.");
            return;
        }
        if (!TryPrepareStartupLadderStep(step, _step53Gates.Snapshot().Passed && _transformedRealStS2VeryEarlyInitialization.ExactStep53ClosurePassed,
                "Step 54.0 requires Step 53.0 4/4 durable admissible OpenSingleplayerSubmenu authority in this same process.", out var button, out var resultLabel, out var detailLabel))
            return;
        if (!TryInitializeStartupLadderTelemetry(step, "Frozen single-player submenu open", "Step54-SingleplayerSubmenuOpen-StaticMap", out var error))
        {
            SetStartupLadderRefusal(step, resultLabel, detailLabel, "TELEMETRY FAIL / NOT RUN", error);
            return;
        }
        BeginSteamOperation(allowCancel: false);
        _step54Gates.Reset();
        try
        {
            WriteStartupLadderCheckpoint(step, "RUN_START — Step 54.0 one-shot frozen OpenSingleplayerSubmenu started. SingleplayerButtonPressed remains uninvoked; rendering must stay stopped.");
            if (!RecordStartupLadderGate(_step54Gates, _transformedRealStS2VeryEarlyInitialization.RunStep54ClosedStep53Authority(!GodotStep15NativeBridge.IsRenderingActive, d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!RecordStartupLadderGate(_step54Gates, _transformedRealStS2VeryEarlyInitialization.RunStep54SingleplayerSubmenuRuntimeBinding(d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!WriteStartupLadderStaticMap(step, out var mapError)) throw new IOException("Step 54 runtime binding map write failed before one-shot submenu open: " + mapError);
            _transformedRealStS2VeryEarlyInitialization.MarkStep54StaticMapDurablyWritten();
            WriteStartupLadderCheckpoint(step, "M54_B_STATIC_MAP_WRITE_RETURNED — runtime token/submenu identity map durably written before exact OpenSingleplayerSubmenu invocation.");
            _step54OpenUiStarted = true;
            button.Enabled = false;
            WriteStartupLadderCheckpoint(step, "M54_C_UI_ARMED — first/only exact OpenSingleplayerSubmenu invocation authorized; no in-process retry after this checkpoint.");
            if (!RecordStartupLadderGate(_step54Gates, _transformedRealStS2VeryEarlyInitialization.RunStep54ControlledSingleplayerSubmenuOpen(d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!RecordStartupLadderGate(_step54Gates, _transformedRealStS2VeryEarlyInitialization.RunStep54FrozenOpenConfinement(!GodotStep15NativeBridge.IsRenderingActive, d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            CompleteStartupLadderStep(step, _step54Gates, resultLabel, detailLabel,
                "REAL SINGLE-PLAYER SUBMENU FROZEN OPEN CLOSED 4/4. Exact NSingleplayerSubmenu is visible/in-tree and renderer remains frozen; Step 55 submenu callback/render audit is unlocked.");
            WriteStartupLadderCheckpoint(step, "RUN_STEP54_4OF4 — exact OpenSingleplayerSubmenu returned once with real visible/in-tree submenu authority; Step 55 may audit/render it once.");
        }
        catch (Exception ex)
        {
            HandleStartupLadderException(step, resultLabel, detailLabel, ex, mutationArmed: _step54OpenUiStarted);
        }
        finally
        {
            await FinishStartupLadderStepAsync(step, "Step54-TransformedRealStS2SingleplayerSubmenuFrozenOpen.txt", "StS2 Launcher — Step 54.0 Singleplayer Submenu Frozen Open", resultLabel, detailLabel,
                "If the one-shot submenu-open boundary was armed, never retry Step 54 in-process.");
        }
    }

    private async Task RunStep55StartupLadderAsync()
    {
        const int step = 55;
        if (_step55PulseUiStarted)
        {
            var labels = GetStartupLadderLabels(step);
            SetStartupLadderRefusal(step, labels.Result, labels.Detail, "ONE-SHOT ALREADY ARMED", "Step 55 submenu render pulse was already armed in this process. Preserve reports and relaunch; never retry StartRendering.");
            return;
        }
        if (!TryPrepareStartupLadderStep(step, _step54Gates.Snapshot().Passed && _transformedRealStS2VeryEarlyInitialization.ExactStep54ClosurePassed,
                "Step 55.0 requires Step 54.0 4/4 real visible/in-tree NSingleplayerSubmenu authority in this same process.", out var button, out var resultLabel, out var detailLabel))
            return;
        if (!TryInitializeStartupLadderTelemetry(step, "Single-player submenu render residency", "Step55-SingleplayerSubmenuFrameInput-StaticMap", out var error))
        {
            SetStartupLadderRefusal(step, resultLabel, detailLabel, "TELEMETRY FAIL / NOT RUN", error);
            return;
        }
        BeginSteamOperation(allowCancel: false);
        _step55Gates.Reset();
        try
        {
            WriteStartupLadderCheckpoint(step, "RUN_START — Step 55.0 actual NSingleplayerSubmenu frame/input audit + bounded render residency started from frozen Step-54 authority.");
            if (!RecordStartupLadderGate(_step55Gates, _transformedRealStS2VeryEarlyInitialization.RunStep55ClosedStep54Authority(!GodotStep15NativeBridge.IsRenderingActive, d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!RecordStartupLadderGate(_step55Gates, _transformedRealStS2VeryEarlyInitialization.RunStep55SingleplayerSubmenuFrameInputAudit(d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!WriteStartupLadderStaticMap(step, out var mapError)) throw new IOException("Step 55 submenu frame/input map write failed before StartRendering: " + mapError);
            _transformedRealStS2VeryEarlyInitialization.MarkStep55StaticMapDurablyWritten();
            WriteStartupLadderCheckpoint(step, "M55_B_STATIC_MAP_WRITE_RETURNED — actual submenu frame/input map durably written before StartRendering.");
            if (!NSThread.IsMain)
                throw new InvalidOperationException("Step 55.0 submenu render pulse must be armed on UIKit main thread.");
            _transformedRealStS2VeryEarlyInitialization.BeginStep55SingleplayerRenderPulse(d => WriteStartupLadderCheckpoint(step, d));
            _step55PulseUiStarted = true;
            button.Enabled = false;
            WriteStartupLadderCheckpoint(step, $"M55_C_START_RENDERING_CALL — invoking StartRendering exactly once for submenu residency; managedThread={Environment.CurrentManagedThreadId}; isMain={NSThread.IsMain}; activeBefore={GodotStep15NativeBridge.IsRenderingActive}.");
            var stopwatch = Stopwatch.StartNew();
            var startReturned = GodotStep15NativeBridge.StartRendering();
            var activeAfterStart = GodotStep15NativeBridge.IsRenderingActive;
            WriteStartupLadderCheckpoint(step, $"M55_C_START_RENDERING_RETURNED — returned={startReturned}; activeAfterStart={activeAfterStart}; nativeError='{SanitizeStartupLadderCheckpoint(GodotStep15NativeBridge.LastError)}'.");
            if (startReturned && activeAfterStart)
                await Task.Delay(TransformedRealStS2VeryEarlyInitialization.Step55SingleplayerRenderTargetMilliseconds);

            var elapsedBeforeStop = stopwatch.Elapsed.TotalMilliseconds;
            var stopReturned = GodotStep15NativeBridge.StopRendering();
            var activeAfterStop = GodotStep15NativeBridge.IsRenderingActive;
            stopwatch.Stop();
            var observedElapsed = stopwatch.Elapsed.TotalMilliseconds;
            WriteStartupLadderCheckpoint(step, $"M55_C_STOP_RENDERING_RETURNED — elapsedBeforeStopMs={elapsedBeforeStop:F1}; stopReturned={stopReturned}; activeAfterStop={activeAfterStop}; observedElapsedMs={observedElapsed:F1}; nativeError='{SanitizeStartupLadderCheckpoint(GodotStep15NativeBridge.LastError)}'.");
            if (!RecordStartupLadderGate(_step55Gates, _transformedRealStS2VeryEarlyInitialization.RunStep55SingleplayerRenderPulseEvidence(startReturned, activeAfterStart, stopReturned, activeAfterStop, observedElapsed, d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!RecordStartupLadderGate(_step55Gates, _transformedRealStS2VeryEarlyInitialization.RunStep55FrozenPostResidencyConfinement(!GodotStep15NativeBridge.IsRenderingActive, d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            CompleteStartupLadderStep(step, _step55Gates, resultLabel, detailLabel,
                "REAL SINGLE-PLAYER SUBMENU RENDER RESIDENCY CLOSED 4/4. NSingleplayerSubmenu rendered under its audited callback surface and synchronously refroze; Step 56 character-select frontier mapping is unlocked.");
            WriteStartupLadderCheckpoint(step, "RUN_STEP55_4OF4 — real single-player submenu render/refreeze closed; OpenCharacterSelect remains uninvoked and Step 56 may map it.");
        }
        catch (Exception ex)
        {
            if (GodotStep15NativeBridge.IsRenderingActive)
            {
                var stopped = GodotStep15NativeBridge.StopRendering();
                WriteStartupLadderCheckpoint(step, $"M55_EXCEPTION_REFREEZE — StopRendering returned={stopped}; activeAfterStop={GodotStep15NativeBridge.IsRenderingActive}; nativeError='{SanitizeStartupLadderCheckpoint(GodotStep15NativeBridge.LastError)}'.");
            }
            HandleStartupLadderException(step, resultLabel, detailLabel, ex, mutationArmed: _step55PulseUiStarted);
        }
        finally
        {
            if (GodotStep15NativeBridge.IsRenderingActive)
                GodotStep15NativeBridge.StopRendering();
            await FinishStartupLadderStepAsync(step, "Step55-TransformedRealStS2SingleplayerSubmenuRender.txt", "StS2 Launcher — Step 55.0 Singleplayer Submenu Render Residency", resultLabel, detailLabel,
                "Step 55 always leaves rendering frozen. If the render pulse was armed, never retry Step 55 in-process.");
        }
    }

    private async Task RunStep56StartupLadderAsync()
    {
        const int step = 56;
        if (!TryPrepareStartupLadderStep(step, _step55Gates.Snapshot().Passed && _transformedRealStS2VeryEarlyInitialization.ExactStep55ClosurePassed,
                "Step 56.0 requires Step 55.0 4/4 rendered/refrozen NSingleplayerSubmenu authority in this same process.", out var button, out var resultLabel, out var detailLabel))
            return;
        if (!TryInitializeStartupLadderTelemetry(step, "Character-select transition frontier", "Step56-CharacterSelectFrontier-StaticMap", out var error))
        {
            SetStartupLadderRefusal(step, resultLabel, detailLabel, "TELEMETRY FAIL / NOT RUN", error);
            return;
        }
        BeginSteamOperation(allowCancel: false);
        _step56Gates.Reset();
        try
        {
            WriteStartupLadderCheckpoint(step, "RUN_START — Step 56.0 exact NSingleplayerSubmenu.OpenCharacterSelect frontier/resource-literal map started. OpenCharacterSelect is not invoked.");
            if (!RecordStartupLadderGate(_step56Gates, _transformedRealStS2VeryEarlyInitialization.RunStep56ClosedStep55Authority(!GodotStep15NativeBridge.IsRenderingActive, d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!RecordStartupLadderGate(_step56Gates, _transformedRealStS2VeryEarlyInitialization.RunStep56CharacterSelectBinding(d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!RecordStartupLadderGate(_step56Gates, _transformedRealStS2VeryEarlyInitialization.RunStep56CharacterSelectFrontierMap(d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!WriteStartupLadderStaticMap(step, out var mapError)) throw new IOException("Step 56 character-select frontier map write failed: " + mapError);
            _transformedRealStS2VeryEarlyInitialization.MarkStep56StaticMapDurablyWritten();
            WriteStartupLadderCheckpoint(step, "M56_C_STATIC_MAP_WRITE_RETURNED — exact OpenCharacterSelect IL/frontier/resource-literal evidence durably written; invocation remains NO.");
            if (!RecordStartupLadderGate(_step56Gates, _transformedRealStS2VeryEarlyInitialization.RunStep56FrozenNoInvocationConfinement(!GodotStep15NativeBridge.IsRenderingActive, d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            CompleteStartupLadderStep(step, _step56Gates, resultLabel, detailLabel,
                "CHARACTER-SELECT FRONTIER MAP CLOSED 4/4. OpenCharacterSelect remains uninvoked; exact resource-string evidence is durable. Step 57 may inspect only the selected PCK resource.");
            button.Enabled = false;
            WriteStartupLadderCheckpoint(step, "RUN_STEP56_4OF4 — non-invoking character-select frontier/resource discovery closed; Step 57 read-only PCK preflight unlocked.");
        }
        catch (Exception ex)
        {
            HandleStartupLadderException(step, resultLabel, detailLabel, ex, mutationArmed: false);
        }
        finally
        {
            await FinishStartupLadderStepAsync(step, "Step56-TransformedRealStS2CharacterSelectFrontier.txt", "StS2 Launcher — Step 56.0 Character Select Frontier Map", resultLabel, detailLabel,
                "Step 56 never invokes OpenCharacterSelect or restarts rendering.");
        }
    }

    private async Task RunStep57StartupLadderAsync()
    {
        const int step = 57;
        if (!TryPrepareStartupLadderStep(step, _step56Gates.Snapshot().Passed && _transformedRealStS2VeryEarlyInitialization.ExactStep56ClosurePassed,
                "Step 57.0 requires Step 56.0 4/4 durable non-invoking character-select frontier/resource discovery authority in this same process.", out var button, out var resultLabel, out var detailLabel))
            return;
        if (!TryInitializeStartupLadderTelemetry(step, "Character-select exact PCK resource preflight", "Step57-CharacterSelectResource-StaticMap", out var error))
        {
            SetStartupLadderRefusal(step, resultLabel, detailLabel, "TELEMETRY FAIL / NOT RUN", error);
            return;
        }
        BeginSteamOperation(allowCancel: false);
        _step57Gates.Reset();
        try
        {
            WriteStartupLadderCheckpoint(step, "RUN_START — Step 57.0 read-only exact character-select PCK resource preflight started. OpenCharacterSelect/ResourceLoader/PackedScene/rendering remain unopened.");
            if (!RecordStartupLadderGate(_step57Gates, _transformedRealStS2VeryEarlyInitialization.RunStep57ClosedStep56Authority(!GodotStep15NativeBridge.IsRenderingActive, d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!RecordStartupLadderGate(_step57Gates, _transformedRealStS2VeryEarlyInitialization.RunStep57CharacterSelectPckExtraction(d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!RecordStartupLadderGate(_step57Gates, _transformedRealStS2VeryEarlyInitialization.RunStep57CharacterSelectResourceRiskMap(d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!WriteStartupLadderStaticMap(step, out var mapError)) throw new IOException("Step 57 character-select resource preflight map write failed: " + mapError);
            _transformedRealStS2VeryEarlyInitialization.MarkStep57StaticMapDurablyWritten();
            WriteStartupLadderCheckpoint(step, "M57_C_STATIC_MAP_WRITE_RETURNED — exact PCK character-select identity/risk evidence durably written; no ResourceLoader or handler invocation occurred.");
            if (!RecordStartupLadderGate(_step57Gates, _transformedRealStS2VeryEarlyInitialization.RunStep57FrozenReadOnlyConfinement(!GodotStep15NativeBridge.IsRenderingActive, d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            CompleteStartupLadderStep(step, _step57Gates, resultLabel, detailLabel,
                "CHARACTER-SELECT RESOURCE PREFLIGHT CLOSED 4/4. Exact trusted-PCK resource/native-risk evidence is durable; character select itself remains completely uninvoked/unloaded for the next dedicated iteration.");
            button.Enabled = false;
            WriteStartupLadderCheckpoint(step, "RUN_STEP57_4OF4 — read-only character-select resource preflight closed; preserve Step-56/57 maps for the next design. No Step 58 behavior exists in this candidate.");
        }
        catch (Exception ex)
        {
            HandleStartupLadderException(step, resultLabel, detailLabel, ex, mutationArmed: false);
        }
        finally
        {
            await FinishStartupLadderStepAsync(step, "Step57-TransformedRealStS2CharacterSelectResourcePreflight.txt", "StS2 Launcher — Step 57.0 Character Select Resource Preflight", resultLabel, detailLabel,
                "Step 57 is read-only: no OpenCharacterSelect, ResourceLoader, PackedScene instantiation, or rendering.");
        }
    }
}
