using System.Reflection;
using System.Runtime.Loader;
using System.Security.Cryptography;
using System.Text;

namespace StS2Launcher.Core;

/// <summary>
/// Step 37.0.1 boundary. Requires a same-process Step-36.0.5 4/4 closure, reuses the exact mounted PCK and
/// physically proven private sts2/GodotSharp authority, extracts the sealed game.tscn bytes directly from
/// the receipt-backed PCK, prepares a copied FMOD-neutral scene derivative, then loads and instantiates that
/// derivative off-tree. It intentionally does not add the instance to the SceneTree, call NGame.GameStartup,
/// launch the main menu, run ExecuteDeferred, initialize Steam, or permit native game/GDExtension loading.
/// </summary>
public sealed partial class TransformedRealStS2VeryEarlyInitialization
{
    public const string GameSceneResourcePath = "res://scenes/game.tscn";
    public const int ClosedGameSceneBytes = 10_414;
    public const string ClosedGameSceneSha256 = "aaec1e04f689122fd812b83fee09ea6e30e2991cf5851dc599b01b7802320ad8";
    public const string ClosedGameSceneMd5 = "dc9a89798eb1bb38e23563866e746ac5";
    public const uint ClosedPckFormat = 3;
    public const uint ClosedPckEngineMajor = 4;
    public const uint ClosedPckEngineMinor = 5;
    public const uint ClosedPckEnginePatch = 1;
    public const uint ClosedPckFlags = 0x00000002;
    public const uint ClosedPckDirectoryEntries = 12_328;
    public const string Step37WorkRootName = "Step37-GameSceneAdmission";
    public const string Step37DerivativeFileName = "game-step37-fmod-inert.tscn";

    private bool _exactStep36ClosurePassed;
    private Step37BaselineSnapshot? _step37Baseline;
    private GameScenePreflightSnapshot? _gameScenePreflight;
    private GameSceneDerivativeSnapshot? _gameSceneDerivative;
    private GameScenePackedResourceSnapshot? _gameScenePackedResource;

    public bool ExactStep36ClosurePassed => _exactStep36ClosurePassed;

    public string GetVerifiedGameSceneStaticMap()
        => _gameScenePreflight?.StaticMap
           ?? throw new InvalidOperationException("Step 37.0.1 Gate A has not produced a verified game-scene static map.");

    private void ResetStep37State()
    {
        _exactStep36ClosurePassed = false;
        _step37Baseline = null;
        _gameScenePreflight = null;
        _gameSceneDerivative = null;
        _gameScenePackedResource = null;
        ResetStep38State();
    }

    private void MarkExactStep36ClosurePassed(Step35ExecutionLoadContext context)
    {
        var handoff = RequireEssentialResourcePackHandoff();
        _exactStep36ClosurePassed = true;
        _step37Baseline = new Step37BaselineSnapshot(
            context.ManagedResolverRequests.ToArray(),
            context.HostLoads.ToArray(),
            context.PrivateLoads.ToArray(),
            handoff.ManagedInstallRoot,
            handoff.PackAbsolutePath,
            handoff.PackLength,
            handoff.ReceiptSha1);
        _gameScenePreflight = null;
        _gameSceneDerivative = null;
        _gameScenePackedResource = null;
    }

