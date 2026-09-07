using Foundation;
using StS2Launcher.Core;
using System.Text;

namespace StS2Launcher.iOS;

public sealed partial class RootViewController
{
    private readonly TransformedRealStS2GameLifecycleEntryGateSequence _step38Gates = new();
    private UIButton? _step38LifecycleButton;
    private UILabel? _step38ResultLabel;
    private UILabel? _step38DetailLabel;
    private readonly object _step38CheckpointSync = new();
    private string? _step38RunId;
    private string? _step38CrashCheckpointPath;
    private string? _step38LastCheckpointPath;
    private string? _step38StaticMapPath;
    private bool _step38TelemetryReady;

    private void AddTransformedRealStS2GameLifecycleEntryControls(UIStackView content)
    {
        content.AddArrangedSubview(Separator());
        content.AddArrangedSubview(Label(
            "Step 38.0.1 — Controlled NGame._EnterTree entry (still off-tree)",
            UIFont.BoldSystemFontOfSize(18),
            UIColor.Label));

        _step38LifecycleButton = SystemButton(
            "Run Step 38.0.1 A–D — Map lifecycle → Reinstantiate NGame → Invoke _EnterTree once → Confinement/release",
            16);
        _step38LifecycleButton.TouchUpInside += async (_, _) => await RunTransformedRealStS2GameLifecycleEntryAsync();
        content.AddArrangedSubview(_step38LifecycleButton);

        _step38ResultLabel = Label(
            "CONTROLLED NGAME _ENTERTREE ENTRY: NOT RUN",
            UIFont.BoldSystemFontOfSize(15),
            UIColor.SecondaryLabel);
        content.AddArrangedSubview(_step38ResultLabel);

        _step38DetailLabel = Label(
            "Physical 0.0.161 closed Step 37.0.1 at 4/4: the sealed FMOD-neutral game.tscn loaded as PackedScene and instantiated the real managed NGame hierarchy off-tree with zero resolver/native escape. Step 38 keeps the node off-tree. Gate A maps exact NGame lifecycle IL/callsites with Cecil and refuses execution if the _EnterTree same-NGame call closure reaches _Ready, GameStartup, InitializePlatform, LaunchMainMenu, deferred startup, or OneTimeInitialization re-entry. Gate B re-instantiates the proven scene off-tree. Gate C invokes only NGame._EnterTree once by MethodInfo. Gate D proves the node is still off-tree/state=2 with zero initializer-bearing/rejected/native activity, then frees it. No AddChild, _Ready, _ExitTree, GameStartup, InitializePlatform, main-menu, ExecuteDeferred, Steam initialization, native GDExtension loading, or gameplay is authorized.",
            UIFont.SystemFontOfSize(13),
            UIColor.SecondaryLabel);
        content.AddArrangedSubview(_step38DetailLabel);
    }

