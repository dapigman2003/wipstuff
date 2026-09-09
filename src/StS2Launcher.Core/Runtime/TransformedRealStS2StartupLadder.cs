using System.Reflection;
using Mono.Cecil;

namespace StS2Launcher.Core;

/// <summary>
/// Steps 43-47 are packaged together as an explicitly sequential startup ladder. Every rung keeps
/// its own four-gate authority and later rungs require the exact same-process closure of the prior
/// rung. Steps 43-46 keep rendering frozen. Step 47 is the first conditional live LaunchMainMenu
/// attempt and is exposed only after Step 46 has fully mapped its compiler-generated async MoveNext
/// closure and found no unapproved startup/native/platform boundary.
/// </summary>
public sealed partial class TransformedRealStS2VeryEarlyInitialization
{
    private const string Step43Name = "NULL PLATFORM AUTHORITY";
    private const string Step44Name = "LEGACY MIGRATION GUARD";
    private const string Step45Name = "LOCAL SAVE INITIALIZATION";
    private const string Step46Name = "LAUNCHMAINMENU ASYNC MAP";
    private const string Step47Name = "CONTROLLED LAUNCHMAINMENU";

    private const string PlatformUtilTypeFullName = "MegaCrit.Sts2.Core.Platform.PlatformUtil";
    private const string PlatformUtilInterfaceTypeFullName = "MegaCrit.Sts2.Core.Platform.IPlatformUtilStrategy";
    private const string AccountScopeMigratorTypeFullName = "MegaCrit.Sts2.Core.Saves.AccountScopeUserDataMigrator";
    private const string ProfileScopeMigratorTypeFullName = "MegaCrit.Sts2.Core.Saves.ProfileAccountScopeMigrator";
    private const string SaveManagerTypeFullName = "MegaCrit.Sts2.Core.Saves.SaveManager";
    private const string SaveManagerInstanceBackingFieldName = "<Instance>k__BackingField";
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
    private StartupLadderBaseline? _step46Baseline;
    private StartupLadderBaseline? _step47Baseline;

    private string _step43StaticMap = string.Empty;
    private string _step44StaticMap = string.Empty;
    private string _step45StaticMap = string.Empty;
    private string _step46StaticMap = string.Empty;
    private string _step43Observation = string.Empty;
    private string _step44Observation = string.Empty;
    private string _step45Observation = string.Empty;
    private string _step47Observation = string.Empty;

    private bool _step43ActionPassed;
    private bool _step44NoLegacyDataPassed;
    private bool _step45InvocationStarted;
    private bool _step45SaveInitPassed;
    private bool _step46StateMachineMapped;
    private bool _step46ClosureMapped;
    private bool _step46LaunchAdmissible;
    private bool _step46StaticMapDurablyWritten;
    private uint _step46LaunchMethodToken;
    private uint _step46MoveNextToken;
    private string _step46StateMachineTypeName = string.Empty;
    private MethodInfo? _step47LaunchMethod;
    private bool _step47InvocationStarted;
    private bool _step47LaunchPassed;
    private int _step47RootSceneChildrenBefore;

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
    public bool Step46LaunchMainMenuAdmissible => _step46LaunchAdmissible;
    public bool Step47InvocationStarted => _step47InvocationStarted;

    public string GetVerifiedStartupLadderStaticMap(int step)
        => step switch
        {
            43 when !string.IsNullOrWhiteSpace(_step43StaticMap) => _step43StaticMap,
            44 when !string.IsNullOrWhiteSpace(_step44StaticMap) => _step44StaticMap,
            45 when !string.IsNullOrWhiteSpace(_step45StaticMap) => _step45StaticMap,
            46 when !string.IsNullOrWhiteSpace(_step46StaticMap) => _step46StaticMap,
            _ => throw new InvalidOperationException($"Step {step}.0 has not produced a verified startup-ladder static map."),
        };

