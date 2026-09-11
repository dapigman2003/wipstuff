using Foundation;
using StS2Launcher.Core;
using StS2Launcher.iOS.Platform;
using System.Text;
using System.Diagnostics;

namespace StS2Launcher.iOS;

public sealed partial class RootViewController
{
    private readonly TransformedRealStS2StartupLadderGateSequence _step43Gates = new(43, "NULL PLATFORM AUTHORITY");
    private readonly TransformedRealStS2StartupLadderGateSequence _step44Gates = new(44, "LEGACY MIGRATION GUARD");
    private readonly TransformedRealStS2StartupLadderGateSequence _step45Gates = new(45, "LOCAL SAVE INITIALIZATION");
    private readonly TransformedRealStS2StartupLadderGateSequence _step46Gates = new(46, "LAUNCHMAINMENU IMMEDIATE FRONTIER MAP");
    private readonly TransformedRealStS2StartupLadderGateSequence _step47Gates = new(47, "MAIN MENU RESOURCE PREPARATION");
    private readonly TransformedRealStS2StartupLadderGateSequence _step48Gates = new(48, "MAIN MENU OFF-TREE INSTANTIATION");
    private readonly TransformedRealStS2StartupLadderGateSequence _step49Gates = new(49, "MAIN MENU FROZEN SCENETREE ADMISSION");
    private readonly TransformedRealStS2StartupLadderGateSequence _step50Gates = new(50, "MAIN MENU CONTROLLED RENDER PULSE");
    private readonly TransformedRealStS2StartupLadderGateSequence _step51Gates = new(51, "SINGLEPLAYER FRONTIER MAP");
    private readonly TransformedRealStS2StartupLadderGateSequence _step52Gates = new(52, "MAIN MENU SUSTAINED RENDER RESIDENCY");

    private readonly Dictionary<int, StartupLadderTelemetryState> _startupLadderTelemetry = [];
    private readonly object _startupLadderCheckpointSync = new();

    private UIButton? _step43Button;
    private UIButton? _step44Button;
    private UIButton? _step45Button;
    private UIButton? _step46Button;
    private UIButton? _step47Button;
    private UIButton? _step48Button;
    private UIButton? _step49Button;
    private UIButton? _step50Button;
    private UIButton? _step51Button;
    private UIButton? _step52Button;
    private UILabel? _step43ResultLabel;
    private UILabel? _step44ResultLabel;
    private UILabel? _step45ResultLabel;
    private UILabel? _step46ResultLabel;
    private UILabel? _step47ResultLabel;
    private UILabel? _step48ResultLabel;
    private UILabel? _step49ResultLabel;
    private UILabel? _step50ResultLabel;
    private UILabel? _step51ResultLabel;
    private UILabel? _step52ResultLabel;
    private UILabel? _step43DetailLabel;
    private UILabel? _step44DetailLabel;
    private UILabel? _step45DetailLabel;
    private UILabel? _step46DetailLabel;
    private UILabel? _step47DetailLabel;
    private UILabel? _step48DetailLabel;
    private UILabel? _step49DetailLabel;
    private UILabel? _step50DetailLabel;
    private UILabel? _step51DetailLabel;
    private UILabel? _step52DetailLabel;
    private bool _step45InvocationUiStarted;
    private bool _step47ResourceLoadUiStarted;
    private bool _step48InstantiationUiStarted;
    private bool _step49AdmissionUiStarted;
    private bool _step50PulseUiStarted;
    private bool _step52PulseUiStarted;

