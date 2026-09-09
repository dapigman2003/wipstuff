using System.Reflection;
using Mono.Cecil;

namespace StS2Launcher.Core;

/// <summary>
/// Step 40.1 boundary. Physical 0.0.170 closed Step 39 at 4/4: the exact real NGame hierarchy is attached
/// to the live SceneTree, NGame.Instance/_window/parent authority is established, OneTimeInitialization is
/// state 2, and the launcher-owned Godot render loop is frozen. Step 40 keeps GameStartupWrapper inert and
/// first audits the actual in-tree hierarchy for managed frame/input callbacks that a short render pulse could
/// admit. Only after that fail-closed audit may the iOS caller restart rendering for the bounded pulse window,
/// synchronously stop it again, and ask this core boundary to prove post-pulse confinement. This step does not
/// authorize GameStartup, platform/main-menu/deferred startup, Steam, native game GDExtensions, explicit
/// _ExitTree, RemoveChild/Free, or leaving rendering active after the pulse.
/// </summary>
public sealed partial class TransformedRealStS2VeryEarlyInitialization
{
    public const int Step40RenderPulseTargetMilliseconds = 100;
    public const int Step40RenderPulseMaximumMilliseconds = 2000;

    private static readonly string[] Step40ImmediateFrameMethodNames =
    [
        "_Process",
        "_PhysicsProcess",
        "_Draw",
        "_Input",
        "_ShortcutInput",
        "_UnhandledInput",
        "_UnhandledKeyInput",
        "_GuiInput",
    ];

    private Step40PreflightSnapshot? _step40Preflight;
    private bool _step40PulseStarted;
    private bool _step40PulsePassed;
    private bool _exactStep40ClosurePassed;

    public bool ExactStep40ClosurePassed => _exactStep40ClosurePassed;

    public string GetVerifiedStep40StaticMap()
        => _step40Preflight?.StaticMap
           ?? throw new InvalidOperationException("Step 40.1 Gate B has not produced the verified frame-driven managed-surface static map.");

    private void ResetStep40State()
    {
        ResetStep41State();
        _step40Preflight = null;
        _step40PulseStarted = false;
        _step40PulsePassed = false;
        _exactStep40ClosurePassed = false;
    }

