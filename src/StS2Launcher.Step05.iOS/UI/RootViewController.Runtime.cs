using Foundation;
using StS2Launcher.Core;
using StS2Launcher.Step05.iOS.Platform;
using UIKit;

namespace StS2Launcher.Step05.iOS;

public sealed partial class RootViewController
{
    private async Task RunDynamicManagedExecutionFoundationAsync()
    {
        if (_dynamicManagedExecutionResultLabel is null ||
            _dynamicManagedExecutionDetailLabel is null ||
            _dynamicManagedExecutionButton is null ||
            _statusLabel is null)
        {
            return;
        }

        if (_godotProcessRequiresRestart)
        {
            _statusLabel.Text = "Step 15 Godot process-global state is still active. Force-quit/relaunch before Step 20 so the external managed-execution proof runs in a clean host process.";
            _statusLabel.TextColor = UIColor.SystemOrange;
            return;
        }

        BeginSteamOperation(allowCancel: true);
        _dynamicManagedExecutionGates.Reset();
        _dynamicManagedExecutionFoundation.Reset();
        _dynamicManagedExecutionResultLabel.Text = "DYNAMIC MANAGED EXECUTION FOUNDATION: GATE A RUNNING…";
        _dynamicManagedExecutionResultLabel.TextColor = UIColor.Label;
        _dynamicManagedExecutionDetailLabel.Text = "Gate A: re-proving OfflineReady and validating/copying the exact-hash project-owned external managed fixtures without loading them.";
        _statusLabel.Text = "STEP 20 GATE A — fixture integrity + OfflineReady.";
        _statusLabel.TextColor = UIColor.Label;

        try
        {
            var token = _operationCts?.Token ?? CancellationToken.None;
            var progress = new Progress<DynamicManagedExecutionProgress>(value =>
            {
                var count = value.TotalItems > 0 ? $" ({value.ProcessedItems:N0}/{value.TotalItems:N0})" : string.Empty;
                _dynamicManagedExecutionDetailLabel.Text = FormatDynamicManagedExecutionDetail(
                    $"Gate {(char)('A' + (int)value.Gate - 1)} progress{count}: {value.Detail}" +
                    (string.IsNullOrWhiteSpace(value.CurrentPath) ? string.Empty : $"\nCurrent: {value.CurrentPath}"));
            });

            var gateA = await _dynamicManagedExecutionFoundation.RunFixtureIntegrityAndOfflineReadyAsync(progress, token);
            if (!RecordDynamicManagedExecutionGate(gateA)) return;

            _dynamicManagedExecutionResultLabel.Text = "DYNAMIC MANAGED EXECUTION FOUNDATION: GATE B RUNNING…";
            _statusLabel.Text = "STEP 20 GATE B — load and execute non-AOT project-owned IL from verified bytes.";
            var gateB = await Task.Run(() => _dynamicManagedExecutionFoundation.RunDynamicFixtureExecution(), token);
            if (!RecordDynamicManagedExecutionGate(gateB)) return;

            _dynamicManagedExecutionResultLabel.Text = "DYNAMIC MANAGED EXECUTION FOUNDATION: GATE C RUNNING…";
            _statusLabel.Text = "STEP 20 GATE C — exact private managed dependency resolution + transitive execution.";
            var gateC = await Task.Run(() => _dynamicManagedExecutionFoundation.RunPrivateDependencyResolution(), token);
            if (!RecordDynamicManagedExecutionGate(gateC)) return;

            _dynamicManagedExecutionResultLabel.Text = "DYNAMIC MANAGED EXECUTION FOUNDATION: GATE D RUNNING…";
            _statusLabel.Text = "STEP 20 GATE D — fixture + managed-install isolation audit.";
            var gateD = await _dynamicManagedExecutionFoundation.RunIsolationAuditAsync(progress, token);
            if (!RecordDynamicManagedExecutionGate(gateD)) return;

            var snapshot = _dynamicManagedExecutionGates.Snapshot();
            _dynamicManagedExecutionResultLabel.Text = snapshot.Summary;
            _dynamicManagedExecutionResultLabel.TextColor = UIColor.Label;
            _dynamicManagedExecutionDetailLabel.Text = FormatDynamicManagedExecutionDetail(
                "All four Step 20 gates passed. A managed DLL that was not linked/AOT-compiled into the IPA executed from verified bytes, a second runtime-loaded fixture resolved and executed one verified private dependency, and the receipt-backed StS2 install stayed untouched. Run OfflineReady + Foundation 5/5 to close Step 20.");
            _statusLabel.Text = "PASS: STEP 20 DYNAMIC MANAGED EXECUTION FOUNDATION — 4/4. External IL execution + private dependency resolution are physically proven; no StS2 assembly was loaded.";
            _statusLabel.TextColor = UIColor.Label;
        }
        catch (OperationCanceledException)
        {
            _dynamicManagedExecutionResultLabel.Text = "DYNAMIC MANAGED EXECUTION FOUNDATION: CANCELLED";
            _dynamicManagedExecutionResultLabel.TextColor = UIColor.SecondaryLabel;
            _dynamicManagedExecutionDetailLabel.Text = FormatDynamicManagedExecutionDetail("Step 20 was cancelled. Rerunning Gate A recreates only the launcher-private fixture workspace; the managed game install is never an intended write target.");
            _statusLabel.Text = "STEP 20 CANCELLED — no later gate is considered proven.";
            _statusLabel.TextColor = UIColor.SecondaryLabel;
        }
        catch (Exception ex)
        {
            _dynamicManagedExecutionResultLabel.Text = "DYNAMIC MANAGED EXECUTION FOUNDATION: EXCEPTION";
            _dynamicManagedExecutionResultLabel.TextColor = UIColor.SystemRed;
            _dynamicManagedExecutionDetailLabel.Text = FormatDynamicManagedExecutionDetail($"Unhandled Step 20 exception: {ex.GetType().Name}: {ex.Message}");
            _statusLabel.Text = "STEP 20 FAIL: stop at the current dynamic-managed-execution gate and report this screen.";
            _statusLabel.TextColor = UIColor.SystemRed;
        }
        finally
        {
            await WriteDeviceTestReportFromLabelsAsync(
                "Step20-DynamicManagedExecution.txt",
                "StS2 Launcher — Step 20 Dynamic Managed Execution Foundation",
                _dynamicManagedExecutionResultLabel,
                _dynamicManagedExecutionDetailLabel,
                CancellationToken.None);
            EndSteamOperation();
        }
    }

