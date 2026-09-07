using System.Reflection;
using System.Runtime.Loader;
using Mono.Cecil;

namespace StS2Launcher.Core;

/// <summary>
/// Step 38.0 boundary. Requires the same-process physically closed Step-37.0.1 4/4 authority.
/// It first maps NGame lifecycle IL/callsites from the exact selected compatibility image and refuses
/// execution if the same-NGame call closure from _EnterTree reaches _Ready/GameStartup/platform/main-menu/
/// deferred initialization. It then re-instantiates the already verified FMOD-neutral game scene off-tree
/// and invokes only NGame._EnterTree() directly once. The node is never inserted into a SceneTree, so Godot
/// cannot automatically invoke _Ready. GameStartup, InitializePlatform, LaunchMainMenu, ExecuteDeferred,
/// Steam initialization, native GDExtension loading, and gameplay remain forbidden.
/// </summary>
public sealed partial class TransformedRealStS2VeryEarlyInitialization
{
    public const string NGameTypeFullName = "MegaCrit.Sts2.Core.Nodes.NGame";
    public const string NGameEnterTreeMethodName = "_EnterTree";
    public const string NGameReadyMethodName = "_Ready";
    public const string NGameGameStartupWrapperMethodName = "GameStartupWrapper";
    public const string NGameGameStartupMethodName = "GameStartup";
    public const string NGameInitializePlatformMethodName = "InitializePlatform";
    public const string NGameLaunchMainMenuMethodName = "LaunchMainMenu";
    public const string NGameLoadDeferredStartupAssetsMethodName = "LoadDeferredStartupAssetsAsync";

    private static readonly HashSet<string> Step38ForbiddenNGameReachableMethods = new(StringComparer.Ordinal)
    {
        NGameReadyMethodName,
        NGameGameStartupWrapperMethodName,
        NGameGameStartupMethodName,
        NGameInitializePlatformMethodName,
        NGameLaunchMainMenuMethodName,
        NGameLoadDeferredStartupAssetsMethodName,
    };

    private bool _exactStep37ClosurePassed;
    private Step38LifecycleStaticAuditSnapshot? _step38LifecycleStaticAudit;
    private object? _step38NGameInstance;
    private MethodInfo? _step38EnterTreeMethod;
    private bool _step38EnterTreeInvocationStarted;
    private bool _step38EnterTreeInvocationPassed;

    public bool ExactStep37ClosurePassed => _exactStep37ClosurePassed;

    public string GetVerifiedNGameLifecycleStaticMap()
        => _step38LifecycleStaticAudit?.StaticMap
           ?? throw new InvalidOperationException("Step 38.0 Gate A has not produced a verified NGame lifecycle static map.");

    private void ResetStep38State()
    {
        _exactStep37ClosurePassed = false;
        _step38LifecycleStaticAudit = null;
        _step38NGameInstance = null;
        _step38EnterTreeMethod = null;
        _step38EnterTreeInvocationStarted = false;
        _step38EnterTreeInvocationPassed = false;
    }

    private void MarkExactStep37ClosurePassed()
    {
        _exactStep37ClosurePassed = true;
        _step38LifecycleStaticAudit = null;
        _step38NGameInstance = null;
        _step38EnterTreeMethod = null;
        _step38EnterTreeInvocationStarted = false;
        _step38EnterTreeInvocationPassed = false;
    }