    public void MarkStep46StaticMapDurablyWritten()
    {
        if (!_step46ClosureMapped || !_step46LaunchAdmissible || string.IsNullOrWhiteSpace(_step46StaticMap))
            throw new InvalidOperationException("Step 46.0 static map cannot be marked durable before the exact admissible LaunchMainMenu closure is complete.");
        _step46StaticMapDurablyWritten = true;
    }

    private void ResetStartupLadderState()
    {
        _step43Baseline = null;
        _step44Baseline = null;
        _step45Baseline = null;
        _step46Baseline = null;
        _step47Baseline = null;
        _step43StaticMap = string.Empty;
        _step44StaticMap = string.Empty;
        _step45StaticMap = string.Empty;
        _step46StaticMap = string.Empty;
        _step43Observation = string.Empty;
        _step44Observation = string.Empty;
        _step45Observation = string.Empty;
        _step47Observation = string.Empty;
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
        _step47LaunchMethod = null;
        _step47InvocationStarted = false;
        _step47LaunchPassed = false;
        _step47RootSceneChildrenBefore = 0;
        _exactStep43ClosurePassed = false;
        _exactStep44ClosurePassed = false;
        _exactStep45ClosurePassed = false;
        _exactStep46ClosurePassed = false;
        _exactStep47ClosurePassed = false;
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
            var instanceField = saveManager.Fields.SingleOrDefault(field => field.Name == SaveManagerInstanceBackingFieldName && field.IsStatic && field.FieldType.FullName == SaveManagerTypeFullName)
                ?? throw new MissingFieldException(SaveManagerTypeFullName, SaveManagerInstanceBackingFieldName);
            var initProfile = RequireStartupLadderMethod(saveManager, "InitProfileId", 1, false, step);
            var initProgress = RequireStartupLadderMethod(saveManager, "InitProgressData", 0, false, step);
            var initPrefs = RequireStartupLadderMethod(saveManager, "InitPrefsData", 0, false, step);
            if (initProfile.ReturnType.FullName != "System.Void" || initProfile.Parameters[0].ParameterType.FullName != "System.Nullable`1<System.Int32>")
                throw new InvalidDataException("Step 45.0 InitProfileId signature drifted from void InitProfileId(Nullable<int>).");
            _ = instanceField; // Serialized authority: Step 45 will access the existing singleton backing field directly, never SaveManager.get_Instance/ConstructDefault.
            var roots = new[] { initProfile, initProgress, initPrefs };
            var audit = AuditStartupLadderRoots(roots, allTypes, allMethods);
            RequireStartupLadderAuditAdmissible(audit, "Step 45.0 local save initialization");
            if (resolver.Requests.Count != 0)
                throw new InvalidDataException("Step 45.0 static audit unexpectedly attempted external Cecil resolution: " + string.Join(" | ", resolver.Requests));
            _step45StaticMap = BuildStartupLadderStaticMap(step, Step45Name, baseline, roots, audit,
                "Exact local SaveManager profile/progress/prefs initialization; cloud sync and migration methods remain uninvoked.");
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
            RequireStartupLadderBaselineUnchanged(context, baseline, "Step 45 Gate C");
            if (stateAfter != stateBefore)
                throw new InvalidDataException($"Step 45.0 local save initialization changed OneTimeInitialization state: {stateBefore} -> {stateAfter}.");
            _step45Observation = "progress={" + DescribeStartupLadderReadSaveResult(progressResult) + "}; prefs={" + DescribeStartupLadderReadSaveResult(prefsResult) + "}";
            _step45SaveInitPassed = true;
            Checkpoint(checkpoint, "M45_C_PASS — local SaveManager sequence returned; " + _step45Observation + "; state=2; resolver/host/private/initializer/rejected/native deltas=0; rendering remains stopped.");
            return StartupLadderPass(step, Step45Name, gate,
                "Exact local SaveManager initialization returned once in original profile/progress/prefs order. " + _step45Observation + ". Context/native deltas remain zero.");
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
            RequireStartupLadderBaselineUnchanged(context, baseline, "Step 45 Gate D");
            _exactStep45ClosurePassed = true;
            Checkpoint(checkpoint, $"M45_D_PASS — renderingStopped=True; state={state}; SettingsSave={settings.GetType().FullName}; PrefsSave={prefs.GetType().FullName}; Progress={progress.GetType().FullName}; resolver/host/private/initializer/rejected/native deltas=0.");
            return StartupLadderPass(step, Step45Name, gate,
                $"Frozen save confinement passed. SettingsSave={settings.GetType().Name}; PrefsSave={prefs.GetType().Name}; Progress={progress.GetType().Name}; NGame/state/context remain authoritative.");
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
            Checkpoint(checkpoint, "M46_C_ENTRY — traversing exact LaunchMainMenu.MoveNext same-sts2 closure. Proven Null-platform read helpers and inert SentryService wrappers are allowed; OneTimeInitialization/InitializePlatform/deferred-startup/external Steamworks/FMOD/Spine/external Sentry/native-extension boundaries fail before any launch invocation.");
            stage = "LaunchMainMenu transitive admissibility audit";
            using var resolver = new RejectingAssemblyResolver();
            using var module = OpenStartupLadderModule(baseline.SelectedPath, resolver);
            var allTypes = EnumerateTypes(module.Types).ToDictionary(type => type.FullName, StringComparer.Ordinal);
            var allMethods = BuildStartupLadderMethodMap(allTypes);
            var moveNext = allTypes.Values.SelectMany(type => type.Methods)
                .SingleOrDefault(method => method.MetadataToken.ToUInt32() == _step46MoveNextToken)
                ?? throw new InvalidDataException($"Step 46.0 could not relocate MoveNext token 0x{_step46MoveNextToken:X8}.");
            var audit = AuditStartupLadderRoots(new[] { moveNext }, allTypes, allMethods);
            RequireStartupLadderAuditAdmissible(audit, "Step 46.0 LaunchMainMenu closure with recursive compiler state-machine expansion");
            if (resolver.Requests.Count != 0)
                throw new InvalidDataException("Step 46.0 closure audit unexpectedly attempted external Cecil resolution: " + string.Join(" | ", resolver.Requests));

            _step46StaticMap += BuildStartupLadderClosureAppendix(audit, "LAUNCHMAINMENU TRANSITIVE + COMPILER STATE-MACHINE CLOSURE");
            _step46ClosureMapped = true;
            _step46LaunchAdmissible = true;
            RequireStartupLadderBaselineUnchanged(context, baseline, "Step 46 Gate C");
            Checkpoint(checkpoint, $"M46_C_PASS — transitiveSameSts2LaunchClosureMethods={audit.ClosureMethods.Length}; compilerStateMachineExpansions={audit.StateMachineExpansions.Length}; classifiedBoundaryRefs={audit.Boundaries.Length}; forbiddenBoundaryRefs=0; unresolvedSameSts2Refs=0; externalResolutionRequests=0; LaunchMainMenu invoked=NO; rendering remains stopped.");
            return StartupLadderPass(step, Step46Name, gate,
                $"LaunchMainMenu closure including nested compiler state machines is admissible for a controlled attempt: methods={audit.ClosureMethods.Length}; state-machine expansions={audit.StateMachineExpansions.Length}; classified refs={audit.Boundaries.Length}; forbidden/unresolved/external resolution=0. LaunchMainMenu remains uninvoked.");
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
            if (!_step46StateMachineMapped || !_step46ClosureMapped || !_step46LaunchAdmissible)
                throw new InvalidOperationException("Step 46.0 Gate D requires a complete admissible LaunchMainMenu map.");
            if (!_step46StaticMapDurablyWritten)
                throw new InvalidOperationException("Step 46.0 Gate D requires the complete LaunchMainMenu static map to be durably written before closure.");
            if (!renderingStopped)
                throw new InvalidOperationException("Step 46.0 never restarts rendering; Gate D requires it to remain frozen.");
            Checkpoint(checkpoint, "M46_D_ENTRY — proving the complete LaunchMainMenu map caused no runtime drift or invocation; renderer remains frozen and NGame/state/context remain authoritative.");
            stage = "frozen LaunchMainMenu no-invocation confinement";
            var state = RequireStep40InsertedAuthority();
            RequireStartupLadderBaselineUnchanged(context, baseline, "Step 46 Gate D");
            _exactStep46ClosurePassed = true;
            Checkpoint(checkpoint, $"M46_D_PASS — renderingStopped=True; state={state}; LaunchMainMenu invoked=NO; launchAdmissible=True; staticMapDurable=True; resolver/host/private/initializer/rejected/native deltas=0.");
            return StartupLadderPass(step, Step46Name, gate,
                "Complete LaunchMainMenu async map is durable and admissible. Rendering stayed frozen, LaunchMainMenu was not invoked, and context/native authority is unchanged.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"M46_D_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return StartupLadderFail(step, Step46Name, gate, stage, ex);
        }
    }

