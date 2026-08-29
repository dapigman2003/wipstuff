using StS2Launcher.Core;
using System.Text;

namespace StS2Launcher.iOS;

public sealed partial class RootViewController
{
    private readonly TransformedRealStS2VeryEarlyInitializationGateSequence _transformedRealStS2VeryEarlyInitializationGates = new();
    private UILabel? _transformedRealStS2VeryEarlyInitializationResultLabel;
    private UILabel? _transformedRealStS2VeryEarlyInitializationDetailLabel;
    private UIButton? _transformedRealStS2VeryEarlyInitializationButton;
    private readonly object _step35CrashCheckpointSync = new();
    private const string Step35CurrentRunFileName = "Step35-CurrentRun.txt";
    private const string Step35LastCheckpointFileName = "Step35-LastCheckpoint.txt";
    private string? _step35RunId;
    private string? _step35CrashCheckpointPath;
    private string? _step35StaticMapPath;
    private string? _step35LastCheckpointPath;
    private bool _step35RunTelemetryReady;

    private void AddTransformedRealStS2VeryEarlyInitializationControls(UIStackView content)
    {
        content.AddArrangedSubview(Label(
            "Step 35.0.3 — Run-Correlated Durable Telemetry Before ExecuteVeryEarly Retry (ordered gates A–D; execution authority unchanged)",
            UIFont.BoldSystemFontOfSize(18),
            UIColor.Label));

        _transformedRealStS2VeryEarlyInitializationButton = SystemButton(
            "Run Step 35.0.3 A–D — Establish Durable Run ID → Reverify + Static IL Map → Admit → Invoke/Await Once → Audit",
            17);
        _transformedRealStS2VeryEarlyInitializationButton.TouchUpInside += async (_, _) => await RunTransformedRealStS2VeryEarlyInitializationAsync();
        content.AddArrangedSubview(_transformedRealStS2VeryEarlyInitializationButton);

        _transformedRealStS2VeryEarlyInitializationResultLabel = Label(
            "TRANSFORMED REAL STS2 VERY-EARLY INITIALIZATION: NOT RUN",
            UIFont.BoldSystemFontOfSize(15),
            UIColor.SecondaryLabel);
        content.AddArrangedSubview(_transformedRealStS2VeryEarlyInitializationResultLabel);

        _transformedRealStS2VeryEarlyInitializationDetailLabel = Label(
            "Physical 0.0.125 reproduced the same main-thread PC=0x0 / CODESIGNING Invalid Page hard termination as 0.0.124, but the available static map and crash report came from different process runs and the expected fixed-name crash checkpoint was absent. Step 35.0.3 / 0.0.126 keeps execution/resolver authority unchanged and first establishes a fail-visible run-correlated telemetry set: Step35-CurrentRun.txt, Step35-LastCheckpoint.txt, a unique run-specific crash journal, and a unique run-specific static IL map sharing the same Run ID/PID. Gate B/C will not begin unless durable telemetry is established. ExecuteEssential, ExecuteDeferred, the game entry point, Harmony, native loading and Godot/game startup remain separate later boundaries.",
            UIFont.SystemFontOfSize(13),
            UIColor.SecondaryLabel);
        content.AddArrangedSubview(_transformedRealStS2VeryEarlyInitializationDetailLabel);
    }

