using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Security.Cryptography;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace StS2Launcher.Core;

/// <summary>
/// Steps 53-57 continue only from physically closed Step-52 authority. Step 53 isolates and
/// proves the smallest single-player menu-open frontier. Step 54 invokes only that exact
/// admissible OpenSingleplayerSubmenu() method once while rendering is frozen. Step 55 audits
/// and briefly renders the resulting real NSingleplayerSubmenu before synchronously refreezing.
/// Steps 56-57 map and read-only-preflight the next OpenCharacterSelect boundary without invoking
/// it or loading its candidate scene. Original GameStartup/LaunchMainMenu, Steam initialization,
/// deferred startup and native game extensions remain unopened.
/// </summary>
public sealed partial class TransformedRealStS2VeryEarlyInitialization
{
    public const int Step55SingleplayerRenderTargetMilliseconds = 750;
    public const int Step55SingleplayerRenderEvidenceCeilingMilliseconds = 5_000;

    private const string Step53Name = "SINGLEPLAYER OPEN FRONTIER";
    private const string Step54Name = "SINGLEPLAYER SUBMENU FROZEN OPEN";
    private const string Step55Name = "SINGLEPLAYER SUBMENU RENDER RESIDENCY";
    private const string Step56Name = "CHARACTER SELECT FRONTIER MAP";
    private const string Step57Name = "CHARACTER SELECT RESOURCE PREFLIGHT";
    private const string SingleplayerSubmenuManagedTypeFullName = "MegaCrit.Sts2.Core.Nodes.Screens.MainMenu.NSingleplayerSubmenu";
    private const string MainMenuSubmenuStackManagedTypeFullName = "MegaCrit.Sts2.Core.Nodes.Screens.MainMenu.NMainMenuSubmenuStack";
    private const string MainMenuSubmenuStackPropertyName = "SubmenuStack";
    private const string MainMenuSingleplayerFieldName = "_singleplayerSubmenu";
    private const string MainMenuCharacterSelectSceneFieldName = "_characterSelectScreenScene";
    private const string PackedSceneManagedTypeFullName = "Godot.PackedScene";
    private const string GodotResourcePathPropertyName = "ResourcePath";
    private const string SingleplayerOpenCharacterSelectMethodName = "OpenCharacterSelect";
    private const string SingleplayerOpenCharacterSelectParameterTypeFullName = "MegaCrit.Sts2.Core.Nodes.GodotExtensions.NButton";
    private const string CharacterSelectSceneShortHint = "screens/character_select_screen";
    private const string CharacterSelectSceneResourcePath = "res://scenes/screens/character_select_screen.tscn";

    private StartupLadderBaseline? _step53Baseline;
    private StartupLadderBaseline? _step54Baseline;
    private StartupLadderBaseline? _step54PostOpenBaseline;
    private StartupLadderBaseline? _step55Baseline;
    private StartupLadderBaseline? _step55PostPulseBaseline;
    private StartupLadderBaseline? _step56Baseline;
    private StartupLadderBaseline? _step57Baseline;
    private string _step53StaticMap = string.Empty;
    private string _step54StaticMap = string.Empty;
    private string _step55StaticMap = string.Empty;
    private string _step56StaticMap = string.Empty;
    private string _step57StaticMap = string.Empty;
    private bool _step53OpenAdmissible;
    private bool _step53StaticMapDurablyWritten;
    private bool _step54OpenStarted;
    private bool _step54OpenPassed;
    private bool _step54StaticMapDurablyWritten;
    private bool _step55StaticMapDurablyWritten;
    private bool _step55PulseStarted;
    private bool _step55PulsePassed;
    private bool _step56FrontierMapped;
    private bool _step56StaticMapDurablyWritten;
    private bool _step57PreflightMapped;
    private bool _step57StaticMapDurablyWritten;
    private bool _exactStep53ClosurePassed;
    private bool _exactStep54ClosurePassed;
    private bool _exactStep55ClosurePassed;
    private bool _exactStep56ClosurePassed;
    private bool _exactStep57ClosurePassed;
    private uint _step53OpenMethodToken;
    private uint _step56OpenCharacterSelectToken;
    private object? _step54SubmenuStack;
    private object? _step54SingleplayerSubmenu;
    private bool _step54SubmenuExistedBefore;
    private bool _step54SubmenuInsideTreeBefore;
    private bool _step54SubmenuVisibleBefore;
    private bool _step54SubmenuVisibleInTreeBefore;
    private string[] _step56CharacterSelectSceneCandidates = [];
    private string _step57CharacterSelectResourcePath = string.Empty;

    public bool ExactStep53ClosurePassed => _exactStep53ClosurePassed;
    public bool ExactStep54ClosurePassed => _exactStep54ClosurePassed;
    public bool ExactStep55ClosurePassed => _exactStep55ClosurePassed;
    public bool ExactStep56ClosurePassed => _exactStep56ClosurePassed;
    public bool ExactStep57ClosurePassed => _exactStep57ClosurePassed;
    public bool Step54OpenStarted => _step54OpenStarted;
    public bool Step55PulseStarted => _step55PulseStarted;

    private void ResetSingleplayerContinuationState()
    {
        _step53Baseline = null;
        _step54Baseline = null;
        _step54PostOpenBaseline = null;
        _step55Baseline = null;
        _step55PostPulseBaseline = null;
        _step56Baseline = null;
        _step57Baseline = null;
        _step53StaticMap = string.Empty;
        _step54StaticMap = string.Empty;
        _step55StaticMap = string.Empty;
        _step56StaticMap = string.Empty;
        _step57StaticMap = string.Empty;
        _step53OpenAdmissible = false;
        _step53StaticMapDurablyWritten = false;
        _step54OpenStarted = false;
        _step54OpenPassed = false;
        _step54StaticMapDurablyWritten = false;
        _step55StaticMapDurablyWritten = false;
        _step55PulseStarted = false;
        _step55PulsePassed = false;
        _step56FrontierMapped = false;
        _step56StaticMapDurablyWritten = false;
        _step57PreflightMapped = false;
        _step57StaticMapDurablyWritten = false;
        _exactStep53ClosurePassed = false;
        _exactStep54ClosurePassed = false;
        _exactStep55ClosurePassed = false;
        _exactStep56ClosurePassed = false;
        _exactStep57ClosurePassed = false;
        _step53OpenMethodToken = 0;
        _step56OpenCharacterSelectToken = 0;
        _step54SubmenuStack = null;
        _step54SingleplayerSubmenu = null;
        _step54SubmenuExistedBefore = false;
        _step54SubmenuInsideTreeBefore = false;
        _step54SubmenuVisibleBefore = false;
        _step54SubmenuVisibleInTreeBefore = false;
        _step56CharacterSelectSceneCandidates = [];
        _step57CharacterSelectResourcePath = string.Empty;
        ResetCharacterSelectContinuationState();
    }

    public string GetVerifiedSingleplayerContinuationStaticMap(int step)
        => step switch
        {
            53 when !string.IsNullOrWhiteSpace(_step53StaticMap) => _step53StaticMap,
            54 when !string.IsNullOrWhiteSpace(_step54StaticMap) => _step54StaticMap,
            55 when !string.IsNullOrWhiteSpace(_step55StaticMap) => _step55StaticMap,
            56 when !string.IsNullOrWhiteSpace(_step56StaticMap) => _step56StaticMap,
            57 when !string.IsNullOrWhiteSpace(_step57StaticMap) => _step57StaticMap,
            _ => throw new InvalidOperationException($"Step {step}.0 has not produced a verified single-player continuation static map."),
        };

    public void MarkStep53StaticMapDurablyWritten()
    {
        if (!_step53OpenAdmissible || string.IsNullOrWhiteSpace(_step53StaticMap))
            throw new InvalidOperationException("Step 53.0 static map cannot be marked durable before exact OpenSingleplayerSubmenu admissibility is established.");
        _step53StaticMapDurablyWritten = true;
    }

    public void MarkStep54StaticMapDurablyWritten()
    {
        if (string.IsNullOrWhiteSpace(_step54StaticMap))
            throw new InvalidOperationException("Step 54.0 runtime binding map is absent.");
        _step54StaticMapDurablyWritten = true;
    }

    public void MarkStep55StaticMapDurablyWritten()
    {
        if (string.IsNullOrWhiteSpace(_step55StaticMap))
            throw new InvalidOperationException("Step 55.0 submenu frame/input map is absent.");
        _step55StaticMapDurablyWritten = true;
    }

    public void MarkStep56StaticMapDurablyWritten()
    {
        if (!_step56FrontierMapped || string.IsNullOrWhiteSpace(_step56StaticMap))
            throw new InvalidOperationException("Step 56.0 character-select frontier map is incomplete.");
        _step56StaticMapDurablyWritten = true;
    }

    public void MarkStep57StaticMapDurablyWritten()
    {
        if (!_step57PreflightMapped || string.IsNullOrWhiteSpace(_step57StaticMap))
            throw new InvalidOperationException("Step 57.0 character-select resource preflight map is incomplete.");
        _step57StaticMapDurablyWritten = true;
    }

    // STEP 53 — isolate the exact direct OpenSingleplayerSubmenu frontier and require it to be admissible.

    public TransformedRealStS2StartupLadderGateResult RunStep53ClosedStep52Authority(bool renderingStopped, Action<string>? checkpoint = null)
    {
        const int step = 53;
        const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.PrerequisiteAuthority;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep53Prerequisite("Step 53 Gate A entry");
            if (!renderingStopped)
                throw new InvalidOperationException("Step 53.0 requires Step 52 to have synchronously refrozen rendering.");
            var selected = RequireStartupLadderSelectedAuthority(step);
            _step53Baseline = CaptureStartupLadderBaseline(selected.Path, selected.Sha256, context);
            RequireStartupLadderBaselineUnchanged(context, _step53Baseline, "Step 53 Gate A");
            RequireStep48LifecycleRuntimeGuardsCurrent(context, "Step 53 Gate A guard recheck", requirePreAdmissionCounts: false);
            RequireRetainedInTreeMainMenu(step);
            Checkpoint(checkpoint, "M53_A_PASS — Step-52 4/4 sustained main-menu authority retained; NMainMenu in-tree; renderingStopped=True; no single-player handler invoked.");
            return StartupLadderPass(step, Step53Name, gate,
                "Physically closed Step-52 authority retained with exact in-tree NMainMenu and frozen rendering. The narrow OpenSingleplayerSubmenu frontier may be isolated without invocation.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"M53_A_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return StartupLadderFail(step, Step53Name, gate, stage, ex);
        }
    }

