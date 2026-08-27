using StS2Launcher.Core;
using UIKit;

namespace StS2Launcher.iOS;

public sealed partial class RootViewController
{
    private void AddControlledManagedInitializationControls(UIStackView content)
    {
        content.AddArrangedSubview(Separator());

        content.AddArrangedSubview(Label(
            "Step 24 — Controlled 0Harmony Module Initialization Boundary (ordered gates A–D)",
            UIFont.BoldSystemFontOfSize(25),
            UIColor.Label));

        _controlledManagedInitializationButton = SystemButton("Run Step 24 A–D — Audit Initializer → Replay Step 23 State → Initialize 0Harmony → Audit", 17);
        _controlledManagedInitializationButton.TouchUpInside += async (_, _) => await RunControlledManagedInitializationAsync();
        content.AddArrangedSubview(_controlledManagedInitializationButton);

        _controlledManagedInitializationResultLabel = Label(
            "CONTROLLED MANAGED INITIALIZATION BOUNDARY: NOT RUN",
            UIFont.BoldSystemFontOfSize(21),
            UIColor.Label);
        content.AddArrangedSubview(_controlledManagedInitializationResultLabel);

        _controlledManagedInitializationDetailLabel = Label(
            "Gate A replays the accepted Step 23 preflight and requires exactly one initializer-bearing dependency: 0Harmony 2.4.2.0 with exactly one <Module>..cctor. A bounded Cecil automatic-initialization audit follows direct same-assembly calls plus type constructors that static calls/fields could implicitly trigger, and rejects P/Invoke, calli, function/delegate indirection, native-library APIs, explicit runtime-constructor APIs, reflection/dynamic invocation, and unexpected non-framework execution edges before loading anything. Gate B recreates the physically proven Step 23 initializer-free private context. Gate C admits exactly 0Harmony, loads it from the receipt-hashed prepared bytes, and uses RuntimeHelpers.RunModuleConstructor as the explicit completion barrier while the strict resolver still refuses native and unplanned managed loads. Gate D re-hashes the plan and every prepared/live file, re-proves OfflineReady, and requires the private context to equal the Step 23 closure plus exactly 0Harmony. No Harmony patch API or game/Godot method is invoked.",
            UIFont.SystemFontOfSize(15),
            UIColor.SecondaryLabel);
        content.AddArrangedSubview(_controlledManagedInitializationDetailLabel);
    }