    private async Task RunTransformedRealStS2VeryEarlyInitializationAsync()
    {
        if (_transformedRealStS2VeryEarlyInitializationResultLabel is null ||
            _transformedRealStS2VeryEarlyInitializationDetailLabel is null ||
            _transformedRealStS2VeryEarlyInitializationButton is null ||
            _statusLabel is null)
            return;

        if (!CurrentReleasePresentation.BundleIdentityMatchesExpected)
        {
            _transformedRealStS2VeryEarlyInitializationResultLabel.Text = "TRANSFORMED REAL STS2 VERY-EARLY INITIALIZATION: RELEASE IDENTITY FAIL";
            _transformedRealStS2VeryEarlyInitializationResultLabel.TextColor = UIColor.SystemRed;
            _transformedRealStS2VeryEarlyInitializationDetailLabel.Text =
                $"Expected {CurrentReleasePresentation.ExpectedDisplayVersion} ({CurrentReleasePresentation.ExpectedBuildVersion}), observed {CurrentReleasePresentation.DisplayVersion} ({CurrentReleasePresentation.DisplayBuild}). Refusing Step 35 so execution evidence cannot be attributed to the wrong candidate.";
            _statusLabel.Text = "STEP 35 REFUSED — built bundle identity does not match the source-pinned candidate.";
            _statusLabel.TextColor = UIColor.SystemRed;
            return;
        }

        if (_godotProcessRequiresRestart || _godotSessionStarted)
        {
            _statusLabel.Text = "Step 35 requires a fresh process with no Godot process-global state and no sts2 assembly already loaded. Force-quit/relaunch before running Step 35.";
            _statusLabel.TextColor = UIColor.SystemOrange;
            return;
        }

        if (!TryInitializeStep35RunTelemetry(out var telemetryError))
        {
            _transformedRealStS2VeryEarlyInitializationResultLabel.Text = "TRANSFORMED REAL STS2 VERY-EARLY INITIALIZATION: TELEMETRY FAIL / NOT RUN";
            _transformedRealStS2VeryEarlyInitializationResultLabel.TextColor = UIColor.SystemRed;
            _transformedRealStS2VeryEarlyInitializationDetailLabel.Text =
                $"Step 35.0.3 refused to begin Gate A because durable run-correlated telemetry could not be established. No CLR admission or ExecuteVeryEarly invocation was attempted. {telemetryError}";
            _statusLabel.Text = "STEP 35 DIAGNOSTIC REFUSED — durable run journal could not be created/flushed. Preserve any Step35-CurrentRun/LastCheckpoint files and fix report storage before retry.";
            _statusLabel.TextColor = UIColor.SystemRed;
            return;
        }

        BeginSteamOperation(allowCancel: true);
        _transformedRealStS2VeryEarlyInitializationGates.Reset();
        _transformedRealStS2VeryEarlyInitialization.Reset();
        _transformedRealStS2VeryEarlyInitializationResultLabel.Text = "TRANSFORMED REAL STS2 VERY-EARLY INITIALIZATION: GATE A RUNNING…";
        _transformedRealStS2VeryEarlyInitializationResultLabel.TextColor = UIColor.Label;
        _statusLabel.Text = $"STEP 35.0.3 RUN {_step35RunId} — durable telemetry established. Gate A re-manufactures/reverifies the closed transform and exact ExecuteVeryEarly wrapper/state machine; no CLR admission yet.";
        _statusLabel.TextColor = UIColor.Label;

        try
        {
            WriteStep35CrashCheckpoint("RUN_START — fresh-process Step 35.0.3 diagnostic run started; execution authority is unchanged from 0.0.123/0.0.124/0.0.125.");
            var token = _operationCts?.Token ?? CancellationToken.None;
            var progress = new Progress<TransformedRealStS2VeryEarlyInitializationProgress>(value =>
            {
                var count = value.TotalItems > 0 ? $" ({value.ProcessedItems:N0}/{value.TotalItems:N0})" : string.Empty;
                _transformedRealStS2VeryEarlyInitializationDetailLabel.Text = FormatTransformedRealStS2VeryEarlyInitializationDetail(
                    $"Gate {(char)('A' + (int)value.Gate - 1)} progress{count}: {value.Detail}" +
                    (string.IsNullOrWhiteSpace(value.CurrentPath) ? string.Empty : $"\nCurrent: {value.CurrentPath}"));
            });

            var gateA = await _transformedRealStS2VeryEarlyInitialization.RunVerifiedExecutionPreflightAsync(progress, token);
            WriteStep35CrashCheckpoint($"A_RESULT — passed={gateA.Passed}; gate={gateA.Gate}.");
            if (!RecordTransformedRealStS2VeryEarlyInitializationGate(gateA)) return;
            WriteStep35CrashCheckpoint("A_PASS — Gate A completed; writing run-correlated verified static ExecuteVeryEarly IL/callsite map before any CLR admission.");
            if (!WriteStep35StaticMap(out var staticMapError))
            {
                WriteStep35CrashCheckpoint($"A_STATIC_MAP_WRITE_FAILED_STOP — {staticMapError}");
                _transformedRealStS2VeryEarlyInitializationResultLabel.Text = "TRANSFORMED REAL STS2 VERY-EARLY INITIALIZATION: STATIC-MAP TELEMETRY FAIL / STOPPED BEFORE GATE B";
                _transformedRealStS2VeryEarlyInitializationResultLabel.TextColor = UIColor.SystemRed;
                _transformedRealStS2VeryEarlyInitializationDetailLabel.Text = FormatTransformedRealStS2VeryEarlyInitializationDetail(
                    $"Gate A compatibility checks passed, but the run-correlated static map could not be durably written. Gate B/C were intentionally not attempted. {staticMapError}");
                _statusLabel.Text = "STEP 35 DIAGNOSTIC STOP — static-map telemetry failed before CLR admission; this is not a compatibility FAIL.";
                _statusLabel.TextColor = UIColor.SystemRed;
                return;
            }
            WriteStep35CrashCheckpoint("A_STATIC_MAP_WRITE_RETURNED — run-correlated diagnostic static-map writer durably returned; about to select/schedule Gate B.");

            _transformedRealStS2VeryEarlyInitializationResultLabel.Text = "TRANSFORMED REAL STS2 VERY-EARLY INITIALIZATION: GATE B RUNNING…";
            _statusLabel.Text = "STEP 35 GATE B — CLR-admit only the exact transformed sts2.dll into the strict execution context and re-prove Step-33 zero-resolution admission behavior.";
            WriteStep35CrashCheckpoint("B_SCHEDULE — Gate B UI selected; scheduling Gate B on Task.Run.");
            var gateB = await Task.Run(() => _transformedRealStS2VeryEarlyInitialization.RunExecutionCapableClrAdmission(WriteStep35CrashCheckpoint), token);
            WriteStep35CrashCheckpoint($"B_TASK_AWAIT_RESUMED — Gate B Task.Run await resumed on launcher thread; passed={gateB.Passed}.");
            WriteStep35CrashCheckpoint("B_RESULT_RECORD_START — about to record Gate B result in the ordered gate sequence/UI.");
            if (!RecordTransformedRealStS2VeryEarlyInitializationGate(gateB))
            {
                WriteStep35CrashCheckpoint("B_RESULT_FAIL_STOP — Gate B returned a managed FAIL; later gates will not run.");
                return;
            }
            WriteStep35CrashCheckpoint("B_RESULT_RECORD_PASS — Gate B PASS recorded; about to select Gate C.");

            _transformedRealStS2VeryEarlyInitializationResultLabel.Text = "TRANSFORMED REAL STS2 VERY-EARLY INITIALIZATION: GATE C RUNNING…";
            _statusLabel.Text = "STEP 35 GATE C — bind exact transformed ExecuteVeryEarly(), invoke it once, await its Task to completion (60s max), and fail closed on initializer-bearing/unplanned/native requests.";
            WriteStep35CrashCheckpoint("C_UI_SELECTED — Gate C labels assigned on the main thread; UIKit may not have repainted before synchronous Gate-C work begins.");
            var gateC = await _transformedRealStS2VeryEarlyInitialization.RunExactExecuteVeryEarlyInvocationAsync(WriteStep35CrashCheckpoint, token);
            WriteStep35CrashCheckpoint($"C_TASK_AWAIT_RESUMED — Gate C async method returned to the UI caller; passed={gateC.Passed}.");
            if (!RecordTransformedRealStS2VeryEarlyInitializationGate(gateC))
            {
                WriteStep35CrashCheckpoint("C_RESULT_FAIL_STOP — Gate C returned a managed FAIL; Gate D will not run.");
                return;
            }
            WriteStep35CrashCheckpoint("C_RESULT_RECORD_PASS — Gate C PASS recorded; about to select Gate D.");

            _transformedRealStS2VeryEarlyInitializationResultLabel.Text = "TRANSFORMED REAL STS2 VERY-EARLY INITIALIZATION: GATE D RUNNING…";
            _statusLabel.Text = "STEP 35 GATE D — re-prove OfflineReady, source/transformed/plan/dependency hashes, exact residency and zero broader startup/native escape.";
            WriteStep35CrashCheckpoint("D_START — entering final isolation audit.");
            var gateD = await _transformedRealStS2VeryEarlyInitialization.RunFinalIsolationAuditAsync(progress, token);
            WriteStep35CrashCheckpoint($"D_RESULT — passed={gateD.Passed}; gate={gateD.Gate}.");
            if (!RecordTransformedRealStS2VeryEarlyInitializationGate(gateD)) return;

            var snapshot = _transformedRealStS2VeryEarlyInitializationGates.Snapshot();
            _transformedRealStS2VeryEarlyInitializationResultLabel.Text = snapshot.Summary;
            _transformedRealStS2VeryEarlyInitializationResultLabel.TextColor = UIColor.Label;
            _transformedRealStS2VeryEarlyInitializationDetailLabel.Text = FormatTransformedRealStS2VeryEarlyInitializationDetail(
                "All four Step 35 gates passed. Preserve this report. A pass proves the earliest measured real-StS2 one-time startup boundary, ExecuteVeryEarly(), can be invoked once and its exact Task awaited to normal completion on physical iOS under the strict prepared resolver. ExecuteEssential, ExecuteDeferred, the game entry point, Harmony/native loading and Godot/game startup remain separately gated.");
            _statusLabel.Text = "PASS: STEP 35 CONTROLLED VERY-EARLY INITIALIZATION — 4/4. ExecuteVeryEarly Task completed normally; ExecuteEssential/ExecuteDeferred and broader startup remain unauthorized.";
            _statusLabel.TextColor = UIColor.Label;
            WriteStep35CrashCheckpoint("RUN_PASS_4OF4 — all Step-35 gates completed successfully.");
        }
        catch (OperationCanceledException)
        {
            WriteStep35CrashCheckpoint("RUN_CANCELLED_INCONCLUSIVE — operator cancellation is not a compatibility FAIL; if Gate B/C began, this process is spent and must be force-quit before retry.");
            _transformedRealStS2VeryEarlyInitializationResultLabel.Text = "TRANSFORMED REAL STS2 VERY-EARLY INITIALIZATION: CANCELLED / INCONCLUSIVE";
            _transformedRealStS2VeryEarlyInitializationResultLabel.TextColor = UIColor.SecondaryLabel;
            _transformedRealStS2VeryEarlyInitializationDetailLabel.Text = FormatTransformedRealStS2VeryEarlyInitializationDetail(
                "Step 35 was cancelled and is INCONCLUSIVE rather than PASS/FAIL. If Gate B had begun, force-quit before retrying because transformed sts2 and any initializer-free dependencies may now be CLR-resident; if Gate C invocation began, ExecuteVeryEarly may also have executed despite cancellation.");
            _statusLabel.Text = "STEP 35 CANCELLED / INCONCLUSIVE — later gates are unproven; force-quit before retry if Gate B or C started.";
            _statusLabel.TextColor = UIColor.SecondaryLabel;
        }
        catch (Exception ex)
        {
            WriteStep35CrashCheckpoint($"RUN_MANAGED_EXCEPTION — {ex.GetType().FullName}: {ex.Message}");
            _transformedRealStS2VeryEarlyInitializationResultLabel.Text = "TRANSFORMED REAL STS2 VERY-EARLY INITIALIZATION: EXCEPTION";
            _transformedRealStS2VeryEarlyInitializationResultLabel.TextColor = UIColor.SystemRed;
            _transformedRealStS2VeryEarlyInitializationDetailLabel.Text = FormatTransformedRealStS2VeryEarlyInitializationDetail($"Unhandled Step 35 exception: {ex.GetType().Name}: {ex.Message}");
            _statusLabel.Text = "STEP 35 FAIL — stop at the first failing gate and preserve the report. Force-quit before retry if Gate B started.";
            _statusLabel.TextColor = UIColor.SystemRed;
        }
        finally
        {
            WriteStep35CrashCheckpoint("RUN_FINALLY_ENTER — managed control reached the Step-35 finally block; writing the normal deterministic report.");
            await WriteDeviceTestReportFromLabelsAsync(
                "Step35-TransformedRealStS2VeryEarlyInitialization.txt",
                "StS2 Launcher — Step 35 Transformed Real StS2 Very-Early Initialization",
                _transformedRealStS2VeryEarlyInitializationResultLabel,
                _transformedRealStS2VeryEarlyInitializationDetailLabel,
                CancellationToken.None);
            WriteStep35CrashCheckpoint("RUN_NORMAL_REPORT_RETURNED — normal Step35 report writer returned.");
            EndSteamOperation();
            WriteStep35CrashCheckpoint("RUN_END — Step-35 UI operation ended normally.");
        }
    }

