using Foundation;
using StS2Launcher.Core;
using StS2Launcher.Step05.iOS.Platform;
using UIKit;

namespace StS2Launcher.Step05.iOS;

public sealed class RootViewController : UIViewController
{
    private readonly KeychainCredentialStore _credentialStore = new();
    private readonly KeychainProbe _keychainProbe;
    private readonly SteamSessionStore _sessionStore;
    private readonly SteamConnectionProbe _steamProbe = new();
    private readonly SteamAuthenticationAttempt _authenticationAttempt;
    private readonly SteamSessionResumeAttempt _resumeAttempt;
    private readonly SteamOwnershipVerificationAttempt _ownershipAttempt;
    private readonly SteamContentDiscoveryAttempt _contentDiscoveryAttempt;
    private readonly SteamSingleFileDownloadAttempt _singleFileDownloadAttempt;
    private readonly SteamFullDepotDownloadAttempt _fullDepotDownloadAttempt;
    private readonly SteamResumableDepotDownloadAttempt _resumableDepotDownloadAttempt;
    private readonly SteamManagedInstallAttempt _managedInstallAttempt;
    private readonly SteamDownloadCacheMaintenance _downloadCacheMaintenance;
    private readonly SteamOfflineInstallInspection _offlineInstallInspection;
    private readonly SteamCompatibilityInventoryInspection _compatibilityInventoryInspection;
    private readonly GodotFoundationGateSequence _godotFoundationGates = new();
    private readonly ManagedPreparationFoundation _managedPreparationFoundation;
    private readonly ManagedPreparationGateSequence _managedPreparationGates = new();
    private readonly CompatibilityCallSiteAnalysis _compatibilityCallSiteAnalysis;
    private readonly CompatibilityCallSiteGateSequence _compatibilityCallSiteGates = new();
    private readonly RealAssemblyRewriteWorkspace _realAssemblyRewriteWorkspace;
    private readonly RealAssemblyRewriteGateSequence _realAssemblyRewriteGates = new();
    private readonly ExpressionInterpreterCompatibility _expressionInterpreterCompatibility;
    private readonly ExpressionInterpreterCompatibilityGateSequence _expressionInterpreterCompatibilityGates = new();
    private readonly DynamicManagedExecutionFoundation _dynamicManagedExecutionFoundation;
    private readonly DynamicManagedExecutionGateSequence _dynamicManagedExecutionGates = new();
    private readonly PreparedRuntimeFrameworkBinding _preparedRuntimeFrameworkBinding;
    private readonly RuntimeBindingDiagnosticsExporter _runtimeBindingDiagnosticsExporter;
    private readonly RuntimeFrameworkBindingGateSequence _runtimeFrameworkBindingGates = new();

    private UILabel? _foundationResultLabel;
    private UILabel? _foundationDetailLabel;
    private UILabel? _authResultLabel;
    private UILabel? _authDetailLabel;
    private UILabel? _savedSessionLabel;
    private UILabel? _autoRestoreResultLabel;
    private UILabel? _autoRestoreDetailLabel;
    private UILabel? _resumeResultLabel;
    private UILabel? _resumeDetailLabel;
    private UILabel? _ownershipResultLabel;
    private UILabel? _ownershipDetailLabel;
    private UILabel? _discoveryResultLabel;
    private UILabel? _discoveryDetailLabel;
    private UILabel? _singleFileResultLabel;
    private UILabel? _singleFileDetailLabel;
    private UILabel? _fullDepotResultLabel;
    private UILabel? _fullDepotDetailLabel;
    private UILabel? _resumableDepotResultLabel;
    private UILabel? _resumableDepotDetailLabel;
    private UILabel? _managedInstallResultLabel;
    private UILabel? _managedInstallDetailLabel;
    private UILabel? _offlineInstallResultLabel;
    private UILabel? _offlineInstallDetailLabel;
    private UILabel? _compatibilityInventoryResultLabel;
    private UILabel? _compatibilityInventoryDetailLabel;
    private UILabel? _godotFoundationResultLabel;
    private UILabel? _godotFoundationDetailLabel;
    private UILabel? _managedPreparationResultLabel;
    private UILabel? _managedPreparationDetailLabel;
    private UILabel? _compatibilityCallSiteResultLabel;
    private UILabel? _compatibilityCallSiteDetailLabel;
    private UILabel? _realAssemblyRewriteResultLabel;
    private UILabel? _realAssemblyRewriteDetailLabel;
    private UILabel? _expressionInterpreterCompatibilityResultLabel;
    private UILabel? _expressionInterpreterCompatibilityDetailLabel;
    private UILabel? _dynamicManagedExecutionResultLabel;
    private UILabel? _dynamicManagedExecutionDetailLabel;
    private UILabel? _runtimeFrameworkBindingResultLabel;
    private UILabel? _runtimeFrameworkBindingDetailLabel;
    private UILabel? _runtimeBindingDiagnosticsExportResultLabel;
    private UILabel? _statusLabel;
    private UILabel? _lifecycleLabel;
    private UITextField? _usernameField;
    private UITextField? _passwordField;
    private UIButton? _foundationButton;
    private UIButton? _authButton;
    private UIButton? _resumeButton;
    private UIButton? _ownershipButton;
    private UIButton? _discoveryButton;
    private UIButton? _singleFileButton;
    private UIButton? _fullDepotButton;
    private UIButton? _resumableDepotButton;
    private UIButton? _managedInstallButton;
    private UIButton? _prepareRepairTestButton;
    private UIButton? _prepareUpdateTestButton;
    private UIButton? _clearDownloadCacheButton;
    private UIButton? _prepareFreshDownloadTestButton;
    private UIButton? _offlineInstallButton;
    private UIButton? _compatibilityInventoryButton;
    private UIButton? _godotFoundationStartButton;
    private UIButton? _godotFoundationGateDButton;
    private UIButton? _managedPreparationButton;
    private UIButton? _compatibilityCallSiteButton;
    private UIButton? _realAssemblyRewriteButton;
    private UIButton? _expressionInterpreterCompatibilityButton;
    private UIButton? _dynamicManagedExecutionButton;
    private UIButton? _runtimeFrameworkBindingButton;
    private UIButton? _runtimeBindingDiagnosticsExportButton;
    private UIView? _godotHostContainer;
    private UIButton? _signOutButton;
    private UIButton? _cancelOperationButton;
    private CancellationTokenSource? _operationCts;
    private bool _uiStartupPassed;
    private bool _lifecycleActive;
    private bool _automaticRestoreStarted;
    private bool _godotSessionStarted;
    private bool _godotProcessRequiresRestart;

    public RootViewController()
    {
        _keychainProbe = new KeychainProbe(_credentialStore);
        _sessionStore = new SteamSessionStore(_credentialStore);
        _authenticationAttempt = new SteamAuthenticationAttempt(_sessionStore);
        _resumeAttempt = new SteamSessionResumeAttempt(_sessionStore);
        _ownershipAttempt = new SteamOwnershipVerificationAttempt(_sessionStore);
        _contentDiscoveryAttempt = new SteamContentDiscoveryAttempt(_sessionStore);
        var documentsRoot = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        _singleFileDownloadAttempt = new SteamSingleFileDownloadAttempt(
            _sessionStore,
            Path.Combine(documentsRoot, "StS2Launcher"));
        _fullDepotDownloadAttempt = new SteamFullDepotDownloadAttempt(
            _sessionStore,
            Path.Combine(documentsRoot, "StS2Launcher"));
        _resumableDepotDownloadAttempt = new SteamResumableDepotDownloadAttempt(
            _sessionStore,
            Path.Combine(documentsRoot, "StS2Launcher"));
        var launcherDataRoot = Path.Combine(documentsRoot, "StS2Launcher");
        _managedInstallAttempt = new SteamManagedInstallAttempt(
            _sessionStore,
            launcherDataRoot);
        _downloadCacheMaintenance = new SteamDownloadCacheMaintenance(launcherDataRoot);
        _offlineInstallInspection = new SteamOfflineInstallInspection(launcherDataRoot);
        _compatibilityInventoryInspection = new SteamCompatibilityInventoryInspection(launcherDataRoot);
        _managedPreparationFoundation = new ManagedPreparationFoundation(launcherDataRoot);
        _compatibilityCallSiteAnalysis = new CompatibilityCallSiteAnalysis(launcherDataRoot);
        _realAssemblyRewriteWorkspace = new RealAssemblyRewriteWorkspace(launcherDataRoot);
        _expressionInterpreterCompatibility = new ExpressionInterpreterCompatibility(launcherDataRoot);
        _dynamicManagedExecutionFoundation = new DynamicManagedExecutionFoundation(
            launcherDataRoot,
            Path.Combine(NSBundle.MainBundle.BundlePath, DynamicManagedExecutionFoundation.BundleFixtureDirectoryName));
        _preparedRuntimeFrameworkBinding = new PreparedRuntimeFrameworkBinding(launcherDataRoot);
        _runtimeBindingDiagnosticsExporter = new RuntimeBindingDiagnosticsExporter(launcherDataRoot);
    }

    public override void ViewDidLoad()
    {
        base.ViewDidLoad();

        View!.BackgroundColor = UIColor.SystemBackground;

        var scroll = new UIScrollView
        {
            TranslatesAutoresizingMaskIntoConstraints = false
        };
        View.AddSubview(scroll);

        var content = new UIStackView
        {
            TranslatesAutoresizingMaskIntoConstraints = false,
            Axis = UILayoutConstraintAxis.Vertical,
            Alignment = UIStackViewAlignment.Fill,
            Spacing = 14,
            LayoutMarginsRelativeArrangement = true,
            LayoutMargins = new UIEdgeInsets(28, 24, 28, 24)
        };
        scroll.AddSubview(content);

        NSLayoutConstraint.ActivateConstraints(
        [
            scroll.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
            scroll.BottomAnchor.ConstraintEqualTo(View.BottomAnchor),
            scroll.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
            scroll.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),

            content.TopAnchor.ConstraintEqualTo(scroll.ContentLayoutGuide.TopAnchor),
            content.BottomAnchor.ConstraintEqualTo(scroll.ContentLayoutGuide.BottomAnchor),
            content.LeadingAnchor.ConstraintEqualTo(scroll.ContentLayoutGuide.LeadingAnchor),
            content.TrailingAnchor.ConstraintEqualTo(scroll.ContentLayoutGuide.TrailingAnchor),
            content.WidthAnchor.ConstraintEqualTo(scroll.FrameLayoutGuide.WidthAnchor)
        ]);

        content.AddArrangedSubview(Label(
            "StS2 Launcher",
            UIFont.BoldSystemFontOfSize(34),
            UIColor.Label));

        content.AddArrangedSubview(Label(
            "STEP 21.1 — BINDING DIAGNOSTIC EXPORT",
            UIFont.BoldSystemFontOfSize(18),
            UIColor.SecondaryLabel));

        content.AddArrangedSubview(Label(
            "Version 0.0.57",
            UIFont.SystemFontOfSize(17),
            UIColor.SecondaryLabel));

        content.AddArrangedSubview(Label(
            "STEP 21 LOGIC PRESERVED • FULL BLOCKER REPORT • FILES APP EXPORT",
            UIFont.BoldSystemFontOfSize(14),
            UIColor.SecondaryLabel));

        content.AddArrangedSubview(Label(
            "Step 21 physically passed A–D and produced an authoritative binding plan with 47 explicit blockers and Runtime closure ready: NO. Step 21.1 is a reporting-only hotfix: the physically proven Step 21 binding/preparation implementation is unchanged. It reads the already persisted runtime-binding-plan.json and writes a complete plain-text blocker report under Documents/StS2Launcher. iOS local file sharing is enabled so the report can be retrieved from Files instead of screenshotting long diagnostics. If the existing plan survived the app update, you can export it immediately without rerunning A–D.",
            UIFont.SystemFontOfSize(14),
            UIColor.SecondaryLabel));

        content.AddArrangedSubview(Separator());

        content.AddArrangedSubview(Label(
            "Steps 01–05 regression",
            UIFont.BoldSystemFontOfSize(24),
            UIColor.Label));

        _foundationButton = SystemButton("Run Foundation 5/5 Regression", 16);
        _foundationButton.TouchUpInside += async (_, _) => await RunFoundationVerificationAsync();
        content.AddArrangedSubview(_foundationButton);

        _foundationResultLabel = Label(
            "FOUNDATION: NOT RUN",
            UIFont.BoldSystemFontOfSize(19),
            UIColor.Label);
        content.AddArrangedSubview(_foundationResultLabel);

        _foundationDetailLabel = Label(
            "The proven Steps 01–05 checks remain available unchanged.",
            UIFont.SystemFontOfSize(14),
            UIColor.SecondaryLabel);
        content.AddArrangedSubview(_foundationDetailLabel);

        content.AddArrangedSubview(Separator());

        content.AddArrangedSubview(Label(
            "Create / replace saved Steam session",
            UIFont.BoldSystemFontOfSize(25),
            UIColor.Label));

        _usernameField = TextField(
            placeholder: "Steam account name",
            secure: false,
            contentType: UITextContentType.Username);
        _usernameField.AutocorrectionType = UITextAutocorrectionType.No;
        _usernameField.AutocapitalizationType = UITextAutocapitalizationType.None;
        content.AddArrangedSubview(_usernameField);

        _passwordField = TextField(
            placeholder: "Steam password",
            secure: true,
            contentType: UITextContentType.Password);
        content.AddArrangedSubview(_passwordField);

        _authButton = SystemButton("Authenticate + Save Session", 17);
        _authButton.TouchUpInside += async (_, _) => await RunAuthenticationAsync();
        content.AddArrangedSubview(_authButton);

        _authResultLabel = Label(
            "STEAM AUTH: NOT RUN",
            UIFont.BoldSystemFontOfSize(21),
            UIColor.Label);
        content.AddArrangedSubview(_authResultLabel);

        _authDetailLabel = Label(
            "Use the proven Step 06.1 flow once. If Steam sends a mobile Guard prompt, approve it in Steam and return here. The refresh token is saved only after LoggedOnCallback returns OK.",
            UIFont.SystemFontOfSize(15),
            UIColor.SecondaryLabel);
        content.AddArrangedSubview(_authDetailLabel);

        content.AddArrangedSubview(Separator());

        content.AddArrangedSubview(Label(
            "Saved-session diagnostics / manual retry",
            UIFont.BoldSystemFontOfSize(25),
            UIColor.Label));

        _savedSessionLabel = Label(
            "Saved session: checking Keychain…",
            UIFont.BoldSystemFontOfSize(16),
            UIColor.Label);
        content.AddArrangedSubview(_savedSessionLabel);

        _autoRestoreResultLabel = Label(
            "AUTO SESSION: WAITING FOR ACTIVE LIFECYCLE",
            UIFont.BoldSystemFontOfSize(21),
            UIColor.Label);
        content.AddArrangedSubview(_autoRestoreResultLabel);

        _autoRestoreDetailLabel = Label(
            "The proven 06.3.1 saved-session regression automatically tests the Keychain session once after launch. No password or new Guard prompt is used.",
            UIFont.SystemFontOfSize(15),
            UIColor.SecondaryLabel);
        content.AddArrangedSubview(_autoRestoreDetailLabel);

        _resumeButton = SystemButton("Retry Saved Session Now (No Password)", 17);
        _resumeButton.TouchUpInside += async (_, _) => await RunSavedSessionResumeAsync();
        content.AddArrangedSubview(_resumeButton);

        _resumeResultLabel = Label(
            "SAVED SESSION: NOT RUN",
            UIFont.BoldSystemFontOfSize(21),
            UIColor.Label);
        content.AddArrangedSubview(_resumeResultLabel);

        _resumeDetailLabel = Label(
            "Manual retry now uses a fresh LoginID and the same persistent-token settings as automatic restore. AccessDenied remains non-destructive unless Steam reports a definitive expired/revoked credential.",
            UIFont.SystemFontOfSize(15),
            UIColor.SecondaryLabel);
        content.AddArrangedSubview(_resumeDetailLabel);

        content.AddArrangedSubview(Separator());

        content.AddArrangedSubview(Label(
            "Step 07 regression — Slay the Spire 2 ownership",
            UIFont.BoldSystemFontOfSize(25),
            UIColor.Label));

        _ownershipButton = SystemButton("Verify Slay the Spire 2 Ownership", 17);
        _ownershipButton.TouchUpInside += async (_, _) => await RunOwnershipVerificationAsync();
        content.AddArrangedSubview(_ownershipButton);

        _ownershipResultLabel = Label(
            "OWNERSHIP: NOT RUN",
            UIFont.BoldSystemFontOfSize(21),
            UIColor.Label);
        content.AddArrangedSubview(_ownershipResultLabel);

        _ownershipDetailLabel = Label(
            "Uses only the saved Keychain session and SteamApps.GetAppOwnershipTicket for App ID 2868840. A non-empty OK ticket proves this account owns the target app. The ticket payload is discarded immediately; no content request follows.",
            UIFont.SystemFontOfSize(15),
            UIColor.SecondaryLabel);
        content.AddArrangedSubview(_ownershipDetailLabel);

        content.AddArrangedSubview(Separator());

        content.AddArrangedSubview(Label(
            "Step 08 — depot / manifest discovery",
            UIFont.BoldSystemFontOfSize(25),
            UIColor.Label));

        _discoveryButton = SystemButton("Discover StS2 Depots + Manifests", 17);
        _discoveryButton.TouchUpInside += async (_, _) => await RunContentDiscoveryAsync();
        content.AddArrangedSubview(_discoveryButton);

        _discoveryResultLabel = Label(
            "DISCOVERY: NOT RUN",
            UIFont.BoldSystemFontOfSize(21),
            UIColor.Label);
        content.AddArrangedSubview(_discoveryResultLabel);

        _discoveryDetailLabel = Label(
            "Metadata only: after the Step 07 ownership gate, request PICS app info for App ID 2868840 and list numeric depot IDs plus visible branch manifest IDs. No depot key, manifest body, CDN request, chunk, or file download is allowed in this step.",
            UIFont.SystemFontOfSize(15),
            UIColor.SecondaryLabel);
        content.AddArrangedSubview(_discoveryDetailLabel);

        content.AddArrangedSubview(Separator());

        content.AddArrangedSubview(Label(
            "Step 09 — one controlled small StS2 file",
            UIFont.BoldSystemFontOfSize(25),
            UIColor.Label));

        _singleFileButton = SystemButton("Download One Small StS2 File", 17);
        _singleFileButton.TouchUpInside += async (_, _) => await RunSingleFileDownloadAsync();
        content.AddArrangedSubview(_singleFileButton);

        _singleFileResultLabel = Label(
            "SINGLE-FILE: NOT RUN",
            UIFont.BoldSystemFontOfSize(21),
            UIColor.Label);
        content.AddArrangedSubview(_singleFileResultLabel);

        _singleFileDetailLabel = Label(
            "Bounded Step 09 regression: one direct public depot, one in-memory manifest, one safe regular file <= 2 MiB, SHA-1 verification, then one atomic Documents write. Depot keys, request codes, CDN auth tokens, manifest bytes and chunk buffers are never displayed or persisted.",
            UIFont.SystemFontOfSize(15),
            UIColor.SecondaryLabel);
        content.AddArrangedSubview(_singleFileDetailLabel);

        content.AddArrangedSubview(Separator());

        content.AddArrangedSubview(Label(
            "Step 10 — minimal full-depot downloader",
            UIFont.BoldSystemFontOfSize(25),
            UIColor.Label));

        _fullDepotButton = SystemButton("Download One Full Public Depot", 17);
        _fullDepotButton.TouchUpInside += async (_, _) => await RunFullDepotDownloadAsync();
        content.AddArrangedSubview(_fullDepotButton);

        _fullDepotResultLabel = Label(
            "DEPOT: NOT RUN",
            UIFont.BoldSystemFontOfSize(21),
            UIColor.Label);
        content.AddArrangedSubview(_fullDepotResultLabel);

        _fullDepotDetailLabel = Label(
            "Step 10 regression remains unchanged: one selected direct public depot, temporary staging removed on cancel, per-file SHA-1 verification, and one atomic final-directory commit.",
            UIFont.SystemFontOfSize(15),
            UIColor.SecondaryLabel);
        content.AddArrangedSubview(_fullDepotDetailLabel);

        content.AddArrangedSubview(Separator());

        content.AddArrangedSubview(Label(
            "Step 11 — interrupted-download resume",
            UIFont.BoldSystemFontOfSize(25),
            UIColor.Label));

        _resumableDepotButton = SystemButton("Resume / Download One Public Depot", 17);
        _resumableDepotButton.TouchUpInside += async (_, _) => await RunResumableDepotDownloadAsync();
        content.AddArrangedSubview(_resumableDepotButton);

        _resumableDepotResultLabel = Label(
            "RESUME: NOT RUN",
            UIFont.BoldSystemFontOfSize(21),
            UIColor.Label);
        content.AddArrangedSubview(_resumableDepotResultLabel);

        _resumableDepotDetailLabel = Label(
            "For the physical Step 11 gate: start this download, wait until chunk/byte progress is non-zero, then force-quit the app from the app switcher. Relaunch and tap this same button. Step 11 must detect the deterministic staging tree, revalidate existing complete files/chunks, download only missing data, SHA-1 verify every file, and atomically commit the complete depot.",
            UIFont.SystemFontOfSize(15),
            UIColor.SecondaryLabel);
        content.AddArrangedSubview(_resumableDepotDetailLabel);

        content.AddArrangedSubview(Separator());

        content.AddArrangedSubview(Label(
            "Step 12.4.1 — completed install/update/repair + cache regression controls",
            UIFont.BoldSystemFontOfSize(25),
            UIColor.Label));

        _managedInstallButton = SystemButton("Inspect + Install / Update / Repair", 17);
        _managedInstallButton.TouchUpInside += async (_, _) => await RunManagedInstallAsync();
        content.AddArrangedSubview(_managedInstallButton);

        _prepareRepairTestButton = SystemButton("Prepare Repair Test (Corrupt One Managed File)", 15);
        _prepareRepairTestButton.TouchUpInside += async (_, _) => await PrepareRepairTestAsync();
        content.AddArrangedSubview(_prepareRepairTestButton);

        _prepareUpdateTestButton = SystemButton("Prepare Update Test (Stale Receipt + One Changed File Identity)", 15);
        _prepareUpdateTestButton.TouchUpInside += async (_, _) => await PrepareUpdateStateTestAsync();
        content.AddArrangedSubview(_prepareUpdateTestButton);

        _clearDownloadCacheButton = SystemButton("Clear Download Cache Only (Keep Managed Install)", 15);
        _clearDownloadCacheButton.TouchUpInside += async (_, _) => await ClearDownloadCacheAsync();
        content.AddArrangedSubview(_clearDownloadCacheButton);

        _prepareFreshDownloadTestButton = SystemButton("Prepare Fresh Download Test (Force Update + Clear Cache)", 15);
        _prepareFreshDownloadTestButton.TouchUpInside += async (_, _) => await PrepareFreshDownloadTestAsync();
        content.AddArrangedSubview(_prepareFreshDownloadTestButton);

        _managedInstallResultLabel = Label(
            "INSTALL MANAGER: NOT RUN",
            UIFont.BoldSystemFontOfSize(21),
            UIColor.Label);
        content.AddArrangedSubview(_managedInstallResultLabel);

        _managedInstallDetailLabel = Label(
            "The existing Install / Repair / Update regression helpers remain unchanged. Clear Download Cache deletes only Step11-ResumableDepot (complete + resume cache) and leaves the managed Step 12 install plus Keychain session untouched. Prepare Fresh Download Test first makes the project-owned install receipt synthetic-update stale, then deletes the Step 11 cache. The next manager run must therefore acquire the real current depot from Steam again, verify it, exercise Update, and atomically return to UpToDate.",
            UIFont.SystemFontOfSize(15),
            UIColor.SecondaryLabel);
        content.AddArrangedSubview(_managedInstallDetailLabel);

        content.AddArrangedSubview(Separator());

        content.AddArrangedSubview(Label(
            "Step 13 — offline launcher state",
            UIFont.BoldSystemFontOfSize(25),
            UIColor.Label));

        _offlineInstallButton = SystemButton("Verify Offline-Ready Install (Local Only)", 17);
        _offlineInstallButton.TouchUpInside += async (_, _) => await RunOfflineInstallInspectionAsync();
        content.AddArrangedSubview(_offlineInstallButton);

        _offlineInstallResultLabel = Label(
            "OFFLINE STATE: NOT CHECKED",
            UIFont.BoldSystemFontOfSize(21),
            UIColor.Label);
        content.AddArrangedSubview(_offlineInstallResultLabel);

        _offlineInstallDetailLabel = Label(
            "Physical Step 13 gate: keep the already-proven Step 12 managed install, enable Airplane Mode and disable Wi-Fi, force-quit/relaunch, let the saved-session attempt finish or time out without clearing the token, then run this local-only check. It must hash-verify the exact managed tree and report OFFLINE READY PASS without consulting Steam/session state. Manifest freshness is intentionally UNKNOWN offline, and Play remains unimplemented.",
            UIFont.SystemFontOfSize(15),
            UIColor.SecondaryLabel);
        content.AddArrangedSubview(_offlineInstallDetailLabel);

        content.AddArrangedSubview(Separator());

        content.AddArrangedSubview(Label(
            "Step 14 — read-only compatibility inventory",
            UIFont.BoldSystemFontOfSize(25),
            UIColor.Label));

        _compatibilityInventoryButton = SystemButton("Inventory Installed Game Compatibility (Read Only)", 17);
        _compatibilityInventoryButton.TouchUpInside += async (_, _) => await RunCompatibilityInventoryAsync();
        content.AddArrangedSubview(_compatibilityInventoryButton);

        _compatibilityInventoryResultLabel = Label(
            "COMPATIBILITY INVENTORY: NOT RUN",
            UIFont.BoldSystemFontOfSize(21),
            UIColor.Label);
        content.AddArrangedSubview(_compatibilityInventoryResultLabel);

