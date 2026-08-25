using StS2Launcher.Core;
using UIKit;

namespace StS2Launcher.iOS;

public sealed partial class RootViewController
{
    private UILabel? _realStS2CompatibilityTargetAuditResultLabel;
    private UILabel? _realStS2CompatibilityTargetAuditDetailLabel;
    private UIButton? _realStS2CompatibilityTargetAuditButton;

    private void AddRealStS2CompatibilityTargetAuditControls(UIStackView content)
    {
        content.AddArrangedSubview(Separator());
        content.AddArrangedSubview(Label(
            "Step 29.0 — Real StS2 Compatibility Target Audit (ordered gates A–D)",
            UIFont.BoldSystemFontOfSize(25),
            UIColor.Label));

        _realStS2CompatibilityTargetAuditButton = SystemButton(
            "Run Step 29 A–D — Admit Real sts2 Metadata → Audit Exact IL → Select One Candidate → Re-Prove Isolation",
            17);
        _realStS2CompatibilityTargetAuditButton.TouchUpInside += async (_, _) => await RunRealStS2CompatibilityTargetAuditAsync();
        content.AddArrangedSubview(_realStS2CompatibilityTargetAuditButton);

        _realStS2CompatibilityTargetAuditResultLabel = Label(
            "REAL STS2 COMPATIBILITY TARGET AUDIT: NOT RUN",
            UIFont.BoldSystemFontOfSize(21),
            UIColor.Label);
        content.AddArrangedSubview(_realStS2CompatibilityTargetAuditResultLabel);

        _realStS2CompatibilityTargetAuditDetailLabel = Label(
            "Physical 0.0.111 closed Step 28 at 5/5 with exact 1000 / 1041 / 1041 transformed execution, source/transformed hash isolation, and OfflineReady 428/428 after execution. Step 29 does not guess the first real game patch from broad historical categories. Gate A re-proves OfflineReady and opens only the exact receipt-backed macOS arm64 sts2.dll as deferred Cecil metadata. Gate B fingerprints concrete direct compatibility-risk call sites without dependency resolution. Gate C deterministically selects at most one audit candidate under the post-Step-28 priority policy. Gate D re-hashes the source and re-proves OfflineReady. This build performs zero Cecil writes and never CLR-loads or invokes sts2.dll.",
            UIFont.SystemFontOfSize(15),
            UIColor.SecondaryLabel);
        content.AddArrangedSubview(_realStS2CompatibilityTargetAuditDetailLabel);
    }

