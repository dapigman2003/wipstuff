using StS2Launcher.Core;
using UIKit;

namespace StS2Launcher.iOS;

public sealed partial class RootViewController
{
    private readonly RealStS2SelectedTargetSemanticAuditGateSequence _realStS2SelectedTargetSemanticAuditGates = new();
    private UILabel? _realStS2SelectedTargetSemanticAuditResultLabel;
    private UILabel? _realStS2SelectedTargetSemanticAuditDetailLabel;
    private UIButton? _realStS2SelectedTargetSemanticAuditButton;

    private void AddRealStS2SelectedTargetSemanticAuditControls(UIStackView content)
    {
        content.AddArrangedSubview(Separator());
        content.AddArrangedSubview(Label(
            "Step 30.0 — Selected Harmony Target Semantic Context Audit (ordered gates A–D)",
            UIFont.BoldSystemFontOfSize(25),
            UIColor.Label));

        _realStS2SelectedTargetSemanticAuditButton = SystemButton(
            "Run Step 30 A–D — Bind Step-29 Evidence → Inspect Exact Context → Disposition → Re-Prove Isolation",
            17);
        _realStS2SelectedTargetSemanticAuditButton.TouchUpInside += async (_, _) => await RunRealStS2SelectedTargetSemanticAuditAsync();
        content.AddArrangedSubview(_realStS2SelectedTargetSemanticAuditButton);

        _realStS2SelectedTargetSemanticAuditResultLabel = Label(
            "SELECTED TARGET SEMANTIC CONTEXT AUDIT: NOT RUN",
            UIFont.BoldSystemFontOfSize(21),
            UIColor.Label);
        content.AddArrangedSubview(_realStS2SelectedTargetSemanticAuditResultLabel);

        _realStS2SelectedTargetSemanticAuditDetailLabel = Label(
            "Physical 0.0.112 closed Step 29 at 4/4 and selected exactly MegaCrit.Sts2.Core.Modding.ModManager::TryLoadMod(Mod) token 0x06007927, IL_0D9D Callvirt -> Harmony.PatchAll(Assembly), body SHA-256 50c8c4394082f3c73df414fad8675540cfc00a99ccc4f350b616cec574cdbcbd. Step 30 is still read-only. Gate A re-proves OfflineReady and binds that exact physical evidence to the same receipt-backed ARM64 sts2.dll. Gate B records the selected method's exact bounded IL/control-flow/exception context. Gate C applies the predeclared product boundary: a site structurally confined to ModManager.TryLoadMod is deferred because Harmony/mod compatibility must not block base-game startup. Gate D re-hashes the source and re-proves isolation. No Cecil write or real-StS2 CLR execution occurs.",
            UIFont.SystemFontOfSize(15),
            UIColor.SecondaryLabel);
        content.AddArrangedSubview(_realStS2SelectedTargetSemanticAuditDetailLabel);
    }