    public TransformedRealStS2StartupLadderGateResult RunStep53SingleplayerOpenBinding(Action<string>? checkpoint = null)
    {
        const int step = 53;
        const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.StaticAuditOrBinding;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep53Prerequisite("Step 53 Gate B entry");
            var baseline = _step53Baseline ?? throw new InvalidOperationException("Step 53.0 Gate A must pass before Gate B.");
            using var resolver = new RejectingAssemblyResolver();
            using var module = OpenStartupLadderModule(baseline.SelectedPath, resolver);
            var allTypes = EnumerateTypes(module.Types).ToDictionary(type => type.FullName, StringComparer.Ordinal);
            var mainMenu = RequireStartupLadderType(allTypes, MainMenuManagedRootTypeFullName, step);
            var open = RequireUniqueStartupLadderNamedMethod(mainMenu, MainMenuOpenSingleplayerSubmenuMethodName, step);
            var pressed = RequireUniqueStartupLadderNamedMethod(mainMenu, MainMenuSingleplayerButtonPressedMethodName, step);
            if (open.Parameters.Count != 0 || open.ReturnType.FullName != SingleplayerSubmenuManagedTypeFullName)
                throw new InvalidDataException($"Step 53.0 requires zero-arg OpenSingleplayerSubmenu returning exact {SingleplayerSubmenuManagedTypeFullName}; observed params={open.Parameters.Count}; return={open.ReturnType.FullName}.");
            if (resolver.Requests.Count != 0)
                throw new InvalidDataException("Step 53.0 binding unexpectedly attempted external Cecil resolution: " + string.Join(" | ", resolver.Requests));
            _step53OpenMethodToken = open.MetadataToken.ToUInt32();
            _step53StaticMap =
                "StS2 Launcher — Step 53.0 exact single-player submenu-open frontier\n" +
                "No menu handler is invoked by this map.\n" +
                $"Selected compatibility SHA-256: {baseline.SelectedSha256}\n" +
                $"OpenSingleplayerSubmenu token=0x{_step53OpenMethodToken:X8}; params={open.Parameters.Count}; return={open.ReturnType.FullName}; IL={open.Body.Instructions.Count}\n" +
                $"SingleplayerButtonPressed token=0x{pressed.MetadataToken.ToUInt32():X8}; params={pressed.Parameters.Count}; return={pressed.ReturnType.FullName}; IL={pressed.Body.Instructions.Count}\n" +
                "Policy: only OpenSingleplayerSubmenu is audited for Step-54 invocation. SingleplayerButtonPressed remains uninvoked/evidence-only.\n" +
                "Rendering restarted: NO\nHandler invocation: NO\n" +
                "[OPENSINGLEPLAYERSUBMENU IL]\n" + string.Join("\n", open.Body.Instructions.Select(instruction => "  " + FormatStep41Instruction(instruction))) + "\n";
            RequireStartupLadderBaselineUnchanged(context, baseline, "Step 53 Gate B");
            Checkpoint(checkpoint, $"M53_B_PASS — exact OpenSingleplayerSubmenu bound; token=0x{_step53OpenMethodToken:X8}; IL={open.Body.Instructions.Count}; SingleplayerButtonPressed remains evidence-only; externalResolution=0; invocation=NO.");
            return StartupLadderPass(step, Step53Name, gate,
                $"Exact zero-arg OpenSingleplayerSubmenu returning {SingleplayerSubmenuManagedTypeFullName} was bound and its IL recorded without invocation. SingleplayerButtonPressed remains unopened.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"M53_B_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return StartupLadderFail(step, Step53Name, gate, stage, ex);
        }
    }

    public TransformedRealStS2StartupLadderGateResult RunStep53SingleplayerOpenFrontierAudit(Action<string>? checkpoint = null)
    {
        const int step = 53;
        const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.ControlledAction;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep53Prerequisite("Step 53 Gate C entry");
            var baseline = _step53Baseline ?? throw new InvalidOperationException("Step 53.0 baseline absent.");
            using var resolver = new RejectingAssemblyResolver();
            using var module = OpenStartupLadderModule(baseline.SelectedPath, resolver);
            var allTypes = EnumerateTypes(module.Types).ToDictionary(type => type.FullName, StringComparer.Ordinal);
            var allMethods = BuildStartupLadderMethodMap(allTypes);
            var mainMenu = RequireStartupLadderType(allTypes, MainMenuManagedRootTypeFullName, step);
            var open = RequireUniqueStartupLadderNamedMethod(mainMenu, MainMenuOpenSingleplayerSubmenuMethodName, step);
            if (open.MetadataToken.ToUInt32() != _step53OpenMethodToken)
                throw new InvalidDataException("Step 53.0 OpenSingleplayerSubmenu metadata token drifted between Gate B and Gate C.");
            var guards = _step48LifecycleGuardAuthority ?? throw new InvalidOperationException("Step 53.0 requires retained Step-48 runtime guards.");
            var audit = AuditStartupLadderInvocationFrontier([open], allTypes, allMethods, guards);
            RequireImmediateFrontierAdmissible(audit, "Step 53.0 exact OpenSingleplayerSubmenu immediate frontier under retained runtime guards");
            if (resolver.Requests.Count != 0)
                throw new InvalidDataException("Step 53.0 frontier audit unexpectedly attempted external Cecil resolution: " + string.Join(" | ", resolver.Requests));
            _step53StaticMap += BuildStartupLadderInvocationFrontierAppendix(audit);
            _step53OpenAdmissible = true;
            RequireStartupLadderBaselineUnchanged(context, baseline, "Step 53 Gate C");
            Checkpoint(checkpoint, $"M53_C_PASS — exact OpenSingleplayerSubmenu frontier admissible; closureMethods={audit.ImmediateClosureMethods.Length}; immediateBoundaryRefs={audit.ImmediateBoundaries.Length}; guardedFrontiers={audit.GuardedMethodFrontiers.Length}; deferredFrontiers={audit.DeferredMethodFrontiers.Length}; forbidden/unresolved/external=0; invocation=NO.");
            return StartupLadderPass(step, Step53Name, gate,
                $"Exact OpenSingleplayerSubmenu frontier is admissible under retained guards: immediate methods={audit.ImmediateClosureMethods.Length}; forbidden/unresolved/external=0. No handler was invoked.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"M53_C_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return StartupLadderFail(step, Step53Name, gate, stage, ex);
        }
    }

    public TransformedRealStS2StartupLadderGateResult RunStep53FrozenNoInvocationConfinement(bool renderingStopped, Action<string>? checkpoint = null)
    {
        const int step = 53;
        const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.PostActionConfinement;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep53Prerequisite("Step 53 Gate D entry");
            var baseline = _step53Baseline ?? throw new InvalidOperationException("Step 53.0 baseline absent.");
            if (!_step53OpenAdmissible || !_step53StaticMapDurablyWritten)
                throw new InvalidOperationException("Step 53.0 requires durable admissible OpenSingleplayerSubmenu evidence before Gate D.");
            if (!renderingStopped)
                throw new InvalidOperationException("Step 53.0 is non-invoking and requires rendering frozen.");
            RequireStartupLadderBaselineUnchanged(context, baseline, "Step 53 Gate D");
            RequireStep48LifecycleRuntimeGuardsCurrent(context, "Step 53 Gate D guard recheck", requirePreAdmissionCounts: false);
            RequireRetainedInTreeMainMenu(step);
            _exactStep53ClosurePassed = true;
            Checkpoint(checkpoint, "M53_D_PASS — admissible OpenSingleplayerSubmenu map durable; invocation=NO; renderingStopped=True; context/native drift=0.");
            return StartupLadderPass(step, Step53Name, gate,
                "Durable admissible single-player submenu-open authority closed without invocation. Step 54 may invoke only that exact method once while frozen.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"M53_D_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return StartupLadderFail(step, Step53Name, gate, stage, ex);
        }
    }

    // STEP 54 — invoke only exact OpenSingleplayerSubmenu once with rendering frozen.

    public TransformedRealStS2StartupLadderGateResult RunStep54ClosedStep53Authority(bool renderingStopped, Action<string>? checkpoint = null)
    {
        const int step = 54;
        const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.PrerequisiteAuthority;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep54Prerequisite("Step 54 Gate A entry");
            if (!renderingStopped)
                throw new InvalidOperationException("Step 54.0 requires frozen rendering before the one-shot submenu open.");
            var selected = RequireStartupLadderSelectedAuthority(step);
            _step54Baseline = CaptureStartupLadderBaseline(selected.Path, selected.Sha256, context);
            RequireStartupLadderBaselineUnchanged(context, _step54Baseline, "Step 54 Gate A");
            RequireStep48LifecycleRuntimeGuardsCurrent(context, "Step 54 Gate A guard recheck", requirePreAdmissionCounts: false);
            RequireRetainedInTreeMainMenu(step);
            Checkpoint(checkpoint, "M54_A_PASS — Step-53 admissible exact OpenSingleplayerSubmenu authority retained; renderingStopped=True; one-shot open not armed.");
            return StartupLadderPass(step, Step54Name, gate,
                "Step-53 exact admissible submenu-open frontier retained. Runtime method/submenu identity may now be bound before one frozen invocation.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"M54_A_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return StartupLadderFail(step, Step54Name, gate, stage, ex);
        }
    }

