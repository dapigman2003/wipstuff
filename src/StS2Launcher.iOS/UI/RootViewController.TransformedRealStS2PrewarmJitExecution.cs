using StS2Launcher.Core;

namespace StS2Launcher.iOS;

public sealed partial class RootViewController
{
    private readonly TransformedRealStS2PrewarmJitExecutionGateSequence _transformedRealStS2PrewarmJitExecutionGates = new();
    private UILabel? _transformedRealStS2PrewarmJitExecutionResultLabel;
    private UILabel? _transformedRealStS2PrewarmJitExecutionDetailLabel;
    private UIButton? _transformedRealStS2PrewarmJitExecutionButton;

    private void AddTransformedRealStS2PrewarmJitExecutionControls(UIStackView content)
    {
        content.AddArrangedSubview(Label(
            "Step 34.0 — Controlled Transformed Real-StS2 PrewarmJit Execution (ordered gates A–D)",
            UIFont.BoldSystemFontOfSize(18),
            UIColor.Label));

        _transformedRealStS2PrewarmJitExecutionButton = SystemButton(
            "Run Step 34 A–D — Reverify Transform → Admit Exact Transformed sts2.dll → Invoke Exact PrewarmJit Once → Audit Isolation",
            UIButtonType.System);
        _transformedRealStS2PrewarmJitExecutionButton.TouchUpInside += async (_, _) => await RunTransformedRealStS2PrewarmJitExecutionAsync();
        content.AddArrangedSubview(_transformedRealStS2PrewarmJitExecutionButton);

        _transformedRealStS2PrewarmJitExecutionResultLabel = Label(
            "TRANSFORMED REAL STS2 PREWARMJIT EXECUTION: NOT RUN",
            UIFont.BoldSystemFontOfSize(15),
            UIColor.SecondaryLabel);
        content.AddArrangedSubview(_transformedRealStS2PrewarmJitExecutionResultLabel);

        _transformedRealStS2PrewarmJitExecutionDetailLabel = Label(
            "Physical 0.0.121 closed Step 33 at 4/4: the exact Step-32 transformed sts2.dll entered its dedicated iOS CLR context with zero resolver requests, zero private dependency admission, zero native loading and no game-member invocation. Step 34 is the first controlled transformed-site execution boundary. Gate A re-manufactures/reverifies the exact closed transform and requalifies the prepared runtime plan. Gate B re-establishes the Step-33 transformed-primary admission state in an execution-capable strict context. Gate C binds only MegaCrit.Sts2.Core.Helpers.OneTimeInitialization::PrewarmJit() and invokes it exactly once; only exact persisted host bindings and hash-pinned initializer-free prepared dependencies may resolve. Initializer-bearing dependencies including 0Harmony, unplanned managed requests and native requests remain fail-closed. Gate D re-proves source/transformed/plan/dependency/context isolation. No entry point, Harmony API, Godot startup or broader game initialization is authorized.",
            UIFont.SystemFontOfSize(13),
            UIColor.SecondaryLabel);
        content.AddArrangedSubview(_transformedRealStS2PrewarmJitExecutionDetailLabel);
    }

