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
    public void PersistedMobileApprovedAuthenticatedResultHasDistinctSummary()
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
            SessionPersisted: true,
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
        Assert.IsTrue(result.SessionPersisted);
        Assert.AreEqual("STEAM AUTH PASS — Guard approved + session saved", result.Summary);
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
            SessionPersisted: false,
            AccountName: null,
            SteamId64: null,
            GuardChallenge: challenge,
            CurrentEndPoint: "cm.example:443",
            Elapsed: TimeSpan.FromSeconds(1),
            Error: null);

        Assert.IsFalse(result.Authenticated);
        Assert.IsTrue(result.GuardRequired);
        Assert.IsFalse(result.SessionPersisted);
        StringAssert.Contains(result.Summary, "STEAM GUARD REQUIRED");
    }

    [TestMethod]
    public void ResumeAuthenticatedResultRequiresIdentityMatchInContract()
    {
        var result = new SteamSessionResumeResult(
            Outcome: SteamSessionResumeOutcome.Authenticated,
            SavedSessionFound: true,
            CmConnected: true,
            LoggedOnCallbackReceived: true,
            LogonResult: EResult.OK,
            ExtendedLogonResult: EResult.OK,
            IdentityMatched: true,
            AccountName: "example",
            SteamId64: "76561198000000000",
            CurrentEndPoint: "cm.example:443",
            Elapsed: TimeSpan.FromSeconds(1),
            Error: null);

        Assert.IsTrue(result.Authenticated);
        Assert.IsTrue(result.IdentityMatched);
        Assert.AreEqual("SAVED SESSION PASS — authenticated", result.Summary);
    }

    [TestMethod]
    public void ResumeNoSavedSessionIsDistinctFromFailure()
    {
        var result = new SteamSessionResumeResult(
            Outcome: SteamSessionResumeOutcome.NoSavedSession,
            SavedSessionFound: false,
            CmConnected: false,
            LoggedOnCallbackReceived: false,
            LogonResult: null,
            ExtendedLogonResult: null,
            IdentityMatched: false,
            AccountName: null,
            SteamId64: null,
            CurrentEndPoint: null,
            Elapsed: TimeSpan.Zero,
            Error: null);

        Assert.IsFalse(result.Authenticated);
        Assert.AreEqual("SAVED SESSION — none", result.Summary);
    }

    [TestMethod]
    public async Task BlankCredentialsAreRejectedBeforeNetworkWork()
    {
        var store = new SteamSessionStore(new InMemoryCredentialStore());
        var attempt = new SteamAuthenticationAttempt(store);

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

    private sealed class InMemoryCredentialStore : ICredentialStore
    {
        private readonly Dictionary<string, string> _values = new(StringComparer.Ordinal);

        public void Set(string key, string value) => _values[key] = value;
        public string? Get(string key) => _values.TryGetValue(key, out var value) ? value : null;
        public bool Delete(string key) => _values.Remove(key);
    }
}
