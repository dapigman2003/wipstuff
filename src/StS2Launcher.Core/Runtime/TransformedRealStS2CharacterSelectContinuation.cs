using System.Reflection;
using System.Runtime.Loader;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace StS2Launcher.Core;

/// <summary>
/// Steps 58-64 continue only from physically closed Step-57 authority. They decompose the exact
/// OpenCharacterSelect(NButton) body already mapped by Step 56 into independently gated operations:
/// GetSubmenuType&lt;NCharacterSelectScreen&gt; factory mapping/creation, InitializeSingleplayer mapping/
/// execution, NSubmenuStack.Push mapping/frozen admission, and finally one bounded rendered residency.
/// The original OpenCharacterSelect handler is never invoked by these steps.
/// </summary>
public sealed partial class TransformedRealStS2VeryEarlyInitialization
{
    public const int Step64CharacterSelectRenderTargetMilliseconds = 750;
    public const int Step64CharacterSelectRenderEvidenceCeilingMilliseconds = 5_000;

    private const string Step58Name = "CHARACTER SELECT FACTORY FRONTIER";
    private const string Step59Name = "CHARACTER SELECT FROZEN OFF-TREE ACQUISITION";
    private const string Step60Name = "CHARACTER SELECT INITIALIZE FRONTIER";
    private const string Step61Name = "CHARACTER SELECT OFF-TREE INITIALIZATION";
    private const string Step62Name = "CHARACTER SELECT PUSH FRONTIER";
    private const string Step63Name = "CHARACTER SELECT FROZEN SCENETREE ADMISSION";
    private const string Step64Name = "CHARACTER SELECT RENDER RESIDENCY";

    private const string CharacterSelectScreenManagedTypeFullName = "MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect.NCharacterSelectScreen";
    private const string SubmenuManagedTypeFullName = "MegaCrit.Sts2.Core.Nodes.Screens.MainMenu.NSubmenu";
    private const string SubmenuStackManagedTypeFullName = "MegaCrit.Sts2.Core.Nodes.Screens.MainMenu.NSubmenuStack";
    private const string MainMenuCharacterSelectSubmenuFieldName = "_characterSelectSubmenu";
    private const string SubmenuStackFieldName = "_stack";
    private const string GetSubmenuTypeMethodName = "GetSubmenuType";
    private const string CharacterSelectInitializeSingleplayerMethodName = "InitializeSingleplayer";
    private const string SubmenuStackPushMethodName = "Push";

    private StartupLadderBaseline? _step58Baseline;
    private StartupLadderBaseline? _step59Baseline;
    private StartupLadderBaseline? _step59PostCreationBaseline;
    private StartupLadderBaseline? _step60Baseline;
    private StartupLadderBaseline? _step61Baseline;
    private StartupLadderBaseline? _step61PostInitializationBaseline;
    private StartupLadderBaseline? _step62Baseline;
    private StartupLadderBaseline? _step63Baseline;
    private StartupLadderBaseline? _step63PostAdmissionBaseline;
    private StartupLadderBaseline? _step64Baseline;
    private StartupLadderBaseline? _step64PostPulseBaseline;

    private string _step58StaticMap = string.Empty;
    private string _step59StaticMap = string.Empty;
    private string _step60StaticMap = string.Empty;
    private string _step61StaticMap = string.Empty;
    private string _step62StaticMap = string.Empty;
    private string _step63StaticMap = string.Empty;
    private string _step64StaticMap = string.Empty;

    private bool _step58FactoryMapped;
    private bool _step58StaticMapDurablyWritten;
    private bool _step59CreationStarted;
    private bool _step59CreationPassed;
    private bool _step59StaticMapDurablyWritten;
    private bool _step60InitializeMapped;
    private bool _step60StaticMapDurablyWritten;
    private bool _step61InitializationStarted;
    private bool _step61InitializationPassed;
    private bool _step61StaticMapDurablyWritten;
    private bool _step62PushMapped;
    private bool _step62StaticMapDurablyWritten;
    private bool _step63AdmissionStarted;
    private bool _step63AdmissionPassed;
    private bool _step63StaticMapDurablyWritten;
    private bool _step64StaticMapDurablyWritten;
    private bool _step64PulseStarted;
    private bool _step64PulsePassed;

    private bool _exactStep58ClosurePassed;
    private bool _exactStep59ClosurePassed;
    private bool _exactStep60ClosurePassed;
    private bool _exactStep61ClosurePassed;
    private bool _exactStep62ClosurePassed;
    private bool _exactStep63ClosurePassed;
    private bool _exactStep64ClosurePassed;

    private uint _step58FactoryMethodToken;
    private uint _step60InitializeMethodToken;
    private uint _step62PushMethodToken;
    private MethodInfo? _step58RuntimeFactoryDefinition;
    private MethodInfo? _step60RuntimeInitializeMethod;
    private MethodInfo? _step62RuntimePushMethod;
    private object? _step59CharacterSelectScreen;
    private bool _step58CharacterSelectCachePreexisting;
    private int _step63StackChildrenBefore;

    public bool ExactStep58ClosurePassed => _exactStep58ClosurePassed;
    public bool ExactStep59ClosurePassed => _exactStep59ClosurePassed;
    public bool ExactStep60ClosurePassed => _exactStep60ClosurePassed;
    public bool ExactStep61ClosurePassed => _exactStep61ClosurePassed;
    public bool ExactStep62ClosurePassed => _exactStep62ClosurePassed;
    public bool ExactStep63ClosurePassed => _exactStep63ClosurePassed;
    public bool ExactStep64ClosurePassed => _exactStep64ClosurePassed;
    public bool Step59CreationStarted => _step59CreationStarted;
    public bool Step59FactoryInvocationRequired => !_step58CharacterSelectCachePreexisting;
    public bool Step61InitializationStarted => _step61InitializationStarted;
    public bool Step63AdmissionStarted => _step63AdmissionStarted;
    public bool Step64PulseStarted => _step64PulseStarted;

    private void ResetCharacterSelectContinuationState()
    {
        _step58Baseline = null;
        _step59Baseline = null;
        _step59PostCreationBaseline = null;
        _step60Baseline = null;
        _step61Baseline = null;
        _step61PostInitializationBaseline = null;
        _step62Baseline = null;
        _step63Baseline = null;
        _step63PostAdmissionBaseline = null;
        _step64Baseline = null;
        _step64PostPulseBaseline = null;
        _step58StaticMap = string.Empty;
        _step59StaticMap = string.Empty;
        _step60StaticMap = string.Empty;
        _step61StaticMap = string.Empty;
        _step62StaticMap = string.Empty;
        _step63StaticMap = string.Empty;
        _step64StaticMap = string.Empty;
        _step58FactoryMapped = false;
        _step58StaticMapDurablyWritten = false;
        _step59CreationStarted = false;
        _step59CreationPassed = false;
        _step59StaticMapDurablyWritten = false;
        _step60InitializeMapped = false;
        _step60StaticMapDurablyWritten = false;
        _step61InitializationStarted = false;
        _step61InitializationPassed = false;
        _step61StaticMapDurablyWritten = false;
        _step62PushMapped = false;
        _step62StaticMapDurablyWritten = false;
        _step63AdmissionStarted = false;
        _step63AdmissionPassed = false;
        _step63StaticMapDurablyWritten = false;
        _step64StaticMapDurablyWritten = false;
        _step64PulseStarted = false;
        _step64PulsePassed = false;
        _exactStep58ClosurePassed = false;
        _exactStep59ClosurePassed = false;
        _exactStep60ClosurePassed = false;
        _exactStep61ClosurePassed = false;
        _exactStep62ClosurePassed = false;
        _exactStep63ClosurePassed = false;
        _exactStep64ClosurePassed = false;
        _step58FactoryMethodToken = 0;
        _step60InitializeMethodToken = 0;
        _step62PushMethodToken = 0;
        _step58RuntimeFactoryDefinition = null;
        _step60RuntimeInitializeMethod = null;
        _step62RuntimePushMethod = null;
        _step59CharacterSelectScreen = null;
        _step58CharacterSelectCachePreexisting = false;
        _step63StackChildrenBefore = 0;
    }

    public string GetVerifiedCharacterSelectContinuationStaticMap(int step)
        => step switch
        {
            58 when !string.IsNullOrWhiteSpace(_step58StaticMap) => _step58StaticMap,
            59 when !string.IsNullOrWhiteSpace(_step59StaticMap) => _step59StaticMap,
            60 when !string.IsNullOrWhiteSpace(_step60StaticMap) => _step60StaticMap,
            61 when !string.IsNullOrWhiteSpace(_step61StaticMap) => _step61StaticMap,
            62 when !string.IsNullOrWhiteSpace(_step62StaticMap) => _step62StaticMap,
            63 when !string.IsNullOrWhiteSpace(_step63StaticMap) => _step63StaticMap,
            64 when !string.IsNullOrWhiteSpace(_step64StaticMap) => _step64StaticMap,
            _ => throw new InvalidOperationException($"Step {step}.0 has not produced a verified character-select continuation static map."),
        };

    public void MarkStep58StaticMapDurablyWritten()
    {
        if (!_step58FactoryMapped || string.IsNullOrWhiteSpace(_step58StaticMap))
            throw new InvalidOperationException("Step 58.0 factory map is incomplete.");
        _step58StaticMapDurablyWritten = true;
    }

    public void MarkStep59StaticMapDurablyWritten()
    {
        if (!_step59CreationPassed || string.IsNullOrWhiteSpace(_step59StaticMap))
            throw new InvalidOperationException("Step 59.0 actual off-tree acquisition/lifecycle map is incomplete.");
        _step59StaticMapDurablyWritten = true;
    }

    public void MarkStep60StaticMapDurablyWritten()
    {
        if (!_step60InitializeMapped || string.IsNullOrWhiteSpace(_step60StaticMap))
            throw new InvalidOperationException("Step 60.0 InitializeSingleplayer frontier map is incomplete.");
        _step60StaticMapDurablyWritten = true;
    }

    public void MarkStep61StaticMapDurablyWritten()
    {
        if (!_step61InitializationPassed || string.IsNullOrWhiteSpace(_step61StaticMap))
            throw new InvalidOperationException("Step 61.0 initialization observation map is incomplete.");
        _step61StaticMapDurablyWritten = true;
    }

    public void MarkStep62StaticMapDurablyWritten()
    {
        if (!_step62PushMapped || string.IsNullOrWhiteSpace(_step62StaticMap))
            throw new InvalidOperationException("Step 62.0 Push frontier map is incomplete.");
        _step62StaticMapDurablyWritten = true;
    }

    public void MarkStep63StaticMapDurablyWritten()
    {
        if (!_step63AdmissionPassed || string.IsNullOrWhiteSpace(_step63StaticMap))
            throw new InvalidOperationException("Step 63.0 frozen character-select admission map is incomplete.");
        _step63StaticMapDurablyWritten = true;
    }

    public void MarkStep64StaticMapDurablyWritten()
    {
        if (string.IsNullOrWhiteSpace(_step64StaticMap))
            throw new InvalidOperationException("Step 64.0 frame/input map is absent.");
        _step64StaticMapDurablyWritten = true;
    }

    // STEP 58 — isolate the exact GetSubmenuType<NCharacterSelectScreen>() factory used by OpenCharacterSelect.

    public TransformedRealStS2StartupLadderGateResult RunStep58ClosedStep57Authority(bool renderingStopped, Action<string>? checkpoint = null)
    {
        const int step = 58;
        const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.PrerequisiteAuthority;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep58Prerequisite("Step 58 Gate A entry");
            if (!renderingStopped)
                throw new InvalidOperationException("Step 58.0 is non-invoking and requires rendering frozen.");
            var selected = RequireStartupLadderSelectedAuthority(step);
            _step58Baseline = CaptureStartupLadderBaseline(selected.Path, selected.Sha256, context);
            RequireStartupLadderBaselineUnchanged(context, _step58Baseline, "Step 58 Gate A");
            RequireVisibleSingleplayerSubmenu(step);
            RequireRetainedCharacterSelectPackedScene(step);
            Checkpoint(checkpoint, "M58_A_PASS — Step-57 4/4 exact retained PackedScene/PCK authority retained; NSingleplayerSubmenu visible; renderer stopped; character-select factory not invoked.");
            return StartupLadderPass(step, Step58Name, gate,
                "Physically closed Step-57 authority retained. The exact generic character-select submenu factory may now be isolated without invoking it.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"M58_A_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return StartupLadderFail(step, Step58Name, gate, stage, ex);
        }
    }

