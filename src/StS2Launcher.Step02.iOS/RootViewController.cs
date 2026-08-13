using UIKit;

namespace StS2Launcher.Step02.iOS;

public sealed class RootViewController : UIViewController
{
    private LauncherDemoState _state = LauncherDemoState.SignedOut;

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

        var appTitle = new UILabel
        {
            TranslatesAutoresizingMaskIntoConstraints = false,
            Text = "StS2 Launcher",
            TextColor = UIColor.Label,
            TextAlignment = UITextAlignment.Center,
            Font = UIFont.BoldSystemFontOfSize(30)
        };

        var step = new UILabel
        {
            TranslatesAutoresizingMaskIntoConstraints = false,
            Text = "STEP 02 — LAUNCHER UI SHELL",
            TextColor = UIColor.SecondaryLabel,
            TextAlignment = UITextAlignment.Center,
            Font = UIFont.BoldSystemFontOfSize(15)
        };

        var version = new UILabel
        {
            TranslatesAutoresizingMaskIntoConstraints = false,
            Text = "Version 0.0.3",
            TextColor = UIColor.SecondaryLabel,
            TextAlignment = UITextAlignment.Center,
            Font = UIFont.SystemFontOfSize(14)
        };

        _stateCounter = new UILabel
        {
            TranslatesAutoresizingMaskIntoConstraints = false,
            TextColor = UIColor.SecondaryLabel,
            TextAlignment = UITextAlignment.Center,
            Font = UIFont.MonospacedDigitSystemFontOfSize(13, UIFontWeight.Regular)
        };

        _stateTitle = new UILabel
        {
            TranslatesAutoresizingMaskIntoConstraints = false,
            TextColor = UIColor.Label,
            TextAlignment = UITextAlignment.Center,
            Font = UIFont.BoldSystemFontOfSize(24),
            Lines = 0
        };

        _stateDetail = new UILabel
        {
            TranslatesAutoresizingMaskIntoConstraints = false,
            TextColor = UIColor.SecondaryLabel,
            TextAlignment = UITextAlignment.Center,
            Font = UIFont.SystemFontOfSize(16),
            Lines = 0
        };

        _progress = new UIProgressView(UIProgressViewStyle.Default)
        {
            TranslatesAutoresizingMaskIntoConstraints = false,
            Progress = 0
        };

        _primaryButton = UIButton.FromType(UIButtonType.System);
        _primaryButton.TranslatesAutoresizingMaskIntoConstraints = false;
        _primaryButton.SetTitle("Sign in with Steam (mock)", UIControlState.Normal);
        _primaryButton.TitleLabel!.Font = UIFont.BoldSystemFontOfSize(17);
        _primaryButton.TouchUpInside += (_, _) =>
        {
            _statusLabel!.Text =
                $"PASS: primary action tapped in state {_state}. No network call was made.";
        };

        var nextStateButton = UIButton.FromType(UIButtonType.System);
        nextStateButton.TranslatesAutoresizingMaskIntoConstraints = false;
        nextStateButton.SetTitle("Next Demo State", UIControlState.Normal);
        nextStateButton.TitleLabel!.Font = UIFont.BoldSystemFontOfSize(18);
        nextStateButton.TouchUpInside += (_, _) =>
        {
            _state = LauncherDemoStatePresentation.Next(_state);
            RenderState();
        };

        var resetButton = UIButton.FromType(UIButtonType.System);
        resetButton.TranslatesAutoresizingMaskIntoConstraints = false;
        resetButton.SetTitle("Reset Demo", UIControlState.Normal);
        resetButton.TouchUpInside += (_, _) =>
        {
            _state = LauncherDemoState.SignedOut;
            _statusLabel!.Text = "Status: demo reset.";
            RenderState();
        };

        _statusLabel = new UILabel
        {
            TranslatesAutoresizingMaskIntoConstraints = false,
            Text = "Status: launcher UI rendered successfully.",
            TextColor = UIColor.Label,
            TextAlignment = UITextAlignment.Center,
            Lines = 0,
            Font = UIFont.SystemFontOfSize(14)
        };

        _lifecycleLabel = new UILabel
        {
            TranslatesAutoresizingMaskIntoConstraints = false,
            Text = "Lifecycle: Starting",
            TextColor = UIColor.SecondaryLabel,
            TextAlignment = UITextAlignment.Center,
            Lines = 0,
            Font = UIFont.SystemFontOfSize(13)
        };

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

        var controlsStack = new UIStackView([nextStateButton, resetButton])
        {
            TranslatesAutoresizingMaskIntoConstraints = false,
            Axis = UILayoutConstraintAxis.Vertical,
            Alignment = UIStackViewAlignment.Fill,
            Distribution = UIStackViewDistribution.Fill,
            Spacing = 10
        };

        var headerStack = new UIStackView([appTitle, step, version])
        {
            TranslatesAutoresizingMaskIntoConstraints = false,
            Axis = UILayoutConstraintAxis.Vertical,
            Alignment = UIStackViewAlignment.Fill,
            Distribution = UIStackViewDistribution.Fill,
            Spacing = 6
        };

        View.AddSubview(headerStack);
        View.AddSubview(stateCard);
        View.AddSubview(controlsStack);
        View.AddSubview(_statusLabel);
        View.AddSubview(_lifecycleLabel);

