using Foundation;
using StS2Launcher.Core;
using StS2Launcher.iOS.Platform;
using UIKit;

namespace StS2Launcher.iOS;

public sealed partial class RootViewController
{
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
                        _ => $"Steam auth: {update.Message}",
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
                _statusLabel.Text = "FAIL: unhandled exception during automatic saved-session restore regression.";
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

    private void BeginSteamOperation(bool allowCancel = true)
    {
        _operationCts?.Dispose();
        _operationCts = new CancellationTokenSource();

        if (_foundationButton is not null) _foundationButton.Enabled = false;
        if (_authButton is not null) _authButton.Enabled = false;
        if (_resumeButton is not null) _resumeButton.Enabled = false;
        if (_ownershipButton is not null) _ownershipButton.Enabled = false;
        if (_discoveryButton is not null) _discoveryButton.Enabled = false;
        if (_singleFileButton is not null) _singleFileButton.Enabled = false;
        if (_fullDepotButton is not null) _fullDepotButton.Enabled = false;
        if (_resumableDepotButton is not null) _resumableDepotButton.Enabled = false;
        if (_managedInstallButton is not null) _managedInstallButton.Enabled = false;
        if (_prepareRepairTestButton is not null) _prepareRepairTestButton.Enabled = false;
        if (_prepareUpdateTestButton is not null) _prepareUpdateTestButton.Enabled = false;
        if (_clearDownloadCacheButton is not null) _clearDownloadCacheButton.Enabled = false;
        if (_prepareFreshDownloadTestButton is not null) _prepareFreshDownloadTestButton.Enabled = false;
        if (_offlineInstallButton is not null) _offlineInstallButton.Enabled = false;
        if (_compatibilityInventoryButton is not null) _compatibilityInventoryButton.Enabled = false;
        if (_godotFoundationStartButton is not null) _godotFoundationStartButton.Enabled = false;
        if (_godotFoundationGateDButton is not null) _godotFoundationGateDButton.Enabled = false;
        if (_managedPreparationButton is not null) _managedPreparationButton.Enabled = false;
        if (_compatibilityCallSiteButton is not null) _compatibilityCallSiteButton.Enabled = false;
        if (_realAssemblyRewriteButton is not null) _realAssemblyRewriteButton.Enabled = false;
        if (_expressionInterpreterCompatibilityButton is not null) _expressionInterpreterCompatibilityButton.Enabled = false;
        if (_dynamicManagedExecutionButton is not null) _dynamicManagedExecutionButton.Enabled = false;
        if (_signOutButton is not null) _signOutButton.Enabled = false;
        if (_cancelOperationButton is not null) _cancelOperationButton.Enabled = allowCancel;
    }

    private void EndSteamOperation()
    {
        var normalControlsEnabled = !_godotProcessRequiresRestart;
        if (_foundationButton is not null) _foundationButton.Enabled = normalControlsEnabled;
        if (_authButton is not null) _authButton.Enabled = normalControlsEnabled;
        if (_resumeButton is not null) _resumeButton.Enabled = normalControlsEnabled;
        if (_ownershipButton is not null) _ownershipButton.Enabled = normalControlsEnabled;
        if (_discoveryButton is not null) _discoveryButton.Enabled = normalControlsEnabled;
        if (_singleFileButton is not null) _singleFileButton.Enabled = normalControlsEnabled;
        if (_fullDepotButton is not null) _fullDepotButton.Enabled = normalControlsEnabled;
        if (_resumableDepotButton is not null) _resumableDepotButton.Enabled = normalControlsEnabled;
        if (_managedInstallButton is not null) _managedInstallButton.Enabled = normalControlsEnabled;
        if (_prepareRepairTestButton is not null) _prepareRepairTestButton.Enabled = normalControlsEnabled;
        if (_prepareUpdateTestButton is not null) _prepareUpdateTestButton.Enabled = normalControlsEnabled;
        if (_clearDownloadCacheButton is not null) _clearDownloadCacheButton.Enabled = normalControlsEnabled;
        if (_prepareFreshDownloadTestButton is not null) _prepareFreshDownloadTestButton.Enabled = normalControlsEnabled;
        if (_offlineInstallButton is not null) _offlineInstallButton.Enabled = normalControlsEnabled;
        if (_compatibilityInventoryButton is not null) _compatibilityInventoryButton.Enabled = normalControlsEnabled;
        if (_godotFoundationStartButton is not null) _godotFoundationStartButton.Enabled = !_godotProcessRequiresRestart;
        if (_godotFoundationGateDButton is not null)
        {
            var snapshot = _godotFoundationGates.Snapshot();
            _godotFoundationGateDButton.Enabled = _godotSessionStarted &&
                snapshot.FirstFailingGate is null &&
                snapshot.Results.Count == 3;
        }
        if (_managedPreparationButton is not null) _managedPreparationButton.Enabled = normalControlsEnabled;
        if (_compatibilityCallSiteButton is not null) _compatibilityCallSiteButton.Enabled = normalControlsEnabled;
        if (_realAssemblyRewriteButton is not null) _realAssemblyRewriteButton.Enabled = normalControlsEnabled;
        if (_expressionInterpreterCompatibilityButton is not null) _expressionInterpreterCompatibilityButton.Enabled = normalControlsEnabled;
        if (_dynamicManagedExecutionButton is not null) _dynamicManagedExecutionButton.Enabled = normalControlsEnabled;
        if (_signOutButton is not null) _signOutButton.Enabled = normalControlsEnabled;
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
}
