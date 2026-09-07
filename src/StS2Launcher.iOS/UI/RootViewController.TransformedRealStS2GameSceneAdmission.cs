using Foundation;
using StS2Launcher.Core;
using System.Text;

namespace StS2Launcher.iOS;

public sealed partial class RootViewController
{
    private readonly TransformedRealStS2GameSceneAdmissionGateSequence _step37Gates = new();
    private UIButton? _step37GameSceneButton;
    private UILabel? _step37ResultLabel;
    private UILabel? _step37DetailLabel;
    private readonly object _step37CheckpointSync = new();
    private string? _step37RunId;
    private string? _step37CrashCheckpointPath;
    private string? _step37LastCheckpointPath;
    private string? _step37StaticMapPath;
    private bool _step37TelemetryReady;

    private void AddTransformedRealStS2GameSceneAdmissionControls(UIStackView content)
    {
        content.AddArrangedSubview(Separator());
        content.AddArrangedSubview(Label(
            "Step 37.0 — Controlled game.tscn admission (FMOD-neutral, off-tree)",
            UIFont.BoldSystemFontOfSize(18),
            UIColor.Label));

        _step37GameSceneButton = SystemButton(
            "Run Step 37.0 A–D — Seal game.tscn → Prepare FMOD-neutral Copy → Load PackedScene → Instantiate Off-tree",
            16);
        _step37GameSceneButton.TouchUpInside += async (_, _) => await RunTransformedRealStS2GameSceneAdmissionAsync();
        content.AddArrangedSubview(_step37GameSceneButton);

        _step37ResultLabel = Label(
            "CONTROLLED GAME-SCENE ADMISSION: NOT RUN",
            UIFont.BoldSystemFontOfSize(15),
            UIColor.SecondaryLabel);
        content.AddArrangedSubview(_step37ResultLabel);

        _step37DetailLabel = Label(
            "Physical 0.0.159 closed Step 36.0.5 at 4/4: unchanged ExecuteEssential returned state 1→2 and all post-ModelDb serialization/message/action initialization completed with zero native loads. Step 37 does not run ExecuteDeferred or game startup. Gate A extracts the exact 10,414-byte game.tscn from the already verified PCK and requires its sealed SHA-256/MD5/layout. Gate B writes a copied derivative with exactly three compatibility edits: FmodBankLoader→Node, remove its desktop bank_paths property, and FmodListener2D→Node. Gate C only loads that copy as PackedScene through exact GodotSharp. Gate D instantiates it off-tree, verifies the NGame/AudioManager/SceneContainer/AssetLoader hierarchy and inert FMOD nodes, then immediately releases it. The instance is never AddChild'ed, so _EnterTree/_Ready, NGame.GameStartup, main-menu launch, ExecuteDeferred, Steam init, and native GDExtension loading remain forbidden.",
            UIFont.SystemFontOfSize(13),
            UIColor.SecondaryLabel);
        content.AddArrangedSubview(_step37DetailLabel);
    }

