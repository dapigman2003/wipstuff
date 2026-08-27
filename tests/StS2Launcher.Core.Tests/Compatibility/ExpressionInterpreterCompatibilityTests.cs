using System.Security.Cryptography;
using System.Reflection.PortableExecutable;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mono.Cecil;
using Mono.Cecil.Cil;
using StS2Launcher.Core;

namespace StS2Launcher.Core.Tests;

[TestClass]
public sealed class ExpressionInterpreterCompatibilityTests
{
    [TestMethod]
    public void OrderedExpressionInterpreterGatesReachFourOfFourPass()
    {
        var gates = new ExpressionInterpreterCompatibilityGateSequence();
        gates.Record(ExpressionInterpreterCompatibilityGate.InterpreterCapabilityAndWorkspaceClone, true, "probe/clone");
        gates.Record(ExpressionInterpreterCompatibilityGate.RealCompileTargetDiscovery, true, "targets");
        gates.Record(ExpressionInterpreterCompatibilityGate.HostFallbackPreparedCopy, true, "prepared-copy");
        gates.Record(ExpressionInterpreterCompatibilityGate.IsolationAudit, true, "audit");

        var summary = gates.Snapshot();
        Assert.IsTrue(summary.Passed);
        Assert.AreEqual(4, summary.PassedGates);
        Assert.AreEqual("EXPRESSION INTERPRETER COMPATIBILITY PASS — 4/4", summary.Summary);
    }

    [TestMethod]
    public void ExpressionInterpreterGatesStopAfterFirstFailure()
    {
        var gates = new ExpressionInterpreterCompatibilityGateSequence();
        gates.Record(ExpressionInterpreterCompatibilityGate.InterpreterCapabilityAndWorkspaceClone, true, "probe/clone");
        gates.Record(ExpressionInterpreterCompatibilityGate.RealCompileTargetDiscovery, false, "no target");
        Assert.ThrowsExactly<InvalidOperationException>(() =>
            gates.Record(ExpressionInterpreterCompatibilityGate.HostFallbackPreparedCopy, true, "must not advance"));
        Assert.AreEqual(ExpressionInterpreterCompatibilityGate.RealCompileTargetDiscovery, gates.Snapshot().FirstFailingGate);
    }


    [TestMethod]
    [DataRow(false, false, true, ExpressionRuntimeCompatibilityPolicy.HistoricalNoDynamicCodeMode)]
    [DataRow(true, false, true, ExpressionRuntimeCompatibilityPolicy.InterpreterEnabledMode)]
    [DataRow(false, true, false, ExpressionRuntimeCompatibilityPolicy.UnexpectedDynamicCompilationMode)]
    [DataRow(true, true, false, ExpressionRuntimeCompatibilityPolicy.UnexpectedDynamicCompilationMode)]
    public void IosExpressionRuntimePolicyTracksCanonicalNonJitContract(
        bool dynamicCodeSupported,
        bool dynamicCodeCompiled,
        bool expectedCompatible,
        string expectedMode)
    {
        var assessment = ExpressionRuntimeCompatibilityPolicy.Evaluate(
            isIos: true,
            dynamicCodeSupported: dynamicCodeSupported,
            dynamicCodeCompiled: dynamicCodeCompiled);

        Assert.AreEqual(expectedCompatible, assessment.Compatible);
        Assert.AreEqual(expectedMode, assessment.Mode);
        if (expectedCompatible)
            StringAssert.Contains(assessment.Detail, dynamicCodeSupported ? "Post-Step-20" : "Historical Step 19");
        else
            StringAssert.Contains(assessment.Detail, "IsDynamicCodeCompiled == false");
    }

    [TestMethod]
    public void NonIosExpressionRuntimePolicyDoesNotImposeIosJitAssertion()
    {
        var assessment = ExpressionRuntimeCompatibilityPolicy.Evaluate(
            isIos: false,
            dynamicCodeSupported: true,
            dynamicCodeCompiled: true);

        Assert.IsTrue(assessment.Compatible);
        Assert.AreEqual(ExpressionRuntimeCompatibilityPolicy.NonIosHostMode, assessment.Mode);
    }

