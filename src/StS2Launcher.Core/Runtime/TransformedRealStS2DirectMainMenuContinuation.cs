using System.Reflection;
using System.Runtime.Loader;
using Mono.Cecil;

namespace StS2Launcher.Core;

/// <summary>
/// Steps 51-52 extend the direct main-menu ladder only after the short Step-50 pulse has closed.
/// Step 51 is a non-invoking single-player frontier map. Step 52 is a longer, still-bounded render
/// residency pulse which always refreezes before evidence is evaluated. Neither step invokes any
/// menu button handler, GameStartup, LaunchMainMenu, deferred startup, Steam initialization, or
/// native game extension.
/// </summary>
public sealed partial class TransformedRealStS2VeryEarlyInitialization
{
    private StartupLadderBaseline? _step51Baseline;
    private StartupLadderBaseline? _step52Baseline;
    private StartupLadderBaseline? _step52PostPulseBaseline;
    private string _step51DirectStaticMap = string.Empty;
    private string _step52DirectStaticMap = string.Empty;
    private bool _step51FrontierMapped;
    private bool _step51StaticMapDurablyWritten;
    private bool _step52StaticMapDurablyWritten;
    private bool _step52PulseStarted;
    private bool _step52PulsePassed;
    private bool _exactStep51ClosurePassed;
    private bool _exactStep52ClosurePassed;

    public bool ExactStep51ClosurePassed => _exactStep51ClosurePassed;
    public bool ExactStep52ClosurePassed => _exactStep52ClosurePassed;
    public bool Step52PulseStarted => _step52PulseStarted;

    private void ResetDirectMainMenuContinuationState()
    {
        _step51Baseline = null;
        _step52Baseline = null;
        _step52PostPulseBaseline = null;
        _step51DirectStaticMap = string.Empty;
        _step52DirectStaticMap = string.Empty;
        _step51FrontierMapped = false;
        _step51StaticMapDurablyWritten = false;
        _step52StaticMapDurablyWritten = false;
        _step52PulseStarted = false;
        _step52PulsePassed = false;
        _exactStep51ClosurePassed = false;
        _exactStep52ClosurePassed = false;
    }

    public void MarkStep51StaticMapDurablyWritten()
    {
        if (!_step51FrontierMapped || string.IsNullOrWhiteSpace(_step51DirectStaticMap))
            throw new InvalidOperationException("Step 51.0 static map cannot be marked durable before the exact non-invoking single-player frontier map is complete.");
        _step51StaticMapDurablyWritten = true;
    }

    public void MarkStep52StaticMapDurablyWritten()
    {
        if (string.IsNullOrWhiteSpace(_step52DirectStaticMap))
            throw new InvalidOperationException("Step 52.0 static map is absent.");
        _step52StaticMapDurablyWritten = true;
    }

    // STEP 51 — non-invoking exact single-player button frontier map.

    public TransformedRealStS2StartupLadderGateResult RunStep51ClosedStep50Authority(bool renderingStopped, Action<string>? checkpoint = null)
    {
        const int step = 51;
        const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.PrerequisiteAuthority;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep51Prerequisite("Step 51 Gate A entry");
            if (!renderingStopped)
                throw new InvalidOperationException("Step 51.0 requires Step 50 to have synchronously refrozen rendering.");
            var selected = RequireStartupLadderSelectedAuthority(step);
            _step51Baseline = CaptureStartupLadderBaseline(selected.Path, selected.Sha256, context);
            RequireStartupLadderBaselineUnchanged(context, _step51Baseline, "Step 51 Gate A");
            RequireStep48LifecycleRuntimeGuardsCurrent(context, "Step 51 Gate A guard recheck", requirePreAdmissionCounts: false);
            var menu = _step48MainMenuInstance ?? throw new InvalidOperationException("Step 51.0 retained NMainMenu absent.");
            if (RequireZeroArgBoolMethod(menu.GetType(), "IsInsideTree").Invoke(menu, null) is not true)
                throw new InvalidDataException("Step 51.0 requires retained in-tree NMainMenu authority.");
            Checkpoint(checkpoint, "M51_A_PASS — Step-50 4/4 short render-pulse authority retained; renderingStopped=True; NMainMenu remains in exact RootSceneContainer; runtime guards remain current; no single-player handler has been invoked.");
            return StartupLadderPass(step, Step51Name, gate,
                "Step-50 short render-pulse authority retained with the real menu in-tree and refrozen. A non-invoking single-player handler frontier map may now be built.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"M51_A_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return StartupLadderFail(step, Step51Name, gate, stage, ex);
        }
    }

