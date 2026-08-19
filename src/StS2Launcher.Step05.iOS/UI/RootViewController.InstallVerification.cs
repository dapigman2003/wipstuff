using Foundation;
using StS2Launcher.Core;
using StS2Launcher.Step05.iOS.Platform;
using UIKit;

namespace StS2Launcher.Step05.iOS;

public sealed partial class RootViewController
{
    private async Task RunOfflineInstallInspectionAsync()
    {
        if (_offlineInstallResultLabel is null || _offlineInstallDetailLabel is null || _statusLabel is null)
            return;

        BeginSteamOperation();
        _offlineInstallResultLabel.Text = "OFFLINE STATE: VERIFYING LOCAL INSTALL…";
        _offlineInstallResultLabel.TextColor = UIColor.Label;
        _offlineInstallDetailLabel.Text =
            "Local-only Step 13 verification started. Reading the Step 12 receipt and hashing managed files; no Steam session or network API is used by this check.";
        _statusLabel.Text = "STEP 13 LOCAL CHECK RUNNING — exact receipt/file verification only; online manifest freshness intentionally unknown.";
        _statusLabel.TextColor = UIColor.Label;

        try
        {
            var progress = new Progress<SteamOfflineInstallProgress>(value =>
            {
                InvokeOnMainThread(() =>
                {
                    if (_offlineInstallResultLabel is null || _offlineInstallDetailLabel is null)
                        return;

                    _offlineInstallResultLabel.Text = $"OFFLINE STATE: {value.Phase.ToString().ToUpperInvariant()}";
                    _offlineInstallResultLabel.TextColor = UIColor.Label;
                    _offlineInstallDetailLabel.Text =
                        $"{value.Message}\nFiles: {value.CompletedFiles}/{value.TotalFiles}\nBytes: {value.CompletedBytes}/{value.TotalBytes}" +
                        (string.IsNullOrWhiteSpace(value.CurrentFile) ? string.Empty : $"\nCurrent: {value.CurrentFile}");
                });
            });

            var result = await _offlineInstallInspection.RunAsync(
                progress,
                _operationCts!.Token);

            InvokeOnMainThread(() =>
            {
                _offlineInstallResultLabel.Text = result.Summary;
                _offlineInstallResultLabel.TextColor = result.Outcome switch
                {
                    SteamOfflineInstallOutcome.OfflineReady => UIColor.Label,
                    SteamOfflineInstallOutcome.NoManagedInstall => UIColor.SystemOrange,
                    SteamOfflineInstallOutcome.Cancelled => UIColor.SecondaryLabel,
                    _ => UIColor.SystemRed,
                };
                _offlineInstallDetailLabel.Text = FormatOfflineInstallDetail(result);
                _statusLabel.Text = result.Outcome switch
                {
                    SteamOfflineInstallOutcome.OfflineReady =>
                        "PASS: Step 13 local state is OfflineReady. The managed install was verified without consulting Steam/session/network; online manifest freshness remains unknown until an online manager check.",
                    SteamOfflineInstallOutcome.NoManagedInstall =>
                        "OFFLINE SETUP REQUIRED: no Step 12 managed install exists. Reconnect and complete the legitimate online setup path first.",
                    SteamOfflineInstallOutcome.Cancelled =>
                        "Step 13 local verification cancelled; no managed files were changed.",
                    _ =>
                        $"OFFLINE REPAIR REQUIRED: {result.Error ?? result.Outcome.ToString()}. Reconnect and use the proven Step 12 manager before treating the install as offline-ready.",
                };
                _statusLabel.TextColor = result.Outcome switch
                {
                    SteamOfflineInstallOutcome.OfflineReady => UIColor.Label,
                    SteamOfflineInstallOutcome.Cancelled => UIColor.SecondaryLabel,
                    SteamOfflineInstallOutcome.NoManagedInstall => UIColor.SystemOrange,
                    _ => UIColor.SystemRed,
                };
            });
        }
        catch (Exception ex)
        {
            InvokeOnMainThread(() =>
            {
                _offlineInstallResultLabel.Text = "OFFLINE CHECK: EXCEPTION";
                _offlineInstallResultLabel.TextColor = UIColor.SystemRed;
                _offlineInstallDetailLabel.Text = $"{ex.GetType().Name}: {ex.Message}";
                _statusLabel.Text = "FAIL: unhandled exception during Step 13 local-only inspection.";
                _statusLabel.TextColor = UIColor.SystemRed;
            });
        }
        finally
        {
            await WriteDeviceTestReportFromLabelsAsync(
                "Step13-OfflineReady.txt",
                "StS2 Launcher — Step 13 Offline-Ready Verification",
                _offlineInstallResultLabel,
                _offlineInstallDetailLabel,
                CancellationToken.None);
            InvokeOnMainThread(EndSteamOperation);
        }
    }

