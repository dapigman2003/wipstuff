using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace StS2Launcher.Core;

/// <summary>
/// Steps 43-64 are packaged together as an explicitly sequential startup ladder. Every rung keeps
/// its own four-gate authority and later rungs require the exact same-process closure of the prior
/// rung. Step 46 maps the real LaunchMainMenu immediate/deferred frontier without invoking it;
/// Steps 47-64 use a separate guarded direct main-menu, single-player, and character-select continuation path so original GameStartup,
/// LaunchMainMenu, ExecuteDeferred, Steam startup, and native game extensions remain unopened.
/// </summary>
public sealed partial class TransformedRealStS2VeryEarlyInitialization
{
    private const string Step43Name = "NULL PLATFORM AUTHORITY";
    private const string Step44Name = "LEGACY MIGRATION GUARD";
    private const string Step45Name = "LOCAL SAVE INITIALIZATION";
    private const string Step46Name = "LAUNCHMAINMENU IMMEDIATE FRONTIER MAP";
    private const string Step47Name = "MAIN MENU RESOURCE PREPARATION";
    private const string Step48Name = "MAIN MENU OFF-TREE INSTANTIATION";
    private const string Step49Name = "MAIN MENU FROZEN SCENETREE ADMISSION";
    private const string Step50Name = "MAIN MENU CONTROLLED RENDER PULSE";
    private const string Step51Name = "SINGLEPLAYER FRONTIER MAP";
    private const string Step52Name = "MAIN MENU SUSTAINED RENDER RESIDENCY";

    private const string PlatformUtilTypeFullName = "MegaCrit.Sts2.Core.Platform.PlatformUtil";
    private const string PlatformUtilInterfaceTypeFullName = "MegaCrit.Sts2.Core.Platform.IPlatformUtilStrategy";
    private const string AccountScopeMigratorTypeFullName = "MegaCrit.Sts2.Core.Saves.AccountScopeUserDataMigrator";
    private const string ProfileScopeMigratorTypeFullName = "MegaCrit.Sts2.Core.Saves.ProfileAccountScopeMigrator";
    private const string SaveManagerTypeFullName = "MegaCrit.Sts2.Core.Saves.SaveManager";
    private const string SaveManagerMockInstanceFieldName = "_mockInstance";
    private const string SaveManagerInstanceFieldName = "_instance";
    private const string Step46AsyncStateMachineAttributeFullName = "System.Runtime.CompilerServices.AsyncStateMachineAttribute";
    private static readonly string[] StartupLadderCompilerStateMachineAttributes =
    [
        Step46AsyncStateMachineAttributeFullName,
        "System.Runtime.CompilerServices.IteratorStateMachineAttribute",
        "System.Runtime.CompilerServices.AsyncIteratorStateMachineAttribute",
    ];

    private StartupLadderBaseline? _step43Baseline;
    private StartupLadderBaseline? _step44Baseline;
    private StartupLadderBaseline? _step45Baseline;
    private StartupLadderBaseline? _step45PostActionBaseline;
    private StartupLadderBaseline? _step46Baseline;
    private StartupLadderBaseline? _step47Baseline;

    private string _step43StaticMap = string.Empty;
    private string _step44StaticMap = string.Empty;
    private string _step45StaticMap = string.Empty;
    private string _step46StaticMap = string.Empty;
    private string _step43Observation = string.Empty;
    private string _step44Observation = string.Empty;
    private string _step45Observation = string.Empty;
    private string _step45AcceptedHostBinding = string.Empty;

    private bool _step43ActionPassed;
    private bool _step44NoLegacyDataPassed;
    private bool _step45InvocationStarted;
    private bool _step45SaveInitPassed;
    private bool _step46StateMachineMapped;
    private bool _step46ClosureMapped;
    private bool _step46LaunchAdmissible; // retained only as a diagnostic compatibility flag; original LaunchMainMenu remains forbidden.
    private bool _step46StaticMapDurablyWritten;
    private uint _step46LaunchMethodToken;
    private uint _step46MoveNextToken;
    private string _step46StateMachineTypeName = string.Empty;

    private bool _exactStep43ClosurePassed;
    private bool _exactStep44ClosurePassed;
    private bool _exactStep45ClosurePassed;
    private bool _exactStep46ClosurePassed;
    private bool _exactStep47ClosurePassed;

    public bool ExactStep43ClosurePassed => _exactStep43ClosurePassed;
    public bool ExactStep44ClosurePassed => _exactStep44ClosurePassed;
    public bool ExactStep45ClosurePassed => _exactStep45ClosurePassed;
    public bool ExactStep46ClosurePassed => _exactStep46ClosurePassed;
    public bool ExactStep47ClosurePassed => _exactStep47ClosurePassed;
    public bool Step46LaunchMainMenuAdmissible => false;

    public string GetVerifiedStartupLadderStaticMap(int step)
        => step switch
        {
            43 when !string.IsNullOrWhiteSpace(_step43StaticMap) => _step43StaticMap,
            44 when !string.IsNullOrWhiteSpace(_step44StaticMap) => _step44StaticMap,
            45 when !string.IsNullOrWhiteSpace(_step45StaticMap) => _step45StaticMap,
            46 when !string.IsNullOrWhiteSpace(_step46StaticMap) => _step46StaticMap,
            47 when !string.IsNullOrWhiteSpace(_step47DirectStaticMap) => _step47DirectStaticMap,
            48 when !string.IsNullOrWhiteSpace(_step48DirectStaticMap) => _step48DirectStaticMap,
            49 when !string.IsNullOrWhiteSpace(_step49DirectStaticMap) => _step49DirectStaticMap,
            50 when !string.IsNullOrWhiteSpace(_step50DirectStaticMap) => _step50DirectStaticMap,
            51 when !string.IsNullOrWhiteSpace(_step51DirectStaticMap) => _step51DirectStaticMap,
            52 when !string.IsNullOrWhiteSpace(_step52DirectStaticMap) => _step52DirectStaticMap,
            >= 53 and <= 57 => GetVerifiedSingleplayerContinuationStaticMap(step),
            >= 58 and <= 64 => GetVerifiedCharacterSelectContinuationStaticMap(step),
            _ => throw new InvalidOperationException($"Step {step}.0 has not produced a verified startup-ladder static map."),
        };

    public void MarkStep46StaticMapDurablyWritten()
    {
        if (!_step46StateMachineMapped || !_step46ClosureMapped || string.IsNullOrWhiteSpace(_step46StaticMap))
            throw new InvalidOperationException("Step 46.0 static map cannot be marked durable before the exact immediate/deferred LaunchMainMenu frontier map is complete.");
        _step46StaticMapDurablyWritten = true;
    }

    private void ResetStartupLadderState()
    {
        _step43Baseline = null;
        _step44Baseline = null;
        _step45Baseline = null;
        _step45PostActionBaseline = null;
        _step46Baseline = null;
        _step47Baseline = null;
        _step43StaticMap = string.Empty;
        _step44StaticMap = string.Empty;
        _step45StaticMap = string.Empty;
        _step46StaticMap = string.Empty;
        _step43Observation = string.Empty;
        _step44Observation = string.Empty;
        _step45Observation = string.Empty;
        _step45AcceptedHostBinding = string.Empty;
        _step43ActionPassed = false;
        _step44NoLegacyDataPassed = false;
        _step45InvocationStarted = false;
        _step45SaveInitPassed = false;
        _step46StateMachineMapped = false;
        _step46ClosureMapped = false;
        _step46LaunchAdmissible = false;
        _step46StaticMapDurablyWritten = false;
        _step46LaunchMethodToken = 0;
        _step46MoveNextToken = 0;
        _step46StateMachineTypeName = string.Empty;
        _exactStep43ClosurePassed = false;
        _exactStep44ClosurePassed = false;
        _exactStep45ClosurePassed = false;
        _exactStep46ClosurePassed = false;
        _exactStep47ClosurePassed = false;
        ResetDirectMainMenuLadderState();
        ResetSingleplayerContinuationState();
    }

    // STEP 43 — NULL PLATFORM AUTHORITY

    public TransformedRealStS2StartupLadderGateResult RunStep43ClosedStep42Authority(
        bool renderingStopped,
        Action<string>? checkpoint = null)
    {
        const int step = 43;
        const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.PrerequisiteAuthority;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            ResetStartupLadderState();
            var context = RequireStep43Prerequisite("Step 43 Gate A entry");
            if (!renderingStopped)
                throw new InvalidOperationException("Step 43.0 requires rendering to remain frozen after Step 42 closure.");
            Checkpoint(checkpoint, "M43_A_ENTRY — requiring same-process Step-42 4/4 authority, retained real NGame/state=2, renderer frozen, and unchanged selected compatibility bytes before platform-strategy inspection.");
            stage = "closed Step-42 authority re-verification";
            var state = RequireStep40InsertedAuthority();
            var selected = RequireStartupLadderSelectedAuthority(step);
            _step43Baseline = CaptureStartupLadderBaseline(selected.Path, selected.Sha256, context);
            RequireStartupLadderBaselineUnchanged(context, _step43Baseline, "Step 43 Gate A");
            Checkpoint(checkpoint, $"M43_A_PASS — Step-42 4/4 retained; renderingStopped=True; NGame state={state}; selectedSha256={selected.Sha256}; resolver/host/private/initializer/rejected/native deltas=0.");
            return StartupLadderPass(step, Step43Name, gate,
                "Closed Step-42 authority retained. Rendering is frozen, NGame/state=2 is intact, and the selected compatibility image/context are unchanged.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"M43_A_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return StartupLadderFail(step, Step43Name, gate, stage, ex);
        }
    }

