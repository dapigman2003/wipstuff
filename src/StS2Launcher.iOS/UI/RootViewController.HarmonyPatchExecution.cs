using StS2Launcher.Core;
using StS2Launcher.iOS.Platform;
using UIKit;

namespace StS2Launcher.iOS;

public sealed partial class RootViewController
{
    private readonly ControlledHarmonyPatchExecution _controlledHarmonyPatchExecution;
    private readonly ControlledHarmonyPatchExecutionGateSequence _controlledHarmonyPatchExecutionGates = new();
    private UILabel? _controlledHarmonyPatchExecutionResultLabel;
    private UILabel? _controlledHarmonyPatchExecutionDetailLabel;
    private UIButton? _controlledHarmonyPatchExecutionButton;

    private void AddControlledHarmonyPatchExecutionControls(UIStackView content)
    {
        // Step 26.0 / 0.0.83 is physically closed. Keep the physically proven Step-25
        // constructor-preservation anchor rooted while Step 27 advances beyond inert PatchProcessor creation.
        Step25HarmonyConstructorFrameworkPreservation.Activate();

        content.AddArrangedSubview(Separator());
        content.AddArrangedSubview(Label(
            "Step 27 — Controlled Launcher-Owned Harmony Patch + Unpatch (ordered gates A–Y)",
            UIFont.BoldSystemFontOfSize(25),
            UIColor.Label));

        _controlledHarmonyPatchExecutionButton = SystemButton(
            "Run Step 27 A–Y — Replay Step 26 → Resolve Patch APIs → Register Launcher Prefix → Patch → Audit → Invoke → Unpatch → Restore → Final Audit",
            17);
        _controlledHarmonyPatchExecutionButton.TouchUpInside += async (_, _) => await RunControlledHarmonyPatchExecutionAsync();
        content.AddArrangedSubview(_controlledHarmonyPatchExecutionButton);

        _controlledHarmonyPatchExecutionResultLabel = Label(
            "CONTROLLED LAUNCHER-OWNED HARMONY PATCH EXECUTION BOUNDARY: NOT RUN",
            UIFont.BoldSystemFontOfSize(21),
            UIColor.Label);
        content.AddArrangedSubview(_controlledHarmonyPatchExecutionResultLabel);

        _controlledHarmonyPatchExecutionDetailLabel = Label(
            "Gates A–N replay the physically closed Step 26 boundary in one fresh dedicated context through one empty PatchProcessor targeting the Step-26 launcher probe. Gate O metadata-audits and resolves only exact AddPrefix(MethodInfo), Patch(), Unpatch(MethodInfo), HarmonyMethod(MethodInfo), and the processor prefix field without constructing a patch descriptor. Gate P resolves a separate launcher-owned Step-27 Target(int) + Prefix(int, ref __result) pair and never reflects StS2. Gate Q proves original target behavior before patching. Gate R registers only that prefix descriptor without replacement. Gate S is the first real Harmony patch-engine boundary: exactly one PatchProcessor.Patch() call against the launcher target, with no target invocation yet. Gate T audits bytes/OfflineReady/context before patched execution. Gate U requires both reflection and direct calls to return the prefix-controlled result while the original body is skipped. Gate V removes exactly that prefix by MethodInfo; Gate W audits before restored invocation; Gate X requires original behavior on both routes; Gate Y performs the final full hash/OfflineReady/context audit. StS2 reflection/patching/invocation, Harmony.Patch/PatchAll, patch classes/categories, transpilers/finalizers, Godot startup, and native game loading remain forbidden.",
            UIFont.SystemFontOfSize(15),
            UIColor.SecondaryLabel);
        content.AddArrangedSubview(_controlledHarmonyPatchExecutionDetailLabel);
    }