    private void AddTransformedRealStS2StartupLadderControls(UIStackView content)
    {
        content.AddArrangedSubview(Separator());
        content.AddArrangedSubview(Label(
            "Steps 43–57 — sequential startup ladder (stop on first failure)",
            UIFont.BoldSystemFontOfSize(18),
            UIColor.Label));
        content.AddArrangedSubview(Label(
            "Each rung is independently gated and reportable. Later rungs remain locked until the prior rung is 4/4 in the same process. Physical authority through Step 52 remains the closed baseline. Steps 53–57 continue only from that exact same-process authority: isolate/open/render the real single-player submenu, then map and read-only-preflight character select without invoking it. Original GameStartup/LaunchMainMenu, Steam startup, deferred startup and native game extensions remain unopened. Only Steps 50, 52 and 55 may restart rendering, and each synchronously refreezes before evaluation.",
            UIFont.SystemFontOfSize(13),
            UIColor.SecondaryLabel));

        (_step43Button, _step43ResultLabel, _step43DetailLabel) = AddStartupLadderStepControls(
            content,
            "Step 43.0 — prove concrete Null platform strategy + read-only platform authority",
            "Run Step 43.0 A–D",
            "NULL PLATFORM AUTHORITY: NOT RUN",
            "Requires Step 42.0 4/4. Maps PlatformUtil + concrete NullPlatformUtilStrategy, durably writes the map, then probes only read-only platform identity/language/window getters and proves frozen zero-native confinement.");
        _step43Button.TouchUpInside += async (_, _) => await RunStep43StartupLadderAsync();

        (_step44Button, _step44ResultLabel, _step44DetailLabel) = AddStartupLadderStepControls(
            content,
            "Step 44.0 — read-only legacy migration guard",
            "Run Step 44.0 A–D",
            "LEGACY MIGRATION GUARD: LOCKED",
            "Requires Step 43.0 4/4. Audits and invokes only the two HasLegacyData() probes. If either returns true, the rung fails before any migration/archive mutation and the ladder stops for a dedicated backup/migration iteration.");
        _step44Button.TouchUpInside += async (_, _) => await RunStep44StartupLadderAsync();

        (_step45Button, _step45ResultLabel, _step45DetailLabel) = AddStartupLadderStepControls(
            content,
            "Step 45.0 — local SaveManager profile/progress/prefs initialization",
            "Run Step 45.0 A–D",
            "LOCAL SAVE INITIALIZATION: LOCKED",
            "Requires Step 44.0 4/4/no legacy data. Maps InitProfileId(null), InitProgressData(), and InitPrefsData(), writes the map, then invokes that exact local sequence once. Cloud sync, migrations, Steam, and GameStartup remain uninvoked.");
        _step45Button.TouchUpInside += async (_, _) => await RunStep45StartupLadderAsync();

        (_step46Button, _step46ResultLabel, _step46DetailLabel) = AddStartupLadderStepControls(
            content,
            "Step 46.0 — execution-aware original LaunchMainMenu frontier map (NO invocation)",
            "Run Step 46.0 A–D",
            "LAUNCHMAINMENU IMMEDIATE FRONTIER MAP: LOCKED",
            "Requires Step 45.0 4/4. Resolves LaunchMainMenu(bool)'s compiler state machine, then traverses only executing IL method edges (call/callvirt/newobj/jmp). ldftn/ldvirtftn/ldtoken targets are recorded as deferred frontiers instead of recursively treated as immediate execution. The map may contain unsafe immediate boundaries; it is evidence only and never authorizes original LaunchMainMenu.");
        _step46Button.TouchUpInside += async (_, _) => await RunStep46StartupLadderAsync();

        (_step47Button, _step47ResultLabel, _step47DetailLabel) = AddStartupLadderStepControls(
            content,
            "Step 47.0 — exact main-menu resource prep + Spine-neutral cache authority",
            "Run Step 47.0 A–D",
            "MAIN MENU RESOURCE PREPARATION: LOCKED",
            "Requires Step 46.0 4/4 map authority. Extracts exact main_menu.tscn + main_menu_bg.tscn from the receipt-backed PCK, prepares a private deterministic Spine-neutral background derivative, writes the static map before loading, then loads the derivative and original menu as PackedScenes without instantiation. Trusted PCK/install bytes stay immutable; original LaunchMainMenu/ExecuteDeferred/Steam remain unopened.");
        _step47Button.TouchUpInside += async (_, _) => await RunStep47StartupLadderAsync();

        (_step48Button, _step48ResultLabel, _step48DetailLabel) = AddStartupLadderStepControls(
            content,
            "Step 48.1 — one-shot off-tree NMainMenu + runtime-guarded lifecycle audit",
            "Run Step 48.1 A–D",
            "MAIN MENU OFF-TREE INSTANTIATION: LOCKED",
            "Requires Step 47.0 4/4. Instantiates exact NMainMenu off-tree, then rehearses the three physical runtime guards before lifecycle admission: empty Godot command line + zero-drift CheckCommandLineArgs, existing production SaveManager + zero-drift get_Instance, and exact Null-platform + zero-drift SetRichPresence. Only those rehearsed calls may terminate static lifecycle traversal; every other immediate Steam/Spine/native boundary still fails before AddChild.");
        _step48Button.TouchUpInside += async (_, _) => await RunStep48StartupLadderAsync();

        (_step49Button, _step49ResultLabel, _step49DetailLabel) = AddStartupLadderStepControls(
            content,
            "Step 49.0 — one frozen RootSceneContainer.AddChild(NMainMenu)",
            "Run Step 49.0 A–D",
            "MAIN MENU FROZEN SCENETREE ADMISSION: LOCKED",
            "Requires Step 48.0 4/4 and its durable actual-node lifecycle map. Resolves exact retained NGame.RootSceneContainer, writes an admission map, then AddChild's the real NMainMenu exactly once while rendering stays stopped. Success requires IsInsideTree=true, exact parent, child count +1, state=2, and zero initializer/rejected/native escape.");
        _step49Button.TouchUpInside += async (_, _) => await RunStep49StartupLadderAsync();

        (_step50Button, _step50ResultLabel, _step50DetailLabel) = AddStartupLadderStepControls(
            content,
            "Step 50.0 — actual in-tree main-menu frame/input audit + ~100 ms render pulse",
            "Run Step 50.0 A–D — RENDER PULSE",
            "MAIN MENU CONTROLLED RENDER PULSE: LOCKED",
            "Requires Step 49.0 4/4. Maps the actual in-tree menu graph's _Process/_PhysicsProcess/_Draw/input callbacks with execution-qualified traversal, writes that map before rendering, starts the existing Godot render loop exactly once, requests ~100 ms, and the first managed continuation synchronously StopRendering before telemetry. Final state remains frozen even on a successful 4/4 pulse.");
        _step50Button.TouchUpInside += async (_, _) => await RunStep50StartupLadderAsync();

        (_step51Button, _step51ResultLabel, _step51DetailLabel) = AddStartupLadderStepControls(
            content,
            "Step 51.0 — non-invoking single-player button frontier map",
            "Run Step 51.0 A–D — MAP ONLY",
            "SINGLEPLAYER FRONTIER MAP: LOCKED",
            "Requires Step 50.0 4/4/refrozen. Locates exact NMainMenu.SingleplayerButtonPressed and OpenSingleplayerSubmenu, records their direct IL, then builds an execution-qualified immediate/deferred frontier under the retained Step-48 runtime guards. Classified boundaries are evidence only; no handler is invoked and rendering stays stopped.");
        _step51Button.TouchUpInside += async (_, _) => await RunStep51StartupLadderAsync();

        (_step52Button, _step52ResultLabel, _step52DetailLabel) = AddStartupLadderStepControls(
            content,
            "Step 52.0 — sustained real-main-menu render residency (~1.5 s) + refreeze",
            "Run Step 52.0 A–D — SUSTAINED RENDER",
            "MAIN MENU SUSTAINED RENDER RESIDENCY: LOCKED",
            "Requires Step 51.0 4/4. Re-audits the actual in-tree frame/input surface under retained runtime guards, writes a fresh pre-render map, then runs one ~1.5 s render residency pulse and synchronously StopRendering before telemetry. Final state must remain frozen with no initializer/rejected/native escape.");
        _step52Button.TouchUpInside += async (_, _) => await RunStep52StartupLadderAsync();
        AddSingleplayerContinuationControls(content);
    }

