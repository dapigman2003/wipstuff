using StS2Launcher.Core;
using UIKit;

namespace StS2Launcher.iOS;

public sealed partial class RootViewController
{
    private readonly TransformedRealStS2AssemblyAdmissionGateSequence _transformedRealStS2AssemblyAdmissionGates = new();
    private UILabel? _transformedRealStS2AssemblyAdmissionResultLabel;
    private UILabel? _transformedRealStS2AssemblyAdmissionDetailLabel;
    private UIButton? _transformedRealStS2AssemblyAdmissionButton;

    private void AddTransformedRealStS2AssemblyAdmissionControls(UIStackView content)
    {
        content.AddArrangedSubview(Separator());
        content.AddArrangedSubview(Label(
            "Step 33.0 — Verified Transformed Real-StS2 CLR Admission (ordered gates A–D)",
            UIFont.BoldSystemFontOfSize(25),
            UIColor.Label));

        _transformedRealStS2AssemblyAdmissionButton = SystemButton(
            "Run Step 33 A–D — Requalify Step 32 Image → Load Exact Transformed sts2.dll → Audit Resolver Isolation → Re-Prove Original Isolation",
            17);
        _transformedRealStS2AssemblyAdmissionButton.TouchUpInside += async (_, _) => await RunTransformedRealStS2AssemblyAdmissionAsync();
        content.AddArrangedSubview(_transformedRealStS2AssemblyAdmissionButton);

        _transformedRealStS2AssemblyAdmissionResultLabel = Label(
            "TRANSFORMED REAL STS2 CLR ADMISSION: NOT RUN",
            UIFont.BoldSystemFontOfSize(21),
            UIColor.Label);
        content.AddArrangedSubview(_transformedRealStS2AssemblyAdmissionResultLabel);

        _transformedRealStS2AssemblyAdmissionDetailLabel = Label(
            "Physical 0.0.120 closed Step 32 at 4/4 and proved the exact launcher-private transformed sts2.dll SHA-256 39c0a89ad0d5c6eb1553e23dd8537a7b7ab8278fad4115d186db5751570211ef. Step 33 is admission-only. Gate A re-runs Step 32 A–D, requires the exact closed transformed hash/identity/semantic fingerprint, and requalifies the prepared Step-21/22 runtime plan without CLR-loading StS2. Gate B LoadFromStream-loads only those exact transformed bytes into a dedicated private AssemblyLoadContext and stops after identity/MVID/context verification. Gate C requires the private context to contain only transformed sts2, with no private dependency admission, no unplanned managed request and no native request. Gate D re-proves OfflineReady, original/transformed/plan hashes and unique transformed-context residency. No game member invocation or Godot/game startup is authorized.",
            UIFont.SystemFontOfSize(15),
            UIColor.SecondaryLabel);
        content.AddArrangedSubview(_transformedRealStS2AssemblyAdmissionDetailLabel);
    }

