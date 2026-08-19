using Foundation;
using StS2Launcher.Core;
using StS2Launcher.Step05.iOS.Platform;
using UIKit;

namespace StS2Launcher.Step05.iOS;

public sealed partial class RootViewController
{
    private async Task RunManagedPreparationFoundationAsync()
    {
        if (_managedPreparationResultLabel is null ||
            _managedPreparationDetailLabel is null ||
            _managedPreparationButton is null ||
            _statusLabel is null)
        {
            return;
        }

        if (_godotProcessRequiresRestart)
        {
            _statusLabel.Text = "Step 15 Godot process-global state is still active. Force-quit/relaunch before running Step 16 so Cecil/real-install evidence is isolated from the Godot session.";
            _statusLabel.TextColor = UIColor.SystemOrange;
            return;
        }

        BeginSteamOperation(allowCancel: true);
        _managedPreparationGates.Reset();
        _managedPreparationResultLabel.Text = "MANAGED PREPARATION: GATE A RUNNING…";
        _managedPreparationResultLabel.TextColor = UIColor.Label;
        _managedPreparationDetailLabel.Text = "Gate A: opening the bundled project-owned fixture as raw managed metadata with Mono.Cecil; the assembly is not loaded or executed.";
        _statusLabel.Text = "STEP 16 GATE A — Cecil fixture read.";
        _statusLabel.TextColor = UIColor.Label;

        try
        {
            var fixturePath = Path.Combine(
                NSBundle.MainBundle.BundlePath,
                "Step16Fixtures",
                "StS2Launcher.Step16.Fixture.dll");

            var gateA = await Task.Run(() => _managedPreparationFoundation.RunFixtureRead(fixturePath));
            if (!RecordManagedPreparationGate(gateA))
                return;

            _managedPreparationResultLabel.Text = "MANAGED PREPARATION: GATE B RUNNING…";
            _statusLabel.Text = "STEP 16 GATE B — Cecil fixture write/reopen.";
            var gateB = await Task.Run(() => _managedPreparationFoundation.RunFixtureRoundTrip(fixturePath));
            if (!RecordManagedPreparationGate(gateB))
                return;

            _managedPreparationResultLabel.Text = "MANAGED PREPARATION: GATE C RUNNING…";
            _statusLabel.Text = "STEP 16 GATE C — controlled fixture-only IL rewrite.";
            var gateC = await Task.Run(() => _managedPreparationFoundation.RunControlledIlRewrite(fixturePath));
            if (!RecordManagedPreparationGate(gateC))
                return;

            _managedPreparationResultLabel.Text = "MANAGED PREPARATION: GATE D RUNNING…";
            _statusLabel.Text = "STEP 16 GATE D — read-only receipt-backed StS2 managed metadata inspection.";
            var progress = new Progress<ManagedPreparationProgress>(value =>
            {
                var count = value.TotalItems > 0
                    ? $" ({value.ProcessedItems:N0}/{value.TotalItems:N0})"
                    : string.Empty;
                _managedPreparationDetailLabel.Text = FormatManagedPreparationDetail(
                    $"Gate D progress{count}: {value.Detail}" +
                    (string.IsNullOrWhiteSpace(value.CurrentPath) ? string.Empty : $"\nCurrent: {value.CurrentPath}"));
            });

            var token = _operationCts?.Token ?? CancellationToken.None;
            var gateD = await _managedPreparationFoundation.RunRealStS2MetadataInspectionAsync(progress, token);
            if (!RecordManagedPreparationGate(gateD))
                return;

            var snapshot = _managedPreparationGates.Snapshot();
            _managedPreparationResultLabel.Text = snapshot.Summary;
            _managedPreparationResultLabel.TextColor = UIColor.Label;
            _managedPreparationDetailLabel.Text = FormatManagedPreparationDetail(
                "All four Step 16 gates passed. Cecil proved read/write/reopen + a controlled project-owned IL transformation, then parsed the real installed StS2 managed metadata without rewriting or loading game assemblies.");
            _statusLabel.Text = "PASS: STEP 16 MANAGED PREPARATION — 4/4. Fixture read/write/rewrite and real read-only StS2 metadata inspection are proven on this iPhone.";
            _statusLabel.TextColor = UIColor.Label;
        }
        catch (OperationCanceledException)
        {
            _managedPreparationResultLabel.Text = "MANAGED PREPARATION: CANCELLED";
            _managedPreparationResultLabel.TextColor = UIColor.SecondaryLabel;
            _managedPreparationDetailLabel.Text = FormatManagedPreparationDetail(
                "Step 16 was cancelled. Fixture outputs may remain only under launcher-private Step16-ManagedPreparation scratch storage; the real managed install was not intentionally modified.");
            _statusLabel.Text = "STEP 16 CANCELLED — no later gate is considered proven.";
            _statusLabel.TextColor = UIColor.SecondaryLabel;
        }
        catch (Exception ex)
        {
            _managedPreparationResultLabel.Text = "MANAGED PREPARATION: EXCEPTION";
            _managedPreparationResultLabel.TextColor = UIColor.SystemRed;
            _managedPreparationDetailLabel.Text = FormatManagedPreparationDetail($"Unhandled Step 16 exception: {ex.GetType().Name}: {ex.Message}");
            _statusLabel.Text = "STEP 16 FAIL: stop at the current managed-preparation gate and report this screen.";
            _statusLabel.TextColor = UIColor.SystemRed;
        }
        finally
        {
            await WriteDeviceTestReportFromLabelsAsync(
                "Step16-ManagedPreparation.txt",
                "StS2 Launcher — Step 16 Managed Preparation",
                _managedPreparationResultLabel,
                _managedPreparationDetailLabel,
                CancellationToken.None);
            EndSteamOperation();
        }
    }

