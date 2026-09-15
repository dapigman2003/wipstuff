using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Runtime.Loader;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace StS2Launcher.Core;

/// <summary>
/// Steps 58-62 pivot from decomposing character-select internals to adopting the state the real Godot game
/// already owns. Physical 0.0.191 proved that _characterSelectSubmenu may already be in the SceneTree before
/// Step 58. Physical 0.0.192 proved that an in-tree cached screen may still have an unbound/null
/// NSubmenu._stack before the actual character-select push transition. Physical 0.0.193 then reached the real
/// OpenCharacterSelect handler. Physical 0.0.194 then proved the retained NSingleplayerSubmenu was already logically
/// bound and the real _standardButton was supplied, yet the original handler still raised NullReferenceException.
/// Static analysis additionally proved that the cached hidden/in-tree/unbound NCharacterSelectScreen is the normal
/// NMainMenuSubmenuStack._Ready preload state. Physical 0.0.195 then localized the first concrete broken prerequisite:
/// NCharacterSelectScreen.IsNodeReady() was true, _charButtonContainer was already populated, but _ascensionPanel was
/// null. Trusted sts2.dll IL proves _Ready assigns _charButtonContainer immediately before resolving
/// GetNode<NAscensionPanel>("%AscensionPanel") into _ascensionPanel. Physical 0.0.197 then proved the exact
/// AscensionPanel node exists with the selected NAscensionPanel runtime type, correct character-select owner, and
/// IsNodeReady()==true, while its live UniqueNameInOwner flag is false even though the exact parent TSCN override says
/// unique_name_in_owner=true. Physical 0.0.200 then proves the global PackedScene compatibility layer restores those
/// instanced-root unique-name values and the previously missing Ready-bound fields. The original transition advances
/// into NCharacterSelectScreen.OnSubmenuOpened() and fails in NConfirmButton.OnEnable() while enabling the retained
/// embark button. Step 59 therefore adds a fail-closed pre-handler NConfirmButton Ready/OnEnable field/IL/scene/TSCN
/// preflight without repairing game state. Steps 60-62 remain available only after a
/// clean original-handler transition and then audit/render the actual active screen
/// and run bounded visible render residencies. Character choice, confirm/embark, run start and Step 63 remain unopened.
/// </summary>
public sealed partial class TransformedRealStS2VeryEarlyInitialization
{
    public const int Step61CharacterSelectRenderTargetMilliseconds = 2_000;
    public const int Step61CharacterSelectRenderEvidenceCeilingMilliseconds = 10_000;
    public const int Step62CharacterSelectSustainedRenderTargetMilliseconds = 10_000;
    public const int Step62CharacterSelectSustainedRenderEvidenceCeilingMilliseconds = 30_000;

    private const string Step58Name = "CHARACTER SELECT RUNTIME OWNERSHIP AUDIT";
    public const string Step59GateName = "GLOBAL PACKEDSCENE UNIQUE-NAME COMPAT + REAL TRANSITION";
    private const string Step59Name = Step59GateName;
    private const string Step60Name = "ACTIVE CHARACTER SELECT SURFACE AUDIT";
    private const string Step61Name = "CHARACTER SELECT SHORT RENDER RESIDENCY";
    private const string Step62Name = "CHARACTER SELECT SUSTAINED RENDER RESIDENCY";

    private const string CharacterSelectScreenManagedTypeFullName = "MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect.NCharacterSelectScreen";
    private const string SubmenuStackManagedTypeFullName = "MegaCrit.Sts2.Core.Nodes.Screens.MainMenu.NSubmenuStack";
    private const string MainMenuCharacterSelectSubmenuFieldName = "_characterSelectSubmenu";
    private const string SingleplayerStandardButtonFieldNameForOwnership = "_standardButton";
    private const string SubmenuManagedTypeFullName = "MegaCrit.Sts2.Core.Nodes.Screens.MainMenu.NSubmenu";
    private const string SubmenuStackPushMethodNameForOwnership = "Push";
    private const string SubmenuStackFieldName = "_stack";
    private const string AscensionPanelManagedTypeFullNameForForensics = "MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect.NAscensionPanel";
    private const string AscensionPanelResourcePathForForensics = "res://scenes/screens/ascension_panel.tscn";
    private const string ConfirmButtonManagedTypeFullNameForForensics = "MegaCrit.Sts2.Core.Nodes.CommonUi.NConfirmButton";
    private const string NButtonManagedTypeFullNameForForensics = "MegaCrit.Sts2.Core.Nodes.GodotExtensions.NButton";
    private const string ClickableControlManagedTypeFullNameForForensics = "MegaCrit.Sts2.Core.Nodes.GodotExtensions.NClickableControl";
    private const string ConfirmButtonResourcePathForForensics = "res://scenes/ui/confirm_button.tscn";

    private StartupLadderBaseline? _step58Baseline;
    private StartupLadderBaseline? _step59Baseline;
    private StartupLadderBaseline? _step59PostTransitionBaseline;
    private StartupLadderBaseline? _step60Baseline;
    private StartupLadderBaseline? _step61Baseline;
    private StartupLadderBaseline? _step61PostPulseBaseline;
    private StartupLadderBaseline? _step62Baseline;
    private StartupLadderBaseline? _step62PostPulseBaseline;

    private string _step58StaticMap = string.Empty;
    private string _step59StaticMap = string.Empty;
    private string _step60StaticMap = string.Empty;
    private string _step61StaticMap = string.Empty;
    private string _step62StaticMap = string.Empty;

    private bool _step58CurrentOwnershipAcquisitionPassed;
    private bool _step58CurrentOwnershipAcquisitionInvokedOpen;
    private bool _step58OwnershipMapped;
    private bool _step58StaticMapDurablyWritten;
    private bool _step59TransitionBound;
    private bool _step59TransitionStarted;
    private bool _step59TransitionPassed;
    private bool _step59StaticMapDurablyWritten;
    private bool _step60SurfaceMapped;
    private bool _step60StaticMapDurablyWritten;
    private bool _step61StaticMapDurablyWritten;
    private bool _step61PulseStarted;
    private bool _step61PulsePassed;
    private bool _step62StaticMapDurablyWritten;
    private bool _step62PulseStarted;
    private bool _step62PulsePassed;

    private bool _exactStep58ClosurePassed;
    private bool _exactStep59ClosurePassed;
    private bool _exactStep60ClosurePassed;
    private bool _exactStep61ClosurePassed;
    private bool _exactStep62ClosurePassed;

    private uint _step58OpenCharacterSelectToken;
    private uint _step59SubmenuPushToken;
    private MethodInfo? _step58RuntimeOpenCharacterSelect;
    private MethodInfo? _step59RuntimeSubmenuPush;
    private object? _step59StandardButton;
    private bool _step59NavigationRepairRequired;
    private bool _step59NavigationRepairInvoked;
    private string _step59PreflightSnapshot = string.Empty;
    private string _step59FailureSnapshot = string.Empty;
    private string _step59ReadyBindingDiagnostics = string.Empty;
    private string _step59ReadyBindingBlocker = string.Empty;
    private string _step59UniqueNameProvenanceDiagnostics = string.Empty;
    private string _step59ConfirmButtonDiagnostics = string.Empty;
    private string _step59ConfirmButtonPostFailureDiagnostics = string.Empty;
    private string _step59ControllerSingletonRehearsalDiagnostics = string.Empty;
    private string _step59RegisterHotkeysProbeDiagnostics = string.Empty;
    private string _step59UpdateControllerProbeDiagnostics = string.Empty;
    private string _step59ConfirmButtonTailProbeDiagnostics = string.Empty;
    private string _step59NativeTweenMatrixDiagnostics = string.Empty;
    private string _step59AlternativeTweenTailDiagnostics = string.Empty;
    private string _step59IsolatedEnableProbeDiagnostics = string.Empty;
    private bool _step59ReadyBindingPreflightPassed;
    private bool _step59DiagnosticMutationProbeStarted;
    private object? _step58ObservedCharacterSelect;
    private CharacterSelectRuntimeState? _step58ObservedState;
    private bool _step58TransitionAlreadyComplete;
    private object? _step59CharacterSelectScreen;

    public bool ExactStep58ClosurePassed => _exactStep58ClosurePassed;
    public bool Step58CurrentOwnershipAcquisitionPassed => _step58CurrentOwnershipAcquisitionPassed;
    public bool Step58CurrentOwnershipAcquisitionInvokedOpen => _step58CurrentOwnershipAcquisitionInvokedOpen;
    public bool ExactStep59ClosurePassed => _exactStep59ClosurePassed;
    public bool ExactStep60ClosurePassed => _exactStep60ClosurePassed;
    public bool ExactStep61ClosurePassed => _exactStep61ClosurePassed;
    public bool ExactStep62ClosurePassed => _exactStep62ClosurePassed;
    public bool Step59TransitionStarted => _step59TransitionStarted;
    public bool Step59HandlerInvocationRequired => !_step58TransitionAlreadyComplete;
    public bool Step59ReadyBindingPreflightPassed => _step59ReadyBindingPreflightPassed;
    public bool Step59DiagnosticMutationProbeStarted => _step59DiagnosticMutationProbeStarted;
    public bool Step61PulseStarted => _step61PulseStarted;
    public bool Step62PulseStarted => _step62PulseStarted;

    private void ResetCharacterSelectContinuationState()
    {
        _step58Baseline = null;
        _step59Baseline = null;
        _step59PostTransitionBaseline = null;
        _step60Baseline = null;
        _step61Baseline = null;
        _step61PostPulseBaseline = null;
        _step62Baseline = null;
        _step62PostPulseBaseline = null;
        _step58StaticMap = string.Empty;
        _step59StaticMap = string.Empty;
        _step60StaticMap = string.Empty;
        _step61StaticMap = string.Empty;
        _step62StaticMap = string.Empty;
        _step58CurrentOwnershipAcquisitionPassed = false;
        _step58CurrentOwnershipAcquisitionInvokedOpen = false;
        _step58OwnershipMapped = false;
        _step58StaticMapDurablyWritten = false;
        _step59TransitionBound = false;
        _step59TransitionStarted = false;
        _step59TransitionPassed = false;
        _step59StaticMapDurablyWritten = false;
        _step60SurfaceMapped = false;
        _step60StaticMapDurablyWritten = false;
        _step61StaticMapDurablyWritten = false;
        _step61PulseStarted = false;
        _step61PulsePassed = false;
        _step62StaticMapDurablyWritten = false;
        _step62PulseStarted = false;
        _step62PulsePassed = false;
        _exactStep58ClosurePassed = false;
        _exactStep59ClosurePassed = false;
        _exactStep60ClosurePassed = false;
        _exactStep61ClosurePassed = false;
        _exactStep62ClosurePassed = false;
        _step58OpenCharacterSelectToken = 0;
        _step59SubmenuPushToken = 0;
        _step58RuntimeOpenCharacterSelect = null;
        _step59RuntimeSubmenuPush = null;
        _step59StandardButton = null;
        _step59NavigationRepairRequired = false;
        _step59NavigationRepairInvoked = false;
        _step59PreflightSnapshot = string.Empty;
        _step59FailureSnapshot = string.Empty;
        _step59ReadyBindingDiagnostics = string.Empty;
        _step59ReadyBindingBlocker = string.Empty;
        _step59UniqueNameProvenanceDiagnostics = string.Empty;
        _step59ConfirmButtonDiagnostics = string.Empty;
        _step59ConfirmButtonPostFailureDiagnostics = string.Empty;
        _step59ControllerSingletonRehearsalDiagnostics = string.Empty;
        _step59RegisterHotkeysProbeDiagnostics = string.Empty;
        _step59UpdateControllerProbeDiagnostics = string.Empty;
        _step59ConfirmButtonTailProbeDiagnostics = string.Empty;
        _step59NativeTweenMatrixDiagnostics = string.Empty;
        _step59AlternativeTweenTailDiagnostics = string.Empty;
        _step59IsolatedEnableProbeDiagnostics = string.Empty;
        _step59ReadyBindingPreflightPassed = false;
        _step59DiagnosticMutationProbeStarted = false;
        _step58ObservedCharacterSelect = null;
        _step58ObservedState = null;
        _step58TransitionAlreadyComplete = false;
        _step59CharacterSelectScreen = null;
    }

    public string GetVerifiedCharacterSelectContinuationStaticMap(int step)
        => step switch
        {
            58 when !string.IsNullOrWhiteSpace(_step58StaticMap) => _step58StaticMap,
            59 when !string.IsNullOrWhiteSpace(_step59StaticMap) => _step59StaticMap,
            60 when !string.IsNullOrWhiteSpace(_step60StaticMap) => _step60StaticMap,
            61 when !string.IsNullOrWhiteSpace(_step61StaticMap) => _step61StaticMap,
            62 when !string.IsNullOrWhiteSpace(_step62StaticMap) => _step62StaticMap,
            _ => throw new InvalidOperationException($"Step {step}.0 has not produced a verified character-select ownership static map."),
        };

    public void MarkStep58StaticMapDurablyWritten()
    {
        if (!_step58OwnershipMapped || string.IsNullOrWhiteSpace(_step58StaticMap)) throw new InvalidOperationException("Step 58.0 ownership map is incomplete.");
        _step58StaticMapDurablyWritten = true;
    }
    public void MarkStep59StaticMapDurablyWritten()
    {
        if (!_step59TransitionBound || string.IsNullOrWhiteSpace(_step59StaticMap)) throw new InvalidOperationException("Step 59.0 forensic/binding map is incomplete.");
        _step59StaticMapDurablyWritten = true;
    }
    public void MarkStep60StaticMapDurablyWritten()
    {
        if (!_step60SurfaceMapped || string.IsNullOrWhiteSpace(_step60StaticMap)) throw new InvalidOperationException("Step 60.0 active-surface map is incomplete.");
        _step60StaticMapDurablyWritten = true;
    }
    public void MarkStep61StaticMapDurablyWritten()
    {
        if (string.IsNullOrWhiteSpace(_step61StaticMap)) throw new InvalidOperationException("Step 61.0 render preflight map is absent.");
        _step61StaticMapDurablyWritten = true;
    }
    public void MarkStep62StaticMapDurablyWritten()
    {
        if (string.IsNullOrWhiteSpace(_step62StaticMap)) throw new InvalidOperationException("Step 62.0 sustained-render preflight map is absent.");
        _step62StaticMapDurablyWritten = true;
    }

    // STEP 58 — adopt the runtime state the game actually owns; do not require null/off-tree cache state.
    public TransformedRealStS2StartupLadderGateResult RunStep58CurrentOwnershipAcquisition(bool renderingStopped, Action<string>? checkpoint = null)
    {
        const int step = 58;
        const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.PrerequisiteAuthority;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep52Prerequisite("Step 58 current-ownership acquisition entry");
            if (!_exactStep52ClosurePassed || !_step52PulsePassed || !_step52StaticMapDurablyWritten)
                throw new InvalidOperationException("Step 58 current-ownership acquisition requires same-process Step-52 4/4 sustained main-menu render/refreeze authority.");
            if (!renderingStopped)
                throw new InvalidOperationException("Step 58 current-ownership acquisition requires rendering frozen.");

            var selected = RequireStartupLadderSelectedAuthority(step);
            _step58Baseline = CaptureStartupLadderBaseline(selected.Path, selected.Sha256, context);
            RequireStartupLadderBaselineUnchanged(context, _step58Baseline, "Step 58 acquisition preflight");
            RequireStep48LifecycleRuntimeGuardsCurrent(context, "Step 58 acquisition guard recheck", requirePreAdmissionCounts: false);
            var menu = RequireRetainedInTreeMainMenu(step);

            using var resolver = new RejectingAssemblyResolver();
            using var module = OpenStartupLadderModule(_step58Baseline.SelectedPath, resolver);
            var allTypes = EnumerateTypes(module.Types).ToDictionary(type => type.FullName, StringComparer.Ordinal);
            var allMethods = BuildStartupLadderMethodMap(allTypes);
            var mainMenuType = RequireStartupLadderType(allTypes, MainMenuManagedRootTypeFullName, step);
            var openMetadata = RequireUniqueStartupLadderNamedMethod(mainMenuType, MainMenuOpenSingleplayerSubmenuMethodName, step);
            if (openMetadata.Parameters.Count != 0 || openMetadata.ReturnType.FullName != SingleplayerSubmenuManagedTypeFullName)
                throw new InvalidDataException($"Step 58 acquisition requires zero-arg OpenSingleplayerSubmenu returning exact {SingleplayerSubmenuManagedTypeFullName}; observed params={openMetadata.Parameters.Count}; return={openMetadata.ReturnType.FullName}.");
            var openAudit = AuditStartupLadderInvocationFrontier([openMetadata], allTypes, allMethods,
                _step48LifecycleGuardAuthority ?? throw new InvalidOperationException("Step 58 acquisition requires retained Step-48 runtime guards."));
            RequireImmediateFrontierAdmissible(openAudit, "Step 58 current-ownership OpenSingleplayerSubmenu acquisition");
            if (resolver.Requests.Count != 0)
                throw new InvalidDataException("Step 58 acquisition attempted external Cecil resolution: " + string.Join(" | ", resolver.Requests));

            _step53OpenMethodToken = openMetadata.MetadataToken.ToUInt32();
            var openRuntime = RequireRuntimeDeclaredZeroArgSingleplayerSubmenuMethod(menu.GetType(), step);
            if (unchecked((uint)openRuntime.MetadataToken) != _step53OpenMethodToken)
                throw new InvalidDataException($"Step 58 acquisition runtime OpenSingleplayerSubmenu token drifted: metadata=0x{_step53OpenMethodToken:X8}; runtime=0x{unchecked((uint)openRuntime.MetadataToken):X8}.");

            var submenuStack = RequireRuntimeExactObjectProperty(menu, MainMenuSubmenuStackPropertyName, MainMenuSubmenuStackManagedTypeFullName, step);
            var submenuField = RequireRuntimeExactInstanceField(submenuStack.GetType(), MainMenuSingleplayerFieldName, SingleplayerSubmenuManagedTypeFullName, step);
            var submenu = submenuField.GetValue(submenuStack);
            var preexisting = submenu is not null;
            var preInside = submenu is not null && Convert.ToBoolean(RequireZeroArgBoolMethod(submenu.GetType(), "IsInsideTree").Invoke(submenu, null), System.Globalization.CultureInfo.InvariantCulture);
            var preVisible = submenu is not null && RequireRuntimeBoolProperty(submenu, "Visible", step);
            var preVisibleInTree = submenu is not null && Convert.ToBoolean(RequireZeroArgBoolMethod(submenu.GetType(), "IsVisibleInTree").Invoke(submenu, null), System.Globalization.CultureInfo.InvariantCulture);

            _step54SubmenuStack = submenuStack;
            _step54SingleplayerSubmenu = submenu;
            _step54SubmenuExistedBefore = preexisting;
            _step54SubmenuInsideTreeBefore = preInside;
            _step54SubmenuVisibleBefore = preVisible;
            _step54SubmenuVisibleInTreeBefore = preVisibleInTree;

            if (submenu is null || !preInside || !preVisible || !preVisibleInTree)
            {
                Checkpoint(checkpoint, $"M58_A_ACQUIRE_OPEN_ARMED — current architecture needs the real game-owned NSingleplayerSubmenu materialized/activated; invoking exact OpenSingleplayerSubmenu token=0x{_step53OpenMethodToken:X8} once while rendering remains frozen; preexisting={preexisting}; inside={preInside}; visible={preVisible}; visibleInTree={preVisibleInTree}.");
                object? returned;
                try
                {
                    returned = openRuntime.Invoke(menu, null);
                }
                catch (TargetInvocationException tie) when (tie.InnerException is not null)
                {
                    Checkpoint(checkpoint, $"M58_A_ACQUIRE_OPEN_INNER_EXCEPTION — type={SanitizeCheckpoint(tie.InnerException.GetType().FullName ?? tie.InnerException.GetType().Name)}; targetSite={SanitizeCheckpoint(tie.InnerException.TargetSite?.ToString() ?? "<null>")}; stack={SanitizeCheckpoint(tie.InnerException.StackTrace ?? "<null>")}");
                    ExceptionDispatchInfo.Capture(tie.InnerException).Throw();
                    throw new InvalidOperationException("Unreachable after preserved Step-58 acquisition exception dispatch.");
                }
                if (returned is null || returned.GetType().FullName != SingleplayerSubmenuManagedTypeFullName)
                    throw new InvalidDataException($"Step 58 acquisition OpenSingleplayerSubmenu returned invalid value: {returned?.GetType().FullName ?? "<null>"}.");
                var fieldAfter = submenuField.GetValue(submenuStack) ?? throw new InvalidDataException("Step 58 acquisition left NMainMenuSubmenuStack._singleplayerSubmenu null.");
                if (!ReferenceEquals(returned, fieldAfter))
                    throw new InvalidDataException("Step 58 acquisition OpenSingleplayerSubmenu return identity did not match the retained stack field.");
                submenu = fieldAfter;
                _step54SingleplayerSubmenu = submenu;
                _step58CurrentOwnershipAcquisitionInvokedOpen = true;
            }
            else
            {
                Checkpoint(checkpoint, "M58_A_ACQUIRE_OPEN_SKIPPED — retained NSingleplayerSubmenu is already visible/in-tree; current ownership path adopts it without duplicate OpenSingleplayerSubmenu invocation.");
            }

            if (submenu is null)
                throw new InvalidDataException("Step 58 acquisition has no retained NSingleplayerSubmenu after adoption/materialization.");
            var inside = Convert.ToBoolean(RequireZeroArgBoolMethod(submenu.GetType(), "IsInsideTree").Invoke(submenu, null), System.Globalization.CultureInfo.InvariantCulture);
            var visible = RequireRuntimeBoolProperty(submenu, "Visible", step);
            var visibleInTree = Convert.ToBoolean(RequireZeroArgBoolMethod(submenu.GetType(), "IsVisibleInTree").Invoke(submenu, null), System.Globalization.CultureInfo.InvariantCulture);
            if (!inside || !visible || !visibleInTree)
                throw new InvalidDataException($"Step 58 acquisition requires a visible/in-tree retained NSingleplayerSubmenu. inside={inside}; visible={visible}; visibleInTree={visibleInTree}.");

            var packedScene = RequireRetainedCharacterSelectPackedSceneForOwnership(step);
            _step57CharacterSelectResourcePath = RequireRuntimeStringProperty(packedScene, GodotResourcePathPropertyName, step).Replace('\\', '/');
            if (!string.Equals(_step57CharacterSelectResourcePath, CharacterSelectSceneResourcePath, StringComparison.Ordinal))
                throw new InvalidDataException($"Step 58 acquisition character-select resource path drifted: {_step57CharacterSelectResourcePath}.");
            var pck = RequireEssentialResourcePackHandoff().PackAbsolutePath;
            var entry = ExtractStartupLadderPckEntryUnpinned(pck, _step57CharacterSelectResourcePath, step);
            if (entry.Bytes.Length == 0)
                throw new InvalidDataException("Step 58 acquisition character-select PCK entry is empty.");

            RequireStartupLadderBaselineUnchanged(context, _step58Baseline, "Step 58 acquisition post-adoption");
            RequireStep48LifecycleRuntimeGuardsCurrent(context, "Step 58 acquisition post-adoption guard recheck", requirePreAdmissionCounts: false);
            _step58CurrentOwnershipAcquisitionPassed = true;
            Checkpoint(checkpoint, $"M58_A_PASS — current ownership acquisition complete from Step-52 baseline; legacy Steps53-57 not required; OpenSingleplayerSubmenuInvoked={_step58CurrentOwnershipAcquisitionInvokedOpen}; submenuVisibleInTree=True; characterSelectResource={SanitizeCheckpoint(_step57CharacterSelectResourcePath)}; pckBytes={entry.Bytes.Length}; rendererStopped=True.");
            return StartupLadderPass(step, Step58Name, gate,
                "Current ownership prerequisites were acquired directly from the physically closed Step-52 main-menu baseline. Legacy Steps 53-57 are diagnostic history, not mandatory choreography.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"M58_A_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return StartupLadderFail(step, Step58Name, gate, stage, ex);
        }
    }

