using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CecilCustomAttributeProvider = Mono.Cecil.ICustomAttributeProvider;
using StS2Launcher.Core;

namespace StS2Launcher.Core.Tests;

[TestClass]
[DoNotParallelize]
public sealed class ControlledHarmonyPatchExecutionTests
{
    [TestMethod]
    public void OrderedHarmonyPatchExecutionGatesReachTwentySixOfTwentySixPass()
    {
        var gates = new ControlledHarmonyPatchExecutionGateSequence();
        gates.Record(new ControlledHarmonyPatchExecutionGateResult(ControlledHarmonyPatchExecutionGate.InitializationPreflight, true, "preflight"));
        gates.Record(new ControlledHarmonyPatchExecutionGateResult(ControlledHarmonyPatchExecutionGate.ProvenLoadStateReplay, true, "replay"));
        gates.Record(new ControlledHarmonyPatchExecutionGateResult(ControlledHarmonyPatchExecutionGate.DeferredModuleInitialization, true, "initialize"));
        gates.Record(new ControlledHarmonyPatchExecutionGateResult(ControlledHarmonyPatchExecutionGate.ProvenInitializationAudit, true, "audit"));
        gates.Record(new ControlledHarmonyPatchExecutionGateResult(ControlledHarmonyPatchExecutionGate.HarmonyApiResolution, true, "resolve"));
        gates.Record(new ControlledHarmonyPatchExecutionGateResult(ControlledHarmonyPatchExecutionGate.HarmonyTypeInitialization, true, "type initialize"));
        gates.Record(new ControlledHarmonyPatchExecutionGateResult(ControlledHarmonyPatchExecutionGate.HarmonyTypeInitializationAudit, true, "type-init audit"));
        gates.Record(new ControlledHarmonyPatchExecutionGateResult(ControlledHarmonyPatchExecutionGate.HarmonyInstanceConstruction, true, "construct"));
        gates.Record(new ControlledHarmonyPatchExecutionGateResult(ControlledHarmonyPatchExecutionGate.PostConstructionAudit, true, "Step 25 replay audit"));
        gates.Record(new ControlledHarmonyPatchExecutionGateResult(ControlledHarmonyPatchExecutionGate.HarmonyProcessorApiResolution, true, "processor API"));
        gates.Record(new ControlledHarmonyPatchExecutionGateResult(ControlledHarmonyPatchExecutionGate.PatchProcessorTypeInitialization, true, "processor type init"));
        gates.Record(new ControlledHarmonyPatchExecutionGateResult(ControlledHarmonyPatchExecutionGate.LauncherProbeResolution, true, "launcher processor probe"));
        gates.Record(new ControlledHarmonyPatchExecutionGateResult(ControlledHarmonyPatchExecutionGate.HarmonyProcessorCreation, true, "processor create"));
        gates.Record(new ControlledHarmonyPatchExecutionGateResult(ControlledHarmonyPatchExecutionGate.PostProcessorAudit, true, "processor audit"));
        gates.Record(new ControlledHarmonyPatchExecutionGateResult(ControlledHarmonyPatchExecutionGate.HarmonyPatchApiResolution, true, "patch API"));
        gates.Record(new ControlledHarmonyPatchExecutionGateResult(ControlledHarmonyPatchExecutionGate.LauncherPatchProbeResolution, true, "patch probe"));
        gates.Record(new ControlledHarmonyPatchExecutionGateResult(ControlledHarmonyPatchExecutionGate.BaselineProbeInvocation, true, "baseline"));
        gates.Record(new ControlledHarmonyPatchExecutionGateResult(ControlledHarmonyPatchExecutionGate.AccessToolsTypeInitialization, true, "AccessTools type init"));
        gates.Record(new ControlledHarmonyPatchExecutionGateResult(ControlledHarmonyPatchExecutionGate.PrefixRegistration, true, "prefix registration"));
        gates.Record(new ControlledHarmonyPatchExecutionGateResult(ControlledHarmonyPatchExecutionGate.PatchEngineExecution, true, "patch"));
        gates.Record(new ControlledHarmonyPatchExecutionGateResult(ControlledHarmonyPatchExecutionGate.PostPatchAudit, true, "post-patch audit"));
        gates.Record(new ControlledHarmonyPatchExecutionGateResult(ControlledHarmonyPatchExecutionGate.PatchedProbeInvocation, true, "patched invocation"));
        gates.Record(new ControlledHarmonyPatchExecutionGateResult(ControlledHarmonyPatchExecutionGate.ExactPrefixUnpatch, true, "unpatch"));
        gates.Record(new ControlledHarmonyPatchExecutionGateResult(ControlledHarmonyPatchExecutionGate.PostUnpatchAudit, true, "post-unpatch audit"));
        gates.Record(new ControlledHarmonyPatchExecutionGateResult(ControlledHarmonyPatchExecutionGate.RestoredProbeInvocation, true, "restored invocation"));
        gates.Record(new ControlledHarmonyPatchExecutionGateResult(ControlledHarmonyPatchExecutionGate.FinalIsolationAudit, true, "final audit"));

        var summary = gates.Snapshot();
        Assert.IsTrue(summary.Passed);
        Assert.AreEqual(26, summary.PassedGates);
        Assert.AreEqual("CONTROLLED LAUNCHER-OWNED HARMONY PATCH EXECUTION BOUNDARY PASS — 26/26", summary.Summary);
    }