    private async Task RunCompatibilityCallSiteAnalysisAsync()
    {
        if (_compatibilityCallSiteResultLabel is null ||
            _compatibilityCallSiteDetailLabel is null ||
            _compatibilityCallSiteButton is null ||
            _statusLabel is null)
        {
            return;
        }

        if (_godotProcessRequiresRestart)
        {
            _statusLabel.Text = "Step 15 Godot process-global state is still active. Force-quit/relaunch before Step 17 so the read-only compatibility evidence is isolated from the Godot session.";
            _statusLabel.TextColor = UIColor.SystemOrange;
            return;
        }

        BeginSteamOperation(allowCancel: true);
        _compatibilityCallSiteGates.Reset();
        _compatibilityCallSiteAnalysis.Reset();
        _compatibilityCallSiteResultLabel.Text = "COMPATIBILITY CALL-SITE ANALYSIS: GATE A RUNNING…";
        _compatibilityCallSiteResultLabel.TextColor = UIColor.Label;
        _compatibilityCallSiteDetailLabel.Text = "Gate A: re-proving OfflineReady and selecting the receipt-backed macOS arm64 + architecture-neutral managed scope without opening game assemblies.";
        _statusLabel.Text = "STEP 17 GATE A — ARM64 managed scope + local integrity.";
        _statusLabel.TextColor = UIColor.Label;

        try
        {
            var token = _operationCts?.Token ?? CancellationToken.None;
            var progress = new Progress<CompatibilityCallSiteProgress>(value =>
            {
                var count = value.TotalItems > 0
                    ? $" ({value.ProcessedItems:N0}/{value.TotalItems:N0})"
                    : string.Empty;
                _compatibilityCallSiteDetailLabel.Text = FormatCompatibilityCallSiteDetail(
                    $"Gate {(char)('A' + (int)value.Gate - 1)} progress{count}: {value.Detail}" +
                    (string.IsNullOrWhiteSpace(value.CurrentPath) ? string.Empty : $"\nCurrent: {value.CurrentPath}"));
            });

            var gateA = await _compatibilityCallSiteAnalysis.RunArm64ManagedScopeAsync(progress, token);
            if (!RecordCompatibilityCallSiteGate(gateA))
                return;

            _compatibilityCallSiteResultLabel.Text = "COMPATIBILITY CALL-SITE ANALYSIS: GATE B RUNNING…";
            _statusLabel.Text = "STEP 17 GATE B — actual IL method-reference scan.";
            var gateB = await _compatibilityCallSiteAnalysis.RunActualIlCallSiteScanAsync(progress, token);
            if (!RecordCompatibilityCallSiteGate(gateB))
                return;

            _compatibilityCallSiteResultLabel.Text = "COMPATIBILITY CALL-SITE ANALYSIS: GATE C RUNNING…";
            _statusLabel.Text = "STEP 17 GATE C — native/platform interop classification.";
            var gateC = await Task.Run(() => _compatibilityCallSiteAnalysis.RunNativePlatformInteropClassification(), token);
            if (!RecordCompatibilityCallSiteGate(gateC))
                return;

            _compatibilityCallSiteResultLabel.Text = "COMPATIBILITY CALL-SITE ANALYSIS: GATE D RUNNING…";
            _statusLabel.Text = "STEP 17 GATE D — primary sts2.dll dependency pressure map + post-scan hashes.";
            var gateD = await _compatibilityCallSiteAnalysis.RunPrimaryDependencyPressureMapAsync(progress, token);
            if (!RecordCompatibilityCallSiteGate(gateD))
                return;

            var snapshot = _compatibilityCallSiteGates.Snapshot();
            _compatibilityCallSiteResultLabel.Text = snapshot.Summary;
            _compatibilityCallSiteResultLabel.TextColor = UIColor.Label;
            _compatibilityCallSiteDetailLabel.Text = FormatCompatibilityCallSiteDetail(
                "All four Step 17 gates passed. The broad Step 14 indicators have been narrowed to concrete arm64 IL/native/dependency evidence, while every scanned file still matches its Step 12 receipt SHA-1.");
            _statusLabel.Text = "PASS: STEP 17 COMPATIBILITY CALL-SITE ANALYSIS — 4/4. Upload the Gate B–D evidence so the next compatibility target can be chosen from actual IL rather than string indicators.";
            _statusLabel.TextColor = UIColor.Label;
        }
        catch (OperationCanceledException)
        {
            _compatibilityCallSiteResultLabel.Text = "COMPATIBILITY CALL-SITE ANALYSIS: CANCELLED";
            _compatibilityCallSiteResultLabel.TextColor = UIColor.SecondaryLabel;
            _compatibilityCallSiteDetailLabel.Text = FormatCompatibilityCallSiteDetail(
                "Step 17 was cancelled. The analysis is read-only; no game-file write or runtime load was intentionally performed.");
            _statusLabel.Text = "STEP 17 CANCELLED — no later gate is considered proven.";
            _statusLabel.TextColor = UIColor.SecondaryLabel;
        }
        catch (Exception ex)
        {
            _compatibilityCallSiteResultLabel.Text = "COMPATIBILITY CALL-SITE ANALYSIS: EXCEPTION";
            _compatibilityCallSiteResultLabel.TextColor = UIColor.SystemRed;
            _compatibilityCallSiteDetailLabel.Text = FormatCompatibilityCallSiteDetail($"Unhandled Step 17 exception: {ex.GetType().Name}: {ex.Message}");
            _statusLabel.Text = "STEP 17 FAIL: stop at the current call-site-analysis gate and report this screen.";
            _statusLabel.TextColor = UIColor.SystemRed;
        }
        finally
        {
            await WriteDeviceTestReportFromLabelsAsync(
                "Step17-CompatibilityCallSites.txt",
                "StS2 Launcher — Step 17 Compatibility Call-Site Analysis",
                _compatibilityCallSiteResultLabel,
                _compatibilityCallSiteDetailLabel,
                CancellationToken.None);
            EndSteamOperation();
        }
    }

