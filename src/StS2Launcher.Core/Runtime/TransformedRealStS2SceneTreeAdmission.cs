using System.Collections;
using System.Reflection;
using System.Runtime.Loader;
using System.Security.Cryptography;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace StS2Launcher.Core;

/// <summary>
/// Step 39.0 boundary. Physical Step 38.2 proved the iOS/off-tree-compatible NGame._EnterTree body returns
/// successfully with the startup wrapper inert. Step 39 does not rerun that synthetic off-tree callback.
/// Instead it requires the same-process Step-37.0.1 scene authority, re-verifies the Step-38.2 compatibility
/// image and a small exact PCK preflight set, instantiates a fresh NGame with NGame.Instance still null,
/// audits the actual managed _EnterTree/_Ready/_Notification surface present in that hierarchy, and only then
/// attaches the NGame once to the live Godot SceneTree root. The caller freezes the render loop immediately
/// after Gate C before Gate D, so _Process-style frame callbacks are not admitted by this step. GameStartup,
/// InitializePlatform, main-menu/deferred startup, native game/GDExtension loading and explicit _ExitTree
/// remain forbidden.
/// </summary>
public sealed partial class TransformedRealStS2VeryEarlyInitialization
{
    public const string Step39AudioProxyRemapPath = "res://src/gdscript/audio_manager_proxy.gd.remap";
    public const string Step39AudioProxyGdcPath = "res://src/gdscript/audio_manager_proxy.gdc";
    public const string Step39ReactionWheelPath = "res://scenes/ui/reaction_wheel.tscn";
    public const string Step39TimeoutOverlayPath = "res://scenes/ui/multiplayer_timeout_overlay.tscn";

    public const string Step39AudioProxyRemapSha256 = "1c923c319eaf9fd5b4c72a4a85364214b915f11738f991d99ec1c269e2825e13";
    public const string Step39AudioProxyRemapMd5 = "f80dcac3ec4374287c72025b58bf5c57";
    public const int Step39AudioProxyRemapBytes = 59;
    public const string Step39AudioProxyGdcSha256 = "4fc30d10a67bd68d1ba57bc4bf214218fb8b6375e2df60341b86f960f094f1bb";
    public const string Step39AudioProxyGdcMd5 = "314fabe03fbc80155e8f494885691205";
    public const int Step39AudioProxyGdcBytes = 2_799;
    public const uint Step39AudioProxyGdcTokenizerVersion = 101;
    public const uint Step39AudioProxyGdcDecompressedBytes = 9_828;
    public const string Step39ReactionWheelSha256 = "0927e9debca56fe6726b682debe2037928be34f1d363b04ea6f18d97430028d5";
    public const string Step39ReactionWheelMd5 = "786dd2f3278c4462bb1bd46e36c89dff";
    public const int Step39ReactionWheelBytes = 12_328;
    public const string Step39TimeoutOverlaySha256 = "f32ce3ff57698802566ff36ee3c11e84f586bdfa72e5e13a467a148117bb6dee";
    public const string Step39TimeoutOverlayMd5 = "3b145fa2a00fcb88d903e3c350cfcd5b";
    public const int Step39TimeoutOverlayBytes = 3_157;
    public const int Step39AuditedAudioProxyIdentifierCount = 69;

    private static readonly string[] Step39ImmediateLifecycleMethodNames = ["_EnterTree", "_Ready", "_Notification"];
    private static readonly string[] Step39ForbiddenLifecycleReferenceFragments =
    [
        "Fmod",
        "FMOD",
        "Spine",
        "Steamworks",
        "SteamService",
    ];

    private Step39PreflightSnapshot? _step39Preflight;
    private object? _step39NGameInstance;
    private object? _step39SceneTreeRoot;
    private bool _step39InsertionStarted;
    private bool _step39InsertionPassed;
    private bool _exactStep39ClosurePassed;

    public bool ExactStep39ClosurePassed => _exactStep39ClosurePassed;

    public string GetVerifiedStep39StaticMap()
        => _step39Preflight?.StaticMap
           ?? throw new InvalidOperationException("Step 39.0 Gate B has not produced the verified SceneTree-admission static map.");

    private void ResetStep39State()
    {
        _step39Preflight = null;
        _step39NGameInstance = null;
        _step39SceneTreeRoot = null;
        _step39InsertionStarted = false;
        _step39InsertionPassed = false;
        _exactStep39ClosurePassed = false;
    }