    private async Task RunHostFrameworkClosureFoundationAsync()
    {
        if (_runtimeFrameworkBindingResultLabel is null ||
            _runtimeFrameworkBindingDetailLabel is null ||
            _runtimeFrameworkBindingButton is null ||
            _statusLabel is null)
        {
            return;
        }

        if (_godotProcessRequiresRestart)
        {
            _statusLabel.Text = "Step 15 Godot process-global state is still active. Force-quit/relaunch before Step 22 so host-framework availability is measured in a clean process.";
            _statusLabel.TextColor = UIColor.SystemOrange;
            return;
        }

        BeginSteamOperation(allowCancel: true);
        _hostFrameworkClosureGates.Reset();
        _hostFrameworkClosureFoundation.Reset();
        _runtimeFrameworkBindingResultLabel.Text = "HOST FRAMEWORK CLOSURE FOUNDATION: GATE A RUNNING…";
        _runtimeFrameworkBindingResultLabel.TextColor = UIColor.Label;
        _runtimeFrameworkBindingDetailLabel.Text = "Gate A: probing all 44 Step 21.1 framework identities from the rooted iOS/.NET host and writing the complete success/failure frontier to Files. No StS2 assembly is loaded.";
        _statusLabel.Text = "STEP 22 GATE A — rooted host framework availability.";
        _statusLabel.TextColor = UIColor.Label;

        try
        {
            var token = _operationCts?.Token ?? CancellationToken.None;
            var progress = new Progress<RuntimeFrameworkBindingProgress>(value =>
            {
                var count = value.TotalItems > 0 ? $" ({value.ProcessedItems:N0}/{value.TotalItems:N0})" : string.Empty;
                _runtimeFrameworkBindingDetailLabel.Text = FormatHostFrameworkClosureDetail(
                    $"Nested Step 21 progress{count}: {value.Detail}" +
                    (string.IsNullOrWhiteSpace(value.CurrentPath) ? string.Empty : $"\nCurrent: {value.CurrentPath}"));
            });

            var gateA = await Task.Run(() => _hostFrameworkClosureFoundation.RunRootedHostAvailabilityProbe(), token);
            if (!RecordHostFrameworkClosureGate(gateA)) return;

            _runtimeFrameworkBindingResultLabel.Text = "HOST FRAMEWORK CLOSURE FOUNDATION: GATE B RUNNING…";
            _statusLabel.Text = "STEP 22 GATE B — recompute real sts2.dll host/private dependency closure.";
            var gateB = await _hostFrameworkClosureFoundation.RunBindingClosureRecomputeAsync(progress, token);
            if (!RecordHostFrameworkClosureGate(gateB)) return;

            _runtimeFrameworkBindingResultLabel.Text = "HOST FRAMEWORK CLOSURE FOUNDATION: GATE C RUNNING…";
            _statusLabel.Text = "STEP 22 GATE C — persist the recomputed plan and qualify zero-blocker host-only framework closure.";
            var gateC = await _hostFrameworkClosureFoundation.RunHostOnlyFrameworkPreparedSetAsync(progress, token);
            if (!RecordHostFrameworkClosureGate(gateC)) return;

            _runtimeFrameworkBindingResultLabel.Text = "HOST FRAMEWORK CLOSURE FOUNDATION: GATE D RUNNING…";
            _statusLabel.Text = "STEP 22 GATE D — independent source/prepared/live/plan isolation audit.";
            var gateD = await _hostFrameworkClosureFoundation.RunIsolationAuditAsync(progress, token);
            if (!RecordHostFrameworkClosureGate(gateD)) return;

            await TryExportRuntimeBindingDiagnosticsAsync(automatic: true, token);

            var snapshot = _hostFrameworkClosureGates.Snapshot();
            _runtimeFrameworkBindingResultLabel.Text = snapshot.Summary;
            _runtimeFrameworkBindingResultLabel.TextColor = UIColor.Label;
            _runtimeFrameworkBindingDetailLabel.Text = FormatHostFrameworkClosureDetail(
                "All four Step 22 gates passed. The complete measured framework frontier is supplied by the iOS/.NET host, the real Step 21 dependency graph has zero explicit blockers, no desktop System.*/netstandard assembly is in the private prepared set, and the audited plan reports Runtime closure ready=YES. StS2 still has not been CLR-loaded or executed.");
            _statusLabel.Text = "PASS: STEP 22 HOST FRAMEWORK CLOSURE FOUNDATION — 4/4. Runtime dependency closure is now eligible for a later first real CLR-load probe.";
            _statusLabel.TextColor = UIColor.Label;
        }
        catch (OperationCanceledException)
        {
            _runtimeFrameworkBindingResultLabel.Text = "HOST FRAMEWORK CLOSURE FOUNDATION: CANCELLED";
            _runtimeFrameworkBindingResultLabel.TextColor = UIColor.SecondaryLabel;
            _runtimeFrameworkBindingDetailLabel.Text = FormatHostFrameworkClosureDetail("Step 22 was cancelled. No StS2 CLR load is part of this subsystem and the trusted managed install remains read-only.");
            _statusLabel.Text = "STEP 22 CANCELLED — no later gate is considered proven.";
            _statusLabel.TextColor = UIColor.SecondaryLabel;
        }
        catch (Exception ex)
        {
            _runtimeFrameworkBindingResultLabel.Text = "HOST FRAMEWORK CLOSURE FOUNDATION: EXCEPTION";
            _runtimeFrameworkBindingResultLabel.TextColor = UIColor.SystemRed;
            _runtimeFrameworkBindingDetailLabel.Text = FormatHostFrameworkClosureDetail($"Unhandled Step 22 exception: {ex.GetType().Name}: {ex.Message}");
            _statusLabel.Text = "STEP 22 FAIL: stop at the current host-framework-closure gate and report/export the diagnostic plan if available.";
            _statusLabel.TextColor = UIColor.SystemRed;
        }
        finally
        {
            await WriteDeviceTestReportFromLabelsAsync(
                "Step22-HostBindingFrontier.txt",
                "StS2 Launcher — Step 22 Host Binding Frontier",
                _runtimeFrameworkBindingResultLabel,
                _runtimeFrameworkBindingDetailLabel,
                CancellationToken.None);
            EndSteamOperation();
        }
    }

