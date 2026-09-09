using System.Reflection;
using Mono.Cecil;

namespace StS2Launcher.Core;

/// <summary>
/// Step 42.0 executes exactly one newly authorized GameStartup primitive: NGame.InitPools().
/// Physical Step 41 mapped the full GameStartup async frontier without invocation. The direct
/// GameStartup body reaches cloud/platform/Steam work before InitPools, so Step 42 does not invoke
/// GameStartup itself and does not reorder those unsafe operations into execution. Instead it
/// re-audits the exact selected compatibility image's InitPools same-sts2 closure with the Step-41
/// rejecting-resolver boundary classifier, requires zero classified/unresolved edges, then invokes
/// InitPools exactly once on the retained real in-tree NGame while rendering remains frozen.
/// </summary>
public sealed partial class TransformedRealStS2VeryEarlyInitialization
{
    private const string Step42InitPoolsMethodName = "InitPools";

    private Step42PreflightSnapshot? _step42Preflight;
    private bool _step42InitPoolsAudited;
    private bool _step42InvocationStarted;
    private bool _step42InvocationPassed;
    private bool _exactStep42ClosurePassed;

    public bool ExactStep42ClosurePassed => _exactStep42ClosurePassed;

    public string GetVerifiedStep42StaticMap()
        => _step42Preflight?.StaticMap
           ?? throw new InvalidOperationException("Step 42.0 Gate B has not produced the verified InitPools static map.");

    private void ResetStep42State()
    {
        ResetStartupLadderState();
        _step42Preflight = null;
        _step42InitPoolsAudited = false;
        _step42InvocationStarted = false;
        _step42InvocationPassed = false;
        _exactStep42ClosurePassed = false;
    }

    public TransformedRealStS2GameStartupInitPoolsGateResult RunStep42ClosedStep41FrozenAuthority(
        bool renderingStopped,
        Action<string>? checkpoint = null)
    {
        const TransformedRealStS2GameStartupInitPoolsGate gate = TransformedRealStS2GameStartupInitPoolsGate.ClosedStep41FrozenAuthority;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            ResetStep42State();
            var context = RequireStep42Prerequisite("Step 42 Gate A entry");
            if (!renderingStopped)
                throw new InvalidOperationException("Step 42.0 requires rendering to remain frozen after Step 41 closure.");

            Checkpoint(checkpoint, "L_A_ENTRY — requiring same-process Step-41 4/4 frozen no-invocation authority, retained real NGame singleton/parent/_window/state=2, unchanged selected compatibility bytes, and still-inert GameStartupWrapper before auditing or invoking InitPools.");
            stage = "closed Step-41 frozen authority re-verification";
            var state = RequireStep40InsertedAuthority();
            var step39 = _step39Preflight ?? throw new InvalidOperationException("Step 42.0 requires retained Step-39 selected-image authority.");
            var selectedSha256 = ComputeSha256Hex(step39.SelectedPath);
            if (!selectedSha256.Equals(step39.SelectedSha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException($"Step 42.0 selected compatibility image hash drifted. expected={step39.SelectedSha256}; actual={selectedSha256}.");
            if (_step41Preflight is null || !_step41StateMachineMapped || !_step41ClosureMapped || !_exactStep41ClosurePassed)
                throw new InvalidOperationException("Step 42.0 requires the exact same-process Step-41 GameStartup frontier map to be closed 4/4.");

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
                    ?? throw new InvalidDataException($"Step 42.0 could not locate {NGameTypeFullName} in selected compatibility authority.");
                RequireInertGameStartupWrapper(RequireLifecycleMethodByName(nGame, NGameGameStartupWrapperMethodName, 0));
                if (resolver.Requests.Count != 0)
                    throw new InvalidDataException("Step 42.0 Gate A unexpectedly attempted external Cecil resolution: " + string.Join(" | ", resolver.Requests));
            }

            _step42Preflight = new Step42PreflightSnapshot(
                step39.SelectedPath,
                step39.SelectedSha256,
                string.Empty,
                0,
                0,
                context.ManagedResolverRequests.Count,
                context.HostLoads.Count,
                context.PrivateLoads.Count,
                context.InitializerBearingRequests.Count,
                context.RejectedManagedRequests.Count,
                context.NativeLoadAttempts.Count);

            Checkpoint(checkpoint, $"L_A_PASS — Step-41 4/4 retained in same process; renderingStopped=True; NGame authority/state={state}; selectedSha256={selectedSha256}; GameStartupWrapper inert; Step41 mappedClosureMethods={_step41Preflight.ClosureMethodCount}; mappedBoundaryRefs={_step41Preflight.Boundaries.Length}; resolver/native deltas=0; InitPools invoked=NO.");
            return Step42Pass(gate,
                "Closed Step-41 frozen authority reverified.\n" +
                $"NGame state: {state}\n" +
                $"Selected SHA-256: {selectedSha256}\n" +
                "GameStartupWrapper: INERT\n" +
                "InitPools invoked: NO\n" +
                "Rendering: STOPPED");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"L_A_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return Step42Fail(gate, stage, ex, includeDiagnostic: true);
        }
    }

