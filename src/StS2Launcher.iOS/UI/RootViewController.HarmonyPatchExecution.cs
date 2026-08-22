using System.Text;
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
        Step27AccessToolsFrameworkPreservation.Activate();
        Step27PatchEngineFrameworkPreservation.Activate();

        content.AddArrangedSubview(Separator());
        content.AddArrangedSubview(Label(
            "Step 27 — Controlled Launcher-Owned Harmony Patch + Unpatch (ordered gates A–Z)",
            UIFont.BoldSystemFontOfSize(25),
            UIColor.Label));

        _controlledHarmonyPatchExecutionButton = SystemButton(
            "Run Step 27 A–Z — Replay Step 26 → Audit/Initialize AccessTools → Register Prefix → Measure Patch Runtime → Initialize HarmonySharedState → Patch → Audit → Invoke → Unpatch → Restore → Final Audit",
            17);
        _controlledHarmonyPatchExecutionButton.TouchUpInside += async (_, _) => await RunControlledHarmonyPatchExecutionAsync();
        content.AddArrangedSubview(_controlledHarmonyPatchExecutionButton);

        _controlledHarmonyPatchExecutionResultLabel = Label(
            "CONTROLLED LAUNCHER-OWNED HARMONY PATCH EXECUTION BOUNDARY: NOT RUN",
            UIFont.BoldSystemFontOfSize(21),
            UIColor.Label);
        content.AddArrangedSubview(_controlledHarmonyPatchExecutionResultLabel);

        _controlledHarmonyPatchExecutionDetailLabel = Label(
            "Gates A–N replay the physically closed Step 26 boundary in one fresh dedicated context through one empty PatchProcessor targeting the Step-26 launcher probe. Gate O keeps the physically passing 0.0.90 runtime-reflection surface for exact AddPrefix/Patch/Unpatch, HarmonyMethod, and AccessTools while retaining the HarmonySharedState → MethodCreator → MonoMod detour chain as Cecil metadata audit only. A synchronous crash checkpoint is flushed before/after every gate and at sensitive O/R/S/T substages. Gate P resolves a separate launcher-owned Step-27 Target(int) + Prefix(int, ref __result) pair and never reflects StS2. Gate Q proves original behavior. Gate R explicitly completes the measured AccessTools runtime-detection/cache initializer. Gate S constructs the bounded annotation-free launcher prefix descriptor without invoking AddPrefix(MethodInfo). Gate T measures the bounded host Reflection.Emit/MethodHandle runtime preflight at T1/T2, exact HarmonySharedState runtime reflection at T3/T4, arms bounded cctor resolver/AssemblyLoad observers at T5a, enters the unchanged explicit HarmonySharedState initialization at T5b/T6, then invokes exactly one public PatchProcessor.Patch() at T7/T8 and validates at T9; the launcher target is still not invoked. Gate U audits bytes/OfflineReady/context before patched execution. Gate V requires both reflection and direct calls to return the prefix-controlled result while the original body is skipped. Gate W removes exactly that prefix by MethodInfo; Gate X audits before restored invocation; Gate Y requires original behavior on both routes; Gate Z performs the final full hash/OfflineReady/context audit. StS2 reflection/patching/invocation, Harmony.Patch/PatchAll, patch classes/categories, transpilers/finalizers, Godot startup, and native game loading remain forbidden.",
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

        if (!CurrentReleasePresentation.BundleIdentityMatchesExpected)
        {
            var mismatch =
                $"Step 27 release identity mismatch. Installed bundle is {CurrentReleasePresentation.DisplayVersion} ({CurrentReleasePresentation.DisplayBuild}); " +
                $"this source candidate requires {CurrentReleasePresentation.ExpectedDisplayVersion} ({CurrentReleasePresentation.ExpectedBuildVersion}). No Step-27 gate was run.";
            WriteStep27CrashCheckpoint("IDENTITY_FAIL", null, mismatch);
            _controlledHarmonyPatchExecutionResultLabel.Text = "CONTROLLED LAUNCHER-OWNED HARMONY PATCH EXECUTION BOUNDARY: RELEASE IDENTITY FAIL";
            _controlledHarmonyPatchExecutionResultLabel.TextColor = UIColor.SystemRed;
            _controlledHarmonyPatchExecutionDetailLabel.Text = mismatch;
            _statusLabel.Text = mismatch;
            _statusLabel.TextColor = UIColor.SystemRed;
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
            WriteStep27CrashCheckpoint("RUN_START", null, "Fresh Step 27 run requested; Gate A has not yet completed.");
            var progress = new InlineProgress<ControlledHarmonyPatchExecutionProgress>(value =>
            {
                var count = value.TotalItems > 0 ? $" ({value.ProcessedItems:N0}/{value.TotalItems:N0})" : string.Empty;
                var detail = $"Gate {Step27GateLabel(value.Gate)} progress{count}: {value.Detail}" +
                    (string.IsNullOrWhiteSpace(value.CurrentPath) ? string.Empty : $"\nCurrent: {value.CurrentPath}");
                WriteStep27CrashCheckpoint("PROGRESS", value.Gate, detail);
                void UpdateUi() => _controlledHarmonyPatchExecutionDetailLabel.Text = FormatControlledHarmonyPatchExecutionDetail(detail);
                if (Foundation.NSThread.IsMain)
                    UpdateUi();
                else
                    InvokeOnMainThread(UpdateUi);
            });

            WriteStep27CrashCheckpoint("START", ControlledHarmonyPatchExecutionGate.InitializationPreflight, _statusLabel.Text ?? string.Empty);
            var gateA = await _controlledHarmonyPatchExecution.RunInitializationPreflightAsync(progress, token);
            if (!RecordControlledHarmonyPatchExecutionGate(gateA)) return;

            _controlledHarmonyPatchExecutionResultLabel.Text = "CONTROLLED LAUNCHER-OWNED HARMONY PATCH EXECUTION BOUNDARY: GATE B RUNNING…";
            _statusLabel.Text = "STEP 27 GATE B — replaying the Step 23 initializer-free private state.";
            WriteStep27CrashCheckpoint("START", ControlledHarmonyPatchExecutionGate.ProvenLoadStateReplay, _statusLabel.Text ?? string.Empty);
            var gateB = await Task.Run(() => _controlledHarmonyPatchExecution.RunProvenLoadStateReplay(), token);
            if (!RecordControlledHarmonyPatchExecutionGate(gateB)) return;

            _controlledHarmonyPatchExecutionResultLabel.Text = "CONTROLLED LAUNCHER-OWNED HARMONY PATCH EXECUTION BOUNDARY: GATE C RUNNING…";
            _statusLabel.Text = "STEP 27 GATE C — replaying the closed Step 24 0Harmony module initializer.";
            WriteStep27CrashCheckpoint("START", ControlledHarmonyPatchExecutionGate.DeferredModuleInitialization, _statusLabel.Text ?? string.Empty);
            var gateC = await Task.Run(() => _controlledHarmonyPatchExecution.RunDeferredModuleInitialization(), token);
            if (!RecordControlledHarmonyPatchExecutionGate(gateC)) return;

            _controlledHarmonyPatchExecutionResultLabel.Text = "CONTROLLED LAUNCHER-OWNED HARMONY PATCH EXECUTION BOUNDARY: GATE D RUNNING…";
            _statusLabel.Text = "STEP 27 GATE D — re-proving the closed Step 24 post-initialization state.";
            WriteStep27CrashCheckpoint("START", ControlledHarmonyPatchExecutionGate.ProvenInitializationAudit, _statusLabel.Text ?? string.Empty);
            var gateD = await _controlledHarmonyPatchExecution.RunProvenInitializationAuditAsync(progress, token);
            if (!RecordControlledHarmonyPatchExecutionGate(gateD)) return;

            _controlledHarmonyPatchExecutionResultLabel.Text = "CONTROLLED LAUNCHER-OWNED HARMONY PATCH EXECUTION BOUNDARY: GATE E RUNNING…";
            _statusLabel.Text = "STEP 27 GATE E — replaying exact Harmony API resolution without type initialization.";
            WriteStep27CrashCheckpoint("START", ControlledHarmonyPatchExecutionGate.HarmonyApiResolution, _statusLabel.Text ?? string.Empty);
            var gateE = await Task.Run(() => _controlledHarmonyPatchExecution.RunHarmonyApiResolution(), token);
            if (!RecordControlledHarmonyPatchExecutionGate(gateE)) return;

            _controlledHarmonyPatchExecutionResultLabel.Text = "CONTROLLED LAUNCHER-OWNED HARMONY PATCH EXECUTION BOUNDARY: GATE F RUNNING…";
            _statusLabel.Text = "STEP 27 GATE F — replaying exact Harmony type initialization.";
            WriteStep27CrashCheckpoint("START", ControlledHarmonyPatchExecutionGate.HarmonyTypeInitialization, _statusLabel.Text ?? string.Empty);
            var gateF = await Task.Run(() => _controlledHarmonyPatchExecution.RunHarmonyTypeInitialization(), token);
            if (!RecordControlledHarmonyPatchExecutionGate(gateF)) return;

            _controlledHarmonyPatchExecutionResultLabel.Text = "CONTROLLED LAUNCHER-OWNED HARMONY PATCH EXECUTION BOUNDARY: GATE G RUNNING…";
            _statusLabel.Text = "STEP 27 GATE G — re-auditing Harmony type-initialization state.";
            WriteStep27CrashCheckpoint("START", ControlledHarmonyPatchExecutionGate.HarmonyTypeInitializationAudit, _statusLabel.Text ?? string.Empty);
            var gateG = await Task.Run(() => _controlledHarmonyPatchExecution.RunHarmonyTypeInitializationAudit(), token);
            if (!RecordControlledHarmonyPatchExecutionGate(gateG)) return;

            _controlledHarmonyPatchExecutionResultLabel.Text = "CONTROLLED LAUNCHER-OWNED HARMONY PATCH EXECUTION BOUNDARY: GATE H RUNNING…";
            _statusLabel.Text = "STEP 27 GATE H — replaying the closed inert Harmony(string) construction boundary.";
            WriteStep27CrashCheckpoint("START", ControlledHarmonyPatchExecutionGate.HarmonyInstanceConstruction, _statusLabel.Text ?? string.Empty);
            var gateH = await Task.Run(() => _controlledHarmonyPatchExecution.RunHarmonyInstanceConstruction(), token);
            if (!RecordControlledHarmonyPatchExecutionGate(gateH)) return;

            _controlledHarmonyPatchExecutionResultLabel.Text = "CONTROLLED LAUNCHER-OWNED HARMONY PATCH EXECUTION BOUNDARY: GATE I RUNNING…";
            _statusLabel.Text = "STEP 27 GATE I — re-proving the closed Step 25 post-construction state.";
            WriteStep27CrashCheckpoint("START", ControlledHarmonyPatchExecutionGate.PostConstructionAudit, _statusLabel.Text ?? string.Empty);
            var gateI = await _controlledHarmonyPatchExecution.RunPostConstructionAuditAsync(progress, token);
            if (!RecordControlledHarmonyPatchExecutionGate(gateI)) return;

            _controlledHarmonyPatchExecutionResultLabel.Text = "CONTROLLED LAUNCHER-OWNED HARMONY PATCH EXECUTION BOUNDARY: GATE J RUNNING…";
            _statusLabel.Text = "STEP 27 GATE J — metadata-auditing and resolving exact CreateProcessor/PatchProcessor API only. No PatchProcessor type initialization or construction.";
            WriteStep27CrashCheckpoint("START", ControlledHarmonyPatchExecutionGate.HarmonyProcessorApiResolution, _statusLabel.Text ?? string.Empty);
            var gateJ = await Task.Run(() => _controlledHarmonyPatchExecution.RunHarmonyProcessorApiResolution(), token);
            if (!RecordControlledHarmonyPatchExecutionGate(gateJ)) return;

            _controlledHarmonyPatchExecutionResultLabel.Text = "CONTROLLED LAUNCHER-OWNED HARMONY PATCH EXECUTION BOUNDARY: GATE K RUNNING…";
            _statusLabel.Text = "STEP 27 GATE K — explicitly completing only the measured PatchProcessor locker type initializer.";
            WriteStep27CrashCheckpoint("START", ControlledHarmonyPatchExecutionGate.PatchProcessorTypeInitialization, _statusLabel.Text ?? string.Empty);
            var gateK = await Task.Run(() => _controlledHarmonyPatchExecution.RunPatchProcessorTypeInitialization(), token);
            if (!RecordControlledHarmonyPatchExecutionGate(gateK)) return;

            _controlledHarmonyPatchExecutionResultLabel.Text = "CONTROLLED LAUNCHER-OWNED HARMONY PATCH EXECUTION BOUNDARY: GATE L RUNNING…";
            _statusLabel.Text = "STEP 27 GATE L — resolving one launcher-owned inert MethodInfo. No StS2 reflection and no method invocation.";
            WriteStep27CrashCheckpoint("START", ControlledHarmonyPatchExecutionGate.LauncherProbeResolution, _statusLabel.Text ?? string.Empty);
            var gateL = await Task.Run(() => _controlledHarmonyPatchExecution.RunLauncherProbeResolution(), token);
            if (!RecordControlledHarmonyPatchExecutionGate(gateL)) return;

            _controlledHarmonyPatchExecutionResultLabel.Text = "CONTROLLED LAUNCHER-OWNED HARMONY PATCH EXECUTION BOUNDARY: GATE M RUNNING…";
            _statusLabel.Text = "STEP 27 GATE M — invoking only Harmony.CreateProcessor(MethodBase). Patch() remains forbidden.";
            WriteStep27CrashCheckpoint("START", ControlledHarmonyPatchExecutionGate.HarmonyProcessorCreation, _statusLabel.Text ?? string.Empty);
            var gateM = await Task.Run(() => _controlledHarmonyPatchExecution.RunHarmonyProcessorCreation(), token);
            if (!RecordControlledHarmonyPatchExecutionGate(gateM)) return;

            _controlledHarmonyPatchExecutionResultLabel.Text = "CONTROLLED LAUNCHER-OWNED HARMONY PATCH EXECUTION BOUNDARY: GATE N RUNNING…";
            _statusLabel.Text = "STEP 27 GATE N — final byte/plan/context/native-isolation audit with Patch() still uninvoked.";
            WriteStep27CrashCheckpoint("START", ControlledHarmonyPatchExecutionGate.PostProcessorAudit, _statusLabel.Text ?? string.Empty);
            var gateN = await _controlledHarmonyPatchExecution.RunPostProcessorAuditAsync(progress, token);
            if (!RecordControlledHarmonyPatchExecutionGate(gateN)) return;

            _controlledHarmonyPatchExecutionResultLabel.Text = "CONTROLLED LAUNCHER-OWNED HARMONY PATCH EXECUTION BOUNDARY: GATE O RUNNING…";
            _statusLabel.Text = "STEP 27 GATE O — admission/resolution only: exact patch APIs + AccessTools runtime reflection on the physically passing 0.0.90 surface; HarmonySharedState/replacement/detour remain Cecil metadata only. No patch-engine runtime reflection here.";
            WriteStep27CrashCheckpoint("START", ControlledHarmonyPatchExecutionGate.HarmonyPatchApiResolution, _statusLabel.Text ?? string.Empty);
            var gateO = await Task.Run(() => _controlledHarmonyPatchExecution.RunHarmonyPatchApiResolution(progress), token);
            if (!RecordControlledHarmonyPatchExecutionGate(gateO)) return;

            _controlledHarmonyPatchExecutionResultLabel.Text = "CONTROLLED LAUNCHER-OWNED HARMONY PATCH EXECUTION BOUNDARY: GATE P RUNNING…";
            _statusLabel.Text = "STEP 27 GATE P — resolving launcher-owned Target(int) + Prefix(int, ref __result). No StS2 reflection and no invocation.";
            WriteStep27CrashCheckpoint("START", ControlledHarmonyPatchExecutionGate.LauncherPatchProbeResolution, _statusLabel.Text ?? string.Empty);
            var gateP = await Task.Run(() => _controlledHarmonyPatchExecution.RunLauncherPatchProbeResolution(), token);
            if (!RecordControlledHarmonyPatchExecutionGate(gateP)) return;

            _controlledHarmonyPatchExecutionResultLabel.Text = "CONTROLLED LAUNCHER-OWNED HARMONY PATCH EXECUTION BOUNDARY: GATE Q RUNNING…";
            _statusLabel.Text = "STEP 27 GATE Q — invoking only the launcher-owned target before patching to establish direct + reflection baseline behavior.";
            WriteStep27CrashCheckpoint("START", ControlledHarmonyPatchExecutionGate.BaselineProbeInvocation, _statusLabel.Text ?? string.Empty);
            var gateQ = await Task.Run(() => _controlledHarmonyPatchExecution.RunBaselineProbeInvocation(), token);
            if (!RecordControlledHarmonyPatchExecutionGate(gateQ)) return;

            _controlledHarmonyPatchExecutionResultLabel.Text = "CONTROLLED LAUNCHER-OWNED HARMONY PATCH EXECUTION BOUNDARY: GATE R RUNNING…";
            _statusLabel.Text = "STEP 27 GATE R — first reflected FrameworkDescription getter invocation, then explicit measured AccessTools type initialization. No HarmonyMethod construction or patching.";
            WriteStep27CrashCheckpoint("START", ControlledHarmonyPatchExecutionGate.AccessToolsTypeInitialization, _statusLabel.Text ?? string.Empty);
            var gateR = await Task.Run(() => _controlledHarmonyPatchExecution.RunAccessToolsTypeInitialization(progress), token);
            if (!RecordControlledHarmonyPatchExecutionGate(gateR)) return;

            _controlledHarmonyPatchExecutionResultLabel.Text = "CONTROLLED LAUNCHER-OWNED HARMONY PATCH EXECUTION BOUNDARY: GATE S RUNNING…";
            _statusLabel.Text = "STEP 27 GATE S — registering one launcher prefix through the bounded iOS descriptor path: exact HarmonyMethod() + method field + PatchProcessor.prefix field. The crashing AddPrefix(MethodInfo)/ImportMethod wrapper is NOT invoked; Patch() is still uninvoked.";
            WriteStep27CrashCheckpoint("START", ControlledHarmonyPatchExecutionGate.PrefixRegistration, _statusLabel.Text ?? string.Empty);
            var gateS = await Task.Run(() => _controlledHarmonyPatchExecution.RunPrefixRegistration(progress), token);
            if (!RecordControlledHarmonyPatchExecutionGate(gateS)) return;

            _controlledHarmonyPatchExecutionResultLabel.Text = "CONTROLLED LAUNCHER-OWNED HARMONY PATCH EXECUTION BOUNDARY: GATE T RUNNING…";
            _statusLabel.Text = "STEP 27 GATE T — PATCH-ENGINE RUNTIME BOUNDARY: replay physically crossed T1–T4, arm bounded cctor resolver/AssemblyLoad observation, enter unchanged HarmonySharedState initialization, then invoke PatchProcessor.Patch() exactly once only if T6 validates. Target is not invoked yet.";
            WriteStep27CrashCheckpoint("START", ControlledHarmonyPatchExecutionGate.PatchEngineExecution, _statusLabel.Text ?? string.Empty);
            var gateT = await Task.Run(() => _controlledHarmonyPatchExecution.RunPatchEngineExecution(progress), token);
            if (!RecordControlledHarmonyPatchExecutionGate(gateT)) return;

            _controlledHarmonyPatchExecutionResultLabel.Text = "CONTROLLED LAUNCHER-OWNED HARMONY PATCH EXECUTION BOUNDARY: GATE U RUNNING…";
            _statusLabel.Text = "STEP 27 GATE U — post-patch hashes/OfflineReady/context audit BEFORE patched target execution.";
            WriteStep27CrashCheckpoint("START", ControlledHarmonyPatchExecutionGate.PostPatchAudit, _statusLabel.Text ?? string.Empty);
            var gateU = await _controlledHarmonyPatchExecution.RunPostPatchAuditAsync(progress, token);
            if (!RecordControlledHarmonyPatchExecutionGate(gateU)) return;

            _controlledHarmonyPatchExecutionResultLabel.Text = "CONTROLLED LAUNCHER-OWNED HARMONY PATCH EXECUTION BOUNDARY: GATE V RUNNING…";
            _statusLabel.Text = "STEP 27 GATE V — invoking patched launcher target through reflection and direct call; exact prefix must replace result and skip original.";
            WriteStep27CrashCheckpoint("START", ControlledHarmonyPatchExecutionGate.PatchedProbeInvocation, _statusLabel.Text ?? string.Empty);
            var gateV = await Task.Run(() => _controlledHarmonyPatchExecution.RunPatchedProbeInvocation(), token);
            if (!RecordControlledHarmonyPatchExecutionGate(gateV)) return;

            _controlledHarmonyPatchExecutionResultLabel.Text = "CONTROLLED LAUNCHER-OWNED HARMONY PATCH EXECUTION BOUNDARY: GATE W RUNNING…";
            _statusLabel.Text = "STEP 27 GATE W — removing exactly the launcher-owned prefix via PatchProcessor.Unpatch(MethodInfo).";
            WriteStep27CrashCheckpoint("START", ControlledHarmonyPatchExecutionGate.ExactPrefixUnpatch, _statusLabel.Text ?? string.Empty);
            var gateW = await Task.Run(() => _controlledHarmonyPatchExecution.RunExactPrefixUnpatch(), token);
            if (!RecordControlledHarmonyPatchExecutionGate(gateW)) return;

            _controlledHarmonyPatchExecutionResultLabel.Text = "CONTROLLED LAUNCHER-OWNED HARMONY PATCH EXECUTION BOUNDARY: GATE X RUNNING…";
            _statusLabel.Text = "STEP 27 GATE X — auditing post-unpatch context/native/hash state before restored invocation.";
            WriteStep27CrashCheckpoint("START", ControlledHarmonyPatchExecutionGate.PostUnpatchAudit, _statusLabel.Text ?? string.Empty);
            var gateX = await Task.Run(() => _controlledHarmonyPatchExecution.RunPostUnpatchAudit(), token);
            if (!RecordControlledHarmonyPatchExecutionGate(gateX)) return;

            _controlledHarmonyPatchExecutionResultLabel.Text = "CONTROLLED LAUNCHER-OWNED HARMONY PATCH EXECUTION BOUNDARY: GATE Y RUNNING…";
            _statusLabel.Text = "STEP 27 GATE Y — invoking launcher target through reflection and direct call; exact original value+1 behavior must be restored.";
            WriteStep27CrashCheckpoint("START", ControlledHarmonyPatchExecutionGate.RestoredProbeInvocation, _statusLabel.Text ?? string.Empty);
            var gateY = await Task.Run(() => _controlledHarmonyPatchExecution.RunRestoredProbeInvocation(), token);
            if (!RecordControlledHarmonyPatchExecutionGate(gateY)) return;

            _controlledHarmonyPatchExecutionResultLabel.Text = "CONTROLLED LAUNCHER-OWNED HARMONY PATCH EXECUTION BOUNDARY: GATE Z RUNNING…";
            _statusLabel.Text = "STEP 27 GATE Z — final plan/prepared/live/OfflineReady/context/native-isolation audit after complete launcher-only patch/unpatch cycle.";
            WriteStep27CrashCheckpoint("START", ControlledHarmonyPatchExecutionGate.FinalIsolationAudit, _statusLabel.Text ?? string.Empty);
            var gateZ = await _controlledHarmonyPatchExecution.RunFinalIsolationAuditAsync(progress, token);
            if (!RecordControlledHarmonyPatchExecutionGate(gateZ)) return;

            var snapshot = _controlledHarmonyPatchExecutionGates.Snapshot();
            _controlledHarmonyPatchExecutionResultLabel.Text = snapshot.Summary;
            _controlledHarmonyPatchExecutionResultLabel.TextColor = UIColor.Label;
            _controlledHarmonyPatchExecutionDetailLabel.Text = FormatControlledHarmonyPatchExecutionDetail(
                "All twenty-six Step 27 gates passed. The physically closed Step 26 state was reproduced; exact patch APIs and a launcher-owned prefix were admitted; one real PatchProcessor.Patch() completed; the launcher target returned the deterministic patched result through reflection and direct calls while the original body was skipped; the exact prefix was then removed and original behavior was restored through both routes; final hashes, OfflineReady, context membership, and native/resolver isolation remained intact. Run OfflineReady + Foundation 5/5 to close Step 27.");
            _statusLabel.Text = "PASS: STEP 27 CONTROLLED LAUNCHER-OWNED HARMONY PATCH EXECUTION — 26/26. Run OfflineReady + Foundation 5/5 for closure.";
            _statusLabel.TextColor = UIColor.Label;
            WriteStep27CrashCheckpoint("RUN_COMPLETE", ControlledHarmonyPatchExecutionGate.FinalIsolationAudit, "All 26 gates passed; run OfflineReady + Foundation 5/5 for closure.");
        }
        catch (OperationCanceledException)
        {
            _controlledHarmonyPatchExecutionResultLabel.Text = "CONTROLLED LAUNCHER-OWNED HARMONY PATCH EXECUTION BOUNDARY: CANCELLED";
            _controlledHarmonyPatchExecutionResultLabel.TextColor = UIColor.SecondaryLabel;
            _controlledHarmonyPatchExecutionDetailLabel.Text = FormatControlledHarmonyPatchExecutionDetail(
                "Step 27 was cancelled. If Gate B had started, force-quit before retrying so Gate A begins from a fresh process.");
            _statusLabel.Text = "STEP 27 CANCELLED — no later gate is considered proven.";
            _statusLabel.TextColor = UIColor.SecondaryLabel;
            WriteStep27CrashCheckpoint("CANCELLED", null, "Step 27 cancellation was caught. If Gate B had started, force-quit before retrying.");
        }
        catch (Exception ex)
        {
            _controlledHarmonyPatchExecutionResultLabel.Text = "CONTROLLED LAUNCHER-OWNED HARMONY PATCH EXECUTION BOUNDARY: EXCEPTION";
            _controlledHarmonyPatchExecutionResultLabel.TextColor = UIColor.SystemRed;
            _controlledHarmonyPatchExecutionDetailLabel.Text = FormatControlledHarmonyPatchExecutionDetail($"Unhandled Step 27 exception: {ex.GetType().Name}: {ex.Message}");
            _statusLabel.Text = "STEP 27 FAIL — stop at the first failing gate. Force-quit before another attempt if Gate B had started.";
            _statusLabel.TextColor = UIColor.SystemRed;
            WriteStep27CrashCheckpoint("EXCEPTION", null, $"Unhandled Step 27 exception: {ex.GetType().Name}: {ex.Message}");
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
        WriteStep27CrashCheckpoint(result.Passed ? "PASS" : "FAIL", result.Gate, result.Detail);
        if (_controlledHarmonyPatchExecutionResultLabel is not null)
        {
            _controlledHarmonyPatchExecutionResultLabel.Text = _controlledHarmonyPatchExecutionGates.Snapshot().Summary;
            _controlledHarmonyPatchExecutionResultLabel.TextColor = result.Passed ? UIColor.Label : UIColor.SystemRed;
        }
        if (_controlledHarmonyPatchExecutionDetailLabel is not null)
            _controlledHarmonyPatchExecutionDetailLabel.Text = FormatControlledHarmonyPatchExecutionDetail(result.Detail);
        if (!result.Passed && _statusLabel is not null)
        {
            var letter = Step27GateLabel(result.Gate);
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
            var letter = Step27GateLabel(gate.Gate);
            lines.Add($"Gate {letter} — {gate.Gate}: {(gate.Passed ? "PASS" : "FAIL")}");
            lines.Add(gate.Detail);
            lines.Add(string.Empty);
        }

        lines.Add("Step 27 prerequisite: physical Step 26.0 / 0.0.83 is closed — Gates A–N PASS, OfflineReady PASS, Foundation 5/5 PASS.");
        lines.Add("Gates A–N replay the closed Step 26 chain in the exact Step 27 private context, preserving the proven System.Collections.Concurrent root and Step-25 constructor framework-preservation anchor with TrimMode=full and MtouchInterpreter=-all.");
        lines.Add("Gate O metadata-audits exact PatchProcessor.AddPrefix(MethodInfo), Patch(), Unpatch(MethodInfo), both HarmonyMethod constructors/fields, the physically traversed AccessTools initializer, and the exact HarmonySharedState/GetOrCreateSharedStateType -> MethodCreatorConfig.Prepare -> PatchFunctions.UpdateWrapper -> PatchTools.DetourMethod -> UpdatePatchInfo chain. Its runtime reflection is deliberately restored to the physically passing 0.0.90 PatchProcessor/HarmonyMethod/AccessTools surface; bounded Reflection.Emit/RuntimeMethodHandle preflight and HarmonySharedState runtime reflection are deferred to Gate T so their loader effects are measured.");
        lines.Add("Gate P resolves only launcher-owned HarmonyPatchProbe.Target(int) + Prefix(int, ref int __result), including exact parameter names required by Harmony. No StS2 member is reflected.");
        lines.Add("Gate Q invokes the launcher target through direct + reflection routes before patching and requires original value+1 behavior with prefix count zero.");
        lines.Add("Gate R first invokes the exact preserved RuntimeInformation.FrameworkDescription getter through PropertyInfo.GetValue, then explicitly completes only the Gate-O-measured HarmonyLib.AccessTools runtime-detection/cache type initializer with RunClassConstructor and verifies the exact measured state. It still constructs no HarmonyMethod and applies no patch.");
        lines.Add("Gate S uses the exact parameterless HarmonyMethod() constructor, verifies priority=-1 and method=null, assigns only the exact launcher Prefix MethodInfo to HarmonyMethod.method, then assigns only that descriptor to PatchProcessor.prefix. This is admitted only because the launcher prefix carries zero Harmony annotations and the exact six-instruction AddPrefix wrapper is still metadata-audited. AddPrefix(MethodInfo), HarmonyMethod(MethodInfo), ImportMethod, and Patch() are not invoked.");
        lines.Add("Gate T decomposes the patch-engine runtime boundary: T1/T2 measure the bounded Reflection.Emit/RuntimeMethodHandle host preflight with private membership unchanged; T3/T4 resolve the exact HarmonySharedState runtime Type/.cctor/version fields and record resolver/load deltas without initialization; T5a arms bounded output-only dedicated-ALC and process AssemblyLoad observers, T5b enters the unchanged RunClassConstructor, and T6 validates only if the cctor returns; T7/T8 invoke exactly one public PatchProcessor.Patch(); T9 validates replacement/isolation. The launcher target is not invoked until Gate V.");
        lines.Add("Gate U re-hashes plan/prepared/live bytes, re-proves OfflineReady, and audits context/native/resolver state before patched execution.");
        lines.Add("Gate V invokes the patched launcher target through reflection and direct calls; both must return 1041, increment the prefix, and skip the original target body.");
        lines.Add("Gate W removes exactly the launcher prefix via PatchProcessor.Unpatch(MethodInfo). Gate X audits before restored invocation. Gate Y invokes the launcher target through both routes and requires restored value+1 behavior with no additional prefix calls. Gate Z performs the final full byte/OfflineReady/context/native audit.");
        lines.Add("Still forbidden: Harmony.Patch/PatchAll/PatchCategory/PatchClassProcessor; postfix/transpiler/finalizer/inner patch registration; StS2 entry-point/type/member reflection, patching, or invocation; broad Activator/CreateInstance; Godot/game startup; native game-library loading; mutation of trusted live/prepared bytes.");
        lines.Add("Fresh-process rule: once Gate B has started, the Step 27 sts2/Harmony load context is process-resident for safety accounting; force-quit before any retry, even if the run fails before patching. If Gate T or later runs, additionally assume launcher probe patch state may remain process-resident until force-quit.");
        lines.Add("Crash telemetry: Documents/StS2Launcher/Reports/Step27-CrashCheckpoint.txt is synchronously overwritten at RUN_START, every gate START/PASS/FAIL, normal progress, and sensitive O/R/S/T substages. If iOS terminates the process without a managed exception/report, preserve that checkpoint before the next Step 27 attempt.");
        lines.Add(tail);
        return string.Join("\n", lines);
    }
    private static string Step27GateLabel(ControlledHarmonyPatchExecutionGate gate)
        => ((char)('A' + (int)gate - 1)).ToString();

    private void WriteStep27CrashCheckpoint(
        string phase,
        ControlledHarmonyPatchExecutionGate? gate,
        string detail)
    {
        try
        {
            Directory.CreateDirectory(_deviceTestReportWriter.ReportsRoot);
            var path = Path.Combine(_deviceTestReportWriter.ReportsRoot, "Step27-CrashCheckpoint.txt");
            var temporaryPath = path + ".tmp";
            var gateText = gate is null ? "<none>" : $"{Step27GateLabel(gate.Value)} — {gate.Value}";
            var normalizedDetail = (detail ?? string.Empty).Replace("\r\n", "\n", StringComparison.Ordinal).Trim();
            var content =
                "StS2 Launcher — Step 27 crash checkpoint\n" +
                "Output-only diagnostic; never consumed as trusted runtime input.\n" +
                $"Generated UTC: {DateTimeOffset.UtcNow:O}\n" +
                $"Process ID: {Environment.ProcessId}\n" +
                $"App version: {CurrentReleasePresentation.DisplayVersion} ({CurrentReleasePresentation.DisplayBuild})\n" +
                $"Expected source version: {CurrentReleasePresentation.ExpectedDisplayVersion} ({CurrentReleasePresentation.ExpectedBuildVersion})\n" +
                $"Candidate: {CurrentReleasePresentation.StepTitle}\n" +
                $"Gate S implementation: {CurrentReleasePresentation.GateSImplementationMarker}\n" +
                $"Gate T implementation: {CurrentReleasePresentation.GateTImplementationMarker}\n" +
                $"Phase: {phase}\n" +
                $"Gate: {gateText}\n" +
                "Detail:\n" + normalizedDetail + "\n";

            using (var stream = new FileStream(temporaryPath, FileMode.Create, FileAccess.Write, FileShare.Read))
            using (var writer = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)))
            {
                writer.Write(content);
                writer.Flush();
                stream.Flush(flushToDisk: true);
            }
            File.Move(temporaryPath, path, overwrite: true);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Step 27 crash-checkpoint write failed: {ex.GetType().Name}: {ex.Message}");
        }
    }

    private sealed class InlineProgress<T>(Action<T> callback) : IProgress<T>
    {
        private readonly Action<T> _callback = callback ?? throw new ArgumentNullException(nameof(callback));
        public void Report(T value) => _callback(value);
    }

}