    public TransformedRealStS2StartupLadderGateResult RunStep54SingleplayerSubmenuRuntimeBinding(Action<string>? checkpoint = null)
    {
        const int step = 54;
        const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.StaticAuditOrBinding;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep54Prerequisite("Step 54 Gate B entry");
            var baseline = _step54Baseline ?? throw new InvalidOperationException("Step 54.0 Gate A must pass before Gate B.");
            var menu = RequireRetainedInTreeMainMenu(step);
            var open = RequireRuntimeDeclaredZeroArgSingleplayerSubmenuMethod(menu.GetType(), step);
            if (unchecked((uint)open.MetadataToken) != _step53OpenMethodToken)
                throw new InvalidDataException($"Step 54.0 runtime OpenSingleplayerSubmenu token drifted: expected=0x{_step53OpenMethodToken:X8}; actual=0x{unchecked((uint)open.MetadataToken):X8}.");

            // The real game owns _singleplayerSubmenu on NMainMenuSubmenuStack, not NMainMenu.
            // The stack lazily spawns submenu instances, so Gate B binds the exact property/field
            // metadata and records any pre-existing value without forcing one to exist.
            var submenuStack = RequireRuntimeExactObjectProperty(menu, MainMenuSubmenuStackPropertyName, MainMenuSubmenuStackManagedTypeFullName, step);
            var field = RequireRuntimeExactInstanceField(submenuStack.GetType(), MainMenuSingleplayerFieldName, SingleplayerSubmenuManagedTypeFullName, step);
            var submenu = field.GetValue(submenuStack);
            if (submenu is not null && submenu.GetType().FullName != SingleplayerSubmenuManagedTypeFullName)
                throw new InvalidDataException($"Step 54.0 runtime submenu type drifted: {submenu.GetType().FullName}.");

            _step54SubmenuStack = submenuStack;
            _step54SingleplayerSubmenu = submenu;
            _step54SubmenuExistedBefore = submenu is not null;
            _step54SubmenuInsideTreeBefore = submenu is not null && Convert.ToBoolean(RequireZeroArgBoolMethod(submenu.GetType(), "IsInsideTree").Invoke(submenu, null), System.Globalization.CultureInfo.InvariantCulture);
            _step54SubmenuVisibleBefore = submenu is not null && RequireRuntimeBoolProperty(submenu, "Visible", step);
            _step54SubmenuVisibleInTreeBefore = submenu is not null && Convert.ToBoolean(RequireZeroArgBoolMethod(submenu.GetType(), "IsVisibleInTree").Invoke(submenu, null), System.Globalization.CultureInfo.InvariantCulture);
            _step54StaticMap =
                "StS2 Launcher — Step 54.0 frozen single-player submenu runtime binding map\n" +
                $"Selected compatibility SHA-256: {baseline.SelectedSha256}\n" +
                $"OpenSingleplayerSubmenu runtime token=0x{unchecked((uint)open.MetadataToken):X8}; exact Step-53 token match=True\n" +
                $"NMainMenu property: {MainMenuSubmenuStackPropertyName}; propertyType={submenuStack.GetType().FullName}\n" +
                $"NMainMenuSubmenuStack field: {MainMenuSingleplayerFieldName}; fieldType={field.FieldType.FullName}\n" +
                $"Submenu existed before open: {_step54SubmenuExistedBefore}\n" +
                $"Submenu inside tree before open: {_step54SubmenuInsideTreeBefore}\n" +
                $"Submenu Visible before open: {_step54SubmenuVisibleBefore}\n" +
                $"Submenu IsVisibleInTree before open: {_step54SubmenuVisibleInTreeBefore}\n" +
                "SingleplayerButtonPressed invoked: NO\nOpenSingleplayerSubmenu invoked: NO\nRendering restarted: NO\n";
            RequireStartupLadderBaselineUnchanged(context, baseline, "Step 54 Gate B");
            Checkpoint(checkpoint, $"M54_B_PASS — exact runtime OpenSingleplayerSubmenu token matched 0x{_step53OpenMethodToken:X8}; NMainMenu.SubmenuStack -> NMainMenuSubmenuStack._singleplayerSubmenu metadata bound; preexistingSubmenu={_step54SubmenuExistedBefore}; insideTree={_step54SubmenuInsideTreeBefore}; visible={_step54SubmenuVisibleBefore}; visibleInTree={_step54SubmenuVisibleInTreeBefore}; invocation=NO.");
            return StartupLadderPass(step, Step54Name, gate,
                "Exact runtime OpenSingleplayerSubmenu plus the real SubmenuStack lazy _singleplayerSubmenu slot were bound with Step-53 token identity; no invocation or rendering yet.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"M54_B_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return StartupLadderFail(step, Step54Name, gate, stage, ex);
        }
    }

    public TransformedRealStS2StartupLadderGateResult RunStep54ControlledSingleplayerSubmenuOpen(Action<string>? checkpoint = null)
    {
        const int step = 54;
        const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.ControlledAction;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep54Prerequisite("Step 54 Gate C entry");
            var baseline = _step54Baseline ?? throw new InvalidOperationException("Step 54.0 baseline absent.");
            if (!_step54StaticMapDurablyWritten)
                throw new InvalidOperationException("Step 54.0 runtime binding map must be durable before invocation.");
            if (_step54OpenStarted)
                throw new InvalidOperationException("Step 54.0 OpenSingleplayerSubmenu is one-shot in-process.");
            var menu = RequireRetainedInTreeMainMenu(step);
            var open = RequireRuntimeDeclaredZeroArgSingleplayerSubmenuMethod(menu.GetType(), step);
            if (unchecked((uint)open.MetadataToken) != _step53OpenMethodToken)
                throw new InvalidDataException("Step 54.0 OpenSingleplayerSubmenu runtime token changed before invocation.");

            var submenuStack = RequireRuntimeExactObjectProperty(menu, MainMenuSubmenuStackPropertyName, MainMenuSubmenuStackManagedTypeFullName, step);
            if (!ReferenceEquals(submenuStack, _step54SubmenuStack))
                throw new InvalidDataException("Step 54.0 NMainMenu.SubmenuStack identity changed between Gate B and Gate C.");
            var field = RequireRuntimeExactInstanceField(submenuStack.GetType(), MainMenuSingleplayerFieldName, SingleplayerSubmenuManagedTypeFullName, step);
            var fieldBefore = field.GetValue(submenuStack);
            if (!ReferenceEquals(fieldBefore, _step54SingleplayerSubmenu))
                throw new InvalidDataException("Step 54.0 NMainMenuSubmenuStack._singleplayerSubmenu identity changed before the one-shot open.");

            RequireStartupLadderBaselineUnchanged(context, baseline, "Step 54 Gate C preinvoke");
            RequireStep48LifecycleRuntimeGuardsCurrent(context, "Step 54 Gate C guard recheck", requirePreAdmissionCounts: false);
            _step54OpenStarted = true;
            Checkpoint(checkpoint, $"M54_C_INVOKE_START — invoking exact NMainMenu.OpenSingleplayerSubmenu token=0x{_step53OpenMethodToken:X8} once while rendering remains frozen; preexistingSubmenu={_step54SubmenuExistedBefore}; SingleplayerButtonPressed remains uninvoked.");
            object? returnedSubmenu;
            try
            {
                returnedSubmenu = open.Invoke(menu, null);
            }
            catch (TargetInvocationException tie) when (tie.InnerException is not null)
            {
                ExceptionDispatchInfo.Capture(tie.InnerException).Throw();
                throw; // unreachable; satisfies flow analysis
            }

            if (returnedSubmenu is null)
                throw new InvalidDataException("Step 54.0 OpenSingleplayerSubmenu returned null.");
            if (returnedSubmenu.GetType().FullName != SingleplayerSubmenuManagedTypeFullName)
                throw new InvalidDataException($"Step 54.0 OpenSingleplayerSubmenu runtime return type drifted: {returnedSubmenu.GetType().FullName}.");
            var fieldAfter = field.GetValue(submenuStack) ?? throw new InvalidDataException("Step 54.0 NMainMenuSubmenuStack._singleplayerSubmenu remained null after OpenSingleplayerSubmenu returned.");
            if (fieldAfter.GetType().FullName != SingleplayerSubmenuManagedTypeFullName)
                throw new InvalidDataException($"Step 54.0 post-open submenu field type drifted: {fieldAfter.GetType().FullName}.");
            if (!ReferenceEquals(returnedSubmenu, fieldAfter))
                throw new InvalidDataException("Step 54.0 OpenSingleplayerSubmenu return identity did not match NMainMenu.SubmenuStack._singleplayerSubmenu.");
            if (_step54SubmenuExistedBefore && !ReferenceEquals(fieldAfter, _step54SingleplayerSubmenu))
                throw new InvalidDataException("Step 54.0 pre-existing NMainMenuSubmenuStack._singleplayerSubmenu identity changed across open invocation.");

            _step54SingleplayerSubmenu = fieldAfter;
            var submenu = fieldAfter;
            var inside = Convert.ToBoolean(RequireZeroArgBoolMethod(submenu.GetType(), "IsInsideTree").Invoke(submenu, null), System.Globalization.CultureInfo.InvariantCulture);
            var visible = RequireRuntimeBoolProperty(submenu, "Visible", step);
            var visibleInTree = Convert.ToBoolean(RequireZeroArgBoolMethod(submenu.GetType(), "IsVisibleInTree").Invoke(submenu, null), System.Globalization.CultureInfo.InvariantCulture);
            if (!inside || !visible || !visibleInTree)
                throw new InvalidDataException($"Step 54.0 submenu did not become an active visible in-tree authority. inside={inside}; visible={visible}; visibleInTree={visibleInTree}.");
            if (_step54SubmenuVisibleInTreeBefore && _step54SubmenuVisibleBefore)
                throw new InvalidDataException("Step 54.0 submenu was already fully visible before the one-shot open; no transition authority can be established.");
            RequireStartupLadderBaselineUnchanged(context, baseline, "Step 54 Gate C postinvoke");
            RequireStep48LifecycleRuntimeGuardsCurrent(context, "Step 54 Gate C postinvoke guard recheck", requirePreAdmissionCounts: false);
            _step54PostOpenBaseline = CaptureStartupLadderBaseline(baseline.SelectedPath, baseline.SelectedSha256, context);
            _step54OpenPassed = true;
            Checkpoint(checkpoint, $"M54_C_PASS — exact OpenSingleplayerSubmenu returned once; return identity matches NMainMenu.SubmenuStack._singleplayerSubmenu; lazilyCreated={!_step54SubmenuExistedBefore}; NSingleplayerSubmenu insideTree={inside}; visible={visible}; visibleInTree={visibleInTree}; renderingStopped=True; resolver/host/private/initializer/rejected/native drift=0.");
            return StartupLadderPass(step, Step54Name, gate,
                "Exact OpenSingleplayerSubmenu returned once while frozen and its returned object is the real SubmenuStack-retained NSingleplayerSubmenu, visible in-tree with zero context/native drift.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"M54_C_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return StartupLadderFail(step, Step54Name, gate, stage, ex);
        }
    }

