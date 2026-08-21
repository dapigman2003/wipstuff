using StS2Launcher.Core;
using StS2Launcher.iOS.Platform;
using UIKit;

namespace StS2Launcher.iOS;

public sealed partial class RootViewController
{
    private void AddControlledHarmonyProcessorCreationControls(UIStackView content)
    {
        // Step 25.0.2 is physically closed. Keep its proven trimming preservation anchor rooted
        // while Step 26 advances beyond construction of the inert Harmony instance.
        Step25HarmonyConstructorFrameworkPreservation.Activate();

        content.AddArrangedSubview(Separator());
        content.AddArrangedSubview(Label(
            "Step 26 — Controlled Empty Harmony PatchProcessor Creation (ordered gates A–N)",
            UIFont.BoldSystemFontOfSize(25),
            UIColor.Label));

        _controlledHarmonyProcessorCreationButton = SystemButton(
            "Run Step 26 A–N — Replay Step 25 → Resolve Processor API → Initialize PatchProcessor Type → Resolve Launcher Probe → Create Empty Processor → Audit",
            17);
        _controlledHarmonyProcessorCreationButton.TouchUpInside += async (_, _) => await RunControlledHarmonyProcessorCreationAsync();
        content.AddArrangedSubview(_controlledHarmonyProcessorCreationButton);

        _controlledHarmonyProcessorCreationResultLabel = Label(
            "CONTROLLED HARMONY PROCESSOR CREATION BOUNDARY: NOT RUN",
            UIFont.BoldSystemFontOfSize(21),
            UIColor.Label);
        content.AddArrangedSubview(_controlledHarmonyProcessorCreationResultLabel);

        _controlledHarmonyProcessorCreationDetailLabel = Label(
            "Gates A–I replay the physically closed Step 25 boundary in one fresh dedicated context: exact 0Harmony module initialization, exact Harmony type initialization, and one inert Harmony(string) instance with the proven Step-25 probe ID. Gate J metadata-audits and resolves only exact Harmony.CreateProcessor(MethodBase), HarmonyLib.PatchProcessor, its measured object-locker .cctor, its field-storage-only .ctor(Harmony,MethodBase), and the exact retained instance/original fields without executing the PatchProcessor type initializer. Gate K explicitly completes only that measured PatchProcessor type initializer. Gate L resolves one launcher-owned inert host MethodInfo and never reflects StS2. Gate M invokes only Harmony.CreateProcessor(MethodBase), verifies the returned empty PatchProcessor retains the exact Harmony instance and launcher probe MethodBase, and never calls Patch(). Gate N re-hashes plan/prepared/live files, re-proves OfflineReady, and requires unchanged context/native/resolver state. Patch(), Harmony.Patch/PatchAll, HarmonyMethod creation, StS2 member reflection/invocation, Godot startup, and native game loading remain forbidden.",
            UIFont.SystemFontOfSize(15),
            UIColor.SecondaryLabel);
        content.AddArrangedSubview(_controlledHarmonyProcessorCreationDetailLabel);
    }