    public TransformedRealStS2StartupLadderGateResult RunStep51SingleplayerHandlerBinding(Action<string>? checkpoint = null)
    {
        const int step = 51;
        const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.StaticAuditOrBinding;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep51Prerequisite("Step 51 Gate B entry");
            var baseline = _step51Baseline ?? throw new InvalidOperationException("Step 51.0 Gate A must pass before Gate B.");
            using var resolver = new RejectingAssemblyResolver();
            using var module = OpenStartupLadderModule(baseline.SelectedPath, resolver);
            var allTypes = EnumerateTypes(module.Types).ToDictionary(type => type.FullName, StringComparer.Ordinal);
            var mainMenu = RequireStartupLadderType(allTypes, MainMenuManagedRootTypeFullName, step);
            var pressed = RequireUniqueStartupLadderNamedMethod(mainMenu, MainMenuSingleplayerButtonPressedMethodName, step);
            var open = RequireUniqueStartupLadderNamedMethod(mainMenu, MainMenuOpenSingleplayerSubmenuMethodName, step);
            if (resolver.Requests.Count != 0)
                throw new InvalidDataException("Step 51.0 binding map unexpectedly attempted external Cecil resolution: " + string.Join(" | ", resolver.Requests));
            RequireStartupLadderBaselineUnchanged(context, baseline, "Step 51 Gate B");
            _step51DirectStaticMap =
                "StS2 Launcher — Step 51.0 single-player frontier map\n" +
                "Evidence-only; no menu handler is invoked by this step.\n" +
                $"Selected compatibility SHA-256: {baseline.SelectedSha256}\n" +
                $"SingleplayerButtonPressed token=0x{pressed.MetadataToken.ToUInt32():X8}; params={pressed.Parameters.Count}; return={pressed.ReturnType.FullName}; IL={pressed.Body.Instructions.Count}\n" +
                $"OpenSingleplayerSubmenu token=0x{open.MetadataToken.ToUInt32():X8}; params={open.Parameters.Count}; return={open.ReturnType.FullName}; IL={open.Body.Instructions.Count}\n" +
                "Rendering restarted: NO\nHandler invocation: NO\nOriginal LaunchMainMenu invoked: NO\n" +
                "[SINGLEPLAYERBUTTONPRESSED IL]\n" + string.Join("\n", pressed.Body.Instructions.Select(instruction => "  " + FormatStep41Instruction(instruction))) + "\n" +
                "[OPENSINGLEPLAYERSUBMENU IL]\n" + string.Join("\n", open.Body.Instructions.Select(instruction => "  " + FormatStep41Instruction(instruction))) + "\n";
            Checkpoint(checkpoint, $"M51_B_PASS — exact single-player handlers bound without invocation; SingleplayerButtonPressed token=0x{pressed.MetadataToken.ToUInt32():X8}/IL={pressed.Body.Instructions.Count}; OpenSingleplayerSubmenu token=0x{open.MetadataToken.ToUInt32():X8}/IL={open.Body.Instructions.Count}; externalResolution=0.");
            return StartupLadderPass(step, Step51Name, gate,
                "Exact NMainMenu single-player handler metadata was bound and direct IL recorded without invocation or rendering.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"M51_B_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return StartupLadderFail(step, Step51Name, gate, stage, ex);
        }
    }