    private async Task RunTransformedRealStS2PrewarmJitExecutionAsync()
    {
        if (_transformedRealStS2PrewarmJitExecutionResultLabel is null ||
            _transformedRealStS2PrewarmJitExecutionDetailLabel is null ||
            _transformedRealStS2PrewarmJitExecutionButton is null ||
            _statusLabel is null)
            return;

        if (!CurrentReleasePresentation.BundleIdentityMatchesExpected)
        {
            _transformedRealStS2PrewarmJitExecutionResultLabel.Text = "TRANSFORMED REAL STS2 PREWARMJIT EXECUTION: RELEASE IDENTITY FAIL";
            _transformedRealStS2PrewarmJitExecutionResultLabel.TextColor = UIColor.SystemRed;
            _transformedRealStS2PrewarmJitExecutionDetailLabel.Text =
                $"Expected {CurrentReleasePresentation.ExpectedDisplayVersion} ({CurrentReleasePresentation.ExpectedBuildVersion}), observed {CurrentReleasePresentation.DisplayVersion} ({CurrentReleasePresentation.DisplayBuild}). Refusing Step 34 so execution evidence cannot be attributed to the wrong candidate.";
            _statusLabel.Text = "STEP 34 REFUSED — built bundle identity does not match the source-pinned candidate.";
            _statusLabel.TextColor = UIColor.SystemRed;
            return;
        }

        if (_godotProcessRequiresRestart || _godotSessionStarted)
        {
            _statusLabel.Text = "Step 34 requires a fresh process with no Godot process-global state and no sts2 assembly already loaded. Force-quit/relaunch before running Step 34.";
            _statusLabel.TextColor = UIColor.SystemOrange;
            return;
        }

        BeginSteamOperation(allowCancel: true);
        _transformedRealStS2PrewarmJitExecutionGates.Reset();
        _transformedRealStS2PrewarmJitExecution.Reset();
        _transformedRealStS2PrewarmJitExecutionResultLabel.Text = "TRANSFORMED REAL STS2 PREWARMJIT EXECUTION: GATE A RUNNING…";
        _transformedRealStS2PrewarmJitExecutionResultLabel.TextColor = UIColor.Label;
        _statusLabel.Text = "STEP 34 GATE A — re-manufacture/reverify exact closed transformed image and requalify the strict execution resolver plan; no CLR admission yet.";
        _statusLabel.TextColor = UIColor.Label;

        try
        {
            var token = _operationCts?.Token ?? CancellationToken.None;
            var progress = new Progress<TransformedRealStS2PrewarmJitExecutionProgress>(value =>
            {
                var count = value.TotalItems > 0 ? $" ({value.ProcessedItems:N0}/{value.TotalItems:N0})" : string.Empty;
                _transformedRealStS2PrewarmJitExecutionDetailLabel.Text = FormatTransformedRealStS2PrewarmJitExecutionDetail(
                    $"Gate {(char)('A' + (int)value.Gate - 1)} progress{count}: {value.Detail}" +
                    (string.IsNullOrWhiteSpace(value.CurrentPath) ? string.Empty : $"\nCurrent: {value.CurrentPath}"));
            });

            var gateA = await _transformedRealStS2PrewarmJitExecution.RunVerifiedExecutionPreflightAsync(progress, token);
            if (!RecordTransformedRealStS2PrewarmJitExecutionGate(gateA)) return;

            _transformedRealStS2PrewarmJitExecutionResultLabel.Text = "TRANSFORMED REAL STS2 PREWARMJIT EXECUTION: GATE B RUNNING…";
            _statusLabel.Text = "STEP 34 GATE B — CLR-admit only the exact transformed sts2.dll into the strict execution context and re-prove Step-33 zero-resolution admission behavior.";
            var gateB = await Task.Run(() => _transformedRealStS2PrewarmJitExecution.RunExecutionCapableClrAdmission(), token);
            if (!RecordTransformedRealStS2PrewarmJitExecutionGate(gateB)) return;

            _transformedRealStS2PrewarmJitExecutionResultLabel.Text = "TRANSFORMED REAL STS2 PREWARMJIT EXECUTION: GATE C RUNNING…";
            _statusLabel.Text = "STEP 34 GATE C — bind the exact transformed PrewarmJit method and invoke it once; log every managed/private/native boundary and fail closed on anything unplanned.";
            var gateC = await Task.Run(() => _transformedRealStS2PrewarmJitExecution.RunExactPrewarmJitInvocation(), token);
            if (!RecordTransformedRealStS2PrewarmJitExecutionGate(gateC)) return;

            _transformedRealStS2PrewarmJitExecutionResultLabel.Text = "TRANSFORMED REAL STS2 PREWARMJIT EXECUTION: GATE D RUNNING…";
            _statusLabel.Text = "STEP 34 GATE D — re-prove OfflineReady, source/transformed/plan/dependency hashes, exact residency and zero broader startup/native escape.";
            var gateD = await _transformedRealStS2PrewarmJitExecution.RunFinalIsolationAuditAsync(progress, token);
            if (!RecordTransformedRealStS2PrewarmJitExecutionGate(gateD)) return;

            var snapshot = _transformedRealStS2PrewarmJitExecutionGates.Snapshot();
            _transformedRealStS2PrewarmJitExecutionResultLabel.Text = snapshot.Summary;
            _transformedRealStS2PrewarmJitExecutionResultLabel.TextColor = UIColor.Label;
            _transformedRealStS2PrewarmJitExecutionDetailLabel.Text = FormatTransformedRealStS2PrewarmJitExecutionDetail(
                "All four Step 34 gates passed. Preserve this report. A pass proves the exact transformed real-StS2 PrewarmJit compatibility site can be invoked once on physical iOS under the strict prepared resolver without admitting the trusted original, initializer-bearing dependencies, unplanned managed/native code, the game entry point, Harmony patching, or Godot/game startup. The next boundary must remain separately gated and measured.");
            _statusLabel.Text = "PASS: STEP 34 CONTROLLED TRANSFORMED PREWARMJIT EXECUTION — 4/4. Exact transformed compatibility site returned normally; broader startup remains unauthorized.";
            _statusLabel.TextColor = UIColor.Label;
        }
        catch (OperationCanceledException)
        {
            _transformedRealStS2PrewarmJitExecutionResultLabel.Text = "TRANSFORMED REAL STS2 PREWARMJIT EXECUTION: CANCELLED";
            _transformedRealStS2PrewarmJitExecutionResultLabel.TextColor = UIColor.SecondaryLabel;
            _transformedRealStS2PrewarmJitExecutionDetailLabel.Text = FormatTransformedRealStS2PrewarmJitExecutionDetail(
                "Step 34 was cancelled. If Gate B had begun, force-quit before retrying because transformed sts2 and any initializer-free dependencies may now be CLR-resident.");
            _statusLabel.Text = "STEP 34 CANCELLED — later gates are unproven; force-quit before retry if Gate B started.";
            _statusLabel.TextColor = UIColor.SecondaryLabel;
        }
        catch (Exception ex)
        {
            _transformedRealStS2PrewarmJitExecutionResultLabel.Text = "TRANSFORMED REAL STS2 PREWARMJIT EXECUTION: EXCEPTION";
            _transformedRealStS2PrewarmJitExecutionResultLabel.TextColor = UIColor.SystemRed;
            _transformedRealStS2PrewarmJitExecutionDetailLabel.Text = FormatTransformedRealStS2PrewarmJitExecutionDetail($"Unhandled Step 34 exception: {ex.GetType().Name}: {ex.Message}");
            _statusLabel.Text = "STEP 34 FAIL — stop at the first failing gate and preserve the report. Force-quit before retry if Gate B started.";
            _statusLabel.TextColor = UIColor.SystemRed;
        }
        finally
        {
            await WriteDeviceTestReportFromLabelsAsync(
                "Step34-TransformedRealStS2PrewarmJitExecution.txt",
                "StS2 Launcher — Step 34 Transformed Real StS2 PrewarmJit Execution",
                _transformedRealStS2PrewarmJitExecutionResultLabel,
                _transformedRealStS2PrewarmJitExecutionDetailLabel,
                CancellationToken.None);
            EndSteamOperation();
        }
    }

