using Foundation;
using StS2Launcher.Core;
using System.Text;

namespace StS2Launcher.iOS;

public sealed partial class RootViewController
{
    private readonly TransformedRealStS2SceneTreeAdmissionGateSequence _step39Gates = new();
    private UIButton? _step39SceneTreeButton;
    private UILabel? _step39ResultLabel;
    private UILabel? _step39DetailLabel;
    private readonly object _step39CheckpointSync = new();
    private string? _step39RunId;
    private string? _step39CrashCheckpointPath;
    private string? _step39LastCheckpointPath;
    private string? _step39StaticMapPath;
    private bool _step39TelemetryReady;

    private void AddTransformedRealStS2SceneTreeAdmissionControls(UIStackView content)
    {
        content.AddArrangedSubview(Separator());
        content.AddArrangedSubview(Label(
            "Step 39.0 — Real SceneTree admission / automatic lifecycle",
            UIFont.BoldSystemFontOfSize(18),
            UIColor.Label));

        _step39SceneTreeButton = SystemButton(
            "Run Step 39.0 A–D — Audit hierarchy → Add NGame to SceneTree → Freeze rendering → Confinement",
            16);
        _step39SceneTreeButton.TouchUpInside += async (_, _) => await RunTransformedRealStS2SceneTreeAdmissionAsync();
        content.AddArrangedSubview(_step39SceneTreeButton);

        _step39ResultLabel = Label(
            "REAL SCENETREE ADMISSION: NOT RUN",
            UIFont.BoldSystemFontOfSize(15),
            UIColor.SecondaryLabel);
        content.AddArrangedSubview(_step39ResultLabel);

        _step39DetailLabel = Label(
            "Physical 0.0.165 closed Step 38.2 at 4/4: the verified iOS/off-tree-compatible NGame._EnterTree returned with state/resolver/native confinement intact. Step 39.0 does not rerun that synthetic callback. It requires the same-process Step-37.0.1 scene authority directly, re-verifies the Step-38.2 selected compatibility image and four exact PCK preflight resources, creates a fresh NGame with NGame.Instance still null, audits the actual instantiated hierarchy's managed _EnterTree/_Ready/_Notification surface, and then performs exactly one real SceneTree.Root.AddChild(NGame). Godot may automatically run the audited enter/ready callbacks. GameStartup remains blocked by the verified inert wrapper. Immediately after Gate C returns, the launcher synchronously stops the Godot render loop before recording the result or entering Gate D so _Process-style frame callbacks do not become an uncontrolled boundary. The inserted NGame is intentionally retained in-tree; Step 39 does not RemoveChild, Free, call _ExitTree, restart rendering, enter GameStartup/platform/main-menu/deferred startup, initialize Steam, or load native game GDExtensions. IMPORTANT: use a fresh process and skip Step 38 before this button.",
            UIFont.SystemFontOfSize(13),
            UIColor.SecondaryLabel);
        content.AddArrangedSubview(_step39DetailLabel);
    }