    public TransformedRealStS2StartupLadderGateResult RunStep51SingleplayerFrontierMap(Action<string>? checkpoint = null)
    {
        const int step = 51;
        const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.ControlledAction;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep51Prerequisite("Step 51 Gate C entry");
            var baseline = _step51Baseline ?? throw new InvalidOperationException("Step 51.0 baseline absent.");
            if (string.IsNullOrWhiteSpace(_step51DirectStaticMap))
                throw new InvalidOperationException("Step 51.0 Gate B direct IL map must pass before Gate C.");
            using var resolver = new RejectingAssemblyResolver();
            using var module = OpenStartupLadderModule(baseline.SelectedPath, resolver);
            var allTypes = EnumerateTypes(module.Types).ToDictionary(type => type.FullName, StringComparer.Ordinal);
            var allMethods = BuildStartupLadderMethodMap(allTypes);
            var mainMenu = RequireStartupLadderType(allTypes, MainMenuManagedRootTypeFullName, step);
            var roots = new[]
            {
                RequireUniqueStartupLadderNamedMethod(mainMenu, MainMenuSingleplayerButtonPressedMethodName, step),
                RequireUniqueStartupLadderNamedMethod(mainMenu, MainMenuOpenSingleplayerSubmenuMethodName, step),
            };
            var guards = _step48LifecycleGuardAuthority ?? throw new InvalidOperationException("Step 51.0 requires retained Step-48 runtime-guard authority.");
            var audit = AuditStartupLadderInvocationFrontier(roots, allTypes, allMethods, guards);
            if (audit.UnresolvedSameAssemblyReferences.Length != 0)
                throw new InvalidDataException("Step 51.0 single-player frontier has unresolved same-sts2 references: " + string.Join(" | ", audit.UnresolvedSameAssemblyReferences));
            if (resolver.Requests.Count != 0)
                throw new InvalidDataException("Step 51.0 single-player frontier unexpectedly attempted external Cecil resolution: " + string.Join(" | ", resolver.Requests));
            _step51DirectStaticMap += BuildStartupLadderInvocationFrontierAppendix(audit);
            _step51FrontierMapped = true;
            RequireStartupLadderBaselineUnchanged(context, baseline, "Step 51 Gate C");
            Checkpoint(checkpoint, $"M51_C_PASS — single-player frontier mapped without invocation; immediateClosureMethods={audit.ImmediateClosureMethods.Length}; stateMachineExpansions={audit.StateMachineExpansions.Length}; immediateBoundaryRefs={audit.ImmediateBoundaries.Length}; runtimeGuardedFrontiers={audit.GuardedMethodFrontiers.Length}; deferredFrontiers={audit.DeferredMethodFrontiers.Length}; unresolved=0; externalResolution=0; renderingStopped=True.");
            return StartupLadderPass(step, Step51Name, gate,
                $"Single-player frontier mapped as evidence only: immediate methods={audit.ImmediateClosureMethods.Length}; classified boundaries={audit.ImmediateBoundaries.Length}; guarded frontiers={audit.GuardedMethodFrontiers.Length}; deferred frontiers={audit.DeferredMethodFrontiers.Length}. No button handler was invoked.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"M51_C_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return StartupLadderFail(step, Step51Name, gate, stage, ex);
        }
    }

    public TransformedRealStS2StartupLadderGateResult RunStep51FrozenNoInvocationConfinement(bool renderingStopped, Action<string>? checkpoint = null)
    {
        const int step = 51;
        const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.PostActionConfinement;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep51Prerequisite("Step 51 Gate D entry");
            var baseline = _step51Baseline ?? throw new InvalidOperationException("Step 51.0 baseline absent.");
            if (!_step51FrontierMapped || !_step51StaticMapDurablyWritten)
                throw new InvalidOperationException("Step 51.0 requires a complete durable single-player frontier map before Gate D.");
            if (!renderingStopped)
                throw new InvalidOperationException("Step 51.0 is non-invoking and requires rendering to remain frozen.");
            RequireStartupLadderBaselineUnchanged(context, baseline, "Step 51 Gate D");
            RequireStep48LifecycleRuntimeGuardsCurrent(context, "Step 51 Gate D guard recheck", requirePreAdmissionCounts: false);
            _ = RequireStep40InsertedAuthority();
            _exactStep51ClosurePassed = true;
            Checkpoint(checkpoint, "M51_D_PASS — single-player map durable; handler invocation=NO; renderingStopped=True; NMainMenu retained in-tree; runtime guards current; resolver/host/private/initializer/rejected/native deltas=0.");
            return StartupLadderPass(step, Step51Name, gate,
                "Durable non-invoking single-player frontier authority closed. The real menu remains in-tree and frozen with no runtime/context drift.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"M51_D_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return StartupLadderFail(step, Step51Name, gate, stage, ex);
        }
    }

    // STEP 52 — longer bounded rendering residency, still with synchronous refreeze.

