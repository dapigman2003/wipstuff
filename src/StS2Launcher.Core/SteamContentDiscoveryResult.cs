using SteamKit2;

namespace StS2Launcher.Core;

public enum SteamContentDiscoveryOutcome
{
    Failed = 0,
    NoSavedSession = 1,
    SessionRejected = 2,
    InvalidLocalSession = 3,
    IdentityMismatch = 4,
    OwnershipNotProven = 5,
    PicsAccessTokenDenied = 6,
    ProductInfoUnavailable = 7,
    MissingPicsToken = 8,
    NoDepots = 9,
    NoVisibleManifests = 10,
    Discovered = 11,
    Cancelled = 12,
    TimedOut = 13,
}

/// <summary>
/// Step 08 result contract. Raw ownership-ticket bytes and PICS access-token
/// values are deliberately excluded. Only non-secret depot/manifest metadata
/// needed to prove discovery is retained.
/// </summary>
public sealed record SteamContentDiscoveryResult(
    SteamContentDiscoveryOutcome Outcome,
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
    uint? PicsChangeNumber,
    IReadOnlyList<SteamDepotDiscovery> Depots,
    string? AccountName,
    string? SteamId64,
    string? CurrentEndPoint,
    TimeSpan Elapsed,
    string? Error,
    uint? LoginId = null)
{
    public int DepotCount => Depots.Count;

    public int ManifestCount => Depots.Sum(depot => depot.Manifests.Count);

    public bool DiscoveryProven => Outcome == SteamContentDiscoveryOutcome.Discovered;

    public string Summary => Outcome switch
    {
        SteamContentDiscoveryOutcome.Discovered =>
            $"DISCOVERY PASS — {DepotCount} depots / {ManifestCount} manifests",
        SteamContentDiscoveryOutcome.NoSavedSession => "DISCOVERY — no saved session",
        SteamContentDiscoveryOutcome.SessionRejected => "DISCOVERY BLOCKED — saved session rejected",
        SteamContentDiscoveryOutcome.InvalidLocalSession => "DISCOVERY BLOCKED — invalid saved session",
        SteamContentDiscoveryOutcome.IdentityMismatch => "DISCOVERY BLOCKED — identity mismatch",
        SteamContentDiscoveryOutcome.OwnershipNotProven => "DISCOVERY BLOCKED — ownership not proven",
        SteamContentDiscoveryOutcome.PicsAccessTokenDenied => "DISCOVERY BLOCKED — PICS token denied",
        SteamContentDiscoveryOutcome.ProductInfoUnavailable => "DISCOVERY FAIL — app info unavailable",
        SteamContentDiscoveryOutcome.MissingPicsToken => "DISCOVERY FAIL — PICS token required",
        SteamContentDiscoveryOutcome.NoDepots => "DISCOVERY FAIL — no depots found",
        SteamContentDiscoveryOutcome.NoVisibleManifests => "DISCOVERY FAIL — no visible manifests",
        SteamContentDiscoveryOutcome.TimedOut => "DISCOVERY TIMEOUT",
        SteamContentDiscoveryOutcome.Cancelled => "DISCOVERY CANCELLED",
        _ => "DISCOVERY FAIL",
    };
}
