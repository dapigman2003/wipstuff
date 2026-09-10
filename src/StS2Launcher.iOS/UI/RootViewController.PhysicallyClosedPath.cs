using StS2Launcher.Core;
using StS2Launcher.iOS.Platform;
using UIKit;

namespace StS2Launcher.iOS;

public sealed partial class RootViewController
{
    private UIButton? _physicallyClosedPathButton;
    private UILabel? _physicallyClosedPathResultLabel;
    private UILabel? _physicallyClosedPathDetailLabel;
    private bool _physicallyClosedPathStarted;
    private readonly List<string> _physicallyClosedPathCompleted = new();

    private void AddPhysicallyClosedPathControls(UIStackView content)
    {
        content.AddArrangedSubview(Label(
            "Physically Closed Path — one-button same-process reproof through Step 52",
            UIFont.BoldSystemFontOfSize(18),
            UIColor.Label));

        _physicallyClosedPathButton = SystemButton(
            "Run Physically Closed Path — Step 15 A–C → 35–37 → SKIP 38 → 39–52",
            16);
        _physicallyClosedPathButton.TouchUpInside += async (_, _) => await RunPhysicallyClosedPathThroughStep52Async();
        content.AddArrangedSubview(_physicallyClosedPathButton);

        _physicallyClosedPathResultLabel = Label(
            "PHYSICALLY CLOSED PATH: READY — FRESH PROCESS REQUIRED",
            UIFont.BoldSystemFontOfSize(15),
            UIColor.SecondaryLabel);
        content.AddArrangedSubview(_physicallyClosedPathResultLabel);

        _physicallyClosedPathDetailLabel = Label(
            "Convenience runner only. It calls the existing physically closed step implementations in their proven order, preserves every normal per-step report/checkpoint, verifies exact closure after each return, explicitly skips Step 38, and stops at the first failure. Step 15 Gate D is not part of this same-process execution prerequisite. Once armed, never retry this runner or any failed one-shot rung in-process; relaunch instead. Step 53 remains unopened.",
            UIFont.SystemFontOfSize(13),
            UIColor.SecondaryLabel);
        content.AddArrangedSubview(_physicallyClosedPathDetailLabel);
    }

