using Foundation;
using StS2Launcher.Core;
using StS2Launcher.iOS.Platform;
using System.Text;

namespace StS2Launcher.iOS;

public sealed partial class RootViewController
{
    private readonly TransformedRealStS2GameStartupInitPoolsGateSequence _step42Gates = new();
    private UIButton? _step42InitPoolsButton;
    private UILabel? _step42ResultLabel;
    private UILabel? _step42DetailLabel;
    private readonly object _step42CheckpointSync = new();
    private string? _step42RunId;
    private string? _step42CrashCheckpointPath;
    private string? _step42LastCheckpointPath;
    private string? _step42StaticMapPath;
    private bool _step42TelemetryReady;
    private bool _step42InvocationUiStarted;

    private void AddTransformedRealStS2GameStartupInitPoolsControls(UIStackView content)
    {
        content.AddArrangedSubview(Separator());
        content.AddArrangedSubview(Label(
            "Step 42.0 — controlled GameStartup InitPools invocation",
            UIFont.BoldSystemFontOfSize(18),
            UIColor.Label));

        _step42InitPoolsButton = SystemButton(
            "Run Step 42.0 A–D — Verify Step 41 → Audit InitPools closure → Invoke once → Frozen confinement",
            16);
        _step42InitPoolsButton.TouchUpInside += async (_, _) => await RunTransformedRealStS2GameStartupInitPoolsAsync();
        content.AddArrangedSubview(_step42InitPoolsButton);

        _step42ResultLabel = Label(
            "CONTROLLED GAMESTARTUP INITPOOLS: NOT RUN",
            UIFont.BoldSystemFontOfSize(15),
            UIColor.SecondaryLabel);
        content.AddArrangedSubview(_step42ResultLabel);

        _step42DetailLabel = Label(
            "Physical 0.0.173 closed Step 41 at 4/4: exact GameStartup/MoveNext mapped 691 same-sts2 methods and 46 startup/platform boundaries with zero unresolved/external Cecil resolution while GameStartup remained uninvoked. Step 42.0 keeps rendering frozen and GameStartupWrapper inert. Gate A re-verifies same-process Step-41 authority. Gate B maps exact NGame.InitPools() and its transitive same-sts2 closure; any platform/Steam/main-menu/deferred/FMOD/Spine/Sentry/native/OneTimeInitialization edge fails before invocation. The verified static map is durably written before Gate C. Gate C invokes exact audited InitPools once by MethodInfo and requires zero resolver/host/private/initializer/rejected/native deltas with state 2 preserved. Gate D proves frozen NGame confinement. GameStartup itself, migrations, cloud sync, platform, Steam, main menu, deferred startup, and rendering remain unopened.",
            UIFont.SystemFontOfSize(13),
            UIColor.SecondaryLabel);
        content.AddArrangedSubview(_step42DetailLabel);
    }