    private async Task RunRealAssemblyRewriteWorkspaceAsync()
    {
        if (_realAssemblyRewriteResultLabel is null ||
            _realAssemblyRewriteDetailLabel is null ||
            _realAssemblyRewriteButton is null ||
            _statusLabel is null)
        {
            return;
        }

        if (_godotProcessRequiresRestart)
        {
            _statusLabel.Text = "Step 15 Godot process-global state is still active. Force-quit/relaunch before Step 18 so Cecil real-copy rewrite testing is isolated from the Godot session.";
            _statusLabel.TextColor = UIColor.SystemOrange;
            return;
        }

        BeginSteamOperation(allowCancel: true);
        _realAssemblyRewriteGates.Reset();
        _realAssemblyRewriteWorkspace.Reset();
        _realAssemblyRewriteResultLabel.Text = "REAL ASSEMBLY REWRITE WORKSPACE: GATE A RUNNING…";
        _realAssemblyRewriteResultLabel.TextColor = UIColor.Label;
        _realAssemblyRewriteDetailLabel.Text = "Gate A: re-proving OfflineReady and cloning the receipt-backed ARM64/shared managed scope into launcher-private Step 18 scratch storage with per-file SHA-1 verification.";
        _statusLabel.Text = "STEP 18 GATE A — clone receipt-backed ARM64 compatibility workspace.";
        _statusLabel.TextColor = UIColor.Label;

        try
        {
            var token = _operationCts?.Token ?? CancellationToken.None;
            var progress = new Progress<RealAssemblyRewriteProgress>(value =>
            {
                var count = value.TotalItems > 0
                    ? $" ({value.ProcessedItems:N0}/{value.TotalItems:N0})"
                    : string.Empty;
                _realAssemblyRewriteDetailLabel.Text = FormatRealAssemblyRewriteDetail(
                    $"Gate {(char)('A' + (int)value.Gate - 1)} progress{count}: {value.Detail}" +
                    (string.IsNullOrWhiteSpace(value.CurrentPath) ? string.Empty : $"\nCurrent: {value.CurrentPath}"));
            });

            var gateA = await _realAssemblyRewriteWorkspace.RunWorkspaceCloneAsync(progress, token);
            if (!RecordRealAssemblyRewriteGate(gateA))
                return;

            _realAssemblyRewriteResultLabel.Text = "REAL ASSEMBLY REWRITE WORKSPACE: GATE B RUNNING…";
            _statusLabel.Text = "STEP 18 GATE B — Cecil write/reopen of copied primary sts2.dll.";
            var gateB = await Task.Run(() => _realAssemblyRewriteWorkspace.RunPrimaryRoundTrip(), token);
            if (!RecordRealAssemblyRewriteGate(gateB))
                return;

            _realAssemblyRewriteResultLabel.Text = "REAL ASSEMBLY REWRITE WORKSPACE: GATE C RUNNING…";
            _statusLabel.Text = "STEP 18 GATE C — semantics-neutral NOP rewrite on copied sts2.dll only.";
            var gateC = await Task.Run(() => _realAssemblyRewriteWorkspace.RunNeutralIlRewrite(), token);
            if (!RecordRealAssemblyRewriteGate(gateC))
                return;

            _realAssemblyRewriteResultLabel.Text = "REAL ASSEMBLY REWRITE WORKSPACE: GATE D RUNNING…";
            _statusLabel.Text = "STEP 18 GATE D — source/install SHA-1 isolation audit.";
            var gateD = await _realAssemblyRewriteWorkspace.RunIsolationAuditAsync(progress, token);
            if (!RecordRealAssemblyRewriteGate(gateD))
                return;

            var snapshot = _realAssemblyRewriteGates.Snapshot();
            _realAssemblyRewriteResultLabel.Text = snapshot.Summary;
            _realAssemblyRewriteResultLabel.TextColor = UIColor.Label;
            _realAssemblyRewriteDetailLabel.Text = FormatRealAssemblyRewriteDetail(
                "All four Step 18 gates passed. A receipt-identical ARM64 managed workspace was created, Cecil round-tripped the real copied sts2.dll, one neutral NOP was written/reopened in a copy, and every original managed file in scope still matched its trusted receipt SHA-1.");
            _statusLabel.Text = "PASS: STEP 18 REAL ASSEMBLY REWRITE WORKSPACE — 4/4. Real copied-assembly writing is proven; the actual managed install remained unchanged.";
            _statusLabel.TextColor = UIColor.Label;
        }
        catch (OperationCanceledException)
        {
            _realAssemblyRewriteResultLabel.Text = "REAL ASSEMBLY REWRITE WORKSPACE: CANCELLED";
            _realAssemblyRewriteResultLabel.TextColor = UIColor.SecondaryLabel;
            _realAssemblyRewriteDetailLabel.Text = FormatRealAssemblyRewriteDetail(
                "Step 18 was cancelled. Gate A recreates its launcher-private workspace from scratch on the next run; no write to the real managed install is intentional." );
            _statusLabel.Text = "STEP 18 CANCELLED — no later gate is considered proven.";
            _statusLabel.TextColor = UIColor.SecondaryLabel;
        }
        catch (Exception ex)
        {
            _realAssemblyRewriteResultLabel.Text = "REAL ASSEMBLY REWRITE WORKSPACE: EXCEPTION";
            _realAssemblyRewriteResultLabel.TextColor = UIColor.SystemRed;
            _realAssemblyRewriteDetailLabel.Text = FormatRealAssemblyRewriteDetail($"Unhandled Step 18 exception: {ex.GetType().Name}: {ex.Message}");
            _statusLabel.Text = "STEP 18 FAIL: stop at the current real-assembly rewrite gate and report this screen.";
            _statusLabel.TextColor = UIColor.SystemRed;
        }
        finally
        {
            await WriteDeviceTestReportFromLabelsAsync(
                "Step18-RealAssemblyRewrite.txt",
                "StS2 Launcher — Step 18 Real Assembly Rewrite Workspace",
                _realAssemblyRewriteResultLabel,
                _realAssemblyRewriteDetailLabel,
                CancellationToken.None);
            EndSteamOperation();
        }
    }