    // STEP 47 — CONTROLLED LAUNCHMAINMENU

    public TransformedRealStS2StartupLadderGateResult RunStep47ClosedStep46Authority(bool renderingStopped, Action<string>? checkpoint = null)
    {
        const int step = 47;
        const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.PrerequisiteAuthority;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep47Prerequisite("Step 47 Gate A entry");
            if (!renderingStopped)
                throw new InvalidOperationException("Step 47.0 requires rendering to be frozen before the controlled launch is armed.");
            if (!_step46LaunchAdmissible || !_step46StaticMapDurablyWritten)
                throw new InvalidOperationException("Step 47.0 requires Step 46 admissible + durable LaunchMainMenu authority.");
            Checkpoint(checkpoint, "M47_A_ENTRY — requiring Step-46 4/4 admissible LaunchMainMenu async map, frozen rendering, local-save authority, Null platform authority, and retained real NGame before the first live menu attempt.");
            stage = "closed Step-46 launch-map authority";
            var state = RequireStep40InsertedAuthority();
            var selected = RequireStartupLadderSelectedAuthority(step);
            _step47Baseline = CaptureStartupLadderBaseline(selected.Path, selected.Sha256, context);
            _step47RootSceneChildrenBefore = GetStartupLadderRootSceneChildCount();
            Checkpoint(checkpoint, $"M47_A_PASS — Step-46 4/4 retained; renderingStopped=True; state={state}; rootSceneChildrenBefore={_step47RootSceneChildrenBefore}; selectedSha256={selected.Sha256}; launchAdmissible=True.");
            return StartupLadderPass(step, Step47Name, gate,
                $"Controlled launch prerequisite passed. RootSceneContainer children before launch={_step47RootSceneChildrenBefore}; rendering frozen; exact Step-46 admissible map retained.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"M47_A_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return StartupLadderFail(step, Step47Name, gate, stage, ex);
        }
    }

