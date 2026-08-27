using Foundation;
using StS2Launcher.Core;
using UIKit;

namespace StS2Launcher.iOS;

public sealed partial class RootViewController
{
    private readonly AheadOfLoadManagedTransformation _aheadOfLoadManagedTransformation;
    private readonly AheadOfLoadManagedTransformationGateSequence _aheadOfLoadManagedTransformationGates = new();
    private UILabel? _aheadOfLoadManagedTransformationResultLabel;
    private UILabel? _aheadOfLoadManagedTransformationDetailLabel;
    private UIButton? _aheadOfLoadManagedTransformationButton;

    private void AddAheadOfLoadManagedTransformationControls(UIStackView content)
    {
        content.AddArrangedSubview(Separator());
        content.AddArrangedSubview(Label(
            "Step 28.0 — Ahead-of-Load Managed Transformation (ordered gates A–E)",
            UIFont.BoldSystemFontOfSize(25),
            UIColor.Label));

        _aheadOfLoadManagedTransformationButton = SystemButton(
            "Run Step 28 A–E — Admit Source → Rewrite Before Load → Verify → Execute Transformed IL → Audit",
            17);
        _aheadOfLoadManagedTransformationButton.TouchUpInside += async (_, _) => await RunAheadOfLoadManagedTransformationAsync();
        content.AddArrangedSubview(_aheadOfLoadManagedTransformationButton);

        _aheadOfLoadManagedTransformationResultLabel = Label(
            "AHEAD-OF-LOAD MANAGED TRANSFORMATION BOUNDARY: NOT RUN",
            UIFont.BoldSystemFontOfSize(21),
            UIColor.Label);
        content.AddArrangedSubview(_aheadOfLoadManagedTransformationResultLabel);

        _aheadOfLoadManagedTransformationDetailLabel = Label(
            "Step 27 is closed negative: physical 0.0.108 proved Harmony PatchProcessor.Patch() still throws NotImplementedException for a genuine post-publish interpreted target. Step 28 therefore uses no Harmony/MonoMod runtime detour. Gate A re-proves OfflineReady and admits a separately built post-publish fixture as Cecil metadata only. Gate B rewrites Adjustment() from 1 to 1000 into a new launcher-private image before CLR admission. Gate C reopens and verifies source/transformed IL and hashes. Gate D loads only the transformed bytes into a dedicated private AssemblyLoadContext and requires Target(41) and the in-fixture direct-call InvokeTarget(41) to both return 1041. Gate E re-hashes all images and re-proves OfflineReady. No real StS2 member is reflected or invoked in this first architecture-pivot candidate.",
            UIFont.SystemFontOfSize(15),
            UIColor.SecondaryLabel);
        content.AddArrangedSubview(_aheadOfLoadManagedTransformationDetailLabel);
    }

