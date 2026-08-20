using Foundation;
using StS2Launcher.Core;
using StS2Launcher.iOS.Platform;
using UIKit;

namespace StS2Launcher.iOS;

public sealed partial class RootViewController : UIViewController
{
    private readonly string _launcherDataRoot;
    private readonly DeviceTestReportWriter _deviceTestReportWriter;
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
    private readonly HostFrameworkClosureFoundation _hostFrameworkClosureFoundation;
    private readonly HostFrameworkClosureGateSequence _hostFrameworkClosureGates = new();
    private readonly FirstRealGameAssemblyLoad _firstRealGameAssemblyLoad;
    private readonly FirstRealGameAssemblyLoadGateSequence _firstRealGameAssemblyLoadGates = new();

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
    private UILabel? _firstRealGameAssemblyLoadResultLabel;
    private UILabel? _firstRealGameAssemblyLoadDetailLabel;
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
    private UIButton? _firstRealGameAssemblyLoadButton;
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
        _launcherDataRoot = Path.Combine(documentsRoot, "StS2Launcher");
        _deviceTestReportWriter = new DeviceTestReportWriter(_launcherDataRoot);
        _singleFileDownloadAttempt = new SteamSingleFileDownloadAttempt(_sessionStore, _launcherDataRoot);
        _fullDepotDownloadAttempt = new SteamFullDepotDownloadAttempt(_sessionStore, _launcherDataRoot);
        _resumableDepotDownloadAttempt = new SteamResumableDepotDownloadAttempt(_sessionStore, _launcherDataRoot);
        _managedInstallAttempt = new SteamManagedInstallAttempt(_sessionStore, _launcherDataRoot);
        _downloadCacheMaintenance = new SteamDownloadCacheMaintenance(_launcherDataRoot);
        _offlineInstallInspection = new SteamOfflineInstallInspection(_launcherDataRoot);
        _compatibilityInventoryInspection = new SteamCompatibilityInventoryInspection(_launcherDataRoot);
        _managedPreparationFoundation = new ManagedPreparationFoundation(_launcherDataRoot);
        _compatibilityCallSiteAnalysis = new CompatibilityCallSiteAnalysis(_launcherDataRoot);
        _realAssemblyRewriteWorkspace = new RealAssemblyRewriteWorkspace(_launcherDataRoot);
        _expressionInterpreterCompatibility = new ExpressionInterpreterCompatibility(_launcherDataRoot);
        _dynamicManagedExecutionFoundation = new DynamicManagedExecutionFoundation(
            _launcherDataRoot,
            Path.Combine(NSBundle.MainBundle.BundlePath, DynamicManagedExecutionFoundation.BundleFixtureDirectoryName));
        _preparedRuntimeFrameworkBinding = new PreparedRuntimeFrameworkBinding(_launcherDataRoot);
        _runtimeBindingDiagnosticsExporter = new RuntimeBindingDiagnosticsExporter(_launcherDataRoot);
        _hostFrameworkClosureFoundation = new HostFrameworkClosureFoundation(_launcherDataRoot);
        _firstRealGameAssemblyLoad = new FirstRealGameAssemblyLoad(_launcherDataRoot);
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
            "STEP 23.2 — FIRST REAL STS2 CLR LOAD BOUNDARY",
            UIFont.BoldSystemFontOfSize(18),
            UIColor.SecondaryLabel));

        content.AddArrangedSubview(Label(
            "Version 0.0.67",
            UIFont.SystemFontOfSize(17),
            UIColor.SecondaryLabel));

        content.AddArrangedSubview(Label(
            "STEPS 01–22 PHYSICALLY CLOSED • FIRST REAL GAME CLR LOAD IS THE ONLY NEW BOUNDARY",
            UIFont.BoldSystemFontOfSize(14),
            UIColor.SecondaryLabel));

        content.AddArrangedSubview(Label(
            "Step 22.4.2 is the fully green canonical foundation: Step 19, Step 22, OfflineReady, Foundation 5/5, and all other current regressions passed on the physical iPhone. Step 23 crosses exactly one new boundary: it loads the receipt-verified prepared real sts2.dll into a dedicated private AssemblyLoadContext and resolves the already-audited managed dependency plan. It does not intentionally inspect game types/members, invoke an entry point or method, initialize Godot/game state, or resolve native game libraries. Gate A refuses to load if any prepared private assembly contains a module initializer, so the load-only boundary remains explicit.",
            UIFont.SystemFontOfSize(15),
            UIColor.Label));

        content.AddArrangedSubview(Label(
            "Automatic test reports: Files → On My iPhone → StS2 Launcher → StS2Launcher → Reports. Each current verification overwrites one deterministic latest .txt file so results can be shared without screenshots.",
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
        _godotFoundationGateDButton.TouchUpInside += async (_, _) => await VerifyGodotFoundationGateDAsync();
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
            "Step 22 — Host Binding Frontier Regression (closed boundary, ordered gates A–D)",
            UIFont.BoldSystemFontOfSize(25),
            UIColor.Label));

        _runtimeFrameworkBindingButton = SystemButton("Run Step 22 A–D Regression — Qualify Roots → Recompute Closure → Prepare Set → Audit", 17);
        _runtimeFrameworkBindingButton.TouchUpInside += async (_, _) => await RunHostFrameworkClosureFoundationAsync();
        content.AddArrangedSubview(_runtimeFrameworkBindingButton);

        _runtimeFrameworkBindingResultLabel = Label(
            "HOST FRAMEWORK CLOSURE FOUNDATION: NOT RUN",
            UIFont.BoldSystemFontOfSize(21),
            UIColor.Label);
        content.AddArrangedSubview(_runtimeFrameworkBindingResultLabel);

        _runtimeFrameworkBindingDetailLabel = Label(
            "Gate A requires all 22 measured direct host-binding roots to load from the iOS/.NET default host. It still probes the full 44-name desktop/workspace framework frontier for diagnostics, but transitive-only misses are non-blocking because host-bound framework assemblies terminate the Step 21 private dependency traversal. Gate B reruns the physically proven Step 21 real sts2.dll classification/binding engine and is authoritative for the new blocker frontier. Gate C persists the recomputed plan, then requires Explicit binding blockers: 0, Runtime closure ready: YES, and no System.*/netstandard framework implementation in private storage; no Cecil writes occur. Gate D reruns the independent Step 21 source/prepared/live/plan audit and re-qualifies zero blockers. The trusted install remains read-only and StS2 is still never CLR-loaded or executed.",
            UIFont.SystemFontOfSize(15),
            UIColor.SecondaryLabel);
        content.AddArrangedSubview(_runtimeFrameworkBindingDetailLabel);

        _runtimeBindingDiagnosticsExportButton = SystemButton("Export Current Runtime Binding Diagnostics to Files", 17);
        _runtimeBindingDiagnosticsExportButton.TouchUpInside += async (_, _) => await RunRuntimeBindingDiagnosticsExportAsync();
        content.AddArrangedSubview(_runtimeBindingDiagnosticsExportButton);

        _runtimeBindingDiagnosticsExportResultLabel = Label(
            "DIAGNOSTIC EXPORT: NOT RUN — exports the current persisted runtime-binding plan",
            UIFont.BoldSystemFontOfSize(17),
            UIColor.SecondaryLabel);
        content.AddArrangedSubview(_runtimeBindingDiagnosticsExportResultLabel);

        content.AddArrangedSubview(Label(
            "Files location after export: On My iPhone → StS2 Launcher → StS2Launcher → Step21.1-RuntimeBindingDiagnostics.txt. The report contains the complete blocker list, grouped blocker counts, unique requested identities, host bindings, prepared assembly identities, and the persisted plan SHA-256. It intentionally omits Steam credentials/tokens and host absolute file locations. The exported text is diagnostic output only and is never trusted as launcher input. Because iOS exposes the app Documents directory for this hotfix, avoid editing or deleting other StS2Launcher files in Files.",
            UIFont.SystemFontOfSize(14),
            UIColor.SecondaryLabel));

        content.AddArrangedSubview(Separator());

        content.AddArrangedSubview(Label(
            "Step 23 — First Real StS2 CLR Load Boundary (ordered gates A–D)",
            UIFont.BoldSystemFontOfSize(25),
            UIColor.Label));

        _firstRealGameAssemblyLoadButton = SystemButton("Run Step 23 A–D — Preflight → Load sts2.dll → Resolve Managed Closure → Audit", 17);
        _firstRealGameAssemblyLoadButton.TouchUpInside += async (_, _) => await RunFirstRealGameAssemblyLoadAsync();
        content.AddArrangedSubview(_firstRealGameAssemblyLoadButton);

        _firstRealGameAssemblyLoadResultLabel = Label(
            "FIRST REAL STS2 CLR LOAD BOUNDARY: NOT RUN",
            UIFont.BoldSystemFontOfSize(21),
            UIColor.Label);
        content.AddArrangedSubview(_firstRealGameAssemblyLoadResultLabel);

        _firstRealGameAssemblyLoadDetailLabel = Label(
            "Gate A re-proves OfflineReady, validates the persisted zero-blocker Step 21/22 plan and every prepared/live SHA-1, and uses Cecil to require IL-only private assemblies with zero <Module> module initializers before loading anything. Gate B performs the first real sts2.dll LoadFromStream into a dedicated private AssemblyLoadContext and stops after identity/context verification. Gate C asks that context to resolve every unique managed dependency identity in the audited plan: host frameworks must come from the default iOS/.NET context, private assemblies only from the exact prepared set, and unplanned fallback is refused. Gate D re-hashes plan/prepared/live state, re-proves OfflineReady, audits load-context ownership, and requires zero native resolution attempts. No game entry point, game type/member reflection, method invocation, Godot startup, or native game load is part of Step 23.",
            UIFont.SystemFontOfSize(15),
            UIColor.SecondaryLabel);
        content.AddArrangedSubview(_firstRealGameAssemblyLoadDetailLabel);

        _signOutButton = SystemButton("Sign Out / Clear Saved Session", 16);
        _signOutButton.TouchUpInside += (_, _) => ClearSavedSession();
        content.AddArrangedSubview(_signOutButton);

        _cancelOperationButton = SystemButton("Cancel Current Operation", 15);
        _cancelOperationButton.Enabled = false;
        _cancelOperationButton.TouchUpInside += (_, _) => _operationCts?.Cancel();
        content.AddArrangedSubview(_cancelOperationButton);

        content.AddArrangedSubview(Separator());

        _statusLabel = Label(
            "Status: Steps 01–22 are physically closed and Step 22.4.2 is the canonical foundation baseline. Step 23 is the first real sts2.dll CLR-load candidate. Run it only in a fresh process; after a successful load, the game assembly remains resident until force-quit. Long results are written to Files under Documents/StS2Launcher/Reports.",
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
        Console.WriteLine("Step 23 first real StS2 CLR load boundary: RootViewController.ViewDidLoad complete");
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