    public TransformedRealStS2StartupLadderGateResult RunStep58CharacterSelectFactoryBinding(Action<string>? checkpoint = null)
    {
        const int step = 58;
        const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.StaticAuditOrBinding;
        var stage = "initialization";
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
            var genericCall = FindExactCharacterSelectFactoryCall(open, step);
            var concreteFactory = RequireConcreteCharacterSelectFactoryDefinition(allTypes, step);
            _step58FactoryMethodToken = concreteFactory.MetadataToken.ToUInt32();

            var characterType = RequireStartupLadderType(allTypes, CharacterSelectScreenManagedTypeFullName, step);
            var stackType = RequireStartupLadderType(allTypes, MainMenuSubmenuStackManagedTypeFullName, step);
            var cacheField = RequireUniqueExactInstanceField(stackType, MainMenuCharacterSelectSubmenuFieldName, CharacterSelectScreenManagedTypeFullName, step);
            var sceneField = RequireUniqueExactInstanceField(stackType, MainMenuCharacterSelectSceneFieldName, PackedSceneManagedTypeFullName, step);

            var runtimeStack = _step54SubmenuStack ?? throw new InvalidOperationException("Step 58.0 requires retained NMainMenuSubmenuStack.");
            var runtimeCharacterType = RequireAdmission().Assembly.GetType(CharacterSelectScreenManagedTypeFullName, true, false)!;
            var runtimeFactory = RequireRuntimeGenericMethodDefinitionByToken(runtimeStack.GetType(), _step58FactoryMethodToken, GetSubmenuTypeMethodName, step);
            var closedRuntimeFactory = runtimeFactory.MakeGenericMethod(runtimeCharacterType);
            if (closedRuntimeFactory.GetParameters().Length != 0 || closedRuntimeFactory.ReturnType != runtimeCharacterType)
                throw new InvalidDataException($"Step 58.0 closed runtime factory signature drifted: {closedRuntimeFactory}.");
            _step58RuntimeFactoryDefinition = runtimeFactory;

            var runtimeCache = RequireRuntimeExactInstanceField(runtimeStack.GetType(), MainMenuCharacterSelectSubmenuFieldName, CharacterSelectScreenManagedTypeFullName, step);
            var cachedScreen = runtimeCache.GetValue(runtimeStack);
            var cacheState = "NULL";
            if (cachedScreen is not null)
            {
                if (cachedScreen.GetType().FullName != CharacterSelectScreenManagedTypeFullName)
                    throw new InvalidDataException($"Step 58.0 pre-existing character-select cache has unexpected type {cachedScreen.GetType().FullName}.");
                if (!ReferenceEquals(AssemblyLoadContext.GetLoadContext(cachedScreen.GetType().Assembly), context))
                    throw new InvalidDataException("Step 58.0 pre-existing character-select cache is not owned by the exact private load context.");
                if (RequireZeroArgBoolMethod(cachedScreen.GetType(), "IsInsideTree").Invoke(cachedScreen, null) is not false)
                    throw new InvalidDataException("Step 58.0 pre-existing character-select cache is already inside the SceneTree; stop before any new character-select operation.");
                var cachedStackField = RequireRuntimeExactInstanceField(cachedScreen.GetType(), SubmenuStackFieldName, SubmenuStackManagedTypeFullName, step);
                if (!ReferenceEquals(cachedStackField.GetValue(cachedScreen), runtimeStack))
                    throw new InvalidDataException("Step 58.0 pre-existing character-select cache does not retain the exact submenu stack in NSubmenu._stack.");
                _step58CharacterSelectCachePreexisting = true;
                _step59CharacterSelectScreen = cachedScreen;
                cacheState = "EXISTING_OFF_TREE";
            }
            else
            {
                _step58CharacterSelectCachePreexisting = false;
                _step59CharacterSelectScreen = null;
            }
            var runtimeScene = RequireRuntimeExactInstanceField(runtimeStack.GetType(), MainMenuCharacterSelectSceneFieldName, PackedSceneManagedTypeFullName, step);
            var packedScene = runtimeScene.GetValue(runtimeStack) ?? throw new InvalidDataException("Step 58.0 retained character-select PackedScene is null.");
            var resourcePath = RequireRuntimeStringProperty(packedScene, GodotResourcePathPropertyName, step).Replace('\\', '/');
            if (!string.Equals(resourcePath, CharacterSelectSceneResourcePath, StringComparison.Ordinal))
                throw new InvalidDataException($"Step 58.0 retained character-select ResourcePath drifted: {resourcePath}.");
            if (resolver.Requests.Count != 0)
                throw new InvalidDataException("Step 58.0 binding unexpectedly attempted external Cecil resolution: " + string.Join(" | ", resolver.Requests));

            _step58StaticMap =
                "StS2 Launcher — Step 58.0 exact character-select factory frontier\n" +
                "Evidence-only. GetSubmenuType<NCharacterSelectScreen>() is not invoked by Step 58.\n" +
                $"Selected compatibility SHA-256: {baseline.SelectedSha256}\n" +
                $"OpenCharacterSelect generic call: {genericCall.FullName}\n" +
                $"Concrete runtime factory token: 0x{_step58FactoryMethodToken:X8}; {concreteFactory.FullName}\n" +
                $"Character-select cache field: {cacheField.FullName}; initial runtime value={cacheState}\n" +
                $"Character-select scene field: {sceneField.FullName}; ResourcePath={resourcePath}\n" +
                "Rendering restarted: NO\nFactory invoked: NO\n" +
                "[CONCRETE FACTORY IL]\n" + string.Join("\n", concreteFactory.Body.Instructions.Select(instruction => "  " + FormatStep41Instruction(instruction))) + "\n";
            RequireStartupLadderBaselineUnchanged(context, baseline, "Step 58 Gate B");
            Checkpoint(checkpoint, $"M58_B_PASS — exact generic factory isolated from OpenCharacterSelect; concreteToken=0x{_step58FactoryMethodToken:X8}; cache={cacheState}; scenePath='{resourcePath}'; invocation=NO; externalResolution=0.");
            return StartupLadderPass(step, Step58Name, gate,
                "Exact concrete GetSubmenuType<T> implementation and current character-select cache state were bound; the retained PackedScene identity still matches Step 57. A pre-existing cache is accepted only when it is the exact off-tree retained NCharacterSelectScreen. No factory invocation occurred.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"M58_B_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return StartupLadderFail(step, Step58Name, gate, stage, ex);
        }
    }

    public TransformedRealStS2StartupLadderGateResult RunStep58CharacterSelectFactoryFrontierMap(Action<string>? checkpoint = null)
    {
        const int step = 58;
        const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.ControlledAction;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep58Prerequisite("Step 58 Gate C entry");
            var baseline = _step58Baseline ?? throw new InvalidOperationException("Step 58.0 baseline absent.");
            using var resolver = new RejectingAssemblyResolver();
            using var module = OpenStartupLadderModule(baseline.SelectedPath, resolver);
            var allTypes = EnumerateTypes(module.Types).ToDictionary(type => type.FullName, StringComparer.Ordinal);
            var allMethods = BuildStartupLadderMethodMap(allTypes);
            var factory = FindMethodByToken(module, _step58FactoryMethodToken);
            RequireCharacterSelectFactoryShape(factory, step);
            var audit = AuditStartupLadderInvocationFrontier([factory], allTypes, allMethods,
                _step48LifecycleGuardAuthority ?? throw new InvalidOperationException("Step 58.0 requires retained Step-48 runtime guards."));
            if (audit.UnresolvedSameAssemblyReferences.Length != 0)
                throw new InvalidDataException("Step 58.0 factory frontier has unresolved same-sts2 references: " + string.Join(" | ", audit.UnresolvedSameAssemblyReferences));
            if (resolver.Requests.Count != 0)
                throw new InvalidDataException("Step 58.0 factory audit attempted external Cecil resolution: " + string.Join(" | ", resolver.Requests));
            var instantiateRefs = factory.Body.Instructions
                .Where(instruction => instruction.OpCode.Code is Code.Call or Code.Callvirt)
                .Select(instruction => instruction.Operand)
                .OfType<MethodReference>()
                .Where(method => method.Name == "Instantiate" && method.DeclaringType.FullName == PackedSceneManagedTypeFullName)
                .ToArray();
            if (instantiateRefs.Length != 1)
                throw new InvalidDataException($"Step 58.0 requires exactly one PackedScene.Instantiate call in concrete character-select factory; observed={instantiateRefs.Length}.");
            var sceneFieldRefs = factory.Body.Instructions.Select(instruction => instruction.Operand).OfType<FieldReference>()
                .Count(field => field.Name == MainMenuCharacterSelectSceneFieldName && field.DeclaringType.FullName == MainMenuSubmenuStackManagedTypeFullName);
            var cacheFieldRefs = factory.Body.Instructions.Select(instruction => instruction.Operand).OfType<FieldReference>()
                .Count(field => field.Name == MainMenuCharacterSelectSubmenuFieldName && field.DeclaringType.FullName == MainMenuSubmenuStackManagedTypeFullName);
            if (sceneFieldRefs == 0 || cacheFieldRefs == 0)
                throw new InvalidDataException($"Step 58.0 concrete factory must reference both character-select scene/cache fields; sceneRefs={sceneFieldRefs}; cacheRefs={cacheFieldRefs}.");
            _step58StaticMap += BuildStartupLadderInvocationFrontierAppendix(audit) +
                $"\n[CHARACTER-SELECT FACTORY SHAPE]\n  PackedScene.Instantiate refs: {instantiateRefs.Length}\n  _characterSelectScreenScene refs: {sceneFieldRefs}\n  _characterSelectSubmenu refs: {cacheFieldRefs}\n";
            _step58FactoryMapped = true;
            RequireStartupLadderBaselineUnchanged(context, baseline, "Step 58 Gate C");
            Checkpoint(checkpoint, $"M58_C_PASS — concrete factory frontier mapped; closureMethods={audit.ImmediateClosureMethods.Length}; boundaries={audit.ImmediateBoundaries.Length}; guarded={audit.GuardedMethodFrontiers.Length}; deferred={audit.DeferredMethodFrontiers.Length}; instantiateRefs=1; scene/cache refs={sceneFieldRefs}/{cacheFieldRefs}; invocation=NO.");
            return StartupLadderPass(step, Step58Name, gate,
                "Concrete character-select factory implementation is mapped with one PackedScene.Instantiate reference and exact retained scene/cache fields. No factory invocation occurred.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"M58_C_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return StartupLadderFail(step, Step58Name, gate, stage, ex);
        }
    }

    public TransformedRealStS2StartupLadderGateResult RunStep58FrozenNoCreationConfinement(bool renderingStopped, Action<string>? checkpoint = null)
    {
        const int step = 58;
        const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.PostActionConfinement;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep58Prerequisite("Step 58 Gate D entry");
            var baseline = _step58Baseline ?? throw new InvalidOperationException("Step 58.0 baseline absent.");
            if (!_step58FactoryMapped || !_step58StaticMapDurablyWritten || !renderingStopped)
                throw new InvalidOperationException("Step 58.0 Gate D requires durable non-invoking factory evidence with rendering frozen.");
            var stack = _step54SubmenuStack ?? throw new InvalidOperationException("Step 58.0 retained submenu stack absent.");
            var cache = RequireRuntimeExactInstanceField(stack.GetType(), MainMenuCharacterSelectSubmenuFieldName, CharacterSelectScreenManagedTypeFullName, step);
            var currentCache = cache.GetValue(stack);
            if (_step58CharacterSelectCachePreexisting)
            {
                var retained = RequireRetainedOffTreeCharacterSelect(step);
                if (!ReferenceEquals(currentCache, retained))
                    throw new InvalidDataException("Step 58.0 pre-existing character-select cache identity changed before Step 59 authorization.");
            }
            else if (currentCache is not null)
            {
                throw new InvalidDataException("Step 58.0 character-select cache changed from null before Step 59 authorization.");
            }
            RequireStartupLadderBaselineUnchanged(context, baseline, "Step 58 Gate D");
            RequireVisibleSingleplayerSubmenu(step);
            _exactStep58ClosurePassed = true;
            Checkpoint(checkpoint, $"M58_D_PASS — factory evidence durable; character-select cache={(_step58CharacterSelectCachePreexisting ? "EXISTING_OFF_TREE" : "NULL")}; factory invocation=NO; renderingStopped=True; drift=0.");
            return StartupLadderPass(step, Step58Name, gate,
                "Character-select factory frontier closed 4/4 without creation. Step 59 will reuse the exact pre-existing off-tree cache when present, otherwise it may invoke only the exact closed generic factory once while frozen.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"M58_D_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return StartupLadderFail(step, Step58Name, gate, stage, ex);
        }
    }