    public TransformedRealStS2GameLifecycleEntryGateResult RunNGameLifecycleStaticAudit(
        Action<string>? checkpoint = null)
    {
        const TransformedRealStS2GameLifecycleEntryGate gate = TransformedRealStS2GameLifecycleEntryGate.LifecycleStaticAudit;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep38Prerequisite("Step 38 Gate A entry");
            var preflight = _preflight ?? throw new InvalidOperationException("Step 38.0 exact selected-authority preflight is absent.");
            if (!IsModelBootstrapCompatibilityMode)
                throw new InvalidOperationException("Step 38.0 requires the physically proven ModelDb-bootstrap selected authority.");

            var selectedPath = preflight.DiagnosticPath;
            stage = "selected compatibility image re-verification";
            VerifyFileLength(selectedPath, preflight.DiagnosticLength, "Step-38 selected sts2 compatibility image");
            var selectedSha256 = ComputeSha256Hex(selectedPath);
            if (!selectedSha256.Equals(preflight.DiagnosticSha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException($"Step 38.0 selected sts2 compatibility image hash drifted. expected={preflight.DiagnosticSha256}; actual={selectedSha256}.");

            Checkpoint(checkpoint, $"H_A_ENTRY — Step-37.0.1 4/4 same-process authority present; opening exact selected sts2 compatibility image read-only with Cecil; path={selectedPath}; sha256={selectedSha256}. No lifecycle method is invoked in Gate A.");
            stage = "NGame lifecycle Cecil map";
            using var module = ModuleDefinition.ReadModule(selectedPath, new ReaderParameters
            {
                ReadingMode = ReadingMode.Immediate,
                InMemory = true,
            });
            var nGame = module.Types.SingleOrDefault(type => type.FullName == NGameTypeFullName)
                ?? throw new InvalidDataException($"Step 38.0 could not locate exact type {NGameTypeFullName}.");

            var enterTree = RequireLifecycleMethod(nGame, NGameEnterTreeMethodName, 0, "System.Void");
            var ready = RequireLifecycleMethod(nGame, NGameReadyMethodName, 0, "System.Void");
            var gameStartupWrapper = RequireLifecycleMethodByName(nGame, NGameGameStartupWrapperMethodName, 0);
            var gameStartup = RequireLifecycleMethodByName(nGame, NGameGameStartupMethodName, 0);
            var initializePlatform = RequireLifecycleMethodByName(nGame, NGameInitializePlatformMethodName, 0);
            var launchMainMenu = RequireLifecycleMethodByName(nGame, NGameLaunchMainMenuMethodName, 1);
            var deferred = RequireLifecycleMethodByName(nGame, NGameLoadDeferredStartupAssetsMethodName, 0);

            var reachable = ComputeSameNGameReachableMethods(nGame, enterTree, out var forbiddenRefs);
            if (forbiddenRefs.Count != 0)
                throw new InvalidDataException("Step 38.0 _EnterTree same-NGame call closure reaches forbidden later-startup boundary/boundaries: " + string.Join(" | ", forbiddenRefs));

            var oneTimeForbidden = reachable
                .SelectMany(method => method.Body.Instructions)
                .Select(instruction => instruction.Operand)
                .OfType<MethodReference>()
                .Where(reference => reference.DeclaringType.FullName == TargetTypeFullName &&
                                    reference.Name is "ExecuteVeryEarly" or "ExecuteEssential" or "ExecuteDeferred" or "PrewarmJit")
                .Select(reference => reference.FullName)
                .Distinct(StringComparer.Ordinal)
                .ToArray();
            if (oneTimeForbidden.Length != 0)
                throw new InvalidDataException("Step 38.0 _EnterTree same-NGame call closure reaches a forbidden OneTimeInitialization method: " + string.Join(" | ", oneTimeForbidden));

            var staticMap = BuildNGameLifecycleStaticMap(
                selectedPath,
                selectedSha256,
                enterTree,
                ready,
                gameStartupWrapper,
                gameStartup,
                initializePlatform,
                launchMainMenu,
                deferred,
                reachable);
            _step38LifecycleStaticAudit = new Step38LifecycleStaticAuditSnapshot(
                selectedPath,
                selectedSha256,
                enterTree.MetadataToken.ToUInt32(),
                staticMap,
                reachable.Select(method => method.FullName).ToArray());

            RequireNoForbiddenStep37Escape(context, 0, 0, 0, "Step 38 Gate A");
            Checkpoint(checkpoint, $"H_A_PASS — NGame lifecycle map sealed; enterTreeToken=0x{enterTree.MetadataToken.ToUInt32():X8}; sameNGameReachable={reachable.Count}; forbiddenLaterStartupReachable=0; no lifecycle execution/native escape.");
            return LifecyclePass(gate,
                "STEP 38.0 NGAME LIFECYCLE STATIC AUDIT PASSED.\n" +
                $"Selected authority SHA-256: {selectedSha256}\n" +
                $"_EnterTree token: 0x{enterTree.MetadataToken.ToUInt32():X8}\n" +
                $"Same-NGame methods reachable from _EnterTree: {reachable.Count}\n" +
                "Reachable _Ready/GameStartupWrapper/GameStartup/InitializePlatform/LaunchMainMenu/LoadDeferredStartupAssetsAsync: 0\n" +
                "Reachable OneTimeInitialization ExecuteVeryEarly/ExecuteEssential/ExecuteDeferred/PrewarmJit: 0\n" +
                "Lifecycle execution: NO");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"H_A_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return LifecycleFail(gate, stage, ex, includeDiagnostic: false);
        }
    }

    public TransformedRealStS2GameLifecycleEntryGateResult RunNGameOffTreeReinstantiation(
        Action<string>? checkpoint = null)
    {
        const TransformedRealStS2GameLifecycleEntryGate gate = TransformedRealStS2GameLifecycleEntryGate.OffTreeNGameReinstantiation;
        var stage = "initialization";
        object? instance = null;
        try
        {
            ThrowIfDisposed();
            var context = RequireStep38Prerequisite("Step 38 Gate B entry");
            _ = RequireStep38StaticAudit();
            var packed = RequireGameScenePackedResource();
            var handoff = _callbackHandoff ?? throw new InvalidOperationException("Step 38.0 exact GodotSharp handoff disappeared.");
            var godotAssembly = handoff.GodotSharpAssembly;
            var packedSceneType = godotAssembly.GetType("Godot.PackedScene", throwOnError: true, ignoreCase: false)
                ?? throw new MissingMemberException("Godot.PackedScene");
            var nodeType = godotAssembly.GetType("Godot.Node", throwOnError: true, ignoreCase: false)
                ?? throw new MissingMemberException("Godot.Node");

            var initializerBefore = context.InitializerBearingRequests.Count;
            var rejectedBefore = context.RejectedManagedRequests.Count;
            var nativeBefore = context.NativeLoadAttempts.Count;
            var resolverBefore = context.ManagedResolverRequests.Count;
            var hostBefore = context.HostLoads.Count;
            var privateBefore = context.PrivateLoads.Count;

            Checkpoint(checkpoint, "H_B_ENTRY — re-instantiating the already verified FMOD-neutral PackedScene off-tree for the isolated _EnterTree experiment; no lifecycle method is called yet.");
            stage = "PackedScene.Instantiate off-tree";
            var instantiate = packedSceneType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .SingleOrDefault(method =>
                {
                    if (method.Name != "Instantiate" || !nodeType.IsAssignableFrom(method.ReturnType))
                        return false;
                    var parameters = method.GetParameters();
                    return parameters.Length == 1 && parameters[0].ParameterType.IsEnum;
                })
                ?? throw new MissingMethodException("Godot.PackedScene", "Instantiate(GenEditState)");
            var disabled = Enum.ToObject(instantiate.GetParameters()[0].ParameterType, 0);
            try
            {
                instance = instantiate.Invoke(packed.Resource, new[] { disabled });
            }
            catch (TargetInvocationException ex) when (ex.InnerException is not null)
            {
                throw new InvalidOperationException(
                    $"Step 38.0 PackedScene.Instantiate threw {ex.InnerException.GetType().FullName}: {ex.InnerException.Message}\n{FormatExceptionDiagnostic(ex.InnerException)}",
                    ex.InnerException);
            }

            if (instance is null || !nodeType.IsInstanceOfType(instance))
                throw new InvalidDataException($"Step 38.0 PackedScene.Instantiate did not return Godot.Node; observed {instance?.GetType().FullName ?? "<null>"}.");
            var rootType = instance.GetType();
            if (!string.Equals(rootType.FullName, NGameTypeFullName, StringComparison.Ordinal))
                throw new InvalidDataException($"Step 38.0 expected root {NGameTypeFullName}; observed {rootType.FullName ?? "<unknown>"}.");
            if (!ReferenceEquals(AssemblyLoadContext.GetLoadContext(rootType.Assembly), context))
                throw new InvalidDataException("Step 38.0 NGame instance is not owned by the exact Step-35/36/37 private load context.");

            var isInsideTree = RequireZeroArgBoolMethod(rootType, "IsInsideTree");
            if (isInsideTree.Invoke(instance, null) is not false)
                throw new InvalidDataException("Step 38.0 fresh NGame unexpectedly reports IsInsideTree=true before _EnterTree direct invocation.");

            var enterTree = rootType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
                .SingleOrDefault(method => method.Name == NGameEnterTreeMethodName && method.GetParameters().Length == 0 && method.ReturnType == typeof(void))
                ?? throw new MissingMethodException(NGameTypeFullName, "_EnterTree()");
            if ((uint)enterTree.MetadataToken != RequireStep38StaticAudit().EnterTreeToken)
                throw new InvalidDataException($"Step 38.0 reflection _EnterTree token drifted: Cecil=0x{RequireStep38StaticAudit().EnterTreeToken:X8}; reflection=0x{enterTree.MetadataToken:X8}.");

            RequireNoForbiddenStep37Escape(context, initializerBefore, rejectedBefore, nativeBefore, "Step 38 Gate B");
            _step38NGameInstance = instance;
            _step38EnterTreeMethod = enterTree;
            instance = null;
            Checkpoint(checkpoint, $"H_B_PASS — fresh off-tree NGame prepared; enterTreeToken=0x{enterTree.MetadataToken:X8}; insideTree=False; resolverDelta={context.ManagedResolverRequests.Count - resolverBefore}; hostDelta={context.HostLoads.Count - hostBefore}; privateDelta={context.PrivateLoads.Count - privateBefore}; initializerDelta=0; rejectedDelta=0; nativeDelta=0; lifecycleCalls=0.");
            return LifecyclePass(gate,
                "STEP 38.0 OFF-TREE NGAME REINSTANTIATION PASSED.\n" +
                $"Root type: {rootType.FullName}\n" +
                $"_EnterTree token: 0x{enterTree.MetadataToken:X8}\n" +
                "IsInsideTree before invocation: FALSE\n" +
                $"Managed resolver delta: {FormatDelta(context.ManagedResolverRequests.Skip(resolverBefore).ToArray())}\n" +
                $"Host-load delta: {FormatDelta(context.HostLoads.Skip(hostBefore).ToArray())}\n" +
                $"Private-load delta: {FormatDelta(context.PrivateLoads.Skip(privateBefore).ToArray())}\n" +
                "Initializer-bearing/rejected/native delta: 0 / 0 / 0\n" +
                "_EnterTree invoked: NO");
        }
        catch (Exception ex)
        {
            if (instance is not null)
                TryReleaseStep38Instance(instance, checkpoint, "Gate-B failure");
            Checkpoint(checkpoint, $"H_B_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return LifecycleFail(gate, stage, ex, includeDiagnostic: true);
        }
    }

    public TransformedRealStS2GameLifecycleEntryGateResult RunNGameDirectEnterTreeInvocation(
        Action<string>? checkpoint = null)
    {
        const TransformedRealStS2GameLifecycleEntryGate gate = TransformedRealStS2GameLifecycleEntryGate.DirectEnterTreeInvocation;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep38Prerequisite("Step 38 Gate C entry");
            _ = RequireStep38StaticAudit();
            var instance = _step38NGameInstance ?? throw new InvalidOperationException("Step 38.0 Gate B must retain a fresh off-tree NGame instance before Gate C.");
            var enterTree = _step38EnterTreeMethod ?? throw new InvalidOperationException("Step 38.0 Gate B must bind NGame._EnterTree before Gate C.");
            if (_step38EnterTreeInvocationStarted)
                throw new InvalidOperationException("Step 38.0 _EnterTree is one-shot in this process; retry is forbidden after invocation begins.");

            var isInsideTree = RequireZeroArgBoolMethod(instance.GetType(), "IsInsideTree");
            if (isInsideTree.Invoke(instance, null) is not false)
                throw new InvalidDataException("Step 38.0 NGame unexpectedly entered the SceneTree before the direct _EnterTree boundary.");

            var initializerBefore = context.InitializerBearingRequests.Count;
            var rejectedBefore = context.RejectedManagedRequests.Count;
            var nativeBefore = context.NativeLoadAttempts.Count;
            var resolverBefore = context.ManagedResolverRequests.Count;
            var hostBefore = context.HostLoads.Count;
            var privateBefore = context.PrivateLoads.Count;
            _step38EnterTreeInvocationStarted = true;

            Checkpoint(checkpoint, $"H_C_INVOKE_START — invoking exact NGame._EnterTree once by MethodInfo on the off-tree instance; token=0x{enterTree.MetadataToken:X8}; insideTreeBefore=False. No AddChild is performed, so Godot automatic _Ready is not authorized.");
            stage = "direct NGame._EnterTree invocation";
            try
            {
                enterTree.Invoke(instance, null);
            }
            catch (TargetInvocationException ex) when (ex.InnerException is not null)
            {
                var insideAfterFailure = TryReadIsInsideTree(instance);
                Checkpoint(checkpoint, $"H_C_EXCEPTION_CAPTURED — top={ex.GetType().FullName}; base={ex.InnerException.GetBaseException().GetType().FullName}; insideTreeAfterFailure={insideAfterFailure}; resolverDelta={context.ManagedResolverRequests.Count - resolverBefore}; hostDelta={context.HostLoads.Count - hostBefore}; privateDelta={context.PrivateLoads.Count - privateBefore}; initializerDelta={context.InitializerBearingRequests.Count - initializerBefore}; rejectedDelta={context.RejectedManagedRequests.Count - rejectedBefore}; nativeDelta={context.NativeLoadAttempts.Count - nativeBefore}.");
                Checkpoint(checkpoint, $"H_C_EXCEPTION_DETAIL — {SanitizeCheckpoint(FormatExceptionDiagnostic(ex.InnerException))}");
                throw new InvalidOperationException(
                    $"Step 38.0 NGame._EnterTree threw {ex.InnerException.GetType().FullName}: {ex.InnerException.Message}\n{FormatExceptionDiagnostic(ex.InnerException)}",
                    ex.InnerException);
            }

            if (isInsideTree.Invoke(instance, null) is not false)
                throw new InvalidDataException("Step 38.0 direct _EnterTree invocation unexpectedly changed Godot IsInsideTree=true without SceneTree insertion.");
            RequireNoForbiddenStep37Escape(context, initializerBefore, rejectedBefore, nativeBefore, "Step 38 Gate C");
            var state = ReadOneTimeInitializationState(RequireEssentialBinding().StateField);
            if (state != ExpectedStateAfterEssential)
                throw new InvalidDataException($"Step 38.0 OneTimeInitialization state drifted during _EnterTree: expected {ExpectedStateAfterEssential}; observed {state}.");

            _step38EnterTreeInvocationPassed = true;
            Checkpoint(checkpoint, $"H_C_PASS — exact NGame._EnterTree returned; insideTreeAfter=False; state={state}; resolverDelta={context.ManagedResolverRequests.Count - resolverBefore}; hostDelta={context.HostLoads.Count - hostBefore}; privateDelta={context.PrivateLoads.Count - privateBefore}; initializerDelta=0; rejectedDelta=0; nativeDelta=0; _Ready/GameStartup/InitializePlatform/main-menu/ExecuteDeferred remain uninvoked by Step-38 authority.");
            return LifecyclePass(gate,
                "STEP 38.0 DIRECT NGAME._ENTERTREE INVOCATION PASSED.\n" +
                "Invocation count: exactly one\n" +
                "SceneTree AddChild: NO\n" +
                "IsInsideTree after direct invocation: FALSE\n" +
                $"OneTimeInitialization state: {state}\n" +
                $"Managed resolver delta: {FormatDelta(context.ManagedResolverRequests.Skip(resolverBefore).ToArray())}\n" +
                $"Host-load delta: {FormatDelta(context.HostLoads.Skip(hostBefore).ToArray())}\n" +
                $"Private-load delta: {FormatDelta(context.PrivateLoads.Skip(privateBefore).ToArray())}\n" +
                "Initializer-bearing request delta: 0\n" +
                "Rejected managed request delta: 0\n" +
                "Native-load attempt delta: 0\n" +
                "_Ready/GameStartup/InitializePlatform/LaunchMainMenu/ExecuteDeferred: not authorized");
        }
        catch (Exception ex)
        {
            if (_step38NGameInstance is not null)
            {
                TryReleaseStep38Instance(_step38NGameInstance, checkpoint, "Gate-C failure");
                _step38NGameInstance = null;
                _step38EnterTreeMethod = null;
            }
            Checkpoint(checkpoint, $"H_C_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return LifecycleFail(gate, stage, ex, includeDiagnostic: true);
        }
    }

    public TransformedRealStS2GameLifecycleEntryGateResult RunNGamePostEnterTreeConfinementAndRelease(
        Action<string>? checkpoint = null)
    {
        const TransformedRealStS2GameLifecycleEntryGate gate = TransformedRealStS2GameLifecycleEntryGate.PostEnterTreeConfinementAndRelease;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep38Prerequisite("Step 38 Gate D entry");
            if (!_step38EnterTreeInvocationStarted || !_step38EnterTreeInvocationPassed)
                throw new InvalidOperationException("Step 38.0 Gate C must return successfully before Gate D.");
            var instance = _step38NGameInstance ?? throw new InvalidOperationException("Step 38.0 retained NGame instance disappeared before Gate D.");

            Checkpoint(checkpoint, "H_D_ENTRY — performing post-_EnterTree confinement audit while NGame remains off-tree; _ExitTree/_Ready/GameStartup are not called. The temporary node will then be freed.");
            stage = "post-_EnterTree confinement audit";
            if (TryReadIsInsideTree(instance) != "False")
                throw new InvalidDataException($"Step 38.0 post-_EnterTree instance is no longer off-tree; observed IsInsideTree={TryReadIsInsideTree(instance)}.");
            if (context.InitializerBearingRequests.Count != 0 || context.RejectedManagedRequests.Count != 0 || context.NativeLoadAttempts.Count != 0)
                throw new InvalidDataException("Step 38.0 post-_EnterTree confinement detected forbidden initializer-bearing/rejected/native activity. " + context.FormatResolverState());
            var state = ReadOneTimeInitializationState(RequireEssentialBinding().StateField);
            if (state != ExpectedStateAfterEssential)
                throw new InvalidDataException($"Step 38.0 post-_EnterTree state drifted: expected {ExpectedStateAfterEssential}; observed {state}.");

            stage = "off-tree instance release";
            TryReleaseStep38Instance(instance, checkpoint, "Gate-D success");
            _step38NGameInstance = null;
            _step38EnterTreeMethod = null;
            Checkpoint(checkpoint, $"H_D_PASS — post-_EnterTree confinement held; insideTree=False; state={state}; initializer=0; rejected=0; native=0; temporary instance released. _ExitTree/_Ready/GameStartup/InitializePlatform/main-menu/ExecuteDeferred/Steam were not invoked by Step 38.");
            return LifecyclePass(gate,
                "STEP 38.0 POST-_ENTERTREE CONFINEMENT PASSED.\n" +
                "NGame remained off-tree: TRUE\n" +
                $"OneTimeInitialization state: {state}\n" +
                "Initializer-bearing requests: 0\n" +
                "Rejected managed requests: 0\n" +
                "Native-load attempts: 0\n" +
                "Temporary NGame released: TRUE\n" +
                "_ExitTree/_Ready/GameStartup/InitializePlatform/LaunchMainMenu/ExecuteDeferred/Steam: NOT INVOKED");
        }
        catch (Exception ex)
        {
            if (_step38NGameInstance is not null)
            {
                TryReleaseStep38Instance(_step38NGameInstance, checkpoint, "Gate-D failure");
                _step38NGameInstance = null;
                _step38EnterTreeMethod = null;
            }
            Checkpoint(checkpoint, $"H_D_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return LifecycleFail(gate, stage, ex, includeDiagnostic: true);
        }
    }

    private Step35ExecutionLoadContext RequireStep38Prerequisite(string boundary)
    {
        var context = RequireStep37Prerequisite(boundary);
        if (!_exactStep37ClosurePassed)
            throw new InvalidOperationException($"{boundary} requires a successful Step-37.0.1 4/4 closure in this same process.");
        if (_gameScenePackedResource is null)
            throw new InvalidOperationException($"{boundary} requires the physically proven Step-37 PackedScene resource to remain present.");
        return context;
    }

    private Step38LifecycleStaticAuditSnapshot RequireStep38StaticAudit()
        => _step38LifecycleStaticAudit ?? throw new InvalidOperationException("Step 38.0 Gate A must pass before later gates.");

    private static MethodDefinition RequireLifecycleMethod(TypeDefinition type, string name, int parameterCount, string returnType)
    {
        var matches = type.Methods.Where(method => method.Name == name && method.Parameters.Count == parameterCount && method.ReturnType.FullName == returnType).ToArray();
        if (matches.Length != 1 || !matches[0].HasBody || matches[0].IsStatic)
            throw new InvalidDataException($"Step 38.0 expected exactly one instance {type.FullName}::{name} with {parameterCount} parameter(s), return {returnType}, and managed IL body; found {matches.Length}.");
        return matches[0];
    }

    private static MethodDefinition RequireLifecycleMethodByName(TypeDefinition type, string name, int parameterCount)
    {
        var matches = type.Methods.Where(method => method.Name == name && method.Parameters.Count == parameterCount).ToArray();
        if (matches.Length != 1 || !matches[0].HasBody || matches[0].IsStatic)
            throw new InvalidDataException($"Step 38.0 expected exactly one instance {type.FullName}::{name} with {parameterCount} parameter(s) and managed IL body; found {matches.Length}.");
        return matches[0];
    }

    private static List<MethodDefinition> ComputeSameNGameReachableMethods(
        TypeDefinition nGame,
        MethodDefinition root,
        out List<string> forbiddenReferences)
    {
        var result = new List<MethodDefinition>();
        var queue = new Queue<MethodDefinition>();
        var seen = new HashSet<uint>();
        var forbidden = new SortedSet<string>(StringComparer.Ordinal);
        queue.Enqueue(root);
        while (queue.Count != 0)
        {
            var current = queue.Dequeue();
            var token = current.MetadataToken.ToUInt32();
            if (!seen.Add(token))
                continue;
            result.Add(current);
            foreach (var reference in current.Body.Instructions.Select(instruction => instruction.Operand).OfType<MethodReference>())
            {
                if (reference.DeclaringType.FullName != NGameTypeFullName)
                    continue;
                if (Step38ForbiddenNGameReachableMethods.Contains(reference.Name))
                    forbidden.Add($"{current.FullName} -> {reference.FullName}");
                var target = FindSameTypeMethod(nGame, reference);
                if (target is not null && !seen.Contains(target.MetadataToken.ToUInt32()))
                    queue.Enqueue(target);
            }
        }
        forbiddenReferences = forbidden.ToList();
        return result;
    }

    private static MethodDefinition? FindSameTypeMethod(TypeDefinition type, MethodReference reference)
    {
        var matches = type.Methods.Where(method =>
            method.Name == reference.Name &&
            method.Parameters.Count == reference.Parameters.Count &&
            method.ReturnType.FullName == reference.ReturnType.FullName &&
            method.Parameters.Select(parameter => parameter.ParameterType.FullName)
                .SequenceEqual(reference.Parameters.Select(parameter => parameter.ParameterType.FullName), StringComparer.Ordinal)).ToArray();
        return matches.Length == 1 ? matches[0] : null;
    }

    private static string BuildNGameLifecycleStaticMap(
        string selectedPath,
        string selectedSha256,
        MethodDefinition enterTree,
        MethodDefinition ready,
        MethodDefinition gameStartupWrapper,
        MethodDefinition gameStartup,
        MethodDefinition initializePlatform,
        MethodDefinition launchMainMenu,
        MethodDefinition deferred,
        IReadOnlyList<MethodDefinition> reachable)
    {
        var lines = new List<string>
        {
            "StS2 Launcher — Step 38.0 NGame lifecycle static IL/callsite map",
            "Read-only Cecil evidence from the exact physically selected Step-36/37 compatibility authority; never consumed as trusted runtime input.",
            $"Selected path: {selectedPath}",
            $"Selected SHA-256: {selectedSha256}",
            $"NGame type: {NGameTypeFullName}",
            $"_EnterTree: token=0x{enterTree.MetadataToken.ToUInt32():X8}; {enterTree.FullName}",
            $"_Ready: token=0x{ready.MetadataToken.ToUInt32():X8}; {ready.FullName}",
            $"GameStartupWrapper: token=0x{gameStartupWrapper.MetadataToken.ToUInt32():X8}; {gameStartupWrapper.FullName}",
            $"GameStartup: token=0x{gameStartup.MetadataToken.ToUInt32():X8}; {gameStartup.FullName}",
            $"InitializePlatform: token=0x{initializePlatform.MetadataToken.ToUInt32():X8}; {initializePlatform.FullName}",
            $"LaunchMainMenu: token=0x{launchMainMenu.MetadataToken.ToUInt32():X8}; {launchMainMenu.FullName}",
            $"LoadDeferredStartupAssetsAsync: token=0x{deferred.MetadataToken.ToUInt32():X8}; {deferred.FullName}",
            $"Same-NGame methods reachable from _EnterTree: {reachable.Count}",
            "Forbidden later-startup methods reachable from _EnterTree same-NGame closure: 0",
            "SceneTree AddChild performed by Step 38: NO",
            string.Empty,
            "[SAME-NGAME _ENTERTREE REACHABLE METHODS]",
        };
        foreach (var method in reachable)
            lines.Add($"  - token=0x{method.MetadataToken.ToUInt32():X8}; {method.FullName}");

        foreach (var (label, method) in new[]
        {
            ("NGAME _ENTERTREE IL", enterTree),
            ("NGAME _READY IL", ready),
            ("NGAME GAMESTARTUPWRAPPER IL", gameStartupWrapper),
            ("NGAME GAMESTARTUP IL", gameStartup),
            ("NGAME INITIALIZEPLATFORM IL", initializePlatform),
            ("NGAME LAUNCHMAINMENU IL", launchMainMenu),
            ("NGAME LOADDEFERREDSTARTUPASSETSASYNC IL", deferred),
        })
        {
            lines.Add(string.Empty);
            lines.Add("[" + label + "]");
            AppendInstructionMap(lines, method);
        }
        return string.Join("\n", lines) + "\n";
    }

    private static MethodInfo RequireZeroArgBoolMethod(Type type, string name)
        => type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
               .SingleOrDefault(method => method.Name == name && method.GetParameters().Length == 0 && method.ReturnType == typeof(bool))
           ?? throw new MissingMethodException(type.FullName, name + "()");

    private static string TryReadIsInsideTree(object instance)
    {
        try
        {
            var method = RequireZeroArgBoolMethod(instance.GetType(), "IsInsideTree");
            return Convert.ToBoolean(method.Invoke(instance, null), System.Globalization.CultureInfo.InvariantCulture).ToString();
        }
        catch (Exception ex)
        {
            return "<unreadable:" + ex.GetType().Name + ">";
        }
    }

    private static void TryReleaseStep38Instance(object instance, Action<string>? checkpoint, string scope)
    {
        try
        {
            var free = instance.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .SingleOrDefault(method => method.Name == "Free" && method.GetParameters().Length == 0);
            if (free is not null)
                free.Invoke(instance, null);
            else if (instance is IDisposable disposable)
                disposable.Dispose();
            Checkpoint(checkpoint, $"H_INSTANCE_RELEASED — scope={scope}; temporary off-tree NGame released without _ExitTree invocation.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"H_INSTANCE_RELEASE_WARNING — scope={scope}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
        }
    }

    private static TransformedRealStS2GameLifecycleEntryGateResult LifecyclePass(
        TransformedRealStS2GameLifecycleEntryGate gate,
        string detail)
        => new(gate, true, detail);

    private static TransformedRealStS2GameLifecycleEntryGateResult LifecycleFail(
        TransformedRealStS2GameLifecycleEntryGate gate,
        string stage,
        Exception ex,
        bool includeDiagnostic)
        => new(gate, false,
            $"Stage: {stage}\n{ex.GetType().Name}: {ex.Message}" +
            (includeDiagnostic ? "\n" + FormatExceptionDiagnostic(ex) : string.Empty));

    private sealed record Step38LifecycleStaticAuditSnapshot(
        string SelectedPath,
        string SelectedSha256,
        uint EnterTreeToken,
        string StaticMap,
        string[] SameNGameReachableMethods);
}
