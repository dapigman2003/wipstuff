using StS2Launcher.Core;

namespace StS2Launcher.iOS;

public sealed partial class RootViewController
{
    private readonly TransformedRealStS2VeryEarlyInitializationGateSequence _transformedRealStS2VeryEarlyInitializationGates = new();
    private UILabel? _transformedRealStS2VeryEarlyInitializationResultLabel;
    private UILabel? _transformedRealStS2VeryEarlyInitializationDetailLabel;
    private UIButton? _transformedRealStS2VeryEarlyInitializationButton;

    private void AddTransformedRealStS2VeryEarlyInitializationControls(UIStackView content)
    {
        content.AddArrangedSubview(Label(
            "Step 35.0 — Controlled Transformed Real-StS2 Very-Early Initialization (ordered gates A–D)",
            UIFont.BoldSystemFontOfSize(18),
            UIColor.Label));

        _transformedRealStS2VeryEarlyInitializationButton = SystemButton(
            "Run Step 35 A–D — Reverify Very-Early Target → Admit Exact Transformed sts2.dll → Invoke + Await ExecuteVeryEarly Once → Audit Isolation",
            17);
        _transformedRealStS2VeryEarlyInitializationButton.TouchUpInside += async (_, _) => await RunTransformedRealStS2VeryEarlyInitializationAsync();
        content.AddArrangedSubview(_transformedRealStS2VeryEarlyInitializationButton);

        _transformedRealStS2VeryEarlyInitializationResultLabel = Label(
            "TRANSFORMED REAL STS2 VERY-EARLY INITIALIZATION: NOT RUN",
            UIFont.BoldSystemFontOfSize(15),
            UIColor.SecondaryLabel);
        content.AddArrangedSubview(_transformedRealStS2VeryEarlyInitializationResultLabel);

        _transformedRealStS2VeryEarlyInitializationDetailLabel = Label(
            "Physical 0.0.122 closed Step 34 at 4/4: exact transformed OneTimeInitialization::PrewarmJit() returned normally once under the strict prepared resolver, with 8 managed resolver requests, 6 exact host-framework loads, 2 initializer-free prepared dependency loads, zero initializer-bearing requests and zero native/unplanned escape. Step 35 begins the real managed startup sequence at its earliest measured one-time boundary. Gate A re-manufactures/reverifies the exact closed transform, proves source token 0x06007D02 is static parameterless Task-returning OneTimeInitialization::ExecuteVeryEarly(), and proves its async MoveNext semantics are unchanged by Step 32 with no direct ExecuteEssential/ExecuteDeferred/PrewarmJit or Harmony call. Gate B admits only the exact transformed primary. Gate C invokes ExecuteVeryEarly exactly once and awaits the returned Task for at most 60 seconds under the same fail-closed resolver. Gate D re-proves source/transformed/plan/dependency/context isolation. ExecuteEssential, ExecuteDeferred, the game entry point, Harmony, native loading and Godot/game startup remain separate later boundaries.",
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

        BeginSteamOperation(allowCancel: true);
        _transformedRealStS2VeryEarlyInitializationGates.Reset();
        _transformedRealStS2VeryEarlyInitialization.Reset();
        _transformedRealStS2VeryEarlyInitializationResultLabel.Text = "TRANSFORMED REAL STS2 VERY-EARLY INITIALIZATION: GATE A RUNNING…";
        _transformedRealStS2VeryEarlyInitializationResultLabel.TextColor = UIColor.Label;
        _statusLabel.Text = "STEP 35 GATE A — re-manufacture/reverify the closed transform, prove exact ExecuteVeryEarly wrapper/async-state-machine preservation, and requalify the strict resolver plan; no CLR admission yet.";
        _statusLabel.TextColor = UIColor.Label;

        try
        {
            var token = _operationCts?.Token ?? CancellationToken.None;
            var progress = new Progress<TransformedRealStS2VeryEarlyInitializationProgress>(value =>
            {
                var count = value.TotalItems > 0 ? $" ({value.ProcessedItems:N0}/{value.TotalItems:N0})" : string.Empty;
                _transformedRealStS2VeryEarlyInitializationDetailLabel.Text = FormatTransformedRealStS2VeryEarlyInitializationDetail(
                    $"Gate {(char)('A' + (int)value.Gate - 1)} progress{count}: {value.Detail}" +
                    (string.IsNullOrWhiteSpace(value.CurrentPath) ? string.Empty : $"\nCurrent: {value.CurrentPath}"));
            });

            var gateA = await _transformedRealStS2VeryEarlyInitialization.RunVerifiedExecutionPreflightAsync(progress, token);
            if (!RecordTransformedRealStS2VeryEarlyInitializationGate(gateA)) return;

            _transformedRealStS2VeryEarlyInitializationResultLabel.Text = "TRANSFORMED REAL STS2 VERY-EARLY INITIALIZATION: GATE B RUNNING…";
            _statusLabel.Text = "STEP 35 GATE B — CLR-admit only the exact transformed sts2.dll into the strict execution context and re-prove Step-33 zero-resolution admission behavior.";
            var gateB = await Task.Run(() => _transformedRealStS2VeryEarlyInitialization.RunExecutionCapableClrAdmission(), token);
            if (!RecordTransformedRealStS2VeryEarlyInitializationGate(gateB)) return;

            _transformedRealStS2VeryEarlyInitializationResultLabel.Text = "TRANSFORMED REAL STS2 VERY-EARLY INITIALIZATION: GATE C RUNNING…";
            _statusLabel.Text = "STEP 35 GATE C — bind exact transformed ExecuteVeryEarly(), invoke it once, await its Task to completion (60s max), and fail closed on initializer-bearing/unplanned/native requests.";
            var gateC = await _transformedRealStS2VeryEarlyInitialization.RunExactExecuteVeryEarlyInvocationAsync(token);
            if (!RecordTransformedRealStS2VeryEarlyInitializationGate(gateC)) return;

            _transformedRealStS2VeryEarlyInitializationResultLabel.Text = "TRANSFORMED REAL STS2 VERY-EARLY INITIALIZATION: GATE D RUNNING…";
            _statusLabel.Text = "STEP 35 GATE D — re-prove OfflineReady, source/transformed/plan/dependency hashes, exact residency and zero broader startup/native escape.";
            var gateD = await _transformedRealStS2VeryEarlyInitialization.RunFinalIsolationAuditAsync(progress, token);
            if (!RecordTransformedRealStS2VeryEarlyInitializationGate(gateD)) return;

            var snapshot = _transformedRealStS2VeryEarlyInitializationGates.Snapshot();
            _transformedRealStS2VeryEarlyInitializationResultLabel.Text = snapshot.Summary;
            _transformedRealStS2VeryEarlyInitializationResultLabel.TextColor = UIColor.Label;
            _transformedRealStS2VeryEarlyInitializationDetailLabel.Text = FormatTransformedRealStS2VeryEarlyInitializationDetail(
                "All four Step 35 gates passed. Preserve this report. A pass proves the earliest measured real-StS2 one-time startup boundary, ExecuteVeryEarly(), can be invoked once and its exact Task awaited to normal completion on physical iOS under the strict prepared resolver. ExecuteEssential, ExecuteDeferred, the game entry point, Harmony/native loading and Godot/game startup remain separately gated.");
            _statusLabel.Text = "PASS: STEP 35 CONTROLLED VERY-EARLY INITIALIZATION — 4/4. ExecuteVeryEarly Task completed normally; ExecuteEssential/ExecuteDeferred and broader startup remain unauthorized.";
            _statusLabel.TextColor = UIColor.Label;
        }
        catch (OperationCanceledException)
        {
            _transformedRealStS2VeryEarlyInitializationResultLabel.Text = "TRANSFORMED REAL STS2 VERY-EARLY INITIALIZATION: CANCELLED";
            _transformedRealStS2VeryEarlyInitializationResultLabel.TextColor = UIColor.SecondaryLabel;
            _transformedRealStS2VeryEarlyInitializationDetailLabel.Text = FormatTransformedRealStS2VeryEarlyInitializationDetail(
                "Step 35 was cancelled. If Gate B had begun, force-quit before retrying because transformed sts2 and any initializer-free dependencies may now be CLR-resident.");
            _statusLabel.Text = "STEP 35 CANCELLED — later gates are unproven; force-quit before retry if Gate B started.";
            _statusLabel.TextColor = UIColor.SecondaryLabel;
        }
        catch (Exception ex)
        {
            _transformedRealStS2VeryEarlyInitializationResultLabel.Text = "TRANSFORMED REAL STS2 VERY-EARLY INITIALIZATION: EXCEPTION";
            _transformedRealStS2VeryEarlyInitializationResultLabel.TextColor = UIColor.SystemRed;
            _transformedRealStS2VeryEarlyInitializationDetailLabel.Text = FormatTransformedRealStS2VeryEarlyInitializationDetail($"Unhandled Step 35 exception: {ex.GetType().Name}: {ex.Message}");
            _statusLabel.Text = "STEP 35 FAIL — stop at the first failing gate and preserve the report. Force-quit before retry if Gate B started.";
            _statusLabel.TextColor = UIColor.SystemRed;
        }
        finally
        {
            await WriteDeviceTestReportFromLabelsAsync(
                "Step35-TransformedRealStS2VeryEarlyInitialization.txt",
                "StS2 Launcher — Step 35 Transformed Real StS2 Very-Early Initialization",
                _transformedRealStS2VeryEarlyInitializationResultLabel,
                _transformedRealStS2VeryEarlyInitializationDetailLabel,
                CancellationToken.None);
            EndSteamOperation();
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
        lines.Add("Step 35 scope: begin the natural managed startup sequence at exact static parameterless Task-returning MegaCrit.Sts2.Core.Helpers.OneTimeInitialization::ExecuteVeryEarly(), source token 0x06007D02. Re-prove its unchanged async state machine, admit only the exact transformed primary, invoke once and await the returned Task for at most 60 seconds.");
        lines.Add("Resolver boundary: exact persisted host-framework bindings and hash-pinned initializer-free prepared private dependencies may be serviced on demand. The known initializer-bearing 0Harmony 2.4.2.0 dependency remains forbidden; any changed/additional initializer-bearing dependency, unplanned managed request, or native request fails closed.");
        lines.Add("Forbidden in Step 35: receipt-backed/prepared original sts2.dll CLR admission, intentional ExecuteEssential/ExecuteDeferred/PrewarmJit invocation by the launcher, game entry-point execution, Harmony/MonoMod API invocation or runtime patching, Godot/game startup, native game loading, arbitrary resolver fallback, or broad startup sequencing.");
        lines.Add("After Gate B, transformed sts2 remains CLR-resident until force-quit on the physical non-collectible context. After Gate C, ExecuteVeryEarly has executed once; do not rerun Step 35 in the same process.");
        lines.Add(tail);
        return string.Join("\n", lines);
    }
}