    private async Task RunTransformedRealStS2GameStartupInitPoolsAsync()
    {
        if (_step42InitPoolsButton is null || _step42ResultLabel is null || _step42DetailLabel is null || _statusLabel is null)
            return;

        if (_step42InvocationUiStarted)
        {
            _step42ResultLabel.Text = "CONTROLLED GAMESTARTUP INITPOOLS: ONE-SHOT ALREADY ARMED";
            _step42ResultLabel.TextColor = UIColor.SystemOrange;
            _step42DetailLabel.Text = "Step 42.0 crossed or attempted the InitPools invocation boundary in this process. Preserve Step42 reports and relaunch; never retry InitPools in-process.";
            _statusLabel.Text = "STEP 42.0 REFUSED — one-shot invocation boundary already armed. Relaunch.";
            _statusLabel.TextColor = UIColor.SystemOrange;
            return;
        }

        if (!CurrentReleasePresentation.BundleIdentityMatchesExpected)
        {
            _step42ResultLabel.Text = "CONTROLLED GAMESTARTUP INITPOOLS: RELEASE IDENTITY FAIL";
            _step42ResultLabel.TextColor = UIColor.SystemRed;
            _statusLabel.Text = "STEP 42 REFUSED — built bundle identity does not match the source-pinned candidate.";
            _statusLabel.TextColor = UIColor.SystemRed;
            return;
        }

        if (!_step41Gates.Snapshot().Passed || !_transformedRealStS2VeryEarlyInitialization.ExactStep41ClosurePassed)
        {
            _step42ResultLabel.Text = "CONTROLLED GAMESTARTUP INITPOOLS: STEP 41 PREREQUISITE NOT MET";
            _step42ResultLabel.TextColor = UIColor.SystemOrange;
            _step42DetailLabel.Text = "Step 42.0 requires Step 41.0 4/4 in this same process with rendering still frozen. Fresh launch sequence: Step 15 A-C → Step 35.0.32 4/4 → Step 36.0.5 4/4 → Step 37.0.1 4/4 → skip Step 38 → Step 39.0 4/4 → Step 40.1 4/4 → Step 41.0 4/4 → Step 42.0 once.";
            _statusLabel.Text = "STEP 42.0 REFUSED — complete Step 41.0 4/4 in this process first.";
            _statusLabel.TextColor = UIColor.SystemOrange;
            return;
        }

        if (GodotStep15NativeBridge.IsRenderingActive)
        {
            _step42ResultLabel.Text = "CONTROLLED GAMESTARTUP INITPOOLS: RENDERER NOT FROZEN";
            _step42ResultLabel.TextColor = UIColor.SystemRed;
            _step42DetailLabel.Text = "Step 42.0 requires rendering to remain stopped. No render restart is authorized by this step.";
            _statusLabel.Text = "STEP 42.0 REFUSED — rendering is active.";
            _statusLabel.TextColor = UIColor.SystemRed;
            return;
        }

        if (!TryInitializeStep42Telemetry(out var telemetryError))
        {
            _step42ResultLabel.Text = "CONTROLLED GAMESTARTUP INITPOOLS: TELEMETRY FAIL / NOT RUN";
            _step42ResultLabel.TextColor = UIColor.SystemRed;
            _step42DetailLabel.Text = telemetryError;
            _statusLabel.Text = "STEP 42 REFUSED — durable run telemetry could not be established.";
            _statusLabel.TextColor = UIColor.SystemRed;
            return;
        }

        BeginSteamOperation(allowCancel: false);
        _step42Gates.Reset();
        _step42ResultLabel.Text = "CONTROLLED GAMESTARTUP INITPOOLS: GATE A RUNNING…";
        _step42ResultLabel.TextColor = UIColor.Label;
        _statusLabel.Text = $"STEP 42.0 RUN {_step42RunId} — Step 41 is closed in-process and rendering is frozen. InitPools has not been invoked.";
        _statusLabel.TextColor = UIColor.Label;

        try
        {
            WriteStep42Checkpoint("RUN_START — Step 42.0 controlled GameStartup InitPools boundary started from same-process Step-41 4/4 frozen authority. InitPools has not been invoked and rendering remains stopped.");

            var gateA = _transformedRealStS2VeryEarlyInitialization.RunStep42ClosedStep41FrozenAuthority(
                !GodotStep15NativeBridge.IsRenderingActive,
                WriteStep42Checkpoint);
            if (!RecordStep42Gate(gateA))
                return;

            _step42ResultLabel.Text = "CONTROLLED GAMESTARTUP INITPOOLS: GATE B RUNNING…";
            _statusLabel.Text = "STEP 42.0 GATE B — audit exact InitPools same-sts2 closure with rendering frozen; fail before invocation on any classified or unresolved boundary.";
            var gateB = _transformedRealStS2VeryEarlyInitialization.RunStep42InitPoolsStaticClosureAudit(WriteStep42Checkpoint);
            if (!RecordStep42Gate(gateB))
                return;
            if (!WriteStep42StaticMap(out var staticMapError))
                throw new IOException("Step 42.0 verified InitPools static map could not be durably written before invocation: " + staticMapError);
            WriteStep42Checkpoint("L_B_STATIC_MAP_WRITE_RETURNED — verified Step-42 InitPools direct IL + transitive zero-boundary closure map durably written before invocation; rendering still stopped.");

            _step42InvocationUiStarted = true;
            _step42InitPoolsButton.Enabled = false;
            _step42ResultLabel.Text = "CONTROLLED GAMESTARTUP INITPOOLS: GATE C RUNNING…";
            _statusLabel.Text = "STEP 42.0 GATE C — invoking exact audited NGame.InitPools once. From this point preserve reports and relaunch; never retry in-process.";
            WriteStep42Checkpoint("L_C_UI_ARMED — first and only Step-42 InitPools invocation authorized. No retry is permitted in this process after this checkpoint.");
            var gateC = _transformedRealStS2VeryEarlyInitialization.RunStep42ControlledInitPoolsInvocation(WriteStep42Checkpoint);
            if (!RecordStep42Gate(gateC))
                return;

            _step42ResultLabel.Text = "CONTROLLED GAMESTARTUP INITPOOLS: GATE D RUNNING…";
            _statusLabel.Text = "STEP 42.0 GATE D — prove frozen post-InitPools NGame/state/native confinement; do not restart rendering or enter later startup work.";
            var gateD = _transformedRealStS2VeryEarlyInitialization.RunStep42FrozenPostInitPoolsConfinement(
                !GodotStep15NativeBridge.IsRenderingActive,
                WriteStep42Checkpoint);
            if (!RecordStep42Gate(gateD))
                return;

            var snapshot = _step42Gates.Snapshot();
            _step42ResultLabel.Text = snapshot.Summary;
            _step42ResultLabel.TextColor = UIColor.Label;
            _step42DetailLabel.Text =
                "All four Step 42.0 gates passed. Same-process Step-41 frozen authority was reverified; exact NGame.InitPools and its transitive same-sts2 closure were mapped with zero classified/unresolved/external Cecil boundaries; the verified map was durably written before execution; exact InitPools returned once on the retained real NGame; and final frozen singleton/parent/_window/state plus resolver/initializer/rejected/native confinement held. GameStartup itself, migrations/cloud/platform/Steam/main-menu/deferred/render restart remain future separately authorized boundaries.";
            _statusLabel.Text = "STEP 42.0 COMPLETE — 4/4. Exact audited InitPools executed once with frozen zero-native confinement. Step 43 is now unlocked in this same process; do not retry Step 42.";
            _statusLabel.TextColor = UIColor.Label;
            WriteStep42Checkpoint("RUN_STEP42_4OF4 — exact audited NGame.InitPools executed once and returned; renderer remained frozen; inserted NGame retained; GameStartup/migrations/cloud/platform/Steam/main-menu/deferred/native boundaries remain unopened.");
        }
        catch (Exception ex)
        {
            WriteStep42Checkpoint($"RUN_MANAGED_EXCEPTION — {ex.GetType().FullName}: {SanitizeStep42Checkpoint(ex.Message)}");
            _step42ResultLabel.Text = "CONTROLLED GAMESTARTUP INITPOOLS: EXCEPTION";
            _step42ResultLabel.TextColor = UIColor.SystemRed;
            _step42DetailLabel.Text = $"Unhandled Step 42.0 exception: {ex.GetType().Name}: {ex.Message}";
            _statusLabel.Text = _step42InvocationUiStarted
                ? "STEP 42 EXCEPTION AFTER INVOCATION ARM — preserve Step42 reports and relaunch; never retry in-process."
                : "STEP 42 EXCEPTION BEFORE INVOCATION — preserve Step42 reports.";
            _statusLabel.TextColor = UIColor.SystemRed;
        }
        finally
        {
            WriteStep42Checkpoint($"RUN_FINALLY_ENTER — Step-42 managed control reached finally; managedThread={Environment.CurrentManagedThreadId}; isMain={NSThread.IsMain}; renderingActive={GodotStep15NativeBridge.IsRenderingActive}; invocationUiStarted={_step42InvocationUiStarted}.");
            await WriteDeviceTestReportFromLabelsAsync(
                "Step42-TransformedRealStS2GameStartupInitPools.txt",
                "StS2 Launcher — Step 42.0 Controlled GameStartup InitPools",
                _step42ResultLabel,
                _step42DetailLabel,
                CancellationToken.None).ConfigureAwait(false);
            WriteStep42Checkpoint("RUN_NORMAL_REPORT_RETURNED — Step42 report writer returned.");
            if (NSThread.IsMain)
                EndSteamOperation();
            else
                InvokeOnMainThread(EndSteamOperation);
            WriteStep42Checkpoint("RUN_END — Step-42 UI operation ended normally; rendering was never restarted. If the InitPools boundary was armed, this process must not retry Step 42.");
        }
    }

