using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mono.Cecil;
using Mono.Cecil.Cil;
using StS2Launcher.Core;

namespace StS2Launcher.Core.Tests;

[TestClass]
public sealed class AheadOfLoadManagedTransformationTests
{
    [TestMethod]
    public void OrderedAheadOfLoadGatesReachFiveOfFivePass()
    {
        var gates = new AheadOfLoadManagedTransformationGateSequence();
        gates.Record(AheadOfLoadManagedTransformationGate.FixtureAdmissionAndOfflineReady, true, "admit");
        gates.Record(AheadOfLoadManagedTransformationGate.DeterministicRewrite, true, "rewrite");
        gates.Record(AheadOfLoadManagedTransformationGate.TransformedImageVerification, true, "verify");
        gates.Record(AheadOfLoadManagedTransformationGate.TransformedExecution, true, "execute");
        gates.Record(AheadOfLoadManagedTransformationGate.FinalIsolationAudit, true, "audit");

        var summary = gates.Snapshot();
        Assert.IsTrue(summary.Passed);
        Assert.AreEqual(5, summary.Gates.Count);
        Assert.AreEqual("AHEAD-OF-LOAD MANAGED TRANSFORMATION BOUNDARY PASS — 5/5", summary.Summary);
    }

    [TestMethod]
    public void AheadOfLoadGatesStopAfterFirstFailure()
    {
        var gates = new AheadOfLoadManagedTransformationGateSequence();
        gates.Record(AheadOfLoadManagedTransformationGate.FixtureAdmissionAndOfflineReady, true, "admit");
        gates.Record(AheadOfLoadManagedTransformationGate.DeterministicRewrite, false, "rewrite failed");
        Assert.ThrowsExactly<InvalidOperationException>(() =>
            gates.Record(AheadOfLoadManagedTransformationGate.TransformedImageVerification, true, "must not advance"));
        Assert.AreEqual(AheadOfLoadManagedTransformationGate.DeterministicRewrite, gates.Snapshot().FirstFailingGate);
    }

    [TestMethod]
    public void PostPublishAheadOfLoadFixtureHasExactSourceSurfaceWithoutProjectReference()
    {
        var fixtureRoot = RequireFixtureRoot();
        var fixturePath = Path.Combine(fixtureRoot, AheadOfLoadManagedTransformation.FixtureFileName);
        using var module = ModuleDefinition.ReadModule(fixturePath, new ReaderParameters { ReadingMode = ReadingMode.Immediate });

        Assert.AreEqual(AheadOfLoadManagedTransformation.FixtureAssemblySimpleName, module.Assembly.Name.Name);
        Assert.IsNull(module.EntryPoint);
        var probe = module.Types.Single(type => type.FullName == AheadOfLoadManagedTransformation.FixtureTypeFullName);
        var adjustment = probe.Methods.Single(method => method.Name == "Adjustment");
        var target = probe.Methods.Single(method => method.Name == "Target");
        var invokeTarget = probe.Methods.Single(method => method.Name == "InvokeTarget");

        Assert.AreEqual(2, adjustment.Body.Instructions.Count);
        Assert.AreEqual(Code.Ldc_I4_1, adjustment.Body.Instructions[0].OpCode.Code);
        Assert.AreEqual(Code.Ret, adjustment.Body.Instructions[1].OpCode.Code);
        Assert.IsTrue(target.Body.Instructions.Any(i => i.OpCode.Code == Code.Call && i.Operand is MethodReference m && m.Name == "Adjustment"));
        Assert.IsTrue(invokeTarget.Body.Instructions.Any(i => i.OpCode.Code == Code.Call && i.Operand is MethodReference m && m.Name == "Target"));
    }