    private async Task RunRealStS2CompatibilityTargetAuditAsync()
    {
        if (_realStS2CompatibilityTargetAuditResultLabel is null ||
            _realStS2CompatibilityTargetAuditDetailLabel is null ||
            _realStS2CompatibilityTargetAuditButton is null ||
            _statusLabel is null)
        {
            return;
        }

        if (!CurrentReleasePresentation.BundleIdentityMatchesExpected)
        {
            _realStS2CompatibilityTargetAuditResultLabel.Text = "REAL STS2 COMPATIBILITY TARGET AUDIT: RELEASE IDENTITY FAIL";
            _realStS2CompatibilityTargetAuditResultLabel.TextColor = UIColor.SystemRed;
            _realStS2CompatibilityTargetAuditDetailLabel.Text =
                $"Expected {CurrentReleasePresentation.ExpectedDisplayVersion} ({CurrentReleasePresentation.ExpectedBuildVersion}), observed {CurrentReleasePresentation.DisplayVersion} ({CurrentReleasePresentation.DisplayBuild}). Refusing Step 29 so physical target-selection evidence cannot be attributed to the wrong source candidate.";
            _statusLabel.Text = "STEP 29 REFUSED — built bundle identity does not match the source-pinned candidate.";
            _statusLabel.TextColor = UIColor.SystemRed;
            return;
        }

        if (_godotProcessRequiresRestart)
        {
            _statusLabel.Text = "Step 15 Godot process-global state is active. Force-quit/relaunch before Step 29 so real-StS2 metadata evidence is isolated from the Godot session.";
            _statusLabel.TextColor = UIColor.SystemOrange;
            return;
        }

        BeginSteamOperation(allowCancel: true);
        _realStS2CompatibilityTargetAuditGates.Reset();
        _realStS2CompatibilityTargetAudit.Reset();
        _realStS2CompatibilityTargetAuditResultLabel.Text = "REAL STS2 COMPATIBILITY TARGET AUDIT: GATE A RUNNING…";
        _realStS2CompatibilityTargetAuditResultLabel.TextColor = UIColor.Label;
        _realStS2CompatibilityTargetAuditDetailLabel.Text = "Gate A: re-proving OfflineReady and admitting the exact receipt-backed macOS arm64 sts2.dll as deferred Cecil metadata only; a fresh process is required and sts2 must remain absent from the CLR.";
        _statusLabel.Text = "STEP 29 GATE A — receipt-backed real sts2 metadata admission + OfflineReady.";
        _statusLabel.TextColor = UIColor.Label;

        try
        {
            var token = _operationCts?.Token ?? CancellationToken.None;
            var progress = new Progress<RealStS2CompatibilityTargetAuditProgress>(value =>
            {
                var count = value.TotalItems > 0 ? $" ({value.ProcessedItems:N0}/{value.TotalItems:N0})" : string.Empty;
                _realStS2CompatibilityTargetAuditDetailLabel.Text = FormatRealStS2CompatibilityTargetAuditDetail(
                    $"Gate {(char)('A' + (int)value.Gate - 1)} progress{count}: {value.Detail}" +
                    (string.IsNullOrWhiteSpace(value.CurrentPath) ? string.Empty : $"\nCurrent: {value.CurrentPath}"));
            });

            var gateA = await _realStS2CompatibilityTargetAudit.RunSourceAdmissionAndOfflineReadyAsync(progress, token);
            if (!RecordRealStS2CompatibilityTargetAuditGate(gateA)) return;

            _realStS2CompatibilityTargetAuditResultLabel.Text = "REAL STS2 COMPATIBILITY TARGET AUDIT: GATE B RUNNING…";
            _statusLabel.Text = "STEP 29 GATE B — exact primary sts2.dll compatibility-risk IL audit; no resolution or execution.";
            var gateB = await Task.Run(() => _realStS2CompatibilityTargetAudit.RunExactRiskCallSiteAudit(), token);
            if (!RecordRealStS2CompatibilityTargetAuditGate(gateB)) return;

            _realStS2CompatibilityTargetAuditResultLabel.Text = "REAL STS2 COMPATIBILITY TARGET AUDIT: GATE C RUNNING…";
            _statusLabel.Text = "STEP 29 GATE C — deterministic audit-candidate selection only; no Cecil write.";
            var gateC = await Task.Run(() => _realStS2CompatibilityTargetAudit.RunDeterministicCandidateSelection(), token);
            if (!RecordRealStS2CompatibilityTargetAuditGate(gateC)) return;

            _realStS2CompatibilityTargetAuditResultLabel.Text = "REAL STS2 COMPATIBILITY TARGET AUDIT: GATE D RUNNING…";
            _statusLabel.Text = "STEP 29 GATE D — source hashes + OfflineReady + no-CLR-load isolation audit.";
            var gateD = await _realStS2CompatibilityTargetAudit.RunFinalIsolationAuditAsync(progress, token);
            if (!RecordRealStS2CompatibilityTargetAuditGate(gateD)) return;

            var snapshot = _realStS2CompatibilityTargetAuditGates.Snapshot();
            _realStS2CompatibilityTargetAuditResultLabel.Text = snapshot.Summary;
            _realStS2CompatibilityTargetAuditResultLabel.TextColor = UIColor.Label;
            _realStS2CompatibilityTargetAuditDetailLabel.Text = FormatRealStS2CompatibilityTargetAuditDetail(
                "All four Step 29.0 audit gates passed. Preserve this report. The selected exact source method/token/IL offset/target/body fingerprint, if any, is evidence for the next candidate; this build intentionally performs zero real-StS2 transformation so the semantic change can be designed from the actual receipt-backed IL rather than guessed.");
            _statusLabel.Text = "PASS: STEP 29.0 REAL STS2 COMPATIBILITY TARGET AUDIT — 4/4. Preserve the report; no game bytes were changed or executed.";
            _statusLabel.TextColor = UIColor.Label;
        }
        catch (OperationCanceledException)
        {
            _realStS2CompatibilityTargetAuditResultLabel.Text = "REAL STS2 COMPATIBILITY TARGET AUDIT: CANCELLED";
            _realStS2CompatibilityTargetAuditResultLabel.TextColor = UIColor.SecondaryLabel;
            _realStS2CompatibilityTargetAuditDetailLabel.Text = FormatRealStS2CompatibilityTargetAuditDetail(
                "Step 29 was cancelled. The subsystem is read-only and intentionally creates no transformed game image.");
            _statusLabel.Text = "STEP 29 CANCELLED — later audit gates are unproven.";
            _statusLabel.TextColor = UIColor.SecondaryLabel;
        }
        catch (Exception ex)
        {
            _realStS2CompatibilityTargetAuditResultLabel.Text = "REAL STS2 COMPATIBILITY TARGET AUDIT: EXCEPTION";
            _realStS2CompatibilityTargetAuditResultLabel.TextColor = UIColor.SystemRed;
            _realStS2CompatibilityTargetAuditDetailLabel.Text = FormatRealStS2CompatibilityTargetAuditDetail($"Unhandled Step 29 exception: {ex.GetType().Name}: {ex.Message}");
            _statusLabel.Text = "STEP 29 FAIL — stop at the current gate and preserve the Files report.";
            _statusLabel.TextColor = UIColor.SystemRed;
        }
        finally
        {
            await WriteDeviceTestReportFromLabelsAsync(
                "Step29-RealStS2CompatibilityTargetAudit.txt",
                "StS2 Launcher — Step 29 Real StS2 Compatibility Target Audit",
                _realStS2CompatibilityTargetAuditResultLabel,
                _realStS2CompatibilityTargetAuditDetailLabel,
                CancellationToken.None);
            EndSteamOperation();
        }
    }

