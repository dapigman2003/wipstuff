using StS2Launcher.Core;
using UIKit;

namespace StS2Launcher.iOS;

public sealed partial class RootViewController
{
    private async Task RunFirstRealGameAssemblyLoadAsync()
    {
        if (_firstRealGameAssemblyLoadResultLabel is null ||
            _firstRealGameAssemblyLoadDetailLabel is null ||
            _firstRealGameAssemblyLoadButton is null ||
            _statusLabel is null)
        {
            return;
        }

        if (_godotProcessRequiresRestart || _godotSessionStarted)
        {
            _statusLabel.Text = "Step 23 requires a fresh process before the first real sts2.dll CLR load. Force-quit/relaunch if the Step 15 Godot host has been started in this process.";
            _statusLabel.TextColor = UIColor.SystemOrange;
            return;
        }

        BeginSteamOperation(allowCancel: true);
        _firstRealGameAssemblyLoadGates.Reset();
        _firstRealGameAssemblyLoad.Reset();
        _firstRealGameAssemblyLoadResultLabel.Text = "FIRST REAL STS2 CLR LOAD BOUNDARY: GATE A RUNNING…";
        _firstRealGameAssemblyLoadResultLabel.TextColor = UIColor.Label;
        _firstRealGameAssemblyLoadDetailLabel.Text = "Gate A: validating the current zero-blocker Step 21/22 plan, receipt-identical prepared/live bytes and module-initializer-free load boundary before any real game CLR load.";
        _statusLabel.Text = "STEP 23 GATE A — prepared-load preflight. No game assembly is loaded yet.";
        _statusLabel.TextColor = UIColor.Label;

        try
        {
            var token = _operationCts?.Token ?? CancellationToken.None;
            var progress = new Progress<FirstRealGameAssemblyLoadProgress>(value =>
            {
                var count = value.TotalItems > 0 ? $" ({value.ProcessedItems:N0}/{value.TotalItems:N0})" : string.Empty;
                _firstRealGameAssemblyLoadDetailLabel.Text = FormatFirstRealGameAssemblyLoadDetail(
                    $"Gate {(char)('A' + (int)value.Gate - 1)} progress{count}: {value.Detail}" +
                    (string.IsNullOrWhiteSpace(value.CurrentPath) ? string.Empty : $"\nCurrent: {value.CurrentPath}"));
            });

            var gateA = await _firstRealGameAssemblyLoad.RunPreparedLoadPreflightAsync(progress, token);
            if (!RecordFirstRealGameAssemblyLoadGate(gateA)) return;

            _firstRealGameAssemblyLoadResultLabel.Text = "FIRST REAL STS2 CLR LOAD BOUNDARY: GATE B RUNNING…";
            _statusLabel.Text = "STEP 23 GATE B — FIRST REAL sts2.dll CLR LOAD. No entry point or game member is invoked.";
            var gateB = await Task.Run(() => _firstRealGameAssemblyLoad.RunPrimaryAssemblyLoad(), token);
            if (!RecordFirstRealGameAssemblyLoadGate(gateB)) return;

            _firstRealGameAssemblyLoadResultLabel.Text = "FIRST REAL STS2 CLR LOAD BOUNDARY: GATE C RUNNING…";
            _statusLabel.Text = "STEP 23 GATE C — strict runtime resolution of the complete audited managed dependency plan.";
            var gateC = await Task.Run(() => _firstRealGameAssemblyLoad.RunPlannedDependencyResolution(), token);
            if (!RecordFirstRealGameAssemblyLoadGate(gateC)) return;

            _firstRealGameAssemblyLoadResultLabel.Text = "FIRST REAL STS2 CLR LOAD BOUNDARY: GATE D RUNNING…";
            _statusLabel.Text = "STEP 23 GATE D — post-load plan/byte/context/native-isolation audit.";
            var gateD = await _firstRealGameAssemblyLoad.RunLoadIsolationAuditAsync(progress, token);
            if (!RecordFirstRealGameAssemblyLoadGate(gateD)) return;

            var snapshot = _firstRealGameAssemblyLoadGates.Snapshot();
            _firstRealGameAssemblyLoadResultLabel.Text = snapshot.Summary;
            _firstRealGameAssemblyLoadResultLabel.TextColor = UIColor.Label;
            _firstRealGameAssemblyLoadDetailLabel.Text = FormatFirstRealGameAssemblyLoadDetail(
                "All four Step 23 gates passed. The real receipt-backed sts2.dll and its complete planned managed closure are CLR-loadable in the private interpreter-backed context without entry-point/member invocation, native resolution, or live-install mutation. Run OfflineReady + Foundation 5/5 to close Step 23. Force-quit before rerunning any earlier pre-load regression that requires no real game assembly in the CLR.");
            _statusLabel.Text = "PASS: STEP 23 FIRST REAL STS2 CLR LOAD BOUNDARY — 4/4. Real managed load + planned dependency binding are proven; game initialization/execution remains out of scope.";
            _statusLabel.TextColor = UIColor.Label;
        }
        catch (OperationCanceledException)
        {
            _firstRealGameAssemblyLoadResultLabel.Text = "FIRST REAL STS2 CLR LOAD BOUNDARY: CANCELLED";
            _firstRealGameAssemblyLoadResultLabel.TextColor = UIColor.SecondaryLabel;
            _firstRealGameAssemblyLoadDetailLabel.Text = FormatFirstRealGameAssemblyLoadDetail(
                "Step 23 was cancelled. If Gate B had already loaded sts2.dll, force-quit before retrying so Gate A starts from a fresh process.");
            _statusLabel.Text = "STEP 23 CANCELLED — no later gate is considered proven.";
            _statusLabel.TextColor = UIColor.SecondaryLabel;
        }
        catch (Exception ex)
        {
            _firstRealGameAssemblyLoadResultLabel.Text = "FIRST REAL STS2 CLR LOAD BOUNDARY: EXCEPTION";
            _firstRealGameAssemblyLoadResultLabel.TextColor = UIColor.SystemRed;
            _firstRealGameAssemblyLoadDetailLabel.Text = FormatFirstRealGameAssemblyLoadDetail($"Unhandled Step 23 exception: {ex.GetType().Name}: {ex.Message}");
            _statusLabel.Text = "STEP 23 FAIL — stop at the first failing gate. If Gate B had started, force-quit before another attempt.";
            _statusLabel.TextColor = UIColor.SystemRed;
        }
        finally
        {
            await WriteDeviceTestReportFromLabelsAsync(
                "Step23-FirstRealGameLoad.txt",
                "StS2 Launcher — Step 23 First Real StS2 CLR Load Boundary",
                _firstRealGameAssemblyLoadResultLabel,
                _firstRealGameAssemblyLoadDetailLabel,
                CancellationToken.None);
            EndSteamOperation();
        }
    }