    private bool RecordStep42Gate(TransformedRealStS2GameStartupInitPoolsGateResult result)
    {
        _step42Gates.Record(result);
        if (_step42ResultLabel is not null)
        {
            _step42ResultLabel.Text = _step42Gates.Snapshot().Summary;
            _step42ResultLabel.TextColor = result.Passed ? UIColor.Label : UIColor.SystemRed;
        }
        if (_step42DetailLabel is not null)
            _step42DetailLabel.Text = result.Detail;
        if (!result.Passed && _statusLabel is not null)
        {
            var letter = (char)('A' + (int)result.Gate - 1);
            _statusLabel.Text = _step42InvocationUiStarted
                ? $"STEP 42.0 FAIL at Gate {letter} ({result.Gate}) after invocation arm. Stop; preserve reports and relaunch."
                : $"STEP 42.0 FAIL at Gate {letter} ({result.Gate}) before invocation. Stop; later gates were not run.";
            _statusLabel.TextColor = UIColor.SystemRed;
        }
        return result.Passed;
    }

    private bool TryInitializeStep42Telemetry(out string error)
    {
        error = string.Empty;
        lock (_step42CheckpointSync)
        {
            _step42TelemetryReady = false;
            _step42RunId = null;
            _step42CrashCheckpointPath = null;
            _step42LastCheckpointPath = null;
            _step42StaticMapPath = null;
            try
            {
                Directory.CreateDirectory(_deviceTestReportWriter.ReportsRoot);
                var now = DateTimeOffset.UtcNow;
                var runId = $"{now:yyyyMMddTHHmmssfffffffZ}-pid{Environment.ProcessId}-{Guid.NewGuid():N}";
                var crashName = $"Step42-CrashCheckpoint-{runId}.txt";
                var staticName = $"Step42-InitPools-StaticMap-{runId}.txt";
                var crashPath = Path.Combine(_deviceTestReportWriter.ReportsRoot, crashName);
                var lastPath = Path.Combine(_deviceTestReportWriter.ReportsRoot, "Step42-LastCheckpoint.txt");
                var staticPath = Path.Combine(_deviceTestReportWriter.ReportsRoot, staticName);
                WriteStep35TextFileDurably(
                    crashPath,
                    "StS2 Launcher — Step 42.0 controlled GameStartup InitPools checkpoint\n" +
                    "Output-only diagnostic; never consumed as trusted runtime input.\n" +
                    $"Run ID: {runId}\n" +
                    $"Initialized UTC: {now:O}\n" +
                    $"Process ID: {Environment.ProcessId}\n" +
                    $"App version: {CurrentReleasePresentation.DisplayVersion} ({CurrentReleasePresentation.DisplayBuild})\n" +
                    "Candidate: STEP 42.0 — CLOSED STEP-41 AUTHORITY + EXACT INITPOOLS CLOSURE AUDIT + ONE CONTROLLED INITPOOLS INVOCATION + FROZEN CONFINEMENT\n" +
                    "Prerequisite: same-process Step-41 4/4 with real NGame retained in-tree and renderer stopped. GameStartup/migrations/cloud/platform/Steam/main-menu/ExecuteDeferred/render restart/native game GDExtension execution remain forbidden.\n\n");
                _step42RunId = runId;
                _step42CrashCheckpointPath = crashPath;
                _step42LastCheckpointPath = lastPath;
                _step42StaticMapPath = staticPath;
                _step42TelemetryReady = true;
                WriteStep42Checkpoint("RUN_TELEMETRY_READY — Step42 run journal created and durably flushed before Gate A.");
                return true;
            }
            catch (Exception ex)
            {
                error = $"{ex.GetType().Name}: {ex.Message}";
                return false;
            }
        }
    }

