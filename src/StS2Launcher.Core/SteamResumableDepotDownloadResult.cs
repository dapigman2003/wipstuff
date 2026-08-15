using SteamKit2;

namespace StS2Launcher.Core;

public enum SteamResumableDepotDownloadOutcome
{
    Failed = 0,
    NoSavedSession = 1,
    InvalidLocalSession = 2,
    SessionRejected = 3,
    IdentityMismatch = 4,
    OwnershipNotProven = 5,
    PicsAccessTokenDenied = 6,
    ProductInfoUnavailable = 7,
    MissingPicsToken = 8,
    NoSuitableDepot = 9,
    DepotKeyDenied = 10,
    ManifestRequestCodeUnavailable = 11,
    NoCdnServers = 12,
    ManifestDownloadFailed = 13,
    InvalidManifest = 14,
    OutputAlreadyExists = 15,
    FileDownloadFailed = 16,
    FileHashMismatch = 17,
    FileWriteFailed = 18,
    CommitFailed = 19,
    Downloaded = 20,
    Cancelled = 21,
    TimedOut = 22,
}

/// <summary>
/// Step 11 result contract. Resume telemetry contains only counts/paths and no
/// Steam secrets, manifest payloads, chunk buffers or downloaded file bytes.
/// </summary>
public sealed record SteamResumableDepotDownloadResult(
    SteamResumableDepotDownloadOutcome Outcome,
    uint TargetAppId,
    bool SavedSessionFound,
    bool CmConnected,
    bool LoggedOnCallbackReceived,
    EResult? LogonResult,
    EResult? ExtendedLogonResult,
    bool IdentityMatched,
    bool OwnershipTicketCallbackReceived,
    EResult? OwnershipResult,
    int OwnershipTicketLength,
    bool OwnershipProven,
    bool PicsAccessTokenCallbackReceived,
    bool PicsAccessTokenReceived,
    bool PicsProductInfoCallbackReceived,
    bool PicsAppInfoFound,
    bool PicsMissingToken,
    uint? SelectedDepotId,
    ulong? SelectedManifestId,
    string? SelectedBranch,
    string? SelectedDepotOsList,
    bool DepotKeyRequested,
    EResult? DepotKeyResult,
    bool DepotKeyReceived,
    bool ManifestRequestCodeRequested,
    bool ManifestRequestCodeReceived,
    int EligibleCdnServerCount,
    bool ManifestDownloaded,
    int PlannedFileCount,
    int PlannedChunkCount,
    ulong PlannedBytes,
    int CompletedFileCount,
    int VerifiedFileCount,
    int SatisfiedChunkCount,
    ulong SatisfiedBytes,
    int ReusedVerifiedFileCount,
    int ReusedChunkCount,
    ulong ReusedBytes,
    int NewlyDownloadedChunkCount,
    ulong NewlyDownloadedBytes,
    int InvalidResumeFileCount,
    int InvalidResumeChunkCount,
    bool CdnAuthTokenRequested,
    bool CdnAuthTokenReceived,
    bool ResumeStagingFoundAtStart,
    bool ResumeStagingCreated,
    bool ResumeDataPreserved,
    bool FinalDirectoryCommitted,
    string? ResumeRelativePath,
    string? OutputRelativePath,
    string? AccountName,
    string? SteamId64,
    string? CurrentEndPoint,
    TimeSpan Elapsed,
    string? Error,
    uint? LoginId = null)
{
    public bool DownloadProven => Outcome == SteamResumableDepotDownloadOutcome.Downloaded;

    public bool ResumeWasUsed => ReusedVerifiedFileCount > 0 || ReusedChunkCount > 0;

    public string Summary => Outcome switch
    {
        SteamResumableDepotDownloadOutcome.Downloaded when ResumeWasUsed =>
            $"RESUME PASS — {CompletedFileCount}/{PlannedFileCount} files; reused {ReusedBytes} bytes",
        SteamResumableDepotDownloadOutcome.Downloaded =>
            $"RESUME BASELINE PASS — {CompletedFileCount}/{PlannedFileCount} files ({PlannedBytes} bytes)",
        SteamResumableDepotDownloadOutcome.NoSavedSession => "RESUME — no saved session",
        SteamResumableDepotDownloadOutcome.InvalidLocalSession => "RESUME BLOCKED — invalid saved session",
        SteamResumableDepotDownloadOutcome.SessionRejected => "RESUME BLOCKED — saved session rejected",
        SteamResumableDepotDownloadOutcome.IdentityMismatch => "RESUME BLOCKED — identity mismatch",
        SteamResumableDepotDownloadOutcome.OwnershipNotProven => "RESUME BLOCKED — ownership not proven",
        SteamResumableDepotDownloadOutcome.NoSuitableDepot => "RESUME FAIL — no suitable direct public depot",
        SteamResumableDepotDownloadOutcome.DepotKeyDenied => "RESUME FAIL — depot key unavailable",
        SteamResumableDepotDownloadOutcome.ManifestRequestCodeUnavailable => "RESUME FAIL — manifest request code unavailable",
        SteamResumableDepotDownloadOutcome.NoCdnServers => "RESUME FAIL — no CDN servers",
        SteamResumableDepotDownloadOutcome.ManifestDownloadFailed => "RESUME FAIL — manifest download failed",
        SteamResumableDepotDownloadOutcome.InvalidManifest => "RESUME FAIL — manifest is unsafe/unsupported",
        SteamResumableDepotDownloadOutcome.OutputAlreadyExists => "RESUME BLOCKED — final manifest directory already exists",
        SteamResumableDepotDownloadOutcome.FileDownloadFailed => "RESUME FAIL — file/chunk download failed",
        SteamResumableDepotDownloadOutcome.FileHashMismatch => "RESUME FAIL — file SHA-1 mismatch",
        SteamResumableDepotDownloadOutcome.FileWriteFailed => "RESUME FAIL — staging write failed",
        SteamResumableDepotDownloadOutcome.CommitFailed => "RESUME FAIL — atomic directory commit failed",
        SteamResumableDepotDownloadOutcome.TimedOut => "RESUME TIMEOUT — staging preserved",
        SteamResumableDepotDownloadOutcome.Cancelled => "RESUME INTERRUPTED — staging preserved",
        _ => "RESUME FAIL",
    };
}
