using Foundation;
using StS2Launcher.Core;
using System.Text;

namespace StS2Launcher.iOS;

public sealed partial class RootViewController
{
    private readonly TransformedRealStS2EssentialInitializationGateSequence _step36Gates = new();
    private UIButton? _step36EssentialButton;
    private UILabel? _step36ResultLabel;
    private UILabel? _step36DetailLabel;
    private UILabel? _step36ProgressLabel;
    private readonly object _step36CheckpointSync = new();
    private string? _step36RunId;
    private string? _step36CrashCheckpointPath;
    private string? _step36LastCheckpointPath;
    private string? _step36StaticMapPath;
    private bool _step36TelemetryReady;

    private void AddTransformedRealStS2EssentialInitializationControls(UIStackView content)
    {
        content.AddArrangedSubview(Separator());
        content.AddArrangedSubview(Label(
            "Step 36.0.3 — ModelDb Bootstrap Compatibility + Full ExecuteEssential Attempt",
            UIFont.BoldSystemFontOfSize(18),
            UIColor.Label));

        _step36EssentialButton = SystemButton(
            "Run Step 36.0.3 A–D — Reprove Compatibility Plan → Mount PCK → Invoke Full ExecuteEssential Once → Audit",
            16);
        _step36EssentialButton.TouchUpInside += async (_, _) => await RunTransformedRealStS2EssentialInitializationAsync();
        content.AddArrangedSubview(_step36EssentialButton);

        _step36ResultLabel = Label(
            "TRANSFORMED REAL STS2 ESSENTIAL INITIALIZATION: NOT RUN",
            UIFont.BoldSystemFontOfSize(15),
            UIColor.SecondaryLabel);
        content.AddArrangedSubview(_step36ResultLabel);

        _step36DetailLabel = Label(
            "Physical 0.0.156 proves PCK/localization are closed and localizes the next failure to ModelDb.Init: Activator.CreateInstance(BowlbugsNormal) triggers its static initializer before BowlbugEgg reaches the generated ModelDb insertion point, producing KeyNotFound MONSTER.BOWLBUG_EGG. Step 36.0.3 derives a bounded sts2 compatibility authority from the exact Step-32 image: it statically discovers ModelDb<T> dependencies in generated model static initializers, pre-injects only the dependency closure needed for backward edges, then removes/re-adds the same instances at their original generated-list positions so final ModelDb insertion order is preserved. It then invokes the full unchanged ExecuteEssential once, allowing ModelIdSerializationCache.Init, ModelDb.InitIds, MessageTypes.Initialize, and ActionTypes.Initialize to run naturally if ModelDb succeeds. Full nested failure telemetry remains enabled. ExecuteDeferred, PrewarmJit, game entry, native game loading, runtime Harmony/MonoMod, arbitrary resolver fallback, retry, and state reset remain forbidden.",
            UIFont.SystemFontOfSize(13),
            UIColor.SecondaryLabel);
        content.AddArrangedSubview(_step36DetailLabel);

        _step36ProgressLabel = Label(
            "Step 36 Gate-D progress will appear here.",
            UIFont.SystemFontOfSize(12),
            UIColor.SecondaryLabel);
        _step36ProgressLabel.Hidden = true;
        content.AddArrangedSubview(_step36ProgressLabel);
    }

