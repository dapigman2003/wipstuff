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
    private readonly SteamOwnershipVerificationAttempt _ownershipAttempt;

    private UILabel? _foundationResultLabel;
    private UILabel? _foundationDetailLabel;
    private UILabel? _authResultLabel;
    private UILabel? _authDetailLabel;
    private UILabel? _savedSessionLabel;
    private UILabel? _autoRestoreResultLabel;
    private UILabel? _autoRestoreDetailLabel;
    private UILabel? _resumeResultLabel;
    private UILabel? _resumeDetailLabel;
    private UILabel? _ownershipResultLabel;
    private UILabel? _ownershipDetailLabel;
    private UILabel? _statusLabel;
    private UILabel? _lifecycleLabel;
    private UITextField? _usernameField;
    private UITextField? _passwordField;
    private UIButton? _foundationButton;
    private UIButton? _authButton;
    private UIButton? _resumeButton;
    private UIButton? _ownershipButton;
    private UIButton? _signOutButton;
    private UIButton? _cancelOperationButton;
    private CancellationTokenSource? _operationCts;
    private bool _uiStartupPassed;
    private bool _lifecycleActive;
    private bool _automaticRestoreStarted;

    public RootViewController()
    {
        _keychainProbe = new KeychainProbe(_credentialStore);
        _sessionStore = new SteamSessionStore(_credentialStore);
        _authenticationAttempt = new SteamAuthenticationAttempt(_sessionStore);
        _resumeAttempt = new SteamSessionResumeAttempt(_sessionStore);
        _ownershipAttempt = new SteamOwnershipVerificationAttempt(_sessionStore);
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
            "STEP 07 — OWNERSHIP VERIFICATION",
            UIFont.BoldSystemFontOfSize(18),
            UIColor.SecondaryLabel));

        content.AddArrangedSubview(Label(
            "Version 0.0.28",
            UIFont.SystemFontOfSize(17),
            UIColor.SecondaryLabel));

        content.AddArrangedSubview(Label(
            "APP ID 2868840 • OWNERSHIP TICKET ONLY • NO DOWNLOAD",
            UIFont.BoldSystemFontOfSize(14),
            UIColor.SecondaryLabel));

        content.AddArrangedSubview(Label(
            "Step 07 keeps the completed authentication/session foundation and adds one new boundary: after password-free saved-session logon with matching Steam identity, request a Steam app ownership ticket for Slay the Spire 2 (App ID 2868840). Ownership is accepted only when Steam returns OK for the exact AppID with a non-empty ticket. Ticket bytes are never displayed, logged, or persisted. No PICS, depot, manifest, CDN, or download request is made.",
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
            "Saved-session diagnostics / manual retry",
            UIFont.BoldSystemFontOfSize(25),
            UIColor.Label));

        _savedSessionLabel = Label(
            "Saved session: checking Keychain…",
            UIFont.BoldSystemFontOfSize(16),
            UIColor.Label);
        content.AddArrangedSubview(_savedSessionLabel);

        _autoRestoreResultLabel = Label(
            "AUTO SESSION: WAITING FOR ACTIVE LIFECYCLE",
            UIFont.BoldSystemFontOfSize(21),
            UIColor.Label);
        content.AddArrangedSubview(_autoRestoreResultLabel);

        _autoRestoreDetailLabel = Label(
            "The proven 06.3.1 saved-session regression automatically tests the Keychain session once after launch. No password or new Guard prompt is used.",
            UIFont.SystemFontOfSize(15),
            UIColor.SecondaryLabel);
        content.AddArrangedSubview(_autoRestoreDetailLabel);

        _resumeButton = SystemButton("Retry Saved Session Now (No Password)", 17);
        _resumeButton.TouchUpInside += async (_, _) => await RunSavedSessionResumeAsync();
        content.AddArrangedSubview(_resumeButton);

        _resumeResultLabel = Label(
            "SAVED SESSION: NOT RUN",
            UIFont.BoldSystemFontOfSize(21),
            UIColor.Label);
        content.AddArrangedSubview(_resumeResultLabel);

        _resumeDetailLabel = Label(
            "Manual retry now uses a fresh LoginID and the same persistent-token settings as automatic restore. AccessDenied remains non-destructive unless Steam reports a definitive expired/revoked credential.",
            UIFont.SystemFontOfSize(15),
            UIColor.SecondaryLabel);
        content.AddArrangedSubview(_resumeDetailLabel);

        content.AddArrangedSubview(Separator());

        content.AddArrangedSubview(Label(
            "Step 07 — Slay the Spire 2 ownership",
            UIFont.BoldSystemFontOfSize(25),
            UIColor.Label));

        _ownershipButton = SystemButton("Verify Slay the Spire 2 Ownership", 17);
        _ownershipButton.TouchUpInside += async (_, _) => await RunOwnershipVerificationAsync();
        content.AddArrangedSubview(_ownershipButton);

        _ownershipResultLabel = Label(
            "OWNERSHIP: NOT RUN",
            UIFont.BoldSystemFontOfSize(21),
            UIColor.Label);
        content.AddArrangedSubview(_ownershipResultLabel);

        _ownershipDetailLabel = Label(
            "Uses only the saved Keychain session and SteamApps.GetAppOwnershipTicket for App ID 2868840. A non-empty OK ticket proves this account owns the target app. The ticket payload is discarded immediately; no content request follows.",
            UIFont.SystemFontOfSize(15),
            UIColor.SecondaryLabel);
        content.AddArrangedSubview(_ownershipDetailLabel);

        _signOutButton = SystemButton("Sign Out / Clear Saved Session", 16);
        _signOutButton.TouchUpInside += (_, _) => ClearSavedSession();
        content.AddArrangedSubview(_signOutButton);

        _cancelOperationButton = SystemButton("Cancel Current Steam Operation", 15);
        _cancelOperationButton.Enabled = false;
        _cancelOperationButton.TouchUpInside += (_, _) => _operationCts?.Cancel();
        content.AddArrangedSubview(_cancelOperationButton);

        content.AddArrangedSubview(Separator());

        _statusLabel = Label(
            "Status: Steps 01–06.3.1 are proven. Step 07 is ready to verify ownership of Slay the Spire 2 (App ID 2868840) without requesting any depot, manifest, or file.",
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
        Console.WriteLine("Step 07: RootViewController.ViewDidLoad complete");
    }

    public void SetLifecycleState(string state)
    {
        _lifecycleActive = string.Equals(state, "Active", StringComparison.Ordinal);

        if (_lifecycleLabel is not null)
            _lifecycleLabel.Text = $"Lifecycle: {state}";

        if (_lifecycleActive && !_automaticRestoreStarted)
        {
            _automaticRestoreStarted = true;
            _ = RunAutomaticSessionRestoreAsync();
        }
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
        _statusLabel.Text = "STEP 06.3.1 AUTH RUNNING — requesting a persistent session and saving only after Steam logon succeeds.";

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
                        _ => $"Step 07 auth regression: {update.Message}",
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
                        "PASS: persistent Steam session saved to the iOS Keychain. On the next launch, the saved-session regression will attempt it automatically.",
                    SteamAuthenticationOutcome.GuardRequired =>
                        "BOUNDARY: Steam requested a code-based Guard method; manual code entry remains out of scope.",
                    SteamAuthenticationOutcome.TimedOut =>
                        "TIMEOUT: authentication did not complete; no new session was saved.",
                    SteamAuthenticationOutcome.Cancelled =>
                        "Authentication cancelled; no new session was saved.",
                    _ =>
                        "FAIL: credential authentication or Keychain persistence failed.",
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
                _statusLabel.Text = "FAIL: unhandled exception during authentication/persistence.";
                RefreshSavedSessionStatus();
            });
        }
        finally
        {
            InvokeOnMainThread(EndSteamOperation);
        }
    }

    private async Task RunAutomaticSessionRestoreAsync()
    {
        if (_autoRestoreResultLabel is null ||
            _autoRestoreDetailLabel is null ||
            _statusLabel is null)
        {
            return;
        }

        BeginSteamOperation();
        _autoRestoreResultLabel.Text = "AUTO SESSION: RUNNING…";
        _autoRestoreResultLabel.TextColor = UIColor.Label;
        _autoRestoreDetailLabel.Text =
            "Active lifecycle reached. Reading the saved Keychain session and attempting password-free Steam logon…";
        _statusLabel.Text = "STEP 06.3.1 AUTO-RESTORE RUNNING — persistent token + fresh LoginID; no password or new Guard flow.";

        try
        {
            var result = await _resumeAttempt.RunAsync(
                TimeSpan.FromSeconds(45),
                _operationCts!.Token);

            var recoveryAction = SteamSessionRecoveryPolicy.Evaluate(result);
            var sessionCleared = false;
            string? recoveryError = null;

            if (recoveryAction == SteamSessionRecoveryAction.ClearSavedSessionAndRequireInteractiveAuthentication)
            {
                try
                {
                    _sessionStore.Clear();
                    sessionCleared = _sessionStore.Load() is null;
                    if (!sessionCleared)
                        recoveryError = "Keychain clear returned but the saved session is still present.";
                }
                catch (Exception ex)
                {
                    recoveryError = $"Keychain recovery clear failed: {ex.GetType().Name}: {ex.Message}";
                }
            }

            InvokeOnMainThread(() =>
            {
                _autoRestoreResultLabel.Text = result.Outcome switch
                {
                    SteamSessionResumeOutcome.Authenticated => "AUTO SESSION PASS — authenticated",
                    SteamSessionResumeOutcome.NoSavedSession => "AUTO SESSION — signed out",
                    SteamSessionResumeOutcome.Rejected when sessionCleared => "AUTO SESSION RESET — rejected token cleared",
                    SteamSessionResumeOutcome.InvalidLocalSession when sessionCleared => "AUTO SESSION RESET — invalid record cleared",
                    SteamSessionResumeOutcome.IdentityMismatch when sessionCleared => "AUTO SESSION RESET — identity mismatch cleared",
                    SteamSessionResumeOutcome.TimedOut => "AUTO SESSION — timeout; saved session preserved",
                    SteamSessionResumeOutcome.Cancelled => "AUTO SESSION — cancelled; saved session preserved",
                    SteamSessionResumeOutcome.Rejected => "AUTO SESSION — rejected; saved session preserved",
                    _ => "AUTO SESSION FAIL — saved session preserved",
                };

                _autoRestoreResultLabel.TextColor = result.Outcome switch
                {
                    SteamSessionResumeOutcome.Authenticated => UIColor.Label,
                    SteamSessionResumeOutcome.NoSavedSession => UIColor.SecondaryLabel,
                    SteamSessionResumeOutcome.TimedOut => UIColor.SystemOrange,
                    SteamSessionResumeOutcome.Cancelled => UIColor.SecondaryLabel,
                    _ when sessionCleared && recoveryError is null => UIColor.SystemOrange,
                    _ => UIColor.SystemRed,
                };

                var details = new List<string>
                {
                    FormatResumeDetail(result),
                    $"Automatic launch restore: YES",
                    $"Recovery policy: {recoveryAction}",
                    $"Saved session cleared by recovery: {YesNo(sessionCleared)}",
                };

                if (!string.IsNullOrWhiteSpace(recoveryError))
                    details.Add($"Recovery error: {recoveryError}");

                _autoRestoreDetailLabel.Text = string.Join("\n", details);

                if (!string.IsNullOrWhiteSpace(result.AccountName) && _usernameField is not null)
                    _usernameField.Text = result.AccountName;

                _statusLabel.Text = result.Outcome switch
                {
                    SteamSessionResumeOutcome.Authenticated =>
                        "PASS: saved Steam session restored automatically on launch with matching identity and no password/Guard prompt.",
                    SteamSessionResumeOutcome.NoSavedSession =>
                        "SIGNED OUT: no saved Steam session exists. Use Authenticate + Save Session when needed.",
                    _ when recoveryError is not null =>
                        $"FAIL: recovery policy selected clear, but Keychain cleanup failed: {recoveryError}",
                    _ when sessionCleared =>
                        "RECOVERED: unusable/unsafe saved session was removed. Interactive Steam authentication is required again.",
                    SteamSessionResumeOutcome.TimedOut =>
                        "TRANSIENT: automatic resume timed out; saved session was preserved for retry.",
                    SteamSessionResumeOutcome.Cancelled =>
                        "Automatic resume cancelled; saved session was preserved.",
                    SteamSessionResumeOutcome.Rejected =>
                        "Steam rejected this resume with a non-definitive result; saved session was preserved for retry rather than destroyed.",
                    _ =>
                        "Automatic resume failed without evidence that the credential is invalid; saved session was preserved.",
                };

                _statusLabel.TextColor = result.Outcome == SteamSessionResumeOutcome.Authenticated ||
                                         result.Outcome == SteamSessionResumeOutcome.NoSavedSession ||
                                         sessionCleared
                    ? UIColor.Label
                    : result.Outcome is SteamSessionResumeOutcome.TimedOut or SteamSessionResumeOutcome.Cancelled
                        ? UIColor.SystemOrange
                        : UIColor.SystemRed;

                RefreshSavedSessionStatus();
            });
        }
        catch (Exception ex)
        {
            InvokeOnMainThread(() =>
            {
                _autoRestoreResultLabel.Text = "AUTO SESSION: EXCEPTION";
                _autoRestoreResultLabel.TextColor = UIColor.SystemRed;
                _autoRestoreDetailLabel.Text =
                    $"{ex.GetType().Name}: {ex.Message}\nSaved session was not cleared because no definitive invalid-session result was obtained.";
                _statusLabel.Text = "FAIL: unhandled exception during Step 07 automatic session restore regression.";
                _statusLabel.TextColor = UIColor.SystemRed;
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
        _statusLabel.Text = "STEP 06.3.1 MANUAL RESUME RUNNING — persistent token + fresh LoginID; no password.";

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
                        "Saved token was rejected by Steam. AccessDenied and other non-definitive results are preserved; only explicit expired/revoked/invalid credentials are cleared.",
                    SteamSessionResumeOutcome.InvalidLocalSession =>
                        "Saved Keychain record is invalid. Automatic recovery would clear it and require interactive authentication.",
                    SteamSessionResumeOutcome.IdentityMismatch =>
                        "SECURITY: saved session authenticated as a different SteamID. Automatic recovery clears it and requires interactive authentication.",
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

    private async Task RunOwnershipVerificationAsync()
    {
        if (_ownershipResultLabel is null || _ownershipDetailLabel is null || _statusLabel is null)
            return;

        BeginSteamOperation();
        _ownershipResultLabel.Text = "OWNERSHIP: RUNNING…";
        _ownershipResultLabel.TextColor = UIColor.Label;
        _ownershipDetailLabel.Text =
            "Authenticating with the saved Keychain refresh token, verifying the stored SteamID, then requesting one ownership ticket for App ID 2868840…";
        _statusLabel.Text = "STEP 07 RUNNING — ownership ticket only; no PICS/depot/manifest/CDN/download request.";
        _statusLabel.TextColor = UIColor.Label;

        try
        {
            var result = await _ownershipAttempt.RunAsync(
                TimeSpan.FromSeconds(45),
                _operationCts!.Token);

            InvokeOnMainThread(() =>
            {
                _ownershipResultLabel.Text = result.Summary;
                _ownershipResultLabel.TextColor = result.Outcome switch
                {
                    SteamOwnershipVerificationOutcome.Owned => UIColor.Label,
                    SteamOwnershipVerificationOutcome.NoSavedSession => UIColor.SystemOrange,
                    SteamOwnershipVerificationOutcome.Cancelled => UIColor.SecondaryLabel,
                    SteamOwnershipVerificationOutcome.TimedOut => UIColor.SystemOrange,
                    _ => UIColor.SystemRed,
                };

                _ownershipDetailLabel.Text = FormatOwnershipDetail(result);
                _statusLabel.Text = result.Outcome switch
                {
                    SteamOwnershipVerificationOutcome.Owned =>
                        "PASS: Steam issued a non-empty ownership ticket for Slay the Spire 2 App ID 2868840. No content/download request was made.",
                    SteamOwnershipVerificationOutcome.NoSavedSession =>
                        "No saved Steam session exists. Authenticate + Save Session first, then retry Step 07.",
                    SteamOwnershipVerificationOutcome.SessionRejected =>
                        "Saved Steam session was rejected. Reauthenticate if needed; Step 07 did not request content.",
                    SteamOwnershipVerificationOutcome.IdentityMismatch =>
                        "SECURITY: saved session returned a different SteamID. Do not trust this ownership result.",
                    SteamOwnershipVerificationOutcome.TicketRejected =>
                        "Steam did not issue an OK ownership ticket. Ownership is not proven; no download was attempted.",
                    SteamOwnershipVerificationOutcome.EmptyTicket =>
                        "Steam returned OK but no ticket bytes. Ownership is not proven; stop before content work.",
                    SteamOwnershipVerificationOutcome.UnexpectedAppId =>
                        "Steam returned an ownership callback for an unexpected AppID. Ownership is not proven.",
                    SteamOwnershipVerificationOutcome.TimedOut =>
                        "Ownership verification timed out. No download was attempted.",
                    SteamOwnershipVerificationOutcome.Cancelled =>
                        "Ownership verification cancelled. No download was attempted.",
                    _ =>
                        "FAIL: Step 07 ownership verification did not complete. No content request was made.",
                };
                _statusLabel.TextColor = result.Outcome == SteamOwnershipVerificationOutcome.Owned
                    ? UIColor.Label
                    : result.Outcome is SteamOwnershipVerificationOutcome.TimedOut or SteamOwnershipVerificationOutcome.Cancelled or SteamOwnershipVerificationOutcome.NoSavedSession
                        ? UIColor.SystemOrange
                        : UIColor.SystemRed;
            });
        }
        catch (Exception ex)
        {
            InvokeOnMainThread(() =>
            {
                _ownershipResultLabel.Text = "OWNERSHIP: EXCEPTION";
                _ownershipResultLabel.TextColor = UIColor.SystemRed;
                _ownershipDetailLabel.Text = $"{ex.GetType().Name}: {ex.Message}";
                _statusLabel.Text = "FAIL: unhandled exception during Step 07 ownership verification. No content request was made.";
                _statusLabel.TextColor = UIColor.SystemRed;
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
                _resumeDetailLabel.Text = "Relaunching now should automatically report AUTO SESSION — signed out because no saved Steam session is available.";
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

            var tokenTiming = SteamRefreshTokenMetadata.TryParse(saved.RefreshToken, out var metadata) &&
                              metadata is not null
                ? $"\nRefresh token expires (UTC): {FormatUtc(metadata.ExpiresAtUtc)}" +
                  $"\nRefresh token expired now: {YesNo(metadata.IsExpiredAt(DateTimeOffset.UtcNow))}"
                : "\nRefresh token timing: unavailable";

            _savedSessionLabel.Text =
                $"Saved session: YES\nAccount: {saved.AccountName}\nSteamID64: {saved.SteamId64}\nRefresh token: PRESENT (not displayed){tokenTiming}";
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

        if (_foundationButton is not null) _foundationButton.Enabled = false;
        if (_authButton is not null) _authButton.Enabled = false;
        if (_resumeButton is not null) _resumeButton.Enabled = false;
        if (_ownershipButton is not null) _ownershipButton.Enabled = false;
        if (_signOutButton is not null) _signOutButton.Enabled = false;
        if (_cancelOperationButton is not null) _cancelOperationButton.Enabled = true;
    }

    private void EndSteamOperation()
    {
        if (_foundationButton is not null) _foundationButton.Enabled = true;
        if (_authButton is not null) _authButton.Enabled = true;
        if (_resumeButton is not null) _resumeButton.Enabled = true;
        if (_ownershipButton is not null) _ownershipButton.Enabled = true;
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
            $"ShouldRememberPassword: YES",
            $"LoginID: {result.LoginId?.ToString() ?? "not-set"}",
            $"Refresh token expires (UTC): {FormatUtc(result.RefreshTokenExpiresAtUtc)}",
            $"Refresh token expired at attempt: {YesNoNullable(result.RefreshTokenExpiredAtAttempt)}",
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
        lines.Add("Ownership request in this operation: NOT RUN");
        return string.Join("\n", lines);
    }

    private static string FormatResumeDetail(SteamSessionResumeResult result)
    {
        var recoveryAction = SteamSessionRecoveryPolicy.Evaluate(result);
        var lines = new List<string>
        {
            $"Outcome: {result.Outcome}",
            $"Recovery action: {recoveryAction}",
            $"Saved session found: {YesNo(result.SavedSessionFound)}",
            $"CM connected: {YesNo(result.CmConnected)}",
            $"LoggedOnCallback: {YesNo(result.LoggedOnCallbackReceived)}",
            $"Logon result: {result.LogonResult?.ToString() ?? "N/A"}",
            $"Extended result: {result.ExtendedLogonResult?.ToString() ?? "N/A"}",
            $"ShouldRememberPassword: YES",
            $"LoginID: {result.LoginId?.ToString() ?? "not-set"}",
            $"Refresh token expires (UTC): {FormatUtc(result.RefreshTokenExpiresAtUtc)}",
            $"Refresh token expired at attempt: {YesNoNullable(result.RefreshTokenExpiredAtAttempt)}",
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
        lines.Add("Ownership request in this operation: NOT RUN");
        return string.Join("\n", lines);
    }

    private static string FormatOwnershipDetail(SteamOwnershipVerificationResult result)
    {
        var lines = new List<string>
        {
            $"Target AppID: {result.TargetAppId}",
            $"Saved session found: {YesNo(result.SavedSessionFound)}",
            $"CM connected: {YesNo(result.CmConnected)}",
            $"LoggedOnCallback: {YesNo(result.LoggedOnCallbackReceived)}",
            $"Logon result: {result.LogonResult?.ToString() ?? "N/A"}",
            $"Extended result: {result.ExtendedLogonResult?.ToString() ?? "N/A"}",
            $"Stored/returned identity match: {YesNo(result.IdentityMatched)}",
            $"Ownership callback: {YesNo(result.OwnershipTicketCallbackReceived)}",
            $"Ownership result: {result.OwnershipResult?.ToString() ?? "N/A"}",
            $"Ownership callback AppID: {result.OwnershipAppId?.ToString() ?? "N/A"}",
            $"Ownership ticket bytes: {result.OwnershipTicketLength}",
            $"Ownership proven: {YesNo(result.OwnershipProven)}",
            $"Account name: {result.AccountName ?? "not-returned"}",
            $"SteamID64: {result.SteamId64 ?? "not-returned"}",
            $"LoginID: {result.LoginId?.ToString() ?? "not-set"}",
            $"CurrentEndPoint: {result.CurrentEndPoint ?? "never-set"}",
            $"Elapsed: {result.Elapsed.TotalSeconds:F1}s",
        };

        if (!string.IsNullOrWhiteSpace(result.Error))
            lines.Add($"Error: {result.Error}");

        lines.Add("Ownership ticket payload display/logging/persistence: NONE");
        lines.Add("PICS request: NOT RUN");
        lines.Add("Depot/manifest/CDN/download request: NOT RUN");
        return string.Join("\n", lines);
    }

    private static string FormatUtc(DateTimeOffset? value) =>
        value?.UtcDateTime.ToString("yyyy-MM-dd HH:mm:ss 'UTC'") ?? "unavailable";

    private static string YesNoNullable(bool? value) =>
        value.HasValue ? YesNo(value.Value) : "unknown";

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
                    : "FAIL: a proven foundation regression failed; stop Step 07 work until understood.";
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
