using Microsoft.VisualStudio.TestTools.UnitTesting;
using StS2Launcher.Core;
using SteamKit2;

namespace StS2Launcher.Core.Tests;

[TestClass]
public sealed class SteamOwnershipVerificationTests
{
    [TestMethod]
    public void TargetAppIdIsSlayTheSpire2()
    {
        Assert.AreEqual(2868840u, SteamOwnershipVerificationAttempt.TargetAppId);
    }

    [TestMethod]
    public void OkMatchingNonEmptyTicketProvesOwnership()
    {
        var outcome = SteamOwnershipDecision.EvaluateTicket(
            SteamOwnershipVerificationAttempt.TargetAppId,
            EResult.OK,
            SteamOwnershipVerificationAttempt.TargetAppId,
            128);

        Assert.AreEqual(SteamOwnershipVerificationOutcome.Owned, outcome);
    }

    [TestMethod]
    public void NonOkResultDoesNotProveOwnership()
    {
        var outcome = SteamOwnershipDecision.EvaluateTicket(
            SteamOwnershipVerificationAttempt.TargetAppId,
            EResult.AccessDenied,
            SteamOwnershipVerificationAttempt.TargetAppId,
            128);

        Assert.AreEqual(SteamOwnershipVerificationOutcome.TicketRejected, outcome);
    }

    [TestMethod]
    public void EmptyTicketDoesNotProveOwnership()
    {
        var outcome = SteamOwnershipDecision.EvaluateTicket(
            SteamOwnershipVerificationAttempt.TargetAppId,
            EResult.OK,
            SteamOwnershipVerificationAttempt.TargetAppId,
            0);

        Assert.AreEqual(SteamOwnershipVerificationOutcome.EmptyTicket, outcome);
    }

    [TestMethod]
    public void WrongAppIdDoesNotProveOwnership()
    {
        var outcome = SteamOwnershipDecision.EvaluateTicket(
            SteamOwnershipVerificationAttempt.TargetAppId,
            EResult.OK,
            730,
            128);

        Assert.AreEqual(SteamOwnershipVerificationOutcome.UnexpectedAppId, outcome);
    }

    [TestMethod]
    public void OwnershipResultNeverExposesRawTicketBytes()
    {
        var exposesByteArray = typeof(SteamOwnershipVerificationResult)
            .GetProperties()
            .Any(property => property.PropertyType == typeof(byte[]));

        Assert.IsFalse(exposesByteArray);
    }

    [TestMethod]
    public void OwnedSummaryNamesTargetApp()
    {
        var result = new SteamOwnershipVerificationResult(
            Outcome: SteamOwnershipVerificationOutcome.Owned,
            TargetAppId: SteamOwnershipVerificationAttempt.TargetAppId,
            SavedSessionFound: true,
            CmConnected: true,
            LoggedOnCallbackReceived: true,
            LogonResult: EResult.OK,
            ExtendedLogonResult: EResult.OK,
            IdentityMatched: true,
            OwnershipTicketCallbackReceived: true,
            OwnershipResult: EResult.OK,
            OwnershipAppId: SteamOwnershipVerificationAttempt.TargetAppId,
            OwnershipTicketLength: 128,
            AccountName: "test",
            SteamId64: "76561198000000000",
            CurrentEndPoint: "example:443",
            Elapsed: TimeSpan.FromSeconds(1),
            Error: null,
            LoginId: 123);

        Assert.IsTrue(result.OwnershipProven);
        Assert.AreEqual("OWNERSHIP PASS — App 2868840 owned", result.Summary);
    }
}