    public TransformedRealStS2StartupLadderGateResult RunStep58RuntimeOwnershipBinding(Action<string>? checkpoint = null)
    {
        const int step = 58; const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.StaticAuditOrBinding; var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep58Prerequisite("Step 58 Gate B entry");
            var baseline = _step58Baseline ?? throw new InvalidOperationException("Step 58.0 Gate A must pass before Gate B.");
            using var resolver = new RejectingAssemblyResolver();
            using var module = OpenStartupLadderModule(baseline.SelectedPath, resolver);
            var allTypes = EnumerateTypes(module.Types).ToDictionary(type => type.FullName, StringComparer.Ordinal);
            var submenuType = RequireStartupLadderType(allTypes, SingleplayerSubmenuManagedTypeFullName, step);
            var open = RequireUniqueStartupLadderNamedMethod(submenuType, SingleplayerOpenCharacterSelectMethodName, step);
            RequireExactStep56OpenCharacterSelectSignature(open);
            if (ReadsFirstExplicitParameter(open)) throw new InvalidDataException("Step 58.0 refuses null-button handler invocation because OpenCharacterSelect reads its NButton parameter in the selected assembly.");
            _step58OpenCharacterSelectToken = open.MetadataToken.ToUInt32();

            var submenu = _step54SingleplayerSubmenu ?? throw new InvalidOperationException("Step 58.0 requires retained NSingleplayerSubmenu authority.");
            _step58RuntimeOpenCharacterSelect = RequireRuntimeMethodByTokenForOwnership(submenu.GetType(), _step58OpenCharacterSelectToken, SingleplayerOpenCharacterSelectMethodName, step, 1, "System.Void");
            var runtimeParam = _step58RuntimeOpenCharacterSelect.GetParameters()[0].ParameterType.FullName;
            if (runtimeParam != SingleplayerOpenCharacterSelectParameterTypeFullName) throw new InvalidDataException($"Step 58.0 runtime handler parameter drifted: {runtimeParam}.");

            var stack = _step54SubmenuStack ?? throw new InvalidOperationException("Step 58.0 retained submenu stack is absent.");
            var cache = RequireRuntimeExactInstanceField(stack.GetType(), MainMenuCharacterSelectSubmenuFieldName, CharacterSelectScreenManagedTypeFullName, step);
            var screen = cache.GetValue(stack);
            CharacterSelectRuntimeState state;
            if (screen is null)
            {
                state = CharacterSelectRuntimeState.Absent;
            }
            else
            {
                RequireCharacterSelectIdentity(screen, stack, context, step);
                state = CaptureCharacterSelectRuntimeState(screen, stack, step);
            }
            _step58ObservedCharacterSelect = screen;
            _step58ObservedState = state;
            _step58TransitionAlreadyComplete = screen is not null && state.InsideTree && state.Visible && state.VisibleInTree && state.StackIsExactRetainedStack;

            _step58StaticMap =
                "StS2 Launcher — Step 58.0 real character-select runtime ownership audit\n" +
                "Evidence-only. Step 58 does not invoke OpenCharacterSelect or mutate the SceneTree.\n" +
                $"Selected compatibility SHA-256: {baseline.SelectedSha256}\n" +
                $"OpenCharacterSelect token=0x{_step58OpenCharacterSelectToken:X8}; params=1; parameter0={SingleplayerOpenCharacterSelectParameterTypeFullName}; return=System.Void\n" +
                "NButton parameter read by IL: NO\n" +
                $"Observed cache state: {state}\n" +
                $"Transition already visibly complete: {_step58TransitionAlreadyComplete}\n" +
                "Rendering restarted: NO\nOpenCharacterSelect invoked: NO\n" +
                "[OPENCHARACTERSELECT IL]\n" + string.Join("\n", open.Body.Instructions.Select(instruction => "  " + FormatStep41Instruction(instruction))) + "\n";
            if (resolver.Requests.Count != 0) throw new InvalidDataException("Step 58.0 binding attempted external Cecil resolution: " + string.Join(" | ", resolver.Requests));
            RequireStartupLadderBaselineUnchanged(context, baseline, "Step 58 Gate B");
            Checkpoint(checkpoint, $"M58_B_PASS — exact OpenCharacterSelect token=0x{_step58OpenCharacterSelectToken:X8}; NButtonUnused=True; observedState={SanitizeCheckpoint(state.ToString())}; transitionAlreadyComplete={_step58TransitionAlreadyComplete}; invocation=NO.");
            return StartupLadderPass(step, Step58Name, gate, "Exact handler and actual runtime-owned character-select cache/tree state bound without imposing a null/off-tree prediction.");
        }
        catch (Exception ex) { Checkpoint(checkpoint, $"M58_B_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}"); return StartupLadderFail(step, Step58Name, gate, stage, ex); }
    }

    public TransformedRealStS2StartupLadderGateResult RunStep58OpenCharacterSelectFrontierAudit(Action<string>? checkpoint = null)
    {
        const int step = 58; const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.ControlledAction; var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep58Prerequisite("Step 58 Gate C entry");
            var baseline = _step58Baseline ?? throw new InvalidOperationException("Step 58.0 baseline absent.");
            using var resolver = new RejectingAssemblyResolver();
            using var module = OpenStartupLadderModule(baseline.SelectedPath, resolver);
            var allTypes = EnumerateTypes(module.Types).ToDictionary(type => type.FullName, StringComparer.Ordinal);
            var allMethods = BuildStartupLadderMethodMap(allTypes);
            var open = FindMethodByToken(module, _step58OpenCharacterSelectToken);
            RequireExactStep56OpenCharacterSelectSignature(open);
            var audit = AuditStartupLadderInvocationFrontier([open], allTypes, allMethods,
                _step48LifecycleGuardAuthority ?? throw new InvalidOperationException("Step 58.0 requires retained Step-48 runtime guards."));
            RequireImmediateFrontierAdmissible(audit, "Step 58.0 exact OpenCharacterSelect frontier under retained runtime guards");
            if (resolver.Requests.Count != 0) throw new InvalidDataException("Step 58.0 handler frontier audit attempted external Cecil resolution: " + string.Join(" | ", resolver.Requests));
            _step58StaticMap += BuildStartupLadderInvocationFrontierAppendix(audit);
            _step58OwnershipMapped = true;
            RequireStartupLadderBaselineUnchanged(context, baseline, "Step 58 Gate C");
            Checkpoint(checkpoint, $"M58_C_PASS — real OpenCharacterSelect frontier mapped without invocation; closure={audit.ImmediateClosureMethods.Length}; boundaries={audit.ImmediateBoundaries.Length}; guarded={audit.GuardedMethodFrontiers.Length}; deferred={audit.DeferredMethodFrontiers.Length}; unresolved/external=0.");
            return StartupLadderPass(step, Step58Name, gate, "The real handler is re-audited as one game-owned transition. No internal create/init/push milestone is forced by the launcher.");
        }
        catch (Exception ex) { Checkpoint(checkpoint, $"M58_C_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}"); return StartupLadderFail(step, Step58Name, gate, stage, ex); }
    }

    public TransformedRealStS2StartupLadderGateResult RunStep58FrozenOwnershipConfinement(bool renderingStopped, Action<string>? checkpoint = null)
    {
        const int step = 58; const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.PostActionConfinement; var stage = "initialization";
        try
        {
            ThrowIfDisposed(); var context = RequireStep58Prerequisite("Step 58 Gate D entry"); var baseline = _step58Baseline ?? throw new InvalidOperationException("Step 58.0 baseline absent.");
            if (!_step58OwnershipMapped || !_step58StaticMapDurablyWritten || !renderingStopped) throw new InvalidOperationException("Step 58.0 Gate D requires durable ownership evidence with rendering frozen.");
            RequireStep58ObservedStateUnchanged(context, step);
            RequireStartupLadderBaselineUnchanged(context, baseline, "Step 58 Gate D");
            _exactStep58ClosurePassed = true;
            Checkpoint(checkpoint, "M58_D_PASS — actual runtime ownership state retained unchanged; handler invocation=NO; renderingStopped=True; drift=0.");
            return StartupLadderPass(step, Step58Name, gate, "Runtime ownership audit closed 4/4. Step 59 may let the real handler complete the transition only if the game has not already done so.");
        }
        catch (Exception ex) { Checkpoint(checkpoint, $"M58_D_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}"); return StartupLadderFail(step, Step58Name, gate, stage, ex); }
    }

    // STEP 59 — use the original game handler as the transition owner, but only when it is still needed.
    public TransformedRealStS2StartupLadderGateResult RunStep59ClosedStep58Authority(bool renderingStopped, Action<string>? checkpoint = null)
    {
        const int step = 59; const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.PrerequisiteAuthority; var stage = "initialization";
        try
        {
            ThrowIfDisposed(); var context = RequireStep59Prerequisite("Step 59 Gate A entry"); if (!renderingStopped) throw new InvalidOperationException("Step 59.0 requires rendering frozen.");
            var selected = RequireStartupLadderSelectedAuthority(step); _step59Baseline = CaptureStartupLadderBaseline(selected.Path, selected.Sha256, context); RequireStartupLadderBaselineUnchanged(context, _step59Baseline, "Step 59 Gate A");
            RequireStep58ObservedStateUnchanged(context, step);
            Checkpoint(checkpoint, $"M59_A_PASS — Step-58 4/4 ownership authority retained; transitionAlreadyComplete={_step58TransitionAlreadyComplete}; renderer stopped; handler not armed.");
            return StartupLadderPass(step, Step59Name, gate, _step58TransitionAlreadyComplete ? "The game already owns a visible/in-tree character-select screen; Step 59 will adopt it without re-invoking the handler." : "The transition is not yet visibly complete; Step 59 may invoke the exact real OpenCharacterSelect handler once while rendering is frozen.");
        }
        catch (Exception ex) { Checkpoint(checkpoint, $"M59_A_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}"); return StartupLadderFail(step, Step59Name, gate, stage, ex); }
    }

    public TransformedRealStS2StartupLadderGateResult RunStep59RealHandlerBinding(Action<string>? checkpoint = null)
    {
        const int step = 59; const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.StaticAuditOrBinding; var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep59Prerequisite("Step 59 Gate B entry");
            var baseline = _step59Baseline ?? throw new InvalidOperationException("Step 59.0 Gate A must pass before Gate B.");
            var submenu = _step54SingleplayerSubmenu ?? throw new InvalidOperationException("Step 59.0 retained NSingleplayerSubmenu is absent.");
            var stack = _step54SubmenuStack ?? throw new InvalidOperationException("Step 59.0 retained NMainMenuSubmenuStack is absent.");

            _step58RuntimeOpenCharacterSelect = RequireRuntimeMethodByTokenForOwnership(submenu.GetType(), _step58OpenCharacterSelectToken, SingleplayerOpenCharacterSelectMethodName, step, 1, "System.Void");
            var openParameterType = _step58RuntimeOpenCharacterSelect.GetParameters()[0].ParameterType;
            var standardButtonField = RequireRuntimeInstanceFieldForOwnership(submenu.GetType(), SingleplayerStandardButtonFieldNameForOwnership, step);
            _step59StandardButton = standardButtonField.GetValue(submenu) ?? throw new InvalidDataException("Step 59.0 retained NSingleplayerSubmenu._standardButton is null.");
            if (!openParameterType.IsInstanceOfType(_step59StandardButton))
                throw new InvalidDataException($"Step 59.0 _standardButton runtime type {_step59StandardButton.GetType().FullName} is not assignable to OpenCharacterSelect parameter {openParameterType.FullName}.");

            var submenuStackField = RequireRuntimeExactInstanceField(submenu.GetType(), SubmenuStackFieldName, SubmenuStackManagedTypeFullName, step);
            var logicalStack = submenuStackField.GetValue(submenu);
            if (logicalStack is not null && !ReferenceEquals(logicalStack, stack))
                throw new InvalidDataException($"Step 59.0 retained NSingleplayerSubmenu is bound to a foreign logical stack: {logicalStack.GetType().FullName}.");
            _step59NavigationRepairRequired = logicalStack is null && !_step58TransitionAlreadyComplete;

            using var resolver = new RejectingAssemblyResolver();
            using var module = OpenStartupLadderModule(baseline.SelectedPath, resolver);
            var allTypes = EnumerateTypes(module.Types).ToDictionary(type => type.FullName, StringComparer.Ordinal);
            var allMethods = BuildStartupLadderMethodMap(allTypes);
            var stackType = RequireStartupLadderType(allTypes, SubmenuStackManagedTypeFullName, step);
            var pushCandidates = stackType.Methods.Where(method => method.Name == SubmenuStackPushMethodNameForOwnership && !method.IsStatic && !method.HasGenericParameters).ToArray();
            var push = pushCandidates.SingleOrDefault(method =>
                method.ReturnType.FullName == "System.Void" &&
                method.Parameters.Count == 1 &&
                method.Parameters[0].ParameterType.FullName == SubmenuManagedTypeFullName)
                ?? throw new MissingMethodException(SubmenuStackManagedTypeFullName, $"{SubmenuStackPushMethodNameForOwnership}({SubmenuManagedTypeFullName})");
            _step59SubmenuPushToken = push.MetadataToken.ToUInt32();
            var pushAudit = AuditStartupLadderInvocationFrontier([push], allTypes, allMethods,
                _step48LifecycleGuardAuthority ?? throw new InvalidOperationException("Step 59.0 requires retained Step-48 runtime guards."));
            RequireImmediateFrontierAdmissible(pushAudit, "Step 59.0 exact NSubmenuStack.Push navigation repair");
            if (resolver.Requests.Count != 0)
                throw new InvalidDataException("Step 59.0 NSubmenuStack.Push audit attempted external Cecil resolution: " + string.Join(" | ", resolver.Requests));

            _step59RuntimeSubmenuPush = RequireRuntimeMethodByTokenForOwnership(stack.GetType(), _step59SubmenuPushToken, SubmenuStackPushMethodNameForOwnership, step, 1, "System.Void");
            var runtimePushParameter = _step59RuntimeSubmenuPush.GetParameters()[0].ParameterType;
            if (!runtimePushParameter.IsInstanceOfType(submenu))
                throw new InvalidDataException($"Step 59.0 retained NSingleplayerSubmenu runtime type {submenu.GetType().FullName} is not assignable to Push parameter {runtimePushParameter.FullName}.");

            var before = CaptureCurrentCharacterSelectState(step, context);
            var forensicScreen = before.Screen ?? throw new InvalidDataException("Step 59.0 expected the normal preloaded NCharacterSelectScreen cache before forensic binding.");
            _step59PreflightSnapshot = CaptureStep59ForensicSnapshot(context, step, requireReadyPrerequisites: true);
            try
            {
                _step59ReadyBindingDiagnostics = BuildStep59ReadyBindingDiagnostics(forensicScreen, context, step, allTypes);
                _step59ConfirmButtonDiagnostics = BuildStep59ConfirmButtonOnEnableDiagnostics(forensicScreen, context, step, allTypes);
                _step59ConfirmButtonDiagnostics += "\n\n" + BuildStep59CompilationEfficientDiagnosticDeck(forensicScreen, context, step, allTypes, allMethods);
                _step59UniqueNameProvenanceDiagnostics = BuildStep59UniqueNameProvenanceDiagnostics(forensicScreen, context, step);
            }
            catch (Exception diagnosticEx)
            {
                _step59ReadyBindingDiagnostics += $"{(string.IsNullOrWhiteSpace(_step59ReadyBindingDiagnostics) ? string.Empty : "\n")}forensic-diagnostic-failed={diagnosticEx.GetType().FullName}: {diagnosticEx.Message}";
                if (string.IsNullOrWhiteSpace(_step59ReadyBindingBlocker))
                    _step59ReadyBindingBlocker = "ready/unique-name provenance diagnostic itself failed before handler authorization";
            }
            _step59ReadyBindingPreflightPassed = string.IsNullOrWhiteSpace(_step59ReadyBindingBlocker);
            Checkpoint(checkpoint, "M59_B_FORENSIC_PREFLIGHT — " + SanitizeCheckpoint(_step59PreflightSnapshot));
            Checkpoint(checkpoint, $"M59_B_READY_BINDING_DIAGNOSIS — handlerAuthorized={_step59ReadyBindingPreflightPassed}; blocker={SanitizeCheckpoint(string.IsNullOrWhiteSpace(_step59ReadyBindingBlocker) ? "<none>" : _step59ReadyBindingBlocker)}; {SanitizeCheckpoint(_step59ReadyBindingDiagnostics)}");
            Checkpoint(checkpoint, "M59_B_CONFIRM_BUTTON_PREFLIGHT — " + SanitizeCheckpoint(_step59ConfirmButtonDiagnostics));
            Checkpoint(checkpoint, "M59_B_UNIQUE_NAME_PROVENANCE — " + SanitizeCheckpoint(_step59UniqueNameProvenanceDiagnostics));
            _step59TransitionBound = true;
            _step59StaticMap = "StS2 Launcher — Step 59.0 character-select ready-binding forensics + game-owned transition\n" +
                $"NSubmenuStack.Push token: 0x{_step59SubmenuPushToken:X8}; parameter={SubmenuManagedTypeFullName}\n" +
                $"Single-player logical stack before repair: {(logicalStack is null ? "<null>" : logicalStack.GetType().FullName)}\n" +
                $"Navigation repair required: {_step59NavigationRepairRequired}\n" +
                $"OpenCharacterSelect token: 0x{_step58OpenCharacterSelectToken:X8}\n" +
                $"OpenCharacterSelect invocation required: {!_step58TransitionAlreadyComplete}\n" +
                $"Real standard button type: {_step59StandardButton.GetType().FullName}\n" +
                $"Before character-select transition: {before.State}\n" +
                $"Forensic preflight: {_step59PreflightSnapshot}\n" +
                $"Ready-binding handler authorization: {_step59ReadyBindingPreflightPassed}\n" +
                $"Ready-binding blocker: {(string.IsNullOrWhiteSpace(_step59ReadyBindingBlocker) ? "<none>" : _step59ReadyBindingBlocker)}\n" +
                "[READY-BINDING DIAGNOSTICS]\n" + _step59ReadyBindingDiagnostics + "\n" +
                "[CONFIRM-BUTTON ONENABLE PREFLIGHT]\n" + _step59ConfirmButtonDiagnostics + "\n" +
                "[UNIQUE-NAME PROVENANCE DIAGNOSTICS]\n" + _step59UniqueNameProvenanceDiagnostics + "\n" +
                "Rendering active before transition: NO\n" +
                BuildStartupLadderInvocationFrontierAppendix(pushAudit);
            RequireStartupLadderBaselineUnchanged(context, baseline, "Step 59 Gate B");
            Checkpoint(checkpoint, $"M59_B_PASS — exact OpenCharacterSelect and NSubmenuStack.Push rebound; forensicMapComplete=True; handlerAuthorized={_step59ReadyBindingPreflightPassed}; invocationRequired={!_step58TransitionAlreadyComplete}; navigationRepairRequired={_step59NavigationRepairRequired}; singleplayerLogicalStack={(logicalStack is null ? "<null>" : "exact-retained")}; pushToken=0x{_step59SubmenuPushToken:X8}; realStandardButton={SanitizeCheckpoint(_step59StandardButton.GetType().FullName ?? "<unknown>")}; beforeState={SanitizeCheckpoint(before.State.ToString())}.");
            return StartupLadderPass(step, Step59Name, gate, _step59ReadyBindingPreflightPassed
                ? "Exact game-owned stack Push/handler are bound and the scene-ready forensic preflight authorizes one original transition while rendering remains frozen."
                : "Ready-binding forensics localized a pre-handler scene lifecycle blocker. Evidence is complete and must be durably written; Gate C will stop before arming Push/OpenCharacterSelect.");
        }
        catch (Exception ex) { Checkpoint(checkpoint, $"M59_B_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}"); return StartupLadderFail(step, Step59Name, gate, stage, ex); }
    }


    public string RunStep59ControllerSingletonRehearsal(Action<string>? checkpoint = null)
    {
        const int step = 59;
        ThrowIfDisposed();
        var context = RequireStep59Prerequisite("Step 59R controller/singleton rehearsal entry");
        if (!_step59TransitionBound)
            throw new InvalidOperationException("Step 59R requires Step-59 Gate B binding before rehearsal.");
        if (!_step59ReadyBindingPreflightPassed)
            throw new InvalidOperationException("Step 59R requires a clean Step-59 ready-binding preflight.");
        if (_step59TransitionStarted || _step59DiagnosticMutationProbeStarted)
            throw new InvalidOperationException("Step 59R is observational and must run before any Step-59 mutation/handler probe in this process.");

        var embark = RequireStep59EmbarkButtonForProbe(context, step);
        _step59ControllerSingletonRehearsalDiagnostics = BuildStep59ControllerSingletonRehearsalDiagnostics(embark, context, step);
        _step59StaticMap += "\n[STEP 59R CONTROLLER / HOTKEY SINGLETON REHEARSAL]\n" + _step59ControllerSingletonRehearsalDiagnostics + "\n";
        Checkpoint(checkpoint, "M59R_COMPLETE — " + SanitizeCheckpoint(_step59ControllerSingletonRehearsalDiagnostics));
        return _step59ControllerSingletonRehearsalDiagnostics;
    }

    public (bool Passed, string Diagnostics) RunStep59RegisterHotkeysProbe(Action<string>? checkpoint = null)
    {
        const int step = 59;
        ThrowIfDisposed();
        var context = RequireStep59Prerequisite("Step 59H RegisterHotkeys probe entry");
        if (!_step59TransitionBound)
            throw new InvalidOperationException("Step 59H requires Step-59 Gate B binding before the isolated RegisterHotkeys probe.");
        if (!_step59ReadyBindingPreflightPassed)
            throw new InvalidOperationException("Step 59H requires a clean Step-59 ready-binding preflight.");
        if (_step59TransitionStarted || _step59DiagnosticMutationProbeStarted)
            throw new InvalidOperationException("Step 59H is one-shot and requires a fresh process with no prior Step-59 mutation/handler probe.");
        _step59DiagnosticMutationProbeStarted = true;

        var embark = RequireStep59EmbarkButtonForProbe(context, step);
        var before = BuildStep59FullInheritedFieldMatrix(embark, "STEP59H PRE-PROBE EMBARK FIELDS");
        var method = RequireStep59InstanceRuntimeMethod(embark.GetType(), "RegisterHotkeys", 0, step);
        Exception? failure = null;
        Checkpoint(checkpoint, $"M59H_INVOKE_ARMED — invoking exact inherited NButton.RegisterHotkeys token=0x{method.MetadataToken:X8} on retained embark button; no UpdateControllerButton/Enable/OnEnable/Push/OpenCharacterSelect.");
        try
        {
            method.Invoke(embark, null);
            Checkpoint(checkpoint, "M59H_INVOKE_RETURNED — exact RegisterHotkeys returned normally.");
        }
        catch (Exception ex)
        {
            failure = ex is TargetInvocationException tie && tie.InnerException is not null ? tie.InnerException : ex;
            Checkpoint(checkpoint, "M59H_INVOKE_EXCEPTION — " + SanitizeCheckpoint(DescribeStep59ProbeException(ex)));
        }
        var after = BuildStep59FullInheritedFieldMatrix(embark, "STEP59H POST-PROBE EMBARK FIELDS");
        _step59RegisterHotkeysProbeDiagnostics =
            $"methodToken=0x{method.MetadataToken:X8}; passed={failure is null}; runtimeType={embark.GetType().FullName}\n" +
            before + "\n" + after +
            (failure is null ? "\nexception=<none>" : "\nexception=" + DescribeStep59ProbeException(failure));
        _step59StaticMap += "\n[STEP 59H ISOLATED REGISTERHOTKEYS PROBE]\n" + _step59RegisterHotkeysProbeDiagnostics + "\n";
        Checkpoint(checkpoint, $"M59H_COMPLETE — passed={failure is null}; freshProcessRequired=True; realHandlerArmed=False.");
        return (failure is null, _step59RegisterHotkeysProbeDiagnostics);
    }

    public (bool Passed, string Diagnostics) RunStep59UpdateControllerButtonProbe(Action<string>? checkpoint = null)
    {
        const int step = 59;
        ThrowIfDisposed();
        var context = RequireStep59Prerequisite("Step 59U UpdateControllerButton probe entry");
        if (!_step59TransitionBound)
            throw new InvalidOperationException("Step 59U requires Step-59 Gate B binding before the isolated controller-update probe.");
        if (!_step59ReadyBindingPreflightPassed)
            throw new InvalidOperationException("Step 59U requires a clean Step-59 ready-binding preflight.");
        if (_step59TransitionStarted || _step59DiagnosticMutationProbeStarted)
            throw new InvalidOperationException("Step 59U is one-shot and requires a fresh process with no prior Step-59 mutation/handler probe.");
        _step59DiagnosticMutationProbeStarted = true;

        var embark = RequireStep59EmbarkButtonForProbe(context, step);
        var before = BuildStep59FullInheritedFieldMatrix(embark, "STEP59U PRE-PROBE EMBARK FIELDS");
        var method = RequireStep59InstanceRuntimeMethod(embark.GetType(), "UpdateControllerButton", 0, step);
        Exception? failure = null;
        Checkpoint(checkpoint, $"M59U_INVOKE_ARMED — invoking exact inherited NButton.UpdateControllerButton token=0x{method.MetadataToken:X8} on retained embark button; no Enable/OnEnable/RegisterHotkeys/Push/OpenCharacterSelect.");
        try
        {
            method.Invoke(embark, null);
            Checkpoint(checkpoint, "M59U_INVOKE_RETURNED — exact UpdateControllerButton returned normally.");
        }
        catch (Exception ex)
        {
            failure = ex is TargetInvocationException tie && tie.InnerException is not null ? tie.InnerException : ex;
            Checkpoint(checkpoint, "M59U_INVOKE_EXCEPTION — " + SanitizeCheckpoint(DescribeStep59ProbeException(ex)));
        }

        var after = BuildStep59FullInheritedFieldMatrix(embark, "STEP59U POST-PROBE EMBARK FIELDS");
        _step59UpdateControllerProbeDiagnostics =
            $"methodToken=0x{method.MetadataToken:X8}; passed={failure is null}; runtimeType={embark.GetType().FullName}\n" +
            before + "\n" + after +
            (failure is null ? "\nexception=<none>" : "\nexception=" + DescribeStep59ProbeException(failure));
        _step59StaticMap += "\n[STEP 59U ISOLATED UPDATECONTROLLERBUTTON PROBE]\n" + _step59UpdateControllerProbeDiagnostics + "\n";
        Checkpoint(checkpoint, $"M59U_COMPLETE — passed={failure is null}; freshProcessRequired=True.");
        return (failure is null, _step59UpdateControllerProbeDiagnostics);
    }

    public (bool Passed, string Diagnostics) RunStep59IsolatedEmbarkEnableProbe(Action<string>? checkpoint = null)
    {
        const int step = 59;
        ThrowIfDisposed();
        var context = RequireStep59Prerequisite("Step 59E isolated embark Enable probe entry");
        if (!_step59TransitionBound)
            throw new InvalidOperationException("Step 59E requires Step-59 Gate B binding before the isolated Enable probe.");
        if (!_step59ReadyBindingPreflightPassed)
            throw new InvalidOperationException("Step 59E requires a clean Step-59 ready-binding preflight.");
        if (_step59TransitionStarted || _step59DiagnosticMutationProbeStarted)
            throw new InvalidOperationException("Step 59E is one-shot and requires a fresh process with no prior Step-59 mutation/handler probe.");
        RequireStep59PropertyTweenerProfile(PropertyTweenerExperimentProfile.Full, "Step 59E isolated embark Enable probe");
        _step59DiagnosticMutationProbeStarted = true;

        var embark = RequireStep59EmbarkButtonForProbe(context, step);
        var enable = RequireStep59InstanceRuntimeMethod(embark.GetType(), "Enable", 0, step);
        SetStep59PropertyTweenerRepairMode(wrapperRepairEnabled: true, fluentRepairEnabled: true, checkpoint: checkpoint, checkpointMarker: "M59E_PROPERTY_TWEENER_REPAIR_MODE");
        var before = BuildStep59FullInheritedFieldMatrix(embark, "STEP59E PRE-PROBE EMBARK FIELDS");
        var compatBefore = BuildStep59PropertyTweenerCompatibilityRuntimeState();
        Exception? failure = null;
        Checkpoint(checkpoint, $"M59E_ENABLE_ARMED — invoking retained embark NClickableControl.Enable token=0x{enable.MetadataToken:X8} directly, outside OpenCharacterSelect, to isolate the button path. This probe may change only the retained embark button/hotkey/tween state and therefore requires a fresh process afterward.");
        try
        {
            enable.Invoke(embark, null);
            Checkpoint(checkpoint, "M59E_ENABLE_RETURNED — isolated retained embark Enable returned normally.");
        }
        catch (Exception ex)
        {
            failure = ex is TargetInvocationException tie && tie.InnerException is not null ? tie.InnerException : ex;
            Checkpoint(checkpoint, "M59E_ENABLE_EXCEPTION — " + SanitizeCheckpoint(DescribeStep59ProbeException(ex)));
        }

        var after = BuildStep59FullInheritedFieldMatrix(embark, "STEP59E POST-PROBE EMBARK FIELDS");
        var compatAfter = BuildStep59PropertyTweenerCompatibilityRuntimeState();
        Checkpoint(checkpoint, "M59E_PROPERTY_TWEENER_COMPAT — " + SanitizeCheckpoint(compatAfter));
        _step59IsolatedEnableProbeDiagnostics =
            $"enableToken=0x{enable.MetadataToken:X8}; passed={failure is null}; runtimeType={embark.GetType().FullName}\n" +
            compatBefore + "\n" + compatAfter + "\n" + before + "\n" + after +
            (failure is null ? "\nexception=<none>" : "\nexception=" + DescribeStep59ProbeException(failure));
        _step59StaticMap += "\n[STEP 59E ISOLATED RETAINED EMBARK ENABLE PROBE]\n" + _step59IsolatedEnableProbeDiagnostics + "\n";
        Checkpoint(checkpoint, $"M59E_COMPLETE — passed={failure is null}; freshProcessRequired=True; realHandlerArmed=False.");
        return (failure is null, _step59IsolatedEnableProbeDiagnostics);
    }

    public (bool Passed, string Diagnostics) RunStep59ConfirmButtonTailProbe(Action<string>? checkpoint = null)
        => RunStep59ConfirmButtonTailProbeCore(wrapperRepairEnabled: false, fluentRepairEnabled: false, probeCode: "59T", checkpoint: checkpoint);

    public (bool Passed, string Diagnostics) RunStep59ConfirmButtonTailWrapperRepairProbe(Action<string>? checkpoint = null)
        => RunStep59ConfirmButtonTailProbeCore(wrapperRepairEnabled: true, fluentRepairEnabled: false, probeCode: "59W", checkpoint: checkpoint);

    public (bool Passed, string Diagnostics) RunStep59ConfirmButtonTailRepairProbe(Action<string>? checkpoint = null)
        => RunStep59ConfirmButtonTailProbeCore(wrapperRepairEnabled: true, fluentRepairEnabled: true, probeCode: "59X", checkpoint: checkpoint);

    private (bool Passed, string Diagnostics) RunStep59ConfirmButtonTailProbeCore(bool wrapperRepairEnabled, bool fluentRepairEnabled, string probeCode, Action<string>? checkpoint)
    {
        const int step = 59;
        ThrowIfDisposed();
        var context = RequireStep59Prerequisite($"Step {probeCode} confirm-button tail probe entry");
        if (!_step59TransitionBound)
            throw new InvalidOperationException( $"Step {probeCode} requires Step-59 Gate B binding before the isolated confirm-button tail probe.");
        if (!_step59ReadyBindingPreflightPassed)
            throw new InvalidOperationException( $"Step {probeCode} requires a clean Step-59 ready-binding preflight.");
        if (_step59TransitionStarted || _step59DiagnosticMutationProbeStarted)
            throw new InvalidOperationException( $"Step {probeCode} is one-shot and requires a fresh process with no prior Step-59 mutation/handler probe.");
        var requiredProfile = probeCode switch
        {
            "59T" => PropertyTweenerExperimentProfile.Baseline,
            "59W" => PropertyTweenerExperimentProfile.Wrapper,
            "59X" => PropertyTweenerExperimentProfile.Full,
            _ => throw new InvalidOperationException($"Unknown Step-59 PropertyTweener probe code: {probeCode}."),
        };
        RequireStep59PropertyTweenerProfile(requiredProfile, $"Step {probeCode} confirm-button tail probe");
        _step59DiagnosticMutationProbeStarted = true;
        SetStep59PropertyTweenerRepairMode(wrapperRepairEnabled, fluentRepairEnabled, checkpoint, $"M{probeCode}_PROPERTY_TWEENER_REPAIR_MODE");

        var embark = RequireStep59EmbarkButtonForProbe(context, step);
        var before = BuildStep59FullInheritedFieldMatrix(embark, $"STEP{probeCode} PRE-PROBE EMBARK FIELDS");
        var compatBefore = BuildStep59PropertyTweenerCompatibilityRuntimeState();
        var stages = new List<string>();
        Exception? failure = null;

        bool Stage(string name, Action action)
        {
            if (failure is not null) return false;
            Checkpoint(checkpoint, $"M{probeCode}_STAGE_PRE — {name}");
            try
            {
                action();
                stages.Add(name + "=PASS");
                Checkpoint(checkpoint, $"M{probeCode}_STAGE_POST — {name} returned normally.");
                return true;
            }
            catch (Exception ex)
            {
                failure = ex is TargetInvocationException tie && tie.InnerException is not null ? tie.InnerException : ex;
                stages.Add(name + "=FAIL:" + DescribeStep59ProbeException(ex));
                Checkpoint(checkpoint, $"M{probeCode}_STAGE_FAIL — {name}; {SanitizeCheckpoint(DescribeStep59ProbeException(ex))}");
                return false;
            }
        }

        var outline = RequireRuntimeInstanceFieldForOwnership(embark.GetType(), "_outline", step).GetValue(embark)
            ?? throw new InvalidDataException($"Step {probeCode} retained embark _outline is null.");
        var image = RequireRuntimeInstanceFieldForOwnership(embark.GetType(), "_buttonImage", step).GetValue(embark)
            ?? throw new InvalidDataException($"Step {probeCode} retained embark _buttonImage is null.");
        var moveTween = RequireRuntimeInstanceFieldForOwnership(embark.GetType(), "_moveTween", step).GetValue(embark);
        var showPos = RequireRuntimeInstanceFieldForOwnership(embark.GetType(), "_showPos", step).GetValue(embark)
            ?? throw new InvalidDataException($"Step {probeCode} retained embark _showPos could not be read.");
        var godotAssembly = (_callbackHandoff ?? throw new InvalidOperationException($"Step {probeCode} GodotSharp handoff absent.")).GodotSharpAssembly;
        var colorsType = godotAssembly.GetType("Godot.Colors", throwOnError: true, ignoreCase: false) ?? throw new MissingMemberException("Godot.Colors");
        var transparent = colorsType.GetProperty("Transparent", BindingFlags.Static | BindingFlags.Public)?.GetValue(null)
            ?? throw new MissingMemberException("Godot.Colors.Transparent");
        var white = colorsType.GetProperty("White", BindingFlags.Static | BindingFlags.Public)?.GetValue(null)
            ?? throw new MissingMemberException("Godot.Colors.White");
        var outlineModulate = outline.GetType().GetProperty("Modulate", BindingFlags.Instance | BindingFlags.Public)
            ?? throw new MissingMemberException(outline.GetType().FullName, "Modulate");
        var imageModulate = image.GetType().GetProperty("Modulate", BindingFlags.Instance | BindingFlags.Public)
            ?? throw new MissingMemberException(image.GetType().FullName, "Modulate");

        object? newTween = null;
        object? propertyTweener = null;
        Stage("outline.Modulate=Transparent", () => outlineModulate.SetValue(outline, transparent));
        Stage("buttonImage.Modulate=White", () => imageModulate.SetValue(image, white));
        if (moveTween is not null)
            Stage("existingTween.Kill", () => RequireStep59InstanceRuntimeMethod(moveTween.GetType(), "Kill", 0, step).Invoke(moveTween, null));
        else
            stages.Add("existingTween.Kill=SKIP_NULL");
        Stage("Node.CreateTween", () =>
        {
            newTween = RequireStep59InstanceRuntimeMethod(embark.GetType(), "CreateTween", 0, step).Invoke(embark, null)
                ?? throw new NullReferenceException("Node.CreateTween returned null.");
        });
        Stage("Tween.TweenProperty", () =>
        {
            if (newTween is null) throw new InvalidOperationException("CreateTween did not produce a tween.");
            var nodePathType = godotAssembly.GetType("Godot.NodePath", true, false) ?? throw new MissingMemberException("Godot.NodePath");
            var variantType = godotAssembly.GetType("Godot.Variant", true, false) ?? throw new MissingMemberException("Godot.Variant");
            var nodePathImplicit = nodePathType.GetMethods(BindingFlags.Static | BindingFlags.Public).Single(method => method.Name == "op_Implicit" && method.GetParameters().Length == 1 && method.GetParameters()[0].ParameterType == typeof(string));
            var variantImplicit = variantType.GetMethods(BindingFlags.Static | BindingFlags.Public).Single(method => method.Name == "op_Implicit" && method.GetParameters().Length == 1 && method.GetParameters()[0].ParameterType == showPos.GetType());
            var nodePath = nodePathImplicit.Invoke(null, new object?[] { "position" }) ?? throw new NullReferenceException("NodePath implicit conversion returned null.");
            var variant = variantImplicit.Invoke(null, new[] { showPos }) ?? throw new NullReferenceException("Variant implicit conversion returned null.");
            var tweenProperty = newTween.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public).Single(method => method.Name == "TweenProperty" && method.GetParameters().Length == 4);
            propertyTweener = tweenProperty.Invoke(newTween, new object?[] { embark, nodePath, variant, 0.35D })
                ?? throw new NullReferenceException("Tween.TweenProperty returned null.");
        });
        Stage("PropertyTweener.SetEase", () =>
        {
            if (propertyTweener is null) throw new InvalidOperationException("TweenProperty did not produce a PropertyTweener.");
            var method = RequireStep59InstanceRuntimeMethod(propertyTweener.GetType(), "SetEase", 1, step);
            var enumValue = Enum.ToObject(method.GetParameters()[0].ParameterType, 1L);
            if (method.Invoke(propertyTweener, new[] { enumValue }) is null) throw new NullReferenceException("PropertyTweener.SetEase returned null.");
        });
        Stage("PropertyTweener.SetTrans", () =>
        {
            if (propertyTweener is null) throw new InvalidOperationException("TweenProperty did not produce a PropertyTweener.");
            var method = RequireStep59InstanceRuntimeMethod(propertyTweener.GetType(), "SetTrans", 1, step);
            var enumValue = Enum.ToObject(method.GetParameters()[0].ParameterType, 10L);
            if (method.Invoke(propertyTweener, new[] { enumValue }) is null) throw new NullReferenceException("PropertyTweener.SetTrans returned null.");
        });
        Stage("PropertyTweener.FromCurrent", () =>
        {
            if (propertyTweener is null) throw new InvalidOperationException("TweenProperty did not produce a PropertyTweener.");
            if (RequireStep59InstanceRuntimeMethod(propertyTweener.GetType(), "FromCurrent", 0, step).Invoke(propertyTweener, null) is null)
                throw new NullReferenceException("PropertyTweener.FromCurrent returned null.");
        });

        var after = BuildStep59FullInheritedFieldMatrix(embark, $"STEP{probeCode} POST-PROBE EMBARK FIELDS");
        var compatAfter = BuildStep59PropertyTweenerCompatibilityRuntimeState();
        Checkpoint(checkpoint, $"M{probeCode}_PROPERTY_TWEENER_COMPAT — {SanitizeCheckpoint(compatAfter)}");
        _step59ConfirmButtonTailProbeDiagnostics =
            $"probeCode={probeCode}; wrapperRepairEnabled={wrapperRepairEnabled}; fluentRepairEnabled={fluentRepairEnabled}; passed={failure is null}; runtimeType={embark.GetType().FullName}; stages={string.Join(" | ", stages)}\n" + compatBefore + "\n" + compatAfter + "\n" + before + "\n" + after +
            (failure is null ? "\nexception=<none>" : "\nexception=" + DescribeStep59ProbeException(failure));
        _step59StaticMap += $"\n[STEP {probeCode} ISOLATED NCONFIRMBUTTON POST-BASE TAIL PROBE]\n" + _step59ConfirmButtonTailProbeDiagnostics + "\n";
        Checkpoint(checkpoint, $"M{probeCode}_COMPLETE — wrapperRepairEnabled={wrapperRepairEnabled}; fluentRepairEnabled={fluentRepairEnabled}; passed={failure is null}; freshProcessRequired=True; realHandlerArmed=False; stages={SanitizeCheckpoint(string.Join(" | ", stages))}.");
        return (failure is null, _step59ConfirmButtonTailProbeDiagnostics);
    }

    public (bool Completed, bool WorkingAlternativeFound, string Diagnostics) RunStep59NativeTweenRejectionMatrix(Action<string>? checkpoint = null)
    {
        const int step = 59;
        ThrowIfDisposed();
        var context = RequireStep59Prerequisite("Step 59N native Tween rejection matrix entry");
        if (!_step59TransitionBound) throw new InvalidOperationException("Step 59N requires Step-59 Gate B binding before the matrix.");
        if (!_step59ReadyBindingPreflightPassed) throw new InvalidOperationException("Step 59N requires a clean Step-59 ready-binding preflight.");
        if (_step59TransitionStarted || _step59DiagnosticMutationProbeStarted) throw new InvalidOperationException("Step 59N is one-shot and requires a fresh process with no prior Step-59 mutation/handler probe.");
        RequireStep59PropertyTweenerProfile(PropertyTweenerExperimentProfile.Full, "Step 59N native Tween rejection matrix");
        _step59DiagnosticMutationProbeStarted = true;
        SetStep59PropertyTweenerRepairMode(wrapperRepairEnabled: false, fluentRepairEnabled: false, checkpoint: checkpoint, checkpointMarker: "M59N_PROPERTY_TWEENER_REPAIR_MODE");

        var embark = RequireStep59EmbarkButtonForProbe(context, step);
        var embarkType = embark.GetType();
        var godotAssembly = (_callbackHandoff ?? throw new InvalidOperationException("Step 59N GodotSharp handoff absent.")).GodotSharpAssembly;
        var oldTween = RequireRuntimeInstanceFieldForOwnership(embarkType, "_moveTween", step).GetValue(embark);
        var showPos = RequireRuntimeInstanceFieldForOwnership(embarkType, "_showPos", step).GetValue(embark)
            ?? throw new InvalidDataException("Step 59N retained embark _showPos could not be read.");
        var outline = RequireRuntimeInstanceFieldForOwnership(embarkType, "_outline", step).GetValue(embark)
            ?? throw new InvalidDataException("Step 59N retained embark _outline is null.");

        object? ReadPublicProperty(object target, string name)
            => target.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Public)?.GetValue(target);

        string SafeCallBool(object target, string methodName)
        {
            try
            {
                var method = target.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public).FirstOrDefault(m => m.Name == methodName && m.GetParameters().Length == 0);
                if (method is null) return "<method-absent>";
                var value = method.Invoke(target, null);
                return value?.ToString() ?? "<null>";
            }
            catch (Exception ex) { return "<" + DescribeStep59ProbeException(ex) + ">"; }
        }

        IntPtr ReadNativePtr(object? target)
        {
            if (target is null) return IntPtr.Zero;
            for (var type = target.GetType(); type is not null; type = type.BaseType)
            {
                var field = type.GetField("NativePtr", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                if (field?.GetValue(target) is IntPtr ptr) return ptr;
            }
            return IntPtr.Zero;
        }

        string DescribeTween(object? tween)
        {
            if (tween is null) return "NULL";
            var ptr = ReadNativePtr(tween);
            return $"type={tween.GetType().FullName}; managedId={System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(tween)}; nativePtr=0x{ptr.ToInt64():X}; isValid={SafeCallBool(tween, "IsValid")}; isRunning={SafeCallBool(tween, "IsRunning")}; sameAsOld={ReferenceEquals(tween, oldTween)}; sameNativeAsOld={(oldTween is not null && ptr != IntPtr.Zero && ptr == ReadNativePtr(oldTween))}";
        }

        object CreateNodeTween()
            => RequireStep59InstanceRuntimeMethod(embarkType, "CreateTween", 0, step).Invoke(embark, null)
               ?? throw new NullReferenceException("Node.CreateTween returned null.");

        object CreateSceneTreeTween(bool bindNode)
        {
            var tree = RequireStep59InstanceRuntimeMethod(embarkType, "GetTree", 0, step).Invoke(embark, null)
                ?? throw new NullReferenceException("Node.GetTree returned null.");
            var create = tree.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public).SingleOrDefault(m => m.Name == "CreateTween" && m.GetParameters().Length == 0)
                ?? throw new MissingMethodException(tree.GetType().FullName, "CreateTween()");
            var tween = create.Invoke(tree, null) ?? throw new NullReferenceException("SceneTree.CreateTween returned null.");
            if (bindNode)
            {
                var bind = tween.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public).SingleOrDefault(m => m.Name == "BindNode" && m.GetParameters().Length == 1)
                    ?? throw new MissingMethodException(tween.GetType().FullName, "BindNode(Node)");
                var bound = bind.Invoke(tween, new[] { embark });
                if (bound is null) throw new NullReferenceException("Tween.BindNode returned null.");
                tween = bound;
            }
            return tween;
        }

        object MakeNodePath(string property)
        {
            var nodePathType = godotAssembly.GetType("Godot.NodePath", true, false) ?? throw new MissingMemberException("Godot.NodePath");
            var implicitMethod = nodePathType.GetMethods(BindingFlags.Static | BindingFlags.Public).Single(m => m.Name == "op_Implicit" && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == typeof(string));
            return implicitMethod.Invoke(null, new object?[] { property }) ?? throw new NullReferenceException("NodePath implicit conversion returned null.");
        }

        object MakeVariant(object value)
        {
            var variantType = godotAssembly.GetType("Godot.Variant", true, false) ?? throw new MissingMemberException("Godot.Variant");
            var implicitMethod = variantType.GetMethods(BindingFlags.Static | BindingFlags.Public).Single(m => m.Name == "op_Implicit" && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == value.GetType());
            return implicitMethod.Invoke(null, new[] { value }) ?? throw new NullReferenceException("Variant implicit conversion returned null.");
        }

        object InvokeTweenProperty(object tween, object target, string property, object value)
        {
            var method = tween.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public).Single(m => m.Name == "TweenProperty" && m.GetParameters().Length == 4);
            return method.Invoke(tween, new object?[] { target, MakeNodePath(property), MakeVariant(value), 0.35D })
                ?? throw new NullReferenceException($"Tween.TweenProperty returned null for {target.GetType().FullName}.{property} valueType={value.GetType().FullName}.");
        }

        void InvokeFullFluentTail(object propertyTweener)
        {
            var ease = RequireStep59InstanceRuntimeMethod(propertyTweener.GetType(), "SetEase", 1, step);
            var easeValue = Enum.ToObject(ease.GetParameters()[0].ParameterType, 1L);
            propertyTweener = ease.Invoke(propertyTweener, new[] { easeValue }) ?? throw new NullReferenceException("PropertyTweener.SetEase returned null.");
            var trans = RequireStep59InstanceRuntimeMethod(propertyTweener.GetType(), "SetTrans", 1, step);
            var transValue = Enum.ToObject(trans.GetParameters()[0].ParameterType, 10L);
            propertyTweener = trans.Invoke(propertyTweener, new[] { transValue }) ?? throw new NullReferenceException("PropertyTweener.SetTrans returned null.");
            propertyTweener = RequireStep59InstanceRuntimeMethod(propertyTweener.GetType(), "FromCurrent", 0, step).Invoke(propertyTweener, null)
                ?? throw new NullReferenceException("PropertyTweener.FromCurrent returned null.");
        }

        var rows = new List<string>();
        var workingAlternativeFound = false;
        bool RunRow(string name, Func<object> createTween, Func<object, object?> operation, bool marksAlternative = false)
        {
            Checkpoint(checkpoint, $"M59N_ROW_PRE — {name}");
            try
            {
                var tween = createTween();
                var beforeState = DescribeTween(tween);
                var result = operation(tween);
                var resultPtr = ReadNativePtr(result);
                var afterState = DescribeTween(tween);
                var compat = BuildStep59PropertyTweenerCompatibilityRuntimeState();
                rows.Add($"{name}=PASS; tweenBefore=[{beforeState}]; resultType={result?.GetType().FullName ?? "<null>"}; resultNativePtr=0x{resultPtr.ToInt64():X}; tweenAfter=[{afterState}]; compat=[{compat}]");
                if (marksAlternative) workingAlternativeFound = true;
                Checkpoint(checkpoint, $"M59N_ROW_POST — {name}; PASS; resultType={result?.GetType().FullName ?? "<null>"}; resultNativePtr=0x{resultPtr.ToInt64():X}; {SanitizeCheckpoint(afterState)}");
                return true;
            }
            catch (Exception ex)
            {
                var failure = ex is TargetInvocationException tie && tie.InnerException is not null ? tie.InnerException : ex;
                var compat = BuildStep59PropertyTweenerCompatibilityRuntimeState();
                rows.Add($"{name}=FAIL; {DescribeStep59ProbeException(failure)}; compat=[{compat}]");
                Checkpoint(checkpoint, $"M59N_ROW_FAIL — {name}; {SanitizeCheckpoint(DescribeStep59ProbeException(failure))}; {SanitizeCheckpoint(compat)}");
                return false;
            }
        }

        var position = ReadPublicProperty(embark, "Position");
        var scale = ReadPublicProperty(embark, "Scale");
        var embarkModulate = ReadPublicProperty(embark, "Modulate");
        var outlineModulate = ReadPublicProperty(outline, "Modulate");
        var compatBefore = BuildStep59PropertyTweenerCompatibilityRuntimeState();
        rows.Add($"MATRIX_CONTEXT; embarkNativePtr=0x{ReadNativePtr(embark).ToInt64():X}; oldTweenBefore=[{DescribeTween(oldTween)}]; showPosType={showPos.GetType().FullName}; positionType={position?.GetType().FullName ?? "<null>"}; scaleType={scale?.GetType().FullName ?? "<null>"}; embarkModulateType={embarkModulate?.GetType().FullName ?? "<null>"}; outlineModulateType={outlineModulate?.GetType().FullName ?? "<null>"}; compatBefore=[{compatBefore}]");

        RunRow("NODE_INTERVAL_PRE_OLD_KILL", CreateNodeTween, tween =>
        {
            var method = tween.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public).SingleOrDefault(m => m.Name == "TweenInterval" && m.GetParameters().Length == 1)
                ?? throw new MissingMethodException(tween.GetType().FullName, "TweenInterval(double)");
            return method.Invoke(tween, new object?[] { 0.35D }) ?? throw new NullReferenceException("Tween.TweenInterval returned null.");
        });

        if (oldTween is not null)
        {
            try
            {
                RequireStep59InstanceRuntimeMethod(oldTween.GetType(), "Kill", 0, step).Invoke(oldTween, null);
                rows.Add("OLD_TWEEN_KILL=PASS; after=[" + DescribeTween(oldTween) + "]");
                Checkpoint(checkpoint, "M59N_OLD_TWEEN_KILL_POST — " + SanitizeCheckpoint(DescribeTween(oldTween)));
            }
            catch (Exception ex)
            {
                var failure = ex is TargetInvocationException tie && tie.InnerException is not null ? tie.InnerException : ex;
                rows.Add("OLD_TWEEN_KILL=FAIL; " + DescribeStep59ProbeException(failure));
                Checkpoint(checkpoint, "M59N_OLD_TWEEN_KILL_FAIL — " + SanitizeCheckpoint(DescribeStep59ProbeException(failure)));
            }
        }
        else rows.Add("OLD_TWEEN_KILL=SKIP_NULL");

        Func<object, object?> interval = tween =>
        {
            var method = tween.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public).SingleOrDefault(m => m.Name == "TweenInterval" && m.GetParameters().Length == 1)
                ?? throw new MissingMethodException(tween.GetType().FullName, "TweenInterval(double)");
            return method.Invoke(tween, new object?[] { 0.35D }) ?? throw new NullReferenceException("Tween.TweenInterval returned null.");
        };
        RunRow("NODE_INTERVAL_POST_OLD_KILL", CreateNodeTween, interval);
        RunRow("SCENETREE_INTERVAL", () => CreateSceneTreeTween(false), interval);
        RunRow("SCENETREE_BOUND_INTERVAL", () => CreateSceneTreeTween(true), interval);

        if (position is not null) RunRow("NODE_PROPERTY_POSITION_CURRENT", CreateNodeTween, tween => InvokeTweenProperty(tween, embark, "position", position));
        RunRow("NODE_PROPERTY_POSITION_SHOWPOS", CreateNodeTween, tween => InvokeTweenProperty(tween, embark, "position", showPos));
        if (scale is not null) RunRow("NODE_PROPERTY_SCALE_CURRENT", CreateNodeTween, tween => InvokeTweenProperty(tween, embark, "scale", scale));
        if (embarkModulate is not null) RunRow("NODE_PROPERTY_MODULATE_CURRENT", CreateNodeTween, tween => InvokeTweenProperty(tween, embark, "modulate", embarkModulate));
        if (outlineModulate is not null) RunRow("NODE_PROPERTY_OUTLINE_MODULATE_CURRENT", CreateNodeTween, tween => InvokeTweenProperty(tween, outline, "modulate", outlineModulate));
        if (position is not null) RunRow("SCENETREE_PROPERTY_POSITION_CURRENT", () => CreateSceneTreeTween(false), tween => InvokeTweenProperty(tween, embark, "position", position));
        if (embarkModulate is not null) RunRow("SCENETREE_PROPERTY_MODULATE_CURRENT", () => CreateSceneTreeTween(false), tween => InvokeTweenProperty(tween, embark, "modulate", embarkModulate));
        if (outlineModulate is not null) RunRow("SCENETREE_PROPERTY_OUTLINE_MODULATE_CURRENT", () => CreateSceneTreeTween(false), tween => InvokeTweenProperty(tween, outline, "modulate", outlineModulate));

        RunRow("SCENETREE_PROPERTY_POSITION_SHOWPOS_FULLTAIL", () => CreateSceneTreeTween(false), tween =>
        {
            var result = InvokeTweenProperty(tween, embark, "position", showPos);
            InvokeFullFluentTail(result);
            return result;
        }, marksAlternative: true);
        RunRow("SCENETREE_BOUND_PROPERTY_POSITION_SHOWPOS_FULLTAIL", () => CreateSceneTreeTween(true), tween =>
        {
            var result = InvokeTweenProperty(tween, embark, "position", showPos);
            InvokeFullFluentTail(result);
            return result;
        }, marksAlternative: true);

        var compatAfter = BuildStep59PropertyTweenerCompatibilityRuntimeState();
        _step59NativeTweenMatrixDiagnostics =
            $"completed=True; workingAlternativeFound={workingAlternativeFound}; profile={SelectedPropertyTweenerProfile}; oldTweenFinal=[{DescribeTween(oldTween)}]\n" +
            string.Join("\n", rows) + "\ncompatAfter=" + compatAfter;
        _step59StaticMap += "\n[STEP 59N NATIVE TWEEN REJECTION MATRIX]\n" + _step59NativeTweenMatrixDiagnostics + "\n";
        Checkpoint(checkpoint, $"M59N_COMPLETE — workingAlternativeFound={workingAlternativeFound}; rows={rows.Count}; freshProcessRequired=True; realHandlerArmed=False.");
        return (true, workingAlternativeFound, _step59NativeTweenMatrixDiagnostics);
    }

    public (bool Passed, string Diagnostics) RunStep59SceneTreeAlternativeTailProbe(bool bindNode, Action<string>? checkpoint = null)
    {
        const int step = 59;
        var code = bindNode ? "59Z" : "59Y";
        ThrowIfDisposed();
        var context = RequireStep59Prerequisite($"Step {code} SceneTree alternative tail entry");
        if (!_step59TransitionBound) throw new InvalidOperationException($"Step {code} requires Step-59 Gate B binding.");
        if (!_step59ReadyBindingPreflightPassed) throw new InvalidOperationException($"Step {code} requires a clean Step-59 ready-binding preflight.");
        if (_step59TransitionStarted || _step59DiagnosticMutationProbeStarted) throw new InvalidOperationException($"Step {code} is one-shot and requires a fresh process.");
        RequireStep59PropertyTweenerProfile(PropertyTweenerExperimentProfile.Full, $"Step {code} SceneTree alternative tail");
        _step59DiagnosticMutationProbeStarted = true;
        SetStep59PropertyTweenerRepairMode(wrapperRepairEnabled: false, fluentRepairEnabled: false, checkpoint: checkpoint, checkpointMarker: $"M{code}_PROPERTY_TWEENER_REPAIR_MODE");

        var embark = RequireStep59EmbarkButtonForProbe(context, step);
        var oldTween = RequireRuntimeInstanceFieldForOwnership(embark.GetType(), "_moveTween", step).GetValue(embark);
        var showPos = RequireRuntimeInstanceFieldForOwnership(embark.GetType(), "_showPos", step).GetValue(embark)
            ?? throw new InvalidDataException($"Step {code} retained embark _showPos could not be read.");
        var godotAssembly = (_callbackHandoff ?? throw new InvalidOperationException($"Step {code} GodotSharp handoff absent.")).GodotSharpAssembly;
        var stages = new List<string>();
        Exception? failure = null;
        object? tween = null;
        object? propertyTweener = null;

        bool Stage(string name, Action action)
        {
            if (failure is not null) return false;
            Checkpoint(checkpoint, $"M{code}_STAGE_PRE — {name}");
            try { action(); stages.Add(name + "=PASS"); Checkpoint(checkpoint, $"M{code}_STAGE_POST — {name} returned normally."); return true; }
            catch (Exception ex) { failure = ex is TargetInvocationException tie && tie.InnerException is not null ? tie.InnerException : ex; stages.Add(name + "=FAIL:" + DescribeStep59ProbeException(failure)); Checkpoint(checkpoint, $"M{code}_STAGE_FAIL — {name}; {SanitizeCheckpoint(DescribeStep59ProbeException(failure))}"); return false; }
        }

        if (oldTween is not null) Stage("existingTween.Kill", () => RequireStep59InstanceRuntimeMethod(oldTween.GetType(), "Kill", 0, step).Invoke(oldTween, null));
        else stages.Add("existingTween.Kill=SKIP_NULL");
        Stage("SceneTree.CreateTween", () =>
        {
            var tree = RequireStep59InstanceRuntimeMethod(embark.GetType(), "GetTree", 0, step).Invoke(embark, null) ?? throw new NullReferenceException("Node.GetTree returned null.");
            var create = tree.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public).SingleOrDefault(m => m.Name == "CreateTween" && m.GetParameters().Length == 0) ?? throw new MissingMethodException(tree.GetType().FullName, "CreateTween()");
            tween = create.Invoke(tree, null) ?? throw new NullReferenceException("SceneTree.CreateTween returned null.");
        });
        if (bindNode) Stage("Tween.BindNode(embark)", () =>
        {
            if (tween is null) throw new InvalidOperationException("SceneTree.CreateTween did not produce a Tween.");
            var bind = tween.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public).SingleOrDefault(m => m.Name == "BindNode" && m.GetParameters().Length == 1) ?? throw new MissingMethodException(tween.GetType().FullName, "BindNode(Node)");
            tween = bind.Invoke(tween, new[] { embark }) ?? throw new NullReferenceException("Tween.BindNode returned null.");
        });
        Stage("Tween.TweenProperty(position)", () =>
        {
            if (tween is null) throw new InvalidOperationException("Alternative path did not produce a Tween.");
            var nodePathType = godotAssembly.GetType("Godot.NodePath", true, false) ?? throw new MissingMemberException("Godot.NodePath");
            var variantType = godotAssembly.GetType("Godot.Variant", true, false) ?? throw new MissingMemberException("Godot.Variant");
            var nodePath = nodePathType.GetMethods(BindingFlags.Static | BindingFlags.Public).Single(m => m.Name == "op_Implicit" && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == typeof(string)).Invoke(null, new object?[] { "position" }) ?? throw new NullReferenceException("NodePath implicit conversion returned null.");
            var variant = variantType.GetMethods(BindingFlags.Static | BindingFlags.Public).Single(m => m.Name == "op_Implicit" && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == showPos.GetType()).Invoke(null, new[] { showPos }) ?? throw new NullReferenceException("Variant implicit conversion returned null.");
            var method = tween.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public).Single(m => m.Name == "TweenProperty" && m.GetParameters().Length == 4);
            propertyTweener = method.Invoke(tween, new object?[] { embark, nodePath, variant, 0.35D }) ?? throw new NullReferenceException("Tween.TweenProperty returned null.");
        });
        Stage("PropertyTweener.SetEase", () =>
        {
            if (propertyTweener is null) throw new InvalidOperationException("TweenProperty did not produce a PropertyTweener.");
            var method = RequireStep59InstanceRuntimeMethod(propertyTweener.GetType(), "SetEase", 1, step); var value = Enum.ToObject(method.GetParameters()[0].ParameterType, 1L);
            propertyTweener = method.Invoke(propertyTweener, new[] { value }) ?? throw new NullReferenceException("PropertyTweener.SetEase returned null.");
        });
        Stage("PropertyTweener.SetTrans", () =>
        {
            if (propertyTweener is null) throw new InvalidOperationException("SetEase did not preserve a PropertyTweener.");
            var method = RequireStep59InstanceRuntimeMethod(propertyTweener.GetType(), "SetTrans", 1, step); var value = Enum.ToObject(method.GetParameters()[0].ParameterType, 10L);
            propertyTweener = method.Invoke(propertyTweener, new[] { value }) ?? throw new NullReferenceException("PropertyTweener.SetTrans returned null.");
        });
        Stage("PropertyTweener.FromCurrent", () =>
        {
            if (propertyTweener is null) throw new InvalidOperationException("SetTrans did not preserve a PropertyTweener.");
            propertyTweener = RequireStep59InstanceRuntimeMethod(propertyTweener.GetType(), "FromCurrent", 0, step).Invoke(propertyTweener, null) ?? throw new NullReferenceException("PropertyTweener.FromCurrent returned null.");
        });

        var compat = BuildStep59PropertyTweenerCompatibilityRuntimeState();
        _step59AlternativeTweenTailDiagnostics = $"probeCode={code}; bindNode={bindNode}; passed={failure is null}; stages={string.Join(" | ", stages)}\n{compat}" + (failure is null ? "\nexception=<none>" : "\nexception=" + DescribeStep59ProbeException(failure));
        _step59StaticMap += $"\n[STEP {code} SCENETREE ALTERNATIVE CONFIRM-BUTTON TAIL]\n" + _step59AlternativeTweenTailDiagnostics + "\n";
        Checkpoint(checkpoint, $"M{code}_COMPLETE — bindNode={bindNode}; passed={failure is null}; freshProcessRequired=True; realHandlerArmed=False.");
        return (failure is null, _step59AlternativeTweenTailDiagnostics);
    }

    public TransformedRealStS2StartupLadderGateResult RunStep59RealHandlerTransition(Action<string>? checkpoint = null)
    {
        const int step = 59; const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.ControlledAction; var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep59Prerequisite("Step 59 Gate C entry");
            var baseline = _step59Baseline ?? throw new InvalidOperationException("Step 59.0 baseline absent.");
            if (!_step59TransitionBound) throw new InvalidOperationException("Step 59.0 exact handlers must be rebound before Gate C.");
            if (!_step59ReadyBindingPreflightPassed)
            {
                Checkpoint(checkpoint, $"M59_C_BLOCKED_READY_BINDING — no mutation/handler arm; blocker={SanitizeCheckpoint(_step59ReadyBindingBlocker)}; renderingStopped=True.");
                throw new InvalidDataException("Step 59.0 handler intentionally blocked before one-shot arm because ready-binding forensics found: " + _step59ReadyBindingBlocker);
            }
            RequireStep59PropertyTweenerProfile(PropertyTweenerExperimentProfile.Full, "real Step 59 game-owned transition");
            SetStep59PropertyTweenerRepairMode(wrapperRepairEnabled: true, fluentRepairEnabled: true, checkpoint: checkpoint, checkpointMarker: "M59_C_PROPERTY_TWEENER_REPAIR_MODE");
            if (!_step58TransitionAlreadyComplete)
            {
                if (_step59TransitionStarted) throw new InvalidOperationException("Step 59.0 game-owned stack repair + character-select transition is one-shot in-process.");
                _step59TransitionStarted = true;

                var submenu = _step54SingleplayerSubmenu ?? throw new InvalidOperationException("Step 59.0 retained NSingleplayerSubmenu is absent.");
                var stack = _step54SubmenuStack ?? throw new InvalidOperationException("Step 59.0 retained NMainMenuSubmenuStack is absent.");
                var submenuStackField = RequireRuntimeExactInstanceField(submenu.GetType(), SubmenuStackFieldName, SubmenuStackManagedTypeFullName, step);
                var logicalStackBefore = submenuStackField.GetValue(submenu);
                if (logicalStackBefore is not null && !ReferenceEquals(logicalStackBefore, stack))
                    throw new InvalidDataException($"Step 59.0 retained NSingleplayerSubmenu acquired a foreign logical stack before Gate C: {logicalStackBefore.GetType().FullName}.");

                if (logicalStackBefore is null)
                {
                    if (!_step59NavigationRepairRequired)
                        throw new InvalidDataException("Step 59.0 single-player logical stack became null unexpectedly after Gate B.");
                    Checkpoint(checkpoint, $"M59_C_NAVIGATION_REPAIR_ARMED — retained NSingleplayerSubmenu._stack is null because Step 54 opened the lazy submenu without pushing it; invoking exact game-owned NSubmenuStack.Push token=0x{_step59SubmenuPushToken:X8} once while rendering remains frozen.");
                    try { _step59RuntimeSubmenuPush!.Invoke(stack, new object?[] { submenu }); }
                    catch (TargetInvocationException tie) when (tie.InnerException is not null)
                    {
                        var inner = tie.InnerException;
                        Checkpoint(checkpoint, $"M59_C_NAVIGATION_REPAIR_INNER_EXCEPTION — type={SanitizeCheckpoint(inner.GetType().FullName ?? inner.GetType().Name)}; targetSite={SanitizeCheckpoint(inner.TargetSite?.ToString() ?? "<null>")}; source={SanitizeCheckpoint(inner.Source ?? "<null>")}; stack={SanitizeCheckpoint(inner.StackTrace ?? "<null>")}");
                        ExceptionDispatchInfo.Capture(inner).Throw();
                        throw new InvalidOperationException("Unreachable after preserved Step-59 navigation-repair exception dispatch.");
                    }
                    _step59NavigationRepairInvoked = true;
                    var logicalStackAfterRepair = submenuStackField.GetValue(submenu);
                    if (!ReferenceEquals(logicalStackAfterRepair, stack))
                        throw new InvalidDataException($"Step 59.0 exact NSubmenuStack.Push returned without binding NSingleplayerSubmenu._stack to the exact retained stack; observed={logicalStackAfterRepair?.GetType().FullName ?? "<null>"}.");
                    Checkpoint(checkpoint, "M59_C_NAVIGATION_REPAIR_PASS — exact game-owned NSubmenuStack.Push returned; retained NSingleplayerSubmenu._stack is now the exact retained NMainMenuSubmenuStack; renderingStopped=True.");
                }
                else
                {
                    Checkpoint(checkpoint, "M59_C_NAVIGATION_REPAIR_SKIPPED — retained NSingleplayerSubmenu is already logically bound to the exact main-menu submenu stack.");
                }

                Checkpoint(checkpoint, $"M59_C_HANDLER_ARMED — invoking original OpenCharacterSelect(NButton) exactly once with the real retained _standardButton ({SanitizeCheckpoint(_step59StandardButton?.GetType().FullName ?? "<null>")}); rendering remains frozen.");
                Exception? handlerFailure = null;
                try
                {
                    _step58RuntimeOpenCharacterSelect!.Invoke(submenu, new object?[] { _step59StandardButton });
                }
                catch (Exception invokeEx)
                {
                    handlerFailure = invokeEx is TargetInvocationException tie && tie.InnerException is not null ? tie.InnerException : invokeEx;
                }

                if (handlerFailure is not null)
                {
                    var inner = handlerFailure;
                    Checkpoint(checkpoint, $"M59_C_HANDLER_INNER_EXCEPTION — type={SanitizeCheckpoint(inner.GetType().FullName ?? inner.GetType().Name)}; targetSite={SanitizeCheckpoint(inner.TargetSite?.ToString() ?? "<null>")}; source={SanitizeCheckpoint(inner.Source ?? "<null>")}; stack={SanitizeCheckpoint(inner.StackTrace ?? "<null>")}");
                    try
                    {
                        _step59FailureSnapshot = CaptureStep59ForensicSnapshot(context, step, requireReadyPrerequisites: false);
                        var postFailureState = CaptureCurrentCharacterSelectState(step, context);
                        _step59ConfirmButtonPostFailureDiagnostics = postFailureState.Screen is null
                            ? "post-failure character-select screen is absent"
                            : BuildStep59PostFailureRuntimeDiagnosticDeck(postFailureState.Screen, context, step);
                        _step59StaticMap += $"Handler inner exception: {inner.GetType().FullName}: {inner.Message}\nTargetSite: {inner.TargetSite}\nOriginal stack: {inner.StackTrace}\nPost-failure forensic snapshot: {_step59FailureSnapshot}\n[POST-FAILURE CONFIRM-BUTTON RUNTIME DECK]\n{_step59ConfirmButtonPostFailureDiagnostics}\n";
                        Checkpoint(checkpoint, "M59_C_FORENSIC_POSTFAIL — " + SanitizeCheckpoint(_step59FailureSnapshot));
                        Checkpoint(checkpoint, "M59_C_CONFIRM_BUTTON_POSTFAIL — " + SanitizeCheckpoint(_step59ConfirmButtonPostFailureDiagnostics));
                    }
                    catch (Exception snapshotEx)
                    {
                        _step59FailureSnapshot = $"snapshot-failed={snapshotEx.GetType().FullName}: {snapshotEx.Message}";
                        Checkpoint(checkpoint, "M59_C_FORENSIC_POSTFAIL_SNAPSHOT_FAIL — " + SanitizeCheckpoint(_step59FailureSnapshot));
                    }
                    ExceptionDispatchInfo.Capture(inner).Throw();
                    throw new InvalidOperationException("Unreachable after preserved Step-59 handler exception dispatch.");
                }
            }
            else
            {
                Checkpoint(checkpoint, "M59_C_HANDLER_SKIPPED — character select was already visible/in-tree and logically bound at Step 58; adopting the game-owned state without duplicate stack repair or handler invocation.");
            }

            var current = CaptureCurrentCharacterSelectState(step, context);
            var screen = current.Screen ?? throw new InvalidDataException("Step 59.0 transition completed without a retained character-select cache.");
            if (!current.State.InsideTree || !current.State.Visible || !current.State.VisibleInTree)
                throw new InvalidDataException($"Step 59.0 requires the game-owned character-select screen to be visible/in-tree after transition; observed={current.State}.");
            if (!current.State.StackIsExactRetainedStack)
                throw new InvalidDataException($"Step 59.0 character-select screen is visible/in-tree but NSubmenu._stack is not the exact retained submenu stack; observed={current.State}.");
            _step59CharacterSelectScreen = screen;
            _step59StaticMap += $"NSubmenuStack.Push navigation repair invoked: {_step59NavigationRepairInvoked}\nOpenCharacterSelect invoked: {!_step58TransitionAlreadyComplete}\nAfter transition: {current.State}\n";
            _step59PostTransitionBaseline = CaptureStartupLadderBaseline(baseline.SelectedPath, baseline.SelectedSha256, context);
            RequireStartupLadderBaselineUnchanged(context, baseline, "Step 59 Gate C");
            _step59TransitionPassed = true;
            Checkpoint(checkpoint, $"M59_C_PASS — game-owned character-select transition complete; navigationRepairInvoked={_step59NavigationRepairInvoked}; handlerInvoked={!_step58TransitionAlreadyComplete}; insideTree=True; visible=True; visibleInTree=True; logicalStackExactRetained=True; renderingStopped=True; drift=0.");
            return StartupLadderPass(step, Step59Name, gate, _step58TransitionAlreadyComplete ? "Existing game-owned visible character-select state adopted without duplicate handler invocation." : "The missing game-owned submenu-stack Push was restored if needed, then original OpenCharacterSelect executed once with the real retained standard button and produced the expected visible/in-tree game-owned screen.");
        }
        catch (Exception ex) { Checkpoint(checkpoint, $"M59_C_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}"); return StartupLadderFail(step, Step59Name, gate, stage, ex); }
    }

    public TransformedRealStS2StartupLadderGateResult RunStep59FrozenPostTransitionConfinement(bool renderingStopped, Action<string>? checkpoint = null)
    {
        const int step = 59; const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.PostActionConfinement; var stage = "initialization";
        try
        {
            ThrowIfDisposed(); var context = RequireStep59Prerequisite("Step 59 Gate D entry"); var post = _step59PostTransitionBaseline ?? throw new InvalidOperationException("Step 59.0 post-transition baseline absent.");
            if (!_step59TransitionPassed || !_step59StaticMapDurablyWritten || !renderingStopped) throw new InvalidOperationException("Step 59.0 Gate D requires durable successful transition evidence with rendering frozen.");
            RequireVisibleInTreeCharacterSelectForOwnership(step, context); RequireStartupLadderBaselineUnchanged(context, post, "Step 59 Gate D");
            _exactStep59ClosurePassed = true;
            Checkpoint(checkpoint, "M59_D_PASS — real character-select screen retained visible/in-tree and logically bound to the exact submenu stack; renderingStopped=True; drift=0.");
            return StartupLadderPass(step, Step59Name, gate, "Real handler/state transition closed 4/4. Step 60 may audit the actual active screen instead of reproducing internal lifecycle milestones.");
        }
        catch (Exception ex) { Checkpoint(checkpoint, $"M59_D_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}"); return StartupLadderFail(step, Step59Name, gate, stage, ex); }
    }

    // STEP 60 — audit the active visible screen and its immediate frame/input surface.
    public TransformedRealStS2StartupLadderGateResult RunStep60ClosedStep59Authority(bool renderingStopped, Action<string>? checkpoint = null)
    {
        const int step = 60; const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.PrerequisiteAuthority; var stage = "initialization";
        try
        {
            ThrowIfDisposed(); var context = RequireStep60Prerequisite("Step 60 Gate A entry"); if (!renderingStopped) throw new InvalidOperationException("Step 60.0 requires rendering frozen.");
            var selected = RequireStartupLadderSelectedAuthority(step); _step60Baseline = CaptureStartupLadderBaseline(selected.Path, selected.Sha256, context); RequireVisibleInTreeCharacterSelectForOwnership(step, context); RequireStartupLadderBaselineUnchanged(context, _step60Baseline, "Step 60 Gate A");
            Checkpoint(checkpoint, "M60_A_PASS — Step-59 visible/in-tree real character-select authority retained; renderingStopped=True.");
            return StartupLadderPass(step, Step60Name, gate, "The actual active character-select subtree may now be audited for frame/input work before rendering is restarted.");
        }
        catch (Exception ex) { Checkpoint(checkpoint, $"M60_A_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}"); return StartupLadderFail(step, Step60Name, gate, stage, ex); }
    }

    public TransformedRealStS2StartupLadderGateResult RunStep60ActiveFrameInputAudit(Action<string>? checkpoint = null)
    {
        const int step = 60; const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.StaticAuditOrBinding; var stage = "initialization";
        try
        {
            ThrowIfDisposed(); var context = RequireStep60Prerequisite("Step 60 Gate B entry"); var baseline = _step60Baseline ?? throw new InvalidOperationException("Step 60.0 Gate A must pass before Gate B.");
            var screen = RequireVisibleInTreeCharacterSelectForOwnership(step, context);
            var godotAssembly = (_callbackHandoff ?? throw new InvalidOperationException("GodotSharp handoff absent.")).GodotSharpAssembly;
            var nodeType = godotAssembly.GetType("Godot.Node", true, false)!;
            var nodes = EnumerateStep39NodeGraph(screen, nodeType);
            var managedTypes = GetSelectedManagedNodeTypeNamesForOwnership(nodes, screen.GetType().Assembly, context);
            using var resolver = new RejectingAssemblyResolver(); using var module = OpenStartupLadderModule(baseline.SelectedPath, resolver);
            var allTypes = EnumerateTypes(module.Types).ToDictionary(type => type.FullName, StringComparer.Ordinal); var allMethods = BuildStartupLadderMethodMap(allTypes);
            var roots = CollectStartupLadderActualNodeCallbackRoots(managedTypes, allTypes, Step40ImmediateFrameMethodNames);
            var audit = AuditStartupLadderInvocationFrontier(roots, allTypes, allMethods, _step48LifecycleGuardAuthority ?? throw new InvalidOperationException("Step 60.0 requires retained Step-48 runtime guards."));
            RequireImmediateFrontierAdmissible(audit, "Step 60.0 actual active character-select frame/input frontier under retained runtime guards");
            if (resolver.Requests.Count != 0) throw new InvalidDataException("Step 60.0 frame/input audit attempted external Cecil resolution: " + string.Join(" | ", resolver.Requests));
            _step60StaticMap = "StS2 Launcher — Step 60.0 active character-select surface audit\n" +
                $"Selected compatibility SHA-256: {baseline.SelectedSha256}\n" +
                BuildCharacterSelectNodeAuditAppendixForOwnership("ACTUAL ACTIVE CHARACTER-SELECT FRAME/INPUT SURFACE", nodes, managedTypes, roots, audit) +
                "Rendering restarted while map built: NO\nCharacter choice/confirm/run start authorized: NO\n";
            RequireStartupLadderBaselineUnchanged(context, baseline, "Step 60 Gate B");
            Checkpoint(checkpoint, $"M60_B_PASS — activeNodes={nodes.Count}; managedTypes={managedTypes.Length}; frameInputRoots={roots.Length}; closure={audit.ImmediateClosureMethods.Length}; boundaries={audit.ImmediateBoundaries.Length}; guarded={audit.GuardedMethodFrontiers.Length}; deferred={audit.DeferredMethodFrontiers.Length}; immediate frontier admissible.");
            return StartupLadderPass(step, Step60Name, gate, "Actual active character-select frame/input callback surface is admissible under retained runtime guards.");
        }
        catch (Exception ex) { Checkpoint(checkpoint, $"M60_B_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}"); return StartupLadderFail(step, Step60Name, gate, stage, ex); }
    }

    public TransformedRealStS2StartupLadderGateResult RunStep60SceneConnectionInventory(Action<string>? checkpoint = null)
    {
        const int step = 60; const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.ControlledAction; var stage = "initialization";
        try
        {
            ThrowIfDisposed(); var context = RequireStep60Prerequisite("Step 60 Gate C entry"); var baseline = _step60Baseline ?? throw new InvalidOperationException("Step 60.0 baseline absent.");
            if (string.IsNullOrWhiteSpace(_step60StaticMap)) throw new InvalidOperationException("Step 60.0 frame/input map absent.");
            var pck = RequireEssentialResourcePackHandoff().PackAbsolutePath; var entry = ExtractStartupLadderPckEntryUnpinned(pck, _step57CharacterSelectResourcePath, step);
            var text = new System.Text.UTF8Encoding(false, true).GetString(entry.Bytes).Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');
            var connections = text.Split('\n').Where(line => line.StartsWith("[connection ", StringComparison.Ordinal)).ToArray();
            _step60StaticMap += "\n[TSCN SIGNAL CONNECTION INVENTORY — EVIDENCE ONLY]\n" + $"Connection records: {connections.Length}\n" + string.Join("\n", connections.Select(line => "  " + line)) + "\n";
            _step60SurfaceMapped = true; RequireVisibleInTreeCharacterSelectForOwnership(step, context); RequireStartupLadderBaselineUnchanged(context, baseline, "Step 60 Gate C");
            Checkpoint(checkpoint, $"M60_C_PASS — exact character-select TSCN connection inventory appended; connectionRecords={connections.Length}; no Godot load or handler invocation.");
            return StartupLadderPass(step, Step60Name, gate, "Exact active subtree audit plus exact TSCN signal-connection inventory are now durable evidence for rendering; interaction remains unopened.");
        }
        catch (Exception ex) { Checkpoint(checkpoint, $"M60_C_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}"); return StartupLadderFail(step, Step60Name, gate, stage, ex); }
    }

    public TransformedRealStS2StartupLadderGateResult RunStep60FrozenSurfaceConfinement(bool renderingStopped, Action<string>? checkpoint = null)
    {
        const int step = 60; const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.PostActionConfinement; var stage = "initialization";
        try
        {
            ThrowIfDisposed(); var context = RequireStep60Prerequisite("Step 60 Gate D entry"); var baseline = _step60Baseline ?? throw new InvalidOperationException("Step 60.0 baseline absent.");
            if (!_step60SurfaceMapped || !_step60StaticMapDurablyWritten || !renderingStopped) throw new InvalidOperationException("Step 60.0 Gate D requires durable active-surface evidence with rendering frozen.");
            RequireVisibleInTreeCharacterSelectForOwnership(step, context); RequireStartupLadderBaselineUnchanged(context, baseline, "Step 60 Gate D"); _exactStep60ClosurePassed = true;
            Checkpoint(checkpoint, "M60_D_PASS — active character-select surface evidence durable; screen visible/in-tree; renderingStopped=True; drift=0.");
            return StartupLadderPass(step, Step60Name, gate, "Active screen audit closed 4/4. Step 61 may perform a short visible render/refreeze without authorizing interaction.");
        }
        catch (Exception ex) { Checkpoint(checkpoint, $"M60_D_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}"); return StartupLadderFail(step, Step60Name, gate, stage, ex); }
    }

    // STEP 61 — short visible render residency, no intentional interaction.
    public TransformedRealStS2StartupLadderGateResult RunStep61ClosedStep60Authority(bool renderingStopped, Action<string>? checkpoint = null)
    {
        const int step = 61; const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.PrerequisiteAuthority; var stage = "initialization";
        try
        {
            ThrowIfDisposed(); var context = RequireStep61Prerequisite("Step 61 Gate A entry"); if (!renderingStopped) throw new InvalidOperationException("Step 61.0 requires rendering frozen at entry.");
            var selected = RequireStartupLadderSelectedAuthority(step); _step61Baseline = CaptureStartupLadderBaseline(selected.Path, selected.Sha256, context); RequireVisibleInTreeCharacterSelectForOwnership(step, context); RequireStartupLadderBaselineUnchanged(context, _step61Baseline, "Step 61 Gate A");
            Checkpoint(checkpoint, "M61_A_PASS — Step-60 audited active character-select authority retained; renderer stopped; short render not armed.");
            return StartupLadderPass(step, Step61Name, gate, "Audited active character-select state retained for one short visible render/refreeze residency.");
        }
        catch (Exception ex) { Checkpoint(checkpoint, $"M61_A_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}"); return StartupLadderFail(step, Step61Name, gate, stage, ex); }
    }

    public TransformedRealStS2StartupLadderGateResult RunStep61RenderPreflight(Action<string>? checkpoint = null)
    {
        const int step = 61; const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.StaticAuditOrBinding; var stage = "initialization";
        try
        {
            ThrowIfDisposed(); var context = RequireStep61Prerequisite("Step 61 Gate B entry"); var baseline = _step61Baseline ?? throw new InvalidOperationException("Step 61.0 baseline absent."); var state = CaptureCurrentCharacterSelectState(step, context);
            if (state.Screen is null || !state.State.InsideTree || !state.State.Visible || !state.State.VisibleInTree) throw new InvalidDataException($"Step 61.0 requires visible/in-tree screen before render; observed={state.State}.");
            _step61StaticMap = "StS2 Launcher — Step 61.0 short character-select render preflight\n" + $"Before render: {state.State}\n" + $"Target residency ms: {Step61CharacterSelectRenderTargetMilliseconds}\n" + "Intentional user interaction authorized: NO\n";
            RequireStartupLadderBaselineUnchanged(context, baseline, "Step 61 Gate B"); Checkpoint(checkpoint, "M61_B_PASS — short render preflight complete; visible/in-tree screen retained; interaction remains unauthorized.");
            return StartupLadderPass(step, Step61Name, gate, "Short-render preflight recorded without restarting rendering.");
        }
        catch (Exception ex) { Checkpoint(checkpoint, $"M61_B_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}"); return StartupLadderFail(step, Step61Name, gate, stage, ex); }
    }

    public void BeginStep61BoundedRenderPulse(Action<string>? checkpoint = null)
    {
        ThrowIfDisposed(); RequireStep61Prerequisite("Step 61 Gate C pulse start"); if (!_step61StaticMapDurablyWritten) throw new InvalidOperationException("Step 61.0 requires durable preflight before rendering."); if (_step61PulseStarted) throw new InvalidOperationException("Step 61.0 render residency is one-shot in-process.");
        _step61PulseStarted = true; Checkpoint(checkpoint, $"M61_C_PULSE_ARMED — short character-select render residency authorized; target={Step61CharacterSelectRenderTargetMilliseconds}ms; ceiling={Step61CharacterSelectRenderEvidenceCeilingMilliseconds}ms; interaction remains unauthorized.");
    }

    public TransformedRealStS2StartupLadderGateResult RunStep61RenderPulseEvidence(bool startReturned, bool activeAfterStart, bool stopReturned, bool activeAfterStop, double elapsedMilliseconds, Action<string>? checkpoint = null)
        => AcceptCharacterSelectRenderEvidence(61, Step61Name, _step61Baseline, ref _step61PostPulseBaseline, _step61PulseStarted, ref _step61PulsePassed, Step61CharacterSelectRenderTargetMilliseconds, Step61CharacterSelectRenderEvidenceCeilingMilliseconds, startReturned, activeAfterStart, stopReturned, activeAfterStop, elapsedMilliseconds, checkpoint);

    public TransformedRealStS2StartupLadderGateResult RunStep61FrozenPostResidencyConfinement(bool renderingStopped, Action<string>? checkpoint = null)
    {
        const int step = 61; const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.PostActionConfinement; var stage = "initialization";
        try
        {
            ThrowIfDisposed(); var context = RequireStep61Prerequisite("Step 61 Gate D entry"); var post = _step61PostPulseBaseline ?? throw new InvalidOperationException("Step 61.0 post-pulse baseline absent."); if (!_step61PulsePassed || !renderingStopped) throw new InvalidOperationException("Step 61.0 Gate D requires successful short residency and rendering stopped.");
            RequireVisibleInTreeCharacterSelectForOwnership(step, context); RequireStartupLadderBaselineUnchanged(context, post, "Step 61 Gate D"); _exactStep61ClosurePassed = true; Checkpoint(checkpoint, "M61_D_PASS — short visible render residency closed; screen retained visible/in-tree; renderingStopped=True; drift=0.");
            return StartupLadderPass(step, Step61Name, gate, "Short visible render/refreeze closed 4/4. Step 62 may run a sustained observation residency so the real UI is plainly visible.");
        }
        catch (Exception ex) { Checkpoint(checkpoint, $"M61_D_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}"); return StartupLadderFail(step, Step61Name, gate, stage, ex); }
    }

    // STEP 62 — sustained visible render residency, still observational/no intentional input.
    public TransformedRealStS2StartupLadderGateResult RunStep62ClosedStep61Authority(bool renderingStopped, Action<string>? checkpoint = null)
    {
        const int step = 62; const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.PrerequisiteAuthority; var stage = "initialization";
        try
        {
            ThrowIfDisposed(); var context = RequireStep62Prerequisite("Step 62 Gate A entry"); if (!renderingStopped) throw new InvalidOperationException("Step 62.0 requires rendering frozen at entry.");
            var selected = RequireStartupLadderSelectedAuthority(step); _step62Baseline = CaptureStartupLadderBaseline(selected.Path, selected.Sha256, context); RequireVisibleInTreeCharacterSelectForOwnership(step, context); RequireStartupLadderBaselineUnchanged(context, _step62Baseline, "Step 62 Gate A");
            Checkpoint(checkpoint, "M62_A_PASS — Step-61 short render/refreeze authority retained; sustained residency not armed."); return StartupLadderPass(step, Step62Name, gate, "Stable short-render authority retained for a longer visible observation residency.");
        }
        catch (Exception ex) { Checkpoint(checkpoint, $"M62_A_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}"); return StartupLadderFail(step, Step62Name, gate, stage, ex); }
    }

    public TransformedRealStS2StartupLadderGateResult RunStep62SustainedRenderPreflight(Action<string>? checkpoint = null)
    {
        const int step = 62; const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.StaticAuditOrBinding; var stage = "initialization";
        try
        {
            ThrowIfDisposed(); var context = RequireStep62Prerequisite("Step 62 Gate B entry"); var baseline = _step62Baseline ?? throw new InvalidOperationException("Step 62.0 baseline absent."); var screen = RequireVisibleInTreeCharacterSelectForOwnership(step, context);
            var godotAssembly = (_callbackHandoff ?? throw new InvalidOperationException("GodotSharp handoff absent.")).GodotSharpAssembly; var nodeType = godotAssembly.GetType("Godot.Node", true, false)!; var nodes = EnumerateStep39NodeGraph(screen, nodeType);
            _step62StaticMap = "StS2 Launcher — Step 62.0 sustained character-select render preflight\n" + $"Visible subtree nodes before sustained render: {nodes.Count}\n" + $"Target residency ms: {Step62CharacterSelectSustainedRenderTargetMilliseconds}\n" + "Intentional user interaction authorized: NO\n" + "If this closes physically, the next candidate may move to continuous/interactive Godot ownership rather than more screen-by-screen reconstruction.\n";
            RequireStartupLadderBaselineUnchanged(context, baseline, "Step 62 Gate B"); Checkpoint(checkpoint, $"M62_B_PASS — sustained render preflight complete; nodes={nodes.Count}; interaction remains unauthorized."); return StartupLadderPass(step, Step62Name, gate, "Sustained visible render preflight recorded without restarting rendering.");
        }
        catch (Exception ex) { Checkpoint(checkpoint, $"M62_B_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}"); return StartupLadderFail(step, Step62Name, gate, stage, ex); }
    }

    public void BeginStep62SustainedRenderPulse(Action<string>? checkpoint = null)
    {
        ThrowIfDisposed(); RequireStep62Prerequisite("Step 62 Gate C pulse start"); if (!_step62StaticMapDurablyWritten) throw new InvalidOperationException("Step 62.0 requires durable preflight before rendering."); if (_step62PulseStarted) throw new InvalidOperationException("Step 62.0 sustained render residency is one-shot in-process.");
        _step62PulseStarted = true; Checkpoint(checkpoint, $"M62_C_PULSE_ARMED — sustained character-select render residency authorized; target={Step62CharacterSelectSustainedRenderTargetMilliseconds}ms; ceiling={Step62CharacterSelectSustainedRenderEvidenceCeilingMilliseconds}ms; observation only, no intentional interaction.");
    }

    public TransformedRealStS2StartupLadderGateResult RunStep62SustainedRenderEvidence(bool startReturned, bool activeAfterStart, bool stopReturned, bool activeAfterStop, double elapsedMilliseconds, Action<string>? checkpoint = null)
        => AcceptCharacterSelectRenderEvidence(62, Step62Name, _step62Baseline, ref _step62PostPulseBaseline, _step62PulseStarted, ref _step62PulsePassed, Step62CharacterSelectSustainedRenderTargetMilliseconds, Step62CharacterSelectSustainedRenderEvidenceCeilingMilliseconds, startReturned, activeAfterStart, stopReturned, activeAfterStop, elapsedMilliseconds, checkpoint);

    public TransformedRealStS2StartupLadderGateResult RunStep62FrozenPostResidencyConfinement(bool renderingStopped, Action<string>? checkpoint = null)
    {
        const int step = 62; const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.PostActionConfinement; var stage = "initialization";
        try
        {
            ThrowIfDisposed(); var context = RequireStep62Prerequisite("Step 62 Gate D entry"); var post = _step62PostPulseBaseline ?? throw new InvalidOperationException("Step 62.0 post-pulse baseline absent."); if (!_step62PulsePassed || !renderingStopped) throw new InvalidOperationException("Step 62.0 Gate D requires successful sustained residency and rendering stopped.");
            RequireVisibleInTreeCharacterSelectForOwnership(step, context); RequireStartupLadderBaselineUnchanged(context, post, "Step 62 Gate D"); _exactStep62ClosurePassed = true; Checkpoint(checkpoint, "M62_D_PASS — sustained visible character-select residency closed and synchronously refroze; renderingStopped=True; drift=0.");
            return StartupLadderPass(step, Step62Name, gate, "Sustained real character-select rendering closed 4/4. This candidate stops here; Step 63/continuous interactive ownership remains unopened.");
        }
        catch (Exception ex) { Checkpoint(checkpoint, $"M62_D_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}"); return StartupLadderFail(step, Step62Name, gate, stage, ex); }
    }

    private TransformedRealStS2StartupLadderGateResult AcceptCharacterSelectRenderEvidence(int step, string name, StartupLadderBaseline? baselineValue, ref StartupLadderBaseline? postBaseline, bool armed, ref bool passed, int targetMs, int ceilingMs, bool startReturned, bool activeAfterStart, bool stopReturned, bool activeAfterStop, double elapsedMilliseconds, Action<string>? checkpoint)
    {
        const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.ControlledAction; var stage = "initialization";
        try
        {
            ThrowIfDisposed(); var context = step == 61 ? RequireStep61Prerequisite($"Step {step} Gate C evidence") : RequireStep62Prerequisite($"Step {step} Gate C evidence"); var baseline = baselineValue ?? throw new InvalidOperationException($"Step {step}.0 baseline absent.");
            if (!armed) throw new InvalidOperationException($"Step {step}.0 render evidence cannot be accepted before the one-shot residency is armed.");
            if (!startReturned || !activeAfterStart || !stopReturned || activeAfterStop) throw new InvalidOperationException($"Step {step}.0 render state mismatch: startReturned={startReturned}; activeAfterStart={activeAfterStart}; stopReturned={stopReturned}; activeAfterStop={activeAfterStop}.");
            if (elapsedMilliseconds < targetMs || elapsedMilliseconds > ceilingMs) throw new InvalidOperationException($"Step {step}.0 post-stop evidence outside accepted window. target={targetMs}; ceiling={ceilingMs}; observed={elapsedMilliseconds:F1}.");
            RequireVisibleInTreeCharacterSelectForOwnership(step, context); RequireStartupLadderBaselineUnchanged(context, baseline, $"Step {step} Gate C post-stop"); postBaseline = CaptureStartupLadderBaseline(baseline.SelectedPath, baseline.SelectedSha256, context); passed = true;
            Checkpoint(checkpoint, $"M{step}_C_PASS — character-select render residency completed and synchronously refroze; elapsedMs={elapsedMilliseconds:F1}; visible/inTree=True; drift=0.");
            return StartupLadderPass(step, name, gate, $"Real character-select UI rendered for {elapsedMilliseconds:F1} ms and synchronously refroze.");
        }
        catch (Exception ex) { Checkpoint(checkpoint, $"M{step}_C_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}"); return StartupLadderFail(step, name, gate, stage, ex); }
    }

    private Step35ExecutionLoadContext RequireStep58Prerequisite(string boundary)
    {
        var context = RequireStep52Prerequisite(boundary);
        if (!_exactStep52ClosurePassed || !_step52PulsePassed || !_step52StaticMapDurablyWritten)
            throw new InvalidOperationException(boundary + " requires same-process Step-52 4/4 sustained main-menu render/refreeze authority.");
        if (!_step58CurrentOwnershipAcquisitionPassed)
            throw new InvalidOperationException(boundary + " requires the current-architecture Step-58 ownership acquisition. Legacy Steps 53-57 are not prerequisites.");
        return context;
    }
    private Step35ExecutionLoadContext RequireStep59Prerequisite(string boundary)
    {
        var context = RequireStep58Prerequisite(boundary); if (!_exactStep58ClosurePassed || !_step58OwnershipMapped || !_step58StaticMapDurablyWritten) throw new InvalidOperationException(boundary + " requires same-process Step-58 4/4 durable runtime-ownership authority."); return context;
    }
    private Step35ExecutionLoadContext RequireStep60Prerequisite(string boundary)
    {
        var context = RequireStep59Prerequisite(boundary); if (!_exactStep59ClosurePassed || !_step59TransitionPassed || !_step59StaticMapDurablyWritten) throw new InvalidOperationException(boundary + " requires same-process Step-59 4/4 visible/in-tree game-owned character-select authority."); return context;
    }
    private Step35ExecutionLoadContext RequireStep61Prerequisite(string boundary)
    {
        var context = RequireStep60Prerequisite(boundary); if (!_exactStep60ClosurePassed || !_step60SurfaceMapped || !_step60StaticMapDurablyWritten) throw new InvalidOperationException(boundary + " requires same-process Step-60 4/4 durable active character-select surface audit."); return context;
    }
    private Step35ExecutionLoadContext RequireStep62Prerequisite(string boundary)
    {
        var context = RequireStep61Prerequisite(boundary); if (!_exactStep61ClosurePassed || !_step61PulsePassed) throw new InvalidOperationException(boundary + " requires same-process Step-61 4/4 short visible render/refreeze authority."); return context;
    }

    private object RequireRetainedCharacterSelectPackedSceneForOwnership(int step)
    {
        var stack = _step54SubmenuStack ?? throw new InvalidOperationException($"Step {step}.0 retained submenu stack is absent."); var sceneField = RequireRuntimeExactInstanceField(stack.GetType(), MainMenuCharacterSelectSceneFieldName, PackedSceneManagedTypeFullName, step); var packedScene = sceneField.GetValue(stack) ?? throw new InvalidDataException($"Step {step}.0 retained character-select PackedScene is null.");
        var resourcePath = RequireRuntimeStringProperty(packedScene, GodotResourcePathPropertyName, step).Replace('\\', '/'); if (!string.Equals(resourcePath, CharacterSelectSceneResourcePath, StringComparison.Ordinal)) throw new InvalidDataException($"Step {step}.0 retained character-select PackedScene path drifted: {resourcePath}."); return packedScene;
    }

    private (object? Screen, CharacterSelectRuntimeState State) CaptureCurrentCharacterSelectState(int step, Step35ExecutionLoadContext context)
    {
        var stack = _step54SubmenuStack ?? throw new InvalidOperationException($"Step {step}.0 retained submenu stack is absent."); var cache = RequireRuntimeExactInstanceField(stack.GetType(), MainMenuCharacterSelectSubmenuFieldName, CharacterSelectScreenManagedTypeFullName, step); var screen = cache.GetValue(stack); if (screen is null) return (null, CharacterSelectRuntimeState.Absent); RequireCharacterSelectIdentity(screen, stack, context, step); return (screen, CaptureCharacterSelectRuntimeState(screen, stack, step));
    }

    private void RequireStep58ObservedStateUnchanged(Step35ExecutionLoadContext context, int step)
    {
        var current = CaptureCurrentCharacterSelectState(step, context); if (!ReferenceEquals(current.Screen, _step58ObservedCharacterSelect)) throw new InvalidDataException($"Step {step}.0 character-select cache identity changed during non-mutating Step 58."); if (_step58ObservedState is null || !current.State.Equals(_step58ObservedState.Value)) throw new InvalidDataException($"Step {step}.0 character-select runtime state changed during non-mutating Step 58. before={_step58ObservedState}; after={current.State}.");
    }

    private object RequireVisibleInTreeCharacterSelectForOwnership(int step, Step35ExecutionLoadContext context)
    {
        var current = CaptureCurrentCharacterSelectState(step, context); var screen = current.Screen ?? throw new InvalidOperationException($"Step {step}.0 retained character-select screen is absent."); if (!current.State.InsideTree || !current.State.Visible || !current.State.VisibleInTree || !current.State.StackIsExactRetainedStack) throw new InvalidDataException($"Step {step}.0 requires visible/in-tree character-select authority logically bound to the exact retained submenu stack; observed={current.State}."); _step59CharacterSelectScreen = screen; return screen;
    }

    private static void RequireCharacterSelectIdentity(object screen, object stack, AssemblyLoadContext context, int step)
    {
        if (screen.GetType().FullName != CharacterSelectScreenManagedTypeFullName) throw new InvalidDataException($"Step {step}.0 character-select cache type drifted: {screen.GetType().FullName}.");
        if (!ReferenceEquals(AssemblyLoadContext.GetLoadContext(screen.GetType().Assembly), context)) throw new InvalidDataException($"Step {step}.0 character-select cache is not owned by the exact private load context.");
        var stackField = RequireRuntimeExactInstanceField(screen.GetType(), SubmenuStackFieldName, SubmenuStackManagedTypeFullName, step);
        var logicalStack = stackField.GetValue(screen);
        // An instantiated/cached submenu may already be attached to the SceneTree before NSubmenuStack.Push
        // has logically bound it with NSubmenu.SetStack. Null is therefore a legitimate pre-push state.
        // A non-null stack must still be the exact retained main-menu stack; foreign ownership is rejected.
        if (logicalStack is not null && !ReferenceEquals(logicalStack, stack))
            throw new InvalidDataException($"Step {step}.0 character-select NSubmenu._stack points to a foreign submenu stack: {logicalStack.GetType().FullName}.");
    }

    private static CharacterSelectRuntimeState CaptureCharacterSelectRuntimeState(object screen, object stack, int step)
    {
        var inside = Convert.ToBoolean(RequireZeroArgBoolMethod(screen.GetType(), "IsInsideTree").Invoke(screen, null), System.Globalization.CultureInfo.InvariantCulture);
        var visible = RequireRuntimeBoolProperty(screen, "Visible", step);
        var visibleInTree = Convert.ToBoolean(RequireZeroArgBoolMethod(screen.GetType(), "IsVisibleInTree").Invoke(screen, null), System.Globalization.CultureInfo.InvariantCulture);
        var stackField = RequireRuntimeExactInstanceField(screen.GetType(), SubmenuStackFieldName, SubmenuStackManagedTypeFullName, step);
        var logicalStack = stackField.GetValue(screen);
        var stackIsNull = logicalStack is null;
        var stackIsExact = ReferenceEquals(logicalStack, stack);
        var logicalStackType = logicalStack?.GetType().FullName ?? "<null>";
        var parentIsStack = false; string parentType = "<none>";
        if (inside)
        {
            var getParent = RequireZeroArgRuntimeMethodForOwnership(screen.GetType(), "GetParent", step);
            var parent = InvokeStartupLadderMethod(getParent, screen, null, $"Step {step} character-select GetParent");
            parentIsStack = ReferenceEquals(parent, stack);
            parentType = parent?.GetType().FullName ?? "<null>";
        }
        return new CharacterSelectRuntimeState(true, inside, visible, visibleInTree, stackIsNull, stackIsExact, logicalStackType, parentIsStack, parentType);
    }

    private string CaptureStep59ForensicSnapshot(Step35ExecutionLoadContext context, int step, bool requireReadyPrerequisites)
    {
        if (requireReadyPrerequisites)
            _step59ReadyBindingBlocker = string.Empty;
        var prerequisiteFailures = new List<string>();
        var current = CaptureCurrentCharacterSelectState(step, context);
        var screen = current.Screen ?? throw new InvalidDataException($"Step {step}.0 forensic snapshot requires the cached NCharacterSelectScreen.");
        var screenReady = InvokeRuntimeBoolForForensics(screen, "IsNodeReady", step);
        if (requireReadyPrerequisites && !screenReady)
            prerequisiteFailures.Add("character-select IsNodeReady()=false");

        var requiredScreenFields = new[]
        {
            "_charButtonContainer", "_ascensionPanel", "_actDropdown", "_actDropdownLabel",
            "_remotePlayerContainer", "_readyAndWaitingContainer", "_backButton", "_unreadyButton",
            "_embarkButton", "_randomCharacterButton"
        };
        var fieldStates = new List<string>();
        object? ascensionPanel = null;
        foreach (var fieldName in requiredScreenFields)
        {
            var field = RequireRuntimeInstanceFieldForOwnership(screen.GetType(), fieldName, step);
            var value = field.GetValue(screen);
            fieldStates.Add(fieldName + "=" + (value is null ? "NULL" : value.GetType().FullName));
            if (fieldName == "_ascensionPanel") ascensionPanel = value;
            if (requireReadyPrerequisites && value is null)
                prerequisiteFailures.Add($"character-select {fieldName}=NULL");
        }

        var ascensionReady = ascensionPanel is not null && InvokeRuntimeBoolForForensics(ascensionPanel, "IsNodeReady", step);
        if (requireReadyPrerequisites && ascensionPanel is not null && !ascensionReady)
            prerequisiteFailures.Add("character-select _ascensionPanel IsNodeReady()=false");

        var ascensionFieldStates = new List<string>();
        if (ascensionPanel is not null)
        {
            var requiredAscensionFields = new[]
            {
                "_leftArrow", "_rightArrow", "_ascensionLevel", "_info",
                "_leftTriggerIcon", "_rightTriggerIcon", "_iconHsv"
            };
            foreach (var fieldName in requiredAscensionFields)
            {
                var field = RequireRuntimeInstanceFieldForOwnership(ascensionPanel.GetType(), fieldName, step);
                var value = field.GetValue(ascensionPanel);
                ascensionFieldStates.Add(fieldName + "=" + (value is null ? "NULL" : value.GetType().FullName));
                if (requireReadyPrerequisites && value is null)
                    prerequisiteFailures.Add($"ascension-panel {fieldName}=NULL");
            }
        }

        var nGame = _step39NGameInstance ?? throw new InvalidOperationException($"Step {step}.0 retained NGame instance is absent.");
        var nGameProperties = new[] { "HotkeyManager", "InputManager", "RemoteCursorContainer", "ReactionContainer", "TimeoutOverlay", "RootSceneContainer" };
        var nGameStates = new List<string>();
        object? rootSceneContainer = null;
        foreach (var propertyName in nGameProperties)
        {
            var property = nGame.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                ?? throw new MissingMemberException(nGame.GetType().FullName, propertyName);
            var value = property.GetValue(nGame);
            nGameStates.Add(propertyName + "=" + (value is null ? "NULL" : value.GetType().FullName));
            if (propertyName == "RootSceneContainer") rootSceneContainer = value;
            if (requireReadyPrerequisites && value is null)
                prerequisiteFailures.Add($"NGame.{propertyName}=NULL");
        }

        var saveType = screen.GetType().Assembly.GetType(SaveManagerTypeFullName, throwOnError: true, ignoreCase: false)
            ?? throw new MissingMemberException(SaveManagerTypeFullName);
        var saveManager = RequireExistingStartupLadderSaveManagerInstance(saveType, step);
        var progress = RequireStartupLadderPropertyValue(saveManager, "Progress", step);
        var epochs = RequireStartupLadderPropertyValue(progress, "Epochs", step);
        var encounterStats = RequireStartupLadderPropertyValue(progress, "EncounterStats", step);

        object? currentScene = null;
        if (rootSceneContainer is not null)
        {
            var currentSceneProperty = rootSceneContainer.GetType().GetProperty("CurrentScene", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            currentScene = currentSceneProperty?.GetValue(rootSceneContainer);
        }
        var retainedMainMenu = _step48MainMenuInstance;
        var currentSceneIdentity = currentScene is null ? "NULL" : ReferenceEquals(currentScene, retainedMainMenu) ? "exact-retained-main-menu" : currentScene.GetType().FullName ?? "<unknown>";

        var lobbyField = RequireRuntimeInstanceFieldForOwnership(screen.GetType(), "_lobby", step);
        var lobby = lobbyField.GetValue(screen);
        var playersCount = "n/a";
        if (lobby is not null)
        {
            try
            {
                var players = lobby.GetType().GetProperty("Players", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(lobby);
                var count = players?.GetType().GetProperty("Count", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(players);
                playersCount = count?.ToString() ?? "unknown";
            }
            catch { playersCount = "unreadable"; }
        }

        if (requireReadyPrerequisites)
            _step59ReadyBindingBlocker = string.Join(" | ", prerequisiteFailures);

        return $"character=[{current.State}; nodeReady={screenReady}; fields={string.Join(",", fieldStates)}]; ascension=[nodeReady={ascensionReady}; fields={string.Join(",", ascensionFieldStates)}]; nGame=[{string.Join(",", nGameStates)}]; save=Progress:{progress.GetType().FullName},Epochs:{epochs.GetType().FullName},EncounterStats:{encounterStats.GetType().FullName}; rootCurrentScene={currentSceneIdentity}; lobby={(lobby is null ? "NULL" : lobby.GetType().FullName)}; lobbyPlayers={playersCount}; requiredFailures={(prerequisiteFailures.Count == 0 ? "<none>" : string.Join(" | ", prerequisiteFailures))}";
    }


    private object RequireStep59EmbarkButtonForProbe(Step35ExecutionLoadContext context, int step)
    {
        var current = CaptureCurrentCharacterSelectState(step, context);
        var screen = current.Screen ?? throw new InvalidOperationException($"Step {step}.0 probe requires the retained preloaded NCharacterSelectScreen.");
        var embarkField = RequireRuntimeInstanceFieldForOwnership(screen.GetType(), "_embarkButton", step);
        var embark = embarkField.GetValue(screen) ?? throw new InvalidDataException($"Step {step}.0 retained NCharacterSelectScreen._embarkButton is null.");
        if (embark.GetType().FullName != ConfirmButtonManagedTypeFullNameForForensics)
            throw new InvalidDataException($"Step {step}.0 retained embark runtime type drifted: {embark.GetType().FullName}.");
        if (!ReferenceEquals(AssemblyLoadContext.GetLoadContext(embark.GetType().Assembly), context))
            throw new InvalidDataException($"Step {step}.0 retained embark button is outside the selected private load context.");
        return embark;
    }

    private static MethodInfo RequireStep59StaticRuntimeMethod(Type type, string name, int parameterCount, int step)
    {
        var candidates = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(method => method.Name == name && !method.IsGenericMethod && method.GetParameters().Length == parameterCount)
            .ToArray();
        return candidates.SingleOrDefault() ?? throw new MissingMethodException(type.FullName, $"Step {step}.0 static {name}/{parameterCount}");
    }

    private static MethodInfo RequireStep59InstanceRuntimeMethod(Type type, string name, int parameterCount, int step)
    {
        var candidates = EnumerateRuntimeMethodsForOwnership(type)
            .Where(method => method.Name == name && !method.IsStatic && !method.IsGenericMethod && method.GetParameters().Length == parameterCount)
            .ToArray();
        return candidates.SingleOrDefault() ?? throw new MissingMethodException(type.FullName, $"Step {step}.0 instance {name}/{parameterCount}");
    }

    private static string DescribeStep59ProbeException(Exception exception)
    {
        var actual = exception is TargetInvocationException tie && tie.InnerException is not null ? tie.InnerException : exception;
        return $"{actual.GetType().FullName}: {actual.Message}; targetSite={actual.TargetSite}; source={actual.Source}; stack={actual.StackTrace}";
    }

    private string BuildStep59ControllerSingletonRehearsalDiagnostics(object embark, Step35ExecutionLoadContext context, int step)
    {
        var text = new System.Text.StringBuilder();
        text.AppendLine("[STEP 59R CONTROLLER / HOTKEY SINGLETON REHEARSAL]");
        text.AppendLine("Observational intent: invoke only getters/icon lookups and compare singleton identities. No Enable/OnEnable/RegisterHotkeys/PushHotkey binding, field write, stack Push, OpenCharacterSelect, or render restart.");

        var admission = _admission ?? throw new InvalidOperationException("Step 59R requires selected sts2 admission.");
        object? InvokeAndRecord(Type type, object? instance, string methodName, int parameterCount, params object?[] args)
        {
            try
            {
                var method = instance is null
                    ? RequireStep59StaticRuntimeMethod(type, methodName, parameterCount, step)
                    : RequireStep59InstanceRuntimeMethod(type, methodName, parameterCount, step);
                var value = method.Invoke(instance, args);
                text.AppendLine($"  PASS {type.FullName}.{methodName}/{parameterCount} => {DescribeStep59RuntimeValue(value)}; token=0x{method.MetadataToken:X8}");
                return value;
            }
            catch (Exception ex)
            {
                text.AppendLine($"  FAIL {type.FullName}.{methodName}/{parameterCount} => {DescribeStep59ProbeException(ex)}");
                return null;
            }
        }

        var hasControllerHotkey = InvokeAndRecord(embark.GetType(), embark, "get_HasControllerHotkey", 0);
        var controllerIconHotkey = InvokeAndRecord(embark.GetType(), embark, "get_ControllerIconHotkey", 0);
        var hotkeys = InvokeAndRecord(embark.GetType(), embark, "get_Hotkeys", 0);
        text.AppendLine($"  embarkSummary: hasControllerHotkey={DescribeStep59RuntimeValue(hasControllerHotkey)}; controllerIconHotkey={DescribeStep59RuntimeValue(controllerIconHotkey)}; hotkeys={DescribeStep59RuntimeValue(hotkeys)}");

        var nGame = _step39NGameInstance ?? throw new InvalidOperationException("Step 59R retained NGame instance is absent.");
        var nGameHotkey = InvokeAndRecord(nGame.GetType(), nGame, "get_HotkeyManager", 0);
        var nGameInput = InvokeAndRecord(nGame.GetType(), nGame, "get_InputManager", 0);

        var hotkeyType = admission.Assembly.GetType("MegaCrit.Sts2.Core.Nodes.CommonUi.NHotkeyManager", throwOnError: true, ignoreCase: false)
            ?? throw new MissingMemberException("NHotkeyManager");
        var inputType = admission.Assembly.GetType("MegaCrit.Sts2.Core.Nodes.CommonUi.NInputManager", throwOnError: true, ignoreCase: false)
            ?? throw new MissingMemberException("NInputManager");
        var controllerType = admission.Assembly.GetType("MegaCrit.Sts2.Core.Nodes.CommonUi.NControllerManager", throwOnError: true, ignoreCase: false)
            ?? throw new MissingMemberException("NControllerManager");

        var hotkeySingleton = InvokeAndRecord(hotkeyType, null, "get_Instance", 0);
        var inputSingleton = InvokeAndRecord(inputType, null, "get_Instance", 0);
        var controllerFromInput = inputSingleton is null ? null : InvokeAndRecord(inputType, inputSingleton, "get_ControllerManager", 0);
        var controllerSingleton = InvokeAndRecord(controllerType, null, "get_Instance", 0);
        var controllerUsing = controllerSingleton is null ? null : InvokeAndRecord(controllerType, controllerSingleton, "get_IsUsingController", 0);

        text.AppendLine($"  identity: NGame.HotkeyManager===NHotkeyManager.Instance => {ReferenceEquals(nGameHotkey, hotkeySingleton)}");
        text.AppendLine($"  identity: NGame.InputManager===NInputManager.Instance => {ReferenceEquals(nGameInput, inputSingleton)}");
        text.AppendLine($"  identity: NInputManager.ControllerManager===NControllerManager.Instance => {ReferenceEquals(controllerFromInput, controllerSingleton)}");
        text.AppendLine($"  controllerUsing={DescribeStep59RuntimeValue(controllerUsing)}");

        var iconKey = controllerIconHotkey as string;
        if (!string.IsNullOrWhiteSpace(iconKey))
        {
            if (inputSingleton is not null)
                InvokeAndRecord(inputType, inputSingleton, "GetHotkeyIcon", 1, iconKey);
            if (controllerSingleton is not null)
                InvokeAndRecord(controllerType, controllerSingleton, "GetHotkeyIcon", 1, iconKey);
        }
        else
        {
            text.AppendLine("  icon lookups skipped because ControllerIconHotkey was null/empty.");
        }

        foreach (var (name, value) in new[]
                 {
                     ("NGame.HotkeyManager", nGameHotkey),
                     ("NHotkeyManager.Instance", hotkeySingleton),
                     ("NGame.InputManager", nGameInput),
                     ("NInputManager.Instance", inputSingleton),
                     ("NInputManager.ControllerManager", controllerFromInput),
                     ("NControllerManager.Instance", controllerSingleton),
                 })
        {
            if (value is null)
            {
                text.AppendLine($"[{name} FIELD MATRIX] NULL");
                continue;
            }
            text.AppendLine($"[{name} FIELD MATRIX]");
            text.AppendLine(BuildStep59FullInheritedFieldMatrix(value, name + " FIELDS"));
            text.AppendLine($"  loadContext={AssemblyLoadContext.GetLoadContext(value.GetType().Assembly)?.Name ?? "<null>"}; selectedContext={ReferenceEquals(AssemblyLoadContext.GetLoadContext(value.GetType().Assembly), context)}");
        }
        return text.ToString().TrimEnd();
    }

    private string BuildStep59ReadyBindingDiagnostics(
        object screen,
        Step35ExecutionLoadContext context,
        int step,
        IReadOnlyDictionary<string, TypeDefinition> allTypes)
    {
        var text = new System.Text.StringBuilder();
        var screenType = RequireStartupLadderType(allTypes, CharacterSelectScreenManagedTypeFullName, step);
        var ready = screenType.Methods.SingleOrDefault(method =>
            method.Name == "_Ready" && !method.IsStatic && !method.HasGenericParameters && method.Parameters.Count == 0 && method.HasBody)
            ?? throw new MissingMethodException(CharacterSelectScreenManagedTypeFullName, "_Ready()");
        var instructions = ready.Body.Instructions;
        var charStoreIndex = instructions
            .Select((instruction, index) => (instruction, index))
            .Where(item => item.instruction.OpCode.Code == Code.Stfld &&
                item.instruction.Operand is FieldReference field && field.Name == "_charButtonContainer")
            .Select(item => item.index).Single();
        var ascensionStringIndex = instructions
            .Select((instruction, index) => (instruction, index))
            .Where(item => item.instruction.OpCode.Code == Code.Ldstr &&
                item.instruction.Operand is string value && value == "%AscensionPanel")
            .Select(item => item.index).Single();
        var ascensionStoreIndex = instructions
            .Select((instruction, index) => (instruction, index))
            .Where(item => item.instruction.OpCode.Code == Code.Stfld &&
                item.instruction.Operand is FieldReference field && field.Name == "_ascensionPanel")
            .Select(item => item.index).Single();
        if (!(charStoreIndex < ascensionStringIndex && ascensionStringIndex < ascensionStoreIndex))
            throw new InvalidDataException($"Step {step}.0 selected NCharacterSelectScreen._Ready ordering drifted around _charButtonContainer/%AscensionPanel/_ascensionPanel.");

        var windowStart = Math.Max(0, ascensionStringIndex - 5);
        var windowEnd = Math.Min(instructions.Count - 1, ascensionStoreIndex + 3);
        text.AppendLine($"Selected _Ready token=0x{ready.MetadataToken.ToUInt32():X8}; IL={instructions.Count}; charButtonStoreIndex={charStoreIndex}; ascensionStringIndex={ascensionStringIndex}; ascensionStoreIndex={ascensionStoreIndex}.");
        text.AppendLine("[SELECTED _READY ASCENSION LOOKUP WINDOW]");
        for (var i = windowStart; i <= windowEnd; i++)
            text.AppendLine("  " + FormatStep41Instruction(instructions[i]));

        var godotAssembly = (_callbackHandoff ?? throw new InvalidOperationException("Step 59.0 GodotSharp handoff absent.")).GodotSharpAssembly;
        var nodeType = godotAssembly.GetType("Godot.Node", throwOnError: true, ignoreCase: false)
            ?? throw new MissingMemberException("Godot.Node");
        var nodes = EnumerateStep39NodeGraph(screen, nodeType);
        var ascensionCandidates = nodes.Where(item =>
        {
            var name = GetStep39NodeName(item.Node, 0);
            var typeName = item.Node.GetType().FullName ?? string.Empty;
            return string.Equals(name, "AscensionPanel", StringComparison.Ordinal) ||
                   string.Equals(typeName, AscensionPanelManagedTypeFullNameForForensics, StringComparison.Ordinal) ||
                   name.Contains("Ascension", StringComparison.OrdinalIgnoreCase) ||
                   typeName.Contains("Ascension", StringComparison.OrdinalIgnoreCase);
        }).ToArray();
        text.AppendLine($"Live character-select descendant nodes={nodes.Count}; ascension-like candidates={ascensionCandidates.Length}.");
        text.AppendLine("[LIVE ASCENSION-LIKE NODES]");
        if (ascensionCandidates.Length == 0)
        {
            text.AppendLine("  <none>");
        }
        else
        {
            foreach (var item in ascensionCandidates)
            {
                var node = item.Node;
                var ownerProperty = node.GetType().GetProperty("Owner", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                var uniqueProperty = node.GetType().GetProperty("UniqueNameInOwner", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                object? owner = null;
                object? unique = null;
                try { owner = ownerProperty?.GetValue(node); } catch { }
                try { unique = uniqueProperty?.GetValue(node); } catch { }
                var loadContext = AssemblyLoadContext.GetLoadContext(node.GetType().Assembly);
                var nodeReady = false;
                var insideTree = false;
                try { nodeReady = InvokeRuntimeBoolForForensics(node, "IsNodeReady", step); } catch { }
                try { insideTree = InvokeRuntimeBoolForForensics(node, "IsInsideTree", step); } catch { }
                text.AppendLine($"  path={item.Path}; name={GetStep39NodeName(node, 0)}; type={node.GetType().FullName}; selectedContext={ReferenceEquals(loadContext, context)}; insideTree={insideTree}; nodeReady={nodeReady}; uniqueNameInOwner={unique?.ToString() ?? "<unreadable/null>"}; owner={(owner is null ? "<null>" : ReferenceEquals(owner, screen) ? "character-select-root" : owner.GetType().FullName)}");
            }
        }

        text.AppendLine("[LIVE CHARACTER-SELECT NODE TYPES AROUND ASCENSION]");
        foreach (var item in nodes.Where(item => item.Path.Contains("Ascension", StringComparison.OrdinalIgnoreCase)).Take(64))
            text.AppendLine($"  {item.Path} | {item.Node.GetType().FullName}");

        var pck = RequireEssentialResourcePackHandoff().PackAbsolutePath;
        RequireExactPckDirectoryResource(pck, CharacterSelectSceneResourcePath, step);
        RequireExactPckDirectoryResource(pck, AscensionPanelResourcePathForForensics, step);
        var characterEntry = ExtractStartupLadderPckEntryUnpinned(pck, CharacterSelectSceneResourcePath, step);
        var ascensionEntry = ExtractStartupLadderPckEntryUnpinned(pck, AscensionPanelResourcePathForForensics, step);
        var characterText = new System.Text.UTF8Encoding(false, true).GetString(characterEntry.Bytes);
        var ascensionText = new System.Text.UTF8Encoding(false, true).GetString(ascensionEntry.Bytes);
        text.AppendLine($"Character-select TSCN bytes={characterEntry.Bytes.Length}; sha256={Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(characterEntry.Bytes)).ToLowerInvariant()}.");
        text.AppendLine($"Ascension-panel TSCN bytes={ascensionEntry.Bytes.Length}; sha256={Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ascensionEntry.Bytes)).ToLowerInvariant()}.");
        text.AppendLine("[CHARACTER-SELECT TSCN ASCENSION CONTEXT]");
        text.AppendLine(BuildStep59TscnContext(characterText, "AscensionPanel", 4));
        text.AppendLine("[ASCENSION-PANEL TSCN ROOT CONTEXT]");
        text.AppendLine(BuildStep59TscnContext(ascensionText, "[node", 2, maxMatches: 8));
        return text.ToString().TrimEnd();
    }


    private string BuildStep59ConfirmButtonOnEnableDiagnostics(
        object screen,
        Step35ExecutionLoadContext context,
        int step,
        IReadOnlyDictionary<string, TypeDefinition> allTypes)
    {
        var text = new System.Text.StringBuilder();
        var failures = new List<string>();
        var embarkField = RequireRuntimeInstanceFieldForOwnership(screen.GetType(), "_embarkButton", step);
        var embark = embarkField.GetValue(screen);
        if (embark is null)
        {
            failures.Add("character-select _embarkButton=NULL");
            text.AppendLine("Retained _embarkButton: NULL");
            AppendStep59ConfirmButtonBlocker(failures);
            return text.ToString().TrimEnd();
        }

        var embarkTypeName = embark.GetType().FullName ?? "<unknown>";
        var loadContext = AssemblyLoadContext.GetLoadContext(embark.GetType().Assembly);
        var insideTree = InvokeRuntimeBoolForForensics(embark, "IsInsideTree", step);
        var nodeReady = InvokeRuntimeBoolForForensics(embark, "IsNodeReady", step);
        text.AppendLine($"Retained _embarkButton: type={embarkTypeName}; selectedContext={ReferenceEquals(loadContext, context)}; insideTree={insideTree}; nodeReady={nodeReady}.");
        if (!string.Equals(embarkTypeName, ConfirmButtonManagedTypeFullNameForForensics, StringComparison.Ordinal))
            failures.Add($"_embarkButton runtime type={embarkTypeName}, expected {ConfirmButtonManagedTypeFullNameForForensics}");
        if (!ReferenceEquals(loadContext, context))
            failures.Add("_embarkButton is not owned by the selected private load context");
        if (!insideTree)
            failures.Add("_embarkButton IsInsideTree()=false");
        if (!nodeReady)
            failures.Add("_embarkButton IsNodeReady()=false");

        var runtimeFields = new[]
        {
            "_outline", "_buttonImage", "_viewport", "_hotkeys", "_moveTween", "_showPos",
            "_isEnabled", "_controllerHotkeyIcon"
        };
        var fieldValues = new Dictionary<string, object?>(StringComparer.Ordinal);
        text.AppendLine("[LIVE EMBARK NCONFIRMBUTTON FIELDS]");
        foreach (var fieldName in runtimeFields)
        {
            var field = RequireRuntimeInstanceFieldForOwnership(embark.GetType(), fieldName, step);
            var value = field.GetValue(embark);
            fieldValues[fieldName] = value;
            text.AppendLine($"  {fieldName}={DescribeStep59RuntimeValue(value)}");
        }
        foreach (var fieldName in new[] { "_outline", "_buttonImage", "_viewport", "_hotkeys" })
        {
            if (fieldValues[fieldName] is null)
                failures.Add($"_embarkButton {fieldName}=NULL");
        }

        var confirmType = RequireStartupLadderType(allTypes, ConfirmButtonManagedTypeFullNameForForensics, step);
        var ready = confirmType.Methods.SingleOrDefault(method => method.Name == "_Ready" && !method.IsStatic && !method.HasGenericParameters && method.Parameters.Count == 0 && method.HasBody)
            ?? throw new MissingMethodException(ConfirmButtonManagedTypeFullNameForForensics, "_Ready()");
        var onEnable = confirmType.Methods.SingleOrDefault(method => method.Name == "OnEnable" && !method.IsStatic && !method.HasGenericParameters && method.Parameters.Count == 0 && method.HasBody)
            ?? throw new MissingMethodException(ConfirmButtonManagedTypeFullNameForForensics, "OnEnable()");
        var nButtonType = RequireStartupLadderType(allTypes, NButtonManagedTypeFullNameForForensics, step);
        var baseOnEnable = nButtonType.Methods.SingleOrDefault(method => method.Name == "OnEnable" && !method.IsStatic && !method.HasGenericParameters && method.Parameters.Count == 0 && method.HasBody)
            ?? throw new MissingMethodException(NButtonManagedTypeFullNameForForensics, "OnEnable()");
        var registerHotkeys = nButtonType.Methods.SingleOrDefault(method => method.Name == "RegisterHotkeys" && !method.IsStatic && !method.HasGenericParameters && method.Parameters.Count == 0 && method.HasBody)
            ?? throw new MissingMethodException(NButtonManagedTypeFullNameForForensics, "RegisterHotkeys()");

        RequireStep59FieldStore(ready, "_outline", step);
        RequireStep59FieldStore(ready, "_buttonImage", step);
        RequireStep59FieldStore(ready, "_viewport", step);
        RequireStep59FieldLoad(onEnable, "_outline", step);
        RequireStep59FieldLoad(onEnable, "_buttonImage", step);
        RequireStep59FieldLoad(onEnable, "_moveTween", step);
        if (!onEnable.Body.Instructions.Any(instruction => instruction.OpCode.Code == Code.Call && instruction.Operand is MethodReference method && method.Name == "CreateTween"))
            throw new InvalidDataException($"Step {step}.0 selected NConfirmButton.OnEnable no longer calls CreateTween().");

        text.AppendLine($"Selected NConfirmButton._Ready token=0x{ready.MetadataToken.ToUInt32():X8}; IL={ready.Body.Instructions.Count}.");
        text.AppendLine("[SELECTED NCONFIRMBUTTON _READY IL]");
        foreach (var instruction in ready.Body.Instructions)
            text.AppendLine("  " + FormatStep41Instruction(instruction));
        text.AppendLine($"Selected NConfirmButton.OnEnable token=0x{onEnable.MetadataToken.ToUInt32():X8}; IL={onEnable.Body.Instructions.Count}.");
        text.AppendLine("[SELECTED NCONFIRMBUTTON ONENABLE IL]");
        foreach (var instruction in onEnable.Body.Instructions)
            text.AppendLine("  " + FormatStep41Instruction(instruction));
        text.AppendLine($"Selected NButton.OnEnable token=0x{baseOnEnable.MetadataToken.ToUInt32():X8}; IL={baseOnEnable.Body.Instructions.Count}; RegisterHotkeys token=0x{registerHotkeys.MetadataToken.ToUInt32():X8}; IL={registerHotkeys.Body.Instructions.Count}.");
        text.AppendLine("[SELECTED NBUTTON ONENABLE + REGISTERHOTKEYS IL]");
        foreach (var instruction in baseOnEnable.Body.Instructions)
            text.AppendLine("  " + FormatStep41Instruction(instruction));
        foreach (var instruction in registerHotkeys.Body.Instructions)
            text.AppendLine("  " + FormatStep41Instruction(instruction));

        var godotAssembly = (_callbackHandoff ?? throw new InvalidOperationException("Step 59.0 GodotSharp handoff absent.")).GodotSharpAssembly;
        var nodeType = godotAssembly.GetType("Godot.Node", throwOnError: true, ignoreCase: false)
            ?? throw new MissingMemberException("Godot.Node");
        var descendants = EnumerateStep39NodeGraph(embark, nodeType);
        text.AppendLine($"[LIVE EMBARK NCONFIRMBUTTON NODE GRAPH] nodes={descendants.Count}");
        foreach (var item in descendants.Take(64))
            text.AppendLine($"  {item.Path} | name={GetStep39NodeName(item.Node, 0)} | type={item.Node.GetType().FullName}");

        var submenuStack = _step54SubmenuStack ?? throw new InvalidOperationException("Step 59.0 confirm-button diagnostics require retained NMainMenuSubmenuStack.");
        var characterPackedField = RequireRuntimeExactInstanceField(submenuStack.GetType(), MainMenuCharacterSelectSceneFieldName, PackedSceneManagedTypeFullName, step);
        var characterPacked = characterPackedField.GetValue(submenuStack)
            ?? throw new InvalidDataException("Step 59.0 retained character-select PackedScene is null during confirm-button diagnostics.");
        var embarkNodeName = GetStep39NodeName(embark, 0);
        text.AppendLine("[CHARACTER-SELECT SCENESTATE EMBARK ROOT — ALL SERIALIZED PROPERTIES]");
        text.AppendLine(DescribeStep59SceneState(characterPacked, [embarkNodeName], step, includeAllProperties: true));

        var pck = RequireEssentialResourcePackHandoff().PackAbsolutePath;
        RequireExactPckDirectoryResource(pck, CharacterSelectSceneResourcePath, step);
        RequireExactPckDirectoryResource(pck, ConfirmButtonResourcePathForForensics, step);
        var characterEntry = ExtractStartupLadderPckEntryUnpinned(pck, CharacterSelectSceneResourcePath, step);
        var confirmEntry = ExtractStartupLadderPckEntryUnpinned(pck, ConfirmButtonResourcePathForForensics, step);
        var characterText = new System.Text.UTF8Encoding(false, true).GetString(characterEntry.Bytes);
        var confirmText = new System.Text.UTF8Encoding(false, true).GetString(confirmEntry.Bytes);
        text.AppendLine($"Confirm-button TSCN bytes={confirmEntry.Bytes.Length}; sha256={Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(confirmEntry.Bytes)).ToLowerInvariant()}.");
        text.AppendLine("[CHARACTER-SELECT TSCN EMBARK CONTEXT]");
        text.AppendLine(BuildStep59TscnContext(characterText, embarkNodeName, 8, maxMatches: 8));
        text.AppendLine("[CONFIRM-BUTTON TSCN OUTLINE CONTEXT]");
        text.AppendLine(BuildStep59TscnContext(confirmText, "Outline", 5, maxMatches: 8));
        text.AppendLine("[CONFIRM-BUTTON TSCN IMAGE CONTEXT]");
        text.AppendLine(BuildStep59TscnContext(confirmText, "Image", 5, maxMatches: 8));

        if (failures.Count == 0)
            text.AppendLine("Definite OnEnable preflight blockers: <none>");
        else
            text.AppendLine("Definite OnEnable preflight blockers: " + string.Join(" | ", failures));
        AppendStep59ConfirmButtonBlocker(failures);
        return text.ToString().TrimEnd();
    }

    private string BuildStep59CompilationEfficientDiagnosticDeck(
        object screen,
        Step35ExecutionLoadContext context,
        int step,
        IReadOnlyDictionary<string, TypeDefinition> allTypes,
        IReadOnlyDictionary<string, MethodDefinition> allMethods)
    {
        var text = new System.Text.StringBuilder();
        text.AppendLine("[COMPILATION-EFFICIENT STEP-59 DIAGNOSTIC DECK]");
        text.AppendLine("Purpose: maximize evidence per compiled IPA. This deck is observational only: no field writes, _Ready replay, OnEnable invocation, handler arm, rendering restart, or game-state repair.");

        var embarkField = RequireRuntimeInstanceFieldForOwnership(screen.GetType(), "_embarkButton", step);
        var embark = embarkField.GetValue(screen);
        if (embark is null)
        {
            text.AppendLine("Retained embark button is NULL; deeper confirm-button runtime sections are unavailable.");
            return text.ToString().TrimEnd();
        }

        text.AppendLine(BuildStep59FullInheritedFieldMatrix(embark, "RETAINED EMBARK FULL INHERITED FIELD MATRIX"));
        text.AppendLine(BuildStep59PropertyTweenerCompatibilityRuntimeState());
        text.AppendLine(BuildStep59RuntimeAccessorSnapshot(embark, step));
        text.AppendLine(BuildStep59ConfirmButtonPeerMatrix(embark, context, step));
        text.AppendLine(BuildStep59NullFieldCandidateMatrix(embark, step));

        try
        {
            var confirmType = RequireStartupLadderType(allTypes, ConfirmButtonManagedTypeFullNameForForensics, step);
            var ready = RequireUniqueZeroArgBodyMethodForStep59(confirmType, "_Ready", step);
            var onEnable = RequireUniqueZeroArgBodyMethodForStep59(confirmType, "OnEnable", step);
            var nButtonType = RequireStartupLadderType(allTypes, NButtonManagedTypeFullNameForForensics, step);
            var baseOnEnable = RequireUniqueZeroArgBodyMethodForStep59(nButtonType, "OnEnable", step);
            var registerHotkeys = RequireUniqueZeroArgBodyMethodForStep59(nButtonType, "RegisterHotkeys", step);
            var clickableType = RequireStartupLadderType(allTypes, ClickableControlManagedTypeFullNameForForensics, step);
            var enable = clickableType.Methods.SingleOrDefault(method => method.Name == "Enable" && !method.IsStatic && !method.HasGenericParameters && method.Parameters.Count == 0 && method.HasBody);
            var characterType = RequireStartupLadderType(allTypes, CharacterSelectScreenManagedTypeFullName, step);
            var opened = characterType.Methods.SingleOrDefault(method => method.Name == "OnSubmenuOpened" && !method.IsStatic && !method.HasGenericParameters && method.Parameters.Count == 0 && method.HasBody);

            text.AppendLine(BuildStep59FieldAccessAndCallOrder(ready, "NCONFIRMBUTTON _READY FIELD/CALL ORDER"));
            text.AppendLine(BuildStep59FieldAccessAndCallOrder(onEnable, "NCONFIRMBUTTON ONENABLE FIELD/CALL ORDER"));
            text.AppendLine(BuildStep59FieldAccessAndCallOrder(baseOnEnable, "NBUTTON ONENABLE FIELD/CALL ORDER"));
            text.AppendLine(BuildStep59FieldAccessAndCallOrder(registerHotkeys, "NBUTTON REGISTERHOTKEYS FIELD/CALL ORDER"));
            if (enable is not null)
                text.AppendLine(BuildStep59FieldAccessAndCallOrder(enable, "NCLICKABLECONTROL ENABLE FIELD/CALL ORDER"));
            if (opened is not null)
                text.AppendLine(BuildStep59FieldAccessAndCallOrder(opened, "NCHARACTERSELECTSCREEN ONSUBMENUOPENED FIELD/CALL ORDER"));

            var guards = _step48LifecycleGuardAuthority ?? throw new InvalidOperationException("Step 59.0 diagnostic deck requires retained Step-48 runtime guards.");
            foreach (var root in new[] { ready, onEnable, baseOnEnable, registerHotkeys, enable, opened }.Where(method => method is not null).Cast<MethodDefinition>())
            {
                try
                {
                    var audit = AuditStartupLadderInvocationFrontier([root], allTypes, allMethods, guards);
                    text.AppendLine($"[TRANSITIVE EXECUTION FRONTIER — {root.DeclaringType.FullName}.{root.Name}]");
                    text.AppendLine(BuildStartupLadderInvocationFrontierAppendix(audit));
                }
                catch (Exception ex)
                {
                    text.AppendLine($"[TRANSITIVE EXECUTION FRONTIER — {root.DeclaringType.FullName}.{root.Name}] audit-failed={ex.GetType().FullName}: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            text.AppendLine($"Static IL/frontier deck failed non-fatally: {ex.GetType().FullName}: {ex.Message}");
        }

        text.AppendLine("Deck boundary: observational only; direct field repair, _Ready replay, direct OnEnable invocation, character selection, confirm/embark, run start, and Step 63+ remain unopened.");
        return text.ToString().TrimEnd();
    }

    private string BuildStep59PostFailureRuntimeDiagnosticDeck(object screen, Step35ExecutionLoadContext context, int step)
    {
        var text = new System.Text.StringBuilder();
        text.AppendLine("Purpose: capture runtime deltas immediately after an original-handler failure, before managed control returns to the UI.");
        var embarkField = RequireRuntimeInstanceFieldForOwnership(screen.GetType(), "_embarkButton", step);
        var embark = embarkField.GetValue(screen);
        if (embark is null)
            return "Retained _embarkButton became NULL after handler failure.";
        text.AppendLine(BuildStep59FullInheritedFieldMatrix(embark, "POSTFAIL EMBARK FULL INHERITED FIELD MATRIX"));
        text.AppendLine(BuildStep59PropertyTweenerCompatibilityRuntimeState());
        text.AppendLine(BuildStep59RuntimeAccessorSnapshot(embark, step));
        text.AppendLine(BuildStep59ConfirmButtonPeerMatrix(embark, context, step));
        text.AppendLine(BuildStep59NullFieldCandidateMatrix(embark, step));
        return text.ToString().TrimEnd();
    }

    private void RequireStep59PropertyTweenerProfile(PropertyTweenerExperimentProfile required, string operation)
    {
        if (SelectedPropertyTweenerProfile != required)
            throw new InvalidOperationException($"{operation} requires PropertyTweener profile={required}, but this fresh process was bootstrapped with profile={SelectedPropertyTweenerProfile}. Relaunch and choose the matching closed-path profile before Step 35.");
    }

    private void SetStep59PropertyTweenerRepairMode(bool wrapperRepairEnabled, bool fluentRepairEnabled, Action<string>? checkpoint, string checkpointMarker)
    {
        if (SelectedPropertyTweenerProfile == PropertyTweenerExperimentProfile.Baseline)
        {
            if (wrapperRepairEnabled || fluentRepairEnabled)
                throw new InvalidOperationException("Baseline PropertyTweener profile contains no experimental repair bridge and cannot enable repair. Relaunch with Wrapper or Full profile.");
            Checkpoint(checkpoint, $"{checkpointMarker} — profile=Baseline; wrapperRepairEnabled=False; fluentRepairEnabled=False; PropertyTweener experiment is absent by construction.");
            return;
        }
        if (SelectedPropertyTweenerProfile == PropertyTweenerExperimentProfile.Wrapper && fluentRepairEnabled)
            throw new InvalidOperationException("Wrapper PropertyTweener profile does not rewrite fluent SetEase/SetTrans/FromCurrent returns. Relaunch with Full profile for fluent repair.");

        var handoff = _callbackHandoff ?? throw new InvalidOperationException("Step 59 PropertyTweener repair mode requires the retained GodotSharp callback handoff.");
        var bridge = handoff.GodotSharpAssembly.GetType(GodotSharpDiagnosticBridgeTypeFullName, throwOnError: true, ignoreCase: false)
            ?? throw new MissingMemberException(GodotSharpDiagnosticBridgeTypeFullName);
        var wrapperField = bridge.GetField(GodotSharpPropertyTweenerRepairEnabledFieldName, BindingFlags.Public | BindingFlags.Static)
            ?? throw new MissingFieldException(GodotSharpDiagnosticBridgeTypeFullName, GodotSharpPropertyTweenerRepairEnabledFieldName);
        var fluentField = bridge.GetField(GodotSharpPropertyTweenerFluentRepairEnabledFieldName, BindingFlags.Public | BindingFlags.Static)
            ?? throw new MissingFieldException(GodotSharpDiagnosticBridgeTypeFullName, GodotSharpPropertyTweenerFluentRepairEnabledFieldName);
        wrapperField.SetValue(null, wrapperRepairEnabled);
        fluentField.SetValue(null, fluentRepairEnabled);
        Checkpoint(checkpoint, $"{checkpointMarker} — wrapperRepairEnabled={wrapperRepairEnabled}; fluentRepairEnabled={fluentRepairEnabled}; " + SanitizeCheckpoint(BuildStep59PropertyTweenerCompatibilityRuntimeState()));
    }

    private string BuildStep59PropertyTweenerCompatibilityRuntimeState()
    {
        var handoff = _callbackHandoff ?? throw new InvalidOperationException("Step 59 PropertyTweener compatibility state requires the retained GodotSharp callback handoff.");
        if (SelectedPropertyTweenerProfile == PropertyTweenerExperimentProfile.Baseline)
            return $"[PROPERTYTWEENER PROFILE] profile=Baseline; experimentInstalled=False; wrapperRepairEnabled=False; fluentRepairEnabled=False; bridgeAssembly={handoff.GodotSharpAssembly.GetName().FullName}; loadContext={AssemblyLoadContext.GetLoadContext(handoff.GodotSharpAssembly)?.Name ?? "<null>"}";

        var bridge = handoff.GodotSharpAssembly.GetType(GodotSharpDiagnosticBridgeTypeFullName, throwOnError: true, ignoreCase: false)
            ?? throw new MissingMemberException(GodotSharpDiagnosticBridgeTypeFullName);
        object? Read(string name) => (bridge.GetField(name, BindingFlags.Public | BindingFlags.Static)
            ?? throw new MissingFieldException(GodotSharpDiagnosticBridgeTypeFullName, name)).GetValue(null);
        var wrapperRepairEnabled = Read(GodotSharpPropertyTweenerRepairEnabledFieldName) is bool wrapperEnabled && wrapperEnabled;
        var fluentRepairEnabled = Read(GodotSharpPropertyTweenerFluentRepairEnabledFieldName) is bool fluentEnabled && fluentEnabled;
        var observations = Read(GodotSharpPropertyTweenerObservationCountFieldName) is int obs ? obs : -1;
        var nativeNulls = Read(GodotSharpPropertyTweenerNativeNullCountFieldName) is int nn ? nn : -1;
        var managedNulls = Read(GodotSharpPropertyTweenerManagedNullCountFieldName) is int mn ? mn : -1;
        var wrongTypes = Read(GodotSharpPropertyTweenerWrongTypeCountFieldName) is int wt ? wt : -1;
        var repairs = Read(GodotSharpPropertyTweenerFallbackCountFieldName) is int rp ? rp : -1;
        var nativePtr = Read(GodotSharpPropertyTweenerLastNativePtrFieldName) is IntPtr ptr ? ptr : IntPtr.Zero;
        var lastManaged = Read(GodotSharpPropertyTweenerLastManagedObjectFieldName);
        var fluentObservations = Read(GodotSharpPropertyTweenerFluentObservationCountFieldName) is int fo ? fo : -1;
        var fluentRepairs = Read(GodotSharpPropertyTweenerFluentFallbackCountFieldName) is int fr ? fr : -1;
        var lastFluentManaged = Read(GodotSharpPropertyTweenerLastFluentManagedObjectFieldName);
        var lastFluentStage = Read(GodotSharpPropertyTweenerLastFluentStageFieldName) as string ?? "<none>";
        var propertyTweenerType = handoff.GodotSharpAssembly.GetType("Godot.PropertyTweener", throwOnError: true, ignoreCase: false)
            ?? throw new MissingMemberException("Godot.PropertyTweener");
        var nativeCtors = propertyTweenerType.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
            .Count(ctor => ctor.GetParameters().Length == 1 && ctor.GetParameters()[0].ParameterType == typeof(IntPtr));
        var lastManagedType = lastManaged?.GetType().FullName ?? "<null>";
        var lastManagedIsPropertyTweener = lastManaged is not null && propertyTweenerType.IsInstanceOfType(lastManaged);
        var lastFluentManagedType = lastFluentManaged?.GetType().FullName ?? "<null>";
        var lastFluentManagedIsPropertyTweener = lastFluentManaged is not null && propertyTweenerType.IsInstanceOfType(lastFluentManaged);
        return $"[PROPERTYTWEENER MANAGED-WRAPPER OBSERVE/REPAIR] profile={SelectedPropertyTweenerProfile}; wrapperRepairEnabled={wrapperRepairEnabled}; fluentRepairEnabled={fluentRepairEnabled}; observations={observations}; nativeNulls={nativeNulls}; managedNulls={managedNulls}; wrongTypes={wrongTypes}; repairs={repairs}; lastNativePtr=0x{nativePtr.ToInt64():X}; lastManagedType={lastManagedType}; lastManagedIsPropertyTweener={lastManagedIsPropertyTweener}; fluentObservations={fluentObservations}; fluentRepairs={fluentRepairs}; lastFluentStage={lastFluentStage}; lastFluentManagedType={lastFluentManagedType}; lastFluentManagedIsPropertyTweener={lastFluentManagedIsPropertyTweener}; propertyTweenerIntPtrConstructors={nativeCtors}; bridgeAssembly={handoff.GodotSharpAssembly.GetName().FullName}; loadContext={AssemblyLoadContext.GetLoadContext(handoff.GodotSharpAssembly)?.Name ?? "<null>"}";
    }

    private static MethodDefinition RequireUniqueZeroArgBodyMethodForStep59(TypeDefinition type, string name, int step)
        => type.Methods.SingleOrDefault(method => method.Name == name && !method.IsStatic && !method.HasGenericParameters && method.Parameters.Count == 0 && method.HasBody)
            ?? throw new MissingMethodException(type.FullName, $"Step {step}.0 {name}()");

    private static string BuildStep59FullInheritedFieldMatrix(object instance, string heading)
    {
        var text = new System.Text.StringBuilder();
        text.AppendLine("[" + heading + "]");
        for (var type = instance.GetType(); type is not null; type = type.BaseType)
        {
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
                .Where(field => !field.IsStatic)
                .OrderBy(field => field.Name, StringComparer.Ordinal)
                .ToArray();
            if (fields.Length == 0)
                continue;
            text.AppendLine($"  declaringType={type.FullName}");
            foreach (var field in fields)
            {
                try
                {
                    text.AppendLine($"    {field.FieldType.FullName} {field.Name}={DescribeStep59RuntimeValue(field.GetValue(instance))}");
                }
                catch (Exception ex)
                {
                    text.AppendLine($"    {field.FieldType.FullName} {field.Name}=<read-failed:{ex.GetType().Name}:{ex.Message}>");
                }
            }
        }
        return text.ToString().TrimEnd();
    }

    private static string BuildStep59RuntimeAccessorSnapshot(object instance, int step)
    {
        var text = new System.Text.StringBuilder();
        text.AppendLine("[RETAINED EMBARK OBSERVATIONAL ACCESSORS]");
        foreach (var methodName in new[] { "IsInsideTree", "IsNodeReady", "IsVisibleInTree", "GetViewport", "GetTree", "GetParent", "GetOwner" })
        {
            try
            {
                var method = EnumerateRuntimeMethodsForOwnership(instance.GetType())
                    .FirstOrDefault(method => method.Name == methodName && !method.IsGenericMethod && method.GetParameters().Length == 0);
                if (method is null)
                {
                    text.AppendLine($"  {methodName}()=<method-absent>");
                    continue;
                }
                var value = method.Invoke(instance, null);
                text.AppendLine($"  {methodName}()={DescribeStep59RuntimeValue(value)}");
            }
            catch (TargetInvocationException tie) when (tie.InnerException is not null)
            {
                text.AppendLine($"  {methodName}()=<inner-exception:{tie.InnerException.GetType().FullName}:{tie.InnerException.Message}>");
            }
            catch (Exception ex)
            {
                text.AppendLine($"  {methodName}()=<exception:{ex.GetType().FullName}:{ex.Message}>");
            }
        }
        return text.ToString().TrimEnd();
    }

    private string BuildStep59ConfirmButtonPeerMatrix(object embark, Step35ExecutionLoadContext context, int step)
    {
        var text = new System.Text.StringBuilder();
        text.AppendLine("[LIVE NCONFIRMBUTTON PEER MATRIX — WHOLE RETAINED SCENETREE]");
        try
        {
            var godotAssembly = (_callbackHandoff ?? throw new InvalidOperationException("Step 59.0 GodotSharp handoff absent.")).GodotSharpAssembly;
            var nodeType = godotAssembly.GetType("Godot.Node", throwOnError: true, ignoreCase: false) ?? throw new MissingMemberException("Godot.Node");
            var root = _step39SceneTreeRoot ?? throw new InvalidOperationException("Step 59.0 retained SceneTree root is absent.");
            var nodes = EnumerateStep39NodeGraph(root, nodeType, maxNodes: 4096);
            var confirmType = embark.GetType();
            var peers = nodes.Where(item => confirmType.IsInstanceOfType(item.Node)).Take(128).ToArray();
            text.AppendLine($"  peers={peers.Length}; exactRuntimeType={confirmType.FullName}; selectedContext={ReferenceEquals(AssemblyLoadContext.GetLoadContext(confirmType.Assembly), context)}");
            foreach (var peer in peers)
            {
                text.Append($"  path={peer.Path}; type={peer.Node.GetType().FullName}");
                foreach (var fieldName in new[] { "_outline", "_buttonImage", "_viewport", "_hotkeys", "_moveTween", "_isEnabled", "_controllerHotkeyIcon" })
                {
                    try
                    {
                        var field = RequireRuntimeInstanceFieldForOwnership(peer.Node.GetType(), fieldName, step);
                        text.Append($"; {fieldName}={DescribeStep59RuntimeValue(field.GetValue(peer.Node))}");
                    }
                    catch (Exception ex)
                    {
                        text.Append($"; {fieldName}=<unavailable:{ex.GetType().Name}>");
                    }
                }
                try { text.Append($"; insideTree={InvokeRuntimeBoolForForensics(peer.Node, "IsInsideTree", step)}"); } catch { text.Append("; insideTree=<error>"); }
                try { text.Append($"; nodeReady={InvokeRuntimeBoolForForensics(peer.Node, "IsNodeReady", step)}"); } catch { text.Append("; nodeReady=<error>"); }
                text.AppendLine();
            }
            if (peers.Length == 0)
                text.AppendLine("  <no live NConfirmButton peers found>");
        }
        catch (Exception ex)
        {
            text.AppendLine($"  peer-matrix-failed={ex.GetType().FullName}: {ex.Message}");
        }
        return text.ToString().TrimEnd();
    }

    private string BuildStep59NullFieldCandidateMatrix(object embark, int step)
    {
        var text = new System.Text.StringBuilder();
        text.AppendLine("[NULL-FIELD LIVE NODE CANDIDATE MATRIX]");
        try
        {
            var godotAssembly = (_callbackHandoff ?? throw new InvalidOperationException("Step 59.0 GodotSharp handoff absent.")).GodotSharpAssembly;
            var nodeType = godotAssembly.GetType("Godot.Node", throwOnError: true, ignoreCase: false) ?? throw new MissingMemberException("Godot.Node");
            var nodes = EnumerateStep39NodeGraph(embark, nodeType);
            foreach (var fieldName in new[] { "_outline", "_buttonImage", "_viewport", "_hotkeys", "_controllerHotkeyIcon" })
            {
                try
                {
                    var field = RequireRuntimeInstanceFieldForOwnership(embark.GetType(), fieldName, step);
                    var current = field.GetValue(embark);
                    if (current is not null)
                    {
                        text.AppendLine($"  {fieldName}: already-bound={DescribeStep59RuntimeValue(current)}");
                        continue;
                    }
                    var candidates = nodes.Where(item => field.FieldType.IsInstanceOfType(item.Node)).Take(32).ToArray();
                    text.AppendLine($"  {fieldName}: NULL; fieldType={field.FieldType.FullName}; descendantCandidates={candidates.Length}");
                    foreach (var candidate in candidates)
                        text.AppendLine($"    {candidate.Path} | name={GetStep39NodeName(candidate.Node, 0)} | type={candidate.Node.GetType().FullName}");
                }
                catch (Exception ex)
                {
                    text.AppendLine($"  {fieldName}: candidate-scan-failed={ex.GetType().FullName}: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            text.AppendLine($"  matrix-failed={ex.GetType().FullName}: {ex.Message}");
        }
        return text.ToString().TrimEnd();
    }

    private static string BuildStep59FieldAccessAndCallOrder(MethodDefinition method, string heading)
    {
        var text = new System.Text.StringBuilder();
        text.AppendLine("[" + heading + "]");
        foreach (var instruction in method.Body.Instructions)
        {
            var include = instruction.OpCode.Code is Code.Ldfld or Code.Ldflda or Code.Stfld or Code.Call or Code.Callvirt or Code.Newobj or Code.Ldstr or Code.Brtrue or Code.Brtrue_S or Code.Brfalse or Code.Brfalse_S;
            if (!include)
                continue;
            text.AppendLine($"  IL_{instruction.Offset:X4}: {FormatStep41Instruction(instruction)}");
        }
        return text.ToString().TrimEnd();
    }

    private void AppendStep59ConfirmButtonBlocker(IReadOnlyCollection<string> failures)
    {
        if (failures.Count == 0)
            return;
        var detail = "confirm-button preflight: " + string.Join(" | ", failures);
        _step59ReadyBindingBlocker = string.IsNullOrWhiteSpace(_step59ReadyBindingBlocker)
            ? detail
            : _step59ReadyBindingBlocker + " | " + detail;
    }

    private static string DescribeStep59RuntimeValue(object? value)
    {
        if (value is null)
            return "NULL";
        if (value is Array array)
            return $"{value.GetType().FullName}[Length={array.Length}]";
        if (value is bool boolean)
            return $"System.Boolean:{boolean}";
        var type = value.GetType();
        if (type.IsValueType || value is string)
            return $"{type.FullName}:{value}";
        return type.FullName ?? type.Name;
    }

    private static void RequireStep59FieldStore(MethodDefinition method, string fieldName, int step)
    {
        if (!method.Body.Instructions.Any(instruction => instruction.OpCode.Code == Code.Stfld && instruction.Operand is FieldReference field && field.Name == fieldName))
            throw new InvalidDataException($"Step {step}.0 selected {method.DeclaringType.FullName}.{method.Name} no longer stores {fieldName}.");
    }

    private static void RequireStep59FieldLoad(MethodDefinition method, string fieldName, int step)
    {
        if (!method.Body.Instructions.Any(instruction => instruction.OpCode.Code == Code.Ldfld && instruction.Operand is FieldReference field && field.Name == fieldName))
            throw new InvalidDataException($"Step {step}.0 selected {method.DeclaringType.FullName}.{method.Name} no longer loads {fieldName}.");
    }


    private string BuildStep59UniqueNameProvenanceDiagnostics(
        object liveCharacterScreen,
        Step35ExecutionLoadContext context,
        int step)
    {
        var text = new System.Text.StringBuilder();
        text.AppendLine("Purpose: distinguish serialized SceneState loss, PackedScene instantiation loss, live-tree/owner loss, and managed %Name lookup failure without mutating retained game state.");

        var submenuStack = _step54SubmenuStack ?? throw new InvalidOperationException("Step 59.0 unique-name provenance requires retained NMainMenuSubmenuStack.");
        var characterPackedField = RequireRuntimeExactInstanceField(submenuStack.GetType(), MainMenuCharacterSelectSceneFieldName, PackedSceneManagedTypeFullName, step);
        var characterPacked = characterPackedField.GetValue(submenuStack)
            ?? throw new InvalidDataException("Step 59.0 retained character-select PackedScene is null during unique-name provenance.");

        text.AppendLine("[CHARACTER-SELECT PACKEDSCENE SCENESTATE]");
        text.AppendLine(DescribeStep59SceneState(characterPacked, ["AscensionPanel", "ActDropdown"], step));

        text.AppendLine("[CHARACTER-SELECT TEMPORARY OFF-TREE INSTANCE]");
        object? temporaryCharacter = null;
        try
        {
            temporaryCharacter = InstantiateStep59DiagnosticPackedScene(characterPacked, step, "character-select");
            text.AppendLine(DescribeStep59LiveUniqueTargets(
                temporaryCharacter,
                context,
                step,
                [
                    new Step59UniqueTarget("AscensionPanel", "%AscensionPanel", "AscensionPanel"),
                    new Step59UniqueTarget("ActDropdown", "%ActDropdown", "ActDropdown")
                ]));
        }
        finally
        {
            if (temporaryCharacter is not null)
                TryReleaseStep39OffTreeInstance(temporaryCharacter, null, "Step59 temporary character-select provenance clone");
        }

        text.AppendLine("[CHARACTER-SELECT RETAINED LIVE INSTANCE]");
        text.AppendLine(DescribeStep59LiveUniqueTargets(
            liveCharacterScreen,
            context,
            step,
            [
                new Step59UniqueTarget("AscensionPanel", "%AscensionPanel", "AscensionPanel"),
                new Step59UniqueTarget("ActDropdown", "%ActDropdown", "ActDropdown")
            ]));

        var gamePacked = _gameScenePackedResource?.Resource
            ?? throw new InvalidOperationException("Step 59.0 unique-name provenance requires retained Step-37 game PackedScene.");
        text.AppendLine("[GAME PACKEDSCENE SCENESTATE]");
        text.AppendLine(DescribeStep59SceneState(
            gamePacked,
            ["InputManager", "HotkeyManager", "RootSceneContainer", "ReactionWheel", "ReactionContainer", "MultiplayerTimeoutOverlay", "WorldEnvironment"],
            step));

        text.AppendLine("[GAME TEMPORARY OFF-TREE INSTANCE]");
        object? temporaryGame = null;
        try
        {
            temporaryGame = InstantiateStep59DiagnosticPackedScene(gamePacked, step, "game");
            text.AppendLine(DescribeStep59LiveUniqueTargets(
                temporaryGame,
                context,
                step,
                [
                    new Step59UniqueTarget("InputManager", "%InputManager", "InputManager"),
                    new Step59UniqueTarget("HotkeyManager", "%HotkeyManager", "HotkeyManager"),
                    new Step59UniqueTarget("RootSceneContainer", "%RootSceneContainer", "RootSceneContainer"),
                    new Step59UniqueTarget("ReactionWheel", "%ReactionWheel", "ReactionWheel"),
                    new Step59UniqueTarget("ReactionContainer", "%ReactionContainer", "ReactionContainer"),
                    new Step59UniqueTarget("MultiplayerTimeoutOverlay", "%MultiplayerTimeoutOverlay", "MultiplayerTimeoutOverlay"),
                    new Step59UniqueTarget("WorldEnvironment", "%WorldEnvironment", "WorldEnvironment")
                ]));
        }
        finally
        {
            if (temporaryGame is not null)
                TryReleaseStep39OffTreeInstance(temporaryGame, null, "Step59 temporary game provenance clone");
        }

        var liveGame = _step39NGameInstance ?? throw new InvalidOperationException("Step 59.0 unique-name provenance requires retained live NGame.");
        text.AppendLine("[GAME RETAINED LIVE INSTANCE]");
        text.AppendLine(DescribeStep59LiveUniqueTargets(
            liveGame,
            context,
            step,
            [
                new Step59UniqueTarget("InputManager", "%InputManager", "InputManager"),
                new Step59UniqueTarget("HotkeyManager", "%HotkeyManager", "HotkeyManager"),
                new Step59UniqueTarget("RootSceneContainer", "%RootSceneContainer", "RootSceneContainer"),
                new Step59UniqueTarget("ReactionWheel", "%ReactionWheel", "ReactionWheel"),
                new Step59UniqueTarget("ReactionContainer", "%ReactionContainer", "ReactionContainer"),
                new Step59UniqueTarget("MultiplayerTimeoutOverlay", "%MultiplayerTimeoutOverlay", "MultiplayerTimeoutOverlay"),
                new Step59UniqueTarget("WorldEnvironment", "%WorldEnvironment", "WorldEnvironment")
            ]));

        if (_step47MainMenuPackedScene is not null && _step48MainMenuInstance is not null)
        {
            text.AppendLine("[MAIN-MENU PACKEDSCENE SCENESTATE]");
            text.AppendLine(DescribeStep59SceneState(
                _step47MainMenuPackedScene,
                ["Submenus", "MainMenuBg", "ContinueRunInfo", "PatchNotesScreen"],
                step));
            text.AppendLine("[MAIN-MENU RETAINED LIVE INSTANCE]");
            text.AppendLine(DescribeStep59LiveUniqueTargets(
                _step48MainMenuInstance,
                context,
                step,
                [
                    new Step59UniqueTarget("Submenus", "%Submenus", "Submenus"),
                    new Step59UniqueTarget("MainMenuBg", "%MainMenuBg", "MainMenuBg"),
                    new Step59UniqueTarget("ContinueRunInfo", "%ContinueRunInfo", "ContinueRunInfo"),
                    new Step59UniqueTarget("PatchNotesScreen", "%PatchNotesScreen", "PatchNotesScreen")
                ]));
        }

        return text.ToString().TrimEnd();
    }

    private static string DescribeStep59SceneState(object packedScene, IReadOnlyCollection<string> targetNames, int step, bool includeAllProperties = false)
    {
        var getState = packedScene.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .SingleOrDefault(method => method.Name == "GetState" && !method.IsGenericMethod && method.GetParameters().Length == 0)
            ?? throw new MissingMethodException(packedScene.GetType().FullName, "GetState()");
        var state = InvokeStep59DiagnosticMethod(getState, packedScene, null, step, "PackedScene.GetState")
            ?? throw new InvalidDataException($"Step {step}.0 PackedScene.GetState returned null.");

        var stateType = state.GetType();
        var getNodeCount = RequireStep59DiagnosticMethod(stateType, "GetNodeCount", 0);
        var getNodeName = RequireStep59DiagnosticMethod(stateType, "GetNodeName", 1);
        var getNodePath = stateType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(method => method.Name == "GetNodePath" && !method.IsGenericMethod)
            .OrderBy(method => method.GetParameters().Length)
            .FirstOrDefault(method =>
            {
                var parameters = method.GetParameters();
                return parameters.Length is 1 or 2 && parameters[0].ParameterType == typeof(int);
            }) ?? throw new MissingMethodException(stateType.FullName, "GetNodePath(int[,bool])");
        var getOwnerPath = RequireStep59DiagnosticMethod(stateType, "GetNodeOwnerPath", 1);
        var getNodeType = RequireStep59DiagnosticMethod(stateType, "GetNodeType", 1);
        var getPropertyCount = RequireStep59DiagnosticMethod(stateType, "GetNodePropertyCount", 1);
        var getPropertyName = RequireStep59DiagnosticMethod(stateType, "GetNodePropertyName", 2);
        var getPropertyValue = RequireStep59DiagnosticMethod(stateType, "GetNodePropertyValue", 2);
        var getNodeInstance = stateType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .SingleOrDefault(method => method.Name == "GetNodeInstance" && !method.IsGenericMethod && method.GetParameters().Length == 1);

        var countObj = InvokeStep59DiagnosticMethod(getNodeCount, state, null, step, "SceneState.GetNodeCount");
        var count = countObj is int c ? c : Convert.ToInt32(countObj, System.Globalization.CultureInfo.InvariantCulture);
        var lines = new List<string> { $"sceneStateType={stateType.FullName}; nodeCount={count}" };
        var found = new HashSet<string>(StringComparer.Ordinal);

        for (var i = 0; i < count; i++)
        {
            var name = InvokeStep59DiagnosticMethod(getNodeName, state, [i], step, "SceneState.GetNodeName")?.ToString() ?? "<null>";
            if (!targetNames.Contains(name, StringComparer.Ordinal))
                continue;
            found.Add(name);
            object? pathValue;
            var pathParameters = getNodePath.GetParameters();
            pathValue = pathParameters.Length == 1
                ? InvokeStep59DiagnosticMethod(getNodePath, state, [i], step, "SceneState.GetNodePath")
                : InvokeStep59DiagnosticMethod(getNodePath, state, [i, false], step, "SceneState.GetNodePath");
            var ownerPath = InvokeStep59DiagnosticMethod(getOwnerPath, state, [i], step, "SceneState.GetNodeOwnerPath")?.ToString() ?? "<null>";
            var nodeType = InvokeStep59DiagnosticMethod(getNodeType, state, [i], step, "SceneState.GetNodeType")?.ToString() ?? "<null>";
            var instance = getNodeInstance is null ? null : InvokeStep59DiagnosticMethod(getNodeInstance, state, [i], step, "SceneState.GetNodeInstance");
            var propertyCountObj = InvokeStep59DiagnosticMethod(getPropertyCount, state, [i], step, "SceneState.GetNodePropertyCount");
            var propertyCount = propertyCountObj is int pc ? pc : Convert.ToInt32(propertyCountObj, System.Globalization.CultureInfo.InvariantCulture);
            var properties = new List<string>();
            for (var p = 0; p < propertyCount; p++)
            {
                var propName = InvokeStep59DiagnosticMethod(getPropertyName, state, [i, p], step, "SceneState.GetNodePropertyName")?.ToString() ?? "<null>";
                if (!includeAllProperties &&
                    !string.Equals(propName, "unique_name_in_owner", StringComparison.Ordinal) &&
                    !string.Equals(propName, "script", StringComparison.Ordinal))
                    continue;
                var propValue = InvokeStep59DiagnosticMethod(getPropertyValue, state, [i, p], step, "SceneState.GetNodePropertyValue");
                properties.Add(propName + "=" + (includeAllProperties ? FormatStep59DiagnosticValueVerbose(propValue) : FormatStep59DiagnosticValue(propValue)));
            }
            lines.Add($"name={name}; path={pathValue?.ToString() ?? "<null>"}; ownerPath={ownerPath}; type={nodeType}; instance={(instance is null ? "NO" : "YES:" + (instance.GetType().FullName ?? "<unknown>"))}; properties=[{string.Join(",", properties)}]");
        }
        foreach (var target in targetNames.Where(target => !found.Contains(target)))
            lines.Add($"name={target}; SCENESTATE_NODE_NOT_FOUND");
        return string.Join("\n", lines);
    }

    private static object InstantiateStep59DiagnosticPackedScene(object packedScene, int step, string scope)
    {
        var nodeType = packedScene.GetType().Assembly.GetType("Godot.Node", throwOnError: true, ignoreCase: false)
            ?? throw new MissingMemberException("Godot.Node");
        var instantiate = RequireStartupLadderInstantiateMethod(packedScene.GetType(), nodeType);
        var parameterType = instantiate.GetParameters()[0].ParameterType;
        var disabled = Enum.ToObject(parameterType, 0);
        return InvokeStep59DiagnosticMethod(instantiate, packedScene, [disabled], step, $"{scope} PackedScene.Instantiate")
            ?? throw new InvalidDataException($"Step {step}.0 temporary {scope} PackedScene.Instantiate returned null.");
    }

    private static string DescribeStep59LiveUniqueTargets(
        object root,
        Step35ExecutionLoadContext context,
        int step,
        IReadOnlyCollection<Step59UniqueTarget> targets)
    {
        var lines = new List<string>();
        foreach (var target in targets)
        {
            object? direct = null;
            try { direct = FindStep59ChildByName(root, target.DirectName); } catch { }
            var unique = TryStep59NodeLookup(root, target.UniquePath, step, out var uniqueError);
            var directPathLookup = TryStep59NodeLookup(root, target.DirectPath, step, out var directError);
            var observed = direct ?? directPathLookup;
            if (observed is null)
            {
                lines.Add($"{target.Label}: direct/find=NULL; percentLookup={(unique is null ? "NULL" : "NONNULL")}; percentError={uniqueError}; directError={directError}");
                continue;
            }

            var owner = TryReadStep59Property(observed, "Owner");
            var uniqueFlag = TryReadStep59UniqueFlag(observed);
            var insideTree = TryInvokeStep59Bool(observed, "IsInsideTree");
            var nodeReady = TryInvokeStep59Bool(observed, "IsNodeReady");
            var loadContext = AssemblyLoadContext.GetLoadContext(observed.GetType().Assembly);
            var ownerIdentity = owner is null ? "<null>" : ReferenceEquals(owner, root) ? "ROOT" : owner.GetType().FullName ?? "<unknown>";
            lines.Add(
                $"{target.Label}: type={observed.GetType().FullName}; selectedContext={ReferenceEquals(loadContext, context)}; " +
                $"insideTree={insideTree}; nodeReady={nodeReady}; uniqueNameInOwner={uniqueFlag}; owner={ownerIdentity}; " +
                $"directPathLookup={(directPathLookup is null ? "NULL" : ReferenceEquals(directPathLookup, observed) ? "EXACT" : "OTHER:" + directPathLookup.GetType().FullName)}; " +
                $"percentLookup={(unique is null ? "NULL" : ReferenceEquals(unique, observed) ? "EXACT" : "OTHER:" + unique.GetType().FullName)}; " +
                $"percentError={uniqueError}; directError={directError}");
        }
        return string.Join("\n", lines);
    }

    private static object? FindStep59ChildByName(object root, string name)
    {
        var findChild = root.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .SingleOrDefault(method =>
            {
                if (method.Name != "FindChild" || method.IsGenericMethod)
                    return false;
                var parameters = method.GetParameters();
                return parameters.Length == 3 &&
                       parameters[0].ParameterType == typeof(string) &&
                       parameters[1].ParameterType == typeof(bool) &&
                       parameters[2].ParameterType == typeof(bool);
            });
        return findChild?.Invoke(root, [name, true, false]);
    }

    private static object? TryStep59NodeLookup(object root, string path, int step, out string error)
    {
        error = "<none>";
        try
        {
            var nodePathType = root.GetType().Assembly.GetType("Godot.NodePath", throwOnError: true, ignoreCase: false)
                ?? throw new MissingMemberException("Godot.NodePath");
            var nodePath = Activator.CreateInstance(nodePathType, [path])
                ?? throw new InvalidOperationException("Godot.NodePath(string) returned null.");
            var method = root.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(candidate => candidate.Name == "GetNodeOrNull" && !candidate.IsGenericMethod)
                .SingleOrDefault(candidate =>
                {
                    var parameters = candidate.GetParameters();
                    return parameters.Length == 1 && parameters[0].ParameterType == nodePathType;
                });
            if (method is null)
            {
                method = root.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .Where(candidate => candidate.Name == "GetNode" && !candidate.IsGenericMethod)
                    .SingleOrDefault(candidate =>
                    {
                        var parameters = candidate.GetParameters();
                        return parameters.Length == 1 && parameters[0].ParameterType == nodePathType;
                    });
            }
            if (method is null)
                throw new MissingMethodException(root.GetType().FullName, "GetNodeOrNull(NodePath)/GetNode(NodePath)");
            return InvokeStep59DiagnosticMethod(method, root, [nodePath], step, $"Node lookup '{path}'");
        }
        catch (Exception ex)
        {
            error = ex.GetType().Name + ":" + ex.Message;
            return null;
        }
    }

    private static MethodInfo RequireStep59DiagnosticMethod(Type type, string name, int parameterCount)
        => type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .SingleOrDefault(method => method.Name == name && !method.IsGenericMethod && method.GetParameters().Length == parameterCount)
            ?? throw new MissingMethodException(type.FullName, name);

    private static object? InvokeStep59DiagnosticMethod(MethodInfo method, object? instance, object?[]? arguments, int step, string label)
    {
        try
        {
            return method.Invoke(instance, arguments);
        }
        catch (TargetInvocationException tie) when (tie.InnerException is not null)
        {
            throw new InvalidOperationException($"Step {step}.0 {label} threw {tie.InnerException.GetType().FullName}: {tie.InnerException.Message}", tie.InnerException);
        }
    }

    private static object? TryReadStep59Property(object instance, string propertyName)
    {
        try
        {
            return instance.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(instance);
        }
        catch { return null; }
    }

    private static string TryReadStep59UniqueFlag(object instance)
    {
        try
        {
            var property = instance.GetType().GetProperty("UniqueNameInOwner", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (property?.GetValue(instance) is bool value)
                return value.ToString();
            var method = instance.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .SingleOrDefault(candidate => candidate.Name == "IsUniqueNameInOwner" && !candidate.IsGenericMethod && candidate.GetParameters().Length == 0);
            return method?.Invoke(instance, null)?.ToString() ?? "<unreadable>";
        }
        catch (Exception ex) { return "<error:" + ex.GetType().Name + ">"; }
    }

    private static string TryInvokeStep59Bool(object instance, string methodName)
    {
        try
        {
            var method = instance.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .SingleOrDefault(candidate => candidate.Name == methodName && !candidate.IsGenericMethod && candidate.GetParameters().Length == 0);
            return method?.Invoke(instance, null)?.ToString() ?? "<unreadable>";
        }
        catch (Exception ex) { return "<error:" + ex.GetType().Name + ">"; }
    }

    private static string FormatStep59DiagnosticValueVerbose(object? value)
    {
        if (value is null)
            return "<null>";
        try
        {
            return $"{value.GetType().FullName}:{value}";
        }
        catch (Exception ex)
        {
            return $"{value.GetType().FullName}:<format-error:{ex.GetType().Name}>";
        }
    }

    private static string FormatStep59DiagnosticValue(object? value)
    {
        if (value is null)
            return "<null>";
        try
        {
            var asBool = value.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .SingleOrDefault(method => method.Name == "AsBool" && !method.IsGenericMethod && method.GetParameters().Length == 0);
            if (asBool is not null)
            {
                try
                {
                    var boolValue = asBool.Invoke(value, null);
                    if (boolValue is bool b)
                        return b.ToString();
                }
                catch { }
            }
            return value.ToString() ?? "<null-string>";
        }
        catch (Exception ex) { return "<format-error:" + ex.GetType().Name + ">"; }
    }

    private sealed record Step59UniqueTarget(string Label, string UniquePath, string DirectPath)
    {
        public string DirectName => DirectPath.Contains('/') ? DirectPath[(DirectPath.LastIndexOf('/') + 1)..] : DirectPath;
    }

    private static string BuildStep59TscnContext(string text, string needle, int radius, int maxMatches = 16)
    {
        var lines = text.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n').Split('\n');
        var selected = new SortedSet<int>();
        var matches = 0;
        for (var i = 0; i < lines.Length && matches < maxMatches; i++)
        {
            if (!lines[i].Contains(needle, StringComparison.OrdinalIgnoreCase))
                continue;
            matches++;
            for (var j = Math.Max(0, i - radius); j <= Math.Min(lines.Length - 1, i + radius); j++)
                selected.Add(j);
        }
        if (selected.Count == 0)
            return "  <no matching lines>";
        return string.Join("\n", selected.Select(index => $"  L{index + 1}: {lines[index]}"));
    }

    private static bool InvokeRuntimeBoolForForensics(object instance, string methodName, int step)
    {
        var method = RequireZeroArgBoolMethod(instance.GetType(), methodName);
        try
        {
            var value = method.Invoke(instance, null);
            return value is bool result ? result : throw new InvalidDataException($"Step {step}.0 {instance.GetType().FullName}.{methodName}() did not return bool.");
        }
        catch (TargetInvocationException tie) when (tie.InnerException is not null)
        {
            throw new InvalidOperationException($"Step {step}.0 {instance.GetType().FullName}.{methodName}() inner exception: {tie.InnerException.GetType().FullName}: {tie.InnerException.Message}", tie.InnerException);
        }
    }

    private static bool ReadsFirstExplicitParameter(MethodDefinition method)
    {
        foreach (var instruction in method.Body.Instructions)
        {
            if (instruction.OpCode.Code == Code.Ldarg_1 || instruction.OpCode.Code == Code.Ldarga_S && instruction.Operand is ParameterDefinition p1 && p1.Index == 0 || instruction.OpCode.Code == Code.Ldarg_S && instruction.Operand is ParameterDefinition p2 && p2.Index == 0 || instruction.OpCode.Code == Code.Ldarg && instruction.Operand is ParameterDefinition p3 && p3.Index == 0 || instruction.OpCode.Code == Code.Ldarga && instruction.Operand is ParameterDefinition p4 && p4.Index == 0) return true;
        }
        return false;
    }

    private static FieldInfo RequireRuntimeInstanceFieldForOwnership(Type runtimeType, string fieldName, int step)
    {
        FieldInfo? field = null;
        for (var type = runtimeType; type is not null && field is null; type = type.BaseType)
            field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
        if (field is null)
            throw new MissingFieldException(runtimeType.FullName, fieldName);
        if (field.IsStatic)
            throw new InvalidDataException($"Step {step}.0 field {runtimeType.FullName}.{fieldName} unexpectedly became static.");
        return field;
    }

    private static MethodInfo RequireRuntimeMethodByTokenForOwnership(Type runtimeType, uint token, string methodName, int step, int parameterCount, string returnTypeFullName)
    {
        var methods = EnumerateRuntimeMethodsForOwnership(runtimeType).Where(method => method.Name == methodName && !method.IsGenericMethod && method.GetParameters().Length == parameterCount && method.ReturnType.FullName == returnTypeFullName && (uint)method.MetadataToken == token).ToArray();
        return methods.SingleOrDefault() ?? throw new MissingMethodException(runtimeType.FullName, $"Step {step}.0 {methodName} token=0x{token:X8}");
    }

    private static IEnumerable<MethodInfo> EnumerateRuntimeMethodsForOwnership(Type type)
    {
        for (var current = type; current is not null; current = current.BaseType) foreach (var method in current.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)) yield return method;
    }

    private static MethodInfo RequireZeroArgRuntimeMethodForOwnership(Type type, string name, int step)
    {
        var candidates = EnumerateRuntimeMethodsForOwnership(type).Where(method => method.Name == name && !method.IsGenericMethod && method.GetParameters().Length == 0).ToArray(); return candidates.SingleOrDefault() ?? throw new MissingMethodException(type.FullName, $"Step {step}.0 {name}()");
    }

    private static string[] GetSelectedManagedNodeTypeNamesForOwnership(IReadOnlyList<Step39NodeObservation> nodes, Assembly selectedAssembly, AssemblyLoadContext context)
        => nodes.Select(item => item.Node.GetType()).Where(type => ReferenceEquals(type.Assembly, selectedAssembly) && ReferenceEquals(AssemblyLoadContext.GetLoadContext(type.Assembly), context)).Select(type => type.FullName).Where(name => !string.IsNullOrWhiteSpace(name)).Cast<string>().Distinct(StringComparer.Ordinal).OrderBy(name => name, StringComparer.Ordinal).ToArray();

    private static string BuildCharacterSelectNodeAuditAppendixForOwnership(string heading, IReadOnlyList<Step39NodeObservation> nodes, IReadOnlyList<string> managedTypes, IReadOnlyList<MethodDefinition> roots, StartupLadderInvocationFrontierAudit audit)
    {
        var lines = new List<string> { string.Empty, "[" + heading + "]", $"Nodes: {nodes.Count}", $"Selected managed node types: {managedTypes.Count}", $"Immediate callback roots: {roots.Count}", $"Invocation-qualified closure methods: {audit.ImmediateClosureMethods.Length}", $"Immediate classified boundaries: {audit.ImmediateBoundaries.Length}", $"Runtime-guarded immediate frontiers: {audit.GuardedMethodFrontiers.Length}", $"Deferred method frontiers: {audit.DeferredMethodFrontiers.Length}", "Forbidden immediate boundaries: 0", "Unresolved same-sts2 references: 0" };
        foreach (var node in nodes) lines.Add($"  node: {node.Path} | {node.Node.GetType().FullName}"); lines.Add("[SELECTED MANAGED TYPES]"); foreach (var type in managedTypes) lines.Add("  - " + type); lines.Add("[CALLBACK ROOTS]"); foreach (var root in roots) lines.Add($"  - token=0x{root.MetadataToken.ToUInt32():X8}; {root.FullName}"); lines.Add(BuildStartupLadderInvocationFrontierAppendix(audit)); return string.Join("\n", lines) + "\n";
    }

    private readonly record struct CharacterSelectRuntimeState(bool Present, bool InsideTree, bool Visible, bool VisibleInTree, bool StackIsNull, bool StackIsExactRetainedStack, string LogicalStackType, bool ParentIsExactStack, string ParentType)
    {
        public static CharacterSelectRuntimeState Absent => new(false, false, false, false, true, false, "<null>", false, "<none>");
        public override string ToString() => Present ? $"present=True; insideTree={InsideTree}; visible={Visible}; visibleInTree={VisibleInTree}; logicalStackNull={StackIsNull}; logicalStackExactRetained={StackIsExactRetainedStack}; logicalStackType={LogicalStackType}; parentExactStack={ParentIsExactStack}; parentType={ParentType}" : "present=False";
    }
}