    private async Task RunTransformedRealStS2AssemblyAdmissionAsync()
    {
        if (_transformedRealStS2AssemblyAdmissionResultLabel is null ||
            _transformedRealStS2AssemblyAdmissionDetailLabel is null ||
            _transformedRealStS2AssemblyAdmissionButton is null ||
            _statusLabel is null)
            return;

        if (!CurrentReleasePresentation.BundleIdentityMatchesExpected)
        {
            _transformedRealStS2AssemblyAdmissionResultLabel.Text = "TRANSFORMED REAL STS2 CLR ADMISSION: RELEASE IDENTITY FAIL";
            _transformedRealStS2AssemblyAdmissionResultLabel.TextColor = UIColor.SystemRed;
            _transformedRealStS2AssemblyAdmissionDetailLabel.Text =
                $"Expected {CurrentReleasePresentation.ExpectedDisplayVersion} ({CurrentReleasePresentation.ExpectedBuildVersion}), observed {CurrentReleasePresentation.DisplayVersion} ({CurrentReleasePresentation.DisplayBuild}). Refusing Step 33 so transformed-admission evidence cannot be attributed to the wrong candidate.";
            _statusLabel.Text = "STEP 33 REFUSED — built bundle identity does not match the source-pinned candidate.";
            _statusLabel.TextColor = UIColor.SystemRed;
            return;
        }

        if (_godotProcessRequiresRestart || _godotSessionStarted)
        {
            _statusLabel.Text = "Step 33 requires a fresh process with no Godot process-global state and no sts2 assembly already loaded. Force-quit/relaunch before running Step 33.";
            _statusLabel.TextColor = UIColor.SystemOrange;
            return;
        }

        BeginSteamOperation(allowCancel: true);
        _transformedRealStS2AssemblyAdmissionGates.Reset();
        _transformedRealStS2AssemblyAdmission.Reset();
        _transformedRealStS2AssemblyAdmissionResultLabel.Text = "TRANSFORMED REAL STS2 CLR ADMISSION: GATE A RUNNING…";
        _transformedRealStS2AssemblyAdmissionResultLabel.TextColor = UIColor.Label;
        _statusLabel.Text = "STEP 33 GATE A — re-manufacture/reverify exact Step-32 transformed image and requalify the prepared runtime plan; no CLR admission yet.";
        _statusLabel.TextColor = UIColor.Label;

        try
        {
            var token = _operationCts?.Token ?? CancellationToken.None;
            var progress = new Progress<TransformedRealStS2AssemblyAdmissionProgress>(value =>
            {
                var count = value.TotalItems > 0 ? $" ({value.ProcessedItems:N0}/{value.TotalItems:N0})" : string.Empty;
                _transformedRealStS2AssemblyAdmissionDetailLabel.Text = FormatTransformedRealStS2AssemblyAdmissionDetail(
                    $"Gate {(char)('A' + (int)value.Gate - 1)} progress{count}: {value.Detail}" +
                    (string.IsNullOrWhiteSpace(value.CurrentPath) ? string.Empty : $"\nCurrent: {value.CurrentPath}"));
            });

            var gateA = await _transformedRealStS2AssemblyAdmission.RunVerifiedTransformedImagePreflightAsync(progress, token);
            if (!RecordTransformedRealStS2AssemblyAdmissionGate(gateA)) return;

            _transformedRealStS2AssemblyAdmissionResultLabel.Text = "TRANSFORMED REAL STS2 CLR ADMISSION: GATE B RUNNING…";
            _statusLabel.Text = "STEP 33 GATE B — load ONLY the exact verified transformed sts2.dll into the dedicated private CLR context; no member invocation.";
            var gateB = await Task.Run(() => _transformedRealStS2AssemblyAdmission.RunTransformedPrimaryClrAdmission(), token);
            if (!RecordTransformedRealStS2AssemblyAdmissionGate(gateB)) return;

            _transformedRealStS2AssemblyAdmissionResultLabel.Text = "TRANSFORMED REAL STS2 CLR ADMISSION: GATE C RUNNING…";
            _statusLabel.Text = "STEP 33 GATE C — prove the admission-only context contains transformed sts2 only and no private/native boundary was crossed.";
            var gateC = await Task.Run(() => _transformedRealStS2AssemblyAdmission.RunAdmissionOnlyResolverAudit(), token);
            if (!RecordTransformedRealStS2AssemblyAdmissionGate(gateC)) return;

            _transformedRealStS2AssemblyAdmissionResultLabel.Text = "TRANSFORMED REAL STS2 CLR ADMISSION: GATE D RUNNING…";
            _statusLabel.Text = "STEP 33 GATE D — OfflineReady + source/transformed/plan hashes + unique transformed-context residency final audit.";
            var gateD = await _transformedRealStS2AssemblyAdmission.RunFinalIsolationAuditAsync(progress, token);
            if (!RecordTransformedRealStS2AssemblyAdmissionGate(gateD)) return;

            var snapshot = _transformedRealStS2AssemblyAdmissionGates.Snapshot();
            _transformedRealStS2AssemblyAdmissionResultLabel.Text = snapshot.Summary;
            _transformedRealStS2AssemblyAdmissionResultLabel.TextColor = UIColor.Label;
            _transformedRealStS2AssemblyAdmissionDetailLabel.Text = FormatTransformedRealStS2AssemblyAdmissionDetail(
                "All four Step 33 gates passed. Preserve this report. A pass proves the independently verified transformed real-StS2 image—not the receipt-backed/prepared original—can enter the dedicated iOS CLR context without game member invocation, private dependency admission, native loading, or Godot/game startup. A later separately gated boundary may prepare for controlled transformed-site execution.");
            _statusLabel.Text = "PASS: STEP 33 TRANSFORMED REAL STS2 CLR ADMISSION — 4/4. Exact transformed image is CLR-resident; execution remains unauthorized.";
            _statusLabel.TextColor = UIColor.Label;
        }
        catch (OperationCanceledException)
        {
            _transformedRealStS2AssemblyAdmissionResultLabel.Text = "TRANSFORMED REAL STS2 CLR ADMISSION: CANCELLED";
            _transformedRealStS2AssemblyAdmissionResultLabel.TextColor = UIColor.SecondaryLabel;
            _transformedRealStS2AssemblyAdmissionDetailLabel.Text = FormatTransformedRealStS2AssemblyAdmissionDetail(
                "Step 33 was cancelled. If Gate B had begun, force-quit before retrying because transformed sts2 may now be CLR-resident.");
            _statusLabel.Text = "STEP 33 CANCELLED — later admission gates are unproven; force-quit before retry if Gate B started.";
            _statusLabel.TextColor = UIColor.SecondaryLabel;
        }
        catch (Exception ex)
        {
            _transformedRealStS2AssemblyAdmissionResultLabel.Text = "TRANSFORMED REAL STS2 CLR ADMISSION: EXCEPTION";
            _transformedRealStS2AssemblyAdmissionResultLabel.TextColor = UIColor.SystemRed;
            _transformedRealStS2AssemblyAdmissionDetailLabel.Text = FormatTransformedRealStS2AssemblyAdmissionDetail($"Unhandled Step 33 exception: {ex.GetType().Name}: {ex.Message}");
            _statusLabel.Text = "STEP 33 FAIL — stop at the first failing gate and preserve the report. Force-quit before retry if Gate B started.";
            _statusLabel.TextColor = UIColor.SystemRed;
        }
        finally
        {
            await WriteDeviceTestReportFromLabelsAsync(
                "Step33-TransformedRealStS2AssemblyAdmission.txt",
                "StS2 Launcher — Step 33 Transformed Real StS2 CLR Admission",
                _transformedRealStS2AssemblyAdmissionResultLabel,
                _transformedRealStS2AssemblyAdmissionDetailLabel,
                CancellationToken.None);
            EndSteamOperation();
        }
    }