    [TestMethod]
    public async Task RealWorkspaceExpressionCompileCallsUseHostRuntimeAndPreparedTreeStaysByteIdentical()
    {
        using var temp = new TempTestDirectory("sts2-step19");
        var managedPath = Path.Combine(temp.Path, SteamOfflineInstallInspection.ManagedRootRelativePath, "Depot-2868842");
        var arm64Relative = "SlayTheSpire2.app/Contents/Resources/data_sts2_macos_arm64/sts2.dll";
        var x86Relative = "SlayTheSpire2.app/Contents/Resources/data_sts2_macos_x86_64/sts2.dll";
        var sharedRelative = "SlayTheSpire2.app/Contents/Resources/shared-helper.dll";

        var arm64Path = Path.Combine(managedPath, arm64Relative.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(arm64Path)!);
        WriteSyntheticExpressionAssembly(arm64Path, "sts2", includeTargets: true);

        var x86Path = Path.Combine(managedPath, x86Relative.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(x86Path)!);
        File.Copy(arm64Path, x86Path, overwrite: true);

        var sharedPath = Path.Combine(managedPath, sharedRelative.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(sharedPath)!);
        WriteSyntheticExpressionAssembly(sharedPath, "shared-helper", includeTargets: false);

        var files = new List<SteamManagedInstallFile>();
        foreach (var relative in new[] { arm64Relative, x86Relative, sharedRelative })
        {
            var path = Path.Combine(managedPath, relative.Replace('/', Path.DirectorySeparatorChar));
            var bytes = await File.ReadAllBytesAsync(path);
            files.Add(new SteamManagedInstallFile(
                relative,
                bytes.LongLength,
                Convert.ToHexString(SHA1.HashData(bytes)).ToLowerInvariant()));
        }

        var receipt = new SteamManagedInstallReceipt(
            SteamManagedInstallReceipt.CurrentSchemaVersion,
            2868840,
            2868842,
            991UL,
            "public",
            DateTimeOffset.UtcNow,
            files);
        await using (var stream = File.Create(Path.Combine(managedPath, SteamManagedInstallReceipt.FileName)))
        {
            await JsonSerializer.SerializeAsync(stream, receipt, SteamManagedInstallJsonContext.Default.SteamManagedInstallReceipt);
        }

        var installBefore = SHA256.HashData(await File.ReadAllBytesAsync(arm64Path));
        var compatibility = new ExpressionInterpreterCompatibility(temp.Path);
        var gateA = await compatibility.RunInterpreterCapabilityAndWorkspaceCloneAsync();
        var gateB = compatibility.RunRealCompileTargetDiscovery();
        var gateC = compatibility.RunHostFallbackPreparedCopy();
        var gateD = await compatibility.RunIsolationAuditAsync();
        var installAfter = SHA256.HashData(await File.ReadAllBytesAsync(arm64Path));

        Assert.IsTrue(gateA.Passed, gateA.Detail);
        Assert.IsTrue(gateB.Passed, gateB.Detail);
        Assert.IsTrue(gateC.Passed, gateC.Detail);
        Assert.IsTrue(gateD.Passed, gateD.Detail);
        CollectionAssert.AreEqual(installBefore, installAfter);

        StringAssert.Contains(gateA.Detail, "Compile() execution probe result: 42");
        StringAssert.Contains(gateA.Detail, "Compile(preferInterpretation: false) probe result: 42");
        StringAssert.Contains(gateA.Detail, "Compile(preferInterpretation: true) probe result: 42");
        StringAssert.Contains(gateA.Detail, "Expression runtime compatibility policy: PASS");
        StringAssert.Contains(gateA.Detail, "macOS x86_64 duplicates excluded: 1");
        StringAssert.Contains(gateB.Detail, "Direct Compile() sites structurally safe for the old insertion design: 2");
        StringAssert.Contains(gateB.Detail, "Direct Compile(false) literal sites: 3");
        StringAssert.Contains(gateB.Detail, "Direct Compile(true) literal sites: 1");
        StringAssert.Contains(gateB.Detail, "Direct Compile(bool) dynamic/non-literal sites: 1");
        StringAssert.Contains(gateB.Detail, "Parameterless sites with branch/EH/prefix insertion hazards (diagnostic only): 2");
        StringAssert.Contains(gateB.Detail, "Direct Compile sites inside non-System.* consumer assemblies: 9");
        StringAssert.Contains(gateB.Detail, "Assemblies selected for Cecil mutation: 0");
        StringAssert.Contains(gateB.Detail, "HOST RUNTIME EXPRESSION SUPPORT — NO GAME/APPLICATION IL REWRITE REQUIRED");
        StringAssert.Contains(gateC.Detail, "Cecil assembly writes performed by Gate C: 0");
        StringAssert.Contains(gateD.Detail, "Managed Compile call sites rewritten: 0");
        StringAssert.Contains(gateD.Detail, "Prepared files unchanged byte-for-byte: 2/2");
        StringAssert.Contains(gateD.Detail, "Original Step 12 install unchanged: YES");

        var sourceCopy = Path.Combine(
            temp.Path,
            ExpressionInterpreterCompatibility.WorkRootName,
            ExpressionInterpreterCompatibility.SourceRootName,
            arm64Relative.Replace('/', Path.DirectorySeparatorChar));
        var preparedCopy = Path.Combine(
            temp.Path,
            ExpressionInterpreterCompatibility.WorkRootName,
            ExpressionInterpreterCompatibility.PreparedRootName,
            arm64Relative.Replace('/', Path.DirectorySeparatorChar));
        CollectionAssert.AreEqual(await File.ReadAllBytesAsync(arm64Path), await File.ReadAllBytesAsync(sourceCopy));
        CollectionAssert.AreEqual(await File.ReadAllBytesAsync(sourceCopy), await File.ReadAllBytesAsync(preparedCopy), "Step 19.2 must not rewrite consumer IL when the canonical host expression runtime already executes the call shapes successfully.");

        using var prepared = ModuleDefinition.ReadModule(preparedCopy, new ReaderParameters
        {
            ReadingMode = ReadingMode.Deferred,
            AssemblyResolver = RejectingTestResolver.Instance,
            MetadataResolver = RejectingTestResolver.Instance,
        });
        var calls = prepared.Types
            .SelectMany(type => type.Methods)
            .Where(method => method.HasBody)
            .SelectMany(method => method.Body.Instructions)
            .Where(instruction => instruction.OpCode.Code is Code.Call or Code.Callvirt)
            .Where(instruction => instruction.Operand is MethodReference method && method.Name == "Compile")
            .ToArray();
        Assert.AreEqual(9, calls.Length);
        Assert.AreEqual(4, calls.Count(instruction => ((MethodReference)instruction.Operand).Parameters.Count == 0));
        Assert.AreEqual(1, calls.Count(IsImmediatelyPrecededByTrueLiteral));

        var expressionUser = prepared.GetType("Synthetic.ExpressionUser")!;
        AssertConstantEncoding(expressionUser, "LiteralFalse", Code.Ldc_I4_0, null);
        AssertConstantEncoding(expressionUser, "LiteralFalseShort", Code.Ldc_I4_S, (sbyte)0);
        AssertConstantEncoding(expressionUser, "LiteralFalseLong", Code.Ldc_I4, 0);
        var unsafeMethod = expressionUser.Methods.Single(method => method.Name == "UnsafeBranchTargetParameterless");
        Assert.AreEqual(0, ((MethodReference)unsafeMethod.Body.Instructions.Single(instruction => instruction.OpCode.Code is Code.Call or Code.Callvirt).Operand).Parameters.Count);
        var crossingShortBranchMethod = expressionUser.Methods.Single(method => method.Name == "CrossingShortBranchParameterless");
        Assert.AreEqual(0, ((MethodReference)crossingShortBranchMethod.Body.Instructions.Single(instruction => instruction.OpCode.Code is Code.Call or Code.Callvirt).Operand).Parameters.Count);
    }

