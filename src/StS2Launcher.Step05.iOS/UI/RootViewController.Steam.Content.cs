using Foundation;
using StS2Launcher.Core;
using StS2Launcher.Step05.iOS.Platform;
using UIKit;

namespace StS2Launcher.Step05.iOS;

public sealed partial class RootViewController
{
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

    private async Task RunFullDepotDownloadAsync()
    {
        if (_fullDepotResultLabel is null || _fullDepotDetailLabel is null || _statusLabel is null)
            return;

        BeginSteamOperation();
        _fullDepotResultLabel.Text = "DEPOT: PREPARING…";
        _fullDepotResultLabel.TextColor = UIColor.Label;
        _fullDepotDetailLabel.Text =
            "Re-proving saved-session identity, Step 07 ownership and Step 08 discovery; then building one verified public-depot queue. Use Cancel Current Steam Operation at any time.";
        _statusLabel.Text =
            "STEP 10 RUNNING — one selected public depot only; staging data is temporary until the complete queue is verified and atomically committed.";
        _statusLabel.TextColor = UIColor.Label;

        var progress = new Progress<SteamDepotDownloadProgress>(value =>
        {
            if (_fullDepotResultLabel is null || _fullDepotDetailLabel is null)
                return;

            _fullDepotResultLabel.Text = value.Summary;
            _fullDepotResultLabel.TextColor = UIColor.Label;
            _fullDepotDetailLabel.Text =
                $"Phase: {value.Phase}\n" +
                $"Files: {value.CompletedFiles}/{value.TotalFiles}\n" +
                $"Chunks: {value.CompletedChunks}/{value.TotalChunks}\n" +
                $"Bytes: {value.CompletedBytes}/{value.TotalBytes}\n" +
                $"Current file: {value.CurrentFile ?? "none"}\n\n" +
                "Final output is not visible until the entire staged depot passes SHA-1 verification.";
        });

        try
        {
            var result = await _fullDepotDownloadAttempt.RunAsync(
                TimeSpan.FromMinutes(60),
                progress,
                _operationCts!.Token);

            InvokeOnMainThread(() =>
            {
                _fullDepotResultLabel.Text = result.Summary;
                _fullDepotResultLabel.TextColor = result.Outcome switch
                {
                    SteamFullDepotDownloadOutcome.Downloaded => UIColor.Label,
                    SteamFullDepotDownloadOutcome.NoSavedSession => UIColor.SystemOrange,
                    SteamFullDepotDownloadOutcome.Cancelled => UIColor.SecondaryLabel,
                    SteamFullDepotDownloadOutcome.TimedOut => UIColor.SystemOrange,
                    SteamFullDepotDownloadOutcome.OutputAlreadyExists => UIColor.SystemOrange,
                    _ => UIColor.SystemRed,
                };
                _fullDepotDetailLabel.Text = FormatFullDepotDownloadDetail(result);
                _statusLabel.Text = result.Outcome switch
                {
                    SteamFullDepotDownloadOutcome.Downloaded =>
                        $"PASS: selected depot {result.SelectedDepotId} manifest {result.SelectedManifestId} completed with {result.VerifiedFileCount} SHA-1-verified files and one atomic final-directory commit.",
                    SteamFullDepotDownloadOutcome.NoSavedSession =>
                        "No saved Steam session exists. Authenticate + Save Session first, then retry Step 10.",
                    SteamFullDepotDownloadOutcome.SessionRejected =>
                        "Saved Steam session was rejected before Step 10. Reauthenticate.",
                    SteamFullDepotDownloadOutcome.IdentityMismatch =>
                        "SECURITY: saved session returned a different SteamID. Step 10 stopped before depot content access.",
                    SteamFullDepotDownloadOutcome.OwnershipNotProven =>
                        "Step 07 ownership could not be re-proven. Step 10 stopped before depot-key/CDN access.",
                    SteamFullDepotDownloadOutcome.InvalidManifest =>
                        "The selected manifest contains a path/chunk/link shape Step 10 will not safely materialize. No final depot directory was committed.",
                    SteamFullDepotDownloadOutcome.OutputAlreadyExists =>
                        "This exact depot/manifest output directory already exists. Step 10 deliberately has no overwrite/update/repair behavior.",
                    SteamFullDepotDownloadOutcome.FileHashMismatch =>
                        "A staged file failed Steam manifest SHA-1 verification. The staging tree was removed and no final depot was committed.",
                    SteamFullDepotDownloadOutcome.Cancelled =>
                        "Step 10 cancelled. No partial final depot was committed; check the staging-cleanup telemetry below.",
                    SteamFullDepotDownloadOutcome.TimedOut =>
                        "Step 10 timed out. No partial final depot was committed; check the staging-cleanup telemetry below.",
                    _ =>
                        "FAIL: Step 10 did not complete. Review the detailed boundary telemetry; do not advance to resume/update yet.",
                };
                _statusLabel.TextColor = result.Outcome == SteamFullDepotDownloadOutcome.Downloaded
                    ? UIColor.Label
                    : result.Outcome is SteamFullDepotDownloadOutcome.NoSavedSession or SteamFullDepotDownloadOutcome.Cancelled or SteamFullDepotDownloadOutcome.TimedOut or SteamFullDepotDownloadOutcome.OutputAlreadyExists
                        ? UIColor.SystemOrange
                        : UIColor.SystemRed;
            });
        }
        catch (Exception ex)
        {
            InvokeOnMainThread(() =>
            {
                _fullDepotResultLabel.Text = "DEPOT: EXCEPTION";
                _fullDepotResultLabel.TextColor = UIColor.SystemRed;
                _fullDepotDetailLabel.Text = $"{ex.GetType().Name}: {ex.Message}";
                _statusLabel.Text = "FAIL: unhandled exception during Step 10 full-depot boundary.";
                _statusLabel.TextColor = UIColor.SystemRed;
            });
        }
        finally
        {
            InvokeOnMainThread(EndSteamOperation);
        }
    }