        _compatibilityInventoryDetailLabel = Label(
            "Step 14 first re-proves the Step 13 OfflineReady local tree, then classifies the receipt-backed installed files and scans only managed-binary metadata strings for compatibility indicators. Evidence is heuristic: a marker means a later boundary needs targeted inspection, not that the marked API is definitely executed. No game assembly is loaded, no managed/native game code is executed, and no file inside the managed install is changed.",
            UIFont.SystemFontOfSize(15),
            UIColor.SecondaryLabel);
        content.AddArrangedSubview(_compatibilityInventoryDetailLabel);

        content.AddArrangedSubview(Separator());

        content.AddArrangedSubview(Label(
            "Step 15 — Godot Foundation (ordered gates A–D)",
            UIFont.BoldSystemFontOfSize(25),
            UIColor.Label));

        _godotFoundationStartButton = SystemButton("Run Gates A–C — Native → Engine Init → Metal Render", 17);
        _godotFoundationStartButton.TouchUpInside += async (_, _) => await RunGodotFoundationGatesABCAsync();
        content.AddArrangedSubview(_godotFoundationStartButton);

        _godotFoundationResultLabel = Label(
            "GODOT FOUNDATION: NOT RUN",
            UIFont.BoldSystemFontOfSize(21),
            UIColor.Label);
        content.AddArrangedSubview(_godotFoundationResultLabel);

        _godotFoundationDetailLabel = Label(
            "Ordered gate policy: A must pass before B, B before C, and C before D. Gates A–C start the embedded Godot 4.5.1 engine and a project-owned smoke scene. If C passes, tap the visible Godot panel, send the app to the background once, return, then verify Gate D. Once Godot has started, relaunch the launcher before running unrelated Steam/foundation regressions. Step 15 does not load or execute StS2 content.",
            UIFont.SystemFontOfSize(15),
            UIColor.SecondaryLabel);
        content.AddArrangedSubview(_godotFoundationDetailLabel);

        _godotHostContainer = new UIView
        {
            TranslatesAutoresizingMaskIntoConstraints = false,
            BackgroundColor = UIColor.Black,
            ClipsToBounds = true,
            Hidden = true,
        };
        _godotHostContainer.Layer.CornerRadius = 12;
        _godotHostContainer.Layer.BorderWidth = 1;
        _godotHostContainer.Layer.BorderColor = UIColor.Separator.CGColor;
        _godotHostContainer.HeightAnchor.ConstraintEqualTo(360).Active = true;
        content.AddArrangedSubview(_godotHostContainer);

        _godotFoundationGateDButton = SystemButton("Verify Gate D — Touch + Background / Foreground", 17);
        _godotFoundationGateDButton.Enabled = false;
        _godotFoundationGateDButton.TouchUpInside += (_, _) => VerifyGodotFoundationGateD();
        content.AddArrangedSubview(_godotFoundationGateDButton);

        content.AddArrangedSubview(Separator());

        content.AddArrangedSubview(Label(
            "Step 16 — Managed Preparation Foundation (ordered gates A–D)",
            UIFont.BoldSystemFontOfSize(25),
            UIColor.Label));

        _managedPreparationButton = SystemButton("Run Gates A–D — Cecil Fixture → IL Rewrite → Real StS2 Metadata", 17);
        _managedPreparationButton.TouchUpInside += async (_, _) => await RunManagedPreparationFoundationAsync();
        content.AddArrangedSubview(_managedPreparationButton);

        _managedPreparationResultLabel = Label(
            "MANAGED PREPARATION: NOT RUN",
            UIFont.BoldSystemFontOfSize(21),
            UIColor.Label);
        content.AddArrangedSubview(_managedPreparationResultLabel);

        _managedPreparationDetailLabel = Label(
            "Ordered gate policy: A reads the bundled project-owned fixture with Mono.Cecil; B writes/reopens only a launcher-private fixture copy; C rewrites RewriteMe() from IL constant 7 to 42 and verifies the rewritten fixture after reopen; D first re-proves the Step 13 OfflineReady tree, then parses the real installed .dll/.exe metadata with Cecil one file at a time. Gate D never resolves/loads game assemblies into the CLR and never writes inside the managed install.",
            UIFont.SystemFontOfSize(15),
            UIColor.SecondaryLabel);
        content.AddArrangedSubview(_managedPreparationDetailLabel);

        content.AddArrangedSubview(Separator());

        content.AddArrangedSubview(Label(
            "Step 17 — Compatibility Call-Site Analysis (ordered gates A–D)",
            UIFont.BoldSystemFontOfSize(25),
            UIColor.Label));

        _compatibilityCallSiteButton = SystemButton("Run Gates A–D — ARM64 Scope → Actual IL Calls → Native/Platform → Dependency Map", 17);
        _compatibilityCallSiteButton.TouchUpInside += async (_, _) => await RunCompatibilityCallSiteAnalysisAsync();
        content.AddArrangedSubview(_compatibilityCallSiteButton);

        _compatibilityCallSiteResultLabel = Label(
            "COMPATIBILITY CALL-SITE ANALYSIS: NOT RUN",
            UIFont.BoldSystemFontOfSize(21),
            UIColor.Label);
        content.AddArrangedSubview(_compatibilityCallSiteResultLabel);

        _compatibilityCallSiteDetailLabel = Label(
            "Gate A re-proves OfflineReady and selects the macOS arm64 + architecture-neutral managed scope while excluding x86_64 duplicates. Gate B reads concrete IL method-reference instructions and records dynamic/AOT-sensitive call sites. Gate C classifies P/Invoke/native modules and platform-sensitive managed APIs. Gate D builds a direct dependency-pressure map for the primary arm64 sts2.dll and re-hashes every scanned candidate. No dependency Resolve(), Assembly.Load, game execution, or game-file writes are allowed.",
            UIFont.SystemFontOfSize(15),
            UIColor.SecondaryLabel);
        content.AddArrangedSubview(_compatibilityCallSiteDetailLabel);

        content.AddArrangedSubview(Separator());

        content.AddArrangedSubview(Label(
            "Step 18 — Real Assembly Rewrite Workspace (ordered gates A–D)",
            UIFont.BoldSystemFontOfSize(25),
            UIColor.Label));

        _realAssemblyRewriteButton = SystemButton("Run Gates A–D — Clone ARM64 → Real Roundtrip → Neutral NOP → Isolation Audit", 17);
        _realAssemblyRewriteButton.TouchUpInside += async (_, _) => await RunRealAssemblyRewriteWorkspaceAsync();
        content.AddArrangedSubview(_realAssemblyRewriteButton);

        _realAssemblyRewriteResultLabel = Label(
            "REAL ASSEMBLY REWRITE WORKSPACE: NOT RUN",
            UIFont.BoldSystemFontOfSize(21),
            UIColor.Label);
        content.AddArrangedSubview(_realAssemblyRewriteResultLabel);

        _realAssemblyRewriteDetailLabel = Label(
            "Step 18 is now a closed regression boundary: Gate A clones the receipt-backed ARM64/shared workspace, Gate B round-trips real copied sts2.dll, Gate C proves a neutral Cecil IL write, and Gate D proves complete source/install isolation. Re-run it only when specifically diagnosing a regression.",
            UIFont.SystemFontOfSize(15),
            UIColor.SecondaryLabel);
        content.AddArrangedSubview(_realAssemblyRewriteDetailLabel);

        content.AddArrangedSubview(Separator());

        content.AddArrangedSubview(Label(
            "Step 19.2 — Expression Interpreter Compatibility (ordered gates A–D)",
            UIFont.BoldSystemFontOfSize(25),
            UIColor.Label));

        _expressionInterpreterCompatibilityButton = SystemButton("Run Gates A–D — Host Fallback → Framework Boundary → Zero-Write Prep → Isolation Audit", 17);
        _expressionInterpreterCompatibilityButton.TouchUpInside += async (_, _) => await RunExpressionInterpreterCompatibilityAsync();
        content.AddArrangedSubview(_expressionInterpreterCompatibilityButton);

        _expressionInterpreterCompatibilityResultLabel = Label(
            "EXPRESSION INTERPRETER COMPATIBILITY: NOT RUN",
            UIFont.BoldSystemFontOfSize(21),
            UIColor.Label);
        content.AddArrangedSubview(_expressionInterpreterCompatibilityResultLabel);

        _expressionInterpreterCompatibilityDetailLabel = Label(
            "Gate A proves three host-runtime paths — Compile(), Compile(preferInterpretation: false), and Compile(preferInterpretation: true) — all execute correctly, records RuntimeFeature dynamic-code flags and the host System.Linq.Expressions identity, then re-proves OfflineReady and clones a fresh receipt-backed ARM64/shared workspace. Gate B read-only scans real direct LambdaExpression/Expression<T>.Compile sites and classifies caller ownership plus IL-only versus ReadyToRun/mixed-mode image shape; no assembly is selected for mutation. Gate C performs zero Cecil assembly writes and copies the complete prepared tree byte-for-byte. Gate D independently re-hashes source, prepared, and live-install trees and requires every prepared file to remain receipt-identical. Copied desktop System.* framework images are diagnostic only; the iOS host runtime is the expression compatibility provider.",
            UIFont.SystemFontOfSize(15),
            UIColor.SecondaryLabel);
        content.AddArrangedSubview(_expressionInterpreterCompatibilityDetailLabel);

        content.AddArrangedSubview(Separator());

        content.AddArrangedSubview(Label(
            "Step 20 — Dynamic Managed Execution Foundation (ordered gates A–D)",
            UIFont.BoldSystemFontOfSize(25),
            UIColor.Label));

        _dynamicManagedExecutionButton = SystemButton("Run Gates A–D — Fixture Integrity → External IL Execute → Private Dependency → Isolation Audit", 17);
        _dynamicManagedExecutionButton.TouchUpInside += async (_, _) => await RunDynamicManagedExecutionFoundationAsync();
        content.AddArrangedSubview(_dynamicManagedExecutionButton);

        _dynamicManagedExecutionResultLabel = Label(
            "DYNAMIC MANAGED EXECUTION FOUNDATION: NOT RUN",
            UIFont.BoldSystemFontOfSize(21),
            UIColor.Label);
        content.AddArrangedSubview(_dynamicManagedExecutionResultLabel);

        _dynamicManagedExecutionDetailLabel = Label(
            "Gate A re-proves OfflineReady, validates the bundled Step 20 fixture manifest, SHA-256 verifies all three project-owned fixture DLLs, probes their Cecil identities, and copies them into launcher-private storage. Gate B uses a new AssemblyLoadContext to load the basic fixture from verified bytes and requires its non-AOT IL to execute to result 42. Gate C loads a second fixture whose exact dependency is resolved only from the verified private fixture directory and also must execute to 42. Gate D re-hashes every fixture, re-proves OfflineReady, and asserts no sts2 assembly entered the CLR. Step 20 deliberately permits AssemblyLoadContext only for project-owned fixtures; real StS2 CLR loading remains out of scope.",
            UIFont.SystemFontOfSize(15),
            UIColor.SecondaryLabel);
        content.AddArrangedSubview(_dynamicManagedExecutionDetailLabel);

        content.AddArrangedSubview(Separator());

        content.AddArrangedSubview(Label(
            "Step 21 — Prepared Runtime / Framework Binding (ordered gates A–D)",
            UIFont.BoldSystemFontOfSize(25),
            UIColor.Label));

        _runtimeFrameworkBindingButton = SystemButton("Run Gates A–D — Classify Runtime → Bind Host Frameworks → Prepare IL Set → Closure Audit", 17);
        _runtimeFrameworkBindingButton.TouchUpInside += async (_, _) => await RunPreparedRuntimeFrameworkBindingAsync();
        content.AddArrangedSubview(_runtimeFrameworkBindingButton);

        _runtimeFrameworkBindingResultLabel = Label(
            "PREPARED RUNTIME / FRAMEWORK BINDING: NOT RUN",
            UIFont.BoldSystemFontOfSize(21),
            UIColor.Label);
        content.AddArrangedSubview(_runtimeFrameworkBindingResultLabel);

        _runtimeFrameworkBindingDetailLabel = Label(
            "Gate A re-proves OfflineReady, clones and SHA-1 verifies the real ARM64/shared managed filename scope, and catalogs actual managed identities plus IL-only versus ReadyToRun/mixed-mode shape without Cecil dependency resolution. Gate B starts at the real ARM64 sts2.dll and classifies every reachable AssemblyRef as an iOS-host framework binding, an exact/controlled-version verified private workspace binding, or an explicit blocker; copied desktop System.* implementations are never preferred when the host can satisfy the contract. Gate C performs zero Cecil writes and byte-copies only reachable IL-only private/game assemblies into Step21-PreparedRuntimeBinding/prepared, then writes runtime-binding-plan.json. Gate D independently audits source/prepared/live hashes, plan integrity, host/private simple-name isolation, and confirms no real StS2 assembly entered the CLR. Step 21 can pass 4/4 with Runtime closure ready: NO; that means the plan is authoritative and Step 22 must solve the recorded blockers before any game CLR load.",
            UIFont.SystemFontOfSize(15),
            UIColor.SecondaryLabel);
        content.AddArrangedSubview(_runtimeFrameworkBindingDetailLabel);

        _runtimeBindingDiagnosticsExportButton = SystemButton("Export Complete Step 21 Binding Diagnostics to Files", 17);
        _runtimeBindingDiagnosticsExportButton.TouchUpInside += async (_, _) => await RunRuntimeBindingDiagnosticsExportAsync();
        content.AddArrangedSubview(_runtimeBindingDiagnosticsExportButton);

        _runtimeBindingDiagnosticsExportResultLabel = Label(
            "DIAGNOSTIC EXPORT: NOT RUN — existing Step 21 plan may be exported immediately",
            UIFont.BoldSystemFontOfSize(17),
            UIColor.SecondaryLabel);
        content.AddArrangedSubview(_runtimeBindingDiagnosticsExportResultLabel);

        content.AddArrangedSubview(Label(
            "Files location after export: On My iPhone → StS2 Launcher → StS2Launcher → Step21.1-RuntimeBindingDiagnostics.txt. The report contains the complete blocker list, grouped blocker counts, unique requested identities, host bindings, prepared assembly identities, and the persisted plan SHA-256. It intentionally omits Steam credentials/tokens and host absolute file locations. The exported text is diagnostic output only and is never trusted as launcher input. Because iOS exposes the app Documents directory for this hotfix, avoid editing or deleting other StS2Launcher files in Files.",
            UIFont.SystemFontOfSize(14),
            UIColor.SecondaryLabel));

        _signOutButton = SystemButton("Sign Out / Clear Saved Session", 16);
        _signOutButton.TouchUpInside += (_, _) => ClearSavedSession();
        content.AddArrangedSubview(_signOutButton);

        _cancelOperationButton = SystemButton("Cancel Current Operation", 15);
        _cancelOperationButton.Enabled = false;
        _cancelOperationButton.TouchUpInside += (_, _) => _operationCts?.Cancel();
        content.AddArrangedSubview(_cancelOperationButton);

        content.AddArrangedSubview(Separator());

        _statusLabel = Label(
            "Status: Step 21 A–D physically passed with 47 explicit binding blockers and Runtime closure ready: NO. Step 21.1 preserves that binding logic and exports the complete persisted plan to a Files-accessible text report so the blocker frontier can be analyzed before Step 22. No real StS2 CLR load should be attempted yet.",
            UIFont.SystemFontOfSize(14),
            UIColor.Label);
        content.AddArrangedSubview(_statusLabel);

        _lifecycleLabel = Label(
            "Lifecycle: Starting",
            UIFont.SystemFontOfSize(13),
            UIColor.SecondaryLabel);
        content.AddArrangedSubview(_lifecycleLabel);

        foreach (var control in content.ArrangedSubviews)
        {
            if (control is UIButton or UITextField)
                control.HeightAnchor.ConstraintGreaterThanOrEqualTo(44).Active = true;
        }