    public TransformedRealStS2GameStartupInitPoolsGateResult RunStep42InitPoolsStaticClosureAudit(Action<string>? checkpoint = null)
    {
        const TransformedRealStS2GameStartupInitPoolsGate gate = TransformedRealStS2GameStartupInitPoolsGate.InitPoolsStaticClosureAudit;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep42Prerequisite("Step 42 Gate B entry");
            var preflight = _step42Preflight ?? throw new InvalidOperationException("Step 42.0 Gate A must pass before Gate B.");
            if (_step42InvocationStarted)
                throw new InvalidOperationException("Step 42.0 refuses to audit InitPools after its invocation boundary has started.");

            Checkpoint(checkpoint, "L_B_ENTRY — mapping exact NGame.InitPools() and its transitive same-sts2 closure with deferred rejecting-resolver Cecil. Any platform/Steam/main-menu/deferred/FMOD/Spine/Sentry/native/OneTimeInitialization edge or unresolved same-sts2 reference fails before invocation.");
            stage = "InitPools same-sts2 static closure audit";

            Step41ClosureAudit closure;
            MethodDefinition initPools;
            string directMap;
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
                var allTypes = EnumerateTypes(module.Types).ToDictionary(type => type.FullName, StringComparer.Ordinal);
                var allMethods = allTypes.Values.SelectMany(type => type.Methods)
                    .GroupBy(method => method.FullName, StringComparer.Ordinal)
                    .ToDictionary(group => group.Key, group => group.Single(), StringComparer.Ordinal);
                var nGame = allTypes.GetValueOrDefault(NGameTypeFullName)
                    ?? throw new InvalidDataException($"Step 42.0 could not locate {NGameTypeFullName}.");
                initPools = nGame.Methods.SingleOrDefault(method =>
                    method.Name == Step42InitPoolsMethodName && !method.IsStatic && method.Parameters.Count == 0 &&
                    method.ReturnType.FullName == "System.Void" && method.HasBody)
                    ?? throw new MissingMethodException(NGameTypeFullName, "InitPools()");
                directMap = BuildStep42DirectMethodMap(initPools);
                closure = AuditStep41StartupClosure(initPools, allTypes, allMethods);
                if (resolver.Requests.Count != 0)
                    throw new InvalidDataException("Step 42.0 InitPools audit unexpectedly attempted external Cecil resolution: " + string.Join(" | ", resolver.Requests));
            }

            if (closure.UnresolvedSameAssemblyReferences.Length != 0)
                throw new InvalidDataException("Step 42.0 InitPools closure contains unresolved same-sts2 references: " + string.Join(" | ", closure.UnresolvedSameAssemblyReferences));
            if (closure.Boundaries.Length != 0)
                throw new InvalidDataException("Step 42.0 InitPools closure reaches forbidden startup/platform/native boundaries: " + string.Join(" | ", closure.Boundaries.Select(item => item.Category + ": " + item.Path)));

            var staticMap = BuildStep42StaticMap(preflight, initPools, directMap, closure);
            _step42Preflight = preflight with
            {
                StaticMap = staticMap,
                InitPoolsToken = initPools.MetadataToken.ToUInt32(),
                ClosureMethodCount = closure.ClosureMethods.Length,
            };
            _step42InitPoolsAudited = true;