    // STEP 59 — acquire the exact retained off-tree NCharacterSelectScreen. Reuse a pre-existing exact cache; invoke the factory only when the cache is null.

    public TransformedRealStS2StartupLadderGateResult RunStep59ClosedStep58Authority(bool renderingStopped, Action<string>? checkpoint = null)
    {
        const int step = 59;
        const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.PrerequisiteAuthority;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep59Prerequisite("Step 59 Gate A entry");
            if (!renderingStopped)
                throw new InvalidOperationException("Step 59.0 requires rendering frozen before off-tree acquisition; a factory call is one-shot only on the null-cache path.");
            var selected = RequireStartupLadderSelectedAuthority(step);
            _step59Baseline = CaptureStartupLadderBaseline(selected.Path, selected.Sha256, context);
            RequireStartupLadderBaselineUnchanged(context, _step59Baseline, "Step 59 Gate A");
            var stack = _step54SubmenuStack ?? throw new InvalidOperationException("Step 59.0 retained submenu stack absent.");
            var cache = RequireRuntimeExactInstanceField(stack.GetType(), MainMenuCharacterSelectSubmenuFieldName, CharacterSelectScreenManagedTypeFullName, step);
            var currentCache = cache.GetValue(stack);
            if (_step58CharacterSelectCachePreexisting)
            {
                var retained = RequireRetainedOffTreeCharacterSelect(step);
                if (!ReferenceEquals(currentCache, retained))
                    throw new InvalidDataException("Step 59.0 pre-existing character-select cache identity drifted after Step 58.");
            }
            else if (currentCache is not null)
            {
                throw new InvalidDataException("Step 59.0 character-select cache became non-null after Step 58 without authorization.");
            }
            Checkpoint(checkpoint, $"M59_A_PASS — Step-58 4/4 factory authority retained; cache={(_step58CharacterSelectCachePreexisting ? "EXISTING_OFF_TREE" : "NULL")}; renderingStopped=True; factory invocation not armed.");
            return StartupLadderPass(step, Step59Name, gate,
                _step58CharacterSelectCachePreexisting
                    ? "Exact pre-existing off-tree NCharacterSelectScreen cache authority retained with frozen rendering; Step 59 will audit/adopt it without invoking the factory."
                    : "Exact non-invoking factory authority retained with null character-select cache and frozen rendering.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"M59_A_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return StartupLadderFail(step, Step59Name, gate, stage, ex);
        }
    }

    public TransformedRealStS2StartupLadderGateResult RunStep59CharacterSelectCreationBinding(Action<string>? checkpoint = null)
    {
        const int step = 59;
        const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.StaticAuditOrBinding;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep59Prerequisite("Step 59 Gate B entry");
            var baseline = _step59Baseline ?? throw new InvalidOperationException("Step 59.0 Gate A must pass before Gate B.");
            var stack = _step54SubmenuStack ?? throw new InvalidOperationException("Step 59.0 retained submenu stack absent.");
            var runtimeFactory = _step58RuntimeFactoryDefinition ?? throw new InvalidOperationException("Step 59.0 retained Step-58 runtime factory definition absent.");
            if ((uint)runtimeFactory.MetadataToken != _step58FactoryMethodToken)
                throw new InvalidDataException("Step 59.0 runtime factory token drifted from Step 58.");
            var characterType = RequireAdmission().Assembly.GetType(CharacterSelectScreenManagedTypeFullName, true, false)!;
            var closed = runtimeFactory.MakeGenericMethod(characterType);
            var cache = RequireRuntimeExactInstanceField(stack.GetType(), MainMenuCharacterSelectSubmenuFieldName, CharacterSelectScreenManagedTypeFullName, step);
            var currentCache = cache.GetValue(stack);
            if (_step58CharacterSelectCachePreexisting)
            {
                var retained = RequireRetainedOffTreeCharacterSelect(step);
                if (!ReferenceEquals(currentCache, retained))
                    throw new InvalidDataException("Step 59.0 pre-existing character-select cache identity drifted before acquisition audit.");
            }
            else if (currentCache is not null)
            {
                throw new InvalidDataException("Step 59.0 character-select cache became non-null before authorization.");
            }
            var acquisitionMode = _step58CharacterSelectCachePreexisting ? "REUSE_EXISTING_OFF_TREE_CACHE" : "FACTORY_IF_NULL";
            _step59StaticMap =
                "StS2 Launcher — Step 59.0 frozen character-select off-tree acquisition\n" +
                "Pre-action binding map; actual hierarchy/lifecycle evidence is appended after acquisition.\n" +
                $"Selected compatibility SHA-256: {baseline.SelectedSha256}\n" +
                $"Closed runtime factory: {closed}\n" +
                $"Factory token: 0x{_step58FactoryMethodToken:X8}\n" +
                $"Acquisition mode: {acquisitionMode}\n" +
                $"Cache before Gate C: {(_step58CharacterSelectCachePreexisting ? "EXISTING_OFF_TREE" : "NULL")}\n" +
                "Rendering restarted: NO\nInitializeSingleplayer invoked: NO\nPush invoked: NO\n";
            RequireStartupLadderBaselineUnchanged(context, baseline, "Step 59 Gate B");
            Checkpoint(checkpoint, $"M59_B_PASS — exact closed runtime factory rebound token=0x{_step58FactoryMethodToken:X8}; acquisitionMode={acquisitionMode}; renderingStopped=True; factoryInvocation=NO.");
            return StartupLadderPass(step, Step59Name, gate,
                _step58CharacterSelectCachePreexisting
                    ? "Exact pre-existing off-tree NCharacterSelectScreen cache rebound and will be adopted without factory invocation at Gate C."
                    : "Exact closed GetSubmenuType<NCharacterSelectScreen>() runtime method rebound with cache still null. One-shot creation is not armed until Gate C.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"M59_B_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return StartupLadderFail(step, Step59Name, gate, stage, ex);
        }
    }

    public TransformedRealStS2StartupLadderGateResult RunStep59FrozenCharacterSelectCreation(Action<string>? checkpoint = null)
    {
        const int step = 59;
        const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.ControlledAction;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep59Prerequisite("Step 59 Gate C entry");
            var baseline = _step59Baseline ?? throw new InvalidOperationException("Step 59.0 baseline absent.");
            if (_step59CreationStarted)
                throw new InvalidOperationException("Step 59.0 character-select factory boundary is one-shot in-process.");
            var stack = _step54SubmenuStack ?? throw new InvalidOperationException("Step 59.0 retained submenu stack absent.");
            var runtimeFactory = _step58RuntimeFactoryDefinition ?? throw new InvalidOperationException("Step 59.0 runtime factory definition absent.");
            var characterType = RequireAdmission().Assembly.GetType(CharacterSelectScreenManagedTypeFullName, true, false)!;
            var closed = runtimeFactory.MakeGenericMethod(characterType);
            var cache = RequireRuntimeExactInstanceField(stack.GetType(), MainMenuCharacterSelectSubmenuFieldName, CharacterSelectScreenManagedTypeFullName, step);
            RequireStep48LifecycleRuntimeGuardsCurrent(context, "Step 59 Gate C immediately before acquisition", requirePreAdmissionCounts: false);
            var initializerBefore = context.InitializerBearingRequests.Count;
            var rejectedBefore = context.RejectedManagedRequests.Count;
            var nativeBefore = context.NativeLoadAttempts.Count;
            object created;
            if (_step58CharacterSelectCachePreexisting)
            {
                stage = "frozen pre-existing character-select cache adoption";
                created = RequireRetainedOffTreeCharacterSelect(step);
                Checkpoint(checkpoint, "M59_C_EXISTING_CACHE_ADOPTED — exact pre-existing off-tree NCharacterSelectScreen cache adopted without invoking GetSubmenuType. InitializeSingleplayer, Push, OpenCharacterSelect, and rendering remain unauthorized.");
            }
            else
            {
                if (cache.GetValue(stack) is not null)
                    throw new InvalidDataException("Step 59.0 cache must still be null immediately before the one-shot factory call.");
                _step59CreationStarted = true;
                Checkpoint(checkpoint, "M59_C_FACTORY_ARMED — first/only GetSubmenuType<NCharacterSelectScreen>() invocation authorized while renderer remains frozen. No InitializeSingleplayer, Push, or OpenCharacterSelect call is authorized.");
                stage = "frozen character-select factory invocation";
                created = InvokeStartupLadderMethod(closed, stack, null, "Step 59 GetSubmenuType<NCharacterSelectScreen>")
                    ?? throw new InvalidDataException("Step 59.0 character-select factory returned null.");
            }
            if (created.GetType().FullName != CharacterSelectScreenManagedTypeFullName)
                throw new InvalidDataException($"Step 59.0 acquired unexpected type {created.GetType().FullName}.");
            if (!ReferenceEquals(AssemblyLoadContext.GetLoadContext(created.GetType().Assembly), context))
                throw new InvalidDataException("Step 59.0 character-select instance is not owned by the exact private load context.");
            if (!ReferenceEquals(cache.GetValue(stack), created))
                throw new InvalidDataException("Step 59.0 acquired character-select instance is not the exact stack cache object.");
            if (RequireZeroArgBoolMethod(created.GetType(), "IsInsideTree").Invoke(created, null) is not false)
                throw new InvalidDataException("Step 59.0 character-select root unexpectedly entered the SceneTree during acquisition.");
            var stackField = RequireRuntimeExactInstanceField(created.GetType(), SubmenuStackFieldName, SubmenuStackManagedTypeFullName, step);
            if (!ReferenceEquals(stackField.GetValue(created), stack))
                throw new InvalidDataException("Step 59.0 acquired character-select root does not retain the exact submenu stack in NSubmenu._stack.");
            RequireNoForbiddenStep37Escape(context, initializerBefore, rejectedBefore, nativeBefore, _step58CharacterSelectCachePreexisting ? "Step 59 existing-cache adoption" : "Step 59 factory invocation");
            _step59CharacterSelectScreen = created;
            _step59PostCreationBaseline = CaptureStartupLadderBaseline(baseline.SelectedPath, baseline.SelectedSha256, context);

            stage = "actual off-tree character-select hierarchy/lifecycle audit";
            var godotAssembly = (_callbackHandoff ?? throw new InvalidOperationException("GodotSharp handoff absent.")).GodotSharpAssembly;
            var nodeType = godotAssembly.GetType("Godot.Node", true, false)!;
            var nodes = EnumerateStep39NodeGraph(created, nodeType);
            var managedTypes = GetSelectedManagedNodeTypeNames(nodes, created.GetType().Assembly, context);
            using var resolver = new RejectingAssemblyResolver();
            using var module = OpenStartupLadderModule(baseline.SelectedPath, resolver);
            var allTypes = EnumerateTypes(module.Types).ToDictionary(type => type.FullName, StringComparer.Ordinal);
            var allMethods = BuildStartupLadderMethodMap(allTypes);
            var lifecycleRoots = CollectStartupLadderActualNodeCallbackRoots(managedTypes, allTypes, new[] { "_EnterTree", "_Ready", "_Notification" });
            var audit = AuditStartupLadderInvocationFrontier(lifecycleRoots, allTypes, allMethods,
                _step48LifecycleGuardAuthority ?? throw new InvalidOperationException("Step 59.0 requires retained Step-48 runtime guards."));
            RequireImmediateFrontierAdmissible(audit, "Step 59.0 actual character-select lifecycle frontier under retained runtime guards");
            if (resolver.Requests.Count != 0)
                throw new InvalidDataException("Step 59.0 lifecycle audit attempted external Cecil resolution: " + string.Join(" | ", resolver.Requests));
            _step59StaticMap += $"Factory invoked by Step 59: {(_step58CharacterSelectCachePreexisting ? "NO — pre-existing exact cache reused" : "YES — one-shot cache-null factory path")}\n" +
                BuildCharacterSelectNodeAuditAppendix("ACTUAL OFF-TREE CHARACTER-SELECT HIERARCHY", nodes, managedTypes, lifecycleRoots, audit);
            _step59CreationPassed = true;
            Checkpoint(checkpoint, $"M59_C_PASS — exact character-select root acquired and retained off-tree; source={(_step58CharacterSelectCachePreexisting ? "existing-cache" : "one-shot-factory")}; nodes={nodes.Count}; managedTypes={managedTypes.Length}; lifecycleRoots={lifecycleRoots.Length}; closure={audit.ImmediateClosureMethods.Length}; guarded={audit.GuardedMethodFrontiers.Length}; cacheIdentity=True; stackIdentity=True; insideTree=False; native/context drift=0.");
            return StartupLadderPass(step, Step59Name, gate,
                $"Real NCharacterSelectScreen acquired from the exact game cache/factory path and retained off-tree. Actual lifecycle surface is admissible: nodes={nodes.Count}; managed types={managedTypes.Length}; lifecycle roots={lifecycleRoots.Length}; no native/context escape.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"M59_C_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return StartupLadderFail(step, Step59Name, gate, stage, ex);
        }
    }