    public TransformedRealStS2StartupLadderGateResult RunStep43NullPlatformStaticAudit(Action<string>? checkpoint = null)
    {
        const int step = 43;
        const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.StaticAuditOrBinding;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep43Prerequisite("Step 43 Gate B entry");
            var baseline = _step43Baseline ?? throw new InvalidOperationException("Step 43.0 Gate A must pass before Gate B.");
            Checkpoint(checkpoint, "M43_B_ENTRY — mapping PlatformUtil plus the concrete NullPlatformUtilStrategy read-only surface. Only platform dispatch/read helpers and exact SteamInitializer.get_Initialized observation are admissible; external Steamworks/native/FMOD/Spine/Sentry-external remain forbidden.");
            stage = "Null platform strategy static closure audit";

            using var resolver = new RejectingAssemblyResolver();
            using var module = OpenStartupLadderModule(baseline.SelectedPath, resolver);
            var allTypes = EnumerateTypes(module.Types).ToDictionary(type => type.FullName, StringComparer.Ordinal);
            var allMethods = BuildStartupLadderMethodMap(allTypes);
            var platformUtil = RequireStartupLadderType(allTypes, PlatformUtilTypeFullName, step);
            var nullStrategy = RequireStartupLadderType(allTypes, NullPlatformTypeFullName, step);
            var roots = new List<MethodDefinition>
            {
                RequireStartupLadderMethod(platformUtil, "get_PrimaryPlatform", 0, true, step),
                RequireStartupLadderMethod(platformUtil, "GetPlatformUtil", 1, true, step),
                RequireStartupLadderMethod(platformUtil, "GetLocalPlayerId", 1, true, step),
                RequireStartupLadderMethod(platformUtil, "GetPlatformBranch", 0, true, step),
                RequireStartupLadderMethod(platformUtil, "GetThreeLetterLanguageCode", 0, true, step),
                RequireStartupLadderMethod(platformUtil, "GetRawLanguage", 0, true, step),
                RequireStartupLadderMethod(platformUtil, "GetSupportedWindowMode", 0, true, step),
                RequireStartupLadderMethod(nullStrategy, "GetLocalPlayerId", 0, false, step),
                RequireStartupLadderMethod(nullStrategy, "GetPlatformBranch", 0, false, step),
                RequireStartupLadderMethod(nullStrategy, "GetThreeLetterLanguageCode", 0, false, step),
                RequireStartupLadderMethod(nullStrategy, "GetRawLanguage", 0, false, step),
                RequireStartupLadderMethod(nullStrategy, "GetSupportedWindowMode", 0, false, step),
            };
            var audit = AuditStartupLadderRoots(roots, allTypes, allMethods);
            RequireStartupLadderAuditAdmissible(audit, "Step 43.0 Null platform audit");
            if (resolver.Requests.Count != 0)
                throw new InvalidDataException("Step 43.0 static audit unexpectedly attempted external Cecil resolution: " + string.Join(" | ", resolver.Requests));

            _step43StaticMap = BuildStartupLadderStaticMap(step, Step43Name, baseline, roots, audit,
                "PlatformUtil + concrete NullPlatformUtilStrategy read-only authority; no invocation while this map was built.");
            RequireStartupLadderBaselineUnchanged(context, baseline, "Step 43 Gate B");
            Checkpoint(checkpoint, $"M43_B_PASS — roots={roots.Count}; closureMethods={audit.ClosureMethods.Length}; classifiedBoundaryRefs={audit.Boundaries.Length}; forbiddenBoundaryRefs=0; unresolvedSameSts2Refs=0; externalResolutionRequests=0; invocation=NO.");
            return StartupLadderPass(step, Step43Name, gate,
                $"Null-platform static audit passed. Roots={roots.Count}; closure={audit.ClosureMethods.Length}; classified refs={audit.Boundaries.Length}; forbidden/unresolved/external resolution=0.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"M43_B_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return StartupLadderFail(step, Step43Name, gate, stage, ex);
        }
    }

