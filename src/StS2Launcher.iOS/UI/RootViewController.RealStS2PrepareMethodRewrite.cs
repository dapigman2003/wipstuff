using StS2Launcher.Core;
using UIKit;

namespace StS2Launcher.iOS;

public sealed partial class RootViewController
{
    private readonly RealStS2PrepareMethodRewriteGateSequence _realStS2PrepareMethodRewriteGates = new();
    private UILabel? _realStS2PrepareMethodRewriteResultLabel;
    private UILabel? _realStS2PrepareMethodRewriteDetailLabel;
    private UIButton? _realStS2PrepareMethodRewriteButton;

    private void AddRealStS2PrepareMethodRewriteControls(UIStackView content)
    {
        content.AddArrangedSubview(Separator());
        content.AddArrangedSubview(Label(
            "Step 32.0 — First Real StS2 PrepareMethod Rewrite (ordered gates A–D)",
            UIFont.BoldSystemFontOfSize(25),
            UIColor.Label));

        _realStS2PrepareMethodRewriteButton = SystemButton(
            "Run Step 32 A–D — Clone Exact sts2.dll → Rewrite 10 PrepareMethod Calls → Reopen/Verify → Re-Prove Isolation",
            17);
        _realStS2PrepareMethodRewriteButton.TouchUpInside += async (_, _) => await RunRealStS2PrepareMethodRewriteAsync();
        content.AddArrangedSubview(_realStS2PrepareMethodRewriteButton);

        _realStS2PrepareMethodRewriteResultLabel = Label(
            "REAL STS2 PREPAREMETHOD REWRITE: NOT RUN",
            UIFont.BoldSystemFontOfSize(21),
            UIColor.Label);
        content.AddArrangedSubview(_realStS2PrepareMethodRewriteResultLabel);

        _realStS2PrepareMethodRewriteDetailLabel = Label(
            "Physical 0.0.114 closed Step 31 at 4/4 and explicitly retained OneTimeInitialization::PrewarmJit() as eligible for a separately predeclared rewrite design. Step 32 is the first real-game semantic transformation, but still performs no real-StS2 CLR admission. Gate A re-proves OfflineReady, binds the exact source/token/body/10-site evidence, and creates a launcher-private source clone. Gate B changes only the ten RuntimeHelpers.PrepareMethod calls: six one-argument calls become one Pop and four two-argument calls become two Pops, preserving the preceding reflection/method-handle discovery while consuming exactly the original void-call stack arguments. Gate C reopens source and transformed images and verifies the exact planned semantic fingerprint with zero remaining PrepareMethod references. Gate D re-hashes all images and re-proves OfflineReady/isolation. The trusted Step-12 install is never written.",
            UIFont.SystemFontOfSize(15),
            UIColor.SecondaryLabel);
        content.AddArrangedSubview(_realStS2PrepareMethodRewriteDetailLabel);
    }

