using System.Collections;
using System.Reflection;
using System.Runtime.Loader;
using System.Security.Cryptography;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace StS2Launcher.Core;

/// <summary>
/// Steps 47-52 direct-main-menu path. This path intentionally does not invoke NGame.LaunchMainMenu,
/// NGame.LoadDeferredStartupAssetsAsync, OneTimeInitialization.ExecuteDeferred, Steam command-line join,
/// or any native game GDExtension. It extracts the exact menu resources from the receipt-backed PCK,
/// prepares a private Spine-neutral background derivative, loads/instantiates the real main-menu scene,
/// admits it into the already-proven NGame.RootSceneContainer while rendering remains frozen, and finally
/// permits one short render pulse after an execution-opcode-qualified frame/input audit, maps the single-player frontier without invoking it, and finally permits one longer bounded render residency pulse.
/// </summary>
public sealed partial class TransformedRealStS2VeryEarlyInitialization
{
    public const string MainMenuResourcePath = "res://scenes/screens/main_menu.tscn";
    public const int ClosedMainMenuBytes = 19_087;
    public const string ClosedMainMenuSha256 = "402b03596092097ffd7742a482642d740a293aa68dad6645f8b0c5aeab3376c0";
    public const string MainMenuBackgroundResourcePath = "res://scenes/backgrounds/main_menu_bg.tscn";
    public const int ClosedMainMenuBackgroundBytes = 7_916;
    public const string ClosedMainMenuBackgroundSha256 = "133a2ce2e05fb8d2e72a8e2087019cb5405b105636e16889525e53cc379aeab7";
    public const int Step47SpineNeutralBackgroundBytes = 6_375;
    public const string Step47SpineNeutralBackgroundSha256 = "0cf0c664f97e36325ef1645986af4ef5efaeff41f0ef0ee8180d115c381b8f3f";
    public const string Step47WorkRootName = "Step47-DirectMainMenu";
    public const string Step47BackgroundDerivativeFileName = "main_menu_bg-step47-spine-neutral.tscn";
    public const string MainMenuManagedRootTypeFullName = "MegaCrit.Sts2.Core.Nodes.Screens.MainMenu.NMainMenu";
    private const string MainMenuCheckCommandLineArgsMethodName = "CheckCommandLineArgs";
    private const string MainMenuSingleplayerButtonPressedMethodName = "SingleplayerButtonPressed";
    private const string MainMenuOpenSingleplayerSubmenuMethodName = "OpenSingleplayerSubmenu";
    private const string PlatformSetRichPresenceMethodName = "SetRichPresence";
    public const int Step52SustainedRenderTargetMilliseconds = 1_500;
    public const int Step52SustainedRenderEvidenceCeilingMilliseconds = 6_000;

    private StartupLadderBaseline? _step47PostLoadBaseline;
    private StartupLadderBaseline? _step48Baseline;
    private StartupLadderBaseline? _step48PostInstantiationBaseline;
    private StartupLadderBaseline? _step49Baseline;
    private StartupLadderBaseline? _step49PostAdmissionBaseline;
    private StartupLadderBaseline? _step50Baseline;
    private StartupLadderBaseline? _step50PostPulseBaseline;
    private StartupLadderRuntimeGuardAuthority? _step48LifecycleGuardAuthority;

    private string _step47DirectStaticMap = string.Empty;
    private string _step48DirectStaticMap = string.Empty;
    private string _step49DirectStaticMap = string.Empty;
    private string _step50DirectStaticMap = string.Empty;
    private bool _step47StaticMapDurablyWritten;
    private bool _step47ResourceLoadStarted;
    private bool _step47ResourceLoadPassed;
    private bool _step48InstantiationStarted;
    private bool _step48InstantiationPassed;
    private bool _step48StaticMapDurablyWritten;
    private bool _step49AdmissionStarted;
    private bool _step49AdmissionPassed;
    private bool _step49StaticMapDurablyWritten;
    private bool _step50StaticMapDurablyWritten;
    private bool _step50PulseStarted;
    private bool _step50PulsePassed;
    private bool _exactStep48ClosurePassed;
    private bool _exactStep49ClosurePassed;
    private bool _exactStep50ClosurePassed;

    private string _step47BackgroundDerivativePath = string.Empty;
    private object? _step47BackgroundPackedScene;
    private object? _step47MainMenuPackedScene;
    private object? _step48MainMenuInstance;
    private object? _step49RootSceneContainer;
    private int _step49ChildrenBefore;
    private int _step49ChildrenAfter;

    public bool ExactStep48ClosurePassed => _exactStep48ClosurePassed;
    public bool ExactStep49ClosurePassed => _exactStep49ClosurePassed;
    public bool ExactStep50ClosurePassed => _exactStep50ClosurePassed;
    public bool Step48InstantiationStarted => _step48InstantiationStarted;
    public bool Step49AdmissionStarted => _step49AdmissionStarted;
    public bool Step50PulseStarted => _step50PulseStarted;

    public void MarkStep47StaticMapDurablyWritten()
    {
        if (string.IsNullOrWhiteSpace(_step47DirectStaticMap))
            throw new InvalidOperationException("Step 47.0 static map is absent.");
        _step47StaticMapDurablyWritten = true;
    }

    public void MarkStep48StaticMapDurablyWritten()
    {
        if (!_step48InstantiationPassed || string.IsNullOrWhiteSpace(_step48DirectStaticMap))
            throw new InvalidOperationException("Step 48.0 static map cannot be marked durable before off-tree instantiation/lifecycle audit passes.");
        _step48StaticMapDurablyWritten = true;
    }

    public void MarkStep49StaticMapDurablyWritten()
    {
        if (string.IsNullOrWhiteSpace(_step49DirectStaticMap))
            throw new InvalidOperationException("Step 49.0 static map is absent.");
        _step49StaticMapDurablyWritten = true;
    }

    public void MarkStep50StaticMapDurablyWritten()
    {
        if (string.IsNullOrWhiteSpace(_step50DirectStaticMap))
            throw new InvalidOperationException("Step 50.0 static map is absent.");
        _step50StaticMapDurablyWritten = true;
    }

    private void ResetDirectMainMenuLadderState()
    {
        _step47PostLoadBaseline = null;
        _step48Baseline = null;
        _step48PostInstantiationBaseline = null;
        _step49Baseline = null;
        _step49PostAdmissionBaseline = null;
        _step50Baseline = null;
        _step50PostPulseBaseline = null;
        _step48LifecycleGuardAuthority = null;
        _step47DirectStaticMap = string.Empty;
        _step48DirectStaticMap = string.Empty;
        _step49DirectStaticMap = string.Empty;
        _step50DirectStaticMap = string.Empty;
        _step47StaticMapDurablyWritten = false;
        _step47ResourceLoadStarted = false;
        _step47ResourceLoadPassed = false;
        _step48InstantiationStarted = false;
        _step48InstantiationPassed = false;
        _step48StaticMapDurablyWritten = false;
        _step49AdmissionStarted = false;
        _step49AdmissionPassed = false;
        _step49StaticMapDurablyWritten = false;
        _step50StaticMapDurablyWritten = false;
        _step50PulseStarted = false;
        _step50PulsePassed = false;
        _exactStep47ClosurePassed = false;
        _exactStep48ClosurePassed = false;
        _exactStep49ClosurePassed = false;
        _exactStep50ClosurePassed = false;
        _step47BackgroundDerivativePath = string.Empty;
        _step47BackgroundPackedScene = null;
        _step47MainMenuPackedScene = null;
        _step48MainMenuInstance = null;
        _step49RootSceneContainer = null;
        _step49ChildrenBefore = 0;
        _step49ChildrenAfter = 0;
        ResetDirectMainMenuContinuationState();
    }

    // STEP 46 helper — invocation-opcode-qualified map.

    private static StartupLadderInvocationFrontierAudit AuditStartupLadderInvocationFrontier(
        IEnumerable<MethodDefinition> roots,
        IReadOnlyDictionary<string, TypeDefinition> allTypes,
        IReadOnlyDictionary<string, MethodDefinition> allMethods,
        StartupLadderRuntimeGuardAuthority? runtimeGuards = null)
    {
        var queue = new Queue<(MethodDefinition Method, string Path)>();
        foreach (var root in roots)
            queue.Enqueue((root, root.FullName));

        var visited = new HashSet<uint>();
        var closure = new SortedDictionary<uint, Step41ClosureMethod>();
        var immediateBoundaries = new SortedDictionary<string, Step41BoundaryObservation>(StringComparer.Ordinal);
        var deferred = new SortedDictionary<string, StartupLadderDeferredMethodFrontier>(StringComparer.Ordinal);
        var guarded = new SortedDictionary<string, StartupLadderGuardedMethodFrontier>(StringComparer.Ordinal);
        var unresolved = new SortedSet<string>(StringComparer.Ordinal);
        var expansions = new SortedSet<string>(StringComparer.Ordinal);

        while (queue.Count != 0)
        {
            var (current, path) = queue.Dequeue();
            var token = current.MetadataToken.ToUInt32();
            if (!visited.Add(token))
                continue;
            closure[token] = new Step41ClosureMethod(token, current.FullName);

            ExpandInvocationFrontierStateMachine(current, path, allTypes, queue, expansions, unresolved);
            if (!current.HasBody)
                continue;

            foreach (var instruction in current.Body.Instructions)
            {
                if (instruction.Operand is not MethodReference reference)
                    continue;

                if (IsImmediateExecutionOpcode(instruction.OpCode.Code))
                {
                    if (TryCreateStartupLadderGuardedFrontier(reference, path, runtimeGuards, out var guardedFrontier))
                    {
                        guarded.TryAdd(guardedFrontier.ReferenceFullName + "|" + guardedFrontier.Path, guardedFrontier);
                        continue;
                    }

                    var category = ClassifyStep41Boundary(reference);
                    if (category is not null)
                    {
                        var observation = new Step41BoundaryObservation(category, reference.FullName, path + " -> " + reference.FullName);
                        immediateBoundaries.TryAdd(category + "|" + reference.FullName + "|" + path, observation);
                    }

                    if (TryResolveInvocationFrontierMethod(reference, allTypes, allMethods, out var definition, out var resolutionError) && definition is not null)
                        queue.Enqueue((definition, path + " -> " + definition.FullName));
                    else if (resolutionError is not null)
                        unresolved.Add(current.FullName + " -> " + reference.FullName + " [" + resolutionError + "]");
                }
                else if (instruction.OpCode.Code is Code.Ldftn or Code.Ldvirtftn or Code.Ldtoken)
                {
                    var category = ClassifyStep41Boundary(reference);
                    var item = new StartupLadderDeferredMethodFrontier(
                        instruction.OpCode.Code.ToString(),
                        reference.FullName,
                        category,
                        path + " --" + instruction.OpCode.Code + "--> " + reference.FullName);
                    deferred.TryAdd(item.Opcode + "|" + item.ReferenceFullName + "|" + item.Path, item);

                    // Resolve same-module deferred references for map integrity, but deliberately do not traverse them.
                    _ = TryResolveInvocationFrontierMethod(reference, allTypes, allMethods, out _, out var resolutionError);
                    if (resolutionError is not null)
                        unresolved.Add(current.FullName + " -> deferred " + reference.FullName + " [" + resolutionError + "]");
                }
                else
                {
                    var item = new StartupLadderDeferredMethodFrontier(
                        "NON_EXECUTING_" + instruction.OpCode.Code,
                        reference.FullName,
                        ClassifyStep41Boundary(reference),
                        path + " --" + instruction.OpCode.Code + "--> " + reference.FullName);
                    deferred.TryAdd(item.Opcode + "|" + item.ReferenceFullName + "|" + item.Path, item);
                }
            }
        }

        return new StartupLadderInvocationFrontierAudit(
            closure.Values.ToArray(),
            immediateBoundaries.Values.ToArray(),
            deferred.Values.ToArray(),
            guarded.Values.ToArray(),
            unresolved.ToArray(),
            expansions.ToArray());
    }