    private bool RecordTransformedRealStS2AssemblyAdmissionGate(TransformedRealStS2AssemblyAdmissionGateResult result)
    {
        _transformedRealStS2AssemblyAdmissionGates.Record(result);
        if (_transformedRealStS2AssemblyAdmissionResultLabel is not null)
        {
            _transformedRealStS2AssemblyAdmissionResultLabel.Text = _transformedRealStS2AssemblyAdmissionGates.Snapshot().Summary;
            _transformedRealStS2AssemblyAdmissionResultLabel.TextColor = result.Passed ? UIColor.Label : UIColor.SystemRed;
        }
        if (_transformedRealStS2AssemblyAdmissionDetailLabel is not null)
            _transformedRealStS2AssemblyAdmissionDetailLabel.Text = FormatTransformedRealStS2AssemblyAdmissionDetail(result.Detail);
        if (!result.Passed && _statusLabel is not null)
        {
            var letter = (char)('A' + (int)result.Gate - 1);
            _statusLabel.Text = $"STEP 33 FAIL at Gate {letter} ({result.Gate}). Stop here; later transformed-admission gates were not run.";
            _statusLabel.TextColor = UIColor.SystemRed;
        }
        return result.Passed;
    }

    private string FormatTransformedRealStS2AssemblyAdmissionDetail(string tail)
    {
        var lines = new List<string>();
        foreach (var gate in _transformedRealStS2AssemblyAdmissionGates.Results)
        {
            var letter = (char)('A' + (int)gate.Gate - 1);
            lines.Add($"Gate {letter} — {gate.Gate}: {(gate.Passed ? "PASS" : "FAIL")}");
            lines.Add(gate.Detail);
            lines.Add(string.Empty);
        }

        lines.Add("Step 32 physical baseline: CLOSED POSITIVE — 0.0.120 passed 4/4. Exact transformed SHA-256 39c0a89ad0d5c6eb1553e23dd8537a7b7ab8278fad4115d186db5751570211ef; transformed semantic fingerprint 47fadf2a46eda098f310b7d0ee54e37d1e952ac272fc966d16d557ed46a0b74a; zero PrepareMethod references; trusted install unchanged.");
        lines.Add("Step 33 scope: re-manufacture/reverify that exact transformed image, then CLR-admit only those transformed primary bytes into a dedicated private AssemblyLoadContext.");
        lines.Add("Resolver boundary: exact Step-21/22 host-framework bindings may be serviced from AssemblyLoadContext.Default if the CLR requests them during primary admission; private prepared dependency admission is refused and any such request fails closed for a later boundary.");
        lines.Add("Forbidden in Step 33: CLR admission of the receipt-backed/prepared original sts2.dll, private game dependency loading, game type/member reflection or invocation, entry-point execution, Godot/game startup, native game loading, Harmony/MonoMod runtime patching, and arbitrary resolver fallback.");
        lines.Add("After Gate B, transformed sts2 remains CLR-resident until force-quit on the physical non-collectible context. Do not rerun pre-load boundaries in the same process.");
        lines.Add(tail);
        return string.Join("\n", lines);
    }
}