    public TransformedRealStS2StartupLadderGateResult RunStep59FrozenOffTreeConfinement(bool renderingStopped, Action<string>? checkpoint = null)
    {
        const int step = 59;
        const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.PostActionConfinement;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep59Prerequisite("Step 59 Gate D entry");
            var post = _step59PostCreationBaseline ?? throw new InvalidOperationException("Step 59.0 post-acquisition baseline absent.");
            if (!_step59CreationPassed || !_step59StaticMapDurablyWritten || !renderingStopped)
                throw new InvalidOperationException("Step 59.0 Gate D requires successful off-tree acquisition, durable map, and frozen rendering.");
            var screen = RequireRetainedOffTreeCharacterSelect(step);
            RequireStartupLadderBaselineUnchanged(context, post, "Step 59 Gate D");
            RequireStep48LifecycleRuntimeGuardsCurrent(context, "Step 59 Gate D", requirePreAdmissionCounts: false);
            _exactStep59ClosurePassed = true;
            Checkpoint(checkpoint, $"M59_D_PASS — NCharacterSelectScreen retained off-tree; type={screen.GetType().FullName}; source={(_step58CharacterSelectCachePreexisting ? "existing-cache" : "one-shot-factory")}; renderingStopped=True; InitializeSingleplayer/Push invocation=NO; drift=0.");
            return StartupLadderPass(step, Step59Name, gate,
                "Frozen off-tree character-select acquisition closed 4/4. Step 60 may isolate InitializeSingleplayer without invoking it.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"M59_D_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return StartupLadderFail(step, Step59Name, gate, stage, ex);
        }
    }

    // STEP 60 — isolate and audit exact NCharacterSelectScreen.InitializeSingleplayer() without invoking it.

    public TransformedRealStS2StartupLadderGateResult RunStep60ClosedStep59Authority(bool renderingStopped, Action<string>? checkpoint = null)
    {
        const int step = 60;
        const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.PrerequisiteAuthority;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep60Prerequisite("Step 60 Gate A entry");
            if (!renderingStopped)
                throw new InvalidOperationException("Step 60.0 is non-invoking and requires rendering frozen.");
            var selected = RequireStartupLadderSelectedAuthority(step);
            _step60Baseline = CaptureStartupLadderBaseline(selected.Path, selected.Sha256, context);
            RequireStartupLadderBaselineUnchanged(context, _step60Baseline, "Step 60 Gate A");
            RequireRetainedOffTreeCharacterSelect(step);
            Checkpoint(checkpoint, "M60_A_PASS — Step-59 4/4 exact off-tree NCharacterSelectScreen authority retained; renderingStopped=True; InitializeSingleplayer not invoked.");
            return StartupLadderPass(step, Step60Name, gate,
                "Off-tree character-select authority retained. The exact InitializeSingleplayer method may be mapped without invocation.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"M60_A_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return StartupLadderFail(step, Step60Name, gate, stage, ex);
        }
    }

    public TransformedRealStS2StartupLadderGateResult RunStep60InitializeSingleplayerBinding(Action<string>? checkpoint = null)
    {
        const int step = 60;
        const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.StaticAuditOrBinding;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep60Prerequisite("Step 60 Gate B entry");
            var baseline = _step60Baseline ?? throw new InvalidOperationException("Step 60.0 Gate A must pass before Gate B.");
            using var resolver = new RejectingAssemblyResolver();
            using var module = OpenStartupLadderModule(baseline.SelectedPath, resolver);
            var allTypes = EnumerateTypes(module.Types).ToDictionary(type => type.FullName, StringComparer.Ordinal);
            var characterType = RequireStartupLadderType(allTypes, CharacterSelectScreenManagedTypeFullName, step);
            var initialize = RequireUniqueStartupLadderNamedMethod(characterType, CharacterSelectInitializeSingleplayerMethodName, step);
            RequireZeroArgVoidMethod(initialize, step, CharacterSelectInitializeSingleplayerMethodName);
            _step60InitializeMethodToken = initialize.MetadataToken.ToUInt32();
            var screen = RequireRetainedOffTreeCharacterSelect(step);
            var runtime = RequireRuntimeDeclaredMethodByToken(screen.GetType(), _step60InitializeMethodToken, CharacterSelectInitializeSingleplayerMethodName, step, parameterCount: 0, returnTypeFullName: "System.Void");
            _step60RuntimeInitializeMethod = runtime;
            if (resolver.Requests.Count != 0)
                throw new InvalidDataException("Step 60.0 binding unexpectedly attempted external Cecil resolution: " + string.Join(" | ", resolver.Requests));
            _step60StaticMap =
                "StS2 Launcher — Step 60.0 exact character-select InitializeSingleplayer frontier\n" +
                "Evidence-only. InitializeSingleplayer is not invoked by Step 60.\n" +
                $"Selected compatibility SHA-256: {baseline.SelectedSha256}\n" +
                $"InitializeSingleplayer token=0x{_step60InitializeMethodToken:X8}; params=0; return=System.Void; IL={initialize.Body.Instructions.Count}\n" +
                "Rendering restarted: NO\nInitializeSingleplayer invoked: NO\nPush invoked: NO\n" +
                "[INITIALIZESINGLEPLAYER IL]\n" + string.Join("\n", initialize.Body.Instructions.Select(instruction => "  " + FormatStep41Instruction(instruction))) + "\n";
            RequireStartupLadderBaselineUnchanged(context, baseline, "Step 60 Gate B");
            Checkpoint(checkpoint, $"M60_B_PASS — exact InitializeSingleplayer bound token=0x{_step60InitializeMethodToken:X8}; IL={initialize.Body.Instructions.Count}; invocation=NO; externalResolution=0.");
            return StartupLadderPass(step, Step60Name, gate,
                "Exact zero-argument void NCharacterSelectScreen.InitializeSingleplayer binding and IL recorded without invocation.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"M60_B_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return StartupLadderFail(step, Step60Name, gate, stage, ex);
        }
    }

    public TransformedRealStS2StartupLadderGateResult RunStep60InitializeSingleplayerFrontierMap(Action<string>? checkpoint = null)
    {
        const int step = 60;
        const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.ControlledAction;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep60Prerequisite("Step 60 Gate C entry");
            var baseline = _step60Baseline ?? throw new InvalidOperationException("Step 60.0 baseline absent.");
            using var resolver = new RejectingAssemblyResolver();
            using var module = OpenStartupLadderModule(baseline.SelectedPath, resolver);
            var allTypes = EnumerateTypes(module.Types).ToDictionary(type => type.FullName, StringComparer.Ordinal);
            var allMethods = BuildStartupLadderMethodMap(allTypes);
            var initialize = FindMethodByToken(module, _step60InitializeMethodToken);
            RequireZeroArgVoidMethod(initialize, step, CharacterSelectInitializeSingleplayerMethodName);
            var audit = AuditStartupLadderInvocationFrontier([initialize], allTypes, allMethods,
                _step48LifecycleGuardAuthority ?? throw new InvalidOperationException("Step 60.0 requires retained Step-48 runtime guards."));
            RequireImmediateFrontierAdmissible(audit, "Step 60.0 exact InitializeSingleplayer frontier under retained runtime guards");
            if (resolver.Requests.Count != 0)
                throw new InvalidDataException("Step 60.0 frontier audit attempted external Cecil resolution: " + string.Join(" | ", resolver.Requests));
            _step60StaticMap += BuildStartupLadderInvocationFrontierAppendix(audit);
            _step60InitializeMapped = true;
            RequireStartupLadderBaselineUnchanged(context, baseline, "Step 60 Gate C");
            Checkpoint(checkpoint, $"M60_C_PASS — InitializeSingleplayer frontier mapped without invocation; closureMethods={audit.ImmediateClosureMethods.Length}; boundaries={audit.ImmediateBoundaries.Length}; guarded={audit.GuardedMethodFrontiers.Length}; deferred={audit.DeferredMethodFrontiers.Length}; unresolved/external=0.");
            return StartupLadderPass(step, Step60Name, gate,
                $"InitializeSingleplayer execution-qualified frontier mapped without invocation: closure={audit.ImmediateClosureMethods.Length}; all immediate boundaries admissible under retained guards; deferred frontiers recorded separately.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"M60_C_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return StartupLadderFail(step, Step60Name, gate, stage, ex);
        }
    }

    public TransformedRealStS2StartupLadderGateResult RunStep60FrozenNoInitializationConfinement(bool renderingStopped, Action<string>? checkpoint = null)
    {
        const int step = 60;
        const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.PostActionConfinement;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep60Prerequisite("Step 60 Gate D entry");
            var baseline = _step60Baseline ?? throw new InvalidOperationException("Step 60.0 baseline absent.");
            if (!_step60InitializeMapped || !_step60StaticMapDurablyWritten || !renderingStopped)
                throw new InvalidOperationException("Step 60.0 Gate D requires durable non-invoking InitializeSingleplayer evidence with rendering frozen.");
            RequireRetainedOffTreeCharacterSelect(step);
            RequireStartupLadderBaselineUnchanged(context, baseline, "Step 60 Gate D");
            _exactStep60ClosurePassed = true;
            Checkpoint(checkpoint, "M60_D_PASS — InitializeSingleplayer frontier durable; invocation=NO; NCharacterSelectScreen remains off-tree; renderingStopped=True; drift=0.");
            return StartupLadderPass(step, Step60Name, gate,
                "InitializeSingleplayer frontier closed 4/4 without invocation. Step 61 may invoke only this exact method once while the screen remains off-tree and rendering frozen.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"M60_D_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return StartupLadderFail(step, Step60Name, gate, stage, ex);
        }
    }

    // STEP 61 — invoke exact InitializeSingleplayer once while the character-select screen remains off-tree.

    public TransformedRealStS2StartupLadderGateResult RunStep61ClosedStep60Authority(bool renderingStopped, Action<string>? checkpoint = null)
    {
        const int step = 61;
        const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.PrerequisiteAuthority;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep61Prerequisite("Step 61 Gate A entry");
            if (!renderingStopped)
                throw new InvalidOperationException("Step 61.0 requires rendering frozen before one-shot initialization.");
            var selected = RequireStartupLadderSelectedAuthority(step);
            _step61Baseline = CaptureStartupLadderBaseline(selected.Path, selected.Sha256, context);
            RequireStartupLadderBaselineUnchanged(context, _step61Baseline, "Step 61 Gate A");
            RequireRetainedOffTreeCharacterSelect(step);
            Checkpoint(checkpoint, "M61_A_PASS — Step-60 4/4 InitializeSingleplayer frontier authority retained; character-select root off-tree; renderingStopped=True; one-shot initialization not armed.");
            return StartupLadderPass(step, Step61Name, gate,
                "Exact InitializeSingleplayer authority retained with the real character-select root still off-tree and rendering frozen.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"M61_A_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return StartupLadderFail(step, Step61Name, gate, stage, ex);
        }
    }