    public TransformedRealStS2StartupLadderGateResult RunStep52ClosedStep51Authority(bool renderingStopped, Action<string>? checkpoint = null)
    {
        const int step = 52;
        const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.PrerequisiteAuthority;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep52Prerequisite("Step 52 Gate A entry");
            if (!renderingStopped)
                throw new InvalidOperationException("Step 52.0 requires rendering frozen before the sustained residency pulse.");
            var selected = RequireStartupLadderSelectedAuthority(step);
            _step52Baseline = CaptureStartupLadderBaseline(selected.Path, selected.Sha256, context);
            RequireStartupLadderBaselineUnchanged(context, _step52Baseline, "Step 52 Gate A");
            RequireStep48LifecycleRuntimeGuardsCurrent(context, "Step 52 Gate A guard recheck", requirePreAdmissionCounts: false);
            if (_step48MainMenuInstance is null || RequireZeroArgBoolMethod(_step48MainMenuInstance.GetType(), "IsInsideTree").Invoke(_step48MainMenuInstance, null) is not true)
                throw new InvalidDataException("Step 52.0 requires retained in-tree NMainMenu authority.");
            Checkpoint(checkpoint, $"M52_A_PASS — Step-51 4/4 non-invoking frontier authority retained; renderingStopped=True; NMainMenu in-tree; state=2; sustained render target={Step52SustainedRenderTargetMilliseconds}ms; pulse not armed.");
            return StartupLadderPass(step, Step52Name, gate,
                "Step-51 map authority retained with menu in-tree and frozen. A longer bounded render residency pulse may now be audited and armed once.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"M52_A_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return StartupLadderFail(step, Step52Name, gate, stage, ex);
        }
    }

    public TransformedRealStS2StartupLadderGateResult RunStep52SustainedFrameInputAudit(Action<string>? checkpoint = null)
    {
        const int step = 52;
        const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.StaticAuditOrBinding;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep52Prerequisite("Step 52 Gate B entry");
            var baseline = _step52Baseline ?? throw new InvalidOperationException("Step 52.0 Gate A must pass before Gate B.");
            var menu = _step48MainMenuInstance ?? throw new InvalidOperationException("Step 52.0 NMainMenu absent.");
            var godotAssembly = (_callbackHandoff ?? throw new InvalidOperationException("GodotSharp handoff absent.")).GodotSharpAssembly;
            var nodeType = godotAssembly.GetType("Godot.Node", true, false)!;
            var nodes = EnumerateStartupLadderMenuNodeGraph(menu, nodeType, "Step 52.0 sustained main-menu frame/input audit");
            var managedTypes = nodes.Select(item => item.Node.GetType())
                .Where(type => ReferenceEquals(type.Assembly, menu.GetType().Assembly))
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
            var guards = _step48LifecycleGuardAuthority ?? throw new InvalidOperationException("Step 52.0 requires retained Step-48 runtime guards.");
            var audit = AuditStartupLadderInvocationFrontier(roots, allTypes, allMethods, guards);
            RequireImmediateFrontierAdmissible(audit, "Step 52.0 sustained main-menu frame/input frontier under retained runtime guards");
            if (resolver.Requests.Count != 0)
                throw new InvalidDataException("Step 52.0 sustained frame/input audit attempted external Cecil resolution: " + string.Join(" | ", resolver.Requests));
            RequireStartupLadderBaselineUnchanged(context, baseline, "Step 52 Gate B");
            _step52DirectStaticMap =
                "StS2 Launcher — Step 52.0 sustained main-menu render residency static map\n" +
                $"Selected compatibility SHA-256: {baseline.SelectedSha256}\n" +
                $"Actual in-tree menu nodes: {nodes.Count}\n" +
                $"Selected managed node types: {managedTypes.Length}\n" +
                $"Immediate frame/input roots: {roots.Length}\n" +
                $"Invocation-qualified closure methods: {audit.ImmediateClosureMethods.Length}\n" +
                $"Immediate classified boundaries: {audit.ImmediateBoundaries.Length}\n" +
                $"Runtime-guarded immediate frontiers: {audit.GuardedMethodFrontiers.Length}\n" +
                $"Deferred method frontiers: {audit.DeferredMethodFrontiers.Length}\n" +
                $"Requested render residency: {Step52SustainedRenderTargetMilliseconds} ms\n" +
                $"Evidence ceiling after synchronous stop: {Step52SustainedRenderEvidenceCeilingMilliseconds} ms\n" +
                "Forbidden immediate boundaries: 0\nUnresolved same-sts2 references: 0\nRendering restarted while map built: NO\nOriginal LaunchMainMenu invoked: NO\n" +
                BuildStartupLadderInvocationFrontierAppendix(audit);
            Checkpoint(checkpoint, $"M52_B_PASS — nodes={nodes.Count}; managedTypes={managedTypes.Length}; frameInputRoots={roots.Length}; closure={audit.ImmediateClosureMethods.Length}; immediateBoundaries={audit.ImmediateBoundaries.Length}; guardedFrontiers={audit.GuardedMethodFrontiers.Length}; deferredFrontiers={audit.DeferredMethodFrontiers.Length}; forbidden/unresolved/external=0; renderingStopped=True.");
            return StartupLadderPass(step, Step52Name, gate,
                $"Sustained-render frame/input audit passed: nodes={nodes.Count}; callbacks={roots.Length}; closure={audit.ImmediateClosureMethods.Length}; forbidden/unresolved/external=0. Rendering remains frozen.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"M52_B_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return StartupLadderFail(step, Step52Name, gate, stage, ex);
        }
    }

