using Foundation;
using StS2Launcher.Core;
using StS2Launcher.iOS.Platform;
using System.Diagnostics;

namespace StS2Launcher.iOS;

public sealed partial class RootViewController
{
    private readonly TransformedRealStS2StartupLadderGateSequence _step58Gates = new(58, "CHARACTER SELECT FACTORY FRONTIER");
    private readonly TransformedRealStS2StartupLadderGateSequence _step59Gates = new(59, "CHARACTER SELECT FROZEN OFF-TREE ACQUISITION");
    private readonly TransformedRealStS2StartupLadderGateSequence _step60Gates = new(60, "CHARACTER SELECT INITIALIZE FRONTIER");
    private readonly TransformedRealStS2StartupLadderGateSequence _step61Gates = new(61, "CHARACTER SELECT OFF-TREE INITIALIZATION");
    private readonly TransformedRealStS2StartupLadderGateSequence _step62Gates = new(62, "CHARACTER SELECT PUSH FRONTIER");
    private readonly TransformedRealStS2StartupLadderGateSequence _step63Gates = new(63, "CHARACTER SELECT FROZEN SCENETREE ADMISSION");
    private readonly TransformedRealStS2StartupLadderGateSequence _step64Gates = new(64, "CHARACTER SELECT RENDER RESIDENCY");

    private UIButton? _step58Button;
    private UIButton? _step59Button;
    private UIButton? _step60Button;
    private UIButton? _step61Button;
    private UIButton? _step62Button;
    private UIButton? _step63Button;
    private UIButton? _step64Button;
    private UILabel? _step58ResultLabel;
    private UILabel? _step59ResultLabel;
    private UILabel? _step60ResultLabel;
    private UILabel? _step61ResultLabel;
    private UILabel? _step62ResultLabel;
    private UILabel? _step63ResultLabel;
    private UILabel? _step64ResultLabel;
    private UILabel? _step58DetailLabel;
    private UILabel? _step59DetailLabel;
    private UILabel? _step60DetailLabel;
    private UILabel? _step61DetailLabel;
    private UILabel? _step62DetailLabel;
    private UILabel? _step63DetailLabel;
    private UILabel? _step64DetailLabel;
    private bool _step59CreationUiStarted;
    private bool _step61InitializationUiStarted;
    private bool _step63AdmissionUiStarted;
    private bool _step64PulseUiStarted;