    public TransformedRealStS2StartupLadderGateResult RunStep54FrozenOpenConfinement(bool renderingStopped, Action<string>? checkpoint = null)
    {
        const int step = 54;
        const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.PostActionConfinement;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep54Prerequisite("Step 54 Gate D entry");
            if (!_step54OpenPassed || !_step54OpenStarted || !renderingStopped)
                throw new InvalidOperationException("Step 54.0 Gate D requires successful one-shot submenu open with rendering frozen.");
            var post = _step54PostOpenBaseline ?? throw new InvalidOperationException("Step 54.0 post-open baseline absent.");
            RequireStartupLadderBaselineUnchanged(context, post, "Step 54 Gate D");
            RequireStep48LifecycleRuntimeGuardsCurrent(context, "Step 54 Gate D guard recheck", requirePreAdmissionCounts: false);
            var submenu = _step54SingleplayerSubmenu ?? throw new InvalidOperationException("Step 54.0 NSingleplayerSubmenu absent.");
            if (RequireZeroArgBoolMethod(submenu.GetType(), "IsInsideTree").Invoke(submenu, null) is not true ||
                RequireZeroArgBoolMethod(submenu.GetType(), "IsVisibleInTree").Invoke(submenu, null) is not true)
                throw new InvalidDataException("Step 54.0 NSingleplayerSubmenu is not retained visible/in-tree at Gate D.");
            _exactStep54ClosurePassed = true;
            Checkpoint(checkpoint, "M54_D_PASS — real NSingleplayerSubmenu retained visible/in-tree; renderingStopped=True; exact open one-shot closed; post-open context/native drift=0.");
            return StartupLadderPass(step, Step54Name, gate,
                "Frozen single-player submenu open closed 4/4 with retained real visible/in-tree NSingleplayerSubmenu and zero post-open drift.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"M54_D_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return StartupLadderFail(step, Step54Name, gate, stage, ex);
        }
    }

    // STEP 55 — audit actual submenu callbacks, then run one bounded render residency and refreeze.

    public TransformedRealStS2StartupLadderGateResult RunStep55ClosedStep54Authority(bool renderingStopped, Action<string>? checkpoint = null)
    {
        const int step = 55;
        const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.PrerequisiteAuthority;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep55Prerequisite("Step 55 Gate A entry");
            if (!renderingStopped)
                throw new InvalidOperationException("Step 55.0 requires rendering frozen before submenu render residency.");
            var selected = RequireStartupLadderSelectedAuthority(step);
            _step55Baseline = CaptureStartupLadderBaseline(selected.Path, selected.Sha256, context);
            RequireStartupLadderBaselineUnchanged(context, _step55Baseline, "Step 55 Gate A");
            RequireVisibleSingleplayerSubmenu(step);
            Checkpoint(checkpoint, $"M55_A_PASS — Step-54 4/4 real NSingleplayerSubmenu authority retained; renderingStopped=True; target={Step55SingleplayerRenderTargetMilliseconds}ms; pulse not armed.");
            return StartupLadderPass(step, Step55Name, gate,
                "Real visible/in-tree NSingleplayerSubmenu authority retained while frozen. Its actual frame/input surface may now be audited before one bounded render residency.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"M55_A_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return StartupLadderFail(step, Step55Name, gate, stage, ex);
        }
    }

    public TransformedRealStS2StartupLadderGateResult RunStep55SingleplayerSubmenuFrameInputAudit(Action<string>? checkpoint = null)
    {
        const int step = 55;
        const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.StaticAuditOrBinding;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep55Prerequisite("Step 55 Gate B entry");
            var baseline = _step55Baseline ?? throw new InvalidOperationException("Step 55.0 Gate A must pass before Gate B.");
            var submenu = RequireVisibleSingleplayerSubmenu(step);
            var godotAssembly = (_callbackHandoff ?? throw new InvalidOperationException("GodotSharp handoff absent.")).GodotSharpAssembly;
            var nodeType = godotAssembly.GetType("Godot.Node", true, false)!;
            var nodes = EnumerateStartupLadderMenuNodeGraph(submenu, nodeType, "Step 55.0 single-player submenu frame/input audit");
            var managedTypes = nodes.Select(item => item.Node.GetType())
                .Where(type => ReferenceEquals(type.Assembly, submenu.GetType().Assembly))
                .Select(type => type.FullName)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Cast<string>()
                .Distinct(StringComparer.Ordinal)
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToArray();
            using var resolver = new RejectingAssemblyResolver();
            using var module = OpenStartupLadderModule(baseline.SelectedPath, resolver);
            var allTypes = EnumerateTypes(module.Types).ToDictionary(type => type.FullName, StringComparer.Ordinal);
            var allMethods = BuildStartupLadderMethodMap(allTypes);
            var roots = CollectStartupLadderActualNodeCallbackRoots(managedTypes, allTypes, Step40ImmediateFrameMethodNames);
            var guards = _step48LifecycleGuardAuthority ?? throw new InvalidOperationException("Step 55.0 requires retained Step-48 runtime guards.");
            var audit = AuditStartupLadderInvocationFrontier(roots, allTypes, allMethods, guards);
            RequireImmediateFrontierAdmissible(audit, "Step 55.0 actual single-player submenu frame/input frontier under retained runtime guards");
            if (resolver.Requests.Count != 0)
                throw new InvalidDataException("Step 55.0 submenu frame/input audit attempted external Cecil resolution: " + string.Join(" | ", resolver.Requests));
            RequireStartupLadderBaselineUnchanged(context, baseline, "Step 55 Gate B");
            _step55StaticMap =
                "StS2 Launcher — Step 55.0 single-player submenu render residency static map\n" +
                $"Selected compatibility SHA-256: {baseline.SelectedSha256}\n" +
                $"Actual submenu nodes: {nodes.Count}\n" +
                $"Selected managed node types: {managedTypes.Length}\n" +
                $"Immediate frame/input roots: {roots.Length}\n" +
                $"Invocation-qualified closure methods: {audit.ImmediateClosureMethods.Length}\n" +
                $"Immediate classified boundaries: {audit.ImmediateBoundaries.Length}\n" +
                $"Runtime-guarded immediate frontiers: {audit.GuardedMethodFrontiers.Length}\n" +
                $"Deferred method frontiers: {audit.DeferredMethodFrontiers.Length}\n" +
                $"Requested render residency: {Step55SingleplayerRenderTargetMilliseconds} ms\n" +
                $"Evidence ceiling after synchronous stop: {Step55SingleplayerRenderEvidenceCeilingMilliseconds} ms\n" +
                "Forbidden immediate boundaries: 0\nUnresolved same-sts2 references: 0\nRendering restarted while map built: NO\n" +
                BuildStartupLadderInvocationFrontierAppendix(audit);
            Checkpoint(checkpoint, $"M55_B_PASS — submenuNodes={nodes.Count}; managedTypes={managedTypes.Length}; callbacks={roots.Length}; closure={audit.ImmediateClosureMethods.Length}; immediateBoundaries={audit.ImmediateBoundaries.Length}; guardedFrontiers={audit.GuardedMethodFrontiers.Length}; deferred={audit.DeferredMethodFrontiers.Length}; forbidden/unresolved/external=0; renderingStopped=True.");
            return StartupLadderPass(step, Step55Name, gate,
                $"Actual single-player submenu frame/input surface is admissible: nodes={nodes.Count}; callbacks={roots.Length}; closure={audit.ImmediateClosureMethods.Length}; forbidden/unresolved/external=0.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"M55_B_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return StartupLadderFail(step, Step55Name, gate, stage, ex);
        }
    }

    public void BeginStep55SingleplayerRenderPulse(Action<string>? checkpoint = null)
    {
        ThrowIfDisposed();
        RequireStep55Prerequisite("Step 55 Gate C pulse start");
        if (!_step55StaticMapDurablyWritten || string.IsNullOrWhiteSpace(_step55StaticMap))
            throw new InvalidOperationException("Step 55.0 requires durable submenu frame/input map before rendering starts.");
        if (_step55PulseStarted)
            throw new InvalidOperationException("Step 55.0 submenu render pulse is one-shot in-process.");
        _step55PulseStarted = true;
        Checkpoint(checkpoint, $"M55_C_PULSE_ARMED — first/only single-player submenu render pulse authorized; requested stop delay={Step55SingleplayerRenderTargetMilliseconds}ms; evidence ceiling={Step55SingleplayerRenderEvidenceCeilingMilliseconds}ms. First managed continuation must StopRendering before telemetry.");
    }

    public TransformedRealStS2StartupLadderGateResult RunStep55SingleplayerRenderPulseEvidence(bool startReturned, bool activeAfterStart, bool stopReturned, bool activeAfterStop, double elapsedMilliseconds, Action<string>? checkpoint = null)
    {
        const int step = 55;
        const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.ControlledAction;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep55Prerequisite("Step 55 Gate C evidence");
            var baseline = _step55Baseline ?? throw new InvalidOperationException("Step 55.0 baseline absent.");
            if (!_step55PulseStarted || !_step55StaticMapDurablyWritten)
                throw new InvalidOperationException("Step 55.0 render pulse was not properly armed after durable map authority.");
            if (!startReturned || !activeAfterStart || !stopReturned || activeAfterStop)
                throw new InvalidOperationException($"Step 55.0 renderer transition failed. startReturned={startReturned}; activeAfterStart={activeAfterStart}; stopReturned={stopReturned}; activeAfterStop={activeAfterStop}.");
            if (elapsedMilliseconds < Step55SingleplayerRenderTargetMilliseconds || elapsedMilliseconds > Step55SingleplayerRenderEvidenceCeilingMilliseconds)
                throw new InvalidOperationException($"Step 55.0 post-stop evidence outside accepted window. target={Step55SingleplayerRenderTargetMilliseconds}; ceiling={Step55SingleplayerRenderEvidenceCeilingMilliseconds}; observed={elapsedMilliseconds:F1}.");
            RequireVisibleSingleplayerSubmenu(step);
            RequireNoInitializerRejectedNativeEscape(context, baseline, "Step 55 Gate C");
            _step55PostPulseBaseline = CaptureStartupLadderBaseline(baseline.SelectedPath, baseline.SelectedSha256, context);
            _step55PulsePassed = true;
            Checkpoint(checkpoint, $"M55_C_PASS — single-player submenu rendered/refroze; observedElapsedMs={elapsedMilliseconds:F1}; activeAfterStop=False; initializer/rejected/native deltas=0.");
            return StartupLadderPass(step, Step55Name, gate,
                $"Real single-player submenu rendered for the bounded window and synchronously refroze. Observed={elapsedMilliseconds:F1} ms; no initializer/rejected/native escape.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"M55_C_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return StartupLadderFail(step, Step55Name, gate, stage, ex);
        }
    }