    private async Task RunTransformedRealStS2EssentialInitializationAsync()
    {
        if (_step36EssentialButton is null || _step36ResultLabel is null || _step36DetailLabel is null || _step36ProgressLabel is null || _statusLabel is null)
            return;

        if (!CurrentReleasePresentation.BundleIdentityMatchesExpected)
        {
            _step36ResultLabel.Text = "TRANSFORMED REAL STS2 ESSENTIAL INITIALIZATION: RELEASE IDENTITY FAIL";
            _step36ResultLabel.TextColor = UIColor.SystemRed;
            _statusLabel.Text = "STEP 36 REFUSED — built bundle identity does not match the source-pinned candidate.";
            _statusLabel.TextColor = UIColor.SystemRed;
            return;
        }

        if (!_transformedRealStS2VeryEarlyInitialization.EssentialCompatibilityAuthorityPassed ||
            _transformedRealStS2VeryEarlyInitialization.DiagnosticMode != Step35DiagnosticMode.GodotCoreModelBootstrapCompatibility)
        {
            _step36ResultLabel.Text = "TRANSFORMED REAL STS2 ESSENTIAL INITIALIZATION: PREREQUISITE NOT MET";
            _step36ResultLabel.TextColor = UIColor.SystemOrange;
            _step36DetailLabel.Text = "Step 36.0.3 requires the same-process MODEL-BOOTSTRAP Step-35 authority. From a fresh launch run Step 15 Gates A-C, then Step 35.0.32 MODEL-BOOTSTRAP once and require 4/4. That mode loads only the verified dependency-aware sts2 derivative with exact prepared GodotSharp; do not run historical EXACT-CLOSURE first because sts2 CLR identity is process-global for this experiment.";
            _statusLabel.Text = "STEP 36.0.3 REFUSED — same-process MODEL-BOOTSTRAP authority is not present.";
            _statusLabel.TextColor = UIColor.SystemOrange;
            return;
        }

        if (!TryInitializeStep36Telemetry(out var telemetryError))
        {
            _step36ResultLabel.Text = "TRANSFORMED REAL STS2 ESSENTIAL INITIALIZATION: TELEMETRY FAIL / NOT RUN";
            _step36ResultLabel.TextColor = UIColor.SystemRed;
            _step36DetailLabel.Text = telemetryError;
            _statusLabel.Text = "STEP 36 REFUSED — durable run telemetry could not be established.";
            _statusLabel.TextColor = UIColor.SystemRed;
            return;
        }

        BeginSteamOperation(allowCancel: true);
        _step36Gates.Reset();
        _step36ProgressLabel.Hidden = true;
        _step36ResultLabel.Text = "TRANSFORMED REAL STS2 ESSENTIAL INITIALIZATION: GATE A RUNNING…";
        _step36ResultLabel.TextColor = UIColor.Label;
        _statusLabel.Text = $"STEP 36.0.3 RUN {_step36RunId} — MODEL-BOOTSTRAP authority is resident. Gate A re-proves the exact source ExecuteEssential semantics against the selected compatibility derivative and emits the dependency/order plan before any new game-member invocation.";
        _statusLabel.TextColor = UIColor.Label;

        try
        {
            WriteStep36Checkpoint("RUN_START — Step 36.0.3 full ExecuteEssential run started after same-process MODEL-BOOTSTRAP Step-35 authority closure.");
            var token = _operationCts?.Token ?? CancellationToken.None;

            var gateA = await Task.Run(() => _transformedRealStS2VeryEarlyInitialization.RunEssentialStaticPreflight(WriteStep36Checkpoint), token);
            WriteStep36Checkpoint($"E_A_TASK_AWAIT_RESUMED — Gate A returned to UI caller; passed={gateA.Passed}.");
            if (!RecordStep36Gate(gateA)) return;
            if (!WriteStep36StaticMap(out var staticMapError))
                throw new IOException("Step 36.0.3 verified static map could not be durably written: " + staticMapError);
            WriteStep36Checkpoint("E_A_STATIC_MAP_WRITE_RETURNED — verified ExecuteEssential static map durably written.");

            _step36ResultLabel.Text = "TRANSFORMED REAL STS2 ESSENTIAL INITIALIZATION: GATE B RUNNING…";
            _statusLabel.Text = "STEP 36.0.3 GATE B — prove MODEL-BOOTSTRAP CLR authority continuity, bind unchanged ExecuteEssential, mount the receipt-backed game PCK additively through exact GodotSharp, and prove res://localization/eng before invocation.";
            var gateB = _transformedRealStS2VeryEarlyInitialization.RunEssentialAuthorityBinding(WriteStep36Checkpoint);
            if (!RecordStep36Gate(gateB)) return;

            _step36ResultLabel.Text = "TRANSFORMED REAL STS2 ESSENTIAL INITIALIZATION: GATE C RUNNING…";
            _statusLabel.Text = "STEP 36.0.3 GATE C — invoke unchanged ExecuteEssential once on the main thread from the verified ModelDb-bootstrap authority. If ModelDb succeeds, the remaining serialization/type/action initializers run in their original order in this same call. This is synchronous and one-shot; do not retry in this process if it fails or stalls.";
            WriteStep36Checkpoint("E_C_UI_SELECTED — Gate C selected on the UI thread; entering the synchronous unchanged ExecuteEssential boundary on the verified ModelDb-bootstrap authority.");
            var gateC = _transformedRealStS2VeryEarlyInitialization.RunExactExecuteEssentialInvocation(WriteStep36Checkpoint);
            if (!RecordStep36Gate(gateC)) return;

            _step36ResultLabel.Text = "TRANSFORMED REAL STS2 ESSENTIAL INITIALIZATION: GATE D RUNNING…";
            _step36ProgressLabel.Hidden = false;
            _step36ProgressLabel.Text = "Step 36 Gate D starting final receipt/hash/resolver/context audit…";
            _statusLabel.Text = "STEP 36.0.3 GATE D — final OfflineReady + mounted-PCK presence + exact-source/compatibility-authority/plan/dependency/resolver/context/state reproof.";
            var progress = new Progress<TransformedRealStS2EssentialInitializationProgress>(value =>
            {
                if (_step36ProgressLabel is null) return;
                if (value.TotalBytes > 0)
                {
                    var pct = value.TotalBytes == 0 ? 0d : (double)value.ProcessedBytes / value.TotalBytes * 100d;
                    _step36ProgressLabel.Text = $"Step 36 Gate D receipt verification — {pct:0.0}% • {value.ProcessedItems:N0}/{value.TotalItems:N0} files";
                }
                else
                {
                    _step36ProgressLabel.Text = $"Step 36 Gate D — {value.ProcessedItems:N0}/{value.TotalItems:N0}: {value.Detail}";
                }
            });
            WriteStep36Checkpoint("E_D_WORKER_SCHEDULE — scheduling Step-36 Gate D behind an outer Task.Run completion boundary; completion continuation explicitly avoids UIKit SynchronizationContext.");
            var gateD = await Task.Run(async () =>
            {
                var result = await _transformedRealStS2VeryEarlyInitialization
                    .RunEssentialFinalIsolationAuditAsync(progress, token, WriteStep36Checkpoint)
                    .ConfigureAwait(false);
                WriteStep36Checkpoint($"E_D_WORKER_RETURN — outer Step-36 Gate-D worker observed result; passed={result.Passed}.");
                return result;
            }, token).ConfigureAwait(false);
            WriteStep36Checkpoint($"E_D_THREADPOOL_CONTINUATION — outer Step-36 Gate-D Task completed without recapturing UIKit SynchronizationContext; passed={gateD.Passed}; managedThread={Environment.CurrentManagedThreadId}; isMain={NSThread.IsMain}.");

            var gateDRecorded = false;
            void FinalizeStep36GateDOnMainThread()
            {
                WriteStep36Checkpoint($"E_D_UI_DISPATCH_ENTER — explicit main-thread Step-36 Gate-D finalization entered; managedThread={Environment.CurrentManagedThreadId}; isMain={NSThread.IsMain}.");
                gateDRecorded = RecordStep36Gate(gateD);
                if (gateDRecorded)
                {
                    WriteStep36Checkpoint("E_D_RESULT_RECORD_PASS — Step-36 Gate-D PASS recorded on explicit main-thread dispatch.");
                    var snapshot = _step36Gates.Snapshot();
                    _step36ResultLabel.Text = snapshot.Summary;
                    _step36ResultLabel.TextColor = UIColor.Label;
                    _step36DetailLabel.Text =
                        "All four Step 36.0.3 gates passed. The selected dependency-aware ModelDb bootstrap derivative retained source-equivalent ExecuteEssential, the receipt-backed PCK/localization boundary remained positive, unchanged ExecuteEssential returned normally with OneTimeInitialization state 1→2, and the original post-ModelDb sequence (ModelIdSerializationCache.Init → ModelDb.InitIds → MessageTypes.Initialize → ActionTypes.Initialize) therefore also returned in the same call. Final OfflineReady/hash/resolver/context reproof passed. ExecuteDeferred, PrewarmJit, game entry, and native game loading remain separate future gates.";
                    _step36ProgressLabel.Text = "Step 36.0.3 Gate D COMPLETE — PASS";
                    _statusLabel.Text = "STEP 36.0.3 COMPLETE — 4/4. Essential initialization is closed under the bounded ModelDb compatibility authority; preserve artifacts before designing deferred initialization.";
                    _statusLabel.TextColor = UIColor.Label;
                    WriteStep36Checkpoint("RUN_STEP36_4OF4 — bounded ModelDb compatibility authority completed unchanged ExecuteEssential and all four Step-36 gates; ExecuteDeferred and later startup remain forbidden.");
                }
                else
                {
                    WriteStep36Checkpoint("E_D_RESULT_RECORD_FAIL_STOP — Step-36 Gate-D result recorded as FAIL on explicit main-thread dispatch.");
                }
                WriteStep36Checkpoint("E_D_UI_DISPATCH_RETURN — explicit main-thread Step-36 Gate-D finalization completed.");
            }

            if (NSThread.IsMain)
                FinalizeStep36GateDOnMainThread();
            else
                InvokeOnMainThread(FinalizeStep36GateDOnMainThread);

            if (!gateDRecorded)
                return;
        }
        catch (OperationCanceledException)
        {
            WriteStep36Checkpoint("RUN_CANCELLED_INCONCLUSIVE — Step 36 cancellation is not a compatibility verdict; if Gate C began, force-quit before retry.");
            void ApplyCancelledUi()
            {
                _step36ResultLabel.Text = "TRANSFORMED REAL STS2 ESSENTIAL INITIALIZATION: CANCELLED / INCONCLUSIVE";
                _step36ResultLabel.TextColor = UIColor.SecondaryLabel;
                _statusLabel.Text = "STEP 36 CANCELLED / INCONCLUSIVE — force-quit before retry if Gate C started.";
                _statusLabel.TextColor = UIColor.SecondaryLabel;
            }
            if (NSThread.IsMain) ApplyCancelledUi(); else InvokeOnMainThread(ApplyCancelledUi);
        }
        catch (Exception ex)
        {
            WriteStep36Checkpoint($"RUN_MANAGED_EXCEPTION — {ex.GetType().FullName}: {ex.Message}");
            void ApplyExceptionUi()
            {
                _step36ResultLabel.Text = "TRANSFORMED REAL STS2 ESSENTIAL INITIALIZATION: EXCEPTION";
                _step36ResultLabel.TextColor = UIColor.SystemRed;
                _step36DetailLabel.Text = $"Unhandled Step 36.0.3 exception: {ex.GetType().Name}: {ex.Message}";
                _statusLabel.Text = "STEP 36 FAIL — preserve Step36 artifacts; force-quit before retry if Gate C started.";
                _statusLabel.TextColor = UIColor.SystemRed;
            }
            if (NSThread.IsMain) ApplyExceptionUi(); else InvokeOnMainThread(ApplyExceptionUi);
        }
        finally
        {
            WriteStep36Checkpoint($"RUN_FINALLY_ENTER — Step-36 managed control reached finally; managedThread={Environment.CurrentManagedThreadId}; isMain={NSThread.IsMain}; writing deterministic report without requiring a captured UIKit continuation.");
            await WriteDeviceTestReportFromLabelsAsync(
                "Step36-TransformedRealStS2EssentialInitialization.txt",
                "StS2 Launcher — Step 36.0.3 ModelDb Bootstrap Compatibility + Full ExecuteEssential",
                _step36ResultLabel,
                _step36DetailLabel,
                CancellationToken.None).ConfigureAwait(false);
            WriteStep36Checkpoint("RUN_NORMAL_REPORT_RETURNED — Step36 report writer returned.");
            if (NSThread.IsMain)
                EndSteamOperation();
            else
                InvokeOnMainThread(EndSteamOperation);
            WriteStep36Checkpoint("RUN_END — Step-36 UI operation ended normally after explicit main-thread teardown.");
        }
    }