    private async Task RunTransformedRealStS2SceneTreeAdmissionAsync()
    {
        if (_step39SceneTreeButton is null || _step39ResultLabel is null || _step39DetailLabel is null || _statusLabel is null)
            return;

        if (!CurrentReleasePresentation.BundleIdentityMatchesExpected)
        {
            _step39ResultLabel.Text = "REAL SCENETREE ADMISSION: RELEASE IDENTITY FAIL";
            _step39ResultLabel.TextColor = UIColor.SystemRed;
            _statusLabel.Text = "STEP 39 REFUSED — built bundle identity does not match the source-pinned candidate.";
            _statusLabel.TextColor = UIColor.SystemRed;
            return;
        }

        if (!_step37Gates.Snapshot().Passed || !_transformedRealStS2VeryEarlyInitialization.ExactStep37ClosurePassed)
        {
            _step39ResultLabel.Text = "REAL SCENETREE ADMISSION: PREREQUISITE NOT MET";
            _step39ResultLabel.TextColor = UIColor.SystemOrange;
            _step39DetailLabel.Text = "Step 39.0 requires the same-process Step-37.0.1 4/4 closure. From a fresh launch run Step 15 A-C, Step 35.0.32 MODEL-BOOTSTRAP 4/4, Step 36.0.5 4/4, then Step 37.0.1 4/4. Do not run Step 38 in that process before Step 39.";
            _statusLabel.Text = "STEP 39.0 REFUSED — same-process Step-37.0.1 4/4 authority is not present.";
            _statusLabel.TextColor = UIColor.SystemOrange;
            return;
        }

        if (_step38Gates.Results.Count != 0)
        {
            _step39ResultLabel.Text = "REAL SCENETREE ADMISSION: FRESH PROCESS REQUIRED";
            _step39ResultLabel.TextColor = UIColor.SystemOrange;
            _step39DetailLabel.Text = "Step 38 was already invoked in this process. Its synthetic off-tree NGame._EnterTree experiment intentionally establishes the NGame singleton without running _ExitTree. Relaunch, run Step 15 → 35 → 36 → 37, skip Step 38, then run Step 39.";
            _statusLabel.Text = "STEP 39.0 REFUSED — relaunch and skip Step 38 before real SceneTree admission.";
            _statusLabel.TextColor = UIColor.SystemOrange;
            return;
        }

        if (!TryInitializeStep39Telemetry(out var telemetryError))
        {
            _step39ResultLabel.Text = "REAL SCENETREE ADMISSION: TELEMETRY FAIL / NOT RUN";
            _step39ResultLabel.TextColor = UIColor.SystemRed;
            _step39DetailLabel.Text = telemetryError;
            _statusLabel.Text = "STEP 39 REFUSED — durable run telemetry could not be established.";
            _statusLabel.TextColor = UIColor.SystemRed;
            return;
        }

        BeginSteamOperation(allowCancel: false);
        _step39Gates.Reset();
        _step39ResultLabel.Text = "REAL SCENETREE ADMISSION: GATE A RUNNING…";
        _step39ResultLabel.TextColor = UIColor.Label;
        _statusLabel.Text = $"STEP 39.0 RUN {_step39RunId} — Step 38 is prior physical evidence and is intentionally skipped in this process. Gate A re-verifies the exact compatibility/PCK preflight authority before any insertion.";
        _statusLabel.TextColor = UIColor.Label;

        try
        {
            WriteStep39Checkpoint("RUN_START — Step 39.0 real SceneTree admission started after same-process Step-37.0.1 4/4 closure; Step 38 was not run in this process.");

            var gateA = _transformedRealStS2VeryEarlyInitialization.RunStep39PreInsertionAuthorityAndResourceAudit(WriteStep39Checkpoint);
            if (!RecordStep39Gate(gateA))
                return;

            _step39ResultLabel.Text = "REAL SCENETREE ADMISSION: GATE B RUNNING…";
            _statusLabel.Text = "STEP 39.0 GATE B — instantiate fresh NGame off-tree, require NGame.Instance=null, resolve the live SceneTree root, and map the actual hierarchy's immediate managed lifecycle surface. AddChild is still forbidden.";
            WriteStep39Checkpoint($"I_B_UI_SELECTED — Gate B selected on UI thread; managedThread={Environment.CurrentManagedThreadId}; isMain={NSThread.IsMain}.");
            var gateB = _transformedRealStS2VeryEarlyInitialization.RunStep39OffTreeHierarchyAndLifecycleSurfaceAudit(WriteStep39Checkpoint);
            if (!RecordStep39Gate(gateB))
                return;
            if (!WriteStep39StaticMap(out var staticMapError))
                throw new IOException("Step 39.0 verified pre-insertion static map could not be durably written: " + staticMapError);
            WriteStep39Checkpoint("I_B_STATIC_MAP_WRITE_RETURNED — verified Step-39 pre-insertion hierarchy/lifecycle static map durably written.");

            _step39ResultLabel.Text = "REAL SCENETREE ADMISSION: GATE C RUNNING…";
            _statusLabel.Text = "STEP 39.0 GATE C — perform exactly one live SceneTree.Root.AddChild(NGame). Automatic audited _EnterTree/_Ready callbacks are authorized; GameStartup remains inert. Rendering will be frozen synchronously immediately when this gate returns.";
            WriteStep39Checkpoint($"I_C_UI_SELECTED — Gate C selected on UI thread; managedThread={Environment.CurrentManagedThreadId}; isMain={NSThread.IsMain}; renderingActiveBefore={GodotStep15NativeBridge.IsRenderingActive}.");
            var gateC = _transformedRealStS2VeryEarlyInitialization.RunStep39RealSceneTreeInsertion(WriteStep39Checkpoint);

            var renderingStopped = GodotStep15NativeBridge.StopRendering();
            var renderingActiveAfterStop = GodotStep15NativeBridge.IsRenderingActive;
            WriteStep39Checkpoint($"I_C_RENDER_FREEZE_RETURNED — Gate C returned passed={gateC.Passed}; StopRendering returned={renderingStopped}; renderingActiveAfterStop={renderingActiveAfterStop}. Rendering is not restarted by Step 39.");

            if (!RecordStep39Gate(gateC))
            {
                _statusLabel.Text = renderingStopped && !renderingActiveAfterStop
                    ? "STEP 39.0 FAIL at Gate C — render loop frozen after the attempted AddChild. Preserve Step39 artifacts and relaunch; do not continue this process."
                    : "STEP 39.0 FAIL at Gate C — AddChild attempt returned failure and render-loop freeze was not confirmed. Preserve Step39 artifacts and relaunch immediately.";
                _statusLabel.TextColor = UIColor.SystemRed;
                return;
            }

            _step39ResultLabel.Text = "REAL SCENETREE ADMISSION: GATE D RUNNING…";
            _statusLabel.Text = "STEP 39.0 GATE D — rendering must be frozen; verify the real NGame remains attached to SceneTree.Root with singleton/_window/state authority and zero initializer/rejected/native escape. No cleanup/restart is authorized.";
            var gateD = _transformedRealStS2VeryEarlyInitialization.RunStep39FrozenPostInsertionConfinement(
                renderingStopped && !renderingActiveAfterStop,
                WriteStep39Checkpoint);
            if (!RecordStep39Gate(gateD))
                return;

            var snapshot = _step39Gates.Snapshot();
            _step39ResultLabel.Text = snapshot.Summary;
            _step39ResultLabel.TextColor = UIColor.Label;
            _step39DetailLabel.Text =
                "All four Step 39.0 gates passed. The exact Step-38.2 compatibility authority and four sealed PCK preflight resources were reverified; a fresh real NGame hierarchy was instantiated with NGame.Instance=null; the actual hierarchy's immediate managed lifecycle surface was Cecil-mapped without a direct startup/native/platform escape; the real NGame was added once to the live SceneTree root; automatic NGame._EnterTree/_Ready established the singleton and non-null window authority while GameStartup remained inert; the Godot render loop was synchronously stopped immediately afterward; and final state/native confinement held while the inserted NGame remained in-tree. RemoveChild/Free/_ExitTree/render restart/GameStartup/platform/main-menu/ExecuteDeferred/Steam/native GDExtensions/gameplay remain future boundaries.";
            _statusLabel.Text = "STEP 39.0 COMPLETE — 4/4. Real SceneTree admission is physically closed with rendering frozen. Preserve Step39 artifacts and relaunch before unrelated tests.";
            _statusLabel.TextColor = UIColor.Label;
            WriteStep39Checkpoint("RUN_STEP39_4OF4 — real NGame SceneTree admission and automatic audited lifecycle completed; render loop frozen immediately; inserted NGame retained in-tree; no GameStartup/platform/main-menu/deferred/native boundary crossed.");
        }
        catch (Exception ex)
        {
            WriteStep39Checkpoint($"RUN_MANAGED_EXCEPTION — {ex.GetType().FullName}: {ex.Message}");
            _step39ResultLabel.Text = "REAL SCENETREE ADMISSION: EXCEPTION";
            _step39ResultLabel.TextColor = UIColor.SystemRed;
            _step39DetailLabel.Text = $"Unhandled Step 39.0 exception: {ex.GetType().Name}: {ex.Message}";
            _statusLabel.Text = "STEP 39 FAIL — preserve Step39 artifacts and relaunch before retrying the real SceneTree boundary.";
            _statusLabel.TextColor = UIColor.SystemRed;
        }
        finally
        {
            WriteStep39Checkpoint($"RUN_FINALLY_ENTER — Step-39 managed control reached finally; managedThread={Environment.CurrentManagedThreadId}; isMain={NSThread.IsMain}; renderingActive={GodotStep15NativeBridge.IsRenderingActive}.");
            await WriteDeviceTestReportFromLabelsAsync(
                "Step39-TransformedRealStS2SceneTreeAdmission.txt",
                "StS2 Launcher — Step 39.0 Real SceneTree Admission",
                _step39ResultLabel,
                _step39DetailLabel,
                CancellationToken.None).ConfigureAwait(false);
            WriteStep39Checkpoint("RUN_NORMAL_REPORT_RETURNED — Step39 report writer returned.");
            if (NSThread.IsMain)
                EndSteamOperation();
            else
                InvokeOnMainThread(EndSteamOperation);
            WriteStep39Checkpoint("RUN_END — Step-39 UI operation ended normally after explicit teardown; Step 39 never restarts Godot rendering.");
        }
    }