    public TransformedRealStS2StartupLadderGateResult RunStep55FrozenPostResidencyConfinement(bool renderingStopped, Action<string>? checkpoint = null)
    {
        const int step = 55;
        const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.PostActionConfinement;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep55Prerequisite("Step 55 Gate D entry");
            if (!_step55PulsePassed || !renderingStopped)
                throw new InvalidOperationException("Step 55.0 Gate D requires successful pulse and synchronous refreeze.");
            var post = _step55PostPulseBaseline ?? throw new InvalidOperationException("Step 55.0 post-pulse baseline absent.");
            RequireStartupLadderBaselineUnchanged(context, post, "Step 55 Gate D");
            RequireStep48LifecycleRuntimeGuardsCurrent(context, "Step 55 Gate D guard recheck", requirePreAdmissionCounts: false);
            RequireVisibleSingleplayerSubmenu(step);
            _exactStep55ClosurePassed = true;
            Checkpoint(checkpoint, "M55_D_PASS — real NSingleplayerSubmenu retained visible/in-tree after bounded render residency; renderingStopped=True; post-pulse drift=0.");
            return StartupLadderPass(step, Step55Name, gate,
                "Single-player submenu render residency closed 4/4 with synchronous refreeze and retained real visible/in-tree submenu authority.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"M55_D_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return StartupLadderFail(step, Step55Name, gate, stage, ex);
        }
    }

    // STEP 56 — map exact NSingleplayerSubmenu.OpenCharacterSelect without invoking it.

    public TransformedRealStS2StartupLadderGateResult RunStep56ClosedStep55Authority(bool renderingStopped, Action<string>? checkpoint = null)
    {
        const int step = 56;
        const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.PrerequisiteAuthority;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep56Prerequisite("Step 56 Gate A entry");
            if (!renderingStopped)
                throw new InvalidOperationException("Step 56.0 is non-invoking and requires rendering frozen.");
            var selected = RequireStartupLadderSelectedAuthority(step);
            _step56Baseline = CaptureStartupLadderBaseline(selected.Path, selected.Sha256, context);
            RequireStartupLadderBaselineUnchanged(context, _step56Baseline, "Step 56 Gate A");
            RequireVisibleSingleplayerSubmenu(step);
            Checkpoint(checkpoint, "M56_A_PASS — Step-55 4/4 rendered/refrozen NSingleplayerSubmenu authority retained; OpenCharacterSelect remains uninvoked.");
            return StartupLadderPass(step, Step56Name, gate,
                "Closed single-player submenu render authority retained while frozen. The exact OpenCharacterSelect frontier may now be mapped without invocation.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"M56_A_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return StartupLadderFail(step, Step56Name, gate, stage, ex);
        }
    }

    public TransformedRealStS2StartupLadderGateResult RunStep56CharacterSelectBinding(Action<string>? checkpoint = null)
    {
        const int step = 56;
        const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.StaticAuditOrBinding;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep56Prerequisite("Step 56 Gate B entry");
            var baseline = _step56Baseline ?? throw new InvalidOperationException("Step 56.0 Gate A must pass before Gate B.");
            using var resolver = new RejectingAssemblyResolver();
            using var module = OpenStartupLadderModule(baseline.SelectedPath, resolver);
            var allTypes = EnumerateTypes(module.Types).ToDictionary(type => type.FullName, StringComparer.Ordinal);
            var submenuType = RequireStartupLadderType(allTypes, SingleplayerSubmenuManagedTypeFullName, step);
            var open = RequireUniqueStartupLadderNamedMethod(submenuType, SingleplayerOpenCharacterSelectMethodName, step);
            RequireExactStep56OpenCharacterSelectSignature(open);
            if (resolver.Requests.Count != 0)
                throw new InvalidDataException("Step 56.0 binding unexpectedly attempted external Cecil resolution: " + string.Join(" | ", resolver.Requests));
            _step56OpenCharacterSelectToken = open.MetadataToken.ToUInt32();
            _step56StaticMap =
                "StS2 Launcher — Step 56.0 exact character-select frontier map\n" +
                "Evidence-only; OpenCharacterSelect is never invoked in Steps 56-57.\n" +
                $"Selected compatibility SHA-256: {baseline.SelectedSha256}\n" +
                $"NSingleplayerSubmenu.OpenCharacterSelect token=0x{_step56OpenCharacterSelectToken:X8}; params={open.Parameters.Count}; parameter0={open.Parameters[0].ParameterType.FullName}; return={open.ReturnType.FullName}; IL={open.Body.Instructions.Count}\n" +
                "Rendering restarted: NO\nOpenCharacterSelect invoked: NO\n" +
                "[OPENCHARACTERSELECT IL]\n" + string.Join("\n", open.Body.Instructions.Select(instruction => "  " + FormatStep41Instruction(instruction))) + "\n";
            RequireStartupLadderBaselineUnchanged(context, baseline, "Step 56 Gate B");
            Checkpoint(checkpoint, $"M56_B_PASS — exact OpenCharacterSelect bound token=0x{_step56OpenCharacterSelectToken:X8}; IL={open.Body.Instructions.Count}; externalResolution=0; invocation=NO.");
            return StartupLadderPass(step, Step56Name, gate,
                "Exact void NSingleplayerSubmenu.OpenCharacterSelect(NButton) binding/IL recorded without invocation.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"M56_B_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return StartupLadderFail(step, Step56Name, gate, stage, ex);
        }
    }

    public TransformedRealStS2StartupLadderGateResult RunStep56CharacterSelectFrontierMap(Action<string>? checkpoint = null)
    {
        const int step = 56;
        const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.ControlledAction;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep56Prerequisite("Step 56 Gate C entry");
            var baseline = _step56Baseline ?? throw new InvalidOperationException("Step 56.0 baseline absent.");
            using var resolver = new RejectingAssemblyResolver();
            using var module = OpenStartupLadderModule(baseline.SelectedPath, resolver);
            var allTypes = EnumerateTypes(module.Types).ToDictionary(type => type.FullName, StringComparer.Ordinal);
            var allMethods = BuildStartupLadderMethodMap(allTypes);
            var submenuType = RequireStartupLadderType(allTypes, SingleplayerSubmenuManagedTypeFullName, step);
            var open = RequireUniqueStartupLadderNamedMethod(submenuType, SingleplayerOpenCharacterSelectMethodName, step);
            RequireExactStep56OpenCharacterSelectSignature(open);
            if (open.MetadataToken.ToUInt32() != _step56OpenCharacterSelectToken)
                throw new InvalidDataException("Step 56.0 OpenCharacterSelect token drifted between Gate B and Gate C.");
            var guards = _step48LifecycleGuardAuthority ?? throw new InvalidOperationException("Step 56.0 requires retained Step-48 runtime guards.");
            var audit = AuditStartupLadderInvocationFrontier([open], allTypes, allMethods, guards);
            if (audit.UnresolvedSameAssemblyReferences.Length != 0)
                throw new InvalidDataException("Step 56.0 OpenCharacterSelect frontier has unresolved same-sts2 references: " + string.Join(" | ", audit.UnresolvedSameAssemblyReferences));
            if (resolver.Requests.Count != 0)
                throw new InvalidDataException("Step 56.0 frontier unexpectedly attempted external Cecil resolution: " + string.Join(" | ", resolver.Requests));
            var methodsByToken = allMethods.Values.GroupBy(method => method.MetadataToken.ToUInt32()).ToDictionary(group => group.Key, group => group.First());
            var literals = new SortedSet<string>(StringComparer.Ordinal);
            var sceneHints = new SortedSet<string>(StringComparer.Ordinal);
            foreach (var item in audit.ImmediateClosureMethods)
            {
                if (!methodsByToken.TryGetValue(item.Token, out var method) || !method.HasBody)
                    continue;
                foreach (var instruction in method.Body.Instructions)
                {
                    if (instruction.OpCode.Code != Code.Ldstr || instruction.Operand is not string text)
                        continue;
                    var normalized = text.Replace('\\', '/');
                    if (normalized.StartsWith("res://", StringComparison.Ordinal))
                        literals.Add(normalized);
                    if (string.Equals(normalized, CharacterSelectSceneShortHint, StringComparison.Ordinal) ||
                        string.Equals(normalized, CharacterSelectSceneResourcePath, StringComparison.Ordinal))
                        sceneHints.Add(normalized);
                }
            }
            _step56CharacterSelectSceneCandidates = sceneHints.ToArray();
            _step56StaticMap += BuildStartupLadderInvocationFrontierAppendix(audit) +
                "\n[RESOURCE STRING LITERALS FROM IMMEDIATE CLOSURE]\n" +
                (literals.Count == 0 ? "  - none\n" : string.Join("\n", literals.Select(value => "  - " + value)) + "\n") +
                "[CHARACTER-SELECT SCENE HINTS FROM IMMEDIATE CLOSURE]\n" +
                (_step56CharacterSelectSceneCandidates.Length == 0 ? "  - none\n" : string.Join("\n", _step56CharacterSelectSceneCandidates.Select(value => "  - " + value)) + "\n");
            _step56FrontierMapped = true;
            RequireStartupLadderBaselineUnchanged(context, baseline, "Step 56 Gate C");
            Checkpoint(checkpoint, $"M56_C_PASS — OpenCharacterSelect frontier mapped without invocation; closureMethods={audit.ImmediateClosureMethods.Length}; immediateBoundaryRefs={audit.ImmediateBoundaries.Length}; guardedFrontiers={audit.GuardedMethodFrontiers.Length}; deferredFrontiers={audit.DeferredMethodFrontiers.Length}; characterSelectSceneHints={_step56CharacterSelectSceneCandidates.Length}; unresolved/external=0.");
            return StartupLadderPass(step, Step56Name, gate,
                $"OpenCharacterSelect frontier and resource literals mapped as evidence only: immediate methods={audit.ImmediateClosureMethods.Length}; classified boundaries={audit.ImmediateBoundaries.Length}; character-select scene hints={_step56CharacterSelectSceneCandidates.Length}; invocation=NO.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"M56_C_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return StartupLadderFail(step, Step56Name, gate, stage, ex);
        }
    }