    private async Task RunTransformedRealStS2GameSceneAdmissionAsync()
    {
        if (_step37GameSceneButton is null || _step37ResultLabel is null || _step37DetailLabel is null || _statusLabel is null)
            return;

        if (!CurrentReleasePresentation.BundleIdentityMatchesExpected)
        {
            _step37ResultLabel.Text = "CONTROLLED GAME-SCENE ADMISSION: RELEASE IDENTITY FAIL";
            _step37ResultLabel.TextColor = UIColor.SystemRed;
            _statusLabel.Text = "STEP 37 REFUSED — built bundle identity does not match the source-pinned candidate.";
            _statusLabel.TextColor = UIColor.SystemRed;
            return;
        }

        if (!_transformedRealStS2VeryEarlyInitialization.ExactStep36ClosurePassed)
        {
            _step37ResultLabel.Text = "CONTROLLED GAME-SCENE ADMISSION: PREREQUISITE NOT MET";
            _step37ResultLabel.TextColor = UIColor.SystemOrange;
            _step37DetailLabel.Text = "Step 37.0 requires the same-process physical Step-36.0.5 4/4 closure. From a fresh launch run Step 15 A-C, then Step 35.0.32 MODEL-BOOTSTRAP 4/4, then Step 36.0.5 A-D 4/4. Do not use a historical exact-closure authority or skip Step 36.";
            _statusLabel.Text = "STEP 37.0 REFUSED — same-process Step-36.0.5 4/4 authority is not present.";
            _statusLabel.TextColor = UIColor.SystemOrange;
            return;
        }

        if (!TryInitializeStep37Telemetry(out var telemetryError))
        {
            _step37ResultLabel.Text = "CONTROLLED GAME-SCENE ADMISSION: TELEMETRY FAIL / NOT RUN";
            _step37ResultLabel.TextColor = UIColor.SystemRed;
            _step37DetailLabel.Text = telemetryError;
            _statusLabel.Text = "STEP 37 REFUSED — durable run telemetry could not be established.";
            _statusLabel.TextColor = UIColor.SystemRed;
            return;
        }

        BeginSteamOperation(allowCancel: false);
        _step37Gates.Reset();
        _step37ResultLabel.Text = "CONTROLLED GAME-SCENE ADMISSION: GATE A RUNNING…";
        _step37ResultLabel.TextColor = UIColor.Label;
        _statusLabel.Text = $"STEP 37.0 RUN {_step37RunId} — Step 36 is closed. Gate A reads only the PCK directory plus sealed game.tscn bytes; no scene load or native extension is allowed.";
        _statusLabel.TextColor = UIColor.Label;

        try
        {
            WriteStep37Checkpoint("RUN_START — Step 37.0 controlled game-scene admission started after same-process Step-36.0.5 4/4 closure.");

            var gateA = _transformedRealStS2VeryEarlyInitialization.RunGameSceneSealedPreflight(WriteStep37Checkpoint);
            if (!RecordStep37Gate(gateA))
                return;
            if (!WriteStep37StaticMap(out var staticMapError))
                throw new IOException("Step 37.0 verified static map could not be durably written: " + staticMapError);
            WriteStep37Checkpoint("G_A_STATIC_MAP_WRITE_RETURNED — verified sealed game-scene map durably written.");

            _step37ResultLabel.Text = "CONTROLLED GAME-SCENE ADMISSION: GATE B RUNNING…";
            _statusLabel.Text = "STEP 37.0 GATE B — create only the copied three-edit FMOD-neutral game.tscn derivative; trusted Step-12 install/PCK remains immutable.";
            var gateB = _transformedRealStS2VeryEarlyInitialization.RunGameSceneDerivativePreparation(WriteStep37Checkpoint);
            if (!RecordStep37Gate(gateB))
                return;

            _step37ResultLabel.Text = "CONTROLLED GAME-SCENE ADMISSION: GATE C RUNNING…";
            _statusLabel.Text = "STEP 37.0 GATE C — ResourceLoader.Load the copied derivative as PackedScene with cache-ignore. Do not instantiate yet; native loads remain forbidden.";
            WriteStep37Checkpoint($"G_C_UI_SELECTED — Gate C selected on UI thread; managedThread={Environment.CurrentManagedThreadId}; isMain={NSThread.IsMain}.");
            var gateC = _transformedRealStS2VeryEarlyInitialization.RunGameScenePackedLoad(WriteStep37Checkpoint);
            if (!RecordStep37Gate(gateC))
                return;

            _step37ResultLabel.Text = "CONTROLLED GAME-SCENE ADMISSION: GATE D RUNNING…";
            _statusLabel.Text = "STEP 37.0 GATE D — instantiate PackedScene off-tree only, verify managed root/expected hierarchy/inert FMOD nodes, release it immediately, and prove no native/initializer/rejected escape. No AddChild or startup.";
            WriteStep37Checkpoint($"G_D_UI_SELECTED — Gate D selected on UI thread; managedThread={Environment.CurrentManagedThreadId}; isMain={NSThread.IsMain}.");
            var gateD = _transformedRealStS2VeryEarlyInitialization.RunGameSceneOffTreeInstantiationAudit(WriteStep37Checkpoint);
            if (!RecordStep37Gate(gateD))
                return;

            var snapshot = _step37Gates.Snapshot();
            _step37ResultLabel.Text = snapshot.Summary;
            _step37ResultLabel.TextColor = UIColor.Label;
            _step37DetailLabel.Text =
                "All four Step 37.0 gates passed. The exact PCK game.tscn authority matched its sealed 10,414-byte SHA-256/MD5/layout; only a private copied derivative was modified; exact GodotSharp loaded it as PackedScene without native escape; and an NGame hierarchy was instantiated off-tree with FmodBankLoader/FmodListener2D replaced by inert Godot.Node instances. The root never entered the SceneTree and was released immediately. ExecuteDeferred, NGame.GameStartup, main-menu launch, Steam initialization, FMOD/Spine/Sentry GDExtension loading, and gameplay remain separate future boundaries.";
            _statusLabel.Text = "STEP 37.0 COMPLETE — 4/4. Controlled off-tree NGame scene admission is closed; preserve Step37 artifacts before designing the first tree/startup boundary.";
            _statusLabel.TextColor = UIColor.Label;
            WriteStep37Checkpoint("RUN_STEP37_4OF4 — sealed game scene loaded and instantiated off-tree under FMOD-neutral compatibility; no SceneTree/startup/deferred/native boundary crossed.");
        }
        catch (Exception ex)
        {
            WriteStep37Checkpoint($"RUN_MANAGED_EXCEPTION — {ex.GetType().FullName}: {ex.Message}");
            _step37ResultLabel.Text = "CONTROLLED GAME-SCENE ADMISSION: EXCEPTION";
            _step37ResultLabel.TextColor = UIColor.SystemRed;
            _step37DetailLabel.Text = $"Unhandled Step 37.0 exception: {ex.GetType().Name}: {ex.Message}";
            _statusLabel.Text = "STEP 37 FAIL — preserve Step37 artifacts and use a fresh process before retry if Gate C or D began.";
            _statusLabel.TextColor = UIColor.SystemRed;
        }
        finally
        {
            WriteStep37Checkpoint($"RUN_FINALLY_ENTER — Step-37 managed control reached finally; managedThread={Environment.CurrentManagedThreadId}; isMain={NSThread.IsMain}.");
            await WriteDeviceTestReportFromLabelsAsync(
                "Step37-TransformedRealStS2GameSceneAdmission.txt",
                "StS2 Launcher — Step 37.0 Controlled FMOD-neutral Game-Scene Admission",
                _step37ResultLabel,
                _step37DetailLabel,
                CancellationToken.None).ConfigureAwait(false);
            WriteStep37Checkpoint("RUN_NORMAL_REPORT_RETURNED — Step37 report writer returned.");
            if (NSThread.IsMain)
                EndSteamOperation();
            else
                InvokeOnMainThread(EndSteamOperation);
            WriteStep37Checkpoint("RUN_END — Step-37 UI operation ended normally after explicit teardown.");
        }
    }