    private async Task RunPhysicallyClosedPathThroughStep52Async()
    {
        if (_physicallyClosedPathButton is null ||
            _physicallyClosedPathResultLabel is null ||
            _physicallyClosedPathDetailLabel is null ||
            _statusLabel is null)
            return;

        if (_physicallyClosedPathStarted)
        {
            SetPhysicallyClosedPathUi(
                "PHYSICALLY CLOSED PATH: ONE-SHOT ALREADY ARMED",
                UIColor.SystemOrange,
                "This convenience runner was already armed in this process. Preserve the existing step reports and force-quit/relaunch before another attempt.");
            await WritePhysicallyClosedPathReportAsync();
            return;
        }

        if (!IsFreshProcessForPhysicallyClosedPath())
        {
            SetPhysicallyClosedPathUi(
                "PHYSICALLY CLOSED PATH: FRESH PROCESS REQUIRED",
                UIColor.SystemOrange,
                "Process-global Godot or numbered-step evidence already exists. This runner will not guess which one-shot boundaries are still safe. Force-quit/relaunch, then use this button before manually running Step 15 or any Step 35+ gate.");
            await WritePhysicallyClosedPathReportAsync();
            return;
        }

        _physicallyClosedPathStarted = true;
        _physicallyClosedPathButton.Enabled = false;
        DisableClosedPhysicalPathManualControls();
        _physicallyClosedPathCompleted.Clear();
        SetPhysicallyClosedPathUi(
            "PHYSICALLY CLOSED PATH: RUNNING",
            UIColor.Label,
            "Fresh-process authority accepted. Starting Step 15 Gates A–C; Step 15 Gate D will intentionally remain unrun.");
        await WritePhysicallyClosedPathReportAsync();

        try
        {
            await RunClosedPhysicalStageAsync(
                "Step 15 A–C",
                RunGodotFoundationGatesABCAsync,
                IsStep15ABCClosedForFastPath);

            await RunClosedPhysicalStageAsync(
                "Step 35.0.32 MODEL-BOOTSTRAP",
                () => RunTransformedRealStS2VeryEarlyInitializationAsync(Step35DiagnosticMode.GodotCoreModelBootstrapCompatibility),
                () => _transformedRealStS2VeryEarlyInitializationGates.Snapshot().Passed &&
                      _transformedRealStS2VeryEarlyInitialization.EssentialCompatibilityAuthorityPassed &&
                      _transformedRealStS2VeryEarlyInitialization.DiagnosticMode == Step35DiagnosticMode.GodotCoreModelBootstrapCompatibility);

            await RunClosedPhysicalStageAsync(
                "Step 36.0.5",
                RunTransformedRealStS2EssentialInitializationAsync,
                () => _step36Gates.Snapshot().Passed && _transformedRealStS2VeryEarlyInitialization.ExactStep36ClosurePassed);

            await RunClosedPhysicalStageAsync(
                "Step 37.0.1",
                RunTransformedRealStS2GameSceneAdmissionAsync,
                () => _step37Gates.Snapshot().Passed && _transformedRealStS2VeryEarlyInitialization.ExactStep37ClosurePassed);

            _physicallyClosedPathCompleted.Add("Step 38 SKIPPED by contract");
            SetPhysicallyClosedPathUi(
                "PHYSICALLY CLOSED PATH: STEP 38 SKIPPED",
                UIColor.Label,
                "Step 37 closed. Step 38 is intentionally NOT invoked in this process; proceeding directly to the physically closed Step 39 SceneTree admission path.");
            await WritePhysicallyClosedPathReportAsync();

            await RunClosedPhysicalStageAsync(
                "Step 39",
                RunTransformedRealStS2SceneTreeAdmissionAsync,
                () => _step39Gates.Snapshot().Passed && _transformedRealStS2VeryEarlyInitialization.ExactStep39ClosurePassed);

            await RunClosedPhysicalStageAsync(
                "Step 40.1",
                RunTransformedRealStS2RenderPulseAsync,
                () => _step40Gates.Snapshot().Passed && _transformedRealStS2VeryEarlyInitialization.ExactStep40ClosurePassed);

            await RunClosedPhysicalStageAsync(
                "Step 41",
                RunTransformedRealStS2GameStartupFrontierAsync,
                () => _step41Gates.Snapshot().Passed && _transformedRealStS2VeryEarlyInitialization.ExactStep41ClosurePassed);

            await RunClosedPhysicalStageAsync(
                "Step 42",
                RunTransformedRealStS2GameStartupInitPoolsAsync,
                () => _step42Gates.Snapshot().Passed && _transformedRealStS2VeryEarlyInitialization.ExactStep42ClosurePassed);

            await RunClosedPhysicalStageAsync("Step 43", RunStep43StartupLadderAsync,
                () => _step43Gates.Snapshot().Passed && _transformedRealStS2VeryEarlyInitialization.ExactStep43ClosurePassed);
            await RunClosedPhysicalStageAsync("Step 44", RunStep44StartupLadderAsync,
                () => _step44Gates.Snapshot().Passed && _transformedRealStS2VeryEarlyInitialization.ExactStep44ClosurePassed);
            await RunClosedPhysicalStageAsync("Step 45", RunStep45StartupLadderAsync,
                () => _step45Gates.Snapshot().Passed && _transformedRealStS2VeryEarlyInitialization.ExactStep45ClosurePassed);
            await RunClosedPhysicalStageAsync("Step 46", RunStep46StartupLadderAsync,
                () => _step46Gates.Snapshot().Passed && _transformedRealStS2VeryEarlyInitialization.ExactStep46ClosurePassed);
            await RunClosedPhysicalStageAsync("Step 47", RunStep47StartupLadderAsync,
                () => _step47Gates.Snapshot().Passed && _transformedRealStS2VeryEarlyInitialization.ExactStep47ClosurePassed);
            await RunClosedPhysicalStageAsync("Step 48", RunStep48StartupLadderAsync,
                () => _step48Gates.Snapshot().Passed && _transformedRealStS2VeryEarlyInitialization.ExactStep48ClosurePassed);
            await RunClosedPhysicalStageAsync("Step 49", RunStep49StartupLadderAsync,
                () => _step49Gates.Snapshot().Passed && _transformedRealStS2VeryEarlyInitialization.ExactStep49ClosurePassed);
            await RunClosedPhysicalStageAsync("Step 50", RunStep50StartupLadderAsync,
                () => _step50Gates.Snapshot().Passed && _transformedRealStS2VeryEarlyInitialization.ExactStep50ClosurePassed);
            await RunClosedPhysicalStageAsync("Step 51", RunStep51StartupLadderAsync,
                () => _step51Gates.Snapshot().Passed && _transformedRealStS2VeryEarlyInitialization.ExactStep51ClosurePassed);
            await RunClosedPhysicalStageAsync("Step 52", RunStep52StartupLadderAsync,
                () => _step52Gates.Snapshot().Passed && _transformedRealStS2VeryEarlyInitialization.ExactStep52ClosurePassed);

            if (GodotStep15NativeBridge.IsRenderingActive)
                throw new InvalidOperationException("Step 52 returned 4/4 but rendering is still active; physically closed path requires the final renderer state to be frozen.");

            SetPhysicallyClosedPathUi(
                "PHYSICALLY CLOSED PATH PASS — THROUGH STEP 52",
                UIColor.Label,
                "All physically closed same-process gates returned their exact pass/closure authority. Step 38 was skipped by contract, Step 15 Gate D was not run, rendering is frozen after Step 52, and Step 53 remains unopened.");
            _statusLabel.Text = "PASS: PHYSICALLY CLOSED PATH THROUGH STEP 52. Renderer is frozen; current frontier remains after Step 52.";
            _statusLabel.TextColor = UIColor.Label;
        }
        catch (Exception ex)
        {
            SetPhysicallyClosedPathUi(
                "PHYSICALLY CLOSED PATH STOPPED — RELAUNCH REQUIRED",
                UIColor.SystemRed,
                $"Stopped after '{LastClosedPhysicalStage()}'. {ex.GetType().Name}: {ex.Message} No later stage was invoked. Preserve the failing step's normal report/checkpoint and force-quit/relaunch before another attempt.");
            _statusLabel.Text = "PHYSICALLY CLOSED PATH STOPPED at first non-closed stage. Later stages were not invoked; relaunch before retry.";
            _statusLabel.TextColor = UIColor.SystemRed;
        }
        finally
        {
            await WritePhysicallyClosedPathReportAsync();
        }
    }