    private bool RecordRealStS2CompatibilityTargetAuditGate(RealStS2CompatibilityTargetAuditGateResult result)
    {
        _realStS2CompatibilityTargetAuditGates.Record(result.Gate, result.Passed, result.Detail);
        if (_realStS2CompatibilityTargetAuditResultLabel is not null)
        {
            _realStS2CompatibilityTargetAuditResultLabel.Text = _realStS2CompatibilityTargetAuditGates.Snapshot().Summary;
            _realStS2CompatibilityTargetAuditResultLabel.TextColor = result.Passed ? UIColor.Label : UIColor.SystemRed;
        }
        if (_realStS2CompatibilityTargetAuditDetailLabel is not null)
            _realStS2CompatibilityTargetAuditDetailLabel.Text = FormatRealStS2CompatibilityTargetAuditDetail(result.Detail);
        if (!result.Passed && _statusLabel is not null)
        {
            var letter = (char)('A' + (int)result.Gate - 1);
            _statusLabel.Text = $"STEP 29 FAIL at Gate {letter} ({result.Gate}). Stop here; later real-StS2 target-audit gates were not run.";
            _statusLabel.TextColor = UIColor.SystemRed;
        }
        return result.Passed;
    }

    private string FormatRealStS2CompatibilityTargetAuditDetail(string tail)
    {
        var lines = new List<string>();
        foreach (var gate in _realStS2CompatibilityTargetAuditGates.Results)
        {
            var letter = (char)('A' + (int)gate.Gate - 1);
            lines.Add($"Gate {letter} — {gate.Gate}: {(gate.Passed ? "PASS" : "FAIL")}");
            lines.Add(gate.Detail);
            lines.Add(string.Empty);
        }

        lines.Add("Step 28 physical baseline: CLOSED POSITIVE — 0.0.111 passed A–E 5/5; transformed execution proved 1000 / 1041 / 1041 and post-execution OfflineReady 428/428.");
        lines.Add("Step 29 scope: exact receipt-backed macOS arm64 sts2.dll metadata/IL audit and deterministic candidate selection only.");
        lines.Add("Forbidden in Step 29.0: Cecil writes, transformed game images, CLR load/invocation of sts2, Harmony/MonoMod runtime patching, Godot/game startup, native game loading, trusted-install mutation, arbitrary Cecil resolver fallback.");
        lines.Add("Candidate-selection result is evidence, not authorization: the next candidate must inspect and predeclare one exact semantic transformation before writing any real-game copy.");
        lines.Add(tail);
        return string.Join("\n", lines);
    }
}