    private bool RecordHostFrameworkClosureGate(HostFrameworkClosureGateResult result)
    {
        _hostFrameworkClosureGates.Record(result);
        if (_runtimeFrameworkBindingResultLabel is not null)
        {
            _runtimeFrameworkBindingResultLabel.Text = _hostFrameworkClosureGates.Snapshot().Summary;
            _runtimeFrameworkBindingResultLabel.TextColor = result.Passed ? UIColor.Label : UIColor.SystemRed;
        }
        if (_runtimeFrameworkBindingDetailLabel is not null)
            _runtimeFrameworkBindingDetailLabel.Text = FormatHostFrameworkClosureDetail(result.Detail);
        if (!result.Passed && _statusLabel is not null)
        {
            var letter = (char)('A' + (int)result.Gate - 1);
            _statusLabel.Text = $"STEP 22 FAIL at Gate {letter} ({result.Gate}). Stop here; later closure gates were not run.";
            _statusLabel.TextColor = UIColor.SystemRed;
        }
        return result.Passed;
    }

    private string FormatHostFrameworkClosureDetail(string tail)
    {
        var lines = new List<string>();
        foreach (var gate in _hostFrameworkClosureGates.Results)
            lines.Add($"{(char)('A' + (int)gate.Gate - 1)} — {gate.Gate}: {(gate.Passed ? "PASS" : "FAIL")}");
        if (lines.Count > 0)
            lines.Add(string.Empty);
        lines.Add(tail);
        return string.Join("\n", lines);
    }

