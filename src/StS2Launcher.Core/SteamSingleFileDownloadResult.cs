using SteamKit2;

namespace StS2Launcher.Core;

public enum SteamSingleFileDownloadOutcome
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
    NoSmallFile = 14,
    ChunkDownloadFailed = 15,
    FileHashMismatch = 16,
    FileWriteFailed = 17,
    Downloaded = 18,
    Cancelled = 19,
    TimedOut = 20,
}

/// <summary>
/// Step 09 result contract. Secret/token/key payloads and raw downloaded bytes
/// are deliberately excluded. Only non-secret proof telemetry is retained.
/// </summary>
public sealed record SteamSingleFileDownloadResult(
    SteamSingleFileDownloadOutcome Outcome,
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
    string? SelectedFileName,
    ulong SelectedFileBytes,
    int SelectedFileChunkCount,
    int ChunksDownloaded,
    ulong DownloadedUncompressedBytes,
    bool CdnAuthTokenRequested,
    bool CdnAuthTokenReceived,
    bool FileHashMatched,
    bool FileWritten,
    string? OutputRelativePath,
    string? AccountName,
    string? SteamId64,
    string? CurrentEndPoint,
    TimeSpan Elapsed,
    string? Error,
    uint? LoginId = null)
{
    public bool DownloadProven => Outcome == SteamSingleFileDownloadOutcome.Downloaded;

    public string Summary => Outcome switch
    {
        SteamSingleFileDownloadOutcome.Downloaded =>
            $"SINGLE-FILE PASS — {SelectedFileName ?? "file"} ({SelectedFileBytes} bytes)",
        SteamSingleFileDownloadOutcome.NoSavedSession => "SINGLE-FILE — no saved session",
        SteamSingleFileDownloadOutcome.InvalidLocalSession => "SINGLE-FILE BLOCKED — invalid saved session",
        SteamSingleFileDownloadOutcome.SessionRejected => "SINGLE-FILE BLOCKED — saved session rejected",
        SteamSingleFileDownloadOutcome.IdentityMismatch => "SINGLE-FILE BLOCKED — identity mismatch",
        SteamSingleFileDownloadOutcome.OwnershipNotProven => "SINGLE-FILE BLOCKED — ownership not proven",
        SteamSingleFileDownloadOutcome.NoSuitableDepot => "SINGLE-FILE FAIL — no suitable direct public depot",
        SteamSingleFileDownloadOutcome.DepotKeyDenied => "SINGLE-FILE FAIL — depot key unavailable",
        SteamSingleFileDownloadOutcome.ManifestRequestCodeUnavailable => "SINGLE-FILE FAIL — manifest request code unavailable",
        SteamSingleFileDownloadOutcome.NoCdnServers => "SINGLE-FILE FAIL — no CDN servers",
        SteamSingleFileDownloadOutcome.ManifestDownloadFailed => "SINGLE-FILE FAIL — manifest download failed",
        SteamSingleFileDownloadOutcome.NoSmallFile => "SINGLE-FILE FAIL — no bounded file found",
        SteamSingleFileDownloadOutcome.ChunkDownloadFailed => "SINGLE-FILE FAIL — chunk download failed",
        SteamSingleFileDownloadOutcome.FileHashMismatch => "SINGLE-FILE FAIL — SHA-1 mismatch",
        SteamSingleFileDownloadOutcome.FileWriteFailed => "SINGLE-FILE FAIL — verified file could not be written",
        SteamSingleFileDownloadOutcome.TimedOut => "SINGLE-FILE TIMEOUT",
        SteamSingleFileDownloadOutcome.Cancelled => "SINGLE-FILE CANCELLED",
        _ => "SINGLE-FILE FAIL",
    };
}
