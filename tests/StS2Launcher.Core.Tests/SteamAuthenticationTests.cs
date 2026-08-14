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
        Assert.IsFalse(authenticator.MobileApprovalRequested);
    }

    [TestMethod]
    public async Task EmailChallengePreservesSteamAssociatedMessageWithoutProvidingCode()
    {
        var authenticator = new SteamGuardChallengeAuthenticator();

        var ex = await Assert.ThrowsExactlyAsync<SteamGuardChallengeRequiredException>(
            () => authenticator.GetEmailCodeAsync("m***@example.com", previousCodeWasIncorrect: true));

        Assert.AreEqual(SteamGuardChallengeKind.EmailCode, ex.Challenge.Kind);
        Assert.AreEqual("m***@example.com", ex.Challenge.AssociatedMessage);
        Assert.IsTrue(ex.Challenge.PreviousCodeWasIncorrect);
        Assert.IsFalse(authenticator.MobileApprovalRequested);
    }

    [TestMethod]
    public async Task MobileConfirmationOptsIntoSteamKitPolling()
    {
        var progress = new RecordingProgress<SteamAuthenticationProgress>();
        var authenticator = new SteamGuardChallengeAuthenticator(progress);

        var shouldPoll = await authenticator.AcceptDeviceConfirmationAsync();

        Assert.IsTrue(shouldPoll);
        Assert.IsTrue(authenticator.MobileApprovalRequested);
        Assert.AreEqual(
            SteamGuardChallengeKind.DeviceConfirmation,
            authenticator.LastChallenge?.Kind);
        Assert.AreEqual(1, progress.Values.Count);
        Assert.AreEqual(
            SteamAuthenticationStage.WaitingForMobileApproval,
            progress.Values[0].Stage);
        StringAssert.Contains(progress.Values[0].Message, "Steam Guard mobile approval");
    }

    [TestMethod]
    public void MobileApprovedAuthenticatedResultHasDistinctSummary()
    {
        var result = new SteamAuthenticationResult(
            Outcome: SteamAuthenticationOutcome.Authenticated,
            CmConnected: true,
            AuthSessionStarted: true,
            MobileApprovalRequested: true,
            MobileApprovalCompleted: true,
            LoggedOnCallbackReceived: true,
            LogonResult: EResult.OK,
            ExtendedLogonResult: EResult.OK,
            AccountName: "example",
            SteamId64: "76561198000000000",
            GuardChallenge: new SteamGuardChallenge(
                SteamGuardChallengeKind.DeviceConfirmation,
                AssociatedMessage: null,
                PreviousCodeWasIncorrect: false),
            CurrentEndPoint: "cm.example:443",
            Elapsed: TimeSpan.FromSeconds(1),
            Error: null);

        Assert.IsTrue(result.Authenticated);
        Assert.IsFalse(result.GuardRequired);
        Assert.IsTrue(result.MobileApprovalRequested);
        Assert.IsTrue(result.MobileApprovalCompleted);
        Assert.AreEqual("STEAM AUTH PASS — Steam Guard approved", result.Summary);
    }

    [TestMethod]
    public void GuardRequiredIsDistinctFromAuthenticationFailure()
    {
        var challenge = new SteamGuardChallenge(
            SteamGuardChallengeKind.DeviceCode,
            AssociatedMessage: null,
            PreviousCodeWasIncorrect: false);
        var result = new SteamAuthenticationResult(
            Outcome: SteamAuthenticationOutcome.GuardRequired,
            CmConnected: true,
            AuthSessionStarted: true,
            MobileApprovalRequested: false,
            MobileApprovalCompleted: false,
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
    public void TimeoutIsDistinctFromUserCancellation()
    {
        var result = new SteamAuthenticationResult(
            Outcome: SteamAuthenticationOutcome.TimedOut,
            CmConnected: true,
            AuthSessionStarted: true,
            MobileApprovalRequested: true,
            MobileApprovalCompleted: false,
            LoggedOnCallbackReceived: false,
            LogonResult: null,
            ExtendedLogonResult: null,
            AccountName: null,
            SteamId64: null,
            GuardChallenge: new SteamGuardChallenge(
                SteamGuardChallengeKind.DeviceConfirmation,
                AssociatedMessage: null,
                PreviousCodeWasIncorrect: false),
            CurrentEndPoint: "cm.example:443",
            Elapsed: TimeSpan.FromMinutes(3),
            Error: "Authentication timed out after 180s.");

        Assert.AreEqual("STEAM AUTH TIMEOUT", result.Summary);
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

    private sealed class RecordingProgress<T> : IProgress<T>
    {
        public List<T> Values { get; } = [];

        public void Report(T value) => Values.Add(value);
    }
}