    private async Task RunTransformedRealStS2GameLifecycleEntryAsync()
    {
        if (_step38LifecycleButton is null || _step38ResultLabel is null || _step38DetailLabel is null || _statusLabel is null)
            return;

        if (!CurrentReleasePresentation.BundleIdentityMatchesExpected)
        {
            _step38ResultLabel.Text = "CONTROLLED NGAME _ENTERTREE ENTRY: RELEASE IDENTITY FAIL";
            _step38ResultLabel.TextColor = UIColor.SystemRed;
            _statusLabel.Text = "STEP 38 REFUSED — built bundle identity does not match the source-pinned candidate.";
            _statusLabel.TextColor = UIColor.SystemRed;
            return;
        }

        if (!_step37Gates.Snapshot().Passed || !_transformedRealStS2VeryEarlyInitialization.ExactStep37ClosurePassed)
        {
            _step38ResultLabel.Text = "CONTROLLED NGAME _ENTERTREE ENTRY: PREREQUISITE NOT MET";
            _step38ResultLabel.TextColor = UIColor.SystemOrange;
            _step38DetailLabel.Text = "Step 38.0.1 requires the same-process physical Step-37.0.1 4/4 closure. From a fresh launch run Step 15 A-C, Step 35.0.32 MODEL-BOOTSTRAP 4/4, Step 36.0.5 4/4, then Step 37.0.1 4/4 before running Step 38 once.";
            _statusLabel.Text = "STEP 38.0.1 REFUSED — same-process Step-37.0.1 4/4 authority is not present.";
            _statusLabel.TextColor = UIColor.SystemOrange;
            return;
        }

        if (!TryInitializeStep38Telemetry(out var telemetryError))
        {
            _step38ResultLabel.Text = "CONTROLLED NGAME _ENTERTREE ENTRY: TELEMETRY FAIL / NOT RUN";
            _step38ResultLabel.TextColor = UIColor.SystemRed;
            _step38DetailLabel.Text = telemetryError;
            _statusLabel.Text = "STEP 38 REFUSED — durable run telemetry could not be established.";
            _statusLabel.TextColor = UIColor.SystemRed;
            return;
        }

        BeginSteamOperation(allowCancel: false);
        _step38Gates.Reset();
        _step38ResultLabel.Text = "CONTROLLED NGAME _ENTERTREE ENTRY: GATE A RUNNING…";
        _step38ResultLabel.TextColor = UIColor.Label;
        _statusLabel.Text = $"STEP 38.0.1 RUN {_step38RunId} — Step 37 is closed. Gate A reads only exact selected sts2 metadata/IL; no lifecycle method is invoked.";
        _statusLabel.TextColor = UIColor.Label;

        try
        {
            WriteStep38Checkpoint("RUN_START — Step 38.0.1 controlled NGame._EnterTree experiment started after same-process Step-37.0.1 4/4 closure.");

            var gateA = _transformedRealStS2VeryEarlyInitialization.RunNGameLifecycleStaticAudit(WriteStep38Checkpoint);
            if (!RecordStep38Gate(gateA))
                return;
            if (!WriteStep38StaticMap(out var staticMapError))
                throw new IOException("Step 38.0.1 verified lifecycle static map could not be durably written: " + staticMapError);
            WriteStep38Checkpoint("H_A_STATIC_MAP_WRITE_RETURNED — verified NGame lifecycle static map durably written.");

            _step38ResultLabel.Text = "CONTROLLED NGAME _ENTERTREE ENTRY: GATE B RUNNING…";
            _statusLabel.Text = "STEP 38.0.1 GATE B — reinstantiate the physically proven FMOD-neutral PackedScene off-tree and bind exact NGame._EnterTree. No lifecycle call yet.";
            WriteStep38Checkpoint($"H_B_UI_SELECTED — Gate B selected on UI thread; managedThread={Environment.CurrentManagedThreadId}; isMain={NSThread.IsMain}.");
            var gateB = _transformedRealStS2VeryEarlyInitialization.RunNGameOffTreeReinstantiation(WriteStep38Checkpoint);
            if (!RecordStep38Gate(gateB))
                return;

            _step38ResultLabel.Text = "CONTROLLED NGAME _ENTERTREE ENTRY: GATE C RUNNING…";
            _statusLabel.Text = "STEP 38.0.1 GATE C — invoke NGame._EnterTree exactly once on the off-tree instance. No AddChild/_Ready/GameStartup/platform/main-menu/deferred authorization.";
            WriteStep38Checkpoint($"H_C_UI_SELECTED — Gate C selected on UI thread; managedThread={Environment.CurrentManagedThreadId}; isMain={NSThread.IsMain}.");
            var gateC = _transformedRealStS2VeryEarlyInitialization.RunNGameDirectEnterTreeInvocation(WriteStep38Checkpoint);
            if (!RecordStep38Gate(gateC))
                return;

            _step38ResultLabel.Text = "CONTROLLED NGAME _ENTERTREE ENTRY: GATE D RUNNING…";
            _statusLabel.Text = "STEP 38.0.1 GATE D — prove post-_EnterTree confinement while still off-tree, then free the temporary node without invoking _ExitTree.";
            var gateD = _transformedRealStS2VeryEarlyInitialization.RunNGamePostEnterTreeConfinementAndRelease(WriteStep38Checkpoint);
            if (!RecordStep38Gate(gateD))
                return;

            var snapshot = _step38Gates.Snapshot();
            _step38ResultLabel.Text = snapshot.Summary;
            _step38ResultLabel.TextColor = UIColor.Label;
            _step38DetailLabel.Text =
                "All four Step 38.0.1 gates passed. Exact lifecycle IL/callsites were mapped from the physically selected compatibility authority; _EnterTree's same-NGame closure contained no later startup/deferred boundary; a fresh real NGame hierarchy was instantiated off-tree; exact NGame._EnterTree returned once while IsInsideTree remained false; and final state/native/resolver confinement held before the node was released. SceneTree insertion, _Ready, _ExitTree, GameStartup, InitializePlatform, LaunchMainMenu, ExecuteDeferred, Steam/native extensions, and gameplay remain future boundaries.";
            _statusLabel.Text = "STEP 38.0.1 COMPLETE — 4/4. Controlled NGame._EnterTree is physically closed; preserve Step38 artifacts before authorizing any real SceneTree/_Ready boundary.";
            _statusLabel.TextColor = UIColor.Label;
            WriteStep38Checkpoint("RUN_STEP38_4OF4 — exact NGame._EnterTree returned once on an off-tree instance and confinement/release completed; no _Ready/GameStartup/platform/main-menu/deferred/native boundary crossed.");
        }
        catch (Exception ex)
        {
            WriteStep38Checkpoint($"RUN_MANAGED_EXCEPTION — {ex.GetType().FullName}: {ex.Message}");
            _step38ResultLabel.Text = "CONTROLLED NGAME _ENTERTREE ENTRY: EXCEPTION";
            _step38ResultLabel.TextColor = UIColor.SystemRed;
            _step38DetailLabel.Text = $"Unhandled Step 38.0.1 exception: {ex.GetType().Name}: {ex.Message}";
            _statusLabel.Text = "STEP 38 FAIL — preserve Step38 artifacts and use a fresh process before retry after Gate C begins.";
            _statusLabel.TextColor = UIColor.SystemRed;
        }
        finally
        {
            WriteStep38Checkpoint($"RUN_FINALLY_ENTER — Step-38 managed control reached finally; managedThread={Environment.CurrentManagedThreadId}; isMain={NSThread.IsMain}.");
            await WriteDeviceTestReportFromLabelsAsync(
                "Step38-TransformedRealStS2NGameLifecycleEntry.txt",
                "StS2 Launcher — Step 38.0.1 Controlled NGame _EnterTree Entry",
                _step38ResultLabel,
                _step38DetailLabel,
                CancellationToken.None).ConfigureAwait(false);
            WriteStep38Checkpoint("RUN_NORMAL_REPORT_RETURNED — Step38 report writer returned.");
            if (NSThread.IsMain)
                EndSteamOperation();
            else
                InvokeOnMainThread(EndSteamOperation);
            WriteStep38Checkpoint("RUN_END — Step-38 UI operation ended normally after explicit teardown.");
        }
    }

