using StS2Launcher.Core;
using UIKit;

namespace StS2Launcher.iOS;

public sealed partial class RootViewController
{
    private void AddControlledHarmonyConstructionControls(UIStackView content)
    {
        content.AddArrangedSubview(Separator());

        content.AddArrangedSubview(Label(
            "Step 25 — Controlled Harmony API Resolution + Type Initialization + Instance Construction (ordered gates A–I)",
            UIFont.BoldSystemFontOfSize(25),
            UIColor.Label));

        _controlledHarmonyConstructionButton = SystemButton("Run Step 25 A–I — Replay Step 24 → Resolve Harmony API → Initialize Harmony Type → Construct Inert Harmony → Audit", 17);
        _controlledHarmonyConstructionButton.TouchUpInside += async (_, _) => await RunControlledHarmonyConstructionAsync();
        content.AddArrangedSubview(_controlledHarmonyConstructionButton);

        _controlledHarmonyConstructionResultLabel = Label(
            "CONTROLLED HARMONY CONSTRUCTION BOUNDARY: NOT RUN",
            UIFont.BoldSystemFontOfSize(21),
            UIColor.Label);
        content.AddArrangedSubview(_controlledHarmonyConstructionResultLabel);

        _controlledHarmonyConstructionDetailLabel = Label(
            "Gates A–D reproduce the physically closed Step 24 state in one fresh dedicated context. Gate A also metadata-audits the exact HarmonyLib.Harmony static initializer and .ctor(string): the type initializer must match the measured 0Harmony 2.4.2 ConditionalWeakTable cache setup, HARMONY_DEBUG must be absent, and debug-only constructor work must remain behind DEBUG=false. Gate E resolves only HarmonyLib.Harmony plus its exact type initializer, public .ctor(string), Id getter, and DEBUG field without executing the type initializer. Gate F explicitly completes the Harmony type initializer with RuntimeHelpers.RunClassConstructor; Gate G audits that state. Gate H invokes only the exact string constructor with the fixed launcher probe ID. Gate I re-hashes plan/prepared/live files, re-proves OfflineReady, and requires unchanged context membership. Patch/PatchAll/CreateProcessor, game reflection/invocation, Godot startup, and native game loading remain forbidden.",
            UIFont.SystemFontOfSize(15),
            UIColor.SecondaryLabel);
        content.AddArrangedSubview(_controlledHarmonyConstructionDetailLabel);
    }