    public TransformedRealStS2RenderPulseGateResult RunStep40ClosedStep39FrozenAuthority(
        bool renderingStopped,
        Action<string>? checkpoint = null)
    {
        const TransformedRealStS2RenderPulseGate gate = TransformedRealStS2RenderPulseGate.ClosedStep39FrozenAuthority;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            ResetStep40State();
            var context = RequireStep40Prerequisite("Step 40 Gate A entry");
            var step39 = _step39Preflight ?? throw new InvalidOperationException("Step 40.1 requires the closed Step-39 preflight/static authority in the same process.");
            if (!renderingStopped)
                throw new InvalidOperationException("Step 40.1 requires the Godot render loop to still be frozen by the successful Step-39 Gate C/D boundary.");

            Checkpoint(checkpoint, "J_A_ENTRY — requiring same-process physical Step-39 4/4 state, rendering frozen, exact inserted NGame singleton/parent/_window/state authority, unchanged selected compatibility bytes, and inert GameStartupWrapper before any render restart.");
            stage = "closed Step-39 frozen authority re-verification";
            var state = RequireStep40InsertedAuthority();
            var selectedSha256 = ComputeSha256Hex(step39.SelectedPath);
            if (!selectedSha256.Equals(step39.SelectedSha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException($"Step 40.1 selected compatibility image hash drifted. expected={step39.SelectedSha256}; actual={selectedSha256}.");

            using (var resolver = new RejectingAssemblyResolver())
            using (var module = ModuleDefinition.ReadModule(step39.SelectedPath, new ReaderParameters
            {
                ReadSymbols = false,
                ReadingMode = ReadingMode.Deferred,
                InMemory = true,
                AssemblyResolver = resolver,
                MetadataResolver = new MetadataResolver(resolver),
            }))
            {
                var nGame = module.Types.SingleOrDefault(type => type.FullName == NGameTypeFullName)
                    ?? throw new InvalidDataException($"Step 40.1 could not locate {NGameTypeFullName} in selected compatibility authority.");
                var wrapper = RequireLifecycleMethodByName(nGame, NGameGameStartupWrapperMethodName, 0);
                RequireInertGameStartupWrapper(wrapper);
                if (resolver.Requests.Count != 0)
                    throw new InvalidDataException("Step 40.1 authority re-verification unexpectedly attempted external Cecil resolution: " + string.Join(" | ", resolver.Requests));
            }

            var snapshot = new Step40PreflightSnapshot(
                step39.SelectedPath,
                step39.SelectedSha256,
                string.Empty,
                context.ManagedResolverRequests.Count,
                context.HostLoads.Count,
                context.PrivateLoads.Count,
                context.InitializerBearingRequests.Count,
                context.RejectedManagedRequests.Count,
                context.NativeLoadAttempts.Count);
            _step40Preflight = snapshot;
            RequireNoForbiddenStep37Escape(context, snapshot.InitializerCount, snapshot.RejectedCount, snapshot.NativeCount, "Step 40 Gate A");

            Checkpoint(checkpoint, $"J_A_PASS — Step-39 4/4 retained in same process; renderingStopped=True; NGame in-tree/singleton/parent/_window authority intact; state={state}; selectedSha256={selectedSha256}; GameStartupWrapper inert; resolver/native deltas=0.");
            return Step40Pass(gate,
                "STEP 40.1 CLOSED STEP-39 FROZEN AUTHORITY PASSED.\n" +
                "Same-process Step 39 closure: 4/4\n" +
                "Rendering active before Step 40: FALSE\n" +
                "NGame in-tree / singleton / parent / _window authority: PRESERVED\n" +
                $"OneTimeInitialization state: {state}\n" +
                "GameStartupWrapper: INERT\n" +
                "Render restart: NOT YET");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"J_A_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return Step40Fail(gate, stage, ex, includeDiagnostic: true);
        }
    }

    public TransformedRealStS2RenderPulseGateResult RunStep40FrameDrivenManagedSurfaceAudit(
        Action<string>? checkpoint = null)
    {
        const TransformedRealStS2RenderPulseGate gate = TransformedRealStS2RenderPulseGate.FrameDrivenManagedSurfaceAudit;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep40Prerequisite("Step 40 Gate B entry");
            var preflight = _step40Preflight ?? throw new InvalidOperationException("Step 40.1 Gate A must pass before Gate B.");
            var instance = _step39NGameInstance ?? throw new InvalidOperationException("Step 40.1 inserted NGame instance is absent.");
            var sceneTreeRoot = _step39SceneTreeRoot ?? throw new InvalidOperationException("Step 40.1 live SceneTree root is absent.");
            var handoff = _callbackHandoff ?? throw new InvalidOperationException("Step 40.1 exact GodotSharp handoff disappeared.");
            var nodeType = handoff.GodotSharpAssembly.GetType("Godot.Node", throwOnError: true, ignoreCase: false)
                ?? throw new MissingMemberException("Godot.Node");

            Checkpoint(checkpoint, "J_B_ENTRY — enumerating the already-in-tree real hierarchy and Cecil-mapping actual sts2 _Process/_PhysicsProcess/_Draw/input callback overrides through each in-module base chain. Step-39 _Notification closure remains prerequisite authority. Rendering stays frozen throughout Gate B.");
            stage = "actual in-tree frame-driven managed-surface audit";
            if (RequireZeroArgBoolMethod(instance.GetType(), "IsInsideTree").Invoke(instance, null) is not true)
                throw new InvalidDataException("Step 40.1 NGame left the SceneTree before frame-surface audit.");

            var nodes = EnumerateStep39NodeGraph(instance, nodeType);
            var managedTypeNames = nodes
                .Select(node => node.Node.GetType())
                .Where(type => type.Assembly == instance.GetType().Assembly)
                .Select(type => type.FullName ?? type.Name)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToArray();

            using var resolver = new RejectingAssemblyResolver();
            using var module = ModuleDefinition.ReadModule(preflight.SelectedPath, new ReaderParameters
            {
                ReadSymbols = false,
                ReadingMode = ReadingMode.Deferred,
                InMemory = true,
                AssemblyResolver = resolver,
                MetadataResolver = new MetadataResolver(resolver),
            });
            var allTypes = EnumerateTypes(module.Types).ToDictionary(type => type.FullName, StringComparer.Ordinal);
            var allMethods = allTypes.Values
                .SelectMany(type => type.Methods)
                .GroupBy(method => method.FullName, StringComparer.Ordinal)
                .ToDictionary(group => group.Key, group => group.Single(), StringComparer.Ordinal);
            var callbacks = new List<Step40ManagedCallbackAudit>();
            var callbackTokens = new HashSet<uint>();
            var closureTokens = new HashSet<uint>();
            var forbiddenReferences = new SortedSet<string>(StringComparer.Ordinal);
            var unresolvedSameAssemblyReferences = new SortedSet<string>(StringComparer.Ordinal);

            foreach (var typeName in managedTypeNames)
            {
                if (!allTypes.TryGetValue(typeName, out var typeDef))
                    throw new InvalidDataException($"Step 40.1 actual managed node type {typeName} is absent from selected Cecil authority.");

                for (TypeDefinition? current = typeDef; current is not null;)
                {
                    foreach (var method in current.Methods.Where(method => Step40ImmediateFrameMethodNames.Contains(method.Name, StringComparer.Ordinal) && method.HasBody))
                    {
                        var token = method.MetadataToken.ToUInt32();
                        if (!callbackTokens.Add(token))
                            continue;
                        var references = method.Body.Instructions
                            .Select(instruction => instruction.Operand)
                            .OfType<MethodReference>()
                            .Select(reference => reference.FullName)
                            .Distinct(StringComparer.Ordinal)
                            .OrderBy(name => name, StringComparer.Ordinal)
                            .ToArray();
                        AuditStep39ManagedLifecycleClosure(
                            method,
                            allTypes,
                            allMethods,
                            closureTokens,
                            forbiddenReferences,
                            unresolvedSameAssemblyReferences);
                        callbacks.Add(new Step40ManagedCallbackAudit(current.FullName, method.Name, token, references));
                    }

                    var baseName = current.BaseType?.FullName;
                    current = baseName is not null && allTypes.TryGetValue(baseName, out var baseType)
                        ? baseType
                        : null;
                }
            }

            if (unresolvedSameAssemblyReferences.Count != 0)
                throw new InvalidDataException("Step 40.1 frame-driven managed closure contains same-sts2 references that could not be mapped without resolution: " + string.Join(" | ", unresolvedSameAssemblyReferences));
            if (forbiddenReferences.Count != 0)
                throw new InvalidDataException("Step 40.1 frame-driven managed closure reaches forbidden startup/native/platform references: " + string.Join(" | ", forbiddenReferences));
            if (resolver.Requests.Count != 0)
                throw new InvalidDataException("Step 40.1 frame-driven hierarchy audit unexpectedly attempted external Cecil resolution: " + string.Join(" | ", resolver.Requests));

            var staticMap = BuildStep40StaticMap(preflight, nodes, managedTypeNames, callbacks, closureTokens.Count, sceneTreeRoot);
            _step40Preflight = preflight with { StaticMap = staticMap };
            RequireNoForbiddenStep37Escape(context, preflight.InitializerCount, preflight.RejectedCount, preflight.NativeCount, "Step 40 Gate B");

            Checkpoint(checkpoint, $"J_B_PASS — in-tree nodeCount={nodes.Count}; selectedManagedNodeTypes={managedTypeNames.Length}; immediateFrameInputCallbacks={callbacks.Count}; transitiveSameSts2FrameClosureMethods={closureTokens.Count}; forbiddenFrameRefs=0; unresolvedSameSts2Refs=0; externalResolutionRequests=0; rendering remains stopped.");
            return Step40Pass(gate,
                "STEP 40.1 FRAME-DRIVEN MANAGED SURFACE AUDIT PASSED.\n" +
                $"Actual in-tree node count: {nodes.Count}\n" +
                $"Selected sts2 managed node types: {managedTypeNames.Length}\n" +
                $"Immediate managed frame/input callbacks mapped: {callbacks.Count}\n" +
                $"Transitive same-sts2 frame/input closure methods: {closureTokens.Count}\n" +
                "Forbidden frame-closure references: 0\n" +
                "Unresolved same-sts2 frame references: 0\n" +
                "Render restart: NOT YET");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"J_B_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return Step40Fail(gate, stage, ex, includeDiagnostic: true);
        }
    }