    private bool RecordTransformedRealStS2PrewarmJitExecutionGate(TransformedRealStS2PrewarmJitExecutionGateResult result)
    {
        _transformedRealStS2PrewarmJitExecutionGates.Record(result);
        if (_transformedRealStS2PrewarmJitExecutionResultLabel is not null)
        {
            _transformedRealStS2PrewarmJitExecutionResultLabel.Text = _transformedRealStS2PrewarmJitExecutionGates.Snapshot().Summary;
            _transformedRealStS2PrewarmJitExecutionResultLabel.TextColor = result.Passed ? UIColor.Label : UIColor.SystemRed;
        }
        if (_transformedRealStS2PrewarmJitExecutionDetailLabel is not null)
            _transformedRealStS2PrewarmJitExecutionDetailLabel.Text = FormatTransformedRealStS2PrewarmJitExecutionDetail(result.Detail);
        if (!result.Passed && _statusLabel is not null)
        {
            var letter = (char)('A' + (int)result.Gate - 1);
            _statusLabel.Text = $"STEP 34 FAIL at Gate {letter} ({result.Gate}). Stop here; later execution gates were not run. Preserve the report and force-quit before retry.";
            _statusLabel.TextColor = UIColor.SystemRed;
        }
        return result.Passed;
    }

    private string FormatTransformedRealStS2PrewarmJitExecutionDetail(string tail)
    {
        var lines = new List<string>();
        foreach (var gate in _transformedRealStS2PrewarmJitExecutionGates.Results)
        {
            var letter = (char)('A' + (int)gate.Gate - 1);
            lines.Add($"Gate {letter} — {gate.Gate}: {(gate.Passed ? "PASS" : "FAIL")}");
            lines.Add(gate.Detail);
            lines.Add(string.Empty);
        }

        lines.Add("Step 32 physical baseline: CLOSED POSITIVE — 0.0.120 passed 4/4. Exact transformed SHA-256 39c0a89ad0d5c6eb1553e23dd8537a7b7ab8278fad4115d186db5751570211ef; transformed semantic fingerprint 47fadf2a46eda098f310b7d0ee54e37d1e952ac272fc966d16d557ed46a0b74a; transformed PrewarmJit token 0x0600AFEA; zero PrepareMethod references; trusted install unchanged.");
        lines.Add("Step 33 physical baseline: CLOSED POSITIVE — 0.0.121 passed 4/4. Only the exact transformed primary entered StS2Launcher-Step33-TransformedGame; admission caused zero managed resolver requests, zero private dependency loads and zero native attempts; no game member was reflected or invoked.");
        lines.Add("Step 34 scope: re-establish the exact transformed-primary state in a strict execution-capable private context, then reflect and invoke only MegaCrit.Sts2.Core.Helpers.OneTimeInitialization::PrewarmJit() exactly once.");
        lines.Add("Resolver boundary: exact persisted host-framework bindings and hash-pinned initializer-free prepared private dependencies may be serviced on demand. The known initializer-bearing 0Harmony 2.4.2.0 dependency remains forbidden; any changed/additional initializer-bearing dependency, unplanned managed request, or native request fails closed.");
        lines.Add("Forbidden in Step 34: receipt-backed/prepared original sts2.dll CLR admission, any intentional game method other than exact PrewarmJit, game entry-point execution, Harmony/MonoMod API invocation or runtime patching, Godot/game startup, native game loading, arbitrary resolver fallback, or broad startup sequencing.");
        lines.Add("After Gate B, transformed sts2 remains CLR-resident until force-quit on the physical non-collectible context. After Gate C, PrewarmJit has executed once; do not rerun Step 34 in the same process.");
        lines.Add(tail);
        return string.Join("\n", lines);
    }
}