    private async Task RunControlledHarmonyConstructionAsync()
    {
        if (_controlledHarmonyConstructionResultLabel is null ||
            _controlledHarmonyConstructionDetailLabel is null ||
            _controlledHarmonyConstructionButton is null ||
            _statusLabel is null)
        {
            return;
        }

        if (_godotProcessRequiresRestart || _godotSessionStarted)
        {
            _statusLabel.Text = "Step 25 requires a fresh process. Force-quit/relaunch if the Step 15 Godot host has been started.";
            _statusLabel.TextColor = UIColor.SystemOrange;
            return;
        }

        BeginSteamOperation(allowCancel: true);
        _controlledHarmonyConstructionGates.Reset();
        _controlledHarmonyConstruction.Reset();
        _controlledHarmonyConstructionResultLabel.Text = "CONTROLLED HARMONY CONSTRUCTION BOUNDARY: GATE A RUNNING…";
        _controlledHarmonyConstructionResultLabel.TextColor = UIColor.Label;
        _controlledHarmonyConstructionDetailLabel.Text = "Gate A: replaying the accepted Step 24 metadata preconditions and auditing the exact Harmony type initializer + Harmony(string) constructor before any Step 25 CLR load.";
        _statusLabel.Text = "STEP 25 GATE A — metadata-only preflight. No Step 25 game/Harmony assembly is loaded yet.";
        _statusLabel.TextColor = UIColor.Label;

        try
        {
            var token = _operationCts?.Token ?? CancellationToken.None;
            var progress = new Progress<ControlledHarmonyConstructionProgress>(value =>
            {
                var count = value.TotalItems > 0 ? $" ({value.ProcessedItems:N0}/{value.TotalItems:N0})" : string.Empty;
                _controlledHarmonyConstructionDetailLabel.Text = FormatControlledHarmonyConstructionDetail(
                    $"Gate {(char)('A' + (int)value.Gate - 1)} progress{count}: {value.Detail}" +
                    (string.IsNullOrWhiteSpace(value.CurrentPath) ? string.Empty : $"\nCurrent: {value.CurrentPath}"));
            });

            var gateA = await _controlledHarmonyConstruction.RunInitializationPreflightAsync(progress, token);
            if (!RecordControlledHarmonyConstructionGate(gateA)) return;

            _controlledHarmonyConstructionResultLabel.Text = "CONTROLLED HARMONY CONSTRUCTION BOUNDARY: GATE B RUNNING…";
            _statusLabel.Text = "STEP 25 GATE B — replaying the Step 23 initializer-free private state in the Step 25 context.";
            var gateB = await Task.Run(() => _controlledHarmonyConstruction.RunProvenLoadStateReplay(), token);
            if (!RecordControlledHarmonyConstructionGate(gateB)) return;

            _controlledHarmonyConstructionResultLabel.Text = "CONTROLLED HARMONY CONSTRUCTION BOUNDARY: GATE C RUNNING…";
            _statusLabel.Text = "STEP 25 GATE C — replaying the physically proven Step 24 0Harmony module-initialization boundary.";
            var gateC = await Task.Run(() => _controlledHarmonyConstruction.RunDeferredModuleInitialization(), token);
            if (!RecordControlledHarmonyConstructionGate(gateC)) return;

            _controlledHarmonyConstructionResultLabel.Text = "CONTROLLED HARMONY CONSTRUCTION BOUNDARY: GATE D RUNNING…";
            _statusLabel.Text = "STEP 25 GATE D — re-proving the closed Step 24 post-initialization isolation state.";
            var gateD = await _controlledHarmonyConstruction.RunProvenInitializationAuditAsync(progress, token);
            if (!RecordControlledHarmonyConstructionGate(gateD)) return;

            _controlledHarmonyConstructionResultLabel.Text = "CONTROLLED HARMONY CONSTRUCTION BOUNDARY: GATE E RUNNING…";
            _statusLabel.Text = "STEP 25 GATE E — resolving only HarmonyLib.Harmony, its exact .cctor/.ctor(string), Id, and DEBUG. No type initialization or object construction yet.";
            var gateE = await Task.Run(() => _controlledHarmonyConstruction.RunHarmonyApiResolution(), token);
            if (!RecordControlledHarmonyConstructionGate(gateE)) return;

            _controlledHarmonyConstructionResultLabel.Text = "CONTROLLED HARMONY CONSTRUCTION BOUNDARY: GATE F RUNNING…";
            _statusLabel.Text = "STEP 25 GATE F — explicitly completing the exact measured HarmonyLib.Harmony type initializer. No object construction or patch API.";
            var gateF = await Task.Run(() => _controlledHarmonyConstruction.RunHarmonyTypeInitialization(), token);
            if (!RecordControlledHarmonyConstructionGate(gateF)) return;

            _controlledHarmonyConstructionResultLabel.Text = "CONTROLLED HARMONY CONSTRUCTION BOUNDARY: GATE G RUNNING…";
            _statusLabel.Text = "STEP 25 GATE G — auditing Harmony type-initialization state before instance construction.";
            var gateG = await Task.Run(() => _controlledHarmonyConstruction.RunHarmonyTypeInitializationAudit(), token);
            if (!RecordControlledHarmonyConstructionGate(gateG)) return;

            _controlledHarmonyConstructionResultLabel.Text = "CONTROLLED HARMONY CONSTRUCTION BOUNDARY: GATE H RUNNING…";
            _statusLabel.Text = "STEP 25 GATE H — invoking only the exact inert Harmony(string) constructor. No patch API is permitted.";
            var gateH = await Task.Run(() => _controlledHarmonyConstruction.RunHarmonyInstanceConstruction(), token);
            if (!RecordControlledHarmonyConstructionGate(gateH)) return;

            _controlledHarmonyConstructionResultLabel.Text = "CONTROLLED HARMONY CONSTRUCTION BOUNDARY: GATE I RUNNING…";
            _statusLabel.Text = "STEP 25 GATE I — post-construction byte/plan/context/native-isolation audit.";
            var gateI = await _controlledHarmonyConstruction.RunPostConstructionAuditAsync(progress, token);
            if (!RecordControlledHarmonyConstructionGate(gateI)) return;

            var snapshot = _controlledHarmonyConstructionGates.Snapshot();
            _controlledHarmonyConstructionResultLabel.Text = snapshot.Summary;
            _controlledHarmonyConstructionResultLabel.TextColor = UIColor.Label;
            _controlledHarmonyConstructionDetailLabel.Text = FormatControlledHarmonyConstructionDetail(
                "All nine Step 25 gates passed. The physically closed Step 24 state was reproduced, the exact Harmony API surface was resolved, the exact measured Harmony type initializer completed under its own barrier, and one inert Harmony instance was constructed with the fixed launcher probe ID without patch APIs, game reflection/invocation, native resolution, or context drift. Run OfflineReady + Foundation 5/5 to close Step 25. Force-quit before rerunning any earlier fresh-process managed-load regression.");
            _statusLabel.Text = "PASS: STEP 25 CONTROLLED HARMONY CONSTRUCTION BOUNDARY — 9/9. Run OfflineReady + Foundation 5/5 for closure.";
            _statusLabel.TextColor = UIColor.Label;
        }
        catch (OperationCanceledException)
        {
            _controlledHarmonyConstructionResultLabel.Text = "CONTROLLED HARMONY CONSTRUCTION BOUNDARY: CANCELLED";
            _controlledHarmonyConstructionResultLabel.TextColor = UIColor.SecondaryLabel;
            _controlledHarmonyConstructionDetailLabel.Text = FormatControlledHarmonyConstructionDetail(
                "Step 25 was cancelled. If Gate B had started, force-quit before retrying so Gate A begins from a fresh process.");
            _statusLabel.Text = "STEP 25 CANCELLED — no later gate is considered proven.";
            _statusLabel.TextColor = UIColor.SecondaryLabel;
        }
        catch (Exception ex)
        {
            _controlledHarmonyConstructionResultLabel.Text = "CONTROLLED HARMONY CONSTRUCTION BOUNDARY: EXCEPTION";
            _controlledHarmonyConstructionResultLabel.TextColor = UIColor.SystemRed;
            _controlledHarmonyConstructionDetailLabel.Text = FormatControlledHarmonyConstructionDetail($"Unhandled Step 25 exception: {ex.GetType().Name}: {ex.Message}");
            _statusLabel.Text = "STEP 25 FAIL — stop at the first failing gate. Force-quit before another attempt if Gate B had started.";
            _statusLabel.TextColor = UIColor.SystemRed;
        }
        finally
        {
            await WriteDeviceTestReportFromLabelsAsync(
                "Step25-ControlledHarmonyConstruction.txt",
                "StS2 Launcher — Step 25 Controlled Harmony API Resolution + Type Initialization + Instance Construction Boundary",
                _controlledHarmonyConstructionResultLabel,
                _controlledHarmonyConstructionDetailLabel,
                CancellationToken.None);
            EndSteamOperation();
        }
    }