    private (UIButton Button, UILabel Result, UILabel Detail) AddStartupLadderStepControls(
        UIStackView content,
        string title,
        string buttonTitle,
        string resultText,
        string detailText)
    {
        content.AddArrangedSubview(Label(title, UIFont.BoldSystemFontOfSize(16), UIColor.Label));
        var button = SystemButton(buttonTitle, 15);
        content.AddArrangedSubview(button);
        var result = Label(resultText, UIFont.BoldSystemFontOfSize(14), UIColor.SecondaryLabel);
        content.AddArrangedSubview(result);
        var detail = Label(detailText, UIFont.SystemFontOfSize(12), UIColor.SecondaryLabel);
        content.AddArrangedSubview(detail);
        return (button, result, detail);
    }

    private async Task RunStep43StartupLadderAsync()
    {
        const int step = 43;
        if (!TryPrepareStartupLadderStep(step, _step42Gates.Snapshot().Passed && _transformedRealStS2VeryEarlyInitialization.ExactStep42ClosurePassed,
                "Step 43.0 requires Step 42.0 4/4 in this same process.", out var button, out var resultLabel, out var detailLabel))
            return;
        if (!TryInitializeStartupLadderTelemetry(step, "Null platform authority", "Step43-Platform-StaticMap", out var error))
        {
            SetStartupLadderRefusal(step, resultLabel, detailLabel, "TELEMETRY FAIL / NOT RUN", error);
            return;
        }
        BeginSteamOperation(allowCancel: false);
        _step43Gates.Reset();
        try
        {
            WriteStartupLadderCheckpoint(step, "RUN_START — Step 43.0 Null platform authority started; rendering is frozen and no later ladder rung is authorized yet.");
            if (!RecordStartupLadderGate(_step43Gates, _transformedRealStS2VeryEarlyInitialization.RunStep43ClosedStep42Authority(!GodotStep15NativeBridge.IsRenderingActive, d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!RecordStartupLadderGate(_step43Gates, _transformedRealStS2VeryEarlyInitialization.RunStep43NullPlatformStaticAudit(d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!WriteStartupLadderStaticMap(step, out var mapError)) throw new IOException("Step 43 static map write failed before runtime read probe: " + mapError);
            WriteStartupLadderCheckpoint(step, "M43_B_STATIC_MAP_WRITE_RETURNED — verified Null-platform map durably written before runtime read probe.");
            if (!RecordStartupLadderGate(_step43Gates, _transformedRealStS2VeryEarlyInitialization.RunStep43NullPlatformReadProbe(d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!RecordStartupLadderGate(_step43Gates, _transformedRealStS2VeryEarlyInitialization.RunStep43FrozenConfinement(!GodotStep15NativeBridge.IsRenderingActive, d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            CompleteStartupLadderStep(step, _step43Gates, resultLabel, detailLabel,
                "Concrete NullPlatformUtilStrategy authority physically established with read-only platform getters and frozen zero-native confinement. Step 44 is now unlocked.");
            button.Enabled = false;
            WriteStartupLadderCheckpoint(step, "RUN_STEP43_4OF4 — Null platform authority closed; Step 44 may run in this same process.");
        }
        catch (Exception ex)
        {
            HandleStartupLadderException(step, resultLabel, detailLabel, ex, mutationArmed: false);
        }
        finally
        {
            await FinishStartupLadderStepAsync(step, "Step43-TransformedRealStS2NullPlatformAuthority.txt", "StS2 Launcher — Step 43.0 Null Platform Authority", resultLabel, detailLabel,
                "Step 43 never restarts rendering.");
        }
    }

    private async Task RunStep44StartupLadderAsync()
    {
        const int step = 44;
        if (!TryPrepareStartupLadderStep(step, _step43Gates.Snapshot().Passed && _transformedRealStS2VeryEarlyInitialization.ExactStep43ClosurePassed,
                "Step 44.0 requires Step 43.0 4/4 in this same process.", out var button, out var resultLabel, out var detailLabel))
            return;
        if (!TryInitializeStartupLadderTelemetry(step, "Legacy migration guard", "Step44-LegacyGuard-StaticMap", out var error))
        {
            SetStartupLadderRefusal(step, resultLabel, detailLabel, "TELEMETRY FAIL / NOT RUN", error);
            return;
        }
        BeginSteamOperation(allowCancel: false);
        _step44Gates.Reset();
        try
        {
            WriteStartupLadderCheckpoint(step, "RUN_START — Step 44.0 read-only legacy-data guard started. Migration/archive mutation remains forbidden.");
            if (!RecordStartupLadderGate(_step44Gates, _transformedRealStS2VeryEarlyInitialization.RunStep44ClosedStep43Authority(!GodotStep15NativeBridge.IsRenderingActive, d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!RecordStartupLadderGate(_step44Gates, _transformedRealStS2VeryEarlyInitialization.RunStep44LegacyGuardStaticAudit(d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!WriteStartupLadderStaticMap(step, out var mapError)) throw new IOException("Step 44 static map write failed before HasLegacyData probes: " + mapError);
            WriteStartupLadderCheckpoint(step, "M44_B_STATIC_MAP_WRITE_RETURNED — verified read-only legacy guard map durably written.");
            if (!RecordStartupLadderGate(_step44Gates, _transformedRealStS2VeryEarlyInitialization.RunStep44LegacyGuardProbe(d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!RecordStartupLadderGate(_step44Gates, _transformedRealStS2VeryEarlyInitialization.RunStep44FrozenConfinement(!GodotStep15NativeBridge.IsRenderingActive, d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            CompleteStartupLadderStep(step, _step44Gates, resultLabel, detailLabel,
                "No legacy account/profile data exists in this launcher sandbox; no migration mutation was required. Step 45 is now unlocked.");
            button.Enabled = false;
            WriteStartupLadderCheckpoint(step, "RUN_STEP44_4OF4 — no-legacy-data guard closed with mutationPerformed=NO; Step 45 may run.");
        }
        catch (Exception ex)
        {
            HandleStartupLadderException(step, resultLabel, detailLabel, ex, mutationArmed: false);
        }
        finally
        {
            await FinishStartupLadderStepAsync(step, "Step44-TransformedRealStS2LegacyMigrationGuard.txt", "StS2 Launcher — Step 44.0 Legacy Migration Guard", resultLabel, detailLabel,
                "Step 44 is read-only and never calls migration/archive methods.");
        }
    }

    private async Task RunStep45StartupLadderAsync()
    {
        const int step = 45;
        if (_step45InvocationUiStarted)
        {
            var labels = GetStartupLadderLabels(step);
            SetStartupLadderRefusal(step, labels.Result, labels.Detail, "ONE-SHOT ALREADY ARMED", "Step 45 crossed or attempted local save initialization in this process. Preserve reports and relaunch rather than retrying.");
            return;
        }
        if (!TryPrepareStartupLadderStep(step, _step44Gates.Snapshot().Passed && _transformedRealStS2VeryEarlyInitialization.ExactStep44ClosurePassed,
                "Step 45.0 requires Step 44.0 4/4/no legacy data in this same process.", out var button, out var resultLabel, out var detailLabel))
            return;
        if (!TryInitializeStartupLadderTelemetry(step, "Local save initialization", "Step45-LocalSave-StaticMap", out var error))
        {
            SetStartupLadderRefusal(step, resultLabel, detailLabel, "TELEMETRY FAIL / NOT RUN", error);
            return;
        }
        BeginSteamOperation(allowCancel: false);
        _step45Gates.Reset();
        try
        {
            WriteStartupLadderCheckpoint(step, "RUN_START — Step 45.0 local SaveManager initialization started; cloud/migration/Steam/main-menu remain unopened.");
            if (!RecordStartupLadderGate(_step45Gates, _transformedRealStS2VeryEarlyInitialization.RunStep45ClosedStep44Authority(!GodotStep15NativeBridge.IsRenderingActive, d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!RecordStartupLadderGate(_step45Gates, _transformedRealStS2VeryEarlyInitialization.RunStep45SaveInitializationStaticAudit(d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!WriteStartupLadderStaticMap(step, out var mapError)) throw new IOException("Step 45 static map write failed before one-shot SaveManager initialization: " + mapError);
            WriteStartupLadderCheckpoint(step, "M45_B_STATIC_MAP_WRITE_RETURNED — verified local-save map durably written before one-shot initialization.");
            _step45InvocationUiStarted = true;
            button.Enabled = false;
            WriteStartupLadderCheckpoint(step, "M45_C_UI_ARMED — Step-45 one-shot local SaveManager initialization armed; no in-process retry after this checkpoint.");
            if (!RecordStartupLadderGate(_step45Gates, _transformedRealStS2VeryEarlyInitialization.RunStep45ControlledSaveInitialization(d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!RecordStartupLadderGate(_step45Gates, _transformedRealStS2VeryEarlyInitialization.RunStep45FrozenSaveConfinement(!GodotStep15NativeBridge.IsRenderingActive, d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            CompleteStartupLadderStep(step, _step45Gates, resultLabel, detailLabel,
                "Local profile/progress/prefs state is initialized with SaveManager settings/prefs/progress authority non-null and frozen zero-native confinement. Step 46 is now unlocked.");
            WriteStartupLadderCheckpoint(step, "RUN_STEP45_4OF4 — local SaveManager initialization closed; Step 46 may map LaunchMainMenu in this same process.");
        }
        catch (Exception ex)
        {
            HandleStartupLadderException(step, resultLabel, detailLabel, ex, mutationArmed: _step45InvocationUiStarted);
        }
        finally
        {
            await FinishStartupLadderStepAsync(step, "Step45-TransformedRealStS2LocalSaveInitialization.txt", "StS2 Launcher — Step 45.0 Local Save Initialization", resultLabel, detailLabel,
                "If the one-shot boundary was armed, never retry Step 45 in-process.");
        }
    }

    private async Task RunStep46StartupLadderAsync()
    {
        const int step = 46;
        if (!TryPrepareStartupLadderStep(step, _step45Gates.Snapshot().Passed && _transformedRealStS2VeryEarlyInitialization.ExactStep45ClosurePassed,
                "Step 46.0 requires Step 45.0 4/4 local-save authority in this same process.", out var button, out var resultLabel, out var detailLabel))
            return;
        if (!TryInitializeStartupLadderTelemetry(step, "LaunchMainMenu immediate frontier map", "Step46-LaunchMainMenu-ImmediateFrontier-StaticMap", out var error))
        {
            SetStartupLadderRefusal(step, resultLabel, detailLabel, "TELEMETRY FAIL / NOT RUN", error);
            return;
        }
        BeginSteamOperation(allowCancel: false);
        _step46Gates.Reset();
        try
        {
            WriteStartupLadderCheckpoint(step, "RUN_START — Step 46.0 execution-aware LaunchMainMenu frontier map started; rendering remains frozen and original LaunchMainMenu is never invoked or authorized.");
            if (!RecordStartupLadderGate(_step46Gates, _transformedRealStS2VeryEarlyInitialization.RunStep46ClosedStep45Authority(!GodotStep15NativeBridge.IsRenderingActive, d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!RecordStartupLadderGate(_step46Gates, _transformedRealStS2VeryEarlyInitialization.RunStep46LaunchMainMenuStateMachineMap(d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!RecordStartupLadderGate(_step46Gates, _transformedRealStS2VeryEarlyInitialization.RunStep46LaunchMainMenuClosureAudit(d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!WriteStartupLadderStaticMap(step, out var mapError)) throw new IOException("Step 46 immediate/deferred frontier map write failed: " + mapError);
            _transformedRealStS2VeryEarlyInitialization.MarkStep46StaticMapDurablyWritten();
            WriteStartupLadderCheckpoint(step, "M46_C_STATIC_MAP_WRITE_RETURNED — complete execution-opcode-qualified immediate/deferred frontier map durably written. Unsafe immediate boundaries remain evidence only; original LaunchMainMenu is still forbidden.");
            if (!RecordStartupLadderGate(_step46Gates, _transformedRealStS2VeryEarlyInitialization.RunStep46FrozenNoInvocationConfinement(!GodotStep15NativeBridge.IsRenderingActive, d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            CompleteStartupLadderStep(step, _step46Gates, resultLabel, detailLabel,
                "Original LaunchMainMenu's exact immediate/deferred frontier is durably mapped without invocation or authorization. Step 47 direct PCK menu-resource preparation is now unlocked.");
            button.Enabled = false;
            WriteStartupLadderCheckpoint(step, "RUN_STEP46_4OF4 — execution-aware LaunchMainMenu frontier map closed; Step 47 direct-resource route may run. Original LaunchMainMenu remains unopened.");
        }
        catch (Exception ex)
        {
            HandleStartupLadderException(step, resultLabel, detailLabel, ex, mutationArmed: false);
        }
        finally
        {
            await FinishStartupLadderStepAsync(step, "Step46-TransformedRealStS2LaunchMainMenuImmediateFrontierMap.txt", "StS2 Launcher — Step 46.0 LaunchMainMenu Immediate Frontier Map", resultLabel, detailLabel,
                "Step 46 never invokes LaunchMainMenu or restarts rendering.");
        }
    }

    private async Task RunStep47StartupLadderAsync()
    {
        const int step = 47;
        if (_step47ResourceLoadUiStarted)
        {
            var labels = GetStartupLadderLabels(step);
            SetStartupLadderRefusal(step, labels.Result, labels.Detail, "ONE-SHOT ALREADY ARMED", "Step 47 Godot resource-cache load/takeover boundary was already armed in this process. Preserve reports and relaunch; never retry.");
            return;
        }
        if (!TryPrepareStartupLadderStep(step, _step46Gates.Snapshot().Passed && _transformedRealStS2VeryEarlyInitialization.ExactStep46ClosurePassed,
                "Step 47.0 requires Step 46.0 4/4 durable frontier-map authority in this same process.", out var button, out var resultLabel, out var detailLabel))
            return;
        if (!TryInitializeStartupLadderTelemetry(step, "Exact main-menu resource preparation", "Step47-MainMenuResources-StaticMap", out var error))
        {
            SetStartupLadderRefusal(step, resultLabel, detailLabel, "TELEMETRY FAIL / NOT RUN", error);
            return;
        }
        BeginSteamOperation(allowCancel: false);
        _step47Gates.Reset();
        try
        {
            WriteStartupLadderCheckpoint(step, "RUN_START — Step 47.0 exact PCK main-menu resource preparation started. Rendering is frozen; original LaunchMainMenu/ExecuteDeferred/Steam/native game extensions remain unopened.");
            if (!RecordStartupLadderGate(_step47Gates, _transformedRealStS2VeryEarlyInitialization.RunStep47ClosedStep46MapAuthority(!GodotStep15NativeBridge.IsRenderingActive, d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!RecordStartupLadderGate(_step47Gates, _transformedRealStS2VeryEarlyInitialization.RunStep47MainMenuResourcePreparation(d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!WriteStartupLadderStaticMap(step, out var mapError)) throw new IOException("Step 47 exact resource/derivative map write failed before ResourceLoader work: " + mapError);
            _transformedRealStS2VeryEarlyInitialization.MarkStep47StaticMapDurablyWritten();
            WriteStartupLadderCheckpoint(step, "M47_B_STATIC_MAP_WRITE_RETURNED — exact PCK resource authority + private Spine-neutral derivative map durably written before any Godot resource load.");
            _step47ResourceLoadUiStarted = true;
            button.Enabled = false;
            WriteStartupLadderCheckpoint(step, "M47_C_UI_ARMED — first/only Godot background cache takeover + exact main-menu PackedScene load authorized; no in-process retry after this checkpoint.");
            if (!RecordStartupLadderGate(_step47Gates, _transformedRealStS2VeryEarlyInitialization.RunStep47MainMenuPackedSceneLoad(d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!RecordStartupLadderGate(_step47Gates, _transformedRealStS2VeryEarlyInitialization.RunStep47FrozenResourceConfinement(!GodotStep15NativeBridge.IsRenderingActive, d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            CompleteStartupLadderStep(step, _step47Gates, resultLabel, detailLabel,
                "Exact main-menu PackedScene plus private Spine-neutral background cache authority closed 4/4 without instantiation. Step 48 off-tree NMainMenu instantiation is unlocked.");
            button.Enabled = false;
            WriteStartupLadderCheckpoint(step, "RUN_STEP47_4OF4 — exact direct-menu resources are loaded/retained with renderer frozen; Step 48 may instantiate NMainMenu off-tree once.");
        }
        catch (Exception ex)
        {
            HandleStartupLadderException(step, resultLabel, detailLabel, ex, mutationArmed: _step47ResourceLoadUiStarted);
        }
        finally
        {
            await FinishStartupLadderStepAsync(step, "Step47-TransformedRealStS2MainMenuResourcePreparation.txt", "StS2 Launcher — Step 47.0 Main Menu Resource Preparation", resultLabel, detailLabel,
                "Step 47 never instantiates the menu, invokes original LaunchMainMenu, or restarts rendering. If Gate C was attempted, relaunch rather than retry Step 47 in-process.");
        }
    }

    private bool TryPrepareStartupLadderStep(
        int step,
        bool prerequisitePassed,
        string prerequisiteMessage,
        out UIButton button,
        out UILabel resultLabel,
        out UILabel detailLabel)
    {
        var controls = GetStartupLadderControls(step);
        button = controls.Button;
        resultLabel = controls.Result;
        detailLabel = controls.Detail;
        if (_statusLabel is null)
            return false;
        if (!CurrentReleasePresentation.BundleIdentityMatchesExpected)
        {
            SetStartupLadderRefusal(step, resultLabel, detailLabel, "RELEASE IDENTITY FAIL", "Built bundle identity does not match the source-pinned candidate.");
            return false;
        }
        if (!prerequisitePassed)
        {
            SetStartupLadderRefusal(step, resultLabel, detailLabel, "PREREQUISITE NOT MET", prerequisiteMessage);
            return false;
        }
        if (GodotStep15NativeBridge.IsRenderingActive)
        {
            SetStartupLadderRefusal(step, resultLabel, detailLabel, "RENDERER MUST BE FROZEN", $"Step {step}.0 requires rendering stopped at entry. Only Steps 50, 52 and 55 may restart rendering, and only after their exact frame/input maps are durably written.");
            return false;
        }
        return true;
    }

    private (UIButton Button, UILabel Result, UILabel Detail) GetStartupLadderControls(int step)
        => step switch
        {
            43 => (_step43Button!, _step43ResultLabel!, _step43DetailLabel!),
            44 => (_step44Button!, _step44ResultLabel!, _step44DetailLabel!),
            45 => (_step45Button!, _step45ResultLabel!, _step45DetailLabel!),
            46 => (_step46Button!, _step46ResultLabel!, _step46DetailLabel!),
            47 => (_step47Button!, _step47ResultLabel!, _step47DetailLabel!),
            48 => (_step48Button!, _step48ResultLabel!, _step48DetailLabel!),
            49 => (_step49Button!, _step49ResultLabel!, _step49DetailLabel!),
            50 => (_step50Button!, _step50ResultLabel!, _step50DetailLabel!),
            51 => (_step51Button!, _step51ResultLabel!, _step51DetailLabel!),
            52 => (_step52Button!, _step52ResultLabel!, _step52DetailLabel!),
            >= 53 and <= 57 => GetSingleplayerContinuationControls(step),
            _ => throw new ArgumentOutOfRangeException(nameof(step)),
        };

    private (UILabel Result, UILabel Detail) GetStartupLadderLabels(int step)
    {
        var controls = GetStartupLadderControls(step);
        return (controls.Result, controls.Detail);
    }

    private void SetStartupLadderRefusal(int step, UILabel resultLabel, UILabel detailLabel, string result, string detail)
    {
        resultLabel.Text = $"STEP {step}.0: {result}";
        resultLabel.TextColor = UIColor.SystemOrange;
        detailLabel.Text = detail;
        if (_statusLabel is not null)
        {
            _statusLabel.Text = $"STEP {step}.0 REFUSED — {detail}";
            _statusLabel.TextColor = UIColor.SystemOrange;
        }
    }

    private bool RecordStartupLadderGate(
        TransformedRealStS2StartupLadderGateSequence sequence,
        TransformedRealStS2StartupLadderGateResult result,
        UILabel resultLabel,
        UILabel detailLabel)
    {
        sequence.Record(result);
        var snapshot = sequence.Snapshot();
        resultLabel.Text = snapshot.Summary;
        resultLabel.TextColor = result.Passed ? UIColor.Label : UIColor.SystemRed;
        detailLabel.Text = result.Detail;
        if (!result.Passed && _statusLabel is not null)
        {
            var gateLetter = (char)('A' + (int)result.Gate - 1);
            _statusLabel.Text = $"STEP {result.Step}.0 FAIL at Gate {gateLetter} ({result.Gate}). Stop the ladder here; preserve this step's reports. Later rungs remain locked.";
            _statusLabel.TextColor = UIColor.SystemRed;
        }
        return result.Passed;
    }

    private void CompleteStartupLadderStep(
        int step,
        TransformedRealStS2StartupLadderGateSequence sequence,
        UILabel resultLabel,
        UILabel detailLabel,
        string detail)
    {
        var snapshot = sequence.Snapshot();
        if (!snapshot.Passed)
            throw new InvalidOperationException($"Step {step}.0 cannot be completed without 4/4 gates.");
        resultLabel.Text = snapshot.Summary;
        resultLabel.TextColor = UIColor.Label;
        detailLabel.Text = detail;
        if (_statusLabel is not null)
        {
            _statusLabel.Text = $"{snapshot.Summary}. {detail}";
            _statusLabel.TextColor = UIColor.Label;
        }
    }

    private void HandleStartupLadderException(int step, UILabel resultLabel, UILabel detailLabel, Exception ex, bool mutationArmed)
    {
        WriteStartupLadderCheckpoint(step, $"RUN_MANAGED_EXCEPTION — {ex.GetType().FullName}: {SanitizeStartupLadderCheckpoint(ex.Message)}");
        resultLabel.Text = $"STEP {step}.0: EXCEPTION";
        resultLabel.TextColor = UIColor.SystemRed;
        detailLabel.Text = $"Unhandled Step {step}.0 exception: {ex.GetType().Name}: {ex.Message}";
        if (_statusLabel is not null)
        {
            _statusLabel.Text = mutationArmed
                ? $"STEP {step}.0 EXCEPTION AFTER ONE-SHOT ARM — preserve reports and relaunch; never retry this rung in-process."
                : $"STEP {step}.0 EXCEPTION — preserve reports; later rungs remain locked.";
            _statusLabel.TextColor = UIColor.SystemRed;
        }
    }

    private bool TryInitializeStartupLadderTelemetry(int step, string title, string? staticMapStem, out string error)
    {
        error = string.Empty;
        lock (_startupLadderCheckpointSync)
        {
            try
            {
                Directory.CreateDirectory(_deviceTestReportWriter.ReportsRoot);
                var now = DateTimeOffset.UtcNow;
                var runId = $"{now:yyyyMMddTHHmmssfffffffZ}-pid{Environment.ProcessId}-{Guid.NewGuid():N}";
                var crashPath = Path.Combine(_deviceTestReportWriter.ReportsRoot, $"Step{step}-CrashCheckpoint-{runId}.txt");
                var lastPath = Path.Combine(_deviceTestReportWriter.ReportsRoot, $"Step{step}-LastCheckpoint.txt");
                var staticPath = staticMapStem is null ? null : Path.Combine(_deviceTestReportWriter.ReportsRoot, $"{staticMapStem}-{runId}.txt");
                var state = new StartupLadderTelemetryState(step, title, runId, crashPath, lastPath, staticPath);
                _startupLadderTelemetry[step] = state;
                WriteStep35TextFileDurably(
                    crashPath,
                    $"StS2 Launcher — Step {step}.0 {title} checkpoint\n" +
                    "Output-only diagnostic; never consumed as trusted runtime input.\n" +
                    $"Run ID: {runId}\n" +
                    $"Initialized UTC: {now:O}\n" +
                    $"Process ID: {Environment.ProcessId}\n" +
                    $"App version: {CurrentReleasePresentation.DisplayVersion} ({CurrentReleasePresentation.DisplayBuild})\n" +
                    "Candidate: STEPS 43–57 SEQUENTIAL STARTUP LADDER — STOP ON FIRST FAILURE; STEPS 53–57 REQUIRE SAME-PROCESS PHYSICALLY CLOSED STEP-52 AUTHORITY.\n" +
                    "Global policy: physical authority through Step 52 remains the baseline. Steps 53–54/56–57 keep rendering frozen; Step 55 alone adds one bounded single-player-submenu render residency and synchronously refreezes. Original GameStartup/LaunchMainMenu, DoCloudSync, migration mutation, InitializePlatform, native Steamworks, ExecuteDeferred, native game GDExtensions, and character-select execution remain unopened.\n\n");
                WriteStartupLadderCheckpoint(step, "RUN_TELEMETRY_READY — run-correlated ladder journal created and durably flushed before Gate A.");
                return true;
            }
            catch (Exception ex)
            {
                error = $"{ex.GetType().Name}: {ex.Message}";
                return false;
            }
        }
    }

    private void WriteStartupLadderCheckpoint(int step, string detail)
    {
        lock (_startupLadderCheckpointSync)
        {
            if (!_startupLadderTelemetry.TryGetValue(step, out var state))
                return;
            try
            {
                var line = $"{DateTimeOffset.UtcNow:O} | run={state.RunId} | pid={Environment.ProcessId} | managedThread={Environment.CurrentManagedThreadId} | {SanitizeStartupLadderCheckpoint(detail)}";
                using (var stream = new FileStream(state.CrashPath, FileMode.Append, FileAccess.Write, FileShare.Read, 4096, FileOptions.None))
                using (var writer = new StreamWriter(stream, new UTF8Encoding(false), 1024, leaveOpen: true))
                {
                    writer.WriteLine(line);
                    writer.Flush();
                    stream.Flush(flushToDisk: true);
                }
                WriteStep35TextFileDurably(
                    state.LastPath,
                    $"StS2 Launcher — Step {step} last durable checkpoint\n" +
                    "Output-only diagnostic; overwrite-on-each-checkpoint convenience file.\n" +
                    $"Run ID: {state.RunId}\n" +
                    $"Journal: {Path.GetFileName(state.CrashPath)}\n" +
                    $"Static map: {(state.StaticMapPath is null ? "none (uses prior Step-46 durable authority for Step 47)" : Path.GetFileName(state.StaticMapPath))}\n" +
                    line + "\n");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Step-{step} ladder checkpoint append failed: {ex.GetType().Name}: {ex.Message}");
            }
        }
    }

    private bool WriteStartupLadderStaticMap(int step, out string error)
    {
        error = string.Empty;
        try
        {
            if (!_startupLadderTelemetry.TryGetValue(step, out var state) || state.StaticMapPath is null)
                throw new InvalidOperationException($"Step {step}.0 telemetry/static-map path is not initialized.");
            WriteStep35TextFileDurably(state.StaticMapPath, _transformedRealStS2VeryEarlyInitialization.GetVerifiedStartupLadderStaticMap(step));
            return true;
        }
        catch (Exception ex)
        {
            error = $"{ex.GetType().Name}: {ex.Message}";
            return false;
        }
    }

    private async Task FinishStartupLadderStepAsync(
        int step,
        string reportFile,
        string reportTitle,
        UILabel resultLabel,
        UILabel detailLabel,
        string finalPolicy)
    {
        WriteStartupLadderCheckpoint(step, $"RUN_FINALLY_ENTER — managed control reached finally; managedThread={Environment.CurrentManagedThreadId}; isMain={NSThread.IsMain}; renderingActive={GodotStep15NativeBridge.IsRenderingActive}. {finalPolicy}");
        await WriteDeviceTestReportFromLabelsAsync(reportFile, reportTitle, resultLabel, detailLabel, CancellationToken.None).ConfigureAwait(false);
        WriteStartupLadderCheckpoint(step, "RUN_NORMAL_REPORT_RETURNED — step report writer returned.");
        if (NSThread.IsMain)
            EndSteamOperation();
        else
            InvokeOnMainThread(EndSteamOperation);
        WriteStartupLadderCheckpoint(step, $"RUN_END — Step-{step} UI operation ended normally; {finalPolicy}");
    }

    private static string SanitizeStartupLadderCheckpoint(string? detail)
        => (detail ?? string.Empty).Replace('\r', ' ').Replace('\n', ' ');

    private sealed record StartupLadderTelemetryState(
        int Step,
        string Title,
        string RunId,
        string CrashPath,
        string LastPath,
        string? StaticMapPath);
}
