using StS2Launcher.Core;
using StS2Launcher.Step04.iOS.Platform;
using UIKit;

namespace StS2Launcher.Step04.iOS;

public sealed class RootViewController : UIViewController
{
    private readonly LauncherController _controller = new();
    private readonly KeychainProbe _keychainProbe =
        new(new KeychainCredentialStore());

    private UILabel? _coreLinkLabel;
    private UILabel? _keychainStartupLabel;
    private UILabel? _keychainTestLabel;
    private UILabel? _stateLabel;
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
            Spacing = 14
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
            "STEP 04 — KEYCHAIN PROBE",
            UIFont.BoldSystemFontOfSize(15),
            UIColor.SecondaryLabel));

        content.AddArrangedSubview(Label(
            "Version 0.0.5",
            UIFont.SystemFontOfSize(14),
            UIColor.SecondaryLabel));

        _coreLinkLabel = Label(
            "CORE LINK: checking…",
            UIFont.BoldSystemFontOfSize(15),
            UIColor.Label);
        content.AddArrangedSubview(_coreLinkLabel);

        content.AddArrangedSubview(Separator());

        content.AddArrangedSubview(Label(
            "Secure-storage probe",
            UIFont.BoldSystemFontOfSize(22),
            UIColor.Label));

        content.AddArrangedSubview(Label(
            "Only the fixed dummy values STEP04-ALPHA and STEP04-BETA are used. " +
            "No Steam account information is present in this build.",
            UIFont.SystemFontOfSize(14),
            UIColor.SecondaryLabel));

        _keychainStartupLabel = Label(
            "PERSISTENCE: checking…",
            UIFont.BoldSystemFontOfSize(16),
            UIColor.Label);
        content.AddArrangedSubview(_keychainStartupLabel);

        _keychainTestLabel = Label(
            "KEYCHAIN ROUND-TRIP: NOT RUN",
            UIFont.BoldSystemFontOfSize(16),
            UIColor.SecondaryLabel);
        content.AddArrangedSubview(_keychainTestLabel);

        var runKeychain = SystemButton("Run Keychain Round-Trip", 17);
        runKeychain.TouchUpInside += (_, _) => RunKeychainRoundTrip();
        content.AddArrangedSubview(runKeychain);

        var deleteKeychain = SystemButton("Delete Test Secret", 16);
        deleteKeychain.TouchUpInside += (_, _) => DeleteKeychainProbe();
        content.AddArrangedSubview(deleteKeychain);

        content.AddArrangedSubview(Separator());

        content.AddArrangedSubview(Label(
            "Core regression check",
            UIFont.BoldSystemFontOfSize(22),
            UIColor.Label));

        _stateLabel = Label(
            "CORE STATE: checking…",
            UIFont.SystemFontOfSize(16),
            UIColor.Label);
        content.AddArrangedSubview(_stateLabel);

        var nextState = SystemButton("Next Core State", 16);
        nextState.TouchUpInside += (_, _) =>
        {
            RenderCoreState(_controller.NextDemoState());
        };
        content.AddArrangedSubview(nextState);

        var resetCore = SystemButton("Reset Core State", 16);
        resetCore.TouchUpInside += (_, _) =>
        {
            RenderCoreState(_controller.Reset());
            _statusLabel!.Text = "PASS: Core reset returned to SignedOut.";
        };
        content.AddArrangedSubview(resetCore);

        var coreSelfTest = SystemButton("Run Core Self-Test", 16);
        coreSelfTest.TouchUpInside += (_, _) =>
        {
            var result = CoreSelfTest.Run();
            _statusLabel!.Text = result.Summary;
        };
        content.AddArrangedSubview(coreSelfTest);

        content.AddArrangedSubview(Separator());

        _statusLabel = Label(
            "Status: starting Step 04 probes.",
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

        Console.WriteLine("Step 04: RootViewController.ViewDidLoad complete");
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
            _coreLinkLabel!.Text = "CORE LINK: PASS";
            RenderCoreState(snapshot);
        }
        catch (Exception ex)
        {
            _coreLinkLabel!.Text = "CORE LINK: FAIL";
            _statusLabel!.Text =
                $"FAIL: Core load: {ex.GetType().Name}: {ex.Message}";
            return;
        }

        try
        {
            var persisted = _keychainProbe.ReadPersistedValue();

            _keychainStartupLabel!.Text = persisted switch
            {
                KeychainProbe.BetaValue =>
                    "PERSISTENCE: PASS — STEP04-BETA found",
                KeychainProbe.AlphaValue =>
                    "PERSISTENCE: FOUND OLD ALPHA — run round-trip again",
                null =>
                    "PERSISTENCE: NOT SET — expected before first test",
                _ =>
                    "PERSISTENCE: UNEXPECTED TEST VALUE"
            };

            _statusLabel!.Text =
                "PASS: UIKit and Core loaded; Keychain startup query completed.";
        }
        catch (Exception ex)
        {
            _keychainStartupLabel!.Text = "PERSISTENCE: FAIL";
            _statusLabel!.Text =
                $"FAIL: Keychain startup query: {ex.GetType().Name}: {ex.Message}";
        }
    }

    private void RunKeychainRoundTrip()
    {
        try
        {
            var result = _keychainProbe.RunRoundTrip();

            _keychainTestLabel!.Text = result.Summary;
            _keychainTestLabel.TextColor = result.Passed
                ? UIColor.Label
                : UIColor.SystemRed;

            _keychainStartupLabel!.Text = result.Passed
                ? "PERSISTENCE: STEP04-BETA stored — terminate and reopen next"
                : "PERSISTENCE: NOT READY";

            _statusLabel!.Text = result.Passed
                ? "PASS: dummy value write/read/overwrite/read succeeded."
                : $"FAIL: only {result.PassedChecks}/{result.TotalChecks} Keychain checks passed.";
        }
        catch (Exception ex)
        {
            _keychainTestLabel!.Text = "KEYCHAIN ROUND-TRIP: EXCEPTION";
            _keychainTestLabel.TextColor = UIColor.SystemRed;
            _statusLabel!.Text =
                $"FAIL: Keychain round-trip: {ex.GetType().Name}: {ex.Message}";
        }
    }

    private void DeleteKeychainProbe()
    {
        try
        {
            var absent = _keychainProbe.DeleteTestValue();

            _keychainStartupLabel!.Text = absent
                ? "PERSISTENCE: DELETED — test value absent"
                : "PERSISTENCE: DELETE CHECK FAILED";

            _statusLabel!.Text = absent
                ? "PASS: dummy Keychain value deleted and confirmed absent."
                : "FAIL: dummy Keychain value is still present.";
        }
        catch (Exception ex)
        {
            _statusLabel!.Text =
                $"FAIL: Keychain delete: {ex.GetType().Name}: {ex.Message}";
        }
    }

    private void RenderCoreState(LauncherSnapshot snapshot)
    {
        if (_stateLabel is null)
            return;

        _stateLabel.Text =
            $"CORE STATE {snapshot.StateNumber}/{snapshot.StateCount}: " +
            $"{snapshot.Title}";
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