    public void BeginStep40BoundedRenderPulse(Action<string>? checkpoint = null)
    {
        ThrowIfDisposed();
        RequireStep40Prerequisite("Step 40 Gate C pulse start");
        if (_step40Preflight is null)
            throw new InvalidOperationException("Step 40.1 Gate B must pass before render-pulse start.");
        if (_step40PulseStarted)
            throw new InvalidOperationException("Step 40.1 render pulse has already started in this process. Relaunch; never retry a render pulse in-process.");
        RequireStep40InsertedAuthority();
        _step40PulseStarted = true;
        Checkpoint(checkpoint, $"J_C_PULSE_ARMED — first and only Step-40 render pulse authorized; requested stop delay={Step40RenderPulseTargetMilliseconds}ms; post-stop evidence ceiling={Step40RenderPulseMaximumMilliseconds}ms. The ceiling is classification only and cannot preempt a long Godot main-thread frame. GameStartup/platform/main-menu/deferred/Steam/native game extensions remain forbidden. First managed continuation after the requested delay must synchronously StopRendering before telemetry or other Step-40 work.");
    }

    public TransformedRealStS2RenderPulseGateResult RunStep40BoundedRenderPulseEvidence(
        bool startReturned,
        bool renderingActiveAfterStart,
        bool stopReturned,
        bool renderingActiveAfterStop,
        double elapsedMilliseconds,
        Action<string>? checkpoint = null)
    {
        const TransformedRealStS2RenderPulseGate gate = TransformedRealStS2RenderPulseGate.BoundedRenderPulseAndRefreeze;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep40Prerequisite("Step 40 Gate C evidence");
            var preflight = _step40Preflight ?? throw new InvalidOperationException("Step 40.1 Gate B authority is absent.");
            if (!_step40PulseStarted)
                throw new InvalidOperationException("Step 40.1 Gate C evidence arrived before the render pulse was armed.");

            Checkpoint(checkpoint, $"J_C_EVIDENCE_ENTRY — startReturned={startReturned}; renderingActiveAfterStart={renderingActiveAfterStart}; stopReturned={stopReturned}; renderingActiveAfterStop={renderingActiveAfterStop}; elapsedMs={elapsedMilliseconds:F1}.");
            stage = "bounded real-game render pulse and synchronous refreeze";
            if (!startReturned || !renderingActiveAfterStart)
                throw new InvalidOperationException($"Step 40.1 StartRendering did not establish an active render loop. startReturned={startReturned}; activeAfterStart={renderingActiveAfterStart}.");
            if (!stopReturned || renderingActiveAfterStop)
                throw new InvalidOperationException($"Step 40.1 StopRendering did not synchronously refreeze the render loop. stopReturned={stopReturned}; activeAfterStop={renderingActiveAfterStop}.");
            if (elapsedMilliseconds < Step40RenderPulseTargetMilliseconds || elapsedMilliseconds > Step40RenderPulseMaximumMilliseconds)
                throw new InvalidOperationException($"Step 40.1 post-stop render observation fell outside the accepted evidence window. requestedDelay={Step40RenderPulseTargetMilliseconds}ms; evidenceCeiling={Step40RenderPulseMaximumMilliseconds}ms; observed={elapsedMilliseconds:F1}ms.");

            var state = RequireStep40InsertedAuthority();
            RequireNoForbiddenStep37Escape(context, preflight.InitializerCount, preflight.RejectedCount, preflight.NativeCount, "Step 40 Gate C");
            _step40PulsePassed = true;

            Checkpoint(checkpoint, $"J_C_PASS — StartRendering established active=True; first managed stop opportunity completed at {elapsedMilliseconds:F1}ms (overshoot={elapsedMilliseconds - Step40RenderPulseTargetMilliseconds:F1}ms); StopRendering returned=True and activeAfterStop=False; NGame authority/state={state} preserved; resolverDelta={context.ManagedResolverRequests.Count - preflight.ResolverCount}; hostDelta={context.HostLoads.Count - preflight.HostCount}; privateDelta={context.PrivateLoads.Count - preflight.PrivateCount}; initializerDelta=0; rejectedDelta=0; nativeDelta=0.");
            return Step40Pass(gate,
                "STEP 40.1 BOUNDED REAL-GAME RENDER PULSE PASSED.\n" +
                "StartRendering: RETURNED / ACTIVE\n" +
                $"Observed first-stop opportunity: {elapsedMilliseconds:F1} ms\n" +
                $"Requested stop delay: {Step40RenderPulseTargetMilliseconds} ms\n" +
                $"Stop-opportunity overshoot: {elapsedMilliseconds - Step40RenderPulseTargetMilliseconds:F1} ms\n" +
                $"Post-stop evidence ceiling: {Step40RenderPulseMaximumMilliseconds} ms\n" +
                "StopRendering: RETURNED / INACTIVE\n" +
                "NGame in-tree / singleton / parent / _window authority: PRESERVED\n" +
                $"OneTimeInitialization state: {state}\n" +
                $"Managed resolver delta: {FormatDelta(context.ManagedResolverRequests.Skip(preflight.ResolverCount).ToArray())}\n" +
                $"Host-load delta: {FormatDelta(context.HostLoads.Skip(preflight.HostCount).ToArray())}\n" +
                $"Private-load delta: {FormatDelta(context.PrivateLoads.Skip(preflight.PrivateCount).ToArray())}\n" +
                "Initializer-bearing request delta: 0\n" +
                "Rejected managed request delta: 0\n" +
                "Native-load attempt delta: 0");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"J_C_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return Step40Fail(gate, stage, ex, includeDiagnostic: true);
        }
    }

    public TransformedRealStS2RenderPulseGateResult RunStep40FrozenPostPulseConfinement(
        bool renderingStopped,
        Action<string>? checkpoint = null)
    {
        const TransformedRealStS2RenderPulseGate gate = TransformedRealStS2RenderPulseGate.FrozenPostPulseConfinement;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep40Prerequisite("Step 40 Gate D entry");
            var preflight = _step40Preflight ?? throw new InvalidOperationException("Step 40.1 Gate-B authority is absent.");
            if (!_step40PulseStarted || !_step40PulsePassed)
                throw new InvalidOperationException("Step 40.1 Gate D requires a successful first render pulse and synchronous refreeze.");
            if (!renderingStopped)
                throw new InvalidOperationException("Step 40.1 Gate D refuses to run unless rendering is confirmed stopped after the pulse.");

            Checkpoint(checkpoint, "J_D_ENTRY — render loop frozen again after the first controlled real-game pulse; proving inserted NGame authority/state and zero initializer/rejected/native escape. No cleanup, restart, GameStartup, platform, main-menu, deferred, Steam, or native game-extension boundary is authorized.");
            stage = "frozen post-pulse confinement";
            var state = RequireStep40InsertedAuthority();
            RequireNoForbiddenStep37Escape(context, preflight.InitializerCount, preflight.RejectedCount, preflight.NativeCount, "Step 40 Gate D");
            _exactStep40ClosurePassed = true;

            Checkpoint(checkpoint, $"J_D_PASS — post-pulse renderingStopped=True; real NGame remains attached to SceneTree.Root with exact singleton/_window authority and state={state}; initializerDelta=0; rejectedDelta=0; nativeDelta=0. Instance retained in-tree; rendering remains stopped.");
            return Step40Pass(gate,
                "STEP 40.1 FROZEN POST-PULSE CONFINEMENT PASSED.\n" +
                "Rendering active: FALSE\n" +
                "NGame remains inside SceneTree: TRUE\n" +
                "NGame.Instance / parent / _window authority: PRESERVED\n" +
                $"OneTimeInitialization state: {state}\n" +
                "Initializer-bearing request delta: 0\n" +
                "Rejected managed request delta: 0\n" +
                "Native-load attempt delta: 0\n" +
                "GameStartup / platform / main-menu / ExecuteDeferred / Steam / native GDExtensions: NOT AUTHORIZED\n" +
                "Inserted instance intentionally retained and renderer intentionally frozen.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"J_D_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return Step40Fail(gate, stage, ex, includeDiagnostic: true);
        }
    }

    private Step35ExecutionLoadContext RequireStep40Prerequisite(string boundary)
    {
        var context = RequireStep39Prerequisite(boundary);
        if (!_exactStep39ClosurePassed || !_step39InsertionStarted || !_step39InsertionPassed)
            throw new InvalidOperationException($"{boundary} requires same-process Step-39.0 4/4 physical authority with the real NGame retained in-tree and rendering frozen.");
        if (_step39NGameInstance is null || _step39SceneTreeRoot is null || _step39Preflight is null)
            throw new InvalidOperationException($"{boundary} requires the retained Step-39 instance/root/preflight authority.");
        return context;
    }

    private int RequireStep40InsertedAuthority()
    {
        var instance = _step39NGameInstance ?? throw new InvalidOperationException("Step 40.1 retained NGame instance is absent.");
        var sceneTreeRoot = _step39SceneTreeRoot ?? throw new InvalidOperationException("Step 40.1 retained SceneTree root is absent.");
        if (RequireZeroArgBoolMethod(instance.GetType(), "IsInsideTree").Invoke(instance, null) is not true)
            throw new InvalidDataException("Step 40.1 retained NGame is no longer inside the SceneTree.");
        var instanceGetter = instance.GetType().GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            .Single(method => method.Name == "get_Instance" && method.GetParameters().Length == 0 && method.ReturnType == instance.GetType());
        if (!ReferenceEquals(instanceGetter.Invoke(null, null), instance))
            throw new InvalidDataException("Step 40.1 NGame.Instance no longer points to the retained inserted object.");
        var getParentCandidates = instance.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(method => method.Name == "GetParent" && method.GetParameters().Length == 0)
            .ToArray();
        var getParent = getParentCandidates.SingleOrDefault(method =>
                !method.IsGenericMethodDefinition &&
                !method.ContainsGenericParameters &&
                method.ReturnType.IsAssignableFrom(sceneTreeRoot.GetType()))
            ?? throw new MissingMethodException(
                instance.GetType().FullName,
                $"GetParent() exact non-generic SceneTree-root-compatible overload; candidates={string.Join(",", getParentCandidates.Select(method => method.ToString()))}");
        if (!ReferenceEquals(getParent.Invoke(instance, null), sceneTreeRoot))
            throw new InvalidDataException("Step 40.1 retained NGame parent drifted from SceneTree.Root.");
        var windowField = instance.GetType().GetField("_window", BindingFlags.Static | BindingFlags.NonPublic)
            ?? throw new MissingFieldException(NGameTypeFullName, "_window");
        if (windowField.GetValue(null) is null)
            throw new InvalidDataException("Step 40.1 NGame._Ready window authority is no longer present.");
        var state = ReadOneTimeInitializationState(RequireEssentialBinding().StateField);
        if (state != ExpectedStateAfterEssential)
            throw new InvalidDataException($"Step 40.1 OneTimeInitialization state drifted: expected {ExpectedStateAfterEssential}; observed {state}.");
        return state;
    }

    private static string BuildStep40StaticMap(
        Step40PreflightSnapshot preflight,
        IReadOnlyList<Step39NodeObservation> nodes,
        IReadOnlyList<string> managedTypes,
        IReadOnlyList<Step40ManagedCallbackAudit> callbacks,
        int closureMethodCount,
        object sceneTreeRoot)
    {
        var lines = new List<string>
        {
            "StS2 Launcher — Step 40.1 controlled real-game render-pulse preflight map",
            "Read-only evidence from the physically closed Step-39 in-tree hierarchy and exact selected compatibility image; never consumed as trusted runtime input.",
            $"Selected compatibility path: {preflight.SelectedPath}",
            $"Selected compatibility SHA-256: {preflight.SelectedSha256}",
            $"Live SceneTree root type: {sceneTreeRoot.GetType().FullName}",
            $"Actual in-tree game hierarchy nodes: {nodes.Count}",
            $"Selected sts2 managed node types: {managedTypes.Count}",
            $"Immediate managed frame/input callbacks mapped including in-module base chains ({string.Join("/", Step40ImmediateFrameMethodNames)}): {callbacks.Count}",
            $"Transitive same-sts2 frame/input closure methods mapped: {closureMethodCount}",
            "Forbidden startup/native/platform references from transitive frame/input closure: 0",
            "Unresolved same-sts2 references from transitive frame/input closure: 0",
            "Step-39 _Notification closure: prior physical/static prerequisite authority, branch-insensitive and already clean",
            $"Step 40 requested stop delay: {Step40RenderPulseTargetMilliseconds} ms",
            $"Step 40 post-stop first-opportunity evidence ceiling: {Step40RenderPulseMaximumMilliseconds} ms",
            "Step 40 policy: StartRendering once -> first managed continuation after requested delay synchronously StopRendering before telemetry -> verify frozen in-tree confinement.",
            "Step 40 forbidden: GameStartup, InitializePlatform, LaunchMainMenu, ExecuteDeferred, Steam init/native Steam, native GDExtensions, explicit _ExitTree, RemoveChild/Free, leaving rendering active.",
            string.Empty,
            "[ACTUAL IN-TREE NODE GRAPH]",
        };
        foreach (var node in nodes)
            lines.Add($"  - {node.Path} | {node.Node.GetType().FullName}");
        lines.Add(string.Empty);
        lines.Add("[SELECTED STS2 MANAGED NODE TYPES]");
        foreach (var type in managedTypes)
            lines.Add("  - " + type);
        lines.Add(string.Empty);
        lines.Add("[IMMEDIATE MANAGED FRAME/INPUT CALLBACKS]");
        foreach (var callback in callbacks.OrderBy(item => item.TypeName, StringComparer.Ordinal).ThenBy(item => item.MethodName, StringComparer.Ordinal))
        {
            lines.Add($"  - token=0x{callback.Token:X8}; {callback.TypeName}::{callback.MethodName}");
            foreach (var reference in callback.DirectMethodReferences)
                lines.Add("      callref: " + reference);
        }
        return string.Join("\n", lines) + "\n";
    }

    private static TransformedRealStS2RenderPulseGateResult Step40Pass(
        TransformedRealStS2RenderPulseGate gate,
        string detail)
        => new(gate, true, detail);

    private static TransformedRealStS2RenderPulseGateResult Step40Fail(
        TransformedRealStS2RenderPulseGate gate,
        string stage,
        Exception ex,
        bool includeDiagnostic)
        => new(gate, false,
            $"Stage: {stage}\n{ex.GetType().Name}: {ex.Message}" +
            (includeDiagnostic ? "\n" + FormatExceptionDiagnostic(ex) : string.Empty));

    private sealed record Step40ManagedCallbackAudit(string TypeName, string MethodName, uint Token, string[] DirectMethodReferences);
    private sealed record Step40PreflightSnapshot(
        string SelectedPath,
        string SelectedSha256,
        string StaticMap,
        int ResolverCount,
        int HostCount,
        int PrivateCount,
        int InitializerCount,
        int RejectedCount,
        int NativeCount);
}
