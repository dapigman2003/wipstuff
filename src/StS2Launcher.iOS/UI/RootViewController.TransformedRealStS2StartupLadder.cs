using Foundation;
using StS2Launcher.Core;
using StS2Launcher.iOS.Platform;
using System.Text;

namespace StS2Launcher.iOS;

public sealed partial class RootViewController
{
    private readonly TransformedRealStS2StartupLadderGateSequence _step43Gates = new(43, "NULL PLATFORM AUTHORITY");
    private readonly TransformedRealStS2StartupLadderGateSequence _step44Gates = new(44, "LEGACY MIGRATION GUARD");
    private readonly TransformedRealStS2StartupLadderGateSequence _step45Gates = new(45, "LOCAL SAVE INITIALIZATION");
    private readonly TransformedRealStS2StartupLadderGateSequence _step46Gates = new(46, "LAUNCHMAINMENU ASYNC MAP");
    private readonly TransformedRealStS2StartupLadderGateSequence _step47Gates = new(47, "CONTROLLED LAUNCHMAINMENU");

    private readonly Dictionary<int, StartupLadderTelemetryState> _startupLadderTelemetry = [];
    private readonly object _startupLadderCheckpointSync = new();

    private UIButton? _step43Button;
    private UIButton? _step44Button;
    private UIButton? _step45Button;
    private UIButton? _step46Button;
    private UIButton? _step47Button;
    private UILabel? _step43ResultLabel;
    private UILabel? _step44ResultLabel;
    private UILabel? _step45ResultLabel;
    private UILabel? _step46ResultLabel;
    private UILabel? _step47ResultLabel;
    private UILabel? _step43DetailLabel;
    private UILabel? _step44DetailLabel;
    private UILabel? _step45DetailLabel;
    private UILabel? _step46DetailLabel;
    private UILabel? _step47DetailLabel;
    private bool _step45InvocationUiStarted;
    private bool _step47InvocationUiStarted;
    private bool _step47RunPassed;