    private bool RecordRealAssemblyRewriteGate(RealAssemblyRewriteGateResult result)
    {
        _realAssemblyRewriteGates.Record(result.Gate, result.Passed, result.Detail);
        if (_realAssemblyRewriteResultLabel is not null)
        {
            _realAssemblyRewriteResultLabel.Text = _realAssemblyRewriteGates.Snapshot().Summary;
            _realAssemblyRewriteResultLabel.TextColor = result.Passed ? UIColor.Label : UIColor.SystemRed;
        }
        if (_realAssemblyRewriteDetailLabel is not null)
            _realAssemblyRewriteDetailLabel.Text = FormatRealAssemblyRewriteDetail(result.Detail);
        if (!result.Passed && _statusLabel is not null)
        {
            var letter = (char)('A' + (int)result.Gate - 1);
            _statusLabel.Text = $"STEP 18 FAIL at Gate {letter} ({result.Gate}). Stop here; later real-assembly rewrite gates were not run.";
            _statusLabel.TextColor = UIColor.SystemRed;
        }
        return result.Passed;
    }

    private string FormatRealAssemblyRewriteDetail(string tail)
    {
        var lines = new List<string>();
        foreach (var gate in _realAssemblyRewriteGates.Results)
        {
            var letter = (char)('A' + (int)gate.Gate - 1);
            lines.Add($"Gate {letter} — {gate.Gate}: {(gate.Passed ? "PASS" : "FAIL")}");
            lines.Add(gate.Detail);
            lines.Add(string.Empty);
        }

        lines.Add("Step 18 write scope: launcher-private Step18-RealAssemblyRewrite copies only; the Step 12 receipt-backed managed install stays read-only.");
        lines.Add("Gate C transformation is intentionally semantics-neutral: one IL NOP inserted into a deterministic method of the copied primary arm64 sts2.dll.");
        lines.Add("Cecil writer-required dependency resolution stays confined to the SHA-1-verified Step 18 workspace; Assembly.Load, StS2 execution, FMOD/Spine runtime integration, Cloud, or Workshop is not advanced by Step 18.");
        lines.Add("Step 15 orientation presentation quirk remains a known non-blocking cleanup item.");
        lines.Add(tail);
        return string.Join("\n", lines);
    }