    private void AddCharacterSelectContinuationControls(UIStackView content)
    {
        content.AddArrangedSubview(Separator());
        content.AddArrangedSubview(Label(
            "Steps 58–64 — character-select acquisition, admission, and bounded render (stop on first failure)",
            UIFont.BoldSystemFontOfSize(18),
            UIColor.Label));
        content.AddArrangedSubview(Label(
            "Requires the physically proven Step 57 extension reconstructed in the same fresh process. The original OpenCharacterSelect handler is never called. The mapped handler body is decomposed into exact GetSubmenuType<NCharacterSelectScreen>(), InitializeSingleplayer(), and Push() boundaries, each audited before one-shot execution. Step 64 alone may restart rendering and must synchronously refreeze. No embark/run-start behavior is present.",
            UIFont.SystemFontOfSize(13),
            UIColor.SecondaryLabel));

        (_step58Button, _step58ResultLabel, _step58DetailLabel) = AddStartupLadderStepControls(
            content,
            "Step 58.0 — isolate exact GetSubmenuType<NCharacterSelectScreen>() factory (NO invocation)",
            "Run Step 58.0 A–D — MAP FACTORY",
            "CHARACTER SELECT FACTORY FRONTIER: LOCKED",
            "Requires Step 57.0 4/4. Re-derives the exact generic factory call from OpenCharacterSelect, binds the concrete NMainMenuSubmenuStack implementation, records whether _characterSelectSubmenu is null or an exact off-tree retained NCharacterSelectScreen, and proves _characterSelectScreenScene still points at the exact Step-57 resource. No factory invocation occurs.");
        _step58Button.TouchUpInside += async (_, _) => await RunStep58StartupLadderAsync();

        (_step59Button, _step59ResultLabel, _step59DetailLabel) = AddStartupLadderStepControls(
            content,
            "Step 59.0 — frozen character-select cache/factory acquisition + actual off-tree lifecycle audit",
            "Run Step 59.0 A–D — ACQUIRE OFF-TREE",
            "CHARACTER SELECT FROZEN OFF-TREE ACQUISITION: LOCKED",
            "Requires Step 58.0 4/4. If the real stack already retains an exact off-tree NCharacterSelectScreen, Step 59 adopts and audits that object without invoking the factory. Only when the cache is null may it invoke exact GetSubmenuType<NCharacterSelectScreen>() once while frozen. Success requires exact cache/stack identity, IsInsideTree=false, zero managed/native escape, and an admissible actual off-tree hierarchy/lifecycle map. InitializeSingleplayer and Push remain unopened.");
        _step59Button.TouchUpInside += async (_, _) => await RunStep59StartupLadderAsync();

        (_step60Button, _step60ResultLabel, _step60DetailLabel) = AddStartupLadderStepControls(
            content,
            "Step 60.0 — exact NCharacterSelectScreen.InitializeSingleplayer non-invoking frontier",
            "Run Step 60.0 A–D — MAP INITIALIZE",
            "CHARACTER SELECT INITIALIZE FRONTIER: LOCKED",
            "Requires Step 59.0 4/4. Binds exact zero-arg void InitializeSingleplayer(), records its IL, and maps its execution-qualified closure under the retained runtime guards without invocation.");
        _step60Button.TouchUpInside += async (_, _) => await RunStep60StartupLadderAsync();

        (_step61Button, _step61ResultLabel, _step61DetailLabel) = AddStartupLadderStepControls(
            content,
            "Step 61.0 — one-shot off-tree InitializeSingleplayer with rendering frozen",
            "Run Step 61.0 A–D — INITIALIZE ONCE",
            "CHARACTER SELECT OFF-TREE INITIALIZATION: LOCKED",
            "Requires Step 60.0 4/4. Invokes only exact InitializeSingleplayer once while the real character-select root remains off-tree. Success requires the root to remain off-tree, exact stack/cache identity to remain intact, and no resolver/native escape. Push remains unopened.");
        _step61Button.TouchUpInside += async (_, _) => await RunStep61StartupLadderAsync();

        (_step62Button, _step62ResultLabel, _step62DetailLabel) = AddStartupLadderStepControls(
            content,
            "Step 62.0 — exact NSubmenuStack.Push + initialized character-select lifecycle frontier (NO invocation)",
            "Run Step 62.0 A–D — MAP PUSH",
            "CHARACTER SELECT PUSH FRONTIER: LOCKED",
            "Requires Step 61.0 4/4. Re-derives the exact Push(NSubmenu) call from OpenCharacterSelect, binds its runtime token, and audits both Push and the actual initialized off-tree character-select _EnterTree/_Ready/_Notification surface. Push is not invoked.");
        _step62Button.TouchUpInside += async (_, _) => await RunStep62StartupLadderAsync();

        (_step63Button, _step63ResultLabel, _step63DetailLabel) = AddStartupLadderStepControls(
            content,
            "Step 63.0 — one-shot frozen Push admission of the real character-select screen",
            "Run Step 63.0 A–D — ADMIT ONCE",
            "CHARACTER SELECT FROZEN SCENETREE ADMISSION: LOCKED",
            "Requires Step 62.0 4/4. Invokes only the exact audited Push once while rendering stays stopped. Success requires the exact retained NCharacterSelectScreen to become a visible in-tree child of the retained submenu stack with zero resolver/native drift.");
        _step63Button.TouchUpInside += async (_, _) => await RunStep63StartupLadderAsync();

        (_step64Button, _step64ResultLabel, _step64DetailLabel) = AddStartupLadderStepControls(
            content,
            "Step 64.0 — actual character-select frame/input audit + bounded render/refreeze",
            "Run Step 64.0 A–D — CHARACTER SELECT RENDER",
            "CHARACTER SELECT RENDER RESIDENCY: LOCKED",
            $"Requires Step 63.0 4/4. Audits the actual visible NCharacterSelectScreen subtree frame/input callbacks, writes the map before rendering, then runs one {TransformedRealStS2VeryEarlyInitialization.Step64CharacterSelectRenderTargetMilliseconds} ms render residency and synchronously refreezes. No button press, character choice, embark, or run-start action is authorized.");
        _step64Button.TouchUpInside += async (_, _) => await RunStep64StartupLadderAsync();
    }

    private (UIButton Button, UILabel Result, UILabel Detail) GetCharacterSelectContinuationControls(int step)
        => step switch
        {
            58 => (_step58Button!, _step58ResultLabel!, _step58DetailLabel!),
            59 => (_step59Button!, _step59ResultLabel!, _step59DetailLabel!),
            60 => (_step60Button!, _step60ResultLabel!, _step60DetailLabel!),
            61 => (_step61Button!, _step61ResultLabel!, _step61DetailLabel!),
            62 => (_step62Button!, _step62ResultLabel!, _step62DetailLabel!),
            63 => (_step63Button!, _step63ResultLabel!, _step63DetailLabel!),
            64 => (_step64Button!, _step64ResultLabel!, _step64DetailLabel!),
            _ => throw new ArgumentOutOfRangeException(nameof(step)),
        };

