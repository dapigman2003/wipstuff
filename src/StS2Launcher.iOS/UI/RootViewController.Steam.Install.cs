using Foundation;
using StS2Launcher.Core;
using StS2Launcher.iOS.Platform;
using UIKit;

namespace StS2Launcher.iOS;

public sealed partial class RootViewController
{
    private async Task RunManagedInstallAsync()
    {
        if (_managedInstallResultLabel is null || _managedInstallDetailLabel is null || _statusLabel is null)
            return;

        BeginSteamOperation();
        _managedInstallResultLabel.Text = "INSTALL MANAGER: INSPECTING…";
        _managedInstallResultLabel.TextColor = UIColor.Label;
        _managedInstallDetailLabel.Text =
            "Discovering the current public manifest, verifying the stable managed install, then performing exactly one of: no-op, install, update, or repair. Any replacement is fully staged and verified before the prior good install is swapped out.";
        _statusLabel.Text = "STEP 12.4.1 RUNNING — completed Step 12 manager regression; current source/cache/receipt/staging/rollback safeguards remain active.";
        _statusLabel.TextColor = UIColor.Label;

        var progress = new Progress<SteamManagedInstallProgress>(value =>
        {
            if (_managedInstallResultLabel is null || _managedInstallDetailLabel is null)
                return;
            _managedInstallResultLabel.Text = $"INSTALL MANAGER: {value.Phase.ToString().ToUpperInvariant()}";
            _managedInstallResultLabel.TextColor = UIColor.Label;
            _managedInstallDetailLabel.Text =
                $"{value.Message}\n" +
                $"Files: {value.CompletedFiles}/{value.TotalFiles}\n" +
                $"Bytes: {value.CompletedBytes}/{value.TotalBytes}\n" +
                $"Current file: {value.CurrentFile ?? "none"}";
        });

        try
        {
            var result = await _managedInstallAttempt.RunAsync(
                TimeSpan.FromMinutes(90),
                progress,
                _operationCts!.Token);

            InvokeOnMainThread(() =>
            {
                _managedInstallResultLabel.Text = result.Summary;
                _managedInstallResultLabel.TextColor = result.Success
                    ? UIColor.Label
                    : result.Outcome is SteamManagedInstallOutcome.Cancelled or SteamManagedInstallOutcome.TimedOut
                        ? UIColor.SystemOrange
                        : UIColor.SystemRed;
                _managedInstallDetailLabel.Text = FormatManagedInstallDetail(result);
                _statusLabel.Text = result.Success
                    ? $"PASS: Step 12.4.1 state {result.StateBefore} -> {result.StateAfter}; action {result.ActionTaken}; stable managed install is verified and current."
                    : $"Step 12.4.1 manager regression did not complete: {result.Error ?? result.Outcome.ToString()}. The prior good install was preserved when one existed.";
                _statusLabel.TextColor = result.Success ? UIColor.Label : UIColor.SystemRed;
            });
        }
        catch (Exception ex)
        {
            InvokeOnMainThread(() =>
            {
                _managedInstallResultLabel.Text = "INSTALL MANAGER: EXCEPTION";
                _managedInstallResultLabel.TextColor = UIColor.SystemRed;
                _managedInstallDetailLabel.Text = $"{ex.GetType().Name}: {ex.Message}";
                _statusLabel.Text = "FAIL: unhandled exception during Step 12.4.1 install/update/repair manager regression.";
                _statusLabel.TextColor = UIColor.SystemRed;
            });
        }
        finally
        {
            await WriteDeviceTestReportFromLabelsAsync(
                "Step12-ManagedInstall.txt",
                "StS2 Launcher — Step 12 Managed Install / Update / Repair",
                _managedInstallResultLabel,
                _managedInstallDetailLabel,
                CancellationToken.None);
            InvokeOnMainThread(EndSteamOperation);
        }
    }

    private async Task PrepareRepairTestAsync()
    {
        if (_managedInstallResultLabel is null || _managedInstallDetailLabel is null || _statusLabel is null)
            return;
        try
        {
            var relative = await _managedInstallAttempt.PrepareRepairTestAsync();
            _managedInstallResultLabel.Text = "REPAIR TEST PREPARED";
            _managedInstallResultLabel.TextColor = UIColor.SystemOrange;
            _managedInstallDetailLabel.Text = $"Intentionally changed one local byte in managed file: {relative}\nRun Inspect + Install / Update / Repair now. It must report StateBefore=RepairNeeded and finish REPAIR PASS.";
            _statusLabel.Text = "Repair test prepared locally; no Steam credential/content request was made by the test helper.";
            _statusLabel.TextColor = UIColor.SystemOrange;
        }
        catch (Exception ex)
        {
            _managedInstallResultLabel.Text = "REPAIR TEST PREP FAILED";
            _managedInstallResultLabel.TextColor = UIColor.SystemRed;
            _managedInstallDetailLabel.Text = $"{ex.GetType().Name}: {ex.Message}";
        }
        finally
        {
            await WriteDeviceTestReportFromLabelsAsync(
                "TestSetup-Repair.txt",
                "StS2 Launcher — Repair Test Setup",
                _managedInstallResultLabel,
                _managedInstallDetailLabel,
                CancellationToken.None);
        }
    }

