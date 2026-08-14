using StS2Launcher.Core;
using StS2Launcher.Step05.iOS.Platform;
using UIKit;

namespace StS2Launcher.Step05.iOS;

public sealed class RootViewController : UIViewController
{
    private readonly KeychainProbe _keychainProbe =
        new(new KeychainCredentialStore());
    private readonly SteamConnectionProbe _steamProbe = new();

    private UILabel? _resultLabel;
    private UILabel? _detailLabel;
    private UILabel? _statusLabel;
    private UILabel? _lifecycleLabel;
    private UIButton? _runButton;
    private bool _uiStartupPassed;
    private bool _lifecycleActive;

    public override void ViewDidLoad()
    {
        base.ViewDidLoad();

        View!.BackgroundColor = UIColor.SystemBackground;

        var scroll = new UIScrollView
        {
            TranslatesAutoresizingMaskIntoConstraints = false
        };
        View.AddSubview(scroll);

        var content = new UIStackView
        {
            TranslatesAutoresizingMaskIntoConstraints = false,
            Axis = UILayoutConstraintAxis.Vertical,
            Alignment = UIStackViewAlignment.Fill,
            Spacing = 14,
            LayoutMarginsRelativeArrangement = true,
            LayoutMargins = new UIEdgeInsets(28, 24, 28, 24)
        };
        scroll.AddSubview(content);

        NSLayoutConstraint.ActivateConstraints(
        [
            scroll.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
            scroll.BottomAnchor.ConstraintEqualTo(View.BottomAnchor),
            scroll.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
            scroll.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),

            content.TopAnchor.ConstraintEqualTo(scroll.ContentLayoutGuide.TopAnchor),
            content.BottomAnchor.ConstraintEqualTo(scroll.ContentLayoutGuide.BottomAnchor),
            content.LeadingAnchor.ConstraintEqualTo(scroll.ContentLayoutGuide.LeadingAnchor),
            content.TrailingAnchor.ConstraintEqualTo(scroll.ContentLayoutGuide.TrailingAnchor),
            content.WidthAnchor.ConstraintEqualTo(scroll.FrameLayoutGuide.WidthAnchor)
        ]);

        content.AddArrangedSubview(Label(
            "StS2 Launcher",
            UIFont.BoldSystemFontOfSize(34),
            UIColor.Label));

        content.AddArrangedSubview(Label(
            "STEP 05.16 — FOUNDATION FINALIZATION",
            UIFont.BoldSystemFontOfSize(18),
            UIColor.SecondaryLabel));

        content.AddArrangedSubview(Label(
            "Version 0.0.22",
            UIFont.SystemFontOfSize(17),
            UIColor.SecondaryLabel));

        content.AddArrangedSubview(Label(
            "STEPS 01–05 • CLEANUP + TEST GATE",
            UIFont.BoldSystemFontOfSize(15),
            UIColor.SecondaryLabel));

        content.AddArrangedSubview(Label(
            "NO LOGIN • NO PASSWORD • NO STEAM GUARD • NO TOKEN",
            UIFont.BoldSystemFontOfSize(14),
            UIColor.SecondaryLabel));

        content.AddArrangedSubview(Separator());

        content.AddArrangedSubview(Label(
            "Foundation verification",
            UIFont.BoldSystemFontOfSize(26),
            UIColor.Label));

        content.AddArrangedSubview(Label(
            "This final Step 05 build keeps only the proven runtime path: UIKit lifecycle, Core state machine, iOS Keychain, and an unauthenticated SteamKit CM WebSocket connection. The temporary endpoint/handler/exception diagnostics used to find the bugs have been removed. Host unit tests run in Codemagic before the iOS publish begins.",
            UIFont.SystemFontOfSize(15),
            UIColor.SecondaryLabel));

        content.AddArrangedSubview(Label(
            $"STEAMKIT ASSEMBLY — {SteamConnectionProbe.AssemblyVersion}",
            UIFont.BoldSystemFontOfSize(16),
            UIColor.Label));

        content.AddArrangedSubview(Label(
            "RETAINED iOS FIXES — SocketsHttpHandler for CM WebSocket • protobuf/SteamKit trim roots • DiskArbitration linker filter • version-aware Process.StartTime patch",
            UIFont.SystemFontOfSize(13),
            UIColor.SecondaryLabel));

        _runButton = SystemButton("Run Steps 01–05 Device Verification", 17);
        _runButton.TouchUpInside += async (_, _) => await RunFoundationVerificationAsync();
        content.AddArrangedSubview(_runButton);

        _resultLabel = Label(
            "FOUNDATION: NOT RUN",
            UIFont.BoldSystemFontOfSize(21),
            UIColor.Label);
        content.AddArrangedSubview(_resultLabel);