    [TestMethod]
    public async Task StrongNameConsumerTargetRemainsByteIdenticalWithIdentityAndSignatureStateUntouched()
    {
        using var temp = new TempTestDirectory("sts2-step19");
        var managedPath = Path.Combine(temp.Path, SteamOfflineInstallInspection.ManagedRootRelativePath, "Depot-2868842");
        var arm64Relative = "SlayTheSpire2.app/Contents/Resources/data_sts2_macos_arm64/sts2.dll";
        var consumerRelative = "SlayTheSpire2.app/Contents/Resources/strong-name-consumer.dll";
        var arm64Path = Path.Combine(managedPath, arm64Relative.Replace('/', Path.DirectorySeparatorChar));
        var consumerPath = Path.Combine(managedPath, consumerRelative.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(arm64Path)!);
        Directory.CreateDirectory(Path.GetDirectoryName(consumerPath)!);
        WriteSyntheticExpressionAssembly(arm64Path, "sts2", includeTargets: true, strongNameIdentity: true);
        WriteSyntheticConsumerAssembly(consumerPath, arm64Path);

        var files = new List<SteamManagedInstallFile>();
        foreach (var (relative, path) in new[] { (arm64Relative, arm64Path), (consumerRelative, consumerPath) })
        {
            var fileBytes = await File.ReadAllBytesAsync(path);
            files.Add(new SteamManagedInstallFile(relative, fileBytes.LongLength, Convert.ToHexString(SHA1.HashData(fileBytes)).ToLowerInvariant()));
        }
        var receipt = new SteamManagedInstallReceipt(
            SteamManagedInstallReceipt.CurrentSchemaVersion,
            2868840,
            2868842,
            993UL,
            "public",
            DateTimeOffset.UtcNow,
            files);
        await using (var stream = File.Create(Path.Combine(managedPath, SteamManagedInstallReceipt.FileName)))
        {
            await JsonSerializer.SerializeAsync(stream, receipt, SteamManagedInstallJsonContext.Default.SteamManagedInstallReceipt);
        }

        using var sourceBefore = ModuleDefinition.ReadModule(arm64Path, new ReaderParameters
        {
            ReadingMode = ReadingMode.Deferred,
            AssemblyResolver = RejectingTestResolver.Instance,
            MetadataResolver = RejectingTestResolver.Instance,
        });
        var sourceFullName = sourceBefore.Assembly.Name.FullName;
        var sourcePublicKey = sourceBefore.Assembly.Name.PublicKey.ToArray();
        var sourceToken = sourceBefore.Assembly.Name.PublicKeyToken.ToArray();
        Assert.IsTrue(sourcePublicKey.Length > 0, "Synthetic source must carry a strong-name public key.");
        Assert.IsTrue((sourceBefore.Attributes & ModuleAttributes.StrongNameSigned) != 0, "Synthetic source must present as StrongNameSigned.");

        var compatibility = new ExpressionInterpreterCompatibility(temp.Path);
        var gateA = await compatibility.RunInterpreterCapabilityAndWorkspaceCloneAsync();
        var gateB = compatibility.RunRealCompileTargetDiscovery();
        var gateC = compatibility.RunHostFallbackPreparedCopy();
        var gateD = await compatibility.RunIsolationAuditAsync();

        Assert.IsTrue(gateA.Passed, gateA.Detail);
        Assert.IsTrue(gateB.Passed, gateB.Detail);
        Assert.IsTrue(gateC.Passed, gateC.Detail);
        Assert.IsTrue(gateD.Passed, gateD.Detail);
        StringAssert.Contains(gateB.Detail, "Direct Compile sites carrying strong-name identity: 9");
        StringAssert.Contains(gateB.Detail, "Direct Compile sites inside non-System.* consumer assemblies: 9");
        StringAssert.Contains(gateB.Detail, "Assemblies selected for Cecil mutation: 0");
        StringAssert.Contains(gateC.Detail, "Strong-name flags/public keys/tokens modified: NO");
        StringAssert.Contains(gateD.Detail, "Strong-name flags/public keys/tokens modified: NO");

        var sourceCopy = Path.Combine(
            temp.Path,
            ExpressionInterpreterCompatibility.WorkRootName,
            ExpressionInterpreterCompatibility.SourceRootName,
            arm64Relative.Replace('/', Path.DirectorySeparatorChar));
        var preparedCopy = Path.Combine(
            temp.Path,
            ExpressionInterpreterCompatibility.WorkRootName,
            ExpressionInterpreterCompatibility.PreparedRootName,
            arm64Relative.Replace('/', Path.DirectorySeparatorChar));

        using var sourceAfter = ModuleDefinition.ReadModule(sourceCopy, new ReaderParameters
        {
            ReadingMode = ReadingMode.Deferred,
            AssemblyResolver = RejectingTestResolver.Instance,
            MetadataResolver = RejectingTestResolver.Instance,
        });
        using var prepared = ModuleDefinition.ReadModule(preparedCopy, new ReaderParameters
        {
            ReadingMode = ReadingMode.Deferred,
            AssemblyResolver = RejectingTestResolver.Instance,
            MetadataResolver = RejectingTestResolver.Instance,
        });

        Assert.AreEqual(sourceFullName, sourceAfter.Assembly.Name.FullName, "Source assembly identity changed.");
        Assert.AreEqual(sourceFullName, prepared.Assembly.Name.FullName, "Prepared assembly full identity must remain unchanged.");
        CollectionAssert.AreEqual(sourcePublicKey, sourceAfter.Assembly.Name.PublicKey.ToArray(), "Source public key changed.");
        CollectionAssert.AreEqual(sourcePublicKey, prepared.Assembly.Name.PublicKey.ToArray(), "Prepared public key changed.");
        CollectionAssert.AreEqual(sourceToken, sourceAfter.Assembly.Name.PublicKeyToken.ToArray(), "Source public-key token changed.");
        CollectionAssert.AreEqual(sourceToken, prepared.Assembly.Name.PublicKeyToken.ToArray(), "Prepared public-key token changed.");
        Assert.IsTrue((sourceAfter.Attributes & ModuleAttributes.StrongNameSigned) != 0, "Receipt-backed source must retain StrongNameSigned.");
        Assert.IsTrue((prepared.Attributes & ModuleAttributes.StrongNameSigned) != 0, "Byte-identical prepared copy must retain the original StrongNameSigned state.");
        CollectionAssert.AreEqual(await File.ReadAllBytesAsync(arm64Path), await File.ReadAllBytesAsync(sourceCopy), "Receipt-backed source copy changed.");
        CollectionAssert.AreEqual(await File.ReadAllBytesAsync(sourceCopy), await File.ReadAllBytesAsync(preparedCopy), "Strong-name target should remain byte-identical in Step 19.2.");

        var preparedConsumer = Path.Combine(
            temp.Path,
            ExpressionInterpreterCompatibility.WorkRootName,
            ExpressionInterpreterCompatibility.PreparedRootName,
            consumerRelative.Replace('/', Path.DirectorySeparatorChar));
        CollectionAssert.AreEqual(await File.ReadAllBytesAsync(consumerPath), await File.ReadAllBytesAsync(preparedConsumer), "Non-target consumer assembly changed.");
        using var consumer = ModuleDefinition.ReadModule(preparedConsumer, new ReaderParameters
        {
            ReadingMode = ReadingMode.Deferred,
            AssemblyResolver = RejectingTestResolver.Instance,
            MetadataResolver = RejectingTestResolver.Instance,
        });
        Assert.IsTrue(consumer.AssemblyReferences.Any(reference => reference.FullName == sourceFullName), "Consumer strong-name reference no longer matches the preserved prepared target identity.");
    }