    private bool RecordControlledHarmonyConstructionGate(ControlledHarmonyConstructionGateResult result)
    {
        _controlledHarmonyConstructionGates.Record(result);
        if (_controlledHarmonyConstructionResultLabel is not null)
        {
            _controlledHarmonyConstructionResultLabel.Text = _controlledHarmonyConstructionGates.Snapshot().Summary;
            _controlledHarmonyConstructionResultLabel.TextColor = result.Passed ? UIColor.Label : UIColor.SystemRed;
        }
        if (_controlledHarmonyConstructionDetailLabel is not null)
            _controlledHarmonyConstructionDetailLabel.Text = FormatControlledHarmonyConstructionDetail(result.Detail);
        if (!result.Passed && _statusLabel is not null)
        {
            var letter = (char)('A' + (int)result.Gate - 1);
            _statusLabel.Text = $"STEP 25 FAIL at Gate {letter} ({result.Gate}). Stop here; later gates were not run.";
            _statusLabel.TextColor = UIColor.SystemRed;
        }
        return result.Passed;
    }

    private string FormatControlledHarmonyConstructionDetail(string tail)
    {
        var lines = new List<string>();
        foreach (var gate in _controlledHarmonyConstructionGates.Results)
        {
            var letter = (char)('A' + (int)gate.Gate - 1);
            lines.Add($"Gate {letter} — {gate.Gate}: {(gate.Passed ? "PASS" : "FAIL")}");
            lines.Add(gate.Detail);
            lines.Add(string.Empty);
        }

        lines.Add("Step 25 prerequisite: physical Step 24.0.6 / 0.0.79 is closed — Gates A–D PASS, OfflineReady PASS, Foundation 5/5 PASS.");
        lines.Add("Gates A–D replay the closed Step 24 chain in the exact Step 25 private context; the physically proven System.Collections.Concurrent preservation root remains active with TrimMode=full and MtouchInterpreter=-all.");
        lines.Add("Gate E crosses only targeted reflection/API resolution: exact 0Harmony 2.4.2.0 → public HarmonyLib.Harmony → exact measured .cctor, public .ctor(System.String), Id getter, and DEBUG field. It does not read DEBUG or execute the type initializer.");
        lines.Add("Gate F is the first new managed execution boundary: explicitly complete the exact Gate-A-measured HarmonyLib.Harmony type initializer with RuntimeHelpers.RunClassConstructor. No Harmony object is constructed.");
        lines.Add("Gate G audits the post-type-initialization hash/context/resolver state and requires Harmony.DEBUG=false before instance construction.");
        lines.Add("Gate H invokes exactly HarmonyLib.Harmony::.ctor(System.String) with the fixed launcher probe ID, then verifies the returned object's type, Id, load context, hashes, and resolver/native isolation.");
        lines.Add("Gate I requires the private context to remain identical to the closed Step 24 context while all plan/prepared/live hashes and OfflineReady remain unchanged.");
        lines.Add("Still forbidden: Harmony Patch/PatchAll/PatchCategory/CreateProcessor or any patching/processor API, broad Harmony reflection, sts2 game entry point or type/member reflection/invocation, Activator/CreateInstance, Godot/game startup, and native game-library loading.");
        lines.Add("After Gate B, managed game assemblies remain process-resident until force-quit. Do not rerun Step 21/22/23/24 fresh-process regressions in the same process.");
        lines.Add(tail);
        return string.Join("\n", lines);
    }
}