    private async Task RunControlledHarmonyPatchExecutionAsync()
    {
        if (_controlledHarmonyPatchExecutionResultLabel is null ||
            _controlledHarmonyPatchExecutionDetailLabel is null ||
            _controlledHarmonyPatchExecutionButton is null ||
            _statusLabel is null)
            return;

        if (_godotProcessRequiresRestart || _godotSessionStarted)
        {
            _statusLabel.Text = "Step 27 requires a fresh process. Force-quit/relaunch if the Step 15 Godot host has been started.";
            _statusLabel.TextColor = UIColor.SystemOrange;
            return;
        }

        BeginSteamOperation(allowCancel: true);
        _controlledHarmonyPatchExecutionGates.Reset();
        _controlledHarmonyPatchExecution.Reset();
        _controlledHarmonyPatchExecutionResultLabel.Text = "CONTROLLED LAUNCHER-OWNED HARMONY PATCH EXECUTION BOUNDARY: GATE A RUNNING…";
        _controlledHarmonyPatchExecutionResultLabel.TextColor = UIColor.Label;
        _controlledHarmonyPatchExecutionDetailLabel.Text = "Gate A: replaying the closed Step 26 metadata preconditions before any Step 27 CLR load.";
        _statusLabel.Text = "STEP 27 GATE A — metadata-only preflight. No Step 27 game/Harmony assembly is loaded yet.";
        _statusLabel.TextColor = UIColor.Label;

        try
        {
            var token = _operationCts?.Token ?? CancellationToken.None;
            var progress = new Progress<ControlledHarmonyPatchExecutionProgress>(value =>
            {
                var count = value.TotalItems > 0 ? $" ({value.ProcessedItems:N0}/{value.TotalItems:N0})" : string.Empty;
                _controlledHarmonyPatchExecutionDetailLabel.Text = FormatControlledHarmonyPatchExecutionDetail(
                    $"Gate {(char)('A' + (int)value.Gate - 1)} progress{count}: {value.Detail}" +
                    (string.IsNullOrWhiteSpace(value.CurrentPath) ? string.Empty : $"\nCurrent: {value.CurrentPath}"));
            });

            var gateA = await _controlledHarmonyPatchExecution.RunInitializationPreflightAsync(progress, token);
            if (!RecordControlledHarmonyPatchExecutionGate(gateA)) return;

            _controlledHarmonyPatchExecutionResultLabel.Text = "CONTROLLED LAUNCHER-OWNED HARMONY PATCH EXECUTION BOUNDARY: GATE B RUNNING…";
            _statusLabel.Text = "STEP 27 GATE B — replaying the Step 23 initializer-free private state.";
            var gateB = await Task.Run(() => _controlledHarmonyPatchExecution.RunProvenLoadStateReplay(), token);
            if (!RecordControlledHarmonyPatchExecutionGate(gateB)) return;

            _controlledHarmonyPatchExecutionResultLabel.Text = "CONTROLLED LAUNCHER-OWNED HARMONY PATCH EXECUTION BOUNDARY: GATE C RUNNING…";
            _statusLabel.Text = "STEP 27 GATE C — replaying the closed Step 24 0Harmony module initializer.";
            var gateC = await Task.Run(() => _controlledHarmonyPatchExecution.RunDeferredModuleInitialization(), token);
            if (!RecordControlledHarmonyPatchExecutionGate(gateC)) return;

            _controlledHarmonyPatchExecutionResultLabel.Text = "CONTROLLED LAUNCHER-OWNED HARMONY PATCH EXECUTION BOUNDARY: GATE D RUNNING…";
            _statusLabel.Text = "STEP 27 GATE D — re-proving the closed Step 24 post-initialization state.";
            var gateD = await _controlledHarmonyPatchExecution.RunProvenInitializationAuditAsync(progress, token);
            if (!RecordControlledHarmonyPatchExecutionGate(gateD)) return;

            _controlledHarmonyPatchExecutionResultLabel.Text = "CONTROLLED LAUNCHER-OWNED HARMONY PATCH EXECUTION BOUNDARY: GATE E RUNNING…";
            _statusLabel.Text = "STEP 27 GATE E — replaying exact Harmony API resolution without type initialization.";
            var gateE = await Task.Run(() => _controlledHarmonyPatchExecution.RunHarmonyApiResolution(), token);
            if (!RecordControlledHarmonyPatchExecutionGate(gateE)) return;

            _controlledHarmonyPatchExecutionResultLabel.Text = "CONTROLLED LAUNCHER-OWNED HARMONY PATCH EXECUTION BOUNDARY: GATE F RUNNING…";
            _statusLabel.Text = "STEP 27 GATE F — replaying exact Harmony type initialization.";
            var gateF = await Task.Run(() => _controlledHarmonyPatchExecution.RunHarmonyTypeInitialization(), token);
            if (!RecordControlledHarmonyPatchExecutionGate(gateF)) return;

            _controlledHarmonyPatchExecutionResultLabel.Text = "CONTROLLED LAUNCHER-OWNED HARMONY PATCH EXECUTION BOUNDARY: GATE G RUNNING…";
            _statusLabel.Text = "STEP 27 GATE G — re-auditing Harmony type-initialization state.";
            var gateG = await Task.Run(() => _controlledHarmonyPatchExecution.RunHarmonyTypeInitializationAudit(), token);
            if (!RecordControlledHarmonyPatchExecutionGate(gateG)) return;

            _controlledHarmonyPatchExecutionResultLabel.Text = "CONTROLLED LAUNCHER-OWNED HARMONY PATCH EXECUTION BOUNDARY: GATE H RUNNING…";
            _statusLabel.Text = "STEP 27 GATE H — replaying the closed inert Harmony(string) construction boundary.";
            var gateH = await Task.Run(() => _controlledHarmonyPatchExecution.RunHarmonyInstanceConstruction(), token);
            if (!RecordControlledHarmonyPatchExecutionGate(gateH)) return;

            _controlledHarmonyPatchExecutionResultLabel.Text = "CONTROLLED LAUNCHER-OWNED HARMONY PATCH EXECUTION BOUNDARY: GATE I RUNNING…";
            _statusLabel.Text = "STEP 27 GATE I — re-proving the closed Step 25 post-construction state.";
            var gateI = await _controlledHarmonyPatchExecution.RunPostConstructionAuditAsync(progress, token);
            if (!RecordControlledHarmonyPatchExecutionGate(gateI)) return;

            _controlledHarmonyPatchExecutionResultLabel.Text = "CONTROLLED LAUNCHER-OWNED HARMONY PATCH EXECUTION BOUNDARY: GATE J RUNNING…";
            _statusLabel.Text = "STEP 27 GATE J — metadata-auditing and resolving exact CreateProcessor/PatchProcessor API only. No PatchProcessor type initialization or construction.";
            var gateJ = await Task.Run(() => _controlledHarmonyPatchExecution.RunHarmonyProcessorApiResolution(), token);
            if (!RecordControlledHarmonyPatchExecutionGate(gateJ)) return;

            _controlledHarmonyPatchExecutionResultLabel.Text = "CONTROLLED LAUNCHER-OWNED HARMONY PATCH EXECUTION BOUNDARY: GATE K RUNNING…";
            _statusLabel.Text = "STEP 27 GATE K — explicitly completing only the measured PatchProcessor locker type initializer.";
            var gateK = await Task.Run(() => _controlledHarmonyPatchExecution.RunPatchProcessorTypeInitialization(), token);
            if (!RecordControlledHarmonyPatchExecutionGate(gateK)) return;

            _controlledHarmonyPatchExecutionResultLabel.Text = "CONTROLLED LAUNCHER-OWNED HARMONY PATCH EXECUTION BOUNDARY: GATE L RUNNING…";
            _statusLabel.Text = "STEP 27 GATE L — resolving one launcher-owned inert MethodInfo. No StS2 reflection and no method invocation.";
            var gateL = await Task.Run(() => _controlledHarmonyPatchExecution.RunLauncherProbeResolution(), token);
            if (!RecordControlledHarmonyPatchExecutionGate(gateL)) return;

            _controlledHarmonyPatchExecutionResultLabel.Text = "CONTROLLED LAUNCHER-OWNED HARMONY PATCH EXECUTION BOUNDARY: GATE M RUNNING…";
            _statusLabel.Text = "STEP 27 GATE M — invoking only Harmony.CreateProcessor(MethodBase). Patch() remains forbidden.";
            var gateM = await Task.Run(() => _controlledHarmonyPatchExecution.RunHarmonyProcessorCreation(), token);
            if (!RecordControlledHarmonyPatchExecutionGate(gateM)) return;

            _controlledHarmonyPatchExecutionResultLabel.Text = "CONTROLLED LAUNCHER-OWNED HARMONY PATCH EXECUTION BOUNDARY: GATE N RUNNING…";
            _statusLabel.Text = "STEP 27 GATE N — final byte/plan/context/native-isolation audit with Patch() still uninvoked.";
            var gateN = await _controlledHarmonyPatchExecution.RunPostProcessorAuditAsync(progress, token);
            if (!RecordControlledHarmonyPatchExecutionGate(gateN)) return;

            _controlledHarmonyPatchExecutionResultLabel.Text = "CONTROLLED LAUNCHER-OWNED HARMONY PATCH EXECUTION BOUNDARY: GATE O RUNNING…";
            _statusLabel.Text = "STEP 27 GATE O — metadata-auditing and resolving exact AddPrefix/Patch/Unpatch/HarmonyMethod surfaces. No patch descriptor construction or patching.";
            var gateO = await Task.Run(() => _controlledHarmonyPatchExecution.RunHarmonyPatchApiResolution(), token);
            if (!RecordControlledHarmonyPatchExecutionGate(gateO)) return;

            _controlledHarmonyPatchExecutionResultLabel.Text = "CONTROLLED LAUNCHER-OWNED HARMONY PATCH EXECUTION BOUNDARY: GATE P RUNNING…";
            _statusLabel.Text = "STEP 27 GATE P — resolving launcher-owned Target(int) + Prefix(int, ref __result). No StS2 reflection and no invocation.";
            var gateP = await Task.Run(() => _controlledHarmonyPatchExecution.RunLauncherPatchProbeResolution(), token);
            if (!RecordControlledHarmonyPatchExecutionGate(gateP)) return;

            _controlledHarmonyPatchExecutionResultLabel.Text = "CONTROLLED LAUNCHER-OWNED HARMONY PATCH EXECUTION BOUNDARY: GATE Q RUNNING…";
            _statusLabel.Text = "STEP 27 GATE Q — invoking only the launcher-owned target before patching to establish direct + reflection baseline behavior.";
            var gateQ = await Task.Run(() => _controlledHarmonyPatchExecution.RunBaselineProbeInvocation(), token);
            if (!RecordControlledHarmonyPatchExecutionGate(gateQ)) return;

            _controlledHarmonyPatchExecutionResultLabel.Text = "CONTROLLED LAUNCHER-OWNED HARMONY PATCH EXECUTION BOUNDARY: GATE R RUNNING…";
            _statusLabel.Text = "STEP 27 GATE R — registering exactly one launcher prefix with AddPrefix(MethodInfo). Patch() is still uninvoked.";
            var gateR = await Task.Run(() => _controlledHarmonyPatchExecution.RunPrefixRegistration(), token);
            if (!RecordControlledHarmonyPatchExecutionGate(gateR)) return;

            _controlledHarmonyPatchExecutionResultLabel.Text = "CONTROLLED LAUNCHER-OWNED HARMONY PATCH EXECUTION BOUNDARY: GATE S RUNNING…";
            _statusLabel.Text = "STEP 27 GATE S — FIRST REAL PATCH ENGINE BOUNDARY: invoking PatchProcessor.Patch() exactly once against launcher-owned target. Target is not invoked yet.";
            var gateS = await Task.Run(() => _controlledHarmonyPatchExecution.RunPatchEngineExecution(), token);
            if (!RecordControlledHarmonyPatchExecutionGate(gateS)) return;

            _controlledHarmonyPatchExecutionResultLabel.Text = "CONTROLLED LAUNCHER-OWNED HARMONY PATCH EXECUTION BOUNDARY: GATE T RUNNING…";
            _statusLabel.Text = "STEP 27 GATE T — post-patch hashes/OfflineReady/context audit BEFORE patched target execution.";
            var gateT = await _controlledHarmonyPatchExecution.RunPostPatchAuditAsync(progress, token);
            if (!RecordControlledHarmonyPatchExecutionGate(gateT)) return;

            _controlledHarmonyPatchExecutionResultLabel.Text = "CONTROLLED LAUNCHER-OWNED HARMONY PATCH EXECUTION BOUNDARY: GATE U RUNNING…";
            _statusLabel.Text = "STEP 27 GATE U — invoking patched launcher target through reflection and direct call; exact prefix must replace result and skip original.";
            var gateU = await Task.Run(() => _controlledHarmonyPatchExecution.RunPatchedProbeInvocation(), token);
            if (!RecordControlledHarmonyPatchExecutionGate(gateU)) return;

            _controlledHarmonyPatchExecutionResultLabel.Text = "CONTROLLED LAUNCHER-OWNED HARMONY PATCH EXECUTION BOUNDARY: GATE V RUNNING…";
            _statusLabel.Text = "STEP 27 GATE V — removing exactly the launcher-owned prefix via PatchProcessor.Unpatch(MethodInfo).";
            var gateV = await Task.Run(() => _controlledHarmonyPatchExecution.RunExactPrefixUnpatch(), token);
            if (!RecordControlledHarmonyPatchExecutionGate(gateV)) return;

            _controlledHarmonyPatchExecutionResultLabel.Text = "CONTROLLED LAUNCHER-OWNED HARMONY PATCH EXECUTION BOUNDARY: GATE W RUNNING…";
            _statusLabel.Text = "STEP 27 GATE W — auditing post-unpatch context/native/hash state before restored invocation.";
            var gateW = await Task.Run(() => _controlledHarmonyPatchExecution.RunPostUnpatchAudit(), token);
            if (!RecordControlledHarmonyPatchExecutionGate(gateW)) return;

            _controlledHarmonyPatchExecutionResultLabel.Text = "CONTROLLED LAUNCHER-OWNED HARMONY PATCH EXECUTION BOUNDARY: GATE X RUNNING…";
            _statusLabel.Text = "STEP 27 GATE X — invoking launcher target through reflection and direct call; exact original value+1 behavior must be restored.";
            var gateX = await Task.Run(() => _controlledHarmonyPatchExecution.RunRestoredProbeInvocation(), token);
            if (!RecordControlledHarmonyPatchExecutionGate(gateX)) return;

            _controlledHarmonyPatchExecutionResultLabel.Text = "CONTROLLED LAUNCHER-OWNED HARMONY PATCH EXECUTION BOUNDARY: GATE Y RUNNING…";
            _statusLabel.Text = "STEP 27 GATE Y — final plan/prepared/live/OfflineReady/context/native-isolation audit after complete launcher-only patch/unpatch cycle.";
            var gateY = await _controlledHarmonyPatchExecution.RunFinalIsolationAuditAsync(progress, token);
            if (!RecordControlledHarmonyPatchExecutionGate(gateY)) return;

            var snapshot = _controlledHarmonyPatchExecutionGates.Snapshot();
            _controlledHarmonyPatchExecutionResultLabel.Text = snapshot.Summary;
            _controlledHarmonyPatchExecutionResultLabel.TextColor = UIColor.Label;
            _controlledHarmonyPatchExecutionDetailLabel.Text = FormatControlledHarmonyPatchExecutionDetail(
                "All twenty-five Step 27 gates passed. The physically closed Step 26 state was reproduced; exact patch APIs and a launcher-owned prefix were admitted; one real PatchProcessor.Patch() completed; the launcher target returned the deterministic patched result through reflection and direct calls while the original body was skipped; the exact prefix was then removed and original behavior was restored through both routes; final hashes, OfflineReady, context membership, and native/resolver isolation remained intact. Run OfflineReady + Foundation 5/5 to close Step 27.");
            _statusLabel.Text = "PASS: STEP 27 CONTROLLED LAUNCHER-OWNED HARMONY PATCH EXECUTION — 25/25. Run OfflineReady + Foundation 5/5 for closure.";
            _statusLabel.TextColor = UIColor.Label;
        }
        catch (OperationCanceledException)
        {
            _controlledHarmonyPatchExecutionResultLabel.Text = "CONTROLLED LAUNCHER-OWNED HARMONY PATCH EXECUTION BOUNDARY: CANCELLED";
            _controlledHarmonyPatchExecutionResultLabel.TextColor = UIColor.SecondaryLabel;
            _controlledHarmonyPatchExecutionDetailLabel.Text = FormatControlledHarmonyPatchExecutionDetail(
                "Step 27 was cancelled. If Gate B had started, force-quit before retrying so Gate A begins from a fresh process.");
            _statusLabel.Text = "STEP 27 CANCELLED — no later gate is considered proven.";
            _statusLabel.TextColor = UIColor.SecondaryLabel;
        }
        catch (Exception ex)
        {
            _controlledHarmonyPatchExecutionResultLabel.Text = "CONTROLLED LAUNCHER-OWNED HARMONY PATCH EXECUTION BOUNDARY: EXCEPTION";
            _controlledHarmonyPatchExecutionResultLabel.TextColor = UIColor.SystemRed;
            _controlledHarmonyPatchExecutionDetailLabel.Text = FormatControlledHarmonyPatchExecutionDetail($"Unhandled Step 27 exception: {ex.GetType().Name}: {ex.Message}");
            _statusLabel.Text = "STEP 27 FAIL — stop at the first failing gate. Force-quit before another attempt if Gate B had started.";
            _statusLabel.TextColor = UIColor.SystemRed;
        }
        finally
        {
            await WriteDeviceTestReportFromLabelsAsync(
                "Step27-ControlledHarmonyPatchExecution.txt",
                "StS2 Launcher — Step 27 Controlled Launcher-Owned Harmony Patch + Unpatch Boundary",
                _controlledHarmonyPatchExecutionResultLabel,
                _controlledHarmonyPatchExecutionDetailLabel,
                CancellationToken.None);
            EndSteamOperation();
        }
    }