    private async Task RunExpressionInterpreterCompatibilityAsync()
    {
        if (_expressionInterpreterCompatibilityResultLabel is null ||
            _expressionInterpreterCompatibilityDetailLabel is null ||
            _expressionInterpreterCompatibilityButton is null ||
            _statusLabel is null)
        {
            return;
        }

        if (_godotProcessRequiresRestart)
        {
            _statusLabel.Text = "Step 15 Godot process-global state is still active. Force-quit/relaunch before Step 19 so the expression runtime/fallback proof runs in a clean process.";
            _statusLabel.TextColor = UIColor.SystemOrange;
            return;
        }

        BeginSteamOperation(allowCancel: true);
        _expressionInterpreterCompatibilityGates.Reset();
        _expressionInterpreterCompatibility.Reset();
        _expressionInterpreterCompatibilityResultLabel.Text = "EXPRESSION INTERPRETER COMPATIBILITY: GATE A RUNNING…";
        _expressionInterpreterCompatibilityResultLabel.TextColor = UIColor.Label;
        _expressionInterpreterCompatibilityDetailLabel.Text = "Gate A: proving Compile(), Compile(false), and Compile(true) in this physical no-dynamic-code iOS process, then cloning a fresh receipt-backed ARM64/shared Step 19 workspace.";
        _statusLabel.Text = "STEP 19.2 GATE A — host expression fallback + receipt-backed workspace clone.";
        _statusLabel.TextColor = UIColor.Label;

        try
        {
            var token = _operationCts?.Token ?? CancellationToken.None;
            var progress = new Progress<ExpressionInterpreterCompatibilityProgress>(value =>
            {
                var count = value.TotalItems > 0
                    ? $" ({value.ProcessedItems:N0}/{value.TotalItems:N0})"
                    : string.Empty;
                _expressionInterpreterCompatibilityDetailLabel.Text = FormatExpressionInterpreterCompatibilityDetail(
                    $"Gate {(char)('A' + (int)value.Gate - 1)} progress{count}: {value.Detail}" +
                    (string.IsNullOrWhiteSpace(value.CurrentPath) ? string.Empty : $"\nCurrent: {value.CurrentPath}"));
            });

            var gateA = await _expressionInterpreterCompatibility.RunInterpreterCapabilityAndWorkspaceCloneAsync(progress, token);
            if (!RecordExpressionInterpreterCompatibilityGate(gateA))
                return;

            _expressionInterpreterCompatibilityResultLabel.Text = "EXPRESSION INTERPRETER COMPATIBILITY: GATE B RUNNING…";
            _statusLabel.Text = "STEP 19.2 GATE B — read-only Compile-site classification across consumer/framework and IL-only/ReadyToRun boundaries.";
            var gateB = await Task.Run(() => _expressionInterpreterCompatibility.RunRealCompileTargetDiscovery(), token);
            if (!RecordExpressionInterpreterCompatibilityGate(gateB))
                return;

            _expressionInterpreterCompatibilityResultLabel.Text = "EXPRESSION INTERPRETER COMPATIBILITY: GATE C RUNNING…";
            _statusLabel.Text = "STEP 19.2 GATE C — zero Cecil writes; build byte-identical prepared tree and prove immediate SHA-1 equality.";
            var gateC = await Task.Run(() => _expressionInterpreterCompatibility.RunHostFallbackPreparedCopy(), token);
            if (!RecordExpressionInterpreterCompatibilityGate(gateC))
                return;

            _expressionInterpreterCompatibilityResultLabel.Text = "EXPRESSION INTERPRETER COMPATIBILITY: GATE D RUNNING…";
            _statusLabel.Text = "STEP 19.2 GATE D — source/prepared/live full SHA-1 isolation audit with zero managed mutations.";
            var gateD = await _expressionInterpreterCompatibility.RunIsolationAuditAsync(progress, token);
            if (!RecordExpressionInterpreterCompatibilityGate(gateD))
                return;

            var snapshot = _expressionInterpreterCompatibilityGates.Snapshot();
            _expressionInterpreterCompatibilityResultLabel.Text = snapshot.Summary;
            _expressionInterpreterCompatibilityResultLabel.TextColor = UIColor.Label;
            _expressionInterpreterCompatibilityDetailLabel.Text = FormatExpressionInterpreterCompatibilityDetail(
                "All four Step 19 gates passed. The physical launcher proved Compile(), Compile(false), and Compile(true) against the no-dynamic-code iOS host, classified real Compile sites across consumer/framework and IL-only/ReadyToRun boundaries, performed zero Cecil assembly writes, kept the complete prepared tree byte-identical, and proved trusted source/live-install isolation.");
            _statusLabel.Text = "PASS: STEP 19.2 EXPRESSION INTERPRETER COMPATIBILITY — 4/4. Host runtime fallback + framework boundary + zero-write prepared tree are proven; no copied desktop framework image was mutated and no game assembly was executed.";
            _statusLabel.TextColor = UIColor.Label;
        }
        catch (OperationCanceledException)
        {
            _expressionInterpreterCompatibilityResultLabel.Text = "EXPRESSION INTERPRETER COMPATIBILITY: CANCELLED";
            _expressionInterpreterCompatibilityResultLabel.TextColor = UIColor.SecondaryLabel;
            _expressionInterpreterCompatibilityDetailLabel.Text = FormatExpressionInterpreterCompatibilityDetail(
                "Step 19 was cancelled. Gate A recreates the launcher-private Step 19 workspace from scratch on the next run; the real managed install is never an intended write target.");
            _statusLabel.Text = "STEP 19.2 CANCELLED — no later gate is considered proven.";
            _statusLabel.TextColor = UIColor.SecondaryLabel;
        }
        catch (Exception ex)
        {
            _expressionInterpreterCompatibilityResultLabel.Text = "EXPRESSION INTERPRETER COMPATIBILITY: EXCEPTION";
            _expressionInterpreterCompatibilityResultLabel.TextColor = UIColor.SystemRed;
            _expressionInterpreterCompatibilityDetailLabel.Text = FormatExpressionInterpreterCompatibilityDetail($"Unhandled Step 19 exception: {ex.GetType().Name}: {ex.Message}");
            _statusLabel.Text = "STEP 19.2 FAIL: stop at the current expression-interpreter compatibility gate and report this screen.";
            _statusLabel.TextColor = UIColor.SystemRed;
        }
        finally
        {
            await WriteDeviceTestReportFromLabelsAsync(
                "Step19-ExpressionInterpreter.txt",
                "StS2 Launcher — Step 19 Expression Interpreter Compatibility",
                _expressionInterpreterCompatibilityResultLabel,
                _expressionInterpreterCompatibilityDetailLabel,
                CancellationToken.None);
            EndSteamOperation();
        }
    }