    private bool RecordStep36Gate(TransformedRealStS2EssentialInitializationGateResult result)
    {
        _step36Gates.Record(result);
        if (_step36ResultLabel is not null)
        {
            _step36ResultLabel.Text = _step36Gates.Snapshot().Summary;
            _step36ResultLabel.TextColor = result.Passed ? UIColor.Label : UIColor.SystemRed;
        }
        if (_step36DetailLabel is not null)
            _step36DetailLabel.Text = result.Detail;
        if (!result.Passed && _statusLabel is not null)
        {
            var letter = (char)('A' + (int)result.Gate - 1);
            _statusLabel.Text = $"STEP 36.0.3 FAIL at Gate {letter} ({result.Gate}). Stop here; later gates were not run. Preserve Step36 artifacts and force-quit before retry if Gate C began.";
            _statusLabel.TextColor = UIColor.SystemRed;
        }
        return result.Passed;
    }

    private bool TryInitializeStep36Telemetry(out string error)
    {
        error = string.Empty;
        lock (_step36CheckpointSync)
        {
            _step36TelemetryReady = false;
            _step36RunId = null;
            _step36CrashCheckpointPath = null;
            _step36LastCheckpointPath = null;
            _step36StaticMapPath = null;
            try
            {
                Directory.CreateDirectory(_deviceTestReportWriter.ReportsRoot);
                var now = DateTimeOffset.UtcNow;
                var runId = $"{now:yyyyMMddTHHmmssfffffffZ}-pid{Environment.ProcessId}-{Guid.NewGuid():N}";
                var crashName = $"Step36-CrashCheckpoint-{runId}.txt";
                var staticName = $"Step36-ExecuteEssential-StaticMap-{runId}.txt";
                var crashPath = Path.Combine(_deviceTestReportWriter.ReportsRoot, crashName);
                var lastPath = Path.Combine(_deviceTestReportWriter.ReportsRoot, "Step36-LastCheckpoint.txt");
                var staticPath = Path.Combine(_deviceTestReportWriter.ReportsRoot, staticName);
                WriteStep35TextFileDurably(
                    crashPath,
                    "StS2 Launcher — Step 36.0.3 ModelDb-bootstrap ExecuteEssential checkpoint\n" +
                    "Output-only diagnostic; never consumed as trusted runtime input.\n" +
                    $"Run ID: {runId}\n" +
                    $"Initialized UTC: {now:O}\n" +
                    $"Process ID: {Environment.ProcessId}\n" +
                    $"App version: {CurrentReleasePresentation.DisplayVersion} ({CurrentReleasePresentation.DisplayBuild})\n" +
                    "Candidate: STEP 36.0.3 — DEPENDENCY-AWARE MODELDB BOOTSTRAP + FULL UNCHANGED EXECUTEESSENTIAL\n" +
                    "Prerequisite: same-process Step-35.0.32 MODEL-BOOTSTRAP 4/4 authority; ExecuteDeferred/PrewarmJit/entry/native game load remain forbidden.\n\n");
                _step36RunId = runId;
                _step36CrashCheckpointPath = crashPath;
                _step36LastCheckpointPath = lastPath;
                _step36StaticMapPath = staticPath;
                _step36TelemetryReady = true;
                WriteStep36Checkpoint("RUN_TELEMETRY_READY — Step36 run journal created and durably flushed before Gate A.");
                return true;
            }
            catch (Exception ex)
            {
                error = $"{ex.GetType().Name}: {ex.Message}";
                return false;
            }
        }
    }