    private async Task RunClosedPhysicalStageAsync(string label, Func<Task> action, Func<bool> passed)
    {
        SetPhysicallyClosedPathUi(
            $"PHYSICALLY CLOSED PATH: RUNNING {label}",
            UIColor.Label,
            $"Last completed: {LastClosedPhysicalStage()}. Running the existing {label} implementation now; later stages remain locked until its exact physical-closure predicate is re-established.");
        await WritePhysicallyClosedPathReportAsync();

        await action();
        // Individual step finalizers may update their own button state. While the
        // convenience runner owns the process, keep every manual 15/35-52 control
        // (and the explicitly forbidden Step 38 control) disabled to prevent races.
        DisableClosedPhysicalPathManualControls();

        if (!passed())
            throw new InvalidOperationException($"{label} returned without its exact physically closed pass/closure predicate. The convenience path will not advance.");

        _physicallyClosedPathCompleted.Add(label);
        SetPhysicallyClosedPathUi(
            $"PHYSICALLY CLOSED PATH: {label} PASS",
            UIColor.Label,
            $"Last completed: {label}. Exact pass/closure predicate re-established; advancing to the next already-closed stage.");
        await WritePhysicallyClosedPathReportAsync();
    }

    private void DisableClosedPhysicalPathManualControls()
    {
        if (_godotFoundationStartButton is not null) _godotFoundationStartButton.Enabled = false;
        if (_godotFoundationGateDButton is not null) _godotFoundationGateDButton.Enabled = false;
        if (_step35GodotModelBootstrapCompatibilityButton is not null) _step35GodotModelBootstrapCompatibilityButton.Enabled = false;
        if (_step36EssentialButton is not null) _step36EssentialButton.Enabled = false;
        if (_step37GameSceneButton is not null) _step37GameSceneButton.Enabled = false;
        if (_step38LifecycleButton is not null) _step38LifecycleButton.Enabled = false;
        if (_step39SceneTreeButton is not null) _step39SceneTreeButton.Enabled = false;
        if (_step40RenderPulseButton is not null) _step40RenderPulseButton.Enabled = false;
        if (_step41GameStartupFrontierButton is not null) _step41GameStartupFrontierButton.Enabled = false;
        if (_step42InitPoolsButton is not null) _step42InitPoolsButton.Enabled = false;
        if (_step43Button is not null) _step43Button.Enabled = false;
        if (_step44Button is not null) _step44Button.Enabled = false;
        if (_step45Button is not null) _step45Button.Enabled = false;
        if (_step46Button is not null) _step46Button.Enabled = false;
        if (_step47Button is not null) _step47Button.Enabled = false;
        if (_step48Button is not null) _step48Button.Enabled = false;
        if (_step49Button is not null) _step49Button.Enabled = false;
        if (_step50Button is not null) _step50Button.Enabled = false;
        if (_step51Button is not null) _step51Button.Enabled = false;
        if (_step52Button is not null) _step52Button.Enabled = false;
    }