    private bool RecordStep38Gate(TransformedRealStS2GameLifecycleEntryGateResult result)
    {
        _step38Gates.Record(result);
        if (_step38ResultLabel is not null)
        {
            _step38ResultLabel.Text = _step38Gates.Snapshot().Summary;
            _step38ResultLabel.TextColor = result.Passed ? UIColor.Label : UIColor.SystemRed;
        }
        if (_step38DetailLabel is not null)
            _step38DetailLabel.Text = result.Detail;
        if (!result.Passed && _statusLabel is not null)
        {
            var letter = (char)('A' + (int)result.Gate - 1);
            _statusLabel.Text = $"STEP 38.0.1 FAIL at Gate {letter} ({result.Gate}). Stop here; later gates were not run. Preserve Step38 artifacts and use a fresh process before retry after Gate C begins.";
            _statusLabel.TextColor = UIColor.SystemRed;
        }
        return result.Passed;
    }

    private bool TryInitializeStep38Telemetry(out string error)
    {
        error = string.Empty;
        lock (_step38CheckpointSync)
        {
            _step38TelemetryReady = false;
            _step38RunId = null;
            _step38CrashCheckpointPath = null;
            _step38LastCheckpointPath = null;
            _step38StaticMapPath = null;
            try
            {
                Directory.CreateDirectory(_deviceTestReportWriter.ReportsRoot);
                var now = DateTimeOffset.UtcNow;
                var runId = $"{now:yyyyMMddTHHmmssfffffffZ}-pid{Environment.ProcessId}-{Guid.NewGuid():N}";
                var crashName = $"Step38-CrashCheckpoint-{runId}.txt";
                var staticName = $"Step38-NGameLifecycle-StaticMap-{runId}.txt";
                var crashPath = Path.Combine(_deviceTestReportWriter.ReportsRoot, crashName);
                var lastPath = Path.Combine(_deviceTestReportWriter.ReportsRoot, "Step38-LastCheckpoint.txt");
                var staticPath = Path.Combine(_deviceTestReportWriter.ReportsRoot, staticName);
                WriteStep35TextFileDurably(
                    crashPath,
                    "StS2 Launcher — Step 38.0.1 controlled NGame._EnterTree checkpoint\n" +
                    "Output-only diagnostic; never consumed as trusted runtime input.\n" +
                    $"Run ID: {runId}\n" +
                    $"Initialized UTC: {now:O}\n" +
                    $"Process ID: {Environment.ProcessId}\n" +
                    $"App version: {CurrentReleasePresentation.DisplayVersion} ({CurrentReleasePresentation.DisplayBuild})\n" +
                    "Candidate: STEP 38.0.1 — LIFECYCLE STATIC MAP + OFF-TREE NGAME REINSTANTIATION + DIRECT _ENTERTREE ONCE + CONFINEMENT/RELEASE\n" +
                    "Prerequisite: same-process Step-37.0.1 4/4 authority; AddChild/_Ready/_ExitTree/GameStartup/InitializePlatform/main-menu/ExecuteDeferred/Steam/native GDExtensions remain forbidden.\n\n");
                _step38RunId = runId;
                _step38CrashCheckpointPath = crashPath;
                _step38LastCheckpointPath = lastPath;
                _step38StaticMapPath = staticPath;
                _step38TelemetryReady = true;
                WriteStep38Checkpoint("RUN_TELEMETRY_READY — Step38 run journal created and durably flushed before Gate A.");
                return true;
            }
            catch (Exception ex)
            {
                error = $"{ex.GetType().Name}: {ex.Message}";
                return false;
            }
        }
    }