    public TransformedRealStS2StartupLadderGateResult RunStep61InitializationRuntimeBinding(Action<string>? checkpoint = null)
    {
        const int step = 61;
        const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.StaticAuditOrBinding;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep61Prerequisite("Step 61 Gate B entry");
            var baseline = _step61Baseline ?? throw new InvalidOperationException("Step 61.0 Gate A must pass before Gate B.");
            var screen = RequireRetainedOffTreeCharacterSelect(step);
            var runtime = RequireRuntimeDeclaredMethodByToken(screen.GetType(), _step60InitializeMethodToken, CharacterSelectInitializeSingleplayerMethodName, step, 0, "System.Void");
            _step60RuntimeInitializeMethod = runtime;
            _step61StaticMap =
                "StS2 Launcher — Step 61.0 one-shot off-tree InitializeSingleplayer observation\n" +
                $"Selected compatibility SHA-256: {baseline.SelectedSha256}\n" +
                $"Runtime method: {runtime}; token=0x{runtime.MetadataToken:X8}\n" +
                "Character-select inside tree before invocation: NO\nRendering restarted: NO\nPush invoked: NO\n";
            RequireStartupLadderBaselineUnchanged(context, baseline, "Step 61 Gate B");
            Checkpoint(checkpoint, $"M61_B_PASS — exact runtime InitializeSingleplayer rebound token=0x{runtime.MetadataToken:X8}; screen off-tree; renderingStopped=True; invocation=NO.");
            return StartupLadderPass(step, Step61Name, gate,
                "Exact runtime InitializeSingleplayer method rebound against the retained off-tree screen. One-shot invocation remains unarmed until Gate C.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"M61_B_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return StartupLadderFail(step, Step61Name, gate, stage, ex);
        }
    }

    public TransformedRealStS2StartupLadderGateResult RunStep61OffTreeInitialization(Action<string>? checkpoint = null)
    {
        const int step = 61;
        const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.ControlledAction;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep61Prerequisite("Step 61 Gate C entry");
            var baseline = _step61Baseline ?? throw new InvalidOperationException("Step 61.0 baseline absent.");
            if (_step61InitializationStarted)
                throw new InvalidOperationException("Step 61.0 InitializeSingleplayer boundary is one-shot in-process.");
            var screen = RequireRetainedOffTreeCharacterSelect(step);
            var runtime = _step60RuntimeInitializeMethod ?? throw new InvalidOperationException("Step 61.0 runtime InitializeSingleplayer binding absent.");
            RequireStep48LifecycleRuntimeGuardsCurrent(context, "Step 61 Gate C immediately before InitializeSingleplayer", requirePreAdmissionCounts: false);
            var initializerBefore = context.InitializerBearingRequests.Count;
            var rejectedBefore = context.RejectedManagedRequests.Count;
            var nativeBefore = context.NativeLoadAttempts.Count;
            _step61InitializationStarted = true;
            Checkpoint(checkpoint, "M61_C_INITIALIZE_ARMED — first/only NCharacterSelectScreen.InitializeSingleplayer() invocation authorized off-tree with renderer frozen. Push/OpenCharacterSelect/rendering remain unauthorized.");
            stage = "off-tree InitializeSingleplayer invocation";
            InvokeStartupLadderMethod(runtime, screen, null, "Step 61 NCharacterSelectScreen.InitializeSingleplayer");
            if (RequireZeroArgBoolMethod(screen.GetType(), "IsInsideTree").Invoke(screen, null) is not false)
                throw new InvalidDataException("Step 61.0 InitializeSingleplayer unexpectedly admitted character select to the SceneTree.");
            var stack = _step54SubmenuStack ?? throw new InvalidOperationException("Step 61.0 retained submenu stack absent.");
            var cache = RequireRuntimeExactInstanceField(stack.GetType(), MainMenuCharacterSelectSubmenuFieldName, CharacterSelectScreenManagedTypeFullName, step);
            if (!ReferenceEquals(cache.GetValue(stack), screen))
                throw new InvalidDataException("Step 61.0 character-select cache identity drifted during initialization.");
            RequireNoForbiddenStep37Escape(context, initializerBefore, rejectedBefore, nativeBefore, "Step 61 InitializeSingleplayer");
            _step61PostInitializationBaseline = CaptureStartupLadderBaseline(baseline.SelectedPath, baseline.SelectedSha256, context);
            _step61InitializationPassed = true;
            _step61StaticMap +=
                "Initialization returned: YES\n" +
                "Character-select inside tree after invocation: NO\n" +
                "Character-select cache identity retained: YES\n" +
                "Managed resolver/host/private/initializer/rejected/native escape: 0\n";
            Checkpoint(checkpoint, "M61_C_PASS — InitializeSingleplayer returned once; character-select root remains off-tree; cache identity retained; resolver/native escape=0; Push/OpenCharacterSelect/rendering=NO.");
            return StartupLadderPass(step, Step61Name, gate,
                "Exact InitializeSingleplayer executed once off-tree and returned without tree admission or dynamic/native escape. Character-select root remains retained for a separately audited Push boundary.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"M61_C_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return StartupLadderFail(step, Step61Name, gate, stage, ex);
        }
    }

    public TransformedRealStS2StartupLadderGateResult RunStep61FrozenPostInitializationConfinement(bool renderingStopped, Action<string>? checkpoint = null)
    {
        const int step = 61;
        const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.PostActionConfinement;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep61Prerequisite("Step 61 Gate D entry");
            var post = _step61PostInitializationBaseline ?? throw new InvalidOperationException("Step 61.0 post-initialization baseline absent.");
            if (!_step61InitializationPassed || !_step61StaticMapDurablyWritten || !renderingStopped)
                throw new InvalidOperationException("Step 61.0 Gate D requires successful one-shot initialization, durable observation map, and frozen rendering.");
            RequireRetainedOffTreeCharacterSelect(step);
            RequireStartupLadderBaselineUnchanged(context, post, "Step 61 Gate D");
            _exactStep61ClosurePassed = true;
            Checkpoint(checkpoint, "M61_D_PASS — initialized NCharacterSelectScreen retained off-tree; renderingStopped=True; Push invocation=NO; post-initialization drift=0.");
            return StartupLadderPass(step, Step61Name, gate,
                "Off-tree InitializeSingleplayer execution closed 4/4. Step 62 may map the exact NSubmenuStack.Push admission frontier without invoking it.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"M61_D_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return StartupLadderFail(step, Step61Name, gate, stage, ex);
        }
    }

    // STEP 62 — isolate exact NSubmenuStack.Push(NSubmenu) plus actual initialized character-select lifecycle callbacks.

    public TransformedRealStS2StartupLadderGateResult RunStep62ClosedStep61Authority(bool renderingStopped, Action<string>? checkpoint = null)
    {
        const int step = 62;
        const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.PrerequisiteAuthority;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep62Prerequisite("Step 62 Gate A entry");
            if (!renderingStopped)
                throw new InvalidOperationException("Step 62.0 is non-invoking and requires rendering frozen.");
            var selected = RequireStartupLadderSelectedAuthority(step);
            _step62Baseline = CaptureStartupLadderBaseline(selected.Path, selected.Sha256, context);
            RequireStartupLadderBaselineUnchanged(context, _step62Baseline, "Step 62 Gate A");
            RequireRetainedOffTreeCharacterSelect(step);
            Checkpoint(checkpoint, "M62_A_PASS — Step-61 4/4 initialized off-tree NCharacterSelectScreen authority retained; renderingStopped=True; Push not invoked.");
            return StartupLadderPass(step, Step62Name, gate,
                "Initialized off-tree character-select authority retained. The exact Push/tree-admission frontier may now be mapped without invocation.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"M62_A_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return StartupLadderFail(step, Step62Name, gate, stage, ex);
        }
    }

    public TransformedRealStS2StartupLadderGateResult RunStep62PushBinding(Action<string>? checkpoint = null)
    {
        const int step = 62;
        const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.StaticAuditOrBinding;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep62Prerequisite("Step 62 Gate B entry");
            var baseline = _step62Baseline ?? throw new InvalidOperationException("Step 62.0 Gate A must pass before Gate B.");
            using var resolver = new RejectingAssemblyResolver();
            using var module = OpenStartupLadderModule(baseline.SelectedPath, resolver);
            var allTypes = EnumerateTypes(module.Types).ToDictionary(type => type.FullName, StringComparer.Ordinal);
            var submenuType = RequireStartupLadderType(allTypes, SingleplayerSubmenuManagedTypeFullName, step);
            var open = RequireUniqueStartupLadderNamedMethod(submenuType, SingleplayerOpenCharacterSelectMethodName, step);
            var pushReference = FindExactCharacterSelectPushCall(open, step);
            var pushDefinition = pushReference.Resolve() ?? throw new InvalidDataException("Step 62.0 Push reference did not resolve to a MethodDef.");
            RequirePushDefinitionShape(pushDefinition, step);
            _step62PushMethodToken = pushDefinition.MetadataToken.ToUInt32();
            var stack = _step54SubmenuStack ?? throw new InvalidOperationException("Step 62.0 retained submenu stack absent.");
            var runtimePush = RequireRuntimeMethodByToken(stack.GetType(), _step62PushMethodToken, SubmenuStackPushMethodName, step, parameterCount: 1, returnTypeFullName: "System.Void");
            if (runtimePush.GetParameters()[0].ParameterType.FullName != SubmenuManagedTypeFullName)
                throw new InvalidDataException($"Step 62.0 runtime Push parameter drifted: {runtimePush.GetParameters()[0].ParameterType.FullName}.");
            _step62RuntimePushMethod = runtimePush;
            if (resolver.Requests.Count != 0)
                throw new InvalidDataException("Step 62.0 Push binding unexpectedly attempted external Cecil resolution: " + string.Join(" | ", resolver.Requests));
            _step62StaticMap =
                "StS2 Launcher — Step 62.0 exact character-select Push frontier\n" +
                "Evidence-only. NSubmenuStack.Push is not invoked by Step 62.\n" +
                $"Selected compatibility SHA-256: {baseline.SelectedSha256}\n" +
                $"OpenCharacterSelect Push call: {pushReference.FullName}\n" +
                $"Push token=0x{_step62PushMethodToken:X8}; IL={pushDefinition.Body.Instructions.Count}; runtime={runtimePush}\n" +
                "Rendering restarted: NO\nPush invoked: NO\n" +
                "[PUSH IL]\n" + string.Join("\n", pushDefinition.Body.Instructions.Select(instruction => "  " + FormatStep41Instruction(instruction))) + "\n";
            RequireStartupLadderBaselineUnchanged(context, baseline, "Step 62 Gate B");
            Checkpoint(checkpoint, $"M62_B_PASS — exact Push bound token=0x{_step62PushMethodToken:X8}; IL={pushDefinition.Body.Instructions.Count}; invocation=NO; externalResolution=0.");
            return StartupLadderPass(step, Step62Name, gate,
                "Exact NSubmenuStack.Push(NSubmenu) call used by OpenCharacterSelect is rebound to the retained runtime stack without invocation.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"M62_B_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return StartupLadderFail(step, Step62Name, gate, stage, ex);
        }
    }