        _uiStartupPassed = true;
        RefreshSavedSessionStatus();
        Console.WriteLine("Step 21: RootViewController.ViewDidLoad complete");
    }

    public void SetLifecycleState(string state)
    {
        _lifecycleActive = string.Equals(state, "Active", StringComparison.Ordinal);

        if (_lifecycleLabel is not null)
            _lifecycleLabel.Text = $"Lifecycle: {state}";

        if (_lifecycleActive && !_automaticRestoreStarted)
        {
            _automaticRestoreStarted = true;
            _ = RunAutomaticSessionRestoreAsync();
        }
    }

    private async Task RunAuthenticationAsync()
    {
        if (_authResultLabel is null ||
            _authDetailLabel is null ||
            _statusLabel is null ||
            _usernameField is null ||
            _passwordField is null)
        {
            return;
        }

        var username = _usernameField.Text?.Trim() ?? string.Empty;
        var password = _passwordField.Text ?? string.Empty;

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password))
        {
            _authResultLabel.Text = "STEAM AUTH: INPUT REQUIRED";
            _authResultLabel.TextColor = UIColor.SystemRed;
            _authDetailLabel.Text = "Enter both the Steam account name and password.";
            return;
        }

        // Password remains runtime-only and is removed from the visible UIKit
        // control immediately when the attempt starts.
        _passwordField.Text = string.Empty;
        BeginSteamOperation();
        _authResultLabel.Text = "STEAM AUTH: RUNNING…";
        _authResultLabel.TextColor = UIColor.Label;
        _authDetailLabel.Text = "Starting persistent Steam auth. If mobile Steam Guard appears, approve it and return here…";
        _statusLabel.Text = "STEP 06.3.1 AUTH RUNNING — requesting a persistent session and saving only after Steam logon succeeds.";

        try
        {
            var progress = new Progress<SteamAuthenticationProgress>(update =>
            {
                InvokeOnMainThread(() =>
                {
                    _authDetailLabel.Text = update.Message;
                    _statusLabel.Text = update.Stage switch
                    {
                        SteamAuthenticationStage.WaitingForMobileApproval =>
                            "WAITING FOR STEAM GUARD — approve the sign-in in Steam, then return here.",
                        SteamAuthenticationStage.MobileApprovalAccepted =>
                            "STEAM GUARD APPROVED — completing logon and Keychain persistence…",
                        _ => $"Steam auth: {update.Message}",
                    };
                });
            });

            var result = await _authenticationAttempt.RunAsync(
                username,
                password,
                TimeSpan.FromMinutes(3),
                _operationCts!.Token,
                progress);

            InvokeOnMainThread(() =>
            {
                _authResultLabel.Text = result.Summary;
                _authResultLabel.TextColor = result.Outcome switch
                {
                    SteamAuthenticationOutcome.Authenticated => UIColor.Label,
                    SteamAuthenticationOutcome.GuardRequired => UIColor.SystemOrange,
                    SteamAuthenticationOutcome.Cancelled => UIColor.SecondaryLabel,
                    SteamAuthenticationOutcome.TimedOut => UIColor.SystemOrange,
                    _ => UIColor.SystemRed,
                };

                _authDetailLabel.Text = FormatAuthDetail(result);
                _statusLabel.Text = result.Outcome switch
                {
                    SteamAuthenticationOutcome.Authenticated when result.SessionPersisted =>
                        "PASS: persistent Steam session saved to the iOS Keychain. On the next launch, the saved-session regression will attempt it automatically.",
                    SteamAuthenticationOutcome.GuardRequired =>
                        "BOUNDARY: Steam requested a code-based Guard method; manual code entry remains out of scope.",
                    SteamAuthenticationOutcome.TimedOut =>
                        "TIMEOUT: authentication did not complete; no new session was saved.",
                    SteamAuthenticationOutcome.Cancelled =>
                        "Authentication cancelled; no new session was saved.",
                    _ =>
                        "FAIL: credential authentication or Keychain persistence failed.",
                };

                RefreshSavedSessionStatus();
            });
        }
        catch (Exception ex)
        {
            InvokeOnMainThread(() =>
            {
                _authResultLabel.Text = "STEAM AUTH: EXCEPTION";
                _authResultLabel.TextColor = UIColor.SystemRed;
                _authDetailLabel.Text = $"{ex.GetType().Name}: {ex.Message}";
                _statusLabel.Text = "FAIL: unhandled exception during authentication/persistence.";
                RefreshSavedSessionStatus();
            });
        }
        finally
        {
            InvokeOnMainThread(EndSteamOperation);
        }
    }

    private async Task RunAutomaticSessionRestoreAsync()
    {
        if (_autoRestoreResultLabel is null ||
            _autoRestoreDetailLabel is null ||
            _statusLabel is null)
        {
            return;
        }

        BeginSteamOperation();
        _autoRestoreResultLabel.Text = "AUTO SESSION: RUNNING…";
        _autoRestoreResultLabel.TextColor = UIColor.Label;
        _autoRestoreDetailLabel.Text =
            "Active lifecycle reached. Reading the saved Keychain session and attempting password-free Steam logon…";
        _statusLabel.Text = "STEP 06.3.1 AUTO-RESTORE RUNNING — persistent token + fresh LoginID; no password or new Guard flow.";

        try
        {
            var result = await _resumeAttempt.RunAsync(
                TimeSpan.FromSeconds(45),
                _operationCts!.Token);

            var recoveryAction = SteamSessionRecoveryPolicy.Evaluate(result);
            var sessionCleared = false;
            string? recoveryError = null;

            if (recoveryAction == SteamSessionRecoveryAction.ClearSavedSessionAndRequireInteractiveAuthentication)
            {
                try
                {
                    _sessionStore.Clear();
                    sessionCleared = _sessionStore.Load() is null;
                    if (!sessionCleared)
                        recoveryError = "Keychain clear returned but the saved session is still present.";
                }
                catch (Exception ex)
                {
                    recoveryError = $"Keychain recovery clear failed: {ex.GetType().Name}: {ex.Message}";
                }
            }

            InvokeOnMainThread(() =>
            {
                _autoRestoreResultLabel.Text = result.Outcome switch
                {
                    SteamSessionResumeOutcome.Authenticated => "AUTO SESSION PASS — authenticated",
                    SteamSessionResumeOutcome.NoSavedSession => "AUTO SESSION — signed out",
                    SteamSessionResumeOutcome.Rejected when sessionCleared => "AUTO SESSION RESET — rejected token cleared",
                    SteamSessionResumeOutcome.InvalidLocalSession when sessionCleared => "AUTO SESSION RESET — invalid record cleared",
                    SteamSessionResumeOutcome.IdentityMismatch when sessionCleared => "AUTO SESSION RESET — identity mismatch cleared",
                    SteamSessionResumeOutcome.TimedOut => "AUTO SESSION — timeout; saved session preserved",
                    SteamSessionResumeOutcome.Cancelled => "AUTO SESSION — cancelled; saved session preserved",
                    SteamSessionResumeOutcome.Rejected => "AUTO SESSION — rejected; saved session preserved",
                    _ => "AUTO SESSION FAIL — saved session preserved",
                };

                _autoRestoreResultLabel.TextColor = result.Outcome switch
                {
                    SteamSessionResumeOutcome.Authenticated => UIColor.Label,
                    SteamSessionResumeOutcome.NoSavedSession => UIColor.SecondaryLabel,
                    SteamSessionResumeOutcome.TimedOut => UIColor.SystemOrange,
                    SteamSessionResumeOutcome.Cancelled => UIColor.SecondaryLabel,
                    _ when sessionCleared && recoveryError is null => UIColor.SystemOrange,
                    _ => UIColor.SystemRed,
                };

                var details = new List<string>
                {
                    FormatResumeDetail(result),
                    $"Automatic launch restore: YES",
                    $"Recovery policy: {recoveryAction}",
                    $"Saved session cleared by recovery: {YesNo(sessionCleared)}",
                };

                if (!string.IsNullOrWhiteSpace(recoveryError))
                    details.Add($"Recovery error: {recoveryError}");

                _autoRestoreDetailLabel.Text = string.Join("\n", details);

                if (!string.IsNullOrWhiteSpace(result.AccountName) && _usernameField is not null)
                    _usernameField.Text = result.AccountName;

                _statusLabel.Text = result.Outcome switch
                {
                    SteamSessionResumeOutcome.Authenticated =>
                        "PASS: saved Steam session restored automatically on launch with matching identity and no password/Guard prompt.",
                    SteamSessionResumeOutcome.NoSavedSession =>
                        "SIGNED OUT: no saved Steam session exists. Use Authenticate + Save Session when needed.",
                    _ when recoveryError is not null =>
                        $"FAIL: recovery policy selected clear, but Keychain cleanup failed: {recoveryError}",
                    _ when sessionCleared =>
                        "RECOVERED: unusable/unsafe saved session was removed. Interactive Steam authentication is required again.",
                    SteamSessionResumeOutcome.TimedOut =>
                        "TRANSIENT: automatic resume timed out; saved session was preserved for retry.",
                    SteamSessionResumeOutcome.Cancelled =>
                        "Automatic resume cancelled; saved session was preserved.",
                    SteamSessionResumeOutcome.Rejected =>
                        "Steam rejected this resume with a non-definitive result; saved session was preserved for retry rather than destroyed.",
                    _ =>
                        "Automatic resume failed without evidence that the credential is invalid; saved session was preserved.",
                };

                _statusLabel.TextColor = result.Outcome == SteamSessionResumeOutcome.Authenticated ||
                                         result.Outcome == SteamSessionResumeOutcome.NoSavedSession ||
                                         sessionCleared
                    ? UIColor.Label
                    : result.Outcome is SteamSessionResumeOutcome.TimedOut or SteamSessionResumeOutcome.Cancelled
                        ? UIColor.SystemOrange
                        : UIColor.SystemRed;

                RefreshSavedSessionStatus();
            });
        }
        catch (Exception ex)
        {
            InvokeOnMainThread(() =>
            {
                _autoRestoreResultLabel.Text = "AUTO SESSION: EXCEPTION";
                _autoRestoreResultLabel.TextColor = UIColor.SystemRed;
                _autoRestoreDetailLabel.Text =
                    $"{ex.GetType().Name}: {ex.Message}\nSaved session was not cleared because no definitive invalid-session result was obtained.";
                _statusLabel.Text = "FAIL: unhandled exception during automatic saved-session restore regression.";
                _statusLabel.TextColor = UIColor.SystemRed;
                RefreshSavedSessionStatus();
            });
        }
        finally
        {
            InvokeOnMainThread(EndSteamOperation);
        }
    }

    private async Task RunSavedSessionResumeAsync()
    {
        if (_resumeResultLabel is null || _resumeDetailLabel is null || _statusLabel is null)
            return;

        BeginSteamOperation();
        _resumeResultLabel.Text = "SAVED SESSION: RUNNING…";
        _resumeResultLabel.TextColor = UIColor.Label;
        _resumeDetailLabel.Text = "Reading the device-bound Keychain entry and logging on with the saved refresh token. No password or Guard code is requested by the launcher.";
        _statusLabel.Text = "STEP 06.3.1 MANUAL RESUME RUNNING — persistent token + fresh LoginID; no password.";

        try
        {
            var result = await _resumeAttempt.RunAsync(
                TimeSpan.FromSeconds(45),
                _operationCts!.Token);

            InvokeOnMainThread(() =>
            {
                _resumeResultLabel.Text = result.Summary;
                _resumeResultLabel.TextColor = result.Outcome switch
                {
                    SteamSessionResumeOutcome.Authenticated => UIColor.Label,
                    SteamSessionResumeOutcome.NoSavedSession => UIColor.SystemOrange,
                    SteamSessionResumeOutcome.Cancelled => UIColor.SecondaryLabel,
                    SteamSessionResumeOutcome.TimedOut => UIColor.SystemOrange,
                    _ => UIColor.SystemRed,
                };
                _resumeDetailLabel.Text = FormatResumeDetail(result);
                _statusLabel.Text = result.Outcome switch
                {
                    SteamSessionResumeOutcome.Authenticated =>
                        "PASS: saved Keychain session authenticated with matching Steam identity and no password/Guard prompt.",
                    SteamSessionResumeOutcome.NoSavedSession =>
                        "No saved Steam session exists. Authenticate + Save Session first.",
                    SteamSessionResumeOutcome.Rejected =>
                        "Saved token was rejected by Steam. AccessDenied and other non-definitive results are preserved; only explicit expired/revoked/invalid credentials are cleared.",
                    SteamSessionResumeOutcome.InvalidLocalSession =>
                        "Saved Keychain record is invalid. Automatic recovery would clear it and require interactive authentication.",
                    SteamSessionResumeOutcome.IdentityMismatch =>
                        "SECURITY: saved session authenticated as a different SteamID. Automatic recovery clears it and requires interactive authentication.",
                    SteamSessionResumeOutcome.TimedOut =>
                        "Saved-session resume timed out.",
                    SteamSessionResumeOutcome.Cancelled =>
                        "Saved-session resume cancelled.",
                    _ =>
                        "FAIL: saved-session resume failed.",
                };
                RefreshSavedSessionStatus();
            });
        }
        catch (Exception ex)
        {
            InvokeOnMainThread(() =>
            {
                _resumeResultLabel.Text = "SAVED SESSION: EXCEPTION";
                _resumeResultLabel.TextColor = UIColor.SystemRed;
                _resumeDetailLabel.Text = $"{ex.GetType().Name}: {ex.Message}";
                _statusLabel.Text = "FAIL: unhandled exception during saved-session resume.";
                RefreshSavedSessionStatus();
            });
        }
        finally
        {
            InvokeOnMainThread(EndSteamOperation);
        }
    }

    private async Task RunOwnershipVerificationAsync()
    {
        if (_ownershipResultLabel is null || _ownershipDetailLabel is null || _statusLabel is null)
            return;

        BeginSteamOperation();
        _ownershipResultLabel.Text = "OWNERSHIP: RUNNING…";
        _ownershipResultLabel.TextColor = UIColor.Label;
        _ownershipDetailLabel.Text =
            "Authenticating with the saved Keychain refresh token, verifying the stored SteamID, then requesting one ownership ticket for App ID 2868840…";
        _statusLabel.Text = "STEP 07 RUNNING — ownership ticket only; no PICS/depot/manifest/CDN/download request.";
        _statusLabel.TextColor = UIColor.Label;

        try
        {
            var result = await _ownershipAttempt.RunAsync(
                TimeSpan.FromSeconds(45),
                _operationCts!.Token);

            InvokeOnMainThread(() =>
            {
                _ownershipResultLabel.Text = result.Summary;
                _ownershipResultLabel.TextColor = result.Outcome switch
                {
                    SteamOwnershipVerificationOutcome.Owned => UIColor.Label,
                    SteamOwnershipVerificationOutcome.NoSavedSession => UIColor.SystemOrange,
                    SteamOwnershipVerificationOutcome.Cancelled => UIColor.SecondaryLabel,
                    SteamOwnershipVerificationOutcome.TimedOut => UIColor.SystemOrange,
                    _ => UIColor.SystemRed,
                };

                _ownershipDetailLabel.Text = FormatOwnershipDetail(result);
                _statusLabel.Text = result.Outcome switch
                {
                    SteamOwnershipVerificationOutcome.Owned =>
                        "PASS: Steam issued a non-empty ownership ticket for Slay the Spire 2 App ID 2868840. No content/download request was made.",
                    SteamOwnershipVerificationOutcome.NoSavedSession =>
                        "No saved Steam session exists. Authenticate + Save Session first, then retry Step 07.",
                    SteamOwnershipVerificationOutcome.SessionRejected =>
                        "Saved Steam session was rejected. Reauthenticate if needed; Step 07 did not request content.",
                    SteamOwnershipVerificationOutcome.IdentityMismatch =>
                        "SECURITY: saved session returned a different SteamID. Do not trust this ownership result.",
                    SteamOwnershipVerificationOutcome.TicketRejected =>
                        "Steam did not issue an OK ownership ticket. Ownership is not proven; no download was attempted.",
                    SteamOwnershipVerificationOutcome.EmptyTicket =>
                        "Steam returned OK but no ticket bytes. Ownership is not proven; stop before content work.",
                    SteamOwnershipVerificationOutcome.UnexpectedAppId =>
                        "Steam returned an ownership callback for an unexpected AppID. Ownership is not proven.",
                    SteamOwnershipVerificationOutcome.TimedOut =>
                        "Ownership verification timed out. No download was attempted.",
                    SteamOwnershipVerificationOutcome.Cancelled =>
                        "Ownership verification cancelled. No download was attempted.",
                    _ =>
                        "FAIL: Step 07 ownership verification did not complete. No content request was made.",
                };
                _statusLabel.TextColor = result.Outcome == SteamOwnershipVerificationOutcome.Owned
                    ? UIColor.Label
                    : result.Outcome is SteamOwnershipVerificationOutcome.TimedOut or SteamOwnershipVerificationOutcome.Cancelled or SteamOwnershipVerificationOutcome.NoSavedSession
                        ? UIColor.SystemOrange
                        : UIColor.SystemRed;
            });
        }
        catch (Exception ex)
        {
            InvokeOnMainThread(() =>
            {
                _ownershipResultLabel.Text = "OWNERSHIP: EXCEPTION";
                _ownershipResultLabel.TextColor = UIColor.SystemRed;
                _ownershipDetailLabel.Text = $"{ex.GetType().Name}: {ex.Message}";
                _statusLabel.Text = "FAIL: unhandled exception during Step 07 ownership verification. No content request was made.";
                _statusLabel.TextColor = UIColor.SystemRed;
            });
        }
        finally
        {
            InvokeOnMainThread(EndSteamOperation);
        }
    }

    private async Task RunContentDiscoveryAsync()
    {
        if (_discoveryResultLabel is null || _discoveryDetailLabel is null || _statusLabel is null)
            return;

        BeginSteamOperation();
        _discoveryResultLabel.Text = "DISCOVERY: RUNNING…";
        _discoveryResultLabel.TextColor = UIColor.Label;
        _discoveryDetailLabel.Text =
            "Restoring the saved session, re-proving App ID 2868840 ownership, then requesting PICS access metadata + product info only…";
        _statusLabel.Text =
            "STEP 08 RUNNING — PICS metadata only; no depot key, manifest body, CDN server/token, chunk, or file request.";
        _statusLabel.TextColor = UIColor.Label;

        try
        {
            var result = await _contentDiscoveryAttempt.RunAsync(
                TimeSpan.FromSeconds(60),
                _operationCts!.Token);

            InvokeOnMainThread(() =>
            {
                _discoveryResultLabel.Text = result.Summary;
                _discoveryResultLabel.TextColor = result.Outcome switch
                {
                    SteamContentDiscoveryOutcome.Discovered => UIColor.Label,
                    SteamContentDiscoveryOutcome.NoSavedSession => UIColor.SystemOrange,
                    SteamContentDiscoveryOutcome.Cancelled => UIColor.SecondaryLabel,
                    SteamContentDiscoveryOutcome.TimedOut => UIColor.SystemOrange,
                    _ => UIColor.SystemRed,
                };
                _discoveryDetailLabel.Text = FormatContentDiscoveryDetail(result);
                _statusLabel.Text = result.Outcome switch
                {
                    SteamContentDiscoveryOutcome.Discovered =>
                        $"PASS: PICS exposed {result.DepotCount} depot(s) and {result.ManifestCount} visible branch manifest ID(s) for App ID 2868840. Metadata only; nothing was downloaded.",
                    SteamContentDiscoveryOutcome.NoSavedSession =>
                        "No saved Steam session exists. Authenticate + Save Session first, then retry Step 08.",
                    SteamContentDiscoveryOutcome.SessionRejected =>
                        "Saved Steam session was rejected before discovery. Reauthenticate; no content bytes were requested.",
                    SteamContentDiscoveryOutcome.IdentityMismatch =>
                        "SECURITY: saved session returned a different SteamID. Discovery stopped before PICS product info.",
                    SteamContentDiscoveryOutcome.OwnershipNotProven =>
                        "Step 07 ownership could not be re-proven. Discovery stopped before PICS product info.",
                    SteamContentDiscoveryOutcome.PicsAccessTokenDenied =>
                        "Steam denied the PICS app access token. No manifest body or CDN request was attempted.",
                    SteamContentDiscoveryOutcome.ProductInfoUnavailable =>
                        "PICS did not return product info for App ID 2868840.",
                    SteamContentDiscoveryOutcome.MissingPicsToken =>
                        "PICS app info says a required access token is still missing; discovery is not proven.",
                    SteamContentDiscoveryOutcome.NoDepots =>
                        "PICS app info returned, but no numeric depot entries were found.",
                    SteamContentDiscoveryOutcome.NoVisibleManifests =>
                        "Depot metadata returned, but no visible branch manifest IDs were found. No manifest body was requested.",
                    SteamContentDiscoveryOutcome.TimedOut =>
                        "Step 08 discovery timed out; no download was attempted.",
                    SteamContentDiscoveryOutcome.Cancelled =>
                        "Step 08 discovery cancelled; no download was attempted.",
                    _ =>
                        "FAIL: Step 08 depot/manifest discovery did not complete. No download was attempted.",
                };
                _statusLabel.TextColor = result.Outcome == SteamContentDiscoveryOutcome.Discovered
                    ? UIColor.Label
                    : result.Outcome is SteamContentDiscoveryOutcome.TimedOut or SteamContentDiscoveryOutcome.Cancelled or SteamContentDiscoveryOutcome.NoSavedSession
                        ? UIColor.SystemOrange
                        : UIColor.SystemRed;
            });
        }
        catch (Exception ex)
        {
            InvokeOnMainThread(() =>
            {
                _discoveryResultLabel.Text = "DISCOVERY: EXCEPTION";
                _discoveryResultLabel.TextColor = UIColor.SystemRed;
                _discoveryDetailLabel.Text = $"{ex.GetType().Name}: {ex.Message}";
                _statusLabel.Text = "FAIL: unhandled exception during Step 08 metadata discovery. No download was attempted.";
                _statusLabel.TextColor = UIColor.SystemRed;
            });
        }
        finally
        {
            InvokeOnMainThread(EndSteamOperation);
        }
    }

    private async Task RunSingleFileDownloadAsync()
    {
        if (_singleFileResultLabel is null || _singleFileDetailLabel is null || _statusLabel is null)
            return;

        BeginSteamOperation();
        _singleFileResultLabel.Text = "SINGLE-FILE: RUNNING…";
        _singleFileResultLabel.TextColor = UIColor.Label;
        _singleFileDetailLabel.Text =
            "Re-proving saved-session identity, Step 07 ownership and Step 08 PICS metadata; then downloading one direct public manifest and one <= 2 MiB file only…";
        _statusLabel.Text =
            "STEP 09 RUNNING — exactly one bounded file; no full-depot queue, resume, install, update, repair, Godot, Cloud, or Workshop operation.";
        _statusLabel.TextColor = UIColor.Label;

        try
        {
            var result = await _singleFileDownloadAttempt.RunAsync(
                TimeSpan.FromSeconds(120),
                _operationCts!.Token);

            InvokeOnMainThread(() =>
            {
                _singleFileResultLabel.Text = result.Summary;
                _singleFileResultLabel.TextColor = result.Outcome switch
                {
                    SteamSingleFileDownloadOutcome.Downloaded => UIColor.Label,
                    SteamSingleFileDownloadOutcome.NoSavedSession => UIColor.SystemOrange,
                    SteamSingleFileDownloadOutcome.Cancelled => UIColor.SecondaryLabel,
                    SteamSingleFileDownloadOutcome.TimedOut => UIColor.SystemOrange,
                    _ => UIColor.SystemRed,
                };
                _singleFileDetailLabel.Text = FormatSingleFileDownloadDetail(result);
                _statusLabel.Text = result.Outcome switch
                {
                    SteamSingleFileDownloadOutcome.Downloaded =>
                        $"PASS: one verified StS2 file was downloaded and written ({result.SelectedFileBytes} bytes). No other game file was requested.",
                    SteamSingleFileDownloadOutcome.NoSavedSession =>
                        "No saved Steam session exists. Authenticate + Save Session first, then retry Step 09.",
                    SteamSingleFileDownloadOutcome.SessionRejected =>
                        "Saved Steam session was rejected before Step 09 content access. Reauthenticate.",
                    SteamSingleFileDownloadOutcome.IdentityMismatch =>
                        "SECURITY: saved session returned a different SteamID. Step 09 stopped before content access.",
                    SteamSingleFileDownloadOutcome.OwnershipNotProven =>
                        "Step 07 ownership could not be re-proven. Step 09 stopped before depot-key/CDN access.",
                    SteamSingleFileDownloadOutcome.NoSuitableDepot =>
                        "Step 08 metadata had no direct depot with a visible public manifest suitable for this controlled test.",
                    SteamSingleFileDownloadOutcome.NoSmallFile =>
                        "The selected manifest had no safe non-empty regular file <= 2 MiB. No file was written.",
                    SteamSingleFileDownloadOutcome.FileHashMismatch =>
                        "Downloaded chunks assembled, but SHA-1 did not match Steam's manifest. Nothing was written.",
                    SteamSingleFileDownloadOutcome.TimedOut =>
                        "Step 09 timed out. No unverified file is persisted.",
                    SteamSingleFileDownloadOutcome.Cancelled =>
                        "Step 09 cancelled. No unverified file is persisted.",
                    _ =>
                        "FAIL: Step 09 did not complete. Review the detailed boundary telemetry before changing scope.",
                };
                _statusLabel.TextColor = result.Outcome == SteamSingleFileDownloadOutcome.Downloaded
                    ? UIColor.Label
                    : result.Outcome is SteamSingleFileDownloadOutcome.TimedOut or SteamSingleFileDownloadOutcome.Cancelled or SteamSingleFileDownloadOutcome.NoSavedSession
                        ? UIColor.SystemOrange
                        : UIColor.SystemRed;
            });
        }
        catch (Exception ex)
        {
            InvokeOnMainThread(() =>
            {
                _singleFileResultLabel.Text = "SINGLE-FILE: EXCEPTION";
                _singleFileResultLabel.TextColor = UIColor.SystemRed;
                _singleFileDetailLabel.Text = $"{ex.GetType().Name}: {ex.Message}";
                _statusLabel.Text = "FAIL: unhandled exception during Step 09 single-file boundary.";
                _statusLabel.TextColor = UIColor.SystemRed;
            });
        }
        finally
        {
            InvokeOnMainThread(EndSteamOperation);
        }
    }

    private async Task RunFullDepotDownloadAsync()
    {
        if (_fullDepotResultLabel is null || _fullDepotDetailLabel is null || _statusLabel is null)
            return;

        BeginSteamOperation();
        _fullDepotResultLabel.Text = "DEPOT: PREPARING…";
        _fullDepotResultLabel.TextColor = UIColor.Label;
        _fullDepotDetailLabel.Text =
            "Re-proving saved-session identity, Step 07 ownership and Step 08 discovery; then building one verified public-depot queue. Use Cancel Current Steam Operation at any time.";
        _statusLabel.Text =
            "STEP 10 RUNNING — one selected public depot only; staging data is temporary until the complete queue is verified and atomically committed.";
        _statusLabel.TextColor = UIColor.Label;

        var progress = new Progress<SteamDepotDownloadProgress>(value =>
        {
            if (_fullDepotResultLabel is null || _fullDepotDetailLabel is null)
                return;

            _fullDepotResultLabel.Text = value.Summary;
            _fullDepotResultLabel.TextColor = UIColor.Label;
            _fullDepotDetailLabel.Text =
                $"Phase: {value.Phase}\n" +
                $"Files: {value.CompletedFiles}/{value.TotalFiles}\n" +
                $"Chunks: {value.CompletedChunks}/{value.TotalChunks}\n" +
                $"Bytes: {value.CompletedBytes}/{value.TotalBytes}\n" +
                $"Current file: {value.CurrentFile ?? "none"}\n\n" +
                "Final output is not visible until the entire staged depot passes SHA-1 verification.";
        });

        try
        {
            var result = await _fullDepotDownloadAttempt.RunAsync(
                TimeSpan.FromMinutes(60),
                progress,
                _operationCts!.Token);

            InvokeOnMainThread(() =>
            {
                _fullDepotResultLabel.Text = result.Summary;
                _fullDepotResultLabel.TextColor = result.Outcome switch
                {
                    SteamFullDepotDownloadOutcome.Downloaded => UIColor.Label,
                    SteamFullDepotDownloadOutcome.NoSavedSession => UIColor.SystemOrange,
                    SteamFullDepotDownloadOutcome.Cancelled => UIColor.SecondaryLabel,
                    SteamFullDepotDownloadOutcome.TimedOut => UIColor.SystemOrange,
                    SteamFullDepotDownloadOutcome.OutputAlreadyExists => UIColor.SystemOrange,
                    _ => UIColor.SystemRed,
                };
                _fullDepotDetailLabel.Text = FormatFullDepotDownloadDetail(result);
                _statusLabel.Text = result.Outcome switch
                {
                    SteamFullDepotDownloadOutcome.Downloaded =>
                        $"PASS: selected depot {result.SelectedDepotId} manifest {result.SelectedManifestId} completed with {result.VerifiedFileCount} SHA-1-verified files and one atomic final-directory commit.",
                    SteamFullDepotDownloadOutcome.NoSavedSession =>
                        "No saved Steam session exists. Authenticate + Save Session first, then retry Step 10.",
                    SteamFullDepotDownloadOutcome.SessionRejected =>
                        "Saved Steam session was rejected before Step 10. Reauthenticate.",
                    SteamFullDepotDownloadOutcome.IdentityMismatch =>
                        "SECURITY: saved session returned a different SteamID. Step 10 stopped before depot content access.",
                    SteamFullDepotDownloadOutcome.OwnershipNotProven =>
                        "Step 07 ownership could not be re-proven. Step 10 stopped before depot-key/CDN access.",
                    SteamFullDepotDownloadOutcome.InvalidManifest =>
                        "The selected manifest contains a path/chunk/link shape Step 10 will not safely materialize. No final depot directory was committed.",
                    SteamFullDepotDownloadOutcome.OutputAlreadyExists =>
                        "This exact depot/manifest output directory already exists. Step 10 deliberately has no overwrite/update/repair behavior.",
                    SteamFullDepotDownloadOutcome.FileHashMismatch =>
                        "A staged file failed Steam manifest SHA-1 verification. The staging tree was removed and no final depot was committed.",
                    SteamFullDepotDownloadOutcome.Cancelled =>
                        "Step 10 cancelled. No partial final depot was committed; check the staging-cleanup telemetry below.",
                    SteamFullDepotDownloadOutcome.TimedOut =>
                        "Step 10 timed out. No partial final depot was committed; check the staging-cleanup telemetry below.",
                    _ =>
                        "FAIL: Step 10 did not complete. Review the detailed boundary telemetry; do not advance to resume/update yet.",
                };
                _statusLabel.TextColor = result.Outcome == SteamFullDepotDownloadOutcome.Downloaded
                    ? UIColor.Label
                    : result.Outcome is SteamFullDepotDownloadOutcome.NoSavedSession or SteamFullDepotDownloadOutcome.Cancelled or SteamFullDepotDownloadOutcome.TimedOut or SteamFullDepotDownloadOutcome.OutputAlreadyExists
                        ? UIColor.SystemOrange
                        : UIColor.SystemRed;
            });
        }
        catch (Exception ex)
        {
            InvokeOnMainThread(() =>
            {
                _fullDepotResultLabel.Text = "DEPOT: EXCEPTION";
                _fullDepotResultLabel.TextColor = UIColor.SystemRed;
                _fullDepotDetailLabel.Text = $"{ex.GetType().Name}: {ex.Message}";
                _statusLabel.Text = "FAIL: unhandled exception during Step 10 full-depot boundary.";
                _statusLabel.TextColor = UIColor.SystemRed;
            });
        }
        finally
        {
            InvokeOnMainThread(EndSteamOperation);
        }
    }

    private async Task RunResumableDepotDownloadAsync()
    {
        if (_resumableDepotResultLabel is null || _resumableDepotDetailLabel is null || _statusLabel is null)
            return;

        BeginSteamOperation();
        _resumableDepotResultLabel.Text = "RESUME: PREPARING…";
        _resumableDepotResultLabel.TextColor = UIColor.Label;
        _resumableDepotDetailLabel.Text =
            "Re-proving saved-session identity, Step 07 ownership and Step 08 discovery, then checking the deterministic Step 11 staging tree for SHA-1-valid complete files and Adler-32-valid partial chunks.";
        _statusLabel.Text =
            "STEP 11 RUNNING — if this is the first run, force-quit after chunk/byte progress becomes non-zero. Relaunch and run Step 11 again to prove resume.";
        _statusLabel.TextColor = UIColor.Label;

        var progress = new Progress<SteamDepotDownloadProgress>(value =>
        {
            if (_resumableDepotResultLabel is null || _resumableDepotDetailLabel is null)
                return;

            _resumableDepotResultLabel.Text = value.Summary;
            _resumableDepotResultLabel.TextColor = UIColor.Label;
            _resumableDepotDetailLabel.Text =
                $"Phase: {value.Phase}\n" +
                $"Files satisfied: {value.CompletedFiles}/{value.TotalFiles}\n" +
                $"Chunks satisfied: {value.CompletedChunks}/{value.TotalChunks}\n" +
                $"Bytes satisfied: {value.CompletedBytes}/{value.TotalBytes}\n" +
                $"Current file: {value.CurrentFile ?? "none"}\n\n" +
                "Step 11 preserves its deterministic staging tree across interruption. The final output remains invisible until the entire depot passes SHA-1 verification.";
        });

        try
        {
            var result = await _resumableDepotDownloadAttempt.RunAsync(
                TimeSpan.FromMinutes(60),
                progress,
                _operationCts!.Token);

            InvokeOnMainThread(() =>
            {
                _resumableDepotResultLabel.Text = result.Summary;
                _resumableDepotResultLabel.TextColor = result.Outcome switch
                {
                    SteamResumableDepotDownloadOutcome.Downloaded when result.ResumeWasUsed => UIColor.Label,
                    SteamResumableDepotDownloadOutcome.Downloaded => UIColor.SystemOrange,
                    SteamResumableDepotDownloadOutcome.NoSavedSession => UIColor.SystemOrange,
                    SteamResumableDepotDownloadOutcome.Cancelled => UIColor.SecondaryLabel,
                    SteamResumableDepotDownloadOutcome.TimedOut => UIColor.SystemOrange,
                    SteamResumableDepotDownloadOutcome.OutputAlreadyExists when result.ExistingFinalVerifiedAgainstManifest => UIColor.Label,
                    SteamResumableDepotDownloadOutcome.OutputAlreadyExists => UIColor.SystemOrange,
                    _ => UIColor.SystemRed,
                };
                _resumableDepotDetailLabel.Text = FormatResumableDepotDownloadDetail(result);
                _statusLabel.Text = result.Outcome switch
                {
                    SteamResumableDepotDownloadOutcome.Downloaded when result.ResumeWasUsed =>
                        $"PASS: Step 11 resumed depot {result.SelectedDepotId} manifest {result.SelectedManifestId}, reused {result.ReusedChunkCount} checksum-valid chunks / {result.ReusedBytes} bytes, downloaded only the remainder, re-verified every file, and atomically committed the final directory.",
                    SteamResumableDepotDownloadOutcome.Downloaded =>
                        "BASELINE ONLY: the Step 11 depot completed, but no prior resume data was reused. Interrupted-download resume is not yet proven; rerun the physical interruption test from clean Step 11 data.",
                    SteamResumableDepotDownloadOutcome.Cancelled =>
                        "Step 11 interruption/cancel preserved the deterministic staging tree. Run Step 11 again and require reused files/chunks/bytes > 0.",
                    SteamResumableDepotDownloadOutcome.TimedOut =>
                        "Step 11 timed out with resume staging preserved. Run it again and require reuse telemetry > 0.",
                    SteamResumableDepotDownloadOutcome.NoSavedSession =>
                        "No saved Steam session exists. Authenticate + Save Session first, then retry Step 11.",
                    SteamResumableDepotDownloadOutcome.SessionRejected =>
                        "Saved Steam session was rejected before Step 11. Reauthenticate.",
                    SteamResumableDepotDownloadOutcome.IdentityMismatch =>
                        "SECURITY: saved session returned a different SteamID. Step 11 stopped before depot content access.",
                    SteamResumableDepotDownloadOutcome.OwnershipNotProven =>
                        "Step 07 ownership could not be re-proven. Step 11 stopped before depot-key/CDN access.",
                    SteamResumableDepotDownloadOutcome.OutputAlreadyExists when result.ExistingFinalVerifiedAgainstManifest =>
                        "PASS: this exact Step 11 final depot already existed and was reverified file-by-file against Steam's current manifest. No overwrite/update occurred inside Step 11.",
                    SteamResumableDepotDownloadOutcome.OutputAlreadyExists =>
                        "This exact Step 11 final depot exists but failed current-manifest verification; Step 12 may discard and reacquire only that invalid cache.",
                    SteamResumableDepotDownloadOutcome.FileHashMismatch =>
                        "A reconstructed file failed Steam manifest SHA-1. That partial file was discarded; other checksum-valid resume data remains uncommitted.",
                    _ =>
                        "FAIL: Step 11 did not complete. Resume staging is preserved when safe, but do not advance to update/repair until this boundary passes.",
                };
                _statusLabel.TextColor =
                    (result.Outcome == SteamResumableDepotDownloadOutcome.Downloaded && result.ResumeWasUsed) ||
                    (result.Outcome == SteamResumableDepotDownloadOutcome.OutputAlreadyExists && result.ExistingFinalVerifiedAgainstManifest)
                        ? UIColor.Label
                        : result.Outcome is SteamResumableDepotDownloadOutcome.NoSavedSession or SteamResumableDepotDownloadOutcome.Cancelled or SteamResumableDepotDownloadOutcome.TimedOut or SteamResumableDepotDownloadOutcome.OutputAlreadyExists or SteamResumableDepotDownloadOutcome.Downloaded
                            ? UIColor.SystemOrange
                            : UIColor.SystemRed;
            });
        }
        catch (Exception ex)
        {
            InvokeOnMainThread(() =>
            {
                _resumableDepotResultLabel.Text = "RESUME: EXCEPTION";
                _resumableDepotResultLabel.TextColor = UIColor.SystemRed;
                _resumableDepotDetailLabel.Text = $"{ex.GetType().Name}: {ex.Message}";
                _statusLabel.Text = "FAIL: unhandled exception during Step 11 interrupted-download-resume boundary.";
                _statusLabel.TextColor = UIColor.SystemRed;
            });
        }
        finally
        {
            InvokeOnMainThread(EndSteamOperation);
        }
    }

    private async Task RunManagedInstallAsync()
    {
        if (_managedInstallResultLabel is null || _managedInstallDetailLabel is null || _statusLabel is null)
            return;

        BeginSteamOperation();
        _managedInstallResultLabel.Text = "INSTALL MANAGER: INSPECTING…";
        _managedInstallResultLabel.TextColor = UIColor.Label;
        _managedInstallDetailLabel.Text =
            "Discovering the current public manifest, verifying the stable managed install, then performing exactly one of: no-op, install, update, or repair. Any replacement is fully staged and verified before the prior good install is swapped out.";
        _statusLabel.Text = "STEP 12.4.1 RUNNING — completed Step 12 manager regression; current source/cache/receipt/staging/rollback safeguards remain active.";
        _statusLabel.TextColor = UIColor.Label;

        var progress = new Progress<SteamManagedInstallProgress>(value =>
        {
            if (_managedInstallResultLabel is null || _managedInstallDetailLabel is null)
                return;
            _managedInstallResultLabel.Text = $"INSTALL MANAGER: {value.Phase.ToString().ToUpperInvariant()}";
            _managedInstallResultLabel.TextColor = UIColor.Label;
            _managedInstallDetailLabel.Text =
                $"{value.Message}\n" +
                $"Files: {value.CompletedFiles}/{value.TotalFiles}\n" +
                $"Bytes: {value.CompletedBytes}/{value.TotalBytes}\n" +
                $"Current file: {value.CurrentFile ?? "none"}";
        });

        try
        {
            var result = await _managedInstallAttempt.RunAsync(
                TimeSpan.FromMinutes(90),
                progress,
                _operationCts!.Token);

            InvokeOnMainThread(() =>
            {
                _managedInstallResultLabel.Text = result.Summary;
                _managedInstallResultLabel.TextColor = result.Success
                    ? UIColor.Label
                    : result.Outcome is SteamManagedInstallOutcome.Cancelled or SteamManagedInstallOutcome.TimedOut
                        ? UIColor.SystemOrange
                        : UIColor.SystemRed;
                _managedInstallDetailLabel.Text = FormatManagedInstallDetail(result);
                _statusLabel.Text = result.Success
                    ? $"PASS: Step 12.4.1 state {result.StateBefore} -> {result.StateAfter}; action {result.ActionTaken}; stable managed install is verified and current."
                    : $"Step 12.4.1 manager regression did not complete: {result.Error ?? result.Outcome.ToString()}. The prior good install was preserved when one existed.";
                _statusLabel.TextColor = result.Success ? UIColor.Label : UIColor.SystemRed;
            });
        }
        catch (Exception ex)
        {
            InvokeOnMainThread(() =>
            {
                _managedInstallResultLabel.Text = "INSTALL MANAGER: EXCEPTION";
                _managedInstallResultLabel.TextColor = UIColor.SystemRed;
                _managedInstallDetailLabel.Text = $"{ex.GetType().Name}: {ex.Message}";
                _statusLabel.Text = "FAIL: unhandled exception during Step 12.4.1 install/update/repair manager regression.";
                _statusLabel.TextColor = UIColor.SystemRed;
            });
        }
        finally
        {
            InvokeOnMainThread(EndSteamOperation);
        }
    }

    private async Task RunOfflineInstallInspectionAsync()
    {
        if (_offlineInstallResultLabel is null || _offlineInstallDetailLabel is null || _statusLabel is null)
            return;

        BeginSteamOperation();
        _offlineInstallResultLabel.Text = "OFFLINE STATE: VERIFYING LOCAL INSTALL…";
        _offlineInstallResultLabel.TextColor = UIColor.Label;
        _offlineInstallDetailLabel.Text =
            "Local-only Step 13 verification started. Reading the Step 12 receipt and hashing managed files; no Steam session or network API is used by this check.";
        _statusLabel.Text = "STEP 13 LOCAL CHECK RUNNING — exact receipt/file verification only; online manifest freshness intentionally unknown.";
        _statusLabel.TextColor = UIColor.Label;

        try
        {
            var progress = new Progress<SteamOfflineInstallProgress>(value =>
            {
                InvokeOnMainThread(() =>
                {
                    if (_offlineInstallResultLabel is null || _offlineInstallDetailLabel is null)
                        return;

                    _offlineInstallResultLabel.Text = $"OFFLINE STATE: {value.Phase.ToString().ToUpperInvariant()}";
                    _offlineInstallResultLabel.TextColor = UIColor.Label;
                    _offlineInstallDetailLabel.Text =
                        $"{value.Message}\nFiles: {value.CompletedFiles}/{value.TotalFiles}\nBytes: {value.CompletedBytes}/{value.TotalBytes}" +
                        (string.IsNullOrWhiteSpace(value.CurrentFile) ? string.Empty : $"\nCurrent: {value.CurrentFile}");
                });
            });

            var result = await _offlineInstallInspection.RunAsync(
                progress,
                _operationCts!.Token);

            InvokeOnMainThread(() =>
            {
                _offlineInstallResultLabel.Text = result.Summary;
                _offlineInstallResultLabel.TextColor = result.Outcome switch
                {
                    SteamOfflineInstallOutcome.OfflineReady => UIColor.Label,
                    SteamOfflineInstallOutcome.NoManagedInstall => UIColor.SystemOrange,
                    SteamOfflineInstallOutcome.Cancelled => UIColor.SecondaryLabel,
                    _ => UIColor.SystemRed,
                };
                _offlineInstallDetailLabel.Text = FormatOfflineInstallDetail(result);
                _statusLabel.Text = result.Outcome switch
                {
                    SteamOfflineInstallOutcome.OfflineReady =>
                        "PASS: Step 13 local state is OfflineReady. The managed install was verified without consulting Steam/session/network; online manifest freshness remains unknown until an online manager check.",
                    SteamOfflineInstallOutcome.NoManagedInstall =>
                        "OFFLINE SETUP REQUIRED: no Step 12 managed install exists. Reconnect and complete the legitimate online setup path first.",
                    SteamOfflineInstallOutcome.Cancelled =>
                        "Step 13 local verification cancelled; no managed files were changed.",
                    _ =>
                        $"OFFLINE REPAIR REQUIRED: {result.Error ?? result.Outcome.ToString()}. Reconnect and use the proven Step 12 manager before treating the install as offline-ready.",
                };
                _statusLabel.TextColor = result.Outcome switch
                {
                    SteamOfflineInstallOutcome.OfflineReady => UIColor.Label,
                    SteamOfflineInstallOutcome.Cancelled => UIColor.SecondaryLabel,
                    SteamOfflineInstallOutcome.NoManagedInstall => UIColor.SystemOrange,
                    _ => UIColor.SystemRed,
                };
            });
        }
        catch (Exception ex)
        {
            InvokeOnMainThread(() =>
            {
                _offlineInstallResultLabel.Text = "OFFLINE CHECK: EXCEPTION";
                _offlineInstallResultLabel.TextColor = UIColor.SystemRed;
                _offlineInstallDetailLabel.Text = $"{ex.GetType().Name}: {ex.Message}";
                _statusLabel.Text = "FAIL: unhandled exception during Step 13 local-only inspection.";
                _statusLabel.TextColor = UIColor.SystemRed;
            });
        }
        finally
        {
            InvokeOnMainThread(EndSteamOperation);
        }
    }

    private async Task RunCompatibilityInventoryAsync()
    {
        if (_compatibilityInventoryResultLabel is null ||
            _compatibilityInventoryDetailLabel is null ||
            _statusLabel is null)
        {
            return;
        }

        BeginSteamOperation();
        _compatibilityInventoryResultLabel.Text = "COMPATIBILITY INVENTORY: RUNNING…";
        _compatibilityInventoryResultLabel.TextColor = UIColor.Label;
        _compatibilityInventoryDetailLabel.Text =
            "Read-only Step 14 inventory started. Re-proving OfflineReady, then classifying installed files and scanning managed metadata strings. No Steam/network request, game-file write, assembly load, or game launch is performed.";
        _statusLabel.Text = "STEP 14 INVENTORY RUNNING — local/read-only compatibility inspection only.";
        _statusLabel.TextColor = UIColor.Label;

        try
        {
            var progress = new Progress<SteamCompatibilityInventoryProgress>(value =>
            {
                InvokeOnMainThread(() =>
                {
                    if (_compatibilityInventoryResultLabel is null || _compatibilityInventoryDetailLabel is null)
                        return;

                    _compatibilityInventoryResultLabel.Text =
                        $"COMPATIBILITY INVENTORY: {value.Phase.ToString().ToUpperInvariant()}";
                    _compatibilityInventoryResultLabel.TextColor = UIColor.Label;
                    _compatibilityInventoryDetailLabel.Text =
                        $"{value.Message}\nFiles: {value.ProcessedFiles}/{value.TotalFiles}\nBytes: {value.ProcessedBytes}/{value.TotalBytes}" +
                        (string.IsNullOrWhiteSpace(value.CurrentRelativePath)
                            ? string.Empty
                            : $"\nCurrent: {value.CurrentRelativePath}");
                });
            });

            var result = await _compatibilityInventoryInspection.RunAsync(
                progress,
                _operationCts!.Token);

            InvokeOnMainThread(() =>
            {
                _compatibilityInventoryResultLabel.Text = result.Summary;
                _compatibilityInventoryResultLabel.TextColor = result.Outcome switch
                {
                    SteamCompatibilityInventoryOutcome.Complete => UIColor.Label,
                    SteamCompatibilityInventoryOutcome.Cancelled => UIColor.SecondaryLabel,
                    SteamCompatibilityInventoryOutcome.LocalInstallNotReady => UIColor.SystemOrange,
                    _ => UIColor.SystemRed,
                };
                _compatibilityInventoryDetailLabel.Text = FormatCompatibilityInventoryDetail(result);
                _statusLabel.Text = result.Outcome switch
                {
                    SteamCompatibilityInventoryOutcome.Complete =>
                        $"PASS: Step 14 classified {result.TotalFiles} installed files read-only. Review the reported dependency and potential iOS blocker signals before choosing the next compatibility boundary.",
                    SteamCompatibilityInventoryOutcome.LocalInstallNotReady =>
                        "STEP 14 BLOCKED: the managed install is not currently OfflineReady. Restore it with the proven Step 12 manager, then rerun the inventory.",
                    SteamCompatibilityInventoryOutcome.Cancelled =>
                        "Step 14 inventory cancelled; the managed install was not modified.",
                    _ =>
                        $"STEP 14 FAIL: {result.Error ?? result.Outcome.ToString()}. The managed install was not modified.",
                };
                _statusLabel.TextColor = result.Outcome switch
                {
                    SteamCompatibilityInventoryOutcome.Complete => UIColor.Label,
                    SteamCompatibilityInventoryOutcome.Cancelled => UIColor.SecondaryLabel,
                    SteamCompatibilityInventoryOutcome.LocalInstallNotReady => UIColor.SystemOrange,
                    _ => UIColor.SystemRed,
                };
            });
        }
        catch (Exception ex)
        {
            InvokeOnMainThread(() =>
            {
                _compatibilityInventoryResultLabel.Text = "COMPATIBILITY INVENTORY: EXCEPTION";
                _compatibilityInventoryResultLabel.TextColor = UIColor.SystemRed;
                _compatibilityInventoryDetailLabel.Text = $"{ex.GetType().Name}: {ex.Message}";
                _statusLabel.Text = "FAIL: unhandled exception during Step 14 read-only compatibility inventory.";
                _statusLabel.TextColor = UIColor.SystemRed;
            });
        }
        finally
        {
            InvokeOnMainThread(EndSteamOperation);
        }
    }

    private async Task RunGodotFoundationGatesABCAsync()
    {
        if (_godotFoundationResultLabel is null ||
            _godotFoundationDetailLabel is null ||
            _godotFoundationStartButton is null ||
            _godotFoundationGateDButton is null ||
            _godotHostContainer is null ||
            _statusLabel is null)
        {
            return;
        }

        if (_godotSessionStarted || _godotProcessRequiresRestart)
        {
            _statusLabel.Text = "A Step 15 Godot start has already touched process-global engine state. Finish Gate D if available, or force-quit/relaunch before another attempt.";
            _statusLabel.TextColor = UIColor.SystemOrange;
            return;
        }

        BeginSteamOperation(allowCancel: false);
        _godotFoundationGates.Reset();
        _godotHostContainer.Hidden = false;
        _godotFoundationResultLabel.Text = "GODOT FOUNDATION: GATE A RUNNING…";
        _godotFoundationResultLabel.TextColor = UIColor.Label;
        _godotFoundationDetailLabel.Text = "Gate A: resolving the statically linked Godot 4.5.1 native bridge. Later gates will not run if this gate fails.";
        _statusLabel.Text = "STEP 15 GATE A — native Godot availability/linkage.";
        _statusLabel.TextColor = UIColor.Label;

        try
        {
            string engineVersion;
            try
            {
                engineVersion = GodotStep15NativeBridge.EngineVersion;
            }
            catch (Exception ex)
            {
                RecordGodotGate(GodotFoundationGate.NativeAvailability, false,
                    $"Native bridge resolution failed: {ex.GetType().Name}: {ex.Message}");
                return;
            }

            if (!string.Equals(engineVersion, "4.5.1-stable", StringComparison.Ordinal))
            {
                RecordGodotGate(GodotFoundationGate.NativeAvailability, false,
                    $"Expected Godot 4.5.1-stable, native bridge reported '{engineVersion}'.");
                return;
            }

            RecordGodotGate(GodotFoundationGate.NativeAvailability, true,
                $"Native static bridge resolved and reported Godot {engineVersion}.");

            _godotFoundationResultLabel.Text = "GODOT FOUNDATION: GATE B RUNNING…";
            _godotFoundationDetailLabel.Text = FormatGodotFoundationDetail(
                "Gate B: initializing Godot with the project-owned smoke project, proving the render loop can stop and restart, then leaving it running for Gate C.");
            _statusLabel.Text = "STEP 15 GATE B — engine initialize + render-loop stop/restart.";

            var smokeProjectPath = Path.Combine(NSBundle.MainBundle.BundlePath, "Step15GodotSmokeProject");
            if (!File.Exists(Path.Combine(smokeProjectPath, "project.godot")))
            {
                RecordGodotGate(GodotFoundationGate.EngineInitializeRenderLoop, false,
                    $"Bundled smoke project missing: {smokeProjectPath}");
                return;
            }

            // Ensure the arranged subview has non-zero UIKit bounds before the
            // native bridge creates Godot's Metal-backed view.
            View?.LayoutIfNeeded();
            _godotHostContainer.LayoutIfNeeded();
            if (_godotHostContainer.Bounds.Width < 1 || _godotHostContainer.Bounds.Height < 1)
            {
                RecordGodotGate(GodotFoundationGate.EngineInitializeRenderLoop, false,
                    $"Godot host container has invalid bounds: {_godotHostContainer.Bounds}.");
                return;
            }

            var startResult = GodotStep15NativeBridge.Start(Handle, _godotHostContainer.Handle, smokeProjectPath);
            _godotProcessRequiresRestart = GodotStep15NativeBridge.RequiresProcessRestart;
            if (startResult != 0 || !GodotStep15NativeBridge.IsEngineStarted)
            {
                RecordGodotGate(GodotFoundationGate.EngineInitializeRenderLoop, false,
                    $"Native Godot start failed ({startResult}): {GodotStep15NativeBridge.LastError}" +
                    (_godotProcessRequiresRestart ? " Force-quit/relaunch before another launcher operation." : string.Empty));
                return;
            }

            _godotSessionStarted = true;
            var stopped = GodotStep15NativeBridge.StopRendering();
            var restarted = GodotStep15NativeBridge.StartRendering();
            if (!stopped || !restarted || !GodotStep15NativeBridge.IsRenderingActive)
            {
                RecordGodotGate(GodotFoundationGate.EngineInitializeRenderLoop, false,
                    $"Engine started but render-loop control failed. stop={stopped}, restart={restarted}, active={GodotStep15NativeBridge.IsRenderingActive}.");
                return;
            }

            RecordGodotGate(GodotFoundationGate.EngineInitializeRenderLoop, true,
                "Godot initialized from the bundled project; CADisplayLink render loop stopped and restarted successfully.");

            _godotFoundationResultLabel.Text = "GODOT FOUNDATION: GATE C RUNNING…";
            _godotFoundationDetailLabel.Text = FormatGodotFoundationDetail(
                "Gate C: waiting for Godot setup2/start, a Metal-backed rendering layer, and the project-owned scene's fresh render marker.");
            _statusLabel.Text = "STEP 15 GATE C — Metal smoke-scene render.";

            var gateCReady = await WaitForGodotConditionAsync(
                () => GodotStep15NativeBridge.IsSetupFinished &&
                      GodotStep15NativeBridge.IsMetalLayerReady &&
                      GodotStep15NativeBridge.IsRenderingActive &&
                      GodotStep15NativeBridge.RenderMarkerReady,
                TimeSpan.FromSeconds(30));

            if (!gateCReady)
            {
                RecordGodotGate(GodotFoundationGate.MetalRender, false,
                    $"Timed out waiting for Metal smoke scene. setup={GodotStep15NativeBridge.IsSetupFinished}, metal={GodotStep15NativeBridge.IsMetalLayerReady}, active={GodotStep15NativeBridge.IsRenderingActive}, marker={GodotStep15NativeBridge.RenderMarkerReady}, nativeError='{GodotStep15NativeBridge.LastError}'.");
                return;
            }

            RecordGodotGate(GodotFoundationGate.MetalRender, true,
                "Godot setup completed with a Metal rendering layer and the project-owned scene produced its render-ready marker.");

            _godotFoundationResultLabel.Text = "GODOT FOUNDATION IN PROGRESS — 3/4";
            _godotFoundationResultLabel.TextColor = UIColor.Label;
            _godotFoundationDetailLabel.Text = FormatGodotFoundationDetail(
                "Gates A–C PASS. Gate D is manual: tap inside the visible Godot panel until it turns green, send the app to the background once, return, then tap Verify Gate D.");
            _statusLabel.Text = "STEP 15 GATES A–C PASS. Complete the touch + background/foreground Gate D now. Do not run unrelated launcher tests until you relaunch after Step 15.";
            _statusLabel.TextColor = UIColor.Label;
        }
        catch (Exception ex)
        {
            try
            {
                _godotProcessRequiresRestart |= GodotStep15NativeBridge.RequiresProcessRestart;
            }
            catch
            {
                // If native bridge telemetry itself is unavailable, preserve the original exception.
            }
            var nextGate = (GodotFoundationGate)Math.Min(_godotFoundationGates.Results.Count + 1, 4);
            if (_godotFoundationGates.Snapshot().FirstFailingGate is null && _godotFoundationGates.Results.Count < 4)
            {
                try
                {
                    _godotFoundationGates.Record(nextGate, false, $"Unhandled {ex.GetType().Name}: {ex.Message}");
                }
                catch
                {
                    // Preserve the original exception in the UI if gate accounting itself cannot advance.
                }
            }
            _godotFoundationResultLabel.Text = _godotFoundationGates.Snapshot().Summary;
            _godotFoundationResultLabel.TextColor = UIColor.SystemRed;
            _godotFoundationDetailLabel.Text = FormatGodotFoundationDetail($"Unhandled Step 15 exception: {ex.GetType().Name}: {ex.Message}");
            _statusLabel.Text = "STEP 15 FAIL: stop at the first failing Godot Foundation gate and report this screen.";
            _statusLabel.TextColor = UIColor.SystemRed;
        }
        finally
        {
            EndSteamOperation();
        }
    }

    private void VerifyGodotFoundationGateD()
    {
        if (_godotFoundationResultLabel is null ||
            _godotFoundationDetailLabel is null ||
            _godotFoundationGateDButton is null ||
            _statusLabel is null)
        {
            return;
        }

        var snapshot = _godotFoundationGates.Snapshot();
        if (!_godotSessionStarted || snapshot.FirstFailingGate is not null || snapshot.Results.Count != 3)
        {
            _statusLabel.Text = "Gate D is only available after Gates A–C pass in this process.";
            _statusLabel.TextColor = UIColor.SystemOrange;
            return;
        }

        try
        {
            var touch = GodotStep15NativeBridge.TouchMarkerReady;
            var background = GodotStep15NativeBridge.BackgroundCount;
            var foreground = GodotStep15NativeBridge.ForegroundCount;
            var focusOut = GodotStep15NativeBridge.FocusOutCount;
            var focusIn = GodotStep15NativeBridge.FocusInCount;

            if (!touch || background < 1 || foreground < 1 || focusOut < 1 || focusIn < 1)
            {
                _godotFoundationResultLabel.Text = "GODOT FOUNDATION IN PROGRESS — 3/4";
                _godotFoundationResultLabel.TextColor = UIColor.SystemOrange;
                _godotFoundationDetailLabel.Text = FormatGodotFoundationDetail(
                    $"Gate D not complete yet. touch={YesNo(touch)}, background={background}, foreground={foreground}, focusOut={focusOut}, focusIn={focusIn}. Tap the Godot panel, background the app once, return, then verify again.");
                _statusLabel.Text = "STEP 15 GATE D PENDING — missing touch or lifecycle evidence; no failure recorded yet.";
                _statusLabel.TextColor = UIColor.SystemOrange;
                return;
            }

            _godotFoundationGates.Record(
                GodotFoundationGate.TouchLifecycle,
                true,
                $"Godot touch marker observed; lifecycle forwarding observed (background={background}, foreground={foreground}, focusOut={focusOut}, focusIn={focusIn}).");

            snapshot = _godotFoundationGates.Snapshot();
            _godotFoundationResultLabel.Text = snapshot.Summary;
            _godotFoundationResultLabel.TextColor = UIColor.Label;
            _godotFoundationDetailLabel.Text = FormatGodotFoundationDetail(
                "All four Step 15 ordered gates passed. Force-quit/relaunch before running the existing Foundation 5/5 regression; Step 15 does not attempt to execute any StS2 game content.");
            _statusLabel.Text = "PASS: STEP 15 GODOT FOUNDATION — 4/4. Native availability, engine/render-loop control, Metal project render, touch, and lifecycle are proven on this iPhone.";
            _statusLabel.TextColor = UIColor.Label;
            _godotFoundationGateDButton.Enabled = false;
        }
        catch (Exception ex)
        {
            _godotFoundationGates.Record(
                GodotFoundationGate.TouchLifecycle,
                false,
                $"Gate D native telemetry failed: {ex.GetType().Name}: {ex.Message}");
            _godotFoundationResultLabel.Text = _godotFoundationGates.Snapshot().Summary;
            _godotFoundationResultLabel.TextColor = UIColor.SystemRed;
            _godotFoundationDetailLabel.Text = FormatGodotFoundationDetail("Gate D failed while reading Godot touch/lifecycle telemetry.");
            _statusLabel.Text = "STEP 15 FAIL at Gate D. Stop and report this result; later work is not proven.";
            _statusLabel.TextColor = UIColor.SystemRed;
            _godotFoundationGateDButton.Enabled = false;
        }
    }

    private void RecordGodotGate(GodotFoundationGate gate, bool passed, string detail)
    {
        _godotFoundationGates.Record(gate, passed, detail);
        if (_godotFoundationResultLabel is not null)
        {
            _godotFoundationResultLabel.Text = _godotFoundationGates.Snapshot().Summary;
            _godotFoundationResultLabel.TextColor = passed ? UIColor.Label : UIColor.SystemRed;
        }
        if (_godotFoundationDetailLabel is not null)
            _godotFoundationDetailLabel.Text = FormatGodotFoundationDetail(detail);
        if (!passed && _statusLabel is not null)
        {
            var letter = (char)('A' + (int)gate - 1);
            _statusLabel.Text = $"STEP 15 FAIL at Gate {letter} ({gate}). Stop here; later Godot gates were not run.";
            _statusLabel.TextColor = UIColor.SystemRed;
        }
    }

    private string FormatGodotFoundationDetail(string tail)
    {
        var lines = new List<string>();
        foreach (var gate in _godotFoundationGates.Results)
        {
            var letter = (char)('A' + (int)gate.Gate - 1);
            lines.Add($"Gate {letter} — {gate.Gate}: {(gate.Passed ? "PASS" : "FAIL")} — {gate.Detail}");
        }

        if (_godotSessionStarted || _godotProcessRequiresRestart)
        {
            lines.Add($"Process relaunch required before another Godot/unrelated launcher operation: {YesNo(_godotProcessRequiresRestart)}");
        }

        if (_godotSessionStarted)
        {
            lines.Add($"Native engine started: {YesNo(GodotStep15NativeBridge.IsEngineStarted)}");
            lines.Add($"Setup finished: {YesNo(GodotStep15NativeBridge.IsSetupFinished)}");
            lines.Add($"Metal layer ready: {YesNo(GodotStep15NativeBridge.IsMetalLayerReady)}");
            lines.Add($"Render loop active: {YesNo(GodotStep15NativeBridge.IsRenderingActive)}");
            lines.Add($"Render marker: {YesNo(GodotStep15NativeBridge.RenderMarkerReady)}");
            lines.Add($"Touch marker: {YesNo(GodotStep15NativeBridge.TouchMarkerReady)}");
            lines.Add($"Lifecycle counts: focusOut={GodotStep15NativeBridge.FocusOutCount}, background={GodotStep15NativeBridge.BackgroundCount}, foreground={GodotStep15NativeBridge.ForegroundCount}, focusIn={GodotStep15NativeBridge.FocusInCount}");
        }

        lines.Add("Step 15 project: launcher-owned smoke scene only; managed StS2 install is not loaded, rewritten, or executed.");
        lines.Add("Audio/game runtime/Cecil/FMOD/Spine/Steamworks integration: NOT TESTED BY STEP 15");
        lines.Add(tail);
        return string.Join("\n", lines);
    }

    private async Task RunManagedPreparationFoundationAsync()
    {
        if (_managedPreparationResultLabel is null ||
            _managedPreparationDetailLabel is null ||
            _managedPreparationButton is null ||
            _statusLabel is null)
        {
            return;
        }

        if (_godotProcessRequiresRestart)
        {
            _statusLabel.Text = "Step 15 Godot process-global state is still active. Force-quit/relaunch before running Step 16 so Cecil/real-install evidence is isolated from the Godot session.";
            _statusLabel.TextColor = UIColor.SystemOrange;
            return;
        }

        BeginSteamOperation(allowCancel: true);
        _managedPreparationGates.Reset();
        _managedPreparationResultLabel.Text = "MANAGED PREPARATION: GATE A RUNNING…";
        _managedPreparationResultLabel.TextColor = UIColor.Label;
        _managedPreparationDetailLabel.Text = "Gate A: opening the bundled project-owned fixture as raw managed metadata with Mono.Cecil; the assembly is not loaded or executed.";
        _statusLabel.Text = "STEP 16 GATE A — Cecil fixture read.";
        _statusLabel.TextColor = UIColor.Label;

        try
        {
            var fixturePath = Path.Combine(
                NSBundle.MainBundle.BundlePath,
                "Step16Fixtures",
                "StS2Launcher.Step16.Fixture.dll");

            var gateA = await Task.Run(() => _managedPreparationFoundation.RunFixtureRead(fixturePath));
            if (!RecordManagedPreparationGate(gateA))
                return;

            _managedPreparationResultLabel.Text = "MANAGED PREPARATION: GATE B RUNNING…";
            _statusLabel.Text = "STEP 16 GATE B — Cecil fixture write/reopen.";
            var gateB = await Task.Run(() => _managedPreparationFoundation.RunFixtureRoundTrip(fixturePath));
            if (!RecordManagedPreparationGate(gateB))
                return;

            _managedPreparationResultLabel.Text = "MANAGED PREPARATION: GATE C RUNNING…";
            _statusLabel.Text = "STEP 16 GATE C — controlled fixture-only IL rewrite.";
            var gateC = await Task.Run(() => _managedPreparationFoundation.RunControlledIlRewrite(fixturePath));
            if (!RecordManagedPreparationGate(gateC))
                return;

            _managedPreparationResultLabel.Text = "MANAGED PREPARATION: GATE D RUNNING…";
            _statusLabel.Text = "STEP 16 GATE D — read-only receipt-backed StS2 managed metadata inspection.";
            var progress = new Progress<ManagedPreparationProgress>(value =>
            {
                var count = value.TotalItems > 0
                    ? $" ({value.ProcessedItems:N0}/{value.TotalItems:N0})"
                    : string.Empty;
                _managedPreparationDetailLabel.Text = FormatManagedPreparationDetail(
                    $"Gate D progress{count}: {value.Detail}" +
                    (string.IsNullOrWhiteSpace(value.CurrentPath) ? string.Empty : $"\nCurrent: {value.CurrentPath}"));
            });

            var token = _operationCts?.Token ?? CancellationToken.None;
            var gateD = await _managedPreparationFoundation.RunRealStS2MetadataInspectionAsync(progress, token);
            if (!RecordManagedPreparationGate(gateD))
                return;

            var snapshot = _managedPreparationGates.Snapshot();
            _managedPreparationResultLabel.Text = snapshot.Summary;
            _managedPreparationResultLabel.TextColor = UIColor.Label;
            _managedPreparationDetailLabel.Text = FormatManagedPreparationDetail(
                "All four Step 16 gates passed. Cecil proved read/write/reopen + a controlled project-owned IL transformation, then parsed the real installed StS2 managed metadata without rewriting or loading game assemblies.");
            _statusLabel.Text = "PASS: STEP 16 MANAGED PREPARATION — 4/4. Fixture read/write/rewrite and real read-only StS2 metadata inspection are proven on this iPhone.";
            _statusLabel.TextColor = UIColor.Label;
        }
        catch (OperationCanceledException)
        {
            _managedPreparationResultLabel.Text = "MANAGED PREPARATION: CANCELLED";
            _managedPreparationResultLabel.TextColor = UIColor.SecondaryLabel;
            _managedPreparationDetailLabel.Text = FormatManagedPreparationDetail(
                "Step 16 was cancelled. Fixture outputs may remain only under launcher-private Step16-ManagedPreparation scratch storage; the real managed install was not intentionally modified.");
            _statusLabel.Text = "STEP 16 CANCELLED — no later gate is considered proven.";
            _statusLabel.TextColor = UIColor.SecondaryLabel;
        }
        catch (Exception ex)
        {
            _managedPreparationResultLabel.Text = "MANAGED PREPARATION: EXCEPTION";
            _managedPreparationResultLabel.TextColor = UIColor.SystemRed;
            _managedPreparationDetailLabel.Text = FormatManagedPreparationDetail($"Unhandled Step 16 exception: {ex.GetType().Name}: {ex.Message}");
            _statusLabel.Text = "STEP 16 FAIL: stop at the current managed-preparation gate and report this screen.";
            _statusLabel.TextColor = UIColor.SystemRed;
        }
        finally
        {
            EndSteamOperation();
        }
    }

    private async Task RunCompatibilityCallSiteAnalysisAsync()
    {
        if (_compatibilityCallSiteResultLabel is null ||
            _compatibilityCallSiteDetailLabel is null ||
            _compatibilityCallSiteButton is null ||
            _statusLabel is null)
        {
            return;
        }

        if (_godotProcessRequiresRestart)
        {
            _statusLabel.Text = "Step 15 Godot process-global state is still active. Force-quit/relaunch before Step 17 so the read-only compatibility evidence is isolated from the Godot session.";
            _statusLabel.TextColor = UIColor.SystemOrange;
            return;
        }

        BeginSteamOperation(allowCancel: true);
        _compatibilityCallSiteGates.Reset();
        _compatibilityCallSiteAnalysis.Reset();
        _compatibilityCallSiteResultLabel.Text = "COMPATIBILITY CALL-SITE ANALYSIS: GATE A RUNNING…";
        _compatibilityCallSiteResultLabel.TextColor = UIColor.Label;
        _compatibilityCallSiteDetailLabel.Text = "Gate A: re-proving OfflineReady and selecting the receipt-backed macOS arm64 + architecture-neutral managed scope without opening game assemblies.";
        _statusLabel.Text = "STEP 17 GATE A — ARM64 managed scope + local integrity.";
        _statusLabel.TextColor = UIColor.Label;

        try
        {
            var token = _operationCts?.Token ?? CancellationToken.None;
            var progress = new Progress<CompatibilityCallSiteProgress>(value =>
            {
                var count = value.TotalItems > 0
                    ? $" ({value.ProcessedItems:N0}/{value.TotalItems:N0})"
                    : string.Empty;
                _compatibilityCallSiteDetailLabel.Text = FormatCompatibilityCallSiteDetail(
                    $"Gate {(char)('A' + (int)value.Gate - 1)} progress{count}: {value.Detail}" +
                    (string.IsNullOrWhiteSpace(value.CurrentPath) ? string.Empty : $"\nCurrent: {value.CurrentPath}"));
            });

            var gateA = await _compatibilityCallSiteAnalysis.RunArm64ManagedScopeAsync(progress, token);
            if (!RecordCompatibilityCallSiteGate(gateA))
                return;

            _compatibilityCallSiteResultLabel.Text = "COMPATIBILITY CALL-SITE ANALYSIS: GATE B RUNNING…";
            _statusLabel.Text = "STEP 17 GATE B — actual IL method-reference scan.";
            var gateB = await _compatibilityCallSiteAnalysis.RunActualIlCallSiteScanAsync(progress, token);
            if (!RecordCompatibilityCallSiteGate(gateB))
                return;

            _compatibilityCallSiteResultLabel.Text = "COMPATIBILITY CALL-SITE ANALYSIS: GATE C RUNNING…";
            _statusLabel.Text = "STEP 17 GATE C — native/platform interop classification.";
            var gateC = await Task.Run(() => _compatibilityCallSiteAnalysis.RunNativePlatformInteropClassification(), token);
            if (!RecordCompatibilityCallSiteGate(gateC))
                return;

            _compatibilityCallSiteResultLabel.Text = "COMPATIBILITY CALL-SITE ANALYSIS: GATE D RUNNING…";
            _statusLabel.Text = "STEP 17 GATE D — primary sts2.dll dependency pressure map + post-scan hashes.";
            var gateD = await _compatibilityCallSiteAnalysis.RunPrimaryDependencyPressureMapAsync(progress, token);
            if (!RecordCompatibilityCallSiteGate(gateD))
                return;

            var snapshot = _compatibilityCallSiteGates.Snapshot();
            _compatibilityCallSiteResultLabel.Text = snapshot.Summary;
            _compatibilityCallSiteResultLabel.TextColor = UIColor.Label;
            _compatibilityCallSiteDetailLabel.Text = FormatCompatibilityCallSiteDetail(
                "All four Step 17 gates passed. The broad Step 14 indicators have been narrowed to concrete arm64 IL/native/dependency evidence, while every scanned file still matches its Step 12 receipt SHA-1.");
            _statusLabel.Text = "PASS: STEP 17 COMPATIBILITY CALL-SITE ANALYSIS — 4/4. Upload the Gate B–D evidence so the next compatibility target can be chosen from actual IL rather than string indicators.";
            _statusLabel.TextColor = UIColor.Label;
        }
        catch (OperationCanceledException)
        {
            _compatibilityCallSiteResultLabel.Text = "COMPATIBILITY CALL-SITE ANALYSIS: CANCELLED";
            _compatibilityCallSiteResultLabel.TextColor = UIColor.SecondaryLabel;
            _compatibilityCallSiteDetailLabel.Text = FormatCompatibilityCallSiteDetail(
                "Step 17 was cancelled. The analysis is read-only; no game-file write or runtime load was intentionally performed.");
            _statusLabel.Text = "STEP 17 CANCELLED — no later gate is considered proven.";
            _statusLabel.TextColor = UIColor.SecondaryLabel;
        }
        catch (Exception ex)
        {
            _compatibilityCallSiteResultLabel.Text = "COMPATIBILITY CALL-SITE ANALYSIS: EXCEPTION";
            _compatibilityCallSiteResultLabel.TextColor = UIColor.SystemRed;
            _compatibilityCallSiteDetailLabel.Text = FormatCompatibilityCallSiteDetail($"Unhandled Step 17 exception: {ex.GetType().Name}: {ex.Message}");
            _statusLabel.Text = "STEP 17 FAIL: stop at the current call-site-analysis gate and report this screen.";
            _statusLabel.TextColor = UIColor.SystemRed;
        }
        finally
        {
            EndSteamOperation();
        }
    }

    private async Task RunRealAssemblyRewriteWorkspaceAsync()
    {
        if (_realAssemblyRewriteResultLabel is null ||
            _realAssemblyRewriteDetailLabel is null ||
            _realAssemblyRewriteButton is null ||
            _statusLabel is null)
        {
            return;
        }

        if (_godotProcessRequiresRestart)
        {
            _statusLabel.Text = "Step 15 Godot process-global state is still active. Force-quit/relaunch before Step 18 so Cecil real-copy rewrite testing is isolated from the Godot session.";
            _statusLabel.TextColor = UIColor.SystemOrange;
            return;
        }

        BeginSteamOperation(allowCancel: true);
        _realAssemblyRewriteGates.Reset();
        _realAssemblyRewriteWorkspace.Reset();
        _realAssemblyRewriteResultLabel.Text = "REAL ASSEMBLY REWRITE WORKSPACE: GATE A RUNNING…";
        _realAssemblyRewriteResultLabel.TextColor = UIColor.Label;
        _realAssemblyRewriteDetailLabel.Text = "Gate A: re-proving OfflineReady and cloning the receipt-backed ARM64/shared managed scope into launcher-private Step 18 scratch storage with per-file SHA-1 verification.";
        _statusLabel.Text = "STEP 18 GATE A — clone receipt-backed ARM64 compatibility workspace.";
        _statusLabel.TextColor = UIColor.Label;

        try
        {
            var token = _operationCts?.Token ?? CancellationToken.None;
            var progress = new Progress<RealAssemblyRewriteProgress>(value =>
            {
                var count = value.TotalItems > 0
                    ? $" ({value.ProcessedItems:N0}/{value.TotalItems:N0})"
                    : string.Empty;
                _realAssemblyRewriteDetailLabel.Text = FormatRealAssemblyRewriteDetail(
                    $"Gate {(char)('A' + (int)value.Gate - 1)} progress{count}: {value.Detail}" +
                    (string.IsNullOrWhiteSpace(value.CurrentPath) ? string.Empty : $"\nCurrent: {value.CurrentPath}"));
            });

            var gateA = await _realAssemblyRewriteWorkspace.RunWorkspaceCloneAsync(progress, token);
            if (!RecordRealAssemblyRewriteGate(gateA))
                return;

            _realAssemblyRewriteResultLabel.Text = "REAL ASSEMBLY REWRITE WORKSPACE: GATE B RUNNING…";
            _statusLabel.Text = "STEP 18 GATE B — Cecil write/reopen of copied primary sts2.dll.";
            var gateB = await Task.Run(() => _realAssemblyRewriteWorkspace.RunPrimaryRoundTrip(), token);
            if (!RecordRealAssemblyRewriteGate(gateB))
                return;

            _realAssemblyRewriteResultLabel.Text = "REAL ASSEMBLY REWRITE WORKSPACE: GATE C RUNNING…";
            _statusLabel.Text = "STEP 18 GATE C — semantics-neutral NOP rewrite on copied sts2.dll only.";
            var gateC = await Task.Run(() => _realAssemblyRewriteWorkspace.RunNeutralIlRewrite(), token);
            if (!RecordRealAssemblyRewriteGate(gateC))
                return;

            _realAssemblyRewriteResultLabel.Text = "REAL ASSEMBLY REWRITE WORKSPACE: GATE D RUNNING…";
            _statusLabel.Text = "STEP 18 GATE D — source/install SHA-1 isolation audit.";
            var gateD = await _realAssemblyRewriteWorkspace.RunIsolationAuditAsync(progress, token);
            if (!RecordRealAssemblyRewriteGate(gateD))
                return;

            var snapshot = _realAssemblyRewriteGates.Snapshot();
            _realAssemblyRewriteResultLabel.Text = snapshot.Summary;
            _realAssemblyRewriteResultLabel.TextColor = UIColor.Label;
            _realAssemblyRewriteDetailLabel.Text = FormatRealAssemblyRewriteDetail(
                "All four Step 18 gates passed. A receipt-identical ARM64 managed workspace was created, Cecil round-tripped the real copied sts2.dll, one neutral NOP was written/reopened in a copy, and every original managed file in scope still matched its trusted receipt SHA-1.");
            _statusLabel.Text = "PASS: STEP 18 REAL ASSEMBLY REWRITE WORKSPACE — 4/4. Real copied-assembly writing is proven; the actual managed install remained unchanged.";
            _statusLabel.TextColor = UIColor.Label;
        }
        catch (OperationCanceledException)
        {
            _realAssemblyRewriteResultLabel.Text = "REAL ASSEMBLY REWRITE WORKSPACE: CANCELLED";
            _realAssemblyRewriteResultLabel.TextColor = UIColor.SecondaryLabel;
            _realAssemblyRewriteDetailLabel.Text = FormatRealAssemblyRewriteDetail(
                "Step 18 was cancelled. Gate A recreates its launcher-private workspace from scratch on the next run; no write to the real managed install is intentional." );
            _statusLabel.Text = "STEP 18 CANCELLED — no later gate is considered proven.";
            _statusLabel.TextColor = UIColor.SecondaryLabel;
        }
        catch (Exception ex)
        {
            _realAssemblyRewriteResultLabel.Text = "REAL ASSEMBLY REWRITE WORKSPACE: EXCEPTION";
            _realAssemblyRewriteResultLabel.TextColor = UIColor.SystemRed;
            _realAssemblyRewriteDetailLabel.Text = FormatRealAssemblyRewriteDetail($"Unhandled Step 18 exception: {ex.GetType().Name}: {ex.Message}");
            _statusLabel.Text = "STEP 18 FAIL: stop at the current real-assembly rewrite gate and report this screen.";
            _statusLabel.TextColor = UIColor.SystemRed;
        }
        finally
        {
            EndSteamOperation();
        }
    }

    private bool RecordRealAssemblyRewriteGate(RealAssemblyRewriteGateResult result)
    {
        _realAssemblyRewriteGates.Record(result.Gate, result.Passed, result.Detail);
        if (_realAssemblyRewriteResultLabel is not null)
        {
            _realAssemblyRewriteResultLabel.Text = _realAssemblyRewriteGates.Snapshot().Summary;
            _realAssemblyRewriteResultLabel.TextColor = result.Passed ? UIColor.Label : UIColor.SystemRed;
        }
        if (_realAssemblyRewriteDetailLabel is not null)
            _realAssemblyRewriteDetailLabel.Text = FormatRealAssemblyRewriteDetail(result.Detail);
        if (!result.Passed && _statusLabel is not null)
        {
            var letter = (char)('A' + (int)result.Gate - 1);
            _statusLabel.Text = $"STEP 18 FAIL at Gate {letter} ({result.Gate}). Stop here; later real-assembly rewrite gates were not run.";
            _statusLabel.TextColor = UIColor.SystemRed;
        }
        return result.Passed;
    }

    private string FormatRealAssemblyRewriteDetail(string tail)
    {
        var lines = new List<string>();
        foreach (var gate in _realAssemblyRewriteGates.Results)
        {
            var letter = (char)('A' + (int)gate.Gate - 1);
            lines.Add($"Gate {letter} — {gate.Gate}: {(gate.Passed ? "PASS" : "FAIL")}");
            lines.Add(gate.Detail);
            lines.Add(string.Empty);
        }

        lines.Add("Step 18 write scope: launcher-private Step18-RealAssemblyRewrite copies only; the Step 12 receipt-backed managed install stays read-only.");
        lines.Add("Gate C transformation is intentionally semantics-neutral: one IL NOP inserted into a deterministic method of the copied primary arm64 sts2.dll.");
        lines.Add("Cecil writer-required dependency resolution stays confined to the SHA-1-verified Step 18 workspace; Assembly.Load, StS2 execution, FMOD/Spine runtime integration, Cloud, or Workshop is not advanced by Step 18.");
        lines.Add("Step 15 orientation presentation quirk remains a known non-blocking cleanup item.");
        lines.Add(tail);
        return string.Join("\n", lines);
    }

    private async Task RunExpressionInterpreterCompatibilityAsync()
    {
        if (_expressionInterpreterCompatibilityResultLabel is null ||
            _expressionInterpreterCompatibilityDetailLabel is null ||
            _expressionInterpreterCompatibilityButton is null ||
            _statusLabel is null)
        {
            return;
        }

        if (_godotProcessRequiresRestart)
        {
            _statusLabel.Text = "Step 15 Godot process-global state is still active. Force-quit/relaunch before Step 19 so the expression runtime/fallback proof runs in a clean process.";
            _statusLabel.TextColor = UIColor.SystemOrange;
            return;
        }

        BeginSteamOperation(allowCancel: true);
        _expressionInterpreterCompatibilityGates.Reset();
        _expressionInterpreterCompatibility.Reset();
        _expressionInterpreterCompatibilityResultLabel.Text = "EXPRESSION INTERPRETER COMPATIBILITY: GATE A RUNNING…";
        _expressionInterpreterCompatibilityResultLabel.TextColor = UIColor.Label;
        _expressionInterpreterCompatibilityDetailLabel.Text = "Gate A: proving Compile(), Compile(false), and Compile(true) in this physical no-dynamic-code iOS process, then cloning a fresh receipt-backed ARM64/shared Step 19 workspace.";
        _statusLabel.Text = "STEP 19.2 GATE A — host expression fallback + receipt-backed workspace clone.";
        _statusLabel.TextColor = UIColor.Label;

        try
        {
            var token = _operationCts?.Token ?? CancellationToken.None;
            var progress = new Progress<ExpressionInterpreterCompatibilityProgress>(value =>
            {
                var count = value.TotalItems > 0
                    ? $" ({value.ProcessedItems:N0}/{value.TotalItems:N0})"
                    : string.Empty;
                _expressionInterpreterCompatibilityDetailLabel.Text = FormatExpressionInterpreterCompatibilityDetail(
                    $"Gate {(char)('A' + (int)value.Gate - 1)} progress{count}: {value.Detail}" +
                    (string.IsNullOrWhiteSpace(value.CurrentPath) ? string.Empty : $"\nCurrent: {value.CurrentPath}"));
            });

            var gateA = await _expressionInterpreterCompatibility.RunInterpreterCapabilityAndWorkspaceCloneAsync(progress, token);
            if (!RecordExpressionInterpreterCompatibilityGate(gateA))
                return;

            _expressionInterpreterCompatibilityResultLabel.Text = "EXPRESSION INTERPRETER COMPATIBILITY: GATE B RUNNING…";
            _statusLabel.Text = "STEP 19.2 GATE B — read-only Compile-site classification across consumer/framework and IL-only/ReadyToRun boundaries.";
            var gateB = await Task.Run(() => _expressionInterpreterCompatibility.RunRealCompileTargetDiscovery(), token);
            if (!RecordExpressionInterpreterCompatibilityGate(gateB))
                return;

            _expressionInterpreterCompatibilityResultLabel.Text = "EXPRESSION INTERPRETER COMPATIBILITY: GATE C RUNNING…";
            _statusLabel.Text = "STEP 19.2 GATE C — zero Cecil writes; build byte-identical prepared tree and prove immediate SHA-1 equality.";
            var gateC = await Task.Run(() => _expressionInterpreterCompatibility.RunHostFallbackPreparedCopy(), token);
            if (!RecordExpressionInterpreterCompatibilityGate(gateC))
                return;

            _expressionInterpreterCompatibilityResultLabel.Text = "EXPRESSION INTERPRETER COMPATIBILITY: GATE D RUNNING…";
            _statusLabel.Text = "STEP 19.2 GATE D — source/prepared/live full SHA-1 isolation audit with zero managed mutations.";
            var gateD = await _expressionInterpreterCompatibility.RunIsolationAuditAsync(progress, token);
            if (!RecordExpressionInterpreterCompatibilityGate(gateD))
                return;

            var snapshot = _expressionInterpreterCompatibilityGates.Snapshot();
            _expressionInterpreterCompatibilityResultLabel.Text = snapshot.Summary;
            _expressionInterpreterCompatibilityResultLabel.TextColor = UIColor.Label;
            _expressionInterpreterCompatibilityDetailLabel.Text = FormatExpressionInterpreterCompatibilityDetail(
                "All four Step 19 gates passed. The physical launcher proved Compile(), Compile(false), and Compile(true) against the no-dynamic-code iOS host, classified real Compile sites across consumer/framework and IL-only/ReadyToRun boundaries, performed zero Cecil assembly writes, kept the complete prepared tree byte-identical, and proved trusted source/live-install isolation.");
            _statusLabel.Text = "PASS: STEP 19.2 EXPRESSION INTERPRETER COMPATIBILITY — 4/4. Host runtime fallback + framework boundary + zero-write prepared tree are proven; no copied desktop framework image was mutated and no game assembly was executed.";
            _statusLabel.TextColor = UIColor.Label;
        }
        catch (OperationCanceledException)
        {
            _expressionInterpreterCompatibilityResultLabel.Text = "EXPRESSION INTERPRETER COMPATIBILITY: CANCELLED";
            _expressionInterpreterCompatibilityResultLabel.TextColor = UIColor.SecondaryLabel;
            _expressionInterpreterCompatibilityDetailLabel.Text = FormatExpressionInterpreterCompatibilityDetail(
                "Step 19 was cancelled. Gate A recreates the launcher-private Step 19 workspace from scratch on the next run; the real managed install is never an intended write target.");
            _statusLabel.Text = "STEP 19.2 CANCELLED — no later gate is considered proven.";
            _statusLabel.TextColor = UIColor.SecondaryLabel;
        }
        catch (Exception ex)
        {
            _expressionInterpreterCompatibilityResultLabel.Text = "EXPRESSION INTERPRETER COMPATIBILITY: EXCEPTION";
            _expressionInterpreterCompatibilityResultLabel.TextColor = UIColor.SystemRed;
            _expressionInterpreterCompatibilityDetailLabel.Text = FormatExpressionInterpreterCompatibilityDetail($"Unhandled Step 19 exception: {ex.GetType().Name}: {ex.Message}");
            _statusLabel.Text = "STEP 19.2 FAIL: stop at the current expression-interpreter compatibility gate and report this screen.";
            _statusLabel.TextColor = UIColor.SystemRed;
        }
        finally
        {
            EndSteamOperation();
        }
    }

    private async Task RunDynamicManagedExecutionFoundationAsync()
    {
        if (_dynamicManagedExecutionResultLabel is null ||
            _dynamicManagedExecutionDetailLabel is null ||
            _dynamicManagedExecutionButton is null ||
            _statusLabel is null)
        {
            return;
        }

        if (_godotProcessRequiresRestart)
        {
            _statusLabel.Text = "Step 15 Godot process-global state is still active. Force-quit/relaunch before Step 20 so the external managed-execution proof runs in a clean host process.";
            _statusLabel.TextColor = UIColor.SystemOrange;
            return;
        }

        BeginSteamOperation(allowCancel: true);
        _dynamicManagedExecutionGates.Reset();
        _dynamicManagedExecutionFoundation.Reset();
        _dynamicManagedExecutionResultLabel.Text = "DYNAMIC MANAGED EXECUTION FOUNDATION: GATE A RUNNING…";
        _dynamicManagedExecutionResultLabel.TextColor = UIColor.Label;
        _dynamicManagedExecutionDetailLabel.Text = "Gate A: re-proving OfflineReady and validating/copying the exact-hash project-owned external managed fixtures without loading them.";
        _statusLabel.Text = "STEP 20 GATE A — fixture integrity + OfflineReady.";
        _statusLabel.TextColor = UIColor.Label;

        try
        {
            var token = _operationCts?.Token ?? CancellationToken.None;
            var progress = new Progress<DynamicManagedExecutionProgress>(value =>
            {
                var count = value.TotalItems > 0 ? $" ({value.ProcessedItems:N0}/{value.TotalItems:N0})" : string.Empty;
                _dynamicManagedExecutionDetailLabel.Text = FormatDynamicManagedExecutionDetail(
                    $"Gate {(char)('A' + (int)value.Gate - 1)} progress{count}: {value.Detail}" +
                    (string.IsNullOrWhiteSpace(value.CurrentPath) ? string.Empty : $"\nCurrent: {value.CurrentPath}"));
            });

            var gateA = await _dynamicManagedExecutionFoundation.RunFixtureIntegrityAndOfflineReadyAsync(progress, token);
            if (!RecordDynamicManagedExecutionGate(gateA)) return;

            _dynamicManagedExecutionResultLabel.Text = "DYNAMIC MANAGED EXECUTION FOUNDATION: GATE B RUNNING…";
            _statusLabel.Text = "STEP 20 GATE B — load and execute non-AOT project-owned IL from verified bytes.";
            var gateB = await Task.Run(() => _dynamicManagedExecutionFoundation.RunDynamicFixtureExecution(), token);
            if (!RecordDynamicManagedExecutionGate(gateB)) return;

            _dynamicManagedExecutionResultLabel.Text = "DYNAMIC MANAGED EXECUTION FOUNDATION: GATE C RUNNING…";
            _statusLabel.Text = "STEP 20 GATE C — exact private managed dependency resolution + transitive execution.";
            var gateC = await Task.Run(() => _dynamicManagedExecutionFoundation.RunPrivateDependencyResolution(), token);
            if (!RecordDynamicManagedExecutionGate(gateC)) return;

            _dynamicManagedExecutionResultLabel.Text = "DYNAMIC MANAGED EXECUTION FOUNDATION: GATE D RUNNING…";
            _statusLabel.Text = "STEP 20 GATE D — fixture + managed-install isolation audit.";
            var gateD = await _dynamicManagedExecutionFoundation.RunIsolationAuditAsync(progress, token);
            if (!RecordDynamicManagedExecutionGate(gateD)) return;

            var snapshot = _dynamicManagedExecutionGates.Snapshot();
            _dynamicManagedExecutionResultLabel.Text = snapshot.Summary;
            _dynamicManagedExecutionResultLabel.TextColor = UIColor.Label;
            _dynamicManagedExecutionDetailLabel.Text = FormatDynamicManagedExecutionDetail(
                "All four Step 20 gates passed. A managed DLL that was not linked/AOT-compiled into the IPA executed from verified bytes, a second runtime-loaded fixture resolved and executed one verified private dependency, and the receipt-backed StS2 install stayed untouched. Run OfflineReady + Foundation 5/5 to close Step 20.");
            _statusLabel.Text = "PASS: STEP 20 DYNAMIC MANAGED EXECUTION FOUNDATION — 4/4. External IL execution + private dependency resolution are physically proven; no StS2 assembly was loaded.";
            _statusLabel.TextColor = UIColor.Label;
        }
        catch (OperationCanceledException)
        {
            _dynamicManagedExecutionResultLabel.Text = "DYNAMIC MANAGED EXECUTION FOUNDATION: CANCELLED";
            _dynamicManagedExecutionResultLabel.TextColor = UIColor.SecondaryLabel;
            _dynamicManagedExecutionDetailLabel.Text = FormatDynamicManagedExecutionDetail("Step 20 was cancelled. Rerunning Gate A recreates only the launcher-private fixture workspace; the managed game install is never an intended write target.");
            _statusLabel.Text = "STEP 20 CANCELLED — no later gate is considered proven.";
            _statusLabel.TextColor = UIColor.SecondaryLabel;
        }
        catch (Exception ex)
        {
            _dynamicManagedExecutionResultLabel.Text = "DYNAMIC MANAGED EXECUTION FOUNDATION: EXCEPTION";
            _dynamicManagedExecutionResultLabel.TextColor = UIColor.SystemRed;
            _dynamicManagedExecutionDetailLabel.Text = FormatDynamicManagedExecutionDetail($"Unhandled Step 20 exception: {ex.GetType().Name}: {ex.Message}");
            _statusLabel.Text = "STEP 20 FAIL: stop at the current dynamic-managed-execution gate and report this screen.";
            _statusLabel.TextColor = UIColor.SystemRed;
        }
        finally
        {
            EndSteamOperation();
        }
    }

    private async Task RunPreparedRuntimeFrameworkBindingAsync()
    {
        if (_runtimeFrameworkBindingResultLabel is null ||
            _runtimeFrameworkBindingDetailLabel is null ||
            _runtimeFrameworkBindingButton is null ||
            _statusLabel is null)
        {
            return;
        }

        if (_godotProcessRequiresRestart)
        {
            _statusLabel.Text = "Step 15 Godot process-global state is still active. Force-quit/relaunch before Step 21 so the real dependency/binding plan is measured in a clean host process.";
            _statusLabel.TextColor = UIColor.SystemOrange;
            return;
        }

        BeginSteamOperation(allowCancel: true);
        _runtimeFrameworkBindingGates.Reset();
        _preparedRuntimeFrameworkBinding.Reset();
        _runtimeFrameworkBindingResultLabel.Text = "PREPARED RUNTIME / FRAMEWORK BINDING: GATE A RUNNING…";
        _runtimeFrameworkBindingResultLabel.TextColor = UIColor.Label;
        _runtimeFrameworkBindingDetailLabel.Text = "Gate A: re-proving OfflineReady and cloning/classifying the real receipt-backed ARM64/shared managed scope without CLR-loading StS2.";
        _statusLabel.Text = "STEP 21 GATE A — runtime payload classification.";
        _statusLabel.TextColor = UIColor.Label;

        try
        {
            var token = _operationCts?.Token ?? CancellationToken.None;
            var progress = new Progress<RuntimeFrameworkBindingProgress>(value =>
            {
                var count = value.TotalItems > 0 ? $" ({value.ProcessedItems:N0}/{value.TotalItems:N0})" : string.Empty;
                _runtimeFrameworkBindingDetailLabel.Text = FormatRuntimeFrameworkBindingDetail(
                    $"Gate {(char)('A' + (int)value.Gate - 1)} progress{count}: {value.Detail}" +
                    (string.IsNullOrWhiteSpace(value.CurrentPath) ? string.Empty : $"\nCurrent: {value.CurrentPath}"));
            });

            var gateA = await _preparedRuntimeFrameworkBinding.RunRuntimePayloadClassificationAsync(progress, token);
            if (!RecordRuntimeFrameworkBindingGate(gateA)) return;

            _runtimeFrameworkBindingResultLabel.Text = "PREPARED RUNTIME / FRAMEWORK BINDING: GATE B RUNNING…";
            _statusLabel.Text = "STEP 21 GATE B — real AssemblyRef graph + iOS host framework/private binding plan.";
            var gateB = await Task.Run(() => _preparedRuntimeFrameworkBinding.RunHostFrameworkBindingPlan(), token);
            if (!RecordRuntimeFrameworkBindingGate(gateB)) return;

            _runtimeFrameworkBindingResultLabel.Text = "PREPARED RUNTIME / FRAMEWORK BINDING: GATE C RUNNING…";
            _statusLabel.Text = "STEP 21 GATE C — byte-identical execution-oriented IL-only prepared set + persisted binding plan.";
            var gateC = await _preparedRuntimeFrameworkBinding.RunPreparedRuntimeAssemblySetAsync(progress, token);
            if (!RecordRuntimeFrameworkBindingGate(gateC)) return;

            _runtimeFrameworkBindingResultLabel.Text = "PREPARED RUNTIME / FRAMEWORK BINDING: GATE D RUNNING…";
            _statusLabel.Text = "STEP 21 GATE D — source/prepared/live/plan closure audit.";
            var gateD = await _preparedRuntimeFrameworkBinding.RunClosureAuditAsync(progress, token);
            if (!RecordRuntimeFrameworkBindingGate(gateD)) return;

            await TryExportRuntimeBindingDiagnosticsAsync(automatic: true, token);

            var snapshot = _runtimeFrameworkBindingGates.Snapshot();
            _runtimeFrameworkBindingResultLabel.Text = snapshot.Summary;
            _runtimeFrameworkBindingResultLabel.TextColor = UIColor.Label;
            _runtimeFrameworkBindingDetailLabel.Text = FormatRuntimeFrameworkBindingDetail(
                "All four Step 21 gates passed. The real managed dependency graph has an audited host/private binding plan and byte-identical prepared IL set. Step 21.1 also attempted to refresh the Files-accessible full diagnostic report. Read Gate B/D's Runtime closure ready signal before Step 22.");
            _statusLabel.Text = "PASS: STEP 21 PREPARED RUNTIME / FRAMEWORK BINDING — 4/4. Binding plan is physically audited; inspect Runtime closure ready YES/NO before the next subsystem.";
            _statusLabel.TextColor = UIColor.Label;
        }
        catch (OperationCanceledException)
        {
            _runtimeFrameworkBindingResultLabel.Text = "PREPARED RUNTIME / FRAMEWORK BINDING: CANCELLED";
            _runtimeFrameworkBindingResultLabel.TextColor = UIColor.SecondaryLabel;
            _runtimeFrameworkBindingDetailLabel.Text = FormatRuntimeFrameworkBindingDetail("Step 21 was cancelled. Rerunning Gate A recreates only the launcher-private Step 21 workspace; the receipt-backed managed install is never an intended write target.");
            _statusLabel.Text = "STEP 21 CANCELLED — no later gate is considered proven.";
            _statusLabel.TextColor = UIColor.SecondaryLabel;
        }
        catch (Exception ex)
        {
            _runtimeFrameworkBindingResultLabel.Text = "PREPARED RUNTIME / FRAMEWORK BINDING: EXCEPTION";
            _runtimeFrameworkBindingResultLabel.TextColor = UIColor.SystemRed;
            _runtimeFrameworkBindingDetailLabel.Text = FormatRuntimeFrameworkBindingDetail($"Unhandled Step 21 exception: {ex.GetType().Name}: {ex.Message}");
            _statusLabel.Text = "STEP 21 FAIL: stop at the current runtime/framework-binding gate and report this screen.";
            _statusLabel.TextColor = UIColor.SystemRed;
        }
        finally
        {
            EndSteamOperation();
        }
    }

    private async Task RunRuntimeBindingDiagnosticsExportAsync()
    {
        await TryExportRuntimeBindingDiagnosticsAsync(automatic: false, CancellationToken.None);
    }

    private async Task TryExportRuntimeBindingDiagnosticsAsync(bool automatic, CancellationToken cancellationToken)
    {
        if (_runtimeBindingDiagnosticsExportResultLabel is null)
            return;

        try
        {
            _runtimeBindingDiagnosticsExportResultLabel.Text = automatic
                ? "DIAGNOSTIC EXPORT: refreshing complete report after Gate D…"
                : "DIAGNOSTIC EXPORT: reading persisted Step 21 plan and writing Files report…";
            _runtimeBindingDiagnosticsExportResultLabel.TextColor = UIColor.Label;

            var result = await _runtimeBindingDiagnosticsExporter.ExportAsync(cancellationToken);
            _runtimeBindingDiagnosticsExportResultLabel.Text =
                $"DIAGNOSTIC EXPORT: PASS — {result.BlockerCount:N0} blockers / {result.UniqueBlockedRequestedIdentityCount:N0} unique requested identities\n" +
                $"Files: On My iPhone → StS2 Launcher → StS2Launcher → {RuntimeBindingDiagnosticsExporter.ReportFileName}\n" +
                $"Runtime closure ready: {(result.RuntimeClosureReady ? "YES" : "NO")}\n" +
                $"Plan SHA-256: {result.PlanSha256}\nReport SHA-256: {result.ReportSha256}";
            _runtimeBindingDiagnosticsExportResultLabel.TextColor = UIColor.Label;

            if (!automatic && _statusLabel is not null)
            {
                _statusLabel.Text = $"STEP 21.1 DIAGNOSTIC EXPORT PASS — open Files and send {RuntimeBindingDiagnosticsExporter.ReportFileName}. No game CLR load was attempted.";
                _statusLabel.TextColor = UIColor.Label;
            }
        }
        catch (OperationCanceledException)
        {
            if (automatic)
                throw;
            _runtimeBindingDiagnosticsExportResultLabel.Text = "DIAGNOSTIC EXPORT: CANCELLED";
            _runtimeBindingDiagnosticsExportResultLabel.TextColor = UIColor.SecondaryLabel;
        }
        catch (Exception ex)
        {
            _runtimeBindingDiagnosticsExportResultLabel.Text =
                $"DIAGNOSTIC EXPORT: FAIL — {ex.GetType().Name}: {ex.Message}\n" +
                "If the persisted Step 21 plan is missing, rerun Step 21 A–D once and then tap Export again.";
            _runtimeBindingDiagnosticsExportResultLabel.TextColor = UIColor.SystemRed;
            if (!automatic && _statusLabel is not null)
            {
                _statusLabel.Text = "STEP 21.1 DIAGNOSTIC EXPORT FAIL — no binding policy was changed; report the export error.";
                _statusLabel.TextColor = UIColor.SystemRed;
            }
        }
    }

    private bool RecordRuntimeFrameworkBindingGate(RuntimeFrameworkBindingGateResult result)
    {
        _runtimeFrameworkBindingGates.Record(result.Gate, result.Passed, result.Detail);
        if (_runtimeFrameworkBindingResultLabel is not null)
        {
            _runtimeFrameworkBindingResultLabel.Text = _runtimeFrameworkBindingGates.Snapshot().Summary;
            _runtimeFrameworkBindingResultLabel.TextColor = result.Passed ? UIColor.Label : UIColor.SystemRed;
        }
        if (_runtimeFrameworkBindingDetailLabel is not null)
            _runtimeFrameworkBindingDetailLabel.Text = FormatRuntimeFrameworkBindingDetail(result.Detail);
        if (!result.Passed && _statusLabel is not null)
        {
            var letter = (char)('A' + (int)result.Gate - 1);
            _statusLabel.Text = $"STEP 21 FAIL at Gate {letter} ({result.Gate}). Stop here; later runtime/framework-binding gates were not run.";
            _statusLabel.TextColor = UIColor.SystemRed;
        }
        return result.Passed;
    }

    private string FormatRuntimeFrameworkBindingDetail(string tail)
    {
        var lines = new List<string>();
        foreach (var gate in _runtimeFrameworkBindingGates.Results)
        {
            var letter = (char)('A' + (int)gate.Gate - 1);
            lines.Add($"Gate {letter} — {gate.Gate}: {(gate.Passed ? "PASS" : "FAIL")}");
            lines.Add(gate.Detail);
            lines.Add(string.Empty);
        }

        lines.Add("Step 21 write scope: launcher-private Step21-PreparedRuntimeBinding/source + prepared + plan only; the Step 12 receipt-backed managed install stays read-only.");
        lines.Add("CLR load scope: iOS host framework contracts only. Real sts2.dll/GodotSharp/game assemblies are inspected with Cecil as data but are never loaded into the CLR in Step 21.");
        lines.Add("Binding policy: prefer a compatible iOS-host framework assembly for System/platform contracts; otherwise resolve only exact/controlled-version identities from the verified ARM64/shared workspace. Missing, ambiguous, lower-version and non-IL-only edges become explicit blockers—never broad fallback.");
        lines.Add("Step 21 4/4 means the plan/prepared set is trustworthy. It does NOT override Runtime closure ready: NO; blockers must be addressed before any first real game CLR load.");
        lines.Add("Steps 01–20 remain closed/protected. Closure requires OfflineReady + Foundation 5/5 after a 4/4 pass.");
        lines.Add("Out of scope: game static initialization/execution, GodotSharp behavioral integration, native game loading, Harmony/MonoMod, FMOD/Spine, Cloud, and Workshop.");
        lines.Add("Step 15 orientation presentation quirk remains a known non-blocking cleanup item.");
        lines.Add(tail);
        return string.Join("\n", lines);
    }

    private bool RecordDynamicManagedExecutionGate(DynamicManagedExecutionGateResult result)
    {
        _dynamicManagedExecutionGates.Record(result.Gate, result.Passed, result.Detail);
        if (_dynamicManagedExecutionResultLabel is not null)
        {
            _dynamicManagedExecutionResultLabel.Text = _dynamicManagedExecutionGates.Snapshot().Summary;
            _dynamicManagedExecutionResultLabel.TextColor = result.Passed ? UIColor.Label : UIColor.SystemRed;
        }
        if (_dynamicManagedExecutionDetailLabel is not null)
            _dynamicManagedExecutionDetailLabel.Text = FormatDynamicManagedExecutionDetail(result.Detail);
        if (!result.Passed && _statusLabel is not null)
        {
            var letter = (char)('A' + (int)result.Gate - 1);
            _statusLabel.Text = $"STEP 20 FAIL at Gate {letter} ({result.Gate}). Stop here; later dynamic-managed-execution gates were not run.";
            _statusLabel.TextColor = UIColor.SystemRed;
        }
        return result.Passed;
    }

    private string FormatDynamicManagedExecutionDetail(string tail)
    {
        var lines = new List<string>();
        foreach (var gate in _dynamicManagedExecutionGates.Results)
        {
            var letter = (char)('A' + (int)gate.Gate - 1);
            lines.Add($"Gate {letter} — {gate.Gate}: {(gate.Passed ? "PASS" : "FAIL")}");
            lines.Add(gate.Detail);
            lines.Add(string.Empty);
        }

        lines.Add("Step 20 write scope: launcher-private Step20-DynamicManagedExecution/fixtures only; the Step 12 receipt-backed managed install stays read-only.");
        lines.Add("Dynamic execution scope: project-owned exact-hash fixtures only. AssemblyLoadContext/reflective invocation are intentionally permitted here solely to prove non-AOT IL execution and one controlled private dependency hop.");
        lines.Add("Out of scope: Assembly.Load/AssemblyLoadContext for sts2.dll or any game assembly, game static initialization, GodotSharp binding, native game integration, Harmony/MonoMod, FMOD/Spine, Cloud, and Workshop.");
        lines.Add("Steps 01–19 remain closed/protected. Step 20 retains AOT for build-time assemblies while adding interpreter availability for runtime/dynamic managed code; closure therefore requires OfflineReady + Foundation 5/5 after a 4/4 pass.");
        lines.Add("Step 15 orientation presentation quirk remains a known non-blocking cleanup item.");
        lines.Add(tail);
        return string.Join("\n", lines);
    }

    private bool RecordExpressionInterpreterCompatibilityGate(ExpressionInterpreterCompatibilityGateResult result)
    {
        _expressionInterpreterCompatibilityGates.Record(result.Gate, result.Passed, result.Detail);
        if (_expressionInterpreterCompatibilityResultLabel is not null)
        {
            _expressionInterpreterCompatibilityResultLabel.Text = _expressionInterpreterCompatibilityGates.Snapshot().Summary;
            _expressionInterpreterCompatibilityResultLabel.TextColor = result.Passed ? UIColor.Label : UIColor.SystemRed;
        }
        if (_expressionInterpreterCompatibilityDetailLabel is not null)
            _expressionInterpreterCompatibilityDetailLabel.Text = FormatExpressionInterpreterCompatibilityDetail(result.Detail);
        if (!result.Passed && _statusLabel is not null)
        {
            var letter = (char)('A' + (int)result.Gate - 1);
            _statusLabel.Text = $"STEP 19.2 FAIL at Gate {letter} ({result.Gate}). Stop here; later expression-interpreter compatibility gates were not run.";
            _statusLabel.TextColor = UIColor.SystemRed;
        }
        return result.Passed;
    }

    private string FormatExpressionInterpreterCompatibilityDetail(string tail)
    {
        var lines = new List<string>();
        foreach (var gate in _expressionInterpreterCompatibilityGates.Results)
        {
            var letter = (char)('A' + (int)gate.Gate - 1);
            lines.Add($"Gate {letter} — {gate.Gate}: {(gate.Passed ? "PASS" : "FAIL")}");
            lines.Add(gate.Detail);
            lines.Add(string.Empty);
        }

        lines.Add("Step 19 write scope: launcher-private Step19-ExpressionInterpreterCompatibility/source + prepared only; the Step 12 receipt-backed managed install stays read-only.");
        lines.Add("Behavioral rewrite scope: NONE in Step 19.2. Gate A proves host Compile()/Compile(false)/Compile(true) fallback behavior; Gate B read-only classifies real call sites; Gate C performs zero Cecil assembly writes and makes the prepared tree byte-identical. System.* framework and non-IL-only/ReadyToRun images are diagnostic-only.");
        lines.Add("Out of scope: mutating any copied expression call site or desktop framework image, framework substitution/binding for actual game execution, Harmony/MonoMod runtime detours, Reflection.Emit replacement, Assembly.Load, native runtime integration, StS2 execution, Cloud, and Workshop.");
        lines.Add("Step 18 remains closed/protected; its verified-workspace resolver principles are preserved in Step 19.");
        lines.Add("Step 15 orientation presentation quirk remains a known non-blocking cleanup item.");
        lines.Add(tail);
        return string.Join("\n", lines);
    }

    private bool RecordCompatibilityCallSiteGate(CompatibilityCallSiteGateResult result)
    {
        _compatibilityCallSiteGates.Record(result.Gate, result.Passed, result.Detail);
        if (_compatibilityCallSiteResultLabel is not null)
        {
            _compatibilityCallSiteResultLabel.Text = _compatibilityCallSiteGates.Snapshot().Summary;
            _compatibilityCallSiteResultLabel.TextColor = result.Passed ? UIColor.Label : UIColor.SystemRed;
        }
        if (_compatibilityCallSiteDetailLabel is not null)
            _compatibilityCallSiteDetailLabel.Text = FormatCompatibilityCallSiteDetail(result.Detail);
        if (!result.Passed && _statusLabel is not null)
        {
            var letter = (char)('A' + (int)result.Gate - 1);
            _statusLabel.Text = $"STEP 17 FAIL at Gate {letter} ({result.Gate}). Stop here; later compatibility-analysis gates were not run.";
            _statusLabel.TextColor = UIColor.SystemRed;
        }
        return result.Passed;
    }

    private string FormatCompatibilityCallSiteDetail(string tail)
    {
        var lines = new List<string>();
        foreach (var gate in _compatibilityCallSiteGates.Results)
        {
            var letter = (char)('A' + (int)gate.Gate - 1);
            lines.Add($"Gate {letter} — {gate.Gate}: {(gate.Passed ? "PASS" : "FAIL")}");
            lines.Add(gate.Detail);
            lines.Add(string.Empty);
        }

        lines.Add("Step 17 scope: receipt-backed macOS arm64 + architecture-neutral managed files only; x86_64 duplicate managed payload excluded from compatibility prioritization.");
        lines.Add("Evidence: actual Cecil IL operands/PInvoke metadata; no dependency Resolve(), Assembly.Load, game execution, or game-file write.");
        lines.Add("Step 15 orientation presentation quirk remains a known non-blocking cleanup item; Step 17 does not alter the Godot host.");
        lines.Add("Real compatibility rewrite / StS2 execution / FMOD / Spine / Cloud / Workshop: NOT ADVANCED BY STEP 17");
        lines.Add(tail);
        return string.Join("\n", lines);
    }

    private bool RecordManagedPreparationGate(ManagedPreparationGateResult result)
    {
        _managedPreparationGates.Record(result.Gate, result.Passed, result.Detail);
        if (_managedPreparationResultLabel is not null)
        {
            _managedPreparationResultLabel.Text = _managedPreparationGates.Snapshot().Summary;
            _managedPreparationResultLabel.TextColor = result.Passed ? UIColor.Label : UIColor.SystemRed;
        }
        if (_managedPreparationDetailLabel is not null)
            _managedPreparationDetailLabel.Text = FormatManagedPreparationDetail(result.Detail);
        if (!result.Passed && _statusLabel is not null)
        {
            var letter = (char)('A' + (int)result.Gate - 1);
            _statusLabel.Text = $"STEP 16 FAIL at Gate {letter} ({result.Gate}). Stop here; later managed-preparation gates were not run.";
            _statusLabel.TextColor = UIColor.SystemRed;
        }
        return result.Passed;
    }

    private string FormatManagedPreparationDetail(string tail)
    {
        var lines = new List<string>();
        foreach (var gate in _managedPreparationGates.Results)
        {
            var letter = (char)('A' + (int)gate.Gate - 1);
            lines.Add($"Gate {letter} — {gate.Gate}: {(gate.Passed ? "PASS" : "FAIL")}\n{gate.Detail}");
        }

        lines.Add("Step 16 write scope: project-owned fixture copies under launcher-private Step16-ManagedPreparation only.");
        lines.Add("Real StS2 gate: receipt-backed metadata read only; no Assembly.Load, no game execution, no game-file write.");
        lines.Add("Godot/game-runtime/FMOD/Spine/Cloud/Workshop integration: NOT ADVANCED BY STEP 16");
        lines.Add(tail);
        return string.Join("\n\n", lines);
    }

    private async Task<bool> WaitForGodotConditionAsync(Func<bool> condition, TimeSpan timeout)
    {
        var deadline = DateTimeOffset.UtcNow + timeout;
        while (DateTimeOffset.UtcNow < deadline)
        {
            if (EvaluateGodotConditionOnMainThread(condition))
                return true;
            await Task.Delay(100);
        }
        return EvaluateGodotConditionOnMainThread(condition);
    }

    private bool EvaluateGodotConditionOnMainThread(Func<bool> condition)
    {
        if (NSThread.IsMain)
            return condition();

        var result = false;
        InvokeOnMainThread(() => result = condition());
        return result;
    }

    private async Task PrepareRepairTestAsync()
    {
        if (_managedInstallResultLabel is null || _managedInstallDetailLabel is null || _statusLabel is null)
            return;
        try
        {
            var relative = await _managedInstallAttempt.PrepareRepairTestAsync();
            _managedInstallResultLabel.Text = "REPAIR TEST PREPARED";
            _managedInstallResultLabel.TextColor = UIColor.SystemOrange;
            _managedInstallDetailLabel.Text = $"Intentionally changed one local byte in managed file: {relative}\nRun Inspect + Install / Update / Repair now. It must report StateBefore=RepairNeeded and finish REPAIR PASS.";
            _statusLabel.Text = "Repair test prepared locally; no Steam credential/content request was made by the test helper.";
            _statusLabel.TextColor = UIColor.SystemOrange;
        }
        catch (Exception ex)
        {
            _managedInstallResultLabel.Text = "REPAIR TEST PREP FAILED";
            _managedInstallResultLabel.TextColor = UIColor.SystemRed;
            _managedInstallDetailLabel.Text = $"{ex.GetType().Name}: {ex.Message}";
        }
    }

    private async Task PrepareUpdateStateTestAsync()
    {
        if (_managedInstallResultLabel is null || _managedInstallDetailLabel is null || _statusLabel is null)
            return;
        try
        {
            var simulatedManifest = await _managedInstallAttempt.PrepareUpdateStateTestAsync();
            _managedInstallResultLabel.Text = "UPDATE-STATE TEST PREPARED";
            _managedInstallResultLabel.TextColor = UIColor.SystemOrange;
            _managedInstallDetailLabel.Text = $"Changed only the project-owned local install receipt: stale manifest ID {simulatedManifest} plus one synthetic changed-file SHA-1 identity. Actual game files were not modified. Run Inspect + Install / Update / Repair now. It must report StateBefore=UpdateAvailable, reverify the existing Step 11 cache against Steam, replace at least one file from that source, and finish UPDATE PASS using Steam's actual current public manifest.";
            _statusLabel.Text = "Update test prepared locally; the next manager run must prove the real update path from current Steam metadata without needlessly redownloading an already-valid current-manifest cache.";
            _statusLabel.TextColor = UIColor.SystemOrange;
        }
        catch (Exception ex)
        {
            _managedInstallResultLabel.Text = "UPDATE TEST PREP FAILED";
            _managedInstallResultLabel.TextColor = UIColor.SystemRed;
            _managedInstallDetailLabel.Text = $"{ex.GetType().Name}: {ex.Message}";
        }
    }

    private async Task ClearDownloadCacheAsync()
    {
        if (_managedInstallResultLabel is null || _managedInstallDetailLabel is null || _statusLabel is null)
            return;

        BeginSteamOperation(allowCancel: false);
        try
        {
            _managedInstallResultLabel.Text = "DOWNLOAD CACHE: CLEARING…";
            _managedInstallResultLabel.TextColor = UIColor.Label;
            _managedInstallDetailLabel.Text = "Deleting only the project-owned Step 11 download cache. The managed install and saved Steam session are not touched.";

            var result = await Task.Run(_downloadCacheMaintenance.Clear);
            _managedInstallResultLabel.Text = result.CacheExisted ? "DOWNLOAD CACHE: CLEARED" : "DOWNLOAD CACHE: ALREADY EMPTY";
            _managedInstallResultLabel.TextColor = UIColor.Label;
            _managedInstallDetailLabel.Text =
                $"Cache path: {result.CacheRelativePath}\n" +
                $"Cache existed: {YesNo(result.CacheExisted)}\n" +
                $"Cache absent now: {YesNo(result.CacheAbsentAfterClear)}\n" +
                "Managed Step 12 install: PRESERVED\nSaved Steam session: PRESERVED";
            _statusLabel.Text = "PASS: Step 11 download cache is absent. A normal UpToDate manager run may still no-op; use Prepare Fresh Download Test when you specifically want to force CDN acquisition.";
            _statusLabel.TextColor = UIColor.Label;
        }
        catch (Exception ex)
        {
            _managedInstallResultLabel.Text = "DOWNLOAD CACHE: CLEAR FAILED";
            _managedInstallResultLabel.TextColor = UIColor.SystemRed;
            _managedInstallDetailLabel.Text = $"{ex.GetType().Name}: {ex.Message}";
            _statusLabel.Text = "FAIL: Step 11 cache clear did not complete. Managed install/session were not intentionally modified by this control.";
            _statusLabel.TextColor = UIColor.SystemRed;
        }
        finally
        {
            EndSteamOperation();
        }
    }

    private async Task PrepareFreshDownloadTestAsync()
    {
        if (_managedInstallResultLabel is null || _managedInstallDetailLabel is null || _statusLabel is null)
            return;

        BeginSteamOperation(allowCancel: false);
        try
        {
            _managedInstallResultLabel.Text = "FRESH DOWNLOAD TEST: PREPARING…";
            _managedInstallResultLabel.TextColor = UIColor.Label;
            _managedInstallDetailLabel.Text = "Preparing the existing synthetic UpdateAvailable receipt, then deleting only the Step 11 source cache.";

            var simulatedManifest = await _managedInstallAttempt.PrepareUpdateStateTestAsync();
            var clearResult = await Task.Run(_downloadCacheMaintenance.Clear);

            _managedInstallResultLabel.Text = "FRESH DOWNLOAD TEST PREPARED";
            _managedInstallResultLabel.TextColor = UIColor.SystemOrange;
            _managedInstallDetailLabel.Text =
                $"Synthetic stale receipt manifest: {simulatedManifest}\n" +
                $"Download cache existed: {YesNo(clearResult.CacheExisted)}\n" +
                $"Download cache absent now: {YesNo(clearResult.CacheAbsentAfterClear)}\n" +
                "Managed game files: UNCHANGED\nSaved Steam session: PRESERVED\n\n" +
                "Now tap Inspect + Install / Update / Repair. It must report StateBefore=UpdateAvailable, reacquire the current public depot from Steam because no Step 11 cache exists, verify the full source, replace at least the synthetic changed-file identity, atomically commit, and finish UPDATE PASS / UpToDate.";
            _statusLabel.Text = "Fresh-download regression prepared. The next manager run is expected to transfer the current depot from Steam; do not clear/prepare again until that run completes or is deliberately cancelled.";
            _statusLabel.TextColor = UIColor.SystemOrange;
        }
        catch (Exception ex)
        {
            _managedInstallResultLabel.Text = "FRESH DOWNLOAD TEST PREP FAILED";
            _managedInstallResultLabel.TextColor = UIColor.SystemRed;
            _managedInstallDetailLabel.Text = $"{ex.GetType().Name}: {ex.Message}";
            _statusLabel.Text = "Fresh-download test preparation did not complete. If the receipt was already made stale before cache deletion failed, the normal manager can safely reconcile it using the existing verified source cache.";
            _statusLabel.TextColor = UIColor.SystemRed;
        }
        finally
        {
            EndSteamOperation();
        }
    }

    private void ClearSavedSession()
    {
        if (_statusLabel is null)
            return;

        try
        {
            var existed = _sessionStore.Clear();
            var stillExists = _sessionStore.Load() is not null;

            if (stillExists)
            {
                _statusLabel.Text = "FAIL: Keychain clear returned but the saved Steam session is still present.";
                _statusLabel.TextColor = UIColor.SystemRed;
            }
            else
            {
                _statusLabel.Text = existed
                    ? "PASS: signed out locally — saved Steam refresh token and identity were removed from Keychain."
                    : "PASS: no saved Steam session existed; Keychain remains clear.";
                _statusLabel.TextColor = UIColor.Label;
            }

            if (_resumeResultLabel is not null)
            {
                _resumeResultLabel.Text = "SAVED SESSION — cleared";
                _resumeResultLabel.TextColor = UIColor.Label;
            }

            if (_resumeDetailLabel is not null)
                _resumeDetailLabel.Text = "Relaunching now should automatically report AUTO SESSION — signed out because no saved Steam session is available.";
        }
        catch (Exception ex)
        {
            _statusLabel.Text = $"FAIL: saved-session clear error: {ex.GetType().Name}: {ex.Message}";
            _statusLabel.TextColor = UIColor.SystemRed;
        }

        RefreshSavedSessionStatus();
    }

    private void RefreshSavedSessionStatus()
    {
        if (_savedSessionLabel is null)
            return;

        try
        {
            var saved = _sessionStore.Load();
            if (saved is null)
            {
                _savedSessionLabel.Text = "Saved session: NONE";
                _savedSessionLabel.TextColor = UIColor.SecondaryLabel;
                return;
            }

            var tokenTiming = SteamRefreshTokenMetadata.TryParse(saved.RefreshToken, out var metadata) &&
                              metadata is not null
                ? $"\nRefresh token expires (UTC): {FormatUtc(metadata.ExpiresAtUtc)}" +
                  $"\nRefresh token expired now: {YesNo(metadata.IsExpiredAt(DateTimeOffset.UtcNow))}"
                : "\nRefresh token timing: unavailable";

            _savedSessionLabel.Text =
                $"Saved session: YES\nAccount: {saved.AccountName}\nSteamID64: {saved.SteamId64}\nRefresh token: PRESENT (not displayed){tokenTiming}";
            _savedSessionLabel.TextColor = UIColor.Label;
        }
        catch (Exception ex)
        {
            _savedSessionLabel.Text = $"Saved session: INVALID — {ex.GetType().Name}: {ex.Message}";
            _savedSessionLabel.TextColor = UIColor.SystemRed;
        }
    }

    private void BeginSteamOperation(bool allowCancel = true)
    {
        _operationCts?.Dispose();
        _operationCts = new CancellationTokenSource();

        if (_foundationButton is not null) _foundationButton.Enabled = false;
        if (_authButton is not null) _authButton.Enabled = false;
        if (_resumeButton is not null) _resumeButton.Enabled = false;
        if (_ownershipButton is not null) _ownershipButton.Enabled = false;
        if (_discoveryButton is not null) _discoveryButton.Enabled = false;
        if (_singleFileButton is not null) _singleFileButton.Enabled = false;
        if (_fullDepotButton is not null) _fullDepotButton.Enabled = false;
        if (_resumableDepotButton is not null) _resumableDepotButton.Enabled = false;
        if (_managedInstallButton is not null) _managedInstallButton.Enabled = false;
        if (_prepareRepairTestButton is not null) _prepareRepairTestButton.Enabled = false;
        if (_prepareUpdateTestButton is not null) _prepareUpdateTestButton.Enabled = false;
        if (_clearDownloadCacheButton is not null) _clearDownloadCacheButton.Enabled = false;
        if (_prepareFreshDownloadTestButton is not null) _prepareFreshDownloadTestButton.Enabled = false;
        if (_offlineInstallButton is not null) _offlineInstallButton.Enabled = false;
        if (_compatibilityInventoryButton is not null) _compatibilityInventoryButton.Enabled = false;
        if (_godotFoundationStartButton is not null) _godotFoundationStartButton.Enabled = false;
        if (_godotFoundationGateDButton is not null) _godotFoundationGateDButton.Enabled = false;
        if (_managedPreparationButton is not null) _managedPreparationButton.Enabled = false;
        if (_compatibilityCallSiteButton is not null) _compatibilityCallSiteButton.Enabled = false;
        if (_realAssemblyRewriteButton is not null) _realAssemblyRewriteButton.Enabled = false;
        if (_expressionInterpreterCompatibilityButton is not null) _expressionInterpreterCompatibilityButton.Enabled = false;
        if (_dynamicManagedExecutionButton is not null) _dynamicManagedExecutionButton.Enabled = false;
        if (_signOutButton is not null) _signOutButton.Enabled = false;
        if (_cancelOperationButton is not null) _cancelOperationButton.Enabled = allowCancel;
    }

    private void EndSteamOperation()
    {
        var normalControlsEnabled = !_godotProcessRequiresRestart;
        if (_foundationButton is not null) _foundationButton.Enabled = normalControlsEnabled;
        if (_authButton is not null) _authButton.Enabled = normalControlsEnabled;
        if (_resumeButton is not null) _resumeButton.Enabled = normalControlsEnabled;
        if (_ownershipButton is not null) _ownershipButton.Enabled = normalControlsEnabled;
        if (_discoveryButton is not null) _discoveryButton.Enabled = normalControlsEnabled;
        if (_singleFileButton is not null) _singleFileButton.Enabled = normalControlsEnabled;
        if (_fullDepotButton is not null) _fullDepotButton.Enabled = normalControlsEnabled;
        if (_resumableDepotButton is not null) _resumableDepotButton.Enabled = normalControlsEnabled;
        if (_managedInstallButton is not null) _managedInstallButton.Enabled = normalControlsEnabled;
        if (_prepareRepairTestButton is not null) _prepareRepairTestButton.Enabled = normalControlsEnabled;
        if (_prepareUpdateTestButton is not null) _prepareUpdateTestButton.Enabled = normalControlsEnabled;
        if (_clearDownloadCacheButton is not null) _clearDownloadCacheButton.Enabled = normalControlsEnabled;
        if (_prepareFreshDownloadTestButton is not null) _prepareFreshDownloadTestButton.Enabled = normalControlsEnabled;
        if (_offlineInstallButton is not null) _offlineInstallButton.Enabled = normalControlsEnabled;
        if (_compatibilityInventoryButton is not null) _compatibilityInventoryButton.Enabled = normalControlsEnabled;
        if (_godotFoundationStartButton is not null) _godotFoundationStartButton.Enabled = !_godotProcessRequiresRestart;
        if (_godotFoundationGateDButton is not null)
        {
            var snapshot = _godotFoundationGates.Snapshot();
            _godotFoundationGateDButton.Enabled = _godotSessionStarted &&
                snapshot.FirstFailingGate is null &&
                snapshot.Results.Count == 3;
        }
        if (_managedPreparationButton is not null) _managedPreparationButton.Enabled = normalControlsEnabled;
        if (_compatibilityCallSiteButton is not null) _compatibilityCallSiteButton.Enabled = normalControlsEnabled;
        if (_realAssemblyRewriteButton is not null) _realAssemblyRewriteButton.Enabled = normalControlsEnabled;
        if (_expressionInterpreterCompatibilityButton is not null) _expressionInterpreterCompatibilityButton.Enabled = normalControlsEnabled;
        if (_dynamicManagedExecutionButton is not null) _dynamicManagedExecutionButton.Enabled = normalControlsEnabled;
        if (_signOutButton is not null) _signOutButton.Enabled = normalControlsEnabled;
        if (_cancelOperationButton is not null) _cancelOperationButton.Enabled = false;
    }

    private static string FormatAuthDetail(SteamAuthenticationResult result)
    {
        var lines = new List<string>
        {
            $"CM connected: {YesNo(result.CmConnected)}",
            $"Auth session started: {YesNo(result.AuthSessionStarted)}",
            $"Persistent auth requested: YES",
            $"Mobile approval requested: {YesNo(result.MobileApprovalRequested)}",
            $"Mobile approval completed: {YesNo(result.MobileApprovalCompleted)}",
            $"LoggedOnCallback: {YesNo(result.LoggedOnCallbackReceived)}",
            $"Logon result: {result.LogonResult?.ToString() ?? "N/A"}",
            $"Extended result: {result.ExtendedLogonResult?.ToString() ?? "N/A"}",
            $"ShouldRememberPassword: YES",
            $"LoginID: {result.LoginId?.ToString() ?? "not-set"}",
            $"Refresh token expires (UTC): {FormatUtc(result.RefreshTokenExpiresAtUtc)}",
            $"Refresh token expired at attempt: {YesNoNullable(result.RefreshTokenExpiredAtAttempt)}",
            $"Session persisted to Keychain: {YesNo(result.SessionPersisted)}",
            $"Account name: {result.AccountName ?? "not-returned"}",
            $"SteamID64: {result.SteamId64 ?? "not-returned"}",
            $"CurrentEndPoint: {result.CurrentEndPoint ?? "never-set"}",
            $"Elapsed: {result.Elapsed.TotalSeconds:F1}s",
        };

        if (result.GuardChallenge is not null)
        {
            lines.Add($"Guard type: {result.GuardChallenge.Kind}");
            if (!string.IsNullOrWhiteSpace(result.GuardChallenge.AssociatedMessage))
                lines.Add($"Guard detail: {result.GuardChallenge.AssociatedMessage}");
        }

        if (!string.IsNullOrWhiteSpace(result.Error))
            lines.Add($"Error: {result.Error}");

        lines.Add("Password persistence: NONE");
        lines.Add("Steam Guard secret/code persistence: NONE");
        lines.Add("Refresh token display/logging: NONE");
        lines.Add("Ownership request in this operation: NOT RUN");
        return string.Join("\n", lines);
    }

    private static string FormatResumeDetail(SteamSessionResumeResult result)
    {
        var recoveryAction = SteamSessionRecoveryPolicy.Evaluate(result);
        var lines = new List<string>
        {
            $"Outcome: {result.Outcome}",
            $"Recovery action: {recoveryAction}",
            $"Saved session found: {YesNo(result.SavedSessionFound)}",
            $"CM connected: {YesNo(result.CmConnected)}",
            $"LoggedOnCallback: {YesNo(result.LoggedOnCallbackReceived)}",
            $"Logon result: {result.LogonResult?.ToString() ?? "N/A"}",
            $"Extended result: {result.ExtendedLogonResult?.ToString() ?? "N/A"}",
            $"ShouldRememberPassword: YES",
            $"LoginID: {result.LoginId?.ToString() ?? "not-set"}",
            $"Refresh token expires (UTC): {FormatUtc(result.RefreshTokenExpiresAtUtc)}",
            $"Refresh token expired at attempt: {YesNoNullable(result.RefreshTokenExpiredAtAttempt)}",
            $"Stored/returned identity match: {YesNo(result.IdentityMatched)}",
            $"Account name: {result.AccountName ?? "not-returned"}",
            $"SteamID64: {result.SteamId64 ?? "not-returned"}",
            $"CurrentEndPoint: {result.CurrentEndPoint ?? "never-set"}",
            $"Elapsed: {result.Elapsed.TotalSeconds:F1}s",
        };

        if (!string.IsNullOrWhiteSpace(result.Error))
            lines.Add($"Error: {result.Error}");

        lines.Add("Password used: NO");
        lines.Add("New Steam Guard approval requested by launcher: NO");
        lines.Add("Refresh token display/logging: NONE");
        lines.Add("Ownership request in this operation: NOT RUN");
        return string.Join("\n", lines);
    }

    private static string FormatOwnershipDetail(SteamOwnershipVerificationResult result)
    {
        var lines = new List<string>
        {
            $"Target AppID: {result.TargetAppId}",
            $"Saved session found: {YesNo(result.SavedSessionFound)}",
            $"CM connected: {YesNo(result.CmConnected)}",
            $"LoggedOnCallback: {YesNo(result.LoggedOnCallbackReceived)}",
            $"Logon result: {result.LogonResult?.ToString() ?? "N/A"}",
            $"Extended result: {result.ExtendedLogonResult?.ToString() ?? "N/A"}",
            $"Stored/returned identity match: {YesNo(result.IdentityMatched)}",
            $"Ownership callback: {YesNo(result.OwnershipTicketCallbackReceived)}",
            $"Ownership result: {result.OwnershipResult?.ToString() ?? "N/A"}",
            $"Ownership callback AppID: {result.OwnershipAppId?.ToString() ?? "N/A"}",
            $"Ownership ticket bytes: {result.OwnershipTicketLength}",
            $"Ownership proven: {YesNo(result.OwnershipProven)}",
            $"Account name: {result.AccountName ?? "not-returned"}",
            $"SteamID64: {result.SteamId64 ?? "not-returned"}",
            $"LoginID: {result.LoginId?.ToString() ?? "not-set"}",
            $"CurrentEndPoint: {result.CurrentEndPoint ?? "never-set"}",
            $"Elapsed: {result.Elapsed.TotalSeconds:F1}s",
        };

        if (!string.IsNullOrWhiteSpace(result.Error))
            lines.Add($"Error: {result.Error}");

        lines.Add("Ownership ticket payload display/logging/persistence: NONE");
        lines.Add("PICS request: NOT RUN");
        lines.Add("Depot/manifest/CDN/download request: NOT RUN");
        return string.Join("\n", lines);
    }

    private static string FormatContentDiscoveryDetail(SteamContentDiscoveryResult result)
    {
        var lines = new List<string>
        {
            $"Target AppID: {result.TargetAppId}",
            $"Saved session found: {YesNo(result.SavedSessionFound)}",
            $"CM connected: {YesNo(result.CmConnected)}",
            $"LoggedOnCallback: {YesNo(result.LoggedOnCallbackReceived)}",
            $"Logon result: {result.LogonResult?.ToString() ?? "N/A"}",
            $"Extended result: {result.ExtendedLogonResult?.ToString() ?? "N/A"}",
            $"Stored/returned identity match: {YesNo(result.IdentityMatched)}",
            $"Step 07 ownership callback: {YesNo(result.OwnershipTicketCallbackReceived)}",
            $"Step 07 ownership result: {result.OwnershipResult?.ToString() ?? "N/A"}",
            $"Step 07 ownership ticket bytes: {result.OwnershipTicketLength}",
            $"Step 07 ownership re-proven: {YesNo(result.OwnershipProven)}",
            $"PICS access-token callback: {YesNo(result.PicsAccessTokenCallbackReceived)}",
            $"PICS app access token returned: {YesNo(result.PicsAccessTokenReceived)} (value never exposed)",
            $"PICS product-info callback: {YesNo(result.PicsProductInfoCallbackReceived)}",
            $"PICS target app found: {YesNo(result.PicsAppInfoFound)}",
            $"PICS reports missing token: {YesNo(result.PicsMissingToken)}",
            $"PICS change number: {result.PicsChangeNumber?.ToString() ?? "N/A"}",
            $"Depot count: {result.DepotCount}",
            $"Visible branch manifest count: {result.ManifestCount}",
            $"Account name: {result.AccountName ?? "not-returned"}",
            $"SteamID64: {result.SteamId64 ?? "not-returned"}",
            $"LoginID: {result.LoginId?.ToString() ?? "not-set"}",
            $"CurrentEndPoint: {result.CurrentEndPoint ?? "never-set"}",
            $"Elapsed: {result.Elapsed.TotalSeconds:F1}s",
        };

        foreach (var depot in result.Depots)
        {
            var platform = new List<string>();
            if (!string.IsNullOrWhiteSpace(depot.OsList)) platform.Add($"oslist={depot.OsList}");
            if (!string.IsNullOrWhiteSpace(depot.OsArch)) platform.Add($"osarch={depot.OsArch}");
            if (!string.IsNullOrWhiteSpace(depot.Language)) platform.Add($"language={depot.Language}");

            lines.Add($"Depot {depot.DepotId}{(platform.Count == 0 ? string.Empty : " — " + string.Join(", ", platform))}");
            foreach (var manifest in depot.Manifests)
                lines.Add($"  {manifest.Branch}: {manifest.ManifestId}");
        }

        if (!string.IsNullOrWhiteSpace(result.Error))
            lines.Add($"Error: {result.Error}");

        lines.Add("Ownership ticket payload display/logging/persistence: NONE");
        lines.Add("PICS access-token value display/logging/persistence: NONE");
        lines.Add("Depot decryption key request: NOT RUN");
        lines.Add("Manifest body request: NOT RUN");
        lines.Add("CDN server/token/chunk/file request: NOT RUN");
        return string.Join("\n", lines);
    }

    private static string FormatSingleFileDownloadDetail(SteamSingleFileDownloadResult result)
    {
        var lines = new List<string>
        {
            $"Target AppID: {result.TargetAppId}",
            $"Saved session found: {YesNo(result.SavedSessionFound)}",
            $"CM connected: {YesNo(result.CmConnected)}",
            $"LoggedOnCallback: {YesNo(result.LoggedOnCallbackReceived)}",
            $"Logon result: {result.LogonResult?.ToString() ?? "N/A"}",
            $"Stored/returned identity match: {YesNo(result.IdentityMatched)}",
            $"Step 07 ownership callback: {YesNo(result.OwnershipTicketCallbackReceived)}",
            $"Step 07 ownership result: {result.OwnershipResult?.ToString() ?? "N/A"}",
            $"Step 07 ownership ticket bytes: {result.OwnershipTicketLength}",
            $"Step 07 ownership re-proven: {YesNo(result.OwnershipProven)}",
            $"Step 08 PICS access-token callback: {YesNo(result.PicsAccessTokenCallbackReceived)}",
            $"Step 08 PICS access token returned: {YesNo(result.PicsAccessTokenReceived)} (value never exposed)",
            $"Step 08 PICS product-info callback: {YesNo(result.PicsProductInfoCallbackReceived)}",
            $"Step 08 target app found: {YesNo(result.PicsAppInfoFound)}",
            $"Step 08 PICS reports missing token: {YesNo(result.PicsMissingToken)}",
            $"Selected depot: {result.SelectedDepotId?.ToString() ?? "N/A"}",
            $"Selected depot oslist: {result.SelectedDepotOsList ?? "not-specified"}",
            $"Selected branch: {result.SelectedBranch ?? "N/A"}",
            $"Selected manifest ID: {result.SelectedManifestId?.ToString() ?? "N/A"}",
            $"Depot key requested: {YesNo(result.DepotKeyRequested)}",
            $"Depot key result: {result.DepotKeyResult?.ToString() ?? "N/A"}",
            $"Depot key received: {YesNo(result.DepotKeyReceived)} (value never exposed)",
            $"Manifest request code requested: {YesNo(result.ManifestRequestCodeRequested)}",
            $"Manifest request code received: {YesNo(result.ManifestRequestCodeReceived)} (value never exposed)",
            $"Eligible CDN servers: {result.EligibleCdnServerCount}",
            $"Manifest downloaded: {YesNo(result.ManifestDownloaded)} (in memory only)",
            $"Selected file: {result.SelectedFileName ?? "N/A"}",
            $"Selected file bytes: {result.SelectedFileBytes}",
            $"Selected file chunks: {result.SelectedFileChunkCount}",
            $"Chunks downloaded: {result.ChunksDownloaded}",
            $"Downloaded uncompressed bytes: {result.DownloadedUncompressedBytes}",
            $"CDN auth token requested after 403: {YesNo(result.CdnAuthTokenRequested)}",
            $"CDN auth token received: {YesNo(result.CdnAuthTokenReceived)} (value never exposed)",
            $"File SHA-1 matches manifest: {YesNo(result.FileHashMatched)}",
            $"Final verified file written: {YesNo(result.FileWritten)}",
            $"Output relative path: {result.OutputRelativePath ?? "not-written"}",
            $"Account name: {result.AccountName ?? "not-returned"}",
            $"SteamID64: {result.SteamId64 ?? "not-returned"}",
            $"Elapsed: {result.Elapsed.TotalSeconds:F1}s",
        };

        if (!string.IsNullOrWhiteSpace(result.Error))
            lines.Add($"Error: {result.Error}");

        lines.Add("Ownership ticket payload display/logging/persistence: NONE");
        lines.Add("PICS access-token value display/logging/persistence: NONE");
        lines.Add("Depot-key value display/logging/persistence: NONE");
        lines.Add("Manifest request-code value display/logging/persistence: NONE");
        lines.Add("CDN auth-token value display/logging/persistence: NONE");
        lines.Add("Manifest body persistence: NONE");
        lines.Add("Chunk cache/partial-file persistence: NONE");
        lines.Add("Full-depot queue: NOT IMPLEMENTED");
        lines.Add("Resume/update/install/repair: NOT IMPLEMENTED");
        return string.Join("\n", lines);
    }

    private static string FormatFullDepotDownloadDetail(SteamFullDepotDownloadResult result)
    {
        var lines = new List<string>
        {
            $"Target AppID: {result.TargetAppId}",
            $"Saved session found: {YesNo(result.SavedSessionFound)}",
            $"CM connected: {YesNo(result.CmConnected)}",
            $"LoggedOnCallback: {YesNo(result.LoggedOnCallbackReceived)}",
            $"Logon result: {result.LogonResult?.ToString() ?? "N/A"}",
            $"Stored/returned identity match: {YesNo(result.IdentityMatched)}",
            $"Step 07 ownership re-proven: {YesNo(result.OwnershipProven)}",
            $"Step 08 PICS access-token callback: {YesNo(result.PicsAccessTokenCallbackReceived)}",
            $"Step 08 PICS product-info callback: {YesNo(result.PicsProductInfoCallbackReceived)}",
            $"Step 08 target app found: {YesNo(result.PicsAppInfoFound)}",
            $"Selected depot: {result.SelectedDepotId?.ToString() ?? "N/A"}",
            $"Selected depot oslist: {result.SelectedDepotOsList ?? "not-specified"}",
            $"Selected branch: {result.SelectedBranch ?? "N/A"}",
            $"Selected manifest ID: {result.SelectedManifestId?.ToString() ?? "N/A"}",
            $"Depot key requested/received: {YesNo(result.DepotKeyRequested)} / {YesNo(result.DepotKeyReceived)}",
            $"Manifest request code requested/received: {YesNo(result.ManifestRequestCodeRequested)} / {YesNo(result.ManifestRequestCodeReceived)}",
            $"Eligible CDN servers: {result.EligibleCdnServerCount}",
            $"Manifest downloaded: {YesNo(result.ManifestDownloaded)} (in memory only)",
            $"Queued files: {result.PlannedFileCount}",
            $"Queued chunks: {result.PlannedChunkCount}",
            $"Queued uncompressed bytes: {result.PlannedBytes}",
            $"Completed files: {result.CompletedFileCount}",
            $"SHA-1 verified files: {result.VerifiedFileCount}",
            $"Downloaded chunks: {result.DownloadedChunkCount}",
            $"Downloaded uncompressed bytes: {result.DownloadedUncompressedBytes}",
            $"CDN auth token requested after 403: {YesNo(result.CdnAuthTokenRequested)}",
            $"CDN auth token received: {YesNo(result.CdnAuthTokenReceived)} (value never exposed)",
            $"Staging directory created: {YesNo(result.StagingDirectoryCreated)}",
            $"Staging directory absent after result: {YesNo(!result.StagingDirectoryCreated || result.StagingDirectoryCleaned || result.FinalDirectoryCommitted)}",
            $"Final directory atomically committed: {YesNo(result.FinalDirectoryCommitted)}",
            $"Output relative path: {result.OutputRelativePath ?? "not-committed"}",
            $"Account name: {result.AccountName ?? "not-returned"}",
            $"SteamID64: {result.SteamId64 ?? "not-returned"}",
            $"Elapsed: {result.Elapsed.TotalSeconds:F1}s",
        };

        if (!string.IsNullOrWhiteSpace(result.Error))
            lines.Add($"Error: {result.Error}");

        lines.Add("Ownership ticket payload display/logging/persistence: NONE");
        lines.Add("PICS access-token value display/logging/persistence: NONE");
        lines.Add("Depot-key value display/logging/persistence: NONE");
        lines.Add("Manifest request-code value display/logging/persistence: NONE");
        lines.Add("CDN auth-token value display/logging/persistence: NONE");
        lines.Add("Manifest body persistence: NONE");
        lines.Add("Chunk cache outside staging: NONE");
        lines.Add("Partial final-depot visibility: NONE — final directory appears only after atomic staging rename");
        lines.Add("Resume: NOT IMPLEMENTED");
        lines.Add("Update/install/repair orchestration: NOT IMPLEMENTED");
        lines.Add("Multi-depot app install: NOT IMPLEMENTED");
        return string.Join("\n", lines);
    }

    private static string FormatResumableDepotDownloadDetail(SteamResumableDepotDownloadResult result)
    {
        var lines = new List<string>
        {
            $"Target AppID: {result.TargetAppId}",
            $"Saved session found: {YesNo(result.SavedSessionFound)}",
            $"CM connected: {YesNo(result.CmConnected)}",
            $"LoggedOnCallback: {YesNo(result.LoggedOnCallbackReceived)}",
            $"Logon result: {result.LogonResult?.ToString() ?? "N/A"}",
            $"Stored/returned identity match: {YesNo(result.IdentityMatched)}",
            $"Step 07 ownership re-proven: {YesNo(result.OwnershipProven)}",
            $"Step 08 PICS access-token callback: {YesNo(result.PicsAccessTokenCallbackReceived)}",
            $"Step 08 PICS product-info callback: {YesNo(result.PicsProductInfoCallbackReceived)}",
            $"Step 08 target app found: {YesNo(result.PicsAppInfoFound)}",
            $"Selected depot: {result.SelectedDepotId?.ToString() ?? "N/A"}",
            $"Selected depot oslist: {result.SelectedDepotOsList ?? "not-specified"}",
            $"Selected branch: {result.SelectedBranch ?? "N/A"}",
            $"Selected manifest ID: {result.SelectedManifestId?.ToString() ?? "N/A"}",
            $"Depot key requested/received: {YesNo(result.DepotKeyRequested)} / {YesNo(result.DepotKeyReceived)}",
            $"Manifest request code requested/received: {YesNo(result.ManifestRequestCodeRequested)} / {YesNo(result.ManifestRequestCodeReceived)}",
            $"Eligible CDN servers: {result.EligibleCdnServerCount}",
            $"Manifest downloaded: {YesNo(result.ManifestDownloaded)} (in memory only)",
            $"Planned files: {result.PlannedFileCount}",
            $"Planned chunks: {result.PlannedChunkCount}",
            $"Planned uncompressed bytes: {result.PlannedBytes}",
            $"Resume staging found at start: {YesNo(result.ResumeStagingFoundAtStart)}",
            $"Resume staging exists/created this run: {YesNo(result.ResumeStagingCreated)}",
            $"Reused fully SHA-1-verified files: {result.ReusedVerifiedFileCount}",
            $"Reused Adler-32-valid chunks: {result.ReusedChunkCount}",
            $"Reused bytes: {result.ReusedBytes}",
            $"Invalid prior resume files discarded: {result.InvalidResumeFileCount}",
            $"Invalid prior resume chunks re-downloaded: {result.InvalidResumeChunkCount}",
            $"New chunks downloaded this run: {result.NewlyDownloadedChunkCount}",
            $"New uncompressed bytes downloaded this run: {result.NewlyDownloadedBytes}",
            $"Satisfied chunks after resume/download: {result.SatisfiedChunkCount}/{result.PlannedChunkCount}",
            $"Satisfied bytes after resume/download: {result.SatisfiedBytes}/{result.PlannedBytes}",
            $"Completed files: {result.CompletedFileCount}/{result.PlannedFileCount}",
            $"SHA-1 verified files: {result.VerifiedFileCount}/{result.PlannedFileCount}",
            $"Resume data preserved after result: {YesNo(result.ResumeDataPreserved)}",
            $"Existing final cache verified against current Steam manifest: {YesNo(result.ExistingFinalVerifiedAgainstManifest)}",
            $"Final directory atomically committed: {YesNo(result.FinalDirectoryCommitted)}",
            $"Resume relative path: {result.ResumeRelativePath ?? "not-created"}",
            $"Output relative path: {result.OutputRelativePath ?? "not-committed"}",
            $"CDN auth token requested after 403: {YesNo(result.CdnAuthTokenRequested)}",
            $"CDN auth token received: {YesNo(result.CdnAuthTokenReceived)} (value never exposed)",
            $"Account name: {result.AccountName ?? "not-returned"}",
            $"SteamID64: {result.SteamId64 ?? "not-returned"}",
            $"Elapsed: {result.Elapsed.TotalSeconds:F1}s",
        };

        if (!string.IsNullOrWhiteSpace(result.Error))
            lines.Add($"Error: {result.Error}");

        lines.Add("Ownership ticket payload display/logging/persistence: NONE");
        lines.Add("PICS access-token value display/logging/persistence: NONE");
        lines.Add("Depot-key value display/logging/persistence: NONE");
        lines.Add("Manifest request-code value display/logging/persistence: NONE");
        lines.Add("CDN auth-token value display/logging/persistence: NONE");
        lines.Add("Manifest body persistence: NONE");
        lines.Add("Resume journal containing Steam secrets: NONE — local files are revalidated directly");
        lines.Add("Partial final-depot visibility: NONE — only deterministic staging persists until atomic commit");
        lines.Add("Manifest delta/update migration: NOT IMPLEMENTED");
        lines.Add("Update/install/repair orchestration: NOT IMPLEMENTED");
        lines.Add("Multi-depot app install: NOT IMPLEMENTED");
        return string.Join("\n", lines);
    }

    private static string FormatOfflineInstallDetail(SteamOfflineInstallResult result)
    {
        var lines = new List<string>
        {
            $"State: {result.State}",
            $"Managed directory found: {YesNo(result.ManagedDirectoryFound)}",
            $"Receipt found: {YesNo(result.ReceiptFound)}",
            $"Receipt structurally valid: {YesNo(result.ReceiptStructurallyValid)}",
            $"Depot: {result.DepotId?.ToString() ?? "N/A"}",
            $"Installed manifest recorded locally: {result.InstalledManifestId?.ToString() ?? "N/A"}",
            $"Branch recorded locally: {result.Branch ?? "N/A"}",
            $"Files verified: {result.VerifiedFiles}/{result.PlannedFiles}",
            $"Bytes verified: {result.VerifiedBytes}/{result.PlannedBytes}",
            $"Exact managed tree verified: {YesNo(result.ExactManagedTreeVerified)}",
            $"Steam session consulted: {YesNo(result.SteamSessionConsulted)}",
            $"Network access attempted by Step 13 check: {YesNo(result.NetworkAccessAttempted)}",
            $"Online manifest freshness known: {YesNo(result.OnlineManifestFreshnessKnown)}",
            $"Managed install: {result.ManagedInstallRelativePath ?? "N/A"}",
            $"Elapsed: {result.Elapsed.TotalSeconds:F1}s",
            "Game launch / compatibility preparation: NOT IMPLEMENTED",
        };

        if (!string.IsNullOrWhiteSpace(result.Error))
            lines.Add($"Error: {result.Error}");

        return string.Join("\n", lines);
    }

    private static string FormatCompatibilityInventoryDetail(SteamCompatibilityInventoryResult result)
    {
        var lines = new List<string>
        {
            $"Target AppID: {result.TargetAppId}",
            $"Depot: {result.DepotId?.ToString() ?? "N/A"}",
            $"Installed manifest recorded locally: {result.InstalledManifestId?.ToString() ?? "N/A"}",
            $"Branch recorded locally: {result.Branch ?? "N/A"}",
            $"OfflineReady precondition re-proven: {YesNo(result.OfflineReadyPreconditionVerified)}",
            $"Total installed files/bytes: {result.TotalFiles} / {result.TotalBytes}",
            $"Asset files/bytes: {result.AssetFiles} / {result.AssetBytes}",
            $"Godot content files: {result.GodotContentFiles}",
            $"Managed assemblies: {result.ManagedAssemblyFiles} ({result.ManagedAssemblyBytes} bytes)",
            $"Managed assemblies metadata-scanned: {result.ManagedAssembliesScanned}",
            $"Native binaries: {result.NativeBinaryFiles} ({result.NativeBinaryBytes} bytes)",
            $"Godot/GodotSharp indicator files: {result.GodotSharpIndicatorFiles}",
            $"FMOD indicator files: {result.FmodIndicatorFiles}",
            $"Spine indicator files: {result.SpineIndicatorFiles}",
            $"General reflection indicator files: {result.ReflectionIndicatorFiles}",
            $"Dynamic-code/JIT indicator files: {result.DynamicCodeIndicatorFiles}",
            $"Platform-specific indicator files: {result.PlatformSpecificFiles}",
            $"Other/unclassified files: {result.OtherFiles}",
            $"Potential iOS blocker signals: {result.PotentialIosBlockerSignals.Count}",
            $"Dependency notes: {result.DependencyNotes.Count}",
            $"Steam session consulted: {YesNo(result.SteamSessionConsulted)}",
            $"Network access attempted by Step 14: {YesNo(result.NetworkAccessAttempted)}",
            $"Managed install modified by Step 14: {YesNo(result.ManagedInstallModified)}",
            $"Game launch attempted: {YesNo(result.GameLaunchAttempted)}",
            $"Elapsed: {result.Elapsed.TotalSeconds:F1}s",
        };

        AddEvidence(lines, "Potential iOS blocker signals", result.PotentialIosBlockerSignals, 8);
        AddEvidence(lines, "Dependency notes", result.DependencyNotes, 8);
        AddEvidence(lines, "Managed assembly sample", result.ManagedAssemblyEvidence, 10);
        AddEvidence(lines, "Native binary sample", result.NativeBinaryEvidence, 10);
        AddEvidence(lines, "Dynamic-code evidence", result.DynamicCodeEvidence, 8);
        AddEvidence(lines, "Reflection evidence", result.ReflectionEvidence, 8);
        AddEvidence(lines, "Godot/GodotSharp evidence", result.GodotSharpEvidence, 8);
        AddEvidence(lines, "FMOD evidence", result.FmodEvidence, 8);
        AddEvidence(lines, "Spine evidence", result.SpineEvidence, 8);
        AddEvidence(lines, "Platform-specific evidence", result.PlatformSpecificEvidence, 8);

        lines.Add("Step 14 evidence policy: metadata/path indicators are triage signals, not proof that an API path executes at runtime.");
        lines.Add("Mono.Cecil rewrite / StS2 game execution: NOT IMPLEMENTED; Step 15 Godot Foundation is a separate launcher-owned smoke-host test.");

        if (!string.IsNullOrWhiteSpace(result.Error))
            lines.Add($"Error: {result.Error}");

        return string.Join("\n", lines);
    }

    private static void AddEvidence(
        List<string> lines,
        string title,
        IReadOnlyList<string> evidence,
        int limit)
    {
        if (evidence.Count == 0)
            return;

        lines.Add($"{title}:");
        foreach (var item in evidence.Take(limit))
            lines.Add($"  • {item}");
        if (evidence.Count > limit)
            lines.Add($"  • … {evidence.Count - limit} more");
    }

    private static string FormatManagedInstallDetail(SteamManagedInstallResult result)
    {
        var lines = new List<string>
        {
            $"Target AppID: {result.TargetAppId}",
            $"Selected depot: {result.DepotId?.ToString() ?? "N/A"}",
            $"Current public manifest: {result.CurrentManifestId?.ToString() ?? "N/A"}",
            $"Installed manifest before: {result.InstalledManifestIdBefore?.ToString() ?? "none"}",
            $"Installed manifest after: {result.InstalledManifestIdAfter?.ToString() ?? "none"}",
            $"Branch: {result.Branch ?? "N/A"}",
            $"State before: {result.StateBefore}",
            $"Action taken: {result.ActionTaken}",
            $"State after: {result.StateAfter}",
            $"Planned files: {result.PlannedFiles}",
            $"Planned bytes: {result.PlannedBytes}",
            $"Verified source files/bytes: {result.VerifiedSourceFiles} / {result.VerifiedSourceBytes}",
            $"Source cache reverified against current Steam manifest: {YesNo(result.SourceCacheReverifiedAgainstCurrentManifest)}",
            $"Source bytes downloaded this manager run: {result.SourceNewlyDownloadedBytes}",
            $"Reused locally verified files/bytes: {result.ReusedLocalFiles} / {result.ReusedLocalBytes}",
            $"Replaced files/bytes: {result.ReplacedFiles} / {result.ReplacedBytes}",
            $"Previous install preserved until commit: {YesNo(result.ExistingInstallPreservedUntilCommit)}",
            $"Atomic commit completed: {YesNo(result.AtomicCommitCompleted)}",
            $"Rollback restored previous install: {YesNo(result.RollbackRestoredPreviousInstall)}",
            $"Staging absent after result: {YesNo(result.StagingAbsentAfterResult)}",
            $"Backup absent after result: {YesNo(result.BackupAbsentAfterResult)}",
            $"Managed install relative path: {result.ManagedInstallRelativePath ?? "not-installed"}",
            $"Verified Step 11 source cache: {result.SourceCacheRelativePath ?? "not-needed"}",
            $"Elapsed: {result.Elapsed.TotalSeconds:F1}s",
        };

        if (!string.IsNullOrWhiteSpace(result.Error))
            lines.Add($"Error: {result.Error}");

        lines.Add("Managed receipt contents: AppID/depot/manifest/branch + relative path/length/SHA-1 only");
        lines.Add("Steam refresh token/password/Guard persistence in install receipt: NONE");
        lines.Add("Depot key / manifest request code / CDN auth token persistence in install receipt: NONE");
        lines.Add("Previous good install visibility during staging: PRESERVED");
        lines.Add("Partial replacement visibility: NONE — replacement becomes live only at directory swap");
        lines.Add("Multi-depot app composition: NOT IMPLEMENTED");
        lines.Add("Compatibility inventory / Cecil / Godot / game launch: NOT RUN");
        lines.Add("Steam Cloud / Workshop: NOT RUN");
        return string.Join("\n", lines);
    }

    private static string FormatUtc(DateTimeOffset? value) =>
        value?.UtcDateTime.ToString("yyyy-MM-dd HH:mm:ss 'UTC'") ?? "unavailable";

    private static string YesNoNullable(bool? value) =>
        value.HasValue ? YesNo(value.Value) : "unknown";

    private async Task RunFoundationVerificationAsync()
    {
        if (_foundationButton is null ||
            _foundationResultLabel is null ||
            _foundationDetailLabel is null ||
            _statusLabel is null)
        {
            return;
        }

        BeginSteamOperation(allowCancel: false);
        _foundationResultLabel.Text = "FOUNDATION: TESTING…";
        _foundationResultLabel.TextColor = UIColor.Label;
        _foundationDetailLabel.Text = "Running the proven Steps 01–05 5/5 regression…";

        try
        {
            var core = CoreSelfTest.Run();
            var keychain = _keychainProbe.RunRoundTrip();
            var steam = await _steamProbe.RunAsync(TimeSpan.FromSeconds(25));
            var final = new FoundationVerificationResult(
                UiStartupPassed: _uiStartupPassed,
                LifecycleActive: _lifecycleActive,
                Core: core,
                CredentialStore: keychain,
                Steam: steam);

            InvokeOnMainThread(() =>
            {
                _foundationResultLabel.Text = final.Summary;
                _foundationResultLabel.TextColor = final.Passed
                    ? UIColor.Label
                    : UIColor.SystemRed;

                _foundationDetailLabel.Text =
                    $"App/UI startup: {PassFail(final.UiStartupPassed)}\n" +
                    $"Lifecycle active: {PassFail(final.LifecycleActive)}\n" +
                    $"{final.Core.Summary}\n" +
                    $"{final.CredentialStore.Summary} — probe value cleaned\n" +
                    $"{final.Steam.Summary}\n" +
                    $"CMWebSocket factory used: {YesNo(final.Steam.CmWebSocketFactoryUsed)}";

                _statusLabel.Text = final.Passed
                    ? "PASS: Steps 01–05 foundation still passes 5/5 on this device."
                    : "FAIL: a proven foundation regression failed; stop and investigate before any later boundary.";
                _statusLabel.TextColor = final.Passed ? UIColor.Label : UIColor.SystemRed;
                RefreshSavedSessionStatus();
            });
        }
        catch (Exception ex)
        {
            InvokeOnMainThread(() =>
            {
                _foundationResultLabel.Text = "FOUNDATION: EXCEPTION";
                _foundationResultLabel.TextColor = UIColor.SystemRed;
                _foundationDetailLabel.Text = $"{ex.GetType().Name}: {ex.Message}";
                _statusLabel.Text = "FAIL: exception during Steps 01–05 regression.";
                _statusLabel.TextColor = UIColor.SystemRed;
            });
        }
        finally
        {
            InvokeOnMainThread(EndSteamOperation);
        }
    }

    private static string PassFail(bool passed) => passed ? "PASS" : "FAIL";
    private static string YesNo(bool value) => value ? "YES" : "NO";

    private static UILabel Label(string text, UIFont font, UIColor color)
    {
        return new UILabel
        {
            TranslatesAutoresizingMaskIntoConstraints = false,
            Text = text,
            TextColor = color,
            TextAlignment = UITextAlignment.Center,
            Lines = 0,
            Font = font
        };
    }

    private static UITextField TextField(
        string placeholder,
        bool secure,
        NSString contentType)
    {
        return new UITextField
        {
            TranslatesAutoresizingMaskIntoConstraints = false,
            Placeholder = placeholder,
            SecureTextEntry = secure,
            TextContentType = contentType,
            BorderStyle = UITextBorderStyle.RoundedRect,
            ClearButtonMode = UITextFieldViewMode.WhileEditing,
            ReturnKeyType = secure ? UIReturnKeyType.Go : UIReturnKeyType.Next,
        };
    }

    private static UIButton SystemButton(string title, nfloat fontSize)
    {
        var button = UIButton.FromType(UIButtonType.System);
        button.TranslatesAutoresizingMaskIntoConstraints = false;
        button.SetTitle(title, UIControlState.Normal);
        button.TitleLabel!.Font = UIFont.BoldSystemFontOfSize(fontSize);
        return button;
    }

    private static UIView Separator()
    {
        var view = new UIView
        {
            TranslatesAutoresizingMaskIntoConstraints = false,
            BackgroundColor = UIColor.Separator
        };
        view.HeightAnchor.ConstraintEqualTo(1).Active = true;
        return view;
    }
}