    private async Task RunCompatibilityInventoryAsync()
    {
        if (_compatibilityInventoryResultLabel is null ||
            _compatibilityInventoryDetailLabel is null ||
            _statusLabel is null)
        {
            return;
        }

        BeginSteamOperation();
        _compatibilityInventoryResultLabel.Text = "COMPATIBILITY INVENTORY: RUNNING…";
        _compatibilityInventoryResultLabel.TextColor = UIColor.Label;
        _compatibilityInventoryDetailLabel.Text =
            "Read-only Step 14 inventory started. Re-proving OfflineReady, then classifying installed files and scanning managed metadata strings. No Steam/network request, game-file write, assembly load, or game launch is performed.";
        _statusLabel.Text = "STEP 14 INVENTORY RUNNING — local/read-only compatibility inspection only.";
        _statusLabel.TextColor = UIColor.Label;

        try
        {
            var progress = new Progress<SteamCompatibilityInventoryProgress>(value =>
            {
                InvokeOnMainThread(() =>
                {
                    if (_compatibilityInventoryResultLabel is null || _compatibilityInventoryDetailLabel is null)
                        return;

                    _compatibilityInventoryResultLabel.Text =
                        $"COMPATIBILITY INVENTORY: {value.Phase.ToString().ToUpperInvariant()}";
                    _compatibilityInventoryResultLabel.TextColor = UIColor.Label;
                    _compatibilityInventoryDetailLabel.Text =
                        $"{value.Message}\nFiles: {value.ProcessedFiles}/{value.TotalFiles}\nBytes: {value.ProcessedBytes}/{value.TotalBytes}" +
                        (string.IsNullOrWhiteSpace(value.CurrentRelativePath)
                            ? string.Empty
                            : $"\nCurrent: {value.CurrentRelativePath}");
                });
            });

            var result = await _compatibilityInventoryInspection.RunAsync(
                progress,
                _operationCts!.Token);

            InvokeOnMainThread(() =>
            {
                _compatibilityInventoryResultLabel.Text = result.Summary;
                _compatibilityInventoryResultLabel.TextColor = result.Outcome switch
                {
                    SteamCompatibilityInventoryOutcome.Complete => UIColor.Label,
                    SteamCompatibilityInventoryOutcome.Cancelled => UIColor.SecondaryLabel,
                    SteamCompatibilityInventoryOutcome.LocalInstallNotReady => UIColor.SystemOrange,
                    _ => UIColor.SystemRed,
                };
                _compatibilityInventoryDetailLabel.Text = FormatCompatibilityInventoryDetail(result);
                _statusLabel.Text = result.Outcome switch
                {
                    SteamCompatibilityInventoryOutcome.Complete =>
                        $"PASS: Step 14 classified {result.TotalFiles} installed files read-only. Review the reported dependency and potential iOS blocker signals before choosing the next compatibility boundary.",
                    SteamCompatibilityInventoryOutcome.LocalInstallNotReady =>
                        "STEP 14 BLOCKED: the managed install is not currently OfflineReady. Restore it with the proven Step 12 manager, then rerun the inventory.",
                    SteamCompatibilityInventoryOutcome.Cancelled =>
                        "Step 14 inventory cancelled; the managed install was not modified.",
                    _ =>
                        $"STEP 14 FAIL: {result.Error ?? result.Outcome.ToString()}. The managed install was not modified.",
                };
                _statusLabel.TextColor = result.Outcome switch
                {
                    SteamCompatibilityInventoryOutcome.Complete => UIColor.Label,
                    SteamCompatibilityInventoryOutcome.Cancelled => UIColor.SecondaryLabel,
                    SteamCompatibilityInventoryOutcome.LocalInstallNotReady => UIColor.SystemOrange,
                    _ => UIColor.SystemRed,
                };
            });
        }
        catch (Exception ex)
        {
            InvokeOnMainThread(() =>
            {
                _compatibilityInventoryResultLabel.Text = "COMPATIBILITY INVENTORY: EXCEPTION";
                _compatibilityInventoryResultLabel.TextColor = UIColor.SystemRed;
                _compatibilityInventoryDetailLabel.Text = $"{ex.GetType().Name}: {ex.Message}";
                _statusLabel.Text = "FAIL: unhandled exception during Step 14 read-only compatibility inventory.";
                _statusLabel.TextColor = UIColor.SystemRed;
            });
        }
        finally
        {
            await WriteDeviceTestReportFromLabelsAsync(
                "Step14-CompatibilityInventory.txt",
                "StS2 Launcher — Step 14 Compatibility Inventory",
                _compatibilityInventoryResultLabel,
                _compatibilityInventoryDetailLabel,
                CancellationToken.None);
            InvokeOnMainThread(EndSteamOperation);
        }
    }