    public TransformedRealStS2StartupLadderGateResult RunStep56FrozenNoInvocationConfinement(bool renderingStopped, Action<string>? checkpoint = null)
    {
        const int step = 56;
        const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.PostActionConfinement;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep56Prerequisite("Step 56 Gate D entry");
            var baseline = _step56Baseline ?? throw new InvalidOperationException("Step 56.0 baseline absent.");
            if (!_step56FrontierMapped || !_step56StaticMapDurablyWritten || !renderingStopped)
                throw new InvalidOperationException("Step 56.0 Gate D requires durable non-invoking character-select frontier evidence with rendering frozen.");
            RequireStartupLadderBaselineUnchanged(context, baseline, "Step 56 Gate D");
            RequireVisibleSingleplayerSubmenu(step);
            _exactStep56ClosurePassed = true;
            Checkpoint(checkpoint, $"M56_D_PASS — character-select frontier durable; OpenCharacterSelect invocation=NO; characterSelectSceneHints={_step56CharacterSelectSceneCandidates.Length}; renderingStopped=True; drift=0.");
            return StartupLadderPass(step, Step56Name, gate,
                "Durable non-invoking character-select frontier authority closed. Step 57 may inspect the discovered exact PCK resource boundary without Godot loading or handler invocation.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"M56_D_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return StartupLadderFail(step, Step56Name, gate, stage, ex);
        }
    }

    private static void RequireExactStep56OpenCharacterSelectSignature(MethodDefinition open)
    {
        if (open.Parameters.Count != 1 ||
            open.Parameters[0].ParameterType.FullName != SingleplayerOpenCharacterSelectParameterTypeFullName ||
            open.ReturnType.FullName != "System.Void")
        {
            var parameterTypes = open.Parameters.Count == 0
                ? "none"
                : string.Join(",", open.Parameters.Select(parameter => parameter.ParameterType.FullName));
            throw new InvalidDataException(
                $"Step 56.0 requires void OpenCharacterSelect({SingleplayerOpenCharacterSelectParameterTypeFullName}); " +
                $"observed params={open.Parameters.Count}; parameterTypes={parameterTypes}; return={open.ReturnType.FullName}.");
        }
    }

    // STEP 57 — read-only exact PCK preflight of the character-select resource candidate. No Godot load.

    public TransformedRealStS2StartupLadderGateResult RunStep57ClosedStep56Authority(bool renderingStopped, Action<string>? checkpoint = null)
    {
        const int step = 57;
        const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.PrerequisiteAuthority;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep57Prerequisite("Step 57 Gate A entry");
            if (!renderingStopped)
                throw new InvalidOperationException("Step 57.0 is read-only and requires rendering frozen.");
            var selected = RequireStartupLadderSelectedAuthority(step);
            var baseline = CaptureStartupLadderBaseline(selected.Path, selected.Sha256, context);
            _step57Baseline = baseline;
            RequireStartupLadderBaselineUnchanged(context, baseline, "Step 57 Gate A");

            // Do not infer the character-select scene identity from incidental ldstr literals.
            // The retained real NMainMenuSubmenuStack owns the exported PackedScene that the game
            // will use for this transition. Gate A proves that field in both selected sts2 metadata
            // and the live retained stack, then reads only the already-loaded ResourcePath string.
            using var resolver = new RejectingAssemblyResolver();
            using var module = OpenStartupLadderModule(baseline.SelectedPath, resolver);
            var allTypes = EnumerateTypes(module.Types).ToDictionary(type => type.FullName, StringComparer.Ordinal);
            var stackType = RequireStartupLadderType(allTypes, MainMenuSubmenuStackManagedTypeFullName, step);
            var sceneFields = stackType.Fields.Where(field => field.Name == MainMenuCharacterSelectSceneFieldName).ToArray();
            if (sceneFields.Length != 1)
                throw new InvalidDataException($"Step 57.0 requires exactly one {MainMenuSubmenuStackManagedTypeFullName}.{MainMenuCharacterSelectSceneFieldName} field; observed={sceneFields.Length}.");
            var sceneFieldDefinition = sceneFields[0];
            if (sceneFieldDefinition.IsStatic || sceneFieldDefinition.FieldType.FullName != PackedSceneManagedTypeFullName)
                throw new InvalidDataException($"Step 57.0 requires instance {PackedSceneManagedTypeFullName} {MainMenuSubmenuStackManagedTypeFullName}.{MainMenuCharacterSelectSceneFieldName}; observedType={sceneFieldDefinition.FieldType.FullName}; static={sceneFieldDefinition.IsStatic}.");
            if (resolver.Requests.Count != 0)
                throw new InvalidDataException("Step 57.0 character-select scene-field binding unexpectedly attempted external Cecil resolution: " + string.Join(" | ", resolver.Requests));

            var submenuStack = _step54SubmenuStack ?? throw new InvalidOperationException("Step 57.0 requires the retained real NMainMenuSubmenuStack from Step 54.");
            if (submenuStack.GetType().FullName != MainMenuSubmenuStackManagedTypeFullName)
                throw new InvalidDataException($"Step 57.0 retained submenu-stack runtime type drifted: {submenuStack.GetType().FullName}.");
            var runtimeSceneField = RequireRuntimeExactInstanceField(submenuStack.GetType(), MainMenuCharacterSelectSceneFieldName, PackedSceneManagedTypeFullName, step);
            var packedScene = runtimeSceneField.GetValue(submenuStack) ?? throw new InvalidDataException($"Step 57.0 retained {MainMenuCharacterSelectSceneFieldName} is null.");
            var resourcePath = RequireRuntimeStringProperty(packedScene, GodotResourcePathPropertyName, step).Replace('\\', '/');
            if (!string.Equals(resourcePath, CharacterSelectSceneResourcePath, StringComparison.Ordinal))
                throw new InvalidDataException($"Step 57.0 retained character-select PackedScene ResourcePath drifted: expected={CharacterSelectSceneResourcePath}; observed={resourcePath}.");

            var pck = RequireEssentialResourcePackHandoff().PackAbsolutePath;
            RequireExactPckDirectoryResource(pck, resourcePath, step);
            _step57CharacterSelectResourcePath = resourcePath;
            RequireStartupLadderBaselineUnchanged(context, baseline, "Step 57 Gate A postbinding");
            Checkpoint(checkpoint, $"M57_A_PASS — Step-56 4/4 non-invoking frontier retained; exact {MainMenuSubmenuStackManagedTypeFullName}.{MainMenuCharacterSelectSceneFieldName}:{PackedSceneManagedTypeFullName} bound in selected metadata and retained runtime stack; existing ResourcePath='{_step57CharacterSelectResourcePath}' matched exactly one receipt-backed PCK directory entry; renderingStopped=True; ResourceLoader=NO; PackedScene instantiation=NO.");
            return StartupLadderPass(step, Step57Name, gate,
                $"Step-56 frontier retained and the already-loaded main-menu character-select PackedScene field proved one exact PCK resource for read-only inspection: {_step57CharacterSelectResourcePath}.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"M57_A_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return StartupLadderFail(step, Step57Name, gate, stage, ex);
        }
    }

    public TransformedRealStS2StartupLadderGateResult RunStep57CharacterSelectPckExtraction(Action<string>? checkpoint = null)
    {
        const int step = 57;
        const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.StaticAuditOrBinding;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep57Prerequisite("Step 57 Gate B entry");
            var baseline = _step57Baseline ?? throw new InvalidOperationException("Step 57.0 Gate A must pass before Gate B.");
            var pck = RequireEssentialResourcePackHandoff().PackAbsolutePath;
            var entry = ExtractStartupLadderPckEntryUnpinned(pck, _step57CharacterSelectResourcePath, step);
            var sha = Convert.ToHexString(SHA256.HashData(entry.Bytes)).ToLowerInvariant();
            var text = new UTF8Encoding(false, true).GetString(entry.Bytes);
            _step57StaticMap =
                "StS2 Launcher — Step 57.0 character-select exact PCK resource preflight\n" +
                "Read-only PCK evidence. This step never calls ResourceLoader, PackedScene.Instantiate, or OpenCharacterSelect.\n" +
                $"Selected compatibility SHA-256: {baseline.SelectedSha256}\n" +
                $"Resource source: {MainMenuSubmenuStackManagedTypeFullName}.{MainMenuCharacterSelectSceneFieldName}.{GodotResourcePathPropertyName}\n" +
                $"Resource path: {_step57CharacterSelectResourcePath}\n" +
                $"Bytes: {entry.Bytes.Length}\n" +
                $"SHA-256: {sha}\n" +
                $"PCK MD5: {entry.PckMd5}\n" +
                $"PCK entry flags: 0x{entry.Flags:X8}\n" +
                $"UTF-8 text decode: PASS\n" +
                "Godot resource load: NO\nOpenCharacterSelect invoked: NO\nRendering restarted: NO\n";
            _step57StaticMap += BuildCharacterSelectTextRiskAppendix(text);
            RequireStartupLadderBaselineUnchanged(context, baseline, "Step 57 Gate B");
            Checkpoint(checkpoint, $"M57_B_PASS — exact PCK resource extracted read-only; path='{_step57CharacterSelectResourcePath}'; bytes={entry.Bytes.Length}; sha256={sha}; ResourceLoader=NO; invocation=NO.");
            return StartupLadderPass(step, Step57Name, gate,
                $"Exact character-select PCK resource extracted and hashed read-only ({entry.Bytes.Length} bytes, {sha}); no Godot loading or handler invocation.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"M57_B_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return StartupLadderFail(step, Step57Name, gate, stage, ex);
        }
    }

