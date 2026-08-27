namespace StS2Launcher.Core;

public enum RealStS2CompatibilityTargetAuditGate
{
    SourceAdmissionAndOfflineReady = 1,
    ExactRiskCallSiteAudit = 2,
    DeterministicCandidateSelection = 3,
    FinalIsolationAudit = 4,
}