    private async Task RunControlledManagedInitializationAsync()
    {
        if (_controlledManagedInitializationResultLabel is null ||
            _controlledManagedInitializationDetailLabel is null ||
            _controlledManagedInitializationButton is null ||
            _statusLabel is null)
        {
            return;
        }

        if (_godotProcessRequiresRestart || _godotSessionStarted)
        {
            _statusLabel.Text = "Step 24 requires a fresh process. Force-quit/relaunch if the Step 15 Godot host has been started.";
            _statusLabel.TextColor = UIColor.SystemOrange;
            return;
        }

        BeginSteamOperation(allowCancel: true);
        _controlledManagedInitializationGates.Reset();
        _controlledManagedInitialization.Reset();
        _controlledManagedInitializationResultLabel.Text = "CONTROLLED MANAGED INITIALIZATION BOUNDARY: GATE A RUNNING…";
        _controlledManagedInitializationResultLabel.TextColor = UIColor.Label;
        _controlledManagedInitializationDetailLabel.Text = "Gate A: replaying the accepted Step 23 preflight and statically auditing the sole deferred 0Harmony module initializer before any Step 24 CLR load.";
        _statusLabel.Text = "STEP 24 GATE A — initializer preflight. No Step 24 game/Harmony assembly is loaded yet.";
        _statusLabel.TextColor = UIColor.Label;

        try
        {
            var token = _operationCts?.Token ?? CancellationToken.None;
            var progress = new Progress<ControlledManagedInitializationProgress>(value =>
            {
                var count = value.TotalItems > 0 ? $" ({value.ProcessedItems:N0}/{value.TotalItems:N0})" : string.Empty;
                _controlledManagedInitializationDetailLabel.Text = FormatControlledManagedInitializationDetail(
                    $"Gate {(char)('A' + (int)value.Gate - 1)} progress{count}: {value.Detail}" +
                    (string.IsNullOrWhiteSpace(value.CurrentPath) ? string.Empty : $"\nCurrent: {value.CurrentPath}"));
            });

            var gateA = await _controlledManagedInitialization.RunInitializationPreflightAsync(progress, token);
            if (!RecordControlledManagedInitializationGate(gateA)) return;

            _controlledManagedInitializationResultLabel.Text = "CONTROLLED MANAGED INITIALIZATION BOUNDARY: GATE B RUNNING…";
            _statusLabel.Text = "STEP 24 GATE B — replaying the physically proven Step 23 initializer-free load state in the Step 24 context.";
            var gateB = await Task.Run(() => _controlledManagedInitialization.RunProvenLoadStateReplay(), token);
            if (!RecordControlledManagedInitializationGate(gateB)) return;

            _controlledManagedInitializationResultLabel.Text = "CONTROLLED MANAGED INITIALIZATION BOUNDARY: GATE C RUNNING…";
            _statusLabel.Text = "STEP 24 GATE C — CONTROLLED 0Harmony LOAD + MODULE CONSTRUCTOR. Native and unplanned managed resolution remain blocked.";
            var gateC = await Task.Run(() => _controlledManagedInitialization.RunDeferredModuleInitialization(), token);
            if (!RecordControlledManagedInitializationGate(gateC)) return;

            _controlledManagedInitializationResultLabel.Text = "CONTROLLED MANAGED INITIALIZATION BOUNDARY: GATE D RUNNING…";
            _statusLabel.Text = "STEP 24 GATE D — post-initialization plan/byte/context/native-isolation audit.";
            var gateD = await _controlledManagedInitialization.RunPostInitializationAuditAsync(progress, token);
            if (!RecordControlledManagedInitializationGate(gateD)) return;

            var snapshot = _controlledManagedInitializationGates.Snapshot();
            _controlledManagedInitializationResultLabel.Text = snapshot.Summary;
            _controlledManagedInitializationResultLabel.TextColor = UIColor.Label;
            _controlledManagedInitializationDetailLabel.Text = FormatControlledManagedInitializationDetail(
                "All four Step 24 gates passed. The accepted Step 23 managed load state was reproduced, exactly 0Harmony 2.4.2.0 entered that private context, and its module constructor completion barrier returned successfully with no native or unplanned managed resolution. No Harmony patch API, game member, entry point, Godot startup, or native game library was invoked. Run OfflineReady + Foundation 5/5 to close Step 24. Force-quit before rerunning any earlier fresh-process managed-load regression.");
            _statusLabel.Text = "PASS: STEP 24 CONTROLLED MANAGED INITIALIZATION BOUNDARY — 4/4. Run OfflineReady + Foundation 5/5 for closure.";
            _statusLabel.TextColor = UIColor.Label;
        }
        catch (OperationCanceledException)
        {
            _controlledManagedInitializationResultLabel.Text = "CONTROLLED MANAGED INITIALIZATION BOUNDARY: CANCELLED";
            _controlledManagedInitializationResultLabel.TextColor = UIColor.SecondaryLabel;
            _controlledManagedInitializationDetailLabel.Text = FormatControlledManagedInitializationDetail(
                "Step 24 was cancelled. If Gate B had started, force-quit before retrying so Gate A begins from a fresh process.");
            _statusLabel.Text = "STEP 24 CANCELLED — no later gate is considered proven.";
            _statusLabel.TextColor = UIColor.SecondaryLabel;
        }
        catch (Exception ex)
        {
            _controlledManagedInitializationResultLabel.Text = "CONTROLLED MANAGED INITIALIZATION BOUNDARY: EXCEPTION";
            _controlledManagedInitializationResultLabel.TextColor = UIColor.SystemRed;
            _controlledManagedInitializationDetailLabel.Text = FormatControlledManagedInitializationDetail($"Unhandled Step 24 exception: {ex.GetType().Name}: {ex.Message}");
            _statusLabel.Text = "STEP 24 FAIL — stop at the first failing gate. Force-quit before another attempt if Gate B had started.";
            _statusLabel.TextColor = UIColor.SystemRed;
        }
        finally
        {
            await WriteDeviceTestReportFromLabelsAsync(
                "Step24-ControlledManagedInitialization.txt",
                "StS2 Launcher — Step 24 Controlled 0Harmony Module Initialization Boundary",
                _controlledManagedInitializationResultLabel,
                _controlledManagedInitializationDetailLabel,
                CancellationToken.None);
            EndSteamOperation();
        }
    }