    private async Task RunRealStS2PrepareMethodRewriteAsync()
    {
        if (_realStS2PrepareMethodRewriteResultLabel is null ||
            _realStS2PrepareMethodRewriteDetailLabel is null ||
            _realStS2PrepareMethodRewriteButton is null ||
            _statusLabel is null)
            return;

        if (!CurrentReleasePresentation.BundleIdentityMatchesExpected)
        {
            _realStS2PrepareMethodRewriteResultLabel.Text = "REAL STS2 PREPAREMETHOD REWRITE: RELEASE IDENTITY FAIL";
            _realStS2PrepareMethodRewriteResultLabel.TextColor = UIColor.SystemRed;
            _realStS2PrepareMethodRewriteDetailLabel.Text =
                $"Expected {CurrentReleasePresentation.ExpectedDisplayVersion} ({CurrentReleasePresentation.ExpectedBuildVersion}), observed {CurrentReleasePresentation.DisplayVersion} ({CurrentReleasePresentation.DisplayBuild}). Refusing Step 32 so transformed-game evidence cannot be attributed to the wrong source candidate.";
            _statusLabel.Text = "STEP 32 REFUSED — built bundle identity does not match the source-pinned candidate.";
            _statusLabel.TextColor = UIColor.SystemRed;
            return;
        }

        if (_godotProcessRequiresRestart)
        {
            _statusLabel.Text = "Step 15 Godot process-global state is active. Force-quit/relaunch before Step 32 so real-StS2 source admission starts from a fresh CLR/game state.";
            _statusLabel.TextColor = UIColor.SystemOrange;
            return;
        }

        BeginSteamOperation(allowCancel: true);
        _realStS2PrepareMethodRewriteGates.Reset();
        _realStS2PrepareMethodRewrite.Reset();
        _realStS2PrepareMethodRewriteResultLabel.Text = "REAL STS2 PREPAREMETHOD REWRITE: GATE A RUNNING…";
        _realStS2PrepareMethodRewriteResultLabel.TextColor = UIColor.Label;
        _statusLabel.Text = "STEP 32 GATE A — verify exact physical source and create launcher-private sts2.dll clone.";
        _statusLabel.TextColor = UIColor.Label;

        try
        {
            var token = _operationCts?.Token ?? CancellationToken.None;
            var progress = new Progress<RealStS2PrepareMethodRewriteProgress>(value =>
            {
                var count = value.TotalItems > 0 ? $" ({value.ProcessedItems:N0}/{value.TotalItems:N0})" : string.Empty;
                _realStS2PrepareMethodRewriteDetailLabel.Text = FormatRealStS2PrepareMethodRewriteDetail(
                    $"Gate {(char)('A' + (int)value.Gate - 1)} progress{count}: {value.Detail}" +
                    (string.IsNullOrWhiteSpace(value.CurrentPath) ? string.Empty : $"\nCurrent: {value.CurrentPath}"));
            });

            var gateA = await _realStS2PrepareMethodRewrite.RunSourceAdmissionAndPrivateCloneAsync(progress, token);
            if (!RecordRealStS2PrepareMethodRewriteGate(gateA)) return;

            _realStS2PrepareMethodRewriteResultLabel.Text = "REAL STS2 PREPAREMETHOD REWRITE: GATE B RUNNING…";
            _statusLabel.Text = "STEP 32 GATE B — deterministic stack-neutral rewrite on private sts2.dll only.";
            var gateB = await Task.Run(() => _realStS2PrepareMethodRewrite.RunDeterministicStackNeutralRewrite(), token);
            if (!RecordRealStS2PrepareMethodRewriteGate(gateB)) return;

            _realStS2PrepareMethodRewriteResultLabel.Text = "REAL STS2 PREPAREMETHOD REWRITE: GATE C RUNNING…";
            _statusLabel.Text = "STEP 32 GATE C — reopen source/transformed images and verify exact planned semantic diff before CLR admission.";
            var gateC = await Task.Run(() => _realStS2PrepareMethodRewrite.RunTransformedImageVerification(), token);
            if (!RecordRealStS2PrepareMethodRewriteGate(gateC)) return;

            _realStS2PrepareMethodRewriteResultLabel.Text = "REAL STS2 PREPAREMETHOD REWRITE: GATE D RUNNING…";
            _statusLabel.Text = "STEP 32 GATE D — source/transformed hashes + OfflineReady + no-CLR-load final isolation audit.";
            var gateD = await _realStS2PrepareMethodRewrite.RunFinalIsolationAuditAsync(progress, token);
            if (!RecordRealStS2PrepareMethodRewriteGate(gateD)) return;

            var snapshot = _realStS2PrepareMethodRewriteGates.Snapshot();
            _realStS2PrepareMethodRewriteResultLabel.Text = snapshot.Summary;
            _realStS2PrepareMethodRewriteResultLabel.TextColor = UIColor.Label;
            _realStS2PrepareMethodRewriteDetailLabel.Text = FormatRealStS2PrepareMethodRewriteDetail(
                "All four Step 32.0 gates passed. Preserve this report. A pass proves the first narrowly audited real-StS2 semantic rewrite can be materialized and verified on a launcher-private image without mutating or CLR-loading the trusted game source; transformed-game CLR admission/execution remains a separate next boundary.");
            _statusLabel.Text = "PASS: STEP 32.0 FIRST REAL STS2 PREPAREMETHOD REWRITE — 4/4. Private transformed image verified; no real game image was CLR-loaded.";
            _statusLabel.TextColor = UIColor.Label;
        }
        catch (OperationCanceledException)
        {
            _realStS2PrepareMethodRewriteResultLabel.Text = "REAL STS2 PREPAREMETHOD REWRITE: CANCELLED";
            _realStS2PrepareMethodRewriteResultLabel.TextColor = UIColor.SecondaryLabel;
            _realStS2PrepareMethodRewriteDetailLabel.Text = FormatRealStS2PrepareMethodRewriteDetail(
                "Step 32 was cancelled. Treat later gates as unproven; the trusted Step-12 install remains immutable.");
            _statusLabel.Text = "STEP 32 CANCELLED — later real-rewrite gates are unproven.";
            _statusLabel.TextColor = UIColor.SecondaryLabel;
        }
        catch (Exception ex)
        {
            _realStS2PrepareMethodRewriteResultLabel.Text = "REAL STS2 PREPAREMETHOD REWRITE: EXCEPTION";
            _realStS2PrepareMethodRewriteResultLabel.TextColor = UIColor.SystemRed;
            _realStS2PrepareMethodRewriteDetailLabel.Text = FormatRealStS2PrepareMethodRewriteDetail($"Unhandled Step 32 exception: {ex.GetType().Name}: {ex.Message}");
            _statusLabel.Text = "STEP 32 FAIL — stop at the current gate and preserve the Files report.";
            _statusLabel.TextColor = UIColor.SystemRed;
        }
        finally
        {
            await WriteDeviceTestReportFromLabelsAsync(
                "Step32-RealStS2PrepareMethodRewrite.txt",
                "StS2 Launcher — Step 32 Real StS2 PrepareMethod Rewrite",
                _realStS2PrepareMethodRewriteResultLabel,
                _realStS2PrepareMethodRewriteDetailLabel,
                CancellationToken.None);
            EndSteamOperation();
        }
    }