    private async Task RunControlledHarmonyProcessorCreationAsync()
    {
        if (_controlledHarmonyProcessorCreationResultLabel is null ||
            _controlledHarmonyProcessorCreationDetailLabel is null ||
            _controlledHarmonyProcessorCreationButton is null ||
            _statusLabel is null)
            return;

        if (_godotProcessRequiresRestart || _godotSessionStarted)
        {
            _statusLabel.Text = "Step 26 requires a fresh process. Force-quit/relaunch if the Step 15 Godot host has been started.";
            _statusLabel.TextColor = UIColor.SystemOrange;
            return;
        }

        BeginSteamOperation(allowCancel: true);
        _controlledHarmonyProcessorCreationGates.Reset();
        _controlledHarmonyProcessorCreation.Reset();
        _controlledHarmonyProcessorCreationResultLabel.Text = "CONTROLLED HARMONY PROCESSOR CREATION BOUNDARY: GATE A RUNNING…";
        _controlledHarmonyProcessorCreationResultLabel.TextColor = UIColor.Label;
        _controlledHarmonyProcessorCreationDetailLabel.Text = "Gate A: replaying the closed Step 25 metadata preconditions before any Step 26 CLR load.";
        _statusLabel.Text = "STEP 26 GATE A — metadata-only preflight. No Step 26 game/Harmony assembly is loaded yet.";
        _statusLabel.TextColor = UIColor.Label;

        try
        {
            var token = _operationCts?.Token ?? CancellationToken.None;
            var progress = new Progress<ControlledHarmonyProcessorCreationProgress>(value =>
            {
                var count = value.TotalItems > 0 ? $" ({value.ProcessedItems:N0}/{value.TotalItems:N0})" : string.Empty;
                _controlledHarmonyProcessorCreationDetailLabel.Text = FormatControlledHarmonyProcessorCreationDetail(
                    $"Gate {(char)('A' + (int)value.Gate - 1)} progress{count}: {value.Detail}" +
                    (string.IsNullOrWhiteSpace(value.CurrentPath) ? string.Empty : $"\nCurrent: {value.CurrentPath}"));
            });

            var gateA = await _controlledHarmonyProcessorCreation.RunInitializationPreflightAsync(progress, token);
            if (!RecordControlledHarmonyProcessorCreationGate(gateA)) return;

            _controlledHarmonyProcessorCreationResultLabel.Text = "CONTROLLED HARMONY PROCESSOR CREATION BOUNDARY: GATE B RUNNING…";
            _statusLabel.Text = "STEP 26 GATE B — replaying the Step 23 initializer-free private state.";
            var gateB = await Task.Run(() => _controlledHarmonyProcessorCreation.RunProvenLoadStateReplay(), token);
            if (!RecordControlledHarmonyProcessorCreationGate(gateB)) return;

            _controlledHarmonyProcessorCreationResultLabel.Text = "CONTROLLED HARMONY PROCESSOR CREATION BOUNDARY: GATE C RUNNING…";
            _statusLabel.Text = "STEP 26 GATE C — replaying the closed Step 24 0Harmony module initializer.";
            var gateC = await Task.Run(() => _controlledHarmonyProcessorCreation.RunDeferredModuleInitialization(), token);
            if (!RecordControlledHarmonyProcessorCreationGate(gateC)) return;

            _controlledHarmonyProcessorCreationResultLabel.Text = "CONTROLLED HARMONY PROCESSOR CREATION BOUNDARY: GATE D RUNNING…";
            _statusLabel.Text = "STEP 26 GATE D — re-proving the closed Step 24 post-initialization state.";
            var gateD = await _controlledHarmonyProcessorCreation.RunProvenInitializationAuditAsync(progress, token);
            if (!RecordControlledHarmonyProcessorCreationGate(gateD)) return;

            _controlledHarmonyProcessorCreationResultLabel.Text = "CONTROLLED HARMONY PROCESSOR CREATION BOUNDARY: GATE E RUNNING…";
            _statusLabel.Text = "STEP 26 GATE E — replaying exact Harmony API resolution without type initialization.";
            var gateE = await Task.Run(() => _controlledHarmonyProcessorCreation.RunHarmonyApiResolution(), token);
            if (!RecordControlledHarmonyProcessorCreationGate(gateE)) return;

            _controlledHarmonyProcessorCreationResultLabel.Text = "CONTROLLED HARMONY PROCESSOR CREATION BOUNDARY: GATE F RUNNING…";
            _statusLabel.Text = "STEP 26 GATE F — replaying exact Harmony type initialization.";
            var gateF = await Task.Run(() => _controlledHarmonyProcessorCreation.RunHarmonyTypeInitialization(), token);
            if (!RecordControlledHarmonyProcessorCreationGate(gateF)) return;

            _controlledHarmonyProcessorCreationResultLabel.Text = "CONTROLLED HARMONY PROCESSOR CREATION BOUNDARY: GATE G RUNNING…";
            _statusLabel.Text = "STEP 26 GATE G — re-auditing Harmony type-initialization state.";
            var gateG = await Task.Run(() => _controlledHarmonyProcessorCreation.RunHarmonyTypeInitializationAudit(), token);
            if (!RecordControlledHarmonyProcessorCreationGate(gateG)) return;

            _controlledHarmonyProcessorCreationResultLabel.Text = "CONTROLLED HARMONY PROCESSOR CREATION BOUNDARY: GATE H RUNNING…";
            _statusLabel.Text = "STEP 26 GATE H — replaying the closed inert Harmony(string) construction boundary.";
            var gateH = await Task.Run(() => _controlledHarmonyProcessorCreation.RunHarmonyInstanceConstruction(), token);
            if (!RecordControlledHarmonyProcessorCreationGate(gateH)) return;

            _controlledHarmonyProcessorCreationResultLabel.Text = "CONTROLLED HARMONY PROCESSOR CREATION BOUNDARY: GATE I RUNNING…";
            _statusLabel.Text = "STEP 26 GATE I — re-proving the closed Step 25 post-construction state.";
            var gateI = await _controlledHarmonyProcessorCreation.RunPostConstructionAuditAsync(progress, token);
            if (!RecordControlledHarmonyProcessorCreationGate(gateI)) return;

            _controlledHarmonyProcessorCreationResultLabel.Text = "CONTROLLED HARMONY PROCESSOR CREATION BOUNDARY: GATE J RUNNING…";
            _statusLabel.Text = "STEP 26 GATE J — metadata-auditing and resolving exact CreateProcessor/PatchProcessor API only. No PatchProcessor type initialization or construction.";
            var gateJ = await Task.Run(() => _controlledHarmonyProcessorCreation.RunHarmonyProcessorApiResolution(), token);
            if (!RecordControlledHarmonyProcessorCreationGate(gateJ)) return;

            _controlledHarmonyProcessorCreationResultLabel.Text = "CONTROLLED HARMONY PROCESSOR CREATION BOUNDARY: GATE K RUNNING…";
            _statusLabel.Text = "STEP 26 GATE K — explicitly completing only the measured PatchProcessor locker type initializer.";
            var gateK = await Task.Run(() => _controlledHarmonyProcessorCreation.RunPatchProcessorTypeInitialization(), token);
            if (!RecordControlledHarmonyProcessorCreationGate(gateK)) return;

            _controlledHarmonyProcessorCreationResultLabel.Text = "CONTROLLED HARMONY PROCESSOR CREATION BOUNDARY: GATE L RUNNING…";
            _statusLabel.Text = "STEP 26 GATE L — resolving one launcher-owned inert MethodInfo. No StS2 reflection and no method invocation.";
            var gateL = await Task.Run(() => _controlledHarmonyProcessorCreation.RunLauncherProbeResolution(), token);
            if (!RecordControlledHarmonyProcessorCreationGate(gateL)) return;

            _controlledHarmonyProcessorCreationResultLabel.Text = "CONTROLLED HARMONY PROCESSOR CREATION BOUNDARY: GATE M RUNNING…";
            _statusLabel.Text = "STEP 26 GATE M — invoking only Harmony.CreateProcessor(MethodBase). Patch() remains forbidden.";
            var gateM = await Task.Run(() => _controlledHarmonyProcessorCreation.RunHarmonyProcessorCreation(), token);
            if (!RecordControlledHarmonyProcessorCreationGate(gateM)) return;

            _controlledHarmonyProcessorCreationResultLabel.Text = "CONTROLLED HARMONY PROCESSOR CREATION BOUNDARY: GATE N RUNNING…";
            _statusLabel.Text = "STEP 26 GATE N — final byte/plan/context/native-isolation audit with Patch() still uninvoked.";
            var gateN = await _controlledHarmonyProcessorCreation.RunPostProcessorAuditAsync(progress, token);
            if (!RecordControlledHarmonyProcessorCreationGate(gateN)) return;

            var snapshot = _controlledHarmonyProcessorCreationGates.Snapshot();
            _controlledHarmonyProcessorCreationResultLabel.Text = snapshot.Summary;
            _controlledHarmonyProcessorCreationResultLabel.TextColor = UIColor.Label;
            _controlledHarmonyProcessorCreationDetailLabel.Text = FormatControlledHarmonyProcessorCreationDetail(
                "All fourteen Step 26 gates passed. The physically closed Step 25 state was reproduced, exact Harmony CreateProcessor/PatchProcessor metadata was verified, only the measured PatchProcessor locker type initializer completed, one launcher-owned inert MethodInfo was resolved, and one empty PatchProcessor retained that exact target without Patch(), game reflection/invocation, native resolution, or context drift. Run OfflineReady + Foundation 5/5 to close Step 26.");
            _statusLabel.Text = "PASS: STEP 26 CONTROLLED EMPTY HARMONY PATCHPROCESSOR CREATION — 14/14. Run OfflineReady + Foundation 5/5 for closure.";
            _statusLabel.TextColor = UIColor.Label;
        }
        catch (OperationCanceledException)
        {
            _controlledHarmonyProcessorCreationResultLabel.Text = "CONTROLLED HARMONY PROCESSOR CREATION BOUNDARY: CANCELLED";
            _controlledHarmonyProcessorCreationResultLabel.TextColor = UIColor.SecondaryLabel;
            _controlledHarmonyProcessorCreationDetailLabel.Text = FormatControlledHarmonyProcessorCreationDetail(
                "Step 26 was cancelled. If Gate B had started, force-quit before retrying so Gate A begins from a fresh process.");
            _statusLabel.Text = "STEP 26 CANCELLED — no later gate is considered proven.";
            _statusLabel.TextColor = UIColor.SecondaryLabel;
        }
        catch (Exception ex)
        {
            _controlledHarmonyProcessorCreationResultLabel.Text = "CONTROLLED HARMONY PROCESSOR CREATION BOUNDARY: EXCEPTION";
            _controlledHarmonyProcessorCreationResultLabel.TextColor = UIColor.SystemRed;
            _controlledHarmonyProcessorCreationDetailLabel.Text = FormatControlledHarmonyProcessorCreationDetail($"Unhandled Step 26 exception: {ex.GetType().Name}: {ex.Message}");
            _statusLabel.Text = "STEP 26 FAIL — stop at the first failing gate. Force-quit before another attempt if Gate B had started.";
            _statusLabel.TextColor = UIColor.SystemRed;
        }
        finally
        {
            await WriteDeviceTestReportFromLabelsAsync(
                "Step26-ControlledHarmonyProcessorCreation.txt",
                "StS2 Launcher — Step 26 Controlled Empty Harmony PatchProcessor Creation Boundary",
                _controlledHarmonyProcessorCreationResultLabel,
                _controlledHarmonyProcessorCreationDetailLabel,
                CancellationToken.None);
            EndSteamOperation();
        }
    }