    private static bool TryCreateStartupLadderGuardedFrontier(
        MethodReference reference,
        string path,
        StartupLadderRuntimeGuardAuthority? guards,
        out StartupLadderGuardedMethodFrontier frontier)
    {
        frontier = null!;
        if (guards is null || !guards.RehearsalPassed)
            return false;

        var declaring = GetStep41DefinitionTypeName(reference.DeclaringType);
        string? reason = null;
        if (declaring == MainMenuManagedRootTypeFullName && reference.Name == MainMenuCheckCommandLineArgsMethodName && reference.Parameters.Count == 0)
            reason = "exact CheckCommandLineArgs() was already invoked once off-tree with an empty Godot command line and returned with zero context/native/tree drift";
        else if (declaring == SaveManagerTypeFullName && reference.Name == "get_Instance" && reference.Parameters.Count == 0)
            reason = "SaveManager _mockInstance is null, production _instance is non-null, and exact get_Instance() rehearsal returned that same instance with zero context/native drift";
        else if (declaring == PlatformUtilTypeFullName && reference.Name == PlatformSetRichPresenceMethodName && reference.Parameters.Count == 3)
            reason = "PrimaryPlatform resolves to exact NullPlatformUtilStrategy and exact PlatformUtil.SetRichPresence rehearsal returned with zero context/native drift";

        if (reason is null)
            return false;

        frontier = new StartupLadderGuardedMethodFrontier(reference.FullName, reason, path + " -> [RUNTIME-GUARDED] " + reference.FullName);
        return true;
    }

    private static bool IsImmediateExecutionOpcode(Code code)
        => code is Code.Call or Code.Callvirt or Code.Newobj or Code.Jmp;

    private static bool TryResolveInvocationFrontierMethod(
        MethodReference reference,
        IReadOnlyDictionary<string, TypeDefinition> allTypes,
        IReadOnlyDictionary<string, MethodDefinition> allMethods,
        out MethodDefinition? definition,
        out string? error)
    {
        definition = null;
        error = null;
        var candidate = reference is GenericInstanceMethod generic ? generic.ElementMethod : reference;
        var declaringDefinitionName = GetStep41DefinitionTypeName(candidate.DeclaringType);
        if (!allTypes.TryGetValue(declaringDefinitionName, out var declaringDefinition))
            return false; // external/BCL/Godot reference; exact external resolution is intentionally not requested.
        if (candidate is MethodDefinition direct)
        {
            definition = direct;
            return true;
        }
        if (allMethods.TryGetValue(candidate.FullName, out var byFullName))
        {
            definition = byFullName;
            return true;
        }

        var fallback = declaringDefinition.Methods
            .Where(method => method.Name == candidate.Name &&
                             method.Parameters.Count == candidate.Parameters.Count &&
                             method.GenericParameters.Count == candidate.GenericParameters.Count)
            .ToArray();
        if (fallback.Length == 1)
        {
            definition = fallback[0];
            return true;
        }
        error = $"declaringDef={declaringDefinitionName}; fallbackCandidates={fallback.Length}";
        return false;
    }

    private static void ExpandInvocationFrontierStateMachine(
        MethodDefinition method,
        string path,
        IReadOnlyDictionary<string, TypeDefinition> allTypes,
        Queue<(MethodDefinition Method, string Path)> queue,
        ISet<string> expansions,
        ISet<string> unresolved)
    {
        var attributes = method.CustomAttributes
            .Where(attribute => StartupLadderCompilerStateMachineAttributes.Contains(attribute.AttributeType.FullName, StringComparer.Ordinal))
            .ToArray();
        if (attributes.Length == 0)
            return;
        if (attributes.Length != 1)
        {
            unresolved.Add(method.FullName + $" has {attributes.Length} compiler state-machine attributes");
            return;
        }
        var attribute = attributes[0];
        if (attribute.ConstructorArguments.Count != 1 || attribute.ConstructorArguments[0].Value is not TypeReference stateMachineReference)
        {
            unresolved.Add(method.FullName + " has malformed " + attribute.AttributeType.FullName);
            return;
        }
        var stateMachineName = GetStep41DefinitionTypeName(stateMachineReference);
        if (!allTypes.TryGetValue(stateMachineName, out var stateMachineType))
        {
            unresolved.Add(method.FullName + " state-machine type not found: " + stateMachineName);
            return;
        }
        var moveNext = stateMachineType.Methods.Where(item => item.Name == "MoveNext" && item.Parameters.Count == 0 && item.HasBody).ToArray();
        if (moveNext.Length != 1)
        {
            unresolved.Add(method.FullName + " state-machine MoveNext candidates=" + moveNext.Length);
            return;
        }
        expansions.Add(attribute.AttributeType.Name + ": " + method.FullName + " -> " + moveNext[0].FullName);
        queue.Enqueue((moveNext[0], path + " -> [" + attribute.AttributeType.Name + "] " + moveNext[0].FullName));
    }

    private static string BuildStartupLadderInvocationFrontierAppendix(StartupLadderInvocationFrontierAudit audit)
    {
        var lines = new List<string>
        {
            string.Empty,
            "[EXECUTION-OPCODE-QUALIFIED LAUNCHMAINMENU FRONTIER]",
            $"Immediate closure methods: {audit.ImmediateClosureMethods.Length}",
            $"Compiler state-machine MoveNext expansions reached from immediate edges: {audit.StateMachineExpansions.Length}",
            $"Immediate classified boundary references: {audit.ImmediateBoundaries.Length}",
            $"Deferred/non-executing method-reference frontiers: {audit.DeferredMethodFrontiers.Length}",
            $"Runtime-guarded immediate frontiers: {audit.GuardedMethodFrontiers.Length}",
            "Unresolved same-sts2 references: 0",
            "External Cecil resolution requests: 0",
            "Policy: call/callvirt/newobj/jmp traverse; ldftn/ldvirtftn/ldtoken are recorded but never traversed. This map never authorizes or invokes original LaunchMainMenu.",
            string.Empty,
            "[IMMEDIATE CLOSURE METHODS]",
        };
        foreach (var method in audit.ImmediateClosureMethods)
            lines.Add($"  - token=0x{method.Token:X8}; {method.FullName}");
        lines.Add(string.Empty);
        lines.Add("[IMMEDIATE CLASSIFIED BOUNDARIES — EVIDENCE ONLY, NOT AUTHORIZATION]");
        if (audit.ImmediateBoundaries.Length == 0)
            lines.Add("  - none");
        foreach (var boundary in audit.ImmediateBoundaries)
        {
            lines.Add("  - category=" + boundary.Category);
            lines.Add("      reference: " + boundary.ReferenceFullName);
            lines.Add("      path: " + boundary.Path);
        }
        lines.Add(string.Empty);
        lines.Add("[DEFERRED / NON-EXECUTING METHOD FRONTIERS]");
        if (audit.DeferredMethodFrontiers.Length == 0)
            lines.Add("  - none");
        foreach (var frontier in audit.DeferredMethodFrontiers)
            lines.Add($"  - opcode={frontier.Opcode}; category={frontier.Category ?? "UNCLASSIFIED"}; reference={frontier.ReferenceFullName}; path={frontier.Path}");
        lines.Add(string.Empty);
        lines.Add("[RUNTIME-GUARDED IMMEDIATE FRONTIERS]");
        if (audit.GuardedMethodFrontiers.Length == 0)
            lines.Add("  - none");
        foreach (var guarded in audit.GuardedMethodFrontiers)
        {
            lines.Add("  - reference=" + guarded.ReferenceFullName);
            lines.Add("      reason: " + guarded.Reason);
            lines.Add("      path: " + guarded.Path);
        }
        lines.Add(string.Empty);
        lines.Add("[COMPILER STATE-MACHINE EXPANSIONS]");
        if (audit.StateMachineExpansions.Length == 0)
            lines.Add("  - none");
        foreach (var expansion in audit.StateMachineExpansions)
            lines.Add("  - " + expansion);
        return string.Join("\n", lines) + "\n";
    }

    // STEP 47 — exact resource authority + private Spine-neutral background + PackedScene loads.

    public TransformedRealStS2StartupLadderGateResult RunStep47ClosedStep46MapAuthority(bool renderingStopped, Action<string>? checkpoint = null)
    {
        const int step = 47;
        const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.PrerequisiteAuthority;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep47Prerequisite("Step 47 Gate A entry");
            if (!renderingStopped)
                throw new InvalidOperationException("Step 47.0 requires rendering frozen.");
            Checkpoint(checkpoint, "M47_A_ENTRY — requiring same-process Step-46 4/4 durable frontier-map authority. Original LaunchMainMenu remains forbidden; Step 47 will work only with exact PCK resources and a private Spine-neutral derivative.");
            stage = "Step-46 map authority re-verification";
            var selected = RequireStartupLadderSelectedAuthority(step);
            var state = RequireStep40InsertedAuthority();
            _step47Baseline = CaptureStartupLadderBaseline(selected.Path, selected.Sha256, context);
            RequireStartupLadderBaselineUnchanged(context, _step47Baseline, "Step 47 Gate A");
            Checkpoint(checkpoint, $"M47_A_PASS — Step-46 authority retained; renderingStopped=True; state={state}; selectedSha256={selected.Sha256}; originalLaunchMainMenuInvoked=NO.");
            return StartupLadderPass(step, Step47Name, gate,
                "Closed Step-46 frontier-map authority retained with frozen rendering. Original LaunchMainMenu remains uninvoked/unreachable; exact PCK menu-resource preparation is authorized.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"M47_A_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return StartupLadderFail(step, Step47Name, gate, stage, ex);
        }
    }