    private async Task RunRealStS2SelectedTargetSemanticAuditAsync()
    {
        if (_realStS2SelectedTargetSemanticAuditResultLabel is null ||
            _realStS2SelectedTargetSemanticAuditDetailLabel is null ||
            _realStS2SelectedTargetSemanticAuditButton is null ||
            _statusLabel is null)
            return;

        if (!CurrentReleasePresentation.BundleIdentityMatchesExpected)
        {
            _realStS2SelectedTargetSemanticAuditResultLabel.Text = "SELECTED TARGET SEMANTIC CONTEXT AUDIT: RELEASE IDENTITY FAIL";
            _realStS2SelectedTargetSemanticAuditResultLabel.TextColor = UIColor.SystemRed;
            _realStS2SelectedTargetSemanticAuditDetailLabel.Text =
                $"Expected {CurrentReleasePresentation.ExpectedDisplayVersion} ({CurrentReleasePresentation.ExpectedBuildVersion}), observed {CurrentReleasePresentation.DisplayVersion} ({CurrentReleasePresentation.DisplayBuild}). Refusing Step 30 so semantic-context evidence cannot be attributed to the wrong source candidate.";
            _statusLabel.Text = "STEP 30 REFUSED — built bundle identity does not match the source-pinned candidate.";
            _statusLabel.TextColor = UIColor.SystemRed;
            return;
        }

        if (_godotProcessRequiresRestart)
        {
            _statusLabel.Text = "Step 15 Godot process-global state is active. Force-quit/relaunch before Step 30 so the real-StS2 semantic audit starts from a fresh CLR/game state.";
            _statusLabel.TextColor = UIColor.SystemOrange;
            return;
        }

        BeginSteamOperation(allowCancel: true);
        _realStS2SelectedTargetSemanticAuditGates.Reset();
        _realStS2SelectedTargetSemanticAudit.Reset();
        _realStS2SelectedTargetSemanticAuditResultLabel.Text = "SELECTED TARGET SEMANTIC CONTEXT AUDIT: GATE A RUNNING…";
        _realStS2SelectedTargetSemanticAuditResultLabel.TextColor = UIColor.Label;
        _realStS2SelectedTargetSemanticAuditDetailLabel.Text = "Gate A: re-proving OfflineReady and binding the exact physical Step-29 source hash/MVID/method token/IL offset/target/body fingerprint without CLR-loading sts2.dll.";
        _statusLabel.Text = "STEP 30 GATE A — bind physical Step-29 evidence to exact receipt-backed sts2.dll.";
        _statusLabel.TextColor = UIColor.Label;

        try
        {
            var token = _operationCts?.Token ?? CancellationToken.None;
            var progress = new Progress<RealStS2SelectedTargetSemanticAuditProgress>(value =>
            {
                var count = value.TotalItems > 0 ? $" ({value.ProcessedItems:N0}/{value.TotalItems:N0})" : string.Empty;
                _realStS2SelectedTargetSemanticAuditDetailLabel.Text = FormatRealStS2SelectedTargetSemanticAuditDetail(
                    $"Gate {(char)('A' + (int)value.Gate - 1)} progress{count}: {value.Detail}" +
                    (string.IsNullOrWhiteSpace(value.CurrentPath) ? string.Empty : $"\nCurrent: {value.CurrentPath}"));
            });

            var gateA = await _realStS2SelectedTargetSemanticAudit.RunSelectedEvidenceBindingAndOfflineReadyAsync(progress, token);
            if (!RecordRealStS2SelectedTargetSemanticAuditGate(gateA)) return;

            _realStS2SelectedTargetSemanticAuditResultLabel.Text = "SELECTED TARGET SEMANTIC CONTEXT AUDIT: GATE B RUNNING…";
            _statusLabel.Text = "STEP 30 GATE B — exact selected-method IL/control-flow/exception context audit; no rewrite.";
            var gateB = await Task.Run(() => _realStS2SelectedTargetSemanticAudit.RunExactSemanticContextAudit(), token);
            if (!RecordRealStS2SelectedTargetSemanticAuditGate(gateB)) return;

            _realStS2SelectedTargetSemanticAuditResultLabel.Text = "SELECTED TARGET SEMANTIC CONTEXT AUDIT: GATE C RUNNING…";
            _statusLabel.Text = "STEP 30 GATE C — deterministic product-scope disposition; no real-game write authorization.";
            var gateC = await Task.Run(() => _realStS2SelectedTargetSemanticAudit.RunDeterministicDisposition(), token);
            if (!RecordRealStS2SelectedTargetSemanticAuditGate(gateC)) return;

            _realStS2SelectedTargetSemanticAuditResultLabel.Text = "SELECTED TARGET SEMANTIC CONTEXT AUDIT: GATE D RUNNING…";
            _statusLabel.Text = "STEP 30 GATE D — source hashes + OfflineReady + no-CLR-load isolation audit.";
            var gateD = await _realStS2SelectedTargetSemanticAudit.RunFinalIsolationAuditAsync(progress, token);
            if (!RecordRealStS2SelectedTargetSemanticAuditGate(gateD)) return;

            var snapshot = _realStS2SelectedTargetSemanticAuditGates.Snapshot();
            _realStS2SelectedTargetSemanticAuditResultLabel.Text = snapshot.Summary;
            _realStS2SelectedTargetSemanticAuditResultLabel.TextColor = UIColor.Label;
            _realStS2SelectedTargetSemanticAuditDetailLabel.Text = FormatRealStS2SelectedTargetSemanticAuditDetail(
                "All four Step 30.0 gates passed. Preserve this report. A pass does not authorize a real-game rewrite; it formally defers the selected Harmony/mod-loading site from the base-game frontier and points the next evidence iteration at the highest-priority non-mod Step-29 family.");
            _statusLabel.Text = "PASS: STEP 30.0 SELECTED TARGET SEMANTIC CONTEXT AUDIT — 4/4. Selected Harmony mod path deferred; no game bytes changed or executed.";
            _statusLabel.TextColor = UIColor.Label;
        }
        catch (OperationCanceledException)
        {
            _realStS2SelectedTargetSemanticAuditResultLabel.Text = "SELECTED TARGET SEMANTIC CONTEXT AUDIT: CANCELLED";
            _realStS2SelectedTargetSemanticAuditResultLabel.TextColor = UIColor.SecondaryLabel;
            _realStS2SelectedTargetSemanticAuditDetailLabel.Text = FormatRealStS2SelectedTargetSemanticAuditDetail(
                "Step 30 was cancelled. This subsystem is read-only and intentionally creates no transformed game image.");
            _statusLabel.Text = "STEP 30 CANCELLED — later semantic-audit gates are unproven.";
            _statusLabel.TextColor = UIColor.SecondaryLabel;
        }
        catch (Exception ex)
        {
            _realStS2SelectedTargetSemanticAuditResultLabel.Text = "SELECTED TARGET SEMANTIC CONTEXT AUDIT: EXCEPTION";
            _realStS2SelectedTargetSemanticAuditResultLabel.TextColor = UIColor.SystemRed;
            _realStS2SelectedTargetSemanticAuditDetailLabel.Text = FormatRealStS2SelectedTargetSemanticAuditDetail($"Unhandled Step 30 exception: {ex.GetType().Name}: {ex.Message}");
            _statusLabel.Text = "STEP 30 FAIL — stop at the current gate and preserve the Files report.";
            _statusLabel.TextColor = UIColor.SystemRed;
        }
        finally
        {
            await WriteDeviceTestReportFromLabelsAsync(
                "Step30-SelectedTargetSemanticContextAudit.txt",
                "StS2 Launcher — Step 30 Selected Target Semantic Context Audit",
                _realStS2SelectedTargetSemanticAuditResultLabel,
                _realStS2SelectedTargetSemanticAuditDetailLabel,
                CancellationToken.None);
            EndSteamOperation();
        }
    }