    public TransformedRealStS2StartupLadderGateResult RunStep47ExactRuntimeBinding(Action<string>? checkpoint = null)
    {
        const int step = 47;
        const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.StaticAuditOrBinding;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep47Prerequisite("Step 47 Gate B entry");
            var baseline = _step47Baseline ?? throw new InvalidOperationException("Step 47.0 Gate A must pass before Gate B.");
            Checkpoint(checkpoint, "M47_B_ENTRY — binding exact runtime NGame.LaunchMainMenu(bool) from the already-admitted selected assembly; token must equal Step-46 Cecil authority and return Task. No invocation yet.");
            stage = "exact LaunchMainMenu runtime binding";
            var instance = _step39NGameInstance ?? throw new InvalidOperationException("Step 47.0 retained real NGame instance is absent.");
            var candidates = instance.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
                .Where(method => method.Name == NGameLaunchMainMenuMethodName && method.GetParameters().Length == 1 && method.GetParameters()[0].ParameterType == typeof(bool) && typeof(Task).IsAssignableFrom(method.ReturnType) && !method.IsGenericMethod)
                .ToArray();
            var method = candidates.SingleOrDefault()
                ?? throw new MissingMethodException(instance.GetType().FullName, $"LaunchMainMenu(bool) exact declared Task-returning overload; candidates={string.Join(",", candidates.Select(item => item.ToString()))}");
            if ((uint)method.MetadataToken != _step46LaunchMethodToken)
                throw new InvalidDataException($"Step 47.0 LaunchMainMenu runtime token drifted. expected=0x{_step46LaunchMethodToken:X8}; actual=0x{method.MetadataToken:X8}.");
            _step47LaunchMethod = method;
            RequireStartupLadderBaselineUnchanged(context, baseline, "Step 47 Gate B");
            Checkpoint(checkpoint, $"M47_B_PASS — exact runtime LaunchMainMenu(bool) bound; token=0x{method.MetadataToken:X8}; returnType={method.ReturnType.FullName}; invocation=NO; rendering remains stopped.");
            return StartupLadderPass(step, Step47Name, gate,
                $"Exact LaunchMainMenu(bool) runtime binding passed. Token=0x{method.MetadataToken:X8}; return type={method.ReturnType.FullName}; no invocation yet.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"M47_B_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return StartupLadderFail(step, Step47Name, gate, stage, ex);
        }
    }

