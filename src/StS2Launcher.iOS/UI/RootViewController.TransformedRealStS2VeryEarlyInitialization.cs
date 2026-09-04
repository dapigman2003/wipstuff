using StS2Launcher.Core;
using StS2Launcher.iOS.Platform;
using System.Text;

namespace StS2Launcher.iOS;

public sealed partial class RootViewController
{
    private readonly TransformedRealStS2VeryEarlyInitializationGateSequence _transformedRealStS2VeryEarlyInitializationGates = new();
    private UILabel? _transformedRealStS2VeryEarlyInitializationResultLabel;
    private UILabel? _transformedRealStS2VeryEarlyInitializationDetailLabel;
    private UIButton? _transformedRealStS2VeryEarlyInitializationButton;
    private UIButton? _step35NaturalGodotReconButton;
    private UIButton? _step35ManagedCommandLineCompatibilityButton;
    private UIButton? _step35GodotCoreCallbackHandoffButton;
    private UIButton? _step35GodotExactClosureButton;
    private readonly object _step35CrashCheckpointSync = new();
    private const string Step35CurrentRunFileName = "Step35-CurrentRun.txt";
    private const string Step35LastCheckpointFileName = "Step35-LastCheckpoint.txt";
    private string? _step35RunId;
    private string? _step35CrashCheckpointPath;
    private string? _step35StaticMapPath;
    private string? _step35GodotReconnaissancePath;
    private string? _step35LastCheckpointPath;
    private bool _step35RunTelemetryReady;

    private void AddTransformedRealStS2VeryEarlyInitializationControls(UIStackView content)
    {
        content.AddArrangedSubview(Label(
            "Step 35.0.27 — Exact-authority closure candidate + Gate-D finalization (four diagnostic controls + one exact mode; ordered gates A–D)",
            UIFont.BoldSystemFontOfSize(18),
            UIColor.Label));

        _step35NaturalGodotReconButton = SystemButton(
            "Run Step 35.0.27 NATURAL diagnostic — Reverify → Preserve Godot Dictionary/OS Path → Invoke Once",
            16);
        _step35NaturalGodotReconButton.TouchUpInside += async (_, _) => await RunTransformedRealStS2VeryEarlyInitializationAsync(Step35DiagnosticMode.NaturalGodotDictionaryRecon);
        content.AddArrangedSubview(_step35NaturalGodotReconButton);

        _transformedRealStS2VeryEarlyInitializationButton = SystemButton(
            "Run Step 35.0.27 OS-RECON diagnostic — Reverify → Managed Dictionary → Natural Godot.OS → Invoke Once",
            17);
        _transformedRealStS2VeryEarlyInitializationButton.TouchUpInside += async (_, _) => await RunTransformedRealStS2VeryEarlyInitializationAsync(Step35DiagnosticMode.ManagedDictionaryCompatibility);
        content.AddArrangedSubview(_transformedRealStS2VeryEarlyInitializationButton);

        _step35ManagedCommandLineCompatibilityButton = SystemButton(
            "Run Step 35.0.27 FORWARD diagnostic — Reverify → Managed Dictionary + Managed Empty Args → Invoke Once",
            17);
        _step35ManagedCommandLineCompatibilityButton.TouchUpInside += async (_, _) => await RunTransformedRealStS2VeryEarlyInitializationAsync(Step35DiagnosticMode.ManagedCommandLineCompatibility);
        content.AddArrangedSubview(_step35ManagedCommandLineCompatibilityButton);

        _step35GodotCoreCallbackHandoffButton = SystemButton(
            "Run Step 35.0.27 CORE-HANDOFF diagnostic — Step 15 → Proven Bridge → Diagnostic sts2/GodotSharp",
            17);
        _step35GodotCoreCallbackHandoffButton.TouchUpInside += async (_, _) => await RunTransformedRealStS2VeryEarlyInitializationAsync(Step35DiagnosticMode.GodotCoreCallbackHandoff);
        content.AddArrangedSubview(_step35GodotCoreCallbackHandoffButton);

        _step35GodotExactClosureButton = SystemButton(
            "Run Step 35.0.27 EXACT-CLOSURE — Step 15 → Exact Transformed sts2 + Exact GodotSharp → Proven Bridge → Invoke Exact Authority",
            16);
        _step35GodotExactClosureButton.TouchUpInside += async (_, _) => await RunTransformedRealStS2VeryEarlyInitializationAsync(Step35DiagnosticMode.GodotCoreExactClosure);
        content.AddArrangedSubview(_step35GodotExactClosureButton);

        _transformedRealStS2VeryEarlyInitializationResultLabel = Label(
            "TRANSFORMED REAL STS2 VERY-EARLY INITIALIZATION: NOT RUN",
            UIFont.BoldSystemFontOfSize(15),
            UIColor.SecondaryLabel);
        content.AddArrangedSubview(_transformedRealStS2VeryEarlyInitializationResultLabel);

        _transformedRealStS2VeryEarlyInitializationDetailLabel = Label(
            "Physical 0.0.149: diagnostic Gate C returned/awaited RanToCompletion and recorded PASS; Gate-D UI reached terminal 4/4 while durable telemetry remained at D_START. 0.0.150 adds durable D-finalization boundaries plus EXACT-CLOSURE using exact transformed sts2 and exact prepared GodotSharp through the proven Godot 4.5.1 bridge.",
            UIFont.SystemFontOfSize(13),
            UIColor.SecondaryLabel);
        content.AddArrangedSubview(_transformedRealStS2VeryEarlyInitializationDetailLabel);
        AddStep35GateDProgressControls(content);
    }