    [TestMethod]
    public async Task NoConsumerExpressionCompileTargetPassesAsNoRewriteRequiredAndPreparedTreeStaysIdentical()
    {
        using var temp = new TempTestDirectory("sts2-step19");
        var managedPath = Path.Combine(temp.Path, SteamOfflineInstallInspection.ManagedRootRelativePath, "Depot-2868842");
        var arm64Relative = "SlayTheSpire2.app/Contents/Resources/data_sts2_macos_arm64/sts2.dll";
        var arm64Path = Path.Combine(managedPath, arm64Relative.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(arm64Path)!);
        WriteSyntheticExpressionAssembly(arm64Path, "sts2", includeTargets: false);

        await WriteReceiptAsync(managedPath, 992UL, [(arm64Relative, arm64Path)]);

        var compatibility = new ExpressionInterpreterCompatibility(temp.Path);
        var gateA = await compatibility.RunInterpreterCapabilityAndWorkspaceCloneAsync();
        var gateB = compatibility.RunRealCompileTargetDiscovery();
        var gateC = compatibility.RunHostFallbackPreparedCopy();
        var gateD = await compatibility.RunIsolationAuditAsync();

        Assert.IsTrue(gateA.Passed, gateA.Detail);
        Assert.IsTrue(gateB.Passed, gateB.Detail);
        Assert.IsTrue(gateC.Passed, gateC.Detail);
        Assert.IsTrue(gateD.Passed, gateD.Detail);
        StringAssert.Contains(gateB.Detail, "HOST RUNTIME EXPRESSION SUPPORT — NO GAME/APPLICATION IL REWRITE REQUIRED");
        StringAssert.Contains(gateC.Detail, "intentionally performs NO IL rewrite");
        StringAssert.Contains(gateD.Detail, "HOST RUNTIME EXPRESSION SUPPORT — NO GAME/APPLICATION IL REWRITE REQUIRED");

        var sourceCopy = Path.Combine(temp.Path, ExpressionInterpreterCompatibility.WorkRootName, ExpressionInterpreterCompatibility.SourceRootName, arm64Relative.Replace('/', Path.DirectorySeparatorChar));
        var preparedCopy = Path.Combine(temp.Path, ExpressionInterpreterCompatibility.WorkRootName, ExpressionInterpreterCompatibility.PreparedRootName, arm64Relative.Replace('/', Path.DirectorySeparatorChar));
        CollectionAssert.AreEqual(await File.ReadAllBytesAsync(arm64Path), await File.ReadAllBytesAsync(sourceCopy));
        CollectionAssert.AreEqual(await File.ReadAllBytesAsync(sourceCopy), await File.ReadAllBytesAsync(preparedCopy));
    }

