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
/// NMainMenuSubmenuStack._Ready preload state. Step 59 is therefore a forensic localization boundary: it records the
/// exact pre-handler scene/service/save/ownership prerequisites, preserves the original inner game exception stack,
/// and records a post-failure mutation snapshot before returning failure. Steps 60-62 remain available only after a
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
    private const string Step59Name = "FORENSIC REAL OPENCHARACTERSELECT FROZEN TRANSITION";
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
    private object? _step58ObservedCharacterSelect;
    private CharacterSelectRuntimeState? _step58ObservedState;
    private bool _step58TransitionAlreadyComplete;
    private object? _step59CharacterSelectScreen;

    public bool ExactStep58ClosurePassed => _exactStep58ClosurePassed;
    public bool ExactStep59ClosurePassed => _exactStep59ClosurePassed;
    public bool ExactStep60ClosurePassed => _exactStep60ClosurePassed;
    public bool ExactStep61ClosurePassed => _exactStep61ClosurePassed;
    public bool ExactStep62ClosurePassed => _exactStep62ClosurePassed;
    public bool Step59TransitionStarted => _step59TransitionStarted;
    public bool Step59HandlerInvocationRequired => !_step58TransitionAlreadyComplete;
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
        if (!_step59TransitionPassed || string.IsNullOrWhiteSpace(_step59StaticMap)) throw new InvalidOperationException("Step 59.0 transition map is incomplete.");
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
    public TransformedRealStS2StartupLadderGateResult RunStep58ClosedStep57Authority(bool renderingStopped, Action<string>? checkpoint = null)
    {
        const int step = 58; const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.PrerequisiteAuthority; var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep58Prerequisite("Step 58 Gate A entry");
            if (!renderingStopped) throw new InvalidOperationException("Step 58.0 is observational and requires rendering frozen.");
            var selected = RequireStartupLadderSelectedAuthority(step);
            _step58Baseline = CaptureStartupLadderBaseline(selected.Path, selected.Sha256, context);
            RequireStartupLadderBaselineUnchanged(context, _step58Baseline, "Step 58 Gate A");
            RequireVisibleSingleplayerSubmenu(step);
            RequireRetainedCharacterSelectPackedSceneForOwnership(step);
            Checkpoint(checkpoint, "M58_A_PASS — Step-57 4/4 retained PackedScene/PCK authority preserved; renderer stopped; runtime character-select ownership may be observed without mutation.");
            return StartupLadderPass(step, Step58Name, gate, "Closed Step-57 authority retained; Step 58 may observe the actual game-owned character-select cache/tree state and re-audit the real handler without invoking it.");
        }
        catch (Exception ex) { Checkpoint(checkpoint, $"M58_A_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}"); return StartupLadderFail(step, Step58Name, gate, stage, ex); }
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
            _step59PreflightSnapshot = CaptureStep59ForensicSnapshot(context, step, requireReadyPrerequisites: true);
            Checkpoint(checkpoint, "M59_B_FORENSIC_PREFLIGHT — " + SanitizeCheckpoint(_step59PreflightSnapshot));
            _step59TransitionBound = true;
            _step59StaticMap = "StS2 Launcher — Step 59.0 game-owned submenu-stack repair + OpenCharacterSelect frozen transition\n" +
                $"NSubmenuStack.Push token: 0x{_step59SubmenuPushToken:X8}; parameter={SubmenuManagedTypeFullName}\n" +
                $"Single-player logical stack before repair: {(logicalStack is null ? "<null>" : logicalStack.GetType().FullName)}\n" +
                $"Navigation repair required: {_step59NavigationRepairRequired}\n" +
                $"OpenCharacterSelect token: 0x{_step58OpenCharacterSelectToken:X8}\n" +
                $"OpenCharacterSelect invocation required: {!_step58TransitionAlreadyComplete}\n" +
                $"Real standard button type: {_step59StandardButton.GetType().FullName}\n" +
                $"Before character-select transition: {before.State}\n" +
                $"Forensic preflight: {_step59PreflightSnapshot}\n" +
                "Rendering active before transition: NO\n" +
                BuildStartupLadderInvocationFrontierAppendix(pushAudit);
            RequireStartupLadderBaselineUnchanged(context, baseline, "Step 59 Gate B");
            Checkpoint(checkpoint, $"M59_B_PASS — exact OpenCharacterSelect and NSubmenuStack.Push rebound; invocationRequired={!_step58TransitionAlreadyComplete}; navigationRepairRequired={_step59NavigationRepairRequired}; singleplayerLogicalStack={(logicalStack is null ? "<null>" : "exact-retained")}; pushToken=0x{_step59SubmenuPushToken:X8}; realStandardButton={SanitizeCheckpoint(_step59StandardButton.GetType().FullName ?? "<unknown>")}; beforeState={SanitizeCheckpoint(before.State.ToString())}.");
            return StartupLadderPass(step, Step59Name, gate, "Exact game-owned stack Push and character-select handler are bound. A missing single-player logical stack is repaired only through NSubmenuStack.Push before the real OpenCharacterSelect handler runs.");
        }
        catch (Exception ex) { Checkpoint(checkpoint, $"M59_B_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}"); return StartupLadderFail(step, Step59Name, gate, stage, ex); }
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
                try { _step58RuntimeOpenCharacterSelect!.Invoke(submenu, new object?[] { _step59StandardButton }); }
                catch (TargetInvocationException tie) when (tie.InnerException is not null)
                {
                    var inner = tie.InnerException;
                    Checkpoint(checkpoint, $"M59_C_HANDLER_INNER_EXCEPTION — type={SanitizeCheckpoint(inner.GetType().FullName ?? inner.GetType().Name)}; targetSite={SanitizeCheckpoint(inner.TargetSite?.ToString() ?? "<null>")}; source={SanitizeCheckpoint(inner.Source ?? "<null>")}; stack={SanitizeCheckpoint(inner.StackTrace ?? "<null>")}");
                    try
                    {
                        _step59FailureSnapshot = CaptureStep59ForensicSnapshot(context, step, requireReadyPrerequisites: false);
                        _step59StaticMap += $"Handler inner exception: {inner.GetType().FullName}: {inner.Message}\nTargetSite: {inner.TargetSite}\nOriginal stack: {inner.StackTrace}\nPost-failure forensic snapshot: {_step59FailureSnapshot}\n";
                        Checkpoint(checkpoint, "M59_C_FORENSIC_POSTFAIL — " + SanitizeCheckpoint(_step59FailureSnapshot));
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
        var context = RequireStep57Prerequisite(boundary); if (!_exactStep57ClosurePassed || !_step57PreflightMapped || !_step57StaticMapDurablyWritten) throw new InvalidOperationException(boundary + " requires same-process Step-57 4/4 durable retained-PackedScene/PCK authority."); return context;
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
        var current = CaptureCurrentCharacterSelectState(step, context);
        var screen = current.Screen ?? throw new InvalidDataException($"Step {step}.0 forensic snapshot requires the cached NCharacterSelectScreen.");
        var screenReady = InvokeRuntimeBoolForForensics(screen, "IsNodeReady", step);
        if (requireReadyPrerequisites && !screenReady)
            throw new InvalidDataException($"Step {step}.0 character-select screen is inside the SceneTree but IsNodeReady() is false; InitializeSingleplayer is not authorized.");

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
                throw new InvalidDataException($"Step {step}.0 character-select IsNodeReady()={screenReady} but required _Ready-bound field {fieldName} is null.");
        }

        var ascensionReady = ascensionPanel is not null && InvokeRuntimeBoolForForensics(ascensionPanel, "IsNodeReady", step);
        if (requireReadyPrerequisites && !ascensionReady)
            throw new InvalidDataException($"Step {step}.0 character-select _ascensionPanel exists but IsNodeReady() is false.");

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
                    throw new InvalidDataException($"Step {step}.0 ascension panel IsNodeReady()={ascensionReady} but required _Ready-bound field {fieldName} is null.");
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
                throw new InvalidDataException($"Step {step}.0 required NGame.{propertyName} is null before InitializeSingleplayer.");
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

        return $"character=[{current.State}; nodeReady={screenReady}; fields={string.Join(",", fieldStates)}]; ascension=[nodeReady={ascensionReady}; fields={string.Join(",", ascensionFieldStates)}]; nGame=[{string.Join(",", nGameStates)}]; save=Progress:{progress.GetType().FullName},Epochs:{epochs.GetType().FullName},EncounterStats:{encounterStats.GetType().FullName}; rootCurrentScene={currentSceneIdentity}; lobby={(lobby is null ? "NULL" : lobby.GetType().FullName)}; lobbyPlayers={playersCount}";
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