    private static string FormatOfflineInstallDetail(SteamOfflineInstallResult result)
    {
        var lines = new List<string>
        {
            $"State: {result.State}",
            $"Managed directory found: {YesNo(result.ManagedDirectoryFound)}",
            $"Receipt found: {YesNo(result.ReceiptFound)}",
            $"Receipt structurally valid: {YesNo(result.ReceiptStructurallyValid)}",
            $"Depot: {result.DepotId?.ToString() ?? "N/A"}",
            $"Installed manifest recorded locally: {result.InstalledManifestId?.ToString() ?? "N/A"}",
            $"Branch recorded locally: {result.Branch ?? "N/A"}",
            $"Files verified: {result.VerifiedFiles}/{result.PlannedFiles}",
            $"Bytes verified: {result.VerifiedBytes}/{result.PlannedBytes}",
            $"Exact managed tree verified: {YesNo(result.ExactManagedTreeVerified)}",
            $"Steam session consulted: {YesNo(result.SteamSessionConsulted)}",
            $"Network access attempted by Step 13 check: {YesNo(result.NetworkAccessAttempted)}",
            $"Online manifest freshness known: {YesNo(result.OnlineManifestFreshnessKnown)}",
            $"Managed install: {result.ManagedInstallRelativePath ?? "N/A"}",
            $"Elapsed: {result.Elapsed.TotalSeconds:F1}s",
            "Game launch / compatibility preparation: NOT IMPLEMENTED",
        };

        if (!string.IsNullOrWhiteSpace(result.Error))
            lines.Add($"Error: {result.Error}");

        return string.Join("\n", lines);
    }

    private static string FormatCompatibilityInventoryDetail(SteamCompatibilityInventoryResult result)
    {
        var lines = new List<string>
        {
            $"Target AppID: {result.TargetAppId}",
            $"Depot: {result.DepotId?.ToString() ?? "N/A"}",
            $"Installed manifest recorded locally: {result.InstalledManifestId?.ToString() ?? "N/A"}",
            $"Branch recorded locally: {result.Branch ?? "N/A"}",
            $"OfflineReady precondition re-proven: {YesNo(result.OfflineReadyPreconditionVerified)}",
            $"Total installed files/bytes: {result.TotalFiles} / {result.TotalBytes}",
            $"Asset files/bytes: {result.AssetFiles} / {result.AssetBytes}",
            $"Godot content files: {result.GodotContentFiles}",
            $"Managed assemblies: {result.ManagedAssemblyFiles} ({result.ManagedAssemblyBytes} bytes)",
            $"Managed assemblies metadata-scanned: {result.ManagedAssembliesScanned}",
            $"Native binaries: {result.NativeBinaryFiles} ({result.NativeBinaryBytes} bytes)",
            $"Godot/GodotSharp indicator files: {result.GodotSharpIndicatorFiles}",
            $"FMOD indicator files: {result.FmodIndicatorFiles}",
            $"Spine indicator files: {result.SpineIndicatorFiles}",
            $"General reflection indicator files: {result.ReflectionIndicatorFiles}",
            $"Dynamic-code/JIT indicator files: {result.DynamicCodeIndicatorFiles}",
            $"Platform-specific indicator files: {result.PlatformSpecificFiles}",
            $"Other/unclassified files: {result.OtherFiles}",
            $"Potential iOS blocker signals: {result.PotentialIosBlockerSignals.Count}",
            $"Dependency notes: {result.DependencyNotes.Count}",
            $"Steam session consulted: {YesNo(result.SteamSessionConsulted)}",
            $"Network access attempted by Step 14: {YesNo(result.NetworkAccessAttempted)}",
            $"Managed install modified by Step 14: {YesNo(result.ManagedInstallModified)}",
            $"Game launch attempted: {YesNo(result.GameLaunchAttempted)}",
            $"Elapsed: {result.Elapsed.TotalSeconds:F1}s",
        };

        AddEvidence(lines, "Potential iOS blocker signals", result.PotentialIosBlockerSignals, 8);
        AddEvidence(lines, "Dependency notes", result.DependencyNotes, 8);
        AddEvidence(lines, "Managed assembly sample", result.ManagedAssemblyEvidence, 10);
        AddEvidence(lines, "Native binary sample", result.NativeBinaryEvidence, 10);
        AddEvidence(lines, "Dynamic-code evidence", result.DynamicCodeEvidence, 8);
        AddEvidence(lines, "Reflection evidence", result.ReflectionEvidence, 8);
        AddEvidence(lines, "Godot/GodotSharp evidence", result.GodotSharpEvidence, 8);
        AddEvidence(lines, "FMOD evidence", result.FmodEvidence, 8);
        AddEvidence(lines, "Spine evidence", result.SpineEvidence, 8);
        AddEvidence(lines, "Platform-specific evidence", result.PlatformSpecificEvidence, 8);

        lines.Add("Step 14 evidence policy: metadata/path indicators are triage signals, not proof that an API path executes at runtime.");
        lines.Add("Mono.Cecil rewrite / StS2 game execution: NOT IMPLEMENTED; Step 15 Godot Foundation is a separate launcher-owned smoke-host test.");

        if (!string.IsNullOrWhiteSpace(result.Error))
            lines.Add($"Error: {result.Error}");

        return string.Join("\n", lines);
    }

    private static void AddEvidence(
        List<string> lines,
        string title,
        IReadOnlyList<string> evidence,
        int limit)
    {
        if (evidence.Count == 0)
            return;

        lines.Add($"{title}:");
        foreach (var item in evidence.Take(limit))
            lines.Add($"  • {item}");
        if (evidence.Count > limit)
            lines.Add($"  • … {evidence.Count - limit} more");
    }
}