    public async Task<TransformedRealStS2StartupLadderGateResult> RunStep47ControlledLaunchMainMenuInvocationAsync(
        bool renderingActive,
        Action<string>? checkpoint = null)
    {
        const int step = 47;
        const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.ControlledAction;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep47Prerequisite("Step 47 Gate C entry");
            var baseline = _step47Baseline ?? throw new InvalidOperationException("Step 47.0 baseline is absent.");
            var method = _step47LaunchMethod ?? throw new InvalidOperationException("Step 47.0 Gate B exact runtime binding is absent.");
            if (!renderingActive)
                throw new InvalidOperationException("Step 47.0 Gate C requires the caller to start Godot rendering immediately before invocation.");
            if (_step47InvocationStarted)
                throw new InvalidOperationException("Step 47.0 LaunchMainMenu invocation is one-shot and cannot be retried in-process.");
            _step47InvocationStarted = true;
            var stateBefore = RequireStep40InsertedAuthority();
            Checkpoint(checkpoint, $"M47_C_INVOKE_START — rendering is active; invoking exact audited NGame.LaunchMainMenu(skipIntro=True) once; token=0x{method.MetadataToken:X8}; stateBefore={stateBefore}. No GameStartup/Steam/native-extension call is authorized. The launcher will not abandon an un-cancelable LaunchMainMenu Task; a hard hang must be diagnosed from this durable checkpoint and terminated by relaunch.");
            stage = "one-shot live LaunchMainMenu invocation";
            var instance = _step39NGameInstance ?? throw new InvalidOperationException("Step 47.0 retained NGame instance is absent.");
            var taskObject = InvokeStartupLadderMethod(method, instance, new object?[] { true }, "Step 47 LaunchMainMenu");
            if (taskObject is not Task launchTask)
                throw new InvalidDataException("Step 47.0 LaunchMainMenu reflection did not return a Task.");
            await launchTask;
            Checkpoint(checkpoint, "M47_C_INVOKE_RETURNED — LaunchMainMenu Task completed successfully while rendering remained active.");
            var stateAfter = RequireStep40InsertedAuthority();
            RequireStartupLadderBaselineUnchanged(context, baseline, "Step 47 Gate C");
            if (stateAfter != stateBefore)
                throw new InvalidDataException($"Step 47.0 LaunchMainMenu changed OneTimeInitialization state: {stateBefore} -> {stateAfter}.");
            _step47LaunchPassed = true;
            Checkpoint(checkpoint, "M47_C_PASS — exact LaunchMainMenu(skipIntro=True) completed once; state=2; resolver/host/private/initializer/rejected/native deltas=0; rendering remains active for Gate D observation.");
            return StartupLadderPass(step, Step47Name, gate,
                "Exact audited LaunchMainMenu(skipIntro=true) completed once under live rendering. OneTimeInitialization state and resolver/native context remain unchanged.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"M47_C_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return StartupLadderFail(step, Step47Name, gate, stage, ex);
        }
    }