    [TestMethod]
    public void AccessToolsMetadataAuditAcceptsOnlyExactMeasuredRuntimeDetectionCacheInitializer()
    {
        using var temp = new TempTestDirectory("sts2-step27-accesstools-metadata");
        var goodPath = Path.Combine(temp.Path, "AccessTools-good.dll");
        var driftPath = Path.Combine(temp.Path, "AccessTools-drift.dll");
        var wrongThrowOnErrorPath = Path.Combine(temp.Path, "AccessTools-wrong-throw-on-error.dll");
        var wrongLockRecursionPath = Path.Combine(temp.Path, "AccessTools-wrong-lock-recursion.dll");
        WriteAccessToolsFixture(goodPath, drift: false);
        WriteAccessToolsFixture(driftPath, drift: true);
        WriteAccessToolsFixture(wrongThrowOnErrorPath, drift: false, wrongThrowOnError: true);
        WriteAccessToolsFixture(wrongLockRecursionPath, drift: false, wrongLockRecursion: true);

        var audit = typeof(ControlledHarmonyPatchExecution).GetMethod("ReadAccessToolsMetadata", BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new AssertFailedException("Step 27 AccessTools metadata audit helper is missing.");
        var good = audit.Invoke(null, [goodPath]) ?? throw new AssertFailedException("Good AccessTools audit returned null.");
        var drift = audit.Invoke(null, [driftPath]) ?? throw new AssertFailedException("Drift AccessTools audit returned null.");
        var wrongThrowOnError = audit.Invoke(null, [wrongThrowOnErrorPath]) ?? throw new AssertFailedException("Wrong-throwOnError AccessTools audit returned null.");
        var wrongLockRecursion = audit.Invoke(null, [wrongLockRecursionPath]) ?? throw new AssertFailedException("Wrong-lock-recursion AccessTools audit returned null.");
        var allowedProperty = good.GetType().GetProperty("Allowed") ?? throw new AssertFailedException("AccessTools audit result has no Allowed property.");
        var detailProperty = good.GetType().GetProperty("Detail") ?? throw new AssertFailedException("AccessTools audit result has no Detail property.");
        Assert.AreEqual(true, allowedProperty.GetValue(good));
        Assert.AreEqual(false, allowedProperty.GetValue(drift));
        Assert.AreEqual(false, allowedProperty.GetValue(wrongThrowOnError));
        Assert.AreEqual(false, allowedProperty.GetValue(wrongLockRecursion));
        StringAssert.Contains((string?)detailProperty.GetValue(good) ?? string.Empty, "Exact Step 27.0.2 physical AccessTools initializer fingerprint: MATCH");
        StringAssert.Contains((string?)detailProperty.GetValue(drift) ?? string.Empty, "Blocking AccessTools initializer hazards:");
        StringAssert.Contains((string?)detailProperty.GetValue(wrongThrowOnError) ?? string.Empty, "expected false then false");
        StringAssert.Contains((string?)detailProperty.GetValue(wrongLockRecursion) ?? string.Empty, "expected SupportsRecursion (1)");
    }

    [TestMethod]
    public void OfficialHarmony242Net9FatNormalizerUsesDeferredMetadataAndPreservesSourceBytes()
    {
        var fixturePath = Environment.GetEnvironmentVariable("STS2_STEP27_REAL_HARMONY_FIXTURE");
        Assert.IsFalse(string.IsNullOrWhiteSpace(fixturePath),
            "STS2_STEP27_REAL_HARMONY_FIXTURE must point to the quarantined official Harmony-Fat 2.4.2 net9.0/0Harmony.dll structural-surrogate fixture.");
        fixturePath = Path.GetFullPath(fixturePath!);
        Assert.IsTrue(File.Exists(fixturePath),
            $"Exact official Harmony-Fat 2.4.2 net9.0 structural-surrogate fixture is missing: {fixturePath}");

        var sourceBytesBefore = File.ReadAllBytes(fixturePath);
        var sourceSha1Before = Convert.ToHexString(SHA1.HashData(sourceBytesBefore)).ToLowerInvariant();

        using (var module = ModuleDefinition.ReadModule(fixturePath, new ReaderParameters
        {
            InMemory = true,
            ReadSymbols = false,
            ReadingMode = ReadingMode.Deferred,
        }))
        {
            Assert.AreEqual("0Harmony", module.Assembly.Name.Name);
            Assert.AreEqual(new Version(2, 4, 2, 0), module.Assembly.Name.Version);
            Assert.IsFalse(module.AssemblyReferences.Any(reference => reference.Name == "netstandard"),
                "The official Harmony-Fat 2.4.2 host structural surrogate must be the net9.0 implementation, not a netstandard reference surface.");
            var systemRuntimeReference = module.AssemblyReferences.SingleOrDefault(reference => reference.Name == "System.Runtime");
            Assert.IsNotNull(systemRuntimeReference, "The official net9.0 Harmony-Fat structural surrogate must reference System.Runtime.");
            Assert.AreEqual(new Version(9, 0, 0, 0), systemRuntimeReference.Version);
            Assert.IsTrue(HasEditorBrowsableAttributeSurface(module),
                "The real-Harmony regression fixture must retain the EditorBrowsable custom-attribute surface that exposed the 0.0.97 Immediate-reader bug.");
        }

        var normalize = typeof(ControlledHarmonyPatchExecution).GetMethod(
            "CreateIosNormalizedHarmonyRuntimeImage",
            BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new AssertFailedException("Step 27 real-Harmony normalizer helper is missing.");

        object snapshot;
        try
        {
            snapshot = normalize.Invoke(null, [fixturePath])
                ?? throw new AssertFailedException("Real-Harmony normalizer returned null.");
        }
        catch (TargetInvocationException ex) when (ex.InnerException is not null)
        {
            throw new AssertFailedException("Real Harmony 2.4.2 normalization failed: " + ex.InnerException, ex.InnerException);
        }

        var snapshotType = snapshot.GetType();
        string ReadString(string name) => (string?)(snapshotType.GetProperty(name)?.GetValue(snapshot))
            ?? throw new AssertFailedException($"Normalizer snapshot is missing string property {name}.");
        var runtimeBytes = (byte[]?)(snapshotType.GetProperty("RuntimeImageBytes")?.GetValue(snapshot))
            ?? throw new AssertFailedException("Normalizer snapshot is missing RuntimeImageBytes.");

        Assert.AreEqual(sourceSha1Before, ReadString("SourcePreparedSha1"));
        Assert.AreNotEqual(sourceSha1Before, ReadString("RuntimeImageSha1"));
        Assert.IsTrue(runtimeBytes.Length > 0);
        StringAssert.Contains(ReadString("NormalizedTypeInitializerAudit"), "instructions=11");
        CollectionAssert.AreEqual(sourceBytesBefore, File.ReadAllBytes(fixturePath),
            "The real Harmony fixture must remain byte-for-byte immutable after in-memory normalization.");
    }

    private static bool HasEditorBrowsableAttributeSurface(ModuleDefinition module)
    {
        const string attributeName = "System.ComponentModel.EditorBrowsableAttribute";
        static bool HasAttribute(CecilCustomAttributeProvider provider, string fullName)
            => provider.HasCustomAttributes && provider.CustomAttributes.Any(attribute => attribute.AttributeType.FullName == fullName);

        if (HasAttribute(module, attributeName) || (module.Assembly is not null && HasAttribute(module.Assembly, attributeName)))
            return true;

        foreach (var type in EnumerateFixtureTypes(module.Types))
        {
            if (HasAttribute(type, attributeName) ||
                type.Fields.Any(field => HasAttribute(field, attributeName)) ||
                type.Properties.Any(property => HasAttribute(property, attributeName)) ||
                type.Events.Any(@event => HasAttribute(@event, attributeName)))
                return true;

            foreach (var method in type.Methods)
            {
                if (HasAttribute(method, attributeName) || HasAttribute(method.MethodReturnType, attributeName) ||
                    method.Parameters.Any(parameter => HasAttribute(parameter, attributeName)))
                    return true;
            }
        }

        return false;
    }

    private static IEnumerable<TypeDefinition> EnumerateFixtureTypes(IEnumerable<TypeDefinition> roots)
    {
        foreach (var root in roots)
        {
            yield return root;
            foreach (var nested in EnumerateFixtureTypes(root.NestedTypes))
                yield return nested;
        }
    }

    [TestMethod]
    public void HarmonyPatchEngineMetadataAuditFailsClosedWhenSharedStateReplacementOrDetourChainIsMissing()
    {
        using var temp = new TempTestDirectory("sts2-step27-patch-engine-metadata");
        var path = Path.Combine(temp.Path, "PatchEngine-incomplete.dll");
        WriteAccessToolsFixture(path, drift: false);

        var audit = typeof(ControlledHarmonyPatchExecution).GetMethod("ReadHarmonyPatchEngineMetadata", BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new AssertFailedException("Step 27 patch-engine metadata audit helper is missing.");
        var result = audit.Invoke(null, [path]) ?? throw new AssertFailedException("Patch-engine metadata audit returned null.");
        var allowedProperty = result.GetType().GetProperty("Allowed") ?? throw new AssertFailedException("Patch-engine audit result has no Allowed property.");
        var detailProperty = result.GetType().GetProperty("Detail") ?? throw new AssertFailedException("Patch-engine audit result has no Detail property.");

        Assert.AreEqual(false, allowedProperty.GetValue(result));
        StringAssert.Contains((string?)detailProperty.GetValue(result) ?? string.Empty, "patch-engine internal types are missing");
    }

    [TestMethod]
    public void HarmonyPatchExecutionGatesStopAfterFirstFailure()
    {
        var gates = new ControlledHarmonyPatchExecutionGateSequence();
        gates.Record(new ControlledHarmonyPatchExecutionGateResult(ControlledHarmonyPatchExecutionGate.InitializationPreflight, true, "preflight"));
        gates.Record(new ControlledHarmonyPatchExecutionGateResult(ControlledHarmonyPatchExecutionGate.ProvenLoadStateReplay, false, "replay failed"));
        Assert.ThrowsExactly<InvalidOperationException>(() =>
            gates.Record(new ControlledHarmonyPatchExecutionGateResult(ControlledHarmonyPatchExecutionGate.DeferredModuleInitialization, true, "must not advance")));
        Assert.AreEqual(ControlledHarmonyPatchExecutionGate.ProvenLoadStateReplay, gates.Snapshot().FirstFailingGate);
    }

    [TestMethod]
    public async Task SyntheticStep26ReplayThroughEmptyProcessorStillPassesBeforePatchBoundary()
    {
        var names = CreateNames();
        using var temp = new TempTestDirectory("sts2-step27-tests");
        await CreateSyntheticPreparedRuntimeAsync(temp.Path, names, InitializerKind.Return);

        using var boundary = CreateSyntheticBoundary(temp.Path, names);
        var gateA = await boundary.RunInitializationPreflightAsync();
        Assert.IsTrue(gateA.Passed, gateA.Detail);
        StringAssert.Contains(gateA.Detail, "Initializer hazards: 0");
        StringAssert.Contains(gateA.Detail, "IL_0000: Ret");
        StringAssert.Contains(gateA.Detail, "HarmonySharedState iOS runtime-image normalization: NOT APPLICABLE — internal synthetic target replay");
        StringAssert.Contains(gateA.Detail, "original fixture bytes retained exactly; production normalization policy not bypassed");

        var gateB = boundary.RunProvenLoadStateReplay();
        Assert.IsTrue(gateB.Passed, gateB.Detail);
        StringAssert.Contains(gateB.Detail, "0Harmony loaded: NO");

        var gateC = boundary.RunDeferredModuleInitialization();
        Assert.IsTrue(gateC.Passed, gateC.Detail);
        StringAssert.Contains(gateC.Detail, "RuntimeHelpers.RunModuleConstructor completion barrier: PASS");
        StringAssert.Contains(gateC.Detail, "Native load attempts during target load/initializer: 0");

        var gateD = await boundary.RunProvenInitializationAuditAsync();
        Assert.IsTrue(gateD.Passed, gateD.Detail);
        StringAssert.Contains(gateD.Detail, "Native load attempts: 0");

        var gateE = boundary.RunHarmonyApiResolution();
        Assert.IsTrue(gateE.Passed, gateE.Detail);
        StringAssert.Contains(gateE.Detail, "Harmony type initializer executed by Step 27: NO");
        StringAssert.Contains(gateE.Detail, "Harmony object constructed: NO");

        var gateF = boundary.RunHarmonyTypeInitialization();
        Assert.IsTrue(gateF.Passed, gateF.Detail);
        StringAssert.Contains(gateF.Detail, "RuntimeHelpers.RunClassConstructor(HarmonyLib.Harmony.TypeHandle) = PASS");
        StringAssert.Contains(gateF.Detail, "Harmony object constructed: NO");

        var gateG = boundary.RunHarmonyTypeInitializationAudit();
        Assert.IsTrue(gateG.Passed, gateG.Detail);
        StringAssert.Contains(gateG.Detail, "HARMONY TYPE-INITIALIZATION AUDIT PASSED");

        var gateH = boundary.RunHarmonyInstanceConstruction();
        Assert.IsTrue(gateH.Passed, gateH.Detail);
        StringAssert.Contains(gateH.Detail, ControlledHarmonyPatchExecution.HarmonyId);
        StringAssert.Contains(gateH.Detail, "Harmony Patch/PatchAll/CreateProcessor invoked: NO");

        var gateI = await boundary.RunPostConstructionAuditAsync();
        Assert.IsTrue(gateI.Passed, gateI.Detail);
        StringAssert.Contains(gateI.Detail, "Harmony type initialization: YES — exact measured static-cache initializer only");
        StringAssert.Contains(gateI.Detail, "Harmony object construction: YES — exact string constructor only");
        StringAssert.Contains(gateI.Detail, "Harmony patching/processor API invocation: NO");

        var gateJ = boundary.RunHarmonyProcessorApiResolution();
        Assert.IsTrue(gateJ.Passed, gateJ.Detail);
        StringAssert.Contains(gateJ.Detail, "CreateProcessor(System.Reflection.MethodBase)");
        StringAssert.Contains(gateJ.Detail, "PatchProcessor object constructed: NO");

        var gateK = boundary.RunPatchProcessorTypeInitialization();
        Assert.IsTrue(gateK.Passed, gateK.Detail);
        StringAssert.Contains(gateK.Detail, "RuntimeHelpers.RunClassConstructor(HarmonyLib.PatchProcessor.TypeHandle) = PASS");

        var gateL = boundary.RunLauncherProbeResolution();
        Assert.IsTrue(gateL.Passed, gateL.Detail);
        StringAssert.Contains(gateL.Detail, "HarmonyProcessorProbe::Target");
        StringAssert.Contains(gateL.Detail, "Method invoked: NO");

        var gateM = boundary.RunHarmonyProcessorCreation();
        Assert.IsTrue(gateM.Passed, gateM.Detail);
        StringAssert.Contains(gateM.Detail, "CONTROLLED EMPTY HARMONY PATCHPROCESSOR CREATION SUCCEEDED");
        StringAssert.Contains(gateM.Detail, "PatchProcessor.Patch invoked: NO");

        var gateN = await boundary.RunPostProcessorAuditAsync();
        Assert.IsTrue(gateN.Passed, gateN.Detail);
        StringAssert.Contains(gateN.Detail, "PatchProcessor retained Harmony/original fields: EXACT");
        StringAssert.Contains(gateN.Detail, "PatchProcessor.Patch/Harmony.Patch/PatchAll: NOT INVOKED");
    }

    [TestMethod]
    public void LauncherPatchProbeHasDeterministicOriginalAndPrefixBehavior()
    {
        HarmonyPatchProbe.ResetCounters();

        Assert.AreEqual(42, HarmonyPatchProbe.Target(41));
        var result = 0;
        Assert.IsFalse(HarmonyPatchProbe.Prefix(41, ref result));

        Assert.AreEqual(1041, result);
        Assert.AreEqual(1, HarmonyPatchProbe.TargetCalls);
        Assert.AreEqual(1, HarmonyPatchProbe.PrefixCalls);
    }

    [TestMethod]
    public void LauncherPatchProbeReflectionSurfaceIsExactAndLauncherOwned()
    {
        var target = typeof(HarmonyPatchProbe).GetMethod(
            nameof(HarmonyPatchProbe.Target),
            BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
        var prefix = typeof(HarmonyPatchProbe).GetMethod(
            nameof(HarmonyPatchProbe.Prefix),
            BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);

        Assert.IsNotNull(target);
        Assert.IsNotNull(prefix);
        Assert.AreEqual(typeof(int), target.ReturnType);
        CollectionAssert.AreEqual(new[] { typeof(int) }, target.GetParameters().Select(parameter => parameter.ParameterType).ToArray());
        Assert.AreEqual(typeof(bool), prefix.ReturnType);
        var prefixParameters = prefix.GetParameters();
        Assert.AreEqual(2, prefixParameters.Length);
        Assert.AreEqual("value", prefixParameters[0].Name);
        Assert.AreEqual(typeof(int), prefixParameters[0].ParameterType);
        Assert.AreEqual("__result", prefixParameters[1].Name);
        Assert.AreEqual(typeof(int).MakeByRefType(), prefixParameters[1].ParameterType);
        Assert.AreSame(typeof(HarmonyPatchProbe).Assembly, target.DeclaringType?.Assembly);
        Assert.AreSame(typeof(HarmonyPatchProbe).Assembly, prefix.DeclaringType?.Assembly);
    }

    [TestMethod]
    public void LauncherPatchPrefixCarriesNoHarmonyAnnotationsSoBoundedDescriptorPathIsEquivalent()
    {
        var prefix = typeof(HarmonyPatchProbe).GetMethod(
            nameof(HarmonyPatchProbe.Prefix),
            BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            ?? throw new AssertFailedException("Launcher prefix MethodInfo is missing.");

        var harmonyAnnotations = prefix.GetCustomAttributesData()
            .Where(attribute =>
                string.Equals(attribute.AttributeType.Namespace, "HarmonyLib", StringComparison.Ordinal) ||
                string.Equals(attribute.AttributeType.Assembly.GetName().Name, "0Harmony", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        Assert.AreEqual(0, harmonyAnnotations.Length,
            "The Step-27 bounded HarmonyMethod() descriptor path is admitted only while the launcher prefix has zero Harmony annotations.");
    }

    [TestMethod]
    public async Task GateARejectsReachablePInvokeBeforeAnyStep27ClrLoad()
    {
        var names = CreateNames();
        using var temp = new TempTestDirectory("sts2-step27-tests");
        await CreateSyntheticPreparedRuntimeAsync(temp.Path, names, InitializerKind.PInvoke);

        using var boundary = CreateSyntheticBoundary(temp.Path, names);
        var gateA = await boundary.RunInitializationPreflightAsync();

        Assert.IsFalse(gateA.Passed);
        StringAssert.Contains(gateA.Detail, "P/Invoke reachable");
        AssertNotLoaded(names.PrimarySimpleName);
        AssertNotLoaded(names.TargetSimpleName);
    }

    [TestMethod]
    public async Task GateARejectsFunctionPointerIndirectionBeforeAnyStep27ClrLoad()
    {
        var names = CreateNames();
        using var temp = new TempTestDirectory("sts2-step27-tests");
        await CreateSyntheticPreparedRuntimeAsync(temp.Path, names, InitializerKind.FunctionPointer);

        using var boundary = CreateSyntheticBoundary(temp.Path, names);
        var gateA = await boundary.RunInitializationPreflightAsync();

        Assert.IsFalse(gateA.Passed);
        StringAssert.Contains(gateA.Detail, "indirect function/delegate target reachable");
        AssertNotLoaded(names.PrimarySimpleName);
        AssertNotLoaded(names.TargetSimpleName);
    }

    [TestMethod]
    public async Task GateARejectsImplicitTypeInitializerPInvokeBeforeAnyStep27ClrLoad()
    {
        var names = CreateNames();
        using var temp = new TempTestDirectory("sts2-step27-tests");
        await CreateSyntheticPreparedRuntimeAsync(temp.Path, names, InitializerKind.TypeInitializerPInvoke);

        using var boundary = CreateSyntheticBoundary(temp.Path, names);
        var gateA = await boundary.RunInitializationPreflightAsync();

        Assert.IsFalse(gateA.Passed);
        StringAssert.Contains(gateA.Detail, "P/Invoke reachable");
        AssertNotLoaded(names.PrimarySimpleName);
        AssertNotLoaded(names.TargetSimpleName);
    }

    [TestMethod]
    public void GateAMetadataAuditDoesNotResolveExternalBaseForNominallyLocalMemberRef()
    {
        using var temp = new TempTestDirectory("sts2-step27-tests");
        var path = Path.Combine(temp.Path, "resolver-free-memberref.dll");
        WriteExternalBaseMemberRefFixture(path);

        var reader = typeof(ControlledHarmonyPatchExecution).GetMethod(
            "ReadPreparedMetadata",
            BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new AssertFailedException("Step 27 metadata reader was not found.");

        object? snapshot;
        try
        {
            snapshot = reader.Invoke(null, [path, true, "SyntheticHarmony"]);
        }
        catch (TargetInvocationException ex)
        {
            Assert.Fail("Gate A metadata reader attempted external resolution instead of failing closed from local metadata: " + ex.InnerException);
            return;
        }

        Assert.IsNotNull(snapshot);
        var hazards = snapshot.GetType().GetProperty("InitializerHazards")?.GetValue(snapshot) as IReadOnlyList<string>;
        Assert.IsNotNull(hazards);
        Assert.IsTrue(
            hazards.Any(value => value.Contains("Unresolved same-assembly call (local metadata only)", StringComparison.Ordinal)),
            "The nominally local inherited MemberRef should become an unresolved-local hazard without consulting GodotSharp metadata.");
    }

    [TestMethod]
    public void GateAConditionallyAcceptsExactPhysicalMonoModLoggerFingerprintOnlyWhenInert()
    {
        var decision = ControlledHarmonyPatchExecution.EvaluateInitializerHazardPolicy(
            ControlledHarmonyPatchExecution.TargetSimpleName,
            ControlledHarmonyPatchExecution.TargetVersion,
            ControlledHarmonyPatchExecution.ObservedMonoModLoggingDispatchHazards,
            PhysicalMonoModAutomaticInitializerAuditShape(),
            debuggerAttached: false,
            monoModEnvironmentOverrideNames: [],
            monoModAppContextOverrideNames: []);

        Assert.IsTrue(decision.Allowed, decision.Detail);
        Assert.AreEqual(0, decision.BlockingHazardCount);
        Assert.AreEqual(7, decision.ConditionalHazardCount);
        StringAssert.Contains(decision.Detail, "Exact Step 24.0.4 MonoMod logger dispatch fingerprint: MATCH");
    }

    [TestMethod]
    public void GateAConditionalMonoModPolicyRejectsAnyFingerprintDrift()
    {
        var hazards = ControlledHarmonyPatchExecution.ObservedMonoModLoggingDispatchHazards
            .Concat(["P/Invoke reachable: System.Void Unexpected::NativeProbe()"])
            .ToArray();
        var decision = ControlledHarmonyPatchExecution.EvaluateInitializerHazardPolicy(
            ControlledHarmonyPatchExecution.TargetSimpleName,
            ControlledHarmonyPatchExecution.TargetVersion,
            hazards,
            PhysicalMonoModAutomaticInitializerAuditShape(),
            debuggerAttached: false,
            monoModEnvironmentOverrideNames: [],
            monoModAppContextOverrideNames: []);

        Assert.IsFalse(decision.Allowed);
        Assert.AreEqual(hazards.Length, decision.BlockingHazardCount);
        StringAssert.Contains(decision.Detail, "fingerprint differs");
        StringAssert.Contains(decision.Detail, "P/Invoke reachable");
    }

    [TestMethod]
    public void GateAConditionalMonoModPolicyRejectsNonInertLoggingState()
    {
        var audits = PhysicalMonoModAutomaticInitializerAuditShape();

        var debugger = ControlledHarmonyPatchExecution.EvaluateInitializerHazardPolicy(
            ControlledHarmonyPatchExecution.TargetSimpleName,
            ControlledHarmonyPatchExecution.TargetVersion,
            ControlledHarmonyPatchExecution.ObservedMonoModLoggingDispatchHazards,
            audits,
            debuggerAttached: true,
            monoModEnvironmentOverrideNames: [],
            monoModAppContextOverrideNames: []);
        Assert.IsFalse(debugger.Allowed);
        StringAssert.Contains(debugger.Detail, "debugger is attached");

        var environment = ControlledHarmonyPatchExecution.EvaluateInitializerHazardPolicy(
            ControlledHarmonyPatchExecution.TargetSimpleName,
            ControlledHarmonyPatchExecution.TargetVersion,
            ControlledHarmonyPatchExecution.ObservedMonoModLoggingDispatchHazards,
            audits,
            debuggerAttached: false,
            monoModEnvironmentOverrideNames: ["MONOMOD_LogToFile"],
            monoModAppContextOverrideNames: []);
        Assert.IsFalse(environment.Allowed);
        StringAssert.Contains(environment.Detail, "MONOMOD_LogToFile");

        var appContext = ControlledHarmonyPatchExecution.EvaluateInitializerHazardPolicy(
            ControlledHarmonyPatchExecution.TargetSimpleName,
            ControlledHarmonyPatchExecution.TargetVersion,
            ControlledHarmonyPatchExecution.ObservedMonoModLoggingDispatchHazards,
            audits,
            debuggerAttached: false,
            monoModEnvironmentOverrideNames: [],
            monoModAppContextOverrideNames: ["MonoMod.LogInMemory"]);
        Assert.IsFalse(appContext.Allowed);
        StringAssert.Contains(appContext.Detail, "MonoMod.LogInMemory");
    }

    [TestMethod]
    public void GateAConditionalMonoModPolicyRequiresExactMeasuredAutomaticInitializerShape()
    {
        var audits = PhysicalMonoModAutomaticInitializerAuditShape()
            .Concat(["method=System.Void MonoMod.Unexpected::.cctor(); token=0x06000005; instructions=1; handlers=0; locals=0; IL=[IL_0000: Ret]"])
            .ToArray();
        var decision = ControlledHarmonyPatchExecution.EvaluateInitializerHazardPolicy(
            ControlledHarmonyPatchExecution.TargetSimpleName,
            ControlledHarmonyPatchExecution.TargetVersion,
            ControlledHarmonyPatchExecution.ObservedMonoModLoggingDispatchHazards,
            audits,
            debuggerAttached: false,
            monoModEnvironmentOverrideNames: [],
            monoModAppContextOverrideNames: []);

        Assert.IsFalse(decision.Allowed);
        StringAssert.Contains(decision.Detail, "four-method MonoMod logging shape");
    }

    private static string[] PhysicalMonoModAutomaticInitializerAuditShape()
        =>
        [
            "method=System.Void <Module>::.cctor(); token=0x06000001; instructions=2; handlers=0; locals=0; IL=[IL_0000: Call System.Void MonoMod.<fixture>MMDbgLog::LogVersion() | IL_0005: Ret]",
            "method=System.Void MonoMod.Switches::.cctor(); token=0x06000002; instructions=48; handlers=1; locals=5; IL=[IL_0000: Call System.Collections.IDictionary System.Environment::GetEnvironmentVariables() | IL_0005: Call System.Object MonoMod.Switches::BestEffortParseEnvVar(System.String) | IL_000A: Ret]",
            "method=System.Void MonoMod.Logs.DebugLog::.cctor(); token=0x06000003; instructions=15; handlers=0; locals=0; IL=[IL_0000: Newobj System.Void MonoMod.Logs.DebugLog::.ctor() | IL_0005: Stsfld MonoMod.Logs.DebugLog MonoMod.Logs.DebugLog::Instance | IL_000A: Stsfld System.Collections.Concurrent.ConcurrentDictionary`2<MonoMod.Logs.DebugLog/OnLogMessage,System.IDisposable> MonoMod.Logs.DebugLog::simpleRegDict | IL_000F: Ret]",
            "method=System.Void MonoMod.Logs.DebugLog/LevelSubscriptions::.cctor(); token=0x06000004; instructions=3; handlers=0; locals=0; IL=[IL_0000: Newobj System.Void MonoMod.Logs.DebugLog/LevelSubscriptions::.ctor() | IL_0005: Stsfld MonoMod.Logs.DebugLog/LevelSubscriptions MonoMod.Logs.DebugLog/LevelSubscriptions::None | IL_000A: Ret]",
        ];

    [TestMethod]
    public async Task GateARejectsHarmonyTypeInitializerShapeDriftBeforeAnyStep27ClrLoad()
    {
        var names = CreateNames();
        using var temp = new TempTestDirectory("sts2-step27-tests");
        await CreateSyntheticPreparedRuntimeAsync(temp.Path, names, InitializerKind.Return, driftHarmonyTypeInitializer: true);

        using var boundary = CreateSyntheticBoundary(temp.Path, names);
        var gateA = await boundary.RunInitializationPreflightAsync();

        Assert.IsFalse(gateA.Passed);
        StringAssert.Contains(gateA.Detail, "HarmonyLib.Harmony::.cctor no longer matches the measured inert 2.4.2 static-cache initialization shape");
        AssertNotLoaded(names.PrimarySimpleName);
        AssertNotLoaded(names.TargetSimpleName);
    }

    [TestMethod]
    public async Task GateCReportsThrowingModuleInitializerAndDoesNotAdvance()
    {
        var names = CreateNames();
        using var temp = new TempTestDirectory("sts2-step27-tests");
        await CreateSyntheticPreparedRuntimeAsync(temp.Path, names, InitializerKind.ThrowNull);

        using var boundary = CreateSyntheticBoundary(temp.Path, names);
        var gateA = await boundary.RunInitializationPreflightAsync();
        Assert.IsTrue(gateA.Passed, gateA.Detail);
        var gateB = boundary.RunProvenLoadStateReplay();
        Assert.IsTrue(gateB.Passed, gateB.Detail);

        var gateC = boundary.RunDeferredModuleInitialization();
        Assert.IsFalse(gateC.Passed);
        StringAssert.Contains(gateC.Detail, "Stage:");
    }

    private static ControlledHarmonyPatchExecution CreateSyntheticBoundary(string launcherRoot, SyntheticNames names)
        => new(
            launcherRoot,
            collectibleLoadContext: true,
            expectedPrimarySimpleName: names.PrimarySimpleName,
            targetSimpleName: names.TargetSimpleName,
            targetVersion: ControlledHarmonyPatchExecution.TargetVersion,
            freshProcessAssemblyNames: [names.PrimarySimpleName, names.TargetSimpleName]);

    private static SyntheticNames CreateNames()
    {
        var suffix = Guid.NewGuid().ToString("N");
        return new SyntheticNames(
            "StS2Launcher.Step27.SyntheticPrimary." + suffix,
            "StS2Launcher.Step27.SyntheticHarmony." + suffix);
    }

    private static void AssertNotLoaded(string simpleName)
        => Assert.IsFalse(
            AppDomain.CurrentDomain.GetAssemblies().Any(assembly =>
                string.Equals(assembly.GetName().Name, simpleName, StringComparison.OrdinalIgnoreCase)),
            $"Synthetic Step 27 assembly '{simpleName}' was loaded before the intended gate.");

    private static async Task CreateSyntheticPreparedRuntimeAsync(
        string launcherRoot,
        SyntheticNames names,
        InitializerKind initializerKind,
        bool driftHarmonyTypeInitializer = false)
    {
        var managedRelative = $"{SteamOfflineInstallInspection.ManagedRootRelativePath}/Depot-2868842";
        var managedRoot = Path.Combine(launcherRoot, managedRelative.Replace('/', Path.DirectorySeparatorChar));
        var arm64RelativeRoot = "SlayTheSpire2.app/Contents/Resources/data_sts2_macos_arm64";
        var liveRoot = Path.Combine(managedRoot, arm64RelativeRoot.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(liveRoot);

        var systemLinq = typeof(Enumerable).Assembly.GetName();
        var systemLinqFullName = systemLinq.FullName ?? throw new AssertFailedException("System.Linq has no FullName.");
        var systemLinqReference = ToReferenceSpec(systemLinq, "System.Linq");

        var systemRuntime = Assembly.Load(new AssemblyName("System.Runtime")).GetName();
        var systemRuntimeFullName = systemRuntime.FullName ?? throw new AssertFailedException("System.Runtime has no FullName.");
        var systemRuntimeReference = ToReferenceSpec(systemRuntime, "System.Runtime");

        var targetRelative = $"{arm64RelativeRoot}/{names.TargetSimpleName}.dll";
        var primaryRelative = $"{arm64RelativeRoot}/sts2.dll";
        var liveTarget = Path.Combine(managedRoot, targetRelative.Replace('/', Path.DirectorySeparatorChar));
        var livePrimary = Path.Combine(managedRoot, primaryRelative.Replace('/', Path.DirectorySeparatorChar));

        WriteAssembly(
            liveTarget,
            names.TargetSimpleName,
            ControlledHarmonyPatchExecution.TargetVersion,
            [systemLinqReference, systemRuntimeReference],
            initializerKind,
            driftHarmonyTypeInitializer);
        WriteAssembly(
            livePrimary,
            names.PrimarySimpleName,
            new Version(0, 1, 0, 0),
            [systemLinqReference, new(names.TargetSimpleName, ControlledHarmonyPatchExecution.TargetVersion, string.Empty)],
            InitializerKind.None);

        var receiptFiles = new List<SteamManagedInstallFile>();
        foreach (var path in Directory.EnumerateFiles(managedRoot, "*", SearchOption.AllDirectories).OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
        {
            var relative = Path.GetRelativePath(managedRoot, path).Replace(Path.DirectorySeparatorChar, '/');
            var bytes = await File.ReadAllBytesAsync(path);
            receiptFiles.Add(new SteamManagedInstallFile(
                relative,
                bytes.LongLength,
                Convert.ToHexString(SHA1.HashData(bytes)).ToLowerInvariant()));
        }

        const ulong manifestId = 24001UL;
        var receipt = new SteamManagedInstallReceipt(
            SteamManagedInstallReceipt.CurrentSchemaVersion,
            SteamOfflineInstallInspection.TargetAppId,
            2868842,
            manifestId,
            "public",
            DateTimeOffset.UtcNow,
            receiptFiles);
        await using (var receiptStream = File.Create(Path.Combine(managedRoot, SteamManagedInstallReceipt.FileName)))
        {
            await JsonSerializer.SerializeAsync(receiptStream, receipt, SteamManagedInstallJsonContext.Default.SteamManagedInstallReceipt);
        }

        var step21Root = Path.Combine(launcherRoot, PreparedRuntimeFrameworkBinding.WorkRootName);
        var preparedRoot = Path.Combine(step21Root, PreparedRuntimeFrameworkBinding.PreparedRootName);
        var planRoot = Path.Combine(step21Root, PreparedRuntimeFrameworkBinding.PlanRootName);
        Directory.CreateDirectory(planRoot);
        var preparedPrimary = Path.Combine(preparedRoot, primaryRelative.Replace('/', Path.DirectorySeparatorChar));
        var preparedTarget = Path.Combine(preparedRoot, targetRelative.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(preparedPrimary)!);
        File.Copy(livePrimary, preparedPrimary);
        File.Copy(liveTarget, preparedTarget);

        var primaryInfo = BuildPreparedPlanItem(preparedPrimary, primaryRelative, isPrimary: true);
        var targetInfo = BuildPreparedPlanItem(preparedTarget, targetRelative, isPrimary: false);

        var edges = new List<RuntimeBindingEdge>();
        AddSyntheticEdges(
            preparedPrimary,
            primaryInfo.AssemblyFullName,
            targetInfo.AssemblyFullName,
            names.TargetSimpleName,
            systemLinqFullName,
            systemRuntimeFullName,
            edges);
        AddSyntheticEdges(
            preparedTarget,
            targetInfo.AssemblyFullName,
            targetInfo.AssemblyFullName,
            names.TargetSimpleName,
            systemLinqFullName,
            systemRuntimeFullName,
            edges);

        var hostBindings = edges
            .Where(edge => edge.BindingKind.Equals("HostFramework", StringComparison.Ordinal))
            .GroupBy(edge => (edge.RequestedFullName, edge.Target))
            .OrderBy(group => group.Key.RequestedFullName, StringComparer.Ordinal)
            .Select(group => new RuntimeBindingHostFramework(
                group.Key.RequestedFullName,
                group.Key.Target,
                string.Empty,
                group.Count()))
            .ToArray();

        var plan = new RuntimeFrameworkBindingPlanDocument(
            RuntimeFrameworkBindingPlanDocument.CurrentSchemaVersion,
            SteamOfflineInstallInspection.TargetAppId,
            2868842,
            manifestId,
            "public",
            managedRelative,
            primaryRelative,
            primaryInfo.AssemblyFullName,
            [primaryInfo, targetInfo],
            hostBindings,
            [],
            edges.OrderBy(edge => edge.SourceAssemblyFullName, StringComparer.Ordinal)
                .ThenBy(edge => edge.RequestedFullName, StringComparer.Ordinal)
                .ToArray(),
            true);

        await using var planStream = File.Create(Path.Combine(planRoot, PreparedRuntimeFrameworkBinding.PlanFileName));
        await JsonSerializer.SerializeAsync(planStream, plan, RuntimeFrameworkBindingJsonContext.Default.RuntimeFrameworkBindingPlanDocument);
    }

    private static void AddSyntheticEdges(
        string assemblyPath,
        string sourceFullName,
        string targetFullName,
        string targetSimpleName,
        string systemLinqFullName,
        string systemRuntimeFullName,
        ICollection<RuntimeBindingEdge> edges)
    {
        using var module = ModuleDefinition.ReadModule(assemblyPath, new ReaderParameters
        {
            InMemory = true,
            ReadSymbols = false,
            ReadingMode = ReadingMode.Immediate,
        });

        foreach (var reference in module.AssemblyReferences)
        {
            if (reference.Name.Equals(targetSimpleName, StringComparison.OrdinalIgnoreCase))
            {
                edges.Add(new RuntimeBindingEdge(sourceFullName, reference.FullName, "WorkspaceExact", targetFullName));
                continue;
            }
            if (reference.Name.Equals("System.Linq", StringComparison.OrdinalIgnoreCase))
            {
                edges.Add(new RuntimeBindingEdge(sourceFullName, reference.FullName, "HostFramework", systemLinqFullName));
                continue;
            }
            if (reference.Name.Equals("System.Runtime", StringComparison.OrdinalIgnoreCase))
            {
                edges.Add(new RuntimeBindingEdge(sourceFullName, reference.FullName, "HostFramework", systemRuntimeFullName));
                continue;
            }
            throw new AssertFailedException("Synthetic Step 27 fixture emitted an unexpected AssemblyRef: " + reference.FullName);
        }
    }

    private static RuntimeBindingPreparedAssembly BuildPreparedPlanItem(string path, string relativePath, bool isPrimary)
    {
        using var module = ModuleDefinition.ReadModule(path, new ReaderParameters { InMemory = true, ReadSymbols = false });
        var fullName = module.Assembly?.Name.FullName ?? throw new AssertFailedException("Synthetic assembly has no identity.");
        var bytes = File.ReadAllBytes(path);
        return new RuntimeBindingPreparedAssembly(
            relativePath,
            fullName,
            Convert.ToHexString(SHA1.HashData(bytes)).ToLowerInvariant(),
            bytes.LongLength,
            isPrimary);
    }

    private static void WriteAssembly(
        string path,
        string name,
        Version version,
        IReadOnlyList<AssemblyReferenceSpec> references,
        InitializerKind initializerKind,
        bool driftHarmonyTypeInitializer = false)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        using var assembly = AssemblyDefinition.CreateAssembly(
            new AssemblyNameDefinition(name, version),
            name,
            ModuleKind.Dll);

        var declaredReferences = new Dictionary<string, AssemblyNameReference>(StringComparer.OrdinalIgnoreCase);
        foreach (var reference in references)
        {
            var item = CreateAssemblyNameReference(reference);
            declaredReferences.Add(reference.Name, item);
            assembly.MainModule.AssemblyReferences.Add(item);
        }

        if (initializerKind != InitializerKind.None)
        {
            if (!declaredReferences.TryGetValue("System.Runtime", out var systemRuntimeReference))
                throw new AssertFailedException("Synthetic Step 27 initializer requires predeclared System.Runtime.");

            var voidType = assembly.MainModule.TypeSystem.Void;
            Assert.AreSame(systemRuntimeReference, voidType.Scope);
            var moduleType = assembly.MainModule.Types.Single(type => type.Name == "<Module>");
            var initializer = new MethodDefinition(
                ".cctor",
                Mono.Cecil.MethodAttributes.Private |
                Mono.Cecil.MethodAttributes.Static |
                Mono.Cecil.MethodAttributes.SpecialName |
                Mono.Cecil.MethodAttributes.RTSpecialName,
                voidType);
            moduleType.Methods.Add(initializer);

            switch (initializerKind)
            {
                case InitializerKind.Return:
                    initializer.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
                    break;
                case InitializerKind.ThrowNull:
                    initializer.Body.Instructions.Add(Instruction.Create(OpCodes.Ldnull));
                    initializer.Body.Instructions.Add(Instruction.Create(OpCodes.Throw));
                    break;
                case InitializerKind.PInvoke:
                    var pinvokeModule = new ModuleReference("libstep27fixture.dylib");
                    assembly.MainModule.ModuleReferences.Add(pinvokeModule);
                    var nativeProbe = CreatePInvokeProbe(voidType, pinvokeModule);
                    moduleType.Methods.Add(nativeProbe);
                    initializer.Body.Instructions.Add(Instruction.Create(OpCodes.Call, nativeProbe));
                    initializer.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
                    break;
                case InitializerKind.FunctionPointer:
                    var helper = new MethodDefinition(
                        "IndirectTarget",
                        Mono.Cecil.MethodAttributes.Private | Mono.Cecil.MethodAttributes.Static,
                        voidType);
                    helper.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
                    moduleType.Methods.Add(helper);
                    initializer.Body.Instructions.Add(Instruction.Create(OpCodes.Ldftn, helper));
                    initializer.Body.Instructions.Add(Instruction.Create(OpCodes.Pop));
                    initializer.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
                    break;
                case InitializerKind.TypeInitializerPInvoke:
                    var typeInitModule = new ModuleReference("libstep27fixture-typeinit.dylib");
                    assembly.MainModule.ModuleReferences.Add(typeInitModule);
                    var autoInitType = new TypeDefinition(
                        "StS2Launcher.Step27.Tests",
                        "AutoInitType",
                        Mono.Cecil.TypeAttributes.NotPublic | Mono.Cecil.TypeAttributes.Abstract | Mono.Cecil.TypeAttributes.Sealed,
                        assembly.MainModule.TypeSystem.Object);
                    assembly.MainModule.Types.Add(autoInitType);
                    var typeInitializer = new MethodDefinition(
                        ".cctor",
                        Mono.Cecil.MethodAttributes.Private |
                        Mono.Cecil.MethodAttributes.Static |
                        Mono.Cecil.MethodAttributes.SpecialName |
                        Mono.Cecil.MethodAttributes.RTSpecialName,
                        voidType);
                    autoInitType.Methods.Add(typeInitializer);
                    var typeNativeProbe = CreatePInvokeProbe(voidType, typeInitModule);
                    autoInitType.Methods.Add(typeNativeProbe);
                    typeInitializer.Body.Instructions.Add(Instruction.Create(OpCodes.Call, typeNativeProbe));
                    typeInitializer.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
                    var touch = new MethodDefinition(
                        "Touch",
                        Mono.Cecil.MethodAttributes.Public | Mono.Cecil.MethodAttributes.Static,
                        voidType);
                    touch.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
                    autoInitType.Methods.Add(touch);
                    initializer.Body.Instructions.Add(Instruction.Create(OpCodes.Call, touch));
                    initializer.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(initializerKind));
            }
        }

        if (initializerKind != InitializerKind.None)
        {
            if (!declaredReferences.TryGetValue("System.Runtime", out var systemRuntimeReference))
                throw new AssertFailedException("Synthetic Step 27 Harmony type requires predeclared System.Runtime.");
            AddSyntheticHarmonyType(assembly.MainModule, systemRuntimeReference, driftHarmonyTypeInitializer);
        }

        assembly.Write(path);

        using var written = ModuleDefinition.ReadModule(path, new ReaderParameters
        {
            InMemory = true,
            ReadSymbols = false,
            ReadingMode = ReadingMode.Immediate,
        });
        Assert.IsFalse(written.AssemblyReferences.Any(reference => reference.Name.Equals("mscorlib", StringComparison.OrdinalIgnoreCase)));
        if (initializerKind != InitializerKind.None)
        {
            var initializer = written.Types.Single(type => type.Name == "<Module>").Methods.Single(method => method.Name == ".cctor");
            Assert.AreEqual(MetadataType.Void, initializer.ReturnType.MetadataType);
            Assert.AreEqual("System.Runtime", initializer.ReturnType.Scope?.Name);
        }
    }

    private static void AddSyntheticHarmonyType(ModuleDefinition module, AssemblyNameReference systemRuntimeReference, bool driftHarmonyTypeInitializer)
    {
        var voidType = module.TypeSystem.Void;
        var boolType = module.TypeSystem.Boolean;
        var stringType = module.TypeSystem.String;
        var objectType = module.TypeSystem.Object;
        Assert.AreSame(systemRuntimeReference, voidType.Scope);

        var harmonyType = new TypeDefinition(
            "HarmonyLib",
            "Harmony",
            Mono.Cecil.TypeAttributes.Public | Mono.Cecil.TypeAttributes.Class | Mono.Cecil.TypeAttributes.BeforeFieldInit,
            objectType);
        module.Types.Add(harmonyType);

        var debugField = new FieldDefinition(
            "DEBUG",
            Mono.Cecil.FieldAttributes.Public | Mono.Cecil.FieldAttributes.Static,
            boolType);
        harmonyType.Fields.Add(debugField);

        var conditionalWeakTableDefinition = new TypeReference(
            "System.Runtime.CompilerServices",
            "ConditionalWeakTable`2",
            module,
            systemRuntimeReference,
            false);
        conditionalWeakTableDefinition.GenericParameters.Add(new GenericParameter("TKey", conditionalWeakTableDefinition));
        conditionalWeakTableDefinition.GenericParameters.Add(new GenericParameter("TValue", conditionalWeakTableDefinition));
        var conditionalWeakTableType = new GenericInstanceType(conditionalWeakTableDefinition);
        conditionalWeakTableType.GenericArguments.Add(objectType);
        conditionalWeakTableType.GenericArguments.Add(objectType);
        var assemblyCachedCategories = new FieldDefinition(
            "AssemblyCachedCategories",
            Mono.Cecil.FieldAttributes.Private | Mono.Cecil.FieldAttributes.Static | Mono.Cecil.FieldAttributes.InitOnly,
            conditionalWeakTableType);
        harmonyType.Fields.Add(assemblyCachedCategories);

        var conditionalWeakTableCtor = new MethodReference(".ctor", voidType, conditionalWeakTableType)
        {
            HasThis = true,
        };
        var harmonyTypeInitializer = new MethodDefinition(
            ".cctor",
            Mono.Cecil.MethodAttributes.Private |
            Mono.Cecil.MethodAttributes.Static |
            Mono.Cecil.MethodAttributes.SpecialName |
            Mono.Cecil.MethodAttributes.RTSpecialName,
            voidType);
        harmonyTypeInitializer.Body.Instructions.Add(Instruction.Create(OpCodes.Newobj, conditionalWeakTableCtor));
        harmonyTypeInitializer.Body.Instructions.Add(Instruction.Create(OpCodes.Stsfld, assemblyCachedCategories));
        if (driftHarmonyTypeInitializer)
            harmonyTypeInitializer.Body.Instructions.Add(Instruction.Create(OpCodes.Nop));
        harmonyTypeInitializer.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
        harmonyType.Methods.Add(harmonyTypeInitializer);

        var idField = new FieldDefinition(
            "<Id>k__BackingField",
            Mono.Cecil.FieldAttributes.Private,
            stringType);
        harmonyType.Fields.Add(idField);

        var getId = new MethodDefinition(
            "get_Id",
            Mono.Cecil.MethodAttributes.Public | Mono.Cecil.MethodAttributes.HideBySig | Mono.Cecil.MethodAttributes.SpecialName,
            stringType)
        {
            HasThis = true,
        };
        getId.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
        getId.Body.Instructions.Add(Instruction.Create(OpCodes.Ldfld, idField));
        getId.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
        harmonyType.Methods.Add(getId);

        var setId = new MethodDefinition(
            "set_Id",
            Mono.Cecil.MethodAttributes.Private | Mono.Cecil.MethodAttributes.HideBySig | Mono.Cecil.MethodAttributes.SpecialName,
            voidType)
        {
            HasThis = true,
        };
        setId.Parameters.Add(new ParameterDefinition("value", Mono.Cecil.ParameterAttributes.None, stringType));
        setId.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
        setId.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_1));
        setId.Body.Instructions.Add(Instruction.Create(OpCodes.Stfld, idField));
        setId.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
        harmonyType.Methods.Add(setId);

        harmonyType.Properties.Add(new PropertyDefinition("Id", Mono.Cecil.PropertyAttributes.None, stringType)
        {
            GetMethod = getId,
            SetMethod = setId,
        });

        var debugOnly = new MethodDefinition(
            "DebugOnly",
            Mono.Cecil.MethodAttributes.Private | Mono.Cecil.MethodAttributes.Static,
            voidType);
        debugOnly.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
        harmonyType.Methods.Add(debugOnly);

        var objectCtor = new MethodReference(".ctor", voidType, objectType)
        {
            HasThis = true,
        };
        var environmentType = new TypeReference("System", "Environment", module, systemRuntimeReference, false);
        var getEnvironmentVariable = new MethodReference("GetEnvironmentVariable", stringType, environmentType)
        {
            HasThis = false,
        };
        getEnvironmentVariable.Parameters.Add(new ParameterDefinition(stringType));

        var ctor = new MethodDefinition(
            ".ctor",
            Mono.Cecil.MethodAttributes.Public |
            Mono.Cecil.MethodAttributes.HideBySig |
            Mono.Cecil.MethodAttributes.SpecialName |
            Mono.Cecil.MethodAttributes.RTSpecialName,
            voidType)
        {
            HasThis = true,
        };
        ctor.Parameters.Add(new ParameterDefinition("id", Mono.Cecil.ParameterAttributes.None, stringType));
        harmonyType.Methods.Add(ctor);

        var ret = Instruction.Create(OpCodes.Ret);
        var afterDebug = Instruction.Create(OpCodes.Ldarg_0);
        ctor.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
        ctor.Body.Instructions.Add(Instruction.Create(OpCodes.Call, objectCtor));
        ctor.Body.Instructions.Add(Instruction.Create(OpCodes.Ldstr, "HARMONY_DEBUG"));
        ctor.Body.Instructions.Add(Instruction.Create(OpCodes.Call, getEnvironmentVariable));
        ctor.Body.Instructions.Add(Instruction.Create(OpCodes.Pop));
        ctor.Body.Instructions.Add(Instruction.Create(OpCodes.Ldsfld, debugField));
        ctor.Body.Instructions.Add(Instruction.Create(OpCodes.Brfalse_S, afterDebug));
        ctor.Body.Instructions.Add(Instruction.Create(OpCodes.Call, debugOnly));
        ctor.Body.Instructions.Add(afterDebug);
        ctor.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_1));
        ctor.Body.Instructions.Add(Instruction.Create(OpCodes.Call, setId));
        ctor.Body.Instructions.Add(ret);

        var methodBaseType = new TypeReference("System.Reflection", "MethodBase", module, systemRuntimeReference, false);
        var patchProcessorType = new TypeDefinition(
            "HarmonyLib",
            "PatchProcessor",
            Mono.Cecil.TypeAttributes.Public | Mono.Cecil.TypeAttributes.Class,
            objectType);
        module.Types.Add(patchProcessorType);

        var processorInstanceField = new FieldDefinition(
            "instance",
            Mono.Cecil.FieldAttributes.Private | Mono.Cecil.FieldAttributes.InitOnly,
            harmonyType);
        var processorOriginalField = new FieldDefinition(
            "original",
            Mono.Cecil.FieldAttributes.Private | Mono.Cecil.FieldAttributes.InitOnly,
            methodBaseType);
        var processorLockerField = new FieldDefinition(
            "locker",
            Mono.Cecil.FieldAttributes.Assembly | Mono.Cecil.FieldAttributes.Static | Mono.Cecil.FieldAttributes.InitOnly,
            objectType);
        patchProcessorType.Fields.Add(processorInstanceField);
        patchProcessorType.Fields.Add(processorOriginalField);
        patchProcessorType.Fields.Add(processorLockerField);

        var processorTypeInitializer = new MethodDefinition(
            ".cctor",
            Mono.Cecil.MethodAttributes.Private | Mono.Cecil.MethodAttributes.Static | Mono.Cecil.MethodAttributes.SpecialName | Mono.Cecil.MethodAttributes.RTSpecialName,
            voidType);
        processorTypeInitializer.Body.Instructions.Add(Instruction.Create(OpCodes.Newobj, objectCtor));
        processorTypeInitializer.Body.Instructions.Add(Instruction.Create(OpCodes.Stsfld, processorLockerField));
        processorTypeInitializer.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
        patchProcessorType.Methods.Add(processorTypeInitializer);

        var processorCtor = new MethodDefinition(
            ".ctor",
            Mono.Cecil.MethodAttributes.Public | Mono.Cecil.MethodAttributes.HideBySig | Mono.Cecil.MethodAttributes.SpecialName | Mono.Cecil.MethodAttributes.RTSpecialName,
            voidType)
        {
            HasThis = true,
        };
        processorCtor.Parameters.Add(new ParameterDefinition("instance", Mono.Cecil.ParameterAttributes.None, harmonyType));
        processorCtor.Parameters.Add(new ParameterDefinition("original", Mono.Cecil.ParameterAttributes.None, methodBaseType));
        processorCtor.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
        processorCtor.Body.Instructions.Add(Instruction.Create(OpCodes.Call, objectCtor));
        processorCtor.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
        processorCtor.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_1));
        processorCtor.Body.Instructions.Add(Instruction.Create(OpCodes.Stfld, processorInstanceField));
        processorCtor.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
        processorCtor.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_2));
        processorCtor.Body.Instructions.Add(Instruction.Create(OpCodes.Stfld, processorOriginalField));
        processorCtor.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
        patchProcessorType.Methods.Add(processorCtor);

        var createProcessor = new MethodDefinition(
            "CreateProcessor",
            Mono.Cecil.MethodAttributes.Public | Mono.Cecil.MethodAttributes.HideBySig,
            patchProcessorType)
        {
            HasThis = true,
        };
        createProcessor.Parameters.Add(new ParameterDefinition("original", Mono.Cecil.ParameterAttributes.None, methodBaseType));
        createProcessor.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
        createProcessor.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_1));
        createProcessor.Body.Instructions.Add(Instruction.Create(OpCodes.Newobj, processorCtor));
        createProcessor.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
        harmonyType.Methods.Add(createProcessor);
    }

    private static void WriteAccessToolsFixture(string path, bool drift, bool wrongThrowOnError = false, bool wrongLockRecursion = false)
    {
        using var module = ModuleDefinition.CreateModule(Path.GetFileName(path), ModuleKind.Dll);
        var bindingFlagsType = module.ImportReference(typeof(BindingFlags));
        var systemType = module.ImportReference(typeof(Type));
        var propertyInfoType = module.ImportReference(typeof(PropertyInfo));
        var readerWriterLockType = module.ImportReference(typeof(System.Threading.ReaderWriterLockSlim));
        var lockRecursionPolicyType = module.ImportReference(typeof(System.Threading.LockRecursionPolicy));

        var fastInvokeHandler = new TypeDefinition(
            "HarmonyLib",
            "FastInvokeHandler",
            Mono.Cecil.TypeAttributes.NotPublic | Mono.Cecil.TypeAttributes.Sealed,
            module.TypeSystem.Object);
        module.Types.Add(fastInvokeHandler);

        var dictionaryOpen = module.ImportReference(typeof(Dictionary<,>));
        var dictionaryType = new GenericInstanceType(dictionaryOpen);
        dictionaryType.GenericArguments.Add(systemType);
        dictionaryType.GenericArguments.Add(fastInvokeHandler);

        var accessTools = new TypeDefinition(
            "HarmonyLib",
            "AccessTools",
            Mono.Cecil.TypeAttributes.Public | Mono.Cecil.TypeAttributes.Abstract | Mono.Cecil.TypeAttributes.Sealed,
            module.TypeSystem.Object);
        module.Types.Add(accessTools);

        var allTypesCached = new FieldDefinition("allTypesCached", Mono.Cecil.FieldAttributes.Private | Mono.Cecil.FieldAttributes.Static, new ArrayType(systemType));
        var all = new FieldDefinition("all", Mono.Cecil.FieldAttributes.Public | Mono.Cecil.FieldAttributes.Static | Mono.Cecil.FieldAttributes.InitOnly, bindingFlagsType);
        var allDeclared = new FieldDefinition("allDeclared", Mono.Cecil.FieldAttributes.Public | Mono.Cecil.FieldAttributes.Static | Mono.Cecil.FieldAttributes.InitOnly, bindingFlagsType);
        var isMonoRuntime = new FieldDefinition("<IsMonoRuntime>k__BackingField", Mono.Cecil.FieldAttributes.Private | Mono.Cecil.FieldAttributes.Static, module.TypeSystem.Boolean);
        var isNetFrameworkRuntime = new FieldDefinition("<IsNetFrameworkRuntime>k__BackingField", Mono.Cecil.FieldAttributes.Private | Mono.Cecil.FieldAttributes.Static, module.TypeSystem.Boolean);
        var isNetCoreRuntime = new FieldDefinition("<IsNetCoreRuntime>k__BackingField", Mono.Cecil.FieldAttributes.Private | Mono.Cecil.FieldAttributes.Static, module.TypeSystem.Boolean);
        var addHandlerCache = new FieldDefinition("addHandlerCache", Mono.Cecil.FieldAttributes.Private | Mono.Cecil.FieldAttributes.Static, dictionaryType);
        var addHandlerCacheLock = new FieldDefinition("addHandlerCacheLock", Mono.Cecil.FieldAttributes.Private | Mono.Cecil.FieldAttributes.Static, readerWriterLockType);
        accessTools.Fields.Add(allTypesCached);
        accessTools.Fields.Add(all);
        accessTools.Fields.Add(allDeclared);
        accessTools.Fields.Add(isMonoRuntime);
        accessTools.Fields.Add(isNetFrameworkRuntime);
        accessTools.Fields.Add(isNetCoreRuntime);
        accessTools.Fields.Add(addHandlerCache);
        accessTools.Fields.Add(addHandlerCacheLock);

        var getIsMonoRuntime = new MethodDefinition(
            "get_IsMonoRuntime",
            Mono.Cecil.MethodAttributes.Public | Mono.Cecil.MethodAttributes.Static | Mono.Cecil.MethodAttributes.HideBySig | Mono.Cecil.MethodAttributes.SpecialName,
            module.TypeSystem.Boolean);
        getIsMonoRuntime.Body.Instructions.Add(Instruction.Create(OpCodes.Ldsfld, isMonoRuntime));
        getIsMonoRuntime.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
        accessTools.Methods.Add(getIsMonoRuntime);

        var typeGetType1 = module.ImportReference(typeof(Type).GetMethod(nameof(Type.GetType), [typeof(string)])!);
        var typeGetType2 = module.ImportReference(typeof(Type).GetMethod(nameof(Type.GetType), [typeof(string), typeof(bool)])!);
        var typeGetProperty = module.ImportReference(typeof(Type).GetMethod(nameof(Type.GetProperty), [typeof(string)])!);
        var propertyGetValue = module.ImportReference(typeof(PropertyInfo).GetMethod(nameof(PropertyInfo.GetValue), [typeof(object), typeof(object[])])!);
        var objectToString = module.ImportReference(typeof(object).GetMethod(nameof(ToString), Type.EmptyTypes)!);
        var stringStartsWith = module.ImportReference(typeof(string).GetMethod(nameof(string.StartsWith), [typeof(string)])!);
        var dictionaryCtor = new MethodReference(".ctor", module.TypeSystem.Void, dictionaryType) { HasThis = true };
        var lockCtor = module.ImportReference(typeof(System.Threading.ReaderWriterLockSlim).GetConstructor([typeof(System.Threading.LockRecursionPolicy)])!);

        var cctor = new MethodDefinition(
            ".cctor",
            Mono.Cecil.MethodAttributes.Private | Mono.Cecil.MethodAttributes.Static | Mono.Cecil.MethodAttributes.SpecialName | Mono.Cecil.MethodAttributes.RTSpecialName,
            module.TypeSystem.Void);
        accessTools.Methods.Add(cctor);
        var il = cctor.Body.GetILProcessor();

        var frameworkType1 = Instruction.Create(OpCodes.Ldstr, "FrameworkDescription");
        var storeFramework = Instruction.Create(OpCodes.Stsfld, isNetFrameworkRuntime);
        var frameworkType2 = Instruction.Create(OpCodes.Ldstr, "FrameworkDescription");
        var storeNetCore = Instruction.Create(OpCodes.Stsfld, isNetCoreRuntime);

        il.Append(Instruction.Create(OpCodes.Ldnull));
        il.Append(Instruction.Create(OpCodes.Stsfld, allTypesCached));
        il.Append(Instruction.Create(OpCodes.Ldc_I4, 15420));
        il.Append(Instruction.Create(OpCodes.Stsfld, all));
        il.Append(Instruction.Create(OpCodes.Ldsfld, all));
        il.Append(Instruction.Create(OpCodes.Ldc_I4_2));
        il.Append(Instruction.Create(OpCodes.Or));
        il.Append(Instruction.Create(OpCodes.Stsfld, allDeclared));

        il.Append(Instruction.Create(OpCodes.Ldstr, "Mono.Runtime"));
        il.Append(Instruction.Create(OpCodes.Call, typeGetType1));
        il.Append(Instruction.Create(OpCodes.Ldnull));
        il.Append(Instruction.Create(OpCodes.Ceq));
        il.Append(Instruction.Create(OpCodes.Ldc_I4_0));
        il.Append(Instruction.Create(OpCodes.Ceq));
        il.Append(Instruction.Create(OpCodes.Stsfld, isMonoRuntime));

        il.Append(Instruction.Create(OpCodes.Ldstr, "System.Runtime.InteropServices.RuntimeInformation"));
        il.Append(Instruction.Create(wrongThrowOnError ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0));
        il.Append(Instruction.Create(OpCodes.Call, typeGetType2));
        il.Append(Instruction.Create(OpCodes.Dup));
        il.Append(Instruction.Create(OpCodes.Brtrue_S, frameworkType1));
        il.Append(Instruction.Create(OpCodes.Pop));
        il.Append(Instruction.Create(OpCodes.Call, getIsMonoRuntime));
        il.Append(Instruction.Create(OpCodes.Ldc_I4_0));
        il.Append(Instruction.Create(OpCodes.Ceq));
        il.Append(Instruction.Create(OpCodes.Br_S, storeFramework));
        il.Append(frameworkType1);
        il.Append(Instruction.Create(OpCodes.Call, typeGetProperty));
        il.Append(Instruction.Create(OpCodes.Ldnull));
        il.Append(Instruction.Create(OpCodes.Ldnull));
        il.Append(Instruction.Create(OpCodes.Callvirt, propertyGetValue));
        il.Append(Instruction.Create(OpCodes.Callvirt, objectToString));
        il.Append(Instruction.Create(OpCodes.Ldstr, ".NET Framework"));
        il.Append(Instruction.Create(OpCodes.Callvirt, stringStartsWith));
        il.Append(storeFramework);

        il.Append(Instruction.Create(OpCodes.Ldstr, "System.Runtime.InteropServices.RuntimeInformation"));
        il.Append(Instruction.Create(OpCodes.Ldc_I4_0));
        il.Append(Instruction.Create(OpCodes.Call, typeGetType2));
        il.Append(Instruction.Create(OpCodes.Dup));
        il.Append(Instruction.Create(OpCodes.Brtrue_S, frameworkType2));
        il.Append(Instruction.Create(OpCodes.Pop));
        il.Append(Instruction.Create(OpCodes.Ldc_I4_0));
        il.Append(Instruction.Create(OpCodes.Br_S, storeNetCore));
        il.Append(frameworkType2);
        il.Append(Instruction.Create(OpCodes.Call, typeGetProperty));
        il.Append(Instruction.Create(OpCodes.Ldnull));
        il.Append(Instruction.Create(OpCodes.Ldnull));
        il.Append(Instruction.Create(OpCodes.Callvirt, propertyGetValue));
        il.Append(Instruction.Create(OpCodes.Callvirt, objectToString));
        il.Append(Instruction.Create(OpCodes.Ldstr, ".NET Core"));
        il.Append(Instruction.Create(OpCodes.Callvirt, stringStartsWith));
        il.Append(storeNetCore);

        il.Append(Instruction.Create(OpCodes.Newobj, dictionaryCtor));
        il.Append(Instruction.Create(OpCodes.Stsfld, addHandlerCache));
        il.Append(Instruction.Create(wrongLockRecursion ? OpCodes.Ldc_I4_0 : OpCodes.Ldc_I4_1));
        il.Append(Instruction.Create(OpCodes.Newobj, lockCtor));
        il.Append(Instruction.Create(OpCodes.Stsfld, addHandlerCacheLock));
        if (drift)
            il.Append(Instruction.Create(OpCodes.Nop));
        il.Append(Instruction.Create(OpCodes.Ret));
        module.Write(path);
    }

    private static void WriteExternalBaseMemberRefFixture(string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        using var assembly = AssemblyDefinition.CreateAssembly(
            new AssemblyNameDefinition("SyntheticHarmony", ControlledHarmonyPatchExecution.TargetVersion),
            "SyntheticHarmony",
            ModuleKind.Dll);

        var runtimeName = Assembly.Load(new AssemblyName("System.Runtime")).GetName();
        var runtimeReference = CreateAssemblyNameReference(ToReferenceSpec(runtimeName, "System.Runtime"));
        assembly.MainModule.AssemblyReferences.Add(runtimeReference);
        var voidType = assembly.MainModule.TypeSystem.Void;
        Assert.AreSame(runtimeReference, voidType.Scope);

        var godotReference = new AssemblyNameReference("GodotSharp", new Version(4, 5, 1, 0));
        assembly.MainModule.AssemblyReferences.Add(godotReference);
        var unavailableBase = new TypeReference(
            "Godot",
            "GodotObject",
            assembly.MainModule,
            godotReference,
            false);
        var derived = new TypeDefinition(
            "StS2Launcher.Step27.Tests",
            "DerivedFromUnavailableGodot",
            Mono.Cecil.TypeAttributes.NotPublic,
            unavailableBase);
        assembly.MainModule.Types.Add(derived);

        var moduleType = assembly.MainModule.Types.Single(type => type.Name == "<Module>");
        var initializer = new MethodDefinition(
            ".cctor",
            Mono.Cecil.MethodAttributes.Private |
            Mono.Cecil.MethodAttributes.Static |
            Mono.Cecil.MethodAttributes.SpecialName |
            Mono.Cecil.MethodAttributes.RTSpecialName,
            voidType);
        moduleType.Methods.Add(initializer);

        // Deliberately declare the MemberRef on the local derived type without defining the method
        // there. A general Cecil Resolve() would be allowed to walk the unavailable Godot base type;
        // Step 27 must instead stop from local metadata with an unresolved-local hazard.
        var inheritedReference = new MethodReference("InheritedTouch", voidType, derived)
        {
            HasThis = false,
        };
        initializer.Body.Instructions.Add(Instruction.Create(OpCodes.Call, inheritedReference));
        initializer.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
        assembly.Write(path);
    }

    private static MethodDefinition CreatePInvokeProbe(TypeReference voidType, ModuleReference nativeModule)
        => new(
            "NativeProbe",
            Mono.Cecil.MethodAttributes.Private |
            Mono.Cecil.MethodAttributes.Static |
            Mono.Cecil.MethodAttributes.PInvokeImpl,
            voidType)
        {
            PInvokeInfo = new PInvokeInfo(Mono.Cecil.PInvokeAttributes.CallConvCdecl, "step27_fixture_probe", nativeModule),
        };

    private static AssemblyNameReference CreateAssemblyNameReference(AssemblyReferenceSpec reference)
    {
        var item = new AssemblyNameReference(reference.Name, reference.Version);
        if (!string.IsNullOrEmpty(reference.PublicKeyTokenHex))
            item.PublicKeyToken = Convert.FromHexString(reference.PublicKeyTokenHex);
        return item;
    }

    private static AssemblyReferenceSpec ToReferenceSpec(AssemblyName name, string fallbackName)
        => new(
            name.Name ?? fallbackName,
            name.Version ?? new Version(9, 0, 0, 0),
            Convert.ToHexString(name.GetPublicKeyToken() ?? []).ToLowerInvariant());

    private enum InitializerKind
    {
        None,
        Return,
        ThrowNull,
        PInvoke,
        FunctionPointer,
        TypeInitializerPInvoke,
    }

    private sealed record SyntheticNames(string PrimarySimpleName, string TargetSimpleName);
    private sealed record AssemblyReferenceSpec(string Name, Version Version, string PublicKeyTokenHex);
}