    public TransformedRealStS2SceneTreeAdmissionGateResult RunStep39PreInsertionAuthorityAndResourceAudit(
        Action<string>? checkpoint = null)
    {
        const TransformedRealStS2SceneTreeAdmissionGate gate = TransformedRealStS2SceneTreeAdmissionGate.PreInsertionAuthorityAndResourceAudit;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            ResetStep39State();
            var context = RequireStep39Prerequisite("Step 39 Gate A entry");
            var preflight = _preflight ?? throw new InvalidOperationException("Step 39.0 exact selected-authority preflight is absent.");
            var baseline = RequireStep37Baseline();
            var resolverBefore = context.ManagedResolverRequests.Count;
            var hostBefore = context.HostLoads.Count;
            var privateBefore = context.PrivateLoads.Count;
            var initializerBefore = context.InitializerBearingRequests.Count;
            var rejectedBefore = context.RejectedManagedRequests.Count;
            var nativeBefore = context.NativeLoadAttempts.Count;

            Checkpoint(checkpoint, "I_A_ENTRY — same-process Step-37.0.1 4/4 authority present; Step 38 is treated as prior physical evidence and must not have been rerun in this process. Re-verifying exact Step-38.2 compatibility image plus four sealed Step-39 preflight PCK resources before any real SceneTree insertion.");

            stage = "selected Step-38.2 compatibility re-verification";
            VerifyFileLength(preflight.DiagnosticPath, preflight.DiagnosticLength, "Step-39 selected sts2 compatibility image");
            var selectedSha256 = ComputeSha256Hex(preflight.DiagnosticPath);
            if (!selectedSha256.Equals(preflight.DiagnosticSha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException($"Step 39.0 selected sts2 compatibility image hash drifted. expected={preflight.DiagnosticSha256}; actual={selectedSha256}.");
            using (var resolver = new RejectingAssemblyResolver())
            using (var module = ModuleDefinition.ReadModule(preflight.DiagnosticPath, new ReaderParameters
            {
                ReadSymbols = false,
                ReadingMode = ReadingMode.Deferred,
                InMemory = true,
                AssemblyResolver = resolver,
                MetadataResolver = new MetadataResolver(resolver),
            }))
            {
                var nGame = module.Types.SingleOrDefault(type => type.FullName == NGameTypeFullName)
                    ?? throw new InvalidDataException($"Step 39.0 could not locate {NGameTypeFullName} in selected authority.");
                var enterTree = RequireLifecycleMethod(nGame, NGameEnterTreeMethodName, 0, "System.Void");
                var ready = RequireLifecycleMethod(nGame, NGameReadyMethodName, 0, "System.Void");
                var wrapper = RequireLifecycleMethodByName(nGame, NGameGameStartupWrapperMethodName, 0);
                RequireInertGameStartupWrapper(wrapper);
                RequireStep38OffTreeCompatibility(enterTree);
                if (!ready.HasBody || ready.Body.Instructions.Count == 0)
                    throw new InvalidDataException("Step 39.0 expected a non-empty NGame._Ready body for the real SceneTree boundary.");
                if (resolver.Requests.Count != 0)
                    throw new InvalidDataException("Step 39.0 selected-authority Cecil verification unexpectedly attempted external resolution: " + string.Join(" | ", resolver.Requests));
            }

            stage = "four-resource PCK preflight";
            var resources = ExtractStep39ResourcesFromPck(baseline.PackAbsolutePath);
            RequireStep39Resource(resources, Step39AudioProxyRemapPath, Step39AudioProxyRemapBytes, Step39AudioProxyRemapSha256, Step39AudioProxyRemapMd5);
            RequireStep39Resource(resources, Step39AudioProxyGdcPath, Step39AudioProxyGdcBytes, Step39AudioProxyGdcSha256, Step39AudioProxyGdcMd5);
            RequireStep39Resource(resources, Step39ReactionWheelPath, Step39ReactionWheelBytes, Step39ReactionWheelSha256, Step39ReactionWheelMd5);
            RequireStep39Resource(resources, Step39TimeoutOverlayPath, Step39TimeoutOverlayBytes, Step39TimeoutOverlaySha256, Step39TimeoutOverlayMd5);

            var remapText = new UTF8Encoding(false, true).GetString(resources[Step39AudioProxyRemapPath].Bytes)
                .Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');
            if (!string.Equals(remapText, "[remap]\n\npath=\"res://src/gdscript/audio_manager_proxy.gdc\"\n", StringComparison.Ordinal))
                throw new InvalidDataException("Step 39.0 audio_manager_proxy.gd.remap content drifted from the exact audited remap.");

            var gdc = resources[Step39AudioProxyGdcPath].Bytes;
            if (gdc.Length < 12 || gdc[0] != (byte)'G' || gdc[1] != (byte)'D' || gdc[2] != (byte)'S' || gdc[3] != (byte)'C')
                throw new InvalidDataException("Step 39.0 audio_manager_proxy.gdc lacks the expected GDSC tokenizer-buffer magic.");
            var tokenizerVersion = BitConverter.ToUInt32(gdc, 4);
            var decompressedBytes = BitConverter.ToUInt32(gdc, 8);
            if (tokenizerVersion != Step39AudioProxyGdcTokenizerVersion || decompressedBytes != Step39AudioProxyGdcDecompressedBytes)
                throw new InvalidDataException($"Step 39.0 audio proxy GDScript tokenizer header drifted: version={tokenizerVersion}; decompressedBytes={decompressedBytes}.");

            foreach (var scenePath in new[] { Step39ReactionWheelPath, Step39TimeoutOverlayPath })
            {
                var sceneText = new UTF8Encoding(false, true).GetString(resources[scenePath].Bytes);
                foreach (var fragment in new[] { "Fmod", "FMOD", "Spine", "Sentry", "Steam", ".gdextension" })
                {
                    if (sceneText.Contains(fragment, StringComparison.OrdinalIgnoreCase))
                        throw new InvalidDataException($"Step 39.0 preflight scene {scenePath} unexpectedly contains forbidden native/platform fragment '{fragment}'.");
                }
            }
            var reactionText = new UTF8Encoding(false, true).GetString(resources[Step39ReactionWheelPath].Bytes);
            RequireExactCount(reactionText, "path=\"res://src/Core/Nodes/Reaction/NReactionWheel.cs\"", 1, "Step39 NReactionWheel script reference");
            RequireExactCount(reactionText, "path=\"res://src/Core/Nodes/Reaction/NReactionWheelWedge.cs\"", 1, "Step39 NReactionWheelWedge script reference");
            var timeoutText = new UTF8Encoding(false, true).GetString(resources[Step39TimeoutOverlayPath].Bytes);
            RequireExactCount(timeoutText, "path=\"res://src/Core/Nodes/Multiplayer/NMultiplayerTimeoutOverlay.cs\"", 1, "Step39 NMultiplayerTimeoutOverlay script reference");

            RequireNoForbiddenStep37Escape(context, initializerBefore, rejectedBefore, nativeBefore, "Step 39 Gate A");
            if (context.ManagedResolverRequests.Count != resolverBefore || context.HostLoads.Count != hostBefore || context.PrivateLoads.Count != privateBefore)
                throw new InvalidDataException("Step 39.0 Gate A metadata/PCK preflight unexpectedly changed the managed resolver/load baseline.");

            _step39Preflight = new Step39PreflightSnapshot(
                preflight.DiagnosticPath,
                selectedSha256,
                resources,
                string.Empty,
                resolverBefore,
                hostBefore,
                privateBefore,
                initializerBefore,
                rejectedBefore,
                nativeBefore);

            Checkpoint(checkpoint, $"I_A_PASS — selected Step-38.2 compatibility authority reverified; four exact PCK preflight resources matched bytes/SHA-256/MD5; audio proxy GDScript header=GDSC/v{tokenizerVersion}/decompressed={decompressedBytes}; offline token audit authority={Step39AuditedAudioProxyIdentifierCount} identifiers with no lifecycle callbacks; nested scenes contain no FMOD/Spine/Sentry/Steam/GDExtension declarations; resolver/native deltas=0.");
            return Step39Pass(gate,
                "STEP 39.0 PRE-INSERTION AUTHORITY/RESOURCE AUDIT PASSED.\n" +
                $"Selected compatibility SHA-256: {selectedSha256}\n" +
                $"Audio proxy GDScript: exact {Step39AudioProxyGdcBytes:N0} bytes; tokenizer v{tokenizerVersion}; decompressed token buffer {decompressedBytes:N0} bytes\n" +
                $"Offline exact-byte token audit: {Step39AuditedAudioProxyIdentifierCount} identifiers; no _enter_tree/_ready/_process lifecycle identifier\n" +
                "Reaction wheel / timeout overlay native-extension declarations: NONE\n" +
                "SceneTree insertion performed: NO\n" +
                "GameStartup / platform / main-menu / deferred / native loading: NO");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"I_A_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return Step39Fail(gate, stage, ex, includeDiagnostic: true);
        }
    }

    public TransformedRealStS2SceneTreeAdmissionGateResult RunStep39OffTreeHierarchyAndLifecycleSurfaceAudit(
        Action<string>? checkpoint = null)
    {
        const TransformedRealStS2SceneTreeAdmissionGate gate = TransformedRealStS2SceneTreeAdmissionGate.OffTreeHierarchyAndLifecycleSurfaceAudit;
        var stage = "initialization";
        object? instance = null;
        try
        {
            ThrowIfDisposed();
            var context = RequireStep39Prerequisite("Step 39 Gate B entry");
            var preflight = _step39Preflight ?? throw new InvalidOperationException("Step 39.0 Gate A must pass before Gate B.");
            var packed = RequireGameScenePackedResource();
            var handoff = _callbackHandoff ?? throw new InvalidOperationException("Step 39.0 exact GodotSharp handoff disappeared.");
            var godotAssembly = handoff.GodotSharpAssembly;
            var packedSceneType = godotAssembly.GetType("Godot.PackedScene", throwOnError: true, ignoreCase: false)
                ?? throw new MissingMemberException("Godot.PackedScene");
            var nodeType = godotAssembly.GetType("Godot.Node", throwOnError: true, ignoreCase: false)
                ?? throw new MissingMemberException("Godot.Node");

            Checkpoint(checkpoint, "I_B_ENTRY — instantiating a fresh copy of the physically proven FMOD-neutral PackedScene off-tree, requiring NGame.Instance to remain null, enumerating the actual node graph, mapping managed immediate lifecycle callbacks, and resolving the live SceneTree root without inserting the game.");
            stage = "fresh off-tree NGame instantiation";
            var instantiate = packedSceneType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .SingleOrDefault(method =>
                {
                    if (method.Name != "Instantiate" || !nodeType.IsAssignableFrom(method.ReturnType))
                        return false;
                    var parameters = method.GetParameters();
                    return parameters.Length == 1 && parameters[0].ParameterType.IsEnum;
                })
                ?? throw new MissingMethodException("Godot.PackedScene", "Instantiate(GenEditState)");
            try
            {
                var disabled = Enum.ToObject(instantiate.GetParameters()[0].ParameterType, 0);
                instance = instantiate.Invoke(packed.Resource, new[] { disabled });
            }
            catch (TargetInvocationException ex) when (ex.InnerException is not null)
            {
                throw new InvalidOperationException(
                    $"Step 39.0 PackedScene.Instantiate threw {ex.InnerException.GetType().FullName}: {ex.InnerException.Message}\n{FormatExceptionDiagnostic(ex.InnerException)}",
                    ex.InnerException);
            }
            if (instance is null || !nodeType.IsInstanceOfType(instance))
                throw new InvalidDataException($"Step 39.0 PackedScene.Instantiate did not return Godot.Node; observed {instance?.GetType().FullName ?? "<null>"}.");
            if (!string.Equals(instance.GetType().FullName, NGameTypeFullName, StringComparison.Ordinal))
                throw new InvalidDataException($"Step 39.0 expected root {NGameTypeFullName}; observed {instance.GetType().FullName}.");
            if (!ReferenceEquals(AssemblyLoadContext.GetLoadContext(instance.GetType().Assembly), context))
                throw new InvalidDataException("Step 39.0 NGame instance is not owned by the exact selected private load context.");
            if (RequireZeroArgBoolMethod(instance.GetType(), "IsInsideTree").Invoke(instance, null) is not false)
                throw new InvalidDataException("Step 39.0 fresh NGame unexpectedly reports IsInsideTree=true before AddChild.");

            stage = "NGame singleton cleanliness";
            var instanceGetter = instance.GetType().GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .SingleOrDefault(method => method.Name == "get_Instance" && method.GetParameters().Length == 0 && method.ReturnType == instance.GetType())
                ?? throw new MissingMethodException(NGameTypeFullName, "get_Instance()");
            var existingSingleton = instanceGetter.Invoke(null, null);
            if (existingSingleton is not null)
                throw new InvalidDataException("Step 39.0 requires NGame.Instance == null before real SceneTree insertion. Do not run Step 38 in the same process before Step 39.");

            stage = "live SceneTree root resolution";
            var engineType = godotAssembly.GetType("Godot.Engine", throwOnError: true, ignoreCase: false)
                ?? throw new MissingMemberException("Godot.Engine");
            var sceneTreeType = godotAssembly.GetType("Godot.SceneTree", throwOnError: true, ignoreCase: false)
                ?? throw new MissingMemberException("Godot.SceneTree");
            var getMainLoop = engineType.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .SingleOrDefault(method => method.Name == "GetMainLoop" && method.GetParameters().Length == 0)
                ?? throw new MissingMethodException("Godot.Engine", "GetMainLoop()");
            var mainLoop = getMainLoop.Invoke(null, null)
                ?? throw new InvalidDataException("Step 39.0 Godot.Engine.GetMainLoop returned null.");
            if (!sceneTreeType.IsInstanceOfType(mainLoop))
                throw new InvalidDataException($"Step 39.0 main loop is not Godot.SceneTree; observed {mainLoop.GetType().FullName}.");
            var rootProperty = sceneTreeType.GetProperty("Root", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                ?? throw new MissingMemberException("Godot.SceneTree", "Root");
            var sceneTreeRoot = rootProperty.GetValue(mainLoop)
                ?? throw new InvalidDataException("Step 39.0 live SceneTree.Root returned null.");
            if (!nodeType.IsInstanceOfType(sceneTreeRoot))
                throw new InvalidDataException($"Step 39.0 SceneTree.Root is not Godot.Node; observed {sceneTreeRoot.GetType().FullName}.");
            if (RequireZeroArgBoolMethod(sceneTreeRoot.GetType(), "IsInsideTree").Invoke(sceneTreeRoot, null) is not true)
                throw new InvalidDataException("Step 39.0 live SceneTree root unexpectedly reports IsInsideTree=false.");

            stage = "actual hierarchy managed-lifecycle static audit";
            var nodes = EnumerateStep39NodeGraph(instance, nodeType);
            if (nodes.Count < 6 || nodes.Count > 256)
                throw new InvalidDataException($"Step 39.0 actual game hierarchy node count is implausible: {nodes.Count}.");
            var selectedAssembly = instance.GetType().Assembly;
            var managedTypeNames = nodes.Select(item => item.Node.GetType())
                .Where(type => ReferenceEquals(type.Assembly, selectedAssembly))
                .Select(type => type.FullName ?? string.Empty)
                .Where(name => name.Length != 0)
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
            var callbacks = new List<Step39ManagedCallbackAudit>();
            var callbackTokens = new HashSet<uint>();
            var managedLifecycleClosureTokens = new HashSet<uint>();
            var forbiddenReferences = new SortedSet<string>(StringComparer.Ordinal);
            var unresolvedSameAssemblyReferences = new SortedSet<string>(StringComparer.Ordinal);
            foreach (var typeName in managedTypeNames)
            {
                if (!allTypes.TryGetValue(typeName, out var typeDef))
                    throw new InvalidDataException($"Step 39.0 actual managed node type {typeName} is absent from selected Cecil authority.");

                // Godot may dispatch a lifecycle override inherited from a game-owned managed base type.
                // Audit the full in-module base chain for every actual selected-sts2 node, stopping as soon as
                // the base leaves sts2 (for example Godot.Node/Control). No external Cecil resolution is used.
                for (TypeDefinition? current = typeDef; current is not null;)
                {
                    foreach (var method in current.Methods.Where(method => Step39ImmediateLifecycleMethodNames.Contains(method.Name, StringComparer.Ordinal) && method.HasBody))
                    {
                        var token = method.MetadataToken.ToUInt32();
                        if (!callbackTokens.Add(token))
                            continue;
                        var methodReferences = method.Body.Instructions
                            .Select(instruction => instruction.Operand)
                            .OfType<MethodReference>()
                            .ToArray();
                        var references = methodReferences
                            .Select(reference => reference.FullName)
                            .Distinct(StringComparer.Ordinal)
                            .OrderBy(name => name, StringComparer.Ordinal)
                            .ToArray();
                        AuditStep39ManagedLifecycleClosure(
                            method,
                            allTypes,
                            allMethods,
                            managedLifecycleClosureTokens,
                            forbiddenReferences,
                            unresolvedSameAssemblyReferences);
                        callbacks.Add(new Step39ManagedCallbackAudit(current.FullName, method.Name, token, references));
                    }

                    var baseName = current.BaseType?.FullName;
                    current = baseName is not null && allTypes.TryGetValue(baseName, out var baseType)
                        ? baseType
                        : null;
                }
            }
            if (unresolvedSameAssemblyReferences.Count != 0)
                throw new InvalidDataException("Step 39.0 managed lifecycle closure contains same-sts2 references that could not be mapped without resolution: " + string.Join(" | ", unresolvedSameAssemblyReferences));
            if (forbiddenReferences.Count != 0)
                throw new InvalidDataException("Step 39.0 managed lifecycle closure reaches forbidden startup/native/platform references: " + string.Join(" | ", forbiddenReferences));
            if (resolver.Requests.Count != 0)
                throw new InvalidDataException("Step 39.0 hierarchy lifecycle audit unexpectedly attempted external Cecil resolution: " + string.Join(" | ", resolver.Requests));

            var staticMap = BuildStep39StaticMap(preflight, nodes, managedTypeNames, callbacks, managedLifecycleClosureTokens.Count, sceneTreeRoot);
            _step39Preflight = preflight with { StaticMap = staticMap };
            _step39NGameInstance = instance;
            _step39SceneTreeRoot = sceneTreeRoot;
            instance = null;

            RequireNoForbiddenStep37Escape(context, preflight.InitializerCount, preflight.RejectedCount, preflight.NativeCount, "Step 39 Gate B");
            Checkpoint(checkpoint, $"I_B_PASS — fresh off-tree NGame prepared with NGame.Instance=null; live SceneTree root={sceneTreeRoot.GetType().FullName}; nodeCount={nodes.Count}; selectedManagedNodeTypes={managedTypeNames.Length}; immediateManagedCallbacks={callbacks.Count}; transitiveSameSts2LifecycleClosureMethods={managedLifecycleClosureTokens.Count}; forbiddenLifecycleRefs=0; unresolvedSameSts2Refs=0; externalResolutionRequests=0; insideTree=False; AddChild not yet invoked.");
            return Step39Pass(gate,
                "STEP 39.0 OFF-TREE HIERARCHY/LIFECYCLE SURFACE AUDIT PASSED.\n" +
                $"Actual node count: {nodes.Count}\n" +
                $"Selected sts2 managed node types: {managedTypeNames.Length}\n" +
                $"Immediate managed _EnterTree/_Ready/_Notification methods mapped: {callbacks.Count}\n" +
                $"Transitive same-sts2 lifecycle closure methods: {managedLifecycleClosureTokens.Count}\n" +
                "Forbidden lifecycle-closure references: 0\n" +
                "Unresolved same-sts2 lifecycle references: 0\n" +
                $"Live SceneTree root: {sceneTreeRoot.GetType().FullName}\n" +
                "NGame.Instance before insertion: NULL\n" +
                "NGame IsInsideTree: FALSE\n" +
                "AddChild: NOT YET");
        }
        catch (Exception ex)
        {
            if (instance is not null)
                TryReleaseStep39OffTreeInstance(instance, checkpoint, "Gate-B failure");
            Checkpoint(checkpoint, $"I_B_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return Step39Fail(gate, stage, ex, includeDiagnostic: true);
        }
    }

    public TransformedRealStS2SceneTreeAdmissionGateResult RunStep39RealSceneTreeInsertion(
        Action<string>? checkpoint = null)
    {
        const TransformedRealStS2SceneTreeAdmissionGate gate = TransformedRealStS2SceneTreeAdmissionGate.RealSceneTreeInsertion;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep39Prerequisite("Step 39 Gate C entry");
            var preflight = _step39Preflight ?? throw new InvalidOperationException("Step 39.0 Gate B must pass before Gate C.");
            var instance = _step39NGameInstance ?? throw new InvalidOperationException("Step 39.0 Gate B NGame instance is absent.");
            var sceneTreeRoot = _step39SceneTreeRoot ?? throw new InvalidOperationException("Step 39.0 live SceneTree root is absent.");
            var handoff = _callbackHandoff ?? throw new InvalidOperationException("Step 39.0 exact GodotSharp handoff disappeared.");
            var godotAssembly = handoff.GodotSharpAssembly;
            var nodeType = godotAssembly.GetType("Godot.Node", throwOnError: true, ignoreCase: false)
                ?? throw new MissingMemberException("Godot.Node");

            if (RequireZeroArgBoolMethod(instance.GetType(), "IsInsideTree").Invoke(instance, null) is not false)
                throw new InvalidDataException("Step 39.0 NGame entered the SceneTree before the authorized Gate-C AddChild boundary.");
            var instanceGetter = instance.GetType().GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .Single(method => method.Name == "get_Instance" && method.GetParameters().Length == 0 && method.ReturnType == instance.GetType());
            if (instanceGetter.Invoke(null, null) is not null)
                throw new InvalidDataException("Step 39.0 NGame.Instance became non-null before AddChild.");

            var resolverBefore = context.ManagedResolverRequests.Count;
            var hostBefore = context.HostLoads.Count;
            var privateBefore = context.PrivateLoads.Count;
            var initializerBefore = context.InitializerBearingRequests.Count;
            var rejectedBefore = context.RejectedManagedRequests.Count;
            var nativeBefore = context.NativeLoadAttempts.Count;

            Checkpoint(checkpoint, "I_C_ADDCHILD_START — adding the fresh real NGame exactly once to the live Godot SceneTree root. This intentionally authorizes Godot automatic _EnterTree/_Ready/_Notification callbacks for the audited hierarchy. GameStartup remains blocked by the verified inert wrapper. No explicit _Ready/_ExitTree/ExecuteDeferred/native extension call is made by the launcher.");
            stage = "live SceneTree Root.AddChild(NGame)";
            var addChild = sceneTreeRoot.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .SingleOrDefault(method =>
                {
                    if (method.Name != "AddChild")
                        return false;
                    var parameters = method.GetParameters();
                    return parameters.Length == 3 &&
                           parameters[0].ParameterType == nodeType &&
                           parameters[1].ParameterType == typeof(bool) &&
                           parameters[2].ParameterType.IsEnum;
                })
                ?? throw new MissingMethodException(sceneTreeRoot.GetType().FullName, "AddChild(Node,bool,InternalMode)");
            _step39InsertionStarted = true;
            try
            {
                var internalDisabled = Enum.ToObject(addChild.GetParameters()[2].ParameterType, 0);
                addChild.Invoke(sceneTreeRoot, new[] { instance, (object)false, internalDisabled });
            }
            catch (TargetInvocationException ex) when (ex.InnerException is not null)
            {
                var inside = TryReadIsInsideTree(instance);
                Checkpoint(checkpoint, $"I_C_ADDCHILD_EXCEPTION — base={ex.InnerException.GetType().FullName}; insideTreeAfterFailure={inside}; resolverDelta={context.ManagedResolverRequests.Count - resolverBefore}; hostDelta={context.HostLoads.Count - hostBefore}; privateDelta={context.PrivateLoads.Count - privateBefore}; initializerDelta={context.InitializerBearingRequests.Count - initializerBefore}; rejectedDelta={context.RejectedManagedRequests.Count - rejectedBefore}; nativeDelta={context.NativeLoadAttempts.Count - nativeBefore}; detail={SanitizeCheckpoint(FormatExceptionDiagnostic(ex.InnerException))}");
                throw new InvalidOperationException(
                    $"Step 39.0 SceneTree AddChild threw {ex.InnerException.GetType().FullName}: {ex.InnerException.Message}\n{FormatExceptionDiagnostic(ex.InnerException)}",
                    ex.InnerException);
            }

            stage = "post-AddChild automatic lifecycle proof";
            if (RequireZeroArgBoolMethod(instance.GetType(), "IsInsideTree").Invoke(instance, null) is not true)
                throw new InvalidDataException("Step 39.0 Root.AddChild returned but NGame reports IsInsideTree=false.");
            if (!ReferenceEquals(instanceGetter.Invoke(null, null), instance))
                throw new InvalidDataException("Step 39.0 automatic NGame._EnterTree did not establish NGame.Instance to the inserted object.");
            var windowField = instance.GetType().GetField("_window", BindingFlags.Static | BindingFlags.NonPublic)
                ?? throw new MissingFieldException(NGameTypeFullName, "_window");
            if (windowField.GetValue(null) is null)
                throw new InvalidDataException("Step 39.0 NGame._Ready evidence is absent: static _window remained null after AddChild.");
            var getParent = instance.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .SingleOrDefault(method => method.Name == "GetParent" && method.GetParameters().Length == 0 && nodeType.IsAssignableFrom(method.ReturnType))
                ?? throw new MissingMethodException(NGameTypeFullName, "GetParent()");
            if (!ReferenceEquals(getParent.Invoke(instance, null), sceneTreeRoot))
                throw new InvalidDataException("Step 39.0 inserted NGame parent is not the exact live SceneTree root.");
            var state = ReadOneTimeInitializationState(RequireEssentialBinding().StateField);
            if (state != ExpectedStateAfterEssential)
                throw new InvalidDataException($"Step 39.0 OneTimeInitialization state drifted during real SceneTree insertion: expected {ExpectedStateAfterEssential}; observed {state}.");
            RequireNoForbiddenStep37Escape(context, initializerBefore, rejectedBefore, nativeBefore, "Step 39 Gate C");
            _step39InsertionPassed = true;

            Checkpoint(checkpoint, $"I_C_PASS — live SceneTree Root.AddChild returned; NGame IsInsideTree=True; NGame.Instance points to inserted object; NGame._Ready established non-null _window; parent=SceneTree.Root; state={state}; resolverDelta={context.ManagedResolverRequests.Count - resolverBefore}; hostDelta={context.HostLoads.Count - hostBefore}; privateDelta={context.PrivateLoads.Count - privateBefore}; initializerDelta=0; rejectedDelta=0; nativeDelta=0. Caller must freeze rendering immediately before Gate D.");
            return Step39Pass(gate,
                "STEP 39.0 REAL SCENETREE INSERTION PASSED.\n" +
                "SceneTree.Root.AddChild(NGame): RETURNED\n" +
                "NGame IsInsideTree: TRUE\n" +
                "NGame.Instance: inserted object\n" +
                "NGame._Ready evidence (_window): NON-NULL\n" +
                "Parent: exact live SceneTree.Root\n" +
                $"OneTimeInitialization state: {state}\n" +
                $"Managed resolver delta: {FormatDelta(context.ManagedResolverRequests.Skip(resolverBefore).ToArray())}\n" +
                $"Host-load delta: {FormatDelta(context.HostLoads.Skip(hostBefore).ToArray())}\n" +
                $"Private-load delta: {FormatDelta(context.PrivateLoads.Skip(privateBefore).ToArray())}\n" +
                "Initializer-bearing request delta: 0\n" +
                "Rejected managed request delta: 0\n" +
                "Native-load attempt delta: 0\n" +
                "GameStartup / platform / main-menu / deferred: blocked by authority");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"I_C_FAIL — stage={stage}; insertionStarted={_step39InsertionStarted}; insideTree={(_step39NGameInstance is null ? "<no-instance>" : TryReadIsInsideTree(_step39NGameInstance))}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return Step39Fail(gate, stage, ex, includeDiagnostic: true);
        }
    }

    public TransformedRealStS2SceneTreeAdmissionGateResult RunStep39FrozenPostInsertionConfinement(
        bool renderingStopped,
        Action<string>? checkpoint = null)
    {
        const TransformedRealStS2SceneTreeAdmissionGate gate = TransformedRealStS2SceneTreeAdmissionGate.FrozenPostInsertionConfinement;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep39Prerequisite("Step 39 Gate D entry");
            var preflight = _step39Preflight ?? throw new InvalidOperationException("Step 39.0 Gate B static audit is absent.");
            var instance = _step39NGameInstance ?? throw new InvalidOperationException("Step 39.0 inserted NGame instance is absent.");
            var sceneTreeRoot = _step39SceneTreeRoot ?? throw new InvalidOperationException("Step 39.0 SceneTree root is absent.");
            if (!_step39InsertionStarted || !_step39InsertionPassed)
                throw new InvalidOperationException("Step 39.0 Gate D requires successful real SceneTree insertion.");
            if (!renderingStopped)
                throw new InvalidOperationException("Step 39.0 refuses post-insertion closure unless the iOS Godot render loop was synchronously stopped immediately after Gate C.");

            Checkpoint(checkpoint, "I_D_ENTRY — render loop is frozen after successful AddChild; proving the inserted NGame remains in-tree/state=2 with exact singleton/parent authority and zero initializer/rejected/native escape. The launcher will not RemoveChild, Free, invoke _ExitTree, restart rendering, or enter GameStartup/deferred work in Step 39.");
            stage = "frozen inserted-tree confinement";
            if (RequireZeroArgBoolMethod(instance.GetType(), "IsInsideTree").Invoke(instance, null) is not true)
                throw new InvalidDataException("Step 39.0 inserted NGame left the SceneTree before Gate-D confinement.");
            var instanceGetter = instance.GetType().GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .Single(method => method.Name == "get_Instance" && method.GetParameters().Length == 0 && method.ReturnType == instance.GetType());
            if (!ReferenceEquals(instanceGetter.Invoke(null, null), instance))
                throw new InvalidDataException("Step 39.0 NGame.Instance no longer points to the inserted object.");
            var getParent = instance.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Single(method => method.Name == "GetParent" && method.GetParameters().Length == 0);
            if (!ReferenceEquals(getParent.Invoke(instance, null), sceneTreeRoot))
                throw new InvalidDataException("Step 39.0 inserted NGame parent drifted from SceneTree.Root.");
            var windowField = instance.GetType().GetField("_window", BindingFlags.Static | BindingFlags.NonPublic)
                ?? throw new MissingFieldException(NGameTypeFullName, "_window");
            if (windowField.GetValue(null) is null)
                throw new InvalidDataException("Step 39.0 NGame._Ready window authority disappeared before Gate D.");
            var state = ReadOneTimeInitializationState(RequireEssentialBinding().StateField);
            if (state != ExpectedStateAfterEssential)
                throw new InvalidDataException($"Step 39.0 OneTimeInitialization state drifted after insertion: expected {ExpectedStateAfterEssential}; observed {state}.");
            RequireNoForbiddenStep37Escape(context, preflight.InitializerCount, preflight.RejectedCount, preflight.NativeCount, "Step 39 Gate D");
            _exactStep39ClosurePassed = true;

            Checkpoint(checkpoint, $"I_D_PASS — real NGame remains attached to SceneTree.Root with IsInsideTree=True, NGame.Instance exact, _window non-null, state={state}; renderingStopped=True; initializerDelta=0; rejectedDelta=0; nativeDelta=0. Instance intentionally retained in-tree; _ExitTree/RemoveChild/Free/GameStartup/platform/main-menu/deferred remain uninvoked by Step 39.");
            return Step39Pass(gate,
                "STEP 39.0 FROZEN POST-INSERTION CONFINEMENT PASSED.\n" +
                "Rendering loop stopped immediately after AddChild: TRUE\n" +
                "NGame remains inside SceneTree: TRUE\n" +
                "NGame.Instance / parent / _window authority: PRESERVED\n" +
                $"OneTimeInitialization state: {state}\n" +
                "Initializer-bearing request delta from Step-39 baseline: 0\n" +
                "Rejected managed request delta from Step-39 baseline: 0\n" +
                "Native-load attempt delta from Step-39 baseline: 0\n" +
                "RemoveChild / Free / _ExitTree: NOT INVOKED\n" +
                "GameStartup / InitializePlatform / main-menu / ExecuteDeferred: NOT INVOKED\n" +
                "Inserted instance intentionally retained for a future separately authorized boundary or process relaunch.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"I_D_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return Step39Fail(gate, stage, ex, includeDiagnostic: true);
        }
    }

    private Step35ExecutionLoadContext RequireStep39Prerequisite(string boundary)
    {
        var context = RequireStep37Prerequisite(boundary);
        if (!_exactStep37ClosurePassed)
            throw new InvalidOperationException($"{boundary} requires same-process Step-37.0.1 4/4 authority.");
        if (_step38EnterTreeInvocationStarted || _step38NGameInstance is not null)
            throw new InvalidOperationException($"{boundary} refuses a process in which Step 38 synthetic lifecycle execution/reinstantiation has already started. Relaunch and run Step 15 -> 35 -> 36 -> 37 -> 39, skipping Step 38 in that process.");
        return context;
    }

    private static Dictionary<string, Step39ExtractedResource> ExtractStep39ResourcesFromPck(string pckPath)
    {
        var expected = new HashSet<string>(StringComparer.Ordinal)
        {
            Step39AudioProxyRemapPath,
            Step39AudioProxyGdcPath,
            Step39ReactionWheelPath,
            Step39TimeoutOverlayPath,
        };
        using var stream = new FileStream(pckPath, FileMode.Open, FileAccess.Read, FileShare.Read, 16 * 1024, FileOptions.SequentialScan);
        using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);
        var magic = reader.ReadUInt32();
        if (magic != 0x43504447)
            throw new InvalidDataException($"Step 39.0 expected standalone Godot PCK magic 0x43504447; observed 0x{magic:X8}.");
        var format = reader.ReadUInt32();
        var major = reader.ReadUInt32();
        var minor = reader.ReadUInt32();
        var patch = reader.ReadUInt32();
        var flags = reader.ReadUInt32();
        var fileBase = reader.ReadUInt64();
        if (format != ClosedPckFormat || major != ClosedPckEngineMajor || minor != ClosedPckEngineMinor || patch != ClosedPckEnginePatch || flags != ClosedPckFlags)
            throw new InvalidDataException($"Step 39.0 PCK header drifted: format={format}; engine={major}.{minor}.{patch}; flags=0x{flags:X8}.");
        var directoryOffset = reader.ReadUInt64();
        stream.Seek(checked((long)directoryOffset), SeekOrigin.Begin);
        var count = reader.ReadUInt32();
        if (count != ClosedPckDirectoryEntries)
            throw new InvalidDataException($"Step 39.0 PCK directory entry count drifted: expected {ClosedPckDirectoryEntries}; observed {count}.");

        var entries = new Dictionary<string, Step39DirectoryEntry>(StringComparer.Ordinal);
        for (var i = 0u; i < count; i++)
        {
            var pathLength = reader.ReadUInt32();
            if (pathLength == 0 || pathLength > 1_048_576)
                throw new InvalidDataException($"Step 39.0 PCK path length invalid at entry {i}: {pathLength}.");
            var pathBytes = ReadExactlyStep37(reader, checked((int)pathLength));
            var storedPath = Encoding.UTF8.GetString(pathBytes).TrimEnd('\0').Replace('\\', '/');
            var offset = reader.ReadUInt64();
            var size = reader.ReadUInt64();
            var md5 = ReadExactlyStep37(reader, 16);
            var entryFlags = reader.ReadUInt32();
            var normalized = storedPath.StartsWith("res://", StringComparison.Ordinal) ? storedPath : "res://" + storedPath.TrimStart('/');
            if (!expected.Contains(normalized))
                continue;
            if (!entries.TryAdd(normalized, new Step39DirectoryEntry(offset, size, md5, entryFlags)))
                throw new InvalidDataException($"Step 39.0 duplicate PCK entry for {normalized}.");
        }
        if (entries.Count != expected.Count)
            throw new InvalidDataException("Step 39.0 PCK preflight set incomplete: missing=" + string.Join(",", expected.Except(entries.Keys, StringComparer.Ordinal)));

        var result = new Dictionary<string, Step39ExtractedResource>(StringComparer.Ordinal);
        foreach (var path in expected.OrderBy(value => value, StringComparer.Ordinal))
        {
            var entry = entries[path];
            if ((entry.Flags & 1u) != 0)
                throw new InvalidDataException($"Step 39.0 refuses encrypted PCK preflight resource {path}.");
            if (entry.Size > 1_048_576)
                throw new InvalidDataException($"Step 39.0 preflight resource unexpectedly exceeds 1 MiB: {path}={entry.Size}.");
            var absoluteOffset = checked(fileBase + entry.Offset);
            if (absoluteOffset + entry.Size > (ulong)stream.Length)
                throw new InvalidDataException($"Step 39.0 PCK resource points beyond file: {path}.");
            stream.Seek(checked((long)absoluteOffset), SeekOrigin.Begin);
            var bytes = ReadExactlyStep37(reader, checked((int)entry.Size));
            var directoryMd5 = Convert.ToHexString(entry.Md5).ToLowerInvariant();
            var actualMd5 = Convert.ToHexString(MD5.HashData(bytes)).ToLowerInvariant();
            if (!directoryMd5.Equals(actualMd5, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException($"Step 39.0 PCK MD5 mismatch for {path}: directory={directoryMd5}; actual={actualMd5}.");
            result[path] = new Step39ExtractedResource(bytes, directoryMd5, Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant());
        }
        return result;
    }

    private static void RequireStep39Resource(
        IReadOnlyDictionary<string, Step39ExtractedResource> resources,
        string path,
        int expectedBytes,
        string expectedSha256,
        string expectedMd5)
    {
        if (!resources.TryGetValue(path, out var resource))
            throw new FileNotFoundException($"Step 39.0 exact PCK resource missing: {path}.");
        if (resource.Bytes.Length != expectedBytes ||
            !resource.Sha256.Equals(expectedSha256, StringComparison.OrdinalIgnoreCase) ||
            !resource.Md5.Equals(expectedMd5, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidDataException($"Step 39.0 resource authority drifted for {path}: bytes={resource.Bytes.Length}; sha256={resource.Sha256}; md5={resource.Md5}.");
        }
    }

    private static List<Step39NodeObservation> EnumerateStep39NodeGraph(object root, Type nodeType)
    {
        var observations = new List<Step39NodeObservation>();
        var queue = new Queue<(object Node, string Path)>();
        queue.Enqueue((root, "/Game"));
        while (queue.Count != 0)
        {
            if (observations.Count >= 256)
                throw new InvalidDataException("Step 39.0 node-graph traversal exceeded 256 nodes.");
            var (node, path) = queue.Dequeue();
            observations.Add(new Step39NodeObservation(path, node));
            var getChildren = node.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(method => method.Name == "GetChildren")
                .OrderBy(method => method.GetParameters().Length)
                .FirstOrDefault(method =>
                {
                    var parameters = method.GetParameters();
                    return parameters.Length == 0 || (parameters.Length == 1 && parameters[0].ParameterType == typeof(bool));
                })
                ?? throw new MissingMethodException(node.GetType().FullName, "GetChildren([bool])");
            var childrenObject = getChildren.GetParameters().Length == 0
                ? getChildren.Invoke(node, null)
                : getChildren.Invoke(node, new object?[] { false });
            if (childrenObject is not IEnumerable children)
                throw new InvalidDataException($"Step 39.0 {node.GetType().FullName}.GetChildren did not return IEnumerable.");
            var index = 0;
            foreach (var child in children)
            {
                if (child is null || !nodeType.IsInstanceOfType(child))
                    continue;
                queue.Enqueue((child, path + "/" + GetStep39NodeName(child, index)));
                index++;
            }
        }
        return observations;
    }

    private static string GetStep39NodeName(object node, int fallbackIndex)
    {
        try
        {
            var property = node.GetType().GetProperty("Name", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var value = property?.GetValue(node)?.ToString();
            return string.IsNullOrWhiteSpace(value) ? $"node{fallbackIndex}" : value;
        }
        catch
        {
            return $"node{fallbackIndex}";
        }
    }

    private static void AuditStep39ManagedLifecycleClosure(
        MethodDefinition root,
        IReadOnlyDictionary<string, TypeDefinition> allTypes,
        IReadOnlyDictionary<string, MethodDefinition> allMethods,
        ISet<uint> globalClosureTokens,
        ISet<string> forbiddenReferences,
        ISet<string> unresolvedSameAssemblyReferences)
    {
        var queue = new Queue<MethodDefinition>();
        var localVisited = new HashSet<uint>();
        queue.Enqueue(root);
        while (queue.Count != 0)
        {
            var current = queue.Dequeue();
            var currentToken = current.MetadataToken.ToUInt32();
            if (!localVisited.Add(currentToken))
                continue;
            globalClosureTokens.Add(currentToken);
            if (!current.HasBody)
                continue;

            foreach (var reference in current.Body.Instructions.Select(instruction => instruction.Operand).OfType<MethodReference>())
            {
                if (IsForbiddenStep39LifecycleReference(reference))
                    forbiddenReferences.Add($"{root.FullName} -> {current.FullName} -> {reference.FullName}");

                var candidate = reference is GenericInstanceMethod generic ? generic.ElementMethod : reference;
                if (!allTypes.ContainsKey(candidate.DeclaringType.FullName))
                    continue; // leaves sts2; direct forbidden service/native names were already checked above.

                if (candidate is MethodDefinition directDefinition)
                {
                    queue.Enqueue(directDefinition);
                    continue;
                }
                if (allMethods.TryGetValue(candidate.FullName, out var definition))
                {
                    queue.Enqueue(definition);
                    continue;
                }

                unresolvedSameAssemblyReferences.Add($"{current.FullName} -> {reference.FullName}");
            }
        }
    }

    private static bool IsForbiddenStep39LifecycleReference(MethodReference reference)
    {
        if (reference.DeclaringType.FullName == TargetTypeFullName &&
            reference.Name is "ExecuteVeryEarly" or "ExecuteEssential" or "ExecuteDeferred" or "PrewarmJit")
            return true;
        if (reference.DeclaringType.FullName == NGameTypeFullName &&
            reference.Name is NGameGameStartupMethodName or NGameInitializePlatformMethodName or NGameLaunchMainMenuMethodName or NGameLoadDeferredStartupAssetsMethodName)
            return true;
        // Step 39.1 preserves the fail-closed Sentry policy while recognizing the exact game-owned
        // SentryService boundary as already transformed to inert default-return bodies in the selected
        // private compatibility image. Any surviving external Sentry reference (Sentry.*), or any
        // Sentry-named type outside this exact wrapper, remains forbidden.
        const string sentryServiceTypeName = "MegaCrit.Sts2.Core.Debug.SentryService";
        var declaringTypeFullName = reference.DeclaringType.FullName;
        if (declaringTypeFullName == sentryServiceTypeName ||
            declaringTypeFullName.StartsWith(sentryServiceTypeName + "/", StringComparison.Ordinal))
            return false;

        // 0.0.168 proved that substring matching the rendered Cecil FullName is too broad: a
        // perfectly ordinary System.Action<Sentry.Scope> constructor renders the generic argument
        // inside FullName and was therefore misclassified as an external Sentry call. Classify the
        // declaring type itself instead. This keeps Sentry.* and any other game-owned Sentry-named
        // wrapper forbidden while allowing BCL generic containers/delegates parameterized by a
        // Sentry type to remain in the closure. The delegate target itself was already inerted and
        // serialized-verified by the Step-39.1 compatibility transform.
        if (reference.DeclaringType.Namespace.Equals("Sentry", StringComparison.Ordinal) ||
            reference.DeclaringType.Namespace.StartsWith("Sentry.", StringComparison.Ordinal) ||
            (declaringTypeFullName.Contains("Sentry", StringComparison.OrdinalIgnoreCase) &&
             reference.DeclaringType.Namespace.StartsWith("MegaCrit.Sts2", StringComparison.Ordinal)))
            return true;

        return Step39ForbiddenLifecycleReferenceFragments.Any(fragment =>
            declaringTypeFullName.Contains(fragment, StringComparison.OrdinalIgnoreCase));
    }

    private static string BuildStep39StaticMap(
        Step39PreflightSnapshot preflight,
        IReadOnlyList<Step39NodeObservation> nodes,
        IReadOnlyList<string> managedTypes,
        IReadOnlyList<Step39ManagedCallbackAudit> callbacks,
        int managedLifecycleClosureMethodCount,
        object sceneTreeRoot)
    {
        var lines = new List<string>
        {
            "StS2 Launcher — Step 39.0 real SceneTree admission pre-insertion map",
            "Read-only evidence from the exact selected compatibility image, exact receipt-backed PCK resources, and a fresh off-tree instantiated hierarchy; never consumed as trusted runtime input.",
            $"Selected compatibility path: {preflight.SelectedPath}",
            $"Selected compatibility SHA-256: {preflight.SelectedSha256}",
            $"Live SceneTree root type: {sceneTreeRoot.GetType().FullName}",
            $"Actual off-tree game hierarchy nodes: {nodes.Count}",
            $"Selected sts2 managed node types: {managedTypes.Count}",
            $"Immediate managed lifecycle callbacks mapped including in-module base chains (_EnterTree/_Ready/_Notification): {callbacks.Count}",
            $"Transitive same-sts2 lifecycle closure methods mapped: {managedLifecycleClosureMethodCount}",
            "Forbidden startup/native/platform references from transitive lifecycle closure: 0",
            "Unresolved same-sts2 references from transitive lifecycle closure: 0",
            $"Audio proxy exact GDScript authority: {Step39AudioProxyGdcBytes} bytes; SHA-256={Step39AudioProxyGdcSha256}; tokenizerVersion={Step39AudioProxyGdcTokenizerVersion}; decompressedBytes={Step39AudioProxyGdcDecompressedBytes}",
            $"Audio proxy offline exact-byte identifier audit: {Step39AuditedAudioProxyIdentifierCount}; lifecycle callback identifiers: 0",
            $"Reaction wheel exact SHA-256: {Step39ReactionWheelSha256}",
            $"Multiplayer timeout overlay exact SHA-256: {Step39TimeoutOverlaySha256}",
            "Step 39 insertion policy: one SceneTree.Root.AddChild(NGame), then caller synchronously freezes rendering before Gate D.",
            "Step 39 forbidden: explicit _Ready/_ExitTree, RemoveChild/Free, GameStartup, InitializePlatform, LaunchMainMenu, ExecuteDeferred, Steam init, native GDExtension loading.",
            string.Empty,
            "[ACTUAL NODE GRAPH]",
        };
        foreach (var node in nodes)
            lines.Add($"  - {node.Path} | {node.Node.GetType().FullName}");
        lines.Add(string.Empty);
        lines.Add("[SELECTED STS2 MANAGED NODE TYPES]");
        foreach (var type in managedTypes)
            lines.Add("  - " + type);
        lines.Add(string.Empty);
        lines.Add("[IMMEDIATE MANAGED LIFECYCLE CALLBACKS]");
        foreach (var callback in callbacks.OrderBy(item => item.TypeName, StringComparer.Ordinal).ThenBy(item => item.MethodName, StringComparer.Ordinal))
        {
            lines.Add($"  - token=0x{callback.Token:X8}; {callback.TypeName}::{callback.MethodName}");
            foreach (var reference in callback.DirectMethodReferences)
                lines.Add("      callref: " + reference);
        }
        return string.Join("\n", lines) + "\n";
    }

    private static void TryReleaseStep39OffTreeInstance(object instance, Action<string>? checkpoint, string scope)
    {
        try
        {
            if (RequireZeroArgBoolMethod(instance.GetType(), "IsInsideTree").Invoke(instance, null) is true)
            {
                Checkpoint(checkpoint, $"I_INSTANCE_RETAINED — scope={scope}; instance is already inside SceneTree, so Step 39 refuses Free/RemoveChild/_ExitTree cleanup and requires process relaunch.");
                return;
            }
            var free = instance.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .SingleOrDefault(method => method.Name == "Free" && method.GetParameters().Length == 0);
            if (free is not null)
                free.Invoke(instance, null);
            else if (instance is IDisposable disposable)
                disposable.Dispose();
            Checkpoint(checkpoint, $"I_INSTANCE_RELEASED — scope={scope}; off-tree Step-39 NGame released before any real insertion.");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"I_INSTANCE_RELEASE_WARNING — scope={scope}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
        }
    }

    private static TransformedRealStS2SceneTreeAdmissionGateResult Step39Pass(
        TransformedRealStS2SceneTreeAdmissionGate gate,
        string detail)
        => new(gate, true, detail);

    private static TransformedRealStS2SceneTreeAdmissionGateResult Step39Fail(
        TransformedRealStS2SceneTreeAdmissionGate gate,
        string stage,
        Exception ex,
        bool includeDiagnostic)
        => new(gate, false,
            $"Stage: {stage}\n{ex.GetType().Name}: {ex.Message}" +
            (includeDiagnostic ? "\n" + FormatExceptionDiagnostic(ex) : string.Empty));

    private sealed record Step39DirectoryEntry(ulong Offset, ulong Size, byte[] Md5, uint Flags);
    private sealed record Step39ExtractedResource(byte[] Bytes, string Md5, string Sha256);
    private sealed record Step39NodeObservation(string Path, object Node);
    private sealed record Step39ManagedCallbackAudit(string TypeName, string MethodName, uint Token, string[] DirectMethodReferences);
    private sealed record Step39PreflightSnapshot(
        string SelectedPath,
        string SelectedSha256,
        Dictionary<string, Step39ExtractedResource> Resources,
        string StaticMap,
        int ResolverCount,
        int HostCount,
        int PrivateCount,
        int InitializerCount,
        int RejectedCount,
        int NativeCount);
}