    private void WriteStep38Checkpoint(string detail)
    {
        if (!_step38TelemetryReady || string.IsNullOrWhiteSpace(_step38RunId) ||
            string.IsNullOrWhiteSpace(_step38CrashCheckpointPath) || string.IsNullOrWhiteSpace(_step38LastCheckpointPath))
            return;
        try
        {
            lock (_step38CheckpointSync)
            {
                var single = (detail ?? string.Empty).Replace('\r', ' ').Replace('\n', ' ');
                var line = $"{DateTimeOffset.UtcNow:O} | run={_step38RunId} | pid={Environment.ProcessId} | managedThread={Environment.CurrentManagedThreadId} | {single}";
                using (var stream = new FileStream(_step38CrashCheckpointPath, FileMode.Append, FileAccess.Write, FileShare.Read, 4096, FileOptions.None))
                using (var writer = new StreamWriter(stream, new UTF8Encoding(false), 1024, leaveOpen: true))
                {
                    writer.WriteLine(line);
                    writer.Flush();
                    stream.Flush(flushToDisk: true);
                }
                WriteStep35TextFileDurably(
                    _step38LastCheckpointPath,
                    "StS2 Launcher — Step 38 last durable checkpoint\n" +
                    "Output-only diagnostic; overwrite-on-each-checkpoint convenience file.\n" +
                    $"Run ID: {_step38RunId}\n" +
                    $"Journal: {Path.GetFileName(_step38CrashCheckpointPath)}\n" +
                    $"Static map: {Path.GetFileName(_step38StaticMapPath)}\n" +
                    line + "\n");
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Step-38 checkpoint append failed: {ex.GetType().Name}: {ex.Message}");
        }
    }

    private bool WriteStep38StaticMap(out string error)
    {
        error = string.Empty;
        try
        {
            if (!_step38TelemetryReady || string.IsNullOrWhiteSpace(_step38StaticMapPath))
                throw new InvalidOperationException("Step38 telemetry/static-map path is not initialized.");
            WriteStep35TextFileDurably(
                _step38StaticMapPath,
                _transformedRealStS2VeryEarlyInitialization.GetVerifiedNGameLifecycleStaticMap());
            return true;
        }
        catch (Exception ex)
        {
            error = $"{ex.GetType().Name}: {ex.Message}";
            return false;
        }
    }
}