    public TransformedRealStS2StartupLadderGateResult RunStep62PushAndLifecycleFrontierMap(Action<string>? checkpoint = null)
    {
        const int step = 62;
        const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.ControlledAction;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep62Prerequisite("Step 62 Gate C entry");
            var baseline = _step62Baseline ?? throw new InvalidOperationException("Step 62.0 baseline absent.");
            var screen = RequireRetainedOffTreeCharacterSelect(step);
            using var resolver = new RejectingAssemblyResolver();
            using var module = OpenStartupLadderModule(baseline.SelectedPath, resolver);
            var allTypes = EnumerateTypes(module.Types).ToDictionary(type => type.FullName, StringComparer.Ordinal);
            var allMethods = BuildStartupLadderMethodMap(allTypes);
            var push = FindMethodByToken(module, _step62PushMethodToken);
            RequirePushDefinitionShape(push, step);
            var guards = _step48LifecycleGuardAuthority ?? throw new InvalidOperationException("Step 62.0 requires retained Step-48 runtime guards.");
            var pushAudit = AuditStartupLadderInvocationFrontier([push], allTypes, allMethods, guards);
            RequireImmediateFrontierAdmissible(pushAudit, "Step 62.0 exact Push frontier under retained runtime guards");

            var godotAssembly = (_callbackHandoff ?? throw new InvalidOperationException("GodotSharp handoff absent.")).GodotSharpAssembly;
            var nodeType = godotAssembly.GetType("Godot.Node", true, false)!;
            var nodes = EnumerateStep39NodeGraph(screen, nodeType);
            var managedTypes = GetSelectedManagedNodeTypeNames(nodes, screen.GetType().Assembly, context);
            var lifecycleRoots = CollectStartupLadderActualNodeCallbackRoots(managedTypes, allTypes, new[] { "_EnterTree", "_Ready", "_Notification" });
            var lifecycleAudit = AuditStartupLadderInvocationFrontier(lifecycleRoots, allTypes, allMethods, guards);
            RequireImmediateFrontierAdmissible(lifecycleAudit, "Step 62.0 initialized character-select lifecycle frontier under retained runtime guards");
            if (resolver.Requests.Count != 0)
                throw new InvalidDataException("Step 62.0 Push/lifecycle audit attempted external Cecil resolution: " + string.Join(" | ", resolver.Requests));
            _step62StaticMap += "\n[PUSH EXECUTION FRONTIER]\n" + BuildStartupLadderInvocationFrontierAppendix(pushAudit) +
                BuildCharacterSelectNodeAuditAppendix("INITIALIZED OFF-TREE CHARACTER-SELECT LIFECYCLE PRE-ADMISSION", nodes, managedTypes, lifecycleRoots, lifecycleAudit);
            _step62PushMapped = true;
            RequireStartupLadderBaselineUnchanged(context, baseline, "Step 62 Gate C");
            Checkpoint(checkpoint, $"M62_C_PASS — Push + initialized character-select lifecycle frontiers mapped without invocation; pushClosure={pushAudit.ImmediateClosureMethods.Length}; lifecycleRoots={lifecycleRoots.Length}; lifecycleClosure={lifecycleAudit.ImmediateClosureMethods.Length}; all immediate boundaries admissible; externalResolution=0.");
            return StartupLadderPass(step, Step62Name, gate,
                "Exact Push frontier and the actual initialized character-select lifecycle surface are both admissible under retained runtime guards. Push remains uninvoked.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"M62_C_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return StartupLadderFail(step, Step62Name, gate, stage, ex);
        }
    }

    public TransformedRealStS2StartupLadderGateResult RunStep62FrozenNoAdmissionConfinement(bool renderingStopped, Action<string>? checkpoint = null)
    {
        const int step = 62;
        const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.PostActionConfinement;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep62Prerequisite("Step 62 Gate D entry");
            var baseline = _step62Baseline ?? throw new InvalidOperationException("Step 62.0 baseline absent.");
            if (!_step62PushMapped || !_step62StaticMapDurablyWritten || !renderingStopped)
                throw new InvalidOperationException("Step 62.0 Gate D requires durable Push/lifecycle evidence with rendering frozen.");
            RequireRetainedOffTreeCharacterSelect(step);
            RequireStartupLadderBaselineUnchanged(context, baseline, "Step 62 Gate D");
            _exactStep62ClosurePassed = true;
            Checkpoint(checkpoint, "M62_D_PASS — Push/lifecycle frontier durable; NCharacterSelectScreen remains off-tree; Push invocation=NO; renderingStopped=True; drift=0.");
            return StartupLadderPass(step, Step62Name, gate,
                "Character-select Push frontier closed 4/4 without admission. Step 63 may invoke only the exact Push once while rendering remains frozen.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"M62_D_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return StartupLadderFail(step, Step62Name, gate, stage, ex);
        }
    }

    // STEP 63 — invoke exact Push once while frozen and prove the real character-select screen is admitted/visible.

    public TransformedRealStS2StartupLadderGateResult RunStep63ClosedStep62Authority(bool renderingStopped, Action<string>? checkpoint = null)
    {
        const int step = 63;
        const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.PrerequisiteAuthority;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep63Prerequisite("Step 63 Gate A entry");
            if (!renderingStopped)
                throw new InvalidOperationException("Step 63.0 requires rendering frozen before one-shot Push admission.");
            var selected = RequireStartupLadderSelectedAuthority(step);
            _step63Baseline = CaptureStartupLadderBaseline(selected.Path, selected.Sha256, context);
            RequireStartupLadderBaselineUnchanged(context, _step63Baseline, "Step 63 Gate A");
            RequireRetainedOffTreeCharacterSelect(step);
            Checkpoint(checkpoint, "M63_A_PASS — Step-62 4/4 Push/lifecycle authority retained; character-select root off-tree; renderingStopped=True; Push not armed.");
            return StartupLadderPass(step, Step63Name, gate,
                "Exact Push/lifecycle authority retained with the initialized real character-select root still off-tree and rendering frozen.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"M63_A_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return StartupLadderFail(step, Step63Name, gate, stage, ex);
        }
    }

    public TransformedRealStS2StartupLadderGateResult RunStep63FrozenAdmissionBinding(Action<string>? checkpoint = null)
    {
        const int step = 63;
        const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.StaticAuditOrBinding;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep63Prerequisite("Step 63 Gate B entry");
            var baseline = _step63Baseline ?? throw new InvalidOperationException("Step 63.0 Gate A must pass before Gate B.");
            var stack = _step54SubmenuStack ?? throw new InvalidOperationException("Step 63.0 retained submenu stack absent.");
            var screen = RequireRetainedOffTreeCharacterSelect(step);
            var push = RequireRuntimeMethodByToken(stack.GetType(), _step62PushMethodToken, SubmenuStackPushMethodName, step, 1, "System.Void");
            if (push.GetParameters()[0].ParameterType.FullName != SubmenuManagedTypeFullName || !push.GetParameters()[0].ParameterType.IsAssignableFrom(screen.GetType()))
                throw new InvalidDataException($"Step 63.0 runtime Push parameter is not compatible with retained character-select root: {push}.");
            _step62RuntimePushMethod = push;
            _step63StackChildrenBefore = GetStartupLadderChildCount(stack);
            _step63StaticMap =
                "StS2 Launcher — Step 63.0 frozen character-select SceneTree admission\n" +
                $"Selected compatibility SHA-256: {baseline.SelectedSha256}\n" +
                $"Runtime Push: {push}; token=0x{push.MetadataToken:X8}\n" +
                $"Retained stack children before Push: {_step63StackChildrenBefore}\n" +
                "Character-select inside tree before Push: NO\nRendering restarted: NO\nOpenCharacterSelect invoked: NO\n";
            RequireStartupLadderBaselineUnchanged(context, baseline, "Step 63 Gate B");
            Checkpoint(checkpoint, $"M63_B_PASS — exact runtime Push rebound token=0x{push.MetadataToken:X8}; stackChildrenBefore={_step63StackChildrenBefore}; screenOffTree=True; admission=NO; renderingStopped=True.");
            return StartupLadderPass(step, Step63Name, gate,
                "Exact runtime Push binding and pre-admission stack shape captured. One-shot tree admission remains unarmed until Gate C.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"M63_B_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return StartupLadderFail(step, Step63Name, gate, stage, ex);
        }
    }

    public TransformedRealStS2StartupLadderGateResult RunStep63FrozenCharacterSelectAdmission(Action<string>? checkpoint = null)
    {
        const int step = 63;
        const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.ControlledAction;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep63Prerequisite("Step 63 Gate C entry");
            var baseline = _step63Baseline ?? throw new InvalidOperationException("Step 63.0 baseline absent.");
            if (_step63AdmissionStarted)
                throw new InvalidOperationException("Step 63.0 Push admission boundary is one-shot in-process.");
            var stack = _step54SubmenuStack ?? throw new InvalidOperationException("Step 63.0 retained submenu stack absent.");
            var screen = RequireRetainedOffTreeCharacterSelect(step);
            var push = _step62RuntimePushMethod ?? throw new InvalidOperationException("Step 63.0 runtime Push binding absent.");
            RequireStep48LifecycleRuntimeGuardsCurrent(context, "Step 63 Gate C immediately before Push", requirePreAdmissionCounts: false);
            var initializerBefore = context.InitializerBearingRequests.Count;
            var rejectedBefore = context.RejectedManagedRequests.Count;
            var nativeBefore = context.NativeLoadAttempts.Count;
            _step63AdmissionStarted = true;
            Checkpoint(checkpoint, $"M63_C_PUSH_ARMED — first/only NSubmenuStack.Push(characterSelect) authorized while renderer remains frozen; stackChildrenBefore={_step63StackChildrenBefore}. Automatic audited lifecycle only; OpenCharacterSelect/rendering remain unauthorized.");
            stage = "frozen NSubmenuStack.Push(characterSelect)";
            InvokeStartupLadderMethod(push, stack, new object?[] { screen }, "Step 63 NSubmenuStack.Push(characterSelect)");
            if (RequireZeroArgBoolMethod(screen.GetType(), "IsInsideTree").Invoke(screen, null) is not true)
                throw new InvalidDataException("Step 63.0 Push returned but character-select root reports IsInsideTree=false.");
            var getParent = RequireZeroArgRuntimeMethod(screen.GetType(), "GetParent", step);
            var parent = InvokeStartupLadderMethod(getParent, screen, null, "Step 63 character-select GetParent");
            if (!ReferenceEquals(parent, stack))
                throw new InvalidDataException($"Step 63.0 character-select parent is not the exact retained submenu stack; observed={parent?.GetType().FullName ?? "null"}.");
            if (!RequireRuntimeBoolProperty(screen, "Visible", step) || RequireZeroArgBoolMethod(screen.GetType(), "IsVisibleInTree").Invoke(screen, null) is not true)
                throw new InvalidDataException("Step 63.0 admitted character-select root is not visible/in-tree after Push.");
            var childrenAfter = GetStartupLadderChildCount(stack);
            if (childrenAfter < _step63StackChildrenBefore + 1)
                throw new InvalidDataException($"Step 63.0 Push did not increase retained stack child count as expected; before={_step63StackChildrenBefore}; after={childrenAfter}.");
            RequireNoForbiddenStep37Escape(context, initializerBefore, rejectedBefore, nativeBefore, "Step 63 Push admission");
            _step63PostAdmissionBaseline = CaptureStartupLadderBaseline(baseline.SelectedPath, baseline.SelectedSha256, context);
            _step63AdmissionPassed = true;
            _step63StaticMap +=
                $"Retained stack children after Push: {childrenAfter}\n" +
                "Character-select inside tree after Push: YES\n" +
                "Character-select parent is exact retained stack: YES\n" +
                "Character-select Visible/IsVisibleInTree: YES/YES\n" +
                "Managed resolver/host/private/initializer/rejected/native escape: 0\n";
            Checkpoint(checkpoint, $"M63_C_PASS — exact initialized NCharacterSelectScreen admitted once through Push; parentExactStack=True; visible/inTree=True; stackChildren={_step63StackChildrenBefore}->{childrenAfter}; renderingStopped=True; context/native drift=0.");
            return StartupLadderPass(step, Step63Name, gate,
                "Real initialized NCharacterSelectScreen was admitted once through the exact audited Push boundary and is visible/in-tree while rendering remains frozen.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"M63_C_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return StartupLadderFail(step, Step63Name, gate, stage, ex);
        }
    }

    public TransformedRealStS2StartupLadderGateResult RunStep63FrozenPostAdmissionConfinement(bool renderingStopped, Action<string>? checkpoint = null)
    {
        const int step = 63;
        const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.PostActionConfinement;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep63Prerequisite("Step 63 Gate D entry");
            var post = _step63PostAdmissionBaseline ?? throw new InvalidOperationException("Step 63.0 post-admission baseline absent.");
            if (!_step63AdmissionPassed || !_step63StaticMapDurablyWritten || !renderingStopped)
                throw new InvalidOperationException("Step 63.0 Gate D requires successful frozen admission, durable map, and rendering stopped.");
            RequireVisibleInTreeCharacterSelect(step);
            RequireStartupLadderBaselineUnchanged(context, post, "Step 63 Gate D");
            _exactStep63ClosurePassed = true;
            Checkpoint(checkpoint, "M63_D_PASS — real NCharacterSelectScreen retained visible/in-tree under exact submenu stack; renderingStopped=True; Push one-shot complete; drift=0.");
            return StartupLadderPass(step, Step63Name, gate,
                "Frozen character-select SceneTree admission closed 4/4. Step 64 may audit the actual frame/input surface and perform one bounded render/refreeze residency.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"M63_D_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return StartupLadderFail(step, Step63Name, gate, stage, ex);
        }
    }

