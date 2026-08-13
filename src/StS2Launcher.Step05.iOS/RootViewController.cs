using StS2Launcher.Core;
using StS2Launcher.Step05.iOS.Platform;
using UIKit;

namespace StS2Launcher.Step05.iOS;

public sealed class RootViewController : UIViewController
{
    private readonly LauncherController _controller = new();
    private readonly KeychainProbe _keychainProbe =
        new(new KeychainCredentialStore());
    private readonly SteamConnectionProbe _steamProbe = new();

    private UILabel? _steamAssemblyLabel;
    private UILabel? _steamResultLabel;
    private UILabel? _steamDetailLabel;
    private UIButton? _steamButton;
    private UILabel? _coreLabel;
    private UILabel? _keychainLabel;
    private UILabel? _statusLabel;
    private UILabel? _lifecycleLabel;

    public override void ViewDidLoad()
    {
        base.ViewDidLoad();

        View!.BackgroundColor = UIColor.SystemBackground;

        var scroll = new UIScrollView
        {
            TranslatesAutoresizingMaskIntoConstraints = false,
            AlwaysBounceVertical = true
        };

        var content = new UIStackView
        {
            TranslatesAutoresizingMaskIntoConstraints = false,
            Axis = UILayoutConstraintAxis.Vertical,
            Alignment = UIStackViewAlignment.Fill,
            Distribution = UIStackViewDistribution.Fill,
            Spacing = 13
        };

        View.AddSubview(scroll);
        scroll.AddSubview(content);

        NSLayoutConstraint.ActivateConstraints(
        [
            scroll.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
            scroll.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor),
            scroll.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
            scroll.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),

            content.TopAnchor.ConstraintEqualTo(scroll.ContentLayoutGuide.TopAnchor, 18),
            content.BottomAnchor.ConstraintEqualTo(scroll.ContentLayoutGuide.BottomAnchor, -18),
            content.LeadingAnchor.ConstraintEqualTo(scroll.FrameLayoutGuide.LeadingAnchor, 24),
            content.TrailingAnchor.ConstraintEqualTo(scroll.FrameLayoutGuide.TrailingAnchor, -24)
        ]);

        content.AddArrangedSubview(Label(
            "StS2 Launcher",
            UIFont.BoldSystemFontOfSize(30),
            UIColor.Label));

        content.AddArrangedSubview(Label(
            "STEP 05.1 — STEAM LINK FIX",
            UIFont.BoldSystemFontOfSize(15),
            UIColor.SecondaryLabel));

        content.AddArrangedSubview(Label(
            "Version 0.0.7",
            UIFont.SystemFontOfSize(14),
            UIColor.SecondaryLabel));

        content.AddArrangedSubview(Label(
            "NO LOGIN • NO PASSWORD • NO STEAM GUARD • NO TOKEN",
            UIFont.BoldSystemFontOfSize(13),
            UIColor.SecondaryLabel));

        content.AddArrangedSubview(Separator());

        content.AddArrangedSubview(Label(
            "SteamKit2 load + connection",
            UIFont.BoldSystemFontOfSize(22),
            UIColor.Label));

        _steamAssemblyLabel = Label(
            "STEAMKIT ASSEMBLY: checking…",
            UIFont.BoldSystemFontOfSize(15),
            UIColor.Label);
        content.AddArrangedSubview(_steamAssemblyLabel);

        _steamResultLabel = Label(
            "STEAM CONNECTION: NOT RUN",
            UIFont.BoldSystemFontOfSize(17),
            UIColor.SecondaryLabel);
        content.AddArrangedSubview(_steamResultLabel);

        _steamDetailLabel = Label(
            "This test only connects to the Steam network and disconnects.",
            UIFont.SystemFontOfSize(14),
            UIColor.SecondaryLabel);
        content.AddArrangedSubview(_steamDetailLabel);

        _steamButton = SystemButton("Run Steam Connection Probe", 17);
        _steamButton.TouchUpInside += async (_, _) => await RunSteamProbeAsync();
        content.AddArrangedSubview(_steamButton);

        content.AddArrangedSubview(Separator());

        content.AddArrangedSubview(Label(
            "Regression checks",
            UIFont.BoldSystemFontOfSize(22),
            UIColor.Label));

        _coreLabel = Label(
            "CORE: checking…",
            UIFont.SystemFontOfSize(15),
            UIColor.Label);
        content.AddArrangedSubview(_coreLabel);

        _keychainLabel = Label(
            "KEYCHAIN: checking…",
            UIFont.SystemFontOfSize(15),
            UIColor.Label);
        content.AddArrangedSubview(_keychainLabel);

        var coreSelfTest = SystemButton("Run Core Self-Test", 15);
        coreSelfTest.TouchUpInside += (_, _) =>
        {
            var result = CoreSelfTest.Run();
            _coreLabel!.Text = result.Summary;
            _statusLabel!.Text = result.Passed
                ? "PASS: Core regression self-test."
                : "FAIL: Core regression self-test.";
        };
        content.AddArrangedSubview(coreSelfTest);

        var keychainRead = SystemButton("Check Step-04 Keychain Is Empty", 15);
        keychainRead.TouchUpInside += (_, _) => CheckKeychainRegression();
        content.AddArrangedSubview(keychainRead);

        var nextCore = SystemButton("Next Core State", 15);
        nextCore.TouchUpInside += (_, _) =>
        {
            var snapshot = _controller.NextDemoState();
            _coreLabel!.Text =
                $"CORE STATE {snapshot.StateNumber}/{snapshot.StateCount}: {snapshot.Title}";
        };
        content.AddArrangedSubview(nextCore);

        var resetCore = SystemButton("Reset Core State", 15);
        resetCore.TouchUpInside += (_, _) =>
        {
            var snapshot = _controller.Reset();
            _coreLabel!.Text =
                $"CORE STATE {snapshot.StateNumber}/{snapshot.StateCount}: {snapshot.Title}";
        };
        content.AddArrangedSubview(resetCore);

        content.AddArrangedSubview(Separator());

        _statusLabel = Label(
            "Status: starting Step 05.1 checks.",
            UIFont.SystemFontOfSize(14),
            UIColor.Label);
        content.AddArrangedSubview(_statusLabel);

        _lifecycleLabel = Label(
            "Lifecycle: Starting",
            UIFont.SystemFontOfSize(13),
            UIColor.SecondaryLabel);
        content.AddArrangedSubview(_lifecycleLabel);

        foreach (var button in content.ArrangedSubviews.OfType<UIButton>())
            button.HeightAnchor.ConstraintGreaterThanOrEqualTo(44).Active = true;

        RunStartupChecks();

        Console.WriteLine("Step 05.1: RootViewController.ViewDidLoad complete");
    }

    public void SetLifecycleState(string state)
    {
        if (_lifecycleLabel is not null)
            _lifecycleLabel.Text = $"Lifecycle: {state}";
    }

    private void RunStartupChecks()
    {
        try
        {
            var snapshot = _controller.Snapshot;
            _coreLabel!.Text =
                $"CORE LINK: PASS — {snapshot.State}";
        }
        catch (Exception ex)
        {
            _coreLabel!.Text =
                $"CORE LINK: FAIL — {ex.GetType().Name}";
        }

        try
        {
            _steamAssemblyLabel!.Text =
                $"STEAMKIT ASSEMBLY: PASS — {SteamConnectionProbe.AssemblyVersion}";
        }
        catch (Exception ex)
        {
            _steamAssemblyLabel!.Text =
                $"STEAMKIT ASSEMBLY: FAIL — {ex.GetType().Name}: {ex.Message}";
        }

        CheckKeychainRegression();

        _statusLabel!.Text =
            "PASS: UIKit startup completed. Steam network probe has not run yet.";
    }

    private async Task RunSteamProbeAsync()
    {
        if (_steamButton is null ||
            _steamResultLabel is null ||
            _steamDetailLabel is null ||
            _statusLabel is null)
        {
            return;
        }

        _steamButton.Enabled = false;
        _steamResultLabel.Text = "STEAM CONNECTION: CONNECTING…";
        _steamResultLabel.TextColor = UIColor.Label;
        _steamDetailLabel.Text =
            "Waiting for SteamKit ConnectedCallback; no authentication will be attempted.";
        _statusLabel.Text =
            "NETWORK TEST RUNNING — leave the app in foreground.";

        try
        {
            var result = await _steamProbe.RunAsync(TimeSpan.FromSeconds(20));

            InvokeOnMainThread(() =>
            {
                _steamResultLabel.Text = result.Summary;
                _steamResultLabel.TextColor = result.Passed
                    ? UIColor.Label
                    : UIColor.SystemRed;

                _steamDetailLabel.Text =
                    $"{result.Detail}\nElapsed: {result.Elapsed.TotalSeconds:F1}s\n" +
                    $"SteamKit assembly: {result.SteamKitAssemblyVersion}";

                _statusLabel.Text = result.Passed
                    ? "PASS: Steam network-only connection/disconnection completed."
                    : "FAIL: Steam network probe did not complete. Report the detail above.";

                _steamButton.Enabled = true;
            });
        }
        catch (Exception ex)
        {
            InvokeOnMainThread(() =>
            {
                _steamResultLabel.Text = "STEAM CONNECTION: EXCEPTION";
                _steamResultLabel.TextColor = UIColor.SystemRed;
                _steamDetailLabel.Text =
                    $"{ex.GetType().Name}: {ex.Message}";
                _statusLabel.Text =
                    "FAIL: unhandled exception in Steam connection probe.";
                _steamButton.Enabled = true;
            });
        }
    }

    private void CheckKeychainRegression()
    {
        try
        {
            var value = _keychainProbe.ReadPersistedValue();

            _keychainLabel!.Text = value is null
                ? "KEYCHAIN REGRESSION: PASS — Step-04 dummy value absent"
                : $"KEYCHAIN REGRESSION: NOTE — Step-04 dummy value present ({value})";
        }
        catch (Exception ex)
        {
            _keychainLabel!.Text =
                $"KEYCHAIN REGRESSION: FAIL — {ex.GetType().Name}: {ex.Message}";
        }
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

    private static UIView Separator()
    {
        var view = new UIView
        {
            TranslatesAutoresizingMaskIntoConstraints = false,
            BackgroundColor = UIColor.Separator
        };
        view.HeightAnchor.ConstraintEqualTo(1).Active = true;
        return view;
    }
}
