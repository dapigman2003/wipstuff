using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace StS2Launcher.Core;

/// <summary>
/// Step 41.0 is deliberately non-invoking. Physical Step 40 proved that the already-admitted real NGame
/// hierarchy tolerates a controlled real render pulse and refreeze. Before GameStartup is ever enabled,
/// this boundary maps the exact selected compatibility image's NGame.GameStartup async state machine,
/// its MoveNext IL, and the transitive same-sts2 closure that reaches platform/Steam/main-menu/deferred/
/// native-facing surfaces. Rendering remains frozen and GameStartupWrapper remains inert throughout.
/// </summary>
public sealed partial class TransformedRealStS2VeryEarlyInitialization
{
    private const string Step41AsyncStateMachineAttributeFullName = "System.Runtime.CompilerServices.AsyncStateMachineAttribute";

    private Step41PreflightSnapshot? _step41Preflight;
    private bool _step41StateMachineMapped;
    private bool _step41ClosureMapped;
    private bool _exactStep41ClosurePassed;

    public bool ExactStep41ClosurePassed => _exactStep41ClosurePassed;

    public string GetVerifiedStep41StaticMap()
        => _step41Preflight?.StaticMap
           ?? throw new InvalidOperationException("Step 41.0 Gate C has not produced the verified GameStartup frontier static map.");

    private void ResetStep41State()
    {
        _step41Preflight = null;
        _step41StateMachineMapped = false;
        _step41ClosureMapped = false;
        _exactStep41ClosurePassed = false;
    }

    public TransformedRealStS2GameStartupFrontierGateResult RunStep41ClosedStep40FrozenAuthority(
        bool renderingStopped,
        Action<string>? checkpoint = null)
    {
        const TransformedRealStS2GameStartupFrontierGate gate = TransformedRealStS2GameStartupFrontierGate.ClosedStep40FrozenAuthority;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            ResetStep41State();
            var context = RequireStep41Prerequisite("Step 41 Gate A entry");
            if (!renderingStopped)
                throw new InvalidOperationException("Step 41.0 requires the render loop to remain frozen after successful Step 40 closure.");

            Checkpoint(checkpoint, "K_A_ENTRY — requiring same-process Step-40 4/4 frozen authority, retained real NGame singleton/parent/_window/state=2, unchanged selected compatibility bytes, and still-inert GameStartupWrapper. Step 41 performs metadata/IL mapping only; GameStartup is not invoked.");
            stage = "closed Step-40 frozen authority re-verification";
            var state = RequireStep40InsertedAuthority();
            var step39 = _step39Preflight ?? throw new InvalidOperationException("Step 41.0 requires retained Step-39 selected-image authority.");
            var selectedSha256 = ComputeSha256Hex(step39.SelectedPath);
            if (!selectedSha256.Equals(step39.SelectedSha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException($"Step 41.0 selected compatibility image hash drifted. expected={step39.SelectedSha256}; actual={selectedSha256}.");

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
                    ?? throw new InvalidDataException($"Step 41.0 could not locate {NGameTypeFullName} in selected compatibility authority.");
                RequireInertGameStartupWrapper(RequireLifecycleMethodByName(nGame, NGameGameStartupWrapperMethodName, 0));
                if (resolver.Requests.Count != 0)
                    throw new InvalidDataException("Step 41.0 Gate A unexpectedly attempted external Cecil resolution: " + string.Join(" | ", resolver.Requests));
            }

            _step41Preflight = new Step41PreflightSnapshot(
                step39.SelectedPath,
                step39.SelectedSha256,
                string.Empty,
                string.Empty,
                0,
                string.Empty,
                0,
                0,
                Array.Empty<Step41BoundaryObservation>(),
                context.ManagedResolverRequests.Count,
                context.HostLoads.Count,
                context.PrivateLoads.Count,
                context.InitializerBearingRequests.Count,
                context.RejectedManagedRequests.Count,
                context.NativeLoadAttempts.Count);
            RequireNoForbiddenStep37Escape(context, _step41Preflight.InitializerCount, _step41Preflight.RejectedCount, _step41Preflight.NativeCount, "Step 41 Gate A");

            Checkpoint(checkpoint, $"K_A_PASS — same-process Step-40 4/4 retained with renderingStopped=True; NGame authority/state={state}; selectedSha256={selectedSha256}; GameStartupWrapper inert; resolver/native deltas=0; GameStartup invoked=NO.");
            return Step41Pass(gate,
                "STEP 41.0 CLOSED STEP-40 FROZEN AUTHORITY PASSED.\n" +
                "Same-process Step 40 closure: 4/4\n" +
                "Rendering active: FALSE\n" +
                "NGame in-tree / singleton / parent / _window authority: PRESERVED\n" +
                $"OneTimeInitialization state: {state}\n" +
                "GameStartupWrapper: INERT\n" +
                "GameStartup invocation: NO");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"K_A_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return Step41Fail(gate, stage, ex, includeDiagnostic: true);
        }
    }