    // STEP 64 — audit the actual in-tree character-select frame/input surface, then render once and refreeze.

    public TransformedRealStS2StartupLadderGateResult RunStep64ClosedStep63Authority(bool renderingStopped, Action<string>? checkpoint = null)
    {
        const int step = 64;
        const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.PrerequisiteAuthority;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep64Prerequisite("Step 64 Gate A entry");
            if (!renderingStopped)
                throw new InvalidOperationException("Step 64.0 requires rendering frozen before frame/input audit.");
            var selected = RequireStartupLadderSelectedAuthority(step);
            _step64Baseline = CaptureStartupLadderBaseline(selected.Path, selected.Sha256, context);
            RequireStartupLadderBaselineUnchanged(context, _step64Baseline, "Step 64 Gate A");
            RequireVisibleInTreeCharacterSelect(step);
            Checkpoint(checkpoint, "M64_A_PASS — Step-63 4/4 frozen character-select admission retained; real NCharacterSelectScreen visible/in-tree; renderer stopped; pulse not armed.");
            return StartupLadderPass(step, Step64Name, gate,
                "Frozen real character-select admission retained. Its actual frame/input callback surface may now be audited before any render restart.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"M64_A_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return StartupLadderFail(step, Step64Name, gate, stage, ex);
        }
    }

    public TransformedRealStS2StartupLadderGateResult RunStep64CharacterSelectFrameInputAudit(Action<string>? checkpoint = null)
    {
        const int step = 64;
        const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.StaticAuditOrBinding;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep64Prerequisite("Step 64 Gate B entry");
            var baseline = _step64Baseline ?? throw new InvalidOperationException("Step 64.0 Gate A must pass before Gate B.");
            var screen = RequireVisibleInTreeCharacterSelect(step);
            var godotAssembly = (_callbackHandoff ?? throw new InvalidOperationException("GodotSharp handoff absent.")).GodotSharpAssembly;
            var nodeType = godotAssembly.GetType("Godot.Node", true, false)!;
            var nodes = EnumerateStep39NodeGraph(screen, nodeType);
            var managedTypes = GetSelectedManagedNodeTypeNames(nodes, screen.GetType().Assembly, context);
            using var resolver = new RejectingAssemblyResolver();
            using var module = OpenStartupLadderModule(baseline.SelectedPath, resolver);
            var allTypes = EnumerateTypes(module.Types).ToDictionary(type => type.FullName, StringComparer.Ordinal);
            var allMethods = BuildStartupLadderMethodMap(allTypes);
            var roots = CollectStartupLadderActualNodeCallbackRoots(managedTypes, allTypes, Step40ImmediateFrameMethodNames);
            var audit = AuditStartupLadderInvocationFrontier(roots, allTypes, allMethods,
                _step48LifecycleGuardAuthority ?? throw new InvalidOperationException("Step 64.0 requires retained Step-48 runtime guards."));
            RequireImmediateFrontierAdmissible(audit, "Step 64.0 actual in-tree character-select frame/input frontier under retained runtime guards");
            if (resolver.Requests.Count != 0)
                throw new InvalidDataException("Step 64.0 frame/input audit attempted external Cecil resolution: " + string.Join(" | ", resolver.Requests));
            _step64StaticMap =
                "StS2 Launcher — Step 64.0 actual character-select frame/input render map\n" +
                $"Selected compatibility SHA-256: {baseline.SelectedSha256}\n" +
                BuildCharacterSelectNodeAuditAppendix("ACTUAL IN-TREE CHARACTER-SELECT FRAME/INPUT SURFACE", nodes, managedTypes, roots, audit) +
                "Rendering restarted while map built: NO\nOpenCharacterSelect invoked: NO\n";
            RequireStartupLadderBaselineUnchanged(context, baseline, "Step 64 Gate B");
            Checkpoint(checkpoint, $"M64_B_PASS — inTreeCharacterSelectNodes={nodes.Count}; managedTypes={managedTypes.Length}; frameInputRoots={roots.Length}; closure={audit.ImmediateClosureMethods.Length}; boundaries={audit.ImmediateBoundaries.Length}; guarded={audit.GuardedMethodFrontiers.Length}; deferred={audit.DeferredMethodFrontiers.Length}; all immediate boundaries admissible; rendering remains stopped.");
            return StartupLadderPass(step, Step64Name, gate,
                $"Actual in-tree character-select frame/input surface mapped: nodes={nodes.Count}; managed types={managedTypes.Length}; immediate roots={roots.Length}; closure={audit.ImmediateClosureMethods.Length}; render pulse not yet started.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"M64_B_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return StartupLadderFail(step, Step64Name, gate, stage, ex);
        }
    }

    public void BeginStep64BoundedRenderPulse(Action<string>? checkpoint = null)
    {
        ThrowIfDisposed();
        RequireStep64Prerequisite("Step 64 Gate C pulse start");
        if (!_step64StaticMapDurablyWritten || string.IsNullOrWhiteSpace(_step64StaticMap))
            throw new InvalidOperationException("Step 64.0 requires its exact frame/input map durably written before rendering starts.");
        if (_step64PulseStarted)
            throw new InvalidOperationException("Step 64.0 render pulse is one-shot in-process.");
        _step64PulseStarted = true;
        Checkpoint(checkpoint, $"M64_C_PULSE_ARMED — first/only character-select render residency authorized; requested stop delay={Step64CharacterSelectRenderTargetMilliseconds}ms; evidence ceiling={Step64CharacterSelectRenderEvidenceCeilingMilliseconds}ms. First managed continuation must StopRendering before telemetry.");
    }

    public TransformedRealStS2StartupLadderGateResult RunStep64CharacterSelectRenderPulseEvidence(bool startReturned, bool activeAfterStart, bool stopReturned, bool activeAfterStop, double elapsedMilliseconds, Action<string>? checkpoint = null)
    {
        const int step = 64;
        const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.ControlledAction;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep64Prerequisite("Step 64 Gate C evidence");
            var baseline = _step64Baseline ?? throw new InvalidOperationException("Step 64.0 baseline absent.");
            if (!_step64PulseStarted)
                throw new InvalidOperationException("Step 64.0 render pulse evidence cannot be accepted before the one-shot pulse is armed.");
            if (!startReturned || !activeAfterStart || !stopReturned || activeAfterStop)
                throw new InvalidOperationException($"Step 64.0 render pulse state mismatch: startReturned={startReturned}; activeAfterStart={activeAfterStart}; stopReturned={stopReturned}; activeAfterStop={activeAfterStop}.");
            if (elapsedMilliseconds < Step64CharacterSelectRenderTargetMilliseconds || elapsedMilliseconds > Step64CharacterSelectRenderEvidenceCeilingMilliseconds)
                throw new InvalidOperationException($"Step 64.0 post-stop evidence outside accepted window. target={Step64CharacterSelectRenderTargetMilliseconds}; ceiling={Step64CharacterSelectRenderEvidenceCeilingMilliseconds}; observed={elapsedMilliseconds:F1}.");
            RequireVisibleInTreeCharacterSelect(step);
            RequireStartupLadderBaselineUnchanged(context, baseline, "Step 64 Gate C post-stop");
            _step64PostPulseBaseline = CaptureStartupLadderBaseline(baseline.SelectedPath, baseline.SelectedSha256, context);
            _step64PulsePassed = true;
            Checkpoint(checkpoint, $"M64_C_PASS — character-select render residency completed and synchronously refroze; elapsedMs={elapsedMilliseconds:F1}; start/active/stop/frozen=True/True/True/True; characterSelectVisible=True; drift=0.");
            return StartupLadderPass(step, Step64Name, gate,
                $"Real character-select UI rendered for {elapsedMilliseconds:F1} ms and synchronously refroze inside the accepted evidence window.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"M64_C_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return StartupLadderFail(step, Step64Name, gate, stage, ex);
        }
    }

    public TransformedRealStS2StartupLadderGateResult RunStep64FrozenPostResidencyConfinement(bool renderingStopped, Action<string>? checkpoint = null)
    {
        const int step = 64;
        const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.PostActionConfinement;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep64Prerequisite("Step 64 Gate D entry");
            var post = _step64PostPulseBaseline ?? throw new InvalidOperationException("Step 64.0 post-pulse baseline absent.");
            if (!_step64PulsePassed || !renderingStopped)
                throw new InvalidOperationException("Step 64.0 Gate D requires successful bounded render residency and rendering stopped.");
            RequireVisibleInTreeCharacterSelect(step);
            RequireStartupLadderBaselineUnchanged(context, post, "Step 64 Gate D");
            _exactStep64ClosurePassed = true;
            Checkpoint(checkpoint, "M64_D_PASS — real NCharacterSelectScreen retained visible/in-tree after bounded render residency; renderingStopped=True; post-pulse drift=0.");
            return StartupLadderPass(step, Step64Name, gate,
                "Character-select render residency closed 4/4 with synchronous refreeze and retained visible/in-tree authority. No run-start/embark action is opened by this candidate.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"M64_D_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return StartupLadderFail(step, Step64Name, gate, stage, ex);
        }
    }

    private Step35ExecutionLoadContext RequireStep58Prerequisite(string boundary)
    {
        var context = RequireStep57Prerequisite(boundary);
        if (!_exactStep57ClosurePassed || !_step57PreflightMapped || !_step57StaticMapDurablyWritten)
            throw new InvalidOperationException(boundary + " requires same-process Step-57 4/4 durable retained-PackedScene/PCK character-select preflight authority.");
        return context;
    }

    private Step35ExecutionLoadContext RequireStep59Prerequisite(string boundary)
    {
        var context = RequireStep58Prerequisite(boundary);
        if (!_exactStep58ClosurePassed || !_step58FactoryMapped || !_step58StaticMapDurablyWritten)
            throw new InvalidOperationException(boundary + " requires same-process Step-58 4/4 durable non-invoking character-select factory authority.");
        return context;
    }

    private Step35ExecutionLoadContext RequireStep60Prerequisite(string boundary)
    {
        var context = RequireStep59Prerequisite(boundary);
        if (!_exactStep59ClosurePassed || !_step59CreationPassed || !_step59StaticMapDurablyWritten)
            throw new InvalidOperationException(boundary + " requires same-process Step-59 4/4 exact off-tree NCharacterSelectScreen creation authority.");
        return context;
    }

    private Step35ExecutionLoadContext RequireStep61Prerequisite(string boundary)
    {
        var context = RequireStep60Prerequisite(boundary);
        if (!_exactStep60ClosurePassed || !_step60InitializeMapped || !_step60StaticMapDurablyWritten)
            throw new InvalidOperationException(boundary + " requires same-process Step-60 4/4 durable non-invoking InitializeSingleplayer authority.");
        return context;
    }

    private Step35ExecutionLoadContext RequireStep62Prerequisite(string boundary)
    {
        var context = RequireStep61Prerequisite(boundary);
        if (!_exactStep61ClosurePassed || !_step61InitializationPassed || !_step61StaticMapDurablyWritten)
            throw new InvalidOperationException(boundary + " requires same-process Step-61 4/4 initialized off-tree NCharacterSelectScreen authority.");
        return context;
    }

    private Step35ExecutionLoadContext RequireStep63Prerequisite(string boundary)
    {
        var context = RequireStep62Prerequisite(boundary);
        if (!_exactStep62ClosurePassed || !_step62PushMapped || !_step62StaticMapDurablyWritten)
            throw new InvalidOperationException(boundary + " requires same-process Step-62 4/4 durable Push/lifecycle frontier authority.");
        return context;
    }

    private Step35ExecutionLoadContext RequireStep64Prerequisite(string boundary)
    {
        var context = RequireStep63Prerequisite(boundary);
        if (!_exactStep63ClosurePassed || !_step63AdmissionPassed || !_step63StaticMapDurablyWritten)
            throw new InvalidOperationException(boundary + " requires same-process Step-63 4/4 frozen visible/in-tree NCharacterSelectScreen authority.");
        return context;
    }