    private bool RecordFirstRealGameAssemblyLoadGate(FirstRealGameAssemblyLoadGateResult result)
    {
        _firstRealGameAssemblyLoadGates.Record(result);
        if (_firstRealGameAssemblyLoadResultLabel is not null)
        {
            _firstRealGameAssemblyLoadResultLabel.Text = _firstRealGameAssemblyLoadGates.Snapshot().Summary;
            _firstRealGameAssemblyLoadResultLabel.TextColor = result.Passed ? UIColor.Label : UIColor.SystemRed;
        }
        if (_firstRealGameAssemblyLoadDetailLabel is not null)
            _firstRealGameAssemblyLoadDetailLabel.Text = FormatFirstRealGameAssemblyLoadDetail(result.Detail);
        if (!result.Passed && _statusLabel is not null)
        {
            var letter = (char)('A' + (int)result.Gate - 1);
            _statusLabel.Text = $"STEP 23 FAIL at Gate {letter} ({result.Gate}). Stop here; later first-load gates were not run.";
            _statusLabel.TextColor = UIColor.SystemRed;
        }
        return result.Passed;
    }

    private string FormatFirstRealGameAssemblyLoadDetail(string tail)
    {
        var lines = new List<string>();
        foreach (var gate in _firstRealGameAssemblyLoadGates.Results)
        {
            var letter = (char)('A' + (int)gate.Gate - 1);
            lines.Add($"Gate {letter} — {gate.Gate}: {(gate.Passed ? "PASS" : "FAIL")}");
            lines.Add(gate.Detail);
            lines.Add(string.Empty);
        }

        lines.Add("Step 23 read scope: the persisted Step21-PreparedRuntimeBinding prepared set + binding plan and the receipt-backed Step 12 managed install. Step 23 does not rewrite/copy game assemblies.");
        lines.Add("CLR scope: real sts2.dll plus the exact zero-blocker managed closure from the Step 21/22 plan in one dedicated private AssemblyLoadContext; host frameworks remain in AssemblyLoadContext.Default.");
        lines.Add("Initialization boundary: no game entry point, no game type/member reflection, no game method/delegate invocation, no RuntimeHelpers.RunClassConstructor, no Activator/CreateInstance, and no Godot/game initialization are permitted in Step 23.");
        lines.Add("Module-initializer policy: Gate A refuses to load if any prepared private assembly contains <Module>..cctor, because module initialization can run as part of assembly loading.");
        lines.Add("Native boundary: the private load context refuses and audits unmanaged-library resolution. FMOD/Spine/Steamworks native integration and other native game dependencies remain later work.");
        lines.Add("Steps 01–22 and the Step 22.4.2 canonical foundation remain protected. Closure requires OfflineReady + Foundation 5/5 after a Step 23 4/4 pass.");
        lines.Add("After Gate B, the real game assembly remains process-resident until force-quit. Do not rerun Step 21/22 pre-load gates in the same process.");
        lines.Add(tail);
        return string.Join("\n", lines);
    }
}