    [TestMethod]
    public async Task FrameworkImplementationCompileSitesAreDiagnosticOnlyAndNeverRewritten()
    {
        using var temp = new TempTestDirectory("sts2-step19");
        var managedPath = Path.Combine(temp.Path, SteamOfflineInstallInspection.ManagedRootRelativePath, "Depot-2868842");
        var arm64Relative = "SlayTheSpire2.app/Contents/Resources/data_sts2_macos_arm64/sts2.dll";
        var frameworkRelative = "SlayTheSpire2.app/Contents/Resources/data_sts2_macos_arm64/System.Linq.Expressions.dll";
        var arm64Path = Path.Combine(managedPath, arm64Relative.Replace('/', Path.DirectorySeparatorChar));
        var frameworkPath = Path.Combine(managedPath, frameworkRelative.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(arm64Path)!);
        WriteSyntheticExpressionAssembly(arm64Path, "sts2", includeTargets: false);
        WriteSyntheticExpressionAssembly(frameworkPath, "System.Linq.Expressions", includeTargets: true, strongNameIdentity: true);

        await WriteReceiptAsync(managedPath, 994UL, [(arm64Relative, arm64Path), (frameworkRelative, frameworkPath)]);
        var frameworkBefore = await File.ReadAllBytesAsync(frameworkPath);

        var compatibility = new ExpressionInterpreterCompatibility(temp.Path);
        var gateA = await compatibility.RunInterpreterCapabilityAndWorkspaceCloneAsync();
        var gateB = compatibility.RunRealCompileTargetDiscovery();
        var gateC = compatibility.RunHostFallbackPreparedCopy();
        var gateD = await compatibility.RunIsolationAuditAsync();

        Assert.IsTrue(gateA.Passed, gateA.Detail);
        Assert.IsTrue(gateB.Passed, gateB.Detail);
        Assert.IsTrue(gateC.Passed, gateC.Detail);
        Assert.IsTrue(gateD.Passed, gateD.Detail);
        StringAssert.Contains(gateB.Detail, "Direct Compile sites inside System.* framework implementation assemblies: 9");
        StringAssert.Contains(gateB.Detail, "Assemblies selected for Cecil mutation: 0");
        StringAssert.Contains(gateB.Detail, "HOST RUNTIME EXPRESSION SUPPORT — NO GAME/APPLICATION IL REWRITE REQUIRED");
        StringAssert.Contains(gateC.Detail, "System.* framework implementation assemblies written by Cecil: NO");

        var preparedFramework = Path.Combine(temp.Path, ExpressionInterpreterCompatibility.WorkRootName, ExpressionInterpreterCompatibility.PreparedRootName, frameworkRelative.Replace('/', Path.DirectorySeparatorChar));
        CollectionAssert.AreEqual(frameworkBefore, await File.ReadAllBytesAsync(preparedFramework), "Framework implementation assembly must remain byte-identical in the prepared tree.");
    }

