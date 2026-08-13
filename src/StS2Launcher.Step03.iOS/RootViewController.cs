using StS2Launcher.Core;
using UIKit;

namespace StS2Launcher.Step03.iOS;

public sealed class RootViewController : UIViewController
{
    private readonly LauncherController _controller = new();

    private UILabel? _coreLinkLabel;
    private UILabel? _selfTestLabel;
    private UILabel? _stateTitle;
    private UILabel? _stateDetail;
    private UILabel? _stateCounter;
    private UILabel? _statusLabel;
    private UILabel? _lifecycleLabel;
    private UIProgressView? _progress;
    private UIButton? _primaryButton;

    public override void ViewDidLoad()
    {
        base.ViewDidLoad();

        View!.BackgroundColor = UIColor.SystemBackground;

        var appTitle = Label(
            "StS2 Launcher",
            UIFont.BoldSystemFontOfSize(30),
            UIColor.Label);

        var step = Label(
            "STEP 03 — CORE STATE MACHINE",
            UIFont.BoldSystemFontOfSize(15),
            UIColor.SecondaryLabel);

        var version = Label(
            "Version 0.0.4",
            UIFont.SystemFontOfSize(14),
            UIColor.SecondaryLabel);

        _coreLinkLabel = Label(
            "CORE LINK: checking…",
            UIFont.BoldSystemFontOfSize(15),
            UIColor.Label);

        _selfTestLabel = Label(
            "CORE SELF-TEST: NOT RUN",
            UIFont.SystemFontOfSize(14),
            UIColor.SecondaryLabel);

        _stateCounter = Label(
            "",
            UIFont.MonospacedDigitSystemFontOfSize(13, UIFontWeight.Regular),
            UIColor.SecondaryLabel);

        _stateTitle = Label(
            "",
            UIFont.BoldSystemFontOfSize(24),
            UIColor.Label);

        _stateDetail = Label(
            "",
            UIFont.SystemFontOfSize(16),
            UIColor.SecondaryLabel);

        _progress = new UIProgressView(UIProgressViewStyle.Default)
        {
            TranslatesAutoresizingMaskIntoConstraints = false,
            Progress = 0
        };

        _primaryButton = SystemButton("Mock action", 17);
        _primaryButton.TouchUpInside += (_, _) =>
        {
            _statusLabel!.Text = _controller.DescribePrimaryAction();
        };

        var nextStateButton = SystemButton("Next Demo State", 18);
        nextStateButton.TouchUpInside += (_, _) =>
        {
            RenderSnapshot(_controller.NextDemoState());
        };

        var resetButton = SystemButton("Reset Demo", 16);
        resetButton.TouchUpInside += (_, _) =>
        {
            RenderSnapshot(_controller.Reset());
            _statusLabel!.Text = "PASS: Core reset returned to SignedOut.";
        };

        var selfTestButton = SystemButton("Run Core Self-Test", 17);
        selfTestButton.TouchUpInside += (_, _) => RunCoreSelfTest();

        _statusLabel = Label(
            "Status: UIKit loaded; checking Core assembly.",
            UIFont.SystemFontOfSize(14),
            UIColor.Label);

        _lifecycleLabel = Label(
            "Lifecycle: Starting",
            UIFont.SystemFontOfSize(13),
            UIColor.SecondaryLabel);

        var stateCard = new UIView
        {
            TranslatesAutoresizingMaskIntoConstraints = false,
            BackgroundColor = UIColor.SecondarySystemBackground
        };
        stateCard.Layer.CornerRadius = 16;

        foreach (var control in new UIView[]
                 {
                     _stateCounter, _stateTitle, _stateDetail, _progress,
                     _primaryButton
                 })
        {
            stateCard.AddSubview(control);
        }

        var headerStack = new UIStackView(
            [appTitle, step, version, _coreLinkLabel, _selfTestLabel])
        {
            TranslatesAutoresizingMaskIntoConstraints = false,
            Axis = UILayoutConstraintAxis.Vertical,
            Alignment = UIStackViewAlignment.Fill,
            Distribution = UIStackViewDistribution.Fill,
            Spacing = 5
        };

        var controlsStack = new UIStackView(
            [nextStateButton, resetButton, selfTestButton])
        {
            TranslatesAutoresizingMaskIntoConstraints = false,
            Axis = UILayoutConstraintAxis.Vertical,
            Alignment = UIStackViewAlignment.Fill,
            Distribution = UIStackViewDistribution.Fill,
            Spacing = 8
        };

        View.AddSubview(headerStack);
        View.AddSubview(stateCard);
        View.AddSubview(controlsStack);
        View.AddSubview(_statusLabel);
        View.AddSubview(_lifecycleLabel);

        NSLayoutConstraint.ActivateConstraints(
        [
            headerStack.TopAnchor.ConstraintEqualTo(
                View.SafeAreaLayoutGuide.TopAnchor, 16),
            headerStack.LeadingAnchor.ConstraintEqualTo(
                View.SafeAreaLayoutGuide.LeadingAnchor, 24),
            headerStack.TrailingAnchor.ConstraintEqualTo(
                View.SafeAreaLayoutGuide.TrailingAnchor, -24),

            stateCard.TopAnchor.ConstraintEqualTo(headerStack.BottomAnchor, 18),
            stateCard.LeadingAnchor.ConstraintEqualTo(
                View.SafeAreaLayoutGuide.LeadingAnchor, 20),
            stateCard.TrailingAnchor.ConstraintEqualTo(
                View.SafeAreaLayoutGuide.TrailingAnchor, -20),

            _stateCounter.TopAnchor.ConstraintEqualTo(stateCard.TopAnchor, 18),
            _stateCounter.LeadingAnchor.ConstraintEqualTo(stateCard.LeadingAnchor, 20),
            _stateCounter.TrailingAnchor.ConstraintEqualTo(stateCard.TrailingAnchor, -20),

            _stateTitle.TopAnchor.ConstraintEqualTo(_stateCounter.BottomAnchor, 10),
            _stateTitle.LeadingAnchor.ConstraintEqualTo(stateCard.LeadingAnchor, 20),
            _stateTitle.TrailingAnchor.ConstraintEqualTo(stateCard.TrailingAnchor, -20),

            _stateDetail.TopAnchor.ConstraintEqualTo(_stateTitle.BottomAnchor, 10),
            _stateDetail.LeadingAnchor.ConstraintEqualTo(stateCard.LeadingAnchor, 20),
            _stateDetail.TrailingAnchor.ConstraintEqualTo(stateCard.TrailingAnchor, -20),

            _progress.TopAnchor.ConstraintEqualTo(_stateDetail.BottomAnchor, 14),
            _progress.LeadingAnchor.ConstraintEqualTo(stateCard.LeadingAnchor, 20),
            _progress.TrailingAnchor.ConstraintEqualTo(stateCard.TrailingAnchor, -20),

            _primaryButton.TopAnchor.ConstraintEqualTo(_progress.BottomAnchor, 12),
            _primaryButton.LeadingAnchor.ConstraintEqualTo(stateCard.LeadingAnchor, 20),
            _primaryButton.TrailingAnchor.ConstraintEqualTo(stateCard.TrailingAnchor, -20),
            _primaryButton.HeightAnchor.ConstraintGreaterThanOrEqualTo(44),
            _primaryButton.BottomAnchor.ConstraintEqualTo(stateCard.BottomAnchor, -16),

            controlsStack.TopAnchor.ConstraintEqualTo(stateCard.BottomAnchor, 14),
            controlsStack.LeadingAnchor.ConstraintEqualTo(
                View.SafeAreaLayoutGuide.LeadingAnchor, 28),
            controlsStack.TrailingAnchor.ConstraintEqualTo(
                View.SafeAreaLayoutGuide.TrailingAnchor, -28),

            nextStateButton.HeightAnchor.ConstraintGreaterThanOrEqualTo(44),
            resetButton.HeightAnchor.ConstraintGreaterThanOrEqualTo(40),
            selfTestButton.HeightAnchor.ConstraintGreaterThanOrEqualTo(44),

            _statusLabel.TopAnchor.ConstraintEqualTo(controlsStack.BottomAnchor, 12),
            _statusLabel.LeadingAnchor.ConstraintEqualTo(
                View.SafeAreaLayoutGuide.LeadingAnchor, 24),
            _statusLabel.TrailingAnchor.ConstraintEqualTo(
                View.SafeAreaLayoutGuide.TrailingAnchor, -24),

            _lifecycleLabel.TopAnchor.ConstraintEqualTo(_statusLabel.BottomAnchor, 6),
            _lifecycleLabel.LeadingAnchor.ConstraintEqualTo(
                View.SafeAreaLayoutGuide.LeadingAnchor, 24),
            _lifecycleLabel.TrailingAnchor.ConstraintEqualTo(
                View.SafeAreaLayoutGuide.TrailingAnchor, -24),
            _lifecycleLabel.BottomAnchor.ConstraintLessThanOrEqualTo(
                View.SafeAreaLayoutGuide.BottomAnchor, -8)
        ]);

        try
        {
            // This call crosses the project/assembly boundary. If it succeeds,
            // the iOS app has loaded and executed StS2Launcher.Core.
            var snapshot = _controller.Snapshot;
            _coreLinkLabel.Text = "CORE LINK: PASS";
            _statusLabel.Text =
                "PASS: StS2Launcher.Core loaded and returned the initial state.";
            RenderSnapshot(snapshot);
        }
        catch (Exception ex)
        {
            _coreLinkLabel.Text = "CORE LINK: FAIL";
            _statusLabel.Text = $"FAIL: Core load: {ex.GetType().Name}: {ex.Message}";
        }

        Console.WriteLine("Step 03: RootViewController.ViewDidLoad complete");
    }