    private async Task RunTransformedRealStS2VeryEarlyInitializationAsync(Step35DiagnosticMode diagnosticMode)
    {
        if (_transformedRealStS2VeryEarlyInitializationResultLabel is null ||
            _transformedRealStS2VeryEarlyInitializationDetailLabel is null ||
            _transformedRealStS2VeryEarlyInitializationButton is null ||
            _step35NaturalGodotReconButton is null ||
            _step35ManagedCommandLineCompatibilityButton is null ||
            _step35GodotCoreCallbackHandoffButton is null ||
            _step35GodotExactClosureButton is null ||
            _statusLabel is null)
            return;

        _transformedRealStS2VeryEarlyInitialization.DiagnosticMode = diagnosticMode;

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

        var callbackHandoffMode = diagnosticMode is Step35DiagnosticMode.GodotCoreCallbackHandoff or Step35DiagnosticMode.GodotCoreExactClosure;
        var exactAuthorityMode = diagnosticMode == Step35DiagnosticMode.GodotCoreExactClosure;
        if (callbackHandoffMode)
        {
            if (!_godotSessionStarted || !GodotStep15NativeBridge.IsEngineStarted || !GodotStep15NativeBridge.IsSetupFinished)
            {
                _statusLabel.Text = $"{(exactAuthorityMode ? "EXACT-CLOSURE" : "CORE-HANDOFF")} requires the Step 15 embedded smoke engine to be started and setup-complete in this same process. Run Step 15 Gates A-C first, then return directly to this button without force-quitting.";
                _statusLabel.TextColor = UIColor.SystemOrange;
                return;
            }
            if (!GodotStep15NativeBridge.IsRuntimeInteropReady)
            {
                _statusLabel.Text = $"{(exactAuthorityMode ? "EXACT-CLOSURE" : "CORE-HANDOFF")} refused: the source-built Godot C# native callback table is not ready. Native detail: {GodotStep15NativeBridge.LastError}";
                _statusLabel.TextColor = UIColor.SystemRed;
                return;
            }
            if (GodotStep15NativeBridge.HasDotNetFeature || GodotStep15NativeBridge.IsDotNetRuntimeInitialized)
            {
                _statusLabel.Text = $"{(exactAuthorityMode ? "EXACT-CLOSURE" : "CORE-HANDOFF")} refused: the embedded smoke project advertises the dotnet feature or Godot reports its own .NET runtime initialized. This path requires native Godot/C# scaffolding but no competing Godot-managed runtime.";
                _statusLabel.TextColor = UIColor.SystemRed;
                return;
            }
        }
        else if (_godotProcessRequiresRestart || _godotSessionStarted)
        {
            _statusLabel.Text = "NATURAL / OS-RECON / FORWARD require a fresh process with no Godot process-global state and no sts2 assembly already loaded. Force-quit/relaunch before those modes.";
            _statusLabel.TextColor = UIColor.SystemOrange;
            return;
        }

        if (!TryInitializeStep35RunTelemetry(out var telemetryError))
        {
            _transformedRealStS2VeryEarlyInitializationResultLabel.Text = "TRANSFORMED REAL STS2 VERY-EARLY INITIALIZATION: TELEMETRY FAIL / NOT RUN";
            _transformedRealStS2VeryEarlyInitializationResultLabel.TextColor = UIColor.SystemRed;
            _transformedRealStS2VeryEarlyInitializationDetailLabel.Text =
                $"Step 35.0.27 refused to begin Gate A because durable run-correlated telemetry could not be established. No CLR admission or ExecuteVeryEarly invocation was attempted. {telemetryError}";
            _statusLabel.Text = "STEP 35 DIAGNOSTIC REFUSED — durable run journal could not be created/flushed. Preserve any Step35-CurrentRun/LastCheckpoint files and fix report storage before retry.";
            _statusLabel.TextColor = UIColor.SystemRed;
            return;
        }

        BeginSteamOperation(allowCancel: true);
        ResetStep35GateDProgress(visible: false);
        _transformedRealStS2VeryEarlyInitializationGates.Reset();
        _transformedRealStS2VeryEarlyInitialization.Reset();
        _transformedRealStS2VeryEarlyInitializationResultLabel.Text = "TRANSFORMED REAL STS2 VERY-EARLY INITIALIZATION: GATE A RUNNING…";
        _transformedRealStS2VeryEarlyInitializationResultLabel.TextColor = UIColor.Label;
        _statusLabel.Text = $"STEP 35.0.27 RUN {_step35RunId} — mode={diagnosticMode}; exactAuthority={exactAuthorityMode}. Gate A re-manufactures/reverifies the closed transform and emits diagnostic derivatives for static/reconnaissance evidence. Gate B will admit {(exactAuthorityMode ? "the exact closed transformed sts2 bytes" : "the selected diagnostic sts2 derivative")}.";
        _statusLabel.TextColor = UIColor.Label;

        try
        {
            WriteStep35CrashCheckpoint($"RUN_START — Step 35.0.27 run started; mode={diagnosticMode}; callbackHandoffMode={callbackHandoffMode}; exactAuthority={exactAuthorityMode}; exact transformed source/resolver authority remains frozen. NATURAL/OS-RECON/FORWARD preserve their fresh-process diagnostic contracts. CORE-HANDOFF uses diagnostic CLR inputs after the proven Step-15 bridge. EXACT-CLOSURE uses exact transformed sts2 plus exact prepared GodotSharp after that same bridge prerequisite.");
            var token = _operationCts?.Token ?? CancellationToken.None;
            var progress = new Progress<TransformedRealStS2VeryEarlyInitializationProgress>(value =>
            {
                if (value.Gate == TransformedRealStS2VeryEarlyInitializationGate.FinalIsolationAudit)
                {
                    // Gate D may emit many receipt-verifier updates. Keep them on the compact dedicated
                    // progress controls instead of repeatedly rebuilding/re-laying out the very large
                    // historical detail label. Physical 0.0.149 reached the terminal 4/4 UI event but
                    // never resumed the Gate-D await; 0.0.150 removes that avoidable UI work and adds
                    // durable D_* checkpoints inside the core audit itself.
                    UpdateStep35GateDProgress(value);
                    return;
                }

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
            WriteStep35CrashCheckpoint("A_STATIC_MAP_WRITE_RETURNED — run-correlated diagnostic static-map writer durably returned; writing comprehensive Godot/native reconnaissance before any CLR admission.");
            if (!WriteStep35GodotReconnaissance(out var godotReconError))
            {
                WriteStep35CrashCheckpoint($"A_GODOT_RECON_WRITE_FAILED_STOP — {godotReconError}");
                _transformedRealStS2VeryEarlyInitializationResultLabel.Text = "TRANSFORMED REAL STS2 VERY-EARLY INITIALIZATION: GODOT/NATIVE RECON TELEMETRY FAIL / STOPPED BEFORE GATE B";
                _transformedRealStS2VeryEarlyInitializationResultLabel.TextColor = UIColor.SystemRed;
                _transformedRealStS2VeryEarlyInitializationDetailLabel.Text = FormatTransformedRealStS2VeryEarlyInitializationDetail(
                    $"Gate A compatibility checks and in-memory reconnaissance passed, but the run-correlated Godot/native report could not be durably written. Gate B/C were intentionally not attempted. {godotReconError}");
                _statusLabel.Text = "STEP 35 DIAGNOSTIC STOP — comprehensive reconnaissance telemetry failed before CLR admission; this is not a compatibility FAIL.";
                _statusLabel.TextColor = UIColor.SystemRed;
                return;
            }
            WriteStep35CrashCheckpoint("A_GODOT_RECON_WRITE_RETURNED — run-correlated GodotSharp IL + Mach-O/native reconnaissance durably returned; about to select/schedule Gate B.");

            _transformedRealStS2VeryEarlyInitializationResultLabel.Text = "TRANSFORMED REAL STS2 VERY-EARLY INITIALIZATION: GATE B RUNNING…";
            _statusLabel.Text = exactAuthorityMode
                ? "STEP 35.0.27 GATE B — re-hash and CLR-admit the exact closed Step-32 transformed sts2 bytes; diagnostic derivatives remain evidence-only and outside the CLR authority path."
                : "STEP 35.0.27 GATE B — re-hash the exact transformed source, then CLR-admit only the separately verified diagnostic clone and re-check zero-resolution primary admission behavior.";
            WriteStep35CrashCheckpoint("B_SCHEDULE — Gate B UI selected; scheduling Gate B on Task.Run.");
            var gateB = await Task.Run(() => _transformedRealStS2VeryEarlyInitialization.RunExecutionCapableClrAdmission(WriteStep35CrashCheckpoint), token);
            WriteStep35CrashCheckpoint($"B_TASK_AWAIT_RESUMED — Gate B Task.Run await resumed on launcher thread; passed={gateB.Passed}.");
            WriteStep35CrashCheckpoint("B_RESULT_RECORD_START — about to record Gate B result in the ordered gate sequence/UI.");
            if (!RecordTransformedRealStS2VeryEarlyInitializationGate(gateB))
            {
                WriteStep35CrashCheckpoint("B_RESULT_FAIL_STOP — Gate B returned a managed FAIL; later gates will not run.");
                return;
            }
            WriteStep35CrashCheckpoint("B_RESULT_RECORD_PASS — Gate B PASS recorded; about to perform any explicit inter-gate callback handoff, then select Gate C.");

            if (callbackHandoffMode)
                RunStep35ManagedPluginBootstrap();

            _transformedRealStS2VeryEarlyInitializationResultLabel.Text = "TRANSFORMED REAL STS2 VERY-EARLY INITIALIZATION: GATE C RUNNING…";
            _statusLabel.Text = exactAuthorityMode
                ? "STEP 35.0.27 GATE C — invoke the exact closed transformed ExecuteVeryEarly once and await it; exact prepared GodotSharp uses the physically proven bridge. No diagnostic derivative is CLR input."
                : $"STEP 35.0.27 GATE C — mode={diagnosticMode}; invoke the verified sts2 diagnostic derivative once. CORE-HANDOFF uses the verified GodotSharp diagnostic derivative; native game loads, initializer-bearing and unplanned requests remain fail-closed.";
            WriteStep35CrashCheckpoint("C_UI_SELECTED — Gate C labels assigned on the main thread; UIKit may not have repainted before synchronous Gate-C work begins.");
            var gateC = await _transformedRealStS2VeryEarlyInitialization.RunDiagnosticExecuteVeryEarlyInvocationAsync(WriteStep35CrashCheckpoint, token);
            WriteStep35CrashCheckpoint($"C_TASK_AWAIT_RESUMED — Gate C async method returned to the UI caller; passed={gateC.Passed}.");
            if (!RecordTransformedRealStS2VeryEarlyInitializationGate(gateC))
            {
                WriteStep35CrashCheckpoint("C_RESULT_FAIL_STOP — Gate C returned a managed FAIL; Gate D will not run.");
                return;
            }
            WriteStep35CrashCheckpoint("C_RESULT_RECORD_PASS — Gate C PASS recorded; about to select Gate D.");

            _transformedRealStS2VeryEarlyInitializationResultLabel.Text = "TRANSFORMED REAL STS2 VERY-EARLY INITIALIZATION: GATE D RUNNING…";
            ResetStep35GateDProgress(visible: true);
            _statusLabel.Text = exactAuthorityMode
                ? "STEP 35.0.27 GATE D — exact-authority final reproof: OfflineReady, exact transformed CLR input, plan/dependency hashes, exact residency, and zero broader startup/native escape."
                : "STEP 35.0.27 GATE D — re-prove OfflineReady, exact-source/diagnostic-clone/plan/dependency hashes, diagnostic-clone residency, and zero broader startup/native escape.";
            WriteStep35CrashCheckpoint($"D_START — entering final isolation audit; exactAuthority={exactAuthorityMode}.");
            var gateD = await _transformedRealStS2VeryEarlyInitialization.RunFinalIsolationAuditAsync(progress, token, WriteStep35CrashCheckpoint);
            WriteStep35CrashCheckpoint($"D_TASK_AWAIT_RESUMED — Gate-D Task returned to the UI caller; passed={gateD.Passed}; exactAuthority={exactAuthorityMode}.");
            CompleteStep35GateDProgress(gateD.Passed, exactAuthorityMode ? "exact transformed authority" : "diagnostic derivative");
            WriteStep35CrashCheckpoint($"D_RESULT — passed={gateD.Passed}; gate={gateD.Gate}.");
            if (!RecordTransformedRealStS2VeryEarlyInitializationGate(gateD)) return;
            WriteStep35CrashCheckpoint("D_RESULT_RECORD_PASS — Gate D PASS recorded in the ordered gate sequence.");

            var snapshot = _transformedRealStS2VeryEarlyInitializationGates.Snapshot();
            _transformedRealStS2VeryEarlyInitializationResultLabel.TextColor = UIColor.Label;
            if (exactAuthorityMode)
            {
                _transformedRealStS2VeryEarlyInitializationResultLabel.Text = "EXACT STEP 35 CLOSURE CANDIDATE: COMPLETE — 4/4";
                _transformedRealStS2VeryEarlyInitializationDetailLabel.Text = FormatTransformedRealStS2VeryEarlyInitializationDetail(
                    "All four exact-authority gates completed. Gate B admitted the exact closed Step-32 transformed sts2 bytes rather than the Step-35 diagnostic derivative; exact prepared GodotSharp received the physically proven source-built Godot 4.5.1 bridge; exact ExecuteVeryEarly returned and awaited successfully; Gate D re-proved OfflineReady, hashes, resolver/native confinement, and CLR ownership. Under this explicitly defined launcher-host authority, this is the physical closure candidate for Step 35.");
                _statusLabel.Text = "EXACT STEP 35 CLOSURE CANDIDATE COMPLETE — 4/4. Preserve the run-correlated report/checkpoints before advancing to the next initialization phase.";
                WriteStep35CrashCheckpoint("RUN_EXACT_STEP35_4OF4 — exact closed transformed sts2 authority completed Gates A-D under the physically proven source-built Godot bridge prerequisite.");
            }
            else
            {
                _transformedRealStS2VeryEarlyInitializationResultLabel.Text = snapshot.Summary;
                _transformedRealStS2VeryEarlyInitializationDetailLabel.Text = FormatTransformedRealStS2VeryEarlyInitializationDetail(
                    "All four Step 35.0.27 diagnostic gates completed. Preserve this report, but do not use it as Step-35 closure evidence: Gate B/C executed an instrumented derivative rather than the exact closed Step-32 transformed bytes. The diagnostic 4/4 remains localization evidence; use a fresh process and the EXACT-CLOSURE button for the exact-authority attempt.");
                _statusLabel.Text = "DIAGNOSTIC COMPLETE: STEP 35.0.27 — 4/4. Use a fresh process + Step 15 + EXACT-CLOSURE for the exact-authority closure attempt.";
                WriteStep35CrashCheckpoint("RUN_DIAGNOSTIC_4OF4 — all Step-35.0.27 diagnostic gates completed; derivative result remains diagnostic evidence only.");
            }
            _statusLabel.TextColor = UIColor.Label;
        }
        catch (OperationCanceledException)
        {
            WriteStep35CrashCheckpoint("RUN_CANCELLED_INCONCLUSIVE — operator cancellation is not a compatibility FAIL; if Gate B/C began, this process is spent and must be force-quit before retry.");
            _transformedRealStS2VeryEarlyInitializationResultLabel.Text = "TRANSFORMED REAL STS2 VERY-EARLY INITIALIZATION: CANCELLED / INCONCLUSIVE";
            _transformedRealStS2VeryEarlyInitializationResultLabel.TextColor = UIColor.SecondaryLabel;
            _transformedRealStS2VeryEarlyInitializationDetailLabel.Text = FormatTransformedRealStS2VeryEarlyInitializationDetail(
                $"Step 35.0.27 was cancelled and is INCONCLUSIVE. If Gate B had begun, force-quit before retrying because the selected {(exactAuthorityMode ? "exact transformed authority" : "diagnostic derivative")} and dependencies may now be CLR-resident; if Gate C invocation began, ExecuteVeryEarly may also have executed despite cancellation.");
            _statusLabel.Text = exactAuthorityMode
                ? "STEP 35.0.27 EXACT-CLOSURE CANCELLED / INCONCLUSIVE — no closure verdict; force-quit before retry if Gate B or C started."
                : "STEP 35.0.27 DIAGNOSTIC CANCELLED / INCONCLUSIVE — exact Step 35 remains OPEN; force-quit before retry if Gate B or C started.";
            _statusLabel.TextColor = UIColor.SecondaryLabel;
        }
        catch (Exception ex)
        {
            WriteStep35CrashCheckpoint($"RUN_MANAGED_EXCEPTION — {ex.GetType().FullName}: {ex.Message}");
            _transformedRealStS2VeryEarlyInitializationResultLabel.Text = "TRANSFORMED REAL STS2 VERY-EARLY INITIALIZATION: EXCEPTION";
            _transformedRealStS2VeryEarlyInitializationResultLabel.TextColor = UIColor.SystemRed;
            _transformedRealStS2VeryEarlyInitializationDetailLabel.Text = FormatTransformedRealStS2VeryEarlyInitializationDetail($"Unhandled Step 35.0.27 {(exactAuthorityMode ? "exact-authority" : "diagnostic")} exception: {ex.GetType().Name}: {ex.Message}");
            _statusLabel.Text = exactAuthorityMode
                ? "STEP 35.0.27 EXACT-CLOSURE FAIL — preserve the exact-authority checkpoints; force-quit before retry if Gate B started."
                : "STEP 35.0.27 DIAGNOSTIC FAIL — preserve the evidence; this derivative failure is not by itself an exact Step-35 compatibility verdict. Force-quit before retry if Gate B started.";
            _statusLabel.TextColor = UIColor.SystemRed;
        }
        finally
        {
            StopStep35GateDHeartbeat();
            WriteStep35CrashCheckpoint("RUN_FINALLY_ENTER — managed control reached the Step-35 finally block; writing the normal deterministic report.");
            await WriteDeviceTestReportFromLabelsAsync(
                "Step35-TransformedRealStS2VeryEarlyInitialization.txt",
                "StS2 Launcher — Step 35.0.27 Exact-Authority Closure + Gate-D Finalization",
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
            _statusLabel.Text = $"STEP 35.0.27 FAIL at Gate {letter} ({result.Gate}). Stop here; later gates were not run. Preserve the run-correlated evidence and force-quit before retry.";
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
        lines.Add("Step 35.0.3 / 0.0.126 physical observation: same-run telemetry succeeded and the durable frontier again ended after the final planned System.Collections.Concurrent 8→9 host resolution with no C_INVOKE_RETURNED, so resolver traffic alone cannot distinguish the pre-first-await game callsite.");
        lines.Add("Step 35.0.6 / physical 0.0.129 observation: deferred-open bounded Cecil writing succeeded through Gate A/B and Gate C armed the callback, but the diagnostic bridge failed normally with MissingMethodException on synthetic Action<string>::Invoke(string) before INMETHOD_001. This was an instrumentation metadata defect, not a new exact-game compatibility frontier.");
        lines.Add("Step 35.0.7 / physical 0.0.130 observation: the ECMA-correct Action<string>::Invoke(!0) bridge worked. Durable markers proved ExecuteVeryEarly.MoveNext → TestMode.get_IsOn → SaveManager..cctor → SaveManager.get_Instance, then a second TestMode.get_IsOn from work under that getter. System.Text.Json and System.Collections.Concurrent 8→9 resolutions followed, then the process hard-terminated before InitSettingsDataForTest/InitSettingsData. No matching .ips is available for this run.");
        lines.Add("Step 35.0.8 / physical 0.0.131 observation: durable markers reached SaveManager.ConstructDefault, UserDataPathProvider.GetAccountScopedBasePath, PlatformUtil..cctor, and INMETHOD_024 — NullPlatformUtilStrategy..ctor entered. The last durable event was then the planned System.Collections.Concurrent 8.0.0.0 → host 9.0.0.0 binding. INMETHOD_025 — GodotFileIo..ctor never appeared, so the failure is physically inside work executed by NullPlatformUtilStrategy..ctor before that constructor call returns or begins.");
        lines.Add("Step 35.0.9 / physical 0.0.132 observation: the diagnostic clone emitted INMETHOD_NP003_PRE immediately before CommandLineHelper.TryGetValue and never emitted its POST or INMETHOD_027. The same-run exact-source [NULL PLATFORM CTOR IL] map identifies CommandLineHelper.TryGetValue as CALLSITE#002, so the run both localizes the hard-kill interval to type-initialization/call work triggered by TryGetValue and physically proves the +1 NP ordinal defect caused by counting the injected entry-marker bridge call. The final durable resolver event was System.Collections.Concurrent 8.0.0.0 → host 9.0.0.0; that remains contextual, not causal evidence.");
        lines.Add("Step 35.0.10 / physical 0.0.133 observation: NP ordinal accounting was corrected to INMETHOD_NP002_PRE, but no CommandLineHelper cctor entry/CL/CLTV marker executed. MethodInfo.Invoke returned a faulted Task whose nested cause was System.InvalidProgramException, and the launcher reached normal RUN_END. This is a diagnostic instrumentation defect: the live-stack sweep raised transient stack depth without raising the serialized cctor MaxStack header; it does not advance or retreat the exact-game frontier from 0.0.132.");
        lines.Add("Step 35.0.12 / physical 0.0.135 observation: verified cctor MaxStack headroom still produced the same pre-instruction-zero InvalidProgramException, disproving MaxStack-only causation and motivating retirement of all live-stack CL/CLTV runtime callbacks.");
        lines.Add("Step 35.0.13 / physical 0.0.136 observation: the stack-neutral-only clone entered CommandLineHelper..cctor and emitted INMETHOD_CL_CRITICAL_001_PRE before _args dictionary construction, then hard-terminated before CL_CRITICAL_001_POST, CL_CRITICAL_002_PRE, INMETHOD_027, NP002_POST, or C_INVOKE_RETURNED. The final durable resolver event was the planned System.Collections.Concurrent 8.0.0.0 -> host 9.0.0.0 binding; it remains contextual, while the PRE/no-POST pair physically localizes the interval to Godot.Collections.Dictionary<string,string> construction before assignment.");
        lines.Add("Step 35.0.27 comprehensive scope: preserve every prior exact source/resolver/later-startup authority and keep live-stack CL/CLTV callbacks retired. Gate A still performs read-only bundle-wide Mach-O dependency/rpath/symbol/string reconnaissance plus a GodotSharp IL/native-callback map and emits separately verified sts2 + GodotSharp diagnostic derivatives. Reconnaissance itself never loads or executes a native game image.");
        lines.Add("Physical 0.0.140 three-mode proof: NATURAL reached GS031 godot_dictionary::GetUnsafeAddress; OS-RECON passed CL_CRITICAL_001_POST and reached Godot.OS..cctor -> StringName.op_Implicit -> GS024 NativeFuncs.godotsharp_string_name_new_from_string; FORWARD passed CL_CRITICAL_002_POST, INMETHOD_027 and NP002_POST, then reached GodotFileIo.CreateDirectory -> Godot.DirAccess.DirExistsAbsolute -> StringName -> GS024. The repeated callback boundary is therefore not command-line-specific.");
        lines.Add("Physical 0.0.146 proved the coordinated managed-plugin bootstrap itself: the 37-pointer ManagedCallbacks table was created, ScriptManagerBridge.LookupScriptsInAssembly returned, native GDMonoCache adoption set godotApiCacheUpdated/createManagedBindingCallback/reverseBindingReady/externalBridgeInstalled true, and GD_OnCoreApiAssemblyLoaded returned. Gate C then failed before target binding because the old callback-handoff resolver snapshot did not account for the bootstrap's exact eight additional planned host-framework requests; initializer-bearing/rejected/native activity remained zero.");
        lines.Add("Physical 0.0.149 completed diagnostic Gate C: MethodInfo.Invoke returned, the Task was RanToCompletion, the await returned, post-await resolver/native confinement passed, and Gate C was recorded PASS. The UI then displayed Gate-D terminal 4/4 final-check progress for an extended interval while the durable journal remained at D_START; because that final progress was UI-only, 0.0.149 is not a formal Gate-D PASS. 0.0.150 adds durable D finalization checkpoints and the exact-authority closure mode.");
        lines.Add("Resolver boundary: exact persisted host-framework bindings and hash-pinned initializer-free prepared private dependencies may be serviced on demand. The known initializer-bearing 0Harmony 2.4.2.0 dependency remains forbidden; any changed/additional initializer-bearing dependency, unplanned managed request, or native game request fails closed.");
        lines.Add("Forbidden in every Step-35.0.27 mode: receipt-backed/prepared original sts2.dll CLR admission, intentional ExecuteEssential/ExecuteDeferred/PrewarmJit invocation by the launcher, game entry-point execution, Harmony/MonoMod API invocation or runtime patching, native game loading, arbitrary resolver fallback, or broad game startup sequencing. NATURAL/OS-RECON/FORWARD additionally forbid any Godot startup; CORE-HANDOFF and EXACT-CLOSURE permit only the already-proven Step-15 smoke engine plus the complete managed/native bridge bootstrap. EXACT-CLOSURE uniquely admits the exact closed transformed sts2 artifact and exact prepared GodotSharp as CLR authority inputs.");
        lines.Add("Cancellation semantics: CANCELLED is INCONCLUSIVE, not a compatibility FAIL. If Gate B has begun the process is spent; after Gate C invocation begins, cancellation cannot undo any code that already ran. Force-quit before retry.");
        lines.Add("After Gate B, the selected sts2 CLR input remains resident until force-quit on the physical non-collectible context. In diagnostic modes it is the instrumented derivative; in EXACT-CLOSURE it is the exact closed transformed artifact. After Gate C, the selected ExecuteVeryEarly has executed once; do not rerun Step 35.0.27 in the same process.");
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
            _step35GodotReconnaissancePath = null;
            _step35LastCheckpointPath = null;

            try
            {
                Directory.CreateDirectory(_deviceTestReportWriter.ReportsRoot);
                var initializedUtc = DateTimeOffset.UtcNow;
                var runId = $"{initializedUtc:yyyyMMddTHHmmssfffffffZ}-pid{Environment.ProcessId}-{Guid.NewGuid():N}";
                var crashFileName = $"Step35-CrashCheckpoint-{runId}.txt";
                var staticMapFileName = $"Step35-ExecuteVeryEarly-StaticMap-{runId}.txt";
                var godotReconFileName = $"Step35-GodotNativeReconnaissance-{runId}.txt";
                var crashPath = Path.Combine(_deviceTestReportWriter.ReportsRoot, crashFileName);
                var staticMapPath = Path.Combine(_deviceTestReportWriter.ReportsRoot, staticMapFileName);
                var godotReconPath = Path.Combine(_deviceTestReportWriter.ReportsRoot, godotReconFileName);
                var lastCheckpointPath = Path.Combine(_deviceTestReportWriter.ReportsRoot, Step35LastCheckpointFileName);
                var currentRunPath = Path.Combine(_deviceTestReportWriter.ReportsRoot, Step35CurrentRunFileName);

                WriteStep35TextFileDurably(
                    crashPath,
                    "StS2 Launcher — Step 35.0.27 Comprehensive GodotSharp / Native reconnaissance crash checkpoint\n" +
                    "Output-only diagnostic; never consumed as trusted runtime input.\n" +
                    $"Run ID: {runId}\n" +
                    $"Initialized UTC: {initializedUtc:O}\n" +
                    $"Process ID: {Environment.ProcessId}\n" +
                    $"App version: {CurrentReleasePresentation.DisplayVersion} ({CurrentReleasePresentation.DisplayBuild})\n" +
                    $"Expected source version: {CurrentReleasePresentation.ExpectedDisplayVersion} ({CurrentReleasePresentation.ExpectedBuildVersion})\n" +
                    "Candidate: STEP 35.0.27 — EXACT-AUTHORITY CLOSURE + GATE-D FINALIZATION\n" +
                    $"Diagnostic mode: {_transformedRealStS2VeryEarlyInitialization.DiagnosticMode}\n" +
                    "Execution policy: exact source transform/resolver/later-game-boundary prohibitions unchanged; Gate A performs read-only bundle-wide native/GodotSharp inspection and emits separately verified sts2 + GodotSharp diagnostic derivatives; GodotSharp probes are entry-only. NATURAL preserves the original Godot dictionary/native path; OS-RECON applies only the bounded four-reference BCL dictionary substitution and keeps Godot.OS natural; FORWARD adds exactly one local new string[0] provider substitution for Godot.OS.GetCmdlineArgs. Those three controls require a fresh process with no Godot state. CORE-HANDOFF is the sole explicit exception: it requires the already-proven Step-15 smoke engine, rejects dotnet-feature or Godot-managed-runtime state, and hands the exact source-built callback table to the private GodotSharp derivative. Native game loading remains forbidden.\n" +
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
                    $"Godot/native reconnaissance: {godotReconFileName}\n" +
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
                    $"Godot/native reconnaissance: {godotReconFileName}\n" +
                    $"Last checkpoint: {Step35LastCheckpointFileName}\n" +
                    $"Diagnostic mode: {_transformedRealStS2VeryEarlyInitialization.DiagnosticMode}\n" +
                    "Candidate: STEP 35.0.27 — EXACT-AUTHORITY CLOSURE + GATE-D FINALIZATION\n");

                _step35RunId = runId;
                _step35CrashCheckpointPath = crashPath;
                _step35StaticMapPath = staticMapPath;
                _step35GodotReconnaissancePath = godotReconPath;
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

    private bool WriteStep35GodotReconnaissance(out string error)
    {
        error = string.Empty;
        try
        {
            if (!_step35RunTelemetryReady || string.IsNullOrWhiteSpace(_step35RunId) || string.IsNullOrWhiteSpace(_step35GodotReconnaissancePath))
                throw new InvalidOperationException("Step-35 run telemetry is not initialized for Godot/native reconnaissance.");

            var body = _transformedRealStS2VeryEarlyInitialization.GetVerifiedGodotReconnaissanceReport();
            var markerMap = _transformedRealStS2VeryEarlyInitialization.GetVerifiedGodotSharpDiagnosticMarkerMap();
            WriteStep35TextFileDurably(
                _step35GodotReconnaissancePath,
                $"Run ID: {_step35RunId}\n" +
                $"Process ID: {Environment.ProcessId}\n" +
                $"App version: {CurrentReleasePresentation.DisplayVersion} ({CurrentReleasePresentation.DisplayBuild})\n" +
                $"Expected source version: {CurrentReleasePresentation.ExpectedDisplayVersion} ({CurrentReleasePresentation.ExpectedBuildVersion})\n" +
                $"Diagnostic mode: {_transformedRealStS2VeryEarlyInitialization.DiagnosticMode}\n" +
                "Candidate: STEP 35.0.27 — COMPREHENSIVE GODOTSHARP / NATIVE RECONNAISSANCE + EXACT-AUTHORITY CLOSURE\n" +
                "This file is output-only and is written before Gate B CLR admission.\n\n" +
                body +
                "\n\n[GODOTSHARP RUNTIME ENTRY-MARKER PLAN]\n" +
                markerMap + "\n");
            return true;
        }
        catch (Exception ex)
        {
            error = $"{ex.GetType().Name}: {ex.Message}";
            Console.Error.WriteLine($"Step-35 Godot/native reconnaissance write failed: {error}");
            return false;
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
                "Candidate: STEP 35.0.27 — same-run exact-source static map + GodotSharp callback-boundary five-mode closure candidate\n" +
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
                    string.IsNullOrWhiteSpace(_step35GodotReconnaissancePath) ||
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
                    $"Godot/native reconnaissance: {Path.GetFileName(_step35GodotReconnaissancePath)}\n" +
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