    private async Task RunAheadOfLoadManagedTransformationAsync()
    {
        if (_aheadOfLoadManagedTransformationResultLabel is null ||
            _aheadOfLoadManagedTransformationDetailLabel is null ||
            _aheadOfLoadManagedTransformationButton is null ||
            _statusLabel is null)
        {
            return;
        }

        if (!CurrentReleasePresentation.BundleIdentityMatchesExpected)
        {
            _aheadOfLoadManagedTransformationResultLabel.Text = "AHEAD-OF-LOAD MANAGED TRANSFORMATION BOUNDARY: RELEASE IDENTITY FAIL";
            _aheadOfLoadManagedTransformationResultLabel.TextColor = UIColor.SystemRed;
            _aheadOfLoadManagedTransformationDetailLabel.Text =
                $"Expected {CurrentReleasePresentation.ExpectedDisplayVersion} ({CurrentReleasePresentation.ExpectedBuildVersion}), observed {CurrentReleasePresentation.DisplayVersion} ({CurrentReleasePresentation.DisplayBuild}). Refusing Step 28 so physical evidence cannot be attributed to the wrong source candidate.";
            _statusLabel.Text = "STEP 28 REFUSED — built bundle identity does not match the source-pinned candidate.";
            _statusLabel.TextColor = UIColor.SystemRed;
            return;
        }

        if (_godotProcessRequiresRestart)
        {
            _statusLabel.Text = "Step 15 Godot process-global state is active. Force-quit/relaunch before Step 28 so ahead-of-load transformation evidence is isolated from the Godot session.";
            _statusLabel.TextColor = UIColor.SystemOrange;
            return;
        }

        BeginSteamOperation(allowCancel: true);
        _aheadOfLoadManagedTransformationGates.Reset();
        _aheadOfLoadManagedTransformation.Reset();
        _aheadOfLoadManagedTransformationResultLabel.Text = "AHEAD-OF-LOAD MANAGED TRANSFORMATION BOUNDARY: GATE A RUNNING…";
        _aheadOfLoadManagedTransformationResultLabel.TextColor = UIColor.Label;
        _aheadOfLoadManagedTransformationDetailLabel.Text = "Gate A: re-proving OfflineReady and validating/copying the post-publish fixture strictly as Cecil metadata; its assembly identity must not already be CLR-loaded.";
        _statusLabel.Text = "STEP 28 GATE A — fixture admission + OfflineReady; no CLR load.";
        _statusLabel.TextColor = UIColor.Label;

        try
        {
            var token = _operationCts?.Token ?? CancellationToken.None;
            var progress = new Progress<AheadOfLoadManagedTransformationProgress>(value =>
            {
                var count = value.TotalItems > 0 ? $" ({value.ProcessedItems:N0}/{value.TotalItems:N0})" : string.Empty;
                _aheadOfLoadManagedTransformationDetailLabel.Text = FormatAheadOfLoadManagedTransformationDetail(
                    $"Gate {(char)('A' + (int)value.Gate - 1)} progress{count}: {value.Detail}" +
                    (string.IsNullOrWhiteSpace(value.CurrentPath) ? string.Empty : $"\nCurrent: {value.CurrentPath}"));
            });

            var gateA = await _aheadOfLoadManagedTransformation.RunFixtureAdmissionAndOfflineReadyAsync(progress, token);
            if (!RecordAheadOfLoadManagedTransformationGate(gateA)) return;

            _aheadOfLoadManagedTransformationResultLabel.Text = "AHEAD-OF-LOAD MANAGED TRANSFORMATION BOUNDARY: GATE B RUNNING…";
            _statusLabel.Text = "STEP 28 GATE B — deterministic Cecil rewrite into a new private image before any CLR admission.";
            var gateB = await Task.Run(() => _aheadOfLoadManagedTransformation.RunDeterministicRewrite(), token);
            if (!RecordAheadOfLoadManagedTransformationGate(gateB)) return;

            _aheadOfLoadManagedTransformationResultLabel.Text = "AHEAD-OF-LOAD MANAGED TRANSFORMATION BOUNDARY: GATE C RUNNING…";
            _statusLabel.Text = "STEP 28 GATE C — reopen source/transformed images and prove exact IL/hash isolation before load.";
            var gateC = await Task.Run(() => _aheadOfLoadManagedTransformation.RunTransformedImageVerification(), token);
            if (!RecordAheadOfLoadManagedTransformationGate(gateC)) return;

            _aheadOfLoadManagedTransformationResultLabel.Text = "AHEAD-OF-LOAD MANAGED TRANSFORMATION BOUNDARY: GATE D RUNNING…";
            _statusLabel.Text = "STEP 28 GATE D — load ONLY transformed bytes and execute reflection + in-fixture direct-call routes; both must return 1041.";
            var gateD = await Task.Run(() => _aheadOfLoadManagedTransformation.RunTransformedExecution(), token);
            if (!RecordAheadOfLoadManagedTransformationGate(gateD)) return;

            _aheadOfLoadManagedTransformationResultLabel.Text = "AHEAD-OF-LOAD MANAGED TRANSFORMATION BOUNDARY: GATE E RUNNING…";
            _statusLabel.Text = "STEP 28 GATE E — final bundle/source/transformed hashes + OfflineReady + private-context isolation audit.";
            var gateE = await _aheadOfLoadManagedTransformation.RunFinalIsolationAuditAsync(progress, token);
            if (!RecordAheadOfLoadManagedTransformationGate(gateE)) return;

            var snapshot = _aheadOfLoadManagedTransformationGates.Snapshot();
            _aheadOfLoadManagedTransformationResultLabel.Text = snapshot.Summary;
            _aheadOfLoadManagedTransformationResultLabel.TextColor = UIColor.Label;
            _aheadOfLoadManagedTransformationDetailLabel.Text = FormatAheadOfLoadManagedTransformationDetail(
                "All five Step 28.0 gates passed. This closes the first architecture-pivot boundary: deterministic ahead-of-load semantic rewriting and post-publish interpreted execution work together without Harmony runtime detours. The next candidate may select a narrowly audited real StS2 compatibility transformation; this build deliberately does not do so.");
            _statusLabel.Text = "PASS: STEP 28.0 AHEAD-OF-LOAD MANAGED TRANSFORMATION — 5/5. Preserve the report and force-quit before another Step-28 run.";
            _statusLabel.TextColor = UIColor.Label;
        }
        catch (OperationCanceledException)
        {
            _aheadOfLoadManagedTransformationResultLabel.Text = "AHEAD-OF-LOAD MANAGED TRANSFORMATION BOUNDARY: CANCELLED";
            _aheadOfLoadManagedTransformationResultLabel.TextColor = UIColor.SecondaryLabel;
            _aheadOfLoadManagedTransformationDetailLabel.Text = FormatAheadOfLoadManagedTransformationDetail(
                "Step 28 was cancelled. The bundle source and Step 12 managed install remain read-only; the launcher-private Step28-AheadOfLoadTransformation workspace is disposable scratch state.");
            _statusLabel.Text = "STEP 28 CANCELLED — later gates are unproven.";
            _statusLabel.TextColor = UIColor.SecondaryLabel;
        }
        catch (Exception ex)
        {
            _aheadOfLoadManagedTransformationResultLabel.Text = "AHEAD-OF-LOAD MANAGED TRANSFORMATION BOUNDARY: EXCEPTION";
            _aheadOfLoadManagedTransformationResultLabel.TextColor = UIColor.SystemRed;
            _aheadOfLoadManagedTransformationDetailLabel.Text = FormatAheadOfLoadManagedTransformationDetail($"Unhandled Step 28 exception: {ex.GetType().Name}: {ex.Message}");
            _statusLabel.Text = "STEP 28 FAIL — stop at the current gate and preserve the Files report.";
            _statusLabel.TextColor = UIColor.SystemRed;
        }
        finally
        {
            await WriteDeviceTestReportFromLabelsAsync(
                "Step28-AheadOfLoadManagedTransformation.txt",
                "StS2 Launcher — Step 28 Ahead-of-Load Managed Transformation",
                _aheadOfLoadManagedTransformationResultLabel,
                _aheadOfLoadManagedTransformationDetailLabel,
                CancellationToken.None);
            EndSteamOperation();
        }
    }