    private async Task RunPreparedRuntimeFrameworkBindingAsync()
    {
        if (_runtimeFrameworkBindingResultLabel is null ||
            _runtimeFrameworkBindingDetailLabel is null ||
            _runtimeFrameworkBindingButton is null ||
            _statusLabel is null)
        {
            return;
        }

        if (_godotProcessRequiresRestart)
        {
            _statusLabel.Text = "Step 15 Godot process-global state is still active. Force-quit/relaunch before Step 21 so the real dependency/binding plan is measured in a clean host process.";
            _statusLabel.TextColor = UIColor.SystemOrange;
            return;
        }

        BeginSteamOperation(allowCancel: true);
        _runtimeFrameworkBindingGates.Reset();
        _preparedRuntimeFrameworkBinding.Reset();
        _runtimeFrameworkBindingResultLabel.Text = "PREPARED RUNTIME / FRAMEWORK BINDING: GATE A RUNNING…";
        _runtimeFrameworkBindingResultLabel.TextColor = UIColor.Label;
        _runtimeFrameworkBindingDetailLabel.Text = "Gate A: re-proving OfflineReady and cloning/classifying the real receipt-backed ARM64/shared managed scope without CLR-loading StS2.";
        _statusLabel.Text = "STEP 21 GATE A — runtime payload classification.";
        _statusLabel.TextColor = UIColor.Label;

        try
        {
            var token = _operationCts?.Token ?? CancellationToken.None;
            var progress = new Progress<RuntimeFrameworkBindingProgress>(value =>
            {
                var count = value.TotalItems > 0 ? $" ({value.ProcessedItems:N0}/{value.TotalItems:N0})" : string.Empty;
                _runtimeFrameworkBindingDetailLabel.Text = FormatRuntimeFrameworkBindingDetail(
                    $"Gate {(char)('A' + (int)value.Gate - 1)} progress{count}: {value.Detail}" +
                    (string.IsNullOrWhiteSpace(value.CurrentPath) ? string.Empty : $"\nCurrent: {value.CurrentPath}"));
            });

            var gateA = await _preparedRuntimeFrameworkBinding.RunRuntimePayloadClassificationAsync(progress, token);
            if (!RecordRuntimeFrameworkBindingGate(gateA)) return;

            _runtimeFrameworkBindingResultLabel.Text = "PREPARED RUNTIME / FRAMEWORK BINDING: GATE B RUNNING…";
            _statusLabel.Text = "STEP 21 GATE B — real AssemblyRef graph + iOS host framework/private binding plan.";
            var gateB = await Task.Run(() => _preparedRuntimeFrameworkBinding.RunHostFrameworkBindingPlan(), token);
            if (!RecordRuntimeFrameworkBindingGate(gateB)) return;

            _runtimeFrameworkBindingResultLabel.Text = "PREPARED RUNTIME / FRAMEWORK BINDING: GATE C RUNNING…";
            _statusLabel.Text = "STEP 21 GATE C — byte-identical execution-oriented IL-only prepared set + persisted binding plan.";
            var gateC = await _preparedRuntimeFrameworkBinding.RunPreparedRuntimeAssemblySetAsync(progress, token);
            if (!RecordRuntimeFrameworkBindingGate(gateC)) return;

            _runtimeFrameworkBindingResultLabel.Text = "PREPARED RUNTIME / FRAMEWORK BINDING: GATE D RUNNING…";
            _statusLabel.Text = "STEP 21 GATE D — source/prepared/live/plan closure audit.";
            var gateD = await _preparedRuntimeFrameworkBinding.RunClosureAuditAsync(progress, token);
            if (!RecordRuntimeFrameworkBindingGate(gateD)) return;

            await TryExportRuntimeBindingDiagnosticsAsync(automatic: true, token);

            var snapshot = _runtimeFrameworkBindingGates.Snapshot();
            _runtimeFrameworkBindingResultLabel.Text = snapshot.Summary;
            _runtimeFrameworkBindingResultLabel.TextColor = UIColor.Label;
            _runtimeFrameworkBindingDetailLabel.Text = FormatRuntimeFrameworkBindingDetail(
                "All four Step 21 gates passed. The real managed dependency graph has an audited host/private binding plan and byte-identical prepared IL set. Step 21.1 also attempted to refresh the Files-accessible full diagnostic report. Read Gate B/D's Runtime closure ready signal before Step 22.");
            _statusLabel.Text = "PASS: STEP 21 PREPARED RUNTIME / FRAMEWORK BINDING — 4/4. Binding plan is physically audited; inspect Runtime closure ready YES/NO before the next subsystem.";
            _statusLabel.TextColor = UIColor.Label;
        }
        catch (OperationCanceledException)
        {
            _runtimeFrameworkBindingResultLabel.Text = "PREPARED RUNTIME / FRAMEWORK BINDING: CANCELLED";
            _runtimeFrameworkBindingResultLabel.TextColor = UIColor.SecondaryLabel;
            _runtimeFrameworkBindingDetailLabel.Text = FormatRuntimeFrameworkBindingDetail("Step 21 was cancelled. Rerunning Gate A recreates only the launcher-private Step 21 workspace; the receipt-backed managed install is never an intended write target.");
            _statusLabel.Text = "STEP 21 CANCELLED — no later gate is considered proven.";
            _statusLabel.TextColor = UIColor.SecondaryLabel;
        }
        catch (Exception ex)
        {
            _runtimeFrameworkBindingResultLabel.Text = "PREPARED RUNTIME / FRAMEWORK BINDING: EXCEPTION";
            _runtimeFrameworkBindingResultLabel.TextColor = UIColor.SystemRed;
            _runtimeFrameworkBindingDetailLabel.Text = FormatRuntimeFrameworkBindingDetail($"Unhandled Step 21 exception: {ex.GetType().Name}: {ex.Message}");
            _statusLabel.Text = "STEP 21 FAIL: stop at the current runtime/framework-binding gate and report this screen.";
            _statusLabel.TextColor = UIColor.SystemRed;
        }
        finally
        {
            await WriteDeviceTestReportFromLabelsAsync(
                "Step21-RuntimeFrameworkBinding.txt",
                "StS2 Launcher — Step 21 Runtime / Framework Binding",
                _runtimeFrameworkBindingResultLabel,
                _runtimeFrameworkBindingDetailLabel,
                CancellationToken.None);
            EndSteamOperation();
        }
    }