    private bool RecordStep39Gate(TransformedRealStS2SceneTreeAdmissionGateResult result)
    {
        _step39Gates.Record(result);
        if (_step39ResultLabel is not null)
        {
            _step39ResultLabel.Text = _step39Gates.Snapshot().Summary;
            _step39ResultLabel.TextColor = result.Passed ? UIColor.Label : UIColor.SystemRed;
        }
        if (_step39DetailLabel is not null)
            _step39DetailLabel.Text = result.Detail;
        if (!result.Passed && _statusLabel is not null)
        {
            var letter = (char)('A' + (int)result.Gate - 1);
            _statusLabel.Text = $"STEP 39.0 FAIL at Gate {letter} ({result.Gate}). Stop here; later gates were not run. Preserve Step39 artifacts and relaunch before retrying any insertion boundary.";
            _statusLabel.TextColor = UIColor.SystemRed;
        }
        return result.Passed;
    }

    private bool TryInitializeStep39Telemetry(out string error)
    {
        error = string.Empty;
        lock (_step39CheckpointSync)
        {
            _step39TelemetryReady = false;
            _step39RunId = null;
            _step39CrashCheckpointPath = null;
            _step39LastCheckpointPath = null;
            _step39StaticMapPath = null;
            try
            {
                Directory.CreateDirectory(_deviceTestReportWriter.ReportsRoot);
                var now = DateTimeOffset.UtcNow;
                var runId = $"{now:yyyyMMddTHHmmssfffffffZ}-pid{Environment.ProcessId}-{Guid.NewGuid():N}";
                var crashName = $"Step39-CrashCheckpoint-{runId}.txt";
                var staticName = $"Step39-SceneTreeAdmission-StaticMap-{runId}.txt";
                var crashPath = Path.Combine(_deviceTestReportWriter.ReportsRoot, crashName);
                var lastPath = Path.Combine(_deviceTestReportWriter.ReportsRoot, "Step39-LastCheckpoint.txt");
                var staticPath = Path.Combine(_deviceTestReportWriter.ReportsRoot, staticName);
                WriteStep35TextFileDurably(
                    crashPath,
                    "StS2 Launcher — Step 39.0 real SceneTree admission checkpoint\n" +
                    "Output-only diagnostic; never consumed as trusted runtime input.\n" +
                    $"Run ID: {runId}\n" +
                    $"Initialized UTC: {now:O}\n" +
                    $"Process ID: {Environment.ProcessId}\n" +
                    $"App version: {CurrentReleasePresentation.DisplayVersion} ({CurrentReleasePresentation.DisplayBuild})\n" +
                    "Candidate: STEP 39.0 — PREFLIGHT/HIERARCHY AUDIT + REAL SCENETREE ROOT.ADDCHILD + IMMEDIATE RENDER FREEZE + FROZEN CONFINEMENT\n" +
                    "Prerequisite: fresh process same-process Step-37.0.1 4/4 authority; Step 38 must be skipped in this process. GameStartup/InitializePlatform/main-menu/ExecuteDeferred/Steam/native GDExtensions/explicit _ExitTree remain forbidden.\n\n");
                _step39RunId = runId;
                _step39CrashCheckpointPath = crashPath;
                _step39LastCheckpointPath = lastPath;
                _step39StaticMapPath = staticPath;
                _step39TelemetryReady = true;
                WriteStep39Checkpoint("RUN_TELEMETRY_READY — Step39 run journal created and durably flushed before Gate A.");
                return true;
            }
            catch (Exception ex)
            {
                error = $"{ex.GetType().Name}: {ex.Message}";
                return false;
            }
        }
    }