    private bool IsStep15ABCClosedForFastPath()
    {
        var snapshot = _godotFoundationGates.Snapshot();
        return _godotSessionStarted &&
               GodotStep15NativeBridge.IsEngineStarted &&
               GodotStep15NativeBridge.IsSetupFinished &&
               snapshot.FirstFailingGate is null &&
               snapshot.Results.Count == 3 &&
               snapshot.Results.All(result => result.Passed);
    }

    private bool IsFreshProcessForPhysicallyClosedPath()
        => !_godotSessionStarted &&
           !_godotProcessRequiresRestart &&
           _godotFoundationGates.Results.Count == 0 &&
           _transformedRealStS2VeryEarlyInitializationGates.Results.Count == 0 &&
           _step36Gates.Results.Count == 0 &&
           _step37Gates.Results.Count == 0 &&
           _step39Gates.Results.Count == 0 &&
           _step40Gates.Results.Count == 0 &&
           _step41Gates.Results.Count == 0 &&
           _step42Gates.Results.Count == 0 &&
           _step43Gates.Results.Count == 0 &&
           _step44Gates.Results.Count == 0 &&
           _step45Gates.Results.Count == 0 &&
           _step46Gates.Results.Count == 0 &&
           _step47Gates.Results.Count == 0 &&
           _step48Gates.Results.Count == 0 &&
           _step49Gates.Results.Count == 0 &&
           _step50Gates.Results.Count == 0 &&
           _step51Gates.Results.Count == 0 &&
           _step52Gates.Results.Count == 0 &&
           !_transformedRealStS2VeryEarlyInitialization.EssentialCompatibilityAuthorityPassed &&
           !_transformedRealStS2VeryEarlyInitialization.ExactStep36ClosurePassed &&
           !_transformedRealStS2VeryEarlyInitialization.ExactStep37ClosurePassed &&
           !_transformedRealStS2VeryEarlyInitialization.ExactStep39ClosurePassed &&
           !_transformedRealStS2VeryEarlyInitialization.ExactStep40ClosurePassed &&
           !_transformedRealStS2VeryEarlyInitialization.ExactStep41ClosurePassed &&
           !_transformedRealStS2VeryEarlyInitialization.ExactStep42ClosurePassed &&
           !_transformedRealStS2VeryEarlyInitialization.ExactStep43ClosurePassed &&
           !_transformedRealStS2VeryEarlyInitialization.ExactStep44ClosurePassed &&
           !_transformedRealStS2VeryEarlyInitialization.ExactStep45ClosurePassed &&
           !_transformedRealStS2VeryEarlyInitialization.ExactStep46ClosurePassed &&
           !_transformedRealStS2VeryEarlyInitialization.ExactStep47ClosurePassed &&
           !_transformedRealStS2VeryEarlyInitialization.ExactStep48ClosurePassed &&
           !_transformedRealStS2VeryEarlyInitialization.ExactStep49ClosurePassed &&
           !_transformedRealStS2VeryEarlyInitialization.ExactStep50ClosurePassed &&
           !_transformedRealStS2VeryEarlyInitialization.ExactStep51ClosurePassed &&
           !_transformedRealStS2VeryEarlyInitialization.ExactStep52ClosurePassed;

    private string LastClosedPhysicalStage()
        => _physicallyClosedPathCompleted.Count == 0 ? "none" : _physicallyClosedPathCompleted[^1];

    private void SetPhysicallyClosedPathUi(string result, UIColor color, string detail)
    {
        if (_physicallyClosedPathResultLabel is not null)
        {
            _physicallyClosedPathResultLabel.Text = result;
            _physicallyClosedPathResultLabel.TextColor = color;
        }
        if (_physicallyClosedPathDetailLabel is not null)
            _physicallyClosedPathDetailLabel.Text = detail;
    }

    private Task WritePhysicallyClosedPathReportAsync()
        => WriteDeviceTestReportFromLabelsAsync(
            "PhysicallyClosedPath-ToStep52.txt",
            "StS2 Launcher — Physically Closed Same-Process Path Through Step 52",
            _physicallyClosedPathResultLabel,
            _physicallyClosedPathDetailLabel,
            CancellationToken.None);
}