    private object RequireRetainedCharacterSelectPackedScene(int step)
    {
        var stack = _step54SubmenuStack ?? throw new InvalidOperationException($"Step {step}.0 retained NMainMenuSubmenuStack is absent.");
        var sceneField = RequireRuntimeExactInstanceField(stack.GetType(), MainMenuCharacterSelectSceneFieldName, PackedSceneManagedTypeFullName, step);
        var packedScene = sceneField.GetValue(stack) ?? throw new InvalidDataException($"Step {step}.0 retained character-select PackedScene is null.");
        var resourcePath = RequireRuntimeStringProperty(packedScene, GodotResourcePathPropertyName, step).Replace('\\', '/');
        if (!string.Equals(resourcePath, CharacterSelectSceneResourcePath, StringComparison.Ordinal))
            throw new InvalidDataException($"Step {step}.0 retained character-select PackedScene path drifted: {resourcePath}.");
        return packedScene;
    }

    private object RequireRetainedOffTreeCharacterSelect(int step)
    {
        var screen = _step59CharacterSelectScreen ?? throw new InvalidOperationException($"Step {step}.0 retained NCharacterSelectScreen is absent.");
        if (screen.GetType().FullName != CharacterSelectScreenManagedTypeFullName)
            throw new InvalidDataException($"Step {step}.0 retained character-select type drifted: {screen.GetType().FullName}.");
        if (RequireZeroArgBoolMethod(screen.GetType(), "IsInsideTree").Invoke(screen, null) is not false)
            throw new InvalidDataException($"Step {step}.0 requires retained NCharacterSelectScreen to remain off-tree.");
        var stack = _step54SubmenuStack ?? throw new InvalidOperationException($"Step {step}.0 retained submenu stack is absent.");
        var cache = RequireRuntimeExactInstanceField(stack.GetType(), MainMenuCharacterSelectSubmenuFieldName, CharacterSelectScreenManagedTypeFullName, step);
        if (!ReferenceEquals(cache.GetValue(stack), screen))
            throw new InvalidDataException($"Step {step}.0 retained character-select cache identity drifted.");
        return screen;
    }

    private object RequireVisibleInTreeCharacterSelect(int step)
    {
        var screen = _step59CharacterSelectScreen ?? throw new InvalidOperationException($"Step {step}.0 retained NCharacterSelectScreen is absent.");
        if (screen.GetType().FullName != CharacterSelectScreenManagedTypeFullName)
            throw new InvalidDataException($"Step {step}.0 retained character-select type drifted: {screen.GetType().FullName}.");
        if (RequireZeroArgBoolMethod(screen.GetType(), "IsInsideTree").Invoke(screen, null) is not true ||
            RequireZeroArgBoolMethod(screen.GetType(), "IsVisibleInTree").Invoke(screen, null) is not true ||
            !RequireRuntimeBoolProperty(screen, "Visible", step))
            throw new InvalidDataException($"Step {step}.0 requires retained visible/in-tree NCharacterSelectScreen authority.");
        var stack = _step54SubmenuStack ?? throw new InvalidOperationException($"Step {step}.0 retained submenu stack is absent.");
        var cache = RequireRuntimeExactInstanceField(stack.GetType(), MainMenuCharacterSelectSubmenuFieldName, CharacterSelectScreenManagedTypeFullName, step);
        if (!ReferenceEquals(cache.GetValue(stack), screen))
            throw new InvalidDataException($"Step {step}.0 retained character-select cache identity drifted.");
        return screen;
    }

    private static GenericInstanceMethod FindExactCharacterSelectFactoryCall(MethodDefinition open, int step)
    {
        var calls = open.Body.Instructions
            .Where(instruction => instruction.OpCode.Code is Code.Call or Code.Callvirt)
            .Select(instruction => instruction.Operand)
            .OfType<GenericInstanceMethod>()
            .Where(method => method.Name == GetSubmenuTypeMethodName && method.GenericArguments.Count == 1 &&
                             method.GenericArguments[0].FullName == CharacterSelectScreenManagedTypeFullName)
            .ToArray();
        return calls.SingleOrDefault()
            ?? throw new InvalidDataException($"Step {step}.0 requires exactly one GetSubmenuType<{CharacterSelectScreenManagedTypeFullName}> call in OpenCharacterSelect; observed={calls.Length}.");
    }

    private static MethodReference FindExactCharacterSelectPushCall(MethodDefinition open, int step)
    {
        var calls = open.Body.Instructions
            .Where(instruction => instruction.OpCode.Code is Code.Call or Code.Callvirt)
            .Select(instruction => instruction.Operand)
            .OfType<MethodReference>()
            .Where(method => method.Name == SubmenuStackPushMethodName && method.Parameters.Count == 1 &&
                             method.Parameters[0].ParameterType.FullName == SubmenuManagedTypeFullName &&
                             method.ReturnType.FullName == "System.Void")
            .ToArray();
        return calls.SingleOrDefault()
            ?? throw new InvalidDataException($"Step {step}.0 requires exactly one void Push({SubmenuManagedTypeFullName}) call in OpenCharacterSelect; observed={calls.Length}.");
    }

    private static MethodDefinition RequireConcreteCharacterSelectFactoryDefinition(IReadOnlyDictionary<string, TypeDefinition> allTypes, int step)
    {
        var stack = RequireStartupLadderType(allTypes, MainMenuSubmenuStackManagedTypeFullName, step);
        var candidates = stack.Methods.Where(method => method.Name == GetSubmenuTypeMethodName && method.GenericParameters.Count == 1 && method.Parameters.Count == 0).ToArray();
        var method = candidates.SingleOrDefault()
            ?? throw new InvalidDataException($"Step {step}.0 requires exactly one concrete {MainMenuSubmenuStackManagedTypeFullName}.{GetSubmenuTypeMethodName}<T>(); observed={candidates.Length}.");
        RequireCharacterSelectFactoryShape(method, step);
        return method;
    }

    private static void RequireCharacterSelectFactoryShape(MethodDefinition method, int step)
    {
        if (method.IsStatic || !method.HasBody || method.GenericParameters.Count != 1 || method.Parameters.Count != 0 || method.ReturnType is not GenericParameter)
            throw new InvalidDataException($"Step {step}.0 character-select factory shape drifted: {method.FullName}.");
    }

    private static FieldDefinition RequireUniqueExactInstanceField(TypeDefinition type, string fieldName, string expectedTypeFullName, int step)
    {
        var fields = type.Fields.Where(field => field.Name == fieldName).ToArray();
        var field = fields.SingleOrDefault()
            ?? throw new InvalidDataException($"Step {step}.0 requires exactly one {type.FullName}.{fieldName}; observed={fields.Length}.");
        if (field.IsStatic || field.FieldType.FullName != expectedTypeFullName)
            throw new InvalidDataException($"Step {step}.0 field {field.FullName} must be instance {expectedTypeFullName}; static={field.IsStatic}; observed={field.FieldType.FullName}.");
        return field;
    }

    private static MethodInfo RequireRuntimeGenericMethodDefinitionByToken(Type runtimeType, uint token, string methodName, int step)
    {
        var methods = EnumerateRuntimeMethods(runtimeType)
            .Where(method => method.Name == methodName && method.IsGenericMethodDefinition && method.GetGenericArguments().Length == 1 && method.GetParameters().Length == 0 && (uint)method.MetadataToken == token)
            .ToArray();
        return methods.SingleOrDefault()
            ?? throw new MissingMethodException(runtimeType.FullName, $"Step {step}.0 {methodName}<T>() token=0x{token:X8}; candidates={string.Join(" | ", EnumerateRuntimeMethods(runtimeType).Where(method => method.Name == methodName).Select(method => $"0x{method.MetadataToken:X8}:{method}"))}");
    }

    private static MethodInfo RequireRuntimeDeclaredMethodByToken(Type runtimeType, uint token, string methodName, int step, int parameterCount, string returnTypeFullName)
    {
        var methods = runtimeType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
            .Where(method => method.Name == methodName && !method.IsGenericMethod && method.GetParameters().Length == parameterCount && method.ReturnType.FullName == returnTypeFullName && (uint)method.MetadataToken == token)
            .ToArray();
        return methods.SingleOrDefault()
            ?? throw new MissingMethodException(runtimeType.FullName, $"Step {step}.0 {methodName} token=0x{token:X8}");
    }

    private static MethodInfo RequireRuntimeMethodByToken(Type runtimeType, uint token, string methodName, int step, int parameterCount, string returnTypeFullName)
    {
        var methods = EnumerateRuntimeMethods(runtimeType)
            .Where(method => method.Name == methodName && !method.IsGenericMethod && method.GetParameters().Length == parameterCount && method.ReturnType.FullName == returnTypeFullName && (uint)method.MetadataToken == token)
            .ToArray();
        return methods.SingleOrDefault()
            ?? throw new MissingMethodException(runtimeType.FullName, $"Step {step}.0 {methodName} token=0x{token:X8}; candidates={string.Join(" | ", EnumerateRuntimeMethods(runtimeType).Where(method => method.Name == methodName).Select(method => $"0x{method.MetadataToken:X8}:{method}"))}");
    }

    private static IEnumerable<MethodInfo> EnumerateRuntimeMethods(Type type)
    {
        for (var current = type; current is not null; current = current.BaseType)
        {
            foreach (var method in current.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                yield return method;
        }
    }

    private static MethodInfo RequireZeroArgRuntimeMethod(Type type, string name, int step)
    {
        var candidates = EnumerateRuntimeMethods(type).Where(method => method.Name == name && !method.IsGenericMethod && method.GetParameters().Length == 0).ToArray();
        return candidates.SingleOrDefault()
            ?? throw new MissingMethodException(type.FullName, $"Step {step}.0 {name}(); candidates={string.Join(" | ", candidates.Select(method => method.ToString()))}");
    }

    private static void RequireZeroArgVoidMethod(MethodDefinition method, int step, string name)
    {
        if (method.IsStatic || !method.HasBody || method.Parameters.Count != 0 || method.ReturnType.FullName != "System.Void")
            throw new InvalidDataException($"Step {step}.0 requires instance zero-arg void {name}; observed={method.FullName}.");
    }

    private static void RequirePushDefinitionShape(MethodDefinition method, int step)
    {
        if (method.IsStatic || !method.HasBody || method.Parameters.Count != 1 || method.Parameters[0].ParameterType.FullName != SubmenuManagedTypeFullName || method.ReturnType.FullName != "System.Void")
            throw new InvalidDataException($"Step {step}.0 requires instance void Push({SubmenuManagedTypeFullName}); observed={method.FullName}.");
    }

    private static string[] GetSelectedManagedNodeTypeNames(IReadOnlyList<Step39NodeObservation> nodes, Assembly selectedAssembly, AssemblyLoadContext context)
        => nodes.Select(item => item.Node.GetType())
            .Where(type => ReferenceEquals(type.Assembly, selectedAssembly) && ReferenceEquals(AssemblyLoadContext.GetLoadContext(type.Assembly), context))
            .Select(type => type.FullName)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Cast<string>()
            .Distinct(StringComparer.Ordinal)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

    private static string BuildCharacterSelectNodeAuditAppendix(
        string heading,
        IReadOnlyList<Step39NodeObservation> nodes,
        IReadOnlyList<string> managedTypes,
        IReadOnlyList<MethodDefinition> roots,
        StartupLadderInvocationFrontierAudit audit)
    {
        var lines = new List<string>
        {
            string.Empty,
            "[" + heading + "]",
            $"Nodes: {nodes.Count}",
            $"Selected managed node types: {managedTypes.Count}",
            $"Immediate callback roots: {roots.Count}",
            $"Invocation-qualified closure methods: {audit.ImmediateClosureMethods.Length}",
            $"Immediate classified boundaries: {audit.ImmediateBoundaries.Length}",
            $"Runtime-guarded immediate frontiers: {audit.GuardedMethodFrontiers.Length}",
            $"Deferred method frontiers: {audit.DeferredMethodFrontiers.Length}",
            "Forbidden immediate boundaries: 0",
            "Unresolved same-sts2 references: 0",
        };
        foreach (var node in nodes)
            lines.Add($"  node: {node.Path} | {node.Node.GetType().FullName}");
        lines.Add("[SELECTED MANAGED TYPES]");
        foreach (var type in managedTypes)
            lines.Add("  - " + type);
        lines.Add("[CALLBACK ROOTS]");
        foreach (var root in roots)
            lines.Add($"  - token=0x{root.MetadataToken.ToUInt32():X8}; {root.FullName}");
        lines.Add(BuildStartupLadderInvocationFrontierAppendix(audit));
        return string.Join("\n", lines) + "\n";
    }
}
