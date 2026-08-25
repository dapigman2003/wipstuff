using StS2Launcher.Core;
using UIKit;

namespace StS2Launcher.iOS;

public sealed partial class RootViewController
{
    private readonly RealStS2PrepareMethodSemanticAuditGateSequence _realStS2PrepareMethodSemanticAuditGates = new();
    private UILabel? _realStS2PrepareMethodSemanticAuditResultLabel;
    private UILabel? _realStS2PrepareMethodSemanticAuditDetailLabel;
    private UIButton? _realStS2PrepareMethodSemanticAuditButton;

    private void AddRealStS2PrepareMethodSemanticAuditControls(UIStackView content)
    {
        content.AddArrangedSubview(Separator());
        content.AddArrangedSubview(Label(
            "Step 31.0 — PrepareMethod Semantic Context Audit (ordered gates A–D)",
            UIFont.BoldSystemFontOfSize(25),
            UIColor.Label));

        _realStS2PrepareMethodSemanticAuditButton = SystemButton(
            "Run Step 31 A–D — Bind PrewarmJit Evidence → Inspect 10 PrepareMethod Sites → Disposition → Re-Prove Isolation",
            17);
        _realStS2PrepareMethodSemanticAuditButton.TouchUpInside += async (_, _) => await RunRealStS2PrepareMethodSemanticAuditAsync();
        content.AddArrangedSubview(_realStS2PrepareMethodSemanticAuditButton);

        _realStS2PrepareMethodSemanticAuditResultLabel = Label(
            "PREPAREMETHOD SEMANTIC CONTEXT AUDIT: NOT RUN",
            UIFont.BoldSystemFontOfSize(21),
            UIColor.Label);
        content.AddArrangedSubview(_realStS2PrepareMethodSemanticAuditResultLabel);

        _realStS2PrepareMethodSemanticAuditDetailLabel = Label(
            "Physical 0.0.113 closed Step 30 at 4/4 and formally deferred the Harmony/ModManager site from the base-game frontier. Step 31 remains read-only. Gate A re-proves OfflineReady and binds the exact Step-29 OneTimeInitialization::PrewarmJit() token/body fingerprint plus all ten RuntimeHelpers.PrepareMethod offsets to the same receipt-backed ARM64 sts2.dll. Gate B records per-site bounded IL/control-flow/exception context. Gate C may retain this non-mod family as eligible for an explicitly predeclared rewrite design, but authorizes no rewrite in this build. Gate D re-hashes the source and re-proves isolation. No Cecil write or real-StS2 CLR execution occurs.",
            UIFont.SystemFontOfSize(15),
            UIColor.SecondaryLabel);
        content.AddArrangedSubview(_realStS2PrepareMethodSemanticAuditDetailLabel);
    }