        NSLayoutConstraint.ActivateConstraints(
        [
            headerStack.TopAnchor.ConstraintEqualTo(
                View.SafeAreaLayoutGuide.TopAnchor, 24),
            headerStack.LeadingAnchor.ConstraintEqualTo(
                View.SafeAreaLayoutGuide.LeadingAnchor, 24),
            headerStack.TrailingAnchor.ConstraintEqualTo(
                View.SafeAreaLayoutGuide.TrailingAnchor, -24),

            stateCard.TopAnchor.ConstraintEqualTo(headerStack.BottomAnchor, 28),
            stateCard.LeadingAnchor.ConstraintEqualTo(
                View.SafeAreaLayoutGuide.LeadingAnchor, 20),
            stateCard.TrailingAnchor.ConstraintEqualTo(
                View.SafeAreaLayoutGuide.TrailingAnchor, -20),

            _stateCounter.TopAnchor.ConstraintEqualTo(stateCard.TopAnchor, 20),
            _stateCounter.LeadingAnchor.ConstraintEqualTo(stateCard.LeadingAnchor, 20),
            _stateCounter.TrailingAnchor.ConstraintEqualTo(stateCard.TrailingAnchor, -20),

            _stateTitle.TopAnchor.ConstraintEqualTo(_stateCounter.BottomAnchor, 12),
            _stateTitle.LeadingAnchor.ConstraintEqualTo(stateCard.LeadingAnchor, 20),
            _stateTitle.TrailingAnchor.ConstraintEqualTo(stateCard.TrailingAnchor, -20),

            _stateDetail.TopAnchor.ConstraintEqualTo(_stateTitle.BottomAnchor, 12),
            _stateDetail.LeadingAnchor.ConstraintEqualTo(stateCard.LeadingAnchor, 20),
            _stateDetail.TrailingAnchor.ConstraintEqualTo(stateCard.TrailingAnchor, -20),

            _progress.TopAnchor.ConstraintEqualTo(_stateDetail.BottomAnchor, 18),
            _progress.LeadingAnchor.ConstraintEqualTo(stateCard.LeadingAnchor, 20),
            _progress.TrailingAnchor.ConstraintEqualTo(stateCard.TrailingAnchor, -20),

            _primaryButton.TopAnchor.ConstraintEqualTo(_progress.BottomAnchor, 16),
            _primaryButton.LeadingAnchor.ConstraintEqualTo(stateCard.LeadingAnchor, 20),
            _primaryButton.TrailingAnchor.ConstraintEqualTo(stateCard.TrailingAnchor, -20),
            _primaryButton.HeightAnchor.ConstraintGreaterThanOrEqualTo(46),
            _primaryButton.BottomAnchor.ConstraintEqualTo(stateCard.BottomAnchor, -18),

            controlsStack.TopAnchor.ConstraintEqualTo(stateCard.BottomAnchor, 20),
            controlsStack.LeadingAnchor.ConstraintEqualTo(
                View.SafeAreaLayoutGuide.LeadingAnchor, 28),
            controlsStack.TrailingAnchor.ConstraintEqualTo(
                View.SafeAreaLayoutGuide.TrailingAnchor, -28),

            nextStateButton.HeightAnchor.ConstraintGreaterThanOrEqualTo(48),
            resetButton.HeightAnchor.ConstraintGreaterThanOrEqualTo(44),

            _statusLabel.TopAnchor.ConstraintEqualTo(controlsStack.BottomAnchor, 18),
            _statusLabel.LeadingAnchor.ConstraintEqualTo(
                View.SafeAreaLayoutGuide.LeadingAnchor, 24),
            _statusLabel.TrailingAnchor.ConstraintEqualTo(
                View.SafeAreaLayoutGuide.TrailingAnchor, -24),

            _lifecycleLabel.TopAnchor.ConstraintEqualTo(_statusLabel.BottomAnchor, 8),
            _lifecycleLabel.LeadingAnchor.ConstraintEqualTo(
                View.SafeAreaLayoutGuide.LeadingAnchor, 24),
            _lifecycleLabel.TrailingAnchor.ConstraintEqualTo(
                View.SafeAreaLayoutGuide.TrailingAnchor, -24),
            _lifecycleLabel.BottomAnchor.ConstraintLessThanOrEqualTo(
                View.SafeAreaLayoutGuide.BottomAnchor, -12)
        ]);

        RenderState();
        Console.WriteLine("Step 02: RootViewController.ViewDidLoad complete");
    }

    public void SetLifecycleState(string state)
    {
        if (_lifecycleLabel is not null)
            _lifecycleLabel.Text = $"Lifecycle: {state}";
    }

    private void RenderState()
    {
        if (_stateTitle is null ||
            _stateDetail is null ||
            _stateCounter is null ||
            _progress is null ||
            _primaryButton is null)
        {
            return;
        }

        _stateCounter.Text = $"DEMO STATE {(int)_state + 1} OF 7";
        _stateTitle.Text = LauncherDemoStatePresentation.Title(_state);
        _stateDetail.Text = LauncherDemoStatePresentation.Detail(_state);

        var progress = LauncherDemoStatePresentation.Progress(_state);
        _progress.Progress = progress;
        _progress.Hidden = _state != LauncherDemoState.Downloading;

        var primaryTitle = _state switch
        {
            LauncherDemoState.SignedOut => "Sign in with Steam (mock)",
            LauncherDemoState.Authenticating => "Authentication busy (mock)",
            LauncherDemoState.CheckingOwnership => "Ownership check busy (mock)",
            LauncherDemoState.ReadyToInstall => "Install (mock)",
            LauncherDemoState.Downloading => "Downloading 42% (mock)",
            LauncherDemoState.ReadyToPlay => "Play disabled in Step 02",
            LauncherDemoState.Error => "Retry (mock)",
            _ => "Mock action"
        };

        _primaryButton.SetTitle(primaryTitle, UIControlState.Normal);

        if (_statusLabel is not null)
        {
            _statusLabel.Text =
                $"PASS: rendered {_state}. Tap Next Demo State to continue.";
        }

        Console.WriteLine($"Step 02: rendered demo state {_state}");
    }
}