    private bool RecordExpressionInterpreterCompatibilityGate(ExpressionInterpreterCompatibilityGateResult result)
    {
        _expressionInterpreterCompatibilityGates.Record(result.Gate, result.Passed, result.Detail);
        if (_expressionInterpreterCompatibilityResultLabel is not null)
        {
            _expressionInterpreterCompatibilityResultLabel.Text = _expressionInterpreterCompatibilityGates.Snapshot().Summary;
            _expressionInterpreterCompatibilityResultLabel.TextColor = result.Passed ? UIColor.Label : UIColor.SystemRed;
        }
        if (_expressionInterpreterCompatibilityDetailLabel is not null)
            _expressionInterpreterCompatibilityDetailLabel.Text = FormatExpressionInterpreterCompatibilityDetail(result.Detail);
        if (!result.Passed && _statusLabel is not null)
        {
            var letter = (char)('A' + (int)result.Gate - 1);
            _statusLabel.Text = $"STEP 19.2 FAIL at Gate {letter} ({result.Gate}). Stop here; later expression-interpreter compatibility gates were not run.";
            _statusLabel.TextColor = UIColor.SystemRed;
        }
        return result.Passed;
    }

    private string FormatExpressionInterpreterCompatibilityDetail(string tail)
    {
        var lines = new List<string>();
        foreach (var gate in _expressionInterpreterCompatibilityGates.Results)
        {
            var letter = (char)('A' + (int)gate.Gate - 1);
            lines.Add($"Gate {letter} — {gate.Gate}: {(gate.Passed ? "PASS" : "FAIL")}");
            lines.Add(gate.Detail);
            lines.Add(string.Empty);
        }

        lines.Add("Step 19 write scope: launcher-private Step19-ExpressionInterpreterCompatibility/source + prepared only; the Step 12 receipt-backed managed install stays read-only.");
        lines.Add("Behavioral rewrite scope: NONE in Step 19.2. Gate A proves host Compile()/Compile(false)/Compile(true) fallback behavior; Gate B read-only classifies real call sites; Gate C performs zero Cecil assembly writes and makes the prepared tree byte-identical. System.* framework and non-IL-only/ReadyToRun images are diagnostic-only.");
        lines.Add("Out of scope: mutating any copied expression call site or desktop framework image, framework substitution/binding for actual game execution, Harmony/MonoMod runtime detours, Reflection.Emit replacement, Assembly.Load, native runtime integration, StS2 execution, Cloud, and Workshop.");
        lines.Add("Step 18 remains closed/protected; its verified-workspace resolver principles are preserved in Step 19.");
        lines.Add("Step 15 orientation presentation quirk remains a known non-blocking cleanup item.");
        lines.Add(tail);
        return string.Join("\n", lines);
    }

