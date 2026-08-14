using Microsoft.VisualStudio.TestTools.UnitTesting;
using SteamKit2;
using StS2Launcher.Core;

namespace StS2Launcher.Core.Tests;

[TestClass]
public sealed class SteamSessionRecoveryTests
{
    [TestMethod]
    public void ValidAuthenticatedSessionIsKept()
    {
        var result = Result(SteamSessionResumeOutcome.Authenticated, EResult.OK, identityMatched: true);
        Assert.AreEqual(
            SteamSessionRecoveryAction.KeepSavedSession,
            SteamSessionRecoveryPolicy.Evaluate(result));
    }

    [TestMethod]
    public void NoSavedSessionDoesNotRequestADelete()
    {
        var result = Result(SteamSessionResumeOutcome.NoSavedSession, null, savedSessionFound: false);
        Assert.AreEqual(
            SteamSessionRecoveryAction.KeepSavedSession,
            SteamSessionRecoveryPolicy.Evaluate(result));
    }

    [TestMethod]
    public void InvalidLocalSessionIsCleared()
    {
        var result = Result(SteamSessionResumeOutcome.InvalidLocalSession, null);
        Assert.AreEqual(
            SteamSessionRecoveryAction.ClearSavedSessionAndRequireInteractiveAuthentication,
            SteamSessionRecoveryPolicy.Evaluate(result));
    }

    [TestMethod]
    public void IdentityMismatchIsCleared()
    {
        var result = Result(SteamSessionResumeOutcome.IdentityMismatch, EResult.OK, identityMatched: false);
        Assert.AreEqual(
            SteamSessionRecoveryAction.ClearSavedSessionAndRequireInteractiveAuthentication,
            SteamSessionRecoveryPolicy.Evaluate(result));
    }

    [TestMethod]
    [DataRow(EResult.InvalidPassword)]
    [DataRow(EResult.Revoked)]
    [DataRow(EResult.Expired)]
    public void DefinitelyUnusableCredentialResultsAreCleared(EResult resultCode)
    {
        var result = Result(SteamSessionResumeOutcome.Rejected, resultCode);
        Assert.AreEqual(
            SteamSessionRecoveryAction.ClearSavedSessionAndRequireInteractiveAuthentication,
            SteamSessionRecoveryPolicy.Evaluate(result));
    }

    [TestMethod]
    public void DefinitelyUnusableExtendedResultIsAlsoCleared()
    {
        var result = Result(
            SteamSessionResumeOutcome.Rejected,
            EResult.Fail,
            extended: EResult.Expired);
        Assert.AreEqual(
            SteamSessionRecoveryAction.ClearSavedSessionAndRequireInteractiveAuthentication,
            SteamSessionRecoveryPolicy.Evaluate(result));
    }

    [TestMethod]
    [DataRow(EResult.ServiceUnavailable)]
    [DataRow(EResult.RateLimitExceeded)]
    [DataRow(EResult.TryAnotherCM)]
    public void TransientOrRoutingResultsPreserveSavedSession(EResult resultCode)
    {
        var result = Result(SteamSessionResumeOutcome.Rejected, resultCode);
        Assert.AreEqual(
            SteamSessionRecoveryAction.KeepSavedSession,
            SteamSessionRecoveryPolicy.Evaluate(result));
    }

    [TestMethod]
    public void TimeoutAndCancellationPreserveSavedSession()
    {
        Assert.AreEqual(
            SteamSessionRecoveryAction.KeepSavedSession,
            SteamSessionRecoveryPolicy.Evaluate(Result(SteamSessionResumeOutcome.TimedOut, null)));
        Assert.AreEqual(
            SteamSessionRecoveryAction.KeepSavedSession,
            SteamSessionRecoveryPolicy.Evaluate(Result(SteamSessionResumeOutcome.Cancelled, null)));
    }

    private static SteamSessionResumeResult Result(
        SteamSessionResumeOutcome outcome,
        EResult? primary,
        EResult? extended = null,
        bool identityMatched = false,
        bool savedSessionFound = true) =>
        new(
            Outcome: outcome,
            SavedSessionFound: savedSessionFound,
            CmConnected: outcome is not SteamSessionResumeOutcome.NoSavedSession and not SteamSessionResumeOutcome.InvalidLocalSession,
            LoggedOnCallbackReceived: primary.HasValue,
            LogonResult: primary,
            ExtendedLogonResult: extended,
            IdentityMatched: identityMatched,
            AccountName: savedSessionFound ? "example" : null,
            SteamId64: savedSessionFound ? "76561198000000000" : null,
            CurrentEndPoint: null,
            Elapsed: TimeSpan.FromSeconds(1),
            Error: null);
}
