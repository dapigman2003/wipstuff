using Foundation;
using StS2Launcher.Core;
using StS2Launcher.Step05.iOS.Platform;
using UIKit;

namespace StS2Launcher.Step05.iOS;

public sealed class RootViewController : UIViewController
{
    private readonly KeychainCredentialStore _credentialStore = new();
    private readonly KeychainProbe _keychainProbe;
    private readonly SteamSessionStore _sessionStore;
    private readonly SteamConnectionProbe _steamProbe = new();
    private readonly SteamAuthenticationAttempt _authenticationAttempt;
    private readonly SteamSessionResumeAttempt _resumeAttempt;

    private UILabel? _foundationResultLabel;
    private UILabel? _foundationDetailLabel;
    private UILabel? _authResultLabel;
    private UILabel? _authDetailLabel;
    private UILabel? _savedSessionLabel;
    private UILabel? _resumeResultLabel;
    private UILabel? _resumeDetailLabel;
    private UILabel? _statusLabel;
    private UILabel? _lifecycleLabel;
    private UITextField? _usernameField;
    private UITextField? _passwordField;
    private UIButton? _foundationButton;
    private UIButton? _authButton;
    private UIButton? _resumeButton;
    private UIButton? _signOutButton;
    private UIButton? _cancelOperationButton;
    private CancellationTokenSource? _operationCts;
    private bool _uiStartupPassed;
    private bool _lifecycleActive;