    public TransformedRealStS2GameSceneAdmissionGateResult RunGameSceneSealedPreflight(
        Action<string>? checkpoint = null)
    {
        const TransformedRealStS2GameSceneAdmissionGate gate = TransformedRealStS2GameSceneAdmissionGate.ClosedEssentialAndSealedScenePreflight;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep37Prerequisite("Step 37 Gate A entry");
            RequireStep37BaselineUnchanged(context, "Gate A entry");
            var baseline = RequireStep37Baseline();

            Checkpoint(checkpoint, "G_A_ENTRY — same-process Step-36.0.5 4/4 authority present; extracting sealed res://scenes/game.tscn directly from the already receipt-backed PCK without loading a scene or GDExtension.");
            stage = "receipt-backed PCK scene extraction";
            var extracted = ExtractSealedGameSceneFromPck(baseline.PackAbsolutePath);
            var sourceSha256 = Convert.ToHexString(SHA256.HashData(extracted.Bytes)).ToLowerInvariant();
            var sourceMd5 = Convert.ToHexString(MD5.HashData(extracted.Bytes)).ToLowerInvariant();
            if (extracted.Bytes.Length != ClosedGameSceneBytes ||
                !sourceSha256.Equals(ClosedGameSceneSha256, StringComparison.OrdinalIgnoreCase) ||
                !sourceMd5.Equals(ClosedGameSceneMd5, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException(
                    $"Step 37.0.1 sealed game.tscn drifted. bytes={extracted.Bytes.Length}; sha256={sourceSha256}; md5={sourceMd5}.");
            }

            stage = "sealed game.tscn structural audit";
            var sourceText = new UTF8Encoding(false, true).GetString(extracted.Bytes);
            RequireExactCount(sourceText, "[node name=\"FmodBankLoader\" type=\"FmodBankLoader\" parent=\".\"]", 1, "FmodBankLoader node declaration");
            RequireExactCount(sourceText, "bank_paths = [\"res://banks/desktop/Master.strings.bank\", \"res://banks/desktop/Master.bank\", \"res://banks/desktop/sfx.bank\", \"res://banks/desktop/temp_sfx.bank\", \"res://banks/desktop/ambience.bank\"]", 1, "FmodBankLoader desktop bank_paths property");
            RequireExactCount(sourceText, "[node name=\"FmodListener2D\" type=\"FmodListener2D\" parent=\"AudioManager\"]", 1, "FmodListener2D node declaration");
            RequireExactCount(sourceText, "path=\"res://src/Core/Nodes/NGame.cs\"", 1, "NGame script reference");
            RequireExactCount(sourceText, "path=\"res://scenes/asset_loader.tscn\"", 1, "asset_loader scene reference");
            RequireExactCount(sourceText, "path=\"res://scenes/scene_container.tscn\"", 1, "scene_container scene reference");
            if (sourceText.Contains("SpineSprite", StringComparison.Ordinal) ||
                sourceText.Contains(".gdextension", StringComparison.Ordinal))
            {
                throw new InvalidDataException("Step 37.0.1 sealed game.tscn unexpectedly contains a direct SpineSprite or GDExtension declaration.");
            }

            var staticMap = BuildGameSceneStaticMap(extracted, sourceSha256, sourceMd5, sourceText);
            _gameScenePreflight = new GameScenePreflightSnapshot(
                extracted.Bytes,
                sourceText,
                sourceSha256,
                sourceMd5,
                staticMap);
            _gameSceneDerivative = null;
            _gameScenePackedResource = null;
            RequireStep37BaselineUnchanged(context, "Gate A exit");
            Checkpoint(checkpoint, $"G_A_PASS — sealed game.tscn matched exact authority; bytes={extracted.Bytes.Length}; sha256={sourceSha256}; pckMd5={sourceMd5}; exactly two FMOD custom node declarations + one desktop bank_paths property identified; no scene load performed.");
            return GameScenePass(gate,
                "Closed Step-36.0.5 authority continuity: PASS\n" +
                $"PCK: {baseline.PackAbsolutePath}\n" +
                $"PCK format/engine/flags/files: {ClosedPckFormat} / {ClosedPckEngineMajor}.{ClosedPckEngineMinor}.{ClosedPckEnginePatch} / 0x{ClosedPckFlags:X8} / {ClosedPckDirectoryEntries:N0}\n" +
                $"Sealed scene: {GameSceneResourcePath}\n" +
                $"Scene bytes: {extracted.Bytes.Length:N0}\n" +
                $"Scene SHA-256: {sourceSha256}\n" +
                $"Scene PCK MD5: {sourceMd5}\n" +
                "FMOD compatibility targets: exactly FmodBankLoader + bank_paths + FmodListener2D\n" +
                "Direct Spine/GDExtension declarations in game.tscn: 0\n" +
                "Scene/resource instantiation: NO");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"G_A_FAIL — stage={stage}; {ex.GetType().FullName}: {ex.Message}");
            return GameSceneFail(gate, stage, ex);
        }
    }

    public TransformedRealStS2GameSceneAdmissionGateResult RunGameSceneDerivativePreparation(
        Action<string>? checkpoint = null)
    {
        const TransformedRealStS2GameSceneAdmissionGate gate = TransformedRealStS2GameSceneAdmissionGate.FmodNeutralDerivativePreparation;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep37Prerequisite("Step 37 Gate B entry");
            RequireStep37BaselineUnchanged(context, "Gate B entry");
            var preflight = RequireGameScenePreflight();

            Checkpoint(checkpoint, "G_B_ENTRY — preparing a copied game.tscn derivative; only FmodBankLoader/FmodListener2D node types and the BankLoader bank_paths property may change.");
            stage = "bounded FMOD-neutral text rewrite";
            var derivativeText = preflight.SourceText;
            derivativeText = ReplaceExactlyOnce(
                derivativeText,
                "[node name=\"FmodBankLoader\" type=\"FmodBankLoader\" parent=\".\"]",
                "[node name=\"FmodBankLoader\" type=\"Node\" parent=\".\"]",
                "FmodBankLoader type");
            derivativeText = RemoveExactLineOnce(
                derivativeText,
                "bank_paths = [\"res://banks/desktop/Master.strings.bank\", \"res://banks/desktop/Master.bank\", \"res://banks/desktop/sfx.bank\", \"res://banks/desktop/temp_sfx.bank\", \"res://banks/desktop/ambience.bank\"]",
                "FmodBankLoader bank_paths");
            derivativeText = ReplaceExactlyOnce(
                derivativeText,
                "[node name=\"FmodListener2D\" type=\"FmodListener2D\" parent=\"AudioManager\"]",
                "[node name=\"FmodListener2D\" type=\"Node\" parent=\"AudioManager\"]",
                "FmodListener2D type");

            RequireExactCount(derivativeText, "[node name=\"FmodBankLoader\" type=\"Node\" parent=\".\"]", 1, "neutral FmodBankLoader node");
            RequireExactCount(derivativeText, "[node name=\"FmodListener2D\" type=\"Node\" parent=\"AudioManager\"]", 1, "neutral FmodListener2D node");
            RequireExactCount(derivativeText, "name=\"FmodBankLoader\"", 1, "preserved FmodBankLoader node name");
            RequireExactCount(derivativeText, "name=\"FmodListener2D\"", 1, "preserved FmodListener2D node name");
            if (derivativeText.Contains("type=\"FmodBankLoader\"", StringComparison.Ordinal) ||
                derivativeText.Contains("type=\"FmodListener2D\"", StringComparison.Ordinal) ||
                derivativeText.Contains("bank_paths =", StringComparison.Ordinal))
            {
                throw new InvalidDataException("Step 37.0.1 FMOD-neutral derivative retained a forbidden custom FMOD node type or bank_paths property.");
            }

            foreach (var preserved in new[]
                     {
                         "path=\"res://src/Core/Nodes/NGame.cs\"",
                         "path=\"res://scenes/asset_loader.tscn\"",
                         "path=\"res://scenes/scene_container.tscn\"",
                         "path=\"res://scenes/ui/reaction_wheel.tscn\"",
                         "path=\"res://scenes/ui/multiplayer_timeout_overlay.tscn\"",
                     })
            {
                RequireExactCount(preflight.SourceText, preserved, 1, "source preserved dependency " + preserved);
                RequireExactCount(derivativeText, preserved, 1, "derivative preserved dependency " + preserved);
            }

            stage = "private Step-37 derivative write";
            var workRoot = ResolveChildPath(_launcherDataRoot, Step37WorkRootName, "Step-37 work root");
            Directory.CreateDirectory(workRoot);
            var derivativePath = ResolveChildPath(workRoot, Step37DerivativeFileName, "Step-37 game-scene derivative");
            var derivativeBytes = new UTF8Encoding(false).GetBytes(derivativeText);
            File.WriteAllBytes(derivativePath, derivativeBytes);
            var immediate = File.ReadAllBytes(derivativePath);
            if (!immediate.AsSpan().SequenceEqual(derivativeBytes))
                throw new IOException("Step 37.0.1 derivative immediate readback did not match the bytes just written.");
            var derivativeSha256 = Convert.ToHexString(SHA256.HashData(immediate)).ToLowerInvariant();

            _gameSceneDerivative = new GameSceneDerivativeSnapshot(
                derivativePath,
                immediate.LongLength,
                derivativeSha256,
                derivativeText);
            _gameScenePackedResource = null;
            RequireStep37BaselineUnchanged(context, "Gate B exit");
            Checkpoint(checkpoint, $"G_B_PASS — FMOD-neutral copied scene prepared; path={derivativePath}; bytes={immediate.LongLength}; sha256={derivativeSha256}; edits=3; original PCK/install untouched.");
            return GameScenePass(gate,
                "Copied scene derivative preparation: PASS\n" +
                $"Derivative path: {derivativePath}\n" +
                $"Derivative bytes: {immediate.LongLength:N0}\n" +
                $"Derivative SHA-256: {derivativeSha256}\n" +
                "Authorized edits: 3\n" +
                "  1. FmodBankLoader type -> Node\n" +
                "  2. Remove FmodBankLoader bank_paths property\n" +
                "  3. FmodListener2D type -> Node\n" +
                "FMOD node names/positions preserved: YES\n" +
                "NGame/asset-loader/scene-container/reaction/multiplayer dependencies preserved: YES\n" +
                "Trusted Step-12 install mutated: NO");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"G_B_FAIL — stage={stage}; {ex.GetType().FullName}: {ex.Message}");
            return GameSceneFail(gate, stage, ex);
        }
    }

    public TransformedRealStS2GameSceneAdmissionGateResult RunGameScenePackedLoad(
        Action<string>? checkpoint = null)
    {
        const TransformedRealStS2GameSceneAdmissionGate gate = TransformedRealStS2GameSceneAdmissionGate.PackedSceneLoad;
        var stage = "initialization";
        try
        {
            ThrowIfDisposed();
            var context = RequireStep37Prerequisite("Step 37 Gate C entry");
            var derivative = RequireGameSceneDerivative();
            var handoff = _callbackHandoff ?? throw new InvalidOperationException("Step 37.0.1 requires the exact prepared GodotSharp bridge.");
            var godotAssembly = handoff.GodotSharpAssembly;
            if (!ReferenceEquals(AssemblyLoadContext.GetLoadContext(godotAssembly), context))
                throw new InvalidDataException("Step 37.0.1 GodotSharp left the exact Step-35/36 private context before scene loading.");

            var initializerBefore = context.InitializerBearingRequests.Count;
            var rejectedBefore = context.RejectedManagedRequests.Count;
            var nativeBefore = context.NativeLoadAttempts.Count;
            var resolverBefore = context.ManagedResolverRequests.Count;
            var hostBefore = context.HostLoads.Count;
            var privateBefore = context.PrivateLoads.Count;

            Checkpoint(checkpoint, $"G_C_ENTRY — binding exact GodotSharp ResourceLoader.Load for copied scene; absolutePath={derivative.AbsolutePath}; cacheMode=IGNORE; no instantiation in Gate C.");
            stage = "exact GodotSharp ResourceLoader.Load binding";
            var resourceLoader = godotAssembly.GetType("Godot.ResourceLoader", throwOnError: true, ignoreCase: false)
                ?? throw new MissingMemberException("Godot.ResourceLoader");
            var resourceType = godotAssembly.GetType("Godot.Resource", throwOnError: true, ignoreCase: false)
                ?? throw new MissingMemberException("Godot.Resource");
            var packedSceneType = godotAssembly.GetType("Godot.PackedScene", throwOnError: true, ignoreCase: false)
                ?? throw new MissingMemberException("Godot.PackedScene");
            var load = resourceLoader.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .SingleOrDefault(candidate =>
                {
                    if (!string.Equals(candidate.Name, "Load", StringComparison.Ordinal) ||
                        !resourceType.IsAssignableFrom(candidate.ReturnType))
                        return false;
                    var parameters = candidate.GetParameters();
                    return parameters.Length == 3 &&
                           parameters[0].ParameterType == typeof(string) &&
                           parameters[1].ParameterType == typeof(string) &&
                           parameters[2].ParameterType.IsEnum;
                })
                ?? throw new MissingMethodException("Godot.ResourceLoader", "Load(string,string,CacheMode)");

            stage = "copied PackedScene resource load";
            object? loaded;
            try
            {
                var cacheIgnore = Enum.ToObject(load.GetParameters()[2].ParameterType, 0);
                loaded = load.Invoke(null, new[] { (object)derivative.AbsolutePath, "PackedScene", cacheIgnore });
            }
            catch (TargetInvocationException ex) when (ex.InnerException is not null)
            {
                throw new InvalidOperationException(
                    $"Step 37.0.1 ResourceLoader.Load threw {ex.InnerException.GetType().FullName}: {ex.InnerException.Message}\n{FormatExceptionDiagnostic(ex.InnerException)}",
                    ex.InnerException);
            }

            if (loaded is null || !packedSceneType.IsInstanceOfType(loaded))
                throw new InvalidDataException($"Step 37.0.1 ResourceLoader.Load did not return Godot.PackedScene; observed {loaded?.GetType().FullName ?? "<null>"}.");

            var canInstantiate = packedSceneType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .SingleOrDefault(method => method.Name == "CanInstantiate" && method.GetParameters().Length == 0 && method.ReturnType == typeof(bool))
                ?? throw new MissingMethodException("Godot.PackedScene", "CanInstantiate()");
            if (canInstantiate.Invoke(loaded, null) is not true)
                throw new InvalidDataException("Step 37.0.1 loaded PackedScene reports CanInstantiate=false.");

            RequireNoForbiddenStep37Escape(context, initializerBefore, rejectedBefore, nativeBefore, "Gate C");
            _gameScenePackedResource = new GameScenePackedResourceSnapshot(
                loaded,
                derivative.AbsolutePath,
                context.ManagedResolverRequests.Skip(resolverBefore).ToArray(),
                context.HostLoads.Skip(hostBefore).ToArray(),
                context.PrivateLoads.Skip(privateBefore).ToArray());

            Checkpoint(checkpoint, $"G_C_PASS — copied FMOD-neutral scene loaded as Godot.PackedScene; canInstantiate=True; resolverDelta={context.ManagedResolverRequests.Count - resolverBefore}; hostDelta={context.HostLoads.Count - hostBefore}; privateDelta={context.PrivateLoads.Count - privateBefore}; initializerDelta=0; rejectedDelta=0; nativeDelta=0; scene still uninstantiated.");
            return GameScenePass(gate,
                "Copied game scene ResourceLoader.Load: PASS\n" +
                $"Returned type: {loaded.GetType().FullName}\n" +
                "PackedScene.CanInstantiate: TRUE\n" +
                $"Managed resolver delta: {FormatDelta(context.ManagedResolverRequests.Skip(resolverBefore).ToArray())}\n" +
                $"Host-load delta: {FormatDelta(context.HostLoads.Skip(hostBefore).ToArray())}\n" +
                $"Private-load delta: {FormatDelta(context.PrivateLoads.Skip(privateBefore).ToArray())}\n" +
                "Initializer-bearing request delta: 0\n" +
                "Rejected managed request delta: 0\n" +
                "Native-load attempt delta: 0\n" +
                "Scene instantiated: NO");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"G_C_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return GameSceneFail(gate, stage, ex, includeDiagnostic: true);
        }
    }

    public TransformedRealStS2GameSceneAdmissionGateResult RunGameSceneOffTreeInstantiationAudit(
        Action<string>? checkpoint = null)
    {
        const TransformedRealStS2GameSceneAdmissionGate gate = TransformedRealStS2GameSceneAdmissionGate.OffTreeInstantiationAudit;
        var stage = "initialization";
        object? instance = null;
        try
        {
            ThrowIfDisposed();
            var context = RequireStep37Prerequisite("Step 37 Gate D entry");
            var packed = RequireGameScenePackedResource();
            var handoff = _callbackHandoff ?? throw new InvalidOperationException("Step 37.0.1 exact GodotSharp handoff disappeared.");
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

            Checkpoint(checkpoint, "G_D_ENTRY — instantiating copied PackedScene off-tree with GenEditState.Disabled. This may construct nodes/scripts and child scenes, but the launcher will not AddChild it to any SceneTree.");
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
            try
            {
                var disabled = Enum.ToObject(instantiate.GetParameters()[0].ParameterType, 0);
                instance = instantiate.Invoke(packed.Resource, new[] { disabled });
            }
            catch (TargetInvocationException ex) when (ex.InnerException is not null)
            {
                throw new InvalidOperationException(
                    $"Step 37.0.1 PackedScene.Instantiate threw {ex.InnerException.GetType().FullName}: {ex.InnerException.Message}\n{FormatExceptionDiagnostic(ex.InnerException)}",
                    ex.InnerException);
            }

            if (instance is null || !nodeType.IsInstanceOfType(instance))
                throw new InvalidDataException($"Step 37.0.1 PackedScene.Instantiate did not return Godot.Node; observed {instance?.GetType().FullName ?? "<null>"}.");

            stage = "off-tree root identity and hierarchy audit";
            var rootType = instance.GetType().FullName ?? "<unknown>";
            if (!string.Equals(rootType, "MegaCrit.Sts2.Core.Nodes.NGame", StringComparison.Ordinal))
                throw new InvalidDataException($"Step 37.0.1 expected managed root type MegaCrit.Sts2.Core.Nodes.NGame; observed {rootType}.");
            if (!ReferenceEquals(AssemblyLoadContext.GetLoadContext(instance.GetType().Assembly), context))
                throw new InvalidDataException("Step 37.0.1 NGame instance is not owned by the exact Step-35/36 private load context.");

            var isInsideTree = instance.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .SingleOrDefault(method => method.Name == "IsInsideTree" && method.GetParameters().Length == 0 && method.ReturnType == typeof(bool))
                ?? throw new MissingMethodException(rootType, "IsInsideTree()");
            if (isInsideTree.Invoke(instance, null) is not false)
                throw new InvalidDataException("Step 37.0.1 off-tree NGame instance unexpectedly reports IsInsideTree=true.");

            var children = new[]
            {
                ("FmodBankLoader", "Godot.Node"),
                ("AudioManager", "MegaCrit.Sts2.Core.Nodes.Audio.NAudioManager"),
                ("DebugAudioManager", "MegaCrit.Sts2.Core.Audio.Debug.NDebugAudioManager"),
                ("RootSceneContainer", "MegaCrit.Sts2.Core.Nodes.NSceneContainer"),
                ("AssetLoader", "MegaCrit.Sts2.Core.Nodes.NAssetLoader"),
                ("FmodListener2D", "Godot.Node"),
            };
            var findChild = instance.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .SingleOrDefault(method =>
                {
                    if (method.Name != "FindChild")
                        return false;
                    var parameters = method.GetParameters();
                    return parameters.Length == 3 &&
                           parameters[0].ParameterType == typeof(string) &&
                           parameters[1].ParameterType == typeof(bool) &&
                           parameters[2].ParameterType == typeof(bool);
                })
                ?? throw new MissingMethodException(rootType, "FindChild(string,bool,bool)");

            var observedChildren = new List<string>();
            foreach (var (name, expectedType) in children)
            {
                var child = findChild.Invoke(instance, new object?[] { name, true, false })
                    ?? throw new InvalidDataException($"Step 37.0.1 off-tree NGame did not contain expected child {name}.");
                var childType = child.GetType().FullName ?? "<unknown>";
                observedChildren.Add($"{name}={childType}");
                if (!string.Equals(childType, expectedType, StringComparison.Ordinal))
                    throw new InvalidDataException($"Step 37.0.1 child {name} type drifted: expected {expectedType}; observed {childType}.");
            }

            RequireNoForbiddenStep37Escape(context, initializerBefore, rejectedBefore, nativeBefore, "Gate D");
            var state = ReadOneTimeInitializationState(RequireEssentialBinding().StateField);
            if (state != ExpectedStateAfterEssential)
                throw new InvalidDataException($"Step 37.0.1 OneTimeInitialization state drifted during off-tree instantiation: expected {ExpectedStateAfterEssential}; observed {state}.");
            MarkExactStep37ClosurePassed();

            Checkpoint(checkpoint, $"G_D_STEP38_AUTHORITY_READY — same-process Step-37.0.1 4/4 authority marked for the next bounded NGame lifecycle-entry experiment.");
            Checkpoint(checkpoint, $"G_D_PASS — off-tree NGame instantiated; rootType={rootType}; insideTree=False; children={string.Join(",", observedChildren)}; resolverDelta={context.ManagedResolverRequests.Count - resolverBefore}; hostDelta={context.HostLoads.Count - hostBefore}; privateDelta={context.PrivateLoads.Count - privateBefore}; initializerDelta=0; rejectedDelta=0; nativeDelta=0; state={state}; AddChild/GameStartup/main-menu/ExecuteDeferred not invoked.");
            return GameScenePass(gate,
                "STEP 37.0.1 OFF-TREE GAME-SCENE INSTANTIATION PASSED.\n" +
                $"Root managed type: {rootType}\n" +
                "Root inside SceneTree: FALSE\n" +
                $"Expected hierarchy: {string.Join(" | ", observedChildren)}\n" +
                $"OneTimeInitialization state: {state}\n" +
                $"Managed resolver delta: {FormatDelta(context.ManagedResolverRequests.Skip(resolverBefore).ToArray())}\n" +
                $"Host-load delta: {FormatDelta(context.HostLoads.Skip(hostBefore).ToArray())}\n" +
                $"Private-load delta: {FormatDelta(context.PrivateLoads.Skip(privateBefore).ToArray())}\n" +
                "Initializer-bearing request delta: 0\n" +
                "Rejected managed request delta: 0\n" +
                "Native-load attempt delta: 0\n" +
                "SceneTree AddChild: NO\n" +
                "NGame.GameStartup / LaunchMainMenu / ExecuteDeferred: NO\n" +
                "Steam/native GDExtension initialization by launcher: NO");
        }
        catch (Exception ex)
        {
            Checkpoint(checkpoint, $"G_D_FAIL — stage={stage}; {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
            return GameSceneFail(gate, stage, ex, includeDiagnostic: true);
        }
        finally
        {
            if (instance is not null)
            {
                try
                {
                    var free = instance.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                        .SingleOrDefault(method => method.Name == "Free" && method.GetParameters().Length == 0);
                    if (free is not null)
                        free.Invoke(instance, null);
                    else if (instance is IDisposable disposable)
                        disposable.Dispose();
                    Checkpoint(checkpoint, "G_D_OFFTREE_INSTANCE_RELEASED — temporary off-tree game-scene instance released after audit.");
                }
                catch (Exception ex)
                {
                    Checkpoint(checkpoint, $"G_D_OFFTREE_RELEASE_WARNING — {ex.GetType().FullName}: {SanitizeCheckpoint(ex.Message)}");
                }
            }
        }
    }

    private Step35ExecutionLoadContext RequireStep37Prerequisite(string boundary)
    {
        if (!_exactStep36ClosurePassed || _step37Baseline is null)
            throw new InvalidOperationException($"{boundary} requires a successful Step-36.0.5 4/4 closure in this same process.");
        if (!IsModelBootstrapCompatibilityMode)
            throw new InvalidOperationException($"{boundary} requires the physically closed GodotCoreModelBootstrapCompatibility authority.");
        if (_callbackHandoff is null || !_managedPluginReverseBridgePrepared)
            throw new InvalidOperationException($"{boundary} requires the physically proven Godot managed/native bridge.");
        var context = RequireLoadContext();
        var state = ReadOneTimeInitializationState(RequireEssentialBinding().StateField);
        if (state != ExpectedStateAfterEssential)
            throw new InvalidOperationException($"{boundary} requires OneTimeInitialization state {ExpectedStateAfterEssential}; observed {state}.");
        return context;
    }

    private Step37BaselineSnapshot RequireStep37Baseline()
        => _step37Baseline ?? throw new InvalidOperationException("Step 37.0.1 baseline is absent.");

    private GameScenePreflightSnapshot RequireGameScenePreflight()
        => _gameScenePreflight ?? throw new InvalidOperationException("Step 37.0.1 Gate A must pass before Gate B.");

    private GameSceneDerivativeSnapshot RequireGameSceneDerivative()
        => _gameSceneDerivative ?? throw new InvalidOperationException("Step 37.0.1 Gate B must pass before Gate C.");

    private GameScenePackedResourceSnapshot RequireGameScenePackedResource()
        => _gameScenePackedResource ?? throw new InvalidOperationException("Step 37.0.1 Gate C must pass before Gate D.");

    private void RequireStep37BaselineUnchanged(Step35ExecutionLoadContext context, string boundary)
    {
        var baseline = RequireStep37Baseline();
        if (!SequenceEqual(baseline.ManagedResolverRequests, context.ManagedResolverRequests) ||
            !SequenceEqual(baseline.HostLoads, context.HostLoads) ||
            !SequenceEqual(baseline.PrivateLoads, context.PrivateLoads) ||
            context.InitializerBearingRequests.Count != 0 ||
            context.RejectedManagedRequests.Count != 0 ||
            context.NativeLoadAttempts.Count != 0)
        {
            throw new InvalidDataException($"Step 37.0.1 Step-36 closure baseline changed before authorized scene work at {boundary}. {context.FormatResolverState()}");
        }

        VerifyFileLength(baseline.PackAbsolutePath, baseline.PackLength, "Step-37 receipt-backed game PCK");
    }

    private static void RequireNoForbiddenStep37Escape(
        Step35ExecutionLoadContext context,
        int initializerBefore,
        int rejectedBefore,
        int nativeBefore,
        string boundary)
    {
        if (context.InitializerBearingRequests.Count != initializerBefore ||
            context.RejectedManagedRequests.Count != rejectedBefore ||
            context.NativeLoadAttempts.Count != nativeBefore)
        {
            throw new InvalidDataException($"{boundary} crossed a forbidden initializer-bearing/rejected/native boundary. {context.FormatResolverState()}");
        }
    }

    private static ExtractedPckEntry ExtractSealedGameSceneFromPck(string pckPath)
    {
        using var stream = new FileStream(pckPath, FileMode.Open, FileAccess.Read, FileShare.Read, 16 * 1024, FileOptions.SequentialScan);
        using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);
        var magic = reader.ReadUInt32();
        if (magic != 0x43504447)
            throw new InvalidDataException($"Step 37.0.1 expected standalone Godot PCK magic 0x43504447; observed 0x{magic:X8}.");
        var format = reader.ReadUInt32();
        var major = reader.ReadUInt32();
        var minor = reader.ReadUInt32();
        var patch = reader.ReadUInt32();
        var flags = reader.ReadUInt32();
        var fileBase = reader.ReadUInt64();
        if (format != ClosedPckFormat || major != ClosedPckEngineMajor || minor != ClosedPckEngineMinor || patch != ClosedPckEnginePatch || flags != ClosedPckFlags)
            throw new InvalidDataException($"Step 37.0.1 PCK header drifted: format={format}; engine={major}.{minor}.{patch}; flags=0x{flags:X8}.");

        var directoryOffset = reader.ReadUInt64();
        if (directoryOffset >= (ulong)stream.Length)
            throw new InvalidDataException($"Step 37.0.1 PCK directory offset {directoryOffset} exceeds file length {stream.Length}.");
        stream.Seek(checked((long)directoryOffset), SeekOrigin.Begin);
        var count = reader.ReadUInt32();
        if (count != ClosedPckDirectoryEntries)
            throw new InvalidDataException($"Step 37.0.1 PCK directory entry count drifted: expected {ClosedPckDirectoryEntries}; observed {count}.");

        PckDirectoryEntry? match = null;
        for (var i = 0u; i < count; i++)
        {
            var pathLength = reader.ReadUInt32();
            if (pathLength == 0 || pathLength > 1_048_576)
                throw new InvalidDataException($"Step 37.0.1 PCK directory path length is invalid at entry {i}: {pathLength}.");
            var pathBytes = ReadExactlyStep37(reader, checked((int)pathLength));
            var storedPath = Encoding.UTF8.GetString(pathBytes).TrimEnd('\0').Replace('\\', '/');
            var offset = reader.ReadUInt64();
            var size = reader.ReadUInt64();
            var md5 = ReadExactlyStep37(reader, 16);
            var entryFlags = reader.ReadUInt32();
            var normalized = storedPath.StartsWith("res://", StringComparison.Ordinal) ? storedPath : "res://" + storedPath.TrimStart('/');
            if (string.Equals(normalized, GameSceneResourcePath, StringComparison.Ordinal))
            {
                if (match is not null)
                    throw new InvalidDataException($"Step 37.0.1 found duplicate PCK entries for {GameSceneResourcePath}.");
                match = new PckDirectoryEntry(normalized, offset, size, md5, entryFlags);
            }
        }

        var entry = match ?? throw new FileNotFoundException($"Step 37.0.1 did not find {GameSceneResourcePath} in the exact receipt-backed PCK.");
        if ((entry.Flags & 1u) != 0)
            throw new InvalidDataException("Step 37.0.1 sealed game.tscn PCK entry is encrypted; refusing to treat ciphertext as scene authority.");
        if (entry.Size != ClosedGameSceneBytes)
            throw new InvalidDataException($"Step 37.0.1 game.tscn PCK entry size drifted: expected {ClosedGameSceneBytes}; observed {entry.Size}.");

        var absoluteOffset = checked(fileBase + entry.Offset);
        if (absoluteOffset + entry.Size > (ulong)stream.Length)
            throw new InvalidDataException("Step 37.0.1 game.tscn PCK entry points beyond the receipt-backed PCK.");
        stream.Seek(checked((long)absoluteOffset), SeekOrigin.Begin);
        var bytes = ReadExactlyStep37(reader, checked((int)entry.Size));
        var pckMd5 = Convert.ToHexString(entry.Md5).ToLowerInvariant();
        var actualMd5 = Convert.ToHexString(MD5.HashData(bytes)).ToLowerInvariant();
        if (!pckMd5.Equals(actualMd5, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException($"Step 37.0.1 game.tscn PCK MD5 verification failed: directory={pckMd5}; actual={actualMd5}.");
        return new ExtractedPckEntry(bytes, entry.Offset, entry.Size, pckMd5, entry.Flags, directoryOffset, fileBase, count);
    }

    private static byte[] ReadExactlyStep37(BinaryReader reader, int count)
    {
        var bytes = reader.ReadBytes(count);
        if (bytes.Length != count)
            throw new EndOfStreamException($"Step 37.0.1 unexpected end of PCK while reading {count} bytes.");
        return bytes;
    }

    private static string BuildGameSceneStaticMap(
        ExtractedPckEntry extracted,
        string sha256,
        string md5,
        string sourceText)
    {
        var nodeLines = sourceText.Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n')
            .Split('\n')
            .Where(line => line.StartsWith("[node ", StringComparison.Ordinal) ||
                           line.StartsWith("[ext_resource ", StringComparison.Ordinal))
            .ToArray();
        return
            "StS2 Launcher — Step 37.0.1 sealed game-scene static map\n" +
            "Read-only evidence from exact receipt-backed PCK bytes; never consumed as trusted runtime input.\n" +
            $"Resource: {GameSceneResourcePath}\n" +
            $"Bytes: {extracted.Bytes.Length}\n" +
            $"SHA-256: {sha256}\n" +
            $"PCK MD5: {md5}\n" +
            $"PCK directory offset: {extracted.DirectoryOffset}\n" +
            $"PCK file base: {extracted.FileBase}\n" +
            $"PCK directory entries: {extracted.DirectoryCount}\n" +
            "Authorized compatibility edits: FmodBankLoader type->Node; remove bank_paths; FmodListener2D type->Node.\n" +
            "Forbidden in Step 37: SceneTree AddChild, NGame.GameStartup, LaunchMainMenu, ExecuteDeferred, Steam init, native GDExtension loading.\n\n" +
            "[EXT_RESOURCE / NODE MAP]\n" +
            string.Join("\n", nodeLines) + "\n";
    }

    private static void RequireExactCount(string text, string value, int expected, string label)
    {
        var count = 0;
        var index = 0;
        while ((index = text.IndexOf(value, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += value.Length;
        }
        if (count != expected)
            throw new InvalidDataException($"Step 37.0.1 expected {expected} occurrence(s) of {label}; observed {count}.");
    }

    private static string ReplaceExactlyOnce(string text, string oldValue, string newValue, string label)
    {
        RequireExactCount(text, oldValue, 1, label);
        return text.Replace(oldValue, newValue, StringComparison.Ordinal);
    }

    private static string RemoveExactLineOnce(string text, string line, string label)
    {
        var normalized = text.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');
        RequireExactCount(normalized, line, 1, label);
        var withLf = line + "\n";
        if (normalized.Contains(withLf, StringComparison.Ordinal))
            return normalized.Replace(withLf, string.Empty, StringComparison.Ordinal);
        if (normalized.EndsWith(line, StringComparison.Ordinal))
            return normalized[..^line.Length];
        throw new InvalidDataException($"Step 37.0.1 could not remove the exact {label} line.");
    }

    private static TransformedRealStS2GameSceneAdmissionGateResult GameScenePass(
        TransformedRealStS2GameSceneAdmissionGate gate,
        string detail)
        => new(gate, true, detail);

    private static TransformedRealStS2GameSceneAdmissionGateResult GameSceneFail(
        TransformedRealStS2GameSceneAdmissionGate gate,
        string stage,
        Exception ex,
        bool includeDiagnostic = false)
        => new(
            gate,
            false,
            $"Stage: {stage}\n{ex.GetType().Name}: {ex.Message}" +
            (includeDiagnostic ? "\n\n" + FormatExceptionDiagnostic(ex) : string.Empty));

    private sealed record Step37BaselineSnapshot(
        string[] ManagedResolverRequests,
        string[] HostLoads,
        string[] PrivateLoads,
        string ManagedInstallRoot,
        string PackAbsolutePath,
        long PackLength,
        string ReceiptSha1);

    private sealed record PckDirectoryEntry(
        string ResourcePath,
        ulong Offset,
        ulong Size,
        byte[] Md5,
        uint Flags);

    private sealed record ExtractedPckEntry(
        byte[] Bytes,
        ulong Offset,
        ulong Size,
        string PckMd5,
        uint Flags,
        ulong DirectoryOffset,
        ulong FileBase,
        uint DirectoryCount);

    private sealed record GameScenePreflightSnapshot(
        byte[] SourceBytes,
        string SourceText,
        string SourceSha256,
        string PckMd5,
        string StaticMap);

    private sealed record GameSceneDerivativeSnapshot(
        string AbsolutePath,
        long Length,
        string Sha256,
        string Text);

    private sealed record GameScenePackedResourceSnapshot(
        object Resource,
        string AbsolutePath,
        string[] ResolverDelta,
        string[] HostLoadDelta,
        string[] PrivateLoadDelta);
}