    public TransformedRealStS2GameStartupFrontierGateResult RunStep41GameStartupAsyncStateMachineMap(Action<string>? checkpoint = null)
    {
        const TransformedRealStS2GameStartupFrontierGate gate = TransformedRealStS2GameStartupFrontierGate.GameStartupAsyncStateMachineMap;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep41Prerequisite("Step 41 Gate B entry");
            var preflight = _step41Preflight ?? throw new InvalidOperationException("Step 41.0 Gate A must pass before Gate B.");

            Checkpoint(checkpoint, "K_B_ENTRY — locating exact NGame.GameStartup metadata without invocation, resolving only its AsyncStateMachineAttribute type reference inside the same module, and mapping the compiler-generated MoveNext body plus state-machine fields. Rendering remains frozen.");
            stage = "exact GameStartup async state-machine metadata/IL map";
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
            var nGame = allTypes.TryGetValue(NGameTypeFullName, out var nGameType)
                ? nGameType
                : throw new InvalidDataException($"Step 41.0 could not locate {NGameTypeFullName}.");
            RequireInertGameStartupWrapper(RequireLifecycleMethodByName(nGame, NGameGameStartupWrapperMethodName, 0));
            var gameStartup = nGame.Methods.SingleOrDefault(method => method.Name == NGameGameStartupMethodName && method.Parameters.Count == 0)
                ?? throw new MissingMethodException(NGameTypeFullName, NGameGameStartupMethodName);
            if (!gameStartup.HasBody)
                throw new InvalidDataException("Step 41.0 exact NGame.GameStartup has no IL body.");

            var asyncAttribute = gameStartup.CustomAttributes.SingleOrDefault(attribute => attribute.AttributeType.FullName == Step41AsyncStateMachineAttributeFullName)
                ?? throw new InvalidDataException("Step 41.0 exact NGame.GameStartup is missing AsyncStateMachineAttribute.");
            if (asyncAttribute.ConstructorArguments.Count != 1 || asyncAttribute.ConstructorArguments[0].Value is not TypeReference stateMachineTypeReference)
                throw new InvalidDataException("Step 41.0 AsyncStateMachineAttribute did not contain exactly one state-machine TypeReference.");
            if (!allTypes.TryGetValue(stateMachineTypeReference.FullName, out var stateMachineType))
                throw new InvalidDataException($"Step 41.0 async state-machine type is not in the selected sts2 module: {stateMachineTypeReference.FullName}.");
            var moveNext = stateMachineType.Methods.SingleOrDefault(method => method.Name == "MoveNext" && method.Parameters.Count == 0)
                ?? throw new MissingMethodException(stateMachineType.FullName, "MoveNext");
            if (!moveNext.HasBody)
                throw new InvalidDataException("Step 41.0 async state-machine MoveNext has no IL body.");

            if (resolver.Requests.Count != 0)
                throw new InvalidDataException("Step 41.0 Gate B unexpectedly attempted external Cecil resolution: " + string.Join(" | ", resolver.Requests));

            var directMap = BuildStep41DirectStateMachineMap(gameStartup, stateMachineType, moveNext);
            _step41Preflight = preflight with
            {
                DirectMap = directMap,
                GameStartupFullName = gameStartup.FullName,
                GameStartupToken = gameStartup.MetadataToken.ToUInt32(),
                StateMachineTypeName = stateMachineType.FullName,
                MoveNextToken = moveNext.MetadataToken.ToUInt32(),
            };
            _step41StateMachineMapped = true;
            RequireNoForbiddenStep37Escape(context, preflight.InitializerCount, preflight.RejectedCount, preflight.NativeCount, "Step 41 Gate B");

            Checkpoint(checkpoint, $"K_B_PASS — GameStartup token=0x{gameStartup.MetadataToken.ToUInt32():X8}; asyncStateMachine={stateMachineType.FullName}; MoveNext token=0x{moveNext.MetadataToken.ToUInt32():X8}; GameStartupIL={gameStartup.Body.Instructions.Count}; MoveNextIL={moveNext.Body.Instructions.Count}; stateMachineFields={stateMachineType.Fields.Count}; externalResolutionRequests=0; invocation=NO.");
            return Step41Pass(gate,
                "STEP 41.0 GAMESTARTUP ASYNC STATE-MACHINE MAP PASSED.\n" +
                $"GameStartup token: 0x{gameStartup.MetadataToken.ToUInt32():X8}\n" +
                $"Async state-machine type: {stateMachineType.FullName}\n" +
                $"MoveNext token: 0x{moveNext.MetadataToken.ToUInt32():X8}\n" +
                $"GameStartup IL instructions: {gameStartup.Body.Instructions.Count}\n" +
                $"MoveNext IL instructions: {moveNext.Body.Instructions.Count}\n" +
                $"State-machine fields: {stateMachineType.Fields.Count}\n" +
                "External Cecil resolution requests: 0\n" +
                "GameStartup invocation: NO");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"K_B_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return Step41Fail(gate, stage, ex, includeDiagnostic: true);
        }
    }

    public TransformedRealStS2GameStartupFrontierGateResult RunStep41TransitiveStartupBoundaryMap(Action<string>? checkpoint = null)
    {
        const TransformedRealStS2GameStartupFrontierGate gate = TransformedRealStS2GameStartupFrontierGate.TransitiveStartupBoundaryMap;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep41Prerequisite("Step 41 Gate C entry");
            var preflight = _step41Preflight ?? throw new InvalidOperationException("Step 41.0 Gate A/B authority is absent.");
            if (!_step41StateMachineMapped)
                throw new InvalidOperationException("Step 41.0 Gate B must pass before Gate C.");

            Checkpoint(checkpoint, "K_C_ENTRY — traversing the exact GameStartup MoveNext same-sts2 call closure without executing it; collecting path-qualified platform/Steam/main-menu/deferred/FMOD/Spine/Sentry/native-facing boundary references while rejecting unresolved same-sts2 metadata and any external Cecil resolution.");
            stage = "transitive GameStartup same-sts2 closure and boundary classification";
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
            if (!allTypes.TryGetValue(preflight.StateMachineTypeName, out var stateMachineType))
                throw new InvalidDataException($"Step 41.0 could not relocate state machine {preflight.StateMachineTypeName}.");
            var moveNext = stateMachineType.Methods.SingleOrDefault(method => method.MetadataToken.ToUInt32() == preflight.MoveNextToken)
                ?? throw new InvalidDataException($"Step 41.0 could not relocate MoveNext token 0x{preflight.MoveNextToken:X8}.");

            var closure = AuditStep41StartupClosure(moveNext, allTypes, allMethods);
            if (closure.UnresolvedSameAssemblyReferences.Length != 0)
                throw new InvalidDataException("Step 41.0 GameStartup closure contains unresolved same-sts2 references: " + string.Join(" | ", closure.UnresolvedSameAssemblyReferences));
            if (resolver.Requests.Count != 0)
                throw new InvalidDataException("Step 41.0 Gate C unexpectedly attempted external Cecil resolution: " + string.Join(" | ", resolver.Requests));

            var staticMap = BuildStep41StaticMap(preflight, closure);
            _step41Preflight = preflight with
            {
                StaticMap = staticMap,
                ClosureMethodCount = closure.ClosureMethods.Length,
                Boundaries = closure.Boundaries,
            };
            _step41ClosureMapped = true;
            RequireNoForbiddenStep37Escape(context, preflight.InitializerCount, preflight.RejectedCount, preflight.NativeCount, "Step 41 Gate C");

            var categories = closure.Boundaries
                .GroupBy(item => item.Category, StringComparer.Ordinal)
                .OrderBy(group => group.Key, StringComparer.Ordinal)
                .Select(group => $"{group.Key}={group.Count()}")
                .ToArray();
            Checkpoint(checkpoint, $"K_C_PASS — transitiveSameSts2StartupClosureMethods={closure.ClosureMethods.Length}; classifiedBoundaryRefs={closure.Boundaries.Length}; categories={string.Join(",", categories)}; unresolvedSameSts2Refs=0; externalResolutionRequests=0; GameStartup invoked=NO; rendering remains stopped.");
            return Step41Pass(gate,
                "STEP 41.0 TRANSITIVE GAMESTARTUP BOUNDARY MAP PASSED.\n" +
                $"Transitive same-sts2 startup closure methods: {closure.ClosureMethods.Length}\n" +
                $"Classified boundary references: {closure.Boundaries.Length}\n" +
                $"Boundary categories: {(categories.Length == 0 ? "none" : string.Join(", ", categories))}\n" +
                "Unresolved same-sts2 references: 0\n" +
                "External Cecil resolution requests: 0\n" +
                "GameStartup invocation: NO\n" +
                "Rendering active: FALSE");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"K_C_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return Step41Fail(gate, stage, ex, includeDiagnostic: true);
        }
    }

    public TransformedRealStS2GameStartupFrontierGateResult RunStep41FrozenNoInvocationConfinement(
        bool renderingStopped,
        Action<string>? checkpoint = null)
    {
        const TransformedRealStS2GameStartupFrontierGate gate = TransformedRealStS2GameStartupFrontierGate.FrozenNoInvocationConfinement;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep41Prerequisite("Step 41 Gate D entry");
            var preflight = _step41Preflight ?? throw new InvalidOperationException("Step 41.0 Gate C authority is absent.");
            if (!_step41StateMachineMapped || !_step41ClosureMapped || string.IsNullOrWhiteSpace(preflight.StaticMap))
                throw new InvalidOperationException("Step 41.0 Gate B/C mapping must pass before Gate D.");
            if (!renderingStopped)
                throw new InvalidOperationException("Step 41.0 Gate D requires rendering to remain frozen; Step 41 never restarts it.");

            Checkpoint(checkpoint, "K_D_ENTRY — proving the static GameStartup map caused no invocation or runtime drift: renderer still frozen, retained real NGame authority/state=2 intact, GameStartupWrapper still inert, and zero initializer/rejected/native escape.");
            stage = "frozen no-invocation confinement";
            var state = RequireStep40InsertedAuthority();
            using (var resolver = new RejectingAssemblyResolver())
            using (var module = ModuleDefinition.ReadModule(preflight.SelectedPath, new ReaderParameters
            {
                ReadSymbols = false,
                ReadingMode = ReadingMode.Deferred,
                InMemory = true,
                AssemblyResolver = resolver,
                MetadataResolver = new MetadataResolver(resolver),
            }))
            {
                var nGame = module.Types.SingleOrDefault(type => type.FullName == NGameTypeFullName)
                    ?? throw new InvalidDataException($"Step 41.0 could not relocate {NGameTypeFullName} for final inert-wrapper proof.");
                RequireInertGameStartupWrapper(RequireLifecycleMethodByName(nGame, NGameGameStartupWrapperMethodName, 0));
                if (resolver.Requests.Count != 0)
                    throw new InvalidDataException("Step 41.0 Gate D unexpectedly attempted external Cecil resolution: " + string.Join(" | ", resolver.Requests));
            }
            RequireNoForbiddenStep37Escape(context, preflight.InitializerCount, preflight.RejectedCount, preflight.NativeCount, "Step 41 Gate D");
            _exactStep41ClosurePassed = true;

            Checkpoint(checkpoint, $"K_D_PASS — renderingStopped=True; real NGame authority/state={state} preserved; GameStartupWrapper inert; GameStartup invoked=NO; mappedClosureMethods={preflight.ClosureMethodCount}; mappedBoundaryRefs={preflight.Boundaries.Length}; resolverDelta={context.ManagedResolverRequests.Count - preflight.ResolverCount}; hostDelta={context.HostLoads.Count - preflight.HostCount}; privateDelta={context.PrivateLoads.Count - preflight.PrivateCount}; initializerDelta=0; rejectedDelta=0; nativeDelta=0.");
            return Step41Pass(gate,
                "STEP 41.0 FROZEN NO-INVOCATION CONFINEMENT PASSED.\n" +
                "Rendering active: FALSE\n" +
                "GameStartup invocation: NO\n" +
                "GameStartupWrapper: INERT\n" +
                "NGame in-tree / singleton / parent / _window authority: PRESERVED\n" +
                $"OneTimeInitialization state: {state}\n" +
                $"Mapped startup closure methods: {preflight.ClosureMethodCount}\n" +
                $"Mapped boundary references: {preflight.Boundaries.Length}\n" +
                "Initializer-bearing request delta: 0\n" +
                "Rejected managed request delta: 0\n" +
                "Native-load attempt delta: 0");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"K_D_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return Step41Fail(gate, stage, ex, includeDiagnostic: true);
        }
    }

    private Step35ExecutionLoadContext RequireStep41Prerequisite(string boundary)
    {
        var context = RequireStep40Prerequisite(boundary);
        if (!_exactStep40ClosurePassed || !_step40PulseStarted || !_step40PulsePassed)
            throw new InvalidOperationException($"{boundary} requires same-process Step-40 4/4 physical authority with rendering frozen again after the controlled pulse.");
        if (_step40Preflight is null)
            throw new InvalidOperationException($"{boundary} requires retained Step-40 frame/input audit authority.");
        return context;
    }

    private static string BuildStep41DirectStateMachineMap(MethodDefinition gameStartup, TypeDefinition stateMachineType, MethodDefinition moveNext)
    {
        var lines = new List<string>
        {
            "[GAMESTARTUP METHOD]",
            $"  token=0x{gameStartup.MetadataToken.ToUInt32():X8}",
            $"  fullname={gameStartup.FullName}",
            $"  returnType={gameStartup.ReturnType.FullName}",
            $"  IL instructions={gameStartup.Body.Instructions.Count}",
        };
        foreach (var instruction in gameStartup.Body.Instructions)
            lines.Add("    " + FormatStep41Instruction(instruction));
        lines.Add(string.Empty);
        lines.Add("[ASYNC STATE MACHINE]");
        lines.Add($"  type={stateMachineType.FullName}");
        lines.Add($"  token=0x{stateMachineType.MetadataToken.ToUInt32():X8}");
        lines.Add($"  fields={stateMachineType.Fields.Count}");
        foreach (var field in stateMachineType.Fields.OrderBy(field => field.MetadataToken.ToUInt32()))
            lines.Add($"    field token=0x{field.MetadataToken.ToUInt32():X8}; {field.FullName}");
        lines.Add(string.Empty);
        lines.Add("[MOVENEXT METHOD]");
        lines.Add($"  token=0x{moveNext.MetadataToken.ToUInt32():X8}");
        lines.Add($"  fullname={moveNext.FullName}");
        lines.Add($"  IL instructions={moveNext.Body.Instructions.Count}");
        foreach (var instruction in moveNext.Body.Instructions)
            lines.Add("    " + FormatStep41Instruction(instruction));
        return string.Join("\n", lines) + "\n";
    }

    private static Step41ClosureAudit AuditStep41StartupClosure(
        MethodDefinition root,
        IReadOnlyDictionary<string, TypeDefinition> allTypes,
        IReadOnlyDictionary<string, MethodDefinition> allMethods)
    {
        var queue = new Queue<(MethodDefinition Method, string Path)>();
        var visited = new HashSet<uint>();
        var closure = new List<Step41ClosureMethod>();
        var boundaries = new SortedDictionary<string, Step41BoundaryObservation>(StringComparer.Ordinal);
        var unresolved = new SortedSet<string>(StringComparer.Ordinal);
        queue.Enqueue((root, root.FullName));

        while (queue.Count != 0)
        {
            var (current, path) = queue.Dequeue();
            var token = current.MetadataToken.ToUInt32();
            if (!visited.Add(token))
                continue;
            closure.Add(new Step41ClosureMethod(token, current.FullName));
            if (!current.HasBody)
                continue;

            foreach (var reference in current.Body.Instructions.Select(instruction => instruction.Operand).OfType<MethodReference>())
            {
                var category = ClassifyStep41Boundary(reference);
                if (category is not null)
                {
                    var observation = new Step41BoundaryObservation(category, reference.FullName, path + " -> " + reference.FullName);
                    boundaries.TryAdd(category + "|" + reference.FullName + "|" + path, observation);
                }

                var candidate = reference is GenericInstanceMethod generic ? generic.ElementMethod : reference;
                var declaringDefinitionName = GetStep41DefinitionTypeName(candidate.DeclaringType);
                if (!allTypes.TryGetValue(declaringDefinitionName, out var declaringDefinition))
                    continue;
                if (candidate is MethodDefinition direct)
                {
                    queue.Enqueue((direct, path + " -> " + direct.FullName));
                    continue;
                }
                if (allMethods.TryGetValue(candidate.FullName, out var definition))
                {
                    queue.Enqueue((definition, path + " -> " + definition.FullName));
                    continue;
                }

                // MemberRefs whose declaring type is a constructed generic render concrete type
                // arguments in FullName, so they do not equal the MethodDef FullName. Resolve only
                // when the declaring-type definition has one unambiguous name/arity/parameter-count
                // candidate; otherwise fail closed rather than guessing.
                var genericArity = candidate.GenericParameters.Count;
                var fallbackCandidates = declaringDefinition.Methods
                    .Where(method => method.Name == candidate.Name &&
                                     method.Parameters.Count == candidate.Parameters.Count &&
                                     method.GenericParameters.Count == genericArity)
                    .ToArray();
                if (fallbackCandidates.Length == 1)
                {
                    queue.Enqueue((fallbackCandidates[0], path + " -> " + fallbackCandidates[0].FullName));
                    continue;
                }
                unresolved.Add($"{current.FullName} -> {reference.FullName} [declaringDef={declaringDefinitionName}; fallbackCandidates={fallbackCandidates.Length}]");
            }
        }

        return new Step41ClosureAudit(
            closure.OrderBy(item => item.Token).ToArray(),
            boundaries.Values.ToArray(),
            unresolved.ToArray());
    }

    private static string GetStep41DefinitionTypeName(TypeReference type)
    {
        while (type is TypeSpecification specification)
            type = specification.ElementType;
        return type.FullName;
    }

    private static string? ClassifyStep41Boundary(MethodReference reference)
    {
        var declaring = reference.DeclaringType.FullName;
        var ns = reference.DeclaringType.Namespace ?? string.Empty;
        var declaringName = reference.DeclaringType.Name;
        if (reference.DeclaringType.FullName == TargetTypeFullName &&
            reference.Name is "ExecuteDeferred" or "ExecuteEssential" or "ExecuteVeryEarly" or "PrewarmJit")
            return "ONE_TIME_INITIALIZATION";
        if (reference.DeclaringType.FullName == NGameTypeFullName && reference.Name == NGameInitializePlatformMethodName)
            return "INITIALIZE_PLATFORM";
        if (reference.DeclaringType.FullName == NGameTypeFullName && reference.Name == NGameLaunchMainMenuMethodName)
            return "LAUNCH_MAIN_MENU";
        if (reference.DeclaringType.FullName == NGameTypeFullName && reference.Name == NGameLoadDeferredStartupAssetsMethodName)
            return "LOAD_DEFERRED_STARTUP_ASSETS";
        if (ns.StartsWith("MegaCrit.Sts2.Core.Platform", StringComparison.Ordinal))
            return "PLATFORM";
        if (ns.StartsWith("Steamworks", StringComparison.Ordinal) ||
            (ns.StartsWith("MegaCrit.Sts2", StringComparison.Ordinal) && declaringName.Contains("SteamService", StringComparison.OrdinalIgnoreCase)))
            return "STEAM";
        if ((ns.StartsWith("MegaCrit.Sts2", StringComparison.Ordinal) || ns.StartsWith("FMOD", StringComparison.OrdinalIgnoreCase)) &&
            (declaringName.Contains("Fmod", StringComparison.OrdinalIgnoreCase) || declaringName.Contains("FMOD", StringComparison.Ordinal)))
            return "FMOD";
        if ((ns.StartsWith("MegaCrit.Sts2", StringComparison.Ordinal) || ns.StartsWith("Spine", StringComparison.OrdinalIgnoreCase)) &&
            declaringName.Contains("Spine", StringComparison.OrdinalIgnoreCase))
            return "SPINE";
        if (declaring == "MegaCrit.Sts2.Core.Debug.SentryService" || declaring.StartsWith("MegaCrit.Sts2.Core.Debug.SentryService/", StringComparison.Ordinal))
            return "SENTRY_INERT_WRAPPER";
        if (ns.Equals("Sentry", StringComparison.Ordinal) || ns.StartsWith("Sentry.", StringComparison.Ordinal))
            return "SENTRY_EXTERNAL";
        if (declaringName.Contains("GDExtension", StringComparison.OrdinalIgnoreCase) ||
            (ns.StartsWith("MegaCrit.Sts2", StringComparison.Ordinal) && declaringName.Contains("Native", StringComparison.OrdinalIgnoreCase)))
            return "NATIVE_EXTENSION";
        return null;
    }

    private static string BuildStep41StaticMap(Step41PreflightSnapshot preflight, Step41ClosureAudit closure)
    {
        var lines = new List<string>
        {
            "StS2 Launcher — Step 41.0 GameStartup async frontier static map",
            "Read-only Cecil evidence from the exact selected compatibility image; this step never invokes GameStartup and never restarts rendering.",
            $"Selected compatibility path: {preflight.SelectedPath}",
            $"Selected compatibility SHA-256: {preflight.SelectedSha256}",
            $"GameStartup full name: {preflight.GameStartupFullName}",
            $"GameStartup token: 0x{preflight.GameStartupToken:X8}",
            $"Async state-machine type: {preflight.StateMachineTypeName}",
            $"MoveNext token: 0x{preflight.MoveNextToken:X8}",
            $"Transitive same-sts2 startup closure methods mapped: {closure.ClosureMethods.Length}",
            $"Classified startup/platform/native boundary references: {closure.Boundaries.Length}",
            "Unresolved same-sts2 references: 0",
            "External Cecil resolution requests: 0",
            "GameStartup invoked: NO",
            "Rendering restarted: NO",
            "Step 41 policy: map exact async state machine and path-qualified closure only; keep GameStartupWrapper inert and renderer frozen.",
            string.Empty,
            preflight.DirectMap.TrimEnd(),
            string.Empty,
            "[TRANSITIVE SAME-STS2 CLOSURE METHODS]",
        };
        foreach (var method in closure.ClosureMethods)
            lines.Add($"  - token=0x{method.Token:X8}; {method.FullName}");
        lines.Add(string.Empty);
        lines.Add("[CLASSIFIED BOUNDARY REFERENCES]");
        if (closure.Boundaries.Length == 0)
            lines.Add("  - none");
        foreach (var boundary in closure.Boundaries.OrderBy(item => item.Category, StringComparer.Ordinal).ThenBy(item => item.ReferenceFullName, StringComparer.Ordinal).ThenBy(item => item.Path, StringComparer.Ordinal))
        {
            lines.Add($"  - category={boundary.Category}");
            lines.Add($"      reference: {boundary.ReferenceFullName}");
            lines.Add($"      path: {boundary.Path}");
        }
        return string.Join("\n", lines) + "\n";
    }

    private static string FormatStep41Instruction(Instruction instruction)
    {
        var operand = instruction.Operand switch
        {
            null => string.Empty,
            MethodReference method => method.FullName,
            FieldReference field => field.FullName,
            TypeReference type => type.FullName,
            Instruction target => $"IL_{target.Offset:X4}",
            Instruction[] targets => string.Join(",", targets.Select(target => $"IL_{target.Offset:X4}")),
            string text => "\"" + text.Replace("\"", "\\\"") + "\"",
            _ => instruction.Operand.ToString() ?? string.Empty,
        };
        return $"IL_{instruction.Offset:X4}: {instruction.OpCode}{(operand.Length == 0 ? string.Empty : " " + operand)}";
    }

    private static TransformedRealStS2GameStartupFrontierGateResult Step41Pass(
        TransformedRealStS2GameStartupFrontierGate gate,
        string detail)
        => new(gate, true, detail);

    private static TransformedRealStS2GameStartupFrontierGateResult Step41Fail(
        TransformedRealStS2GameStartupFrontierGate gate,
        string stage,
        Exception ex,
        bool includeDiagnostic)
        => new(gate, false,
            $"Stage: {stage}\n{ex.GetType().Name}: {ex.Message}" +
            (includeDiagnostic ? "\n" + FormatExceptionDiagnostic(ex) : string.Empty));

    private sealed record Step41BoundaryObservation(string Category, string ReferenceFullName, string Path);
    private sealed record Step41ClosureMethod(uint Token, string FullName);
    private sealed record Step41ClosureAudit(
        Step41ClosureMethod[] ClosureMethods,
        Step41BoundaryObservation[] Boundaries,
        string[] UnresolvedSameAssemblyReferences);
    private sealed record Step41PreflightSnapshot(
        string SelectedPath,
        string SelectedSha256,
        string StaticMap,
        string GameStartupFullName,
        uint GameStartupToken,
        string StateMachineTypeName,
        uint MoveNextToken,
        int ClosureMethodCount,
        Step41BoundaryObservation[] Boundaries,
        int ResolverCount,
        int HostCount,
        int PrivateCount,
        int InitializerCount,
        int RejectedCount,
        int NativeCount)
    {
        public string DirectMap { get; init; } = string.Empty;
    }
}