    [TestMethod]
    public async Task NonFrameworkNonIlOnlyConsumerIsClassifiedReadOnlyAndNeverWritten()
    {
        using var temp = new TempTestDirectory("sts2-step19");
        var managedPath = Path.Combine(temp.Path, SteamOfflineInstallInspection.ManagedRootRelativePath, "Depot-2868842");
        var arm64Relative = "SlayTheSpire2.app/Contents/Resources/data_sts2_macos_arm64/sts2.dll";
        var arm64Path = Path.Combine(managedPath, arm64Relative.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(arm64Path)!);
        WriteSyntheticExpressionAssembly(arm64Path, "sts2", includeTargets: true);
        ClearIlOnlyCorFlag(arm64Path);

        await WriteReceiptAsync(managedPath, 995UL, [(arm64Relative, arm64Path)]);

        var compatibility = new ExpressionInterpreterCompatibility(temp.Path);
        var gateA = await compatibility.RunInterpreterCapabilityAndWorkspaceCloneAsync();
        var gateB = compatibility.RunRealCompileTargetDiscovery();
        var gateC = compatibility.RunHostFallbackPreparedCopy();
        var gateD = await compatibility.RunIsolationAuditAsync();

        Assert.IsTrue(gateA.Passed, gateA.Detail);
        Assert.IsTrue(gateB.Passed, gateB.Detail);
        Assert.IsTrue(gateC.Passed, gateC.Detail);
        Assert.IsTrue(gateD.Passed, gateD.Detail);
        StringAssert.Contains(gateB.Detail, "Direct Compile sites inside non-IL-only/ReadyToRun-or-mixed-mode images: 9");
        StringAssert.Contains(gateB.Detail, "Assemblies selected for Cecil mutation: 0");
        StringAssert.Contains(gateC.Detail, "Non-IL-only/ReadyToRun-or-mixed-mode assemblies written by Cecil: NO");

        var sourceCopy = Path.Combine(temp.Path, ExpressionInterpreterCompatibility.WorkRootName, ExpressionInterpreterCompatibility.SourceRootName, arm64Relative.Replace('/', Path.DirectorySeparatorChar));
        var preparedCopy = Path.Combine(temp.Path, ExpressionInterpreterCompatibility.WorkRootName, ExpressionInterpreterCompatibility.PreparedRootName, arm64Relative.Replace('/', Path.DirectorySeparatorChar));
        CollectionAssert.AreEqual(await File.ReadAllBytesAsync(sourceCopy), await File.ReadAllBytesAsync(preparedCopy), "Non-IL-only consumer must remain byte-identical.");
    }

    private static async Task WriteReceiptAsync(string managedPath, ulong manifestId, IReadOnlyList<(string Relative, string Path)> files)
    {
        var receiptFiles = new List<SteamManagedInstallFile>();
        foreach (var (relative, path) in files)
        {
            var bytes = await File.ReadAllBytesAsync(path);
            receiptFiles.Add(new SteamManagedInstallFile(relative, bytes.LongLength, Convert.ToHexString(SHA1.HashData(bytes)).ToLowerInvariant()));
        }

        var receipt = new SteamManagedInstallReceipt(
            SteamManagedInstallReceipt.CurrentSchemaVersion,
            2868840,
            2868842,
            manifestId,
            "public",
            DateTimeOffset.UtcNow,
            receiptFiles);
        await using var stream = File.Create(Path.Combine(managedPath, SteamManagedInstallReceipt.FileName));
        await JsonSerializer.SerializeAsync(stream, receipt, SteamManagedInstallJsonContext.Default.SteamManagedInstallReceipt);
    }

    private static void ClearIlOnlyCorFlag(string path)
    {
        int corHeaderOffset;
        using (var stream = File.OpenRead(path))
        using (var pe = new PEReader(stream))
        {
            corHeaderOffset = pe.PEHeaders.CorHeaderStartOffset;
        }
        if (corHeaderOffset < 0)
            throw new InvalidDataException("Synthetic managed PE has no CLR header.");

        var bytes = File.ReadAllBytes(path);
        var flagsOffset = checked(corHeaderOffset + 16); // IMAGE_COR20_HEADER.Flags
        var flags = BitConverter.ToUInt32(bytes, flagsOffset);
        flags &= ~0x00000001u; // COMIMAGE_FLAGS_ILONLY
        BitConverter.GetBytes(flags).CopyTo(bytes, flagsOffset);
        File.WriteAllBytes(path, bytes);
    }

    private static bool IsImmediatelyPrecededByTrueLiteral(Instruction instruction)
        => instruction.Previous is { } previous && TryGetConstant(previous, out var value) && value == 1;