        _detailLabel = Label(
            "Device gates will appear here.",
            UIFont.SystemFontOfSize(15),
            UIColor.SecondaryLabel);
        content.AddArrangedSubview(_detailLabel);

        content.AddArrangedSubview(Separator());

        _statusLabel = Label(
            "Status: UIKit startup completed. Final device verification has not run yet.",
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

        _uiStartupPassed = true;
        Console.WriteLine("Step 05.16: RootViewController.ViewDidLoad complete");
    }

    public void SetLifecycleState(string state)
    {
        _lifecycleActive = string.Equals(state, "Active", StringComparison.Ordinal);

        if (_lifecycleLabel is not null)
            _lifecycleLabel.Text = $"Lifecycle: {state}";
    }

    private async Task RunFoundationVerificationAsync()
    {
        if (_runButton is null ||
            _resultLabel is null ||
            _detailLabel is null ||
            _statusLabel is null)
        {
            return;
        }

        _runButton.Enabled = false;
        _resultLabel.Text = "FOUNDATION: TESTING…";
        _resultLabel.TextColor = UIColor.Label;
        _detailLabel.Text = "Running Core and Keychain checks, then SteamKit CM WebSocket 3/3…";
        _statusLabel.Text = "FINAL STEP 05 VERIFICATION RUNNING — keep the app in the foreground.";

        try
        {
            var core = CoreSelfTest.Run();
            var keychain = _keychainProbe.RunRoundTrip();

            InvokeOnMainThread(() =>
            {
                _detailLabel.Text =
                    $"App/UI startup: {PassFail(_uiStartupPassed)}\n" +
                    $"Lifecycle active: {PassFail(_lifecycleActive)}\n" +
                    $"{core.Summary}\n" +
                    $"{keychain.Summary} — probe value cleaned\n" +
                    "SteamKit CM WebSocket: testing…";
            });

            var steam = await _steamProbe.RunAsync(TimeSpan.FromSeconds(25));
            var final = new FoundationVerificationResult(
                UiStartupPassed: _uiStartupPassed,
                LifecycleActive: _lifecycleActive,
                Core: core,
                CredentialStore: keychain,
                Steam: steam);

            InvokeOnMainThread(() =>
            {
                _resultLabel.Text = final.Summary;
                _resultLabel.TextColor = final.Passed
                    ? UIColor.Label
                    : UIColor.SystemRed;

                _detailLabel.Text =
                    $"App/UI startup: {PassFail(final.UiStartupPassed)}\n" +
                    $"Lifecycle active: {PassFail(final.LifecycleActive)}\n" +
                    $"{final.Core.Summary}\n" +
                    $"{final.CredentialStore.Summary} — probe value cleaned\n" +
                    $"{final.Steam.Summary}\n" +
                    $"SteamKit assembly: {final.Steam.SteamKitAssemblyVersion}\n" +
                    $"CMWebSocket factory used: {YesNo(final.Steam.CmWebSocketFactoryUsed)}\n" +
                    $"ConnectedCallback: {YesNo(final.Steam.ConnectedCallbackReceived)}\n" +
                    $"DisconnectedCallback: {YesNo(final.Steam.DisconnectedCallbackReceived)}\n" +
                    $"Disconnected.UserInitiated: {(final.Steam.DisconnectedUserInitiated?.ToString() ?? "N/A")}\n" +
                    $"IsConnected ever: {final.Steam.IsConnectedEver}\n" +
                    $"CurrentEndPoint: {final.Steam.LastCurrentEndPoint ?? "never-set"}\n" +
                    $"Elapsed: {final.Steam.Elapsed.TotalSeconds:F1}s" +
                    (string.IsNullOrWhiteSpace(final.Steam.Error)
                        ? string.Empty
                        : $"\nError: {final.Steam.Error}");

                _statusLabel.Text = final.Passed
                    ? "PASS: Steps 01–05 foundation verified on this device. Step 05 is finalized."
                    : "FAIL: one or more foundation gates failed; do not advance until the failing gate is understood.";

                _runButton.Enabled = true;
            });
        }
        catch (Exception ex)
        {
            InvokeOnMainThread(() =>
            {
                _resultLabel.Text = "FOUNDATION: EXCEPTION";
                _resultLabel.TextColor = UIColor.SystemRed;
                _detailLabel.Text = $"{ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}";
                _statusLabel.Text = "FAIL: unhandled exception during final Step 05 verification.";
                _runButton.Enabled = true;
            });
        }
    }

    private static string PassFail(bool passed) => passed ? "PASS" : "FAIL";
    private static string YesNo(bool value) => value ? "YES" : "NO";

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