    private bool RecordAheadOfLoadManagedTransformationGate(AheadOfLoadManagedTransformationGateResult result)
    {
        _aheadOfLoadManagedTransformationGates.Record(result.Gate, result.Passed, result.Detail);
        if (_aheadOfLoadManagedTransformationResultLabel is not null)
        {
            _aheadOfLoadManagedTransformationResultLabel.Text = _aheadOfLoadManagedTransformationGates.Snapshot().Summary;
            _aheadOfLoadManagedTransformationResultLabel.TextColor = result.Passed ? UIColor.Label : UIColor.SystemRed;
        }
        if (_aheadOfLoadManagedTransformationDetailLabel is not null)
            _aheadOfLoadManagedTransformationDetailLabel.Text = FormatAheadOfLoadManagedTransformationDetail(result.Detail);
        if (!result.Passed && _statusLabel is not null)
        {
            var letter = (char)('A' + (int)result.Gate - 1);
            _statusLabel.Text = $"STEP 28 FAIL at Gate {letter} ({result.Gate}). Stop here; later ahead-of-load transformation gates were not run.";
            _statusLabel.TextColor = UIColor.SystemRed;
        }
        return result.Passed;
    }

    private string FormatAheadOfLoadManagedTransformationDetail(string tail)
    {
        var lines = new List<string>();
        foreach (var gate in _aheadOfLoadManagedTransformationGates.Results)
        {
            var letter = (char)('A' + (int)gate.Gate - 1);
            lines.Add($"Gate {letter} — {gate.Gate}: {(gate.Passed ? "PASS" : "FAIL")}");
            lines.Add(gate.Detail);
            lines.Add(string.Empty);
        }

        lines.Add("Step 27 physical decision: CLOSED NEGATIVE — 0.0.108 failed PatchProcessor.Patch() on a genuine post-publish interpreted target with System.NotImplementedException from PatchFunctions.UpdateWrapper.");
        lines.Add("Step 28 architecture: verify immutable source -> clone to launcher-private workspace -> deterministic Cecil transform -> verify transformed bytes/IL -> CLR-load only transformed image -> execute under interpreter.");
        lines.Add("Forbidden in Step 28.0: Harmony/MonoMod runtime patching, real StS2 member reflection/invocation, Godot/game startup, native game loading, mutation of the trusted Step 12 managed install, arbitrary resolver fallback.");
        lines.Add("Fresh-process rule: after Gate D loads the Step-28 fixture identity, force-quit before another Step-28 run so Gate A can prove the source identity was not previously resident.");
        lines.Add(tail);
        return string.Join("\n", lines);
    }
}
