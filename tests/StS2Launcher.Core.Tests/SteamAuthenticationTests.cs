using Microsoft.VisualStudio.TestTools.UnitTesting;
using SteamKit2;
using StS2Launcher.Core;

namespace StS2Launcher.Core.Tests;

[TestClass]
public sealed class SteamAuthenticationTests
{
    [TestMethod]
    public async Task DeviceCodeChallengeIsObservedWithoutProvidingCode()
    {
        var authenticator = new SteamGuardChallengeAuthenticator();

        var ex = await Assert.ThrowsExactlyAsync<SteamGuardChallengeRequiredException>(
            () => authenticator.GetDeviceCodeAsync(previousCodeWasIncorrect: false));

        Assert.AreEqual(SteamGuardChallengeKind.DeviceCode, ex.Challenge.Kind);
        Assert.IsFalse(ex.Challenge.PreviousCodeWasIncorrect);
        Assert.AreEqual(ex.Challenge, authenticator.LastChallenge);
    }

    [TestMethod]
    public async Task EmailChallengePreservesSteamAssociatedMessage()
    {
        var authenticator = new SteamGuardChallengeAuthenticator();

        var ex = await Assert.ThrowsExactlyAsync<SteamGuardChallengeRequiredException>(
            () => authenticator.GetEmailCodeAsync("m***@example.com", previousCodeWasIncorrect: true));

        Assert.AreEqual(SteamGuardChallengeKind.EmailCode, ex.Challenge.Kind);
        Assert.AreEqual("m***@example.com", ex.Challenge.AssociatedMessage);
        Assert.IsTrue(ex.Challenge.PreviousCodeWasIncorrect);
    }

    [TestMethod]
    public async Task MobileConfirmationIsObservedWithoutAcceptingIt()
    {
        var authenticator = new SteamGuardChallengeAuthenticator();

        var ex = await Assert.ThrowsExactlyAsync<SteamGuardChallengeRequiredException>(
            () => authenticator.AcceptDeviceConfirmationAsync());

        Assert.AreEqual(SteamGuardChallengeKind.DeviceConfirmation, ex.Challenge.Kind);
    }

    [TestMethod]
    public void AuthenticatedResultRequiresExplicitAuthenticatedOutcome()
    {
        var result = new SteamAuthenticationResult(
            Outcome: SteamAuthenticationOutcome.Authenticated,
            CmConnected: true,
            AuthSessionStarted: true,
            LoggedOnCallbackReceived: true,
            LogonResult: EResult.OK,
            ExtendedLogonResult: EResult.OK,
            AccountName: "example",
            SteamId64: "76561198000000000",
            GuardChallenge: null,
            CurrentEndPoint: "cm.example:443",
            Elapsed: TimeSpan.FromSeconds(1),
            Error: null);

        Assert.IsTrue(result.Authenticated);
        Assert.IsFalse(result.GuardRequired);
        Assert.AreEqual("STEAM AUTH PASS — authenticated", result.Summary);
    }

    [TestMethod]
    public void GuardRequiredIsDistinctFromAuthenticationFailure()
    {
        var challenge = new SteamGuardChallenge(
            SteamGuardChallengeKind.DeviceConfirmation,
            AssociatedMessage: null,
            PreviousCodeWasIncorrect: false);
        var result = new SteamAuthenticationResult(
            Outcome: SteamAuthenticationOutcome.GuardRequired,
            CmConnected: true,
            AuthSessionStarted: true,
            LoggedOnCallbackReceived: false,
            LogonResult: null,
            ExtendedLogonResult: null,
            AccountName: null,
            SteamId64: null,
            GuardChallenge: challenge,
            CurrentEndPoint: "cm.example:443",
            Elapsed: TimeSpan.FromSeconds(1),
            Error: null);

        Assert.IsFalse(result.Authenticated);
        Assert.IsTrue(result.GuardRequired);
        StringAssert.Contains(result.Summary, "STEAM GUARD REQUIRED");
    }

    [TestMethod]
    public async Task BlankCredentialsAreRejectedBeforeNetworkWork()
    {
        var attempt = new SteamAuthenticationAttempt();

        await Assert.ThrowsExactlyAsync<ArgumentException>(() =>
            attempt.RunAsync(" ", "password", TimeSpan.FromSeconds(1)));
        await Assert.ThrowsExactlyAsync<ArgumentException>(() =>
            attempt.RunAsync("user", "", TimeSpan.FromSeconds(1)));
    }
}