    private void WriteStep42Checkpoint(string detail)
    {
        if (!_step42TelemetryReady || string.IsNullOrWhiteSpace(_step42RunId) ||
            string.IsNullOrWhiteSpace(_step42CrashCheckpointPath) || string.IsNullOrWhiteSpace(_step42LastCheckpointPath))
            return;
        try
        {
            lock (_step42CheckpointSync)
            {
                var single = SanitizeStep42Checkpoint(detail);
                var line = $"{DateTimeOffset.UtcNow:O} | run={_step42RunId} | pid={Environment.ProcessId} | managedThread={Environment.CurrentManagedThreadId} | {single}";
                using (var stream = new FileStream(_step42CrashCheckpointPath, FileMode.Append, FileAccess.Write, FileShare.Read, 4096, FileOptions.None))
                using (var writer = new StreamWriter(stream, new UTF8Encoding(false), 1024, leaveOpen: true))
                {
                    writer.WriteLine(line);
                    writer.Flush();
                    stream.Flush(flushToDisk: true);
                }
                WriteStep35TextFileDurably(
                    _step42LastCheckpointPath,
                    "StS2 Launcher — Step 42 last durable checkpoint\n" +
                    "Output-only diagnostic; overwrite-on-each-checkpoint convenience file.\n" +
                    $"Run ID: {_step42RunId}\n" +
                    $"Journal: {Path.GetFileName(_step42CrashCheckpointPath)}\n" +
                    $"Static map: {Path.GetFileName(_step42StaticMapPath)}\n" +
                    line + "\n");
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Step-42 checkpoint append failed: {ex.GetType().Name}: {ex.Message}");
        }
    }

    private bool WriteStep42StaticMap(out string error)
    {
        error = string.Empty;
        try
        {
            if (!_step42TelemetryReady || string.IsNullOrWhiteSpace(_step42StaticMapPath))
                throw new InvalidOperationException("Step42 telemetry/static-map path is not initialized.");
            WriteStep35TextFileDurably(
                _step42StaticMapPath,
                _transformedRealStS2VeryEarlyInitialization.GetVerifiedStep42StaticMap());
            return true;
        }
        catch (Exception ex)
        {
            error = $"{ex.GetType().Name}: {ex.Message}";
            return false;
        }
    }

    private static string SanitizeStep42Checkpoint(string? detail)
        => (detail ?? string.Empty).Replace('\r', ' ').Replace('\n', ' ');
}