    private bool RecordControlledHarmonyProcessorCreationGate(ControlledHarmonyProcessorCreationGateResult result)
    {
        _controlledHarmonyProcessorCreationGates.Record(result);
        if (_controlledHarmonyProcessorCreationResultLabel is not null)
        {
            _controlledHarmonyProcessorCreationResultLabel.Text = _controlledHarmonyProcessorCreationGates.Snapshot().Summary;
            _controlledHarmonyProcessorCreationResultLabel.TextColor = result.Passed ? UIColor.Label : UIColor.SystemRed;
        }
        if (_controlledHarmonyProcessorCreationDetailLabel is not null)
            _controlledHarmonyProcessorCreationDetailLabel.Text = FormatControlledHarmonyProcessorCreationDetail(result.Detail);
        if (!result.Passed && _statusLabel is not null)
        {
            var letter = (char)('A' + (int)result.Gate - 1);
            _statusLabel.Text = $"STEP 26 FAIL at Gate {letter} ({result.Gate}). Stop here; later gates were not run.";
            _statusLabel.TextColor = UIColor.SystemRed;
        }
        return result.Passed;
    }

    private string FormatControlledHarmonyProcessorCreationDetail(string tail)
    {
        var lines = new List<string>();
        foreach (var gate in _controlledHarmonyProcessorCreationGates.Results)
        {
            var letter = (char)('A' + (int)gate.Gate - 1);
            lines.Add($"Gate {letter} — {gate.Gate}: {(gate.Passed ? "PASS" : "FAIL")}");
            lines.Add(gate.Detail);
            lines.Add(string.Empty);
        }

        lines.Add("Step 26 prerequisite: physical Step 25.0.2 / 0.0.82 is closed — Gates A–I PASS, OfflineReady PASS, Foundation 5/5 PASS.");
        lines.Add("Gates A–I replay the closed Step 25 chain in the exact Step 26 private context, including the proven System.Collections.Concurrent root and Step-25 constructor framework-preservation anchor with TrimMode=full and MtouchInterpreter=-all.");
        lines.Add("Gate J resolves only exact Harmony.CreateProcessor(MethodBase), HarmonyLib.PatchProcessor, its measured locker .cctor, field-storage-only .ctor(Harmony,MethodBase), and exact private retained fields; it does not initialize or construct PatchProcessor.");
        lines.Add("Gate K explicitly completes only the exact Gate-J-measured PatchProcessor type initializer with RuntimeHelpers.RunClassConstructor and audits load/native/context stability.");
        lines.Add("Gate L resolves exactly one launcher-owned host MethodInfo, HarmonyProcessorProbe.Target(int), without invoking it and without reflecting any StS2 member.");
        lines.Add("Gate M invokes only Harmony.CreateProcessor(MethodBase) and verifies the returned empty PatchProcessor retains the exact proven Harmony object and launcher probe MethodBase. PatchProcessor.Patch is not invoked.");
        lines.Add("Gate N re-hashes plan/prepared/live bytes, re-proves OfflineReady, and verifies exact retained processor/context/native/resolver state.");
        lines.Add("Still forbidden: PatchProcessor.Patch; Harmony.Patch/PatchAll/PatchCategory or patch-class APIs; HarmonyMethod/prefix/postfix/transpiler/finalizer creation; StS2 entry-point/type/member reflection or invocation; broad Activator/CreateInstance; Godot/game startup; native game-library loading.");
        lines.Add("After Gate B, managed game/Harmony assemblies remain process-resident until force-quit. Do not rerun Step 21/22/23/24/25 fresh-process regressions in the same process.");
        lines.Add(tail);
        return string.Join("\n", lines);
    }
}