    private async Task RunStep58StartupLadderAsync()
    {
        const int step = 58;
        if (!TryPrepareStartupLadderStep(step, _step57Gates.Snapshot().Passed && _transformedRealStS2VeryEarlyInitialization.ExactStep57ClosurePassed,
                "Step 58.0 requires Step 57.0 4/4 retained-PackedScene/PCK preflight authority in this same process.", out var button, out var resultLabel, out var detailLabel)) return;
        if (!TryInitializeStartupLadderTelemetry(step, "Character-select generic factory frontier", "Step58-CharacterSelectFactory-StaticMap", out var error)) { SetStartupLadderRefusal(step, resultLabel, detailLabel, "TELEMETRY FAIL / NOT RUN", error); return; }
        BeginSteamOperation(allowCancel: false);
        _step58Gates.Reset();
        try
        {
            WriteStartupLadderCheckpoint(step, "RUN_START — Step 58.0 exact GetSubmenuType<NCharacterSelectScreen>() non-invoking factory map started from closed Step-57 authority.");
            if (!RecordStartupLadderGate(_step58Gates, _transformedRealStS2VeryEarlyInitialization.RunStep58ClosedStep57Authority(!GodotStep15NativeBridge.IsRenderingActive, d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!RecordStartupLadderGate(_step58Gates, _transformedRealStS2VeryEarlyInitialization.RunStep58CharacterSelectFactoryBinding(d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!RecordStartupLadderGate(_step58Gates, _transformedRealStS2VeryEarlyInitialization.RunStep58CharacterSelectFactoryFrontierMap(d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!WriteStartupLadderStaticMap(step, out var mapError)) throw new IOException("Step 58 character-select factory map write failed: " + mapError);
            _transformedRealStS2VeryEarlyInitialization.MarkStep58StaticMapDurablyWritten();
            WriteStartupLadderCheckpoint(step, "M58_C_STATIC_MAP_WRITE_RETURNED — exact non-invoking character-select factory map durably written.");
            if (!RecordStartupLadderGate(_step58Gates, _transformedRealStS2VeryEarlyInitialization.RunStep58FrozenNoCreationConfinement(!GodotStep15NativeBridge.IsRenderingActive, d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            CompleteStartupLadderStep(step, _step58Gates, resultLabel, detailLabel, "CHARACTER-SELECT FACTORY FRONTIER CLOSED 4/4. Exact current cache state is confined; Step 59 may acquire the real off-tree screen without unnecessary re-creation.");
            button.Enabled = false;
            WriteStartupLadderCheckpoint(step, "RUN_STEP58_4OF4 — exact non-invoking character-select factory/cache authority closed; Step 59 off-tree acquisition unlocked.");
        }
        catch (Exception ex) { HandleStartupLadderException(step, resultLabel, detailLabel, ex, mutationArmed: false); }
        finally { await FinishStartupLadderStepAsync(step, "Step58-TransformedRealStS2CharacterSelectFactoryFrontier.txt", "StS2 Launcher — Step 58.0 Character Select Factory Frontier", resultLabel, detailLabel, "Step 58 never creates character select or restarts rendering."); }
    }

    private async Task RunStep59StartupLadderAsync()
    {
        const int step = 59;
        if (_step59CreationUiStarted || _transformedRealStS2VeryEarlyInitialization.Step59CreationStarted) { var labels = GetStartupLadderLabels(step); SetStartupLadderRefusal(step, labels.Result, labels.Detail, "ONE-SHOT ALREADY ARMED", "Step 59 character-select factory invocation was already armed in this process. Preserve reports and relaunch; never retry it in-process."); return; }
        if (!TryPrepareStartupLadderStep(step, _step58Gates.Snapshot().Passed && _transformedRealStS2VeryEarlyInitialization.ExactStep58ClosurePassed,
                "Step 59.0 requires Step 58.0 4/4 durable character-select factory authority in this same process.", out var button, out var resultLabel, out var detailLabel)) return;
        if (!TryInitializeStartupLadderTelemetry(step, "Frozen character-select off-tree acquisition", "Step59-CharacterSelectOffTree-StaticMap", out var error)) { SetStartupLadderRefusal(step, resultLabel, detailLabel, "TELEMETRY FAIL / NOT RUN", error); return; }
        BeginSteamOperation(allowCancel: false);
        _step59Gates.Reset();
        try
        {
            WriteStartupLadderCheckpoint(step, "RUN_START — Step 59.0 exact off-tree NCharacterSelectScreen acquisition + actual lifecycle audit started. Reuse an exact pre-existing off-tree cache when present; invoke GetSubmenuType only if the cache is null.");
            if (!RecordStartupLadderGate(_step59Gates, _transformedRealStS2VeryEarlyInitialization.RunStep59ClosedStep58Authority(!GodotStep15NativeBridge.IsRenderingActive, d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!RecordStartupLadderGate(_step59Gates, _transformedRealStS2VeryEarlyInitialization.RunStep59CharacterSelectCreationBinding(d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            _step59CreationUiStarted = _transformedRealStS2VeryEarlyInitialization.Step59FactoryInvocationRequired;
            button.Enabled = false;
            var gateC = _transformedRealStS2VeryEarlyInitialization.RunStep59FrozenCharacterSelectCreation(d => WriteStartupLadderCheckpoint(step, d));
            if (!RecordStartupLadderGate(_step59Gates, gateC, resultLabel, detailLabel)) return;
            if (!WriteStartupLadderStaticMap(step, out var mapError)) throw new IOException("Step 59 actual off-tree character-select map write failed: " + mapError);
            _transformedRealStS2VeryEarlyInitialization.MarkStep59StaticMapDurablyWritten();
            WriteStartupLadderCheckpoint(step, "M59_C_STATIC_MAP_WRITE_RETURNED — actual off-tree character-select hierarchy/lifecycle evidence durably written after exact cache/factory acquisition.");
            if (!RecordStartupLadderGate(_step59Gates, _transformedRealStS2VeryEarlyInitialization.RunStep59FrozenOffTreeConfinement(!GodotStep15NativeBridge.IsRenderingActive, d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            CompleteStartupLadderStep(step, _step59Gates, resultLabel, detailLabel, "REAL NCharacterSelectScreen OFF-TREE ACQUISITION CLOSED 4/4. InitializeSingleplayer remains uninvoked; Step 60 mapping is unlocked.");
            WriteStartupLadderCheckpoint(step, "RUN_STEP59_4OF4 — exact game cache/factory path acquired and retained character select off-tree; Step 60 InitializeSingleplayer map unlocked.");
        }
        catch (Exception ex) { HandleStartupLadderException(step, resultLabel, detailLabel, ex, mutationArmed: _step59CreationUiStarted || _transformedRealStS2VeryEarlyInitialization.Step59CreationStarted); }
        finally { await FinishStartupLadderStepAsync(step, "Step59-TransformedRealStS2CharacterSelectOffTree.txt", "StS2 Launcher — Step 59.0 Character Select Frozen Off-Tree Acquisition", resultLabel, detailLabel, "Step 59 leaves rendering frozen. If the factory path was armed, never retry Step 59 in-process; a pre-existing exact cache path does not invoke the factory."); }
    }

    private async Task RunStep60StartupLadderAsync()
    {
        const int step = 60;
        if (!TryPrepareStartupLadderStep(step, _step59Gates.Snapshot().Passed && _transformedRealStS2VeryEarlyInitialization.ExactStep59ClosurePassed,
                "Step 60.0 requires Step 59.0 4/4 exact acquired off-tree NCharacterSelectScreen authority in this same process.", out var button, out var resultLabel, out var detailLabel)) return;
        if (!TryInitializeStartupLadderTelemetry(step, "Character-select InitializeSingleplayer frontier", "Step60-CharacterSelectInitialize-StaticMap", out var error)) { SetStartupLadderRefusal(step, resultLabel, detailLabel, "TELEMETRY FAIL / NOT RUN", error); return; }
        BeginSteamOperation(allowCancel: false);
        _step60Gates.Reset();
        try
        {
            WriteStartupLadderCheckpoint(step, "RUN_START — Step 60.0 exact NCharacterSelectScreen.InitializeSingleplayer non-invoking frontier map started.");
            if (!RecordStartupLadderGate(_step60Gates, _transformedRealStS2VeryEarlyInitialization.RunStep60ClosedStep59Authority(!GodotStep15NativeBridge.IsRenderingActive, d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!RecordStartupLadderGate(_step60Gates, _transformedRealStS2VeryEarlyInitialization.RunStep60InitializeSingleplayerBinding(d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!RecordStartupLadderGate(_step60Gates, _transformedRealStS2VeryEarlyInitialization.RunStep60InitializeSingleplayerFrontierMap(d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!WriteStartupLadderStaticMap(step, out var mapError)) throw new IOException("Step 60 InitializeSingleplayer frontier map write failed: " + mapError);
            _transformedRealStS2VeryEarlyInitialization.MarkStep60StaticMapDurablyWritten();
            WriteStartupLadderCheckpoint(step, "M60_C_STATIC_MAP_WRITE_RETURNED — exact InitializeSingleplayer execution frontier durably written; invocation remains NO.");
            if (!RecordStartupLadderGate(_step60Gates, _transformedRealStS2VeryEarlyInitialization.RunStep60FrozenNoInitializationConfinement(!GodotStep15NativeBridge.IsRenderingActive, d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            CompleteStartupLadderStep(step, _step60Gates, resultLabel, detailLabel, "INITIALIZESINGLEPLAYER FRONTIER CLOSED 4/4. Real character-select root remains off-tree; Step 61 one-shot initialization is unlocked.");
            button.Enabled = false;
            WriteStartupLadderCheckpoint(step, "RUN_STEP60_4OF4 — exact InitializeSingleplayer frontier closed non-invoking; Step 61 one-shot off-tree initialization unlocked.");
        }
        catch (Exception ex) { HandleStartupLadderException(step, resultLabel, detailLabel, ex, mutationArmed: false); }
        finally { await FinishStartupLadderStepAsync(step, "Step60-TransformedRealStS2CharacterSelectInitializeFrontier.txt", "StS2 Launcher — Step 60.0 Character Select Initialize Frontier", resultLabel, detailLabel, "Step 60 never invokes InitializeSingleplayer or restarts rendering."); }
    }

    private async Task RunStep61StartupLadderAsync()
    {
        const int step = 61;
        if (_step61InitializationUiStarted || _transformedRealStS2VeryEarlyInitialization.Step61InitializationStarted) { var labels = GetStartupLadderLabels(step); SetStartupLadderRefusal(step, labels.Result, labels.Detail, "ONE-SHOT ALREADY ARMED", "Step 61 InitializeSingleplayer was already armed in this process. Preserve reports and relaunch; never retry it in-process."); return; }
        if (!TryPrepareStartupLadderStep(step, _step60Gates.Snapshot().Passed && _transformedRealStS2VeryEarlyInitialization.ExactStep60ClosurePassed,
                "Step 61.0 requires Step 60.0 4/4 exact InitializeSingleplayer frontier authority in this same process.", out var button, out var resultLabel, out var detailLabel)) return;
        if (!TryInitializeStartupLadderTelemetry(step, "Off-tree character-select InitializeSingleplayer", "Step61-CharacterSelectInitializedOffTree-StaticMap", out var error)) { SetStartupLadderRefusal(step, resultLabel, detailLabel, "TELEMETRY FAIL / NOT RUN", error); return; }
        BeginSteamOperation(allowCancel: false);
        _step61Gates.Reset();
        try
        {
            WriteStartupLadderCheckpoint(step, "RUN_START — Step 61.0 one-shot NCharacterSelectScreen.InitializeSingleplayer execution started while the real character-select root remains off-tree and rendering frozen.");
            if (!RecordStartupLadderGate(_step61Gates, _transformedRealStS2VeryEarlyInitialization.RunStep61ClosedStep60Authority(!GodotStep15NativeBridge.IsRenderingActive, d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!RecordStartupLadderGate(_step61Gates, _transformedRealStS2VeryEarlyInitialization.RunStep61InitializationRuntimeBinding(d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            _step61InitializationUiStarted = true;
            button.Enabled = false;
            var gateC = _transformedRealStS2VeryEarlyInitialization.RunStep61OffTreeInitialization(d => WriteStartupLadderCheckpoint(step, d));
            if (!RecordStartupLadderGate(_step61Gates, gateC, resultLabel, detailLabel)) return;
            if (!WriteStartupLadderStaticMap(step, out var mapError)) throw new IOException("Step 61 off-tree initialization observation map write failed: " + mapError);
            _transformedRealStS2VeryEarlyInitialization.MarkStep61StaticMapDurablyWritten();
            WriteStartupLadderCheckpoint(step, "M61_C_STATIC_MAP_WRITE_RETURNED — off-tree InitializeSingleplayer observation durably written; Push remains NO.");
            if (!RecordStartupLadderGate(_step61Gates, _transformedRealStS2VeryEarlyInitialization.RunStep61FrozenPostInitializationConfinement(!GodotStep15NativeBridge.IsRenderingActive, d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            CompleteStartupLadderStep(step, _step61Gates, resultLabel, detailLabel, "OFF-TREE INITIALIZESINGLEPLAYER CLOSED 4/4. The initialized real screen remains off-tree/refrozen; Step 62 Push/lifecycle mapping is unlocked.");
            WriteStartupLadderCheckpoint(step, "RUN_STEP61_4OF4 — exact InitializeSingleplayer executed once off-tree; Step 62 Push/lifecycle map unlocked.");
        }
        catch (Exception ex) { HandleStartupLadderException(step, resultLabel, detailLabel, ex, mutationArmed: _step61InitializationUiStarted || _transformedRealStS2VeryEarlyInitialization.Step61InitializationStarted); }
        finally { await FinishStartupLadderStepAsync(step, "Step61-TransformedRealStS2CharacterSelectInitializeOffTree.txt", "StS2 Launcher — Step 61.0 Character Select Off-Tree Initialization", resultLabel, detailLabel, "Step 61 leaves rendering frozen and does not Push. If initialization was armed, never retry Step 61 in-process."); }
    }

    private async Task RunStep62StartupLadderAsync()
    {
        const int step = 62;
        if (!TryPrepareStartupLadderStep(step, _step61Gates.Snapshot().Passed && _transformedRealStS2VeryEarlyInitialization.ExactStep61ClosurePassed,
                "Step 62.0 requires Step 61.0 4/4 initialized off-tree NCharacterSelectScreen authority in this same process.", out var button, out var resultLabel, out var detailLabel)) return;
        if (!TryInitializeStartupLadderTelemetry(step, "Character-select Push/lifecycle frontier", "Step62-CharacterSelectPush-StaticMap", out var error)) { SetStartupLadderRefusal(step, resultLabel, detailLabel, "TELEMETRY FAIL / NOT RUN", error); return; }
        BeginSteamOperation(allowCancel: false);
        _step62Gates.Reset();
        try
        {
            WriteStartupLadderCheckpoint(step, "RUN_START — Step 62.0 exact NSubmenuStack.Push + initialized character-select lifecycle map started; Push is not invoked.");
            if (!RecordStartupLadderGate(_step62Gates, _transformedRealStS2VeryEarlyInitialization.RunStep62ClosedStep61Authority(!GodotStep15NativeBridge.IsRenderingActive, d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!RecordStartupLadderGate(_step62Gates, _transformedRealStS2VeryEarlyInitialization.RunStep62PushBinding(d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!RecordStartupLadderGate(_step62Gates, _transformedRealStS2VeryEarlyInitialization.RunStep62PushAndLifecycleFrontierMap(d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!WriteStartupLadderStaticMap(step, out var mapError)) throw new IOException("Step 62 Push/lifecycle map write failed: " + mapError);
            _transformedRealStS2VeryEarlyInitialization.MarkStep62StaticMapDurablyWritten();
            WriteStartupLadderCheckpoint(step, "M62_C_STATIC_MAP_WRITE_RETURNED — exact Push and initialized character-select lifecycle frontiers durably written; Push remains NO.");
            if (!RecordStartupLadderGate(_step62Gates, _transformedRealStS2VeryEarlyInitialization.RunStep62FrozenNoAdmissionConfinement(!GodotStep15NativeBridge.IsRenderingActive, d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            CompleteStartupLadderStep(step, _step62Gates, resultLabel, detailLabel, "PUSH/CHARACTER-SELECT LIFECYCLE FRONTIER CLOSED 4/4. Screen remains off-tree; Step 63 one-shot frozen Push admission is unlocked.");
            button.Enabled = false;
            WriteStartupLadderCheckpoint(step, "RUN_STEP62_4OF4 — exact Push + actual initialized lifecycle frontiers closed non-invoking; Step 63 frozen admission unlocked.");
        }
        catch (Exception ex) { HandleStartupLadderException(step, resultLabel, detailLabel, ex, mutationArmed: false); }
        finally { await FinishStartupLadderStepAsync(step, "Step62-TransformedRealStS2CharacterSelectPushFrontier.txt", "StS2 Launcher — Step 62.0 Character Select Push Frontier", resultLabel, detailLabel, "Step 62 never invokes Push or restarts rendering."); }
    }

    private async Task RunStep63StartupLadderAsync()
    {
        const int step = 63;
        if (_step63AdmissionUiStarted || _transformedRealStS2VeryEarlyInitialization.Step63AdmissionStarted) { var labels = GetStartupLadderLabels(step); SetStartupLadderRefusal(step, labels.Result, labels.Detail, "ONE-SHOT ALREADY ARMED", "Step 63 Push admission was already armed in this process. Preserve reports and relaunch; never retry it in-process."); return; }
        if (!TryPrepareStartupLadderStep(step, _step62Gates.Snapshot().Passed && _transformedRealStS2VeryEarlyInitialization.ExactStep62ClosurePassed,
                "Step 63.0 requires Step 62.0 4/4 durable Push/lifecycle frontier authority in this same process.", out var button, out var resultLabel, out var detailLabel)) return;
        if (!TryInitializeStartupLadderTelemetry(step, "Frozen character-select SceneTree admission", "Step63-CharacterSelectAdmission-StaticMap", out var error)) { SetStartupLadderRefusal(step, resultLabel, detailLabel, "TELEMETRY FAIL / NOT RUN", error); return; }
        BeginSteamOperation(allowCancel: false);
        _step63Gates.Reset();
        try
        {
            WriteStartupLadderCheckpoint(step, "RUN_START — Step 63.0 one-shot frozen NSubmenuStack.Push(characterSelect) admission started from Step-62 audited authority.");
            if (!RecordStartupLadderGate(_step63Gates, _transformedRealStS2VeryEarlyInitialization.RunStep63ClosedStep62Authority(!GodotStep15NativeBridge.IsRenderingActive, d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!RecordStartupLadderGate(_step63Gates, _transformedRealStS2VeryEarlyInitialization.RunStep63FrozenAdmissionBinding(d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            _step63AdmissionUiStarted = true;
            button.Enabled = false;
            var gateC = _transformedRealStS2VeryEarlyInitialization.RunStep63FrozenCharacterSelectAdmission(d => WriteStartupLadderCheckpoint(step, d));
            if (!RecordStartupLadderGate(_step63Gates, gateC, resultLabel, detailLabel)) return;
            if (!WriteStartupLadderStaticMap(step, out var mapError)) throw new IOException("Step 63 frozen character-select admission map write failed: " + mapError);
            _transformedRealStS2VeryEarlyInitialization.MarkStep63StaticMapDurablyWritten();
            WriteStartupLadderCheckpoint(step, "M63_C_STATIC_MAP_WRITE_RETURNED — exact frozen Push admission observations durably written; rendering remains stopped.");
            if (!RecordStartupLadderGate(_step63Gates, _transformedRealStS2VeryEarlyInitialization.RunStep63FrozenPostAdmissionConfinement(!GodotStep15NativeBridge.IsRenderingActive, d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            CompleteStartupLadderStep(step, _step63Gates, resultLabel, detailLabel, "REAL CHARACTER-SELECT FROZEN ADMISSION CLOSED 4/4. NCharacterSelectScreen is visible/in-tree with renderer stopped; Step 64 frame/input render audit is unlocked.");
            WriteStartupLadderCheckpoint(step, "RUN_STEP63_4OF4 — exact initialized character-select screen admitted/visible while frozen; Step 64 bounded render residency unlocked.");
        }
        catch (Exception ex) { HandleStartupLadderException(step, resultLabel, detailLabel, ex, mutationArmed: _step63AdmissionUiStarted || _transformedRealStS2VeryEarlyInitialization.Step63AdmissionStarted); }
        finally { await FinishStartupLadderStepAsync(step, "Step63-TransformedRealStS2CharacterSelectFrozenAdmission.txt", "StS2 Launcher — Step 63.0 Character Select Frozen SceneTree Admission", resultLabel, detailLabel, "Step 63 leaves rendering frozen. If Push was armed, never retry Step 63 in-process."); }
    }

    private async Task RunStep64StartupLadderAsync()
    {
        const int step = 64;
        if (_step64PulseUiStarted || _transformedRealStS2VeryEarlyInitialization.Step64PulseStarted) { var labels = GetStartupLadderLabels(step); SetStartupLadderRefusal(step, labels.Result, labels.Detail, "ONE-SHOT ALREADY ARMED", "Step 64 render residency was already armed in this process. Preserve reports and relaunch; never retry StartRendering."); return; }
        if (!TryPrepareStartupLadderStep(step, _step63Gates.Snapshot().Passed && _transformedRealStS2VeryEarlyInitialization.ExactStep63ClosurePassed,
                "Step 64.0 requires Step 63.0 4/4 visible/in-tree frozen NCharacterSelectScreen authority in this same process.", out var button, out var resultLabel, out var detailLabel)) return;
        if (!TryInitializeStartupLadderTelemetry(step, "Character-select render residency", "Step64-CharacterSelectFrameInput-StaticMap", out var error)) { SetStartupLadderRefusal(step, resultLabel, detailLabel, "TELEMETRY FAIL / NOT RUN", error); return; }
        BeginSteamOperation(allowCancel: false);
        _step64Gates.Reset();
        try
        {
            WriteStartupLadderCheckpoint(step, "RUN_START — Step 64.0 actual in-tree NCharacterSelectScreen frame/input audit + bounded render/refreeze residency started.");
            if (!RecordStartupLadderGate(_step64Gates, _transformedRealStS2VeryEarlyInitialization.RunStep64ClosedStep63Authority(!GodotStep15NativeBridge.IsRenderingActive, d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!RecordStartupLadderGate(_step64Gates, _transformedRealStS2VeryEarlyInitialization.RunStep64CharacterSelectFrameInputAudit(d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!WriteStartupLadderStaticMap(step, out var mapError)) throw new IOException("Step 64 character-select frame/input map write failed before StartRendering: " + mapError);
            _transformedRealStS2VeryEarlyInitialization.MarkStep64StaticMapDurablyWritten();
            WriteStartupLadderCheckpoint(step, "M64_B_STATIC_MAP_WRITE_RETURNED — actual character-select frame/input map durably written before StartRendering.");
            if (!NSThread.IsMain) throw new InvalidOperationException("Step 64.0 render pulse must be armed on UIKit main thread.");
            _transformedRealStS2VeryEarlyInitialization.BeginStep64BoundedRenderPulse(d => WriteStartupLadderCheckpoint(step, d));
            _step64PulseUiStarted = true;
            button.Enabled = false;
            WriteStartupLadderCheckpoint(step, $"M64_C_START_RENDERING_CALL — invoking StartRendering exactly once for character-select residency; managedThread={Environment.CurrentManagedThreadId}; isMain={NSThread.IsMain}; activeBefore={GodotStep15NativeBridge.IsRenderingActive}.");
            var stopwatch = Stopwatch.StartNew();
            var startReturned = GodotStep15NativeBridge.StartRendering();
            var activeAfterStart = GodotStep15NativeBridge.IsRenderingActive;
            WriteStartupLadderCheckpoint(step, $"M64_C_START_RENDERING_RETURNED — returned={startReturned}; activeAfterStart={activeAfterStart}; nativeError='{SanitizeStartupLadderCheckpoint(GodotStep15NativeBridge.LastError)}'.");
            if (startReturned && activeAfterStart) await Task.Delay(TransformedRealStS2VeryEarlyInitialization.Step64CharacterSelectRenderTargetMilliseconds);
            var elapsedBeforeStop = stopwatch.Elapsed.TotalMilliseconds;
            var stopReturned = GodotStep15NativeBridge.StopRendering();
            var activeAfterStop = GodotStep15NativeBridge.IsRenderingActive;
            stopwatch.Stop();
            var observedElapsed = stopwatch.Elapsed.TotalMilliseconds;
            WriteStartupLadderCheckpoint(step, $"M64_C_STOP_RENDERING_RETURNED — elapsedBeforeStopMs={elapsedBeforeStop:F1}; stopReturned={stopReturned}; activeAfterStop={activeAfterStop}; observedElapsedMs={observedElapsed:F1}; nativeError='{SanitizeStartupLadderCheckpoint(GodotStep15NativeBridge.LastError)}'.");
            if (!RecordStartupLadderGate(_step64Gates, _transformedRealStS2VeryEarlyInitialization.RunStep64CharacterSelectRenderPulseEvidence(startReturned, activeAfterStart, stopReturned, activeAfterStop, observedElapsed, d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!RecordStartupLadderGate(_step64Gates, _transformedRealStS2VeryEarlyInitialization.RunStep64FrozenPostResidencyConfinement(!GodotStep15NativeBridge.IsRenderingActive, d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            CompleteStartupLadderStep(step, _step64Gates, resultLabel, detailLabel, "REAL CHARACTER-SELECT RENDER RESIDENCY CLOSED 4/4. NCharacterSelectScreen rendered under its audited callback surface and synchronously refroze. No character choice/embark/run-start action is opened.");
            WriteStartupLadderCheckpoint(step, "RUN_STEP64_4OF4 — real character-select render/refreeze closed; stop here. No Step 65 behavior exists in this candidate.");
        }
        catch (Exception ex)
        {
            if (GodotStep15NativeBridge.IsRenderingActive)
            {
                var stopped = GodotStep15NativeBridge.StopRendering();
                WriteStartupLadderCheckpoint(step, $"M64_EXCEPTION_REFREEZE — StopRendering returned={stopped}; activeAfterStop={GodotStep15NativeBridge.IsRenderingActive}; nativeError='{SanitizeStartupLadderCheckpoint(GodotStep15NativeBridge.LastError)}'.");
            }
            HandleStartupLadderException(step, resultLabel, detailLabel, ex, mutationArmed: _step64PulseUiStarted || _transformedRealStS2VeryEarlyInitialization.Step64PulseStarted);
        }
        finally
        {
            if (GodotStep15NativeBridge.IsRenderingActive) GodotStep15NativeBridge.StopRendering();
            await FinishStartupLadderStepAsync(step, "Step64-TransformedRealStS2CharacterSelectRender.txt", "StS2 Launcher — Step 64.0 Character Select Render Residency", resultLabel, detailLabel, "Step 64 always leaves rendering frozen. If the pulse was armed, never retry Step 64 in-process.");
        }
    }
}