            Checkpoint(checkpoint, $"L_B_PASS — InitPools token=0x{initPools.MetadataToken.ToUInt32():X8}; IL={initPools.Body.Instructions.Count}; transitiveSameSts2ClosureMethods={closure.ClosureMethods.Length}; classifiedBoundaryRefs=0; unresolvedSameSts2Refs=0; externalResolutionRequests=0; rendering remains stopped; invocation=NO.");
            return Step42Pass(gate,
                "Exact NGame.InitPools static closure audit passed.\n" +
                $"Token: 0x{initPools.MetadataToken.ToUInt32():X8}\n" +
                $"IL instructions: {initPools.Body.Instructions.Count}\n" +
                $"Transitive same-sts2 closure methods: {closure.ClosureMethods.Length}\n" +
                "Classified boundary refs: 0\nUnresolved same-sts2 refs: 0\nExternal Cecil resolution: 0\nInvocation: NO");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"L_B_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return Step42Fail(gate, stage, ex, includeDiagnostic: true);
        }
    }

    public TransformedRealStS2GameStartupInitPoolsGateResult RunStep42ControlledInitPoolsInvocation(Action<string>? checkpoint = null)
    {
        const TransformedRealStS2GameStartupInitPoolsGate gate = TransformedRealStS2GameStartupInitPoolsGate.ControlledInitPoolsInvocation;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep42Prerequisite("Step 42 Gate C entry");
            var preflight = _step42Preflight ?? throw new InvalidOperationException("Step 42.0 Gate B must pass before Gate C.");
            if (!_step42InitPoolsAudited || string.IsNullOrWhiteSpace(preflight.StaticMap) || preflight.InitPoolsToken == 0)
                throw new InvalidOperationException("Step 42.0 requires a successful exact InitPools static audit before invocation.");
            if (_step42InvocationStarted)
                throw new InvalidOperationException("Step 42.0 InitPools invocation is one-shot and cannot be retried in-process.");

            var instance = _step39NGameInstance ?? throw new InvalidOperationException("Step 42.0 retained NGame instance is absent.");
            var stateBefore = RequireStep40InsertedAuthority();
            var methodCandidates = instance.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
                .Where(method => method.Name == Step42InitPoolsMethodName && method.GetParameters().Length == 0 && method.ReturnType == typeof(void) && !method.IsGenericMethod)
                .ToArray();
            var method = methodCandidates.SingleOrDefault()
                ?? throw new MissingMethodException(instance.GetType().FullName, $"InitPools() exact declared instance void overload; candidates={string.Join(",", methodCandidates.Select(item => item.ToString()))}");
            if ((uint)method.MetadataToken != preflight.InitPoolsToken)
                throw new InvalidDataException($"Step 42.0 reflection InitPools token drifted. expected=0x{preflight.InitPoolsToken:X8}; actual=0x{method.MetadataToken:X8}.");

            var resolverBefore = context.ManagedResolverRequests.Count;
            var hostBefore = context.HostLoads.Count;
            var privateBefore = context.PrivateLoads.Count;
            var initializerBefore = context.InitializerBearingRequests.Count;
            var rejectedBefore = context.RejectedManagedRequests.Count;
            var nativeBefore = context.NativeLoadAttempts.Count;

            _step42InvocationStarted = true;
            Checkpoint(checkpoint, $"L_C_INVOKE_START — invoking exact audited NGame.InitPools() once on the retained real in-tree NGame; token=0x{preflight.InitPoolsToken:X8}; renderingStopped=True; stateBefore={stateBefore}. No GameStartup/migrations/cloud/platform/Steam/main-menu/deferred call is authorized.");
            stage = "exact controlled NGame.InitPools invocation";
            try
            {
                method.Invoke(instance, null);
            }
            catch (TargetInvocationException tie) when (tie.InnerException is not null)
            {
                throw new InvalidOperationException($"Step 42.0 NGame.InitPools inner exception: {tie.InnerException.GetType().FullName}: {tie.InnerException.Message}", tie.InnerException);
            }
            Checkpoint(checkpoint, "L_C_INVOKE_RETURNED — NGame.InitPools MethodInfo.Invoke returned to the launcher.");

            var stateAfter = RequireStep40InsertedAuthority();
            var resolverDelta = context.ManagedResolverRequests.Count - resolverBefore;
            var hostDelta = context.HostLoads.Count - hostBefore;
            var privateDelta = context.PrivateLoads.Count - privateBefore;
            var initializerDelta = context.InitializerBearingRequests.Count - initializerBefore;
            var rejectedDelta = context.RejectedManagedRequests.Count - rejectedBefore;
            var nativeDelta = context.NativeLoadAttempts.Count - nativeBefore;
            if (resolverDelta != 0 || hostDelta != 0 || privateDelta != 0 || initializerDelta != 0 || rejectedDelta != 0 || nativeDelta != 0)
                throw new InvalidDataException($"Step 42.0 InitPools escaped the sealed execution context. resolverDelta={resolverDelta}; hostDelta={hostDelta}; privateDelta={privateDelta}; initializerDelta={initializerDelta}; rejectedDelta={rejectedDelta}; nativeDelta={nativeDelta}.");
            if (stateAfter != ExpectedStateAfterEssential)
                throw new InvalidDataException($"Step 42.0 InitPools changed OneTimeInitialization state: before={stateBefore}; after={stateAfter}.");

            _step42InvocationPassed = true;
            Checkpoint(checkpoint, $"L_C_PASS — exact audited NGame.InitPools returned once; state={stateAfter}; resolverDelta=0; hostDelta=0; privateDelta=0; initializerDelta=0; rejectedDelta=0; nativeDelta=0; rendering remains stopped.");
            return Step42Pass(gate,
                "Exact audited NGame.InitPools invocation returned normally.\n" +
                $"Token: 0x{preflight.InitPoolsToken:X8}\n" +
                $"State: {stateBefore} → {stateAfter}\n" +
                "Resolver/host/private/initializer/rejected/native deltas: 0\nRendering: STOPPED\nInvocation count: 1");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"L_C_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return Step42Fail(gate, stage, ex, includeDiagnostic: true);
        }
    }

    public TransformedRealStS2GameStartupInitPoolsGateResult RunStep42FrozenPostInitPoolsConfinement(
        bool renderingStopped,
        Action<string>? checkpoint = null)
    {
        const TransformedRealStS2GameStartupInitPoolsGate gate = TransformedRealStS2GameStartupInitPoolsGate.FrozenPostInitPoolsConfinement;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep42Prerequisite("Step 42 Gate D entry");
            var preflight = _step42Preflight ?? throw new InvalidOperationException("Step 42.0 preflight authority is absent.");
            if (!_step42InitPoolsAudited || !_step42InvocationStarted || !_step42InvocationPassed)
                throw new InvalidOperationException("Step 42.0 Gate D requires a successful one-shot InitPools invocation.");
            if (!renderingStopped)
                throw new InvalidOperationException("Step 42.0 Gate D requires rendering to remain frozen.");

            Checkpoint(checkpoint, "L_D_ENTRY — proving post-InitPools frozen confinement: retained real NGame authority/state=2, renderer stopped, exact selected compatibility bytes unchanged, GameStartupWrapper still inert, and zero initializer/rejected/native escape.");
            stage = "frozen post-InitPools confinement";
            var state = RequireStep40InsertedAuthority();
            var selectedSha256 = ComputeSha256Hex(preflight.SelectedPath);
            if (!selectedSha256.Equals(preflight.SelectedSha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException($"Step 42.0 selected compatibility image changed after InitPools invocation. expected={preflight.SelectedSha256}; actual={selectedSha256}.");
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
                var nGame = module.Types.Single(type => type.FullName == NGameTypeFullName);
                RequireInertGameStartupWrapper(RequireLifecycleMethodByName(nGame, NGameGameStartupWrapperMethodName, 0));
                if (resolver.Requests.Count != 0)
                    throw new InvalidDataException("Step 42.0 Gate D unexpectedly attempted external Cecil resolution: " + string.Join(" | ", resolver.Requests));
            }

            if (context.ManagedResolverRequests.Count != preflight.ResolverCount ||
                context.HostLoads.Count != preflight.HostCount ||
                context.PrivateLoads.Count != preflight.PrivateCount ||
                context.InitializerBearingRequests.Count != preflight.InitializerCount ||
                context.RejectedManagedRequests.Count != preflight.RejectedCount ||
                context.NativeLoadAttempts.Count != preflight.NativeCount)
            {
                throw new InvalidDataException($"Step 42.0 post-InitPools context drifted from Gate-A baseline. resolver={context.ManagedResolverRequests.Count - preflight.ResolverCount}; host={context.HostLoads.Count - preflight.HostCount}; private={context.PrivateLoads.Count - preflight.PrivateCount}; initializer={context.InitializerBearingRequests.Count - preflight.InitializerCount}; rejected={context.RejectedManagedRequests.Count - preflight.RejectedCount}; native={context.NativeLoadAttempts.Count - preflight.NativeCount}.");
            }

            _exactStep42ClosurePassed = true;
            Checkpoint(checkpoint, $"L_D_PASS — post-InitPools renderingStopped=True; real NGame authority/state={state} preserved; GameStartupWrapper inert; InitPools invoked exactly once; closureMethods={preflight.ClosureMethodCount}; resolverDelta=0; hostDelta=0; privateDelta=0; initializerDelta=0; rejectedDelta=0; nativeDelta=0.");
            return Step42Pass(gate,
                "Frozen post-InitPools confinement passed.\n" +
                $"NGame state: {state}\n" +
                "GameStartupWrapper: INERT\nInitPools invocation: EXACTLY ONCE\nRendering: STOPPED\nResolver/host/private/initializer/rejected/native deltas: 0");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"L_D_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return Step42Fail(gate, stage, ex, includeDiagnostic: true);
        }
    }

    private Step35ExecutionLoadContext RequireStep42Prerequisite(string boundary)
    {
        var context = RequireStep41Prerequisite(boundary);
        if (!_exactStep41ClosurePassed || !_step41StateMachineMapped || !_step41ClosureMapped || _step41Preflight is null)
            throw new InvalidOperationException($"{boundary} requires same-process Step-41 4/4 GameStartup frontier-map authority with GameStartup still uninvoked.");
        return context;
    }

    private static string BuildStep42DirectMethodMap(MethodDefinition method)
    {
        var lines = new List<string>
        {
            "[INITPOOLS METHOD]",
            $"  token=0x{method.MetadataToken.ToUInt32():X8}",
            $"  fullname={method.FullName}",
            $"  IL instructions={method.Body.Instructions.Count}",
        };
        foreach (var instruction in method.Body.Instructions)
            lines.Add("    " + FormatStep41Instruction(instruction));
        return string.Join("\n", lines) + "\n";
    }

    private static string BuildStep42StaticMap(
        Step42PreflightSnapshot preflight,
        MethodDefinition initPools,
        string directMap,
        Step41ClosureAudit closure)
    {
        var lines = new List<string>
        {
            "StS2 Launcher — Step 42.0 controlled GameStartup InitPools pre-invocation map",
            "Read-only Cecil evidence from the exact selected compatibility image; this map is durably written before InitPools is invoked.",
            $"Selected compatibility path: {preflight.SelectedPath}",
            $"Selected compatibility SHA-256: {preflight.SelectedSha256}",
            $"InitPools full name: {initPools.FullName}",
            $"InitPools token: 0x{initPools.MetadataToken.ToUInt32():X8}",
            $"InitPools IL instructions: {initPools.Body.Instructions.Count}",
            $"Transitive same-sts2 InitPools closure methods mapped: {closure.ClosureMethods.Length}",
            $"Classified startup/platform/native boundary references: {closure.Boundaries.Length}",
            $"Unresolved same-sts2 references: {closure.UnresolvedSameAssemblyReferences.Length}",
            "External Cecil resolution requests: 0",
            "InitPools invoked while this map was built: NO",
            "Rendering restarted: NO",
            "Step 42 policy: only a zero-boundary/zero-unresolved exact InitPools closure may be invoked once; GameStartup/migrations/cloud/platform/Steam/main-menu/deferred remain unopened.",
            string.Empty,
            directMap.TrimEnd(),
            string.Empty,
            "[TRANSITIVE SAME-STS2 INITPOOLS CLOSURE METHODS]",
        };
        foreach (var method in closure.ClosureMethods)
            lines.Add($"  - token=0x{method.Token:X8}; {method.FullName}");
        return string.Join("\n", lines) + "\n";
    }

    private static TransformedRealStS2GameStartupInitPoolsGateResult Step42Pass(
        TransformedRealStS2GameStartupInitPoolsGate gate,
        string detail)
        => new(gate, true, detail);

    private static TransformedRealStS2GameStartupInitPoolsGateResult Step42Fail(
        TransformedRealStS2GameStartupInitPoolsGate gate,
        string stage,
        Exception ex,
        bool includeDiagnostic)
        => new(gate, false, includeDiagnostic
            ? $"Stage: {stage}\n{FormatExceptionDiagnostic(ex)}"
            : $"Stage: {stage}\n{ex.GetType().Name}: {ex.Message}");

    private sealed record Step42PreflightSnapshot(
        string SelectedPath,
        string SelectedSha256,
        string StaticMap,
        uint InitPoolsToken,
        int ClosureMethodCount,
        int ResolverCount,
        int HostCount,
        int PrivateCount,
        int InitializerCount,
        int RejectedCount,
        int NativeCount);
}