    public TransformedRealStS2StartupLadderGateResult RunStep57CharacterSelectResourceRiskMap(Action<string>? checkpoint = null)
    {
        const int step = 57;
        const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.ControlledAction;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep57Prerequisite("Step 57 Gate C entry");
            var baseline = _step57Baseline ?? throw new InvalidOperationException("Step 57.0 baseline absent.");
            if (string.IsNullOrWhiteSpace(_step57StaticMap))
                throw new InvalidOperationException("Step 57.0 exact PCK resource identity map must exist before risk classification.");
            _step57PreflightMapped = true;
            RequireStartupLadderBaselineUnchanged(context, baseline, "Step 57 Gate C");
            RequireVisibleSingleplayerSubmenu(step);
            Checkpoint(checkpoint, "M57_C_PASS — character-select resource risk map complete from exact trusted PCK bytes; no ResourceLoader/PackedScene/handler invocation; no rendering; current submenu authority retained.");
            return StartupLadderPass(step, Step57Name, gate,
                "Exact character-select resource textual risk surface is durable evidence only. Native/Spine/FMOD findings, if present, are not authorized; no resource was loaded.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"M57_C_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return StartupLadderFail(step, Step57Name, gate, stage, ex);
        }
    }

    public TransformedRealStS2StartupLadderGateResult RunStep57FrozenReadOnlyConfinement(bool renderingStopped, Action<string>? checkpoint = null)
    {
        const int step = 57;
        const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.PostActionConfinement;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep57Prerequisite("Step 57 Gate D entry");
            var baseline = _step57Baseline ?? throw new InvalidOperationException("Step 57.0 baseline absent.");
            if (!_step57PreflightMapped || !_step57StaticMapDurablyWritten || !renderingStopped)
                throw new InvalidOperationException("Step 57.0 Gate D requires durable read-only character-select resource evidence with rendering frozen.");
            RequireStartupLadderBaselineUnchanged(context, baseline, "Step 57 Gate D");
            RequireVisibleSingleplayerSubmenu(step);
            _exactStep57ClosurePassed = true;
            Checkpoint(checkpoint, "M57_D_PASS — read-only character-select resource preflight durable; OpenCharacterSelect/ResourceLoader invocation=NO; NSingleplayerSubmenu retained; renderingStopped=True; drift=0.");
            return StartupLadderPass(step, Step57Name, gate,
                "Character-select resource preflight closed 4/4 without loading or invoking the transition. The exact resource/native risk evidence is ready for the next dedicated design.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"M57_D_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return StartupLadderFail(step, Step57Name, gate, stage, ex);
        }
    }

    private Step35ExecutionLoadContext RequireStep53Prerequisite(string boundary)
    {
        var context = RequireStep52Prerequisite(boundary);
        if (!_exactStep52ClosurePassed || !_step52PulsePassed || !_step52StaticMapDurablyWritten)
            throw new InvalidOperationException(boundary + " requires same-process Step-52 4/4 sustained main-menu render/refreeze authority.");
        return context;
    }

    private Step35ExecutionLoadContext RequireStep54Prerequisite(string boundary)
    {
        var context = RequireStep53Prerequisite(boundary);
        if (!_exactStep53ClosurePassed || !_step53OpenAdmissible || !_step53StaticMapDurablyWritten)
            throw new InvalidOperationException(boundary + " requires same-process Step-53 4/4 durable admissible OpenSingleplayerSubmenu authority.");
        return context;
    }

    private Step35ExecutionLoadContext RequireStep55Prerequisite(string boundary)
    {
        var context = RequireStep54Prerequisite(boundary);
        if (!_exactStep54ClosurePassed || !_step54OpenPassed || !_step54StaticMapDurablyWritten)
            throw new InvalidOperationException(boundary + " requires same-process Step-54 4/4 real visible/in-tree NSingleplayerSubmenu authority.");
        return context;
    }

    private Step35ExecutionLoadContext RequireStep56Prerequisite(string boundary)
    {
        var context = RequireStep55Prerequisite(boundary);
        if (!_exactStep55ClosurePassed || !_step55PulsePassed || !_step55StaticMapDurablyWritten)
            throw new InvalidOperationException(boundary + " requires same-process Step-55 4/4 rendered/refrozen NSingleplayerSubmenu authority.");
        return context;
    }

    private Step35ExecutionLoadContext RequireStep57Prerequisite(string boundary)
    {
        var context = RequireStep56Prerequisite(boundary);
        if (!_exactStep56ClosurePassed || !_step56FrontierMapped || !_step56StaticMapDurablyWritten)
            throw new InvalidOperationException(boundary + " requires same-process Step-56 4/4 durable non-invoking OpenCharacterSelect frontier authority.");
        return context;
    }

    private object RequireRetainedInTreeMainMenu(int step)
    {
        var menu = _step48MainMenuInstance ?? throw new InvalidOperationException($"Step {step}.0 retained NMainMenu is absent.");
        if (RequireZeroArgBoolMethod(menu.GetType(), "IsInsideTree").Invoke(menu, null) is not true)
            throw new InvalidDataException($"Step {step}.0 requires retained in-tree NMainMenu authority.");
        var root = _step49RootSceneContainer ?? throw new InvalidOperationException($"Step {step}.0 exact RootSceneContainer authority absent.");
        RequireStartupLadderRootSceneContainerPropertyIdentity(root, step, $"Step {step}.0 retained root authority");
        return menu;
    }

    private object RequireVisibleSingleplayerSubmenu(int step)
    {
        var submenu = _step54SingleplayerSubmenu ?? throw new InvalidOperationException($"Step {step}.0 retained NSingleplayerSubmenu is absent.");
        if (submenu.GetType().FullName != SingleplayerSubmenuManagedTypeFullName)
            throw new InvalidDataException($"Step {step}.0 retained submenu type drifted: {submenu.GetType().FullName}.");
        if (RequireZeroArgBoolMethod(submenu.GetType(), "IsInsideTree").Invoke(submenu, null) is not true ||
            RequireZeroArgBoolMethod(submenu.GetType(), "IsVisibleInTree").Invoke(submenu, null) is not true ||
            !RequireRuntimeBoolProperty(submenu, "Visible", step))
            throw new InvalidDataException($"Step {step}.0 requires retained visible/in-tree NSingleplayerSubmenu authority.");
        return submenu;
    }

    private static MethodInfo RequireRuntimeDeclaredZeroArgSingleplayerSubmenuMethod(Type type, int step)
    {
        var namedCandidates = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
            .Where(method => method.Name == MainMenuOpenSingleplayerSubmenuMethodName && !method.IsGenericMethod)
            .ToArray();
        var exactCandidates = namedCandidates
            .Where(method => method.GetParameters().Length == 0 && method.ReturnType.FullName == SingleplayerSubmenuManagedTypeFullName)
            .ToArray();
        return exactCandidates.SingleOrDefault()
            ?? throw new MissingMethodException(type.FullName,
                $"{MainMenuOpenSingleplayerSubmenuMethodName}() -> {SingleplayerSubmenuManagedTypeFullName}; Step {step}.0 candidates={string.Join(" | ", namedCandidates.Select(method => method.ToString()))}");
    }

    private static object RequireRuntimeExactObjectProperty(object instance, string propertyName, string expectedTypeFullName, int step)
    {
        PropertyInfo? property = null;
        for (var type = instance.GetType(); type is not null && property is null; type = type.BaseType)
            property = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
        if (property is null)
            throw new MissingMemberException(instance.GetType().FullName, propertyName);
        if (property.GetIndexParameters().Length != 0 || property.GetMethod is null || property.PropertyType.FullName != expectedTypeFullName)
            throw new InvalidDataException($"Step {step}.0 property {instance.GetType().FullName}.{propertyName} is not a readable non-indexed {expectedTypeFullName} property; observed={property.PropertyType.FullName}.");
        var value = property.GetValue(instance) ?? throw new InvalidDataException($"Step {step}.0 property {propertyName} returned null.");
        if (value.GetType().FullName != expectedTypeFullName)
            throw new InvalidDataException($"Step {step}.0 property {propertyName} runtime type drifted: {value.GetType().FullName}.");
        return value;
    }

    private static FieldInfo RequireRuntimeExactInstanceField(Type runtimeType, string fieldName, string expectedTypeFullName, int step)
    {
        FieldInfo? field = null;
        for (var type = runtimeType; type is not null && field is null; type = type.BaseType)
            field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
        if (field is null)
            throw new MissingFieldException(runtimeType.FullName, fieldName);
        if (field.IsStatic || field.FieldType.FullName != expectedTypeFullName)
            throw new InvalidDataException($"Step {step}.0 field {runtimeType.FullName}.{fieldName} is not an instance {expectedTypeFullName} field; observed={field.FieldType.FullName}; static={field.IsStatic}.");
        return field;
    }

    private static bool RequireRuntimeBoolProperty(object instance, string propertyName, int step)
    {
        PropertyInfo? property = null;
        for (var type = instance.GetType(); type is not null && property is null; type = type.BaseType)
            property = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
        if (property is null)
            throw new MissingMemberException(instance.GetType().FullName, propertyName);
        if (property.PropertyType != typeof(bool) || property.GetIndexParameters().Length != 0 || property.GetMethod is null)
            throw new InvalidDataException($"Step {step}.0 property {instance.GetType().FullName}.{propertyName} is not a readable non-indexed bool.");
        return (bool)(property.GetValue(instance) ?? throw new InvalidDataException($"Step {step}.0 property {propertyName} returned null."));
    }

    private static string RequireRuntimeStringProperty(object instance, string propertyName, int step)
    {
        PropertyInfo? property = null;
        for (var type = instance.GetType(); type is not null && property is null; type = type.BaseType)
            property = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
        if (property is null)
            throw new MissingMemberException(instance.GetType().FullName, propertyName);
        if (property.PropertyType != typeof(string) || property.GetIndexParameters().Length != 0 || property.GetMethod is null)
            throw new InvalidDataException($"Step {step}.0 property {instance.GetType().FullName}.{propertyName} is not a readable non-indexed string.");
        return (string?)property.GetValue(instance) ?? throw new InvalidDataException($"Step {step}.0 property {propertyName} returned null.");
    }

