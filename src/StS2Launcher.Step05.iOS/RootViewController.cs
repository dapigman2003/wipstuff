using Foundation;
using StS2Launcher.Core;
using StS2Launcher.Step05.iOS.Platform;
using UIKit;

namespace StS2Launcher.Step05.iOS;

public sealed class RootViewController : UIViewController
{
    private readonly KeychainProbe _keychainProbe =
        new(new KeychainCredentialStore());
    private readonly SteamConnectionProbe _steamProbe = new();
    private readonly SteamAuthenticationAttempt _authenticationAttempt = new();

    private UILabel? _foundationResultLabel;
    private UILabel? _foundationDetailLabel;
    private UILabel? _authResultLabel;
    private UILabel? _authDetailLabel;
    private UILabel? _statusLabel;
    private UILabel? _lifecycleLabel;
    private UITextField? _usernameField;
    private UITextField? _passwordField;
    private UIButton? _foundationButton;
    private UIButton? _authButton;
    private UIButton? _cancelAuthButton;
    private CancellationTokenSource? _authCts;
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
            "STEP 06 — STEAM AUTHENTICATION SESSION",
            UIFont.BoldSystemFontOfSize(18),
            UIColor.SecondaryLabel));

        content.AddArrangedSubview(Label(
            "Version 0.0.23",
            UIFont.SystemFontOfSize(17),
            UIColor.SecondaryLabel));

        content.AddArrangedSubview(Label(
            "AUTHENTICATION ONLY • NO OWNERSHIP • NO DOWNLOAD",
            UIFont.BoldSystemFontOfSize(14),
            UIColor.SecondaryLabel));

        content.AddArrangedSubview(Label(
            "Credentials are used only for this in-memory authentication attempt. Step 06 does not save passwords, refresh tokens, access tokens, or Steam Guard data. If Steam Guard is required, this build reports the challenge and stops without submitting a code or approving a device; that interaction belongs to Step 06.1.",
            UIFont.SystemFontOfSize(14),
            UIColor.SecondaryLabel));

        content.AddArrangedSubview(Separator());

        content.AddArrangedSubview(Label(
            "Steps 01–05 regression",
            UIFont.BoldSystemFontOfSize(24),
            UIColor.Label));

        _foundationButton = SystemButton("Run Foundation 5/5 Regression", 16);
        _foundationButton.TouchUpInside += async (_, _) => await RunFoundationVerificationAsync();
        content.AddArrangedSubview(_foundationButton);

        _foundationResultLabel = Label(
            "FOUNDATION: NOT RUN",
            UIFont.BoldSystemFontOfSize(19),
            UIColor.Label);
        content.AddArrangedSubview(_foundationResultLabel);

        _foundationDetailLabel = Label(
            "The proven Steps 01–05 checks remain available unchanged.",
            UIFont.SystemFontOfSize(14),
            UIColor.SecondaryLabel);
        content.AddArrangedSubview(_foundationDetailLabel);

        content.AddArrangedSubview(Separator());

        content.AddArrangedSubview(Label(
            "Step 06 authentication",
            UIFont.BoldSystemFontOfSize(26),
            UIColor.Label));

        _usernameField = TextField(
            placeholder: "Steam account name",
            secure: false,
            contentType: UITextContentType.Username);
        _usernameField.AutocorrectionType = UITextAutocorrectionType.No;
        _usernameField.AutocapitalizationType = UITextAutocapitalizationType.None;
        content.AddArrangedSubview(_usernameField);

        _passwordField = TextField(
            placeholder: "Steam password",
            secure: true,
            contentType: UITextContentType.Password);
        content.AddArrangedSubview(_passwordField);

        _authButton = SystemButton("Start Step 06 Authentication", 17);
        _authButton.TouchUpInside += async (_, _) => await RunAuthenticationAsync();
        content.AddArrangedSubview(_authButton);

        _cancelAuthButton = SystemButton("Cancel Authentication", 15);
        _cancelAuthButton.Enabled = false;
        _cancelAuthButton.TouchUpInside += (_, _) => _authCts?.Cancel();
        content.AddArrangedSubview(_cancelAuthButton);

        _authResultLabel = Label(
            "STEAM AUTH: NOT RUN",
            UIFont.BoldSystemFontOfSize(21),
            UIColor.Label);
        content.AddArrangedSubview(_authResultLabel);

        _authDetailLabel = Label(
            "Enter your Steam account name and password to test the modern SteamKit authentication session.",
            UIFont.SystemFontOfSize(15),
            UIColor.SecondaryLabel);
        content.AddArrangedSubview(_authDetailLabel);

        content.AddArrangedSubview(Separator());

        _statusLabel = Label(
            "Status: Steps 01–05 are proven. Step 06 has not run yet.",
            UIFont.SystemFontOfSize(14),
            UIColor.Label);
        content.AddArrangedSubview(_statusLabel);

        _lifecycleLabel = Label(
            "Lifecycle: Starting",
            UIFont.SystemFontOfSize(13),
            UIColor.SecondaryLabel);
        content.AddArrangedSubview(_lifecycleLabel);

        foreach (var control in content.ArrangedSubviews)
        {
            if (control is UIButton or UITextField)
                control.HeightAnchor.ConstraintGreaterThanOrEqualTo(44).Active = true;
        }

        _uiStartupPassed = true;
        Console.WriteLine("Step 06: RootViewController.ViewDidLoad complete");
    }

    public void SetLifecycleState(string state)
    {
        _lifecycleActive = string.Equals(state, "Active", StringComparison.Ordinal);

        if (_lifecycleLabel is not null)
            _lifecycleLabel.Text = $"Lifecycle: {state}";
    }

    private async Task RunAuthenticationAsync()
    {
        if (_authButton is null ||
            _cancelAuthButton is null ||
            _authResultLabel is null ||
            _authDetailLabel is null ||
            _statusLabel is null ||
            _usernameField is null ||
            _passwordField is null)
        {
            return;
        }

        var username = _usernameField.Text?.Trim() ?? string.Empty;
        var password = _passwordField.Text ?? string.Empty;

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password))
        {
            _authResultLabel.Text = "STEAM AUTH: INPUT REQUIRED";
            _authResultLabel.TextColor = UIColor.SystemRed;
            _authDetailLabel.Text = "Enter both the Steam account name and password.";
            return;
        }

        // Remove the password from the visible UIKit control immediately. The
        // transient managed string is used only for this single request and is
        // never stored or logged.
        _passwordField.Text = string.Empty;

        _authCts?.Dispose();
        _authCts = new CancellationTokenSource();

        _authButton.Enabled = false;
        _cancelAuthButton.Enabled = true;
        _authResultLabel.Text = "STEAM AUTH: RUNNING…";
        _authResultLabel.TextColor = UIColor.Label;
        _authDetailLabel.Text =
            "Connecting with the proven Step 05 WebSocket path, then beginning the modern credential auth session…";
        _statusLabel.Text = "STEP 06 RUNNING — keep the app in the foreground.";

        try
        {
            var result = await _authenticationAttempt.RunAsync(
                username,
                password,
                TimeSpan.FromSeconds(60),
                _authCts.Token);

            InvokeOnMainThread(() =>
            {
                _authResultLabel.Text = result.Summary;
                _authResultLabel.TextColor = result.Outcome switch
                {
                    SteamAuthenticationOutcome.Authenticated => UIColor.Label,
                    SteamAuthenticationOutcome.GuardRequired => UIColor.SystemOrange,
                    SteamAuthenticationOutcome.Cancelled => UIColor.SecondaryLabel,
                    _ => UIColor.SystemRed,
                };

                _authDetailLabel.Text = FormatAuthDetail(result);

                _statusLabel.Text = result.Outcome switch
                {
                    SteamAuthenticationOutcome.Authenticated =>
                        "PASS: Step 06 authenticated and returned Steam identity. No ownership request was made.",
                    SteamAuthenticationOutcome.GuardRequired =>
                        "BOUNDARY REACHED: Steam accepted the base credential session and requires Steam Guard. Step 06.1 should implement this exact challenge.",
                    SteamAuthenticationOutcome.Cancelled =>
                        "Authentication attempt cancelled; no credentials or tokens were persisted.",
                    _ =>
                        "FAIL: Step 06 authentication failed before a successful identity or expected Steam Guard boundary.",
                };
            });
        }
        catch (Exception ex)
        {
            InvokeOnMainThread(() =>
            {
                _authResultLabel.Text = "STEAM AUTH: EXCEPTION";
                _authResultLabel.TextColor = UIColor.SystemRed;
                _authDetailLabel.Text = $"{ex.GetType().Name}: {ex.Message}";
                _statusLabel.Text = "FAIL: unhandled exception during Step 06 authentication.";
            });
        }
        finally
        {
            InvokeOnMainThread(() =>
            {
                _authButton.Enabled = true;
                _cancelAuthButton.Enabled = false;
            });
        }
    }

    private static string FormatAuthDetail(SteamAuthenticationResult result)
    {
        var lines = new List<string>
        {
            $"CM connected: {YesNo(result.CmConnected)}",
            $"Auth session started: {YesNo(result.AuthSessionStarted)}",
            $"LoggedOnCallback: {YesNo(result.LoggedOnCallbackReceived)}",
            $"Logon result: {result.LogonResult?.ToString() ?? "N/A"}",
            $"Extended result: {result.ExtendedLogonResult?.ToString() ?? "N/A"}",
            $"Account name: {result.AccountName ?? "not-returned"}",
            $"SteamID64: {result.SteamId64 ?? "not-returned"}",
            $"CurrentEndPoint: {result.CurrentEndPoint ?? "never-set"}",
            $"Elapsed: {result.Elapsed.TotalSeconds:F1}s",
        };

        if (result.GuardChallenge is not null)
        {
            lines.Add($"Guard type: {result.GuardChallenge.Kind}");
            if (!string.IsNullOrWhiteSpace(result.GuardChallenge.AssociatedMessage))
                lines.Add($"Guard detail: {result.GuardChallenge.AssociatedMessage}");
        }

        if (!string.IsNullOrWhiteSpace(result.Error))
            lines.Add($"Error: {result.Error}");

        lines.Add("Credential persistence: NONE");
        lines.Add("Ownership request: NOT RUN");
        return string.Join("\n", lines);
    }

    private async Task RunFoundationVerificationAsync()
    {
        if (_foundationButton is null ||
            _foundationResultLabel is null ||
            _foundationDetailLabel is null ||
            _statusLabel is null)
        {
            return;
        }

        _foundationButton.Enabled = false;
        _foundationResultLabel.Text = "FOUNDATION: TESTING…";
        _foundationResultLabel.TextColor = UIColor.Label;
        _foundationDetailLabel.Text = "Running the proven Steps 01–05 5/5 regression…";

        try
        {
            var core = CoreSelfTest.Run();
            var keychain = _keychainProbe.RunRoundTrip();
            var steam = await _steamProbe.RunAsync(TimeSpan.FromSeconds(25));
            var final = new FoundationVerificationResult(
                UiStartupPassed: _uiStartupPassed,
                LifecycleActive: _lifecycleActive,
                Core: core,
                CredentialStore: keychain,
                Steam: steam);

            InvokeOnMainThread(() =>
            {
                _foundationResultLabel.Text = final.Summary;
                _foundationResultLabel.TextColor = final.Passed
                    ? UIColor.Label
                    : UIColor.SystemRed;

                _foundationDetailLabel.Text =
                    $"App/UI startup: {PassFail(final.UiStartupPassed)}\n" +
                    $"Lifecycle active: {PassFail(final.LifecycleActive)}\n" +
                    $"{final.Core.Summary}\n" +
                    $"{final.CredentialStore.Summary} — probe value cleaned\n" +
                    $"{final.Steam.Summary}\n" +
                    $"CMWebSocket factory used: {YesNo(final.Steam.CmWebSocketFactoryUsed)}";

                _statusLabel.Text = final.Passed
                    ? "PASS: Steps 01–05 foundation still passes 5/5 on this device."
                    : "FAIL: a proven foundation regression failed; stop Step 06 work until understood.";
            });
        }
        catch (Exception ex)
        {
            InvokeOnMainThread(() =>
            {
                _foundationResultLabel.Text = "FOUNDATION: EXCEPTION";
                _foundationResultLabel.TextColor = UIColor.SystemRed;
                _foundationDetailLabel.Text = $"{ex.GetType().Name}: {ex.Message}";
                _statusLabel.Text = "FAIL: exception during Steps 01–05 regression.";
            });
        }
        finally
        {
            InvokeOnMainThread(() => _foundationButton.Enabled = true);
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

    private static UITextField TextField(
        string placeholder,
        bool secure,
        NSString contentType)
    {
        return new UITextField
        {
            TranslatesAutoresizingMaskIntoConstraints = false,
            Placeholder = placeholder,
            SecureTextEntry = secure,
            TextContentType = contentType,
            BorderStyle = UITextBorderStyle.RoundedRect,
            ClearButtonMode = UITextFieldViewMode.WhileEditing,
            ReturnKeyType = secure ? UIReturnKeyType.Go : UIReturnKeyType.Next,
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