    private bool RecordRealStS2PrepareMethodRewriteGate(RealStS2PrepareMethodRewriteGateResult result)
    {
        _realStS2PrepareMethodRewriteGates.Record(result.Gate, result.Passed, result.Detail);
        if (_realStS2PrepareMethodRewriteResultLabel is not null)
        {
            _realStS2PrepareMethodRewriteResultLabel.Text = _realStS2PrepareMethodRewriteGates.Snapshot().Summary;
            _realStS2PrepareMethodRewriteResultLabel.TextColor = result.Passed ? UIColor.Label : UIColor.SystemRed;
        }
        if (_realStS2PrepareMethodRewriteDetailLabel is not null)
            _realStS2PrepareMethodRewriteDetailLabel.Text = FormatRealStS2PrepareMethodRewriteDetail(result.Detail);
        if (!result.Passed && _statusLabel is not null)
        {
            var letter = (char)('A' + (int)result.Gate - 1);
            _statusLabel.Text = $"STEP 32 FAIL at Gate {letter} ({result.Gate}). Stop here; later real-rewrite gates were not run.";
            _statusLabel.TextColor = UIColor.SystemRed;
        }
        return result.Passed;
    }

    private string FormatRealStS2PrepareMethodRewriteDetail(string tail)
    {
        var lines = new List<string>();
        foreach (var gate in _realStS2PrepareMethodRewriteGates.Results)
        {
            var letter = (char)('A' + (int)gate.Gate - 1);
            lines.Add($"Gate {letter} — {gate.Gate}: {(gate.Passed ? "PASS" : "FAIL")}");
            lines.Add(gate.Detail);
            lines.Add(string.Empty);
        }
        lines.Add("Step 28 physical baseline: CLOSED POSITIVE — 0.0.111 passed 5/5 with 1000 / 1041 / 1041 and OfflineReady 428/428.");
        lines.Add("Step 29 physical baseline: CLOSED POSITIVE — 0.0.112 passed 4/4 and produced the exact receipt-backed compatibility-site inventory.");
        lines.Add("Step 30 physical baseline: CLOSED POSITIVE — 0.0.113 passed 4/4 and deferred ModManager.TryLoadMod(Mod) -> Harmony.PatchAll from the base-game frontier.");
        lines.Add("Step 31 physical baseline: CLOSED POSITIVE — 0.0.114 passed 4/4 and confirmed OneTimeInitialization::PrewarmJit() token 0x06007D05 / ten PrepareMethod sites as eligible for explicit rewrite design, with no write authorized in that build.");
        lines.Add("Step 32 predeclared semantic change: only the ten exact RuntimeHelpers.PrepareMethod calls are suppressed; one-argument calls become Pop, two-argument calls become Pop + Pop, preserving preceding reflection/method-handle discovery and the surrounding method control-flow/exception structure.");
        lines.Add("Forbidden in Step 32.0: mutation of the receipt-backed Step-12 install, any other real-StS2 semantic change, real-StS2 CLR admission/invocation, Harmony/MonoMod runtime patching, Godot/game startup, native game loading, arbitrary Cecil resolver fallback.");
        lines.Add("A Step-32 PASS authorizes only a later separately gated transformed-real-StS2 admission/execution experiment; this build itself never CLR-loads the transformed game image.");
        lines.Add(tail);
        return string.Join("\n", lines);
    }
}