    private static void RequireNoInitializerRejectedNativeEscape(Step35ExecutionLoadContext context, StartupLadderBaseline baseline, string boundary)
    {
        var initializerDelta = context.InitializerBearingRequests.Count - baseline.InitializerCount;
        var rejectedDelta = context.RejectedManagedRequests.Count - baseline.RejectedCount;
        var nativeDelta = context.NativeLoadAttempts.Count - baseline.NativeCount;
        if (initializerDelta != 0 || rejectedDelta != 0 || nativeDelta != 0)
            throw new InvalidDataException($"{boundary} escaped managed/native confinement. initializer={initializerDelta}; rejected={rejectedDelta}; native={nativeDelta}.");
    }

    private static void RequireExactPckDirectoryResource(string pckPath, string resourcePath, int step)
    {
        using var stream = new FileStream(pckPath, FileMode.Open, FileAccess.Read, FileShare.Read, 16 * 1024, FileOptions.SequentialScan);
        using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);
        var magic = reader.ReadUInt32();
        if (magic != 0x43504447)
            throw new InvalidDataException($"Step {step}.0 expected standalone Godot PCK magic 0x43504447; observed 0x{magic:X8}.");
        var format = reader.ReadUInt32();
        var major = reader.ReadUInt32();
        var minor = reader.ReadUInt32();
        var patch = reader.ReadUInt32();
        var flags = reader.ReadUInt32();
        _ = reader.ReadUInt64(); // fileBase; directory-only identity proof does not read entry bytes.
        if (format != ClosedPckFormat || major != ClosedPckEngineMajor || minor != ClosedPckEngineMinor || patch != ClosedPckEnginePatch || flags != ClosedPckFlags)
            throw new InvalidDataException($"Step {step}.0 PCK header drifted: format={format}; engine={major}.{minor}.{patch}; flags=0x{flags:X8}.");
        var directoryOffset = reader.ReadUInt64();
        if (directoryOffset >= (ulong)stream.Length)
            throw new InvalidDataException($"Step {step}.0 PCK directory offset {directoryOffset} exceeds file length {stream.Length}.");
        stream.Seek(checked((long)directoryOffset), SeekOrigin.Begin);
        var count = reader.ReadUInt32();
        if (count != ClosedPckDirectoryEntries)
            throw new InvalidDataException($"Step {step}.0 PCK directory count drifted: {count}.");
        var matches = 0;
        for (var i = 0u; i < count; i++)
        {
            var pathLength = reader.ReadUInt32();
            if (pathLength == 0 || pathLength > 1_048_576)
                throw new InvalidDataException($"Step {step}.0 PCK directory path length is invalid at entry {i}: {pathLength}.");
            var pathBytes = ReadExactlyStep37(reader, checked((int)pathLength));
            var storedPath = Encoding.UTF8.GetString(pathBytes).TrimEnd('\0').Replace('\\', '/');
            _ = reader.ReadUInt64(); // offset
            _ = reader.ReadUInt64(); // size
            _ = ReadExactlyStep37(reader, 16); // directory MD5
            _ = reader.ReadUInt32(); // entry flags
            var normalized = storedPath.StartsWith("res://", StringComparison.Ordinal) ? storedPath : "res://" + storedPath.TrimStart('/');
            if (string.Equals(normalized, resourcePath, StringComparison.Ordinal))
                matches++;
        }
        if (matches != 1)
            throw new InvalidDataException($"Step {step}.0 requires exactly one exact PCK directory entry for {resourcePath}; observed={matches}.");
    }

    private static ExtractedPckEntry ExtractStartupLadderPckEntryUnpinned(string pckPath, string resourcePath, int step)
    {
        using var stream = new FileStream(pckPath, FileMode.Open, FileAccess.Read, FileShare.Read, 16 * 1024, FileOptions.SequentialScan);
        using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);
        var magic = reader.ReadUInt32();
        if (magic != 0x43504447)
            throw new InvalidDataException($"Step {step}.0 expected standalone Godot PCK magic 0x43504447; observed 0x{magic:X8}.");
        var format = reader.ReadUInt32();
        var major = reader.ReadUInt32();
        var minor = reader.ReadUInt32();
        var patch = reader.ReadUInt32();
        var flags = reader.ReadUInt32();
        var fileBase = reader.ReadUInt64();
        if (format != ClosedPckFormat || major != ClosedPckEngineMajor || minor != ClosedPckEngineMinor || patch != ClosedPckEnginePatch || flags != ClosedPckFlags)
            throw new InvalidDataException($"Step {step}.0 PCK header drifted: format={format}; engine={major}.{minor}.{patch}; flags=0x{flags:X8}.");
        var directoryOffset = reader.ReadUInt64();
        if (directoryOffset >= (ulong)stream.Length)
            throw new InvalidDataException($"Step {step}.0 PCK directory offset {directoryOffset} exceeds file length {stream.Length}.");
        stream.Seek(checked((long)directoryOffset), SeekOrigin.Begin);
        var count = reader.ReadUInt32();
        if (count != ClosedPckDirectoryEntries)
            throw new InvalidDataException($"Step {step}.0 PCK directory count drifted: {count}.");
        PckDirectoryEntry? match = null;
        for (var i = 0u; i < count; i++)
        {
            var pathLength = reader.ReadUInt32();
            if (pathLength == 0 || pathLength > 1_048_576)
                throw new InvalidDataException($"Step {step}.0 PCK directory path length is invalid at entry {i}: {pathLength}.");
            var pathBytes = ReadExactlyStep37(reader, checked((int)pathLength));
            var storedPath = Encoding.UTF8.GetString(pathBytes).TrimEnd('\0').Replace('\\', '/');
            var offset = reader.ReadUInt64();
            var size = reader.ReadUInt64();
            var md5 = ReadExactlyStep37(reader, 16);
            var entryFlags = reader.ReadUInt32();
            var normalized = storedPath.StartsWith("res://", StringComparison.Ordinal) ? storedPath : "res://" + storedPath.TrimStart('/');
            if (string.Equals(normalized, resourcePath, StringComparison.Ordinal))
            {
                if (match is not null)
                    throw new InvalidDataException($"Step {step}.0 found duplicate PCK entry for {resourcePath}.");
                match = new PckDirectoryEntry(normalized, offset, size, md5, entryFlags);
            }
        }
        var entry = match ?? throw new FileNotFoundException($"Step {step}.0 did not find exact PCK resource {resourcePath}.");
        if ((entry.Flags & 1u) != 0)
            throw new InvalidDataException($"Step {step}.0 refuses encrypted PCK resource {resourcePath}.");
        if (entry.Size > int.MaxValue)
            throw new InvalidDataException($"Step {step}.0 refuses oversized resource {resourcePath}: {entry.Size} bytes.");
        var absolute = checked(fileBase + entry.Offset);
        if (absolute + entry.Size > (ulong)stream.Length)
            throw new InvalidDataException($"Step {step}.0 PCK resource {resourcePath} points beyond pack length.");
        stream.Seek(checked((long)absolute), SeekOrigin.Begin);
        var bytes = ReadExactlyStep37(reader, checked((int)entry.Size));
        var pckMd5 = Convert.ToHexString(entry.Md5).ToLowerInvariant();
        var actualMd5 = Convert.ToHexString(MD5.HashData(bytes)).ToLowerInvariant();
        if (!pckMd5.Equals(actualMd5, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException($"Step {step}.0 PCK MD5 mismatch for {resourcePath}: directory={pckMd5}; actual={actualMd5}.");
        return new ExtractedPckEntry(bytes, entry.Offset, entry.Size, pckMd5, entry.Flags, directoryOffset, fileBase, count);
    }

    private static string BuildCharacterSelectTextRiskAppendix(string text)
    {
        var normalized = text.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');
        var lines = normalized.Split('\n');
        var resourcePaths = new SortedSet<string>(StringComparer.Ordinal);
        foreach (var line in lines)
        {
            var search = 0;
            while (true)
            {
                var start = line.IndexOf("res://", search, StringComparison.Ordinal);
                if (start < 0)
                    break;
                var endQuote = line.IndexOf('"', start);
                var endBracket = line.IndexOf(']', start);
                var endSpace = line.IndexOf(' ', start);
                var candidates = new[] { endQuote, endBracket, endSpace }.Where(value => value > start).ToArray();
                var end = candidates.Length == 0 ? line.Length : candidates.Min();
                var path = line[start..end].TrimEnd(',', ')', '}');
                if (!string.IsNullOrWhiteSpace(path))
                    resourcePaths.Add(path);
                search = Math.Max(start + 6, end);
            }
        }
        var spineCount = CountOccurrences(normalized, "Spine");
        var fmodCount = CountOccurrences(normalized, "Fmod") + CountOccurrences(normalized, "FMOD");
        var gdextensionCount = CountOccurrences(normalized, ".gdextension");
        var dylibCount = CountOccurrences(normalized, ".dylib");
        var dllCount = CountOccurrences(normalized, ".dll");
        var nativeRisk = spineCount + fmodCount + gdextensionCount + dylibCount + dllCount;
        return
            "[TEXTUAL NATIVE/EXTENSION RISK SCAN]\n" +
            $"Spine token occurrences: {spineCount}\n" +
            $"FMOD/Fmod token occurrences: {fmodCount}\n" +
            $".gdextension occurrences: {gdextensionCount}\n" +
            $".dylib occurrences: {dylibCount}\n" +
            $".dll occurrences: {dllCount}\n" +
            $"Combined textual native-risk occurrences: {nativeRisk}\n" +
            "Risk tokens are evidence only and authorize nothing.\n" +
            "[REFERENCED RES:// PATHS]\n" +
            (resourcePaths.Count == 0 ? "  - none\n" : string.Join("\n", resourcePaths.Select(value => "  - " + value)) + "\n");
    }

    private static int CountOccurrences(string text, string value)
    {
        if (string.IsNullOrEmpty(value))
            return 0;
        var count = 0;
        var offset = 0;
        while ((offset = text.IndexOf(value, offset, StringComparison.Ordinal)) >= 0)
        {
            count++;
            offset += value.Length;
        }
        return count;
    }
}