    private async Task PrepareUpdateStateTestAsync()
    {
        if (_managedInstallResultLabel is null || _managedInstallDetailLabel is null || _statusLabel is null)
            return;
        try
        {
            var simulatedManifest = await _managedInstallAttempt.PrepareUpdateStateTestAsync();
            _managedInstallResultLabel.Text = "UPDATE-STATE TEST PREPARED";
            _managedInstallResultLabel.TextColor = UIColor.SystemOrange;
            _managedInstallDetailLabel.Text = $"Changed only the project-owned local install receipt: stale manifest ID {simulatedManifest} plus one synthetic changed-file SHA-1 identity. Actual game files were not modified. Run Inspect + Install / Update / Repair now. It must report StateBefore=UpdateAvailable, reverify the existing Step 11 cache against Steam, replace at least one file from that source, and finish UPDATE PASS using Steam's actual current public manifest.";
            _statusLabel.Text = "Update test prepared locally; the next manager run must prove the real update path from current Steam metadata without needlessly redownloading an already-valid current-manifest cache.";
            _statusLabel.TextColor = UIColor.SystemOrange;
        }
        catch (Exception ex)
        {
            _managedInstallResultLabel.Text = "UPDATE TEST PREP FAILED";
            _managedInstallResultLabel.TextColor = UIColor.SystemRed;
            _managedInstallDetailLabel.Text = $"{ex.GetType().Name}: {ex.Message}";
        }
        finally
        {
            await WriteDeviceTestReportFromLabelsAsync(
                "TestSetup-Update.txt",
                "StS2 Launcher — Update-State Test Setup",
                _managedInstallResultLabel,
                _managedInstallDetailLabel,
                CancellationToken.None);
        }
    }

    private async Task ClearDownloadCacheAsync()
    {
        if (_managedInstallResultLabel is null || _managedInstallDetailLabel is null || _statusLabel is null)
            return;

        BeginSteamOperation(allowCancel: false);
        try
        {
            _managedInstallResultLabel.Text = "DOWNLOAD CACHE: CLEARING…";
            _managedInstallResultLabel.TextColor = UIColor.Label;
            _managedInstallDetailLabel.Text = "Deleting only the project-owned Step 11 download cache. The managed install and saved Steam session are not touched.";

            var result = await Task.Run(_downloadCacheMaintenance.Clear);
            _managedInstallResultLabel.Text = result.CacheExisted ? "DOWNLOAD CACHE: CLEARED" : "DOWNLOAD CACHE: ALREADY EMPTY";
            _managedInstallResultLabel.TextColor = UIColor.Label;
            _managedInstallDetailLabel.Text =
                $"Cache path: {result.CacheRelativePath}\n" +
                $"Cache existed: {YesNo(result.CacheExisted)}\n" +
                $"Cache absent now: {YesNo(result.CacheAbsentAfterClear)}\n" +
                "Managed Step 12 install: PRESERVED\nSaved Steam session: PRESERVED";
            _statusLabel.Text = "PASS: Step 11 download cache is absent. A normal UpToDate manager run may still no-op; use Prepare Fresh Download Test when you specifically want to force CDN acquisition.";
            _statusLabel.TextColor = UIColor.Label;
        }
        catch (Exception ex)
        {
            _managedInstallResultLabel.Text = "DOWNLOAD CACHE: CLEAR FAILED";
            _managedInstallResultLabel.TextColor = UIColor.SystemRed;
            _managedInstallDetailLabel.Text = $"{ex.GetType().Name}: {ex.Message}";
            _statusLabel.Text = "FAIL: Step 11 cache clear did not complete. Managed install/session were not intentionally modified by this control.";
            _statusLabel.TextColor = UIColor.SystemRed;
        }
        finally
        {
            await WriteDeviceTestReportFromLabelsAsync(
                "TestSetup-DownloadCacheClear.txt",
                "StS2 Launcher — Download Cache Test Setup",
                _managedInstallResultLabel,
                _managedInstallDetailLabel,
                CancellationToken.None);
            EndSteamOperation();
        }
    }