    public void BeginStep52SustainedRenderPulse(Action<string>? checkpoint = null)
    {
        ThrowIfDisposed();
        RequireStep52Prerequisite("Step 52 Gate C pulse start");
        if (!_step52StaticMapDurablyWritten || string.IsNullOrWhiteSpace(_step52DirectStaticMap))
            throw new InvalidOperationException("Step 52.0 requires the sustained frame/input static map durably written before rendering starts.");
        if (_step52PulseStarted)
            throw new InvalidOperationException("Step 52.0 sustained render pulse is one-shot in-process.");
        _step52PulseStarted = true;
        Checkpoint(checkpoint, $"M52_C_PULSE_ARMED — first/only sustained main-menu render pulse authorized; requested stop delay={Step52SustainedRenderTargetMilliseconds}ms; evidence ceiling={Step52SustainedRenderEvidenceCeilingMilliseconds}ms. First managed continuation must StopRendering before telemetry.");
    }

    public TransformedRealStS2StartupLadderGateResult RunStep52SustainedRenderPulseEvidence(
        bool startReturned,
        bool renderingActiveAfterStart,
        bool stopReturned,
        bool renderingActiveAfterStop,
        double elapsedMilliseconds,
        Action<string>? checkpoint = null)
    {
        const int step = 52;
        const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.ControlledAction;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep52Prerequisite("Step 52 Gate C evidence");
            var baseline = _step52Baseline ?? throw new InvalidOperationException("Step 52.0 baseline absent.");
            if (!_step52PulseStarted)
                throw new InvalidOperationException("Step 52.0 render evidence arrived before one-shot arm.");
            stage = "sustained direct-main-menu render residency + synchronous refreeze";
            if (!startReturned || !renderingActiveAfterStart)
                throw new InvalidOperationException($"Step 52.0 StartRendering failed. returned={startReturned}; activeAfterStart={renderingActiveAfterStart}.");
            if (!stopReturned || renderingActiveAfterStop)
                throw new InvalidOperationException($"Step 52.0 StopRendering failed to refreeze synchronously. returned={stopReturned}; activeAfterStop={renderingActiveAfterStop}.");
            if (elapsedMilliseconds < Step52SustainedRenderTargetMilliseconds || elapsedMilliseconds > Step52SustainedRenderEvidenceCeilingMilliseconds)
                throw new InvalidDataException($"Step 52.0 render residency timing outside accepted physical evidence window. requested={Step52SustainedRenderTargetMilliseconds}ms; observed={elapsedMilliseconds:F1}ms; maximum={Step52SustainedRenderEvidenceCeilingMilliseconds}ms.");
            if (_step48MainMenuInstance is null || RequireZeroArgBoolMethod(_step48MainMenuInstance.GetType(), "IsInsideTree").Invoke(_step48MainMenuInstance, null) is not true)
                throw new InvalidDataException("Step 52.0 NMainMenu left the SceneTree during sustained rendering.");
            var initializerDelta = context.InitializerBearingRequests.Count - baseline.InitializerCount;
            var rejectedDelta = context.RejectedManagedRequests.Count - baseline.RejectedCount;
            var nativeDelta = context.NativeLoadAttempts.Count - baseline.NativeCount;
            if (initializerDelta != 0 || rejectedDelta != 0 || nativeDelta != 0)
                throw new InvalidDataException($"Step 52.0 sustained pulse crossed a forbidden confinement boundary. initializer={initializerDelta}; rejected={rejectedDelta}; native={nativeDelta}.");
            _step52PostPulseBaseline = CaptureStartupLadderBaseline(baseline.SelectedPath, baseline.SelectedSha256, context);
            _step52PulsePassed = true;
            Checkpoint(checkpoint, $"M52_C_PASS — StartRendering returned={startReturned}; activeAfterStart={renderingActiveAfterStart}; StopRendering returned={stopReturned}; activeAfterStop={renderingActiveAfterStop}; observedElapsedMs={elapsedMilliseconds:F1}; resolverDelta={context.ManagedResolverRequests.Count - baseline.ResolverCount}; hostDelta={context.HostLoads.Count - baseline.HostCount}; privateDelta={context.PrivateLoads.Count - baseline.PrivateCount}; initializerDelta=0; rejectedDelta=0; nativeDelta=0.");
            return StartupLadderPass(step, Step52Name, gate,
                $"Sustained main-menu rendering ran for {elapsedMilliseconds:F1} ms and synchronously refroze. NMainMenu remained in-tree and no initializer/rejected/native escape occurred.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"M52_C_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return StartupLadderFail(step, Step52Name, gate, stage, ex);
        }
    }