    private bool RecordRealStS2SelectedTargetSemanticAuditGate(RealStS2SelectedTargetSemanticAuditGateResult result)
    {
        _realStS2SelectedTargetSemanticAuditGates.Record(result.Gate, result.Passed, result.Detail);
        if (_realStS2SelectedTargetSemanticAuditResultLabel is not null)
        {
            _realStS2SelectedTargetSemanticAuditResultLabel.Text = _realStS2SelectedTargetSemanticAuditGates.Snapshot().Summary;
            _realStS2SelectedTargetSemanticAuditResultLabel.TextColor = result.Passed ? UIColor.Label : UIColor.SystemRed;
        }
        if (_realStS2SelectedTargetSemanticAuditDetailLabel is not null)
            _realStS2SelectedTargetSemanticAuditDetailLabel.Text = FormatRealStS2SelectedTargetSemanticAuditDetail(result.Detail);
        if (!result.Passed && _statusLabel is not null)
        {
            var letter = (char)('A' + (int)result.Gate - 1);
            _statusLabel.Text = $"STEP 30 FAIL at Gate {letter} ({result.Gate}). Stop here; later selected-target semantic-audit gates were not run.";
            _statusLabel.TextColor = UIColor.SystemRed;
        }
        return result.Passed;
    }

    private string FormatRealStS2SelectedTargetSemanticAuditDetail(string tail)
    {
        var lines = new List<string>();
        foreach (var gate in _realStS2SelectedTargetSemanticAuditGates.Results)
        {
            var letter = (char)('A' + (int)gate.Gate - 1);
            lines.Add($"Gate {letter} — {gate.Gate}: {(gate.Passed ? "PASS" : "FAIL")}");
            lines.Add(gate.Detail);
            lines.Add(string.Empty);
        }
        lines.Add("Step 28 physical baseline: CLOSED POSITIVE — 0.0.111 passed 5/5 with 1000 / 1041 / 1041 and OfflineReady 428/428.");
        lines.Add("Step 29 physical baseline: CLOSED POSITIVE — 0.0.112 passed 4/4 and selected ModManager.TryLoadMod(Mod) token 0x06007927 @ IL_0D9D -> Harmony.PatchAll(Assembly), body SHA-256 50c8c4394082f3c73df414fad8675540cfc00a99ccc4f350b616cec574cdbcbd.");
        lines.Add("Step 30 scope: bind that exact selection, inspect its surrounding semantics, and decide product-scope disposition only.");
        lines.Add("Forbidden in Step 30.0: Cecil writes, transformed game images, CLR load/invocation of sts2, Harmony/MonoMod runtime patching, Godot/game startup, native game loading, trusted-install mutation, arbitrary Cecil resolver fallback.");
        lines.Add("A Step-30 PASS explicitly authorizes no semantic rewrite of the selected PatchAll site; it records whether the site should be deferred from the base-game frontier.");
        lines.Add(tail);
        return string.Join("\n", lines);
    }
}