    private void AddTransformedRealStS2StartupLadderControls(UIStackView content)
    {
        content.AddArrangedSubview(Separator());
        content.AddArrangedSubview(Label(
            "Steps 43–47 — sequential startup ladder (stop on first failure)",
            UIFont.BoldSystemFontOfSize(18),
            UIColor.Label));
        content.AddArrangedSubview(Label(
            "Each rung is independently gated and reportable. Later rungs remain locked until the prior rung is 4/4 in the same process. Steps 43–46 keep rendering frozen. Step 47 is exposed only after a complete admissible LaunchMainMenu async map and is one-shot; it starts rendering immediately before exact LaunchMainMenu(skipIntro=true), then leaves rendering active only on a full 4/4 pass.",
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
            "Step 46.0 — exact LaunchMainMenu async state-machine + closure map",
            "Run Step 46.0 A–D",
            "LAUNCHMAINMENU ASYNC MAP: LOCKED",
            "Requires Step 45.0 4/4. Resolves LaunchMainMenu(bool)'s own compiler state machine and maps MoveNext transitively. Null-platform reads and inert Sentry wrappers are allowed; OneTimeInitialization, InitializePlatform, deferred-startup, external Steamworks, FMOD, Spine, external Sentry, and native-extension edges fail before launch.");
        _step46Button.TouchUpInside += async (_, _) => await RunStep46StartupLadderAsync();

        (_step47Button, _step47ResultLabel, _step47DetailLabel) = AddStartupLadderStepControls(
            content,
            "Step 47.0 — one-shot controlled live LaunchMainMenu",
            "Run Step 47.0 A–D — LIVE MAIN MENU ATTEMPT",
            "CONTROLLED LAUNCHMAINMENU: LOCKED",
            "Requires Step 46.0 4/4 plus a recursively expanded durable admissible map. Binds exact runtime token, starts rendering once, invokes LaunchMainMenu(skipIntro=true) once, and awaits the un-cancelable Task to completion with no abandonment timeout. A hard hang is diagnosed from the durable pre-invocation checkpoint and requires relaunch; normal managed failures refreeze rendering. On 4/4 success RootSceneContainer must gain a real child scene and rendering intentionally remains active for observation.");
        _step47Button.TouchUpInside += async (_, _) => await RunStep47StartupLadderAsync();
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
        if (!TryInitializeStartupLadderTelemetry(step, "LaunchMainMenu async map", "Step46-LaunchMainMenu-StaticMap", out var error))
        {
            SetStartupLadderRefusal(step, resultLabel, detailLabel, "TELEMETRY FAIL / NOT RUN", error);
            return;
        }
        BeginSteamOperation(allowCancel: false);
        _step46Gates.Reset();
        try
        {
            WriteStartupLadderCheckpoint(step, "RUN_START — Step 46.0 exact LaunchMainMenu async map started; rendering remains frozen and LaunchMainMenu is not invoked.");
            if (!RecordStartupLadderGate(_step46Gates, _transformedRealStS2VeryEarlyInitialization.RunStep46ClosedStep45Authority(!GodotStep15NativeBridge.IsRenderingActive, d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!RecordStartupLadderGate(_step46Gates, _transformedRealStS2VeryEarlyInitialization.RunStep46LaunchMainMenuStateMachineMap(d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!RecordStartupLadderGate(_step46Gates, _transformedRealStS2VeryEarlyInitialization.RunStep46LaunchMainMenuClosureAudit(d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!WriteStartupLadderStaticMap(step, out var mapError)) throw new IOException("Step 46 complete LaunchMainMenu static map write failed: " + mapError);
            _transformedRealStS2VeryEarlyInitialization.MarkStep46StaticMapDurablyWritten();
            WriteStartupLadderCheckpoint(step, "M46_C_STATIC_MAP_WRITE_RETURNED — complete LaunchMainMenu state-machine + transitive admissibility map durably written and marked authoritative before Step 46 Gate D / any Step 47 launch.");
            if (!RecordStartupLadderGate(_step46Gates, _transformedRealStS2VeryEarlyInitialization.RunStep46FrozenNoInvocationConfinement(!GodotStep15NativeBridge.IsRenderingActive, d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            CompleteStartupLadderStep(step, _step46Gates, resultLabel, detailLabel,
                "LaunchMainMenu's own compiler-generated MoveNext closure is fully mapped, durable, and admissible with no invocation. Step 47 live launch is now unlocked in this same process.");
            button.Enabled = false;
            WriteStartupLadderCheckpoint(step, "RUN_STEP46_4OF4 — LaunchMainMenu async map closed with launchAdmissible=True; Step 47 may run once.");
        }
        catch (Exception ex)
        {
            HandleStartupLadderException(step, resultLabel, detailLabel, ex, mutationArmed: false);
        }
        finally
        {
            await FinishStartupLadderStepAsync(step, "Step46-TransformedRealStS2LaunchMainMenuMap.txt", "StS2 Launcher — Step 46.0 LaunchMainMenu Async Map", resultLabel, detailLabel,
                "Step 46 never invokes LaunchMainMenu or restarts rendering.");
        }
    }

    private async Task RunStep47StartupLadderAsync()
    {
        const int step = 47;
        if (_step47InvocationUiStarted)
        {
            var labels = GetStartupLadderLabels(step);
            SetStartupLadderRefusal(step, labels.Result, labels.Detail, "ONE-SHOT ALREADY ARMED", "Step 47 crossed or attempted the live LaunchMainMenu boundary in this process. Preserve reports and relaunch; never retry.");
            return;
        }
        if (!TryPrepareStartupLadderStep(step, _step46Gates.Snapshot().Passed && _transformedRealStS2VeryEarlyInitialization.ExactStep46ClosurePassed && _transformedRealStS2VeryEarlyInitialization.Step46LaunchMainMenuAdmissible,
                "Step 47.0 requires Step 46.0 4/4 with a durable admissible LaunchMainMenu map in this same process.", out var button, out var resultLabel, out var detailLabel))
            return;
        if (!TryInitializeStartupLadderTelemetry(step, "Controlled live LaunchMainMenu", null, out var error))
        {
            SetStartupLadderRefusal(step, resultLabel, detailLabel, "TELEMETRY FAIL / NOT RUN", error);
            return;
        }
        BeginSteamOperation(allowCancel: false);
        _step47Gates.Reset();
        _step47RunPassed = false;
        try
        {
            WriteStartupLadderCheckpoint(step, "RUN_START — Step 47.0 one-shot controlled live LaunchMainMenu started from Step-46 admissible map authority. Rendering is still frozen at entry.");
            if (!RecordStartupLadderGate(_step47Gates, _transformedRealStS2VeryEarlyInitialization.RunStep47ClosedStep46Authority(!GodotStep15NativeBridge.IsRenderingActive, d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;
            if (!RecordStartupLadderGate(_step47Gates, _transformedRealStS2VeryEarlyInitialization.RunStep47ExactRuntimeBinding(d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel)) return;

            _step47InvocationUiStarted = true;
            button.Enabled = false;
            WriteStartupLadderCheckpoint(step, $"M47_C_UI_ARMED — first and only live LaunchMainMenu attempt authorized; managedThread={Environment.CurrentManagedThreadId}; isMain={NSThread.IsMain}; renderingActiveBefore={GodotStep15NativeBridge.IsRenderingActive}.");
            WriteStartupLadderCheckpoint(step, "M47_C_START_RENDERING_CALL — invoking StartRendering exactly once immediately before LaunchMainMenu.");
            var startReturned = GodotStep15NativeBridge.StartRendering();
            WriteStartupLadderCheckpoint(step, $"M47_C_START_RENDERING_RETURNED — returned={startReturned}; renderingActiveAfterStart={GodotStep15NativeBridge.IsRenderingActive}; nativeError='{SanitizeStartupLadderCheckpoint(GodotStep15NativeBridge.LastError)}'.");
            var gateC = await _transformedRealStS2VeryEarlyInitialization.RunStep47ControlledLaunchMainMenuInvocationAsync(
                startReturned && GodotStep15NativeBridge.IsRenderingActive,
                d => WriteStartupLadderCheckpoint(step, d));
            if (!RecordStartupLadderGate(_step47Gates, gateC, resultLabel, detailLabel))
            {
                RefreezeStep47AfterFailure();
                return;
            }
            if (!RecordStartupLadderGate(_step47Gates, _transformedRealStS2VeryEarlyInitialization.RunStep47LivePostLaunchConfinement(GodotStep15NativeBridge.IsRenderingActive, d => WriteStartupLadderCheckpoint(step, d)), resultLabel, detailLabel))
            {
                RefreezeStep47AfterFailure();
                return;
            }
            _step47RunPassed = true;
            CompleteStartupLadderStep(step, _step47Gates, resultLabel, detailLabel,
                "FIRST CONTROLLED REAL MAIN-MENU LAUNCH CLOSED 4/4. LaunchMainMenu(skipIntro=true) completed, RootSceneContainer gained a child scene, NGame/state/context remained confined, and rendering intentionally remains active for visual observation.");
            WriteStartupLadderCheckpoint(step, "RUN_STEP47_4OF4 — controlled LaunchMainMenu completed and a real root-scene child is present; rendering intentionally remains active on successful closure.");
        }
        catch (Exception ex)
        {
            HandleStartupLadderException(step, resultLabel, detailLabel, ex, mutationArmed: _step47InvocationUiStarted);
            RefreezeStep47AfterFailure();
        }
        finally
        {
            if (_step47InvocationUiStarted && !_step47RunPassed)
                RefreezeStep47AfterFailure();
            await FinishStartupLadderStepAsync(step, "Step47-TransformedRealStS2ControlledLaunchMainMenu.txt", "StS2 Launcher — Step 47.0 Controlled LaunchMainMenu", resultLabel, detailLabel,
                _step47RunPassed ? "Step 47 succeeded; rendering intentionally remains active." : "Step 47 failed/was incomplete; launcher refroze rendering at the first managed opportunity.");
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
            SetStartupLadderRefusal(step, resultLabel, detailLabel, "RENDERER MUST BE FROZEN", $"Step {step}.0 requires rendering stopped at entry. Step 47 itself will start rendering only after its exact Gate-B binding.");
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

    private void RefreezeStep47AfterFailure()
    {
        if (!GodotStep15NativeBridge.IsRenderingActive)
            return;
        WriteStartupLadderCheckpoint(47, "M47_FAIL_STOP_RENDERING_CALL — Step 47 did not close 4/4; synchronously refreezing rendering at the first managed opportunity.");
        var stopped = GodotStep15NativeBridge.StopRendering();
        WriteStartupLadderCheckpoint(47, $"M47_FAIL_STOP_RENDERING_RETURNED — returned={stopped}; renderingActiveAfterStop={GodotStep15NativeBridge.IsRenderingActive}; nativeError='{SanitizeStartupLadderCheckpoint(GodotStep15NativeBridge.LastError)}'.");
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
                    "Candidate: STEPS 43–47 SEQUENTIAL STARTUP LADDER — STOP ON FIRST FAILURE; LATER RUNGS REQUIRE SAME-PROCESS PRIOR 4/4 AUTHORITY.\n" +
                    "Global policy: Steps 43–46 keep rendering frozen. GameStartup itself, DoCloudSync, migration mutation, InitializePlatform, native Steamworks, FMOD/Spine/native game extensions remain unopened unless a rung explicitly says otherwise. Step 47 alone may start rendering and invoke exact admissible LaunchMainMenu once.\n\n");
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