    public TransformedRealStS2StartupLadderGateResult RunStep52FrozenPostResidencyConfinement(bool renderingStopped, Action<string>? checkpoint = null)
    {
        const int step = 52;
        const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.PostActionConfinement;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep52Prerequisite("Step 52 Gate D entry");
            if (!_step52PulsePassed || !_step52StaticMapDurablyWritten)
                throw new InvalidOperationException("Step 52.0 Gate D requires a successful sustained pulse and durable pre-pulse map.");
            if (!renderingStopped)
                throw new InvalidOperationException("Step 52.0 Gate D requires rendering synchronously refrozen.");
            var post = _step52PostPulseBaseline ?? throw new InvalidOperationException("Step 52.0 post-pulse baseline absent.");
            RequireStartupLadderBaselineUnchanged(context, post, "Step 52 Gate D");
            RequireStep48LifecycleRuntimeGuardsCurrent(context, "Step 52 Gate D guard recheck", requirePreAdmissionCounts: false);
            if (_step48MainMenuInstance is null || RequireZeroArgBoolMethod(_step48MainMenuInstance.GetType(), "IsInsideTree").Invoke(_step48MainMenuInstance, null) is not true)
                throw new InvalidDataException("Step 52.0 NMainMenu is not retained in-tree after refreeze.");
            _ = RequireStep40InsertedAuthority();
            _exactStep52ClosurePassed = true;
            Checkpoint(checkpoint, "M52_D_PASS — sustained render residency closed; renderingStopped=True; NMainMenu retained in exact RootSceneContainer; state=2; runtime guards current; post-pulse resolver/host/private/initializer/rejected/native drift=0.");
            return StartupLadderPass(step, Step52Name, gate,
                "Sustained direct-main-menu render residency closed with synchronous refreeze, retained real NMainMenu/state authority, and frozen post-pulse confinement.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"M52_D_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return StartupLadderFail(step, Step52Name, gate, stage, ex);
        }
    }

    private Step35ExecutionLoadContext RequireStep51Prerequisite(string boundary)
    {
        var context = RequireStep50Prerequisite(boundary);
        if (!_exactStep50ClosurePassed || !_step50PulsePassed || !_step50StaticMapDurablyWritten)
            throw new InvalidOperationException(boundary + " requires same-process Step-50 4/4 short main-menu render-pulse authority.");
        return context;
    }

    private Step35ExecutionLoadContext RequireStep52Prerequisite(string boundary)
    {
        var context = RequireStep51Prerequisite(boundary);
        if (!_exactStep51ClosurePassed || !_step51FrontierMapped || !_step51StaticMapDurablyWritten)
            throw new InvalidOperationException(boundary + " requires same-process Step-51 4/4 durable non-invoking single-player frontier authority.");
        return context;
    }

    private static MethodDefinition RequireUniqueStartupLadderNamedMethod(TypeDefinition type, string name, int step)
    {
        var candidates = type.Methods.Where(method => method.Name == name && method.HasBody).ToArray();
        return candidates.SingleOrDefault()
            ?? throw new MissingMethodException(type.FullName, $"{name}; Step {step}.0 candidates={string.Join(" | ", candidates.Select(method => method.FullName))}");
    }
}