    private bool RecordTransformedRealStS2VeryEarlyInitializationGate(TransformedRealStS2VeryEarlyInitializationGateResult result)
    {
        _transformedRealStS2VeryEarlyInitializationGates.Record(result);
        if (_transformedRealStS2VeryEarlyInitializationResultLabel is not null)
        {
            _transformedRealStS2VeryEarlyInitializationResultLabel.Text = _transformedRealStS2VeryEarlyInitializationGates.Snapshot().Summary;
            _transformedRealStS2VeryEarlyInitializationResultLabel.TextColor = result.Passed ? UIColor.Label : UIColor.SystemRed;
        }
        if (_transformedRealStS2VeryEarlyInitializationDetailLabel is not null)
            _transformedRealStS2VeryEarlyInitializationDetailLabel.Text = FormatTransformedRealStS2VeryEarlyInitializationDetail(result.Detail);
        if (!result.Passed && _statusLabel is not null)
        {
            var letter = (char)('A' + (int)result.Gate - 1);
            _statusLabel.Text = $"STEP 35 FAIL at Gate {letter} ({result.Gate}). Stop here; later execution gates were not run. Preserve the report and force-quit before retry.";
            _statusLabel.TextColor = UIColor.SystemRed;
        }
        return result.Passed;
    }

    private string FormatTransformedRealStS2VeryEarlyInitializationDetail(string tail)
    {
        var lines = new List<string>();
        foreach (var gate in _transformedRealStS2VeryEarlyInitializationGates.Results)
        {
            var letter = (char)('A' + (int)gate.Gate - 1);
            lines.Add($"Gate {letter} — {gate.Gate}: {(gate.Passed ? "PASS" : "FAIL")}");
            lines.Add(gate.Detail);
            lines.Add(string.Empty);
        }

        lines.Add("Step 32 physical baseline: CLOSED POSITIVE — 0.0.120 passed 4/4. Exact transformed SHA-256 39c0a89ad0d5c6eb1553e23dd8537a7b7ab8278fad4115d186db5751570211ef; transformed PrewarmJit token 0x0600AFEA; semantic fingerprint 47fadf2a46eda098f310b7d0ee54e37d1e952ac272fc966d16d557ed46a0b74a; zero PrepareMethod references; trusted install unchanged.");
        lines.Add("Step 33 physical baseline: CLOSED POSITIVE — 0.0.121 passed 4/4. Only the exact transformed primary entered StS2Launcher-Step33-TransformedGame; admission caused zero managed resolver requests, zero private dependency loads and zero native attempts; no game member was reflected or invoked.");
        lines.Add("Step 34 physical baseline: CLOSED POSITIVE — 0.0.122 passed 4/4. Exact transformed PrewarmJit was invoked once and returned normally; 8 managed requests produced 6 exact host loads + 2 initializer-free private loads, with zero initializer-bearing/unplanned/native escape and no entry point/Godot startup.");
        lines.Add("Step 35.0 physical 0.0.123 observation: app hard-terminated while the UI still appeared near Gate B; the matching iOS .ips reported EXC_BAD_ACCESS/SIGKILL with faulting main-thread PC=0x0 and no managed Step-35 report.");
        lines.Add("Step 35.0.1 physical 0.0.124 localization: Gate A passed; Gate B passed fully through LoadFromStream/identity/MVID/zero-resolution/residency checks; Gate C bound exact transformed ExecuteVeryEarly, entered MethodInfo.Invoke, loaded GodotSharp and Steamworks.NET plus exact host frameworks, then hard-terminated before C_INVOKE_RETURNED. The matching .ips repeats the main-thread PC=0x0 native signature and essentially the same runtime stack shape as 0.0.123.");
        lines.Add("Step 35.0.2 / 0.0.125 physical observation: the same main-thread PC=0x0 / CODESIGNING Invalid Page hard-kill family reproduced. The available static map predated the matching .ips process, while the expected fixed-name crash checkpoint was absent, proving that cross-run artifact correlation and silently swallowed telemetry errors were an evidence-quality gap.");
        lines.Add("Step 35.0.3 diagnostic scope: preserve the exact execution/resolver authority while making telemetry a pre-execution prerequisite. Each run receives one immutable Run ID/PID shared by a unique crash journal and unique static map; Step35-CurrentRun.txt names those files, and Step35-LastCheckpoint.txt is independently flushed after each journal append. Failure to establish the journal is visible and stops before Gate A; static-map failure stops before Gate B. These are diagnostic stops, not compatibility failures.");
        lines.Add("Step 35 scope: begin the natural managed startup sequence at exact static parameterless Task-returning MegaCrit.Sts2.Core.Helpers.OneTimeInitialization::ExecuteVeryEarly(), source token 0x06007D02. Re-prove its unchanged async state machine, admit only the exact transformed primary, invoke once and await the returned Task for at most 60 seconds.");
        lines.Add("Resolver boundary: exact persisted host-framework bindings and hash-pinned initializer-free prepared private dependencies may be serviced on demand. The known initializer-bearing 0Harmony 2.4.2.0 dependency remains forbidden; any changed/additional initializer-bearing dependency, unplanned managed request, or native request fails closed.");
        lines.Add("Forbidden in Step 35: receipt-backed/prepared original sts2.dll CLR admission, intentional ExecuteEssential/ExecuteDeferred/PrewarmJit invocation by the launcher, game entry-point execution, Harmony/MonoMod API invocation or runtime patching, Godot/game startup, native game loading, arbitrary resolver fallback, or broad startup sequencing.");
        lines.Add("Cancellation semantics: CANCELLED is INCONCLUSIVE, not a compatibility FAIL. If Gate B has begun the process is spent; after Gate C invocation begins, cancellation cannot undo any code that already ran. Force-quit before retry.");
        lines.Add("After Gate B, transformed sts2 remains CLR-resident until force-quit on the physical non-collectible context. After Gate C, ExecuteVeryEarly has executed once; do not rerun Step 35 in the same process.");
        lines.Add(tail);
        return string.Join("\n", lines);
    }
    private bool TryInitializeStep35RunTelemetry(out string error)
    {
        error = string.Empty;
        lock (_step35CrashCheckpointSync)
        {
            _step35RunTelemetryReady = false;
            _step35RunId = null;
            _step35CrashCheckpointPath = null;
            _step35StaticMapPath = null;
            _step35LastCheckpointPath = null;

            try
            {
                Directory.CreateDirectory(_deviceTestReportWriter.ReportsRoot);
                var initializedUtc = DateTimeOffset.UtcNow;
                var runId = $"{initializedUtc:yyyyMMddTHHmmssfffffffZ}-pid{Environment.ProcessId}-{Guid.NewGuid():N}";
                var crashFileName = $"Step35-CrashCheckpoint-{runId}.txt";
                var staticMapFileName = $"Step35-ExecuteVeryEarly-StaticMap-{runId}.txt";
                var crashPath = Path.Combine(_deviceTestReportWriter.ReportsRoot, crashFileName);
                var staticMapPath = Path.Combine(_deviceTestReportWriter.ReportsRoot, staticMapFileName);
                var lastCheckpointPath = Path.Combine(_deviceTestReportWriter.ReportsRoot, Step35LastCheckpointFileName);
                var currentRunPath = Path.Combine(_deviceTestReportWriter.ReportsRoot, Step35CurrentRunFileName);

                WriteStep35TextFileDurably(
                    crashPath,
                    "StS2 Launcher — Step 35.0.3 run-correlated crash checkpoint\n" +
                    "Output-only diagnostic; never consumed as trusted runtime input.\n" +
                    $"Run ID: {runId}\n" +
                    $"Initialized UTC: {initializedUtc:O}\n" +
                    $"Process ID: {Environment.ProcessId}\n" +
                    $"App version: {CurrentReleasePresentation.DisplayVersion} ({CurrentReleasePresentation.DisplayBuild})\n" +
                    $"Expected source version: {CurrentReleasePresentation.ExpectedDisplayVersion} ({CurrentReleasePresentation.ExpectedBuildVersion})\n" +
                    "Candidate: STEP 35.0.3 — RUN-CORRELATED DURABLE TELEMETRY\n" +
                    "Execution policy: unchanged from Step 35.0 / 0.0.123 through Step 35.0.2 / 0.0.125; diagnostic telemetry gating only.\n" +
                    $"Implementation: {CurrentReleasePresentation.Step35ImplementationMarker}\n\n");

                var initialLine = $"{initializedUtc:O} | run={runId} | pid={Environment.ProcessId} | managedThread={Environment.CurrentManagedThreadId} | RUN_TELEMETRY_READY — run-specific journal created and durably flushed before Gate A.";
                using (var stream = new FileStream(crashPath, FileMode.Append, FileAccess.Write, FileShare.Read, 4096, FileOptions.None))
                using (var writer = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), 1024, leaveOpen: true))
                {
                    writer.WriteLine(initialLine);
                    writer.Flush();
                    stream.Flush(flushToDisk: true);
                }

                WriteStep35TextFileDurably(
                    lastCheckpointPath,
                    "StS2 Launcher — Step 35 last durable checkpoint\n" +
                    "Output-only diagnostic; overwrite-on-each-checkpoint convenience file.\n" +
                    $"Run ID: {runId}\n" +
                    $"Journal: {crashFileName}\n" +
                    $"Static map: {staticMapFileName}\n" +
                    initialLine + "\n");

                WriteStep35TextFileDurably(
                    currentRunPath,
                    "StS2 Launcher — Step 35 current run manifest\n" +
                    "Output-only diagnostic; use this file to correlate artifacts from the same process run.\n" +
                    $"Run ID: {runId}\n" +
                    $"Initialized UTC: {initializedUtc:O}\n" +
                    $"Process ID: {Environment.ProcessId}\n" +
                    $"App version: {CurrentReleasePresentation.DisplayVersion} ({CurrentReleasePresentation.DisplayBuild})\n" +
                    $"Crash journal: {crashFileName}\n" +
                    $"Static map: {staticMapFileName}\n" +
                    $"Last checkpoint: {Step35LastCheckpointFileName}\n" +
                    "Candidate: STEP 35.0.3 — RUN-CORRELATED DURABLE TELEMETRY\n");

                _step35RunId = runId;
                _step35CrashCheckpointPath = crashPath;
                _step35StaticMapPath = staticMapPath;
                _step35LastCheckpointPath = lastCheckpointPath;
                _step35RunTelemetryReady = true;
                return true;
            }
            catch (Exception ex)
            {
                error = $"{ex.GetType().Name}: {ex.Message}";
                Console.Error.WriteLine($"Step-35 run-telemetry initialization failed: {error}");
                return false;
            }
        }
    }

    private bool WriteStep35StaticMap(out string error)
    {
        error = string.Empty;
        try
        {
            if (!_step35RunTelemetryReady || string.IsNullOrWhiteSpace(_step35RunId) || string.IsNullOrWhiteSpace(_step35StaticMapPath))
                throw new InvalidOperationException("Step-35 run telemetry is not initialized.");

            var body = _transformedRealStS2VeryEarlyInitialization.GetVerifiedVeryEarlyStaticInstructionMap();
            WriteStep35TextFileDurably(
                _step35StaticMapPath,
                $"Generated UTC: {DateTimeOffset.UtcNow:O}\n" +
                $"Run ID: {_step35RunId}\n" +
                $"Process ID: {Environment.ProcessId}\n" +
                $"App version: {CurrentReleasePresentation.DisplayVersion} ({CurrentReleasePresentation.DisplayBuild})\n" +
                $"Expected source version: {CurrentReleasePresentation.ExpectedDisplayVersion} ({CurrentReleasePresentation.ExpectedBuildVersion})\n" +
                "Candidate: STEP 35.0.3 — output-only run-correlated static ExecuteVeryEarly IL/callsite localization\n" +
                "This file is generated from the already-verified exact transformed image before CLR admission and is never consumed as runtime input.\n\n" +
                body + "\n");
            return true;
        }
        catch (Exception ex)
        {
            error = $"{ex.GetType().Name}: {ex.Message}";
            Console.Error.WriteLine($"Step-35 static-map write failed: {error}");
            return false;
        }
    }

    private void WriteStep35CrashCheckpoint(string detail)
    {
        try
        {
            lock (_step35CrashCheckpointSync)
            {
                if (!_step35RunTelemetryReady ||
                    string.IsNullOrWhiteSpace(_step35RunId) ||
                    string.IsNullOrWhiteSpace(_step35CrashCheckpointPath) ||
                    string.IsNullOrWhiteSpace(_step35StaticMapPath) ||
                    string.IsNullOrWhiteSpace(_step35LastCheckpointPath))
                {
                    Console.Error.WriteLine("Step-35 crash-checkpoint append skipped because run telemetry is not initialized.");
                    return;
                }

                var singleLine = (detail ?? string.Empty).Replace('\r', ' ').Replace('\n', ' ');
                var line = $"{DateTimeOffset.UtcNow:O} | run={_step35RunId} | pid={Environment.ProcessId} | managedThread={Environment.CurrentManagedThreadId} | {singleLine}";

                using (var stream = new FileStream(_step35CrashCheckpointPath, FileMode.Append, FileAccess.Write, FileShare.Read, 4096, FileOptions.None))
                using (var writer = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), 1024, leaveOpen: true))
                {
                    writer.WriteLine(line);
                    writer.Flush();
                    stream.Flush(flushToDisk: true);
                }

                WriteStep35TextFileDurably(
                    _step35LastCheckpointPath,
                    "StS2 Launcher — Step 35 last durable checkpoint\n" +
                    "Output-only diagnostic; overwrite-on-each-checkpoint convenience file.\n" +
                    $"Run ID: {_step35RunId}\n" +
                    $"Journal: {Path.GetFileName(_step35CrashCheckpointPath)}\n" +
                    $"Static map: {Path.GetFileName(_step35StaticMapPath)}\n" +
                    line + "\n");
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Step-35 crash-checkpoint append failed: {ex.GetType().Name}: {ex.Message}");
        }
    }

    private static void WriteStep35TextFileDurably(string path, string body)
    {
        using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read, 4096, FileOptions.None);
        using var writer = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), 1024, leaveOpen: true);
        writer.Write(body);
        writer.Flush();
        stream.Flush(flushToDisk: true);
    }

}