    private bool RecordCompatibilityCallSiteGate(CompatibilityCallSiteGateResult result)
    {
        _compatibilityCallSiteGates.Record(result.Gate, result.Passed, result.Detail);
        if (_compatibilityCallSiteResultLabel is not null)
        {
            _compatibilityCallSiteResultLabel.Text = _compatibilityCallSiteGates.Snapshot().Summary;
            _compatibilityCallSiteResultLabel.TextColor = result.Passed ? UIColor.Label : UIColor.SystemRed;
        }
        if (_compatibilityCallSiteDetailLabel is not null)
            _compatibilityCallSiteDetailLabel.Text = FormatCompatibilityCallSiteDetail(result.Detail);
        if (!result.Passed && _statusLabel is not null)
        {
            var letter = (char)('A' + (int)result.Gate - 1);
            _statusLabel.Text = $"STEP 17 FAIL at Gate {letter} ({result.Gate}). Stop here; later compatibility-analysis gates were not run.";
            _statusLabel.TextColor = UIColor.SystemRed;
        }
        return result.Passed;
    }

    private string FormatCompatibilityCallSiteDetail(string tail)
    {
        var lines = new List<string>();
        foreach (var gate in _compatibilityCallSiteGates.Results)
        {
            var letter = (char)('A' + (int)gate.Gate - 1);
            lines.Add($"Gate {letter} — {gate.Gate}: {(gate.Passed ? "PASS" : "FAIL")}");
            lines.Add(gate.Detail);
            lines.Add(string.Empty);
        }

        lines.Add("Step 17 scope: receipt-backed macOS arm64 + architecture-neutral managed files only; x86_64 duplicate managed payload excluded from compatibility prioritization.");
        lines.Add("Evidence: actual Cecil IL operands/PInvoke metadata; no dependency Resolve(), Assembly.Load, game execution, or game-file write.");
        lines.Add("Step 15 orientation presentation quirk remains a known non-blocking cleanup item; Step 17 does not alter the Godot host.");
        lines.Add("Real compatibility rewrite / StS2 execution / FMOD / Spine / Cloud / Workshop: NOT ADVANCED BY STEP 17");
        lines.Add(tail);
        return string.Join("\n", lines);
    }