    private void WriteStep39Checkpoint(string detail)
    {
        if (!_step39TelemetryReady || string.IsNullOrWhiteSpace(_step39RunId) ||
            string.IsNullOrWhiteSpace(_step39CrashCheckpointPath) || string.IsNullOrWhiteSpace(_step39LastCheckpointPath))
            return;
        try
        {
            lock (_step39CheckpointSync)
            {
                var single = (detail ?? string.Empty).Replace('\r', ' ').Replace('\n', ' ');
                var line = $"{DateTimeOffset.UtcNow:O} | run={_step39RunId} | pid={Environment.ProcessId} | managedThread={Environment.CurrentManagedThreadId} | {single}";
                using (var stream = new FileStream(_step39CrashCheckpointPath, FileMode.Append, FileAccess.Write, FileShare.Read, 4096, FileOptions.None))
                using (var writer = new StreamWriter(stream, new UTF8Encoding(false), 1024, leaveOpen: true))
                {
                    writer.WriteLine(line);
                    writer.Flush();
                    stream.Flush(flushToDisk: true);
                }
                WriteStep35TextFileDurably(
                    _step39LastCheckpointPath,
                    "StS2 Launcher — Step 39 last durable checkpoint\n" +
                    "Output-only diagnostic; overwrite-on-each-checkpoint convenience file.\n" +
                    $"Run ID: {_step39RunId}\n" +
                    $"Journal: {Path.GetFileName(_step39CrashCheckpointPath)}\n" +
                    $"Static map: {Path.GetFileName(_step39StaticMapPath)}\n" +
                    line + "\n");
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Step-39 checkpoint append failed: {ex.GetType().Name}: {ex.Message}");
        }
    }

    private bool WriteStep39StaticMap(out string error)
    {
        error = string.Empty;
        try
        {
            if (!_step39TelemetryReady || string.IsNullOrWhiteSpace(_step39StaticMapPath))
                throw new InvalidOperationException("Step39 telemetry/static-map path is not initialized.");
            WriteStep35TextFileDurably(
                _step39StaticMapPath,
                _transformedRealStS2VeryEarlyInitialization.GetVerifiedStep39StaticMap());
            return true;
        }
        catch (Exception ex)
        {
            error = $"{ex.GetType().Name}: {ex.Message}";
            return false;
        }
    }
}