    private static bool TryGetConstant(Instruction instruction, out int value)
    {
        switch (instruction.OpCode.Code)
        {
            case Code.Ldc_I4_0: value = 0; return true;
            case Code.Ldc_I4_1: value = 1; return true;
            case Code.Ldc_I4_S: value = Convert.ToInt32(instruction.Operand); return true;
            case Code.Ldc_I4: value = Convert.ToInt32(instruction.Operand); return true;
            default: value = 0; return false;
        }
    }

    private static void AssertConstantEncoding(TypeDefinition type, string methodName, Code expectedCode, object? expectedOperand)
    {
        var method = type.Methods.Single(value => value.Name == methodName);
        var call = method.Body.Instructions.Single(instruction => instruction.OpCode.Code is Code.Call or Code.Callvirt);
        var constant = call.Previous ?? throw new AssertFailedException($"{methodName} Compile call has no argument instruction.");
        Assert.AreEqual(expectedCode, constant.OpCode.Code, $"Unexpected constant opcode in {methodName}.");
        if (expectedOperand is null)
            Assert.IsNull(constant.Operand, $"Unexpected constant operand in {methodName}.");
        else
            Assert.AreEqual(expectedOperand, constant.Operand, $"Unexpected constant operand in {methodName}.");
    }

    private static void WriteSyntheticExpressionAssembly(string path, string assemblyName, bool includeTargets, bool strongNameIdentity = false)
    {
        using var assembly = AssemblyDefinition.CreateAssembly(
            new AssemblyNameDefinition(assemblyName, new Version(1, 0, 0, 0)),
            assemblyName,
            ModuleKind.Dll);
        var module = assembly.MainModule;
        if (strongNameIdentity)
        {
            var publicKey = typeof(object).Assembly.GetName().GetPublicKey();
            if (publicKey is not { Length: > 0 })
                throw new InvalidOperationException("The host core library did not expose a strong-name public key for the Step 19 test fixture.");
            assembly.Name.PublicKey = publicKey;
            module.Attributes |= ModuleAttributes.StrongNameSigned;
        }
        var type = new TypeDefinition("Synthetic", "ExpressionUser", TypeAttributes.Public | TypeAttributes.Class, module.TypeSystem.Object);
        module.Types.Add(type);

        if (!includeTargets)
        {
            var noop = new MethodDefinition("Noop", MethodAttributes.Public | MethodAttributes.Static, module.TypeSystem.Void);
            type.Methods.Add(noop);
            noop.Body.GetILProcessor().Append(Instruction.Create(OpCodes.Ret));
            assembly.Write(path);
            return;
        }

        var expressionsAssembly = new AssemblyNameReference("System.Linq.Expressions", new Version(9, 0, 0, 0));
        module.AssemblyReferences.Add(expressionsAssembly);
        var lambdaType = new TypeReference("System.Linq.Expressions", "LambdaExpression", module, expressionsAssembly);
        var compileNoArgs = new MethodReference("Compile", module.TypeSystem.Object, lambdaType) { HasThis = true };
        var compileBool = new MethodReference("Compile", module.TypeSystem.Object, lambdaType) { HasThis = true };
        compileBool.Parameters.Add(new ParameterDefinition(module.TypeSystem.Boolean));

        var genericExpressionType = new TypeReference("System.Linq.Expressions", "Expression`1", module, expressionsAssembly);
        genericExpressionType.GenericParameters.Add(new GenericParameter("TDelegate", genericExpressionType));
        var expressionOfObject = new GenericInstanceType(genericExpressionType);
        expressionOfObject.GenericArguments.Add(module.TypeSystem.Object);
        var genericCompileNoArgs = new MethodReference("Compile", module.TypeSystem.Object, expressionOfObject) { HasThis = true };

        AddCompileMethod(type, "Parameterless", lambdaType, compileNoArgs, argumentKind: 0);
        AddCompileMethod(type, "GenericParameterless", expressionOfObject, genericCompileNoArgs, argumentKind: 0);
        AddCompileMethod(type, "LiteralFalse", lambdaType, compileBool, argumentKind: 1);
        AddCompileMethod(type, "LiteralFalseShort", lambdaType, compileBool, argumentKind: 4);
        AddCompileMethod(type, "LiteralFalseLong", lambdaType, compileBool, argumentKind: 5);
        AddCompileMethod(type, "LiteralTrue", lambdaType, compileBool, argumentKind: 2);
        AddCompileMethod(type, "DynamicBool", lambdaType, compileBool, argumentKind: 3);
        AddUnsafeBranchTargetCompileMethod(type, lambdaType, compileNoArgs);
        AddCrossingShortBranchCompileMethod(type, lambdaType, compileNoArgs);
        assembly.Write(path);
    }