    public void SetLifecycleState(string state)
    {
        if (_lifecycleLabel is not null)
            _lifecycleLabel.Text = $"Lifecycle: {state}";
    }

    private void RunCoreSelfTest()
    {
        try
        {
            var result = CoreSelfTest.Run();

            _selfTestLabel!.Text = result.Summary;
            _selfTestLabel.TextColor = result.Passed
                ? UIColor.Label
                : UIColor.SystemRed;

            _statusLabel!.Text = result.Passed
                ? "PASS: separate Core assembly state-machine self-test completed."
                : $"FAIL: Core self-test passed only {result.PassedChecks}/{result.TotalChecks}.";
        }
        catch (Exception ex)
        {
            _selfTestLabel!.Text = "CORE SELF-TEST: EXCEPTION";
            _selfTestLabel.TextColor = UIColor.SystemRed;
            _statusLabel!.Text =
                $"FAIL: self-test: {ex.GetType().Name}: {ex.Message}";
        }
    }

    private void RenderSnapshot(LauncherSnapshot snapshot)
    {
        if (_stateTitle is null ||
            _stateDetail is null ||
            _stateCounter is null ||
            _progress is null ||
            _primaryButton is null)
        {
            return;
        }

        _stateCounter.Text =
            $"CORE STATE {snapshot.StateNumber} OF {snapshot.StateCount}";

        _stateTitle.Text = snapshot.Title;
        _stateDetail.Text = snapshot.Detail;

        _progress.Progress = snapshot.Progress;
        _progress.Hidden = !snapshot.ShowProgress;

        _primaryButton.SetTitle(
            snapshot.PrimaryActionTitle,
            UIControlState.Normal);

        Console.WriteLine($"Step 03: rendered Core state {snapshot.State}");
    }

    private static UILabel Label(string text, UIFont font, UIColor color)
    {
        return new UILabel
        {
            TranslatesAutoresizingMaskIntoConstraints = false,
            Text = text,
            TextColor = color,
            TextAlignment = UITextAlignment.Center,
            Lines = 0,
            Font = font
        };
    }

    private static UIButton SystemButton(string title, nfloat fontSize)
    {
        var button = UIButton.FromType(UIButtonType.System);
        button.TranslatesAutoresizingMaskIntoConstraints = false;
        button.SetTitle(title, UIControlState.Normal);
        button.TitleLabel!.Font = UIFont.BoldSystemFontOfSize(fontSize);
        return button;
    }
}