    private async Task RunRealStS2PrepareMethodSemanticAuditAsync()
    {
        if (_realStS2PrepareMethodSemanticAuditResultLabel is null ||
            _realStS2PrepareMethodSemanticAuditDetailLabel is null ||
            _realStS2PrepareMethodSemanticAuditButton is null ||
            _statusLabel is null)
            return;

        if (!CurrentReleasePresentation.BundleIdentityMatchesExpected)
        {
            _realStS2PrepareMethodSemanticAuditResultLabel.Text = "PREPAREMETHOD SEMANTIC CONTEXT AUDIT: RELEASE IDENTITY FAIL";
            _realStS2PrepareMethodSemanticAuditResultLabel.TextColor = UIColor.SystemRed;
            _realStS2PrepareMethodSemanticAuditDetailLabel.Text =
                $"Expected {CurrentReleasePresentation.ExpectedDisplayVersion} ({CurrentReleasePresentation.ExpectedBuildVersion}), observed {CurrentReleasePresentation.DisplayVersion} ({CurrentReleasePresentation.DisplayBuild}). Refusing Step 31 so semantic-context evidence cannot be attributed to the wrong source candidate.";
            _statusLabel.Text = "STEP 31 REFUSED — built bundle identity does not match the source-pinned candidate.";
            _statusLabel.TextColor = UIColor.SystemRed;
            return;
        }

        if (_godotProcessRequiresRestart)
        {
            _statusLabel.Text = "Step 15 Godot process-global state is active. Force-quit/relaunch before Step 31 so the real-StS2 semantic audit starts from a fresh CLR/game state.";
            _statusLabel.TextColor = UIColor.SystemOrange;
            return;
        }

        BeginSteamOperation(allowCancel: true);
        _realStS2PrepareMethodSemanticAuditGates.Reset();
        _realStS2PrepareMethodSemanticAudit.Reset();
        _realStS2PrepareMethodSemanticAuditResultLabel.Text = "PREPAREMETHOD SEMANTIC CONTEXT AUDIT: GATE A RUNNING…";
        _realStS2PrepareMethodSemanticAuditResultLabel.TextColor = UIColor.Label;
        _realStS2PrepareMethodSemanticAuditDetailLabel.Text = "Gate A: re-proving OfflineReady and binding the exact PrewarmJit token/body fingerprint plus all ten physical Step-29 PrepareMethod offsets without CLR-loading sts2.dll.";
        _statusLabel.Text = "STEP 31 GATE A — bind exact PrewarmJit/PrepareMethod physical evidence to receipt-backed sts2.dll.";
        _statusLabel.TextColor = UIColor.Label;

        try
        {
            var token = _operationCts?.Token ?? CancellationToken.None;
            var progress = new Progress<RealStS2PrepareMethodSemanticAuditProgress>(value =>
            {
                var count = value.TotalItems > 0 ? $" ({value.ProcessedItems:N0}/{value.TotalItems:N0})" : string.Empty;
                _realStS2PrepareMethodSemanticAuditDetailLabel.Text = FormatRealStS2PrepareMethodSemanticAuditDetail(
                    $"Gate {(char)('A' + (int)value.Gate - 1)} progress{count}: {value.Detail}" +
                    (string.IsNullOrWhiteSpace(value.CurrentPath) ? string.Empty : $"\nCurrent: {value.CurrentPath}"));
            });

            var gateA = await _realStS2PrepareMethodSemanticAudit.RunEvidenceBindingAndOfflineReadyAsync(progress, token);
            if (!RecordRealStS2PrepareMethodSemanticAuditGate(gateA)) return;

            _realStS2PrepareMethodSemanticAuditResultLabel.Text = "PREPAREMETHOD SEMANTIC CONTEXT AUDIT: GATE B RUNNING…";
            _statusLabel.Text = "STEP 31 GATE B — exact per-site PrepareMethod IL/control-flow/exception context audit; no rewrite.";
            var gateB = await Task.Run(() => _realStS2PrepareMethodSemanticAudit.RunExactPrepareMethodSemanticContextAudit(), token);
            if (!RecordRealStS2PrepareMethodSemanticAuditGate(gateB)) return;

            _realStS2PrepareMethodSemanticAuditResultLabel.Text = "PREPAREMETHOD SEMANTIC CONTEXT AUDIT: GATE C RUNNING…";
            _statusLabel.Text = "STEP 31 GATE C — classify rewrite-design eligibility; no real-game write authorization.";
            var gateC = await Task.Run(() => _realStS2PrepareMethodSemanticAudit.RunDeterministicDisposition(), token);
            if (!RecordRealStS2PrepareMethodSemanticAuditGate(gateC)) return;

            _realStS2PrepareMethodSemanticAuditResultLabel.Text = "PREPAREMETHOD SEMANTIC CONTEXT AUDIT: GATE D RUNNING…";
            _statusLabel.Text = "STEP 31 GATE D — source hashes + OfflineReady + no-CLR-load isolation audit.";
            var gateD = await _realStS2PrepareMethodSemanticAudit.RunFinalIsolationAuditAsync(progress, token);
            if (!RecordRealStS2PrepareMethodSemanticAuditGate(gateD)) return;

            var snapshot = _realStS2PrepareMethodSemanticAuditGates.Snapshot();
            _realStS2PrepareMethodSemanticAuditResultLabel.Text = snapshot.Summary;
            _realStS2PrepareMethodSemanticAuditResultLabel.TextColor = UIColor.Label;
            _realStS2PrepareMethodSemanticAuditDetailLabel.Text = FormatRealStS2PrepareMethodSemanticAuditDetail(
                "All four Step 31.0 gates passed. Preserve this report. A pass still authorizes no real-game rewrite; it establishes whether the exact PrewarmJit/PrepareMethod family is eligible for a separately predeclared transformation design.");
            _statusLabel.Text = "PASS: STEP 31.0 PREPAREMETHOD SEMANTIC CONTEXT AUDIT — 4/4. Rewrite-design disposition recorded; no game bytes changed or executed.";
            _statusLabel.TextColor = UIColor.Label;
        }
        catch (OperationCanceledException)
        {
            _realStS2PrepareMethodSemanticAuditResultLabel.Text = "PREPAREMETHOD SEMANTIC CONTEXT AUDIT: CANCELLED";
            _realStS2PrepareMethodSemanticAuditResultLabel.TextColor = UIColor.SecondaryLabel;
            _realStS2PrepareMethodSemanticAuditDetailLabel.Text = FormatRealStS2PrepareMethodSemanticAuditDetail(
                "Step 31 was cancelled. This subsystem is read-only and intentionally creates no transformed game image.");
            _statusLabel.Text = "STEP 31 CANCELLED — later semantic-audit gates are unproven.";
            _statusLabel.TextColor = UIColor.SecondaryLabel;
        }
        catch (Exception ex)
        {
            _realStS2PrepareMethodSemanticAuditResultLabel.Text = "PREPAREMETHOD SEMANTIC CONTEXT AUDIT: EXCEPTION";
            _realStS2PrepareMethodSemanticAuditResultLabel.TextColor = UIColor.SystemRed;
            _realStS2PrepareMethodSemanticAuditDetailLabel.Text = FormatRealStS2PrepareMethodSemanticAuditDetail($"Unhandled Step 31 exception: {ex.GetType().Name}: {ex.Message}");
            _statusLabel.Text = "STEP 31 FAIL — stop at the current gate and preserve the Files report.";
            _statusLabel.TextColor = UIColor.SystemRed;
        }
        finally
        {
            await WriteDeviceTestReportFromLabelsAsync(
                "Step31-PrepareMethodSemanticContextAudit.txt",
                "StS2 Launcher — Step 31 PrepareMethod Semantic Context Audit",
                _realStS2PrepareMethodSemanticAuditResultLabel,
                _realStS2PrepareMethodSemanticAuditDetailLabel,
                CancellationToken.None);
            EndSteamOperation();
        }
    }