    private static void WriteSyntheticConsumerAssembly(string path, string targetPath)
    {
        using var target = ModuleDefinition.ReadModule(targetPath, new ReaderParameters
        {
            ReadingMode = ReadingMode.Deferred,
            AssemblyResolver = RejectingTestResolver.Instance,
            MetadataResolver = RejectingTestResolver.Instance,
        });
        var targetName = target.Assembly.Name;
        var targetReference = new AssemblyNameReference(targetName.Name, targetName.Version)
        {
            Culture = targetName.Culture,
            PublicKeyToken = targetName.PublicKeyToken.ToArray(),
        };

        using var assembly = AssemblyDefinition.CreateAssembly(
            new AssemblyNameDefinition("strong-name-consumer", new Version(1, 0, 0, 0)),
            "strong-name-consumer",
            ModuleKind.Dll);
        var module = assembly.MainModule;
        module.AssemblyReferences.Add(targetReference);
        var consumerType = new TypeDefinition("Synthetic", "StrongNameConsumer", TypeAttributes.Public | TypeAttributes.Class, module.TypeSystem.Object);
        module.Types.Add(consumerType);
        var targetType = new TypeReference("Synthetic", "ExpressionUser", module, targetReference);
        consumerType.Fields.Add(new FieldDefinition("Target", FieldAttributes.Public | FieldAttributes.Static, targetType));
        assembly.Write(path);
    }

    private static void AddCompileMethod(
        TypeDefinition type,
        string name,
        TypeReference lambdaType,
        MethodReference compile,
        int argumentKind)
    {
        var module = type.Module;
        var method = new MethodDefinition(name, MethodAttributes.Public | MethodAttributes.Static, module.TypeSystem.Void);
        method.Parameters.Add(new ParameterDefinition("lambda", ParameterAttributes.None, lambdaType));
        if (argumentKind == 3)
            method.Parameters.Add(new ParameterDefinition("prefer", ParameterAttributes.None, module.TypeSystem.Boolean));
        type.Methods.Add(method);

        var il = method.Body.GetILProcessor();
        il.Append(il.Create(OpCodes.Ldarg_0));
        if (argumentKind == 1)
            il.Append(il.Create(OpCodes.Ldc_I4_0));
        else if (argumentKind == 2)
            il.Append(il.Create(OpCodes.Ldc_I4_1));
        else if (argumentKind == 3)
            il.Append(il.Create(OpCodes.Ldarg_1));
        else if (argumentKind == 4)
            il.Append(il.Create(OpCodes.Ldc_I4_S, (sbyte)0));
        else if (argumentKind == 5)
            il.Append(il.Create(OpCodes.Ldc_I4, 0));
        il.Append(il.Create(OpCodes.Callvirt, compile));
        il.Append(il.Create(OpCodes.Pop));
        il.Append(il.Create(OpCodes.Ret));
    }


    private static void AddUnsafeBranchTargetCompileMethod(
        TypeDefinition type,
        TypeReference lambdaType,
        MethodReference compile)
    {
        var module = type.Module;
        var method = new MethodDefinition("UnsafeBranchTargetParameterless", MethodAttributes.Public | MethodAttributes.Static, module.TypeSystem.Void);
        method.Parameters.Add(new ParameterDefinition("lambda", ParameterAttributes.None, lambdaType));
        type.Methods.Add(method);

        var il = method.Body.GetILProcessor();
        var call = il.Create(OpCodes.Callvirt, compile);
        il.Append(il.Create(OpCodes.Ldarg_0));
        il.Append(il.Create(OpCodes.Br_S, call));
        il.Append(il.Create(OpCodes.Nop));
        il.Append(call);
        il.Append(il.Create(OpCodes.Pop));
        il.Append(il.Create(OpCodes.Ret));
    }


    private static void AddCrossingShortBranchCompileMethod(
        TypeDefinition type,
        TypeReference lambdaType,
        MethodReference compile)
    {
        var module = type.Module;
        var method = new MethodDefinition("CrossingShortBranchParameterless", MethodAttributes.Public | MethodAttributes.Static, module.TypeSystem.Void);
        method.Parameters.Add(new ParameterDefinition("lambda", ParameterAttributes.None, lambdaType));
        type.Methods.Add(method);

        var il = method.Body.GetILProcessor();
        var done = il.Create(OpCodes.Ret);
        il.Append(il.Create(OpCodes.Ldc_I4_0));
        il.Append(il.Create(OpCodes.Brtrue_S, done));
        il.Append(il.Create(OpCodes.Ldarg_0));
        il.Append(il.Create(OpCodes.Callvirt, compile));
        il.Append(il.Create(OpCodes.Pop));
        il.Append(done);
    }

    private sealed class RejectingTestResolver : IAssemblyResolver, IMetadataResolver
    {
        public static RejectingTestResolver Instance { get; } = new();
        private RejectingTestResolver() { }
        public AssemblyDefinition Resolve(AssemblyNameReference name)
            => throw new InvalidOperationException($"Unexpected test dependency resolution: {name.FullName}");
        public AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters) => Resolve(name);
        TypeDefinition IMetadataResolver.Resolve(TypeReference type)
            => throw new InvalidOperationException($"Unexpected test type resolution: {type.FullName}");
        FieldDefinition IMetadataResolver.Resolve(FieldReference field)
            => throw new InvalidOperationException($"Unexpected test field resolution: {field.FullName}");
        MethodDefinition IMetadataResolver.Resolve(MethodReference method)
            => throw new InvalidOperationException($"Unexpected test method resolution: {method.FullName}");
        public void Dispose() { }
    }
}
