using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StS2Launcher.Core;

namespace StS2Launcher.Core.Tests;

[TestClass]
public sealed class SteamPersistentSessionTests
{
    [TestMethod]
    public void RefreshTokenMetadataParsesJwtTimingClaims()
    {
        const long issued = 1_700_000_000;
        const long expires = 1_800_000_000;
        var payload = "{\"iat\":" + issued + ",\"exp\":" + expires + ",\"sub\":\"test\"}";
        var token = "header." + Base64Url(payload) + ".signature";

        Assert.IsTrue(SteamRefreshTokenMetadata.TryParse(token, out var metadata));
        Assert.IsNotNull(metadata);
        Assert.AreEqual(DateTimeOffset.FromUnixTimeSeconds(issued), metadata.IssuedAtUtc);
        Assert.AreEqual(DateTimeOffset.FromUnixTimeSeconds(expires), metadata.ExpiresAtUtc);
        Assert.IsFalse(metadata.IsExpiredAt(DateTimeOffset.FromUnixTimeSeconds(issued + 1)));
        Assert.IsTrue(metadata.IsExpiredAt(DateTimeOffset.FromUnixTimeSeconds(expires)));
    }

    [TestMethod]
    public void RefreshTokenMetadataRejectsMalformedToken()
    {
        Assert.IsFalse(SteamRefreshTokenMetadata.TryParse("not-a-jwt", out var metadata));
        Assert.IsNull(metadata);
    }


    [TestMethod]
    public void PersistentLogOnDetailsMatchPersistentAuthContract()
    {
        var details = SteamPersistentLogOnDetails.Create(
            "example",
            "test-refresh-token");

        Assert.AreEqual("example", details.Username);
        Assert.AreEqual("test-refresh-token", details.AccessToken);
        Assert.IsTrue(details.ShouldRememberPassword);
        Assert.IsTrue(details.LoginID.HasValue);
        Assert.AreNotEqual(0u, details.LoginID.Value);
        Assert.AreEqual(SteamKit2.EOSType.IOSUnknown, details.ClientOSType);
        Assert.AreEqual(SteamAuthenticationAttempt.DeviceFriendlyName, details.MachineName);
        Assert.IsNull(details.Password);
    }

    [TestMethod]
    public void LoginIdIsNonZero()
    {
        Assert.AreNotEqual(0u, SteamLoginIdentity.Create());
    }

    private static string Base64Url(string value)
    {
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(value))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