    private bool RecordRealStS2PrepareMethodSemanticAuditGate(RealStS2PrepareMethodSemanticAuditGateResult result)
    {
        _realStS2PrepareMethodSemanticAuditGates.Record(result.Gate, result.Passed, result.Detail);
        if (_realStS2PrepareMethodSemanticAuditResultLabel is not null)
        {
            _realStS2PrepareMethodSemanticAuditResultLabel.Text = _realStS2PrepareMethodSemanticAuditGates.Snapshot().Summary;
            _realStS2PrepareMethodSemanticAuditResultLabel.TextColor = result.Passed ? UIColor.Label : UIColor.SystemRed;
        }
        if (_realStS2PrepareMethodSemanticAuditDetailLabel is not null)
            _realStS2PrepareMethodSemanticAuditDetailLabel.Text = FormatRealStS2PrepareMethodSemanticAuditDetail(result.Detail);
        if (!result.Passed && _statusLabel is not null)
        {
            var letter = (char)('A' + (int)result.Gate - 1);
            _statusLabel.Text = $"STEP 31 FAIL at Gate {letter} ({result.Gate}). Stop here; later PrepareMethod semantic-audit gates were not run.";
            _statusLabel.TextColor = UIColor.SystemRed;
        }
        return result.Passed;
    }

    private string FormatRealStS2PrepareMethodSemanticAuditDetail(string tail)
    {
        var lines = new List<string>();
        foreach (var gate in _realStS2PrepareMethodSemanticAuditGates.Results)
        {
            var letter = (char)('A' + (int)gate.Gate - 1);
            lines.Add($"Gate {letter} — {gate.Gate}: {(gate.Passed ? "PASS" : "FAIL")}");
            lines.Add(gate.Detail);
            lines.Add(string.Empty);
        }
        lines.Add("Step 28 physical baseline: CLOSED POSITIVE — 0.0.111 passed 5/5 with 1000 / 1041 / 1041 and OfflineReady 428/428.");
        lines.Add("Step 29 physical baseline: CLOSED POSITIVE — 0.0.112 passed 4/4; non-mod evidence includes OneTimeInitialization::PrewarmJit() token 0x06007D05 with ten PrepareMethod calls and body SHA-256 7f25b7bd955c407fc69306cf26af2162223353f5606560458066aed085e72ab9.");
        lines.Add("Step 30 physical baseline: CLOSED POSITIVE — 0.0.113 passed 4/4 and deferred ModManager.TryLoadMod(Mod) -> Harmony.PatchAll from the base-game frontier.");
        lines.Add("Step 31 scope: bind the exact PrewarmJit token/body fingerprint and ten PrepareMethod sites, inspect their surrounding semantics, and record rewrite-design eligibility only.");
        lines.Add("Forbidden in Step 31.0: Cecil writes, transformed game images, CLR load/invocation of sts2, Harmony/MonoMod runtime patching, Godot/game startup, native game loading, trusted-install mutation, arbitrary Cecil resolver fallback.");
        lines.Add("A Step-31 PASS explicitly authorizes no semantic rewrite; it may only retain the exact PrewarmJit/PrepareMethod family as eligible for a later predeclared rewrite design.");
        lines.Add(tail);
        return string.Join("\n", lines);
    }
}