    public TransformedRealStS2StartupLadderGateResult RunStep47MainMenuResourcePreparation(Action<string>? checkpoint = null)
    {
        const int step = 47;
        const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.StaticAuditOrBinding;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep47Prerequisite("Step 47 Gate B entry");
            var baseline = _step47Baseline ?? throw new InvalidOperationException("Step 47.0 Gate A must pass before Gate B.");
            RequireStartupLadderBaselineUnchanged(context, baseline, "Step 47 Gate B entry");
            var pck = RequireEssentialResourcePackHandoff().PackAbsolutePath;
            Checkpoint(checkpoint, "M47_B_ENTRY — extracting exact main_menu.tscn and main_menu_bg.tscn from the receipt-backed PCK, then preparing only a private Spine-neutral background derivative. Trusted install/PCK bytes remain immutable.");
            stage = "exact PCK main-menu resource extraction";
            var mainMenu = ExtractExactStartupLadderPckEntry(pck, MainMenuResourcePath, ClosedMainMenuBytes, ClosedMainMenuSha256, step);
            var background = ExtractExactStartupLadderPckEntry(pck, MainMenuBackgroundResourcePath, ClosedMainMenuBackgroundBytes, ClosedMainMenuBackgroundSha256, step);
            var mainMenuText = new UTF8Encoding(false, true).GetString(mainMenu.Bytes);
            var backgroundText = new UTF8Encoding(false, true).GetString(background.Bytes);
            RequireExactCount(mainMenuText, $"path=\"{MainMenuBackgroundResourcePath}\"", 1, "main-menu background ext_resource");
            RequireExactCount(mainMenuText, "path=\"res://src/Core/Nodes/Screens/MainMenu/NMainMenu.cs\"", 1, "NMainMenu script ext_resource");
            RequireExactCount(backgroundText, "type=\"SpineSprite\"", 3, "main-menu background SpineSprite nodes");
            RequireExactCount(backgroundText, "path=\"res://src/Core/Nodes/Animation/NSpineAutoPlayer.cs\"", 1, "NSpineAutoPlayer ext_resource");

            stage = "deterministic private Spine-neutral background derivative";
            var derivativeText = BuildStep47SpineNeutralBackground(backgroundText);
            var derivativeBytes = new UTF8Encoding(false).GetBytes(derivativeText);
            var derivativeSha = Convert.ToHexString(SHA256.HashData(derivativeBytes)).ToLowerInvariant();
            if (derivativeBytes.Length != Step47SpineNeutralBackgroundBytes || !derivativeSha.Equals(Step47SpineNeutralBackgroundSha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException($"Step 47.0 Spine-neutral background derivative drifted. bytes={derivativeBytes.Length}; sha256={derivativeSha}.");
            if (derivativeText.Contains("Spine", StringComparison.Ordinal) || derivativeText.Contains("skeleton_data_res", StringComparison.Ordinal) || derivativeText.Contains("NSpineAutoPlayer", StringComparison.Ordinal))
                throw new InvalidDataException("Step 47.0 Spine-neutral background derivative still contains a Spine/NSpineAutoPlayer token.");

            var workRoot = ResolveChildPath(_launcherDataRoot, Step47WorkRootName, "Step-47 work root");
            Directory.CreateDirectory(workRoot);
            _step47BackgroundDerivativePath = ResolveChildPath(workRoot, Step47BackgroundDerivativeFileName, "Step-47 background derivative");
            File.WriteAllBytes(_step47BackgroundDerivativePath, derivativeBytes);
            var readback = File.ReadAllBytes(_step47BackgroundDerivativePath);
            if (!readback.AsSpan().SequenceEqual(derivativeBytes))
                throw new IOException("Step 47.0 background derivative immediate readback mismatch.");

            _step47DirectStaticMap = BuildStep47DirectResourceMap(mainMenu, background, derivativeSha, derivativeText);
            RequireStartupLadderBaselineUnchanged(context, baseline, "Step 47 Gate B exit");
            Checkpoint(checkpoint, $"M47_B_PASS — mainMenuBytes={mainMenu.Bytes.Length}; mainMenuSha256={ClosedMainMenuSha256}; backgroundBytes={background.Bytes.Length}; backgroundSha256={ClosedMainMenuBackgroundSha256}; derivativeBytes={derivativeBytes.Length}; derivativeSha256={derivativeSha}; SpineTokens=0; installMutation=NO; resourceLoad=NO.");
            return StartupLadderPass(step, Step47Name, gate,
                $"Exact PCK main-menu resources verified and private Spine-neutral background derivative prepared. main_menu={mainMenu.Bytes.Length} bytes; background={background.Bytes.Length} bytes; derivative={derivativeBytes.Length} bytes / {derivativeSha}; trusted install unchanged; no Godot resource load yet.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"M47_B_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return StartupLadderFail(step, Step47Name, gate, stage, ex);
        }
    }

    public TransformedRealStS2StartupLadderGateResult RunStep47MainMenuPackedSceneLoad(Action<string>? checkpoint = null)
    {
        const int step = 47;
        const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.ControlledAction;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep47Prerequisite("Step 47 Gate C entry");
            var baseline = _step47Baseline ?? throw new InvalidOperationException("Step 47.0 baseline is absent.");
            if (!_step47StaticMapDurablyWritten)
                throw new InvalidOperationException("Step 47.0 requires its exact resource/derivative static map durably written before any Godot resource load.");
            if (_step47ResourceLoadStarted)
                throw new InvalidOperationException("Step 47.0 resource load boundary is one-shot in-process.");
            if (string.IsNullOrWhiteSpace(_step47BackgroundDerivativePath) || !File.Exists(_step47BackgroundDerivativePath))
                throw new FileNotFoundException("Step 47.0 private background derivative is absent.", _step47BackgroundDerivativePath);
            _step47ResourceLoadStarted = true;

            var handoff = _callbackHandoff ?? throw new InvalidOperationException("Step 47.0 requires retained GodotSharp bridge authority.");
            var godotAssembly = handoff.GodotSharpAssembly;
            if (!ReferenceEquals(AssemblyLoadContext.GetLoadContext(godotAssembly), context))
                throw new InvalidDataException("Step 47.0 GodotSharp assembly left the exact private load context.");
            var resourceLoader = godotAssembly.GetType("Godot.ResourceLoader", true, false)!;
            var resourceType = godotAssembly.GetType("Godot.Resource", true, false)!;
            var packedSceneType = godotAssembly.GetType("Godot.PackedScene", true, false)!;
            var load = RequireStartupLadderResourceLoadMethod(resourceLoader, resourceType);
            var cacheType = load.GetParameters()[2].ParameterType;
            var cacheIgnore = Enum.Parse(cacheType, "Ignore", ignoreCase: true);
            var cacheReuse = Enum.Parse(cacheType, "Reuse", ignoreCase: true);
            var canInstantiate = packedSceneType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .SingleOrDefault(method => method.Name == "CanInstantiate" && method.GetParameters().Length == 0 && method.ReturnType == typeof(bool))
                ?? throw new MissingMethodException("Godot.PackedScene", "CanInstantiate()");
            var takeOverPath = resourceType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .SingleOrDefault(method => method.Name == "TakeOverPath" && method.GetParameters().Length == 1 && method.GetParameters()[0].ParameterType == typeof(string))
                ?? throw new MissingMethodException("Godot.Resource", "TakeOverPath(string)");

            var initializerBefore = context.InitializerBearingRequests.Count;
            var rejectedBefore = context.RejectedManagedRequests.Count;
            var nativeBefore = context.NativeLoadAttempts.Count;
            Checkpoint(checkpoint, "M47_C_LOAD_BACKGROUND — loading the private Spine-neutral background PackedScene with CacheMode.Ignore, then taking exact res:// background path authority in Godot's resource cache. Original PCK remains unchanged.");
            stage = "Spine-neutral background PackedScene load + TakeOverPath";
            var background = InvokeStartupLadderResourceLoad(load, _step47BackgroundDerivativePath, "PackedScene", cacheIgnore, "Step 47 background derivative");
            if (background is null || !packedSceneType.IsInstanceOfType(background) || canInstantiate.Invoke(background, null) is not true)
                throw new InvalidDataException("Step 47.0 private background did not load as an instantiable Godot.PackedScene.");
            InvokeStartupLadderMethod(takeOverPath, background, new object?[] { MainMenuBackgroundResourcePath }, "Step 47 background TakeOverPath");
            _step47BackgroundPackedScene = background;

            Checkpoint(checkpoint, "M47_C_LOAD_MAIN_MENU — loading original exact res://scenes/screens/main_menu.tscn with CacheMode.Reuse so its background ext_resource resolves to the retained private Spine-neutral cache authority. No scene instantiation yet.");
            stage = "exact main-menu PackedScene load";
            var mainMenu = InvokeStartupLadderResourceLoad(load, MainMenuResourcePath, "PackedScene", cacheReuse, "Step 47 exact main menu");
            if (mainMenu is null || !packedSceneType.IsInstanceOfType(mainMenu) || canInstantiate.Invoke(mainMenu, null) is not true)
                throw new InvalidDataException("Step 47.0 original main_menu.tscn did not load as an instantiable Godot.PackedScene.");
            _step47MainMenuPackedScene = mainMenu;
            RequireNoForbiddenStep37Escape(context, initializerBefore, rejectedBefore, nativeBefore, "Step 47 Gate C");
            _step47PostLoadBaseline = CaptureStartupLadderBaseline(baseline.SelectedPath, baseline.SelectedSha256, context);
            _step47ResourceLoadPassed = true;
            Checkpoint(checkpoint, $"M47_C_PASS — private background cache takeover returned; exact main-menu PackedScene loaded/canInstantiate=True; resolverDelta={context.ManagedResolverRequests.Count - baseline.ResolverCount}; hostDelta={context.HostLoads.Count - baseline.HostCount}; privateDelta={context.PrivateLoads.Count - baseline.PrivateCount}; initializerDelta=0; rejectedDelta=0; nativeDelta=0; sceneInstantiation=NO; originalLaunchMainMenu=NO.");
            return StartupLadderPass(step, Step47Name, gate,
                "Private Spine-neutral background loaded and took exact background resource-path authority; original main_menu.tscn loaded as an instantiable PackedScene through that cache. No scene instantiated, no original LaunchMainMenu/ExecuteDeferred/Steam/native extension executed.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"M47_C_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return StartupLadderFail(step, Step47Name, gate, stage, ex);
        }
    }

    public TransformedRealStS2StartupLadderGateResult RunStep47FrozenResourceConfinement(bool renderingStopped, Action<string>? checkpoint = null)
    {
        const int step = 47;
        const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.PostActionConfinement;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep47Prerequisite("Step 47 Gate D entry");
            if (!_step47ResourceLoadPassed || _step47MainMenuPackedScene is null || _step47BackgroundPackedScene is null)
                throw new InvalidOperationException("Step 47.0 Gate D requires successful exact PackedScene loads.");
            if (!renderingStopped)
                throw new InvalidOperationException("Step 47.0 must leave rendering frozen.");
            var post = _step47PostLoadBaseline ?? throw new InvalidOperationException("Step 47.0 post-load baseline is absent.");
            RequireStartupLadderBaselineUnchanged(context, post, "Step 47 Gate D");
            var state = RequireStep40InsertedAuthority();
            _exactStep47ClosurePassed = true;
            Checkpoint(checkpoint, $"M47_D_PASS — renderingStopped=True; state={state}; exact main-menu/background PackedScenes retained; originalLaunchMainMenu=NO; resolver/host/private/initializer/rejected/native post-load drift=0.");
            return StartupLadderPass(step, Step47Name, gate,
                "Frozen resource confinement passed. Exact main-menu PackedScene and private Spine-neutral background authority are retained, state=2 remains authoritative, and no post-load resolver/native drift occurred.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"M47_D_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return StartupLadderFail(step, Step47Name, gate, stage, ex);
        }
    }

    // STEP 48 — off-tree main-menu instantiation + actual-node lifecycle audit.

    public TransformedRealStS2StartupLadderGateResult RunStep48ClosedStep47Authority(bool renderingStopped, Action<string>? checkpoint = null)
    {
        const int step = 48;
        const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.PrerequisiteAuthority;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep48Prerequisite("Step 48 Gate A entry");
            if (!renderingStopped)
                throw new InvalidOperationException("Step 48.0 requires rendering frozen.");
            var selected = RequireStartupLadderSelectedAuthority(step);
            _step48Baseline = CaptureStartupLadderBaseline(selected.Path, selected.Sha256, context);
            RequireStartupLadderBaselineUnchanged(context, _step48Baseline, "Step 48 Gate A");
            Checkpoint(checkpoint, "M48_A_PASS — Step-47 4/4 exact main-menu PackedScene authority retained; renderingStopped=True; state=2; off-tree instantiation not yet started.");
            return StartupLadderPass(step, Step48Name, gate,
                "Step-47 exact resource authority retained with frozen rendering and no runtime drift. Off-tree main-menu instantiation may now be audited and attempted once.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"M48_A_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return StartupLadderFail(step, Step48Name, gate, stage, ex);
        }
    }

    public TransformedRealStS2StartupLadderGateResult RunStep48ExactInstantiationBinding(Action<string>? checkpoint = null)
    {
        const int step = 48;
        const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.StaticAuditOrBinding;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep48Prerequisite("Step 48 Gate B entry");
            var baseline = _step48Baseline ?? throw new InvalidOperationException("Step 48.0 Gate A must pass before Gate B.");
            var packed = _step47MainMenuPackedScene ?? throw new InvalidOperationException("Step 48.0 retained main-menu PackedScene is absent.");
            var godotAssembly = (_callbackHandoff ?? throw new InvalidOperationException("GodotSharp handoff absent.")).GodotSharpAssembly;
            var packedSceneType = godotAssembly.GetType("Godot.PackedScene", true, false)!;
            var nodeType = godotAssembly.GetType("Godot.Node", true, false)!;
            _ = RequireStartupLadderInstantiateMethod(packedSceneType, nodeType);

            using var resolver = new RejectingAssemblyResolver();
            using var module = OpenStartupLadderModule(baseline.SelectedPath, resolver);
            var allTypes = EnumerateTypes(module.Types).ToDictionary(type => type.FullName, StringComparer.Ordinal);
            var mainMenuType = RequireStartupLadderType(allTypes, MainMenuManagedRootTypeFullName, step);
            var ready = RequireStartupLadderMethod(mainMenuType, "_Ready", 0, false, step);
            var checkCommandLineArgs = RequireStartupLadderMethod(mainMenuType, MainMenuCheckCommandLineArgsMethodName, 0, false, step);
            var readyCommandLineCalls = ready.Body.Instructions.Count(instruction =>
                instruction.Operand is MethodReference method &&
                GetStep41DefinitionTypeName(method.DeclaringType) == MainMenuManagedRootTypeFullName &&
                method.Name == MainMenuCheckCommandLineArgsMethodName && method.Parameters.Count == 0);
            if (readyCommandLineCalls != 1)
                throw new InvalidDataException($"Step 48.1 expected exactly one NMainMenu._Ready -> CheckCommandLineArgs call; observed {readyCommandLineCalls}.");

            var commandLineHelperCalls = checkCommandLineArgs.Body.Instructions
                .Where(instruction => instruction.Operand is MethodReference method &&
                                      GetStep41DefinitionTypeName(method.DeclaringType) == "MegaCrit.Sts2.Core.Helpers.CommandLineHelper")
                .Select(instruction => ((MethodReference)instruction.Operand).FullName)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToArray();
            if (commandLineHelperCalls.Length == 0)
                throw new InvalidDataException("Step 48.1 CheckCommandLineArgs no longer references CommandLineHelper; runtime empty-command-line guard cannot be justified by the physically proven command-line compatibility path.");

            var saveManager = RequireStartupLadderType(allTypes, SaveManagerTypeFullName, step);
            var getInstance = RequireStartupLadderMethod(saveManager, "get_Instance", 0, true, step);
            var getInstanceConstructDefaultCalls = getInstance.Body.Instructions.Count(instruction =>
                instruction.Operand is MethodReference method &&
                GetStep41DefinitionTypeName(method.DeclaringType) == SaveManagerTypeFullName &&
                method.Name == "ConstructDefault" && method.Parameters.Count == 0);
            if (getInstanceConstructDefaultCalls != 1)
                throw new InvalidDataException($"Step 48.1 expected SaveManager.get_Instance to retain exactly one ConstructDefault fallback; observed {getInstanceConstructDefaultCalls}.");

            var nullPlatform = RequireStartupLadderType(allTypes, NullPlatformTypeFullName, step);
            var nullSetRichPresence = RequireStartupLadderMethod(nullPlatform, PlatformSetRichPresenceMethodName, 3, false, step);
            var allMethods = BuildStartupLadderMethodMap(allTypes);
            var nullPresenceAudit = AuditStartupLadderInvocationFrontier(new[] { nullSetRichPresence }, allTypes, allMethods);
            RequireImmediateFrontierAdmissible(nullPresenceAudit, "Step 48.1 exact Null-platform SetRichPresence implementation");
            if (resolver.Requests.Count != 0)
                throw new InvalidDataException("Step 48.1 binding/guard-shape audit attempted external Cecil resolution: " + string.Join(" | ", resolver.Requests));
            RequireStartupLadderBaselineUnchanged(context, baseline, "Step 48 Gate B");
            _step48DirectStaticMap =
                "StS2 Launcher — Step 48.1 guarded off-tree main-menu instantiation static map\n" +
                "Pre-instantiation authority plus exact runtime-guard shape; actual node/lifecycle map is appended after controlled off-tree Instantiate and guard rehearsal.\n" +
                $"Selected compatibility SHA-256: {baseline.SelectedSha256}\n" +
                $"Expected managed root: {MainMenuManagedRootTypeFullName}\n" +
                "PackedScene.Instantiate(GenEditState) exact reflection binding: PRESENT\n" +
                $"NMainMenu._Ready -> CheckCommandLineArgs calls: {readyCommandLineCalls}\n" +
                $"CheckCommandLineArgs CommandLineHelper references: {commandLineHelperCalls.Length} ({string.Join(" | ", commandLineHelperCalls)})\n" +
                $"SaveManager.get_Instance ConstructDefault fallback calls: {getInstanceConstructDefaultCalls}\n" +
                $"NullPlatformUtilStrategy.SetRichPresence immediate closure methods: {nullPresenceAudit.ImmediateClosureMethods.Length}; forbidden boundaries=0; unresolved=0\n" +
                "Guard policy: before lifecycle admission, require empty Godot OS.GetCmdlineArgs, existing production SaveManager _instance with null _mockInstance, exact getter rehearsal returning that same instance, exact Null-platform routing, zero-drift SetRichPresence rehearsal, and zero-drift off-tree CheckCommandLineArgs rehearsal. Only those exact rehearsed methods may terminate static traversal.\n" +
                "Rendering restarted: NO\nOriginal LaunchMainMenu invoked: NO\n";
            Checkpoint(checkpoint, $"M48_B_PASS — exact Instantiate binding + NMainMenu guard shape sealed; _Ready->CheckCommandLineArgs calls={readyCommandLineCalls}; CommandLineHelper refs={commandLineHelperCalls.Length}; SaveManager getter fallback calls={getInstanceConstructDefaultCalls}; Null SetRichPresence closure={nullPresenceAudit.ImmediateClosureMethods.Length}; forbidden/unresolved/external=0; instantiation=NO.");
            return StartupLadderPass(step, Step48Name, gate,
                "Exact PackedScene.Instantiate binding and NMainMenu type authority passed without instantiation. Rendering remains frozen.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"M48_B_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return StartupLadderFail(step, Step48Name, gate, stage, ex);
        }
    }

    public TransformedRealStS2StartupLadderGateResult RunStep48OffTreeInstantiationAndLifecycleAudit(Action<string>? checkpoint = null)
    {
        const int step = 48;
        const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.ControlledAction;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep48Prerequisite("Step 48 Gate C entry");
            var baseline = _step48Baseline ?? throw new InvalidOperationException("Step 48.0 baseline is absent.");
            var packed = _step47MainMenuPackedScene ?? throw new InvalidOperationException("Step 48.0 retained main-menu PackedScene is absent.");
            if (_step48InstantiationStarted)
                throw new InvalidOperationException("Step 48.0 off-tree instantiation is one-shot in-process.");
            _step48InstantiationStarted = true;
            var godotAssembly = (_callbackHandoff ?? throw new InvalidOperationException("GodotSharp handoff absent.")).GodotSharpAssembly;
            var packedSceneType = godotAssembly.GetType("Godot.PackedScene", true, false)!;
            var nodeType = godotAssembly.GetType("Godot.Node", true, false)!;
            var instantiate = RequireStartupLadderInstantiateMethod(packedSceneType, nodeType);
            var disabled = Enum.Parse(instantiate.GetParameters()[0].ParameterType, "Disabled", ignoreCase: true);
            var initializerBefore = context.InitializerBearingRequests.Count;
            var rejectedBefore = context.RejectedManagedRequests.Count;
            var nativeBefore = context.NativeLoadAttempts.Count;

            Checkpoint(checkpoint, "M48_C_INSTANTIATE_START — instantiating exact main-menu PackedScene once off-tree with GenEditState.Disabled. No AddChild, lifecycle-entry, render restart, LaunchMainMenu, deferred startup, or Steam work is authorized.");
            stage = "off-tree main-menu PackedScene.Instantiate";
            var instance = InvokeStartupLadderMethod(instantiate, packed, new object?[] { disabled }, "Step 48 PackedScene.Instantiate")
                ?? throw new InvalidDataException("Step 48.0 PackedScene.Instantiate returned null.");
            if (!nodeType.IsInstanceOfType(instance))
                throw new InvalidDataException("Step 48.0 PackedScene.Instantiate did not return Godot.Node: " + instance.GetType().FullName);
            if (!string.Equals(instance.GetType().FullName, MainMenuManagedRootTypeFullName, StringComparison.Ordinal))
                throw new InvalidDataException($"Step 48.0 expected root {MainMenuManagedRootTypeFullName}; observed {instance.GetType().FullName}.");
            if (!ReferenceEquals(AssemblyLoadContext.GetLoadContext(instance.GetType().Assembly), context))
                throw new InvalidDataException("Step 48.0 NMainMenu instance is not owned by the exact private load context.");
            if (RequireZeroArgBoolMethod(instance.GetType(), "IsInsideTree").Invoke(instance, null) is not false)
                throw new InvalidDataException("Step 48.0 off-tree NMainMenu unexpectedly reports IsInsideTree=true.");
            _step48MainMenuInstance = instance;
            RequireNoForbiddenStep37Escape(context, initializerBefore, rejectedBefore, nativeBefore, "Step 48 off-tree instantiate");
            var postInstantiateBaseline = CaptureStartupLadderBaseline(baseline.SelectedPath, baseline.SelectedSha256, context);

            stage = "runtime lifecycle guard rehearsal";
            _step48LifecycleGuardAuthority = RehearseStep48LifecycleRuntimeGuards(instance, context, postInstantiateBaseline, checkpoint);

            stage = "actual off-tree hierarchy + runtime-guarded immediate lifecycle audit";
            var nodes = EnumerateStep39NodeGraph(instance, nodeType);
            var managedTypes = nodes.Select(item => item.Node.GetType())
                .Where(type => ReferenceEquals(AssemblyLoadContext.GetLoadContext(type.Assembly), context) && ReferenceEquals(type.Assembly, instance.GetType().Assembly))
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
            var lifecycleRoots = CollectStartupLadderActualNodeCallbackRoots(managedTypes, allTypes, new[] { "_EnterTree", "_Ready", "_Notification" });
            var guards = _step48LifecycleGuardAuthority ?? throw new InvalidOperationException("Step 48.1 runtime lifecycle guard authority absent after rehearsal.");
            var audit = AuditStartupLadderInvocationFrontier(lifecycleRoots, allTypes, allMethods, guards);
            RequireImmediateFrontierAdmissible(audit, "Step 48.1 actual main-menu lifecycle frontier after exact runtime guard rehearsal");
            if (resolver.Requests.Count != 0)
                throw new InvalidDataException("Step 48.0 lifecycle audit attempted external Cecil resolution: " + string.Join(" | ", resolver.Requests));
            RequireNoForbiddenStep37Escape(context, initializerBefore, rejectedBefore, nativeBefore, "Step 48 Gate C");
            _step48PostInstantiationBaseline = CaptureStartupLadderBaseline(baseline.SelectedPath, baseline.SelectedSha256, context);
            _step48DirectStaticMap += BuildStep48ActualHierarchyAppendix(nodes, managedTypes, lifecycleRoots, audit);
            _step48InstantiationPassed = true;
            Checkpoint(checkpoint, $"M48_C_PASS — offTreeRoot={instance.GetType().FullName}; nodeCount={nodes.Count}; selectedManagedTypes={managedTypes.Length}; immediateLifecycleRoots={lifecycleRoots.Length}; immediateClosureMethods={audit.ImmediateClosureMethods.Length}; immediateBoundaryRefs={audit.ImmediateBoundaries.Length}; runtimeGuardedFrontiers={audit.GuardedMethodFrontiers.Length}; deferredFrontiers={audit.DeferredMethodFrontiers.Length}; instantiateResolverDelta={postInstantiateBaseline.ResolverCount - baseline.ResolverCount}; instantiateHostDelta={postInstantiateBaseline.HostCount - baseline.HostCount}; instantiatePrivateDelta={postInstantiateBaseline.PrivateCount - baseline.PrivateCount}; unresolved=0; externalResolution=0; nativeDelta=0; insideTree=False.");
            return StartupLadderPass(step, Step48Name, gate,
                $"Real NMainMenu instantiated off-tree once. Runtime guards were rehearsed with zero post-rehearsal drift; actual graph nodes={nodes.Count}; selected managed types={managedTypes.Length}; immediate lifecycle roots={lifecycleRoots.Length}; immediate closure={audit.ImmediateClosureMethods.Length}; guarded frontiers={audit.GuardedMethodFrontiers.Length}; remaining immediate boundaries admissible; deferred delegate/frontier refs recorded separately; no native escape.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"M48_C_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return StartupLadderFail(step, Step48Name, gate, stage, ex);
        }
    }

    public TransformedRealStS2StartupLadderGateResult RunStep48FrozenOffTreeConfinement(bool renderingStopped, Action<string>? checkpoint = null)
    {
        const int step = 48;
        const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.PostActionConfinement;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep48Prerequisite("Step 48 Gate D entry");
            if (!_step48InstantiationPassed || !_step48StaticMapDurablyWritten || _step48MainMenuInstance is null)
                throw new InvalidOperationException("Step 48.0 Gate D requires successful off-tree instantiation plus a durable actual-node/lifecycle map.");
            if (!renderingStopped)
                throw new InvalidOperationException("Step 48.0 must leave rendering frozen.");
            var post = _step48PostInstantiationBaseline ?? throw new InvalidOperationException("Step 48.0 post-instantiation baseline is absent.");
            RequireStartupLadderBaselineUnchanged(context, post, "Step 48 Gate D");
            RequireStep48LifecycleRuntimeGuardsCurrent(context, "Step 48 Gate D", requirePreAdmissionCounts: true);
            if (RequireZeroArgBoolMethod(_step48MainMenuInstance.GetType(), "IsInsideTree").Invoke(_step48MainMenuInstance, null) is not false)
                throw new InvalidDataException("Step 48.0 NMainMenu entered the SceneTree before Step 49 authorization.");
            var state = RequireStep40InsertedAuthority();
            _exactStep48ClosurePassed = true;
            Checkpoint(checkpoint, $"M48_D_PASS — renderingStopped=True; state={state}; NMainMenu retained off-tree; staticMapDurable=True; post-instantiation resolver/host/private/initializer/rejected/native drift=0.");
            return StartupLadderPass(step, Step48Name, gate,
                "Off-tree main-menu confinement passed. Real NMainMenu remains retained outside the SceneTree, renderer frozen, exact actual-node/lifecycle map durable, and post-instantiation context/native state frozen.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"M48_D_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return StartupLadderFail(step, Step48Name, gate, stage, ex);
        }
    }

    // STEP 49 — frozen admission to real NGame.RootSceneContainer.

    public TransformedRealStS2StartupLadderGateResult RunStep49ClosedStep48Authority(bool renderingStopped, Action<string>? checkpoint = null)
    {
        const int step = 49;
        const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.PrerequisiteAuthority;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep49Prerequisite("Step 49 Gate A entry");
            if (!renderingStopped)
                throw new InvalidOperationException("Step 49.0 requires rendering frozen.");
            var selected = RequireStartupLadderSelectedAuthority(step);
            _step49Baseline = CaptureStartupLadderBaseline(selected.Path, selected.Sha256, context);
            RequireStartupLadderBaselineUnchanged(context, _step49Baseline, "Step 49 Gate A");
            RequireStep48LifecycleRuntimeGuardsCurrent(context, "Step 49 Gate A pre-admission guard recheck", requirePreAdmissionCounts: true);
            if (_step48MainMenuInstance is null || RequireZeroArgBoolMethod(_step48MainMenuInstance.GetType(), "IsInsideTree").Invoke(_step48MainMenuInstance, null) is not false)
                throw new InvalidDataException("Step 49.0 requires retained off-tree NMainMenu authority.");
            Checkpoint(checkpoint, "M49_A_PASS — Step-48 4/4 retained; real NMainMenu remains off-tree; renderingStopped=True; state=2; first frozen RootSceneContainer admission not yet armed.");
            return StartupLadderPass(step, Step49Name, gate,
                "Step-48 real off-tree NMainMenu authority retained with frozen rendering and exact context. One frozen RootSceneContainer.AddChild may now be bound/audited.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"M49_A_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return StartupLadderFail(step, Step49Name, gate, stage, ex);
        }
    }

    public TransformedRealStS2StartupLadderGateResult RunStep49RootSceneContainerBinding(Action<string>? checkpoint = null)
    {
        const int step = 49;
        const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.StaticAuditOrBinding;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep49Prerequisite("Step 49 Gate B entry");
            var baseline = _step49Baseline ?? throw new InvalidOperationException("Step 49.0 Gate A must pass before Gate B.");
            var menu = _step48MainMenuInstance ?? throw new InvalidOperationException("Step 49.0 NMainMenu instance absent.");
            var nGame = _step39NGameInstance ?? throw new InvalidOperationException("Step 49.0 retained NGame absent.");
            var rootScene = RequireStartupLadderPropertyValue(nGame, "RootSceneContainer", step);
            var nodeType = (_callbackHandoff ?? throw new InvalidOperationException("GodotSharp handoff absent.")).GodotSharpAssembly.GetType("Godot.Node", true, false)!;
            var addChild = RequireStartupLadderAddChildMethod(rootScene.GetType(), nodeType);
            _step49RootSceneContainer = rootScene;
            _step49ChildrenBefore = GetStartupLadderChildCount(rootScene);
            _step49DirectStaticMap =
                "StS2 Launcher — Step 49.0 frozen main-menu SceneTree admission static map\n" +
                $"Selected compatibility SHA-256: {baseline.SelectedSha256}\n" +
                $"Menu root: {menu.GetType().FullName}\n" +
                $"RootSceneContainer: {rootScene.GetType().FullName}\n" +
                $"Children before AddChild: {_step49ChildrenBefore}\n" +
                $"AddChild binding: {addChild}\n" +
                "Rendering restarted: NO\nOriginal LaunchMainMenu invoked: NO\n";
            RequireStartupLadderBaselineUnchanged(context, baseline, "Step 49 Gate B");
            Checkpoint(checkpoint, $"M49_B_PASS — exact RootSceneContainer resolved; childrenBefore={_step49ChildrenBefore}; AddChild(Node,bool,InternalMode) bound; NMainMenu still off-tree; renderingStopped=True; AddChild=NO.");
            return StartupLadderPass(step, Step49Name, gate,
                $"Exact RootSceneContainer + AddChild binding passed with menu still off-tree. Child count before admission={_step49ChildrenBefore}; rendering remains frozen.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"M49_B_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return StartupLadderFail(step, Step49Name, gate, stage, ex);
        }
    }

    public TransformedRealStS2StartupLadderGateResult RunStep49FrozenMainMenuAdmission(Action<string>? checkpoint = null)
    {
        const int step = 49;
        const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.ControlledAction;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep49Prerequisite("Step 49 Gate C entry");
            var baseline = _step49Baseline ?? throw new InvalidOperationException("Step 49.0 baseline absent.");
            var menu = _step48MainMenuInstance ?? throw new InvalidOperationException("Step 49.0 NMainMenu absent.");
            var rootScene = _step49RootSceneContainer ?? throw new InvalidOperationException("Step 49.0 RootSceneContainer binding absent.");
            if (!_step49StaticMapDurablyWritten)
                throw new InvalidOperationException("Step 49.0 requires its exact admission map durably written before AddChild.");
            if (_step49AdmissionStarted)
                throw new InvalidOperationException("Step 49.0 AddChild boundary is one-shot in-process.");
            if (RequireZeroArgBoolMethod(menu.GetType(), "IsInsideTree").Invoke(menu, null) is not false)
                throw new InvalidDataException("Step 49.0 NMainMenu is already in the SceneTree before authorized AddChild.");
            RequireStep48LifecycleRuntimeGuardsCurrent(context, "Step 49 Gate C immediately before AddChild", requirePreAdmissionCounts: true);
            _step49AdmissionStarted = true;
            var nodeType = (_callbackHandoff ?? throw new InvalidOperationException("GodotSharp handoff absent.")).GodotSharpAssembly.GetType("Godot.Node", true, false)!;
            var addChild = RequireStartupLadderAddChildMethod(rootScene.GetType(), nodeType);
            var internalMode = Enum.Parse(addChild.GetParameters()[2].ParameterType, "Disabled", ignoreCase: true);
            var initializerBefore = context.InitializerBearingRequests.Count;
            var rejectedBefore = context.RejectedManagedRequests.Count;
            var nativeBefore = context.NativeLoadAttempts.Count;
            Checkpoint(checkpoint, $"M49_C_ADDCHILD_START — adding exact audited NMainMenu once to retained NGame.RootSceneContainer while renderer remains frozen; childrenBefore={_step49ChildrenBefore}. Automatic audited _EnterTree/_Ready/_Notification only; no original LaunchMainMenu/deferred/Steam/native extension authorized.");
            stage = "frozen RootSceneContainer.AddChild(NMainMenu)";
            InvokeStartupLadderMethod(addChild, rootScene, new object?[] { menu, false, internalMode }, "Step 49 RootSceneContainer.AddChild");
            if (RequireZeroArgBoolMethod(menu.GetType(), "IsInsideTree").Invoke(menu, null) is not true)
                throw new InvalidDataException("Step 49.0 AddChild returned but NMainMenu reports IsInsideTree=false.");
            var getParent = menu.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(method => method.Name == "GetParent" && method.GetParameters().Length == 0 && !method.IsGenericMethodDefinition && !method.ContainsGenericParameters)
                .FirstOrDefault(method => method.ReturnType.IsAssignableFrom(rootScene.GetType()) || method.ReturnType.IsInstanceOfType(rootScene))
                ?? menu.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .FirstOrDefault(method => method.Name == "GetParent" && method.GetParameters().Length == 0);
            if (getParent is null || !ReferenceEquals(getParent.Invoke(menu, null), rootScene))
                throw new InvalidDataException("Step 49.0 NMainMenu parent is not the exact retained RootSceneContainer after AddChild.");
            _step49ChildrenAfter = GetStartupLadderChildCount(rootScene);
            if (_step49ChildrenAfter != _step49ChildrenBefore + 1)
                throw new InvalidDataException($"Step 49.0 RootSceneContainer child count drifted unexpectedly. before={_step49ChildrenBefore}; after={_step49ChildrenAfter}; expected={_step49ChildrenBefore + 1}.");
            RequireNoForbiddenStep37Escape(context, initializerBefore, rejectedBefore, nativeBefore, "Step 49 Gate C");
            _step49PostAdmissionBaseline = CaptureStartupLadderBaseline(baseline.SelectedPath, baseline.SelectedSha256, context);
            _step49AdmissionPassed = true;
            Checkpoint(checkpoint, $"M49_C_PASS — AddChild returned; NMainMenu insideTree=True; parent=RootSceneContainer; childrenAfter={_step49ChildrenAfter}; state={RequireStep40InsertedAuthority()}; resolverDelta={context.ManagedResolverRequests.Count - baseline.ResolverCount}; hostDelta={context.HostLoads.Count - baseline.HostCount}; privateDelta={context.PrivateLoads.Count - baseline.PrivateCount}; initializerDelta=0; rejectedDelta=0; nativeDelta=0; rendering remains stopped.");
            return StartupLadderPass(step, Step49Name, gate,
                $"Real NMainMenu admitted once into NGame.RootSceneContainer under frozen rendering. insideTree=true; exact parent retained; child count {_step49ChildrenBefore}->{_step49ChildrenAfter}; no initializer/rejected/native escape.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"M49_C_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return StartupLadderFail(step, Step49Name, gate, stage, ex);
        }
    }

    public TransformedRealStS2StartupLadderGateResult RunStep49FrozenAdmissionConfinement(bool renderingStopped, Action<string>? checkpoint = null)
    {
        const int step = 49;
        const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.PostActionConfinement;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep49Prerequisite("Step 49 Gate D entry");
            if (!_step49AdmissionPassed || _step48MainMenuInstance is null || _step49RootSceneContainer is null)
                throw new InvalidOperationException("Step 49.0 Gate D requires successful frozen menu admission.");
            if (!renderingStopped)
                throw new InvalidOperationException("Step 49.0 Gate D requires rendering still frozen.");
            var post = _step49PostAdmissionBaseline ?? throw new InvalidOperationException("Step 49.0 post-admission baseline absent.");
            RequireStartupLadderBaselineUnchanged(context, post, "Step 49 Gate D");
            if (RequireZeroArgBoolMethod(_step48MainMenuInstance.GetType(), "IsInsideTree").Invoke(_step48MainMenuInstance, null) is not true)
                throw new InvalidDataException("Step 49.0 NMainMenu left the SceneTree before Gate D.");
            if (GetStartupLadderChildCount(_step49RootSceneContainer) != _step49ChildrenAfter)
                throw new InvalidDataException("Step 49.0 RootSceneContainer child count changed after admission.");
            var state = RequireStep40InsertedAuthority();
            _exactStep49ClosurePassed = true;
            Checkpoint(checkpoint, $"M49_D_PASS — renderingStopped=True; state={state}; NMainMenu retained inside RootSceneContainer; children={_step49ChildrenAfter}; post-admission resolver/host/private/initializer/rejected/native drift=0.");
            return StartupLadderPass(step, Step49Name, gate,
                "Frozen SceneTree admission confinement passed. Real NMainMenu remains inside exact RootSceneContainer with state=2, rendering stopped, and no post-admission drift.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"M49_D_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return StartupLadderFail(step, Step49Name, gate, stage, ex);
        }
    }

    // STEP 50 — actual in-tree menu frame/input audit + controlled render pulse + refreeze.

    public TransformedRealStS2StartupLadderGateResult RunStep50ClosedStep49Authority(bool renderingStopped, Action<string>? checkpoint = null)
    {
        const int step = 50;
        const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.PrerequisiteAuthority;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep50Prerequisite("Step 50 Gate A entry");
            if (!renderingStopped)
                throw new InvalidOperationException("Step 50.0 requires rendering frozen before frame/input audit.");
            var selected = RequireStartupLadderSelectedAuthority(step);
            _step50Baseline = CaptureStartupLadderBaseline(selected.Path, selected.Sha256, context);
            RequireStartupLadderBaselineUnchanged(context, _step50Baseline, "Step 50 Gate A");
            RequireStep48LifecycleRuntimeGuardsCurrent(context, "Step 50 Gate A guard recheck", requirePreAdmissionCounts: false);
            if (_step48MainMenuInstance is null || RequireZeroArgBoolMethod(_step48MainMenuInstance.GetType(), "IsInsideTree").Invoke(_step48MainMenuInstance, null) is not true)
                throw new InvalidDataException("Step 50.0 requires retained in-tree NMainMenu authority.");
            Checkpoint(checkpoint, "M50_A_PASS — Step-49 4/4 frozen main-menu admission retained; state=2; NMainMenu inside RootSceneContainer; renderer stopped; no render pulse armed yet.");
            return StartupLadderPass(step, Step50Name, gate,
                "Step-49 frozen main-menu admission retained. Real NMainMenu is in-tree under exact RootSceneContainer with renderer stopped and context authority unchanged.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"M50_A_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return StartupLadderFail(step, Step50Name, gate, stage, ex);
        }
    }

    public TransformedRealStS2StartupLadderGateResult RunStep50MenuFrameInputAudit(Action<string>? checkpoint = null)
    {
        const int step = 50;
        const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.StaticAuditOrBinding;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep50Prerequisite("Step 50 Gate B entry");
            var baseline = _step50Baseline ?? throw new InvalidOperationException("Step 50.0 Gate A must pass before Gate B.");
            var menu = _step48MainMenuInstance ?? throw new InvalidOperationException("Step 50.0 NMainMenu absent.");
            var godotAssembly = (_callbackHandoff ?? throw new InvalidOperationException("GodotSharp handoff absent.")).GodotSharpAssembly;
            var nodeType = godotAssembly.GetType("Godot.Node", true, false)!;
            var nodes = EnumerateStep39NodeGraph(menu, nodeType);
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
            var guards = _step48LifecycleGuardAuthority ?? throw new InvalidOperationException("Step 50.0 requires retained Step-48 runtime-guard authority.");
            var audit = AuditStartupLadderInvocationFrontier(roots, allTypes, allMethods, guards);
            RequireImmediateFrontierAdmissible(audit, "Step 50.0 actual in-tree main-menu frame/input frontier under retained runtime guards");
            if (resolver.Requests.Count != 0)
                throw new InvalidDataException("Step 50.0 frame/input audit attempted external Cecil resolution: " + string.Join(" | ", resolver.Requests));
            RequireStartupLadderBaselineUnchanged(context, baseline, "Step 50 Gate B");
            _step50DirectStaticMap = BuildStep50FrameAuditMap(nodes, managedTypes, roots, audit, baseline);
            Checkpoint(checkpoint, $"M50_B_PASS — inTreeMenuNodes={nodes.Count}; selectedManagedTypes={managedTypes.Length}; immediateFrameInputRoots={roots.Length}; immediateClosureMethods={audit.ImmediateClosureMethods.Length}; immediateBoundaryRefs={audit.ImmediateBoundaries.Length}; deferredFrontiers={audit.DeferredMethodFrontiers.Length}; forbiddenImmediateRefs=0; unresolved=0; externalResolution=0; rendering remains stopped.");
            return StartupLadderPass(step, Step50Name, gate,
                $"Actual in-tree main-menu frame/input surface mapped with execution-qualified traversal: nodes={nodes.Count}; managed types={managedTypes.Length}; immediate callbacks={roots.Length}; closure={audit.ImmediateClosureMethods.Length}; all immediate boundaries admissible; deferred frontiers recorded separately. Render pulse not yet started.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"M50_B_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return StartupLadderFail(step, Step50Name, gate, stage, ex);
        }
    }

    public void BeginStep50BoundedRenderPulse(Action<string>? checkpoint = null)
    {
        ThrowIfDisposed();
        RequireStep50Prerequisite("Step 50 Gate C pulse start");
        if (!_step50StaticMapDurablyWritten || string.IsNullOrWhiteSpace(_step50DirectStaticMap))
            throw new InvalidOperationException("Step 50.0 requires the exact frame/input static map durably written before rendering starts.");
        if (_step50PulseStarted)
            throw new InvalidOperationException("Step 50.0 render pulse is one-shot in-process.");
        _step50PulseStarted = true;
        Checkpoint(checkpoint, $"M50_C_PULSE_ARMED — first/only direct-main-menu render pulse authorized; requested stop delay={Step40RenderPulseTargetMilliseconds}ms; evidence ceiling={Step40RenderPulseMaximumMilliseconds}ms. First managed continuation must StopRendering before telemetry.");
    }

    public TransformedRealStS2StartupLadderGateResult RunStep50BoundedRenderPulseEvidence(
        bool startReturned,
        bool renderingActiveAfterStart,
        bool stopReturned,
        bool renderingActiveAfterStop,
        double elapsedMilliseconds,
        Action<string>? checkpoint = null)
    {
        const int step = 50;
        const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.ControlledAction;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep50Prerequisite("Step 50 Gate C evidence");
            var baseline = _step50Baseline ?? throw new InvalidOperationException("Step 50.0 baseline absent.");
            if (!_step50PulseStarted)
                throw new InvalidOperationException("Step 50.0 render pulse evidence arrived before one-shot arm.");
            stage = "bounded direct-main-menu render pulse + synchronous refreeze";
            if (!startReturned || !renderingActiveAfterStart)
                throw new InvalidOperationException($"Step 50.0 StartRendering failed. returned={startReturned}; activeAfterStart={renderingActiveAfterStart}.");
            if (!stopReturned || renderingActiveAfterStop)
                throw new InvalidOperationException($"Step 50.0 StopRendering failed to refreeze synchronously. returned={stopReturned}; activeAfterStop={renderingActiveAfterStop}.");
            if (elapsedMilliseconds < Step40RenderPulseTargetMilliseconds || elapsedMilliseconds > Step40RenderPulseMaximumMilliseconds)
                throw new InvalidOperationException($"Step 50.0 post-stop evidence outside accepted window. target={Step40RenderPulseTargetMilliseconds}; ceiling={Step40RenderPulseMaximumMilliseconds}; observed={elapsedMilliseconds:F1}.");
            if (_step48MainMenuInstance is null || RequireZeroArgBoolMethod(_step48MainMenuInstance.GetType(), "IsInsideTree").Invoke(_step48MainMenuInstance, null) is not true)
                throw new InvalidDataException("Step 50.0 NMainMenu left the SceneTree during render pulse.");
            if (context.InitializerBearingRequests.Count != baseline.InitializerCount || context.RejectedManagedRequests.Count != baseline.RejectedCount || context.NativeLoadAttempts.Count != baseline.NativeCount)
                throw new InvalidDataException($"Step 50.0 render pulse crossed forbidden initializer/rejected/native boundary. initializer={context.InitializerBearingRequests.Count - baseline.InitializerCount}; rejected={context.RejectedManagedRequests.Count - baseline.RejectedCount}; native={context.NativeLoadAttempts.Count - baseline.NativeCount}.");
            _step50PostPulseBaseline = CaptureStartupLadderBaseline(baseline.SelectedPath, baseline.SelectedSha256, context);
            _step50PulsePassed = true;
            Checkpoint(checkpoint, $"M50_C_PASS — StartRendering active=True; first stop opportunity={elapsedMilliseconds:F1}ms; StopRendering returned=True/activeAfterStop=False; NMainMenu retained; state={RequireStep40InsertedAuthority()}; resolverDelta={context.ManagedResolverRequests.Count - baseline.ResolverCount}; hostDelta={context.HostLoads.Count - baseline.HostCount}; privateDelta={context.PrivateLoads.Count - baseline.PrivateCount}; initializerDelta=0; rejectedDelta=0; nativeDelta=0.");
            return StartupLadderPass(step, Step50Name, gate,
                $"Controlled direct-main-menu render pulse passed and refroze synchronously at {elapsedMilliseconds:F1} ms. NMainMenu remained in-tree, state=2, and initializer/rejected/native deltas stayed zero.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"M50_C_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return StartupLadderFail(step, Step50Name, gate, stage, ex);
        }
    }

    public TransformedRealStS2StartupLadderGateResult RunStep50FrozenPostPulseConfinement(bool renderingStopped, Action<string>? checkpoint = null)
    {
        const int step = 50;
        const TransformedRealStS2StartupLadderGate gate = TransformedRealStS2StartupLadderGate.PostActionConfinement;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep50Prerequisite("Step 50 Gate D entry");
            if (!_step50PulseStarted || !_step50PulsePassed)
                throw new InvalidOperationException("Step 50.0 Gate D requires a successful first render pulse and synchronous refreeze.");
            if (!renderingStopped)
                throw new InvalidOperationException("Step 50.0 Gate D requires rendering stopped.");
            var post = _step50PostPulseBaseline ?? throw new InvalidOperationException("Step 50.0 post-pulse baseline absent.");
            RequireStartupLadderBaselineUnchanged(context, post, "Step 50 Gate D");
            if (_step48MainMenuInstance is null || RequireZeroArgBoolMethod(_step48MainMenuInstance.GetType(), "IsInsideTree").Invoke(_step48MainMenuInstance, null) is not true)
                throw new InvalidDataException("Step 50.0 NMainMenu left the SceneTree after pulse.");
            var state = RequireStep40InsertedAuthority();
            _exactStep50ClosurePassed = true;
            Checkpoint(checkpoint, $"M50_D_PASS — renderingStopped=True; state={state}; real NMainMenu retained in exact RootSceneContainer; post-pulse resolver/host/private/initializer/rejected/native drift=0; originalLaunchMainMenu=NO.");
            return StartupLadderPass(step, Step50Name, gate,
                "Direct-main-menu controlled render pulse closed 4/4. Rendering is frozen again; real NMainMenu remains admitted under RootSceneContainer; state/context/native confinement remains exact; original LaunchMainMenu was never invoked.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"M50_D_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return StartupLadderFail(step, Step50Name, gate, stage, ex);
        }
    }

    // Direct-main-menu shared helpers.

    private StartupLadderRuntimeGuardAuthority RehearseStep48LifecycleRuntimeGuards(
        object menu,
        Step35ExecutionLoadContext context,
        StartupLadderBaseline baseline,
        Action<string>? checkpoint)
    {
        RequireStartupLadderBaselineUnchanged(context, baseline, "Step 48 runtime-guard rehearsal entry");
        var godotAssembly = (_callbackHandoff ?? throw new InvalidOperationException("GodotSharp handoff absent.")).GodotSharpAssembly;
        var osType = godotAssembly.GetType("Godot.OS", throwOnError: true, ignoreCase: false)
            ?? throw new MissingMemberException("Godot.OS");
        var getCmdlineArgs = osType.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            .SingleOrDefault(method => method.Name == "GetCmdlineArgs" && method.GetParameters().Length == 0)
            ?? throw new MissingMethodException("Godot.OS", "GetCmdlineArgs()");
        var commandLineArgs = NormalizeStartupLadderCommandLineArgs(
            InvokeStartupLadderMethod(getCmdlineArgs, null, null, "Step 48 Godot.OS.GetCmdlineArgs"));
        if (commandLineArgs.Length != 0)
            throw new InvalidDataException("Step 48.1 refuses lifecycle admission unless Godot.OS.GetCmdlineArgs is empty. observed=" + string.Join(" | ", commandLineArgs.Select(SanitizeCheckpoint)));

        var admission = RequireAdmission();
        var saveManagerType = admission.Assembly.GetType(SaveManagerTypeFullName, throwOnError: true, ignoreCase: false)
            ?? throw new MissingMemberException(SaveManagerTypeFullName);
        var saveManager = RequireExistingStartupLadderSaveManagerInstance(saveManagerType, 48);
        var getInstance = saveManagerType.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            .SingleOrDefault(method => method.Name == "get_Instance" && method.GetParameters().Length == 0 && method.ReturnType == saveManagerType)
            ?? throw new MissingMethodException(SaveManagerTypeFullName, "get_Instance()");
        var getterResult = InvokeStartupLadderMethod(getInstance, null, null, "Step 48 SaveManager.get_Instance rehearsal")
            ?? throw new InvalidDataException("Step 48.1 SaveManager.get_Instance rehearsal returned null.");
        if (!ReferenceEquals(getterResult, saveManager))
            throw new InvalidDataException("Step 48.1 SaveManager.get_Instance rehearsal did not return the exact already-created production _instance.");

        var platformUtilType = admission.Assembly.GetType(PlatformUtilTypeFullName, throwOnError: true, ignoreCase: false)
            ?? throw new MissingMemberException(PlatformUtilTypeFullName);
        var nullPlatformType = admission.Assembly.GetType(NullPlatformTypeFullName, throwOnError: true, ignoreCase: false)
            ?? throw new MissingMemberException(NullPlatformTypeFullName);
        var primaryProperty = platformUtilType.GetProperty("PrimaryPlatform", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            ?? throw new MissingMemberException(PlatformUtilTypeFullName, "PrimaryPlatform");
        var primaryPlatform = primaryProperty.GetValue(null)
            ?? throw new InvalidDataException("Step 48.1 PlatformUtil.PrimaryPlatform returned null.");
        var getPlatformUtil = platformUtilType.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            .SingleOrDefault(method => method.Name == "GetPlatformUtil" && method.GetParameters().Length == 1 && method.GetParameters()[0].ParameterType == primaryPlatform.GetType())
            ?? throw new MissingMethodException(PlatformUtilTypeFullName, "GetPlatformUtil(PlatformType)");
        var strategy = InvokeStartupLadderMethod(getPlatformUtil, null, new[] { primaryPlatform }, "Step 48 PlatformUtil.GetPlatformUtil rehearsal")
            ?? throw new InvalidDataException("Step 48.1 PlatformUtil.GetPlatformUtil returned null.");
        if (strategy.GetType() != nullPlatformType)
            throw new InvalidDataException($"Step 48.1 requires exact NullPlatformUtilStrategy before NMainMenu lifecycle admission; observed {strategy.GetType().FullName} for PrimaryPlatform={primaryPlatform}.");

        var setRichPresence = platformUtilType.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            .SingleOrDefault(method =>
            {
                if (method.Name != PlatformSetRichPresenceMethodName || method.ReturnType != typeof(void))
                    return false;
                var p = method.GetParameters();
                return p.Length == 3 && p[0].ParameterType == typeof(string) && p[1].ParameterType == typeof(string) && p[2].ParameterType == typeof(int?);
            }) ?? throw new MissingMethodException(PlatformUtilTypeFullName, "SetRichPresence(string,string,int?)");
        InvokeStartupLadderMethod(setRichPresence, null, new object?[] { string.Empty, string.Empty, null }, "Step 48 Null-platform SetRichPresence rehearsal");

        var nGame = _step39NGameInstance ?? throw new InvalidOperationException("Step 48.1 retained NGame absent during lifecycle-guard rehearsal.");
        var rootScene = RequireStartupLadderPropertyValue(nGame, "RootSceneContainer", 48);
        var rootChildrenBefore = GetStartupLadderChildCount(rootScene);
        var menuChildrenBefore = GetStartupLadderChildCount(menu);
        var checkCommandLineArgs = menu.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .SingleOrDefault(method => method.Name == MainMenuCheckCommandLineArgsMethodName && method.GetParameters().Length == 0 && method.ReturnType == typeof(void))
            ?? throw new MissingMethodException(MainMenuManagedRootTypeFullName, MainMenuCheckCommandLineArgsMethodName + "()");
        InvokeStartupLadderMethod(checkCommandLineArgs, menu, null, "Step 48 off-tree CheckCommandLineArgs rehearsal");
        if (RequireZeroArgBoolMethod(menu.GetType(), "IsInsideTree").Invoke(menu, null) is not false)
            throw new InvalidDataException("Step 48.1 CheckCommandLineArgs rehearsal unexpectedly admitted NMainMenu into the SceneTree.");
        if (GetStartupLadderChildCount(rootScene) != rootChildrenBefore)
            throw new InvalidDataException("Step 48.1 CheckCommandLineArgs rehearsal changed NGame.RootSceneContainer child count.");
        if (GetStartupLadderChildCount(menu) != menuChildrenBefore)
            throw new InvalidDataException("Step 48.1 CheckCommandLineArgs rehearsal changed NMainMenu child count despite an empty Godot command line.");
        if (!ReferenceEquals(RequireExistingStartupLadderSaveManagerInstance(saveManagerType, 48), saveManager))
            throw new InvalidDataException("Step 48.1 runtime-guard rehearsal changed production SaveManager singleton identity.");
        _ = RequireStep40InsertedAuthority();
        RequireStartupLadderBaselineUnchanged(context, baseline, "Step 48 runtime-guard rehearsal exit");
        var post = CaptureStartupLadderBaseline(baseline.SelectedPath, baseline.SelectedSha256, context);
        Checkpoint(checkpoint,
            $"M48_C_GUARDS_PASS — GodotCmdlineArgs=0; SaveManager getter returned exact production _instance; PrimaryPlatform={primaryPlatform}; strategy={strategy.GetType().FullName}; SetRichPresence rehearsal returned; CheckCommandLineArgs rehearsal returned off-tree; rootChildren={rootChildrenBefore}; menuChildren={menuChildrenBefore}; resolver/host/private/initializer/rejected/native drift=0.");
        return new StartupLadderRuntimeGuardAuthority(commandLineArgs, saveManager, strategy, primaryPlatform.ToString() ?? string.Empty,
            rootChildrenBefore, menuChildrenBefore, post, true);
    }

    private void RequireStep48LifecycleRuntimeGuardsCurrent(
        Step35ExecutionLoadContext context,
        string boundary,
        bool requirePreAdmissionCounts)
    {
        var guards = _step48LifecycleGuardAuthority
            ?? throw new InvalidOperationException(boundary + " requires retained Step-48 runtime-guard rehearsal authority.");
        if (!guards.RehearsalPassed)
            throw new InvalidOperationException(boundary + " runtime-guard rehearsal is not marked passed.");
        if (requirePreAdmissionCounts)
            RequireStartupLadderBaselineUnchanged(context, guards.PostRehearsalBaseline, boundary + " context");

        var godotAssembly = (_callbackHandoff ?? throw new InvalidOperationException("GodotSharp handoff absent.")).GodotSharpAssembly;
        var osType = godotAssembly.GetType("Godot.OS", throwOnError: true, ignoreCase: false)
            ?? throw new MissingMemberException("Godot.OS");
        var getCmdlineArgs = osType.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            .SingleOrDefault(method => method.Name == "GetCmdlineArgs" && method.GetParameters().Length == 0)
            ?? throw new MissingMethodException("Godot.OS", "GetCmdlineArgs()");
        var args = NormalizeStartupLadderCommandLineArgs(InvokeStartupLadderMethod(getCmdlineArgs, null, null, boundary + " GetCmdlineArgs"));
        if (args.Length != 0 || guards.GodotCommandLineArgs.Length != 0)
            throw new InvalidDataException(boundary + " requires the rehearsed empty Godot command-line state to remain unchanged.");

        var admission = RequireAdmission();
        var saveManagerType = admission.Assembly.GetType(SaveManagerTypeFullName, true, false)!;
        if (!ReferenceEquals(RequireExistingStartupLadderSaveManagerInstance(saveManagerType, 48), guards.SaveManagerInstance))
            throw new InvalidDataException(boundary + " production SaveManager singleton identity drifted after Step-48 rehearsal.");
        var platformUtilType = admission.Assembly.GetType(PlatformUtilTypeFullName, true, false)!;
        var primaryProperty = platformUtilType.GetProperty("PrimaryPlatform", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            ?? throw new MissingMemberException(PlatformUtilTypeFullName, "PrimaryPlatform");
        var primaryPlatform = primaryProperty.GetValue(null) ?? throw new InvalidDataException(boundary + " PrimaryPlatform returned null.");
        var getPlatformUtil = platformUtilType.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            .Single(method => method.Name == "GetPlatformUtil" && method.GetParameters().Length == 1 && method.GetParameters()[0].ParameterType == primaryPlatform.GetType());
        var strategy = InvokeStartupLadderMethod(getPlatformUtil, null, new[] { primaryPlatform }, boundary + " GetPlatformUtil")
            ?? throw new InvalidDataException(boundary + " GetPlatformUtil returned null.");
        if (!ReferenceEquals(strategy, guards.NullPlatformStrategy) || strategy.GetType().FullName != NullPlatformTypeFullName ||
            !string.Equals(primaryPlatform.ToString(), guards.PrimaryPlatform, StringComparison.Ordinal))
            throw new InvalidDataException(boundary + " Null-platform authority drifted after Step-48 rehearsal.");

        if (requirePreAdmissionCounts)
        {
            var menu = _step48MainMenuInstance ?? throw new InvalidOperationException(boundary + " NMainMenu instance absent.");
            var nGame = _step39NGameInstance ?? throw new InvalidOperationException(boundary + " NGame instance absent.");
            var rootScene = RequireStartupLadderPropertyValue(nGame, "RootSceneContainer", 48);
            if (GetStartupLadderChildCount(rootScene) != guards.RootSceneChildren || GetStartupLadderChildCount(menu) != guards.MenuChildren)
                throw new InvalidDataException(boundary + " tree shape drifted after Step-48 guard rehearsal and before lifecycle admission.");
        }
    }

    private static string[] NormalizeStartupLadderCommandLineArgs(object? raw)
    {
        if (raw is null)
            return [];
        if (raw is string[] array)
            return array;
        if (raw is IEnumerable enumerable)
            return enumerable.Cast<object?>().Select(item => item?.ToString() ?? string.Empty).ToArray();
        throw new InvalidDataException("Godot.OS.GetCmdlineArgs returned unsupported runtime type: " + raw.GetType().FullName);
    }

    private Step35ExecutionLoadContext RequireStep48Prerequisite(string boundary)
    {
        var context = RequireStep47Prerequisite(boundary);
        if (!_exactStep47ClosurePassed || !_step47ResourceLoadPassed)
            throw new InvalidOperationException(boundary + " requires same-process Step-47 4/4 exact main-menu resource authority.");
        return context;
    }

    private Step35ExecutionLoadContext RequireStep49Prerequisite(string boundary)
    {
        var context = RequireStep48Prerequisite(boundary);
        if (!_exactStep48ClosurePassed || !_step48InstantiationPassed || !_step48StaticMapDurablyWritten)
            throw new InvalidOperationException(boundary + " requires same-process Step-48 4/4 retained off-tree NMainMenu authority.");
        return context;
    }

    private Step35ExecutionLoadContext RequireStep50Prerequisite(string boundary)
    {
        var context = RequireStep49Prerequisite(boundary);
        if (!_exactStep49ClosurePassed || !_step49AdmissionPassed || !_step49StaticMapDurablyWritten)
            throw new InvalidOperationException(boundary + " requires same-process Step-49 4/4 frozen in-tree NMainMenu authority.");
        return context;
    }

    private static ExtractedPckEntry ExtractExactStartupLadderPckEntry(string pckPath, string resourcePath, int expectedBytes, string expectedSha256, int step)
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
        stream.Seek(checked((long)directoryOffset), SeekOrigin.Begin);
        var count = reader.ReadUInt32();
        if (count != ClosedPckDirectoryEntries)
            throw new InvalidDataException($"Step {step}.0 PCK directory count drifted: {count}.");
        PckDirectoryEntry? match = null;
        for (var i = 0u; i < count; i++)
        {
            var pathLength = reader.ReadUInt32();
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
        if (entry.Size != (ulong)expectedBytes)
            throw new InvalidDataException($"Step {step}.0 PCK resource size drifted for {resourcePath}: expected={expectedBytes}; actual={entry.Size}.");
        var absolute = checked(fileBase + entry.Offset);
        if (absolute + entry.Size > (ulong)stream.Length)
            throw new InvalidDataException($"Step {step}.0 PCK resource {resourcePath} points beyond pack length.");
        stream.Seek(checked((long)absolute), SeekOrigin.Begin);
        var bytes = ReadExactlyStep37(reader, expectedBytes);
        var pckMd5 = Convert.ToHexString(entry.Md5).ToLowerInvariant();
        var actualMd5 = Convert.ToHexString(MD5.HashData(bytes)).ToLowerInvariant();
        if (!pckMd5.Equals(actualMd5, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException($"Step {step}.0 PCK MD5 mismatch for {resourcePath}: directory={pckMd5}; actual={actualMd5}.");
        var sha = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
        if (!sha.Equals(expectedSha256, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException($"Step {step}.0 SHA-256 mismatch for {resourcePath}: expected={expectedSha256}; actual={sha}.");
        return new ExtractedPckEntry(bytes, entry.Offset, entry.Size, pckMd5, entry.Flags, directoryOffset, fileBase, count);
    }

    private static string BuildStep47SpineNeutralBackground(string sourceText)
    {
        var normalized = sourceText.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');
        var lines = normalized.Split('\n').ToList();
        if (lines.Count != 0 && lines[^1].Length == 0)
            lines.RemoveAt(lines.Count - 1);
        var output = new List<string>(lines.Count);
        var removedSpineResources = 0;
        var removedAutoPlayerResources = 0;
        var removedSkeletonProperties = 0;
        var removedPreviewProperties = 0;
        var removedAutoPlayerNodes = 0;
        var replacedSpineNodes = 0;
        for (var i = 0; i < lines.Count; i++)
        {
            var line = lines[i];
            if (line == "[gd_scene load_steps=26 format=3 uid=\"uid://b7h703c8cnsc1\"]")
            {
                output.Add("[gd_scene load_steps=22 format=3 uid=\"uid://b7h703c8cnsc1\"]");
                continue;
            }
            if (line.StartsWith("[ext_resource type=\"SpineSkeletonDataResource\" ", StringComparison.Ordinal))
            {
                removedSpineResources++;
                continue;
            }
            if (line == "[ext_resource type=\"Script\" uid=\"uid://b0oho7pjc1dtg\" path=\"res://src/Core/Nodes/Animation/NSpineAutoPlayer.cs\" id=\"3_yvrjj\"]")
            {
                removedAutoPlayerResources++;
                continue;
            }
            if (line.StartsWith("skeleton_data_res = ", StringComparison.Ordinal))
            {
                removedSkeletonProperties++;
                continue;
            }
            if (line is "preview_skin = \"Default\"" or "preview_animation = \"-- Empty --\"" or "preview_frame = false" or "preview_time = 0.0")
            {
                removedPreviewProperties++;
                continue;
            }
            if (line.StartsWith("[node name=\"NSpineAutoPlayer\" type=\"Node\" parent=", StringComparison.Ordinal))
            {
                if (i + 2 >= lines.Count || lines[i + 1] != "script = ExtResource(\"3_yvrjj\")" || lines[i + 2] != "metadata/_custom_type_script = \"uid://b0oho7pjc1dtg\"")
                    throw new InvalidDataException("Step 47.0 NSpineAutoPlayer node block shape drifted.");
                i += 2;
                if (i + 1 < lines.Count && lines[i + 1].Length == 0)
                    i += 1;
                removedAutoPlayerNodes++;
                continue;
            }
            if (line == "[node name=\"Bg\" type=\"SpineSprite\" parent=\"BgContainer\"]")
            {
                output.Add("[node name=\"Bg\" type=\"Node2D\" parent=\"BgContainer\"]");
                replacedSpineNodes++;
                continue;
            }
            if (line == "[node name=\"Fg\" type=\"SpineSprite\" parent=\"BgContainer\"]")
            {
                output.Add("[node name=\"Fg\" type=\"Node2D\" parent=\"BgContainer\"]");
                replacedSpineNodes++;
                continue;
            }
            if (line == "[node name=\"Logo\" type=\"SpineSprite\" parent=\"BgContainer/Control\"]")
            {
                output.Add("[node name=\"Logo\" type=\"Node2D\" parent=\"BgContainer/Control\"]");
                replacedSpineNodes++;
                continue;
            }
            output.Add(line);
        }
        if (removedSpineResources != 3 || removedAutoPlayerResources != 1 || removedSkeletonProperties != 3 || removedPreviewProperties != 12 || removedAutoPlayerNodes != 3 || replacedSpineNodes != 3)
            throw new InvalidDataException($"Step 47.0 Spine-neutral rewrite counts drifted: spineResources={removedSpineResources}; autoPlayerResources={removedAutoPlayerResources}; skeletonProps={removedSkeletonProperties}; previewProps={removedPreviewProperties}; autoPlayerNodes={removedAutoPlayerNodes}; replacedSpineNodes={replacedSpineNodes}.");
        return string.Join("\n", output) + "\n";
    }

    private static MethodInfo RequireStartupLadderResourceLoadMethod(Type resourceLoader, Type resourceType)
        => resourceLoader.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            .SingleOrDefault(candidate =>
            {
                if (candidate.Name != "Load" || !resourceType.IsAssignableFrom(candidate.ReturnType))
                    return false;
                var p = candidate.GetParameters();
                return p.Length == 3 && p[0].ParameterType == typeof(string) && p[1].ParameterType == typeof(string) && p[2].ParameterType.IsEnum;
            }) ?? throw new MissingMethodException("Godot.ResourceLoader", "Load(string,string,CacheMode)");

    private static object? InvokeStartupLadderResourceLoad(MethodInfo load, string path, string typeHint, object cacheMode, string boundary)
        => InvokeStartupLadderMethod(load, null, new[] { (object)path, typeHint, cacheMode }, boundary);

    private static MethodInfo RequireStartupLadderInstantiateMethod(Type packedSceneType, Type nodeType)
        => packedSceneType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .SingleOrDefault(method =>
            {
                if (method.Name != "Instantiate" || !nodeType.IsAssignableFrom(method.ReturnType))
                    return false;
                var p = method.GetParameters();
                return p.Length == 1 && p[0].ParameterType.IsEnum;
            }) ?? throw new MissingMethodException("Godot.PackedScene", "Instantiate(GenEditState)");

    private static MethodInfo RequireStartupLadderAddChildMethod(Type parentType, Type nodeType)
        => parentType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .SingleOrDefault(method =>
            {
                if (method.Name != "AddChild")
                    return false;
                var p = method.GetParameters();
                return p.Length == 3 && nodeType.IsAssignableFrom(p[0].ParameterType) && p[1].ParameterType == typeof(bool) && p[2].ParameterType.IsEnum;
            }) ?? throw new MissingMethodException(parentType.FullName, "AddChild(Node,bool,InternalMode)");

    private static int GetStartupLadderChildCount(object parent)
    {
        var candidates = parent.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(method => method.Name == "GetChildCount" && method.ReturnType == typeof(int))
            .ToArray();
        var zero = candidates.SingleOrDefault(method => method.GetParameters().Length == 0);
        if (zero is not null)
            return (int)(zero.Invoke(parent, null) ?? throw new InvalidDataException("GetChildCount() returned null."));
        var one = candidates.SingleOrDefault(method => method.GetParameters().Length == 1 && method.GetParameters()[0].ParameterType == typeof(bool))
            ?? throw new MissingMethodException(parent.GetType().FullName, "GetChildCount([bool])");
        return (int)(one.Invoke(parent, new object?[] { false }) ?? throw new InvalidDataException("GetChildCount(false) returned null."));
    }

    private static MethodDefinition[] CollectStartupLadderActualNodeCallbackRoots(
        IEnumerable<string> managedTypeNames,
        IReadOnlyDictionary<string, TypeDefinition> allTypes,
        IEnumerable<string> callbackNames)
    {
        var names = new HashSet<string>(callbackNames, StringComparer.Ordinal);
        var roots = new SortedDictionary<uint, MethodDefinition>();
        foreach (var managedName in managedTypeNames)
        {
            if (!allTypes.TryGetValue(managedName, out var type))
                throw new InvalidDataException("Actual managed node type is absent from selected sts2 module: " + managedName);
            TypeDefinition? current = type;
            var baseVisited = new HashSet<string>(StringComparer.Ordinal);
            while (current is not null && baseVisited.Add(current.FullName))
            {
                foreach (var method in current.Methods.Where(method => names.Contains(method.Name) && method.HasBody))
                    roots.TryAdd(method.MetadataToken.ToUInt32(), method);
                var baseName = current.BaseType is null ? null : GetStep41DefinitionTypeName(current.BaseType);
                current = baseName is not null && allTypes.TryGetValue(baseName, out var baseType) ? baseType : null;
            }
        }
        return roots.Values.ToArray();
    }

    private static void RequireImmediateFrontierAdmissible(StartupLadderInvocationFrontierAudit audit, string boundary)
    {
        if (audit.UnresolvedSameAssemblyReferences.Length != 0)
            throw new InvalidDataException(boundary + " unresolved same-sts2 refs: " + string.Join(" | ", audit.UnresolvedSameAssemblyReferences));
        var forbidden = audit.ImmediateBoundaries.Where(item => !IsStartupLadderBoundaryAllowed(item)).ToArray();
        if (forbidden.Length != 0)
            throw new InvalidDataException(boundary + " reaches forbidden immediate boundaries: " + string.Join(" | ", forbidden.Select(item => item.Category + ": " + item.Path)));
    }

    private static string BuildStep47DirectResourceMap(ExtractedPckEntry mainMenu, ExtractedPckEntry background, string derivativeSha, string derivativeText)
        =>
            "StS2 Launcher — Step 47.0 exact main-menu resource preparation static map\n" +
            "Read-only source authority from exact receipt-backed PCK; private derivative is launcher-owned and never replaces trusted install bytes.\n" +
            $"Main menu resource: {MainMenuResourcePath}\n" +
            $"Main menu bytes: {mainMenu.Bytes.Length}\n" +
            $"Main menu SHA-256: {ClosedMainMenuSha256}\n" +
            $"Background resource: {MainMenuBackgroundResourcePath}\n" +
            $"Background bytes: {background.Bytes.Length}\n" +
            $"Background SHA-256: {ClosedMainMenuBackgroundSha256}\n" +
            $"Spine-neutral derivative bytes: {Step47SpineNeutralBackgroundBytes}\n" +
            $"Spine-neutral derivative SHA-256: {derivativeSha}\n" +
            "Authorized derivative edits: load_steps 26->22; remove 3 SpineSkeletonDataResource ext resources; remove NSpineAutoPlayer script ext resource; SpineSprite->Node2D for Bg/Fg/Logo; remove skeleton/preview properties; remove 3 NSpineAutoPlayer nodes.\n" +
            $"Derivative contains Spine token: {derivativeText.Contains("Spine", StringComparison.Ordinal)}\n" +
            "Original LaunchMainMenu invoked: NO\nExecuteDeferred invoked: NO\nSteam startup invoked: NO\nTrusted PCK/install mutation: NO\n";

    private static string BuildStep48ActualHierarchyAppendix(
        IReadOnlyList<Step39NodeObservation> nodes,
        IReadOnlyList<string> managedTypes,
        IReadOnlyList<MethodDefinition> lifecycleRoots,
        StartupLadderInvocationFrontierAudit audit)
    {
        var lines = new List<string>
        {
            string.Empty,
            "[ACTUAL OFF-TREE MAIN-MENU HIERARCHY]",
            $"Nodes: {nodes.Count}",
            $"Selected managed node types: {managedTypes.Count}",
            $"Immediate lifecycle callback roots: {lifecycleRoots.Count}",
            $"Invocation-qualified lifecycle closure methods: {audit.ImmediateClosureMethods.Length}",
            $"Immediate classified boundaries: {audit.ImmediateBoundaries.Length}",
            $"Deferred method frontiers: {audit.DeferredMethodFrontiers.Length}",
            $"Runtime-guarded immediate frontiers: {audit.GuardedMethodFrontiers.Length}",
            "Forbidden immediate boundaries: 0",
            "Unresolved same-sts2 references: 0",
        };
        foreach (var node in nodes)
            lines.Add($"  node: {node.Path} | {node.Node.GetType().FullName}");
        lines.Add("[SELECTED MANAGED TYPES]");
        foreach (var type in managedTypes)
            lines.Add("  - " + type);
        lines.Add("[LIFECYCLE ROOTS]");
        foreach (var root in lifecycleRoots)
            lines.Add($"  - token=0x{root.MetadataToken.ToUInt32():X8}; {root.FullName}");
        lines.Add(BuildStartupLadderInvocationFrontierAppendix(audit));
        return string.Join("\n", lines) + "\n";
    }

    private static string BuildStep50FrameAuditMap(
        IReadOnlyList<Step39NodeObservation> nodes,
        IReadOnlyList<string> managedTypes,
        IReadOnlyList<MethodDefinition> roots,
        StartupLadderInvocationFrontierAudit audit,
        StartupLadderBaseline baseline)
    {
        var lines = new List<string>
        {
            "StS2 Launcher — Step 50.0 direct-main-menu frame/input static map",
            $"Selected compatibility SHA-256: {baseline.SelectedSha256}",
            $"Actual in-tree menu nodes: {nodes.Count}",
            $"Selected managed node types: {managedTypes.Count}",
            $"Immediate frame/input roots: {roots.Count}",
            $"Invocation-qualified closure methods: {audit.ImmediateClosureMethods.Length}",
            $"Immediate classified boundaries: {audit.ImmediateBoundaries.Length}",
            $"Deferred method frontiers: {audit.DeferredMethodFrontiers.Length}",
            $"Runtime-guarded immediate frontiers: {audit.GuardedMethodFrontiers.Length}",
            "Forbidden immediate boundaries: 0",
            "Unresolved same-sts2 references: 0",
            "Rendering restarted while map built: NO",
            "Original LaunchMainMenu invoked: NO",
            "[FRAME/INPUT ROOTS]",
        };
        foreach (var root in roots)
            lines.Add($"  - token=0x{root.MetadataToken.ToUInt32():X8}; {root.FullName}");
        lines.Add(BuildStartupLadderInvocationFrontierAppendix(audit));
        return string.Join("\n", lines) + "\n";
    }

    private sealed record StartupLadderDeferredMethodFrontier(string Opcode, string ReferenceFullName, string? Category, string Path);
    private sealed record StartupLadderGuardedMethodFrontier(string ReferenceFullName, string Reason, string Path);
    private sealed record StartupLadderRuntimeGuardAuthority(
        string[] GodotCommandLineArgs,
        object SaveManagerInstance,
        object NullPlatformStrategy,
        string PrimaryPlatform,
        int RootSceneChildren,
        int MenuChildren,
        StartupLadderBaseline PostRehearsalBaseline,
        bool RehearsalPassed);
    private sealed record StartupLadderInvocationFrontierAudit(
        Step41ClosureMethod[] ImmediateClosureMethods,
        Step41BoundaryObservation[] ImmediateBoundaries,
        StartupLadderDeferredMethodFrontier[] DeferredMethodFrontiers,
        StartupLadderGuardedMethodFrontier[] GuardedMethodFrontiers,
        string[] UnresolvedSameAssemblyReferences,
        string[] StateMachineExpansions);
}