    public RootViewController()
    {
        _keychainProbe = new KeychainProbe(_credentialStore);
        _sessionStore = new SteamSessionStore(_credentialStore);
        _authenticationAttempt = new SteamAuthenticationAttempt(_sessionStore);
        _resumeAttempt = new SteamSessionResumeAttempt(_sessionStore);
    }

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
            "STEP 06.2 — KEYCHAIN SESSION RESUME",
            UIFont.BoldSystemFontOfSize(18),
            UIColor.SecondaryLabel));

        content.AddArrangedSubview(Label(
            "Version 0.0.25",
            UIFont.SystemFontOfSize(17),
            UIColor.SecondaryLabel));

        content.AddArrangedSubview(Label(
            "PERSIST REFRESH TOKEN ONLY • NO OWNERSHIP • NO DOWNLOAD",
            UIFont.BoldSystemFontOfSize(14),
            UIColor.SecondaryLabel));

        content.AddArrangedSubview(Label(
            "After a successful Steam login, Step 06.2 stores the returned reusable refresh token plus account identity in the device-bound iOS Keychain. The Steam password is never stored. No Steam Guard secret/code is stored. Relaunch the app and use Resume Saved Session to prove password-free login, then Sign Out to delete the saved session.",
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
            "Create / replace saved Steam session",
            UIFont.BoldSystemFontOfSize(25),
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

        _authButton = SystemButton("Authenticate + Save Session", 17);
        _authButton.TouchUpInside += async (_, _) => await RunAuthenticationAsync();
        content.AddArrangedSubview(_authButton);

        _authResultLabel = Label(
            "STEAM AUTH: NOT RUN",
            UIFont.BoldSystemFontOfSize(21),
            UIColor.Label);
        content.AddArrangedSubview(_authResultLabel);

        _authDetailLabel = Label(
            "Use the proven Step 06.1 flow once. If Steam sends a mobile Guard prompt, approve it in Steam and return here. The refresh token is saved only after LoggedOnCallback returns OK.",
            UIFont.SystemFontOfSize(15),
            UIColor.SecondaryLabel);
        content.AddArrangedSubview(_authDetailLabel);

        content.AddArrangedSubview(Separator());

        content.AddArrangedSubview(Label(
            "Relaunch / saved-session verification",
            UIFont.BoldSystemFontOfSize(25),
            UIColor.Label));

        _savedSessionLabel = Label(
            "Saved session: checking Keychain…",
            UIFont.BoldSystemFontOfSize(16),
            UIColor.Label);
        content.AddArrangedSubview(_savedSessionLabel);

        _resumeButton = SystemButton("Resume Saved Session (No Password)", 17);
        _resumeButton.TouchUpInside += async (_, _) => await RunSavedSessionResumeAsync();
        content.AddArrangedSubview(_resumeButton);

        _resumeResultLabel = Label(
            "SAVED SESSION: NOT RUN",
            UIFont.BoldSystemFontOfSize(21),
            UIColor.Label);
        content.AddArrangedSubview(_resumeResultLabel);

        _resumeDetailLabel = Label(
            "After the first login succeeds, force-close and reopen the app. A saved account should still be detected here. Resume must authenticate without password entry or a new Guard approval.",
            UIFont.SystemFontOfSize(15),
            UIColor.SecondaryLabel);
        content.AddArrangedSubview(_resumeDetailLabel);

        _signOutButton = SystemButton("Sign Out / Clear Saved Session", 16);
        _signOutButton.TouchUpInside += (_, _) => ClearSavedSession();
        content.AddArrangedSubview(_signOutButton);

        _cancelOperationButton = SystemButton("Cancel Current Steam Operation", 15);
        _cancelOperationButton.Enabled = false;
        _cancelOperationButton.TouchUpInside += (_, _) => _operationCts?.Cancel();
        content.AddArrangedSubview(_cancelOperationButton);

        content.AddArrangedSubview(Separator());

        _statusLabel = Label(
            "Status: Step 06.1 is proven. Step 06.2 is ready to test Keychain session persistence and password-free resume.",
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
        RefreshSavedSessionStatus();
        Console.WriteLine("Step 06.2: RootViewController.ViewDidLoad complete");
    }

    public void SetLifecycleState(string state)
    {
        _lifecycleActive = string.Equals(state, "Active", StringComparison.Ordinal);

        if (_lifecycleLabel is not null)
            _lifecycleLabel.Text = $"Lifecycle: {state}";
    }

    private async Task RunAuthenticationAsync()
    {
        if (_authResultLabel is null ||
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

        // Password remains runtime-only and is removed from the visible UIKit
        // control immediately when the attempt starts.
        _passwordField.Text = string.Empty;
        BeginSteamOperation();
        _authResultLabel.Text = "STEAM AUTH: RUNNING…";
        _authResultLabel.TextColor = UIColor.Label;
        _authDetailLabel.Text = "Starting persistent Steam auth. If mobile Steam Guard appears, approve it and return here…";
        _statusLabel.Text = "STEP 06.2 AUTH RUNNING — session is saved only after Steam logon succeeds.";

        try
        {
            var progress = new Progress<SteamAuthenticationProgress>(update =>
            {
                InvokeOnMainThread(() =>
                {
                    _authDetailLabel.Text = update.Message;
                    _statusLabel.Text = update.Stage switch
                    {
                        SteamAuthenticationStage.WaitingForMobileApproval =>
                            "WAITING FOR STEAM GUARD — approve the sign-in in Steam, then return here.",
                        SteamAuthenticationStage.MobileApprovalAccepted =>
                            "STEAM GUARD APPROVED — completing logon and Keychain persistence…",
                        _ => $"Step 06.2: {update.Message}",
                    };
                });
            });

            var result = await _authenticationAttempt.RunAsync(
                username,
                password,
                TimeSpan.FromMinutes(3),
                _operationCts!.Token,
                progress);

            InvokeOnMainThread(() =>
            {
                _authResultLabel.Text = result.Summary;
                _authResultLabel.TextColor = result.Outcome switch
                {
                    SteamAuthenticationOutcome.Authenticated => UIColor.Label,
                    SteamAuthenticationOutcome.GuardRequired => UIColor.SystemOrange,
                    SteamAuthenticationOutcome.Cancelled => UIColor.SecondaryLabel,
                    SteamAuthenticationOutcome.TimedOut => UIColor.SystemOrange,
                    _ => UIColor.SystemRed,
                };

                _authDetailLabel.Text = FormatAuthDetail(result);
                _statusLabel.Text = result.Outcome switch
                {
                    SteamAuthenticationOutcome.Authenticated when result.SessionPersisted =>
                        "PASS: persistent Steam session saved to the iOS Keychain. Force-close/relaunch, then run Resume Saved Session.",
                    SteamAuthenticationOutcome.GuardRequired =>
                        "BOUNDARY: Steam requested a code-based Guard method; manual code entry remains out of scope.",
                    SteamAuthenticationOutcome.TimedOut =>
                        "TIMEOUT: authentication did not complete; no new session was saved.",
                    SteamAuthenticationOutcome.Cancelled =>
                        "Authentication cancelled; no new session was saved.",
                    _ =>
                        "FAIL: Step 06.2 credential authentication or Keychain persistence failed.",
                };

                RefreshSavedSessionStatus();
            });
        }
        catch (Exception ex)
        {
            InvokeOnMainThread(() =>
            {
                _authResultLabel.Text = "STEAM AUTH: EXCEPTION";
                _authResultLabel.TextColor = UIColor.SystemRed;
                _authDetailLabel.Text = $"{ex.GetType().Name}: {ex.Message}";
                _statusLabel.Text = "FAIL: unhandled exception during Step 06.2 authentication/persistence.";
                RefreshSavedSessionStatus();
            });
        }
        finally
        {
            InvokeOnMainThread(EndSteamOperation);
        }
    }

    private async Task RunSavedSessionResumeAsync()
    {
        if (_resumeResultLabel is null || _resumeDetailLabel is null || _statusLabel is null)
            return;

        BeginSteamOperation();
        _resumeResultLabel.Text = "SAVED SESSION: RUNNING…";
        _resumeResultLabel.TextColor = UIColor.Label;
        _resumeDetailLabel.Text = "Reading the device-bound Keychain entry and logging on with the saved refresh token. No password or Guard code is requested by the launcher.";
        _statusLabel.Text = "STEP 06.2 RESUME RUNNING — password-free saved-session login.";

        try
        {
            var result = await _resumeAttempt.RunAsync(
                TimeSpan.FromSeconds(45),
                _operationCts!.Token);

            InvokeOnMainThread(() =>
            {
                _resumeResultLabel.Text = result.Summary;
                _resumeResultLabel.TextColor = result.Outcome switch
                {
                    SteamSessionResumeOutcome.Authenticated => UIColor.Label,
                    SteamSessionResumeOutcome.NoSavedSession => UIColor.SystemOrange,
                    SteamSessionResumeOutcome.Cancelled => UIColor.SecondaryLabel,
                    SteamSessionResumeOutcome.TimedOut => UIColor.SystemOrange,
                    _ => UIColor.SystemRed,
                };
                _resumeDetailLabel.Text = FormatResumeDetail(result);
                _statusLabel.Text = result.Outcome switch
                {
                    SteamSessionResumeOutcome.Authenticated =>
                        "PASS: saved Keychain session authenticated with matching Steam identity and no password/Guard prompt.",
                    SteamSessionResumeOutcome.NoSavedSession =>
                        "No saved Steam session exists. Authenticate + Save Session first.",
                    SteamSessionResumeOutcome.Rejected =>
                        "Saved token was rejected by Steam. Step 06.2 does not silently delete it; use Sign Out/Clear or authenticate again to replace it.",
                    SteamSessionResumeOutcome.TimedOut =>
                        "Saved-session resume timed out.",
                    SteamSessionResumeOutcome.Cancelled =>
                        "Saved-session resume cancelled.",
                    _ =>
                        "FAIL: saved-session resume failed.",
                };
                RefreshSavedSessionStatus();
            });
        }
        catch (Exception ex)
        {
            InvokeOnMainThread(() =>
            {
                _resumeResultLabel.Text = "SAVED SESSION: EXCEPTION";
                _resumeResultLabel.TextColor = UIColor.SystemRed;
                _resumeDetailLabel.Text = $"{ex.GetType().Name}: {ex.Message}";
                _statusLabel.Text = "FAIL: unhandled exception during saved-session resume.";
                RefreshSavedSessionStatus();
            });
        }
        finally
        {
            InvokeOnMainThread(EndSteamOperation);
        }
    }

    private void ClearSavedSession()
    {
        if (_statusLabel is null)
            return;

        try
        {
            var existed = _sessionStore.Clear();
            var stillExists = _sessionStore.Load() is not null;

            if (stillExists)
            {
                _statusLabel.Text = "FAIL: Keychain clear returned but the saved Steam session is still present.";
                _statusLabel.TextColor = UIColor.SystemRed;
            }
            else
            {
                _statusLabel.Text = existed
                    ? "PASS: signed out locally — saved Steam refresh token and identity were removed from Keychain."
                    : "PASS: no saved Steam session existed; Keychain remains clear.";
                _statusLabel.TextColor = UIColor.Label;
            }

            if (_resumeResultLabel is not null)
            {
                _resumeResultLabel.Text = "SAVED SESSION — cleared";
                _resumeResultLabel.TextColor = UIColor.Label;
            }

            if (_resumeDetailLabel is not null)
                _resumeDetailLabel.Text = "Relaunching now should show that no saved Steam session is available.";
        }
        catch (Exception ex)
        {
            _statusLabel.Text = $"FAIL: saved-session clear error: {ex.GetType().Name}: {ex.Message}";
            _statusLabel.TextColor = UIColor.SystemRed;
        }

        RefreshSavedSessionStatus();
    }

    private void RefreshSavedSessionStatus()
    {
        if (_savedSessionLabel is null)
            return;

        try
        {
            var saved = _sessionStore.Load();
            if (saved is null)
            {
                _savedSessionLabel.Text = "Saved session: NONE";
                _savedSessionLabel.TextColor = UIColor.SecondaryLabel;
                return;
            }

            _savedSessionLabel.Text =
                $"Saved session: YES\nAccount: {saved.AccountName}\nSteamID64: {saved.SteamId64}\nRefresh token: PRESENT (not displayed)";
            _savedSessionLabel.TextColor = UIColor.Label;
        }
        catch (Exception ex)
        {
            _savedSessionLabel.Text = $"Saved session: INVALID — {ex.GetType().Name}: {ex.Message}";
            _savedSessionLabel.TextColor = UIColor.SystemRed;
        }
    }

    private void BeginSteamOperation()
    {
        _operationCts?.Dispose();
        _operationCts = new CancellationTokenSource();

        if (_authButton is not null) _authButton.Enabled = false;
        if (_resumeButton is not null) _resumeButton.Enabled = false;
        if (_signOutButton is not null) _signOutButton.Enabled = false;
        if (_cancelOperationButton is not null) _cancelOperationButton.Enabled = true;
    }

    private void EndSteamOperation()
    {
        if (_authButton is not null) _authButton.Enabled = true;
        if (_resumeButton is not null) _resumeButton.Enabled = true;
        if (_signOutButton is not null) _signOutButton.Enabled = true;
        if (_cancelOperationButton is not null) _cancelOperationButton.Enabled = false;
    }

    private static string FormatAuthDetail(SteamAuthenticationResult result)
    {
        var lines = new List<string>
        {
            $"CM connected: {YesNo(result.CmConnected)}",
            $"Auth session started: {YesNo(result.AuthSessionStarted)}",
            $"Persistent auth requested: YES",
            $"Mobile approval requested: {YesNo(result.MobileApprovalRequested)}",
            $"Mobile approval completed: {YesNo(result.MobileApprovalCompleted)}",
            $"LoggedOnCallback: {YesNo(result.LoggedOnCallbackReceived)}",
            $"Logon result: {result.LogonResult?.ToString() ?? "N/A"}",
            $"Extended result: {result.ExtendedLogonResult?.ToString() ?? "N/A"}",
            $"Session persisted to Keychain: {YesNo(result.SessionPersisted)}",
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

        lines.Add("Password persistence: NONE");
        lines.Add("Steam Guard secret/code persistence: NONE");
        lines.Add("Refresh token display/logging: NONE");
        lines.Add("Ownership request: NOT RUN");
        return string.Join("\n", lines);
    }

    private static string FormatResumeDetail(SteamSessionResumeResult result)
    {
        var lines = new List<string>
        {
            $"Saved session found: {YesNo(result.SavedSessionFound)}",
            $"CM connected: {YesNo(result.CmConnected)}",
            $"LoggedOnCallback: {YesNo(result.LoggedOnCallbackReceived)}",
            $"Logon result: {result.LogonResult?.ToString() ?? "N/A"}",
            $"Extended result: {result.ExtendedLogonResult?.ToString() ?? "N/A"}",
            $"Stored/returned identity match: {YesNo(result.IdentityMatched)}",
            $"Account name: {result.AccountName ?? "not-returned"}",
            $"SteamID64: {result.SteamId64 ?? "not-returned"}",
            $"CurrentEndPoint: {result.CurrentEndPoint ?? "never-set"}",
            $"Elapsed: {result.Elapsed.TotalSeconds:F1}s",
        };

        if (!string.IsNullOrWhiteSpace(result.Error))
            lines.Add($"Error: {result.Error}");

        lines.Add("Password used: NO");
        lines.Add("New Steam Guard approval requested by launcher: NO");
        lines.Add("Refresh token display/logging: NONE");
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
                    : "FAIL: a proven foundation regression failed; stop Step 06.2 work until understood.";
                _statusLabel.TextColor = final.Passed ? UIColor.Label : UIColor.SystemRed;
                RefreshSavedSessionStatus();
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
                _statusLabel.TextColor = UIColor.SystemRed;
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