    private bool RecordStep37Gate(TransformedRealStS2GameSceneAdmissionGateResult result)
    {
        _step37Gates.Record(result);
        if (_step37ResultLabel is not null)
        {
            _step37ResultLabel.Text = _step37Gates.Snapshot().Summary;
            _step37ResultLabel.TextColor = result.Passed ? UIColor.Label : UIColor.SystemRed;
        }
        if (_step37DetailLabel is not null)
            _step37DetailLabel.Text = result.Detail;
        if (!result.Passed && _statusLabel is not null)
        {
            var letter = (char)('A' + (int)result.Gate - 1);
            _statusLabel.Text = $"STEP 37.0 FAIL at Gate {letter} ({result.Gate}). Stop here; later gates were not run. Preserve Step37 artifacts and use a fresh process before retry if Gate C or D began.";
            _statusLabel.TextColor = UIColor.SystemRed;
        }
        return result.Passed;
    }

    private bool TryInitializeStep37Telemetry(out string error)
    {
        error = string.Empty;
        lock (_step37CheckpointSync)
        {
            _step37TelemetryReady = false;
            _step37RunId = null;
            _step37CrashCheckpointPath = null;
            _step37LastCheckpointPath = null;
            _step37StaticMapPath = null;
            try
            {
                Directory.CreateDirectory(_deviceTestReportWriter.ReportsRoot);
                var now = DateTimeOffset.UtcNow;
                var runId = $"{now:yyyyMMddTHHmmssfffffffZ}-pid{Environment.ProcessId}-{Guid.NewGuid():N}";
                var crashName = $"Step37-CrashCheckpoint-{runId}.txt";
                var staticName = $"Step37-GameScene-StaticMap-{runId}.txt";
                var crashPath = Path.Combine(_deviceTestReportWriter.ReportsRoot, crashName);
                var lastPath = Path.Combine(_deviceTestReportWriter.ReportsRoot, "Step37-LastCheckpoint.txt");
                var staticPath = Path.Combine(_deviceTestReportWriter.ReportsRoot, staticName);
                WriteStep35TextFileDurably(
                    crashPath,
                    "StS2 Launcher — Step 37.0 controlled game-scene admission checkpoint\n" +
                    "Output-only diagnostic; never consumed as trusted runtime input.\n" +
                    $"Run ID: {runId}\n" +
                    $"Initialized UTC: {now:O}\n" +
                    $"Process ID: {Environment.ProcessId}\n" +
                    $"App version: {CurrentReleasePresentation.DisplayVersion} ({CurrentReleasePresentation.DisplayBuild})\n" +
                    "Candidate: STEP 37.0 — SEALED GAME.TSCN + FMOD-NEUTRAL COPY + PACKEDSCENE LOAD + OFF-TREE INSTANTIATION\n" +
                    "Prerequisite: same-process Step-36.0.5 4/4 authority; ExecuteDeferred/GameStartup/main-menu/Steam/native GDExtension loading remain forbidden.\n\n");
                _step37RunId = runId;
                _step37CrashCheckpointPath = crashPath;
                _step37LastCheckpointPath = lastPath;
                _step37StaticMapPath = staticPath;
                _step37TelemetryReady = true;
                WriteStep37Checkpoint("RUN_TELEMETRY_READY — Step37 run journal created and durably flushed before Gate A.");
                return true;
            }
            catch (Exception ex)
            {
                error = $"{ex.GetType().Name}: {ex.Message}";
                return false;
            }
        }
    }