    private bool RecordControlledHarmonyPatchExecutionGate(ControlledHarmonyPatchExecutionGateResult result)
    {
        _controlledHarmonyPatchExecutionGates.Record(result);
        if (_controlledHarmonyPatchExecutionResultLabel is not null)
        {
            _controlledHarmonyPatchExecutionResultLabel.Text = _controlledHarmonyPatchExecutionGates.Snapshot().Summary;
            _controlledHarmonyPatchExecutionResultLabel.TextColor = result.Passed ? UIColor.Label : UIColor.SystemRed;
        }
        if (_controlledHarmonyPatchExecutionDetailLabel is not null)
            _controlledHarmonyPatchExecutionDetailLabel.Text = FormatControlledHarmonyPatchExecutionDetail(result.Detail);
        if (!result.Passed && _statusLabel is not null)
        {
            var letter = (char)('A' + (int)result.Gate - 1);
            _statusLabel.Text = $"STEP 27 FAIL at Gate {letter} ({result.Gate}). Stop here; later gates were not run.";
            _statusLabel.TextColor = UIColor.SystemRed;
        }
        return result.Passed;
    }

    private string FormatControlledHarmonyPatchExecutionDetail(string tail)
    {
        var lines = new List<string>();
        foreach (var gate in _controlledHarmonyPatchExecutionGates.Results)
        {
            var letter = (char)('A' + (int)gate.Gate - 1);
            lines.Add($"Gate {letter} — {gate.Gate}: {(gate.Passed ? "PASS" : "FAIL")}");
            lines.Add(gate.Detail);
            lines.Add(string.Empty);
        }

        lines.Add("Step 27 prerequisite: physical Step 26.0 / 0.0.83 is closed — Gates A–N PASS, OfflineReady PASS, Foundation 5/5 PASS.");
        lines.Add("Gates A–N replay the closed Step 26 chain in the exact Step 27 private context, preserving the proven System.Collections.Concurrent root and Step-25 constructor framework-preservation anchor with TrimMode=full and MtouchInterpreter=-all.");
        lines.Add("Gate O metadata-audits and resolves only exact PatchProcessor.AddPrefix(MethodInfo), Patch(), Unpatch(MethodInfo), HarmonyMethod(MethodInfo), PatchProcessor.prefix, and HarmonyMethod.method. It does not construct a patch descriptor or patch anything.");
        lines.Add("Gate P resolves only launcher-owned HarmonyPatchProbe.Target(int) + Prefix(int, ref int __result), including exact parameter names required by Harmony. No StS2 member is reflected.");
        lines.Add("Gate Q invokes the launcher target through direct + reflection routes before patching and requires original value+1 behavior with prefix count zero.");
        lines.Add("Gate R invokes only AddPrefix(MethodInfo), verifies the constructed HarmonyMethod retains the exact launcher prefix, and still forbids Patch().");
        lines.Add("Gate S is the first real patch-engine boundary: exactly one PatchProcessor.Patch() against the launcher target. The patched target is not invoked until Gate U.");
        lines.Add("Gate T re-hashes plan/prepared/live bytes, re-proves OfflineReady, and audits context/native/resolver state before patched execution.");
        lines.Add("Gate U invokes the patched launcher target through reflection and direct calls; both must return 1041, increment the prefix, and skip the original target body.");
        lines.Add("Gate V removes exactly the launcher prefix via PatchProcessor.Unpatch(MethodInfo). Gate W audits before restored invocation.");
        lines.Add("Gate X invokes the launcher target through both routes and requires restored value+1 behavior with no additional prefix calls. Gate Y performs the final full byte/OfflineReady/context/native audit.");
        lines.Add("Still forbidden: Harmony.Patch/PatchAll/PatchCategory/PatchClassProcessor; postfix/transpiler/finalizer/inner patch registration; StS2 entry-point/type/member reflection, patching, or invocation; broad Activator/CreateInstance; Godot/game startup; native game-library loading; mutation of trusted live/prepared bytes.");
        lines.Add("If Gate S or any later gate runs, assume launcher probe patch state may remain process-resident until force-quit. Do not retry Step 27 or run earlier fresh-process regressions in the same process after a failure at or beyond Gate S.");
        lines.Add(tail);
        return string.Join("\n", lines);
    }
}