    private async Task RunRuntimeBindingDiagnosticsExportAsync()
    {
        await TryExportRuntimeBindingDiagnosticsAsync(automatic: false, CancellationToken.None);
    }

    private async Task TryExportRuntimeBindingDiagnosticsAsync(bool automatic, CancellationToken cancellationToken)
    {
        if (_runtimeBindingDiagnosticsExportResultLabel is null)
            return;

        try
        {
            _runtimeBindingDiagnosticsExportResultLabel.Text = automatic
                ? "DIAGNOSTIC EXPORT: refreshing complete report after Gate D…"
                : "DIAGNOSTIC EXPORT: reading persisted Step 21 plan and writing Files report…";
            _runtimeBindingDiagnosticsExportResultLabel.TextColor = UIColor.Label;

            var result = await _runtimeBindingDiagnosticsExporter.ExportAsync(cancellationToken);
            _runtimeBindingDiagnosticsExportResultLabel.Text =
                $"DIAGNOSTIC EXPORT: PASS — {result.BlockerCount:N0} blockers / {result.UniqueBlockedRequestedIdentityCount:N0} unique requested identities\n" +
                $"Files: On My iPhone → StS2 Launcher → StS2Launcher → {RuntimeBindingDiagnosticsExporter.ReportFileName}\n" +
                $"Runtime closure ready: {(result.RuntimeClosureReady ? "YES" : "NO")}\n" +
                $"Plan SHA-256: {result.PlanSha256}\nReport SHA-256: {result.ReportSha256}";
            _runtimeBindingDiagnosticsExportResultLabel.TextColor = UIColor.Label;

            if (!automatic && _statusLabel is not null)
            {
                _statusLabel.Text = $"STEP 21.1 DIAGNOSTIC EXPORT PASS — open Files and send {RuntimeBindingDiagnosticsExporter.ReportFileName}. No game CLR load was attempted.";
                _statusLabel.TextColor = UIColor.Label;
            }
        }
        catch (OperationCanceledException)
        {
            if (automatic)
                throw;
            _runtimeBindingDiagnosticsExportResultLabel.Text = "DIAGNOSTIC EXPORT: CANCELLED";
            _runtimeBindingDiagnosticsExportResultLabel.TextColor = UIColor.SecondaryLabel;
        }
        catch (Exception ex)
        {
            _runtimeBindingDiagnosticsExportResultLabel.Text =
                $"DIAGNOSTIC EXPORT: FAIL — {ex.GetType().Name}: {ex.Message}\n" +
                "If the persisted Step 21 plan is missing, rerun Step 21 A–D once and then tap Export again.";
            _runtimeBindingDiagnosticsExportResultLabel.TextColor = UIColor.SystemRed;
            if (!automatic && _statusLabel is not null)
            {
                _statusLabel.Text = "STEP 21.1 DIAGNOSTIC EXPORT FAIL — no binding policy was changed; report the export error.";
                _statusLabel.TextColor = UIColor.SystemRed;
            }
        }
    }