    private async Task RunResumableDepotDownloadAsync()
    {
        if (_resumableDepotResultLabel is null || _resumableDepotDetailLabel is null || _statusLabel is null)
            return;

        BeginSteamOperation();
        _resumableDepotResultLabel.Text = "RESUME: PREPARING…";
        _resumableDepotResultLabel.TextColor = UIColor.Label;
        _resumableDepotDetailLabel.Text =
            "Re-proving saved-session identity, Step 07 ownership and Step 08 discovery, then checking the deterministic Step 11 staging tree for SHA-1-valid complete files and Adler-32-valid partial chunks.";
        _statusLabel.Text =
            "STEP 11 RUNNING — if this is the first run, force-quit after chunk/byte progress becomes non-zero. Relaunch and run Step 11 again to prove resume.";
        _statusLabel.TextColor = UIColor.Label;

        var progress = new Progress<SteamDepotDownloadProgress>(value =>
        {
            if (_resumableDepotResultLabel is null || _resumableDepotDetailLabel is null)
                return;

            _resumableDepotResultLabel.Text = value.Summary;
            _resumableDepotResultLabel.TextColor = UIColor.Label;
            _resumableDepotDetailLabel.Text =
                $"Phase: {value.Phase}\n" +
                $"Files satisfied: {value.CompletedFiles}/{value.TotalFiles}\n" +
                $"Chunks satisfied: {value.CompletedChunks}/{value.TotalChunks}\n" +
                $"Bytes satisfied: {value.CompletedBytes}/{value.TotalBytes}\n" +
                $"Current file: {value.CurrentFile ?? "none"}\n\n" +
                "Step 11 preserves its deterministic staging tree across interruption. The final output remains invisible until the entire depot passes SHA-1 verification.";
        });

        try
        {
            var result = await _resumableDepotDownloadAttempt.RunAsync(
                TimeSpan.FromMinutes(60),
                progress,
                _operationCts!.Token);

            InvokeOnMainThread(() =>
            {
                _resumableDepotResultLabel.Text = result.Summary;
                _resumableDepotResultLabel.TextColor = result.Outcome switch
                {
                    SteamResumableDepotDownloadOutcome.Downloaded when result.ResumeWasUsed => UIColor.Label,
                    SteamResumableDepotDownloadOutcome.Downloaded => UIColor.SystemOrange,
                    SteamResumableDepotDownloadOutcome.NoSavedSession => UIColor.SystemOrange,
                    SteamResumableDepotDownloadOutcome.Cancelled => UIColor.SecondaryLabel,
                    SteamResumableDepotDownloadOutcome.TimedOut => UIColor.SystemOrange,
                    SteamResumableDepotDownloadOutcome.OutputAlreadyExists when result.ExistingFinalVerifiedAgainstManifest => UIColor.Label,
                    SteamResumableDepotDownloadOutcome.OutputAlreadyExists => UIColor.SystemOrange,
                    _ => UIColor.SystemRed,
                };
                _resumableDepotDetailLabel.Text = FormatResumableDepotDownloadDetail(result);
                _statusLabel.Text = result.Outcome switch
                {
                    SteamResumableDepotDownloadOutcome.Downloaded when result.ResumeWasUsed =>
                        $"PASS: Step 11 resumed depot {result.SelectedDepotId} manifest {result.SelectedManifestId}, reused {result.ReusedChunkCount} checksum-valid chunks / {result.ReusedBytes} bytes, downloaded only the remainder, re-verified every file, and atomically committed the final directory.",
                    SteamResumableDepotDownloadOutcome.Downloaded =>
                        "BASELINE ONLY: the Step 11 depot completed, but no prior resume data was reused. Interrupted-download resume is not yet proven; rerun the physical interruption test from clean Step 11 data.",
                    SteamResumableDepotDownloadOutcome.Cancelled =>
                        "Step 11 interruption/cancel preserved the deterministic staging tree. Run Step 11 again and require reused files/chunks/bytes > 0.",
                    SteamResumableDepotDownloadOutcome.TimedOut =>
                        "Step 11 timed out with resume staging preserved. Run it again and require reuse telemetry > 0.",
                    SteamResumableDepotDownloadOutcome.NoSavedSession =>
                        "No saved Steam session exists. Authenticate + Save Session first, then retry Step 11.",
                    SteamResumableDepotDownloadOutcome.SessionRejected =>
                        "Saved Steam session was rejected before Step 11. Reauthenticate.",
                    SteamResumableDepotDownloadOutcome.IdentityMismatch =>
                        "SECURITY: saved session returned a different SteamID. Step 11 stopped before depot content access.",
                    SteamResumableDepotDownloadOutcome.OwnershipNotProven =>
                        "Step 07 ownership could not be re-proven. Step 11 stopped before depot-key/CDN access.",
                    SteamResumableDepotDownloadOutcome.OutputAlreadyExists when result.ExistingFinalVerifiedAgainstManifest =>
                        "PASS: this exact Step 11 final depot already existed and was reverified file-by-file against Steam's current manifest. No overwrite/update occurred inside Step 11.",
                    SteamResumableDepotDownloadOutcome.OutputAlreadyExists =>
                        "This exact Step 11 final depot exists but failed current-manifest verification; Step 12 may discard and reacquire only that invalid cache.",
                    SteamResumableDepotDownloadOutcome.FileHashMismatch =>
                        "A reconstructed file failed Steam manifest SHA-1. That partial file was discarded; other checksum-valid resume data remains uncommitted.",
                    _ =>
                        "FAIL: Step 11 did not complete. Resume staging is preserved when safe, but do not advance to update/repair until this boundary passes.",
                };
                _statusLabel.TextColor =
                    (result.Outcome == SteamResumableDepotDownloadOutcome.Downloaded && result.ResumeWasUsed) ||
                    (result.Outcome == SteamResumableDepotDownloadOutcome.OutputAlreadyExists && result.ExistingFinalVerifiedAgainstManifest)
                        ? UIColor.Label
                        : result.Outcome is SteamResumableDepotDownloadOutcome.NoSavedSession or SteamResumableDepotDownloadOutcome.Cancelled or SteamResumableDepotDownloadOutcome.TimedOut or SteamResumableDepotDownloadOutcome.OutputAlreadyExists or SteamResumableDepotDownloadOutcome.Downloaded
                            ? UIColor.SystemOrange
                            : UIColor.SystemRed;
            });
        }
        catch (Exception ex)
        {
            InvokeOnMainThread(() =>
            {
                _resumableDepotResultLabel.Text = "RESUME: EXCEPTION";
                _resumableDepotResultLabel.TextColor = UIColor.SystemRed;
                _resumableDepotDetailLabel.Text = $"{ex.GetType().Name}: {ex.Message}";
                _statusLabel.Text = "FAIL: unhandled exception during Step 11 interrupted-download-resume boundary.";
                _statusLabel.TextColor = UIColor.SystemRed;
            });
        }
        finally
        {
            InvokeOnMainThread(EndSteamOperation);
        }
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

    private static string FormatFullDepotDownloadDetail(SteamFullDepotDownloadResult result)
    {
        var lines = new List<string>
        {
            $"Target AppID: {result.TargetAppId}",
            $"Saved session found: {YesNo(result.SavedSessionFound)}",
            $"CM connected: {YesNo(result.CmConnected)}",
            $"LoggedOnCallback: {YesNo(result.LoggedOnCallbackReceived)}",
            $"Logon result: {result.LogonResult?.ToString() ?? "N/A"}",
            $"Stored/returned identity match: {YesNo(result.IdentityMatched)}",
            $"Step 07 ownership re-proven: {YesNo(result.OwnershipProven)}",
            $"Step 08 PICS access-token callback: {YesNo(result.PicsAccessTokenCallbackReceived)}",
            $"Step 08 PICS product-info callback: {YesNo(result.PicsProductInfoCallbackReceived)}",
            $"Step 08 target app found: {YesNo(result.PicsAppInfoFound)}",
            $"Selected depot: {result.SelectedDepotId?.ToString() ?? "N/A"}",
            $"Selected depot oslist: {result.SelectedDepotOsList ?? "not-specified"}",
            $"Selected branch: {result.SelectedBranch ?? "N/A"}",
            $"Selected manifest ID: {result.SelectedManifestId?.ToString() ?? "N/A"}",
            $"Depot key requested/received: {YesNo(result.DepotKeyRequested)} / {YesNo(result.DepotKeyReceived)}",
            $"Manifest request code requested/received: {YesNo(result.ManifestRequestCodeRequested)} / {YesNo(result.ManifestRequestCodeReceived)}",
            $"Eligible CDN servers: {result.EligibleCdnServerCount}",
            $"Manifest downloaded: {YesNo(result.ManifestDownloaded)} (in memory only)",
            $"Queued files: {result.PlannedFileCount}",
            $"Queued chunks: {result.PlannedChunkCount}",
            $"Queued uncompressed bytes: {result.PlannedBytes}",
            $"Completed files: {result.CompletedFileCount}",
            $"SHA-1 verified files: {result.VerifiedFileCount}",
            $"Downloaded chunks: {result.DownloadedChunkCount}",
            $"Downloaded uncompressed bytes: {result.DownloadedUncompressedBytes}",
            $"CDN auth token requested after 403: {YesNo(result.CdnAuthTokenRequested)}",
            $"CDN auth token received: {YesNo(result.CdnAuthTokenReceived)} (value never exposed)",
            $"Staging directory created: {YesNo(result.StagingDirectoryCreated)}",
            $"Staging directory absent after result: {YesNo(!result.StagingDirectoryCreated || result.StagingDirectoryCleaned || result.FinalDirectoryCommitted)}",
            $"Final directory atomically committed: {YesNo(result.FinalDirectoryCommitted)}",
            $"Output relative path: {result.OutputRelativePath ?? "not-committed"}",
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
        lines.Add("Chunk cache outside staging: NONE");
        lines.Add("Partial final-depot visibility: NONE — final directory appears only after atomic staging rename");
        lines.Add("Resume: NOT IMPLEMENTED");
        lines.Add("Update/install/repair orchestration: NOT IMPLEMENTED");
        lines.Add("Multi-depot app install: NOT IMPLEMENTED");
        return string.Join("\n", lines);
    }

    private static string FormatResumableDepotDownloadDetail(SteamResumableDepotDownloadResult result)
    {
        var lines = new List<string>
        {
            $"Target AppID: {result.TargetAppId}",
            $"Saved session found: {YesNo(result.SavedSessionFound)}",
            $"CM connected: {YesNo(result.CmConnected)}",
            $"LoggedOnCallback: {YesNo(result.LoggedOnCallbackReceived)}",
            $"Logon result: {result.LogonResult?.ToString() ?? "N/A"}",
            $"Stored/returned identity match: {YesNo(result.IdentityMatched)}",
            $"Step 07 ownership re-proven: {YesNo(result.OwnershipProven)}",
            $"Step 08 PICS access-token callback: {YesNo(result.PicsAccessTokenCallbackReceived)}",
            $"Step 08 PICS product-info callback: {YesNo(result.PicsProductInfoCallbackReceived)}",
            $"Step 08 target app found: {YesNo(result.PicsAppInfoFound)}",
            $"Selected depot: {result.SelectedDepotId?.ToString() ?? "N/A"}",
            $"Selected depot oslist: {result.SelectedDepotOsList ?? "not-specified"}",
            $"Selected branch: {result.SelectedBranch ?? "N/A"}",
            $"Selected manifest ID: {result.SelectedManifestId?.ToString() ?? "N/A"}",
            $"Depot key requested/received: {YesNo(result.DepotKeyRequested)} / {YesNo(result.DepotKeyReceived)}",
            $"Manifest request code requested/received: {YesNo(result.ManifestRequestCodeRequested)} / {YesNo(result.ManifestRequestCodeReceived)}",
            $"Eligible CDN servers: {result.EligibleCdnServerCount}",
            $"Manifest downloaded: {YesNo(result.ManifestDownloaded)} (in memory only)",
            $"Planned files: {result.PlannedFileCount}",
            $"Planned chunks: {result.PlannedChunkCount}",
            $"Planned uncompressed bytes: {result.PlannedBytes}",
            $"Resume staging found at start: {YesNo(result.ResumeStagingFoundAtStart)}",
            $"Resume staging exists/created this run: {YesNo(result.ResumeStagingCreated)}",
            $"Reused fully SHA-1-verified files: {result.ReusedVerifiedFileCount}",
            $"Reused Adler-32-valid chunks: {result.ReusedChunkCount}",
            $"Reused bytes: {result.ReusedBytes}",
            $"Invalid prior resume files discarded: {result.InvalidResumeFileCount}",
            $"Invalid prior resume chunks re-downloaded: {result.InvalidResumeChunkCount}",
            $"New chunks downloaded this run: {result.NewlyDownloadedChunkCount}",
            $"New uncompressed bytes downloaded this run: {result.NewlyDownloadedBytes}",
            $"Satisfied chunks after resume/download: {result.SatisfiedChunkCount}/{result.PlannedChunkCount}",
            $"Satisfied bytes after resume/download: {result.SatisfiedBytes}/{result.PlannedBytes}",
            $"Completed files: {result.CompletedFileCount}/{result.PlannedFileCount}",
            $"SHA-1 verified files: {result.VerifiedFileCount}/{result.PlannedFileCount}",
            $"Resume data preserved after result: {YesNo(result.ResumeDataPreserved)}",
            $"Existing final cache verified against current Steam manifest: {YesNo(result.ExistingFinalVerifiedAgainstManifest)}",
            $"Final directory atomically committed: {YesNo(result.FinalDirectoryCommitted)}",
            $"Resume relative path: {result.ResumeRelativePath ?? "not-created"}",
            $"Output relative path: {result.OutputRelativePath ?? "not-committed"}",
            $"CDN auth token requested after 403: {YesNo(result.CdnAuthTokenRequested)}",
            $"CDN auth token received: {YesNo(result.CdnAuthTokenReceived)} (value never exposed)",
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
        lines.Add("Resume journal containing Steam secrets: NONE — local files are revalidated directly");
        lines.Add("Partial final-depot visibility: NONE — only deterministic staging persists until atomic commit");
        lines.Add("Manifest delta/update migration: NOT IMPLEMENTED");
        lines.Add("Update/install/repair orchestration: NOT IMPLEMENTED");
        lines.Add("Multi-depot app install: NOT IMPLEMENTED");
        return string.Join("\n", lines);
    }
}