    private bool RecordControlledManagedInitializationGate(ControlledManagedInitializationGateResult result)
    {
        _controlledManagedInitializationGates.Record(result);
        if (_controlledManagedInitializationResultLabel is not null)
        {
            _controlledManagedInitializationResultLabel.Text = _controlledManagedInitializationGates.Snapshot().Summary;
            _controlledManagedInitializationResultLabel.TextColor = result.Passed ? UIColor.Label : UIColor.SystemRed;
        }
        if (_controlledManagedInitializationDetailLabel is not null)
            _controlledManagedInitializationDetailLabel.Text = FormatControlledManagedInitializationDetail(result.Detail);
        if (!result.Passed && _statusLabel is not null)
        {
            var letter = (char)('A' + (int)result.Gate - 1);
            _statusLabel.Text = $"STEP 24 FAIL at Gate {letter} ({result.Gate}). Stop here; later initialization gates were not run.";
            _statusLabel.TextColor = UIColor.SystemRed;
        }
        return result.Passed;
    }

    private string FormatControlledManagedInitializationDetail(string tail)
    {
        var lines = new List<string>();
        foreach (var gate in _controlledManagedInitializationGates.Results)
        {
            var letter = (char)('A' + (int)gate.Gate - 1);
            lines.Add($"Gate {letter} — {gate.Gate}: {(gate.Passed ? "PASS" : "FAIL")}");
            lines.Add(gate.Detail);
            lines.Add(string.Empty);
        }

        lines.Add("Step 24 prerequisite: physical Step 23.4.3 is closed — Gates A–D PASS, OfflineReady PASS, Foundation 5/5 PASS.");
        lines.Add("Gate A is metadata-only and fail-closed: exactly one deferred initializer-bearing dependency is permitted, exactly 0Harmony 2.4.2.0 with one <Module>..cctor. The bounded same-assembly automatic-initialization closure must contain no P/Invoke/calli/function-pointer or delegate indirection/native loader/explicit runtime-constructor, reflection/dynamic invocation, or unexpected non-framework execution edge; implicitly triggerable same-assembly type constructors are included in the audit.");
        lines.Add("Gate B reproduces the accepted Step 23 initializer-free managed closure in the same dedicated context used by Gate C; 0Harmony must still be absent after Gate B.");
        lines.Add("Gate C is the only new execution boundary: exactly 0Harmony may enter the context, and RuntimeHelpers.RunModuleConstructor is the explicit module-constructor completion barrier. The strict resolver still refuses every native and unplanned managed request.");
        lines.Add("Gate D requires the private context to equal the Step 23 initializer-free closure plus exactly 0Harmony, while all plan/prepared/live hashes and OfflineReady remain unchanged.");
        lines.Add("Still forbidden: explicit Harmony APIs/patching, sts2 game entry point or type/member reflection/invocation, Activator/CreateInstance, broad reflection/dynamic invocation, Godot/game startup, and native game-library loading.");
        lines.Add("After Gate B, managed game assemblies remain process-resident until force-quit. Do not rerun Step 21/22/23 fresh-process regressions in the same process.");
        lines.Add(tail);
        return string.Join("\n", lines);
    }
}
