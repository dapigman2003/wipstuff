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
    private readonly SteamContentDiscoveryAttempt _contentDiscoveryAttempt;
    private readonly SteamSingleFileDownloadAttempt _singleFileDownloadAttempt;

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
    private UILabel? _discoveryResultLabel;
    private UILabel? _discoveryDetailLabel;
    private UILabel? _singleFileResultLabel;
    private UILabel? _singleFileDetailLabel;
    private UILabel? _statusLabel;
    private UILabel? _lifecycleLabel;
    private UITextField? _usernameField;
    private UITextField? _passwordField;
    private UIButton? _foundationButton;
    private UIButton? _authButton;
    private UIButton? _resumeButton;
    private UIButton? _ownershipButton;
    private UIButton? _discoveryButton;
    private UIButton? _singleFileButton;
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
        _contentDiscoveryAttempt = new SteamContentDiscoveryAttempt(_sessionStore);
        var documentsRoot = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        _singleFileDownloadAttempt = new SteamSingleFileDownloadAttempt(
            _sessionStore,
            Path.Combine(documentsRoot, "StS2Launcher"));
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
            "STEP 09 — ONE CONTROLLED SMALL FILE",
            UIFont.BoldSystemFontOfSize(18),
            UIColor.SecondaryLabel));

        content.AddArrangedSubview(Label(
            "Version 0.0.30",
            UIFont.SystemFontOfSize(17),
            UIColor.SecondaryLabel));

        content.AddArrangedSubview(Label(
            "APP ID 2868840 • ONE FILE ≤ 2 MiB • SHA-1 VERIFIED",
            UIFont.BoldSystemFontOfSize(14),
            UIColor.SecondaryLabel));

        content.AddArrangedSubview(Label(
            "Step 09 preserves the proven Step 08 discovery path, then adds one tightly bounded content-access test. It re-proves the saved session and App ID 2868840 ownership, re-discovers direct public depots, prefers a macOS depot when available, downloads one manifest in memory, selects the smallest safe regular file no larger than 2 MiB, downloads only that file's chunks, verifies the assembled SHA-1 against Steam's manifest, and atomically writes only that verified file. No full-depot queue, resume, install/update, repair, Godot, Cloud, or Workshop work is included.",
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
            "Step 07 regression — Slay the Spire 2 ownership",
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

        content.AddArrangedSubview(Separator());

        content.AddArrangedSubview(Label(
            "Step 08 — depot / manifest discovery",
            UIFont.BoldSystemFontOfSize(25),
            UIColor.Label));

        _discoveryButton = SystemButton("Discover StS2 Depots + Manifests", 17);
        _discoveryButton.TouchUpInside += async (_, _) => await RunContentDiscoveryAsync();
        content.AddArrangedSubview(_discoveryButton);

        _discoveryResultLabel = Label(
            "DISCOVERY: NOT RUN",
            UIFont.BoldSystemFontOfSize(21),
            UIColor.Label);
        content.AddArrangedSubview(_discoveryResultLabel);

        _discoveryDetailLabel = Label(
            "Metadata only: after the Step 07 ownership gate, request PICS app info for App ID 2868840 and list numeric depot IDs plus visible branch manifest IDs. No depot key, manifest body, CDN request, chunk, or file download is allowed in this step.",
            UIFont.SystemFontOfSize(15),
            UIColor.SecondaryLabel);
        content.AddArrangedSubview(_discoveryDetailLabel);

        content.AddArrangedSubview(Separator());

        content.AddArrangedSubview(Label(
            "Step 09 — one controlled small StS2 file",
            UIFont.BoldSystemFontOfSize(25),
            UIColor.Label));

        _singleFileButton = SystemButton("Download One Small StS2 File", 17);
        _singleFileButton.TouchUpInside += async (_, _) => await RunSingleFileDownloadAsync();
        content.AddArrangedSubview(_singleFileButton);

        _singleFileResultLabel = Label(
            "SINGLE-FILE: NOT RUN",
            UIFont.BoldSystemFontOfSize(21),
            UIColor.Label);
        content.AddArrangedSubview(_singleFileResultLabel);

        _singleFileDetailLabel = Label(
            "Bounded content test only: one direct public depot, one in-memory manifest, one safe regular file <= 2 MiB, SHA-1 verification, then one atomic Documents write. Depot keys, request codes, CDN auth tokens, manifest bytes and chunk buffers are never displayed or persisted.",
            UIFont.SystemFontOfSize(15),
            UIColor.SecondaryLabel);
        content.AddArrangedSubview(_singleFileDetailLabel);

        _signOutButton = SystemButton("Sign Out / Clear Saved Session", 16);
        _signOutButton.TouchUpInside += (_, _) => ClearSavedSession();
        content.AddArrangedSubview(_signOutButton);

        _cancelOperationButton = SystemButton("Cancel Current Steam Operation", 15);
        _cancelOperationButton.Enabled = false;
        _cancelOperationButton.TouchUpInside += (_, _) => _operationCts?.Cancel();
        content.AddArrangedSubview(_cancelOperationButton);

        content.AddArrangedSubview(Separator());

        _statusLabel = Label(
            "Status: Step 08 passed on the physical iPhone. Step 09 is ready to download exactly one small StS2 file after re-proving the saved session, ownership, and discovery gates.",
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
        Console.WriteLine("Step 09: RootViewController.ViewDidLoad complete");
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
                        _ => $"Step 08 auth regression: {update.Message}",
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
                _statusLabel.Text = "FAIL: unhandled exception during Step 08 automatic session restore regression.";
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

    private async Task RunContentDiscoveryAsync()
    {
        if (_discoveryResultLabel is null || _discoveryDetailLabel is null || _statusLabel is null)
            return;

        BeginSteamOperation();
        _discoveryResultLabel.Text = "DISCOVERY: RUNNING…";
        _discoveryResultLabel.TextColor = UIColor.Label;
        _discoveryDetailLabel.Text =
            "Restoring the saved session, re-proving App ID 2868840 ownership, then requesting PICS access metadata + product info only…";
        _statusLabel.Text =
            "STEP 08 RUNNING — PICS metadata only; no depot key, manifest body, CDN server/token, chunk, or file request.";
        _statusLabel.TextColor = UIColor.Label;

        try
        {
            var result = await _contentDiscoveryAttempt.RunAsync(
                TimeSpan.FromSeconds(60),
                _operationCts!.Token);

            InvokeOnMainThread(() =>
            {
                _discoveryResultLabel.Text = result.Summary;
                _discoveryResultLabel.TextColor = result.Outcome switch
                {
                    SteamContentDiscoveryOutcome.Discovered => UIColor.Label,
                    SteamContentDiscoveryOutcome.NoSavedSession => UIColor.SystemOrange,
                    SteamContentDiscoveryOutcome.Cancelled => UIColor.SecondaryLabel,
                    SteamContentDiscoveryOutcome.TimedOut => UIColor.SystemOrange,
                    _ => UIColor.SystemRed,
                };
                _discoveryDetailLabel.Text = FormatContentDiscoveryDetail(result);
                _statusLabel.Text = result.Outcome switch
                {
                    SteamContentDiscoveryOutcome.Discovered =>
                        $"PASS: PICS exposed {result.DepotCount} depot(s) and {result.ManifestCount} visible branch manifest ID(s) for App ID 2868840. Metadata only; nothing was downloaded.",
                    SteamContentDiscoveryOutcome.NoSavedSession =>
                        "No saved Steam session exists. Authenticate + Save Session first, then retry Step 08.",
                    SteamContentDiscoveryOutcome.SessionRejected =>
                        "Saved Steam session was rejected before discovery. Reauthenticate; no content bytes were requested.",
                    SteamContentDiscoveryOutcome.IdentityMismatch =>
                        "SECURITY: saved session returned a different SteamID. Discovery stopped before PICS product info.",
                    SteamContentDiscoveryOutcome.OwnershipNotProven =>
                        "Step 07 ownership could not be re-proven. Discovery stopped before PICS product info.",
                    SteamContentDiscoveryOutcome.PicsAccessTokenDenied =>
                        "Steam denied the PICS app access token. No manifest body or CDN request was attempted.",
                    SteamContentDiscoveryOutcome.ProductInfoUnavailable =>
                        "PICS did not return product info for App ID 2868840.",
                    SteamContentDiscoveryOutcome.MissingPicsToken =>
                        "PICS app info says a required access token is still missing; discovery is not proven.",
                    SteamContentDiscoveryOutcome.NoDepots =>
                        "PICS app info returned, but no numeric depot entries were found.",
                    SteamContentDiscoveryOutcome.NoVisibleManifests =>
                        "Depot metadata returned, but no visible branch manifest IDs were found. No manifest body was requested.",
                    SteamContentDiscoveryOutcome.TimedOut =>
                        "Step 08 discovery timed out; no download was attempted.",
                    SteamContentDiscoveryOutcome.Cancelled =>
                        "Step 08 discovery cancelled; no download was attempted.",
                    _ =>
                        "FAIL: Step 08 depot/manifest discovery did not complete. No download was attempted.",
                };
                _statusLabel.TextColor = result.Outcome == SteamContentDiscoveryOutcome.Discovered
                    ? UIColor.Label
                    : result.Outcome is SteamContentDiscoveryOutcome.TimedOut or SteamContentDiscoveryOutcome.Cancelled or SteamContentDiscoveryOutcome.NoSavedSession
                        ? UIColor.SystemOrange
                        : UIColor.SystemRed;
            });
        }
        catch (Exception ex)
        {
            InvokeOnMainThread(() =>
            {
                _discoveryResultLabel.Text = "DISCOVERY: EXCEPTION";
                _discoveryResultLabel.TextColor = UIColor.SystemRed;
                _discoveryDetailLabel.Text = $"{ex.GetType().Name}: {ex.Message}";
                _statusLabel.Text = "FAIL: unhandled exception during Step 08 metadata discovery. No download was attempted.";
                _statusLabel.TextColor = UIColor.SystemRed;
            });
        }
        finally
        {
            InvokeOnMainThread(EndSteamOperation);
        }
    }

    private async Task RunSingleFileDownloadAsync()
    {
        if (_singleFileResultLabel is null || _singleFileDetailLabel is null || _statusLabel is null)
            return;

        BeginSteamOperation();
        _singleFileResultLabel.Text = "SINGLE-FILE: RUNNING…";
        _singleFileResultLabel.TextColor = UIColor.Label;
        _singleFileDetailLabel.Text =
            "Re-proving saved-session identity, Step 07 ownership and Step 08 PICS metadata; then downloading one direct public manifest and one <= 2 MiB file only…";
        _statusLabel.Text =
            "STEP 09 RUNNING — exactly one bounded file; no full-depot queue, resume, install, update, repair, Godot, Cloud, or Workshop operation.";
        _statusLabel.TextColor = UIColor.Label;

        try
        {
            var result = await _singleFileDownloadAttempt.RunAsync(
                TimeSpan.FromSeconds(120),
                _operationCts!.Token);

            InvokeOnMainThread(() =>
            {
                _singleFileResultLabel.Text = result.Summary;
                _singleFileResultLabel.TextColor = result.Outcome switch
                {
                    SteamSingleFileDownloadOutcome.Downloaded => UIColor.Label,
                    SteamSingleFileDownloadOutcome.NoSavedSession => UIColor.SystemOrange,
                    SteamSingleFileDownloadOutcome.Cancelled => UIColor.SecondaryLabel,
                    SteamSingleFileDownloadOutcome.TimedOut => UIColor.SystemOrange,
                    _ => UIColor.SystemRed,
                };
                _singleFileDetailLabel.Text = FormatSingleFileDownloadDetail(result);
                _statusLabel.Text = result.Outcome switch
                {
                    SteamSingleFileDownloadOutcome.Downloaded =>
                        $"PASS: one verified StS2 file was downloaded and written ({result.SelectedFileBytes} bytes). No other game file was requested.",
                    SteamSingleFileDownloadOutcome.NoSavedSession =>
                        "No saved Steam session exists. Authenticate + Save Session first, then retry Step 09.",
                    SteamSingleFileDownloadOutcome.SessionRejected =>
                        "Saved Steam session was rejected before Step 09 content access. Reauthenticate.",
                    SteamSingleFileDownloadOutcome.IdentityMismatch =>
                        "SECURITY: saved session returned a different SteamID. Step 09 stopped before content access.",
                    SteamSingleFileDownloadOutcome.OwnershipNotProven =>
                        "Step 07 ownership could not be re-proven. Step 09 stopped before depot-key/CDN access.",
                    SteamSingleFileDownloadOutcome.NoSuitableDepot =>
                        "Step 08 metadata had no direct depot with a visible public manifest suitable for this controlled test.",
                    SteamSingleFileDownloadOutcome.NoSmallFile =>
                        "The selected manifest had no safe non-empty regular file <= 2 MiB. No file was written.",
                    SteamSingleFileDownloadOutcome.FileHashMismatch =>
                        "Downloaded chunks assembled, but SHA-1 did not match Steam's manifest. Nothing was written.",
                    SteamSingleFileDownloadOutcome.TimedOut =>
                        "Step 09 timed out. No unverified file is persisted.",
                    SteamSingleFileDownloadOutcome.Cancelled =>
                        "Step 09 cancelled. No unverified file is persisted.",
                    _ =>
                        "FAIL: Step 09 did not complete. Review the detailed boundary telemetry before changing scope.",
                };
                _statusLabel.TextColor = result.Outcome == SteamSingleFileDownloadOutcome.Downloaded
                    ? UIColor.Label
                    : result.Outcome is SteamSingleFileDownloadOutcome.TimedOut or SteamSingleFileDownloadOutcome.Cancelled or SteamSingleFileDownloadOutcome.NoSavedSession
                        ? UIColor.SystemOrange
                        : UIColor.SystemRed;
            });
        }
        catch (Exception ex)
        {
            InvokeOnMainThread(() =>
            {
                _singleFileResultLabel.Text = "SINGLE-FILE: EXCEPTION";
                _singleFileResultLabel.TextColor = UIColor.SystemRed;
                _singleFileDetailLabel.Text = $"{ex.GetType().Name}: {ex.Message}";
                _statusLabel.Text = "FAIL: unhandled exception during Step 09 single-file boundary.";
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
        if (_discoveryButton is not null) _discoveryButton.Enabled = false;
        if (_singleFileButton is not null) _singleFileButton.Enabled = false;
        if (_signOutButton is not null) _signOutButton.Enabled = false;
        if (_cancelOperationButton is not null) _cancelOperationButton.Enabled = true;
    }

    private void EndSteamOperation()
    {
        if (_foundationButton is not null) _foundationButton.Enabled = true;
        if (_authButton is not null) _authButton.Enabled = true;
        if (_resumeButton is not null) _resumeButton.Enabled = true;
        if (_ownershipButton is not null) _ownershipButton.Enabled = true;
        if (_discoveryButton is not null) _discoveryButton.Enabled = true;
        if (_singleFileButton is not null) _singleFileButton.Enabled = true;
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

    private static string FormatContentDiscoveryDetail(SteamContentDiscoveryResult result)
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
            $"Step 07 ownership callback: {YesNo(result.OwnershipTicketCallbackReceived)}",
            $"Step 07 ownership result: {result.OwnershipResult?.ToString() ?? "N/A"}",
            $"Step 07 ownership ticket bytes: {result.OwnershipTicketLength}",
            $"Step 07 ownership re-proven: {YesNo(result.OwnershipProven)}",
            $"PICS access-token callback: {YesNo(result.PicsAccessTokenCallbackReceived)}",
            $"PICS app access token returned: {YesNo(result.PicsAccessTokenReceived)} (value never exposed)",
            $"PICS product-info callback: {YesNo(result.PicsProductInfoCallbackReceived)}",
            $"PICS target app found: {YesNo(result.PicsAppInfoFound)}",
            $"PICS reports missing token: {YesNo(result.PicsMissingToken)}",
            $"PICS change number: {result.PicsChangeNumber?.ToString() ?? "N/A"}",
            $"Depot count: {result.DepotCount}",
            $"Visible branch manifest count: {result.ManifestCount}",
            $"Account name: {result.AccountName ?? "not-returned"}",
            $"SteamID64: {result.SteamId64 ?? "not-returned"}",
            $"LoginID: {result.LoginId?.ToString() ?? "not-set"}",
            $"CurrentEndPoint: {result.CurrentEndPoint ?? "never-set"}",
            $"Elapsed: {result.Elapsed.TotalSeconds:F1}s",
        };

        foreach (var depot in result.Depots)
        {
            var platform = new List<string>();
            if (!string.IsNullOrWhiteSpace(depot.OsList)) platform.Add($"oslist={depot.OsList}");
            if (!string.IsNullOrWhiteSpace(depot.OsArch)) platform.Add($"osarch={depot.OsArch}");
            if (!string.IsNullOrWhiteSpace(depot.Language)) platform.Add($"language={depot.Language}");

            lines.Add($"Depot {depot.DepotId}{(platform.Count == 0 ? string.Empty : " — " + string.Join(", ", platform))}");
            foreach (var manifest in depot.Manifests)
                lines.Add($"  {manifest.Branch}: {manifest.ManifestId}");
        }

        if (!string.IsNullOrWhiteSpace(result.Error))
            lines.Add($"Error: {result.Error}");

        lines.Add("Ownership ticket payload display/logging/persistence: NONE");
        lines.Add("PICS access-token value display/logging/persistence: NONE");
        lines.Add("Depot decryption key request: NOT RUN");
        lines.Add("Manifest body request: NOT RUN");
        lines.Add("CDN server/token/chunk/file request: NOT RUN");
        return string.Join("\n", lines);
    }

    private static string FormatSingleFileDownloadDetail(SteamSingleFileDownloadResult result)
    {
        var lines = new List<string>
        {
            $"Target AppID: {result.TargetAppId}",
            $"Saved session found: {YesNo(result.SavedSessionFound)}",
            $"CM connected: {YesNo(result.CmConnected)}",
            $"LoggedOnCallback: {YesNo(result.LoggedOnCallbackReceived)}",
            $"Logon result: {result.LogonResult?.ToString() ?? "N/A"}",
            $"Stored/returned identity match: {YesNo(result.IdentityMatched)}",
            $"Step 07 ownership callback: {YesNo(result.OwnershipTicketCallbackReceived)}",
            $"Step 07 ownership result: {result.OwnershipResult?.ToString() ?? "N/A"}",
            $"Step 07 ownership ticket bytes: {result.OwnershipTicketLength}",
            $"Step 07 ownership re-proven: {YesNo(result.OwnershipProven)}",
            $"Step 08 PICS access-token callback: {YesNo(result.PicsAccessTokenCallbackReceived)}",
            $"Step 08 PICS access token returned: {YesNo(result.PicsAccessTokenReceived)} (value never exposed)",
            $"Step 08 PICS product-info callback: {YesNo(result.PicsProductInfoCallbackReceived)}",
            $"Step 08 target app found: {YesNo(result.PicsAppInfoFound)}",
            $"Step 08 PICS reports missing token: {YesNo(result.PicsMissingToken)}",
            $"Selected depot: {result.SelectedDepotId?.ToString() ?? "N/A"}",
            $"Selected depot oslist: {result.SelectedDepotOsList ?? "not-specified"}",
            $"Selected branch: {result.SelectedBranch ?? "N/A"}",
            $"Selected manifest ID: {result.SelectedManifestId?.ToString() ?? "N/A"}",
            $"Depot key requested: {YesNo(result.DepotKeyRequested)}",
            $"Depot key result: {result.DepotKeyResult?.ToString() ?? "N/A"}",
            $"Depot key received: {YesNo(result.DepotKeyReceived)} (value never exposed)",
            $"Manifest request code requested: {YesNo(result.ManifestRequestCodeRequested)}",
            $"Manifest request code received: {YesNo(result.ManifestRequestCodeReceived)} (value never exposed)",
            $"Eligible CDN servers: {result.EligibleCdnServerCount}",
            $"Manifest downloaded: {YesNo(result.ManifestDownloaded)} (in memory only)",
            $"Selected file: {result.SelectedFileName ?? "N/A"}",
            $"Selected file bytes: {result.SelectedFileBytes}",
            $"Selected file chunks: {result.SelectedFileChunkCount}",
            $"Chunks downloaded: {result.ChunksDownloaded}",
            $"Downloaded uncompressed bytes: {result.DownloadedUncompressedBytes}",
            $"CDN auth token requested after 403: {YesNo(result.CdnAuthTokenRequested)}",
            $"CDN auth token received: {YesNo(result.CdnAuthTokenReceived)} (value never exposed)",
            $"File SHA-1 matches manifest: {YesNo(result.FileHashMatched)}",
            $"Final verified file written: {YesNo(result.FileWritten)}",
            $"Output relative path: {result.OutputRelativePath ?? "not-written"}",
            $"Account name: {result.AccountName ?? "not-returned"}",
            $"SteamID64: {result.SteamId64 ?? "not-returned"}",
            $"Elapsed: {result.Elapsed.TotalSeconds:F1}s",
        };

        if (!string.IsNullOrWhiteSpace(result.Error))
            lines.Add($"Error: {result.Error}");

        lines.Add("Ownership ticket payload display/logging/persistence: NONE");
        lines.Add("PICS access-token value display/logging/persistence: NONE");
        lines.Add("Depot-key value display/logging/persistence: NONE");
        lines.Add("Manifest request-code value display/logging/persistence: NONE");
        lines.Add("CDN auth-token value display/logging/persistence: NONE");
        lines.Add("Manifest body persistence: NONE");
        lines.Add("Chunk cache/partial-file persistence: NONE");
        lines.Add("Full-depot queue: NOT IMPLEMENTED");
        lines.Add("Resume/update/install/repair: NOT IMPLEMENTED");
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
                    : "FAIL: a proven foundation regression failed; stop Step 09 work until understood.";
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