    public TransformedRealStS2StartupLadderGateResult RunStep43NullPlatformReadProbe(Action<string>? checkpoint = null)
    {
        const int step = 43;
        const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.ControlledAction;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep43Prerequisite("Step 43 Gate C entry");
            var baseline = _step43Baseline ?? throw new InvalidOperationException("Step 43.0 Gate A/B authority is absent.");
            if (string.IsNullOrWhiteSpace(_step43StaticMap))
                throw new InvalidOperationException("Step 43.0 requires a completed static map before the read probe.");
            Checkpoint(checkpoint, "M43_C_ENTRY — reflecting current PlatformUtil authority and invoking only read-only platform getters. The concrete strategy must be exactly NullPlatformUtilStrategy; no platform initialization or Steam API is authorized.");
            stage = "exact Null platform runtime read probe";

            var stateBefore = RequireStep40InsertedAuthority();
            var admission = RequireAdmission();
            var platformUtilType = admission.Assembly.GetType(PlatformUtilTypeFullName, throwOnError: true, ignoreCase: false)
                ?? throw new MissingMemberException(PlatformUtilTypeFullName);
            var nullStrategyType = admission.Assembly.GetType(NullPlatformTypeFullName, throwOnError: true, ignoreCase: false)
                ?? throw new MissingMemberException(NullPlatformTypeFullName);
            var interfaceType = admission.Assembly.GetType(PlatformUtilInterfaceTypeFullName, throwOnError: true, ignoreCase: false)
                ?? throw new MissingMemberException(PlatformUtilInterfaceTypeFullName);

            var primaryProperty = platformUtilType.GetProperty("PrimaryPlatform", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                ?? throw new MissingMemberException(PlatformUtilTypeFullName, "PrimaryPlatform");
            var primaryPlatform = primaryProperty.GetValue(null)
                ?? throw new InvalidDataException("Step 43.0 PlatformUtil.PrimaryPlatform returned null.");
            var getPlatformUtil = platformUtilType.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .SingleOrDefault(method => method.Name == "GetPlatformUtil" && method.GetParameters().Length == 1 && method.GetParameters()[0].ParameterType == primaryPlatform.GetType())
                ?? throw new MissingMethodException(PlatformUtilTypeFullName, "GetPlatformUtil(PlatformType)");
            var strategy = getPlatformUtil.Invoke(null, new[] { primaryPlatform })
                ?? throw new InvalidDataException("Step 43.0 PlatformUtil.GetPlatformUtil returned null.");
            if (strategy.GetType() != nullStrategyType)
                throw new InvalidDataException($"Step 43.0 requires the concrete Null platform strategy; observed {strategy.GetType().FullName} for PrimaryPlatform={primaryPlatform}.");
            if (!interfaceType.IsInstanceOfType(strategy))
                throw new InvalidDataException("Step 43.0 concrete Null strategy does not implement the expected IPlatformUtilStrategy authority.");

            var localPlayerId = InvokeStartupLadderZeroArg(interfaceType, strategy, "GetLocalPlayerId");
            var platformBranch = InvokeStartupLadderZeroArg(interfaceType, strategy, "GetPlatformBranch");
            var threeLetterLanguage = InvokeStartupLadderZeroArg(interfaceType, strategy, "GetThreeLetterLanguageCode");
            var rawLanguage = InvokeStartupLadderZeroArg(interfaceType, strategy, "GetRawLanguage");
            var supportedWindowMode = InvokeStartupLadderZeroArg(interfaceType, strategy, "GetSupportedWindowMode");

            var stateAfter = RequireStep40InsertedAuthority();
            RequireStartupLadderBaselineUnchanged(context, baseline, "Step 43 Gate C");
            if (stateAfter != stateBefore)
                throw new InvalidDataException($"Step 43.0 read probe changed OneTimeInitialization state: {stateBefore} -> {stateAfter}.");
            _step43Observation = $"PrimaryPlatform={primaryPlatform}; strategy={strategy.GetType().FullName}; localPlayerId={localPlayerId}; branch={platformBranch}; threeLetterLanguage={threeLetterLanguage}; rawLanguage={rawLanguage}; supportedWindowMode={supportedWindowMode}";
            _step43ActionPassed = true;
            Checkpoint(checkpoint, "M43_C_PASS — " + _step43Observation + "; resolver/host/private/initializer/rejected/native deltas=0; rendering remains stopped.");
            return StartupLadderPass(step, Step43Name, gate,
                "Exact runtime platform read probe passed. " + _step43Observation + ". No resolver/native/context drift occurred.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"M43_C_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return StartupLadderFail(step, Step43Name, gate, stage, ex);
        }
    }

    public TransformedRealStS2StartupLadderGateResult RunStep43FrozenConfinement(bool renderingStopped, Action<string>? checkpoint = null)
        => RunStartupLadderFrozenConfinement(43, Step43Name, renderingStopped, _step43Baseline, _step43ActionPassed,
            () => _exactStep43ClosurePassed = true, _step43Observation, checkpoint);

    // STEP 44 — LEGACY MIGRATION GUARD

    public TransformedRealStS2StartupLadderGateResult RunStep44ClosedStep43Authority(bool renderingStopped, Action<string>? checkpoint = null)
        => RunStartupLadderSimplePrerequisite(44, Step44Name, renderingStopped, RequireStep44Prerequisite, baseline => _step44Baseline = baseline,
            "M44_A", "same-process Step-43 4/4 Null-platform authority", checkpoint);

    public TransformedRealStS2StartupLadderGateResult RunStep44LegacyGuardStaticAudit(Action<string>? checkpoint = null)
    {
        const int step = 44;
        const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.StaticAuditOrBinding;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep44Prerequisite("Step 44 Gate B entry");
            var baseline = _step44Baseline ?? throw new InvalidOperationException("Step 44.0 Gate A must pass before Gate B.");
            Checkpoint(checkpoint, "M44_B_ENTRY — mapping only AccountScopeUserDataMigrator.HasLegacyData() and ProfileAccountScopeMigrator.HasLegacyData(). This rung is read-only; mutation methods are never invoked.");
            stage = "legacy-data read-only closure audit";
            using var resolver = new RejectingAssemblyResolver();
            using var module = OpenStartupLadderModule(baseline.SelectedPath, resolver);
            var allTypes = EnumerateTypes(module.Types).ToDictionary(type => type.FullName, StringComparer.Ordinal);
            var allMethods = BuildStartupLadderMethodMap(allTypes);
            var roots = new[]
            {
                RequireStartupLadderMethod(RequireStartupLadderType(allTypes, AccountScopeMigratorTypeFullName, step), "HasLegacyData", 0, true, step),
                RequireStartupLadderMethod(RequireStartupLadderType(allTypes, ProfileScopeMigratorTypeFullName, step), "HasLegacyData", 0, true, step),
            };
            if (roots.Any(method => method.ReturnType.FullName != "System.Boolean"))
                throw new InvalidDataException("Step 44.0 HasLegacyData methods no longer return bool.");
            var audit = AuditStartupLadderRoots(roots, allTypes, allMethods);
            RequireStartupLadderAuditAdmissible(audit, "Step 44.0 legacy guard");
            var mutationReachability = audit.ClosureMethods
                .Where(method => method.FullName.Contains("::MigrateTo", StringComparison.Ordinal) ||
                                 method.FullName.Contains("::ArchiveLegacyData(", StringComparison.Ordinal))
                .ToArray();
            if (mutationReachability.Length != 0)
                throw new InvalidDataException("Step 44.0 read-only HasLegacyData closure unexpectedly reaches migration/archive mutation: " + string.Join(" | ", mutationReachability.Select(method => method.FullName)));
            if (resolver.Requests.Count != 0)
                throw new InvalidDataException("Step 44.0 static audit unexpectedly attempted external Cecil resolution: " + string.Join(" | ", resolver.Requests));
            _step44StaticMap = BuildStartupLadderStaticMap(step, Step44Name, baseline, roots, audit,
                "Read-only legacy-data detection only. MigrateTo*/ArchiveLegacyData are not invoked by Step 44.");
            RequireStartupLadderBaselineUnchanged(context, baseline, "Step 44 Gate B");
            Checkpoint(checkpoint, $"M44_B_PASS — roots=2; closureMethods={audit.ClosureMethods.Length}; forbiddenBoundaryRefs=0; unresolvedSameSts2Refs=0; externalResolutionRequests=0; mutation=NO.");
            return StartupLadderPass(step, Step44Name, gate,
                $"Both HasLegacyData closures are admissible and read-only. Combined closure={audit.ClosureMethods.Length}; forbidden/unresolved/external resolution=0.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"M44_B_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return StartupLadderFail(step, Step44Name, gate, stage, ex);
        }
    }

    public TransformedRealStS2StartupLadderGateResult RunStep44LegacyGuardProbe(Action<string>? checkpoint = null)
    {
        const int step = 44;
        const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.ControlledAction;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep44Prerequisite("Step 44 Gate C entry");
            var baseline = _step44Baseline ?? throw new InvalidOperationException("Step 44.0 baseline is absent.");
            if (string.IsNullOrWhiteSpace(_step44StaticMap))
                throw new InvalidOperationException("Step 44.0 requires its verified static map before the read-only probe.");
            Checkpoint(checkpoint, "M44_C_ENTRY — invoking the two audited HasLegacyData() probes. If either reports legacy data, Step 44 fails safely and later ladder rungs remain locked; no migration/archive method is called.");
            stage = "read-only legacy-data runtime probe";
            var admission = RequireAdmission();
            var accountType = admission.Assembly.GetType(AccountScopeMigratorTypeFullName, throwOnError: true, ignoreCase: false)!;
            var profileType = admission.Assembly.GetType(ProfileScopeMigratorTypeFullName, throwOnError: true, ignoreCase: false)!;
            var accountLegacy = InvokeStartupLadderStaticBool(accountType, "HasLegacyData");
            var profileLegacy = InvokeStartupLadderStaticBool(profileType, "HasLegacyData");
            _step44Observation = $"accountLegacy={accountLegacy}; profileLegacy={profileLegacy}; mutationPerformed=NO";
            RequireStartupLadderBaselineUnchanged(context, baseline, "Step 44 Gate C");
            if (accountLegacy || profileLegacy)
                throw new InvalidOperationException("Step 44.0 detected legacy save data and intentionally stopped before mutation. " + _step44Observation);
            _step44NoLegacyDataPassed = true;
            Checkpoint(checkpoint, "M44_C_PASS — " + _step44Observation + "; safe no-migration path established.");
            return StartupLadderPass(step, Step44Name, gate,
                "No legacy account/profile data was detected. Migration/archive work is unnecessary for this launcher sandbox and no mutation occurred.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"M44_C_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return StartupLadderFail(step, Step44Name, gate, stage, ex);
        }
    }

    public TransformedRealStS2StartupLadderGateResult RunStep44FrozenConfinement(bool renderingStopped, Action<string>? checkpoint = null)
        => RunStartupLadderFrozenConfinement(44, Step44Name, renderingStopped, _step44Baseline, _step44NoLegacyDataPassed,
            () => _exactStep44ClosurePassed = true, _step44Observation, checkpoint);

    // STEP 45 — LOCAL SAVE INITIALIZATION

    public TransformedRealStS2StartupLadderGateResult RunStep45ClosedStep44Authority(bool renderingStopped, Action<string>? checkpoint = null)
        => RunStartupLadderSimplePrerequisite(45, Step45Name, renderingStopped, RequireStep45Prerequisite, baseline => _step45Baseline = baseline,
            "M45_A", "same-process Step-44 4/4 no-legacy-data authority", checkpoint);

    public TransformedRealStS2StartupLadderGateResult RunStep45SaveInitializationStaticAudit(Action<string>? checkpoint = null)
    {
        const int step = 45;
        const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.StaticAuditOrBinding;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep45Prerequisite("Step 45 Gate B entry");
            var baseline = _step45Baseline ?? throw new InvalidOperationException("Step 45.0 Gate A must pass before Gate B.");
            Checkpoint(checkpoint, "M45_B_ENTRY — mapping SaveManager.InitProfileId(null), InitProgressData(), and InitPrefsData() in original GameStartup order. Null-platform reads and already-inert SentryService wrappers are admissible; external Steamworks/native/FMOD/Spine/later-startup boundaries are not.");
            stage = "local SaveManager initialization closure audit";
            using var resolver = new RejectingAssemblyResolver();
            using var module = OpenStartupLadderModule(baseline.SelectedPath, resolver);
            var allTypes = EnumerateTypes(module.Types).ToDictionary(type => type.FullName, StringComparer.Ordinal);
            var allMethods = BuildStartupLadderMethodMap(allTypes);
            var saveManager = RequireStartupLadderType(allTypes, SaveManagerTypeFullName, step);
            var mockInstanceField = saveManager.Fields.SingleOrDefault(field => field.Name == SaveManagerMockInstanceFieldName && field.IsStatic && field.FieldType.FullName == SaveManagerTypeFullName)
                ?? throw new MissingFieldException(SaveManagerTypeFullName, SaveManagerMockInstanceFieldName);
            var instanceField = saveManager.Fields.SingleOrDefault(field => field.Name == SaveManagerInstanceFieldName && field.IsStatic && field.FieldType.FullName == SaveManagerTypeFullName)
                ?? throw new MissingFieldException(SaveManagerTypeFullName, SaveManagerInstanceFieldName);
            var getInstance = RequireStartupLadderMethod(saveManager, "get_Instance", 0, true, step);
            var constructDefault = RequireStartupLadderMethod(saveManager, "ConstructDefault", 0, true, step);
            var getterFieldRefs = getInstance.Body.Instructions
                .Where(instruction => instruction.Operand is FieldReference)
                .Select(instruction => (OpCode: instruction.OpCode.Name, Field: (FieldReference)instruction.Operand))
                .Where(item => item.Field.DeclaringType.FullName == SaveManagerTypeFullName)
                .ToArray();
            var mockLoads = getterFieldRefs.Count(item => item.OpCode == "ldsfld" && item.Field.Name == SaveManagerMockInstanceFieldName);
            var instanceLoads = getterFieldRefs.Count(item => item.OpCode == "ldsfld" && item.Field.Name == SaveManagerInstanceFieldName);
            var instanceStores = getterFieldRefs.Count(item => item.OpCode == "stsfld" && item.Field.Name == SaveManagerInstanceFieldName);
            var constructCalls = getInstance.Body.Instructions.Count(instruction =>
                (instruction.OpCode.Name is "call" or "callvirt") &&
                instruction.Operand is MethodReference method &&
                method.DeclaringType.FullName == SaveManagerTypeFullName &&
                method.Name == constructDefault.Name &&
                method.Parameters.Count == 0);
            if (mockLoads != 2 || instanceLoads != 2 || instanceStores != 1 || constructCalls != 1)
                throw new InvalidDataException($"Step 45.0 SaveManager.get_Instance serialized singleton pattern drifted: mockLoads={mockLoads}; instanceLoads={instanceLoads}; instanceStores={instanceStores}; constructDefaultCalls={constructCalls}.");
            var initProfile = RequireStartupLadderMethod(saveManager, "InitProfileId", 1, false, step);
            var initProgress = RequireStartupLadderMethod(saveManager, "InitProgressData", 0, false, step);
            var initPrefs = RequireStartupLadderMethod(saveManager, "InitPrefsData", 0, false, step);
            if (initProfile.ReturnType.FullName != "System.Void" || initProfile.Parameters[0].ParameterType.FullName != "System.Nullable`1<System.Int32>")
                throw new InvalidDataException("Step 45.0 InitProfileId signature drifted from void InitProfileId(Nullable<int>).");
            _ = mockInstanceField;
            _ = instanceField; // Serialized authority: get_Instance is verified to use _mockInstance -> _instance -> ConstructDefault fallback; Step 45 requires production _mockInstance null and reads _instance directly, never invoking the getter/fallback.
            var roots = new[] { initProfile, initProgress, initPrefs };
            var audit = AuditStartupLadderRoots(roots, allTypes, allMethods);
            RequireStartupLadderAuditAdmissible(audit, "Step 45.0 local save initialization");
            if (resolver.Requests.Count != 0)
                throw new InvalidDataException("Step 45.0 static audit unexpectedly attempted external Cecil resolution: " + string.Join(" | ", resolver.Requests));
            _step45StaticMap = BuildStartupLadderStaticMap(step, Step45Name, baseline, roots, audit,
                "Exact local SaveManager profile/progress/prefs initialization; cloud sync and migration methods remain uninvoked.")
                + Environment.NewLine
                + $"SaveManager singleton metadata: mockField={SaveManagerMockInstanceFieldName}; instanceField={SaveManagerInstanceFieldName}; get_Instance mockLoads={mockLoads}; instanceLoads={instanceLoads}; instanceStores={instanceStores}; ConstructDefaultCalls={constructCalls}. Step 45 reads _instance directly and never invokes get_Instance/ConstructDefault.";
            RequireStartupLadderBaselineUnchanged(context, baseline, "Step 45 Gate B");
            Checkpoint(checkpoint, $"M45_B_PASS — roots=3; closureMethods={audit.ClosureMethods.Length}; classifiedBoundaryRefs={audit.Boundaries.Length}; forbiddenBoundaryRefs=0; unresolvedSameSts2Refs=0; externalResolutionRequests=0; invocation=NO.");
            return StartupLadderPass(step, Step45Name, gate,
                $"SaveManager local-init audit passed. Combined closure={audit.ClosureMethods.Length}; all classified boundaries are limited to proven Null-platform reads/inert Sentry wrappers.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"M45_B_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return StartupLadderFail(step, Step45Name, gate, stage, ex);
        }
    }

    public TransformedRealStS2StartupLadderGateResult RunStep45ControlledSaveInitialization(Action<string>? checkpoint = null)
    {
        const int step = 45;
        const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.ControlledAction;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep45Prerequisite("Step 45 Gate C entry");
            var baseline = _step45Baseline ?? throw new InvalidOperationException("Step 45.0 baseline is absent.");
            if (string.IsNullOrWhiteSpace(_step45StaticMap))
                throw new InvalidOperationException("Step 45.0 requires its verified static map before invocation.");
            if (_step45InvocationStarted)
                throw new InvalidOperationException("Step 45.0 local save initialization is one-shot and cannot be retried in-process.");
            _step45InvocationStarted = true;
            Checkpoint(checkpoint, "M45_C_ARMED — first and only Step-45 local save initialization authorized. Invoking InitProfileId(null) -> InitProgressData() -> InitPrefsData() exactly once; cloud/migration/Steam/main-menu work remains unopened.");
            stage = "controlled local SaveManager initialization";

            var stateBefore = RequireStep40InsertedAuthority();
            var admission = RequireAdmission();
            var saveType = admission.Assembly.GetType(SaveManagerTypeFullName, throwOnError: true, ignoreCase: false)
                ?? throw new MissingMemberException(SaveManagerTypeFullName);
            var instance = RequireExistingStartupLadderSaveManagerInstance(saveType, step);
            var initProfile = saveType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .SingleOrDefault(method => method.Name == "InitProfileId" && method.GetParameters().Length == 1 && Nullable.GetUnderlyingType(method.GetParameters()[0].ParameterType) == typeof(int) && method.ReturnType == typeof(void))
                ?? throw new MissingMethodException(SaveManagerTypeFullName, "InitProfileId(Nullable<int>)");
            var initProgress = saveType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .SingleOrDefault(method => method.Name == "InitProgressData" && method.GetParameters().Length == 0)
                ?? throw new MissingMethodException(SaveManagerTypeFullName, "InitProgressData()");
            var initPrefs = saveType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .SingleOrDefault(method => method.Name == "InitPrefsData" && method.GetParameters().Length == 0)
                ?? throw new MissingMethodException(SaveManagerTypeFullName, "InitPrefsData()");

            InvokeStartupLadderMethod(initProfile, instance, new object?[] { null }, "Step 45 InitProfileId");
            var progressResult = InvokeStartupLadderMethod(initProgress, instance, null, "Step 45 InitProgressData");
            var prefsResult = InvokeStartupLadderMethod(initPrefs, instance, null, "Step 45 InitPrefsData");
            if (progressResult is null || prefsResult is null)
                throw new InvalidDataException("Step 45.0 local save initialization returned a null ReadSaveResult.");

            var stateAfter = RequireStep40InsertedAuthority();
            var hostDelta = RequireStep45PlannedHostOnlyDelta(context, baseline, "Step 45 Gate C");
            if (stateAfter != stateBefore)
                throw new InvalidDataException($"Step 45.0 local save initialization changed OneTimeInitialization state: {stateBefore} -> {stateAfter}.");
            _step45AcceptedHostBinding = hostDelta.HostEntry ?? "<none>";
            _step45PostActionBaseline = CaptureStartupLadderBaseline(baseline.SelectedPath, baseline.SelectedSha256, context);
            _step45Observation = "progress={" + DescribeStartupLadderReadSaveResult(progressResult) + "}; prefs={" + DescribeStartupLadderReadSaveResult(prefsResult) + "}; plannedHostBinding={" + _step45AcceptedHostBinding + "}";
            _step45SaveInitPassed = true;
            Checkpoint(checkpoint, $"M45_C_PASS — local SaveManager sequence returned; {_step45Observation}; state=2; resolverDelta={hostDelta.ResolverDelta}; hostDelta={hostDelta.HostDelta}; privateDelta=0; initializerDelta=0; rejectedDelta=0; nativeDelta=0; rendering remains stopped.");
            return StartupLadderPass(step, Step45Name, gate,
                "Exact local SaveManager initialization returned once in original profile/progress/prefs order. " + _step45Observation + ". Only zero-or-one exact planned host-framework binding is admissible; private/initializer/rejected/native deltas remain zero.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"M45_C_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return StartupLadderFail(step, Step45Name, gate, stage, ex);
        }
    }

    public TransformedRealStS2StartupLadderGateResult RunStep45FrozenSaveConfinement(bool renderingStopped, Action<string>? checkpoint = null)
    {
        const int step = 45;
        const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.PostActionConfinement;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep45Prerequisite("Step 45 Gate D entry");
            var baseline = _step45Baseline ?? throw new InvalidOperationException("Step 45.0 baseline is absent.");
            var postActionBaseline = _step45PostActionBaseline ?? throw new InvalidOperationException("Step 45.0 post-action baseline is absent.");
            if (!_step45InvocationStarted || !_step45SaveInitPassed)
                throw new InvalidOperationException("Step 45.0 Gate D requires the one-shot local save initialization to have returned successfully.");
            if (!renderingStopped)
                throw new InvalidOperationException("Step 45.0 Gate D requires rendering to remain frozen.");
            Checkpoint(checkpoint, "M45_D_ENTRY — proving SaveManager settings/prefs/progress authority is non-null after local initialization while NGame/state/context remain confined and rendering stays frozen.");
            stage = "post-save frozen confinement";
            var state = RequireStep40InsertedAuthority();
            var admission = RequireAdmission();
            var saveType = admission.Assembly.GetType(SaveManagerTypeFullName, throwOnError: true, ignoreCase: false)!;
            var instance = RequireExistingStartupLadderSaveManagerInstance(saveType, step);
            var settings = RequireStartupLadderPropertyValue(instance, "SettingsSave", step);
            var prefs = RequireStartupLadderPropertyValue(instance, "PrefsSave", step);
            var progress = RequireStartupLadderPropertyValue(instance, "Progress", step);
            RequireStep45PlannedHostOnlyDelta(context, baseline, "Step 45 Gate D cumulative");
            RequireStartupLadderBaselineUnchanged(context, postActionBaseline, "Step 45 Gate D post-action");
            _exactStep45ClosurePassed = true;
            Checkpoint(checkpoint, $"M45_D_PASS — renderingStopped=True; state={state}; SettingsSave={settings.GetType().FullName}; PrefsSave={prefs.GetType().FullName}; Progress={progress.GetType().FullName}; acceptedHostBinding={SanitizeCheckpoint(_step45AcceptedHostBinding)}; no further resolver/host/private/initializer/rejected/native drift after Gate C.");
            return StartupLadderPass(step, Step45Name, gate,
                $"Frozen save confinement passed. SettingsSave={settings.GetType().Name}; PrefsSave={prefs.GetType().Name}; Progress={progress.GetType().Name}; accepted planned host binding={_step45AcceptedHostBinding}; NGame/state/context remain authoritative with no post-action drift.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"M45_D_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return StartupLadderFail(step, Step45Name, gate, stage, ex);
        }
    }

    // STEP 46 — LAUNCHMAINMENU ASYNC MAP (NO INVOCATION)

    public TransformedRealStS2StartupLadderGateResult RunStep46ClosedStep45Authority(bool renderingStopped, Action<string>? checkpoint = null)
        => RunStartupLadderSimplePrerequisite(46, Step46Name, renderingStopped, RequireStep46Prerequisite, baseline => _step46Baseline = baseline,
            "M46_A", "same-process Step-45 4/4 local-save authority", checkpoint);

    public TransformedRealStS2StartupLadderGateResult RunStep46LaunchMainMenuStateMachineMap(Action<string>? checkpoint = null)
    {
        const int step = 46;
        const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.StaticAuditOrBinding;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep46Prerequisite("Step 46 Gate B entry");
            var baseline = _step46Baseline ?? throw new InvalidOperationException("Step 46.0 Gate A must pass before Gate B.");
            Checkpoint(checkpoint, "M46_B_ENTRY — locating exact NGame.LaunchMainMenu(bool) without invocation and resolving its compiler AsyncStateMachineAttribute + MoveNext body/fields inside the selected sts2 module. Rendering remains frozen.");
            stage = "exact LaunchMainMenu async state-machine map";
            using var resolver = new RejectingAssemblyResolver();
            using var module = OpenStartupLadderModule(baseline.SelectedPath, resolver);
            var allTypes = EnumerateTypes(module.Types).ToDictionary(type => type.FullName, StringComparer.Ordinal);
            var nGame = RequireStartupLadderType(allTypes, NGameTypeFullName, step);
            var launch = nGame.Methods.SingleOrDefault(method => method.Name == NGameLaunchMainMenuMethodName && !method.IsStatic && method.Parameters.Count == 1 && method.Parameters[0].ParameterType.FullName == "System.Boolean" && method.HasBody)
                ?? throw new MissingMethodException(NGameTypeFullName, "LaunchMainMenu(bool)");
            var asyncAttribute = launch.CustomAttributes.SingleOrDefault(attribute => attribute.AttributeType.FullName == Step46AsyncStateMachineAttributeFullName)
                ?? throw new InvalidDataException("Step 46.0 LaunchMainMenu is missing AsyncStateMachineAttribute.");
            if (asyncAttribute.ConstructorArguments.Count != 1 || asyncAttribute.ConstructorArguments[0].Value is not TypeReference stateMachineReference)
                throw new InvalidDataException("Step 46.0 LaunchMainMenu AsyncStateMachineAttribute did not contain one TypeReference.");
            if (!allTypes.TryGetValue(stateMachineReference.FullName, out var stateMachineType))
                throw new InvalidDataException($"Step 46.0 LaunchMainMenu state-machine type is not in the selected sts2 module: {stateMachineReference.FullName}.");
            var moveNext = stateMachineType.Methods.SingleOrDefault(method => method.Name == "MoveNext" && method.Parameters.Count == 0 && method.HasBody)
                ?? throw new MissingMethodException(stateMachineType.FullName, "MoveNext()");
            if (resolver.Requests.Count != 0)
                throw new InvalidDataException("Step 46.0 state-machine map unexpectedly attempted external Cecil resolution: " + string.Join(" | ", resolver.Requests));

            _step46LaunchMethodToken = launch.MetadataToken.ToUInt32();
            _step46MoveNextToken = moveNext.MetadataToken.ToUInt32();
            _step46StateMachineTypeName = stateMachineType.FullName;
            _step46StaticMap = BuildStartupLadderAsyncDirectMap(step, Step46Name, baseline, launch, stateMachineType, moveNext);
            _step46StateMachineMapped = true;
            RequireStartupLadderBaselineUnchanged(context, baseline, "Step 46 Gate B");
            Checkpoint(checkpoint, $"M46_B_PASS — LaunchMainMenu token=0x{_step46LaunchMethodToken:X8}; asyncStateMachine={_step46StateMachineTypeName}; MoveNext token=0x{_step46MoveNextToken:X8}; MoveNextIL={moveNext.Body.Instructions.Count}; externalResolutionRequests=0; invocation=NO.");
            return StartupLadderPass(step, Step46Name, gate,
                $"Exact LaunchMainMenu async state machine mapped without invocation. Launch token=0x{_step46LaunchMethodToken:X8}; stateMachine={_step46StateMachineTypeName}; MoveNext token=0x{_step46MoveNextToken:X8}.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"M46_B_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return StartupLadderFail(step, Step46Name, gate, stage, ex);
        }
    }

    public TransformedRealStS2StartupLadderGateResult RunStep46LaunchMainMenuClosureAudit(Action<string>? checkpoint = null)
    {
        const int step = 46;
        const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.ControlledAction;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep46Prerequisite("Step 46 Gate C entry");
            var baseline = _step46Baseline ?? throw new InvalidOperationException("Step 46.0 baseline is absent.");
            if (!_step46StateMachineMapped || _step46MoveNextToken == 0 || string.IsNullOrWhiteSpace(_step46StaticMap))
                throw new InvalidOperationException("Step 46.0 Gate B state-machine map must pass before Gate C.");
            Checkpoint(checkpoint, "M46_C_ENTRY — mapping LaunchMainMenu.MoveNext with execution-opcode-qualified traversal. call/callvirt/newobj/jmp edges are traversed; ldftn/ldvirtftn/ldtoken method references are recorded as deferred frontiers and never promoted into the immediate execution closure. Original LaunchMainMenu remains uninvoked regardless of mapped boundaries.");
            stage = "LaunchMainMenu immediate/deferred frontier map";
            using var resolver = new RejectingAssemblyResolver();
            using var module = OpenStartupLadderModule(baseline.SelectedPath, resolver);
            var allTypes = EnumerateTypes(module.Types).ToDictionary(type => type.FullName, StringComparer.Ordinal);
            var allMethods = BuildStartupLadderMethodMap(allTypes);
            var moveNext = allTypes.Values.SelectMany(type => type.Methods)
                .SingleOrDefault(method => method.MetadataToken.ToUInt32() == _step46MoveNextToken)
                ?? throw new InvalidDataException($"Step 46.0 could not relocate MoveNext token 0x{_step46MoveNextToken:X8}.");
            var audit = AuditStartupLadderInvocationFrontier(new[] { moveNext }, allTypes, allMethods);
            if (audit.UnresolvedSameAssemblyReferences.Length != 0)
                throw new InvalidDataException("Step 46.0 immediate/deferred frontier map has unresolved same-sts2 references: " + string.Join(" | ", audit.UnresolvedSameAssemblyReferences));
            if (resolver.Requests.Count != 0)
                throw new InvalidDataException("Step 46.0 frontier map unexpectedly attempted external Cecil resolution: " + string.Join(" | ", resolver.Requests));

            _step46StaticMap += BuildStartupLadderInvocationFrontierAppendix(audit);
            _step46ClosureMapped = true;
            _step46LaunchAdmissible = false;
            RequireStartupLadderBaselineUnchanged(context, baseline, "Step 46 Gate C");
            Checkpoint(checkpoint, $"M46_C_PASS — immediateClosureMethods={audit.ImmediateClosureMethods.Length}; compilerStateMachineExpansions={audit.StateMachineExpansions.Length}; immediateBoundaryRefs={audit.ImmediateBoundaries.Length}; deferredMethodFrontiers={audit.DeferredMethodFrontiers.Length}; unresolvedSameSts2Refs=0; externalResolutionRequests=0; LaunchMainMenu invoked=NO; rendering remains stopped.");
            return StartupLadderPass(step, Step46Name, gate,
                $"Execution-opcode-qualified LaunchMainMenu frontier mapped without invocation: immediate methods={audit.ImmediateClosureMethods.Length}; state-machine expansions={audit.StateMachineExpansions.Length}; immediate classified boundaries={audit.ImmediateBoundaries.Length}; deferred method frontiers={audit.DeferredMethodFrontiers.Length}; unresolved/external resolution=0. This map is evidence only and does not authorize original LaunchMainMenu.");
        }
        catch (Exception ex)
        {
            _step46LaunchAdmissible = false;
            Checkpoint(checkpoint, $"M46_C_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return StartupLadderFail(step, Step46Name, gate, stage, ex);
        }
    }

    public TransformedRealStS2StartupLadderGateResult RunStep46FrozenNoInvocationConfinement(bool renderingStopped, Action<string>? checkpoint = null)
    {
        const int step = 46;
        const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.PostActionConfinement;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep46Prerequisite("Step 46 Gate D entry");
            var baseline = _step46Baseline ?? throw new InvalidOperationException("Step 46.0 baseline is absent.");
            if (!_step46StateMachineMapped || !_step46ClosureMapped)
                throw new InvalidOperationException("Step 46.0 Gate D requires a complete immediate/deferred LaunchMainMenu frontier map.");
            if (!_step46StaticMapDurablyWritten)
                throw new InvalidOperationException("Step 46.0 Gate D requires the complete LaunchMainMenu static map to be durably written before closure.");
            if (!renderingStopped)
                throw new InvalidOperationException("Step 46.0 never restarts rendering; Gate D requires it to remain frozen.");
            Checkpoint(checkpoint, "M46_D_ENTRY — proving the complete LaunchMainMenu map caused no runtime drift or invocation; renderer remains frozen and NGame/state/context remain authoritative.");
            stage = "frozen LaunchMainMenu no-invocation confinement";
            var state = RequireStep40InsertedAuthority();
            RequireStartupLadderBaselineUnchanged(context, baseline, "Step 46 Gate D");
            _exactStep46ClosurePassed = true;
            Checkpoint(checkpoint, $"M46_D_PASS — renderingStopped=True; state={state}; LaunchMainMenu invoked=NO; launchAuthorized=NO; staticMapDurable=True; resolver/host/private/initializer/rejected/native deltas=0.");
            return StartupLadderPass(step, Step46Name, gate,
                "Complete execution-opcode-qualified LaunchMainMenu frontier map is durable. Rendering stayed frozen, original LaunchMainMenu was not invoked or authorized, and context/native authority is unchanged.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"M46_D_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return StartupLadderFail(step, Step46Name, gate, stage, ex);
        }
    }

    // Shared ladder helpers

    private Step35ExecutionLoadContext RequireStep43Prerequisite(string boundary)
    {
        var context = RequireStep42Prerequisite(boundary);
        if (!_exactStep42ClosurePassed || !_step42InvocationPassed)
            throw new InvalidOperationException($"{boundary} requires same-process Step-42 4/4 authority with exact InitPools already returned once.");
        return context;
    }

    private Step35ExecutionLoadContext RequireStep44Prerequisite(string boundary)
    {
        var context = RequireStep43Prerequisite(boundary);
        if (!_exactStep43ClosurePassed || !_step43ActionPassed)
            throw new InvalidOperationException($"{boundary} requires same-process Step-43 4/4 Null-platform authority.");
        return context;
    }

    private Step35ExecutionLoadContext RequireStep45Prerequisite(string boundary)
    {
        var context = RequireStep44Prerequisite(boundary);
        if (!_exactStep44ClosurePassed || !_step44NoLegacyDataPassed)
            throw new InvalidOperationException($"{boundary} requires same-process Step-44 4/4 no-legacy-data authority.");
        return context;
    }

    private Step35ExecutionLoadContext RequireStep46Prerequisite(string boundary)
    {
        var context = RequireStep45Prerequisite(boundary);
        if (!_exactStep45ClosurePassed || !_step45SaveInitPassed)
            throw new InvalidOperationException($"{boundary} requires same-process Step-45 4/4 local-save initialization authority.");
        return context;
    }

    private Step35ExecutionLoadContext RequireStep47Prerequisite(string boundary)
    {
        var context = RequireStep46Prerequisite(boundary);
        if (!_exactStep46ClosurePassed || !_step46ClosureMapped || !_step46StaticMapDurablyWritten)
            throw new InvalidOperationException($"{boundary} requires same-process Step-46 4/4 durable immediate/deferred LaunchMainMenu frontier-map authority.");
        return context;
    }

    private (string Path, string Sha256) RequireStartupLadderSelectedAuthority(int step)
    {
        var step39 = _step39Preflight ?? throw new InvalidOperationException($"Step {step}.0 requires retained Step-39 selected-image authority.");
        var actual = ComputeSha256Hex(step39.SelectedPath);
        if (!actual.Equals(step39.SelectedSha256, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException($"Step {step}.0 selected compatibility image hash drifted. expected={step39.SelectedSha256}; actual={actual}.");
        return (step39.SelectedPath, step39.SelectedSha256);
    }

    private static StartupLadderBaseline CaptureStartupLadderBaseline(string selectedPath, string selectedSha256, Step35ExecutionLoadContext context)
        => new(selectedPath, selectedSha256, context.ManagedResolverRequests.Count, context.HostLoads.Count, context.PrivateLoads.Count,
            context.InitializerBearingRequests.Count, context.RejectedManagedRequests.Count, context.NativeLoadAttempts.Count);

    private static StartupLadderHostDelta RequireStep45PlannedHostOnlyDelta(Step35ExecutionLoadContext context, StartupLadderBaseline baseline, string boundary)
    {
        var resolverDelta = context.ManagedResolverRequests.Skip(baseline.ResolverCount).ToArray();
        var hostDelta = context.HostLoads.Skip(baseline.HostCount).ToArray();
        var privateDelta = context.PrivateLoads.Skip(baseline.PrivateCount).ToArray();
        var initializerDelta = context.InitializerBearingRequests.Skip(baseline.InitializerCount).ToArray();
        var rejectedDelta = context.RejectedManagedRequests.Skip(baseline.RejectedCount).ToArray();
        var nativeDelta = context.NativeLoadAttempts.Skip(baseline.NativeCount).ToArray();

        if (privateDelta.Length != 0 || initializerDelta.Length != 0 || rejectedDelta.Length != 0 || nativeDelta.Length != 0)
            throw new InvalidDataException($"{boundary} crossed a forbidden resolver boundary. resolver={resolverDelta.Length}; host={hostDelta.Length}; private={privateDelta.Length}; initializer={initializerDelta.Length}; rejected={rejectedDelta.Length}; native={nativeDelta.Length}; hostEntries={FormatDelta(hostDelta)}.");
        if (resolverDelta.Length != hostDelta.Length || hostDelta.Length > 1)
            throw new InvalidDataException($"{boundary} permits only zero-or-one paired exact planned host-framework binding. resolver={resolverDelta.Length}; host={hostDelta.Length}; resolverEntries={FormatDelta(resolverDelta)}; hostEntries={FormatDelta(hostDelta)}.");
        if (hostDelta.Length == 0)
            return new StartupLadderHostDelta(0, 0, null);

        const string separator = " => ";
        var split = hostDelta[0].IndexOf(separator, StringComparison.Ordinal);
        if (split <= 0 || split + separator.Length >= hostDelta[0].Length)
            throw new InvalidDataException($"{boundary} host-load diagnostic shape drifted: {hostDelta[0]}");
        var requested = hostDelta[0][..split];
        if (!requested.Equals(resolverDelta[0], StringComparison.Ordinal))
            throw new InvalidDataException($"{boundary} resolver/host request mismatch. resolver={resolverDelta[0]}; hostRequested={requested}.");
        var requestedName = new AssemblyName(requested).Name
            ?? throw new InvalidDataException($"{boundary} host request has no simple assembly name: {requested}");
        if (!IsHostFrameworkContractName(requestedName))
            throw new InvalidDataException($"{boundary} observed a non-framework host binding: {hostDelta[0]}");

        // Step35ExecutionLoadContext records HostLoads only after exact persisted host-binding identity
        // matching and exact planned actual-identity verification. Therefore this single entry is not
        // an arbitrary fallback: it is an already fail-closed host-framework admission.
        return new StartupLadderHostDelta(1, 1, hostDelta[0]);
    }

    private static void RequireStartupLadderBaselineUnchanged(Step35ExecutionLoadContext context, StartupLadderBaseline baseline, string boundary)
    {
        if (context.ManagedResolverRequests.Count != baseline.ResolverCount ||
            context.HostLoads.Count != baseline.HostCount ||
            context.PrivateLoads.Count != baseline.PrivateCount ||
            context.InitializerBearingRequests.Count != baseline.InitializerCount ||
            context.RejectedManagedRequests.Count != baseline.RejectedCount ||
            context.NativeLoadAttempts.Count != baseline.NativeCount)
        {
            throw new InvalidDataException($"{boundary} context drifted. resolver={context.ManagedResolverRequests.Count - baseline.ResolverCount}; host={context.HostLoads.Count - baseline.HostCount}; private={context.PrivateLoads.Count - baseline.PrivateCount}; initializer={context.InitializerBearingRequests.Count - baseline.InitializerCount}; rejected={context.RejectedManagedRequests.Count - baseline.RejectedCount}; native={context.NativeLoadAttempts.Count - baseline.NativeCount}.");
        }
    }

    private TransformedRealStS2StartupLadderGateResult RunStartupLadderSimplePrerequisite(
        int step,
        string stepName,
        bool renderingStopped,
        Func<string, Step35ExecutionLoadContext> prerequisite,
        Action<StartupLadderBaseline> capture,
        string checkpointPrefix,
        string authorityDescription,
        Action<string>? checkpoint)
    {
        const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.PrerequisiteAuthority;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = prerequisite($"Step {step} Gate A entry");
            if (!renderingStopped)
                throw new InvalidOperationException($"Step {step}.0 requires rendering to remain frozen at Gate A.");
            Checkpoint(checkpoint, $"{checkpointPrefix}_ENTRY — requiring {authorityDescription}, retained real NGame/state=2, frozen rendering, and unchanged selected compatibility/context authority.");
            stage = authorityDescription + " re-verification";
            var state = RequireStep40InsertedAuthority();
            var selected = RequireStartupLadderSelectedAuthority(step);
            var baseline = CaptureStartupLadderBaseline(selected.Path, selected.Sha256, context);
            capture(baseline);
            RequireStartupLadderBaselineUnchanged(context, baseline, $"Step {step} Gate A");
            Checkpoint(checkpoint, $"{checkpointPrefix}_PASS — prerequisite retained; renderingStopped=True; state={state}; selectedSha256={selected.Sha256}; resolver/host/private/initializer/rejected/native deltas=0.");
            return StartupLadderPass(step, stepName, gate,
                $"{authorityDescription} retained with frozen rendering, state={state}, exact selected bytes, and zero context/native drift.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"{checkpointPrefix}_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return StartupLadderFail(step, stepName, gate, stage, ex);
        }
    }

    private TransformedRealStS2StartupLadderGateResult RunStartupLadderFrozenConfinement(
        int step,
        string stepName,
        bool renderingStopped,
        StartupLadderBaseline? baseline,
        bool actionPassed,
        Action markPassed,
        string observation,
        Action<string>? checkpoint)
    {
        const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.PostActionConfinement;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = step switch
            {
                43 => RequireStep43Prerequisite("Step 43 Gate D entry"),
                44 => RequireStep44Prerequisite("Step 44 Gate D entry"),
                _ => throw new InvalidOperationException($"Unsupported frozen ladder confinement step {step}."),
            };
            var authority = baseline ?? throw new InvalidOperationException($"Step {step}.0 baseline is absent.");
            if (!actionPassed)
                throw new InvalidOperationException($"Step {step}.0 Gate D requires Gate C to pass.");
            if (!renderingStopped)
                throw new InvalidOperationException($"Step {step}.0 Gate D requires rendering to remain frozen.");
            Checkpoint(checkpoint, $"M{step}_D_ENTRY — proving frozen post-action NGame/state/context confinement with exact selected bytes unchanged.");
            stage = "frozen post-action confinement";
            var state = RequireStep40InsertedAuthority();
            var selected = RequireStartupLadderSelectedAuthority(step);
            if (!selected.Sha256.Equals(authority.SelectedSha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException($"Step {step}.0 selected image authority changed after Gate A.");
            RequireStartupLadderBaselineUnchanged(context, authority, $"Step {step} Gate D");
            markPassed();
            Checkpoint(checkpoint, $"M{step}_D_PASS — renderingStopped=True; state={state}; {observation}; resolver/host/private/initializer/rejected/native deltas=0.");
            return StartupLadderPass(step, stepName, gate,
                $"Frozen confinement passed. State={state}; {observation}; selected bytes/context/native authority unchanged.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"M{step}_D_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return StartupLadderFail(step, stepName, gate, stage, ex);
        }
    }

    private static ModuleDefinition OpenStartupLadderModule(string path, RejectingAssemblyResolver resolver)
        => ModuleDefinition.ReadModule(path, new ReaderParameters
        {
            ReadSymbols = false,
            ReadingMode = ReadingMode.Deferred,
            InMemory = true,
            AssemblyResolver = resolver,
            MetadataResolver = new MetadataResolver(resolver),
        });

    private static IReadOnlyDictionary<string, MethodDefinition> BuildStartupLadderMethodMap(IReadOnlyDictionary<string, TypeDefinition> allTypes)
        => allTypes.Values.SelectMany(type => type.Methods)
            .GroupBy(method => method.FullName, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Single(), StringComparer.Ordinal);

    private static TypeDefinition RequireStartupLadderType(IReadOnlyDictionary<string, TypeDefinition> allTypes, string fullName, int step)
        => allTypes.TryGetValue(fullName, out var type)
            ? type
            : throw new InvalidDataException($"Step {step}.0 could not locate {fullName} in the selected compatibility image.");

    private static MethodDefinition RequireStartupLadderMethod(TypeDefinition type, string name, int parameterCount, bool isStatic, int step)
    {
        var candidates = type.Methods.Where(method => method.Name == name && method.Parameters.Count == parameterCount && method.IsStatic == isStatic && method.HasBody).ToArray();
        return candidates.SingleOrDefault()
            ?? throw new MissingMethodException(type.FullName, $"{name} with parameterCount={parameterCount}, static={isStatic}; candidates={string.Join(",", candidates.Select(method => method.FullName))}");
    }

    private static StartupLadderAudit AuditStartupLadderRoots(
        IEnumerable<MethodDefinition> roots,
        IReadOnlyDictionary<string, TypeDefinition> allTypes,
        IReadOnlyDictionary<string, MethodDefinition> allMethods)
    {
        var closure = new SortedDictionary<uint, Step41ClosureMethod>();
        var boundaries = new SortedDictionary<string, Step41BoundaryObservation>(StringComparer.Ordinal);
        var unresolved = new SortedSet<string>(StringComparer.Ordinal);
        var expansions = new SortedSet<string>(StringComparer.Ordinal);
        var methodsByToken = allTypes.Values.SelectMany(type => type.Methods)
            .ToDictionary(method => method.MetadataToken.ToUInt32());
        var pendingRoots = new Queue<MethodDefinition>(roots);
        var auditedRootTokens = new HashSet<uint>();

        while (pendingRoots.Count != 0)
        {
            var root = pendingRoots.Dequeue();
            if (!auditedRootTokens.Add(root.MetadataToken.ToUInt32()))
                continue;

            var audit = AuditStep41StartupClosure(root, allTypes, allMethods);
            foreach (var method in audit.ClosureMethods)
                closure.TryAdd(method.Token, method);
            foreach (var boundary in audit.Boundaries)
                boundaries.TryAdd(boundary.Category + "|" + boundary.ReferenceFullName + "|" + boundary.Path, boundary);
            foreach (var item in audit.UnresolvedSameAssemblyReferences)
                unresolved.Add(item);

            foreach (var closureMethod in audit.ClosureMethods)
            {
                if (!methodsByToken.TryGetValue(closureMethod.Token, out var methodDefinition))
                {
                    unresolved.Add($"state-machine expansion could not relocate token 0x{closureMethod.Token:X8}: {closureMethod.FullName}");
                    continue;
                }

                var stateMachineAttributes = methodDefinition.CustomAttributes
                    .Where(attribute => StartupLadderCompilerStateMachineAttributes.Contains(attribute.AttributeType.FullName, StringComparer.Ordinal))
                    .ToArray();
                if (stateMachineAttributes.Length == 0)
                    continue;
                if (stateMachineAttributes.Length != 1)
                {
                    unresolved.Add($"{methodDefinition.FullName} has {stateMachineAttributes.Length} compiler state-machine attributes");
                    continue;
                }

                var attribute = stateMachineAttributes[0];
                if (attribute.ConstructorArguments.Count != 1 || attribute.ConstructorArguments[0].Value is not TypeReference stateMachineReference)
                {
                    unresolved.Add($"{methodDefinition.FullName} has malformed {attribute.AttributeType.FullName}");
                    continue;
                }

                var stateMachineTypeName = GetStep41DefinitionTypeName(stateMachineReference);
                if (!allTypes.TryGetValue(stateMachineTypeName, out var stateMachineType))
                {
                    unresolved.Add($"{methodDefinition.FullName} compiler state-machine type is not in selected sts2 module: {stateMachineTypeName}");
                    continue;
                }

                var moveNextCandidates = stateMachineType.Methods
                    .Where(method => method.Name == "MoveNext" && method.Parameters.Count == 0 && method.HasBody)
                    .ToArray();
                if (moveNextCandidates.Length != 1)
                {
                    unresolved.Add($"{methodDefinition.FullName} compiler state-machine {stateMachineTypeName} has MoveNext candidates={moveNextCandidates.Length}");
                    continue;
                }

                var moveNext = moveNextCandidates[0];
                expansions.Add($"{attribute.AttributeType.Name}: {methodDefinition.FullName} -> {moveNext.FullName}");
                pendingRoots.Enqueue(moveNext);
            }
        }

        return new StartupLadderAudit(closure.Values.ToArray(), boundaries.Values.ToArray(), unresolved.ToArray(), expansions.ToArray());
    }

    private static void RequireStartupLadderAuditAdmissible(StartupLadderAudit audit, string boundary)
    {
        if (audit.UnresolvedSameAssemblyReferences.Length != 0)
            throw new InvalidDataException(boundary + " has unresolved same-sts2 references: " + string.Join(" | ", audit.UnresolvedSameAssemblyReferences));
        var forbidden = audit.Boundaries.Where(item => !IsStartupLadderBoundaryAllowed(item)).ToArray();
        if (forbidden.Length != 0)
            throw new InvalidDataException(boundary + " reaches unapproved boundaries: " + string.Join(" | ", forbidden.Select(item => item.Category + ": " + item.Path)));
    }

    private static bool IsStartupLadderBoundaryAllowed(Step41BoundaryObservation boundary)
    {
        if (boundary.Category == "SENTRY_INERT_WRAPPER")
            return true;
        if (boundary.Category is "ONE_TIME_INITIALIZATION" or "INITIALIZE_PLATFORM" or "LAUNCH_MAIN_MENU" or
            "LOAD_DEFERRED_STARTUP_ASSETS" or "STEAM" or "FMOD" or "SPINE" or "SENTRY_EXTERNAL" or "NATIVE_EXTENSION")
            return false;
        if (boundary.Category != "PLATFORM")
            return false;
        var reference = boundary.ReferenceFullName;
        if (reference.Contains(" MegaCrit.Sts2.Core.Platform.Null.", StringComparison.Ordinal))
            return true;
        if (reference.Contains(" MegaCrit.Sts2.Core.Platform.PlatformUtil::get_PrimaryPlatform()", StringComparison.Ordinal) ||
            reference.Contains(" MegaCrit.Sts2.Core.Platform.PlatformUtil::GetPlatformUtil(MegaCrit.Sts2.Core.Platform.PlatformType)", StringComparison.Ordinal) ||
            reference.Contains(" MegaCrit.Sts2.Core.Platform.PlatformUtil::GetLocalPlayerId(MegaCrit.Sts2.Core.Platform.PlatformType)", StringComparison.Ordinal) ||
            reference.Contains(" MegaCrit.Sts2.Core.Platform.PlatformUtil::GetPlatformBranch()", StringComparison.Ordinal) ||
            reference.Contains(" MegaCrit.Sts2.Core.Platform.PlatformUtil::GetThreeLetterLanguageCode()", StringComparison.Ordinal) ||
            reference.Contains(" MegaCrit.Sts2.Core.Platform.PlatformUtil::GetRawLanguage()", StringComparison.Ordinal) ||
            reference.Contains(" MegaCrit.Sts2.Core.Platform.PlatformUtil::GetSupportedWindowMode()", StringComparison.Ordinal) ||
            reference.Contains(" MegaCrit.Sts2.Core.Platform.IPlatformUtilStrategy::GetLocalPlayerId()", StringComparison.Ordinal) ||
            reference.Contains(" MegaCrit.Sts2.Core.Platform.IPlatformUtilStrategy::GetPlatformBranch()", StringComparison.Ordinal) ||
            reference.Contains(" MegaCrit.Sts2.Core.Platform.IPlatformUtilStrategy::GetThreeLetterLanguageCode()", StringComparison.Ordinal) ||
            reference.Contains(" MegaCrit.Sts2.Core.Platform.IPlatformUtilStrategy::GetRawLanguage()", StringComparison.Ordinal) ||
            reference.Contains(" MegaCrit.Sts2.Core.Platform.IPlatformUtilStrategy::GetSupportedWindowMode()", StringComparison.Ordinal) ||
            reference.Contains(" MegaCrit.Sts2.Core.Platform.PlatformBranchExtensions::ToName(", StringComparison.Ordinal) ||
            reference.Contains(" MegaCrit.Sts2.Core.Platform.SupportedWindowModeExtensions::ShouldForceFullscreen(", StringComparison.Ordinal) ||
            reference.Contains(" MegaCrit.Sts2.Core.Platform.Steam.SteamInitializer::get_Initialized()", StringComparison.Ordinal))
            return true;
        return false;
    }

    private static string BuildStartupLadderStaticMap(
        int step,
        string stepName,
        StartupLadderBaseline baseline,
        IEnumerable<MethodDefinition> roots,
        StartupLadderAudit audit,
        string policy)
    {
        var rootArray = roots.ToArray();
        var lines = new List<string>
        {
            $"StS2 Launcher — Step {step}.0 {stepName} static map",
            "Read-only Cecil evidence from the exact selected compatibility image; never consumed as trusted runtime input.",
            $"Selected compatibility path: {baseline.SelectedPath}",
            $"Selected compatibility SHA-256: {baseline.SelectedSha256}",
            $"Root methods mapped: {rootArray.Length}",
            $"Transitive same-sts2 closure methods: {audit.ClosureMethods.Length}",
            $"Classified boundary references: {audit.Boundaries.Length}",
            $"Compiler state-machine MoveNext expansions: {audit.StateMachineExpansions.Length}",
            "Unapproved boundary references: 0",
            "Unresolved same-sts2 references: 0",
            "External Cecil resolution requests: 0",
            $"Policy: {policy}",
            string.Empty,
            "[COMPILER STATE-MACHINE EXPANSIONS]",
        };
        if (audit.StateMachineExpansions.Length == 0)
            lines.Add("  - none");
        foreach (var expansion in audit.StateMachineExpansions)
            lines.Add("  - " + expansion);
        lines.Add(string.Empty);
        lines.Add("[ROOT METHODS / IL]");
        foreach (var root in rootArray)
        {
            lines.Add($"  ROOT token=0x{root.MetadataToken.ToUInt32():X8}; {root.FullName}; IL={root.Body.Instructions.Count}");
            foreach (var instruction in root.Body.Instructions)
                lines.Add("    " + FormatStep41Instruction(instruction));
        }
        lines.Add(string.Empty);
        lines.Add("[TRANSITIVE SAME-STS2 CLOSURE METHODS]");
        foreach (var method in audit.ClosureMethods)
            lines.Add($"  - token=0x{method.Token:X8}; {method.FullName}");
        lines.Add(string.Empty);
        lines.Add("[CLASSIFIED BOUNDARY REFERENCES — ALL APPROVED FOR THIS RUNG]");
        if (audit.Boundaries.Length == 0)
            lines.Add("  - none");
        foreach (var boundary in audit.Boundaries)
        {
            lines.Add($"  - category={boundary.Category}");
            lines.Add($"      reference: {boundary.ReferenceFullName}");
            lines.Add($"      path: {boundary.Path}");
        }
        return string.Join("\n", lines) + "\n";
    }

    private static string BuildStartupLadderAsyncDirectMap(
        int step,
        string stepName,
        StartupLadderBaseline baseline,
        MethodDefinition method,
        TypeDefinition stateMachine,
        MethodDefinition moveNext)
    {
        var lines = new List<string>
        {
            $"StS2 Launcher — Step {step}.0 {stepName} static map",
            "Read-only Cecil evidence from the exact selected compatibility image; LaunchMainMenu is not invoked while this map is built.",
            $"Selected compatibility path: {baseline.SelectedPath}",
            $"Selected compatibility SHA-256: {baseline.SelectedSha256}",
            $"LaunchMainMenu full name: {method.FullName}",
            $"LaunchMainMenu token: 0x{method.MetadataToken.ToUInt32():X8}",
            $"Async state-machine type: {stateMachine.FullName}",
            $"MoveNext token: 0x{moveNext.MetadataToken.ToUInt32():X8}",
            $"MoveNext IL instructions: {moveNext.Body.Instructions.Count}",
            "LaunchMainMenu invoked: NO",
            "Rendering restarted: NO",
            string.Empty,
            "[LAUNCHMAINMENU METHOD]",
        };
        foreach (var instruction in method.Body.Instructions)
            lines.Add("  " + FormatStep41Instruction(instruction));
        lines.Add(string.Empty);
        lines.Add("[ASYNC STATE MACHINE FIELDS]");
        foreach (var field in stateMachine.Fields.OrderBy(field => field.MetadataToken.ToUInt32()))
            lines.Add($"  field token=0x{field.MetadataToken.ToUInt32():X8}; {field.FullName}");
        lines.Add(string.Empty);
        lines.Add("[MOVENEXT METHOD]");
        foreach (var instruction in moveNext.Body.Instructions)
            lines.Add("  " + FormatStep41Instruction(instruction));
        return string.Join("\n", lines) + "\n";
    }

    private static string BuildStartupLadderClosureAppendix(StartupLadderAudit audit, string heading)
    {
        var lines = new List<string>
        {
            string.Empty,
            $"[{heading}]",
            $"Transitive same-sts2 closure methods: {audit.ClosureMethods.Length}",
            $"Classified boundary references: {audit.Boundaries.Length}",
            $"Compiler state-machine MoveNext expansions: {audit.StateMachineExpansions.Length}",
            "Unapproved boundary references: 0",
            "Unresolved same-sts2 references: 0",
            "External Cecil resolution requests: 0",
            string.Empty,
            "[CLOSURE METHODS]",
        };
        foreach (var method in audit.ClosureMethods)
            lines.Add($"  - token=0x{method.Token:X8}; {method.FullName}");
        lines.Add(string.Empty);
        lines.Add("[CLASSIFIED BOUNDARIES — ALL APPROVED]");
        if (audit.Boundaries.Length == 0)
            lines.Add("  - none");
        foreach (var boundary in audit.Boundaries)
        {
            lines.Add($"  - category={boundary.Category}");
            lines.Add($"      reference: {boundary.ReferenceFullName}");
            lines.Add($"      path: {boundary.Path}");
        }
        return string.Join("\n", lines) + "\n";
    }

    private static object? InvokeStartupLadderZeroArg(Type interfaceType, object target, string methodName)
    {
        var method = interfaceType.GetMethods().SingleOrDefault(item => item.Name == methodName && item.GetParameters().Length == 0)
            ?? throw new MissingMethodException(interfaceType.FullName, methodName + "()");
        return InvokeStartupLadderMethod(method, target, null, "platform read " + methodName);
    }

    private static bool InvokeStartupLadderStaticBool(Type type, string methodName)
    {
        var method = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            .SingleOrDefault(item => item.Name == methodName && item.GetParameters().Length == 0 && item.ReturnType == typeof(bool))
            ?? throw new MissingMethodException(type.FullName, methodName + "()");
        var result = InvokeStartupLadderMethod(method, null, null, type.FullName + "." + methodName);
        return result is bool value ? value : throw new InvalidDataException(type.FullName + "." + methodName + " did not return bool.");
    }

    private static object? InvokeStartupLadderMethod(MethodInfo method, object? instance, object?[]? args, string boundary)
    {
        try
        {
            return method.Invoke(instance, args);
        }
        catch (TargetInvocationException tie) when (tie.InnerException is not null)
        {
            throw new InvalidOperationException($"{boundary} inner exception: {tie.InnerException.GetType().FullName}: {tie.InnerException.Message}", tie.InnerException);
        }
    }

    private static object RequireExistingStartupLadderSaveManagerInstance(Type saveType, int step)
    {
        var mockField = saveType.GetField(SaveManagerMockInstanceFieldName, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)
            ?? throw new MissingFieldException(SaveManagerTypeFullName, SaveManagerMockInstanceFieldName);
        var instanceField = saveType.GetField(SaveManagerInstanceFieldName, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)
            ?? throw new MissingFieldException(SaveManagerTypeFullName, SaveManagerInstanceFieldName);
        if (mockField.FieldType != saveType || instanceField.FieldType != saveType)
            throw new InvalidDataException($"Step {step}.0 SaveManager singleton field type drifted: mock={mockField.FieldType.FullName}; instance={instanceField.FieldType.FullName}.");
        if (mockField.GetValue(null) is not null)
            throw new InvalidDataException($"Step {step}.0 refuses SaveManager test/mock authority: {SaveManagerMockInstanceFieldName} is non-null.");
        return instanceField.GetValue(null)
            ?? throw new InvalidDataException($"Step {step}.0 requires the already-created production SaveManager singleton; field {SaveManagerInstanceFieldName} is null. get_Instance/ConstructDefault are intentionally not called by this rung.");
    }

    private static string DescribeStartupLadderReadSaveResult(object result)
    {
        var type = result.GetType();
        var success = type.GetProperty("Success", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(result);
        var status = type.GetProperty("Status", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(result);
        var error = type.GetProperty("ErrorMessage", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(result);
        return $"type={type.FullName}; success={success}; status={status}; error={error}";
    }

    private static object RequireStartupLadderPropertyValue(object instance, string propertyName, int step)
    {
        var property = instance.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?? throw new MissingMemberException(instance.GetType().FullName, propertyName);
        return property.GetValue(instance)
            ?? throw new InvalidDataException($"Step {step}.0 required property {instance.GetType().FullName}.{propertyName} to be non-null.");
    }

    private int GetStartupLadderRootSceneChildCount()
    {
        var instance = _step39NGameInstance ?? throw new InvalidOperationException("Startup ladder retained NGame instance is absent.");
        var rootSceneContainer = RequireStartupLadderPropertyValue(instance, "RootSceneContainer", 47);
        var candidates = rootSceneContainer.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(method => method.Name == "GetChildCount" && method.GetParameters().Length == 1 && method.GetParameters()[0].ParameterType == typeof(bool) && method.ReturnType == typeof(int))
            .ToArray();
        var method = candidates.SingleOrDefault()
            ?? throw new MissingMethodException(rootSceneContainer.GetType().FullName, $"GetChildCount(bool); candidates={string.Join(",", candidates.Select(item => item.ToString()))}");
        var value = InvokeStartupLadderMethod(method, rootSceneContainer, new object?[] { false }, "RootSceneContainer.GetChildCount(false)");
        return value is int count ? count : throw new InvalidDataException("RootSceneContainer.GetChildCount(false) did not return int.");
    }

    private static TransformedRealStS2StartupLadderGateResult StartupLadderPass(
        int step,
        string stepName,
        TransformedRealStS2StartupLadderGate gate,
        string detail)
        => new(step, stepName, gate, true, detail);

    private static TransformedRealStS2StartupLadderGateResult StartupLadderFail(
        int step,
        string stepName,
        TransformedRealStS2StartupLadderGate gate,
        string stage,
        Exception ex)
        => new(step, stepName, gate, false, $"Stage: {stage}\n{FormatExceptionDiagnostic(ex)}");

    private sealed record StartupLadderHostDelta(
        int ResolverDelta,
        int HostDelta,
        string? HostEntry);

    private sealed record StartupLadderBaseline(
        string SelectedPath,
        string SelectedSha256,
        int ResolverCount,
        int HostCount,
        int PrivateCount,
        int InitializerCount,
        int RejectedCount,
        int NativeCount);

    private sealed record StartupLadderAudit(
        Step41ClosureMethod[] ClosureMethods,
        Step41BoundaryObservation[] Boundaries,
        string[] UnresolvedSameAssemblyReferences,
        string[] StateMachineExpansions);
}