    [TestMethod]
    public async Task VerifiedSourceIsRewrittenBeforeLoadAndOnlyTransformedBehaviorExecutes()
    {
        var fixtureRoot = RequireFixtureRoot();
        using var temp = new TempTestDirectory("sts2-step28-tests");
        var managedPath = await CreateOfflineReadyInstallAsync(temp.Path, 28001UL);
        var managedFile = Path.Combine(managedPath, "payload.bin");
        var managedBefore = SHA256.HashData(await File.ReadAllBytesAsync(managedFile));
        var bundlePath = Path.Combine(fixtureRoot, AheadOfLoadManagedTransformation.FixtureFileName);
        var bundleBefore = SHA256.HashData(await File.ReadAllBytesAsync(bundlePath));

        var transformation = new AheadOfLoadManagedTransformation(temp.Path, fixtureRoot);
        var gateA = await transformation.RunFixtureAdmissionAndOfflineReadyAsync();
        var gateB = transformation.RunDeterministicRewrite();
        var gateC = transformation.RunTransformedImageVerification();
        var gateD = transformation.RunTransformedExecution();
        var gateE = await transformation.RunFinalIsolationAuditAsync();

        Assert.IsTrue(gateA.Passed, gateA.Detail);
        Assert.IsTrue(gateB.Passed, gateB.Detail);
        Assert.IsTrue(gateC.Passed, gateC.Detail);
        Assert.IsTrue(gateD.Passed, gateD.Detail);
        Assert.IsTrue(gateE.Passed, gateE.Detail);
        CollectionAssert.AreEqual(bundleBefore, SHA256.HashData(await File.ReadAllBytesAsync(bundlePath)));
        CollectionAssert.AreEqual(managedBefore, SHA256.HashData(await File.ReadAllBytesAsync(managedFile)));

        StringAssert.Contains(gateA.Detail, "ORIGINAL IMAGE HAS NOT ENTERED THE CLR");
        StringAssert.Contains(gateB.Detail, "Adjustment() constant 1 -> 1000");
        StringAssert.Contains(gateC.Detail, "Transformed Adjustment() is: 1000");
        StringAssert.Contains(gateD.Detail, "Target(41) reflection result: 1041");
        StringAssert.Contains(gateD.Detail, "InvokeTarget(41) in-fixture direct-call result: 1041");
        StringAssert.Contains(gateD.Detail, "Original bundled/private-source bytes CLR-loaded: NO");
        StringAssert.Contains(gateE.Detail, "Trusted Step 12 managed install unchanged: YES");
        StringAssert.Contains(gateE.Detail, "Harmony/MonoMod runtime patching by Step 28: NO");

        var sourcePath = Path.Combine(
            temp.Path,
            AheadOfLoadManagedTransformation.WorkRootName,
            AheadOfLoadManagedTransformation.SourceRootName,
            AheadOfLoadManagedTransformation.FixtureFileName);
        var transformedPath = Path.Combine(
            temp.Path,
            AheadOfLoadManagedTransformation.WorkRootName,
            AheadOfLoadManagedTransformation.TransformedRootName,
            AheadOfLoadManagedTransformation.FixtureFileName);
        Assert.IsTrue(File.Exists(sourcePath));
        Assert.IsTrue(File.Exists(transformedPath));
        Assert.IsFalse((await File.ReadAllBytesAsync(sourcePath)).SequenceEqual(await File.ReadAllBytesAsync(transformedPath)));

        using var source = ModuleDefinition.ReadModule(sourcePath);
        using var transformed = ModuleDefinition.ReadModule(transformedPath);
        var sourceAdjustment = source.Types.Single(type => type.FullName == AheadOfLoadManagedTransformation.FixtureTypeFullName).Methods.Single(method => method.Name == "Adjustment");
        var transformedAdjustment = transformed.Types.Single(type => type.FullName == AheadOfLoadManagedTransformation.FixtureTypeFullName).Methods.Single(method => method.Name == "Adjustment");
        Assert.AreEqual(Code.Ldc_I4_1, sourceAdjustment.Body.Instructions[0].OpCode.Code);
        Assert.AreEqual(Code.Ldc_I4, transformedAdjustment.Body.Instructions[0].OpCode.Code);
        Assert.AreEqual(AheadOfLoadManagedTransformation.TransformedAdjustment, transformedAdjustment.Body.Instructions[0].Operand);
    }

    private static string RequireFixtureRoot()
    {
        var root = Environment.GetEnvironmentVariable("STS2_STEP28_AHEAD_OF_LOAD_FIXTURE_ROOT");
        if (string.IsNullOrWhiteSpace(root) || !Directory.Exists(root))
            throw new AssertInconclusiveException("STS2_STEP28_AHEAD_OF_LOAD_FIXTURE_ROOT was not supplied by scripts/test.sh.");
        return root;
    }

    private static async Task<string> CreateOfflineReadyInstallAsync(string launcherRoot, ulong manifestId)
    {
        var managedPath = Path.Combine(launcherRoot, SteamOfflineInstallInspection.ManagedRootRelativePath, "Depot-2868842");
        Directory.CreateDirectory(managedPath);
        var payloadPath = Path.Combine(managedPath, "payload.bin");
        var payload = new byte[] { 2, 8, 0, 0, 1 };
        await File.WriteAllBytesAsync(payloadPath, payload);

        var receipt = new SteamManagedInstallReceipt(
            SteamManagedInstallReceipt.CurrentSchemaVersion,
            2868840,
            2868842,
            manifestId,
            "public",
            DateTimeOffset.UtcNow,
            [new SteamManagedInstallFile("payload.bin", payload.LongLength, Convert.ToHexString(SHA1.HashData(payload)).ToLowerInvariant())]);
        await using var stream = File.Create(Path.Combine(managedPath, SteamManagedInstallReceipt.FileName));
        await JsonSerializer.SerializeAsync(stream, receipt, SteamManagedInstallJsonContext.Default.SteamManagedInstallReceipt);
        return managedPath;
    }
}