    private async Task PrepareFreshDownloadTestAsync()
    {
        if (_managedInstallResultLabel is null || _managedInstallDetailLabel is null || _statusLabel is null)
            return;

        BeginSteamOperation(allowCancel: false);
        try
        {
            _managedInstallResultLabel.Text = "FRESH DOWNLOAD TEST: PREPARING…";
            _managedInstallResultLabel.TextColor = UIColor.Label;
            _managedInstallDetailLabel.Text = "Preparing the existing synthetic UpdateAvailable receipt, then deleting only the Step 11 source cache.";

            var simulatedManifest = await _managedInstallAttempt.PrepareUpdateStateTestAsync();
            var clearResult = await Task.Run(_downloadCacheMaintenance.Clear);

            _managedInstallResultLabel.Text = "FRESH DOWNLOAD TEST PREPARED";
            _managedInstallResultLabel.TextColor = UIColor.SystemOrange;
            _managedInstallDetailLabel.Text =
                $"Synthetic stale receipt manifest: {simulatedManifest}\n" +
                $"Download cache existed: {YesNo(clearResult.CacheExisted)}\n" +
                $"Download cache absent now: {YesNo(clearResult.CacheAbsentAfterClear)}\n" +
                "Managed game files: UNCHANGED\nSaved Steam session: PRESERVED\n\n" +
                "Now tap Inspect + Install / Update / Repair. It must report StateBefore=UpdateAvailable, reacquire the current public depot from Steam because no Step 11 cache exists, verify the full source, replace at least the synthetic changed-file identity, atomically commit, and finish UPDATE PASS / UpToDate.";
            _statusLabel.Text = "Fresh-download regression prepared. The next manager run is expected to transfer the current depot from Steam; do not clear/prepare again until that run completes or is deliberately cancelled.";
            _statusLabel.TextColor = UIColor.SystemOrange;
        }
        catch (Exception ex)
        {
            _managedInstallResultLabel.Text = "FRESH DOWNLOAD TEST PREP FAILED";
            _managedInstallResultLabel.TextColor = UIColor.SystemRed;
            _managedInstallDetailLabel.Text = $"{ex.GetType().Name}: {ex.Message}";
            _statusLabel.Text = "Fresh-download test preparation did not complete. If the receipt was already made stale before cache deletion failed, the normal manager can safely reconcile it using the existing verified source cache.";
            _statusLabel.TextColor = UIColor.SystemRed;
        }
        finally
        {
            await WriteDeviceTestReportFromLabelsAsync(
                "TestSetup-FreshDownload.txt",
                "StS2 Launcher — Fresh Download Test Setup",
                _managedInstallResultLabel,
                _managedInstallDetailLabel,
                CancellationToken.None);
            EndSteamOperation();
        }
    }

    private static string FormatManagedInstallDetail(SteamManagedInstallResult result)
    {
        var lines = new List<string>
        {
            $"Target AppID: {result.TargetAppId}",
            $"Selected depot: {result.DepotId?.ToString() ?? "N/A"}",
            $"Current public manifest: {result.CurrentManifestId?.ToString() ?? "N/A"}",
            $"Installed manifest before: {result.InstalledManifestIdBefore?.ToString() ?? "none"}",
            $"Installed manifest after: {result.InstalledManifestIdAfter?.ToString() ?? "none"}",
            $"Branch: {result.Branch ?? "N/A"}",
            $"State before: {result.StateBefore}",
            $"Action taken: {result.ActionTaken}",
            $"State after: {result.StateAfter}",
            $"Planned files: {result.PlannedFiles}",
            $"Planned bytes: {result.PlannedBytes}",
            $"Verified source files/bytes: {result.VerifiedSourceFiles} / {result.VerifiedSourceBytes}",
            $"Source cache reverified against current Steam manifest: {YesNo(result.SourceCacheReverifiedAgainstCurrentManifest)}",
            $"Source bytes downloaded this manager run: {result.SourceNewlyDownloadedBytes}",
            $"Reused locally verified files/bytes: {result.ReusedLocalFiles} / {result.ReusedLocalBytes}",
            $"Replaced files/bytes: {result.ReplacedFiles} / {result.ReplacedBytes}",
            $"Previous install preserved until commit: {YesNo(result.ExistingInstallPreservedUntilCommit)}",
            $"Atomic commit completed: {YesNo(result.AtomicCommitCompleted)}",
            $"Rollback restored previous install: {YesNo(result.RollbackRestoredPreviousInstall)}",
            $"Staging absent after result: {YesNo(result.StagingAbsentAfterResult)}",
            $"Backup absent after result: {YesNo(result.BackupAbsentAfterResult)}",
            $"Managed install relative path: {result.ManagedInstallRelativePath ?? "not-installed"}",
            $"Verified Step 11 source cache: {result.SourceCacheRelativePath ?? "not-needed"}",
            $"Elapsed: {result.Elapsed.TotalSeconds:F1}s",
        };

        if (!string.IsNullOrWhiteSpace(result.Error))
            lines.Add($"Error: {result.Error}");

        lines.Add("Managed receipt contents: AppID/depot/manifest/branch + relative path/length/SHA-1 only");
        lines.Add("Steam refresh token/password/Guard persistence in install receipt: NONE");
        lines.Add("Depot key / manifest request code / CDN auth token persistence in install receipt: NONE");
        lines.Add("Previous good install visibility during staging: PRESERVED");
        lines.Add("Partial replacement visibility: NONE — replacement becomes live only at directory swap");
        lines.Add("Multi-depot app composition: NOT IMPLEMENTED");
        lines.Add("Compatibility inventory / Cecil / Godot / game launch: NOT RUN");
        lines.Add("Steam Cloud / Workshop: NOT RUN");
        return string.Join("\n", lines);
    }

    private static string FormatUtc(DateTimeOffset? value) =>
        value?.UtcDateTime.ToString("yyyy-MM-dd HH:mm:ss 'UTC'") ?? "unavailable";

    private static string YesNoNullable(bool? value) =>
        value.HasValue ? YesNo(value.Value) : "unknown";
}