    private bool RecordManagedPreparationGate(ManagedPreparationGateResult result)
    {
        _managedPreparationGates.Record(result.Gate, result.Passed, result.Detail);
        if (_managedPreparationResultLabel is not null)
        {
            _managedPreparationResultLabel.Text = _managedPreparationGates.Snapshot().Summary;
            _managedPreparationResultLabel.TextColor = result.Passed ? UIColor.Label : UIColor.SystemRed;
        }
        if (_managedPreparationDetailLabel is not null)
            _managedPreparationDetailLabel.Text = FormatManagedPreparationDetail(result.Detail);
        if (!result.Passed && _statusLabel is not null)
        {
            var letter = (char)('A' + (int)result.Gate - 1);
            _statusLabel.Text = $"STEP 16 FAIL at Gate {letter} ({result.Gate}). Stop here; later managed-preparation gates were not run.";
            _statusLabel.TextColor = UIColor.SystemRed;
        }
        return result.Passed;
    }

    private string FormatManagedPreparationDetail(string tail)
    {
        var lines = new List<string>();
        foreach (var gate in _managedPreparationGates.Results)
        {
            var letter = (char)('A' + (int)gate.Gate - 1);
            lines.Add($"Gate {letter} — {gate.Gate}: {(gate.Passed ? "PASS" : "FAIL")}\n{gate.Detail}");
        }

        lines.Add("Step 16 write scope: project-owned fixture copies under launcher-private Step16-ManagedPreparation only.");
        lines.Add("Real StS2 gate: receipt-backed metadata read only; no Assembly.Load, no game execution, no game-file write.");
        lines.Add("Godot/game-runtime/FMOD/Spine/Cloud/Workshop integration: NOT ADVANCED BY STEP 16");
        lines.Add(tail);
        return string.Join("\n\n", lines);
    }
}