    public TransformedRealStS2StartupLadderGateResult RunStep47LivePostLaunchConfinement(bool renderingActive, Action<string>? checkpoint = null)
    {
        const int step = 47;
        const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.PostActionConfinement;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep47Prerequisite("Step 47 Gate D entry");
            var baseline = _step47Baseline ?? throw new InvalidOperationException("Step 47.0 baseline is absent.");
            if (!_step47InvocationStarted || !_step47LaunchPassed)
                throw new InvalidOperationException("Step 47.0 Gate D requires the one-shot LaunchMainMenu Task to have completed successfully.");
            if (!renderingActive)
                throw new InvalidOperationException("Step 47.0 Gate D expects rendering to remain active after a successful main-menu launch.");
            Checkpoint(checkpoint, "M47_D_ENTRY — proving live post-launch NGame/state/context confinement and requiring RootSceneContainer to contain a real launched child scene. Rendering stays active on success so the menu can be observed.");
            stage = "live post-LaunchMainMenu confinement";
            var state = RequireStep40InsertedAuthority();
            var childrenAfter = GetStartupLadderRootSceneChildCount();
            if (childrenAfter <= 0 || childrenAfter <= _step47RootSceneChildrenBefore)
                throw new InvalidDataException($"Step 47.0 LaunchMainMenu completed but RootSceneContainer did not gain a child scene. before={_step47RootSceneChildrenBefore}; after={childrenAfter}.");
            RequireStartupLadderBaselineUnchanged(context, baseline, "Step 47 Gate D");
            _step47Observation = $"rootSceneChildren={_step47RootSceneChildrenBefore}->{childrenAfter}; renderingActive=True; state={state}";
            _exactStep47ClosurePassed = true;
            Checkpoint(checkpoint, "M47_D_PASS — " + _step47Observation + "; exact NGame singleton/parent/_window authority preserved; resolver/host/private/initializer/rejected/native deltas=0. Rendering intentionally remains active.");
            return StartupLadderPass(step, Step47Name, gate,
                "Live post-launch confinement passed. " + _step47Observation + ". A real child scene is present and rendering intentionally remains active for observation.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"M47_D_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return StartupLadderFail(step, Step47Name, gate, stage, ex);
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
        if (!_exactStep46ClosurePassed || !_step46ClosureMapped || !_step46LaunchAdmissible || !_step46StaticMapDurablyWritten)
            throw new InvalidOperationException($"{boundary} requires same-process Step-46 4/4 admissible/durable LaunchMainMenu async-map authority.");
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
        var field = saveType.GetField(SaveManagerInstanceBackingFieldName, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)
            ?? throw new MissingFieldException(SaveManagerTypeFullName, SaveManagerInstanceBackingFieldName);
        if (field.FieldType != saveType)
            throw new InvalidDataException($"Step {step}.0 SaveManager singleton backing-field type drifted: {field.FieldType.FullName}.");
        return field.GetValue(null)
            ?? throw new InvalidDataException($"Step {step}.0 requires the already-created SaveManager singleton; backing field {SaveManagerInstanceBackingFieldName} is null. ConstructDefault is intentionally not called by this rung.");
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