    private bool RecordRuntimeFrameworkBindingGate(RuntimeFrameworkBindingGateResult result)
    {
        _runtimeFrameworkBindingGates.Record(result.Gate, result.Passed, result.Detail);
        if (_runtimeFrameworkBindingResultLabel is not null)
        {
            _runtimeFrameworkBindingResultLabel.Text = _runtimeFrameworkBindingGates.Snapshot().Summary;
            _runtimeFrameworkBindingResultLabel.TextColor = result.Passed ? UIColor.Label : UIColor.SystemRed;
        }
        if (_runtimeFrameworkBindingDetailLabel is not null)
            _runtimeFrameworkBindingDetailLabel.Text = FormatRuntimeFrameworkBindingDetail(result.Detail);
        if (!result.Passed && _statusLabel is not null)
        {
            var letter = (char)('A' + (int)result.Gate - 1);
            _statusLabel.Text = $"STEP 21 FAIL at Gate {letter} ({result.Gate}). Stop here; later runtime/framework-binding gates were not run.";
            _statusLabel.TextColor = UIColor.SystemRed;
        }
        return result.Passed;
    }

    private string FormatRuntimeFrameworkBindingDetail(string tail)
    {
        var lines = new List<string>();
        foreach (var gate in _runtimeFrameworkBindingGates.Results)
        {
            var letter = (char)('A' + (int)gate.Gate - 1);
            lines.Add($"Gate {letter} — {gate.Gate}: {(gate.Passed ? "PASS" : "FAIL")}");
            lines.Add(gate.Detail);
            lines.Add(string.Empty);
        }

        lines.Add("Step 21 write scope: launcher-private Step21-PreparedRuntimeBinding/source + prepared + plan only; the Step 12 receipt-backed managed install stays read-only.");
        lines.Add("CLR load scope: iOS host framework contracts only. Real sts2.dll/GodotSharp/game assemblies are inspected with Cecil as data but are never loaded into the CLR in Step 21.");
        lines.Add("Binding policy: prefer a compatible iOS-host framework assembly for System/platform contracts; otherwise resolve only exact/controlled-version identities from the verified ARM64/shared workspace. Missing, ambiguous, lower-version and non-IL-only edges become explicit blockers—never broad fallback.");
        lines.Add("Step 21 4/4 means the plan/prepared set is trustworthy. It does NOT override Runtime closure ready: NO; blockers must be addressed before any first real game CLR load.");
        lines.Add("Steps 01–20 remain closed/protected. Closure requires OfflineReady + Foundation 5/5 after a 4/4 pass.");
        lines.Add("Out of scope: game static initialization/execution, GodotSharp behavioral integration, native game loading, Harmony/MonoMod, FMOD/Spine, Cloud, and Workshop.");
        lines.Add("Step 15 orientation presentation quirk remains a known non-blocking cleanup item.");
        lines.Add(tail);
        return string.Join("\n", lines);
    }