    private void WriteStep37Checkpoint(string detail)
    {
        if (!_step37TelemetryReady || string.IsNullOrWhiteSpace(_step37RunId) ||
            string.IsNullOrWhiteSpace(_step37CrashCheckpointPath) || string.IsNullOrWhiteSpace(_step37LastCheckpointPath))
            return;
        try
        {
            lock (_step37CheckpointSync)
            {
                var single = (detail ?? string.Empty).Replace('\r', ' ').Replace('\n', ' ');
                var line = $"{DateTimeOffset.UtcNow:O} | run={_step37RunId} | pid={Environment.ProcessId} | managedThread={Environment.CurrentManagedThreadId} | {single}";
                using (var stream = new FileStream(_step37CrashCheckpointPath, FileMode.Append, FileAccess.Write, FileShare.Read, 4096, FileOptions.None))
                using (var writer = new StreamWriter(stream, new UTF8Encoding(false), 1024, leaveOpen: true))
                {
                    writer.WriteLine(line);
                    writer.Flush();
                    stream.Flush(flushToDisk: true);
                }
                WriteStep35TextFileDurably(
                    _step37LastCheckpointPath,
                    "StS2 Launcher — Step 37 last durable checkpoint\n" +
                    "Output-only diagnostic; overwrite-on-each-checkpoint convenience file.\n" +
                    $"Run ID: {_step37RunId}\n" +
                    $"Journal: {Path.GetFileName(_step37CrashCheckpointPath)}\n" +
                    $"Static map: {Path.GetFileName(_step37StaticMapPath)}\n" +
                    line + "\n");
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Step-37 checkpoint append failed: {ex.GetType().Name}: {ex.Message}");
        }
    }

    private bool WriteStep37StaticMap(out string error)
    {
        error = string.Empty;
        try
        {
            if (!_step37TelemetryReady || string.IsNullOrWhiteSpace(_step37StaticMapPath))
                throw new InvalidOperationException("Step37 telemetry/static-map path is not initialized.");
            WriteStep35TextFileDurably(
                _step37StaticMapPath,
                _transformedRealStS2VeryEarlyInitialization.GetVerifiedGameSceneStaticMap());
            return true;
        }
        catch (Exception ex)
        {
            error = $"{ex.GetType().Name}: {ex.Message}";
            return false;
        }
    }
}