    private void WriteStep36Checkpoint(string detail)
    {
        if (!_step36TelemetryReady || string.IsNullOrWhiteSpace(_step36RunId) || string.IsNullOrWhiteSpace(_step36CrashCheckpointPath) || string.IsNullOrWhiteSpace(_step36LastCheckpointPath))
            return;
        try
        {
            lock (_step36CheckpointSync)
            {
                var single = (detail ?? string.Empty).Replace('\r', ' ').Replace('\n', ' ');
                var line = $"{DateTimeOffset.UtcNow:O} | run={_step36RunId} | pid={Environment.ProcessId} | managedThread={Environment.CurrentManagedThreadId} | {single}";
                using (var stream = new FileStream(_step36CrashCheckpointPath, FileMode.Append, FileAccess.Write, FileShare.Read, 4096, FileOptions.None))
                using (var writer = new StreamWriter(stream, new UTF8Encoding(false), 1024, leaveOpen: true))
                {
                    writer.WriteLine(line);
                    writer.Flush();
                    stream.Flush(flushToDisk: true);
                }
                WriteStep35TextFileDurably(
                    _step36LastCheckpointPath,
                    "StS2 Launcher — Step 36 last durable checkpoint\n" +
                    "Output-only diagnostic; overwrite-on-each-checkpoint convenience file.\n" +
                    $"Run ID: {_step36RunId}\n" +
                    $"Journal: {Path.GetFileName(_step36CrashCheckpointPath)}\n" +
                    $"Static map: {Path.GetFileName(_step36StaticMapPath)}\n" +
                    line + "\n");
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Step-36 checkpoint append failed: {ex.GetType().Name}: {ex.Message}");
        }
    }

    private bool WriteStep36StaticMap(out string error)
    {
        error = string.Empty;
        try
        {
            if (!_step36TelemetryReady || string.IsNullOrWhiteSpace(_step36StaticMapPath))
                throw new InvalidOperationException("Step36 telemetry is not initialized.");
            WriteStep35TextFileDurably(_step36StaticMapPath, _transformedRealStS2VeryEarlyInitialization.GetVerifiedEssentialStaticInstructionMap());
            return true;
        }
        catch (Exception ex)
        {
            error = $"{ex.GetType().Name}: {ex.Message}";
            return false;
        }
    }
}