    private bool RecordDynamicManagedExecutionGate(DynamicManagedExecutionGateResult result)
    {
        _dynamicManagedExecutionGates.Record(result.Gate, result.Passed, result.Detail);
        if (_dynamicManagedExecutionResultLabel is not null)
        {
            _dynamicManagedExecutionResultLabel.Text = _dynamicManagedExecutionGates.Snapshot().Summary;
            _dynamicManagedExecutionResultLabel.TextColor = result.Passed ? UIColor.Label : UIColor.SystemRed;
        }
        if (_dynamicManagedExecutionDetailLabel is not null)
            _dynamicManagedExecutionDetailLabel.Text = FormatDynamicManagedExecutionDetail(result.Detail);
        if (!result.Passed && _statusLabel is not null)
        {
            var letter = (char)('A' + (int)result.Gate - 1);
            _statusLabel.Text = $"STEP 20 FAIL at Gate {letter} ({result.Gate}). Stop here; later dynamic-managed-execution gates were not run.";
            _statusLabel.TextColor = UIColor.SystemRed;
        }
        return result.Passed;
    }

    private string FormatDynamicManagedExecutionDetail(string tail)
    {
        var lines = new List<string>();
        foreach (var gate in _dynamicManagedExecutionGates.Results)
        {
            var letter = (char)('A' + (int)gate.Gate - 1);
            lines.Add($"Gate {letter} — {gate.Gate}: {(gate.Passed ? "PASS" : "FAIL")}");
            lines.Add(gate.Detail);
            lines.Add(string.Empty);
        }

        lines.Add("Step 20 write scope: launcher-private Step20-DynamicManagedExecution/fixtures only; the Step 12 receipt-backed managed install stays read-only.");
        lines.Add("Dynamic execution scope: project-owned exact-hash fixtures only. AssemblyLoadContext/reflective invocation are intentionally permitted here solely to prove non-AOT IL execution and one controlled private dependency hop.");
        lines.Add("Out of scope: Assembly.Load/AssemblyLoadContext for sts2.dll or any game assembly, game static initialization, GodotSharp binding, native game integration, Harmony/MonoMod, FMOD/Spine, Cloud, and Workshop.");
        lines.Add("Steps 01–19 remain closed/protected. Step 20 retains AOT for build-time assemblies while adding interpreter availability for runtime/dynamic managed code; closure therefore requires OfflineReady + Foundation 5/5 after a 4/4 pass.");
        lines.Add("Step 15 orientation presentation quirk remains a known non-blocking cleanup item.");
        lines.Add(tail);
        return string.Join("\n", lines);
    }
}
